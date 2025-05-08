using System;
using System.Collections.Generic;
using System.Linq;

namespace JackCompiling
{
    public class Parser
    {
        private readonly Tokenizer tokenizer;

        public Parser(Tokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
        }

        public ClassSyntax ReadClass()
        {
            var declaration = tokenizer.Read("class");
            var name = tokenizer.Read(TokenType.Identifier);
            var open = tokenizer.Read("{");
            var classVars = ReadClassVarDecs();
            var subroutineDecs = ReadSubroutineDecs();
            var close = tokenizer.Read("}");
            return new ClassSyntax(declaration, name, open, classVars, subroutineDecs, close);
        }

        private List<ClassVarDecSyntax> ReadClassVarDecs() =>
            tokenizer.ReadList(
                (token) =>
                {
                    if (token.Value is not "static" and not "field")
                        return null;
                    tokenizer.PushBack(token);
                    return ReadClassVarDec();
                }
            );

        private List<SubroutineDecSyntax> ReadSubroutineDecs() =>
            tokenizer.ReadList(
                (token) =>
                {
                    if (!IsSubroutineKind(token))
                        return null;
                    tokenizer.PushBack(token);
                    return ReadSubroutineDec();
                }
            );

        private bool IsSubroutineKind(Token token) =>
            token.Value is "constructor" or "function" or "method";

        private List<VarDecSyntax> ReadVarDeclarations() =>
            tokenizer.ReadList(
                (token) =>
                {
                    if (token.Value != "var")
                        return null;
                    tokenizer.PushBack(token);
                    return ReadVarDeclaration();
                }
            );

        private SubroutineDecSyntax ReadSubroutineDec()
        {
            var kind = tokenizer.Read(TokenType.Keyword);
            var returnType = ReadSubroutineReturnType();
            var name = tokenizer.Read(TokenType.Identifier);
            var open = tokenizer.Read("(");
            var parameters = ReadParameterList();
            var close = tokenizer.Read(")");
            var body = ReadSubroutineBody();

            return new SubroutineDecSyntax(kind, returnType, name, open, parameters, close, body);
        }

        private SubroutineBodySyntax ReadSubroutineBody()
        {
            var open = tokenizer.Read("{");
            var vars = ReadVarDeclarations();
            var statesments = ReadStatements();
            var close = tokenizer.Read("}");

            return new SubroutineBodySyntax(open, vars, statesments, close);
        }

        private VarDecSyntax ReadVarDeclaration()
        {
            var kind = tokenizer.Read("var");
            var type = ReadType();
            var names = tokenizer.ReadDelimitedList(() => tokenizer.Read(TokenType.Identifier), ",", ";");
            var semicolon = tokenizer.Read(";");

            return new VarDecSyntax(kind, type, names, semicolon);
        }

        private Token ReadSubroutineReturnType()
        {
            var returnType = tokenizer.Read();
            if (returnType.Value == "void" || IsCorrectType(returnType))
                return returnType;
            throw new ExpectedException("void or correct return type", returnType);
        }

        private ClassVarDecSyntax ReadClassVarDec()
        {
            var kind = tokenizer.Read(TokenType.Keyword);
            var varType = ReadType();
            var names = tokenizer.ReadDelimitedList(() => tokenizer.Read(TokenType.Identifier), ",", ";");
            var semicolon = tokenizer.Read(";");

            return new ClassVarDecSyntax(kind, varType, names, semicolon);
        }

        private bool IsCorrectType(Token token) =>
            token.Value is "int" or "char" or "boolean" || token.TokenType == TokenType.Identifier;

        private Token ReadType()
        {
            var token = tokenizer.Read();
            if (IsCorrectType(token))
                return token;
            throw new ExpectedException("int, char, boolean or TokenType.Identifier", token);
        }

