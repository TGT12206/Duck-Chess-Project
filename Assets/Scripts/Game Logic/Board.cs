using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine;

namespace DuckChess
{
    /// <summary>
    /// Represents a board state in Duck Chess.
    /// </summary>
    public class Board
    {
        public int[] Squares;

        public const int NOT_ON_BOARD = -2; // 63 is max

        #region Turn Information
        public int turnColor;
        public bool isWhite { get { return turnColor == Piece.White; } }
        public bool turnIsDuck = false;
        #endregion

        public int enPassantSquare;

        #region Castling Info
        public bool CastleKingSideW, CastleQueenSideW;
        public bool CastleKingSideB, CastleQueenSideB;

        private int PlyWhereLostKingSideCastleW, PlyWhereLostQueenSideCastleW;
        private int PlyWhereLostKingSideCastleB, PlyWhereLostQueenSideCastleB;
        #endregion

        #region Game End Info
        public int winnerColor;
        public bool isGameOver;

        public int plyCount;
        //public int numPlySinceLastEvent;
        #endregion

        #region Lists of Legal Moves
        public List<Move> legalMoves;
        #endregion

        public int this[int index]
        {
            get => Squares[index];
            set => Squares[index] = value;
        }

        public Board()
        {
            Squares = new int[64];
            ResetBoardState();
        }


        /// <summary>
        /// Checks whether the given move is legal in the current board position.
        /// </summary>
        /// <param name="move">The move to check.</param>
        /// <returns>True if the move is legal, otherwise false.</returns>
        public bool IsMoveLegal(ref Move move)
        {
            // If this move looks like a capture, make it a capture
            if (this[move.TargetSquare] != Piece.None)
            {
                move = new Move(move, this[move.TargetSquare]);
            }
            // Iterate through all generated legal moves for the current position
            foreach (Move legalMove in legalMoves)
            {
                if (Move.SameMove(move, legalMove))
                {
                    return true; // The move is legal
                }
            }
            return false; // The move is not legal
        }

        public Board Clone()
        {
            // Create a new board instance
            Board copy = new Board
            {
                Squares = (int[])Squares.Clone(),
                turnColor = turnColor,
                turnIsDuck = turnIsDuck,
                enPassantSquare = enPassantSquare,
                CastleKingSideW = CastleKingSideW,
                CastleQueenSideW = CastleQueenSideW,
                CastleKingSideB = CastleKingSideB,
                CastleQueenSideB = CastleQueenSideB,
                PlyWhereLostKingSideCastleW = PlyWhereLostKingSideCastleW,
                PlyWhereLostQueenSideCastleW = PlyWhereLostQueenSideCastleW,
                PlyWhereLostKingSideCastleB = PlyWhereLostKingSideCastleB,
                PlyWhereLostQueenSideCastleB = PlyWhereLostQueenSideCastleB,
                winnerColor = winnerColor,
                isGameOver = isGameOver,
                plyCount = plyCount //,
                //numPlySinceLastEvent = numPlySinceLastEvent
            };
            // Clone legal moves
            copy.legalMoves = new List<Move>(legalMoves);

            return copy;
        }


        private void ResetBoardState()
        {
            legalMoves = new List<Move>();
            CastleKingSideW = CastleQueenSideW = true;
            CastleKingSideB = CastleQueenSideB = true;
            turnColor = Piece.NoColor;
            winnerColor = Piece.NoColor;
            isGameOver = false;
            //numPlySinceLastEvent = 0;
            plyCount = 0;
            PlyWhereLostKingSideCastleW = PlyWhereLostQueenSideCastleW = NOT_ON_BOARD;
            PlyWhereLostKingSideCastleB = PlyWhereLostQueenSideCastleB = NOT_ON_BOARD;
            enPassantSquare = -1;
        }

