using DuckChess;
using UnityEngine;

public class UITurnManager : MonoBehaviour, ITurnManager
{
    public RealTimePlayer WhitePlayer;
    public RealTimePlayer BlackPlayer;
    public RealTimePlayer PlayerToMove;
    public BoardUI boardUI;
    public Board board;

    /// <summary>
    /// The color of the player whose turn it is to move.
    /// </summary>
    private int turnColor;

    /// <summary>
    /// Whether or not it is a duck turn.
    /// </summary>
    private bool duckTurn;

    public bool IsDuckPly()
    {
        return duckTurn;
    }

    public bool IsGameOver()
    {
        throw new System.NotImplementedException();
    }

    public bool IsWhiteToMove()
    {
        return turnColor == Piece.White;
    }

    public void MakeMove(Move move)
    {
        turnColor = turnColor == Piece.White ? Piece.Black : Piece.White;
        board.MakeMove(move);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Instantiating fields
        board = new Board();

        // Load the start position
        board.LoadStartPosition();
        turnColor = Piece.White;

        // Create the appropriate player types
        // placeholder
        WhitePlayer = new HumanPlayer(boardUI, ref board, Piece.White);
        BlackPlayer = new HumanPlayer(boardUI, ref board, Piece.Black);

        // Set the MakeMove method to be called whenever either player makes a move
        WhitePlayer.OnMoveChosen.AddListener(MakeMove);
        BlackPlayer.OnMoveChosen.AddListener(MakeMove);
    }

    // Update is called once per frame
    void Update()
    {
        PlayerToMove = turnColor == Piece.White ? WhitePlayer : BlackPlayer;
        PlayerToMove.Update();
    }
}
