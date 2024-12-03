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
<<<<<<< Updated upstream
                Piece.NoColor => 0, // Draw
                Piece.White => color == Piece.White ? int.MaxValue : int.MinValue,
                Piece.Black => color == Piece.Black ? int.MaxValue : int.MinValue,
=======
                int piece = board[i];
                if (piece == Piece.None)
                    continue;

                int pieceType = Piece.PieceType(piece);
                int pieceColor = Piece.Color(piece);

                int value = GetPieceValue(pieceType);

                if (pieceColor == color)
                {
                    material += value;
                }
                else
                {
                    material -= value;
                }
            }
            return material;
        }

        // 2. Positional Evaluation
        private static int EvaluatePosition(Board board, int color)
        {
            int positionalScore = 0;
            bool endGame = IsEndGame(board);

            for (int i = 0; i < 64; i++)
            {
                int piece = board[i];
                if (piece == Piece.None)
                    continue;

                int pieceType = Piece.PieceType(piece);
                int pieceColor = Piece.Color(piece);

                if (pieceColor == color)
                {
                    positionalScore += GetPositionValue(pieceType, i, color, endGame);
                }
                else
                {
                    positionalScore -= GetPositionValue(pieceType, i, Piece.OpponentColor(color), endGame);
                }
            }

            return positionalScore;
        }


        private static int GetPositionValue(int pieceType, int position, int color, bool endGame)
        {
            // Flip the board for Black pieces
            int index = color == Piece.White ? position : 63 - position;

            // Ensure the index is within the bounds of the piece-square table arrays
            if (index < 0 || index >= 64)
            {
                Debug.LogError($"Invalid index {index} for pieceType {pieceType}, position {position}, color {color}");
                return 0; // Return a neutral score if the index is invalid
            }

            return pieceType switch
            {
                Piece.Pawn => PawnTable[index],
                Piece.Knight => KnightTable[index],
                Piece.Bishop => BishopTable[index],
                Piece.Rook => RookTable[index],
                Piece.Queen => QueenTable[index],
                Piece.King => endGame ? KingTableEndGame[index] : KingTableMiddleGame[index],
>>>>>>> Stashed changes
                _ => 0
            };
        }

<<<<<<< Updated upstream
        public static int GetPieceValue(int pieceType)
=======

        // 3. Pawn Structure Evaluation
        private static int EvaluatePawnStructure(Board board, int color)
>>>>>>> Stashed changes
        {
            return pieceType switch
            {
<<<<<<< Updated upstream
                Piece.Pawn => PawnValue,
                Piece.Knight => KnightValue,
                Piece.Bishop => BishopValue,
                Piece.Rook => RookValue,
                Piece.Queen => QueenValue,
                Piece.King => KingValue,
                _ => 0
=======
                if (IsDoubledPawn(board, pos, color))
                    score += DoubledPawnPenalty;

                if (IsIsolatedPawn(board, pos, color))
                    score += IsolatedPawnPenalty;

                if (IsPassedPawn(board, pos, color))
                    score += PassedPawnBonus;
            }

            return score;
        }

        private static bool IsDoubledPawn(Board board, int pawnPosition, int color)
        {
            int file = BoardInfo.GetFile(pawnPosition);
            int rank = BoardInfo.GetRank(pawnPosition);

            for (int r = 0; r < 8; r++)
            {
                if (r == rank)
                    continue;

                int otherPawnPos = BoardInfo.GetSquare(r, file);
                if (board[otherPawnPos] == (color | Piece.Pawn))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsIsolatedPawn(Board board, int pawnPosition, int color)
        {
            int file = BoardInfo.GetFile(pawnPosition);

            // Check adjacent files for any pawns of the same color
            if (file > 0)
            {
                for (int r = 0; r < 8; r++)
                {
                    if (board[BoardInfo.GetSquare(r, file - 1)] == (color | Piece.Pawn))
                        return false;
                }
            }

            if (file < 7)
            {
                for (int r = 0; r < 8; r++)
                {
                    if (board[BoardInfo.GetSquare(r, file + 1)] == (color | Piece.Pawn))
                        return false;
                }
            }

            return true;
        }

        private static bool IsPassedPawn(Board board, int pawnPosition, int color)
        {
            int file = BoardInfo.GetFile(pawnPosition);
            int rank = BoardInfo.GetRank(pawnPosition);
            int direction = color == Piece.White ? 1 : -1;

            for (int r = rank + direction; r >= 0 && r < 8; r += direction)
            {
                // Check the file and adjacent files for enemy pawns
                for (int f = Math.Max(0, file - 1); f <= Math.Min(7, file + 1); f++)
                {
                    int pos = BoardInfo.GetSquare(r, f);
                    if (board[pos] == (Piece.OpponentColor(color) | Piece.Pawn))
                        return false;
                }
            }

            return true;
        }

        // 4. King Safety Evaluation
        private static int EvaluateKingSafety(Board board, int color)
        {
            int score = 0;
            int kingPos = BoardInfo.KingLocation(board, color);

            if (IsKingExposed(board, kingPos))
            {
                score += KingSafetyPenalty;
            }

            return score;
        }

        private static bool IsKingExposed(Board board, int kingPosition)
        {
            // number of enemy pieces attacking the king
            int attackingPieces = 0;
            int kingRow = BoardInfo.GetRow(kingPosition);
            int kingCol = BoardInfo.GetFile(kingPosition);
            int enemyColor = Piece.NoColor; // Default to no color if invalid

            // Validate kingPosition and the piece at that position
            if (kingPosition >= 0 && kingPosition < 64 && board[kingPosition] != Piece.None)
            {
                enemyColor = Piece.OpponentColor(Piece.Color(board[kingPosition]));
            }
            else
            {
                Debug.LogError($"Invalid king position or piece at position: {kingPosition}");
            }


            // directions for attacks
            int[][] directions = new int[][]
            {
                new int[] {1, 0}, // Up
                new int[] {-1, 0}, // Down
                new int[] {0, 1}, // Right
                new int[] {0, -1}, // Left
                new int[] {1, 1}, // Up-Right
                new int[] {1, -1}, // Up-Left
                new int[] {-1, 1}, // Down-Right
                new int[] {-1, -1} // Down-Left
>>>>>>> Stashed changes
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