// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using ContinuousLearning;
using DesignConversionDomain;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;

public sealed class DesignConversionDpoTeacherProductionPathTests
{
    private const string ExpectedTeacherModelId = "qwen-test:9b";
    private static readonly string ExpectedTeacherManifestDigest =
        new('c', 64);
    private static readonly string ExpectedTeacherModelLayerDigest =
        new('d', 64);

    [Fact]
    public async Task ImprovementLoopCompletion_CreatesPreferenceCandidateOnlyThroughExactAssociatedTeacherRuntime()
    {
        var fixture = CreateProviderAllowedPair("coordinator");
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-dpo-teacher-coordinator-" + Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = new FakeTeacherInferenceClient();
        using var teacherRuntime =
            CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        try
        {
            var evaluator = new RequestBindingSequenceEvaluator(
                fixture.Rejected,
                fixture.Chosen);
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                evaluator,
                new SingleCandidateStrategy(fixture.Chosen.RuntimeFlowResult.Candidate),
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);

            var completed = await coordinator.RunAsync(
                new DesignConversionImprovementLoopRequest(
                    "teacher-coordinator-loop",
                    CreateInternalRequest(fixture.Rejected.SubmittedRequest),
                    MaxAttempts: 2,
                    Metadata: new Dictionary<string, string>()));

            Assert.True(
                completed.IsSuccess,
                completed.ErrorMessage + " " + completed.ErrorCode);
            var records = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            Assert.True(records.IsSuccess, records.ErrorMessage);
            var localRecord = Assert.Single(records.Value!);
            var candidates = localRecord.DpoCandidates.ToArray();
            Assert.Equal(2, candidates.Length);
            Assert.Equal(
                new[]
                {
                    DesignConversionDpoUnits.CriticUnit,
                    DesignConversionDpoUnits.ImproverUnit
                },
                candidates.Select(item => item.UnitId));
            Assert.Equal(
                2,
                candidates.Select(item => item.CandidateId)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            var candidate = candidates[0];
            Assert.All(candidates, item =>
            {
                Assert.Null(item.TrainingRecord.PayloadCustodySecret);
                Assert.NotNull(item.TrainingRecord.TeacherProvenance);
                Assert.Equal(
                    bool.TrueString,
                    item.TrainingRecord.Provenance.Metadata[
                        "runtime_teacher_authored_candidate"]);
                Assert.False(item.TrainingRecord.Provenance.Metadata.ContainsKey(
                    "runtime_teacher_selected_preference"));
                Assert.Equal(
                    bool.TrueString,
                    item.TrainingRecord.Provenance.Metadata[
                        "independent_preference_proven"]);
                Assert.Equal(
                    bool.FalseString,
                    item.TrainingRecord.Provenance.Metadata[
                        "trained_model_authority"]);
                AssertPendingLocalTeacherCandidateBeforeApproval(
                    localRecord,
                    item);
            });
            Assert.Equal(1, teacher.InvocationCount);
            Assert.Equal(
                bool.TrueString,
                completed.Value!.PublicMetadata["training_candidate_only"]);

            var bindings = candidates.Select(item =>
                    item.TrainingRecord.Provenance.Metadata[
                        DesignConversionDpoGenerator.PayloadCustodyBindingSha256MetadataKey])
                .ToArray();
            Assert.Equal(2, bindings.Distinct(StringComparer.Ordinal).Count());
            var binding = candidate.TrainingRecord.Provenance.Metadata[
                DesignConversionDpoGenerator.PayloadCustodyBindingSha256MetadataKey];
            var committedBindings = bindings.Order(StringComparer.Ordinal).ToArray();
            var custodyStore = new DesignConversionCandidatePayloadCustodyStore(root);
            var committedCustody = custodyStore.ResolveCommitted(
                new DesignConversionCandidatePayloadCustodyCommitment(
                    localRecord.Metadata[DesignConversionCandidatePayloadCustodyStore.TransactionMetadataKey],
                    localRecord.Metadata[DesignConversionCandidatePayloadCustodyStore.ManifestMetadataKey],
                    localRecord.Metadata[DesignConversionCandidatePayloadCustodyStore.BindingSetMetadataKey],
                    localRecord.RecordId,
                    localRecord.Metadata[JsonlDesignConversionLocalArtifactDb.ImmutableAppendFingerprintMetadataKey],
                    committedBindings));
            Assert.True(committedCustody.IsSuccess, committedCustody.ErrorMessage);
            var resolvedCustody = custodyStore.ResolveSecretEnvelope(
                committedCustody.Value!);
            Assert.True(resolvedCustody.IsSuccess, resolvedCustody.ErrorMessage);
            Assert.Equal(2, resolvedCustody.Value!.Entries.Count);
            const string distinctiveHtml =
                "<main data-teacher-authoring=\"candidate\">Candidate</main>";
            const string distinctiveCss =
                "[data-teacher-authoring=\"candidate\"]{display:grid;gap:24px;}";
            Assert.All(resolvedCustody.Value.Entries, payloadEntry =>
            {
                var chosenPayload = ReadPayloadResponse(
                    payloadEntry.Secret.ChosenResponse);
                Assert.Equal(distinctiveHtml, chosenPayload.Html);
                Assert.Equal(distinctiveCss, chosenPayload.Css);
                Assert.Contains(payloadEntry.BindingSha256, bindings);
            });

            var jsonl = await File.ReadAllTextAsync(
                Path.Combine(root, "conversion-results.jsonl"));
            Assert.DoesNotContain(distinctiveHtml, jsonl, StringComparison.Ordinal);
            Assert.DoesNotContain(distinctiveCss, jsonl, StringComparison.Ordinal);
            Assert.DoesNotContain("payloadCustody", jsonl, StringComparison.Ordinal);

            var api = new DesignConversionDpoUsefulnessApi(artifactDb);
            var approval = await api.ApproveRuntimeCreatedDpoCandidateAsync(
                localRecord.RecordId,
                candidate.CandidateId,
                "reviewer:payload-custody",
                "verified teacher payload custody",
                DateTimeOffset.UnixEpoch.AddMinutes(1));
            Assert.True(approval.IsSuccess, approval.ErrorMessage);
            var approvedRecords = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            Assert.True(approvedRecords.IsSuccess, approvedRecords.ErrorMessage);
            var approvedRecord = Assert.Single(approvedRecords.Value!);
            Assert.Equal(2, approvedRecord.DpoCandidates.Count);
            var approvedCandidate = Assert.Single(
                approvedRecord.DpoCandidates,
                item => string.Equals(
                    item.CandidateId,
                    candidate.CandidateId,
                    StringComparison.Ordinal));
            var pendingSibling = Assert.Single(
                approvedRecord.DpoCandidates,
                item => !string.Equals(
                    item.CandidateId,
                    candidate.CandidateId,
                    StringComparison.Ordinal));
            var approvedBlockReasons =
                DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(
                    approvedCandidate.TrainingRecord);
            Assert.True(
                approvedBlockReasons.Count == 0 ||
                approvedBlockReasons.SequenceEqual(["not_training_grade"]),
                "approved_block_reasons=" +
                string.Join(",", approvedBlockReasons));
            Assert.DoesNotContain("approval_missing", approvedBlockReasons);
            Assert.DoesNotContain("provider_missing", approvedBlockReasons);
            Assert.Equal(
                "approved",
                ReadMetadata(
                    approvedCandidate.TrainingRecord.Provenance.Metadata,
                    "approval_status"));
            Assert.Equal(
                JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewApprovedStatus,
                ReadMetadata(
                    approvedCandidate.TrainingRecord.Provenance.Metadata,
                    "review_status"));
            Assert.Equal(
                bool.TrueString,
                ReadMetadata(
                    approvedCandidate.TrainingRecord.Provenance.Metadata,
                    "approved_for_training"));
            Assert.Equal(
                "pending",
                ReadMetadata(
                    pendingSibling.TrainingRecord.Provenance.Metadata,
                    "approval_status"));
            Assert.Equal(
                bool.FalseString,
                ReadMetadata(
                    pendingSibling.TrainingRecord.Provenance.Metadata,
                    "approved_for_training"));
            var handoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: localRecord.JobId),
                DateTimeOffset.UnixEpoch.AddMinutes(2));
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var handoffRow = Assert.Single(handoff.Value!.Rows);
            Assert.Equal(
                candidate.TrainingRecord.Provenance.Metadata[
                    DesignConversionDpoGenerator.PayloadCustodyBindingSha256MetadataKey],
                handoffRow.BoundaryMetadata[
                    DesignConversionDpoGenerator.PayloadCustodyBindingSha256MetadataKey]);
            Assert.Equal(
                candidate.TrainingRecord.Provenance.Metadata[
                    DesignConversionDpoGenerator.PayloadCustodyGovernanceSha256MetadataKey],
                handoffRow.BoundaryMetadata[
                    DesignConversionDpoGenerator.PayloadCustodyGovernanceSha256MetadataKey]);
            Assert.Equal(
                DesignConversionDpoGenerator.PayloadCustodyVersion,
                handoffRow.BoundaryMetadata[
                    DesignConversionDpoGenerator.PayloadCustodyVersionMetadataKey]);
            Assert.Equal("1", handoff.Value.BoundaryMetadata["payload_custody_required_count"]);
            Assert.Equal(
                JsonlDesignConversionLocalArtifactDb.ApprovedDpoHandoffCustodySchemaVersion,
                handoff.Value.SchemaVersion);

            var intake = await api.CreateTrainingIntakeAsync(
                handoff.Value,
                "requester:payload-custody",
                "payload custody boundary propagation",
                DateTimeOffset.UnixEpoch.AddMinutes(3));
            Assert.True(intake.IsSuccess, intake.ErrorMessage);
            var intakeCandidate = Assert.Single(intake.Value!.CandidateRefs);
            Assert.Equal(
                handoffRow.BoundaryMetadata[
                    DesignConversionDpoGenerator.PayloadCustodyBindingSha256MetadataKey],
                intakeCandidate.BoundaryMetadata[
                    DesignConversionDpoGenerator.PayloadCustodyBindingSha256MetadataKey]);
            Assert.Equal(
                handoffRow.BoundaryMetadata[
                    DesignConversionDpoGenerator.PayloadCustodyGovernanceSha256MetadataKey],
                intakeCandidate.BoundaryMetadata[
                    DesignConversionDpoGenerator.PayloadCustodyGovernanceSha256MetadataKey]);
            Assert.Equal(
                handoff.Value.BoundaryMetadata["payload_custody_commitment_set_sha256"],
                intake.Value.BoundaryMetadata["payload_custody_commitment_set_sha256"]);
            Assert.Equal(
                JsonlDesignConversionLocalArtifactDb.TrainingIntakeCustodySchemaVersion,
                intake.Value.BoundaryMetadata["schema_version"]);
            var serializedHandoff = JsonSerializer.Serialize(handoff.Value);
            var serializedIntake = JsonSerializer.Serialize(intake.Value);
            Assert.DoesNotContain(distinctiveHtml, serializedHandoff, StringComparison.Ordinal);
            Assert.DoesNotContain(distinctiveCss, serializedHandoff, StringComparison.Ordinal);
            Assert.DoesNotContain(distinctiveHtml, serializedIntake, StringComparison.Ordinal);
            Assert.DoesNotContain(distinctiveCss, serializedIntake, StringComparison.Ordinal);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task ImprovementLoopCompletion_InvalidEnabledRepositoryOwnedDpoContextFailsClosedBeforeTeacherInference()
    {
        var fixture = CreateProviderAllowedPair(
            "coordinator-invalid-repository-owned-dpo-context");
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-dpo-invalid-context-" +
            Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = new FakeTeacherInferenceClient();
        using var teacherRuntime = CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        var deterministic = new CountingCandidateStrategy(
            fixture.Chosen.RuntimeFlowResult.Candidate);
        try
        {
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(
                    fixture.Rejected,
                    fixture.Chosen),
                deterministic,
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);
            var invalidContext =
                new DesignConversionRepositoryOwnedDpoTransactionContext(
                    Enabled: true,
                    Address: null,
                    RequestIdentity: null);

            var completed = await coordinator.RunAsync(new(
                "teacher-invalid-repository-owned-dpo-context-loop",
                CreateInternalRequest(fixture.Rejected.SubmittedRequest) with
                {
                    Budget = fixture.Rejected.SubmittedRequest.Budget with
                    {
                        MaxTeacherCalls = 1,
                        MaxImprovementCandidates = 2
                    }
                },
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>())
            {
                // Enabled repository-owned DPO mode is an authority boundary;
                // a missing address/identity must stop before teacher inference.
                RepositoryOwnedDpoTransactionContext = invalidContext
            });

            Assert.True(completed.IsSuccess, completed.ErrorMessage);
            Assert.Equal(
                DesignConversionImprovementLoopCoordinator.CandidateStrategyFailed,
                completed.Value!.StopReason);
            Assert.True(completed.Value.CandidateStrategyFailed);
            Assert.Equal(0, teacher.InvocationCount);
            Assert.Equal(0, deterministic.InvocationCount);
            Assert.Equal(
                bool.TrueString,
                completed.Value.PublicMetadata[
                    DesignConversionRepositoryOwnedDpoTransactionContext
                        .EnabledMetadataKey]);
            Assert.Equal(
                bool.FalseString,
                completed.Value.PublicMetadata[
                    DesignConversionRepositoryOwnedDpoTransactionContext
                        .ValidationMetadataKey]);
            Assert.Contains(
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoTransactionPreflightMarker,
                completed.Value.EvidenceRefs);
            Assert.Contains(
                "improvement_loop:" +
                DesignConversionRepositoryOwnedDpoTransactionContext
                    .ValidationMetadataKey +
                "=" + bool.FalseString,
                completed.Value.EvidenceRefs);
            Assert.Contains(
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoTransactionPreflightFailedWarning +
                ":" +
                DesignConversionRepositoryOwnedDpoTransactionContext
                    .InvalidErrorCode,
                completed.Value.Warnings);
            var records = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            Assert.True(records.IsSuccess, records.ErrorMessage);
            Assert.All(records.Value!, stored =>
            {
                Assert.Empty(stored.DpoCandidates);
                Assert.Equal(
                    bool.FalseString,
                    stored.Metadata[
                        DesignConversionRepositoryOwnedDpoTransactionContext
                            .ValidationMetadataKey]);
            });
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task AuthorCandidateAsync_InvalidEnabledRepositoryOwnedDpoContextReturnsExactErrorBeforeInference()
    {
        var fixture = CreateProviderAllowedPair(
            "authoring-invalid-repository-owned-dpo-context");
        using var teacher = new FakeTeacherInferenceClient();
        using var runtime = CreateTeacherRuntime(teacher);
        var incumbent = WithScoreAndContinuation(fixture.Rejected, 0.2, 2);
        var request = new DesignConversionCandidateUpdateRequest(
            LoopId: "teacher-authoring-invalid-repository-owned-dpo-context-loop",
            CompletedAttemptNumber: 1,
            CurrentRequest: CreateInternalRequest(incumbent.SubmittedRequest),
            CurrentEvaluation: incumbent,
            TierPolicy: incumbent.VisualQaBoundaryTrace.TierPolicy,
            AttemptHistory: [])
        {
            RepositoryOwnedDpoTransactionContext =
                new DesignConversionRepositoryOwnedDpoTransactionContext(
                    Enabled: true,
                    Address: null,
                    RequestIdentity: null)
        };
        var reservation = DesignConversionDpoTeacherCallReservation.Create(
            configuredMaximumCalls: 1,
            observedCallCount: 0);
        Assert.True(reservation.IsSuccess, reservation.ErrorMessage);

        var authored = await runtime.AuthorCandidateAsync(
            request,
            reservation.Value!);

        Assert.False(authored.IsSuccess);
        Assert.Equal(
            DesignConversionRepositoryOwnedDpoTransactionContext
                .InvalidErrorCode,
            authored.ErrorCode);
        Assert.False(reservation.Value.CallIssued);
        Assert.Equal(0, reservation.Value.ActualObservedCallCount);
        Assert.Equal(0, teacher.InvocationCount);
    }

    [Fact]
    public async Task ImprovementLoopCompletion_EnabledRepositoryOwnedDpoContextReusesExactSealedDatasetWithoutTeacherOrLocalCandidate()
    {
        var fixture = CreateProviderAllowedPair(
            "repository-owned-dpo-existing");
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-dpo-existing-reuse-" +
            Guid.NewGuid().ToString("N"));
        var repositoryRoot = CreateRepositoryOwnedDpoRepositoryRoot(
            "existing-reuse");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = new FakeTeacherInferenceClient();
        using var teacherRuntime = CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        var deterministic = new CountingCandidateStrategy(
            fixture.Chosen.RuntimeFlowResult.Candidate);
        const string paidReferenceRawText =
            "paid model answer kept outside repository-owned DPO pair bodies";
        try
        {
            var address = CreateRepositoryOwnedDpoDatasetAddress(
                repositoryRoot);
            var identity = CreateRepositoryOwnedDpoRequestIdentity(
                "existing-reuse",
                paidReferenceRawText);
            var pair = CreateRepositoryOwnedDpoPair(
                "existing-reuse",
                identity,
                paidReferenceRawText);
            var manifest = AssertSuccess(
                RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
                    address,
                    identity,
                    [pair]));
            var context = new DesignConversionRepositoryOwnedDpoTransactionContext(
                Enabled: true,
                Address: address,
                RequestIdentity: identity);
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(
                    fixture.Rejected,
                    fixture.Chosen),
                deterministic,
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);

            var completed = await coordinator.RunAsync(new(
                "repository-owned-dpo-existing-reuse-loop",
                CreateInternalRequest(fixture.Rejected.SubmittedRequest) with
                {
                    Budget = fixture.Rejected.SubmittedRequest.Budget with
                    {
                        MaxTeacherCalls = 1,
                        MaxImprovementCandidates = 2
                    }
                },
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>())
            {
                RepositoryOwnedDpoTransactionContext = context
            });

