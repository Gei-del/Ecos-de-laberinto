using System;
using System.Collections.Generic;
using EcosDelLaberinto.Core.Events;
using EcosDelLaberinto.Data;
using EcosDelLaberinto.Domain;
using EcosDelLaberinto.Persistence;
using UnityEngine;
using UnityEngine.UI;

namespace EcosDelLaberinto.UI
{
    /// <summary>
    /// Builds and drives the entire UI in code: main menu (with character select), in-game HUD,
    /// pause overlay, results screen and achievement toasts. Subscribes to the event bus so HUD
    /// values update reactively. Game-flow states call the Show* methods to switch screens.
    /// </summary>
    public sealed class UIController : MonoBehaviour
    {
        // Callbacks wired by the bootstrap / flow states.
        public Action OnPlay, OnQuit, OnResume, OnRestartLevel, OnReturnMenu, OnNextLevel, OnRetryLevel;
        public Action<string> OnSelectCharacter;

        private IEventBus _events;
        private GameDatabase _db;
        private ISaveService _save;

        private RectTransform _menu, _hud, _pause, _results, _toast;
        private Text _timeText, _deathsText, _echoesText, _crystalsText, _fragmentsText, _hintText;
        private Text _menuFragments, _menuAchievements, _resultsText, _toastText, _selectedCharText;
        private float _toastTimer;

        public void Initialize(IEventBus events, GameDatabase db, ISaveService save)
        {
            _events = events;
            _db = db;
            _save = save;

            var canvas = UIFactory.CreateCanvas("UICanvas", transform);
            BuildMenu(canvas.transform);
            BuildHud(canvas.transform);
            BuildPause(canvas.transform);
            BuildResults(canvas.transform);
            BuildToast(canvas.transform);

            Subscribe();
            ShowMenu();
        }

        private void Update()
        {
            if (_toastTimer > 0f)
            {
                _toastTimer -= Time.deltaTime;
                if (_toastTimer <= 0f && _toast != null)
                {
                    _toast.gameObject.SetActive(false);
                }
            }
        }

        // --- Screen switching --------------------------------------------------------------------

        public void ShowMenu()
        {
            SetActive(_menu, true);
            SetActive(_hud, false);
            SetActive(_pause, false);
            SetActive(_results, false);
            RefreshMenuStats();
        }

        public void ShowHud()
        {
            SetActive(_menu, false);
            SetActive(_hud, true);
            SetActive(_pause, false);
            SetActive(_results, false);
        }

        public void ShowPause()
        {
            SetActive(_pause, true);
        }

        public void HidePause()
        {
            SetActive(_pause, false);
        }

        public void ShowResults(LevelResult result)
        {
            SetActive(_results, true);
            var stars = new string('\u2605', result.Stars) + new string('\u2606', 3 - result.Stars);
            _resultsText.text =
                $"NIVEL COMPLETADO\n\n{stars}\n\n" +
                $"Tiempo: {result.TimeSeconds:0.0}s\n" +
                $"Muertes: {result.Deaths}\n" +
                $"Ecos usados: {result.EchoesUsed}\n" +
                $"Cristales: {result.CrystalsCollected}/{result.CrystalsTotal}\n" +
                $"Fragmentos: +{result.FragmentsEarned}";
        }

        public void SetHint(string hint)
        {
            if (_hintText != null)
            {
                _hintText.text = hint ?? string.Empty;
            }
        }

        public void SetSelectedCharacter(string id)
        {
            if (_selectedCharText != null)
            {
                var c = _db != null ? _db.GetCharacter(id) : null;
                _selectedCharText.text = $"Personaje: {(c != null ? c.DisplayName : id)}";
            }
        }

        // --- Builders ----------------------------------------------------------------------------

