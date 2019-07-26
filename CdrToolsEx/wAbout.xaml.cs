using System.Windows;
using System.Windows.Input;

namespace CdrToolsEx
{
    public partial class wAbout : Window
    {
        public wAbout()
        {
            InitializeComponent();

            sName.Text = Docker.MName;
            sInfo.Text = "Version: " + Docker.MVer + "\n" +
                "Release date: " + Docker.MDate + "\n" +
                "Copyright © Sanich, " + Docker.MYear;
            sWeb.Text = Docker.MWebSite;
            sEmail.Text = "e-mail: " + Docker.MEmail;
        }

        private void Exit(object sender, RoutedEventArgs e) { Close(); }

        private void GoToWebtite(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(Docker.MWebSite);
            Close();
        }
    }
}
