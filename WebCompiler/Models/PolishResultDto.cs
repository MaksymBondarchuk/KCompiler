using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WebCompiler.Models
{
	public class PolishResultDto
	{
		[DataMember] public string ReversePolishNotation { get; set; }

		[DataMember] public IEnumerable<PolishTraceDto> Trace { get; set; }
	}
}