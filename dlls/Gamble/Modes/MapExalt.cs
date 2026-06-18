using PoE.dlls.Automation;
using PoE.dlls.Gamba;
using PoE.dlls.Gamble;
using PoE.dlls.Gamble.Bulk;
using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.InteropServices;
using PoE.dlls.Logger;

namespace PoE.dlls.Gamble.Modes
{
    public class MapExalt : IGamba
    {
        private const int TargetAffixCount = 6;
        private const int MaxConsecutiveExaltNoModSlams = 3;

        private readonly Main _main;
        private readonly InputSimulatorHost _inputHost;
        private readonly double _speed;
        private readonly TimeSpan _delay;
        private readonly TimeSpan _refreshDelay;
        private readonly Coordinates _item;
        private readonly Coordinates _alchemy;
        private readonly Coordinates _exalt;
        private readonly List<Rule> _rules;
        private readonly MapGambleSession _session;
        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _token;

        private bool _shiftHeld;
        private bool _alchemyPrimed;
        private bool _exaltPrimed;
        private Coordinates? _heldOrbStack;
        private int _consecutiveExaltNoModSlams;
        private string _lastItemContent = string.Empty;
        private bool _cursorOnItem;

        public MapExalt(
            Main main,
            InputSimulatorHost inputHost,
            CancellationTokenSource cts,
            TimeSpan delay,
            double speed,
            Coordinates item,
            Coordinates alchemy,
            Coordinates scouring,
            Coordinates exalt,
            List<Rule> rules,
            MapGambleSession session)
        {
            _main = main;
            _inputHost = inputHost;
            _delay = delay;
            _speed = speed;
            _cts = cts;
            _token = _cts.Token;
            _item = item;
            _alchemy = alchemy;
            _exalt = exalt;
            _rules = rules;
            _session = session;
            _refreshDelay = BulkMapActionHelper.ResolveRefreshDelay(session.BulkGrid, delay);
        }

        public async Task Gamble()
        {
            try
            {
                await MoveToItemAsync();
                _cursorOnItem = true;

                while (!_token.IsCancellationRequested)
                {
                    MapRulesResult eval = await CopyAndEvaluateAsync();
                    if (!eval.IsMap)
                    {
                        GamblerLog.Warn("Item is not a map.");
                        return;
                    }

                    if (eval.IsCorrupted)
                    {
                        GamblerLog.Warn("Corrupted map cannot be modified further (single item — no action).");
                        return;
                    }

                    if (!eval.IsRare)
                    {
                        if (!await ApplyNonRarePrepAsync())
                            return;

                        continue;
                    }

                    if (eval.ExcludeHit)
                    {
                        if (!await ApplyScourAlchemyAsync())
                            return;

                        continue;
                    }

                    if (eval.AffixModCount >= TargetAffixCount)
                    {
                        if (eval.RulesPassed)
                        {
                            await FinishWithOptionalCorruptAsync();
                            return;
                        }

                        if (!await ApplyScourAlchemyAsync())
                            return;

                        continue;
                    }

                    if (!await EnsureExaltPrimedAsync())
                        return;

                    int affixBefore = eval.AffixModCount;
                    await SlamExaltAsync();
                    await Task.Delay(_refreshDelay, _token);

                    MapRulesResult after = await CopyAndEvaluateAsync();
                    if (!after.IsMap)
                        return;

                    if (after.IsCorrupted)
                    {
                        GamblerLog.Warn("Corrupted map cannot be modified further (single item — no action).");
                        return;
                    }

                    if (after.ExcludeHit)
                    {
                        if (!await ApplyScourAlchemyAsync())
                            return;

                        continue;
                    }

                    if (after.AffixModCount >= TargetAffixCount)
                    {
                        if (after.RulesPassed)
                        {
                            await FinishWithOptionalCorruptAsync();
                            return;
                        }

                        if (!await ApplyScourAlchemyAsync())
                            return;

                        continue;
                    }

                    if (!CheckExaltSlamResult(affixBefore, after.AffixModCount))
                        return;
                }

                if (_token.IsCancellationRequested)
                    GamblerLog.Cancelled();
            }
            finally
            {
                await CleanupOrbStateAsync();
            }
        }

        private async Task FinishWithOptionalCorruptAsync()
        {
            await ReleaseShiftAndReturnHeldOrbAsync();

            var corruptResult = await MapCorruptHelper.TryFinishWithOptionalCorruptAsync(
                _main,
                _inputHost,
                _token,
                _delay,
                _speed,
                _item,
                _session.Vaal,
                _session.CorruptOnSuccess,
                _session.RequiresEightModsAfterCorrupt,
                _rules);

            if (_token.IsCancellationRequested)
            {
                GamblerLog.Cancelled();
                return;
            }

            if (corruptResult == true)
                GamblerLog.Success();
        }

        private async Task<bool> ApplyNonRarePrepAsync()
        {
            if (MapRulesEvaluator.IsMagic(_lastItemContent))
                return await ApplyScourAlchemyAsync();

            return await ApplyAlchemyOnlyAsync();
        }

        private async Task<bool> ApplyScourAlchemyAsync()
        {
            if (!await EnsureAlchemyPrimedAsync())
                return false;

            await MoveToItemIfNeededAsync();
            await SlamScourAsync();
            await Task.Delay(_refreshDelay, _token);
            await SlamAlchemyAsync();
            await Task.Delay(_refreshDelay, _token);
            return true;
        }

        private async Task<bool> ApplyAlchemyOnlyAsync()
        {
            if (!await EnsureAlchemyPrimedAsync())
                return false;

            await MoveToItemIfNeededAsync();
            await SlamAlchemyAsync();
            await Task.Delay(_refreshDelay, _token);
            return true;
        }

