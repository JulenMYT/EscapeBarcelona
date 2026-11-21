using System.Globalization;
using System.Text;

/// <summary>
/// Comparaison des lettres des mots croisés sans tenir compte de la casse ni des accents :
/// « é », « É » et « E » sont la même lettre, « ñ » et « N » aussi. Le joueur n'est donc
/// pas pénalisé s'il tape un accent (ou s'il n'en tape pas), quelle que soit l'écriture
/// des mots dans les données.
/// </summary>
public static class CrosswordText
{
    public static char Normalize(char c)
    {
        string decomposed = char.ToString(c).Normalize(NormalizationForm.FormD);

        foreach (char part in decomposed)
        {
            // La première partie qui n'est pas un accent est la lettre de base.
            if (CharUnicodeInfo.GetUnicodeCategory(part) != UnicodeCategory.NonSpacingMark)
                return char.ToUpperInvariant(part);
        }

        return char.ToUpperInvariant(c);
    }

    public static bool SameLetter(char a, char b)
    {
        return Normalize(a) == Normalize(b);
    }
}
