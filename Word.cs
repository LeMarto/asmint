using System;

namespace asmint
{
    public class Word
    {
        private UInt64 value;
        public static int BITS = 8;
        public static UInt64 MAX_NUMBER = (UInt64)Math.Pow((double)2, (double)BITS);
        private Word(UInt64 value)
        {
            this.value = value;
        }
        
        public static Word operator +(Word c1, Word c2)
        {
            if (c1.value + c2.value > MAX_NUMBER)
            {
               //overflow
            }
            UInt64 c1_msb = c1.value >> BITS - 1;
            UInt64 c2_msb = c2.value >> BITS - 1;

           /* 
            if(c1.value >> BITS - 1 = 1)
            {

            }
            */
            Word retval = new Word(c1.value + c2.value);
            
            return retval;
        }

        public static Word operator ++(Word c1)
        {
            if (c1.value + 1 > MAX_NUMBER)
            {
                //overflow
            }

            Word retval = new Word(c1.value + 1);
            
            return retval;
        }
    }
}