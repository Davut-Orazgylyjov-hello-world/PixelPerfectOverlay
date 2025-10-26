using UnityEditor;
using UnityEngine;

namespace OrazgylyjovFuteres.PixelPerfectOverlay.Editor
{
    public class PixelPerfectOverlayWindow : EditorWindow
    {
        private PixelPerfectSettings settings;

        [MenuItem("Tools/Pixel Perfect Overlay")]
        public static void OpenWindow()
        {
            var window = GetWindow<PixelPerfectOverlayWindow>("Pixel Perfect Overlay");
            window.minSize = new Vector2(340, 180);
        }

        private void OnEnable()
        {
            settings = new PixelPerfectSettings();
            settings.Load();

            // Инициализируем Canvas Overlay
            PixelPerfectOverlayCanvas.SetSettings(settings);
        }

        private void OnDisable()
        {
            settings.Save();
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawTextureField();
            DrawAlphaSlider();
            DrawDisplayOptions();
            DrawPixelPerfectToggle();
            DrawActionButtons();
            DrawFooter();

            // Передаём настройки Canvas
            PixelPerfectOverlayCanvas.SetSettings(settings);
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Pixel Perfect Overlay — Editor Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }

        private void DrawTextureField()
        {
            EditorGUI.BeginChangeCheck();
            settings.overlayTexture = (Texture2D) EditorGUILayout.ObjectField(
                new GUIContent("Overlay Texture", "Mockup image (PNG) to overlay in editor"),
                settings.overlayTexture,
                typeof(Texture2D),
                false);

            if (EditorGUI.EndChangeCheck())
            {
                settings.SaveTexturePath();
                PixelPerfectOverlayCanvas.SetSettings(settings);
            }
        }

        private void DrawAlphaSlider()
        {
            float newAlpha = EditorGUILayout.Slider(
                new GUIContent("Overlay Alpha", "Transparency of overlay image (0 = invisible, 1 = opaque)"),
                settings.alpha, 0f, 1f);

            if (!Mathf.Approximately(newAlpha, settings.alpha))
            {
                settings.alpha = newAlpha;
                PixelPerfectOverlayCanvas.SetSettings(settings);
            }
        }

        private void DrawDisplayOptions()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Display Options", EditorStyles.boldLabel);

            bool newShowScene = EditorGUILayout.Toggle(
                new GUIContent("Show in Scene View", "Display overlay in Scene View"), settings.showInScene);
            bool newShowGame = EditorGUILayout.Toggle(
                new GUIContent("Show in Game View", "Display overlay in Game View (editor only)"), settings.showInGame);

            if (newShowScene != settings.showInScene || newShowGame != settings.showInGame)
            {
                settings.showInScene = newShowScene;
                settings.showInGame = newShowGame;
                PixelPerfectOverlayCanvas.SetSettings(settings);
            }
        }

        private void DrawPixelPerfectToggle()
        {
            settings.enablePixelPerfect = EditorGUILayout.Toggle(
                new GUIContent("Enable Pixel Perfect on Canvases",
                    "Set Canvas.pixelPerfect = true on all Canvas components in the scene"),
                settings.enablePixelPerfect);
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Apply (preview)"))
            {
                ShowNotification(new GUIContent("Preview applied. Overlay is visible in Scene/Game view."));
            }

            if (GUILayout.Button("Revert"))
            {
                RemoveNotification();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Overlay now uses Editor-only Canvas in Screen Space – Overlay.\n" +
                "Alpha, visibility and texture can be updated in real-time.",
                MessageType.Info);
        }
    }
}