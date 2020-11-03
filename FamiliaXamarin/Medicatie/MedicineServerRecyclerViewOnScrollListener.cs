using System;
using Android.Util;
using AndroidX.RecyclerView.Widget;

namespace Familia.Medicatie
{
    class MedicineServerRecyclerViewOnScrollListener : RecyclerView.OnScrollListener
    {
        public delegate void LoadMoreEventHandler(object sender, EventArgs e);
        public event LoadMoreEventHandler LoadMoreEvent;

        private LinearLayoutManager LayoutManager;

        public MedicineServerRecyclerViewOnScrollListener(LinearLayoutManager layoutManager)
        {
            LayoutManager = layoutManager;
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            base.OnScrolled(recyclerView, dx, dy);

            int visibleItemCount = recyclerView.ChildCount;
            int totalItemCount = recyclerView.GetAdapter().ItemCount;
            int pastVisiblesItems = LayoutManager.FindFirstVisibleItemPosition();

            if ((visibleItemCount + pastVisiblesItems) >= totalItemCount)
            {
                Log.Error("ONSCROLLED ", visibleItemCount + ", " + totalItemCount + " "+ pastVisiblesItems);

                LoadMoreEvent(this, null);
            }
        }

    }
}