namespace DuckChess
{
    using System;
    using System.Collections.Generic;
    using Unity.VisualScripting;
    using UnityEngine;

    public class AIPlayer : RealTimePlayer
    {
        public override string Type { get { return "AIPlayer"; } }
        public override int Color { get; set; }

        private int maxDepth;
        private Board board;

        public AIPlayer(Board board, int color, int maxDepth)
        {
            this.board = board;
            this.Color = color;
            this.maxDepth = maxDepth;
        }

        public override void Update()
        {
            if (board.turnColor != Color || board.isGameOver)
                return;

            Move bestMove = GetBestMove();
            ChooseMove(bestMove);
        }

        private Move GetBestMove()
        {
            int alpha = int.MinValue;
            int beta = int.MaxValue;
            Move bestMove = Move.InvalidMove;
            int maxEval = int.MinValue;

            List<Move> legalMoves = GetAllLegalMoves(board);

            foreach (Move move in legalMoves)
            {
                Debug.Log("Evaluating Move: " + move.ToString());

                Board newBoard = board.Clone();
                newBoard.MakeMove(move);
                int eval = AlphaBeta(newBoard, maxDepth - 1, alpha, beta, false);

                if (eval > maxEval)
                {
                    maxEval = eval;
                    bestMove = move;
                }
                alpha = Math.Max(alpha, eval);
            }

            Debug.Log("AI Selected Move: " + bestMove.ToString());
            return bestMove;
        }


        private int AlphaBeta(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
        {
            if (depth == 0 || board.isGameOver)
            {
                return EvaluateBoard(board);
            }

            List<Move> legalMoves = GetAllLegalMoves(board);

            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;
                foreach (Move move in legalMoves)
                {
                    Board newBoard = board.Clone();
                    newBoard.MakeMove(move);
                    int eval = AlphaBeta(newBoard, depth - 1, alpha, beta, false);
                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha)
                        break;
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (Move move in legalMoves)
                {
                    Board newBoard = board.Clone();
                    newBoard.MakeMove(move);
                    int eval = AlphaBeta(newBoard, depth - 1, alpha, beta, true);
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha)
                        break;
                }
                return minEval;
            }
        }

        private int EvaluateBoard(Board board)
        {
            int evaluation = 0;

            // Piece values
            const int pawnValue = 100;
            const int knightValue = 320;
            const int bishopValue = 330;
            const int rookValue = 500;
            const int queenValue = 900;
            const int kingValue = 20000;

            // Piece-square tables
            int[] pawnTable = new int[64]
            {
        0,  5,  5, -10, -10,  5,  5,  0,
        0,  10, -5,  0,  0, -5, 10,  0,
        0,   0,  0, 20, 20,  0,  0,  0,
        5,   5, 10, 25, 25, 10,  5,  5,
        10, 10, 20, 30, 30, 20, 10, 10,
        20, 20, 30, 35, 35, 30, 20, 20,
        50, 50, 50, 50, 50, 50, 50, 50,
        0,   0,   0,   0,  0,   0,   0,   0
            };

            int[] knightTable = new int[64]
            {
        -50, -40, -30, -30, -30, -30, -40, -50,
        -40, -20,   0,   5,   5,   0, -20, -40,
        -30,   5,  10, 15, 15, 10,   5, -30,
        -30,   0, 15, 20, 20, 15,   0, -30,
        -30,   5, 15, 20, 20, 15,   5, -30,
        -30,   0, 10, 15, 15, 10,   0, -30,
        -40, -20,   0,   0,   0,   0, -20, -40,
        -50, -40, -30, -30, -30, -30, -40, -50,
            };

            int[] bishopTable = new int[64]
            {
        -20, -10, -10, -10, -10, -10, -10, -20,
        -10,   5,   0,   0,   0,   0,   5, -10,
        -10,  10, 10, 10, 10, 10, 10, -10,
        -10,   0, 10, 10, 10, 10,   0, -10,
        -10,   5,  5, 10, 10,  5,   5, -10,
        -10,   0,  5, 10, 10,  5,   0, -10,
        -10,   0,   0,   0,   0,   0,   0, -10,
        -20, -10, -10, -10, -10, -10, -10, -20,
            };

            int[] rookTable = new int[64]
            {
        0,   0,   5, 10, 10,   5,   0,   0,
        -5,   0,   0,   0,   0,   0,   0,  -5,
        -5,   0,   0,   0,   0,   0,   0,  -5,
        -5,   0,   0,   0,   0,   0,   0,  -5,
        -5,   0,   0,   0,   0,   0,   0,  -5,
        -5,   0,   0,   0,   0,   0,   0,  -5,
        5,  10, 10, 10, 10, 10, 10,   5,
        0,   0,   0,   5,  5,   0,   0,   0
            };

            int[] queenTable = new int[64]
            {
        -20, -10, -10,  -5,  -5, -10, -10, -20,
        -10,   0,   5,   0,   0,   0,   0, -10,
        -10,   5,   5,   5,   5,   5,   0, -10,
        0,   0,   5,   5,   5,   5,   0,  -5,
        -5,   0,   5,   5,   5,   5,   0,  -5,
        -10,   0,   5,   5,   5,   5,   0, -10,
        -10,   0,   0,   0,   0,   0,   0, -10,
        -20, -10, -10,  -5,  -5, -10, -10, -20
            };

            int[] kingTable = new int[64]
            {
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -20, -30, -30, -40, -40, -30, -30, -20,
        -10, -20, -20, -20, -20, -20, -20, -10,
        20,  20,   0,   0,   0,   0,  20,  20,
        20,  30,  10,   0,   0,  10,  30,  20
            };

            // Mobility, pawn structure, and king safety
            int mobility = 0;
            int pawnStructure = 0;
            int kingSafety = 0;

            for (int i = 0; i < 64; i++)
            {
                int piece = board[i];
                int pieceType = Piece.PieceType(piece);
                int pieceColor = Piece.Color(piece);
                int pieceValue = 0;
                int positionalValue = 0;

                switch (pieceType)
                {
                    case Piece.Pawn:
                        pieceValue = pawnValue;
                        positionalValue = pawnTable[i];
                        pawnStructure += EvaluatePawnStructure(board, i, pieceColor);
                        break;
                    case Piece.Knight:
                        pieceValue = knightValue;
                        positionalValue = knightTable[i];
                        break;
                    case Piece.Bishop:
                        pieceValue = bishopValue;
                        positionalValue = bishopTable[i];
                        break;
                    case Piece.Rook:
                        pieceValue = rookValue;
                        positionalValue = rookTable[i];
                        break;
                    case Piece.Queen:
                        pieceValue = queenValue;
                        positionalValue = queenTable[i];
                        break;
                    case Piece.King:
                        pieceValue = kingValue;
                        positionalValue = kingTable[i];
                        kingSafety += EvaluateKingSafety(board, i, pieceColor);
                        break;
                    default:
                        continue;
                }

                int sign = (pieceColor == this.Color) ? 1 : -1;

                evaluation += sign * (pieceValue + positionalValue);

                // Mobility
                if (pieceColor == this.Color)
                {
                    mobility += sign * GetPieceMobility(board, i, pieceType, pieceColor);
                }
            }

            evaluation += mobility * 10;        // Weight for mobility
            evaluation += pawnStructure * 10;   // Weight for pawn structure
            evaluation += kingSafety * 5;       // Weight for king safety

            return evaluation;
        }


