﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;


namespace Practicum1
{
    public class Emotion_detector
    {
        private InferenceSession session;

        public Emotion_detector()
        {

            using var modelStream = typeof(Emotion_detector).Assembly.GetManifestResourceStream("Practicum1.emotion-ferplus-8.onnx");
            using var memoryStream = new MemoryStream();
            modelStream.CopyTo(memoryStream);

            var sessionOptions = new SessionOptions();
            sessionOptions.ExecutionMode = ExecutionMode.ORT_PARALLEL;
            this.session = new InferenceSession(memoryStream.ToArray(), sessionOptions);

        }

        public async Task<Dictionary<string, float>> Start(string image_path, CancellationToken token)
        {

            return await Task<Dictionary<string, float>>.Factory.StartNew(() => {
                byte[] img = File.ReadAllBytes(image_path);
                using Image<Rgb24> image = Image.Load<Rgb24>(img);

                image.Mutate(ctx =>
                {
                    ctx.Resize(new Size(64, 64));
                });


                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("Input3", GrayscaleImageToTensor(image)) };

                float[] res;

                
                lock (session)
                {
                    using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);
                    res = results.First(v => v.Name == "Plus692_Output_0").AsEnumerable<float>().ToArray();
                }
                var emotions = Softmax(res);

                string[] keys = { "neutral", "happiness", "surprise", "sadness", "anger", "disgust", "fear", "contempt" };
                Sort(emotions, keys);

                var tupleList = new (String Name, float Value)[emotions.Length];

                Dictionary<string, float> result = new Dictionary<string, float>();

                for (int i = 0; i < emotions.Length; i++)
                {
                    if (!token.IsCancellationRequested)
                        result[keys[i]] = emotions[i];
                    else
                        return result;
                }
                return result;
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        }

        private DenseTensor<float> GrayscaleImageToTensor(Image<Rgb24> img)
        {
            var w = img.Width;
            var h = img.Height;
            var t = new DenseTensor<float>(new[] { 1, 1, h, w });

            img.ProcessPixelRows(pa =>
            {
                for (int y = 0; y < h; y++)
                {
                    Span<Rgb24> pixelSpan = pa.GetRowSpan(y);
                    for (int x = 0; x < w; x++)
                    {
                        t[0, 0, y, x] = pixelSpan[x].R;
                    }
                }
            });

            return t;
        }


        private float[] Softmax(float[] z)
        {
            var exps = z.Select(x => Math.Exp(x)).ToArray();
            var sum = exps.Sum();
            return exps.Select(x => (float)(x / sum)).ToArray();
        }

        private void Sort(in float[] emotions, in string[] keys)
        {
            for (int i = 0; i < emotions.Length; i++)
            {
                for (int j = 0; j < emotions.Length - i - 1; j++)
                {
                    if (emotions[j] < emotions[j + 1])
                    {
                        string temp_str = keys[j];
                        float temp_float = emotions[j];
                        emotions[j] = emotions[j + 1];
                        emotions[j + 1] = temp_float;
                        keys[j] = keys[j + 1];
                        keys[j + 1] = temp_str;
                    }
                }
            }
        }
    }
}