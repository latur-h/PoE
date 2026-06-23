namespace PoE.dlls.Updates
{
    internal sealed record GitHubReleaseInfo(
        string TagName,
        Version Version,
        string AssetName,
        string DownloadUrl);
}
