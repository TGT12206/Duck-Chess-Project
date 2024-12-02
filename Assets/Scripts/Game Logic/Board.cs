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
        public int numPlySinceLastEvent;
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
                plyCount = plyCount,
                numPlySinceLastEvent = numPlySinceLastEvent
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
            numPlySinceLastEvent = 0;
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
            int piece = Squares[move.StartSquare];
            if (move.MoveFlag == Move.Flag.FirstDuckMove)
            {
                piece = Piece.Duck;
            }

            String dbgStr = "Updating board (move attempt).\n";
            dbgStr += "Turn is white: " + (turnColor == Piece.White) + "\n";
            dbgStr += "turn is duck: " + turnIsDuck + "\n";
            dbgStr += "Piece: " + Piece.PieceStr(piece) + "\n";
            dbgStr += "Is white: " + isWhite + "\n";
            dbgStr += "Move: " + move + "\n";
            dbgStr += "Board: " + this + "\n";
            Debug.Log(dbgStr);

            if (move.MoveFlag == Move.Flag.EnPassantCapture)
            {
                HandleEnPassant(move, isWhite);
            }
            else if (move.IsPromotion)
            {
                PromotePawn(move, isWhite);
            }
            else if (move.MoveFlag == Move.Flag.Castling)
            {
                HandleCastling(move, isWhite);
            }
            else
            {
                if (move.MoveFlag == Move.Flag.PawnTwoForward)
                {
                    enPassantSquare = move.TargetSquare;
                }

                Squares[move.TargetSquare] = piece;
                if (move.MoveFlag != Move.Flag.FirstDuckMove)
                {
                    Squares[move.StartSquare] = Piece.None;
                }
            }

            Debug.Log("After attempt:\nBoard: " + this);
        }

        private void HandleEnPassant(Move move, bool isWhite)
        {
            int enemySquare = move.TargetSquare + (isWhite ? -8 : 8);
            Squares[enemySquare] = Piece.None;
            Squares[move.TargetSquare] = Squares[move.StartSquare];
            Squares[move.StartSquare] = Piece.None;
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
            Debug.Log("Just performed move for: " + turnColor + " move type: " + (turnIsDuck ? "Duck" : "Regular"));
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

        private void SwitchTurnBackward(ref Move move)
        {
            bool duckMove = move.isDuckMove(this);
            Debug.Log("We are undoing move for: " + turnColor + " move type: " + (duckMove ? "Duck" : "Regular") + " board type (should be opposite): " + (turnIsDuck ? "Duck" : "Regular"));

            turnIsDuck = !turnIsDuck;

            // the next turn we want is here.
            if (turnIsDuck)
            {
                GenerateDuckMoves();
            }
            else
            {
                // keep as same color.
                // turnColor = turnColor == Piece.White ? Piece.Black : Piece.White;
                GenerateNormalMoves();
            }



        }

        /// <summary>
        /// Makes a move on the board and updates its state.
        /// </summary>
        public void MakeMove(ref Move move)
        {
            plyCount++;
            numPlySinceLastEvent++;

            bool isWhite = turnColor == Piece.White;
            UpdateBoard(ref move, isWhite);

            SwitchTurnForward();
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
        public void UnmakeMove(Move move, int previousNumPlySinceLastEvent)
        {
            //String dbgStr = "Unmaking move.\n";
            //dbgStr += "Move: " + move + "\n";
            //dbgStr += "Board: " + this + "\n";

            //Debug.Log(dbgStr);

            //plyCount--;
            //numPlySinceLastEvent = previousNumPlySinceLastEvent;

            //// Restore the piece to its original position
            //int targetPiece = move.TargetPiece(this);

            //if (move.MoveFlag == Move.Flag.FirstDuckMove)
            //{
            //    Duck = NOT_ON_BOARD;
            //    Squares[move.TargetSquare] = Piece.None;
            //    return;
            //}

            //Squares[move.StartSquare] = targetPiece;
            //Squares[move.TargetSquare] = move.CapturedPiece;

            //// Revert en passant square
            //if (move.MoveFlag == Move.Flag.PawnTwoForward)
            //{
            //    enPassantSquare = NOT_ON_BOARD;
            //}

            //// Handle captured piece restoration
            //else if (move.IsCapture)
            //{
            //    int capturedPiece = move.CapturedPiece;
            //    if (capturedPiece != Piece.None)
            //    {
            //        Debug.Log("Piece type: " + Piece.PieceStr(capturedPiece));
            //        PieceList capturedPieceList = GetPieceList(Piece.PieceType(capturedPiece), Piece.Color(capturedPiece) == Piece.White);
            //        capturedPieceList.AddPieceAtSquare(move.TargetSquare);
            //    }
            //}

            //// Handle pawn promotion reversal
            //else if (move.IsPromotion)
            //{
            //    PieceList promotionList = GetPieceList(Piece.PieceType(targetPiece), Piece.Color(targetPiece) == Piece.White);
            //    promotionList.RemovePieceAtSquare(move.TargetSquare);
            //    PieceList pawnList = GetPieceList(Piece.Pawn, Piece.Color(targetPiece) == Piece.White);
            //    pawnList.AddPieceAtSquare(move.TargetSquare);
            //}


            //// Handle castling
            //else if (move.MoveFlag == Move.Flag.Castling)
            //{
            //    bool isKingSide = move.TargetSquare > move.StartSquare;
            //    int rookStart = isKingSide ? move.TargetSquare + 1 : move.TargetSquare - 2;
            //    int rookEnd = isKingSide ? move.TargetSquare - 1 : move.TargetSquare + 1;

            //    Squares[rookStart] = Squares[rookEnd];
            //    Squares[rookEnd] = Piece.None;

            //    PieceList rookList = GetPieceList(Piece.Rook, Piece.Color(targetPiece) == Piece.White);
            //    rookList.UnmovePiece(new Move(rookEnd, rookStart));
            //}

            //// default
            //else
            //{

            //    int pieceType = Piece.PieceType(targetPiece);
            //    if (pieceType == Piece.King)
            //    {
            //        if (Piece.Color(targetPiece) == Piece.White)
            //        {
            //            WhiteKing = move.StartSquare;
            //        }
            //        else
            //        {
            //            BlackKing = move.StartSquare;
            //        }
            //    }

            //    else if (pieceType == Piece.Duck)
            //    {
            //        Duck = move.StartSquare;
            //    }

            //    else
            //    {
            //        // Regular piece movement undo
            //        PieceList movedPieceList = GetPieceList(pieceType, Piece.Color(targetPiece) == Piece.White);
            //        movedPieceList.UnmovePiece(move);
            //    }
            //}
            //// Restore king position if necessary


            //SwitchTurnBackward(ref move);


            //String dbgStr1 = "Unmaking move (finish).\n";
            //dbgStr1 += "Move: " + move + "\n";
            //dbgStr1 += "Board: " + this + "\n";

            //Debug.Log(dbgStr1);

        }
    }

}
