using UnityEngine;

/// <summary>
/// Lance une cutscene quand la grille de mots croisés est entièrement remplie,
/// avant le mot final (ex. dialogue « Room04CrosswordComplete »).
/// Ne se déclenche que sur une vraie réussite, pas quand la grille est restaurée
/// en revenant dans la salle.
/// </summary>
public class CutsceneTriggerCrosswordGrid : MonoBehaviour
{
    [SerializeField] private CrosswordManager crossword;
    [SerializeField] private CutsceneInitiator cutscene;

    private void OnEnable()
    {
        crossword.OnGridCompleted += OnGridCompleted;
    }

    private void OnDisable()
    {
        crossword.OnGridCompleted -= OnGridCompleted;
    }

    private void OnGridCompleted()
    {
        cutscene.StartCutscene();
    }
}
