using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using Unity.VisualScripting;
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
        /// The ply where the king side rook moved or the king moved
        /// </summary>
        private int PlyWhereLostKingSideCastleW;

        /// <summary>
        /// Whether queenside castling as white is still possible
        /// </summary>
        public bool CastleQueenSideW;

        /// <summary>
        /// The ply where the queen side rook moved or the king moved
        /// </summary>
        private int PlyWhereLostQueenSideCastleW;

        /// <summary>
        /// Whether kingside castling as black is still possible
        /// </summary>
        public bool CastleKingSideB;

        /// <summary>
        /// The ply where the king side rook moved or the king moved
        /// </summary>
        private int PlyWhereLostKingSideCastleB;

        /// <summary>
        /// Whether queenside castling as black is still possible
        /// </summary>
        public bool CastleQueenSideB;

        /// <summary>
        /// The ply where the queen side rook moved or the king moved
        /// </summary>
        private int PlyWhereLostQueenSideCastleB;
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

        public int plyCount;

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
        /// made in this position and turn.
        /// </summary>
        public List<Move> legalMoves;
        #endregion

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
            plyCount = 0;
            PlyWhereLostKingSideCastleB = NOT_ON_BOARD;
            PlyWhereLostKingSideCastleW = NOT_ON_BOARD;
            PlyWhereLostQueenSideCastleB = NOT_ON_BOARD;
            PlyWhereLostQueenSideCastleW = NOT_ON_BOARD;
        }

        private void ResetLegalMoves()
        {
            legalMoves = new List<Move>();
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
            plyCount = 0;
            PlyWhereLostKingSideCastleB = NOT_ON_BOARD;
            PlyWhereLostKingSideCastleW = NOT_ON_BOARD;
            PlyWhereLostQueenSideCastleB = NOT_ON_BOARD;
            PlyWhereLostQueenSideCastleW = NOT_ON_BOARD;

            // Set the duck
            Duck = NOT_ON_BOARD;
            GenerateNormalMoves();
        }

        /// <summary>
        /// Make the given move on the board.
        /// If it is a normal capture, the board will also add the captured
        /// piece to the move.
        /// </summary>
        /// <param name="move">The move to make</param>
        public void MakeMove(ref Move move)
        {
            /*
             * Checking pawn moves (en passant, two forward, promotion)
             * Check bishop moves
             * Check knight moves
             * Check rook moves (castling)
             * Check king moves (castling)
             * Check duck moves
             */
            bool isWhite = turnColor == Piece.White;
            MovePieceInPieceLists(move, isWhite);
            MovePieceInSquares(move, isWhite);

            SwitchToNextTurn();
        }

        private void MovePieceInPieceLists(Move move, bool isWhite)
        {
            int startSquare = move.StartSquare;
            int targetSquare = move.TargetSquare;
            // If this is the first duck move, then the piece at the
            // "start" square is actually the rook. Therefore, none of
            // The following code works, and we need to return asap.
            if (move.MoveFlag == Move.Flag.FirstDuckMove)
            {
                Duck = targetSquare;
                return;
            }
            int pieceToMove = Squares[startSquare];
            bool isPawn = false;
            bool isRook = false;
            bool isCapture = move.IsCapture;
            PieceList pieceListToUpdate = null;
            #region Find the piece list of the moved piece
            switch (Piece.PieceType(pieceToMove))
            {
                case Piece.Pawn:
                    pieceListToUpdate = isWhite ? WhitePawns : BlackPawns;
                    isPawn = true;
                    break;
                case Piece.Knight:
                    pieceListToUpdate = isWhite ? WhiteKnights : BlackKnights;
                    break;
                case Piece.Bishop:
                    pieceListToUpdate = isWhite ? WhiteBishops : BlackBishops;
                    break;
                case Piece.Rook:
                    pieceListToUpdate = isWhite ? WhiteRooks : BlackRooks;
                    isRook = true;
                    break;
                case Piece.Queen:
                    pieceListToUpdate = isWhite ? WhiteQueens : BlackQueens;
                    break;
            }
            #endregion

            // If it is null, it must be a duck or a king
            if (pieceListToUpdate == null)
            {
                // Must be a king or a duck
                if (Piece.PieceType(pieceToMove) == Piece.Duck)
                {
                    // Duck
                    Duck = targetSquare;
                }
                else
                {
                    //// King
                    //int kingSpot = isWhite ? ref WhiteKing : ref BlackKing;
                    //kingSpot = targetSquare;
                    //bool kingSideCastle = isWhite ? ref CastleKingSideW : ref CastleKingSideB;
                    //bool queenSideCastle = isWhite ? ref CastleQueenSideW : ref CastleQueenSideB;

                    //// Can't castle after moving king
                    //kingSideCastle = false;
                    //queenSideCastle = false;

                    //// Castle
                    //if (move.MoveFlag == Move.Flag.Castling)
                    //{
                    //    bool isKingSide = move.TargetSquare - move.StartSquare > 0;

                    //    int rookSpot = move.StartSquare;
                    //    rookSpot += isKingSide ? 3 : -4;

                    //    int newRookSpot = move.StartSquare;
                    //    newRookSpot += isKingSide ? 1 : -1;

                    //    PieceList friendlyRooks = isWhite ? WhiteRooks : BlackRooks;

                    //    Move moveRook = new Move(rookSpot, newRookSpot);
                    //    friendlyRooks.MovePiece(moveRook);
                    //}
                }
            }
            else
            {
                pieceListToUpdate.MovePiece(move);
                if (isPawn)
                {
                    // En Passant
                    if (move.MoveFlag == Move.Flag.EnPassantCapture)
                    {
                        EnPassantInPieceList(move, isWhite);
                    }
                    // Promotion
                    if (move.IsPromotion)
                    {
                        PromoteInPieceList(move, isWhite, pieceListToUpdate);
                    }
                    // Two forward
                    if (move.MoveFlag == Move.Flag.PawnTwoForward)
                    {
                        enPassantSquare = targetSquare;
                    }
                }
                // If a rook is moving for the first time, disable castling on that side
                //if (isRook)
                //{
                //    int startRowOfRook = isWhite ? 0 : 7;
                //    bool isKingSideRook = GetColumnOf(startSquare) == 7 && GetRowOf(startSquare) == startRowOfRook;
                //    bool isQueenSideRook = GetColumnOf(startSquare) == 0 && GetRowOf(startSquare) == startRowOfRook;
                //    bool kingSideCastle = isWhite ? ref CastleKingSideW : ref CastleKingSideB;
                //    bool queenSideCastle = isWhite ? ref CastleQueenSideW : ref CastleQueenSideB;
                //    if (isKingSideRook && kingSideCastle)
                //    {
                //        kingSideCastle = false;
                //    } else if (isQueenSideRook && queenSideCastle)
                //    {
                //        queenSideCastle = false;
                //    }
                //}
            }
            if (!isPawn && !isCapture)
            {
                // If not a pawn move or a capture
                numPlySinceLastEvent++;
            }
            else
            {
                // Reset the draw counter
                numPlySinceLastEvent = 0;
            }
            if (isCapture)
            {
                bool isRookCaptured = false;
                int capturedPiece = Squares[targetSquare];
                PieceList capturedPieceList = null;
                switch (Piece.PieceType(capturedPiece))
                {
                    case Piece.Pawn:
                        capturedPieceList = isWhite ? WhitePawns : BlackPawns;
                        break;
                    case Piece.Knight:
                        capturedPieceList = isWhite ? WhiteKnights : BlackKnights;
                        break;
                    case Piece.Bishop:
                        capturedPieceList = isWhite ? WhiteBishops : BlackBishops;
                        break;
                    case Piece.Rook:
                        capturedPieceList = isWhite ? WhiteRooks : BlackRooks;
                        isRookCaptured = true;
                        break;
                    case Piece.Queen:
                        capturedPieceList = isWhite ? WhiteQueens : BlackQueens;
                        break;
                }
                if (capturedPieceList == null)
                {
                    // Must be a king that was captured
                    isGameOver = true;
                    winnerColor = turnColor;
                    int kingSpot = isWhite ? ref WhiteKing : ref BlackKing;
                    kingSpot = NOT_ON_BOARD;
                }
                capturedPieceList.RemovePieceAtSquare(targetSquare);
                //if (isRookCaptured)
                //{
                //    // If we capture an enemy rook, disable enemy castling on that side
                //    int startRowOfEnemyRook = isWhite ? 7 : 0;
                //    bool isKingSideRook = GetColumnOf(targetSquare) == 7 && GetRowOf(targetSquare) == startRowOfEnemyRook;
                //    bool isQueenSideRook = GetColumnOf(targetSquare) == 0 && GetRowOf(targetSquare) == startRowOfEnemyRook;
                //    bool enemyKingSideCastle = isWhite ? ref CastleKingSideB : ref CastleKingSideW;
                //    bool enemyQueenSideCastle = isWhite ? ref CastleQueenSideB : ref CastleQueenSideW;
                //    if (isKingSideRook && enemyKingSideCastle)
                //    {
                //        enemyKingSideCastle = false;
                //    }
                //    else if (isQueenSideRook && enemyQueenSideCastle)
                //    {
                //        enemyQueenSideCastle = false;
                //    }
                //}
            }
        }
        private void MovePieceInSquares(Move move, bool isWhite)
        {
            // If this is the first duck move, then the piece at the
            // "start" square is actually the rook. Therefore, some of
            // The following code will break, and we need to return asap.
            if (move.MoveFlag == Move.Flag.FirstDuckMove)
            {
                Squares[move.TargetSquare] = Piece.Duck;
                return;
            }

            // Handles both normal captures and moving the piece to the new square
            Squares[move.TargetSquare] = Squares[move.StartSquare];
            Squares[move.StartSquare] = Piece.None;

            // Special cases

            // Castle
            //if (move.MoveFlag == Move.Flag.Castling)
            //{
            //    bool isKingSide = move.TargetSquare - move.StartSquare > 0;

            //    int rookSpot = move.StartSquare;
            //    rookSpot += isKingSide ? 3 : -4;

            //    int newRookSpot = move.StartSquare;
            //    newRookSpot += isKingSide ? 1 : -1;

            //    PieceList friendlyRooks = isWhite ? WhiteRooks : BlackRooks;

            //    Squares[newRookSpot] = Squares[rookSpot];
            //    Squares[rookSpot] = Piece.None;
            //}

            // If en passant, delete the enemy pawn
            if (move.MoveFlag == Move.Flag.EnPassantCapture)
            {
                int enemyPawnSquare = isWhite ? -8 : 8;
                enemyPawnSquare += move.TargetSquare;
                Squares[enemyPawnSquare] = Piece.None;
                return;
            }

            // If promotion, replace the pawn with the new piece
            if (move.IsPromotion)
            {
                Squares[move.TargetSquare] = move.PromotionPieceType;
                return;
            }
        }
        private void SwitchToNextTurn()
        {
            plyCount++;
            ResetLegalMoves();
            if (numPlySinceLastEvent >= 200)
            {
                isGameOver = true;
                winnerColor = Piece.NoColor;
            }
            duckTurn = !duckTurn;
            if (duckTurn)
            {
                GenerateDuckMoves();
            }
            else
            {
                turnColor = turnColor == Piece.White ? Piece.Black : Piece.White;
                GenerateNormalMoves();
            }
        }

        /// <summary>
        /// Undo the given move
        /// </summary>
        public void UnmakeMove(Move move, int drawCounterOfLastMove)
        {
            /**
             * move the piece back to the original square
             * - update the piecelist of the unmoved piece
             * - update the piecelist of the enemy piece
             * - swap the target and start squares on the board
             * replace the piece that was captured
             */
            bool isWhite = turnColor == Piece.White;
            int currentSquare = move.TargetSquare;
            int originalSquare = move.StartSquare;
            int piece = Squares[currentSquare];
            int capturedPiece = move.CapturedPiece;
            PieceList pieceListToUpdate = null;

            // Update the piece lists
            #region select the piece list corresponding to the piece type and color
            switch (Piece.PieceType(piece))
            {
                case Piece.Pawn:
                    pieceListToUpdate = isWhite ? WhitePawns : BlackPawns;
                    break;
                case Piece.Knight:
                    pieceListToUpdate = isWhite ? WhiteKnights : BlackKnights;
                    break;
                case Piece.Bishop:
                    pieceListToUpdate = isWhite ? WhiteBishops : BlackBishops;
                    break;
                case Piece.Rook:
                    pieceListToUpdate = isWhite ? WhiteRooks : BlackRooks;
                    break;
                case Piece.Queen:
                    pieceListToUpdate = isWhite ? WhiteQueens : BlackQueens;
                    break;
            }
            #endregion

            # region If no piece list was selected, it is a king or a duck
            if (pieceListToUpdate == null)
            {
                if (Piece.PieceType(piece) == Piece.Duck)
                {
                    Duck = originalSquare;
                    if (plyCount == 2)
                    {
                        Duck = NOT_ON_BOARD;
                    } else
                    {
                        Squares[originalSquare] = Squares[currentSquare];
                    }
                    Squares[currentSquare] = Piece.None;
                    return;
                }
                //else
                //{
                //    int kingSpot = isWhite ? ref WhiteKing : ref BlackKing;
                //    if (move.MoveFlag == Move.Flag.Castling)
                //    {
                //        bool wasKingSide = GetColumnOf(kingSpot) == 6;

                //        // The location of the rook relative to the king
                //        int rookSpot = wasKingSide ? -1 : 1;
                //        int originalRookSpot = wasKingSide ? 1 : -2;

                //        // The actual location of the rook
                //        rookSpot += kingSpot;
                //        originalRookSpot += kingSpot;

                //        PieceList rookPieceList = isWhite ? WhiteRooks : BlackRooks;
                //        rookPieceList.UnmovePiece(new Move(originalRookSpot, rookSpot));

                //        // Update squares as well
                //        Squares[originalRookSpot] = Squares[rookSpot];
                //        Squares[rookSpot] = Piece.None;
                //    }
                //    int plyWhereLostKingSideCastle = isWhite ?
                //        ref PlyWhereLostKingSideCastleW :
                //        ref PlyWhereLostKingSideCastleB;
                //    int plyWhereLostQueenSideCastle = isWhite ?
                //        ref PlyWhereLostQueenSideCastleW :
                //        ref PlyWhereLostQueenSideCastleB;
                //    bool CastleKingSide = isWhite ? ref CastleKingSideW : ref CastleKingSideB;
                //    bool CastleQueenSide = isWhite ? ref CastleQueenSideW : ref CastleQueenSideB;
                //    kingSpot = originalSquare;
                //    if (plyCount == plyWhereLostKingSideCastle)
                //    {
                //        CastleKingSide = true;
                //        plyWhereLostKingSideCastle = NOT_ON_BOARD;
                //    }
                //    if (plyCount == plyWhereLostQueenSideCastle)
                //    {
                //        CastleQueenSide = true;
                //        plyWhereLostQueenSideCastle = NOT_ON_BOARD;
                //    }
                //}
            }
            #endregion
            else
            #region Otherwise, update the piece list
            {
                if (move.MoveFlag == Move.Flag.EnPassantCapture)
                {
                    PieceList enemyPawnList = isWhite ? BlackPawns : WhitePawns;
                    int capturedPawnSpot = isWhite ? -1 : 1;
                    capturedPawnSpot += currentSquare;
                    enemyPawnList.AddPieceAtSquare(capturedPawnSpot);
                } else if (move.IsPromotion)
                {
                    Debug.Log("IsPromotion");
                    PieceList pawnList = isWhite ? WhitePawns : BlackPawns;
                    pieceListToUpdate.RemovePieceAtSquare(currentSquare);
                    pawnList.AddPieceAtSquare(currentSquare);
                    pieceListToUpdate = pawnList;
                }
                pieceListToUpdate.UnmovePiece(move);
            }
            #endregion

            #region Check if this move was a normal capture
            if (Piece.PieceType(capturedPiece) != Piece.None)
            {
                PieceList capturedPieceList = null;
                switch (Piece.PieceType(capturedPiece))
                {
                    case Piece.Pawn:
                        capturedPieceList = isWhite ? BlackPawns : WhitePawns;
                        break;
                    case Piece.Knight:
                        capturedPieceList = isWhite ? BlackKnights : WhiteKnights;
                        break;
                    case Piece.Bishop:
                        capturedPieceList = isWhite ? BlackBishops : WhiteBishops;
                        break;
                    case Piece.Rook:
                        capturedPieceList = isWhite ? BlackRooks : WhiteRooks;
                        break;
                    case Piece.Queen:
                        capturedPieceList = isWhite ? BlackQueens : WhiteQueens;
                        break;
                }
                if (capturedPieceList != null)
                {
                    capturedPieceList.AddPieceAtSquare(currentSquare);
                } else
                {
                    // Must be a king
                    int enemyKing = isWhite ? ref BlackKing : ref WhiteKing;
                    enemyKing = currentSquare;
                    isGameOver = false;
                    winnerColor = Piece.NoColor;
                }
            }
            #endregion

            // Update the squares
            // Not the first duck move
            if (plyCount != 2)
            {
                Squares[originalSquare] = Squares[currentSquare];
            }
            Squares[currentSquare] = move.CapturedPiece;

            // Update the turn info
            plyCount--;
            duckTurn = !duckTurn;
            ResetLegalMoves();
            if (duckTurn)
            {
                turnColor = isWhite ? Piece.Black : Piece.White;
                GenerateDuckMoves();
            } else
            {
                GenerateNormalMoves();
            }
        }

        private void EnPassantInPieceList(Move move, bool isWhite)
        {
            PieceList enemyPawns = isWhite ? BlackPawns : WhitePawns;
            int enemyPawnSquare = isWhite ? -8 : 8;
            enemyPawnSquare += move.TargetSquare;
            enemyPawns.RemovePieceAtSquare(enemyPawnSquare);
        }
        private void PromoteInPieceList(Move move, bool isWhite, PieceList friendlyPawns)
        {
            // The pawn should already be at the target square
            // Remove it so we can place the new piece there
            friendlyPawns.RemovePieceAtSquare(move.TargetSquare);
            PieceList promotionPieceList = null;
            #region Select the piece list of the promoted piece type
            switch (move.PromotionPieceType)
            {
                case Piece.Knight:
                    promotionPieceList = isWhite ? WhiteKnights : BlackKnights;
                    break;
                case Piece.Bishop:
                    promotionPieceList = isWhite ? WhiteBishops : BlackBishops;
                    break;
                case Piece.Rook:
                    promotionPieceList = isWhite ? WhiteRooks : BlackRooks;
                    break;
                case Piece.Queen:
                    promotionPieceList = isWhite ? WhiteQueens : BlackQueens;
                    break;
            }
            #endregion

            promotionPieceList.AddPieceAtSquare(move.TargetSquare);
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

        private void MoveKing(Move move, bool isWhite)
        {
            if (isWhite)
            {
                WhiteKing = move.TargetSquare;
                if (CastleKingSideW)
                {
                    CastleKingSideW = false;
                    PlyWhereLostKingSideCastleW = plyCount;
                }
                if (CastleQueenSideW)
                {
                    CastleQueenSideW = false;
                    PlyWhereLostQueenSideCastleW = plyCount;
                }
            } else
            {
                BlackKing = move.TargetSquare;
                if (CastleKingSideB)
                {
                    CastleKingSideB = false;
                    PlyWhereLostKingSideCastleB = plyCount;
                }
                if (CastleQueenSideB)
                {
                    CastleQueenSideB = false;
                    PlyWhereLostQueenSideCastleB = plyCount;
                }
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
                //SwapSquares(moveRook);
            }
            enPassantSquare = NOT_ON_BOARD;
            numPlySinceLastEvent++;
        }

        private void GenerateNormalMoves()
        {
            LegalMoveGenerator.GeneratePawnMoves(ref legalMoves, this);
            //LegalMoveGenerator.GenerateKnightMoves(ref legalMoves, this);
            //LegalMoveGenerator.GenerateBishopMoves(ref legalMoves, this);
            //LegalMoveGenerator.GenerateRookMoves(ref legalMoves, this);
            //LegalMoveGenerator.GenerateQueenMoves(ref legalMoves, this);
            //LegalMoveGenerator.GenerateKingMoves(ref legalMoves, this);
        }

        private void GenerateDuckMoves()
        {
            LegalMoveGenerator.GenerateDuckMoves(ref legalMoves, this);
        }

        /// <summary>
        /// Given a move, check whether or not it is legal in this position.
        /// </summary>
        /// <param name="move">The move to check</param>
        public bool IsMoveLegal(ref Move move)
        {
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
                            pieceTypeChar = "  D";
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
        public Board Clone()
        {
            Board copy = new Board();
            copy.Squares = (int[])this.Squares.Clone();
            copy.turnColor = this.turnColor;
            copy.duckTurn = this.duckTurn;
            copy.enPassantSquare = this.enPassantSquare;
            copy.CastleKingSideW = this.CastleKingSideW;
            copy.CastleQueenSideW = this.CastleQueenSideW;
            copy.CastleKingSideB = this.CastleKingSideB;
            copy.CastleQueenSideB = this.CastleQueenSideB;
            copy.PlyWhereLostKingSideCastleW = this.PlyWhereLostKingSideCastleW;
            copy.PlyWhereLostKingSideCastleB = this.PlyWhereLostKingSideCastleB;
            copy.PlyWhereLostQueenSideCastleW = this.PlyWhereLostQueenSideCastleW;
            copy.PlyWhereLostQueenSideCastleB = this.PlyWhereLostQueenSideCastleB;
            copy.winnerColor = this.winnerColor;
            copy.isGameOver = this.isGameOver;
            copy.plyCount = this.plyCount;
            copy.numPlySinceLastEvent = this.numPlySinceLastEvent;

            copy.legalMoves = this.legalMoves;

            // Clone piece lists
            copy.WhitePawns = this.WhitePawns.Clone();
            copy.BlackPawns = this.BlackPawns.Clone();
            copy.WhiteKnights = this.WhiteKnights.Clone();
            copy.BlackKnights = this.BlackKnights.Clone();
            copy.WhiteBishops = this.WhiteBishops.Clone();
            copy.BlackBishops = this.BlackBishops.Clone();
            copy.WhiteRooks = this.WhiteRooks.Clone();
            copy.BlackRooks = this.BlackRooks.Clone();
            copy.WhiteQueens = this.WhiteQueens.Clone();
            copy.BlackQueens = this.BlackQueens.Clone();
            copy.WhiteKing = this.WhiteKing;
            copy.BlackKing = this.BlackKing;
            copy.Duck = this.Duck;

            return copy;
        }

    }
}