using System;
using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Com.Airbnb.Lottie.Utils;
using MikePhil.Charting.Charts;
using MikePhil.Charting.Components;
using MikePhil.Charting.Data;
using MikePhil.Charting.Highlight;
using MikePhil.Charting.Interfaces.Datasets;
using MikePhil.Charting.Listener;
using Newtonsoft.Json;
using Org.Json;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Utils = FamiliaXamarin.Helpers.Utils;


namespace FamiliaXamarin.Sharing
{
    [Activity(Label = "PressureAndGlucoseActivity")]
    public class PressureAndGlucoseActivity : AppCompatActivity, IOnChartValueSelectedListenerSupport
    {

        class PressureModel
        {
            public int Systolic { get; set; }
            public int Diastolic { get; set; }
            public int PulseRate { get; set; }
            public DateTime Data { get; set; }
        }

        private string _name, _email, _imei;
        List<int> _systolicColors = new List<int>();
        List<int> _diastolicolors = new List<int>();
        List<int> _pulseColors = new List<int>();
        List<PressureModel> _bloodPressureDataList = new List<PressureModel>();

        private async void InitUi()
        {

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            SetSupportActionBar(toolbar);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate
            {
//                var intent = new Intent(this, typeof(MainActivity));
//                intent.AddFlags(ActivityFlags.ClearTop);
//                StartActivity(inent);
                OnBackPressed();
            };

            _name = Intent.GetStringExtra("Name");
            _email = Intent.GetStringExtra("Email");
            _imei = Intent.GetStringExtra("Imei");
   
            switch (Intent.GetStringExtra("DataType"))
            {

                case "BloodPressure":
                    Title = $"Tensiune: {_name}";
                    try
                    {
                        var result = await WebServices.Post($"{Constants.PublicServerAddress}/api/getUsersDataSharing",
                            new JSONObject().Put("dataType", "bloodPressure").Put("imei", _imei)
                                .Put("date", "2018/12/13"), Utils.GetDefaults("Token", this));
                        if (!string.IsNullOrEmpty(result))
                        {
                            var dataArray = new JSONArray(result);
                            
                            for (int i = 0; i < dataArray.Length(); i++)
                            {
                               _bloodPressureDataList.Add(JsonConvert.DeserializeObject<PressureModel>(dataArray.GetJSONObject(i).ToString()));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    break;
                case "BloodGlucose":
                    Title = $"Glicemie: {_name}";
                    break;
            }
            BarChart chart = (BarChart)FindViewById(Resource.Id.chart);
          
            List<BarEntry> systolicEntries = new List<BarEntry>();
            List<BarEntry> diastolicEntries = new List<BarEntry>();
            List<BarEntry> pulseEntries = new List<BarEntry>();

            for (int i = 0; i < _bloodPressureDataList.Count; i++)
            {
                if(_bloodPressureDataList[i].Systolic > 139 || _bloodPressureDataList[i].Systolic < 120)
                {
                    _systolicColors.Add(Resource.Color.highHealthValue);
                }
                else
                {
                    _systolicColors.Add(Resource.Color.normalHealthValue);
                }
                if (_bloodPressureDataList[i].Diastolic > 88 || _bloodPressureDataList[i].Diastolic < 80)
                {
                    _diastolicolors.Add(Resource.Color.highHealthValue);
                }
                else
                {
                    _diastolicolors.Add(Resource.Color.normalHealthValue);
                }
                if (_bloodPressureDataList[i].PulseRate > 90 || _bloodPressureDataList[i].PulseRate < 55)
                {
                    _pulseColors.Add(Resource.Color.highHealthValue);
                }
                else
                {
                    _pulseColors.Add(Resource.Color.normalHealthValue);
                }


                systolicEntries.Add(new BarEntry(i, _bloodPressureDataList[i].Systolic, Resource.Drawable.design_password_eye));
                diastolicEntries.Add(new BarEntry(i, _bloodPressureDataList[i].Diastolic, Resource.Drawable.design_password_eye));
                pulseEntries.Add(new BarEntry(i, _bloodPressureDataList[i].PulseRate, Resource.Drawable.design_password_eye));
            }
            BarDataSet SdataSet = new BarDataSet(systolicEntries, "Sistola");
            SdataSet.HighLightColor = Color.Lime;
            SdataSet.ValueTextSize = 16f;
            SdataSet.SetColors(_systolicColors.ToArray(), this);
            BarDataSet DdataSet = new BarDataSet(diastolicEntries, "Diastola");
            DdataSet.HighLightColor = Color.Lime;
            DdataSet.ValueTextSize = 16f;
            DdataSet.SetColors(_diastolicolors.ToArray(), this);
            BarDataSet PdataSet = new BarDataSet(pulseEntries, "Puls");
            PdataSet.HighLightColor = Color.Lime;
            PdataSet.ValueTextSize = 16f;
            PdataSet.SetColors(_pulseColors.ToArray(), this);
            BarData lineData = new BarData(new IBarDataSet[]{SdataSet, DdataSet, PdataSet});
            chart.Data = lineData;
            chart.Invalidate();
            chart.SetOnChartValueSelectedListener(this);
            chart.SetDrawBorders(false);
            chart.SetDrawGridBackground(false);
            chart.SetNoDataText("Nu exista date");
            chart.Description.Enabled = false;
            chart.AxisLeft.SetDrawGridLines(false);
            chart.AxisRight.SetDrawGridLines(false);
            chart.XAxis.SetDrawGridLines(false);

            chart.XAxis.Enabled = false;
            //            chart.AxisLeft.Enabled = false;
            chart.AxisRight.Enabled = false;
            chart.SetFitBars(true);
            chart.XAxis.AxisMaximum = _bloodPressureDataList.Count*3;
//            chart.BarData.BarWidth = 0.1f;
            chart.GroupBars(0, 0.04f, 0.02f);
            chart.AnimateXY(3000, 3000); // animate horizontal and vertical 3000 milliseconds

            Legend l = chart.Legend;
            l.FormSize = 10f; // set the size of the legend forms/shapes
            l.Form = Legend.LegendForm.Circle; // set what type of form/shape should be used
            l.Position = Legend.LegendPosition.BelowChartCenter;
            l.TextSize = 12f;
            l.TextColor = Color.Black;
            l.XEntrySpace = 5f; // set the space between the legend entries on the x-axis
            l.YEntrySpace = 5f; // set the space between the legend entries on the y-axis

        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_pressure_and_glucose);
            InitUi();
            // Create your application here
        }

        public void OnNothingSelected()
        {
            
        }

        public void OnValueSelected(Entry e, Highlight h)
        {
            Toast.MakeText(this, e.GetY().ToString(), ToastLength.Short).Show();
        }
    }
}