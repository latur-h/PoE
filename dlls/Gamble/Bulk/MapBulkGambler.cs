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

        private bool _shiftHeld;
        private bool _alchemyPrimed;
        private bool _exaltPrimed;
        private bool _chaosPrimed;
        private bool _vaalPrimed;

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

            GamblerLog.Info($"Bulk map session: {_slots.Count} slots, mode {_mapMode}");
        }

        public async Task Gamble()
        {
            LogBulkSetup();

            try
            {
                while (!_token.IsCancellationRequested && _slots.Any(s => s.IsActive))
                {
                    await ExecuteStashBrokenBatchAsync();

                    await PrecheckAllAsync();

                    if (_token.IsCancellationRequested)
                        break;

                    if (!_slots.Any(s => s.IsActive && s.NextAction is BulkMapAction.ScourAlchemy or BulkMapAction.Exalt or BulkMapAction.Chaos or BulkMapAction.Vaal or BulkMapAction.StashBroken))
                        break;

                    await ExecuteScourAlchemyBatchAsync();
                    await ExecuteExaltBatchAsync();
                    await ExecuteChaosBatchAsync();
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
                GambleInputReleaseHelper.ReleaseAll(_inputHost);
            }
        }

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
        }

        private bool ShouldCorrupt() => _corruptOnSuccess && MapCorruptHelper.IsVaalConfigured(_vaal);

        private async Task PrecheckAllAsync()
        {
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

            slot.NextAction = BulkMapAction.ScourAlchemy;
        }

        private void AssignMapExaltAction(BulkMapSlot slot, MapRulesResult eval)
        {
            if (!eval.IsRare)
            {
                slot.NextAction = BulkMapAction.ScourAlchemy;
                return;
            }

            if (eval.ExcludeHit)
            {
                slot.NextAction = BulkMapAction.ScourAlchemy;
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
                    slot.NextAction = BulkMapAction.ScourAlchemy;
                }

                return;
            }

            slot.NextAction = BulkMapAction.Exalt;
        }

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
            var targets = _slots.Where(s => s.IsActive && s.NextAction == BulkMapAction.ScourAlchemy).ToList();
            if (targets.Count == 0)
                return;

            if (!await EnsureAlchemyPrimedAsync())
                return;

            foreach (var slot in targets)
            {
                _token.ThrowIfCancellationRequested();
                await MoveToAsync(slot.Position);

                _inputHost.Simulator.Send("Alt Down");
                await Task.Delay(_delay, _token);
                _inputHost.Simulator.Send("LButton Down");
                await Task.Delay(_delay, _token);
                _inputHost.Simulator.Send("LButton Up");
                await Task.Delay(_delay, _token);
                _inputHost.Simulator.Send("Alt Up");
                await Task.Delay(_delay, _token);

                await RefreshAndAssignAsync(slot);

                _inputHost.Simulator.Send("LButton Down");
                await Task.Delay(_delay, _token);
                _inputHost.Simulator.Send("LButton Up");
                await Task.Delay(_delay, _token);

                await RefreshAndAssignAsync(slot);
            }
        }

        private async Task ExecuteExaltBatchAsync()
        {
            if (_mapMode != GambleType.MapExalt)
                return;

            if (!await EnsureExaltPrimedAsync())
                return;

            foreach (var slot in _slots.Where(s => s.IsActive).ToList())
            {
                _token.ThrowIfCancellationRequested();
                await MoveToAsync(slot.Position);

                if (!await RefreshAndAssignAsync(slot))
                    continue;

                if (slot.NextAction != BulkMapAction.Exalt)
                    continue;

                _inputHost.Simulator.Send("LButton Down");
                await Task.Delay(_delay, _token);
                _inputHost.Simulator.Send("LButton Up");
                await Task.Delay(_delay, _token);

                await RefreshAndAssignAsync(slot);
            }
        }

        private async Task ExecuteChaosBatchAsync()
        {
            if (_mapMode != GambleType.MapT17)
                return;

            if (!_slots.Any(s => s.IsActive && s.NextAction == BulkMapAction.Chaos))
                return;

            if (!await EnsureChaosPrimedAsync())
                return;

            foreach (var slot in _slots.Where(s => s.IsActive).ToList())
            {
                _token.ThrowIfCancellationRequested();
                await MoveToAsync(slot.Position);

                if (!await RefreshAndAssignAsync(slot))
                    continue;

                if (slot.NextAction != BulkMapAction.Chaos)
                    continue;

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

            foreach (var slot in _slots.Where(s => s.IsActive).ToList())
            {
                _token.ThrowIfCancellationRequested();

                await MoveToAsync(slot.Position);

                if (!await RefreshAndAssignAsync(slot))
                {
                    slot.IsFinished = true;
                    continue;
                }

                if (slot.NextAction != BulkMapAction.Vaal)
                    continue;

                _inputHost.Simulator.Send("LButton Down");
                await Task.Delay(_delay, _token);
                _inputHost.Simulator.Send("LButton Up");
                await Task.Delay(_delay, _token);

                if (!await RefreshSlotAsync(slot))
                {
                    slot.IsFinished = true;
                    continue;
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

            await PrepareOrbSwapAsync();

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

        private static bool IsOrbConfigured(Coordinates orb) => orb.X > 0 && orb.Y > 0;

        private async Task PrepareOrbSwapAsync()
        {
            GambleInputReleaseHelper.ReleaseAll(_inputHost);
            _shiftHeld = false;
            ClearPrimedOrbs();
            await Task.Delay(_delay, _token);
        }

        private async Task<bool> EnsureAlchemyPrimedAsync()
        {
            if (_alchemyPrimed)
                return true;

            if (!IsOrbConfigured(_alchemy))
            {
                GamblerLog.Warn("Alchemy orb coordinates are not configured.");
                return false;
            }

            await PrepareOrbSwapAsync();
            await MoveToAsync(_alchemy);
            _inputHost.Simulator.Send("RButton Down");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("RButton Up");
            await Task.Delay(_delay, _token);

            await EnsureShiftHeldAsync();
            _alchemyPrimed = true;
            return true;
        }

        private async Task<bool> EnsureExaltPrimedAsync()
        {
            if (_exaltPrimed)
                return true;

            if (!IsOrbConfigured(_exalt))
            {
                GamblerLog.Warn("Exalt orb coordinates are not configured.");
                return false;
            }

            await PrepareOrbSwapAsync();
            await MoveToAsync(_exalt);
            _inputHost.Simulator.Send("RButton Down");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("RButton Up");
            await Task.Delay(_delay, _token);

            await EnsureShiftHeldAsync();
            _exaltPrimed = true;
            GamblerLog.Info("Exalt orb picked up — shift held for batch slams");
            return true;
        }

        private async Task<bool> EnsureChaosPrimedAsync()
        {
            if (_chaosPrimed)
                return true;

            if (!IsOrbConfigured(_chaos))
            {
                GamblerLog.Warn("Chaos orb coordinates are not configured.");
                return false;
            }

            await PrepareOrbSwapAsync();
            await MoveToAsync(_chaos);
            _inputHost.Simulator.Send("RButton Down");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("RButton Up");
            await Task.Delay(_delay, _token);

            await EnsureShiftHeldAsync();
            _chaosPrimed = true;
            return true;
        }

        private async Task<bool> EnsureVaalPrimedAsync()
        {
            if (_vaalPrimed)
                return true;

            if (!IsOrbConfigured(_vaal))
            {
                GamblerLog.Warn("Vaal orb coordinates are not configured.");
                return false;
            }

            await PrepareOrbSwapAsync();
            await MoveToAsync(_vaal);
            _inputHost.Simulator.Send("RButton Down");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("RButton Up");
            await Task.Delay(_delay, _token);

            await EnsureShiftHeldAsync();
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

        private void ReleaseShift()
        {
            GambleInputReleaseHelper.ReleaseAll(_inputHost);
            _shiftHeld = false;
            ClearPrimedOrbs();
        }
    }
}
