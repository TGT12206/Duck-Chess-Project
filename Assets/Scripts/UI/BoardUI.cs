using DuckChess;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles converting things to and from the internal game
/// representation and the Unity game world.
/// </summary>
public class BoardUI : MonoBehaviour
{
    #region Max number of each piece type, for one color
    const int MAX_PAWN_COUNT = Board.MAX_PAWN_COUNT;
    const int MAX_KNIGHT_COUNT = Board.MAX_KNIGHT_COUNT;
    const int MAX_BISHOP_COUNT = Board.MAX_BISHOP_COUNT;
    const int MAX_ROOK_COUNT = Board.MAX_ROOK_COUNT;
    const int MAX_QUEEN_COUNT = Board.MAX_QUEEN_COUNT;
    #endregion

    #region References to Prefabs of the pieces
    public GameObject BishopB;
    public GameObject BishopW;
    public GameObject KingB;
    public GameObject KingW;
    public GameObject KnightB;
    public GameObject KnightW;
    public GameObject PawnB;
    public GameObject PawnW;
    public GameObject QueenB;
    public GameObject QueenW;
    public GameObject RookB;
    public GameObject RookW;
    public GameObject Duck;
    #endregion

    #region References to the actual pieces in the scene
    /// <summary>
    /// A array of references to the game object of the pieces,
    /// in the same format that the Board class uses.
    /// </summary>
    public GameObject[] Pieces;
    public Board board;
    #endregion

    /// <summary>
    /// The z value in unity that I have arbitrarily designated as the
    /// "layer" / position where pieces are placed. This means they are
    /// rendered above the board but below dragged pieces.
    /// </summary>
    private const int PLACE_LAYER = -1;

    /// <summary>
    /// The z value in unity that I have arbitrarily designated as the
    /// "layer" / position where pieces are dragged. This means they are
    /// rendered above the board and other pieces.
    /// </summary>
    private const int DRAG_LAYER = -2;

    private int selectedSquare;

    public Camera cam;

    private void Start()
    {
        cam = Camera.main;
        Pieces = new GameObject[64];
        board = null;
    }

    public void SelectPiece(int square)
    {
        selectedSquare = square;
    }

    public void SelectDuckInitially()
    {
        selectedSquare = -1;
    }

    public void DragPiece()
    {
        Vector2 position = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newPosition = new Vector3(position.x, position.y, DRAG_LAYER);
        Pieces[selectedSquare].transform.position = newPosition;
    }

    public void MakeMove(Move move)
    {
        int newSquare = move.TargetSquare;
        int squareToCapture = newSquare;
        bool isWhite = board.turnColor == Piece.White;
        if (move.MoveFlag == Move.Flag.EnPassantCapture)
        {
            squareToCapture += isWhite ? -8 : 8;
            Destroy(Pieces[squareToCapture]);
            Pieces[squareToCapture] = null;
        }
        if (Pieces[newSquare] != null)
        {
            Destroy(Pieces[squareToCapture]);
            Pieces[squareToCapture] = null;
        }
        if (move.IsPromotion)
        {
            Destroy(Pieces[move.StartSquare]);
            Pieces[move.StartSquare] = null;
            GameObject newPiece = null;
            switch(move.MoveFlag)
            {
                case Move.Flag.PromoteToKnight:
                    newPiece = isWhite ? KnightW : KnightB;
                    break;
                case Move.Flag.PromoteToBishop:
                    newPiece = isWhite ? BishopW : BishopB;
                    break;
                case Move.Flag.PromoteToRook:
                    newPiece = isWhite ? RookW : RookB;
                    break;
                case Move.Flag.PromoteToQueen:
                    newPiece = isWhite ? QueenW : QueenB;
                    break;
            }
            Pieces[move.StartSquare] = Instantiate(newPiece);
        }
        if (move.MoveFlag == Move.Flag.Castling)
        {
            bool isKingSide = move.TargetSquare - move.StartSquare > 0;

            int rookSpot = move.StartSquare;
            rookSpot += isKingSide ? +3 : -4;

            int newRookSpot = move.StartSquare;
            newRookSpot += isKingSide ? 1 : -1;

            Move moveRooks = new Move(rookSpot, newRookSpot);
            MakeMove(moveRooks);
        }
        if (board.duckTurn && board.Duck == Board.NOT_ON_BOARD)
        {
            Pieces[newSquare] = Instantiate(Duck);
        } else
        {
            Pieces[newSquare] = Pieces[move.StartSquare];
            Pieces[move.StartSquare] = null;
        }
        Vector3 newPosition = SquareToWorldCoordinates(newSquare, PLACE_LAYER);
        Pieces[newSquare].transform.position = newPosition;
    }

