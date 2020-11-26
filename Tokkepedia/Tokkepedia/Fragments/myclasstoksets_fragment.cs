﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Supercharge;
using Tokkepedia.Adapters;
using Tokkepedia.Listener;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokkepedia.Shared.ViewModels;
using OperationCanceledException = Android.OS.OperationCanceledException;

namespace Tokkepedia.Fragments
{
    public class myclasstoksets_fragment : AndroidX.Fragment.App.Fragment
    {
        internal static myclasstoksets_fragment Instance { get; private set; }
        View page; string UserId; public List<ClassSetModel> ListClassTokSets; //public List<ClassSetViewModel> ListClassSetModel;
        MySetsAdapter MySetsAdapter; string groupId = "";
        GridLayoutManager mLayoutManager; SwipeRefreshLayout refreshLayout = null;
        public ViewModels.MySetsViewModel MySetsVm => App.Locator.MySetsPageVM;
        public myclasstoksets_fragment(string _groupId)
        {
            groupId = _groupId;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var v = inflater.Inflate(Resource.Layout.mysets_toksets_page, container, false);
            page = v;
            Instance = this;
            UserId = Settings.GetUserModel().UserId;
            ListClassTokSets = new List<ClassSetModel>();

            MySetsVm.RecyclerMainList = RecyclerMainList;
            MySetsVm.ShimmerLayout = ShimmerLayout;

            mLayoutManager = new GridLayoutManager(Application.Context, 1);
            RecyclerMainList.SetLayoutManager(mLayoutManager);

            if (!string.IsNullOrEmpty(groupId))
            {
                TextNothingFound.Visibility = ViewStates.Visible;
            }

            if (Settings.ActivityInt == Convert.ToInt16(ActivityType.LeftMenuSets) || Settings.ActivityInt == Convert.ToInt16(ActivityType.TokInfo) || Settings.ActivityInt == Convert.ToInt16(ActivityType.ClassGroupActivity))
            {
                ((Activity)Context).RunOnUiThread(async () => await Initialize());
            }
            else if (Settings.ActivityInt == Convert.ToInt16(ActivityType.MySetsView))
            {
                ((Activity)Context).RunOnUiThread(async () => await MySetsVm.InitializeData());
            }

            refreshLayout = v.FindViewById<SwipeRefreshLayout>(Resource.Id.mytoksets_swiperefresh);
            refreshLayout.SetColorSchemeColors(Color.ParseColor("#3498db"), Color.ParseColor("#4ea4de"), Color.ParseColor("#68aede"), Color.ParseColor("#88bee3"));
            refreshLayout.Refresh += RefreshLayout_Refresh;

            if (RecyclerMainList != null)
            {
                RecyclerMainList.HasFixedSize = true;

                var onScrollListener = new XamarinRecyclerViewOnScrollListener(mLayoutManager);
                onScrollListener.LoadMoreEvent += async (object sender, EventArgs e) =>
                {
                    //Load more stuff here
                    if (!string.IsNullOrEmpty(RecyclerMainList.ContentDescription))
                    {
                        await Initialize(RecyclerMainList.ContentDescription);
                    }
                };


                RecyclerMainList.AddOnScrollListener(onScrollListener);

                RecyclerMainList.SetLayoutManager(mLayoutManager);
            }

            return v;
        }
        private void showBottomDialog()
        {
            bottomProgress.Visibility = ViewStates.Visible;
        }
        private void hideBottomDialog()
        {
            bottomProgress.Visibility = ViewStates.Gone;
        }

        private async Task Initialize(string paginationId = "")
        {
            int lastposition = RecyclerMainList.ChildCount - 1;
            var taskCompletionSource = new TaskCompletionSource<List<ClassTokModel>>();
            CancellationToken cancellationToken;

            RecyclerMainList.SetAdapter(null);
            ShimmerLayout.StartShimmerAnimation();
            ShimmerLayout.Visibility = ViewStates.Visible;

            //ListClassSetModel = new List<ClassSetViewModel>();
            ListClassTokSets = new List<ClassSetModel>();

            cancellationToken.Register(() =>
            {
                // We received a cancellation message, cancel the TaskCompletionSource.Task
                taskCompletionSource.TrySetCanceled();
            });

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));
                cancellationToken = cancellationTokenSource.Token;

                var setQueryValues = new ClassSetQueryValues()
                {
                    userid = UserId,
                    groupid = groupId,
                    partitionkeybase = "classsets",
                    startswith = false,
                    paginationid = paginationId
                };

                if (!string.IsNullOrEmpty(paginationId))
                {
                    showBottomDialog();
                }

