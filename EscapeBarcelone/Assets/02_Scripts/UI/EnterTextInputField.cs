using System;
using TMPro;
using UnityEngine;

public class EnterTextInputField : MonoBehaviour
{ 
    [SerializeField] private TMP_InputField inputField;

    public event Action<string> OnInputEntered;

    private void Awake()
    {
        inputField.onEndEdit.AddListener((text) => {
            OnInputEntered?.Invoke(text);
        });
    }
}
