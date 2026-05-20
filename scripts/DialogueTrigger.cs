using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Configuración del Diálogo")]
    [SerializeField] private DialogueScript myDialogue;

    [Header("Referencias")]
    // Al arrastrarlo aquí, el botón no tiene que "buscar" nada, ya sabe a dónde ir
    [SerializeField] private DialogueManager dialogueManager;

    public void TriggerDialogue()
    {
        if (dialogueManager != null && myDialogue != null)
        {
            dialogueManager.GetConversation(myDialogue);
        }
        else
        {
            Debug.LogWarning("¡Falta asignar el DialogueManager o el DialogueScript en el botón!");
        }
    }
}