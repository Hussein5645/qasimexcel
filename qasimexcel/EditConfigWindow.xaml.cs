using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows;

namespace PatientManagementApp
{
    public partial class EditConfigWindow : Window
    {
        private PatientFieldConfig config;
        private ObservableCollection<PatientField> fieldsCollection;

        public EditConfigWindow()
        {
            InitializeComponent();
            LoadConfig();
            fieldsCollection = new ObservableCollection<PatientField>(config.Fields);
            fieldsListView.ItemsSource = fieldsCollection;
        }

        private void LoadConfig()
        {
            config = ConfigManager.LoadConfig();
            if (config == null)
            {
                config = new PatientFieldConfig { Fields = new List<PatientField>() };
            }
        }

        private void AddField_Click(object sender, RoutedEventArgs e)
        {
            AddFieldDialog addFieldDialog = new AddFieldDialog();
            if (addFieldDialog.ShowDialog() == true)
            {
                var newField = addFieldDialog.NewField;
                fieldsCollection.Add(newField);
            }
        }


        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            config.Fields = new List<PatientField>(fieldsCollection);
            ConfigManager.SaveConfig(config);
            MessageBox.Show("Configuration saved successfully.");
            
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void fieldsListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Enable or disable the delete button based on selection
            deleteButton.IsEnabled = fieldsListView.SelectedItem != null;
        }

        private void DeleteField_Click(object sender, RoutedEventArgs e)
        {
            if (fieldsListView.SelectedItem is PatientField selectedField)
            {
                fieldsCollection.Remove(selectedField);
            }
        }
    }
}
