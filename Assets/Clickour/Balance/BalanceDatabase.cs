using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Clickour.Balance
{
    public static class BalanceDatabase
    {
        const string FILE_NAME = "clickour_balance.yaml";
        const string MAP_KEY = "map_data";

        static BalanceDocument document;

        public static bool IsLoaded => document != null;

        public static IEnumerable<string> EditableKeys
        {
            get
            {
                foreach (var key in document.Keys)
                    if (key != MAP_KEY)
                        yield return key;
            }
        }

        public static IEnumerator Load()
        {
            if (IsLoaded)
                yield break;

            var path = GetPath();
            using var request = UnityWebRequest.Get(path);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
                throw new IOException($"Balance YAML load failed: {request.error}");

            document = BalanceDocument.Parse(request.downloadHandler.text);
        }

        public static string Get(string key) => document.Get(key);

        public static float GetFloat(string key) =>
            float.Parse(document.Get(key), CultureInfo.InvariantCulture);

        public static void Set(string key, string value) => document.Set(key, value);

        public static string GetEncodedMap() => document.Get(MAP_KEY);

        public static void SetEncodedMap(string value) => document.Set(MAP_KEY, value);

        public static bool Save()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return false;
#else
            var path = GetPath();
            File.WriteAllText(path, document.ToYaml());
            return true;
#endif
        }

        static string GetPath()
        {
#if UNITY_EDITOR
            return Path.Combine(Application.dataPath, "Clickour/Balance/StreamingAssets", FILE_NAME);
#else
            return Path.Combine(Application.streamingAssetsPath, FILE_NAME);
#endif
        }

#if UNITY_INCLUDE_TESTS
        public static void LoadForTests(string yaml) => document = BalanceDocument.Parse(yaml);
#endif
    }
}
