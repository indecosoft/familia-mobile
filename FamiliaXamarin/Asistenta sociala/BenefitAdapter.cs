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
using Java.Lang;
using Java.Util;
using Object = Java.Lang.Object;

namespace FamiliaXamarin
{
    class BenefitAdapter : ArrayAdapter<BenefitSpinnerState>
    {
        private Context mContext;
        private List<BenefitSpinnerState> listState;
        private BenefitAdapter myAdapter;
        private bool isFromView = false;
        public BenefitAdapter(Context context, int resource, List<BenefitSpinnerState> objects) : base(context, resource, objects)
        {

            this.mContext = context;
            this.listState = objects;
            this.myAdapter = this;
        }

        public override View GetDropDownView(int position, View convertView,
                                ViewGroup parent)
        {
            return GetCustomView(position, convertView, parent);
        }

        
    public override View GetView(int position, View convertView, ViewGroup parent)
        {
            return GetCustomView(position, convertView, parent);
        }

        public View GetCustomView( int position, View convertView,
                                  ViewGroup parent)
        {

            ViewHolder holder;
            if (convertView == null)
            {
                LayoutInflater layoutInflator = LayoutInflater.From(mContext);
                convertView = layoutInflator.Inflate(Resource.Layout.item_benefit, null);
                holder = new ViewHolder();
                holder.mTextView = (TextView)convertView
                        .FindViewById(Resource.Id.text);
                holder.mCheckBox = (CheckBox)convertView
                        .FindViewById(Resource.Id.checkbox);
                convertView.Tag = holder;
            }
            else
            {
                holder = (ViewHolder)convertView.Tag;
            }

            holder.mTextView.Text = listState[position].GetTitle();

            // To check weather checked event fire from getview() or user input
            isFromView = true;
            holder.mCheckBox.Checked = listState[position].IsSelected();
            isFromView = false;

            if ((position == 0))
            {
                holder.mCheckBox.Visibility = ViewStates.Invisible;
            }
            else
            {
                holder.mCheckBox.Visibility = ViewStates.Visible;
            }
            holder.mCheckBox.Tag = position;
            //holder.mCheckBox.SetOnCheckedChangeListener(delegate(CompoundButton buttonView, bool isChecked){});
            holder.mCheckBox.CheckedChange += delegate(object sender, CompoundButton.CheckedChangeEventArgs args)
            {
                int getPosition = (int)holder.mCheckBox.Tag;

                if (!isFromView)
                {
                    listState[position].SetSelected(args.IsChecked);
                }
            }; 
        return convertView;
    }
//        public void OnCheckedChanged(CompoundButton buttonView, bool isChecked)
//        {
//            int getPosition = (int)buttonView.Tag;
//
//            if (!isFromView)
//            {
//                listState[position].SetSelected(args.IsChecked);
//            }
//        }
        internal class ViewHolder : Object
    {
        public TextView mTextView;
        public CheckBox mCheckBox;
    }

        
    }
}