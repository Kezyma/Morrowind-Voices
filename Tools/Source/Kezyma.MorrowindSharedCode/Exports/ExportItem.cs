namespace Kezyma.MorrowindSharedCode.Exports
{
    public class ExportItem
    {
        public int? Done { get; set; }
        public string InfoId { get; set; }
        public string SpeakerId { get; set; }
        public string SpeakerName { get; set; }
        public string SpeakerRace { get; set; }
        public string SpeakerGender { get; set; }
        public string SpeakerFaction { get; set; }
        public string SpeakerClass { get; set; }
        public string SpeakerRank { get; set; }
        public string SpeakerCell { get; set; }
        public string PCRankId { get; set; }
        public string PCRank { get; set; }
        public string NextPCRank { get; set; }
        public string PCFaction { get; set; }
        public string DialogueType { get; set; }
        public string DialogueText { get; set; }
        public string Source { get; set; }

        public ExportItem Clone()
        {
            return new ExportItem
            {
                Done = Done,
                InfoId = InfoId,
                SpeakerId = SpeakerId,
                SpeakerName = SpeakerName,
                SpeakerRace = SpeakerRace,
                SpeakerGender = SpeakerGender,
                SpeakerFaction = SpeakerFaction,
                SpeakerClass = SpeakerClass,
                SpeakerRank = SpeakerRank,
                SpeakerCell = SpeakerCell,
                PCRank = PCRank,
                PCFaction = PCFaction,
                DialogueType = DialogueType,
                DialogueText = DialogueText,
                Source = Source
            };
        }
    }
}