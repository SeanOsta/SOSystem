using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Events
{
    public static class EventLoggingUtility
    {
        public static StringBuilder GetEventLogs<T>(string aEventName, List<T> aEventListeners, params object[] aParameters) where T : IDebugListener
        {
            StringBuilder output = new StringBuilder();
            if (aEventListeners == null)
            {
                Debug.LogError("Event listeners are null! Cannot get logs");
                return output;
            }

            output.AppendLine($"<b>[{aEventName}]</b> is being raised with {aEventListeners.Count} listeners in ascending order");

            if (aParameters != null && aParameters.Length != 0)
            {
                for (int i = 0; i < aParameters.Length; i++)
                {
                    string parameterValueAsString = aParameters[i] != null ? aParameters[i].ToString() : "NULL";
                    output.AppendLine($"<b>[Parameter {(i + 1)}]</b> {parameterValueAsString}");
                }
                output.AppendLine();
            }
            else
                output.AppendLine();

            for(int i = aEventListeners.Count - 1; i >= 0; i--)
            {
                IDebugListener listener = aEventListeners[i];
                if (listener == null)
                {
                    Debug.LogError("Debug listener is null!");
                    continue;
                }

                output.AppendLine($"<b>[Listener {aEventListeners.Count - i}] </b>");
                listener.PopulateLogs(output);
                output.AppendLine();
            }

            return output;
        }

        public static void PopulateUnityEventLogs(StringBuilder aBuilder, UnityEventBase aEvent)
        {
            if (aEvent == null)
            {
                Debug.LogError("Unity event is null! Cannot get logs");
                return;
            }

            int listenerCount = aEvent.GetPersistentEventCount();
            for (int i = 0; i < listenerCount; i++)
            {
                UnityEngine.Object persistentTarget = aEvent.GetPersistentTarget(i);
                if (persistentTarget == null)
                    continue;
                
                aBuilder.AppendLine($"<b>[Unity Editor Event]</b>  Target: {persistentTarget.name} | Method: {aEvent.GetPersistentMethodName(i)}");
            }
        }

        public static void PopulateDelegateArrayLogs(StringBuilder aBuilder, Delegate[] aDelegates)
        {
            if (aDelegates == null)
            {
                Debug.LogError("Delegate array is null! Cannot get logs");
                return;
            }

            for (int i = 0; i < aDelegates.Length; i++)
            {
                Delegate listener = aDelegates[i];

                aBuilder.AppendLine($"<b>[Internal Event]</b> Target: {listener.Target.ToString()} | Method: {listener.Method.ToString()}");
            }
        }
    }
}
