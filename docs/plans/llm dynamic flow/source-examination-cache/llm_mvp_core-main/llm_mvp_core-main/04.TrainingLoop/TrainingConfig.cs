namespace TrainingLoop;

/// <summary>
/// All training hyperparameters in one place — no inline constants anywhere else (DNA rule 1).
/// </summary>
public record TrainingConfig
{
    public int    BatchSize          { get; init; } = 8;
    public int    SeqLen             { get; init; } = 2048;
    public float  MaxLr              { get; init; } = 3e-4f;
    public float  MinLr              { get; init; } = 3e-5f;
    public int    WarmupSteps        { get; init; } = 100;
    public int    TotalSteps         { get; init; } = 10_000;
    public int    GradAccumSteps     { get; init; } = 4;
    public float  GradClipNorm       { get; init; } = 1.0f;
    public float  WeightDecay        { get; init; } = 0.1f;
    public int    CheckpointEvery    { get; init; } = 500;
    public string CheckpointDir      { get; init; } = "checkpoints";
    public int    LogEvery           { get; init; } = 10;
}
