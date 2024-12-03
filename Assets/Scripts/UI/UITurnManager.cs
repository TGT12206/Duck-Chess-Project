using System;
using System.Collections.Generic;
using DuckChess;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class UITurnManager : MonoBehaviour, ITurnManager
{
    public HumanPlayer HumanWhitePlayer;
    public HumanPlayer HumanBlackPlayer;
    public AlphaBetaAIPlayer AlphaBetaAIWhitePlayer;
    public AlphaBetaAIPlayer AlphaBetaAIBlackPlayer;
    public RealTimePlayer PlayerToMove;
    public BoardUI boardUI;
    public Board board;
    public bool haltForError;
    public int framesToWaitWhite = 1;
    public int framesToWaitBlack = 1;
    public bool WhiteIsBot;
    public bool BlackIsBot;
    public bool showWhiteSearchBoard;
    public bool showBlackSearchBoard;
    public int framesToWait;
    public int framesWaited = 0;

    public bool IsGameOver()
    {
        return board.isGameOver;
    }

    public void MakeMove(Move move)
    {
        board.MakeMove(ref move);
        boardUI.LoadPosition(ref board, false, "");
        Debug.Log("UI\n" + board.ToString());
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Instantiating fields
        board = new Board();
        framesToWait = framesToWaitWhite;

        // Load the start position
        board.LoadStartPosition();
        boardUI.LoadPosition(ref board, false, "");
        board.turnColor = Piece.White;

        // Create the appropriate player types
        // placeholder
        HumanWhitePlayer = new HumanPlayer(boardUI, ref board, Piece.White);
        HumanBlackPlayer = new HumanPlayer(boardUI, ref board, Piece.Black);
        AlphaBetaAIWhitePlayer = new AlphaBetaAIPlayer(board, Piece.White, maxDepth: 5, boardUI);
        AlphaBetaAIBlackPlayer = new AlphaBetaAIPlayer(board, Piece.Black, maxDepth: 5, boardUI);

        // Set the MakeMove method to be called whenever either player makes a move
        HumanWhitePlayer.OnMoveChosen.AddListener(MakeMove);
        HumanBlackPlayer.OnMoveChosen.AddListener(MakeMove);
        AlphaBetaAIWhitePlayer.OnMoveChosen.AddListener(MakeMove);
        AlphaBetaAIBlackPlayer.OnMoveChosen.AddListener(MakeMove);

        haltForError = false;
    }

    // Update is called once per frame
    void Update()
    {
        framesToWait = board.isWhite ? framesToWaitWhite : framesToWaitBlack;
        framesWaited++;
        if (!board.isGameOver && !haltForError && framesWaited >= framesToWait)
        {
            RealTimePlayer WhitePlayer = WhiteIsBot ? AlphaBetaAIWhitePlayer : HumanWhitePlayer;
            RealTimePlayer BlackPlayer = BlackIsBot ? AlphaBetaAIBlackPlayer : HumanBlackPlayer;
            AlphaBetaAIWhitePlayer.showSearchBoard = showWhiteSearchBoard;
            AlphaBetaAIBlackPlayer.showSearchBoard = showBlackSearchBoard;
            PlayerToMove = board.turnColor == Piece.White ? WhitePlayer : BlackPlayer;
            try
            {
                PlayerToMove.Update();
            } catch (Exception e)
            {
                haltForError = true;
                Debug.Log(e);
            }
            framesWaited = 0;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log(board.ToString());
        }
    }
}
