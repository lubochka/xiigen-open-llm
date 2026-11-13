// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using ContinuousLearning;
using DesignConversionDomain;

public sealed class
    DesignConversionLocalIndependentPreferenceEvaluatorRuntimeWiringTests
{
    private const string ModelId = "qwen-runtime-wiring-test:9b";
    private static readonly string ManifestDigest = new('a', 64);
    private static readonly string ModelLayerDigest = new('b', 64);

    [Fact]
    public void DefaultOff_ArtifactDbHasNoIndependentPreferenceAuthority()
    {
        var artifactDb = CreateArtifactDb("default-off");

        var registration =
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                artifactDb);

        Assert.Null(registration);
    }

    [Fact]
    public void ExplicitRuntime_BindsExactRootAndDisposeRevokesAssociationAndClient()
    {
        var artifactDb = CreateArtifactDb("owned");
        var unrelatedArtifactDb = CreateArtifactDb("unrelated");
        var client = new TrackingEvaluationClient();
        var evaluator = CreateEvaluator(client, "owned-runtime");
        var created = DesignConversionLocalIndependentPreferenceEvaluatorRuntime
            .OwnForTesting(artifactDb, evaluator);
        Assert.True(created.IsSuccess, created.ErrorMessage);
        var runtime = created.Value!;

        var registration =
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                artifactDb);
        Assert.NotNull(registration);
        Assert.Same(evaluator, registration.Source);
        Assert.Equal(runtime.ArtifactRootBinding,
            registration.ArtifactRootBinding);
        Assert.Null(
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                unrelatedArtifactDb));
        Assert.False(runtime.IsDisposed);
        Assert.False(client.Disposed);

        runtime.Dispose();

        Assert.True(runtime.IsDisposed);
        Assert.True(client.Disposed);
        Assert.Null(
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                artifactDb));
    }

    [Fact]
    public async Task ConcurrentSameRootOwnerFailsClosedWithoutReplacingAuthority()
    {
        var sharedRoot = CreateRoot("same-root");
        var firstArtifactDb = new JsonlDesignConversionLocalArtifactDb(sharedRoot);
        var secondArtifactDb = new JsonlDesignConversionLocalArtifactDb(
            OperatingSystem.IsWindows()
                ? sharedRoot.ToUpperInvariant()
                : sharedRoot);
        var firstClient = new TrackingEvaluationClient();
        var secondClient = new TrackingEvaluationClient();
        var firstEvaluator = CreateEvaluator(firstClient, "same-root-first");
        var secondEvaluator = CreateEvaluator(secondClient, "same-root-second");
        var readyCount = 0;
        var release = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        async Task<OperationResult<
            DesignConversionLocalIndependentPreferenceEvaluatorRuntime>> OwnAsync(
                IDesignConversionLocalArtifactDb artifactDb,
                DesignConversionLocalIndependentPreferenceEvaluator evaluator)
        {
            if (Interlocked.Increment(ref readyCount) == 2)
                release.TrySetResult();
            await release.Task;
            return await Task.Run(() =>
                DesignConversionLocalIndependentPreferenceEvaluatorRuntime
                    .OwnForTesting(artifactDb, evaluator));
        }

        var attempts = await Task.WhenAll(
            OwnAsync(firstArtifactDb, firstEvaluator),
            OwnAsync(secondArtifactDb, secondEvaluator));
        var winnerIndex = Assert.Single(
            Enumerable.Range(0, attempts.Length),
            index => attempts[index].IsSuccess);
        var loserIndex = 1 - winnerIndex;
        var winner = attempts[winnerIndex].Value!;
        var loser = attempts[loserIndex];
        var winnerArtifactDb = winnerIndex == 0
            ? firstArtifactDb
            : secondArtifactDb;
        var loserArtifactDb = loserIndex == 0
            ? firstArtifactDb
            : secondArtifactDb;
        var winnerEvaluator = winnerIndex == 0
            ? firstEvaluator
            : secondEvaluator;
        var loserEvaluator = loserIndex == 0
            ? firstEvaluator
            : secondEvaluator;
        var loserClient = loserIndex == 0 ? firstClient : secondClient;

        Assert.False(loser.IsSuccess);
        Assert.Equal(
            DesignConversionLocalIndependentPreferenceEvaluatorRuntime
                .ArtifactRootAlreadyOwnedErrorCode,
            loser.ErrorCode);
        Assert.Null(loser.Value);
        Assert.Same(
            winnerEvaluator,
            DesignConversionIndependentPreferenceEvidenceAssociation
                .Resolve(winnerArtifactDb)!.Source);
        Assert.NotSame(
            loserEvaluator,
            DesignConversionIndependentPreferenceEvidenceAssociation
                .Resolve(winnerArtifactDb)!.Source);
        Assert.Null(
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                loserArtifactDb));
        Assert.True(loserClient.Disposed);

        winner.Dispose();

        Assert.Null(
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                winnerArtifactDb));
        Assert.Null(
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                loserArtifactDb));
    }

    [Fact]
    public void DistinctRootsOwnDistinctEvaluatorCapabilitiesAndDisposeIndependently()
    {
        var firstArtifactDb = CreateArtifactDb("root-a");
        var secondArtifactDb = CreateArtifactDb("root-b");
        var firstEvaluator = CreateEvaluator(
            new TrackingEvaluationClient(),
            "distinct-first");
        var secondEvaluator = CreateEvaluator(
            new TrackingEvaluationClient(),
            "distinct-second");
        var first = DesignConversionLocalIndependentPreferenceEvaluatorRuntime
            .OwnForTesting(firstArtifactDb, firstEvaluator).Value!;
        using var second = DesignConversionLocalIndependentPreferenceEvaluatorRuntime
            .OwnForTesting(secondArtifactDb, secondEvaluator).Value!;

        Assert.NotEqual(first.ArtifactRootBinding, second.ArtifactRootBinding);
        Assert.NotSame(
            DesignConversionIndependentPreferenceEvidenceAssociation
                .Resolve(firstArtifactDb)!.Source,
            DesignConversionIndependentPreferenceEvidenceAssociation
                .Resolve(secondArtifactDb)!.Source);

        first.Dispose();

        Assert.Null(
            DesignConversionIndependentPreferenceEvidenceAssociation.Resolve(
                firstArtifactDb));
        Assert.Same(
            secondEvaluator,
            DesignConversionIndependentPreferenceEvidenceAssociation
                .Resolve(secondArtifactDb)!.Source);
    }

    private static JsonlDesignConversionLocalArtifactDb CreateArtifactDb(
        string suffix)
        => new(CreateRoot(suffix));

    private static string CreateRoot(string suffix)
        => Path.Combine(
            Path.GetTempPath(),
            "dclie-runtime-" + suffix + "-" + Guid.NewGuid().ToString("N"));

    private static DesignConversionLocalIndependentPreferenceEvaluator
        CreateEvaluator(
            ILegalTeacherEvaluationInferenceClient client,
            string runtimeSuffix)
        => DesignConversionLocalIndependentPreferenceEvaluator.OwnForTesting(
            client,
            ModelId,
            ManifestDigest,
            ModelLayerDigest,
            DesignConversionDpoGenerator.Sha256Ref(runtimeSuffix));

    private sealed class TrackingEvaluationClient
        : ILegalTeacherEvaluationInferenceClient
    {
        public bool Disposed { get; private set; }

        public Task<OperationResult<TeacherEvaluationInferenceResult>>
            EvaluateAsync(
                string prompt,
                ILegalTeacherCallPermit callPermit,
                CancellationToken cancellationToken = default)
            => throw new InvalidOperationException(
                "This lifetime test must not invoke local inference.");

        public void Dispose() => Disposed = true;
    }
}
