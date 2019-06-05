using System;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
		private readonly IMemoryCache _cache;

		private readonly ICompilerManager _manager;
		private readonly IPolishManager _polishManager;
		private readonly IExecutionManager _executionManager;

		public CompilerController(
			IMemoryCache cache,
			ICompilerManager manager,
			IPolishManager polishManager,
			IExecutionManager executionManager)
		{
			_cache = cache;
			_manager = manager;
			_polishManager = polishManager;
			_executionManager = executionManager;
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
					SyntaxResult = syn,
					ReferenceNumber = Guid.NewGuid()
				};
			}

			PolishResult polishResult = _polishManager.Run(lex);
			
			Guid referenceNumber = Guid.NewGuid();
			_cache.Set(referenceNumber, new ExecutionPoint {PolishResult = polishResult});

			return new Result
			{
				OuterLexemes = lex,
				SyntaxResult = syn,
				// todo Use AutoMapper
				PolishResult = new PolishResultDto
				{
					ReversePolishNotation = string.Join(" ", polishResult.ReversePolishNotation.Select(pn => pn.Token)),
					Trace = polishResult.Trace.Select(t => new PolishTraceDto
					{
						Input = t.Input,
						Stack = string.Join(" | ", t.Stack.Select(pn => pn.Token)),
						ReversePolishNotation = string.Join(" ", t.ReversePolishNotation.Select(pn => pn.Token))
					})
				},
				ReferenceNumber = referenceNumber
			};
		}

		[HttpPost("{referenceNumber:guid}:execute")]
		public ActionResult<ExecutionResultDto> Execute(Guid referenceNumber, [FromQuery] decimal? input)
		{
			var executionPoint = _cache.Get<ExecutionPoint>(referenceNumber);
			
			ExecutionResult result = executionPoint.PolishNotationIndex != 0 
				? input.HasValue 
					? _executionManager.Run(executionPoint.PolishResult, executionPoint, input.Value)
					: _executionManager.Run(executionPoint.PolishResult, executionPoint)
				: _executionManager.Run(executionPoint.PolishResult);

			_cache.Set(referenceNumber, result.ExecutionPoint);
			
			return new ExecutionResultDto
			{
				Output = result.Output,
				Type = result.Type
			};
		}
	}
}