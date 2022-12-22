using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class Server_image
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public string Blob { get; set; }
        public string Name { get; set; }
        public List<Emotion> Emotions { get; set; } = null;

    }


    public class Emotion
    {
        public int Id { get; set; }
        public string emotion_name { get; set; }
        public float emotion_val { get; set; }
    }
}
