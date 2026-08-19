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
        string selectedFolderPath;
        string selectedFilePath;

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
                    selectedFolderPath = folderDialog.SelectedPath;
                    statusLabel.Content += DateTime.Now.ToString("[dd/mm/yyyy HH:mm:ss] ") + "Chọn thư mục: " + selectedFolderPath + "\n";
                }
            }
        }

        private void FileSelect_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Title = "Chọn file";

            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;
                statusLabel.Content += DateTime.Now.ToString("[dd/mm/yyyy HH:mm:ss] ") + "Chọn file: " + selectedFilePath + "\n";

                OCR_Helper.OCR(selectedFilePath);
            }
        }
    }
}
