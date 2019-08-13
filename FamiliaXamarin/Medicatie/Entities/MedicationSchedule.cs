namespace FamiliaXamarin.Medicatie.Entities
{
    public class MedicationSchedule
    {
        public string Uuid { get; set; }
        public string Timestampstring { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Postpone { get; set; }
        public int IdNotification { get; set; }

        public MedicationSchedule(string uuid, string timestampstring, string title, string content, int postpone, int idNotification)
        {
            Uuid = uuid;
            Timestampstring = timestampstring;
            Title = title;
            Content = content;
            Postpone = postpone;
            IdNotification = idNotification;
        }

        public string ToString()
        {
            return "Uuid: " + Uuid + " Timestampstring: " + Timestampstring + " Title: " + Title + " Content: " +
                   Content + " Postpone" + Postpone + " IdNotification" + IdNotification;
        }
    }
}