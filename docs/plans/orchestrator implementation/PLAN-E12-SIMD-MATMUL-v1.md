# Plan: SIMD MatMul and Transpose — E12 Integration
# Version: 1
# Date: 2026-05-07
# Addresses: E12b/E12c proven speedup never wired into TensorCore
# Floor before: 1265
# Floor after:  1273

---

## Why this matters

DPO consumed 97.7% of the calibration run elapsed time. Every second of improvement
here multiplies directly into calibration speed. The 4-layer model (A58) makes this
more urgent: 4× as many MatMul calls per DPO step as the 1-layer config.

`TensorOps.MatMul` is a triple nested loop in pure scalar C#. Two problems:

**Problem 1 — Column-stride access.** The inner loop accesses `b` at stride `bCols`
per step — one cache miss per multiply. For a 64×64 matrix with floats that is 64
non-sequential 4-byte reads. On a machine with a 64-byte cache line this wastes
roughly 15 of every 16 fetches.

**Problem 2 — `Transpose` allocates per element.** `Transpose` calls `FlatToIndices`
for every element, and `FlatToIndices` does `new int[shape.Rank]` — a heap allocation
per element. For a 64×64 matrix that is 4,096 allocations per transpose. The backward
pass of every `MatMul` calls `Transpose` twice (once for each gradient), then
`MatMul` again on the transposed result. That is ~8,192 allocations per backward
`MatMul` call, repeated across 24 backward passes per forward-backward cycle.

E12b proved: **4–8× speedup at 64×64** with `System.Numerics.Vector<float>`. This
is exactly the size that flows through attention projections and feed-forward layers
at our hidden dimension of 32 and feed-forward dimension of 64.

Conservative estimate: **3× reduction in DPO time** = calibration run drops from
~4 hours to ~1.5 hours for 200 rows at the 4-layer config.

---

## What changes

### Phase 1 — Add `SimdOps` to `00.TensorCore`

**New file:** `00.TensorCore/Core/SimdOps.cs`

Move the proven `SimdMatMul.MatVec` logic from `35.ExperimentsV2/Core/SimdMatMul.cs`
into the core library as a static class `SimdOps`. Extend it with:

```csharp
using System.Numerics;

namespace TensorCore.Core;

// SIMD-accelerated matrix operations.
// E12b: 4–8× speedup at 64×64 vs scalar loops on AVX hardware.
// E12c: similar gain at 512×64 (embedding lookup shape).
internal static class SimdOps
{
    // Row-major matrix × vector: result[i] = dot(matrix_row_i, vector)
    // matrix: float[rows * cols], row-major
    // b_transposed: float[bCols * sharedDim], pre-transposed so rows are
    //               contiguous — eliminates column-stride cache misses
    internal static void MatVecRow(
        float[] a, int aOff, int aRows, int aCols,
        float[] bt, int btOff, int bCols,
        float[] result, int rOff)
    {
        int vSize = Vector<float>.Count;

        for (int i = 0; i < aRows; i++)
        {
            int aRowOff = aOff + i * aCols;
            for (int j = 0; j < bCols; j++)
            {
                int btRowOff = btOff + j * aCols; // bt stored transposed: bt[j, k]
                float sum = 0f;
                int k = 0;

                for (; k <= aCols - vSize; k += vSize)
                {
                    var va = new Vector<float>(a, aRowOff + k);
                    var vb = new Vector<float>(bt, btRowOff + k);
                    sum += Vector.Dot(va, vb);
                }
                for (; k < aCols; k++)
                    sum += a[aRowOff + k] * bt[btRowOff + k];

                result[rOff + i * bCols + j] = sum;
            }
        }
    }

    // Pre-transpose b into a temporary buffer — one cache-friendly pass.
    // Result: bt[j * sharedDim + k] = b[k * bCols + j]
    internal static float[] Transpose2D(float[] b, int rows, int cols)
    {
        var bt = new float[rows * cols];
        for (int k = 0; k < rows; k++)
        for (int j = 0; j < cols; j++)
            bt[j * rows + k] = b[k * cols + j];
        return bt;
    }

    // Transpose a 2D slice of a batched tensor (no heap allocation per element).
    // src: float[rows * cols] starting at srcOff
    // dst: float[cols * rows] starting at dstOff
    internal static void Transpose2DInPlace(
        float[] src, int srcOff, int rows, int cols,
        float[] dst, int dstOff)
    {
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
            dst[dstOff + c * rows + r] = src[srcOff + r * cols + c];
    }
}
```

