using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static DuckChess.Board;

namespace DuckChess
{
    public static class BoardEvaluator
    {
        // Piece Values
        private const int PawnValue = 1000;
        private const int KnightValue = 3200;
        private const int BishopValue = 3300;
        private const int RookValue = 5000;
        private const int QueenValue = 9000;
        private const int KingValue = 100000;

        // Positional Bonuses
        private const int MobilityBonus = 10;
        private const int CenterControlBonus = 20;

        // Pawn Structure Penalties and Bonuses
        private const int DoubledPawnPenalty = -20;
        private const int IsolatedPawnPenalty = -10;
        private const int PassedPawnBonus = 30;

        // King Safety Penalty
        private const int KingSafetyPenalty = -50;

        // Duck-Specific Bonuses
        private const int DuckBlockageBonus = 15;
        private const int DuckMobilityBonus = 5;

        // Piece-Square Tables (values scaled by 10 for precision)
        private static readonly int[] PawnTable = new int[64]
        {
            0, 0, 0, 0, 0, 0, 0, 0,
            50, 50, 50, 50, 50, 50, 50, 50,
            10, 10, 20, 30, 30, 20, 10, 10,
            5, 5, 10, 25, 25, 10, 5, 5,
            0, 0, 0, 20, 20, 0, 0, 0,
            5, -5, -10, 0, 0, -10, -5, 5,
            5, 10, 10, -20, -20, 10, 10, 5,
            0, 0, 0, 0, 0, 0, 0, 0
        };

        private static readonly int[] KnightTable = new int[64]
        {
            -50, -40, -30, -30, -30, -30, -40, -50,
            -40, -20, 0, 0, 0, 0, -20, -40,
            -30, 0, 10, 15, 15, 10, 0, -30,
            -30, 5, 15, 20, 20, 15, 5, -30,
            -30, 0, 15, 20, 20, 15, 0, -30,
            -30, 5, 10, 15, 15, 10, 5, -30,
            -40, -20, 0, 5, 5, 0, -20, -40,
            -50, -40, -30, -30, -30, -30, -40, -50
        };

        private static readonly int[] BishopTable = new int[64]
        {
            -20, -10, -10, -10, -10, -10, -10, -20,
            -10, 5, 0, 0, 0, 0, 5, -10,
            -10, 10, 10, 10, 10, 10, 10, -10,
            -10, 0, 10, 10, 10, 10, 0, -10,
            -10, 5, 5, 10, 10, 5, 5, -10,
            -10, 0, 5, 10, 10, 5, 0, -10,
            -10, 0, 0, 0, 0, 0, 0, -10,
            -20, -10, -10, -10, -10, -10, -10, -20
        };

        private static readonly int[] RookTable = new int[64]
        {
            0, 0, 0, 0, 0, 0, 0, 0,
            5, 10, 10, 10, 10, 10, 10, 5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            -5, 0, 0, 0, 0, 0, 0, -5,
            0, 0, 0, 5, 5, 0, 0, 0
        };

        private static readonly int[] QueenTable = new int[64]
        {
            -20, -10, -10, -5, -5, -10, -10, -20,
            -10, 0, 5, 0, 0, 0, 0, -10,
            -10, 5, 5, 5, 5, 5, 0, -10,
            0, 0, 5, 5, 5, 5, 0, -5,
            -5, 0, 5, 5, 5, 5, 0, -5,
            -10, 0, 5, 5, 5, 5, 0, -10,
            -10, 0, 0, 0, 0, 0, 0, -10,
            -20, -10, -10, -5, -5, -10, -10, -20
        };

        private static readonly int[] KingTableMiddleGame = new int[64]
        {
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -30, -40, -40, -50, -50, -40, -40, -30,
            -20, -30, -30, -40, -40, -30, -30, -20,
            -10, -20, -20, -20, -20, -20, -20, -10,
            20, 20, 0, 0, 0, 0, 20, 20,
            20, 30, 10, 0, 0, 10, 30, 20
        };

