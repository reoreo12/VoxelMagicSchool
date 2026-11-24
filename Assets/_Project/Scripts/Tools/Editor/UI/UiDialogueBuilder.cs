// 초상화 대사창 프리팹(UI_SPEC 6장)을 만들어 UI_Root의 Popup 층에 넣는다. 대사 그림을 UiAtlas에 등록하고, 테마에 그림을 채우고,
// 크리처용 임시 화자 데이터(이름 '그것', 테스트 초상화)를 만들어 열린 씬의 CreatureNpc에 비어 있으면 연결한다.
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using TMPro;
using VoxelMagic.Core;
using VoxelMagic.Gameplay;
using VoxelMagic.UI;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class UiDialogueBuilder
    {
        private const string Art = "Assets/_Project/Art/UI/dialogue_ui/";
        private const string AtlasPath = "Assets/_Project/Art/UI/UiAtlas.spriteatlasv2";
        private const string SpeakerFolder = "Assets/_Project/Data/Speakers";
        private const string CreatureSpeakerPath = SpeakerFolder + "/Speaker_It.asset";
        private const string WindowName = "UI_Dialogue";

        [MenuItem("VoxelMagic/UI/화면 만들기/대사창")]
        public static void Build()
        {
            Debug.Log(UiImportRuleApplier.ApplyAll()); // Point·압축 없음·PPU 16·Single
            AddToAtlas();
            var theme = UiWidgetPresetBuilder.FillThemeDefaults();
            theme.Dialogue.SetSprites(Load("dialogue_panel.png"), Load("dialogue_left_column.png"), Load("dialogue_next_arrow.png"), Load("portrait_placeholder_56.png"));
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();

            var prefab = UiHudBuilder.SavePrefab(BuildWindow(theme));
            InsertIntoRoot(prefab);
            AssignCreatureSpeaker(GetOrCreateCreatureSpeaker());
            Debug.Log("[UI] 대사창을 UI_Root에 넣었습니다.");
        }

        private static void AddToAtlas()
        {
            var folder = AssetDatabase.LoadAssetAtPath<Object>(Art.TrimEnd('/'));
            var atlasAsset = SpriteAtlasAsset.Load(AtlasPath);
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AtlasPath);
            if (atlasAsset == null || atlas == null || folder == null) return;
            foreach (var packed in atlas.GetPackables()) if (packed == folder) return;
            atlasAsset.Add(new[] { folder });
            SpriteAtlasAsset.Save(atlasAsset, AtlasPath);
            AssetDatabase.Refresh();
        }

        private static GameObject BuildWindow(UiTheme theme)
        {
            var root = new GameObject(WindowName, typeof(RectTransform), typeof(CanvasGroup));
            PixelLayout.Stretch((RectTransform)root.transform);
            var frame = new GameObject("Frame", typeof(RectTransform));
            frame.transform.SetParent(root.transform, false);

            // 그리는 순서: 패널 → 초상화 → 왼쪽 열(초상화 창 테두리 + 이름표) → 이름 → 대사 → ▼
            var panel = NewImage(frame.transform, "Panel");
            var portrait = NewImage(frame.transform, "Portrait");
            var column = NewImage(frame.transform, "LeftColumn");
            var name = Text(frame.transform, "Name", UiTextStyle.BodyBold, theme, TextAlignmentOptions.Center, false);
            var nameBold = Text(frame.transform, "NameBold", UiTextStyle.Small, theme, TextAlignmentOptions.Center, false);
            nameBold.gameObject.SetActive(false);
            var text = Text(frame.transform, "Text", UiTextStyle.Body, theme, TextAlignmentOptions.TopLeft, true);
            var arrow = NewImage(frame.transform, "NextArrow");

            var window = root.AddComponent<DialogueWindow>();
            var so = new SerializedObject(window);
            so.FindProperty("_id").enumValueIndex = (int)GameScreen.Dialogue;
            so.FindProperty("_theme").objectReferenceValue = theme;
            so.FindProperty("_frame").objectReferenceValue = frame.transform;
            so.FindProperty("_panel").objectReferenceValue = panel;
            so.FindProperty("_portrait").objectReferenceValue = portrait;
            so.FindProperty("_leftColumn").objectReferenceValue = column;
            so.FindProperty("_name").objectReferenceValue = name;
            so.FindProperty("_nameBold").objectReferenceValue = nameBold;
            so.FindProperty("_text").objectReferenceValue = text;
            so.FindProperty("_nextArrow").objectReferenceValue = arrow;
            so.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        private static void InsertIntoRoot(GameObject prefab)
        {
            var path = UiRootPrefabBuilder.PrefabPath;
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var instance = UiHudBuilder.Replace(root.transform.Find("Popup"), prefab);
                PixelLayout.Stretch((RectTransform)instance.transform);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static SpeakerProfile GetOrCreateCreatureSpeaker()
        {
            var speaker = AssetDatabase.LoadAssetAtPath<SpeakerProfile>(CreatureSpeakerPath);
            if (speaker != null) return speaker;
            System.IO.Directory.CreateDirectory(SpeakerFolder);
            speaker = ScriptableObject.CreateInstance<SpeakerProfile>();
            var so = new SerializedObject(speaker);
            so.FindProperty("_displayName").stringValue = GameTexts.CreatureName;
            so.FindProperty("_defaultPortrait").objectReferenceValue = Load("portrait_placeholder_56.png");
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(speaker, CreatureSpeakerPath);
            AssetDatabase.SaveAssets();
            return speaker;
        }

        // 열린 씬의 크리처 중 화자가 비어 있는 것에만 연결한다(다른 씬은 그 씬을 연 뒤 메뉴를 다시 실행).
        private static void AssignCreatureSpeaker(SpeakerProfile speaker)
        {
            foreach (var npc in Object.FindObjectsByType<CreatureNpc>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var so = new SerializedObject(npc);
                var field = so.FindProperty("_speaker");
                if (field == null || field.objectReferenceValue != null) continue;
                field.objectReferenceValue = speaker;
                so.ApplyModifiedProperties();
                EditorSceneManager.MarkSceneDirty(npc.gameObject.scene);
                Debug.Log($"[UI] {npc.name}에 화자 '{speaker.DisplayName}' 연결");
            }
        }

        private static ThemedText Text(Transform parent, string name, UiTextStyle style, UiTheme theme, TextAlignmentOptions alignment, bool wrap)
        {
            var go = UiHudBuilder.Instantiate("W_Text", parent, name);
            var label = go.GetComponent<TMP_Text>();
            label.alignment = alignment;
            label.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            var themed = go.GetComponent<ThemedText>();
            themed.SetTheme(theme);
            themed.SetStyle(style);
            return themed;
        }

        private static Image NewImage(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        private static Sprite Load(string file)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + file);
            if (sprite == null) Debug.LogWarning($"[UI] 스프라이트가 없습니다: {file}");
            return sprite;
        }
    }
}
