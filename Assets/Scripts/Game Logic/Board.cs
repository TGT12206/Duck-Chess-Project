using System;
using System.Collections.Generic;
using UnityEngine;

namespace DuckChess
{
    /// <summary>
    /// Represents a board state in Duck Chess.
    /// </summary>
    public class Board
    {
        public int[] Squares;

        public const int NOT_ON_BOARD = -2;

        #region Turn Information
        public int turnColor;
        public bool duckTurn;
        #endregion

        public int enPassantSquare;

        #region Castling Info
        public bool CastleKingSideW, CastleQueenSideW;
        public bool CastleKingSideB, CastleQueenSideB;

        private int PlyWhereLostKingSideCastleW, PlyWhereLostQueenSideCastleW;
        private int PlyWhereLostKingSideCastleB, PlyWhereLostQueenSideCastleB;
        #endregion

        #region Game End Info
        public int winnerColor;
        public bool isGameOver;

        public int plyCount;
        public int numPlySinceLastEvent;
        #endregion

        #region Lists of Legal Moves
        public List<Move> legalMoves;
        #endregion

        #region Piece Locations
        public PieceList WhitePawns, BlackPawns, WhiteKnights, BlackKnights;
        public PieceList WhiteBishops, BlackBishops, WhiteRooks, BlackRooks;
        public PieceList WhiteQueens, BlackQueens;

        public int WhiteKing, BlackKing;
        public int Duck;
        #endregion

        #region Constants for Maximum Pieces
        public const int MAX_PAWN_COUNT = 8;
        public const int MAX_KNIGHT_COUNT = 10;
        public const int MAX_BISHOP_COUNT = 10;
        public const int MAX_ROOK_COUNT = 10;
        public const int MAX_QUEEN_COUNT = 9;
        #endregion

        public int this[int index]
        {
            get => Squares[index];
            set => Squares[index] = value;
        }

        #region Piece Locations
        // Information about (mostly location and number) each piece type
        /// <summary>
        /// A Piecelist that contains the location of every piece on the board.
        /// </summary>
        public PieceList AllPieces
        {
            get
            {
                PieceList allPieces = new PieceList(64);
                allPieces.MergeWithPieceList(WhitePawns);
                allPieces.MergeWithPieceList(BlackPawns);
                allPieces.MergeWithPieceList(WhiteKnights);
                allPieces.MergeWithPieceList(BlackKnights);
                allPieces.MergeWithPieceList(WhiteBishops);
                allPieces.MergeWithPieceList(BlackBishops);
                allPieces.MergeWithPieceList(WhiteRooks);
                allPieces.MergeWithPieceList(BlackRooks);
                allPieces.MergeWithPieceList(WhiteQueens);
                allPieces.MergeWithPieceList(BlackQueens);
                allPieces.AddPieceAtSquare(WhiteKing);
                allPieces.AddPieceAtSquare(BlackKing);
                if (Duck != NOT_ON_BOARD)
                {
                    allPieces.AddPieceAtSquare(Duck);
                }
                return allPieces;
            }
        }
        #endregion

