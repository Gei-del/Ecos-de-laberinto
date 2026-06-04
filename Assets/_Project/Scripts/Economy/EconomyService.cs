using EcosDelLaberinto.Core.Events;
using EcosDelLaberinto.Persistence;
using UnityEngine;

namespace EcosDelLaberinto.Economy
{
    /// <summary>
    /// Fragment wallet backed by the save data. Every mutation publishes a
    /// <see cref="FragmentsChangedEvent"/> so the HUD/store update reactively.
    /// </summary>
    public sealed class EconomyService : IEconomyService
    {
        private readonly ISaveService _save;
        private readonly IEventBus _eventBus;

        public EconomyService(ISaveService save, IEventBus eventBus)
        {
            _save = save;
            _eventBus = eventBus;
        }

        public int Fragments => _save.Data.TemporalFragments;

        public void Add(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _save.Data.TemporalFragments += amount;
            _eventBus.Publish(new FragmentsChangedEvent(_save.Data.TemporalFragments, amount));
        }

        public bool CanAfford(int amount) => _save.Data.TemporalFragments >= amount;

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || !CanAfford(amount))
            {
                return false;
            }

            _save.Data.TemporalFragments -= amount;
            _eventBus.Publish(new FragmentsChangedEvent(_save.Data.TemporalFragments, -amount));
            _save.Save();
            return true;
        }
    }
}
