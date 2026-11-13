namespace LoRA;

public record LoraConfig
{
    public int    Rank        { get; init; } = 8;
    public float  Alpha       { get; init; } = 16f;   // scaling = alpha / rank
    public float  Dropout     { get; init; } = 0.05f;

    // Which module name substrings to apply LoRA to
    public string[] TargetModules { get; init; } = ["q_proj", "v_proj"];

    public float Scaling => Alpha / Rank;
}
