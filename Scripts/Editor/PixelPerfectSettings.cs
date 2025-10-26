using System;
using UnityEditor;
using UnityEngine;

namespace OrazgylyjovFuteres.PixelPerfectOverlay.Editor
{
    [System.Serializable]
    public class PixelPerfectSettings
    {
        private const string PREF_PREFIX = "PPO_";
        private const string PREF_VERSION = "_v1";

        public Texture2D[] overlayTextures;
        [System.NonSerialized] public int currentIndex = 0; 
        public float alpha = 0.5f;
        public bool showGrid = true;
        public bool enablePixelPerfect = false;
        public bool show = true;

        // --- Load / Save ---
        public void Load()
        {
            alpha = EditorPrefs.GetFloat(Key("Alpha"), 0.5f);
            showGrid = EditorPrefs.GetBool(Key("ShowGrid"), true);
            enablePixelPerfect = EditorPrefs.GetBool(Key("EnablePixelPerfect"), false);
            show = EditorPrefs.GetBool(Key("ShowInScene"), true);

            // Получаем сохранённые пути, разделённые ";"
            string pathsStr = EditorPrefs.GetString(Key("TexturePaths"), "");
            if (!string.IsNullOrEmpty(pathsStr))
            {
                string[] paths = pathsStr.Split(';');
                overlayTextures = new Texture2D[paths.Length];
                for (int i = 0; i < paths.Length; i++)
                {
                    overlayTextures[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(paths[i]);
                }
            }
            else
            {
                overlayTextures = new Texture2D[0];
            }
        }

        public void Save()
        {
            EditorPrefs.SetFloat(Key("Alpha"), alpha);
            EditorPrefs.SetBool(Key("ShowGrid"), showGrid);
            EditorPrefs.SetBool(Key("EnablePixelPerfect"), enablePixelPerfect);
            EditorPrefs.SetBool(Key("ShowInScene"), show);
            SaveTexturePath();
        }
        
        public Texture2D CurrentTexture()
        {
            if (overlayTextures == null || overlayTextures.Length == 0)
                return null;
            
            currentIndex = Mathf.Clamp(currentIndex, 0, overlayTextures.Length - 1);
            return overlayTextures[currentIndex];
        }

        public void SaveTexturePath()
        {
            if (overlayTextures != null && overlayTextures.Length > 0)
            {
                string pathsStr = string.Join(";", Array.ConvertAll(overlayTextures, t => t != null ? AssetDatabase.GetAssetPath(t) : ""));
                EditorPrefs.SetString(Key("TexturePaths"), pathsStr);
            }
            else
            {
                EditorPrefs.SetString(Key("TexturePaths"), "");
            }
        }


        private string Key(string name) => PREF_PREFIX + name + PREF_VERSION;
    }
}