        private List<Move> GetAllLegalMoves(Board board)
        {
            List<Move> legalMoves = new List<Move>();
            if (board.duckTurn)
            {
                LegalMoveGenerator.GenerateDuckMoves(ref legalMoves, board);
            }
            else
            {
                LegalMoveGenerator.GeneratePawnMoves(ref legalMoves, board);
                LegalMoveGenerator.GenerateKnightMoves(ref legalMoves, board);
                LegalMoveGenerator.GenerateBishopMoves(ref legalMoves, board);
                LegalMoveGenerator.GenerateRookMoves(ref legalMoves, board);
                LegalMoveGenerator.GenerateQueenMoves(ref legalMoves, board);
                LegalMoveGenerator.GenerateKingMoves(ref legalMoves, board);
            }
            return legalMoves;
        }

        private int EvaluatePawnStructure(Board board, int square, int color)
        {
            int score = 0;
            int row = Board.GetRowOf(square);
            int col = Board.GetColumnOf(square);

            // Check for doubled pawns
            for (int i = 0; i < 8; i++)
            {
                int index = i * 8 + col;
                if (index != square && Piece.PieceType(board[index]) == Piece.Pawn && Piece.Color(board[index]) == color)
                {
                    score -= 20; // Penalty for doubled pawns
                    break;
                }
            }

            // Check for isolated pawns
            bool hasNeighbor = false;
            if (col > 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    int index = i * 8 + (col - 1);
                    if (Piece.PieceType(board[index]) == Piece.Pawn && Piece.Color(board[index]) == color)
                    {
                        hasNeighbor = true;
                        break;
                    }
                }
            }
            if (col < 7 && !hasNeighbor)
            {
                for (int i = 0; i < 8; i++)
                {
                    int index = i * 8 + (col + 1);
                    if (Piece.PieceType(board[index]) == Piece.Pawn && Piece.Color(board[index]) == color)
                    {
                        hasNeighbor = true;
                        break;
                    }
                }
            }
            if (!hasNeighbor)
            {
                score -= 15; // Penalty for isolated pawns
            }

            return score;
        }

        private int EvaluateKingSafety(Board board, int square, int color)
        {
            int score = 0;
            int row = Board.GetRowOf(square);
            int col = Board.GetColumnOf(square);

            // Encourage castling
            if ((color == Piece.White && row >= 6) || (color == Piece.Black && row <= 1))
            {
                score += 20; // Bonus for castled king
            }
            else
            {
                score -= 20; // Penalty for uncastled king
            }

            // Check for pawn shield
            int direction = (color == Piece.White) ? -1 : 1;
            int pawnRow = row + direction;
            for (int i = -1; i <= 1; i++)
            {
                int file = col + i;
                if (file >= 0 && file < 8)
                {
                    int index = pawnRow * 8 + file;
                    if (index >= 0 && index < 64)
                    {
                        int piece = board[index];
                        if (Piece.PieceType(piece) == Piece.Pawn && Piece.Color(piece) == color)
                        {
                            score += 10; // Bonus for pawn shield
                        }
                    }
                }
            }

            return score;
        }

        private int GetPieceMobility(Board board, int square, int pieceType, int color)
        {
            List<Move> moves = new List<Move>();

            switch (pieceType)
            {
                case Piece.Knight:
                    LegalMoveGenerator.GenerateKnightMoves(ref moves, board);
                    break;
                case Piece.Bishop:
                    LegalMoveGenerator.GenerateBishopMoves(ref moves, board);
                    break;
                case Piece.Rook:
                    LegalMoveGenerator.GenerateRookMoves(ref moves, board);
                    break;
                case Piece.Queen:
                    LegalMoveGenerator.GenerateQueenMoves(ref moves, board);
                    break;
                default:
                    return 0;
            }

            int mobility = 0;
            foreach (var move in moves)
            {
                if (move.StartSquare == square)
                {
                    mobility++;
                }
            }

            return mobility;
        }

    }
}