            Assert.True(completed.IsSuccess, completed.ErrorMessage);
            Assert.Equal(
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetExistingReusedStop,
                completed.Value!.StopReason);
            Assert.False(completed.Value.CandidateStrategyFailed);
            Assert.NotNull(completed.Value.FinalEvaluation);
            Assert.Equal(0, teacher.InvocationCount);
            Assert.Equal(0, deterministic.InvocationCount);
            Assert.Equal(
                bool.TrueString,
                completed.Value.PublicMetadata[
                    DesignConversionRepositoryOwnedDpoTransactionContext
                        .EnabledMetadataKey]);
            Assert.Equal(
                bool.TrueString,
                completed.Value.PublicMetadata[
                    DesignConversionRepositoryOwnedDpoTransactionContext
                        .ValidationMetadataKey]);
            Assert.Equal(
                RepositoryOwnedDpoDatasetResolutionSource
                    .ExistingSealedRepositoryDataset
                    .ToString(),
                completed.Value.PublicMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .RepositoryOwnedDpoDatasetResolutionSourceMetadataKey]);
            Assert.Equal(
                "1",
                completed.Value.PublicMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .RepositoryOwnedDpoDatasetPairCountMetadataKey]);
            Assert.Equal(
                manifest.ManifestSha256,
                completed.Value.PublicMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .RepositoryOwnedDpoDatasetManifestSha256MetadataKey]);
            Assert.Equal(
                bool.FalseString,
                completed.Value.PublicMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .RepositoryOwnedDpoDatasetLocalCandidateCreatedMetadataKey]);
            Assert.Contains(
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetExistingReusedMarker,
                completed.Value.EvidenceRefs);
            Assert.Contains(
                "improvement_loop:" +
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetManifestSha256MetadataKey +
                "=" + manifest.ManifestSha256,
                completed.Value.EvidenceRefs);
            await AssertNoLocalDpoCandidateWasWrittenAsync(artifactDb);
            var reopened = AssertSuccess(
                RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                    address,
                    identity));
            Assert.Equal(manifest.ManifestSha256, reopened.Manifest.ManifestSha256);
            Assert.Equal(1, reopened.Pairs.Count);
            var serializedDataset = JsonSerializer.Serialize(new
            {
                reopened.Manifest,
                reopened.Pairs
            });
            Assert.DoesNotContain(
                paidReferenceRawText,
                serializedDataset,
                StringComparison.Ordinal);
        }
        finally
        {
            TryDelete(root);
            TryDelete(repositoryRoot);
        }
    }

    [Fact]
    public async Task ImprovementLoopCompletion_EnabledRepositoryOwnedDpoContextMissingCleanDatasetCreatesSealedDatasetThenReusesIt()
    {
        var fixture = CreateProviderAllowedPair(
            "repository-owned-dpo-missing");
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-dpo-missing-reuse-" +
            Guid.NewGuid().ToString("N"));
        var repositoryRoot = CreateRepositoryOwnedDpoRepositoryRoot(
            "missing-reuse");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = new FakeTeacherInferenceClient();
        using var teacherRuntime = CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        var deterministic = new CountingCandidateStrategy(
            fixture.Chosen.RuntimeFlowResult.Candidate);
        try
        {
            var address = CreateRepositoryOwnedDpoDatasetAddress(
                repositoryRoot);
            var identity = CreateRepositoryOwnedDpoRequestIdentity(
                "missing-reuse");
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(
                    fixture.Rejected,
                    fixture.Chosen),
                deterministic,
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);

            var completed = await coordinator.RunAsync(new(
                "repository-owned-dpo-missing-reuse-loop",
                CreateInternalRequest(fixture.Rejected.SubmittedRequest) with
                {
                    Budget = fixture.Rejected.SubmittedRequest.Budget with
                    {
                        MaxTeacherCalls = 1,
                        MaxImprovementCandidates = 2
                    }
                },
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>())
            {
                RepositoryOwnedDpoTransactionContext =
                    new DesignConversionRepositoryOwnedDpoTransactionContext(
                        Enabled: true,
                        Address: address,
                        RequestIdentity: identity)
            });

            Assert.True(completed.IsSuccess, completed.ErrorMessage);
            Assert.False(
                completed.Value!.CandidateStrategyFailed,
                "The clean-missing repository DPO path should create and seal " +
                "the dataset instead of ending as a candidate-strategy failure. " +
                "StopReason=" + completed.Value.StopReason +
                "; Warnings=" + string.Join("|", completed.Value.Warnings));
            Assert.Equal(1, teacher.InvocationCount);
            Assert.Equal(0, deterministic.InvocationCount);
            Assert.Equal(
                RepositoryOwnedDpoDatasetResolutionSource
                    .NewlyPersistedRepositoryDataset
                    .ToString(),
                completed.Value.PublicMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .RepositoryOwnedDpoDatasetResolutionSourceMetadataKey]);
            Assert.Equal(
                "2",
                completed.Value.PublicMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .RepositoryOwnedDpoDatasetPairCountMetadataKey]);
            Assert.Equal(
                bool.FalseString,
                completed.Value.PublicMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .RepositoryOwnedDpoDatasetLocalCandidateCreatedMetadataKey]);
            Assert.Contains(
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetCleanMissGenerationAuthorizedMarker,
                completed.Value.EvidenceRefs);
            await AssertNoLocalDpoCandidateWasWrittenAsync(artifactDb);
            var reopened = AssertSuccess(
                RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                    address,
                    identity));
            Assert.Equal(2, reopened.Pairs.Count);
            Assert.All(
                reopened.Pairs,
                pair => Assert.Equal("figma-html-css", pair.Domain));
            Assert.Equal(
                2,
                reopened.Pairs
                    .Select(pair => pair.TeacherProvenance?.RequestId)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.All(
                reopened.Pairs,
                pair =>
                {
                    Assert.StartsWith(
                        "repository-owned-dpo-child:sha256:",
                        pair.TeacherProvenance?.RequestId,
                        StringComparison.Ordinal);
                    Assert.False(string.IsNullOrWhiteSpace(pair.Prompt));
                    Assert.False(string.IsNullOrWhiteSpace(pair.Chosen));
                    Assert.False(string.IsNullOrWhiteSpace(pair.Rejected));
                });
            Assert.Equal(
                reopened.Manifest.ManifestSha256,
                completed.Value.PublicMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .RepositoryOwnedDpoDatasetManifestSha256MetadataKey]);
            var firstReopenedDataset = JsonSerializer.Serialize(new
            {
                reopened.Manifest,
                reopened.Pairs
            });

            var reused = await coordinator.RunAsync(new(
                "repository-owned-dpo-missing-reuse-second-loop",
                CreateInternalRequest(fixture.Rejected.SubmittedRequest) with
                {
                    Budget = fixture.Rejected.SubmittedRequest.Budget with
                    {
                        MaxTeacherCalls = 1,
                        MaxImprovementCandidates = 2
                    }
                },
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>())
            {
                RepositoryOwnedDpoTransactionContext =
                    new DesignConversionRepositoryOwnedDpoTransactionContext(
                        Enabled: true,
                        Address: address,
                        RequestIdentity: identity)
            });

            Assert.True(reused.IsSuccess, reused.ErrorMessage);
            Assert.Equal(1, teacher.InvocationCount);
            Assert.Equal(
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetExistingReusedStop,
                reused.Value!.StopReason);
            Assert.Equal(
                RepositoryOwnedDpoDatasetResolutionSource
                    .ExistingSealedRepositoryDataset
                    .ToString(),
                reused.Value.PublicMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .RepositoryOwnedDpoDatasetResolutionSourceMetadataKey]);
            var reopenedAfterReuse = AssertSuccess(
                RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                    address,
                    identity));
            Assert.Equal(
                reopened.Manifest.ManifestSha256,
                reopenedAfterReuse.Manifest.ManifestSha256);
            Assert.Equal(
                firstReopenedDataset,
                JsonSerializer.Serialize(new
                {
                    reopenedAfterReuse.Manifest,
                    reopenedAfterReuse.Pairs
                }));
            await AssertNoLocalDpoCandidateWasWrittenAsync(artifactDb);
        }
        finally
        {
            TryDelete(root);
            TryDelete(repositoryRoot);
        }
    }

    [Fact]
    public async Task ImprovementLoopCompletion_EnabledRepositoryOwnedDpoContextIdentityMismatchStopsBeforeTeacherOrLocalCandidate()
    {
        var fixture = CreateProviderAllowedPair(
            "repository-owned-dpo-identity-mismatch");
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-dpo-identity-mismatch-" +
            Guid.NewGuid().ToString("N"));
        var repositoryRoot = CreateRepositoryOwnedDpoRepositoryRoot(
            "identity-mismatch");
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = new FakeTeacherInferenceClient();
        using var teacherRuntime = CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        var deterministic = new CountingCandidateStrategy(
            fixture.Chosen.RuntimeFlowResult.Candidate);
        try
        {
            var address = CreateRepositoryOwnedDpoDatasetAddress(
                repositoryRoot);
            var sealedIdentity = CreateRepositoryOwnedDpoRequestIdentity(
                "identity-mismatch-sealed");
            var requestedIdentity = CreateRepositoryOwnedDpoRequestIdentity(
                "identity-mismatch-requested");
            var pair = CreateRepositoryOwnedDpoPair(
                "identity-mismatch",
                sealedIdentity,
                "paid answer for the sealed identity");
            var manifest = AssertSuccess(
                RepositoryOwnedDpoDatasetManifestSealer.PersistAndSeal(
                    address,
                    sealedIdentity,
                    [pair]));
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(
                    fixture.Rejected,
                    fixture.Chosen),
                deterministic,
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);

            var completed = await coordinator.RunAsync(new(
                "repository-owned-dpo-identity-mismatch-loop",
                CreateInternalRequest(fixture.Rejected.SubmittedRequest) with
                {
                    Budget = fixture.Rejected.SubmittedRequest.Budget with
                    {
                        MaxTeacherCalls = 1,
                        MaxImprovementCandidates = 2
                    }
                },
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>())
            {
                RepositoryOwnedDpoTransactionContext =
                    new DesignConversionRepositoryOwnedDpoTransactionContext(
                        Enabled: true,
                        Address: address,
                        RequestIdentity: requestedIdentity)
            });

            Assert.True(completed.IsSuccess, completed.ErrorMessage);
            Assert.Equal(
                DesignConversionImprovementLoopCoordinator.CandidateStrategyFailed,
                completed.Value!.StopReason);
            Assert.True(completed.Value.CandidateStrategyFailed);
            Assert.Equal(0, teacher.InvocationCount);
            Assert.Equal(0, deterministic.InvocationCount);
            AssertRepositoryOwnedDpoUnavailableStop(
                completed.Value,
                RepositoryOwnedDpoDatasetManifestSealer
                    .RequestIdentityMismatchErrorCode);
            await AssertNoLocalDpoCandidateWasWrittenAsync(artifactDb);
            var reopenedWithSealedIdentity = AssertSuccess(
                RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                    address,
                    sealedIdentity));
            Assert.Equal(
                manifest.ManifestSha256,
                reopenedWithSealedIdentity.Manifest.ManifestSha256);
            var mismatchOpen =
                RepositoryOwnedDpoDatasetManifestSealer.OpenSealed(
                    address,
                    requestedIdentity);
            Assert.False(mismatchOpen.IsSuccess);
            Assert.Equal(
                RepositoryOwnedDpoDatasetManifestSealer
                    .RequestIdentityMismatchErrorCode,
                mismatchOpen.ErrorCode);
        }
        finally
        {
            TryDelete(root);
            TryDelete(repositoryRoot);
        }
    }

    [Fact]
    public async Task ImprovementLoopCompletion_ProviderMissingPendingCandidateKeepsLocalIdentityButApprovalFailsClosed()
    {
        var fixture = CreatePair("coordinator-provider-missing");
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-dpo-teacher-provider-missing-" +
            Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = new FakeTeacherInferenceClient();
        using var teacherRuntime = CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        try
        {
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(fixture.Rejected, fixture.Chosen),
                new SingleCandidateStrategy(
                    fixture.Chosen.RuntimeFlowResult.Candidate),
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);

            var completed = await coordinator.RunAsync(new(
                "teacher-provider-missing-loop",
                CreateInternalRequest(fixture.Rejected.SubmittedRequest),
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>()));

            Assert.True(
                completed.IsSuccess,
                completed.ErrorMessage + " " + completed.ErrorCode);
            var records = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            Assert.True(records.IsSuccess, records.ErrorMessage);
            var localRecord = Assert.Single(records.Value!);
            Assert.Equal(2, localRecord.DpoCandidates.Count);
            var candidate = localRecord.DpoCandidates[0];
            Assert.All(localRecord.DpoCandidates, item =>
            {
                var state = BuildCandidateStateMessage(localRecord, item);
                Assert.True(item.TrainingCandidateOnly, state);
                Assert.True(item.ProductNotReady, state);
                Assert.Contains(
                    "provider_missing_or_blocked",
                    item.BlockReasons);
                Assert.Contains(
                    "training_record_not_trainable",
                    item.BlockReasons);
                Assert.Equal(
                    bool.FalseString,
                    ReadMetadata(
                        item.TrainingRecord.Provenance.Metadata,
                        "approved_for_training"));
            });

            var api = new DesignConversionDpoUsefulnessApi(artifactDb);
            var approval = await api.ApproveRuntimeCreatedDpoCandidateAsync(
                localRecord.RecordId,
                candidate.CandidateId,
                "reviewer:provider-missing",
                "provider boundary must stay closed",
                DateTimeOffset.UnixEpoch.AddMinutes(1));

            Assert.False(approval.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_DPO_APPROVAL_BLOCKED",
                approval.ErrorCode);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Theory]
    [InlineData(0, 0, 0, "True", "True")]
    [InlineData(1, 2, 1, "True", "True")]
    [InlineData(2, 2, 1, "False", "True")]
    [InlineData(null, 2, 1, "False", "False")]
    public async Task ImprovementLoopCompletion_PreferenceAuthoringRequestScopedCapCountsAvailableCandidateOpportunities(
        int? maxTeacherCalls,
        int expectedCandidateCount,
        int expectedObservedCalls,
        string expectedCapReached,
        string expectedEnforcement)
    {
        var fixture = CreatePair("coordinator-cap-" + (maxTeacherCalls?.ToString() ?? "none"));
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-dpo-teacher-coordinator-cap-" +
            Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = new FakeTeacherInferenceClient();
        using var teacherRuntime = CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        try
        {
            var initialRequest = CreateInternalRequest(
                fixture.Rejected.SubmittedRequest) with
            {
                Budget = fixture.Rejected.SubmittedRequest.Budget with
                {
                    MaxTeacherCalls = maxTeacherCalls,
                    MaxImprovementCandidates = 2
                }
            };
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(fixture.Rejected, fixture.Chosen),
                new SingleCandidateStrategy(fixture.Chosen.RuntimeFlowResult.Candidate),
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);

            var completed = await coordinator.RunAsync(
                new DesignConversionImprovementLoopRequest(
                    "teacher-coordinator-cap-loop",
                    initialRequest,
                    MaxAttempts: 2,
                    Metadata: new Dictionary<string, string>()));

            Assert.True(completed.IsSuccess, completed.ErrorMessage);
            var records = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            Assert.True(records.IsSuccess, records.ErrorMessage);
            var stored = Assert.Single(records.Value!);
            Assert.Equal(expectedCandidateCount, stored.DpoCandidates.Count);
            Assert.Equal(expectedObservedCalls, teacher.InvocationCount);
            if (expectedCandidateCount == 2)
            {
                Assert.Equal(
                    new[]
                    {
                        DesignConversionDpoUnits.CriticUnit,
                        DesignConversionDpoUnits.ImproverUnit
                    },
                    stored.DpoCandidates.Select(candidate => candidate.UnitId));
                Assert.All(stored.DpoCandidates, candidate =>
                {
                    Assert.Equal(
                        bool.TrueString,
                        candidate.TrainingRecord.Provenance.Metadata[
                            "runtime_teacher_authored_candidate"]);
                    Assert.False(candidate.TrainingRecord.Provenance.Metadata.ContainsKey(
                        "runtime_teacher_selected_preference"));
                    Assert.Equal(
                        bool.TrueString,
                        candidate.TrainingRecord.Provenance.Metadata[
                            "independent_preference_proven"]);
                    Assert.Equal(
                        bool.FalseString,
                        candidate.TrainingRecord.Provenance.Metadata[
                            "trained_model_authority"]);
                });
            }
            Assert.Equal(
                expectedObservedCalls.ToString(),
                completed.Value!.PublicMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .MaxTeacherCallsObservedCallCountMetadataKey]);
            Assert.Equal(
                expectedCapReached,
                completed.Value.PublicMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .MaxTeacherCallsCapReachedMetadataKey]);
            Assert.Equal(
                expectedEnforcement,
                completed.Value.PublicMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .MaxTeacherCallsRuntimeEnforcementMetadataKey]);
            Assert.Equal(
                "contract_transport_with_dpo_attestation_teacher_calls_request_scoped_cap_v2",
                completed.Value.PublicMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .MaxTeacherCallsContractBoundaryMetadataKey]);
            foreach (var key in TeacherCallMetadataKeys())
            {
                Assert.Equal(
                    completed.Value.PublicMetadata[key],
                    stored.Metadata[key]);
            }
            if (expectedObservedCalls > 0)
            {
                Assert.Contains(
                    DesignConversionImprovementLoopCoordinator
                        .ObservedTeacherCallEvidenceRefPrefix +
                    expectedObservedCalls,
                    stored.EvidenceRefs);
            }
            Assert.Contains(
                "improvement_loop:" +
                DesignConversionImprovementLoopCoordinator
                    .MaxTeacherCallsObservedCallCountMetadataKey +
                "=" + expectedObservedCalls,
                stored.EvidenceRefs);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task ImprovementLoopCompletion_NoEligiblePairDoesNotSpendTeacherCall()
    {
        var fixture = CreatePair("coordinator-no-pair");
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = new FakeTeacherInferenceClient();
        using var teacherRuntime = CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        try
        {
            var initialRequest = CreateInternalRequest(fixture.Chosen.SubmittedRequest) with
            {
                Budget = fixture.Chosen.SubmittedRequest.Budget with
                {
                    MaxTeacherCalls = 1
                }
            };
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new SequenceEvaluator(fixture.Chosen),
                new SingleCandidateStrategy(fixture.Chosen.RuntimeFlowResult.Candidate),
                artifactDb);

            var completed = await coordinator.RunAsync(new(
                "teacher-coordinator-no-pair-loop",
                initialRequest,
                MaxAttempts: 1,
                Metadata: new Dictionary<string, string>()));

            Assert.True(completed.IsSuccess, completed.ErrorMessage);
            Assert.Equal(0, teacher.InvocationCount);
            Assert.Equal(
                "0",
                completed.Value!.PublicMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .MaxTeacherCallsObservedCallCountMetadataKey]);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task ThinClientPersistence_WithoutTypedHandoffNeverCallsTeacherOrCreatesDpoAcrossProviderAssociation(
        int maxTeacherCalls)
    {
        var fixture = CreatePair("thin");
        var matchingRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-dpo-teacher-thin-match-" + Guid.NewGuid().ToString("N"));
        var mismatchedRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-dpo-teacher-thin-mismatch-" + Guid.NewGuid().ToString("N"));
        var ledgerRoot = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-dpo-teacher-ledger-" + Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var otherProvider = new DesignConversionDpoPolicyBaseTestProvider();
        using var teacher = new FakeTeacherInferenceClient();
        using var teacherRuntime =
            CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        try
        {
            var matchingDb = new JsonlDesignConversionLocalArtifactDb(
                matchingRoot,
                dpoPolicyBaseProvider: provider);
            var matching = CreateThinRuntime(
                fixture,
                matchingDb,
                Path.Combine(ledgerRoot, "matching"));
            var matchingResult = await SubmitAndRunAsync(
                matching,
                fixture.Chosen.SubmittedRequest with
                {
                    Budget = fixture.Chosen.SubmittedRequest.Budget with
                    {
                        MaxTeacherCalls = maxTeacherCalls
                    }
                });
            Assert.True(matchingResult.IsSuccess, matchingResult.ErrorMessage);
            var matchingValue = matchingResult.Value!;
            var matchingRecords = await matchingDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            var matchingStored = Assert.Single(matchingRecords.Value!);
            Assert.Empty(matchingStored.DpoCandidates);
            Assert.Equal(
                bool.FalseString,
                matchingValue.Job.Metadata["training_candidate_only"]);
            Assert.Equal(
                bool.FalseString,
                matchingValue.Provenance["training_candidate_only"]);
            Assert.Equal(0, teacher.InvocationCount);
            Assert.DoesNotContain(
                matchingValue.Warnings,
                warning => warning.StartsWith(
                    DesignConversionImprovementLoopCoordinator
                        .TeacherAuthoringErrorWarningPrefix,
                    StringComparison.Ordinal));

            var mismatchedDb = new JsonlDesignConversionLocalArtifactDb(
                mismatchedRoot,
                dpoPolicyBaseProvider: otherProvider);
            var mismatched = CreateThinRuntime(
                fixture,
                mismatchedDb,
                Path.Combine(ledgerRoot, "mismatched"));
            var mismatchedResult = await SubmitAndRunAsync(
                mismatched,
                fixture.Chosen.SubmittedRequest with
                {
                    RequestId = fixture.Chosen.SubmittedRequest.RequestId + "-mismatch",
                    Budget = fixture.Chosen.SubmittedRequest.Budget with
                    {
                        MaxTeacherCalls = maxTeacherCalls
                    }
                });
            Assert.True(mismatchedResult.IsSuccess, mismatchedResult.ErrorMessage);
            var mismatchedRecords = await mismatchedDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            Assert.Empty(Assert.Single(mismatchedRecords.Value!).DpoCandidates);
            Assert.Equal(0, teacher.InvocationCount);
            Assert.DoesNotContain(
                mismatchedResult.Value!.Warnings,
                warning => warning.StartsWith(
                    DesignConversionImprovementLoopCoordinator
                        .TeacherAuthoringErrorWarningPrefix,
                    StringComparison.Ordinal));
        }
        finally
        {
            TryDelete(matchingRoot);
            TryDelete(mismatchedRoot);
            TryDelete(ledgerRoot);
        }
    }

    [Fact]
    public async Task ThinClientPersistence_RealCoordinatorReceiptAtCapPreventsSecondRootCall()
    {
        var fixture = CreatePair("thin-issued-receipt-cap");
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = new FakeTeacherInferenceClient();
        using var teacherRuntime =
            CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        try
        {
            var rootRequest = CreateInternalRequest(
                fixture.Rejected.SubmittedRequest) with
            {
                Budget = fixture.Rejected.SubmittedRequest.Budget with
                {
                    MaxImprovementCandidates = 2,
                    MaxTeacherCalls = 1
                }
            };
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(fixture.Rejected, fixture.Chosen),
                new SingleCandidateStrategy(
                    fixture.Chosen.RuntimeFlowResult.Candidate),
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);
            var firstRoot = await coordinator.RunAsync(
                new DesignConversionImprovementLoopRequest(
                    "thin-issued-receipt-cap-loop",
                    rootRequest,
                    MaxAttempts: 2,
                    Metadata: new Dictionary<string, string>()));

            Assert.True(firstRoot.IsSuccess, firstRoot.ErrorMessage);
            Assert.Equal(1, teacher.InvocationCount);
            Assert.True(DesignConversionTeacherCallUsageReceiptAssociation.TryResolve(
                firstRoot.Value!,
                out var firstRootReceipt));
            Assert.NotNull(firstRootReceipt);
            Assert.Equal(1, firstRootReceipt!.ObservedCallCount);
            var projectedEvaluation =
                DesignConversionImprovementLoopThinClientJobEvaluator
                    .ProjectFinalEvaluation(
                        firstRoot.Value!,
                        rootRequest,
                        maxAttempts: 2,
                        maxAttemptsSource: "cross_root_production_projection_test");
            Assert.Contains(
                DesignConversionImprovementLoopThinClientJobEvaluator
                    .SubmitPathLoopExecutedMarker,
                projectedEvaluation.EvidenceRefs);
            Assert.True(DesignConversionTeacherCallUsageReceiptAssociation.TryResolve(
                projectedEvaluation,
                out var projectedReceipt));
            Assert.Same(firstRootReceipt, projectedReceipt);

            var firstRootRecords = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            Assert.True(firstRootRecords.IsSuccess, firstRootRecords.ErrorMessage);
            Assert.Equal(
                2,
                firstRootRecords.Value!.Sum(record => record.DpoCandidates.Count));

            var carriedFixture = new PairFixture(
                projectedEvaluation,
                fixture.Rejected);
            var runtime = CreateThinRuntime(
                carriedFixture,
                artifactDb,
                Path.Combine(root, "ledger"));
            var request = projectedEvaluation.SubmittedRequest with
            {
                Budget = projectedEvaluation.SubmittedRequest.Budget with
                {
                    MaxTeacherCalls = 1
                }
            };

            var completed = await SubmitAndRunAsync(runtime, request);

            Assert.True(completed.IsSuccess, completed.ErrorMessage);
            Assert.Equal(1, teacher.InvocationCount);
            Assert.Equal(
                "1",
                completed.Value!.Provenance[
                    DesignConversionImprovementLoopCoordinator
                        .MaxTeacherCallsObservedCallCountMetadataKey]);
            Assert.Equal(
                bool.TrueString,
                completed.Value.Provenance[
                    DesignConversionImprovementLoopCoordinator
                        .MaxTeacherCallsCapReachedMetadataKey]);
            Assert.DoesNotContain(
                completed.Value.Warnings,
                warning => warning.StartsWith(
                    DesignConversionImprovementLoopCoordinator
                        .TeacherAuthoringErrorWarningPrefix,
                    StringComparison.Ordinal));
            var records = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            Assert.True(records.IsSuccess, records.ErrorMessage);
            Assert.Equal(
                2,
                records.Value!.Sum(record => record.DpoCandidates.Count));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Theory]
    [MemberData(nameof(TeacherFailureCases))]
    public async Task ImprovementLoopCompletion_TeacherAuthoringFailuresUseOneSanitizedDeterministicFallback(
        string scenario,
        string expectedErrorCode,
        int expectedInvocationCount)
    {
        var fixture = CreatePair("coordinator-failure-" + scenario);
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-dpo-teacher-coordinator-failure-" +
            Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        var teacher = scenario == "absent"
            ? null
            : new FakeTeacherInferenceClient(scenario);
        using var teacherRuntime = teacher is null
            ? null
            : CreateTeacherRuntime(teacher);
        using var association = teacherRuntime is null
            ? null
            : DesignConversionDpoTeacherAttestationAssociation.Associate(
                provider,
                teacherRuntime);
        try
        {
            var deterministic = new CountingCandidateStrategy(
                fixture.Chosen.RuntimeFlowResult.Candidate);
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(fixture.Rejected, fixture.Chosen),
                deterministic,
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);

            var completed = await coordinator.RunAsync(
                new DesignConversionImprovementLoopRequest(
                    "teacher-coordinator-failure-" + scenario,
                    CreateInternalRequest(fixture.Rejected.SubmittedRequest),
                    MaxAttempts: 2,
                    Metadata: new Dictionary<string, string>()));

            Assert.True(completed.IsSuccess, completed.ErrorMessage);
            Assert.NotNull(completed.Value!.FinalEvaluation);
            Assert.Equal(1, deterministic.InvocationCount);
            var records = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            Assert.True(records.IsSuccess, records.ErrorMessage);
            var stored = Assert.Single(records.Value!);
            Assert.Empty(stored.DpoCandidates);
            Assert.Equal(expectedInvocationCount, teacher?.InvocationCount ?? 0);
            Assert.Equal(
                expectedInvocationCount.ToString(),
                completed.Value.PublicMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .MaxTeacherCallsObservedCallCountMetadataKey]);
            Assert.Equal(
                expectedInvocationCount.ToString(),
                stored.Metadata[
                    DesignConversionImprovementLoopCoordinator
                        .MaxTeacherCallsObservedCallCountMetadataKey]);
            foreach (var key in TeacherCallMetadataKeys())
            {
                Assert.Equal(
                    completed.Value.PublicMetadata[key],
                    stored.Metadata[key]);
            }
            Assert.Contains(
                "improvement_loop:" +
                DesignConversionImprovementLoopCoordinator
                    .MaxTeacherCallsObservedCallCountMetadataKey +
                "=" + expectedInvocationCount,
                stored.EvidenceRefs);
            if (expectedInvocationCount > 0)
            {
                Assert.Contains(
                    DesignConversionImprovementLoopCoordinator
                        .TeacherAuthoringErrorWarningPrefix + expectedErrorCode,
                    completed.Value.Warnings);
                Assert.Contains(
                    DesignConversionImprovementLoopCoordinator
                        .TeacherAuthoringFallbackMarker,
                    completed.Value.Warnings);
            }
            else
            {
                Assert.DoesNotContain(
                    completed.Value.Warnings,
                    warning => warning.StartsWith(
                        DesignConversionImprovementLoopCoordinator
                            .TeacherAuthoringErrorWarningPrefix,
                        StringComparison.Ordinal));
            }
            Assert.Equal(
                bool.FalseString,
                completed.Value.PublicMetadata["training_candidate_only"]);
            AssertTeacherRawResponseNotPersisted(
                teacher,
                completed.Value.Warnings,
                completed.Value,
                stored,
                root);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ImprovementLoopCompletion_ValidAuthoringUsesOneCallAndExactIncumbentBinding(
        bool useDisabledRepositoryOwnedDpoContext)
    {
        var fixture = CreatePair(
            "authoring-valid-generated-" +
            (useDisabledRepositoryOwnedDpoContext ? "disabled" : "null"));
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-authoring-valid-" + Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = new FakeTeacherInferenceClient();
        using var teacherRuntime = CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        var deterministic = new CountingCandidateStrategy(
            fixture.Chosen.RuntimeFlowResult.Candidate);
        try
        {
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(
                    fixture.Rejected,
                    fixture.Chosen),
                deterministic,
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);

            var completed = await coordinator.RunAsync(new(
                "teacher-authoring-valid-loop",
                CreateInternalRequest(fixture.Rejected.SubmittedRequest) with
                {
                    Budget = fixture.Rejected.SubmittedRequest.Budget with
                    {
                        MaxTeacherCalls = 1,
                        MaxImprovementCandidates = 2
                    }
                },
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>())
            {
                RepositoryOwnedDpoTransactionContext =
                    useDisabledRepositoryOwnedDpoContext
                        ? DesignConversionRepositoryOwnedDpoTransactionContext
                            .Disabled
                        : null
            });

            Assert.True(completed.IsSuccess, completed.ErrorMessage);
            Assert.Equal(1, teacher.InvocationCount);
            Assert.Equal(0, deterministic.InvocationCount);
            Assert.Equal(
                bool.FalseString,
                completed.Value!.PublicMetadata[
                    DesignConversionRepositoryOwnedDpoTransactionContext
                        .EnabledMetadataKey]);
            Assert.Equal(
                bool.TrueString,
                completed.Value.PublicMetadata[
                    DesignConversionRepositoryOwnedDpoTransactionContext
                        .ValidationMetadataKey]);
            Assert.NotNull(teacher.LastPrompt);
            using var prompt = JsonDocument.Parse(teacher.LastPrompt);
            Assert.Equal(
                DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
                prompt.RootElement.GetProperty("schema").GetString());
            var expectedIncumbentRef = prompt.RootElement
                .GetProperty("incumbent_ref")
                .GetString();
            var records = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            var stored = Assert.Single(records.Value!);
            Assert.Equal(
                bool.FalseString,
                stored.Metadata[
                    DesignConversionRepositoryOwnedDpoTransactionContext
                        .EnabledMetadataKey]);
            Assert.Equal(
                bool.TrueString,
                stored.Metadata[
                    DesignConversionRepositoryOwnedDpoTransactionContext
                        .ValidationMetadataKey]);
            Assert.Equal(2, stored.DpoCandidates.Count);
            Assert.Equal(
                new[]
                {
                    DesignConversionDpoUnits.CriticUnit,
                    DesignConversionDpoUnits.ImproverUnit
                },
                stored.DpoCandidates.Select(candidate => candidate.UnitId));
            Assert.All(stored.DpoCandidates, candidate =>
            {
                Assert.Equal(
                    expectedIncumbentRef,
                    candidate.TrainingRecord.Provenance.Metadata[
                        "teacher_authoring_incumbent_ref"]);
                Assert.Equal(
                    bool.TrueString,
                    candidate.TrainingRecord.Provenance.Metadata[
                        "runtime_teacher_authored_candidate"]);
                    Assert.False(candidate.TrainingRecord.Provenance.Metadata.ContainsKey(
                        "runtime_teacher_selected_preference"));
                    Assert.Equal(
                        bool.TrueString,
                        candidate.TrainingRecord.Provenance.Metadata[
                            "independent_preference_proven"]);
                Assert.Equal(
                    bool.FalseString,
                    candidate.TrainingRecord.Provenance.Metadata[
                        "trained_model_authority"]);
            });
            var projection = JsonSerializer.Serialize(new { completed, stored });
            Assert.DoesNotContain(teacher.LastOutput!, projection, StringComparison.Ordinal);
            Assert.DoesNotContain(teacher.LeakMarker, projection, StringComparison.Ordinal);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task TrainingIntake_DualTargetTeacherRowsRequireCorpusAdmissionBeforeOneRequestPerRowAuthority()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-teacher-payload-custody-success-" +
            Guid.NewGuid().ToString("N"));
        try
        {
            var intake = await CreateTeacherPayloadCustodyIntakeAsync(root);
            var custodyRoot = Path.Combine(
                root,
                DesignConversionCandidatePayloadCustodyStore.DirectoryName);
            var custodyBeforeRun = CaptureFileTree(custodyRoot);
            var finalPairStore = new RecordingDpoPairStore();
            var beforeRefreshCount = 0;
            var runnerInvoked = 0;
            var db = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeRunnerBeforeInvokeObserverForTesting:
                    () => Interlocked.Increment(ref beforeRefreshCount),
                runTrainingIntakeRunnerInvokedObserverForTesting:
                    () => Interlocked.Increment(ref runnerInvoked),
                legalTeacherAdmissionAuthority:
                    DesignConversionLegalTeacherCapabilityTestFactory
                        .CreateAdmitted(
                            modelId: ExpectedTeacherModelId,
                            expectedOllamaManifestDigest:
                                ExpectedTeacherManifestDigest,
                            expectedOllamaModelLayerDigest:
                                ExpectedTeacherModelLayerDigest),
                trainingIntakeDpoPairStoreForTesting: finalPairStore);

            var run = await db.RunTrainingIntakeAsync(intake.IntakeId);
            var canonicalPairs = finalPairStore.GetAll();

            Assert.False(run.IsSuccess);
            Assert.Equal(
                DesignConversionDpoCorpusAdmissionAuthority.RequiredErrorCode,
                run.ErrorCode);
            Assert.Equal(
                "Independently-ranked teacher candidates require a live corpus admission.",
                run.ErrorMessage);
            Assert.Equal(0, beforeRefreshCount);
            Assert.Equal(0, runnerInvoked);
            Assert.Equal(0, finalPairStore.AddCallCount);
            Assert.True(canonicalPairs.IsSuccess, canonicalPairs.ErrorMessage);
            Assert.Empty(canonicalPairs.Value!);
            Assert.False(File.Exists(Path.Combine(root, "training-intake-runs.jsonl")));
            Assert.Null(run.Value);
            Assert.Equal(custodyBeforeRun, CaptureFileTree(custodyRoot));
            Assert.True(DesignConversionSafeTrainingProjection.IsSupportedSchemaVersion(
                DesignConversionSafeTrainingProjection.SchemaVersion));
            Assert.False(DesignConversionSafeTrainingProjection.IsSupportedSchemaVersion(
                "design-conversion-safe-training-projection-v1"));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task TrainingIntake_CorpusAdmissionPrecedesSecondResolutionDuplicateAndAnyMutation()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-teacher-request-toctou-" +
            Guid.NewGuid().ToString("N"));
        try
        {
            var intake = await CreateTeacherPayloadCustodyIntakeAsync(root);
            var resolutionCount = 0;
            var beforeRefreshCount = 0;
            var runnerInvoked = 0;
            var finalPairStore = new RecordingDpoPairStore();
            var db = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeRunnerBeforeInvokeObserverForTesting:
                    () => Interlocked.Increment(ref beforeRefreshCount),
                runTrainingIntakeRunnerInvokedObserverForTesting:
                    () => Interlocked.Increment(ref runnerInvoked),
                legalTeacherAdmissionAuthority:
                    DesignConversionLegalTeacherCapabilityTestFactory
                        .CreateAdmitted(
                            modelId: ExpectedTeacherModelId,
                            expectedOllamaManifestDigest:
                                ExpectedTeacherManifestDigest,
                            expectedOllamaModelLayerDigest:
                                ExpectedTeacherModelLayerDigest),
                trainingIntakeDpoPairStoreForTesting: finalPairStore,
                runTrainingIntakeResolvedRecordsTransformerForTesting: records =>
                {
                    if (Interlocked.Increment(ref resolutionCount) != 1)
                        return records;

                    return records.Select((record, index) => record with
                    {
                        TeacherProvenance = record.TeacherProvenance is null
                            ? null
                            : record.TeacherProvenance with
                            {
                                RequestId = $"teacher-request-initially-unique-{index}"
                            }
                    }).ToArray();
                });
            var options = new DesignConversionTrainingPipelineOptions(
                RunId: "teacher-request-toctou",
                BatchSize: 1,
                MeasurementGateRequest:
                    BuildPassingAdmissionMeasurement("teacher-request-toctou"));

            var run = await db.RunTrainingIntakeAsync(intake.IntakeId, options);

            Assert.False(run.IsSuccess);
            Assert.Equal(
                DesignConversionDpoCorpusAdmissionAuthority.RequiredErrorCode,
                run.ErrorCode);
            Assert.Equal(
                "Independently-ranked teacher candidates require a live corpus admission.",
                run.ErrorMessage);
            Assert.Equal(1, resolutionCount);
            Assert.Equal(0, beforeRefreshCount);
            Assert.Equal(0, runnerInvoked);
            Assert.Equal(0, finalPairStore.AddCallCount);
            Assert.False(File.Exists(Path.Combine(root, "training-intake-runs.jsonl")));
            Assert.Null(run.Value);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Theory]
    [InlineData("missing", "teacher_payload_custody_missing_or_corrupt")]
    [InlineData("tampered", "teacher_payload_custody_missing_or_corrupt")]
    [InlineData("authority-mismatch", "teacher_payload_custody_missing_or_corrupt")]
    public async Task TrainingIntake_InvalidTeacherPayloadCustodyBlocksBeforeRunnerAndFinalPairStore(
        string scenario,
        string expectedBlockReason)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-teacher-payload-custody-" + scenario + "-" +
            Guid.NewGuid().ToString("N"));
        try
        {
            var intake = await CreateTeacherPayloadCustodyIntakeAsync(root);
            var committedPaths = Directory.GetFiles(
                Path.Combine(
                    root,
                    DesignConversionCandidatePayloadCustodyStore.DirectoryName,
                    "committed"),
                "*.custody-committed.json")
                .Order(StringComparer.Ordinal)
                .ToArray();
            Assert.Equal(2, committedPaths.Length);
            var committedPath = committedPaths[0];
            if (scenario == "missing")
            {
                File.Delete(committedPath);
            }
            else
            {
                var json = await File.ReadAllTextAsync(committedPath);
                var mutated = scenario == "tampered"
                    ? json.Replace(
                        "\"chosenResponse\":\"",
                        "\"chosenResponse\":\"tampered",
                        StringComparison.Ordinal)
                    : json.Replace("\"authorityFingerprintSha256\":\"sha256:",
                        "\"authorityFingerprintSha256\":\"sha256:0",
                        StringComparison.Ordinal);
                await File.WriteAllTextAsync(committedPath, mutated);
            }
            var custodyRoot = Path.Combine(
                root,
                DesignConversionCandidatePayloadCustodyStore.DirectoryName);
            var custodyBeforeRun = CaptureFileTree(custodyRoot);
            var finalPairStore = new RecordingDpoPairStore();
            var runnerInvoked = 0;
            var db = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeRunnerInvokedObserverForTesting:
                    () => Interlocked.Increment(ref runnerInvoked),
                trainingIntakeDpoPairStoreForTesting: finalPairStore);

            var run = await db.RunTrainingIntakeAsync(intake.IntakeId);

            Assert.True(run.IsSuccess, run.ErrorMessage);
            Assert.False(run.Value!.TrainingRunnerInvoked);
            Assert.Equal(0, runnerInvoked);
            Assert.Equal(0, finalPairStore.AddCallCount);
            Assert.False(File.Exists(Path.Combine(root, "training-intake-runs.jsonl")));
            Assert.False(run.Value.BoundaryMetadata.ContainsKey(
                "training_intake_idempotency_created_new_side_effect"));
            Assert.Equal(custodyBeforeRun, CaptureFileTree(custodyRoot));
            Assert.Contains(run.Value.BlockReasons, reason =>
                reason.StartsWith(expectedBlockReason, StringComparison.Ordinal));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task TrainingIntake_MeasuredInvalidTeacherPayloadCustodyAppendsBlockedResultWithoutRunner()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-teacher-payload-custody-measured-block-" +
            Guid.NewGuid().ToString("N"));
        try
        {
            var intake = await CreateTeacherPayloadCustodyIntakeAsync(root);
            var committedPath = Assert.Single(Directory.GetFiles(
                Path.Combine(
                    root,
                    DesignConversionCandidatePayloadCustodyStore.DirectoryName,
                    "committed"),
                "*.custody-committed.json").Order(StringComparer.Ordinal).Take(1));
            File.Delete(committedPath);
            var finalPairStore = new RecordingDpoPairStore();
            var beforeRefreshCount = 0;
            var runnerInvoked = 0;
            var db = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                runTrainingIntakeRunnerBeforeInvokeObserverForTesting:
                    () => Interlocked.Increment(ref beforeRefreshCount),
                runTrainingIntakeRunnerInvokedObserverForTesting:
                    () => Interlocked.Increment(ref runnerInvoked),
                trainingIntakeDpoPairStoreForTesting: finalPairStore);
            var options = new DesignConversionTrainingPipelineOptions(
                RunId: "teacher-payload-custody-measured-block",
                BatchSize: 1,
                MeasurementGateRequest:
                    BuildPassingAdmissionMeasurement(
                        "teacher-payload-custody-measured-block"));
            var admission = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["training_intake_tenant_id_ref"] =
                    "tenant:teacher-payload-custody-measured-block",
                ["training_intake_idempotency_key_ref"] =
                    "idempotency:teacher-payload-custody-measured-block"
            };

            var run = await db.RunTrainingIntakeAsync(
                intake.IntakeId,
                options,
                admissionBoundaryMetadata: admission);

            Assert.True(run.IsSuccess, run.ErrorMessage);
            Assert.NotNull(run.Value);
            Assert.False(run.Value.TrainingRunnerInvoked);
            Assert.Equal(
                "blocked_before_runner_by_training_intake_resolution",
                run.Value.TrainingExecutionBoundary);
            Assert.Equal(
                bool.TrueString,
                run.Value.BoundaryMetadata[
                    "training_intake_idempotency_created_new_side_effect"]);
            Assert.Equal(0, beforeRefreshCount);
            Assert.Equal(0, runnerInvoked);
            Assert.Equal(0, finalPairStore.AddCallCount);
            Assert.True(File.Exists(Path.Combine(root, "training-intake-runs.jsonl")));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task ImprovementLoopCompletion_UnavailableCandidatePayloadStoreFailsBeforeAuthorityJsonlAppend()
    {
        var fixture = CreateProviderAllowedPair(
            "candidate-payload-store-unavailable");
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-candidate-payload-store-unavailable-" +
            Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            createTrainingIntakeJournalWaitTaskObserverForTesting: null,
            createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
            dpoPolicyBaseProvider: provider,
            candidatePayloadCustodyFaultInjectionForTesting: point =>
                throw new IOException($"injected:{point}"));
        using var teacher = new FakeTeacherInferenceClient();
        using var teacherRuntime = CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        try
        {
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(
                    fixture.Rejected,
                    fixture.Chosen),
                new SingleCandidateStrategy(
                    fixture.Chosen.RuntimeFlowResult.Candidate),
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);

            await Assert.ThrowsAsync<
                DesignConversionCandidatePayloadCustodyCrashException>(() =>
                coordinator.RunAsync(new(
                    "candidate-payload-store-unavailable-loop",
                    CreateInternalRequest(fixture.Rejected.SubmittedRequest),
                    MaxAttempts: 2,
                    Metadata: new Dictionary<string, string>())));
            var authorityPath = Path.Combine(root, "conversion-results.jsonl");
            Assert.True(!File.Exists(authorityPath) || new FileInfo(authorityPath).Length == 0);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void CandidatePayloadCustodyStore_MultiEntryEnvelopeRoundTripsExactUtf8AndRejectsFabricatedEvidence()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-candidate-payload-custody-exact-" +
            Guid.NewGuid().ToString("N"));
        var otherRoot = root + "-other-store";
        try
        {
            const string firstPrompt = "  {\"request\":\"שלום\"}\r\n";
            const string firstChosen =
                "{\"html\":\"<main>  café שלום  </main>\",\"css\":\"main { gap:  2px; }\\n\"}";
            const string firstRejected =
                "{\"html\":\"<main>baseline</main>\",\"css\":\"main{}\"}";
            const string secondPrompt = "{\"request\":\"second\"}";
            const string secondChosen =
                "{\"html\":\"<section>二</section>\",\"css\":\"section { display: grid; }\"}";
            const string secondRejected =
                "{\"html\":\"<section>one</section>\",\"css\":\"section{}\"}";
            var entries = new[]
            {
                CreateCustodyEntry("record:first", firstPrompt, firstChosen, firstRejected),
                CreateCustodyEntry("record:second", secondPrompt, secondChosen, secondRejected)
            };
            var draft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                "authority:exact",
                TestSha256Ref("authority identity"),
                entries);
            Assert.True(draft.IsSuccess, draft.ErrorMessage);
            var commitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
                draft.Value!,
                TestSha256Ref("authority append fingerprint"));
            Assert.True(commitment.IsSuccess, commitment.ErrorMessage);
            var store = new DesignConversionCandidatePayloadCustodyStore(root);
            var prepared = store.Prepare(draft.Value!, commitment.Value!);
            Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
            var committed = store.Commit(prepared.Value!);
            Assert.True(committed.IsSuccess, committed.ErrorMessage);
            var resolved = store.ResolveSecretEnvelope(committed.Value!);
            Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
            Assert.Equal(2, resolved.Value!.Entries.Count);

            var first = DesignConversionCandidatePayloadCustodyStore.FindExactEntry(
                resolved.Value,
                committed.Value!,
                entries[0].BindingSha256);
            Assert.NotNull(first);
            Assert.Equal(firstPrompt, first.Secret.Prompt);
            Assert.Equal(firstChosen, first.Secret.ChosenResponse);
            Assert.Equal(firstRejected, first.Secret.RejectedResponse);
            var second = DesignConversionCandidatePayloadCustodyStore.FindExactEntry(
                resolved.Value,
                committed.Value!,
                entries[1].BindingSha256);
            Assert.NotNull(second);
            Assert.Equal(secondPrompt, second.Secret.Prompt);
            Assert.Equal(secondChosen, second.Secret.ChosenResponse);
            Assert.Equal(secondRejected, second.Secret.RejectedResponse);

            var fabricated = new DesignConversionCandidatePayloadCustodyStore
                .CommittedEvidence(commitment.Value!);
            Assert.False(DesignConversionCandidatePayloadCustodyStore.IsStoreMinted(
                fabricated));
            Assert.Null(DesignConversionCandidatePayloadCustodyStore.FindExactEntry(
                resolved.Value,
                fabricated,
                entries[0].BindingSha256));

            var mutableBindings = commitment.Value!.Bindings.ToArray();
            var commitmentSnapshot = new DesignConversionCandidatePayloadCustodyCommitment(
                commitment.Value.TransactionId,
                commitment.Value.ManifestSha256,
                commitment.Value.BindingSetSha256,
                commitment.Value.AuthorityRecordId,
                commitment.Value.AuthorityFingerprintSha256,
                mutableBindings);
            mutableBindings[0] = TestSha256Ref("caller mutation");
            Assert.Equal(commitment.Value.Bindings[0], commitmentSnapshot.Bindings[0]);
            Assert.False(commitmentSnapshot.Bindings is IList<string>);

            var unregisteredEnvelope = DesignConversionCandidatePayloadCustodyStore
                .ResolvedSecretEnvelope.CreateForMintedEvidence(
                    committed.Value!,
                    commitment.Value.TransactionId,
                    resolved.Value.Entries);
            Assert.Null(DesignConversionCandidatePayloadCustodyStore.FindExactEntry(
                unregisteredEnvelope,
                committed.Value!,
                entries[0].BindingSha256));

            var sameBindingDraft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                "authority:same-store-second-transaction",
                TestSha256Ref("same store second authority"),
                [entries[0]]);
            var sameBindingCommitment = DesignConversionCandidatePayloadCustodyStore
                .BindAuthority(
                    sameBindingDraft.Value!,
                    TestSha256Ref("same store second append"));
            Assert.NotEqual(
                commitment.Value.TransactionId,
                sameBindingCommitment.Value!.TransactionId);
            var sameBindingPrepared = store.Prepare(
                sameBindingDraft.Value!,
                sameBindingCommitment.Value);
            Assert.True(sameBindingPrepared.IsSuccess, sameBindingPrepared.ErrorMessage);
            var sameBindingEvidence = store.Commit(sameBindingPrepared.Value!);
            Assert.True(sameBindingEvidence.IsSuccess, sameBindingEvidence.ErrorMessage);
            var sameBindingEnvelope = store.ResolveSecretEnvelope(
                sameBindingEvidence.Value!);
            Assert.True(sameBindingEnvelope.IsSuccess, sameBindingEnvelope.ErrorMessage);
            var firstSecret = first.Secret;
            var secondTransactionSecret = Assert.Single(
                sameBindingEnvelope.Value!.Entries).Secret;
            var firstCapability = DesignConversionCandidatePayloadCustodyStore
                .MintTrainingCapability([committed.Value!]);
            var secondCapability = DesignConversionCandidatePayloadCustodyStore
                .MintTrainingCapability([sameBindingEvidence.Value!]);
            Assert.True(firstCapability.IsSuccess, firstCapability.ErrorMessage);
            Assert.True(secondCapability.IsSuccess, secondCapability.ErrorMessage);
            Assert.True(DesignConversionCandidatePayloadCustodyStore
                .HasExactCapabilityAuthorityBinding(
                    firstCapability.Value,
                    firstSecret));
            Assert.False(DesignConversionCandidatePayloadCustodyStore
                .HasExactCapabilityAuthorityBinding(
                    firstCapability.Value,
                    secondTransactionSecret));
            Assert.True(DesignConversionCandidatePayloadCustodyStore
                .HasExactCapabilityAuthorityBinding(
                    secondCapability.Value,
                    secondTransactionSecret));

            var otherStore = new DesignConversionCandidatePayloadCustodyStore(
                otherRoot);
            Assert.False(otherStore.ResolveSecretEnvelope(committed.Value!).IsSuccess);
            var otherEntry = CreateCustodyEntry(
                "record:other-store",
                "{\"request\":\"other\"}",
                "{\"html\":\"<main>other</main>\",\"css\":\"main{}\"}",
                "{\"html\":\"<main>old</main>\",\"css\":\"main{}\"}");
            var otherDraft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                "authority:other-store",
                TestSha256Ref("other authority"),
                [otherEntry]);
            var otherCommitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
                otherDraft.Value!,
                TestSha256Ref("other append"));
            var otherPrepared = otherStore.Prepare(
                otherDraft.Value!,
                otherCommitment.Value!);
            Assert.True(otherPrepared.IsSuccess, otherPrepared.ErrorMessage);
            var otherEvidence = otherStore.Commit(otherPrepared.Value!);
            Assert.True(otherEvidence.IsSuccess, otherEvidence.ErrorMessage);
            Assert.False(DesignConversionCandidatePayloadCustodyStore
                .MintTrainingCapability([committed.Value!, otherEvidence.Value!]).IsSuccess);

            foreach (var secretBearing in new object[]
                     {
                         entries[0].Secret,
                         entries[0],
                         draft.Value!,
                         resolved.Value
                     })
            {
                var serialized = JsonSerializer.Serialize(secretBearing,
                    secretBearing.GetType());
                Assert.DoesNotContain("café שלום", serialized, StringComparison.Ordinal);
                Assert.DoesNotContain("café שלום", secretBearing.ToString(), StringComparison.Ordinal);
            }
            var evidenceJson = JsonSerializer.Serialize(
                committed.Value!,
                committed.Value!.GetType());
            Assert.DoesNotContain("café שלום", evidenceJson, StringComparison.Ordinal);
            Assert.DoesNotContain("café שלום", committed.Value.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            TryDelete(root);
            TryDelete(otherRoot);
        }
    }

    [Fact]
    public void CandidatePayloadCustodyStore_ReconcileHasNoPermanentTwoHundredFiftySixCommitCap()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-candidate-payload-custody-unbounded-committed-" +
            Guid.NewGuid().ToString("N"));
        try
        {
            var store = new DesignConversionCandidatePayloadCustodyStore(root);
            var authorities =
                new Dictionary<string, DesignConversionCandidatePayloadCustodyCommitment>(
                    StringComparer.Ordinal);
            for (var index = 0;
                 index <= DesignConversionCandidatePayloadCustodyStore
                     .MaximumStagedTransactions;
                 index++)
            {
                var scope = "record:committed:" + index;
                var entry = CreateCustodyEntry(
                    scope,
                    "{\"request\":\"" + index + "\"}",
                    "{\"html\":\"<main>" + index + "</main>\",\"css\":\"main{}\"}",
                    "{\"html\":\"<main>old</main>\",\"css\":\"main{}\"}");
                var draft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                    scope,
                    TestSha256Ref("authority identity " + index),
                    [entry]);
                Assert.True(draft.IsSuccess, draft.ErrorMessage);
                var commitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
                    draft.Value!,
                    TestSha256Ref("authority fingerprint " + index));
                Assert.True(commitment.IsSuccess, commitment.ErrorMessage);
                var prepared = store.Prepare(draft.Value!, commitment.Value!);
                Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
                var committed = store.Commit(prepared.Value!);
                Assert.True(committed.IsSuccess, committed.ErrorMessage);
                authorities.Add(commitment.Value!.TransactionId, commitment.Value);
            }

            var reconciled = store.Reconcile(authorities);

            Assert.True(reconciled.IsSuccess, reconciled.ErrorMessage);
            Assert.Equal(
                DesignConversionCandidatePayloadCustodyStore.MaximumStagedTransactions + 1,
                Directory.GetFiles(
                    Path.Combine(
                        root,
                        DesignConversionCandidatePayloadCustodyStore.DirectoryName,
                        "committed"),
                    "*.custody-committed.json").Length);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void CandidatePayloadCustodyStore_ReconcileAcceptsIdenticalStageAndCommitButRejectsMismatch()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-candidate-payload-custody-dual-state-" +
            Guid.NewGuid().ToString("N"));
        try
        {
            var entry = CreateCustodyEntry(
                "record:dual-state",
                "{\"request\":\"dual\"}",
                "{\"html\":\"<main>new</main>\",\"css\":\"main{}\"}",
                "{\"html\":\"<main>old</main>\",\"css\":\"main{}\"}");
            var draft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                "authority:dual-state",
                TestSha256Ref("authority dual state identity"),
                [entry]);
            var commitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
                draft.Value!,
                TestSha256Ref("authority dual state fingerprint"));
            var store = new DesignConversionCandidatePayloadCustodyStore(root);
            var prepared = store.Prepare(draft.Value!, commitment.Value!);
            var committed = store.Commit(prepared.Value!);
            Assert.True(committed.IsSuccess, committed.ErrorMessage);

            var custodyRoot = Path.Combine(
                root,
                DesignConversionCandidatePayloadCustodyStore.DirectoryName);
            var committedPath = Assert.Single(Directory.GetFiles(
                Path.Combine(custodyRoot, "committed"),
                "*.custody-committed.json"));
            var stagePath = Path.Combine(
                custodyRoot,
                "staged",
                Path.GetFileName(committedPath).Replace(
                    ".custody-committed.json",
                    ".custody-stage.json",
                    StringComparison.Ordinal));
            WriteOwnerOnlyCustodyFile(stagePath, File.ReadAllBytes(committedPath));
            var authorities =
                new Dictionary<string, DesignConversionCandidatePayloadCustodyCommitment>(
                    StringComparer.Ordinal)
                {
                    [commitment.Value!.TransactionId] = commitment.Value
                };

            var identical = store.Reconcile(authorities);

            Assert.True(identical.IsSuccess, identical.ErrorMessage);
            Assert.False(File.Exists(stagePath));

            WriteOwnerOnlyCustodyFile(stagePath, File.ReadAllBytes(committedPath));
            var stagedBytes = File.ReadAllBytes(stagePath);
            stagedBytes[^2] ^= 1;
            WriteOwnerOnlyCustodyFile(stagePath, stagedBytes, FileMode.Create);

            var mismatch = store.Reconcile(authorities);

            Assert.False(mismatch.IsSuccess);
            Assert.True(File.Exists(stagePath));
            Assert.True(File.Exists(committedPath));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void CandidatePayloadCustodyStore_ReconcileRecoversStageWithAuthorityAndDeletesCrashOrphan()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-candidate-payload-custody-recovery-" +
            Guid.NewGuid().ToString("N"));
        try
        {
            var entry = CreateCustodyEntry(
                "record:recover",
                "{\"request\":\"recover\"}",
                "{\"html\":\"<main>recover</main>\",\"css\":\"main{}\"}",
                "{\"html\":\"<main>old</main>\",\"css\":\"main{}\"}");
            var draft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                "authority:recover",
                TestSha256Ref("authority recovery identity"),
                [entry]);
            var commitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
                draft.Value!,
                TestSha256Ref("authority recovery fingerprint"));
            var store = new DesignConversionCandidatePayloadCustodyStore(root);
            var prepared = store.Prepare(draft.Value!, commitment.Value!);
            Assert.True(prepared.IsSuccess, prepared.ErrorMessage);

            var recovered = store.Reconcile(
                new Dictionary<string, DesignConversionCandidatePayloadCustodyCommitment>(
                    StringComparer.Ordinal)
                {
                    [commitment.Value!.TransactionId] = commitment.Value
                });
            Assert.True(recovered.IsSuccess, recovered.ErrorMessage);
            Assert.True(store.ResolveCommitted(commitment.Value).IsSuccess);

            var orphanRoot = root + "-orphan";
            try
            {
                var crashing = new DesignConversionCandidatePayloadCustodyStore(
                    orphanRoot,
                    point =>
                    {
                        if (string.Equals(point, "after_stage_publish", StringComparison.Ordinal))
                            throw new IOException("injected crash after stage");
                    });
                Assert.Throws<DesignConversionCandidatePayloadCustodyCrashException>(
                    () => crashing.Prepare(draft.Value!, commitment.Value!));
                Assert.Single(Directory.GetFiles(
                    Path.Combine(
                        orphanRoot,
                        DesignConversionCandidatePayloadCustodyStore.DirectoryName,
                        "staged"),
                    "*.custody-stage.json"));
                var orphanRecovery = new DesignConversionCandidatePayloadCustodyStore(orphanRoot)
                    .Reconcile(
                        new Dictionary<string, DesignConversionCandidatePayloadCustodyCommitment>(
                            StringComparer.Ordinal));
                Assert.True(orphanRecovery.IsSuccess, orphanRecovery.ErrorMessage);
                Assert.Empty(Directory.GetFiles(
                    Path.Combine(
                        orphanRoot,
                        DesignConversionCandidatePayloadCustodyStore.DirectoryName,
                        "staged"),
                    "*.custody-stage.json"));
            }
            finally
            {
                TryDelete(orphanRoot);
            }
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Theory]
    [InlineData("before_commit_rename")]
    [InlineData("after_commit_rename")]
    public void CandidatePayloadCustodyStore_CommitCrashReconcilesFromAuthority(
        string crashPoint)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-custody-commit-crash-" + crashPoint + "-" +
            Guid.NewGuid().ToString("N"));
        try
        {
            var entry = CreateCustodyEntry(
                "record:" + crashPoint,
                "{\"request\":\"crash\"}",
                "{\"html\":\"<main>new</main>\",\"css\":\"main{}\"}",
                "{\"html\":\"<main>old</main>\",\"css\":\"main{}\"}");
            var draft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                "authority:" + crashPoint,
                TestSha256Ref("crash authority " + crashPoint),
                [entry]);
            var commitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
                draft.Value!,
                TestSha256Ref("crash append " + crashPoint));
            var initial = new DesignConversionCandidatePayloadCustodyStore(root);
            var prepared = initial.Prepare(draft.Value!, commitment.Value!);
            Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
            var crashing = new DesignConversionCandidatePayloadCustodyStore(
                root,
                point =>
                {
                    if (string.Equals(point, crashPoint, StringComparison.Ordinal))
                        throw new IOException("injected commit crash");
                });

            Assert.Throws<DesignConversionCandidatePayloadCustodyCrashException>(
                () => crashing.Commit(prepared.Value!));
            var recovering = new DesignConversionCandidatePayloadCustodyStore(root);
            var reconciled = recovering.Reconcile(
                new Dictionary<string, DesignConversionCandidatePayloadCustodyCommitment>(
                    StringComparer.Ordinal)
                {
                    [commitment.Value!.TransactionId] = commitment.Value
                });

            Assert.True(reconciled.IsSuccess, reconciled.ErrorMessage);
            Assert.True(recovering.ResolveCommitted(commitment.Value).IsSuccess);
            Assert.Empty(Directory.GetFiles(
                Path.Combine(
                    root,
                    DesignConversionCandidatePayloadCustodyStore.DirectoryName,
                    "staged"),
                "*.custody-stage.json"));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void CandidatePayloadCustodyStore_ReconcileRecoversCurrentAuthorityAndRetiresOnlyImmediatePredecessor()
    {
        using var rootScope = WorkspaceLocalTestRoot.Create(
            "design-conversion-candidate-payload-custody",
            "successor");
        using var unrelatedScope = WorkspaceLocalTestRoot.Create(
            "design-conversion-candidate-payload-custody",
            "unrelated");
        var root = rootScope.Path;
        var unrelatedRoot = unrelatedScope.Path;
        try
        {
            var store = new DesignConversionCandidatePayloadCustodyStore(root);
            var oldEntry = CreateCustodyEntry(
                "record:successor:first",
                "{\"request\":\"first\"}",
                "{\"html\":\"<main>first</main>\",\"css\":\"main{}\"}",
                "{\"html\":\"<main>old</main>\",\"css\":\"main{}\"}");
            var oldDraft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                "authority:successor-chain",
                TestSha256Ref("successor authority identity v1"),
                [oldEntry]);
            Assert.True(oldDraft.IsSuccess, oldDraft.ErrorMessage);
            var oldCommitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
                oldDraft.Value!,
                TestSha256Ref("successor authority fingerprint v1"));
            Assert.True(oldCommitment.IsSuccess, oldCommitment.ErrorMessage);
            var oldPrepared = store.Prepare(oldDraft.Value!, oldCommitment.Value!);
            Assert.True(oldPrepared.IsSuccess, oldPrepared.ErrorMessage);
            var oldCommitted = store.Commit(oldPrepared.Value!);
            Assert.True(oldCommitted.IsSuccess, oldCommitted.ErrorMessage);

            var newEntry = CreateCustodyEntry(
                "record:successor:second",
                "{\"request\":\"second\"}",
                "{\"html\":\"<main>second</main>\",\"css\":\"main{}\"}",
                "{\"html\":\"<main>first</main>\",\"css\":\"main{}\"}");
            var currentDraft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                "authority:successor-chain",
                oldCommitment.Value!.AuthorityFingerprintSha256,
                [oldEntry, newEntry]);
            Assert.True(currentDraft.IsSuccess, currentDraft.ErrorMessage);
            var currentCommitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
                currentDraft.Value!,
                TestSha256Ref("successor authority fingerprint v2"));
            Assert.True(currentCommitment.IsSuccess, currentCommitment.ErrorMessage);
            var currentPrepared = store.Prepare(
                currentDraft.Value!,
                currentCommitment.Value!);
            Assert.True(currentPrepared.IsSuccess, currentPrepared.ErrorMessage);

            var custodyRoot = Path.Combine(
                root,
                DesignConversionCandidatePayloadCustodyStore.DirectoryName);
            var committedRoot = Path.Combine(custodyRoot, "committed");
            var stagedRoot = Path.Combine(custodyRoot, "staged");
            Assert.Contains(
                Directory.GetFiles(committedRoot, "*.custody-committed.json"),
                path => Path.GetFileName(path).StartsWith(
                    oldCommitment.Value.TransactionId,
                    StringComparison.Ordinal));
            Assert.Contains(
                Directory.GetFiles(stagedRoot, "*.custody-stage.json"),
                path => Path.GetFileName(path).StartsWith(
                    currentCommitment.Value!.TransactionId,
                    StringComparison.Ordinal));
            Assert.False(store.ResolveCommitted(currentCommitment.Value!).IsSuccess);

            var reconciled = store.Reconcile(
                new Dictionary<string, DesignConversionCandidatePayloadCustodyCommitment>(
                    StringComparer.Ordinal)
                {
                    [currentCommitment.Value.TransactionId] = currentCommitment.Value
                });

            Assert.True(reconciled.IsSuccess, reconciled.ErrorMessage);
            var currentResolved = store.ResolveCommitted(currentCommitment.Value);
            Assert.True(currentResolved.IsSuccess, currentResolved.ErrorMessage);
            var currentEnvelope = store.ResolveSecretEnvelope(currentResolved.Value!);
            Assert.True(currentEnvelope.IsSuccess, currentEnvelope.ErrorMessage);
            var firstResolved = DesignConversionCandidatePayloadCustodyStore.FindExactEntry(
                currentEnvelope.Value!,
                currentResolved.Value!,
                oldEntry.BindingSha256);
            var secondResolved = DesignConversionCandidatePayloadCustodyStore.FindExactEntry(
                currentEnvelope.Value!,
                currentResolved.Value!,
                newEntry.BindingSha256);
            Assert.NotNull(firstResolved);
            Assert.NotNull(secondResolved);
            Assert.Equal(oldEntry.Secret.Prompt, firstResolved!.Secret.Prompt);
            Assert.Equal(newEntry.Secret.Prompt, secondResolved!.Secret.Prompt);
            Assert.False(store.ResolveCommitted(oldCommitment.Value).IsSuccess);
            Assert.DoesNotContain(
                Directory.GetFiles(committedRoot, "*.custody-committed.json"),
                path => Path.GetFileName(path).StartsWith(
                    oldCommitment.Value.TransactionId,
                    StringComparison.Ordinal));
            Assert.Empty(Directory.GetFiles(stagedRoot, "*.custody-stage.json"));

            var unrelatedEntry = CreateCustodyEntry(
                "record:successor:unrelated",
                "{\"request\":\"unrelated\"}",
                "{\"html\":\"<main>unrelated</main>\",\"css\":\"main{}\"}",
                "{\"html\":\"<main>old</main>\",\"css\":\"main{}\"}");
            var unrelatedDraft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                "authority:successor-unrelated",
                TestSha256Ref("successor unrelated authority"),
                [unrelatedEntry]);
            Assert.True(unrelatedDraft.IsSuccess, unrelatedDraft.ErrorMessage);
            var unrelatedCommitment =
                DesignConversionCandidatePayloadCustodyStore.BindAuthority(
                    unrelatedDraft.Value!,
                    TestSha256Ref("successor unrelated fingerprint"));
            Assert.True(unrelatedCommitment.IsSuccess, unrelatedCommitment.ErrorMessage);
            var unrelatedStore =
                new DesignConversionCandidatePayloadCustodyStore(unrelatedRoot);
            var unrelatedPrepared = unrelatedStore.Prepare(
                unrelatedDraft.Value!,
                unrelatedCommitment.Value!);
            Assert.True(unrelatedPrepared.IsSuccess, unrelatedPrepared.ErrorMessage);
            Assert.True(
                unrelatedStore.Commit(unrelatedPrepared.Value!).IsSuccess);

            var unrelatedNoAuthority = unrelatedStore.Reconcile(
                new Dictionary<string, DesignConversionCandidatePayloadCustodyCommitment>(
                    StringComparer.Ordinal));

            Assert.False(unrelatedNoAuthority.IsSuccess);
            Assert.True(
                unrelatedStore.ResolveCommitted(unrelatedCommitment.Value!).IsSuccess);
        }
        finally
        {
            unrelatedScope.Dispose();
            rootScope.Dispose();
        }
    }

    [Fact]
    public void CandidatePayloadCustodyStore_ReconcileRejectsOneSidedCommittedAndAuthorityStates()
    {
        using var rootScope = WorkspaceLocalTestRoot.Create(
            "design-conversion-candidate-payload-custody",
            "one-sided");
        var root = rootScope.Path;
        try
        {
            var entry = CreateCustodyEntry(
                "record:one-sided",
                "{\"request\":\"one-sided\"}",
                "{\"html\":\"<main>new</main>\",\"css\":\"main{}\"}",
                "{\"html\":\"<main>old</main>\",\"css\":\"main{}\"}");
            var draft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                "authority:one-sided",
                TestSha256Ref("authority one-sided identity"),
                [entry]);
            var commitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
                draft.Value!,
                TestSha256Ref("authority one-sided fingerprint"));
            var store = new DesignConversionCandidatePayloadCustodyStore(root);
            var prepared = store.Prepare(draft.Value!, commitment.Value!);
            var committed = store.Commit(prepared.Value!);
            Assert.True(committed.IsSuccess, committed.ErrorMessage);
            var noAuthority = store.Reconcile(
                new Dictionary<string, DesignConversionCandidatePayloadCustodyCommitment>(
                    StringComparer.Ordinal));
            Assert.False(noAuthority.IsSuccess);

            using var missingScope = WorkspaceLocalTestRoot.Create(
                "design-conversion-candidate-payload-custody",
                "one-sided-missing");
            var missingRoot = missingScope.Path;
            try
            {
                var authorityOnly = new DesignConversionCandidatePayloadCustodyStore(missingRoot)
                    .Reconcile(
                        new Dictionary<string, DesignConversionCandidatePayloadCustodyCommitment>(
                            StringComparer.Ordinal)
                        {
                            [commitment.Value!.TransactionId] = commitment.Value
                        });
                Assert.False(authorityOnly.IsSuccess);
            }
            finally
            {
                missingScope.Dispose();
            }
        }
        finally
        {
            rootScope.Dispose();
        }
    }

    [Fact]
    public void CandidatePayloadCustodyStore_FreshWindowsStoreAtomicallyCreatesProtectedAcl()
    {
        if (!OperatingSystem.IsWindows())
            return;
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-custody-windows-acl-" + Guid.NewGuid().ToString("N"));
        try
        {
            var entry = CreateCustodyEntry(
                "record:windows-acl",
                "{\"request\":\"acl\"}",
                "{\"html\":\"<main>new</main>\",\"css\":\"main{}\"}",
                "{\"html\":\"<main>old</main>\",\"css\":\"main{}\"}");
            var draft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                "authority:windows-acl",
                TestSha256Ref("windows acl authority"),
                [entry]);
            var commitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
                draft.Value!,
                TestSha256Ref("windows acl append"));
            var store = new DesignConversionCandidatePayloadCustodyStore(root);

            var prepared = store.Prepare(draft.Value!, commitment.Value!);

            Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
            var custodyRoot = Path.Combine(
                root,
                DesignConversionCandidatePayloadCustodyStore.DirectoryName);
            Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(custodyRoot));
            Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(
                Path.Combine(custodyRoot, "staged")));
            var staged = Assert.Single(Directory.GetFiles(
                Path.Combine(custodyRoot, "staged"),
                "*.custody-stage.json"));
            Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(staged));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void CandidatePayloadCustodyStore_WindowsSddlDeduplicatesLocalSystemIdentity()
    {
        if (!OperatingSystem.IsWindows())
            return;
        var localSystem = new SecurityIdentifier(
            WellKnownSidType.LocalSystemSid,
            null);

        var sddl = SecureLocalFileBoundary.BuildWindowsCustodySddl(
            localSystem,
            requireDirectory: true);
        var descriptor = new RawSecurityDescriptor(sddl);
        var dacl = Assert.IsType<RawAcl>(descriptor.DiscretionaryAcl);
        var aces = dacl.Cast<GenericAce>().Select(Assert.IsType<CommonAce>).ToArray();

        Assert.StartsWith("D:P", sddl, StringComparison.Ordinal);
        Assert.DoesNotContain("O:", sddl, StringComparison.Ordinal);
        Assert.DoesNotContain("G:", sddl, StringComparison.Ordinal);
        Assert.Equal(2, aces.Length);
        Assert.Equal(
            2,
            aces.Select(ace => ace.SecurityIdentifier.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());
        Assert.All(aces, ace =>
        {
            Assert.Equal(AceQualifier.AccessAllowed, ace.AceQualifier);
            Assert.Equal((int)FileSystemRights.FullControl, ace.AccessMask);
            Assert.Equal(
                AceFlags.ContainerInherit | AceFlags.ObjectInherit,
                ace.AceFlags);
        });
        Assert.Contains(aces, ace => string.Equals(
            ace.SecurityIdentifier.Value,
            localSystem.Value,
            StringComparison.OrdinalIgnoreCase));
        Assert.Contains(aces, ace => string.Equals(
            ace.SecurityIdentifier.Value,
            new SecurityIdentifier(
                WellKnownSidType.BuiltinAdministratorsSid,
                null).Value,
            StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CandidatePayloadCustodyStore_RejectsReparsePointAtCustodyRoot()
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
            return;
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-custody-reparse-" + Guid.NewGuid().ToString("N"));
        var outside = root + "-outside";
        try
        {
            Directory.CreateDirectory(root);
            Directory.CreateDirectory(outside);
            try
            {
                Directory.CreateSymbolicLink(
                    Path.Combine(
                        root,
                        DesignConversionCandidatePayloadCustodyStore.DirectoryName),
                    outside);
            }
            catch (Exception exception)
                when (exception is UnauthorizedAccessException or IOException)
            {
                return;
            }
            var entry = CreateCustodyEntry(
                "record:reparse",
                "{\"request\":\"reparse\"}",
                "{\"html\":\"<main>new</main>\",\"css\":\"main{}\"}",
                "{\"html\":\"<main>old</main>\",\"css\":\"main{}\"}");
            var draft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                "authority:reparse",
                TestSha256Ref("reparse authority"),
                [entry]);
            var commitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
                draft.Value!,
                TestSha256Ref("reparse append"));

            var prepared = new DesignConversionCandidatePayloadCustodyStore(root)
                .Prepare(draft.Value!, commitment.Value!);

            Assert.False(prepared.IsSuccess);
            Assert.Empty(Directory.EnumerateFileSystemEntries(outside));
        }
        finally
        {
            TryDelete(root);
            TryDelete(outside);
        }
    }

    [Theory]
    [InlineData("stage-symlink")]
    [InlineData("stage-directory")]
    [InlineData("committed-symlink")]
    [InlineData("committed-directory")]
    public void CandidatePayloadCustodyStore_LinuxRejectsLinkedOrDirectorySidecarLeaf(
        string attack)
    {
        if (!OperatingSystem.IsLinux())
            return;

        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-custody-linux-leaf-" + attack + "-" +
            Guid.NewGuid().ToString("N"));
        try
        {
            var entry = CreateCustodyEntry(
                "record:" + attack,
                "{\"request\":\"" + attack + "\"}",
                "{\"html\":\"<main>new</main>\",\"css\":\"main{}\"}",
                "{\"html\":\"<main>old</main>\",\"css\":\"main{}\"}");
            var draft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                "authority:" + attack,
                TestSha256Ref("linux leaf authority " + attack),
                [entry]);
            Assert.True(draft.IsSuccess, draft.ErrorMessage);
            var commitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
                draft.Value!,
                TestSha256Ref("linux leaf append " + attack));
            Assert.True(commitment.IsSuccess, commitment.ErrorMessage);
            var roots = CreateLinuxCustodySidecarDirectories(root);
            var leafName = commitment.Value!.TransactionId +
                           (attack.StartsWith("stage", StringComparison.Ordinal)
                               ? ".custody-stage.json"
                               : ".custody-committed.json");
            var attackedRoot = attack.StartsWith("stage", StringComparison.Ordinal)
                ? roots.StageRoot
                : roots.CommittedRoot;
            var attackPath = Path.Combine(attackedRoot, leafName);
            var foreignTarget = Path.Combine(root, "foreign-target.json");
            if (attack.EndsWith("symlink", StringComparison.Ordinal))
            {
                File.WriteAllText(foreignTarget, "foreign-custody-target");
                try
                {
                    File.CreateSymbolicLink(attackPath, foreignTarget);
                }
                catch (Exception exception)
                    when (exception is UnauthorizedAccessException or IOException)
                {
                    return;
                }
            }
            else
            {
                Directory.CreateDirectory(attackPath);
            }

            var prepared = new DesignConversionCandidatePayloadCustodyStore(root)
                .Prepare(draft.Value!, commitment.Value);

            Assert.False(prepared.IsSuccess);
            if (attack.EndsWith("symlink", StringComparison.Ordinal))
                Assert.Equal("foreign-custody-target", File.ReadAllText(foreignTarget));
            Assert.True(File.Exists(attackPath) || Directory.Exists(attackPath));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void CandidatePayloadCustodyStore_LinuxRejectsExtraHardlinkOnStagedEnvelope()
    {
        if (!OperatingSystem.IsLinux())
            return;

        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-custody-linux-hardlink-" + Guid.NewGuid().ToString("N"));
        try
        {
            var entry = CreateCustodyEntry(
                "record:linux-hardlink",
                "{\"request\":\"hardlink\"}",
                "{\"html\":\"<main>new</main>\",\"css\":\"main{}\"}",
                "{\"html\":\"<main>old</main>\",\"css\":\"main{}\"}");
            var draft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                "authority:linux-hardlink",
                TestSha256Ref("linux hardlink authority"),
                [entry]);
            Assert.True(draft.IsSuccess, draft.ErrorMessage);
            var commitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
                draft.Value!,
                TestSha256Ref("linux hardlink append"));
            Assert.True(commitment.IsSuccess, commitment.ErrorMessage);
            var store = new DesignConversionCandidatePayloadCustodyStore(root);
            var prepared = store.Prepare(draft.Value!, commitment.Value!);
            Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
            var roots = GetCustodySidecarDirectories(root);
            var stagePath = Path.Combine(
                roots.StageRoot,
                commitment.Value.TransactionId + ".custody-stage.json");
            var hardlinkPath = stagePath + ".extra-hardlink";
            if (!CreateLinuxHardLink(stagePath, hardlinkPath))
                return;

            var committed = store.Commit(prepared.Value!);

            Assert.False(committed.IsSuccess);
            Assert.True(File.Exists(stagePath));
            Assert.True(File.Exists(hardlinkPath));
            Assert.Empty(Directory.GetFiles(roots.CommittedRoot, "*.custody-committed.json"));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void CandidatePayloadCustodyStore_LinuxCommitCollisionPreservesForeignDestination()
    {
        if (!OperatingSystem.IsLinux())
            return;

        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-custody-linux-collision-" + Guid.NewGuid().ToString("N"));
        try
        {
            var entry = CreateCustodyEntry(
                "record:linux-collision",
                "{\"request\":\"collision\"}",
                "{\"html\":\"<main>new</main>\",\"css\":\"main{}\"}",
                "{\"html\":\"<main>old</main>\",\"css\":\"main{}\"}");
            var draft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                "authority:linux-collision",
                TestSha256Ref("linux collision authority"),
                [entry]);
            Assert.True(draft.IsSuccess, draft.ErrorMessage);
            var commitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
                draft.Value!,
                TestSha256Ref("linux collision append"));
            Assert.True(commitment.IsSuccess, commitment.ErrorMessage);
            var initial = new DesignConversionCandidatePayloadCustodyStore(root);
            var prepared = initial.Prepare(draft.Value!, commitment.Value!);
            Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
            var roots = GetCustodySidecarDirectories(root);
            var stagePath = Path.Combine(
                roots.StageRoot,
                commitment.Value.TransactionId + ".custody-stage.json");
            var committedPath = Path.Combine(
                roots.CommittedRoot,
                commitment.Value.TransactionId + ".custody-committed.json");
            var foreignBytes = Encoding.UTF8.GetBytes("{\"foreign\":\"destination\"}");
            var colliding = new DesignConversionCandidatePayloadCustodyStore(
                root,
                point =>
                {
                    if (string.Equals(point, "before_commit_rename", StringComparison.Ordinal))
                        WriteOwnerOnlyCustodyFile(committedPath, foreignBytes);
                });

            var committed = colliding.Commit(prepared.Value!);

            Assert.False(committed.IsSuccess);
            Assert.Equal(foreignBytes, File.ReadAllBytes(committedPath));
            Assert.True(File.Exists(stagePath));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void CandidatePayloadCustodyStore_LinuxRejectsGroupWorldAccessibleCustodyRoot()
    {
        if (!OperatingSystem.IsLinux())
            return;

        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-custody-linux-root-mode-" +
            Guid.NewGuid().ToString("N"));
        try
        {
            var entry = CreateCustodyEntry(
                "record:linux-root-mode",
                "{\"request\":\"root-mode\"}",
                "{\"html\":\"<main>new</main>\",\"css\":\"main{}\"}",
                "{\"html\":\"<main>old</main>\",\"css\":\"main{}\"}");
            var draft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                "authority:linux-root-mode",
                TestSha256Ref("linux root mode authority"),
                [entry]);
            Assert.True(draft.IsSuccess, draft.ErrorMessage);
            var commitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
                draft.Value!,
                TestSha256Ref("linux root mode append"));
            Assert.True(commitment.IsSuccess, commitment.ErrorMessage);
            var roots = CreateLinuxCustodySidecarDirectories(root);
            if (!TrySetAndObserveLinuxMode(
                    roots.CustodyRoot,
                    UnixFileMode.UserRead |
                    UnixFileMode.UserWrite |
                    UnixFileMode.UserExecute |
                    UnixFileMode.GroupRead |
                    UnixFileMode.GroupExecute |
                    UnixFileMode.OtherRead |
                    UnixFileMode.OtherExecute))
            {
                return;
            }

            var prepared = new DesignConversionCandidatePayloadCustodyStore(root)
                .Prepare(draft.Value!, commitment.Value!);

            Assert.False(prepared.IsSuccess);
            Assert.Empty(Directory.GetFiles(roots.StageRoot, "*.custody-stage.json"));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void CandidatePayloadCustodyStore_LinuxRejectsGroupWorldAccessibleSidecarLeaf()
    {
        if (!OperatingSystem.IsLinux())
            return;

        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-custody-linux-leaf-mode-" +
            Guid.NewGuid().ToString("N"));
        try
        {
            var entry = CreateCustodyEntry(
                "record:linux-leaf-mode",
                "{\"request\":\"leaf-mode\"}",
                "{\"html\":\"<main>new</main>\",\"css\":\"main{}\"}",
                "{\"html\":\"<main>old</main>\",\"css\":\"main{}\"}");
            var draft = DesignConversionCandidatePayloadCustodyStore.CreateDraft(
                "authority:linux-leaf-mode",
                TestSha256Ref("linux leaf mode authority"),
                [entry]);
            Assert.True(draft.IsSuccess, draft.ErrorMessage);
            var commitment = DesignConversionCandidatePayloadCustodyStore.BindAuthority(
                draft.Value!,
                TestSha256Ref("linux leaf mode append"));
            Assert.True(commitment.IsSuccess, commitment.ErrorMessage);
            var store = new DesignConversionCandidatePayloadCustodyStore(root);
            var prepared = store.Prepare(draft.Value!, commitment.Value!);
            Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
            var roots = GetCustodySidecarDirectories(root);
            var stagePath = Path.Combine(
                roots.StageRoot,
                commitment.Value!.TransactionId + ".custody-stage.json");
            if (!TrySetAndObserveLinuxMode(
                    stagePath,
                    UnixFileMode.UserRead |
                    UnixFileMode.UserWrite |
                    UnixFileMode.GroupRead |
                    UnixFileMode.OtherRead))
            {
                return;
            }

            var committed = store.Commit(prepared.Value!);

            Assert.False(committed.IsSuccess);
            Assert.True(File.Exists(stagePath));
            Assert.Empty(Directory.GetFiles(roots.CommittedRoot, "*.custody-committed.json"));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public async Task ImprovementLoopCompletion_PluralAuthoringHonorsCapAndPersistsEveryMeasuredPair(
        int maxTeacherCalls)
    {
        var fixture = CreatePair("coordinator-plural-" + maxTeacherCalls);
        using var testRoot = WorkspaceLocalTestRoot.Create(
            "design-conversion-dpo-teacher",
            "plural-" + maxTeacherCalls);
        var root = testRoot.Path;
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = new FakeTeacherInferenceClient("plural");
        using var teacherRuntime = CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        {
            var initial = WithScoreAndContinuation(fixture.Rejected, 0.2, 4);
            var first = WithScoreAndContinuation(fixture.Chosen, 0.4, 4);
            var second = WithScoreAndContinuation(fixture.Chosen, 0.6, 4);
            var third = WithScoreAndContinuation(fixture.Chosen, 0.8, 4);
            var initialRequest = CreateInternalRequest(initial.SubmittedRequest) with
            {
                Budget = initial.SubmittedRequest.Budget with
                {
                    MaxTeacherCalls = maxTeacherCalls,
                    MaxImprovementCandidates = 4
                }
            };
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(initial, first, second, third),
                new SingleCandidateStrategy(fixture.Chosen.RuntimeFlowResult.Candidate),
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);

            var completed = await coordinator.RunAsync(new(
                "teacher-coordinator-plural-loop-" + maxTeacherCalls,
                initialRequest,
                MaxAttempts: 4,
                Metadata: new Dictionary<string, string>()));

            Assert.True(
                completed.IsSuccess,
                $"code={completed.ErrorCode ?? "<null>"}; message={completed.ErrorMessage}");
            Assert.Equal(maxTeacherCalls, teacher.InvocationCount);
            Assert.Equal(
                Enumerable.Range(2, maxTeacherCalls),
                teacher.Prompts.Select(prompt =>
                {
                    using var document = JsonDocument.Parse(prompt);
                    return document.RootElement
                        .GetProperty("candidate_attempt_number")
                        .GetInt32();
                }));
            Assert.Equal(
                maxTeacherCalls.ToString(),
                completed.Value!.PublicMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .MaxTeacherCallsObservedCallCountMetadataKey]);
            var records = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            var candidates = Assert.Single(records.Value!).DpoCandidates;
            Assert.Equal(maxTeacherCalls * 2, candidates.Count);
            Assert.Equal(
                maxTeacherCalls * 2,
                candidates.Select(candidate => candidate.TrainingRecord.RecordId)
                    .Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(
                Enumerable.Range(0, maxTeacherCalls)
                    .SelectMany(_ => new[]
                    {
                        DesignConversionDpoUnits.CriticUnit,
                        DesignConversionDpoUnits.ImproverUnit
                    }),
                candidates.Select(candidate => candidate.UnitId));
            Assert.All(candidates, candidate => Assert.Equal(
                bool.FalseString,
                candidate.TrainingRecord.Provenance.Metadata["trained_model_authority"]));
        }
    }

    [Fact]
    public async Task ImprovementLoopCompletion_SequentialApprovalKeepsDualTargetSiblingIndependent()
    {
        var fixture = CreateProviderAllowedPair("coordinator-sequential-approval");
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-dpo-teacher-sequential-approval-" +
            Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = new FakeTeacherInferenceClient("plural");
        using var teacherRuntime = CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        try
        {
            const string candidate1Html =
                "<main data-teacher-authoring=\"candidate-1\">Candidate 1</main>";
            const string candidate1Css =
                "[data-teacher-authoring=\"candidate-1\"]{display:grid;gap:21px;}";
            var initial = WithScoreAndContinuation(fixture.Rejected, 0.2, 2);
            var first = WithScoreAndContinuation(fixture.Chosen, 0.4, 2);
            var initialRequest = CreateInternalRequest(initial.SubmittedRequest) with
            {
                Budget = initial.SubmittedRequest.Budget with
                {
                    MaxTeacherCalls = 1,
                    MaxImprovementCandidates = 2
                }
            };
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(initial, first),
                new SingleCandidateStrategy(
                    fixture.Chosen.RuntimeFlowResult.Candidate),
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);

            var completed = await coordinator.RunAsync(new(
                "teacher-coordinator-sequential-approval-loop",
                initialRequest,
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>()));

            Assert.True(
                completed.IsSuccess,
                $"code={completed.ErrorCode ?? "<null>"}; message={completed.ErrorMessage}");
            Assert.Equal(1, teacher.InvocationCount);
            var records = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            Assert.True(records.IsSuccess, records.ErrorMessage);
            var initialRecord = Assert.Single(records.Value!);
            var initialCandidates = initialRecord.DpoCandidates.ToArray();
            Assert.Equal(2, initialCandidates.Length);
            Assert.Equal(
                new[]
                {
                    DesignConversionDpoUnits.CriticUnit,
                    DesignConversionDpoUnits.ImproverUnit
                },
                initialCandidates.Select(candidate => candidate.UnitId));
            var firstCandidate = initialCandidates[0];
            var secondCandidate = initialCandidates[1];
            var initialTransaction =
                initialRecord.Metadata[
                    DesignConversionCandidatePayloadCustodyStore
                        .TransactionMetadataKey];
            var initialEntries =
                ResolveCandidatePayloadCustodyEntries(root, initialRecord);
            Assert.Equal(2, initialEntries.Count);
            AssertCandidatePayload(
                initialEntries,
                firstCandidate.TrainingRecord.RecordId,
                candidate1Html,
                candidate1Css);
            AssertCandidatePayload(
                initialEntries,
                secondCandidate.TrainingRecord.RecordId,
                candidate1Html,
                candidate1Css);

            var api = new DesignConversionDpoUsefulnessApi(artifactDb);
            var firstApproval = await api.ApproveRuntimeCreatedDpoCandidateAsync(
                initialRecord.RecordId,
                firstCandidate.CandidateId,
                "reviewer:sequential-custody",
                "first sequential approval must rebind successor custody",
                DateTimeOffset.UnixEpoch.AddMinutes(1));
            Assert.True(
                firstApproval.IsSuccess,
                $"candidate={firstCandidate.CandidateId}; code={firstApproval.ErrorCode ?? "<null>"}; message={firstApproval.ErrorMessage}");
            var afterFirstApprovalRecords = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            Assert.True(
                afterFirstApprovalRecords.IsSuccess,
                afterFirstApprovalRecords.ErrorMessage);
            var afterFirstApproval = Assert.Single(afterFirstApprovalRecords.Value!);
            Assert.NotEqual(
                initialTransaction,
                afterFirstApproval.Metadata[
                    DesignConversionCandidatePayloadCustodyStore
                        .TransactionMetadataKey]);
            var afterFirstById = afterFirstApproval.DpoCandidates.ToDictionary(
                candidate => candidate.CandidateId,
                StringComparer.Ordinal);
            Assert.Equal(
                "approved",
                ReadMetadata(
                    afterFirstById[firstCandidate.CandidateId]
                        .TrainingRecord.Provenance.Metadata,
                    "approval_status"));
            Assert.Equal(
                "pending",
                ReadMetadata(
                    afterFirstById[secondCandidate.CandidateId]
                        .TrainingRecord.Provenance.Metadata,
                    "approval_status"));
            Assert.Equal(
                bool.TrueString,
                ReadMetadata(
                    afterFirstById[firstCandidate.CandidateId]
                        .TrainingRecord.Provenance.Metadata,
                    "approved_for_training"));
            Assert.Equal(
                bool.FalseString,
                ReadMetadata(
                    afterFirstById[secondCandidate.CandidateId]
                        .TrainingRecord.Provenance.Metadata,
                    "approved_for_training"));
            var afterFirstEntries =
                ResolveCandidatePayloadCustodyEntries(root, afterFirstApproval);
            Assert.Equal(2, afterFirstEntries.Count);
            AssertCandidatePayload(
                afterFirstEntries,
                firstCandidate.TrainingRecord.RecordId,
                candidate1Html,
                candidate1Css);
            AssertCandidatePayload(
                afterFirstEntries,
                secondCandidate.TrainingRecord.RecordId,
                candidate1Html,
                candidate1Css);

            var jsonl = await File.ReadAllTextAsync(
                Path.Combine(root, "conversion-results.jsonl"));
            Assert.DoesNotContain(candidate1Html, jsonl, StringComparison.Ordinal);
            Assert.DoesNotContain(candidate1Css, jsonl, StringComparison.Ordinal);
            Assert.DoesNotContain("payloadCustodySecret", jsonl, StringComparison.Ordinal);
            Assert.DoesNotContain("payloadCustody", jsonl, StringComparison.Ordinal);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task ImprovementLoopCompletion_StructuralPluralCollisionPersistsZeroAndNoProofMetadata()
    {
        var fixture = CreatePair("coordinator-plural-structural-collision");
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = new FakeTeacherInferenceClient(
            "plural-duplicate-provenance");
        using var teacherRuntime = CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        try
        {
            var initial = WithScoreAndContinuation(fixture.Rejected, 0.2, 3);
            var first = WithScoreAndContinuation(fixture.Chosen, 0.4, 3);
            var second = WithScoreAndContinuation(fixture.Chosen, 0.6, 3);
            var initialRequest = CreateInternalRequest(initial.SubmittedRequest) with
            {
                Budget = initial.SubmittedRequest.Budget with
                {
                    MaxTeacherCalls = 2,
                    MaxImprovementCandidates = 3
                }
            };
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(initial, first, second),
                new SingleCandidateStrategy(fixture.Chosen.RuntimeFlowResult.Candidate),
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);

            var completed = await coordinator.RunAsync(new(
                "teacher-coordinator-plural-structural-collision-loop",
                initialRequest,
                MaxAttempts: 3,
                Metadata: new Dictionary<string, string>()));

            Assert.True(completed.IsSuccess, completed.ErrorMessage);
            Assert.Equal(2, teacher.InvocationCount);
            var record = Assert.Single((await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery())).Value!);
            Assert.Empty(record.DpoCandidates);
            Assert.Equal(bool.FalseString, record.Metadata["runtime_created_dpo_candidate"]);
            Assert.Equal(bool.FalseString, record.Metadata["chosen_rejected_pair_proven"]);
            Assert.Equal(bool.TrueString, record.Metadata["chosen_rejected_pair_not_proven"]);
            Assert.Equal(bool.FalseString, record.Metadata["high_quality_negative_pair_proven"]);
            Assert.Equal(
                bool.FalseString,
                record.Metadata["runtime_rejected_independently_evaluated"]);
            Assert.Contains(
                "chosen_rejected_pair_or_negative_quality_not_proven",
                completed.Value!.Warnings);
            Assert.DoesNotContain("chosen_rejected_pair_proven", completed.Value.Warnings);
            Assert.DoesNotContain("high_quality_negative_pair_proven", completed.Value.Warnings);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task ImprovementLoopCompletion_IssuedFailureConsumesPermitBeforeLaterSuccessfulAuthoring()
    {
        var fixture = CreatePair("coordinator-issued-failure-retry");
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = new FakeTeacherInferenceClient("first-operation-failure");
        using var teacherRuntime = CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        try
        {
            var initial = WithScoreAndContinuation(fixture.Rejected, 0.2, 3);
            var fallback = WithScoreAndContinuation(fixture.Chosen, 0.4, 3);
            var authored = WithScoreAndContinuation(fixture.Chosen, 0.6, 3);
            var initialRequest = CreateInternalRequest(initial.SubmittedRequest) with
            {
                Budget = initial.SubmittedRequest.Budget with
                {
                    MaxTeacherCalls = 2,
                    MaxImprovementCandidates = 3
                }
            };
            var deterministic = new CountingCandidateStrategy(
                fixture.Chosen.RuntimeFlowResult.Candidate);
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(initial, fallback, authored),
                deterministic,
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);

            var completed = await coordinator.RunAsync(new(
                "teacher-coordinator-issued-failure-retry-loop",
                initialRequest,
                MaxAttempts: 3,
                Metadata: new Dictionary<string, string>()));

            Assert.True(completed.IsSuccess, completed.ErrorMessage);
            Assert.Equal(2, teacher.InvocationCount);
            Assert.Equal(1, deterministic.InvocationCount);
            Assert.Equal(
                "2",
                completed.Value!.PublicMetadata[
                    DesignConversionImprovementLoopCoordinator
                        .MaxTeacherCallsObservedCallCountMetadataKey]);
            var records = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            var candidates = Assert.Single(records.Value!).DpoCandidates;
            Assert.Equal(2, candidates.Count);
            Assert.All(candidates, candidate => Assert.Equal(
                bool.FalseString,
                candidate.TrainingRecord.Provenance.Metadata["trained_model_authority"]));
            Assert.Contains(
                DesignConversionImprovementLoopCoordinator.TeacherAuthoringFallbackMarker,
                completed.Value.Warnings);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Theory]
    [InlineData(
        "malformed",
        DesignConversionDpoTeacherAttestationRuntime.AuthoringOutputInvalidErrorCode)]
    [InlineData(
        "operation-failure",
        DesignConversionDpoTeacherAttestationRuntime.RuntimeUnavailableErrorCode)]
    [InlineData(
        "throw-ordinary",
        DesignConversionDpoTeacherAttestationRuntime.RuntimeUnavailableErrorCode)]
    public async Task ImprovementLoopCompletion_IssuedInvalidAuthoringFallsBackOnceWithoutTeacherRecord(
        string scenario,
        string expectedErrorCode)
    {
        var fixture = CreatePair("authoring-malformed-fallback");
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-authoring-fallback-" + Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = new FakeTeacherInferenceClient(scenario);
        using var teacherRuntime = CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        var deterministic = new CountingCandidateStrategy(
            fixture.Chosen.RuntimeFlowResult.Candidate);
        try
        {
            var initial = WithScoreAndContinuation(fixture.Rejected, 0.2, 2);
            var fallback = WithScoreAndContinuation(fixture.Chosen, 0.4, 2);
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(
                    initial,
                    fallback),
                deterministic,
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);

            var completed = await coordinator.RunAsync(new(
                "teacher-authoring-fallback-loop",
                CreateInternalRequest(fixture.Rejected.SubmittedRequest) with
                {
                    Budget = fixture.Rejected.SubmittedRequest.Budget with
                    {
                        MaxTeacherCalls = 1,
                        MaxImprovementCandidates = 2
                    }
                },
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>()));

            Assert.True(
                completed.IsSuccess,
                $"{completed.ErrorCode}:{completed.ErrorMessage}");
            Assert.False(completed.Value!.CandidateStrategyFailed);
            Assert.Equal(1, teacher.InvocationCount);
            Assert.Equal(1, deterministic.InvocationCount);
            Assert.Contains(
                DesignConversionImprovementLoopCoordinator
                    .TeacherAuthoringFallbackMarker,
                completed.Value.Warnings);
            Assert.Contains(
                DesignConversionImprovementLoopCoordinator
                    .TeacherAuthoringErrorWarningPrefix +
                expectedErrorCode,
                completed.Value.Warnings);
            Assert.Contains(
                DesignConversionImprovementLoopCoordinator
                    .ObservedTeacherCallEvidenceRefPrefix + "1",
                completed.Value.EvidenceRefs);
            var records = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            var stored = Assert.Single(records.Value!);
            Assert.Empty(stored.DpoCandidates);
            var projection = JsonSerializer.Serialize(new { completed, stored });
            if (!string.IsNullOrEmpty(teacher.LastOutput))
            {
                Assert.DoesNotContain(
                    teacher.LastOutput,
                    projection,
                    StringComparison.Ordinal);
            }
            Assert.DoesNotContain(teacher.LeakMarker, projection, StringComparison.Ordinal);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Theory]
    [InlineData("runtime-absent")]
    [InlineData("max-teacher-calls-zero")]
    public async Task TeacherAuthoringStrategy_NoRuntimeOrCapSignalsFallbackWithoutInvokingInner(
        string scenario)
    {
        var fixture = CreatePair("authoring-signal-only-" + scenario);
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-authoring-signal-only-" + Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = string.Equals(
            scenario,
            "max-teacher-calls-zero",
            StringComparison.Ordinal)
            ? new FakeTeacherInferenceClient()
            : null;
        using var teacherRuntime = teacher is null
            ? null
            : CreateTeacherRuntime(teacher);
        using var association = teacherRuntime is null
            ? null
            : DesignConversionDpoTeacherAttestationAssociation.Associate(
                provider,
                teacherRuntime);
        var inner = new CountingCandidateStrategy(
            fixture.Chosen.RuntimeFlowResult.Candidate);
        var strategy = new DesignConversionDpoTeacherAuthoringCandidateStrategy(
            inner,
            artifactDb);
        var incumbent = WithScoreAndContinuation(fixture.Rejected, 0.2, 2);
        var request = new DesignConversionCandidateUpdateRequest(
            LoopId: "teacher-authoring-signal-only-loop-" + scenario,
            CompletedAttemptNumber: 1,
            CurrentRequest: CreateInternalRequest(incumbent.SubmittedRequest),
            CurrentEvaluation: incumbent,
            TierPolicy: incumbent.VisualQaBoundaryTrace.TierPolicy,
            AttemptHistory: []);
        var reservation = DesignConversionDpoTeacherCallReservation.Create(
            string.Equals(
                scenario,
                "max-teacher-calls-zero",
                StringComparison.Ordinal) ? 0 : 1,
            observedCallCount: 0);
        Assert.True(reservation.IsSuccess, reservation.ErrorMessage);

        try
        {
            var authored = await strategy.AuthorCandidateAsync(
                request,
                reservation.Value!);

            Assert.False(authored.IsSuccess);
            Assert.Null(authored.Value);
            Assert.Equal(
                DesignConversionDpoTeacherAuthoringCandidateStrategy
                    .FallbackRequiredErrorCode,
                authored.ErrorCode);
            Assert.Equal(0, inner.InvocationCount);
            Assert.Equal(0, teacher?.InvocationCount ?? 0);
            Assert.False(reservation.Value!.CallIssued);
            Assert.DoesNotContain(
                fixture.Rejected.SubmittedRequest.Html,
                authored.ErrorMessage,
                StringComparison.Ordinal);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Theory]
    [InlineData("runtime-absent")]
    [InlineData("max-teacher-calls-zero")]
    [InlineData("prompt-oversized")]
    public async Task ImprovementLoopCompletion_NoRuntimeOrZeroCapUsesDeterministicStrategyWithoutTeacherCall(
        string scenario)
    {
        var fixture = CreatePair("authoring-no-call-" + scenario);
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-authoring-no-call-" + Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = !string.Equals(
            scenario,
            "runtime-absent",
            StringComparison.Ordinal)
            ? new FakeTeacherInferenceClient()
            : null;
        using var teacherRuntime = teacher is null
            ? null
            : CreateTeacherRuntime(teacher);
        using var association = teacherRuntime is null
            ? null
            : DesignConversionDpoTeacherAttestationAssociation.Associate(
                provider,
                teacherRuntime);
        var deterministic = new CountingCandidateStrategy(
            fixture.Chosen.RuntimeFlowResult.Candidate);
        try
        {
            var initial = WithScoreAndContinuation(fixture.Rejected, 0.2, 2);
            var fallback = WithScoreAndContinuation(fixture.Chosen, 0.4, 2);
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(
                    initial,
                    fallback),
                deterministic,
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);

            var initialRequest = CreateInternalRequest(
                fixture.Rejected.SubmittedRequest) with
            {
                Html = string.Equals(
                    scenario,
                    "prompt-oversized",
                    StringComparison.Ordinal)
                    ? new string(
                        'p',
                        DesignConversionDpoTeacherAttestationRuntime
                            .MaximumAuthoringPromptUtf8Bytes)
                    : fixture.Rejected.SubmittedRequest.Html,
                Budget = fixture.Rejected.SubmittedRequest.Budget with
                {
                    MaxTeacherCalls = string.Equals(
                        scenario,
                        "max-teacher-calls-zero",
                        StringComparison.Ordinal) ? 0 : 1,
                    MaxImprovementCandidates = 2
                }
            };
            var completed = await coordinator.RunAsync(new(
                "teacher-authoring-no-call-loop-" + scenario,
                initialRequest,
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>()));

            Assert.True(
                completed.IsSuccess,
                $"{completed.ErrorCode}:{completed.ErrorMessage}");
            Assert.False(completed.Value!.CandidateStrategyFailed);
            Assert.Equal(0, teacher?.InvocationCount ?? 0);
            Assert.Equal(1, deterministic.InvocationCount);
            var records = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            Assert.Empty(Assert.Single(records.Value!).DpoCandidates);
            Assert.DoesNotContain(
                DesignConversionDpoTeacherAuthoringCandidateStrategy
                    .CandidateAuthoringMarker,
                completed.Value.EvidenceRefs);
            var attemptedTeacherAuthoring = string.Equals(
                    scenario,
                    "prompt-oversized",
                    StringComparison.Ordinal);
            if (attemptedTeacherAuthoring)
            {
                Assert.Contains(
                    DesignConversionImprovementLoopCoordinator
                        .TeacherAuthoringFallbackMarker,
                    completed.Value.Warnings);
            }
            else
            {
                Assert.DoesNotContain(
                    DesignConversionImprovementLoopCoordinator
                        .TeacherAuthoringFallbackMarker,
                    completed.Value.Warnings);
                Assert.DoesNotContain(
                    completed.Value.Warnings,
                    warning => warning.StartsWith(
                        DesignConversionImprovementLoopCoordinator
                            .TeacherAuthoringErrorWarningPrefix,
                        StringComparison.Ordinal));
            }
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Theory]
    [InlineData("incumbent-preferred", 2)]
    [InlineData("success", 2)]
    public async Task ImprovementLoopCompletion_CandidateOutputCannotChangeIndependentDirection(
        string scenario,
        int expectedIndependentPreferenceRecords)
    {
        var fixture = CreatePair("authoring-incumbent-direction-" + scenario);
        var initial = WithLoopContinuation(fixture.Chosen);
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-authoring-direction-" + Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = new FakeTeacherInferenceClient(scenario);
        using var teacherRuntime = CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        var deterministic = new CountingCandidateStrategy(
            fixture.Rejected.RuntimeFlowResult.Candidate);
        try
        {
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(initial, fixture.Rejected),
                deterministic,
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);

            var completed = await coordinator.RunAsync(new(
                "teacher-authoring-incumbent-direction-loop-" + scenario,
                CreateInternalRequest(initial.SubmittedRequest) with
                {
                    Budget = initial.SubmittedRequest.Budget with
                    {
                        MaxTeacherCalls = 1,
                        MaxImprovementCandidates = 2
                    }
                },
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>()));

            Assert.True(completed.IsSuccess, completed.ErrorMessage);
            Assert.Equal(1, teacher.InvocationCount);
            Assert.Equal(0, deterministic.InvocationCount);
            var records = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            Assert.Equal(
                expectedIndependentPreferenceRecords,
                Assert.Single(records.Value!).DpoCandidates.Count);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task ImprovementLoopCompletion_CallerCancellationAtTeacherStagePropagatesWithoutPersistence()
    {
        var fixture = CreatePair("coordinator-cancel");
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-dpo-teacher-coordinator-cancel-" +
            Guid.NewGuid().ToString("N"));
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        var teacher = new FakeTeacherInferenceClient("cancel");
        using var teacherRuntime =
            CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);
        using var cancellation = new CancellationTokenSource();
        try
        {
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new SequenceEvaluator(fixture.Rejected, fixture.Chosen),
                new SingleCandidateStrategy(
                    fixture.Chosen.RuntimeFlowResult.Candidate),
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);
            var run = coordinator.RunAsync(
                new DesignConversionImprovementLoopRequest(
                    "teacher-coordinator-cancel",
                    CreateInternalRequest(fixture.Rejected.SubmittedRequest),
                    MaxAttempts: 2,
                    Metadata: new Dictionary<string, string>()),
                cancellation.Token);
            await teacher.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await run);
            Assert.Equal(1, teacher.InvocationCount);
            Assert.True(teacher.CapturedCancellationToken.IsCancellationRequested);
            var records = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery());
            Assert.True(records.IsSuccess, records.ErrorMessage);
            Assert.Empty(records.Value!);
            Assert.DoesNotContain(
                teacher.LeakMarker,
                JsonSerializer.Serialize(records.Value),
                StringComparison.Ordinal);
        }
        finally
        {
            TryDelete(root);
        }
    }

    public static IEnumerable<object[]> TeacherFailureCases()
    {
        yield return
        [
            "absent",
            DesignConversionDpoTeacherAttestationRuntime.RuntimeUnavailableErrorCode,
            0
        ];
        yield return
        [
            "operation-failure",
            DesignConversionDpoTeacherAttestationRuntime.RuntimeUnavailableErrorCode,
            1
        ];
        yield return
        [
            "malformed",
            DesignConversionDpoTeacherAttestationRuntime.AuthoringOutputInvalidErrorCode,
            1
        ];
        yield return
        [
            "preference-rejected",
            DesignConversionDpoTeacherAttestationRuntime.AuthoringOutputInvalidErrorCode,
            1
        ];

        var invalidProvenanceScenarios = new[]
        {
            "invalid-provenance",
            "provider-terms-checked",
            "provider-terms-allow-training",
            "model-id-blank",
            "model-id-untrimmed",
            "model-id-control",
            "model-id-overlong",
            "model-id-uppercase",
            "model-id-untagged",
            "model-id-wrong-canonical",
            "model-id-wrong-prefix",
            "model-version-blank",
            "model-version-untrimmed",
            "model-version-control",
            "model-version-overlong",
            "model-version-short",
            "model-version-uppercase",
            "model-version-nonhex",
            "model-version-wrong-canonical",
            "model-artifact-kind-wrong",
            "model-artifact-digest-missing",
            "model-artifact-digest-wrong-canonical",
            "request-id-blank",
            "request-id-untrimmed",
            "request-id-control",
            "request-id-overlong"
        };
        foreach (var scenario in invalidProvenanceScenarios)
        {
            yield return
            [
                scenario,
                DesignConversionDpoTeacherAttestationRuntime.ProvenanceInvalidErrorCode,
                1
            ];
        }
    }

    private static DesignConversionThinClientJobRuntime CreateThinRuntime(
        PairFixture fixture,
        IDesignConversionLocalArtifactDb artifactDb,
        string ledgerRoot)
        => new(
            new SequenceEvaluator(fixture.Chosen),
            artifactDb: artifactDb,
            visualCostLedger: new JsonlDesignConversionVisualCostLedger(ledgerRoot),
             rejectedEvaluationSource: new RejectedEvaluationSource(
                 fixture.Chosen,
                 fixture.Rejected));

    private static void AssertTeacherRawResponseNotPersisted(
        FakeTeacherInferenceClient? teacher,
        IReadOnlyList<string> warnings,
        object completed,
        DesignConversionLocalArtifactDbRecord stored,
        string root)
    {
        var projection = JsonSerializer.Serialize(new { completed, stored });
        Assert.DoesNotContain(
            teacher?.LeakMarker ?? "RAW_TEACHER_SECRET_absent",
            projection,
            StringComparison.Ordinal);
        if (!string.IsNullOrEmpty(teacher?.LastOutput))
        {
            Assert.All(
                warnings,
                warning => Assert.DoesNotContain(
                    teacher.LastOutput,
                    warning,
                    StringComparison.Ordinal));
            Assert.All(
                stored.Metadata.Values,
                value => Assert.DoesNotContain(
                    teacher.LastOutput,
                    value,
                    StringComparison.Ordinal));
            Assert.DoesNotContain(
                teacher.LastOutput,
                projection,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                JsonEncodedText.Encode(teacher.LastOutput).ToString(),
                projection,
                StringComparison.Ordinal);
        }
        if (teacher?.LastProvenance is { } provenance)
        {
            foreach (var identity in new[]
                     {
                         provenance.ModelId,
                         provenance.ModelVersion,
                         provenance.RequestId
                     }.Where(value => !string.IsNullOrEmpty(value))
                     .Distinct(StringComparer.Ordinal))
            {
                Assert.All(
                    warnings,
                    warning => Assert.DoesNotContain(
                        identity,
                        warning,
                        StringComparison.Ordinal));
                Assert.All(
                    stored.Metadata.Values,
                    value => Assert.DoesNotContain(
                        identity,
                        value,
                        StringComparison.Ordinal));
                Assert.DoesNotContain(
                    JsonEncodedText.Encode(identity).ToString(),
                    projection,
                    StringComparison.Ordinal);
            }
        }

        var persistedPath = Path.Combine(root, "conversion-results.jsonl");
        if (!File.Exists(persistedPath))
            return;
        var persisted = File.ReadAllText(persistedPath);
        Assert.DoesNotContain(
            teacher?.LeakMarker ?? "RAW_TEACHER_SECRET_absent",
            persisted,
            StringComparison.Ordinal);
        if (!string.IsNullOrEmpty(teacher?.LastOutput))
        {
            Assert.DoesNotContain(
                teacher.LastOutput,
                persisted,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                JsonEncodedText.Encode(teacher.LastOutput).ToString(),
                persisted,
                StringComparison.Ordinal);
        }
        if (teacher?.LastProvenance is { } persistedProvenance)
        {
            foreach (var identity in new[]
                     {
                         persistedProvenance.ModelId,
                         persistedProvenance.ModelVersion,
                         persistedProvenance.RequestId
                     }.Where(value => !string.IsNullOrEmpty(value))
                     .Distinct(StringComparer.Ordinal))
            {
                Assert.DoesNotContain(
                    JsonEncodedText.Encode(identity).ToString(),
                    persisted,
                    StringComparison.Ordinal);
            }
        }
    }

    private static async Task<OperationResult<DesignConversionThinClientStatusResult>>
        SubmitAndRunAsync(
            DesignConversionThinClientJobRuntime runtime,
            SubmittedDesignConversionRequest request)
    {
        var submitted = runtime.Submit(request);
        Assert.True(submitted.IsSuccess, submitted.ErrorMessage);
        return await runtime.RunAsync(submitted.Value!.JobId);
    }

    private static IEnumerable<string> TeacherCallMetadataKeys()
    {
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractSuppliedMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractValueMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsContractBoundaryMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsRuntimeEnforcementMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCallCapEnforcedMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsObservedCallCountMetadataKey;
        yield return DesignConversionImprovementLoopCoordinator.MaxTeacherCallsCapReachedMetadataKey;
    }

    [Fact]
    public async Task TeacherCallReservation_IsOneShotAndFailsClosedForInvalidOrExhaustedState()
    {
        var fixture = CreatePair("reservation-one-shot");
        using var teacher = new FakeTeacherInferenceClient();
        using var runtime = CreateTeacherRuntime(teacher);

        var invalidNegative = DesignConversionDpoTeacherCallReservation.Create(-1, 0);
        var invalidOverCap = DesignConversionDpoTeacherCallReservation.Create(1, 2);
        Assert.False(invalidNegative.IsSuccess);
        Assert.False(invalidOverCap.IsSuccess);
        Assert.Equal(
            DesignConversionDpoTeacherCallReservation.InvalidStateErrorCode,
            invalidNegative.ErrorCode);
        Assert.Equal(
            DesignConversionDpoTeacherCallReservation.InvalidStateErrorCode,
            invalidOverCap.ErrorCode);

        var exhausted = DesignConversionDpoTeacherCallReservation.Create(0, 0);
        Assert.True(exhausted.IsSuccess, exhausted.ErrorMessage);
        var exhaustedAttestation = await runtime.AttestAsync(
            fixture.Chosen,
            fixture.Rejected,
            exhausted.Value!);
        Assert.False(exhaustedAttestation.IsSuccess);
        Assert.Equal(
            DesignConversionDpoTeacherCallReservation.ExhaustedErrorCode,
            exhaustedAttestation.ErrorCode);
        Assert.False(exhausted.Value!.CallIssued);
        Assert.Equal(0, teacher.InvocationCount);

        var oneShot = DesignConversionDpoTeacherCallReservation.Create(1, 0);
        Assert.True(oneShot.IsSuccess, oneShot.ErrorMessage);
        var first = await runtime.AttestAsync(
            fixture.Chosen,
            fixture.Rejected,
            oneShot.Value!);
        var reused = await runtime.AttestAsync(
            fixture.Chosen,
            fixture.Rejected,
            oneShot.Value!);
        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.False(reused.IsSuccess);
        Assert.Equal(
            DesignConversionDpoTeacherCallReservation.InvalidStateErrorCode,
            reused.ErrorCode);
        Assert.True(oneShot.Value!.CallIssued);
        Assert.Equal(1, oneShot.Value.ActualObservedCallCount);
        Assert.Equal(1, teacher.InvocationCount);
    }

    [Fact]
    public async Task TeacherCallReservation_CancellationBeforeIssueDoesNotSpendCall()
    {
        var fixture = CreatePair("reservation-cancel-before");
        using var teacher = new FakeTeacherInferenceClient();
        using var runtime = CreateTeacherRuntime(teacher);
        var reservation = DesignConversionDpoTeacherCallReservation.Create(1, 0).Value!;
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => runtime.AttestAsync(
            fixture.Chosen,
            fixture.Rejected,
            reservation,
            cancellation.Token));

        Assert.False(reservation.CallIssued);
        Assert.Equal(0, reservation.ActualObservedCallCount);
        Assert.Equal(0, teacher.InvocationCount);
    }

    [Theory]
    [InlineData("operation-failure")]
    [InlineData("malformed")]
    public async Task TeacherCallReservation_InferenceEntryIsCountedWhenAttestationFails(
        string scenario)
    {
        var fixture = CreatePair("reservation-failure-" + scenario);
        using var teacher = new FakeTeacherInferenceClient(scenario);
        using var runtime = CreateTeacherRuntime(teacher);
        var reservation = DesignConversionDpoTeacherCallReservation.Create(1, 0).Value!;

        var attestation = await runtime.AttestAsync(
            fixture.Chosen,
            fixture.Rejected,
            reservation);

        Assert.False(attestation.IsSuccess);
        Assert.True(reservation.CallIssued);
        Assert.Equal(1, reservation.ActualObservedCallCount);
        Assert.Equal(1, teacher.InvocationCount);
    }

    [Fact]
    public async Task TeacherCallReservation_CancellationAfterInferenceEntryIsCounted()
    {
        var fixture = CreatePair("reservation-cancel-after");
        using var teacher = new FakeTeacherInferenceClient("cancel");
        using var runtime = CreateTeacherRuntime(teacher);
        var reservation = DesignConversionDpoTeacherCallReservation.Create(1, 0).Value!;
        using var cancellation = new CancellationTokenSource();

        var attestation = runtime.AttestAsync(
            fixture.Chosen,
            fixture.Rejected,
            reservation,
            cancellation.Token);
        await teacher.Entered.Task.WaitAsync(TimeSpan.FromSeconds(1));
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => attestation);

        Assert.True(reservation.CallIssued);
        Assert.Equal(1, reservation.ActualObservedCallCount);
        Assert.Equal(1, teacher.InvocationCount);
    }

    private static PairFixture CreatePair(string scope)
    {
        var chosen = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: true,
            jobId: scope + "-chosen-job",
            requestId: scope + "-chosen-request",
            outputId: scope + "-chosen-output",
            candidateId: scope + "-chosen-candidate",
            verdictId: scope + "-chosen-verdict",
            score: 0.86,
            passed: true,
            htmlSuffix: scope + "-chosen");
        var rejected = DesignConversionDpoGeneratorTests.CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            jobId: scope + "-rejected-job",
            requestId: scope + "-rejected-request",
            outputId: scope + "-rejected-output",
            candidateId: scope + "-rejected-candidate",
            verdictId: scope + "-rejected-verdict",
            score: 0.43,
            passed: false,
            htmlSuffix: scope + "-rejected");
        var sourceManifestRef = "manifest://runtime-source/" + scope;
        chosen = WithSourceManifestRef(chosen, sourceManifestRef);
        rejected = WithSourceManifestRef(rejected, sourceManifestRef);
        chosen = chosen with
        {
            VisualQaBoundaryTrace = chosen.VisualQaBoundaryTrace with
            {
                TierPolicy = chosen.VisualQaBoundaryTrace.TierPolicy with
                {
                    ShouldContinue = false,
                    StopReason = DesignConversionVisualQaBoundaryPolicy.QualityReached
                }
            }
        };
        rejected = rejected with
        {
            VisualQaBoundaryTrace = rejected.VisualQaBoundaryTrace with
            {
                TierPolicy = rejected.VisualQaBoundaryTrace.TierPolicy with
                {
                    ShouldContinue = true,
                    StopReason = DesignConversionVisualQaBoundaryPolicy.ContinueImprovement
                },
                VisualDecision = rejected.VisualQaBoundaryTrace.VisualDecision with
                {
                    CanRetry = true,
                    Decision = "retry"
                }
            }
        };
        return new PairFixture(chosen, rejected);
    }

    private static DesignConversionInternalEvaluationResult WithSourceManifestRef(
        DesignConversionInternalEvaluationResult evaluation,
        string sourceManifestRef)
        => evaluation with
        {
            SubmittedRequest = evaluation.SubmittedRequest with
            {
                Provenance = new Dictionary<string, string>(
                    evaluation.SubmittedRequest.Provenance,
                    StringComparer.Ordinal)
                {
                    ["source_manifest_ref"] = sourceManifestRef
                }
            }
        };

    private static PairFixture CreateProviderAllowedPair(string scope)
    {
        var pair = CreatePair(scope);
        return new PairFixture(
            WithVisualJudgeTrainingAllowed(pair.Chosen),
            WithVisualJudgeTrainingAllowed(pair.Rejected));
    }

    private static DesignConversionInternalEvaluationResult
        WithVisualJudgeTrainingAllowed(
            DesignConversionInternalEvaluationResult evaluation)
    {
        const string providerReady = "visual_training_allowed";
        return evaluation with
        {
            VisualJudgeResults = evaluation.VisualJudgeResults
                .Select(judge => judge with
                {
                    Status = providerReady,
                    IsBlocked = false,
                    Provider = judge.Provider with
                    {
                        ExternalCallExecuted = true
                    },
                    Metadata = new Dictionary<string, string>(
                        judge.Metadata,
                        StringComparer.Ordinal)
                    {
                        ["visual_training_provider_allowed"] =
                            bool.TrueString
                    }
                })
                .ToArray(),
            VisualQaBoundaryTrace = evaluation.VisualQaBoundaryTrace with
            {
                VisualJudgeStatus = providerReady,
                VisualJudgeProviderBoundary = providerReady,
                ExternalAiVisualJudgeExecuted = true,
                VisualDecision =
                    evaluation.VisualQaBoundaryTrace.VisualDecision with
                    {
                        ProviderBoundaryStatus = providerReady,
                        ExternalAiVisualJudgeExecuted = true
                    }
            }
        };
    }

    private static DesignConversionDpoTeacherAttestationRuntime
        CreateTeacherRuntime(ITrainingTeacherInferenceClient teacher)
        => new(
            teacher,
            ExpectedTeacherModelId,
            ExpectedTeacherManifestDigest,
            ExpectedTeacherModelLayerDigest);

    private static DesignConversionInternalEvaluateRequest CreateInternalRequest(
        SubmittedDesignConversionRequest request)
        => new(
            request.RequestId + "-root-job",
            request.RequestId,
            request.SourceKind,
            request.Html,
            request.Css,
            request.DesignTreePayload,
            request.OriginalTreePayload,
            request.Screenshots,
            request.DesignRenders,
            request.Budget with { MaxImprovementCandidates = 2 },
            [new RenderQaResolution("desktop", 1440, 900, 1)],
            AiTeacherEvidenceRefs: [],
            DeterministicEvidenceRefs: new Dictionary<string, string>(),
            Provenance: new Dictionary<string, string>(
                request.Provenance,
                StringComparer.Ordinal));

    private static string CreateRepositoryOwnedDpoRepositoryRoot(string scope)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-repository-owned-dpo-" +
            scope +
            "-" +
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(
            Path.Combine(root, "LlmFromScratch.sln"),
            "repository-owned DPO test sentinel");
        Directory.CreateDirectory(Path.Combine(root, "14.ContinuousLearning"));
        Directory.CreateDirectory(Path.Combine(root, "11.DPO"));
        return root;
    }

    private static RepositoryOwnedDpoDatasetAddress
        CreateRepositoryOwnedDpoDatasetAddress(string repositoryRoot)
    {
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateDpoDatasetAddress(
                repositoryRoot,
                "figma-html-css",
                "learning-unit",
                "v0001");
        Assert.True(address.IsSuccess, address.ErrorMessage);
        return address.Value!;
    }

    private static RepositoryOwnedDpoDatasetRequestIdentity
        CreateRepositoryOwnedDpoRequestIdentity(
            string scope,
            string paidReferenceRawText = "paid answer used only as a hash")
    {
        var identity = RepositoryOwnedDpoDatasetManifestSealer
            .CreateRequestIdentity(
                [Sha256Hex(paidReferenceRawText)],
                new RepositoryOwnedDpoOpenTeacherIdentity(
                    OpenWeightTeacherFamily.Qwen.ToString(),
                    ExpectedTeacherModelId,
                    ExpectedTeacherManifestDigest,
                    LegalTrainingTeacherAdmissionAuthority
                        .OllamaModelLayerSha256DigestKind,
                    ExpectedTeacherModelLayerDigest),
                new RepositoryOwnedDpoGenerationPolicyIdentity(
                    "design-conversion-existing-dpo-reuse",
                    "v1",
                    Sha256Hex("generation policy " + scope)),
                new RepositoryOwnedDpoCreatorIdentity(
                    "design-conversion-existing-dpo-reuse",
                    "v1",
                    Sha256Hex("creator policy " + scope)));
        Assert.True(identity.IsSuccess, identity.ErrorMessage);
        return identity.Value!;
    }

    private static DpoPair CreateRepositoryOwnedDpoPair(
        string scope,
        RepositoryOwnedDpoDatasetRequestIdentity identity,
        string paidReferenceRawText)
    {
        var passport = new TrainingTeacherProvenance(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            identity.OpenTeacher.ModelId,
            identity.OpenTeacher.ModelVersion,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            "repository-owned-dpo-" + scope,
            InvocationCount: 1)
        {
            ModelArtifactDigestKind =
                identity.OpenTeacher.ModelArtifactDigestKind,
            ModelArtifactDigest = identity.OpenTeacher.ModelArtifactSha256
        };
        var pair = DpoPairTrainingContentAuthority.Bind(new DpoPair(
            "build a responsive card for " + scope,
            "open teacher candidate for " + scope,
            "local model candidate for " + scope,
            "figma-html-css",
            DateTimeOffset.UnixEpoch)
        {
            TeacherProvenance = passport,
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                evidenceRefs: ["sha256:" + Sha256Hex("governance " + scope)]),
            ChosenAuthorship = new DpoPairSideAuthorship(
                DPO.Models.DpoPairSideAuthorKind.OpenTeacher,
                passport),
            RejectedAuthorship = new DpoPairSideAuthorship(
                DPO.Models.DpoPairSideAuthorKind.LocalModel)
        });
        Assert.True(pair.IsSuccess, pair.ErrorMessage);
        Assert.NotEqual(paidReferenceRawText, pair.Value!.Chosen);
        Assert.NotEqual(paidReferenceRawText, pair.Value.Rejected);
        return pair.Value;
    }

    private static void AssertRepositoryOwnedDpoUnavailableStop(
        DesignConversionImprovementLoopResult completed,
        string expectedErrorCode)
    {
        Assert.Equal(
            bool.TrueString,
            completed.PublicMetadata[
                DesignConversionRepositoryOwnedDpoTransactionContext
                    .EnabledMetadataKey]);
        Assert.Equal(
            bool.TrueString,
            completed.PublicMetadata[
                DesignConversionRepositoryOwnedDpoTransactionContext
                    .ValidationMetadataKey]);
        Assert.Equal(
            "unavailable",
            completed.PublicMetadata[
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetResolutionSourceMetadataKey]);
        Assert.Equal(
            "0",
            completed.PublicMetadata[
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetPairCountMetadataKey]);
        Assert.Equal(
            "unavailable",
            completed.PublicMetadata[
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetManifestSha256MetadataKey]);
        Assert.Equal(
            bool.FalseString,
            completed.PublicMetadata[
                DesignConversionImprovementLoopCoordinator
                    .RepositoryOwnedDpoDatasetLocalCandidateCreatedMetadataKey]);
        Assert.Contains(
            DesignConversionImprovementLoopCoordinator
                .RepositoryOwnedDpoDatasetResolutionFailedWarning +
            ":" +
            expectedErrorCode,
            completed.Warnings);
    }

    private static async Task AssertNoLocalDpoCandidateWasWrittenAsync(
        IDesignConversionLocalArtifactDb artifactDb)
    {
        var records = await artifactDb.ListRecordsAsync(
            new DesignConversionLocalArtifactDbQuery());
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Equal(
            0,
            records.Value!.Sum(record => record.DpoCandidates.Count));
    }

    private static T AssertSuccess<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);
        return result.Value!;
    }

    private static string Sha256Hex(string value)
        => Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static string BuildOutput(
        string prompt,
        bool supportsPreference = true)
    {
        using var document = JsonDocument.Parse(prompt);
        var root = document.RootElement;
        if (string.Equals(
                root.GetProperty("schema").GetString(),
                DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
                StringComparison.Ordinal))
        {
            if (!supportsPreference)
            {
                return JsonSerializer.Serialize(new
                {
                    schema = DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
                    incumbent_ref = root.GetProperty("incumbent_ref").GetString(),
                    candidate_html =
                        "<main data-teacher-authoring=\"candidate\">Candidate</main>",
                    candidate_css =
                        "[data-teacher-authoring=\"candidate\"]{display:grid;gap:24px;}",
                    preferred_side = "generated"
                });
            }

            return JsonSerializer.Serialize(new
            {
                schema = DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
                incumbent_ref = root.GetProperty("incumbent_ref").GetString(),
                candidate_html =
                    "<main data-teacher-authoring=\"candidate\">Candidate</main>",
                candidate_css =
                    "[data-teacher-authoring=\"candidate\"]{display:grid;gap:24px;}"
            });
        }

        var chosenRef = root.GetProperty("chosen").GetProperty("ref").GetString()!;
        var rejectedRef = root.GetProperty("rejected").GetProperty("ref").GetString()!;
        var evidenceRef = root.GetProperty("evidence_refs")[0].GetString()!;
        return JsonSerializer.Serialize(new
        {
            schema = DesignConversionDpoTeacherAttestationRuntime.Schema,
            chosen_ref = chosenRef,
            rejected_ref = rejectedRef,
            preferred_ref = chosenRef,
            supports_preference = supportsPreference,
            evidence_refs = new[] { evidenceRef }
        });
    }

    private static async Task<DesignConversionTrainingIntakeRecord>
        CreateTeacherPayloadCustodyIntakeAsync(string root)
    {
        var provider = new DesignConversionDpoPolicyBaseTestProvider();
        var artifactDb = new JsonlDesignConversionLocalArtifactDb(
            root,
            dpoPolicyBaseProvider: provider);
        using var teacher = new FakeTeacherInferenceClient();
        using var teacherRuntime = CreateTeacherRuntime(teacher);
        using var association = DesignConversionDpoTeacherAttestationAssociation
            .Associate(provider, teacherRuntime);
        using var independentPreferenceAssociation =
            DesignConversionDpoTeacherTestAssociation
                .AssociateIndependentPreferenceEvidence(artifactDb);

        foreach (var scope in new[] { "custody-train", "custody-heldout" })
        {
            var fixture = CreateProviderAllowedPair(scope);
            var coordinator = new DesignConversionImprovementLoopCoordinator(
                new RequestBindingSequenceEvaluator(
                    fixture.Rejected,
                    fixture.Chosen),
                new SingleCandidateStrategy(
                    fixture.Chosen.RuntimeFlowResult.Candidate),
                artifactDb,
                utcNow: () => DateTimeOffset.UnixEpoch);
            var completed = await coordinator.RunAsync(new(
                scope + "-loop",
                CreateInternalRequest(fixture.Rejected.SubmittedRequest),
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>()));
            Assert.True(completed.IsSuccess, completed.ErrorMessage);
        }

        var records = await artifactDb.ListRecordsAsync(
            new DesignConversionLocalArtifactDbQuery());
        Assert.True(records.IsSuccess, records.ErrorMessage);
        Assert.Equal(2, records.Value!.Count);
        var api = new DesignConversionDpoUsefulnessApi(artifactDb);
        foreach (var record in records.Value)
        {
            Assert.Equal(2, record.DpoCandidates.Count);
            foreach (var candidate in record.DpoCandidates)
            {
                var approval = await api.ApproveRuntimeCreatedDpoCandidateAsync(
                    record.RecordId,
                    candidate.CandidateId,
                    "reviewer:payload-custody",
                    "verified payload-custody training intake",
                    DateTimeOffset.UnixEpoch.AddMinutes(10));
                Assert.True(approval.IsSuccess, approval.ErrorMessage);
            }
        }

        var handoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
            exportedAtUtc: DateTimeOffset.UnixEpoch.AddMinutes(11));
        Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
        Assert.Equal(4, handoff.Value!.Rows.Count);
        var intake = await api.CreateTrainingIntakeAsync(
            handoff.Value,
            "requester:payload-custody",
            "two-source verified payload custody",
            DateTimeOffset.UnixEpoch.AddMinutes(12));
        Assert.True(intake.IsSuccess, intake.ErrorMessage);
        Assert.Equal(4, intake.Value!.AcceptedCandidateCount);
        return intake.Value;
    }

    private static (string Html, string Css) ReadPayloadResponse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.True(root.TryGetProperty("html", out var html));
        Assert.True(root.TryGetProperty("css", out var css));
        return (html.GetString() ?? string.Empty, css.GetString() ?? string.Empty);
    }

    private static IReadOnlyList<DesignConversionCandidatePayloadCustodyEntry>
        ResolveCandidatePayloadCustodyEntries(
            string root,
            DesignConversionLocalArtifactDbRecord record)
    {
        var bindings = record.DpoCandidates
            .Select(candidate => candidate.TrainingRecord.Provenance.Metadata[
                DesignConversionDpoGenerator.PayloadCustodyBindingSha256MetadataKey])
            .Order(StringComparer.Ordinal)
            .ToArray();
        var custodyStore = new DesignConversionCandidatePayloadCustodyStore(root);
        var committed = custodyStore.ResolveCommitted(
            new DesignConversionCandidatePayloadCustodyCommitment(
                record.Metadata[
                    DesignConversionCandidatePayloadCustodyStore
                        .TransactionMetadataKey],
                record.Metadata[
                    DesignConversionCandidatePayloadCustodyStore
                        .ManifestMetadataKey],
                record.Metadata[
                    DesignConversionCandidatePayloadCustodyStore
                        .BindingSetMetadataKey],
                record.RecordId,
                record.Metadata[
                    JsonlDesignConversionLocalArtifactDb
                        .ImmutableAppendFingerprintMetadataKey],
                bindings));
        Assert.True(committed.IsSuccess, committed.ErrorMessage);
        var resolved = custodyStore.ResolveSecretEnvelope(committed.Value!);
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        var entries = bindings
            .Select(binding => DesignConversionCandidatePayloadCustodyStore
                .FindExactEntry(
                    resolved.Value!,
                    committed.Value!,
                    binding))
            .ToArray();
        Assert.All(entries, entry => Assert.NotNull(entry));
        return entries.Select(entry => entry!).ToArray();
    }

    private static void AssertCandidatePayload(
        IReadOnlyList<DesignConversionCandidatePayloadCustodyEntry> entries,
        string trainingRecordId,
        string expectedHtml,
        string expectedCss)
    {
        var entry = Assert.Single(entries, item => string.Equals(
            item.TrainingRecordId,
            trainingRecordId,
            StringComparison.Ordinal));
        var payload = ReadPayloadResponse(entry.Secret.ChosenResponse);
        Assert.Equal(expectedHtml, payload.Html);
        Assert.Equal(expectedCss, payload.Css);
    }

    private static void AssertPendingLocalTeacherCandidateBeforeApproval(
        DesignConversionLocalArtifactDbRecord localRecord,
        DesignConversionLocalArtifactDbDpoCandidate candidate)
    {
        var metadata = candidate.TrainingRecord.Provenance.Metadata;
        var state = BuildCandidateStateMessage(localRecord, candidate);
        Assert.True(candidate.TrainingCandidateOnly, state);
        Assert.True(candidate.ProductNotReady, state);
        Assert.True(candidate.AllowsTraining, state);
        Assert.False(candidate.ContainsUserPrivateData, state);
        Assert.False(candidate.IsQuarantined, state);
        Assert.Empty(candidate.BlockReasons);
        Assert.Equal(
            bool.TrueString,
            ReadMetadata(metadata, "runtime_created_dpo_candidate"));
        Assert.Equal(
            bool.TrueString,
            ReadMetadata(metadata, "runtime_teacher_authored_candidate"));
        Assert.False(metadata.ContainsKey(
            "runtime_teacher_selected_preference"));
        Assert.Equal(
            bool.TrueString,
            ReadMetadata(metadata, "independent_preference_proven"));
        Assert.Equal("pending", ReadMetadata(metadata, "approval_status"));
        Assert.Equal(
            JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus,
            ReadMetadata(metadata, "review_status"));
        Assert.Equal(
            bool.FalseString,
            ReadMetadata(metadata, "approved_for_training"));
        Assert.Equal(
            bool.FalseString,
            ReadMetadata(metadata, "production_dpo"));
        Assert.Equal(
            bool.FalseString,
            ReadMetadata(metadata, "model_readiness_claimed"));
        Assert.DoesNotContain(
            "provider_missing_or_blocked",
            candidate.BlockReasons);
    }

    private static string BuildCandidateStateMessage(
        DesignConversionLocalArtifactDbRecord localRecord,
        DesignConversionLocalArtifactDbDpoCandidate candidate)
    {
        var metadata = candidate.TrainingRecord.Provenance.Metadata;
        return string.Join(
            " | ",
            [
                $"trainingCandidateOnly={candidate.TrainingCandidateOnly}",
                $"productNotReady={candidate.ProductNotReady}",
                $"allowsTraining={candidate.AllowsTraining}",
                $"containsPrivateData={candidate.ContainsUserPrivateData}",
                $"isQuarantined={candidate.IsQuarantined}",
                $"blockReasons={string.Join(",", candidate.BlockReasons)}",
                $"approval_status={ReadMetadata(metadata, "approval_status")}",
                $"review_status={ReadMetadata(metadata, "review_status")}",
                $"approved_for_training={ReadMetadata(metadata, "approved_for_training")}",
                $"production_dpo={ReadMetadata(metadata, "production_dpo")}",
                $"visual_status={localRecord.VisualJudgeStatus.Status}",
                $"visual_provider_boundary={localRecord.VisualJudgeStatus.ProviderBoundary}",
                $"visual_tier_decision={localRecord.VisualJudgeStatus.TierDecision}"
            ]);
    }

    private static string ReadMetadata(
        IReadOnlyDictionary<string, string>? metadata,
        string key)
        => metadata is not null && metadata.TryGetValue(key, out var value)
            ? value
            : string.Empty;

    private static void WriteOwnerOnlyCustodyFile(
        string path,
        byte[] bytes,
        FileMode mode = FileMode.CreateNew)
    {
        if (mode == FileMode.Create && File.Exists(path))
        {
            File.Delete(path);
            mode = FileMode.CreateNew;
        }

        using var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
            path,
            mode,
            FileAccess.Write,
            FileShare.None,
            FileOptions.WriteThrough,
            4096);
        stream.Write(bytes);
        stream.Flush(flushToDisk: true);
        Assert.True(SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path));
    }

    [SupportedOSPlatform("linux")]
    private static (string CustodyRoot, string StageRoot, string CommittedRoot)
        CreateLinuxCustodySidecarDirectories(string root)
    {
        Directory.CreateDirectory(root, SecureLocalFileBoundary.OwnerDirectoryMode);
        var roots = GetCustodySidecarDirectories(root);
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(roots.CustodyRoot);
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(roots.StageRoot);
        SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(roots.CommittedRoot);
        return roots;
    }

    private static (string CustodyRoot, string StageRoot, string CommittedRoot)
        GetCustodySidecarDirectories(string root)
    {
        var custodyRoot = Path.Combine(
            root,
            DesignConversionCandidatePayloadCustodyStore.DirectoryName);
        return (
            custodyRoot,
            Path.Combine(custodyRoot, "staged"),
            Path.Combine(custodyRoot, "committed"));
    }

    private static bool CreateLinuxHardLink(string existingPath, string newPath)
        => OperatingSystem.IsLinux() &&
           CreateLinuxHardLinkNative(existingPath, newPath) == 0;

    [SupportedOSPlatform("linux")]
    private static bool TrySetAndObserveLinuxMode(
        string path,
        UnixFileMode mode)
    {
        try
        {
            File.SetUnixFileMode(path, mode);
            return File.GetUnixFileMode(path) == mode;
        }
        catch (Exception exception)
            when (exception is IOException or UnauthorizedAccessException or
                  PlatformNotSupportedException)
        {
            return false;
        }
    }

    [DllImport("libc", EntryPoint = "link", SetLastError = true)]
    private static extern int CreateLinuxHardLinkNative(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string existingPath,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string newPath);

    private static void TryDelete(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
        }
    }

    private sealed class WorkspaceLocalTestRoot : IDisposable
    {
        private readonly string _parent;
        private readonly string _childName;
        private int _disposed;

        private WorkspaceLocalTestRoot(string parent, string path, string childName)
        {
            _parent = parent;
            Path = path;
            _childName = childName;
        }

        internal string Path { get; }

        internal static WorkspaceLocalTestRoot Create(
            string parentName,
            string childPurpose)
        {
            var parent = System.IO.Path.TrimEndingDirectorySeparator(
                System.IO.Path.GetTempPath());
            Directory.CreateDirectory(parent);
            var childName = "dc-" + Guid.NewGuid().ToString("N");
            var path = System.IO.Path.Combine(parent, childName);
            return new WorkspaceLocalTestRoot(parent, path, childName);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            var parent = new DirectoryInfo(_parent);
            var child = new DirectoryInfo(Path);
            if (!string.Equals(child.Parent?.FullName, parent.FullName, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(child.Name, _childName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Refusing non-child workspace test root cleanup: " + Path);
            }

            if (Directory.Exists(child.FullName))
                Directory.Delete(child.FullName, recursive: true);

            if (Directory.Exists(child.FullName))
                throw new IOException("Workspace test root cleanup left child behind: " + child.FullName);
        }

    }

    private sealed record PairFixture(
        DesignConversionInternalEvaluationResult Chosen,
        DesignConversionInternalEvaluationResult Rejected);

    private sealed class RecordingDpoPairStore : IDpoPairStore
    {
        private readonly List<DpoPair> _pairs = [];

        public int AddCallCount => _pairs.Count;

        public OperationResult<string> Add(DpoPair pair)
        {
            _pairs.Add(pair);
            return OperationResult<string>.Ok($"recording:{_pairs.Count}");
        }

        public OperationResult<IReadOnlyList<DpoPair>> GetAll()
            => OperationResult<IReadOnlyList<DpoPair>>.Ok(_pairs.ToArray());
    }

    private static DesignConversionCandidatePayloadCustodyEntry CreateCustodyEntry(
        string recordId,
        string prompt,
        string chosen,
        string rejected)
    {
        var promptSha256 = TestSha256Ref(prompt);
        var chosenSha256 = TestSha256Ref(chosen);
        var rejectedSha256 = TestSha256Ref(rejected);
        var governanceSha256 = TestSha256Ref("governance:" + recordId);
        var bindingSha256 = TestSha256Ref(string.Join(
            "|",
            recordId,
            promptSha256,
            chosenSha256,
            rejectedSha256,
            governanceSha256));
        var secret = new DesignConversionDpoPayloadCustodySecret(
            DesignConversionDpoGenerator.PayloadCustodyVersion,
            prompt,
            chosen,
            rejected,
            promptSha256,
            chosenSha256,
            rejectedSha256,
            governanceSha256,
            bindingSha256);
        return new DesignConversionCandidatePayloadCustodyEntry(
            recordId,
            bindingSha256,
            governanceSha256,
            promptSha256,
            chosenSha256,
            rejectedSha256,
            secret);
    }

    private static string TestSha256Ref(string value)
        => "sha256:" + Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string[] CaptureFileTree(string root)
        => Directory.Exists(root)
            ? Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                .Select(path => string.Join(
                    "|",
                    Path.GetRelativePath(root, path).Replace('\\', '/'),
                    new FileInfo(path).Length.ToString(CultureInfo.InvariantCulture),
                    Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))))
                .Order(StringComparer.Ordinal)
                .ToArray()
            : [];

    private static DesignConversionMeasurementGateRequest
        BuildPassingAdmissionMeasurement(string scope)
        => new(
            GateId: $"admission:{scope}",
            UnitId: DesignConversionDpoUnits.CriticUnit,
            HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                HeldOutRecordCount: 1,
                HeldOutQualityScore: 0.95,
                RuntimeMs: 10,
                CostUsd: 0.01m,
                PeakRamMb: 64,
                EvidenceRefs:
                [
                    $"artifact://measurement/{scope}/heldout-a.json",
                    $"artifact://measurement/{scope}/heldout-b.json"
                ]),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                SourceDisjoint: true,
                LeakageDetected: false,
                HiddenDeterminismDetected: false,
                AblationPassed: true,
                EvidenceRefs:
                [
                    $"artifact://measurement/{scope}/integrity-a.json",
                    $"artifact://measurement/{scope}/integrity-b.json"
                ]),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0.90,
                MaxRuntimeMs: 100,
                MaxCostUsd: 1m,
                MaxPeakRamMb: 512));

    private static DesignConversionInternalEvaluationResult WithLoopContinuation(
        DesignConversionInternalEvaluationResult evaluation)
        => evaluation with
        {
            VisualQaBoundaryTrace = evaluation.VisualQaBoundaryTrace with
            {
                TierPolicy = evaluation.VisualQaBoundaryTrace.TierPolicy with
                {
                    ShouldContinue = true,
                    StopReason = DesignConversionVisualQaBoundaryPolicy.ContinueImprovement,
                    MaxImprovementCandidates = 2
                },
                VisualDecision = evaluation.VisualQaBoundaryTrace.VisualDecision with
                {
                    CanRetry = true,
                    Decision = "retry"
                }
            }
        };

    private static DesignConversionInternalEvaluationResult WithScoreAndContinuation(
        DesignConversionInternalEvaluationResult evaluation,
        double score,
        int maxImprovementCandidates)
        => evaluation with
        {
            VisualQaBoundaryTrace = evaluation.VisualQaBoundaryTrace with
            {
                TierPolicy = evaluation.VisualQaBoundaryTrace.TierPolicy with
                {
                    ShouldContinue = true,
                    StopReason = DesignConversionVisualQaBoundaryPolicy.ContinueImprovement,
                    MaxImprovementCandidates = maxImprovementCandidates
                },
                VisualDecision = evaluation.VisualQaBoundaryTrace.VisualDecision with
                {
                    QualityScore = score,
                    CanPersistTrainingCandidate = true,
                    CanRetry = true,
                    Decision = "retry"
                }
            },
            RuntimeFlowResult = evaluation.RuntimeFlowResult with
            {
                Output = evaluation.RuntimeFlowResult.Output with
                {
                    Verdict = evaluation.RuntimeFlowResult.Output.Verdict with
                    {
                        Score = score
                    }
                }
            }
        };

    private sealed class RequestBindingSequenceEvaluator(
        params DesignConversionInternalEvaluationResult[] values)
        : IDesignConversionThinClientJobEvaluator
    {
        private int _next;

        public Task<OperationResult<DesignConversionInternalEvaluationResult>>
            EvaluateAsync(
                DesignConversionInternalEvaluateRequest request,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var template = values[Math.Min(_next++, values.Length - 1)];
            var submitted = template.SubmittedRequest with
            {
                RequestId = request.RequestId,
                Html = request.Html,
                Css = request.Css,
                Budget = request.Budget,
                Provenance = request.Provenance
            };
            var candidate = template.RuntimeFlowResult.Candidate with
            {
                RequestId = request.RequestId,
                Html = request.Html,
                Css = request.Css
            };
            var output = template.RuntimeFlowResult.Output with
            {
                Html = request.Html,
                Css = request.Css
            };
            var rebound = template with
            {
                JobId = request.JobId,
                SubmittedRequest = submitted,
                ImprovedHtml = request.Html,
                ImprovedCss = request.Css,
                RuntimeFlowResult = template.RuntimeFlowResult with
                {
                    Request = submitted,
                    Candidate = candidate,
                    Output = output,
                    ImproverOutput = template.RuntimeFlowResult.ImproverOutput with
                    {
                        Candidate = candidate,
                        Output = output
                    }
                }
            };
            return Task.FromResult(
                OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                    rebound));
        }
    }

    private sealed class CountingCandidateStrategy(
        DesignConversionImprovementCandidate candidate)
        : IDesignConversionImprovementCandidateStrategy
    {
        public int InvocationCount { get; private set; }

        public Task<OperationResult<DesignConversionImprovementCandidate>>
            CreateNextCandidateAsync(
                DesignConversionCandidateUpdateRequest request,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            InvocationCount++;
            return Task.FromResult(
                OperationResult<DesignConversionImprovementCandidate>.Ok(
                    candidate));
        }
    }

    private sealed class SequenceEvaluator(
        params DesignConversionInternalEvaluationResult[] values)
        : IDesignConversionThinClientJobEvaluator
    {
        private int _next;

        public Task<OperationResult<DesignConversionInternalEvaluationResult>>
            EvaluateAsync(
                DesignConversionInternalEvaluateRequest request,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var index = Math.Min(_next++, values.Length - 1);
            return Task.FromResult(
                OperationResult<DesignConversionInternalEvaluationResult>.Ok(
                    values[index]));
        }
    }

    private sealed class SingleCandidateStrategy(
        DesignConversionImprovementCandidate candidate)
        : IDesignConversionImprovementCandidateStrategy
    {
        public Task<OperationResult<DesignConversionImprovementCandidate>>
            CreateNextCandidateAsync(
                DesignConversionCandidateUpdateRequest request,
                CancellationToken cancellationToken = default)
            => Task.FromResult(
                OperationResult<DesignConversionImprovementCandidate>.Ok(
                    candidate));
    }

    private sealed class RejectedEvaluationSource(
        DesignConversionInternalEvaluationResult chosen,
        DesignConversionInternalEvaluationResult rejected)
        : IDesignConversionRuntimeRejectedEvaluationSource
    {
        public Task<OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>>
            ResolveMeasuredRejectedEvaluationAsync(
                DesignConversionRuntimeRejectedEvaluationLookup chosenEvaluation,
                CancellationToken cancellationToken = default)
            => Task.FromResult(
                OperationResult<DesignConversionRuntimeRejectedEvaluationReference?>.Ok(
                    DesignConversionRuntimeRejectedEvaluationReference
                        .CreateInternallyHydrated(
                            rejected.JobId,
                            rejected,
                            chosen.JobId,
                            chosen)));
    }

    private sealed class FakeTeacherInferenceClient : ITrainingTeacherInferenceClient
    {
        private readonly string _scenario;

        internal FakeTeacherInferenceClient(string scenario = "success")
            => _scenario = scenario;

        public int InvocationCount { get; private set; }
        public string LeakMarker => "RAW_TEACHER_SECRET_" + _scenario;
        public string? LastOutput { get; private set; }
        public string? LastPrompt { get; private set; }
        public List<string> Prompts { get; } = [];
        public TrainingTeacherProvenance? LastProvenance { get; private set; }
        public CancellationToken CapturedCancellationToken { get; private set; }
        public TaskCompletionSource<bool> Entered { get; } = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<OperationResult<TrainingTeacherInferenceResult>> InferAsync(
            string prompt,
            ITrainingTeacherCallPermit callPermit,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var issued = callPermit.TryIssue();
            if (!issued.IsSuccess)
                return issued.MapError<TrainingTeacherInferenceResult>();
            InvocationCount++;
            LastPrompt = prompt;
            Prompts.Add(prompt);
            CapturedCancellationToken = cancellationToken;
            Entered.TrySetResult(true);
            if (_scenario == "cancel")
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                throw new InvalidOperationException("Unreachable cancellation branch.");
            }
            if (_scenario == "operation-failure")
            {
                return OperationResult<TrainingTeacherInferenceResult>.Fail(
                    LeakMarker,
                    "private teacher failure code!");
            }
            if (_scenario == "first-operation-failure" && InvocationCount == 1)
            {
                return OperationResult<TrainingTeacherInferenceResult>.Fail(
                    LeakMarker,
                    "private first teacher failure code!");
            }
            if (_scenario == "throw-ordinary")
                throw new InvalidOperationException(LeakMarker);

            LastOutput = _scenario switch
            {
                "malformed" => LeakMarker + "<not-json>",
                "preference-rejected" => BuildOutput(
                    prompt,
                    supportsPreference: false),
                "incumbent-preferred" => BuildAuthoringOutput(
                    prompt,
                    "incumbent"),
                "plural" => BuildPluralAuthoringOutput(prompt, InvocationCount),
                "plural-duplicate-provenance" =>
                    BuildPluralAuthoringOutput(prompt, InvocationCount),
                "first-operation-failure" =>
                    BuildPluralAuthoringOutput(prompt, InvocationCount),
                _ => BuildOutput(prompt)
            };
            var provenance = BuildProvenance(_scenario, InvocationCount);
            if (_scenario is "plural" or "first-operation-failure")
            {
                provenance = provenance with
                {
                    RequestId = $"teacher-production-path-plural-{InvocationCount}"
                };
            }
            else if (_scenario == "plural-duplicate-provenance")
            {
                provenance = provenance with
                {
                    RequestId = "teacher-production-path-duplicate-provenance"
                };
            }
            LastProvenance = provenance;
            return OperationResult<TrainingTeacherInferenceResult>.Ok(new(
                LastOutput,
                provenance)
            {
                InferenceProfile =
                    LocalQwenOllamaInferenceProfileAuthority.Authoring
            });
        }

        private static TrainingTeacherProvenance BuildProvenance(
            string scenario,
            int invocationCount)
        {
            if (scenario == "invalid-provenance")
            {
                return TrainingTeacherProvenance.Deterministic(
                    "invalid-production-path-teacher");
            }

            var provenance = new TrainingTeacherProvenance(
                TrainingTeacherSourceKind.ExternalTeacherOutput,
                OpenWeightTeacherFamily.Qwen,
                TrainingTeacherAccessMode.LocalHostedOpenWeight,
                TrainingTeacherHostingProvider.Ollama,
                ExpectedTeacherModelId,
                ExpectedTeacherManifestDigest,
                TrainingTeacherModelLicense.Apache20,
                ProviderTermsChecked: false,
                ProviderTermsAllowTraining: false,
                ClosedProprietaryModel: false,
                TeacherOutputUsedForTraining: true,
                RequestId: $"teacher-production-path-test-{invocationCount}",
                InvocationCount: 1)
            {
                ModelArtifactDigestKind =
                    LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                ModelArtifactDigest = ExpectedTeacherModelLayerDigest
            };
            return scenario switch
            {
                "provider-terms-checked" => provenance with
                {
                    ProviderTermsChecked = true
                },
                "provider-terms-allow-training" => provenance with
                {
                    ProviderTermsAllowTraining = true
                },
                "model-id-blank" => provenance with { ModelId = string.Empty },
                "model-id-untrimmed" => provenance with { ModelId = " qwen-test:9b" },
                "model-id-control" => provenance with { ModelId = "qwen-test:9b\u0001" },
                "model-id-overlong" => provenance with
                {
                    ModelId = "qwen-" + new string(
                        'a',
                        DesignConversionDpoTeacherAttestationRuntime
                            .MaximumProvenanceIdentityLength)
                },
                "model-id-uppercase" => provenance with { ModelId = "Qwen-test:9b" },
                "model-id-untagged" => provenance with { ModelId = "qwen-test" },
                "model-id-wrong-canonical" => provenance with { ModelId = "qwen-other:9b" },
                "model-id-wrong-prefix" => provenance with { ModelId = "mistral-test:9b" },
                "model-version-blank" => provenance with { ModelVersion = string.Empty },
                "model-version-untrimmed" => provenance with
                {
                    ModelVersion = ExpectedTeacherManifestDigest + " "
                },
                "model-version-control" => provenance with
                {
                    ModelVersion = ExpectedTeacherManifestDigest[..^1] + "\u0001"
                },
                "model-version-overlong" => provenance with
                {
                    ModelVersion = new string(
                        'v',
                        DesignConversionDpoTeacherAttestationRuntime
                            .MaximumProvenanceIdentityLength + 1)
                },
                "model-version-short" => provenance with
                {
                    ModelVersion = new string('c', 63)
                },
                "model-version-uppercase" => provenance with
                {
                    ModelVersion = new string('C', 64)
                },
                "model-version-nonhex" => provenance with
                {
                    ModelVersion = new string('c', 63) + "g"
                },
                "model-version-wrong-canonical" => provenance with
                {
                    ModelVersion = new string('d', 64)
                },
                "model-artifact-kind-wrong" => provenance with
                {
                    ModelArtifactDigestKind = "ollama_manifest_sha256"
                },
                "model-artifact-digest-missing" => provenance with
                {
                    ModelArtifactDigest = string.Empty
                },
                "model-artifact-digest-wrong-canonical" => provenance with
                {
                    ModelArtifactDigest = new string('e', 64)
                },
                "request-id-blank" => provenance with { RequestId = string.Empty },
                "request-id-untrimmed" => provenance with { RequestId = " request-id" },
                "request-id-control" => provenance with { RequestId = "request\u0001-id" },
                "request-id-overlong" => provenance with
                {
                    RequestId = new string(
                        'r',
                        DesignConversionDpoTeacherAttestationRuntime
                            .MaximumProvenanceIdentityLength + 1)
                },
                _ => provenance
            };
        }

        public void Dispose()
        {
        }

        private static string BuildAuthoringOutput(
            string prompt,
            string obsoleteDirectionFixture)
        {
            using var document = JsonDocument.Parse(prompt);
            return JsonSerializer.Serialize(new
            {
                schema = DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
                incumbent_ref = document.RootElement
                    .GetProperty("incumbent_ref")
                    .GetString(),
                candidate_html =
                    "<main data-teacher-authoring=\"candidate\">Candidate</main>",
                candidate_css =
                    "[data-teacher-authoring=\"candidate\"]{display:grid;gap:24px;}"
            });
        }

        private static string BuildPluralAuthoringOutput(
            string prompt,
            int invocationCount)
        {
            using var document = JsonDocument.Parse(prompt);
            return JsonSerializer.Serialize(new
            {
                schema = DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
                incumbent_ref = document.RootElement
                    .GetProperty("incumbent_ref")
                    .GetString(),
                candidate_html =
                    $"<main data-teacher-authoring=\"candidate-{invocationCount}\">Candidate {invocationCount}</main>",
                candidate_css =
                    $"[data-teacher-authoring=\"candidate-{invocationCount}\"]{{display:grid;gap:{20 + invocationCount}px;}}"
            });
        }
    }
}

