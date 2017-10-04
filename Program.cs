using System;

namespace asmint
{
    class Program
    {
        static void Main(string[] args)
        {

            ProgramFile t = new ProgramFile("test.asm");
/*
            //mov ax, 99
            Instruction mov = new Instruction(enum_op_code.mov, enum_op_type.register, (byte)enum_register.ax, enum_op_type.constant, 99);
            
            //push ax
            Instruction push = new Instruction(enum_op_code.push, enum_op_type.register, (byte)enum_register.ax, enum_op_type.empty, 0);
            
            //pop bx
            Instruction pop = new Instruction(enum_op_code.pop, enum_op_type.register, (byte)enum_register.bx, enum_op_type.empty, 0);
            
            //mov [ax], 133
            Instruction mov_ptr = new Instruction(enum_op_code.mov, enum_op_type.register_pointer, (byte)enum_register.ax, enum_op_type.constant, 133);
            
            //inc ax
            Instruction inc = new Instruction(enum_op_code.inc, enum_op_type.register, (byte)enum_register.ax, enum_op_type.empty, 0);

            //mov [ax], 133
            Instruction mov_ptr2 = new Instruction(enum_op_code.mov, enum_op_type.register_pointer, (byte)enum_register.ax, enum_op_type.constant, 133);
            
            //dec ax
            Instruction dec = new Instruction(enum_op_code.dec, enum_op_type.register, (byte)enum_register.ax, enum_op_type.empty, 0);

            //mov [ax], 133
            Instruction mov_ptr3 = new Instruction(enum_op_code.mov, enum_op_type.register_pointer, (byte)enum_register.ax, enum_op_type.constant, 133);

            //dec ax
            Instruction dec2 = new Instruction(enum_op_code.dec, enum_op_type.register, (byte)enum_register.ax, enum_op_type.empty, 0);

            t.instructions.Add(mov);
            t.instructions.Add(push);
            t.instructions.Add(pop);
            t.instructions.Add(mov_ptr);
            t.instructions.Add(inc);
            t.instructions.Add(dec);
            t.Write();

*/
            CPU cpu = new CPU();
            t.Read();
            cpu.LoadProgram(t.instructions);

            while(!cpu.EOP)
            {
                cpu.RunNextInstruction();
            }

        }
    }
}
