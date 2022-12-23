using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Contracts
{
    public class Image
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public string Blob { get; set; }
        public string Name { get; set; }
        public List<Emotion>? Emotions { get; set; } = null;

        public static string GetHash(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                return string.Concat(sha256.ComputeHash(data).Select(x => x.ToString("X2")));
            }
        }

    }

    public class Emotion
    {
        public int Id { get; set; }
        public string emotion_name { get; set; }
        public float emotion_val { get; set; }

    }
}