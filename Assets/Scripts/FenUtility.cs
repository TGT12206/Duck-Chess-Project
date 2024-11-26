namespace DuckChess
{
    using System.Collections.Generic;
    public static class FenUtility
    {

        static Dictionary<char, int> pieceTypeFromSymbol = new Dictionary<char, int>()
        {
            ['k'] = Piece.King,
            ['p'] = Piece.Pawn,
            ['n'] = Piece.Knight,
            ['b'] = Piece.Bishop,
            ['r'] = Piece.Rook,
            ['q'] = Piece.Queen,
            ['d'] = Piece.Duck
        };

        public const string startFen = "rnbqkbnr/pppppppp/////PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        // Load position from fen string
        public static LoadedPositionInfo PositionFromFen(string fen)
        {

            LoadedPositionInfo loadedPositionInfo = new LoadedPositionInfo();
            string[] sections = fen.Split(' ');

            int file = 0;
            int rank = 7;

            // Interpret the first section, which contains all the piece locations
            foreach (char symbol in sections[0])
            {
                // Each '/' indicates a new row (rank)
                if (symbol == '/')
                {
                    // iterate to the next row (rank)
                    file = 0;
                    rank--;
                }
                else
                {
                    // A number indicates how many spaces to skip before placing a piece
                    if (char.IsDigit(symbol))
                    {
                        file += (int)char.GetNumericValue(symbol);
                    }
                    else
                    {
                        /**
                         * A letter indicates the piece to place down.
                         * Whether or not it is capitalized indicates the color.
                         * - uppercase = white
                         * - lowercase = black
                         * The dictionary "pieceTypeFromSymbol" maps the char to
                         * a piece
                         */
                        int pieceColor;
                        if (symbol == 'd' || symbol == 'D')
                        {
                            pieceColor = Piece.NoColor;
                        } else
                        {
                            pieceColor = (char.IsUpper(symbol)) ? Piece.White : Piece.Black;
                        }
                        int pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
                        loadedPositionInfo.squares[rank * 8 + file] = pieceType | pieceColor;
                        file++;
                    }
                }
            }

            // If the next section is 'w', it is white to move
            // If the next section is 'b', it is black to move
            loadedPositionInfo.whiteToMove = (sections[1] == "w");

            // Section 2 contains which directions the players can still castle
            string castlingRights = sections[2];
            loadedPositionInfo.whiteCastleKingside = castlingRights.Contains("K");
            loadedPositionInfo.whiteCastleQueenside = castlingRights.Contains("Q");
            loadedPositionInfo.blackCastleKingside = castlingRights.Contains("k");
            loadedPositionInfo.blackCastleQueenside = castlingRights.Contains("q");

            /**
             * Section 3 tells us where en passant is possible.
             * (even if no pawn can take, it is "possible" right after a pawn moves
             * There is only at most 1 possible position per turn where it is possible
             * '-' means no en passant is possible
             */
            if (sections.Length > 3)
            {
                // We actually only need the file to know if en passant is valid
                // The file is the first char of this section
                string enPassantFileName = sections[3][0].ToString();
                if (BoardAlgebraicNotation.fileNames.Contains(enPassantFileName))
                {
                    loadedPositionInfo.epFile = BoardAlgebraicNotation.fileNames.IndexOf(enPassantFileName) + 1;
                }
            }

            /**
             * Section 4 contains the half move clock
             * We use the half move clock for the 50 move countdown until a draw
             * A half move is one ply, or one player's turn
             */
            if (sections.Length > 4)
            {
                int.TryParse(sections[4], out loadedPositionInfo.plyCount);
            }

            /**
             * Section 5 contains the full move count of the game.
             * Not used atm, can be added later
             */
            return loadedPositionInfo;
        }

        //// Get the fen string of the current position
        //public static string CurrentFen(Board board)
        //{
        //    string fen = "";
        //    for (int rank = 7; rank >= 0; rank--)
        //    {
        //        int numEmptyFiles = 0;
        //        for (int file = 0; file < 8; file++)
        //        {
        //            int i = rank * 8 + file;
        //            int piece = board.Square[i];
        //            if (piece != 0)
        //            {
        //                if (numEmptyFiles != 0)
        //                {
        //                    fen += numEmptyFiles;
        //                    numEmptyFiles = 0;
        //                }
        //                bool isBlack = Piece.IsColour(piece, Piece.Black);
        //                int pieceType = Piece.PieceType(piece);
        //                char pieceChar = ' ';
        //                switch (pieceType)
        //                {
        //                    case Piece.Rook:
        //                        pieceChar = 'R';
        //                        break;
        //                    case Piece.Knight:
        //                        pieceChar = 'N';
        //                        break;
        //                    case Piece.Bishop:
        //                        pieceChar = 'B';
        //                        break;
        //                    case Piece.Queen:
        //                        pieceChar = 'Q';
        //                        break;
        //                    case Piece.King:
        //                        pieceChar = 'K';
        //                        break;
        //                    case Piece.Pawn:
        //                        pieceChar = 'P';
        //                        break;
        //                }
        //                fen += (isBlack) ? pieceChar.ToString().ToLower() : pieceChar.ToString();
        //            }
        //            else
        //            {
        //                numEmptyFiles++;
        //            }

        //        }
        //        if (numEmptyFiles != 0)
        //        {
        //            fen += numEmptyFiles;
        //        }
        //        if (rank != 0)
        //        {
        //            fen += '/';
        //        }
        //    }

        //    // Side to move
        //    fen += ' ';
        //    fen += (board.WhiteToMove) ? 'w' : 'b';

        //    // Castling
        //    bool whiteKingside = (board.currentGameState & 1) == 1;
        //    bool whiteQueenside = (board.currentGameState >> 1 & 1) == 1;
        //    bool blackKingside = (board.currentGameState >> 2 & 1) == 1;
        //    bool blackQueenside = (board.currentGameState >> 3 & 1) == 1;
        //    fen += ' ';
        //    fen += (whiteKingside) ? "K" : "";
        //    fen += (whiteQueenside) ? "Q" : "";
        //    fen += (blackKingside) ? "k" : "";
        //    fen += (blackQueenside) ? "q" : "";
        //    fen += ((board.currentGameState & 15) == 0) ? "-" : "";

        //    // En-passant
        //    fen += ' ';
        //    int epFile = (int)(board.currentGameState >> 4) & 15;
        //    if (epFile == 0)
        //    {
        //        fen += '-';
        //    }
        //    else
        //    {
        //        string fileName = BoardRepresentation.fileNames[epFile - 1].ToString();
        //        int epRank = (board.WhiteToMove) ? 6 : 3;
        //        fen += fileName + epRank;
        //    }

        //    // 50 move counter
        //    fen += ' ';
        //    fen += board.fiftyMoveCounter;

        //    // Full-move count (should be one at start, and increase after each move by black)
        //    fen += ' ';
        //    fen += (board.plyCount / 2) + 1;

        //    return fen;
        //}

        public class LoadedPositionInfo
        {
            public int[] squares;
            public bool whiteCastleKingside;
            public bool whiteCastleQueenside;
            public bool blackCastleKingside;
            public bool blackCastleQueenside;
            public int epFile;
            public bool whiteToMove;
            public int plyCount;

            public LoadedPositionInfo()
            {
                squares = new int[64];
            }
        }
    }
}