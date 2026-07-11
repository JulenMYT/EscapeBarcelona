using UnityEngine;

public static class PuzzleGridGenerator
{
    public static PuzzleGridData CalculateGrid(int targetPieces, int imageWidth, int imageHeight)
    {
        float aspectRatio = (float)imageWidth / imageHeight;

        int estimatedColumns = Mathf.RoundToInt(Mathf.Sqrt(targetPieces * aspectRatio));
        int estimatedRows = Mathf.RoundToInt((float)targetPieces / estimatedColumns);

        float bestError = float.MaxValue;

        int columns = estimatedColumns;
        int rows = estimatedRows;

        for (int rowOffset = -2; rowOffset <= 2; rowOffset++)
        {
            int currentRows = estimatedRows + rowOffset;

            if (currentRows < 1)
                continue;

            for (int columnOffset = -2; columnOffset <= 2; columnOffset++)
            {
                int currentColumns = estimatedColumns + columnOffset;

                if (currentColumns < 1)
                    continue;

                float ratioError = currentColumns * (float)imageHeight / currentRows / imageWidth;
                ratioError = (ratioError + 1f / ratioError) - 2f;

                float pieceCountError = Mathf.Abs(1f - (float)(currentColumns * currentRows) / targetPieces);

                float totalError = ratioError + pieceCountError;

                if (totalError < bestError)
                {
                    bestError = totalError;
                    columns = currentColumns;
                    rows = currentRows;
                }
            }
        }

        return new PuzzleGridData
        {
            columns = columns,
            rows = rows,
            pieceWidth = (float)imageWidth / columns,
            pieceHeight = (float)imageHeight / rows
        };
    }
}