internal static class DesignConversionDpoTeacherTestAssociation
{
    internal static IDisposable AssociateIndependentPreferenceEvidence(
        IDesignConversionLocalArtifactDb artifactDb)
        => DesignConversionIndependentPreferenceEvidenceAssociation
            .AssociateArtifactDbForTesting(
                artifactDb,
                new FakeIndependentPreferenceEvidenceSource());

    internal static IDisposable Associate(
        IDesignConversionLocalArtifactDb artifactDb,
        bool supportTeacherAuthoring = false,
        bool supportIndependentPreferenceEvidence = false)
    {
        var runtime = new DesignConversionDpoTeacherAttestationRuntime(
            new FakeTeacherInferenceClient(supportTeacherAuthoring),
            "qwen-test:9b",
            new string('c', 64),
            new string('d', 64));
        try
        {
            var association = DesignConversionDpoTeacherAttestationAssociation
                .AssociateArtifactDbForTesting(artifactDb, runtime);
            IDisposable? evidenceAssociation = null;
            try
            {
                if (supportIndependentPreferenceEvidence)
                {
                    evidenceAssociation =
                        DesignConversionIndependentPreferenceEvidenceAssociation
                            .AssociateArtifactDbForTesting(
                                artifactDb,
                                new FakeIndependentPreferenceEvidenceSource());
                }

                return new Lease(association, evidenceAssociation, runtime);
            }
            catch
            {
                evidenceAssociation?.Dispose();
                association.Dispose();
                throw;
            }
        }
        catch
        {
            runtime.Dispose();
            throw;
        }
    }

