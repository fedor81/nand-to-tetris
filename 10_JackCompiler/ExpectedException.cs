using System;

namespace JackCompiling
{
    public class ExpectedException : FormatException
    {
        public ExpectedException(string expected, Token? was)
            : base($"Expected {expected} but was: {was?.ToString() ?? "end of file"}")
        {
        }
    }
}
