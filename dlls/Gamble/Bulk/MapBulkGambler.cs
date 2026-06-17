using PoE.dlls.Automation;
using PoE.dlls.Gamble;
using PoE.dlls.Gamble.Modes;
using PoE.dlls.InteropServices;
using PoE.dlls.Logger;
using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.Gamble.UI;
using PoE.dlls.Settings.Mods;

namespace PoE.dlls.Gamble.Bulk
{
    internal sealed class MapBulkGambler : IGamba
    {
        private const int TargetAffixCount = 6;
        private const int MaxConsecutiveExaltNoModSlams = 3;

        private readonly Main _main;
        private readonly InputSimulatorHost _inputHost;
        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _token;
        private readonly TimeSpan _delay;
        private readonly double _speed;
        private readonly GambleType _mapMode;
        private readonly List<BulkMapSlot> _slots;
        private readonly List<Rule> _rules;
        private readonly Coordinates _alchemy;
        private readonly Coordinates _exalt;
        private readonly Coordinates _chaos;
        private readonly Coordinates _vaal;
        private readonly bool _corruptOnSuccess;
        private readonly GambleMapBulkSettings? _bulkGrid;
        private readonly TimeSpan _refreshDelay;

        private bool _shiftHeld;
        private bool _alchemyPrimed;
        private bool _exaltPrimed;
        private bool _chaosPrimed;
        private bool _vaalPrimed;
        private Coordinates? _heldOrbStack;
        private int _consecutiveExaltNoModSlams;

        public MapBulkGambler(
            Main main,
            InputSimulatorHost inputHost,
            CancellationTokenSource cts,
            TimeSpan delay,
            double speed,
            GambleType mapMode,
            IReadOnlyList<Coordinates> cells,
            Coordinates alchemy,
            Coordinates exalt,
            Coordinates chaos,
            Coordinates vaal,
            bool corruptOnSuccess,
            GambleMapBulkSettings? bulkGrid,
            List<Rule> rules)
        {
            _main = main;
            _inputHost = inputHost;
            _cts = cts;
            _token = cts.Token;
            _delay = delay;
            _speed = speed;
            _mapMode = mapMode;
            _alchemy = alchemy;
            _exalt = exalt;
            _chaos = chaos;
            _vaal = vaal;
            _corruptOnSuccess = corruptOnSuccess;
            _bulkGrid = bulkGrid;
            _rules = rules;
            _slots = cells.Select(c => new BulkMapSlot { Position = c }).ToList();
            _refreshDelay = BulkMapActionHelper.ResolveRefreshDelay(bulkGrid, delay);

            GamblerLog.Info($"Bulk map session: {_slots.Count} slots, mode {_mapMode}");
        }

        public async Task Gamble()
        {
            LogBulkSetup();

            try
            {
                await PrecheckAllAsync();

                while (!_token.IsCancellationRequested && HasRollingWork())
                {
                    await ExecuteScourAlchemyBatchAsync();
                    await ExecuteExaltBatchLoopAsync();
                    await ExecuteChaosBatchLoopAsync();
                }

                if (!_token.IsCancellationRequested)
                {
                    await ExecuteVaalBatchAsync();
                    await ExecuteStashBrokenBatchAsync();
                }

                if (_token.IsCancellationRequested)
                    GamblerLog.Cancelled();
                else
                {
                    int passed = _slots.Count(s => s.IsFinished && !s.IsEmpty);
                    GamblerLog.Success($"Bulk map session finished ({passed} kept)");
                }
            }
            finally
            {
                _inputHost.Simulator.Send("Shift Up");
                GambleInputReleaseHelper.ReleaseModifiers(_inputHost);
                _shiftHeld = false;
                _heldOrbStack = null;
                ClearPrimedOrbs();
            }
        }

        private bool HasRollingWork() =>
            _slots.Any(s => s.IsActive && s.NextAction is BulkMapAction.ScourAlchemy or BulkMapAction.AlchemyOnly or BulkMapAction.Exalt or BulkMapAction.Chaos);

