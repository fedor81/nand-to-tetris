using System;

namespace JackCompiling
{
    public partial class CodeWriter
    {
        /// <summary>
        /// "string constant"
        /// </summary>
        private bool TryWriteStringValue(TermSyntax term)
        {
            if (term is not ValueTermSyntax valueTerm || valueTerm.Value.TokenType is not TokenType.StringConstant)
                return false;

            var stringConstant = valueTerm.Value.Value;

            resultVmCode.Add($"push constant {stringConstant.Length}");
            resultVmCode.Add("call String.new 1");

            foreach (var c in stringConstant)
            {
                resultVmCode.Add($"push constant {(int)c}");
                resultVmCode.Add("call String.appendChar 2");
            }

            return true;
        }

        /// <summary>
        /// arr[index]
        /// </summary>
        private bool TryWriteArrayAccess(TermSyntax term)
        {
            if (term is not ValueTermSyntax valueTerm || valueTerm.Indexing is not Indexing indexing)
                return false;

            var varInfo = FindVarInfo(valueTerm.Value.Value) ??
                throw new Exception($"Variable {valueTerm.Value.Value} not found");

            // Находим адрес ячейки в которой храниться нужный элемент
            resultVmCode.Add($"push {varInfo.SegmentName} {varInfo.Index}");
            WriteExpression(indexing.Index);
            resultVmCode.Add("add");

            // Сохраняем на стек
            resultVmCode.Add("pop pointer 1");
            resultVmCode.Add("push that 0");

            return true;
        }

        /// <summary>
        /// let arr[index] = expr;
        /// </summary>
        private bool TryWriteArrayAssignmentStatement(StatementSyntax statement)
        {
            if (statement is not LetStatementSyntax letStatement || letStatement.Index is not Indexing indexing)
                return false;

            var varInfo = FindVarInfo(letStatement.VarName.Value) ??
                throw new Exception($"Variable {letStatement.VarName.Value} not found");

            // Находим адрес ячейки в которой храниться нужный элемент
            resultVmCode.Add($"push {varInfo.SegmentName} {varInfo.Index}");
            WriteExpression(indexing.Index);
            resultVmCode.Add("add");

            // Сохраняем значение выражения
            WriteExpression(letStatement.Value);
            resultVmCode.Add("pop temp 0");

            // Записываем значение в по адресу, который мы посчитали выше
            resultVmCode.Add("pop pointer 1");
            resultVmCode.Add("push temp 0");
            resultVmCode.Add("pop that 0");

            return true;
        }
    }
}
