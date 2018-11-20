using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Linq;
using System;

namespace FuncVisionApi
{
    public static class FaceDetectHttpTrigger
    {
        [FunctionName("FaceDetectHttpTrigger")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "FaceDetectHttpTrigger/{imageName}")] HttpRequestMessage req,
            [Blob("facedetectscontainer-edits/{imageName}.{sys.utcnow}.jpeg", FileAccess.Write, Connection = "face_detect_storage")] Stream outBlob,
            string imageName,
            ILogger log)
        {
            log.LogInformation($"FaceDetectHttpTrigger func for {imageName}.{DateTime.UtcNow}");

            var imageStream = Task.Run(async () =>
            {
                var formData = await req.Content.ReadAsMultipartAsync();
                return await formData.Contents.FirstOrDefault(f => f.Headers.ContentType.MediaType.StartsWith("image/"))?.ReadAsStreamAsync();
            }).Result;

            var imageService = new ImageService();
            var detected = Task.Run(async () =>
            {
                return await imageService.DetectImageFaces(imageStream);
            }).Result;

            if (detected.Count > 0)
            {
                imageService.DrawRectangleOnImage(imageStream, detected, outBlob);
                var memStream = new MemoryStream();
                imageService.DrawRectangleOnImage(imageStream, detected, memStream);
                var outImage = memStream.GetBuffer();
                return new FileContentResult(outImage, "image/jpeg");
            }

            return new OkObjectResult("No faces detected");
        }
    }
}
