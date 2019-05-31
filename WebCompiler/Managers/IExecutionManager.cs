using WebCompiler.Models;

namespace WebCompiler.Managers
{
	public interface IExecutionManager
	{
		string Run(PolishResult polishResult);
	}
}