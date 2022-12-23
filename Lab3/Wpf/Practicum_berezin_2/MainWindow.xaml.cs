
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.IO;
using System.Collections.ObjectModel;
using Practicum1;
using Ookii.Dialogs.Wpf;
using Microsoft.EntityFrameworkCore;
using System.Windows.Controls;

namespace Practicum_berezin_2
{
    public partial class MainWindow : Window
    {
        private List<string> list_full_paths;
        private CancellationTokenSource cancelTokenSource;
        private CancellationToken token;
        private Emotion_detector detector;
        private ListView? listview = null;
        private Node? selected = null;
        public class Node
        {

            public string Descr { get; set;}

            public string Path {get; set;}
 
            public Node(string path)
            {
                this.Path = path;
                this.Descr = "   Distribution:\n";
            }
        }


        private ObservableCollection<Node> Neutral = new();
        private ObservableCollection<Node> Happiness = new();
        private ObservableCollection<Node> Surprise = new();
        private ObservableCollection<Node> Sadness = new();
        private ObservableCollection<Node> Anger = new();
        private ObservableCollection<Node> Disgust = new();
        private ObservableCollection<Node> Fear = new();
        private ObservableCollection<Node> Contempt = new();


        private void SelectedElement(object sender, SelectionChangedEventArgs e)
        {
            if (((ListView)sender).SelectedItem != null)
            {
                listview = (ListView)sender;
                selected = (Node)listview.SelectedItem;
            }

            if(listview.Name != "Tree_Neutral")
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


        private void Unpack_emotions_to_node(in Node node, in List<Emotion> Emotions)
        {
            foreach(Emotion e in Emotions)
                node.Descr += ($" {e.emotion_name}: {e.emotion_val}\n");
        }


        private void Load_db()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                var images = db.Images.Include(x => x.Emotions).ToArray();
                foreach (var image in images)
                {
                    Node node = new(image.Name);
                    list_full_paths.Add(image.Name);
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
            }
        }

        public MainWindow()
        {
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
            detector = new Emotion_detector();
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

        private void Button_Load_Images(object sender, RoutedEventArgs e)
        {
            list_full_paths = new List<string>();

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


                using (ApplicationContext db = new ApplicationContext())
                {
                    foreach (FileInfo File in Files)
                    {
                        byte[] image_data = System.IO.File.ReadAllBytes(File.FullName);
                        string hash = Image.GetHash(image_data);
                        var q = db.Images.Where(x => x.Hash == hash).Include(x => x.Blob)
                            .Where(x => Equals(x.Blob.Raw_data, image_data));
                        if (!q.Any())
                            list_full_paths.Add(File.FullName);
                    }
                }
                cancelTokenSource = new CancellationTokenSource();
                token = cancelTokenSource.Token;
            }

        }


        private void Button_Start_Calc(object sender, RoutedEventArgs e)
        {
            Start_Calc();
        }


        private void Emotions_fill(in Node Node, in Dictionary<string, float> dict, in Image image)
        {
            image.Emotions = new();
            foreach (KeyValuePair<string, float> entry in dict)
            {
                string str = ($" {entry.Key}: {entry.Value}");
                Node.Descr += str + "\n";
                Emotion em = new();
                em.emotion_name = entry.Key;
                em.emotion_val = entry.Value;
                em.image = image;
                image.Emotions.Add(em);
            }
        }

