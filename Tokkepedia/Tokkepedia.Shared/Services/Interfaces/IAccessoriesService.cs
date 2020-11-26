﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tokkepedia.Shared.Models;
using Tokket.Tokkepedia;
using Tokket.Tokkepedia.Tools;

namespace Tokkepedia.Shared.Services.Interfaces
{
    public interface IAccessoriesService
    {
        Task<DefaultColor> DefaultColorAsync(string colorHex, string userId, string keyvalue, string key = "tok_type", string method = "get");
        Task<bool> UpdateColorAsync(string colorHex, string id);
    }
}
