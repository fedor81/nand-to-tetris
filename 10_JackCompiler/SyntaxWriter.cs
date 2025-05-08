using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace JackCompiling
{
    public abstract class SyntaxWriter
    {
        private readonly List<string> result = new();

        public IReadOnlyList<string> GetResult()
        {
            return result;
        }

        protected abstract void OpenContainerTag(Type type);
        protected abstract void Write(Token token);
        protected abstract void CloseContainerTag(Type type);

        public void Write(object? syntax)
        {
            if (syntax is null) return;
            if (syntax is Token t)
            {
                Write(t);
                return;
            }

            var type = syntax.GetType();
            var shouldCreateSyntaxNode = type.Name.EndsWith("Syntax");
            if (shouldCreateSyntaxNode)
                OpenContainerTag(type);

            var parameters = type.GetConstructors().Single().GetParameters().Select(p => p.Name!);
            foreach (var parameter in parameters)
            {
                var prop = type.GetProperty(parameter)!;
                var value = prop.GetValue(syntax);
                if (value is IList list)
                    WriteList(list, prop.Name.StartsWith("Delimited"));
                else
                    Write(value);
            }

            if (shouldCreateSyntaxNode)
                CloseContainerTag(type);
        }

        protected void WriteLine(string line)
        {
            result.Add(line);
        }

        protected void Append(string text)
        {
            if (result.Count == 0)
                result.Add(text);
            else
                result[^1] += text;
        }

        private void WriteList(IList items, bool delimit)
        {
            for (var index = 0; index < items.Count; index++)
            {
                Write(items[index]);
                if (delimit && index != items.Count - 1)
                    Write(new Token(TokenType.Symbol, ",", 0, 0));
            }
        }

        protected static string GetTagName(Type type)
        {
            var contract = type.GetCustomAttribute<DataContractAttribute>();
            var typeName = contract?.Name ?? type.Name;
            if (typeName.EndsWith("Syntax"))
                typeName = typeName[..^"Syntax".Length];
            return char.ToLower(typeName[0]) + typeName[1..];
        }
    }
}
