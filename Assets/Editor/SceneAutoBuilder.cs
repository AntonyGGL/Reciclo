#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ReCiclo.Sprint1;
using ReCiclo.Sprint2;
using ReCiclo.Sprint3;
using ReCiclo.Sprint4;
using ReCiclo.Sprint5;
using ReCiclo.Sprint6;

namespace ReCiclo.Editor
{
    [InitializeOnLoad]
    public static class SceneAutoBuilder
    {
        private const string SCENE_PATH = "Assets/Scenes/SampleScene.unity";
        private const string AUTO_BUILT_FLAG = "ReCiclo_SceneAutoBuilt_v3";

        static SceneAutoBuilder()
        {
            EditorApplication.delayCall += CheckAndAutoBuildScene;
        }

        private static void CheckAndAutoBuildScene()
        {
            if (!SessionState.GetBool(AUTO_BUILT_FLAG, false))
            {
                SessionState.SetBool(AUTO_BUILT_FLAG, true);
                BuildFullGameScene();
            }
        }

        [MenuItem("ReCiclo/Construir Escena Completa de Juego", false, 1)]
        public static void BuildFullGameScene()
        {
            Debug.Log("[ReCiclo] Iniciando construccion automatizada de la escena...");

            // 1. Crear nueva escena limpia
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 2. Main Camera (2D Orthographic)
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 9.6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.12f, 0.16f, 1f);
            cam.transform.position = new Vector3(0f, 0f, -10f);
            camObj.AddComponent<AudioListener>();

            // 3. EventSystem
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();

            // 4. GameCanvas (1080x1920)
            GameObject canvasObj = new GameObject("GameCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // 4.1 Background Environment
            GameObject bgObj = CreateUIElement("Background_Environment", canvasObj.transform);
            SetRectStretched(bgObj.GetComponent<RectTransform>());
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.12f, 0.18f, 0.24f, 1f);

            // 4.2 HUD Top Container
            GameObject hudTop = CreateUIElement("HUD_Top", canvasObj.transform);
            RectTransform hudRect = hudTop.GetComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0f, 0.80f);
            hudRect.anchorMax = new Vector2(1f, 1.0f);
            hudRect.offsetMin = Vector2.zero;
            hudRect.offsetMax = Vector2.zero;

