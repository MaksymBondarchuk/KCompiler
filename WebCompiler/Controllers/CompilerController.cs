using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly IPolishManager _polishManager;
        
        public CompilerController(ICompilerManager manager, IPolishManager polishManager)
        {
            _manager = manager;
            _polishManager = polishManager;
        }

        [HttpPost]
        public ActionResult<Result> Post([FromBody] Dto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Text))
            {
                return BadRequest();
            }

            string add = dto.Text.Last().Equals('\n') ? " " : "\n";
            OuterLexemes lex = _manager.LexicalAnalyzer(dto.Text + add);
            SyntaxResult syn = _manager.SyntaxAnalyzer(lex);

            if (syn.Success)
            {
                _polishManager.Run(lex);
                List<PolishNotation> polishNotation = _polishManager.ReversePolishNotation;
                Debugger.Break();
            }

            return new Result
            {
                OuterLexemes = lex,
                SyntaxResult = syn
            };

        }
    }
}
