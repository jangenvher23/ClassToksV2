﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Request;
using Tokkepedia.Shared.Models;
using Tokkepedia.ViewHolders;
using Tokkepedia.Shared.Extensions;
using Tokkepedia.Shared.Helpers;
using Android.Webkit;
using Tokkepedia.Shared.Services;
using AndroidX.RecyclerView.Widget;
using Tokket.Tokkepedia;
using Tokkepedia.Helpers;
using Android.Text;
using Color = Android.Graphics.Color;
using Tokket.Tokkepedia.Tools;

namespace Tokkepedia.Adapters
{
    public class ClassTokDataAdapter : RecyclerView.Adapter, View.IOnTouchListener, View.IOnLongClickListener
    {
        #region Members/Properties
        // Event handler for item clicks:
        public event EventHandler<int> ItemClick;
        public List<ClassTokModel> items;
        List<string> Colors = new List<string>() {
               "#d32f2f", "#C2185B", "#7B1FA2", "#512DA8",
               "#303F9F", "#1976D2", "#0288D1", "#0097A7",
               "#00796B", "#388E3C", "#689F38", "#AFB42B",
               "#FBC02D", "#FFA000", "#F57C00", "#E64A19"
               };
        List<string> randomcolors = new List<string>();
        public override int ItemCount => items.Count;
        View itemView;
        #endregion

        #region Constructor
        public ClassTokDataAdapter(List<ClassTokModel> _items, List<Tokmoji> _listTokMoji) //If caller is Activity or Fragment
        {
            items = _items;
            SpannableHelper.ListTokMoji = _listTokMoji;
        }
        #endregion

        #region Abstract Methods
        public void UpdateItems(List<ClassTokModel> listUpdate, int position)
        {
            items.AddRange(listUpdate);
            NotifyItemRangeChanged(position, listUpdate.Count);
        }

        public void AddItem(ClassTokModel item)
        {
            items.Add(item);
            NotifyDataSetChanged();
        }

        public void RemoveItem(ClassTokModel item)
        {
            items.Remove(item);
            NotifyDataSetChanged();
        }
        #endregion

        #region Override Events/Methods/Delegates
        TokViewHolder vh;
        AssetManager assetManager = Application.Context.Assets;
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            vh = holder as TokViewHolder;
            Resources res = Application.Context.Resources;

            bool isCountry = true;
            string flagImg = "";
            Stream sr = null;

            try
            {
                if (string.IsNullOrEmpty(items[position].UserCountry))
                {
                    isCountry = false;
                    flagImg = "https://tokketcontent.blob.core.windows.net/pointssymbol48/set1-black%20(0).jpg";
                }
                else if (items[position].UserCountry.ToLower() == "us")
                {
                    if (!string.IsNullOrEmpty(items[position].UserState))
                    {
                        isCountry = false;
                        flagImg = CountryTool.GetCountryFlagJPG1x1(items[position].UserState);
                    }
                    else
                    {
                        sr = assetManager.Open("Flags/" + items[position].UserCountry + ".jpg");
                    }
                }
                else
                {
                    sr = assetManager.Open("Flags/" + items[position].UserCountry + ".jpg");
                }
            }
            catch (Exception)
            {

                isCountry = false;
                flagImg = "https://tokketcontent.blob.core.windows.net/pointssymbol48/set1-black%20(0).jpg";
            }


            Bitmap bitmapFlag = BitmapFactory.DecodeStream(sr);
            var ssbName = new SpannableStringBuilder(items[position].PrimaryFieldText);
            var spannableResText = SpannableHelper.AddStickersSpannable(MainActivity.Instance, ssbName);

