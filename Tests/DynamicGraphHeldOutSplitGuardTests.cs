// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace Tests;

using System.Collections;
using System.Reflection;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class DynamicGraphHeldOutSplitGuardTests
{
    private const string InvalidCode = "INVALID_SPLIT_PROOF";
    private const string InvalidMessage = "Dynamic graph measurement split proof is invalid.";

    [Fact]
    public void Contract_ContainsExactlyTheSixPlannedFieldsInOrder()
    {
        var constructor = Assert.Single(typeof(DynamicGraphMeasurementSplitProof).GetConstructors());
        Assert.Equal(
            [
                ("TrainSourceRoots", typeof(IReadOnlyList<string>)),
                ("EvalSourceRoots", typeof(IReadOnlyList<string>)),
                ("SourceHashes", typeof(IReadOnlyList<string>)),
                ("CrossSplitNearDuplicateCount", typeof(int)),
                ("SimhashOrMinhash", typeof(string)),
                ("NearDuplicateThreshold", typeof(double))
            ],
            constructor.GetParameters()
                .Select(parameter => (parameter.Name!, parameter.ParameterType))
                .ToArray());

        Assert.Equal(
            [
                "TrainSourceRoots",
                "EvalSourceRoots",
                "SourceHashes",
                "CrossSplitNearDuplicateCount",
                "SimhashOrMinhash",
                "NearDuplicateThreshold"
            ],
            typeof(DynamicGraphMeasurementSplitProof)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)
                .ToArray());
    }

    [Fact]
    public void Validate_AcceptsOpaqueHashesOrdinallyDistinctRootsAndThresholdAboveOne()
    {
        var proof = BuildProof(
            trainSourceRoots: ["Root-A"],
            evalSourceRoots: ["root-a"],
            sourceHashes: ["opaque-source-identity-with-no-invented-algorithm"],
            method: "minhash",
            threshold: 1.25d);

        var result = DynamicGraphHeldOutSplitGuard.Validate(proof);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);
        Assert.Equal(["Root-A"], result.Value.TrainSourceRoots);
        Assert.Equal(["root-a"], result.Value.EvalSourceRoots);
        Assert.Equal(["opaque-source-identity-with-no-invented-algorithm"], result.Value.SourceHashes);
        Assert.Equal("minhash", result.Value.SimhashOrMinhash);
        Assert.Equal(1.25d, result.Value.NearDuplicateThreshold);
    }

    [Fact]
    public void Validate_ReturnsDeeplyOwnedReadOnlyCollectionSnapshots()
    {
        var train = new List<string> { "train-a" };
        var eval = new List<string> { "eval-a" };
        var hashes = new List<string> { "hash-a" };
        var proof = BuildProof(train, eval, hashes);

        var result = DynamicGraphHeldOutSplitGuard.Validate(proof);
        Assert.True(result.IsSuccess, result.ErrorMessage);

        train[0] = "train-mutated";
        eval[0] = "eval-mutated";
        hashes[0] = "hash-mutated";
        train.Add("train-added");
        eval.Add("eval-added");
        hashes.Add("hash-added");

        Assert.NotSame(train, result.Value!.TrainSourceRoots);
        Assert.NotSame(eval, result.Value.EvalSourceRoots);
        Assert.NotSame(hashes, result.Value.SourceHashes);
        Assert.Equal(["train-a"], result.Value.TrainSourceRoots);
        Assert.Equal(["eval-a"], result.Value.EvalSourceRoots);
        Assert.Equal(["hash-a"], result.Value.SourceHashes);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)result.Value.TrainSourceRoots).Add("forbidden"));
    }

    [Fact]
    public void Validate_NullProofFailsClosed()
    {
        var result = DynamicGraphHeldOutSplitGuard.Validate(null);

        AssertInvalid(result);
    }

    [Theory]
    [InlineData("train")]
    [InlineData("eval")]
    [InlineData("hashes")]
    public void Validate_NullCollectionsFailClosed(string carrier)
    {
        var valid = BuildProof();
        var result = DynamicGraphHeldOutSplitGuard.Validate(carrier switch
        {
            "train" => valid with { TrainSourceRoots = null! },
            "eval" => valid with { EvalSourceRoots = null! },
            "hashes" => valid with { SourceHashes = null! },
            _ => throw new InvalidOperationException()
        });

        AssertInvalid(result);
    }

    [Theory]
    [InlineData("train")]
    [InlineData("eval")]
    [InlineData("hashes")]
    public void Validate_EmptyCollectionsFailClosed(string carrier)
    {
        var result = DynamicGraphHeldOutSplitGuard.Validate(carrier switch
        {
            "train" => BuildProof(trainSourceRoots: []),
            "eval" => BuildProof(evalSourceRoots: []),
            "hashes" => BuildProof(sourceHashes: []),
            _ => throw new InvalidOperationException()
        });

        AssertInvalid(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" leading")]
    [InlineData("trailing ")]
    [InlineData("line\nfeed")]
    [InlineData("control\u0001value")]
    [InlineData("separator\u2028value")]
    public void Validate_NonCanonicalCollectionElementsFailClosed(string? invalidValue)
    {
        AssertInvalid(DynamicGraphHeldOutSplitGuard.Validate(BuildProof(
            trainSourceRoots: new[] { invalidValue! })));
        AssertInvalid(DynamicGraphHeldOutSplitGuard.Validate(BuildProof(
            evalSourceRoots: new[] { invalidValue! })));
        AssertInvalid(DynamicGraphHeldOutSplitGuard.Validate(BuildProof(
            sourceHashes: new[] { invalidValue! })));
    }

    [Theory]
    [InlineData("train")]
    [InlineData("eval")]
    [InlineData("hashes")]
    public void Validate_DuplicateCollectionEntriesFailClosed(string carrier)
    {
        var duplicate = new[] { "same", "same" };
        var result = DynamicGraphHeldOutSplitGuard.Validate(carrier switch
        {
            "train" => BuildProof(trainSourceRoots: duplicate),
            "eval" => BuildProof(evalSourceRoots: duplicate),
            "hashes" => BuildProof(sourceHashes: duplicate),
            _ => throw new InvalidOperationException()
        });

        AssertInvalid(result);
    }

    [Fact]
    public void Validate_ExactRootOverlapPrecedesNearDuplicateFailure()
    {
        var result = DynamicGraphHeldOutSplitGuard.Validate(BuildProof(
            trainSourceRoots: ["shared-root"],
            evalSourceRoots: ["shared-root"],
            nearDuplicateCount: 3));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("TRAIN_HELDOUT_OVERLAP", result.ErrorCode);
        Assert.Equal("Training and held-out source roots overlap.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_PositiveNearDuplicateCountBlocksAdmission()
    {
        var result = DynamicGraphHeldOutSplitGuard.Validate(BuildProof(nearDuplicateCount: 1));

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal("TRAIN_HELDOUT_NEAR_DUPLICATE", result.ErrorCode);
        Assert.Equal("Training and held-out sources contain near-duplicates.", result.ErrorMessage);
    }

    [Fact]
    public void Validate_NegativeNearDuplicateCountIsInvalidProof()
    {
        AssertInvalid(DynamicGraphHeldOutSplitGuard.Validate(BuildProof(nearDuplicateCount: -1)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("simhash ")]
    [InlineData(" simhash")]
    [InlineData("Simhash")]
    [InlineData("MINHASH")]
    [InlineData("sha256")]
    [InlineData("simhash\n")]
    public void Validate_MethodMustBeExactClosedVocabulary(string? method)
    {
        AssertInvalid(DynamicGraphHeldOutSplitGuard.Validate(BuildProof(method: method!)));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(-0.001d)]
    public void Validate_NonFiniteOrNegativeThresholdIsInvalid(double threshold)
    {
        AssertInvalid(DynamicGraphHeldOutSplitGuard.Validate(BuildProof(threshold: threshold)));
    }

    [Fact]
    public void Validate_ZeroThresholdIsAccepted()
    {
        var result = DynamicGraphHeldOutSplitGuard.Validate(BuildProof(threshold: 0d));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0d, result.Value!.NearDuplicateThreshold);
    }

    [Theory]
    [InlineData("train")]
    [InlineData("eval")]
    [InlineData("hashes")]
    public void Validate_OrdinaryCollectionReadFailureIsNormalized(string carrier)
    {
        var faulting = new FaultingReadOnlyList<string>(new InvalidOperationException("read raced"));
        var proof = carrier switch
        {
            "train" => BuildProof(trainSourceRoots: faulting),
            "eval" => BuildProof(evalSourceRoots: faulting),
            "hashes" => BuildProof(sourceHashes: faulting),
            _ => throw new InvalidOperationException()
        };

        AssertInvalid(DynamicGraphHeldOutSplitGuard.Validate(proof));
    }

    [Fact]
    public void Validate_NestedOrdinaryCollectionReadFailureIsNormalized()
    {
        var faulting = new FaultingReadOnlyList<string>(
            new AggregateException(new InvalidOperationException("ordinary nested")));

        AssertInvalid(DynamicGraphHeldOutSplitGuard.Validate(BuildProof(trainSourceRoots: faulting)));
    }

    [Fact]
    public void Validate_MidSnapshotReadRaceIsNormalized()
    {
        var racing = new MidReadFaultingList<string>(
            "first-hash",
            new InvalidOperationException("collection changed during snapshot"));

        AssertInvalid(DynamicGraphHeldOutSplitGuard.Validate(BuildProof(sourceHashes: racing)));
    }

    [Fact]
    public void Validate_SnapshotsEveryCollectionBeforeApplyingStructuralValidation()
    {
        var evalWasRead = false;
        var hashesWereRead = false;
        var proof = BuildProof(
            trainSourceRoots: [" invalid-train-root"],
            evalSourceRoots: new ObservingReadOnlyList<string>(["eval-a"], () => evalWasRead = true),
            sourceHashes: new ObservingReadOnlyList<string>(["hash-a"], () => hashesWereRead = true));

        AssertInvalid(DynamicGraphHeldOutSplitGuard.Validate(proof));
        Assert.True(evalWasRead);
        Assert.True(hashesWereRead);
    }

    [Fact]
    public void Validate_OperationCancellationFromSnapshotEscapes()
    {
        var cancellation = new OperationCanceledException("snapshot cancelled");

        var thrown = Assert.Throws<OperationCanceledException>(() =>
            DynamicGraphHeldOutSplitGuard.Validate(BuildProof(
                trainSourceRoots: new FaultingReadOnlyList<string>(cancellation))));

        Assert.Same(cancellation, thrown);
    }

    [Fact]
    public void Validate_FatalSnapshotFailureEscapes()
    {
        var fatal = new OutOfMemoryException("snapshot fatal");

        var thrown = Assert.Throws<OutOfMemoryException>(() =>
            DynamicGraphHeldOutSplitGuard.Validate(BuildProof(
                evalSourceRoots: new FaultingReadOnlyList<string>(fatal))));

        Assert.Same(fatal, thrown);
    }

    [Fact]
    public void Validate_NestedCancellationSnapshotFailureEscapesCompleteExceptionGraph()
    {
        var cancellation = new OperationCanceledException("nested cancellation");
        var aggregate = new AggregateException(
            new InvalidOperationException("ordinary branch"),
            new InvalidOperationException("wrapper", cancellation));

        var thrown = Assert.Throws<AggregateException>(() =>
            DynamicGraphHeldOutSplitGuard.Validate(BuildProof(
                sourceHashes: new FaultingReadOnlyList<string>(aggregate))));

        Assert.Same(aggregate, thrown);
    }

    [Fact]
    public void Validate_NestedFatalSnapshotFailureEscapesCompleteExceptionGraph()
    {
        var fatal = new OutOfMemoryException("nested fatal");
        var wrapper = new InvalidOperationException("wrapper", fatal);

        var thrown = Assert.Throws<InvalidOperationException>(() =>
            DynamicGraphHeldOutSplitGuard.Validate(BuildProof(
                trainSourceRoots: new FaultingReadOnlyList<string>(wrapper))));

        Assert.Same(wrapper, thrown);
    }

    private static DynamicGraphMeasurementSplitProof BuildProof(
        IReadOnlyList<string>? trainSourceRoots = null,
        IReadOnlyList<string>? evalSourceRoots = null,
        IReadOnlyList<string>? sourceHashes = null,
        int nearDuplicateCount = 0,
        string method = "simhash",
        double threshold = 0.85d)
        => new(
            trainSourceRoots ?? ["train-a"],
            evalSourceRoots ?? ["eval-a"],
            sourceHashes ?? ["hash-a"],
            nearDuplicateCount,
            method,
            threshold);

    private static void AssertInvalid(OperationResult<DynamicGraphMeasurementSplitProof> result)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(InvalidCode, result.ErrorCode);
        Assert.Equal(InvalidMessage, result.ErrorMessage);
    }

    private sealed class FaultingReadOnlyList<T>(Exception exception) : IReadOnlyList<T>
    {
        public int Count => 1;

        public T this[int index] => throw exception;

        public IEnumerator<T> GetEnumerator() => throw exception;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class ObservingReadOnlyList<T>(IReadOnlyList<T> values, Action onRead) : IReadOnlyList<T>
    {
        public int Count => values.Count;

        public T this[int index]
        {
            get
            {
                onRead();
                return values[index];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            onRead();
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class MidReadFaultingList<T>(T firstValue, Exception exception) : IReadOnlyList<T>
    {
        public int Count => 2;

        public T this[int index] => index == 0 ? firstValue : throw exception;

        public IEnumerator<T> GetEnumerator()
        {
            yield return firstValue;
            throw exception;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
