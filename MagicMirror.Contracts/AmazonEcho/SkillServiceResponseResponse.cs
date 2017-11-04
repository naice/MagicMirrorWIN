namespace MagicMirror.Contracts.AmazonEcho
{
    public class SkillServiceResponseResponse
    {
        public SkillServiceResponseResponseOutputSpeech OutputSpeech { get; set; } = new SkillServiceResponseResponseOutputSpeech();

        public bool ShouldEndSession { get; set; } = true;
    }
}
