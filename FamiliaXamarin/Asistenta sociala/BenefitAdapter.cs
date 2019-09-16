﻿using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Familia;
using Android.Content;
using Android.Views;
using Android.Widget;
using Object = Java.Lang.Object;

namespace FamiliaXamarin.Asistenta_sociala
{
    class BenefitAdapter : ArrayAdapter<BenefitSpinnerState>
    {
        private readonly Context _mContext;
        readonly List<BenefitSpinnerState> _listState;
//        readonly BenefitAdapter _myAdapter;
        bool _isFromView;
        public BenefitAdapter(Context context, int resource, List<BenefitSpinnerState> objects) : base(context, resource, objects)
        {

            _mContext = context;
            _listState = objects;
//            _myAdapter = this;
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

        private View GetCustomView(int position, View convertView,
                                  ViewGroup parent)
        {
            if (parent == null)
            {
                throw new System.ArgumentNullException(nameof(parent));
            }

//            Contract.Ensures(Contract.Result<View>() != null);

            ViewHolder holder;
            if (convertView == null)
            {
                LayoutInflater layoutInflator = LayoutInflater.From(_mContext);
                convertView = layoutInflator.Inflate(Resource.Layout.item_benefit, null);
                holder = new ViewHolder
                {
                    mTextView = (TextView)convertView
                        .FindViewById(Resource.Id.text),
                    mCheckBox = (CheckBox)convertView
                        .FindViewById(Resource.Id.checkbox)
                };
                convertView.Tag = holder;
            }
            else
            {
                holder = (ViewHolder)convertView.Tag;
            }

            holder.mTextView.Text = _listState[position].Title;

            // To check weather checked event fire from getview() or user input
            _isFromView = true;
            holder.mCheckBox.Checked = _listState[position].IsSelected;
            _isFromView = false;

            holder.mCheckBox.Visibility = position == 0 ? ViewStates.Invisible : ViewStates.Visible;
            holder.mCheckBox.Tag = position;
            holder.mCheckBox.CheckedChange += delegate (object sender, CompoundButton.CheckedChangeEventArgs args)
            {
//                Contract.Requires(sender != null);
//                int getPosition = (int)holder.mCheckBox.Tag;

                if (!_isFromView)
                {
                    _listState[position].IsSelected = args.IsChecked;
                }
            };
            return convertView;
        }
        class ViewHolder : Object
        {
            public TextView mTextView;
            public CheckBox mCheckBox;
        }


    }
}