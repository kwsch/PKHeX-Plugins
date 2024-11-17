using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PKHeX.Core.AutoMod;

public static class InternalNetUtil
{
    /// <summary>
    /// Gets the html string from the requested <see cref="address"/>.
    /// </summary>
    /// <param name="address">Address to fetch from</param>
    /// <returns>Page response</returns>
    public static string GetPageText(string address)
    {
        var stream = GetStreamFromURL(address);
        return GetStringResponse(stream);
    }

    private static Stream GetStreamFromURL(string url)
    {
        using var client = new HttpClient();
        const string agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";
        client.DefaultRequestHeaders.Add("User-Agent", agent);
        var response = client.GetAsync(url).Result;
        return response.Content.ReadAsStreamAsync().Result;
    }

    /// <summary>
    /// Downloads a string response with hard-coded address parameters.
    /// </summary>
    /// <param name="address">Address to fetch from</param>
    /// <returns>Page response</returns>
    public static string DownloadString(string address)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "PKHeX-Auto-Legality-Mod");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        var response = client.GetAsync(address).Result;
        var stream = response.Content.ReadAsStreamAsync().Result;
        return GetStringResponse(stream);
    }

    private static string GetStringResponse(Stream? dataStream)
    {
        if (dataStream == null)
            return string.Empty;

        using var reader = new StreamReader(dataStream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// GPSS upload function. POST request using multipart form-data
    /// </summary>
    /// <param name="data">pkm data in bytes.</param>
    /// <param name="generation">The generation for the game the Pokémon is being uploaded from.</param>
    /// <param name="Url">location to fetch from</param>
    public static async Task<HttpResponseMessage> GPSSPost(byte[] data, byte generation, string Url = "flagbrew.org")
    {
        using var client = new HttpClient();

        var uploadData = new MultipartFormDataContent
        {
            { new ByteArrayContent(data), "pkmn", "pkmn" },
        };

        uploadData.Headers.Add("source", "PKHeX AutoMod Plugins");
        uploadData.Headers.Add("generation", generation.ToString());

        var response = await client.PostAsync($"https://{Url}/api/v2/gpss/upload/Pokémon", uploadData);
        return response;
    }

    /// <summary>
    /// GPSS downloader
    /// </summary>
    /// <param name="code">url long</param>
    /// <param name="Url">location to fetch from</param>
    /// <returns>byte array corresponding to a pkm</returns>
    public static byte[]? GPSSDownload(long code, string Url = "flagbrew.org")
    {
        // code is returned as a long
        var json = DownloadString($"https://{Url}/api/v2/gpss/download/Pokémon/{code}");
        if (!json.Contains("\"Pokémon\":\""))
            return null;

        var b64 = json.Split("\"Pokémon\":\"")[1].Split("\"")[0];
        return System.Convert.FromBase64String(b64);
    }
}