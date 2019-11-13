#region copyright
// LuiGUI - PSD to Lui made easier
// Copyright (c) 2019 Ardivee
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion
using LuiGUI.Models;
using LuiGUI.Utils;
using LuiGUI.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.PSD;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace LuiGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// To open files
        /// </summary>
        private OpenFileDialog openFileDialog = new OpenFileDialog();

        /// <summary>
        /// To save files
        /// </summary>
        private SaveFileDialog saveFileDialog = new SaveFileDialog();

        /// <summary>
        /// The PSD file loaded
        /// </summary>
        private PsdFile psdFile = null;

        /// <summary>
        /// The selected HudElem from the ListView
        /// </summary>
        private HudElem selectedHudElem = null;

        /// <summary>
        /// The selected Image on the Canvas
        /// </summary>
        private Rectangle selectedPreviewImage = null;

        /// <summary>
        /// Lua Output Window
        /// </summary>
        private LuaOutputWindow luaOutputWindow;

        /// <summary>
        /// Zone Output Window
        /// </summary>
        private ZoneOutputWindow zoneOutputWindow;

        public MainWindow()
        {
            InitializeComponent();

            // Set default background for Canvas
            HudCanvas.Background = new ImageBrush(new BitmapImage(new Uri(@"pack://application:,,,/luigui_bg.png", UriKind.Absolute)));

            // We need to store this snippet somewhere
            /*
            var control = Application.Current.FindResource(typeof(ComboBox));
            using (XmlTextWriter writer = new XmlTextWriter(@"defaultTemplate.xml", System.Text.Encoding.UTF8))
            {
                writer.Formatting = System.Xml.Formatting.Indented;
                XamlWriter.Save(control, writer);
            }
            */
        }

        /// <summary>
        /// Load a PSD File
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenPSDMenu_Click(object sender, RoutedEventArgs e)
        {
            // Check if we already have loaded a PSD File
            if(psdFile != null)
            {
                MessageBox.Show("We already have a PSD loaded");
                return;
            }
            // TODO: Change text etc
            // Setup filters etc
            openFileDialog = new OpenFileDialog()
            {
                FileName = "Select a PSD File",
                Filter = "PSD file (*.psd)|*.psd",
                Title = "Open PSD"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Read the PSD File
                psdFile = new PsdFile().Load(openFileDialog.FileName);

                // Reverse  the layers, so we have them in the same order as PS
                List<Layer> reverseLayers = psdFile.Layers.ToList();
                reverseLayers.Reverse();

                // Loop through it's layers
                foreach(Layer layer in reverseLayers)
                {
                    // Do we skip invisible layers?
                    if (!layer.Visible && Properties.Settings.Default.SkipInvisiblePSDLayers)
                        continue;

                    // Get a Bitmap from current layer
                    var bmp = ImageDecoder.DecodeImage(layer);

                    // Get the downscale value for the imported PSD
                    var downScale = Properties.Settings.Default.PSDDownScaleImport;

                    // Do we have Bitmap ?
                    if(bmp != null)
                    {
                        // Create a BitmapSource
                        var bmpSrc = Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight((int)(bmp.Width / downScale), (int)(bmp.Height / downScale)));

                        // Create a Rectangle and fill with our BitmapSource
                        var rectangle = new Rectangle
                        {
                            Width = bmp.Width / downScale,
                            Height = bmp.Height / downScale,
                            Fill = new ImageBrush(bmpSrc),
                            Opacity = PSDUtil.CalculateOpacity(layer.Opacity),
                            StrokeThickness = 0,
                            Stroke = Brushes.Orange
                        };

                        // Add it to our Canvas
                        HudCanvas.Children.Add(rectangle);

                        // Set the positioning
                        Canvas.SetTop(rectangle, layer.Rect.Y / downScale);
                        Canvas.SetLeft(rectangle, layer.Rect.X / downScale);

                        // Setup our HudElem
                        var hudElem = new HudElem
                        {
                            Image = bmp,
                            HudPreviewImage = rectangle,
                            PSDLayerName = layer.Name,
                            LeftAnchor = true,
                            RightAnchor = false,
                            TopAnchor = true,
                            BottomAnchor = false,
                            Alpha = PSDUtil.CalculateOpacity(layer.Opacity),
                            IsText = false,
                        };

                        // Create a ListViewItem
                        var lvi = new ListViewItem
                        {
                            Content = layer.Name,
                            Tag = hudElem
                        };

                        // Attach a selected listener
                        lvi.Selected += ListViewItem_Selected;

                        // Add it to the list
                        HudItemList.Items.Add(lvi);
                    }
                }

                // Update the Z Index
                SetupZIndexOrder();
            }
        }

        /// <summary>
        /// Fires when the ListViewItem is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewItem_Selected(object sender, RoutedEventArgs e)
        {
            // Retrieve our ListViewItem
            var listViewItem = (ListViewItem)sender;
            // Retrieve our HudElem from the ListViewItem
            var hudElem = (HudElem)listViewItem.Tag;

            // Update our selected HudElem
            selectedHudElem = hudElem;

            // Get our Preview Image (Rectangle)
            var previewImage = hudElem.HudPreviewImage;

            if (selectedPreviewImage != null)
                selectedPreviewImage.StrokeThickness = 0;

            // Update our selected PreviewImage
            selectedPreviewImage = previewImage;
            selectedPreviewImage.StrokeThickness = 1;

            // Get our offsets
            double canvasX = Canvas.GetLeft(previewImage);
            double canvasY = Canvas.GetTop(previewImage);

            // Set our calculated Lui offsets
            LeftOffsetTxtBox.Text = LuiUtil.CalculateLeftOffsetValue(hudElem.LeftAnchor, hudElem.RightAnchor, canvasX).ToString();
            RightOffsetTxtBox.Text = LuiUtil.CalculateRightOffsetValue(hudElem.LeftAnchor, hudElem.RightAnchor, canvasX, previewImage.Width).ToString();

            TopOffsetTxtBox.Text = LuiUtil.CalculateTopOffsetValue(hudElem.TopAnchor, hudElem.BottomAnchor, canvasY).ToString();
            BottomOffsetTxtBox.Text = LuiUtil.CalculateBottomOffsetValue(hudElem.TopAnchor, hudElem.BottomAnchor, canvasY, previewImage.Height).ToString();

            // Update our Checkboxes
            LeftAnchorCheckBox.IsChecked = (selectedHudElem.LeftAnchor) ? true : false;
            RightAnchorCheckBox.IsChecked = (selectedHudElem.RightAnchor) ? true : false;

            TopAnchorCheckBox.IsChecked = (selectedHudElem.TopAnchor) ? true : false;
            BottomAnchorCheckBox.IsChecked = (selectedHudElem.BottomAnchor) ? true : false;

            // Update our Element / Image name
            ElementNameTxtBox.Text = hudElem.ElemName;
            ImageNameTxtBox.Text = hudElem.ImageName;

            // Update our Text part
            IsTextCheckBox.IsChecked = (selectedHudElem.IsText) ? true : false;
            HasTextPanel.Visibility = (selectedHudElem.IsText) ? Visibility.Visible : Visibility.Hidden;
            FontTextBox.Text = selectedHudElem.Text;
            FontComboBox.Text = selectedHudElem.FontName;

            Console.WriteLine(string.Format("Selected Layer: {0}", listViewItem.Content));
        }

        /// <summary>
        /// Fires when the window is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            // Save settings
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Exit the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Fires when a Checkbox is Checked / Unchecked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var txtBox = (CheckBox)sender;

            // Check if we have HudElem & PreviewImage
            if(selectedHudElem != null && selectedPreviewImage != null)
            {
                // Get our offsets
                double canvasX = Canvas.GetLeft(selectedPreviewImage);
                double canvasY = Canvas.GetTop(selectedPreviewImage);

                // Find our checkbox and update our offset values
                switch (txtBox.Name)
                {
                    case "LeftAnchorCheckBox":
                        selectedHudElem.LeftAnchor = (txtBox.IsChecked == true) ? true : false;
                        LeftOffsetTxtBox.Text = LuiUtil.CalculateLeftOffsetValue(selectedHudElem.LeftAnchor, selectedHudElem.RightAnchor, canvasX).ToString();
                        RightOffsetTxtBox.Text = LuiUtil.CalculateRightOffsetValue(selectedHudElem.LeftAnchor, selectedHudElem.RightAnchor, canvasX, selectedPreviewImage.Width).ToString();
                        break;
                    case "RightAnchorCheckBox":
                        selectedHudElem.RightAnchor = (txtBox.IsChecked == true) ? true : false;
                        LeftOffsetTxtBox.Text = LuiUtil.CalculateLeftOffsetValue(selectedHudElem.LeftAnchor, selectedHudElem.RightAnchor, canvasX).ToString();
                        RightOffsetTxtBox.Text = LuiUtil.CalculateRightOffsetValue(selectedHudElem.LeftAnchor, selectedHudElem.RightAnchor, canvasX, selectedPreviewImage.Width).ToString();
                        break;
                    case "TopAnchorCheckBox":
                        selectedHudElem.TopAnchor = (txtBox.IsChecked == true) ? true : false;
                        TopOffsetTxtBox.Text = LuiUtil.CalculateTopOffsetValue(selectedHudElem.TopAnchor, selectedHudElem.BottomAnchor, canvasY).ToString();
                        BottomOffsetTxtBox.Text = LuiUtil.CalculateBottomOffsetValue(selectedHudElem.TopAnchor, selectedHudElem.BottomAnchor, canvasY, selectedPreviewImage.Height).ToString();
                        break;
                    case "BottomAnchorCheckBox":
                        selectedHudElem.BottomAnchor = (txtBox.IsChecked == true) ? true : false;
                        TopOffsetTxtBox.Text = LuiUtil.CalculateTopOffsetValue(selectedHudElem.TopAnchor, selectedHudElem.BottomAnchor, canvasY).ToString();
                        BottomOffsetTxtBox.Text = LuiUtil.CalculateBottomOffsetValue(selectedHudElem.TopAnchor, selectedHudElem.BottomAnchor, canvasY, selectedPreviewImage.Height).ToString();
                        break;
                }
            }
        }

        /// <summary>
        /// Save the settings for our Layers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveLayerSettingsMenu_Click(object sender, RoutedEventArgs e)
        {
            // Create empty list for our HudElems
            List<HudElem> hudElems = new List<HudElem>();

            // Loop through the list and retrieve our HudElems
            foreach( ListViewItem lvi in HudItemList.Items )
            {
                var hudElem = (HudElem)lvi.Tag;
                hudElems.Add(hudElem);
            }

            // See if we have HudElems
            if( hudElems.Count > 0 )
            {
                saveFileDialog = new SaveFileDialog()
                {
                    Filter = "Layer Settings (*.luigui)|*.luigui",
                    Title = "Save Layer Settings"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Check if we have a valid FileName
                    if(saveFileDialog.FileName != "")
                    {
                        // Create our JSON object
                        var json = JsonConvert.SerializeObject(hudElems);

                        // Save our Layer Settings
                        File.WriteAllText(saveFileDialog.FileName, json);
                    }
                }
            }
        }

        /// <summary>
        /// Load the settings for our Layers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadLayerSettingsMenu_Click(object sender, RoutedEventArgs e)
        {
            if(psdFile == null)
            {
                MessageBox.Show("No PSD Loaded");
                return;
            }

            openFileDialog = new OpenFileDialog()
            {
                FileName = "Select a Layer Setting",
                Filter = "Layer Settings (*.luigui)|*.luigui",
                Title = "Open Layer Settings"
            };

            if(openFileDialog.ShowDialog() == true)
            {
                // Read our JSON file
                var json = System.IO.File.ReadAllText(openFileDialog.FileName);

                // Create a new list from our JSON Objects
                List<HudElem> hudElems = JsonConvert.DeserializeObject<List<HudElem>>(json);

                // Loop through all the Elements
                foreach(var hudElem in hudElems)
                {
                    // Loop through our List
                    foreach(ListViewItem lvi in HudItemList.Items)
                    {
                        // See if we have a Layer match
                        if( hudElem.PSDLayerName == (string)lvi.Content )
                        {
                            // Retrieve our old HudElem
                            var oldHudELem = (HudElem)lvi.Tag;

                            // Update the old values with our newly loaded values
                            oldHudELem.ElemName = hudElem.ElemName;
                            oldHudELem.ImageName = hudElem.ImageName;
                            oldHudELem.LeftAnchor = hudElem.LeftAnchor;
                            oldHudELem.RightAnchor = hudElem.RightAnchor;
                            oldHudELem.TopAnchor = hudElem.TopAnchor;
                            oldHudELem.BottomAnchor = hudElem.BottomAnchor;
                            oldHudELem.IsText = hudElem.IsText;
                            oldHudELem.Text = hudElem.Text;
                            oldHudELem.FontName = hudElem.FontName;
                            oldHudELem.FontFile = hudElem.FontFile;
                        }
                    }
                }

                // Try to get our selected ListViewItem
                ListViewItem selectedLvi = (ListViewItem)HudItemList.SelectedItem;

                // Check if we actually have one
                if(selectedLvi != null)
                {
                    // Refresh 
                    HudItemList.UnselectAll();
                    selectedLvi.IsSelected = true;
                    HudItemList.Focus();
                }

            }
        }

        /// <summary>
        /// Fires when the TextBox text has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtBox = (TextBox)sender;

            if(selectedHudElem != null)
            {
                switch(txtBox.Name)
                {
                    case "ElementNameTxtBox":
                        selectedHudElem.ElemName = txtBox.Text;
                        break;
                    case "ImageNameTxtBox":
                        selectedHudElem.ImageName = txtBox.Text;
                        break;
                }
            }
        }

        /// <summary>
        /// Show the Settings Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsMenu_Click(object sender, RoutedEventArgs e)
        {
            var window = new SettingsWindow();
            window.Show();
        }

        /// <summary>
        /// Change the Canvas Background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeBackgroundMenu_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog = new OpenFileDialog()
            {
                FileName = "Select an Image",
                Filter = "Image (*.png)|*.png",
                Title = "Change Background"
            };

            if (openFileDialog.ShowDialog() == true)
            {

                // Create new BitmapImage
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(openFileDialog.FileName);
                bitmap.EndInit();

                // Set the Background
                HudCanvas.Background = new ImageBrush(bitmap);

            }
        }

        /// <summary>
        /// Reset everything back to default
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewMenu_Click(object sender, RoutedEventArgs e)
        {
            // Clear our ListView, Canvas, Images
            HudItemList.Items.Clear();
            HudCanvas.Children.Clear();

            // Set all back to null
            psdFile = null;
            selectedHudElem = null;
            selectedPreviewImage = null;
        }

        /// <summary>
        /// Generate LUA code from the PSD Layers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateLuaMenu_Click(object sender, RoutedEventArgs e)
        {
            if(psdFile == null)
            {
                MessageBox.Show("Nothing to generate");
                return;
            }

            StringBuilder sb = new StringBuilder();

            // Reverse the list, so the Z Order is correct
            List<ListViewItem> hudItems = HudItemList.Items.Cast<ListViewItem>().ToList();
            hudItems.Reverse();

            foreach (ListViewItem listViewItem in hudItems)
            {
                HudElem hudElem = (HudElem)listViewItem.Tag;

                var previewImage = hudElem.HudPreviewImage;

                // Get our offsets
                double canvasX = Canvas.GetLeft(previewImage);
                double canvasY = Canvas.GetTop(previewImage);

                // Start writing our Code
                // See if the Element is Text or an Image
                if(hudElem.IsText)
                {
                    // The padding that might be used ingame??
                    var textPadding = Math.Round(previewImage.Height / 4);

                    Console.WriteLine("Text Height: " + previewImage.Height);

                    sb.AppendLine(string.Format("local {0} = LUI.UIText.new(HudRef, InstanceRef)", hudElem.ElemName));

                    sb.AppendLine(string.Format("{0}:setLeftRight({1}, {2}, {3}, {4})", hudElem.ElemName,
                        hudElem.LeftAnchor.ToString().ToLower(), hudElem.RightAnchor.ToString().ToLower(),
                        LuiUtil.CalculateLeftOffsetValue(hudElem.LeftAnchor, hudElem.RightAnchor, canvasX).ToString(),
                        LuiUtil.CalculateRightOffsetValue(hudElem.LeftAnchor, hudElem.RightAnchor, canvasX, previewImage.Width).ToString()));

                    sb.AppendLine(string.Format("{0}:setTopBottom({1}, {2}, {3}, {4})", hudElem.ElemName,
                        hudElem.TopAnchor.ToString().ToLower(), hudElem.BottomAnchor.ToString().ToLower(),
                        LuiUtil.CalculateTopOffsetValue(hudElem.TopAnchor, hudElem.BottomAnchor, canvasY - textPadding).ToString(),
                        LuiUtil.CalculateBottomOffsetValue(hudElem.TopAnchor, hudElem.BottomAnchor, canvasY + textPadding, previewImage.Height).ToString()));

                    sb.AppendLine(string.Format("{0}:setTTF(\"fonts/{1}\")", hudElem.ElemName, hudElem.FontFile));

                    sb.AppendLine(string.Format("{0}:setText(\"{1}\")", hudElem.ElemName, hudElem.Text));

                    sb.AppendLine(string.Format("{0}:setAlpha({1})", hudElem.ElemName, hudElem.Alpha.ToString()));

                    sb.AppendLine(string.Format("{0}:addElement({1})", "Elem", hudElem.ElemName));

                    sb.AppendLine("");

                } else
                {
                    sb.AppendLine(string.Format("local {0} = LUI.UIImage.new()", hudElem.ElemName));

                    sb.AppendLine(string.Format("{0}:setLeftRight({1}, {2}, {3}, {4})", hudElem.ElemName,
                        hudElem.LeftAnchor.ToString().ToLower(), hudElem.RightAnchor.ToString().ToLower(),
                        LuiUtil.CalculateLeftOffsetValue(hudElem.LeftAnchor, hudElem.RightAnchor, canvasX).ToString(),
                        LuiUtil.CalculateRightOffsetValue(hudElem.LeftAnchor, hudElem.RightAnchor, canvasX, previewImage.Width).ToString()));

                    sb.AppendLine(string.Format("{0}:setTopBottom({1}, {2}, {3}, {4})", hudElem.ElemName,
                        hudElem.TopAnchor.ToString().ToLower(), hudElem.BottomAnchor.ToString().ToLower(),
                        LuiUtil.CalculateTopOffsetValue(hudElem.TopAnchor, hudElem.BottomAnchor, canvasY).ToString(),
                        LuiUtil.CalculateBottomOffsetValue(hudElem.TopAnchor, hudElem.BottomAnchor, canvasY, previewImage.Height).ToString()));

                    sb.AppendLine(string.Format("{0}:setImage(RegisterImage(\"{1}\"))", hudElem.ElemName, hudElem.ImageName));

                    sb.AppendLine(string.Format("{0}:setAlpha({1})", hudElem.ElemName, hudElem.Alpha.ToString()));

                    sb.AppendLine(string.Format("{0}:addElement({1})", "Elem", hudElem.ElemName));

                    sb.AppendLine("");
                }
            }

            // See if the Output Window is still active
            if (luaOutputWindow == null || !luaOutputWindow.IsActive)
            {
                // Create new Output Window and update the Text
                luaOutputWindow = new LuaOutputWindow();
                luaOutputWindow.LuaOutputText.Text = sb.ToString();
                luaOutputWindow.Show();
            }
            else
            {
                // Just update the Text
                luaOutputWindow.LuaOutputText.Text = sb.ToString();
            }
        }

        /// <summary>
        /// Export images from the loaded PSD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportImagesMenu_Click(object sender, RoutedEventArgs e)
        {
            // Check if we have Items
            if (HudItemList.Items.Count > 0)
            {
                // Select an export folder
                var dialog = new FolderSelectDialog
                {
                    Title = "Select the export folder"
                };

                if (dialog.Show())
                {
                    if (!string.IsNullOrWhiteSpace(dialog.FileName))
                    {
                        foreach( ListViewItem lvi in HudItemList.Items )
                        {
                            // Retrieve our Huelem
                            var hudElem = (HudElem)lvi.Tag;

                            // We don't need to export Text as an Image
                            if (hudElem.IsText)
                                continue;

                            // Get our Image Name
                            var imageName = hudElem.ImageName;
                            
                            // Check if we have setup an ImageName or have the setting
                            if (string.IsNullOrWhiteSpace(imageName) || !Properties.Settings.Default.ExportWithImageName)
                                imageName = hudElem.PSDLayerName;

                            var extension = ".png";
                            var format = System.Drawing.Imaging.ImageFormat.Png;

                            // Check our Image Export Type
                            if (Properties.Settings.Default.ImageExportType == ".tif")
                            {
                                extension = ".tif";
                                format = System.Drawing.Imaging.ImageFormat.Tiff;
                            }

                            // Save the image to png
                            hudElem.Image.Save(System.IO.Path.Combine(dialog.FileName, imageName + extension), format);
                        }
                        // Let the user know the export is done
                        MessageBox.Show("Export done");
                    }
                }
            } else
            {
                MessageBox.Show("Nothing to export");
            }
        }

        /// <summary>
        /// Set the Z Index of the Images
        /// </summary>
        private void SetupZIndexOrder()
        {
            // Get the Items
            int count = HudItemList.Items.Count;

            // Loop through all the items
            foreach (ListViewItem listViewItem in HudItemList.Items)
            {
                // Retrieve the HudElem
                HudElem hudELem = (HudElem)listViewItem.Tag;

                if (hudELem != null)
                {
                    // Retrieve the Image
                    Rectangle rectangle = hudELem.HudPreviewImage;
                    // Set the Z Index
                    Canvas.SetZIndex(rectangle, count);
                }

                count--;
            }
        }

        /// <summary>
        /// Event fires when a Key is down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void keyDown(object sender, KeyEventArgs e)
        {
            // See if the Escape key is pressed
            if (e.Key == Key.Escape)
            {
                if (selectedPreviewImage != null)
                    selectedPreviewImage.StrokeThickness = 0;
            }
        }

        /// <summary>
        /// Fires when the Window is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Monitor KeyPresses
            EventManager.RegisterClassHandler(typeof(Window),Keyboard.KeyDownEvent, new KeyEventHandler(keyDown), true);

            // List to keep track of unique font names
            HashSet<string> fontNames = new HashSet<string>();

            // default font list size
            int fontCount = 0;

            // Get the font folder
            var fontFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

            // Get all the fonts
            var fonts = System.IO.Directory.GetFiles(fontFolder, "*.ttf");

            // Loop through all the found fonts
            foreach (var font in fonts)
            {
                using( PrivateFontCollection fontCol = new PrivateFontCollection())
                {
                    // Add it to our collection
                    fontCol.AddFontFile(font);

                    // Check if we found a valid font
                    if (fontCol.Families.Length > 0)
                    {
                        // Get the font name
                        var actualFontName = fontCol.Families[0].Name;

                        // Add it to our unique list
                        fontNames.Add(actualFontName);

                        // Make sure  the size has increased and not a duplicate Font
                        if (fontNames.Count > fontCount)
                        {
                            // Create a ComboBoxItem and add to our ComboBox
                            var comboBoxItem = new ComboBoxItem();
                            comboBoxItem.Content = actualFontName;
                            comboBoxItem.Tag = System.IO.Path.GetFileName(font);

                            FontComboBox.Items.Add(comboBoxItem);

                            // Increase the count since we have a new unique font
                            fontCount++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fires when the Checkbox is Checked / Unchecked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IsTextCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;

            if(selectedHudElem != null)
            {
                selectedHudElem.IsText = (checkBox.IsChecked == true) ? true : false;
                HasTextPanel.Visibility = (checkBox.IsChecked == true) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        /// <summary>
        /// Fires when the TextBox Text has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;

            if(selectedHudElem != null)
            {
                selectedHudElem.Text = textBox.Text;
            }
        }

        /// <summary>
        /// Fires when the ComboBox Selection has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBoxItem = (ComboBoxItem)((ComboBox)sender).SelectedItem;

            if(selectedHudElem != null && comboBoxItem != null)
            {
                selectedHudElem.FontName = (string)comboBoxItem.Content;
                selectedHudElem.FontFile = (string)comboBoxItem.Tag;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateZoneMenu_Click(object sender, RoutedEventArgs e)
        {
            if (psdFile == null)
            {
                MessageBox.Show("Nothing to generate");
                return;
            }

            StringBuilder sb = new StringBuilder();
            HashSet<string> fonts = new HashSet<string>();
            HashSet<string> images = new HashSet<string>();

            // Reverse the list, so the Z Order is correct
            List<ListViewItem> hudItems = HudItemList.Items.Cast<ListViewItem>().ToList();
            hudItems.Reverse();

            foreach (ListViewItem listViewItem in hudItems)
            {
                HudElem hudElem = (HudElem)listViewItem.Tag;

                // Start writing our Code
                // See if the Element is Text or an Image
                if (hudElem.IsText)
                {
                    fonts.Add(hudElem.FontFile);
                }
                else
                {
                    images.Add(hudElem.ImageName);
                }
            }

            // Check if we actually have some Fonts
            if(fonts.Count > 0)
            {
                foreach(var font in fonts)
                {
                    sb.AppendLine(string.Format("ttf,fonts/{0}", font));
                }
                
            }

            // Check if we actually have some Images
            if (images.Count > 0)
            {
                foreach(var image in images)
                {
                    sb.AppendLine(string.Format("image,{0}", image));
                }
            }

            // See if the Output Window is still active
            if (zoneOutputWindow == null || !zoneOutputWindow.IsActive)
            {
                // Create new Output Window and update the Text
                zoneOutputWindow = new ZoneOutputWindow();
                zoneOutputWindow.ZoneOutputText.Text = sb.ToString();
                zoneOutputWindow.Show();
            }
            else
            {
                // Just update the Text
                zoneOutputWindow.ZoneOutputText.Text = sb.ToString();
            }
        }

        /// <summary>
        /// Shows the About Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AboutMenu_Click(object sender, RoutedEventArgs e)
        {
            // Show the About Window
            var aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }
    }
}
