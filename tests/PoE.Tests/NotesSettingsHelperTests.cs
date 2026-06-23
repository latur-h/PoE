using PoE.dlls.Settings.Notes;
using Xunit;

namespace PoE.Tests;

public class NotesSettingsHelperTests
{
        [Fact]
        public void EnsureInitialized_CreatesDefaultProfile()
        {
            var settings = new NotesSettings();

            NotesSettingsHelper.EnsureInitialized(settings);

            Assert.Single(settings.Profiles);
            Assert.Equal(NotesSettings.DefaultProfileName, settings.ActiveProfileName);
            Assert.Equal(NotesSettings.DefaultProfileName, settings.Profiles[0].Name);
        }

        [Fact]
        public void IsProfileNameAvailable_RejectsDuplicates()
        {
            var settings = new NotesSettings();
            NotesSettingsHelper.EnsureInitialized(settings);
            settings.Profiles.Add(new NotesProfile { Name = "Build A" });

            Assert.False(NotesSettingsHelper.IsProfileNameAvailable("Build A", settings.Profiles));
            Assert.True(NotesSettingsHelper.IsProfileNameAvailable("Build B", settings.Profiles));
        }

        [Fact]
        public void NormalizeProfiles_EnforcesMaxCount()
        {
            var settings = new NotesSettings
            {
                Profiles = Enumerable.Range(1, 105)
                    .Select(i => new NotesProfile { Name = $"Profile {i}" })
                    .ToList(),
            };

            NotesSettingsHelper.EnsureInitialized(settings);

            Assert.Equal(NotesSettingsHelper.MaxProfiles, settings.Profiles.Count);
        }

        [Fact]
        public void CanRemoveProfile_RequiresAtLeastOneProfile()
        {
            var settings = new NotesSettings();
            NotesSettingsHelper.EnsureInitialized(settings);

            Assert.False(NotesSettingsHelper.CanRemoveProfile(settings, NotesSettings.DefaultProfileName));

            settings.Profiles.Add(new NotesProfile { Name = "Extra" });
            Assert.True(NotesSettingsHelper.CanRemoveProfile(settings, NotesSettings.DefaultProfileName));
    }
}
