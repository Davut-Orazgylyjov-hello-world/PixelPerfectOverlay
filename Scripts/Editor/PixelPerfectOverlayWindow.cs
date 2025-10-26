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
            if (settings.overlayTextures == null)
                settings.overlayTextures = new Texture2D[0];

            EditorGUILayout.LabelField("Overlay Textures", EditorStyles.boldLabel);

            int newSize = Mathf.Max(0, EditorGUILayout.IntField("Number of Textures", settings.overlayTextures.Length));
            if (newSize != settings.overlayTextures.Length)
            {
                Texture2D[] newArray = new Texture2D[newSize];
                for (int i = 0; i < Mathf.Min(newSize, settings.overlayTextures.Length); i++)
                    newArray[i] = settings.overlayTextures[i];
                settings.overlayTextures = newArray;
            }

            for (int i = 0; i < settings.overlayTextures.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                settings.overlayTextures[i] = (Texture2D) EditorGUILayout.ObjectField(
                    settings.overlayTextures[i],
                    typeof(Texture2D),
                    false,
                    GUILayout.Width(60),
                    GUILayout.Height(60)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    settings.SaveTexturePath();
                    PixelPerfectOverlayCanvas.SetSettings(settings);
                }

                // Рамка вокруг текущей активной текстуры
                if (i == settings.currentIndex && settings.CurrentTexture() != null)
                {
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    lastRect.x -= 2;
                    lastRect.y -= 2;
                    lastRect.width += 4;
                    lastRect.height += 4;
                    Color green = Color.green;
                    green.a = 0.5f;
                    EditorGUI.DrawRect(lastRect, green);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            if (settings.CurrentTexture() != null)
                EditorGUILayout.LabelField($"Current: {settings.CurrentTexture().name}");
            else
                EditorGUILayout.LabelField("Current: None");
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
                new GUIContent("Show", "Display overlay in View"), settings.show);

            if (newShowScene != settings.show)
            {
                settings.show = newShowScene;
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

            // Включение/выключение overlay
            string toggleLabel = settings.show ? "Disable Overlay" : "Enable Overlay";
            if (GUILayout.Button(toggleLabel))
            {
                settings.show = !settings.show;
                PixelPerfectOverlayCanvas.SetSettings(settings);
            }

            // Кнопка переключения на следующую текстуру
            if (GUILayout.Button("Next Overlay"))
            {
                if (settings.overlayTextures != null && settings.overlayTextures.Length > 0)
                {
                    settings.currentIndex = (settings.currentIndex + 1) % settings.overlayTextures.Length;
                    PixelPerfectOverlayCanvas.SetSettings(settings);
                }
            }

            // (Опционально) Кнопка переключения на предыдущую текстуру
            if (GUILayout.Button("Previous Overlay"))
            {
                if (settings.overlayTextures != null && settings.overlayTextures.Length > 0)
                {
                    settings.currentIndex--;
                    if (settings.currentIndex < 0) settings.currentIndex = settings.overlayTextures.Length - 1;
                    PixelPerfectOverlayCanvas.SetSettings(settings);
                }
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