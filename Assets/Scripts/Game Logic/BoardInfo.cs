using System.Collections.Generic;

namespace DuckChess
{
    public static class BoardInfo
    {

        /// <summary>
        /// Gets a list of locations of all pawns of the specified color.
        /// </summary>
        /// <param name="board">The board to analyze.</param>
        /// <param name="color">The color of the pawns to locate.</param>
        /// <returns>A list of square indices where the specified pawns are located.</returns>
        public static List<int> PawnLocations(Board board, int color)
        {
            List<int> pawnLocations = new List<int>();
            for (int square = 0; square < board.Squares.Length; square++)
            {
                int piece = board.Squares[square];
                if (Piece.PieceType(piece) == Piece.Pawn && Piece.Color(piece) == color)
                {
                    pawnLocations.Add(square);
                }
            }
            return pawnLocations;
        }


        /// <summary>
        /// Checks if a given file is open (contains no pawns).
        /// </summary>
        public static bool IsOpenFile(Board board, int file)
        {
            for (int row = 0; row < 8; row++)
            {
                int square = row * 8 + file;
                int piece = board.Squares[square];
                if (Piece.PieceType(piece) == Piece.Pawn)
                {
                    return false; // A pawn exists in this file
                }
            }
            return true; // No pawns in this file
        }

        /// <summary>
        /// Counts the number of pawns of a specific color in a given file.
        /// </summary>
        public static int CountPawnsInFile(Board board, int file, int color)
        {
            int count = 0;
            for (int row = 0; row < 8; row++)
            {
                int square = row * 8 + file;
                int piece = board.Squares[square];
                if (Piece.PieceType(piece) == Piece.Pawn && Piece.Color(piece) == color)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Calculates the file (column) number of a given square.
        /// </summary>
        public static int GetFile(int square)
        {
            return square % 8;
        }

        /// <summary>
        /// Calculates the row (rank) number of a given square.
        /// </summary>
        public static int GetRow(int square)
        {
            return square / 8;
        }

        /// <summary>
        /// Checks if a pawn is doubled (multiple pawns in the same file).
        /// </summary>
        public static bool IsDoubledPawn(Board board, int pawnLocation, int color)
        {
            int file = GetFile(pawnLocation);
            return CountPawnsInFile(board, file, color) > 1;
        }

        /// <summary>
        /// Checks if a pawn is isolated (no pawns in adjacent files).
        /// </summary>
        public static bool IsIsolatedPawn(Board board, int pawnLocation, int color)
        {
            int file = GetFile(pawnLocation);
            return CountPawnsInFile(board, file - 1, color) == 0 &&
                   CountPawnsInFile(board, file + 1, color) == 0;
        }

        /// <summary>
        /// Checks if the king is exposed by verifying the openness of its file.
        /// </summary>
        public static bool IsKingExposed(Board board, int kingLocation)
        {
            int file = GetFile(kingLocation);
            return IsOpenFile(board, file);
        }

        /// <summary>
        /// Calculates the rank (row) number of a given square (0 to 7 for rows 1 to 8).
        /// </summary>
        /// <param name="square">The square index (0 to 63).</param>
        /// <returns>The rank (row) number (0 to 7).</returns>
        public static int GetRank(int square)
        {
            return GetRow(square);
        }

        /// <summary>
        /// Calculates the square index from a given rank and file.
        /// </summary>
        /// <param name="rank">The rank (row) number (0 to 7).</param>
        /// <param name="file">The file (column) number (0 to 7).</param>
        /// <returns>The square index (0 to 63).</returns>
        public static int GetSquare(int rank, int file)
        {
            return rank * 8 + file;
        }

        /// <summary>
        /// Finds the location of the king of the specified color.
        /// </summary>
        /// <param name="board">The board to analyze.</param>
        /// <param name="color">The color of the king to locate (Piece.White or Piece.Black).</param>
        /// <returns>The square index of the king (0 to 63) or -1 if not found.</returns>
        public static int KingLocation(Board board, int color)
        {
            for (int square = 0; square < board.Squares.Length; square++)
            {
                int piece = board.Squares[square];
                if (Piece.PieceType(piece) == Piece.King && Piece.Color(piece) == color)
                {
                    return square;
                }
            }
            return -1; // King not found
        }

        /// <summary>
        /// Finds the position of the duck on the board.
        /// </summary>
        public static int GetDuckPosition(Board board)
        {
            for (int square = 0; square < board.Squares.Length; square++)
            {
                int piece = board.Squares[square];
                if (Piece.PieceType(piece) == Piece.Duck)
                {
                    return square;
                }
            }
            return -1; // Duck not found
        }
    }
}
