using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JackCompiling;

public class VmEmulator
{
    public static VmEmulator LoadTestCode(IReadOnlyList<string> testCode)
    {
        var vmEmulator = new VmEmulator();
        vmEmulator.StackFrame = new StackFrame(new short[5], new short[5], 100, 0, null);
        vmEmulator.Load(testCode);
        return vmEmulator;
    }
    
    public readonly List<string> lines = new();
    public readonly Dictionary<string, int> symbolLineIndex = new();

    public short[] Statics = new short[256];
    public short[] Heap = new short[65536];
    public short[] Temp = new short[16];
    public short NextFreeHeapAddress = 777;
    public StackFrame StackFrame = new(Array.Empty<short>(), Array.Empty<short>(), 0, 0, null);
    
    public void Load(IReadOnlyList<string> vmCodeLines)
    {
        for (int i = 0; i < vmCodeLines.Count; i++)
        {
            var lineIndex = lines.Count;
            var line = vmCodeLines[i];
            lines.Add(line);
            if (line.StartsWith("function "))
            {
                var parts = line.Split(" ");
                symbolLineIndex.Add(parts[1], lineIndex);
            }
            else if (line.StartsWith("label "))
            {
                var parts = line.Split(" ");
                symbolLineIndex.Add(parts[1], lineIndex);
            }
        }
    }

    public void CallMain()
    {
        CallFunction("Main.main", 0);
    }

    public void CallFunction(string funcName, int argsCount)
    {
        if (TryCallStandardFunction(funcName, argsCount))
            return;

        var lineIndex = symbolLineIndex[funcName];
        var parts = lines[lineIndex].Split(" ");
        var localsCount = int.Parse(parts[2]);
        var args = new short[argsCount];
        for (int i = argsCount - 1; i >= 0; i--)
        {
            var arg = StackFrame.Stack.Pop();
            args[i] = arg;
        }
        StackFrame = new StackFrame(
            new short[localsCount], 
            args, 
            1234,
            1234,
            StackFrame);
        lineIndex++;
        ExecuteFunctionBody(ref lineIndex);
    }

    private bool TryCallStandardFunction(string funcName, int argsCount)
    {
        if (funcName == "Memory.alloc")
        {
            CallMemoryAlloc();
            return true;
        }
        if (funcName == "String.new")
        {
            var size = StackFrame.Stack.Pop();
            StackFrame.Stack.Push((short)(size+1));
            CallMemoryAlloc();
            var addr = StackFrame.Stack.Peek();
            Heap[addr] = 0;
            return true;
        }
        if (funcName == "String.length")
        {
            var addr = StackFrame.Stack.Pop();
            StackFrame.Stack.Push(Heap[addr]);
            return true;
        }
        if (funcName == "String.appendChar")
        {
            var ch = StackFrame.Stack.Pop();
            var addr = StackFrame.Stack.Pop();
            var size = (short)(Heap[addr] + 1);
            Heap[addr + size] = ch;
            Heap[addr] = size;
            StackFrame.Stack.Push(addr);
            return true;
        }
        if (funcName == "String.charAt")
        {
            var index = StackFrame.Stack.Pop();
            var stringAddr = StackFrame.Stack.Pop();
            var charAddr = (short)(stringAddr + 1 + index);
            StackFrame.Stack.Push(Heap[charAddr]);
            return true;
        }
        if (funcName == "Array.new")
        {
            var size = StackFrame.Stack.Pop();
            StackFrame.Stack.Push(size);
            CallMemoryAlloc();
            return true;
        }
        if (funcName == "Math.multiply")
        {
            var a = StackFrame.Stack.Pop();
            var b = StackFrame.Stack.Pop();
            StackFrame.Stack.Push((short)unchecked(a*b));
            return true;
        }
        if (funcName == "Math.divide")
        {
            var a = StackFrame.Stack.Pop();
            var b = StackFrame.Stack.Pop();
            StackFrame.Stack.Push((short)(b/a));
            return true;
        }
        if (funcName == "Math.abs")
        {
            short a = StackFrame.Stack.Pop();
            StackFrame.Stack.Push(Math.Abs(a));
            return true;
        }

        return false;
    }

    private void CallMemoryAlloc()
    {
        var size = StackFrame.Stack.Pop();
        StackFrame.Stack.Push(NextFreeHeapAddress);
        NextFreeHeapAddress += size;
    }

    public void ExecuteFunctionBody(ref int lineIndex)
    {
        var counter = 0;
        while (lineIndex < lines.Count)
        {
            if (counter++ > 100000)
                throw new Exception("Forever loop? Executed more than 100000 vm instructions");
            var line = lines[lineIndex++];
            var result = ExecuteVmCommand(line);
            if (result.exitFunction) return;
            if (result.labelToJump != null)
                lineIndex = symbolLineIndex[result.labelToJump]; 
        }
    }

