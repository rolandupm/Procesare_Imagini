using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WindowsFormsApp1
{
    static class Debug
    {
        public static void writeToFile(string text)
        {
            using (StreamWriter writetext = File.AppendText("debug.txt"))
            {
                writetext.Write(text,1);
            }

        }
    }
}
