using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EcosDelLaberinto.UIToolkit
{
    /// <summary>
    /// Navegador del sistema de UI (UI Toolkit) replicado del diseño oficial de V0.
    /// Carga las pantallas (VisualTreeAsset) en un único UIDocument y conmuta entre
    /// ellas, cableando los botones por nombre. No simplifica el diseño: solo gestiona
    /// la navegación entre Menú, Personajes, Mundos, HUD, Inventario, Tienda y Logros.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class EcosUINavigator : MonoBehaviour
    {
        public enum Screen
        {
            MainMenu,
            CharacterSelect,
            WorldSelect,
            GameHud,
            Inventory,
            Shop,
            Achievements
        }

        [Header("Pantallas (VisualTreeAsset)")]
        [SerializeField] private VisualTreeAsset mainMenu;
        [SerializeField] private VisualTreeAsset characterSelect;
        [SerializeField] private VisualTreeAsset worldSelect;
        [SerializeField] private VisualTreeAsset gameHud;
        [SerializeField] private VisualTreeAsset inventory;
        [SerializeField] private VisualTreeAsset shop;
        [SerializeField] private VisualTreeAsset achievements;

        [Header("Inicio")]
        [SerializeField] private Screen startScreen = Screen.MainMenu;
        [Tooltip("Permite conmutar de pantalla con las teclas 1-7 (útil para revisar el diseño).")]
        [SerializeField] private bool debugHotkeys = true;

        private UIDocument _document;
        private readonly Dictionary<Screen, VisualTreeAsset> _screens = new();
        private Screen _current;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            Register(Screen.MainMenu, mainMenu);
            Register(Screen.CharacterSelect, characterSelect);
            Register(Screen.WorldSelect, worldSelect);
            Register(Screen.GameHud, gameHud);
            Register(Screen.Inventory, inventory);
            Register(Screen.Shop, shop);
            Register(Screen.Achievements, achievements);
        }

        private void OnEnable() => Show(startScreen);

        private void Register(Screen screen, VisualTreeAsset asset)
        {
            if (asset != null)
            {
                _screens[screen] = asset;
            }
        }

        public void Show(Screen screen)
        {
            if (_document == null)
            {
                return;
            }

            if (!_screens.TryGetValue(screen, out VisualTreeAsset asset) || asset == null)
            {
                Debug.LogWarning($"[EcosUINavigator] Pantalla no asignada: {screen}");
                return;
            }

            VisualElement root = _document.rootVisualElement;
            if (root == null)
            {
                return;
            }

            root.Clear();
            asset.CloneTree(root);

            // Las pantallas usan position:absolute para llenar el panel.
            VisualElement screenRoot = root.childCount > 0 ? root[0] : root;
            screenRoot.style.position = Position.Absolute;
            screenRoot.style.left = 0;
            screenRoot.style.top = 0;
            screenRoot.style.right = 0;
            screenRoot.style.bottom = 0;

            _current = screen;
            WireNavigation(root, screen);
        }

        private void WireNavigation(VisualElement root, Screen screen)
        {
            // Botón "VOLVER" presente en la mayoría de pantallas.
            Bind(root, "BtnBack", () => Show(Screen.MainMenu));

            switch (screen)
            {
                case Screen.MainMenu:
                    Bind(root, "BtnNewGame", () => Show(Screen.GameHud));
                    Bind(root, "BtnContinue", () => Show(Screen.GameHud));
                    Bind(root, "BtnCharacters", () => Show(Screen.CharacterSelect));
                    Bind(root, "BtnWorlds", () => Show(Screen.WorldSelect));
                    Bind(root, "BtnInventory", () => Show(Screen.Inventory));
                    Bind(root, "BtnShop", () => Show(Screen.Shop));
                    Bind(root, "BtnAchievements", () => Show(Screen.Achievements));
                    Bind(root, "BtnQuit", Quit);
                    break;

                case Screen.CharacterSelect:
                    Bind(root, "BtnSelect", () => Show(Screen.WorldSelect));
                    break;

                case Screen.WorldSelect:
                    Bind(root, "World1", () => Show(Screen.GameHud));
                    Bind(root, "World2", () => Show(Screen.GameHud));
                    Bind(root, "World3", () => Show(Screen.GameHud));
                    break;

                case Screen.GameHud:
                    Bind(root, "BtnPause", () => Show(Screen.MainMenu));
                    break;
            }
        }

        private static void Bind(VisualElement root, string elementName, System.Action action)
        {
            VisualElement element = root.Q(elementName);
            if (element is Button button)
            {
                button.clicked += action;
            }
            else if (element != null)
            {
                element.RegisterCallback<ClickEvent>(_ => action());
            }
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void Update()
        {
            if (!debugHotkeys)
            {
                return;
            }

            for (int i = 0; i < 7; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    Show((Screen)i);
                }
            }
        }
    }
}
