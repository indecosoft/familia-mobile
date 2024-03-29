﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Familia.Helpers;
using MikePhil.Charting.Charts;
using MikePhil.Charting.Components;
using MikePhil.Charting.Data;
using MikePhil.Charting.Highlight;
using MikePhil.Charting.Listener;
using Newtonsoft.Json;
using Org.Json;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Familia.Sharing
{
    [Activity(Label = "PressureAndGlucoseActivity", Theme = "@style/AppTheme.Dark", ScreenOrientation = ScreenOrientation.Portrait)]
    public class PressureAndGlucoseActivity : AppCompatActivity, IOnChartValueSelectedListenerSupport
    {
        private class PressureModel
        {
            public int Systolic { get; set; }
            public int Diastolic { get; set; }
            public int PulseRate { get; set; }
            public DateTime Data { get; set; }
        }
        private class GlucoseModel
        {
            public int Avg { get; set; }
            public DateTime Data { get; set; }
        }
        private string _name, _email, _imei;
        private readonly List<PressureModel> _bloodPressureDataList = new List<PressureModel>();
        private readonly List<GlucoseModel> _bloodGlucoseDataList = new List<GlucoseModel>();
        private Drawable buttonBackground;
        private Drawable buttonBackgroundA;
        private Drawable buttonBackgroundDatePicker;
        private LinearLayout layoutButtons;
        private LinearLayout horizontalScrollLinearLayout;
        private LinearLayout verticalScrollLinearLayout;
        private LineChart _lineChart;
        private HorizontalScrollView _scrollViewButtons;
        private BottomNavigationView bottomNavigation;
        private TextView _tvDate;

        private string _dataType;
        // Summary:
        //     Specifies the day of the week.
        [Serializable]
        [ComVisible(true)]
        public enum DayOfWeek
        {
            //
            // Summary:
            //     Indicates Monday.
            Monday = 0,
            //
            // Summary:
            //     Indicates Tuesday.
            Tuesday = 1,
            //
            // Summary:
            //     Indicates Wednesday.
            Wednesday = 2,
            //
            // Summary:
            //     Indicates Thursday.
            Thursday = 3,
            //
            // Summary:
            //     Indicates Friday.
            Friday = 4,
            //
            // Summary:
            //     Indicates Saturday.
            Saturday = 5,
            // Summary:
            //     Indicates Sunday.
            Sunday = 6
        }
        private void InitUi()
        {
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            SetSupportActionBar(toolbar);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
            toolbar.NavigationClick += delegate
            {
                OnBackPressed();
            };

            verticalScrollLinearLayout = FindViewById<LinearLayout>(Resource.Id.LinearLayout);
            horizontalScrollLinearLayout = FindViewById<LinearLayout>(Resource.Id.linearLayout2);
            _scrollViewButtons = FindViewById<HorizontalScrollView>(Resource.Id.ScrollViewButtons);
            _tvDate = FindViewById<TextView>(Resource.Id.tvDate);

            buttonBackground = Resources.GetDrawable(Resource.Drawable.sharing_round_button_inactiv, Theme);
            buttonBackgroundA = Resources.GetDrawable(Resource.Drawable.sharing_round_button_activ, Theme);
            buttonBackgroundDatePicker = Resources.GetDrawable(Resource.Drawable.sharing_date_button_style, Theme);
            bottomNavigation = FindViewById<BottomNavigationView>(Resource.Id.top_navigation);
            _lineChart = FindViewById<LineChart>(Resource.Id.chart);
            bottomNavigation.NavigationItemSelected += BottomNavigationOnNavigationItemSelected;

            _name = Intent.GetStringExtra("Name");
            _email = Intent.GetStringExtra("Email");
            _imei = Intent.GetStringExtra("Imei");
            _dataType = Intent.GetStringExtra("DataType");
            switch (_dataType)
            {
                case "BloodPressure":
                    Title = $"Tensiune: {_name}";
                    break;
                case "BloodGlucose":
                    Title = $"Glicemie: {_name}";
                    break;
            }
            bottomNavigation.SelectedItemId = Resource.Id.week_tab;
            bottomNavigation.PerformClick();
            //LoadDaySelectorButtons();
            _tvDate.Text = DateTime.Now.ToShortDateString();
        }

        private async void GetDataForChart(DateTime startDate, bool dataType = true)
        {
            try
            {

                if (dataType)
                {
                    _bloodPressureDataList.Clear();
                    string bloodPressureResult = await WebServices.WebServices.Post($"{Constants.PublicServerAddress}/api/getUsersDataSharing",
                        new JSONObject().Put("dataType", "bloodPressure").Put("imei", _imei)
                            .Put("date", startDate.ToString("yyyy-MM-dd")), Utils.GetDefaults("Token"));
                    if (!string.IsNullOrEmpty(bloodPressureResult))
                    {
                        var dataArray = new JSONArray(bloodPressureResult);

                        for (var i = 0; i < dataArray.Length(); i++)
                        {
                            _bloodPressureDataList.Add(JsonConvert.DeserializeObject<PressureModel>(dataArray.GetJSONObject(i).ToString()));
                        }
                    }

                    CreateBloodPresureChart();
                    verticalScrollLinearLayout.RemoveAllViewsInLayout();
                    horizontalScrollLinearLayout.RemoveAllViewsInLayout();
                    LoadDataInScrollLayouts(DateTime.Now);
                }
                else
                {
                    _bloodGlucoseDataList.Clear();
                    string bloodGlucoseResult = await WebServices.WebServices.Post($"{Constants.PublicServerAddress}/api/getUsersDataSharing",
                        new JSONObject().Put("dataType", "bloodGlucose").Put("imei", _imei)
                            .Put("date", startDate.ToString("yyyy-MM-dd")), Utils.GetDefaults("Token"));
                    if (!string.IsNullOrEmpty(bloodGlucoseResult))
                    {
                        var dataArray = new JSONArray(bloodGlucoseResult);

                        for (var i = 0; i < dataArray.Length(); i++)
                        {
                            _bloodGlucoseDataList.Add(JsonConvert.DeserializeObject<GlucoseModel>(dataArray.GetJSONObject(i).ToString()));
                        }
                    }
                    CreateGlucoseChart();
                    verticalScrollLinearLayout.RemoveAllViewsInLayout();
                    horizontalScrollLinearLayout.RemoveAllViewsInLayout();
                    LoadDataInScrollLayouts(DateTime.Now,false);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private void BottomNavigationOnNavigationItemSelected(object sender, BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            switch (e.Item.ItemId)
            {
                case Resource.Id.week_tab:
                    LoadDaySelectorButtons();
                    break;
                case Resource.Id.month_tab:
                    LoadDaySelectorButtons(2);
                    break;
                case Resource.Id.year_tab:
                    LoadDaySelectorButtons(3);
                    break;
            }
        }

        private void CreateGlucoseChart()
        {
            if (_bloodGlucoseDataList.Count == 0)
            {
                _lineChart.Data.ClearValues();
                _lineChart.Clear();
                _lineChart.Invalidate();

                return;
            }
            var glucoseEntries = _bloodGlucoseDataList.Select((t, i) => new BarEntry(i, t.Avg)).Cast<Entry>().ToList();

            var glucoseDataSet = new LineDataSet(glucoseEntries, "Glucoza")
            {
                ValueTextSize = 14f,
                ValueTextColor = Color.White,
                LineWidth = 3f,
                AxisDependency = YAxis.AxisDependency.Left
            };
            glucoseDataSet.SetMode(LineDataSet.Mode.HorizontalBezier);
            glucoseDataSet.SetColors(Resources.GetColor(Resource.Color.accent, Theme));
            glucoseDataSet.SetCircleColor(Resources.GetColor(Resource.Color.accent, Theme));
            var barData = new LineData(glucoseDataSet);
            _lineChart.Data = barData;

            _lineChart.SetOnChartValueSelectedListener(this);
            _lineChart.SetDrawBorders(false);
            _lineChart.SetDrawGridBackground(false);
            _lineChart.SetNoDataText("Nu exista date");
            _lineChart.Description.Enabled = false;
            _lineChart.AxisLeft.SetDrawGridLines(false);
            _lineChart.AxisRight.SetDrawGridLines(false);
            _lineChart.XAxis.SetDrawGridLines(false);

            _lineChart.XAxis.Enabled = false;
            _lineChart.AxisLeft.Enabled = false;
            _lineChart.AxisRight.Enabled = false;

            _lineChart.AnimateXY(3000, 3000); // animate horizontal and vertical 3000 milliseconds
            _lineChart.SetPinchZoom(false);
            SetLegend();
            _lineChart.Invalidate();
        }

        private void CreateBloodPresureChart()
        {
            if (_bloodPressureDataList.Count == 0)
            {
                _lineChart.Data.ClearValues();
                _lineChart.Clear();
                _lineChart.Invalidate();

                return;
            }
            var systolicEntries = new List<Entry>();
            var diastolicEntries = new List<Entry>();

            for (var i = 0; i < _bloodPressureDataList.Count; i++)
            {
                systolicEntries.Add(new BarEntry(i, _bloodPressureDataList[i].Systolic));
                diastolicEntries.Add(new BarEntry(i, _bloodPressureDataList[i].Diastolic));
            }

            var systolicDataSet = new LineDataSet(systolicEntries, "Sistola")
            {
                ValueTextSize = 14f,
                ValueTextColor = Color.White,
                LineWidth = 3f,
                AxisDependency = YAxis.AxisDependency.Left
            };
            systolicDataSet.SetMode(LineDataSet.Mode.HorizontalBezier);
            systolicDataSet.SetColors(Resources.GetColor(Resource.Color.accent, Theme));
            systolicDataSet.SetCircleColor(Resources.GetColor(Resource.Color.accent, Theme));
            var diastolicDataSet = new LineDataSet(diastolicEntries, "Diastola")
            {
                ValueTextSize = 14f,
                ValueTextColor = Color.White,
                LineWidth = 3f,
                AxisDependency = YAxis.AxisDependency.Left
            };

            diastolicDataSet.SetMode(LineDataSet.Mode.HorizontalBezier);
            diastolicDataSet.SetColors(Color.White);
            diastolicDataSet.SetCircleColor(Color.White);
            var barData = new LineData(systolicDataSet, diastolicDataSet);
            _lineChart.Data = barData;

            _lineChart.SetOnChartValueSelectedListener(this);
            _lineChart.SetDrawBorders(false);
            _lineChart.SetDrawGridBackground(false);
            _lineChart.SetNoDataText("Nu exista date");
            _lineChart.Description.Enabled = false;
            _lineChart.AxisLeft.SetDrawGridLines(false);
            _lineChart.AxisRight.SetDrawGridLines(false);
            _lineChart.XAxis.SetDrawGridLines(false);

            _lineChart.XAxis.Enabled = false;
            _lineChart.AxisLeft.Enabled = false;
            _lineChart.AxisRight.Enabled = false;

            _lineChart.AnimateXY(1000, 1000); // animate horizontal and vertical 3000 milliseconds
            _lineChart.SetPinchZoom(false);
            SetLegend();
            _lineChart.Invalidate();
        }

        private void SetLegend()
        {
            Legend l = _lineChart.Legend;
            l.FormSize = 10f; // set the size of the legend forms/shapes
            l.Form = Legend.LegendForm.Circle; // set what type of form/shape should be used
            l.VerticalAlignment = Legend.LegendVerticalAlignment.Bottom;
            l.HorizontalAlignment = Legend.LegendHorizontalAlignment.Center;
            l.TextSize = 12f;
            l.TextColor = Color.White;
            l.XEntrySpace = 5f; // set the space between the legend entries on the x-axis
            l.YEntrySpace = 5f; // set the space between the legend entries on the y-axis
        }

        private void LoadDataInScrollLayouts(DateTime date, bool pressure = true)
        {
            Drawable bloodPressureIconDrawable = Resources.GetDrawable(Resource.Drawable.heart, Theme);
            Drawable pulseRateIconDrawable = Resources.GetDrawable(Resource.Drawable.heart_pulse, Theme);
            Drawable glucoseIconDrawable = Resources.GetDrawable(Resource.Drawable.water, Theme);
            verticalScrollLinearLayout.RemoveAllViewsInLayout();
            horizontalScrollLinearLayout.RemoveAllViewsInLayout();
            horizontalScrollLinearLayout.Invalidate();
            verticalScrollLinearLayout.Invalidate();
            //for (int i = 0; i < horizontalScrollLinearLayout.ChildCount; i++)
            //{
            //    horizontalScrollLinearLayout.RemoveViewAt(i);
            //}

            var verticalScrollLayoutParams = new LinearLayoutCompat.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent)
            {
                MarginStart = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 1, Resources.DisplayMetrics),
                MarginEnd = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 1, Resources.DisplayMetrics),
                BottomMargin = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 20, Resources.DisplayMetrics)
            };

            var horizontalScrollLayoutParams = new LinearLayoutCompat.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent)
            {
                Width = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 220, Resources.DisplayMetrics),
                MarginStart = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 1, Resources.DisplayMetrics),
                MarginEnd = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 5, Resources.DisplayMetrics)
            };
            if (pressure)
            {

                try
                {
                    Task.Run(async () =>
                    {
                        string result = await WebServices.WebServices.Post($"{Constants.PublicServerAddress}/api/getDayRec", new JSONObject().Put("imei", _imei).Put("dataType", "bloodPressure").Put("date", date.ToString("yyyy-MM-dd")), Utils.GetDefaults("Token"));

                        if (!string.IsNullOrEmpty(result))
                        {
                            try
                            {
                                var array = new JSONArray(result);
                                for (var i = 0; i < array.Length(); i++)
                                {
                                    var obj = new JSONObject(array.Get(i).ToString());
                                    var time = Convert.ToDateTime(obj.GetString("date"));
                                    RunOnUiThread(() =>
                                    {
                                        verticalScrollLinearLayout.AddView(CreateCard(bloodPressureIconDrawable, $"{obj.GetString("systolic")}/{obj.GetString("diastolic")}", "mmHg", time.ToShortTimeString(), verticalScrollLayoutParams));
                                        horizontalScrollLinearLayout.AddView(CreateCard(pulseRateIconDrawable, $"{obj.GetString("pulseRate")}", "bpm", time.ToShortTimeString(), horizontalScrollLayoutParams));
                                    });


                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error("result error", ex.Message);
                            }
                        }
                    });

                }
                catch (Exception ex)
                {
                    Log.Error("Task error", ex.Message);
                }
                //var size = new Random().Next(5, 20);
                //for (int i = 0; i < size; i++)
                //{
                //    verticalScrollLinearLayout.AddView(CreateCard(bloodPressureIconDrawable, $"{new Random().Next(100, 180)}/{new Random().Next(20, 80)}", "mmHg", $"{new Random().Next(10, 24)}:{new Random().Next(10, 59)}", verticalScrollLayoutParams));
                //    horizontalScrollLinearLayout.AddView(CreateCard(pulseRateIconDrawable, $"{new Random().Next(45, 140)}", "bpm", $"{new Random().Next(10, 24)}:{new Random().Next(10, 59)}", horizontalScrollLayoutParams));
                //}
            }
            else
            {
                horizontalScrollLinearLayout.Visibility = ViewStates.Gone;
                //var size = new Random().Next(5, 20);
                //for (var i = 0; i < size; i++)
                //{
                //    verticalScrollLinearLayout.AddView(CreateCard(glucoseIconDrawable, $"{new Random().Next(100, 180)}", "mg/dL", $"{new Random().Next(10, 24)}:{new Random().Next(10, 59)}", verticalScrollLayoutParams));
                //}
                try
                {
                    Task.Run(async () =>
                    {
                        string result = await WebServices.WebServices.Post($"{Constants.PublicServerAddress}/api/getDayRec", new JSONObject().Put("imei", _imei).Put("dataType", "bloodGlucose").Put("date", date.ToString("yyyy-MM-dd")), Utils.GetDefaults("Token"));

                        if (!string.IsNullOrEmpty(result))
                        {
                            try
                            {
                                var array = new JSONArray(result);
                                for (var i = 0; i < array.Length(); i++)
                                {
                                    var obj = new JSONObject(array.Get(i).ToString());
                                    var time = Convert.ToDateTime(obj.GetString("date"));
                                    RunOnUiThread(() =>
                                    {
                                        verticalScrollLinearLayout.AddView(CreateCard(glucoseIconDrawable, $"{obj.GetInt("bloodGlucose")}", "mg/dL", time.ToShortTimeString(), verticalScrollLayoutParams));
                                    });


                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error("result error", ex.Message);
                            }
                        }
                    });

                }
                catch (Exception ex)
                {
                    Log.Error("Task error", ex.Message);
                }
            }
        }
        private void LoadDaySelectorButtons(int type = 1)
        {
            layoutButtons = FindViewById<LinearLayout>(Resource.Id.layout_buttons);
            layoutButtons.RemoveAllViews();
            var layoutButtonParams = new LinearLayoutCompat.LayoutParams(
                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 40, Resources.DisplayMetrics),
                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 40, Resources.DisplayMetrics))
            {
                Width = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 40, Resources.DisplayMetrics),
                Height = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 40, Resources.DisplayMetrics),
                MarginEnd = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 5, Resources.DisplayMetrics)
            };
            int dayIndex = ReturnDayIndex();
            DateTime baseDate = DateTime.Today;

