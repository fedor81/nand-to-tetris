using System.Linq;
using System;

namespace VMTranslator;

// Records - новый (C# 9) способ определения классов.
// Сразу создает конструктор и readonly свойства с аналогичными именами и типами.
// https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9#record-types
public record VmInstruction(int LineNumber, string Name, params string[] Args)
{
    // Нужно переопределить, чтобы в тестах было проще сравнивать объекты с эталоном.
    // Без этого Args сравнивались бы по ссылкам, а нужно, чтобы сравнивались поэлементно
    public virtual bool Equals(VmInstruction other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name && Args.SequenceEqual(other.Args);
    }

    // Каждый раз, когда переопределяешь Equals необходимо переопределять и GetHashCode тоже
    // чтобы не нарушать их согласованность: если объекты равны, их хэшкоды должны совпадать.
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Args);
    }

    public override string ToString()
    {
        return Args.Length == 0 
            ? Name : 
            $"{Name} {string.Join(" ", Args)}";
    }
}
