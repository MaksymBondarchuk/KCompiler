using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
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
        private ICompilerManager _manager;
        public CompilerController(ICompilerManager manager)
        {
            _manager = manager;
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpPost]
        public ActionResult<Result> Post([FromBody] Dto dto)
        {
            if (dto != null && !string.IsNullOrEmpty(dto.Text))
            {
                var add = dto.Text.LastOrDefault().Equals("\n") ? " " : "\n";
                var lex = _manager.LexicalAnalizer(dto.Text + add);
                var syn = _manager.SyntaxAnalizer(lex);
                return new Result
                    {
                        OuterLexemes = lex,
                        SyntaxResult = syn
                    };
            }
            else
            {
                return BadRequest();
            }

        }

        
    }
}
