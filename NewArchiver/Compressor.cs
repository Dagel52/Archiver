using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace NewArchiver
{
    internal sealed class Compressor : Archiver
    {
        public Compressor(string input, string output) : base(input, output)
        {
           InputFile = new FileStream(_inputFile, FileMode.Open);
           var fileNamePosition = _inputFile.LastIndexOf('\\');
           var fileName = _inputFile.Remove(0, fileNamePosition + 1);
           OutFile = new FileStream(Path.Combine(_outputFile, fileName + ".gz"), FileMode.Append);
        }

        private readonly FileStream InputFile;
        private readonly FileStream OutFile;
        private readonly Thread RW = Thread.CurrentThread;
        private Task[] _taskPool;
        private int _standartBlockSize = 10485760;
        private ProgressBar _progress;

        public override void Launch(ProgressBar progress)
        {
            _progress = progress;
            Thread _RW = new Thread(new ThreadStart(Compress));
            _RW.Name = "R/W";
            _RW.Start();
        }

        private void Compress()
        {
            _progress.OverallSize = InputFile.Length;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                while (InputFile.Position < InputFile.Length)
                {
                    ReadAndTasksRun();
                    WriteToFile();
                }
                OutFile.Close();
                InputFile.Close();

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
            RW.Priority = ThreadPriority.Lowest;
            for (int taskNumber = 0; (taskNumber < _threadNumber) && (_taskPool[taskNumber] != null);)
            {
                if (_taskPool[taskNumber].IsCompleted)
                {
                    BitConverter.GetBytes(_compressedDataBlock[taskNumber].Length)
                    .CopyTo(_compressedDataBlock[taskNumber], 4);
                    OutFile.Write(_compressedDataBlock[taskNumber], 0, _compressedDataBlock[taskNumber].Length);
                    taskNumber++;
                }
            }
        }

        protected override void ReadAndTasksRun()
        {
            int _dataBlockSize;
            RW.Priority = ThreadPriority.Normal;
            _taskPool = new Task[_threadNumber];
            for (int taskNumber = 0; (taskNumber < _threadNumber) && (InputFile.Position < InputFile.Length);
                 taskNumber++)
            {
                if (InputFile.Length - InputFile.Position <= _standartBlockSize)
                {
                    _dataBlockSize = (int)(InputFile.Length - InputFile.Position);
                }
                else
                {
                    _dataBlockSize = _standartBlockSize;
                }
                _dataBlock[taskNumber] = new byte[_dataBlockSize];
                InputFile.Read(_dataBlock[taskNumber], 0, _dataBlockSize);
                CompressTaskRun(taskNumber);
                _progress.UpdatePosition(InputFile.Position);
            }
        }
    }
}
