using UnityEngine;
using UnityEngine.UI;
using ReCiclo.Sprint4;
using ReCiclo.Sprint6;

namespace ReCiclo.Sprint2
{
    public class EndGameUI : MonoBehaviour
    {
        public static EndGameUI Instance { get; private set; }

        [Header("Modal de Victoria")]
        [SerializeField] private GameObject victoryModal;
        [SerializeField] private Text victoryScoreText;
        [SerializeField] private Text victoryAccuracyText;
        [SerializeField] private GameObject[] starObjects;
        [SerializeField] private Sprite victoryVeroSprite;

        private Image victoryVeroImage;
        private Text victoryMessageText;
        private static int lastVictoryMessage = -1;

        private readonly string[] victoryMessages =
        {
            "Cada residuo en su lugar ayuda a mantener limpia nuestra ciudad.",
            "Reciclar una lata de aluminio ahorra hasta un 95% de la energia necesaria para fabricar otra.",
            "Una botella de plastico puede tardar cerca de 450 anos en degradarse.",
            "Separar papel, vidrio, plastico y organicos convierte residuos en nuevos recursos.",
            "Una ciudad limpia comienza con una pequena accion: reciclar correctamente.",
            "El vidrio puede reciclarse muchas veces sin perder su calidad.",
            "Recolectar y clasificar residuos protege el suelo, el agua y a los animales."
        };

        [Header("Modal de Derrota")]
        [SerializeField] private GameObject gameOverModal;
        [SerializeField] private Text gameOverScoreText;

        [Header("Botones")]
        [SerializeField] private Button victoryRestartButton;
        [SerializeField] private Button gameOverRestartButton;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (victoryRestartButton != null)
            {
                victoryRestartButton.onClick.AddListener(OnRestartButtonClicked);
            }

            if (gameOverRestartButton != null)
            {
                gameOverRestartButton.onClick.AddListener(OnRestartButtonClicked);
            }
        }

        public void ShowVictory(int finalScore, float accuracy)
        {
            if (victoryModal != null) victoryModal.SetActive(true);
            if (gameOverModal != null) gameOverModal.SetActive(false);

            EnsureVictoryPresentation();
            ShowRandomVictoryMessage();

            if (victoryScoreText != null) victoryScoreText.text = $"Puntaje: {finalScore}";
            if (victoryAccuracyText != null) victoryAccuracyText.text = $"Precisión: {accuracy:F1}%";

            int stars = 1;
            if (accuracy >= 85f && finalScore >= 1200) stars = 3;
            else if (accuracy >= 60f && finalScore >= 800) stars = 2;

            if (starObjects != null)
            {
                for (int i = 0; i < starObjects.Length; i++)
                {
                    if (starObjects[i] != null)
                    {
                        starObjects[i].SetActive(i < stars);
                    }
                }
            }

            // Persistencia en SaveSystem y WorldMapManager (Sprint 7)
            int currentLevel = (WorldMapManager.Instance != null) ? WorldMapManager.Instance.GetCurrentLevel().levelIndex : 1;
            if (WorldMapManager.Instance != null)
            {
                WorldMapManager.Instance.CompleteLevel(currentLevel, finalScore, accuracy);
            }
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SaveLevelResult(currentLevel, stars, finalScore);
            }
        }

        private void EnsureVictoryPresentation()
        {
            if (victoryModal == null) return;

            Transform content = victoryScoreText != null && victoryScoreText.transform.parent != null
                ? victoryScoreText.transform.parent
                : victoryModal.transform;

            Canvas rootCanvas = victoryModal.GetComponentInParent<Canvas>()?.rootCanvas;
            if (rootCanvas != null) rootCanvas.pixelPerfect = true;

            Canvas modalCanvas = victoryModal.GetComponent<Canvas>();
            if (modalCanvas == null) modalCanvas = victoryModal.AddComponent<Canvas>();
            modalCanvas.overrideSorting = true;
            modalCanvas.sortingOrder = 1000;
            modalCanvas.pixelPerfect = true;

            if (victoryVeroImage == null)
            {
                GameObject imageObject = new GameObject("VictoryVero", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                imageObject.transform.SetParent(content, false);
                victoryVeroImage = imageObject.GetComponent<Image>();
                victoryVeroImage.raycastTarget = false;
                victoryVeroImage.preserveAspect = true;

                RectTransform rect = imageObject.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(-190f, 20f);
                rect.sizeDelta = new Vector2(190f, 190f);
            }

            victoryVeroImage.sprite = victoryVeroSprite;
            victoryVeroImage.color = Color.white;

            if (victoryMessageText == null)
            {
                GameObject textObject = new GameObject("VictoryEcoMessage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline));
                textObject.transform.SetParent(content, false);
                victoryMessageText = textObject.GetComponent<Text>();
                victoryMessageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                victoryMessageText.fontSize = 28;
                victoryMessageText.fontStyle = FontStyle.Bold;
                victoryMessageText.alignment = TextAnchor.MiddleCenter;
                victoryMessageText.color = Color.white;
                victoryMessageText.horizontalOverflow = HorizontalWrapMode.Wrap;
                victoryMessageText.verticalOverflow = VerticalWrapMode.Truncate;
                victoryMessageText.resizeTextForBestFit = true;
                victoryMessageText.resizeTextMinSize = 22;
                victoryMessageText.resizeTextMaxSize = 28;
                victoryMessageText.raycastTarget = false;

                Outline outline = textObject.GetComponent<Outline>();
                outline.effectColor = new Color(0.02f, 0.08f, 0.04f, 0.95f);
                outline.effectDistance = new Vector2(1f, -1f);

                RectTransform rect = textObject.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(105f, 20f);
                rect.sizeDelta = new Vector2(370f, 190f);
            }
        }

        private void ShowRandomVictoryMessage()
        {
            if (victoryMessageText == null || victoryMessages.Length == 0) return;

            int index = Random.Range(0, victoryMessages.Length);
            if (victoryMessages.Length > 1 && index == lastVictoryMessage)
            {
                index = (index + Random.Range(1, victoryMessages.Length)) % victoryMessages.Length;
            }

            lastVictoryMessage = index;
            victoryMessageText.text = victoryMessages[index];
        }

        public void ShowGameOver(int finalScore)
        {
            if (gameOverModal != null) gameOverModal.SetActive(true);
            if (victoryModal != null) victoryModal.SetActive(false);

            if (gameOverScoreText != null) gameOverScoreText.text = $"Puntaje Final: {finalScore}";
        }

        public void OnRestartButtonClicked()
        {
            if (victoryModal != null) victoryModal.SetActive(false);
            if (gameOverModal != null) gameOverModal.SetActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
        }
    }
}
