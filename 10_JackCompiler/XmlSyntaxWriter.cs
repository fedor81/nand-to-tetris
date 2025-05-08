using System;
using System.Xml.Linq;

namespace JackCompiling
{
    public class XmlSyntaxWriter : SyntaxWriter
    {
        private int indentation;

        protected override void Write(Token token)
        {
            var tokenType = token.TokenType.ToString();
            var tagName = char.ToLower(tokenType[0]) + tokenType.Substring(1);
            var escapedValue = new XText(token.Value).ToString();
            WriteLine($"{Indentation}<{tagName}> {escapedValue} </{tagName}>");
        }

        private string Indentation => new string(' ', indentation * 2);

        protected override void CloseContainerTag(Type type)
        {
            indentation--;
            WriteLine($"{Indentation}</{GetTagName(type)}>");
        }

        protected override void OpenContainerTag(Type type)
        {
            WriteLine($"{Indentation}<{GetTagName(type)}>");
            indentation++;
        }
    }
}
