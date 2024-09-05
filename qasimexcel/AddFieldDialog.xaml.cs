using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PatientManagementApp
{
    public partial class AddFieldDialog : Window
    {
        public PatientField NewField { get; private set; }

        public AddFieldDialog()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Field name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewField = new PatientField
            {
                Name = nameTextBox.Text,
                Type = ((ComboBoxItem)typeComboBox.SelectedItem).Content.ToString(),
                Options = optionsTextBox.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(o => o.Trim()).ToList()
            };

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle visibility of the options TextBox based on the selected item
            var selectedItem = (ComboBoxItem)typeComboBox.SelectedItem;
            if (selectedItem != null)
            {
                string selectedType = selectedItem.Content.ToString();
                optionsTextBox.Visibility = (selectedType == "Boolean" || selectedType == "Choice")
                                            ? Visibility.Visible
                                            : Visibility.Collapsed;
            }
        }
    }
}
