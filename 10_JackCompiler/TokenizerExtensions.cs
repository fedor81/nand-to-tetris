using System;
using System.Collections.Generic;

namespace JackCompiling
{
    public static class TokenizerExtensions
    {
        /// <summary>Проверяет, что прочитанный токен имеет ожидаемое значение</summary>
        /// <exception cref="ExpectedException">Если прочитан неверный токен</exception>
        public static Token Read(this Tokenizer tokenizer, string expectedValue)
        {
            var token = tokenizer.Read();
            if (token.Value != expectedValue)
                throw new ExpectedException(expectedValue, token);
            return token;
        }

        /// <summary>Проверяет, что прочитанный токен имеет ожидаемый тип</summary>
        /// <exception cref="ExpectedException">Если прочитан неверный токен</exception>
        public static Token Read(this Tokenizer tokenizer, TokenType expectedType)
        {
            var token = tokenizer.Read();
            if (token.TokenType != expectedType)
                throw new ExpectedException(expectedType.ToString(), token);
            return token;
        }

        /// <summary>Читает токен или кидает исключение, если достигнут конец входа</summary>
        /// <exception cref="ExpectedException">Если достигнут конец входа</exception>
        public static Token Read(this Tokenizer tokenizer)
        {
            var token = tokenizer.TryReadNext();
            if (token == null)
                throw new ExpectedException("token", null);
            return token;
        }

        /// <summary>
        /// Читает список однотипных элементов с помощью tryReadItem, пока читается.
        ///
        /// Например, список полей, определенных в классе.
        /// </summary>
        public static List<T> ReadList<T>(this Tokenizer tokenizer, Func<Token, T?> tryReadItem)
        {
            var res = new List<T>();
            do
            {
                var token = tokenizer.TryReadNext();
                if (token is null) return res;
                var item = tryReadItem(token);
                if (item is null)
                {
                    tokenizer.PushBack(token);
                    return res;
                }

                res.Add(item);
            } while (true);
        }

        /// <summary>Читает список однотипных элементов,
        /// разделенных разделителем и заканчивающийся терминатором.
        /// Причем терминатор возвращается в tokenizer обратно.
        ///
        /// Например, список аргументов при вызове функции.
        /// </summary>
        public static List<T> ReadDelimitedList<T>(this Tokenizer tokenizer, Func<T> readItem, string delimiter,
            string terminator)
        {
            var res = new List<T>();
            var token = tokenizer.Read();
            tokenizer.PushBack(token);
            if (token.Value == terminator)
                return res;
            do
            {
                var item = readItem();
                var delimiterToken = tokenizer.Read();
                if (delimiterToken.Value == terminator)
                {
                    res.Add(item);
                    tokenizer.PushBack(delimiterToken);
                    return res;
                }

                if (delimiterToken.Value != delimiter)
                    throw new ExpectedException(delimiter, delimiterToken);
                res.Add(item);
            } while (true);
        }
    }
}
