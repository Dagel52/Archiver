using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NewArchiver
{
    internal class Compressor : Archiver
    {
        public Compressor(string input, string output) : base(input, output)
        {
            inputFile = new FileStream(_inputFile, FileMode.Open);
            var abc =_inputFile.LastIndexOf('\\');
            var result = _inputFile.Remove(0, abc + 1);
            outFile = new FileStream($"{_outputFile}\\{result}.gz", FileMode.Append);
        }

        private FileStream inputFile;
        private FileStream outFile;
        private Thread _RW = Thread.CurrentThread;
        private Task[] _taskPool;
        private int _standartBlockSize = 10485760;
        ProgressBar _progress;

        public override void Launch(ProgressBar progress)
        {
            _progress = progress;
            Thread _RW = new Thread(new ThreadStart(Compress));
            _RW.Name = "R/W";
            _RW.Start();
        }

        private void Compress()
        {
            _progress._overallSize = inputFile.Length;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                while (inputFile.Position < inputFile.Length)
                {
                    ReadAndTasksRun();
                    WriteToFile();
                }
                outFile.Close();
                inputFile.Close();

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                MessageBox.Show("Архивирование завершено за" + elapsedTime);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private static void CompressBlock(object i)
        {
            using (MemoryStream ms = new MemoryStream(_dataBlock[(int)i].Length))
            {
                using (GZipStream compressedStream = new GZipStream(ms, CompressionMode.Compress))
                {
                    compressedStream.Write(_dataBlock[(int)i], 0, _dataBlock[(int)i].Length);
                }
                _compressedDataBlock[(int)i] = ms.ToArray();
            }
        }

        private void CompressTaskRun (int taskNumber)
        {
            _taskPool[taskNumber] = Task.Run(() => CompressBlock(taskNumber));
        }

        protected override void WriteToFile()
        {
            _RW.Priority = ThreadPriority.Lowest;
            for (int taskNumber = 0; (taskNumber < _threadNumber) && (_taskPool[taskNumber] != null);)
            {
                if (_taskPool[taskNumber].IsCompleted)
                {
                    BitConverter.GetBytes(_compressedDataBlock[taskNumber].Length)
                    .CopyTo(_compressedDataBlock[taskNumber], 4);
                    outFile.Write(_compressedDataBlock[taskNumber], 0, _compressedDataBlock[taskNumber].Length);
                    taskNumber++;
                }
            }
        }

        protected override void ReadAndTasksRun()
        {
            int _dataBlockSize;
            _RW.Priority = ThreadPriority.Normal;
            _taskPool = new Task[_threadNumber];
            for (int taskNumber = 0; (taskNumber < _threadNumber) && (inputFile.Position < inputFile.Length);
                 taskNumber++)
            {
                if (inputFile.Length - inputFile.Position <= _standartBlockSize)
                {
                    _dataBlockSize = (int)(inputFile.Length - inputFile.Position);
                }
                else
                {
                    _dataBlockSize = _standartBlockSize;
                }
                _dataBlock[taskNumber] = new byte[_dataBlockSize];
                inputFile.Read(_dataBlock[taskNumber], 0, _dataBlockSize);
                CompressTaskRun(taskNumber);
                _progress.UpdatePosition(inputFile.Position);
            }
        }
    }
}
