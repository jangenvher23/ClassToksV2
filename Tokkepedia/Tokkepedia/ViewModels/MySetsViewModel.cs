﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppActivity = Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using Supercharge;
using Tokkepedia.Adapters;
using Tokkepedia.Setups;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokkepedia.Shared.Services;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;
using SharedSetsVM = Tokkepedia.Shared.ViewModels;
using AndroidApp = Android.App;
using Android.Text;
using Android.App;
using Result = Tokkepedia.Shared.Helpers.Result;
using AndroidX.RecyclerView.Widget;
using Android.Appwidget;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Views;

namespace Tokkepedia.ViewModels
{
    public class MySetsViewModel : ViewModelBase
    {
        #region Properties
        public string TokTypeID { get; set; }
        public string user_id { get; set; }
        public bool IsAddToksToSet { get; set; }
        public List<string> TokIdsList { get; set; }
        public List<string> TokPKsList { get; set; }
        public List<Set> SetResult;
        public List<ClassTokModel> ClassTokResult;
        public List<TokModel> TokResult;
        public List<TokModel> ToksSelected;
        public Activity Instance { get; set; }
        public Set SetModel { get; set; }
        public ClassTokModel ClasstokModel { get; set; }
        public TokModel tokList { get; set; }
        public MySetsAdapter MySetsAdapter { get; set; }
        public MyToksAdapter MyToksAdapter { get; set; }
        List<Tokmoji> ListTokmojiModel { get; set; }
        //public ObservableCollection<Set> SetList { get; private set; }
        #endregion

        #region Commands
        public RelayCommand RemoveToksFromSetCommand { get; set; }
        public RelayCommand AddSetCommand { get; set; }
        public RelayCommand CancelSetCommand { get; set; }
        public TextView lblMySetPopUp { get; set; }
        public RecyclerView RecyclerMainList { get; set; }
        public ShimmerLayout ShimmerLayout { get; set; }
        public LinearLayout LinearProgress { get; set; }
        public ProgressBar ProgressCircle { get; set; }
        public TextView ProgressText { get; set; }
        #endregion

        #region Constructors
        public AddSetPageViewModel AddSetVm => App.Locator.AddSetPageVM;
        public MySetsViewModel()
        {
            AddSetCommand = new RelayCommand(async () => await AddSet());
            RemoveToksFromSetCommand = new RelayCommand(async () => await RemoveToksFromSet());
            CancelSetCommand = new RelayCommand(CancelSet);
            user_id = Settings.GetUserModel().UserId;
            TokIdsList = new List<string>();
            TokPKsList = new List<string>();
        }
        #endregion
        #region Events
        public async Task InitializeData()
        {
            RecyclerMainList.SetAdapter(null);
            ShimmerLayout.StartShimmerAnimation();
            ShimmerLayout.Visibility = Android.Views.ViewStates.Visible;

            if (Settings.ActivityInt == Convert.ToInt16(ActivityType.LeftMenuSets) || Settings.ActivityInt == Convert.ToInt16(ActivityType.TokInfo))
            {
                var result = await GetMySetsData("", FilterType.User);
                SetResult = result.Sets.ToList();

                //get tokmojis
                var tokmojiResult = await TokMojiService.Instance.GetTokmojisAsync(null);
                ListTokmojiModel = tokmojiResult.Results.ToList();

                AssignRecyclerAdapter(SetResult);

                if (Settings.ActivityInt == Convert.ToInt16(ActivityType.LeftMenuSets))
                {
#if (_CLASSTOKS)
                    if (SetResult.Count == 0)
                        {
                            MyClassSetsActivity.Instance.TextNoSetsInfo.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            MyClassSetsActivity.Instance.TextNoSetsInfo.Visibility = ViewStates.Gone;
                        }
#else
                    if (SetResult.Count == 0)
                    {
                        MySetsActivity.Instance.TextNoSetsInfo.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        MySetsActivity.Instance.TextNoSetsInfo.Visibility = ViewStates.Gone;
                    }
#endif
                }
            }
            else if (Settings.ActivityInt == Convert.ToInt16(ActivityType.MySetsView))
            {
                if (IsAddToksToSet)
                {
#if (_CLASSTOKS)
                    ClassTokResult = await GetAllClassToks(TokTypeID, "");
                    AssignRecyclerToksAdapter(null, ClassTokResult);
#endif

#if (_TOKKEPEDIA)
                    TokResult = await GetAllToks(TokTypeID, "");
                    AssignRecyclerToksAdapter(TokResult);
#endif
                }
                else
                {
#if (_CLASSTOKS)
                    var classtokRes = await ClassService.Instance.GetClassToksAsync(new ClassTokQueryValues() { partitionkeybase = $"{SetModel.Id}-classtoks", publicfeed = false });
                    ClassTokResult = classtokRes.Results.ToList();

                    AssignRecyclerToksAdapter(null, ClassTokResult);
#endif

#if (_TOKKEPEDIA)
                    TokResult = await GetSetToks();

                    AssignRecyclerToksAdapter(TokResult, null);
#endif
                }
            }

            ShimmerLayout.Visibility = Android.Views.ViewStates.Invisible;
        }
        public void AssignRecyclerAdapter(List<Set> ItemSets)
        {
            MySetsAdapter = new MySetsAdapter(ItemSets,null, ListTokmojiModel);
            MySetsAdapter.ItemClick += MySetsAdapter.OnItemRowClick;
            RecyclerMainList.SetAdapter(MySetsAdapter);
        }
        public void AssignRecyclerToksAdapter(List<TokModel> ItemToks, List<ClassTokModel> ItemClassToks)
        {
            MyToksAdapter = new MyToksAdapter(ItemToks, ItemClassToks);
            MyToksAdapter.ItemClick += MyToksAdapter.OnItemRowClick;
            RecyclerMainList.SetAdapter(MyToksAdapter);
        }

