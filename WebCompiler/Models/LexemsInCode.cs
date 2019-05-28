namespace WebCompiler.Models
{
    public class LexemeInCode
    {
        public int LineNumber { get; set; }
        public string SubString { get; set; }
        public string Token { get; set; }
        public string Index { get; set; }

        public override string ToString()
        {
            return $"{SubString} ({Token})";
        }
    }
}
