using System;
using System.Collections.Generic;
using NUnit.Framework;
using Moq;

namespace VMTranslator.Tests;

/// <summary>
/// В этих тестах используется техника Mock-объектов с использованием библиотеки Moq.
/// Тест проверяет, что WriteModuleFromFile и WriteModule вызывают WriteInstruction
/// правильное количество раз с правильными параметрами.
/// </summary>
[TestFixture]
public class CodeWriterTests
{
    [Test]
    [Description("При трансляции целого файла ")]
    public void WriteModuleFromFile_ShouldCallTranslate_ForEveryInstructionInFile()
    {
        // Создаем "поддельный" CodeWriter, который наследуется от CodeWriter и переопределяет WriteInstruction так,
        // чтобы он не писал ничего в файл, а сохранял информацию о факте его вызова, чтобы потом можно было проверить
        // Чтобы это сработало, метод Writeinstruction объявлен виртуальным
        var mock = new Mock<CodeWriter>(new List<string>());

        // Перечисляем, какие вызовы к Writeinstruction мы ожидаем
        mock.Setup(codeWriter => codeWriter.WriteInstruction(new VmInstruction(1, "push", "constant", "7"), "SimpleAdd")).Verifiable();
        mock.Setup(codeWriter => codeWriter.WriteInstruction(new VmInstruction(2, "push", "constant", "8"), "SimpleAdd")).Verifiable();
        mock.Setup(codeWriter => codeWriter.WriteInstruction(new VmInstruction(3, "add"), "SimpleAdd")).Verifiable();

        // Вызываем тестируемый метод:
        mock.Object.WriteModuleFromFile("Tests/StackArithmetic/SimpleAdd/SimpleAdd.vm");

        // Проверяем, что все вызовы совершились и лишних вызовов не было
        mock.Verify();
        mock.Verify(codeWriter => codeWriter.WriteInstruction(It.IsAny<VmInstruction>(), It.IsAny<string>()), Times.Exactly(3));
    }

    [Test]
    public void WriteModule_ShouldCallTranslate_ForEveryInstructionInModule()
    {
        var moduleName = "module";
        var program = @"// comment
push constant 1
neg"
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var mock = new Mock<CodeWriter>(new List<string>());
        mock.Setup(codeWriter => codeWriter.WriteInstruction(new VmInstruction(1, "push", "constant", "1"), moduleName)).Verifiable();
        mock.Setup(codeWriter => codeWriter.WriteInstruction(new VmInstruction(2, "neg"), moduleName)).Verifiable();

        mock.Object.WriteModule(moduleName, program);

        mock.Verify();
        mock.Verify(codeWriter => codeWriter.WriteInstruction(It.IsAny<VmInstruction>(), It.IsAny<string>()), Times.Exactly(2));
    }
}
