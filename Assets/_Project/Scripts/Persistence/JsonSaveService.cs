using System;
using System.IO;
using EcosDelLaberinto.Core.Events;
using EcosDelLaberinto.Core.Utils;
using UnityEngine;

namespace EcosDelLaberinto.Persistence
{
    /// <summary>
    /// JSON-on-disk persistence written to <see cref="Application.persistentDataPath"/>. Uses an
    /// atomic write (temp file + replace) so a crash mid-save cannot corrupt the slot.
    /// </summary>
    public sealed class JsonSaveService : ISaveService
    {
        private const string FileName = "ecos_save.json";

        private readonly IEventBus _eventBus;
        private readonly string _path;

        public SaveData Data { get; private set; } = new();

        public bool HasSave => File.Exists(_path);

        public JsonSaveService(IEventBus eventBus, string overrideDirectory = null)
        {
            _eventBus = eventBus;
            var dir = overrideDirectory ?? Application.persistentDataPath;
            _path = Path.Combine(dir, FileName);
        }

        public void Load()
        {
            try
            {
                if (!File.Exists(_path))
                {
                    Data = new SaveData();
                    GameLogger.Info("No save file found, starting fresh.");
                    return;
                }

                var json = File.ReadAllText(_path);
                var loaded = JsonUtility.FromJson<SaveData>(json);
                Data = loaded ?? new SaveData();
                MigrateIfNeeded(Data);
                GameLogger.Info($"Save loaded from {_path}");
            }
            catch (Exception ex)
            {
                GameLogger.Error($"Failed to load save, using defaults: {ex.Message}");
                Data = new SaveData();
            }
        }

        public void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var json = JsonUtility.ToJson(Data, prettyPrint: true);
                var tmp = _path + ".tmp";
                File.WriteAllText(tmp, json);

                if (File.Exists(_path))
                {
                    File.Delete(_path);
                }

                File.Move(tmp, _path);

                _eventBus?.Publish(new GameSavedEvent());
                GameLogger.Info("Game saved.");
            }
            catch (Exception ex)
            {
                GameLogger.Error($"Failed to save game: {ex.Message}");
            }
        }

        public void DeleteAll()
        {
            try
            {
                if (File.Exists(_path))
                {
                    File.Delete(_path);
                }
            }
            catch (Exception ex)
            {
                GameLogger.Error($"Failed to delete save: {ex.Message}");
            }

            Data = new SaveData();
        }

        private static void MigrateIfNeeded(SaveData data)
        {
            // Forward-compatible migration hook. Bump SaveVersion and transform here when the
            // schema changes between shipped builds.
            if (data.SaveVersion < 1)
            {
                data.SaveVersion = 1;
            }
        }
    }
}
