using System;

namespace asmint
{
    public class Instruction
    {
        public byte code;  //opcode
        public byte meta;  //register/memory/value
        public byte op1;
        public byte op2;

        public Instruction(byte code, byte meta, byte op1, byte op2)
        {
            this.code = code;
            this.meta = meta;
            this.op1 = op1;
            this.op2 = op2;
        }
    }
    public class CPU
    {

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

        private void LoadInstructionToMemory(Instruction ins)
        {
            memory[cs,ip] = ins.code;
            memory[cs,ip+1] = ins.meta;
            memory[cs,ip+2] = ins.op1;
            memory[cs,ip+3] = ins.op2;
            
            ip += 4;
            if (ip > 255)
            {
                ip = 0;
                cs +=1;

                if (cs > 255)
                    Console.WriteLine("Error! Out of Memory!");
            }
        }
        public CPU()
        {
            //set code segment and instruction pointer to top of memory
            ip = 0;
            cs = 0;
        }
    }
}