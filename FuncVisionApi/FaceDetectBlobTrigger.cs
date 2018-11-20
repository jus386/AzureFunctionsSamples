using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FuncVisionApi
{
    public static class FaceDetectBlobTrigger
    {
        [FunctionName("FaceDetectBlobTrigger")]
        public static void Run(
            [BlobTrigger("facedetectscontainer/{name}", Connection = "face_detect_storage")]Stream imageStream,
            [Blob("facedetectscontainer-edits/{name}", FileAccess.Write, Connection = "face_detect_storage")] Stream outBlob,
            string name,
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {imageStream.Length} Bytes");

            //string lowerCaseName = name.ToLower();
            ImageService imageService = new ImageService();

            var facesResult = Task.Run(async () =>
            {
                return await imageService.DetectImageFaces(imageStream);
            }).Result;
            if (facesResult.Count > 0)
            {
                imageService.DrawRectangleOnImage(imageStream, facesResult, outBlob);
            }
        }
    }
}
