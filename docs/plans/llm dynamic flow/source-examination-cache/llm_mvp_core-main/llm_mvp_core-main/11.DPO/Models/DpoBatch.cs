using TensorCore.Core;

namespace DPO.Models;

// Батч данных для DPO: предпочтительный (chosen) и отклонённый (rejected) ответы.
public record DpoBatch
{
    // Token IDs: [1, T] — целые числа в виде float
    public required Tensor ChosenIds    { get; init; }
    public required Tensor RejectedIds  { get; init; }
    // Маски внимания: [1, T] — 1 для реальных токенов, 0 для padding
    public required Tensor ChosenMask   { get; init; }
    public required Tensor RejectedMask { get; init; }
}
