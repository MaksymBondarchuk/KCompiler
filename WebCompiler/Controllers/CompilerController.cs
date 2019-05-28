using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebCompiler.Managers;
using WebCompiler.Models;

namespace WebCompiler.Controllers
{
    public class Dto
    {
        public string Text { get; set; }
    }
    
    [Route("api/[controller]")]
    [ApiController]
    public class CompilerController : ControllerBase
    {
        private readonly ICompilerManager _manager;
        
        public CompilerController(ICompilerManager manager)
        {
            _manager = manager;
        }

        [HttpPost]
        public ActionResult<Result> Post([FromBody] Dto dto)
        {
            if (dto != null && !string.IsNullOrEmpty(dto.Text))
            {
                string add = dto.Text.LastOrDefault().Equals("\n") ? " " : "\n";
                OuterLexemes lex = _manager.LexicalAnalyzer(dto.Text + add);
                SyntaxResult syn = _manager.SyntaxAnalyzer(lex);
                return new Result
                    {
                        OuterLexemes = lex,
                        SyntaxResult = syn
                    };
            }

            return BadRequest();
        }
    }
}
