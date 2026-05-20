using System.Collections.Generic;
using UnityEngine;

public enum ImagePosition { Izquierda, Derecha, Centro }
public enum PanelType { Panel1, Panel2 }

[System.Serializable]
public struct Choice
{
    public string buttonText;
    public DialogueScript nextDialogue;
    public Sprite buttonBackground;

    [Header("Cambio de Escena (Opcional)")]
    public bool changeScene;
    public string sceneToLoad;
}

[System.Serializable]
public struct Line
{
    public string speaker;
    [TextArea(3, 5)]
    public string text;

    [Header("Configuración de Imagen de Personaje")]
    public bool showImage;
    public Sprite characterImage;
    public ImagePosition imagePosition;
    public Vector2 characterImageSize;

    [Header("Configuración del Panel de Diálogo")]
    public PanelType panelType;

    [Header("Configuración de Fondo (Opcional)")]
    public Sprite newBackground; // <-- ¡NUEVO! Si pones un Sprite aquí, el fondo del juego cambiará en este texto.

    [Header("Efectos Especiales")]
    public bool triggerShake;
    public float shakeDuration;
    public float shakeIntensity;

    [Header("Decisiones (Opcional)")]
    public PanelType choicePanelType;
    public List<Choice> choices;
}

[CreateAssetMenu(fileName = "NewConversation", menuName = "Dialogues/Conversation")]
public class DialogueScript : ScriptableObject
{
    [Header("Configuración Global de la Conversación")]
    public PanelType defaultPanelType;

    [Header("Continuación Automática (Opcional)")]
    public DialogueScript nextConversation;

    [Header("Cambio de Escena al Terminar (Opcional)")]
    public bool changeSceneOnEnd; // <-- ¡NUEVO! ¿Quieres cambiar de escena al finalizar todo el diálogo?
    public string sceneToLoadOnEnd; // <-- ¡NUEVO! Nombre de la escena final.

    [Space(10)]
    public List<Line> conversationLines;
}