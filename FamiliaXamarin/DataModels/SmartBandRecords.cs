using Org.Json;
using SQLite;

namespace Familia.DataModels
{
    public class SmartBandRecords
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string DataObject { get; set; }
        public string Type { get; set; }
    }
}