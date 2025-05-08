using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Xml.Serialization;

namespace JackCompiling
{
    public class Tokenizer
    {
        private Stack<Token> pushedBack = new Stack<Token>();
        private string text;
        private int column = 1;
        private int line = 1;
        private int position = 0;

        private static readonly HashSet<char> Symbols = new()
        {
            '{',
            '}',
            '(',
            ')',
            '[',
            ']',
            '.',
            ',',
            ';',
            '+',
            '-',
            '*',
            '/',
            '&',
            '|',
            '<',
            '>',
            '=',
            '~',
        };

        private static readonly HashSet<string> Identifiers = new()
        {
            "class",
            "constructor",
            "function",
            "method",
            "field",
            "static",
            "var",
            "int",
            "char",
            "boolean",
            "void",
            "true",
            "false",
            "null",
            "this",
            "let",
            "do",
            "if",
            "else",
            "while",
            "return",
        };

        public Tokenizer(string text)
        {
            this.text = text;
        }

        /// <summary>
        /// Сначала возвращает все токены, которые вернули методом PushBack в порядке First In Last Out.
        /// Потом читает и возвращает один следующий токен, либо null, если больше токенов нет.
        /// Пропускает пробелы и комментарии.
        ///
        /// Хорошо, если внутри Token сохранит ещё и строку и позицию в исходном тексте. Но это не проверяется тестами.
        /// </summary>
        public Token? TryReadNext()
        {
            if (pushedBack.Count != 0)
                return pushedBack.Pop();

            SkipWhiteSpacesAndComments();

            if (position >= text.Length)
                return null;
            else if (
                TryReadKeyword(out var token)
                || TryReadIdentifier(out token)
                || TryReadSymbol(out token)
                || TryReadStringConstant(out token)
                || TryReadIntegerConstant(out token)
            )
                return token;
            throw new FormatException();
        }

        private bool TryReadIdentifier(out Token? token)
        {
            token = null;
            var current = text[position];

            if (char.IsDigit(current) || !char.IsLetter(current))
                return false;

            var end = position + 1;

            for (; end < text.Length; end++)
            {
                current = text[end];

                if (current != '_' && !char.IsLetterOrDigit(current))
                    break;
            }

            token = CreateToken(TokenType.Identifier, text[position..end]);
            return true;
        }

        private bool TryReadKeyword(out Token? token)
        {
            var end = position + 1;

            for (; end < text.Length; end++)
                if (!char.IsLetter(text[end]))
                    break;

            var value = text[position..end];

            if (Identifiers.Contains(value))
            {
                token = CreateToken(TokenType.Keyword, value);
                return true;
            }

            token = null;
            return false;
        }

        private bool TryReadSymbol(out Token? token)
        {
            token = null;
            var value = text[position];

            if (Symbols.Contains(value))
            {
                token = CreateToken(TokenType.Symbol, value.ToString());
                return true;
            }

            return false;
        }

        private bool TryReadStringConstant(out Token? token)
        {
            token = null;

            if (text[position] != '"')
                return false;

            var start = position + 1; // Пропускаем символ кавычек
            var end = start;

            for (; end < text.Length; end++)
            {
                var current = text[end];

                if (current == '"')
                    break;
                else if (current == '\n')
                    return false;
            }

            token = CreateToken(TokenType.StringConstant, text[start..end]);
            IncreasePosition(2); // Пропуск кавычек
            return true;
        }

        private bool TryReadIntegerConstant(out Token? token)
        {
            var end = position + 1;

            for (; end < text.Length; end++)
                if (!char.IsDigit(text[end]))
                    break;

            var value = text[position..end];

            if (int.TryParse(value, out int number) && 0 <= number && number <= 32767)
            {
                token = CreateToken(TokenType.IntegerConstant, value);
                return true;
            }

            token = null;
            return false;
        }

        private void SkipWhiteSpacesAndComments()
        {
            for (; position < text.Length; IncreasePosition())
            {
                var current = text[position];

                if (current == ' ' || current == '\t' || current == '\r')
                    continue;
                else if (current == '/')
                {
                    if (position + 1 < text.Length)
                    {
                        var next = text[position + 1];

                        if (next == '/')
                            SkipSinglelineComment();
                        else if (next == '*')
                            SkipMultilineComment();
                        else
                            break;
                    }
                    else
                        break;
                }
                else if (current == '\n')
                {
                    line++;
                    column = 0;
                }
                else
                    break;
            }
        }

        /// <summary>
        /// Требует установки position на начало комментария /*
        /// </summary>
        private void SkipMultilineComment()
        {
            IncreasePosition(2); // Пропуск начала комментария

            for (; position < text.Length; IncreasePosition())
            {
                var previous = text[position - 1];
                var current = text[position];

                if (previous == '*' && current == '/')
                    break;
                else if (previous == '\n')
                    IncreaseLineNumber();
            }
        }

        private void SkipSinglelineComment()
        {
            for (; position < text.Length; IncreasePosition())
            {
                if (text[position] == '\n')
                    break;
            }

            line++;
            column = 0;
        }

        private Token CreateToken(TokenType type, string value)
        {
            var token = new Token(type, value, line, column);
            IncreasePosition(value.Length);
            return token;
        }

        private void IncreasePosition(int offset = 1)
        {
            position += offset;
            column += offset;
        }

        private void IncreaseLineNumber()
        {
            line++;
            column = 1;
        }

        /// <summary>
        /// Откатывает токенайзер на один токен назад.
        /// Если token - null, то игнорирует его и никуда не возвращает.
        /// Поддержка null нужна для удобства, чтобы использовать TryReadNext, вместе с PushBack без лишних if-ов.
        /// </summary>
        public void PushBack(Token? token)
        {
            if (token != null)
            {
                pushedBack.Push(token);
            }
        }
    }
}
