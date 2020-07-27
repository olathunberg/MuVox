using System;
using System.ComponentModel;

namespace TTech.MuVox.UI.VolumeMeter
{
    [Serializable]
    public class VolumeMeterSettings
    { 
        [DisplayName("Peekmark fallbackspeed")]
        public int UX_VolumeMeter_PeakMarkFallBackSpeed { get; set; } = 2;

        [DisplayName("Peekmark holdtime (ms)")]
        public int UX_VolumeMeter_PeakMarkHoldTime { get; set; } = 500;

        [DisplayName("MinDb")]
        public float UX_VolumeMeter_MinDb { get; set; } = -24;

        [DisplayName("MaxDb")]
        public float UX_VolumeMeter_MaxDb { get; set; } = 2;

        [DisplayName("Num of samples")]
        public byte UX_VolumeMeter_NoSamples { get; set; } = 8;

        public override string ToString()
        {
            return "Volumemeters";
        }
    }
}
