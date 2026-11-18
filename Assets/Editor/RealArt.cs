#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Emberfall.EditorTools
{

    public static class RealArt
    {
        private const string Root = "Assets/Art/Real";

        private static Sprite LoadSprite(string path, int ppu, Vector4 border = default)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = ppu;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spriteBorder = border;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite[] LoadFrames(string dir, string prefix, int count, int ppu)
        {
            var frames = new Sprite[count];
            for (int i = 0; i < count; i++)
                frames[i] = LoadSprite($"{dir}/{prefix}_{i:00}.png", ppu);
            return frames;
        }

        public struct CharacterFrames
        {
            public Sprite[] idle, run, jump, fall, hit;
        }

        private static CharacterFrames LoadCharacter(string name)
        {
            string baseDir = $"{Root}/Characters/{name}";
            return new CharacterFrames
            {
                idle = LoadFrames($"{baseDir}/Idle", "Idle", 11, 32),
                run = LoadFrames($"{baseDir}/Run", "Run", 12, 32),
                jump = LoadFrames($"{baseDir}/Jump", "Jump", 1, 32),
                fall = LoadFrames($"{baseDir}/Fall", "Fall", 1, 32),
                hit = LoadFrames($"{baseDir}/Hit", "Hit", 7, 32),
            };
        }

        public static CharacterFrames VirtualGuy() => LoadCharacter("VirtualGuy");

        public struct EnemyFrames
        {
            public Sprite[] idle, run;
        }

        private static EnemyFrames LoadEnemyCharacter(string name, int frameCount, int ppu)
        {
            string baseDir = $"{Root}/Characters/{name}";
            return new EnemyFrames
            {
                idle = LoadFrames($"{baseDir}/Idle", "Idle", frameCount, ppu),
                run = LoadFrames($"{baseDir}/Run", "Run", frameCount, ppu),
            };
        }

        public static EnemyFrames SkullEnemy() => LoadEnemyCharacter("Skull", 4, 16);
        public static EnemyFrames GhostEnemy() => LoadEnemyCharacter("Ghosts", 4, 16);
        public static EnemyFrames MouthEnemy() => LoadEnemyCharacter("Mouth", 4, 16);

        public static Sprite PlatformTile() => LoadSprite($"{Root}/Terrain/lava_dirt_top.png", 16);

        public static Sprite HeartIcon() => LoadSprite($"{Root}/HUD/heart.png", 36);
        public static Sprite FireIcon() => LoadSprite($"{Root}/HUD/fire.png", 36);
        public static Sprite JoystickBg() => LoadSprite($"{Root}/Joystick/stick_bg.png", 512);
        public static Sprite JoystickKnob() => LoadSprite($"{Root}/Joystick/stick_knob.png", 512);

        public static Sprite SkyBackground() => LoadSprite($"{Root}/Background/sky_bg.png", 64);

        public static Sprite MenuBackground() => LoadSprite($"{Root}/Background/menu_bg.png", 736);

        private static readonly Vector4 UiBorder = new Vector4(4, 4, 4, 4);
        public static Sprite ButtonBg() => LoadSprite($"{Root}/UI/button_bg.png", 16, UiBorder);
        public static Sprite JumpButtonBg() => LoadSprite($"{Root}/UI/jump_button_bg.png", 16, UiBorder);
        public static Sprite PauseIcon() => LoadSprite($"{Root}/UI/pause_icon.png", 16, UiBorder);

        public static Font PixelFont()
        {
            string path = $"{Root}/Fonts/LazyFoxPixelFont1.ttf";
            return AssetDatabase.LoadAssetAtPath<Font>(path);
        }
    }
}
#endif
