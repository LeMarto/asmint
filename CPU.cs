using System;
using System.Collections.Generic;
using asmint.Exceptions;

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
        pop,
        inc,
        dec
    }
    
    public class CPU
    {

        public enum flags_values
        {
            carry=1,
            zero=2,
            sign=4,
            overflow=8,
            unused1=16,
            unused2=32,
            unused3=64,
            unused4=128
        }
        public byte flags;
        private bool program_ended = false; //set to true when the program finished.
        public bool EOP
        {
            get
            {
                return program_ended;
            }
        }
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
                            throw new CPUOutOfMemoryException("Out of memory while positioning memory to load program.");
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
                        throw new CPUOutOfMemoryException("Out of memory while loading instruction.");
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

            switch (instruction.op_code)
            {
                //MOV op1, op2
                case enum_op_code.mov:
                    MOV(instruction);
                    break;
                
                //PUSH op1
                case enum_op_code.push:
                    PUSH(instruction);
                    break;
                
                //POP op1
                case enum_op_code.pop:
                    POP(instruction);
                    break;

                //INC op1
                case enum_op_code.inc:
                    INC(instruction);
                    break;

                //DEC op1
                case enum_op_code.dec:
                    DEC(instruction);
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
                        throw new CPUOutOfMemoryException("Ran out of memory while trying to increment the instruction pointer.");
                }
            }

            //Update the registers
            registers[(int)enum_register.ip] = (byte)ip;
            registers[(int)enum_register.cs] = (byte)cs;
        }

        #region Operations
        private void MOV(Instruction instruction)
        {
            /*
            MOV op1, op2
            */
            byte value=0;
            //Get the Data Segment value
            int ds = registers[(int)enum_register.ds];

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
                        throw new CPUReadOnlyMemoryException("MOV Error: Attempt to write to a read only memory sector.");
                    else
                        memory[ds, (int)instruction.op1] = value;
                    break;
                case enum_op_type.register_pointer: //move to a memory address stored in the following register
                    if(memory_readonly[ds, registers[(int)instruction.op1]] == true)
                        throw new CPUReadOnlyMemoryException("MOV Error: Attempt to write to a read only memory sector.");
                    else
                        memory[ds, registers[(int)instruction.op1]] = value;
                    break;
            }
        }
        
        private void PUSH(Instruction instruction)
        {
            /*
            PUSH op1
            */
            byte value=0;
            //Get the Data Segment value
            int ds = registers[(int)enum_register.ds];
            //Get the Stack Segment value
            int ss = registers[(int)enum_register.ss];
            //Get the Stack Pointer value
            int sp = registers[(int)enum_register.sp];
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
                    throw new CPUOutOfMemoryException("PUSH Error: While trying to increment the stack pointer I arrived to the end of the memory.");
            }

            //If stack pointer points to a read only memory, error out
            if(memory_readonly[ss, sp] == true)
                throw new CPUReadOnlyMemoryException("PUSH Error: Attempt to write to a read only memory sector.");
            else
                memory[ss, sp] = value;
            
            registers[(int)enum_register.sp] = (byte)sp;
            registers[(int)enum_register.ss] = (byte)ss;
        }

        private void POP(Instruction instruction)
        {
            /*
            POP op1
            */
            byte value=0;
            //Get the Data Segment value
            int ds = registers[(int)enum_register.ds];
            //Get the Stack Segment value
            int ss = registers[(int)enum_register.ss];
            //Get the Stack Pointer value
            int sp = registers[(int)enum_register.sp];

            value = memory[ss, sp];

            switch(instruction.op1_type)
            {
                case enum_op_type.register:
                    registers[instruction.op1] = value;
                    break;
                case enum_op_type.memory_address:
                    //Make sure the position we re trying to move is not read only
                    if(memory_readonly[ds, (int)instruction.op1] == true)
                        throw new CPUReadOnlyMemoryException("POP Error: Attempt to write to a read only memory sector.");
                    else
                        memory[ds, (int)instruction.op1] = value;
                    break;

            }

            sp--;
            if (sp < 0)
            {
                sp = 255;
                ss--;
                if (ss < 0)
                    throw new CPUOutOfMemoryException("POP Error: While trying to decrement the stack pointer I arrived at the beggining of the memory.");
            }
            
            registers[(int)enum_register.sp] = (byte)sp;
            registers[(int)enum_register.ss] = (byte)ss;
        }

        private void INC(Instruction instruction)
        {
            /*
            INC op1
            */
            
            //Get the Data Segment value
            int ds = registers[(int)enum_register.ds];
            
            int value=0;

            switch(instruction.op1_type)
            {
                case enum_op_type.register:
                    value = registers[instruction.op1];
                    break;
                case enum_op_type.memory_address:
                    //Make sure the position we re trying to move is not read only
                    if(memory_readonly[ds, (int)instruction.op1] == true)
                        throw new CPUReadOnlyMemoryException("INC Error: Attempt to write to a read only memory sector.");
                    else
                        value = memory[ds, (int)instruction.op1];
                    break;

            }
            
            value++;
            
            /*
                Because we are incrementing op1 by 1, "op2" always = 1
                or 0000 0001. The 8th bit will always be 0.

                That means that the only variables are operand 1
                and the result after the increment.

                The 2 situations where overflow flag should be true are:
                [ ]op1 negative, op2 negative, result positive <- Never possible, because op2 always positive.
                [X]op1 positive, op2 positive, result negative
            */

            bool op1_negative = (128 & (int)instruction.op1) == 128;
            bool value_negative = (128 & value) == 128;

            //Overflow flag ON because of the negative representation
            if (!op1_negative && value_negative)
                flags = (byte)((int)flags | (int)flags_values.overflow);
            else //overflow flag OFF
                flags = (byte)((int)flags & ~(int)flags_values.overflow);

            switch(instruction.op1_type)
            {
                case enum_op_type.register:
                    registers[instruction.op1] = (byte) value;
                    break;
                case enum_op_type.memory_address:
                    memory[ds, (int)instruction.op1] = (byte) value;
                    break;
            }
        }
        private void DEC(Instruction instruction)
        {
            /*
            DEC op1
            */
            
            //Get the Data Segment value
            int ds = registers[(int)enum_register.ds];
            
            int value=0;

            switch(instruction.op1_type)
            {
                case enum_op_type.register:
                    value = registers[instruction.op1];
                    break;
                case enum_op_type.memory_address:
                    //Make sure the position we re trying to move is not read only
                    if(memory_readonly[ds, (int)instruction.op1] == true)
                        throw new CPUReadOnlyMemoryException("DEC Error: Attempt to write to a read only memory sector.");
                    else
                        value = memory[ds, (int)instruction.op1];
                    break;
            }
            
            value--;
            /*
                Because we are decrementing op1 by 1, "op2" always = 1
                or 0000 0001. The 8th bit will always be 0.

                That means that the only variables are operand 1
                and the result after the decrement.

                The 2 situations where overflow flag should be true are:
                [ ]op1 positive, op2 negative, result negative <- Never possible, because op2 always positive.
                [X]op1 negative, op2 positive, result positive
            */

            bool op1_negative = (128 & (int)instruction.op1) == 128;
            bool value_negative = (128 & value) == 128;

            //Overflow flag ON because of the negative representation
            if (op1_negative && !value_negative)
                flags = (byte)((int)flags | (int)flags_values.overflow);
            else //overflow flag OFF
                flags = (byte)((int)flags & ~(int)flags_values.overflow);

            if (value == 0) //zero flag ON
                flags = (byte)((int)flags | (int)flags_values.zero);

            else if (value > 0) //zero flag OFF
                flags = (byte)((int)flags & ~(int)flags_values.zero);

            switch(instruction.op1_type)
            {
                case enum_op_type.register:
                    registers[instruction.op1] = (byte) value;
                    break;
                case enum_op_type.memory_address:
                    memory[ds, (int)instruction.op1] = (byte) value;
                    break;
            }
        }
        #endregion
    }
}