namespace WebCompiler.Models
{
	public enum PolishNotationTokenType
	{
		Identifier,
		Literal,
		Operator,
		Delimiter,
		If,
		Then,
		Fi,
		While,
		TechnicalDo,	// <loop operator>         ::= do while (<logical expression>) TechnicalDo <operators list> enddo
		Enddo,
		Label
	}
}