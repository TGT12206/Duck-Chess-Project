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

    /// <summary>
    /// The number of this piece type on the board
    /// </summary>
    public int Count
    {
        get
        {
            return numPieces;
        }
    }

    /// <summary>
    /// Remember that a piece of this piece type is at the given square
    /// </summary>
    public void AddPieceAtSquare(int square)
    {
        occupiedSquares[numPieces] = square;
        map[square] = numPieces;
        numPieces++;
    }

    /// <summary>
    /// Remove the piece of this piece type from memory at the given square
    /// </summary>
    public void RemovePieceAtSquare(int square)
    {
        int pieceIndex = map[square];
        for (int i = pieceIndex; i < numPieces - 1; i++)
        {
            occupiedSquares[i] = occupiedSquares[i + 1];
            map[occupiedSquares[i]] = i;
        }
        occupiedSquares[numPieces - 1] = -1;
        map[square] = -1;
        numPieces--;
    }

    /// <summary>
    /// Update the locations in memory to reflect the given move
    /// </summary>
    public void MovePiece(Move move)
    {
        int startSquare = move.StartSquare;
        int targetSquare = move.TargetSquare;
        int pieceIndex = map[startSquare]; // get the index of this element in the occupiedSquares array
        occupiedSquares[pieceIndex] = targetSquare;
        map[targetSquare] = pieceIndex;
        map[startSquare] = -1;
    }

    public int this[int index] => occupiedSquares[index];

    /// <summary>
    /// Returns this Piece List as a formatted string
    /// </summary>
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