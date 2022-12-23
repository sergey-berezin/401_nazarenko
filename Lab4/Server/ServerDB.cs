using Contracts;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Practicum1;

namespace Server
{
    public interface IImageDataBase
    {
        public Task<int[]?> GetIds();
        public Task<Image?> TryGetImage(int id);
        public Task<int> AddImage(Image img);
        public Task DeleteImages();
        public Task DeleteImage(int id);
    }

    public class ApplicationContext : DbContext
    {
        public DbSet<Image> Images { get; set; }
        public DbSet<Emotion> Image_emotions { get; set; }

        public ApplicationContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder o)
        {
            o.UseSqlite("Data Source=images_db.db");
        }
    }

    public class DbManage : IImageDataBase
    {

        private Emotion_detector detector;

        public DbManage()
        {
            detector = new Emotion_detector();
        }

        async Task<int[]?> IImageDataBase.GetIds()
        {
            return await Task<int[]?>.Factory.StartNew(() =>
            {
                int[]? images = null;
                using (var db = new ApplicationContext())
                {
                        images = db.Images.Select(x => x.Id).ToArray();
                }
                return images;
            },TaskCreationOptions.LongRunning);
        }

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

        private void Emotions_fill(in Dictionary<string, float> dict, in Image image)
        {
            image.Emotions = new();
            foreach (KeyValuePair<string, float> entry in dict)
            {
                Emotion em = new();
                em.emotion_name = entry.Key;
                em.emotion_val = entry.Value;
                image.Emotions.Add(em);
            }
        }

        async Task<int> IImageDataBase.AddImage(Image img)
        { 
            int i = -1;
            using (var db = new ApplicationContext())
            {

                    var q = db.Images.Where(x => x.Hash == img.Hash);
                    if (!q.Any())
                    {
                        img.Hash = Image.GetHash(Convert.FromBase64String(img.Blob));
                        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                        CancellationToken token = cancelTokenSource.Token;
                        var res = await detector.Start(Convert.FromBase64String(img.Blob), token);
                        Emotions_fill(res, img);
                        lock (this)
                        {
                            db.Images.Add(img);
                            db.SaveChanges();
                        }
                        i = img.Id;
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
