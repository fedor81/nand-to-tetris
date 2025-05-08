using System;

namespace JackCompiling
{
    [Flags]
    public enum TokenType
    {
        Symbol = 1,
        Keyword = 2,
        IntegerConstant = 4,
        StringConstant = 8,
        Identifier = 16
    }

    public record Token(TokenType TokenType, string Value, int LineNumber, int ColNumber)
    {
        public int IntValue => int.Parse(Value);

        public override string ToString()
        {
            return $"[{TokenType} {Value}] at line {LineNumber}, col {ColNumber}";
        }

        public void Deconstruct(out TokenType tokenType, out string value)
        {
            tokenType = TokenType;
            value = Value;
        }
        
    }
}
