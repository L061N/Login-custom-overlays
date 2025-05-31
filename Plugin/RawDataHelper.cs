using GameReaderCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace benofficial2.Plugin
{
    public static class RawDataHelper
    {
        public static bool TryGetSessionData<T>(ref GameData data, out T result, params object[] path)
        {
            result = default;
            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null)
                return false;

            try
            {
                if (raw.AllSessionData is IDictionary<object, object> allSessionData)
                {
                    return TryGetValue<T>(allSessionData, out result, path);
                }
            }
            catch { Debug.Assert(false); }
            return false;
        }

        public static bool TryGetTelemetryData<T>(ref GameData data, out T result, params object[] path)
        {
            result = default;
            dynamic raw = data.NewData.GetRawDataObject();
            if (raw == null)
                return false;

            try
            {
                if (raw.Telemetry is IDictionary<object, object> telemetry)
                {
                    return TryGetValue<T>(telemetry, out result, path);
                }
            }
            catch { Debug.Assert(false); }
            return false;
        }

        public static bool TryGetValue<T>(object root, out T result, params object[] path)
        {
            result = default;

            if (root == null || path == null || path.Length == 0)
                return false;

            object current = root;

            foreach (var key in path)
            {
                if (current is Dictionary<object, object> dict)
                {
                    if (!dict.TryGetValue(key, out current))
                        return false;
                }
                else if (current is List<object> list && key is int index)
                {
                    if (index < 0 || index >= list.Count)
                        return false;
                    current = list[index];
                }
                else
                {
                    return false;
                }
            }

            if (current is T casted)
            {
                result = casted;
                return true;
            }

            try
            {
                result = (T)Convert.ChangeType(current, typeof(T));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
