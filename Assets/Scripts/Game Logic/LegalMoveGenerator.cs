using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// Generate all the pawn moves for the color of the board
        /// and adds them into the provided list.
        /// </summary>
        /// <param name="generatedMoves">The list to add the generated moves into</param>
        /// <param name="board">The board to use while checking for legal moves</param>
        public static void GeneratePawnMoves(ref List<Move> generatedMoves, Board board)
        {
            if (board.isGameOver)
            {
                return;
            }
            int pawnSpot;
            bool isWhite = board.turnColor == Piece.White;
            int enemyColor = isWhite ? Piece.Black : Piece.White;
            int findSpotOneFront = isWhite ? 8 : -8;
            int rowBeforePromotion = isWhite ? 6 : 1;
            int startRow = isWhite ? 1 : 6;
            int findCaptureSpotOnRight = isWhite ? 9 : -7;
            int findCaptureSpotOnLeft = isWhite ? 7 : -9;
            int enPassantRow = isWhite ? 4 : 3;
            List<int> pawnLocations = GetLocationOfPieces(board, Piece.Pawn);
            for (int i = 0; i < pawnLocations.Count; i++)
            {
                pawnSpot = pawnLocations[i];
                GenerateOnePawnsMoves(
                    ref generatedMoves,
                    board,
                    pawnSpot,
                    isWhite,
                    enemyColor,
                    findSpotOneFront,
                    rowBeforePromotion,
                    startRow,
                    findCaptureSpotOnRight,
                    findCaptureSpotOnLeft,
                    enPassantRow
                );
            }

            string moves = "ALL MOVES\n";
            foreach (Move move in generatedMoves)
            {
                moves += "Piece: " + Piece.PieceStr(board[move.StartSquare]) + " | " + move.ToString() + "\n";
            }
           // Debug.Log( moves );
        }

        public static List<int> GetLocationOfPieces(Board board, int pieceType)
        {
            int piece = board.turnColor | pieceType;
            int pieceColor = Piece.Color(piece);
            List<int> pieceLocations = new List<int>();
            for (int i = 0; i < 64; i++)
            {
                int boardPieceType = Piece.PieceType(board[i]);
                int boardPieceColor = Piece.Color(board[i]);
                // Just in case the implementation of piece ever changes,
                // (ex. new flags) hard code it to check the piece type and color
                if (pieceType == boardPieceType && pieceColor == boardPieceColor)
                {
                    pieceLocations.Add(i);
                }
            }
            return pieceLocations;
        }

        public static int GetLocationOfDuck(Board board)
        {
            for (int i = 0; i < 64; i++)
            {
                if (Piece.PieceType(board[i]) == Piece.Duck)
                {
                    return i;
                }
            }
            Debug.Log("First Duck move");
            return Board.NOT_ON_BOARD;
        }

        private static void GenerateOnePawnsMoves(
            ref List<Move> generatedMoves,
            Board board,
            int pawnSpot,
            bool isWhite,
            int enemyColor,
            int findSpotOneFront,
            int rowBeforePromotion,
            int startRow,
            int findCaptureSpotOnRight,
            int findCaptureSpotOnLeft,
            int enPassantRow
        )
        {
            GeneratePawnForwardMoves(ref generatedMoves, board, pawnSpot, isWhite, findSpotOneFront, rowBeforePromotion, startRow);
            GeneratePawnCaptureMoves(ref generatedMoves, board, pawnSpot, enemyColor, findCaptureSpotOnRight, findCaptureSpotOnLeft, rowBeforePromotion);
            GenerateEnPassantMoves(ref generatedMoves, board, pawnSpot, findCaptureSpotOnLeft, findCaptureSpotOnRight, enPassantRow);

        }

        public static void GenerateOnePawnsMoves(ref List<Move> generatedMoves, Board board, int pawnSpot)
        {
            bool isWhite = board.turnColor == Piece.White;
            int enemyColor = isWhite ? Piece.Black : Piece.White;
            int findSpotOneFront = isWhite ? 8 : -8;
            int rowBeforePromotion = isWhite ? 6 : 1;
            int startRow = isWhite ? 1 : 6;
            int findCaptureSpotOnRight = isWhite ? 9 : -7;
            int findCaptureSpotOnLeft = isWhite ? 7 : -9;
            int enPassantRow = isWhite ? 4 : 3;
            GeneratePawnForwardMoves(ref generatedMoves, board, pawnSpot, isWhite, findSpotOneFront, rowBeforePromotion, startRow);
            GeneratePawnCaptureMoves(ref generatedMoves, board, pawnSpot, enemyColor, findCaptureSpotOnRight, findCaptureSpotOnLeft, rowBeforePromotion);
            GenerateEnPassantMoves(ref generatedMoves, board, pawnSpot, findCaptureSpotOnLeft, findCaptureSpotOnRight, enPassantRow);
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

            int pawnCol = BoardInfo.GetFile(pawnSpot);

            if (pawnCol < 7 && Piece.Color(board[rightCaptureSpot]) == enemyColor)
            {
                int enemyPiece = board[rightCaptureSpot];
                if (BoardInfo.GetRow(pawnSpot) == rowBeforePromotion)
                {
                    AddPawnPromotions(ref generatedMoves, pawnSpot, rightCaptureSpot, enemyPiece);
                }
                else
                {
                    generatedMoves.Add(new Move(pawnSpot, rightCaptureSpot, Move.Flag.None, enemyPiece));
                }
            }

            if (pawnCol > 0 && Piece.Color(board[leftCaptureSpot]) == enemyColor)
            {
                int enemyPiece = board[leftCaptureSpot];
                if (BoardInfo.GetRow(pawnSpot) == rowBeforePromotion)
                {
                    AddPawnPromotions(ref generatedMoves, pawnSpot, leftCaptureSpot, enemyPiece);
                }
                else
                {
                    generatedMoves.Add(new Move(pawnSpot, leftCaptureSpot, Move.Flag.None, enemyPiece));
                }
            }
        }

        private static void GenerateEnPassantMoves(ref List<Move> generatedMoves, Board board, int pawnSpot, int findLeftCaptureSpot, int findRightCaptureSpot, int enPassantRow)
        {
            //Debug.Log("Legal Moves Check for En Passant");
            if (BoardInfo.GetRow(pawnSpot) == enPassantRow)
            {
                int leftCaptureSpot = pawnSpot + findLeftCaptureSpot;
                int rightCaptureSpot = pawnSpot + findRightCaptureSpot;
                //Debug.Log("On En Passant Row");
                if (leftCaptureSpot == board.enPassantSquare || rightCaptureSpot == board.enPassantSquare)
                {
                    //Debug.Log("En Passant Square is there");
                    if (board[board.enPassantSquare] != Piece.Duck)
                    {
                        //Debug.Log("Generating En Passant");
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
        private static void AddPawnPromotions(ref List<Move> generatedMoves, int startSquare, int targetSquare, int enemyPiece)
        {
            generatedMoves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToKnight, enemyPiece));
            generatedMoves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToBishop, enemyPiece));
            generatedMoves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToRook, enemyPiece));
            generatedMoves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToQueen, enemyPiece));
        }
        public static void GenerateForOnePiece(ref List<Move> generatedMoves, Board board, int pieceSpot)
        {
            if (board.plyCount == 1)
            {
                GenerateDuckMoves(ref generatedMoves, board);
            }
            int piece = board[pieceSpot];
            switch (Piece.PieceType(piece))
            {
                case Piece.Pawn:
                    GenerateOnePawnsMoves(ref generatedMoves, board, pieceSpot);
                    break;
                case Piece.Knight:
                    GenerateOneKnightsMoves(ref generatedMoves, board, pieceSpot);
                    break;
                case Piece.Bishop:
                    GenerateOneBishopsMoves(ref generatedMoves, board, pieceSpot);
                    break;
                case Piece.Rook:
                    GenerateOneRooksMoves(ref generatedMoves, board, pieceSpot);
                    break;
                case Piece.Queen:
                    GenerateOneQueensMoves(ref generatedMoves, board, pieceSpot);
                    break;
                case Piece.King:
                    GenerateKingMoves(ref generatedMoves, board);
                    break;
                case Piece.Duck:
                    GenerateDuckMoves(ref generatedMoves, board);
                    break;
            }
        }

        /// <summary>
        /// Generate all the knight moves for the color of the board
        /// and adds them into the provided list.
        /// </summary>
        /// <param name="generatedMoves">The list to add the generated moves into</param>
        /// <param name="board">The board to use while checking for legal moves</param>
        public static void GenerateKnightMoves(ref List<Move> generatedMoves, Board board)
        {
            if (board.isGameOver)
            {
                return;
            }
            bool isWhite = board.isWhite;
            int enemyColor = isWhite ? Piece.Black : Piece.White;
            int[] knightOffsets = { 15, 17, -15, -17, 10, -10, 6, -6 };
            List<int> knightLocations = GetLocationOfPieces(board, Piece.Knight);
            for (int i = 0; i < knightLocations.Count; i++)
            {
                int knightSpot = knightLocations[i];
                GenerateOneKnightsMoves(ref generatedMoves, board, knightSpot, enemyColor, knightOffsets);
            }
        }

        private static bool isKnightInBoundsForThisJump(int knightSpot, int jumpOffset)
        {
            bool rowIsInBounds = true;
            bool fileIsInBounds = true;
            int knightRow = BoardInfo.GetRow(knightSpot);
            int knightCol = BoardInfo.GetFile(knightSpot);
            switch (jumpOffset)
            {
                // jumps that move 1 left
                case 15:
                case -17:
                    fileIsInBounds = knightCol > 0;
                    break;
                // jumps that move 2 left
                case 6:
                case -10:
                    fileIsInBounds = knightCol > 1;
                    break;
                // jumps that move 1 right
                case 17:
                case -15:
                    fileIsInBounds = knightCol < 7;
                    break;
                // jumps that move 2 right
                case 10:
                case -6:
                    fileIsInBounds = knightCol < 6;
                    break;
            }
            switch (jumpOffset)
            {
                // jumps that move 1 up
                case 6:
                case 10:
                    rowIsInBounds = knightRow < 7;
                    break;
                // jumps that move 2 up
                case 15:
                case 17:
                    rowIsInBounds = knightRow < 6;
                    break;
                // jumps that move 1 down
                case -6:
                case -10:
                    rowIsInBounds = knightRow > 0;
                    break;
                // jumps that move 2 down
                case -15:
                case -17:
                    rowIsInBounds = knightRow > 1;
                    break;
            }
            return rowIsInBounds && fileIsInBounds;
        }

        public static void GenerateOneKnightsMoves(ref List<Move> generatedMoves, Board board, int knightSpot)
        {
            bool isWhite = board.isWhite;
            int enemyColor = isWhite ? Piece.Black : Piece.White;
            int[] knightOffsets = { 15, 17, -15, -17, 10, -10, 6, -6 };
            for (int i = 0; i < knightOffsets.Length; i++)
            {
                int targetSpot = knightSpot + knightOffsets[i];

                if (isKnightInBoundsForThisJump(knightSpot, knightOffsets[i]) &&
                    (Piece.PieceType(board[targetSpot]) == Piece.None || Piece.Color(board[targetSpot]) == enemyColor))
                {
                    generatedMoves.Add(new Move(knightSpot, targetSpot));
                }
            }
        }

        public static void GenerateOneKnightsMoves(ref List<Move> generatedMoves, Board board, int knightSpot, int enemyColor, int[] knightOffsets)
        {
            for (int i = 0; i < knightOffsets.Length; i++)
            {
                int targetSpot = knightSpot + knightOffsets[i];

                if (isKnightInBoundsForThisJump(knightSpot, knightOffsets[i]) &&
                    (Piece.PieceType(board[targetSpot]) == Piece.None || Piece.Color(board[targetSpot]) == enemyColor))
                {
                    generatedMoves.Add(new Move(knightSpot, targetSpot));
                }
            }
        }

        /// <summary>
        /// Generate all the bishop moves for the color of the board
        /// and adds them into the provided list.
        /// </summary>
        /// <param name="generatedMoves">The list to add the generated moves into</param>
        /// <param name="board">The board to use while checking for legal moves</param>
        public static void GenerateBishopMoves(ref List<Move> generatedMoves, Board board)
        {
            if (board.isGameOver)
            {
                return;
            }
            bool isWhite = board.turnColor == Piece.White;
            List<int> bishopLocations = GetLocationOfPieces(board, Piece.Bishop);
            for (int i = 0; i < bishopLocations.Count; i++)
            {
                int bishopSpot = bishopLocations[i];
                GenerateOneBishopsMoves(ref generatedMoves, board, bishopSpot);
            }
        }

        public static void GenerateOneBishopsMoves(ref List<Move> generatedMoves, Board board, int bishopSpot)
        {
            bool isWhite = board.turnColor == Piece.White;
            int enemyColor = isWhite ? Piece.Black : Piece.White;
            GenerateDiagonalMoves(ref generatedMoves, board, bishopSpot, enemyColor);
        }

        private static void GenerateDiagonalMoves(ref List<Move> generatedMoves, Board board, int spotOfPieceMoving, int enemyColor)
        {
            if (board.isGameOver)
            {
                return;
            }
            // Up left
            int potentialTarget = spotOfPieceMoving;
            int rowOfPiece = BoardInfo.GetRow(spotOfPieceMoving);
            int colOfPiece = BoardInfo.GetFile(spotOfPieceMoving);
            for (int i = 0; i < 8; i++)
            {
                // If the piece itself is on one of these edges,
                // there are no moves in this direction.
                if (rowOfPiece == 7 || colOfPiece == 0)
                {
                    break;
                }

                potentialTarget += 7;

                int rowOfTarget = BoardInfo.GetRow(potentialTarget);
                int colOfTarget = BoardInfo.GetFile(potentialTarget);

                // If the target is out of bounds, stop
                if (potentialTarget > 63 || colOfTarget > colOfPiece)
                {
                    break;
                }

                int pieceAtTarget = board[potentialTarget];
                bool targetIsEmpty = Piece.PieceType(pieceAtTarget) == Piece.None;
                bool targetHasEnemy = Piece.Color(pieceAtTarget) == enemyColor;

                if (targetIsEmpty || targetHasEnemy)
                {
                    Move move = new Move(spotOfPieceMoving, potentialTarget);
                    generatedMoves.Add(move);
                }

                // If the target is on the edge or we've encountered a piece, stop
                if (rowOfTarget == 7 || colOfTarget == 0 || !targetIsEmpty)
                {
                    break;
                }
            }

            potentialTarget = spotOfPieceMoving;
            // Up right
            for (int i = 0; i < 8; i++)
            {
                // If the piece itself is on one of these edges,
                // there are no moves in this direction.
                if (rowOfPiece == 7 || colOfPiece == 7)
                {
                    break;
                }

                potentialTarget += 9;

                int rowOfTarget = BoardInfo.GetRow(potentialTarget);
                int colOfTarget = BoardInfo.GetFile(potentialTarget);

                // If the target is out of bounds, stop
                if (potentialTarget > 63 || colOfTarget < colOfPiece)
                {
                    break;
                }

                int pieceAtTarget = board[potentialTarget];
                bool targetIsEmpty = Piece.PieceType(pieceAtTarget) == Piece.None;
                bool targetHasEnemy = Piece.Color(pieceAtTarget) == enemyColor;

                if (targetIsEmpty || targetHasEnemy)
                {
                    Move move = new Move(spotOfPieceMoving, potentialTarget);
                    generatedMoves.Add(move);
                }

                // If the target is on the edge or we've encountered a piece, stop
                if (rowOfTarget == 7 || colOfTarget == 7 || !targetIsEmpty)
                {
                    break;
                }
            }

            potentialTarget = spotOfPieceMoving;
            // Down left
            for (int i = 0; i < 8; i++)
            {
                // If the piece itself is on one of these edges,
                // there are no moves in this direction.
                if (rowOfPiece == 0 || colOfPiece == 0)
                {
                    break;
                }

                potentialTarget -= 9;

                int rowOfTarget = BoardInfo.GetRow(potentialTarget);
                int colOfTarget = BoardInfo.GetFile(potentialTarget);

                // If the target is out of bounds, stop
                if (potentialTarget < 0 || colOfTarget > colOfPiece)
                {
                    break;
                }

                int pieceAtTarget = board[potentialTarget];
                bool targetIsEmpty = Piece.PieceType(pieceAtTarget) == Piece.None;
                bool targetHasEnemy = Piece.Color(pieceAtTarget) == enemyColor;

                if (targetIsEmpty || targetHasEnemy)
                {
                    Move move = new Move(spotOfPieceMoving, potentialTarget);
                    generatedMoves.Add(move);
                }

                // If the target is on the edge or we've encountered a piece, stop
                if (rowOfTarget == 0 || colOfTarget == 0 || !targetIsEmpty)
                {
                    break;
                }
            }

            potentialTarget = spotOfPieceMoving;
            // Down right
            for (int i = 0; i < 8; i++)
            {
                // If the piece itself is on one of these edges,
                // there are no moves in this direction.
                if (rowOfPiece == 0 || colOfPiece == 7)
                {
                    break;
                }

                potentialTarget -= 7;

                int rowOfTarget = BoardInfo.GetRow(potentialTarget);
                int colOfTarget = BoardInfo.GetFile(potentialTarget);

                // If the target is out of bounds, stop
                if (potentialTarget < 0 || colOfTarget < colOfPiece)
                {
                    break;
                }

                int pieceAtTarget = board[potentialTarget];
                bool targetIsEmpty = Piece.PieceType(pieceAtTarget) == Piece.None;
                bool targetHasEnemy = Piece.Color(pieceAtTarget) == enemyColor;

                if (targetIsEmpty || targetHasEnemy)
                {
                    Move move = new Move(spotOfPieceMoving, potentialTarget);
                    generatedMoves.Add(move);
                }

                // If the target is on the edge or we've encountered a piece, stop
                if (rowOfTarget == 7 || colOfTarget == 0 || !targetIsEmpty)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Generate all the rook moves for the color of the board
        /// and adds them into the provided list.
        /// </summary>
        /// <param name="generatedMoves">The list to add the generated moves into</param>
        /// <param name="board">The board to use while checking for legal moves</param>
        public static void GenerateRookMoves(ref List<Move> generatedMoves, Board board)
        {
            if (board.isGameOver)
            {
                return;
            }
            bool isWhite = board.turnColor == Piece.White;
            List<int> rookLocations = GetLocationOfPieces(board, Piece.Rook);
            for (int i = 0; i < rookLocations.Count; i++)
            {
                int rookSpot = rookLocations[i];
                GenerateOneRooksMoves(ref generatedMoves, board, rookSpot);
            }
        }

        public static void GenerateOneRooksMoves(ref List<Move> generatedMoves, Board board, int rookSpot)
        {
            bool isWhite = board.turnColor == Piece.White;
            int enemyColor = isWhite ? Piece.Black : Piece.White;
            GenerateOrthogonalMoves(ref generatedMoves, board, rookSpot, enemyColor);
        }

        private static void GenerateOrthogonalMoves(ref List<Move> generatedMoves, Board board, int spotOfPieceMoving, int enemyColor)
        {
            if (board.isGameOver)
            {
                return;
            }
            int potentialTarget = spotOfPieceMoving;
            int rowOfPiece = BoardInfo.GetRow(spotOfPieceMoving);
            int colOfPiece = BoardInfo.GetFile(spotOfPieceMoving);

            // Up
            for (int i = 0; i < 8; i++)
            {
                // If the piece itself is on this edge,
                // there are no moves in this direction.
                if (rowOfPiece == 7)
                {
                    break;
                }

                potentialTarget += 8;

                // If the target is out of bounds, stop
                if (potentialTarget > 63)
                {
                    break;
                }

                int rowOfTarget = BoardInfo.GetRow(potentialTarget);
                int pieceAtTarget = board[potentialTarget];
                bool targetIsEmpty = Piece.PieceType(pieceAtTarget) == Piece.None;
                bool targetHasEnemy = Piece.Color(pieceAtTarget) == enemyColor;

                if (targetIsEmpty || targetHasEnemy)
                {
                    Move move = new Move(spotOfPieceMoving, potentialTarget);
                    generatedMoves.Add(move);
                }

                // If the target is on the edge or we've encountered a piece, stop
                if (rowOfTarget == 7 || !targetIsEmpty)
                {
                    break;
                }
            }

            potentialTarget = spotOfPieceMoving;
            // Down
            for (int i = 0; i < 8; i++)
            {
                // If the piece itself is on this edge,
                // there are no moves in this direction.
                if (rowOfPiece == 0)
                {
                    break;
                }

                potentialTarget -= 8;

                // If the target is out of bounds, stop
                if (potentialTarget < 0)
                {
                    break;
                }

                int rowOfTarget = BoardInfo.GetRow(potentialTarget);
                int pieceAtTarget = board[potentialTarget];
                bool targetIsEmpty = Piece.PieceType(pieceAtTarget) == Piece.None;
                bool targetHasEnemy = Piece.Color(pieceAtTarget) == enemyColor;

                if (targetIsEmpty || targetHasEnemy)
                {
                    Move move = new Move(spotOfPieceMoving, potentialTarget);
                    generatedMoves.Add(move);
                }

                // If the target is on the edge or we've encountered a piece, stop
                if (rowOfTarget == 0 || !targetIsEmpty)
                {
                    break;
                }
            }

            potentialTarget = spotOfPieceMoving;
            // Left
            for (int i = 0; i < 8; i++)
            {
                // If the piece itself is on this edge,
                // there are no moves in this direction.
                if (colOfPiece == 0)
                {
                    break;
                }

                potentialTarget -= 1;

                int colOfTarget = BoardInfo.GetFile(potentialTarget);

                // If the target is out of bounds, stop
                if (colOfTarget > colOfPiece)
                {
                    break;
                }

                int pieceAtTarget = board[potentialTarget];
                bool targetIsEmpty = Piece.PieceType(pieceAtTarget) == Piece.None;
                bool targetHasEnemy = Piece.Color(pieceAtTarget) == enemyColor;

                if (targetIsEmpty || targetHasEnemy)
                {
                    Move move = new Move(spotOfPieceMoving, potentialTarget);
                    generatedMoves.Add(move);
                }

                // If the target is on the edge or we've encountered a piece, stop
                if (colOfTarget == 0 || !targetIsEmpty)
                {
                    break;
                }
            }

            potentialTarget = spotOfPieceMoving;
            // Right
            for (int i = 0; i < 8; i++)
            {
                // If the piece itself is on this edge,
                // there are no moves in this direction.
                if (colOfPiece == 7)
                {
                    break;
                }

                potentialTarget += 1;

                int colOfTarget = BoardInfo.GetFile(potentialTarget);

                // If the target is out of bounds, stop
                if (colOfTarget < colOfPiece)
                {
                    break;
                }

                int pieceAtTarget = board[potentialTarget];
                bool targetIsEmpty = Piece.PieceType(pieceAtTarget) == Piece.None;
                bool targetHasEnemy = Piece.Color(pieceAtTarget) == enemyColor;

                if (targetIsEmpty || targetHasEnemy)
                {
                    Move move = new Move(spotOfPieceMoving, potentialTarget);
                    generatedMoves.Add(move);
                }

                // If the target is on the edge or we've encountered a piece, stop
                if (colOfTarget == 7 || !targetIsEmpty)
                {
                    break;
                }
            }
        }


        /// <summary>
        /// Generate all the queen moves for the color of the board
        /// and adds them into the provided list.
        /// </summary>
        /// <param name="generatedMoves">The list to add the generated moves into</param>
        /// <param name="board">The board to use while checking for legal moves</param>
        public static void GenerateQueenMoves(ref List<Move> generatedMoves, Board board)
        {
            if (board.isGameOver)
            {
                return;
            }
            bool isWhite = board.turnColor == Piece.White;
            List<int> queenLocations = GetLocationOfPieces(board, Piece.Queen);
            int enemyColor = isWhite ? Piece.Black : Piece.White;
            for (int i = 0; i < queenLocations.Count; i++)
            {
                int queenSpot = queenLocations[i];
                GenerateOneQueensMoves(ref generatedMoves, board, queenSpot);
            }
        }

        public static void GenerateOneQueensMoves(ref List<Move> generatedMoves, Board board, int queenSpot)
        {
            bool isWhite = board.turnColor == Piece.White;
            int enemyColor = isWhite ? Piece.Black : Piece.White;
            GenerateDiagonalMoves(ref generatedMoves, board, queenSpot, enemyColor);
            GenerateOrthogonalMoves(ref generatedMoves, board, queenSpot, enemyColor);
        }

        /// <summary>
        /// Generate all the king moves for the color of the board
        /// and adds them into the provided list.
        /// </summary>
        /// <param name="generatedMoves">The list to add the generated moves into</param>
        /// <param name="board">The board to use while checking for legal moves</param>
        public static void GenerateKingMoves(ref List<Move> generatedMoves, Board board)
        {
            if (board.isGameOver)
            {
                return;
            }
            bool isWhite = board.turnColor == Piece.White;
            List<int> kingSpots = GetLocationOfPieces(board, Piece.King);
            int kingSpot = kingSpots.Count == 1 ? kingSpots[0] : -1;
            if (kingSpot < 0)
            {
                board.isGameOver = true;
                return;
            }
            int enemyColor = isWhite ? Piece.Black : Piece.White;
            bool isOnTopEdge = BoardInfo.GetRow(kingSpot) == 7;
            bool isOnBottomEdge = BoardInfo.GetRow(kingSpot) == 0;
            bool isOnLeftEdge = BoardInfo.GetFile(kingSpot) == 0;
            bool isOnRightEdge = BoardInfo.GetFile(kingSpot) == 7;
            int potentialTarget = kingSpot + 7;
            if (
                !(isOnTopEdge || isOnLeftEdge) &&
                (Piece.PieceType(board[potentialTarget]) == Piece.None ||
                Piece.Color(board[potentialTarget]) == enemyColor)
            )
            {
                Move move = new Move(kingSpot, potentialTarget);
                generatedMoves.Add(move);
            }
            potentialTarget = kingSpot + 8;
            if (
                !isOnTopEdge &&
                (Piece.PieceType(board[potentialTarget]) == Piece.None ||
                Piece.Color(board[potentialTarget]) == enemyColor)
            )
            {
                Move move = new Move(kingSpot, potentialTarget);
                generatedMoves.Add(move);
            }
            potentialTarget = kingSpot + 9;
            if (
                !(isOnTopEdge || isOnRightEdge) &&
                (Piece.PieceType(board[potentialTarget]) == Piece.None ||
                Piece.Color(board[potentialTarget]) == enemyColor)
            )
            {
                Move move = new Move(kingSpot, potentialTarget);
                generatedMoves.Add(move);
            }
            potentialTarget = kingSpot + 1;
            if (
                !isOnRightEdge &&
                (Piece.PieceType(board[potentialTarget]) == Piece.None ||
                Piece.Color(board[potentialTarget]) == enemyColor)
            )
            {
                Move move = new Move(kingSpot, potentialTarget);
                generatedMoves.Add(move);
            }
            potentialTarget = kingSpot - 7;
            if (
                !(isOnBottomEdge || isOnRightEdge) &&
                (Piece.PieceType(board[potentialTarget]) == Piece.None ||
                Piece.Color(board[potentialTarget]) == enemyColor)
            )
            {
                Move move = new Move(kingSpot, potentialTarget);
                generatedMoves.Add(move);
            }
            potentialTarget = kingSpot - 8;
            if (
                !isOnBottomEdge &&
                (Piece.PieceType(board[potentialTarget]) == Piece.None ||
                Piece.Color(board[potentialTarget]) == enemyColor)
            )
            {
                Move move = new Move(kingSpot, potentialTarget);
                generatedMoves.Add(move);
            }
            potentialTarget = kingSpot - 9;
            if (
                !(isOnBottomEdge || isOnLeftEdge) &&
                (Piece.PieceType(board[potentialTarget]) == Piece.None ||
                Piece.Color(board[potentialTarget]) == enemyColor)
            )
            {
                Move move = new Move(kingSpot, potentialTarget);
                generatedMoves.Add(move);
            }
            potentialTarget = kingSpot - 1;
            if (
                !isOnLeftEdge &&
                (Piece.PieceType(board[potentialTarget]) == Piece.None ||
                Piece.Color(board[potentialTarget]) == enemyColor)
            )
            {
                Move move = new Move(kingSpot, potentialTarget);
                generatedMoves.Add(move);
            }
            // Remember to add castling
            bool kingSideCastle = isWhite ? board.CastleKingSideW : board.CastleKingSideB;
            bool queenSideCastle = isWhite ? board.CastleQueenSideW : board.CastleQueenSideB;
            if (kingSideCastle)
            {
                if (
                    Piece.PieceType(board[kingSpot + 1]) == Piece.None &&
                    Piece.PieceType(board[kingSpot + 2]) == Piece.None
                )
                {
                    if (Piece.PieceType(board[kingSpot + 3]) == Piece.Rook)
                    {
                        Move move = new Move(kingSpot, kingSpot + 2, Move.Flag.Castling);
                        generatedMoves.Add(move);
                    }
                    else
                    {
                        // The rook was captured, so castling isn't actually valid
                        if (isWhite)
                        {
                            board.CastleKingSideW = false;
                        }
                        else
                        {
                            board.CastleKingSideB = false;
                        }
                    }
                }
            }
            if (queenSideCastle)
            {
                if (
                    Piece.PieceType(board[kingSpot - 1]) == Piece.None &&
                    Piece.PieceType(board[kingSpot - 2]) == Piece.None &&
                    Piece.PieceType(board[kingSpot - 3]) == Piece.None
                )
                {
                    if (Piece.PieceType(board[kingSpot - 4]) == Piece.Rook)
                    {
                        Move move = new Move(kingSpot, kingSpot - 2, Move.Flag.Castling);
                        generatedMoves.Add(move);
                    }
                    else
                    {
                        // The rook was captured, so castling isn't actually valid
                        if (isWhite)
                        {
                            board.CastleQueenSideW = false;
                        }
                        else
                        {
                            board.CastleQueenSideB = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generate all the duck moves
        /// and adds them into the provided list.
        /// </summary>
        /// <param name="generatedMoves">The list to add the generated moves into</param>
        /// <param name="board">The board to use while checking for legal moves</param>
        public static void GenerateDuckMoves(ref List<Move> generatedMoves, Board board)
        {
            // The starting square for the duck (its current position).
            int startSquare = GetLocationOfDuck(board);

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

                    //if (duckMove.StartSquare == 0)
                    //{
                    //    Debug.Log(duckMove.ToString());
                    //}

                    // Add the move to the list of generated moves.
                    generatedMoves.Add(duckMove);
                }
            }
        }
    }
}