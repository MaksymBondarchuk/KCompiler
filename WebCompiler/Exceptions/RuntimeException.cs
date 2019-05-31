namespace WebCompiler.Exceptions
{
	public class RuntimeException: System.Exception
	{
		public override string Message { get; }

		public RuntimeException(string message)
		{
			Message = message;
		}
	}
}