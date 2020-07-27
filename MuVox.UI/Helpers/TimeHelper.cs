namespace TTech.MuVox.UI.Helpers
{
    public static class TimeHelper
    {
        public static int PositionToTime(double position, int averageBytesPerSecond, int startPosition, double samplesPerPixel)
        {
            if (averageBytesPerSecond == 0)
                return 0;

            return (int)((startPosition + position * samplesPerPixel) * 2) / (averageBytesPerSecond / 10);
        }


        public static int TimeToPosition(int mark, int averageBytesPerSecond, int startPosition, double samplesPerPixel)
        {
            if (samplesPerPixel == 0)
                return 0;

            return (int)(((mark * (averageBytesPerSecond / 10) / 2) - startPosition) / samplesPerPixel);
        }
    }
}
