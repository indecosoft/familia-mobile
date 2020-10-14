namespace Familia.Chat
{
    class UserCard
    {
        public string Name;
        public string Problems;
        public string Email;
        public string Avatar;
        public int BackroundResourceId;

        public UserCard(string name, string email, string problems, string avatar, int backroundResourceId)
        {
            Name = name;
            Problems = problems;
            BackroundResourceId = backroundResourceId;
            Email = email;
            Avatar = avatar;
        }
    }
}