        private static readonly int[] KingTableEndGame = new int[64]
        {
            -50, -40, -30, -20, -20, -30, -40, -50,
            -30, -20, -10, 0, 0, -10, -20, -30,
            -30, -10, 20, 30, 30, 20, -10, -30,
            -30, -10, 30, 40, 40, 30, -10, -30,
            -30, -10, 30, 40, 40, 30, -10, -30,
            -30, -10, 20, 30, 30, 20, -10, -30,
            -30, -30, 0, 0, 0, 0, -30, -30,
            -50, -30, -30, -30, -30, -30, -30, -50
        };

        // Central Squares
        private static readonly HashSet<int> CenterSquares = new HashSet<int> { 27, 28, 35, 36 }; // d4, d5, e4, e5

        // Helper function to determine if it's endgame based on material
        public static bool IsEndGame(Board board)
        {
            Board.PieceList[] pieceLocations = board.AllPieceLocations;
            // Simple criteria: if total material is below a threshold, consider it endgame
            const int threshold =
                (PawnValue * 8 + KnightValue * 2 + BishopValue * 2 + RookValue * 4 + QueenValue * 2);
            int totalMaterial = 0;
            for (int i = 0; i < pieceLocations.Length; i++)
            {
                int pieceType = Piece.PieceType(pieceLocations[i].piece);
                totalMaterial += GetPieceValue(pieceType) * pieceLocations[i].Length;
            }
            return totalMaterial < threshold;
        }
        private static bool IsEndGame(Board board, Board.PieceList[] pieceLocations)
        {
            // Simple criteria: if total material is below a threshold, consider it endgame
            const int threshold =
                (PawnValue * 8 + KnightValue * 2 + BishopValue * 2 + RookValue * 4 + QueenValue * 2);
            int totalMaterial = 0;
            for (int i = 0; i < pieceLocations.Length; i++)
            {
                int pieceType = Piece.PieceType(pieceLocations[i].piece);
                totalMaterial += GetPieceValue(pieceType) * pieceLocations[i].Length;
            }
            return totalMaterial < threshold;
        }
        public static int Evaluate(Board board, int playerColor)
        {
            if (board.isGameOver)
            {
                return EvaluateGameOver(board, playerColor);
            }

            int evaluation = 0;

            Board.PieceList[] pieceLocations = board.AllPieceLocations;
            bool isEndGame = IsEndGame(board, pieceLocations);

            // Mobility evaluation
            evaluation += EvaluateMobility(board, playerColor);

            // In each iteration of this loop, we are going through the locations of
            // a given piece type and color.
            for (int i = 0; i < pieceLocations.Length; i++)
            {
                Board.PieceList pieceLocation = pieceLocations[i];
                bool isEnemyPiece = !Piece.IsColor(pieceLocation.piece, playerColor);
                // Evaluate the material bonus based on the number of
                // pieces of this type and color on the board
                evaluation += EvaluateMaterial(pieceLocation, playerColor);
                // Control of the center
                evaluation += EvaluateCenterControl(board, playerColor);

                // In each iteration of this loop, we are checking out
                // a specific piece
                for (int j = 0; j < pieceLocation.Length; j++)
                {
                    int pos = pieceLocation[j];
                    // Evaluate how good the position of this piece is
                    evaluation += EvaluatePosition(
                        board,
                        pos,
                        isEnemyPiece,
                        isEndGame
                    );
                    // If this is a pawn piece list, evaluate the structure
                    if (Piece.PieceType(pieceLocations[i].piece) == Piece.Pawn)
                    {
                        evaluation += EvaluatePawnStructure(
                            board,
                            pos,
                            playerColor,
                            isEnemyPiece
                        );
                    }
                    // If this is a king piece list, evaluate the king safety
                    if (Piece.PieceType(pieceLocations[i].piece) == Piece.Pawn)
                    {
                        evaluation += EvaluateKingSafety(
                            board,
                            pos,
                            isEnemyPiece
                        );
                    }
                }
            }

            // Duck-specific evaluation
            evaluation += EvaluateDuckImpact(board, playerColor);

            return evaluation;
        }

