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

     /// <summary>
        /// Generate all the duck moves and add them into the provided list.
        /// </summary>
        /// <param name="generatedMoves">The list to add the generated moves into.</param>
        /// <param name="board">The board to use while checking for legal duck moves.</param>
        public static void GenerateDuckMoves(ref List<Move> generatedMoves, Board board)
        {
            // The starting square for the duck (its current position).
            int startSquare = board.Duck;

            // If the duck is not on the board, it starts at square 0.
            int firstDuckMoveFlag = Move.Flag.None;
            if (startSquare == Board.NOT_ON_BOARD)
            {
                startSquare = 0;
                firstDuckMoveFlag = Move.Flag.FirstDuckMove;
            }

            // Iterate over all squares to find empty ones for the duck to move to.
            for (int i = 0; i < board.Squares.Length; i++)
            {
                // The duck can only move to empty squares.
                if (Piece.PieceType(board[i]) == Piece.None)
                {
                    int targetSquare = i;

                    // Create a duck move.
                    Move duckMove = new Move(startSquare, targetSquare, firstDuckMoveFlag);

                    // Debugging: Log invalid duck moves.
                    if (duckMove.StartSquare == 0 && duckMove.TargetSquare == 0)
                    {
                        Debug.LogWarning("Generated invalid duck move: " + board.ToString());
                    }

                    // Add the move to the list of generated moves.
                    generatedMoves.Add(duckMove);
                }
            }
        }


        public static void GeneratePawnMoves(ref List<Move> generatedMoves, Board board)
        {
            bool isWhite = board.turnColor == Piece.White;
            int enemyColor = isWhite ? Piece.Black : Piece.White;
            PieceList pawnLocations = isWhite ? board.WhitePawns : board.BlackPawns;

            int findSpotOneFront = isWhite ? 8 : -8;
            int rowBeforePromotion = isWhite ? 6 : 1;
            int startRow = isWhite ? 1 : 6;
            int findCaptureSpotOnRight = isWhite ? 9 : -7;
            int findCaptureSpotOnLeft = isWhite ? 7 : -9;

            for (int i = 0; i < pawnLocations.Count; i++)
            {
                int pawnSpot = pawnLocations[i];

                GeneratePawnForwardMoves(ref generatedMoves, board, pawnSpot, isWhite, findSpotOneFront, rowBeforePromotion, startRow);
                GeneratePawnCaptureMoves(ref generatedMoves, board, pawnSpot, enemyColor, findCaptureSpotOnRight, findCaptureSpotOnLeft, rowBeforePromotion);
                GenerateEnPassantMoves(ref generatedMoves, board, pawnSpot, findCaptureSpotOnLeft, findCaptureSpotOnRight);
            }
        }

        private static void GeneratePawnForwardMoves(ref List<Move> generatedMoves, Board board, int pawnSpot, bool isWhite, int findSpotOneFront, int rowBeforePromotion, int startRow)
        {
            int spotInFrontOfPawn = pawnSpot + findSpotOneFront;
            int spotTwoInFrontOfPawn = spotInFrontOfPawn + findSpotOneFront;

            if (Piece.PieceType(board[spotInFrontOfPawn]) == Piece.None)
            {
                if (Board.GetRowOf(pawnSpot) == rowBeforePromotion)
                {
                    AddPawnPromotions(ref generatedMoves, pawnSpot, spotInFrontOfPawn);
                }
                else
                {
                    generatedMoves.Add(new Move(pawnSpot, spotInFrontOfPawn));
                }

                if (Board.GetRowOf(pawnSpot) == startRow && Piece.PieceType(board[spotTwoInFrontOfPawn]) == Piece.None)
                {
                    generatedMoves.Add(new Move(pawnSpot, spotTwoInFrontOfPawn, Move.Flag.PawnTwoForward));
                }
            }
        }

        private static void GeneratePawnCaptureMoves(ref List<Move> generatedMoves, Board board, int pawnSpot, int enemyColor, int findCaptureSpotOnRight, int findCaptureSpotOnLeft, int rowBeforePromotion)
        {
            int rightCaptureSpot = pawnSpot + findCaptureSpotOnRight;
            int leftCaptureSpot = pawnSpot + findCaptureSpotOnLeft;

            int pawnCol = Board.GetColumnOf(pawnSpot);

            if (pawnCol < 7 && Piece.Color(board[rightCaptureSpot]) == enemyColor)
            {
                if (Board.GetRowOf(pawnSpot) == rowBeforePromotion)
                {
                    AddPawnPromotions(ref generatedMoves, pawnSpot, rightCaptureSpot);
                }
                else
                {
                    generatedMoves.Add(new Move(pawnSpot, rightCaptureSpot));
                }
            }

            if (pawnCol > 0 && Piece.Color(board[leftCaptureSpot]) == enemyColor)
            {
                if (Board.GetRowOf(pawnSpot) == rowBeforePromotion)
                {
                    AddPawnPromotions(ref generatedMoves, pawnSpot, leftCaptureSpot);
                }
                else
                {
                    generatedMoves.Add(new Move(pawnSpot, leftCaptureSpot));
                }
            }
        }

        private static void GenerateEnPassantMoves(ref List<Move> generatedMoves, Board board, int pawnSpot, int leftCaptureSpot, int rightCaptureSpot)
        {
            if (Board.GetRowOf(pawnSpot) == 4)
            {
                if (leftCaptureSpot == board.enPassantSquare || rightCaptureSpot == board.enPassantSquare)
                {
                    if (board.Duck != board.enPassantSquare)
                    {
                        generatedMoves.Add(new Move(pawnSpot, board.enPassantSquare, Move.Flag.EnPassantCapture));
                    }
                }
            }
        }

        private static void AddPawnPromotions(ref List<Move> generatedMoves, int startSquare, int targetSquare)
        {
            generatedMoves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToKnight));
            generatedMoves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToBishop));
            generatedMoves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToRook));
            generatedMoves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToQueen));
        }

        public static void GenerateKnightMoves(ref List<Move> generatedMoves, Board board)
        {
            bool isWhite = board.turnColor == Piece.White;
            PieceList knightLocations = isWhite ? board.WhiteKnights : board.BlackKnights;
            int enemyColor = isWhite ? Piece.Black : Piece.White;

            for (int i = 0; i < knightLocations.Count; i++)
            {
                int knightSpot = knightLocations[i];
                GenerateKnightJumps(ref generatedMoves, board, knightSpot, enemyColor);
            }
        }

        private static void GenerateKnightJumps(ref List<Move> generatedMoves, Board board, int knightSpot, int enemyColor)
        {
            int[] knightOffsets = { 15, 17, -15, -17, 10, -10, 6, -6 };

            for (int j = 0; j < knightOffsets.Length; j++)
            {
                int targetSpot = knightSpot + knightOffsets[j];

                if (targetSpot >= 0 && targetSpot < 64 &&
                    (Piece.PieceType(board[targetSpot]) == Piece.None || Piece.Color(board[targetSpot]) == enemyColor))
                {
                    generatedMoves.Add(new Move(knightSpot, targetSpot));
                }
            }
        }

        public static void GenerateBishopMoves(ref List<Move> generatedMoves, Board board)
        {
            bool isWhite = board.turnColor == Piece.White;
            PieceList bishopLocations = isWhite ? board.WhiteBishops : board.BlackBishops;
            int enemyColor = isWhite ? Piece.Black : Piece.White;

            for (int i = 0; i < bishopLocations.Count; i++)
            {
                int bishopSpot = bishopLocations[i];
                GenerateDiagonalMoves(ref generatedMoves, board, bishopSpot, enemyColor);
            }
        }

        private static void GenerateDiagonalMoves(ref List<Move> generatedMoves, Board board, int spotOfPiece, int enemyColor)
        {
            int[] diagonalOffsets = { 7, 9, -7, -9 };

            foreach (int offset in diagonalOffsets)
            {
                GenerateSlidingMovesInDirection(ref generatedMoves, board, spotOfPiece, offset, enemyColor);
            }
        }

        public static void GenerateRookMoves(ref List<Move> generatedMoves, Board board)
        {
            bool isWhite = board.turnColor == Piece.White;
            PieceList rookLocations = isWhite ? board.WhiteRooks : board.BlackRooks;
            int enemyColor = isWhite ? Piece.Black : Piece.White;

            for (int i = 0; i < rookLocations.Count; i++)
            {
                int rookSpot = rookLocations[i];
                GenerateOrthogonalMoves(ref generatedMoves, board, rookSpot, enemyColor);
            }
        }

        private static void GenerateOrthogonalMoves(ref List<Move> generatedMoves, Board board, int spotOfPiece, int enemyColor)
        {
            int[] orthogonalOffsets = { 1, -1, 8, -8 };

            foreach (int offset in orthogonalOffsets)
            {
                GenerateSlidingMovesInDirection(ref generatedMoves, board, spotOfPiece, offset, enemyColor);
            }
        }

        private static void GenerateSlidingMovesInDirection(ref List<Move> generatedMoves, Board board, int spotOfPiece, int offset, int enemyColor)
        {
            int currentSpot = spotOfPiece;

            while (true)
            {
                currentSpot += offset;

                if (currentSpot < 0 || currentSpot >= 64)
                    break;

                int pieceAtTarget = board[currentSpot];
                bool targetIsEmpty = Piece.PieceType(pieceAtTarget) == Piece.None;
                bool targetHasEnemy = Piece.Color(pieceAtTarget) == enemyColor;

                if (targetIsEmpty || targetHasEnemy)
                {
                    generatedMoves.Add(new Move(spotOfPiece, currentSpot));
                }

                if (!targetIsEmpty)
                    break;
            }
        }

        public static void GenerateQueenMoves(ref List<Move> generatedMoves, Board board)
        {
            bool isWhite = board.turnColor == Piece.White;
            PieceList queenLocations = isWhite ? board.WhiteQueens : board.BlackQueens;
            int enemyColor = isWhite ? Piece.Black : Piece.White;

            for (int i = 0; i < queenLocations.Count; i++)
            {
                int queenSpot = queenLocations[i];
                GenerateDiagonalMoves(ref generatedMoves, board, queenSpot, enemyColor);
                GenerateOrthogonalMoves(ref generatedMoves, board, queenSpot, enemyColor);
            }
        }

        public static void GenerateKingMoves(ref List<Move> generatedMoves, Board board)
        {
            bool isWhite = board.turnColor == Piece.White;
            int kingSpot = isWhite ? board.WhiteKing : board.BlackKing;
            int enemyColor = isWhite ? Piece.Black : Piece.White;

            GenerateKingMovesInternal(ref generatedMoves, board, kingSpot, enemyColor);
            GenerateCastlingMoves(ref generatedMoves, board, kingSpot, isWhite);
        }

        private static void GenerateKingMovesInternal(ref List<Move> generatedMoves, Board board, int kingSpot, int enemyColor)
        {
            int[] kingOffsets = { 1, -1, 8, -8, 9, -9, 7, -7 };

            foreach (int offset in kingOffsets)
            {
                int targetSpot = kingSpot + offset;

                if (targetSpot >= 0 && targetSpot < 64 &&
                    (Piece.PieceType(board[targetSpot]) == Piece.None || Piece.Color(board[targetSpot]) == enemyColor))
                {
                    generatedMoves.Add(new Move(kingSpot, targetSpot));
                }
            }
        }

        private static void GenerateCastlingMoves(ref List<Move> generatedMoves, Board board, int kingSpot, bool isWhite)
        {
            if (isWhite ? board.CastleKingSideW : board.CastleKingSideB)
            {
                if (Piece.PieceType(board[kingSpot + 1]) == Piece.None &&
                    Piece.PieceType(board[kingSpot + 2]) == Piece.None &&
                    Piece.PieceType(board[kingSpot + 3]) == Piece.Rook)
                {
                    generatedMoves.Add(new Move(kingSpot, kingSpot + 2, Move.Flag.Castling));
                }
            }

            if (isWhite ? board.CastleQueenSideW : board.CastleQueenSideB)
            {
                if (Piece.PieceType(board[kingSpot - 1]) == Piece.None &&
                    Piece.PieceType(board[kingSpot - 2]) == Piece.None &&
                    Piece.PieceType(board[kingSpot - 3]) == Piece.None &&
                    Piece.PieceType(board[kingSpot - 4]) == Piece.Rook)
                {
                    generatedMoves.Add(new Move(kingSpot, kingSpot - 2, Move.Flag.Castling));
                }
            }
        }
    }
}
