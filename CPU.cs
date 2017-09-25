using System;
using System.Collections.Generic;

namespace asmint
{
    public enum enum_register
    {
         ax, bx, cx, dx,
         sp, ss, /*Stack*/
         bp, /*Base*/
         si, di,
         ip, cs, /*Code*/
         ds, /*Data Segment*/
         es,
         fs, gs, flags
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
        pop
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
        public byte[] registers = new byte[256];
        public byte[,] memory = new byte[256,256];
        private byte lip=255; //load instruction pointer.
        private byte lis=255; //load instruction segment

        public void LoadProgram(List<Instruction> program)
        {
            lip=255;
            lis=255;
            int ic = 3; 

            for (int i=0; i< program.Count; i++)
            {
                for(int j=0; j<ic; j++)
                {
                    lip -= 1;
                    if (lip < 0)
                    {
                        lip = 255;
                        lis--;
                        if (lis < 0)
                        {
                            Console.WriteLine("Error: Out of memory while loading program");
                        }
                    }
                }
                ic=4;
            }

            registers[(int)enum_register.ip] = lip;
            registers[(int)enum_register.cs] = lis;
            
            foreach(Instruction i in program)
            {
                this.LoadInstructionToMemory(i);
            }
        }
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
            registers[(int)enum_register.ss] = 0;
            registers[(int)enum_register.sp] = 0;
            registers[(int)enum_register.ds] = 10;  //Temporary
        }
        public void RunNextInstruction()
        {
            //Get Code Segment value
            int cs = registers[(int)enum_register.cs];
            //Get Instruction Pointer value
            int ip = registers[(int)enum_register.ip];
            //Get the Stack Segment value
            int ss = registers[(int)enum_register.ss];
            //Get the Stack Pointer value
            int sp = registers[(int)enum_register.sp];
            //Get the Default Segment value
            int ds = registers[(int)enum_register.ds];

            //Store in a byte array the 32 bits instruction to be executed
            byte[] instruction_chunk = 
            {
                memory[cs,ip],
                memory[cs,ip+1],
                memory[cs,ip+2],
                memory[cs,ip+3],
            };

            //Create instruction object
            Instruction instruction = new Instruction(instruction_chunk);

            //Where well store temporally te value to manipulate
            byte value=0;

            switch (instruction.op_code)
            {
                //MOV operation
                case enum_op_code.mov:
                    /*Use Base segment for memory operations! */
                    switch(instruction.op2_type)
                    {
                        case enum_op_type.constant:
                            value = instruction.op2;
                            break;
                        case enum_op_type.register:
                            value = registers[instruction.op2];
                            break;
                        case enum_op_type.memory_address:
                            value = memory[ds,(int)instruction.op2];
                            break;
                    }

                    switch(instruction.op1_type)
                    {
                        case enum_op_type.register:
                            registers[(int)instruction.op1] = value;
                            break;
                        case enum_op_type.memory_address:
                            memory[ds, (int)instruction.op1] = value;
                            break;
                    }
                    break;
                
                //PUSH operation
                case enum_op_code.push:
                    /*Use Base segment for memory operations! */
                    switch(instruction.op1_type)
                    {
                        case enum_op_type.constant:
                            value = instruction.op1;
                            break;
                        case enum_op_type.register:
                            value = registers[instruction.op1];
                            break;
                        case enum_op_type.memory_address:
                            value = memory[ds, (int)instruction.op1];
                            break;
                    }

                    memory[ss, sp] = value;

                    sp++;
                    if (sp > 255)
                    {
                        sp = 0;
                        ss++;
                        if (ss > 255)
                        {
                            Console.WriteLine("Out of memory!");
                            return;
                        }
                    }
                    break;
                
                //POP operation
                case enum_op_code.pop:

                    memory[ss, sp] = value;
                
                    /*Use Base segment for memory operations! */
                    switch(instruction.op1_type)
                    {
                        case enum_op_type.register:
                            registers[instruction.op1] = value;
                            break;
                        case enum_op_type.memory_address:
                            memory[ds, (int)instruction.op1] = value;
                            break;
                    }

                    sp--;
                    if (sp == 0)
                    {
                        sp = 255;
                        ss--;
                        if (ss < 0)
                        {
                            Console.WriteLine("Nothing on stack!");
                            return;
                        }
                    }
                    break;
            }

            //Increment the instruction pointer
            for (int i=0; i<4; i++)
            {
                ip += 1;
                if (ip > 255)
                {
                    ip = 0;
                    cs +=1;

                    if (cs > 255)
                        Console.WriteLine("Error! Out of Memory!");
                }
            }

            //Update the registers
            registers[(int)enum_register.ip] = (byte)ip;
            registers[(int)enum_register.cs] = (byte)cs;
            registers[(int)enum_register.sp] = (byte)sp;
            registers[(int)enum_register.ss] = (byte)ss; 
        }

    }
}