            if (!string.IsNullOrEmpty(items[position].Image))
            {
                vh.ItemView.Tag = vh.TokImgPrimaryFieldText;
                string tokimg = tokimg = items[position].Image;

                if (URLUtil.IsValidUrl(tokimg))
                {
                    if (items[position].Image.EndsWith(".png") || items[position].Image.EndsWith(".jpg"))
                    {
                        tokimg = items[position].Image;
                    }
                    else
                    {
                        tokimg = items[position].Image + ".jpg";
                    }

                    Glide.With(itemView).Load(tokimg).Apply(RequestOptions.PlaceholderOf(Resource.Animation.loader_animation).Error(Resource.Drawable.no_image).FitCenter()).Into(vh.TokImgMain);
                }
                else
                {
                    tokimg = tokimg.Replace("data:image/jpeg;base64,", "");
                    byte[] imageDetailBytes = Convert.FromBase64String(tokimg);
                    vh.TokImgMain.SetImageBitmap((BitmapFactory.DecodeByteArray(imageDetailBytes, 0, imageDetailBytes.Length)));
                }

                Glide.With(itemView).Load(items[position].UserPhoto).Apply(RequestOptions.CircleCropTransform().Placeholder(Resource.Drawable.Man3).CircleCrop()).Into(vh.TokImgUserPhoto);
                vh.TokImgUserPhoto.SetOnTouchListener(this);

                if (!string.IsNullOrEmpty(Settings.GetTokketUser().AccountType))
                {
                    if (Settings.GetTokketUser().AccountType == "group")
                    {
                        vh.TokUserTitleImg.Text = items[position].SubaccountName; //Settings.GetTokketUser().SubaccountName;
                    }
                    else
                    {
                        if (Settings.GetTokketUser().TitleEnabled == true && Settings.GetTokketUser().TitleId != null)
                        {
                            vh.TokUserTitleImg.Text = items[position].TitleId; //Settings.GetTokketUser().TitleId;
                        }
                    }
                }
                else
                {
                    if (Settings.GetTokketUser().TitleEnabled == true && Settings.GetTokketUser().TitleId != null)
                    {
                        vh.TokUserTitleImg.Text = items[position].TitleId; //Settings.GetTokketUser().TitleId;
                    }
                }

                if (string.IsNullOrEmpty(vh.TokUserTitleImg.Text))
                {
                    vh.TokUserTitleImg.Visibility = ViewStates.Gone;
                }

                if (isCountry)
                {
                    vh.TokImgUserFlag.SetImageBitmap(bitmapFlag);
                }
                else
                {
                    Glide.With(itemView).Load(flagImg).Into(vh.TokImgUserFlag);
                }

                Glide.With(itemView).Load(items[position].StickerImage).Into(vh.TileStickerImg);
                if (string.IsNullOrEmpty(items[position].StickerImage))
                {
                    vh.TileStickerImg.Visibility = ViewStates.Gone;
                }
                
                vh.ImgUserDisplayName.Text = items[position].UserDisplayName;
                vh.ImgUserDisplayName.ContentDescription = position.ToString();
                vh.ImgUserDisplayName.Click -= onImageUsernameClick;
                vh.ImgUserDisplayName.Click += onImageUsernameClick;

                //vh.TokImgPrimaryFieldText.Text = items[position].PrimaryFieldText;
                vh.TokImgPrimaryFieldText.SetText(spannableResText, TextView.BufferType.Spannable);

                vh.TokImgCategory.Text = items[position].Category;
                vh.TokImgTokType.Text = items[position].TokType;

                vh.gridTokImage.SetBackgroundResource(Resource.Drawable.tileview_layout);
                vh.gridTokImage.Tag = position;

                int ndx = position % Colors.Count;
                if (ndx == 0 || randomcolors.Count == 0) randomcolors = Colors.Shuffle().ToList();
                GradientDrawable tokimagedrawable = (GradientDrawable)vh.tokimgdrawable.Background;
                //tokimagedrawable.SetStroke(10, Color.ParseColor(randomcolors[ndx]));
                tokimagedrawable.SetColor(Color.White);

                vh.TokImgMain.ContentDescription = position.ToString();

                vh.TokImgCategory.Tag = 0;
                vh.TokImgCategory.ContentDescription = items[position].CategoryId;
                vh.TokImgCategory.Click += OnTokButtonClick;

                //vh.TokImgTokGroup.Text = items[position].TokGroup;
                //vh.TokImgTokGroup.Tag = 1;
                //vh.TokImgTokGroup.ContentDescription = items[position].TokGroup;
                //vh.TokImgTokGroup.Click += OnTokButtonClick;

                vh.TokImgTokType.Tag = 2;
                vh.TokImgTokType.ContentDescription = items[position].TokTypeId;
                vh.TokImgTokType.Click += OnTokButtonClick;

                vh.gridTokImage.Visibility = ViewStates.Visible;
                vh.gridBackground.Visibility = ViewStates.Gone;
            }
            else
            {
                vh.ItemView.Tag = vh.PrimaryFieldText;
                //Purple Gem
                if (items[position].HasGemReaction)
                {
                    vh.ImgPurpleGem.Visibility = ViewStates.Visible;
                }
                else
                {
                    vh.ImgPurpleGem.Visibility = ViewStates.Gone;
                }

                Glide.With(itemView).Load(items[position].UserPhoto).Apply(RequestOptions.CircleCropTransform().Placeholder(Resource.Drawable.Man3).CircleCrop()).Into(vh.UserPhoto);

                if (!string.IsNullOrEmpty(Settings.GetTokketUser().AccountType))
                {
                    if (Settings.GetTokketUser().AccountType == "group")
                    {
                        vh.TokUserTitle.Text = items[position].SubaccountName; //Settings.GetTokketUser().SubaccountName;
                    }
                    else
                    {
                        if (Settings.GetTokketUser().TitleEnabled == true && Settings.GetTokketUser().TitleId != null)
                        {
                            vh.TokUserTitle.Text = items[position].TitleId; //Settings.GetTokketUser().TitleId;
                        }
                    }
                }
                else
                {
                    if (Settings.GetTokketUser().TitleEnabled == true && Settings.GetTokketUser().TitleId != null)
                    {
                        vh.TokUserTitle.Text = items[position].TitleId; //Settings.GetTokketUser().TitleId;
                    }
                }

                if (string.IsNullOrEmpty(vh.TokUserTitle.Text))
                {
                    vh.TokUserTitle.Visibility = ViewStates.Gone;
                }

                if (isCountry)
                {
                    vh.UserFlag.SetImageBitmap(bitmapFlag);
                }
                else
                {
                    Glide.With(itemView).Load(flagImg).Into(vh.UserFlag);
                }

                Glide.With(itemView).Load(items[position].StickerImage).Into(vh.TileSticker);
                if (string.IsNullOrEmpty(items[position].StickerImage))
                {
                    vh.TileSticker.Visibility = ViewStates.Gone;
                }

                vh.UserDisplayName.Text = items[position].UserDisplayName;
                vh.PrimaryFieldText.Text = items[position].PrimaryFieldText;
                vh.Category.Text = items[position].Category;
                vh.TokType.Text = items[position].TokType;
                if (string.IsNullOrEmpty(items[position].EnglishPrimaryFieldText))
                {
                    vh.EnglishPrimaryFieldText.Visibility = ViewStates.Gone;
                }
                else
                {
                    vh.EnglishPrimaryFieldText.Visibility = ViewStates.Visible;
                }
                vh.EnglishPrimaryFieldText.Text = items[position].EnglishPrimaryFieldText;
                vh.gridBackground.SetBackgroundResource(Resource.Drawable.tileview_layout);
                vh.gridBackground.Tag = position;
                int ndx = position % Colors.Count;
                if (ndx == 0 || randomcolors.Count == 0) randomcolors = Colors.Shuffle().ToList();
                GradientDrawable Tokdrawable = (GradientDrawable)vh.Tokdrawable.Background;
                //Tokdrawable.SetColor(Color.ParseColor(randomcolors[ndx]));

                if (items[position].ColorMainHex == "#FFFFFF" || string.IsNullOrEmpty(items[position].ColorMainHex))
                {
                    Tokdrawable.SetColor(Color.White);
                    Tokdrawable.SetStroke(10, Color.ParseColor(randomcolors[ndx]));
                    setTextColor(Color.Black);
                }
                else
                {
                    Tokdrawable.SetStroke(10, Color.ParseColor(items[position].ColorMainHex));
                    Tokdrawable.SetColor(Color.ParseColor(items[position].ColorMainHex));
                    setTextColor(Color.White);
                }

                vh.UserPhoto.ContentDescription = position.ToString();

                if (items[position].IsDetailBased || (items[position].IsMegaTok == true && items[position].TokGroup.ToLower() == "mega"))
                {
                    vh.lblTokViewMore.Visibility = ViewStates.Visible;
                    if (vh.SecondaryFieldText.Text.Length > 30)
                    {
                        vh.lblTokViewMore.Text = "View More";
                    }
                    else if (items[position].IsDetailBased)
                    {
                        int cnt = 0;

                        if (items[position].Details != null)
                        {
                            cnt = items[position].Details.Where(x => (!string.IsNullOrEmpty(x))).ToList().Count();
                        }

                        vh.lblTokViewMore.Text = "View " + cnt + " Details";
                    }
                    else if (items[position].IsMegaTok == true && items[position].TokGroup.ToLower() == "mega")
                    {
                        if (items[position].Sections != null)
                        {
                            vh.lblTokViewMore.Text = "View " + items[position].Sections.Count() + " Sections";
                        }
                        else
                        {
                            vh.lblTokViewMore.Visibility = ViewStates.Gone;
                        }
                    }
                }
                else
                {
                    vh.lblTokViewMore.Visibility = ViewStates.Gone;
                }

                //vh.UserPhoto.Click -= onImageUsernameClick;
                //vh.UserPhoto.Click += onImageUsernameClick;
                vh.UserPhoto.SetOnTouchListener(this);


                vh.UserDisplayName.ContentDescription = position.ToString();
                vh.UserDisplayName.Click -= onImageUsernameClick;
                vh.UserDisplayName.Click += onImageUsernameClick;

                vh.Category.Tag = 0;
                vh.Category.ContentDescription = items[position].CategoryId;
                vh.Category.Click += OnTokButtonClick;

                //vh.TokGroup.Text = items[position].TokGroup;
                //vh.TokGroup.Tag = 1;
                //vh.TokGroup.ContentDescription = items[position].TokGroup;
                //vh.TokGroup.Click += OnTokButtonClick;

                vh.TokType.Tag = 2;
                vh.TokType.ContentDescription = items[position].TokTypeId;
                vh.TokType.Click += OnTokButtonClick;

                vh.gridBackground.Visibility = ViewStates.Visible;
                vh.gridTokImage.Visibility = ViewStates.Gone;
            }

