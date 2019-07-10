
using SQLite;
namespace Familia.DataModels
{
    class MedicineServerRecords
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Uuid { get; set; }
        public string DateTime { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public string Postpone { get; set; }
    }
}