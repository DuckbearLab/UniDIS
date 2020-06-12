using DuckbearLab.UniSim.CustomEventReports;
using DuckbearLab.UniSim.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DuckbearLab.UniSim
{
    public class CustomEventReportsManager : MonoBehaviour
    {
        public static CustomEventReportsManager Instance;

        [SerializeField] private ExerciseConnection _exerciseConnection = null;

        private Dictionary<uint, List<Action<BaseCustomEventReport>>> subscribersByEventType;

        private static Dictionary<uint, Type> eventReportsTypes;

        static CustomEventReportsManager()
        {
            eventReportsTypes = new Dictionary<uint, Type>();
            foreach (var type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            {
                var eventReportAttr = GetCustomEventreportAttribute(type);
                if (eventReportAttr != null)
                    eventReportsTypes[eventReportAttr.EventReportType] = type;
            }
        }

        void Awake()
        {
            Instance = this;
            if (null == subscribersByEventType)
                subscribersByEventType = new Dictionary<uint, List<Action<BaseCustomEventReport>>>();
        }

        void Start()
        {
            _exerciseConnection.Subscribe<EventReportPDU>(EventReportReceived);
        }

        private void OnDestroy()
        {
            _exerciseConnection.Unsubscribe<EventReportPDU>(EventReportReceived);
        }

        public void Subscribe<EventReportType>(Action<EventReportType> handler) where EventReportType : BaseCustomEventReport
        {
            if (subscribersByEventType == null)
            {
                subscribersByEventType = new Dictionary<uint, List<Action<BaseCustomEventReport>>>();
            }
            var eventReportAttr = GetCustomEventreportAttribute(typeof(EventReportType));

            if (eventReportAttr == null)
                throw new Exception("Event report type must have EventReportAttribute");

            var eventReportType = eventReportAttr.EventReportType;

            if (!subscribersByEventType.ContainsKey(eventReportType))
                subscribersByEventType[eventReportType] = new List<Action<BaseCustomEventReport>>();

            subscribersByEventType[eventReportType].Add((BaseCustomEventReport obj) => { handler((EventReportType)obj); });
        }

        private void EventReportReceived(EventReportPDU eventReportPdu)
        {
            uint eventType = eventReportPdu.EventType;
            if (subscribersByEventType.ContainsKey(eventType) && subscribersByEventType[eventType].Count > 0)
            {
                if (eventReportsTypes.ContainsKey(eventType))
                {
                    Type eventReportType = eventReportsTypes[eventType];
                    BaseCustomEventReport o = (BaseCustomEventReport)Activator.CreateInstance(eventReportType);

                    var eventReportData = new CustomEventReportEncoder(eventReportPdu, o);

                    o.SenderId = eventReportData.SenderId;
                    o.ReceiverId = eventReportData.ReceiverId;

                    foreach (var field in eventReportType.GetFields())
                    {
                        field.SetValue(o, eventReportData.Read(field.FieldType, field));
                    }

                    foreach (var subscriber in subscribersByEventType[eventType])
                        subscriber(o);
                }
            }
        }


        public void Send(BaseCustomEventReport eventReport)
        {
            var eventReportAttr = GetCustomEventreportAttribute(eventReport);

            if (eventReportAttr == null)
                throw new Exception("Event report must have CustomEventReportAttribute");

            var eventReportData = new CustomEventReportEncoder(eventReportAttr.EventReportType, eventReport);

            eventReportData.SenderId = eventReport.SenderId;
            eventReportData.ReceiverId = eventReport.ReceiverId;

            foreach (var field in eventReport.GetType().GetFields())
            {
                if (field.Name == "SenderId" || field.Name == "ReceiverId")
                {
                    continue;
                }
                var value = field.GetValue(eventReport);
                eventReportData.Write(value, field);
            }

            _exerciseConnection.SendPDU(eventReportData.EventReportPdu);
        }

        private static CustomEventReportAttribute GetCustomEventreportAttribute(object eventReport)
        {
            return GetCustomEventreportAttribute(eventReport.GetType());
        }

        private static CustomEventReportAttribute GetCustomEventreportAttribute(Type eventReportType)
        {
            var attrs = eventReportType.GetCustomAttributes(typeof(CustomEventReportAttribute), true);
            if (attrs.Length == 0) return null;
            return (CustomEventReportAttribute)attrs[0];
        }
    }
}