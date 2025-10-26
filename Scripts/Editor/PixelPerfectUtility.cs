using UnityEngine;

namespace OrazgylyjovFuteres.PixelPerfectOverlay.Editor
{
    public static class PixelPerfectUtility
    {
        public static Texture2D CreateInvertedTexture(Texture2D source)
        {
            if (source == null) 
                return null;

            Texture2D tex = new Texture2D(source.width, source.height, source.format, false);
            Color[] pixels = source.GetPixels();

            for (int i = 0; i < pixels.Length; i++)
            {
                Color c = pixels[i];
                c.r = 1f - c.r;
                c.g = 1f - c.g;
                c.b = 1f - c.b;
                pixels[i] = c;
            }

            tex.SetPixels(pixels);
            tex.Apply();
            tex.hideFlags = HideFlags.DontSave;
            return tex;
        }
    }
}