        // 1. Material Evaluation
        private static int EvaluateMaterial(Board.PieceList pieceList, int playerColor)
        {
            int pieceType = Piece.PieceType(pieceList.piece);
            int pieceColor = Piece.Color(pieceList.piece);

            int value = GetPieceValue(pieceType) * pieceList.Length;

            return pieceColor == playerColor ? value : -value;
        }

        // 2. Positional Evaluation
        private static int EvaluatePosition(Board board, int pieceSpot, bool isEnemy, bool isEndGame)
        {
            int piece = board[pieceSpot];
            int pieceType = Piece.PieceType(piece);
            int pieceColor = Piece.Color(piece);
            int value = GetPositionValue(pieceType, pieceSpot, pieceColor, isEndGame);

            return (isEnemy) ? -value : value;
        }

        private static int GetPositionValue(int pieceType, int position, int color, bool endGame)
        {
            // Flip the board for Black pieces
            int index = color == Piece.White ? position : 63 - position;

            return pieceType switch
            {
                Piece.Pawn => PawnTable[index],
                Piece.Knight => KnightTable[index],
                Piece.Bishop => BishopTable[index],
                Piece.Rook => RookTable[index],
                Piece.Queen => QueenTable[index],
                Piece.King => endGame ? KingTableEndGame[index] : KingTableMiddleGame[index],
                _ => 0
            };
        }

        // 3. Pawn Structure Evaluation
        private static int EvaluatePawnStructure(Board board, int pos, int playerColor, bool isEnemyPawn)
        {
            int score = 0;
            if (IsDoubledPawn(board, pos, playerColor))
                score += DoubledPawnPenalty;

            if (IsIsolatedPawn(board, pos, playerColor))
                score += IsolatedPawnPenalty;

            if (IsPassedPawn(board, pos, playerColor))
                score += PassedPawnBonus;

            return isEnemyPawn ? -score : score;
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
        private static int EvaluateKingSafety(Board board, int kingPos, bool isEnemy)
        {
            int score = IsKingExposed(board, kingPos) ? KingSafetyPenalty : 0;
            return isEnemy ? -score : score;
        }

        private static bool IsKingExposed(Board board, int kingPosition)
        {
            // number of enemy pieces attacking the king
            int attackingPieces = 0;
            int kingRow = BoardInfo.GetRow(kingPosition);
            int kingCol = BoardInfo.GetFile(kingPosition);
            int enemyColor = Piece.OpponentColor(Piece.Color(board[kingPosition]));

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
            };

            foreach (var dir in directions)
            {
                int r = kingRow + dir[0];
                int c = kingCol + dir[1];
                while (r >= 0 && r < 8 && c >= 0 && c < 8)
                {
                    int pos = BoardInfo.GetSquare(r, c);
                    int piece = board[pos];
                    if (piece == Piece.None)
                    {
                        r += dir[0];
                        c += dir[1];
                        continue;
                    }

                    if (Piece.Color(piece) == enemyColor)
                    {
                        int pieceType = Piece.PieceType(piece);
                        if (IsAttackingDirection(pieceType, dir))
                        {
                            attackingPieces++;
                        }
                    }
                    break;
                }
            }

            return attackingPieces > 2; // Threshold can be adjusted
        }

        private static bool IsAttackingDirection(int pieceType, int[] direction)
        {
            switch (pieceType)
            {
                case Piece.Queen:
                    return true;
                case Piece.Rook:
                    return direction[0] == 0 || direction[1] == 0;
                case Piece.Bishop:
                    return direction[0] != 0 && direction[1] != 0;
                case Piece.Knight:
                    // Knights have unique movement; handle separately if needed
                    return false;
                default:
                    return false;
            }
        }

        private static int EvaluateMobility(Board board, int color)
        {
            int score = 0;

            // check mobility of all legal moves
            foreach (Move move in board.legalMoves)
            {
                // check piece color
                int piece = board[move.StartSquare];
                if (Piece.Color(piece) == color)
                {
                    score += MobilityBonus;
                }
            }

            return score;
        }

