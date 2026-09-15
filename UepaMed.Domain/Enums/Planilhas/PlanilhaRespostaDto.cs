using System;
using System.Collections.Generic;
using System.Text;

namespace UepaMed.Domain.Enums.Planilhas
{
    public class PlanilhaRespostaDto
    {
        public int Id { get; set; }

        public int RevisaoId { get; set; }

        public List<PlanilhaColunaRespostaDto> Colunas { get; set; } = new();

        public List<PlanilhaLinhaRespostaDto> Linhas { get; set; } = new();
    }
}
