
namespace asmint
{
    public class Instruction
    {
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
            this.op_code = (enum_op_code)chunk[0];
            op1_type = (enum_op_type)(chunk[1] & 15);
            op2_type = (enum_op_type)(chunk[1] >> 4);
            this.op1 = chunk[2];
            this.op2 = chunk[3];
        }

        public override string ToString()
        {
            return "blah";
        }
    }
}