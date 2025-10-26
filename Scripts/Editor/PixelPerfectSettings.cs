using UnityEditor;
using UnityEngine;

namespace OrazgylyjovFuteres.PixelPerfectOverlay.Editor
{
    [System.Serializable]
    public class PixelPerfectSettings
    {
        private const string PREF_PREFIX = "PPO_";
        private const string PREF_VERSION = "_v1";

        public Texture2D overlayTexture;
        public float alpha = 0.5f;
        public bool showGrid = true;
        public bool enablePixelPerfect = false;
        public bool showInScene = true;
        public bool showInGame = false;

        // --- Load / Save ---
        public void Load()
        {
            alpha = EditorPrefs.GetFloat(Key("Alpha"), 0.5f);
            showGrid = EditorPrefs.GetBool(Key("ShowGrid"), true);
            enablePixelPerfect = EditorPrefs.GetBool(Key("EnablePixelPerfect"), false);
            showInScene = EditorPrefs.GetBool(Key("ShowInScene"), true);
            showInGame = EditorPrefs.GetBool(Key("ShowInGame"), false);

            string path = EditorPrefs.GetString(Key("TexturePath"), "");
            if (!string.IsNullOrEmpty(path))
                overlayTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public void Save()
        {
            EditorPrefs.SetFloat(Key("Alpha"), alpha);
            EditorPrefs.SetBool(Key("ShowGrid"), showGrid);
            EditorPrefs.SetBool(Key("EnablePixelPerfect"), enablePixelPerfect);
            EditorPrefs.SetBool(Key("ShowInScene"), showInScene);
            EditorPrefs.SetBool(Key("ShowInGame"), showInGame);
            SaveTexturePath();
        }

        public void SaveTexturePath()
        {
            string path = overlayTexture ? AssetDatabase.GetAssetPath(overlayTexture) : "";
            EditorPrefs.SetString(Key("TexturePath"), path);
        }

        private string Key(string name) => PREF_PREFIX + name + PREF_VERSION;
    }
}