        public Board()
        {
            Squares = new int[64];
            ResetBoardState();
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

        public Board Clone()
        {
            // Create a new board instance
            Board copy = new Board
            {
                Squares = (int[])Squares.Clone(),
                turnColor = turnColor,
                duckTurn = duckTurn,
                enPassantSquare = enPassantSquare,
                CastleKingSideW = CastleKingSideW,
                CastleQueenSideW = CastleQueenSideW,
                CastleKingSideB = CastleKingSideB,
                CastleQueenSideB = CastleQueenSideB,
                PlyWhereLostKingSideCastleW = PlyWhereLostKingSideCastleW,
                PlyWhereLostQueenSideCastleW = PlyWhereLostQueenSideCastleW,
                PlyWhereLostKingSideCastleB = PlyWhereLostKingSideCastleB,
                PlyWhereLostQueenSideCastleB = PlyWhereLostQueenSideCastleB,
                winnerColor = winnerColor,
                isGameOver = isGameOver,
                plyCount = plyCount,
                numPlySinceLastEvent = numPlySinceLastEvent,
                Duck = Duck
            };

            // Deep copy all piece lists
            copy.WhitePawns = WhitePawns.Clone();
            copy.BlackPawns = BlackPawns.Clone();
            copy.WhiteKnights = WhiteKnights.Clone();
            copy.BlackKnights = BlackKnights.Clone();
            copy.WhiteBishops = WhiteBishops.Clone();
            copy.BlackBishops = BlackBishops.Clone();
            copy.WhiteRooks = WhiteRooks.Clone();
            copy.BlackRooks = BlackRooks.Clone();
            copy.WhiteQueens = WhiteQueens.Clone();
            copy.BlackQueens = BlackQueens.Clone();

            copy.WhiteKing = WhiteKing;
            copy.BlackKing = BlackKing;

            // Clone legal moves
            copy.legalMoves = new List<Move>(legalMoves);

            return copy;
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
            PlyWhereLostKingSideCastleW = PlyWhereLostQueenSideCastleW = NOT_ON_BOARD;
            PlyWhereLostKingSideCastleB = PlyWhereLostQueenSideCastleB = NOT_ON_BOARD;
            Duck = NOT_ON_BOARD;
            enPassantSquare = -1;
        }

        /// <summary>
        /// Initializes the board with the starting positions.
        /// </summary>
        public void LoadStartPosition()
        {
            ResetBoardState();

            // Initialize pieces
            WhitePawns = InitializePieceList(MAX_PAWN_COUNT, 8, 16, Piece.White | Piece.Pawn);
            BlackPawns = InitializePieceList(MAX_PAWN_COUNT, 48, 56, Piece.Black | Piece.Pawn);

            WhiteKnights = InitializePieceList(MAX_KNIGHT_COUNT, new[] { 1, 6 }, Piece.White | Piece.Knight);
            BlackKnights = InitializePieceList(MAX_KNIGHT_COUNT, new[] { 57, 62 }, Piece.Black | Piece.Knight);

            WhiteBishops = InitializePieceList(MAX_BISHOP_COUNT, new[] { 2, 5 }, Piece.White | Piece.Bishop);
            BlackBishops = InitializePieceList(MAX_BISHOP_COUNT, new[] { 58, 61 }, Piece.Black | Piece.Bishop);

            WhiteRooks = InitializePieceList(MAX_ROOK_COUNT, new[] { 0, 7 }, Piece.White | Piece.Rook);
            BlackRooks = InitializePieceList(MAX_ROOK_COUNT, new[] { 56, 63 }, Piece.Black | Piece.Rook);

            WhiteQueens = InitializePieceList(MAX_QUEEN_COUNT, new[] { 3 }, Piece.White | Piece.Queen);
            BlackQueens = InitializePieceList(MAX_QUEEN_COUNT, new[] { 59 }, Piece.Black | Piece.Queen);

            PlaceKings();
            turnColor = Piece.White;
            GenerateNormalMoves();
        }

        private PieceList InitializePieceList(int maxCount, int start, int end, int piece)
        {
            var list = new PieceList(maxCount);
            for (int i = start; i < end; i++)
            {
                Squares[i] = piece;
                list.AddPieceAtSquare(i);
            }
            return list;
        }

        private PieceList InitializePieceList(int maxCount, int[] positions, int piece)
        {
            var list = new PieceList(maxCount);
            foreach (var pos in positions)
            {
                Squares[pos] = piece;
                list.AddPieceAtSquare(pos);
            }
            return list;
        }

        private void PlaceKings()
        {
            WhiteKing = 4;
            BlackKing = 60;

            Squares[WhiteKing] = Piece.White | Piece.King;
            Squares[BlackKing] = Piece.Black | Piece.King;
        }

        /// <summary>
        /// Makes a move on the board and updates its state.
        /// </summary>
        public void MakeMove(ref Move move)
        {
            bool isWhite = turnColor == Piece.White;
            UpdatePieceLists(ref move, isWhite);
            UpdateSquares(move);

            SwitchTurn();
        }

        private void UpdatePieceLists(ref Move move, bool isWhite)
        {
            int piece = Squares[move.StartSquare];
            PieceList pieceList = GetPieceList(Piece.PieceType(piece), isWhite);

            if (pieceList != null)
            {
                pieceList.MovePiece(move);
                HandleSpecialMoves(ref move, isWhite, pieceList);
            }
            else if (Piece.PieceType(piece) == Piece.Duck)
            {
                Duck = move.TargetSquare;
            }
        }

        private void HandleSpecialMoves(ref Move move, bool isWhite, PieceList pieceList)
        {
            if (move.MoveFlag == Move.Flag.EnPassantCapture)
                HandleEnPassant(move, isWhite);

            if (move.IsPromotion)
                PromotePawn(move, isWhite, pieceList);

            if (move.MoveFlag == Move.Flag.PawnTwoForward)
                enPassantSquare = move.TargetSquare;
        }

        private void HandleEnPassant(Move move, bool isWhite)
        {
            PieceList enemyPawns = isWhite ? BlackPawns : WhitePawns;
            int enemySquare = move.TargetSquare + (isWhite ? -8 : 8);
            enemyPawns.RemovePieceAtSquare(enemySquare);
            Squares[enemySquare] = Piece.None;
        }

        private void PromotePawn(Move move, bool isWhite, PieceList pawns)
        {
            pawns.RemovePieceAtSquare(move.TargetSquare);
            PieceList promotionList = GetPieceList(move.PromotionPieceType, isWhite);
            promotionList.AddPieceAtSquare(move.TargetSquare);
            Squares[move.TargetSquare] = move.PromotionPieceType | (isWhite ? Piece.White : Piece.Black);
        }

        private PieceList GetPieceList(int pieceType, bool isWhite)
        {
            return pieceType switch
            {
                Piece.Pawn => isWhite ? WhitePawns : BlackPawns,
                Piece.Knight => isWhite ? WhiteKnights : BlackKnights,
                Piece.Bishop => isWhite ? WhiteBishops : BlackBishops,
                Piece.Rook => isWhite ? WhiteRooks : BlackRooks,
                Piece.Queen => isWhite ? WhiteQueens : BlackQueens,
                _ => null
            };
        }

        private void UpdateSquares(Move move)
        {
            Squares[move.TargetSquare] = Squares[move.StartSquare];
            Squares[move.StartSquare] = Piece.None;
        }

        private void SwitchTurn()
        {
            plyCount++;
            numPlySinceLastEvent++;
            duckTurn = !duckTurn;

            if (duckTurn)
                GenerateDuckMoves();
            else
            {
                turnColor = turnColor == Piece.White ? Piece.Black : Piece.White;
                GenerateNormalMoves();
            }
        }

        private void GenerateNormalMoves() => LegalMoveGenerator.GeneratePawnMoves(ref legalMoves, this);
        private void GenerateDuckMoves() => LegalMoveGenerator.GenerateDuckMoves(ref legalMoves, this);

        public override string ToString()
        {
            var boardString = "Board State:\n";
            for (int i = 7; i >= 0; i--)
            {
                int row = i * 8;
                for (int j = 0; j < 8; j++)
                {
                    boardString += FormatPieceString(Squares[row + j]) + " ";
                }
                boardString += "\n";
            }
            return boardString;
        }

        private string FormatPieceString(int piece)
        {
            string pieceChar = Piece.PieceType(piece) switch
            {
                Piece.Pawn => "P",
                Piece.Knight => "N",
                Piece.Bishop => "B",
                Piece.Rook => "R",
                Piece.Queen => "Q",
                Piece.King => "K",
                Piece.Duck => "D",
                _ => "-"
            };
            string color = Piece.Color(piece) == Piece.White ? "W" : Piece.Color(piece) == Piece.Black ? "B" : " ";
            return $"{color}{pieceChar}";
        }
    

    /// <summary>
/// Reverts the given move, restoring the previous board state.
/// </summary>
/// <param name="move">The move to undo.</param>
/// <param name="previousNumPlySinceLastEvent">The draw counter value before the move was made.</param>
public void UnmakeMove(Move move, int previousNumPlySinceLastEvent)
{
    // Revert turn info
    plyCount--;
    duckTurn = !duckTurn;
    numPlySinceLastEvent = previousNumPlySinceLastEvent;

    // If this was a duck move
    if (move.MoveFlag == Move.Flag.FirstDuckMove)
    {
        Duck = NOT_ON_BOARD;
        Squares[move.TargetSquare] = Piece.None;
        return;
    }

    // Restore the piece to its original position
    int piece = Squares[move.TargetSquare];
    Squares[move.StartSquare] = piece;
    Squares[move.TargetSquare] = move.CapturedPiece;

    // Revert en passant square
    if (move.MoveFlag == Move.Flag.PawnTwoForward)
    {
        enPassantSquare = NOT_ON_BOARD;
    }

    // Handle captured piece restoration
    if (move.IsCapture)
    {
        int capturedPiece = move.CapturedPiece;
        if (capturedPiece != Piece.None)
        {
            PieceList capturedPieceList = GetPieceList(Piece.PieceType(capturedPiece), Piece.Color(capturedPiece) == Piece.White);
            capturedPieceList.AddPieceAtSquare(move.TargetSquare);
        }
    }

    // Handle pawn promotion reversal
    if (move.IsPromotion)
    {
        PieceList promotionList = GetPieceList(Piece.PieceType(piece), move.PromotionPieceType == Piece.White);
        promotionList.RemovePieceAtSquare(move.TargetSquare);
        PieceList pawnList = GetPieceList(Piece.Pawn, move.PromotionPieceType == Piece.White);
        pawnList.AddPieceAtSquare(move.TargetSquare);
    }
    else
    {
        // Regular piece movement undo
        PieceList movedPieceList = GetPieceList(Piece.PieceType(piece), Piece.Color(piece) == Piece.White);
        movedPieceList.UnmovePiece(move);
    }

    // Handle castling
    if (move.MoveFlag == Move.Flag.Castling)
    {
        bool isKingSide = move.TargetSquare > move.StartSquare;
        int rookStart = isKingSide ? move.TargetSquare + 1 : move.TargetSquare - 2;
        int rookEnd = isKingSide ? move.TargetSquare - 1 : move.TargetSquare + 1;

        Squares[rookStart] = Squares[rookEnd];
        Squares[rookEnd] = Piece.None;

        PieceList rookList = GetPieceList(Piece.Rook, Piece.Color(piece) == Piece.White);
        rookList.UnmovePiece(new Move(rookEnd, rookStart));
    }

    // Restore king position if necessary
    if (Piece.PieceType(piece) == Piece.King)
    {
        if (Piece.Color(piece) == Piece.White)
        {
            WhiteKing = move.StartSquare;
        }
        else
        {
            BlackKing = move.StartSquare;
        }
    }

    // Restore turn color and generate moves
    if (!duckTurn)
    {
        turnColor = turnColor == Piece.White ? Piece.Black : Piece.White;
        GenerateNormalMoves();
    }
    else
    {
        GenerateDuckMoves();
    }
}
    }

}
