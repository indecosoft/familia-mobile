using SQLite;

namespace Familia.Profile.Data.db
{
    class DiseaseDataModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int Cod { get; set; }
        public string Name { get; set; }
    }
}