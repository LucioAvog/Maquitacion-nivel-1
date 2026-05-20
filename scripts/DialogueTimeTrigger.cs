using System.Collections;
using UnityEngine;

public class DialogueTimeTrigger : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueScript dialogueToTrigger;

    [Header("Configuración de Tiempo")]
    [SerializeField] private float delayInSeconds = 3f; // Tiempo de espera en segundos
    [SerializeField] private bool triggerOnStart = true; // ¿Arranca el temporizador al iniciar la escena?

    void Start()
    {
        // Si no lo asignaste, intenta buscarlo automáticamente en la escena
        if (dialogueManager == null)
        {
            dialogueManager = Object.FindAnyObjectByType<DialogueManager>();
        }

        if (triggerOnStart)
        {
            StartTimer();
        }
    }

    // Puedes llamar a este método desde otros scripts si quieres iniciar el conteo manualmente
    public void StartTimer()
    {
        if (dialogueToTrigger != null)
        {
            StartCoroutine(WaitAndTriggerRoutine());
        }
    }

    private IEnumerator WaitAndTriggerRoutine()
    {
        // Espera la cantidad de segundos configurada
        yield return new WaitForSeconds(delayInSeconds);

        if (dialogueManager != null)
        {
            dialogueManager.GetConversation(dialogueToTrigger);
        }
        else
        {
            Debug.LogError("No se puede iniciar el diálogo porque falta el DialogueManager.");
        }
    }
}