        public StatementsSyntax ReadStatements() => new StatementsSyntax(
                tokenizer.ReadList(
                    (token) =>
                    {
                        foreach (var reader in new Func<Token, StatementSyntax?>[]
                        {
                            TryReadLetStatement,
                            TryReadIfStatement,
                            TryReadWhileStatement,
                            TryReadDoStatement,
                            TryReadReturnStatement
                        })
                            if (reader(token) is { } result)
                                return result;

                        return null;
                    }
                )
            );

        private ReturnStatementSyntax? TryReadReturnStatement(Token token)
        {
            if (token.Value != "return")
                return null;

            var value = TryReadExpression();
            var semicolon = tokenizer.Read(";");
            return new ReturnStatementSyntax(token, value, semicolon);
        }

        private DoStatementSyntax? TryReadDoStatement(Token token)
        {
            SubroutineCall? subroutine;

            if (token.Value == "do")
                subroutine = ReadSubroutineCall();
            else
            {
                subroutine = TryReadSubroutineCall(token);

                if (subroutine == null)
                    return null;

                token = new Token(TokenType.Keyword, "do", token.ColNumber, token.LineNumber);
            }

            var semicolon = tokenizer.Read(";");
            return new DoStatementSyntax(token, subroutine, semicolon);
        }

        private WhileStatementSyntax? TryReadWhileStatement(Token token)
        {
            if (token.Value != "while")
                return null;

            var (conditionOpen, condition, conditionClose, bodyOpen, body, bodyClose) =
                ReadConditionAndBody();

            return new WhileStatementSyntax(
                token,
                conditionOpen,
                condition,
                conditionClose,
                bodyOpen,
                body,
                bodyClose
            );
        }

        private IfStatementSyntax? TryReadIfStatement(Token token)
        {
            if (token.Value != "if")
                return null;

            var (conditionOpen, condition, conditionClose, trueOpen, trueBody, trueClose) =
                ReadConditionAndBody();
            var elseClause = TryReadElseClause();

            return new IfStatementSyntax(
                token,
                conditionOpen,
                condition,
                conditionClose,
                trueOpen,
                trueBody,
                trueClose,
                elseClause
            );
        }

        private record ConditionAndBody(
            Token ConditionOpen,
            ExpressionSyntax Condition,
            Token ConditionClose,
            Token BodyOpen,
            StatementsSyntax Body,
            Token BodyClose
        );

        private ConditionAndBody ReadConditionAndBody()
        {
            return new ConditionAndBody(
                ConditionOpen: tokenizer.Read("("),
                Condition: ReadExpression(),
                ConditionClose: tokenizer.Read(")"),
                BodyOpen: tokenizer.Read("{"),
                Body: ReadStatements(),
                BodyClose: tokenizer.Read("}")
            );
        }

        private ElseClause? TryReadElseClause()
        {
            var elseKeyword = tokenizer.TryReadNext();
            if (elseKeyword == null)
                return null;

            if (elseKeyword.Value != "else")
            {
                tokenizer.PushBack(elseKeyword);
                return null;
            }

            var open = tokenizer.Read("{");
            var statements = ReadStatements();
            var close = tokenizer.Read("}");

            return new ElseClause(elseKeyword, open, statements, close);
        }

        private LetStatementSyntax? TryReadLetStatement(Token token)
        {
            Token name;

            if (token.Value == "let")
                name = tokenizer.Read(TokenType.Identifier);
            else
            {
                name = token;

                if (name == null || name.TokenType != TokenType.Identifier)
                    return null;

                token = new Token(TokenType.Keyword, "let", token.ColNumber, token.LineNumber);
            }

            var index = TryReadIndexing();
            var eq = tokenizer.TryReadNext();

            if (eq == null || eq.Value != "=")
            {
                tokenizer.PushBack(eq);
                tokenizer.PushBack(name);
                return null;
            }

            var value = ReadExpression();
            var semicolon = tokenizer.Read(";");

            return new LetStatementSyntax(token, name, index, eq, value, semicolon);
        }

        private Indexing? TryReadIndexing(Token? token = null)
        {
            token ??= tokenizer.TryReadNext();
            if (token?.Value != "[")
            {
                tokenizer.PushBack(token);
                return null;
            }

            var index = ReadExpression();
            var close = tokenizer.Read("]");
            return new Indexing(token, index, close);
        }

