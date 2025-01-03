using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DuckChess
{
    /// <summary>
    /// Represents a board state in Duck Chess.
    /// </summary>
    public class Board
    {
        public const int NOT_ON_BOARD = -2; // 63 is max

        public int[] Squares;

        public int turnColor;
        public bool isWhite { get { return turnColor == Piece.White; } }
        public bool turnIsDuck;

        public int enPassantSquare;

        public bool CastleKingSideW, CastleQueenSideW;
        public bool CastleKingSideB, CastleQueenSideB;

        public int winnerColor;
        public bool isGameOver;

        public int plyCount;
        public int numPlySinceLastEvent;

        public List<Move> legalMoves;

        public PieceList WhitePawnLocations;
        public PieceList BlackPawnLocations;
        public PieceList WhiteKnightLocations;
        public PieceList BlackKnightLocations;
        public PieceList WhiteBishopLocations;
        public PieceList BlackBishopLocations;
        public PieceList WhiteRookLocations;
        public PieceList BlackRookLocations;
        public PieceList WhiteQueenLocations;
        public PieceList BlackQueenLocations;
        public PieceList WhiteKingLocation;
        public PieceList BlackKingLocation;
        public PieceList DuckLocation;
        /// <summary>
        /// An array of arrays. The first index is for which piece list
        /// (do not rely on the order being constant), The second index is for which
        /// piece of that type. Given both of these indexes, the location of the
        /// piece is returned.
        /// </summary>
        /// <returns></returns>
        public PieceList[] AllPieceLocations;

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
        /// By default, copies squares and turn information.
        /// <br></br>
        /// This is enough to calculate the next move correctly.
        /// <br></br>
        /// If you want to copy everything, you must specify.
        /// <br></br>
        /// </summary>
        /// <param name="otherBoard"></param>
        public Board(Board otherBoard)
        {
            Squares = new int[64];
            Array.Copy(otherBoard.Squares, Squares, 64);
            turnColor = otherBoard.turnColor;
            turnIsDuck = otherBoard.turnIsDuck;
            enPassantSquare = otherBoard.enPassantSquare;
            CastleKingSideW = otherBoard.CastleKingSideW;
            CastleQueenSideW = otherBoard.CastleQueenSideW;
            CastleKingSideB = otherBoard.CastleKingSideB;
            CastleQueenSideB = otherBoard.CastleQueenSideB;
            winnerColor = otherBoard.winnerColor;
            isGameOver = otherBoard.isGameOver;
            plyCount = otherBoard.plyCount;
            numPlySinceLastEvent = otherBoard.numPlySinceLastEvent;
            legalMoves = new List<Move>(otherBoard.legalMoves);
            WhitePawnLocations = new PieceList(otherBoard.WhitePawnLocations);
            BlackPawnLocations = new PieceList(otherBoard.BlackPawnLocations);
            WhiteKnightLocations = new PieceList(otherBoard.WhiteKnightLocations);
            BlackKnightLocations = new PieceList(otherBoard.BlackKnightLocations);
            WhiteBishopLocations = new PieceList(otherBoard.WhiteBishopLocations);
            BlackBishopLocations = new PieceList(otherBoard.BlackBishopLocations);
            WhiteRookLocations = new PieceList(otherBoard.WhiteRookLocations);
            BlackRookLocations = new PieceList(otherBoard.BlackRookLocations);
            WhiteQueenLocations = new PieceList(otherBoard.WhiteQueenLocations);
            BlackQueenLocations = new PieceList(otherBoard.BlackQueenLocations);
            WhiteKingLocation = new PieceList(otherBoard.WhiteKingLocation);
            BlackKingLocation = new PieceList(otherBoard.BlackKingLocation);
            DuckLocation = new PieceList(otherBoard.DuckLocation);
            AllPieceLocations = new PieceList[] {
                WhitePawnLocations, BlackPawnLocations,
                WhiteKnightLocations, BlackKnightLocations,
                WhiteBishopLocations, BlackBishopLocations,
                WhiteRookLocations, BlackRookLocations,
                WhiteQueenLocations, BlackQueenLocations,
                WhiteKingLocation, BlackKingLocation,
                DuckLocation
            };
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
            enPassantSquare = -1;
            ReinitializePieceLists();
        }

        private void ReinitializePieceLists()
        {

            WhitePawnLocations = new PieceList(NOT_ON_BOARD, 8, Piece.White | Piece.Pawn);
            BlackPawnLocations = new PieceList(NOT_ON_BOARD, 8, Piece.Black | Piece.Pawn);
            WhiteKnightLocations = new PieceList(NOT_ON_BOARD, 10, Piece.White | Piece.Knight);
            BlackKnightLocations = new PieceList(NOT_ON_BOARD, 10, Piece.Black | Piece.Knight);
            WhiteBishopLocations = new PieceList(NOT_ON_BOARD, 10, Piece.White | Piece.Bishop);
            BlackBishopLocations = new PieceList(NOT_ON_BOARD, 10, Piece.Black | Piece.Bishop);
            WhiteRookLocations = new PieceList(NOT_ON_BOARD, 10, Piece.White | Piece.Rook);
            BlackRookLocations = new PieceList(NOT_ON_BOARD, 10, Piece.Black | Piece.Rook);
            WhiteQueenLocations = new PieceList(NOT_ON_BOARD, 9, Piece.White | Piece.Queen);
            BlackQueenLocations = new PieceList(NOT_ON_BOARD, 9, Piece.Black | Piece.Queen);
            WhiteKingLocation = new PieceList(NOT_ON_BOARD, 1, Piece.White | Piece.King);
            BlackKingLocation = new PieceList(NOT_ON_BOARD, 1, Piece.Black | Piece.King);
            DuckLocation = new PieceList(NOT_ON_BOARD, 1, Piece.Duck);
            AllPieceLocations = new PieceList[] {
                WhitePawnLocations, BlackPawnLocations,
                WhiteKnightLocations, BlackKnightLocations,
                WhiteBishopLocations, BlackBishopLocations,
                WhiteRookLocations, BlackRookLocations,
                WhiteQueenLocations, BlackQueenLocations,
                WhiteKingLocation, BlackKingLocation,
                DuckLocation
            };
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
            turnIsDuck = false;
            SaveLocationOfPieces();
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

        private void UpdateBoard(ref Move move)
        {
            /*
             * DONE
             * - place the piece on the target square
             * -- if it is the first duck move, replace the piece to place with Piece.Duck
             * - if it is a normal capture, remove the piece on the target square
             * - if it is not the first duck move, remove the piece at the start square
             * - if it is a pawn 2 forward move, set the en passant square behind the pawn
             * - if it is en passant, remove the pawn behind the target square
             * - if it is a promotion, replace the piece at the target square with the promoted piece
             * TO DO (NOW):
             * - 
             * TO DO (LATER):
             * - castling
             * -- the king goes from start to target (alr handled)
             * -- the rook goes from initial pos to castled spot
             * -- if the king moves, disable both castles
             * -- if the rook moves, disable the right castling based on the exact row and col
             * -- if the move is a capture, check if it is an enemy rook
             * --- if it is a rook disable the right castling based on exact row and col
             * - if it is a capture or a pawn move, reset the counter for a draw
             * - idk
             */
            if (Squares[move.TargetSquare] != Piece.None) {
                move = new Move(move, Squares[move.TargetSquare]);
            }
            // Check if this is a capture, and if so, check if it is a rook or a king.
            if (move.IsCapture)
            {
                int enemyPiece = Squares[move.TargetSquare];
                // If it is a rook, disable castling
                if (Piece.PieceType(enemyPiece) == Piece.Rook)
                {
                    DisableCastlingOnRookSide(move.TargetSquare, !isWhite);
                }
                else if (Piece.PieceType(enemyPiece) == Piece.King)
                {
                    isGameOver = true;
                    winnerColor = turnColor;
                }
            }
            // Check if this is a rook move, and if so, disable castling if first rook move
            if (Piece.PieceType(Squares[move.StartSquare]) == Piece.Rook)
            {
                DisableCastlingOnRookSide(move.StartSquare, isWhite);
            }

            #region place the piece on the target square
            int pieceToMove = Squares[move.StartSquare];

            #region if first duck move, piece to move = duck
            if (move.MoveFlag == Move.Flag.FirstDuckMove)
            {
                pieceToMove = Piece.Duck;
            }
            #endregion
            // Also deletes the piece we are capturing, if any
            Squares[move.TargetSquare] = pieceToMove;
            #endregion

            if (move.MoveFlag != Move.Flag.FirstDuckMove)
            {
                Squares[move.StartSquare] = Piece.None;
            }

            // If it is a king move, disable castling on both sides
            if (Piece.PieceType(pieceToMove) == Piece.King)
            {
                if (isWhite)
                {
                    CastleKingSideW = false;
                    CastleQueenSideW = false;
                } else
                {
                    CastleKingSideB = false;
                    CastleQueenSideB = false;
                }
            }

            if (move.MoveFlag == Move.Flag.Castling)
            {
                bool isKingSide = move.TargetSquare > move.StartSquare;
                int rookSpot = move.TargetSquare + (isKingSide ? 1 : -2);
                int newRookSpot = move.TargetSquare + (isKingSide ? -1 : 1);
                Squares[newRookSpot] = Squares[rookSpot];
                Squares[rookSpot] = Piece.None;
            }

            if (move.MoveFlag == Move.Flag.PawnTwoForward)
            {
                enPassantSquare = move.TargetSquare + (isWhite ? -8 : 8);
            }
            else if (!turnIsDuck)
            {
                enPassantSquare = NOT_ON_BOARD;
            }

            if (move.MoveFlag == Move.Flag.EnPassantCapture)
            {
                int enemySquare = move.TargetSquare + (isWhite ? -8 : 8);
                Squares[enemySquare] = Piece.None;
            }

            if (move.IsPromotion)
            {
                Squares[move.TargetSquare] = turnColor | move.PromotionPieceType;
            }

            if (Piece.PieceType(pieceToMove) == Piece.Pawn || move.IsCapture)
            {
                // Reset the timer
                numPlySinceLastEvent = 0;
            } else
            {
                // Add to the timer
                numPlySinceLastEvent++;
            }
        }

        private void DisableCastlingOnRookSide(int rookSpot, bool isWhiteRook)
        {
            int rook = Squares[rookSpot];
            // Is on start square
            bool isOnStartSquare = false;
            int row = BoardInfo.GetRow(rook);
            int file = BoardInfo.GetFile(rook);
            int startRow = isWhiteRook ? 0 : 7;
            int kingSideCol = 7;
            int queenSideCol = 0;
            isOnStartSquare = (row == startRow) && (file == kingSideCol || file == queenSideCol);
            if (isOnStartSquare)
            {
                bool isKingSideRook = file == kingSideCol;
                if (isKingSideRook)
                {
                    if (isWhiteRook)
                    {
                        CastleKingSideW = false;
                    } else
                    {
                        CastleKingSideB = false;
                    }
                } else
                {
                    if (isWhiteRook)
                    {
                        CastleQueenSideW = false;
                    }
                    else
                    {
                        CastleQueenSideB = false;
                    }
                }
            }
        }

        private void SwitchTurnForward()
        {
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
            if (numPlySinceLastEvent >= 200)
            {
                // 50 move draw
                isGameOver = true;
                winnerColor = Piece.NoColor;
            }
        }

        /// <summary>
        /// Makes a move on the board and updates its state.
        /// </summary>
        public void MakeMove(ref Move move)
        {
            plyCount++;

            ReinitializePieceLists();
            SaveLocationOfPieces();

            UpdateBoard(ref move);

            SwitchTurnForward();
        }

        private void GenerateNormalMoves()
        {
            legalMoves = new List<Move>();
            LegalMoveGenerator.GeneratePawnMoves(ref legalMoves, this);
            LegalMoveGenerator.GenerateKnightMoves(ref legalMoves, this);
            LegalMoveGenerator.GenerateBishopMoves(ref legalMoves, this);
            LegalMoveGenerator.GenerateRookMoves(ref legalMoves, this);
            LegalMoveGenerator.GenerateKingMoves(ref legalMoves, this);
            LegalMoveGenerator.GenerateQueenMoves(ref legalMoves, this);
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
        private void SaveLocationOfPieces()
        {
            PieceList listToUpdate = null;
            for (int i = 0; i < 64; i++)
            {
                int boardPieceType = Piece.PieceType(Squares[i]);
                bool boardPieceIsWhite = Piece.IsColor(Squares[i], Piece.White);
                listToUpdate = null;
                switch (boardPieceType)
                {
                    case Piece.Pawn:
                        listToUpdate = boardPieceIsWhite ? WhitePawnLocations : BlackPawnLocations;
                        break;
                    case Piece.Knight:
                        listToUpdate = boardPieceIsWhite ? WhiteKnightLocations : BlackKnightLocations;
                        break;
                    case Piece.Bishop:
                        listToUpdate = boardPieceIsWhite ? WhiteBishopLocations : BlackBishopLocations;
                        break;
                    case Piece.Rook:
                        listToUpdate = boardPieceIsWhite ? WhiteRookLocations : BlackRookLocations;
                        break;
                    case Piece.Queen:
                        listToUpdate = boardPieceIsWhite ? WhiteQueenLocations : BlackQueenLocations;
                        break;
                    case Piece.King:
                        listToUpdate = boardPieceIsWhite ? WhiteKingLocation : BlackKingLocation;
                        break;
                    case Piece.Duck:
                        listToUpdate = DuckLocation;
                        break;
                }
                if (listToUpdate != null)
                {
                    listToUpdate.AddToSquare(i);
                }
            }
            AllPieceLocations = new PieceList[] {
                WhitePawnLocations, BlackPawnLocations,
                WhiteKnightLocations, BlackKnightLocations,
                WhiteBishopLocations, BlackBishopLocations,
                WhiteRookLocations, BlackRookLocations,
                WhiteQueenLocations, BlackQueenLocations,
                WhiteKingLocation, BlackKingLocation,
                DuckLocation
            };
        }
        public PieceList GetLocationOfPieces(int pieceType)
        {
            return pieceType switch
            {
                Piece.Pawn => isWhite ? WhitePawnLocations : BlackPawnLocations,
                Piece.Knight => isWhite ? WhiteKnightLocations : BlackKnightLocations,
                Piece.Bishop => isWhite ? WhiteBishopLocations : BlackBishopLocations,
                Piece.Rook => isWhite ? WhiteRookLocations : BlackRookLocations,
                Piece.Queen => isWhite ? WhiteQueenLocations : BlackQueenLocations,
                Piece.King => isWhite ? WhiteKingLocation : BlackKingLocation,
                Piece.Duck => DuckLocation,
                _ => null
            };
        }
        public PieceList GetLocationOfPieces(int pieceType, int pieceColor)
        {
            bool isWhite = pieceColor == Piece.White;
            return pieceType switch
            {
                Piece.Pawn => isWhite ? WhitePawnLocations : BlackPawnLocations,
                Piece.Knight => isWhite ? WhiteKnightLocations : BlackKnightLocations,
                Piece.Bishop => isWhite ? WhiteBishopLocations : BlackBishopLocations,
                Piece.Rook => isWhite ? WhiteRookLocations : BlackRookLocations,
                Piece.Queen => isWhite ? WhiteQueenLocations : BlackQueenLocations,
                Piece.King => isWhite ? WhiteKingLocation : BlackKingLocation,
                Piece.Duck => DuckLocation,
                _ => null
            };
        }
        public class PieceList
        {
            public int[] locations;
            public int Length;
            public int maxLength;
            public int piece;
            internal PieceList(int defaultValue, int maxLength, int piece)
            {
                locations = Enumerable.Repeat(NOT_ON_BOARD, maxLength).ToArray();
                this.maxLength = maxLength;
                Length = 0;
                this.piece = piece;
            }
            internal PieceList(PieceList otherList)
            {
                locations = new int[otherList.maxLength];
                Array.Copy(otherList.locations, locations, otherList.maxLength);
                Length = otherList.Length;
                maxLength = otherList.maxLength;
                piece = otherList.piece;
            }
            internal void AddToSquare(int square)
            {
                locations[Length] = square;
                Length++;
            }

            public int this[int index] => locations[index];
        }
    }
}
