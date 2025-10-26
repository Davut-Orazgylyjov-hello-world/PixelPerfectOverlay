using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace OrazgylyjovFuteres.PixelPerfectOverlay.Editor
{
    [InitializeOnLoad]
    public static class PixelPerfectOverlayCanvas
    {
        private static Canvas overlayCanvas;
        private static Image overlayImage;
        private static PixelPerfectSettings settings;

        static PixelPerfectOverlayCanvas()
        {
            EditorApplication.update += UpdateOverlay;
        }

        public static void SetSettings(PixelPerfectSettings currentSettings)
        {
            settings = currentSettings;
            EnsureCanvas();
        }

        private static void EnsureCanvas()
        {
            if (overlayCanvas == null)
            {
                GameObject go = new GameObject("PPO_OverlayCanvas_EditorOnly");
                go.hideFlags = HideFlags.HideAndDontSave;

                overlayCanvas = go.AddComponent<Canvas>();
                overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                overlayCanvas.sortingOrder = 1000; // поверх UI

                CanvasScaler scaler = go.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                go.AddComponent<GraphicRaycaster>();

                GameObject imgGo = new GameObject("OverlayImage");
                imgGo.transform.SetParent(go.transform);
                overlayImage = imgGo.AddComponent<Image>();
                overlayImage.raycastTarget = false;
            }

            UpdateOverlayImage();
        }

        private static void UpdateOverlayImage()
        {
            if (overlayImage == null || settings == null)
                return;

            Texture2D currentTexture = settings.CurrentTexture();
            overlayImage.sprite = currentTexture
                ? Sprite.Create(currentTexture,
                    new Rect(0, 0, currentTexture.width, currentTexture.height),
                    new Vector2(0.5f, 0.5f))
                : null;
            
            overlayImage.color = new Color(1, 1, 1, settings.alpha);

            RectTransform rt = overlayImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        private static void UpdateOverlay()
        {
            if (overlayCanvas != null && settings != null)
            {
                overlayCanvas.gameObject.SetActive(settings.show);
                overlayCanvas.enabled = overlayCanvas.gameObject.activeSelf;
            }
        }
    }
}