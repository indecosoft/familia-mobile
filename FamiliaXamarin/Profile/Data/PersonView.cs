using System.Collections.Generic;

namespace Familia.Profile.Data
{
    class PersonView
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Birthdate { get; set; }
        public string Gender { get; set; }
        public string Avatar { get; set; }
        public string LastUpdated { get; set; }
        public List<PersonalDisease> ListOfPersonalDiseases { get; set; }

        public PersonView(string name, string email, string birthdate, string gender, string avatar, List<PersonalDisease> list, string lastUpdated)
        {
            Name = name;
            Email = email;
            Birthdate = birthdate;
            Gender = gender;
            Avatar = avatar;
            ListOfPersonalDiseases = list;
            LastUpdated = lastUpdated;
        }
        
    }
}