    private sealed class FakeIndependentPreferenceEvidenceSource
        : IDesignConversionIndependentPreferenceEvidenceSource
    {
        public bool TryConsumeEvidence(
            string artifactRootBinding,
            DesignConversionTeacherAuthoredEvaluationPair pair,
            DateTimeOffset observedAtUtc,
            out DesignConversionIndependentPreferenceEvidence evidence)
        {
            var preferredDirection =
                pair.GeneratedEvaluation.VisualQaBoundaryTrace.VisualDecision
                    .QualityScore >=
                pair.IncumbentEvaluation.VisualQaBoundaryTrace.VisualDecision
                    .QualityScore
                    ? DesignConversionIndependentPreferenceEvidenceContract
                        .GeneratedSide
                    : DesignConversionIndependentPreferenceEvidenceContract
                        .IncumbentSide;
            evidence = DesignConversionIndependentPreferenceEvidenceTestIssuer
                .Issue(
                    pair,
                    artifactRootBinding,
                    observedAtUtc,
                    preferredDirection);
            return true;
        }
    }

    private sealed class FakeTeacherInferenceClient(bool supportTeacherAuthoring)
        : ITrainingTeacherInferenceClient
    {
        private readonly bool _supportTeacherAuthoring = supportTeacherAuthoring;
        private int _authoringInvocationCount;

        public Task<OperationResult<TrainingTeacherInferenceResult>> InferAsync(
            string prompt,
            ITrainingTeacherCallPermit callPermit,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var issued = callPermit.TryIssue();
            if (!issued.IsSuccess)
            {
                return Task.FromResult(
                    issued.MapError<TrainingTeacherInferenceResult>());
            }
            using var document = JsonDocument.Parse(prompt);
            var root = document.RootElement;
            var schema = root.GetProperty("schema").GetString();
            var authoringInvocationCount =
                _supportTeacherAuthoring &&
                string.Equals(
                schema,
                DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
                StringComparison.Ordinal)
                ? Interlocked.Increment(ref _authoringInvocationCount)
                : 0;
            var output = authoringInvocationCount > 0
                ? BuildAuthoringOutput(root, authoringInvocationCount)
                : BuildAttestationOutput(root);
            return Task.FromResult(
                OperationResult<TrainingTeacherInferenceResult>.Ok(new(
                    output,
                    new TrainingTeacherProvenance(
                        TrainingTeacherSourceKind.ExternalTeacherOutput,
                        OpenWeightTeacherFamily.Qwen,
                        TrainingTeacherAccessMode.LocalHostedOpenWeight,
                        TrainingTeacherHostingProvider.Ollama,
                        "qwen-test:9b",
                        new string('c', 64),
                        TrainingTeacherModelLicense.Apache20,
                        ProviderTermsChecked: false,
                        ProviderTermsAllowTraining: false,
                        ClosedProprietaryModel: false,
                        TeacherOutputUsedForTraining: true,
                        RequestId: authoringInvocationCount > 0
                            ? "teacher-test-authoring-" +
                              authoringInvocationCount.ToString(
                                  CultureInfo.InvariantCulture)
                            : "teacher-test-association",
                        InvocationCount: 1)
                    {
                        ModelArtifactDigestKind =
                            LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                        ModelArtifactDigest = new string('d', 64)
                    })
                {
                    InferenceProfile =
                        LocalQwenOllamaInferenceProfileAuthority.Authoring
                }));
        }

        public void Dispose()
        {
        }

        private static string BuildAttestationOutput(JsonElement root)
        {
            var chosenRef = root.GetProperty("chosen").GetProperty("ref")
                .GetString()!;
            var rejectedRef = root.GetProperty("rejected").GetProperty("ref")
                .GetString()!;
            var evidenceRef = root.GetProperty("evidence_refs")[0]
                .GetString()!;
            return JsonSerializer.Serialize(new
            {
                schema = DesignConversionDpoTeacherAttestationRuntime.Schema,
                chosen_ref = chosenRef,
                rejected_ref = rejectedRef,
                preferred_ref = chosenRef,
                supports_preference = true,
                evidence_refs = new[] { evidenceRef }
            });
        }

        private static string BuildAuthoringOutput(
            JsonElement root,
            int invocationCount)
        {
            var suffix = invocationCount.ToString(CultureInfo.InvariantCulture);
            return JsonSerializer.Serialize(new
            {
                schema = DesignConversionDpoTeacherAttestationRuntime.AuthoringSchema,
                incumbent_ref = root.GetProperty("incumbent_ref").GetString(),
                candidate_html =
                    $"<main data-teacher-authoring=\"candidate-{suffix}\">Candidate {suffix}</main>",
                candidate_css =
                    $"[data-teacher-authoring=\"candidate-{suffix}\"]{{display:grid;gap:{(20 + invocationCount).ToString(CultureInfo.InvariantCulture)}px;}}"
            });
        }
    }

    private sealed class Lease(
        IDisposable association,
        IDisposable? evidenceAssociation,
        DesignConversionDpoTeacherAttestationRuntime runtime)
        : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;
            evidenceAssociation?.Dispose();
            association.Dispose();
            runtime.Dispose();
        }
    }
}
