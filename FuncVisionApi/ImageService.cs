using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FuncVisionApi
{
    public class ImageService
    {
        public async Task<List<FaceLocation>> DetectImageFaces(Stream image)
        {
            var apiKey = Environment.GetEnvironmentVariable("FaceApiKey");
            var apiEndpoint = Environment.GetEnvironmentVariable("FaceApiEndpoint");

            IFaceServiceClient faceServiceClient = new FaceServiceClient(apiKey, apiEndpoint);
            List<FaceLocation> faceLocations = new List<FaceLocation>();
            try
            {
                Face[] faces = await faceServiceClient.DetectAsync(image, returnFaceId: true, returnFaceLandmarks: false);
                foreach (Face face in faces)
                {
                    var faceRectangle = new FaceLocation()
                    {
                        Top = face.FaceRectangle.Top,
                        Left = face.FaceRectangle.Left,
                        Height = face.FaceRectangle.Height,
                        Width = face.FaceRectangle.Width
                    };
                    faceLocations.Add(faceRectangle);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return faceLocations;
        }

        public void DrawRectangleOnImage(Stream image, List<FaceLocation> faces, Stream outImage)
        {
            Bitmap faceBitmap = new Bitmap(image);
            using (var g = Graphics.FromImage(faceBitmap))
            {
                foreach (var face in faces)
                {
                    var faceRect = new Rectangle(face.Left, face.Top, face.Width, face.Height);
                    Pen skyBluePen = new Pen(Brushes.DeepSkyBlue);
                    skyBluePen.Width = 4.0F;
                    skyBluePen.LineJoin = System.Drawing.Drawing2D.LineJoin.Bevel;
                    g.DrawRectangle(skyBluePen, faceRect);
                    skyBluePen.Dispose();
                }

                var random = new Random();
                var randomWinnerNumber = random.Next(0, faces.Count - 1);
                var randomWinner = faces[randomWinnerNumber];
                var winnerFaceRect = new Rectangle(randomWinner.Left, randomWinner.Top, randomWinner.Width, randomWinner.Height);
                Pen winnerPen = new Pen(Brushes.OrangeRed);
                winnerPen.Width = 8.0F;
                winnerPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Bevel;
                g.DrawRectangle(winnerPen, winnerFaceRect);

                var font = new Font(FontFamily.GenericMonospace, 24.0f);
                g.DrawString($"{faces.Count} people", font, Brushes.DeepSkyBlue, 10.0f, 10.0f);

                winnerPen.Dispose();
            }
            var memStream = new MemoryStream();
            faceBitmap.Save(memStream, ImageFormat.Jpeg);
            var bw = new BinaryWriter(outImage);
            bw.Write(memStream.GetBuffer());
        }
    }

    public class FaceLocation
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
