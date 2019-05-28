namespace WebCompiler.Models
{
    public class LexicalError
    {
        public int Line { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return $"{Line} {Text}";
        }
    }
}