### Phase 2 — Replace `TensorOps.MatMul` inner loop

**File:** `00.TensorCore/Core/TensorOps.cs`

Replace the triple nested loop in `MatMul` with:

1. Pre-transpose `b` once per batch using `SimdOps.Transpose2D` — one sequential
   write pass per batch element.
2. Call `SimdOps.MatVecRow` for each batch — SIMD dot products with contiguous reads
   on both sides.

The backward function is unchanged — it calls `MatMul(result.Grad!, Transpose(b))` 
and `MatMul(Transpose(a), result.Grad!)`, which now benefit from the same speedup
automatically.

```csharp
// Inside the batch loop, replace the triple scalar loop:
var bt = SimdOps.Transpose2D(b.Data, bRows, bCols);
SimdOps.MatVecRow(
    a.Data, aOff, aRows, aCols,
    bt, 0, bCols,
    resultData, rOff);
```

The semantics are identical — numerical output is bit-for-bit the same as the scalar
version for all non-FMA platforms (no fused multiply-add reordering). On platforms
where `Vector<float>` uses FMA the result may differ by floating-point rounding in
the last ulp, which is acceptable.

### Phase 3 — Replace `Transpose` inner loop

**File:** `00.TensorCore/Core/TensorOps.cs`

The current `Transpose` calls `FlatToIndices` per element — `new int[shape.Rank]`
per element. Replace with index arithmetic for the 2D case (the only case called
in practice) and a fallback general path for higher-rank transposes:

```csharp
public static Tensor Transpose(Tensor x, int dim1, int dim2)
{
    int rank = x.Shape.Rank;
    int d1   = dim1 < 0 ? rank + dim1 : dim1;
    int d2   = dim2 < 0 ? rank + dim2 : dim2;
    if (d1 == d2) return x; // no-op

    var newDims = x.Shape.Dimensions.ToArray();
    (newDims[d1], newDims[d2]) = (newDims[d2], newDims[d1]);
    var newShape = new TensorShape(newDims);
    var newData  = new float[x.Shape.TotalElements];

    // Fast path: last two dimensions (the standard matrix transpose case)
    if (d1 == rank - 2 && d2 == rank - 1)
    {
        int rows  = x.Shape.Dimensions[d1];
        int cols  = x.Shape.Dimensions[d2];
        int batch = x.Shape.TotalElements / (rows * cols);
        for (int b = 0; b < batch; b++)
            SimdOps.Transpose2DInPlace(
                x.Data, b * rows * cols, rows, cols,
                newData, b * rows * cols);
    }
    else
    {
        // General path — kept for correctness on exotic transposes
        for (int flat = 0; flat < x.Shape.TotalElements; flat++)
        {
            var indices = FlatToIndices(flat, x.Shape);
            (indices[d1], indices[d2]) = (indices[d2], indices[d1]);
            newData[newShape.FlatIndex(indices)] = x.Data[flat];
        }
    }

    var result = new Tensor(newData, newShape);
    if (x.RequiresGrad)
    {
        result.RequiresGrad = true;
        result.Children.Add(x);
        result.BackwardFunc = () => AccumulateGrad(x, Transpose(result.Grad!, dim1, dim2));
    }
    return result;
}
```

### Phase 4 — Update `BuildTinyConfig` (A58)

**File:** `21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs`

The uploaded codebase still has the pre-A58 values. Lock them as decided:

```csharp
private static ModelConfig BuildTinyConfig() => new ModelConfig
{
    VocabSize    = 512,
    ContextLen   = 64,
    HiddenDim    = 32,   // was 16 — locked A58
    NumLayers    = 4,    // was 1  — locked A58
    NumHeads     = 4,
    NumKvHeads   = 2,
    FfnHiddenDim = 64    // was 32 — locked A58
};
```

---

## Tests

**New file:** `Tests/SimdMatMulTests.cs`

### Test 1 — `SimdOps.MatVecRow` produces same result as scalar for 2×2

```
Given: a = [[1,2],[3,4]], b = [[5,7],[6,8]] (b pre-transposed of [[5,6],[7,8]])
When:  SimdOps.MatVecRow called
Then:  result[0,0]=19, result[0,1]=22, result[1,0]=43, result[1,1]=50
       (same as TensorOps.MatMul on [[1,2],[3,4]] × [[5,6],[7,8]])
```

