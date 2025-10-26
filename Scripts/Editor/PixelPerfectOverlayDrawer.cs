using UnityEditor;
using UnityEngine;

namespace OrazgylyjovFuteres.PixelPerfectOverlay.Editor
{
   [InitializeOnLoad]
    public static class PixelPerfectOverlayDrawer
    {
        private static PixelPerfectSettings settings;
        private static Material overlayMaterial;

        static PixelPerfectOverlayDrawer()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            Camera.onPostRender += OnCameraPostRender;
        }

        public static void SetSettings(PixelPerfectSettings currentSettings)
        {
            settings = currentSettings;
            EnsureMaterial();
            if (overlayMaterial != null && settings != null)
                overlayMaterial.color = new Color(1f, 1f, 1f, settings.alpha);
        }

        // ---------------- Scene View ----------------
        private static void OnSceneGUI(SceneView sceneView)
        {
            if (settings == null || !settings.showInScene || settings.overlayTexture == null)
                return;

            Handles.BeginGUI();
            Rect viewRect = sceneView.position;

            DrawOverlayInRect(viewRect);
            Handles.EndGUI();
        }

        // ---------------- Game View ----------------
        private static void OnCameraPostRender(Camera cam)
        {
            if (settings == null || !settings.showInGame || settings.overlayTexture == null)
                return;

            if (cam.cameraType != CameraType.Game)
                return;

            Rect viewRect = new Rect(0, 0, Screen.width, Screen.height);
            DrawOverlayInRect(viewRect);
        }

        // ---------------- Core Drawing ----------------
        private static void DrawOverlayInRect(Rect viewRect)
        {
            if (overlayMaterial != null)
            {
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, viewRect.width, viewRect.height, 0);

                float texW = settings.overlayTexture.width;
                float texH = settings.overlayTexture.height;

                // центрируем текстуру по экрану
                float x = (viewRect.width - texW) / 2f;
                float y = (viewRect.height - texH) / 2f;

                Rect drawRect = new Rect(x, y, texW, texH);
                Graphics.DrawTexture(drawRect, settings.overlayTexture, overlayMaterial);

                GL.PopMatrix();
            }

            // сетка поверх
            if (settings.showGrid)
                DrawGrid(viewRect);
        }

        // ---------------- Grid ----------------
        private static Material lineMaterial;
        private static void DrawGrid(Rect viewRect)
        {
            if (lineMaterial == null)
            {
                Shader s = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(s)
                {
                    hideFlags = HideFlags.DontSave
                };
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                lineMaterial.SetInt("_ZWrite", 0);
            }

            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, viewRect.width, viewRect.height, 0);

            GL.Begin(GL.LINES);
            GL.Color(new Color(1f, 1f, 1f, 0.12f));
            int cellSize = 8;

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
            GL.PopMatrix();
        }

        // ---------------- Material ----------------
        private static void EnsureMaterial()
        {
            if (overlayMaterial != null)
                return;

            Shader shader = Shader.Find("Unlit/Transparent") ?? Shader.Find("Unlit/Texture");
            if (shader != null)
            {
                overlayMaterial = new Material(shader) { hideFlags = HideFlags.DontSave };
                if (settings != null)
                    overlayMaterial.color = new Color(1f, 1f, 1f, settings.alpha);
            }
            else
            {
                overlayMaterial = null;
                Debug.LogWarning("[PixelPerfectOverlay] Shader Unlit/Transparent не найден. Overlay будет работать fallback GUI.");
            }
        }
    }
}