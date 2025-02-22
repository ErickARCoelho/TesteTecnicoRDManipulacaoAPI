using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RDManipulacaoAPI.Data;
using RDManipulacaoAPI.Models;
using RDManipulacaoAPI.Services;

namespace RDManipulacaoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IYouTubeService _youTubeService;

        public VideosController(AppDbContext context, IYouTubeService youTubeService)
        {
            _context = context;
            _youTubeService = youTubeService;
        }

        /// <summary>
        /// Retorna uma lista de vídeos filtrados conforme os parâmetros informados.
        /// </summary>
        /// <param name="titulo">Filtra vídeos cujo título contenha o texto informado.</param>
        /// <param name="duracao">Filtra vídeos que possuam a duração informada.</param>
        /// <param name="autor">Filtra vídeos pelo nome do autor/canal.</param>
        /// <param name="dataPublicacao">Retorna vídeos publicados após a data informada.</param>
        /// <param name="q">Busca geral que filtra em título, descrição e nome do canal.</param>
        /// <returns>Lista de vídeos que atendem aos critérios de pesquisa.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Video>>> GetVideos(
            [FromQuery] string? titulo,
            [FromQuery] string? duracao,
            [FromQuery] string? autor,
            [FromQuery] DateTime? dataPublicacao,
            [FromQuery] string? q)
        {
            var query = _context.Videos.AsQueryable().Where(v => !v.Excluido);

            if (!string.IsNullOrEmpty(titulo))
                query = query.Where(v => v.Titulo.Contains(titulo));

            if (!string.IsNullOrEmpty(duracao))
                query = query.Where(v => v.Duracao.Contains(duracao));

            if (!string.IsNullOrEmpty(autor))
                query = query.Where(v => v.Autor.Contains(autor));

            if (dataPublicacao.HasValue)
                query = query.Where(v => v.DataPublicacao > dataPublicacao.Value);

            if (!string.IsNullOrEmpty(q))
                query = query.Where(v => v.Titulo.Contains(q) || v.Descricao.Contains(q) || v.NomeCanal.Contains(q));

            return await query.ToListAsync();
        }

        /// <summary>
        /// Consome a API do YouTube para buscar vídeos com critérios pré-definidos e os insere no banco de dados.
        /// </summary>
        /// <remarks>
        /// Esse endpoint realiza duas requisições:
        /// 1. Uma para buscar os vídeos relacionados à manipulação de medicamentos, filtrando por região e período.
        /// 2. Outra para obter detalhes dos vídeos, como a duração.
        /// </remarks>
        /// <returns>Mensagem informando quantos vídeos foram inseridos ou um erro caso nenhum vídeo seja encontrado.</returns>
        [Authorize]
        [HttpPost("fetch")]
        public async Task<ActionResult> FetchVideos()
        {
            var videos = await _youTubeService.GetVideosAsync();
            if (videos == null || videos.Count == 0)
            {
                return NotFound("Nenhum vídeo encontrado a partir da API do YouTube.");
            }

            _context.Videos.AddRange(videos);
            await _context.SaveChangesAsync();

            return Ok($"{videos.Count} vídeos foram adicionados ao banco de dados.");
        }

        /// <summary>
        /// Insere um novo vídeo manualmente no banco de dados.
        /// </summary>
        /// <param name="video">Objeto do tipo Video contendo os dados a serem inseridos.</param>
        /// <returns>Dados do vídeo criado, juntamente com a URI para acesso ao recurso.</returns>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Video>> CreateVideo([FromBody] Video video)
        {
            _context.Videos.Add(video);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVideoById), new { id = video.Id }, video);
        }

        /// <summary>
        /// Retorna os detalhes de um vídeo específico identificado pelo ID.
        /// </summary>
        /// <param name="id">Identificador único do vídeo.</param>
        /// <returns>Dados completos do vídeo, se encontrado e não excluído.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Video>> GetVideoById(int id)
        {
            var video = await _context.Videos.FindAsync(id);
            if (video == null || video.Excluido)
                return NotFound();

            return video;
        }

        /// <summary>
        /// Atualiza os dados de um vídeo existente.
        /// </summary>
        /// <param name="id">Identificador único do vídeo a ser atualizado.</param>
        /// <param name="updatedVideo">Objeto com os dados atualizados do vídeo.</param>
        /// <returns>Status 204 (No Content) se a atualização for realizada com sucesso.</returns>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVideo(int id, [FromBody] Video updatedVideo)
        {
            if (id != updatedVideo.Id)
                return BadRequest("ID do vídeo não corresponde.");

            var video = await _context.Videos.FindAsync(id);
            if (video == null || video.Excluido)
                return NotFound();

            video.Titulo = updatedVideo.Titulo;
            video.Duracao = updatedVideo.Duracao;
            video.Autor = updatedVideo.Autor;
            video.DataPublicacao = updatedVideo.DataPublicacao;
            video.Descricao = updatedVideo.Descricao;
            video.NomeCanal = updatedVideo.NomeCanal;

            _context.Entry(video).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VideoExists(id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
        }

        /// <summary>
        /// Realiza a exclusão lógica de um vídeo, marcando-o como excluído sem removê-lo fisicamente do banco de dados.
        /// </summary>
        /// <param name="id">Identificador único do vídeo a ser excluído.</param>
        /// <returns>Status 204 (No Content) se a exclusão for realizada com sucesso.</returns>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVideo(int id)
        {
            var video = await _context.Videos.FindAsync(id);
            if (video == null || video.Excluido)
                return NotFound();

            video.Excluido = true;
            _context.Entry(video).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Verifica se um vídeo existe e não está marcado como excluído.
        /// </summary>
        /// <param name="id">Identificador do vídeo.</param>
        /// <returns>Verdadeiro se o vídeo existir e estiver ativo; caso contrário, falso.</returns>
        private bool VideoExists(int id)
        {
            return _context.Videos.Any(e => e.Id == id && !e.Excluido);
        }
    }
}
