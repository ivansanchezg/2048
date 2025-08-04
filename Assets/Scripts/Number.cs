using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Number : MonoBehaviour
{
    const float MAX_COLOR = 255f;

    Dictionary<int, Color> lightColors = new Dictionary<int, Color>
    {
        { 2, new Color(214f / MAX_COLOR, 236f / MAX_COLOR, 250f / MAX_COLOR) }, // Powder Blue
        { 4, new Color(163f / MAX_COLOR, 213f / MAX_COLOR, 247f / MAX_COLOR) }, // Sky Blue
        { 8, new Color(137f / MAX_COLOR, 197f / MAX_COLOR, 240f / MAX_COLOR) }, // Baby Blue
        { 16, new Color(111f / MAX_COLOR, 183f / MAX_COLOR, 233f / MAX_COLOR) }, // Cornflower Blue
        { 32, new Color(79f / MAX_COLOR, 165f / MAX_COLOR, 224f / MAX_COLOR) }, // Capri
        { 64, new Color(63f / MAX_COLOR, 143f / MAX_COLOR, 196f / MAX_COLOR) }, // Azure
        { 128, new Color(50f / MAX_COLOR, 115f / MAX_COLOR, 179f / MAX_COLOR) }, // True Blue
        { 256, new Color(44f / MAX_COLOR, 111f / MAX_COLOR, 158f / MAX_COLOR) }, // Steel Blue
        { 512, new Color(36f / MAX_COLOR, 92f / MAX_COLOR, 133f / MAX_COLOR) }, // Denim
        { 1024, new Color(31f / MAX_COLOR, 79f / MAX_COLOR, 122f / MAX_COLOR) }, // Cobalt
        { 2048, new Color(20f / MAX_COLOR, 58f / MAX_COLOR, 90f / MAX_COLOR) }, // Indigo Night
        { 4096, new Color(15f / MAX_COLOR, 44f / MAX_COLOR, 79f / MAX_COLOR) }, // Midnight Blue
        { 8192, new Color(8f / MAX_COLOR, 26f / MAX_COLOR, 47f / MAX_COLOR) }, // Deep Abyss
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
        spriteRenderer.color = lightColors[value];
    }

    public void UpdateTextAndColor()
    {
        textMeshPro.text = value.ToString();
        spriteRenderer.color = lightColors[value];
        if (value >= 64)
        {
            textMeshPro.color = Color.white;
        }
        else
        {
            textMeshPro.color = Color.black;
        }
    }
}
