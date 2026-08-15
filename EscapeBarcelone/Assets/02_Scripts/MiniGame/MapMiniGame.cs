using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MapMiniGame : MonoBehaviour
{
    [SerializeField] private GameObject mapMiniGameCanvas;
    [SerializeField] private List<TMP_InputField> inputFields;

    //[SerializeField] private InteractionElementBase[] pictures;

    [SerializeField] private CutsceneInitiator cutsceneInitiator;

    private GameplayPanel gameplayPanel;

    private static readonly List<string> correctAnswers = new()
{
    "Giant Step",
    "Lilliput Steps",
    "Milky Well",
    "Rainy Circle",
    "Magnet Hill",
    "Pink Cloud",
    "Lumine Hall",
    "Fire Spring"
};

    private void Start()
    {
    }

    public void OpenMapMiniGame()
    {
        //foreach (var picture in pictures)
        //{
        //    picture.SetInteractable(true);
        //}

        gameplayPanel.SetValidateButtonVisible(true);

        gameplayPanel.OnToggleButtonClicked -= ToggleCanvas;
        gameplayPanel.OnToggleButtonClicked += ToggleCanvas;

        gameplayPanel.OnValidateButtonClicked -= VerifyAnswers;
        gameplayPanel.OnValidateButtonClicked += VerifyAnswers;

        mapMiniGameCanvas.SetActive(true);
    }

    public void VerifyAnswers()
    {
        int correctCount = 0;

        for (int i = 0; i < inputFields.Count; i++)
        {
            if (string.Equals(inputFields[i].text.Trim(), correctAnswers[i], StringComparison.OrdinalIgnoreCase))
            {
                correctCount++;
                Debug.Log(correctAnswers[i]);
            }
        }

        if (correctCount == inputFields.Count)
        {
            Debug.Log("All answers are correct!");
            cutsceneInitiator.StartCutscene();
            mapMiniGameCanvas.SetActive(false);
            gameplayPanel.SetValidateButtonVisible(false);
            gameplayPanel.SetValidateButtonVisible(false);

            gameplayPanel.OnToggleButtonClicked -= ToggleCanvas;
            gameplayPanel.OnValidateButtonClicked -= VerifyAnswers;

            //foreach (var picture in pictures)
            //{
            //    picture.Close();
            //}
        }
        else
        {
            Debug.Log($"{correctCount}/{inputFields.Count} answers are correct.");
        }
    }

    public void ToggleCanvas()
    {
        mapMiniGameCanvas.SetActive(!mapMiniGameCanvas.activeSelf);
    }
}
