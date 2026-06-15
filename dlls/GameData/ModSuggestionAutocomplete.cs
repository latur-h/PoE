namespace PoE.dlls.GameData
{
    public static class ModSuggestionAutocomplete
    {
        public static void Attach(TextBox textBox, ModSuggestionService service)
        {
            var source = new AutoCompleteStringCollection();
            textBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            textBox.AutoCompleteCustomSource = source;

            textBox.TextChanged += (_, _) => RefreshSuggestions(textBox, service, source);
            textBox.GotFocus += (_, _) => RefreshSuggestions(textBox, service, source);
        }

        private static void RefreshSuggestions(TextBox textBox, ModSuggestionService service, AutoCompleteStringCollection source)
        {
            if (!service.IsReady)
            {
                source.Clear();
                return;
            }

            string prefix = GetSearchPrefix(textBox);
            if (prefix.Length < 2)
            {
                source.Clear();
                return;
            }

            source.Clear();
            foreach (string suggestion in service.Search(prefix))
                source.Add(suggestion);
        }

        private static string GetSearchPrefix(TextBox textBox)
        {
            string text = textBox.Text;
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            int caret = textBox.SelectionStart;
            if (caret < 0 || caret > text.Length)
                caret = text.Length;

            int start = caret;
            while (start > 0 && !char.IsWhiteSpace(text[start - 1]))
                start--;

            return text[start..caret].Trim();
        }
    }
}
