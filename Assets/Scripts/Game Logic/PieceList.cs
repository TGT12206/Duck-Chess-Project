using DuckChess;
using UnityEngine;

/// <summary>
/// A class that stores all of the squares that have a specific piece type on them.
/// The piece type a given PieceList object cares about is up to the user of this class
/// to decide.
/// </summary>
public class PieceList
{
    /// <summary>
    /// A list that stores squares with the specific piece type
    /// </summary>
    public int[] occupiedSquares;

    /// <summary>
    /// A mapping from square (0-63) to the index with that square in occupiedSquares
    /// </summary>
    int[] map;

    /// <summary>
    /// The number of this piece type on the board
    /// </summary>
    int numPieces;

    public PieceList(int maxPieceCount)
    {
        occupiedSquares = new int[maxPieceCount];
        map = new int[64];
        for (int i = 0; i < maxPieceCount; i++)
        {
            occupiedSquares[i] = -1;
        }
        for (int i = 0; i < 64; i++)
        {
            map[i] = -1;
        }
        numPieces = 0;
    }

    public int Count
    {
        get
        {
            return numPieces;
        }
    }

    public void AddPieceAtSquare(int square)
    {
        occupiedSquares[numPieces] = square;
        map[square] = numPieces;
        numPieces++;
    }

    public void RemovePieceAtSquare(int square)
    {
        Debug.Log("PieceList Before Remove\n" + this.ToString());
        int pieceIndex = map[square];
        for (int i = pieceIndex; i < numPieces - 1; i++)
        {
            occupiedSquares[i] = occupiedSquares[i + 1];
            map[occupiedSquares[i]] = i;
        }
        occupiedSquares[numPieces - 1] = -1;
        map[square] = -1;
        numPieces--;
        Debug.Log("PieceList After Remove\n" + this.ToString());
    }

    public void MovePiece(Move move)
    {
        Debug.Log("PieceList Before move\n" + this.ToString());
        int startSquare = move.StartSquare;
        int targetSquare = move.TargetSquare;
        int pieceIndex = map[startSquare]; // get the index of this element in the occupiedSquares array
        occupiedSquares[pieceIndex] = targetSquare;
        map[targetSquare] = pieceIndex;
        map[startSquare] = -1;
        Debug.Log("PieceList After move\n" + this.ToString());
    }

    public int this[int index] => occupiedSquares[index];

    public override string ToString()
    {
        string output = "Occupied Squares:\n";
        for (int i = 0; i < occupiedSquares.Length; i++)
        {
            output += occupiedSquares[i] + " ";
            output += i == numPieces - 1 ? "| " : "";
        }
        output += "\n" + numPieces;
        output += "\nMap:\n";
        for (int i = 7; i >= 0; i--)
        {
            int row = i * 8;
            for (int j = 0; j < 8; j++)
            {
                output += map[row + j] + " ";
            }
            output += "\n";
        }
        return output;
    }

}