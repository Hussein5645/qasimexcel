using System.Windows;
using System.Windows.Controls;

namespace PatientManagementApp
{
    public partial class FloatingPanelWindow : Window
    {
        public FloatingPanelWindow()
        {
            InitializeComponent();
        }

        public void UpdateDynamicFieldsUI(FrameworkElement dynamicFields)
        {
            // Ensure that dynamicFields is not already part of another visual tree
            if (dynamicFields.Parent is Panel currentParent)
            {
                currentParent.Children.Remove(dynamicFields);
            }

            // Clear the existing children in the panel
            dynamicFieldsPanelFloating.Children.Clear();

            // Add the new dynamicFields element
            dynamicFieldsPanelFloating.Children.Add(dynamicFields);
        }

    }
}