        /// <summary>
        /// Initializes the board with the starting positions.
        /// </summary>
        public void LoadStartPosition()
        {
            ResetBoardState();

            // Initialize pieces
            PlaceOnSquares(8, 16, Piece.White | Piece.Pawn);
            PlaceOnSquares(48, 56, Piece.Black | Piece.Pawn);

            PlaceOnSquares(new int[] { 1, 6 }, Piece.White | Piece.Knight);
            PlaceOnSquares(new int[] { 57, 62 }, Piece.Black | Piece.Knight);

            PlaceOnSquares(new int[] { 2, 5 }, Piece.White | Piece.Bishop);
            PlaceOnSquares(new int[] { 58, 61 }, Piece.Black | Piece.Bishop);

            PlaceOnSquares(new int[] { 0, 7 }, Piece.White | Piece.Rook);
            PlaceOnSquares(new int[] { 56, 63 }, Piece.Black | Piece.Rook);

            PlaceOnSquares(new int[] { 3 }, Piece.White | Piece.Queen);
            PlaceOnSquares(new int[] { 59 }, Piece.Black | Piece.Queen);

            PlaceKings();
            turnColor = Piece.White;
            GenerateNormalMoves();
        }

        private void PlaceOnSquares(int start, int end, int piece)
        {
            for (int i = start; i < end; i++)
            {
                Squares[i] = piece;
            }
        }

        private void PlaceOnSquares(int[] positions, int piece)
        {
            foreach (var pos in positions)
            {
                Squares[pos] = piece;
            }
        }

        private void PlaceKings()
        {
            Squares[4] = Piece.White | Piece.King;
            Squares[60] = Piece.Black | Piece.King;
        }

        private void UpdateBoard(ref Move move, bool isWhite)
        {
            /*
             * DONE
             * - place the piece on the target square
             * -- if it is the first duck move, replace the piece to place with Piece.Duck
             * - if it is a normal capture, remove the piece on the target square
             * - if it is not the first duck move, remove the piece at the start square
             * - if it is a pawn 2 forward move, set the en passant square behind the pawn
             * - if it is en passant, remove the pawn behind the target square
             * - if it is a promotion, replace the piece at the target square with the promoted piece
             * TO DO (NOW):
             * - 
             * TO DO (LATER):
             * - castling
             * -- if the move is a capture, check if it is an enemy rook
             * - if it is a capture or a pawn move, reset the counter for a draw
             * - idk
             */

            #region place the piece on the target square
            int pieceToMove = Squares[move.StartSquare];

            #region if first duck move, piece to move = duck
            if (move.MoveFlag == Move.Flag.FirstDuckMove)
            {
                pieceToMove = Piece.Duck;
            }
            #endregion
            // Also deletes the piece we are capturing, if any
            Squares[move.TargetSquare] = pieceToMove;
            #endregion

            if (move.MoveFlag != Move.Flag.FirstDuckMove)
            {
                Squares[move.StartSquare] = Piece.None;
            }

            if (move.MoveFlag == Move.Flag.PawnTwoForward)
            {
                enPassantSquare = move.TargetSquare;
            }
            else if (!turnIsDuck)
            {
                enPassantSquare = NOT_ON_BOARD;
            }

            if (move.MoveFlag == Move.Flag.EnPassantCapture)
            {
                int enemySquare = move.TargetSquare + (isWhite ? -8 : 8);
                Squares[enemySquare] = Piece.None;
            }
            
            if (move.IsPromotion)
            {
                Squares[move.TargetSquare] = turnColor | move.PromotionPieceType;
            }

            if (Piece.PieceType(pieceToMove) == Piece.Pawn || move.IsCapture)
            {
                // Reset the timer
                // numPlySinceLastEvent = 0;
            } else
            {
                // Add to the timer
                // numPlySinceLastEvent++;
            }

            String dbgStr = "Updating board (move attempt).\n";
            dbgStr += "Turn is white: " + (turnColor == Piece.White) + "\n";
            dbgStr += "turn is duck: " + turnIsDuck + "\n";
            dbgStr += "Piece: " + Piece.PieceStr(pieceToMove) + "\n";
            dbgStr += "Is white: " + isWhite + "\n";
            dbgStr += "Move: " + move + "\n";
            dbgStr += "Board: " + this + "\n";
            Debug.Log(dbgStr);

            Debug.Log("After attempt:\nBoard: " + this);
        }

        private void PromotePawn(Move move, bool isWhite)
        {
            Squares[move.TargetSquare] = move.PromotionPieceType | (isWhite ? Piece.White : Piece.Black);
            Squares[move.StartSquare] = Piece.None;
        }

