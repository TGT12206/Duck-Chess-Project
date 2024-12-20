using System;
using System.Collections.Generic;
using DuckChess;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITurnManager : MonoBehaviour, ITurnManager
{
    public HumanPlayer HumanWhitePlayer;
    public HumanPlayer HumanBlackPlayer;
    public AlphaBetaAIPlayer AlphaBetaAIWhitePlayer;
    public AlphaBetaAIPlayer AlphaBetaAIBlackPlayer;
    public MCTSAIPlayer MCTSAIWhitePlayer;
    public MCTSAIPlayer MCTSAIBlackPlayer;

    public RealTimePlayer PlayerToMove;
    public BoardUI boardUI;
    public Board board;
    public bool haltForError;
    public int framesToWaitWhite = 1;
    public int framesToWaitBlack = 1;
    public TMP_Dropdown WhitePlayerDropdown;
    public TMP_Dropdown BlackPlayerDropdown;
    public Slider sliderForWhite;
    public Slider sliderForBlack;
    public List<string> PlayerTypes;

    private const int HUMAN = 0;
    private const int ABAI = 1;
    private const int MCTSAI = 2;

    public bool showWhiteSearchBoard;
    public bool showBlackSearchBoard;

    public bool ABSkipDuckSearchWhite;
    public bool ABSkipDuckSearchBlack;

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
        HumanWhitePlayer = new HumanPlayer(boardUI, ref board, Piece.White);
        HumanBlackPlayer = new HumanPlayer(boardUI, ref board, Piece.Black);

        AlphaBetaAIWhitePlayer = new AlphaBetaAIPlayer(board, Piece.White, maxDepth: 4, boardUI);
        AlphaBetaAIBlackPlayer = new AlphaBetaAIPlayer(board, Piece.Black, maxDepth: 4, boardUI);

        MCTSAIWhitePlayer = new MCTSAIPlayer(board, Piece.White, boardUI);
        MCTSAIBlackPlayer = new MCTSAIPlayer(board, Piece.Black, boardUI);

        // Set the MakeMove method to be called whenever any player makes a move
        HumanWhitePlayer.OnMoveChosen.AddListener(MakeMove);
        HumanBlackPlayer.OnMoveChosen.AddListener(MakeMove);

        AlphaBetaAIWhitePlayer.OnMoveChosen.AddListener(MakeMove);
        AlphaBetaAIBlackPlayer.OnMoveChosen.AddListener(MakeMove);

        MCTSAIWhitePlayer.OnMoveChosen.AddListener(MakeMove);
        MCTSAIBlackPlayer.OnMoveChosen.AddListener(MakeMove);

        // Create the dropdown options
        WhitePlayerDropdown.options.Clear();
        BlackPlayerDropdown.options.Clear();

        WhitePlayerDropdown.AddOptions(PlayerTypes);
        BlackPlayerDropdown.AddOptions(PlayerTypes);

        haltForError = false;
    }

    // Update is called once per frame
    void Update()
    {
        framesToWait = board.isWhite ? framesToWaitWhite : framesToWaitBlack;
        framesWaited++;
        if (!board.isGameOver && !haltForError && framesWaited >= framesToWait)
        {
            RealTimePlayer WhitePlayer;
            RealTimePlayer BlackPlayer;

            // Select the appropriate player for white
            switch (WhitePlayerDropdown.value)
            {
                case HUMAN:
                    WhitePlayer = HumanWhitePlayer;
                    break;
                case ABAI:
                    AlphaBetaAIWhitePlayer.skipDuckSearch = ABSkipDuckSearchWhite;
                    WhitePlayer = AlphaBetaAIWhitePlayer;
                    Debug.Log("slider: " + sliderForWhite.value);
                    AlphaBetaAIWhitePlayer.maxDepth = (int) sliderForWhite.value;
                    Debug.Log("Depth: " + AlphaBetaAIWhitePlayer.maxDepth);
                    break;
                case MCTSAI:
                    WhitePlayer = MCTSAIWhitePlayer;
                    MCTSAIWhitePlayer.NumSimulationsPerTurn = (int) sliderForWhite.value * 10000;
                    break;
                default:
                    WhitePlayer = HumanWhitePlayer;
                    break;
            }

            // Select the appropriate player for black
            switch (BlackPlayerDropdown.value)
            {
                case HUMAN:
                    BlackPlayer = HumanBlackPlayer;
                    break;
                case ABAI:
                    AlphaBetaAIBlackPlayer.skipDuckSearch = ABSkipDuckSearchBlack;
                    BlackPlayer = AlphaBetaAIBlackPlayer;
                    AlphaBetaAIBlackPlayer.maxDepth = (int) sliderForBlack.value;
                    break;
                case MCTSAI:
                    BlackPlayer = MCTSAIBlackPlayer;
                    MCTSAIBlackPlayer.NumSimulationsPerTurn = (int) sliderForBlack.value * 10000;
                    break;
                default:
                    BlackPlayer = HumanBlackPlayer;
                    break;
            }

            // Set showSearchBoard flags if needed
            AlphaBetaAIWhitePlayer.showSearchBoard = showWhiteSearchBoard;
            AlphaBetaAIBlackPlayer.showSearchBoard = showBlackSearchBoard;

            // If MCTSAIPlayer has a showSearchBoard property, set it
            MCTSAIWhitePlayer.showSearchBoard = showWhiteSearchBoard;
            MCTSAIBlackPlayer.showSearchBoard = showBlackSearchBoard;

            PlayerToMove = board.turnColor == Piece.White ? WhitePlayer : BlackPlayer;
            try
            {
                PlayerToMove.Update();
            }
            catch (Exception e)
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
