using Microsoft.AspNetCore.Mvc;
using WallpaperApp.Models;
using WallpaperApp.Services;

namespace WallpaperApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WallpapersController : ControllerBase
{
    private readonly IUnsplashService _unsplashService;

    public WallpapersController(IUnsplashService unsplashService)
    {
        _unsplashService = unsplashService;
    }

    [HttpGet("popular")]
    public async Task<ActionResult<WallpaperSearchResult>> GetPopular(int page = 1, int perPage = 20)
    {
        var result = await _unsplashService.GetPopularWallpapersAsync(page, perPage);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<WallpaperSearchResult>> Search(string query, int page = 1, int perPage = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query parameter is required");
        }

        var result = await _unsplashService.SearchWallpapersAsync(query, page, perPage);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Wallpaper>> GetById(string id)
    {
        var wallpaper = await _unsplashService.GetWallpaperByIdAsync(id);
        if (wallpaper == null)
        {
            return NotFound();
        }
        return Ok(wallpaper);
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(string id, int? width = null, int? height = null)
    {
        var wallpaper = await _unsplashService.GetWallpaperByIdAsync(id);
        if (wallpaper == null)
        {
            return NotFound();
        }

        var downloadUrl = wallpaper.DownloadUrl;
        if (width.HasValue && height.HasValue)
        {
            if (downloadUrl.Contains("picsum.photos"))
            {
                var seed = id;
                downloadUrl = $"https://picsum.photos/seed/{seed}/{width}/{height}";
            }
            else
            {
                downloadUrl += $"&w={width}&h={height}&fit=crop";
            }
        }

        using var httpClient = new HttpClient();
        var imageBytes = await httpClient.GetByteArrayAsync(downloadUrl);
        var fileName = $"wallpaper_{id}_{width ?? 1920}x{height ?? 1080}.jpg";
        return File(imageBytes, "image/jpeg", fileName);
    }

    [HttpGet("{id}/download-url")]
    public async Task<ActionResult<string>> GetDownloadUrl(string id)
    {
        var url = await _unsplashService.GetDownloadUrlAsync(id);
        return Ok(new { url });
    }
}
