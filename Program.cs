using System;

namespace asmint
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.WriteLine("Hello World!");
            byte x = 128;
            byte y = 128;

            Console.WriteLine(x);
            Console.WriteLine(y);
            int z = (byte)(x | y);
            Console.WriteLine(z);
        }
    }
}
