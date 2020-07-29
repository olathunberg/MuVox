using System;
using System.ComponentModel;

namespace TTech.MuVox.UI.VolumeMeter
{
    [Serializable]
    public class VolumeMeterSettings
    { 
        [DisplayName("Peekmark holdtime (ms)")]
        public int PeakHoldTime { get; set; } = 100;

        [DisplayName("MinDb")]
        public float MinDb { get; set; } = -39;

        [DisplayName("MaxDb")]
        public float MaxDb { get; set; } = 0;

        public override string ToString()
        {
            return "Volumemeters";
        }
    }
}
