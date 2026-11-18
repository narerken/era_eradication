#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Emberfall.Core;
using Emberfall.Data;
using Emberfall.Enemies;
using Emberfall.Input;
using Emberfall.Player;
using Emberfall.Save;
using Emberfall.UI;
using Emberfall.World;
using Emberfall.CameraCtrl;
using Emberfall.Visual;

namespace Emberfall.EditorTools
{

    public static class ProjectBuilder
    {
        private const int LayerGround = 8;
        private const int LayerPlayer = 9;
        private const int LayerEnemy = 10;

        [MenuItem("Emberfall/Build Full Project")]
        public static void BuildAll()
        {
            Debug.Log("[ProjectBuilder] Starting full build...");
            SetupLayers();
            SetupPlayerSettings();

            var data = BuildData();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var prefabs = BuildPrefabs(data);
            BuildMainMenuScene(data, prefabs);
            BuildGameScene(data, prefabs);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true),
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ProjectBuilder] Done. Open Assets/Scenes/MainMenu.unity and press Play.");
        }

        private static void SetupLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            SetLayer(layers, LayerGround, "Ground");
            SetLayer(layers, LayerPlayer, "Player");
            SetLayer(layers, LayerEnemy, "Enemy");
            tagManager.ApplyModifiedProperties();
        }

        private static void SetLayer(SerializedProperty layers, int index, string name)
        {
            var el = layers.GetArrayElementAtIndex(index);
            if (string.IsNullOrEmpty(el.stringValue)) el.stringValue = name;
        }

        private static void SetupPlayerSettings()
        {
            PlayerSettings.productName = "Emberfall";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        }

        private struct DataRefs
        {
            public PlayerStats player;
            public EnemyStats sentinel, turret, brute;
            public ItemData emberShard;
            public LevelData level1, level2, level3;
        }

        private static T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static DataRefs BuildData()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Data")) AssetDatabase.CreateFolder("Assets", "Data");

            var player = CreateOrLoad<PlayerStats>("Assets/Data/PlayerStats.asset");

            player.jumpForce = 13f;

            var sentinel = CreateOrLoad<EnemyStats>("Assets/Data/SentinelStats.asset");
            sentinel.kind = EnemyKind.Patroller; sentinel.moveSpeed = 2.2f; sentinel.maxHP = 40;
            sentinel.visionRange = 6f; sentinel.visionAngle = 70f; sentinel.windUpTime = 0.5f; sentinel.attackCooldown = 1.2f;

            var turret = CreateOrLoad<EnemyStats>("Assets/Data/TurretStats.asset");
            turret.kind = EnemyKind.Shooter; turret.maxHP = 30; turret.visionRange = 8f; turret.attackRange = 7f;
            turret.windUpTime = 0.6f; turret.attackCooldown = 1.6f; turret.projectileSpeed = 7.5f;

            var brute = CreateOrLoad<EnemyStats>("Assets/Data/BruteStats.asset");
            brute.kind = EnemyKind.Charger; brute.moveSpeed = 1.6f; brute.maxHP = 60; brute.contactDamage = 2;
            brute.visionRange = 5.5f; brute.visionAngle = 55f; brute.windUpTime = 0.7f;
            brute.chargeSpeedMultiplier = 3.2f; brute.chargeDuration = 0.55f; brute.attackCooldown = 1.8f;

            LayerMask groundMask = 1 << LayerGround;
            sentinel.visionBlockers = groundMask;
            turret.visionBlockers = groundMask;
            brute.visionBlockers = groundMask;

            var emberShard = CreateOrLoad<ItemData>("Assets/Data/EmberShard.asset");
            emberShard.kind = ItemKind.EmberShard; emberShard.scoreValue = 50;

            var level1 = CreateOrLoad<LevelData>("Assets/Data/Level_01.asset");
            BuildLevel1(level1);
            var level2 = CreateOrLoad<LevelData>("Assets/Data/Level_02.asset");
            BuildLevel2(level2);
            var level3 = CreateOrLoad<LevelData>("Assets/Data/Level_03.asset");
            BuildLevel3(level3);

            EditorUtility.SetDirty(player); EditorUtility.SetDirty(sentinel);
            EditorUtility.SetDirty(turret); EditorUtility.SetDirty(brute); EditorUtility.SetDirty(emberShard);
            EditorUtility.SetDirty(level1); EditorUtility.SetDirty(level2); EditorUtility.SetDirty(level3);

            return new DataRefs
            {
                player = player, sentinel = sentinel, turret = turret, brute = brute,
                emberShard = emberShard, level1 = level1, level2 = level2, level3 = level3
            };
        }

        private static DataRefs ReloadData()
        {

            DataRefs r = default;
            for (int attempt = 0; attempt < 10; attempt++)
            {
                r = new DataRefs
                {
                    player = AssetDatabase.LoadAssetAtPath<PlayerStats>("Assets/Data/PlayerStats.asset"),
                    sentinel = AssetDatabase.LoadAssetAtPath<EnemyStats>("Assets/Data/SentinelStats.asset"),
                    turret = AssetDatabase.LoadAssetAtPath<EnemyStats>("Assets/Data/TurretStats.asset"),
                    brute = AssetDatabase.LoadAssetAtPath<EnemyStats>("Assets/Data/BruteStats.asset"),
                    emberShard = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/EmberShard.asset"),
                    level1 = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/Data/Level_01.asset"),
                    level2 = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/Data/Level_02.asset"),
                    level3 = AssetDatabase.LoadAssetAtPath<LevelData>("Assets/Data/Level_03.asset"),
                };
                if (r.player != null && r.sentinel != null && r.turret != null && r.brute != null &&
                    r.emberShard != null && r.level1 != null && r.level2 != null && r.level3 != null)
                {
                    break;
                }
                Debug.LogWarning($"[ProjectBuilder] ReloadData attempt {attempt} got a null asset - retrying after a Refresh.");
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
            return r;
        }

        private static PlatformSpec P(float x, float y, float w, float h) =>
            new PlatformSpec { center = new Vector2(x, y), size = new Vector2(w, h) };

        private static HazardSpec H(float x, float y, float w, float h) =>
            new HazardSpec { center = new Vector2(x, y), size = new Vector2(w, h) };

        private static void BuildLevel1(LevelData d)
        {
            d.levelIndex = 1; d.displayName = "Ember Gate"; d.timeLimitSeconds = 90f;
            d.playerStart = new Vector2(-7, 1);
            d.goalPosition = new Vector2(26, 1.6f);
            d.platforms = new List<PlatformSpec> {
                P(-7, -0.5f, 8, 1), P(0, -0.5f, 3, 1), P(6, -0.5f, 5, 1),
                P(10, 1.2f, 2.5f, 0.6f), P(13.5f, -0.5f, 6, 1), P(19, 0.8f, 2.5f, 0.6f),
                P(22, -0.5f, 10, 1)
            };
            d.enemies = new List<EnemySpawnSpec> {
                new EnemySpawnSpec{ kind = EnemyKind.Patroller, position = new Vector2(7, 0.6f), faceRight = true, patrolRadius = 2.2f }
            };
            d.pickups = new List<Vector2> {
                new Vector2(0, 0.6f), new Vector2(13.5f, 0.6f), new Vector2(19, 1.9f),
                new Vector2(10, 2.0f), new Vector2(22, 0.6f)
            };

            d.hazards = new List<HazardSpec> { H(-2.25f, -1.1f, 1.8f, 0.8f), H(2.5f, -1.1f, 2.3f, 0.8f) };
            d.designNotes = "Intro level: safe platforming, one Patroller, teaches jump + vision telegraph.";
        }

        private static void BuildLevel2(LevelData d)
        {
            d.levelIndex = 2; d.displayName = "Rust Hollow"; d.timeLimitSeconds = 120f;
            d.playerStart = new Vector2(-7, 1);
            d.goalPosition = new Vector2(32, 2.6f);
            d.platforms = new List<PlatformSpec> {
                P(-7, -0.5f, 6, 1), P(-1, -0.5f, 3, 1), P(4, 1.0f, 2.5f, 0.6f), P(8, -0.5f, 5, 1),
                P(13, 0.8f, 2.5f, 0.6f), P(17, -0.5f, 4, 1), P(21, 1.4f, 2.5f, 0.6f),
                P(25, -0.5f, 3, 1), P(29, 1.6f, 6, 1)
            };
            d.enemies = new List<EnemySpawnSpec> {
                new EnemySpawnSpec{ kind = EnemyKind.Patroller, position = new Vector2(8, 0.6f), faceRight = true, patrolRadius = 2.0f },
                new EnemySpawnSpec{ kind = EnemyKind.Shooter, position = new Vector2(21, 2.4f), faceRight = false }
            };
            d.pickups = new List<Vector2> {
                new Vector2(-1, 0.6f), new Vector2(17, 0.6f), new Vector2(25, 0.6f),
                new Vector2(4, 2.0f), new Vector2(29, 2.6f)
            };
            d.hazards = new List<HazardSpec> { H(1.6f, -1.1f, 2.2f, 0.8f), H(22.9f, -1.1f, 1.2f, 0.8f) };
            d.designNotes = "Adds the Shooter on a high ledge; player must use cover/timing against ranged wind-up.";
        }

        private static void BuildLevel3(LevelData d)
        {
            d.levelIndex = 3; d.displayName = "Last Ember"; d.timeLimitSeconds = 150f;
            d.playerStart = new Vector2(-8, 1);
            d.goalPosition = new Vector2(38, 1.6f);
            d.platforms = new List<PlatformSpec> {
                P(-8, -0.5f, 6, 1), P(-2, -0.5f, 10, 1), P(5, 1.2f, 2.5f, 0.6f),
                P(9, -0.5f, 10, 1), P(16, 1.0f, 2.5f, 0.6f), P(20, -0.5f, 6, 1),
                P(26, 1.4f, 2.5f, 0.6f), P(29, -0.5f, 14, 1)
            };
            d.enemies = new List<EnemySpawnSpec> {
                new EnemySpawnSpec{ kind = EnemyKind.Charger, position = new Vector2(2, 0.6f), faceRight = true, patrolRadius = 2.5f },
                new EnemySpawnSpec{ kind = EnemyKind.Shooter, position = new Vector2(16, 2.4f), faceRight = false },
                new EnemySpawnSpec{ kind = EnemyKind.Charger, position = new Vector2(23, 0.6f), faceRight = false, patrolRadius = 2.5f },
                new EnemySpawnSpec{ kind = EnemyKind.Patroller, position = new Vector2(32, 0.6f), faceRight = true, patrolRadius = 2.5f }
            };
            d.pickups = new List<Vector2> {
                new Vector2(-2, 0.6f), new Vector2(9, 0.6f), new Vector2(29, 0.6f),
                new Vector2(16, 2.0f), new Vector2(26, 2.4f)
            };

            d.hazards = new List<HazardSpec> { H(9f, 0.05f, 1.2f, 0.4f), H(31f, 0.05f, 1.4f, 0.4f) };
            d.designNotes = "Gauntlet level: all three enemy types together, two Chargers bracket the mid platform.";
        }

        private struct PrefabRefs
        {
            public GameObject player, platform, patroller, shooter, charger, projectile, pickup, goal, hazard, levelButton;
        }

        private static GameObject SavePrefab(GameObject go, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static PrefabRefs BuildPrefabs(DataRefs data)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI")) AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

            var virtualGuy = RealArt.VirtualGuy();
            var skull = RealArt.SkullEnemy();
            var ghost = RealArt.GhostEnemy();
            var mouth = RealArt.MouthEnemy();
            var playerSprite = virtualGuy.idle[0];
            var groundSprite = RealArt.PlatformTile();
            var patrollerSprite = skull.idle[0];
            var shooterSprite = ghost.idle[0];
            var chargerSprite = mouth.idle[0];
            var projectileSprite = RealArt.FireIcon();
            var coinSprite = ProceduralSprites.EmberCoinSprite();
            var goalSprite = ProceduralSprites.GoalSprite();
            var hazardSprite = ProceduralSprites.HazardSprite();

            var playerGO = new GameObject("Player");
            playerGO.tag = "Player";
            playerGO.layer = LayerPlayer;
            var sr = playerGO.AddComponent<SpriteRenderer>(); sr.sprite = playerSprite; sr.sortingOrder = 10;
            var rb = playerGO.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic; rb.freezeRotation = true; rb.gravityScale = data.player.gravityScale;
            var col = playerGO.AddComponent<CapsuleCollider2D>(); col.size = new Vector2(0.7f, 1.1f);
            var groundCheck = new GameObject("GroundCheck").transform;
            groundCheck.SetParent(playerGO.transform); groundCheck.localPosition = new Vector3(0, -0.58f, 0);

            var ctrl = playerGO.AddComponent<PlayerController2D>();
            var health = playerGO.AddComponent<PlayerHealth>();
            SetPrivate(ctrl, "stats", data.player);
            SetPrivate(ctrl, "groundCheck", groundCheck);
            SetPrivate(ctrl, "groundLayer", (LayerMask)(1 << LayerGround));
            SetPrivate(ctrl, "sprite", sr);
            SetPrivate(health, "stats", data.player);
            SetPrivate(health, "sprite", sr);
            var playerAnim = AddAnimator(playerGO, new (string, Sprite[], float, bool)[]
            {
                ("Idle", virtualGuy.idle, 10f, true),
                ("Run", virtualGuy.run, 14f, true),
                ("Jump", virtualGuy.jump, 1f, false),
                ("Fall", virtualGuy.fall, 1f, false),
                ("Hit", virtualGuy.hit, 12f, false),
            }, autoPlay: "Idle");
            SetPrivate(ctrl, "anim", playerAnim);
            var playerPrefab = SavePrefab(playerGO, "Assets/Prefabs/Player.prefab");

            var platGO = new GameObject("Platform");
            platGO.layer = LayerGround;
            var platSr = platGO.AddComponent<SpriteRenderer>(); platSr.sprite = groundSprite; platSr.sortingOrder = 0;
            platGO.AddComponent<BoxCollider2D>();
            var platformPrefab = SavePrefab(platGO, "Assets/Prefabs/Platform.prefab");

            var projGO = new GameObject("Projectile");
            var projSr = projGO.AddComponent<SpriteRenderer>(); projSr.sprite = projectileSprite; projSr.sortingOrder = 8;
            var projRb = projGO.AddComponent<Rigidbody2D>(); projRb.gravityScale = 0f;
            var projCol = projGO.AddComponent<CircleCollider2D>(); projCol.isTrigger = true; projCol.radius = 0.12f;
            projGO.AddComponent<Projectile>();
            var projectilePrefab = SavePrefab(projGO, "Assets/Prefabs/Projectile.prefab");

            var patGO = new GameObject("Sentinel");
            patGO.layer = LayerEnemy;
            var patSr = patGO.AddComponent<SpriteRenderer>(); patSr.sprite = patrollerSprite; patSr.sortingOrder = 9;
            var patRb = patGO.AddComponent<Rigidbody2D>(); patRb.freezeRotation = true;
            var patCol = patGO.AddComponent<BoxCollider2D>(); patCol.size = new Vector2(0.9f, 0.9f);
            var patHealth = patGO.AddComponent<EnemyHealth>();
            var pat = patGO.AddComponent<PatrollerEnemy>();
            SetPrivate(pat, "stats", data.sentinel); SetPrivate(pat, "sprite", patSr); SetPrivate(pat, "patrolRadius", 2.2f);
            SetPrivate(patHealth, "stats", data.sentinel);
            var patAnim = AddAnimator(patGO, new (string, Sprite[], float, bool)[]
            {
                ("Idle", skull.idle, 10f, true),
                ("Run", skull.run, 14f, true),
            }, autoPlay: "Idle");
            SetPrivate(pat, "anim", patAnim);
            var patrollerPrefab = SavePrefab(patGO, "Assets/Prefabs/Sentinel.prefab");

            var shGO = new GameObject("Turret");
            shGO.layer = LayerEnemy;
            var shSr = shGO.AddComponent<SpriteRenderer>(); shSr.sprite = shooterSprite; shSr.sortingOrder = 9;
            var shRb = shGO.AddComponent<Rigidbody2D>(); shRb.freezeRotation = true;
            var shCol = shGO.AddComponent<BoxCollider2D>(); shCol.size = new Vector2(0.9f, 0.9f);
            var shHealth = shGO.AddComponent<EnemyHealth>();
            var muzzle = new GameObject("Muzzle").transform; muzzle.SetParent(shGO.transform); muzzle.localPosition = Vector3.zero;
            var sh = shGO.AddComponent<ShooterEnemy>();
            SetPrivate(sh, "stats", data.turret); SetPrivate(sh, "sprite", shSr);
            SetPrivate(sh, "projectilePrefab", projectilePrefab.GetComponent<Projectile>());
            SetPrivate(sh, "muzzle", muzzle);
            SetPrivate(shHealth, "stats", data.turret);
            var shAnim = AddAnimator(shGO, new (string, Sprite[], float, bool)[]
            {
                ("Idle", ghost.idle, 10f, true),
                ("Run", ghost.run, 14f, true),
            }, autoPlay: "Idle");
            SetPrivate(sh, "anim", shAnim);
            var shooterPrefab = SavePrefab(shGO, "Assets/Prefabs/Turret.prefab");

            var chGO = new GameObject("Brute");
            chGO.layer = LayerEnemy;
            var chSr = chGO.AddComponent<SpriteRenderer>(); chSr.sprite = chargerSprite; chSr.sortingOrder = 9;
            var chRb = chGO.AddComponent<Rigidbody2D>(); chRb.freezeRotation = true;
            var chCol = chGO.AddComponent<BoxCollider2D>(); chCol.size = new Vector2(1.15f, 0.9f);
            var chHealth = chGO.AddComponent<EnemyHealth>();
            var ch = chGO.AddComponent<ChargerEnemy>();
            SetPrivate(ch, "stats", data.brute); SetPrivate(ch, "sprite", chSr); SetPrivate(ch, "patrolRadius", 2.5f);
            SetPrivate(chHealth, "stats", data.brute);
            var chAnim = AddAnimator(chGO, new (string, Sprite[], float, bool)[]
            {
                ("Idle", mouth.idle, 10f, true),
                ("Run", mouth.run, 14f, true),
            }, autoPlay: "Idle");
            SetPrivate(ch, "anim", chAnim);
            var chargerPrefab = SavePrefab(chGO, "Assets/Prefabs/Brute.prefab");

            var pickGO = new GameObject("EmberShardPickup");
            var pickSr = pickGO.AddComponent<SpriteRenderer>(); pickSr.sprite = coinSprite; pickSr.sortingOrder = 5;
            var pickCol = pickGO.AddComponent<CircleCollider2D>(); pickCol.isTrigger = true; pickCol.radius = 0.3f;
            var pickup = pickGO.AddComponent<Pickup>();
            SetPrivate(pickup, "item", data.emberShard);
            var pickupPrefab = SavePrefab(pickGO, "Assets/Prefabs/EmberShardPickup.prefab");

            var goalGO = new GameObject("GoalPortal");
            var goalSr = goalGO.AddComponent<SpriteRenderer>(); goalSr.sprite = goalSprite; goalSr.sortingOrder = 4;
            var goalCol = goalGO.AddComponent<BoxCollider2D>(); goalCol.isTrigger = true; goalCol.size = new Vector2(1f, 1.4f);
            goalGO.AddComponent<Goal>();
            var goalPrefab = SavePrefab(goalGO, "Assets/Prefabs/GoalPortal.prefab");

            var hazGO = new GameObject("Hazard");
            var hazSr = hazGO.AddComponent<SpriteRenderer>(); hazSr.sprite = hazardSprite; hazSr.sortingOrder = 3;
            var hazCol = hazGO.AddComponent<BoxCollider2D>(); hazCol.isTrigger = true;
            hazGO.AddComponent<Hazard>();
            var hazardPrefab = SavePrefab(hazGO, "Assets/Prefabs/Hazard.prefab");

            var btnGO = new GameObject("LevelButton", typeof(RectTransform));
            var btnImg = btnGO.AddComponent<Image>(); btnImg.sprite = RealArt.ButtonBg(); btnImg.type = Image.Type.Sliced;
            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            var btnLabel = CreateText(btnGO.transform, "Label", "Level", 28, TextAnchor.MiddleCenter);
            Stretch(btnLabel.rectTransform);
            var lockGO = new GameObject("Lock", typeof(RectTransform));
            lockGO.transform.SetParent(btnGO.transform, false);
            var lockImg = lockGO.AddComponent<Image>(); lockImg.color = new Color(0, 0, 0, 0.55f);
            Stretch(lockGO.GetComponent<RectTransform>());
            ((RectTransform)btnGO.transform).sizeDelta = new Vector2(220, 70);
            var levelButtonPrefab = SavePrefab(btnGO, "Assets/Prefabs/UI/LevelButton.prefab");

            return new PrefabRefs
            {
                player = playerPrefab, platform = platformPrefab, patroller = patrollerPrefab,
                shooter = shooterPrefab, charger = chargerPrefab, projectile = projectilePrefab,
                pickup = pickupPrefab, goal = goalPrefab, hazard = hazardPrefab, levelButton = levelButtonPrefab
            };
        }

        private static void SetPrivate(object target, string field, object value)
        {

            if (target is UnityEngine.Object uObj)
            {
                var so = new SerializedObject(uObj);
                var prop = so.FindProperty(field);
                if (prop != null && TrySetSerializedProperty(prop, value))
                {
                    so.ApplyModifiedPropertiesWithoutUndo();
                    return;
                }
            }
            SetPrivateReflection(target, field, value);
        }

        private static bool TrySetSerializedProperty(SerializedProperty prop, object value)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = value as UnityEngine.Object;
                    return true;
                case SerializedPropertyType.LayerMask:
                    prop.intValue = value is LayerMask lm ? lm.value : System.Convert.ToInt32(value);
                    return true;
                case SerializedPropertyType.Float:
                    prop.floatValue = System.Convert.ToSingle(value);
                    return true;
                case SerializedPropertyType.Integer:
                    prop.intValue = System.Convert.ToInt32(value);
                    return true;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = System.Convert.ToBoolean(value);
                    return true;
                case SerializedPropertyType.String:
                    prop.stringValue = value as string;
                    return true;
                case SerializedPropertyType.Generic:
                    if (prop.isArray && value is System.Collections.IEnumerable list && !(value is string))
                    {
                        var items = new List<object>();
                        foreach (var it in list) items.Add(it);

                        prop.arraySize = items.Count;
                        prop.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        prop.serializedObject.Update();
                        for (int i = 0; i < items.Count; i++)
                        {
                            var elem = prop.GetArrayElementAtIndex(i);
                            elem.objectReferenceValue = items[i] as UnityEngine.Object;
                        }
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        private static void SetPrivateReflection(object target, string field, object value)
        {
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.DeclaredOnly;

            var type = target.GetType();
            System.Reflection.FieldInfo fi = null;
            while (type != null && fi == null)
            {
                fi = type.GetField(field, flags);
                type = type.BaseType;
            }
            if (fi == null) { Debug.LogWarning($"[ProjectBuilder] field '{field}' not found on {target.GetType().Name}"); return; }
            fi.SetValue(target, value);
        }

        private static void AddClick(Button button, UnityEngine.Events.UnityAction call) =>
            UnityEventTools.AddPersistentListener(button.onClick, call);

        private static Canvas CreateCanvas(string name, out GameObject root)
        {
            root = new GameObject(name, typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;
            root.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static GameObject CreateEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem));
            var uiModule = go.AddComponent<InputSystemUIInputModule>();

            uiModule.pointerBehavior = UIPointerBehavior.AllPointersAsIs;
            return go;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        private static void Anchor(RectTransform rt, Vector2 anchor, Vector2 sizeDelta, Vector2 anchoredPos)
        {
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor;
            rt.sizeDelta = sizeDelta; rt.anchoredPosition = anchoredPos;
        }

        private static Image CreateImage(Transform parent, string name, Sprite sprite, Color color)
        {
            var rt = CreateRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite; img.color = color;
            return img;
        }

        private static SimpleSpriteAnimator AddAnimator(GameObject go, (string name, Sprite[] frames, float fps, bool loop)[] clipDefs, string autoPlay = null)
        {
            var anim = go.AddComponent<SimpleSpriteAnimator>();
            var clips = new SimpleSpriteAnimator.Clip[clipDefs.Length];
            for (int i = 0; i < clipDefs.Length; i++)
            {
                clips[i] = new SimpleSpriteAnimator.Clip
                {
                    name = clipDefs[i].name,
                    frames = clipDefs[i].frames,
                    frameRate = clipDefs[i].fps,
                    loop = clipDefs[i].loop
                };
            }

            anim.Configure(clips, autoPlay);
            return anim;
        }

        private static Text CreateText(Transform parent, string name, string content, int size, TextAnchor anchor)
        {
            var rt = CreateRect(name, parent);
            var txt = rt.gameObject.AddComponent<Text>();
            txt.text = content;
            txt.font = RealArt.PixelFont() != null ? RealArt.PixelFont() : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = size;
            txt.alignment = anchor;
            txt.color = Color.white;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;

            txt.raycastTarget = false;
            return txt;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchor, Vector2 size, Vector2 pos, int fontSize = 32)
        {
            var rt = CreateRect(name, parent);
            Anchor(rt, anchor, size, pos);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = RealArt.ButtonBg();
            img.type = Image.Type.Sliced;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var txt = CreateText(rt, "Label", label, fontSize, TextAnchor.MiddleCenter);
            Stretch(txt.rectTransform);
            return btn;
        }

        private static Button CreateIconButton(Transform parent, string name, Sprite icon, Vector2 anchor, Vector2 size, Vector2 pos)
        {
            var rt = CreateRect(name, parent);
            Anchor(rt, anchor, size, pos);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = RealArt.ButtonBg();
            img.type = Image.Type.Sliced;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var iconImg = CreateImage(rt, "Icon", icon, Color.white);
            iconImg.raycastTarget = false;
            Anchor(iconImg.rectTransform, new Vector2(0.5f, 0.5f), size * 0.55f, Vector2.zero);
            return btn;
        }

        private static void BuildMainMenuScene(DataRefs data, PrefabRefs prefabs)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            data = ReloadData();

            var camGO = new GameObject("Main Camera", typeof(Camera));
            camGO.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camGO.GetComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = 5f;
            cam.backgroundColor = ProceduralSprites.SkyBottom; cam.tag = "MainCamera";

            CreateEventSystem();

            var gmGO = new GameObject("GameManager");
            var gm = gmGO.AddComponent<GameManager>();
            SetPrivate(gm, "allLevels", new[] { data.level1, data.level2, data.level3 });
            gmGO.AddComponent<SaveManager>();

            var canvas = CreateCanvas("Canvas", out var canvasRoot);

            var bg = CreateImage(canvasRoot.transform, "Background", RealArt.MenuBackground(), Color.white);
            bg.type = Image.Type.Simple;
            bg.preserveAspect = false;
            Stretch(bg.rectTransform);
            bg.raycastTarget = false;

            var safeArea = CreateRect("SafeArea", canvasRoot.transform);
            Stretch(safeArea);
            safeArea.gameObject.AddComponent<Emberfall.UI.SafeArea>();

            var menuPanel = CreateRect("MenuPanel", safeArea);
            Stretch(menuPanel);

            var titleBacker = CreateImage(menuPanel, "TitleBacker", null, new Color(0, 0, 0, 0.35f));
            Anchor(titleBacker.rectTransform, new Vector2(0.5f, 0.75f), new Vector2(1760, 280), Vector2.zero);
            titleBacker.raycastTarget = false;

            var title = CreateText(menuPanel, "Title", "THE ERA OF ERADICATION", 64, TextAnchor.MiddleCenter);
            Anchor(title.rectTransform, new Vector2(0.5f, 0.78f), new Vector2(1700, 160), Vector2.zero);
            title.color = ProceduralSprites.Ember;
            title.fontStyle = FontStyle.Bold;
            var titleOutline = title.gameObject.AddComponent<Outline>();
            titleOutline.effectColor = new Color(0.15f, 0.05f, 0.02f, 1f);
            titleOutline.effectDistance = new Vector2(3, -3);
            var titleShadow = title.gameObject.AddComponent<Shadow>();
            titleShadow.effectColor = new Color(0, 0, 0, 0.6f);
            titleShadow.effectDistance = new Vector2(0, -6);

            var subtitle = CreateText(menuPanel, "Subtitle", "a small light, a long ruin", 30, TextAnchor.MiddleCenter);
            Anchor(subtitle.rectTransform, new Vector2(0.5f, 0.68f), new Vector2(900, 60), Vector2.zero);
            var subtitleShadow = subtitle.gameObject.AddComponent<Shadow>();
            subtitleShadow.effectColor = new Color(0, 0, 0, 0.7f);
            subtitleShadow.effectDistance = new Vector2(0, -3);

            var playBtn = CreateButton(menuPanel, "PlayButton", "PLAY", new Vector2(0.5f, 0.42f), new Vector2(360, 120), Vector2.zero, 40);
            var settingsBtn = CreateButton(menuPanel, "SettingsButton", "SETTINGS", new Vector2(0.5f, 0.27f), new Vector2(360, 120), Vector2.zero, 40);
            var quitBtn = CreateButton(menuPanel, "QuitButton", "QUIT", new Vector2(0.5f, 0.12f), new Vector2(360, 120), Vector2.zero, 40);

            var levelPanel = CreateRect("LevelSelectPanel", safeArea);
            Stretch(levelPanel);
            var levelTitle = CreateText(levelPanel, "Title", "SELECT LEVEL", 56, TextAnchor.MiddleCenter);
            Anchor(levelTitle.rectTransform, new Vector2(0.5f, 0.85f), new Vector2(900, 100), Vector2.zero);
            var container = CreateRect("ButtonContainer", levelPanel);
            Anchor(container, new Vector2(0.5f, 0.5f), new Vector2(1200, 300), Vector2.zero);
            var layout = container.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 40; layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false; layout.childForceExpandHeight = false;
            var backBtn = CreateButton(levelPanel, "BackButton", "BACK", new Vector2(0.5f, 0.1f), new Vector2(240, 90), Vector2.zero, 32);
            levelPanel.gameObject.SetActive(false);

            var settingsPanel = CreateRect("SettingsPanel", safeArea);
            Stretch(settingsPanel);
            var settingsTitle = CreateText(settingsPanel, "Title", "SETTINGS", 56, TextAnchor.MiddleCenter);
            Anchor(settingsTitle.rectTransform, new Vector2(0.5f, 0.85f), new Vector2(900, 100), Vector2.zero);

            var musicLabel = CreateText(settingsPanel, "MusicLabel", "MUSIC", 28, TextAnchor.MiddleLeft);
            Anchor(musicLabel.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(200, 50), new Vector2(-420, 0));
            var musicSlider = CreateSlider(settingsPanel, "MusicSlider", new Vector2(0.5f, 0.62f), new Vector2(500, 40), new Vector2(120, 0));

            var sfxLabel = CreateText(settingsPanel, "SfxLabel", "SFX", 28, TextAnchor.MiddleLeft);
            Anchor(sfxLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(200, 50), new Vector2(-420, 0));
            var sfxSlider = CreateSlider(settingsPanel, "SfxSlider", new Vector2(0.5f, 0.5f), new Vector2(500, 40), new Vector2(120, 0));

            var resetBtn = CreateButton(settingsPanel, "ResetProgressButton", "RESET PROGRESS", new Vector2(0.5f, 0.32f), new Vector2(420, 90), Vector2.zero, 28);
            var settingsBackBtn = CreateButton(settingsPanel, "BackButton", "BACK", new Vector2(0.5f, 0.1f), new Vector2(240, 90), Vector2.zero, 32);

            var confirmPanel = CreateRect("ConfirmResetPanel", settingsPanel);
            Stretch(confirmPanel);
            var dim = confirmPanel.gameObject.AddComponent<Image>(); dim.color = new Color(0, 0, 0, 0.75f);
            var confirmBox = CreateImage(confirmPanel, "Box", ProceduralSprites.PanelSprite(), Color.white);
            Anchor(confirmBox.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(760, 320), Vector2.zero);
            var confirmText = CreateText(confirmBox.transform, "Text", "Reset ALL progress?\nThis cannot be undone.", 32, TextAnchor.MiddleCenter);
            Anchor(confirmText.rectTransform, new Vector2(0.5f, 0.65f), new Vector2(700, 140), Vector2.zero);
            var confirmYes = CreateButton(confirmBox.transform, "ConfirmButton", "YES, RESET", new Vector2(0.3f, 0.2f), new Vector2(280, 90), Vector2.zero, 26);
            var confirmNo = CreateButton(confirmBox.transform, "CancelButton", "CANCEL", new Vector2(0.7f, 0.2f), new Vector2(280, 90), Vector2.zero, 26);
            confirmPanel.gameObject.SetActive(false);
            settingsPanel.gameObject.SetActive(false);

            var levelSelectCtrl = levelPanel.gameObject.AddComponent<LevelSelectController>();
            var menuCtrl = canvasRoot.AddComponent<MainMenuController>();
            SetPrivate(menuCtrl, "menuPanel", menuPanel.gameObject);
            SetPrivate(menuCtrl, "levelSelectPanel", levelPanel.gameObject);
            SetPrivate(menuCtrl, "settingsPanel", settingsPanel.gameObject);
            SetPrivate(menuCtrl, "levelSelect", levelSelectCtrl);

            SetPrivate(levelSelectCtrl, "menu", menuCtrl);
            SetPrivate(levelSelectCtrl, "buttonContainer", container);
            SetPrivate(levelSelectCtrl, "levelButtonPrefab", prefabs.levelButton);

            var settingsCtrl = settingsPanel.gameObject.AddComponent<SettingsPanel>();
            SetPrivate(settingsCtrl, "musicSlider", musicSlider);
            SetPrivate(settingsCtrl, "sfxSlider", sfxSlider);
            SetPrivate(settingsCtrl, "menu", menuCtrl);

            var resetPanelCtrl = settingsPanel.gameObject.AddComponent<ResetProgressPanel>();
            SetPrivate(resetPanelCtrl, "confirmPanel", confirmPanel.gameObject);

            AddClick(playBtn, menuCtrl.OnPlayPressed);
            AddClick(settingsBtn, menuCtrl.OnSettingsPressed);
            AddClick(quitBtn, menuCtrl.OnQuitPressed);
            AddClick(backBtn, levelSelectCtrl.OnBackPressed);
            AddClick(settingsBackBtn, settingsCtrl.OnBackPressed);
            AddClick(resetBtn, resetPanelCtrl.OnResetPressed);
            AddClick(confirmYes, resetPanelCtrl.OnConfirmPressed);
            AddClick(confirmNo, resetPanelCtrl.OnCancelPressed);
            UnityEventTools.AddFloatPersistentListener(musicSlider.onValueChanged, settingsCtrl.OnVolumeChanged, 0f);
            UnityEventTools.AddFloatPersistentListener(sfxSlider.onValueChanged, settingsCtrl.OnVolumeChanged, 0f);

            EnsureScenesFolder();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainMenu.unity");
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 anchor, Vector2 size, Vector2 pos)
        {
            var rt = CreateRect(name, parent);
            Anchor(rt, anchor, size, pos);
            var bg = rt.gameObject.AddComponent<Image>(); bg.color = new Color(1, 1, 1, 0.15f);
            var slider = rt.gameObject.AddComponent<Slider>();

            var fillArea = CreateRect("FillArea", rt);
            Stretch(fillArea);
            var fill = CreateImage(fillArea, "Fill", null, ProceduralSprites.Ember);
            Stretch(fill.rectTransform);

            var handleArea = CreateRect("HandleArea", rt);
            Stretch(handleArea);
            var handle = CreateImage(handleArea, "Handle", ProceduralSprites.KnobSprite(), Color.white);
            handle.rectTransform.sizeDelta = new Vector2(36, 36);

            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0.8f;
            return slider;
        }

        private static void BuildGameScene(DataRefs data, PrefabRefs prefabs)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            data = ReloadData();

            var camGO = new GameObject("Main Camera", typeof(Camera));
            camGO.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camGO.GetComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = 5.5f; cam.tag = "MainCamera";
            cam.backgroundColor = ProceduralSprites.SkyBottom;

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(camGO.transform, false);
            bgGO.transform.localPosition = new Vector3(0f, 0f, 10f);
            bgGO.transform.localScale = new Vector3(40f, 14f, 1f);
            var bgSr = bgGO.AddComponent<SpriteRenderer>();
            bgSr.sprite = RealArt.SkyBackground();
            bgSr.sortingOrder = -100;

            CreateEventSystem();

            var playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.player);
            playerInstance.transform.position = new Vector3(-7, 1, 0);

            cam.gameObject.AddComponent<CameraFollow2D>();
            SetPrivate(cam.GetComponent<CameraFollow2D>(), "target", playerInstance.transform);

            var levelRoot = new GameObject("LevelRoot").transform;
            var spawner = levelRoot.gameObject.AddComponent<LevelSpawner>();
            SetPrivate(spawner, "fallbackLevel", data.level1);
            SetPrivate(spawner, "player", playerInstance.transform);
            SetPrivate(spawner, "platformPrefab", prefabs.platform);
            SetPrivate(spawner, "patrollerPrefab", prefabs.patroller);
            SetPrivate(spawner, "shooterPrefab", prefabs.shooter);
            SetPrivate(spawner, "chargerPrefab", prefabs.charger);
            SetPrivate(spawner, "pickupPrefab", prefabs.pickup);
            SetPrivate(spawner, "goalPrefab", prefabs.goal);
            SetPrivate(spawner, "hazardPrefab", prefabs.hazard);

            var canvas = CreateCanvas("Canvas", out var canvasRoot);
            var safeArea = CreateRect("SafeArea", canvasRoot.transform);
            Stretch(safeArea);
            safeArea.gameObject.AddComponent<Emberfall.UI.SafeArea>();

            var heartsContainer = CreateRect("Hearts", safeArea);
            Anchor(heartsContainer, new Vector2(0f, 1f), new Vector2(260, 70), new Vector2(150, -60));
            var heartsLayout = heartsContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            heartsLayout.spacing = 8; heartsLayout.childAlignment = TextAnchor.MiddleLeft;
            for (int i = 0; i < data.player.maxHearts; i++)
            {
                var heart = CreateImage(heartsContainer, $"Heart{i}", RealArt.HeartIcon(), Color.white);
                heart.rectTransform.sizeDelta = new Vector2(56, 52);
            }

            var scoreText = CreateText(safeArea, "ScoreText", "SCORE 0", 40, TextAnchor.MiddleCenter);
            Anchor(scoreText.rectTransform, new Vector2(0.5f, 1f), new Vector2(380, 70), new Vector2(0, -60));

            var pauseBtn = CreateIconButton(safeArea, "PauseButton", RealArt.PauseIcon(), new Vector2(1f, 1f), new Vector2(120, 120), new Vector2(-100, -80));

            var stickBg = CreateImage(safeArea, "StickBackground", RealArt.JoystickBg(), Color.white);
            Anchor(stickBg.rectTransform, new Vector2(0f, 0f), new Vector2(220, 220), new Vector2(150, 150));
            var knob = CreateImage(stickBg.transform, "Knob", RealArt.JoystickKnob(), Color.white);
            Anchor(knob.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(90, 90), Vector2.zero);
            var stick = knob.gameObject.AddComponent<OnScreenStick>();
            SetPrivate(stick, "background", stickBg.rectTransform);
            SetPrivate(stick, "knob", knob.rectTransform);
            SetPrivate(stick, "radius", 70f);

            var jumpBtnImg = CreateImage(safeArea, "JumpButton", RealArt.JumpButtonBg(), new Color(1, 1, 1, 0.95f));
            Anchor(jumpBtnImg.rectTransform, new Vector2(1f, 0f), new Vector2(170, 170), new Vector2(-150, 150));
            var jumpOnScreenBtn = jumpBtnImg.gameObject.AddComponent<OnScreenButton>();

            var jumpBtnComp = jumpBtnImg.gameObject.AddComponent<Button>();
            jumpBtnComp.targetGraphic = jumpBtnImg;
            AddClick(jumpBtnComp, jumpOnScreenBtn.OnClickPulse);
            var jumpLabel = CreateText(jumpBtnImg.transform, "Label", "JUMP", 30, TextAnchor.MiddleCenter);
            Stretch(jumpLabel.rectTransform);

            var pausePanel = CreateRect("PausePanel", safeArea);
            Stretch(pausePanel);
            var pauseDim = pausePanel.gameObject.AddComponent<Image>(); pauseDim.color = new Color(0, 0, 0, 0.75f);
            var pauseTitle = CreateText(pausePanel, "Title", "PAUSED", 64, TextAnchor.MiddleCenter);
            Anchor(pauseTitle.rectTransform, new Vector2(0.5f, 0.7f), new Vector2(600, 100), Vector2.zero);
            var resumeBtn = CreateButton(pausePanel, "ResumeButton", "RESUME", new Vector2(0.5f, 0.52f), new Vector2(340, 100), Vector2.zero, 34);
            var restartBtn = CreateButton(pausePanel, "RestartButton", "RESTART", new Vector2(0.5f, 0.38f), new Vector2(340, 100), Vector2.zero, 34);
            var pauseMenuBtn = CreateButton(pausePanel, "MenuButton", "MENU", new Vector2(0.5f, 0.24f), new Vector2(340, 100), Vector2.zero, 34);
            pausePanel.gameObject.SetActive(false);

            var completePanel = CreateRect("LevelCompletePanel", safeArea);
            Stretch(completePanel);
            completePanel.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.8f);
            var completeTitle = CreateText(completePanel, "Title", "LEVEL COMPLETE", 56, TextAnchor.MiddleCenter);
            Anchor(completeTitle.rectTransform, new Vector2(0.5f, 0.65f), new Vector2(800, 100), Vector2.zero);
            var nextBtn = CreateButton(completePanel, "NextButton", "NEXT LEVEL", new Vector2(0.5f, 0.45f), new Vector2(360, 100), Vector2.zero, 32);
            var completeMenuBtn = CreateButton(completePanel, "MenuButton", "MENU", new Vector2(0.5f, 0.3f), new Vector2(360, 100), Vector2.zero, 32);
            completePanel.gameObject.SetActive(false);

            var overPanel = CreateRect("GameOverPanel", safeArea);
            Stretch(overPanel);
            overPanel.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.85f);
            var overTitle = CreateText(overPanel, "Title", "THE LIGHT FADES", 56, TextAnchor.MiddleCenter);
            Anchor(overTitle.rectTransform, new Vector2(0.5f, 0.65f), new Vector2(900, 100), Vector2.zero);
            var retryBtn = CreateButton(overPanel, "RetryButton", "RETRY", new Vector2(0.5f, 0.45f), new Vector2(360, 100), Vector2.zero, 32);
            var overMenuBtn = CreateButton(overPanel, "MenuButton", "MENU", new Vector2(0.5f, 0.3f), new Vector2(360, 100), Vector2.zero, 32);
            overPanel.gameObject.SetActive(false);

            var pauseCtrl = canvasRoot.AddComponent<PauseController>();
            SetPrivate(pauseCtrl, "pausePanel", pausePanel.gameObject);

            var hud = canvasRoot.AddComponent<HUDController>();
            SetPrivate(hud, "heartsContainer", heartsContainer);
            SetPrivate(hud, "scoreText", scoreText);
            SetPrivate(hud, "levelCompletePanel", completePanel.gameObject);
            SetPrivate(hud, "gameOverPanel", overPanel.gameObject);
            SetPrivate(hud, "pauseController", pauseCtrl);

            AddClick(pauseBtn, hud.OnPausePressed);
            AddClick(resumeBtn, pauseCtrl.OnResumePressed);
            AddClick(restartBtn, pauseCtrl.OnRestartPressed);
            AddClick(pauseMenuBtn, pauseCtrl.OnMenuPressed);
            AddClick(nextBtn, hud.OnNextLevelPressed);
            AddClick(completeMenuBtn, hud.OnMenuFromEndPressed);
            AddClick(retryBtn, hud.OnRetryPressed);
            AddClick(overMenuBtn, hud.OnMenuFromEndPressed);

            EnsureScenesFolder();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Game.unity");
        }

        private static void EnsureScenesFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        }
    }
}
#endif
