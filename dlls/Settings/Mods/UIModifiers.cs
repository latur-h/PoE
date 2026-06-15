using Newtonsoft.Json;
using PoE.dlls.Gamble;
using PoE.dlls.Gamble.Modifiers;

namespace PoE.dlls.Settings.Mods
{
    public class UIModifiers
    {
        public GambleType GambleType { get; set; } = GambleType.Alt;

        public string GetCoorinatesKey = "F6";
        public string GamblerStart = "F7";
        public string GamblerStop = "F8";
        public int Delay { get; set; } = 40;
        public double Speed { get; set; } = 10.0;

        public Dictionary<GambleType, GambleModeStore> ModeStores { get; set; } = [];

        public GambleItemCoordinates Items { get; set; } = new();
        public GambleOrbCoordinates Orbs { get; set; } = new();

        [JsonIgnore]
        private GambleEditAdapter? _editAdapter;

        [JsonIgnore]
        public IUIMods Mode => GetEditAdapter();

        [JsonProperty]
        internal UIAlt _uialt { get; set; } = new();

        [JsonProperty]
        internal UIAlt_Aug _uialt_aug { get; set; } = new();

        [JsonProperty]
        internal UIChaos _uichaos { get; set; } = new();

        [JsonProperty]
        internal UIChromatic _uichromatic { get; set; } = new();

        [JsonProperty]
        internal UIEldritch _uieldritch { get; set; } = new();

        [JsonProperty]
        internal UIEssence _uiesscence { get; set; } = new();

        [JsonProperty]
        internal UIHarvest _uiharvest { get; set; } = new();

        [JsonProperty]
        internal UIMap _uimap { get; set; } = new();

        [JsonProperty]
        internal UIMapT17 _uimapT17 { get; set; } = new();

        public UIModifiers()
        {
            foreach (GambleType type in Enum.GetValues<GambleType>())
                ModeStores[type] = new GambleModeStore();
        }

        public GambleModeStore GetModeStore(GambleType type) => ModeStores[type];

        public GamblePreset GetActivePreset(GambleType? type = null)
        {
            type ??= GambleType;
            var store = GetModeStore(type.Value);
            GamblePresetHelper.NormalizeModeStore(store);

            return store.Presets.First(p =>
                string.Equals(p.Name, store.ActivePresetName, StringComparison.OrdinalIgnoreCase));
        }

        public IUIMods GetEditAdapter()
        {
            _editAdapter ??= new GambleEditAdapter(this);
            _editAdapter.Bind(GambleType);
            return _editAdapter;
        }

        public void RefreshEditAdapter() => _editAdapter?.Bind(GambleType);

        public bool ShouldSerialize_uialt() => false;
        public bool ShouldSerialize__uialt_aug() => false;
        public bool ShouldSerialize__uichaos() => false;
        public bool ShouldSerialize__uichromatic() => false;
        public bool ShouldSerialize__uieldritch() => false;
        public bool ShouldSerialize__uiesscence() => false;
        public bool ShouldSerialize__uiharvest() => false;
        public bool ShouldSerialize__uimap() => false;
        public bool ShouldSerialize__uimapT17() => false;
    }
}
