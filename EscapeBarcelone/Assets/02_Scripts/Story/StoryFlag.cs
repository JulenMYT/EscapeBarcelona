// ⚠️ Unity enregistre ces flags dans les scènes sous forme de NOMBRES, pas de noms.
// Chaque flag a donc une valeur fixe :
//  - ne JAMAIS changer la valeur d'un flag existant ;
//  - ne JAMAIS réutiliser la valeur d'un flag supprimé ;
//  - un nouveau flag prend la prochaine valeur libre (ici : 9), où qu'il soit placé dans la liste.
// Sinon toutes les conditions et cutscenes déjà réglées dans les scènes pointent
// silencieusement vers le mauvais flag.
public enum StoryFlag
{
    None = 0,

    // Room 1
    Room01CutsceneSeen = 1,

    // Room 2
    Room02CutsceneSeen = 2,

    // Room 3
    Room03PasswordCutsceneSeen = 3,
    Room03PuzzleZoomCutsceneSeen = 4,
    Room03PuzzleFlipCutsceneSeen = 5,

    // Room 4
    Room04BookOpenCutsceneSeen = 6,

    // Objets débloqués
    HasCompass = 7,
    HasBook = 8,

    // Prochaine valeur libre : 9
}
