/// <summary>
/// A class that stores all of the squares that have a specific piece type on them.
/// The piece type a given PieceList object cares about is up to the user of this class
/// to decide.
/// </summary>
public class PieceList
{
    /// <summary>
    /// A stack that stores squares with the specific piece type
    /// </summary>
    public int[] occupiedSquares;

    /// <summary>
    /// A mapping from square (int 0-63) to the index with that square in occupiedSquares
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
        int pieceIndex = map[square]; // get the index of this element in the occupiedSquares array
        occupiedSquares[pieceIndex] = occupiedSquares[numPieces - 1]; // move last element in array to the place of the removed element
        map[occupiedSquares[pieceIndex]] = pieceIndex; // update map to point to the moved element's new location in the array
        numPieces--;
    }

    public void MovePiece(int startSquare, int targetSquare)
    {
        int pieceIndex = map[startSquare]; // get the index of this element in the occupiedSquares array
        occupiedSquares[pieceIndex] = targetSquare;
        map[targetSquare] = pieceIndex;
    }

    public int this[int index] => occupiedSquares[index];

}