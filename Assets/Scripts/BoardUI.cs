using DuckChess;
using UnityEngine;

/// <summary>
/// Renders the pieces on the board
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

    private Board board;

    private void Start()
    {

        board = new Board();
        board.LoadStartPosition();

        BlackPawns = new GameObject[MAX_PAWN_COUNT];
        WhitePawns = new GameObject[MAX_PAWN_COUNT];
        for (int i = 0; i < MAX_PAWN_COUNT; i++)
        {
            BlackPawns[i] = Instantiate(PawnB);
            BlackPawns[i].SetActive(false);
            WhitePawns[i] = Instantiate(PawnW);
            WhitePawns[i].SetActive(false);
        }
        BlackKnights = new GameObject[MAX_KNIGHT_COUNT];
        WhiteKnights = new GameObject[MAX_KNIGHT_COUNT];
        for (int i = 0; i < MAX_KNIGHT_COUNT; i++)
        {
            BlackKnights[i] = Instantiate(KnightB);
            BlackKnights[i].SetActive(false);
            WhiteKnights[i] = Instantiate(KnightW);
            WhiteKnights[i].SetActive(false);
        }
        BlackBishops = new GameObject[MAX_BISHOP_COUNT];
        WhiteBishops = new GameObject[MAX_BISHOP_COUNT];
        for (int i = 0; i < MAX_BISHOP_COUNT; i++)
        {
            BlackBishops[i] = Instantiate(BishopB);
            BlackBishops[i].SetActive(false);
            WhiteBishops[i] = Instantiate(BishopW);
            WhiteBishops[i].SetActive(false);
        }
        BlackRooks = new GameObject[MAX_ROOK_COUNT];
        WhiteRooks = new GameObject[MAX_ROOK_COUNT];
        for (int i = 0; i < MAX_ROOK_COUNT; i++)
        {
            BlackRooks[i] = Instantiate(RookB);
            BlackRooks[i].SetActive(false);
            WhiteRooks[i] = Instantiate(RookW);
            WhiteRooks[i].SetActive(false);
        }
        BlackQueens = new GameObject[MAX_QUEEN_COUNT];
        WhiteQueens = new GameObject[MAX_QUEEN_COUNT];
        for (int i = 0; i < MAX_QUEEN_COUNT; i++)
        {
            BlackQueens[i] = Instantiate(QueenB);
            BlackQueens[i].SetActive(false);
            WhiteQueens[i] = Instantiate(QueenW);
            WhiteQueens[i].SetActive(false);
        }
        WhiteKing = Instantiate(KingW);
        WhiteKing.SetActive(true);
        BlackKing = Instantiate(KingB);
        BlackKing.SetActive(true);
        ReloadPosition();
    }
    
    public void ReloadPosition()
    {
        int numPawnsB = board.BlackPawns.Count;
        int numPawnsW = board.WhitePawns.Count;
        int[] pawnBSpots = board.BlackPawns.occupiedSquares;
        int[] pawnWSpots = board.WhitePawns.occupiedSquares;

        for (int i = 0; i < MAX_PAWN_COUNT; i++)
        {
            if (numPawnsB <= i)
            {
                BlackPawns[i].SetActive(false);
            } else
            {
                BlackPawns[i].SetActive(true);
                BlackPawns[i].transform.position = SquareToWorldCoordinates(pawnBSpots[i], -1);
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

    }

    public static Vector3 SquareToWorldCoordinates(int squareIndex, int z)
    {
        int x = squareIndex % 8;
        int y = squareIndex / 8;
        return new Vector3(x, y, z);
    }

}