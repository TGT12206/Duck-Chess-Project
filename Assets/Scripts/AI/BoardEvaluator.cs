using System;
using System.Collections.Generic;
using UnityEngine;

namespace DuckChess
{
    public static class BoardEvaluator
    {
        private const int PawnValue = 1000;
        private const int KnightValue = 3200;
        private const int BishopValue = 3300;
        private const int RookValue = 5000;
        private const int QueenValue = 9000;
        private const int KingValue = 100000;

        //private const int CenterBonus = 20;
        //private const int DoubledPawnPenalty = -20;
        //private const int IsolatedPawnPenalty = -10;
        //private const int KingSafetyPenalty = -50;

        private static readonly HashSet<int> CenterSquares = new HashSet<int> { 27, 28, 35, 36 }; // Example indices for d4, d5, e4, e5

        public static int Evaluate(Board board, int color)
        {
            if (board.isGameOver)
            {
                return EvaluateGameOver(board, color);
            }

            int evaluation = 0;

            // Material and positional evaluation
            for (int i = 0; i < 64; i++)
            {
                int piece = board[i];
                int pieceType = Piece.PieceType(piece);
                int pieceColor = Piece.Color(piece);

                int score = GetPieceValue(pieceType);

                //// Positional bonus
                //if (CenterSquares.Contains(pieceLocation))
                //{
                //    score += CenterBonus;
                //}

                //// King safety penalty
                //if (pieceType == Piece.King && IsKingExposed(board, pieceLocation))
                //{
                //    score += KingSafetyPenalty;
                //}

                // Accumulate the score based on piece color
                evaluation += (pieceColor == color) ? score : -score;
            }

            // Pawn structure evaluation
            evaluation += EvaluatePawnStructure(board, color);

            return evaluation;
        }

        private static int EvaluateGameOver(Board board, int color)
        {
            return board.winnerColor switch
            {
                Piece.NoColor => 0, // Draw
                Piece.White => color == Piece.White ? int.MaxValue : int.MinValue,
                Piece.Black => color == Piece.Black ? int.MaxValue : int.MinValue,
                _ => 0
            };
        }

        private static int GetPieceValue(int pieceType)
        {
            return pieceType switch
            {
                Piece.Pawn => PawnValue,
                Piece.Knight => KnightValue,
                Piece.Bishop => BishopValue,
                Piece.Rook => RookValue,
                Piece.Queen => QueenValue,
                Piece.King => KingValue,
                _ => 0
            };
        }

        private static int EvaluatePawnStructure(Board board, int color)
        {
            int score = 0;

            //foreach (int pawnLocation in BoardInfo.PawnLocations(board, color))
            //{
            //    if (IsDoubledPawn(board, pawnLocation, color))
            //    {
            //        score += DoubledPawnPenalty;
            //    }

            //    if (IsIsolatedPawn(board, pawnLocation, color))
            //    {
            //        score += IsolatedPawnPenalty;
            //    }
            //}

            return score;
        }

       private static bool IsKingExposed(Board board, int kingLocation)
{
    return BoardInfo.IsKingExposed(board, kingLocation);
}

private static bool IsDoubledPawn(Board board, int pawnLocation, int color)
{
    return BoardInfo.IsDoubledPawn(board, pawnLocation, color);
}

private static bool IsIsolatedPawn(Board board, int pawnLocation, int color)
{
    return BoardInfo.IsIsolatedPawn(board, pawnLocation, color);
}
    }

}