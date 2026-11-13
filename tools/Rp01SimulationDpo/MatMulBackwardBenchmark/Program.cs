// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using TensorCore.Core;

// Finite, tensor-only comparison. Run this same executable before and after
// changing TensorOps; no model, optimizer, dataset or generation is loaded.
if (args.Length != 3 || (args[0] != "scalar-before" && args[0] != "simd-after"))
    throw new ArgumentException("Usage: <scalar-before|simd-after> <repository-root> <receipt.json>");

const int warmups = 2;
const int repeats = 5;
var cases = new[]
{
    new Scenario("hidden-frozen-right", 150, 512, 512, false),
    new Scenario("ffn-up-frozen-right", 150, 512, 1408, false),
    new Scenario("ffn-down-frozen-right", 150, 1408, 512, false),
    new Scenario("lora-down", 150, 512, 8, true),
    new Scenario("lora-up", 150, 8, 512, true)
};
var results = new List<object>();
foreach (var scenario in cases)
{
    var a = new Tensor(Data(scenario.Rows * scenario.Inner, 1701),
        new TensorShape(scenario.Rows, scenario.Inner), requiresGrad: true);
    var b = new Tensor(Data(scenario.Inner * scenario.Columns, 1702),
        new TensorShape(scenario.Inner, scenario.Columns), requiresGrad: scenario.RightGradient);
    var output = TensorOps.MatMul(a, b);
    var upstream = new Tensor(Data(scenario.Rows * scenario.Columns, 1703), output.Shape);
    var (expectedA, expectedB) = ScalarGradients(a.Data, b.Data, upstream.Data, scenario);
    output.Backward(upstream);
    var errorA = Check(expectedA, a.Grad!.Data);
    var errorB = scenario.RightGradient ? Check(expectedB, b.Grad!.Data) : 0;
    if (!scenario.RightGradient && b.Grad is not null)
        throw new InvalidOperationException("Frozen operand acquired a gradient.");

    for (var i = 0; i < warmups; i++)
    {
        a.Grad = b.Grad = null;
        output.Backward(upstream);
    }

    var elapsed = new double[repeats];
    var allocated = new long[repeats];
    var process = Process.GetCurrentProcess();
    var cpuBefore = process.TotalProcessorTime;
    for (var i = 0; i < repeats; i++)
    {
        // The timed region includes actual Backward graph traversal, gradient
        // buffers, transpose work, reductions, and gradient publication.
        a.Grad = b.Grad = null;
        var allocationBefore = GC.GetAllocatedBytesForCurrentThread();
        var start = Stopwatch.GetTimestamp();
        output.Backward(upstream);
        elapsed[i] = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        allocated[i] = GC.GetAllocatedBytesForCurrentThread() - allocationBefore;
    }
    var cpuMs = (process.TotalProcessorTime - cpuBefore).TotalMilliseconds;
    var sorted = elapsed.Order().ToArray();
    var median = sorted[repeats / 2];
    results.Add(new
    {
        scenario.Name, scenario.Rows, scenario.Inner, scenario.Columns,
        leftRequiresGrad = true, rightRequiresGrad = scenario.RightGradient,
        backwardMultiplyAddPairs = (long)scenario.Rows * scenario.Inner * scenario.Columns
            * (scenario.RightGradient ? 2 : 1),
        warmups, repeats, milliseconds = elapsed, medianMilliseconds = median,
        minimumMilliseconds = sorted[0], processCpuMilliseconds = cpuMs,
        allocatedBytes = allocated, maxAbsoluteErrorA = errorA, maxAbsoluteErrorB = errorB
    });
    Console.WriteLine($"{scenario.Name} [{scenario.Rows},{scenario.Inner}]x[{scenario.Inner},{scenario.Columns}] rightGrad={scenario.RightGradient} median_ms={median:F6} errorA={errorA:G6} errorB={errorB:G6}");
}

var sourcePath = Path.Combine(args[1], "00.TensorCore", "Core", "TensorOps.cs");
var receipt = new
{
    phase = args[0], utc = DateTimeOffset.UtcNow,
    scope = "Actual Tensor.Backward only; forward/setup/scalar validation excluded; no training or whole-run speedup measured.",
    runtime = RuntimeInformation.FrameworkDescription,
    os = RuntimeInformation.OSDescription, architecture = RuntimeInformation.ProcessArchitecture.ToString(),
    logicalProcessors = Environment.ProcessorCount,
    vectorHardwareAccelerated = Vector.IsHardwareAccelerated, vectorFloatCount = Vector<float>.Count,
    tensorOpsSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(sourcePath))).ToLowerInvariant(),
    tensorAssemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(Tensor).Assembly.Location))).ToLowerInvariant(),
    seeds = new[] { 1701, 1702, 1703 }, warmups, repeats, results
};
File.WriteAllText(args[2], JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Receipt: {Path.GetFullPath(args[2])}");

static float[] Data(int count, int seed)
{
    var random = new Random(seed);
    var data = new float[count];
    for (var i = 0; i < count; i++) data[i] = (float)(random.NextDouble() * 0.2 - 0.1);
    return data;
}

static (float[] A, float[] B) ScalarGradients(float[] a, float[] b, float[] grad, Scenario s)
{
    var gradA = new float[a.Length];
    var gradB = new float[b.Length];
    // Frozen pre-repair loop orders, retained only as a numerical oracle.
    for (var row = 0; row < s.Rows; row++)
    for (var inner = 0; inner < s.Inner; inner++)
    for (var column = 0; column < s.Columns; column++)
        gradA[row * s.Inner + inner] += grad[row * s.Columns + column] * b[inner * s.Columns + column];
    if (s.RightGradient)
        for (var inner = 0; inner < s.Inner; inner++)
        for (var column = 0; column < s.Columns; column++)
        for (var row = 0; row < s.Rows; row++)
            gradB[inner * s.Columns + column] += a[row * s.Inner + inner] * grad[row * s.Columns + column];
    return (gradA, gradB);
}

static float Check(float[] expected, float[] actual)
{
    var maximum = 0f;
    for (var i = 0; i < expected.Length; i++)
    {
        var error = Math.Abs(expected[i] - actual[i]);
        if (!float.IsFinite(actual[i]) || error > 2e-5f + 2e-5f * Math.Abs(expected[i]))
            throw new InvalidOperationException($"Gradient mismatch at {i}: expected={expected[i]}, actual={actual[i]}");
        maximum = Math.Max(maximum, error);
    }
    return maximum;
}

internal sealed record Scenario(string Name, int Rows, int Inner, int Columns, bool RightGradient);
