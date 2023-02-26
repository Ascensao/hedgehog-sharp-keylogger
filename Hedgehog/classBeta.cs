using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace Hedgehog
{
    class classBeta
    {
        string path_beta = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Hedgehog\beta.txt";

        public void writeBeta(string _content)
        {
            try
            {
                using (FileStream fs = new FileStream(path_beta, FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(_content);
                }
            }
            catch { }
        }

        public void cleanBeta()
        {
            if(File.Exists(path_beta))
                File.WriteAllText(path_beta, string.Empty);
            else
                File.Create(path_beta);
        }
    }
}
