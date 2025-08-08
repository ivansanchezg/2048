using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class ColorModeToggle : MonoBehaviour
{
    [Header("Main")]
    [SerializeField] Toggle toggle;
    [SerializeField] RectTransform handle;
    [SerializeField] Vector2 onPosition;
    [SerializeField] Vector2 offPosition;
    [SerializeField] float duration;

    [Header("Bounce")]
    [SerializeField] float strength;
    [SerializeField] float bounceDuration;
    [SerializeField] float frequency;

    void Start()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);
        handle.anchoredPosition = toggle.isOn ? onPosition : offPosition;
    }

    void OnToggleChanged(bool isOn)
    {
        Tween.UIAnchoredPosition(handle, isOn ? onPosition : offPosition, duration, Ease.OutCubic)
            .OnComplete(() =>
            {
                // Apply bounce after slide completes
                Vector3 bounceDirection = Vector3.right * (isOn ? -1 : 1); // bounce opposite to slide
                Tween.PunchLocalPosition(
                    handle,
                    strength: bounceDirection * strength,
                    duration: bounceDuration,
                    frequency: frequency
                );
                GameSettings.instance.ToggleColorMode();
            });
    }
}
