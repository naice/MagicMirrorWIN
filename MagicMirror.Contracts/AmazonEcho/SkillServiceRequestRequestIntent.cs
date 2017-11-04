using System.Collections.Generic;

namespace MagicMirror.Contracts.AmazonEcho
{
    public class SkillServiceRequestRequestIntent
    {
        public string Name { get; set; }

        public Dictionary<string, SkillServiceRequestRequestIntentSlot> Slots { get; set; } = new Dictionary<string, SkillServiceRequestRequestIntentSlot>();
    }
}
