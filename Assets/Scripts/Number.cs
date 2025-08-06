using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Number : MonoBehaviour
{
    const float MAX_COLOR = 255f;

    Dictionary<int, NumberColors> lightColors = new Dictionary<int, NumberColors>
    {
        { 2, new(new Color(214f / MAX_COLOR, 236f / MAX_COLOR, 250f / MAX_COLOR), Color.black) }, // Powder Blue
        { 4, new(new Color(163f / MAX_COLOR, 213f / MAX_COLOR, 247f / MAX_COLOR), Color.black) }, // Sky Blue
        { 8, new(new Color(137f / MAX_COLOR, 197f / MAX_COLOR, 240f / MAX_COLOR), Color.black) }, // Baby Blue
        { 16, new(new Color(111f / MAX_COLOR, 183f / MAX_COLOR, 233f / MAX_COLOR), Color.black) }, // Cornflower Blue
        { 32, new(new Color(79f / MAX_COLOR, 165f / MAX_COLOR, 224f / MAX_COLOR), Color.black) }, // Capri
        { 64, new(new Color(63f / MAX_COLOR, 143f / MAX_COLOR, 196f / MAX_COLOR), Color.white) }, // Azure
        { 128, new(new Color(50f / MAX_COLOR, 115f / MAX_COLOR, 179f / MAX_COLOR), Color.white) }, // True Blue
        { 256, new(new Color(44f / MAX_COLOR, 111f / MAX_COLOR, 158f / MAX_COLOR), Color.white) }, // Steel Blue
        { 512, new(new Color(36f / MAX_COLOR, 92f / MAX_COLOR, 133f / MAX_COLOR), Color.white) }, // Denim
        { 1024, new(new Color(31f / MAX_COLOR, 79f / MAX_COLOR, 122f / MAX_COLOR), Color.white) }, // Cobalt
        { 2048, new(new Color(20f / MAX_COLOR, 58f / MAX_COLOR, 90f / MAX_COLOR), Color.white) }, // Indigo Night
        { 4096, new(new Color(15f / MAX_COLOR, 44f / MAX_COLOR, 79f / MAX_COLOR), Color.white) }, // Midnight Blue
        { 8192, new(new Color(8f / MAX_COLOR, 26f / MAX_COLOR, 47f / MAX_COLOR), Color.white) }, // Deep Abyss
    };

    Dictionary<int, NumberColors> darkColors = new Dictionary<int, NumberColors>
    {
        { 2, new(new Color(0f / MAX_COLOR, 0f / MAX_COLOR, 0f / MAX_COLOR), Color.white) }, // Charcoal Blue
        { 4, new(new Color(20f / MAX_COLOR, 30f / MAX_COLOR, 40f / MAX_COLOR), Color.white) }, // Steel Shadow
        { 8, new(new Color(30f / MAX_COLOR, 45f / MAX_COLOR, 60f / MAX_COLOR), Color.white) }, // Graphite Fog
        { 16, new(new Color(40f / MAX_COLOR, 60f / MAX_COLOR, 80f / MAX_COLOR), Color.white) }, // Dusty Steel
        { 32, new(new Color(50f / MAX_COLOR, 75f / MAX_COLOR, 100f / MAX_COLOR), Color.white) }, // Clouded Blue
        { 64, new(new Color(60f / MAX_COLOR, 90f / MAX_COLOR, 120f / MAX_COLOR), Color.white) }, // Slate Mist
        { 128, new(new Color(70f / MAX_COLOR, 105f / MAX_COLOR, 140f / MAX_COLOR), Color.white) }, // Faded Gunmetal
        { 256, new(new Color(80f / MAX_COLOR, 120f / MAX_COLOR, 160f / MAX_COLOR), Color.white) }, // Soft Graphite
        { 512, new(new Color(90f / MAX_COLOR, 135f / MAX_COLOR, 180f / MAX_COLOR), Color.white) }, // Misty Steel
        { 1024, new(new Color(100f / MAX_COLOR, 150f / MAX_COLOR, 200f / MAX_COLOR), Color.white) }, // Light Slate Fog
        { 2048, new(new Color(110f / MAX_COLOR, 165f / MAX_COLOR, 220f / MAX_COLOR), Color.white) }, // Pale Gunmetal
        { 4096, new(new Color(120f / MAX_COLOR, 180f / MAX_COLOR, 240f / MAX_COLOR), Color.white) }, // Misty Sky
        { 8192, new(new Color(130f / MAX_COLOR, 200f / MAX_COLOR, 255f / MAX_COLOR), Color.white) }, // ???
    };

    public int value = 2;

    private SpriteRenderer spriteRenderer;
    private TextMeshPro textMeshPro;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        textMeshPro = GetComponentInChildren<TextMeshPro>();
    }

    void Start()
    {
        int roll = Random.Range(1, 101);
        value = roll <= 90 ? 2 : 4;
        textMeshPro.text = value.ToString();
        SetColors();
    }

    void SetColors()
    {
        if (GameSettings.instance.colorMode == ColorMode.Light)
        {
            spriteRenderer.color = lightColors[value].tileColor;
            textMeshPro.color = lightColors[value].fontColor;
        }
        else
        {
            spriteRenderer.color = darkColors[value].tileColor;
            textMeshPro.color = darkColors[value].fontColor;
        }
    }

    public void UpdateTextAndColor()
    {
        textMeshPro.text = value.ToString();
        SetColors();

        if (value >= 1024)
        {
            textMeshPro.fontSize = 34f;
            textMeshPro.characterSpacing = -12f;
        }
    }

    public void ToggleColors()
    {
        SetColors();
    }
}

struct NumberColors
{
    public Color tileColor { get; private set; }
    public Color fontColor { get; private set; }

    internal NumberColors(Color tileColor, Color fontColor)
    {
        this.tileColor = tileColor;
        this.fontColor = fontColor;
    }
}