    public void UnmakeMove(Move move)
    {
        bool isWhite = board.turnColor == Piece.White;
        int currentSquare = move.TargetSquare;
        int originalSquare = move.StartSquare;

        // En passant
        if (move.MoveFlag == Move.Flag.EnPassantCapture)
        {
            int enemyPawnSpot = isWhite ? -1 : 1;
            enemyPawnSpot += currentSquare;

            Pieces[enemyPawnSpot] = isWhite ? Instantiate(PawnB) : Instantiate(PawnW);
            Pieces[enemyPawnSpot].transform.position = SquareToWorldCoordinates(enemyPawnSpot, PLACE_LAYER);
        }

        // Castling
        if (move.MoveFlag == Move.Flag.Castling) {
            int kingSpot = isWhite ? board.WhiteKing : board.BlackKing;
            bool wasKingSide = Board.GetColumnOf(kingSpot) == 6;
            int rookSpot = wasKingSide ? -1 : 1;
            int originalRookSpot = wasKingSide ? 1 : -2;
            Pieces[originalRookSpot] = Pieces[rookSpot];
            Pieces[originalRookSpot].transform.position = SquareToWorldCoordinates(originalRookSpot, PLACE_LAYER);
            Pieces[rookSpot] = null;
        }

        // Update the squares
        // Not the first duck move
        if (board.plyCount != 2)
        {
            Pieces[originalSquare] = Pieces[currentSquare];
            Pieces[originalSquare].transform.position = SquareToWorldCoordinates(originalSquare, PLACE_LAYER);
        }
        else
        {
            Destroy(Pieces[currentSquare]);
            Pieces[currentSquare] = null;
        }
        GameObject capturedPiecePrefab = null;
        switch (Piece.PieceType(move.CapturedPiece))
        {
            case Piece.Pawn:
                capturedPiecePrefab = isWhite ? PawnB : PawnW;
                break;
            case Piece.Knight:
                capturedPiecePrefab = isWhite ? KnightB : KnightW;
                break;
            case Piece.Bishop:
                capturedPiecePrefab = isWhite ? BishopB : BishopW;
                break;
            case Piece.Rook:
                capturedPiecePrefab = isWhite ? RookB : RookW;
                break;
            case Piece.Queen:
                capturedPiecePrefab = isWhite ? QueenB : QueenW;
                break;
            case Piece.King:
                capturedPiecePrefab = isWhite ? KingB : KingW;
                break;
        }
        if (capturedPiecePrefab != null)
        {
            Pieces[currentSquare] = Instantiate(capturedPiecePrefab);
            Pieces[currentSquare].transform.position = SquareToWorldCoordinates(currentSquare, PLACE_LAYER);
        } else
        {
            Pieces[currentSquare] = null;
        }
    }

    public void SnapPieceBack()
    {
        Pieces[selectedSquare].transform.position = SquareToWorldCoordinates(selectedSquare, PLACE_LAYER);
    }

    public void CancelSelection()
    {
        SnapPieceBack();
        selectedSquare = -1;
    }

