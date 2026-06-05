using System.IO;
using EcosDelLaberinto.UIToolkit;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace EcosDelLaberinto.Editor
{
    /// <summary>
    /// Genera todo lo necesario para ver el sistema de UI (UI Toolkit) replicado de V0:
    /// el tema de runtime (.tss), un PanelSettings y una escena con UIDocument + navegador
    /// ya cableado a las 7 pantallas. Un solo menú deja el diseño listo para pulsar Play.
    /// </summary>
    public static class UIToolkitSetup
    {
        private const string UiRoot = "Assets/_Project/UIToolkit";
        private const string ScreensRoot = UiRoot + "/Screens";
        private const string ThemeTss = UiRoot + "/Theme/EcosRuntimeTheme.tss";
        private const string PanelAsset = UiRoot + "/EcosPanelSettings.asset";
        private const string ScenePath = "Assets/_Project/Scenes/EcosUI.unity";

        [MenuItem("Ecos/Create UI Toolkit Showcase Scene")]
        public static void CreateShowcase()
        {
            ThemeStyleSheet theme = EnsureRuntimeTheme();
            PanelSettings panel = EnsurePanelSettings(theme);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.027f, 0.05f, 1f);
            cam.orthographic = true;
            camGo.tag = "MainCamera";

            var uiGo = new GameObject("EcosUI");
            UIDocument document = uiGo.AddComponent<UIDocument>();
            document.panelSettings = panel;

            VisualTreeAsset Load(string file) =>
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{ScreensRoot}/{file}");

            VisualTreeAsset mainMenu = Load("MainMenu.uxml");
            document.visualTreeAsset = mainMenu;

            EcosUINavigator navigator = uiGo.AddComponent<EcosUINavigator>();
            var so = new SerializedObject(navigator);
            AssignScreen(so, "mainMenu", mainMenu);
            AssignScreen(so, "characterSelect", Load("CharacterSelect.uxml"));
            AssignScreen(so, "worldSelect", Load("WorldSelect.uxml"));
            AssignScreen(so, "gameHud", Load("GameHud.uxml"));
            AssignScreen(so, "inventory", Load("Inventory.uxml"));
            AssignScreen(so, "shop", Load("Shop.uxml"));
            AssignScreen(so, "achievements", Load("Achievements.uxml"));
            so.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath)!);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuild(ScenePath);

            EditorUtility.DisplayDialog(
                "Ecos · UI Toolkit",
                "Escena creada en\n" + ScenePath +
                "\n\nPulsa Play para ver el menú. Teclas 1-7 conmutan entre pantallas.",
                "OK");
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(ScenePath));
        }

        private static void AssignScreen(SerializedObject so, string field, VisualTreeAsset asset)
        {
            SerializedProperty prop = so.FindProperty(field);
            if (prop != null)
            {
                prop.objectReferenceValue = asset;
            }
            else
            {
                Debug.LogWarning($"[UIToolkitSetup] Campo no encontrado: {field}");
            }
        }

        private static ThemeStyleSheet EnsureRuntimeTheme()
        {
            if (!File.Exists(ThemeTss))
            {
                const string content =
                    "@import url(\"unity-theme://default\");\n" +
                    "@import url(\"project://database/Assets/_Project/UIToolkit/Theme/EcosTheme.uss\");\n";
                File.WriteAllText(ThemeTss, content);
                AssetDatabase.ImportAsset(ThemeTss, ImportAssetOptions.ForceSynchronousImport);
            }

            var theme = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemeTss);
            if (theme == null)
            {
                Debug.LogWarning("[UIToolkitSetup] No se pudo cargar el ThemeStyleSheet. " +
                                 "Crea uno con click derecho → Create → UI Toolkit → TSS Theme File.");
            }
            return theme;
        }

        private static PanelSettings EnsurePanelSettings(ThemeStyleSheet theme)
        {
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelAsset);
            if (panel == null)
            {
                panel = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(panel, PanelAsset);
            }

            if (theme != null)
            {
                panel.themeStyleSheet = theme;
            }
            panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panel.referenceResolution = new Vector2Int(1920, 1080);
            panel.match = 0.5f;

            EditorUtility.SetDirty(panel);
            AssetDatabase.SaveAssets();
            return panel;
        }

        private static void AddSceneToBuild(string path)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!scenes.Exists(s => s.path == path))
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}
