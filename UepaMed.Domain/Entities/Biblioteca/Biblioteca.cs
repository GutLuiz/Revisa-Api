using UepaMed.Domain.Entities.Usuarios;

namespace UepaMed.Domain.Entities.Bibliotecas
{
    public class Biblioteca
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; } = null!;

        public string Titulo { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public DateTime DataCriacao { get; set; }

        public ICollection<ArquivoImportacaoBiblioteca> Importacoes { get; set; }
            = new List<ArquivoImportacaoBiblioteca>();
    }
}