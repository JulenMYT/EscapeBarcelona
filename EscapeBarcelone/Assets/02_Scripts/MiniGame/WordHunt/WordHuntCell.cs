using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WordHuntCell : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
{
    [SerializeField] private TMP_Text letterText;

    public event Action<Vector2Int> PointerDown;
    public event Action<Vector2Int> PointerEnter;

    public Vector2 Position => transform.position;

    private Vector2Int position;

    public void Setup(Vector2Int position, char letter)
    {
        this.position = position;
        letterText.text = letter.ToString();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PointerDown?.Invoke(position);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnter?.Invoke(position);
    }
}