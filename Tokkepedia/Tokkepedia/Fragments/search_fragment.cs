﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XFragment = AndroidX.Fragment.App.Fragment;
using XFragmentManager = AndroidX.Fragment.App.FragmentManager;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Tokkepedia.Shared.Helpers;
using AndroidX.ViewPager.Widget;
using Tokkepedia.Adapters;
using Google.Android.Material.Tabs;
using AndroidX.CoordinatorLayout.Widget;
using Android.Views.InputMethods;
using Android.InputMethodServices;
using Newtonsoft.Json.Bson;

namespace Tokkepedia.Fragments
{
    public class search_fragment : AndroidX.Fragment.App.Fragment
    {
        internal static search_fragment Instance { get; private set; }
        Android.Views.View v;
        public string toksfilter = "";
        public ViewPager viewpager;
        public TabLayout tabLayout => v.FindViewById<TabLayout>(Resource.Id.tabLayout);
        private AdapterStatePagerFragmentX fragment { get; set; }
        public Button SearchButton => v.FindViewById<Button>(Resource.Id.SearchButton);
        public EditText SearchText => v.FindViewById<EditText>(Resource.Id.SearchText);
        public List<XFragment> fragments = new List<XFragment>();
        public List<string> fragmentTitles = new List<string>();
        AdapterStatePagerFragmentX adapter; public bool isSearchedClicked = false;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            v = inflater.Inflate(Resource.Layout.search_page, container, false);

            Instance = this;
            isSearchedClicked = false;
#if (_CLASSTOKS)
            framTip.Visibility = ViewStates.Visible;
            if (Settings.ClassSearchFilter == (int)FilterType.All)
            {
                SearchText.Hint = "Search in Public Class Toks";
            }
            else if (Settings.ClassSearchFilter == (int)FilterType.Featured)
            {
                SearchText.Hint = "Search in My Class Toks";
            }
            
#endif

            Typeface font = Typeface.CreateFromAsset(MainActivity.Instance.Application.Assets, "fa_solid_900.otf");
            SearchButton.Typeface = font;
            viewpager = v.FindViewById<ViewPager>(Resource.Id.viewpagerSearch);

            loadSearchFragments();
            setupViewPager(viewpager);
            tabLayout.SetupWithViewPager(viewpager);

            SearchButton.Click -= searchButtonClicked;
            SearchButton.Click += searchButtonClicked;
            viewpager.PageSelected += Viewpager_PageSelected;

            return v;
        }

        public void searchButtonClicked(object sender, EventArgs e)
        {
            isSearchedClicked = true;
            //hide keyboard
            var inputManager = (InputMethodManager)this.Activity.GetSystemService(Context.InputMethodService);
            inputManager.HideSoftInputFromWindow(SearchButton.WindowToken, 0);

            int tabIndx = tabLayout.SelectedTabPosition;
            string tileText = "";
            fragments.RemoveAt(tabIndx);
            fragmentTitles.RemoveAt(tabIndx);
            switch (tabIndx)
            {
                case 0:
#if (_TOKKEPEDIA)
                        fragments.Insert(0, new home_fragment()
                        {
                            isSearchFragment = true,
                            filterText = SearchText.Text,
                            filterType = Shared.Helpers.FilterType.Text
                        });
                        tileText = "Toks";
#endif
#if (_CLASSTOKS)
                    fragments.Insert(0, new classtoks_fragment(Settings.GetUserModel().UserId)
                    {
                        isSearchFragment = true,
                        filterText = SearchText.Text,
                        filterType = Shared.Helpers.FilterType.Text
                    });
                    tileText = "Class Toks";
#endif

                    break;
                case 1:
                    fragments.Insert(1, new users_fragment()
                    {
                        filterText = SearchText.Text
                    });
                    tileText = "Users";
                    break;
                case 2:
                    fragments.Insert(2, new categories_fragment()
                    {
                        filterText = SearchText.Text
                    });
                    tileText = "Categories";
                    break;
            }
            fragmentTitles.Insert(tabIndx, tileText);
            setupViewPager(viewpager, tabIndx);
            tabLayout.GetTabAt(tabIndx).Select();
        }
        private void loadSearchFragments()
        {
            fragments.Clear();
            fragmentTitles.Clear();

            fragments.Add(new XFragment());
            fragments.Add(new XFragment());
            fragments.Add(new XFragment());

            fragmentTitles.Add("Toks");
            fragmentTitles.Add("Users");
            fragmentTitles.Add("Categories");
        }

        public void setupViewPager(ViewPager viewPager, int index = 0)
        {
            adapter = new AdapterStatePagerFragmentX(MainActivity.Instance.SupportFragmentManager, fragments, fragmentTitles);
            viewPager.Adapter = adapter;
            fragment = adapter;
        }

        private void Viewpager_PageSelected(object sender, AndroidX.ViewPager.Widget.ViewPager.PageSelectedEventArgs e)
        {
            int tabIndx = e.Position;
            adapter.RemoveFragment(adapter.GetItem(tabIndx));
            switch (tabIndx)
            {
                case 0:
                    adapter.InsertFragmentAtPosition(new home_fragment()
                    {
                        filterText = SearchText.Text,
                        filterType = Shared.Helpers.FilterType.Text
                    }, tabIndx);
                    break;
                case 1:
                    adapter.InsertFragmentAtPosition(new users_fragment()
                    {
                        filterText = SearchText.Text
                    }, tabIndx);
                    break;
                case 2:
                    adapter.InsertFragmentAtPosition(new categories_fragment()
                    {
                        filterText = SearchText.Text
                    }, tabIndx);
                    break;
            }                   
            adapter.GetItemPosition(adapter.GetItem(tabIndx));
            viewpager.Adapter.NotifyDataSetChanged();
            fragment = adapter;
        }

        public FrameLayout framTip => v.FindViewById<FrameLayout>(Resource.Id.frameTip);
    }
}