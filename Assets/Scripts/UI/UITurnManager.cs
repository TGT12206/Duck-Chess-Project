using System.Collections.Generic;
using DuckChess;
using UnityEngine;

public class UITurnManager : MonoBehaviour, ITurnManager
{
    public RealTimePlayer WhitePlayer;
    public RealTimePlayer BlackPlayer;
    public RealTimePlayer PlayerToMove;
    public BoardUI boardUI;
    public Board board;
    public Stack<Move> moveHistory;
    public Stack<int> significantMoveCounters;

    public bool IsGameOver()
    {
        return board.isGameOver;
    }

    public void MakeMove(Move move)
    {
        boardUI.MakeMove(move);
        board.MakeMove(ref move);
        significantMoveCounters.Push(board.numPlySinceLastEvent);
        moveHistory.Push(move);
        Debug.Log(board.ToString());
    }
    public void UnmakeMove()
    {
        Move move = moveHistory.Pop();
        int counter = significantMoveCounters.Pop();
        PlayerToMove.UnmakeMove();
        boardUI.UnmakeMove(move);
        board.UnmakeMove(move, counter);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Instantiating fields
        board = new Board();
        moveHistory = new Stack<Move>();
        significantMoveCounters = new Stack<int>();

        // Load the start position
        board.LoadStartPosition();
        boardUI.LoadPosition(ref board);
        board.turnColor = Piece.White;

        // Create the appropriate player types
        // placeholder
        WhitePlayer = new HumanPlayer(boardUI, ref board, Piece.White);
        //BlackPlayer = new HumanPlayer(boardUI, ref board, Piece.Black);
        //WhitePlayer = new AlphaBetaAIPlayer(board, Piece.White, maxDepth: 6);
        BlackPlayer = new AlphaBetaAIPlayer(board, Piece.Black, maxDepth: 6);

        // Set the MakeMove method to be called whenever either player makes a move
        WhitePlayer.OnMoveChosen.AddListener(MakeMove);
        BlackPlayer.OnMoveChosen.AddListener(MakeMove);
    }

    // Update is called once per frame
    void Update()
    {
        if (!board.isGameOver)
        {
            PlayerToMove = board.turnColor == Piece.White ? WhitePlayer : BlackPlayer;
            PlayerToMove.Update();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log(board.ToString());
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            UnmakeMove();
        }
    }
}
