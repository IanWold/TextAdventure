namespace TextAdventure;

public class SceneConnection : IUsable, IDescribable
{
    public required Scene To { get; set; }

    public string Name { get; set; } = "Passageway";

    public string DisambiguatingName { get; set; } = "Passageway";

    public string Description { get; set; } = string.Empty;

    public bool IsImpassable { get; set; }

    public string ImpassableMessage  { get; set; } = string.Empty;

    public Dictionary<Prop, Func<Scene, Player, string>> UseEffects { get; set; } = [];
}
