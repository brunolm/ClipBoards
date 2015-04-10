using ClipBoards.Properties;
using ClipBoards.ViewModels;
using System.Windows;

namespace ClipBoards.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();

            Closing += MainView_Closing;
        }

        void MainView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!(DataContext as MainViewModel).Save())
            {
                if (MessageBox.Show("There was an error saving. Are you sure you want to exit?"
                    , "Confirm exit"
                    , MessageBoxButton.YesNo
                    , MessageBoxImage.Error
                    , MessageBoxResult.No) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }

            if (!e.Cancel)
            {
                Settings.Default.Save();
            }
        }
    }
}
