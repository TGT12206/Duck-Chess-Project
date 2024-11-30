using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using UnityEngine;

namespace DuckChess
{
    /// <summary>
    /// Represents a board state.
    /// </summary>
    public class Board {
        /// <summary>
        /// This array stores what exactly is on each square.
        /// A square could contain no pieces or contain a piece.
        /// </summary>
        public int[] Squares;

        /// <summary>
        /// Used by variables that represent a space on the board, but
        /// sometimes don't exist on a given turn.
        /// </summary>
        public const int NOT_ON_BOARD = -2;

        #region Turn Information
        /// <summary>
        /// The color of the player whose turn it is to move.
        /// </summary>
        public int turnColor;

        /// <summary>
        /// Whether or not it is a duck turn.
        /// </summary>
        public bool duckTurn;
        #endregion

        /// <summary>
        /// The square behind a pawn that just moved two spaces forward.
        /// </summary>
        public int enPassantSquare;

        #region Castling Info
        /// <summary>
        /// Whether kingside castling as white is still possible
        /// </summary>
        public bool CastleKingSideW;

        /// <summary>
        /// Whether queenside castling as white is still possible
        /// </summary>
        public bool CastleQueenSideW;

        /// <summary>
        /// Whether kingside castling as black is still possible
        /// </summary>
        public bool CastleKingSideB;

        /// <summary>
        /// Whether queenside castling as black is still possible
        /// </summary>
        public bool CastleQueenSideB;
        #endregion

        #region Game End Info
        /// <summary>
        /// The color of the winning player.
        /// If the game is a draw or has not ended, it is
        /// equal to Piece.NoColor
        /// </summary>
        public int winnerColor;

        /// <summary>
        /// Whether the game has ended
        /// </summary>
        public bool isGameOver;

        /// <summary>
        /// The number of moves since the last significant action.
        /// <br></br>
        /// Used for the 50 move automatic draw.
        /// <br></br>
        /// The draw happens when this value reaches 200.
        /// </summary>
        public int numPlySinceLastEvent;
        #endregion

        #region Lists of Legal Moves
        /// <summary>
        /// A list containing all of the legal moves that can be
        /// made by a pawn in this position and turn.
        /// </summary>
        public List<Move> legalPawnMoves;

        /// <summary>
        /// A list containing all of the legal moves that can be
        /// made by a knight in this position and turn.
        /// </summary>
        public List<Move> legalKnightMoves;

        /// <summary>
        /// A list containing all of the legal moves that can be
        /// made by a bishop in this position and turn.
        /// </summary>
        public List<Move> legalBishopMoves;

        /// <summary>
        /// A list containing all of the legal moves that can be
        /// made by a rook in this position and turn.
        /// </summary>
        public List<Move> legalRookMoves;

        /// <summary>
        /// A list containing all of the legal moves that can be
        /// made by a queen in this position and turn.
        /// </summary>
        public List<Move> legalQueenMoves;

        /// <summary>
        /// A list containing all of the legal moves that can be
        /// made by a king in this position and turn.
        /// </summary>
        public List<Move> legalKingMoves;

        /// <summary>
        /// A list containing all of the legal moves that can be
        /// made by a duck in this position and turn.
        /// </summary>
        public List<Move> legalDuckMoves;
        #endregion

        #region Piece Locations
        // Information about (mostly location and number) each piece type
        /// <summary>
        /// The location and number of the black bishops
        /// </summary>
        public PieceList BlackBishops;
        /// <summary>
        /// The location and number of the white bishops
        /// </summary>
        public PieceList WhiteBishops;
        /// <summary>
        /// The location of the black king
        /// </summary>
        public int BlackKing;
        /// <summary>
        /// The location and number of the white king
        /// </summary>
        public int WhiteKing;
        /// <summary>
        /// The location and number of the black knights
        /// </summary>
        public PieceList BlackKnights;
        /// <summary>
        /// The location and number of the white knights
        /// </summary>
        public PieceList WhiteKnights;
        /// <summary>
        /// The location and number of the black pawns
        /// </summary>
        public PieceList BlackPawns;
        /// <summary>
        /// The location and number of the white pawns
        /// </summary>
        public PieceList WhitePawns;
        /// <summary>
        /// The location and number of the black queens
        /// </summary>
        public PieceList BlackQueens;
        /// <summary>
        /// The location and number of the white queens
        /// </summary>
        public PieceList WhiteQueens;
        /// <summary>
        /// The location and number of the black rooks
        /// </summary>
        public PieceList BlackRooks;
        /// <summary>
        /// The location and number of the white rooks
        /// </summary>
        public PieceList WhiteRooks;
        /// <summary>
        /// The location of the duck
        /// </summary>
        public int Duck;
        #endregion

