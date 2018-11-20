using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Linq;

namespace FuncVisionApi
{
    public static class CountFacesHttpTrigger
    {
        [FunctionName("CountFacesHttpTrigger")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var formData = await req.Content.ReadAsMultipartAsync();
            var imageStream = await formData.Contents.FirstOrDefault(f => f.Headers.ContentType.MediaType.StartsWith("image/"))?.ReadAsStreamAsync();
            if (imageStream == null)
            {
                return new BadRequestObjectResult("No image specified");
            }

            ImageService svc = new ImageService();
            var detected = await svc.DetectImageFaces(imageStream);

            return new OkObjectResult($"Detected {detected.Count} people on the image");
        }
    }
}
