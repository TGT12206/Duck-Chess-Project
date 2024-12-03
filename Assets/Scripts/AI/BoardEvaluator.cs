using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace DuckChess
{
    public static class BoardEvaluator
    {
        private static System.Random random = new System.Random();
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

        public static int GetPieceValue(int pieceType)
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

        // NEW METHOD: GetHeuristicMove
        public static Move GetHeuristicMove(Board board, List<Move> moves, int currentPlayer)
        {
            // Assign a score to each move based on heuristic evaluation
            List<Tuple<Move, int>> scoredMoves = new List<Tuple<Move, int>>();
            foreach (var move in moves)
            {
                int score = EvaluateMove(board, move, currentPlayer);
                scoredMoves.Add(new Tuple<Move, int>(move, score));
            }

            // Sort moves based on the score (higher is better)
            scoredMoves.Sort((a, b) => b.Item2.CompareTo(a.Item2));

            // Select the best move with some randomness
            int topN = Math.Min(5, scoredMoves.Count);
            int selectedIndex = random.Next(topN);
            return scoredMoves[selectedIndex].Item1;
        }

        // NEW METHOD: EvaluateMove
        public static int EvaluateMove(Board board, Move move, int player)
        {
            int score = 0;

            int startPiece = board.Squares[move.StartSquare];
            int targetPiece = board.Squares[move.TargetSquare];

            int pieceType = Piece.PieceType(startPiece);
            int pieceColor = Piece.Color(startPiece);

            // Encourage capturing opponent's pieces
            if (Piece.Color(targetPiece) != Piece.NoColor && Piece.Color(targetPiece) != pieceColor)
            {
                int capturedPieceValue = BoardEvaluator.GetPieceValue(Piece.PieceType(targetPiece));
                score += capturedPieceValue * 10; // Higher weight for captures
            }

            // Encourage advancing pawns
            if (pieceType == Piece.Pawn)
            {
                int direction = (pieceColor == Piece.White) ? 1 : -1;
                int startRow = BoardInfo.GetRow(move.StartSquare);
                int targetRow = BoardInfo.GetRow(move.TargetSquare);
                score += (targetRow - startRow) * direction * 2;
            }

            // Encourage central control
            int targetCol = BoardInfo.GetFile(move.TargetSquare);
            int targetRowCentrality = (int)Math.Abs(3.5f - BoardInfo.GetRow(move.TargetSquare));
            int targetColCentrality = (int)Math.Abs(3.5f - targetCol);
            score += (int)(7 - (targetRowCentrality + targetColCentrality)); // Closer to center gets higher score

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