        private void LogBulkSetup()
        {
            GamblerLog.Info($"Bulk mode: {_mapMode} | {_slots.Count} slots");
            GamblerLog.Info(GambleBulkHelp.Short.GridArea);
            if (_bulkGrid is not null)
            {
                if (_bulkGrid.HasGridArea)
                {
                    int left = Math.Min(_bulkGrid.GridStart.X, _bulkGrid.GridEnd.X);
                    int top = Math.Min(_bulkGrid.GridStart.Y, _bulkGrid.GridEnd.Y);
                    int right = Math.Max(_bulkGrid.GridStart.X, _bulkGrid.GridEnd.X);
                    int bottom = Math.Max(_bulkGrid.GridStart.Y, _bulkGrid.GridEnd.Y);
                    GamblerLog.Info($"Grid area: ({left},{top}) → ({right},{bottom})");
                }

                if (_bulkGrid.CellAnchor.X > 0 || _bulkGrid.CellAnchor.Y > 0)
                    GamblerLog.Info($"First cell: {_bulkGrid.CellAnchor.X},{_bulkGrid.CellAnchor.Y}");

                if (_bulkGrid.HasCellStep)
                    GamblerLog.Info($"Step: Next X={_bulkGrid.NextX}, Next Y={_bulkGrid.NextY}");
            }

            if (_corruptOnSuccess)
                GamblerLog.Info(GambleBulkHelp.Short.CorruptOnSuccess);

            if (RequiresEightModsAfterCorrupt())
                GamblerLog.Info(GambleBulkHelp.Short.CorruptRequireEightMods);

            if (_bulkGrid is { FastEmptyColorCheck: true })
                GamblerLog.Info(GambleBulkHelp.Short.FastEmptyColorCheck);
        }

        private void ApplyFastEmptyColorCheckIfEnabled()
        {
            if (_bulkGrid is null || !_bulkGrid.FastEmptyColorCheck)
                return;

            if (!BulkEmptySlotHelper.IsRegistrationValid(_bulkGrid))
            {
                GamblerLog.Warn("Fast empty check is enabled but empty slots are not registered for the current grid.");
                return;
            }

            int skipped = BulkEmptySlotHelper.MarkMatchingSlotsEmpty(_slots, _bulkGrid.EmptySlotSignatures);
            if (skipped > 0)
                GamblerLog.Info($"Fast empty check: skipped {skipped} slot(s) before precheck.");
        }

        private bool ShouldCorrupt() => _corruptOnSuccess && MapCorruptHelper.IsVaalConfigured(_vaal);

        private bool RequiresEightModsAfterCorrupt() =>
            _corruptOnSuccess && _bulkGrid?.CorruptRequireEightMods == true;

        private async Task PrecheckAllAsync()
        {
            ApplyFastEmptyColorCheckIfEnabled();

            foreach (var slot in _slots.Where(s => s.IsActive))
            {
                slot.NextAction = BulkMapAction.None;
                await MoveToAsync(slot.Position);

                string? content = await MapClipboardHelper.CopyMapAsync(_main, _inputHost, _delay, _token);
                if (content is null)
                {
                    slot.IsEmpty = true;
                    GamblerLog.Debug($"Slot {slot.Position.X},{slot.Position.Y}: empty");
                    continue;
                }

                slot.Content = content;
                slot.Evaluation = MapRulesEvaluator.Evaluate(content, _rules, logMods: false);
                AssignNextAction(slot);
            }
        }

        private void AssignNextAction(BulkMapSlot slot)
        {
            var eval = slot.Evaluation;
            if (!eval.IsMap)
            {
                slot.IsEmpty = true;
                return;
            }

            if (eval.IsCorrupted)
            {
                AssignCorruptedMapAction(slot, eval);
                return;
            }

            switch (_mapMode)
            {
                case GambleType.Map:
                    AssignMapAction(slot, eval);
                    break;
                case GambleType.MapExalt:
                    AssignMapExaltAction(slot, eval);
                    break;
                case GambleType.MapT17:
                    AssignMapT17Action(slot, eval);
                    break;
            }
        }

        private static void AssignCorruptedMapAction(BulkMapSlot slot, MapRulesResult eval)
        {
            if (eval.RulesPassed)
            {
                MarkFinished(slot);
                return;
            }

            GamblerLog.Warn($"Corrupted map at {slot.Position.X},{slot.Position.Y} failed rules — queued for stash");
            slot.NextAction = BulkMapAction.StashBroken;
        }

        private void AssignMapAction(BulkMapSlot slot, MapRulesResult eval)
        {
            if (eval.RulesPassed)
            {
                if (ShouldCorrupt())
                    slot.NextAction = BulkMapAction.Vaal;
                else
                    MarkFinished(slot);
                return;
            }

            AssignScourOrAlchemyOnly(slot, eval);
        }

