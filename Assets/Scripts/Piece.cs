namespace DuckChess
{
    /// <summary>
    /// A static class containing methods to interpret a given integer as a piece.
    /// </summary>
    public static class Piece
    {
        public const int None = 0;
        public const int King = 1;
        public const int Pawn = 2;
        public const int Knight = 3;
        public const int Bishop = 5;
        public const int Rook = 6;
        public const int Queen = 7;
        public const int Duck = 8;

        public const int NoColor = 0;
        public const int White = 16;
        public const int Black = 32;

        const int typeMask = 0b001111;
        const int blackMask = 0b100000;
        const int whiteMask = 0b010000;
        const int colourMask = whiteMask | blackMask;

        public static bool IsColour(int piece, int colour)
        {
            return (piece & colourMask) == colour;
        }

        public static int Colour(int piece)
        {
            return piece & colourMask;
        }

        public static int PieceType(int piece)
        {
            return piece & typeMask;
        }

        public static bool IsRookOrQueen(int piece)
        {
            return (piece & 0b110) == 0b110;
        }

        public static bool IsBishopOrQueen(int piece)
        {
            return (piece & 0b101) == 0b101;
        }

        public static bool IsSlidingPiece(int piece)
        {
            return (piece & 0b100) != 0;
        }

        public static bool IsDuck(int piece)
        {
            return (piece & 0b1000) != 0;
        }
    }
}