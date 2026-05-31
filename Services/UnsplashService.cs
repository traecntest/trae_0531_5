using Newtonsoft.Json.Linq;
using WallpaperApp.Models;

namespace WallpaperApp.Services;

public interface IUnsplashService
{
    Task<WallpaperSearchResult> SearchWallpapersAsync(string query, int page, int perPage);
    Task<WallpaperSearchResult> GetPopularWallpapersAsync(int page, int perPage);
    Task<Wallpaper?> GetWallpaperByIdAsync(string id);
    Task<string> GetDownloadUrlAsync(string id);
}

public class UnsplashService : IUnsplashService
{
    private readonly HttpClient _httpClient;
    private readonly string _accessKey;

    public UnsplashService(IConfiguration configuration, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _accessKey = configuration["Unsplash:AccessKey"] ?? string.Empty;
        var baseUrl = configuration["Unsplash:BaseUrl"] ?? "https://api.unsplash.com";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<WallpaperSearchResult> SearchWallpapersAsync(string query, int page, int perPage)
    {
        if (string.IsNullOrEmpty(_accessKey) || _accessKey == "YOUR_UNSPLASH_ACCESS_KEY")
        {
            return GetMockWallpapers(page, perPage, query);
        }

        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Client-ID", _accessKey);

        var response = await _httpClient.GetAsync($"/search/photos?query={Uri.EscapeDataString(query)}&page={page}&per_page={perPage}&orientation=landscape");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(content);

        return ParseSearchResult(json);
    }

    public async Task<WallpaperSearchResult> GetPopularWallpapersAsync(int page, int perPage)
    {
        if (string.IsNullOrEmpty(_accessKey) || _accessKey == "YOUR_UNSPLASH_ACCESS_KEY")
        {
            return GetMockWallpapers(page, perPage);
        }

        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Client-ID", _accessKey);

        var response = await _httpClient.GetAsync($"/photos?page={page}&per_page={perPage}&order_by=popular");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var jsonArray = JArray.Parse(content);

        return new WallpaperSearchResult
        {
            Results = jsonArray.Select(ParseWallpaper).ToList(),
            Total = 1000,
            TotalPages = 100
        };
    }

    public async Task<Wallpaper?> GetWallpaperByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(_accessKey) || _accessKey == "YOUR_UNSPLASH_ACCESS_KEY")
        {
            var mock = GetMockWallpapers(1, 1);
            return mock.Results.FirstOrDefault();
        }

        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Client-ID", _accessKey);

        var response = await _httpClient.GetAsync($"/photos/{id}");
        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(content);

        return ParseWallpaper(json);
    }

    public async Task<string> GetDownloadUrlAsync(string id)
    {
        if (string.IsNullOrEmpty(_accessKey) || _accessKey == "YOUR_UNSPLASH_ACCESS_KEY")
        {
            return $"https://picsum.photos/1920/1080?random={id}";
        }

        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Client-ID", _accessKey);

        var response = await _httpClient.GetAsync($"/photos/{id}/download");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(content);

        return json["url"]?.ToString() ?? string.Empty;
    }

    private WallpaperSearchResult ParseSearchResult(JObject json)
    {
        return new WallpaperSearchResult
        {
            Total = json["total"]?.Value<int>() ?? 0,
            TotalPages = json["total_pages"]?.Value<int>() ?? 0,
            Results = (json["results"] as JArray)?.Select(ParseWallpaper).ToList() ?? new List<Wallpaper>()
        };
    }

    private Wallpaper ParseWallpaper(JToken token)
    {
        var urls = token["urls"];
        var user = token["user"];
        var tags = token["tags"] as JArray;

        return new Wallpaper
        {
            Id = token["id"]?.ToString() ?? string.Empty,
            Url = urls?["regular"]?.ToString() ?? string.Empty,
            ThumbUrl = urls?["small"]?.ToString() ?? string.Empty,
            DownloadUrl = urls?["full"]?.ToString() ?? string.Empty,
            Description = token["description"]?.ToString() ?? token["alt_description"]?.ToString() ?? string.Empty,
            Photographer = user?["name"]?.ToString() ?? string.Empty,
            PhotographerUrl = user?["links"]?["html"]?.ToString() ?? string.Empty,
            Width = token["width"]?.Value<int>() ?? 1920,
            Height = token["height"]?.Value<int>() ?? 1080,
            Tags = tags?.Select(t => t["title"]?.ToString() ?? string.Empty).Where(t => !string.IsNullOrEmpty(t)).ToList() ?? new List<string>()
        };
    }

    private WallpaperSearchResult GetMockWallpapers(int page, int perPage, string query = "")
    {
        var wallpapers = new List<Wallpaper>();
        var random = new Random(page * 1000);
        var topics = new[] { "nature", "city", "abstract", "minimal", "landscape", "architecture", "ocean", "forest" };
        var photographers = new[] { "Alice Johnson", "Bob Smith", "Carol Williams", "David Brown", "Emma Davis" };

        for (int i = 0; i < perPage; i++)
        {
            var id = $"{page}-{i}-{random.Next(10000)}";
            var topic = topics[random.Next(topics.Length)];
            wallpapers.Add(new Wallpaper
            {
                Id = id,
                Url = $"https://picsum.photos/seed/{id}/1200/800",
                ThumbUrl = $"https://picsum.photos/seed/{id}/400/300",
                DownloadUrl = $"https://picsum.photos/seed/{id}/1920/1080",
                Description = $"{query ?? topic} wallpaper #{i + 1}",
                Photographer = photographers[random.Next(photographers.Length)],
                PhotographerUrl = "#",
                Width = 1920,
                Height = 1080,
                Tags = new List<string> { topic, query ?? "wallpaper" }.Distinct().ToList()
            });
        }

        return new WallpaperSearchResult
        {
            Results = wallpapers,
            Total = 1000,
            TotalPages = 100
        };
    }
}
