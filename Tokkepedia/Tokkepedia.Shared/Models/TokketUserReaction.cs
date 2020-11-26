﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Tokket.Tokkepedia;

namespace Tokkepedia.Shared.Models
{
    public class TokketUserReaction : TokketUser
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "kind")]
        public string Kind = null;
    }
}
