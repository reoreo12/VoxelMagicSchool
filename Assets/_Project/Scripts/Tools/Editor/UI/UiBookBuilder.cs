// 책(메뉴) 화면 프리팹을 만들어 UI_Root의 Popup 층에 넣는다(UI_SPEC 5장): 책 그림·사이드 탭 5개·탭별 페이지. 의뢰 목록 탭만 내용이 있고 나머지는 빈 페이지.
// 테마에 책 페이지·책등·탭 색·탭 아이콘·체크 상자 스프라이트와 문구를 채운다.
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VoxelMagic.Core;
using VoxelMagic.UI;

namespace VoxelMagic.Tools.EditorTools.UiWorkbench
{
    public static class UiBookBuilder
    {
        private const string Art = "Assets/_Project/Art/UI/";
        private const string BookName = "UI_Book";

        // 탭 순서·색·아이콘 (사용자 결정 2026-11-20: 의뢰 목록 → 마법 스펠 → 마법진 → 환경 설정 → 나가기/종료 메뉴. 색은 13장 임시값 + 의뢰는 노랑).
        private static readonly (BookTab tab, string color, string icon)[] Tabs =
        {
            (BookTab.Requests, "amber", "tab_icon_quest.png"),
            (BookTab.Spells, "purple", "tab_icon_spell_v2.png"),
            (BookTab.MagicCircles, "blue", "tab_icon_circle_v2.png"),
            (BookTab.Settings, "green", "tab_icon_settings.png"),
            (BookTab.Menu, "cream", "tab_icon_menu.png"),
        };

        [MenuItem("VoxelMagic/UI/화면 만들기/책")]
        public static void Build()
        {
            Debug.Log(UiImportRuleApplier.ApplyAll());
            var theme = UiWidgetPresetBuilder.FillThemeDefaults();
            FillTheme(theme);
            SeedTexts();
            var prefab = UiHudBuilder.SavePrefab(BuildBook(theme));
            InsertIntoRoot(prefab);
            Debug.Log("[UI] 책 화면을 UI_Root에 넣었습니다.");
        }

        private static void FillTheme(UiTheme theme)
        {
            var tabs = new BookTabValues[Tabs.Length];
            for (int i = 0; i < Tabs.Length; i++)
                tabs[i] = new BookTabValues(Tabs[i].tab, Load($"book_ui/tabs/tab_{Tabs[i].color}_normal.png"),
                    Load($"book_ui/tabs/tab_{Tabs[i].color}_selected.png"), Load($"menu_icons/tab_icons/{Tabs[i].icon}"));
            var checkbox = new[]
            {
                Load("quest_ui/checkbox/checkbox_on_normal.png"), Load("quest_ui/checkbox/checkbox_on_hover.png"),
                Load("quest_ui/checkbox/checkbox_off_normal.png"), Load("quest_ui/checkbox/checkbox_off_hover.png"),
            };
            theme.Book.SetSprites(Load("book_ui/book_page_left.png"), Load("book_ui/book_page_right.png"), Load("book_ui/book_spine.png"), tabs, checkbox);
            theme.Book.SetTurnSprites(new[] { Load("book_ui/page_turn/leaf_0.png"), Load("book_ui/page_turn/leaf_1.png"), Load("book_ui/page_turn/leaf_2.png") },
                Load("book_ui/page_turn/leaf_edge.png"), Load("book_ui/page_turn/shadow_strip.png"));
            theme.Book.SetPageButtonSprites(Load("book_ui/buttons/btn_page_normal.png"), Load("book_ui/buttons/btn_page_hover.png"), Load("book_ui/buttons/btn_page_pressed.png"));
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
        }

