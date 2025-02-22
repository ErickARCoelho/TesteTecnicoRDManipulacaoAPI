using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RDManipulacaoAPI.Controllers;
using RDManipulacaoAPI.Data;
using RDManipulacaoAPI.Models;
using RDManipulacaoAPI.Services;

namespace RDManipulacaoAPI.NUnitTests
{
    [TestFixture]
    public class VideosControllerTests
    {
        private AppDbContext _context;
        private Mock<IYouTubeService> _mockYouTubeService;
        private VideosController _controller;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);

            _mockYouTubeService = new Mock<IYouTubeService>();
            _mockYouTubeService.Setup(s => s.GetVideosAsync())
                .ReturnsAsync(new List<Video>
                {
                    new() {
                        Id = 1,
                        Titulo = "Vídeo Teste 1",
                        Duracao = "PT5M",
                        Autor = "Autor Teste",
                        DataPublicacao = DateTime.UtcNow,
                        Descricao = "Descrição Teste 1",
                        NomeCanal = "Canal Teste 1",
                        Excluido = false
                    },
                    new() {
                        Id = 2,
                        Titulo = "Vídeo Teste 2",
                        Duracao = "PT3M",
                        Autor = "Outro Autor",
                        DataPublicacao = DateTime.UtcNow,
                        Descricao = "Descrição Teste 2",
                        NomeCanal = "Canal Teste 2",
                        Excluido = false
                    }
                });

            _controller = new VideosController(_context, _mockYouTubeService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        /// <summary>
        /// Testa o endpoint GET sem dados na base, retornando uma lista vazia.
        /// </summary>
        [Test]
        public async Task GetVideos_EmptyDatabase_ReturnsEmptyList()
        {
            var result = await _controller.GetVideos(null, null, null, null, null);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Count(), Is.EqualTo(0));
        }

        /// <summary>
        /// Testa o endpoint POST para criar um novo vídeo.
        /// </summary>
        [Test]
        public async Task CreateVideo_AddsVideoToDatabase()
        {
            var video = new Video
            {
                Titulo = "Novo Vídeo",
                Duracao = "PT4M",
                Autor = "Autor Novo",
                DataPublicacao = DateTime.UtcNow,
                Descricao = "Descrição Novo",
                NomeCanal = "Canal Novo",
                Excluido = false
            };

            var actionResult = await _controller.CreateVideo(video);
            var result = actionResult.Result as CreatedAtActionResult;
            Assert.That(result, Is.Not.Null);
            var createdVideo = result.Value as Video;
            Assert.That(createdVideo, Is.Not.Null);
            Assert.That(createdVideo.Titulo, Is.EqualTo("Novo Vídeo"));

            var dbVideo = await _context.Videos.FirstOrDefaultAsync(v => v.Titulo == "Novo Vídeo");
            Assert.That(dbVideo, Is.Not.Null);
        }

