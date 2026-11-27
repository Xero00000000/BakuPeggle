using UnityEngine;

public enum SlideDirection { FromLeft, FromRight, FromTop, FromBottom }
public enum UIReaction { None, ShakeEnemy, PulsePlayer }

[CreateAssetMenu(fileName = "NewAbilityFeedback", menuName = "Feedback/Ability Feedback")]
public class AbilityFeedbackData : ScriptableObject
{
    [Header("Identity")]
    public Color color = Color.white;

    [Header("Cut-in")]
    public SlideDirection slideFrom = SlideDirection.FromLeft;
    public float slideInDuration = 0.12f;
    [Tooltip("How long the cut-in stays on screen. Keep it short so it doesn't block the board.")]
    public float holdTime = 0.35f;

    [Header("Cut-in outline")]
    public Color outlineColor = Color.black;
    [Tooltip("Outline thickness in screen pixels. 0 = no outline.")]
    public float outlineSize = 8f;



    [Header("Impact")]
    [Tooltip("Tiny beat of stillness right before the effect fires.")]
    public float anticipationPause = 0.05f;
    [Range(0f, 1f)] public float flashAlpha = 0.5f;
    public float flashDuration = 0.15f;

    [Tooltip("Camera shake strength (world units). 0 = none.")]
    public float shake = 0.12f;
    [Tooltip("Duration of the shake and of the speed lines.")]
    public float effectDuration = 0.3f;

    [Tooltip("Speed lines (the Custom/Damage shader).")]
    public bool useLines = true;
    [Range(0f, 1f)] public float lineIntensity = 1f;

    [Tooltip("Chromatic aberration spike. 0 = none.")]
    public float aberrationPeak = 0.6f;
    public float aberrationDuration = 0.35f;

    [Tooltip("Camera zoom-in punch as a fraction (0.04 = 4%). 0 = none.")]
    public float zoomPunch = 0.04f;
    public float zoomDuration = 0.2f;

    [Header("Aftermath")]
    public UIReaction uiReaction = UIReaction.None;
    public float uiShakePixels = 15f;
    public float uiDuration = 0.3f;
    [Tooltip("Delay between each regenerated peg popping in (Refresh ability).")]
    public float pegPopStagger = 0.02f;

    [Header("Sound")]
    public AudioClip introSound;
    public AudioClip impactSound;
}