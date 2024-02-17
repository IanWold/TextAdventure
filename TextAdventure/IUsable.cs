namespace TextAdventure;

public interface IUsable : INamable
{    Dictionary<Prop, Func<Scene, Player, string>> UseEffects { get; }
}
