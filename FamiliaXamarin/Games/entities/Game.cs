using System.Collections.Generic;

namespace Familia.Games.entities
{
    class Game
    {
        public string Name;

        public int Type;

        public List<Category> Categories;

        public Game(string name, int type, List<Category> categories) {
            Name = name;
            Type = type;
            Categories = new List<Category>(categories);
        }

    }
}