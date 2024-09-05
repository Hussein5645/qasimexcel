using ClosedXML.Excel;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

using System.Collections.Generic;


namespace PatientManagementApp
{
    public partial class MainWindow : Window
    {
        private readonly string appDirectory;

        public MainWindow()
        {
            InitializeComponent();

            // Automatically create the directory for storing files
            appDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PatientFiles");
            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }

            DisplayFilesInTreeView(appDirectory);
        }

        private void btnCreateNew_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Create New Patient Database",
                InitialDirectory = appDirectory
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Patients");
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Name";
                worksheet.Cell(1, 3).Value = "Age";
                worksheet.Cell(1, 4).Value = "Gender";
                worksheet.Cell(1, 5).Value = "Condition";

                workbook.SaveAs(saveFileDialog.FileName);

                AddFileToDirectoryTree(saveFileDialog.FileName);

                AddPatientWindow addPatientWindow = new AddPatientWindow(saveFileDialog.FileName);
                addPatientWindow.Show();
                this.Close();
            }
        }
        private void btnEditConfig_Click(object sender, RoutedEventArgs e)
        {
            EditConfigWindow editConfigWindow = new EditConfigWindow();
            editConfigWindow.Show();
        }

        private void btnAddFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Add Patient Database",
                InitialDirectory = appDirectory
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string destinationFilePath = Path.Combine(appDirectory, Path.GetFileName(openFileDialog.FileName));
                File.Copy(openFileDialog.FileName, destinationFilePath, true);

                AddFileToDirectoryTree(destinationFilePath);
            }
        }

        private void DisplayFilesInTreeView(string directoryPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            TreeViewItem rootItem = new TreeViewItem { Header = directoryInfo.Name, Tag = directoryInfo };
            treeView.Items.Clear();
            treeView.Items.Add(rootItem);

            foreach (var file in directoryInfo.GetFiles("*.xlsx"))
            {
                TreeViewItem fileItem = new TreeViewItem { Header = file.Name, Tag = file.FullName };
                rootItem.Items.Add(fileItem);
            }

            rootItem.IsExpanded = true;

            // Handle double-click event to open files
            treeView.MouseDoubleClick += TreeView_MouseDoubleClick;
        }

        private void TreeView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (treeView.SelectedItem is TreeViewItem selectedItem && selectedItem.Tag is string filePath)
            {
                try
                {
                    // Attempt to open the selected file
                    AddPatientWindow addPatientWindow = new AddPatientWindow(filePath);
                    addPatientWindow.Show();
                    this.Close();
                }
                catch (IOException ex)
                {
                    MessageBox.Show("The file is currently in use by another process. Please close the file and try again.",
                        "File Access Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An unexpected error occurred: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void AddFileToDirectoryTree(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            TreeViewItem fileItem = new TreeViewItem { Header = fileInfo.Name, Tag = fileInfo.FullName };

            if (treeView.Items[0] is TreeViewItem rootItem)
            {
                rootItem.Items.Add(fileItem);
                rootItem.IsExpanded = true;
            }
        }
    }


}