        private void AssignMapExaltAction(BulkMapSlot slot, MapRulesResult eval)
        {
            if (!eval.IsRare)
            {
                slot.NextAction = BulkMapAction.AlchemyOnly;
                return;
            }

            if (eval.ExcludeHit)
            {
                AssignScourOrAlchemyOnly(slot, eval);
                return;
            }

            if (eval.AffixModCount >= TargetAffixCount)
            {
                if (eval.RulesPassed)
                {
                    if (ShouldCorrupt())
                        slot.NextAction = BulkMapAction.Vaal;
                    else
                        MarkFinished(slot);
                }
                else
                {
                    AssignScourOrAlchemyOnly(slot, eval);
                }

                return;
            }

            slot.NextAction = BulkMapAction.Exalt;
        }

        private static void AssignScourOrAlchemyOnly(BulkMapSlot slot, MapRulesResult eval) =>
            BulkMapActionHelper.AssignScourOrAlchemyOnly(slot, eval);

        private void AssignMapT17Action(BulkMapSlot slot, MapRulesResult eval)
        {
            if (eval.RulesPassed)
            {
                if (ShouldCorrupt())
                    slot.NextAction = BulkMapAction.Vaal;
                else
                    MarkFinished(slot);
                return;
            }

            slot.NextAction = BulkMapAction.Chaos;
        }

        private static void MarkFinished(BulkMapSlot slot)
        {
            slot.IsFinished = true;
            slot.NextAction = BulkMapAction.Done;
        }

        private async Task ExecuteScourAlchemyBatchAsync()
        {
            var scourTargets = _slots.Where(s => s.IsActive && s.NextAction == BulkMapAction.ScourAlchemy).ToList();
            var alchemyTargets = _slots.Where(s => s.IsActive && s.NextAction == BulkMapAction.AlchemyOnly).ToList();
            if (scourTargets.Count == 0 && alchemyTargets.Count == 0)
                return;

            if (!await EnsureAlchemyPrimedAsync())
                return;

            foreach (var slot in scourTargets)
            {
                _token.ThrowIfCancellationRequested();
                await MoveToAsync(slot.Position);
                await SlamScourAsync();
                await RefreshAndAssignAsync(slot);
                await SlamAlchemyAsync();
                await RefreshAndAssignAsync(slot);
            }

            foreach (var slot in alchemyTargets)
            {
                _token.ThrowIfCancellationRequested();
                await MoveToAsync(slot.Position);
                await SlamAlchemyAsync();
                await RefreshAndAssignAsync(slot);
            }
        }

        private async Task SlamScourAsync()
        {
            _inputHost.Simulator.Send("Alt Down");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("LButton Down");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("LButton Up");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("Alt Up");
            await Task.Delay(_delay, _token);
        }

        private async Task SlamAlchemyAsync()
        {
            _inputHost.Simulator.Send("LButton Down");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("LButton Up");
            await Task.Delay(_delay, _token);
        }

        private async Task ExecuteExaltBatchLoopAsync()
        {
            if (_mapMode != GambleType.MapExalt)
                return;

            if (!_slots.Any(s => s.IsActive && s.NextAction == BulkMapAction.Exalt))
                return;

            if (!await EnsureExaltPrimedAsync())
                return;

            while (_slots.Any(s => s.IsActive && s.NextAction == BulkMapAction.Exalt))
            {
                _token.ThrowIfCancellationRequested();
                await ExecuteExaltBatchPassAsync();
            }
        }

        private async Task ExecuteExaltBatchPassAsync()
        {
            foreach (var slot in _slots.Where(s => s.IsActive && s.NextAction == BulkMapAction.Exalt).ToList())
            {
                _token.ThrowIfCancellationRequested();

                if (slot.Evaluation.AffixModCount >= TargetAffixCount)
                {
                    AssignNextAction(slot);
                    continue;
                }

                await MoveToAsync(slot.Position);

                int affixBefore = slot.Evaluation.AffixModCount;

                _inputHost.Simulator.Send("LButton Down");
                await Task.Delay(_delay, _token);
                _inputHost.Simulator.Send("LButton Up");
                await Task.Delay(_delay, _token);

                if (!await RefreshAndAssignAsync(slot))
                    continue;

                CheckExaltSlamResult(slot, affixBefore);
                if (_token.IsCancellationRequested)
                    return;
            }
        }

        private void CheckExaltSlamResult(BulkMapSlot slot, int affixCountBefore)
        {
            int affixAfter = slot.Evaluation.AffixModCount;
            if (affixAfter > affixCountBefore)
            {
                _consecutiveExaltNoModSlams = 0;
                return;
            }

            _consecutiveExaltNoModSlams++;
            GamblerLog.Warn(
                $"Exalt did not add a mod at {slot.Position.X},{slot.Position.Y} " +
                $"({affixCountBefore} → {affixAfter}; empty hand, lag, or stale copy?)");

            if (_consecutiveExaltNoModSlams >= MaxConsecutiveExaltNoModSlams)
            {
                GamblerLog.Error(
                    $"Stopping after {MaxConsecutiveExaltNoModSlams} exalt slams in a row with no new mods " +
                    "(orb not on cursor or clipboard not refreshed).");
                _cts.Cancel();
            }
        }

