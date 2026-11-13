// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Collections;
using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class DynamicGraphSpeedMeterTests
{
    private const string InvalidCode = "INVALID_TRAINING_SPEED_MEASUREMENT";
    private const string InvalidMessage = "Dynamic graph training speed measurement is invalid.";

    public static IEnumerable<object[]> InvalidBatchIds()
    {
        yield return [null!];
        yield return [""];
        yield return ["   "];
        yield return [" batch-1"];
        yield return ["batch-1 "];
        yield return ["batch\r1"];
        yield return ["batch\n1"];
        yield return ["batch\t1"];
        yield return ["batch\u00851"];
        yield return ["batch\u20281"];
        yield return ["batch\u20291"];
    }

    [Fact]
    public void Contracts_ContainExactlyTheTrainingCostFields()
    {
        AssertRecordShape<DynamicGraphTrainingSpeedAxis>(
            ("CpuMs", typeof(long)),
            ("PeakRamMb", typeof(double)),
            ("ElapsedMs", typeof(long)),
            ("TrainRows", typeof(int)));
        AssertRecordShape<DynamicGraphTrainingSpeedMeasurement>(
            ("BatchId", typeof(string)),
            ("Speed", typeof(DynamicGraphTrainingSpeedAxis)));
    }

    [Fact]
    public void Measure_MapsDistinctTrainingCostSentinelsExactly()
    {
        var result = DynamicGraphSpeedMeter.Measure(Batch(
            batchId: "batch-sentinel",
            rowsTrained: 47,
            cpuMs: 1_003,
            peakRamMb: 2_009,
            elapsedMs: 3_017));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("batch-sentinel", result.Value!.BatchId);
        Assert.Equal(1_003L, result.Value.Speed.CpuMs);
        Assert.Equal(2_009d, result.Value.Speed.PeakRamMb);
        Assert.Equal(3_017L, result.Value.Speed.ElapsedMs);
        Assert.Equal(47, result.Value.Speed.TrainRows);
    }

    [Fact]
    public void Measure_NullResultFailsWithStableError()
        => AssertInvalid(DynamicGraphSpeedMeter.Measure(null));

    [Theory]
    [MemberData(nameof(InvalidBatchIds))]
    public void Measure_MalformedBatchIdFailsWithStableError(string? batchId)
        => AssertInvalid(DynamicGraphSpeedMeter.Measure(Batch(batchId: batchId!)));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Measure_NonPositiveRowsFailsWithStableError(int rowsTrained)
        => AssertInvalid(DynamicGraphSpeedMeter.Measure(Batch(rowsTrained: rowsTrained)));

    [Fact]
    public void Measure_NegativeCpuFailsWithStableError()
        => AssertInvalid(DynamicGraphSpeedMeter.Measure(Batch(cpuMs: -1)));

    [Fact]
    public void Measure_NegativePeakRamFailsWithStableError()
        => AssertInvalid(DynamicGraphSpeedMeter.Measure(Batch(peakRamMb: -1)));

    [Fact]
    public void Measure_NegativeElapsedFailsWithStableError()
        => AssertInvalid(DynamicGraphSpeedMeter.Measure(Batch(elapsedMs: -1)));

    [Fact]
    public void Measure_AllZeroResourcesRemainValid()
    {
        var result = DynamicGraphSpeedMeter.Measure(Batch(
            rowsTrained: 1,
            cpuMs: 0,
            peakRamMb: 0,
            elapsedMs: 0));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(new DynamicGraphTrainingSpeedAxis(0, 0d, 0, 1), result.Value!.Speed);
    }

    [Fact]
    public void Measure_DoesNotInspectThrowingPromotedUnitIds()
    {
        var fault = new InvalidOperationException("Promotion collection must not be inspected.");
        var result = DynamicGraphSpeedMeter.Measure(Batch(
            promotedUnitIds: new FaultingReadOnlyList<string>(fault)));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("batch-1", result.Value!.BatchId);
    }

    [Fact]
    public void Measure_DoesNotRequirePromotedUnitIds()
    {
        var result = DynamicGraphSpeedMeter.Measure(new LearningBatchResult(
            "batch-1",
            11,
            0.10d,
            0.90d,
            0.40d,
            101,
            202,
            303,
            null!));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(new DynamicGraphTrainingSpeedAxis(101, 202d, 303, 11), result.Value!.Speed);
    }

    [Fact]
    public void Measure_ContradictoryAccuracyAndRewardDoNotAlterOutput()
    {
        var first = DynamicGraphSpeedMeter.Measure(Batch(
            accuracyBefore: double.NaN,
            accuracyAfter: double.PositiveInfinity,
            rewardMargin: double.NegativeInfinity));
        var second = DynamicGraphSpeedMeter.Measure(Batch(
            accuracyBefore: double.NegativeInfinity,
            accuracyAfter: -123.45d,
            rewardMargin: double.NaN));

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(first.Value, second.Value);
    }

    private static LearningBatchResult Batch(
        string batchId = "batch-1",
        int rowsTrained = 11,
        double accuracyBefore = 0.10d,
        double accuracyAfter = 0.90d,
        double rewardMargin = 0.40d,
        long cpuMs = 101,
        long peakRamMb = 202,
        long elapsedMs = 303,
        IReadOnlyList<string>? promotedUnitIds = null)
        => new(
            batchId,
            rowsTrained,
            accuracyBefore,
            accuracyAfter,
            rewardMargin,
            cpuMs,
            peakRamMb,
            elapsedMs,
            promotedUnitIds ?? ["unit-ignored"]);

    private static void AssertInvalid(
        OperationResult<DynamicGraphTrainingSpeedMeasurement> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(InvalidCode, result.ErrorCode);
        Assert.Equal(InvalidMessage, result.ErrorMessage);
    }

    private static void AssertRecordShape<T>(params (string Name, Type Type)[] expected)
    {
        var constructor = Assert.Single(typeof(T).GetConstructors());
        Assert.Equal(
            expected,
            constructor.GetParameters()
                .Select(parameter => (parameter.Name!, parameter.ParameterType))
                .ToArray());
        Assert.Equal(
            expected.Select(field => field.Name),
            typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name));
    }

    private sealed class FaultingReadOnlyList<T>(Exception exception) : IReadOnlyList<T>
    {
        public int Count => throw exception;
        public T this[int index] => throw exception;
        public IEnumerator<T> GetEnumerator() => throw exception;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
