using System;
using System.Collections.Generic;

namespace JackCompiling
{
    public partial class CodeWriter
    {
        /// <summary>Statement; Statement; ...</summary>
        public void WriteStatements(StatementsSyntax statements)
        {
            foreach (var statement in statements.Statements)
            {
                WriteStatement(statement);
            }
        }

        private void WriteStatement(StatementSyntax statement)
        {
            var ok = TryWriteVarAssignmentStatement(statement)
                     || TryWriteProgramFlowStatement(statement)
                     || TryWriteDoStatement(statement) // будет реализована в следующий задачах
                     || TryWriteArrayAssignmentStatement(statement)  // будет реализована в следующий задачах
                     || TryWriteReturnStatement(statement);  // будет реализована в следующий задачах
            if (!ok)
                throw new FormatException($"Unknown statement [{statement}]");
        }

        /// <summary>let VarName = Expression;</summary>
        private bool TryWriteVarAssignmentStatement(StatementSyntax statement)
        {
            if (statement is not LetStatementSyntax letStatement)
                return false;

            if (classSymbols.TryGetValue(letStatement.VarName.Value, out var varInfo)
                || methodSymbols.TryGetValue(letStatement.VarName.Value, out varInfo))
            {
                WriteExpression(letStatement.Value);
                resultVmCode.Add($"pop {varInfo.SegmentName} {varInfo.Index}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// if ( Expression ) { Statements } [else { Statements }]
        /// while ( Expression ) { Statements }
        /// </summary>
        private bool TryWriteProgramFlowStatement(StatementSyntax statement) => statement switch
        {
            WhileStatementSyntax s => TryWriteWhileStatement(s),
            IfStatementSyntax s => TryWriteIfStatement(s),
            _ => false
        };


        private bool TryWriteWhileStatement(WhileStatementSyntax statement)
        {
            var startLabel = GetUniqueLabel("while_start");
            var endLabel = GetUniqueLabel("while_end");

            resultVmCode.Add($"label {startLabel}");
            WriteExpression(statement.Condition);
            resultVmCode.Add("not");
            resultVmCode.Add($"if-goto {endLabel}");
            WriteStatements(statement.Statements);
            resultVmCode.Add($"goto {startLabel}");
            resultVmCode.Add($"label {endLabel}");
            return true;
        }

        private bool TryWriteIfStatement(IfStatementSyntax statement)
        {
            var trueLabel = GetUniqueLabel("if_true");
            var endLabel = GetUniqueLabel("if_end");

            WriteExpression(statement.Condition);
            resultVmCode.Add($"if-goto {trueLabel}");

            if (statement.ElseClause is { } elseClause)
                WriteStatements(elseClause.FalseStatements);

            resultVmCode.Add($"goto {endLabel}");
            resultVmCode.Add($"label {trueLabel}");
            WriteStatements(statement.TrueStatements);
            resultVmCode.Add($"label {endLabel}");

            return true;
        }

        private uint _labelCounter = 0;
        private string GetUniqueLabel(string label) => $"{label}_{_labelCounter++}";
    }
}
