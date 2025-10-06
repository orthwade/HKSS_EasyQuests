using System.Collections;
using UnityEngine;

namespace owd.EasyQuests
{
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;

        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("EasyQuestsCoroutineRunner");
                    _instance = go.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public void RunCoroutine(IEnumerator routine)
        {
            StartCoroutine(routine);
        }
    }
}