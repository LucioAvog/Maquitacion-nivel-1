using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    [System.Serializable]
    public struct ChoicePanelGroup
    {
        public GameObject choicePanel;
        public Button buttonA, buttonB, buttonC;
        public TextMeshProUGUI textA, textB, textC;

        [HideInInspector] public Sprite defaultSpriteA, defaultSpriteB, defaultSpriteC;

        public void InitializeDefaults()
        {
            if (buttonA != null) defaultSpriteA = buttonA.image.sprite;
            if (buttonB != null) defaultSpriteB = buttonB.image.sprite;
            if (buttonC != null) defaultSpriteC = buttonC.image.sprite;
        }

        public void SetActive(bool active)
        {
            if (choicePanel != null) choicePanel.SetActive(active);
        }
    }

    [Header("UI General")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI lineText;
    [SerializeField] private TextMeshProUGUI lineText2;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject dialoguePanel2;
    [SerializeField] private Image gameBackgroundDisplay;

    [Header("Configuración de Texto Animado")]
    [SerializeField] private float textSpeed = 0.03f;
    private Coroutine typingCoroutine;
    private string fullTextOfCurrentLine;
    private TextMeshProUGUI activeLineText;

    [Header("Imagen del Personaje")]
    [SerializeField] private Image characterImageDisplay;
    [SerializeField] private RectTransform characterImageRect;
    [SerializeField] private Vector2 positionLeft = new Vector2(-350, -90);
    [SerializeField] private Vector2 positionCenter = new Vector2(0, -90);
    [SerializeField] private Vector2 positionRight = new Vector2(350, -90);

    [Header("Efectos Especiales - Temblor")]
    [SerializeField] private Transform objectToShake;
    private Coroutine activeShakeCoroutine;

    [Header("Paneles de Decisiones Separados")]
    [SerializeField] private ChoicePanelGroup choicesPanel1;
    [SerializeField] private ChoicePanelGroup choicesPanel2;

    private bool dialogueActive;
    private bool awaitingChoice;
    private Queue<Line> lines = new Queue<Line>();

    private DialogueScript currentConversation;
    private Vector2 defaultCharacterSize;

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialoguePanel2 != null) dialoguePanel2.SetActive(false);
        if (characterImageDisplay != null) characterImageDisplay.gameObject.SetActive(false);

        if (characterImageRect != null) defaultCharacterSize = characterImageRect.sizeDelta;

        choicesPanel1.InitializeDefaults();
        choicesPanel2.InitializeDefaults();

        choicesPanel1.SetActive(false);
        choicesPanel2.SetActive(false);
    }

    public void AdvanceDialogue()
    {
        if (dialogueActive && !awaitingChoice)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
                if (activeLineText != null) activeLineText.text = fullTextOfCurrentLine;
            }
            else
            {
                ProduceNextLine();
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AdvanceDialogue();
        }
    }

    public void GetConversation(DialogueScript dialogue)
    {
        if (dialogueActive) return;
        StartDialogueSequence(dialogue);
    }

    private void StartDialogueSequence(DialogueScript dialogue)
    {
        currentConversation = dialogue;

        lines.Clear();
        foreach (var line in dialogue.conversationLines) lines.Enqueue(line);

        dialogueActive = true;
        awaitingChoice = false;

        choicesPanel1.SetActive(false);
        choicesPanel2.SetActive(false);

        ProduceNextLine();
    }

    void ProduceNextLine()
    {
        if (lines.Count == 0)
        {
            if (currentConversation != null && currentConversation.nextConversation != null)
            {
                StartDialogueSequence(currentConversation.nextConversation);
            }
            else if (currentConversation != null && currentConversation.changeSceneOnEnd && !string.IsNullOrEmpty(currentConversation.sceneToLoadOnEnd))
            {
                SceneManager.LoadScene(currentConversation.sceneToLoadOnEnd);
            }
            else
            {
                EndDialogue();
            }
            return;
        }

        Line currentLine = lines.Dequeue();
        if (nameText != null) nameText.text = currentLine.speaker;

        if (currentLine.newBackground != null && gameBackgroundDisplay != null)
        {
            gameBackgroundDisplay.sprite = currentLine.newBackground;
        }

        if (dialoguePanel != null && dialoguePanel2 != null)
        {
            dialoguePanel.SetActive(currentLine.panelType == PanelType.Panel1);
            dialoguePanel2.SetActive(currentLine.panelType == PanelType.Panel2);
            activeLineText = currentLine.panelType == PanelType.Panel1 ? lineText : lineText2;
        }

        fullTextOfCurrentLine = currentLine.text;
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(fullTextOfCurrentLine));

        if (currentLine.triggerShake)
        {
            if (activeShakeCoroutine != null) StopCoroutine(activeShakeCoroutine);
            float duration = currentLine.shakeDuration > 0 ? currentLine.shakeDuration : 0.5f;
            float intensity = currentLine.shakeIntensity > 0 ? currentLine.shakeIntensity : 10f;
            activeShakeCoroutine = StartCoroutine(ScreenShakeRoutine(duration, intensity));
        }

        HandleCharacterImage(currentLine);

        choicesPanel1.SetActive(false);
        choicesPanel2.SetActive(false);

        if (currentLine.choices != null && currentLine.choices.Count > 0)
        {
            SetupChoices(currentLine.choices, currentLine.choicePanelType);
        }
        else
        {
            awaitingChoice = false;
        }
    }

    private IEnumerator TypeText(string textToType)
    {
        if (activeLineText == null) yield break;

        activeLineText.text = "";
        foreach (char letter in textToType.ToCharArray())
        {
            activeLineText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }
        typingCoroutine = null;
    }

    private IEnumerator ScreenShakeRoutine(float duration, float intensity)
    {
        if (objectToShake == null) yield break;

        Vector3 originalPosition = objectToShake.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            objectToShake.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        objectToShake.localPosition = originalPosition;
        activeShakeCoroutine = null;
    }

    private void HandleCharacterImage(Line currentLine)
    {
        if (characterImageDisplay == null) return;

        if (currentLine.showImage && currentLine.characterImage != null)
        {
            characterImageDisplay.gameObject.SetActive(true);
            characterImageDisplay.sprite = currentLine.characterImage;

            // NUEVO: Estas dos líneas obligan al componente de Unity a adaptarse a las dimensiones originales del Sprite
            characterImageDisplay.SetNativeSize();
            characterImageDisplay.preserveAspect = true;

            if (characterImageRect != null)
            {
                // Si definiste un tamaño manual en el DialogueScript, lo aplica sin perder proporción.
                // Si lo dejaste en 0,0, mantiene el tamaño original del Sprite.
                if (currentLine.characterImageSize != Vector2.zero)
                {
                    characterImageRect.sizeDelta = currentLine.characterImageSize;
                }

                switch (currentLine.imagePosition)
                {
                    case ImagePosition.Izquierda:
                        characterImageRect.anchoredPosition = positionLeft;
                        break;
                    case ImagePosition.Centro:
                        characterImageRect.anchoredPosition = positionCenter;
                        break;
                    case ImagePosition.Derecha:
                        characterImageRect.anchoredPosition = positionRight;
                        break;
                }
            }
        }
        else
        {
            characterImageDisplay.gameObject.SetActive(false);
        }
    }

    void SetupChoices(List<Choice> choices, PanelType panelType)
    {
        awaitingChoice = true;

        ChoicePanelGroup activeGroup = (panelType == PanelType.Panel1) ? choicesPanel1 : choicesPanel2;

        activeGroup.SetActive(true);

        if (activeGroup.buttonA != null) activeGroup.buttonA.gameObject.SetActive(false);
        if (activeGroup.buttonB != null) activeGroup.buttonB.gameObject.SetActive(false);
        if (activeGroup.buttonC != null) activeGroup.buttonC.gameObject.SetActive(false);

        if (activeGroup.buttonA != null) activeGroup.buttonA.onClick.RemoveAllListeners();
        if (activeGroup.buttonB != null) activeGroup.buttonB.onClick.RemoveAllListeners();
        if (activeGroup.buttonC != null) activeGroup.buttonC.onClick.RemoveAllListeners();

        if (choices.Count > 0 && activeGroup.buttonA != null)
        {
            activeGroup.buttonA.gameObject.SetActive(true);
            if (activeGroup.textA != null) activeGroup.textA.text = choices[0].buttonText;
            activeGroup.buttonA.image.sprite = choices[0].buttonBackground != null ? choices[0].buttonBackground : activeGroup.defaultSpriteA;
            Choice choiceA = choices[0];
            activeGroup.buttonA.onClick.AddListener(() => OnChoiceSelected(choiceA));
        }

        if (choices.Count > 1 && activeGroup.buttonB != null)
        {
            activeGroup.buttonB.gameObject.SetActive(true);
            if (activeGroup.textB != null) activeGroup.textB.text = choices[1].buttonText;
            activeGroup.buttonB.image.sprite = choices[1].buttonBackground != null ? choices[1].buttonBackground : activeGroup.defaultSpriteB;
            Choice choiceB = choices[1];
            activeGroup.buttonB.onClick.AddListener(() => OnChoiceSelected(choiceB));
        }

        if (choices.Count > 2 && activeGroup.buttonC != null)
        {
            activeGroup.buttonC.gameObject.SetActive(true);
            if (activeGroup.textC != null) activeGroup.textC.text = choices[2].buttonText;
            activeGroup.buttonC.image.sprite = choices[2].buttonBackground != null ? choices[2].buttonBackground : activeGroup.defaultSpriteC;
            Choice choiceC = choices[2];
            activeGroup.buttonC.onClick.AddListener(() => OnChoiceSelected(choiceC));
        }
    }

    void OnChoiceSelected(Choice selectedChoice)
    {
        awaitingChoice = false;
        choicesPanel1.SetActive(false);
        choicesPanel2.SetActive(false);

        if (selectedChoice.changeScene && !string.IsNullOrEmpty(selectedChoice.sceneToLoad))
        {
            SceneManager.LoadScene(selectedChoice.sceneToLoad);
            return;
        }

        if (selectedChoice.nextDialogue != null)
        {
            StartDialogueSequence(selectedChoice.nextDialogue);
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialogueActive = false;
        awaitingChoice = false;
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialoguePanel2 != null) dialoguePanel2.SetActive(false);

        choicesPanel1.SetActive(false);
        choicesPanel2.SetActive(false);

        if (characterImageDisplay != null) characterImageDisplay.gameObject.SetActive(false);
        Debug.Log("Diálogo terminado.");
    }
}