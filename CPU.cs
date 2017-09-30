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
         fs, gs
    }
    public enum enum_op_type
    {
        empty=0,
        constant=1,
        register=2,
        memory_address=4,
        register_pointer=8
    }

    public enum enum_op_code
    {
        mov,
        push,
        pop
    }
    
    public class CPU
    {

        public enum flags_values
        {
            carry=1,
            zero=2,
            sign=4,
            overflow=8,
            parity=16,
            unused2=32,
            unused3=64,
            unused4=128
        }
        public byte flags;
        private bool program_ended = false; //set to true when the program finished.
        public byte[] registers = new byte[256];
        public byte[,] memory = new byte[256,256];
        public bool[,] memory_readonly = new bool[256,256];
        private byte lip=255; //load instruction pointer.
        private byte lis=255; //load instruction segment

        public void LoadProgram(List<Instruction> program)
        {
            //Start at the bottom of the memory
            lip=255;
            lis=255;
            
            /*
            magic number. For the first iteration, 
            i want to consider the current position of memory.
            */
            int ic = 3; 

            /*
            Count backwards from the bottom of the memory,
            moving one position at the time till we arrive
            at the first instruction.
             */
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

            //Set the instruction pointer and code segment to the last position
            registers[(int)enum_register.ip] = lip;
            registers[(int)enum_register.cs] = lis;
            
            //Load all the instructions to memory from the ip onwards
            foreach(Instruction instr in program)
            {
                this.LoadInstructionToMemory(instr);
            }
        }
        public void LoadInstructionToMemory(Instruction ins)
        {
            //load instruction
            memory[lis,lip] = ins.code;
            memory[lis,lip+1] = ins.meta;
            memory[lis,lip+2] = ins.op1;
            memory[lis,lip+3] = ins.op2;
            
            //mark memory as read only            
            memory_readonly[lis,lip] = true;
            memory_readonly[lis,lip+1] = true;
            memory_readonly[lis,lip+2] = true;
            memory_readonly[lis,lip+3] = true;

            //advance one position at a time
            for(int i=0; i<4; i++)
            {
                lip++;
                if (lip > 255)
                {
                    lip = 0;
                    lis++;
                    if (lis > 255)
                        Console.WriteLine("Error! Out of Memory!");
                }
            }

        }
        public CPU()
        {
            //initialize read only memory array
            for(int i=0; i<256; i++)
            {
                for(int j=0;j<256; j++)
                {
                    memory_readonly[i,j] = false;
                }
            }
            
            //set code segment and instruction pointer to top of memory
            registers[(int)enum_register.ip] = 0;
            registers[(int)enum_register.cs] = 0;
            registers[(int)enum_register.ss] = 0;
            registers[(int)enum_register.sp] = 0;
            registers[(int)enum_register.ds] = 10;  //Temporary
        }
        public void RunNextInstruction()
        {
            if (program_ended)
            {
                //Do something
                return;
            }

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
                //MOV op1, op2
                case enum_op_code.mov:
                    /*Use Base segment for memory operations!*/
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
                        case enum_op_type.register: //move to a register
                            registers[(int)instruction.op1] = value;
                            break;
                        case enum_op_type.memory_address: //move to a memory address
                            if(memory_readonly[ds, (int)instruction.op1] == true)
                                Console.WriteLine("Error: Attempt to write to a read only memory sector");
                            else
                                memory[ds, (int)instruction.op1] = value;
                            break;
                        case enum_op_type.register_pointer: //move to a memory address stored in the following register
                            if(memory_readonly[ds, registers[(int)instruction.op1]] == true)
                                Console.WriteLine("Error: Attempt to write to a read only memory sector");
                            else
                                memory[ds, registers[(int)instruction.op1]] = value;
                            break;
                    }
                    break;
                
                //PUSH op1
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
                    memory[ss, sp] = value;
                    
                    break;
                
                //POP op1
                case enum_op_code.pop:

                    value = memory[ss, sp];
                
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
                    if (sp < 0)
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

            if (cs==255 && ip==252) //Are we at the last instruction of the program?
            {
                program_ended = true;
                return;
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