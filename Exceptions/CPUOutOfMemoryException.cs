using System;

namespace asmint.Exceptions
{
    public class CPUOutOfMemoryException : System.Exception
    {
        public CPUOutOfMemoryException() { }
        public CPUOutOfMemoryException(string message) : base(message) { }
        public CPUOutOfMemoryException(string message, System.Exception inner) : base(message, inner) { }
    }
}