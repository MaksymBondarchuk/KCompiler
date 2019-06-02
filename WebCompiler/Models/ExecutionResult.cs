using System.Runtime.Serialization;

namespace WebCompiler.Models
{
	public class ExecutionResult
	{
		[DataMember] public ExecutionResultType Type { get; set; }
		
		[DataMember] public string Output { get; set; }
		
		[DataMember] public ExecutionPoint ExecutionPoint { get; set; }
	}
}