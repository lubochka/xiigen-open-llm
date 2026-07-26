// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Collections;
using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class DynamicGraphRunSpeedMeterTests
{
    private const string InvalidCode = "INVALID_RUN_SPEED_OBSERVATION";
    private const string InvalidMessage = "Dynamic graph run speed observation is invalid.";
    private const string OverflowCode = "RUN_SPEED_MEASUREMENT_OVERFLOW";
    private const string OverflowMessage = "Dynamic graph run speed measurement overflowed.";

    [Fact]
    public void Contracts_ContainExactlyTheRunSpeedFields()
    {
        AssertRecordShape<DynamicGraphRunSpeedObservation>(
            ("EvaluatedRowCount", typeof(int)),
            ("InferenceElapsedMs", typeof(long)),
            ("RoutingElapsedMs", typeof(long)),
            ("CandidateElapsedMsSamples", typeof(IReadOnlyList<long>)),
            ("BatchElapsedMsSamples", typeof(IReadOnlyList<long>)));
        AssertRecordShape<DynamicGraphRunSpeedAxis>(
            ("InferenceElapsedMs", typeof(long)),
            ("ThroughputRowsPerSec", typeof(double)),
            ("RoutingElapsedMs", typeof(long)),
            ("PerCandidateElapsedMs", typeof(long?)),
            ("PerBatchElapsedMs", typeof(long?)));
        AssertRecordShape<DynamicGraphRunSpeedMeasurement>(
            ("EvaluatedRowCount", typeof(int)),
            ("CandidateSampleCount", typeof(int)),
            ("BatchSampleCount", typeof(int)),
            ("RunSpeed", typeof(DynamicGraphRunSpeedAxis)));
    }

    [Fact]
    public void Meter_PublicSurfaceContainsOnlyThePureMeasureMethod()
    {
        var method = Assert.Single(typeof(DynamicGraphRunSpeedMeter).GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

        Assert.Equal(nameof(DynamicGraphRunSpeedMeter.Measure), method.Name);
        Assert.Equal(typeof(OperationResult<DynamicGraphRunSpeedMeasurement>), method.ReturnType);
        var parameter = Assert.Single(method.GetParameters());
        Assert.Equal("observation", parameter.Name);
        Assert.Equal(typeof(DynamicGraphRunSpeedObservation), parameter.ParameterType);
        Assert.Empty(typeof(DynamicGraphRunSpeedMeter).GetFields(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Empty(typeof(DynamicGraphRunSpeedMeter).GetProperties(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.All(
            typeof(DynamicGraphRunSpeedMeter).GetFields(
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly),
            field => Assert.True(field.IsLiteral, $"Meter retained non-constant field {field.Name}."));

        var publicSurface = string.Join('|', new[]
        {
            method.ReturnType.FullName,
            parameter.ParameterType.FullName,
            string.Join(',', typeof(DynamicGraphRunSpeedObservation).GetProperties().Select(value => value.Name)),
            string.Join(',', typeof(DynamicGraphRunSpeedAxis).GetProperties().Select(value => value.Name)),
            string.Join(',', typeof(DynamicGraphRunSpeedMeasurement).GetProperties().Select(value => value.Name))
        });
        foreach (var forbidden in new[]
                 {
                     "AsyncCandidateBatch", "GraphCandidateOutput", "Training", "Dpo", "Lifecycle",
                     "Checkpoint", "Promotion"
                 })
        {
            Assert.DoesNotContain(forbidden, publicSurface, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Measure_ProjectsExactThroughputAndIndependentTimingAxes()
    {
        var result = DynamicGraphRunSpeedMeter.Measure(Observation(
            rows: 25,
            inferenceMs: 200,
            routingMs: 37,
            candidateSamples: [1, 2],
            batchSamples: [10, 20]));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(25, result.Value!.EvaluatedRowCount);
        Assert.Equal(2, result.Value.CandidateSampleCount);
        Assert.Equal(2, result.Value.BatchSampleCount);
        Assert.Equal(200L, result.Value.RunSpeed.InferenceElapsedMs);
        Assert.Equal(125d, result.Value.RunSpeed.ThroughputRowsPerSec);
        Assert.Equal(37L, result.Value.RunSpeed.RoutingElapsedMs);
        Assert.Equal(2L, result.Value.RunSpeed.PerCandidateElapsedMs);
        Assert.Equal(15L, result.Value.RunSpeed.PerBatchElapsedMs);
    }

    [Fact]
    public void Measure_ThroughputIsIndependentOfRoutingAndSampleBanks()
    {
        var first = DynamicGraphRunSpeedMeter.Measure(Observation(
            rows: 25,
            inferenceMs: 200,
            routingMs: 0,
            candidateSamples: [],
            batchSamples: [999]));
        var second = DynamicGraphRunSpeedMeter.Measure(Observation(
            rows: 25,
            inferenceMs: 200,
            routingMs: 7_777,
            candidateSamples: [1, 2, 3, 4],
            batchSamples: []));

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(125d, first.Value!.RunSpeed.ThroughputRowsPerSec);
        Assert.Equal(first.Value.RunSpeed.ThroughputRowsPerSec, second.Value!.RunSpeed.ThroughputRowsPerSec);
    }

    [Fact]
    public void Measure_CandidateHalfMillisecondRoundsAwayFromZero()
    {
        var result = DynamicGraphRunSpeedMeter.Measure(Observation(
            candidateSamples: [1, 2],
            batchSamples: []));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2L, result.Value!.RunSpeed.PerCandidateElapsedMs);
    }

    [Fact]
    public void Measure_CandidateEvenHalfDoesNotUseBankersRounding()
    {
        var result = DynamicGraphRunSpeedMeter.Measure(Observation(
            candidateSamples: [2, 3],
            batchSamples: []));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(3L, result.Value!.RunSpeed.PerCandidateElapsedMs);
    }

    [Fact]
    public void Measure_CandidateSingletonAboveExactDoubleRangeRemainsExact()
    {
        const long sample = 9_007_199_254_740_993L;
        var result = DynamicGraphRunSpeedMeter.Measure(Observation(
            candidateSamples: [sample],
            batchSamples: []));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(sample, result.Value!.RunSpeed.PerCandidateElapsedMs);
    }

    [Fact]
    public void Measure_BatchSingletonAboveExactDoubleRangeRemainsExact()
    {
        const long sample = 9_007_199_254_740_993L;
        var result = DynamicGraphRunSpeedMeter.Measure(Observation(
            candidateSamples: [],
            batchSamples: [sample]));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(sample, result.Value!.RunSpeed.PerBatchElapsedMs);
    }

    [Fact]
    public void Measure_CandidateSingletonLongMaxValueSucceedsExactly()
    {
        var result = DynamicGraphRunSpeedMeter.Measure(Observation(
            candidateSamples: [long.MaxValue],
            batchSamples: []));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(long.MaxValue, result.Value!.RunSpeed.PerCandidateElapsedMs);
    }

    [Fact]
    public void Measure_BatchSingletonLongMaxValueSucceedsExactly()
    {
        var result = DynamicGraphRunSpeedMeter.Measure(Observation(
            candidateSamples: [],
            batchSamples: [long.MaxValue]));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(long.MaxValue, result.Value!.RunSpeed.PerBatchElapsedMs);
    }

    [Fact]
    public void Measure_KeepsCandidateAndBatchSampleBanksSeparate()
    {
        var result = DynamicGraphRunSpeedMeter.Measure(Observation(
            candidateSamples: [2, 4],
            batchSamples: [101, 103]));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(3L, result.Value!.RunSpeed.PerCandidateElapsedMs);
        Assert.Equal(102L, result.Value.RunSpeed.PerBatchElapsedMs);
    }

    [Fact]
    public void Measure_EmptySampleBanksProduceNullAveragesAndZeroCounts()
    {
        var result = DynamicGraphRunSpeedMeter.Measure(Observation(
            candidateSamples: [],
            batchSamples: []));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, result.Value!.CandidateSampleCount);
        Assert.Equal(0, result.Value.BatchSampleCount);
        Assert.Null(result.Value.RunSpeed.PerCandidateElapsedMs);
        Assert.Null(result.Value.RunSpeed.PerBatchElapsedMs);
    }

    [Fact]
    public void Measure_ZeroRoutingTimeIsValid()
    {
        var result = DynamicGraphRunSpeedMeter.Measure(Observation(routingMs: 0));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0L, result.Value!.RunSpeed.RoutingElapsedMs);
    }

    [Fact]
    public void Measure_NullObservationFailsWithStableError()
        => AssertInvalid(DynamicGraphRunSpeedMeter.Measure(null));

    [Theory]
    [InlineData("candidate")]
    [InlineData("batch")]
    public void Measure_NullSampleBankFailsWithStableError(string bank)
    {
        var observation = bank == "candidate"
            ? new DynamicGraphRunSpeedObservation(25, 200, 37, null!, [10, 20])
            : new DynamicGraphRunSpeedObservation(25, 200, 37, [1, 3], null!);

        AssertInvalid(DynamicGraphRunSpeedMeter.Measure(observation));
    }

    [Theory]
    [InlineData(0, 200L, 0L)]
    [InlineData(-1, 200L, 0L)]
    [InlineData(25, 0L, 0L)]
    [InlineData(25, -1L, 0L)]
    [InlineData(25, 200L, -1L)]
    public void Measure_InvalidScalarFailsWithStableError(int rows, long inferenceMs, long routingMs)
        => AssertInvalid(DynamicGraphRunSpeedMeter.Measure(Observation(
            rows: rows,
            inferenceMs: inferenceMs,
            routingMs: routingMs)));

    [Theory]
    [InlineData("candidate")]
    [InlineData("batch")]
    public void Measure_NegativeSampleFailsWithStableError(string bank)
    {
        var observation = bank == "candidate"
            ? Observation(candidateSamples: [1, -1], batchSamples: [2])
            : Observation(candidateSamples: [1], batchSamples: [2, -1]);

        AssertInvalid(DynamicGraphRunSpeedMeter.Measure(observation));
    }

    [Theory]
    [InlineData("candidate")]
    [InlineData("batch")]
    public void Measure_CheckedSampleSumOverflowReturnsStableOverflow(string bank)
    {
        var observation = bank == "candidate"
            ? Observation(candidateSamples: [long.MaxValue, 1], batchSamples: [2])
            : Observation(candidateSamples: [2], batchSamples: [long.MaxValue, 1]);

        AssertOverflow(DynamicGraphRunSpeedMeter.Measure(observation));
    }

    [Theory]
    [InlineData("candidate")]
    [InlineData("batch")]
    public void Measure_OrdinarySnapshotFailureReturnsStableInvalid(string bank)
    {
        var fault = new InvalidOperationException("snapshot race", new IOException("ordinary inner"));
        var observation = bank == "candidate"
            ? Observation(candidateSamples: new FaultingReadOnlyList<long>(fault))
            : Observation(batchSamples: new FaultingReadOnlyList<long>(fault));

        AssertInvalid(DynamicGraphRunSpeedMeter.Measure(observation));
    }

    [Fact]
    public void Measure_CancellationFromFirstSnapshotEscapesUnchanged()
    {
        var cancellation = new OperationCanceledException("cancel candidate snapshot");

        var thrown = Assert.Throws<OperationCanceledException>(() =>
            DynamicGraphRunSpeedMeter.Measure(Observation(
                rows: 0,
                candidateSamples: new FaultingReadOnlyList<long>(cancellation))));

        Assert.Same(cancellation, thrown);
    }

    [Fact]
    public void Measure_WrapperContainingCancellationFromSecondSnapshotEscapesUnchanged()
    {
        var wrapper = new InvalidOperationException(
            "wrapped cancellation",
            new OperationCanceledException("cancel batch snapshot"));

        var thrown = Assert.Throws<InvalidOperationException>(() =>
            DynamicGraphRunSpeedMeter.Measure(Observation(
                inferenceMs: 0,
                batchSamples: new FaultingReadOnlyList<long>(wrapper))));

        Assert.Same(wrapper, thrown);
    }

    [Fact]
    public void Measure_AggregateContainingFatalFromSecondSnapshotEscapesUnchanged()
    {
        var wrapper = new AggregateException(
            new InvalidOperationException("ordinary"),
            new OutOfMemoryException("fatal batch snapshot"));

        var thrown = Assert.Throws<AggregateException>(() =>
            DynamicGraphRunSpeedMeter.Measure(Observation(
                routingMs: -1,
                batchSamples: new FaultingReadOnlyList<long>(wrapper))));

        Assert.Same(wrapper, thrown);
    }

    [Fact]
    public void Measure_SnapshotsCandidateBankBeforeReadingBatchBank()
    {
        var candidateSamples = new List<long> { 1, 3 };
        var batchSamples = new CallbackReadOnlyList<long>([10, 20], () =>
        {
            candidateSamples[0] = 100;
            candidateSamples[1] = 100;
        });

        var result = DynamicGraphRunSpeedMeter.Measure(Observation(
            candidateSamples: candidateSamples,
            batchSamples: batchSamples));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2L, result.Value!.RunSpeed.PerCandidateElapsedMs);
        Assert.Equal(15L, result.Value.RunSpeed.PerBatchElapsedMs);
    }

    [Fact]
    public void Measure_ResultDoesNotChangeAfterCallerMutatesOriginalBanks()
    {
        var candidateSamples = new List<long> { 1, 3 };
        var batchSamples = new List<long> { 10, 20 };
        var result = DynamicGraphRunSpeedMeter.Measure(Observation(
            candidateSamples: candidateSamples,
            batchSamples: batchSamples));
        Assert.True(result.IsSuccess, result.ErrorMessage);

        candidateSamples[0] = 999;
        candidateSamples.Add(999);
        batchSamples.Clear();

        Assert.Equal(2, result.Value!.CandidateSampleCount);
        Assert.Equal(2, result.Value.BatchSampleCount);
        Assert.Equal(2L, result.Value.RunSpeed.PerCandidateElapsedMs);
        Assert.Equal(15L, result.Value.RunSpeed.PerBatchElapsedMs);
    }

    private static DynamicGraphRunSpeedObservation Observation(
        int rows = 25,
        long inferenceMs = 200,
        long routingMs = 37,
        IReadOnlyList<long>? candidateSamples = null,
        IReadOnlyList<long>? batchSamples = null)
        => new(
            rows,
            inferenceMs,
            routingMs,
            candidateSamples ?? [1, 3],
            batchSamples ?? [10, 20]);

    private static void AssertInvalid(OperationResult<DynamicGraphRunSpeedMeasurement> result)
        => AssertFailure(result, InvalidCode, InvalidMessage);

    private static void AssertOverflow(OperationResult<DynamicGraphRunSpeedMeasurement> result)
        => AssertFailure(result, OverflowCode, OverflowMessage);

    private static void AssertFailure(
        OperationResult<DynamicGraphRunSpeedMeasurement> result,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(expectedMessage, result.ErrorMessage);
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
        public int Count => 1;
        public T this[int index] => throw exception;
        public IEnumerator<T> GetEnumerator() => throw exception;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class CallbackReadOnlyList<T>(IReadOnlyList<T> values, Action callback)
        : IReadOnlyList<T>
    {
        public int Count => values.Count;
        public T this[int index] => values[index];

        public IEnumerator<T> GetEnumerator()
        {
            callback();
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
