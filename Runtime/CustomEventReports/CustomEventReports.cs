using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DuckbearLab.UniSim.Networking;

namespace DuckbearLab.UniSim.CustomEventReports
{
    #region Custom Event Report Attribute
    public class CustomEventReportAttribute : Attribute
    {
        public uint EventReportType { get; private set; }

        public CustomEventReportAttribute(uint eventReportType)
        {
            EventReportType = eventReportType;
        }
    }

    public class CustomEventReportArrayAttribute : Attribute
    {
        public string ArrayLengthField { get; private set; }

        public CustomEventReportArrayAttribute(string arrayLengthField)
        {
            ArrayLengthField = arrayLengthField;
        }
    }

    public class CustomEventReportCalculatedAttribute : Attribute
    {
        public CustomEventReportCalculatedAttribute()
        {
        }
    }
    #endregion

    #region BaseCustomEventReport

    public abstract class BaseCustomEventReport
    {
        public EntityId SenderId { get; set; }
        public EntityId ReceiverId { get; set; }
    }

    #endregion

    #region Event Reports
    /// <summary>
    /// Sample Custom Event Report
    /// </summary>
    [CustomEventReport(9999)]
    public class SampleEventReport : BaseCustomEventReport
    {
        public float A;
        public int B;
    }
    #endregion

}