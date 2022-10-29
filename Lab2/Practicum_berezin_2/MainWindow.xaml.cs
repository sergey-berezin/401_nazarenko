
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.IO;
using System.Collections.ObjectModel;
using Practicum1;
using Ookii.Dialogs.Wpf;

namespace Practicum_berezin_2
{
    public partial class MainWindow : Window
    {
        private List<string> list_full_paths;
        private List<string> list_names;
        private CancellationTokenSource cancelTokenSource;
        private CancellationToken token;
        private Emotion_detector detector;

        private class Node
        {
            public Node(string name)
            {
                this.Name = name;
                this.Emotions = new ObservableCollection<Node>();
            }
            public string Name { get; set; }
            public ObservableCollection<Node> Emotions { get; set; }
        }

        private ObservableCollection<Node> Neutral;
        private ObservableCollection<Node> Happiness;
        private ObservableCollection<Node> Surprise;
        private ObservableCollection<Node> Sadness;
        private ObservableCollection<Node> Anger;
        private ObservableCollection<Node> Disgust;
        private ObservableCollection<Node> Fear;
        private ObservableCollection<Node> Contempt;

        public MainWindow()
        {
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
            detector = new Emotion_detector();
            list_full_paths = new List<string>();
            list_names = new List<string>();
            InitializeComponent();
        }

        private void Button_Load_Images(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dlg = new VistaFolderBrowserDialog();
            
            dlg.ShowNewFolderButton = true;

            string path = null;
            if (dlg.ShowDialog() == true)
            {
               path = dlg.SelectedPath;
            }

            DirectoryInfo d = new DirectoryInfo(path);
            FileInfo[] Files = d.GetFiles("*.*").Where(x => x.Name.EndsWith(".png") || x.Name.EndsWith(".jpg")).ToArray();

            list_full_paths = new List<string>();
            list_names = new List<string>();

            foreach (FileInfo File in Files)
            {
                list_full_paths.Add(File.FullName);
                list_names.Add(File.Name);
            }

            Neutral = new ObservableCollection<Node>();
            Happiness = new ObservableCollection<Node>();
            Surprise = new ObservableCollection<Node>();
            Sadness = new ObservableCollection<Node>();
            Anger = new ObservableCollection<Node>();
            Disgust = new ObservableCollection<Node>();
            Fear = new ObservableCollection<Node>();
            Contempt = new ObservableCollection<Node>();


            Tree_Neutral.ItemsSource = new ObservableCollection<Node>();
            Tree_Happiness.ItemsSource = new ObservableCollection<Node>();
            Tree_Contempt.ItemsSource = new ObservableCollection<Node>();
            Tree_Disgust.ItemsSource = new ObservableCollection<Node>();
            Tree_Fear.ItemsSource = new ObservableCollection<Node>();
            Tree_Sadness.ItemsSource = new ObservableCollection<Node>();
            Tree_Surprise.ItemsSource = new ObservableCollection<Node>();
            Tree_Anger.ItemsSource = new ObservableCollection<Node>();

            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;

        }


        private void Button_Start_Calc(object sender, RoutedEventArgs e)
        {
            Start_Calc();
        }


        private string Find_most_likely_emotion(in Dictionary<string, float> dict)
        {
            foreach (KeyValuePair<string, float> entry in dict)
            {
                return entry.Key;
            }
            return null;
        }

        private void Emotions_fill(in ObservableCollection<Node> Emotions, in Dictionary<string, float> dict)
        {
            foreach (KeyValuePair<string, float> entry in dict)
            {
                string str = ($"{entry.Key}: {entry.Value}");
                Emotions.Add(new Node(str));
            }
        }

        private async void Start_Calc()
        {

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

    
            for(int i = 0; i < list_full_paths.Count; i++)
            {
                var emotions = tasks[i].Result;
                Node node;
                switch (Find_most_likely_emotion(emotions))
                {
                    case "neutral":
                        {
                            node = new Node(list_names[i]);
                            Emotions_fill(node.Emotions, emotions);
                            Neutral.Add(node);
                            break;
                        }
                    case "happiness":
                        {
                            node = new Node(list_names[i]);
                            Emotions_fill(node.Emotions, emotions);
                            Happiness.Add(node);
                            break;
                        }
                    case "surprise":
                        {
                            node = new Node(list_names[i]);
                            Emotions_fill(node.Emotions, emotions);
                            Surprise.Add(node);
                            break;
                        }
                    case "sadness":
                        {
                            node = new Node(list_names[i]);
                            Emotions_fill(node.Emotions, emotions);
                            Sadness.Add(node);
                            break;
                        }
                    case "anger":
                        {
                            node = new Node(list_names[i]);
                            Emotions_fill(node.Emotions, emotions);
                            Anger.Add(node);
                            break;
                        }
                    case "disgust":
                        {
                            node = new Node(list_names[i]);
                            Emotions_fill(node.Emotions, emotions);
                            Disgust.Add(node);
                            break;
                        }
                    case "fear":
                        {
                            node = new Node(list_names[i]);
                            Emotions_fill(node.Emotions, emotions);
                            Fear.Add(node);
                            break;
                        }
                    case "contempt":
                        {
                            node = new Node(list_names[i]);
                            Emotions_fill(node.Emotions, emotions);
                            Contempt.Add(node);
                            break;
                        }
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

        }

        private void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            cancelTokenSource.Cancel();
        }
    }
}