        public async Task AddSet()
        {
            Intent nextactivty;
            if (Settings.ActivityInt == Convert.ToInt16(ActivityType.LeftMenuSets))
            {
#if (_CLASSTOKS)
                nextactivty = new Intent(Instance, typeof(AddClassSetActivity));
                Instance.StartActivity(nextactivty);
#else
                nextactivty = new Intent(Instance, typeof(AddSetActivity));
                Instance.StartActivity(nextactivty);
#endif
            }
            else if (Settings.ActivityInt == Convert.ToInt16(ActivityType.MySetsView))
            {
                ProgressCircle.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.LightBlue, Android.Graphics.PorterDuff.Mode.Multiply);
                ProgressText.Text = "Saving...";
                LinearProgress.Visibility = ViewStates.Visible;
                Instance.Window.AddFlags(WindowManagerFlags.NotTouchable);
                //Add Toks To Set
                ResultModel result; //Adding toks to set

#if (_TOKKEPEDIA)
                result = await AddToksToSet(TokIdsList);
#endif

#if (_CLASSTOKS)
                result = await AddClassToksToSet(TokIdsList, TokPKsList);
#endif
                Instance.Window.ClearFlags(WindowManagerFlags.NotTouchable);

                for (int i = 0; i < TokIdsList.Count; i++)
                {
                    SetModel.TokIds.Add(TokIdsList[i]);
                }
                TokIdsList.Clear();
                TokPKsList.Clear();

                LinearProgress.Visibility = ViewStates.Gone;

                var builder = new AlertDialog.Builder(Instance);
                builder.SetMessage(result.ResultMessage.ToString());
                builder.SetTitle("");
                var dialog = (AndroidApp.AlertDialog)null;
                builder.SetPositiveButton("OK" ?? "OK", (d, index) => 
                {
                    CancelSet();
                });
                dialog = builder.Create();
                dialog.Show();
                dialog.SetCanceledOnTouchOutside(false);
            }
            else if (Settings.ActivityInt == Convert.ToInt16(ActivityType.TokInfo))
            {
                ProgressCircle.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.LightBlue, Android.Graphics.PorterDuff.Mode.Multiply);
                ProgressText.Text = "Saving...";
                LinearProgress.Visibility = ViewStates.Visible;
                Instance.Window.AddFlags(WindowManagerFlags.NotTouchable);
                //Add Toks To Set
                var result = await AddTokToSet(); //Adding toks to set from TokInfo
                SetModel.TokIds.Add(tokList.Id);
                TokIdsList.Clear(); //After adding, clear TokIdsList
                TokPKsList.Clear();
                Instance.Window.ClearFlags(WindowManagerFlags.NotTouchable);
                LinearProgress.Visibility = ViewStates.Gone;

