namespace DuckChess
{

    public class BoardEvaluator
    {
        // Piece values (scaled to be more significant)
        private const int PawnValue = 1000;
        private const int KnightValue = 3200;
        private const int BishopValue = 3300;
        private const int RookValue = 5000;
        private const int QueenValue = 9000;
        private const int KingValue = 100000;
        private const int DuckValue = 5000; // Duck value: higher to reflect blocking and control

        // Penalty values for blocking our own pieces
        private const int OwnBlockPenalty = -200;

        // Bonus for controlling center squares
        private const int CenterBonus = 20;

        // Rewards for checks and valuable piece checks
        private const int CheckBonus = 50;
        private const int ValuableCheckBonus = 200;

        // Scaling factor for the overall evaluation
        private const int ScalingFactor = 10;

        private Board board;

        public BoardEvaluator(Board board)
        {
            this.board = board;
        }

        public int EvaluateBoard()
        {
            int totalScore = 0;

            // Evaluate material (pieces on the board)
            totalScore += EvaluateMaterial();

            // Evaluate the board's control over the center (for all pieces)
            totalScore += EvaluateCenterControl();

            // Evaluate for any checks or valuable checks
            totalScore += EvaluateChecks();

            // Evaluate any penalties for blocking own pieces
            totalScore += EvaluateOwnBlocks();

            return totalScore * ScalingFactor; // Apply the scaling factor to the total score
        }

        private int EvaluateMaterial()
        {
            int score = 0;

            // Loop through all squares on the board and evaluate the pieces
            foreach (int piece in board.AllPieces)
            {
                score += GetPieceValue(piece);
            }

            return score;
        }

        private int GetPieceValue(int piece)
        {
            switch (piece)
            {
                case Piece.Pawn:
                    return PawnValue;
                case Piece.Knight:
                    return KnightValue;
                case Piece.Bishop:
                    return BishopValue;
                case Piece.Rook:
                    return RookValue;
                case Piece.Queen:
                    return QueenValue;
                case Piece.King:
                    return KingValue;
                case Piece.Duck:
                    return DuckValue;
                default:
                    return 0;
            }
        }

        private int EvaluateCenterControl()
        {
            int score = 0;

            // The center squares are the most important, let's assign them a higher value
            int[] centerSquares = { 27, 28, 35, 36 }; // Example squares for center control

            foreach (int square in centerSquares)
            {
                int piece = board[square];
                if (piece != Piece.None)
                {
                    score += CenterBonus;
                }
            }

            return score;
        }

        private int EvaluateChecks()
        {
            int score = 0;

            // Check if the opponent is in check or if valuable pieces are in check
            foreach (var move in board.legalMoves)
            {
                if (move.IsCapture && move.CapturedPiece == Piece.King)
                {
                    score += CheckBonus; // Opponent's king is in check
                    if (Piece.PieceType(move.CapturedPiece) == Piece.Queen || Piece.PieceType(move.CapturedPiece) == Piece.Rook)
                    {
                        score += ValuableCheckBonus; // Valuable check on the opponent's pieces
                    }
                }
            }

            return score;
        }

        private int EvaluateOwnBlocks()
        {
            int score = 0;

            // Check if any pieces are blocking their own pieces (for the player)
            foreach (int piece in board.AllPieces)
            {
                if (piece != Piece.None && IsOwnPiece(piece))
                {
                    // Check surrounding squares and penalize for blocking
                    if (IsBlockingOwnPiece(i))
                    {
                        score += OwnBlockPenalty;
                    }
                }
            }

            return score;
        }

        private bool IsOwnPiece(int piece)
        {
            // Assuming you can determine if a piece belongs to the current player
            return board.IsPieceOwnedByCurrentPlayer(piece);
        }

        private bool IsBlockingOwnPiece(int square)
        {
            // Check if this square is blocking movement of an allied piece (simplified logic)
            // In a real implementation, this would involve checking the piece's legal moves
            // and seeing if it blocks other pieces from moving
            return false; // Placeholder, needs proper logic
        }
    }

}