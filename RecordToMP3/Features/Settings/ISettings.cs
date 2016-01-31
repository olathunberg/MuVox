namespace RecordToMP3.Features.Settings
{
    public interface ISettings
    {
        string FILE_PATH { get; }

        bool Verify();
    }
}
