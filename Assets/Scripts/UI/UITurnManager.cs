using DuckChess;
using UnityEngine;

public class UITurnManager : MonoBehaviour, ITurnManager
{
    public RealTimePlayer WhitePlayer;
    public RealTimePlayer BlackPlayer;
    public RealTimePlayer PlayerToMove;
    public BoardUI boardUI;
    public Board board;

    public bool IsGameOver()
    {
        return board.isGameOver;
    }

    public void MakeMove(Move move)
    {
        boardUI.MakeMove(move);
        board.MakeMove(move);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Instantiating fields
        board = new Board();

        // Load the start position
        board.LoadStartPosition();
        boardUI.LoadPosition(ref board);
        board.turnColor = Piece.White;

        // Create the appropriate player types
        // placeholder
        ///WhitePlayer = new HumanPlayer(boardUI, ref board, Piece.White);
        WhitePlayer = new AIPlayer(board, Piece.White, maxDepth: 3);
        BlackPlayer = new AIPlayerMCTS(board, Piece.Black, simulationCount: 1000, timeLimit: 5.0f);

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
    }
}
