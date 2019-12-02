using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorMart.Server.Models
{
    public class BlazorMartDatabaseSettings : IBlazorMartDatabaseSettings
    {
        public string InventoryCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IBlazorMartDatabaseSettings
    {
        string InventoryCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
