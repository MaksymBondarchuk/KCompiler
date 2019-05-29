namespace WebCompiler.Models
{
	public class PolishNotation
	{
		public string Token { get; set; }
		
		public PolishNotationTokenType Type { get; set; }

		public override string ToString()
		{
			return $"{Token} ({Type.ToString()})";
		}
	}
}