using System;
using UnityEngine;
using UnityEngine.UI;

namespace EcosDelLaberinto.UI
{
    /// <summary>
    /// Helpers to build uGUI hierarchies entirely in code so the game ships without hand-wired
    /// scene UI. Uses the built-in legacy font so no TMP import step is required to run.
    /// </summary>
    public static class UIFactory
    {
        public static readonly Color Panel = new(0.06f, 0.07f, 0.12f, 0.95f);
        public static readonly Color Accent = new(0.35f, 0.85f, 1f, 1f);
        public static readonly Color ButtonColor = new(0.16f, 0.20f, 0.34f, 1f);

        private static Font _font;

        private static Font Font
        {
            get
            {
                if (_font == null)
                {
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (_font == null)
                    {
                        // Older Unity versions name it Arial.ttf.
                        _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    }
                }

                return _font;
            }
        }

        public static Canvas CreateCanvas(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Color? color = null)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color ?? Panel;
            var rt = go.GetComponent<RectTransform>();
            Stretch(rt);
            return rt;
        }

        public static Text CreateText(Transform parent, string content, int fontSize,
            TextAnchor anchor = TextAnchor.MiddleCenter, Color? color = null)
        {
            var go = new GameObject("Text", typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Font;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = color ?? Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Button CreateButton(Transform parent, string label, Action onClick,
            Vector2 size)
        {
            var go = new GameObject($"Button_{label}", typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = ButtonColor;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;

            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = Accent;
            colors.pressedColor = Accent * 0.8f;
            button.colors = colors;
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }

            var text = CreateText(go.transform, label, 32);
            Stretch(text.rectTransform);
            return button;
        }

        public static VerticalLayoutGroup CreateColumn(Transform parent, float spacing)
        {
            var go = new GameObject("Column", typeof(VerticalLayoutGroup));
            go.transform.SetParent(parent, false);
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
            Stretch(go.GetComponent<RectTransform>());
            return layout;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static void Anchor(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }
    }
}
