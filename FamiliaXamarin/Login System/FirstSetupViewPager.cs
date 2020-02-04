using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Java.Lang;
using Java.Lang.Reflect;
using Exception = System.Exception;

namespace Familia.Login_System
{
    public class FirstSetupViewPager : ViewPager, View.IOnTouchListener
    {
        bool _enabled;

        protected FirstSetupViewPager(IntPtr javaReference, JniHandleOwnership transfer) : base(
            javaReference, transfer)
        {
        }

        public FirstSetupViewPager(Context context) : base(context)
        {
           // SetMyScroller();
        }

        public FirstSetupViewPager(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            //SetMyScroller();
        }


        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            // Never allow swiping to switch between pages
            return _enabled;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            return _enabled;
        }

        public void SetPagingEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        void SetMyScroller()
        {
            try
            {
                Class viewpager = Class.FromType(typeof(ViewPager));


                Field scroller = viewpager.GetDeclaredField("mScroller");
                scroller.Accessible = true;
                scroller.Set(this, new MyScroller(Context));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        class MyScroller : Scroller
        {
            public MyScroller(Context context) : base(context, new DecelerateInterpolator())
            {
            }

            public override void StartScroll(int startX, int startY, int dx, int dy, int duration)
            {
                base.StartScroll(startX, startY, dx, dy, 350 /*1 secs*/);
            }
        }
    }
}