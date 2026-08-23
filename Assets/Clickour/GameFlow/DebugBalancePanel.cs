using System;
using System.Collections.Generic;
using Clickour.Balance;
using TMPro;
using UnityEngine;

namespace Clickour.GameFlow
{
    public sealed class DebugBalancePanel : MonoBehaviour
    {
        [Serializable]
        public sealed class Row
        {
            public string Key;
            public TMP_InputField Input;
        }

        [SerializeField] List<Row> rows = new();
        [SerializeField] TMP_Text save_status;

        public void Configure(List<Row> rows, TMP_Text save_status)
        {
            this.rows = rows;
            this.save_status = save_status;
        }

        void OnEnable()
        {
            foreach (var row in rows)
            {
                row.Input.SetTextWithoutNotify(BalanceDatabase.Get(row.Key));
                var key = row.Key;
                row.Input.onEndEdit.AddListener(value => Apply(key, value));
            }
        }

        void Apply(string key, string value)
        {
            BalanceDatabase.Set(key, value);
            save_status.text = BalanceDatabase.Save()
                ? "YAML saved"
                : "WebGL session value applied";
        }
    }
}
