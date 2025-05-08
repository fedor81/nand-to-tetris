using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JackCompiling
{
    public partial class CodeWriter
    {
        private string _className;

        /// <summary>
        /// class Name { ... }
        /// </summary>
        public void WriteClass(ClassSyntax classSyntax)
        {
            _className = classSyntax.Name.Value;
            ParseClassSymbols(classSyntax.ClassVars);

            foreach (var subroutine in classSyntax.SubroutineDec)
                CompileSubroutine(subroutine);
        }

        private void CompileSubroutine(SubroutineDecSyntax subroutine)
        {
            WriteFunctionDeclaration(subroutine);

            switch (subroutine.KindKeyword.Value)
            {
                case "constructor":
                    WriteConstructorBody(subroutine);
                    break;
                case "method":
                    WriteMethodBody(subroutine);
                    break;
                case "function":
                    WriteStatements(subroutine.SubroutineBody.Statements);
                    break;
            }
        }

        private void WriteFunctionDeclaration(SubroutineDecSyntax subroutine)
        {
            var dict = new Dictionary<string, VarInfo>();
            methodSymbols = dict;

            ParseMethodVarDecs(dict, subroutine.SubroutineBody.VarDec);
            var localsCount = dict.Count;

            // Если метод, то первый аргумент this 
            var paramsOffset = subroutine.KindKeyword.Value == "method" ? 1 : 0;

            ParseMethodParameters(
                dict,
                subroutine.ParameterList.DelimitedParameters,
                paramsOffset
            );

            resultVmCode.Add($"function {_className}.{subroutine.Name.Value} {localsCount}");
        }

        /// <summary>
        /// method Type Name ( ParameterList ) { Body }
        /// </summary>
        private void WriteMethodBody(SubroutineDecSyntax subroutine)
        {
            resultVmCode.Add("push argument 0");
            resultVmCode.Add("pop pointer 0");

            WriteStatements(subroutine.SubroutineBody.Statements);
        }

        /// <summary>
        /// constructor Type Name ( ParameterList ) { Body }
        /// </summary>
        private void WriteConstructorBody(SubroutineDecSyntax subroutine)
        {
            var fieldCount = classSymbols.Values.Count(v => v.Kind == VarKind.Field);
            resultVmCode.Add($"push constant {fieldCount}");
            resultVmCode.Add("call Memory.alloc 1");
            resultVmCode.Add("pop pointer 0");

            WriteStatements(subroutine.SubroutineBody.Statements);

            resultVmCode.Add("push pointer 0");
            resultVmCode.Add("return");
        }

        /// <summary>
        /// ObjOrClassName . SubroutineName ( ExpressionList ) 
        /// </summary>
        private bool TryWriteSubroutineCall(TermSyntax term)
        {
            if (term is not SubroutineCallTermSyntax subroutineCallTerm)
                return false;

            WriteSubroutineCall(subroutineCallTerm.Call);
            return true;
        }

        /// <summary>
        /// do SubroutineCall ; 
        /// </summary>
        private bool TryWriteDoStatement(StatementSyntax statement)
        {
            if (statement is not DoStatementSyntax doStatement)
                return false;

            WriteSubroutineCall(doStatement.SubroutineCall);
            resultVmCode.Add("pop temp 0");
            return true;
        }

        private void WriteSubroutineCall(SubroutineCall subroutineCall)
        {
            var (subroutineName, argsCount) = PrepareSubroutineCall(subroutineCall);

            foreach (var expression in subroutineCall.Arguments.DelimitedExpressions)
                WriteExpression(expression);

            resultVmCode.Add($"call {subroutineName} {argsCount}");
        }

        private (string Name, int ArgCount) PrepareSubroutineCall(SubroutineCall call)
        {
            int argCount = call.Arguments.DelimitedExpressions.Count;
            string subroutineName;

            if (call.ObjectOrClass is { } objectOrClass)
            {
                if (FindVarInfo(objectOrClass.Name.Value) is { } varInfo)
                {
                    // Method call on an object
                    resultVmCode.Add($"push {varInfo.SegmentName} {varInfo.Index}");
                    subroutineName = $"{varInfo.Type}.{call.SubroutineName.Value}";
                    argCount++;
                }
                else
                {
                    // Function call on a class
                    subroutineName = $"{objectOrClass.Name.Value}.{call.SubroutineName.Value}";
                }
            }
            else
            {
                // Method call on this
                resultVmCode.Add("push pointer 0");
                subroutineName = $"{_className}.{call.SubroutineName.Value}";
                argCount++;
            }

            return (subroutineName, argCount);
        }

        /// <summary>
        /// return ;
        /// return Expression ;
        /// </summary>
        private bool TryWriteReturnStatement(StatementSyntax statement)
        {
            if (statement is not ReturnStatementSyntax returnStatement)
                return false;

            if (returnStatement.ReturnValue is null)
                resultVmCode.Add("push constant 0");
            else
                WriteExpression(returnStatement.ReturnValue);

            resultVmCode.Add("return");
            return true;
        }

        /// <summary>
        /// this | null
        /// </summary>
        private bool TryWriteObjectValue(TermSyntax term)
        {
            if (term is not ValueTermSyntax valueTerm || valueTerm.Indexing is not null)
                return false;

            if (valueTerm.Value.Value == "this")
                resultVmCode.Add("push pointer 0");
            else if (valueTerm.Value.Value == "null")
                resultVmCode.Add("push constant 0");
            else
                return false;

            return true;
        }

        private void ParseClassSymbols(IReadOnlyList<ClassVarDecSyntax> classVars)
        {
            int staticCount = 0, fieldCount = 0;

            foreach (var classVar in classVars)
            {
                var isStatic = classVar.KindKeyword.Value == "static";
                var kind = isStatic ? VarKind.Static : VarKind.Field;
                var counter = isStatic ? ref staticCount : ref fieldCount;

                AddSymbols(
                    classSymbols,
                    classVar.DelimitedNames.Select(n => (n.Value, classVar.Type.Value, kind)),
                    ref counter
                );
            }
        }

        private void AddSymbols(
            Dictionary<string, VarInfo> symbols,
            IEnumerable<(string Name, string Type, VarKind Kind)> variables,
            int startIndex = 0)
        {
            AddSymbols(symbols, variables, ref startIndex);
        }

        private void AddSymbols(
            Dictionary<string, VarInfo> symbols,
            IEnumerable<(string Name, string Type, VarKind Kind)> variables,
            ref int counter)
        {
            foreach (var (name, type, kind) in variables)
            {
                if (!symbols.TryAdd(name, new VarInfo(counter++, kind, type)))
                    throw new DuplicateNameException(name);
            }
        }

        private void ParseMethodVarDecs(
            Dictionary<string, VarInfo> symbols,
            IReadOnlyList<VarDecSyntax> varDecs)
        {
            AddSymbols(
                symbols,
                varDecs.SelectMany(vd => vd.DelimitedNames
                    .Select(n => (n.Value, vd.Type.Value, VarKind.Local)))
            );
        }

        private void ParseMethodParameters(
            Dictionary<string, VarInfo> symbols,
            IReadOnlyList<Parameter> parameters, int startWith = 0)
        {
            AddSymbols(
                symbols,
                parameters.Select(p => (p.Name.Value, p.Type.Value, VarKind.Parameter)),
                startWith
            );
        }
    }
}