        /// <summary>
        /// Testa o endpoint GET por ID, verificando se o vídeo correto é retornado.
        /// </summary>
        [Test]
        public async Task GetVideoById_ReturnsCorrectVideo()
        {
            var video = new Video
            {
                Titulo = "Vídeo para Buscar",
                Duracao = "PT2M",
                Autor = "Autor Teste",
                DataPublicacao = DateTime.UtcNow,
                Descricao = "Descrição Teste",
                NomeCanal = "Canal Teste",
                Excluido = false
            };
            _context.Videos.Add(video);
            await _context.SaveChangesAsync();

            var result = await _controller.GetVideoById(video.Id);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Titulo, Is.EqualTo("Vídeo para Buscar"));
        }

        /// <summary>
        /// Testa o endpoint GET por ID para um vídeo inexistente.
        /// </summary>
        [Test]
        public async Task GetVideoById_NonExistent_ReturnsNotFound()
        {
            var result = await _controller.GetVideoById(999);
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        /// <summary>
        /// Testa o endpoint PUT para atualizar os dados de um vídeo existente.
        /// </summary>
        [Test]
        public async Task UpdateVideo_ValidData_UpdatesVideo()
        {
            var video = new Video
            {
                Titulo = "Vídeo Antigo",
                Duracao = "PT2M",
                Autor = "Autor A",
                DataPublicacao = DateTime.UtcNow,
                Descricao = "Descrição Antiga",
                NomeCanal = "Canal A",
                Excluido = false
            };
            _context.Videos.Add(video);
            await _context.SaveChangesAsync();

            var updatedVideo = new Video
            {
                Id = video.Id,
                Titulo = "Vídeo Atualizado",
                Duracao = "PT3M",
                Autor = "Autor A",
                DataPublicacao = DateTime.UtcNow,
                Descricao = "Descrição Atualizada",
                NomeCanal = "Canal A",
                Excluido = false
            };

            var result = await _controller.UpdateVideo(video.Id, updatedVideo);
            Assert.That(result, Is.InstanceOf<NoContentResult>());
            var dbVideo = await _context.Videos.FindAsync(video.Id);
            Assert.Multiple(() =>
            {
                Assert.That(dbVideo.Titulo, Is.EqualTo("Vídeo Atualizado"));
                Assert.That(dbVideo.Duracao, Is.EqualTo("PT3M"));
            });
        }

        /// <summary>
        /// Testa o endpoint DELETE para realizar a exclusão lógica de um vídeo.
        /// </summary>
        [Test]
        public async Task DeleteVideo_SetsExcluidoFlag()
        {
            var video = new Video
            {
                Titulo = "Vídeo para Deletar",
                Duracao = "PT2M",
                Autor = "Autor B",
                DataPublicacao = DateTime.UtcNow,
                Descricao = "Descrição B",
                NomeCanal = "Canal B",
                Excluido = false
            };
            _context.Videos.Add(video);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteVideo(video.Id);
            Assert.That(result, Is.InstanceOf<NoContentResult>());
            var dbVideo = await _context.Videos.FindAsync(video.Id);
            Assert.That(dbVideo.Excluido, Is.True);
        }

        /// <summary>
        /// Testa o endpoint FetchVideos, que utiliza o serviço mockado para adicionar vídeos à base.
        /// </summary>
        [Test]
        public async Task FetchVideos_AddsVideosToDatabase()
        {
            _context.Videos.RemoveRange(_context.Videos);
            await _context.SaveChangesAsync();

            var result = await _controller.FetchVideos() as OkObjectResult;
            Assert.That(result, Is.Not.Null);
            var message = result.Value as string;
            Assert.That(message, Does.Contain("2 vídeos"));
            int count = await _context.Videos.CountAsync();
            Assert.That(count, Is.EqualTo(2));
        }

        /// <summary>
        /// Testa o endpoint GET com filtro, verificando se retorna apenas os vídeos que atendem ao critério.
        /// </summary>
        [Test]
        public async Task GetVideos_WithFilter_ReturnsFilteredVideos()
        {
            var videos = new List<Video>
            {
                new() { Titulo = "A", Duracao = "PT1M", Autor = "X", DataPublicacao = DateTime.UtcNow.AddDays(-1), Descricao = "desc A", NomeCanal = "Canal A",     Excluido =  false },
                new() { Titulo = "B", Duracao = "PT2M", Autor = "Y", DataPublicacao = DateTime.UtcNow.AddDays(-2), Descricao = "desc B", NomeCanal = "Canal B",     Excluido =  false },
                new() { Titulo = "C", Duracao = "PT3M", Autor = "Z", DataPublicacao = DateTime.UtcNow.AddDays(-3), Descricao = "desc C", NomeCanal = "Canal C",     Excluido =  false }
            };
            _context.Videos.AddRange(videos);
            await _context.SaveChangesAsync();

            var result = await _controller.GetVideos("B", null, null, null, null);
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.Count(), Is.EqualTo(1));
            Assert.That(result.Value.First().Titulo, Is.EqualTo("B"));
        }

    }
}
