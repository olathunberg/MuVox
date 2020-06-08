using System.Collections.Generic;

namespace TTech.MuVox.Features.Settings
{
    public interface ISettings
    {
        string FILE_PATH { get; }

        bool AutoSave { get; }

        IEnumerable<string> Verify();
    }
}
