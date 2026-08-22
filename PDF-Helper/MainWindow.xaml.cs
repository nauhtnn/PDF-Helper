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
            ToggleOCR_Buttons(startOCR: false);

            _timer = new System.Threading.Timer((state) =>
            {
                string messages = StatusMessage.GetInstance().ConsumeMessage();
                if (!string.IsNullOrEmpty(messages))
                {
                    if (!Dispatcher.HasShutdownStarted && !Dispatcher.HasShutdownFinished)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            if (AutoScrollCkb.IsChecked == true)
                            {
                                statusLabel.Text += messages;

                                scrollViewer.ScrollToEnd();
                            }
                            else
                            {
                                double currentOffset = scrollViewer.VerticalOffset;

                                statusLabel.Text += messages;
                                scrollViewer.UpdateLayout();
                                scrollViewer.ScrollToVerticalOffset(currentOffset);
                            }
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

        private void ToggleOCR_Buttons(bool startOCR)
        {
            if (startOCR)
            {
                StopTask_Btn.IsEnabled = true;

                StartTask_Btn.IsEnabled = false;

                FolderSelect_Btn.IsEnabled = false;
                FolderPathTxb.IsEnabled = false;
                IncludeSubfoldersCkb.IsEnabled = false;
                FileSelect_Btn.IsEnabled = false;
                FilePathTxb.IsEnabled = false;
                TaskCbb.IsEnabled = false;
            }
            else
            {
                StopTask_Btn.IsEnabled = false;

                StartTask_Btn.IsEnabled = true;

                FolderSelect_Btn.IsEnabled = true;
                FolderPathTxb.IsEnabled = true;
                IncludeSubfoldersCkb.IsEnabled = true;
                FileSelect_Btn.IsEnabled = true;
                FilePathTxb.IsEnabled = true;
                TaskCbb.IsEnabled = true;
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            System.Threading.Thread folder_thread = null;
            System.Threading.Thread file_thread = null;

            StatusMessage.GetInstance().AddMessage("Thực thi tác vụ.");

            ToggleOCR_Buttons(startOCR: true);

            DocumentProcessor processor = null;
            if(TaskCbb.SelectedItem == OCRItem)
            {
                processor = ProcessorFactory.CreateProcessor("OCR");
            }
            else
            {
                processor = ProcessorFactory.CreateProcessor("NER");
            }

            processor.StopRequested = false;

            if (!string.IsNullOrEmpty(FolderPathTxb.Text))
            {
                string folderPath = FolderPathTxb.Text.Trim();
                if(folderPath != NO_FOLDER_PATH)
                {
                    bool includeSubfolders = IncludeSubfoldersCkb.IsChecked ?? false;
                    folder_thread = new System.Threading.Thread(new System.Threading.ThreadStart(() => processor.ProcessFolder(folderPath, includeSubfolders)));
                    folder_thread.Start();
                }
            }
            if (!string.IsNullOrEmpty(FilePathTxb.Text))
            {
                string filePath = FilePathTxb.Text.Trim();
                if(filePath != NO_FILE_PATH)
                {
                    file_thread = new System.Threading.Thread(new System.Threading.ThreadStart(() => processor.ProcessFile(filePath)));
                    file_thread.Start();
                }
            }

            var finishedMessageThread = new System.Threading.Thread(new System.Threading.ThreadStart(() => {
                while((folder_thread != null && folder_thread.IsAlive) || (file_thread != null && file_thread.IsAlive))
                {
                    System.Threading.Thread.Sleep(1000);
                }

                StatusMessage.GetInstance().AddMessage("Hoàn thành tác vụ.");

                Dispatcher.Invoke(() =>
                {
                    ToggleOCR_Buttons(startOCR: false);
                });
            }));
            finishedMessageThread.Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StatusMessage.GetInstance().AddMessage("Ra lệnh ngắt tác vụ.");
            OCR_Helper.GetInstance().StopRequested = true;
        }

        private void ClearMessage_Click(object sender, RoutedEventArgs e)
        {
            statusLabel.Text = string.Empty;
        }

        private void Btn_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue is bool)
            {
                if ((bool)e.NewValue)
                {
                    (sender as System.Windows.Controls.Button).Foreground = System.Windows.Media.Brushes.White;
                }
                else
                {
                    (sender as System.Windows.Controls.Button).Foreground = System.Windows.Media.Brushes.Black;
                }
            }
        }

        private void AutoScrollCkb_Checked(object sender, RoutedEventArgs e)
        {
            scrollViewer.ScrollToEnd();
        }
    }
}
