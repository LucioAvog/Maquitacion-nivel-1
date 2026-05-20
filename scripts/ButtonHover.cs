using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    private bool scaleSaved = false; 

    [Header("Configuración de Tamaño")]
    [SerializeField] private float scaleMultiplier = 1.1f;

    void Awake()
    {
        originalScale = transform.localScale;

        if (originalScale.magnitude < 0.01f)
        {
            originalScale = Vector3.one;
        }

        scaleSaved = true; 
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (scaleSaved)
        {
            transform.localScale = originalScale * scaleMultiplier;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (scaleSaved)
        {
            transform.localScale = originalScale;
        }
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        if (scaleSaved)
        {
            transform.localScale = originalScale;
        }
    }
}