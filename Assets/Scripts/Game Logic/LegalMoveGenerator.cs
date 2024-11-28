using System.CodeDom.Compiler;
using System.Collections.Generic;
using UnityEngine;

namespace DuckChess
{
    /// <summary>
    /// A static class containing methods to generate a list of legal moves
    /// for the given piece type on that turn, or all moves for that turn.
    /// </summary>
    public static class LegalMoveGenerator
    {
        public static void GenerateAllMoves(ref List<Move> generatedMoves, Board board)
        {
            if (board.duckTurn)
            {
                GenerateDuckMoves(ref generatedMoves, board);
            }
        }

        /// <summary>
        /// Generate all the pawn moves for the color of the board
        /// and places them into the provided list.
        /// </summary>
        /// <param name="generatedMoves"></param>
        /// <param name="board"></param>
        public static void GeneratePawnMoves(ref List<Move> generatedMoves, Board board)
        {
            int pawnSpot;
            if (board.turnColor == Piece.White)
            {
                for (int i = 0; i < board.BlackPawns.Count; i++)
                {
                    pawnSpot = board.WhitePawns[i];
                    // Forward pawn moves
                    if (Piece.PieceType(board[pawnSpot + 8]) == Piece.None)
                    {
                        if (pawnSpot + 8 > 55)
                        {
                            #region Add pawn promotions
                            Move move = new Move(pawnSpot, pawnSpot + 8, Move.Flag.PromoteToKnight);
                            generatedMoves.Add(move);

                            move = new Move(pawnSpot, pawnSpot + 8, Move.Flag.PromoteToBishop);
                            generatedMoves.Add(move);

                            move = new Move(pawnSpot, pawnSpot + 8, Move.Flag.PromoteToRook);
                            generatedMoves.Add(move);

                            move = new Move(pawnSpot, pawnSpot + 8, Move.Flag.PromoteToQueen);
                            generatedMoves.Add(move);
                            #endregion
                        }
                        else
                        {
                            // Normal pawn move
                            Move move = new Move(pawnSpot, pawnSpot + 8);
                            generatedMoves.Add(move);
                        }

                        // Pawn move two forward
                        if (board.GetRowOf(pawnSpot) == 1)
                        {
                            if (Piece.PieceType(board[pawnSpot + 16]) == Piece.None)
                            {
                                Move move = new Move(pawnSpot, pawnSpot + 16, Move.Flag.PawnTwoForward);
                                generatedMoves.Add(move);
                            }
                        }
                        // En Passant
                        if (board.GetRowOf(pawnSpot) == 4)
                        {
                            if (
                                board.GetColumnOf(pawnSpot) == board.enPassantColumn - 1 ||
                                board.GetColumnOf(pawnSpot) == board.enPassantColumn + 1
                            )
                            {
                                if (board.Duck != 40 + board.enPassantColumn)
                                {
                                    Move move = new Move(pawnSpot,
                                        40 + board.enPassantColumn,
                                        Move.Flag.EnPassantCapture
                                    );
                                    generatedMoves.Add(move);
                                }
                            }
                        }
                    }
                    // Normal pawn captures
                    int pawnCol = board.GetColumnOf(pawnSpot);
                    if (pawnCol < 7)
                    {
                        if (Piece.PieceType(board[pawnSpot + 9]) != Piece.None &&
                            Piece.PieceType(board[pawnSpot + 9]) != Piece.Duck)
                        {
                            Move move = new Move(pawnSpot, pawnSpot + 9);
                            generatedMoves.Add(move);
                        }
                    }
                    if (pawnCol > 0)
                    {
                        if (Piece.PieceType(board[pawnSpot + 7]) != Piece.None &&
                            Piece.PieceType(board[pawnSpot + 7]) != Piece.Duck)
                        {
                            Move move = new Move(pawnSpot, pawnSpot + 7);
                            generatedMoves.Add(move);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < board.BlackPawns.Count; i++)
                {
                    pawnSpot = board.BlackPawns[i];
                    // Forward pawn moves
                    if (Piece.PieceType(board[pawnSpot - 8]) == Piece.None)
                    {
                        if (pawnSpot - 8 < 8)
                        {
                            #region Add pawn promotions
                            Move move = new Move(pawnSpot, pawnSpot - 8, Move.Flag.PromoteToKnight);
                            generatedMoves.Add(move);

                            move = new Move(pawnSpot, pawnSpot - 8, Move.Flag.PromoteToBishop);
                            generatedMoves.Add(move);

                            move = new Move(pawnSpot, pawnSpot - 8, Move.Flag.PromoteToRook);
                            generatedMoves.Add(move);

                            move = new Move(pawnSpot, pawnSpot - 8, Move.Flag.PromoteToQueen);
                            generatedMoves.Add(move);
                            #endregion
                        }
                        else
                        {
                            // Normal pawn move
                            Move move = new Move(pawnSpot, pawnSpot - 8);
                            generatedMoves.Add(move);
                        }

                        // Pawn move two forward
                        if (board.GetRowOf(pawnSpot) == 6)
                        {
                            if (Piece.PieceType(board[pawnSpot - 16]) == Piece.None)
                            {
                                Move move = new Move(pawnSpot, pawnSpot - 16, Move.Flag.PawnTwoForward);
                                generatedMoves.Add(move);
                            }
                        }

                        // En Passant
                        if (board.GetRowOf(pawnSpot) == 3)
                        {
                            if (
                                board.GetColumnOf(pawnSpot) == board.enPassantColumn - 1 ||
                                board.GetColumnOf(pawnSpot) == board.enPassantColumn + 1
                            )
                            {
                                if (board.Duck != 16 + board.enPassantColumn)
                                {
                                    Move move = new Move(pawnSpot,
                                        16 + board.enPassantColumn,
                                        Move.Flag.EnPassantCapture
                                    );
                                    generatedMoves.Add(move);
                                }
                            }
                        }
                    }
                    // Normal pawn captures
                    int pawnCol = board.GetColumnOf(pawnSpot);
                    if (pawnCol > 0)
                    {
                        if (Piece.PieceType(board[pawnSpot - 9]) != Piece.None &&
                            Piece.PieceType(board[pawnSpot - 9]) != Piece.Duck)
                        {
                            Move move = new Move(pawnSpot, pawnSpot - 9);
                            generatedMoves.Add(move);
                        }
                    }
                    if (pawnCol < 7)
                    {
                        if (Piece.PieceType(board[pawnSpot - 7]) != Piece.None &&
                            Piece.PieceType(board[pawnSpot - 7]) != Piece.Duck)
                        {
                            Move move = new Move(pawnSpot, pawnSpot - 7);
                            generatedMoves.Add(move);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generate all the knight moves for the color of the board
        /// and places them into the provided list.
        /// </summary>
        /// <param name="generatedMoves"></param>
        /// <param name="board"></param>
        public static void GenerateKnightMoves(ref List<Move> generatedMoves, Board board)
        {
            
        }

        /// <summary>
        /// Generate all the bishop moves for the color of the board
        /// and places them into the provided list.
        /// </summary>
        /// <param name="generatedMoves"></param>
        /// <param name="board"></param>
        public static void GenerateBishopMoves(ref List<Move> generatedMoves, Board board)
        {

        }

        /// <summary>
        /// Generate all the rook moves for the color of the board
        /// and places them into the provided list.
        /// </summary>
        /// <param name="generatedMoves"></param>
        /// <param name="board"></param>
        public static void GenerateRookMoves(ref List<Move> generatedMoves, Board board)
        {

        }

        /// <summary>
        /// Generate all the queen moves for the color of the board
        /// and places them into the provided list.
        /// </summary>
        /// <param name="generatedMoves"></param>
        /// <param name="board"></param>
        public static void GenerateQueenMoves(ref List<Move> generatedMoves, Board board)
        {

        }

        /// <summary>
        /// Generate all the king moves for the color of the board
        /// and places them into the provided list.
        /// </summary>
        /// <param name="generatedMoves"></param>
        /// <param name="board"></param>
        public static void GenerateKingMoves(ref List<Move> generatedMoves, Board board)
        {

        }

        /// <summary>
        /// Generate all the duck moves and places them into the provided list.
        /// </summary>
        /// <param name="generatedMoves"></param>
        /// <param name="board"></param>
        public static void GenerateDuckMoves(ref List<Move> generatedMoves, Board board)
        {
            int firstDuckMoveFlag = Move.Flag.None;
            int startSquare = board.Duck;
            if (startSquare == -1)
            {
                startSquare = 0;
                firstDuckMoveFlag = Move.Flag.FirstDuckMove;
            }

            for (int i = 0; i < board.Squares.Length; i++)
            {
                if (board[i] == Piece.None)
                {
                    int targetSquare = i;
                    Move newMove = new Move(startSquare, targetSquare, firstDuckMoveFlag);
                    generatedMoves.Add(newMove);
                }
            }
        }
    }
}