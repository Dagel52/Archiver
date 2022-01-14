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
    public partial class MainWindow : Window
    {
        static Archiver archiver;
        string _input, _output;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_File_Path(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                Input.Text = Path.GetFullPath(openFileDialog?.FileName);
                _input = Input.Text;
                Output.Text = _output ?? Path.GetDirectoryName(_input);
            }

        }

        private void Button_Decompress(object sender, RoutedEventArgs e)
        {
            try
            {
                archiver = new Decompressor(_input, _output);
                var progress = new ProgressBar(this.Dispatcher, progressBar1, Percent, Heading);
                archiver.Launch(progress);
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
                var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
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
                archiver = new Compressor(_input, _output);
                var progress = new ProgressBar(this.Dispatcher, progressBar1, Percent, Heading);
                archiver.Launch(progress);
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
