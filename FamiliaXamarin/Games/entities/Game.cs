using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

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