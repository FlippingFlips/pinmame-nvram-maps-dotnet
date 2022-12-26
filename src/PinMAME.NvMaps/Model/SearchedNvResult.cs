namespace PinMAME.NvMaps.Model
{
    public class SearchedNvResult
    {
        public SearchedNvResult(int offset, int length, string searchVal)
        {
            Offset = offset;
            Length = length;
            SearchVal = searchVal;
        }

        /// <summary>
        /// Index value was found
        /// </summary>
        public int Offset { get; }
        /// <summary>
        /// Length, given by search value
        /// </summary>
        public int Length { get; }
        /// <summary>
        /// The value used on the search
        /// </summary>
        public string SearchVal { get; }
    }
}
