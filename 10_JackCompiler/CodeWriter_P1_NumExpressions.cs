using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JackCompiling
{
    public partial class CodeWriter
    {
        private void WriteTerm(TermSyntax term)
        {
            var ok = TryWriteStringValue(term) // будет реализована в следующих задачах
                     || TryWriteArrayAccess(term) // будет реализована в следующих задачах
                     || TryWriteObjectValue(term) // будет реализована в следующих задачах
                     || TryWriteSubroutineCall(term) // будет реализована в следующих задачах
                     || TryWriteNumericTerm(term);
            if (!ok)
                throw new FormatException($"Unknown term [{term}]");
        }

        /// <summary>2+x</summary>
        public void WriteExpression(ExpressionSyntax expression)
        {
            WriteTerm(expression.Term);

            foreach (var term in expression.Tail)
            {
                if (term.Term is ParenthesizedTermSyntax t)
                    WriteExpression(t.Expression);
                else
                    WriteTerm(term.Term);

                WriteOperation(term.Operator);
            }
        }

        private static readonly Dictionary<string, string> OperationMap = new()
        {
            ["+"] = "add",
            ["-"] = "sub",
            ["*"] = "call Math.multiply 2",
            ["/"] = "call Math.divide 2",
            ["&"] = "and",
            ["|"] = "or",
            ["<"] = "lt",
            [">"] = "gt",
            ["="] = "eq"
        };

        private bool WriteOperation(Token operation)
        {
            if (OperationMap.TryGetValue(operation.Value, out var vmCommand))
                resultVmCode.Add(vmCommand);
            else
                return false;
            return true;
        }

        /// <summary>42 | true | false | varName | -x | ( x )</summary>
        private bool TryWriteNumericTerm(TermSyntax term) => term switch
        {
            ValueTermSyntax t => TryWriteValueTerm(t),
            UnaryOpTermSyntax t => TryWriteUnaryOpTerm(t),
            ParenthesizedTermSyntax t => TryWriteParenthesizedTerm(t),
            _ => false,
        };

        private bool TryWriteValueTerm(ValueTermSyntax term)
        {
            if (term.Indexing is not null)
                return false;

            switch (term.Value.TokenType)
            {
                case TokenType.Identifier:
                    return TryWriteIdentifierTerm(term.Value.Value);
                case TokenType.IntegerConstant:
                    resultVmCode.Add($"push constant {term.Value.Value}");
                    return true;
                case TokenType.Keyword:
                    return TryWriteKeywordTerm(term.Value.Value);
                default:
                    return false;
            }
        }

        private bool TryWriteIdentifierTerm(string identifier)
        {
            if (methodSymbols.TryGetValue(identifier, out var varInfo) ||
                classSymbols.TryGetValue(identifier, out varInfo))
            {
                resultVmCode.Add($"push {varInfo.SegmentName} {varInfo.Index}");
                return true;
            }
            return false;
        }

        private bool TryWriteKeywordTerm(string keyword)
        {
            switch (keyword)
            {
                case "true":
                    resultVmCode.Add("push constant -1");
                    return true;
                case "false":
                    resultVmCode.Add("push constant 0");
                    return true;
                default:
                    return false;
            }
        }

        private bool TryWriteUnaryOpTerm(UnaryOpTermSyntax term)
        {
            WriteTerm(term.Term);
            if (term.UnaryOp.Value == "-")
                resultVmCode.Add("neg");
            else if (term.UnaryOp.Value == "~")
                resultVmCode.Add("not");
            else
                return false;
            return true;
        }

        private bool TryWriteParenthesizedTerm(ParenthesizedTermSyntax term)
        {
            WriteExpression(term.Expression);
            return true;
        }
    }
}