//            var today = baseDate;
//            var yesterday = baseDate.AddDays(-1);
            DateTime thisWeekStart = baseDate.AddDays(-dayIndex);
//            var thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);
//            var lastWeekStart = thisWeekStart.AddDays(-7);
//            var lastWeekEnd = thisWeekStart.AddSeconds(-1);
            DateTime thisMonthStart = baseDate.AddDays(1 - baseDate.Day);
//            var thisMonthEnd = thisMonthStart.AddMonths(1).AddSeconds(-1);
//            var lastMonthStart = thisMonthStart.AddMonths(-1);
//            var lastMonthEnd = thisMonthStart.AddSeconds(-1);

            switch (type)
            {
                case 1:
                    GetDataForChart(thisWeekStart, _dataType == "BloodPressure");
                    break;
                case 2:
                    GetDataForChart(thisMonthStart, _dataType == "BloodPressure");
                    break;
                case 3:
                    GetDataForChart(new DateTime(DateTime.Now.Year, 1,1), _dataType == "BloodPressure");
                    break;
                default:
                    return;
            }
           
            if (type != 3)
            {
                string[] days = { "L", "M", "M", "J", "V", "S", "D" };
                int dayOfMonth = DateTime.Now.Day;
                int numberOfDaysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                int size = type == 1 && type != 3 ? days.Length : numberOfDaysInMonth;

                var activeButton = new AppCompatButton(this);
                for (var i = 0; i < size; i++)
                {
                    bool isActive = type == 1 && type != 3 ? i == dayIndex : i+1 == dayOfMonth;
                    AppCompatButton btn = DaySelectorButton(isActive);
                    btn.Id = i + 1;
                    btn.Text = type == 1 && type != 3 ? i < days.Length ? days[i] : i.ToString() : (i + 1).ToString();
                    btn.LayoutParameters = layoutButtonParams;
                    if(type == 1 && type != 3 && i > dayIndex)
                    {
                        btn.Enabled = false;
                        btn.SetTextColor(Resources.GetColor(Resource.Color.colorSecondary, Theme));
                    }
                    else 
                    if (i+1 > dayOfMonth)
                    {
                        btn.Enabled = false;
                        btn.SetTextColor(Resources.GetColor(Resource.Color.colorSecondary, Theme));
                    }
                    if (isActive) activeButton = btn;
                    layoutButtons.AddView(btn);
                }

                if (type == 2)
                {

                    _scrollViewButtons.PostDelayed(delegate { _scrollViewButtons.SmoothScrollTo(activeButton.Left - 5, 0); }, 30);

                }
            }
            else
            {
                Drawable img = Resources.GetDrawable(Resource.Drawable.calendar_date, Theme);
                img.SetBounds(0, 0, 50, 50);
                var btn = new AppCompatButton(this)
                {
                    Id = 1,
                    Text = "Data"
                    //Background = buttonBackgroundA,
                    //LayoutParameters = layoutButtonParams
                };
                btn.SetTextColor(Color.White);
                btn.Background = buttonBackgroundDatePicker;
                btn.SetAllCaps(false);
                btn.SetCompoundDrawables(img, null, null, null);
                btn.SetPadding((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 8, Resources.DisplayMetrics),
                    (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 8, Resources.DisplayMetrics),
                    (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 8, Resources.DisplayMetrics),
                    (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 8, Resources.DisplayMetrics));

                btn.Click += (sender, args) =>
                {
                    var frag = SharingDatePickerFragment.NewInstance(delegate (DateTime time)
                    {
                        //_tbDate.Text = time.ToShortDateString();
                        _tvDate.Text = time.ToShortDateString();
                        switch (_dataType)
                        {
                            case "BloodPressure":
                                verticalScrollLinearLayout.RemoveAllViewsInLayout();
                                horizontalScrollLinearLayout.RemoveAllViewsInLayout();
                                LoadDataInScrollLayouts(time);
                                break;
                            case "BloodGlucose":
                                verticalScrollLinearLayout.RemoveAllViewsInLayout();
                                LoadDataInScrollLayouts(time,false);
                                break;
                        }
                    });
                    frag.Show(SupportFragmentManager, SharingDatePickerFragment.TAG);
                };
                layoutButtons.AddView(btn);
            }

        }
        private AppCompatButton DaySelectorButton(bool active = false)
        {
            var btn = new AppCompatButton(this);
            btn.SetTextSize(ComplexUnitType.Sp, 12f);
            btn.SetTextColor(Color.White);
            btn.Background = active ? buttonBackgroundA : buttonBackground;
            btn.Click += DaySelectorButtonOnClick;
            return btn;
        }

        private static int ReturnDayIndex()
        {
            switch (DateTime.Now.DayOfWeek)
            {
                case System.DayOfWeek.Sunday:
                    return (int)DayOfWeek.Sunday;
                case System.DayOfWeek.Monday:
                    return (int)DayOfWeek.Monday;
                case System.DayOfWeek.Tuesday:
                    return (int)DayOfWeek.Tuesday;
                case System.DayOfWeek.Wednesday:
                    return (int)DayOfWeek.Wednesday;
                case System.DayOfWeek.Thursday:
                    return (int)DayOfWeek.Thursday;
                case System.DayOfWeek.Friday:
                    return (int)DayOfWeek.Friday;
                case System.DayOfWeek.Saturday:
                    return (int)DayOfWeek.Saturday;
                default:
                    return 0;
            }
        }
        private void DaySelectorButtonOnClick(object sender, EventArgs e)
        {
            var btn = (AppCompatButton)sender;

            for (var i = 0; i < layoutButtons.ChildCount; i++)
            {
                if ((layoutButtons.GetChildAt(i) as AppCompatButton)?.Background == buttonBackgroundA)
                    ((AppCompatButton)layoutButtons.GetChildAt(i)).Background = buttonBackground;
            }

            if (btn.Background == buttonBackground)
                btn.Background = buttonBackgroundA;


            DateTime selectionDate = DateTime.Now;
            if(bottomNavigation.SelectedItemId == Resource.Id.week_tab)
            {
                int dayIndex = ReturnDayIndex();

                DateTime baseDate = DateTime.Today;

                DateTime today = baseDate;
                DateTime yesterday = baseDate.AddDays(-1);
                DateTime thisWeekStart = baseDate.AddDays(-dayIndex);

                DateTime customWeekStart = baseDate.AddDays(-(dayIndex - (btn.Id - 1)));

                DateTime thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);
                DateTime lastWeekStart = thisWeekStart.AddDays(-7);
                DateTime lastWeekEnd = thisWeekStart.AddSeconds(-1);
                DateTime thisMonthStart = baseDate.AddDays(1 - baseDate.Day);
                DateTime thisMonthEnd = thisMonthStart.AddMonths(1).AddSeconds(-1);
                DateTime lastMonthStart = thisMonthStart.AddMonths(-1);
                DateTime lastMonthEnd = thisMonthStart.AddSeconds(-1);
                _tvDate.Text = customWeekStart.ToString("dd.MM.yyyy");
                selectionDate = customWeekStart;
            }
            else if(bottomNavigation.SelectedItemId == Resource.Id.month_tab)
            {
                DateTime baseDate = DateTime.Today;

                var month = baseDate.Month.ToString();
                if (baseDate.Month < 10) month = $"0{month}";

                var day = btn.Id.ToString();
                if (btn.Id < 10) day = $"0{day}";

                string date = $"{baseDate.Year}-{month}-{day}";
                _tvDate.Text = $"{day}.{month}.{baseDate.Year}";
                selectionDate = Convert.ToDateTime(date);
            }
            switch (_dataType)
            {
                case "BloodPressure":
                    verticalScrollLinearLayout.RemoveAllViewsInLayout();
                    horizontalScrollLinearLayout.RemoveAllViewsInLayout();
                    LoadDataInScrollLayouts(selectionDate);
                    break;
                case "BloodGlucose":
                    verticalScrollLinearLayout.RemoveAllViewsInLayout();
                    LoadDataInScrollLayouts(selectionDate,false);
                    break;
            }

        }
        private CardView CreateCard(Drawable image, string value, string measureType, string hour, LinearLayoutCompat.LayoutParams layoutParams)
        {
            var cardView = new CardView(this) { LayoutParameters = layoutParams };
            cardView.SetCardBackgroundColor(Resources.GetColor(Resource.Color.colorPrimaryDark, Theme));

            var rlContent = new RelativeLayout(this);
            var tvHour = new TextView(this) { Id = 4 };
            var tvBloodPressure = new TextView(this) { Id = 2 };
            var tvMmHg = new TextView(this) { Id = 3 };
            var imIcon = new ImageView(this) { Id = 1 };

            var rlParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.WrapContent);
            var tvHourParams = new RelativeLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent)
            {
                TopMargin = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Resources.DisplayMetrics)
            };
            tvHourParams.AddRule(LayoutRules.AlignParentRight);

            var tvBloodPressureParams = new RelativeLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent)
            {
                MarginStart = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Resources.DisplayMetrics)
            };
            tvBloodPressureParams.AddRule(LayoutRules.AlignParentTop);
            tvBloodPressureParams.AddRule(LayoutRules.RightOf, imIcon.Id);

            var tvMmHgParams = new RelativeLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent)
            {
                MarginStart = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 3, Resources.DisplayMetrics),
                TopMargin = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Resources.DisplayMetrics)
            };
            tvMmHgParams.AddRule(LayoutRules.RightOf, tvBloodPressure.Id);

            var tvIconParams = new RelativeLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent)
            {
                Width = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 30, Resources.DisplayMetrics),
                Height = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 30, Resources.DisplayMetrics),
                TopMargin = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 3, Resources.DisplayMetrics)
            };

            rlContent.SetPadding((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 16, Resources.DisplayMetrics),
                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 16, Resources.DisplayMetrics),
                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 16, Resources.DisplayMetrics),
                (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 16, Resources.DisplayMetrics));
            rlContent.LayoutParameters = rlParams;

            tvHour.LayoutParameters = tvHourParams;
            tvHour.Text = hour;
            tvHour.SetTextSize(ComplexUnitType.Sp, 16);
            tvHour.SetTextColor(Color.White);

            tvBloodPressure.LayoutParameters = tvBloodPressureParams;
            tvBloodPressure.Text = value;
            tvBloodPressure.SetTextSize(ComplexUnitType.Sp, 28);
            tvBloodPressure.SetTextColor(Resources.GetColor(Resource.Color.accent,Theme));
            tvBloodPressure.SetTypeface(tvBloodPressure.Typeface, TypefaceStyle.Bold);

            tvMmHg.LayoutParameters = tvMmHgParams;
            tvMmHg.Text = measureType;
            tvMmHg.SetTextSize(ComplexUnitType.Sp, 18);
            tvMmHg.SetTextColor(Resources.GetColor(Resource.Color.colorSecondary, Theme));

            imIcon.LayoutParameters = tvIconParams;

            imIcon.SetImageDrawable(image);
            imIcon.SetColorFilter(Color.White);

            rlContent.AddView(imIcon);
            rlContent.AddView(tvBloodPressure);
            rlContent.AddView(tvMmHg);
            rlContent.AddView(tvHour);
            cardView.AddView(rlContent);
            return cardView;
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_pressure_and_glucose);
            InitUi();
        }
        public void OnNothingSelected()
        {

        }

        public void OnValueSelected(Entry e, Highlight h)
        {
            //Toast.MakeText(this, e.GetY().ToString(), ToastLength.Short).Show();
        }
    }
}