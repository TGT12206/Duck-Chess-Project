using DuckChess;
using System.Collections.Generic;
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
            if (board.plyCount == 1)
            {
                mouseSquare = 0;
                selectedSquare = 0;
                boardUI.SelectDuckInitially();
                currentState = InputState.PieceSelected;
            } else
            {
                HandlePieceSelection(mouseSquare);
            }
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
            if (
                (!board.turnIsDuck &&
                Piece.IsColor(board[mouseSquare], Color))
                ||
                (board.turnIsDuck &&
                Piece.PieceType(board[mouseSquare]) == Piece.Duck)
            )
            {
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
            List<Move> legalMovesForThisPiece = new List<Move>();
            LegalMoveGenerator.GenerateForOnePiece(ref legalMovesForThisPiece, board, selectedSquare);
            Move newMove = new Move();
            foreach (Move legalMove in legalMovesForThisPiece)
            {
                if (selectedSquare == legalMove.StartSquare && targetSquare == legalMove.TargetSquare)
                {
                    newMove = legalMove;
                }
            }
            if (board.IsMoveLegal(ref newMove))
            {
                ChooseMove(newMove);
                currentState = InputState.None;
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
