using System;

namespace asmint.Exceptions
{
    public class CPUReadOnlyMemoryException : System.Exception
    {
        public CPUReadOnlyMemoryException() { }
        public CPUReadOnlyMemoryException(string message) : base(message) { }
        public CPUReadOnlyMemoryException(string message, System.Exception inner) : base(message, inner) { }
    }
}