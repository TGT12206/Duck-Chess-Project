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
        int pieceIndex = map[startSquare];
        occupiedSquares[pieceIndex] = targetSquare;
        map[targetSquare] = pieceIndex;
        map[startSquare] = -1;
    }

    /// <summary>
    /// Update the locations in memory to undo the given move
    /// </summary>
    public void UnmovePiece(Move move)
    {
        int startSquare = move.StartSquare;
        int targetSquare = move.TargetSquare;
        int pieceIndex = map[targetSquare];
        occupiedSquares[pieceIndex] = startSquare;
        map[startSquare] = pieceIndex;
        map[targetSquare] = -1;
    }

    /// <summary>
    /// Returns a new piecelist with the combined information.
    /// It is not possible to add more pieces to this list, so treat it as readonly.
    /// However, you may still merge it with another list.
    /// </summary>
    public static PieceList MergePieceLists(PieceList firstList, PieceList secondList)
    {
        int firstPieceCount = firstList.Count;
        int secondPieceCount = secondList.Count;
        int total = firstPieceCount + secondPieceCount;
        PieceList newList = new PieceList(total);
        for (int i = 0; i < firstPieceCount; i++)
        {
            int occupiedSquare = firstList[i];
            newList.AddPieceAtSquare(occupiedSquare);
        }
        for (int i = 0; i < secondPieceCount; i++)
        {
            int occupiedSquare = secondList[i];
            newList.AddPieceAtSquare(occupiedSquare);
        }
        return newList;
    }

    /// <summary>
    /// Adds the information of the other list into this one. Make sure that
    /// the maximum capacity of this list is greater than the combined
    /// total of pieces, or it will result in an index out of bounds.
    /// </summary>
    public void MergeWithPieceList(PieceList otherList)
    {
        int otherPieceCount = otherList.Count;
        for (int i = 0; i < otherPieceCount; i++)
        {
            int occupiedSquare = otherList[i];
            AddPieceAtSquare(occupiedSquare);
        }
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

    public PieceList Clone()
    {
        PieceList clone = new PieceList(this.occupiedSquares.Length);
        clone.numPieces = this.numPieces;
        this.occupiedSquares.CopyTo(clone.occupiedSquares, 0);
        this.map.CopyTo(clone.map, 0);
        return clone;
    }
}