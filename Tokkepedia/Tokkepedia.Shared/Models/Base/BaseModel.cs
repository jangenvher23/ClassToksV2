﻿using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Tokkepedia.Shared.Models.Base
{
    public class BaseModel : ObservableObject
    {
            // Audit Fields
        public DateTime? DateCreated { get; set; } = DateTime.Now;
        public DateTime? DateModified { get; set; }
        public DateTime? DateDeleted { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }
        // Summary:
        //     Points earned.
        [JsonProperty(PropertyName = "points", NullValueHandling = NullValueHandling.Ignore)]
        public long? Points { get; set; }
        // Summary:
        //     Points count before action was applied.
        [JsonProperty(PropertyName = "points_previous", NullValueHandling = NullValueHandling.Ignore)]
        public long? PointsPrevious { get; set; }

    }
}
