using System;
using System.Collections.Generic;
using System.Text;

namespace UepaMed.Domain.Enums.Planilhas
{
    public class PlanilhaLinhaRespostaDto
    {
        public int Id { get; set; }

        public int? ArtigoId { get; set; }

        public int Ordem { get; set; }

        public List<PlanilhaCelulaRespostaDto> Celulas { get; set; } = new();
    }
}
