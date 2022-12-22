using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Contracts;
using Ookii.Dialogs.Wpf;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Client
{

    public partial class MainWindow : Window
    {
        private CancellationTokenSource cancelTokenSource;
        private CancellationToken token;

        public MainWindow()
        {
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
            InitializeComponent();
        }

        private async void Button_Load_Images(object sender, RoutedEventArgs e)
        {
            Button_Load.IsEnabled = false;

            ClientFunctions Client = new();

            VistaFolderBrowserDialog dlg = new VistaFolderBrowserDialog();

            dlg.ShowNewFolderButton = true;

            string path = null;
            if (dlg.ShowDialog() == true)
            {
                path = dlg.SelectedPath;
            }
            if (path != null)
            {
                DirectoryInfo d = new DirectoryInfo(path);
                FileInfo[] Files = d.GetFiles("*.*").Where(x => x.Name.EndsWith(".png") || x.Name.EndsWith(".jpg")).ToArray();


                pBar.Minimum = 0;
                pBar.Maximum = Files.Length - 1;
                pBar.Value = 0;
                
                foreach (FileInfo File in Files)
                {
                    Contracts.Image_post image = new();
                    byte[] data = System.IO.File.ReadAllBytes(File.FullName);
                    image.Blob = Convert.ToBase64String(data);
                    image.Hash = GetHash(data);
                    image.Name = File.FullName;
                    var i = await Client.PostImageToServer(image);
                    if (token.IsCancellationRequested)
                        return;
                    pBar.Value += 1;
                    if (i != null)
                        image.Id = (int)i;
                }
                
            }
            Button_Load.IsEnabled = true;

        }

        public static string GetHash(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                return string.Concat(sha256.ComputeHash(data).Select(x => x.ToString("X2")));
            }
        }

        private async void Button_Stop(object sender, RoutedEventArgs e)
        {
            cancelTokenSource.Cancel();
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
            Button_Load.IsEnabled = true;
        }
    }
}