        private void HandleCastling(Move move, bool isWhite)
        {
            bool isKingSide = move.TargetSquare > move.StartSquare;
            int rookStart = isKingSide ? (isWhite ? 7 : 63) : (isWhite ? 0 : 56);
            int rookEnd = isKingSide ? move.TargetSquare - 1 : move.TargetSquare + 1;

            // Update squares for king
            Squares[move.TargetSquare] = Squares[move.StartSquare];
            Squares[move.StartSquare] = Piece.None;

            // Update squares for the rook
            Squares[rookEnd] = Squares[rookStart];
            Squares[rookStart] = Piece.None;
        }

        private void SwitchTurnForward()
        {
            turnIsDuck = !turnIsDuck;

            // the next turn we want is here.
            if (turnIsDuck)
            {
                GenerateDuckMoves();
            }
            else
            {
                turnColor = turnColor == Piece.White ? Piece.Black : Piece.White;
                GenerateNormalMoves();
            }
        }

        /// <summary>
        /// Makes a move on the board and updates its state.
        /// </summary>
        public PreviousBoardInfo MakeMove(ref Move move)
        {
            plyCount++;

            bool isWhite = turnColor == Piece.White;
            PreviousBoardInfo infoToStore = StoreInfoToUnmakeMove(move);
            UpdateBoard(ref move, isWhite);

            Debug.Log("Just performed move for: " + turnColor + " move type: " + (turnIsDuck ? "Duck" : "Regular") + "\n" + move.ToString());

            SwitchTurnForward();
            return infoToStore;
        }

        private PreviousBoardInfo StoreInfoToUnmakeMove(Move move)
        {
            PreviousBoardInfo infoToStore = new PreviousBoardInfo();
            infoToStore.moveBackToPreviousBoard = move;
            infoToStore.enPassantSquare = enPassantSquare;
            infoToStore.turnColor = turnColor;
            infoToStore.turnIsDuck = turnIsDuck;
            infoToStore.legalMoves = new List<Move>(legalMoves);
            infoToStore.squares = new int[Squares.Length];
            Array.Copy(Squares, infoToStore.squares, Squares.Length);
            return infoToStore;
        }

        private void GenerateNormalMoves()
        {
            legalMoves = new List<Move>();
            LegalMoveGenerator.GeneratePawnMoves(ref legalMoves, this);
        }
        private void GenerateDuckMoves()
        {
            legalMoves = new List<Move>();
            LegalMoveGenerator.GenerateDuckMoves(ref legalMoves, this);
        }

        public override string ToString()
        {
            var boardString = "Board State:\n";
            for (int i = 7; i >= 0; i--)
            {
                int row = i * 8;
                for (int j = 0; j < 8; j++)
                {
                    boardString += Piece.PieceStr(Squares[row + j]) + " ";
                }
                boardString += "\n";
            }
            return boardString;
        }


        /// <summary>
        /// Reverts the given move, restoring the previous board state.
        /// </summary>
        /// <param name="move">The move to undo.</param>
        /// <param name="previousNumPlySinceLastEvent">The draw counter value before the move was made.</param>
        public void UnmakeMove(PreviousBoardInfo previousBoardInfo)
        {
            Move move = previousBoardInfo.moveBackToPreviousBoard;

            // Set the board info back to what it was before
            plyCount--;
            enPassantSquare = previousBoardInfo.enPassantSquare;
            turnColor = previousBoardInfo.turnColor;
            turnIsDuck = previousBoardInfo.turnIsDuck;
            legalMoves = previousBoardInfo.legalMoves;
            Squares = previousBoardInfo.squares;
        }

        /// <summary>
        /// Stores all of the info that can't easily be undone,
        /// such as the draw counter that is sometimes reset.
        /// </summary>
        public class PreviousBoardInfo
        {
            public Move moveBackToPreviousBoard;
            // int previousNumPlySinceLastEvent;
            internal int enPassantSquare;
            internal int turnColor;
            internal bool turnIsDuck;
            internal List<Move> legalMoves;
            internal int[] squares;
        }
    }

}
