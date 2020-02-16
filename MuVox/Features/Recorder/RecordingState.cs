using System;

namespace TTech.MuVox.Features.Recorder
{
    public enum RecordingState
    {
        Paused,
        Stopped,
        Monitoring,
        Recording,
        RequestedStop
    }
}