        private bool CheckExaltSlamResult(int affixCountBefore, int affixCountAfter)
        {
            if (affixCountAfter > affixCountBefore)
            {
                _consecutiveExaltNoModSlams = 0;
                return true;
            }

            _consecutiveExaltNoModSlams++;
            GamblerLog.Warn(
                $"Exalt did not add a mod ({affixCountBefore} → {affixCountAfter}; empty hand, lag, or stale copy?)");

            if (_consecutiveExaltNoModSlams >= MaxConsecutiveExaltNoModSlams)
            {
                GamblerLog.Error(
                    $"Stopping after {MaxConsecutiveExaltNoModSlams} exalt slams in a row with no new mods " +
                    "(orb not on cursor or clipboard not refreshed).");
                _cts.Cancel();
                return false;
            }

            return true;
        }

        private async Task<MapRulesResult> CopyAndEvaluateAsync()
        {
            string? content = await CopyMapAsync();
            if (content is null)
                return default;

            _lastItemContent = content;
            return MapRulesEvaluator.Evaluate(content, _rules);
        }

        private async Task<string?> CopyMapAsync()
        {
            await MoveToItemIfNeededAsync();

            _inputHost.Simulator.Send("Ctrl Down");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("Alt Down");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("C Down");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("C Up");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("Alt Up");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("Ctrl Up");
            await Task.Delay(_delay, _token);

            string itemContent = _main.Invoke(() => Clipboard.GetText(TextDataFormat.Text));
            if (string.IsNullOrEmpty(itemContent))
            {
                GamblerLog.ClipboardEmptyWarning();
                _cts.Cancel();
                return null;
            }

            _main.Invoke(Clipboard.Clear);
            return itemContent;
        }

        private async Task MoveToItemIfNeededAsync()
        {
            if (_cursorOnItem)
                return;

            await MoveToItemAsync();
            _cursorOnItem = true;
        }

        private async Task MoveToItemAsync()
        {
            _inputHost.Simulator.MouseDeltaMove(_item.X, _item.Y, _speed);
            await Task.Delay(_delay, _token);
            _cursorOnItem = true;
        }

        private void MarkCursorLeftItem() => _cursorOnItem = false;

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

        private async Task SlamExaltAsync()
        {
            await MoveToItemIfNeededAsync();
            _inputHost.Simulator.Send("LButton Down");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("LButton Up");
            await Task.Delay(_delay, _token);
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
            _exaltPrimed = false;
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
            _alchemyPrimed = false;
            return true;
        }

        private async Task SwapToOrbAsync(Coordinates stack)
        {
            await ReleaseShiftAndReturnHeldOrbAsync();
            await PickUpOrbWithoutShiftAsync(stack);
            await EnsureShiftHeldAsync();
        }

        private async Task PickUpOrbWithoutShiftAsync(Coordinates stack)
        {
            GamblerLog.Info($"Picking up orb ({stack.X},{stack.Y})");
            MarkCursorLeftItem();
            await MoveToAsync(stack);
            await SendOrbRightClickAsync();
            await Task.Delay(_refreshDelay, _token);
            _heldOrbStack = stack;
            _consecutiveExaltNoModSlams = 0;
        }

        private async Task ReleaseShiftAndReturnHeldOrbAsync()
        {
            await ReleaseShiftAndModifiersAsync();
            await ReturnHeldOrbAsync();
        }

        private async Task ReleaseShiftAndModifiersAsync()
        {
            _inputHost.Simulator.Send("Shift Up");
            _shiftHeld = false;
            await Task.Delay(_delay, _token);
            GambleInputReleaseHelper.ReleaseModifiers(_inputHost);
            await Task.Delay(_refreshDelay, _token);
        }

        private async Task ReturnHeldOrbAsync()
        {
            if (_heldOrbStack is null)
            {
                ClearPrimedOrbs();
                return;
            }

            GamblerLog.Info($"Dropping orb over map ({_item.X},{_item.Y})");
            await MoveToItemIfNeededAsync();
            await SendOrbRightClickAsync();
            await Task.Delay(_refreshDelay, _token);
            _heldOrbStack = null;
            ClearPrimedOrbs();
        }

        private async Task CleanupOrbStateAsync()
        {
            try
            {
                await ReleaseShiftAndReturnHeldOrbAsync();
            }
            catch (OperationCanceledException) when (_token.IsCancellationRequested)
            {
            }

            GambleInputReleaseHelper.ReleaseModifiers(_inputHost);
        }

        private async Task EnsureShiftHeldAsync()
        {
            if (_shiftHeld)
                return;

            _inputHost.Simulator.Send("Shift Down");
            await Task.Delay(_delay, _token);
            _shiftHeld = true;
        }

        private async Task SendOrbRightClickAsync()
        {
            _inputHost.Simulator.Send("RButton Down");
            await Task.Delay(_delay, _token);
            _inputHost.Simulator.Send("RButton Up");
            await Task.Delay(_delay, _token);
        }

        private async Task MoveToAsync(Coordinates point)
        {
            if (point.X != _item.X || point.Y != _item.Y)
                MarkCursorLeftItem();

            _inputHost.Simulator.MouseDeltaMove(point.X, point.Y, _speed);
            await Task.Delay(_delay, _token);
        }

        private void ClearPrimedOrbs()
        {
            _alchemyPrimed = false;
            _exaltPrimed = false;
        }

        private bool IsHoldingOrbAt(Coordinates stack) =>
            _heldOrbStack is { } held
            && held.X == stack.X
            && held.Y == stack.Y
            && IsOrbConfigured(stack);

        private static bool IsOrbConfigured(Coordinates orb) => orb.X > 0 && orb.Y > 0;
    }
}
