using PoE.dlls.Settings;

namespace PoE.dlls.Flasks
{
    public class FlaskTiming
    {
        public TimeSpan PollDelay { get; set; } = TimeSpan.FromMilliseconds(100);
        public TimeSpan KeyPressDelay { get; set; } = TimeSpan.FromMilliseconds(5);
        public TimeSpan HpMpCooldown { get; set; } = TimeSpan.FromSeconds(2);
        public TimeSpan UtilityCooldown { get; set; } = TimeSpan.FromMicroseconds(500);
        public TimeSpan TinctureCooldown { get; set; } = TimeSpan.FromMilliseconds(500);

        public void Apply(UIFlaskControls controls)
        {
            PollDelay = TimeSpan.FromMilliseconds(controls.Delay);
            KeyPressDelay = TimeSpan.FromMilliseconds(controls.KeyPressDelay);
            HpMpCooldown = TimeSpan.FromMilliseconds(controls.HpMpCooldown);
            UtilityCooldown = TimeSpan.FromMilliseconds(controls.UtilityCooldown);
            TinctureCooldown = TimeSpan.FromMilliseconds(controls.TinctureCooldown);
        }
    }
}
