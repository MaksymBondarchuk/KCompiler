namespace WebCompiler.Exceptions
{
	public class SyntaxException: System.Exception
	{
		public override string Message { get; }

		public SyntaxException(string message)
		{
			Message = message;
		}
	}
}