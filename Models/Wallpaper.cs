namespace WallpaperApp.Models;

public class Wallpaper
{
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ThumbUrl { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Photographer { get; set; } = string.Empty;
    public string PhotographerUrl { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class WallpaperSearchResult
{
    public List<Wallpaper> Results { get; set; } = new();
    public int Total { get; set; }
    public int TotalPages { get; set; }
}
