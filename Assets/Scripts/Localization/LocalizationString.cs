using System;
using UnityEngine;

namespace Localization
{
    [Serializable]
    public class LocalizationString
    {
        public const string LOCAL_NULL_DATA = "local_null_data";
        [SerializeField]
        public int LocalizationName;

        public override string ToString()
        {
            return LocalizationManager.GetData(LocalizationName);
        }
    }
}
