namespace TTech.MuVox.Features.Settings
{
    public interface ISettings
    {
        string FILE_PATH { get; }

        bool AutoSave { get; }

        bool Verify();
    }
}
