using Microsoft.AspNetCore.Http;
using UepaMed.Domain.Enums.Arquivos;

namespace UepaMed.Application.Dtos.Biblioteca
{
    public class ArquivoComBasePesquisaDto
    {
        public IFormFile? Arquivo { get; set; }
        public BasePesquisa? BasePesquisa { get; set; }
    }
}