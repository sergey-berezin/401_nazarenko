using Contracts;
using Microsoft.EntityFrameworkCore;
using Practicum1;

namespace Server
{
    public interface IImageDataBase
    {
        public Task<List<List<Image_get>>> GetDB();

        //public Task<Image_post?> TryGetImage(int id);
        public Task<int> AddImage(Image_post img);
        public Task DeleteImages();
        public Task DeleteImage(int id);
    }

    public class ApplicationContext : DbContext
    {
        public DbSet<Server_image> Images { get; set; }
        public DbSet<Emotion> Image_emotions { get; set; }

        public ApplicationContext()
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder o)
        {
            o.UseSqlite("Data Source=images1_db.db");
        }
    }

    public class DbManage : IImageDataBase
    {

        private Emotion_detector detector;

        public DbManage()
        {
            detector = new Emotion_detector();
        }

        async Task<List<List<Image_get>>> IImageDataBase.GetDB()
        {
            return await Task<List<List<Image_get>>>.Factory.StartNew(() =>
            {
                string[] emotion = new string[] { "neutral", "happiness", "surprise",
                    "sadness", "anger", "disgust", "fear", "contempt" };

                List<List<Image_get>> imagesList = new List<List<Image_get>>();

                using (var db = new ApplicationContext())
                {
                    foreach(var em in emotion)
                    {
                        var temp = db.Images.Include(x => x.Emotions).ToList();
                        var x = temp.Where(x => x.Emotions[0].emotion_name == em).ToList();
                        var x1 = x.Select(x => new Image_get { Emotions = x.Emotions, Id = x.Id, Base64 = x.Blob}).ToList();
                        imagesList.Add(x1);
                    }
                }
                return imagesList;
            },TaskCreationOptions.LongRunning);
        }

        /*
        async Task<Image?> IImageDataBase.TryGetImage(int id)
        {
            return await Task<Image?>.Factory.StartNew(() =>
            {
                Image? image = null;

                using (var db = new ApplicationContext())
                {

                        var q = db.Images.Where(x => x.Id == id).
                            Include(x => x.Emotions);
                        if (q.Any())
                            image = q.FirstOrDefault();

                }
                return image;
            },TaskCreationOptions.LongRunning);
        }
        */

        //Распаковка эмоций с нейросети
        private void Emotions_fill(in Dictionary<string, float> dict, in Server_image image)
        {
            image.Emotions = new();
            List<Tuple<string, float>> list = new List<Tuple<string, float>>();

            foreach (KeyValuePair<string, float> entry in dict)
            {
                Tuple<string, float> temp = new Tuple<string, float>(entry.Key, entry.Value);
                list.Add(temp);

            }
            var sorted = list.OrderByDescending(x => x.Item2).ToList();

            foreach (var entry in sorted)
            {
                Emotion em = new();
                em.emotion_name = entry.Item1;
                em.emotion_val = entry.Item2;
                image.Emotions.Add(em);
            }
        }

        async Task<int> IImageDataBase.AddImage(Image_post img)
        { 
            int i = -1;
            using (var db = new ApplicationContext())
            {
                var q = db.Images.Where(x => x.Hash == img.Hash);
                if (!q.Any())
                {
                    CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                    CancellationToken token = cancelTokenSource.Token;
                    var res = await detector.Start(Convert.FromBase64String(img.Blob), token);
                    Server_image image_db = new();
                    image_db.Blob = img.Blob;
                    image_db.Hash = img.Hash;
                    image_db.Name = img.Name;
                    Emotions_fill(res, image_db);
                    lock (this)
                    {
                        db.Images.Add(image_db);
                        db.SaveChanges();
                    }
                    i = image_db.Id;
                }
                else
                {
                    i = q.FirstOrDefault().Id;
                }
            }
            return i;
        }

        async Task IImageDataBase.DeleteImages()
        {
            await Task.Factory.StartNew(() =>
            {
                lock (this)
                {
                    using (var db = new ApplicationContext())
                    {
                        db.Image_emotions.RemoveRange(db.Image_emotions);
                        db.Images.RemoveRange(db.Images);
                        db.SaveChanges();
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        async Task IImageDataBase.DeleteImage(int id)
        {
            await Task.Factory.StartNew(() =>
            {
                lock (this)
                {
                    using (var db = new ApplicationContext())
                    {
                        var q = db.Images.Where(x => x.Id == id).Include(x=>x.Emotions).First();
                        db.Image_emotions.RemoveRange(q.Emotions);
                        db.Images.Remove(q);
                        db.SaveChanges();
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

    }
}
