using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NewArchiver
{
    internal class Decompressor : Archiver
    {
        public Decompressor(string input, string output) : base(input, output)
        {
            try
            {
                inputFile = new FileStream(_inputFile, FileMode.Open);
                var fileNamePosition = _inputFile.LastIndexOf('\\');
                var fileName = _inputFile.Remove(0, fileNamePosition + 1);
                fileName = fileName.Remove(fileName.Length - 3, 3);
                outFile = new FileStream(Path.Combine(_outputFile, fileName), FileMode.Append);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private Task[] _taskPool;
        private readonly Thread _RW = Thread.CurrentThread;
        private readonly FileStream inputFile;
        private readonly FileStream outFile;
        private readonly byte[] buffer = new byte[8];
        private int _dataPortionSize;
        private int compressedBlockLength;
        ProgressBar _progress;
        public override void Launch(ProgressBar progress)
        {
            _progress = progress;
            Thread _RW = new Thread(new ThreadStart(Decompress));
            _RW.Name = "R/W";
            _RW.Start();
        }

        public void Decompress()
        {
            _progress.OverallSize = inputFile.Length;
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
                MessageBox.Show("Разархивирование завершено за" + elapsedTime);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void DecompressBlock(object i)
        {
            using (MemoryStream ms = new MemoryStream(_compressedDataBlock[(int)i]))
            {

                using (GZipStream decompressedStream = new GZipStream(ms, CompressionMode.Decompress))
                {
                    decompressedStream.Read(_dataBlock[(int)i], 0, _dataBlock[(int)i].Length);
                }

            }
        }

        private void DecompressTaskRun(int taskNumber)
        {
            _taskPool[taskNumber] = Task.Run(() => DecompressBlock(taskNumber));
        }

        protected override void ReadAndTasksRun()
        {
            _RW.Priority = ThreadPriority.Normal;
            _taskPool = new Task[_threadNumber];
            for (int taskNumber = 0;
                 (taskNumber < _threadNumber) && (inputFile.Position < inputFile.Length);
                 taskNumber++)
            {
                inputFile.Read(buffer, 0, buffer.Length);
                compressedBlockLength = BitConverter.ToInt32(buffer, 4);
                _compressedDataBlock[taskNumber] = new byte[compressedBlockLength];
                buffer.CopyTo(_compressedDataBlock[taskNumber], 0);

                inputFile.Read(_compressedDataBlock[taskNumber], 8, compressedBlockLength - 8);
                _dataPortionSize = BitConverter.ToInt32(_compressedDataBlock[taskNumber], compressedBlockLength - 4);
                _dataBlock[taskNumber] = new byte[_dataPortionSize];
                DecompressTaskRun(taskNumber);
                _progress.UpdatePosition(inputFile.Position);
            }
        }

        protected override void WriteToFile()
        {
            _RW.Priority = ThreadPriority.Lowest;
            for (int taskNumber = 0; (taskNumber < _threadNumber) && (_taskPool[taskNumber] != null);)
            {
                if (_taskPool[taskNumber].IsCompleted)
                {
                    outFile.Write(_dataBlock[taskNumber], 0, _dataBlock[taskNumber].Length);
                    taskNumber++;
                }
            }
        }
    }
}
