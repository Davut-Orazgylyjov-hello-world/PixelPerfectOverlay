using UnityEditor;
using UnityEngine;

namespace OrazgylyjovFuteres.PixelPerfectOverlay.Editor
{
    [InitializeOnLoad]
    public static class PixelPerfectOverlayDrawer
    {
        private static PixelPerfectSettings settings;
        private static Material overlayMaterial;
        private static bool isInitialized;

        // Инициализация — подписываемся на события
        static PixelPerfectOverlayDrawer()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            Camera.onPostRender += OnCameraPostRender;
            isInitialized = true;
        }

        public static void SetSettings(PixelPerfectSettings currentSettings)
        {
            settings = currentSettings;
            // Обновляем материал при изменении настроек (альфа)
            EnsureMaterial();
            if (overlayMaterial != null && settings != null)
                overlayMaterial.color = new Color(1f, 1f, 1f, settings.alpha);
        }

        // ---------------- Scene View ----------------
        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!isInitialized || settings == null || !settings.show || settings.CurrentTexture() == null)
                return;

            // GUI блок для SceneView
            Handles.BeginGUI();

            Rect viewRect = sceneView.position;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, viewRect.width, viewRect.height, 0);

            Texture2D currentTex = settings.CurrentTexture();
            float texW = currentTex.width;
            float texH = currentTex.height;

            float x = (viewRect.width - texW) / 2f;
            float yTop = (viewRect.height - texH) / 2f;

            Rect drawRect = new Rect(x, yTop, texW, texH);

            if (overlayMaterial != null)
            {
                Graphics.DrawTexture(drawRect, currentTex, overlayMaterial);
            }
            else
            {
                GUI.color = new Color(1f, 1f, 1f, settings.alpha);
                GUI.DrawTexture(drawRect, currentTex);
                GUI.color = Color.white;
            }

            // Сетка
            if (settings.showGrid)
                DrawGrid(viewRect);

            GL.PopMatrix();
            Handles.EndGUI();
        }


        // ---------------- Game View ----------------
        private static void OnCameraPostRender(Camera cam)
        {
            if (!isInitialized || settings == null || !settings.show || settings.CurrentTexture() == null)
                return;

            // рисуем только в Game камеры (editor)
            if (cam.cameraType != CameraType.Game)
                return;

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, Screen.width, 0, Screen.height);

            Texture2D currentTex = settings.CurrentTexture();
            float texW = currentTex.width;
            float texH = currentTex.height;

            // Центрируем по экрану
            float x = (Screen.width - texW) / 2f;
            float y = (Screen.height - texH) / 2f;

            Rect drawRect = new Rect(x, y, texW, texH);

            if (overlayMaterial != null)
            {
                Graphics.DrawTexture(drawRect, currentTex, overlayMaterial);
            }
            else
            {
                GUI.color = new Color(1f, 1f, 1f, settings.alpha);
                Graphics.DrawTexture(drawRect, currentTex, new Rect(0, 0, 1, 1), 0, 0, 0, 0, GUI.color);
                GUI.color = Color.white;
            }

            if (settings.showGrid)
                DrawGridGameView();

            GL.PopMatrix();
        }

        // ---------------- Grid для SceneView (GUI координаты: origin top-left) ----------------
        private static void DrawGrid(Rect viewRect)
        {
            // Рисуем линиями через GL/LINES — нужно создать материал для GL. Упростим и используем Handles.DrawLine,
            // но Handles в GUI блоке рисует в GUI пространстве, поэтому используем GL immediate.
            // Здесь мы воспользуемся GL и встроенным цветом.
            Color oldColor = GUI.color;
            // Прозрачный белый
            Color gridColor = new Color(1f, 1f, 1f, 0.15f);

            // Draw using GL lines
            var lineMat = GetLineMaterial();
            lineMat.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(gridColor);

            int cellSize = 8; // пиксельный шаг сетки — можно вынести в настройки

            for (int x = 0; x <= viewRect.width; x += cellSize)
            {
                GL.Vertex3(x, 0, 0);
                GL.Vertex3(x, viewRect.height, 0);
            }

            for (int y = 0; y <= viewRect.height; y += cellSize)
            {
                GL.Vertex3(0, y, 0);
                GL.Vertex3(viewRect.width, y, 0);
            }

            GL.End();
            GUI.color = oldColor;
        }

        // ---------------- Grid для GameView (origin bottom-left) ----------------
        private static void DrawGridGameView()
        {
            var lineMat = GetLineMaterial();
            lineMat.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(new Color(1f, 1f, 1f, 0.12f));

            int cellSize = 8;
            for (int x = 0; x <= Screen.width; x += cellSize)
            {
                GL.Vertex3(x, 0, 0);
                GL.Vertex3(x, Screen.height, 0);
            }

            for (int y = 0; y <= Screen.height; y += cellSize)
            {
                GL.Vertex3(0, y, 0);
                GL.Vertex3(Screen.width, y, 0);
            }

            GL.End();
        }

        // ---------------- Materials Utilities ----------------
        private static void EnsureMaterial()
        {
            if (overlayMaterial != null)
                return;

            // Пытаемся найти простой шейдер Unlit/Transparent (есть в Unity по умолчанию)
            Shader shader = Shader.Find("Unlit/Transparent");
            if (shader == null)
            {
                // fallback: try Unlit/Texture
                shader = Shader.Find("Unlit/Texture");
            }

            if (shader != null)
            {
                overlayMaterial = new Material(shader);
                if (settings != null)
                    overlayMaterial.color = new Color(1f, 1f, 1f, settings.alpha);
                overlayMaterial.hideFlags = HideFlags.DontSave;
            }
            else
            {
                overlayMaterial = null;
                Debug.LogWarning(
                    "[PixelPerfectOverlay] Не найден подходящий Unlit шейдер. Overlay будет рисоваться без материала.");
            }
        }

        // Материал для линий (GL)
        private static Material lineMaterial;

        private static Material GetLineMaterial()
        {
            if (lineMaterial != null)
                return lineMaterial;

            // simple colored material using Unity's built-in shader
            Shader s = Shader.Find("Hidden/Internal-Colored");
            if (s == null)
            {
                // Если не найден — создаём минимальный шейдер (редко)
                lineMaterial = new Material(Shader.Find("Unlit/Color"));
            }
            else
            {
                lineMaterial = new Material(s);
                // Настройки для рисования линий в GL
                lineMaterial.hideFlags = HideFlags.DontSave;
                lineMaterial.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                lineMaterial.SetInt("_Cull", (int) UnityEngine.Rendering.CullMode.Off);
                lineMaterial.SetInt("_ZWrite", 0);
            }

            return lineMaterial;
        }
    }
}