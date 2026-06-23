using Newtonsoft.Json.Linq;

namespace PoE.dlls.Updates
{
    internal static class GitHubReleaseClient
    {
        public const string Repository = "latur-h/PoE";
        private const string LatestReleaseUrl = $"https://api.github.com/repos/{Repository}/releases/latest";
        private const string AssetSuffix = "-win-x64.zip";

        public static async Task<GitHubReleaseInfo?> GetLatestReleaseAsync(CancellationToken cancellationToken = default)
        {
            using var client = CreateClient();
            string json = await client.GetStringAsync(LatestReleaseUrl, cancellationToken).ConfigureAwait(false);

            var release = JObject.Parse(json);
            string? tagName = release["tag_name"]?.Value<string>();
            Version? version = AppVersion.ParseReleaseTag(tagName);
            if (version is null)
                return null;

            if (release["assets"] is not JArray assets)
                return null;

            foreach (JToken asset in assets)
            {
                string? name = asset["name"]?.Value<string>();
                string? downloadUrl = asset["browser_download_url"]?.Value<string>();
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(downloadUrl))
                    continue;

                if (!name.StartsWith("PoE-", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!name.EndsWith(AssetSuffix, StringComparison.OrdinalIgnoreCase))
                    continue;

                return new GitHubReleaseInfo(tagName!, version, name, downloadUrl);
            }

            return null;
        }

        public static HttpClient CreateClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5),
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("PoE-App-Updater");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
            return client;
        }
    }
}
