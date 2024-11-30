namespace DuckChess
{
    /// <summary>
    /// A static class containing methods to interpret a given integer as a piece.
    /// </summary>
    public static class Piece
    {
        /// <summary>
        /// The piece value of an empty square
        /// </summary>
        public const int None = 0;

        /// <summary>
        /// The piece value of a king
        /// </summary>
        public const int King = 1;

        /// <summary>
        /// The piece value of a pawn
        /// </summary>
        public const int Pawn = 2;

        /// <summary>
        /// The piece value of a knight
        /// </summary>
        public const int Knight = 3;

        /// <summary>
        /// The piece value of a bishop
        /// </summary>
        public const int Bishop = 5;

        /// <summary>
        /// The piece value of a rook
        /// </summary>
        public const int Rook = 6;

        /// <summary>
        /// The piece value of a queen
        /// </summary>
        public const int Queen = 7;

        /// <summary>
        /// The piece value of a duck
        /// </summary>
        public const int Duck = 8;

        /// <summary>
        /// The color value of a white piece
        /// </summary>
        public const int White = 16;

        /// <summary>
        /// The color value of a black piece
        /// </summary>
        public const int Black = 32;

        /// <summary>
        /// The color value of an empty square or a duck
        /// </summary>
        public const int NoColor = 0;

        const int typeMask =  0b001111;
        const int colorMask = 0b110000;

        /// <summary>
        /// Whether or not a given piece is the given color
        /// </summary>
        /// <param name="piece">The piece to check</param>
        /// <param name="color">The color to check</param>
        public static bool IsColor(int piece, int color)
        {
            return (piece & colorMask) == color;
        }

        /// <summary>
        /// The color of a given piece
        /// </summary>
        /// <param name="piece">The piece to check</param>
        public static int Color(int piece)
        {
            return piece & colorMask;
        }

        /// <summary>
        /// The type of a given piece
        /// </summary>
        /// <param name="piece">The piece to check</param>
        public static int PieceType(int piece)
        {
            return piece & typeMask;
        }

        //public static bool IsRookOrQueen(int piece)
        //{
        //    return (piece & 0b110) == 0b110;
        //}

        //public static bool IsBishopOrQueen(int piece)
        //{
        //    return (piece & 0b101) == 0b101;
        //}

        //public static bool IsSlidingPiece(int piece)
        //{
        //    return (piece & 0b100) != 0;
        //}
    }
}