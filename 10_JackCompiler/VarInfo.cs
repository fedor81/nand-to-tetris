using System;

namespace JackCompiling
{
    public class VarInfo
    {
        public VarInfo(int index, VarKind kind, string type)
        {
            Index = index;
            Kind = kind;
            Type = type;
        }

        public string SegmentName =>
            Kind switch
            {
                VarKind.Static => "static",
                VarKind.Local => "local",
                VarKind.Field => "this",
                VarKind.Parameter => "argument",
                _ => throw new FormatException(Kind.ToString())
            };

        public string Type;
        public int Index;
        public VarKind Kind;
    }
}
