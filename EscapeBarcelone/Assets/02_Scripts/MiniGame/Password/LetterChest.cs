using UnityEngine;

public class LetterChest : MonoBehaviour
{
    [SerializeField] private LetterWheel[] wheels;

    private string solution = "ZELDA";

    public void CheckCode()
    {
        string result = "";

        foreach (LetterWheel wheel in wheels)
            result += wheel.Letter;

        if (result == solution)
            Open();
    }

    private void Open()
    {
        Debug.Log("Coffre ouvert !");
    }
}