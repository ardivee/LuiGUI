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
using System.Windows.Shapes;

namespace LuiGUI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Fires when the Window is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load our settings
            SkipInvisibleLayersCheckBox.IsChecked = Properties.Settings.Default.SkipInvisiblePSDLayers;
            ExportImagesAsImageNameCheckBox.IsChecked = Properties.Settings.Default.ExportWithImageName;
            ImageTypeComboBox.Text = (Properties.Settings.Default.ImageExportType == ".png") ? "PNG" : "TIF";
            PSDResolutionTypeComboBox.Text = GetPSDResolutionName(Properties.Settings.Default.PSDDownScaleImport);
        }

        /// <summary>
        /// Fires when the Checkbox is checked or unchecked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SkipInvisibleLayersCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;

            Properties.Settings.Default.SkipInvisiblePSDLayers = (checkBox.IsChecked == true) ? true : false;
        }

        /// <summary>
        /// Fire when the Window is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            // Save settings
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Fires when the Checkbox is checked or unchecked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportImagesAsImageNameCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;

            Properties.Settings.Default.ExportWithImageName = (checkBox.IsChecked == true) ? true : false;
        }

        /// <summary>
        /// Fires when the ComboBox selection is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBoxItem = (ComboBoxItem)((ComboBox)sender).SelectedItem;

            if ((string)comboBoxItem.Content == "PNG")
                Properties.Settings.Default.ImageExportType = ".png";
            else
                Properties.Settings.Default.ImageExportType = ".tif";
        }

        /// <summary>
        /// Fires when the ComboBox selection is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PSDResolutionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBoxItem = (ComboBoxItem)((ComboBox)sender).SelectedItem;

            switch((string)comboBoxItem.Content)
            {
                case "1280 x 720":
                    Properties.Settings.Default.PSDDownScaleImport = 1;
                    break;
                case "1920 x 1080":
                    Properties.Settings.Default.PSDDownScaleImport = 1.5;
                    break;
                case "2560 x 1440":
                    Properties.Settings.Default.PSDDownScaleImport = 2;
                    break;
            }
        }

        /// <summary>
        /// Get the PSD resolution Name from the Scale
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        private string GetPSDResolutionName( double scale )
        {
            switch(scale)
            {
                case 1:
                    return "1280 x 720";
                case 1.5:
                    return "1920 x 1080";      
            }

            return "2560 x 1440";
        }
    }
}