        #region Max number of each piece type, for one color
        public const int MAX_PAWN_COUNT = 8; // 8 pawns initially
        public const int MAX_KNIGHT_COUNT = 10; // 2 knights initially, 8 pawns to promote
        public const int MAX_BISHOP_COUNT = 10; // 2 bishops initially, 8 pawns to promote
        public const int MAX_ROOK_COUNT = 10; // 2 rooks initially, 8 pawns to promote
        public const int MAX_QUEEN_COUNT = 9; // 1 queen initially, 8 pawns to promote
        #endregion

        public Board ()
        {
            Squares = new int[64];
            ResetLegalMoves();
            CastleKingSideW = true;
            CastleQueenSideW = true;
            CastleKingSideB = true;
            CastleQueenSideB = true;
            turnColor = Piece.NoColor;
            winnerColor = Piece.NoColor;
            isGameOver = false;
            numPlySinceLastEvent = 0;
        }

        private void ResetLegalMoves()
        {
            legalPawnMoves = new List<Move>();
            legalKnightMoves = new List<Move>();
            legalBishopMoves = new List<Move>();
            legalRookMoves = new List<Move>();
            legalQueenMoves = new List<Move>();
            legalKingMoves = new List<Move>();
            legalDuckMoves = new List<Move>();
        }

        /// <summary>
        /// Loads the starting position onto the board.
        /// </summary>
        public void LoadStartPosition()
        {
            // All the castling can still happen
            CastleKingSideW = true;
            CastleQueenSideW = true;
            CastleKingSideB = true;
            CastleQueenSideB = true;

            // Set the en passant square out of bounds
            enPassantSquare = -1;

            // Set the pawns
            WhitePawns = new PieceList(MAX_PAWN_COUNT);
            for (int i = 8; i < 16; i++)
            {
                Squares[i] = Piece.White | Piece.Pawn;
                WhitePawns.AddPieceAtSquare(i);
            }
            BlackPawns = new PieceList(MAX_PAWN_COUNT);
            for (int i = 48; i < 56; i++)
            {
                Squares[i] = Piece.Black | Piece.Pawn;
                BlackPawns.AddPieceAtSquare(i);
            }

            // Set the knights
            WhiteKnights = new PieceList(MAX_KNIGHT_COUNT);
            for (int i = 1; i < 7; i += 5)
            {
                Squares[i] = Piece.White | Piece.Knight;
                WhiteKnights.AddPieceAtSquare(i);
            }
            BlackKnights = new PieceList(MAX_KNIGHT_COUNT);
            for (int i = 57; i < 63; i += 5)
            {
                Squares[i] = Piece.Black | Piece.Knight;
                BlackKnights.AddPieceAtSquare(i);
            }

            // Set the bishops
            WhiteBishops = new PieceList(MAX_BISHOP_COUNT);
            for (int i = 2; i < 6; i += 3)
            {
                Squares[i] = Piece.White | Piece.Bishop;
                WhiteBishops.AddPieceAtSquare(i);
            }
            BlackBishops = new PieceList(MAX_BISHOP_COUNT);
            for (int i = 58; i < 62; i += 3)
            {
                Squares[i] = Piece.Black | Piece.Bishop;
                BlackBishops.AddPieceAtSquare(i);
            }

            // Set the rooks
            WhiteRooks = new PieceList(MAX_ROOK_COUNT);
            for (int i = 0; i < 8; i += 7)
            {
                Squares[i] = Piece.White | Piece.Rook;
                WhiteRooks.AddPieceAtSquare(i);
            }
            BlackRooks = new PieceList(MAX_ROOK_COUNT);
            for (int i = 56; i < 64; i += 7)
            {
                Squares[i] = Piece.Black | Piece.Rook;
                BlackRooks.AddPieceAtSquare(i);
            }

            // Set the Queens
            WhiteQueens = new PieceList(MAX_QUEEN_COUNT);
            Squares[3] = Piece.White | Piece.Queen;
            WhiteQueens.AddPieceAtSquare(3);
            BlackQueens= new PieceList(MAX_QUEEN_COUNT);
            Squares[59] = Piece.Black | Piece.Queen;
            BlackQueens.AddPieceAtSquare(59);

            // Set the Kings
            WhiteKing = 4;
            Squares[4] = Piece.White | Piece.King;
            BlackKing = 60;
            Squares[60] = Piece.Black | Piece.King;

            // Set the turn color
            turnColor = Piece.White;
            duckTurn = false;
            winnerColor = Piece.NoColor;
            isGameOver = false;
            numPlySinceLastEvent = 0;

            // Set the duck
            Duck = NOT_ON_BOARD;
            GenerateNormalMoves();
        }

