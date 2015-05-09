using System;

namespace RecordToMP3.Features.Recorder
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
