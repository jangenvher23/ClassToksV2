﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;

namespace Tokkepedia.Shared.Services.Interfaces
{
    public interface IBadgeService
    {
        Task<ResultData<BadgeOwned>> GetUserBadgesAsync(string userId, string paginationId = null);
        Task<bool> SelectBadgeAsync(string badgeId);
        Task<bool> UpdateUserBadgeColor(string badgeId, string id, string color = "black");
        Task<bool> UpdateUserPointsSymbolColorAsync(string color, string UserId);
    }
}
