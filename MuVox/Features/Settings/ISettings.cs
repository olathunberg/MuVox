namespace TTech.Muvox.Features.Settings
{
    public interface ISettings
    {
        string FILE_PATH { get; }

        bool AutoSave { get; }

        bool Verify();
    }
}
