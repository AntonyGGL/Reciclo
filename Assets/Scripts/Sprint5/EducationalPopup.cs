using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ReCiclo.Sprint5
{
    [System.Serializable]
    public class EducationalFact
    {
        public string title;
        public string categoryTag; // ej. ODS 11, ODS 12, Huancayo
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

        [Header("Banco de Datos ODS 11, 12 y Huancayo")]
        [SerializeField] private List<EducationalFact> factsDatabase = new List<EducationalFact>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeDefaultFacts();
        }

        private void InitializeDefaultFacts()
        {
            if (factsDatabase != null && factsDatabase.Count > 0) return;

            factsDatabase = new List<EducationalFact>
            {
                new EducationalFact
                {
                    title = "¿Sabías qué?",
                    categoryTag = "ODS 11: Ciudades Sostenibles",
                    factText = "En Huancayo se generan más de 200 toneladas de residuos sólidos al día. Clasificar el papel y cartón reduce el volumen de basura en los botaderos locales."
                },
                new EducationalFact
                {
                    title = "Impacto del Plástico",
                    categoryTag = "ODS 12: Consumo Responsable",
                    factText = "Una sola botella de plástico tarda hasta 450 años en degradarse. ¡Al reciclarla en el contenedor amarillo le das una nueva vida útil!"
                },
                new EducationalFact
                {
                    title = "Vidrio Infinitamente Reciclable",
                    categoryTag = "ODS 12: Producción Responsable",
                    factText = "El vidrio se puede reciclar el 100% de las veces sin perder su calidad ni pureza. ¡Reciclar 3 botellas de vidrio ahorra energía para cargar un smartphone durante un año!"
                },
                new EducationalFact
                {
                    title = "Compostaje Orgánico",
                    categoryTag = "ODS 11: Comunidades Limpias",
                    factText = "Los restos de cáscaras de frutas y vegetales arrojados en el contenedor marrón pueden transformarse en abono orgánico para los parques de la ciudad."
                },
                new EducationalFact
                {
                    title = "Peligro Electrónico",
                    categoryTag = "ODS 12: Gestión de Residuos",
                    factText = "Las pilas y baterías usadas contienen metales pesados como mercurio y plomo. Nunca deben mezclarse con la basura común; van en el contenedor rojo especial."
                }
            };
        }

        public void ShowRandomFact()
        {
            if (factsDatabase == null || factsDatabase.Count == 0) return;

            EducationalFact randomFact = factsDatabase[Random.Range(0, factsDatabase.Count)];
            DisplayFact(randomFact);
        }

        public void ShowFactByCategory(string category)
        {
            EducationalFact matchingFact = factsDatabase.Find(f => f.categoryTag.Contains(category));
            if (matchingFact != null)
            {
                DisplayFact(matchingFact);
            }
            else
            {
                ShowRandomFact();
            }
        }

        private void DisplayFact(EducationalFact fact)
        {
            if (popupPanel != null) popupPanel.SetActive(true);

            if (titleText != null) titleText.text = fact.title;
            if (categoryTagText != null) categoryTagText.text = fact.categoryTag;
            if (factContentText != null) factContentText.text = fact.factText;
            if (veroImage != null && fact.veroIllustration != null) veroImage.sprite = fact.veroIllustration;
        }

        public void ClosePopup()
        {
            if (popupPanel != null) popupPanel.SetActive(false);
        }
    }
}
