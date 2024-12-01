using System;
using System.Collections.Generic;

namespace DuckChess
{
    public class AlphaBetaAIPlayer : RealTimePlayer
    {
        public override string Type { get { return "AlphaBetaAIPlayer"; } }
        public override int Color { get; set; }

        private const int NumActionsPerFrame = 10000;

        private Board board;
        private int maxDepth;
        private Move bestMove;
        private bool startSearch;
        private List<Move> legalMoves;
        private Stack<AlphaBetaNode> alphaBetaNodes;
        private Stack<int> significantMoveCounters;
        private Board searchBoard;
        private int currentDepth;

        public AlphaBetaAIPlayer(Board board, int color, int maxDepth)
        {
            this.board = board;
            this.Color = color;
            this.maxDepth = maxDepth;
            startSearch = true;
        }

        public override void Update()
        {
            LookForBestMove();
        }

        private void LookForBestMove()
        {
            if (startSearch)
            {
                InitializeSearch();
            }

            for (int i = 0; i < NumActionsPerFrame; i++)
            {
                if (alphaBetaNodes.Count == 0)
                {
                    // Search is complete
                    ChooseMove(bestMove);
                    startSearch = true;
                    return;
                }

                AlphaBetaNode currentNode = alphaBetaNodes.Peek();

                if (currentDepth < maxDepth && currentNode.indexLeftOffAt < legalMoves.Count)
                {
                    if (currentNode.ShouldPrune())
                    {
                        CloseNode();
                    }
                    else
                    {
                        ExpandNode();
                    }
                }
                else
                {
                    CloseNode();
                }
            }
        }

        private void InitializeSearch()
        {
            bestMove = new Move();
            legalMoves = board.legalMoves;
            searchBoard = board.Clone();
            alphaBetaNodes = new Stack<AlphaBetaNode>();
            significantMoveCounters = new Stack<int>();
            alphaBetaNodes.Push(new AlphaBetaNode(int.MinValue, int.MaxValue, true, new Move()));
            currentDepth = 1;
            startSearch = false;
        }

        private void ExpandNode()
        {
            currentDepth++;
            AlphaBetaNode parent = alphaBetaNodes.Peek();
            int indexOfNextMove = parent.indexLeftOffAt;
            Move nextMove = legalMoves[indexOfNextMove];

            bool isMaximizing = searchBoard.duckTurn ? !parent.isMaximizing : parent.isMaximizing;
            AlphaBetaNode newNode = new AlphaBetaNode(parent.alpha, parent.beta, isMaximizing, nextMove);

            searchBoard.MakeMove(ref nextMove);
            legalMoves = searchBoard.legalMoves;
            significantMoveCounters.Push(searchBoard.numPlySinceLastEvent);
            alphaBetaNodes.Push(newNode);
        }

        private void CloseNode()
        {
            AlphaBetaNode node = alphaBetaNodes.Pop();
            currentDepth--;

            if (alphaBetaNodes.Count == 0)
            {
                // If we've returned to the root, end the search
                return;
            }

            AlphaBetaNode parent = alphaBetaNodes.Peek();
            searchBoard.UnmakeMove(node.moveFromParent, significantMoveCounters.Pop());
            legalMoves = searchBoard.legalMoves;

            if (currentDepth == maxDepth || searchBoard.isGameOver)
            {
                node.value = EvaluateBoard(searchBoard);
            }

            if (parent.JudgeNewValue(node.value, node.moveFromParent) && currentDepth == 1)
            {
                bestMove = node.moveFromParent;
            }

            parent.indexLeftOffAt++;
        }

        private int EvaluateBoard(Board board)
        {
            if (board.isGameOver)
            {
                bool isWhite = Color == Piece.White;
                return board.winnerColor switch
                {
                    Piece.NoColor => 0,
                    Piece.White => isWhite ? int.MaxValue : int.MinValue,
                    Piece.Black => isWhite ? int.MinValue : int.MaxValue,
                    _ => 0
                };
            }

            // Material evaluation
            int evaluation = 0;
            const int pawnValue = 100, knightValue = 320, bishopValue = 330, rookValue = 500, queenValue = 900, kingValue = 10000;

              PieceList allPieces = board.AllPieces;

            for (int i = 0; i < allPieces.Count; i++)
            {
               
                 int pieceLocation = allPieces[i];
                int piece = board[pieceLocation];
                int pieceType = Piece.PieceType(piece);
                int pieceColor = Piece.Color(piece);
                int pieceValue = pieceType switch
                {
                    Piece.Pawn => pawnValue,
                    Piece.Knight => knightValue,
                    Piece.Bishop => bishopValue,
                    Piece.Rook => rookValue,
                    Piece.Queen => queenValue,
                    Piece.King => kingValue,
                    _ => 0
                };

                if (pieceColor == this.Color)
                {
                    evaluation += pieceValue;
                }
                else
                {
                    evaluation -= pieceValue;
                }
            }

            return evaluation;
        }

        public override void UnmakeMove()
        {
            throw new NotImplementedException();
        }
    }

    // public class AlphaBetaNode
    // {
    //     public int alpha;
    //     public int beta;
    //     public bool isMaximizing;
    //     public int indexLeftOffAt;
    //     public Move moveFromParent;
        // public int value;

    //     public AlphaBetaNode(int alpha, int beta, bool isMaximizing, Move moveFromParent)
    //     {
    //         this.alpha = alpha;
    //         this.beta = beta;
    //         this.isMaximizing = isMaximizing;
    //         this.moveFromParent = moveFromParent;
    //         this.indexLeftOffAt = 0;
    //         this.value = isMaximizing ? int.MinValue : int.MaxValue;
    //     }

    //     public bool ShouldPrune()
    //     {
    //         return alpha >= beta;
    //     }

    //     public bool JudgeNewValue(int newValue, Move move)
    //     {
    //         if (isMaximizing && newValue > value)
    //         {
    //             value = newValue;
    //             moveFromParent = move;
    //             alpha = Math.Max(alpha, value);
    //             return true;
    //         }
    //         else if (!isMaximizing && newValue < value)
    //         {
    //             value = newValue;
    //             moveFromParent = move;
    //             beta = Math.Min(beta, value);
    //             return true;
    //         }
    //         return false;
    //     }
    // }
}
