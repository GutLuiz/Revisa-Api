using UepaMed.Domain.Enums;

namespace UepaMed.Application.Dtos.Artigos
{
    public class DecidirDuplicidadeDto
    {
        public int ArtigoAId { get; set; }

        public int ArtigoBId { get; set; }

        public DecisaoDuplicidade Decisao { get; set; }
    }
}