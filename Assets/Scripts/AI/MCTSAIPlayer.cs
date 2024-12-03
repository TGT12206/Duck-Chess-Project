namespace DuckChess
{
    using System;
    using System.Collections.Generic;
    using Unity.VisualScripting;
    using UnityEngine;

    public class MCTSAIPlayer : RealTimePlayer
    {
        public override string Type { get { return "AIPlayerMCTS"; } }
        public override int Color { get; set; }

        private const int NumActionsPerFrame = 1;

        private Board board;
        private Node rootNode;
        private Node currentNode;
        private int simulationCount;
        private int fullNodeMaxDepth;
        private int simulationMaxDepth;

        public MCTSAIPlayer(Board board, int color, int fullNodeMaxDepth, int simulationMaxDepth)
        {
            this.board = board;
            this.Color = color;
            this.fullNodeMaxDepth = fullNodeMaxDepth;
            this.simulationMaxDepth = simulationMaxDepth;
        }

        public override void Update()
        {
            LookForBestMove();
        }

        private void InitializeSearch()
        {
            rootNode = new Node(new Board(board), fullNodeMaxDepth, simulationMaxDepth);
        }

        private Move LookForBestMove()
        {
            for (int i = 0; i < NumActionsPerFrame; i++)
            {
                // Selection
                while (node.IsFullyExpanded && !node.IsTerminal)
                {
                    node = node.GetBestChild();
                }

                // Expansion
                if (!node.IsTerminal)
                {
                    node = node.Expand();
                }

                // Simulation
                int result = node.Simulate();

                // Backpropagation
                node.Backpropagate(result);

                Node bestChild = rootNode.GetChildWithMostVisits();
            }

            if (bestChild != null && bestChild.Move != Move.InvalidMove)
            {
                Debug.Log("AI Selected Move: " + bestChild.Move.ToString());
                return bestChild.Move;
            }
            else
            {
                // If no move found, pick a random legal move
                List<Move> legalMoves = GetAllLegalMoves(board);
                return legalMoves[UnityEngine.Random.Range(0, legalMoves.Count)];
            }
        }
    }

    public class Node
    {
        private int fullNodeMaxDepth;
        public int simulationMaxDepth;
        public int nodeDepth;
        
        public Node Parent;
        public List<Node> Children;
        public Move MoveFromParent;
        public Board BoardState;
        public int Visits;
        public float Wins;
        public bool IsFullyExpanded;
        public bool IsTerminal;

        private List<Move> untriedMoves;
        private System.Random random = new System.Random();

        /// <summary>
        /// Used to construct a root node
        /// </summary>
        public Node(Board boardState, int fullNodeMaxDepth, int simulationMaxDepth)
        {
            Parent = null;
            this.simulationMaxDepth = simulationMaxDepth;
            nodeDepth = 0;
            BoardState = boardState;
            Visits = 0;
            Wins = 0;
        }
        public Node(Node parent, Move move, Board boardState, int fullNodeMaxDepth, int simulationMaxDepth)
        {
            Parent = parent;
            MoveFromParent = move;
            BoardState = boardState;
            Visits = 0;
            Wins = 0;
            Children = new List<Node>();
            nodeDepth = Parent.nodeDepth + 1;
            IsTerminal = boardState.isGameOver || nodeDepth == fullNodeMaxDepth;
            if (!IsTerminal)
            {
                untriedMoves = boardState.legalMoves;
                IsFullyExpanded = untriedMoves.Count == 0;
            }
            else
            {
                untriedMoves = new List<Move>();
                IsFullyExpanded = true;
            }
        }

        public Node Expand()
        {
            Move move = untriedMoves[0];
            untriedMoves.RemoveAt(0);

            Board newBoard = new Board(BoardState);
            newBoard.MakeMove(ref move);

            Node childNode = new Node(this, move, newBoard, fullNodeMaxDepth, simulationMaxDepth);
            Children.Add(childNode);

            if (untriedMoves.Count == 0)
            {
                IsFullyExpanded = true;
            }

            return childNode;
        }

        public int Simulate()
        {
            Board simulationBoard = new Board(BoardState);
            int currentPlayer = simulationBoard.turnColor;

            for (int i = 0; !(simulationMaxDepth < i || simulationBoard.isGameOver); i++)
            {
                List<Move> legalMoves = simulationBoard.legalMoves;
                if (legalMoves.Count == 0)
                    break;

                Move selectedMove = GetHeuristicMove(simulationBoard, legalMoves, currentPlayer);
                simulationBoard.MakeMove(ref selectedMove);

                currentPlayer = simulationBoard.turnColor;
            }

            return BoardEvaluator.Evaluate(simulationBoard, BoardState.turnColor);
        }

        public void Backpropagate(int result)
        {
            Visits++;
            Wins += result;

            if (Parent != null)
            {
                Parent.Backpropagate(-result);
            }
        }

        public Node GetBestChild()
        {
            Node bestChild = null;
            float bestValue = float.NegativeInfinity;

            foreach (Node child in Children)
            {
                float uctValue = (child.Wins / (child.Visits + 1e-6f)) +
                                 Mathf.Sqrt(2 * Mathf.Log(Visits + 1) / (child.Visits + 1e-6f));
                if (uctValue > bestValue)
                {
                    bestValue = uctValue;
                    bestChild = child;
                }
            }

            return bestChild;
        }

        public Node GetChildWithMostVisits()
        {
            Node bestChild = null;
            int mostVisits = -1;

            foreach (Node child in Children)
            {
                if (child.Visits > mostVisits)
                {
                    mostVisits = child.Visits;
                    bestChild = child;
                }
            }

            return bestChild;
        }

        // NEW METHOD: GetHeuristicMove
        private Move GetHeuristicMove(Board board, List<Move> moves, int currentPlayer)
        {
            // Assign a score to each move based on heuristic evaluation
            List<Tuple<Move, int>> scoredMoves = new List<Tuple<Move, int>>();
            foreach (var move in moves)
            {
                int score = EvaluateMove(board, move, currentPlayer);
                scoredMoves.Add(new Tuple<Move, int>(move, score));
            }

            // Sort moves based on the score (higher is better)
            scoredMoves.Sort((a, b) => b.Item2.CompareTo(a.Item2));

            // Select the best move with some randomness
            int topN = Math.Min(5, scoredMoves.Count);
            int selectedIndex = random.Next(topN);
            return scoredMoves[selectedIndex].Item1;
        }

        // NEW METHOD: EvaluateMove
        private int EvaluateMove(Board board, Move move, int player)
        {
            int score = 0;

            int startPiece = board.Squares[move.StartSquare];
            int targetPiece = board.Squares[move.TargetSquare];

            int pieceType = Piece.PieceType(startPiece);
            int pieceColor = Piece.Color(startPiece);

            // Encourage capturing opponent's pieces
            if (Piece.Color(targetPiece) != Piece.NoColor && Piece.Color(targetPiece) != pieceColor)
            {
                int capturedPieceValue = BoardEvaluator.GetPieceValue(Piece.PieceType(targetPiece));
                score += capturedPieceValue * 10; // Higher weight for captures
            }

            // Encourage advancing pawns
            if (pieceType == Piece.Pawn)
            {
                int direction = (pieceColor == Piece.White) ? 1 : -1;
                int startRow = BoardInfo.GetRow(move.StartSquare);
                int targetRow = BoardInfo.GetRow(move.TargetSquare);
                score += (targetRow - startRow) * direction * 2;
            }

            // Encourage central control
            int targetCol = BoardInfo.GetFile(move.TargetSquare);
            int targetRowCentrality = (int)Math.Abs(3.5f - BoardInfo.GetRow(move.TargetSquare));
            int targetColCentrality = (int)Math.Abs(3.5f - targetCol);
            score += (int)(7 - (targetRowCentrality + targetColCentrality)); // Closer to center gets higher score

            return score;
        }
    }
}
