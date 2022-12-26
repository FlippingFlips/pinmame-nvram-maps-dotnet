namespace PinMAME.NvMaps.Model
{
    /// <summary>
    /// Standard game adjustment model
    /// </summary>
    public class GameAdjustment
    {
        /// <summary>
        /// short_label = BPG
        /// </summary>
        public int BallsPerGame { get; set; }
        /// <summary>
        /// short_label = MEB
        /// </summary>
        public int MaxExtraBall { get; set; }
        /// <summary>
        /// short_label = TW
        /// </summary>
        public int TiltWarnings { get; set; }
        public int BallSaveTime { get; set; }
    }
}