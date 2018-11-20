using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace FuncTodosApp.Entities
{
    public static class CloudTableExtensions
    {
        public static async Task AddOrUpdateToDoToTable(this CloudTable table, ToDo todo)
        {
            if (string.IsNullOrEmpty(todo.id))
            {
                todo.id = Guid.NewGuid().ToString();
            }

            var saveOperation = TableOperation.InsertOrReplace(todo.MapToTableEntity());
            await table.ExecuteAsync(saveOperation);
        }

        public static async Task<ToDo> GetToDoFromTable(this CloudTable table, string id)
        {
            var retrieveOperation = TableOperation.Retrieve<ToDoItem>("ToDoItem", id);
            var item = await table.ExecuteAsync(retrieveOperation);
            return ((ToDoItem)item.Result).MapFromTableEntity();
        }

        public static async Task DeleteToDoFromTable(this CloudTable table, string id)
        {
            var item = new ToDoItem { RowKey = id, ETag = "*" };
            var deleteOperation = TableOperation.Delete(item);
            await table.ExecuteAsync(deleteOperation);
        }
    }
}
