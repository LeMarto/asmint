using System;

namespace asmint
{
    class Program
    {
        static void Main(string[] args)
        {
            ProgramFile t = new ProgramFile("test.asm");
            CPU cpu = new CPU();
            /*
            //mov ax, 1
            Instruction mov = new Instruction(enum_op_code.mov, enum_op_type.register, (byte)enum_register.ax, enum_op_type.constant, 2);
            //push ax
            Instruction mov2 = new Instruction(enum_op_code.push, enum_op_type.register, (byte)enum_register.ax, enum_op_type.empty, 0);
            
            t.instructions.Add(mov);
            t.instructions.Add(mov2);

            t.Write();
            */
            t.Read();

            foreach(Instruction i in t.instructions)
            {
                cpu.LoadInstructionToMemory(i);
            }
            cpu.RunNextInstruction();
            cpu.RunNextInstruction();
        }
    }
}
