using System;
using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FamiliaXamarin;
using System.Linq;

namespace Familia.Asistentasociala
{
    public class SearchListAdapter : RecyclerView.Adapter
    {
        public event EventHandler<BenefitsAdapterClickEventArgs> ItemClick;
        private List<SearchListModel> _benefits, _benefitsCopy;
        private LayoutInflater mInflater;

        public SearchListAdapter(Context context, List<SearchListModel> benefits)
        {
            mInflater = LayoutInflater.From(context);
            _benefits = benefits;
            _benefitsCopy = benefits;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            int layout = Resource.Layout.item_benefit;

            var itemView = mInflater.Inflate(layout, parent, false);

            var viewHolder = new SucHolder(itemView, OnClick);
            return viewHolder;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as SucHolder;

            var benefit = _benefits[position];
            if (viewHolder == null) return;
            viewHolder.Name.Text = benefit.Title;
            viewHolder.IsSelected.Checked = benefit.IsSelected;
            viewHolder.IsSelected.CheckedChange += delegate (object sender, CompoundButton.CheckedChangeEventArgs e)
            {
                benefit.IsSelected = e.IsChecked;
                };
        }
        void OnClick(BenefitsAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);

        //This will fire any event handlers that are registered with our ItemClick
        //event.
        public override int ItemCount => _benefits.Count;

        public SearchListModel GetItem(int position) => _benefits[position];

        public void Search(string textToSearch)
        {
            _benefits = _benefitsCopy.Where(c => c.Title.ToLower().StartsWith(textToSearch.ToLower(), StringComparison.Ordinal)).ToList();
            NotifyDataSetChanged();
        }
        
        class SucHolder : RecyclerView.ViewHolder
        {
            public TextView Name { get; set; }
            public CheckBox IsSelected { get; set; }
            public SucHolder(View itemView, Action<BenefitsAdapterClickEventArgs> clickListener) : base(itemView)
            {
                itemView.Click += (sender, e) => clickListener(new BenefitsAdapterClickEventArgs { View = itemView, Position = AdapterPosition});
                Name = itemView.FindViewById<TextView>(Resource.Id.name);
                IsSelected = itemView.FindViewById<CheckBox>(Resource.Id.isSelected);
              
            }

        }
        public class BenefitsAdapterClickEventArgs : EventArgs
        {
            public View View { get; set; }
            public int Position { get; set; }
        }
    }
}
