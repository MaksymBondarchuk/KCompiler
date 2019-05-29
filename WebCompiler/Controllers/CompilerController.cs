using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using WebCompiler.Managers;
using WebCompiler.Models;

namespace WebCompiler.Controllers
{
	public class Dto
	{
		[DataMember] public string Text { get; set; }
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

			if (!syn.Success)
			{
				return new Result
				{
					OuterLexemes = lex,
					SyntaxResult = syn
				};
			}

			_polishManager.Run(lex);
			List<PolishNotation> polishNotation = _polishManager.ReversePolishNotation;
			List<PolishTrace> trace = _polishManager.Trace;

			return new Result
			{
				OuterLexemes = lex,
				SyntaxResult = syn,
				PolishResult = new PolishResult
				{
					ReversePolishNotation = string.Join(" ", polishNotation.Select(pn => pn.Token)),
					Trace = trace.Select(t => new PolishTraceDto
					{
						Input = t.Input,
						Stack = string.Join("\n", t.Stack.Select(pn => pn.Token)),
						ReversePolishNotation = string.Join(" ", t.ReversePolishNotation.Select(pn => pn.Token))
					})
				}
			};
		}
	}
}