        private static void SeedTexts()
        {
            var table = AssetDatabase.LoadAssetAtPath<TextTable>(UiRootPrefabBuilder.TextTablePath);
            if (table == null) return;
            void Seed(string key, string value)
            {
                if (!table.TryGet(key, out _)) table.Set(key, value);
            }
            Seed(UiTextKeys.BookQuestShowHud, "화면에 의뢰 목록 표시");
            Seed(UiTextKeys.BookQuestNeeds, "필요한 재료");
            Seed(UiTextKeys.BookQuestReward, "보상");
            Seed(UiTextKeys.BookQuestRewardValue, "필기 점수 +{0}");
            Seed(UiTextKeys.BookQuestLine, "“{0}”");
            Seed(UiTextKeys.BookMenuTitle, "메뉴");
            Seed(UiTextKeys.BookMenuMainMenu, "메인 화면으로");
            Seed(UiTextKeys.BookMenuQuit, "게임 종료");
            Seed(UiTextKeys.BookMenuClose, "책 닫기");
            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();
        }

        private static GameObject BuildBook(UiTheme theme)
        {
            var root = new GameObject(BookName, typeof(RectTransform), typeof(CanvasGroup));
            PixelLayout.Stretch((RectTransform)root.transform);
            var screen = root.AddComponent<BookScreen>();
            var screenSo = new SerializedObject(screen);
            screenSo.FindProperty("_id").enumValueIndex = (int)GameScreen.Book;
            screenSo.ApplyModifiedPropertiesWithoutUndo();

            var book = new GameObject("Book", typeof(RectTransform));
            book.transform.SetParent(root.transform, false);

            // 탭이 먼저(= 책 그림 뒤에 그려짐, 5-1).
            var tabs = new BookTabView[Tabs.Length];
            for (int i = 0; i < Tabs.Length; i++) tabs[i] = BuildTab(book.transform, Tabs[i].tab);
            var left = NewImage(book.transform, "LeftPage");
            var spine = NewImage(book.transform, "Spine");
            var right = NewImage(book.transform, "RightPage");

            var pages = new BookPage[Tabs.Length];
            pages[0] = BuildQuestPage(book.transform, theme);
            for (int i = 1; i < Tabs.Length; i++)
                pages[i] = Tabs[i].tab == BookTab.Menu ? BuildMenuPage(book.transform, theme) : BuildEmptyPage(book.transform, Tabs[i].tab, theme);

            var turner = BuildTurner(book.transform);

            var view = book.AddComponent<BookView>();
            UiHudBuilder.Assign(view, "_leftPage", left);
            UiHudBuilder.Assign(view, "_spine", spine);
            UiHudBuilder.Assign(view, "_rightPage", right);
            AssignArray(view, "_tabs", tabs);
            AssignArray(view, "_pages", pages);
            UiHudBuilder.Assign(view, "_turner", turner);
            view.SetTheme(theme);
            UiHudBuilder.Assign(screen, "_view", view);
            return root;
        }

