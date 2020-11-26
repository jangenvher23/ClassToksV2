﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Icu.Util;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Tokkepedia.Fragments
{
    public class datepicker_fragment : AndroidX.AppCompat.App.AppCompatDialogFragment, DatePickerDialog.IOnDateSetListener
    {
        // TAG can be any string of your choice.
        public static readonly string TAG = "X:" + typeof(datepicker_fragment).Name.ToUpper();

        // Initialize this value to prevent NullReferenceExceptions.
        Action<DateTime> _dateSelectedHandler = delegate { };

        public static datepicker_fragment NewInstance(Action<DateTime> onDateSelected)
        {
            datepicker_fragment frag = new datepicker_fragment();
            frag._dateSelectedHandler = onDateSelected;
            return frag;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            DateTime currently = DateTime.Now;
            DatePickerDialog dialog = new DatePickerDialog(Activity,
                                                           this,
                                                           currently.Year,
                                                           currently.Month - 1,
                                                           currently.Day);
            return dialog;
        }

        public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth)
        {
            DateTime selectedDate = new DateTime(year, monthOfYear + 1, dayOfMonth);
            Log.Debug(TAG, selectedDate.ToLongDateString());
            _dateSelectedHandler(selectedDate);
        }
    }
}