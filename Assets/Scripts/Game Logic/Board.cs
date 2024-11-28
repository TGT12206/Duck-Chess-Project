using System.Collections.Generic;

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
        /// The color of the player whose turn it is to move.
        /// </summary>
        public int turnColor;

        /// <summary>
        /// Whether or not it is a duck turn.
        /// </summary>
        public bool duckTurn;

        public int enPassantColumn;

        public List<Move> legalPawnMoves;
        public List<Move> legalKnightMoves;
        public List<Move> legalBishopMoves;
        public List<Move> legalRookMoves;
        public List<Move> legalQueenMoves;
        public List<Move> legalKingMoves;
        public List<Move> legalDuckMoves;

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
        /// The location and number of the duck
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
            // Set the en passant column out of bounds
            enPassantColumn = -1;

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

            // Set the duck
            Duck = -1;
            GenerateNormalMoves();
        }
        public void MakeMove(Move move)
        {
            int startSquare = move.StartSquare;
            int targetSquare = move.TargetSquare;
            Squares[targetSquare] = Squares[startSquare];
            Squares[startSquare] = Piece.None;
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
            LegalMoveGenerator.GenerateKingMoves(ref legalKingMoves, this);
        }

        public bool IsMoveLegal(ref Move move)
        {
            bool isLegal = false;
            int pieceType = Piece.PieceType(Squares[move.StartSquare]);
            switch (pieceType)
            {
                case Piece.Pawn:
                    isLegal = CheckPawnMove(ref move);
                    break;
            }
            return isLegal;
        }

        public bool CheckPawnMove(ref Move move)
        {
            foreach(Move legalMove in legalPawnMoves)
            {
                if (Move.SameMove(move, legalMove))
                {
                    // It is easier to copy the already calculated flags in legal move
                    // than to recalculate before calling this method
                    move = legalMove;
                    return true;
                }
            }
            return false;
        }

        public int GetRowOf(int square)
        {
            return square / 8;
        }
        public int GetColumnOf(int square)
        {
            return square % 8;
        }

        public int this[int index] => Squares[index];
    }
}