    /// <summary>
    /// Loads a given position into the game world.
    /// </summary>
    public void LoadPosition(ref Board board)
    {
        // Create a new array and save the board
        Pieces = new GameObject[64];
        this.board = board;

        // Place the pawns
        for (int i = 0; i < board.WhitePawns.Count; i++)
        {
            int square = board.WhitePawns[i];
            Pieces[square] = Instantiate(PawnW);
            Pieces[square].transform.position = SquareToWorldCoordinates(square, PLACE_LAYER);
        }
        for (int i = 0; i < board.BlackPawns.Count; i++)
        {
            int square = board.BlackPawns[i];
            Pieces[square] = Instantiate(PawnB);
            Pieces[square].transform.position = SquareToWorldCoordinates(square, PLACE_LAYER);
        }

        // Place the knights
        for (int i = 0; i < board.WhiteKnights.Count; i++)
        {
            int square = board.WhiteKnights[i];
            Pieces[square] = Instantiate(KnightW);
            Pieces[square].transform.position = SquareToWorldCoordinates(square, PLACE_LAYER);
        }
        for (int i = 0; i < board.BlackKnights.Count; i++)
        {
            int square = board.BlackKnights[i];
            Pieces[square] = Instantiate(KnightB);
            Pieces[square].transform.position = SquareToWorldCoordinates(square, PLACE_LAYER);
        }

        // Place the bishops
        for (int i = 0; i < board.WhiteBishops.Count; i++)
        {
            int square = board.WhiteBishops[i];
            Pieces[square] = Instantiate(BishopW);
            Pieces[square].transform.position = SquareToWorldCoordinates(square, PLACE_LAYER);
        }
        for (int i = 0; i < board.BlackBishops.Count; i++)
        {
            int square = board.BlackBishops[i];
            Pieces[square] = Instantiate(BishopB);
            Pieces[square].transform.position = SquareToWorldCoordinates(square, PLACE_LAYER);
        }

        // Place the rooks
        for (int i = 0; i < board.WhiteRooks.Count; i++)
        {
            int square = board.WhiteRooks[i];
            Pieces[square] = Instantiate(RookW);
            Pieces[square].transform.position = SquareToWorldCoordinates(square, PLACE_LAYER);
        }
        for (int i = 0; i < board.BlackRooks.Count; i++)
        {
            int square = board.BlackRooks[i];
            Pieces[square] = Instantiate(RookB);
            Pieces[square].transform.position = SquareToWorldCoordinates(square, PLACE_LAYER);
        }

        // Place the queens
        for (int i = 0; i < board.WhiteQueens.Count; i++)
        {
            int square = board.WhiteQueens[i];
            Pieces[square] = Instantiate(QueenW);
            Pieces[square].transform.position = SquareToWorldCoordinates(square, PLACE_LAYER);
        }
        for (int i = 0; i < board.BlackQueens.Count; i++)
        {
            int square = board.BlackQueens[i];
            Pieces[square] = Instantiate(QueenB);
            Pieces[square].transform.position = SquareToWorldCoordinates(square, PLACE_LAYER);
        }

        // Place the kings
        Pieces[board.WhiteKing] = Instantiate(KingW);
        Pieces[board.BlackKing] = Instantiate(KingB);

        Pieces[board.WhiteKing].transform.position = SquareToWorldCoordinates(board.WhiteKing, PLACE_LAYER);
        Pieces[board.BlackKing].transform.position = SquareToWorldCoordinates(board.BlackKing, PLACE_LAYER);

        // Place the duck
        if (board.Duck != Board.NOT_ON_BOARD)
        {
            Pieces[board.Duck] = Instantiate(Duck);
            Pieces[board.Duck].transform.position = SquareToWorldCoordinates(board.Duck, PLACE_LAYER);
        }
    }

    /// <summary>
    /// Used to calculate where in the unity game world a piece is based on the
    /// square's index.
    /// </summary>
    /// <param name="squareIndex">The number of the square as used by the Board class. 0-63</param>
    /// <param name="z">The "layer" a piece should be rendered on. Smaller numbers get rendered on top of larger ones.</param>
    /// <returns>The world position of the square index as a Vector3</returns>
    public static Vector3 SquareToWorldCoordinates(int squareIndex, int z)
    {
        int x = squareIndex % 8;
        int y = squareIndex / 8;
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Converts unity world coordinates to the square index used by the Board class.
    /// Automatically rounds to an index within bounds if the location is outside of
    /// the board.
    /// </summary>
    /// <param name="worldCoordinates">The location in the unity scene</param>
    public static int WorldCoordinatesToSquare(Vector2 worldCoordinates)
    {
        int square;

        // Round to the nearest integer
        int row = (int) Mathf.RoundToInt(worldCoordinates.y);
        int col = (int) Mathf.RoundToInt(worldCoordinates.x);

        // Place the location back within bounds if needed
        row = row < 0 ? 0 : row;
        col = col < 0 ? 0 : col;
        row = row > 7 ? 7 : row;
        col = col > 7 ? 7 : col;

        // Convert to the index and return
        square = (row * 8) + col;
        return square;
    }

    /// <summary>
    /// Fetches the position of the mouse and converts it to the square index used by
    /// the Board class.
    /// </summary>
    public static int GetMouseSquare()
    {
        Camera mainCam = Camera.main;
        Vector2 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        return WorldCoordinatesToSquare(mouseWorldPos);
    }

}