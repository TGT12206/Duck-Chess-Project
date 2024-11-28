using DuckChess;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class HumanPlayer : RealTimePlayer
{
    private BoardUI boardUI;
    private Board board;

    private enum InputState
    {
        /// <summary>
        /// Represents a state where the human has no piece selected
        /// </summary>
        None,

        /// <summary>
        /// Represents a state where the human has clicked a piece
        /// </summary>
        PieceSelected,

        /// <summary>
        /// Represents a state where the human is dragging a piece
        /// </summary>
        DraggingPiece
    }
    
    /// <summary>
    /// A representation of which input the human is currently doing.
    /// </summary>
    private InputState currentState;
    public override string Type
    {
        get { return "HumanPlayer"; }
    }
    public override int Color { get; set; }

    private int selectedSquare;
    private int targetSquare;

    public HumanPlayer (BoardUI boardUI, ref Board board, int color)
    {
        this.boardUI = boardUI;
        this.board = board;
        Color = color;
    }

    /// <summary>
    /// Handles the user's input during their turn.
    /// </summary>
    public override void Update()
    {
        int mouseSquare = BoardUI.GetMouseSquare();
        if (currentState == InputState.None)
        {
            HandlePieceSelection(mouseSquare);
        }
        else if (currentState == InputState.DraggingPiece)
        {
            HandleDragMovement(mouseSquare);
        }
        else if (currentState == InputState.PieceSelected)
        {
            HandlePointAndClickMovement(mouseSquare);
        }

        if (Input.GetMouseButtonDown(1))
        {
            CancelPieceSelection();
        }
    }

    private void HandlePieceSelection(int mouseSquare)
    {
        if (Input.GetMouseButtonDown(0))
        {
            // If square contains a piece, select that piece for dragging
            if (Piece.IsColor(board.Squares[mouseSquare], Color))
            {
                // boardUI.HighlightLegalMoves(board, selectedPieceSquare);
                selectedSquare = mouseSquare;
                boardUI.SelectPiece(mouseSquare);
                currentState = InputState.DraggingPiece;
            }
        }
    }

    private void HandleDragMovement(int mouseSquare)
    {
        boardUI.DragPiece();
        if (Input.GetMouseButtonUp(0))
        {
            HandlePiecePlacement(mouseSquare);
        }
    }

    private void HandlePointAndClickMovement(int mouseSquare)
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandlePiecePlacement(mouseSquare);
        }
    }

    private void HandlePiecePlacement(int mouseSquare)
    {
        if (mouseSquare == selectedSquare)
        {
            if (currentState == InputState.DraggingPiece)
            {
                currentState = InputState.PieceSelected;
                boardUI.SnapPieceBack();
            } else if (currentState == InputState.PieceSelected)
            {
                CancelPieceSelection();
            }
        } else
        {
            targetSquare = mouseSquare;
            currentState = InputState.None;
            Move newMove = new Move(selectedSquare, targetSquare);
            if (board.IsMoveLegal(ref newMove))
            {
                boardUI.PlacePiece(mouseSquare);
                ChooseMove(newMove);
            } else
            {
                boardUI.CancelSelection();
            }
        }
    }
    private void CancelPieceSelection()
    {
        currentState = InputState.None;
        boardUI.CancelSelection();
    }
}
