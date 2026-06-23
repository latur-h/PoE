namespace PoE.dlls.Settings.Notes
{
    public sealed class NotesSettings
    {
        public const string DefaultProfileName = "Default";

        public string ActiveProfileName { get; set; } = DefaultProfileName;

        public List<NotesProfile> Profiles { get; set; } = [];
    }
}