                var result = await ClassService.Instance.GetClassSetAsync(setQueryValues, cancellationToken);

                if (!string.IsNullOrEmpty(paginationId))
                {
                    hideBottomDialog();
                }

                if (result.ContinuationToken == "cancelled")
                {
                    ShimmerLayout.Visibility = ViewStates.Gone;
                    showRetryDialog("Task was cancelled.");
                }
                else
                {
                    RecyclerMainList.ContentDescription = result.ContinuationToken;

                    ListClassTokSets.AddRange(result.Results.ToList());

                    /*for (int i = 0; i < ListClassTokSets.Count; i++)
                    {
                        ClassSetViewModel ModelItem = new ClassSetViewModel();

                        var classtokRes = await ClassService.Instance.GetClassToksAsync(new ClassTokQueryValues() { partitionkeybase = $"{ListClassTokSets[i].Id}-classtoks" }, cancellationToken);
                        if (classtokRes.ContinuationToken == "cancelled")
                        {
                            ShimmerLayout.Visibility = ViewStates.Gone;
                            showRetryDialog("Task was cancelled.");
                            return;
                        }
                        ModelItem.ClassToks = classtokRes.Results.ToList();
                        ModelItem.ClassSet = ListClassTokSets[i];
                        ListClassSetModel.Add(ModelItem);
                    }*/
                }
            }

            if (ListClassTokSets.Count == 0)
            {
                TextNothingFound.Text = "No class sets.";
            }
            
            ShimmerLayout.Visibility = ViewStates.Gone;
            AssignRecyclerAdapter(); //ListClassSetModel
            if (!string.IsNullOrEmpty(paginationId))
            {
                RecyclerMainList.ScrollToPosition(lastposition);
            }
        }

        private void showRetryDialog(string message)
        {
            var builder = new Android.Support.V7.App.AlertDialog.Builder(MyClassSetsActivity.Instance)
                            .SetMessage(message)
                            .SetPositiveButton("Cancel", (_, args) =>
                            {

                            })
                            .SetNegativeButton("Retry",async (_, args) =>
                            {
                                await Initialize();
                            })
                            .SetCancelable(false)
                            .Show();
        }
        public void AssignRecyclerAdapter()
        {
            MySetsAdapter = new MySetsAdapter(null, ListClassTokSets, null);
            MySetsAdapter.ItemClick += MySetsAdapter.OnItemRowClick;
            RecyclerMainList.SetAdapter(MySetsAdapter);
        }

        public void PassItemClassSetsFromAddClassSet(ClassSetModel model, bool isSave = true)
        {
            if (isSave)
            {
                ListClassTokSets.Insert(0, model);

                AssignRecyclerAdapter();
            }
            else
            {
                //If Update
                var result = ListClassTokSets.FirstOrDefault(c => c.Id == model.Id);
                if (result != null) //If Edit
                {
                    int ndx = ListClassTokSets.IndexOf(result);
                    ListClassTokSets.Remove(result);
                    ListClassTokSets.Insert(ndx, model);
                }
                AssignRecyclerAdapter();
            }
        }

        public void deleteItemClassSet(ClassSetModel model)
        {
            var result = ListClassTokSets.FirstOrDefault(c => c.Id == model.Id);
            if (result != null) //If Edit
            {
                int ndx = ListClassTokSets.IndexOf(result);
                ListClassTokSets.Remove(result);
            }
            AssignRecyclerAdapter();
        }
        private void RefreshLayout_Refresh(object sender, EventArgs e)
        {
            //Data Refresh Place  
            BackgroundWorker work = new BackgroundWorker();
            work.DoWork += Work_DoWork;
            work.RunWorkerCompleted += Work_RunWorkerCompleted;
            work.RunWorkerAsync();
        }
        private void Work_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            refreshLayout.Refreshing = false;
        }
        private void Work_DoWork(object sender, DoWorkEventArgs e)
        {
            RecyclerMainList.ContentDescription = null;
            FilterType type = (FilterType)Settings.FilterTag;
            ((Activity)Context).RunOnUiThread(async () => await Initialize());
            Thread.Sleep(1000);
        }
        public ProgressBar bottomProgress => page.FindViewById<ProgressBar>(Resource.Id.bottomProgress);
        public RecyclerView RecyclerMainList => page.FindViewById<RecyclerView>(Resource.Id.recyclerView_mytoksets);
        public ShimmerLayout ShimmerLayout => page.FindViewById<ShimmerLayout>(Resource.Id.mysets_shimmer_view_container);
        public TextView TextNothingFound => page.FindViewById<TextView>(Resource.Id.TextNothingFound);
    }
}