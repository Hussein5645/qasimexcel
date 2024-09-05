using ClosedXML.Excel;
using Newtonsoft.Json;
using OfficeOpenXml.Drawing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Windows;
using ClosedXML.Excel;
using static PatientManagementApp.Patient;
using System.Windows.Media;

namespace PatientManagementApp
{
    public partial class AddPatientWindow : Window
    {
        private string excelFilePath;
        private ObservableCollection<Patient> patients;
        private List<DynamicField> dynamicFields;
        private FloatingPanelWindow floatingPanelWindow;
        private StackPanel _cachedDynamicFieldsPanel;
        public AddPatientWindow(string filePath)
        {
            InitializeComponent();

            excelFilePath = filePath;
            dynamicFields = new List<DynamicField>();

            
            LoadDynamicFieldsConfig();
            LoadExcelData();

        }
        private void LoadExcelData()
        {
            // Initialize the DataTable for the DataGrid
            DataTable dataTable = new DataTable();
            patients = new ObservableCollection<Patient>();

            if (File.Exists(excelFilePath))
            {
                using (var workbook = new XLWorkbook(excelFilePath))
                {
                    var worksheet = workbook.Worksheet("Patients");
                    var range = worksheet.RangeUsed();

                    // Create columns for the DataTable
                    foreach (var header in range.Row(1).Cells())
                    {
                        dataTable.Columns.Add(header.GetValue<string>());
                    }

                    // Process rows and create Patient objects
                    foreach (var row in range.RowsUsed().Skip(1))
                    {
                        // Create a new Patient instance
                        var patient = new Patient();
                        var dataRow = dataTable.NewRow();

                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            var cellValue = row.Cell(i + 1).GetString();
                            dataRow[i] = cellValue;

                            // Convert and assign to Patient object based on dynamic fields
                            if (i < dynamicFields.Count)
                            {
                                var field = dynamicFields[i];
                                switch (field.Type)
                                {
                                    case Patient.FieldType.Text:
                                        patient.FieldValues[field.Name] = cellValue;
                                        break;
                                    case Patient.FieldType.Integer:
                                        patient.FieldValues[field.Name] = int.TryParse(cellValue, out var intVal) ? intVal : (int?)null;
                                        break;
                                    case Patient.FieldType.Boolean:
                                        patient.FieldValues[field.Name] = bool.TryParse(cellValue, out var boolVal) ? boolVal : (bool?)null;
                                        break;
                                    case Patient.FieldType.Choice:
                                        patient.FieldValues[field.Name] = cellValue;
                                        break;
                                }
                            }
                        }

                        // Add the patient to the collection and the DataTable row
                        patients.Add(patient);
                        dataTable.Rows.Add(dataRow);
                    }

                    // Set DataGrid1 items source
                    dataGrid1.ItemsSource = dataTable.DefaultView;
                }

            }else
            {
                MessageBox.Show(" file not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDynamicFieldsConfig()
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PatientFieldsConfig.json");
            
            if (!File.Exists(configPath))
            {
                MessageBox.Show("Configuration file not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            var configJson = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<PatientFieldConfig>(configJson);

            foreach (var field in config.Fields)
            {
                Label label = new Label { Content = field.Name };
                UIElement inputControl;

                switch (field.Type)
                {
                    case "Text":
                        inputControl = new TextBox();
                        break;
                    case "Integer":
                        inputControl = new TextBox();
                        break;
                    case "Boolean":
                        inputControl = new CheckBox();
                        break;
                    case "Choice":
                        var comboBox = new ComboBox();
                        foreach (var option in field.Options)
                        {
                            comboBox.Items.Add(option);
                        }
                        inputControl = comboBox;
                        break;
                    default:
                        continue;
                }

                dynamicFieldsPanel.Children.Add(label);
                dynamicFieldsPanel.Children.Add(inputControl);

                dynamicFields.Add(new DynamicField
                {
                    Name = field.Name,
                    Type = (Patient.FieldType)Enum.Parse(typeof(Patient.FieldType), field.Type),
                    Control = inputControl
                });
            }
        }
        private void btnanalyze_Click(object sender, RoutedEventArgs e)
        {

            // Open the AnalysisWindow
            AnalysisWindow analysisWindow = new AnalysisWindow(excelFilePath);
            analysisWindow.Show();
        }
        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Handle the text change event
            // For example, filter the DataGrid based on the search text
            string searchText = searchBox.Text.ToLower();
            // Assuming you have a method to filter DataGrid items
            FilterDataGrid(searchText);
        }

        private void FilterDataGrid(string searchText)
        {
            if (dataGrid1 == null)
            {
               // MessageBox.Show("DataGrid is not initialized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (dataGrid1.ItemsSource is DataView dataView)
            {
                if (dataView.Table != null)
                {
                    if (string.IsNullOrWhiteSpace(searchText))
                    {
                        dataView.RowFilter = string.Empty;
                    }
                    else
                    {
                        var filterExpressions = new List<string>();
                        foreach (DataColumn column in dataView.Table.Columns)
                        {
                            // Escape column names with square brackets and handle search text escaping
                            string columnName = column.ColumnName;
                            string escapedColumnName = $"[{columnName}]";

                            // Escape single quotes in search text
                            string escapedSearchText = searchText.Replace("'", "''");

                            // Create a filter expression for the current column
                            filterExpressions.Add($"{escapedColumnName} LIKE '%{escapedSearchText}%'");
                        }

                        dataView.RowFilter = string.Join(" OR ", filterExpressions);
                    }
                }
                else
                {
                    MessageBox.Show("Data table is null.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("DataGrid ItemsSource is not set or is not a DataView.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid1.SelectedItem is DataRowView rowView)
            {
                var selectedPatient = patients[dataGrid1.Items.IndexOf(rowView)];
                PopulatePatientDataToFields(selectedPatient);
            }
        }

        private void PopulatePatientDataToFields(Patient selectedPatient)
        {
            foreach (var field in dynamicFields)
            {
                string fieldName = field.Name;

                if (selectedPatient.FieldValues.TryGetValue(fieldName, out var value))
                {
                    switch (field.Type)
                    {
                        case Patient.FieldType.Text:
                            if (field.Control is TextBox textBox)
                            {
                                textBox.Text = value?.ToString();
                            }
                            break;
                        case Patient.FieldType.Integer:
                            if (field.Control is TextBox intTextBox)
                            {
                                intTextBox.Text = value?.ToString();
                            }
                            break;
                        case Patient.FieldType.Boolean:
                            if (field.Control is CheckBox checkBox)
                            {
                                checkBox.IsChecked = value is bool boolVal && boolVal;
                            }
                            break;
                        case Patient.FieldType.Choice:
                            if (field.Control is ComboBox comboBox)
                            {
                                comboBox.SelectedItem = value?.ToString();
                            }
                            break;
                    }
                }
            }
            
        }
        private void UpdatePatientsFromDataGrid()
        {
            if (dataGrid1.ItemsSource is DataView dataView)
            {
                for (int i = 0; i < dataView.Count; i++)
                {
                    var dataRow = dataView[i].Row;

                    if (i < patients.Count)
                    {
                        var patient = patients[i];

                        foreach (var field in dynamicFields)
                        {
                            string fieldName = field.Name;

                            if (dataRow.Table.Columns.Contains(fieldName))
                            {
                                patient.FieldValues[fieldName] = dataRow[fieldName];
                            }
                        }
                    }
                }
            }
        }


        private void ToggleDockButton_Click(object sender, RoutedEventArgs e)
        {
            if (toggleDockButton.IsChecked == true)
            {
                // Dock the panel by showing the docked panel and closing the floating window if it's open
                dockPanel.Visibility = Visibility.Visible;

                // Ensure dynamic fields are preserved
                if (_cachedDynamicFieldsPanel != null)
                {
                    // Clear current dynamic fields in dockPanel
                    dynamicFieldsPanel.Children.Clear();

                    // Move cached elements to dockPanel
                    foreach (UIElement child in _cachedDynamicFieldsPanel.Children.OfType<UIElement>().ToList())
                    {
                        // Remove from cached panel's parent if it has one
                        if (child is FrameworkElement frameworkElement)
                        {
                            // Detach the child from its current parent
                            var parent = VisualTreeHelper.GetParent(child) as Panel;
                            if (parent != null)
                            {
                                parent.Children.Remove(child);
                            }
                        }

                        dynamicFieldsPanel.Children.Add(child);
                    }

                    // Clear the cached panel
                    _cachedDynamicFieldsPanel.Children.Clear();
                }



                if (floatingPanelWindow != null)
                {
                    floatingPanelWindow.Close();
                    floatingPanelWindow = null;
                }
            }
            else
            {
                // Undock the panel by showing the floating window and hiding the docked panel
                dockPanel.Visibility = Visibility.Collapsed;

                // Cache dynamic fields from dockPanel
                _cachedDynamicFieldsPanel = new StackPanel();
                foreach (UIElement child in dynamicFieldsPanel.Children.OfType<UIElement>().ToList())
                {
                    // Remove from dockPanel's parent if it has one
                    if (child is FrameworkElement frameworkElement)
                    {
                        // Detach the child from its current parent
                        var parent = VisualTreeHelper.GetParent(child) as Panel;
                        if (parent != null)
                        {
                            parent.Children.Remove(child);
                        }
                    }

                    _cachedDynamicFieldsPanel.Children.Add(child);
                }


                if (floatingPanelWindow == null)
                {
                    floatingPanelWindow = new FloatingPanelWindow
                    {
                        Owner = this // Set the main window as the owner to ensure proper z-order
                    };
                    floatingPanelWindow.UpdateDynamicFieldsUI(_cachedDynamicFieldsPanel);
                    floatingPanelWindow.Show();
                }
                else
                {
                    floatingPanelWindow.Activate();
                }
            }
        }

        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            UpdatePatientsFromDataGrid(); // Sync DataGrid changes to the patients collection

            if (dataGrid1.SelectedItem is DataRowView rowView)
            {
                var selectedPatient = patients[dataGrid1.Items.IndexOf(rowView)];

                foreach (var field in dynamicFields)
                {
                    string fieldName = field.Name;

                    switch (field.Type)
                    {
                        case Patient.FieldType.Text:
                            selectedPatient.FieldValues[fieldName] = (field.Control as TextBox)?.Text;
                            break;
                        case Patient.FieldType.Integer:
                            if (int.TryParse((field.Control as TextBox)?.Text, out var intValue))
                            {
                                selectedPatient.FieldValues[fieldName] = intValue;
                            }
                            else
                            {
                                selectedPatient.FieldValues[fieldName] = null;  // or default value
                            }
                            break;
                        case Patient.FieldType.Boolean:
                            selectedPatient.FieldValues[fieldName] = (field.Control as CheckBox)?.IsChecked ?? false;
                            break;
                        case Patient.FieldType.Choice:
                            selectedPatient.FieldValues[fieldName] = (field.Control as ComboBox)?.SelectedItem?.ToString();
                            break;
                        default:
                            selectedPatient.FieldValues[fieldName] = null;  // or handle other types as needed
                            break;
                    }
                }

                SavePatientsToExcel();
                LoadExcelData();
            }
            else
            {
                MessageBox.Show("Please select a patient to modify.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            UpdatePatientsFromDataGrid(); // Sync DataGrid changes to the patients collection

            var newPatient = new Patient();

            foreach (var field in dynamicFields)
            {
                string fieldName = field.Name;

                switch (field.Type)
                {
                    case Patient.FieldType.Text:
                        newPatient.FieldValues[fieldName] = (field.Control as TextBox)?.Text;
                        break;
                    case Patient.FieldType.Integer:
                        if (int.TryParse((field.Control as TextBox)?.Text, out var intValue))
                        {
                            newPatient.FieldValues[fieldName] = intValue;
                        }
                        else
                        {
                            newPatient.FieldValues[fieldName] = null;  // or default value
                        }
                        break;
                    case Patient.FieldType.Boolean:
                        newPatient.FieldValues[fieldName] = (field.Control as CheckBox)?.IsChecked ?? false;
                        break;
                    case Patient.FieldType.Choice:
                        newPatient.FieldValues[fieldName] = (field.Control as ComboBox)?.SelectedItem?.ToString();
                        break;
                    default:
                        newPatient.FieldValues[fieldName] = null;  // or handle other types as needed
                        break;
                }
            }

            patients.Add(newPatient);
            SavePatientsToExcel();
            LoadExcelData();

        }


        private void btncancel_Click(object sender, RoutedEventArgs e)
        {
            ClearInputFields();
        }

            private void ClearInputFields()
        {
            foreach (var field in dynamicFields)
            {
                switch (field.Type)
                {
                    case Patient.FieldType.Text:
                    case Patient.FieldType.Integer:
                        if (field.Control is TextBox textBox)
                        {
                            textBox.Clear();
                        }
                        break;
                    case Patient.FieldType.Boolean:
                        if (field.Control is CheckBox checkBox)
                        {
                            checkBox.IsChecked = false;
                        }
                        break;
                    case Patient.FieldType.Choice:
                        if (field.Control is ComboBox comboBox)
                        {
                            comboBox.SelectedIndex = -1;
                        }
                        break;
                }
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid1.SelectedItem is DataRowView rowView)
            {
                // Get the index of the selected patient
                int selectedIndex = dataGrid1.Items.IndexOf(rowView);

                // Check if the index is valid
                if (selectedIndex >= 0 && selectedIndex < patients.Count)
                {
                    // Remove the selected patient from the list
                    patients.RemoveAt(selectedIndex);

                    // Save the updated list to Excel
                    SavePatientsToExcel();

                    // Refresh the DataGrid
                    LoadExcelData();
                }
                else
                {
                    MessageBox.Show("No patient is selected or invalid selection.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a patient to delete.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SavePatientsToExcel()
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Patients");

            // Add headers
            for (int i = 0; i < dynamicFields.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = dynamicFields[i].Name;
            }

            // Add patient data
            for (int i = 0; i < patients.Count; i++)
            {
                var patient = patients[i];
                for (int j = 0; j < dynamicFields.Count; j++)
                {
                    string fieldName = dynamicFields[j].Name;

                    // Ensure the key exists in the FieldValues dictionary
                    if (patient.FieldValues.ContainsKey(fieldName))
                    {
                        worksheet.Cell(i + 2, j + 1).Value = patient.FieldValues[fieldName]?.ToString();
                    }
                    else
                    {
                        worksheet.Cell(i + 2, j + 1).Value = string.Empty; // or some default value
                    }
                }
            }

            workbook.SaveAs(excelFilePath);
            LoadExcelData();
        }

    }

    public class PatientFieldConfig
    {
        public List<PatientField> Fields { get; set; }
    }

    public class PatientField
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<string> Options { get; set; }
        // Property to display options as a single string
        [JsonIgnore] // Ignore this property when serializing to JSON
        public string OptionsString => Options != null ? string.Join(", ", Options) : string.Empty;
    }

    public class Patient
    {
        public Dictionary<string, object> FieldValues { get; set; }

        public Patient()
        {
            FieldValues = new Dictionary<string, object>();
        }

        public enum FieldType
        {
            Text,
            Integer,
            Boolean,
            Choice
        }

        public class DynamicField
        {
            public string Name { get; set; }
            public FieldType Type { get; set; }
            public UIElement Control { get; set; }
        }

    }
    public static class ConfigManager
    {
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PatientFieldsConfig.json");

        public static PatientFieldConfig LoadConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                return JsonConvert.DeserializeObject<PatientFieldConfig>(json);
            }
            return new PatientFieldConfig { Fields = new List<PatientField>() };
        }

        public static void SaveConfig(PatientFieldConfig config)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigFilePath, json);
        }
    }

}
