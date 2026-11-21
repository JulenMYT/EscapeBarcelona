using UnityEngine;

/// <summary>
/// Identifiant unique et stable d'un objet, pour retenir son état entre les salles
/// (pièce ramassée, mini-jeu réussi…).
///
/// ⚠️ Dupliquer un objet (Ctrl+D) ou le copier dans une autre scène copie aussi son
/// identifiant. Pour vérifier tout le projet : Tools > Escape Barcelone > Vérifier les
/// identifiants persistants. Pour un seul objet : clic droit sur le composant > Nouvel identifiant.
/// </summary>
public class PersistentGuid : MonoBehaviour
{
    [SerializeField] private string guid;
    public string Guid => guid;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(guid))
        {
            guid = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
            return;
        }

        if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        // Doublon dans les scènes ouvertes (cas typique : Ctrl+D) : on prévient.
        foreach (PersistentGuid other in FindObjectsByType<PersistentGuid>(FindObjectsInactive.Include))
        {
            if (other != this && other.guid == guid)
            {
                Debug.LogWarning($"PersistentGuid : « {name} » et « {other.name} » ont le même identifiant. " +
                                 "Clic droit sur le composant > Nouvel identifiant.", this);
                break;
            }
        }
    }

    [ContextMenu("Nouvel identifiant")]
    public void RegenerateGuid()
    {
        UnityEditor.Undo.RecordObject(this, "Nouvel identifiant");
        guid = System.Guid.NewGuid().ToString();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
