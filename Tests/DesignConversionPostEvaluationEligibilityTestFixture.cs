// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

using AgentInfrastructure;
using DesignConversionDomain;
using Xunit;

internal static class DesignConversionPostEvaluationEligibilityTestFixture
{
    public static string CreateHandleBoundTempRoot(string scope)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        return Path.GetFullPath(
            Path.Combine(
                FindRepositoryRoot(),
                ".codex-test-temp",
                $"{SanitizePathSegment(scope)}-{Guid.NewGuid():N}"));
    }

    public static async Task<DesignConversionLocalArtifactDbRecord>
        AppendEligibleRecordAsync(
            JsonlDesignConversionLocalArtifactDb db,
            DesignConversionLocalArtifactDbRecord record)
    {
        var eligibleRecord = await PrepareProvisionalRecordAsync(db, record);
        var exact = await ResolveExactDurableCanonicalRecordAsync(
            db,
            eligibleRecord);
        await CommitVerifiedEligibleRecordAsync(db, exact);
        return exact;
    }

    public static async Task<DesignConversionLocalArtifactDbRecord>
        PrepareProvisionalRecordAsync(
            JsonlDesignConversionLocalArtifactDb db,
            DesignConversionLocalArtifactDbRecord record)
    {
        var stateRef =
            JsonDesignConversionPostEvaluationStateStore.BuildStateRef(
                $"test:{record.RecordId}");
        var store = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(
                db.AllowedArtifactRootDirectory,
                JsonDesignConversionPostEvaluationStateStore.DirectoryName));
        DesignConversionPostEvaluationState state;
        var statePath = Path.Combine(
            db.AllowedArtifactRootDirectory,
            JsonDesignConversionPostEvaluationStateStore.DirectoryName,
            $"{stateRef}.json");
        if (File.Exists(statePath))
        {
            var existing = await store.ReadAsync(stateRef);
            Assert.True(existing.IsSuccess, existing.ErrorMessage);
            Assert.NotNull(existing.Value);
            state = existing.Value!;
        }
        else
        {
            var initial = await store.WriteAsync(InitialState(stateRef));
            Assert.True(initial.IsSuccess, initial.ErrorMessage);
            state = initial.Value!;
        }

        var metadata = new Dictionary<string, string>(
            record.Metadata,
            StringComparer.Ordinal)
        {
            [JsonDesignConversionPostEvaluationStateStore.StateRefMetadataKey]
                = state.StateRef,
            [JsonDesignConversionPostEvaluationStateStore
                .PersistenceStatusMetadataKey]
                = JsonDesignConversionPostEvaluationStateStore
                    .PersistenceProvisional,
            [JsonDesignConversionPostEvaluationStateStore
                .TrainingEligibleMetadataKey] = bool.FalseString
        };
        return record with { Metadata = metadata };
    }

    public static async Task CommitEligibleRecordAsync(
        JsonlDesignConversionLocalArtifactDb db,
        DesignConversionLocalArtifactDbRecord record)
    {
        var exact = await ResolveExactDurableCanonicalRecordAsync(
            db,
            record);
        await CommitVerifiedEligibleRecordAsync(db, exact);
    }

    private static async Task<DesignConversionLocalArtifactDbRecord>
        ResolveExactDurableCanonicalRecordAsync(
            JsonlDesignConversionLocalArtifactDb db,
            DesignConversionLocalArtifactDbRecord record)
    {
        // Always execute the authority append. Its immutable replay contract
        // decides whether this is an exact idempotent replay or a conflicting
        // attempt; an ID-only pre-check can never grant test authority.
        var append = await db.AppendRecordAsync(record);
        Assert.True(append.IsSuccess, append.ErrorMessage);
        Assert.NotNull(append.Value);
        var canonicalAppend = append.Value!;

        var durable = await db.ListRecordsAsync(
            new DesignConversionLocalArtifactDbQuery(
                RequestId: canonicalAppend.RequestId));
        Assert.True(durable.IsSuccess, durable.ErrorMessage);
        var exact = Assert.Single(
            durable.Value!,
            item =>
                string.Equals(
                    item.RecordId,
                    canonicalAppend.RecordId,
                    StringComparison.Ordinal));
        Assert.True(
            JsonlDesignConversionLocalArtifactDb
                .HasExactDurableImmutableAppendIdentity(
                    canonicalAppend,
                    exact),
            "The durable authority row did not preserve the canonical immutable append fingerprint/content.");
        return exact;
    }

    private static async Task CommitVerifiedEligibleRecordAsync(
        JsonlDesignConversionLocalArtifactDb db,
        DesignConversionLocalArtifactDbRecord record)
    {
        var stateRef = record.Metadata[
            JsonDesignConversionPostEvaluationStateStore
                .StateRefMetadataKey];
        var store = new JsonDesignConversionPostEvaluationStateStore(
            Path.Combine(
                db.AllowedArtifactRootDirectory,
                JsonDesignConversionPostEvaluationStateStore.DirectoryName));
        var current = await store.ReadAsync(stateRef);
        Assert.True(current.IsSuccess, current.ErrorMessage);
        Assert.NotNull(current.Value);
        if (string.Equals(
                current.Value!.PersistenceStatus,
                JsonDesignConversionPostEvaluationStateStore
                    .PersistenceCommitted,
                StringComparison.Ordinal))
        {
            Assert.Equal(record.RecordId, current.Value.ArtifactRecordId);
            Assert.Equal(
                JsonDesignConversionPostEvaluationStateStore
                    .CompletionBindingLocalArtifactDbRecord,
                current.Value.CompletionBindingKind);
            Assert.Equal(
                record.RecordId,
                current.Value.CompletionBindingRef);
            return;
        }

        Assert.Equal(
            JsonDesignConversionPostEvaluationStateStore
                .PersistenceProvisional,
            current.Value.PersistenceStatus);
        var committed = await store.WriteAsync(
            current.Value with
            {
                PersistenceStatus =
                    JsonDesignConversionPostEvaluationStateStore
                        .PersistenceCommitted,
                LedgerRecordId = string.Empty,
                ArtifactRecordId = record.RecordId,
                CompletionBindingKind =
                    JsonDesignConversionPostEvaluationStateStore
                        .CompletionBindingLocalArtifactDbRecord,
                CompletionBindingRef = record.RecordId,
                UpdatedAtUtc = current.Value.UpdatedAtUtc.AddSeconds(1),
                Version = current.Value.Version
            });
        Assert.True(committed.IsSuccess, committed.ErrorMessage);
    }

    public static async Task<DesignConversionApprovedDpoHandoffArtifact>
        SeedApprovedHandoffAsync(
            JsonlDesignConversionLocalArtifactDb db,
            string scope,
            bool includeSourceManifest = true,
            string? semanticContentScope = null)
    {
        var trainingRecord = BuildApprovedRuntimeTrainingRecord(
            scope,
            includeSourceManifest,
            semanticContentScope);
        var localRecordId = $"local-record-{scope}";
        var requestId = $"request:{localRecordId}";
        var record = BuildArtifactRecord(
            localRecordId,
            requestId,
            trainingRecord,
            scope);
        await AppendEligibleRecordAsync(db, record);

        var export = await db.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(RequestId: requestId),
            DateTimeOffset.UnixEpoch);
        Assert.True(export.IsSuccess, export.ErrorMessage);
        Assert.Single(export.Value!.Rows);
        return export.Value;
    }

    private static DesignConversionDpoTrainingRecord
        BuildApprovedRuntimeTrainingRecord(
            string scope,
            bool includeSourceManifest,
            string? semanticContentScope)
    {
        var preferenceScope = semanticContentScope ?? scope;
        var example = DesignConversionDpoGeneratorTests.CreateExample(
            $"training-record-{scope}",
            DesignConversionDpoUnits.CriticUnit,
            usefulness: 0.91);
        var build = new DesignConversionDpoGenerator()
            .BuildTrainReadyRecords([example]);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        var record = Assert.Single(build.Value!.AcceptedRecords);
        var metadata = new Dictionary<string, string>(
            record.Provenance.Metadata,
            StringComparer.Ordinal)
        {
            ["runtime_created_dpo_candidate"] = bool.TrueString,
            ["split"] = "runtime_training_candidate_review",
            ["review_required"] = bool.TrueString,
            ["approval_required"] = bool.TrueString,
            ["approval_status"] = "approved",
            ["review_status"] =
                JsonlDesignConversionLocalArtifactDb
                    .RuntimeCandidateReviewApprovedStatus,
            ["approved_for_training"] = bool.TrueString,
            ["training_candidate_only"] = bool.TrueString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["trained_model_ready"] = bool.FalseString,
            ["production_dpo"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString,
            ["approved_by_ref"] = "reviewer:durability-fixture",
            ["approval_reason"] =
                "durable post-evaluation eligibility fixture",
            ["approved_at_utc"] =
                DateTimeOffset.UnixEpoch.AddMinutes(1).ToString("O")
        };
        if (includeSourceManifest)
            metadata["source_design_id"] = $"source-{scope}";
        else
            metadata.Remove("source_design_id");

        return record with
        {
            RecordId = $"training-record-{scope}",
            Provenance = record.Provenance with
            {
                SourceRequestId = $"request:{scope}",
                SourceManifestRef = includeSourceManifest
                    ? $"manifest://durability/{scope}"
                    : string.Empty,
                Metadata = metadata
            },
            Chosen = record.Chosen with
            {
                Html = $"artifact://durability/{preferenceScope}/chosen.html",
                Css = $"artifact://durability/{preferenceScope}/chosen.css",
                QaResults =
                [
                    BuildQaResult($"{preferenceScope}:chosen", 0.92, true)
                ]
            },
            Rejected = record.Rejected with
            {
                Html = $"artifact://durability/{preferenceScope}/rejected.html",
                Css = $"artifact://durability/{preferenceScope}/rejected.css",
                QaResults =
                [
                    BuildQaResult($"{preferenceScope}:rejected", 0.42, false)
                ]
            }
        };
    }

    private static DesignConversionLocalArtifactDbRecord BuildArtifactRecord(
        string recordId,
        string requestId,
        DesignConversionDpoTrainingRecord trainingRecord,
        string scope)
        => new(
            RecordId: recordId,
            DatabaseKind:
                JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
            JobId: $"job:{scope}",
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma.ToString(),
            Tier: "local-fixture",
            SanitizedSourceRefs: new Dictionary<string, string>
            {
                ["design_tree_payload_ref"] =
                    $"artifact://durability/{scope}/design-tree.json",
                ["original_tree_payload_ref"] =
                    $"artifact://durability/{scope}/original-tree.json",
                ["source_artifact:design_tree"] =
                    $"artifact://durability/{scope}/design-tree.json",
                ["source_artifact:original_tree"] =
                    $"artifact://durability/{scope}/original-tree.json"
            },
            ConversionResult: new DesignConversionConversionResultArtifact(
                ResultId: $"{recordId}:conversion",
                HtmlRef: $"artifact://durability/{scope}/chosen.html",
                HtmlSha256: $"sha256:{scope}:html",
                CssRef: $"artifact://durability/{scope}/chosen.css",
                CssSha256: $"sha256:{scope}:css",
                Metadata: new Dictionary<string, string>()),
            RenderArtifactRefs:
            [
                new DesignConversionRenderArtifactDbRef(
                    ViewportId: "desktop",
                    ArtifactId: $"{recordId}:render",
                    Kind: "browser_screenshot",
                    UriRef: $"artifact://durability/{scope}/render.png",
                    Sha256: $"sha256:{scope}:render",
                    ByteLength: 1024,
                    Width: 1440,
                    Height: 900,
                    Metadata: new Dictionary<string, string>())
            ],
            VisualJudgeStatus:
                new DesignConversionVisualJudgeArtifactDbStatus(
                    Status: "passed",
                    ProviderBoundary: "local-test-provider",
                    ExternalCallExecuted: false,
                    EstimatedCostUsd: 0m,
                    TierDecision: "local",
                    EvidenceRefs: [],
                    Metadata: new Dictionary<string, string>()),
            DpoCandidates:
            [
                DesignConversionLocalArtifactDbDpoCandidate
                    .FromTrainingRecord(
                        trainingRecord,
                        $"candidate-{scope}")
            ],
            CreatedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs:
            [
                "local_artifact_db",
                "training_candidate_only",
                "product_not_ready"
            ],
            Metadata: new Dictionary<string, string>());

    private static RenderQaResultRecord BuildQaResult(
        string id,
        double score,
        bool passed)
    {
        var resolution = new RenderQaResolution("desktop", 1440, 900, 1);
        var expected = new RenderQaImageRecord(
            $"{id}:expected",
            "expected",
            resolution,
            $"artifact://durability/{id}/expected.png",
            $"sha256:{id}:expected",
            new Dictionary<string, string>());
        var actual = new RenderQaImageRecord(
            $"{id}:actual",
            "actual",
            resolution,
            $"artifact://durability/{id}/actual.png",
            $"sha256:{id}:actual",
            new Dictionary<string, string>());
        return new RenderQaResultRecord(
            $"{id}:result",
            $"{id}:request",
            expected,
            actual,
            new VisualVerdict(
                $"{id}:verdict",
                score,
                [],
                [$"artifact://durability/{id}/comparison.png"],
                passed),
            new Dictionary<string, double>
            {
                ["visual_distance"] = 1.0 - score
            },
            []);
    }

    private static DesignConversionPostEvaluationState InitialState(
        string stateRef)
        => new(
            StateRef: stateRef,
            ReservationRef: stateRef,
            UserRef:
                JsonDesignConversionPostEvaluationStateStore
                    .NoReservationUserRef,
            PeriodStartUtc:
                JsonDesignConversionPostEvaluationStateStore
                    .NoReservationPeriodStartUtc,
            PeriodEndExclusiveUtc:
                JsonDesignConversionPostEvaluationStateStore
                    .NoReservationPeriodEndExclusiveUtc,
            ChargedCostUsd: 0m,
            AccountingStatus:
                JsonDesignConversionPostEvaluationStateStore
                    .AccountingSettled,
            AccountingDesiredAction:
                JsonDesignConversionPostEvaluationStateStore.DesiredNone,
            SettlementAttempted: false,
            SettlementDefinitivelyNotApplied: false,
            ReleaseAttempted: false,
            PersistenceStatus:
                JsonDesignConversionPostEvaluationStateStore
                    .PersistenceProvisional,
            LedgerRecordId: string.Empty,
            ArtifactRecordId: string.Empty,
            CompletionBindingKind: string.Empty,
            CompletionBindingRef: string.Empty,
            LastErrorCode: string.Empty,
            UpdatedAtUtc:
                new DateTimeOffset(
                    2026,
                    7,
                    30,
                    0,
                    0,
                    0,
                    TimeSpan.Zero),
            Version: 1);

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                return current.FullName;
            current = current.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the repository root for DesignConversion test output.");
    }

    private static string SanitizePathSegment(string value)
    {
        var chars = value.Select(static ch =>
            ch is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or
                >= '0' and <= '9' or '-' or '_' or '.'
                ? ch
                : '-').ToArray();
        var sanitized = new string(chars).Trim('-', '.');
        if (string.IsNullOrWhiteSpace(sanitized))
            return "root";
        return sanitized.Length <= 3 ? sanitized : sanitized[..3];
    }
}
