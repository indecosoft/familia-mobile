
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Android.Support.V7.Widget;
using FamiliaXamarin;
using Org.Json;
using Android.Support.Design.Widget;
using Newtonsoft.Json;

namespace Familia.Asistentasociala
{
    [Activity(Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SearchListActivity : AppCompatActivity
    {
        List<SearchListModel> list = new List<SearchListModel>();
        SearchListAdapter mAdapter;
        FloatingActionButton fabSubmit;
        public override void OnBackPressed()
        {
            base.OnBackPressed();
            Intent returnIntent = new Intent();
            returnIntent.PutExtra("result", string.Empty);
            SetResult(Result.Canceled, returnIntent);
            Finish();
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_diseases);
            // Create your application here
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Title = string.Empty;
            //SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            //SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate { OnBackPressed(); };
            Title = "Inapoi";
            var benefitsArray = JsonConvert.DeserializeObject<List<SearchListModel>>(Intent.GetStringExtra("Items"));
            var selectedBenefits = JsonConvert.DeserializeObject<List<SearchListModel>>(Intent.GetStringExtra("SelectedItems"));
            foreach (var benefit in benefitsArray)
            {
                list.Add(new SearchListModel
                {
                    Id = benefit.Id,
                    Title = benefit.Title,
                    IsSelected = selectedBenefits.Where(el => el.Id == benefit.Id).Count() > 0
                });
            }
            list = list.OrderByDescending(el => el.IsSelected).ToList();
            mAdapter = new SearchListAdapter(this, list);
            mAdapter.ItemClick += ItemClick;

            var layoutManager = new LinearLayoutManager(this)
            { Orientation = LinearLayoutManager.Vertical };

            var recyclerView = FindViewById<RecyclerView>(Resource.Id.diseases);
            var etSearch = FindViewById<AppCompatEditText>(Resource.Id.et_search);
            fabSubmit = FindViewById<FloatingActionButton>(Resource.Id.fabSubmit);
            fabSubmit.Click += SubmitBtn;
            etSearch.TextChanged += EtSearch_TextChanged;
            recyclerView.SetLayoutManager(layoutManager);
            recyclerView.SetAdapter(mAdapter);
        }

        private void EtSearch_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            mAdapter.Search(e.Text.ToString());
        }

        private void ItemClick(object sender, SearchListAdapter.BenefitsAdapterClickEventArgs e)
        {
            mAdapter.GetItem(e.Position).IsSelected = !mAdapter.GetItem(e.Position).IsSelected;
            mAdapter.NotifyItemChanged(e.Position);
        }
        private void SubmitBtn(object sender, EventArgs e)
        {
            Intent returnIntent = new Intent();
            var selectedItems = list.Where(el => el.IsSelected == true).ToList();
            returnIntent.PutExtra("result", JsonConvert.SerializeObject(selectedItems));
            SetResult(selectedItems.Count != 0 ? Result.Ok : Result.Canceled, returnIntent);
            Finish();
        }
    }
}
