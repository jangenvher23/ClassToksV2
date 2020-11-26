﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Tokkepedia.Shared.Models;
using Xamarin.Essentials;
using ServiceAccount = Tokkepedia.Shared.Services;

namespace Tokkepedia
{
    [Activity(Label = "Quote of the Hour", Theme = "@style/CustomAppTheme")]
    public class QuoteActivity : BaseActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.quote_page);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            // Create your application here     

            //setting the value of the quote
            GetQuoteOfTheHour();

            BackButton.Click += delegate
            {
                this.Finish();
            };

            ShareButton.Click += async(sender,e) =>
            {
                await ShareText(QuoteTitle.Text);
            };
        }

        public async Task ShareText(string text)
        {
            await Share.RequestAsync(new ShareTextRequest
            {
                Text = text,
                Title = "Share Text"
            });
        }

        private async void GetQuoteOfTheHour()
        {
            OggClass sendItem = await ServiceAccount.CommonService.Instance.GetQuoteAsync();
            QuoteTitle.Text = sendItem.PrimaryText;
            QuoteAuthor.Text = sendItem.SecondaryText;
            QuoteCategory.Text = "Category: " + sendItem.Category;

        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
        public Button BackButton => FindViewById<Button>(Resource.Id.btnTokkepedia);
        public Button ShareButton => FindViewById<Button>(Resource.Id.btnShare);
        public TextView QuoteTitle => FindViewById<TextView>(Resource.Id.lblQuote);
        public TextView QuoteAuthor => FindViewById<TextView>(Resource.Id.lblQuoteAuthor);
        public TextView QuoteCategory => FindViewById<TextView>(Resource.Id.lblQuoteCategory);
    }
}