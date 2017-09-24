using System;

namespace asmint
{
    public enum enum_register
    {
         ax, bx, cx, dx, sp, bp, si, di, ip, cs, ds, es, fs, gs, ss, flags
    }
    public enum enum_op_type
    {
        empty=0,
        constant=1,
        register=2,
        memory_address=4,
        reserved=8
    }

    public enum enum_op_code
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
    public class Instruction
    {

        /*
            mov ax, constant        -> set content of register ax to constant value
            mov ax, register        -> set content of register ax to value of the register
            mov ax, memory address  -> set content of register ax to value stored at memory address

            mov ff, constant        -> set content of memory address ff to value constant
            mov ff, register        -> set content of memory address ff to value of the register 
            mov ff, memory address  -> set content of memory address ff to the content of memory address

            push 1                  -> push to stack the value 1
            push ax                 -> push to stack the value stored in register ax
            push ff                 -> push to the stack the value stored in memory address ff

            first 4 bits op1
            last 4 bits op2
            0000 0000
         */


        public enum_op_code op_code;
        public byte code
        {
            get
            {
                return (byte)op_code; 
            }
        }  //opcode
        public byte meta
        {
            get
            {
                return (byte) (((byte)op2_type << 4) | (byte)op1_type);
            } 
        }        
        public byte op1;
        public byte op2;
        public enum_op_type op1_type = enum_op_type.register;
        public enum_op_type op2_type = enum_op_type.empty;
        public Instruction(enum_op_code op_code, enum_op_type op1_type, byte op1, enum_op_type op2_type, byte op2)
        {
            /*Constructor to help me manually create instructions*/
            this.op_code = op_code;
            this.op1_type = op1_type;
            this.op1 = op1;
            this.op2_type = op2_type;
            this.op2 = op2;
        }
        public Instruction(byte op_code, byte meta, byte op1, byte op2)
        {
            /*Constructor used to load from file */
            this.op_code = (enum_op_code)op_code;
            op1_type = (enum_op_type)((meta << 4) >> 4);
            op2_type = (enum_op_type)(meta >> 4);
            this.op1 = op1;
            this.op2 = op2;
        }

        public Instruction(byte[] chunk)
        {
            // 1010 0010
            // 1+2+4+8
            // 0000 1111
            // 0000 0010
            this.op_code = (enum_op_code)chunk[0];
            op1_type = (enum_op_type)(chunk[1] & 15);
            op2_type = (enum_op_type)(chunk[1] >> 4);
            this.op1 = chunk[2];
            this.op2 = chunk[3];
        }
    }
    public class CPU
    {

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
        public byte[] registers = new byte[255];
        public byte[,] memory = new byte[255,255];
        private byte lip=0; //load instruction pointer.
        private byte lis=0; //load instruction segment

        public void LoadInstructionToMemory(Instruction ins)
        {
            memory[lis,lip] = ins.code;
            memory[lis,lip+1] = ins.meta;
            memory[lis,lip+2] = ins.op1;
            memory[lis,lip+3] = ins.op2;
            
            lip += 4;
            if (lip > 255)
            {
                lip = 0;
                lis +=1;

                if (lis > 255)
                    Console.WriteLine("Error! Out of Memory!");
            }
        }
        public CPU()
        {
            //set code segment and instruction pointer to top of memory
            
            registers[(int)enum_register.ip] = 0;
            registers[(int)enum_register.cs] = 0;
            registers[(int)enum_register.ss] = 255;
            registers[(int)enum_register.sp] = 255;
        }
        public void RunNextInstruction()
        {
            int cs = registers[(int)enum_register.cs];
            int ip = registers[(int)enum_register.ip];
            
            byte[] instruction_chunk = 
            {
                memory[cs,ip],
                memory[cs,ip+1],
                memory[cs,ip+2],
                memory[cs,ip+3],
            };

            Instruction instruction = new Instruction(instruction_chunk);
            byte value=0;

            switch (instruction.op_code)
            {
                case enum_op_code.mov:

                    switch(instruction.op2_type)
                    {
                        case enum_op_type.constant:
                            value = instruction.op2;
                            break;
                        case enum_op_type.register:
                            value = registers[instruction.op2];
                            break;
                        case enum_op_type.memory_address:
                            value = memory[(int)registers[(int)enum_register.cs],(int)instruction.op2];
                            break;
                    }

                    switch(instruction.op1_type)
                    {
                        case enum_op_type.register:
                            registers[(int)instruction.op1] = value;
                            break;
                        case enum_op_type.memory_address:
                            memory[(int)registers[(int)enum_register.cs], (int)instruction.op1] = value;
                            break;
                    }
                    break;

                case enum_op_code.push:
                    switch(instruction.op1_type)
                    {
                        case enum_op_type.constant:
                            value = instruction.op1;
                            break;
                        case enum_op_type.register:
                            value = registers[instruction.op1];
                            break;
                        case enum_op_type.memory_address:
                            value = memory[(int)registers[(int)enum_register.cs],(int)instruction.op1];
                            break;
                    }
                    
                    memory[(int)registers[(int)enum_register.ss], (int)registers[(int)enum_register.sp]] = value;

                    registers[(int)enum_register.sp] -= 1;
                    if (registers[(int)enum_register.sp] < 0)
                    {
                        registers[(int)enum_register.ss]--;
                        registers[(int)enum_register.sp] = 0;
                        //guarda aca, verificar que no sobreescriba codigo...
                    }
                    break;
            }

            ip += 4;
            if (ip > 255)
            {
                ip = 0;
                cs +=1;

                if (cs > 255)
                    Console.WriteLine("Error! Out of Memory!");
            }

            registers[(int)enum_register.ip] = (byte)ip;
            registers[(int)enum_register.cs] = (byte)cs; 
        }

    }
}