        private void ResetExaltSlamGuard() => _consecutiveExaltNoModSlams = 0;

        private async Task ExecuteChaosBatchLoopAsync()
        {
            if (_mapMode != GambleType.MapT17)
                return;

            if (!_slots.Any(s => s.IsActive && s.NextAction == BulkMapAction.Chaos))
                return;

            if (!await EnsureChaosPrimedAsync())
                return;

            while (_slots.Any(s => s.IsActive && s.NextAction == BulkMapAction.Chaos))
            {
                _token.ThrowIfCancellationRequested();
                await ExecuteChaosBatchPassAsync();
            }
        }

        private async Task ExecuteChaosBatchPassAsync()
        {
            foreach (var slot in _slots.Where(s => s.IsActive && s.NextAction == BulkMapAction.Chaos).ToList())
            {
                _token.ThrowIfCancellationRequested();
                await MoveToAsync(slot.Position);

                _inputHost.Simulator.Send("LButton Down");
                await Task.Delay(_delay, _token);
                _inputHost.Simulator.Send("LButton Up");
                await Task.Delay(_delay, _token);

                await RefreshAndAssignAsync(slot);
            }
        }

        private async Task ExecuteVaalBatchAsync()
        {
            if (!_slots.Any(s => s.IsActive && s.NextAction == BulkMapAction.Vaal))
                return;

            if (!await EnsureVaalPrimedAsync())
                return;

            foreach (var slot in _slots.Where(s => s.IsActive && s.NextAction == BulkMapAction.Vaal).ToList())
            {
                _token.ThrowIfCancellationRequested();

                await MoveToAsync(slot.Position);

                _inputHost.Simulator.Send("LButton Down");
                await Task.Delay(_delay, _token);
                _inputHost.Simulator.Send("LButton Up");
                await Task.Delay(_delay, _token);

                if (!await RefreshSlotAsync(slot))
                {
                    slot.IsFinished = true;
                    continue;
                }

                if (RequiresEightModsAfterCorrupt())
                {
                    slot.Evaluation = MapRulesEvaluator.Evaluate(
                        slot.Content!,
                        MapCorruptRulesHelper.RulesForPostCorruptEvaluation(_rules, requireEightAffixes: true),
                        logMods: false);
                }

                if (slot.Evaluation.RulesPassed)
                {
                    MarkFinished(slot);
                    continue;
                }

                GamblerLog.Warn($"Broken map at {slot.Position.X},{slot.Position.Y} after Vaal — queued for stash");
                slot.NextAction = BulkMapAction.StashBroken;
            }
        }

        private async Task ExecuteStashBrokenBatchAsync()
        {
            var targets = _slots.Where(s => s.IsActive && s.NextAction == BulkMapAction.StashBroken).ToList();
            if (targets.Count == 0)
                return;

            await ReleaseShiftAndReturnHeldOrbAsync();

            foreach (var slot in targets)
            {
                _token.ThrowIfCancellationRequested();
                await StashMapAsync(slot);
                slot.IsFinished = true;
                slot.NextAction = BulkMapAction.Done;
            }
        }

        private async Task StashMapAsync(BulkMapSlot slot)
        {
            await MoveToAsync(slot.Position);
            _inputHost.Simulator.Send("Ctrl Down");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("LButton Down");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("LButton Up");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("Ctrl Up");
            await Task.Delay(_delay, _token);
        }

        private async Task<bool> RefreshAndAssignAsync(BulkMapSlot slot)
        {
            if (!await RefreshSlotAsync(slot))
                return false;

            AssignNextAction(slot);
            return true;
        }

        private async Task<bool> RefreshSlotAsync(BulkMapSlot slot)
        {
            await Task.Delay(_refreshDelay, _token);

            string? content = await MapClipboardHelper.CopyMapAsync(_main, _inputHost, _delay, _token);
            if (content is null)
            {
                slot.IsEmpty = true;
                return false;
            }

            slot.Content = content;
            slot.Evaluation = MapRulesEvaluator.Evaluate(content, _rules, logMods: false);
            return true;
        }

        private async Task MoveToAsync(Coordinates point)
        {
            _inputHost.Simulator.MouseDeltaMove(point.X, point.Y, _speed);
            await Task.Delay(_delay, _token);
        }

