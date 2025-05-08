using System;
using System.Collections.Generic;

namespace JackCompiling;

public record CompilationError(string Message, int LineNumber);

public class Validator
{
    private readonly List<CompilationError> errors = new();

    public IReadOnlyList<CompilationError> Errors => errors;

    public void ValidateClass(ClassSyntax classSyntax)
    {
        foreach (var subroutine in classSyntax.SubroutineDec)
        {
            ValidateSubroutine(subroutine);
        }
    }

    private void ValidateSubroutine(SubroutineDecSyntax subroutine)
    {
        if (!AllPathsReturnValue(subroutine.SubroutineBody.Statements))
        {
            var lineNumber = subroutine.SubroutineBody.Open.LineNumber;
            errors.Add(new CompilationError("Not all execution paths return a value", lineNumber));
        }
    }

    private bool AllPathsReturnValue(StatementsSyntax statements)
    {
        for (int i = 0; i < statements.Statements.Count; i++)
        {
            var statement = statements.Statements[i];

            if (statement is ReturnStatementSyntax)
                return true;

            if (statement is IfStatementSyntax ifStatement && i == statements.Statements.Count - 1)
                return AllPathsReturnValue(ifStatement);

            if (statement is WhileStatementSyntax whileStatement && AllPathsReturnValue(whileStatement.Statements))
                return true;
        }
        return false;
    }

    private bool AllPathsReturnValue(IfStatementSyntax ifStatement)
    {
        var trueReturns = AllPathsReturnValue(ifStatement.TrueStatements);
        var elseReturns = ifStatement.ElseClause != null
            ? AllPathsReturnValue(ifStatement.ElseClause.FalseStatements)
            : false;

        return trueReturns && elseReturns;
    }
}
