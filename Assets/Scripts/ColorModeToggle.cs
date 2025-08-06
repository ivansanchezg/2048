using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class ColorModeToggle : MonoBehaviour
{
    public Toggle toggle;
    public RectTransform handle;
    public Vector2 onPosition = new Vector2(-15, 0);
    public Vector2 offPosition = new Vector2(15, 0);
    public float duration = 0.2f;


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
                Vector3 bounceDir = Vector3.right * (isOn ? -1 : 1); // bounce opposite to slide
                Tween.PunchLocalPosition(handle, strength: bounceDir * 5f, duration: 0.1f, frequency: 5f);
                GameSettings.instance.ToggleColorMode();
            });
    }
}
