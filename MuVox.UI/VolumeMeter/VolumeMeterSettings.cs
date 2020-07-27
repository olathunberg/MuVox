using System.ComponentModel;

namespace TTech.MuVox.UI.VolumeMeter
{
    public class VolumeMeterSettings
    {
        private const string UX = "UX";
  
        [Category(UX)]
        [DisplayName("Volumemeter, Peekmark fallbackspeed")]
        public int UX_VolumeMeter_PeakMarkFallBackSpeed { get; set; } = 2;

        [Category(UX)]
        [DisplayName("Volumemeter, Peekmark holdtime (ms)")]
        public int UX_VolumeMeter_PeakMarkHoldTime { get; set; } = 500;

        [Category(UX)]
        [DisplayName("Volumemeter, MinDb")]
        public float UX_VolumeMeter_MinDb { get; set; } = -24;

        [Category(UX)]
        [DisplayName("Volumemeter, MaxDb")]
        public float UX_VolumeMeter_MaxDb { get; set; } = 2;

        [Category(UX)]
        [DisplayName("Volumemeter, Num of samples")]
        public byte UX_VolumeMeter_NoSamples { get; set; } = 8;
    }
}
