using WebCompiler.Models;

namespace WebCompiler.Managers
{
	public interface IExecutionManager
	{
		ExecutionResult Run(PolishResult polishResult, ExecutionPoint executionPoint = default, decimal input = default);
	}
}