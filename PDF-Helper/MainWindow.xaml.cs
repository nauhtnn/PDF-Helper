using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Forms; // Namespace for FolderBrowserDialog
using OCR_Lib;

namespace PDF_Helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public const string NO_FOLDER_PATH = "Chưa chọn thư mục";
        public const string NO_FILE_PATH = "Chưa chọn file";

        System.Threading.Timer _timer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void FolderSelect_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Chọn thư mục";
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FolderPathTxb.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void FileSelect_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Title = "Chọn file";

            if (openFileDialog.ShowDialog() == true)
            {
                FilePathTxb.Text = openFileDialog.FileName;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _timer = new System.Threading.Timer((state) =>
            {
                string messages = StatusMessage.GetInstance().ConsumeMessage();
                if (!string.IsNullOrEmpty(messages))
                {
                    if(!Dispatcher.HasShutdownStarted && !Dispatcher.HasShutdownFinished)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            statusLabel.Text += messages;

                            // Force layout update before scrolling
                            scrollViewer.UpdateLayout();

                            // Scroll to the end
                            scrollViewer.ScrollToEnd();
                        });
                    }
                }
            }, null, 0, 1000); // Check every second
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer?.Dispose();
        }

        private void PathTxb_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox.Text == NO_FOLDER_PATH || textBox.Text == NO_FILE_PATH)
            {
                textBox.Text = string.Empty;
            }
        }

        private void PathTxb_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox.Text == string.Empty || textBox.Text.Trim() == string.Empty)
            {
                if (sender == FolderPathTxb)
                {
                    FolderPathTxb.Text = NO_FOLDER_PATH;
                }
                else if(sender == FilePathTxb)
                {
                    FilePathTxb.Text = NO_FILE_PATH;
                }
            }
        }

        private void OCR_Click(object sender, RoutedEventArgs e)
        {
            System.Threading.Thread folder_thread = null;
            System.Threading.Thread file_thread = null;

            StatusMessage.GetInstance().AddMessage("Nhận lệnh.");

            if (!string.IsNullOrEmpty(FolderPathTxb.Text) && FolderPathTxb.Text != NO_FOLDER_PATH)
            {
                string folderPath = FolderPathTxb.Text;
                folder_thread = new System.Threading.Thread(new System.Threading.ThreadStart(() => OCR_Helper.OCR_Folder(folderPath)));
                folder_thread.Start();
            }
            if (!string.IsNullOrEmpty(FilePathTxb.Text) && FilePathTxb.Text != NO_FILE_PATH)
            {
                string filePath = FilePathTxb.Text;
                file_thread = new System.Threading.Thread(new System.Threading.ThreadStart(() => OCR_Helper.OCR_File(filePath)));
                file_thread.Start();
            }

            var finishedMessageThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => {
                while((folder_thread != null && folder_thread.IsAlive) || (file_thread != null && file_thread.IsAlive))
                {
                    System.Threading.Thread.Sleep(1000);
                }

                StatusMessage.GetInstance().AddMessage("Hoàn thành lệnh.");
            }));
            finishedMessageThread.Start();
        }
    }
}
