using SQLite;

namespace Familia.DataModels
{
    class MedicineRecords
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Uuid { get; set; }
        public string DateTime { get; set; }

       
    }
}