using UnityEngine;
using DuckChess;
using static DuckChess.Board;

namespace DuckChess
{
    public class AlphaBetaNode
    {
        public int alpha;
        public int beta;
        public int value;
        public bool isMaximizing;
        public int indexLeftOffAt;
        public Move moveToValue;
        public AlphaBetaNode child;
        public Board currentBoard;
        public Move moveFromParent;
        public AlphaBetaNode(int alpha, int beta, bool isMaximizing, Board board)
        {
            this.alpha = alpha;
            this.beta = beta;
            this.isMaximizing = isMaximizing;
            indexLeftOffAt = 0;
            value = isMaximizing ? alpha : beta;
            currentBoard = board;
        }
        public AlphaBetaNode(int alpha, int beta, bool isMaximizing, Board board, Move moveFromParent)
        {
            this.alpha = alpha;
            this.beta = beta;
            this.isMaximizing = isMaximizing;
            indexLeftOffAt = 0;
            currentBoard = board;
            this.moveFromParent = moveFromParent;
            value = isMaximizing ? alpha : beta;
        }

        /// <summary>
        /// Judges whether to change to the new value or not. If it does, it updates the
        /// move to its new value
        /// </summary>
        /// <returns>Whether or not it decided the new value was better</returns>
        public bool JudgeNewValue(int newValue, Move moveToNewValue, AlphaBetaNode child)
        {
            bool valueChanged = false;
            if (isMaximizing)
            {
                valueChanged = newValue > alpha;
                alpha = valueChanged ? newValue : alpha;
                value = alpha;
            }
            else
            {
                valueChanged = newValue < beta;
                beta = valueChanged ? newValue : beta;
                value = beta;
            }
            if (valueChanged)
            {
                moveToValue = moveToNewValue;
                this.child = child;
            }

            return valueChanged;
        }
        public bool ShouldPrune()
        {
            return alpha >= beta;
        }

        public override string ToString()
        {
            string output = "";
            output += "alpha " + alpha + " beta " + beta + " value " + value + " Max? " + isMaximizing + "\n";
            output += "move: " + moveToValue.ToString();
            if (child != null)
            {
                output += "\n" + child.ToString();
            }
            return output;
        }

    }

}