using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EventRakyatData", menuName = "Heat Abnormal/Event Rakyat Data")]
public class EventRakyatData : ScriptableObject
{
    public string id;
    [TextArea]
    public string narasiPembuka;
    public List<RagamOption> ragamList = new List<RagamOption>();

    [Serializable]
    public class RagamOption
    {
        [TextArea]
        public string dialogText;
        [TextArea]
        public string aftermathText;
        public int satisfactoryMin;
        public int satisfactoryMax;
        public int kekuatanPolitikMin;
        public int kekuatanPolitikMax;
        public int waktuPembangunanMin;
        public int waktuPembangunanMax;
        public bool isInstantGameOver;
    }
}
