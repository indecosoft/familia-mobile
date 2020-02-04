namespace Familia.Medicatie.Entities
{
    public class Hour
    {
        public string HourName { get; set; }
        public string Id { get; set; }

        public Hour(string hourName, string id)
        {
            HourName = hourName;
            Id = id;
        }
        public Hour() { }
    }


}