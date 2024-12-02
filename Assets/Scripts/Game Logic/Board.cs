using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine;

namespace DuckChess
{
    /// <summary>
    /// Represents a board state in Duck Chess.
    /// </summary>
    public class Board
    {
        public int[] Squares;

        public const int NOT_ON_BOARD = -2; // 63 is max

        #region Turn Information
        public int turnColor;
        public bool turnIsDuck = false;
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
        public int Duck = NOT_ON_BOARD;
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
                turnIsDuck = turnIsDuck,
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


        private void UpdateBoardAndLists(ref Move move, bool isWhite)
        {
            int piece = move.StartPiece(this);
            PieceList pieceList = GetPieceList(Piece.PieceType(piece), isWhite);

            String dbgStr = "Updating board (move attempt).\n";
            dbgStr += "Turn is white: " + (turnColor == Piece.White) + "\n";
            dbgStr += "turn is duck: " + turnIsDuck + "\n";
            dbgStr += "Piece: " + Piece.PieceStr(piece) + "\n";
            dbgStr += "Is white: " + isWhite + "\n";
            dbgStr += "Move: " + move + "\n";
            dbgStr += "List: " + pieceList + "\n";
            dbgStr += "Board: " + this + "\n";
            Debug.Log(dbgStr);

            if (pieceList != null)
            {
                AdditionalBoardHandling(ref move, isWhite, pieceList);
            }
            else if (Piece.PieceType(piece) == Piece.Duck)
            {
                Duck = move.TargetSquare;
                Squares[move.TargetSquare] = piece;
                if (move.MoveFlag != Move.Flag.FirstDuckMove)
                    Squares[move.StartSquare] = Piece.None;
            }

            Debug.Log("After attempt:\nBoard: " + this);
        }

        private void AdditionalBoardHandling(ref Move move, bool isWhite, PieceList pieceList)
        {

            if (move.MoveFlag == Move.Flag.None || move.MoveFlag == Move.Flag.PawnTwoForward)
            {
                MoveToTarget(ref move);
            }
            else
            {
                pieceList.MovePiece(move);
                if (move.MoveFlag == Move.Flag.EnPassantCapture)
                    HandleEnPassant(move, isWhite);

                else if (move.IsPromotion)
                    PromotePawn(move, isWhite, pieceList);

                else if (move.MoveFlag == Move.Flag.Castling)
                    HandleCastling(move, isWhite);
            }
        }

        /// This is probably too robust.
        private void MoveToTarget(ref Move move)
        {
            int startPiece = move.StartPiece(this);
            int startType = Piece.PieceType(startPiece);
            int startColor = Piece.Color(startPiece);
            if (startType == Piece.King)
            {
                if (startColor == Piece.White) WhiteKing = move.TargetSquare;
                else if (startColor == Piece.Black) BlackKing = move.TargetSquare;
            }
            else
            {
                PieceList startList = GetPieceList(startType, startColor == Piece.White);
                startList.MovePiece(move);
            }

            int endPiece = move.TargetPiece(this);
            int endType = Piece.PieceType(endPiece);

            if (endType != Piece.None)
            {
                PieceList endList = GetPieceList(endType, startColor != Piece.White);
                endList.RemovePieceAtSquare(move.TargetSquare);
            }

            if (move.MoveFlag == Move.Flag.PawnTwoForward)
            {
                enPassantSquare = move.TargetSquare;
            }

            Squares[move.TargetSquare] = Squares[move.StartSquare];
            Squares[move.StartSquare] = Piece.None;
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
            pawns.RemovePieceAtSquare(move.StartSquare);
            PieceList promotionList = GetPieceList(move.PromotionPieceType, isWhite);
            promotionList.AddPieceAtSquare(move.TargetSquare);
            Squares[move.TargetSquare] = move.PromotionPieceType | (isWhite ? Piece.White : Piece.Black);
        }

