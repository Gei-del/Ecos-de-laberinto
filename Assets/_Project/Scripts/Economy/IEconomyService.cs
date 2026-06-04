namespace EcosDelLaberinto.Economy
{
    /// <summary>Manages the single soft currency: Temporal Fragments. No pay-to-win, cosmetics only.</summary>
    public interface IEconomyService
    {
        int Fragments { get; }
        void Add(int amount);
        bool TrySpend(int amount);
        bool CanAfford(int amount);
    }
}
