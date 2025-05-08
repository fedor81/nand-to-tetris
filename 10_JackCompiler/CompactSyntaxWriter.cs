using System;

namespace JackCompiling
{
    public class CompactSyntaxWriter : SyntaxWriter
    {
        private bool needSpace;

        protected override void Write(Token token)
        {
            if (needSpace)
                Append(" ");
            Append($"{token.Value}");
            needSpace = true;
        }

        protected override void OpenContainerTag(Type type)
        {
            if (needSpace)
                Append(" ");
            Append($"{GetTagName(type)}[");
            needSpace = false;
        }

        protected override void CloseContainerTag(Type type)
        {
            Append("]");
            needSpace = true;
        }
    }
}
