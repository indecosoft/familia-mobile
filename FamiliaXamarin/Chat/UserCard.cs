namespace Familia.Chat
{
    class UserCard
    {
        public string Name;
        public string Probleme;
        public string Email;
        public string Avatar;
        public int Url;

        public UserCard(string name, string email, string probleme, string avatar, int url)
        {
            Name = name;
            Probleme = probleme;
            Url = url;
            Email = email;
            Avatar = avatar;
        }
    }
}