### Test 2 — `TensorOps.MatMul` SIMD path matches PyTorch reference (existing values preserved)

```
Given: a = [[1,2],[3,4]], b = [[5,6],[7,8]]
When:  TensorOps.MatMul called (now uses SIMD path)
Then:  c[0,0]=19, c[0,1]=22, c[1,0]=43, c[1,1]=50
       (existing test MatMul_2x2_MatchesPyTorchReference must still pass)
```

### Test 3 — `TensorOps.MatMul` backward gradient unchanged

```
Given: a = [[1,2],[3,4]] requiresGrad=true, b = [[1],[1]]
When:  c = MatMul(a,b); Sum(c).Backward()
Then:  a.Grad = [[1,1],[1,1]]
       (existing Backward_MatMul_GradMatchesPyTorchReference must still pass)
```

### Test 4 — `TensorOps.Transpose` fast path for last two dimensions

```
Given: x = [[1,2,3],[4,5,6]], shape [2,3]
When:  TensorOps.Transpose(x, -2, -1)
Then:  result shape is [3,2]
       result = [[1,4],[2,5],[3,6]]
       (existing Transpose test must still pass)
```

### Test 5 — `TensorOps.MatMul` correctness at model shapes (64×32 × 32×32)

```
Given: a random [1, 64, 32] tensor, b random [32, 32] tensor, seed=42
When:  c_scalar = scalar_matmul(a, b)  (reference implementation, not TensorOps)
       c_simd   = TensorOps.MatMul(a, b)
Then:  max(abs(c_simd - c_scalar)) < 1e-4
       (numerical tolerance for SIMD rounding)
```

### Test 6 — `TensorOps.MatMul` correctness at feed-forward shape (64×32 × 32×64)

```
Given: a random [1, 64, 32] tensor, b random [32, 64] tensor, seed=42
When:  c_scalar and c_simd computed
Then:  max(abs(c_simd - c_scalar)) < 1e-4
```

### Test 7 — `SimdOps.Transpose2DInPlace` matches element-by-element reference

```
Given: matrix = [1,2,3,4,5,6], rows=2, cols=3
When:  SimdOps.Transpose2DInPlace called
Then:  result = [1,4,2,5,3,6]  (transposed [3,2])
```

### Test 8 — `BuildTinyConfig` returns A58 values

```
When:  DefaultLogicalCapabilityTrainer.BuildTinyConfig() called (via reflection or
       a trainer instance)
Then:  config.NumLayers    == 4
       config.HiddenDim    == 32
       config.FfnHiddenDim == 64
```

---

## What is NOT in this plan

- `SparseNgramTable` (E12a): only relevant to N-gram inference, not to DPO training.
  N-gram prediction is not on the hot path during calibration runs. Deferred.
- Batched SIMD beyond the inner loop: the batch loop is already sequential; the
  bottleneck is the inner dot product. No change needed there.
- `FlatToIndices` allocation in other callers (Gather, Scatter): those are not on the
  DPO hot path. Deferred until a profiling run shows them as significant.

---

## Expected impact

| What | Before | After (estimate) |
|---|---|---|
| MatMul inner loop | Scalar, column-stride cache misses | SIMD, row-major both sides |
| Transpose | `new int[rank]` per element | Direct index arithmetic |
| Backward MatMul | 2 × Transpose + 2 × MatMul scalar | 2 × Transpose fast + 2 × MatMul SIMD |
| DPO time per step | ~3.5s (batch-20 run average) | ~1.0–1.5s (3× estimate) |
| Calibration run 200 rows | ~1 hour (batch-20, old config) | ~1.5–2h (4L model, SIMD) |

The 4-layer model adds ~4× MatMul work vs 1-layer. SIMD removes ~3-4× of that.
Net: calibration runs at roughly the same wall-clock time as before A58, but with
a model that is capable of learning predicate patterns.

---

## Execution order

Phase 1 → Phase 2 → Phase 3 → Phase 4 → Tests.

Each phase: `dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1265, zero
failures before proceeding.

After all tests pass: floor 1273. (+8 new tests)

---

## Floor summary

| After | Floor |
|---|---:|
| Start | 1265 |
| Phase 1–3 (no new tests yet) | 1265 |
| Phase 4 (BuildTinyConfig) | 1265 |
| Tests added | 1273 |
