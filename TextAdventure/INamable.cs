namespace TextAdventure;

public interface INamable
{
    string Name { get; }
    string DisambiguatingName => Name;
}
