using System;

namespace RecordToMP3.Features.Recorder
{
    internal enum RecordingState
    {
        Paused,
        Stopped,
        Monitoring,
        Recording,
        RequestedStop
    }
}
