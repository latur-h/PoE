using PoE.dlls.Gamble;
using PoE.dlls.Gamble.Modifiers;
using PoE.dlls.InteropServices;

namespace PoE.dlls.Settings.Mods
{
    internal sealed class GambleEditAdapter : IUIMods
    {
        private static readonly GambleRuleRow EmptySlot = new();

        private readonly UIModifiers _modifiers;
        private GambleModeStore _store = null!;
        private GamblePreset _preset = null!;

        public GambleEditAdapter(UIModifiers modifiers) => _modifiers = modifiers;

        public void Bind(GambleType type)
        {
            _store = _modifiers.GetModeStore(type);
            _preset = _modifiers.GetActivePreset(type);
        }

        public Coordinates Item
        {
            get => GambleCoordinateResolver.GetItem(_modifiers.GambleType, _modifiers.Items);
            set => GambleCoordinateResolver.SetItem(_modifiers.GambleType, _modifiers.Items, value);
        }

        public Coordinates Base
        {
            get => ResolveOrb(GambleCoordinateResolver.PrimaryOrb(_modifiers.GambleType));
            set => SetOrb(GambleCoordinateResolver.PrimaryOrb(_modifiers.GambleType), value);
        }

        public Coordinates Second
        {
            get => ResolveOrb(GambleCoordinateResolver.SecondaryOrb(_modifiers.GambleType));
            set => SetOrb(GambleCoordinateResolver.SecondaryOrb(_modifiers.GambleType), value);
        }

        private Coordinates ResolveOrb(GambleOrbType? type) =>
            type is null ? new Coordinates(0, 0) : _modifiers.Orbs.Get(type.Value);

        private void SetOrb(GambleOrbType? type, Coordinates value)
        {
            if (type is not null)
                _modifiers.Orbs.Set(type.Value, value);
        }

        public decimal Priority1 { get => Read(0).Priority; set => Write(0).Priority = value; }
        public decimal Priority2 { get => Read(1).Priority; set => Write(1).Priority = value; }
        public decimal Priority3 { get => Read(2).Priority; set => Write(2).Priority = value; }
        public decimal Priority4 { get => Read(3).Priority; set => Write(3).Priority = value; }
        public decimal Priority5 { get => Read(4).Priority; set => Write(4).Priority = value; }
        public decimal Priority6 { get => Read(5).Priority; set => Write(5).Priority = value; }
        public decimal Priority7 { get => Read(6).Priority; set => Write(6).Priority = value; }
        public decimal Priority8 { get => Read(7).Priority; set => Write(7).Priority = value; }

        public ModifierType modifierType1 { get => Read(0).ModifierType; set => Write(0).ModifierType = value; }
        public ModifierType modifierType2 { get => Read(1).ModifierType; set => Write(1).ModifierType = value; }
        public ModifierType modifierType3 { get => Read(2).ModifierType; set => Write(2).ModifierType = value; }
        public ModifierType modifierType4 { get => Read(3).ModifierType; set => Write(3).ModifierType = value; }
        public ModifierType modifierType5 { get => Read(4).ModifierType; set => Write(4).ModifierType = value; }
        public ModifierType modifierType6 { get => Read(5).ModifierType; set => Write(5).ModifierType = value; }
        public ModifierType modifierType7 { get => Read(6).ModifierType; set => Write(6).ModifierType = value; }
        public ModifierType modifierType8 { get => Read(7).ModifierType; set => Write(7).ModifierType = value; }

        public int Tier1 { get => Read(0).Tier; set => Write(0).Tier = value; }
        public int Tier2 { get => Read(1).Tier; set => Write(1).Tier = value; }
        public int Tier3 { get => Read(2).Tier; set => Write(2).Tier = value; }
        public int Tier4 { get => Read(3).Tier; set => Write(3).Tier = value; }
        public int Tier5 { get => Read(4).Tier; set => Write(4).Tier = value; }
        public int Tier6 { get => Read(5).Tier; set => Write(5).Tier = value; }
        public int Tier7 { get => Read(6).Tier; set => Write(6).Tier = value; }
        public int Tier8 { get => Read(7).Tier; set => Write(7).Tier = value; }

        public string Content1 { get => Read(0).Content; set => Write(0).Content = value; }
        public string Content2 { get => Read(1).Content; set => Write(1).Content = value; }
        public string Content3 { get => Read(2).Content; set => Write(2).Content = value; }
        public string Content4 { get => Read(3).Content; set => Write(3).Content = value; }
        public string Content5 { get => Read(4).Content; set => Write(4).Content = value; }
        public string Content6 { get => Read(5).Content; set => Write(5).Content = value; }
        public string Content7 { get => Read(6).Content; set => Write(6).Content = value; }
        public string Content8 { get => Read(7).Content; set => Write(7).Content = value; }

        private GambleRuleRow Read(int index) =>
            index < _preset.Rules.Count ? _preset.Rules[index] : EmptySlot;

        private GambleRuleRow Write(int index)
        {
            while (_preset.Rules.Count <= index && _preset.Rules.Count < GambleModeLayout.MaxRules)
                _preset.Rules.Add(new GambleRuleRow());

            return _preset.Rules[index];
        }
    }
}