    public (bool exitFunction, string? labelToJump) ExecuteVmCommand(string line)
    {
        var parts = line.Split(" ");
        var command = parts[0];
        Console.WriteLine("> " + line + " // " + VmStateToString());
        if (command == "return")
        {
            var result = StackFrame.Stack.Peek();
            Console.WriteLine("RETURN " + result);
            StackFrame.ParentFrame!.Stack.Push(result);
            StackFrame = StackFrame.ParentFrame;
            return (true, null);
        }

        if (command == "push")
        {
            var segment = parts[1];
            var index = int.Parse(parts[2]);
            var value = GetValue(segment, index);

            StackFrame.Stack.Push(value);
        }
        else if (command == "pop")
        {
            var value = StackFrame.Stack.Pop();
            var segment = parts[1];
            var index = int.Parse(parts[2]);
            SetValue(segment, index, value);
        }
        else if (command == "add") ExecuteBinaryOperation((a, b) => a + b);
        else if (command == "sub") ExecuteBinaryOperation((a, b) => a - b);
        else if (command == "and") ExecuteBinaryOperation((a, b) => a & b);
        else if (command == "or") ExecuteBinaryOperation((a, b) => a | b);
        else if (command == "lt") ExecuteBinaryOperation((a, b) => a < b ? -1 : 0);
        else if (command == "gt") ExecuteBinaryOperation((a, b) => a > b ? -1 : 0);
        else if (command == "eq") ExecuteBinaryOperation((a, b) => a == b ? -1 : 0);
        else if (command == "not") ExecuteUnaryOperation(a => ~a);
        else if (command == "neg") ExecuteUnaryOperation(a => -a);
        else if (command == "if-goto")
        {
            var label = parts[1];
            if (StackFrame.Stack.Peek() != 0)
                return (false, label);
        }
        else if (command == "goto")
        {
            var label = parts[1];
            return (false, label);
        }
        else if (command == "call")
        {
            CallFunction(parts[1], int.Parse(parts[2]));
        }
        else if (command == "label")
        {
        }
        else
        {
            throw new Exception(line);
        }
        return (false, null);
    }

    private object VmStateToString()
    {
        return $"Stack: [{string.Join(", ", StackFrame.Stack)}] this: {StackFrame.ThisAddress} [this]: {Heap[StackFrame.ThisAddress]}";
    }

    private void ExecuteUnaryOperation(Func<short, int> operation)
    {
        var a = StackFrame.Stack.Pop();
        StackFrame.Stack.Push(unchecked((short)operation(a)));
    }

    private void ExecuteBinaryOperation(Func<short, short, int> operation)
    {
        var b = StackFrame.Stack.Pop();
        var a = StackFrame.Stack.Pop();
        StackFrame.Stack.Push(unchecked((short)operation(a, b)));
    }

    private void SetValue(string segment, int index, short value)
    {
        if (segment == "static") Statics[index] = value;
        else if (segment == "argument") StackFrame.Args[index] = value;
        else if (segment == "local") StackFrame.Locals[index] = value;
        else if (segment == "temp") Temp[index] = value;
        else if (segment == "this") Heap[StackFrame.ThisAddress + index] = value;
        else if (segment == "that") Heap[StackFrame.ThatAddress + index] = value;
        else if (segment == "pointer")
        {
            if (index == 0) StackFrame.ThisAddress = value;
            else if (index == 1) StackFrame.ThatAddress = value;
            else throw new InvalidOperationException($"Wrong pointer index {index}");
        }
        else throw new Exception(segment);
    }

    private short GetValue(string segment, int index)
    {
        if (segment == "static") return Statics[index];
        else if (segment == "argument") return StackFrame.Args[index];
        else if (segment == "local") return StackFrame.Locals[index];
        else if (segment == "constant") return (short)index;
        else if (segment == "temp") return Temp[index];
        else if (segment == "this") return Heap[StackFrame.ThisAddress + index];
        else if (segment == "that") return Heap[StackFrame.ThatAddress + index];
        else if (segment == "pointer")
        {
            if (index == 0) return StackFrame.ThisAddress;
            else if (index == 1) return StackFrame.ThatAddress;
            else throw new InvalidOperationException($"Wrong pointer index {index}");
        }
        else throw new Exception(segment);
    }
}

public record StackFrame(short[] Locals, short[] Args, short ThisAddress, short ThatAddress, StackFrame? ParentFrame)
{
    public short ThisAddress { get; set; } = ThisAddress;
    public short ThatAddress { get; set; } = ThatAddress;
    public Stack<short> Stack { get; } = new();
}
