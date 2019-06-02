using System.Runtime.Serialization;

namespace WebCompiler.Models
{
	public class ExecutionResultDto
	{
		[DataMember] public ExecutionResultType Type { get; set; }
		
		[DataMember] public string Output { get; set; }
	}
}