namespace Kezyma.MorrowindSharedCode.Voice
{
    public class VoiceListItem
    {
        public string voice_id { get; set; }
        public string name { get; set; }
        public VoiceListSamplesItem[] samples { get; set; }
        public string category { get; set; }
        public string preview_url { get; set; }
        public string[] available_for_tiers { get; set; }
        public object settings { get; set; }
    }
}