                var builder = new AlertDialog.Builder(Instance);
                builder.SetMessage(result.ResultMessage.ToString());
                builder.SetTitle("");
                var dialog = (AlertDialog)null;
                builder.SetPositiveButton("OK" ?? "OK", (d, index) => 
                {
                    CancelSet();
                });
                dialog = builder.Create();
                dialog.Show();
                dialog.SetCanceledOnTouchOutside(false);
            }
        }
        public async Task RemoveToksFromSet()
        {
            ProgressCircle.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Red, Android.Graphics.PorterDuff.Mode.Multiply);
            ProgressText.Text = "Deleting...";
            LinearProgress.Visibility = ViewStates.Visible;
            Instance.Window.AddFlags(WindowManagerFlags.NotTouchable);
            ResultModel result = new ResultModel();
#if (_CLASSTOKS)
            result = await DeleteClassToksFromSet(TokIdsList);
#endif

#if (_TOKKEPEDIA)
            result = await DeleteToksFromSet(TokIdsList); // Deleting Toks From Set
#endif
            for (int i = 0; i < TokIdsList.Count; i++)
            {
                SetModel.TokIds.Remove(TokIdsList[i]);
            }
            TokIdsList.Clear();
            TokPKsList.Clear();

            LinearProgress.Visibility = ViewStates.Gone;

            var builder = new AndroidApp.AlertDialog.Builder(Instance);
            builder.SetMessage(result.ResultMessage.ToString());
            builder.SetTitle("");
            var dialog = (AndroidApp.AlertDialog)null;
            builder.SetPositiveButton("OK" ?? "OK", (d, index) =>
            {
                CancelSet();
            });
            dialog = builder.Create();
            dialog.Show();
            dialog.SetCanceledOnTouchOutside(false);

            LinearProgress.Visibility = ViewStates.Gone;
        }
        public void CancelSet()
        {
            if (Settings.ActivityInt == Convert.ToInt16(ActivityType.LeftMenuSets))
            {
                //MainActivity.Instance.Finish();
                //var _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
                //_navigationService.NavigateTo(ViewModelLocator.MainPageKey);
                var nextActivity = new Intent(MainActivity.Instance, typeof(MainActivity));
                nextActivity.SetFlags(ActivityFlags.ReorderToFront);
                MainActivity.Instance.StartActivity(nextActivity);

                Instance.Finish();
            }
            else if (Settings.ActivityInt == Convert.ToInt16(ActivityType.MySetsView))
            {
#if (_CLASSTOKS)
                Settings.ActivityInt = Convert.ToInt16(ActivityType.LeftMenuSets);

                //var _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
                //_navigationService.NavigateTo(ViewModelLocator.MyClassSetsViewPageKey, JsonConvert.SerializeObject(SetModel));
#else
                var _navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
                _navigationService.NavigateTo(ViewModelLocator.MySetsViewPageKey, JsonConvert.SerializeObject(SetModel));
#endif
                Instance.Finish();
            }
            else
            {
                Instance.Finish();
            }
        }
        public void PopUpMenuClick(View v)
        {
            Android.Widget.PopupMenu menu = new Android.Widget.PopupMenu(Instance, v);
            
            // Call inflate directly on the menu:
            menu.Inflate(Resource.Menu.mysets_popmenu);
            var viewtokinfo = menu.Menu.FindItem(Resource.Id.item0);
            var viewtoks = menu.Menu.FindItem(Resource.Id.item1);
            var playtokcards = menu.Menu.FindItem(Resource.Id.item2);
            var playtokchoice = menu.Menu.FindItem(Resource.Id.item3);
            var playtokmatch = menu.Menu.FindItem(Resource.Id.item4);
            var gameset = menu.Menu.FindItem(Resource.Id.item5);
            var edit = menu.Menu.FindItem(Resource.Id.item6);
            var delete = menu.Menu.FindItem(Resource.Id.item7);

            if (TokTypeID == "") //From Sets
            {
                SetModel = MySetsAdapter.items[(int)(v as TextView).Tag];
                viewtokinfo.SetVisible(false);
            }
            else if (TokTypeID != "") //From Toks
            {
                tokList = MyToksAdapter.items[(int)(v as TextView).Tag];

                viewtoks.SetVisible(false);
                playtokcards.SetVisible(false);
                playtokchoice.SetVisible(false);
                playtokmatch.SetVisible(false);
                gameset.SetVisible(false);
                edit.SetVisible(false);
                delete.SetVisible(false);
            }

            Android.Support.V7.App.AlertDialog.Builder alertDiag;
            AppActivity.Dialog diag;

            // A menu item was clicked:
            menu.MenuItemClick += (s1, arg1) => {
                Intent nextActivity; string modelConvert;
                switch (arg1.Item.TitleFormatted.ToString().ToLower())
                {
                    case "view tok info":
                        nextActivity = new Intent(MainActivity.Instance, typeof(TokInfoActivity));
                        modelConvert = JsonConvert.SerializeObject(tokList);
                        nextActivity.PutExtra("tokModel", modelConvert);
                        MainActivity.Instance.StartActivity(nextActivity);
                        
                        break;
                    case "view toks":
                        nextActivity = new Intent(MainActivity.Instance, typeof(MySetsViewActivity));
                        modelConvert = JsonConvert.SerializeObject(SetModel);
                        nextActivity.PutExtra("setModel", modelConvert);
                        MainActivity.Instance.StartActivity(nextActivity);
                        break;
                    case "play tok cards":
                        nextActivity = new Intent(MainActivity.Instance, typeof(TokCardsMiniGameActivity));
                        modelConvert = JsonConvert.SerializeObject(SetModel);
                        nextActivity.PutExtra("setModel", modelConvert);
                        MainActivity.Instance.StartActivity(nextActivity);
                        break;
                    case "play tok choice":
                        if (SetModel.TokIds.Count > 3)
                        {
                            alertDiag = new Android.Support.V7.App.AlertDialog.Builder(Instance);
                            alertDiag.SetTitle("Tok Choice");
                            alertDiag.SetMessage("Continue to Play Set?");
                            alertDiag.SetPositiveButton(Html.FromHtml("<font color='#dc3545'>Return</font>", FromHtmlOptions.ModeLegacy), (senderAlert, args) => {
                                alertDiag.Dispose();
                            });
                            alertDiag.SetNegativeButton(Html.FromHtml("<font color='#007bff'>Play</font>", FromHtmlOptions.ModeLegacy), (senderAlert, args) => {
                                nextActivity = new Intent(MainActivity.Instance, typeof(TokChoiceActivity));
                                modelConvert = JsonConvert.SerializeObject(SetModel);
                                nextActivity.PutExtra("setModel", modelConvert);
                                MainActivity.Instance.StartActivity(nextActivity);
                            });
                            diag = alertDiag.Create();
                            diag.Show();
                        }
                        else
                        {
                            var mssgDialog = new AlertDialog.Builder(Instance);
                            var alertMssg = mssgDialog.Create();
                            alertMssg.SetTitle("");
                            alertMssg.SetIcon(Resource.Drawable.alert_icon_blue);
                            alertMssg.SetMessage("Tok Choice requires at least 4 toks in the set. Add more toks to play.");
                            alertMssg.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) => {});
                            alertMssg.Show();
                        }
                        
                        break;
                    case "play tok match":
                        nextActivity = new Intent(MainActivity.Instance, typeof(TokMatchActivity));
                        modelConvert = JsonConvert.SerializeObject(SetModel);
                        nextActivity.PutExtra("setModel", modelConvert);
                        nextActivity.PutExtra("isSet", true);
                        MainActivity.Instance.StartActivity(nextActivity);
                        break;
                    case "edit":
                        nextActivity = new Intent(MainActivity.Instance, typeof(AddSetActivity));
                        modelConvert = JsonConvert.SerializeObject(SetModel);
                        nextActivity.PutExtra("setModel", modelConvert);
                        nextActivity.AddFlags(ActivityFlags.NewTask);
                        MainActivity.Instance.StartActivity(nextActivity);
                        break;
                    case "delete":
                        alertDiag = new Android.Support.V7.App.AlertDialog.Builder(Instance);
                        alertDiag.SetTitle("Confirm");
                        alertDiag.SetMessage("Are you sure you want to delete " + SetModel.Name + "?");
                        alertDiag.SetPositiveButton("Cancel", (senderAlert, args) => {
                            alertDiag.Dispose();
                        });
                        alertDiag.SetNegativeButton(Html.FromHtml("<font color='#dc3545'>Delete</font>", FromHtmlOptions.ModeLegacy), async(senderAlert, args) => {
                            ProgressCircle.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.LightBlue, Android.Graphics.PorterDuff.Mode.Multiply);
                            ProgressText.Text = "Deleting set...";
                            LinearProgress.Visibility = ViewStates.Visible;
                            Instance.Window.AddFlags(WindowManagerFlags.NotTouchable);

                            await SetService.Instance.DeleteSetAsync(SetModel.Id);
                            //(Instance).RunOnUiThread(async () => await DeleteSet(setList.Id));

                            LinearProgress.Visibility = ViewStates.Gone;
                            Instance.Window.ClearFlags(WindowManagerFlags.NotTouchable);

                            var detail = SetResult.FirstOrDefault(a => a.Id == SetModel.Id);
                            if (detail != null)
                            {
                                var dialogDelete = new AlertDialog.Builder(Instance);
                                var alertDelete = dialogDelete.Create();
                                alertDelete.SetTitle("");
                                alertDelete.SetIcon(Resource.Drawable.alert_icon_blue);
                                alertDelete.SetMessage("Set deleted!");
                                alertDelete.SetButton(Html.FromHtml("<font color='#007bff'>OK</font>", FromHtmlOptions.ModeLegacy), (d, fv) =>
                                {
                                    SetResult.Remove(detail);
                                    AssignRecyclerAdapter(SetResult);
                                });
                                alertDelete.Show();
                                alertDelete.SetCanceledOnTouchOutside(false);
                            }
                        });
                        diag = alertDiag.Create();
                        diag.Show();
                        break;
                }
                //Console.WriteLine("{0} selected", arg1.Item.TitleFormatted);
            };

            // Menu was dismissed:
            menu.DismissEvent += (s2, arg2) => {
                //Console.WriteLine("menu dismissed");
            };

            menu.Show();
        }
        public async Task<SharedSetsVM.MySetsViewModel> GetMySetsData(string filter = "", FilterType type = FilterType.None, bool WithSelection = false)
        {
            SharedSetsVM.MySetsViewModel viewModel = new SharedSetsVM.MySetsViewModel();
            if (user_id != null)
            {
                ResultData<Set> setResults = null;
                switch (type)
                {
                    case FilterType.TokType:
                        setResults = await SetService.Instance.GetSetsAsync(new SetQueryValues() { toktypeid = filter });
                        break;
                    case FilterType.User:
                        var setQueryValues = new SetQueryValues() { userid = user_id };
                        setResults = await SetService.Instance.GetSetsAsync(setQueryValues);
                        break;
                }
                viewModel.Sets = setResults?.Results ?? new List<Set>();
                viewModel.Token = setResults?.ContinuationToken ?? string.Empty;
            }
            return viewModel;
        }
        //Add Toks
        public async Task<List<TokModel>> GetAllToks(string tokTypeId, string token)
        {
            List<TokModel> tokResult = new List<TokModel>();
            if (!string.IsNullOrEmpty(tokTypeId))
            {
                var qry = new TokQueryValues() { toktype = tokTypeId, sortby = Settings.SortByFilter };
                if (!string.IsNullOrEmpty(token))
                {
                    qry.loadmore = "yes";
                    qry.token = token;
                }
                tokResult = await TokService.Instance.GetToksAsync(qry);
                
            }
            return tokResult;
        }
        public async Task<List<ClassTokModel>> GetAllClassToks(string tokTypeId, string token)
        {
            List<ClassTokModel> tokResult = new List<ClassTokModel>();
            if (!string.IsNullOrEmpty(tokTypeId))
            {
                var qry = new ClassTokQueryValues() { toktype = tokTypeId};
                if (!string.IsNullOrEmpty(token))
                {
                    qry.paginationid = token;
                }
                var resultData = await ClassService.Instance.GetClassToksAsync(qry);
                tokResult = resultData.Results.ToList();

            }
            return tokResult;
        }

        public async Task<ResultModel> AddToksToSet(List<string> tokIds)
        {
            ResultModel result = new ResultModel() { ResultEnum = Result.Failed, ResultMessage = "Saving Failed!" };
            if (tokIds.Count > 0)
            {
                var flag = await SetService.Instance.AddToksToSetAsync(SetModel.Id, user_id, tokIds.ToArray());
                if (flag)
                {
                    result.ResultEnum = Result.Success;
                    result.ResultMessage = "Save Successful!";
                }
            }
            return result;
        }

        public async Task<ResultModel> AddClassToksToSet(List<string> tokIds, List<string> tokPKs)
        {
            ResultModel result = new ResultModel() { ResultEnum = Result.Failed, ResultMessage = "Saving Failed!" };
            if (tokIds.Count > 0)
            {
                var flag = await ClassService.Instance.AddClassToksToClassSetAsync(SetModel.Id, SetModel.PartitionKey, tokIds, tokPKs);
                if (flag)
                {
                    result.ResultEnum = Result.Success;
                    result.ResultMessage = "Save Successful!";
                }
            }
            return result;
        }

        public async Task<ResultModel> AddTokToSet()
        {
            ResultModel result = new ResultModel() { ResultEnum = Result.Failed, ResultMessage = "Saving Failed!" };
            var flag = await SetService.Instance.AddTokToSetAsync(SetModel.Id, user_id, tokList.Id);
            if (flag)
            {
                result.ResultEnum = Result.Success;
                result.ResultMessage = "Save Successful!";
            }
            return result;
        }

        //Remove Toks From Set
        public async Task<List<TokModel>> GetSetToks(int takeCnt = 100, int skip = 0, bool isListing = false, bool isCard = false)
        {
            List<TokModel> tokResult = new List<TokModel>();
            if (skip < SetModel.TokIds.Count) // Check if there's new
            {
                var tokRes = await TokService.Instance.GetToksByIdsAsync(SetModel.TokIds.Skip(skip).Take(takeCnt).ToList());
                tokResult = tokRes;
            }
            return tokResult;
        }
        public async Task<ResultModel> DeleteToksFromSet(List<string> tokIds)
        {
            ResultModel result = new ResultModel() { ResultEnum = Result.Failed, ResultMessage = "Failed to Delete!" };
            if (tokIds.Count > 0)
            {
                var flag = await SetService.Instance.DeleteToksFromSetAsync(SetModel, tokIds.ToArray());
                if (flag)
                {
                    result.ResultEnum = Result.Success;
                    result.ResultMessage = "Delete Successful!";
                }
            }
            return result;
        }

        public async Task<ResultModel> DeleteClassToksFromSet(List<string> tokIds)
        {
            ResultModel result = new ResultModel() { ResultEnum = Result.Failed, ResultMessage = "Failed to Delete!" };
            if (tokIds.Count > 0)
            {
                ClassSetModel classSetModel = new ClassSetModel();
                classSetModel.Id = SetModel.Id;
                classSetModel.PartitionKey = SetModel.PartitionKey;
                var flag = await ClassService.Instance.DeleteClassToksFromClassSetAsync(classSetModel, tokIds);
                if (flag)
                {
                    result.ResultEnum = Result.Success;
                    result.ResultMessage = "Delete Successful!";
                }
            }
            return result;
        }

        //private async Task DeleteSet(string id)
        //{
        //    await SetService.Instance.DeleteSetAsync(id);
        //}
#endregion
    }
}