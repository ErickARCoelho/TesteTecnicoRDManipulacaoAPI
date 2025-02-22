namespace RDManipulacaoAPI.Models
{
    public class Video
    {
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public string? Duracao { get; set; }
        public string? Autor { get; set; }
        public DateTime DataPublicacao { get; set; }
        public string? Descricao { get; set; }
        public string? NomeCanal { get; set; }
        public bool Excluido { get; set; } 
    }
}
