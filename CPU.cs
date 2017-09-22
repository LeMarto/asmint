using System;

namespace asmint
{
    public class CPU
    {
        public class Instruction
        {
            byte header;  //opcode and info first 4 bytes opcode, last 4, metadata of op1 and op 2 param
            byte op1;
            byte op2;
        }
        public enum opcodes
        {
            mov,
            push,
            pop,
            add,
            inc,
            sub,
            dec,
            mul,
            div,
            not,
            and,
            or,
            shl,
            shr,
            xor,
            jmp,
            jc,
            jz
        }

        public enum flags_values
        {
            zero=1,
            carry=2,
            sign=4,
            overflow=8,
            unused1=16,
            unused2=32,
            unused3=64,
            unused4=128
        }
        public byte ax, bx, cx, dx, sp, bp, si, di, ip, cs, ds, es, fs, gs, ss, flags;
        public byte[,] memory = new byte[255,255];

        public CPU()
        {
            Console.WriteLine("Hello World!");
        }
    }
}