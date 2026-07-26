// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests
{
    using AgentInfrastructure.Logical;
    using LlmFromScratch.DynamicGraphCompositionLiveLocalServer;
    using OrchestratorModel.Logical;

    public sealed class DynamicGraphCompositionSessionRegistryTests
    {
        [Fact]
        public async Task CreateAsync_CancelledWaitPreservesInFlightIdempotencyAndCapacityOwnership()
        {
            var options = new DynamicGraphCompositionServerOptions
            {
                TenantId = "tenant-a",
                MaxSessions = 1,
                MaxConcurrentCompositions = 1,
                MaxConcurrentRuns = 1,
                SessionTtl = TimeSpan.FromMinutes(1)
            };
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            var compositionStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseComposition = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var composeCount = 0;

            async Task<(CompositionEvidenceHttpResponse Evidence, IDynamicGraphLogicalOrchestratorRunSession RunSession)>
                ComposeIgnoringCancellation(CancellationToken _)
            {
                Interlocked.Increment(ref composeCount);
                compositionStarted.TrySetResult();
                await releaseComposition.Task.ConfigureAwait(false);
                return (
                    new CompositionEvidenceHttpResponse("profile-a", 1, "LogicalRegistryProjection", 1, 0, true),
                    new SuccessfulRunSession());
            }

            using var cancelledWait = new CancellationTokenSource();
            var first = registry.CreateAsync(
                options.TenantId,
                "idempotency-a",
                Digest('A'),
                CreateProfile("profile-a"),
                ComposeIgnoringCancellation,
                () => true,
                cancelledWait.Token);
            try
            {
                await compositionStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
                cancelledWait.Cancel();
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first);

                var duplicateWhileInFlight = await registry.CreateAsync(
                    options.TenantId,
                    "idempotency-a",
                    Digest('A'),
                    CreateProfile("profile-a"),
                    ComposeIgnoringCancellation,
                    () => true,
                    CancellationToken.None);

                Assert.Equal(SessionCreationStatus.Pending, duplicateWhileInFlight.Status);
                Assert.Null(duplicateWhileInFlight.Session);
                Assert.Equal(1, Volatile.Read(ref composeCount));

                var differentKeyAtCapacity = await registry.CreateAsync(
                    options.TenantId,
                    "idempotency-b",
                    Digest('B'),
                    CreateProfile("profile-b"),
                    ComposeIgnoringCancellation,
                    () => true,
                    CancellationToken.None);
                Assert.Equal(SessionCreationStatus.CapacityExceeded, differentKeyAtCapacity.Status);
                Assert.Equal(1, Volatile.Read(ref composeCount));
            }
            finally
            {
                releaseComposition.TrySetResult();
            }

            SessionCreationResult replay;
            do
            {
                replay = await registry.CreateAsync(
                    options.TenantId,
                    "idempotency-a",
                    Digest('A'),
                    CreateProfile("profile-a"),
                    ComposeIgnoringCancellation,
                    () => true,
                    CancellationToken.None);
                if (replay.Status == SessionCreationStatus.Pending)
                    await Task.Yield();
            }
            while (replay.Status == SessionCreationStatus.Pending);

            Assert.Equal(SessionCreationStatus.Replayed, replay.Status);
            Assert.NotNull(replay.Session);
            Assert.Equal(1, Volatile.Read(ref composeCount));
        }

        [Theory]
        [InlineData("fault")]
        [InlineData("cancel")]
        public async Task CreateAsync_DetachedUnsuccessfulCompositionReleasesIdempotencyAndCapacityOwnership(
            string completionMode)
        {
            var options = new DynamicGraphCompositionServerOptions
            {
                TenantId = "tenant-a",
                MaxSessions = 1,
                MaxConcurrentCompositions = 1,
                MaxConcurrentRuns = 1,
                SessionTtl = TimeSpan.FromMinutes(1)
            };
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            var compositionStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseComposition = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var unsuccessfulCompletionReached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var sharedCancellation = new CancellationTokenSource();
            var failingComposeCount = 0;
            var recoveryComposeCount = 0;

            async Task<(CompositionEvidenceHttpResponse Evidence, IDynamicGraphLogicalOrchestratorRunSession RunSession)>
                ComposeUnsuccessfully(CancellationToken _)
            {
                Interlocked.Increment(ref failingComposeCount);
                compositionStarted.TrySetResult();
                await releaseComposition.Task.ConfigureAwait(false);
                try
                {
                    if (completionMode == "fault")
                        throw new InvalidOperationException("Expected detached composition failure.");

                    sharedCancellation.Cancel();
                    throw new OperationCanceledException(sharedCancellation.Token);
                }
                finally
                {
                    unsuccessfulCompletionReached.TrySetResult();
                }
            }

            Task<(CompositionEvidenceHttpResponse Evidence, IDynamicGraphLogicalOrchestratorRunSession RunSession)>
                ComposeRecovery(CancellationToken _)
            {
                Interlocked.Increment(ref recoveryComposeCount);
                return Task.FromResult<(
                    CompositionEvidenceHttpResponse Evidence,
                    IDynamicGraphLogicalOrchestratorRunSession RunSession)>(
                    (new CompositionEvidenceHttpResponse(
                        "profile-recovery",
                        1,
                        "LogicalRegistryProjection",
                        1,
                        0,
                        true),
                     new SuccessfulRunSession()));
            }

            using var cancelledWait = new CancellationTokenSource();
            var first = registry.CreateAsync(
                options.TenantId,
                "idempotency-failing",
                Digest('A'),
                CreateProfile("profile-failing"),
                ComposeUnsuccessfully,
                () => true,
                cancelledWait.Token);
            try
            {
                await compositionStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
                cancelledWait.Cancel();
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first);

                releaseComposition.TrySetResult();
                await unsuccessfulCompletionReached.Task.WaitAsync(TimeSpan.FromSeconds(5));

                var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
                SessionCreationResult recovered;
                do
                {
                    recovered = await registry.CreateAsync(
                        options.TenantId,
                        "idempotency-recovery",
                        Digest('B'),
                        CreateProfile("profile-recovery"),
                        ComposeRecovery,
                        () => true,
                        CancellationToken.None);
                    if (recovered.Status == SessionCreationStatus.CapacityExceeded
                        && DateTimeOffset.UtcNow < deadline)
                    {
                        await Task.Delay(10);
                    }
                }
                while (recovered.Status == SessionCreationStatus.CapacityExceeded
                       && DateTimeOffset.UtcNow < deadline);

                Assert.Contains(
                    recovered.Status,
                    new[] { SessionCreationStatus.Created, SessionCreationStatus.Replayed });
                Assert.NotNull(recovered.Session);
                Assert.Equal(1, Volatile.Read(ref failingComposeCount));
                Assert.Equal(1, Volatile.Read(ref recoveryComposeCount));
            }
            finally
            {
                releaseComposition.TrySetResult();
            }
        }

        private static string Digest(char value) => new(value, 64);

        private static DynamicGraphLogicalOrchestratorBootstrapProfile CreateProfile(string profileId)
            => new(
                profileId,
                "tenant-a",
                "domain-a",
                "context-a",
                "signal-a",
                "source-a",
                "trace-a",
                [],
                [],
                new TopologyBuildBudget(1, 1, 1, 1));

        private sealed class SuccessfulRunSession : IDynamicGraphLogicalOrchestratorRunSession
        {
            public Task<AgentInfrastructure.OperationResult<DynamicGraphLogicalOrchestratorRunResult>> RunAsync(
                DynamicGraphLogicalOrchestratorRunRequest request,
                CancellationToken cancellationToken = default)
                => Task.FromResult(AgentInfrastructure.OperationResult<DynamicGraphLogicalOrchestratorRunResult>.Ok(
                    new DynamicGraphLogicalOrchestratorRunResult(
                        "scenario",
                        "flow",
                        "PASS",
                        "ok",
                        new Dictionary<string, string>(StringComparer.Ordinal),
                        [])));
        }
    }
}

