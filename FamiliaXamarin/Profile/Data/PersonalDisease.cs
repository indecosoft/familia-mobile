namespace Familia.Profile.Data
{
    class PersonalDisease
    {
        public int Id { get; set; }
        public int Cod { get; set; }
        public string Name { get; set; }

        public PersonalDisease(int cod, string name)
        {
            Id++;
            Cod = cod;
            Name = name;
        }

       

    }
}