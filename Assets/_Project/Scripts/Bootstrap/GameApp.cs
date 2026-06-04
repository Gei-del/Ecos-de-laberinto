using EcosDelLaberinto.Audio;
using EcosDelLaberinto.Core.DI;
using EcosDelLaberinto.Core.Events;
using EcosDelLaberinto.Core.StateMachine;
using EcosDelLaberinto.Core.Utils;
using EcosDelLaberinto.Data;
using EcosDelLaberinto.Domain;
using EcosDelLaberinto.Economy;
using EcosDelLaberinto.Gameplay.Level;
using EcosDelLaberinto.Gameplay.Player;
using EcosDelLaberinto.Gameplay.Time;
using EcosDelLaberinto.Input;
using EcosDelLaberinto.Persistence;
using EcosDelLaberinto.Scoring;
using EcosDelLaberinto.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EcosDelLaberinto.Bootstrap
{
    /// <summary>
    /// Composition root. Wires the dependency-injection container, builds every service, the UI and
    /// the level session, then drives the game-flow state machine each frame. The whole game boots
    /// from this single component dropped on an empty scene — no hand-wired scene graph required.
    ///
    /// Assign a <see cref="GameDatabase"/> asset for shipping content; if left empty it falls back
    /// to <see cref="DefaultContent"/> so the project is playable straight after import.
    /// </summary>
    public sealed class GameApp : MonoBehaviour
    {
        [Tooltip("Optional. When empty, a full database is generated in code (DefaultContent).")]
        [SerializeField] private GameDatabase _database;
        [SerializeField] private AudioLibrary _audioLibrary;

        private IServiceContainer _container;

        public IEventBus Events { get; private set; }
        public ITickService Tick { get; private set; }
        public IInputService Input { get; private set; }
        public ISaveService Save { get; private set; }
        public IEconomyService Economy { get; private set; }
        public GameConfig Config { get; private set; }
        public GameDatabase Database { get; private set; }
        public UIController UI { get; private set; }
        public LevelSession Session { get; private set; }

        public StateMachine Flow { get; private set; }
        public MenuState Menu { get; private set; }
        public PlayingState Playing { get; private set; }
        public PausedState Paused { get; private set; }
        public ResultsState Results { get; private set; }

        public int CurrentLevelIndex { get; private set; }
        public string SelectedCharacterId { get; private set; } = "nova";
        public LevelResult LastResult { get; private set; }

        /// <summary>
        /// Zero-config boot: if no scene already contains a <see cref="GameApp"/>, one is spawned
        /// automatically after the first scene loads. This means the game runs by pressing Play on
        /// an empty scene, while a designer-configured GameApp (with a database asset) still wins.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBoot()
        {
            if (FindFirstObjectByType<GameApp>() != null)
            {
                return;
            }

            var go = new GameObject("GameApp");
            go.AddComponent<GameApp>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            BuildDatabase();
            EnsureSceneInfrastructure();
            BuildServices();
            BuildUI();
            WireUiCallbacks();
            BuildFlow();
        }

        private void Update()
        {
            Input.Poll();
            Flow.Tick(UnityEngine.Time.deltaTime);
        }

        // --- Composition ----------------------------------------------------------------------

        private void BuildDatabase()
        {
            Database = _database != null ? _database : DefaultContent.Build();
            Database.BuildIndices();
            Config = Database.Config != null ? Database.Config : DefaultContent.BuildConfig();
        }

        private void BuildServices()
        {
            _container = new ServiceContainer();

            Events = new EventBus();
            Save = new JsonSaveService(Events);
            Save.Load();
            SelectedCharacterId = Save.Data.SelectedCharacterId;

            Tick = new TickService(Config);
            Input = new UnityInputService();
            Economy = new EconomyService(Save, Events);
            var score = new ScoreService(Config);
            var abilityFactory = new AbilityFactory(Config);

            var audio = BuildAudio();
            var achievements = new AchievementService(Events, Save, Database);

            var sessionRoot = new GameObject("LevelRoot").transform;
            sessionRoot.SetParent(transform, false);
            Session = new LevelSession(Input, Tick, Events, score, Save, Economy, Config,
                abilityFactory, sessionRoot);

            // Capture results so the flow can show them without coupling to the session internals.
            Events.Subscribe<LevelCompletedEvent>(e => LastResult = e.Result);

            Tick.Ticked += Session.OnTick;

            _container.RegisterInstance(Events);
            _container.RegisterInstance(Tick);
            _container.RegisterInstance(Input);
            _container.RegisterInstance(Save);
            _container.RegisterInstance(Economy);
            _container.RegisterInstance<IScoreService>(score);
            _container.RegisterInstance<IAudioService>(audio);
            _container.RegisterInstance(Config);
            _container.RegisterInstance(Database);

            // Touch the achievement service so the analyser never flags it as unused; it stays
            // alive through its event subscriptions.
            GameLogger.Info($"Boot complete. Achievements ready: {achievements != null}.");
        }

        private IAudioService BuildAudio()
        {
            var music = gameObject.AddComponent<AudioSource>();
            var sfx = gameObject.AddComponent<AudioSource>();
            var library = _audioLibrary != null
                ? _audioLibrary
                : ScriptableObject.CreateInstance<AudioLibrary>();
            return new AudioService(music, sfx, library, Events);
        }

        private void BuildUI()
        {
            var uiGo = new GameObject("UI");
            uiGo.transform.SetParent(transform, false);
            UI = uiGo.AddComponent<UIController>();
            UI.Initialize(Events, Database, Save);
            UI.SetSelectedCharacter(SelectedCharacterId);
        }

        private void WireUiCallbacks()
        {
            UI.OnPlay = () => { StartLevel(CurrentLevelIndex); Flow.ChangeState(Playing); };
            UI.OnQuit = QuitGame;
            UI.OnResume = () => Flow.ChangeState(Playing);
            UI.OnRestartLevel = () => { StartLevel(CurrentLevelIndex); Flow.ChangeState(Playing); };
            UI.OnReturnMenu = () => { Session.Dispose(); Flow.ChangeState(Menu); };
            UI.OnNextLevel = () =>
            {
                CurrentLevelIndex = Mathf.Min(CurrentLevelIndex + 1, Database.Levels.Count - 1);
                StartLevel(CurrentLevelIndex);
                Flow.ChangeState(Playing);
            };
            UI.OnRetryLevel = () => { StartLevel(CurrentLevelIndex); Flow.ChangeState(Playing); };
            UI.OnSelectCharacter = SelectCharacter;
        }

        private void BuildFlow()
        {
            Flow = new StateMachine();
            Menu = new MenuState(this);
            Playing = new PlayingState(this);
            Paused = new PausedState(this);
            Results = new ResultsState(this);
            Flow.ChangeState(Menu);
        }

        // --- Flow helpers ---------------------------------------------------------------------

        private void StartLevel(int index)
        {
            var level = Database.GetLevelByOrder(index);
            if (level == null)
            {
                GameLogger.Error($"No level at index {index}.");
                return;
            }

            var character = Database.GetCharacter(SelectedCharacterId);
            UI.SetHint(level.TutorialHint);
            Session.StartLevel(level, character);
        }

        private void SelectCharacter(string id)
        {
            var character = Database.GetCharacter(id);
            if (character == null)
            {
                return;
            }

            if (!Save.Data.IsCharacterUnlocked(id))
            {
                if (character.UnlockedByDefault)
                {
                    Save.Data.UnlockedCharacters.Add(id);
                }
                else if (Economy.TrySpend(character.UnlockCost))
                {
                    Save.Data.UnlockedCharacters.Add(id);
                }
                else
                {
                    return; // not enough fragments; keep current selection.
                }
            }

            SelectedCharacterId = id;
            Save.Data.SelectedCharacterId = id;
            Save.Save();
            UI.SetSelectedCharacter(id);
        }

        private static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // --- Scene infrastructure -------------------------------------------------------------

        private void EnsureSceneInfrastructure()
        {
            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                var cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                cam.transform.position = new Vector3(0, 0, -10f);
            }

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem),
                    typeof(StandaloneInputModule));
                es.transform.SetParent(transform, false);
            }
        }
    }
}
