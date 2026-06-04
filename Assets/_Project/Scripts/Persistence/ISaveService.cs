namespace EcosDelLaberinto.Persistence
{
    /// <summary>
    /// Abstraction over persistence so gameplay never touches the file system directly and can be
    /// unit-tested with an in-memory fake.
    /// </summary>
    public interface ISaveService
    {
        SaveData Data { get; }
        void Load();
        void Save();
        void DeleteAll();
        bool HasSave { get; }
    }
}
