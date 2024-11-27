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

        #region Piece Locations
        // Information about (mostly location and number) each piece type
        public PieceList BlackBishops;
        public PieceList WhiteBishops;
        public int BlackKing;
        public int WhiteKing;
        public PieceList BlackKnights;
        public PieceList WhiteKnights;
        public PieceList BlackPawns;
        public PieceList WhitePawns;
        public PieceList BlackQueens;
        public PieceList WhiteQueens;
        public PieceList BlackRooks;
        public PieceList WhiteRooks;
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
        }

        /// <summary>
        /// Loads the starting position onto the board.
        /// </summary>
        public void LoadStartPosition()
        {
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
        }
        public void MakeMove(Move move)
        {
            int startSquare = move.StartSquare;
            int targetSquare = move.TargetSquare;
            Squares[targetSquare] = Squares[startSquare];
            Squares[startSquare] = Piece.None;
        }
    }
}