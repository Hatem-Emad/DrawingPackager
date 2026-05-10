using DrawingPackager.Core.Packaging;
using DrawingPackager.SolidEdge;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows;
using Forms = System.Windows.Forms;

namespace DrawingPackager.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseDrawingButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Solid Edge Draft (*.dft)|*.dft|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog(this) == true)
            {
                DrawingPathTextBox.Text = dialog.FileName;
            }
        }

        private void BrowseOutputButton_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new Forms.FolderBrowserDialog
            {
                Description = "Select the package output folder"
            };

            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                OutputFolderTextBox.Text = dialog.SelectedPath;
            }
        }

        private async void PackageButton_Click(object sender, RoutedEventArgs e)
        {
            PackageButton.IsEnabled = false;
            LogTextBox.Text = "Starting package..." + Environment.NewLine;

            try
            {
                var result = await PackageAsync();
                LogTextBox.Text = string.Join(Environment.NewLine, result.Messages);

                if (result.PackageFolder is not null)
                {
                    LogTextBox.AppendText(Environment.NewLine + $"Package folder: {result.PackageFolder}");
                }
            }
            finally
            {
                PackageButton.IsEnabled = true;
            }
        }

        private Task<PackageResult> PackageAsync()
        {
            var request = new PackageRequest(
                DrawingPathTextBox.Text,
                OutputFolderTextBox.Text,
                new PackageOptions
                {
                    ExportPdf = ExportPdfCheckBox.IsChecked == true,
                    CopySourceDrawing = CopyDrawingCheckBox.IsChecked == true
                });

            var service = new DrawingPackageService(new SolidEdgeDrawingAutomationService());
            return service.PackageAsync(request);
        }
    }
}
