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

namespace Client
{

    public partial class MainWindow : Window
    {
        private List<string> list_full_paths;
        private CancellationTokenSource cancelTokenSource;
        private CancellationToken token;
        private ListView? listview = null;
        private Node? selected = null;
        private ObservableCollection<Node> Neutral = new();
        private ObservableCollection<Node> Happiness = new();
        private ObservableCollection<Node> Surprise = new();
        private ObservableCollection<Node> Sadness = new();
        private ObservableCollection<Node> Anger = new();
        private ObservableCollection<Node> Disgust = new();
        private ObservableCollection<Node> Fear = new();
        private ObservableCollection<Node> Contempt = new();
        public class Node
        {

            public string Descr { get; set; }

            public byte[] RawData { get; set; }

            public int Id { get; set; }
            public Node(byte[] data)
            {
                this.RawData = data;
                this.Descr = "   Distribution:\n";
            }
        }

        private void Unpack_emotions_to_node(in Node node, in List<Emotion> Emotions)
        {
            foreach (Emotion e in Emotions)
                node.Descr += ($" {e.emotion_name}: {e.emotion_val}\n");
        }

        async private void Load_db()
        {
            Button_Load.IsEnabled = false;
            Button_Delete_Selected.IsEnabled = false;
            Button_Delete_DB.IsEnabled = false;

            ClientFunctions Client = new();
            int[]? ids = await Client.GetIdsFromServer();
            Neutral = new();
            Happiness = new();
            Surprise = new();
            Sadness = new();
            Anger = new();
            Disgust = new();
            Fear = new();
            Contempt = new();
            if (ids != null)
            {
                foreach (int id in ids)
                {
                    Contracts.Image? image = await Client.GetImageFromServer(id);

                    Node node = new(Convert.FromBase64String(image.Blob));
                    node.Id = id;
                    list_full_paths.Add(image.Name);
                    image.Emotions = image.Emotions.OrderByDescending(x => x.emotion_val).ToList();
                    Unpack_emotions_to_node(node, image.Emotions);
                    switch (image.Emotions[0].emotion_name)
                    {
                        case "neutral":
                            Neutral.Add(node);
                            break;

                        case "happiness":
                            Happiness.Add(node);
                            break;

                        case "surprise":
                            Surprise.Add(node);
                            break;

                        case "sadness":
                            Sadness.Add(node);
                            break;

                        case "anger":
                            Anger.Add(node);
                            break;

                        case "disgust":
                            Disgust.Add(node);
                            break;

                        case "fear":
                            Fear.Add(node);
                            break;

                        case "contempt":
                            Contempt.Add(node);
                            break;
                    }
                }
                Tree_Neutral.ItemsSource = Neutral;
                Tree_Happiness.ItemsSource = Happiness;
                Tree_Contempt.ItemsSource = Contempt;
                Tree_Disgust.ItemsSource = Disgust;
                Tree_Fear.ItemsSource = Fear;
                Tree_Sadness.ItemsSource = Sadness;
                Tree_Surprise.ItemsSource = Surprise;
                Tree_Anger.ItemsSource = Anger;

                Button_Load.IsEnabled = true;
                Button_Delete_Selected.IsEnabled = true;
                Button_Delete_DB.IsEnabled = true;
            }
        }



        public MainWindow()
        {
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
            list_full_paths = new List<string>();
            InitializeComponent();
            Tree_Neutral.ItemsSource = Neutral;
            Tree_Happiness.ItemsSource = Happiness;
            Tree_Contempt.ItemsSource = Contempt;
            Tree_Disgust.ItemsSource = Disgust;
            Tree_Fear.ItemsSource = Fear;
            Tree_Sadness.ItemsSource = Sadness;
            Tree_Surprise.ItemsSource = Surprise;
            Tree_Anger.ItemsSource = Anger;

            Tree_Neutral.SelectionChanged += SelectedElement;
            Tree_Happiness.SelectionChanged += SelectedElement;
            Tree_Contempt.SelectionChanged += SelectedElement;
            Tree_Disgust.SelectionChanged += SelectedElement;
            Tree_Fear.SelectionChanged += SelectedElement;
            Tree_Sadness.SelectionChanged += SelectedElement; ;
            Tree_Surprise.SelectionChanged += SelectedElement;
            Tree_Anger.SelectionChanged += SelectedElement;
            Load_db();
        }


        private void SelectedElement(object sender, SelectionChangedEventArgs e)
        {
            if (((ListView)sender).SelectedItem != null)
            {
                listview = (ListView)sender;
                selected = (Node)listview.SelectedItem;


                if (listview.Name != "Tree_Neutral")
                    Tree_Neutral.UnselectAll();

                if (listview.Name != "Tree_Happiness")
                    Tree_Happiness.UnselectAll();

                if (listview.Name != "Tree_Contempt")
                    Tree_Contempt.UnselectAll();

                if (listview.Name != "Tree_Disgust")
                    Tree_Disgust.UnselectAll();

                if (listview.Name != "Tree_Fear")
                    Tree_Fear.UnselectAll();

                if (listview.Name != "Tree_Sadness")
                    Tree_Sadness.UnselectAll();

                if (listview.Name != "Tree_Surprise")
                    Tree_Surprise.UnselectAll();

                if (listview.Name != "Tree_Anger")
                    Tree_Anger.UnselectAll();
            }
        }

        private async void Button_Load_Images(object sender, RoutedEventArgs e)
        {
            Button_Load.IsEnabled = false;
            Button_Delete_Selected.IsEnabled = false;
            Button_Delete_DB.IsEnabled = false;

            list_full_paths = new List<string>();

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

                list_full_paths = new List<string>();

                pBar.Minimum = 0;
                pBar.Maximum = Files.Length - 1;
                pBar.Value = 0;

                foreach (FileInfo File in Files)
                {

                    Contracts.Image image = new();
                    byte[] data = System.IO.File.ReadAllBytes(File.FullName);
                    image.Blob = Convert.ToBase64String(data);
                    image.Hash = Contracts.Image.GetHash(data);
                    image.Name = File.FullName;
                    var i = await Client.PostImageToServer(image);
                    if (token.IsCancellationRequested)
                        return;
                    pBar.Value += 1;
                    if (i != null)
                        image.Id = (int)i;
                    list_full_paths.Add(File.FullName);
                }
                Load_db();
            }
            Button_Load.IsEnabled = true;
            Button_Delete_Selected.IsEnabled = true;
            Button_Delete_DB.IsEnabled = true;
        }

        async private void Button_Delete_All(object sender, RoutedEventArgs e)
        {
            ClientFunctions Client = new();
            await Client.DeleteAllServer();
            Load_db();
        }

        async private void Button_Click_delete(object sender, RoutedEventArgs e)
        {
            if (listview != null && selected != null)
            {
                ClientFunctions Client = new();
                await Client.DeleteImageServer(selected.Id);
                Load_db();
            }
            listview = null;
            selected = null;
        }

        private async void Button_Stop(object sender, RoutedEventArgs e)
        {
            cancelTokenSource.Cancel();
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
            Button_Load.IsEnabled = true;
            Button_Delete_Selected.IsEnabled = true;
            Button_Delete_DB.IsEnabled = true;
        }
    }
}