        /// <summary>
        /// Make the given move on the board.
        /// </summary>
        /// <param name="move">The move to make</param>
        public void MakeMove(Move move)
        {
            int startSquare = move.StartSquare;
            int targetSquare = move.TargetSquare;
            if (duckTurn && Duck == NOT_ON_BOARD)
            {
                MoveDuck(move);
                Squares[targetSquare] = Piece.Duck;
            } else
            {
                int pieceType = Piece.PieceType(Squares[startSquare]);
                bool isWhite = turnColor == Piece.White;
                switch (pieceType)
                {
                    case Piece.Pawn:
                        MovePawn(move, isWhite);
                        break;
                    case Piece.Knight:
                        MoveKnight(move, isWhite);
                        break;
                    case Piece.Bishop:
                        MoveBishop(move, isWhite);
                        break;
                    case Piece.Rook:
                        MoveRook(move, isWhite);
                        break;
                    case Piece.Queen:
                        MoveQueen(move, isWhite);
                        break;
                    case Piece.King:
                        MoveKing(move, isWhite);
                        break;
                    case Piece.Duck:
                        MoveDuck(move);
                        break;
                }
                if (Squares[move.TargetSquare] != Piece.None)
                {
                    CapturePieceNormally(move, isWhite);
                }
                SwapSquares(move);
            }
            ResetLegalMoves();
            if (duckTurn)
            {
                duckTurn = false;
                turnColor = turnColor == Piece.White ? Piece.Black : Piece.White;
                GenerateNormalMoves();
            } else
            {
                duckTurn= true;
                GenerateDuckMoves();
            }
            if (numPlySinceLastEvent >= 200)
            {
                isGameOver = true;
                winnerColor = Piece.NoColor;
            }
        }

        private void SwapSquares(Move move)
        {
            Squares[move.TargetSquare] = Squares[move.StartSquare];
            Squares[move.StartSquare] = Piece.None;
        }

        private void MovePawn(Move move, bool isWhite)
        {
            PieceList friendlyPawns = isWhite ? WhitePawns : BlackPawns;
            int findSquareBehind = isWhite ? -8 : 8;

            friendlyPawns.MovePiece(move);

            if (move.MoveFlag == Move.Flag.PawnTwoForward)
            {
                enPassantSquare = move.TargetSquare + findSquareBehind;
            } else
            {
                enPassantSquare = NOT_ON_BOARD;
            }
            if (move.IsPromotion)
            {
                PromotePawn(move, friendlyPawns, isWhite);
            }
            else if (move.MoveFlag == Move.Flag.EnPassantCapture)
            {
                EnPassant(move, isWhite);
            }
            numPlySinceLastEvent = 0;
        }

        private void MoveKnight(Move move, bool isWhite)
        {
            PieceList friendlyKnights = isWhite ? WhiteKnights : BlackKnights;
            friendlyKnights.MovePiece(move);
            enPassantSquare = NOT_ON_BOARD;
            numPlySinceLastEvent++;
        }

        private void MoveBishop(Move move, bool isWhite)
        {
            PieceList friendlyBishops = isWhite ? WhiteBishops : BlackBishops;
            friendlyBishops.MovePiece(move);
            enPassantSquare = NOT_ON_BOARD;
            numPlySinceLastEvent++;
        }

