using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NewArchiver
{
    internal abstract class Archiver
    {
        protected static int _threadNumber = Environment.ProcessorCount;
        protected static byte[][] _dataBlock = new byte[_threadNumber][];
        protected static byte[][] _compressedDataBlock = new byte[_threadNumber][];
        protected static int _dataBlockSize = 10485760;
        protected static string _inputFile, _outputFile;

        protected Archiver()
        {

        }

        internal Archiver(string input, string output)
        {
            _inputFile = input;
            _outputFile = output;
        }

        public abstract void Launch(ProgressBar progress);

        protected abstract void WriteToFile();
        
        protected abstract void ReadAndTasksRun();
    }
}
