using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Linq;
using FuncTodosApp.Entities;
using System.Collections.Generic;
using System.Net;
using Microsoft.WindowsAzure.Storage.Table;
using System.Web;

namespace FuncTodosApp
{
    public static class Functions
    {
        [FunctionName("GetAllToDos")]
        public static HttpResponseMessage GetAllToDos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/todos")]HttpRequestMessage req,
            [Table("todotable", Connection = "MyTable")]IQueryable<ToDoItem> inTable,
            ILogger log)
        {
            var queryParams = req.RequestUri.Query;

            var includeCompleted = !queryParams.Contains("includecompleted=false");
            var includeActive = !queryParams.Contains("includeactive=false");

            var items = inTable
                .Where(p => (p.IsComplete == false || includeCompleted) && (p.IsComplete == true || includeActive)).ToList()
                .Select(i => i.MapFromTableEntity()).ToList();

            return req.CreateResponse(HttpStatusCode.OK, items);
        }

        [FunctionName("GetToDo")]
        public static HttpResponseMessage GetToDo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/todos/{id}")]HttpRequestMessage req,
            [Table("todotable", Connection = "MyTable")]CloudTable table,
            string id, ILogger log)
        {
            var item = table.GetToDoFromTable(id);
            return req.CreateResponse(HttpStatusCode.OK, item);
        }

        [FunctionName("CreateToDo")]
        public static async Task<HttpResponseMessage> CreateToDo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/todos")]HttpRequestMessage req,
            [Table("todotable", Connection = "MyTable")]CloudTable table,
            ILogger log,
            ExecutionContext context)
        {;
            try
            {
                var json = await req.Content.ReadAsStringAsync();
                var todo = JsonConvert.DeserializeObject<ToDo>(json);
                await table.AddOrUpdateToDoToTable(todo);
                return req.CreateResponse(HttpStatusCode.Created, todo);
            }
            catch (Exception e)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
            }
        }

        [FunctionName("UpdateToDo")]
        public static async Task<HttpResponseMessage> UpdateToDo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "api/todos/{id}")]HttpRequestMessage req,
            [Table("todotable", Connection = "MyTable")]CloudTable table,
            string id,
            ILogger log)
        {
            var json = await req.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<ToDo>(json);

            var oldItem = await table.GetToDoFromTable(id);
            item.id = id; // ensure item id matches id passed in
            item.isComplete = oldItem.isComplete; // ensure we don't change isComplete

            await table.AddOrUpdateToDoToTable(item);

            return req.CreateResponse(HttpStatusCode.OK, item);
        }

        [FunctionName("SetCompleteToDo")]
        public static async Task<HttpResponseMessage> SetCompleteToDo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "api/todos/{id}")]HttpRequestMessage req,
            [Table("todotable", Connection = "MyTable")]CloudTable table,
            string id,
            ILogger log)
        {
            var json = await req.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<ToDo>(json);

            var oldItem = await table.GetToDoFromTable(id);
            oldItem.isComplete = item.isComplete;

            await table.AddOrUpdateToDoToTable(oldItem);
            return req.CreateResponse(HttpStatusCode.OK, oldItem);
        }

        [FunctionName("DeleteToDo")]
        public static async Task<HttpResponseMessage> DeleteToDo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "api/todos/{id}")]HttpRequestMessage req,
            string id, [Table("todotable", Connection = "MyTable")]CloudTable table,
            ILogger log)
        {
            await table.DeleteToDoFromTable(id);
            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
