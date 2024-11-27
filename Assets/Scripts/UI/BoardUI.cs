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
    GameObject[] BlackPawns;
    GameObject[] WhitePawns;
    GameObject[] BlackKnights;
    GameObject[] WhiteKnights;
    GameObject[] BlackBishops;
    GameObject[] WhiteBishops;
    GameObject[] BlackRooks;
    GameObject[] WhiteRooks;
    GameObject[] BlackQueens;
    GameObject[] WhiteQueens;
    GameObject WhiteKing;
    GameObject BlackKing;
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
        DeclarePieceArrays();
        LoadStartPosition();
    }
    
    public void SelectPiece(int square)
    {
        selectedSquare = square;
    }

    public void DragPiece()
    {
        Vector2 position = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newPosition = new Vector3(position.x, position.y, DRAG_LAYER);
        Pieces[selectedSquare].transform.position = newPosition;
    }

    public void PlacePiece(int newSquare)
    {
        if (Pieces[newSquare] != null)
        {
            Destroy(Pieces[newSquare]);
        }
        Pieces[newSquare] = Pieces[selectedSquare];
        Pieces[selectedSquare] = null;
        Vector3 newPosition = SquareToWorldCoordinates(newSquare, PLACE_LAYER);
        Pieces[newSquare].transform.position = newPosition;
    }

    public void SnapPieceBack()
    {
        Pieces[selectedSquare].transform.position = SquareToWorldCoordinates(selectedSquare, PLACE_LAYER);
    }

    /// <summary>
    /// Loads the start position into the game world.
    /// </summary>
    public void LoadStartPosition()
    {
        // Place the pawns
        for (int i = 0; i < 8; i++)
        {
            WhitePawns[i] = Instantiate(PawnW);
            BlackPawns[i] = Instantiate(PawnB);

            WhitePawns[i].transform.position = SquareToWorldCoordinates(8 + i, PLACE_LAYER);
            BlackPawns[i].transform.position = SquareToWorldCoordinates(48 + i, PLACE_LAYER);

            Pieces[8 + i] = WhitePawns[i];
            Pieces[48 + i] = BlackPawns[i];
        }

        // Place the knights
        WhiteKnights[0] = Instantiate(KnightW);
        WhiteKnights[1] = Instantiate(KnightW);
        BlackKnights[0] = Instantiate(KnightB);
        BlackKnights[1] = Instantiate(KnightB);

        WhiteKnights[0].transform.position = SquareToWorldCoordinates(1, PLACE_LAYER);
        WhiteKnights[1].transform.position = SquareToWorldCoordinates(6, PLACE_LAYER);
        BlackKnights[0].transform.position = SquareToWorldCoordinates(57, PLACE_LAYER);
        BlackKnights[1].transform.position = SquareToWorldCoordinates(62, PLACE_LAYER);

        Pieces[1] = WhiteKnights[0];
        Pieces[6] = WhiteKnights[1];
        Pieces[57] = BlackKnights[0];
        Pieces[62] = BlackKnights[1];

        // Place the bishops
        WhiteBishops[0] = Instantiate(BishopW);
        WhiteBishops[1] = Instantiate(BishopW);
        BlackBishops[0] = Instantiate(BishopB);
        BlackBishops[1] = Instantiate(BishopB);

        WhiteBishops[0].transform.position = SquareToWorldCoordinates(2, PLACE_LAYER);
        WhiteBishops[1].transform.position = SquareToWorldCoordinates(5, PLACE_LAYER);
        BlackBishops[0].transform.position = SquareToWorldCoordinates(58, PLACE_LAYER);
        BlackBishops[1].transform.position = SquareToWorldCoordinates(61, PLACE_LAYER);

        Pieces[2] = WhiteBishops[0];
        Pieces[5] = WhiteBishops[1];
        Pieces[58] = BlackBishops[0];
        Pieces[61] = BlackBishops[1];

        // Place the rooks
        WhiteRooks[0] = Instantiate(RookW);
        WhiteRooks[1] = Instantiate(RookW);
        BlackRooks[0] = Instantiate(RookB);
        BlackRooks[1] = Instantiate(RookB);

        WhiteRooks[0].transform.position = SquareToWorldCoordinates(0, PLACE_LAYER);
        WhiteRooks[1].transform.position = SquareToWorldCoordinates(7, PLACE_LAYER);
        BlackRooks[0].transform.position = SquareToWorldCoordinates(56, PLACE_LAYER);
        BlackRooks[1].transform.position = SquareToWorldCoordinates(63, PLACE_LAYER);

        Pieces[0] = WhiteRooks[0];
        Pieces[7] = WhiteRooks[1];
        Pieces[56] = BlackRooks[0];
        Pieces[63] = BlackRooks[1];

        // Place the queens
        WhiteQueens[0] = Instantiate(QueenW);
        BlackQueens[0] = Instantiate(QueenB);

        WhiteQueens[0].transform.position = SquareToWorldCoordinates(3, PLACE_LAYER);
        BlackQueens[0].transform.position = SquareToWorldCoordinates(59, PLACE_LAYER);

        Pieces[3] = WhiteQueens[0];
        Pieces[59] = BlackQueens[0];

        // Place the kings
        WhiteKing = Instantiate(KingW);
        BlackKing = Instantiate(KingB);

        WhiteKing.transform.position = SquareToWorldCoordinates(4, PLACE_LAYER);
        BlackKing.transform.position = SquareToWorldCoordinates(60, PLACE_LAYER);

        Pieces[4] = WhiteKing;
        Pieces[60] = BlackKing;
    }

    /// <summary>
    /// NOT IMPLEMENTED
    /// Loads a given board's position.
    /// </summary>
    public void LoadPosition(Board board)
    {
        int numPawnsB = board.BlackPawns.Count;
        int numPawnsW = board.WhitePawns.Count;
        int[] pawnBSpots = board.BlackPawns.occupiedSquares;
        int[] pawnWSpots = board.WhitePawns.occupiedSquares;

        for (int i = 0; i < MAX_PAWN_COUNT; i++)
        {
            if (numPawnsB <= i && BlackPawns[i] != null)
            {
                Destroy(BlackPawns[i]);
                BlackPawns[i] = null;
            } else
            {
                if (BlackPawns[i] == null)
                {
                    BlackPawns[i] = Instantiate(PawnB);
                }
                BlackPawns[i].transform.position = SquareToWorldCoordinates(pawnBSpots[i], PLACE_LAYER);
            }
            if (numPawnsW <= i)
            {
                WhitePawns[i].SetActive(false);
            }
            else
            {
                WhitePawns[i].SetActive(true);
                WhitePawns[i].transform.position = SquareToWorldCoordinates(pawnWSpots[i], -1);
            }
        }

        int numKnightsB = board.BlackKnights.Count;
        int numKnightsW = board.WhiteKnights.Count;
        int[] knightBSpots = board.BlackKnights.occupiedSquares;
        int[] knightWSpots = board.WhiteKnights.occupiedSquares;

        for (int i = 0; i < MAX_KNIGHT_COUNT; i++)
        {
            if (numKnightsB <= i)
            {
                BlackKnights[i].SetActive(false);
            }
            else
            {
                BlackKnights[i].SetActive(true);
                BlackKnights[i].transform.position = SquareToWorldCoordinates(knightBSpots[i], -1);
            }
            if (numKnightsW <= i)
            {
                WhiteKnights[i].SetActive(false);
            }
            else
            {
                WhiteKnights[i].SetActive(true);
                WhiteKnights[i].transform.position = SquareToWorldCoordinates(knightWSpots[i], -1);
            }
        }

        int numBishopsB = board.BlackBishops.Count;
        int numBishopsW = board.WhiteBishops.Count;
        int[] bishopBSpots = board.BlackBishops.occupiedSquares;
        int[] bishopWSpots = board.WhiteBishops.occupiedSquares;

        for (int i = 0; i < MAX_BISHOP_COUNT; i++)
        {
            if (numBishopsB <= i)
            {
                BlackBishops[i].SetActive(false);
            }
            else
            {
                BlackBishops[i].SetActive(true);
                BlackBishops[i].transform.position = SquareToWorldCoordinates(bishopBSpots[i], -1);
            }
            if (numBishopsW <= i)
            {
                WhiteBishops[i].SetActive(false);
            }
            else
            {
                WhiteBishops[i].SetActive(true);
                WhiteBishops[i].transform.position = SquareToWorldCoordinates(bishopWSpots[i], -1);
            }
        }

        int numRooksB = board.BlackRooks.Count;
        int numRooksW = board.WhiteRooks.Count;
        int[] rookBSpots = board.BlackRooks.occupiedSquares;
        int[] rookWSpots = board.WhiteRooks.occupiedSquares;

        for (int i = 0; i < MAX_ROOK_COUNT; i++)
        {
            if (numRooksB <= i)
            {
                BlackRooks[i].SetActive(false);
            }
            else
            {
                BlackRooks[i].SetActive(true);
                BlackRooks[i].transform.position = SquareToWorldCoordinates(rookBSpots[i], -1);
            }
            if (numRooksW <= i)
            {
                WhiteRooks[i].SetActive(false);
            }
            else
            {
                WhiteRooks[i].SetActive(true);
                WhiteRooks[i].transform.position = SquareToWorldCoordinates(rookWSpots[i], -1);
            }
        }

        int numQueensB = board.BlackQueens.Count;
        int numQueensW = board.WhiteQueens.Count;
        int[] queenBSpots = board.BlackQueens.occupiedSquares;
        int[] queenWSpots = board.WhiteQueens.occupiedSquares;

        for (int i = 0; i < MAX_QUEEN_COUNT; i++)
        {
            if (numQueensB <= i)
            {
                BlackQueens[i].SetActive(false);
            }
            else
            {
                BlackQueens[i].SetActive(true);
                BlackQueens[i].transform.position = SquareToWorldCoordinates(queenBSpots[i], -1);
            }
            if (numQueensW <= i)
            {
                WhiteQueens[i].SetActive(false);
            }
            else
            {
                WhiteQueens[i].SetActive(true);
                WhiteQueens[i].transform.position = SquareToWorldCoordinates(queenWSpots[i], -1);
            }
        }

        WhiteKing.transform.position = SquareToWorldCoordinates(board.WhiteKing, -1);
        BlackKing.transform.position = SquareToWorldCoordinates(board.BlackKing, -1);
        throw new System.NotImplementedException();
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

    /// <summary>
    /// Instantiates all of the pieces as game objects, saves them, and de-activates them.
    /// </summary>
    private void DeclarePieceArrays()
    {
        BlackPawns = new GameObject[MAX_PAWN_COUNT];
        WhitePawns = new GameObject[MAX_PAWN_COUNT];
        BlackKnights = new GameObject[MAX_KNIGHT_COUNT];
        WhiteKnights = new GameObject[MAX_KNIGHT_COUNT];
        BlackBishops = new GameObject[MAX_BISHOP_COUNT];
        WhiteBishops = new GameObject[MAX_BISHOP_COUNT];
        BlackRooks = new GameObject[MAX_ROOK_COUNT];
        WhiteRooks = new GameObject[MAX_ROOK_COUNT];
        BlackQueens = new GameObject[MAX_QUEEN_COUNT];
        WhiteQueens = new GameObject[MAX_QUEEN_COUNT];
        Pieces = new GameObject[64];
    }

}