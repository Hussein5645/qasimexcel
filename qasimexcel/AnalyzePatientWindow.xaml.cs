using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json;

namespace PatientManagementApp
{
    public partial class AnalysisWindow : Window
    {
        private PatientFieldsConfig fieldConfig;
        private DataTable dataTable;

        public AnalysisWindow(string excelFilePath)
        {
            InitializeComponent();

            // Load field config
            fieldConfig = LoadFieldConfig("PatientFieldsConfig.json");

            // Load and process the Excel file
            dataTable = LoadExcelFile(excelFilePath, fieldConfig);

            // Populate checkboxes for field selection
            PopulateFieldSelection();
        }

        private PatientFieldsConfig LoadFieldConfig(string configFilePath)
        {
            if (!File.Exists(configFilePath))
                throw new FileNotFoundException("Configuration file not found.", configFilePath);

            var json = File.ReadAllText(configFilePath);
            return JsonConvert.DeserializeObject<PatientFieldsConfig>(json);
        }

        private DataTable LoadExcelFile(string filePath, PatientFieldsConfig config)
        {
            DataTable dt = new DataTable();
            var fieldMap = config.Fields.ToDictionary(f => f.Name, f => f);

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);
                bool firstRow = true;

                foreach (var row in worksheet.Rows())
                {
                    if (firstRow)
                    {
                        // Add columns to DataTable
                        foreach (var header in row.Cells())
                        {
                            if (fieldMap.ContainsKey(header.GetValue<string>()))
                            {
                                dt.Columns.Add(header.GetValue<string>());
                            }
                        }
                        firstRow = false;
                    }
                    else
                    {
                        // Add rows to DataTable
                        DataRow dataRow = dt.NewRow();
                        int colIndex = 0;
                        foreach (var cell in row.Cells())
                        {
                            if (colIndex < dt.Columns.Count)
                            {
                                dataRow[colIndex] = cell.GetValue<string>();
                                colIndex++;
                            }
                        }
                        dt.Rows.Add(dataRow);
                    }
                }
            }

            return dt;
        }

        private void PopulateFieldSelection()
        {
            FieldSelectionPanel.Items.Clear();
            foreach (var field in fieldConfig.Fields)
            {
                var checkBox = new CheckBox
                {
                    Content = field.Name,
                    Tag = field
                };
                FieldSelectionPanel.Items.Add(checkBox);
            }
        }
        private void CancelSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in FieldSelectionPanel.Items.OfType<CheckBox>())
            {
                item.IsChecked = false;
            }
        }

        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedFields = FieldSelectionPanel.Items.OfType<CheckBox>()
                .Where(cb => cb.IsChecked == true)
                .Select(cb => cb.Tag as FieldConfig)
                .ToList();

            if (selectedFields.Count < 2)
            {
                MessageBox.Show("Please select at least two fields for combination analysis.");
                return;
            }

            var analysisResults = AnalyzeData(dataTable, selectedFields);
            ResultsDataGrid.ItemsSource = analysisResults.DefaultView;
        }


        private DataTable AnalyzeData(DataTable dataTable, List<FieldConfig> selectedFields)
        {
            DataTable analysisResults = new DataTable();
            analysisResults.Columns.Add("Field Combination", typeof(string));
            analysisResults.Columns.Add("Count", typeof(int));

            // Print column names for debugging
            Console.WriteLine("DataTable columns:");
            foreach (DataColumn column in dataTable.Columns)
            {
                Console.WriteLine(column.ColumnName);
            }

            if (selectedFields.Count < 2)
            {
                MessageBox.Show("Please select at least two fields for combination analysis.");
                return analysisResults;
            }

            var combinations = dataTable.AsEnumerable()
                .Select(row => string.Join(" | ", selectedFields
                    .Select(field =>
                    {
                        if (!dataTable.Columns.Contains(field.Name))
                        {
                            Console.WriteLine($"Column {field.Name} does not exist in the DataTable.");
                            return $"{field.Name}: null"; // Default value for missing column
                        }
                        return $"{field.Name}: {(string.IsNullOrEmpty(row[field.Name]?.ToString()) ? "null" : row[field.Name].ToString())}";
                    })))
                .GroupBy(combination => combination)
                .Select(group => new
                {
                    Combination = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(result => result.Count);

            foreach (var result in combinations)
            {
                analysisResults.Rows.Add(result.Combination, result.Count);
            }
            DisplayChart(analysisResults);
            return analysisResults;
        }


        private bool isPieChartVisible = true;
        private double dataGridOpacity = 0.5;

        private void ToggleVisibilityAndOpacity_Click(object sender, RoutedEventArgs e)
        {
            // Toggle PieChart visibility
            isPieChartVisible = !isPieChartVisible;
            pieChart.Visibility = isPieChartVisible ? Visibility.Visible : Visibility.Collapsed;

            // Toggle DataGrid opacity
            dataGridOpacity = dataGridOpacity == 1.0 ? 0.5 : 1.0;
            ResultsDataGrid.Opacity = dataGridOpacity;
        }

        private void DisplayChart(DataTable analysisResults)
        {
            var chartValues = new SeriesCollection();

            foreach (DataRow row in analysisResults.Rows)
            {
                var fieldCombination = row["Field Combination"].ToString();
                var count = Convert.ToInt32(row["Count"]);

                chartValues.Add(new PieSeries
                {
                    Title = fieldCombination,
                    Values = new ChartValues<double> { count },
                    DataLabels = true
                });
            }

            pieChart.Series = chartValues;
        }

    }

    public class FieldConfig
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<string> Options { get; set; }
    }

    public class PatientFieldsConfig
    {
        public List<FieldConfig> Fields { get; set; }
    }
}