        // 페이지 위에 그리는 넘김 연출: 그림자(종이 안쪽에서 자름) → 넘어가는 종이.
        private static BookPageTurner BuildTurner(Transform parent)
        {
            var go = new GameObject("PageTurn", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var clip = new GameObject("ShadowClip", typeof(RectTransform), typeof(RectMask2D));
            clip.transform.SetParent(go.transform, false);
            var shadowA = NewImage(clip.transform, "ShadowA");
            var shadowB = NewImage(clip.transform, "ShadowB");
            var leaf = NewImage(go.transform, "Leaf");
            var turner = go.AddComponent<BookPageTurner>();
            UiHudBuilder.Assign(turner, "_leaf", leaf);
            UiHudBuilder.Assign(turner, "_shadowClip", clip.transform);
            UiHudBuilder.Assign(turner, "_shadowA", shadowA);
            UiHudBuilder.Assign(turner, "_shadowB", shadowB);
            go.SetActive(false);
            return turner;
        }

        private static BookTabView BuildTab(Transform parent, BookTab tab)
        {
            var go = new GameObject($"Tab_{tab}", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var background = NewImage(go.transform, "Background");
            var icon = NewImage(go.transform, "Icon");
            var view = go.AddComponent<BookTabView>();
            var so = new SerializedObject(view);
            so.FindProperty("_tab").enumValueIndex = (int)tab;
            so.FindProperty("_background").objectReferenceValue = background;
            so.FindProperty("_icon").objectReferenceValue = icon;
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        private static BookPage BuildEmptyPage(Transform parent, BookTab tab, UiTheme theme)
        {
            var go = PageRoot(parent, $"Page_{tab}", out var left, out var right);
            var page = go.AddComponent<BookPage>();
            SetPageFields(page, tab, left, right);
            page.SetTheme(theme);
            return page;
        }

        private static BookPage BuildQuestPage(Transform parent, UiTheme theme)
        {
            var go = PageRoot(parent, $"Page_{BookTab.Requests}", out var left, out var right);

            var header = Text(left, "Header", UiTextStyle.HudTitle, theme, TextOverflowModes.Ellipsis, false);
            var headerLine = NewImage(left, "HeaderLine");
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(left, false);
            var row = BuildRow(viewport.transform, theme);
            row.gameObject.SetActive(false);
            var scrollbar = UiHudBuilder.Instantiate("W_Scrollbar", left, "Scrollbar").GetComponent<Scrollbar>();
            foreach (var w in scrollbar.GetComponentsInChildren<ThemedWidget>(true)) w.SetTheme(theme);
            scrollbar.gameObject.SetActive(false);
            var checkboxGo = new GameObject("ShowHud", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            checkboxGo.transform.SetParent(left, false);
            var checkbox = checkboxGo.AddComponent<ThemedCheckbox>();
            checkbox.SetTheme(theme);
            var checkboxLabel = Text(left, "ShowHudLabel", UiTextStyle.Body, theme, TextOverflowModes.Overflow, false);

            var title = Text(right, "Title", UiTextStyle.BodyBold, theme, TextOverflowModes.Ellipsis, false);
            var line = Text(right, "Line", UiTextStyle.Body, theme, TextOverflowModes.Overflow, true);
            var needsLabel = Text(right, "NeedsLabel", UiTextStyle.BodyBold, theme, TextOverflowModes.Overflow, false);
            var needs = Text(right, "Needs", UiTextStyle.Body, theme, TextOverflowModes.Overflow, false);
            var rewardLabel = Text(right, "RewardLabel", UiTextStyle.BodyBold, theme, TextOverflowModes.Overflow, false);
            var reward = Text(right, "Reward", UiTextStyle.Body, theme, TextOverflowModes.Overflow, false);

            var page = go.AddComponent<QuestBookPage>();
            SetPageFields(page, BookTab.Requests, left, right);
            UiHudBuilder.Assign(page, "_header", header);
            UiHudBuilder.Assign(page, "_headerLine", headerLine);
            UiHudBuilder.Assign(page, "_viewport", viewport.transform);
            UiHudBuilder.Assign(page, "_rowTemplate", row);
            UiHudBuilder.Assign(page, "_scrollbar", scrollbar);
            UiHudBuilder.Assign(page, "_showHud", checkbox);
            UiHudBuilder.Assign(page, "_showHudLabel", checkboxLabel);
            UiHudBuilder.Assign(page, "_title", title);
            UiHudBuilder.Assign(page, "_line", line);
            UiHudBuilder.Assign(page, "_needsLabel", needsLabel);
            UiHudBuilder.Assign(page, "_needs", needs);
            UiHudBuilder.Assign(page, "_rewardLabel", rewardLabel);
            UiHudBuilder.Assign(page, "_reward", reward);
            page.SetTheme(theme);
            return page;
        }

        // 나가기/종료 메뉴 탭: 왼쪽 페이지에 제목·구분선·버튼 세 개(배치만, 책 닫기만 동작).
        private static BookPage BuildMenuPage(Transform parent, UiTheme theme)
        {
            var go = PageRoot(parent, $"Page_{BookTab.Menu}", out var left, out var right);
            var header = Text(left, "Header", UiTextStyle.HudTitle, theme, TextOverflowModes.Ellipsis, false);
            var line = NewImage(left, "HeaderLine");
            line.enabled = true;
            var page = go.AddComponent<MenuBookPage>();
            SetPageFields(page, BookTab.Menu, left, right);
            UiHudBuilder.Assign(page, "_header", header);
            UiHudBuilder.Assign(page, "_headerLine", line);
            UiHudBuilder.Assign(page, "_mainMenu", BuildMenuButton(left, "MainMenu", theme));
            UiHudBuilder.Assign(page, "_quit", BuildMenuButton(left, "Quit", theme));
            UiHudBuilder.Assign(page, "_close", BuildMenuButton(left, "Close", theme));
            page.SetTheme(theme);
            return page;
        }

        private static BookMenuButton BuildMenuButton(Transform parent, string name, UiTheme theme)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var background = NewImage(go.transform, "Background");
            background.raycastTarget = true; // 버튼 누르기 영역
            var label = Text(go.transform, "Label", UiTextStyle.Body, theme, TextOverflowModes.Overflow, false);
            label.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
            var button = go.AddComponent<BookMenuButton>();
            UiHudBuilder.Assign(button, "_background", background);
            UiHudBuilder.Assign(button, "_label", label);
            return button;
        }

        private static QuestBookRowView BuildRow(Transform parent, UiTheme theme)
        {
            var go = new GameObject("RowTemplate", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var hit = NewImage(go.transform, "Hit");
            hit.enabled = true;
            hit.color = new Color(1f, 1f, 1f, 0f);
            hit.raycastTarget = true;
            var selection = UiHudBuilder.Instantiate("W_PaperPanel", go.transform, "Selection").GetComponent<ThemedPanel>();
            selection.SetSkin(UiPanelSkin.QuestBar);
            selection.SetTheme(theme);
            var title = Text(go.transform, "Title", UiTextStyle.Body, theme, TextOverflowModes.Ellipsis, false);
            var line = NewImage(go.transform, "Line");
            line.enabled = true;
            var view = go.AddComponent<QuestBookRowView>();
            UiHudBuilder.Assign(view, "_hit", hit);
            UiHudBuilder.Assign(view, "_selection", selection);
            UiHudBuilder.Assign(view, "_title", title);
            UiHudBuilder.Assign(view, "_line", line);
            return view;
        }

        private static GameObject PageRoot(Transform parent, string name, out RectTransform left, out RectTransform right)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            left = (RectTransform)new GameObject("Left", typeof(RectTransform)).transform;
            left.SetParent(go.transform, false);
            right = (RectTransform)new GameObject("Right", typeof(RectTransform)).transform;
            right.SetParent(go.transform, false);
            return go;
        }

        private static void SetPageFields(BookPage page, BookTab tab, RectTransform left, RectTransform right)
        {
            var so = new SerializedObject(page);
            so.FindProperty("_tab").enumValueIndex = (int)tab;
            so.FindProperty("_left").objectReferenceValue = left;
            so.FindProperty("_right").objectReferenceValue = right;
            so.ApplyModifiedPropertiesWithoutUndo();
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

        private static ThemedText Text(Transform parent, string name, UiTextStyle style, UiTheme theme, TextOverflowModes overflow, bool wrap)
        {
            var go = UiHudBuilder.Instantiate("W_Text", parent, name);
            var label = go.GetComponent<TMP_Text>();
            label.alignment = TextAlignmentOptions.TopLeft;
            label.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            label.overflowMode = overflow;
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

        private static void AssignArray(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            var list = so.FindProperty(field);
            list.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite Load(string relative)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Art + relative);
            if (sprite == null) Debug.LogWarning($"[UI] 스프라이트가 없습니다: {relative}");
            return sprite;
        }
    }
}
