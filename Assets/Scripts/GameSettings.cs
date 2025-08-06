using System;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings instance { get; private set; }

    public ColorMode colorMode { get; private set; }

    public Action colorModeChanged;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void Start()
    {
        colorMode = ColorMode.Light;
    }

    public void ToggleColorMode()
    {
        if (colorMode == ColorMode.Light)
        {
            colorMode = ColorMode.Dark;
        }
        else
        {
            colorMode = ColorMode.Light;
        }

        colorModeChanged?.Invoke();
    }
}

public enum ColorMode
{
    Light,
    Dark,
}