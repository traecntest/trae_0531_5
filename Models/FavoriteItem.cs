namespace WallpaperApp.Models;

public class FavoriteItem
{
    public string WallpaperId { get; set; } = string.Empty;
    public Wallpaper Wallpaper { get; set; } = null!;
    public DateTime AddedAt { get; set; }
}
