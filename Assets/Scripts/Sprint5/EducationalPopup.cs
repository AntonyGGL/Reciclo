using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ReCiclo.Sprint5
{
    [Serializable]
    public class EducationalFact
    {
        public string title;
        public string categoryTag;
        [TextArea(3, 6)] public string factText;
        public Sprite veroIllustration;
    }

    public class EducationalPopup : MonoBehaviour
    {
        public static EducationalPopup Instance { get; private set; }

        [Header("Referencias UI")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text categoryTagText;
        [SerializeField] private Text factContentText;
        [SerializeField] private Image veroImage;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button nextFactButton;
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Animacion")]
        [SerializeField] private float fadeInDuration = 0.4f;
        [SerializeField] private float autoCloseDelay = 0f;

        [Header("Banco de Datos ODS 11, 12 y Huancayo")]
        [SerializeField] private List<EducationalFact> factsDatabase = new List<EducationalFact>();

        private int currentFactIndex = 0;
        private Coroutine animRoutine;
        private List<int> shownFacts = new List<int>();

        public event Action OnPopupOpened;
        public event Action OnPopupClosed;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            InitializeDefaultFacts();
            if (popupPanel != null) popupPanel.SetActive(false);
        }

        private void Start()
        {
            if (closeButton != null) closeButton.onClick.AddListener(ClosePopup);
            if (nextFactButton != null) nextFactButton.onClick.AddListener(ShowNextFact);
        }

        private void InitializeDefaultFacts()
        {
            if (factsDatabase != null && factsDatabase.Count > 0) return;

            factsDatabase = new List<EducationalFact>
            {
                new EducationalFact
                {
                    title = "Dato Ambiental",
                    categoryTag = "ODS 11: Ciudades Sostenibles",
                    factText = "En Huancayo se generan mas de 200 toneladas de residuos solidos al dia. Clasificar el papel y carton reduce el volumen de basura en los botaderos locales."
                },
                new EducationalFact
                {
                    title = "Impacto del Plastico",
                    categoryTag = "ODS 12: Consumo Responsable",
                    factText = "Una sola botella de plastico tarda hasta 450 anos en degradarse. Al reciclarla en el contenedor amarillo le das una nueva vida util!"
                },
                new EducationalFact
                {
                    title = "Vidrio Infinitamente Reciclable",
                    categoryTag = "ODS 12: Produccion Responsable",
                    factText = "El vidrio se puede reciclar el 100% de las veces sin perder su calidad ni pureza. Reciclar 3 botellas de vidrio ahorra energia para cargar un smartphone durante un ano!"
                },
                new EducationalFact
                {
                    title = "Compostaje Organico",
                    categoryTag = "ODS 11: Comunidades Limpias",
                    factText = "Los restos de cascaras de frutas y vegetales arrojados en el contenedor marron pueden transformarse en abono organico para los parques de la ciudad."
                },
                new EducationalFact
                {
                    title = "Peligro Electronico",
                    categoryTag = "ODS 12: Gestion de Residuos",
                    factText = "Las pilas y baterias usadas contienen metales pesados como mercurio y plomo. Nunca deben mezclarse con la basura comun; van en el contenedor rojo especial."
                },
                new EducationalFact
                {
                    title = "Agua Limpia",
                    categoryTag = "ODS 6: Agua Limpia",
                    factText = "El Rio Mantaro en Huancayo recibe residuos industriales y domesticos. Al clasificar correctamente la basura, reducimos la contaminacion de fuentes hidricas."
                },
                new EducationalFact
                {
                    title = "Economia Circular",
                    categoryTag = "ODS 12: Produccion Sostenible",
                    factText = "Cada tonelada de papel reciclado ahorra 17 arboles, 26,500 litros de agua y 4,100 kWh de electricidad. El reciclaje es la base de la economia circular!"
                },
                new EducationalFact
                {
                    title = "Recicladores de Huancayo",
                    categoryTag = "ODS 11: Trabajo Digno",
                    factText = "En Huancayo hay mas de 300 recicladores formales que dependen de los materiales que separamos. Al clasificar correctamente, tambien apoyas su trabajo."
                },
                new EducationalFact
                {
                    title = "Cambio Climatico",
                    categoryTag = "ODS 13: Accion Climatica",
                    factText = "La descomposicion de residuos organicos en botaderos genera metano, un gas de efecto invernadero 28 veces mas potente que el CO2. El compostaje es la solucion!"
                },
                new EducationalFact
                {
                    title = "Microplasticos",
                    categoryTag = "ODS 14: Vida Marina",
                    factText = "Los plasticos no reciclados se fragmentan en microplasticos que contaminan rios y lagos. Estos llegan a los alimentos que consumimos. Reciclar salva vidas."
                }
            };
        }

        public void ShowRandomFact()
        {
            if (factsDatabase == null || factsDatabase.Count == 0) return;

            int index;
            if (shownFacts.Count >= factsDatabase.Count) shownFacts.Clear();

            do { index = UnityEngine.Random.Range(0, factsDatabase.Count); }
            while (shownFacts.Contains(index) && shownFacts.Count < factsDatabase.Count);

            shownFacts.Add(index);
            currentFactIndex = index;
            DisplayFact(factsDatabase[index]);
        }

        public void ShowNextFact()
        {
            currentFactIndex = (currentFactIndex + 1) % factsDatabase.Count;
            DisplayFact(factsDatabase[currentFactIndex]);
        }

        public void ShowFactByCategory(string category)
        {
            EducationalFact matchingFact = factsDatabase.Find(f => f.categoryTag.Contains(category));
            if (matchingFact != null) DisplayFact(matchingFact);
            else ShowRandomFact();
        }

        private void DisplayFact(EducationalFact fact)
        {
            if (titleText != null) titleText.text = fact.title;
            if (categoryTagText != null) categoryTagText.text = fact.categoryTag;
            if (factContentText != null) factContentText.text = fact.factText;
            if (veroImage != null && fact.veroIllustration != null) veroImage.sprite = fact.veroIllustration;

            if (animRoutine != null) StopCoroutine(animRoutine);
            animRoutine = StartCoroutine(ShowPopupAnimated());
        }

        private IEnumerator ShowPopupAnimated()
        {
            if (popupPanel != null) popupPanel.SetActive(true);
            OnPopupOpened?.Invoke();

            if (panelCanvasGroup == null) panelCanvasGroup = popupPanel != null ? popupPanel.GetComponent<CanvasGroup>() : null;
            if (panelCanvasGroup == null && popupPanel != null) panelCanvasGroup = popupPanel.AddComponent<CanvasGroup>();

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                RectTransform rt = popupPanel.GetComponent<RectTransform>();
                Vector3 targetScale = rt != null ? rt.localScale : Vector3.one;
                if (rt != null) rt.localScale = targetScale * 0.8f;

                float elapsed = 0f;
                while (elapsed < fadeInDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = elapsed / fadeInDuration;
                    float smoothT = t * t * (3f - 2f * t);
                    panelCanvasGroup.alpha = smoothT;
                    if (rt != null) rt.localScale = Vector3.Lerp(targetScale * 0.8f, targetScale, smoothT);
                    yield return null;
                }
                panelCanvasGroup.alpha = 1f;
                if (rt != null) rt.localScale = targetScale;
            }

            if (autoCloseDelay > 0f)
            {
                yield return new WaitForSecondsRealtime(autoCloseDelay);
                ClosePopup();
            }
        }

        public void ClosePopup()
        {
            if (animRoutine != null) StopCoroutine(animRoutine);
            animRoutine = StartCoroutine(ClosePopupAnimated());
        }

        private IEnumerator ClosePopupAnimated()
        {
            if (panelCanvasGroup != null)
            {
                float elapsed = 0f;
                float closeDuration = fadeInDuration * 0.6f;
                while (elapsed < closeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    panelCanvasGroup.alpha = 1f - (elapsed / closeDuration);
                    yield return null;
                }
            }

            if (popupPanel != null) popupPanel.SetActive(false);
            OnPopupClosed?.Invoke();
        }
    }
}