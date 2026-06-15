using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.InteropServices;
using PoE.dlls.Logger;
using Poss.Win.Automation.Input;
using PoE.dlls.Automation;

namespace PoE.dlls.Gamble.Modes
{
    public class MapExalt : IGamba
    {
        private const int TargetModCount = 6;

        private readonly Main _main;
        private readonly InputSimulatorHost inputHost;

        private readonly double speed;
        private readonly TimeSpan delay;

        private readonly Coordinates item;
        private readonly Coordinates alchemy;

        private readonly List<Rule> rules;

        private readonly CancellationTokenSource _cts;
        private readonly CancellationToken _token;

        private int _hash;
        private int count;
        private readonly int maxAttempts = 3;

        private bool _isShiftHeld;
        private bool _alchemyPrimed;

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
            List<Rule> rules)
        {
            _main = main;
            this.inputHost = inputHost;
            this.delay = delay;
            this.speed = speed;
            _cts = cts;
            _token = _cts.Token;
            this.item = item;
            this.alchemy = alchemy;
            _exalt = exalt;
            this.rules = rules;
        }

        private readonly Coordinates _exalt;

        public async Task Gamble()
        {
            try
            {
                while (!_token.IsCancellationRequested)
                {
                    inputHost.Simulator.MouseDeltaMove(item.X, item.Y, speed);
                    await Task.Delay(delay);

                    string? itemContent = await Copy();
                    if (itemContent is null)
                        return;

                    var evaluation = EvaluateClipboard(itemContent);
                    if (!evaluation.IsMap)
                    {
                        GamblerLog.Warn("Item is not a map.");
                        _cts.Cancel();
                        return;
                    }

                    if (!evaluation.IsRare)
                    {
                        await EnsureAlchemyPrimed();
                        await ScouringAlchemyOnItem();
                        continue;
                    }

                    if (evaluation.ExcludeHit)
                    {
                        await ScouringAlchemyOnItem();
                        continue;
                    }

                    if (evaluation.ModCount >= TargetModCount)
                    {
                        if (evaluation.RulesPassed)
                        {
                            GamblerLog.Success();
                            return;
                        }

                        await ScouringAlchemyOnItem();
                        continue;
                    }

                    await ExaltSlam();
                }

                if (_token.IsCancellationRequested)
                    GamblerLog.Cancelled();
            }
            finally
            {
                ReleaseShift();
            }
        }

        private MapRulesResult EvaluateClipboard(string itemContent)
        {
            if (!TrackClipboardHash(itemContent))
                return default;

            return MapRulesEvaluator.Evaluate(itemContent, rules);
        }

        private bool TrackClipboardHash(string itemContent)
        {
            int hash = itemContent.GetHashCode();
            if (_hash != hash)
            {
                _hash = hash;
                return true;
            }

            if (count >= maxAttempts)
            {
                GamblerLog.MaxAttemptsReached();
                _cts.Cancel();
                return false;
            }

            count++;
            return true;
        }

        private async Task<string?> Copy()
        {
            inputHost.Simulator.Send("Ctrl Down");
            await Task.Delay(delay);
            inputHost.Simulator.Send("Alt Down");
            await Task.Delay(delay);
            inputHost.Simulator.Send("C Down");
            await Task.Delay(delay);
            inputHost.Simulator.Send("C Up");
            await Task.Delay(delay);
            inputHost.Simulator.Send("Alt Up");
            await Task.Delay(delay);
            inputHost.Simulator.Send("Ctrl Up");
            await Task.Delay(delay);

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

        private async Task EnsureAlchemyPrimed()
        {
            if (_alchemyPrimed)
                return;

            inputHost.Simulator.MouseDeltaMove(alchemy.X, alchemy.Y, speed);
            await Task.Delay(delay);
            inputHost.Simulator.Send("RButton Down");
            await Task.Delay(delay);
            inputHost.Simulator.Send("RButton Up");
            await Task.Delay(delay);

            inputHost.Simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay);

            await EnsureShiftHeld();
            _alchemyPrimed = true;
        }

        private async Task EnsureShiftHeld()
        {
            if (_isShiftHeld)
                return;

            inputHost.Simulator.Send("Shift Down");
            await Task.Delay(delay);
            _isShiftHeld = true;
        }

        private void ReleaseShift()
        {
            if (!_isShiftHeld)
                return;

            inputHost.Simulator.Send("Shift Up");
            _isShiftHeld = false;
        }

        private async Task ScouringAlchemyOnItem()
        {
            await EnsureAlchemyPrimed();

            inputHost.Simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay);

            inputHost.Simulator.Send("Alt Down");
            await Task.Delay(delay);
            inputHost.Simulator.Send("LButton Down");
            await Task.Delay(delay);
            inputHost.Simulator.Send("LButton Up");
            await Task.Delay(delay);
            inputHost.Simulator.Send("Alt Up");
            await Task.Delay(delay);

            inputHost.Simulator.Send("LButton Down");
            await Task.Delay(delay);
            inputHost.Simulator.Send("LButton Up");
            await Task.Delay(delay);
        }

        private async Task ExaltSlam()
        {
            await EnsureShiftHeld();

            inputHost.Simulator.MouseDeltaMove(_exalt.X, _exalt.Y, speed);
            await Task.Delay(delay);
            inputHost.Simulator.Send("RButton Down");
            await Task.Delay(delay);
            inputHost.Simulator.Send("RButton Up");
            await Task.Delay(delay);

            inputHost.Simulator.MouseDeltaMove(item.X, item.Y, speed);
            await Task.Delay(delay);
            inputHost.Simulator.Send("LButton Down");
            await Task.Delay(delay);
            inputHost.Simulator.Send("LButton Up");
            await Task.Delay(delay);
        }
    }
}
