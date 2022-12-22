using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Contracts
{
    public class Image_post
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public string Blob { get; set; }
        public string Name { get; set; }
    }

    public class Image_get
    {
        public int Id { get; set; }
        public string Base64 { get; set; }
        public List<Emotion> Emotions { get; set; } = null;
    }

}