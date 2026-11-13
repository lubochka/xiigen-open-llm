// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests
{
    using AgentInfrastructure;
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

        [Fact]
        public async Task RunOnCurrentTopologyAsync_UsesOnlyTopologyCapabilityAndPreservesOwnedResult()
        {
            var options = Options();
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            var session = new DualRunSession();
            var sessionId = await CreateSessionAsync(registry, options, session, "topology-dual");

            var executed = await registry.RunOnCurrentTopologyAsync(
                options.TenantId,
                sessionId,
                new DynamicGraphLogicalOrchestratorRunRequest("topology request"),
                () => true,
                CancellationToken.None);

            Assert.Equal(TopologySessionExecutionStatus.Completed, executed.Status);
            var operation = Assert.IsType<
                OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>>(
                executed.Result);
            Assert.True(operation.IsSuccess);
            var result = Assert.IsType<DynamicGraphLogicalOrchestratorTopologyRunResult>(
                operation.Value);
            Assert.Equal(7, result.TopologyRevision);
            Assert.Equal("topology-signal-7", result.TopologySignalId);
            Assert.Equal(DynamicGraphBuildSourceKind.LogicalRegistryProjection, result.TopologySourceKind);
            Assert.Equal("topology-scenario", result.Run.ScenarioId);
            Assert.Equal("topology-flow", result.Run.SelectedFlowId);
            Assert.Equal(1, session.TopologyRunCount);
            Assert.Equal(0, session.LegacyRunCount);
        }

        [Fact]
        public async Task RunOnCurrentTopologyAsync_LegacyOnlySessionRemainsAvailableForOrdinaryRuns()
        {
            var options = Options();
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            var session = new SuccessfulRunSession();
            var sessionId = await CreateSessionAsync(registry, options, session, "legacy-only");

            var topology = await registry.RunOnCurrentTopologyAsync(
                options.TenantId,
                sessionId,
                new DynamicGraphLogicalOrchestratorRunRequest("topology request"),
                () => true,
                CancellationToken.None);
            var legacy = await registry.RunAsync(
                options.TenantId,
                sessionId,
                new DynamicGraphLogicalOrchestratorRunRequest("legacy request"),
                () => true,
                CancellationToken.None);

            Assert.Equal(TopologySessionExecutionStatus.Unavailable, topology.Status);
            Assert.Null(topology.Result);
            Assert.Equal(SessionExecutionStatus.Completed, legacy.Status);
            Assert.True(Assert.IsType<OperationResult<DynamicGraphLogicalOrchestratorRunResult>>(
                legacy.Result).IsSuccess);
        }

        [Fact]
        public async Task TopologyAndLegacyRuns_ShareTheSameBoundedSlotInBothDirections()
        {
            var options = Options();
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            var session = new BlockingDualRunSession();
            var sessionId = await CreateSessionAsync(registry, options, session, "shared-slot");
            var request = new DynamicGraphLogicalOrchestratorRunRequest("shared slot request");

            var topologyHolder = registry.RunOnCurrentTopologyAsync(
                options.TenantId,
                sessionId,
                request,
                () => true,
                CancellationToken.None);
            await session.TopologyStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var legacyBusy = await registry.RunAsync(
                options.TenantId,
                sessionId,
                request,
                () => true,
                CancellationToken.None);
            Assert.Equal(SessionExecutionStatus.Busy, legacyBusy.Status);
            session.ReleaseTopology.TrySetResult();
            Assert.Equal(
                TopologySessionExecutionStatus.Completed,
                (await topologyHolder.WaitAsync(TimeSpan.FromSeconds(5))).Status);

            var legacyHolder = registry.RunAsync(
                options.TenantId,
                sessionId,
                request,
                () => true,
                CancellationToken.None);
            await session.LegacyStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var topologyBusy = await registry.RunOnCurrentTopologyAsync(
                options.TenantId,
                sessionId,
                request,
                () => true,
                CancellationToken.None);
            Assert.Equal(TopologySessionExecutionStatus.Busy, topologyBusy.Status);
            session.ReleaseLegacy.TrySetResult();
            Assert.Equal(
                SessionExecutionStatus.Completed,
                (await legacyHolder.WaitAsync(TimeSpan.FromSeconds(5))).Status);
        }

        [Fact]
        public async Task RunOnCurrentTopologyAsync_MapsUnknownExpiredAndForbiddenWithoutInvocation()
        {
            var now = DateTimeOffset.Parse("2026-07-27T00:00:00Z");
            var time = new MutableTimeProvider(now);
            var options = Options() with { SessionTtl = TimeSpan.FromSeconds(1) };
            using var registry = new DynamicGraphCompositionSessionRegistry(options, time);
            var session = new DualRunSession();
            var sessionId = await CreateSessionAsync(registry, options, session, "status-map");
            var request = new DynamicGraphLogicalOrchestratorRunRequest("status request");

            var unknown = await registry.RunOnCurrentTopologyAsync(
                options.TenantId,
                "missing-session",
                request,
                () => true,
                CancellationToken.None);
            var forbidden = await registry.RunOnCurrentTopologyAsync(
                options.TenantId,
                sessionId,
                request,
                () => false,
                CancellationToken.None);
            time.Advance(TimeSpan.FromMilliseconds(1500));
            var expired = await registry.RunOnCurrentTopologyAsync(
                options.TenantId,
                sessionId,
                request,
                () => true,
                CancellationToken.None);

            Assert.Equal(TopologySessionExecutionStatus.Unknown, unknown.Status);
            Assert.Equal(TopologySessionExecutionStatus.ForbiddenAssembly, forbidden.Status);
            Assert.Equal(TopologySessionExecutionStatus.Expired, expired.Status);
            Assert.Equal(0, session.TopologyRunCount);
            Assert.Equal(0, session.LegacyRunCount);
        }

        [Fact]
        public async Task RunOnCurrentTopologyAsync_NullSuccessNullFailureAndNonFatalExceptionReleaseSlot()
        {
            var options = Options();
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            var session = new ShapeChangingTopologyRunSession();
            var sessionId = await CreateSessionAsync(registry, options, session, "shape-release");
            var request = new DynamicGraphLogicalOrchestratorRunRequest("shape request");

            session.TopologyHandler = (_, _) => Task.FromResult<
                OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>>(null!);
            var nullEnvelope = await registry.RunOnCurrentTopologyAsync(
                options.TenantId, sessionId, request, () => true, CancellationToken.None);
            Assert.Equal(TopologySessionExecutionStatus.Completed, nullEnvelope.Status);
            Assert.Null(nullEnvelope.Result);
            await AssertLegacySlotAvailableAsync(registry, options, sessionId, request);

            session.TopologyHandler = (_, _) => Task.FromResult(
                OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>.Ok(null!));
            var successNull = await registry.RunOnCurrentTopologyAsync(
                options.TenantId, sessionId, request, () => true, CancellationToken.None);
            Assert.Equal(TopologySessionExecutionStatus.Completed, successNull.Status);
            Assert.True(Assert.IsType<
                OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>>(
                successNull.Result).IsSuccess);
            Assert.Null(successNull.Result!.Value);
            await AssertLegacySlotAvailableAsync(registry, options, sessionId, request);

            session.TopologyHandler = (_, _) => Task.FromResult(
                OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>.Fail(
                    "topology failed",
                    "TOPOLOGY_FAILED"));
            var failure = await registry.RunOnCurrentTopologyAsync(
                options.TenantId, sessionId, request, () => true, CancellationToken.None);
            Assert.Equal(TopologySessionExecutionStatus.Completed, failure.Status);
            Assert.False(Assert.IsType<
                OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>>(
                failure.Result).IsSuccess);
            Assert.Equal("TOPOLOGY_FAILED", failure.Result!.ErrorCode);
            await AssertLegacySlotAvailableAsync(registry, options, sessionId, request);

            session.TopologyHandler = (_, _) =>
                throw new InvalidOperationException("expected nonfatal topology failure");
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                registry.RunOnCurrentTopologyAsync(
                    options.TenantId, sessionId, request, () => true, CancellationToken.None));
            await AssertLegacySlotAvailableAsync(registry, options, sessionId, request);
        }

        [Fact]
        public async Task RunOnCurrentTopologyAsync_CancellationAndArgumentFailuresDoNotConsumeSlot()
        {
            var options = Options();
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            var session = new DualRunSession();
            var sessionId = await CreateSessionAsync(registry, options, session, "cancel-arguments");
            var request = new DynamicGraphLogicalOrchestratorRunRequest("cancel request");

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                registry.RunOnCurrentTopologyAsync(
                    options.TenantId,
                    sessionId,
                    null!,
                    () => true,
                    CancellationToken.None));
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                registry.RunOnCurrentTopologyAsync(
                    options.TenantId,
                    sessionId,
                    request,
                    null!,
                    CancellationToken.None));

            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                registry.RunOnCurrentTopologyAsync(
                    options.TenantId,
                    sessionId,
                    request,
                    () => true,
                    cancelled.Token));
            await AssertLegacySlotAvailableAsync(registry, options, sessionId, request);
            Assert.Equal(0, session.TopologyRunCount);
        }

        [Fact]
        public void TopologyExecutionResult_HasExactIndependentSixStateSurface()
        {
            Assert.Equal(
                ["Completed", "Expired", "Unknown", "Busy", "ForbiddenAssembly", "Unavailable"],
                Enum.GetNames<TopologySessionExecutionStatus>());
            Assert.Equal(
                ["Completed", "Expired", "Unknown", "Busy", "ForbiddenAssembly"],
                Enum.GetNames<SessionExecutionStatus>());
        }

        [Fact]
        public void TopologyMutationResult_HasExactIndependentSevenStateSurface()
        {
            Assert.False(typeof(TopologyMutationSessionStatus).IsPublic);
            Assert.True(typeof(TopologyMutationSessionStatus).IsNotPublic);
            Assert.False(typeof(TopologyMutationSessionResult).IsPublic);
            Assert.True(typeof(TopologyMutationSessionResult).IsNotPublic);
            Assert.Equal(
                [
                    "Completed",
                    "Expired",
                    "Unknown",
                    "Busy",
                    "ForbiddenAssembly",
                    "Unavailable",
                    "EphemeralModeRequired"
                ],
                Enum.GetNames<TopologyMutationSessionStatus>());
            Assert.Equal(
                ["Completed", "Expired", "Unknown", "Busy", "ForbiddenAssembly", "Unavailable"],
                Enum.GetNames<TopologySessionExecutionStatus>());
            Assert.Equal(
                ["Completed", "Expired", "Unknown", "Busy", "ForbiddenAssembly"],
                Enum.GetNames<SessionExecutionStatus>());
        }

        [Fact]
        public async Task ApplyTopologyMutationAsync_RequiresEphemeralModeBeforeAdmissionOrApply()
        {
            var options = Options();
            using var persistence = new DynamicGraphCompositionSessionPersistence(
                options,
                enabled: true);
            using var registry = new DynamicGraphCompositionSessionRegistry(
                options,
                persistence,
                TimeProvider.System);
            var session = new MutationSession("profile-persistent");
            var sessionId = await CreateSessionAsync(
                registry,
                options,
                session,
                "persistent");
            var malformed = CreateProfile("profile-persistent") with
            {
                NodeDefinitions = null!
            };

            var result = await registry.ApplyTopologyMutationAsync(
                options.TenantId,
                sessionId,
                1,
                malformed,
                () => true,
                CancellationToken.None);

            Assert.Equal(TopologyMutationSessionStatus.EphemeralModeRequired, result.Status);
            Assert.Null(result.Result);
            Assert.Equal(0, session.ApplyCount);
        }

        [Fact]
        public async Task ApplyTopologyMutationAsync_RequiresBothCapabilitiesAndExactSessionScope()
        {
            var options = Options();
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            var mutationOnlyId = await CreateSessionAsync(
                registry,
                options,
                new MutationOnlyRunSession(),
                "mutation-only");
            var unavailable = await registry.ApplyTopologyMutationAsync(
                options.TenantId,
                mutationOnlyId,
                1,
                CreateProfile("profile-mutation-only"),
                () => true,
                CancellationToken.None);
            Assert.Equal(TopologyMutationSessionStatus.Unavailable, unavailable.Status);

            var profileMismatchSession = new MutationSession("other-profile");
            var profileMismatchId = await CreateSessionAsync(
                registry,
                options,
                profileMismatchSession,
                "profile-mismatch");
            var profileMismatch = await registry.ApplyTopologyMutationAsync(
                options.TenantId,
                profileMismatchId,
                1,
                CreateProfile("profile-profile-mismatch"),
                () => true,
                CancellationToken.None);
            Assert.Equal(TopologyMutationSessionStatus.Completed, profileMismatch.Status);
            Assert.Equal(
                "HOST_TOPOLOGY_MUTATION_PROFILE_ID_MISMATCH",
                Assert.IsType<OperationResult<DynamicGraphTopologyApplyResult>>(
                    profileMismatch.Result).ErrorCode);
            Assert.Equal(0, profileMismatchSession.ApplyCount);

            var scopeSession = new MutationSession(
                "profile-scope-mismatch",
                domainTag: "other-domain");
            var scopeId = await CreateSessionAsync(
                registry,
                options,
                scopeSession,
                "scope-mismatch");
            var scopeMismatch = await registry.ApplyTopologyMutationAsync(
                options.TenantId,
                scopeId,
                1,
                CreateProfile("profile-scope-mismatch"),
                () => true,
                CancellationToken.None);
            Assert.Equal(
                "HOST_TOPOLOGY_MUTATION_SCOPE_MISMATCH",
                Assert.IsType<OperationResult<DynamicGraphTopologyApplyResult>>(
                    scopeMismatch.Result).ErrorCode);
            Assert.Equal(0, scopeSession.ApplyCount);
        }

        [Fact]
        public async Task ApplyTopologyMutationAsync_MapsInvalidForbiddenUnknownAndExpiredBeforeApply()
        {
            var now = DateTimeOffset.Parse("2026-07-27T00:00:00Z");
            var time = new MutableTimeProvider(now);
            var options = Options() with { SessionTtl = TimeSpan.FromSeconds(1) };
            using var registry = new DynamicGraphCompositionSessionRegistry(options, time);
            var session = new MutationSession("profile-status");
            var sessionId = await CreateSessionAsync(registry, options, session, "status");

            var invalid = await registry.ApplyTopologyMutationAsync(
                options.TenantId,
                sessionId,
                1,
                CreateProfile("profile-status") with { EdgeBindings = null! },
                () => true,
                CancellationToken.None);
            Assert.Equal(TopologyMutationSessionStatus.Completed, invalid.Status);
            Assert.Equal(
                "HOST_TOPOLOGY_MUTATION_PROFILE_INVALID",
                invalid.Result!.ErrorCode);

            var forbidden = await registry.ApplyTopologyMutationAsync(
                options.TenantId,
                sessionId,
                1,
                CreateProfile("profile-status"),
                () => false,
                CancellationToken.None);
            Assert.Equal(TopologyMutationSessionStatus.ForbiddenAssembly, forbidden.Status);

            var unknown = await registry.ApplyTopologyMutationAsync(
                options.TenantId,
                "missing-session",
                1,
                CreateProfile("profile-status"),
                () => true,
                CancellationToken.None);
            Assert.Equal(TopologyMutationSessionStatus.Unknown, unknown.Status);

            time.Advance(TimeSpan.FromMilliseconds(1500));
            var expired = await registry.ApplyTopologyMutationAsync(
                options.TenantId,
                sessionId,
                1,
                CreateProfile("profile-status"),
                () => true,
                CancellationToken.None);
            Assert.Equal(TopologyMutationSessionStatus.Expired, expired.Status);
            Assert.Equal(0, session.ApplyCount);
        }

        [Fact]
        public async Task ApplyTopologyMutationAsync_OwnsProjectionAndInvokesExactlyOnce()
        {
            var options = Options();
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            var session = new MutationSession("profile-owned");
            var sessionId = await CreateSessionAsync(registry, options, session, "owned");
            var transitions = new[] { "transition-a" };
            var exclusions = new[] { "candidate_index" };
            var descriptor = new DynamicGraphNodeDescriptor(
                "node-a",
                "domain-a",
                "System.String",
                "System.String",
                false,
                false,
                false,
                "baseline_candidate_generator",
                null,
                null);
            var profile = CreateProfile("profile-owned") with
            {
                SignalId = "mutation-signal",
                NodeDefinitions =
                [
                    new DynamicGraphLogicalOrchestratorNodeDefinition("sub-phase-a", descriptor)
                ],
                EdgeBindings =
                [
                    new DynamicGraphRegistryEdgeBinding(
                        "edge-a",
                        transitions,
                        GraphEdgeKind.FanOut,
                        false,
                        "authority-a",
                        "L2",
                        false,
                        "baseline-a",
                        "candidate_generator",
                        exclusions,
                        "source-a",
                        "cutover-a",
                        "ablation-a")
                ],
                Budget = new TopologyBuildBudget(3, 3, 3, 3)
            };

            var executed = await registry.ApplyTopologyMutationAsync(
                options.TenantId,
                sessionId,
                1,
                profile,
                () => true,
                CancellationToken.None);

            Assert.Equal(TopologyMutationSessionStatus.Completed, executed.Status);
            Assert.Equal("DOWNSTREAM_SENTINEL", executed.Result!.ErrorCode);
            Assert.Equal(1, session.ApplyCount);
            Assert.Equal(1, session.ExpectedRevision);
            Assert.NotSame(profile.Budget, session.Budget);
            Assert.NotSame(profile.NodeDefinitions, session.Request!.NodeBindings);
            Assert.NotSame(profile.EdgeBindings, session.Request.EdgeBindings);
            Assert.NotSame(descriptor, session.Request.NodeBindings[0].Node.Descriptor);
            Assert.Equal(["transition-a"], session.Request.EdgeBindings[0].LogicalTransitionIds);
            Assert.Equal(["candidate_index"], session.Request.EdgeBindings[0].ExcludedLeakageSignals);
            Assert.Equal(
                [typeof(IDynamicGraphNode)],
                session.Request.NodeBindings[0].Node.GetType().GetInterfaces());

            transitions[0] = "caller-mutated";
            exclusions[0] = "caller-mutated";
            Assert.Equal(["transition-a"], session.Request.EdgeBindings[0].LogicalTransitionIds);
            Assert.Equal(["candidate_index"], session.Request.EdgeBindings[0].ExcludedLeakageSignals);
        }

        [Fact]
        public async Task ApplyTopologyMutationAsync_SharedSlotCancellationAndNoPostApplyReadinessMask()
        {
            var options = Options();
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            using var cancelImmediatelyBeforeApply = new CancellationTokenSource();
            var cancellationSession = new MutationSession(
                "profile-cancel",
                descriptorRead: count =>
                {
                    if (count == 2)
                        cancelImmediatelyBeforeApply.Cancel();
                });
            var cancellationId = await CreateSessionAsync(
                registry,
                options,
                cancellationSession,
                "cancel");
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                registry.ApplyTopologyMutationAsync(
                    options.TenantId,
                    cancellationId,
                    1,
                    CreateProfile("profile-cancel"),
                    () => true,
                    cancelImmediatelyBeforeApply.Token));
            Assert.Equal(0, cancellationSession.ApplyCount);
            await AssertLegacySlotAvailableAsync(
                registry,
                options,
                cancellationId,
                new DynamicGraphLogicalOrchestratorRunRequest("after cancellation"));

            var readinessCalls = 0;
            var completedSession = new MutationSession("profile-readiness");
            var completedId = await CreateSessionAsync(
                registry,
                options,
                completedSession,
                "readiness");
            var completed = await registry.ApplyTopologyMutationAsync(
                options.TenantId,
                completedId,
                1,
                CreateProfile("profile-readiness"),
                () => Interlocked.Increment(ref readinessCalls) <= 2,
                CancellationToken.None);
            Assert.Equal(TopologyMutationSessionStatus.Completed, completed.Status);
            Assert.Equal(2, readinessCalls);
            Assert.Equal(1, completedSession.ApplyCount);
        }

        [Fact]
        public async Task TopologyMutationAndLegacyRun_ShareTheSameBoundedSlotInBothDirections()
        {
            var options = Options();
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            var mutationStarted = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseMutation = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var runStarted = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseRun = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var session = new MutationSession("profile-shared")
            {
                MutationHandler = (_, _, _) =>
                {
                    mutationStarted.TrySetResult();
                    releaseMutation.Task.GetAwaiter().GetResult();
                    return OperationResult<DynamicGraphTopologyApplyResult>.Fail(
                        "sentinel",
                        "DOWNSTREAM_SENTINEL");
                }
            };
            var sessionId = await CreateSessionAsync(
                registry,
                options,
                session,
                "shared");
            var mutationHolder = Task.Run(() => registry.ApplyTopologyMutationAsync(
                options.TenantId,
                sessionId,
                1,
                CreateProfile("profile-shared"),
                () => true,
                CancellationToken.None));
            await mutationStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var legacyBusy = await registry.RunAsync(
                options.TenantId,
                sessionId,
                new DynamicGraphLogicalOrchestratorRunRequest("legacy busy"),
                () => true,
                CancellationToken.None);
            Assert.Equal(SessionExecutionStatus.Busy, legacyBusy.Status);
            releaseMutation.TrySetResult();
            Assert.Equal(
                TopologyMutationSessionStatus.Completed,
                (await mutationHolder.WaitAsync(TimeSpan.FromSeconds(5))).Status);

            session.RunHandler = async (_, cancellationToken) =>
            {
                runStarted.TrySetResult();
                await releaseRun.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                return OperationResult<DynamicGraphLogicalOrchestratorRunResult>.Ok(
                    new DynamicGraphLogicalOrchestratorRunResult(
                        "scenario",
                        "flow",
                        "PASS",
                        "ok",
                        new Dictionary<string, string>(),
                        []));
            };
            var legacyHolder = registry.RunAsync(
                options.TenantId,
                sessionId,
                new DynamicGraphLogicalOrchestratorRunRequest("legacy holder"),
                () => true,
                CancellationToken.None);
            await runStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var mutationBusy = await registry.ApplyTopologyMutationAsync(
                options.TenantId,
                sessionId,
                1,
                CreateProfile("profile-shared"),
                () => true,
                CancellationToken.None);
            Assert.Equal(TopologyMutationSessionStatus.Busy, mutationBusy.Status);
            releaseRun.TrySetResult();
            Assert.Equal(
                SessionExecutionStatus.Completed,
                (await legacyHolder.WaitAsync(TimeSpan.FromSeconds(5))).Status);
        }

        [Fact]
        public async Task ApplyTopologyMutationAsync_SameRevisionWritersApplyOnceAndConflictOnce()
        {
            var options = Options() with { MaxConcurrentRuns = 2 };
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            using var ready = new CountdownEvent(2);
            using var start = new ManualResetEventSlim(false);
            var currentRevision = 1L;
            var session = new MutationSession("profile-cas")
            {
                MutationHandler = (expectedRevision, _, _) =>
                {
                    ready.Signal();
                    start.Wait();
                    lock (ready)
                    {
                        if (expectedRevision != currentRevision)
                        {
                            return OperationResult<DynamicGraphTopologyApplyResult>.Fail(
                                "conflict",
                                "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_REVISION_CONFLICT");
                        }
                        currentRevision++;
                        return OperationResult<DynamicGraphTopologyApplyResult>.Ok(
                            new DynamicGraphTopologyApplyResult(
                                1,
                                2,
                                true,
                                null!,
                                null));
                    }
                }
            };
            var sessionId = await CreateSessionAsync(registry, options, session, "cas");
            Task<TopologyMutationSessionResult> Writer() => Task.Run(() =>
                registry.ApplyTopologyMutationAsync(
                    options.TenantId,
                    sessionId,
                    1,
                    CreateProfile("profile-cas"),
                    () => true,
                    CancellationToken.None));

            var first = Writer();
            var second = Writer();
            Assert.True(ready.Wait(TimeSpan.FromSeconds(5)));
            start.Set();
            var results = await Task.WhenAll(first, second);

            Assert.Equal(2, session.ApplyCount);
            Assert.Single(results, result => result.Result?.IsSuccess == true);
            Assert.Single(
                results,
                result => result.Result is
                {
                    IsSuccess: false,
                    ErrorCode: "DYNAMIC_GRAPH_TOPOLOGY_RUNTIME_REVISION_CONFLICT"
                });
        }

        [Fact]
        public async Task ApplyTopologyMutationAsync_NullFailureAndFatalShapesHaveExactSlotPolicy()
        {
            var options = Options();
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            var session = new MutationSession("profile-shapes");
            var sessionId = await CreateSessionAsync(
                registry,
                options,
                session,
                "shapes");

            session.MutationHandler = (_, _, _) => null!;
            var nullEnvelope = await registry.ApplyTopologyMutationAsync(
                options.TenantId,
                sessionId,
                1,
                CreateProfile("profile-shapes"),
                () => true,
                CancellationToken.None);
            Assert.Equal(TopologyMutationSessionStatus.Completed, nullEnvelope.Status);
            Assert.Null(nullEnvelope.Result);

            session.MutationHandler = (_, _, _) =>
                OperationResult<DynamicGraphTopologyApplyResult>.Ok(null!);
            var successfulNull = await registry.ApplyTopologyMutationAsync(
                options.TenantId,
                sessionId,
                1,
                CreateProfile("profile-shapes"),
                () => true,
                CancellationToken.None);
            Assert.Equal(TopologyMutationSessionStatus.Completed, successfulNull.Status);
            Assert.True(successfulNull.Result!.IsSuccess);
            Assert.Null(successfulNull.Result.Value);

            session.MutationHandler = (_, _, _) =>
                OperationResult<DynamicGraphTopologyApplyResult>.Fail("failed", "FAILED");
            var failure = await registry.ApplyTopologyMutationAsync(
                options.TenantId,
                sessionId,
                1,
                CreateProfile("profile-shapes"),
                () => true,
                CancellationToken.None);
            Assert.Equal("FAILED", failure.Result!.ErrorCode);

            session.MutationHandler = (_, _, _) =>
                throw new OutOfMemoryException("fatal sentinel");
            await Assert.ThrowsAsync<OutOfMemoryException>(() =>
                registry.ApplyTopologyMutationAsync(
                    options.TenantId,
                    sessionId,
                    1,
                    CreateProfile("profile-shapes"),
                    () => true,
                    CancellationToken.None));
            var poisoned = await registry.RunAsync(
                options.TenantId,
                sessionId,
                new DynamicGraphLogicalOrchestratorRunRequest("must remain busy"),
                () => true,
                CancellationToken.None);
            Assert.Equal(SessionExecutionStatus.Busy, poisoned.Status);
        }

        [Fact]
        public void CurrentTopologyResult_HasExactInternalFiveStateSurface()
        {
            Assert.False(typeof(CurrentTopologySessionStatus).IsPublic);
            Assert.True(typeof(CurrentTopologySessionStatus).IsNotPublic);
            Assert.False(typeof(CurrentTopologySessionResult).IsPublic);
            Assert.True(typeof(CurrentTopologySessionResult).IsNotPublic);
            Assert.Equal(
                ["Completed", "Expired", "Unknown", "Unavailable", "ForbiddenAssembly"],
                Enum.GetNames<CurrentTopologySessionStatus>());
            Assert.Equal(
                ["Status", "Session", "Result"],
                typeof(CurrentTopologySessionResult)
                    .GetProperties()
                    .Select(property => property.Name));
        }

        [Fact]
        public async Task ReadCurrentTopology_ReturnsLiveEvidenceAndDoesNotUseRunSlot()
        {
            var options = Options();
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            var runStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseRun = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var session = new MutationSession("profile-live-read")
            {
                ReadHandler = () => OperationResult<DynamicGraphLogicalOrchestratorSessionResult>.Ok(
                    CurrentResult(
                        "profile-live-read",
                        revision: 2,
                        signalId: "signal-revision-2",
                        DynamicGraphBuildSourceKind.LogicalRegistryProjection,
                        nodeCount: 2,
                        edgeCount: 1)),
                RunHandler = async (_, cancellationToken) =>
                {
                    runStarted.TrySetResult();
                    await releaseRun.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                    return OperationResult<DynamicGraphLogicalOrchestratorRunResult>.Ok(
                        new DynamicGraphLogicalOrchestratorRunResult(
                            "scenario",
                            "flow",
                            "PASS",
                            "ok",
                            new Dictionary<string, string>(),
                            []));
                }
            };
            var sessionId = await CreateSessionAsync(registry, options, session, "live-read");
            var occupiedRun = registry.RunAsync(
                options.TenantId,
                sessionId,
                new DynamicGraphLogicalOrchestratorRunRequest("occupy the sole run slot"),
                () => true,
                CancellationToken.None);
            await runStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

            var read = registry.ReadCurrentTopology(
                options.TenantId,
                sessionId,
                () => true);

            Assert.Equal(CurrentTopologySessionStatus.Completed, read.Status);
            Assert.NotNull(read.Session);
            Assert.Equal(1, read.Session.Result.Revision);
            Assert.True(read.Result!.IsSuccess);
            Assert.Equal(2, read.Result.Value!.Revision);
            Assert.Equal("signal-revision-2", read.Result.Value.SignalId);
            Assert.Equal(1, session.ReadCount);
            releaseRun.TrySetResult();
            Assert.Equal(
                SessionExecutionStatus.Completed,
                (await occupiedRun.WaitAsync(TimeSpan.FromSeconds(5))).Status);
        }

        [Fact]
        public async Task ReadCurrentTopology_MapsCapabilityLookupAndReadinessBoundariesBeforeRead()
        {
            var options = Options();
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            var unavailableId = await CreateSessionAsync(
                registry,
                options,
                new SuccessfulRunSession(),
                "read-unavailable");
            Assert.Equal(
                CurrentTopologySessionStatus.Unavailable,
                registry.ReadCurrentTopology(
                    options.TenantId,
                    unavailableId,
                    () => true).Status);
            Assert.Equal(
                CurrentTopologySessionStatus.Unknown,
                registry.ReadCurrentTopology(
                    options.TenantId,
                    "dgs_" + new string('0', 32),
                    () => true).Status);

            var session = new MutationSession("profile-read-guards")
            {
                ReadHandler = () => OperationResult<DynamicGraphLogicalOrchestratorSessionResult>.Ok(
                    CurrentResult("profile-read-guards"))
            };
            var sessionId = await CreateSessionAsync(
                registry,
                options,
                session,
                "read-guards");
            Assert.Equal(
                CurrentTopologySessionStatus.ForbiddenAssembly,
                registry.ReadCurrentTopology(
                    options.TenantId,
                    sessionId,
                    () => false).Status);
            Assert.Equal(0, session.ReadCount);

            var calls = 0;
            Assert.Equal(
                CurrentTopologySessionStatus.ForbiddenAssembly,
                registry.ReadCurrentTopology(
                    options.TenantId,
                    sessionId,
                    () => Interlocked.Increment(ref calls) == 1).Status);
            Assert.Equal(2, calls);
            Assert.Equal(0, session.ReadCount);

            var expiringOptions = Options() with { SessionTtl = TimeSpan.FromSeconds(1) };
            var time = new MutableTimeProvider(
                DateTimeOffset.Parse("2026-07-27T00:00:00Z"));
            using var expiringRegistry = new DynamicGraphCompositionSessionRegistry(
                expiringOptions,
                time);
            var expiringSession = new MutationSession("profile-read-expired")
            {
                ReadHandler = () =>
                    OperationResult<DynamicGraphLogicalOrchestratorSessionResult>.Ok(
                        CurrentResult("profile-read-expired"))
            };
            var expiringId = await CreateSessionAsync(
                expiringRegistry,
                expiringOptions,
                expiringSession,
                "read-expired");
            time.Advance(TimeSpan.FromMilliseconds(1500));
            Assert.Equal(
                CurrentTopologySessionStatus.Expired,
                expiringRegistry.ReadCurrentTopology(
                    expiringOptions.TenantId,
                    expiringId,
                    () => true).Status);
            Assert.Equal(0, expiringSession.ReadCount);
        }

        [Fact]
        public async Task ReadCurrentTopology_FailsClosedAfterReadinessFlipOrSessionRollback()
        {
            var options = Options();
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            var session = new MutationSession("profile-read-after")
            {
                ReadHandler = () => OperationResult<DynamicGraphLogicalOrchestratorSessionResult>.Ok(
                    CurrentResult("profile-read-after"))
            };
            var sessionId = await CreateSessionAsync(
                registry,
                options,
                session,
                "read-after");
            var calls = 0;
            var forbidden = registry.ReadCurrentTopology(
                options.TenantId,
                sessionId,
                () => Interlocked.Increment(ref calls) < 3);
            Assert.Equal(CurrentTopologySessionStatus.ForbiddenAssembly, forbidden.Status);
            Assert.Equal(1, session.ReadCount);
            Assert.Equal(3, calls);

            var rollbackSession = new MutationSession("profile-read-rollback");
            var rollbackId = await CreateSessionAsync(
                registry,
                options,
                rollbackSession,
                "read-rollback");
            var rollbackEntry = Assert.IsType<DynamicGraphCompositionSessionRegistry.SessionEntry>(
                registry.Get(options.TenantId, rollbackId).Session);
            rollbackSession.ReadHandler = () =>
            {
                Assert.True(registry.RollbackPublished(rollbackEntry));
                return OperationResult<DynamicGraphLogicalOrchestratorSessionResult>.Ok(
                    CurrentResult("profile-read-rollback"));
            };

            var rolledBack = registry.ReadCurrentTopology(
                options.TenantId,
                rollbackId,
                () => true);
            Assert.Equal(CurrentTopologySessionStatus.Unknown, rolledBack.Status);
            Assert.Equal(1, rollbackSession.ReadCount);
        }

        [Fact]
        public async Task ReadCurrentTopology_PreservesExactEnvelopeAndExceptionShapes()
        {
            var options = Options();
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            var session = new MutationSession("profile-read-shapes");
            var sessionId = await CreateSessionAsync(
                registry,
                options,
                session,
                "read-shapes");

            session.ReadHandler = () => null!;
            var nullEnvelope = registry.ReadCurrentTopology(
                options.TenantId,
                sessionId,
                () => true);
            Assert.Equal(CurrentTopologySessionStatus.Completed, nullEnvelope.Status);
            Assert.Null(nullEnvelope.Result);

            session.ReadHandler = () =>
                OperationResult<DynamicGraphLogicalOrchestratorSessionResult>.Ok(null!);
            var successfulNull = registry.ReadCurrentTopology(
                options.TenantId,
                sessionId,
                () => true);
            Assert.True(successfulNull.Result!.IsSuccess);
            Assert.Null(successfulNull.Result.Value);

            session.ReadHandler = () =>
                OperationResult<DynamicGraphLogicalOrchestratorSessionResult>.Fail(
                    "private downstream message",
                    "PRIVATE_DOWNSTREAM_CODE");
            var failure = registry.ReadCurrentTopology(
                options.TenantId,
                sessionId,
                () => true);
            Assert.False(failure.Result!.IsSuccess);
            Assert.Equal("PRIVATE_DOWNSTREAM_CODE", failure.Result.ErrorCode);

            session.ReadHandler = () => throw new InvalidOperationException("nonfatal sentinel");
            Assert.Throws<InvalidOperationException>(() =>
                registry.ReadCurrentTopology(options.TenantId, sessionId, () => true));
            session.ReadHandler = () => throw new OutOfMemoryException("fatal sentinel");
            Assert.Throws<OutOfMemoryException>(() =>
                registry.ReadCurrentTopology(options.TenantId, sessionId, () => true));
        }

        [Fact]
        public async Task ReadCurrentTopology_ConcurrentMutationReturnsOnlyWholeRevision()
        {
            var options = Options() with { MaxConcurrentRuns = 2 };
            using var registry = new DynamicGraphCompositionSessionRegistry(options);
            var revision = 1L;
            var signal = "signal-revision-1";
            var nodeCount = 1;
            var edgeCount = 0;
            var gate = new object();
            var session = new MutationSession("profile-read-race")
            {
                ReadHandler = () =>
                {
                    lock (gate)
                    {
                        return OperationResult<DynamicGraphLogicalOrchestratorSessionResult>.Ok(
                            CurrentResult(
                                "profile-read-race",
                                revision,
                                signal,
                                DynamicGraphBuildSourceKind.LogicalRegistryProjection,
                                nodeCount,
                                edgeCount));
                    }
                },
                MutationHandler = (_, _, _) =>
                {
                    lock (gate)
                    {
                        revision = 2;
                        signal = "signal-revision-2";
                        nodeCount = 2;
                        edgeCount = 1;
                        return OperationResult<DynamicGraphTopologyApplyResult>.Ok(
                            new DynamicGraphTopologyApplyResult(1, 2, true, null!, null));
                    }
                }
            };
            var sessionId = await CreateSessionAsync(
                registry,
                options,
                session,
                "read-race");

            var mutation = Task.Run(() => registry.ApplyTopologyMutationAsync(
                options.TenantId,
                sessionId,
                1,
                CreateProfile("profile-read-race"),
                () => true,
                CancellationToken.None));
            var read = await Task.Run(() => registry.ReadCurrentTopology(
                options.TenantId,
                sessionId,
                () => true));
            _ = await mutation;

            Assert.Equal(CurrentTopologySessionStatus.Completed, read.Status);
            var current = Assert.IsType<DynamicGraphLogicalOrchestratorSessionResult>(
                read.Result!.Value);
            Assert.True(
                current is
                {
                    Revision: 1,
                    SignalId: "signal-revision-1",
                    NodeCount: 1,
                    EdgeCount: 0
                }
                or
                {
                    Revision: 2,
                    SignalId: "signal-revision-2",
                    NodeCount: 2,
                    EdgeCount: 1
                });
        }

        private static DynamicGraphCompositionServerOptions Options()
            => new()
            {
                TenantId = "tenant-a",
                MaxSessions = 8,
                MaxConcurrentCompositions = 1,
                MaxConcurrentRuns = 1,
                SessionTtl = TimeSpan.FromMinutes(1)
            };

        private static async Task<string> CreateSessionAsync(
            DynamicGraphCompositionSessionRegistry registry,
            DynamicGraphCompositionServerOptions options,
            IDynamicGraphLogicalOrchestratorRunSession runSession,
            string key)
        {
            var created = await registry.CreateAsync(
                options.TenantId,
                key,
                Digest('A'),
                CreateProfile("profile-" + key),
                _ => Task.FromResult((
                    new CompositionEvidenceHttpResponse(
                        "profile-" + key,
                        1,
                        "LogicalRegistryProjection",
                        1,
                        0,
                        true),
                    runSession)),
                () => true,
                CancellationToken.None);
            Assert.Equal(SessionCreationStatus.Created, created.Status);
            return Assert.IsType<DynamicGraphCompositionSessionRegistry.SessionEntry>(
                created.Session).SessionId;
        }

        private static async Task AssertLegacySlotAvailableAsync(
            DynamicGraphCompositionSessionRegistry registry,
            DynamicGraphCompositionServerOptions options,
            string sessionId,
            DynamicGraphLogicalOrchestratorRunRequest request)
        {
            var legacy = await registry.RunAsync(
                options.TenantId,
                sessionId,
                request,
                () => true,
                CancellationToken.None);
            Assert.Equal(SessionExecutionStatus.Completed, legacy.Status);
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

        private static DynamicGraphLogicalOrchestratorSessionResult CurrentResult(
            string profileId,
            long revision = 1,
            string signalId = "signal-a",
            DynamicGraphBuildSourceKind sourceKind =
                DynamicGraphBuildSourceKind.LogicalRegistryProjection,
            int nodeCount = 1,
            int edgeCount = 0)
            => new(
                new DynamicGraphLogicalOrchestratorSessionDescriptor(
                    profileId,
                    "tenant-a",
                    "domain-a",
                    "context-a"),
                revision,
                signalId,
                sourceKind,
                nodeCount,
                edgeCount,
                true);

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

        private sealed class MutationOnlyRunSession :
            IDynamicGraphLogicalOrchestratorRunSession,
            IDynamicGraphLogicalOrchestratorTopologyMutationSession
        {
            public Task<OperationResult<DynamicGraphLogicalOrchestratorRunResult>> RunAsync(
                DynamicGraphLogicalOrchestratorRunRequest request,
                CancellationToken cancellationToken = default)
                => Task.FromResult(OperationResult<DynamicGraphLogicalOrchestratorRunResult>.Ok(
                    new DynamicGraphLogicalOrchestratorRunResult(
                        "scenario",
                        "flow",
                        "PASS",
                        "ok",
                        new Dictionary<string, string>(),
                        [])));

            public OperationResult<DynamicGraphTopologyApplyResult> ApplyRegistryProjection(
                long expectedRevision,
                DynamicGraphRegistryProjectionRequest request,
                TopologyBuildBudget budget)
                => OperationResult<DynamicGraphTopologyApplyResult>.Fail(
                    "unavailable",
                    "SHOULD_NOT_RUN");
        }

        private sealed class MutationSession :
            IDynamicGraphLogicalOrchestratorRunSession,
            IDynamicGraphLogicalOrchestratorCompositionSession,
            IDynamicGraphLogicalOrchestratorTopologyMutationSession,
            IDynamicGraphLogicalOrchestratorCheckpointSession
        {
            private readonly DynamicGraphLogicalOrchestratorSessionDescriptor _descriptor;
            private readonly Action<int>? _descriptorRead;
            private int _descriptorReadCount;
            private int _applyCount;
            private int _readCount;

            internal MutationSession(
                string profileId,
                string tenantId = "tenant-a",
                string domainTag = "domain-a",
                string contextId = "context-a",
                Action<int>? descriptorRead = null)
            {
                _descriptor = new(profileId, tenantId, domainTag, contextId);
                _descriptorRead = descriptorRead;
            }

            public DynamicGraphLogicalOrchestratorSessionDescriptor Descriptor
            {
                get
                {
                    _descriptorRead?.Invoke(Interlocked.Increment(ref _descriptorReadCount));
                    return _descriptor with { };
                }
            }

            internal int ApplyCount => Volatile.Read(ref _applyCount);
            internal int ReadCount => Volatile.Read(ref _readCount);
            internal long ExpectedRevision { get; private set; }
            internal DynamicGraphRegistryProjectionRequest? Request { get; private set; }
            internal TopologyBuildBudget? Budget { get; private set; }
            internal Func<
                long,
                DynamicGraphRegistryProjectionRequest,
                TopologyBuildBudget,
                OperationResult<DynamicGraphTopologyApplyResult>> MutationHandler { get; set; }
                = (_, _, _) => OperationResult<DynamicGraphTopologyApplyResult>.Fail(
                    "sentinel",
                    "DOWNSTREAM_SENTINEL");
            internal Func<OperationResult<DynamicGraphLogicalOrchestratorSessionResult>>
                ReadHandler { get; set; }
                = () => OperationResult<DynamicGraphLogicalOrchestratorSessionResult>.Fail(
                    "read forbidden",
                    "READ_SHOULD_NOT_RUN");
            internal Func<
                DynamicGraphLogicalOrchestratorRunRequest,
                CancellationToken,
                Task<OperationResult<DynamicGraphLogicalOrchestratorRunResult>>> RunHandler
                { get; set; }
                = (_, _) => Task.FromResult(
                    OperationResult<DynamicGraphLogicalOrchestratorRunResult>.Ok(
                        new DynamicGraphLogicalOrchestratorRunResult(
                            "scenario",
                            "flow",
                            "PASS",
                            "ok",
                            new Dictionary<string, string>(),
                            [])));

            public OperationResult<DynamicGraphLogicalOrchestratorSessionResult> ReadCurrent()
            {
                Interlocked.Increment(ref _readCount);
                return ReadHandler();
            }

            public OperationResult<DynamicGraphTopologyApplyResult> ApplyRegistryProjection(
                long expectedRevision,
                DynamicGraphRegistryProjectionRequest request,
                TopologyBuildBudget budget)
            {
                Interlocked.Increment(ref _applyCount);
                ExpectedRevision = expectedRevision;
                Request = request;
                Budget = budget;
                return MutationHandler(expectedRevision, request, budget);
            }

            public Task<OperationResult<DynamicGraphLogicalOrchestratorRunResult>> RunAsync(
                DynamicGraphLogicalOrchestratorRunRequest request,
                CancellationToken cancellationToken = default)
                => RunHandler(request, cancellationToken);

            public OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>
                CaptureCheckpoint(
                    DynamicGraphLogicalOrchestratorBootstrapProfile expectedCurrentProfile)
                => OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>.Ok(
                    new DynamicGraphLogicalOrchestratorCheckpointCapture(
                        expectedCurrentProfile,
                        new DynamicGraphTopology(
                            expectedCurrentProfile.TenantId,
                            expectedCurrentProfile.DomainTag,
                            expectedCurrentProfile.ContextId,
                            [],
                            [],
                            []),
                        Revision: 1,
                        expectedCurrentProfile.SignalId,
                        DynamicGraphBuildSourceKind.DeterministicBootstrap,
                        expectedCurrentProfile.SourceRef,
                        expectedCurrentProfile.TraceRef,
                        NodeCount: 0,
                        EdgeCount: 0,
                        new TopologyBudgetProof(
                            expectedCurrentProfile.Budget.MaxNodes,
                            ActualNodes: 0,
                            expectedCurrentProfile.Budget.MaxEdges,
                            ActualEdges: 0,
                            expectedCurrentProfile.Budget.MaxDepth,
                            ActualDepth: 0,
                            expectedCurrentProfile.Budget.MaxParallelBranches,
                            ActualParallelBranches: 0,
                            WithinBudget: true,
                            ExceededDimensions: []),
                        new TopologyMutationEvidence(
                            expectedCurrentProfile.SignalId,
                            DynamicGraphBuildSourceKind.DeterministicBootstrap,
                            expectedCurrentProfile.SourceRef,
                            expectedCurrentProfile.TraceRef,
                            [],
                            [],
                            [],
                            [],
                            [],
                            [],
                            [],
                            MutationApplied: false),
                        InitialRevisionAdvanced: true));

            public OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>
                RestoreCheckpoint(
                    DynamicGraphLogicalOrchestratorBootstrapProfile expectedCurrentProfile,
                    DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint checkpoint)
                => OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>.Fail(
                    "The registry persistence-mode guard test does not restore checkpoints.",
                    "TEST_RESTORE_NOT_CONFIGURED");
        }

        private class DualRunSession :
            IDynamicGraphLogicalOrchestratorRunSession,
            IDynamicGraphLogicalOrchestratorTopologyRunSession
        {
            private int _legacyRunCount;
            private int _topologyRunCount;

            internal int LegacyRunCount => Volatile.Read(ref _legacyRunCount);
            internal int TopologyRunCount => Volatile.Read(ref _topologyRunCount);

            public virtual Task<OperationResult<DynamicGraphLogicalOrchestratorRunResult>> RunAsync(
                DynamicGraphLogicalOrchestratorRunRequest request,
                CancellationToken cancellationToken = default)
            {
                Interlocked.Increment(ref _legacyRunCount);
                return Task.FromResult(OperationResult<DynamicGraphLogicalOrchestratorRunResult>.Ok(
                    RunResult("legacy-scenario", "legacy-flow")));
            }

            public virtual Task<OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>>
                RunOnCurrentTopologyAsync(
                    DynamicGraphLogicalOrchestratorRunRequest request,
                    CancellationToken cancellationToken = default)
            {
                Interlocked.Increment(ref _topologyRunCount);
                return Task.FromResult(
                    OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>.Ok(
                        TopologyResult()));
            }

            protected static DynamicGraphLogicalOrchestratorRunResult RunResult(
                string scenarioId,
                string flowId)
                => new(
                    scenarioId,
                    flowId,
                    "PASS",
                    "ok",
                    new Dictionary<string, string>(StringComparer.Ordinal),
                    []);

            protected static DynamicGraphLogicalOrchestratorTopologyRunResult TopologyResult()
                => new(
                    7,
                    "topology-signal-7",
                    DynamicGraphBuildSourceKind.LogicalRegistryProjection,
                    RunResult("topology-scenario", "topology-flow"));
        }

        private sealed class BlockingDualRunSession : DualRunSession
        {
            internal TaskCompletionSource TopologyStarted { get; } =
                new(TaskCreationOptions.RunContinuationsAsynchronously);
            internal TaskCompletionSource ReleaseTopology { get; } =
                new(TaskCreationOptions.RunContinuationsAsynchronously);
            internal TaskCompletionSource LegacyStarted { get; } =
                new(TaskCreationOptions.RunContinuationsAsynchronously);
            internal TaskCompletionSource ReleaseLegacy { get; } =
                new(TaskCreationOptions.RunContinuationsAsynchronously);

            public override async Task<OperationResult<DynamicGraphLogicalOrchestratorRunResult>>
                RunAsync(
                    DynamicGraphLogicalOrchestratorRunRequest request,
                    CancellationToken cancellationToken = default)
            {
                LegacyStarted.TrySetResult();
                await ReleaseLegacy.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                return await base.RunAsync(request, cancellationToken).ConfigureAwait(false);
            }

            public override async Task<
                OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>>
                RunOnCurrentTopologyAsync(
                    DynamicGraphLogicalOrchestratorRunRequest request,
                    CancellationToken cancellationToken = default)
            {
                TopologyStarted.TrySetResult();
                await ReleaseTopology.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
                return await base.RunOnCurrentTopologyAsync(request, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        private sealed class ShapeChangingTopologyRunSession :
            IDynamicGraphLogicalOrchestratorRunSession,
            IDynamicGraphLogicalOrchestratorTopologyRunSession
        {
            internal Func<
                DynamicGraphLogicalOrchestratorRunRequest,
                CancellationToken,
                Task<OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>>>
                TopologyHandler { get; set; } =
                    (_, _) => Task.FromResult(
                        OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>.Ok(
                            new DynamicGraphLogicalOrchestratorTopologyRunResult(
                                7,
                                "topology-signal-7",
                                DynamicGraphBuildSourceKind.LogicalRegistryProjection,
                                new DynamicGraphLogicalOrchestratorRunResult(
                                    "topology-scenario",
                                    "topology-flow",
                                    "PASS",
                                    "ok",
                                    new Dictionary<string, string>(StringComparer.Ordinal),
                                    []))));

            public Task<OperationResult<DynamicGraphLogicalOrchestratorRunResult>> RunAsync(
                DynamicGraphLogicalOrchestratorRunRequest request,
                CancellationToken cancellationToken = default)
                => Task.FromResult(OperationResult<DynamicGraphLogicalOrchestratorRunResult>.Ok(
                    new DynamicGraphLogicalOrchestratorRunResult(
                        "legacy-scenario",
                        "legacy-flow",
                        "PASS",
                        "ok",
                        new Dictionary<string, string>(StringComparer.Ordinal),
                        [])));

            public Task<OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>>
                RunOnCurrentTopologyAsync(
                    DynamicGraphLogicalOrchestratorRunRequest request,
                    CancellationToken cancellationToken = default)
                => TopologyHandler(request, cancellationToken);
        }

        private sealed class MutableTimeProvider(DateTimeOffset now) : TimeProvider
        {
            private DateTimeOffset _now = now;

            public override DateTimeOffset GetUtcNow() => _now;

            internal void Advance(TimeSpan amount) => _now = _now.Add(amount);
        }
    }
}

namespace LlmFromScratch.DynamicGraphCompositionLiveLocalServer
{
    using AgentInfrastructure;
    using AgentInfrastructure.Logical;
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
        internal static TestAdmission Admit(
            DynamicGraphLogicalOrchestratorBootstrapProfile? profile,
            string tenantId)
            => profile is null
               || profile.NodeDefinitions is null
               || profile.EdgeBindings is null
               || profile.Budget is null
               || !string.Equals(profile.TenantId, tenantId, StringComparison.Ordinal)
                ? new(false, null, null)
                : new(
                    true,
                    DynamicGraphCompositionSessionPersistence.SnapshotProfile(profile),
                    new string('A', 64));

        internal static TestAdmission ValidateOwnedSnapshot(
            DynamicGraphLogicalOrchestratorBootstrapProfile profile,
            string tenantId)
            => new(
                true,
                DynamicGraphCompositionSessionPersistence.SnapshotProfile(profile),
                new string('A', 64));
    }

    internal sealed record TestAdmission(
        bool IsAccepted,
        DynamicGraphLogicalOrchestratorBootstrapProfile? Profile,
        string? CanonicalDigest);

    internal sealed record PersistedCompositionSession(
        string Schema,
        string SessionId,
        string TenantIndex,
        string OpaqueIdempotencyIndex,
        string CanonicalDigest,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset ExpiresAtUtc,
        DynamicGraphLogicalOrchestratorBootstrapProfile Profile,
        CompositionEvidenceHttpResponse Evidence,
        DynamicGraphLogicalOrchestratorCheckpointCapture? Capture,
        DynamicGraphLogicalOrchestratorAuthenticatedCheckpoint? AuthenticatedCheckpoint);

    public sealed class DynamicGraphCompositionSessionPersistence : IDisposable
    {
        internal const string LegacyCheckpointSchema = "dynamic-graph-composition-session.v1";
        internal const string CheckpointSchema = "dynamic-graph-composition-session.v2";

        private readonly object _gate = new();
        private readonly Dictionary<string, PersistedCompositionSession> _sessions =
            new(StringComparer.Ordinal);
        private readonly bool _enabled;
        private int _persistCount;
        private int _readCount;
        private int _deleteCount;

        public DynamicGraphCompositionSessionPersistence(DynamicGraphCompositionServerOptions options)
            : this(options, enabled: false)
        {
        }

        internal DynamicGraphCompositionSessionPersistence(
            DynamicGraphCompositionServerOptions options,
            bool enabled)
        {
            ArgumentNullException.ThrowIfNull(options);
            _enabled = enabled;
        }

        public bool IsEnabled => _enabled;

        internal int PersistCount => Volatile.Read(ref _persistCount);

        internal int ReadCount => Volatile.Read(ref _readCount);

        internal int DeleteCount => Volatile.Read(ref _deleteCount);

        internal IReadOnlyList<PersistedCompositionSession> PersistedSnapshots
        {
            get
            {
                lock (_gate)
                {
                    return _sessions.Values
                        .OrderBy(session => session.SessionId, StringComparer.Ordinal)
                        .Select(SnapshotSession)
                        .ToArray();
                }
            }
        }

        internal void PersistNew(PersistedCompositionSession session)
        {
            if (!_enabled)
                return;

            ArgumentNullException.ThrowIfNull(session);
            var snapshot = SnapshotSession(session);
            lock (_gate)
            {
                if (!_sessions.TryAdd(snapshot.SessionId, snapshot))
                {
                    throw new DynamicGraphCompositionPersistenceException(
                        "A test session checkpoint already exists.");
                }
                Interlocked.Increment(ref _persistCount);
            }
        }

        internal void Delete(string sessionId)
        {
            if (!_enabled)
                return;

            lock (_gate)
            {
                _sessions.Remove(sessionId);
                Interlocked.Increment(ref _deleteCount);
            }
        }

        internal IReadOnlyList<PersistedCompositionSession> ReadAll(int maxSessions)
        {
            if (!_enabled)
                return [];

            lock (_gate)
            {
                Interlocked.Increment(ref _readCount);
                if (_sessions.Count > maxSessions)
                {
                    throw new DynamicGraphCompositionPersistenceException(
                        "Persisted test session state exceeds the configured capacity.");
                }
                return _sessions.Values
                    .OrderBy(session => session.SessionId, StringComparer.Ordinal)
                    .Select(SnapshotSession)
                    .ToArray();
            }
        }

        internal string DeriveIndex(params string[] components)
            => Convert.ToHexStringLower(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(string.Join("\u001f", components))));

        internal static DynamicGraphLogicalOrchestratorBootstrapProfile SnapshotProfile(
            DynamicGraphLogicalOrchestratorBootstrapProfile profile)
            => profile with
            {
                NodeDefinitions = profile.NodeDefinitions
                    .Select(definition => definition with
                    {
                        Descriptor = definition.Descriptor with { }
                    })
                    .ToArray(),
                EdgeBindings = profile.EdgeBindings
                    .Select(binding => binding with
                    {
                        LogicalTransitionIds = binding.LogicalTransitionIds.ToArray(),
                        ExcludedLeakageSignals = binding.ExcludedLeakageSignals.ToArray()
                    })
                    .ToArray(),
                Budget = profile.Budget with { }
            };

        private static PersistedCompositionSession SnapshotSession(
            PersistedCompositionSession session)
            => session with
            {
                Profile = SnapshotProfile(session.Profile),
                Evidence = session.Evidence with { },
                Capture = session.Capture is null
                    ? null
                    : session.Capture with
                    {
                        CurrentProfile = SnapshotProfile(session.Capture.CurrentProfile),
                        Topology = SnapshotTopology(session.Capture.Topology),
                        BudgetProof = SnapshotBudgetProof(session.Capture.BudgetProof),
                        MutationEvidence = SnapshotMutationEvidence(
                            session.Capture.MutationEvidence)
                    },
                AuthenticatedCheckpoint = session.AuthenticatedCheckpoint is null
                    ? null
                    : session.AuthenticatedCheckpoint with
                    {
                        BudgetProof = SnapshotBudgetProof(
                            session.AuthenticatedCheckpoint.BudgetProof),
                        MutationEvidence = SnapshotMutationEvidence(
                            session.AuthenticatedCheckpoint.MutationEvidence)
                    }
            };

        private static DynamicGraphTopology SnapshotTopology(DynamicGraphTopology topology)
            => new(
                topology.TenantId,
                topology.DomainTag,
                topology.ContextId,
                topology.Nodes,
                topology.Edges,
                topology.Order);

        private static TopologyBudgetProof SnapshotBudgetProof(TopologyBudgetProof proof)
            => proof with
            {
                ExceededDimensions = Array.AsReadOnly(
                    proof.ExceededDimensions.ToArray())
            };

        private static TopologyMutationEvidence SnapshotMutationEvidence(
            TopologyMutationEvidence evidence)
            => evidence with
            {
                ProposedAddedNodeIds = Array.AsReadOnly(
                    evidence.ProposedAddedNodeIds.ToArray()),
                ProposedRemovedNodeIds = Array.AsReadOnly(
                    evidence.ProposedRemovedNodeIds.ToArray()),
                ProposedUpdatedNodeIds = Array.AsReadOnly(
                    evidence.ProposedUpdatedNodeIds.ToArray()),
                ProposedAddedEdgeIds = Array.AsReadOnly(
                    evidence.ProposedAddedEdgeIds.ToArray()),
                ProposedRemovedEdgeIds = Array.AsReadOnly(
                    evidence.ProposedRemovedEdgeIds.ToArray()),
                ProposedUpdatedEdgeIds = Array.AsReadOnly(
                    evidence.ProposedUpdatedEdgeIds.ToArray()),
                ProposedReorderedNodeIds = Array.AsReadOnly(
                    evidence.ProposedReorderedNodeIds.ToArray())
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
