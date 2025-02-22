using RDManipulacaoAPI.Models;
using System.Text.Json;

namespace RDManipulacaoAPI.Services
{
    public class YouTubeService : IYouTubeService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public YouTubeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = Environment.GetEnvironmentVariable("YOUTUBE_API_KEY")
                      ?? throw new Exception("Variável de ambiente YOUTUBE_API_KEY não definida.");
        }

        public async Task<List<Video>> GetVideosAsync()
        {
            var videos = new List<Video>();

            string pesquisaUrl = $"https://www.googleapis.com/youtube/v3/search" +
                               $"?part=snippet" +
                               $"&maxResults=10" +
                               $"&type=video" +
                               $"&q=manipulação%20medicamentos" +
                               $"&regionCode=BR" +
                               $"&publishedAfter=2025-01-01T00:00:00Z" +
                               $"&publishedBefore=2026-01-01T00:00:00Z" +
                               $"&key={_apiKey}";

            var respostaPesquisa = await _httpClient.GetAsync(pesquisaUrl);
            if (!respostaPesquisa.IsSuccessStatusCode)
            {
                throw new Exception("Erro ao buscar vídeos do YouTube. Status: " + respostaPesquisa.StatusCode);
            }

            var pesquisaJson = await respostaPesquisa.Content.ReadAsStringAsync();
            using JsonDocument searchDoc = JsonDocument.Parse(pesquisaJson);
            var root = searchDoc.RootElement;

            if (root.TryGetProperty("items", out JsonElement items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    var snippet = item.GetProperty("snippet");
                    string videoId = item.GetProperty("id").GetProperty("videoId").GetString();

                    string detalheUrl = $"https://www.googleapis.com/youtube/v3/videos" +
                                        $"?part=contentDetails" +
                                        $"&id={videoId}" +
                                        $"&key={_apiKey}";

                    var detalheResposta = await _httpClient.GetAsync(detalheUrl);
                    string duracao = "";
                    if (detalheResposta.IsSuccessStatusCode)
                    {
                        var detalheJson = await detalheResposta.Content.ReadAsStringAsync();
                        using JsonDocument dadosDoc = JsonDocument.Parse(detalheJson);
                        var detalheRoot = dadosDoc.RootElement;
                        if (detalheRoot.TryGetProperty("items", out JsonElement dadosItem))
                        {
                            foreach (var dadoItem in dadosItem.EnumerateArray())
                            {
                                var contentDetails = dadoItem.GetProperty("contentDetails");
                                duracao = contentDetails.GetProperty("duration").GetString();
                                break;
                            }
                        }
                    }

                    var video = new Video
                    {
                        Titulo = snippet.GetProperty("title").GetString(),
                        Descricao = snippet.GetProperty("description").GetString(),
                        NomeCanal = snippet.GetProperty("channelTitle").GetString(),
                        DataPublicacao = snippet.GetProperty("publishedAt").GetDateTime(),
                        Duracao = duracao,
                        Autor = snippet.GetProperty("channelTitle").GetString()
                    };

                    videos.Add(video);
                }
            }

            return videos;
        }
    }
}
