using DuckChess;

public class AlphaBetaNode
{
    public int alpha;
    public int beta;
    public int value;
    public bool isMaximizing;
    public int indexLeftOffAt;
    public Move moveToValue;
    public Move moveFromParent;
    public AlphaBetaNode (int alpha, int beta, bool isMaximizing, Move moveFromParent)
    {
        this.alpha = alpha;
        this.beta = beta;
        this.isMaximizing = isMaximizing;
        indexLeftOffAt = 0;
        this.moveFromParent = moveFromParent;
    }

    /// <summary>
    /// Judges whether to change to the new value or not. If it does, it updates the
    /// move to its new value
    /// </summary>
    /// <returns>Whether or not it decided the new value was better</returns>
    public bool JudgeNewValue(int newValue, Move moveToNewValue)
    {
        bool valueChanged;
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
        moveToValue = valueChanged ? moveToNewValue : moveToValue;
        return valueChanged;
    }
    public bool ShouldPrune()
    {
        return alpha >= beta;
    }

}