        private void MoveRook(Move move, bool isWhite)
        {
            PieceList friendlyRooks = isWhite ? WhiteRooks : BlackRooks;
            friendlyRooks.MovePiece(move);
            if (isWhite)
            {
                switch (move.StartSquare)
                {
                    case 0:
                        CastleQueenSideW = false;
                        break;
                    case 7:
                        CastleKingSideW = false;
                        break;
                    default:
                        break;
                }
            } else
            {
                switch (move.StartSquare)
                {
                    case 56:
                        CastleQueenSideB = false;
                        break;
                    case 63:
                        CastleKingSideB = false;
                        break;
                    default:
                        break;
                }
            }
            enPassantSquare = NOT_ON_BOARD;
            numPlySinceLastEvent++;
        }

        private void MoveQueen(Move move, bool isWhite)
        {
            PieceList friendlyQueens = isWhite ? WhiteQueens : BlackQueens;
            friendlyQueens.MovePiece(move);
            enPassantSquare = NOT_ON_BOARD;
            numPlySinceLastEvent++;
        }

        private void MoveKing(Move move, bool isWhite)
        {
            if (isWhite)
            {
                WhiteKing = move.TargetSquare;
                CastleQueenSideW = false;
                CastleKingSideW = false;
            } else
            {
                BlackKing = move.TargetSquare;
                CastleQueenSideB = false;
                CastleKingSideB = false;
            }
            if (move.MoveFlag == Move.Flag.Castling)
            {
                bool isKingSide = move.TargetSquare - move.StartSquare > 0;

                int rookSpot = move.StartSquare;
                rookSpot += isKingSide ? + 3 : -4;

                int newRookSpot = move.StartSquare;
                newRookSpot += isKingSide ? 1 : -1;

                PieceList friendlyRooks = isWhite ? WhiteRooks : BlackRooks;

                Move moveRook = new Move(rookSpot, newRookSpot);

                friendlyRooks.MovePiece(moveRook);
                SwapSquares(moveRook);
            }
            enPassantSquare = NOT_ON_BOARD;
            numPlySinceLastEvent++;
        }

        private void MoveDuck(Move move)
        {
            Duck = move.TargetSquare;
            numPlySinceLastEvent++;
        }

        private void CapturePieceNormally(Move move, bool isWhite)
        {
            int capturedPiece = Squares[move.TargetSquare];
            PieceList CapturedTypeEnemyPieces = null;
            switch (Piece.PieceType(capturedPiece))
            {
                case Piece.Pawn:
                    CapturedTypeEnemyPieces = isWhite ? BlackPawns : WhitePawns;
                    break;
                case Piece.Knight:
                    CapturedTypeEnemyPieces = isWhite ? BlackKnights : WhiteKnights;
                    break;
                case Piece.Bishop:
                    CapturedTypeEnemyPieces = isWhite ? BlackBishops : WhiteBishops;
                    break;
                case Piece.Rook:
                    CapturedTypeEnemyPieces = isWhite ? BlackRooks : WhiteRooks;
                    break;
                case Piece.Queen:
                    CapturedTypeEnemyPieces = isWhite ? BlackQueens : WhiteQueens;
                    break;
                case Piece.King:
                    int capturedKing = isWhite ? BlackKing : WhiteKing;
                    capturedKing = NOT_ON_BOARD;
                    isGameOver = true;
                    winnerColor = turnColor;
                    return;
            }
            CapturedTypeEnemyPieces.RemovePieceAtSquare(move.TargetSquare);
            numPlySinceLastEvent = 0;
        }

        private void PromotePawn(Move move, PieceList friendlyPawns, bool isWhite)
        {
            friendlyPawns.RemovePieceAtSquare(move.TargetSquare);
            switch(move.PromotionPieceType)
            {
                case Piece.Knight:
                    PieceList friendlyKnights = isWhite ? WhiteKnights : BlackKnights;
                    friendlyKnights.AddPieceAtSquare(move.TargetSquare);
                    Squares[move.StartSquare] = Piece.Knight;
                    break;
                case Piece.Bishop:
                    PieceList friendlyBishops = isWhite ? WhiteBishops : BlackBishops;
                    friendlyBishops.AddPieceAtSquare(move.TargetSquare);
                    Squares[move.StartSquare] = Piece.Bishop;
                    break;
                case Piece.Rook:
                    PieceList friendlyRooks = isWhite ? WhiteRooks : BlackRooks;
                    friendlyRooks.AddPieceAtSquare(move.TargetSquare);
                    Squares[move.StartSquare] = Piece.Rook;
                    break;
                case Piece.Queen:
                    PieceList friendlyQueens = isWhite ? WhiteQueens : BlackQueens;
                    friendlyQueens.AddPieceAtSquare(move.TargetSquare);
                    Squares[move.StartSquare] = Piece.Queen;
                    break;
            }
            Squares[move.StartSquare] |= isWhite ? Piece.White : Piece.Black;
        }

