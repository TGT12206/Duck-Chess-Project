using System;
using System.Collections.Generic;

namespace DuckChess
{
    public static class BoardEvaluator
    {
        // Piece values (using existing values for standard pieces)
        private const int PawnValue = 1000;
        private const int KnightValue = 3200;
        private const int BishopValue = 3300;
        private const int RookValue = 5000;
        private const int QueenValue = 9000;
        private const int KingValue = 100000;
        private const int DuckValue = 5000; // Duck value: higher to reflect blocking and control

        // Penalty values for blocking our own pieces
        private const int OwnBlockPenalty = -200; // For blocking own pieces

        // Bonus for controlling center squares
        private const int CenterBonus = 20;

        // Rewards for checks and valuable piece checks
        private const int CheckBonus = 50;
        private const int ValuableCheckBonus = 200; // High reward for checking valuable pieces

        public static int EvaluateNormal(Board board, int color)
        {
            if (board.isGameOver)
            {
                return EvaluateGameOver(board, color);
            }

            int evaluation = 0;

            PieceList allPieces = board.AllPieces;

            // Material and positional evaluation
            for (int i = 0; i < allPieces.Count; i++)
            {
                int pieceLocation = allPieces[i];
                int piece = board[pieceLocation];
                int pieceType = Piece.PieceType(piece);
                int pieceColor = Piece.Color(piece);

                int score = GetPieceValue(pieceType);

                // Check if piece is a Duck and add Duck-specific evaluation
                if (pieceType == Piece.Duck)
                {
                    score += DuckValue; // Encourage Duck usage for blocking and control
                    // Check if the Duck is blocking its own pieces
                    if (IsBlockingOwnPieces(board, pieceLocation, pieceColor))
                    {
                        score += OwnBlockPenalty;
                    }
                }

                // Positional bonus for controlling the center
                if (IsInCenter(pieceLocation))
                {
                    score += CenterBonus;
                }

                // Check if the piece is putting the opponent's king in check
                if (IsCheckingOpponentKing(board, pieceLocation, pieceColor))
                {
                    score += CheckBonus;
                }

                // Reward checking valuable pieces
                if (IsCheckingValuablePiece(board, pieceLocation, pieceColor))
                {
                    score += ValuableCheckBonus;
                }

                // Accumulate the score based on piece color
                evaluation += (pieceColor == color) ? score : -score;
            }

            return evaluation;
        }

        public static int EvaluateDuck(Board board, int color)
        {
            int evaluation = 0;

            PieceList allPieces = board.AllPieces;

            // Duck-specific evaluation logic
            for (int i = 0; i < allPieces.Count; i++)
            {
                int pieceLocation = allPieces[i];
                int piece = board[pieceLocation];
                int pieceType = Piece.PieceType(piece);
                int pieceColor = Piece.Color(piece);

                if (pieceType == Piece.Duck)
                {
                    // For Ducks: prioritize blocking opponent's attack avenues while leaving our own open
                    if (IsBlockingOpponent(board, pieceLocation, pieceColor))
                    {
                        evaluation += DuckValue;
                    }
                    else
                    {
                        // Penalize Ducks if they block our attack avenues
                        evaluation -= OwnBlockPenalty;
                    }

                    // Encourage Duck placements that force opponent into check
                    if (IsPuttingOpponentInCheck(board, pieceLocation, pieceColor))
                    {
                        evaluation += CheckBonus;
                    }

                    // Encourage Duck placements that help in pinning valuable pieces
                    if (IsPinningValuablePiece(board, pieceLocation, pieceColor))
                    {
                        evaluation += ValuableCheckBonus;
                    }
                }
            }

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
                Piece.Duck => DuckValue, // Duck piece evaluation
                _ => 0
            };
        }

        private static bool IsInCenter(int square)
        {
            // Center squares (e.g., d4, d5, e4, e5)
            HashSet<int> centerSquares = new HashSet<int> { 27, 28, 35, 36 };
            return centerSquares.Contains(square);
        }

        private static bool IsBlockingOwnPieces(Board board, int pieceLocation, int color)
        {
            // Logic to check if the Duck is blocking its own pieces
            // (This could be more complex, depending on the specific piece arrangement)
            return false; // Placeholder logic; to be refined
        }

        private static bool IsBlockingOpponent(Board board, int pieceLocation, int color)
        {
            // Check if the Duck is blocking opponent's pieces
            // Check if the Duck is occupying squares where opponent's pieces could move
            return false; // Placeholder logic; to be refined
        }

        private static bool IsCheckingOpponentKing(Board board, int pieceLocation, int color)
        {
            // Logic to check if the piece is putting the opponent's king in check
            return false; // Placeholder logic; to be refined
        }

        private static bool IsCheckingValuablePiece(Board board, int pieceLocation, int color)
        {
            // Logic to check if the piece is putting an opponent's valuable piece (e.g., Queen) in check
            return false; // Placeholder logic; to be refined
        }

        private static bool IsPinningValuablePiece(Board board, int pieceLocation, int color)
        {
            // Logic to check if the piece is pinning an opponent's valuable piece
            return false; // Placeholder logic; to be refined
        }

        private static bool IsPuttingOpponentInCheck(Board board, int pieceLocation, int color)
        {
            // Logic to check if the Duck is placing the opponent's king in check
            return false; // Placeholder logic; to be refined
        }
    }
}
