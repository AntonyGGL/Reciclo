using System.Collections.Generic;
using ReCiclo.Sprint4;
using ReCiclo.Sprint6;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReCiclo.UI
{
    public sealed class ReCicloNavigation : MonoBehaviour
    {
        private const string GameScene = "Reciclo2D_Game";
        private static readonly Color Ink = new Color32(22, 54, 48, 255);
        private static readonly Color Cream = new Color32(247, 244, 222, 255);
        private static readonly Color Green = new Color32(42, 155, 91, 255);
        private Font font;

        private void Awake()
        {
            MenuMapMusic.EnsurePlaying();
            EnsureEventSystem();
            EnsureManagers();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildScreen();
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static void EnsureManagers()
        {
            if (SaveSystem.Instance == null)
            {
                var save = new GameObject("SaveSystem");
                save.AddComponent<SaveSystem>();
                DontDestroyOnLoad(save);
            }
            if (WorldMapManager.Instance == null)
            {
                var map = new GameObject("WorldMapManager");
                map.AddComponent<WorldMapManager>();
            }
            WorldMapManager.Instance.SyncWithSaveSystem();
        }

        private void BuildScreen()
        {
            bool isMenu = SceneManager.GetActiveScene().name == "Inicio";
            var canvasGo = new GameObject("NavigationCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;

            string backgroundName = isMenu ? "ReCicloUI/MainMenu_Background_Wide" : "ReCicloUI/WorldMap_Background_v4";
            Sprite backgroundSprite = LoadSprite(backgroundName);
            var backgroundFill = Image("BackgroundFill", canvasGo.transform, Vector2.zero, Vector2.one,
                isMenu ? new Color(.55f, .65f, .65f, 1f) : new Color32(26, 74, 68, 255));
            if (isMenu && backgroundSprite != null)
            {
                backgroundFill.sprite = backgroundSprite;
                backgroundFill.preserveAspect = true;
                var fillAspect = backgroundFill.gameObject.AddComponent<AspectRatioFitter>();
                fillAspect.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fillAspect.aspectRatio = backgroundSprite.rect.width / backgroundSprite.rect.height;
            }
            var background = Image("IllustratedBackground", canvasGo.transform, Vector2.zero, Vector2.one, Color.white);
            background.sprite = backgroundSprite;
            background.preserveAspect = isMenu;
            if (isMenu && background.sprite != null)
            {
                var cover = background.gameObject.AddComponent<AspectRatioFitter>();
                cover.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                cover.aspectRatio = background.sprite.rect.width / background.sprite.rect.height;
            }
            Image("ReadabilityShade", canvasGo.transform, Vector2.zero, Vector2.one,
                isMenu ? new Color(0.02f, .10f, .09f, .28f) : new Color(.02f, .08f, .08f, .12f));

            if (isMenu) BuildMainMenu(canvasGo.transform);
            else BuildWorldMap(canvasGo.transform);
        }

        private void AddDecorations(Transform root)
        {
            Image("TopBand", root, new Vector2(0, .83f), Vector2.one, Ink);
            Image("BottomBand", root, Vector2.zero, new Vector2(1, .045f), Green);
            Label("LeafMark", root, "♻", new Vector2(.06f, .865f), new Vector2(.27f, .985f), 112, Color.white);
        }

        private void BuildMainMenu(Transform root)
        {
            Image("SkyVeil", root, new Vector2(0, .52f), Vector2.one, new Color(0.01f, .12f, .13f, .10f));

            var titlePlaque = Image("TitlePlaque", root, new Vector2(.34f, .78f), new Vector2(.66f, .96f), Color.white);
            titlePlaque.sprite = LoadSprite("ReCicloUI/MainMenu_TitleSign");
            titlePlaque.preserveAspect = true;
            AddShadow(titlePlaque.gameObject, new Color(.08f, .04f, .01f, .55f), new Vector2(0, -7));

            var title = Label("Title", titlePlaque.transform, "ReCiclo", new Vector2(.10f, .15f), new Vector2(.76f, .87f),
                86, new Color32(54, 82, 45, 255));
            var titleOutline = title.gameObject.AddComponent<Outline>();
            titleOutline.effectColor = new Color(1f, .91f, .66f, .62f);
            titleOutline.effectDistance = new Vector2(2, -2);

            var subtitle = Label("Subtitle", root, "UNA AVENTURA POR UNA CIUDAD MÁS LIMPIA",
                new Vector2(.32f, .71f), new Vector2(.68f, .78f), 24, new Color32(255, 245, 190, 255));
            AddShadow(subtitle.gameObject, new Color(0, 0, 0, .65f), new Vector2(0, -3));

            var play = Button("Play", root, "JUGAR", new Vector2(.35f, .54f), new Vector2(.65f, .68f),
                Color.white, () => SceneManager.LoadScene("MapaMundial"), 38);
            StyleMenuSign(play, LoadSprite("ReCicloUI/LocationSign"));

            var continueButton = Button("Continue", root, "CONTINUAR", new Vector2(.36f, .38f), new Vector2(.64f, .52f),
                Color.white, ContinueGame, 32);
            StyleMenuSign(continueButton, LoadSprite("ReCicloUI/LocationSignLevel2"));

            var exit = Button("Exit", root, "SALIR", new Vector2(.38f, .23f), new Vector2(.62f, .36f),
                Color.white, Quit, 29);
            StyleMenuSign(exit, LoadSprite("ReCicloUI/LocationSignLevel3"));

            var invitation = Label("Invitation", root, "EL CAMBIO COMIENZA CONTIGO",
                new Vector2(.35f, .15f), new Vector2(.65f, .22f), 20, Color.white);
            AddShadow(invitation.gameObject, new Color(0, 0, 0, .72f), new Vector2(0, -3));

            var footer = Image("VersionPlate", root, new Vector2(.40f, .025f), new Vector2(.60f, .075f), new Color(.02f, .14f, .11f, .62f));
            Label("Version", footer.transform, "AVENTURA ECOLÓGICA", Vector2.zero, Vector2.one, 16, Color.white);
        }

        private void StyleMenuSign(Button button, Sprite sign)
        {
            button.image.sprite = sign;
            button.image.preserveAspect = true;
            AddShadow(button.gameObject, new Color(.08f, .04f, .01f, .58f), new Vector2(0, -7));

            var text = button.GetComponentInChildren<Text>();
            text.color = new Color32(64, 43, 24, 255);
            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, .92f, .68f, .45f);
            outline.effectDistance = new Vector2(1, -1);

            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color32(255, 244, 190, 255);
            colors.pressedColor = new Color32(208, 184, 128, 255);
            colors.selectedColor = colors.highlightedColor;
            colors.fadeDuration = .12f;
            button.colors = colors;
        }

        private void BuildWorldMap(Transform root)
        {
            var header = Image("MapHeader", root, new Vector2(.35f, .81f), new Vector2(.65f, .98f), Color.white);
            header.sprite = LoadSprite("ReCicloUI/HuancayoMapBanner");
            header.preserveAspect = true;
            AddShadow(header.gameObject, new Color(0, 0, 0, .38f), new Vector2(0, -5));

            List<LevelData> levels = WorldMapManager.Instance.GetAllLevels();
            Sprite locationSign = LoadSprite("ReCicloUI/LocationSign");
            Sprite marketSign = LoadSprite("ReCicloUI/LocationSignLevel2");
            Sprite dumpSign = LoadSprite("ReCicloUI/LocationSignLevel3");
            Vector2[] signPositions =
            {
                new Vector2(.24f, .235f), // Plaza: debajo de la fuente
                new Vector2(.52f, .515f), // Mercado: debajo de los puestos
                new Vector2(.80f, .685f)  // Basurero: debajo del complejo
            };
            for (int i = 0; i < levels.Count; i++)
            {
                LevelData level = levels[i];
                int captured = level.levelIndex;
                Vector2 p = signPositions[i];
                Vector2 half = new Vector2(.065f, .055f);
                var button = Button("Level" + captured, root, level.districtName.ToUpperInvariant(), p - half, p + half,
                    Color.white, () => PlayLevel(captured), 19);
                button.image.sprite = captured == 2 ? marketSign : captured == 3 ? dumpSign : locationSign;
                button.image.preserveAspect = true;
                AddShadow(button.gameObject, new Color(0.08f, .05f, .02f, .45f), new Vector2(0, -3));
                var signText = button.GetComponentInChildren<Text>();
                signText.color = new Color32(55, 37, 22, 255);
                signText.fontStyle = FontStyle.Bold;
                var colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color32(255, 244, 197, 255);
                colors.pressedColor = new Color32(196, 176, 120, 255);
                colors.disabledColor = new Color32(183, 174, 143, 220);
                button.colors = colors;
                button.interactable = level.isUnlocked;
            }
            var homeButton = Button("Back", root, "", new Vector2(.018f, .02f), new Vector2(.078f, .127f),
                Color.white, () => SceneManager.LoadScene("Inicio"), 1);
            homeButton.image.sprite = LoadSprite("ReCicloUI/HomeButton");
            homeButton.image.preserveAspect = true;
            AddShadow(homeButton.gameObject, new Color(0, 0, 0, .45f), new Vector2(0, -4));

            var footer = Image("MapFooter", root, new Vector2(.38f, .018f), new Vector2(.62f, .072f), new Color(.035f, .15f, .11f, .72f));
            Label("Progress", footer.transform, "ZONAS RECUPERADAS  " + CountCompleted(levels) + "/3", Vector2.zero, Vector2.one, 18, Color.white);
        }

        private static int CountCompleted(List<LevelData> levels)
        {
            int count = 0;
            foreach (var level in levels) if (level.starsEarned > 0) count++;
            return count;
        }

        private static string CategoryName(WasteCategory category)
        {
            switch (category)
            {
                case WasteCategory.Paper: return "Papel";
                case WasteCategory.Plastic: return "Plástico";
                case WasteCategory.Glass: return "Vidrio";
                case WasteCategory.Organic: return "Orgánico";
                default: return "Electrónico";
            }
        }

        private static Sprite LoadSprite(string resourcePath)
        {
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null) return null;
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100f);
        }

        private static void AddShadow(GameObject target, Color color, Vector2 distance)
        {
            var shadow = target.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        private void ContinueGame()
        {
            SceneManager.LoadScene("MapaMundial");
        }

        private void PlayLevel(int levelIndex)
        {
            if (!WorldMapManager.Instance.SelectLevel(levelIndex)) return;
            SceneManager.LoadScene(GameScene);
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private Image Image(string name, Transform parent, Vector2 min, Vector2 max, Color color)
        {
            var go = Rect(name, parent, min, max);
            var image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private Text Label(string name, Transform parent, string value, Vector2 min, Vector2 max, int size, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var go = Rect(name, parent, min, max);
            var label = go.AddComponent<Text>();
            label.font = font;
            label.text = value;
            label.fontSize = size;
            label.fontStyle = FontStyle.Bold;
            label.alignment = alignment;
            label.color = color;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 16;
            label.resizeTextMaxSize = size;
            return label;
        }

        private Button Button(string name, Transform parent, string caption, Vector2 min, Vector2 max, Color color, UnityEngine.Events.UnityAction action, int size = 34)
        {
            var image = Image(name, parent, min, max, color);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            var label = Label("Text", image.transform, caption, Vector2.zero, Vector2.one, size, Color.white);
            // The sign PNG contains transparent decoration around its light writing area.
            // Keep text inside that visible center instead of centering against the full texture.
            label.rectTransform.anchorMin = new Vector2(.18f, .24f);
            label.rectTransform.anchorMax = new Vector2(.82f, .74f);
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            label.alignment = TextAnchor.MiddleCenter;
            return button;
        }

        private static GameObject Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return go;
        }
    }
}
