using TMPro;
using UnityEngine;

public class Number : MonoBehaviour
{
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
    }

    public void UpdateText()
    {
        textMeshPro.text = value.ToString();
    }
}
