using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JackCompiling
{
    public static class SyntaxExtensions
    {
        public static bool Is(this Token? token, TokenType tokenType, string value)
        {
            return token is { } && token.TokenType == tokenType && token.Value == value;
        }

        public static bool IsOneOf(this Token? token, params string[] values)
        {
            return token is { } && values.Any(value => token.Value == value);
        }
    }
}
