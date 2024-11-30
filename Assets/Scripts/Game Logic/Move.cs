using UnityEngine;

namespace DuckChess
{
    public readonly struct Move
    {

        /// <summary>
        /// Essentially an enum that stores whether the move was
        /// one that requires special functionality, and which type it is.
        /// </summary>
        public readonly struct Flag
        {
            /// <summary>
            /// Indicates that this move is not one of the special moves
            /// </summary>
            public const int None = 0;

            /// <summary>
            /// Indicates that this move is enpassant
            /// </summary>
            public const int EnPassantCapture = 1;

            /// <summary>
            /// Indicates that this move is castling
            /// </summary>
            public const int Castling = 2;

            /// <summary>
            /// Indicates that this move is a promotion to a queen
            /// </summary>
            public const int PromoteToQueen = 3;

            /// <summary>
            /// Indicates that this move is a promotion to a knight
            /// </summary>
            public const int PromoteToKnight = 4;

            /// <summary>
            /// Indicates that this move is a promotion to a rook
            /// </summary>
            public const int PromoteToRook = 5;

            /// <summary>
            /// Indicates that this move is a promotion to a bishop
            /// </summary>
            public const int PromoteToBishop = 6;

            /// <summary>
            /// Indicates that this move is one where a pawn moves two spaces forward.
            /// </summary>
            public const int PawnTwoForward = 7;

            /// <summary>
            /// Indicates that this move is the first time a duck is being placed.
            /// </summary>
            public const int FirstDuckMove = 8;
        }

        readonly int moveValue;

        const int startSquareMask =   0b0000000000000000111111;
        const int targetSquareMask =  0b0000000000111111000000;
        const int flagMask =          0b0000001111000000000000;
        const int capturedPieceMask = 0b1111110000000000000000;

        //public Move(int moveValue)
        //{
        //    this.moveValue = moveValue;
        //}

        /// <summary>
        /// Creates a move that simply moves a piece from start to target
        /// </summary>
        /// <param name="startSquare">The index of the start square</param>
        /// <param name="targetSquare">The index of the target square</param>
        public Move(int startSquare, int targetSquare)
        {
            moveValue = (int) (startSquare | targetSquare << 6);
        }

        /// <summary>
        /// Creates a move that moves a piece from start to target, but also
        /// is one of the special moves, indicated by the flag.
        /// </summary>
        /// <param name="startSquare">The index of the start square</param>
        /// <param name="targetSquare">The index of the target square</param>
        /// <param name="flag">Which special move it is</param>
        public Move(int startSquare, int targetSquare, int flag)
        {
            moveValue = startSquare | targetSquare << 6 | flag << 12;
        }

        /// <summary>
        /// Use to add a captured piece to a move. Note that this must be a normal capture,
        /// not en passant.
        /// Creates a copy of the given move, but with the data of the captured piece.
        /// </summary>
        /// <param name="move">The move to add the captured piece to.
        /// This move must not have a record of a captured piece already</param>
        /// <param name="capturedPiece">The piece that was captured by this move</param>
        public Move(Move move, int capturedPiece)
        {
            moveValue = move.moveValue | capturedPiece << 16;
        }

        /// <summary>
        /// The starting square of this move
        /// </summary>
        public int StartSquare
        {
            get
            {
                return moveValue & startSquareMask;
            }
        }

        /// <summary>
        /// The target square of this move
        /// </summary>
        public int TargetSquare
        {
            get
            {
                return (moveValue & targetSquareMask) >> 6;
            }
        }

        /// <summary>
        /// The piece that was captured by this move
        /// </summary>
        public int CapturedPiece
        {
            get
            {
                return (moveValue & capturedPieceMask) >> 16;
            }
        }

        public bool IsCapture
        {
            get
            {
                return Piece.PieceType(CapturedPiece) != Piece.None || MoveFlag == Flag.EnPassantCapture;
            }
        }

        /// <summary>
        /// Whether or not this move is a pawn promotion
        /// </summary>
        public bool IsPromotion
        {
            get
            {
                int flag = MoveFlag;
                return flag == Flag.PromoteToQueen || flag == Flag.PromoteToRook || flag == Flag.PromoteToKnight || flag == Flag.PromoteToBishop;
            }
        }

        /// <summary>
        /// Indicates whether the move is one of a few special moves.
        /// <br></br>
        /// Check the enum Move.Flag for the possible values of the flag.
        /// </summary>
        public int MoveFlag
        {
            get
            {
                return moveValue >> 12;
            }
        }

        /// <summary>
        /// What piece the pawn is promoting to.
        /// </summary>
        public int PromotionPieceType
        {
            get
            {
                switch (MoveFlag)
                {
                    case Flag.PromoteToRook:
                        return Piece.Rook;
                    case Flag.PromoteToKnight:
                        return Piece.Knight;
                    case Flag.PromoteToBishop:
                        return Piece.Bishop;
                    case Flag.PromoteToQueen:
                        return Piece.Queen;
                    default:
                        return Piece.None;
                }
            }
        }

        //public static Move InvalidMove
        //{
        //    get
        //    {
        //        return new Move(0);
        //    }
        //}

        /// <summary>
        /// Whether or not two moves are equal in value
        /// </summary>
        public static bool SameMove(Move a, Move b)
        {
            return a.moveValue == b.moveValue;
        }

        //public int Value
        //{
        //    get
        //    {
        //        return moveValue;
        //    }
        //}

        //public bool IsInvalid
        //{
        //    get
        //    {
        //        return moveValue == 0;
        //    }
        //}

        /// <summary>
        /// Returns this move as a formatted string
        /// </summary>
        public override string ToString()
        {
            string moveString = "";
            moveString += StartSquare + " to " + TargetSquare + " flag: " + MoveFlag;
            return moveString;
        }

        //public string Name
        //{
        //    get
        //    {
        //        return BoardRepresentation.SquareNameFromIndex(StartSquare) + "-" + BoardRepresentation.SquareNameFromIndex(TargetSquare);
        //    }
        //}
    }
}