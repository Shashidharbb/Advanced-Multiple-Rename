using System;
using System.Windows;
using System.Windows.Input;


namespace Advanced_Multiple_Rename
{
    /// <summary>
    /// Advanced Multiple Rename : developed By shashidhar B  Badiger
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            InitializeComponent();
        }
       
        public String Caller = "";
        const string Files = "Files";
        const string Folder = "Folder";
        private void powericon_MouseDown(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

      

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            double width = mainGrid.ColumnDefinitions[0].Width.Value;
            if (width == 72)
            {
                mainGrid.ColumnDefinitions[0].Width = new GridLength(180);
            }
            else
            {
                mainGrid.ColumnDefinitions[0].Width = new GridLength(72);

            }
        }
        
        private void Folder_Click(object sender, RoutedEventArgs e)
        {
            ActionGrid.Children.Clear();

            RenameController Contrl = new RenameController();
            Contrl.Caller = Folder;
            ActionGrid.Children.Add(Contrl);
            ActionGrid.Visibility = Visibility.Visible;
        }

        private void Files_Click(object sender, RoutedEventArgs e)
        {
            ActionGrid.Children.Clear();
            if (ActionGrid.IsVisible)
            {
                ActionGrid.Children.Clear();
            }
            RenameController Contrl = new RenameController();
            Contrl.Caller = Files;
            ActionGrid.Children.Add(Contrl);
            ActionGrid.Visibility = Visibility.Visible;
        }

       
    }

}
