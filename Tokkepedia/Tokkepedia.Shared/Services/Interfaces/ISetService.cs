﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tokkepedia.Shared.Helpers;
using Tokkepedia.Shared.Models;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;

namespace Tokkepedia.Shared.Services.Interfaces
{
    public interface ISetService
    {
        Task<ResultModel> CreateSetAsync(Set set);
        Task<TokModel> GetTokAsync(string id);
        Task<Set> GetSetAsync(string id);
        Task<ResultModel> UpdateSetAsync(Set set);
        Task<ResultModel> DeleteSetAsync(string id);
        Task<ResultData<Set>> GetSetsAsync(SetQueryValues values = null);
        //Task<ResultData<Set>> GetGameSetsAsync(SetQueryValues values = null);

        Task<GameSet> GetGameSetAsync(string id, string pk);
        Task<ResultData<GameSet>> GetGameSetsAsync(SetQueryValues values = null);
        Task<bool> AddTokToSetAsync(string setId, string setUserId, string tokId);
        Task<bool> DeleteTokFromSetAsync(string setId, string setUserId, string tokId);
        Task<bool> AddToksToSetAsync(string setId, string setUserId, string[] tokIds);
        Task<bool> DeleteToksFromSetAsync(Set set, string[] tokIds);
    }
}
