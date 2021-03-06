﻿using Octokit;

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DynaHub.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        // Store user inputs
        public Login()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void field_GotFocus(object sender, RoutedEventArgs e)
        {
            // Get calling element
            TextBox s = sender as TextBox;

            // Set calling element to empty
            s.Text = "";
        }

        private void field_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox s = sender as TextBox;

            // Set to initial value only if user didn't input any value
            if (s.Text == "")
            {
                s.Text = s.Name;
            }
        }

        private void pass_GotFocus(object sender, RoutedEventArgs e)
        {
            // Get calling element
            PasswordBox p = sender as PasswordBox;

            // Set calling element to empty
            p.Password = "";
        }

        private void pass_LostFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox p = sender as PasswordBox;

            // Set to initial value only if user didn't input any value
            if (p.Password == "")
            {
                p.Password = p.Name;
            }
        }


        private void button_MouseUp(object sender, RoutedEventArgs e)
        {
        }

        // GitHub client
        readonly GitHubClient client = new GitHubClient(new ProductHeaderValue("DynaHub"));

        // Dictionary with both repo path and download_url
        public static Dictionary<string, string> repoFiles = new Dictionary<string, string>();

        // Aknowledge if the user logged in
        public static bool logged = false;

        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            GlobalSettings.user = username.Text;
            GlobalSettings.repo = reponame.Text;
            GlobalSettings.tok = token.Password;

            // It only works with a simple repo structure (for now): repo > folders [NO SUBFOLDERS]

            // Try to authenticate through personal access token
            try
            {
                client.Credentials = new Credentials(GlobalSettings.tok);
            }
            catch (Exception)
            {
                MessageBox.Show("It seems like you've input the wrong token",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            // List for all folders in repo to be queried
            List<string> repoFolders = new List<string>();

            IReadOnlyList<RepositoryContent> repoLevel = null;

            // Get content from GitHub at highest/repo level
            try
            {
                repoLevel =
                    await client.Repository.Content.GetAllContents(
                        GlobalSettings.user,
                        GlobalSettings.repo);
            }
            catch
            {
                MessageBox.Show("I couldn't find anything with those credentials.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            try
            {
                foreach (RepositoryContent r in repoLevel)
                {
                    if (r.Name.EndsWith(".dyn"))
                    {
                        repoFiles.Add(r.Path, r.DownloadUrl);
                    }
                    else if (r.Type == "dir")
                    {
                        repoFolders.Add(r.Path);
                    }
                }
            }
            catch (NullReferenceException)
            {
                // Do nothing. Already managed in catch above
            }

            // Check repo's subfolders
            foreach (string f in repoFolders)
            {
                IReadOnlyList<RepositoryContent> foldersLevel =
                    await client.Repository.Content.GetAllContents(
                        GlobalSettings.user,
                        GlobalSettings.repo,
                        f);

                foreach (RepositoryContent s in foldersLevel)
                {
                    if (s.Name.EndsWith(".dyn"))
                    {
                        repoFiles.Add(s.Path, s.DownloadUrl);
                    }
                }
            }

            // Notify user
            AutoClosingMessageBox.Show("The login was successful.", "Success", 3000);
            // If you go to this point, it was successful
            logged = true;
            // And close the log in form
            Close();
        }
    }
}
