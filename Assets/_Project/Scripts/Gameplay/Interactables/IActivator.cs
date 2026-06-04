namespace EcosDelLaberinto.Gameplay.Interactables
{
    /// <summary>Something that can be "on" or "off" and drive doors (button, switch, pressure pad).</summary>
    public interface IActivator
    {
        string Id { get; }
        bool IsActive { get; }
    }
}
