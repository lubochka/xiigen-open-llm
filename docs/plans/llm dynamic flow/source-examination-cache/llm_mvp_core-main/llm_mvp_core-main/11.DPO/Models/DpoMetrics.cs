namespace DPO.Models;

public record DpoMetrics
{
    public int    Step          { get; init; }
    public float  Loss          { get; init; }
    public float  ChosenReward  { get; init; }
    public float  RejectedReward { get; init; }
    public float  RewardMargin  { get; init; }
    public float  Accuracy      { get; init; }  // fraction where chosen > rejected
}
