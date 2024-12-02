using System.Collections.Generic;
using UnityEngine;

namespace DuckChess
{
    /// <summary>
    /// A static class containing methods to generate a list of legal moves
    /// for the given piece type on that turn, or all moves for that turn.
    /// </summary>
    public static class NewLegalMoveGenerator
    {
        /// <summary>
        /// Generate all the pawn moves for the color of the board
        /// and add them into the provided list.
        /// </summary>
        /// <param name="generatedMoves">The list to add the generated moves into.</param>
        /// <param name="board">The board to use while checking for legal moves.</param>
        public static void GeneratePawnMoves(ref List<Move> generatedMoves, Board board)
        {
            if (board.isGameOver)
                return;

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
                GeneratePawnMovesForOne(
                    ref generatedMoves, 
                    board, 
                    pawnSpot, 
                    isWhite, 
                    enemyColor, 
                    findSpotOneFront, 
                    rowBeforePromotion, 
                    startRow, 
                    findCaptureSpotOnRight, 
                    findCaptureSpotOnLeft
                );
            }
        }

        private static void GeneratePawnMovesForOne(ref List<Move> generatedMoves, Board board, int pawnSpot, bool isWhite, int enemyColor, int findSpotOneFront, int rowBeforePromotion, int startRow, int findCaptureSpotOnRight, int findCaptureSpotOnLeft)
        {
            GeneratePawnForwardMoves(ref generatedMoves, board, pawnSpot, isWhite, findSpotOneFront, rowBeforePromotion, startRow);
            GeneratePawnCaptureMoves(ref generatedMoves, board, pawnSpot, enemyColor, findCaptureSpotOnRight, findCaptureSpotOnLeft, rowBeforePromotion);
            GenerateEnPassantMoves(ref generatedMoves, board, pawnSpot, findCaptureSpotOnLeft, findCaptureSpotOnRight);
        }

        private static void GeneratePawnForwardMoves(ref List<Move> generatedMoves, Board board, int pawnSpot, bool isWhite, int findSpotOneFront, int rowBeforePromotion, int startRow)
        {
            int spotInFrontOfPawn = pawnSpot + findSpotOneFront;
            int spotTwoInFrontOfPawn = spotInFrontOfPawn + findSpotOneFront;

            if (Piece.PieceType(board[spotInFrontOfPawn]) == Piece.None)
            {
                if (BoardInfo.GetRow(pawnSpot) == rowBeforePromotion)
                {
                    AddPawnPromotions(ref generatedMoves, pawnSpot, spotInFrontOfPawn);
                }
                else
                {
                    generatedMoves.Add(new Move(pawnSpot, spotInFrontOfPawn));
                }

                if (BoardInfo.GetRow(pawnSpot) == startRow && Piece.PieceType(board[spotTwoInFrontOfPawn]) == Piece.None)
                {
                    generatedMoves.Add(new Move(pawnSpot, spotTwoInFrontOfPawn, Move.Flag.PawnTwoForward));
                }
            }
        }

        private static void GeneratePawnCaptureMoves(ref List<Move> generatedMoves, Board board, int pawnSpot, int enemyColor, int findCaptureSpotOnRight, int findCaptureSpotOnLeft, int rowBeforePromotion)
        {
            int rightCaptureSpot = pawnSpot + findCaptureSpotOnRight;
            int leftCaptureSpot = pawnSpot + findCaptureSpotOnLeft;

            if (BoardInfo.GetFile(pawnSpot) < 7 && Piece.Color(board[rightCaptureSpot]) == enemyColor)
            {
                AddPawnCaptureMove(ref generatedMoves, board, pawnSpot, rightCaptureSpot, rowBeforePromotion);
            }

            if (BoardInfo.GetFile(pawnSpot) > 0 && Piece.Color(board[leftCaptureSpot]) == enemyColor)
            {
                AddPawnCaptureMove(ref generatedMoves, board, pawnSpot, leftCaptureSpot, rowBeforePromotion);
            }
        }

        private static void AddPawnCaptureMove(ref List<Move> generatedMoves, Board board, int pawnSpot, int captureSpot, int rowBeforePromotion)
        {
            if (BoardInfo.GetRow(pawnSpot) == rowBeforePromotion)
            {
                AddPawnPromotions(ref generatedMoves, pawnSpot, captureSpot, board[captureSpot]);
            }
            else
            {
                generatedMoves.Add(new Move(pawnSpot, captureSpot, board[captureSpot]));
            }
        }

        private static void GenerateEnPassantMoves(ref List<Move> generatedMoves, Board board, int pawnSpot, int leftCaptureSpot, int rightCaptureSpot)
        {
            if (BoardInfo.GetRow(pawnSpot) == 4)
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

        private static void AddPawnPromotions(ref List<Move> generatedMoves, int startSquare, int targetSquare, int capturedPiece = Piece.None)
        {
            generatedMoves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToKnight, capturedPiece));
            generatedMoves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToBishop, capturedPiece));
            generatedMoves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToRook, capturedPiece));
            generatedMoves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToQueen, capturedPiece));
        }

        /// <summary>
        /// Generate all the duck moves and add them into the provided list.
        /// </summary>
        /// <param name="generatedMoves">The list to add the generated moves into.</param>
        /// <param name="board">The board to use while checking for legal duck moves.</param>
        public static void GenerateDuckMoves(ref List<Move> generatedMoves, Board board)
        {
            if (board.isGameOver)
                return;

            int startSquare = board.Duck == Board.NOT_ON_BOARD ? 0 : board.Duck;
            int duckFlag = board.Duck == Board.NOT_ON_BOARD ? Move.Flag.FirstDuckMove : Move.Flag.None;

            for (int i = 0; i < board.Squares.Length; i++)
            {
                if (Piece.PieceType(board[i]) == Piece.None)
                {
                    generatedMoves.Add(new Move(startSquare, i, duckFlag));
                }
            }
        }
    }
}
