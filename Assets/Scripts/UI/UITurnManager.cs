using System;
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
    public bool haltForError;
    public int framesToWait = 1;
    public int framesWaited = 0;

    public bool IsGameOver()
    {
        return board.isGameOver;
    }

    public void MakeMove(Move move)
    {
        board.MakeMove(ref move);
        boardUI.LoadPosition(ref board);
        significantMoveCounters.Push(board.numPlySinceLastEvent);
        moveHistory.Push(move);
        Debug.Log("UI\n" + board.ToString());
    }
    public void UnmakeMove()
    {
        Move move = moveHistory.Pop();
        int counter = significantMoveCounters.Pop();
        PlayerToMove.UnmakeMove();
        boardUI.LoadPosition(ref board);
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
        // WhitePlayer = new AlphaBetaAIPlayer(board, Piece.White, maxDepth: 2, boardUI);
        BlackPlayer = new AlphaBetaAIPlayer(board, Piece.Black, maxDepth: 2, boardUI);

        // Set the MakeMove method to be called whenever either player makes a move
        WhitePlayer.OnMoveChosen.AddListener(MakeMove);
        BlackPlayer.OnMoveChosen.AddListener(MakeMove);

        haltForError = false;
    }

    // Update is called once per frame
    void Update()
    {
        framesWaited++;
        if (!board.isGameOver && !haltForError && framesWaited >= framesToWait)
        {
            PlayerToMove = board.turnColor == Piece.White ? WhitePlayer : BlackPlayer;
            try
            {
                PlayerToMove.Update();
            } catch (Exception e)
            {
                haltForError = true;
            }
            framesWaited = 0;
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
