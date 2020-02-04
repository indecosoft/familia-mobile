using System.Collections.Generic;
using Android.Support.V4.App;
using Android.Util;
using Java.Lang;

namespace Familia.Sharing
{
    public class SharingPagerAdapter: FragmentStatePagerAdapter
    {
        
        private readonly List<Fragment> mFragmentList = new List<Fragment>();
        private readonly List<String> mTitleList = new List<String>();
        
    
        public SharingPagerAdapter(FragmentManager fm) : base(fm)
        {
        }

        public override Fragment GetItem(int position)
        {
            return mFragmentList[position];
        }
    
        public void AddFragment(Fragment fragment, String title) {
            mFragmentList.Add(fragment);
            Log.Error("TITLE", title +"");
            mTitleList.Add(title);
        }

    public override ICharSequence GetPageTitleFormatted(int position)
    {
        return  mTitleList[position];
   }

    public override int Count => mFragmentList.Count;
    }
}