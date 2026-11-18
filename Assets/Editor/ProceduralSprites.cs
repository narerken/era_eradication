#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Emberfall.EditorTools
{

    public static class ProceduralSprites
    {
        public const string Folder = "Assets/Art/Generated";

        public static readonly Color Stone = new Color(0.16f, 0.15f, 0.17f);
        public static readonly Color StoneLight = new Color(0.27f, 0.24f, 0.24f);
        public static readonly Color Ember = new Color(1f, 0.55f, 0.15f);
        public static readonly Color EmberHot = new Color(1f, 0.82f, 0.35f);
        public static readonly Color Robot = new Color(0.82f, 0.86f, 0.9f);
        public static readonly Color RobotDark = new Color(0.45f, 0.5f, 0.56f);
        public static readonly Color Sentinel = new Color(0.85f, 0.26f, 0.22f);
        public static readonly Color Turret = new Color(0.27f, 0.55f, 0.86f);
        public static readonly Color Brute = new Color(0.72f, 0.4f, 0.18f);
        public static readonly Color SkyTop = new Color(0.08f, 0.07f, 0.12f);
        public static readonly Color SkyBottom = new Color(0.22f, 0.12f, 0.1f);

        public static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Art")) AssetDatabase.CreateFolder("Assets", "Art");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Art", "Generated");
        }

        private static Texture2D NewTex(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var clear = new Color[w * h];
            for (int i = 0; i < clear.Length; i++) clear[i] = Color.clear;
            tex.SetPixels(clear);
            return tex;
        }

        private static void FillRoundedRect(Texture2D tex, Color c, float inset = 0f, float corner = 0.28f)
        {
            int w = tex.width, h = tex.height;
            float r = Mathf.Min(w, h) * corner;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float px = x + 0.5f, py = y + 0.5f;
                float minX = inset, minY = inset, maxX = w - inset, maxY = h - inset;
                bool inside = px >= minX && px <= maxX && py >= minY && py <= maxY;
                if (inside)
                {

                    float cx = Mathf.Clamp(px, minX + r, maxX - r);
                    float cy = Mathf.Clamp(py, minY + r, maxY - r);
                    float dist = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));
                    if (dist > r) inside = false;
                }
                if (inside) Blend(tex, x, y, c);
            }
        }

        private static void FillCircle(Texture2D tex, Color c, Vector2 centerPx, float radiusPx)
        {
            for (int y = 0; y < tex.height; y++)
            for (int x = 0; x < tex.width; x++)
                if (Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), centerPx) <= radiusPx)
                    Blend(tex, x, y, c);
        }

        private static void Blend(Texture2D tex, int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= tex.width || y >= tex.height) return;
            Color under = tex.GetPixel(x, y);
            float a = c.a + under.a * (1f - c.a);
            Color result = a <= 0.0001f ? Color.clear :
                new Color((c.r * c.a + under.r * under.a * (1f - c.a)) / a,
                          (c.g * c.a + under.g * under.a * (1f - c.a)) / a,
                          (c.b * c.a + under.b * under.a * (1f - c.a)) / a, a);
            tex.SetPixel(x, y, result);
        }

        private static Sprite SaveSprite(Texture2D tex, string name, int pixelsPerUnit, Vector2? customPivot = null)
        {
            EnsureFolder();
            byte[] png = tex.EncodeToPNG();
            string path = $"{Folder}/{name}.png";
            File.WriteAllBytes(path, png);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            var settings = importer.GetDefaultPlatformTextureSettings();
            settings.format = TextureImporterFormat.RGBA32;
            importer.SetPlatformTextureSettings(settings);
            if (customPivot.HasValue)
            {
                importer.spritePivot = customPivot.Value;
                var so = new SerializedObject(importer);
                so.FindProperty("m_SpriteMode").intValue = (int)SpriteImportMode.Single;
                so.ApplyModifiedProperties();
            }
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        public static Sprite EmberCoinSprite()
        {
            var tex = NewTex(28, 28);
            FillCircle(tex, new Color(0.45f, 0.22f, 0.08f), new Vector2(14, 14), 13f);
            FillCircle(tex, Ember, new Vector2(14, 14), 10.5f);
            FillCircle(tex, EmberHot, new Vector2(14, 14), 6f);
            FillCircle(tex, new Color(1f, 1f, 1f, 0.85f), new Vector2(10.5f, 10.5f), 2f);
            return SaveSprite(tex, "ember_coin", 48);
        }

        public static Sprite HazardSprite()
        {
            var tex = NewTex(48, 28);
            int w = tex.width, h = tex.height;
            const int spikes = 4;
            float segW = w / (float)spikes;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int i = Mathf.FloorToInt(x / segW);
                float localCenter = i * segW + segW / 2f;
                float halfWidthAtY = (segW / 2f) * (1f - y / (float)h);
                if (Mathf.Abs(x + 0.5f - localCenter) <= halfWidthAtY)
                    Blend(tex, x, y, Sentinel);
            }
            return SaveSprite(tex, "hazard_spikes", 40);
        }

        public static Sprite GoalSprite()
        {
            var tex = NewTex(40, 56);
            FillRoundedRect(tex, new Color(Ember.r, Ember.g, Ember.b, 0.25f), 0, 0.5f);
            FillCircle(tex, EmberHot, new Vector2(20, 28), 13f);
            FillCircle(tex, new Color(1f, 1f, 1f, 0.8f), new Vector2(20, 28), 5f);
            return SaveSprite(tex, "goal_portal", 40);
        }

        public static Sprite PanelSprite()
        {
            var tex = NewTex(32, 32);
            FillRoundedRect(tex, new Color(Stone.r, Stone.g, Stone.b, 0.92f), 0, 0.22f);
            return SaveSprite(tex, "panel_bg", 100);
        }

        public static Sprite KnobSprite()
        {
            var tex = NewTex(32, 32);
            FillCircle(tex, new Color(1f, 1f, 1f, 0.55f), new Vector2(16, 16), 15f);
            return SaveSprite(tex, "stick_knob", 100);
        }
    }
}
#endif
