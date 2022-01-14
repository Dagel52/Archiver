using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NewArchiver
{
    internal class Checker
    {
        internal void CompressCheck(string inputFile, string outputFile)
        {
            var fileNamePosition = inputFile.LastIndexOf('\\');
            var fileName = inputFile.Remove(0, fileNamePosition + 1);
            outputFile = Path.Combine(outputFile, fileName + ".gz");
            FileInfo fileInf = new FileInfo(inputFile);
            if (fileInf.Extension == ".gz")
                throw new FileFormatException("Файл уже имеет формат архива");
            else if (File.Exists(outputFile))
                throw new FileFormatException("В заданной директории уже имеется данный архив");
        }

        internal void DecompressCheck(string inputFile, string outputFile)
        {
            var fileNamePosition = inputFile.LastIndexOf('\\');
            var fileName = inputFile.Remove(0, fileNamePosition + 1);
            fileName = fileName.Remove(fileName.Length - 3, 3);
            outputFile = Path.Combine(outputFile, fileName);
            FileInfo fileInf = new FileInfo(inputFile);
            if (fileInf.Extension != ".gz")
                throw new FileFormatException("Файл должен иметь расширение .gz");
            else if (File.Exists(outputFile))
                throw new FileFormatException("В заданной директории уже имеется данный файл");
        }
    }
}
