using Microsoft.Win32;
using PdfLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms; // Namespace for FolderBrowserDialog
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PdfHelper
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
                folderDialog.ShowNewFolderButton = false;

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
                string messages = StatusMessage.Instance.ConsumeMessage();
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
            textBox.Text = textBox.Text.Trim();
            if (textBox.Text == string.Empty)
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

            ToggleOCR_Buttons(startOCR: true);

            string taskName = TaskCbb.SelectedItem == OCRItem ? "OCR" : "NER";

            StatusMessage.Instance.AddMessage("Thực thi tác vụ " + taskName + ".");

            DocumentProcessor processor = ProcessorFactory.CreateProcessor(taskName);

            processor.StopRequested = false;

            if (!string.IsNullOrEmpty(FolderPathTxb.Text))
            {
                string folderPath = FolderPathTxb.Text;
                if(folderPath != NO_FOLDER_PATH)
                {
                    bool includeSubfolders = IncludeSubfoldersCkb.IsChecked ?? false;
                    folder_thread = new System.Threading.Thread(new System.Threading.ThreadStart(() => processor.ProcessFolder(folderPath, includeSubfolders)));
                    folder_thread.Start();
                }
            }
            if (!string.IsNullOrEmpty(FilePathTxb.Text))
            {
                string filePath = FilePathTxb.Text;
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

                Dispatcher.Invoke(() =>
                {
                    ToggleOCR_Buttons(startOCR: false);
                    if(TaskCbb.SelectedItem == NERItem)
                    {
                        string exportFilePath = null;
                        Microsoft.Win32.SaveFileDialog saveResultDialog = new Microsoft.Win32.SaveFileDialog();
                        saveResultDialog.Title = "Chọn file lưu kết quả";

                        if (saveResultDialog.ShowDialog() == true)
                        {
                            exportFilePath = saveResultDialog.FileName;
                        }

                        if(string.IsNullOrEmpty(exportFilePath))
                        {
                            int tempIndex = 1;
                            while (File.Exists(System.IO.Path.GetTempPath() + $"LeaveSlip_Export_{tempIndex}.xlsx"))
                            {
                                tempIndex++;
                            }
                            exportFilePath = System.IO.Path.GetTempPath() + $"LeaveSlip_Export_{tempIndex}.xlsx";
                        }

                        if(!exportFilePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                        {
                            exportFilePath += ".xlsx";
                        }

                        NerLeaveSlip.Instance.ExportToXlsx(exportFilePath);
                        StatusMessage.Instance.AddMessage($"Xuất dữ liệu giấy nghỉ phép ra file Excel: {exportFilePath}");
                    }
                });

                StatusMessage.Instance.AddMessage("Hoàn thành tác vụ " + taskName + ".");

            }));
            finishedMessageThread.Start();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            StatusMessage.Instance.AddMessage("Ra lệnh ngắt tác vụ.");
            OcrHelper.Instance.StopRequested = true;
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