namespace LlmFromScratch.DynamicGraphCompositionLiveLocalServer
{
    using OrchestratorModel.Logical;

    public sealed record DynamicGraphCompositionServerOptions
    {
        public required string TenantId { get; init; }
        public int MaxSessions { get; init; } = 64;
        public int MaxConcurrentCompositions { get; init; } = 2;
        public int MaxConcurrentRuns { get; init; } = 4;
        public TimeSpan SessionTtl { get; init; } = TimeSpan.FromMinutes(15);

        public void Validate()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(TenantId);
            if (MaxSessions <= 0 || MaxConcurrentCompositions <= 0 || MaxConcurrentRuns <= 0 || SessionTtl <= TimeSpan.Zero)
                throw new InvalidOperationException("The test host options are invalid.");
        }
    }

    public sealed record CompositionEvidenceHttpResponse(
        string ProfileId,
        long Revision,
        string SourceKind,
        int NodeCount,
        int EdgeCount,
        bool InitialRevisionAdvanced);

    internal static class DynamicGraphCompositionProfilePolicy
    {
        internal static TestAdmission ValidateOwnedSnapshot(
            DynamicGraphLogicalOrchestratorBootstrapProfile profile,
            string tenantId)
            => new(true, new string('A', 64));
    }

    internal sealed record TestAdmission(bool IsAccepted, string CanonicalDigest);

    internal sealed record PersistedCompositionSession(
        string Schema,
        string SessionId,
        string TenantIndex,
        string OpaqueIdempotencyIndex,
        string CanonicalDigest,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset ExpiresAtUtc,
        DynamicGraphLogicalOrchestratorBootstrapProfile Profile,
        CompositionEvidenceHttpResponse Evidence);

    public sealed class DynamicGraphCompositionSessionPersistence : IDisposable
    {
        internal const string CheckpointSchema = "test-disabled";

        public DynamicGraphCompositionSessionPersistence(DynamicGraphCompositionServerOptions options)
        {
        }

        public bool IsEnabled => false;

        internal void PersistNew(PersistedCompositionSession session)
        {
        }

        internal void Delete(string sessionId)
        {
        }

        internal IReadOnlyList<PersistedCompositionSession> ReadAll(int maxSessions)
            => [];

        internal string DeriveIndex(params string[] components)
            => Convert.ToHexStringLower(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(string.Join("\u001f", components))));

        internal static DynamicGraphLogicalOrchestratorBootstrapProfile SnapshotProfile(
            DynamicGraphLogicalOrchestratorBootstrapProfile profile)
            => profile with
            {
                NodeDefinitions = [.. profile.NodeDefinitions],
                EdgeBindings = [.. profile.EdgeBindings]
            };

        public void Dispose()
        {
        }
    }

    public sealed class DynamicGraphCompositionPersistenceException : Exception
    {
        public DynamicGraphCompositionPersistenceException(string message) : base(message)
        {
        }
    }
}