        private void BuildMenu(Transform parent)
        {
            _menu = UIFactory.CreatePanel(parent, "Menu", new Color(0.04f, 0.05f, 0.10f, 1f));

            UIFactory.CreateText(_menu, "ECOS DEL LABERINTO", 84, TextAnchor.UpperCenter, UIFactory.Accent)
                .rectTransform.anchoredPosition = new Vector2(0, -80);
            UIFactory.CreateText(_menu, "CRONOFRAGMENTOS", 44, TextAnchor.UpperCenter)
                .rectTransform.anchoredPosition = new Vector2(0, -180);

            var column = UIFactory.CreateColumn(_menu, 18f);
            UIFactory.CreateButton(column.transform, "JUGAR", () => OnPlay?.Invoke(), new Vector2(360, 70));

            _selectedCharText = UIFactory.CreateText(column.transform, "Personaje: Nova", 28);
            _selectedCharText.rectTransform.sizeDelta = new Vector2(420, 40);

            // Character select row.
            var row = new GameObject("Characters", typeof(HorizontalLayoutGroup));
            row.transform.SetParent(column.transform, false);
            var hl = row.GetComponent<HorizontalLayoutGroup>();
            hl.spacing = 12;
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childControlWidth = false;
            hl.childControlHeight = false;
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 70);

            if (_db != null)
            {
                foreach (var c in _db.Characters)
                {
                    if (c == null)
                    {
                        continue;
                    }

                    var captured = c;
                    UIFactory.CreateButton(row.transform, c.DisplayName, () => TrySelect(captured),
                        new Vector2(180, 60));
                }
            }

            _menuFragments = UIFactory.CreateText(column.transform, "Fragmentos: 0", 26);
            _menuAchievements = UIFactory.CreateText(column.transform, "Logros: 0", 26);

            UIFactory.CreateButton(column.transform, "SALIR", () => OnQuit?.Invoke(), new Vector2(360, 60));
        }

        private void BuildHud(Transform parent)
        {
            _hud = UIFactory.CreatePanel(parent, "HUD", new Color(0, 0, 0, 0));

            var bar = UIFactory.CreatePanel(_hud, "TopBar", new Color(0.05f, 0.06f, 0.12f, 0.7f));
            UIFactory.Anchor(bar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -40), new Vector2(0, 80));

            _timeText = HudLabel(bar, "Tiempo: 0.0s", new Vector2(0.02f, 0.5f), TextAnchor.MiddleLeft);
            _deathsText = HudLabel(bar, "Muertes: 0", new Vector2(0.22f, 0.5f), TextAnchor.MiddleLeft);
            _echoesText = HudLabel(bar, "Ecos: 0", new Vector2(0.40f, 0.5f), TextAnchor.MiddleLeft);
            _crystalsText = HudLabel(bar, "Cristales: 0/0", new Vector2(0.58f, 0.5f), TextAnchor.MiddleLeft);
            _fragmentsText = HudLabel(bar, "Fragmentos: 0", new Vector2(0.80f, 0.5f), TextAnchor.MiddleLeft);

