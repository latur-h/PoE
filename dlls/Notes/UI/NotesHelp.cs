namespace PoE.dlls.Notes.UI
{
    internal static class NotesHelp
    {
        public static class Short
        {
            public const string OpenHelp =
                "Open notes help with markdown syntax and examples.";

            public const string Profiles =
                "Switch between saved note profiles (up to 100). Each profile stores its own markdown. Use + / − to add or remove profiles; the default profile cannot be removed.";

            public const string AddProfile =
                "Create a new note profile with a unique name.";

            public const string RemoveProfile =
                "Delete the selected profile. The default profile cannot be removed.";

            public const string Editor =
                "Preview rendered markdown. Double-click to edit; Esc or click outside to save. Selecting text and copying uses plain text (no HTML).";

            public const string CopyChips =
                "In markdown source, [[copy] your text] renders as a clickable chip that copies plain text to the clipboard.";
        }

        public static IEnumerable<(string Heading, string Body)> Sections()
        {
            yield return ("Profiles",
                "Notes are grouped into profiles — useful for different builds, leagues, or characters.\r\n\r\n" +
                "• The default profile is always present.\r\n" +
                "• Use + to add a profile (up to 100 total).\r\n" +
                "• Use − to remove the selected profile.\r\n" +
                "• All profiles are saved automatically in userSettings.json.");

            yield return ("Editing",
                "The note area shows a live markdown preview.\r\n\r\n" +
                "• Double-click the preview to switch to edit mode.\r\n" +
                "• Press Esc or click outside the editor to save and return to preview.\r\n" +
                "• Tab inserts a tab character in edit mode.\r\n" +
                "• Switching tabs or closing the app commits the active profile.");

            yield return ("Copy from preview",
                "When you select text in the preview and copy (Ctrl+C), the clipboard receives plain text only — no HTML tags.\r\n\r\n" +
                "Copy chips (below) are the exception: clicking a chip copies its stored text directly.");

            yield return ("Copy chips",
                "Add a one-click copy button inline using this syntax in markdown source:\r\n\r\n" +
                "[[copy] text to copy]\r\n\r\n" +
                "• The label inside the brackets is shown on the chip and copied to the clipboard when clicked.\r\n" +
                "• Whitespace after [[copy] is optional.\r\n" +
                "• Useful for trade whispers, build paste strings, or repeated commands.\r\n\r\n" +
                "Example in source:\r\n" +
                "[[copy] /w PlayerName Hi, I'd like to buy your Item listed for 10 chaos]]");

            yield return ("Markdown basics",
                "Notes support standard markdown plus common extensions (tables, task lists, strikethrough, etc.).\r\n\r\n" +
                "Headings:\r\n" +
                "# Title\r\n" +
                "## Section\r\n" +
                "### Subsection\r\n\r\n" +
                "Emphasis:\r\n" +
                "**bold** and *italic*\r\n\r\n" +
                "Lists:\r\n" +
                "- bullet item\r\n" +
                "1. numbered item\r\n\r\n" +
                "Links and code:\r\n" +
                "[PoE Wiki](https://www.poewiki.net/)\r\n" +
                "`inline code`\r\n\r\n" +
                "Code block:\r\n" +
                "```\r\n" +
                "q80r60ps25\r\n" +
                "```\r\n\r\n" +
                "Blockquote:\r\n" +
                "> Remember to re-register empty slots after moving the grid.");

            yield return ("Example note",
                "Paste this into a profile to try the features:\r\n\r\n" +
                "# Atlas notes\r\n\r\n" +
                "## Map goals\r\n" +
                "- **Quantity** and **Rarity** on rare maps\r\n" +
                "- Avoid `reflect` and `cannot regenerate`\r\n\r\n" +
                "## Trade whisper\r\n" +
                "[[copy] /w PlayerName Hi, I'd like to buy your Item Name listed for 10 chaos in Standard]]\r\n\r\n" +
                "## Links\r\n" +
                "[PoE Wiki — Map modifiers](https://www.poewiki.net/wiki/Map_modifiers)\r\n\r\n" +
                "---\r\n\r\n" +
                "> Double-click anywhere here to edit this note.");
        }
    }
}
