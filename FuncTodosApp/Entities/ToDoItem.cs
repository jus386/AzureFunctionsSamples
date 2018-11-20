using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace FuncTodosApp.Entities
{
    public class ToDoItem : TableEntity
    {
        public ToDoItem()
        {
            PartitionKey = "ToDoItem";
        }

        public string Title { get; set; }
        public string Description { get; set; }
        
        public DateTime? Due { get; set; }
        public bool IsComplete { get; set; }
    }
}