    // 6. Control of the Center
        private static int EvaluateCenterControl(Board board, int color)
        {
            int score = 0;

            foreach (int square in CenterSquares)
            {
                int piece = board[square];
                if (piece == Piece.None)
                    continue;

                if (Piece.Color(piece) == color)
                {
                    score += CenterControlBonus;
                }
                else if (Piece.Color(piece) == Piece.OpponentColor(color))
                {
                    score -= CenterControlBonus;
                }
            }

            return score;
        }

        // 7. Duck-Specific Evaluation
        private static int EvaluateDuckImpact(Board board, int color)
        {
            int score = 0;

            // Assess how the duck is affecting the opponent
            score += EvaluateDuckBlockage(board, color);

            // Potential duck placements
            score += EvaluateDuckPlacementOptions(board, color);

            return score;
        }

        private static int EvaluateDuckBlockage(Board board, int color)
        {
            int score = 0;

            // Get the duck's position
            int duckPosition = board.GetLocationOfPieces(Piece.Duck)[0];
            if (duckPosition == -1)
            {
                // If the duck is not on the board, no blockage can occur
                return score;
            }

            // Locate the king of the specified color
            int kingPosition = board.GetLocationOfPieces(Piece.King, color)[0];

            // Check legal moves to identify enemy piece attacks
            foreach (Move move in board.legalMoves)
            {
                int movingPiece = board[move.StartSquare];
                int pieceType = Piece.PieceType(movingPiece);
                int pieceColor = Piece.Color(movingPiece);

                // Only consider moves from the opponent
                if (pieceColor == color)
                {
                    continue;
                }

                // Focus on sliding pieces (Queen, Rook, Bishop)
                if (pieceType == Piece.Queen || pieceType == Piece.Rook || pieceType == Piece.Bishop)
                {
                    // Check if the duck blocks the line between the attacking piece and the king
                    if (IsBlocking(move.StartSquare, kingPosition, duckPosition))
                    {
                        score += DuckBlockageBonus;
                    }
                }
            }

            return score;
        }


        private static bool IsBlocking(int attackerPos, int targetPos, int blockPos)
        {
            // Determine if blockPos is on the line between attackerPos and targetPos
            int attackerRow = BoardInfo.GetRow(attackerPos);
            int attackerCol = BoardInfo.GetFile(attackerPos);
            int targetRow = BoardInfo.GetRow(targetPos);
            int targetCol = BoardInfo.GetFile(targetPos);
            int blockRow = BoardInfo.GetRow(blockPos);
            int blockCol = BoardInfo.GetFile(blockPos);

            int rowDir = Math.Sign(targetRow - attackerRow);
            int colDir = Math.Sign(targetCol - attackerCol);

            int currentRow = attackerRow + rowDir;
            int currentCol = attackerCol + colDir;

            while (currentRow != targetRow || currentCol != targetCol)
            {
                if (currentRow == blockRow && currentCol == blockCol)
                    return true;

                currentRow += rowDir;
                currentCol += colDir;

                if (currentRow < 0 || currentRow > 7 || currentCol < 0 || currentCol > 7)
                    break;
            }

            return false;
        }

        private static int EvaluateDuckPlacementOptions(Board board, int color)
        {
            int score = 0;

            // Filter legalMoves for duck moves
            foreach (Move move in board.legalMoves)
            {
                // Check if the move is a duck move
                int movingPiece = board[move.StartSquare];
                if (Piece.PieceType(movingPiece) == Piece.Duck)
                {
                    score += DuckMobilityBonus;
                }
            }

            return score;
        }


        // Helper Methods

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
                    Piece.Duck => 2000, // Assign a value to the Duck
                    _ => 0
                };
            }

        private static int EvaluateGameOver(Board board, int color)
        {
            return board.winnerColor switch
            {
                Piece.NoColor => 0, // Draw
                Piece.White => color == Piece.White ? 1000000 : -1000000,
                Piece.Black => color == Piece.Black ? 1000000 : -1000000,
                _ => 0
            };
        }

    }
}
