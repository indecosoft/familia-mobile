using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Android.Content;
using Android.Views;
using Android.Widget;
using Object = Java.Lang.Object;

namespace FamiliaXamarin
{
    class BenefitAdapter : ArrayAdapter<BenefitSpinnerState>
    {
        readonly Context mContext;
        readonly List<BenefitSpinnerState> listState;
        readonly BenefitAdapter myAdapter;
        bool isFromView;
        public BenefitAdapter(Context context, int resource, List<BenefitSpinnerState> objects) : base(context, resource, objects)
        {

            mContext = context;
            listState = objects;
            myAdapter = this;
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

        public View GetCustomView(int position, View convertView,
                                  ViewGroup parent)
        {
            if (parent == null)
            {
                throw new System.ArgumentNullException(nameof(parent));
            }

            Contract.Ensures(Contract.Result<View>() != null);

            ViewHolder holder;
            if (convertView == null)
            {
                LayoutInflater layoutInflator = LayoutInflater.From(mContext);
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

            holder.mTextView.Text = listState[position].Title;

            // To check weather checked event fire from getview() or user input
            isFromView = true;
            holder.mCheckBox.Checked = listState[position].IsSelected;
            isFromView = false;

            holder.mCheckBox.Visibility = position == 0 ? ViewStates.Invisible : ViewStates.Visible;
            holder.mCheckBox.Tag = position;
            holder.mCheckBox.CheckedChange += delegate (object sender, CompoundButton.CheckedChangeEventArgs args)
            {
                Contract.Requires(sender != null);
                int getPosition = (int)holder.mCheckBox.Tag;

                if (!isFromView)
                {
                    listState[position].IsSelected = args.IsChecked;
                }
            };
            return convertView;
        }
        internal class ViewHolder : Object
        {
            public TextView mTextView;
            public CheckBox mCheckBox;
        }


    }
}