using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace Practicum_berezin_2
{
    public class Image
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Hash { get; set; }
        public Image_directly Blob { get; set; }
        public List<Emotion> Emotions { get; set; }

        public static string GetHash(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                return string.Concat(sha256.ComputeHash(data).Select(x => x.ToString("X2")));
            }
        }

    }

    public class Image_directly
    {
        public int Id { get; set; }
        public Image image { get; set; }
        public byte[] Raw_data { get; set; }
        public int ImageId { get; set; }
    }

    public class Emotion
    {
        public int Id { get; set; } 
        public string emotion_name { get; set; }
        public float emotion_val { get; set; }
        public int ImageId { get; set; } 
        public Image image { get; set; }

    }


    public class ApplicationContext : DbContext
    {
        public DbSet<Image> Images { get; set; }
        public DbSet<Image_directly> Blobs { get; set; }
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
}
