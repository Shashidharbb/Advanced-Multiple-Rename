using Microsoft.Win32;
using System;
using System.Windows;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.ComponentModel;
using System.Resources;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

using Ookii.Dialogs.Wpf; // ooki Dialogs for folderdialog
using System.Windows.Forms; // ooki package : contains windows.Forms is Need to Folderdialog
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Threading;

namespace Advanced_Multiple_Rename
{
    

    /// <summary>
    /// Interaction logic for RenameController.xaml
    /// </summary>
    public partial class RenameController : System.Windows.Controls.UserControl
    {
        public RenameController()
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

            InitializeComponent();
            DefultValues();

        }



        public static string datetimeFormats = CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern.ToString();
       
        List<filesExtensions> filesExtensions_List = null;  // add selected folder file extensions.
        ObservableCollection<Grid_data_Info> GridDataSource = null;
        public string Caller = "";
        List<FileInfo> FilesInfo = null;
        List<DirectoryInfo> FoldersInfo = null;
        DirectoryInfo DirInfo = null;
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        string MultipleRename = ResourceFile.Multiple_rename;
        bool Caller_isFile = true;

        List<string> Failure_list = new List<string>();
        List<Grid_data_Info> selRowlist1 = new List<Grid_data_Info>();

        internal void DefultValues()
        {
            Grid2.Visibility = Visibility.Hidden;
            List<PrefixClass> sList = new List<PrefixClass>();
            sList.Add(new PrefixClass { ID = 1, Name = "Add Auto-Increment Prefix Numbers" });
            sList.Add(new PrefixClass { ID = 2, Name = "Enter Custom First Prefix Numbers---> After Auto-Increment itself" });
            sList.Add(new PrefixClass { ID = 3, Name = "Enter Custom First Alpha & Numeric Values---> After Auto-Increment itself" });
            sList.Add(new PrefixClass { ID = 4, Name = "Enter Constant Cutsom Prefix" });

            prefixCombo.ItemsSource = sList;
            prefixCombo.SelectedValuePath = "ID";
            prefixCombo.DisplayMemberPath = "Name";


            List<RenameType> rList = new List<RenameType>();
            rList.Add(new RenameType { ID = 1, Name = "Rename By Adding Prefix" });
            rList.Add(new RenameType { ID = 2, Name = "Rename By Adding Suffix" });
            rList.Add(new RenameType { ID = 3, Name = "Complete Rename" });

            RenameTypeCombo.ItemsSource = rList;
            RenameTypeCombo.SelectedValuePath = "ID";
            RenameTypeCombo.DisplayMemberPath = "Name";

            Display_DataGrid.Visibility = Visibility.Hidden;

            StartButton.Visibility = Visibility.Hidden;
            progressbar.Visibility = Visibility.Hidden;
            CancelButton.Visibility = Visibility.Hidden;

            List<SortBy_Details> sortList = new List<SortBy_Details>();
            sortList.Add(new SortBy_Details { ID = 1, Name = "Rename By Date Created" });
            sortList.Add(new SortBy_Details { ID = 2, Name = "Rename By Date Modified" });

            SortCombo.ItemsSource = sortList;
            SortCombo.SelectedValuePath = "ID";
            SortCombo.DisplayMemberPath = "Name";
            SortCombo.SelectedIndex = 0;
            SortBy_Details combo = SortCombo.SelectedItem as SortBy_Details;
            SortCombo.ToolTip = combo.Name;


        }


        public string getSize(double length)
        {

            double len = length;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                len = len / 1024;
            }

            return String.Format("{0:0.##} {1}", len, sizes[order]).ToString();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            #region Work around 
            /**** work around for open folder using openfildeDialog  which have certain limitations
             * 
             * 
            OpenFileDialog openFileDlg = new OpenFileDialog();
            openFileDlg.Filter = "Folders|\n";
            openFileDlg.AddExtension = false;
            openFileDlg.CheckFileExists = false;
            openFileDlg.DereferenceLinks = true;
            openFileDlg.Multiselect = false;
            openFileDlg.ValidateNames = false;
            openFileDlg.FileName = "Folder Selection.";

             launch openfiledialog by calling showdialog method
            nullable<bool> result = openfiledlg.showdialog();
             get the selected file name and display in a textbox.
             load content of file in a textblock
            if (result == true)
            {
                textbox_path.text = openfiledlg.filename;
                S textblock1.text = system.io.file.readalltext(openfiledlg.filename);
            string folderpath = system.io.path.getfullpath(openfiledlg.filename);

               }
          ***************/
            #endregion

            FileExt_Combobox.ItemsSource = null;
            FileExt_Combobox.Text = "";
            comboxLabel.Content = "Select";
            TextBox_Path.Text = "";
            RenameTypeCombo.SelectedIndex = -1;
            prefixCombo.SelectedIndex = -1;
            Display_DataGrid.ItemsSource = null;
            prefixCombo.Visibility = Visibility.Hidden;
            CustomLabel.Visibility = Visibility.Hidden;
            comboxLabel.Visibility = Visibility.Hidden;
            DefultValues();
            HideAll_CustomPrefix_Options();


            Caller_isFile = Caller.ToLower() == "files" ? true : false;

