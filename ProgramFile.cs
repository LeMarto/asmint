
using System;
using System.IO;
using System.Collections.Generic;

namespace asmint
{
    public class ProgramFile
    {
        private string filename;
        private string cwd;

        private string filePath
        {
            get
            {
                return cwd + "\\" +  filename;
            }
        }
        public List<Instruction> instructions = new List<Instruction>();       
        public ProgramFile(string filename)
        {
            this.cwd = Directory.GetCurrentDirectory();
            this.filename = filename;
        }

        public void Write()
        {
            
            if (File.Exists(filePath)) 
            {
                File.Delete(filePath);
            }

                    // Create the file.
            using (FileStream fs = File.Create(filePath)) 
            {
                foreach(Instruction i in instructions)
                {
                    Byte[] info = {i.code, i.meta, i.op1, i.op2};
                    fs.Write(info, 0, info.Length);
                }
            }
        }

        public void Read()
        {
            byte[] chunk = new byte[4];
            
            if (File.Exists(filePath)) 
            {
                // Create the file.
                using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read)) 
                {
                    while ( fs.Read(chunk, 0, chunk.Length) > 0) 
                    {
                        instructions.Add(new Instruction(chunk));
                    }
                }
            }
        }

    }
}
