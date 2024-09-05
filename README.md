# Doctor/Patient Management System

This repository contains a Doctor/Patient Management System, a WPF application designed to assist doctors in managing and analyzing patient data. The system is flexible, allowing doctors to configure patient fields dynamically through a `PatientFieldsConfig.json` file. This adaptability makes the system suitable for various use cases and practices.

## Features

- **Add & Manage Patients**: Doctors can add new patients and manage existing records. The system starts with basic fields (e.g., Name, Age, Sex) and can expand with additional fields dynamically.
  
- **Customizable Patient Fields**: Patient data fields are defined in the `PatientFieldsConfig.json` file, allowing users to customize fields such as text inputs, dropdowns, and boolean values. The system supports:
  - Text fields
  - Integer fields
  - Choice fields (dropdowns)

- **Visual Data Analysis**: The system includes a visual analysis tool that enables doctors to analyze patient data. Key analyses include:
  - **Choice Fields**: View how many patients selected each option.
  - **Integer Fields**: Analyze numerical data with statistics (minimum, maximum, average) and visual charts.

- **Field Configuration**: By modifying the `PatientFieldsConfig.json`, doctors can adjust which fields appear when adding new patients. This makes the system highly flexible for various medical practices.

## How to Use

1. **Clone the repository**:
    ```bash
    git clone <[repository-url](https://github.com/Hussein5645/qasimexcel.git)>
    ```

2. **Set up the environment**:
   - Ensure you have the required dependencies (e.g., WPF, ClosedXML, OpenXML) installed.

3. **Modify PatientFieldsConfig.json**:
   - Define the fields you'd like to appear for patient records in the `PatientFieldsConfig.json` file. Supported field types are `Text`, `Integer`, `Choice`, and `MultiChoice`.

4. **Run the application**:
   - Open the project in Visual Studio and build the solution. The application allows you to manage patient data dynamically based on the fields defined in `PatientFieldsConfig.json`.

## Files and Structure

- **PatientFieldsConfig.json**: Configuration file defining the fields that appear in the patient data entry form. Modify this file to customize the fields for your practice.
- **MainWindow.xaml**: Main interface where patient data is displayed and managed.
- **AddPatientWindow.xaml**: The window for adding new patients, with dynamic field generation based on the configuration file.
- **AnalysisWindow.xaml**: Visual analysis interface for analyzing patient data, providing statistics and visual charts.

