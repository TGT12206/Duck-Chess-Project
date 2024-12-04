using System.Collections.Generic;
using DuckChess;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles converting things to and from the internal game
/// representation and the Unity game world.
/// </summary>
public class BoardUI : MonoBehaviour
{
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
    public GameObject Circle;
    #endregion

    #region References to the actual pieces in the scene
    /// <summary>
    /// A array of references to the game object of the pieces,
    /// in the same format that the Board class uses.
    /// </summary>
    public GameObject[] Pieces;
    public Board board;
    public GameObject[] Circles;
    public TextMeshProUGUI playerMoving;
    public TextMeshProUGUI boardType;
    public TextMeshProUGUI displayIsDuckTurn;
    public TextMeshProUGUI displayOtherInfo;
    #endregion

    public List<Move> highlightedMoves = new List<Move>();

    /// <summary>
    /// The z value in unity that I have arbitrarily designated as the
    /// "layer" / position where pieces are placed. This means they are
    /// rendered above the board but below dragged pieces.
    /// </summary>
    private const int PLACE_LAYER = -2;
    private const int SELECT_LAYER = -1;

    /// <summary>
    /// The z value in unity that I have arbitrarily designated as the
    /// "layer" / position where pieces are dragged. This means they are
    /// rendered above the board and other pieces.
    /// </summary>
    private const int DRAG_LAYER = -3;

    private int selectedStartSquare;
    private int selectedTargetSquare;

    public Camera cam;

    private void Start()
    {
        cam = Camera.main;
        Pieces = null;
        Circles = null;
        highlightedMoves = new List<Move>();
        board = null;
        selectedStartSquare = 0;
        boardType.text = "Real Board";
        displayIsDuckTurn.text = "Normal Turn";
    }

    public void AISelectPiece(Move move)
    {
        Circles[selectedStartSquare].SetActive(false);
        Circles[selectedTargetSquare].SetActive(false);
        selectedStartSquare = move.StartSquare;
        selectedTargetSquare = move.TargetSquare;
        Circles[selectedStartSquare].SetActive(true);
        Circles[selectedTargetSquare].SetActive(true);
    }

    public void SelectPiece(int square)
    {
        if (Duck == null)
        {
            SelectDuckInitially();
            return;
        }
        selectedStartSquare = square;
        highlightedMoves.Clear();
        LegalMoveGenerator.GenerateForOnePiece(ref highlightedMoves, board, square);
        foreach (Move move in highlightedMoves)
        {
            Circles[move.TargetSquare].SetActive(true);
        }
    }

    public void SelectDuckInitially()
    {
        selectedStartSquare = 0;
        highlightedMoves.Clear();
        LegalMoveGenerator.GenerateDuckMoves(ref highlightedMoves, board);
        foreach (Move move in highlightedMoves)
        {
            Circles[move.TargetSquare].SetActive(true);
        }
    }

    public void DragPiece()
    {
        Vector2 position = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newPosition = new Vector3(position.x, position.y, DRAG_LAYER);
        Pieces[selectedStartSquare].transform.position = newPosition;
    }


    public void SnapPieceBack()
    {
        Pieces[selectedStartSquare].transform.position = SquareToWorldCoordinates(selectedStartSquare, PLACE_LAYER);
        foreach (Move possibleMove in highlightedMoves)
        {
            Circles[possibleMove.TargetSquare].SetActive(false);
        }
        highlightedMoves.Clear();
    }

    public void CancelSelection()
    {
        SnapPieceBack();
        selectedStartSquare = -1;
    }
    private void GetPrefabOfPiece(ref GameObject whereToPlacePrefab, int piece)
    {
        switch (piece)
        {
            case Piece.White | Piece.Pawn:
                whereToPlacePrefab = PawnW;
                break;
            case Piece.Black | Piece.Pawn:
                whereToPlacePrefab = PawnB;
                break;
            case Piece.White | Piece.Knight:
                whereToPlacePrefab = KnightW;
                break;
            case Piece.Black | Piece.Knight:
                whereToPlacePrefab = KnightB;
                break;
            case Piece.White | Piece.Bishop:
                whereToPlacePrefab = BishopW;
                break;
            case Piece.Black | Piece.Bishop:
                whereToPlacePrefab = BishopB;
                break;
            case Piece.White | Piece.Rook:
                whereToPlacePrefab = RookW;
                break;
            case Piece.Black | Piece.Rook:
                whereToPlacePrefab = RookB;
                break;
            case Piece.White | Piece.Queen:
                whereToPlacePrefab = QueenW;
                break;
            case Piece.Black | Piece.Queen:
                whereToPlacePrefab = QueenB;
                break;
            case Piece.White | Piece.King:
                whereToPlacePrefab = KingW;
                break;
            case Piece.Black | Piece.King:
                whereToPlacePrefab = KingB;
                break;
            case Piece.Duck:
                whereToPlacePrefab = Duck;
                break;
        }
    }
    /// <summary>
    /// Loads a given position into the game world.
    /// </summary>
    public void LoadPosition(ref Board board, bool isSearchBoard, string InfoToDisplay)
    {
        playerMoving.text = board.isWhite ? "White to Move" : "Black to Move";
        if (isSearchBoard)
        {
            boardType.text = "AI Search Board";
            boardType.color = Color.cyan;
        } else
        {
            boardType.text = "Real Board";
            boardType.color = Color.white;
        }
        displayIsDuckTurn.text = board.turnIsDuck ? "Duck Turn" : "Normal Turn";
        displayOtherInfo.text = InfoToDisplay;
        this.board = board;
        if (Pieces != null)
        {
            foreach (GameObject piece in Pieces)
            {
                Destroy(piece);
            }
        }
        if (Circles != null)
        {
            foreach (GameObject circle in Circles)
            {
                Destroy(circle);
            }
        }
        // Create a new array and save the board
        Pieces = new GameObject[64];
        Circles = new GameObject[64];
        for (int i = 0; i < 64; i++)
        {
            Circles[i] = Instantiate(Circle);
            Circles[i].SetActive(false);
            Circles[i].transform.position = SquareToWorldCoordinates(i, SELECT_LAYER);
            GameObject piecePrefab = null;
            GetPrefabOfPiece(ref piecePrefab, board[i]);
            if (piecePrefab != null)
            {
                Pieces[i] = Instantiate(piecePrefab);
                Pieces[i].SetActive(true);
                Pieces[i].transform.position = SquareToWorldCoordinates(i, PLACE_LAYER);
            }
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