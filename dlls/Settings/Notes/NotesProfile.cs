namespace PoE.dlls.Settings.Notes
{
    public sealed class NotesProfile
    {
        public string Name { get; set; } = NotesSettings.DefaultProfileName;

        public string Markdown { get; set; } = string.Empty;
    }
}
