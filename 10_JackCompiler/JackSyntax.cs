using System.Collections.Generic;
using System.Runtime.Serialization;

// ReSharper disable NotAccessedPositionalProperty.Global

namespace JackCompiling
{
    // Program structure:
    
    public record ClassSyntax(Token Class, Token Name,
        Token Open,
        IReadOnlyList<ClassVarDecSyntax> ClassVars,
        IReadOnlyList<SubroutineDecSyntax> SubroutineDec,
        Token Close);

    public record ClassVarDecSyntax(Token KindKeyword, Token Type, IReadOnlyList<Token> DelimitedNames,
        Token Semicolon);

    public record SubroutineDecSyntax(
        Token KindKeyword, Token ReturnType, Token Name, Token OpenArgs, ParameterListSyntax ParameterList,
        Token CloseArgs,
        SubroutineBodySyntax SubroutineBody);

    public record ParameterListSyntax(IReadOnlyList<Parameter> DelimitedParameters);

    public record Parameter(Token Type, Token Name);

    public record SubroutineBodySyntax(
        Token Open,
        IReadOnlyList<VarDecSyntax> VarDec,
        StatementsSyntax Statements,
        Token Close);

    public record VarDecSyntax(Token KindKeyword, Token Type, IReadOnlyList<Token> DelimitedNames, Token Semicolon);

    // Statements:

    public record StatementsSyntax(IReadOnlyList<StatementSyntax> Statements);

    public record StatementSyntax;

    public record LetStatementSyntax(Token Let, Token VarName, Indexing? Index, Token Eq,
        ExpressionSyntax Value, Token Semicolon) : StatementSyntax;

    public record Indexing(Token Open, ExpressionSyntax Index, Token Close);

    public record DoStatementSyntax(Token Do, SubroutineCall SubroutineCall, Token Semicolon) : StatementSyntax;

    public record SubroutineCall(
        MethodObjectOrClass? ObjectOrClass,
        Token SubroutineName,
        Token Open,
        ExpressionListSyntax Arguments,
        Token Close);

    public record ExpressionListSyntax(List<ExpressionSyntax> DelimitedExpressions);


    public record MethodObjectOrClass(Token Name, Token Dot);

    public record ReturnStatementSyntax(Token Return, ExpressionSyntax? ReturnValue, Token Semicolon) : StatementSyntax;

    public record IfStatementSyntax(
        Token If, Token Open, ExpressionSyntax Condition, Token Close,
        Token OpenTrue, StatementsSyntax TrueStatements, Token CloseTrue,
        ElseClause? ElseClause) : StatementSyntax;

    public record ElseClause(Token Else, Token Open, StatementsSyntax FalseStatements, Token Close);

    public record WhileStatementSyntax(Token While, Token Open, ExpressionSyntax Condition, Token Close,
        Token OpenStatements, StatementsSyntax Statements, Token CloseStatements) : StatementSyntax;

    // Expressions:

    public record ExpressionSyntax(TermSyntax Term, IReadOnlyList<ExpressionTail> Tail);

    public record ExpressionTail(Token Operator, TermSyntax Term);

    public record TermSyntax;

    [DataContract(Name = "term")]
    public record ValueTermSyntax(Token Value, Indexing? Indexing) : TermSyntax;

    [DataContract(Name = "term")]
    public record UnaryOpTermSyntax(Token UnaryOp, TermSyntax Term) : TermSyntax;

    [DataContract(Name = "term")]
    public record ParenthesizedTermSyntax(Token Open, ExpressionSyntax Expression, Token Close) : TermSyntax;

    [DataContract(Name = "term")]
    public record SubroutineCallTermSyntax(SubroutineCall Call) : TermSyntax;
}