            /****** using  Ookii.Dialogs.Wpfnuget package for folderbrowserDialog *****/
            FolderBrowserDialog openfolderDlg = new FolderBrowserDialog();
            DialogResult result = openfolderDlg.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(openfolderDlg.SelectedPath))
            {
                // FolderFiles = (Caller.Trim().Length > 0 && Caller.ToLower() == "files") ? Directory.GetFiles(openfolderDlg.SelectedPath).ToList<string>() : Directory.GetDirectories(openfolderDlg.SelectedPath).ToList<string>();
                DirInfo = new DirectoryInfo(openfolderDlg.SelectedPath);
                if (Caller != null && Caller.ToLower() == "files" && DirInfo.Exists)
                {
                    FilesInfo = DirInfo.GetFiles().ToList<FileInfo>();

                    if (FilesInfo == null || FilesInfo.Count == 0)
                    {
                        Grid2.Visibility = Visibility.Hidden;
                        if (Caller.Trim().Length > 0 && Caller.ToLower() == "files")
                        {
                            System.Windows.Forms.MessageBox.Show("No Files found: Please  selected a folder containes files", MultipleRename);
                            return;
                        }
                    }
                    else
                    {
                        Grid2.Visibility = Visibility.Visible;
                        checkbox_Asc.IsChecked = true;
                        if (Caller.Trim().Length > 0 && Caller.ToLower() == "files")
                        {
                            CreateCombobox_list();
                            FileExt_Combobox.ItemsSource = filesExtensions_List;
                            if (filesExtensions_List.Count == 1)
                            {
                                FileExt_Combobox.SelectedIndex = 0;
                                FileExt_Combobox.Text = filesExtensions_List[0].ExtensionName;
                            }
                        }
                    }
                }
                else if (Caller != null && Caller.ToLower() != "files" && DirInfo.Exists)
                {
                    FoldersInfo = DirInfo.GetDirectories().ToList<DirectoryInfo>();
                    if (FoldersInfo == null || FoldersInfo.Count == 0)
                    {
                        System.Windows.Forms.MessageBox.Show("No Folders found: Please  selected a folder containes folders", MultipleRename);
                        return;
                    }
                    else
                    {
                        Grid2.Visibility = Visibility.Visible;
                        checkbox_Asc.IsChecked = true;
                        comboxLabel.Visibility = Visibility.Hidden;
                        label_fileType.Visibility = Visibility.Hidden;
                        FileExt_Combobox.Visibility = Visibility.Hidden;
                    }

                }
                TextBox_Path.Text = Path.GetFullPath(openfolderDlg.SelectedPath);

                if (Caller != null && Caller.ToLower() == "files")
                {
                    if(filesExtensions_List.Count > 1)
                    Display_DataGrid.Visibility = Visibility.Hidden;
                }
                else
                {
                    Display_DataGrid.Visibility = Visibility.Visible;
                    PopulateDataGrid("");
                }

            }

        }


        /// <summary>
        /// This method add is used to add extension in the combobox list
        /// </summary>
        private void CreateCombobox_list()
        {
            filesExtensions_List = new List<filesExtensions>();  // add selected folder file extensions.
            List<string> sExtensionList = new List<string>();
            if (FilesInfo != null && FilesInfo.Count > 1)
            {

                for (int i = 0; i < FilesInfo.Count; i++)
                {
                    FileInfo filepath = FilesInfo[i];
                    string attributes = filepath.Attributes.ToString();
                    if (attributes.Contains("Hidden"))
                    {
                        continue;
                    }
                    string extension = Path.GetExtension(filepath.FullName);

                    if (sExtensionList.Count > 0 && !sExtensionList.Contains(extension))
                    {
                        sExtensionList.Add(extension);

                    }
                    else if (sExtensionList.Count == 0)
                    {
                        sExtensionList.Add(extension);
                    }
                }

                if (sExtensionList.Count > 0)
                {
                    if (sExtensionList.Count > 1)
                    {
                        AddFilesExenionList("ALL", 0, false);
                        for (int i = 0; i < sExtensionList.Count; i++)
                        {
                            string Exten = sExtensionList[i];
                            AddFilesExenionList(Exten, sExtensionList.Count, false);
                        }
                    }
                    else if(sExtensionList.Count ==1)
                    {
                        string Exten = sExtensionList[0];
                        comboxLabel.Visibility = Visibility.Hidden;
                        comboxLabel.Content = Exten;
                        AddFilesExenionList(Exten, 1, true);
                        PopulateDataGrid(Exten);
                        Display_DataGrid.Visibility = Visibility.Visible;
                    }
                   
                }


            }
            else if (FilesInfo != null && FilesInfo.Count == 1)
            {
                FileInfo filepath = FilesInfo[0];
                string attributes = filepath.Attributes.ToString();
                if (!attributes.Contains("Hidden"))
                {
                    string extension = Path.GetExtension(filepath.FullName);
                    comboxLabel.Visibility = Visibility.Hidden;
                    comboxLabel.Content = extension;
                    AddFilesExenionList(extension, 1, true);
                    PopulateDataGrid(extension);
                    Display_DataGrid.Visibility = Visibility.Visible;
                }

            }

        }

        /// <summary>
        /// take 3 argumnets file Extneison  name , check box sataus
        /// to make default check box check send true to @checkboxStatus
        /// ID is any non duplicate integer numbers, 
        /// </summary>
        /// <param name="extensionName"></param>
        /// <param name="ID"></param>
        /// <param name="checkboxStatus"></param>
        private void AddFilesExenionList(string extensionName, int ID, bool checkboxStatus)
        {
            filesExtensions obj = new filesExtensions();
            obj.ID = ID;
            obj.ExtensionName = extensionName;
            obj.Check_Status = checkboxStatus;
            filesExtensions_List.Add(obj);
        }

        private void FileExentCombobox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (FileExt_Combobox.Text.Trim().Length > 0)
                FileExt_Combobox.ItemsSource = filesExtensions_List.Where(x => x.ExtensionName.Contains(FileExt_Combobox.Text.Trim()));

        }

        private void ComboBox_CheckedAndUnchecked(object sender, RoutedEventArgs e)
        {
            /** Remember due to ooki dialogs we used windows forms also so please mention clearly the control type in cast  *****/
            System.Windows.Controls.CheckBox box1 = sender as System.Windows.Controls.CheckBox;
            if (box1.IsChecked == true && box1.Content.ToString() == "ALL")
            {
                FileExt_Combobox.ItemsSource = filesExtensions_List.Where(x => x.Check_Status = true);

            }
            else if (box1.IsChecked == false && box1.Content.ToString() == "ALL")
            {
                FileExt_Combobox.ItemsSource = filesExtensions_List.Where(x => x.Check_Status = false);
            }

            string ComboboxText = GetFileExt_ComboBoxSelectedItems();

            if (ComboboxText.Length == 0)
            {
                FileExt_Combobox.SelectedIndex = -1;
                FileExt_Combobox.ItemsSource = filesExtensions_List;
            }
            else
            {
                comboxLabel.Content = ComboboxText;
            }


        }

        /// <summary>
        /// Returns the file extension in string format
        /// if multiple file types are selected  file extensions are seprated with comma
        /// </summary>
        /// <returns> file extension in string format with comma separated </returns>
        protected string GetFileExt_ComboBoxSelectedItems()
        {
            string selectedItmes = "";
            if (FileExt_Combobox.ItemsSource != null)
            {

                foreach (var item in FileExt_Combobox.ItemsSource)
                {
                    var item1 = item as filesExtensions;
                    if (item1.Check_Status == true && item1.ExtensionName.ToString() != "ALL")
                    {
                        selectedItmes += selectedItmes.Length > 0 ? (", " + item1.ExtensionName) : item1.ExtensionName;
                    }
                    else if (item1.Check_Status == true && item1.ExtensionName.ToLower() == "all")
                    {
                        selectedItmes = item1.ExtensionName;
                        return selectedItmes;
                    }

                }
            }
            return selectedItmes;
        }

        private void comboxLabel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            FileExt_Combobox.IsDropDownOpen = true;
            comboxLabel.Visibility = Visibility.Hidden;
        }

        private void FileExt_ComboboxClosed(object sender, EventArgs e)
        {
            comboxLabel.Visibility = Visibility.Visible;
            FileExt_Combobox.Text = "";
            string ComboboxText = GetFileExt_ComboBoxSelectedItems();
            FileExt_Combobox.ItemsSource = filesExtensions_List; // Due to serach filter , need to reassigne the all dataSouce once again 

            comboxLabel.Content = ComboboxText.Length == 0 ? "Select" : ComboboxText;
            
            comboxLabel.ToolTip = comboxLabel.Content;
            
            dummyTB.Focus();
            if (ComboboxText.Length > 0)
            {
                Display_DataGrid.Visibility = Visibility.Visible;
                PopulateDataGrid(ComboboxText);
            }
            else
            {
                Display_DataGrid.ItemsSource = null;
                Display_DataGrid.Visibility = Visibility.Hidden;
                StartButton.Visibility = Visibility.Hidden;
            }

        }



        private void FileExt_Combobox_DropDownOpened(object sender, EventArgs e)
        {
            comboxLabel.Visibility = Visibility.Hidden;
            
        }
        private void checkBox_Asc_checked(object sender, RoutedEventArgs e)
        {
            checkbox_Desc.IsChecked = false;
        }

        private void checkBox_Dsc_checked(object sender, RoutedEventArgs e)
        {
            checkbox_Asc.IsChecked = false;
        }

        private void checkBox_Asc_unchecked(object sender, RoutedEventArgs e)
        {
            checkbox_Desc.IsChecked = true;

        }
        private void checkBox_Dsc_unchecked(object sender, RoutedEventArgs e)
        {
            checkbox_Asc.IsChecked = true;

        }

        /// <summary>
        /// update prefix /sufix based on Rename type combobox selection 
        /// </summary>
        /// <param name="selected"></param>
        private void updateprefix_ComboBox(RenameType selected)
        {
            if (selected != null)
            {
                List<PrefixClass> sList = new List<PrefixClass>();

                if (selected.ID == 1)
                {
                    sList.Add(new PrefixClass { ID = 1, Name = "Add Auto-Increment Prefix Numbers" });
                    sList.Add(new PrefixClass { ID = 2, Name = "Enter Custom First Prefix Numbers---> After Auto-Increment itself" });
                    sList.Add(new PrefixClass { ID = 3, Name = "Enter Custom First Alpha & Numeric Values---> After Auto-Increment itself" });
                    sList.Add(new PrefixClass { ID = 4, Name = "Enter Constant Cutsom Prefix Name" });
                }
                else if (selected.ID == 2)
                {
                    sList.Add(new PrefixClass { ID = 1, Name = "Add Auto-Increment Suffix Numbers" });
                    sList.Add(new PrefixClass { ID = 2, Name = "Enter Custom First Suffix Numbers---> After Auto-Increment itself" });
                    sList.Add(new PrefixClass { ID = 3, Name = "Enter Custom First Alpha & Numeric Values---> After Auto-Increment itself" });
                    sList.Add(new PrefixClass { ID = 4, Name = "Enter Constant Cutsom Suffix Name" });

                }
                prefixCombo.ItemsSource = sList;
                prefixCombo.SelectedValuePath = "ID";
                prefixCombo.DisplayMemberPath = "Name";

            }


        }
        private void showMessgae_forComboboxSelection(PrefixClass selected)
        {
            if (selected != null)
            {
                MessageBoxImage icon = MessageBoxImage.Information;
                if (selected.ID == -1) return;

                RenameType selRename = RenameTypeCombo.SelectedItem as RenameType;

                if (Caller != null && Caller.ToLower() == "files")
                {

                    switch (selected.ID)
                    {
                        case 2:
                            HideAll_CustomPrefix_Options();
                            string message = selRename.ID == 1 ? (ResourceFile.ResourceManager.GetString("CustomMessage_2", CultureInfo.CurrentUICulture)) : ResourceFile.ResourceManager.GetString("CustomMessage_2_Suffix", CultureInfo.CurrentUICulture);
                            System.Windows.MessageBox.Show(message, MultipleRename, MessageBoxButton.OK, icon);
                            Textbox1.Visibility = Visibility.Visible;
                            PrefixNumlabel1.Visibility = Visibility.Visible;
                            break;
                        case 3:
                            HideAll_CustomPrefix_Options();
                            string message1 = selRename.ID == 1 ? ResourceFile.ResourceManager.GetString("CustomMessage_3", CultureInfo.CurrentUICulture) : ResourceFile.ResourceManager.GetString("CustomMessage_3_Suffix", CultureInfo.CurrentUICulture);
                            System.Windows.MessageBox.Show(message1, MultipleRename, MessageBoxButton.OK, icon);
                            PrefixLabel1.Visibility = Visibility.Visible;
                            CustomTextbox1.Visibility = Visibility.Visible;

                            PrefixLabel2.Visibility = Visibility.Visible;
                            CustomTextbox2.Visibility = Visibility.Visible;
                            break;
                        case 4:
                            HideAll_CustomPrefix_Options();
                            string message2 = selRename.ID == 1 ? ResourceFile.ResourceManager.GetString("CustomMessage_4", CultureInfo.CurrentUICulture) : ResourceFile.ResourceManager.GetString("CustomMessage_4_Suffix", CultureInfo.CurrentUICulture);
                            System.Windows.MessageBox.Show(message2, MultipleRename, MessageBoxButton.OK, icon);
                            ConstPre_Textbox.Visibility = Visibility.Visible;
                            ConstPre_label.Visibility = Visibility.Visible;
                            break;
                        default:
                            HideAll_CustomPrefix_Options(); ;
                            string message3 = selRename.ID == 1 ? ResourceFile.ResourceManager.GetString("PrefixMessage", CultureInfo.CurrentUICulture) : ResourceFile.ResourceManager.GetString("SuffixMessage", CultureInfo.CurrentUICulture);
                            System.Windows.MessageBox.Show(message3, MultipleRename, MessageBoxButton.OK, icon);

                            break;
                    }
                }
                else
                {
                    switch (selected.ID)
                    {
                        case 2:
                            HideAll_CustomPrefix_Options();
                            string message = selRename.ID == 1 ? (ResourceFile.ResourceManager.GetString("FolderPrefix2", CultureInfo.CurrentUICulture)) : ResourceFile.ResourceManager.GetString("FolderSuffix2", CultureInfo.CurrentUICulture);
                            System.Windows.MessageBox.Show(message, MultipleRename, MessageBoxButton.OK, icon);
                            Textbox1.Visibility = Visibility.Visible;
                            PrefixNumlabel1.Visibility = Visibility.Visible;
                            break;
                        case 3:
                            HideAll_CustomPrefix_Options();
                            string message1 = selRename.ID == 1 ? ResourceFile.ResourceManager.GetString("FolderPrefix_3", CultureInfo.CurrentUICulture) : ResourceFile.ResourceManager.GetString("FolderSuffix_3", CultureInfo.CurrentUICulture);
                            System.Windows.MessageBox.Show(message1, MultipleRename, MessageBoxButton.OK, icon);
                            PrefixLabel1.Visibility = Visibility.Visible;
                            CustomTextbox1.Visibility = Visibility.Visible;

                            PrefixLabel2.Visibility = Visibility.Visible;
                            CustomTextbox2.Visibility = Visibility.Visible;
                            break;
                        case 4:
                            HideAll_CustomPrefix_Options();
                            string message2 = selRename.ID == 1 ? ResourceFile.ResourceManager.GetString("FolderPrefix_4", CultureInfo.CurrentUICulture) : ResourceFile.ResourceManager.GetString("FolderSuffix_3", CultureInfo.CurrentUICulture);
                            System.Windows.MessageBox.Show(message2, MultipleRename, MessageBoxButton.OK, icon);
                            ConstPre_Textbox.Visibility = Visibility.Visible;
                            ConstPre_label.Visibility = Visibility.Visible;
                            break;
                        default:
                            HideAll_CustomPrefix_Options(); ;
                            string message3 = selRename.ID == 1 ? ResourceFile.ResourceManager.GetString("FolderPrefix1", CultureInfo.CurrentUICulture) : ResourceFile.ResourceManager.GetString("FolderSuffix1", CultureInfo.CurrentUICulture);
                            System.Windows.MessageBox.Show(message3, MultipleRename, MessageBoxButton.OK, icon);

                            break;
                    }

                }
            }

        }


        private void prefixCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox ComBo1 = sender as System.Windows.Controls.ComboBox;

            PrefixClass selected = ComBo1.SelectedItem as PrefixClass;
            ComBo1.ToolTip = selected!=null ? selected.Name:null;
            showMessgae_forComboboxSelection(selected);
        }

        private void HideAll_CustomPrefix_Options()
        {
            Textbox1.Visibility = Visibility.Hidden;
            PrefixNumlabel1.Visibility = Visibility.Hidden;
            Textbox1.Text = "";

            PrefixLabel1.Visibility = Visibility.Hidden;
            CustomTextbox1.Visibility = Visibility.Hidden;
            CustomTextbox1.Text = "";

            PrefixLabel2.Visibility = Visibility.Hidden;
            CustomTextbox2.Visibility = Visibility.Hidden;
            CustomTextbox2.Text = "";

            ConstPre_Textbox.Visibility = Visibility.Hidden;
            ConstPre_label.Visibility = Visibility.Hidden;
            ConstPre_Textbox.Text = "";

        }

        private void Textbox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[0-9]+");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void Textbox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            String text = (String)e.DataObject.GetData(typeof(String));
            Regex regex = new Regex("^[0-9]*$");
            if (regex.IsMatch(text) == false)
            {
                e.CancelCommand();
            }
        }


        private void ALphaTextbox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[a-zA-z]+");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void AlphaTextbox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            String text = (String)e.DataObject.GetData(typeof(String));
            Regex regex = new Regex("^[a-zA-z]*$");
            if (regex.IsMatch(text) == false)
            {
                e.CancelCommand();
            }
        }

        private void ConstPre__PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {

            //  Regex regex = new Regex(@"[a-zA-Z0-9\_]+");
            Regex regex = new Regex(@"\w+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void ConstPre_Textbox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            String text = (String)e.DataObject.GetData(typeof(String));
            string reg1 = @"^\w*$";
            Regex regex = new Regex(reg1);
            if (regex.IsMatch(text) == false)
            {
                e.CancelCommand();
            }
        }

        private void RenameType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                prefixCombo.Visibility = Visibility.Visible;
                CustomLabel.Visibility = Visibility.Visible;

                prefixCombo.SelectedItem = -1;
                prefixCombo.SelectedIndex = -1;
                System.Windows.Controls.ComboBox ComBo1 = sender as System.Windows.Controls.ComboBox;
                RenameType selected = ComBo1.SelectedItem as RenameType;
                ComBo1.ToolTip = selected!=null ? selected.Name:null;
                if (selected == null) return;
                if (selected != null && selected.ID == 3)
                {

                    //prefixCombo.SelectedValue = 3;
                    //PrefixClass selected1 = prefixCombo.SelectedItem as PrefixClass;
                    //showMessgae_forComboboxSelection(selected1);

                    prefixCombo.Visibility = Visibility.Hidden;
                    HideAll_CustomPrefix_Options();

                    ConstPre_Textbox.Visibility = Visibility.Visible;
                    ConstPre_label.Visibility = Visibility.Visible;
                    CustomLabel.Visibility = Visibility.Hidden;
                    MessageBoxImage icon = MessageBoxImage.Information;
                    string message = ResourceFile.ResourceManager.GetString("CompleteRename_message", CultureInfo.CurrentUICulture);
                    System.Windows.MessageBox.Show(message, MultipleRename, MessageBoxButton.OK, icon);

                }
                else
                {
                    HideAll_CustomPrefix_Options();
                    CustomLabel.Visibility = Visibility.Visible;
                    prefixCombo.Visibility = Visibility.Visible;


                }
                var temp = ResourceFile.ResourceManager;
                PrefixClass selected1 = prefixCombo.SelectedItem as PrefixClass;

                if (Caller == "Files")
                {
                    try
                    {
                        updateprefix_ComboBox(selected);
                        Update_labels(selected);

                        if (selected != null && selected.ID == 1)
                            CustomLabel.Content = temp.GetString("Add Preffix to Files", CultureInfo.CurrentCulture).ToString();
                        else
                        {
                            CustomLabel.Content = (selected.ID == 2) ? temp.GetString("Add Suffix to Files", CultureInfo.CurrentCulture) : "";
                        }
                    }
                    catch (Exception ex)
                    {
                        var s = ex;
                    }
                }
                else
                {
                    try
                    {
                        Update_labels(selected);
                        updateprefix_ComboBox(selected);
                        if (selected.ID == 1)
                            CustomLabel.Content = temp.GetString("Add Preffix to Folders", CultureInfo.CurrentCulture).ToString();
                        else
                        {
                            CustomLabel.Content = (selected.ID == 2) ? temp.GetString("Add Suffix to Folders", CultureInfo.CurrentCulture) : "";
                        }
                    }
                    catch (Exception er)
                    {
                        var s = er;


                    }

                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Update_labels based on Renametype selection suffix / prfix etc
        /// </summary>
        /// <param name="selected"></param>
        internal void Update_labels(RenameType selected)
        {
            var temp = ResourceFile.ResourceManager;
            if (selected != null && selected.ID == 1)
            {
                PrefixNumlabel1.Content = temp.GetString("Enter_Prefix_Number", CultureInfo.CurrentCulture).ToString();
                PrefixLabel1.Content = temp.GetString("Enter_Prefix_Name", CultureInfo.CurrentCulture).ToString();
                PrefixLabel2.Content = temp.GetString("Enter_Prefix_Number", CultureInfo.CurrentCulture).ToString();
                ConstPre_label.Content = temp.GetString("Enter_Const_Prefix", CultureInfo.CurrentCulture).ToString();
            }
            else if (selected != null && selected.ID == 2)
            {
                PrefixNumlabel1.Content = temp.GetString("Enter_Suffix_Number", CultureInfo.CurrentCulture).ToString();
                PrefixLabel1.Content = temp.GetString("Enter_Suffix_Name", CultureInfo.CurrentCulture).ToString();
                PrefixLabel2.Content = temp.GetString("Enter_Suffix_Number", CultureInfo.CurrentCulture).ToString();
                ConstPre_label.Content = temp.GetString("Enter_Const_Suffix", CultureInfo.CurrentCulture).ToString();
            }
            else if (selected != null && selected.ID == 3)
            {
                ConstPre_label.Content = temp.GetString("Enter_Const_Name", CultureInfo.CurrentCulture).ToString();

            }
        }


        /// <summary>
        /// populate data in grid
        /// </summary>
        /// <param name="FileType_boxText"  value ="file extension selected string"></param>
        private void PopulateDataGrid(string FileType_boxText)
        {
            if (Caller != null && Caller.ToLower() == "files")
            {
                if (FileType_boxText.Trim().Length > 0)
                {
                    bool Is_Allselected = FileType_boxText.Contains("ALL");
                   GridDataSource = new ObservableCollection<Grid_data_Info>();
                    List<Grid_data_Info> ListDataSource = new List<Grid_data_Info>();

                    if (FilesInfo.Count > 0)
                    {
                        List<FileInfo> temp_FilesInfo = new List<FileInfo>();
                        if (checkbox_Asc.IsChecked == true)
                        {
                            temp_FilesInfo = FilesInfo.OrderBy(x => x.Extension).ThenBy(y=>y.LastWriteTime).ToList<FileInfo>();
                        }
                        else if (checkbox_Desc.IsChecked == true)
                        {
                            temp_FilesInfo = FilesInfo.OrderBy(x => x.Extension).ThenByDescending(y => y.LastWriteTime).ToList<FileInfo>();

                        }
                        int count = 1;
                        foreach (var item in temp_FilesInfo)
                        {
                            Grid_data_Info obj = null;
                            string ext = item.Extension;
                            string attributes = item.Attributes.ToString();
                            if (attributes.Contains("Hidden")) continue;
                           

                            if (Is_Allselected)
                            {
                                obj = new Grid_data_Info() { ID = count, Name = item.Name, Location = item.FullName, Folder_Name = item.DirectoryName, Type = item.Extension, Date_Modified = item.LastWriteTime, Date_Created = item.CreationTime, IsSelected = true, Result = "" };
                            }
                            else if (FileType_boxText.Contains(ext))
                            {
                                obj = new Grid_data_Info() { ID = count, Name = item.Name, Location = item.FullName, Folder_Name = item.DirectoryName, Type = item.Extension, Date_Modified = item.LastWriteTime, Date_Created = item.CreationTime, IsSelected = true, Result = "" };
                            }
                            else
                            {
                                continue;
                            }

                            GridDataSource.Add(obj);
                            ListDataSource.Add(obj);
                            count++;
                        }

                        Display_DataGrid.ItemsSource = GridDataSource;
                        StartButton.Visibility = Visibility.Visible;

                    }
                }
                else if (FileType_boxText.Trim().Length == 0)
                {
                    Display_DataGrid.ItemsSource = null;
                    StartButton.Visibility = Visibility.Hidden;
                    progressbar.Visibility = Visibility.Hidden;
                    CancelButton.Visibility = Visibility.Hidden;

                }
            }
            else if (Caller != null && Caller.ToLower() != "files")
            {
                GridDataSource = new ObservableCollection<Grid_data_Info>();
                if (FoldersInfo != null && FoldersInfo.Count > 0)
                {
                    List<DirectoryInfo> tempDirInfo = new List<DirectoryInfo>();
                    tempDirInfo = FoldersInfo.OrderBy(x => x.Name).ToList<DirectoryInfo>();
                    int count = 1;
                    foreach (var item in tempDirInfo)
                    {
                        Grid_data_Info obj = null;
                        string attributes = item.Attributes.ToString();
                        if (attributes.Contains("Hidden")) continue;

                        obj = new Grid_data_Info() { ID = count, Name = item.Name, Location = item.FullName, Folder_Name = item.Parent.Name, Type = "Folder", Date_Created = item.LastAccessTime, Date_Modified = item.CreationTime, IsSelected = true, Result = "" };

                        GridDataSource.Add(obj);
                        count++;
                    }
                    Display_DataGrid.ItemsSource = GridDataSource;
                    StartButton.Visibility = Visibility.Visible;
                    progressbar.Visibility = Visibility.Hidden;
                    CancelButton.Visibility = Visibility.Hidden;

                }
            }
        }

      
        /// <summary>
        /// datagrid row check box checked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cellcheckbox_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox rowCheckbox = sender as System.Windows.Controls.CheckBox;

            if (Display_DataGrid.ItemsSource != null && GridDataSource != null && GridDataSource.Count > 0)
            {
                if (rowCheckbox.IsChecked == true)
                {
                    var datarow = rowCheckbox.DataContext as Grid_data_Info;

                    bool allchecked = true;
                    foreach (Grid_data_Info item in Display_DataGrid.ItemsSource)
                    {
                        if (datarow.Name == item.Name)
                        {
                            item.IsSelected = true;
                        }; // due when checkbox checked data row IsSelected Not updated
                        if (!item.IsSelected)
                        {
                            allchecked = false;
                        }
                    }

                    chkSelectAll.IsChecked = (allchecked == true) ? true : false;
                }
            }
        }

        /// <summary>
        /// datagrid row check box unchecked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cellcheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            //  chkSelectAll.IsChecked = false;
            System.Windows.Controls.CheckBox rowCheckbox = sender as System.Windows.Controls.CheckBox;
            var datarow = rowCheckbox.DataContext as Grid_data_Info;
            bool allchecked = true;

            foreach (Grid_data_Info item in Display_DataGrid.ItemsSource)
            {
                if (datarow.Name == item.Name)
                {
                    item.IsSelected = false;
                }
                if (!item.IsSelected)
                {
                    allchecked = false;
                }
            }
            chkSelectAll.IsChecked = (allchecked == true) ? true : false;
        }

        /// <summary>
        /// data Grid Header check box click event used here check /uncheck child rows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkSelectAll_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox HeaderCheckbox = sender as System.Windows.Controls.CheckBox;
            bool? value = chkSelectAll.IsChecked;
            if (HeaderCheckbox.IsChecked == true)
            {
                foreach (Grid_data_Info item in Display_DataGrid.ItemsSource)
                {
                    item.IsSelected = true;
                }
            }
            else
            {
                foreach (Grid_data_Info item in Display_DataGrid.ItemsSource)
                {
                    item.IsSelected = false;
                }
            }
            Display_DataGrid.Items.Refresh();
        }


       
       


        /// <summary>
        /// Rename process start here on click on start process button 
        /// validations check is done here 
        /// after validaion Start_RenameOperations called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void StartProcess_click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Grid_data_Info> slist = Display_DataGrid.ItemsSource as ObservableCollection<Grid_data_Info>;
            // List<Grid_data_Info> slist1 = slist.Where(x => x.IsSelected == true).ToList();
            selRowlist1 = slist.Where(x => x.IsSelected).ToList();
            List<string> Failure_list = new List<string>();
            string order = "";

            SortBy_Details combo = SortCombo.SelectedItem as SortBy_Details;
            if (combo != null)
            {
               if(combo.ID == 1)
                {
                    if (checkbox_Asc.IsChecked == true)
                    {
                        order = "Ascending";
                        slist = new ObservableCollection<Grid_data_Info>(slist.OrderBy(y => y.Date_Created));
                    }
                    else if (checkbox_Desc.IsChecked == true)
                    {
                        order = "Descending";
                        slist = new ObservableCollection<Grid_data_Info>(slist.OrderByDescending(y => y.Date_Created));
                    }

                }
                else if(combo.ID == 2)
                {

                    if (checkbox_Asc.IsChecked == true)
                    {
                        order = "Ascending";
                        slist = new ObservableCollection<Grid_data_Info>(slist.OrderBy(y => y.Date_Modified));
                    }
                    else if (checkbox_Desc.IsChecked == true)
                    {
                        order = "Descending";
                        slist = new ObservableCollection<Grid_data_Info>(slist.OrderByDescending(y => y.Date_Modified));
                    }

                }
            }

           

            DateTime s = DateTime.Now;
            string sMessgae = ResourceFile.Start_Rename;
            sMessgae = sMessgae.Replace("#0#", order);
            if (System.Windows.MessageBox.Show(sMessgae, MultipleRename, MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
               
                if (Validate_Values())
                {
                    StartButton.IsEnabled = false;
                    progressbar.Visibility = Visibility.Visible;
                    progressbar.Value = 0;


                    if (Caller_isFile)
                    {
                        string fileExtselceted = GetFileExt_ComboBoxSelectedItems();
                        List<string> Ext_types = new List<string>();
                        if (fileExtselceted == "ALL")
                        {
                            for (int i = 0; i < FileExt_Combobox.Items.Count; i++)
                            {
                                filesExtensions stemp = FileExt_Combobox.Items[i] as filesExtensions;

                                if (stemp.ExtensionName != "ALL")
                                {
                                    Ext_types.Add(stemp.ExtensionName);
                                }
                            }
                        }
                        else
                        {
                            if (fileExtselceted.Contains(","))
                            {
                                Ext_types = fileExtselceted.Split(",").ToList();
                            }
                            else
                            {
                                Ext_types.Add(fileExtselceted);
                            }
                        }



                        for (int i = 0; i < Ext_types.Count; i++)
                        {
                            string extName = Ext_types[i];
                            int st = (i == 0) ? 1 : i;
                            int newvalue = (st / Ext_types.Count) * 100;
                            
                            progressbar.Dispatcher.Invoke(new Action(() =>
                               progressbar.Value = newvalue), System.Windows.Threading.DispatcherPriority.Background
                                );
                            Thread.Sleep(100);
                            Start_RenameOperations(ref slist, extName);
                        }

                    }
                    else
                    {
                        Start_RenameOperations(ref slist, "");
                    }
                    if (Caller_isFile)
                    {
                        GridDataSource = new ObservableCollection<Grid_data_Info>(slist.OrderBy(x => x.ID).ThenBy(x => x.Type));
                    }
                    else
                    {
                        GridDataSource = new ObservableCollection<Grid_data_Info>(slist.OrderBy(x => x.ID).ThenBy(x => x.Name));

                    }
                    Display_DataGrid.ItemsSource = GridDataSource;
                    if (Failure_list.Count > 0)
                    {
                        MessageBoxImage icon = MessageBoxImage.Exclamation;
                        if (System.Windows.MessageBox.Show(ResourceFile.Failure, MultipleRename, MessageBoxButton.OK, icon) == MessageBoxResult.OK)
                        {
                            progressbar.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            progressbar.Visibility = Visibility.Hidden;
                        }

                    }
                    else

                    {
                        if (System.Windows.MessageBox.Show(ResourceFile.Success, MultipleRename) == MessageBoxResult.OK)
                        {
                            progressbar.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            progressbar.Visibility = Visibility.Hidden;
                        }

                    }
                    StartButton.IsEnabled = true;
                }

            }
            DateTime end = DateTime.Now;
            TimeSpan sTimeSpan = end.Subtract(s);
        }

        
     


        /// <summary>
        /// Rename operation is handled here
        /// </summary>
        private void Start_RenameOperations(ref ObservableCollection<Grid_data_Info> slist , string fileExtension_Text)
        {

            bool? isPrefix = null; 
            int incrementNum = 0; int count = 1;
            string Fix_Name = ""; int Fix_Num = 0; string const_text = ""; string fullrename = "";
            RenameType selRename = RenameTypeCombo.SelectedItem as RenameType;
            PrefixClass sel_Renameoption = prefixCombo.SelectedItem as PrefixClass;

            if (selRename != null && selRename.ID == 1)
                isPrefix = true;
            else if (selRename != null && selRename.ID == 2)
                isPrefix = false;

            else if (selRename != null && selRename.ID == 3 && ConstPre_Textbox.IsVisible)
            {
                fullrename = ConstPre_Textbox.Text;
            }

            if (sel_Renameoption != null && sel_Renameoption.ID == 2 && Textbox1.IsVisible == true && Textbox1.Text.Trim().Length > 0)
            {
                incrementNum = Convert.ToInt32(Textbox1.Text);

            }
            else if (sel_Renameoption != null && sel_Renameoption.ID == 3 && CustomTextbox1.IsVisible && CustomTextbox2.IsVisible)
            {
                Fix_Name = CustomTextbox1.Text;
                Fix_Num = Convert.ToInt32(CustomTextbox2.Text.ToString());

            }
            else if (sel_Renameoption != null && sel_Renameoption.ID == 4 && ConstPre_Textbox.IsVisible)
            {
                const_text = ConstPre_Textbox.Text;
            }

            int progressCount = 1;
          
            foreach (Grid_data_Info item in slist)
            {
              
                if (item.IsSelected ==false) continue; // item not checked in check box skip it;
                string Name = item.Name;


                if (Caller_isFile)
                {
                    if (item.Type != fileExtension_Text) continue;
                    Name = Name.Replace(item.Type, "");
                    if (!File.Exists(item.Location))
                    {
                        Failure_list.Add("File :" + item.Name + "--------------------------" + "  Not exist in the Given location");
                        continue;
                    }

                }
                else
                {

                    int st = (progressCount == 0) ? progressCount : progressCount;
                    int newvalue = (st / slist.Count) * 100;
                    progressbar.Dispatcher.Invoke(new Action(() =>  progressbar.Value = newvalue), System.Windows.Threading.DispatcherPriority.Background);
                    
                    if (!Directory.Exists(item.Location))
                    {
                        Failure_list.Add("Folder :" + item.Name + "--------------------------" + "  Not exist in the Given location");
                        continue;
                    }
                }

                

                string sfileName = ""; string sNewPath = "";
                if (selRename.ID != 3) {
                    switch (sel_Renameoption.ID)
                    {
                        case 2:
                            sfileName = isPrefix == true ? (incrementNum + "-" + Name) : (Name + "-" + incrementNum);
                            sfileName = (Caller_isFile) ? sfileName + item.Type : sfileName;
                            sNewPath = item.Location.Substring(0, item.Location.LastIndexOf("\\") + 1);
                            sNewPath = sNewPath + sfileName;
                            Rename_operations(sNewPath, sfileName, item, ref Failure_list);
                            incrementNum++;
                            break;

                        case 3:
                            sfileName = isPrefix == true ? (Fix_Name + "-" + Fix_Num + "-" + Name) : (Name + "-" + Fix_Name + "-" + Fix_Num);
                            sfileName = (Caller_isFile) ? sfileName + item.Type : sfileName;
                            sNewPath = item.Location.Substring(0, item.Location.LastIndexOf("\\") + 1);
                            sNewPath = sNewPath + sfileName;
                            Rename_operations(sNewPath, sfileName, item, ref Failure_list);
                            Fix_Num++;
                            break;

                        case 4:
                            sfileName = isPrefix == true ? (const_text + "-" + Name) : (Name + "-" + const_text);
                            sfileName = (Caller_isFile) ? sfileName + item.Type : sfileName;
                            sNewPath = item.Location.Substring(0, item.Location.LastIndexOf("\\") + 1);
                            sNewPath = sNewPath + sfileName;
                            Rename_operations(sNewPath, sfileName, item, ref Failure_list);
                            break;


                        default:
                            sfileName = isPrefix == true ? (count + "-" + Name) : (Name + "-" + count);
                            sfileName = (Caller_isFile) ? sfileName + item.Type : sfileName;
                            sNewPath = item.Location.Substring(0, item.Location.LastIndexOf("\\") + 1);
                            sNewPath = sNewPath + sfileName;
                            Rename_operations(sNewPath, sfileName, item, ref Failure_list);
                            count++;
                            break;

                    }
                }
                else if(selRename.ID == 3 && fullrename.Trim().Length>0)
                {
                  string temp =   fullrename + "-" + count;
                    sfileName = (Caller_isFile) ? temp + item.Type : temp;
                    sNewPath = item.Location.Substring(0, item.Location.LastIndexOf("\\") + 1);
                    sNewPath = sNewPath + sfileName;
                    Rename_operations(sNewPath, sfileName, item, ref Failure_list);
                    count++;

                }

            }
           
        }


        /// <summary>
        /// Rename_operations  is used to rename the folder or files if any expetion is handled here .
        /// </summary>
        /// <param name="newPath"></param>
        /// <param name="newName"></param>
        /// <param name="item"  type="Grid_data_Info"></param>
        private void Rename_operations(string newPath, string newName,  Grid_data_Info item, ref List<String> Failure_list)
        {
            if (Caller_isFile && item!= null && newPath.Trim().Length>0 && newName.Trim().Length>0)
            {
                try
                {
                    if (File.Exists(item.Location) == false)
                    {
                        _ = item.Result == "Failure : File Not Found in the Given location.";
                        return;
                    }
                    else
                    {
                        File.Move(item.Location, newPath);
                        item.Name = newName;
                        item.Location = newPath;
                        item.Result = "Success";
                    }

                }
                catch (Exception ex )
                {
                    if(ex is UnauthorizedAccessException)
                    {
                        item.Result = "Failure : it seems you don't have the required permission, please check it and try again ";
                        Failure_list.Add(item.Result.ToString());
                    }
                    else if(ex is PathTooLongException)
                    {
                        item.Result = "Failure : The specified path, file name, or both exceed the system-defined maximum length.";
                        Failure_list.Add(item.Result.ToString());

                    }
                    else if(ex is DirectoryNotFoundException)
                    {
                        
                        item.Result = "Failure: The selected parent Folder not  found.";
                        Failure_list.Add(item.Result.ToString());
                    }
                    else if(ex is IOException)
                    {
                        item.Result = "Failure: The selcted file name is already present";
                        Failure_list.Add(item.Result.ToString());
                    }
                    else
                    {
                        item.Result = ex.Message;
                        Failure_list.Add(item.Result.ToString());
                    }

                }

            }else if (Caller_isFile == false && item != null && newPath.Trim().Length > 0 && newName.Trim().Length > 0)
            {
                try
                {
                    if (Directory.Exists(item.Location) == false)
                    {
                        _ = item.Result == "Failure : File Not Found in the Given location.";
                        return;
                    }
                    else
                    {
                        Directory.Move(item.Location, newPath);
                        item.Name = newName;
                        item.Location = newPath;
                        item.Result = "Success";
                    }
                }
                catch (Exception ex)
                {
                    if (ex is UnauthorizedAccessException)
                    {
                        item.Result = "Failure : it seems you don't have the required permission, please check it and try again ";
                        Failure_list.Add(item.Result.ToString());
                    }
                    else if (ex is PathTooLongException)
                    {
                        item.Result = "Failure : The specified path, Folder name, or both exceed the system-defined maximum length.";
                        Failure_list.Add(item.Result.ToString());
                    }
                    else if (ex is DirectoryNotFoundException)
                    {
                        item.Result = "Failure: The selected parent Folder not  found.";
                        Failure_list.Add(item.Result.ToString());
                    }
                    else if (ex is IOException)
                    {
                        item.Result = "Failure: The selcted Folder name is already present";
                        Failure_list.Add(item.Result.ToString());
                    }
                    else
                    {
                        item.Result = ex.Message;
                        Failure_list.Add(item.Result.ToString());
                    }

                }
            }
        }

        /// <summary>
        /// Validate_Values  =  validation are done here
        /// </summary>
        /// <returns></returns>
        private bool Validate_Values()
        {
            bool result = true;

            string fileExtselceted = GetFileExt_ComboBoxSelectedItems();
            RenameType selRename = RenameTypeCombo.SelectedItem as RenameType;
            PrefixClass selected1 = prefixCombo.SelectedItem as PrefixClass;

            if (checkbox_Asc.IsChecked == false && checkbox_Desc.IsChecked == false)
            {
                System.Windows.MessageBox.Show(ResourceFile.Please_check_Asc_desc, MultipleRename, MessageBoxButton.OK);
                return result = false;
            }
            else if (Caller != null && Caller.ToLower() == "files" && fileExtselceted.Trim().Length == 0)
            {
                System.Windows.MessageBox.Show(ResourceFile.Please_Select_FileTpes, MultipleRename, MessageBoxButton.OK);
                return result = false;
            }
            else if (selRename == null)
            {
                System.Windows.MessageBox.Show(ResourceFile.Please_Select_Rename_Type, MultipleRename, MessageBoxButton.OK);
                return result = false;
            }
            else if (selRename.ID == 1)
            {
                if (selected1 == null)
                {
                    string message = Caller_isFile == true ? ResourceFile.Please_Sel_Prifx_Type : ResourceFile.Please_Sel_Prifx_Type1;
                    System.Windows.MessageBox.Show(message, MultipleRename, MessageBoxButton.OK);
                    return result = false;
                }

            }
            else if (selRename.ID == 2)
            {
                if (selected1 == null)
                {
                    string message = Caller_isFile == true ? ResourceFile.Please_Sel_Suffix_Type : ResourceFile.Please_Sel_Suffix_Type1;

                    System.Windows.MessageBox.Show(message, MultipleRename, MessageBoxButton.OK);
                    return result = false;
                }
            }
            else if (selRename.ID == 3)
            {
                if (ConstPre_Textbox.Text.Trim().Length == 0)
                {

                    System.Windows.MessageBox.Show(ResourceFile.Please_Enter_Constant_Name, MultipleRename, MessageBoxButton.OK);
                    return result = false;
                }
            }

            if (selRename.ID != 3)
            {

                if (selected1.ID == 2)
                {
                    if (Textbox1.Text.Trim().Length == 0)
                    {
                        System.Windows.MessageBox.Show(ResourceFile.Please_enter + " " + PrefixNumlabel1.Content.ToString() + ".", MultipleRename, MessageBoxButton.OK);
                        return result = false;
                    }

                }
                else if (selected1.ID == 3)
                {
                    if (CustomTextbox1.Text.Trim().Length == 0)
                    {
                        System.Windows.MessageBox.Show(ResourceFile.Please_enter + " " + PrefixLabel1.Content.ToString() + ".", MultipleRename, MessageBoxButton.OK);
                        return result = false;
                    }
                    else if (CustomTextbox2.Text.Trim().Length == 0)
                    {
                        System.Windows.MessageBox.Show(ResourceFile.Please_enter + " " + PrefixLabel2.Content.ToString() + ".", MultipleRename, MessageBoxButton.OK);
                        return result = false;
                    }
                }
                else if (selected1.ID == 4)
                {
                    if (ConstPre_Textbox.Text.Trim().Length == 0)
                    {
                        System.Windows.MessageBox.Show(ResourceFile.Please_enter + " " + ConstPre_label.Content.ToString() + ".", MultipleRename, MessageBoxButton.OK);
                        return result = false;
                    }
                }

            }
            if (Display_DataGrid.Items.Count == 0)
            {
                return result = false;

            }
            else if (Display_DataGrid.Items.Count > 0)
            {
                ObservableCollection<Grid_data_Info> slist = Display_DataGrid.ItemsSource as ObservableCollection<Grid_data_Info>;
                List<Grid_data_Info> slist1 = slist.Where(x => x.IsSelected == true).ToList();
                if (slist1 == null || slist1.Count == 0)
                {
                    System.Windows.MessageBox.Show(ResourceFile.Please_Sel_GridItems, MultipleRename, MessageBoxButton.OK);
                    return result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// Grid : Grid2 column no 0 width is increases or decreases based on label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomLabel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            double width = Grid2.ColumnDefinitions[2].ActualWidth;
            if (width == 100 || width <= 155)
            {
                Grid2.ColumnDefinitions[2].MinWidth = new GridLength(200).Value;
            }
            else
            {
                Grid2.ColumnDefinitions[2].MinWidth = new GridLength(100).Value;
                if (label_fileType.IsVisible == false && Caller_isFile == false)
                {
                    if (CustomLabel.IsVisible == true)
                        Grid2.ColumnDefinitions[2].MinWidth = new GridLength(200).Value;
                }else if(Caller_isFile == true && CustomLabel.IsVisible == true && label_fileType.IsVisible==true)
                {
                    Grid2.ColumnDefinitions[2].MinWidth = new GridLength(200).Value;

                }
            }
        }

        /// <summary>
        /// Grid : Grid2 column no 1 width is increases or decreases based on label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConstPre_label_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
           
        }

        private void SortCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            SortBy_Details combo = SortCombo.SelectedItem as SortBy_Details;
            if (combo != null)
            SortCombo.ToolTip = combo.Name;
        }
    }
}