            // 4.2.1 District Title
            GameObject titleObj = CreateUIElement("District_Title", hudTop.transform);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.70f);
            titleRect.anchorMax = new Vector2(0.9f, 0.96f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = "NIVEL 1: PLAZA CENTRAL";
            titleText.font = defaultFont;
            titleText.fontSize = 36;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;

            // 4.2.2 Score Text
            GameObject scoreObj = CreateUIElement("Score_Text", hudTop.transform);
            RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0.05f, 0.40f);
            scoreRect.anchorMax = new Vector2(0.48f, 0.68f);
            scoreRect.offsetMin = Vector2.zero;
            scoreRect.offsetMax = Vector2.zero;
            Text scoreText = scoreObj.AddComponent<Text>();
            scoreText.text = "PUNTAJE: 0";
            scoreText.font = defaultFont;
            scoreText.fontSize = 32;
            scoreText.fontStyle = FontStyle.Bold;
            scoreText.alignment = TextAnchor.MiddleLeft;
            scoreText.color = new Color(0f, 0.94f, 1f);

            // 4.2.3 Timer Text
            GameObject timerObj = CreateUIElement("Timer_Text", hudTop.transform);
            RectTransform timerRect = timerObj.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.52f, 0.40f);
            timerRect.anchorMax = new Vector2(0.95f, 0.68f);
            timerRect.offsetMin = Vector2.zero;
            timerRect.offsetMax = Vector2.zero;
            Text timerText = timerObj.AddComponent<Text>();
            timerText.text = "01:30";
            timerText.font = defaultFont;
            timerText.fontSize = 36;
            timerText.fontStyle = FontStyle.Bold;
            timerText.alignment = TextAnchor.MiddleRight;
            timerText.color = new Color(1f, 0.9f, 0.1f);

            // 4.2.4 Combo Text
            GameObject comboObj = CreateUIElement("Combo_Text", hudTop.transform);
            RectTransform comboRect = comboObj.GetComponent<RectTransform>();
            comboRect.anchorMin = new Vector2(0.2f, 0.12f);
            comboRect.anchorMax = new Vector2(0.8f, 0.38f);
            comboRect.offsetMin = Vector2.zero;
            comboRect.offsetMax = Vector2.zero;
            Text comboText = comboObj.AddComponent<Text>();
            comboText.text = "COMBO x2 (x2)";
            comboText.font = defaultFont;
            comboText.fontSize = 28;
            comboText.fontStyle = FontStyle.BoldAndItalic;
            comboText.alignment = TextAnchor.MiddleCenter;
            comboText.color = new Color(1f, 0.6f, 0f);
            comboObj.SetActive(false);

            // 4.2.5 Pollution Bar Slider
            GameObject polBarObj = CreateUIElement("Pollution_Bar", hudTop.transform);
            RectTransform polRect = polBarObj.GetComponent<RectTransform>();
            polRect.anchorMin = new Vector2(0.05f, 0.02f);
            polRect.anchorMax = new Vector2(0.95f, 0.20f);
            polRect.offsetMin = Vector2.zero;
            polRect.offsetMax = Vector2.zero;

            Slider polSlider = polBarObj.AddComponent<Slider>();
            polSlider.minValue = 0f;
            polSlider.maxValue = 100f;
            polSlider.value = 0f;

            GameObject polBg = CreateUIElement("Background", polBarObj.transform);
            SetRectStretched(polBg.GetComponent<RectTransform>());
            Image polBgImg = polBg.AddComponent<Image>();
            polBgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            GameObject polFillArea = CreateUIElement("Fill Area", polBarObj.transform);
            SetRectStretched(polFillArea.GetComponent<RectTransform>());

            GameObject polFill = CreateUIElement("Fill", polFillArea.transform);
            SetRectStretched(polFill.GetComponent<RectTransform>());
            Image polFillImg = polFill.AddComponent<Image>();
            polFillImg.color = new Color(0.1f, 0.9f, 0.3f);

            polSlider.fillRect = polFill.GetComponent<RectTransform>();
            polSlider.targetGraphic = polFillImg;

            // 4.2.6 Boss Health Bar (Slider)
            GameObject bossBarObj = CreateUIElement("Boss_Health_Bar", hudTop.transform);
            RectTransform bossBarRect = bossBarObj.GetComponent<RectTransform>();
            bossBarRect.anchorMin = new Vector2(0.05f, 0.02f);
            bossBarRect.anchorMax = new Vector2(0.95f, 0.20f);
            bossBarRect.offsetMin = Vector2.zero;
            bossBarRect.offsetMax = Vector2.zero;

            Slider bossSlider = bossBarObj.AddComponent<Slider>();
            bossSlider.minValue = 0f;
            bossSlider.maxValue = 100f;
            bossSlider.value = 100f;

            GameObject bossBg = CreateUIElement("Background", bossBarObj.transform);
            SetRectStretched(bossBg.GetComponent<RectTransform>());
            Image bossBgImg = bossBg.AddComponent<Image>();
            bossBgImg.color = new Color(0.3f, 0.1f, 0.1f, 0.85f);

            GameObject bossFillArea = CreateUIElement("Fill Area", bossBarObj.transform);
            SetRectStretched(bossFillArea.GetComponent<RectTransform>());

            GameObject bossFill = CreateUIElement("Fill", bossFillArea.transform);
            SetRectStretched(bossFill.GetComponent<RectTransform>());
            Image bossFillImg = bossFill.AddComponent<Image>();
            bossFillImg.color = new Color(0.95f, 0.2f, 0.2f);

            bossSlider.fillRect = bossFill.GetComponent<RectTransform>();
            bossSlider.targetGraphic = bossFillImg;

            GameObject bossHpTextObj = CreateUIElement("Boss_HP_Text", bossBarObj.transform);
            SetRectStretched(bossHpTextObj.GetComponent<RectTransform>());
            Text bossHpText = bossHpTextObj.AddComponent<Text>();
            bossHpText.text = "SR. BASURA: 100 / 100 HP";
            bossHpText.font = defaultFont;
            bossHpText.fontSize = 20;
            bossHpText.fontStyle = FontStyle.Bold;
            bossHpText.alignment = TextAnchor.MiddleCenter;
            bossHpText.color = Color.white;

            BossHealthBar bossHealthBarComp = bossBarObj.AddComponent<BossHealthBar>();
            bossBarObj.SetActive(false); // Oculto por defecto

            // 4.3 Characters (Vero & Sr. Basura Avatars)
            GameObject veroAvatar = CreateUIElement("Vero_Avatar", canvasObj.transform);
            RectTransform veroRect = veroAvatar.GetComponent<RectTransform>();
            veroRect.anchorMin = new Vector2(0.02f, 0.65f);
            veroRect.anchorMax = new Vector2(0.25f, 0.78f);
            veroRect.offsetMin = Vector2.zero;
            veroRect.offsetMax = Vector2.zero;
            Image veroImg = veroAvatar.AddComponent<Image>();
            veroImg.color = new Color(0.2f, 0.8f, 0.4f, 0.8f);

            GameObject bossAvatar = CreateUIElement("Boss_Avatar", canvasObj.transform);
            RectTransform bossRect = bossAvatar.GetComponent<RectTransform>();
            bossRect.anchorMin = new Vector2(0.75f, 0.65f);
            bossRect.anchorMax = new Vector2(0.98f, 0.78f);
            bossRect.offsetMin = Vector2.zero;
            bossRect.offsetMax = Vector2.zero;
            Image bossImg = bossAvatar.AddComponent<Image>();
            bossImg.color = new Color(0.8f, 0.2f, 0.3f, 0.8f);
            bossAvatar.SetActive(false);

            // 4.4 Waste Spawner Area (Center Area)
            GameObject spawnerObj = CreateUIElement("Waste_Spawner", canvasObj.transform);
            RectTransform spawnerRect = spawnerObj.GetComponent<RectTransform>();
            spawnerRect.anchorMin = new Vector2(0.05f, 0.26f);
            spawnerRect.anchorMax = new Vector2(0.95f, 0.65f);
            spawnerRect.offsetMin = Vector2.zero;
            spawnerRect.offsetMax = Vector2.zero;
            WasteSpawner spawnerComp = spawnerObj.AddComponent<WasteSpawner>();

            // 4.5 Bins Container (Bottom Area)
            GameObject binsObj = CreateUIElement("Bins_Container", canvasObj.transform);
            RectTransform binsRect = binsObj.GetComponent<RectTransform>();
            binsRect.anchorMin = new Vector2(0.02f, 0.02f);
            binsRect.anchorMax = new Vector2(0.98f, 0.24f);
            binsRect.offsetMin = Vector2.zero;
            binsRect.offsetMax = Vector2.zero;

            HorizontalLayoutGroup hlg = binsObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            CreateBinUI(binsObj.transform, "Bin_Papel", WasteCategory.Paper, new Color(0.15f, 0.5f, 0.95f), "PAPEL", defaultFont);
            CreateBinUI(binsObj.transform, "Bin_Plastico", WasteCategory.Plastic, new Color(0.98f, 0.82f, 0.1f), "PLÁSTICO", defaultFont);
            CreateBinUI(binsObj.transform, "Bin_Vidrio", WasteCategory.Glass, new Color(0.15f, 0.82f, 0.35f), "VIDRIO", defaultFont);
            CreateBinUI(binsObj.transform, "Bin_Organico", WasteCategory.Organic, new Color(0.55f, 0.35f, 0.18f), "ORGÁNICO", defaultFont);
            CreateBinUI(binsObj.transform, "Bin_Electronico", WasteCategory.Electronic, new Color(0.92f, 0.22f, 0.25f), "ELECTRO", defaultFont);

            // 4.6 Modals (Victory / GameOver / Educational)
            GameObject modalsObj = CreateUIElement("Modals", canvasObj.transform);
            SetRectStretched(modalsObj.GetComponent<RectTransform>());

            // Victory Modal
            GameObject vicModal = CreateUIElement("Victory_Modal", modalsObj.transform);
            SetRectStretched(vicModal.GetComponent<RectTransform>());
            Image vicBg = vicModal.AddComponent<Image>();
            vicBg.color = new Color(0f, 0f, 0f, 0.88f);

            GameObject vicTitleObj = CreateUIElement("Victory_Title", vicModal.transform);
            RectTransform vicTitleRect = vicTitleObj.GetComponent<RectTransform>();
            vicTitleRect.anchorMin = new Vector2(0.1f, 0.65f);
            vicTitleRect.anchorMax = new Vector2(0.9f, 0.8f);
            vicTitleRect.offsetMin = Vector2.zero;
            vicTitleRect.offsetMax = Vector2.zero;
            Text vicTitle = vicTitleObj.AddComponent<Text>();
            vicTitle.text = "¡NIVEL COMPLETADO!\nCIUDAD LIMPIA";
            vicTitle.font = defaultFont;
            vicTitle.fontSize = 44;
            vicTitle.fontStyle = FontStyle.Bold;
            vicTitle.alignment = TextAnchor.MiddleCenter;
            vicTitle.color = new Color(0.2f, 1f, 0.4f);

            GameObject vicScoreObj = CreateUIElement("Victory_Score", vicModal.transform);
            RectTransform vicScoreRect = vicScoreObj.GetComponent<RectTransform>();
            vicScoreRect.anchorMin = new Vector2(0.1f, 0.52f);
            vicScoreRect.anchorMax = new Vector2(0.9f, 0.62f);
            vicScoreRect.offsetMin = Vector2.zero;
            vicScoreRect.offsetMax = Vector2.zero;
            Text vicScore = vicScoreObj.AddComponent<Text>();
            vicScore.text = "Puntaje: 0";
            vicScore.font = defaultFont;
            vicScore.fontSize = 36;
            vicScore.alignment = TextAnchor.MiddleCenter;
            vicScore.color = Color.white;

            GameObject vicAccObj = CreateUIElement("Victory_Accuracy", vicModal.transform);
            RectTransform vicAccRect = vicAccObj.GetComponent<RectTransform>();
            vicAccRect.anchorMin = new Vector2(0.1f, 0.42f);
            vicAccRect.anchorMax = new Vector2(0.9f, 0.50f);
            vicAccRect.offsetMin = Vector2.zero;
            vicAccRect.offsetMax = Vector2.zero;
            Text vicAcc = vicAccObj.AddComponent<Text>();
            vicAcc.text = "Precisión: 100%";
            vicAcc.font = defaultFont;
            vicAcc.fontSize = 32;
            vicAcc.alignment = TextAnchor.MiddleCenter;
            vicAcc.color = new Color(1f, 0.9f, 0.3f);

            GameObject restartBtnObj = CreateButton("Restart_Button", vicModal.transform, "JUGAR DE NUEVO", defaultFont);
            RectTransform restartBtnRect = restartBtnObj.GetComponent<RectTransform>();
            restartBtnRect.anchorMin = new Vector2(0.2f, 0.22f);
            restartBtnRect.anchorMax = new Vector2(0.8f, 0.34f);
            restartBtnRect.offsetMin = Vector2.zero;
            restartBtnRect.offsetMax = Vector2.zero;

            vicModal.SetActive(false);

            // GameOver Modal
            GameObject goModal = CreateUIElement("GameOver_Modal", modalsObj.transform);
            SetRectStretched(goModal.GetComponent<RectTransform>());
            Image goBg = goModal.AddComponent<Image>();
            goBg.color = new Color(0.15f, 0f, 0f, 0.92f);

            GameObject goTitleObj = CreateUIElement("GameOver_Title", goModal.transform);
            RectTransform goTitleRect = goTitleObj.GetComponent<RectTransform>();
            goTitleRect.anchorMin = new Vector2(0.1f, 0.65f);
            goTitleRect.anchorMax = new Vector2(0.9f, 0.8f);
            goTitleRect.offsetMin = Vector2.zero;
            goTitleRect.offsetMax = Vector2.zero;
            Text goTitle = goTitleObj.AddComponent<Text>();
            goTitle.text = "¡CIUDAD CONTAMINADA!\nFIN DEL JUEGO";
            goTitle.font = defaultFont;
            goTitle.fontSize = 44;
            goTitle.fontStyle = FontStyle.Bold;
            goTitle.alignment = TextAnchor.MiddleCenter;
            goTitle.color = new Color(1f, 0.2f, 0.2f);

            GameObject goScoreObj = CreateUIElement("GameOver_Score", goModal.transform);
            RectTransform goScoreRect = goScoreObj.GetComponent<RectTransform>();
            goScoreRect.anchorMin = new Vector2(0.1f, 0.52f);
            goScoreRect.anchorMax = new Vector2(0.9f, 0.62f);
            goScoreRect.offsetMin = Vector2.zero;
            goScoreRect.offsetMax = Vector2.zero;
            Text goScore = goScoreObj.AddComponent<Text>();
            goScore.text = "Puntaje Final: 0";
            goScore.font = defaultFont;
            goScore.fontSize = 36;
            goScore.alignment = TextAnchor.MiddleCenter;
            goScore.color = Color.white;

            GameObject retryBtnObj = CreateButton("Retry_Button", goModal.transform, "REINTENTAR", defaultFont);
            RectTransform retryBtnRect = retryBtnObj.GetComponent<RectTransform>();
            retryBtnRect.anchorMin = new Vector2(0.2f, 0.22f);
            retryBtnRect.anchorMax = new Vector2(0.8f, 0.34f);
            retryBtnRect.offsetMin = Vector2.zero;
            retryBtnRect.offsetMax = Vector2.zero;

            goModal.SetActive(false);

            // Educational Modal
            GameObject eduModal = CreateUIElement("Educational_Popup", modalsObj.transform);
            SetRectStretched(eduModal.GetComponent<RectTransform>());
            Image eduBg = eduModal.AddComponent<Image>();
            eduBg.color = new Color(0.05f, 0.12f, 0.2f, 0.95f);

            GameObject eduTitleObj = CreateUIElement("Edu_Title", eduModal.transform);
            RectTransform eduTitleRect = eduTitleObj.GetComponent<RectTransform>();
            eduTitleRect.anchorMin = new Vector2(0.1f, 0.72f);
            eduTitleRect.anchorMax = new Vector2(0.9f, 0.85f);
            eduTitleRect.offsetMin = Vector2.zero;
            eduTitleRect.offsetMax = Vector2.zero;
            Text eduTitle = eduTitleObj.AddComponent<Text>();
            eduTitle.text = "¿SABÍAS QUE?";
            eduTitle.font = defaultFont;
            eduTitle.fontSize = 40;
            eduTitle.fontStyle = FontStyle.Bold;
            eduTitle.alignment = TextAnchor.MiddleCenter;
            eduTitle.color = new Color(1f, 0.85f, 0.2f);

            GameObject eduTagObj = CreateUIElement("Edu_Tag", eduModal.transform);
            RectTransform eduTagRect = eduTagObj.GetComponent<RectTransform>();
            eduTagRect.anchorMin = new Vector2(0.1f, 0.62f);
            eduTagRect.anchorMax = new Vector2(0.9f, 0.70f);
            eduTagRect.offsetMin = Vector2.zero;
            eduTagRect.offsetMax = Vector2.zero;
            Text eduTag = eduTagObj.AddComponent<Text>();
            eduTag.text = "ODS 11: Ciudades Sostenibles";
            eduTag.font = defaultFont;
            eduTag.fontSize = 26;
            eduTag.fontStyle = FontStyle.Bold;
            eduTag.alignment = TextAnchor.MiddleCenter;
            eduTag.color = new Color(0.3f, 0.85f, 1f);

            GameObject eduContentObj = CreateUIElement("Edu_Content", eduModal.transform);
            RectTransform eduContentRect = eduContentObj.GetComponent<RectTransform>();
            eduContentRect.anchorMin = new Vector2(0.1f, 0.35f);
            eduContentRect.anchorMax = new Vector2(0.9f, 0.60f);
            eduContentRect.offsetMin = Vector2.zero;
            eduContentRect.offsetMax = Vector2.zero;
            Text eduContent = eduContentObj.AddComponent<Text>();
            eduContent.text = "En Huancayo se generan más de 200 toneladas de residuos al día. ¡Clasificar en casa protege nuestra ciudad!";
            eduContent.font = defaultFont;
            eduContent.fontSize = 28;
            eduContent.alignment = TextAnchor.MiddleCenter;
            eduContent.color = Color.white;

            GameObject eduCloseBtnObj = CreateButton("Edu_Close_Button", eduModal.transform, "¡ENTENDIDO!", defaultFont);
            RectTransform eduCloseBtnRect = eduCloseBtnObj.GetComponent<RectTransform>();
            eduCloseBtnRect.anchorMin = new Vector2(0.2f, 0.18f);
            eduCloseBtnRect.anchorMax = new Vector2(0.8f, 0.28f);
            eduCloseBtnRect.offsetMin = Vector2.zero;
            eduCloseBtnRect.offsetMax = Vector2.zero;

            eduModal.SetActive(false);

            // 5. MANAGERS
            GameObject managersObj = new GameObject("--- MANAGERS ---");

            GameManager gm = managersObj.AddComponent<GameManager>();
            ScoreManager sm = managersObj.AddComponent<ScoreManager>();
            LevelTimer lt = managersObj.AddComponent<LevelTimer>();
            PollutionBar pb = managersObj.AddComponent<PollutionBar>();
            ComboSystem cs = managersObj.AddComponent<ComboSystem>();
            EndGameUI eg = managersObj.AddComponent<EndGameUI>();
            WorldMapManager wmm = managersObj.AddComponent<WorldMapManager>();
            EnvironmentController ec = managersObj.AddComponent<EnvironmentController>();
            AudioManager am = managersObj.AddComponent<AudioManager>();
            VisualJuiceEffects vje = managersObj.AddComponent<VisualJuiceEffects>();
            PowerUpManager pum = managersObj.AddComponent<PowerUpManager>();
            SaveSystem ss = managersObj.AddComponent<SaveSystem>();
            GameDifficultyBalancer gdb = managersObj.AddComponent<GameDifficultyBalancer>();
            ObjectPooler op = managersObj.AddComponent<ObjectPooler>();
            BossController bc = managersObj.AddComponent<BossController>();
            CharacterControllerUI ccui = managersObj.AddComponent<CharacterControllerUI>();
            EducationalPopup eduComp = managersObj.AddComponent<EducationalPopup>();

            // Wire up Serialized Fields
            SerializedObject soSM = new SerializedObject(sm);
            soSM.FindProperty("scoreText").objectReferenceValue = scoreText;
            soSM.ApplyModifiedProperties();

            SerializedObject soLT = new SerializedObject(lt);
            soLT.FindProperty("timerText").objectReferenceValue = timerText;
            soLT.ApplyModifiedProperties();

            SerializedObject soPB = new SerializedObject(pb);
            soPB.FindProperty("pollutionSlider").objectReferenceValue = polSlider;
            soPB.FindProperty("fillImage").objectReferenceValue = polFillImg;
            soPB.ApplyModifiedProperties();

            SerializedObject soCS = new SerializedObject(cs);
            soCS.FindProperty("comboText").objectReferenceValue = comboText;
            soCS.ApplyModifiedProperties();

            SerializedObject soEG = new SerializedObject(eg);
            soEG.FindProperty("victoryModal").objectReferenceValue = vicModal;
            soEG.FindProperty("victoryScoreText").objectReferenceValue = vicScore;
            soEG.FindProperty("victoryAccuracyText").objectReferenceValue = vicAcc;
            soEG.FindProperty("gameOverModal").objectReferenceValue = goModal;
            soEG.FindProperty("gameOverScoreText").objectReferenceValue = goScore;
            soEG.FindProperty("victoryRestartButton").objectReferenceValue = restartBtnObj.GetComponent<Button>();
            soEG.FindProperty("gameOverRestartButton").objectReferenceValue = retryBtnObj.GetComponent<Button>();
            soEG.ApplyModifiedProperties();

            SerializedObject soEC = new SerializedObject(ec);
            soEC.FindProperty("environmentImage").objectReferenceValue = bgImg;
            soEC.FindProperty("districtTitleText").objectReferenceValue = titleText;
            soEC.ApplyModifiedProperties();

            SerializedObject soWS = new SerializedObject(spawnerComp);
            soWS.FindProperty("itemsContainer").objectReferenceValue = spawnerRect;
            soWS.ApplyModifiedProperties();

            SerializedObject soBHB = new SerializedObject(bossHealthBarComp);
            soBHB.FindProperty("healthSlider").objectReferenceValue = bossSlider;
            soBHB.FindProperty("fillImage").objectReferenceValue = bossFillImg;
            soBHB.FindProperty("hpText").objectReferenceValue = bossHpText;
            soBHB.FindProperty("containerTransform").objectReferenceValue = bossBarRect;
            soBHB.ApplyModifiedProperties();

            SerializedObject soBC = new SerializedObject(bc);
            soBC.FindProperty("bossHealthBar").objectReferenceValue = bossHealthBarComp;
            soBC.ApplyModifiedProperties();

            SerializedObject soCC = new SerializedObject(ccui);
            soCC.FindProperty("veroImage").objectReferenceValue = veroImg;
            soCC.FindProperty("bossImage").objectReferenceValue = bossImg;
            soCC.ApplyModifiedProperties();

            SerializedObject soEDU = new SerializedObject(eduComp);
            soEDU.FindProperty("popupPanel").objectReferenceValue = eduModal;
            soEDU.FindProperty("titleText").objectReferenceValue = eduTitle;
            soEDU.FindProperty("categoryTagText").objectReferenceValue = eduTag;
            soEDU.FindProperty("factContentText").objectReferenceValue = eduContent;
            soEDU.ApplyModifiedProperties();

            // Connect button listener for educational close
            Button eduBtn = eduCloseBtnObj.GetComponent<Button>();
            if (eduBtn != null)
            {
                eduBtn.onClick.AddListener(() => eduComp.ClosePopup());
            }

            // 6. Guardar escena
            if (!Directory.Exists("Assets/Scenes"))
            {
                Directory.CreateDirectory("Assets/Scenes");
            }
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            EditorSceneManager.OpenScene(SCENE_PATH);

            Debug.Log("[ReCiclo] ¡Escena construida y guardada exitosamente en Assets/Scenes/SampleScene.unity!");
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        private static void SetRectStretched(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void CreateBinUI(Transform parent, string name, WasteCategory category, Color color, string labelText, Font font)
        {
            GameObject binObj = CreateUIElement(name, parent);
            Image img = binObj.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = true;

            Outline outline = binObj.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(2, -2);

            GameObject textObj = CreateUIElement("Label", binObj.transform);
            SetRectStretched(textObj.GetComponent<RectTransform>());
            Text label = textObj.AddComponent<Text>();
            label.text = labelText;
            label.font = font;
            label.fontSize = 22;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;

            RecycleBin binComp = binObj.AddComponent<RecycleBin>();
            binComp.SetAcceptedCategory(category, color);
        }

        private static GameObject CreateButton(string name, Transform parent, string labelText, Font font)
        {
            GameObject btnObj = CreateUIElement(name, parent);
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.7f, 0.3f);
            Button btn = btnObj.AddComponent<Button>();

            GameObject textObj = CreateUIElement("Text", btnObj.transform);
            SetRectStretched(textObj.GetComponent<RectTransform>());
            Text text = textObj.AddComponent<Text>();
            text.text = labelText;
            text.font = font;
            text.fontSize = 28;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            return btnObj;
        }
    }
}
#endif
// Trigger compile 17/09/2026 04:41:00 p. m.