        private async void Start_Calc()
        {
            Button_Calc.IsEnabled = false;
            Button_Load.IsEnabled = false;
            Button_Delete.IsEnabled = false;

            pBar.Minimum = 0;
            pBar.Maximum = list_full_paths.Count - 1;
            pBar.Value = 0;

            Task<Dictionary<string, float>>[] tasks = new Task<Dictionary<string, float>>[list_full_paths.Count];

            for (int i = 0; i < list_full_paths.Count; i++)
            {
                var task = detector.Start(list_full_paths[i], token);
                tasks[i] = task;
            }

           
            for (int i = 0; i < list_full_paths.Count; i++)
            {
                await tasks[i];
                if (!token.IsCancellationRequested)
                    pBar.Value += 1;
            }

            lock (list_full_paths)
            {
                for (int i = 0; i < list_full_paths.Count; i++)
                {
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        var emotions = tasks[i].Result;
                        Node node;
                        Image img = new();
                        img.Name = list_full_paths[i];
                        img.Blob = new Image_directly
                        {
                            Raw_data = File.ReadAllBytes(img.Name),
                            image = img
                        };
                        img.Hash = Image.GetHash(img.Blob.Raw_data);
                        switch (emotions.FirstOrDefault().Key)
                        {

                            case "neutral":
                                {
                                    node = new Node(list_full_paths[i]);
                                    Emotions_fill(node, emotions, img);
                                    Neutral.Add(node);
                                    break;
                                }
                            case "happiness":
                                {
                                    node = new Node(list_full_paths[i]);
                                    Emotions_fill(node, emotions, img);
                                    Happiness.Add(node);
                                    break;
                                }
                            case "surprise":
                                {
                                    node = new Node(list_full_paths[i]);
                                    Emotions_fill(node, emotions, img);
                                    Surprise.Add(node);
                                    break;
                                }
                            case "sadness":
                                {
                                    node = new Node(list_full_paths[i]);
                                    Emotions_fill(node, emotions, img);
                                    Sadness.Add(node);
                                    break;
                                }
                            case "anger":
                                {
                                    node = new Node(list_full_paths[i]);
                                    Emotions_fill(node, emotions, img);
                                    Anger.Add(node);
                                    break;
                                }
                            case "disgust":
                                {
                                    node = new Node(list_full_paths[i]);
                                    Emotions_fill(node, emotions, img);
                                    Disgust.Add(node);
                                    break;
                                }
                            case "fear":
                                {
                                    node = new Node(list_full_paths[i]);
                                    Emotions_fill(node, emotions, img);
                                    Fear.Add(node);
                                    break;
                                }
                            case "contempt":
                                {
                                    node = new Node(list_full_paths[i]);
                                    Emotions_fill(node, emotions, img);
                                    Contempt.Add(node);
                                    break;
                                }
                        }
                        db.Images.Add(img);
                        db.SaveChanges();
                    }

                }
                Button_Calc.IsEnabled = true;
                Button_Load.IsEnabled = true;
                Button_Delete.IsEnabled = true;
                list_full_paths = new();
            }
        }

        private void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            cancelTokenSource.Cancel();
            pBar.Value = 0;
            Button_Calc.IsEnabled = true;
            Button_Load.IsEnabled = true;
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
        }

        private void Button_Click_delete(object sender, RoutedEventArgs e)
        {
            lock (sender)
            {
                if (listview != null && selected != null)
                {
                    //Node img = (Node)listview.SelectedItem;
                    using (ApplicationContext db = new ApplicationContext())
                    {
                        byte[] image_data = File.ReadAllBytes(selected.Path);
                        string hash = Image.GetHash(image_data);
                        var q = db.Images.Where(x => x.Hash == hash).Include(x => x.Blob).
                            Include(x => x.Emotions).Where(x => Equals(x.Blob.Raw_data, image_data)).First();
                        db.Images.Remove(q);
                        db.SaveChanges();
                    }
                    switch (listview.Name)
                    {
                        case "Tree_Neutral":
                            Neutral.Remove(selected);
                            break;
                        case "Tree_Happiness":
                            Happiness.Remove(selected);
                            break;
                        case "Tree_Surprise":
                            Surprise.Remove(selected);
                            break;
                        case "Tree_Sadness":
                            Sadness.Remove(selected);
                            break;
                        case "Tree_Anger":
                            Anger.Remove(selected);
                            break;
                        case "Tree_Disgust":
                            Disgust.Remove(selected);
                            break;
                        case "Tree_Fear":
                            Fear.Remove(selected);
                            break;
                        case "Tree_Contempt":
                            Contempt.Remove(selected);
                            break;
                    }
                }
                listview = null;
                selected = null;
            }
        }

    }
}
