using RDManipulacaoAPI.Models;

namespace RDManipulacaoAPI.Services
{
    public interface IYouTubeService
    {
        /// <summary>
        /// Busca vídeos na API do YouTube com os filtros definidos e retorna uma lista de objetos Video.
        /// </summary>
        /// <returns>Lista de vídeos obtidos da API.</returns>
        Task<List<Video>> GetVideosAsync();
    }
}
