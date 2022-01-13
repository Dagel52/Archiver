using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using Path = System.IO.Path;

namespace NewArchiver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Archiver archiver;
        string _input, _output;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Compress_Path(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                Input.Text = Path.GetFullPath(openFileDialog.FileName);
                _input = Input.Text;
                Output.Text = _output ?? Path.GetDirectoryName(_input);
            }

        }

        private void Button_Decompress(object sender, RoutedEventArgs e)
        {
            archiver = new Decompressor(_input, _output);
            var progress = new ProgressBar(this.Dispatcher, progressBar1, Percent, Heading);
            archiver.Launch(progress);
        }

        private void Button_Folder_Path(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            Output.Text = _output = dialog.FileName;
        }

        private void Button_Compress(object sender, RoutedEventArgs e)
        {
            archiver = new Compressor(_input, _output);
            var progress = new ProgressBar(this.Dispatcher, progressBar1, Percent, Heading);
            archiver.Launch(progress);
        }
    }
}
