using System.Collections.Generic;

namespace JackCompiling
{
    public partial class CodeWriter
    {
        private readonly List<string> resultVmCode = new List<string>();
        private readonly Dictionary<string, VarInfo> classSymbols;
        private IReadOnlyDictionary<string, VarInfo> methodSymbols;
        private string currentClassName = "Main";

        public CodeWriter(Dictionary<string, VarInfo>? classSymbols = null, Dictionary<string, VarInfo>? methodSymbols = null)
        {
            this.classSymbols = classSymbols ?? new Dictionary<string, VarInfo>();
            this.methodSymbols = methodSymbols ?? new Dictionary<string, VarInfo>();
        }

        public IReadOnlyList<string> ResultVmCode => resultVmCode;

        private VarInfo? FindVarInfo(string varName)
        {
            if (!methodSymbols.TryGetValue(varName, out var varInfo) && !classSymbols.TryGetValue(varName, out varInfo))
                return null;
            return varInfo;
        }

        public void Write(string vmCodeLine)
        {
            resultVmCode.Add(vmCodeLine);
        }
        
        public string GetResult() => string.Join("\n", resultVmCode);
    }
}
