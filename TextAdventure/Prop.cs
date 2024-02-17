namespace TextAdventure;

public class Prop : IUsable, IDescribable
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<Prop> Props { get; set; } = [];

    public Prop? BreakageReplacement { get; set; }

    public List<Prop> PropsOnBreakage =>
        BreakageReplacement is null
        ? Props
        : [..Props, BreakageReplacement];

    public Dictionary<Prop, Func<Scene, Player, string>> UseEffects { get; set; } = [];
}
