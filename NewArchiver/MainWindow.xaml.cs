using Microsoft.Win32;
using System;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using Path = System.IO.Path;

namespace NewArchiver
{
    public partial class MainWindow : Window
    {
        private static Archiver _archiver;
        private string _input, _output;
        private Checker _checker = new Checker();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_File_Path(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (openFileDialog.ShowDialog() == true)
            {
                Input.Text = Path.GetFullPath(openFileDialog?.FileName);
                _input = Input.Text;
                Output.Text =_output ??= Path.GetDirectoryName(_input);
            }

        }

        private void Button_Decompress(object sender, RoutedEventArgs e)
        {
            try
            {
                _checker.DecompressCheck(_input, _output);
                _archiver = new Decompressor(_input, _output);
                ProgressBar progress = new ProgressBar(Dispatcher, progressBar1, Percent, Heading);
                _archiver.Launch(progress);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                progressBar1.Visibility = Visibility.Hidden;
                Percent.Visibility = Visibility.Hidden;
                Heading.Visibility = Visibility.Hidden;
            }
        }

        private void Button_Folder_Path(object sender, RoutedEventArgs e)
        {
            try
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = true,
                };
                CommonFileDialogResult result = dialog.ShowDialog();
                Output.Text = _output = dialog?.FileName;
            }
            catch (Exception)
            {
                
            }
        }

        private void Button_Compress(object sender, RoutedEventArgs e)
        {
            try
            {
                _checker.CompressCheck(_input,_output);
                _archiver = new Compressor(_input, _output);
                ProgressBar progress = new ProgressBar(Dispatcher, progressBar1, Percent, Heading);
                _archiver.Launch(progress);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                progressBar1.Visibility = Visibility.Hidden;
                Percent.Visibility = Visibility.Hidden;
                Heading.Visibility = Visibility.Hidden;
            }
        }
    }
}
