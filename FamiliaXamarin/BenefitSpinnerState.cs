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

namespace FamiliaXamarin
{
    class BenefitSpinnerState
    {
        private string title;
        private bool selected;

        public string GetTitle()
        {
            return title;
        }

        public void SetTitle(string title)
        {
            this.title = title;
        }

        public bool IsSelected()
        {
            return selected;
        }

        public void SetSelected(bool selected)
        {
            this.selected = selected;
        }
    }
}