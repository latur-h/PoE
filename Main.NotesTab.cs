using PoE.dlls.Notes.UI;
using PoE.dlls.Style;

namespace PoE
{
    public partial class Main
    {
        private NotesPanel _notesPanel = null!;
        private ToolTip toolTip_Notes = null!;

        private void InitializeNotesTab()
        {
            tabPage_Notes.BackColor = StaticColors.BackGround;
            tabPage_Notes.AutoScroll = false;
            tabPage_Notes.Padding = new Padding(7);

            _notesPanel = new NotesPanel
            {
                Dock = DockStyle.Fill,
            };
            _notesPanel.Bind(_settings.Notes);
            _notesPanel.Changed += (_, _) => _userSettings.SaveSettings();

            tabPage_Notes.Controls.Add(_notesPanel);
            SetupNotesHints();
        }

        private void SetupNotesHints()
        {
            toolTip_Notes = new ToolTip(components);
            SettingsHintHelper.Configure(toolTip_Notes);
            _notesPanel.SetupHints(toolTip_Notes, this);
        }

        private void NotesTab_CommitIfLeaving(TabControlCancelEventArgs e)
        {
            if (tabControl_Main.SelectedTab != tabPage_Notes || e.TabPage == tabPage_Notes)
                return;

            _notesPanel?.Commit();
        }
    }
}