            vh.ItemView.ContentDescription = position + "";
            vh.ItemView.SetOnLongClickListener(this);
        }

        private void setTextColor(Color color)
        {
            vh.UserDisplayName.SetTextColor(color);
            vh.PrimaryFieldText.SetTextColor(color);
            vh.Category.SetTextColor(color);
            //vh.TokGroup.SetTextColor(color);
            vh.TokType.SetTextColor(color);
            vh.EnglishPrimaryFieldText.SetTextColor(color);
            vh.lblTokViewMore.SetTextColor(color);
        }

        void onImageUsernameClick(object sender, EventArgs e)
        {
            var sendertype = sender.GetType().Name;
            int position = 0;
            if (sendertype == "AppCompatImageView")
            {
                position = Convert.ToInt32((sender as ImageView).ContentDescription);
            }
            else if (sendertype == "AppCompatTextView")
            {
                position = Convert.ToInt32((sender as TextView).ContentDescription);
            }

            //if (Settings.GetUserModel().UserId == items[position].UserId)
            //{
            //    MainActivity.Instance.viewpager.SetCurrentItem(4, true);
            //}
            //else
            //{
            Intent nextActivity = new Intent(MainActivity.Instance, typeof(ProfileUserActivity));
            nextActivity.PutExtra("userid", items[position].UserId);
            MainActivity.Instance.StartActivity(nextActivity);
            //}

        }
        void OnTokButtonClick(object sender, EventArgs e)
        {
            string titlepage = "";
            string filter = "";
            string headerpage = headerpage = (sender as TextView).Text;

            if ((int)(sender as TextView).Tag == (int)Toks.Category)
            {
                Settings.FilterTag = 3;
                titlepage = "Category";
                filter = (sender as TextView).ContentDescription;
            }
            else if ((int)(sender as TextView).Tag == (int)Toks.TokGroup)
            {
                Settings.FilterTag = 6;
                titlepage = "Tok Group";
                filter = (sender as TextView).ContentDescription.ToLower();
            }
            else if ((int)(sender as TextView).Tag == (int)Toks.TokType)
            {
                Settings.FilterTag = 1;
                titlepage = "Tok Type";
                filter = (sender as TextView).ContentDescription;
            }
            Intent nextActivity = new Intent(MainActivity.Instance, typeof(ClassToksActivity));
            nextActivity.PutExtra("titlepage", titlepage);
            nextActivity.PutExtra("filter", filter);
            nextActivity.PutExtra("headerpage", headerpage);
            nextActivity.SetFlags(ActivityFlags.ReorderToFront);
            MainActivity.Instance.StartActivity(nextActivity);
        }
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.classtok_tile_row, parent, false);

            vh = new TokViewHolder(itemView, OnClick);
            return vh;
        }
        #endregion

        #region Custom Events/Delegates
        void OnClick(int position)
        {
            if (ItemClick != null)
                ItemClick(this, position);
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            if (e.Action == MotionEventActions.Down)
            {
                return false;
            }
            else if (e.Action == MotionEventActions.Up)
            {
                //When Image of User Photo is clicked.
                int position = Convert.ToInt32(v.ContentDescription);
                Intent nextActivity = new Intent(MainActivity.Instance, typeof(ProfileUserActivity));
                nextActivity.PutExtra("userid", items[position].UserId);
                MainActivity.Instance.StartActivity(nextActivity);
            }
            return true;
        }

        public bool OnLongClick(View v)
        {
            int position = int.Parse((string)v.ContentDescription);
            //try { position = (int)v.Tag; } catch { position = int.Parse((string)v.Tag); }

            if (!string.IsNullOrEmpty(items[position].GroupId))
            {
                View labelview = (v.Tag as View); //This is use in order to show the menu in this View's location
                Android.Widget.PopupMenu menu = new Android.Widget.PopupMenu(ClassGroupActivity.Instance, labelview);
                // Call inflate directly on the menu:
                menu.Inflate(Resource.Menu.delete_menu);

                // A menu item was clicked:
                menu.MenuItemClick += async (s1, arg1) => {
                    switch (arg1.Item.TitleFormatted.ToString().ToLower())
                    {
                        case "delete":
                            ClassTokModel model = items[position];
                            model.GroupId = "";
                            ClassGroupActivity.Instance.Window.AddFlags(WindowManagerFlags.NotTouchable);
                            ClassGroupActivity.Instance.LinearProgress.Visibility = ViewStates.Visible;

                            var result = await ClassService.Instance.UpdateClassToksAsync(model);

                            ClassGroupActivity.Instance.Window.ClearFlags(WindowManagerFlags.NotTouchable);
                            ClassGroupActivity.Instance.LinearProgress.Visibility = ViewStates.Gone;

                            var objBuilder = new AlertDialog.Builder(ClassGroupActivity.Instance);
                            objBuilder.SetTitle("");
                            objBuilder.SetMessage(result.ResultEnum.ToString());
                            objBuilder.SetCancelable(false);

                            AlertDialog objDialog = objBuilder.Create();
                            objDialog.SetButton((int)(DialogButtonType.Positive), "OK", (s, ev) =>
                            {
                                if (result.ResultEnum == Shared.Helpers.Result.Success)
                                {
                                    items.RemoveAt(position);
                                    NotifyDataSetChanged();
                                }
                            });
                            objDialog.Show();
                            objDialog.SetCanceledOnTouchOutside(false);
                            break;
                    }
                };

                // Menu was dismissed:
                menu.DismissEvent += (s2, arg2) => {
                    //Console.WriteLine("menu dismissed");
                };

                menu.Show();
            }
            return true;
        }

        #endregion
    }
}