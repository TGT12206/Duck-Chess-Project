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
            bool isIsolated = false;
            bool leftFileEmpty = false;
            bool rightFileEmpty = false;
            if (file > 0)
            {
                leftFileEmpty = CountPawnsInFile(board, file - 1, color) == 0;
            }
            else
            {
                leftFileEmpty = true;
            }
            if (file < 7)
            {
                rightFileEmpty = CountPawnsInFile(board, file + 1, color) == 0;
            } else
            {
                rightFileEmpty = true;
            }
            return leftFileEmpty && rightFileEmpty;
        }

        /// <summary>
        /// Checks if the king is exposed by verifying the openness of its file.
        /// </summary>
        public static bool IsKingExposed(Board board, int kingLocation)
        {
            int file = GetFile(kingLocation);
            return IsOpenFile(board, file);
        }
    }
}
