﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Tokkepedia.Shared.Models;
using Tokkepedia.ViewHolders;
using Tokket.Tokkepedia;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Services;
using Android.Graphics;
using Android.Text;
using Color = Android.Graphics.Color;

namespace Tokkepedia.Adapters
{
    public class BadgesAdapter : RecyclerView.Adapter
    {

        public event EventHandler<int> ItemClick;
        ObservableCollection<BadgeOwned> BadgeCollection;
        public override int ItemCount => BadgeCollection.Count();
        View itemView;

        #region Constructor
        public BadgesAdapter(ObservableCollection<BadgeOwned> _BadgeCollection)
        {
            BadgeCollection = _BadgeCollection;
        }
        #endregion

        #region Override Events/Methods/Delegates
        BadgeViewHolder vh;
        int selectedPosition = -1;
        View holderView;
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            vh = holder as BadgeViewHolder;

            Glide.With(itemView).Load(BadgeCollection[position].Image).Apply(RequestOptions.PlaceholderOf(Resource.Animation.loader_animation).FitCenter()).Into(vh.ImgBadge);

            vh.ImgBadge.Click += (sender, e) =>
            {
                if (selectedPosition != -1)
                {
                    holderView.SetBackgroundColor(Color.ParseColor("#dddddd"));
                }
                holder.ItemView.SetBackgroundColor(Color.LightBlue);

                BadgesActivity.Instance.SelectCommand.Tag = position;
                selectedPosition = position;
                holderView = holder.ItemView;
            };
        }
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.badgesrow, parent, false);

            vh = new BadgeViewHolder(itemView, OnClick);
            return vh;
        }
        public override int GetItemViewType(int position)
        {
            return position;

            //This was added due to the reason that when clicked, there will be 2 rows that will highlight
            //For Reference, this one have similar issue with the link below
            //https://stackoverflow.com/questions/32065267/recyclerview-changing-items-during-scroll
        }
        #endregion
        void OnClick(int position)
        {
            if (ItemClick != null)
                ItemClick(this, position);
        }
    }
}