        private void HandleCastling(Move move, bool isWhite)
        {
            bool isKingSide = move.TargetSquare > move.StartSquare;
            int rookStart = isKingSide ? (isWhite ? 7 : 63) : (isWhite ? 0 : 56);
            int rookEnd = isKingSide ? move.TargetSquare - 1 : move.TargetSquare + 1;

            // Update squares for the rook
            Squares[rookEnd] = Squares[rookStart];
            Squares[rookStart] = Piece.None;

            // Update piece list for the rook
            PieceList rookList = GetPieceList(Piece.Rook, isWhite);
            rookList.MovePiece(new Move(rookStart, rookEnd));
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

        private void SwitchTurnForward()
        {
            Debug.Log("Just performed move for: " + turnColor + " move type: " + (turnIsDuck ? "Duck" : "Regular"));
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
        }

        private void SwitchTurnBackward(ref Move move)
        {
            bool duckMove = move.isDuckMove(this);
            Debug.Log("We are undoing move for: " + turnColor + " move type: " + (duckMove ? "Duck" : "Regular") + " board type (should be opposite): " + (turnIsDuck ? "Duck" : "Regular"));

            turnIsDuck = !turnIsDuck;

            // the next turn we want is here.
            if (turnIsDuck)
            {
                GenerateDuckMoves();
            }
            else
            {
                // keep as same color.
                // turnColor = turnColor == Piece.White ? Piece.Black : Piece.White;
                GenerateNormalMoves();
            }



        }

        /// <summary>
        /// Makes a move on the board and updates its state.
        /// </summary>
        public void MakeMove(ref Move move)
        {
            plyCount++;
            numPlySinceLastEvent++;

            bool isWhite = turnColor == Piece.White;
            UpdateBoardAndLists(ref move, isWhite);

            SwitchTurnForward();
        }


        private void GenerateNormalMoves()
        {
            legalMoves.Clear();
            LegalMoveGenerator.GeneratePawnMoves(ref legalMoves, this);

            string moves = "ALL MOVES FOR " + (turnColor == Piece.White ? "White" : "Black") + "\n";
            moves += "Duck?: " + turnIsDuck + "\n";
            foreach (Move move in legalMoves)
            {
                moves += "Piece: " + Piece.PieceStr(move.StartPiece(this)) + " | " + move.ToString() + "\n";
            }
            moves += "Board: " + this + "\n";
            Debug.Log( moves );
        }
        private void GenerateDuckMoves()
        {
            legalMoves.Clear();
            LegalMoveGenerator.GenerateDuckMoves(ref legalMoves, this);

            string moves = "ALL MOVES FOR " + (turnColor == Piece.White ? "White" : "Black") + "\n";
            moves += "Duck?: " + turnIsDuck + "\n";
            foreach (Move move in legalMoves)
            {
                moves += "Piece: " + Piece.PieceStr(move.StartPiece(this)) + " | " + move.ToString() + "\n";
            }
            moves += "Board: " + this + "\n";
            Debug.Log( moves );
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


        /// <summary>
        /// Reverts the given move, restoring the previous board state.
        /// </summary>
        /// <param name="move">The move to undo.</param>
        /// <param name="previousNumPlySinceLastEvent">The draw counter value before the move was made.</param>
        public void UnmakeMove(Move move, int previousNumPlySinceLastEvent)
        {
            String dbgStr = "Unmaking move.\n";
            dbgStr += "Move: " + move + "\n";
            dbgStr += "Board: " + this + "\n";

            Debug.Log(dbgStr);

            plyCount--;
            numPlySinceLastEvent = previousNumPlySinceLastEvent;

            // Restore the piece to its original position
            int targetPiece = move.TargetPiece(this);

            if (move.MoveFlag == Move.Flag.FirstDuckMove)
            {
                Duck = NOT_ON_BOARD;
                Squares[move.TargetSquare] = Piece.None;
                return;
            }

            // Revert en passant square
            if (move.MoveFlag == Move.Flag.PawnTwoForward)
            {
                enPassantSquare = NOT_ON_BOARD;
            }

            // Handle captured piece restoration
            else if (move.IsCapture)
            {
                int capturedPiece = move.CapturedPiece;
                if (capturedPiece != Piece.None)
                {
                    Debug.Log("Piece type: " + Piece.PieceStr(capturedPiece));
                    PieceList capturedPieceList = GetPieceList(Piece.PieceType(capturedPiece), Piece.Color(capturedPiece) == Piece.White);
                    capturedPieceList.AddPieceAtSquare(move.TargetSquare);
                    Squares[move.TargetSquare] = move.CapturedPiece;
                }
            }

            // Handle pawn promotion reversal
            else if (move.IsPromotion)
            {
                PieceList promotionList = GetPieceList(Piece.PieceType(targetPiece), Piece.Color(targetPiece) == Piece.White);
                promotionList.RemovePieceAtSquare(move.TargetSquare);
                PieceList pawnList = GetPieceList(Piece.Pawn, Piece.Color(targetPiece) == Piece.White);
                pawnList.AddPieceAtSquare(move.TargetSquare);
            }


            // Handle castling
            else if (move.MoveFlag == Move.Flag.Castling)
            {
                bool isKingSide = move.TargetSquare > move.StartSquare;
                int rookStart = isKingSide ? move.TargetSquare + 1 : move.TargetSquare - 2;
                int rookEnd = isKingSide ? move.TargetSquare - 1 : move.TargetSquare + 1;

                Squares[rookStart] = Squares[rookEnd];
                Squares[rookEnd] = Piece.None;

                PieceList rookList = GetPieceList(Piece.Rook, Piece.Color(targetPiece) == Piece.White);
                rookList.UnmovePiece(new Move(rookEnd, rookStart));
            }



            int pieceType = Piece.PieceType(targetPiece);
            if (pieceType == Piece.King)
            {
                if (Piece.Color(targetPiece) == Piece.White)
                {
                    WhiteKing = move.StartSquare;
                }
                else
                {
                    BlackKing = move.StartSquare;
                }
            }

            else if (pieceType == Piece.Duck)
            {
                Duck = move.StartSquare;
            }

            else
            {
                // Regular piece movement undo
                PieceList movedPieceList = GetPieceList(pieceType, Piece.Color(targetPiece) == Piece.White);
                movedPieceList.UnmovePiece(move);
            }


            Squares[move.StartSquare] = targetPiece;
            Squares[move.TargetSquare] = move.CapturedPiece;


            // Restore king position if necessary


            SwitchTurnBackward(ref move);


            String dbgStr1 = "Unmaking move (finish).\n";
            dbgStr1 += "Move: " + move + "\n";
            dbgStr1 += "Board: " + this + "\n";

            Debug.Log(dbgStr1);

        }
    }

}
