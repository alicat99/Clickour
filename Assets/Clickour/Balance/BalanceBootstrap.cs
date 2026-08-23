using System.Collections;
using UnityEngine;

namespace Clickour.Balance
{
    public sealed class BalanceBootstrap : MonoBehaviour
    {
        [SerializeField] GameObject content_root;

        public void Configure(GameObject content_root) => this.content_root = content_root;

        IEnumerator Start()
        {
            content_root.SetActive(false);
            yield return BalanceDatabase.Load();
            content_root.SetActive(true);
        }
    }
}
