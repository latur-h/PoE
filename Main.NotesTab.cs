using PoE.dlls.Notes.UI;
using PoE.dlls.Style;

namespace PoE
{
    public partial class Main
    {
        private NotesPanel _notesPanel = null!;

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
        }

        private void NotesTab_CommitIfLeaving(TabControlCancelEventArgs e)
        {
            if (tabControl_Main.SelectedTab != tabPage_Notes || e.TabPage == tabPage_Notes)
                return;

            _notesPanel?.Commit();
        }
    }
}