        private void EnPassant(Move move, bool isWhite)
        {
            PieceList enemyPawns = isWhite ? BlackPawns : WhitePawns;
            int enemyPawnSquare = isWhite ? -8 : 8;
            enemyPawnSquare += move.TargetSquare;

            enemyPawns.RemovePieceAtSquare(enemyPawnSquare);
            Squares[enemyPawnSquare] = Piece.None;
        }

        private void GenerateNormalMoves()
        {
            LegalMoveGenerator.GeneratePawnMoves(ref legalPawnMoves, this);
            LegalMoveGenerator.GenerateKnightMoves(ref legalKnightMoves, this);
            LegalMoveGenerator.GenerateBishopMoves(ref legalBishopMoves, this);
            LegalMoveGenerator.GenerateRookMoves(ref legalRookMoves, this);
            LegalMoveGenerator.GenerateQueenMoves(ref legalQueenMoves, this);
            LegalMoveGenerator.GenerateKingMoves(ref legalKingMoves, this);
        }

        private void GenerateDuckMoves()
        {
            LegalMoveGenerator.GenerateDuckMoves(ref legalDuckMoves, this);
        }

        /// <summary>
        /// Given a move, check whether or not it is legal in this position.
        /// </summary>
        /// <param name="move">The move to check</param>
        public bool IsMoveLegal(ref Move move)
        {
            int pieceType;
            if (duckTurn)
            {
                pieceType = Piece.Duck;
            } else
            {
                pieceType = Piece.PieceType(Squares[move.StartSquare]);
            }
            List<Move> legalMoves = null;
            switch (pieceType)
            {
                case Piece.Duck:
                    legalMoves = legalDuckMoves;
                    break;
                case Piece.Pawn:
                    legalMoves = legalPawnMoves;
                    break;
                case Piece.Knight:
                    legalMoves = legalKnightMoves;
                    break;
                case Piece.Bishop:
                    legalMoves = legalBishopMoves;
                    break;
                case Piece.Rook:
                    legalMoves = legalRookMoves;
                    break;
                case Piece.Queen:
                    legalMoves = legalQueenMoves;
                    break;
                case Piece.King:
                    legalMoves = legalKingMoves;
                    break;
            }
            foreach (Move legalMove in legalMoves)
            {
                if (Move.SameMove(move, legalMove))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Given the index of a square,
        /// returns the row of the square (from 0-7)
        /// </summary>
        public static int GetRowOf(int square)
        {
            return square / 8;
        }

        /// <summary>
        /// Given the index of a square,
        /// returns the column of the square (from 0-7)
        /// </summary>
        public static int GetColumnOf(int square)
        {
            return square % 8;
        }

        public int this[int index] => Squares[index];

        /// <summary>
        /// Returns this board as a formatted string.
        /// </summary>
        public override string ToString()
        {
            string boardString = "";
            for (int i = 7; i >= 0; i--)
            {
                int row = i * 8;
                for (int j = 0; j < 8; j++)
                {
                    int piece = Squares[row + j];
                    int pieceType = Piece.PieceType(piece);
                    String pieceTypeChar = "  -";
                    switch (pieceType)
                    {
                        case Piece.Pawn:
                            pieceTypeChar = "P";
                            break;
                        case Piece.Knight:
                            pieceTypeChar = "N";
                            break;
                        case Piece.Bishop:
                            pieceTypeChar = "B";
                            break;
                        case Piece.Rook:
                            pieceTypeChar = "R";
                            break;
                        case Piece.Queen:
                            pieceTypeChar = "Q";
                            break;
                        case Piece.King:
                            pieceTypeChar = "K";
                            break;
                        case Piece.Duck:
                            pieceTypeChar = "D";
                            break;
                    }
                    int color = Piece.Color(piece);
                    string colorChar = " ";
                    switch(color)
                    {
                        case Piece.Black:
                            colorChar = "B";
                            break;
                        case Piece.White:
                            colorChar = "W";
                            break;
                    }
                    boardString += colorChar + pieceTypeChar + " ";
                }
                boardString += "\n";
            }
            return boardString;
        }
    }
}