            _hintText = UIFactory.CreateText(_hud, "", 28, TextAnchor.LowerCenter, UIFactory.Accent);
            UIFactory.Anchor(_hintText.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 120), new Vector2(1200, 80));

            var help = UIFactory.CreateText(_hud,
                "WASD/Flechas: mover   |   Espacio/E: interactuar   |   Shift/Q: habilidad   |   R: reiniciar (crea eco)   |   Esc: pausa",
                22, TextAnchor.LowerCenter, new Color(1, 1, 1, 0.6f));
            UIFactory.Anchor(help.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 40), new Vector2(1700, 40));
        }

        private void BuildPause(Transform parent)
        {
            _pause = UIFactory.CreatePanel(parent, "Pause", new Color(0, 0, 0, 0.75f));
            UIFactory.CreateText(_pause, "PAUSA", 64, TextAnchor.UpperCenter, UIFactory.Accent)
                .rectTransform.anchoredPosition = new Vector2(0, -160);

            var column = UIFactory.CreateColumn(_pause, 18f);
            UIFactory.CreateButton(column.transform, "CONTINUAR", () => OnResume?.Invoke(), new Vector2(360, 64));
            UIFactory.CreateButton(column.transform, "REINICIAR NIVEL", () => OnRestartLevel?.Invoke(), new Vector2(360, 64));
            UIFactory.CreateButton(column.transform, "MENU PRINCIPAL", () => OnReturnMenu?.Invoke(), new Vector2(360, 64));
            SetActive(_pause, false);
        }

        private void BuildResults(Transform parent)
        {
            _results = UIFactory.CreatePanel(parent, "Results", new Color(0.03f, 0.05f, 0.10f, 0.95f));
            _resultsText = UIFactory.CreateText(_results, "", 36, TextAnchor.UpperCenter);
            UIFactory.Anchor(_resultsText.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -120), new Vector2(900, 520));

            var column = UIFactory.CreateColumn(_results, 16f);
            column.padding = new RectOffset(0, 0, 560, 0);
            UIFactory.CreateButton(column.transform, "SIGUIENTE", () => OnNextLevel?.Invoke(), new Vector2(340, 60));
            UIFactory.CreateButton(column.transform, "REINTENTAR", () => OnRetryLevel?.Invoke(), new Vector2(340, 60));
            UIFactory.CreateButton(column.transform, "MENU", () => OnReturnMenu?.Invoke(), new Vector2(340, 60));
            SetActive(_results, false);
        }

        private void BuildToast(Transform parent)
        {
            _toast = UIFactory.CreatePanel(parent, "Toast", new Color(0.1f, 0.3f, 0.4f, 0.9f));
            UIFactory.Anchor(_toast, new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -140), new Vector2(620, 90));
            _toastText = UIFactory.CreateText(_toast, "", 28);
            SetActive(_toast, false);
        }

        private Text HudLabel(Transform parent, string content, Vector2 anchor, TextAnchor align)
        {
            var t = UIFactory.CreateText(parent, content, 28, align);
            UIFactory.Anchor(t.rectTransform, anchor, anchor, Vector2.zero, new Vector2(420, 60));
            return t;
        }

        // --- Event wiring ------------------------------------------------------------------------

        private void Subscribe()
        {
            _events.Subscribe<TimerTickedEvent>(e => _timeText.text = $"Tiempo: {e.ElapsedSeconds:0.0}s");
            _events.Subscribe<PlayerDiedEvent>(_ =>
            {
                _deathsCounter++;
                _deathsText.text = $"Muertes: {_deathsCounter}";
            });
            _events.Subscribe<LoopStartedEvent>(e => _echoesText.text = $"Ecos: {e.ActiveEchoes}");
            _events.Subscribe<CrystalCollectedEvent>(e => _crystalsText.text = $"Cristales: {e.Collected}/{e.Total}");
            _events.Subscribe<LevelLoadedEvent>(e =>
            {
                _deathsCounter = 0;
                _deathsText.text = "Muertes: 0";
                _echoesText.text = "Ecos: 0";
                _timeText.text = "Tiempo: 0.0s";
                _crystalsText.text = $"Cristales: 0/{e.CrystalsTotal}";
            });
            _events.Subscribe<FragmentsChangedEvent>(e => UpdateFragments(e.Total));
            _events.Subscribe<AchievementUnlockedEvent>(e => ShowToast($"\u2605 Logro: {e.DisplayName}"));
            _events.Subscribe<RelicUnlockedEvent>(e => ShowToast($"\u25C8 Reliquia: {e.DisplayName}"));
        }

        private int _deathsCounter;

        private void UpdateFragments(int total)
        {
            if (_fragmentsText != null) _fragmentsText.text = $"Fragmentos: {total}";
            if (_menuFragments != null) _menuFragments.text = $"Fragmentos: {total}";
        }

        private void RefreshMenuStats()
        {
            if (_save == null)
            {
                return;
            }

            UpdateFragments(_save.Data.TemporalFragments);
            if (_menuAchievements != null)
            {
                _menuAchievements.text = $"Logros: {_save.Data.UnlockedAchievements.Count}";
            }
        }

        private void TrySelect(CharacterData c)
        {
            if (_save != null && !_save.Data.IsCharacterUnlocked(c.Id) && !c.UnlockedByDefault)
            {
                ShowToast($"{c.DisplayName} bloqueado ({c.UnlockCost} fragmentos)");
                OnSelectCharacter?.Invoke(c.Id); // bootstrap decides whether to unlock/select
                return;
            }

            OnSelectCharacter?.Invoke(c.Id);
            SetSelectedCharacter(c.Id);
        }

        private void ShowToast(string message)
        {
            if (_toast == null)
            {
                return;
            }

            _toastText.text = message;
            SetActive(_toast, true);
            _toastTimer = 3f;
        }

        private static void SetActive(Component c, bool active)
        {
            if (c != null)
            {
                c.gameObject.SetActive(active);
            }
        }
    }
}
