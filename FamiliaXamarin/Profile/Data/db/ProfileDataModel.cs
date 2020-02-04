using SQLite;

namespace Familia.Profile.Data.db
{
    class ProfileDataModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Base64Image { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string ImageName { get; set; }
        public string ImageExtension { get; set; }
    }
}