        private void ClearPrimedOrbs()
        {
            _alchemyPrimed = false;
            _exaltPrimed = false;
            _chaosPrimed = false;
            _vaalPrimed = false;
        }

        private bool IsHoldingOrbAt(Coordinates stack) =>
            _heldOrbStack is { } held
            && held.X == stack.X
            && held.Y == stack.Y
            && IsOrbConfigured(stack);

        private static bool IsOrbConfigured(Coordinates orb) => orb.X > 0 && orb.Y > 0;

        private async Task ReleaseShiftAndModifiersAsync()
        {
            _inputHost.Simulator.Send("Shift Up");
            _shiftHeld = false;
            await Task.Delay(_delay, _token);
            GambleInputReleaseHelper.ReleaseModifiers(_inputHost);
            await Task.Delay(_refreshDelay, _token);
        }

        private async Task ReleaseShiftAndReturnHeldOrbAsync()
        {
            await ReleaseShiftAndModifiersAsync();
            await ReturnHeldOrbAsync();
        }

        private async Task ReturnHeldOrbAsync()
        {
            if (_heldOrbStack is null)
            {
                ClearPrimedOrbs();
                return;
            }

            Coordinates dropOn = ResolveOrbDropMapCell();
            GamblerLog.Info($"Dropping orb over map slot ({dropOn.X},{dropOn.Y})");
            await MoveToAsync(dropOn);
            await SendOrbRightClickAsync();
            await Task.Delay(_refreshDelay, _token);
            _heldOrbStack = null;
            ClearPrimedOrbs();
        }

        private Coordinates ResolveOrbDropMapCell()
        {
            foreach (var slot in _slots)
            {
                if (slot.IsActive)
                    return slot.Position;
            }

            return _slots[0].Position;
        }

        private async Task SendOrbRightClickAsync()
        {
            _inputHost.Simulator.Send("RButton Down");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("RButton Up");
            await Task.Delay(_delay, _token);
        }

        private async Task PickUpOrbWithoutShiftAsync(Coordinates stack)
        {
            GamblerLog.Info($"Picking up orb ({stack.X},{stack.Y})");
            await MoveToAsync(stack);
            await SendOrbRightClickAsync();
            await Task.Delay(_refreshDelay, _token);
            _heldOrbStack = stack;
            ResetExaltSlamGuard();
        }

        private async Task SwapToOrbAsync(Coordinates stack)
        {
            await ReleaseShiftAndReturnHeldOrbAsync();
            await PickUpOrbWithoutShiftAsync(stack);
            await EnsureShiftHeldAsync();
        }

        private async Task<bool> EnsureAlchemyPrimedAsync()
        {
            if (_alchemyPrimed && IsHoldingOrbAt(_alchemy))
                return true;

            if (!IsOrbConfigured(_alchemy))
            {
                GamblerLog.Warn("Alchemy orb coordinates are not configured.");
                return false;
            }

            await SwapToOrbAsync(_alchemy);
            _alchemyPrimed = true;
            return true;
        }

        private async Task<bool> EnsureExaltPrimedAsync()
        {
            if (_exaltPrimed && IsHoldingOrbAt(_exalt))
                return true;

            if (!IsOrbConfigured(_exalt))
            {
                GamblerLog.Warn("Exalt orb coordinates are not configured.");
                return false;
            }

            await SwapToOrbAsync(_exalt);
            _exaltPrimed = true;
            GamblerLog.Info("Exalt orb on cursor — shift held for batch slams");
            return true;
        }

        private async Task<bool> EnsureChaosPrimedAsync()
        {
            if (_chaosPrimed && IsHoldingOrbAt(_chaos))
                return true;

            if (!IsOrbConfigured(_chaos))
            {
                GamblerLog.Warn("Chaos orb coordinates are not configured.");
                return false;
            }

            await SwapToOrbAsync(_chaos);
            _chaosPrimed = true;
            return true;
        }

        private async Task<bool> EnsureVaalPrimedAsync()
        {
            if (_vaalPrimed && IsHoldingOrbAt(_vaal))
                return true;

            if (!IsOrbConfigured(_vaal))
            {
                GamblerLog.Warn("Vaal orb coordinates are not configured.");
                return false;
            }

            await SwapToOrbAsync(_vaal);
            _vaalPrimed = true;
            return true;
        }

        private async Task EnsureShiftHeldAsync()
        {
            if (_shiftHeld)
                return;

            _inputHost.Simulator.Send("Shift Down");
            await Task.Delay(_delay, _token);
            _shiftHeld = true;
        }
    }
}
