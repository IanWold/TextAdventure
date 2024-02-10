namespace TextAdventure;

public class Item : IUsable, IDescribable
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<Item> Items { get; set; } = [];

    public Item? BreakageReplacement { get; set; }

    public List<Item> ItemsOnBreakage =>
        BreakageReplacement is null
        ? Items
        : [..Items, BreakageReplacement];

    public Dictionary<Item, Func<Room, Player, string>> UseEffects { get; set; } = [];
}