        public SubroutineCall ReadSubroutineCall()
        {
            var token = tokenizer.Read();
            if (TryReadSubroutineCall(token) is { } subroutine)
                return subroutine;
            throw new ExpectedException("Subroutine call", token);
        }

        private ExpressionListSyntax ReadExpressionList() =>
            new ExpressionListSyntax(tokenizer.ReadDelimitedList(ReadExpression, ",", ")"));

        public ParameterListSyntax ReadParameterList() =>
            new ParameterListSyntax(tokenizer.ReadDelimitedList(ReadParameter, ",", ")"));

        private Parameter ReadParameter()
        {
            var type = ReadType();
            var name = tokenizer.Read(TokenType.Identifier);
            return new Parameter(type, name);
        }

        private ExpressionSyntax? TryReadExpression()
        {
            var term = TryReadTerm();
            if (term == null)
                return null;
            var tail = tokenizer.ReadList(TryReadExpressionTail);
            return new ExpressionSyntax(term, tail);
        }

        public ExpressionSyntax ReadExpression() =>
            TryReadExpression() ?? throw new ExpectedException("ExpressionSyntax", null);

        private readonly static HashSet<string> operators = new() { "+", "-", "*", "/", "&", "|", "<", ">", "=" };

        private ExpressionTail? TryReadExpressionTail(Token token)
        {
            if (!operators.Contains(token.Value))
                return null;

            var term = ReadTerm();
            return new ExpressionTail(token, term);
        }

        private readonly static HashSet<string> keywordConstants = new() { "true", "false", "null", "this" };

        private TermSyntax? TryReadTerm()
        {
            var token = tokenizer.TryReadNext();
            if (token == null)
                return null;

            if (TryReadSubroutineCall(token) is { } subroutineCall)
                return new SubroutineCallTermSyntax(subroutineCall);

            if (TryReadParenthesizedTerm(token) is { } parenthesized)
                return parenthesized;

            if (token.Value is "-" or "~")
                return new UnaryOpTermSyntax(token, ReadTerm());

            if (TryReadIndexing() is { } indexing)
                return new ValueTermSyntax(token, indexing);

            if (IsValidSimpleTerm(token))
                return new ValueTermSyntax(token, null);

            tokenizer.PushBack(token);
            return null;
        }

        private ParenthesizedTermSyntax? TryReadParenthesizedTerm(Token open)
        {
            if (open.Value == "(")
            {
                var expression = ReadExpression();
                var close = tokenizer.Read(")");
                return new ParenthesizedTermSyntax(open, expression, close);
            }

            return null;
        }

        private bool IsValidSimpleTerm(Token token) => token.TokenType is TokenType.Identifier
            or TokenType.IntegerConstant
            or TokenType.StringConstant
            || keywordConstants.Contains(token.Value);

        public TermSyntax ReadTerm() =>
            TryReadTerm() ?? throw new ExpectedException("TermSyntax", null);

        private SubroutineCall? TryReadSubroutineCall(Token methodName)
        {
            if (methodName.TokenType != TokenType.Identifier)
                return null;

            var objectOrClass = TryReadMethodObjectOrClass(methodName);

            if (objectOrClass is not null)
                methodName = tokenizer.Read(TokenType.Identifier);

            var open = tokenizer.TryReadNext();

            if (open is null || open.Value != "(")
            {
                tokenizer.PushBack(open);
                return null;
            }

            var args = ReadExpressionList();
            var close = tokenizer.Read(")");
            return new SubroutineCall(objectOrClass, methodName, open, args, close);
        }

        private MethodObjectOrClass? TryReadMethodObjectOrClass(Token token)
        {
            if (token.TokenType != TokenType.Identifier)
                return null;

            var dot = tokenizer.TryReadNext();
            if (dot != null && dot.Value == ".")
                return new MethodObjectOrClass(token, dot);

            tokenizer.PushBack(dot);
            return null;
        }
    }
}
