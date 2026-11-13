// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ContinuousLearning;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using LoRA;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tokenizer;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TransformerModel;
using TransformerModel.Config;
using SC29 = SemanticCompiler29;

namespace LlmFromScratch.Tests;

// 14.ContinuousLearning — LoRA per-project specialisation, E28 hypothesis test.
// Gate B: dotnet test --filter "ContinuousLearning" → 22 passed, 0 failed
// Acceptance: grep -rn "ISemanticCompiler" 14.ContinuousLearning/ → ≥ 1 line
public class ContinuousLearningTests : IDisposable
{
    [Fact]
    public void ServerOwnedReplaySeal_RequiresExactAuthorizedOrderedComposite()
    {
        (LogicalFlowResult Branch,
            LogicalTrainingContentDeterministicInputs Inputs,
            LogicalSubPhase SubPhase) Source(
                string marker,
                string subPhaseId = "subphase")
        {
            var branch = new LogicalFlowResult($"flow-{marker}", "scenario",
                "KNOWLEDGE_EDGE", marker,
                new LogicalFlowSummary($"summary-{marker}", 1, 0, 0,
                    "knowledge_edge", false, false, false, "TRAIN", 1,
                    marker, marker, 0, 0),
                new Dictionary<string, string> { ["marker"] = marker }, [], []);
            var inputs = new LogicalTrainingContentDeterministicInputs(
                "request", "PLAN", "phase", subPhaseId,
                "name", "purpose", ["P"], ["D"]);
            return (branch, inputs, new LogicalSubPhase(
                subPhaseId, "phase", "name", "purpose",
                ["D"], ["P"], []));
        }
        var aSource = Source("a");
        var bSource = Source("b");
        var cSource = Source("c", "other-subphase");
        var sameGroup = LogicalCapabilityTrainingAuthorityTestFactory
            .IssueGroup(
                (aSource.Branch, aSource.Inputs),
                (bSource.Branch, bSource.Inputs));
        var a = sameGroup[0];
        var b = sameGroup[1];
        var c = LogicalCapabilityTrainingAuthorityTestFactory.IssueSingle(
            cSource.Branch,
            cSource.Inputs);
        var aCurrent = LogicalCapabilityTrainingPreferenceProjector
            .ProjectCurrent("request", aSource.Branch, aSource.SubPhase).Value!;
        var bCurrent = LogicalCapabilityTrainingPreferenceProjector
            .ProjectCurrent("request", bSource.Branch, bSource.SubPhase).Value!;
        var cCurrent = LogicalCapabilityTrainingPreferenceProjector
            .ProjectCurrent("request", cSource.Branch, cSource.SubPhase).Value!;
        var batch = DefaultLogicalCapabilityTrainer.BuildAdmissionBatch(
            "exact-composite", "PLAN",
            [
                new LogicalDpoPreferenceExample(
                    aCurrent.Prompt, aCurrent.Chosen, aCurrent.Rejected,
                    aCurrent.TeachingVector)
                    { TrainingContentProvenance = a },
                new LogicalDpoPreferenceExample(
                    bCurrent.Prompt, bCurrent.Chosen, bCurrent.Rejected,
                    bCurrent.TeachingVector)
                    { TrainingContentProvenance = b },
                new LogicalDpoPreferenceExample(
                    cCurrent.Prompt, cCurrent.Chosen, cCurrent.Rejected,
                    cCurrent.TeachingVector)
                    { TrainingContentProvenance = c }
            ]);
        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        var legal = CreateLegalTeacherAuthority([], 0);
        var server = ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
            legal.ValidateBatch, static () => true,
            sealReplay: legal.SealLogicalCapabilityReplay,
            verifyReplay: legal.VerifyLogicalCapabilityReplay);
        using var lease = ((ILogicalCapabilityTrainingAdmissionAuthority)server)
            .AcquireTrainingLease(batch.Value!).Value!;
        LogicalCapabilityTrainingReplayDescriptor Descriptor(
            LogicalTrainingContentProvenance source) => new(
                LogicalCapabilityTrainingReplayDescriptor.CurrentSchemaVersion,
                LogicalTrainingContentSourceKind
                    .DeterministicSyntheticNoExternalTeacher,
                source.ReplaySealGroupSha256!,
                new LogicalCapabilityTrainingSourceBindingComposite(
                    source.AuthorizedReplaySourceBindingSha256s),
                source.AuthorizedReplayPayloadSha256!,
                source.AuthorizedReplayDpoPreferenceSha256!);

        var replayDescriptor = Descriptor(a);
        var cDescriptor = Descriptor(c);
        Assert.True(lease.SealReplay(replayDescriptor).IsSuccess);
        Assert.True(lease.SealReplay(cDescriptor).IsSuccess);
        Assert.False(lease.SealReplay(replayDescriptor with
        {
            SourceBindings = new(
                [a.BindingSha256])
        }).IsSuccess);
        Assert.False(lease.SealReplay(replayDescriptor with
        {
            SourceBindings = new(
                replayDescriptor.SourceBindings.Values.Reverse().ToArray())
        }).IsSuccess);
        Assert.False(lease.SealReplay(replayDescriptor with
        {
            SourceBindings = new(
                [a.BindingSha256, a.BindingSha256])
        }).IsSuccess);
        Assert.False(lease.SealReplay(replayDescriptor with
        {
            SourceBindings = new(
                replayDescriptor.SourceBindings.Values
                    .Append(c.BindingSha256)
                    .OrderBy(static value => value, StringComparer.Ordinal)
                    .ToArray())
        }).IsSuccess);
        Assert.False(lease.SealReplay(replayDescriptor with
        {
            ReplaySealGroupSha256 = new string('0', 64)
        }).IsSuccess);
        Assert.False(lease.SealReplay(replayDescriptor with
        {
            ReplaySealGroupSha256 = string.Empty
        }).IsSuccess);
        Assert.False(lease.SealReplay(replayDescriptor with
        {
            SchemaVersion = LogicalCapabilityTrainingReplayDescriptor
                .CurrentSchemaVersion - 1
        }).IsSuccess);
        Assert.False(lease.SealReplay(replayDescriptor with
        {
            ReplayPayloadSha256 = new string('d', 64)
        }).IsSuccess);
        Assert.False(lease.SealReplay(replayDescriptor with
        {
            DpoPreferenceSha256 = new string('e', 64)
        }).IsSuccess);
        Assert.False(lease.SealReplay(cDescriptor with
        {
            ReplayPayloadSha256 = replayDescriptor.ReplayPayloadSha256,
            DpoPreferenceSha256 = replayDescriptor.DpoPreferenceSha256
        }).IsSuccess);

        var alteredCurrent = DefaultLogicalCapabilityTrainer
            .BuildAdmissionBatch(
                "altered-current",
                "PLAN",
                [new LogicalDpoPreferenceExample(
                    aCurrent.Prompt,
                    aCurrent.Chosen + " altered",
                    aCurrent.Rejected,
                    aCurrent.TeachingVector)
                {
                    TrainingContentProvenance = a
                }]);
        var alteredVector = DefaultLogicalCapabilityTrainer
            .BuildAdmissionBatch(
                "altered-current-vector",
                "PLAN",
                [new LogicalDpoPreferenceExample(
                    aCurrent.Prompt,
                    aCurrent.Chosen,
                    aCurrent.Rejected,
                    aCurrent.TeachingVector.Select(static value => value + 1f)
                        .ToArray())
                {
                    TrainingContentProvenance = a
                }]);
        Assert.False(alteredCurrent.IsSuccess);
        Assert.False(alteredVector.IsSuccess);

        var replayProjection = LogicalCapabilityTrainingPreferenceProjector
            .ProjectAuthorizedReplay(
                [aCurrent, bCurrent],
                replayDescriptor.SourceBindings.Values,
                replayDescriptor.ReplaySealGroupSha256,
                "PLAN",
                aSource.SubPhase).Value!;
        var replayAttestation = lease.SealReplay(replayDescriptor);
        Assert.True(replayAttestation.IsSuccess,
            replayAttestation.ErrorMessage);
        var replayBatch = DefaultLogicalCapabilityTrainer.BuildAdmissionBatch(
            "replay-cannot-authorize", "PLAN",
            [new LogicalDpoPreferenceExample(
                replayProjection.TrainingText,
                replayProjection.OutputText,
                replayProjection.RejectedText,
                replayProjection.TeachingVector)
            {
                ReplayDescriptor = replayDescriptor,
                ReplayAttestation = replayAttestation.Value
            }]);
        Assert.True(replayBatch.IsSuccess, replayBatch.ErrorMessage);
        using var replayOnlyLease =
            ((ILogicalCapabilityTrainingAdmissionAuthority)server)
            .AcquireTrainingLease(replayBatch.Value!).Value!;
        Assert.False(replayOnlyLease.SealReplay(replayDescriptor).IsSuccess);
    }
    private static readonly ISessionStorage Storage = new NullSessionStorageCL();
    private readonly List<string> _feedbackStorageRoots = [];

    [Fact]
    public void LegalTrainingTeacherAuthority_ExactAllowlistMatchUsesOnlySaltedPublicReceipts()
    {
        const string privateRequest = "PRIVATE-teacher-request-001";
        var authority = CreateLegalTeacherAuthority([QwenPolicy()], maxCalls: 1);
        var result = authority.ValidateBatch([QwenProvenance(privateRequest)]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var receipt = Assert.Single(result.Value!.Receipts);
        Assert.Equal(OpenWeightTeacherFamily.Qwen, receipt.Family);
        Assert.Equal(TrainingTeacherHostingProvider.Ollama, receipt.HostingProvider);
        Assert.Equal(1, result.Value.ExternalTeacherInvocationCount);
        Assert.StartsWith("sha256:", receipt.RequestRef, StringComparison.Ordinal);
        Assert.StartsWith("sha256:", receipt.ModelRef, StringComparison.Ordinal);
        Assert.Equal(
            IndependentHmacRef(
                LegalTeacherSalt(),
                "teacher-model-artifact-digest",
                QwenModelLayerDigest),
            receipt.ModelArtifactRef);
        var differentSalt = Enumerable.Range(33, 32)
            .Select(static value => (byte)value)
            .ToArray();
        var differentAuthority = LegalTrainingTeacherAdmissionAuthority.Create(
            [QwenPolicy()],
            1,
            differentSalt);
        Assert.True(differentAuthority.IsSuccess, differentAuthority.ErrorMessage);
        var differentAdmission = differentAuthority.Value!
            .ValidateBatch([QwenProvenance(privateRequest)]);
        Assert.True(differentAdmission.IsSuccess, differentAdmission.ErrorMessage);
        var differentReceipt = Assert.Single(differentAdmission.Value!.Receipts);
        Assert.Equal(
            IndependentHmacRef(
                differentSalt,
                "teacher-model-artifact-digest",
                QwenModelLayerDigest),
            differentReceipt.ModelArtifactRef);
        Assert.NotEqual(receipt.ModelArtifactRef, differentReceipt.ModelArtifactRef);
        var serialized = JsonSerializer.Serialize(result.Value);
        Assert.DoesNotContain(privateRequest, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("qwen3.5-9b", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(QwenManifestDigest, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(QwenModelLayerDigest, serialized, StringComparison.Ordinal);
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_DeterministicModeIsExplicitAndNullFailsClosed()
    {
        var authority = CreateLegalTeacherAuthority([], maxCalls: 0);
        var provenance = TrainingTeacherProvenance.Deterministic("deterministic-request-001");

        var accepted = authority.ValidateBatch([provenance]);
        var missing = authority.ValidateBatch([null]);
        var falseClaim = authority.ValidateBatch(
            [provenance with { TeacherOutputUsedForTraining = true }]);
        var nullModelId = authority.ValidateBatch(
            [provenance with { ModelId = null! }]);
        var nullModelVersion = authority.ValidateBatch(
            [provenance with { ModelVersion = null! }]);
        var nullRequestId = authority.ValidateBatch(
            [provenance with { RequestId = null! }]);

        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        Assert.Equal(0, accepted.Value!.ExternalTeacherInvocationCount);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ProvenanceRequiredErrorCode,
            missing.ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ProvenanceInvalidErrorCode,
            falseClaim.ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ProvenanceInvalidErrorCode,
            nullModelId.ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ProvenanceInvalidErrorCode,
            nullModelVersion.ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ProvenanceInvalidErrorCode,
            nullRequestId.ErrorCode);
    }

    [Fact]
    public void ServerOwnedLegalTeacherAuthority_NeutralCapabilityBatchMapsExactRowsOnce()
    {
        (LogicalFlowResult Branch,
            LogicalTrainingContentDeterministicInputs Inputs,
            LogicalSubPhase SubPhase) Source(string marker)
        {
            var branch = new LogicalFlowResult(
                $"neutral-flow-{marker}",
                "scenario",
                "KNOWLEDGE_EDGE",
                marker,
                new LogicalFlowSummary(
                    $"neutral-summary-{marker}",
                    1,
                    0,
                    0,
                    "knowledge_edge",
                    false,
                    false,
                    false,
                    "TRAIN",
                    1,
                    marker,
                    marker,
                    0,
                    0),
                new Dictionary<string, string> { ["marker"] = marker },
                [],
                []);
            var inputs = new LogicalTrainingContentDeterministicInputs(
                "neutral exact request",
                "PLAN",
                "neutral-phase",
                "neutral-subphase",
                "neutral name",
                "neutral purpose",
                ["NEUTRAL"],
                ["neutral-domain"]);
            return (
                branch,
                inputs,
                new LogicalSubPhase(
                    inputs.SubPhaseId,
                    inputs.PhaseId,
                    inputs.SubPhaseName,
                    inputs.SubPhasePurpose,
                    inputs.DomainIds.ToArray(),
                    inputs.PredicateIds.ToArray(),
                    []));
        }

        var legalAuthority = CreateLegalTeacherAuthority([], maxCalls: 0);
        var validationCount = 0;
        TrainingTeacherProvenance?[] observed = [];
        var serverAuthority = ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
            provenance =>
            {
                validationCount++;
                observed = provenance?.ToArray() ?? [];
                return legalAuthority.ValidateBatch(observed);
            },
            static () => true);
        var firstSource = Source("first");
        var secondSource = Source("second");
        var sourceProvenance = LogicalCapabilityTrainingAuthorityTestFactory
            .IssueGroup(
                (firstSource.Branch, firstSource.Inputs),
                (secondSource.Branch, secondSource.Inputs));
        var firstCurrent = LogicalCapabilityTrainingPreferenceProjector
            .ProjectCurrent(
                firstSource.Inputs.OriginalUserRequest,
                firstSource.Branch,
                firstSource.SubPhase);
        var secondCurrent = LogicalCapabilityTrainingPreferenceProjector
            .ProjectCurrent(
                secondSource.Inputs.OriginalUserRequest,
                secondSource.Branch,
                secondSource.SubPhase);
        Assert.True(firstCurrent.IsSuccess, firstCurrent.ErrorMessage);
        Assert.True(secondCurrent.IsSuccess, secondCurrent.ErrorMessage);
        var batch = DefaultLogicalCapabilityTrainer.BuildAdmissionBatch(
            "legal-adapter-exact-rows",
            "PLAN",
            [
                new LogicalDpoPreferenceExample(
                    firstCurrent.Value!.Prompt,
                    firstCurrent.Value.Chosen,
                    firstCurrent.Value.Rejected,
                    firstCurrent.Value.TeachingVector)
                {
                    TrainingContentProvenance = sourceProvenance[0].Snapshot()
                },
                new LogicalDpoPreferenceExample(
                    secondCurrent.Value!.Prompt,
                    secondCurrent.Value.Chosen,
                    secondCurrent.Value.Rejected,
                    secondCurrent.Value.TeachingVector)
                {
                    TrainingContentProvenance = sourceProvenance[1].Snapshot()
                }
            ]);
        Assert.True(batch.IsSuccess, batch.ErrorMessage);
        var exactBatch = Assert.IsType<LogicalCapabilityTrainingAdmissionBatch>(
            batch.Value);

        var leaseResult = ((ILogicalCapabilityTrainingAdmissionAuthority)serverAuthority)
            .AcquireTrainingLease(exactBatch);

        Assert.True(leaseResult.IsSuccess, leaseResult.ErrorMessage);
        Assert.Equal(1, validationCount);
        Assert.Equal(exactBatch.Rows.Select(static row => row.Sha256),
            observed.Select(static row => row!.RequestId).ToArray());
        Assert.All(observed, static row =>
        {
            var exactRow = Assert.IsType<TrainingTeacherProvenance>(row);
            Assert.Equal(
                TrainingTeacherSourceKind.DeterministicNoExternalTeacher,
                exactRow.SourceKind);
            Assert.Equal(0, exactRow.InvocationCount);
            Assert.False(exactRow.TeacherOutputUsedForTraining);
            Assert.Empty(exactRow.ModelArtifactDigestKind);
            Assert.Empty(exactRow.ModelArtifactDigest);
        });
        using var lease = leaseResult.Value!;
        var permit = lease.AcquireMutationPermit();
        Assert.True(permit.IsSuccess, permit.ErrorMessage);
        permit.Value!.Dispose();

        var publicRewrap = LogicalCapabilityTrainingAdmissionBatch.Create(
            exactBatch.Rows);
        Assert.True(publicRewrap.IsSuccess, publicRewrap.ErrorMessage);
        var rewrappedLease =
            ((ILogicalCapabilityTrainingAdmissionAuthority)serverAuthority)
            .AcquireTrainingLease(publicRewrap.Value!);
        Assert.False(rewrappedLease.IsSuccess);
        Assert.Equal(
            LogicalCapabilityTrainingAdmissionBatch
                .ProvenanceInvalidErrorCode,
            rewrappedLease.ErrorCode);
        Assert.Equal(1, validationCount);
    }

    [Fact]
    public void ServerOwnedLegalTeacherAuthority_RejectsUnattestedRowsBeforeValidationOrLease()
    {
        var validationCount = 0;
        var serverAuthority =
            ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
                _ =>
                {
                    validationCount++;
                    throw new InvalidOperationException(
                        "Validation must not run for unattested rows.");
                },
                static () => true);
        var identity = LogicalCapabilityTrainingRowIdentity.Create(
            new string('a', 64));
        Assert.True(identity.IsSuccess, identity.ErrorMessage);
        var batch = LogicalCapabilityTrainingAdmissionBatch.Create(
            [identity.Value]);
        Assert.True(batch.IsSuccess, batch.ErrorMessage);

        var leaseResult =
            ((ILogicalCapabilityTrainingAdmissionAuthority)serverAuthority)
            .AcquireTrainingLease(batch.Value!);

        Assert.False(leaseResult.IsSuccess);
        Assert.Equal(
            LogicalCapabilityTrainingAdmissionBatch
                .ProvenanceRequiredErrorCode,
            leaseResult.ErrorCode);
        Assert.Equal(0, validationCount);
    }

    [Fact]
    public void LogicalCapabilityReplay_HmacRejectsTamperingAndRotatedKey()
    {
        var first = CreateLegalTeacherAuthority([], maxCalls: 0);
        var secondResult = LegalTrainingTeacherAdmissionAuthority.Create(
            [],
            0,
            Enumerable.Range(65, 32).Select(static value => (byte)value)
                .ToArray());
        Assert.True(secondResult.IsSuccess, secondResult.ErrorMessage);
        var descriptor = new LogicalCapabilityTrainingReplayDescriptor(
            LogicalCapabilityTrainingReplayDescriptor.CurrentSchemaVersion,
            LogicalTrainingContentSourceKind
                .DeterministicSyntheticNoExternalTeacher,
            new string('f', 64),
            new LogicalCapabilityTrainingSourceBindingComposite(
                [new string('a', 64)]),
            new string('b', 64),
            new string('c', 64));

        var sealedReplay = first.SealLogicalCapabilityReplay(descriptor);
        Assert.True(sealedReplay.IsSuccess, sealedReplay.ErrorMessage);
        var verified = first.VerifyLogicalCapabilityReplay(
            sealedReplay.Value);
        var tampered = first.VerifyLogicalCapabilityReplay(
            sealedReplay.Value! with
            {
                ReplayPayloadSha256 = new string('c', 64)
            });
        var rotated = secondResult.Value!.VerifyLogicalCapabilityReplay(
            sealedReplay.Value);

        Assert.True(verified.IsSuccess, verified.ErrorMessage);
        Assert.True(verified.Value);
        Assert.False(tampered.IsSuccess);
        Assert.Equal(
            LogicalCapabilityTrainingAdmissionBatch
                .ProvenanceInvalidErrorCode,
            tampered.ErrorCode);
        Assert.False(rotated.IsSuccess);
        Assert.Equal(
            LogicalCapabilityTrainingAdmissionBatch
                .ReplayKeyMismatchErrorCode,
            rotated.ErrorCode);
    }

    [Fact]
    public void LogicalCapabilityReplay_LeaseSealsOnlyAdmittedCurrentSourceAndVerifiesBeforeMapping()
    {
        var legal = CreateLegalTeacherAuthority([], maxCalls: 0);
        var validationCount = 0;
        var server = ServerOwnedLegalTrainingTeacherAdmissionAuthority.Issue(
            provenance =>
            {
                validationCount++;
                return legal.ValidateBatch(provenance);
            },
            static () => true,
            sealReplay: legal.SealLogicalCapabilityReplay,
            verifyReplay: legal.VerifyLogicalCapabilityReplay);
        var source = CapabilityTrainingSeparationTests
            .CreateOpaqueSyntheticProvenance();
        var current = CapabilityTrainingSeparationTests
            .CreateOpaqueSyntheticCurrentProjection();
        var authorizedSourceBindings =
            new LogicalCapabilityTrainingSourceBindingComposite(
                [source.BindingSha256]);
        var currentBatch = DefaultLogicalCapabilityTrainer.BuildAdmissionBatch(
            "replay-seal-current",
            "PLAN",
            [new LogicalDpoPreferenceExample(
                current.Prompt,
                current.Chosen,
                current.Rejected,
                current.TeachingVector)
            {
                TrainingContentProvenance = source.Snapshot()
            }]);
        Assert.True(currentBatch.IsSuccess, currentBatch.ErrorMessage);
        var leaseResult =
            ((ILogicalCapabilityTrainingAdmissionAuthority)server)
            .AcquireTrainingLease(currentBatch.Value!);
        Assert.True(leaseResult.IsSuccess, leaseResult.ErrorMessage);
        using var lease = leaseResult.Value!;
        var descriptor = new LogicalCapabilityTrainingReplayDescriptor(
            LogicalCapabilityTrainingReplayDescriptor.CurrentSchemaVersion,
            LogicalTrainingContentSourceKind
                .DeterministicSyntheticNoExternalTeacher,
            source.ReplaySealGroupSha256!,
            authorizedSourceBindings,
            source.AuthorizedReplayPayloadSha256!,
            source.AuthorizedReplayDpoPreferenceSha256!);
        var sealedReplay = lease.SealReplay(descriptor);
        var foreignSeal = lease.SealReplay(descriptor with
        {
            SourceBindings =
                new LogicalCapabilityTrainingSourceBindingComposite(
                    [new string('e', 64)])
        });
        Assert.True(sealedReplay.IsSuccess, sealedReplay.ErrorMessage);
        Assert.False(foreignSeal.IsSuccess);

        var replayProjection =
            LogicalCapabilityTrainingPreferenceProjector
                .ProjectAuthorizedReplay(
                    [current],
                    authorizedSourceBindings.Values,
                    source.ReplaySealGroupSha256,
                    "PLAN",
                    new LogicalSubPhase(
                        "synthetic-subphase",
                        "synthetic-phase",
                        "synthetic name",
                        "synthetic purpose",
                        ["synthetic-domain"],
                        ["SYNTHETIC"],
                        []))
                .Value!;
        var replayPreference = new LogicalDpoPreferenceExample(
            replayProjection.TrainingText,
            replayProjection.OutputText,
            replayProjection.RejectedText,
            replayProjection.TeachingVector)
        {
            ReplayDescriptor = descriptor,
            ReplayAttestation = sealedReplay.Value
        };
        var replayBatch = DefaultLogicalCapabilityTrainer.BuildAdmissionBatch(
            "replay-admission",
            "PLAN",
            [replayPreference]);
        Assert.True(replayBatch.IsSuccess, replayBatch.ErrorMessage);
        using var replayLease =
            ((ILogicalCapabilityTrainingAdmissionAuthority)server)
            .AcquireTrainingLease(replayBatch.Value!).Value;
        Assert.NotNull(replayLease);
        Assert.Equal(2, validationCount);

        var tamperedBatch = DefaultLogicalCapabilityTrainer.BuildAdmissionBatch(
            "replay-tampered",
            "PLAN",
            [replayPreference with
            {
                ReplayAttestation = sealedReplay.Value! with
                {
                    MacSha256 = new string('f', 64)
                }
            }]);
        Assert.True(tamperedBatch.IsSuccess, tamperedBatch.ErrorMessage);
        var tamperedLease =
            ((ILogicalCapabilityTrainingAdmissionAuthority)server)
            .AcquireTrainingLease(tamperedBatch.Value!);
        Assert.False(tamperedLease.IsSuccess);
        Assert.Equal(2, validationCount);
    }

    [Theory]
    [InlineData("openai")]
    [InlineData("chatgpt")]
    [InlineData("gpt-5")]
    [InlineData("o1")]
    [InlineData("o3")]
    [InlineData("o4")]
    [InlineData("anthropic")]
    [InlineData("claude")]
    [InlineData("sonnet")]
    [InlineData("haiku")]
    [InlineData("opus")]
    [InlineData("google")]
    [InlineData("gemini")]
    [InlineData("xai")]
    [InlineData("grok")]
    [InlineData("copilot")]
    public void LegalTrainingTeacherAuthority_DeniesClosedAliasesRegardlessOfClaimedFlags(
        string prohibitedAlias)
    {
        var authority = CreateLegalTeacherAuthority([QwenPolicy()], maxCalls: 1);
        var provenance = QwenProvenance("closed-alias-request") with
        {
            ModelId = $"qwen-{prohibitedAlias}-spoof",
            ClosedProprietaryModel = false
        };

        var result = authority.ValidateBatch([provenance]);

        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ClosedProprietaryDeniedErrorCode,
            result.ErrorCode);
    }

    [Theory]
    [InlineData("qwen-jsonnet-9b")]
    [InlineData("qwen-sonnetry-9b")]
    public void LegalTrainingTeacherAuthority_AllowsNonIdentitySuperstringsAtTokenBoundaries(
        string modelId)
    {
        var authority = CreateLegalTeacherAuthority(
            [QwenPolicy(modelId: modelId)],
            maxCalls: 1);
        var provenance = QwenProvenance($"non-identity-superstring-{modelId}") with
        {
            ModelId = modelId
        };

        var result = authority.ValidateBatch([provenance]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(result.Value!.Receipts);
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_RequiresSelfHostedNoProviderTermsAndDeniesIdentitySpoofs()
    {
        var authority = CreateLegalTeacherAuthority([QwenPolicy()], maxCalls: 1);
        var allowed = QwenProvenance("teacher-request-terms");

        Assert.True(authority.ValidateBatch([allowed]).IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.TermsNotApprovedErrorCode,
            authority.ValidateBatch([allowed with { ProviderTermsChecked = true }]).ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.TermsNotApprovedErrorCode,
            authority.ValidateBatch([allowed with { ProviderTermsAllowTraining = true }]).ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.TermsNotApprovedErrorCode,
            authority.ValidateBatch([allowed with { TeacherOutputUsedForTraining = false }]).ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.NotAllowlistedErrorCode,
            authority.ValidateBatch([allowed with { ModelId = "Qwen3.5-9b" }]).ErrorCode);
        var malformedHosting = authority.ValidateBatch([allowed with
        {
            HostingProvider = TrainingTeacherHostingProvider.DashScope
        }]);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ProvenanceInvalidErrorCode,
            malformedHosting.ErrorCode);
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_EvaluationOnlyAcceptsExactLocalQwenAndTrainingAdmissionStillRejectsIt()
    {
        var authority = CreateLegalTeacherAuthority([QwenPolicy()], maxCalls: 1);
        var evaluationOnly = QwenProvenance("evaluation-only-request") with
        {
            TeacherOutputUsedForTraining = false
        };

        var evaluationAdmission = authority.ValidateLocalEvaluationOutput(evaluationOnly);
        var trainingAdmission = authority.ValidateBatch([evaluationOnly]);

        Assert.True(evaluationAdmission.IsSuccess, evaluationAdmission.ErrorMessage);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.TermsNotApprovedErrorCode,
            trainingAdmission.ErrorCode);
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_EvaluationOnlyRejectsTrainingUseAndEveryIdentityOrBudgetBypass()
    {
        var authority = CreateLegalTeacherAuthority([QwenPolicy()], maxCalls: 1);
        var allowed = QwenProvenance("evaluation-only-guard") with
        {
            TeacherOutputUsedForTraining = false
        };
        var rejected = new[]
        {
            allowed with { TeacherOutputUsedForTraining = true },
            allowed with { ClosedProprietaryModel = true },
            allowed with { Family = OpenWeightTeacherFamily.Mistral },
            allowed with { AccessMode = TrainingTeacherAccessMode.OnlineHostedOpenWeight },
            allowed with { HostingProvider = TrainingTeacherHostingProvider.DashScope },
            allowed with { ModelId = "qwen3.5-9b-spoof" },
            allowed with { ModelVersion = new string('e', 64) },
            allowed with { ModelLicense = TrainingTeacherModelLicense.MIT },
            allowed with { ProviderTermsChecked = true },
            allowed with { ProviderTermsAllowTraining = true },
            allowed with { RequestId = "unsafe\nrequest" },
            allowed with { InvocationCount = 0 },
            allowed with { InvocationCount = 2 },
            TrainingTeacherProvenance.Deterministic("evaluation-deterministic")
        };

        Assert.All(rejected, row =>
            Assert.False(authority.ValidateLocalEvaluationOutput(row).IsSuccess));
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_RejectsInvalidEnumsAndUnsafeIdentityFields()
    {
        var authority = CreateLegalTeacherAuthority([QwenPolicy()], maxCalls: 1);
        var allowed = QwenProvenance("safe-request");
        var unsafeRows = new[]
        {
            allowed with { SourceKind = (TrainingTeacherSourceKind)999 },
            allowed with { Family = (OpenWeightTeacherFamily)999 },
            allowed with { AccessMode = (TrainingTeacherAccessMode)999 },
            allowed with { HostingProvider = (TrainingTeacherHostingProvider)999 },
            allowed with { ModelLicense = (TrainingTeacherModelLicense)999 },
            allowed with { ModelId = " qwen3.5-9b" },
            allowed with { ModelId = "qwen3.5-9b\n" },
            allowed with { ModelId = new string('q', 257) },
            allowed with { ModelVersion = " " + QwenManifestDigest },
            allowed with { ModelVersion = "2026\t07" },
            allowed with { ModelVersion = new string('v', 257) },
            allowed with { RequestId = " request" },
            allowed with { RequestId = "request\r" },
            allowed with { RequestId = new string('r', 257) }
        };

        foreach (var row in unsafeRows)
        {
            var result = authority.ValidateBatch([row]);
            Assert.False(result.IsSuccess);
            Assert.Contains(
                result.ErrorCode,
                new[]
                {
                    LegalTrainingTeacherAdmissionAuthority.ProvenanceInvalidErrorCode,
                    LegalTrainingTeacherAdmissionAuthority.NotAllowlistedErrorCode
                });
        }
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_BoundsBatchBeforeRowValidation()
    {
        var authority = CreateLegalTeacherAuthority([], maxCalls: 0);
        var exact = Enumerable.Range(0, LegalTrainingTeacherAdmissionAuthority.MaxBatchRows)
            .Select(index => TrainingTeacherProvenance.Deterministic($"request-{index}"));
        var over = Enumerable.Range(0, LegalTrainingTeacherAdmissionAuthority.MaxBatchRows + 1)
            .Select(index => TrainingTeacherProvenance.Deterministic($"request-{index}"));

        Assert.True(authority.ValidateBatch(exact).IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.BatchLimitExceededErrorCode,
            authority.ValidateBatch(over).ErrorCode);
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_BoundsAndSnapshotsServerPolicy()
    {
        var entries = Enumerable.Range(0, LegalTrainingTeacherAdmissionAuthority.MaxAllowlistEntries)
            .Select(index => (LegalTrainingTeacherPolicyEntry?)QwenPolicy(
                modelId: $"qwen-policy-{index}"))
            .ToList();
        var salt = LegalTeacherSalt();
        var exact = LegalTrainingTeacherAdmissionAuthority.Create(entries, 1, salt);
        Assert.True(exact.IsSuccess, exact.ErrorMessage);

        entries.Add(QwenPolicy("qwen-policy-over"));
        salt[0] ^= 0xff;
        var admitted = exact.Value!.ValidateBatch([
            QwenProvenance("snapshot-request") with
            {
                ModelId = "qwen-policy-0",
                ModelVersion = QwenManifestDigest
            }]);
        Assert.True(admitted.IsSuccess, admitted.ErrorMessage);

        var over = LegalTrainingTeacherAdmissionAuthority.Create(entries, 1, LegalTeacherSalt());
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.AllowlistLimitExceededErrorCode,
            over.ErrorCode);
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_RejectsNullDuplicateAndInvalidPolicyEntries()
    {
        var entry = QwenPolicy();
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            LegalTrainingTeacherAdmissionAuthority.Create([], -1, LegalTeacherSalt()).ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            LegalTrainingTeacherAdmissionAuthority.Create([], 0, ReadOnlySpan<byte>.Empty).ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.EnumerationFailedErrorCode,
            LegalTrainingTeacherAdmissionAuthority.Create(null, 1, LegalTeacherSalt()).ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            LegalTrainingTeacherAdmissionAuthority.Create([null], 1, LegalTeacherSalt()).ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            LegalTrainingTeacherAdmissionAuthority.Create([entry, entry], 1, LegalTeacherSalt()).ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            LegalTrainingTeacherAdmissionAuthority.Create(
                [entry, entry with { OutputLicenseRef = HashRef('c') }],
                1,
                LegalTeacherSalt()).ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            LegalTrainingTeacherAdmissionAuthority.Create(
                [entry with { Family = (OpenWeightTeacherFamily)999 }],
                1,
                LegalTeacherSalt()).ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            LegalTrainingTeacherAdmissionAuthority.Create(
                [entry with { HostingProvider = TrainingTeacherHostingProvider.DeepSeek }],
                1,
                LegalTeacherSalt()).ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            LegalTrainingTeacherAdmissionAuthority.Create(
                [entry with { ModelId = "Qwen3.5-9b" }],
                1,
                LegalTeacherSalt()).ErrorCode);
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_DeduplicatesConsistentRequestsAndRejectsInconsistentOrOverflowingCounts()
    {
        var authority = CreateLegalTeacherAuthority([QwenPolicy()], maxCalls: 2);
        var first = QwenProvenance("same-request", invocationCount: 2);
        var second = QwenProvenance("second-request", invocationCount: 1);
        var overflowFirst = first with { InvocationCount = int.MaxValue };
        var overflowSecond = QwenProvenance("overflow-request", int.MaxValue);

        var duplicate = authority.ValidateBatch([first, first]);
        var inconsistent = authority.ValidateBatch([
            first,
            first with { InvocationCount = 1 }
        ]);
        var mixedSourceKinds = authority.ValidateBatch([
            first,
            TrainingTeacherProvenance.Deterministic("same-request")
        ]);
        var overBudget = authority.ValidateBatch([
            first,
            second
        ]);
        var overflow = CreateLegalTeacherAuthority([QwenPolicy()], maxCalls: 10_000)
            .ValidateBatch([
                overflowFirst,
                overflowSecond
            ]);

        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.DuplicateRequestIdErrorCode,
            duplicate.ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.DuplicateRequestIdErrorCode,
            inconsistent.ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.DuplicateRequestIdErrorCode,
            mixedSourceKinds.ErrorCode);
        Assert.NotEqual(first.RequestId, second.RequestId);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.CallBudgetExceededErrorCode,
            overBudget.ErrorCode);
        Assert.NotEqual(overflowFirst.RequestId, overflowSecond.RequestId);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.CallBudgetExceededErrorCode,
            overflow.ErrorCode);
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_AdmissionCollectionsAreImmutableOwnedSnapshots()
    {
        var authority = CreateLegalTeacherAuthority([], maxCalls: 0);
        var source = new List<TrainingTeacherProvenance?>
        {
            TrainingTeacherProvenance.Deterministic("immutable-request")
        };
        var result = authority.ValidateBatch(source);
        source.Clear();

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var receipts = Assert.IsAssignableFrom<IList<LegalTrainingTeacherAdmissionReceipt>>(
            result.Value!.Receipts);
        var evidence = Assert.IsAssignableFrom<IList<string>>(result.Value.EvidenceRefs);
        Assert.True(receipts.IsReadOnly);
        Assert.True(evidence.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => receipts.Clear());
        Assert.Throws<NotSupportedException>(() => evidence.Clear());
        Assert.Single(result.Value.Receipts);
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_OrdinaryEnumerationFailsClosedButFatalGraphsEscape()
    {
        var ordinary = LegalTrainingTeacherAdmissionAuthority.Create(
            ThrowDuringEnumeration<LegalTrainingTeacherPolicyEntry?>(new InvalidOperationException("ordinary")),
            1,
            LegalTeacherSalt());
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.EnumerationFailedErrorCode,
            ordinary.ErrorCode);

        Assert.Throws<OperationCanceledException>(() =>
            LegalTrainingTeacherAdmissionAuthority.Create(
                ThrowDuringEnumeration<LegalTrainingTeacherPolicyEntry?>(new OperationCanceledException()),
                1,
                LegalTeacherSalt()));
        Assert.Throws<AggregateException>(() =>
            LegalTrainingTeacherAdmissionAuthority.Create(
                ThrowDuringEnumeration<LegalTrainingTeacherPolicyEntry?>(
                    new AggregateException(new InvalidOperationException(), new OperationCanceledException())),
                1,
                LegalTeacherSalt()));
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_BoundedEnumerablesUseExactlyOneLookahead()
    {
        var exactAllowlist = new MoveNextProbeEnumerable<LegalTrainingTeacherPolicyEntry?>(
            Enumerable.Range(0, LegalTrainingTeacherAdmissionAuthority.MaxAllowlistEntries)
                .Select(index => (LegalTrainingTeacherPolicyEntry?)QwenPolicy(
                    $"qwen3-policy-{index}")));
        var exactAuthority = LegalTrainingTeacherAdmissionAuthority.Create(
            exactAllowlist,
            0,
            LegalTeacherSalt());
        Assert.True(exactAuthority.IsSuccess, exactAuthority.ErrorMessage);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.MaxAllowlistEntries + 1,
            exactAllowlist.MoveNextCallCount);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.MaxAllowlistEntries,
            exactAllowlist.YieldedCount);

        var overAllowlist = new MoveNextProbeEnumerable<LegalTrainingTeacherPolicyEntry?>(
            Enumerable.Range(0, LegalTrainingTeacherAdmissionAuthority.MaxAllowlistEntries + 1)
                .Select(index => (LegalTrainingTeacherPolicyEntry?)QwenPolicy(
                    $"qwen3-policy-{index}")));
        var overAuthority = LegalTrainingTeacherAdmissionAuthority.Create(
            overAllowlist,
            0,
            LegalTeacherSalt());
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.AllowlistLimitExceededErrorCode,
            overAuthority.ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.MaxAllowlistEntries + 1,
            overAllowlist.MoveNextCallCount);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.MaxAllowlistEntries + 1,
            overAllowlist.YieldedCount);

        var deterministicAuthority = CreateLegalTeacherAuthority([], maxCalls: 0);
        var exactBatch = new MoveNextProbeEnumerable<TrainingTeacherProvenance?>(
            Enumerable.Range(0, LegalTrainingTeacherAdmissionAuthority.MaxBatchRows)
                .Select(index => (TrainingTeacherProvenance?)TrainingTeacherProvenance.Deterministic(
                    $"bounded-request-{index}")));
        var exactAdmission = deterministicAuthority.ValidateBatch(exactBatch);
        Assert.True(exactAdmission.IsSuccess, exactAdmission.ErrorMessage);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.MaxBatchRows + 1,
            exactBatch.MoveNextCallCount);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.MaxBatchRows,
            exactBatch.YieldedCount);

        var overBatch = new MoveNextProbeEnumerable<TrainingTeacherProvenance?>(
            Enumerable.Range(0, LegalTrainingTeacherAdmissionAuthority.MaxBatchRows + 1)
                .Select(index => (TrainingTeacherProvenance?)TrainingTeacherProvenance.Deterministic(
                    $"bounded-request-{index}")));
        var overAdmission = deterministicAuthority.ValidateBatch(overBatch);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.BatchLimitExceededErrorCode,
            overAdmission.ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.MaxBatchRows + 1,
            overBatch.MoveNextCallCount);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.MaxBatchRows + 1,
            overBatch.YieldedCount);
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_ValidateBatchEnumerationFailsClosedButCancellationAndFatalGraphsEscape()
    {
        var authority = CreateLegalTeacherAuthority([], maxCalls: 0);
        var row = (TrainingTeacherProvenance?)TrainingTeacherProvenance.Deterministic(
            "enumeration-request");

        var ordinary = authority.ValidateBatch(
            new MoveNextProbeEnumerable<TrainingTeacherProvenance?>(
                [row],
                throwOnMoveNextCall: 2,
                new InvalidOperationException("ordinary-enumeration")));
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.EnumerationFailedErrorCode,
            ordinary.ErrorCode);

        Assert.Throws<OperationCanceledException>(() => authority.ValidateBatch(
            new MoveNextProbeEnumerable<TrainingTeacherProvenance?>(
                [row],
                throwOnMoveNextCall: 2,
                new OperationCanceledException("cancelled-enumeration"))));
        Assert.Throws<AggregateException>(() => authority.ValidateBatch(
            new MoveNextProbeEnumerable<TrainingTeacherProvenance?>(
                [row],
                throwOnMoveNextCall: 2,
                new AggregateException(
                    new InvalidOperationException(
                        "outer",
                        new OperationCanceledException("nested-cancellation"))))));
        Assert.Throws<AggregateException>(() => authority.ValidateBatch(
            new MoveNextProbeEnumerable<TrainingTeacherProvenance?>(
                [row],
                throwOnMoveNextCall: 2,
                new AggregateException(
                    new InvalidOperationException(
                        "outer",
                        new OutOfMemoryException("nested-fatal"))))));
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_ClosedAliasesAreDeniedInModelAndVersionRegardlessOfCase()
    {
        var authority = CreateLegalTeacherAuthority([QwenPolicy()], maxCalls: 1);
        var allowed = QwenProvenance("alias-matrix-request");
        var aliases = new[]
        {
            "openai", "chatgpt", "gpt-", "o1", "o3", "o4",
            "anthropic", "claude", "sonnet", "haiku", "opus",
            "google", "gemini", "xai", "grok", "copilot"
        };

        foreach (var alias in aliases)
        {
            var mixedCaseAlias = new string(alias
                .Select((character, index) => index % 2 == 0
                    ? char.ToUpperInvariant(character)
                    : char.ToLowerInvariant(character))
                .ToArray());
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.ClosedProprietaryDeniedErrorCode,
                authority.ValidateBatch([
                    allowed with { ModelId = $"qwen-{mixedCaseAlias}-spoof" }
                ]).ErrorCode);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.ProvenanceInvalidErrorCode,
                authority.ValidateBatch([
                    allowed with { ModelVersion = $"release-{mixedCaseAlias}-2026" }
                ]).ErrorCode);
        }
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_RequiresCanonicalPrefixAndExactProviderAccessLicenseFamily()
    {
        var qwen3Policy = QwenPolicy("qwen3");
        var authority = CreateLegalTeacherAuthority([qwen3Policy], maxCalls: 1);
        var allowed = QwenProvenance("canonical-request") with
        {
            ModelId = qwen3Policy.ModelId,
            ModelVersion = qwen3Policy.ModelVersion
        };
        Assert.True(authority.ValidateBatch([allowed]).IsSuccess);

        foreach (var invalidModelId in new[] { "qwenx", "model-qwen3", "Qwen3" })
        {
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
                LegalTrainingTeacherAdmissionAuthority.Create(
                    [QwenPolicy(invalidModelId)],
                    1,
                    LegalTeacherSalt()).ErrorCode);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.NotAllowlistedErrorCode,
                authority.ValidateBatch([allowed with { ModelId = invalidModelId }]).ErrorCode);
        }

        var invalidPolicies = new[]
        {
            qwen3Policy with { Family = OpenWeightTeacherFamily.DeepSeek },
            qwen3Policy with { HostingProvider = TrainingTeacherHostingProvider.DeepSeek },
            qwen3Policy with
            {
                AccessMode = TrainingTeacherAccessMode.OnlineHostedOpenWeight,
                HostingProvider = TrainingTeacherHostingProvider.Ollama
            },
            qwen3Policy with
            {
                AccessMode = TrainingTeacherAccessMode.LocalHostedOpenWeight,
                HostingProvider = TrainingTeacherHostingProvider.DashScope
            },
            qwen3Policy with { ModelLicense = TrainingTeacherModelLicense.MIT }
        };
        Assert.All(invalidPolicies, invalidPolicy => Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            LegalTrainingTeacherAdmissionAuthority.Create(
                [invalidPolicy],
                1,
                LegalTeacherSalt()).ErrorCode));

        var notAllowlistedRows = new[]
        {
            allowed with { Family = OpenWeightTeacherFamily.DeepSeek },
            allowed with { ModelLicense = TrainingTeacherModelLicense.MIT }
        };
        Assert.All(notAllowlistedRows, invalidRow => Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.NotAllowlistedErrorCode,
            authority.ValidateBatch([invalidRow]).ErrorCode));
        var invalidRows = new[]
        {
            allowed with { HostingProvider = TrainingTeacherHostingProvider.DeepSeek },
            allowed with
            {
                AccessMode = TrainingTeacherAccessMode.OnlineHostedOpenWeight,
                HostingProvider = TrainingTeacherHostingProvider.DashScope
            }
        };
        Assert.All(invalidRows, invalidRow => Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ProvenanceInvalidErrorCode,
            authority.ValidateBatch([invalidRow]).ErrorCode));
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_OwnsCompletePolicyAndSaltSnapshots()
    {
        var originalPolicy = QwenPolicy();
        var mutableEntries = new List<LegalTrainingTeacherPolicyEntry?> { originalPolicy };
        var mutableSalt = LegalTeacherSalt();
        var untouchedSalt = mutableSalt.ToArray();
        var mutatedAuthority = LegalTrainingTeacherAdmissionAuthority.Create(
            mutableEntries,
            1,
            mutableSalt);
        var untouchedAuthority = LegalTrainingTeacherAdmissionAuthority.Create(
            [originalPolicy],
            1,
            untouchedSalt);
        Assert.True(mutatedAuthority.IsSuccess, mutatedAuthority.ErrorMessage);
        Assert.True(untouchedAuthority.IsSuccess, untouchedAuthority.ErrorMessage);

        mutableEntries[0] = QwenPolicy("qwen3-replaced");
        mutableEntries.RemoveAt(0);
        mutableEntries.Clear();
        for (var index = 0; index < mutableSalt.Length; index++)
            mutableSalt[index] ^= 0xff;

        var provenance = QwenProvenance("immutable-authority-request");
        var mutatedAdmission = mutatedAuthority.Value!.ValidateBatch([provenance]);
        var untouchedAdmission = untouchedAuthority.Value!.ValidateBatch([provenance]);
        Assert.True(mutatedAdmission.IsSuccess, mutatedAdmission.ErrorMessage);
        Assert.True(untouchedAdmission.IsSuccess, untouchedAdmission.ErrorMessage);
        Assert.Equal(
            untouchedAuthority.Value.PolicyIdentityRef,
            mutatedAuthority.Value.PolicyIdentityRef);
        Assert.Equal(
            JsonSerializer.Serialize(untouchedAdmission.Value),
            JsonSerializer.Serialize(mutatedAdmission.Value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("pending")]
    [InlineData("mock")]
    [InlineData("C:/legal/retention.txt")]
    [InlineData("https://example.invalid/legal")]
    [InlineData("sha256:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("sha256:gggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggg")]
    public void LegalTrainingTeacherAuthority_RejectsNonCanonicalServerEvidenceRefs(
        string invalidRef)
    {
        var retentionPolicy = QwenPolicy() with { LocalRetentionPolicyRef = invalidRef };
        var outputPolicy = QwenPolicy() with { OutputLicenseRef = invalidRef };

        var retentionResult = LegalTrainingTeacherAdmissionAuthority.Create(
            [retentionPolicy],
            1,
            LegalTeacherSalt());
        var outputResult = LegalTrainingTeacherAdmissionAuthority.Create(
            [outputPolicy],
            1,
            LegalTeacherSalt());

        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            retentionResult.ErrorCode);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            outputResult.ErrorCode);
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_ServerEvidenceControlsReceiptAndPolicyIdentity()
    {
        var firstPolicy = QwenPolicy();
        var secondPolicy = firstPolicy with { OutputLicenseRef = HashRef('c') };
        var thirdPolicy = firstPolicy with { LocalRetentionPolicyRef = HashRef('d') };
        var fourthPolicy = QwenPolicy(layerDigest: new string('e', 64));
        var first = CreateLegalTeacherAuthority([firstPolicy], maxCalls: 1);
        var second = CreateLegalTeacherAuthority([secondPolicy], maxCalls: 1);
        var third = CreateLegalTeacherAuthority([thirdPolicy], maxCalls: 1);
        var fourth = CreateLegalTeacherAuthority([fourthPolicy], maxCalls: 1);
        var mutablePolicies = new List<LegalTrainingTeacherPolicyEntry?> { firstPolicy };
        var snapshotted = CreateLegalTeacherAuthority(mutablePolicies, maxCalls: 1);
        mutablePolicies[0] = secondPolicy;
        mutablePolicies.Clear();

        var admission = snapshotted.ValidateBatch([QwenProvenance("server-evidence")]);

        Assert.True(admission.IsSuccess, admission.ErrorMessage);
        Assert.NotEqual(first.PolicyIdentityRef, second.PolicyIdentityRef);
        Assert.NotEqual(first.PolicyIdentityRef, third.PolicyIdentityRef);
        Assert.NotEqual(first.PolicyIdentityRef, fourth.PolicyIdentityRef);
        var receipt = Assert.Single(admission.Value!.Receipts);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.SelfHostedNoProviderTermsEvidenceMode,
            receipt.LegalEvidenceMode);
        Assert.Equal(firstPolicy.LocalRetentionPolicyRef, receipt.LocalRetentionPolicyRef);
        Assert.Equal(firstPolicy.OutputLicenseRef, receipt.OutputLicenseRef);
        Assert.False(receipt.ProviderTermsChecked);
        Assert.False(receipt.ProviderTermsAllowTraining);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
            receipt.ModelArtifactDigestKind);
        Assert.True(
            LegalTrainingTeacherAdmissionAuthority.IsCanonicalEvidenceRef(
                receipt.ModelArtifactRef));
        Assert.DoesNotContain(QwenModelLayerDigest, JsonSerializer.Serialize(admission.Value));
        Assert.Contains(
            $"teacher_legal_evidence:terms_snapshot_ref={firstPolicy.LocalRetentionPolicyRef}",
            admission.Value.EvidenceRefs);
        Assert.DoesNotContain(secondPolicy.OutputLicenseRef, JsonSerializer.Serialize(admission.Value));
        Assert.DoesNotContain(
            typeof(TrainingTeacherProvenance).GetProperties(),
            property => property.Name.Contains("Retention", StringComparison.Ordinal)
                        || property.Name.Contains("EvidenceRef", StringComparison.Ordinal)
                        || property.Name.Contains("OutputLicense", StringComparison.Ordinal));
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_LocalArtifactIdentityFailsClosedByTaxonomy()
    {
        var authority = CreateLegalTeacherAuthority([QwenPolicy()], maxCalls: 1);
        var training = QwenProvenance("artifact-taxonomy");
        var evaluation = training with { TeacherOutputUsedForTraining = false };
        var malformedTraining = new[]
        {
            training with { ModelVersion = new string('C', 64) },
            training with { ModelVersion = new string('g', 64) },
            training with { ModelVersion = new string('c', 63) },
            training with { ModelVersion = new string('c', 65) },
            training with { ModelArtifactDigestKind = string.Empty },
            training with { ModelArtifactDigest = string.Empty },
            training with { ModelArtifactDigestKind = "ollama_manifest_sha256" },
            training with { ModelArtifactDigest = new string('D', 64) },
            training with { ModelArtifactDigest = new string('g', 64) },
            training with { ModelArtifactDigest = new string('d', 63) },
            training with { ModelArtifactDigest = new string('d', 65) },
            training with
            {
                AccessMode = TrainingTeacherAccessMode.OnlineHostedOpenWeight,
                HostingProvider = TrainingTeacherHostingProvider.DashScope
            }
        };

        foreach (var malformed in malformedTraining)
        {
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.ProvenanceInvalidErrorCode,
                authority.ValidateBatch([malformed]).ErrorCode);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.ProvenanceInvalidErrorCode,
                authority.ValidateLocalEvaluationOutput(
                    malformed with { TeacherOutputUsedForTraining = false }).ErrorCode);
        }

        foreach (var canonicalMismatch in new[]
        {
            training with { ModelVersion = new string('e', 64) },
            training with { ModelArtifactDigest = new string('e', 64) },
        })
        {
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.NotAllowlistedErrorCode,
                authority.ValidateBatch([canonicalMismatch]).ErrorCode);
            Assert.Equal(
                LegalTrainingTeacherAdmissionAuthority.NotAllowlistedErrorCode,
                authority.ValidateLocalEvaluationOutput(
                    canonicalMismatch with { TeacherOutputUsedForTraining = false }).ErrorCode);
        }

        var malformedPolicies = new[]
        {
            QwenPolicy(version: new string('C', 64)),
            QwenPolicy(version: new string('g', 64)),
            QwenPolicy(version: new string('c', 63)),
            QwenPolicy(version: new string('c', 65)),
            QwenPolicy() with { ModelArtifactDigestKind = "ollama_manifest_sha256" },
            QwenPolicy(layerDigest: new string('D', 64)),
            QwenPolicy(layerDigest: new string('g', 64)),
            QwenPolicy(layerDigest: new string('d', 63)),
            QwenPolicy(layerDigest: new string('d', 65)),
        };
        Assert.All(malformedPolicies, policy => Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            LegalTrainingTeacherAdmissionAuthority.Create(
                [policy],
                1,
                LegalTeacherSalt()).ErrorCode));

        Assert.True(authority.ValidateLocalEvaluationOutput(evaluation).IsSuccess);
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_DeterministicReceiptHasNoExternalLegalEvidenceRefs()
    {
        var authority = CreateLegalTeacherAuthority([], maxCalls: 0);

        var admission = authority.ValidateBatch([
            TrainingTeacherProvenance.Deterministic("deterministic-no-evidence")]);

        Assert.True(admission.IsSuccess, admission.ErrorMessage);
        var receipt = Assert.Single(admission.Value!.Receipts);
        Assert.Null(receipt.LegalEvidenceMode);
        Assert.Null(receipt.LocalRetentionPolicyRef);
        Assert.Null(receipt.OutputLicenseRef);
        Assert.DoesNotContain(
            admission.Value.EvidenceRefs,
            value => value.StartsWith("teacher_legal_evidence:", StringComparison.Ordinal));
    }

    [Fact]
    public void LegalTrainingTeacherAuthority_LegacyPolicyAndReceiptShapesRemainButCannotSupplyEvidence()
    {
        var legacyPolicy = new LegalTrainingTeacherPolicyEntry(
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            "qwen3.5-9b",
            "2026-07-31",
            TrainingTeacherModelLicense.Apache20);
        var (
            family,
            accessMode,
            hostingProvider,
            modelId,
            modelVersion,
            modelLicense) = legacyPolicy;

        var authority = LegalTrainingTeacherAdmissionAuthority.Create(
            [legacyPolicy],
            1,
            LegalTeacherSalt());

        Assert.Equal(OpenWeightTeacherFamily.Qwen, family);
        Assert.Equal(TrainingTeacherAccessMode.LocalHostedOpenWeight, accessMode);
        Assert.Equal(TrainingTeacherHostingProvider.Ollama, hostingProvider);
        Assert.Equal("qwen3.5-9b", modelId);
        Assert.Equal("2026-07-31", modelVersion);
        Assert.Equal(TrainingTeacherModelLicense.Apache20, modelLicense);
        Assert.Equal(string.Empty, legacyPolicy.LocalRetentionPolicyRef);
        Assert.Equal(string.Empty, legacyPolicy.OutputLicenseRef);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            authority.ErrorCode);
        Assert.Contains(
            typeof(LegalTrainingTeacherPolicyEntry).GetConstructors(),
            constructor => constructor.GetParameters().Length == 6);
        var legacyPolicyDeconstruct = Assert.Single(
            typeof(LegalTrainingTeacherPolicyEntry).GetMethods(),
            method => method.Name == "Deconstruct" && method.GetParameters().Length == 6);
        Assert.Equal(
            ["Family", "AccessMode", "HostingProvider", "ModelId", "ModelVersion", "ModelLicense"],
            legacyPolicyDeconstruct.GetParameters().Select(parameter => parameter.Name!));

        var legacyReceipt = new LegalTrainingTeacherAdmissionReceipt(
            TrainingTeacherSourceKind.DeterministicNoExternalTeacher,
            Family: null,
            AccessMode: null,
            HostingProvider: null,
            ModelLicense: null,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: false,
            InvocationCount: 0,
            RequestRef: HashRef('1'),
            ProviderRef: HashRef('2'),
            ModelRef: HashRef('3'),
            ModelVersionRef: HashRef('4'),
            LicenseRef: HashRef('5'));
        var (
            sourceKind,
            _,
            _,
            _,
            _,
            _,
            _,
            _,
            _,
            invocationCount,
            requestRef,
            _,
            _,
            _,
            _) = legacyReceipt;

        Assert.Equal(TrainingTeacherSourceKind.DeterministicNoExternalTeacher, sourceKind);
        Assert.Equal(0, invocationCount);
        Assert.Equal(HashRef('1'), requestRef);
        Assert.Null(legacyReceipt.LegalEvidenceMode);
        Assert.Null(legacyReceipt.LocalRetentionPolicyRef);
        Assert.Null(legacyReceipt.OutputLicenseRef);
        Assert.Contains(
            typeof(LegalTrainingTeacherAdmissionReceipt).GetConstructors(),
            constructor => constructor.GetParameters().Length == 15);
        var legacyReceiptDeconstruct = Assert.Single(
            typeof(LegalTrainingTeacherAdmissionReceipt).GetMethods(),
            method => method.Name == "Deconstruct" && method.GetParameters().Length == 15);
        Assert.Equal(
            [
                "SourceKind",
                "Family",
                "AccessMode",
                "HostingProvider",
                "ModelLicense",
                "ProviderTermsChecked",
                "ProviderTermsAllowTraining",
                "ClosedProprietaryModel",
                "TeacherOutputUsedForTraining",
                "InvocationCount",
                "RequestRef",
                "ProviderRef",
                "ModelRef",
                "ModelVersionRef",
                "LicenseRef",
            ],
            legacyReceiptDeconstruct.GetParameters().Select(parameter => parameter.Name!));
    }

    private static LegalTrainingTeacherAdmissionAuthority CreateLegalTeacherAuthority(
        IEnumerable<LegalTrainingTeacherPolicyEntry?> entries,
        int maxCalls)
    {
        var result = LegalTrainingTeacherAdmissionAuthority.Create(entries, maxCalls, LegalTeacherSalt());
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!;
    }

    private static LegalTrainingTeacherPolicyEntry QwenPolicy(
        string modelId = "qwen3.5-9b",
        string? version = null,
        string? layerDigest = null)
        => new(
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            modelId,
            version ?? QwenManifestDigest,
            TrainingTeacherModelLicense.Apache20,
            HashRef('a'),
            HashRef('b'))
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = layerDigest ?? QwenModelLayerDigest
        };

    private static TrainingTeacherProvenance QwenProvenance(
        string requestId,
        int invocationCount = 1)
        => new(
            TrainingTeacherSourceKind.ExternalTeacherOutput,
            OpenWeightTeacherFamily.Qwen,
            TrainingTeacherAccessMode.LocalHostedOpenWeight,
            TrainingTeacherHostingProvider.Ollama,
            "qwen3.5-9b",
            QwenManifestDigest,
            TrainingTeacherModelLicense.Apache20,
            ProviderTermsChecked: false,
            ProviderTermsAllowTraining: false,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            requestId,
            invocationCount)
        {
            ModelArtifactDigestKind =
                LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
            ModelArtifactDigest = QwenModelLayerDigest
        };

    private static readonly string QwenManifestDigest = new('c', 64);
    private static readonly string QwenModelLayerDigest = new('d', 64);

    private static byte[] LegalTeacherSalt()
        => Enumerable.Range(1, 32).Select(static value => (byte)value).ToArray();

    private static string HashRef(char value)
        => $"sha256:{new string(value, 64)}";

    private static string IndependentHmacRef(
        byte[] salt,
        string scope,
        string value)
    {
        using var hmac = new HMACSHA256(salt);
        var payload = Encoding.UTF8.GetBytes($"{scope}\0{value}");
        return $"sha256:{Convert.ToHexString(hmac.ComputeHash(payload)).ToLowerInvariant()}";
    }

    private static IEnumerable<T> ThrowDuringEnumeration<T>(Exception exception)
    {
        if (DateTimeOffset.UtcNow == DateTimeOffset.MinValue)
            yield return default!;
        throw exception;
    }

    // ── Phase 1: Core types ───────────────────────────────────────────────────

    [Fact]
    public void ContinuousLearning_TrainingExample_StoresDomainAndWeight()
    {
        // Gate B: Domain and Weight round-trip unchanged through record construction.
        var ex = new TrainingExample("fix null", "use ?.", "BUG", 1.5, DateTimeOffset.UtcNow);

        Assert.Equal("BUG", ex.Domain);
        Assert.Equal(1.5, ex.Weight);
    }

    [Fact]
    public void ContinuousLearning_DpoPair_StoresChosenAndRejected()
    {
        // Gate B: Chosen and Rejected fields preserved exactly as given.
        var pair = new DpoPair("prompt", "good", "bad", "CODE_Q", DateTimeOffset.UtcNow);

        Assert.Equal("good", pair.Chosen);
        Assert.Equal("bad", pair.Rejected);
    }

    [Fact]
    public void ContinuousLearning_FeedbackSignal_HasThreeValues()
    {
        // Gate B: enum has exactly 3 values — Accepted, Edited, Rejected.
        Assert.Equal(3, Enum.GetValues<FeedbackSignal>().Length);
    }

    // ── Phase 2: ProjectIndexer ───────────────────────────────────────────────

    [Fact]
    public void ProjectIndexer_EmptyDirectory_ReturnsEmpty()
    {
        // Gate B: no files listed → IsSuccess=true, empty Value.
        var indexer = new ProjectIndexer(Storage, new StubSC29CompilerCL(SC29.Domain.CODE_Q), new EmptyCodeReaderCL());
        var result  = indexer.Index("/repo");

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public void ProjectIndexer_CSharpFile_ProducesExampleWithCodeQDomain()
    {
        // Gate B: one .cs file → one example with domain CODE_Q from stub compiler.
        var reader  = new SingleFileCodeReaderCL("UserService.cs", "public class UserService { }");
        var indexer = new ProjectIndexer(Storage, new StubSC29CompilerCL(SC29.Domain.CODE_Q), reader);
        var result  = indexer.Index("/repo");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal("CODE_Q", result.Value![0].Domain);
    }

    [Fact]
    public void ProjectIndexer_ISemanticCompilerInjected_NotCopied()
    {
        // Gate B: constructor has an ISemanticCompiler29 parameter — no reimplementation.
        var ctor       = typeof(ProjectIndexer).GetConstructors()[0];
        var hasCompiler = ctor.GetParameters()
            .Any(p => p.ParameterType == typeof(SC29.ISemanticCompiler29));

        Assert.True(hasCompiler, "ProjectIndexer must accept ISemanticCompiler29 via constructor.");
    }

    // ── Phase 2: DomainDatasetCache ───────────────────────────────────────────

    [Fact]
    public void DomainDatasetCache_Add_IncrementsTotalCount()
    {
        // Gate B: adding one example makes TotalCount return 1.
        var cache = BuildCache();
        var ex    = new TrainingExample("fix null", "if (x == null)", "BUG", 1.0, DateTimeOffset.UtcNow);

        cache.Add(ex);

        Assert.Equal(1, cache.TotalCount().Value!);
    }

    [Fact]
    public void DomainDatasetCache_GetByDomain_ReturnsOnlyMatchingDomain()
    {
        // Gate B: GetByDomain("BUG") returns only BUG examples, not CODE_Q.
        var cache = BuildCache();
        cache.Add(new TrainingExample("p1", "r1", "BUG",    1.0, DateTimeOffset.UtcNow));
        cache.Add(new TrainingExample("p2", "r2", "CODE_Q", 1.0, DateTimeOffset.UtcNow));

        var bugResults = cache.GetByDomain("BUG").Value!;

        Assert.Single(bugResults);
        Assert.Equal("BUG", bugResults[0].Domain);
    }

    [Fact]
    public void DomainDatasetCache_TotalCount_SumsAcrossDomains()
    {
        // Gate B: 2 BUG + 1 CODE_Q = total 3.
        var cache = BuildCache();
        cache.Add(new TrainingExample("p1", "r1", "BUG",    1.0, DateTimeOffset.UtcNow));
        cache.Add(new TrainingExample("p2", "r2", "BUG",    1.0, DateTimeOffset.UtcNow));
        cache.Add(new TrainingExample("p3", "r3", "CODE_Q", 1.0, DateTimeOffset.UtcNow));

        Assert.Equal(3, cache.TotalCount().Value!);
    }

    // ── Phase 3: FeedbackRouter ───────────────────────────────────────────────

    [Fact]
    public void FeedbackRouter_Accepted_AddsPositiveExample()
    {
        // Gate B: ACCEPTED signal → cache gains one example.
        var cache  = BuildCache();
        var router = BuildFeedbackRouter(cache);
        var ev     = new FeedbackEvent("fix null", "if (x == null) throw", null, FeedbackSignal.Accepted, "CODE_Q");

        router.Route(ev);

        Assert.Equal(1, cache.TotalCount().Value!);
    }

    [Fact]
    public void FeedbackRouter_Edited_CreatesDpoPair_WithUserCorrectionAsChosen()
    {
        // Gate B: EDITED signal → DPO pair with UserCorrection as Chosen.
        var cache  = BuildCache();
        var router = BuildFeedbackRouter(cache);
        var requestId = new string('1', 64);
        var governance = new TrainingDataGovernanceDecision(
            licenseAccepted: true,
            isQuarantined: false,
            [HashRef('a')]);
        var ev = new FeedbackEvent(
            "fix null",
            "return null;",
            "return Optional.empty();",
            FeedbackSignal.Edited,
            "CODE_Q")
        {
            TeacherProvenance = TrainingTeacherProvenance.Deterministic(requestId),
            GovernanceDecision = governance
        };

        var routed = router.Route(ev);
        var readback = router.GetDpoPairs();

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.True(routed.Value);
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        var pair = Assert.Single(readback.Value!);
        Assert.Equal("fix null", pair.Prompt);
        Assert.Equal("return Optional.empty();", pair.Chosen);
        Assert.Equal("return null;", pair.Rejected);
        Assert.Equal("CODE_Q", pair.Domain);
        Assert.Equal(requestId, pair.TeacherProvenance!.RequestId);
        Assert.False(string.IsNullOrWhiteSpace(
            pair.TeacherProvenance.TrainingContentSha256));
        Assert.Equal(governance, pair.GovernanceDecision);
        Assert.Equal(1, cache.TotalCount().Value!);
    }

    [Fact]
    public void FeedbackRouter_Rejected_CreatesDpoPair_WithNoPreferredResponse()
    {
        // Gate B: REJECTED signal → DPO pair with "[no preferred response]" as Chosen.
        var cache  = BuildCache();
        var router = BuildFeedbackRouter(cache);
        var requestId = new string('2', 64);
        var governance = new TrainingDataGovernanceDecision(
            licenseAccepted: true,
            isQuarantined: false,
            [HashRef('b')]);
        var ev = new FeedbackEvent(
            "fix null",
            "return null;",
            null,
            FeedbackSignal.Rejected,
            "BUG")
        {
            TeacherProvenance = TrainingTeacherProvenance.Deterministic(requestId),
            GovernanceDecision = governance
        };

        var routed = router.Route(ev);
        var readback = router.GetDpoPairs();

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.True(routed.Value);
        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        var pair = Assert.Single(readback.Value!);
        Assert.Equal("fix null", pair.Prompt);
        Assert.Equal("[no preferred response]", pair.Chosen);
        Assert.Equal("return null;", pair.Rejected);
        Assert.Equal("BUG", pair.Domain);
        Assert.Equal(requestId, pair.TeacherProvenance!.RequestId);
        Assert.False(string.IsNullOrWhiteSpace(
            pair.TeacherProvenance.TrainingContentSha256));
        Assert.Equal(governance, pair.GovernanceDecision);
        Assert.Equal(0, cache.TotalCount().Value!);
    }

    [Fact]
    public void FeedbackRouter_AcceptedSignalsOnly_DoNotTriggerLearningEventScheduler()
    {
        // Gate B: ACCEPTED signals create positive examples, not DPO pairs.
        // The idle scheduler receives DPO count, so accepted-only feedback must not queue training.
        var cache = BuildCache();
        var router = BuildFeedbackRouter(cache);
        var scheduler = new LearningEventScheduler(() => true, minimumExamples: 5);

        for (int i = 0; i < 5; i++)
        {
            var routed = router.Route(new FeedbackEvent(
                $"accepted prompt {i}",
                $"accepted response {i}",
                null,
                FeedbackSignal.Accepted,
                "BUG"));
            Assert.True(routed.IsSuccess, routed.ErrorMessage);
        }

        var dpoPairs = router.GetDpoPairs();
        Assert.True(dpoPairs.IsSuccess, dpoPairs.ErrorMessage);
        Assert.Empty(dpoPairs.Value!);
        Assert.Equal(5, cache.TotalCount().Value!);

        var queued = scheduler.QueueIfReady("BUG", dpoPairs.Value!.Count, "accepted feedback only");

        Assert.True(queued.IsSuccess, queued.ErrorMessage);
        Assert.False(queued.Value);
        Assert.Equal(0, scheduler.QueuedCount);
    }

    [Fact]
    public void FeedbackRouter_ComputeMaxWeight_Formula_1000_5_50_Returns4()
    {
        // Gate B (D-09): 1000 examples / (5 domains × 50 rare) = 4.0 — not a hardcoded ×10.
        Assert.Equal(4.0, FeedbackRouter.ComputeMaxWeight(1000, 5, 50));
    }

    [Fact]
    public void FeedbackRouter_ComputeMaxWeight_ZeroRare_Returns1()
    {
        // Gate B: rareExamples=0 guard → returns 1.0 to avoid division by zero.
        Assert.Equal(1.0, FeedbackRouter.ComputeMaxWeight(100, 5, 0));
    }

    // ── Phase 4: IncrementalTrainer ───────────────────────────────────────────

    [Fact]
    public async Task IncrementalTrainer_EmptyExamples_ReturnsFailure()
    {
        // Gate B: zero examples → Fail with EMPTY_EXAMPLES code before reaching LoRA.
        var trainer = new IncrementalTrainer(Storage);
        var result  = await trainer.TrainAsync("CODE_Q", [], []);

        Assert.False(result.IsSuccess);
        Assert.Equal("EMPTY_EXAMPLES", result.ErrorCode);
    }

    [Fact]
    public void IncrementalTrainer_DefaultConfig_RankIsEight()
    {
        // Gate B: Rank=8 matching existing 10.LoRA experiment baseline.
        Assert.Equal(8, IncrementalTrainer.DefaultConfig.Rank);
    }

    [Fact]
    public void IncrementalTrainer_DefaultConfig_TargetModulesContainQProj()
    {
        // Gate B: q_proj in TargetModules — delegates to LoraApplicator which matches this name.
        Assert.Contains("q_proj", IncrementalTrainer.DefaultConfig.TargetModules);
    }

    [Fact]
    public void IncrementalTrainer_DefaultConfig_DoesNotExposeSharedMutableTargets()
    {
        var first = IncrementalTrainer.DefaultConfig;
        first.TargetModules[0] = "mutated";

        var second = IncrementalTrainer.DefaultConfig;

        Assert.Equal(["q_proj", "v_proj"], second.TargetModules);
    }

    [Fact]
    public void IncrementalTrainer_UsesLoraApplicator_NotCustomGradients()
    {
        // Gate B: ContinuousLearning assembly references LoRA — proves delegation,
        // not reimplementation of gradient logic.
        var refs = typeof(IncrementalTrainer).Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? "");
        Assert.Contains(refs, n => n.Equals("LoRA", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task IncrementalTrainer_SupervisedStep_MutatesOnlyAdapter_AndPublishesFreshLoadableCheckpoint()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var factory = new RecordingSessionFactoryCL(fixture.CreateFactory());
        var trainer = new IncrementalTrainer(
            Storage,
            factory,
            fixture.AdapterDirectory + Path.DirectorySeparatorChar);
        var baseCheckpointBefore = File.ReadAllBytes(fixture.BaseCheckpointPath);

        var result = await trainer.TrainAsync(
            "BUG",
            [fixture.Example("BUG", "fix", "fixed")],
            []);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var artifact = Assert.IsType<TrainedAdapterArtifact>(result.Value);
        Assert.True(File.Exists(artifact.AdapterPath));
        Assert.NotEmpty(File.ReadAllBytes(artifact.AdapterPath));
        Assert.True(Path.IsPathFullyQualified(artifact.AdapterPath));
        Assert.Equal(baseCheckpointBefore, File.ReadAllBytes(fixture.BaseCheckpointPath));

        var trainedSession = Assert.Single(factory.Sessions);
        Assert.Null(trainedSession.ParentAdapterCheckpoint);
        AssertParametersEqual(
            factory.BaseSnapshots.Single(),
            trainedSession.Model.BaseParameters);
        Assert.All(
            trainedSession.Model.Parameters(),
            parameter => Assert.All(parameter.Value.Data, value => Assert.True(float.IsFinite(value))));
        var trainedAdapterBits = SnapshotParameterBits(
            GetAdapterParameters(trainedSession.Model));

        var registeredAdapter = new ModelAdapter(
            "BUG",
            artifact.AdapterPath,
            1,
            DateTimeOffset.UtcNow,
            artifact.Provenance);
        fixture.Registry.Register(registeredAdapter);
        Assert.Equal(
            artifact.AdapterPath,
            fixture.Registry.GetAdapter("BUG").Value!.AdapterPath);

        var fresh = new LlmModel(fixture.ModelConfig);
        fresh.LoadCheckpoint(fixture.BaseCheckpointPath);
        LoraApplicator.Apply(fresh, IncrementalTrainer.DefaultConfig);
        fresh.LoadAdapterCheckpoint(artifact.AdapterPath);

        var loadedAdapterBits = SnapshotParameterBits(GetAdapterParameters(fresh));
        Assert.Equal(trainedAdapterBits.Count, loadedAdapterBits.Count);
        for (var index = 0; index < trainedAdapterBits.Count; index++)
            Assert.Equal(trainedAdapterBits[index], loadedAdapterBits[index]);
    }

    [Fact]
    public void Trainer_SupervisedNextTokenStep_SameSessionMutatesAdapterOnly_AndClearsGradients()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var session = fixture.CreateFactory().Create("BUG").Value!;
        LoraApplicator.Apply(session.Model, NoDropoutLoraConfig());
        var baseBefore = SnapshotParameterBits(session.Model.BaseParameters);
        var adapterParameters = GetAdapterParameters(session.Model);
        var adaptersBefore = SnapshotParameterBits(adapterParameters);
        var tokenIds = session.Tokenizer.Encode("fix\nfixed");

        var loss = session.Trainer.TrainSupervisedNextTokenStep(
            session.Model,
            session.Optimizer,
            tokenIds,
            1f);

        Assert.True(float.IsFinite(loss));
        AssertParameterBitsEqual(baseBefore, session.Model.BaseParameters);
        var adaptersAfter = SnapshotParameterBits(adapterParameters);
        Assert.Contains(
            adaptersBefore.Zip(adaptersAfter),
            pair => !pair.First.SequenceEqual(pair.Second));
        Assert.All(
            adapterParameters,
            parameter => Assert.True(
                parameter.Grad is null || parameter.Grad.Data.All(value => value == 0f)));
    }

    [Fact]
    public void Trainer_SupervisedNextTokenStep_WeightedLossScalesOnce_FromIdenticalState()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var first = fixture.CreateFactory().Create("BUG").Value!;
        var second = fixture.CreateFactory().Create("BUG").Value!;
        LoraApplicator.Apply(first.Model, NoDropoutLoraConfig());
        string initialAdapter = Path.Combine(fixture.RootDirectory, "initial-adapter.lora");
        first.Model.SaveAdapterCheckpoint(initialAdapter);
        LoraApplicator.Apply(second.Model, NoDropoutLoraConfig());
        second.Model.LoadAdapterCheckpoint(initialAdapter);
        var tokenIds = first.Tokenizer.Encode("weighted\nresponse");
        var firstBaseBefore = SnapshotParameterBits(first.Model.BaseParameters);
        var secondBaseBefore = SnapshotParameterBits(second.Model.BaseParameters);
        var firstAdapters = GetAdapterParameters(first.Model);
        var secondAdapters = GetAdapterParameters(second.Model);
        var initialAdapterBits = SnapshotParameterBits(firstAdapters);
        var unitConfig = fixture.TrainingConfig with
        {
            WarmupSteps = 0,
            MaxLr = 1f,
            MinLr = 1f,
            TotalSteps = 2,
            GradClipNorm = 1_000_000f,
            WeightDecay = 0f,
        };
        var doubleConfig = fixture.TrainingConfig with
        {
            WarmupSteps = 0,
            MaxLr = 1f,
            MinLr = 1f,
            TotalSteps = 2,
            GradClipNorm = 1_000_000f,
            WeightDecay = 0f,
        };
        var unitTrainer = new Trainer(unitConfig);
        var doubleTrainer = new Trainer(doubleConfig);
        var unitOptimizer = new AdamW(weightDecay: 0f, eps: 1_000f);
        var doubleOptimizer = new AdamW(weightDecay: 0f, eps: 1_000f);

        var unitLoss = unitTrainer.TrainSupervisedNextTokenStep(
            first.Model,
            unitOptimizer,
            tokenIds,
            1f);
        var doubleLoss = doubleTrainer.TrainSupervisedNextTokenStep(
            second.Model,
            doubleOptimizer,
            tokenIds,
            2f);

        var unitAdapterBits = SnapshotParameterBits(firstAdapters);
        var doubleAdapterBits = SnapshotParameterBits(secondAdapters);
        Assert.Equal(unitLoss * 2f, doubleLoss);
        Assert.Contains(
            initialAdapterBits.Zip(unitAdapterBits),
            pair => !pair.First.SequenceEqual(pair.Second));
        Assert.Contains(
            initialAdapterBits.Zip(doubleAdapterBits),
            pair => !pair.First.SequenceEqual(pair.Second));
        Assert.Contains(
            unitAdapterBits.Zip(doubleAdapterBits),
            pair => !pair.First.SequenceEqual(pair.Second));
        AssertParameterBitsEqual(firstBaseBefore, first.Model.BaseParameters);
        AssertParameterBitsEqual(secondBaseBefore, second.Model.BaseParameters);
        Assert.All(
            firstAdapters.Concat(secondAdapters),
            parameter => Assert.True(
                parameter.Grad is null || parameter.Grad.Data.All(value => value == 0f)));
    }

    [Fact]
    public void Trainer_SupervisedNextTokenStep_EnforcesTokenAndWeightBoundaries()
    {
        using var fixture = new TinyTrainingFixtureCL(contextLength: 4);
        var session = fixture.CreateFactory().Create("BUG").Value!;
        LoraApplicator.Apply(session.Model, NoDropoutLoraConfig());

        Assert.True(float.IsFinite(session.Trainer.TrainSupervisedNextTokenStep(
            session.Model,
            session.Optimizer,
            [1, 2],
            1f)));
        Assert.True(float.IsFinite(session.Trainer.TrainSupervisedNextTokenStep(
            session.Model,
            session.Optimizer,
            [1, 2, 3, 4, 5],
            1f)));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            session.Trainer.TrainSupervisedNextTokenStep(
                session.Model, session.Optimizer, [1], 1f));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            session.Trainer.TrainSupervisedNextTokenStep(
                session.Model, session.Optimizer, [1, 2, 3, 4, 5, 6], 1f));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            session.Trainer.TrainSupervisedNextTokenStep(
                session.Model, session.Optimizer, [1, 256], 1f));

        foreach (var invalidWeight in new[] { 0f, float.NaN, float.PositiveInfinity })
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                session.Trainer.TrainSupervisedNextTokenStep(
                    session.Model, session.Optimizer, [1, 2], invalidWeight));
        }
    }

    [Fact]
    public void Trainer_SupervisedNextTokenStep_InvalidLearningRate_DoesNotUpdateParameters()
    {
        using var fixture = new TinyTrainingFixtureCL();

        foreach (var invalidLearningRate in new[] { 0f, float.NaN })
        {
            var session = fixture.CreateFactory().Create("BUG").Value!;
            LoraApplicator.Apply(session.Model, NoDropoutLoraConfig());
            var adapterParameters = GetAdapterParameters(session.Model);
            var before = SnapshotParameterBits(adapterParameters);
            var invalidTrainer = new Trainer(fixture.TrainingConfig with
            {
                WarmupSteps = 0,
                MaxLr = invalidLearningRate,
                MinLr = invalidLearningRate,
            });

            // Снимок памяти оптимизатора до отказа: счётчик шага и накопленные оценки.
            var optimizerBindings = adapterParameters
                .Select((parameter, index) => new AdamWParameterBinding($"p{index:D4}", parameter))
                .ToArray();
            var optimizerBefore = session.Optimizer.ExportPortableState(optimizerBindings);

            Assert.Throws<InvalidOperationException>(() =>
                invalidTrainer.TrainSupervisedNextTokenStep(
                    session.Model,
                    session.Optimizer,
                    [1, 2],
                    1f));
            AssertParameterBitsEqual(before, adapterParameters);

            // Непригодная скорость отвергается до любого обращения к модели: у
            // параметров не появилось ни одного градиента, значит ни прямого, ни
            // обратного прохода не было. Это наблюдаемое число вызовов, равное нулю.
            Assert.All(adapterParameters, parameter => Assert.Null(parameter.Grad));

            // Память оптимизатора тоже не сдвинулась: тот же счётчик шага и те же
            // накопленные оценки, то есть ни одного обновления не произошло.
            var optimizerAfter = session.Optimizer.ExportPortableState(optimizerBindings);
            Assert.Equal(optimizerBefore.Step, optimizerAfter.Step);
            Assert.Equal(optimizerBefore.Entries.Count, optimizerAfter.Entries.Count);
            Assert.Equal(optimizerBefore.ToBytes(), optimizerAfter.ToBytes());
        }
    }

    // Очистка градиентов при отказе проверяется ОТДЕЛЬНО от неизменности весов:
    // ранее накопленные градиенты обязаны обнулиться, а веса остаться прежними.
    [Fact]
    public void Trainer_SupervisedNextTokenStep_InvalidLearningRate_ClearsGradientsWithoutTouchingWeights()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var session = fixture.CreateFactory().Create("BUG").Value!;
        LoraApplicator.Apply(session.Model, NoDropoutLoraConfig());
        var adapterParameters = GetAdapterParameters(session.Model);

        // Заранее положенные ненулевые градиенты: они и должны быть очищены отказом.
        foreach (var parameter in adapterParameters)
        {
            var gradient = TensorCore.Core.TensorOps.ZerosLike(parameter.Value);
            for (var index = 0; index < gradient.Data.Length; index++) gradient.Data[index] = 1f;
            parameter.Value.Grad = gradient;
        }

        var before = SnapshotParameterBits(adapterParameters);
        var invalidTrainer = new Trainer(fixture.TrainingConfig with
        {
            WarmupSteps = 0,
            MaxLr = 0f,
            MinLr = 0f,
        });

        Assert.Throws<InvalidOperationException>(() =>
            invalidTrainer.TrainSupervisedNextTokenStep(
                session.Model,
                session.Optimizer,
                [1, 2],
                1f));

        // Веса не сдвинулись ни одним битом.
        AssertParameterBitsEqual(before, adapterParameters);
        // А накопленные градиенты обнулены: прежний контракт очистки сохранён.
        Assert.All(
            adapterParameters,
            parameter =>
            {
                Assert.NotNull(parameter.Grad);
                Assert.All(parameter.Grad!.Data, value => Assert.Equal(0f, value));
            });
    }

    // Разогрев: самый первый урок дообучения обязан дойти до весов, а не быть
    // отвергнутым нулевой скоростью. Остальные шаги расписания не сдвигаются.
    [Fact]
    public void Trainer_FirstSupervisedStepWithWarmup_ReachesWeightsAndKeepsLaterSteps()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var session = fixture.CreateFactory().Create("BUG").Value!;
        LoraApplicator.Apply(session.Model, NoDropoutLoraConfig());
        var adapterParameters = GetAdapterParameters(session.Model);
        var before = SnapshotParameterBits(adapterParameters);
        var warmupTrainer = new Trainer(fixture.TrainingConfig with
        {
            WarmupSteps = 4,
            TotalSteps = 8,
            MaxLr = 0.001f,
            MinLr = 0.0001f,
        });

        // Первый урок проходит целиком: отказа по нулевой скорости больше нет.
        warmupTrainer.TrainSupervisedNextTokenStep(
            session.Model,
            session.Optimizer,
            [1, 2, 3],
            1f);

        // Состояние действительно сдвинулось: урок дошёл до весов.
        var after = SnapshotParameterBits(adapterParameters);
        Assert.Contains(before.Zip(after), pair => !pair.First.SequenceEqual(pair.Second));
    }

    [Fact]
    public async Task IncrementalTrainer_DpoPairs_RequireServerPolicyBeforeSessionOrFilesystemMutation()
    {
        string adapterDirectory = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-dpo-{Guid.NewGuid():N}");
        var factory = new CountingSessionFactoryCL();
        var trainer = new IncrementalTrainer(Storage, factory, adapterDirectory);
        var example = new TrainingExample(
            "prompt", "response", "BUG", 1d, DateTimeOffset.UtcNow);

        var pair = new DpoPair(
            "p",
            "chosen",
            "rejected",
            "BUG",
            DateTimeOffset.UtcNow)
        {
            TeacherProvenance = TrainingTeacherProvenance.Deterministic(
                "missing-server-policy-request"),
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                [HashRef('f')]),
        };
        var boundPair = DpoPairTrainingContentAuthority.Bind(pair);
        Assert.True(boundPair.IsSuccess, boundPair.ErrorMessage);
        Assert.NotNull(boundPair.Value);

        var result = await trainer.TrainAsync(
            "BUG",
            [example],
            [boundPair.Value!]);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            LegalTrainingTeacherAdmissionAuthority.ConfigurationRequiredErrorCode,
            result.ErrorCode);
        Assert.Equal(0, factory.CreateCount);
        Assert.False(Directory.Exists(adapterDirectory));
    }

    [Fact]
    public async Task IncrementalTrainer_QueuedCancellation_EscapesWithoutSecondSessionOrArtifact()
    {
        string adapterDirectory = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-cancel-{Guid.NewGuid():N}");
        var factory = new BlockingSessionFactoryCL();
        var trainer = new IncrementalTrainer(Storage, factory, adapterDirectory);
        var example = new TrainingExample(
            "prompt", "response", "BUG", 1d, DateTimeOffset.UtcNow);
        var first = Task.Run(() => trainer.TrainAsync("BUG", [example], []));

        try
        {
            Assert.True(factory.Entered.Wait(TimeSpan.FromSeconds(10)));
            using var cancellation = new CancellationTokenSource();
            var queued = trainer.TrainAsync(
                "BUG",
                [example],
                [],
                cancellation.Token);
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => queued);
            Assert.Equal(1, factory.CreateCount);
            Assert.False(Directory.Exists(adapterDirectory));
        }
        finally
        {
            factory.Release.Set();
        }

        var firstResult = await first;
        Assert.False(firstResult.IsSuccess);
        Assert.Equal("TRAINING_SESSION_CREATION_FAILED", firstResult.ErrorCode);
        Assert.Equal(1, factory.CreateCount);
        Assert.False(Directory.Exists(adapterDirectory));
    }

    [Fact]
    public void IncrementalTrainer_InvalidLoraConfig_FailsInConstructorBeforeSessionOrFilesystem()
    {
        string adapterDirectory = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-config-{Guid.NewGuid():N}");
        var factory = new CountingSessionFactoryCL();
        var invalid = new LoraConfig
        {
            Rank = 0,
            Alpha = 16f,
            Dropout = 0f,
            TargetModules = ["q_proj"],
        };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IncrementalTrainer(
                Storage,
                factory,
                adapterDirectory,
                invalid));
        Assert.Equal(0, factory.CreateCount);
        Assert.False(Directory.Exists(adapterDirectory));
    }

    [Fact]
    public async Task IncrementalTrainer_UsesCanonicalInstanceLoraConfig_NotCallerMutation()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var factory = new RecordingSessionFactoryCL(fixture.CreateFactory());
        var supplied = new LoraConfig
        {
            Rank = 2,
            Alpha = 4f,
            Dropout = 0f,
            TargetModules = ["k_proj"],
        };
        var trainer = new IncrementalTrainer(
            Storage,
            factory,
            fixture.AdapterDirectory,
            supplied);
        supplied.TargetModules[0] = "q_proj";

        var result = await trainer.TrainAsync(
            "BUG",
            [fixture.Example("BUG", "fix", "fixed")],
            []);

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        var session = Assert.Single(factory.Sessions);
        Assert.NotNull(session.Model.GetAttachedProjectionAdapter(0, "k_proj"));
        Assert.Null(session.Model.GetAttachedProjectionAdapter(0, "q_proj"));
        Assert.Null(session.Model.GetAttachedProjectionAdapter(0, "v_proj"));
    }

    [Fact]
    public void IncrementalTrainingSessionFactory_ReturnsFreshIsolatedSessions_AndResolvesParent()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var factory = fixture.CreateFactory();

        var first = factory.Create("BUG");
        var second = factory.Create("BUG");

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.NotSame(first.Value!.Model, second.Value!.Model);
        Assert.NotSame(first.Value.Tokenizer, second.Value.Tokenizer);
        Assert.NotSame(first.Value.Trainer, second.Value.Trainer);
        Assert.NotSame(first.Value.Optimizer, second.Value.Optimizer);

        LoraApplicator.Apply(first.Value.Model, IncrementalTrainer.DefaultConfig);
        Assert.NotNull(first.Value.Model.GetAttachedProjectionAdapter(0, "q_proj"));
        Assert.Null(second.Value.Model.GetAttachedProjectionAdapter(0, "q_proj"));

        string parentPath = Path.Combine(fixture.AdapterDirectory, "parent.lora");
        Directory.CreateDirectory(fixture.AdapterDirectory);
        first.Value.Model.SaveAdapterCheckpoint(parentPath);
        fixture.Registry.Register(
            CreateAdapterRegistrationCL(
                fixture,
                "BUG",
                parentPath,
                NoDropoutLoraConfig()));

        var continuation = factory.Create("BUG");
        Assert.True(continuation.IsSuccess, continuation.ErrorMessage);
        var checkpoint = Assert.IsType<IncrementalTrainingParentAdapterCheckpoint>(
            continuation.Value!.ParentAdapterCheckpoint);
        using (var checkpointStream = checkpoint.OpenRead())
        {
            Assert.Equal(
                HashArtifactCL(parentPath),
                TrainingSourceProvenance.Sha256(checkpointStream));
        }
        Assert.Null(continuation.Value.Model.GetAttachedProjectionAdapter(0, "q_proj"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IncrementalTrainer_UsesCapturedParentAfterSourceDeletionOrReplacement(
        bool replaceSource)
    {
        using var fixture = new TinyTrainingFixtureCL();
        Directory.CreateDirectory(fixture.AdapterDirectory);
        var parentModel = new LlmModel(fixture.ModelConfig);
        LoraApplicator.Apply(parentModel, IncrementalTrainer.DefaultConfig);
        MutateAdapterParametersCL(parentModel, 0.25f);
        var expectedParentBits = SnapshotParameterBits(GetAdapterParameters(parentModel));
        var parentPath = Path.Combine(fixture.AdapterDirectory, "captured-parent.lora");
        parentModel.SaveAdapterCheckpoint(parentPath);
        var registration = fixture.Registry.Register(
            CreateAdapterRegistrationCL(
                fixture,
                "BUG",
                parentPath,
                IncrementalTrainer.DefaultConfig));
        Assert.True(registration.IsSuccess, registration.ErrorMessage);

        var sessionResult = fixture.CreateFactory().Create("BUG");
        Assert.True(sessionResult.IsSuccess, sessionResult.ErrorMessage);
        var session = sessionResult.Value!;
        Assert.NotNull(session.ParentAdapterCheckpoint);
        var baseBefore = SnapshotParameterBits(session.Model.BaseParameters);

        if (replaceSource)
        {
            var replacementModel = new LlmModel(fixture.ModelConfig);
            LoraApplicator.Apply(replacementModel, IncrementalTrainer.DefaultConfig);
            MutateAdapterParametersCL(replacementModel, -0.75f);
            replacementModel.SaveAdapterCheckpoint(parentPath);
            Assert.NotEqual(
                expectedParentBits[0],
                SnapshotParameterBits(GetAdapterParameters(replacementModel))[0]);
        }
        else
        {
            File.Delete(parentPath);
        }

        var observer = new ParentCheckpointObservationStepExecutorCL(
            expectedParentBits);
        var trainer = CreateTrainerWithStepExecutorCL(
            fixture,
            new FixedSessionFactoryCL(session),
            observer);

        var result = await trainer.TrainAsync(
            "BUG",
            [fixture.Example("BUG", "captured", "parent")],
            []);

        Assert.False(result.IsSuccess);
        Assert.Equal("SUPERVISED_ADAPTER_TRAINING_FAILED", result.ErrorCode);
        Assert.True(observer.ObservedLoadedCheckpoint);
        Assert.Equal(1, observer.ExecuteCount);
        AssertParameterBitsEqual(baseBefore, session.Model.BaseParameters);
        AssertParameterBitsEqual(
            expectedParentBits,
            GetAdapterParameters(session.Model));
        Assert.False(File.Exists(Path.Combine(fixture.AdapterDirectory, "BUG.lora")));
    }

    [Fact]
    public void IncrementalTrainingParentAdapterCheckpoint_OwnsBytesAndProvenanceAndCreatesIndependentReaders()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var parentPath = CreateValidAdapterArtifactCL(
            fixture,
            "owned-parent.lora");
        var registration = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            parentPath,
            IncrementalTrainer.DefaultConfig);
        var callerBytes = File.ReadAllBytes(parentPath);
        var expectedBytes = callerBytes.ToArray();
        var mutableSources = registration.Provenance!.TrainingSources.SourceIds.ToList();
        var mutableTargets = registration.Provenance.Lora.OrderedTargetModules.ToList();
        var callerProvenance = registration.Provenance with
        {
            TrainingSources = registration.Provenance.TrainingSources with
            {
                SourceIds = mutableSources,
            },
            Lora = registration.Provenance.Lora with
            {
                OrderedTargetModules = mutableTargets,
            },
        };
        using var callerStream = new MemoryStream(callerBytes, writable: true);

        var checkpoint = new IncrementalTrainingParentAdapterCheckpoint(
            callerStream,
            callerProvenance);
        Array.Fill(callerBytes, (byte)0x5a);
        mutableSources.Clear();
        mutableTargets.Clear();

        Assert.True(callerStream.CanRead);
        Assert.NotEmpty(checkpoint.Provenance.TrainingSources.SourceIds);
        Assert.Equal(
            ["q_proj", "v_proj"],
            checkpoint.Provenance.Lora.OrderedTargetModules);
        using var first = checkpoint.OpenRead();
        using var second = checkpoint.OpenRead();
        Assert.True(first.CanRead);
        Assert.True(first.CanSeek);
        Assert.False(first.CanWrite);
        Assert.NotSame(first, second);
        Assert.Equal(0, first.Position);
        Assert.Equal(0, second.Position);
        Assert.NotEqual(-1, first.ReadByte());
        Assert.Equal(1, first.Position);
        Assert.Equal(0, second.Position);
        Assert.Equal(expectedBytes, ReadAllBytesCL(second));
        Assert.False(Assert.IsType<MemoryStream>(first).TryGetBuffer(out _));
    }

    [Theory]
    [InlineData("empty", "ADAPTER_CHECKPOINT_INVALID")]
    [InlineData("malformed", "ADAPTER_CHECKPOINT_INVALID")]
    [InlineData("oversized", "ADAPTER_CHECKPOINT_INVALID")]
    [InlineData("hash-mismatch", "ADAPTER_PROVENANCE_MISMATCH")]
    public void IncrementalTrainingSessionFactory_MapsCapturedParentFailuresExactly(
        string mutation,
        string expectedCode)
    {
        using var fixture = new TinyTrainingFixtureCL();
        Directory.CreateDirectory(fixture.AdapterDirectory);
        var validPath = CreateValidAdapterArtifactCL(fixture, "capture-valid.lora");
        var valid = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            validPath,
            IncrementalTrainer.DefaultConfig);
        var candidatePath = Path.Combine(
            fixture.AdapterDirectory,
            $"capture-{mutation}.lora");
        var provenance = valid.Provenance!;
        switch (mutation)
        {
            case "empty":
                File.WriteAllBytes(candidatePath, []);
                provenance = provenance with
                {
                    AdapterSha256 = HashBytesCL([]),
                };
                break;
            case "malformed":
                byte[] malformed = [1, 2, 3, 4];
                File.WriteAllBytes(candidatePath, malformed);
                provenance = provenance with
                {
                    AdapterSha256 = HashBytesCL(malformed),
                };
                break;
            case "oversized":
                using (var oversized = new FileStream(
                           candidatePath,
                           FileMode.CreateNew,
                           FileAccess.Write,
                           FileShare.None))
                {
                    oversized.SetLength(
                        IncrementalTrainingParentAdapterCheckpoint.MaximumArtifactBytes + 1);
                }
                break;
            case "hash-mismatch":
                File.Copy(validPath, candidatePath);
                provenance = provenance with
                {
                    AdapterSha256 = new string('0', 64),
                };
                break;
            default:
                throw new InvalidOperationException($"Unexpected mutation: {mutation}");
        }

        var factory = CreateFactoryWithParentCL(
            fixture,
            new ModelAdapter(
                "BUG",
                candidatePath,
                1,
                DateTimeOffset.UtcNow,
                provenance));

        var result = factory.Create("BUG");

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.ErrorCode);
    }

    [Fact]
    public void IncrementalTrainingSessionFactory_MissingParentOpenIsNotFound()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var validPath = CreateValidAdapterArtifactCL(fixture, "missing-provenance-source.lora");
        var valid = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            validPath,
            IncrementalTrainer.DefaultConfig);
        var missingPath = Path.Combine(fixture.AdapterDirectory, "missing-parent.lora");
        var factory = CreateFactoryWithParentCL(
            fixture,
            valid with { AdapterPath = missingPath });

        var result = factory.Create("BUG");

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_CHECKPOINT_NOT_FOUND", result.ErrorCode);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IncrementalTrainingSessionFactory_PostOpenDeletionOrNameExchangeIsInvalid(
        bool replaceName)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var parentPath = CreateValidAdapterArtifactCL(
            fixture,
            "post-open-parent.lora");
        var parent = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            parentPath,
            IncrementalTrainer.DefaultConfig);
        var alternatePath = CreateValidAdapterArtifactCL(
            fixture,
            "post-open-alternate.lora",
            ["k_proj"]);
        var alternateSha256 = HashArtifactCL(alternatePath);
        var displacedPath = Path.Combine(
            fixture.AdapterDirectory,
            "post-open-displaced.lora");
        FileStream? observedStream = null;
        var callbackCount = 0;
        var exchangeCompleted = false;
        var factory = CreateFactoryWithParentCL(
            fixture,
            parent,
            (openedStream, openedPath) =>
            {
                callbackCount++;
                observedStream = openedStream;
                Assert.True(openedStream.CanRead);
                Assert.Equal(parentPath, openedPath);
                if (replaceName)
                {
                    File.Move(openedPath, displacedPath);
                    File.Move(alternatePath, openedPath);
                }
                else
                {
                    File.Delete(openedPath);
                }

                exchangeCompleted = true;
            });

        var result = factory.Create("BUG");

        Assert.True(exchangeCompleted);
        Assert.Equal(1, callbackCount);
        Assert.NotNull(observedStream);
        Assert.False(observedStream.CanRead);
        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_CHECKPOINT_INVALID", result.ErrorCode);
        if (replaceName)
            Assert.Equal(alternateSha256, HashArtifactCL(parentPath));
    }

    [ReparsePointFactCL]
    public void IncrementalTrainingSessionFactory_PostOpenSymbolicLinkExchangeIsInvalidWhenSupported()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var parentPath = CreateValidAdapterArtifactCL(
            fixture,
            "post-open-link-parent.lora");
        var parent = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            parentPath,
            IncrementalTrainer.DefaultConfig);
        var alternatePath = CreateValidAdapterArtifactCL(
            fixture,
            "post-open-link-target.lora",
            ["k_proj"]);
        var displacedPath = Path.Combine(
            fixture.AdapterDirectory,
            "post-open-link-displaced.lora");
        var exchangeCompleted = false;
        var factory = CreateFactoryWithParentCL(
            fixture,
            parent,
            (_, openedPath) =>
            {
                File.Move(openedPath, displacedPath);
                File.CreateSymbolicLink(openedPath, alternatePath);
                exchangeCompleted = true;
            });

        var result = factory.Create("BUG");

        Assert.True(exchangeCompleted);
        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_CHECKPOINT_INVALID", result.ErrorCode);
        Assert.True(
            (File.GetAttributes(parentPath) & FileAttributes.ReparsePoint) != 0);
    }

    [LinuxX64StatxFactCL]
    public void IncrementalTrainingSessionFactory_LinuxStatxIdentityMatchesIndependentNativeEvidence()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var parentPath = CreateValidAdapterArtifactCL(
            fixture,
            "linux-native-identity-parent.lora");
        var parent = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            parentPath,
            IncrementalTrainer.DefaultConfig);
        var observed = false;
        var factory = CreateFactoryWithParentCL(
            fixture,
            parent,
            (stream, openedPath) =>
            {
                var descriptor = checked(
                    (int)stream.SafeFileHandle.DangerousGetHandle().ToInt64());
                Assert.Equal(0, FStatLinuxCL(descriptor, out var descriptorStatus));
                Assert.Equal(
                    0,
                    StatxLinuxCL(
                        -100,
                        openedPath,
                        0x100,
                        0x07ff,
                        out var pathStatus));
                Assert.Equal(1UL, descriptorStatus.LinkCount);
                Assert.Equal(1U, pathStatus.LinkCount);

                var validator = Assert.IsAssignableFrom<MethodInfo>(
                    typeof(CheckpointBackedIncrementalTrainingSessionFactory)
                        .GetMethod(
                            "ValidateOpenedParentAdapter",
                            BindingFlags.Static | BindingFlags.NonPublic));
                var productionIdentity = validator.Invoke(
                    null,
                    [stream, openedPath, null]);
                Assert.NotNull(productionIdentity);
                var identityType = productionIdentity.GetType();
                var productionDevice = Assert.IsType<ulong>(
                    identityType.GetProperty("DeviceOrVolume")!
                        .GetValue(productionIdentity));
                var productionInode = Assert.IsType<ulong>(
                    identityType.GetProperty("FileIdLow")!
                        .GetValue(productionIdentity));
                var productionLinks = Assert.IsType<uint>(
                    identityType.GetProperty("LinkCount")!
                        .GetValue(productionIdentity));
                var productionLength = Assert.IsType<ulong>(
                    identityType.GetProperty("Length")!
                        .GetValue(productionIdentity));

                var fstatDevice = PackLinuxDeviceCL(descriptorStatus.Device);
                var statxDevice = ((ulong)pathStatus.DeviceMajor << 32)
                    | pathStatus.DeviceMinor;
                Assert.Equal(fstatDevice, statxDevice);
                Assert.Equal(fstatDevice, productionDevice);
                Assert.Equal(descriptorStatus.Inode, pathStatus.Inode);
                Assert.Equal(descriptorStatus.Inode, productionInode);
                Assert.Equal((uint)descriptorStatus.LinkCount, productionLinks);
                Assert.Equal((ulong)stream.Length, productionLength);
                observed = true;
            });

        var result = factory.Create("BUG");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(observed);
    }

    [LinuxFactCL]
    public void IncrementalTrainingSessionFactory_SizeMutationAfterCaptureIsInvalid()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var parentPath = CreateValidAdapterArtifactCL(
            fixture,
            "size-mutated-parent.lora");
        var parent = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            parentPath,
            IncrementalTrainer.DefaultConfig);
        var captureMutationObserved = false;
        var factory = CreateFactoryWithParentCL(
            fixture,
            parent,
            afterParentAdapterCaptureForTesting: (_, openedPath) =>
            {
                using var mutator = new FileStream(
                    openedPath,
                    FileMode.Open,
                    FileAccess.Write,
                    FileShare.ReadWrite | FileShare.Delete);
                mutator.SetLength(checked(mutator.Length + 1));
                captureMutationObserved = true;
            });

        var result = factory.Create("BUG");

        Assert.True(captureMutationObserved);
        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_CHECKPOINT_INVALID", result.ErrorCode);
        Assert.Null(result.Value);
    }

    [LinuxFifoFactCL]
    public async Task IncrementalTrainingSessionFactory_LinuxFifoIsInvalidWithoutBlocking()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var provenanceSource = CreateValidAdapterArtifactCL(
            fixture,
            "fifo-provenance-source.lora");
        var parent = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            provenanceSource,
            IncrementalTrainer.DefaultConfig);
        var fifoPath = Path.Combine(fixture.AdapterDirectory, "parent-fifo.lora");
        Assert.Equal(0, MakeFifoLinuxCL(fifoPath, 0x180));
        var callbackObserved = false;
        var factory = CreateFactoryWithParentCL(
            fixture,
            parent with { AdapterPath = fifoPath },
            (_, _) => callbackObserved = true);
        var factoryBoundaryEntered = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var factoryTask = Task.Factory.StartNew(
            () =>
            {
                factoryBoundaryEntered.TrySetResult();
                return factory.Create("BUG");
            },
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        var cleanupOwnershipTransferred = false;

        async Task<(
            OperationResult<IncrementalTrainingSession>? Result,
            Exception? Exception,
            bool WorkerTerminated,
            int OpenError,
            int? CloseError)> ReleaseBlockingMutantAndJoinAsync()
        {
            var cleanupDescriptor = OpenLinuxCL(
                fifoPath,
                0x0000_0002 | 0x0000_0800 | 0x0008_0000);
            var openError = cleanupDescriptor >= 0
                ? 0
                : Marshal.GetLastPInvokeError();
            OperationResult<IncrementalTrainingSession>? eventualResult = null;
            Exception? eventualException = null;
            try
            {
                eventualResult = await factoryTask
                    .WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (Exception exception)
            {
                eventualException = exception;
            }

            var workerTerminated = factoryTask.IsCompleted;
            int? closeError = null;
            if (cleanupDescriptor >= 0 && workerTerminated)
            {
                if (CloseLinuxCL(cleanupDescriptor) != 0)
                    closeError = Marshal.GetLastPInvokeError();
                cleanupDescriptor = -1;
            }
            else if (cleanupDescriptor >= 0)
            {
                var heldDescriptor = cleanupDescriptor;
                cleanupDescriptor = -1;
                cleanupOwnershipTransferred = true;
                _ = factoryTask.ContinueWith(
                    (completedTask, state) =>
                    {
                        _ = CloseLinuxCL((int)state!);
                        completedTask.Dispose();
                    },
                    heldDescriptor,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }

            return (
                eventualResult,
                eventualException,
                workerTerminated,
                openError,
                closeError);
        }

        try
        {
            _ = await Task.WhenAny(
                factoryBoundaryEntered.Task,
                factoryTask,
                Task.Delay(TimeSpan.FromSeconds(5)));
            if (!factoryBoundaryEntered.Task.IsCompleted)
            {
                var cleanup = await ReleaseBlockingMutantAndJoinAsync();
                Assert.Fail(
                    "Long-running FIFO worker did not reach the factory boundary "
                    + "within the setup deadline. "
                    + FormatFifoCleanupEvidenceCL(cleanup));
            }

            await factoryBoundaryEntered.Task;
            var deadline = Task.Delay(TimeSpan.FromMilliseconds(750));
            _ = await Task.WhenAny(factoryTask, deadline);
            if (!factoryTask.IsCompleted)
            {
                var cleanup = await ReleaseBlockingMutantAndJoinAsync();
                Assert.Fail(
                    "Factory blocked on FIFO past the writer-free deadline. "
                    + FormatFifoCleanupEvidenceCL(cleanup));
            }

            var result = await factoryTask;

            Assert.True(callbackObserved);
            Assert.False(result.IsSuccess);
            Assert.Equal("ADAPTER_CHECKPOINT_INVALID", result.ErrorCode);
        }
        finally
        {
            if (!cleanupOwnershipTransferred && factoryTask.IsCompleted)
                factoryTask.Dispose();
        }
    }

    [ReparsePointTheoryCL]
    [InlineData(false)]
    [InlineData(true)]
    public void IncrementalTrainingSessionFactory_SymbolicLinkInitialOpenIsInvalid(
        bool dangling)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var targetPath = CreateValidAdapterArtifactCL(
            fixture,
            "initial-link-target.lora");
        var parent = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            targetPath,
            IncrementalTrainer.DefaultConfig);
        var linkPath = Path.Combine(
            fixture.AdapterDirectory,
            dangling ? "dangling-parent.lora" : "linked-parent.lora");
        var linkTarget = dangling
            ? Path.Combine(fixture.AdapterDirectory, "absent-link-target.lora")
            : targetPath;
        File.CreateSymbolicLink(linkPath, linkTarget);

        var callbackCount = 0;
        var factory = CreateFactoryWithParentCL(
            fixture,
            parent with { AdapterPath = linkPath },
            (_, _) => callbackCount++);

        var result = factory.Create("BUG");

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_CHECKPOINT_INVALID", result.ErrorCode);
        Assert.InRange(callbackCount, 0, 1);
    }

    [HardLinkFactCL]
    public void IncrementalTrainingSessionFactory_HardLinkedParentIsInvalidWhenSupported()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var parentPath = CreateValidAdapterArtifactCL(
            fixture,
            "hard-link-parent.lora");
        var parent = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            parentPath,
            IncrementalTrainer.DefaultConfig);
        var secondName = Path.Combine(
            fixture.AdapterDirectory,
            "hard-link-second-name.lora");
        Assert.True(
            TryCreateHardLinkCL(secondName, parentPath, out var error),
            $"Hard-link capability changed after discovery: native error {error}.");

        var result = CreateFactoryWithParentCL(fixture, parent).Create("BUG");

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_CHECKPOINT_INVALID", result.ErrorCode);
    }

    [WindowsFactCL]
    public void IncrementalTrainingSessionFactory_WindowsIdentityPathNormalizationIsNamespaceAware()
    {
        const string drive = @"C:\Models\Parent.lora";
        const string unc = @"\\Server\Share\Models\Parent.lora";
        const string volume =
            @"\\?\Volume{aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee}\Models\Parent.lora";
        Assert.Equal(drive, NormalizeWindowsIdentityPathCL(drive));
        Assert.Equal(drive, NormalizeWindowsIdentityPathCL(@"\\?\C:\Models\Parent.lora"));
        Assert.Equal(unc, NormalizeWindowsIdentityPathCL(unc));
        Assert.Equal(
            unc,
            NormalizeWindowsIdentityPathCL(
                @"\\?\UNC\Server\Share\Models\Parent.lora"));
        Assert.Equal(
            @"\\?\Volume{AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE}\Models\Parent.lora",
            NormalizeWindowsIdentityPathCL(volume));
    }

    [WindowsFactCL]
    public void IncrementalTrainingSessionFactory_WindowsExtendedDriveParentIsAccepted()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var parentPath = CreateValidAdapterArtifactCL(
            fixture,
            "extended-drive-parent.lora");
        var parent = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            parentPath,
            IncrementalTrainer.DefaultConfig);
        var extendedPath = @"\\?\" + parentPath;

        var result = CreateFactoryWithParentCL(
                fixture,
                parent with { AdapterPath = extendedPath })
            .Create("BUG");

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
    }

    [WindowsExtendedUncFactCL]
    public void IncrementalTrainingSessionFactory_WindowsExtendedUncParentIsAcceptedWhenAvailable()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var parentPath = CreateValidAdapterArtifactCL(
            fixture,
            "extended-unc-parent.lora");
        var root = Path.GetPathRoot(parentPath)!;
        Assert.True(root.Length >= 2 && root[1] == ':');
        var relative = parentPath[root.Length..];
        var extendedUncPath = string.Concat(
            @"\\?\UNC\localhost\",
            char.ToUpperInvariant(root[0]),
            "$\\",
            relative);
        Assert.True(File.Exists(extendedUncPath));

        var parent = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            parentPath,
            IncrementalTrainer.DefaultConfig);
        var result = CreateFactoryWithParentCL(
                fixture,
                parent with { AdapterPath = extendedUncPath })
            .Create("BUG");

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
    }

    [WindowsVolumeGuidFactCL]
    public void IncrementalTrainingSessionFactory_WindowsVolumeGuidParentIsAcceptedWhenAvailable()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var parentPath = CreateValidAdapterArtifactCL(
            fixture,
            "volume-guid-parent.lora");
        var mountPoint = new StringBuilder(1024);
        var volumeName = new StringBuilder(1024);
        Assert.True(GetVolumePathNameWindowsCL(
                parentPath,
                mountPoint,
                (uint)mountPoint.Capacity));
        Assert.True(GetVolumeNameForVolumeMountPointWindowsCL(
                mountPoint.ToString(),
                volumeName,
                (uint)volumeName.Capacity));

        var relative = Path.GetRelativePath(mountPoint.ToString(), parentPath);
        var volumePath = volumeName.ToString() + relative;
        Assert.True(File.Exists(volumePath));
        var parent = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            parentPath,
            IncrementalTrainer.DefaultConfig);
        var result = CreateFactoryWithParentCL(
                fixture,
                parent with { AdapterPath = volumePath })
            .Create("BUG");

        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
    }

    [WindowsCaseSensitiveDirectoryFactCL]
    public void IncrementalTrainingSessionFactory_WindowsCaseOnlyPostOpenExchangeIsInvalidWhenSupported()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var caseDirectory = Path.Combine(
            fixture.AdapterDirectory,
            "case-sensitive-parent-boundary");
        Directory.CreateDirectory(caseDirectory);
        Assert.True(
            TryEnableWindowsCaseSensitiveDirectoryCL(caseDirectory, out var error),
            $"Case-sensitivity capability changed after discovery: Win32 error {error}.");
        var parentPath = Path.Combine(caseDirectory, "CaseParent.lora");
        var model = new LlmModel(fixture.ModelConfig);
        LoraApplicator.Apply(model, IncrementalTrainer.DefaultConfig);
        model.SaveAdapterCheckpoint(parentPath);
        var parent = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            parentPath,
            IncrementalTrainer.DefaultConfig);
        var lowerCasePath = Path.Combine(caseDirectory, "caseparent.lora");
        Assert.False(WindowsIdentityPathsEqualCL(parentPath, lowerCasePath));
        var callbackObserved = false;
        var factory = CreateFactoryWithParentCL(
            fixture,
            parent,
            (_, openedPath) =>
            {
                File.Move(openedPath, lowerCasePath);
                callbackObserved = true;
            });

        var result = factory.Create("BUG");

        Assert.True(callbackObserved);
        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_CHECKPOINT_INVALID", result.ErrorCode);
    }

    [Fact]
    public void IncrementalTrainingSessionFactory_ParentWithoutProvenanceFailsBeforeCapture()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var parentPath = CreateValidAdapterArtifactCL(fixture, "missing-parent-provenance.lora");
        var factory = CreateFactoryWithParentCL(
            fixture,
            new ModelAdapter(
                "BUG",
                parentPath,
                1,
                DateTimeOffset.UtcNow));

        var result = factory.Create("BUG");

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_PROVENANCE_REQUIRED", result.ErrorCode);
    }

    [Fact]
    public async Task IncrementalTrainer_LoadFailureOccursAfterLoraAttachmentAndMapsInvalid()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var incompatiblePath = CreateValidAdapterArtifactCL(
            fixture,
            "load-incompatible.lora",
            ["k_proj"]);
        var forgedCompatibleMetadata = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            incompatiblePath,
            IncrementalTrainer.DefaultConfig);
        var sessionResult = CreateFactoryWithParentCL(
                fixture,
                forgedCompatibleMetadata)
            .Create("BUG");
        Assert.True(sessionResult.IsSuccess, sessionResult.ErrorMessage);
        var factory = new RecordingSessionFactoryCL(
            new FixedSessionFactoryCL(sessionResult.Value!));
        var trainer = new IncrementalTrainer(
            Storage,
            factory,
            fixture.AdapterDirectory);

        var result = await trainer.TrainAsync(
            "BUG",
            [fixture.Example("BUG", "load", "failure")],
            []);

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_CHECKPOINT_INVALID", result.ErrorCode);
        var session = Assert.Single(factory.Sessions);
        Assert.NotNull(session.Model.GetAttachedProjectionAdapter(0, "q_proj"));
        Assert.False(File.Exists(Path.Combine(fixture.AdapterDirectory, "BUG.lora")));
    }

    [Fact]
    public void IncrementalTrainingParentAdapterCheckpoint_PropagatesCancellationAndFatalGraphs()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var parentPath = CreateValidAdapterArtifactCL(fixture, "graph-parent.lora");
        var provenance = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            parentPath,
            IncrementalTrainer.DefaultConfig).Provenance!;
        var cancellation = new AggregateException(
            new InvalidOperationException(
                "outer",
                new OperationCanceledException("cancelled capture")));
        var fatal = new AggregateException(
            new InvalidOperationException(
                "outer",
                new OutOfMemoryException("fatal capture")));

        Assert.Same(
            cancellation,
            Assert.Throws<AggregateException>(() =>
                new IncrementalTrainingParentAdapterCheckpoint(
                    new ThrowingCheckpointReadStreamCL(cancellation),
                    provenance)));
        Assert.Same(
            fatal,
            Assert.Throws<AggregateException>(() =>
                new IncrementalTrainingParentAdapterCheckpoint(
                    new ThrowingCheckpointReadStreamCL(fatal),
                    provenance)));
    }

    [Fact]
    public void IncrementalTrainingSessionFactory_PostOpenSeamPropagatesCancellationAndFatalGraphs()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var parentPath = CreateValidAdapterArtifactCL(
            fixture,
            "post-open-graph-parent.lora");
        var parent = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            parentPath,
            IncrementalTrainer.DefaultConfig);
        AggregateException[] graphs =
        [
            new(new InvalidOperationException(
                "outer",
                new OperationCanceledException("cancelled post-open validation"))),
            new(new InvalidOperationException(
                "outer",
                new OutOfMemoryException("fatal post-open validation"))),
        ];

        foreach (var graph in graphs)
        {
            var factory = CreateFactoryWithParentCL(
                fixture,
                parent,
                (_, _) => throw graph);

            Assert.Same(
                graph,
                Assert.Throws<AggregateException>(() => factory.Create("BUG")));
        }
    }

    [Fact]
    public void ParentAdapterCheckpointHandoff_HasNoPathOrMutableByteAuthorityAndSourceOrderIsLocked()
    {
        var checkpointType = typeof(IncrementalTrainingParentAdapterCheckpoint);
        Assert.False(checkpointType.IsPublic);
        var exposedMembers = checkpointType
            .GetMembers(
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.DeclaredOnly)
            .Where(member => member switch
            {
                FieldInfo field => !field.IsPrivate,
                MethodBase method => !method.IsPrivate,
                PropertyInfo property => property.GetMethod is { IsPrivate: false },
                _ => false,
            })
            .ToArray();
        Assert.DoesNotContain(exposedMembers, member =>
            MemberAuthorityTypeCL(member) is { } type
            && (type == typeof(string)
                || type == typeof(byte[])
                || type == typeof(FileStream)
                || type == typeof(FileInfo)
                || typeof(System.Runtime.InteropServices.SafeHandle)
                    .IsAssignableFrom(type)));
        Assert.DoesNotContain(
            exposedMembers.OfType<MethodBase>().SelectMany(
                static method => method.GetParameters()),
            parameter => parameter.ParameterType == typeof(string)
                         || parameter.ParameterType == typeof(byte[])
                         || parameter.ParameterType == typeof(FileStream)
                         || parameter.ParameterType == typeof(FileInfo)
                         || typeof(System.Runtime.InteropServices.SafeHandle)
                             .IsAssignableFrom(parameter.ParameterType));
        var openRead = Assert.Single(
            checkpointType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic),
            method => method.Name == "OpenRead");
        Assert.Equal(typeof(Stream), openRead.ReturnType);
        Assert.Empty(openRead.GetParameters());
        Assert.DoesNotContain(
            typeof(IncrementalTrainingSession).GetProperties(),
            property => property.Name is "ParentAdapterPath" or "ParentAdapterProvenance");
        Assert.NotNull(typeof(IncrementalTrainingSession).GetProperty(
            "ParentAdapterCheckpoint",
            BindingFlags.Instance | BindingFlags.NonPublic));
        Assert.Equal(
            64L * 1024 * 1024,
            IncrementalTrainingParentAdapterCheckpoint.MaximumArtifactBytes);
        var openedValidator = Assert.IsAssignableFrom<MethodInfo>(
            typeof(CheckpointBackedIncrementalTrainingSessionFactory).GetMethod(
                "ValidateOpenedParentAdapter",
                BindingFlags.Static | BindingFlags.NonPublic));
        Assert.Equal(
            typeof(FileStream),
            openedValidator.GetParameters()[0].ParameterType);

        var sessionSource = File.ReadAllText(FindRepositoryFileCL(
                "14.ContinuousLearning",
                "Training",
                "IncrementalTrainingSession.cs"))
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        var trainerSource = File.ReadAllText(FindRepositoryFileCL(
                "14.ContinuousLearning",
                "Training",
                "IncrementalTrainer.cs"))
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        Assert.DoesNotContain("ParentAdapterPath", sessionSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ParentAdapterPath", trainerSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ParentAdapterProvenance", sessionSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ParentAdapterProvenance", trainerSource, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "TrainingSourceProvenance.RequireCanonicalRegularFilePath(\n"
            + "                        parentAdapterPath)",
            sessionSource,
            StringComparison.Ordinal);
        Assert.Contains("stream.SafeFileHandle", sessionSource, StringComparison.Ordinal);
        Assert.Contains("GetFileInformationByHandleExWindows", sessionSource, StringComparison.Ordinal);
        Assert.Contains("StatxLinux", sessionSource, StringComparison.Ordinal);
        Assert.Contains("fileFlagOpenReparsePoint", sessionSource, StringComparison.Ordinal);
        Assert.Contains("openNonBlocking", sessionSource, StringComparison.Ordinal);
        Assert.Contains("openNoFollow", sessionSource, StringComparison.Ordinal);
        Assert.Contains("openCloseOnExec", sessionSource, StringComparison.Ordinal);
        Assert.Contains("NormalizeWindowsIdentityPath", sessionSource, StringComparison.Ordinal);
        Assert.Contains("ulong Length", sessionSource, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "new FileStream(\n                        parentAdapterPath,",
            sessionSource,
            StringComparison.Ordinal);
        Assert.Equal(
            1,
            CountOccurrencesCL(
                sessionSource,
                "return new FileStream(\n"
                + "                ownedHandle,"));
        Assert.Equal(
            4,
            CountOccurrencesCL(
                sessionSource,
                "ValidateOpenedParentAdapter("));
        var sourceOpenIndex = sessionSource.IndexOf(
            "parentStream = OpenParentAdapterRead(parentAdapterPath);",
            StringComparison.Ordinal);
        var exchangeSeamIndex = sessionSource.IndexOf(
            "_afterParentAdapterOpenForTesting?.Invoke(",
            StringComparison.Ordinal);
        var openedValidationIndex = sessionSource.IndexOf(
            "var openedIdentity = ValidateOpenedParentAdapter(",
            StringComparison.Ordinal);
        var captureIndex = sessionSource.IndexOf(
            "new IncrementalTrainingParentAdapterCheckpoint(\n"
            + "                                    parentStream,",
            StringComparison.Ordinal);
        var revalidationIndex = sessionSource.IndexOf(
            "_ = ValidateOpenedParentAdapter(",
            StringComparison.Ordinal);
        Assert.True(sourceOpenIndex >= 0 && sourceOpenIndex < exchangeSeamIndex);
        Assert.True(exchangeSeamIndex < openedValidationIndex);
        Assert.True(openedValidationIndex < captureIndex);
        Assert.True(captureIndex < revalidationIndex);
        const string applyNeedle =
            "var adapters = LoraApplicator.Apply(\n"
            + "                        session.Model,\n"
            + "                        _loraConfig);";
        const string openLoadNeedle =
            "using var parentStream = parentAdapterCheckpoint.OpenRead();\n"
            + "                            session.Model.LoadAdapterCheckpoint(parentStream);";
        var applyIndex = trainerSource.IndexOf(
            applyNeedle,
            StringComparison.Ordinal);
        var openIndex = trainerSource.IndexOf(
            "parentAdapterCheckpoint.OpenRead()",
            StringComparison.Ordinal);
        var loadIndex = trainerSource.IndexOf(
            "session.Model.LoadAdapterCheckpoint(parentStream)",
            StringComparison.Ordinal);
        Assert.Contains(openLoadNeedle, trainerSource, StringComparison.Ordinal);
        Assert.True(
            applyIndex >= 0 && applyIndex < openIndex,
            "The LoRA adapter must be applied before opening the parent checkpoint.");
        Assert.True(
            openIndex >= 0 && openIndex < loadIndex,
            "The parent checkpoint must be opened before it is loaded.");

        var linuxStatx = typeof(CheckpointBackedIncrementalTrainingSessionFactory)
            .GetNestedType("LinuxStatx", BindingFlags.NonPublic);
        Assert.NotNull(linuxStatx);
        Assert.Equal(256, Marshal.SizeOf(linuxStatx));
        Assert.Equal(136, Marshal.OffsetOf(linuxStatx, "DeviceMajor").ToInt32());
        Assert.Equal(140, Marshal.OffsetOf(linuxStatx, "DeviceMinor").ToInt32());
    }

    [Fact]
    public async Task IncrementalTrainer_IncompatibleRegisteredParentProvenance_FailsBeforeLoraApply()
    {
        using var fixture = new TinyTrainingFixtureCL();
        Directory.CreateDirectory(fixture.AdapterDirectory);
        string incompatibleParent = CreateValidAdapterArtifactCL(
            fixture,
            "parent-k-only.lora",
            ["k_proj"]);
        var registration = fixture.Registry.Register(
            CreateAdapterRegistrationCL(
                fixture,
                "BUG",
                incompatibleParent,
                new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4f,
                    Dropout = 0f,
                    TargetModules = ["k_proj"],
                }));
        Assert.True(registration.IsSuccess, registration.ErrorMessage);
        var factory = new RecordingSessionFactoryCL(fixture.CreateFactory());
        var trainer = new IncrementalTrainer(Storage, factory, fixture.AdapterDirectory);

        var result = await trainer.TrainAsync(
            "BUG",
            [fixture.Example("BUG", "fix", "fixed")],
            []);

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_PROVENANCE_MISMATCH", result.ErrorCode);
        var session = Assert.Single(factory.Sessions);
        Assert.Null(session.Model.GetAttachedProjectionAdapter(0, "q_proj"));
        Assert.False(File.Exists(Path.Combine(fixture.AdapterDirectory, "BUG.lora")));
    }

    [Fact]
    public void IncrementalTrainingSessionFactory_RejectsCustomRegistryDomainMismatch()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var adapterPath = CreateValidAdapterArtifactCL(
            fixture,
            "mismatched-domain.lora");
        var factory = new CheckpointBackedIncrementalTrainingSessionFactory(
            fixture.ModelConfig,
            fixture.BaseCheckpointPath,
            fixture.TokenizerPath,
            new FixedResultRegistryCL(
                OperationResult<ModelAdapter?>.Ok(
                    new ModelAdapter(
                        "PLAN",
                        adapterPath,
                        1,
                        DateTimeOffset.UtcNow))),
            fixture.TrainingConfig);

        var result = factory.Create("BUG");

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_DOMAIN_MISMATCH", result.ErrorCode);
    }

    [Fact]
    public void IncrementalTrainingSessionFactory_PropagatesRegistryArtifactFailure()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var factory = new CheckpointBackedIncrementalTrainingSessionFactory(
            fixture.ModelConfig,
            fixture.BaseCheckpointPath,
            fixture.TokenizerPath,
            new FixedResultRegistryCL(
                OperationResult<ModelAdapter?>.Fail(
                    "artifact disappeared",
                    "ADAPTER_CHECKPOINT_NOT_FOUND")),
            fixture.TrainingConfig);

        var result = factory.Create("BUG");

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_CHECKPOINT_NOT_FOUND", result.ErrorCode);
        Assert.Equal("artifact disappeared", result.ErrorMessage);
    }

    [Fact]
    public void IncrementalTrainingSessionFactory_UsesStableMissingArtifactCodes()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var missingBase = new CheckpointBackedIncrementalTrainingSessionFactory(
            fixture.ModelConfig,
            fixture.BaseCheckpointPath + ".missing",
            fixture.TokenizerPath,
            fixture.Registry,
            fixture.TrainingConfig);
        var missingTokenizer = new CheckpointBackedIncrementalTrainingSessionFactory(
            fixture.ModelConfig,
            fixture.BaseCheckpointPath,
            fixture.TokenizerPath + ".missing",
            fixture.Registry,
            fixture.TrainingConfig);

        Assert.Equal("BASE_CHECKPOINT_NOT_FOUND", missingBase.Create("BUG").ErrorCode);
        Assert.Equal(
            "TOKENIZER_CHECKPOINT_NOT_FOUND",
            missingTokenizer.Create("BUG").ErrorCode);
    }

    [Fact]
    public void IncrementalTrainingSessionFactory_NormalizesOnlyNonFatalExceptionGraphs()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var nonFatalFactory = new CheckpointBackedIncrementalTrainingSessionFactory(
            fixture.ModelConfig,
            fixture.BaseCheckpointPath,
            fixture.TokenizerPath,
            new ThrowingRegistryCL(new InvalidOperationException("sensitive detail")),
            fixture.TrainingConfig);
        var cancellationFactory = new CheckpointBackedIncrementalTrainingSessionFactory(
            fixture.ModelConfig,
            fixture.BaseCheckpointPath,
            fixture.TokenizerPath,
            new ThrowingRegistryCL(
                new AggregateException(
                    new InvalidOperationException("outer"),
                    new OperationCanceledException("cancelled"))),
            fixture.TrainingConfig);

        var normalized = nonFatalFactory.Create("BUG");
        Assert.False(normalized.IsSuccess);
        Assert.Equal("TRAINING_SESSION_CREATION_FAILED", normalized.ErrorCode);
        Assert.DoesNotContain("sensitive detail", normalized.ErrorMessage);
        Assert.Throws<AggregateException>(() => cancellationFactory.Create("BUG"));
    }

    [Fact]
    public async Task IncrementalTrainer_InvalidExamples_FailBeforeSessionCreation()
    {
        var invalidRequests = new (string Domain, TrainingExample Example)[]
        {
            ("BUG", new TrainingExample("", "response", "BUG", 1d, DateTimeOffset.UtcNow)),
            ("BUG", new TrainingExample("prompt", "", "BUG", 1d, DateTimeOffset.UtcNow)),
            ("BUG", new TrainingExample("prompt", "response", "PLAN", 1d, DateTimeOffset.UtcNow)),
            ("BUG", new TrainingExample("prompt", "response", "BUG", 0d, DateTimeOffset.UtcNow)),
            ("BUG", new TrainingExample("prompt", "response", "BUG", double.NaN, DateTimeOffset.UtcNow)),
            ("../BUG", new TrainingExample("prompt", "response", "../BUG", 1d, DateTimeOffset.UtcNow)),
            ("CON", new TrainingExample("prompt", "response", "CON", 1d, DateTimeOffset.UtcNow)),
            ("BUG", null!),
        };

        foreach (var request in invalidRequests)
        {
            var factory = new CountingSessionFactoryCL();
            var trainer = new IncrementalTrainer(Storage, factory);

            var result = await trainer.TrainAsync(
                request.Domain,
                [request.Example],
                []);

            Assert.False(result.IsSuccess);
            Assert.Equal("INVALID_TRAINING_EXAMPLE", result.ErrorCode);
            Assert.Equal(0, factory.CreateCount);
        }
    }

    [Fact]
    public async Task IncrementalTrainer_ContextOverflow_FailsBeforeLoraMutation()
    {
        using var fixture = new TinyTrainingFixtureCL(contextLength: 2);
        var factory = new RecordingSessionFactoryCL(fixture.CreateFactory());
        var trainer = new IncrementalTrainer(Storage, factory, fixture.AdapterDirectory);
        var longPrompt = string.Join(' ', Enumerable.Repeat("tiny", 20));
        Assert.True(BpeTokenizer.Load(fixture.TokenizerPath).Encode(
            string.Concat(longPrompt, "\n", "long")).Count
            > fixture.ModelConfig.ContextLen + 1);

        var result = await trainer.TrainAsync(
            "BUG",
            [fixture.Example("BUG", longPrompt, "long")],
            []);

        Assert.False(result.IsSuccess);
        Assert.Equal("TRAINING_EXAMPLE_CONTEXT_EXCEEDED", result.ErrorCode);
        var session = Assert.Single(factory.Sessions);
        Assert.Null(session.Model.GetAttachedProjectionAdapter(0, "q_proj"));
        Assert.False(File.Exists(Path.Combine(fixture.AdapterDirectory, "BUG.lora")));
        Assert.False(File.Exists(Path.Combine(fixture.AdapterDirectory, "registry.json")));
        Assert.Empty(Directory.EnumerateFiles(
            fixture.AdapterDirectory,
            ".registry.json.*.tmp"));
        Assert.Empty(Directory.EnumerateFiles(
            fixture.AdapterDirectory,
            ".registry.json.*.bak"));
    }

    [Fact]
    public async Task IncrementalTrainer_FinalPathDirectoryCollision_PreservesSentinelAndLeavesNoTemp()
    {
        using var fixture = new TinyTrainingFixtureCL();
        Directory.CreateDirectory(fixture.AdapterDirectory);
        string target = Path.Combine(fixture.AdapterDirectory, "BUG.lora");
        Directory.CreateDirectory(target);
        string sentinelPath = Path.Combine(target, "sentinel.bin");
        byte[] sentinelBytes = [7, 8, 9, 10];
        File.WriteAllBytes(sentinelPath, sentinelBytes);
        var trainer = new IncrementalTrainer(
            Storage,
            fixture.CreateFactory(),
            fixture.AdapterDirectory);

        var result = await trainer.TrainAsync(
            "BUG",
            [fixture.Example("BUG", "fix", "fixed")],
            []);

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_CHECKPOINT_SAVE_FAILED", result.ErrorCode);
        Assert.True(Directory.Exists(target));
        Assert.True(File.Exists(sentinelPath));
        Assert.Equal(sentinelBytes, File.ReadAllBytes(sentinelPath));
        Assert.Empty(Directory.GetFiles(
            fixture.AdapterDirectory,
            $"{Path.GetFileName(target)}.*.tmp"));
    }

    // ── Phase 5: DomainModelRegistry ─────────────────────────────────────────

    [Fact]
    public void DomainModelRegistry_Register_StoresAdapter()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var reg = new DomainModelRegistry(
            Storage,
            fixture.AdapterDirectory);
        var artifact = CreateValidAdapterArtifactCL(fixture, "bug.lora");
        var nonCanonicalPath = Path.Combine(
            Path.GetDirectoryName(artifact)!,
            ".",
            Path.GetFileName(artifact));
        var adapter = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            nonCanonicalPath,
            NoDropoutLoraConfig(),
            trainingExamples: 120);

        var registration = reg.Register(adapter);
        var result = reg.GetAdapter("BUG");

        Assert.True(registration.IsSuccess, registration.ErrorMessage);
        Assert.True(registration.Value);
        Assert.True(result.IsSuccess);
        Assert.Equal("BUG", result.Value!.Domain);
        Assert.Equal(Path.GetFullPath(artifact), result.Value.AdapterPath);
    }

    [Fact]
    public void DomainModelRegistry_GetAdapter_UnknownDomain_ReturnsNull()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var reg = new DomainModelRegistry(
            Storage,
            fixture.AdapterDirectory);
        var result = reg.GetAdapter("UNKNOWN_DOMAIN");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    [Fact]
    public void DomainModelRegistry_ListAll_ReturnsAllRegistered()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var reg = new DomainModelRegistry(
            Storage,
            fixture.AdapterDirectory);
        foreach (var domain in new[] { "BUG", "CODE_Q", "REFACTOR" })
        {
            var artifact = CreateValidAdapterArtifactCL(
                fixture,
                $"{domain}.lora");
            var registration = reg.Register(
                CreateAdapterRegistrationCL(
                    fixture,
                    domain,
                    artifact,
                    NoDropoutLoraConfig(),
                    trainingExamples: 10));
            Assert.True(registration.IsSuccess, registration.ErrorMessage);
        }

        var listed = reg.ListAll();

        Assert.True(listed.IsSuccess, listed.ErrorMessage);
        Assert.Equal(
            ["BUG", "CODE_Q", "REFACTOR"],
            listed.Value!.Select(adapter => adapter.Domain).ToArray());
    }

    [Fact]
    public void DomainModelRegistry_RestartRestoresFullAdapterAndNestedProvenance()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var artifact = CreateValidAdapterArtifactCL(fixture, "restart.lora");
        var adapter = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            artifact,
            NoDropoutLoraConfig(),
            trainingExamples: 37) with
        {
            TrainedAt = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)),
        };
        var first = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        Assert.True(first.Register(adapter).IsSuccess);
        var normalized = first.GetAdapter("BUG").Value!;

        var restarted = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        var restored = restarted.GetAdapter("BUG");

        Assert.True(restored.IsSuccess, restored.ErrorMessage);
        Assert.Equal(TimeSpan.Zero, normalized.TrainedAt.Offset);
        Assert.Equal(
            JsonSerializer.Serialize(normalized),
            JsonSerializer.Serialize(restored.Value));
        Assert.True(File.Exists(Path.Combine(fixture.AdapterDirectory, "registry.json")));
    }

    [Fact]
    public void DomainModelRegistry_TwoPreexistingInstancesMergeDistinctDomainsAndRefreshReads()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var first = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        var second = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        var bugPath = CreateValidAdapterArtifactCL(fixture, "multi-writer-bug.lora");
        var planPath = CreateValidAdapterArtifactCL(fixture, "multi-writer-plan.lora");

        Assert.True(first.Register(CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            bugPath,
            NoDropoutLoraConfig())).IsSuccess);
        Assert.True(second.Register(CreateAdapterRegistrationCL(
            fixture,
            "PLAN",
            planPath,
            NoDropoutLoraConfig())).IsSuccess);

        Assert.NotNull(first.GetAdapter("PLAN").Value);
        Assert.NotNull(second.GetAdapter("BUG").Value);
        var restarted = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        Assert.Equal(
            ["BUG", "PLAN"],
            restarted.ListAll().Value!.Select(value => value.Domain).ToArray());
    }

    [Fact]
    public async Task DomainModelRegistry_ConcurrentDistinctDomainWritersSurviveRestart()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var first = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        var second = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        var bug = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            CreateValidAdapterArtifactCL(fixture, "concurrent-bug.lora"),
            NoDropoutLoraConfig());
        var plan = CreateAdapterRegistrationCL(
            fixture,
            "PLAN",
            CreateValidAdapterArtifactCL(fixture, "concurrent-plan.lora"),
            NoDropoutLoraConfig());

        var results = await Task.WhenAll(
            Task.Run(() => first.Register(bug)),
            Task.Run(() => second.Register(plan)));

        Assert.All(results, result => Assert.True(result.IsSuccess, result.ErrorMessage));
        var restarted = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        Assert.Equal(
            ["BUG", "PLAN"],
            restarted.ListAll().Value!.Select(value => value.Domain).ToArray());
    }

    [Fact]
    public async Task DomainModelRegistry_DpoBatchHoldsBoundaryThroughCommitCallback()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var transactionRegistry = new DomainModelRegistry(
            Storage,
            fixture.AdapterDirectory);
        var legacyRegistry = new DomainModelRegistry(
            Storage,
            fixture.AdapterDirectory);
        var receipt = TrainingSourceProvenance.BuildDpoPublicationReceipt(
            [new string('a', 64)],
            ["BUG"]).Value!;
        var bug = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            CreateValidAdapterArtifactCL(fixture, "dpo-batch-bug.lora"),
            NoDropoutLoraConfig()) with
        {
            Provenance = CreateAdapterRegistrationCL(
                    fixture,
                    "BUG",
                    CreateValidAdapterArtifactCL(
                        fixture,
                        "dpo-batch-bug-provenance.lora"),
                    NoDropoutLoraConfig())
                .Provenance! with
                {
                    AdapterSha256 = HashArtifactCL(Path.Combine(
                        fixture.AdapterDirectory,
                        "dpo-batch-bug.lora")),
                    DpoPublication = receipt,
                },
        };
        var plan = CreateAdapterRegistrationCL(
            fixture,
            "PLAN",
            CreateValidAdapterArtifactCL(fixture, "dpo-batch-plan.lora"),
            NoDropoutLoraConfig());
        using var callbackEntered = new ManualResetEventSlim();
        using var releaseCallback = new ManualResetEventSlim();
        var legacyAttempted = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var publication = Task.Run(() =>
            ((IDpoRegistryPublicationTransaction)transactionRegistry)
            .PublishBatchAndCommit(
                [bug],
                _ =>
                {
                    callbackEntered.Set();
                    if (!releaseCallback.Wait(TimeSpan.FromSeconds(10)))
                    {
                        return OperationResult<bool>.Fail(
                            "The test commit callback timed out.",
                            "TEST_TIMEOUT");
                    }
                    return OperationResult<bool>.Ok(true);
                }));
        Assert.True(callbackEntered.Wait(TimeSpan.FromSeconds(10)));
        var legacyWrite = Task.Run(() =>
        {
            legacyAttempted.SetResult(true);
            return legacyRegistry.Register(plan);
        });
        await legacyAttempted.Task.WaitAsync(TimeSpan.FromSeconds(10));

        try
        {
            await Task.Delay(150);
            Assert.False(legacyWrite.IsCompleted);
        }
        finally
        {
            releaseCallback.Set();
        }

        var published = await publication.WaitAsync(TimeSpan.FromSeconds(10));
        var registered = await legacyWrite.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.True(published.IsSuccess, published.ErrorMessage);
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        Assert.Equal(
            ["BUG", "PLAN"],
            new DomainModelRegistry(Storage, fixture.AdapterDirectory)
                .ListAll().Value!
                .Select(static value => value.Domain)
                .ToArray());
    }

    [WindowsFactCL]
    public void DomainModelRegistry_OsLeaseContentionFailsClosedWithoutPublication()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException(
                "Same-process FileStream lock contention is a Windows-only registry proof.");

        using var fixture = new TinyTrainingFixtureCL();
        _ = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        var contender = new DomainModelRegistry(
            Storage,
            fixture.AdapterDirectory,
            beforeFinalPublicationValidation: null,
            processLeaseTimeout: TimeSpan.FromMilliseconds(150));
        var adapter = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            CreateValidAdapterArtifactCL(fixture, "lease-contention.lora"),
            NoDropoutLoraConfig());
        var lockPath = Path.Combine(fixture.AdapterDirectory, ".registry.json.lock");

        OperationResult<bool> result;
        // Same-process byte-range locks are the Windows share/handle oracle.
        // Linux portability is proven by the neighboring cross-process lock test,
        // because Linux does not make this in-process FileStream lock contend
        // with the registry writer in the same way.
        using (var lease = new FileStream(
                   lockPath,
                   FileMode.Open,
                   FileAccess.ReadWrite,
                   FileShare.ReadWrite))
        {
            lease.Lock(0, 1);
            try
            {
                result = contender.Register(adapter);
            }
            finally
            {
                lease.Unlock(0, 1);
            }
        }

        Assert.False(result.IsSuccess);
        Assert.Equal("REGISTRY_PERSISTENCE_FAILED", result.ErrorCode);
        Assert.False(File.Exists(Path.Combine(fixture.AdapterDirectory, "registry.json")));
    }

    [Fact]
    public async Task DomainModelRegistry_CrossProcessByteRangeLockTimesOutWithoutPublicationThenRecovers()
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
            return;

        using var fixture = new TinyTrainingFixtureCL();
        var registry = new DomainModelRegistry(
            Storage,
            fixture.AdapterDirectory,
            beforeFinalPublicationValidation: null,
            processLeaseTimeout: TimeSpan.FromMilliseconds(350));
        var adapter = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            CreateValidAdapterArtifactCL(fixture, "cross-process-lock.lora"),
            NoDropoutLoraConfig());
        var lockPath = Path.Combine(fixture.AdapterDirectory, ".registry.json.lock");
        var helperPath = ResolveRegistryLockHolderPath();
        var dotnetHost = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH")
                         ?? Environment.ProcessPath;
        Assert.False(string.IsNullOrWhiteSpace(dotnetHost));
        Assert.True(File.Exists(dotnetHost), $"dotnet host not found: {dotnetHost}");
        Assert.True(File.Exists(helperPath), $"lock helper not built: {helperPath}");

        using var child = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = dotnetHost,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            },
        };
        child.StartInfo.ArgumentList.Add(helperPath);
        child.StartInfo.ArgumentList.Add(lockPath);
        Assert.True(child.Start());

        try
        {
            var ready = await child.StandardOutput
                .ReadLineAsync()
                .WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Equal("READY", ready);
            Assert.False(child.HasExited);

            var elapsed = Stopwatch.StartNew();
            var blocked = registry.Register(adapter);
            elapsed.Stop();

            Assert.False(blocked.IsSuccess);
            Assert.Equal("REGISTRY_PERSISTENCE_FAILED", blocked.ErrorCode);
            Assert.InRange(
                elapsed.Elapsed,
                TimeSpan.FromMilliseconds(250),
                TimeSpan.FromSeconds(5));
            Assert.False(File.Exists(Path.Combine(fixture.AdapterDirectory, "registry.json")));
            Assert.Empty(Directory.EnumerateFiles(
                fixture.AdapterDirectory,
                ".registry.json.*.tmp"));
            Assert.Empty(Directory.EnumerateFiles(
                fixture.AdapterDirectory,
                ".registry.json.*.bak"));
        }
        finally
        {
            if (!child.HasExited)
            {
                await child.StandardInput.WriteLineAsync("RELEASE");
                await child.StandardInput.FlushAsync();
                try
                {
                    await child.WaitForExitAsync()
                        .WaitAsync(TimeSpan.FromSeconds(10));
                }
                catch (TimeoutException)
                {
                    child.Kill(entireProcessTree: true);
                    await child.WaitForExitAsync();
                }
            }
        }

        Assert.Equal(0, child.ExitCode);
        Assert.True(registry.Register(adapter).IsSuccess);
        var restarted = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        var recovered = restarted.GetAdapter("BUG");
        Assert.True(recovered.IsSuccess, recovered.ErrorMessage);
        Assert.NotNull(recovered.Value);
        Assert.NotNull(recovered.Value.Provenance);
        Assert.Equal(
            adapter.Provenance!.AdapterSha256,
            recovered.Value.Provenance!.AdapterSha256);
        Assert.Equal(
            adapter.Provenance.TrainingSources.Sha256,
            recovered.Value.Provenance.TrainingSources.Sha256);
        Assert.Equal(adapter.Provenance.Model.Sha256, recovered.Value.Provenance.Model.Sha256);
        Assert.Equal(adapter.Provenance.Lora.Sha256, recovered.Value.Provenance.Lora.Sha256);
    }

    [Fact]
    public void DomainModelRegistry_ConstructionRemovesOnlyExactStalePublicationArtifacts()
    {
        using var fixture = new TinyTrainingFixtureCL();
        Directory.CreateDirectory(fixture.AdapterDirectory);
        var staleTemp = Path.Combine(
            fixture.AdapterDirectory,
            $".registry.json.{Guid.NewGuid():N}.tmp");
        var staleBackup = Path.Combine(
            fixture.AdapterDirectory,
            $".registry.json.{Guid.NewGuid():N}.bak");
        var unrelated = Path.Combine(fixture.AdapterDirectory, ".registry.json.keep.tmp");
        File.WriteAllText(staleTemp, "partial");
        File.WriteAllBytes(staleBackup, []);
        File.WriteAllText(unrelated, "unrelated");

        _ = new DomainModelRegistry(Storage, fixture.AdapterDirectory);

        Assert.False(File.Exists(staleTemp));
        Assert.False(File.Exists(staleBackup));
        Assert.True(File.Exists(unrelated));
    }

    [Fact]
    public void DomainModelRegistry_ConstructionRollsBackCrashBackupBeforeLoading()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var registry = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        Assert.True(registry.Register(CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            CreateValidAdapterArtifactCL(fixture, "crash-prior.lora"),
            NoDropoutLoraConfig())).IsSuccess);
        var registryPath = Path.Combine(fixture.AdapterDirectory, "registry.json");
        var priorBytes = File.ReadAllBytes(registryPath);

        Assert.True(registry.Register(CreateAdapterRegistrationCL(
            fixture,
            "PLAN",
            CreateValidAdapterArtifactCL(fixture, "crash-uncommitted.lora"),
            NoDropoutLoraConfig())).IsSuccess);
        var crashBackup = Path.Combine(
            fixture.AdapterDirectory,
            $".registry.json.{Guid.NewGuid():N}.bak");
        File.WriteAllBytes(crashBackup, priorBytes);

        var recovered = new DomainModelRegistry(Storage, fixture.AdapterDirectory);

        Assert.NotNull(recovered.GetAdapter("BUG").Value);
        Assert.Null(recovered.GetAdapter("PLAN").Value);
        Assert.False(File.Exists(crashBackup));
    }

    [Fact]
    public void DomainModelRegistry_StrictParserRejectsDuplicateCasedAndTrailingJson()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var registry = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        var artifact = CreateValidAdapterArtifactCL(fixture, "strict-json.lora");
        Assert.True(registry.Register(CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            artifact,
            NoDropoutLoraConfig())).IsSuccess);
        var registryPath = Path.Combine(fixture.AdapterDirectory, "registry.json");
        var canonical = File.ReadAllText(registryPath);
        var mutations = new[]
        {
            canonical.Replace(
                "{\"SchemaVersion\":1,",
                "{\"SchemaVersion\":1,\"SchemaVersion\":1,",
                StringComparison.Ordinal),
            canonical.Replace("\"Domain\":", "\"domain\":", StringComparison.Ordinal),
            string.Concat(canonical, "{}"),
        };

        foreach (var mutation in mutations)
        {
            File.WriteAllText(registryPath, mutation);
            AssertRegistryRestartFails(fixture.AdapterDirectory, "REGISTRY_STATE_CORRUPT");
        }
    }

    [Fact]
    public void DomainModelRegistry_Version1RestartDoesNotRewriteAndRegistrationStaysVersion1()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var registry = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        Assert.True(registry.Register(CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            CreateValidAdapterArtifactCL(fixture, "version1-bug.lora"),
            NoDropoutLoraConfig())).IsSuccess);
        var registryPath = Path.Combine(fixture.AdapterDirectory, "registry.json");
        var priorBytes = File.ReadAllBytes(registryPath);

        _ = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        Assert.Equal(priorBytes, File.ReadAllBytes(registryPath));

        Assert.True(registry.Register(CreateAdapterRegistrationCL(
            fixture,
            "PLAN",
            CreateValidAdapterArtifactCL(fixture, "version1-plan.lora"),
            NoDropoutLoraConfig())).IsSuccess);
        using var document = JsonDocument.Parse(File.ReadAllBytes(registryPath));
        Assert.Equal(1, document.RootElement.GetProperty("SchemaVersion").GetInt32());
        Assert.All(
            document.RootElement.GetProperty("Adapters").EnumerateArray(),
            adapter =>
            {
                Assert.True(adapter.TryGetProperty("AdapterPath", out _));
                Assert.False(adapter.TryGetProperty("StorageKind", out _));
            });
    }

    [Fact]
    public void DomainModelRegistry_Version2LegacyLoadsMutatesAndRestartsWithoutAdapterPath()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var registryPath = SeedVersion2LegacyRegistryCL(fixture, "BUG", "version2-bug.lora");
        var registry = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        Assert.NotNull(registry.GetAdapter("BUG").Value);

        var registered = registry.Register(CreateAdapterRegistrationCL(
            fixture,
            "PLAN",
            CreateValidAdapterArtifactCL(fixture, "version2-plan.lora"),
            NoDropoutLoraConfig()));

        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        var bytes = File.ReadAllBytes(registryPath);
        Assert.DoesNotContain("\"AdapterPath\":", Encoding.UTF8.GetString(bytes), StringComparison.Ordinal);
        using (var document = JsonDocument.Parse(bytes))
        {
            Assert.Equal(2, document.RootElement.GetProperty("SchemaVersion").GetInt32());
            Assert.All(
                document.RootElement.GetProperty("Adapters").EnumerateArray(),
                adapter =>
                {
                    Assert.Equal(1, adapter.GetProperty("StorageKind").GetInt32());
                    Assert.True(adapter.TryGetProperty("LegacyAbsolutePath", out _));
                    Assert.False(adapter.TryGetProperty("RetainedReference", out _));
                });
        }
        var restarted = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        Assert.Equal(
            ["BUG", "PLAN"],
            restarted.ListAll().Value!.Select(adapter => adapter.Domain).ToArray());
    }

    [Fact]
    public void DomainModelRegistry_Version2PathFailureRestoresExactPriorBytes()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var registryPath = SeedVersion2LegacyRegistryCL(fixture, "BUG", "version2-prior.lora");
        var priorBytes = File.ReadAllBytes(registryPath);
        var callbackCount = 0;
        var candidatePublicationObserved = false;
        Exception? publicationInspectionFailure = null;
        var registry = new DomainModelRegistry(
            Storage,
            fixture.AdapterDirectory,
            () =>
            {
                callbackCount++;
                try
                {
                    var candidateBytes = File.ReadAllBytes(registryPath);
                    Assert.False(candidateBytes.AsSpan().SequenceEqual(priorBytes));
                    AssertVersion2LegacyRegistryDomainsCL(candidateBytes, "BUG", "PLAN");
                    var backupPath = Assert.Single(Directory.EnumerateFiles(
                        fixture.AdapterDirectory,
                        ".registry.json.*.bak"));
                    AssertVersion2LegacyRegistryDomainsCL(priorBytes, "BUG");
                    Assert.Equal(priorBytes, File.ReadAllBytes(backupPath));
                    candidatePublicationObserved = true;
                }
                catch (Exception exception)
                {
                    publicationInspectionFailure = exception;
                }
                throw new InvalidDataException("Injected version2 rejection.");
            });

        var result = registry.Register(CreateAdapterRegistrationCL(
            fixture,
            "PLAN",
            CreateValidAdapterArtifactCL(fixture, "version2-rejected.lora"),
            NoDropoutLoraConfig()));

        Assert.False(result.IsSuccess);
        Assert.Equal("REGISTRY_PERSISTENCE_FAILED", result.ErrorCode);
        Assert.Equal(1, callbackCount);
        Assert.Null(publicationInspectionFailure);
        Assert.True(candidatePublicationObserved);
        Assert.Equal(priorBytes, File.ReadAllBytes(registryPath));
        Assert.NotNull(new DomainModelRegistry(Storage, fixture.AdapterDirectory)
            .GetAdapter("BUG").Value);
    }

    [Fact]
    public void DomainModelRegistry_ValidRetainedReferenceIsQuarantinedWithoutResidueOrMutation()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var registryPath = SeedVersion2LegacyRegistryCL(fixture, "BUG", "retained-quarantine.lora");
        var liveRegistry = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        var root = JsonNode.Parse(File.ReadAllText(registryPath))!.AsObject();
        var adapter = root["Adapters"]!.AsArray()[0]!.AsObject();
        var provenanceSha = adapter["Provenance"]!["AdapterSha256"]!.GetValue<string>();
        adapter.Remove("LegacyAbsolutePath");
        adapter["StorageKind"] = 2;
        adapter["RetainedReference"] = CreateValidAdapterReferenceNodeCL(provenanceSha);
        var bytes = Encoding.UTF8.GetBytes(root.ToJsonString());
        File.WriteAllBytes(registryPath, bytes);

        var registration = liveRegistry.Register(CreateAdapterRegistrationCL(
            fixture,
            "PLAN",
            CreateValidAdapterArtifactCL(fixture, "retained-quarantine-plan.lora"),
            NoDropoutLoraConfig()));
        Assert.False(registration.IsSuccess);
        Assert.Equal(
            "REGISTRY_RETAINED_REFERENCE_WRITE_UNSUPPORTED",
            registration.ErrorCode);
        AssertRegistryRestartFails(
            fixture.AdapterDirectory,
            "ADAPTER_REFERENCE_RESOLVER_REQUIRED");
        Assert.Equal(bytes, File.ReadAllBytes(registryPath));
        Assert.Empty(Directory.EnumerateFiles(
            fixture.AdapterDirectory,
            ".registry.json.*.tmp"));
        Assert.Empty(Directory.EnumerateFiles(
            fixture.AdapterDirectory,
            ".registry.json.*.bak"));
    }

    [Theory]
    [InlineData("reference-version")]
    [InlineData("reference-kind")]
    [InlineData("reference-length")]
    [InlineData("registry-kind")]
    [InlineData("registry-version")]
    public void DomainModelRegistry_InvalidVersion2ReferenceFieldsKindsAndVersionsAreCorrupt(
        string mutation)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var registryPath = SeedVersion2LegacyRegistryCL(fixture, "BUG", "invalid-reference.lora");
        var root = JsonNode.Parse(File.ReadAllText(registryPath))!.AsObject();
        var adapter = root["Adapters"]!.AsArray()[0]!.AsObject();
        var provenanceSha = adapter["Provenance"]!["AdapterSha256"]!.GetValue<string>();
        adapter.Remove("LegacyAbsolutePath");
        adapter["StorageKind"] = 2;
        var reference = CreateValidAdapterReferenceNodeCL(provenanceSha);
        adapter["RetainedReference"] = reference;
        switch (mutation)
        {
            case "reference-version": reference["schema_version"] = 2; break;
            case "reference-kind": reference["storage_kind"] = 2; break;
            case "reference-length": reference["length"] = 0; break;
            case "registry-kind": adapter["StorageKind"] = 3; break;
            case "registry-version": root["SchemaVersion"] = 3; break;
        }
        File.WriteAllText(registryPath, root.ToJsonString());

        AssertRegistryRestartFails(fixture.AdapterDirectory, "REGISTRY_STATE_CORRUPT");
    }

    [Fact]
    public void DomainModelRegistry_RetainedReferenceHashMustBindToProvenance()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var registryPath = SeedVersion2LegacyRegistryCL(fixture, "BUG", "hash-binding.lora");
        var root = JsonNode.Parse(File.ReadAllText(registryPath))!.AsObject();
        var adapter = root["Adapters"]!.AsArray()[0]!.AsObject();
        adapter.Remove("LegacyAbsolutePath");
        adapter["StorageKind"] = 2;
        adapter["RetainedReference"] = CreateValidAdapterReferenceNodeCL(new string('a', 64));
        File.WriteAllText(registryPath, root.ToJsonString());

        AssertRegistryRestartFails(fixture.AdapterDirectory, "REGISTRY_STATE_CORRUPT");
    }

    [Fact]
    public void DomainModelRegistry_Version2StorageUnionRejectsEveryNonExclusiveShape()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var registryPath = SeedVersion2LegacyRegistryCL(fixture, "BUG", "union-matrix.lora");
        var validRoot = JsonNode.Parse(File.ReadAllText(registryPath))!.AsObject();
        var provenanceSha = validRoot["Adapters"]!.AsArray()[0]!["Provenance"]!["AdapterSha256"]!
            .GetValue<string>();
        var mutations = new Action<JsonObject>[]
        {
            adapter => adapter.Remove("StorageKind"),
            adapter => adapter.Remove("LegacyAbsolutePath"),
            adapter => adapter["RetainedReference"] = CreateValidAdapterReferenceNodeCL(provenanceSha),
            adapter => { adapter["StorageKind"] = 2; adapter.Remove("LegacyAbsolutePath"); },
            adapter => { adapter["StorageKind"] = 2; adapter["RetainedReference"] = CreateValidAdapterReferenceNodeCL(provenanceSha); },
            adapter => { adapter["StorageKind"] = 2; adapter["LegacyAbsolutePath"] = null; },
            adapter => adapter["LegacyAbsolutePath"] = null,
            adapter => adapter["UnknownUnionField"] = "x",
            adapter => { adapter.Remove("StorageKind"); adapter["storageKind"] = 1; },
        };

        foreach (var mutate in mutations)
        {
            var root = validRoot.DeepClone().AsObject();
            mutate(root["Adapters"]!.AsArray()[0]!.AsObject());
            File.WriteAllText(registryPath, root.ToJsonString());
            AssertRegistryRestartFails(fixture.AdapterDirectory, "REGISTRY_STATE_CORRUPT");
        }

        var duplicate = validRoot.ToJsonString().Replace(
            "\"StorageKind\":1,",
            "\"StorageKind\":1,\"StorageKind\":1,",
            StringComparison.Ordinal);
        File.WriteAllText(registryPath, duplicate);
        AssertRegistryRestartFails(fixture.AdapterDirectory, "REGISTRY_STATE_CORRUPT");
    }

    [Fact]
    public void DomainModelRegistry_HasNoReferenceRegistrationOrResolverApi()
    {
        var methods = typeof(DomainModelRegistry).GetMethods(
            BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.Public
            | BindingFlags.NonPublic);
        var publicApis = methods.Where(method => method.IsPublic).ToArray();

        Assert.DoesNotContain(methods, method =>
            method.ReturnType == typeof(AdapterArtifactReference)
            || method.GetParameters().Any(parameter =>
                parameter.ParameterType == typeof(AdapterArtifactReference)));
        Assert.DoesNotContain(publicApis, method =>
            method.Name.Contains("Reference", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("Resolve", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DomainModelRegistry_AdapterMutationDuringPublicationRollsBackDurableAndMemoryState()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var firstPath = CreateValidAdapterArtifactCL(fixture, "mutation-first.lora");
        var initial = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        Assert.True(initial.Register(CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            firstPath,
            NoDropoutLoraConfig(),
            trainingExamples: 1)).IsSuccess);

        var secondPath = CreateValidAdapterArtifactCL(fixture, "mutation-second.lora");
        var registry = new DomainModelRegistry(
            Storage,
            fixture.AdapterDirectory,
            () => File.AppendAllText(secondPath, "mutated-during-publication"));
        var result = registry.Register(CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            secondPath,
            NoDropoutLoraConfig(),
            trainingExamples: 2));

        Assert.False(result.IsSuccess);
        Assert.Equal("REGISTRY_PERSISTENCE_FAILED", result.ErrorCode);
        Assert.Equal(1, registry.GetAdapter("BUG").Value!.TrainingExamples);
        var restarted = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        Assert.Equal(1, restarted.GetAdapter("BUG").Value!.TrainingExamples);
    }

    [Fact]
    public void DomainModelRegistry_RollbackBlockedAfterReplaceRetainsRecoveryBackup()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var initial = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        Assert.True(initial.Register(CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            CreateValidAdapterArtifactCL(fixture, "blocked-rollback-prior.lora"),
            NoDropoutLoraConfig(),
            trainingExamples: 1)).IsSuccess);
        var registryPath = Path.Combine(fixture.AdapterDirectory, "registry.json");
        var priorBytes = File.ReadAllBytes(registryPath);
        var failing = new DomainModelRegistry(
            Storage,
            fixture.AdapterDirectory,
            () =>
            {
                throw new InvalidDataException("Injected post-publication failure.");
            },
            beforeRollbackForTesting: () =>
            {
                // The rollback failure is injected at the registry seam instead
                // of relying on Windows-only FileShare behavior, so both Windows
                // and Linux keep the same recovery-backup evidence.
                throw new IOException("Injected rollback failure for deterministic recovery evidence.");
            });

        var result = failing.Register(CreateAdapterRegistrationCL(
            fixture,
            "PLAN",
            CreateValidAdapterArtifactCL(fixture, "blocked-rollback-candidate.lora"),
            NoDropoutLoraConfig()));

        Assert.False(result.IsSuccess);
        Assert.Equal("REGISTRY_PERSISTENCE_FAILED", result.ErrorCode);
        Assert.Single(Directory.EnumerateFiles(
            fixture.AdapterDirectory,
            ".registry.json.*.bak"));

        // Do not read through the failing instance here: any registry read is
        // allowed to perform recovery. The assertion below proves the restart
        // path consumes the durable backup and restores the prior adapter.
        var recovered = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        Assert.Equal(priorBytes, File.ReadAllBytes(registryPath));
        Assert.NotNull(recovered.GetAdapter("BUG").Value);
        Assert.Null(recovered.GetAdapter("PLAN").Value);
        Assert.Empty(Directory.EnumerateFiles(
            fixture.AdapterDirectory,
            ".registry.json.*.bak"));
    }

    [Fact]
    public void DomainModelRegistry_RollbackBlockedAfterFirstPublishRetainsAbsenceMarker()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var registryPath = Path.Combine(fixture.AdapterDirectory, "registry.json");
        var failing = new DomainModelRegistry(
            Storage,
            fixture.AdapterDirectory,
            () =>
            {
                throw new InvalidDataException("Injected first-publication failure.");
            },
            beforeRollbackForTesting: () =>
            {
                // First publication uses a zero-byte absence marker; this seam
                // blocks rollback after publication so the restart path must
                // prove that absence is recovered on every supported platform.
                throw new IOException("Injected rollback failure for deterministic absence-marker evidence.");
            });

        var result = failing.Register(CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            CreateValidAdapterArtifactCL(fixture, "blocked-first-candidate.lora"),
            NoDropoutLoraConfig()));

        Assert.False(result.IsSuccess);
        var marker = Assert.Single(Directory.EnumerateFiles(
            fixture.AdapterDirectory,
            ".registry.json.*.bak"));
        Assert.Equal(0, new FileInfo(marker).Length);

        var recovered = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        Assert.Null(recovered.GetAdapter("BUG").Value);
        Assert.False(File.Exists(registryPath));
        Assert.Empty(Directory.EnumerateFiles(
            fixture.AdapterDirectory,
            ".registry.json.*.bak"));
    }

    [Fact]
    public void DomainModelRegistry_RegisterSnapshotsCallerOwnedNestedProvenance()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var artifact = CreateValidAdapterArtifactCL(fixture, "mutable.lora");
        var adapter = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            artifact,
            NoDropoutLoraConfig());
        var mutableSources = adapter.Provenance!.TrainingSources.SourceIds.ToList();
        var mutableTargets = adapter.Provenance.Lora.OrderedTargetModules.ToList();
        adapter = adapter with
        {
            Provenance = adapter.Provenance with
            {
                TrainingSources = adapter.Provenance.TrainingSources with
                {
                    SourceIds = mutableSources,
                },
                Lora = adapter.Provenance.Lora with
                {
                    OrderedTargetModules = mutableTargets,
                },
            },
        };
        var registry = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        Assert.True(registry.Register(adapter).IsSuccess);

        mutableSources[0] = "not-a-source-id";
        mutableTargets.Clear();
        var stored = registry.GetAdapter("BUG");

        Assert.True(stored.IsSuccess, stored.ErrorMessage);
        Assert.True(TrainingSourceProvenance.IsCanonicalSourceId(
            Assert.Single(stored.Value!.Provenance!.TrainingSources.SourceIds)));
        Assert.Equal(["q_proj", "v_proj"], stored.Value.Provenance.Lora.OrderedTargetModules);
    }

    [Fact]
    public void DomainModelRegistry_WindowsPublishFailureRollsBackMemoryAndDurableState()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var fixture = new TinyTrainingFixtureCL();
        var registry = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        var firstPath = CreateValidAdapterArtifactCL(fixture, "first.lora");
        var first = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            firstPath,
            NoDropoutLoraConfig(),
            trainingExamples: 1);
        Assert.True(registry.Register(first).IsSuccess);
        var registryPath = Path.Combine(fixture.AdapterDirectory, "registry.json");
        var secondPath = CreateValidAdapterArtifactCL(fixture, "second.lora");
        var second = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            secondPath,
            NoDropoutLoraConfig(),
            trainingExamples: 2);

        OperationResult<bool> failed;
        using (var lease = new FileStream(
                   registryPath,
                   FileMode.Open,
                   FileAccess.Read,
                   FileShare.None))
        {
            failed = registry.Register(second);
        }

        Assert.False(failed.IsSuccess);
        Assert.Equal("REGISTRY_PERSISTENCE_FAILED", failed.ErrorCode);
        Assert.Equal(1, registry.GetAdapter("BUG").Value!.TrainingExamples);
        var restarted = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        Assert.Equal(1, restarted.GetAdapter("BUG").Value!.TrainingExamples);
    }

    [Fact]
    public void DomainModelRegistry_RestartRejectsTruncatedDuplicateAndNoncanonicalState()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var registry = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        var artifact = CreateValidAdapterArtifactCL(fixture, "state.lora");
        Assert.True(registry.Register(CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            artifact,
            NoDropoutLoraConfig())).IsSuccess);
        var registryPath = Path.Combine(fixture.AdapterDirectory, "registry.json");
        var canonicalJson = File.ReadAllText(registryPath);

        File.WriteAllText(registryPath, "{");
        AssertRegistryRestartFails(fixture.AdapterDirectory, "REGISTRY_STATE_CORRUPT");

        var duplicate = JsonNode.Parse(canonicalJson)!.AsObject();
        var duplicateAdapters = duplicate["Adapters"]!.AsArray();
        duplicateAdapters.Add(duplicateAdapters[0]!.DeepClone());
        File.WriteAllText(registryPath, duplicate.ToJsonString());
        AssertRegistryRestartFails(fixture.AdapterDirectory, "REGISTRY_STATE_CORRUPT");

        var noncanonical = JsonNode.Parse(canonicalJson)!.AsObject();
        var storedAdapter = noncanonical["Adapters"]!.AsArray()[0]!.AsObject();
        storedAdapter["AdapterPath"] = Path.Combine(
            Path.GetDirectoryName(artifact)!,
            ".",
            Path.GetFileName(artifact));
        File.WriteAllText(registryPath, noncanonical.ToJsonString());
        AssertRegistryRestartFails(fixture.AdapterDirectory, "REGISTRY_STATE_CORRUPT");
    }

    [Theory]
    [InlineData(false, "ADAPTER_CHECKPOINT_NOT_FOUND")]
    [InlineData(true, "ADAPTER_PROVENANCE_MISMATCH")]
    public void DomainModelRegistry_RestartRejectsMissingOrTamperedAdapter(
        bool tamper,
        string expectedCode)
    {
        using var fixture = new TinyTrainingFixtureCL();
        var registry = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        var artifact = CreateValidAdapterArtifactCL(fixture, "restart-invalid.lora");
        Assert.True(registry.Register(CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            artifact,
            NoDropoutLoraConfig())).IsSuccess);
        if (tamper)
            File.AppendAllText(artifact, "tampered");
        else
            File.Delete(artifact);

        AssertRegistryRestartFails(fixture.AdapterDirectory, expectedCode);
    }

    [Fact]
    public void DomainModelRegistry_RejectsInvalidMetadataAndArtifactsWithStableCodes()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var reg = new DomainModelRegistry(
            Storage,
            fixture.AdapterDirectory);
        var valid = CreateValidAdapterArtifactCL(fixture, "valid.lora");
        var ownedRelativeRoot = Path.Combine(
            Directory.GetCurrentDirectory(),
            $".continuous-learning-relative-{Guid.NewGuid():N}");
        var relativeArtifactFullPath = Path.Combine(
            ownedRelativeRoot,
            "relative-valid.lora");
        var relativeArtifactSpelling = Path.Combine(
            ".",
            Path.GetFileName(ownedRelativeRoot),
            Path.GetFileName(relativeArtifactFullPath));
        try
        {
            Directory.CreateDirectory(ownedRelativeRoot);
            var relativeModel = new LlmModel(fixture.ModelConfig);
            _ = LoraApplicator.Apply(relativeModel, NoDropoutLoraConfig());
            relativeModel.SaveAdapterCheckpoint(relativeArtifactFullPath);
            Assert.True(File.Exists(relativeArtifactFullPath));
            Assert.False(Path.IsPathFullyQualified(relativeArtifactSpelling));

            var relativeResult = reg.Register(
                new ModelAdapter(
                    "BUG",
                    relativeArtifactSpelling,
                    1,
                    DateTimeOffset.UtcNow));
            Assert.False(relativeResult.IsSuccess);
            Assert.Equal("ADAPTER_PATH_NOT_ABSOLUTE", relativeResult.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(ownedRelativeRoot))
                Directory.Delete(ownedRelativeRoot, recursive: true);
        }

        var directoryPath = Path.Combine(fixture.RootDirectory, "directory.lora");
        Directory.CreateDirectory(directoryPath);
        var zeroPath = Path.Combine(fixture.RootDirectory, "zero.lora");
        File.WriteAllBytes(zeroPath, []);
        var corruptPath = Path.Combine(fixture.RootDirectory, "corrupt.lora");
        File.WriteAllBytes(corruptPath, [1, 2, 3, 4]);
        var fullPath = Path.Combine(fixture.RootDirectory, "full.bin");
        new LlmModel(fixture.ModelConfig).SaveCheckpoint(fullPath);
        var missingPath = Path.Combine(fixture.RootDirectory, "missing.lora");

        var nullResult = reg.Register(null!);
        Assert.False(nullResult.IsSuccess);
        Assert.Equal("NULL_ADAPTER", nullResult.ErrorCode);

        var invalidCount = reg.Register(
            CreateAdapterRegistrationCL(
                fixture,
                "BUG",
                valid,
                NoDropoutLoraConfig()) with
            {
                TrainingExamples = 0,
            });
        Assert.False(invalidCount.IsSuccess);
        Assert.Equal("ADAPTER_TRAINING_METADATA_INVALID", invalidCount.ErrorCode);

        var invalidTimestamp = reg.Register(
            CreateAdapterRegistrationCL(
                fixture,
                "BUG",
                valid,
                NoDropoutLoraConfig()) with
            {
                TrainedAt = default,
            });
        Assert.False(invalidTimestamp.IsSuccess);
        Assert.Equal("ADAPTER_TRAINING_METADATA_INVALID", invalidTimestamp.ErrorCode);

        var cases = new (ModelAdapter Adapter, string Code)[]
        {
            (new ModelAdapter("", valid, 1, DateTimeOffset.UtcNow),
                "INVALID_ADAPTER_DOMAIN"),
            (CreateAdapterRegistrationCL(fixture, "BUG", missingPath, NoDropoutLoraConfig()),
                "ADAPTER_CHECKPOINT_NOT_FOUND"),
            (CreateAdapterRegistrationCL(fixture, "BUG", directoryPath, NoDropoutLoraConfig()),
                "ADAPTER_CHECKPOINT_INVALID"),
            (CreateAdapterRegistrationCL(fixture, "BUG", zeroPath, NoDropoutLoraConfig()),
                "ADAPTER_CHECKPOINT_INVALID"),
            (CreateAdapterRegistrationCL(fixture, "BUG", corruptPath, NoDropoutLoraConfig()),
                "ADAPTER_CHECKPOINT_INVALID"),
            (CreateAdapterRegistrationCL(fixture, "BUG", fullPath, NoDropoutLoraConfig()),
                "ADAPTER_CHECKPOINT_INVALID"),
        };

        foreach (var (adapter, expectedCode) in cases)
        {
            var result = reg.Register(adapter);
            Assert.False(result.IsSuccess);
            Assert.Equal(expectedCode, result.ErrorCode);
        }
    }

    [Fact]
    public void DomainModelRegistry_InvalidOverwritePreservesPreviouslyValidatedAdapter()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var reg = new DomainModelRegistry(
            Storage,
            fixture.AdapterDirectory);
        var valid = CreateValidAdapterArtifactCL(fixture, "valid-a.lora");
        var first = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            valid,
            NoDropoutLoraConfig());
        Assert.True(reg.Register(first).IsSuccess);
        var invalid = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            Path.Combine(fixture.RootDirectory, "missing-b.lora"),
            NoDropoutLoraConfig(),
            trainingExamples: 2);

        var replacement = reg.Register(invalid);
        var retained = reg.GetAdapter("BUG");

        Assert.False(replacement.IsSuccess);
        Assert.Equal("ADAPTER_CHECKPOINT_NOT_FOUND", replacement.ErrorCode);
        Assert.True(retained.IsSuccess, retained.ErrorMessage);
        Assert.Equal(Path.GetFullPath(valid), retained.Value!.AdapterPath);
        Assert.Equal(1, retained.Value.TrainingExamples);
    }

    [Fact]
    public void DomainModelRegistry_RevalidatesStoredArtifactsOnGetAndList()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var deletedRegistry = new DomainModelRegistry(
            Storage,
            fixture.AdapterDirectory);
        var deletedPath = CreateValidAdapterArtifactCL(fixture, "deleted.lora");
        Assert.True(deletedRegistry.Register(
            CreateAdapterRegistrationCL(
                fixture,
                "BUG",
                deletedPath,
                NoDropoutLoraConfig())).IsSuccess);
        File.Delete(deletedPath);

        var missingGet = deletedRegistry.GetAdapter("BUG");
        var missingList = deletedRegistry.ListAll();
        Assert.False(missingGet.IsSuccess);
        Assert.Equal("ADAPTER_CHECKPOINT_NOT_FOUND", missingGet.ErrorCode);
        Assert.False(missingList.IsSuccess);
        Assert.Equal("ADAPTER_CHECKPOINT_NOT_FOUND", missingList.ErrorCode);

        using var corruptFixture = new TinyTrainingFixtureCL();
        var corruptRegistry = new DomainModelRegistry(
            Storage,
            corruptFixture.AdapterDirectory);
        var corruptPath = CreateValidAdapterArtifactCL(corruptFixture, "corrupted.lora");
        Assert.True(corruptRegistry.Register(
            CreateAdapterRegistrationCL(
                corruptFixture,
                "PLAN",
                corruptPath,
                NoDropoutLoraConfig())).IsSuccess);
        File.WriteAllBytes(corruptPath, [1, 2, 3, 4]);

        var invalidGet = corruptRegistry.GetAdapter("PLAN");
        var invalidList = corruptRegistry.ListAll();
        Assert.False(invalidGet.IsSuccess);
        Assert.Equal("ADAPTER_PROVENANCE_MISMATCH", invalidGet.ErrorCode);
        Assert.False(invalidList.IsSuccess);
        Assert.Equal("ADAPTER_PROVENANCE_MISMATCH", invalidList.ErrorCode);
    }

    [UnreadableRegularFileFactCL]
    public void DomainModelRegistryAndSession_UnreadableRegularArtifact_IsInvalidNotMissing()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var artifactPath = CreateValidAdapterArtifactCL(
            fixture,
            "unreadable.lora");
        var adapter = CreateAdapterRegistrationCL(
            fixture,
            "BUG",
            artifactPath,
            NoDropoutLoraConfig());
        var registry = new DomainModelRegistry(
            Storage,
            fixture.AdapterDirectory);
        var sessionFactory = new CheckpointBackedIncrementalTrainingSessionFactory(
            fixture.ModelConfig,
            fixture.BaseCheckpointPath,
            fixture.TokenizerPath,
            new FixedResultRegistryCL(
                OperationResult<ModelAdapter?>.Ok(adapter)),
            fixture.TrainingConfig);

        string strategy;
        using (var unreadable = new UnreadableAdapterArtifactLeaseCL(artifactPath))
        {
            strategy = unreadable.Strategy;
            var registration = registry.Register(adapter);
            var session = sessionFactory.Create("BUG");

            Assert.False(registration.IsSuccess);
            Assert.Equal("ADAPTER_CHECKPOINT_INVALID", registration.ErrorCode);
            Assert.False(session.IsSuccess);
            Assert.Equal("ADAPTER_CHECKPOINT_INVALID", session.ErrorCode);
            Assert.True(File.Exists(artifactPath));
        }

        Assert.True(
            OperatingSystem.IsWindows()
                ? strategy == "windows-share-none"
                : strategy == "unix-mode-none",
            $"Unexpected unreadable-artifact strategy: {strategy}");
        LlmModel.ValidateAdapterCheckpointArtifact(artifactPath);
    }

    [ReparsePointFactCL]
    public void DomainModelRegistryAndSession_ReparseAdapterArtifact_IsInvalidNotMissing()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var targetPath = CreateValidAdapterArtifactCL(
            fixture,
            "reparse-target.lora");
        var linkPath = Path.Combine(
            fixture.AdapterDirectory,
            "reparse-link.lora");
        File.CreateSymbolicLink(linkPath, targetPath);

        try
        {
            Assert.True(
                (File.GetAttributes(linkPath) & FileAttributes.ReparsePoint) != 0);
            var adapter = CreateAdapterRegistrationCL(
                fixture,
                "BUG",
                linkPath,
                NoDropoutLoraConfig());
            var registry = new DomainModelRegistry(
                Storage,
                fixture.AdapterDirectory);
            var sessionFactory = new CheckpointBackedIncrementalTrainingSessionFactory(
                fixture.ModelConfig,
                fixture.BaseCheckpointPath,
                fixture.TokenizerPath,
                new FixedResultRegistryCL(
                    OperationResult<ModelAdapter?>.Ok(adapter)),
                fixture.TrainingConfig);

            var registration = registry.Register(adapter);
            var session = sessionFactory.Create("BUG");

            Assert.False(registration.IsSuccess);
            Assert.Equal("ADAPTER_CHECKPOINT_INVALID", registration.ErrorCode);
            Assert.False(session.IsSuccess);
            Assert.Equal("ADAPTER_CHECKPOINT_INVALID", session.ErrorCode);
            Assert.True(File.Exists(targetPath));
        }
        finally
        {
            if (File.Exists(linkPath))
                File.Delete(linkPath);
        }
    }

    [Fact]
    public void ReparseCapabilityClassifier_RecognizesOnlyPreciseCapabilityFailures()
    {
        Assert.Equal(
            unchecked((int)0x80070522),
            ReparsePointTestSupportCL.HResultPrivilegeNotHeld);
        Assert.Equal(
            unchecked((int)0x80070032),
            ReparsePointTestSupportCL.HResultOperationNotSupported);
        Assert.Equal(
            unchecked((int)0x80070001),
            ReparsePointTestSupportCL.HResultInvalidFunction);
        Assert.Equal(
            unchecked((int)0x80070005),
            ReparsePointTestSupportCL.HResultAccessDenied);
        Assert.True(ReparsePointTestSupportCL.IsRecognizedCapabilityLimitation(
            new SyntheticIOExceptionCL(unchecked((int)0x80070522)),
            isWindows: true));
        Assert.True(ReparsePointTestSupportCL.IsRecognizedCapabilityLimitation(
            new SyntheticIOExceptionCL(unchecked((int)0x80070032)),
            isWindows: true));
        Assert.True(ReparsePointTestSupportCL.IsRecognizedCapabilityLimitation(
            new SyntheticUnauthorizedAccessExceptionCL(
                unchecked((int)0x80070005)),
            isWindows: true));
        Assert.True(ReparsePointTestSupportCL.IsRecognizedCapabilityLimitation(
            new SyntheticIOExceptionCL(unchecked((int)0x80070001)),
            isWindows: true));
        Assert.True(ReparsePointTestSupportCL.IsRecognizedCapabilityLimitation(
            new PlatformNotSupportedException("symbolic links are unavailable"),
            isWindows: false));
        Assert.True(ReparsePointTestSupportCL.IsRecognizedCapabilityLimitation(
            new NotSupportedException("symbolic links are unavailable"),
            isWindows: false));
    }

    [Fact]
    public void ReparseCapabilityClassifier_RejectsUnexpectedIoAndAccessFailures()
    {
        Assert.False(ReparsePointTestSupportCL.IsRecognizedCapabilityLimitation(
            new IOException("ordinary I/O failure"),
            isWindows: true));
        Assert.False(ReparsePointTestSupportCL.IsRecognizedCapabilityLimitation(
            new SyntheticIOExceptionCL(unchecked((int)0x80070020)),
            isWindows: true));
        Assert.False(ReparsePointTestSupportCL.IsRecognizedCapabilityLimitation(
            new SyntheticUnauthorizedAccessExceptionCL(
                unchecked((int)0x80070020)),
            isWindows: true));
        Assert.False(ReparsePointTestSupportCL.IsRecognizedCapabilityLimitation(
            new SyntheticUnauthorizedAccessExceptionCL(
                unchecked((int)0x80070522)),
            isWindows: false));
    }

    // ── Phase 5: IContinuousLearningPipeline interface ────────────────────────

    [Fact]
    public void ContinuousLearning_IContinuousLearningPipeline_InterfaceExists()
    {
        // Gate B: IContinuousLearningPipeline declared in Fabric/Interfaces.cs for 16.AutonomousLearning.
        var type = typeof(IContinuousLearningPipeline);
        Assert.NotNull(type);
    }

    [Fact]
    public async Task ContinuousLearningPipeline_PropagatesExactTrainerFailure_WithoutRegistration()
    {
        var expected = new TrainingExample(
            "prompt", "response", "BUG", 1d, DateTimeOffset.UtcNow);
        var registry = new CountingRegistryCL();
        var pipeline = new ContinuousLearningPipeline(
            Storage,
            new FixedIndexerCL([expected]),
            new InMemoryCacheCL(),
            new EmptyFeedbackRouterCL(),
            new FailingIncrementalTrainerCL(
                "training failed exactly",
                "SUPERVISED_ADAPTER_TRAINING_FAILED"),
            registry);

        var result = await pipeline.RunCycleAsync("project");

        Assert.False(result.IsSuccess);
        Assert.Equal("SUPERVISED_ADAPTER_TRAINING_FAILED", result.ErrorCode);
        Assert.Equal("training failed exactly", result.ErrorMessage);
        Assert.Equal(0, registry.RegisterCount);
    }

    [Fact]
    public async Task ContinuousLearningPipeline_DoesNotRegisterFalseGreenNullAdapter()
    {
        var expected = new TrainingExample(
            "prompt", "response", "BUG", 1d, DateTimeOffset.UtcNow);
        var registry = new CountingRegistryCL();
        var pipeline = new ContinuousLearningPipeline(
            Storage,
            new FixedIndexerCL([expected]),
            new InMemoryCacheCL(),
            new EmptyFeedbackRouterCL(),
            new NullSuccessIncrementalTrainerCL(),
            registry);

        var result = await pipeline.RunCycleAsync("project");

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_CHECKPOINT_INCOMPLETE", result.ErrorCode);
        Assert.Equal(0, registry.RegisterCount);
    }

    [Fact]
    public async Task ContinuousLearningPipeline_TransactionalNullAdapterUsesStableCheckpointFailure()
    {
        var expected = new TrainingExample(
            "prompt", "response", "BUG", 1d, DateTimeOffset.UtcNow);
        var cache = new InMemoryCacheCL();
        var router = BuildFeedbackRouter(cache);
        var requestId = new string('1', 64);
        RouteTrainingReadyRejectedFeedbackCL(
            router,
            requestId);
        var pendingBefore = router.GetDpoPairs();
        Assert.True(pendingBefore.IsSuccess, pendingBefore.ErrorMessage);
        var routedPair = Assert.Single(pendingBefore.Value!);
        var registry = new CountingRegistryCL();
        var pipeline = new ContinuousLearningPipeline(
            Storage,
            new FixedIndexerCL([expected]),
            cache,
            router,
            new FixedTransactionalResultIncrementalTrainerCL(
                OperationResult<TrainedAdapterArtifact>.Ok(null!)),
            registry);

        var result = await pipeline.RunCycleAsync("project");
        var pendingAfter = router.GetDpoPairs();

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_CHECKPOINT_INCOMPLETE", result.ErrorCode);
        Assert.Equal(0, cache.FlushCount);
        Assert.Equal(0, registry.RegisterCount);
        Assert.True(pendingAfter.IsSuccess, pendingAfter.ErrorMessage);
        var releasedPair = Assert.Single(pendingAfter.Value!);
        Assert.Equal(routedPair.Domain, releasedPair.Domain);
        Assert.Equal("BUG", releasedPair.Domain);
        Assert.Equal(
            routedPair.TeacherProvenance!.RequestId,
            releasedPair.TeacherProvenance!.RequestId);
        Assert.Equal(requestId, releasedPair.TeacherProvenance.RequestId);
        Assert.Equal(
            routedPair.TeacherProvenance.TrainingContentSha256,
            releasedPair.TeacherProvenance.TrainingContentSha256);
        Assert.False(string.IsNullOrWhiteSpace(
            releasedPair.TeacherProvenance.TrainingContentSha256));
        Assert.Equal(routedPair.Prompt, releasedPair.Prompt);
        Assert.Equal(routedPair.Chosen, releasedPair.Chosen);
        Assert.Equal(routedPair.Rejected, releasedPair.Rejected);
        Assert.Equal(routedPair.CapturedAt, releasedPair.CapturedAt);
        Assert.Equal(routedPair.TeacherProvenance, releasedPair.TeacherProvenance);
        Assert.Equal(routedPair.GovernanceDecision, releasedPair.GovernanceDecision);
    }

    [Fact]
    public async Task ContinuousLearningPipeline_TransactionalTrainerFailurePropagatesExactly()
    {
        var expected = new TrainingExample(
            "prompt", "response", "BUG", 1d, DateTimeOffset.UtcNow);
        var cache = new InMemoryCacheCL();
        var router = BuildFeedbackRouter(cache);
        var requestId = new string('2', 64);
        RouteTrainingReadyRejectedFeedbackCL(
            router,
            requestId);
        var pendingBefore = router.GetDpoPairs();
        Assert.True(pendingBefore.IsSuccess, pendingBefore.ErrorMessage);
        var routedPair = Assert.Single(pendingBefore.Value!);
        var registry = new CountingRegistryCL();
        var pipeline = new ContinuousLearningPipeline(
            Storage,
            new FixedIndexerCL([expected]),
            cache,
            router,
            new FixedTransactionalResultIncrementalTrainerCL(
                OperationResult<TrainedAdapterArtifact>.Fail(
                    "transactional training failed exactly",
                    "TRANSACTIONAL_TRAINING_FAILED_EXACT")),
            registry);

        var result = await pipeline.RunCycleAsync("project");
        var pendingAfter = router.GetDpoPairs();

        Assert.False(result.IsSuccess);
        Assert.Equal("TRANSACTIONAL_TRAINING_FAILED_EXACT", result.ErrorCode);
        Assert.Equal("transactional training failed exactly", result.ErrorMessage);
        Assert.Equal(0, cache.FlushCount);
        Assert.Equal(0, registry.RegisterCount);
        Assert.True(pendingAfter.IsSuccess, pendingAfter.ErrorMessage);
        var releasedPair = Assert.Single(pendingAfter.Value!);
        Assert.Equal(routedPair.Domain, releasedPair.Domain);
        Assert.Equal("BUG", releasedPair.Domain);
        Assert.Equal(
            routedPair.TeacherProvenance!.RequestId,
            releasedPair.TeacherProvenance!.RequestId);
        Assert.Equal(requestId, releasedPair.TeacherProvenance.RequestId);
        Assert.Equal(
            routedPair.TeacherProvenance.TrainingContentSha256,
            releasedPair.TeacherProvenance.TrainingContentSha256);
        Assert.False(string.IsNullOrWhiteSpace(
            releasedPair.TeacherProvenance.TrainingContentSha256));
        Assert.Equal(routedPair.Prompt, releasedPair.Prompt);
        Assert.Equal(routedPair.Chosen, releasedPair.Chosen);
        Assert.Equal(routedPair.Rejected, releasedPair.Rejected);
        Assert.Equal(routedPair.CapturedAt, releasedPair.CapturedAt);
        Assert.Equal(routedPair.TeacherProvenance, releasedPair.TeacherProvenance);
        Assert.Equal(routedPair.GovernanceDecision, releasedPair.GovernanceDecision);
    }

    [Fact]
    public async Task ContinuousLearningPipeline_InvalidArtifactCannotBeRegistered_AfterDurableFlush()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var invalidPath = Path.Combine(fixture.RootDirectory, "invalid.lora");
        File.WriteAllBytes(invalidPath, [1, 2, 3, 4]);
        var registry = new DomainModelRegistry(
            Storage,
            fixture.AdapterDirectory);
        var cache = new InMemoryCacheCL();
        var pipeline = new ContinuousLearningPipeline(
            Storage,
            new FixedIndexerCL(
            [
                new TrainingExample(
                    "prompt",
                    "response",
                    "BUG",
                    1d,
                    DateTimeOffset.UtcNow),
            ]),
            cache,
            new EmptyFeedbackRouterCL(),
            new FixedPathIncrementalTrainerCL(invalidPath),
            registry);

        var result = await pipeline.RunCycleAsync("project");
        var lookup = registry.GetAdapter("BUG");

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_CHECKPOINT_INVALID", result.ErrorCode);
        Assert.Equal(1, cache.FlushCount);
        Assert.True(lookup.IsSuccess, lookup.ErrorMessage);
        Assert.Null(lookup.Value);
    }

    [Fact]
    public async Task ContinuousLearningPipeline_RequiresSuccessfulExactRegistrationReadback()
    {
        var candidatePath = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            $"continuous-candidate-{Guid.NewGuid():N}.lora"));
        var expected = new TrainingExample(
            "prompt", "response", "BUG", 1d, DateTimeOffset.UtcNow);
        var cases = new[]
        {
            new ScriptedRegistryCL(
                OperationResult<bool>.Ok(false),
                OperationResult<ModelAdapter?>.Ok(null)),
            new ScriptedRegistryCL(
                OperationResult<bool>.Ok(true),
                OperationResult<ModelAdapter?>.Ok(null)),
            new ScriptedRegistryCL(
                OperationResult<bool>.Ok(true),
                OperationResult<ModelAdapter?>.Ok(
                    new ModelAdapter(
                        "BUG",
                        candidatePath + ".different",
                        1,
                        DateTimeOffset.UtcNow))),
            new ScriptedRegistryCL(
                OperationResult<bool>.Ok(true),
                OperationResult<ModelAdapter?>.Ok(
                    new ModelAdapter(
                        "PLAN",
                        candidatePath,
                        1,
                        DateTimeOffset.UtcNow))),
        };

        foreach (var registry in cases)
        {
            var cache = new InMemoryCacheCL();
            var pipeline = new ContinuousLearningPipeline(
                Storage,
                new FixedIndexerCL([expected]),
                cache,
                new EmptyFeedbackRouterCL(),
                new FixedPathIncrementalTrainerCL(candidatePath),
                registry);

            var result = await pipeline.RunCycleAsync("project");

            Assert.False(result.IsSuccess);
            Assert.Equal("ADAPTER_REGISTRATION_INCOMPLETE", result.ErrorCode);
            Assert.Equal(1, cache.FlushCount);
        }
    }

    [Fact]
    public async Task ContinuousLearningPipeline_RegisterFalseUsesStableIncompleteFailure()
    {
        var cache = new InMemoryCacheCL();
        var pipeline = BuildSingleDomainPipelineCL(
            cache,
            new FixedPathIncrementalTrainerCL(
                SingleAdapterPathCL()["BUG"]),
            new ScriptedRegistryCL(
                OperationResult<bool>.Ok(false),
                OperationResult<ModelAdapter?>.Ok(null)));

        var result = await pipeline.RunCycleAsync("project");

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_REGISTRATION_INCOMPLETE", result.ErrorCode);
        Assert.Equal("Adapter registration did not complete.", result.ErrorMessage);
        Assert.Equal(1, cache.FlushCount);
    }

    [Fact]
    public async Task ContinuousLearningPipeline_RegisterFailurePropagatesExactly()
    {
        var cache = new InMemoryCacheCL();
        var pipeline = BuildSingleDomainPipelineCL(
            cache,
            new FixedPathIncrementalTrainerCL(
                SingleAdapterPathCL()["BUG"]),
            new ScriptedRegistryCL(
                OperationResult<bool>.Fail(
                    "registration failed exactly",
                    "REGISTRATION_FAILED_EXACT"),
                OperationResult<ModelAdapter?>.Ok(null)));

        var result = await pipeline.RunCycleAsync("project");

        Assert.False(result.IsSuccess);
        Assert.Equal("REGISTRATION_FAILED_EXACT", result.ErrorCode);
        Assert.Equal("registration failed exactly", result.ErrorMessage);
        Assert.Equal(1, cache.FlushCount);
    }

    [Fact]
    public async Task ContinuousLearningPipeline_CacheAddFailure_PropagatesAndPreventsTrainingPublicationAndFlush()
    {
        var events = new List<string>();
        var cache = new ControlledCacheCL(
            OperationResult<bool>.Fail(
                "cache add failed exactly",
                "CACHE_ADD_FAILED_EXACT"),
            OperationResult<bool>.Ok(true),
            events);
        var registry = new CountingRegistryCL();
        var pipeline = BuildSingleDomainPipelineCL(
            cache,
            new RecordingPathIncrementalTrainerCL(
                events,
                SingleAdapterPathCL()),
            registry);

        var result = await pipeline.RunCycleAsync("project");

        Assert.False(result.IsSuccess);
        Assert.Equal("CACHE_ADD_FAILED_EXACT", result.ErrorCode);
        Assert.Equal("cache add failed exactly", result.ErrorMessage);
        Assert.Equal(1, cache.AddCount);
        Assert.Equal(0, cache.FlushCount);
        Assert.Equal(0, registry.RegisterCount);
        Assert.Empty(events);
    }

    [Fact]
    public async Task ContinuousLearningPipeline_CacheAddIncomplete_PreventsTrainingPublicationAndFlush()
    {
        var events = new List<string>();
        var cache = new ControlledCacheCL(
            OperationResult<bool>.Ok(false),
            OperationResult<bool>.Ok(true),
            events);
        var registry = new CountingRegistryCL();
        var pipeline = BuildSingleDomainPipelineCL(
            cache,
            new RecordingPathIncrementalTrainerCL(
                events,
                SingleAdapterPathCL()),
            registry);

        var result = await pipeline.RunCycleAsync("project");

        Assert.False(result.IsSuccess);
        Assert.Equal("CACHE_ADD_INCOMPLETE", result.ErrorCode);
        Assert.Equal(1, cache.AddCount);
        Assert.Equal(0, cache.FlushCount);
        Assert.Equal(0, registry.RegisterCount);
        Assert.Empty(events);
    }

    [Fact]
    public async Task ContinuousLearningPipeline_CacheFlushFailure_PropagatesAfterTrainingWithoutPublication()
    {
        var events = new List<string>();
        var cache = new ControlledCacheCL(
            OperationResult<bool>.Ok(true),
            OperationResult<bool>.Fail(
                "cache flush failed exactly",
                "CACHE_FLUSH_FAILED_EXACT"),
            events);
        var registry = new CountingRegistryCL();
        var pipeline = BuildSingleDomainPipelineCL(
            cache,
            new RecordingPathIncrementalTrainerCL(
                events,
                SingleAdapterPathCL()),
            registry);

        var result = await pipeline.RunCycleAsync("project");

        Assert.False(result.IsSuccess);
        Assert.Equal("CACHE_FLUSH_FAILED_EXACT", result.ErrorCode);
        Assert.Equal("cache flush failed exactly", result.ErrorMessage);
        Assert.Equal(1, cache.AddCount);
        Assert.Equal(1, cache.FlushCount);
        Assert.Equal(0, registry.RegisterCount);
        Assert.Equal(["Train:BUG", "Flush"], events);
    }

    [Fact]
    public async Task ContinuousLearningPipeline_CacheFlushIncomplete_AfterTrainingPreventsPublication()
    {
        var events = new List<string>();
        var cache = new ControlledCacheCL(
            OperationResult<bool>.Ok(true),
            OperationResult<bool>.Ok(false),
            events);
        var registry = new CountingRegistryCL();
        var pipeline = BuildSingleDomainPipelineCL(
            cache,
            new RecordingPathIncrementalTrainerCL(
                events,
                SingleAdapterPathCL()),
            registry);

        var result = await pipeline.RunCycleAsync("project");

        Assert.False(result.IsSuccess);
        Assert.Equal("CACHE_FLUSH_INCOMPLETE", result.ErrorCode);
        Assert.Equal(1, cache.AddCount);
        Assert.Equal(1, cache.FlushCount);
        Assert.Equal(0, registry.RegisterCount);
        Assert.Equal(["Train:BUG", "Flush"], events);
    }

    [Fact]
    public async Task ContinuousLearningPipeline_CancellationDuringSuccessfulFlush_IsObservedBeforePublication()
    {
        var events = new List<string>();
        using var cancellation = new CancellationTokenSource();
        var cache = new ControlledCacheCL(
            OperationResult<bool>.Ok(true),
            OperationResult<bool>.Ok(true),
            events,
            cancellation);
        var registry = new CountingRegistryCL();
        var pipeline = BuildSingleDomainPipelineCL(
            cache,
            new RecordingPathIncrementalTrainerCL(
                events,
                SingleAdapterPathCL()),
            registry);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            pipeline.RunCycleAsync("project", cancellation.Token));

        Assert.True(cancellation.IsCancellationRequested);
        Assert.Equal(1, cache.AddCount);
        Assert.Equal(1, cache.FlushCount);
        Assert.Equal(0, registry.RegisterCount);
        Assert.Equal(["Train:BUG", "Flush"], events);
    }

    [Fact]
    public async Task ContinuousLearningPipeline_CancellationAfterTraining_PreventsPublicationAndFlush()
    {
        var expected = new TrainingExample(
            "prompt", "response", "BUG", 1d, DateTimeOffset.UtcNow);
        var registry = new CountingRegistryCL();
        var cache = new InMemoryCacheCL();
        using var cancellation = new CancellationTokenSource();
        var pipeline = new ContinuousLearningPipeline(
            Storage,
            new FixedIndexerCL([expected]),
            cache,
            new EmptyFeedbackRouterCL(),
            new CancelThenSuccessIncrementalTrainerCL(
                cancellation,
                Path.GetFullPath(Path.Combine(
                    Path.GetTempPath(),
                    "continuous-learning-cancelled",
                    "BUG.lora"))),
            registry);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            pipeline.RunCycleAsync("project", cancellation.Token));

        Assert.Equal(0, registry.RegisterCount);
        Assert.Equal(0, cache.FlushCount);
    }

    [Fact]
    public async Task ContinuousLearningPipeline_CancellationInsideFirstRegistration_CompletesReadbackFlushAndSuccess()
    {
        var events = new List<string>();
        using var cancellation = new CancellationTokenSource();
        var adapterPath = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-publish-{Guid.NewGuid():N}",
            "BUG.lora"));
        var registry = new RecordingCancellationRegistryCL(
            events,
            cancellation,
            cancelOnFirstRegistration: true);
        var cache = new InMemoryCacheCL(events);
        var pipeline = new ContinuousLearningPipeline(
            Storage,
            new FixedIndexerCL(
            [
                new TrainingExample(
                    "prompt",
                    "response",
                    "BUG",
                    1d,
                    DateTimeOffset.UtcNow),
            ]),
            cache,
            new EmptyFeedbackRouterCL(),
            new RecordingPathIncrementalTrainerCL(
                events,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["BUG"] = adapterPath,
                }),
            registry);

        var result = await pipeline.RunCycleAsync(
            "project",
            cancellation.Token);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var published = Assert.Single(result.Value!);
        Assert.Equal("BUG", published.Domain);
        Assert.Equal(adapterPath, published.AdapterPath);
        Assert.True(cancellation.IsCancellationRequested);
        Assert.Equal(1, registry.RegisterCount);
        Assert.Equal(2, registry.ReadbackCount);
        Assert.Equal(1, cache.FlushCount);
        Assert.Equal(
            [
                "Train:BUG",
                "Flush",
                "Readback:BUG",
                "Register:BUG",
                "Readback:BUG",
            ],
            events);
    }

    [Fact]
    public async Task ContinuousLearningPipeline_RegistersOnlyAfterAllDomainsTrain_AndCancellationDuringFirstRegistrationCompletesPublication()
    {
        var events = new List<string>();
        using var cancellation = new CancellationTokenSource();
        var bugPath = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-publish-{Guid.NewGuid():N}",
            "BUG.lora"));
        var codePath = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-publish-{Guid.NewGuid():N}",
            "CODE_Q.lora"));
        var registry = new RecordingCancellationRegistryCL(
            events,
            cancellation,
            cancelOnFirstRegistration: true);
        var cache = new InMemoryCacheCL(events);
        var pipeline = new ContinuousLearningPipeline(
            Storage,
            new FixedIndexerCL(
            [
                new TrainingExample(
                    "bug prompt",
                    "bug response",
                    "BUG",
                    1d,
                    DateTimeOffset.UtcNow),
                new TrainingExample(
                    "code prompt",
                    "code response",
                    "CODE_Q",
                    1d,
                    DateTimeOffset.UtcNow),
            ]),
            cache,
            new EmptyFeedbackRouterCL(),
            new RecordingPathIncrementalTrainerCL(
                events,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["BUG"] = bugPath,
                    ["CODE_Q"] = codePath,
                }),
            registry);

        var result = await pipeline.RunCycleAsync(
            "project",
            cancellation.Token);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(cancellation.IsCancellationRequested);
        Assert.Equal(
            ["BUG", "CODE_Q"],
            result.Value!.Select(adapter => adapter.Domain).ToArray());
        Assert.Equal(2, registry.RegisterCount);
        Assert.Equal(4, registry.ReadbackCount);
        Assert.Equal(1, cache.FlushCount);
        Assert.Equal(
            [
                "Train:BUG",
                "Train:CODE_Q",
                "Flush",
                "Readback:BUG",
                "Register:BUG",
                "Readback:BUG",
                "Readback:CODE_Q",
                "Register:CODE_Q",
                "Readback:CODE_Q",
            ],
            events);
    }

    [Fact]
    public async Task ContinuousLearningPipeline_CancellationDuringLaterTraining_PreventsAllPublicationAndFlush()
    {
        var events = new List<string>();
        using var cancellation = new CancellationTokenSource();
        var registry = new CountingRegistryCL();
        var cache = new InMemoryCacheCL();
        var pipeline = new ContinuousLearningPipeline(
            Storage,
            new FixedIndexerCL(
            [
                new TrainingExample(
                    "bug prompt",
                    "bug response",
                    "BUG",
                    1d,
                    DateTimeOffset.UtcNow),
                new TrainingExample(
                    "code prompt",
                    "code response",
                    "CODE_Q",
                    1d,
                    DateTimeOffset.UtcNow),
            ]),
            cache,
            new EmptyFeedbackRouterCL(),
            new RecordingPathIncrementalTrainerCL(
                events,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["BUG"] = Path.GetFullPath(Path.Combine(
                        Path.GetTempPath(),
                        $"continuous-learning-training-{Guid.NewGuid():N}",
                        "BUG.lora")),
                    ["CODE_Q"] = Path.GetFullPath(Path.Combine(
                        Path.GetTempPath(),
                        $"continuous-learning-training-{Guid.NewGuid():N}",
                        "CODE_Q.lora")),
                },
                cancellation,
                cancelDuringDomain: "CODE_Q"),
            registry);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            pipeline.RunCycleAsync("project", cancellation.Token));

        Assert.Equal(["Train:BUG", "Train:CODE_Q"], events);
        Assert.Equal(0, registry.RegisterCount);
        Assert.Equal(0, cache.FlushCount);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static DomainDatasetCache BuildCache()
        => new(Storage, new NullCodeWriterCL(), "test-cache");

    private FeedbackRouter BuildFeedbackRouter(IDomainDatasetCache cache)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-feedback-{Guid.NewGuid():N}");
        _feedbackStorageRoots.Add(root);
        return new(
            Storage,
            cache,
            root);
    }

    private static void RouteTrainingReadyRejectedFeedbackCL(
        FeedbackRouter router,
        string requestId)
    {
        var routed = router.Route(new FeedbackEvent(
            "transactional prompt",
            "transactional rejected response",
            null,
            FeedbackSignal.Rejected,
            "BUG")
        {
            TeacherProvenance = TrainingTeacherProvenance.Deterministic(
                requestId),
            GovernanceDecision = new TrainingDataGovernanceDecision(
                licenseAccepted: true,
                isQuarantined: false,
                [HashRef('e')]),
        });
        Assert.True(routed.IsSuccess, routed.ErrorMessage);
    }

    [Fact]
    public void TrainingArtifactPathGuard_RequiresCanonicalAbsoluteRegularFile()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-canonical-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string artifactPath = Path.Combine(directory, "artifact.bin");
        File.WriteAllBytes(artifactPath, [1]);

        try
        {
            Assert.Equal(
                artifactPath,
                TrainingSourceProvenance.RequireCanonicalRegularFilePath(
                    artifactPath));
            Assert.Throws<InvalidDataException>(() =>
                TrainingSourceProvenance.RequireCanonicalRegularFilePath(
                    Path.GetRelativePath(Environment.CurrentDirectory, artifactPath)));
            Assert.Throws<InvalidDataException>(() =>
                TrainingSourceProvenance.RequireCanonicalRegularFilePath(
                    Path.Combine(directory, ".", "artifact.bin")));
            Assert.Throws<InvalidDataException>(() =>
                TrainingSourceProvenance.RequireCanonicalRegularFilePath(directory));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [ReparsePointFactCL]
    public void TrainingArtifactPathGuard_RejectsReparseAncestor()
    {
        string root = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-ancestor-link-{Guid.NewGuid():N}");
        string targetDirectory = Path.Combine(root, "target");
        string linkDirectory = Path.Combine(root, "link");
        Directory.CreateDirectory(targetDirectory);
        string targetPath = Path.Combine(targetDirectory, "artifact.bin");
        File.WriteAllBytes(targetPath, [1]);
        Directory.CreateSymbolicLink(linkDirectory, targetDirectory);

        try
        {
            string linkedPath = Path.Combine(linkDirectory, "artifact.bin");
            Assert.True(File.Exists(linkedPath));
            Assert.Throws<InvalidDataException>(() =>
                TrainingSourceProvenance.RequireCanonicalRegularFilePath(linkedPath));
        }
        finally
        {
            if (Directory.Exists(linkDirectory))
                Directory.Delete(linkDirectory);
            Directory.Delete(root, recursive: true);
        }
    }

    public void Dispose()
    {
        var temporaryRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(Path.GetTempPath()));
        foreach (var root in _feedbackStorageRoots)
        {
            var fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
            Assert.True(
                string.Equals(
                    temporaryRoot,
                    Path.GetDirectoryName(fullRoot),
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal));
            Assert.StartsWith(
                "continuous-learning-feedback-",
                Path.GetFileName(fullRoot),
                StringComparison.Ordinal);
            if (Directory.Exists(fullRoot))
            {
                Assert.False(
                    (File.GetAttributes(fullRoot) & FileAttributes.ReparsePoint) != 0);
                Directory.Delete(fullRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task ContinuousLearningPipeline_ReadbackWithSamePathButDifferentProvenance_FailsClosed()
    {
        var candidatePath = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            $"continuous-provenance-candidate-{Guid.NewGuid():N}.lora"));
        var expected = ProvenanceTestFactory.WithSource(
            new TrainingExample(
                "prompt",
                "response",
                "BUG",
                1d,
                DateTimeOffset.UtcNow),
            "continuous/readback");
        var cache = new InMemoryCacheCL();
        var pipeline = new ContinuousLearningPipeline(
            Storage,
            new FixedIndexerCL([expected]),
            cache,
            new EmptyFeedbackRouterCL(),
            new FixedPathIncrementalTrainerCL(candidatePath),
            new ProvenanceTamperingRegistryCL());

        var result = await pipeline.RunCycleAsync("project");

        Assert.False(result.IsSuccess);
        Assert.Equal("ADAPTER_REGISTRATION_INCOMPLETE", result.ErrorCode);
        Assert.Equal(1, cache.FlushCount);
    }

    private static ContinuousLearningPipeline BuildSingleDomainPipelineCL(
        IDomainDatasetCache cache,
        IIncrementalTrainer trainer,
        ContinuousLearning.IDomainModelRegistry registry)
        => new(
            Storage,
            new FixedIndexerCL(
            [
                new TrainingExample(
                    "prompt",
                    "response",
                    "BUG",
                    1d,
                    DateTimeOffset.UtcNow),
            ]),
            cache,
            new EmptyFeedbackRouterCL(),
            trainer,
            registry);

    private static IReadOnlyDictionary<string, string> SingleAdapterPathCL()
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["BUG"] = Path.GetFullPath(Path.Combine(
                Path.GetTempPath(),
                $"continuous-learning-cache-{Guid.NewGuid():N}",
                "BUG.lora")),
        };

    private static CheckpointBackedIncrementalTrainingSessionFactory
        CreateFactoryWithParentCL(
            TinyTrainingFixtureCL fixture,
            ModelAdapter parentAdapter,
            Action<FileStream, string>? afterParentAdapterOpenForTesting = null,
            Action<FileStream, string>? afterParentAdapterCaptureForTesting = null)
        => new(
            fixture.ModelConfig,
            fixture.BaseCheckpointPath,
            fixture.TokenizerPath,
            new FixedResultRegistryCL(
                OperationResult<ModelAdapter?>.Ok(parentAdapter)),
            fixture.TrainingConfig,
            afterParentAdapterOpenForTesting,
            afterParentAdapterCaptureForTesting);

    private static IncrementalTrainer CreateTrainerWithStepExecutorCL(
        TinyTrainingFixtureCL fixture,
        IIncrementalTrainingSessionFactory sessionFactory,
        ISupervisedTrainingStepExecutor supervisedTrainingStepExecutor)
        => new(
            Storage,
            sessionFactory,
            ServerOwnedLegalTrainingTeacherAdmissionAuthority.Unavailable(),
            new StableDpoTrainingExecutor(),
            supervisedTrainingStepExecutor,
            fixture.AdapterDirectory,
            IncrementalTrainer.DefaultConfig);

    private static void MutateAdapterParametersCL(LlmModel model, float delta)
    {
        foreach (var parameter in GetAdapterParameters(model))
        {
            for (var index = 0; index < parameter.Value.Data.Length; index++)
                parameter.Value.Data[index] += delta;
        }
    }

    private static byte[] ReadAllBytesCL(Stream stream)
    {
        using var copy = new MemoryStream();
        stream.CopyTo(copy);
        return copy.ToArray();
    }

    private static string HashBytesCL(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes, writable: false);
        return TrainingSourceProvenance.Sha256(stream);
    }

    private static Type? MemberAuthorityTypeCL(MemberInfo member)
        => member switch
        {
            FieldInfo field => field.FieldType,
            MethodInfo method => method.ReturnType,
            PropertyInfo property => property.PropertyType,
            _ => null,
        };

    private static int CountOccurrencesCL(string source, string value)
    {
        var count = 0;
        var offset = 0;
        while ((offset = source.IndexOf(value, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += value.Length;
        }

        return count;
    }

    private static string FormatFifoCleanupEvidenceCL((
        OperationResult<IncrementalTrainingSession>? Result,
        Exception? Exception,
        bool WorkerTerminated,
        int OpenError,
        int? CloseError) cleanup)
        => $"cleanup_open_error={cleanup.OpenError}; "
            + $"worker_terminated={cleanup.WorkerTerminated}; "
            + $"eventual_success={cleanup.Result?.IsSuccess.ToString() ?? "none"}; "
            + $"eventual_error_code={cleanup.Result?.ErrorCode ?? "none"}; "
            + $"eventual_exception={cleanup.Exception?.GetType().Name ?? "none"}; "
            + $"cleanup_close_error={cleanup.CloseError?.ToString() ?? "none"}.";

    private static string FindRepositoryFileCL(params string[] pathSegments)
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            ".."));
        return Path.GetFullPath(Path.Combine(
            repositoryRoot,
            Path.Combine(pathSegments)));
    }

    private static string NormalizeWindowsIdentityPathCL(string path)
    {
        var method = Assert.IsAssignableFrom<MethodInfo>(
            typeof(CheckpointBackedIncrementalTrainingSessionFactory).GetMethod(
                "NormalizeWindowsIdentityPath",
                BindingFlags.Static | BindingFlags.NonPublic));
        return Assert.IsType<string>(method.Invoke(null, [path]));
    }

    private static bool WindowsIdentityPathsEqualCL(string first, string second)
    {
        var method = Assert.IsAssignableFrom<MethodInfo>(
            typeof(CheckpointBackedIncrementalTrainingSessionFactory).GetMethod(
                "PathsEqual",
                BindingFlags.Static | BindingFlags.NonPublic));
        return Assert.IsType<bool>(method.Invoke(null, [first, second]));
    }

    private static ulong PackLinuxDeviceCL(ulong device)
    {
        var major = ((device & 0x0000_0000_000f_ff00UL) >> 8)
            | ((device & 0xffff_f000_0000_0000UL) >> 32);
        var minor = (device & 0x0000_0000_0000_00ffUL)
            | ((device & 0x0000_0fff_fff0_0000UL) >> 12);
        return (major << 32) | minor;
    }

    private static bool TryCreateHardLinkCL(
        string newPath,
        string existingPath,
        out int error)
    {
        bool created;
        if (OperatingSystem.IsWindows())
        {
            created = CreateHardLinkWindowsCL(
                newPath,
                existingPath,
                IntPtr.Zero);
        }
        else
        {
            created = CreateHardLinkLinuxCL(existingPath, newPath) == 0;
        }

        error = created ? 0 : Marshal.GetLastPInvokeError();
        return created;
    }

    private static bool TryEnableWindowsCaseSensitiveDirectoryCL(
        string path,
        out int error)
    {
        const uint fileReadAttributes = 0x0000_0080;
        const uint fileWriteAttributes = 0x0000_0100;
        const uint shareRead = 0x0000_0001;
        const uint shareWrite = 0x0000_0002;
        const uint shareDelete = 0x0000_0004;
        const uint openExisting = 3;
        const uint fileFlagBackupSemantics = 0x0200_0000;
        using var handle = CreateFileWindowsCL(
            path,
            fileReadAttributes | fileWriteAttributes,
            shareRead | shareWrite | shareDelete,
            IntPtr.Zero,
            openExisting,
            fileFlagBackupSemantics,
            IntPtr.Zero);
        if (handle.IsInvalid)
        {
            error = Marshal.GetLastPInvokeError();
            return false;
        }
        var information = new WindowsCaseSensitiveInformationCL { Flags = 1 };
        var enabled = SetFileInformationByHandleWindowsCL(
            handle,
            23,
            ref information,
            (uint)Marshal.SizeOf<WindowsCaseSensitiveInformationCL>());
        error = enabled ? 0 : Marshal.GetLastPInvokeError();
        return enabled;
    }

    [StructLayout(LayoutKind.Explicit, Size = 144)]
    private struct LinuxStatX64CL
    {
        [FieldOffset(0)] internal ulong Device;
        [FieldOffset(8)] internal ulong Inode;
        [FieldOffset(16)] internal ulong LinkCount;
        [FieldOffset(24)] internal uint Mode;
    }

    [StructLayout(LayoutKind.Explicit, Size = 256)]
    private struct LinuxStatxEvidenceCL
    {
        [FieldOffset(0)] internal uint Mask;
        [FieldOffset(16)] internal uint LinkCount;
        [FieldOffset(28)] internal ushort Mode;
        [FieldOffset(32)] internal ulong Inode;
        [FieldOffset(136)] internal uint DeviceMajor;
        [FieldOffset(140)] internal uint DeviceMinor;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowsCaseSensitiveInformationCL
    {
        internal uint Flags;
    }

    [DllImport("libc", EntryPoint = "fstat", SetLastError = true)]
    private static extern int FStatLinuxCL(
        int descriptor,
        out LinuxStatX64CL status);

    [DllImport("libc", EntryPoint = "statx", SetLastError = true)]
    private static extern int StatxLinuxCL(
        int directoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags,
        uint mask,
        out LinuxStatxEvidenceCL status);

    [DllImport("libc", EntryPoint = "mkfifo", SetLastError = true)]
    private static extern int MakeFifoLinuxCL(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        uint mode);

    [DllImport("libc", EntryPoint = "open", SetLastError = true)]
    private static extern int OpenLinuxCL(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags);

    [DllImport("libc", EntryPoint = "close", SetLastError = true)]
    private static extern int CloseLinuxCL(int descriptor);

    [DllImport("libc", EntryPoint = "link", SetLastError = true)]
    private static extern int CreateHardLinkLinuxCL(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string existingPath,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string newPath);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "CreateHardLinkW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateHardLinkWindowsCL(
        string fileName,
        string existingFileName,
        IntPtr securityAttributes);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "CreateFileW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern SafeFileHandle CreateFileWindowsCL(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "SetFileInformationByHandle",
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetFileInformationByHandleWindowsCL(
        SafeFileHandle file,
        int fileInformationClass,
        ref WindowsCaseSensitiveInformationCL fileInformation,
        uint bufferSize);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetVolumePathNameW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetVolumePathNameWindowsCL(
        string fileName,
        StringBuilder volumePathName,
        uint bufferLength);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetVolumeNameForVolumeMountPointW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetVolumeNameForVolumeMountPointWindowsCL(
        string volumeMountPoint,
        StringBuilder volumeName,
        uint bufferLength);

    private static LoraConfig NoDropoutLoraConfig()
        => new()
        {
            Rank = 8,
            Alpha = 16f,
            Dropout = 0f,
            TargetModules = ["q_proj", "v_proj"],
        };

    private static string ResolveRegistryLockHolderPath()
    {
        var frameworkDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        var configurationDirectory = frameworkDirectory.Parent
            ?? throw new InvalidOperationException("Test configuration directory is unavailable.");
        var repositoryRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            ".."));
        return Path.GetFullPath(Path.Combine(
            repositoryRoot,
            "Tests.Tools",
            "RegistryLockHolder",
            "bin",
            configurationDirectory.Name,
            frameworkDirectory.Name,
            "RegistryLockHolder.dll"));
    }

    private static void AssertRegistryRestartFails(
        string registryDirectory,
        string expectedCode)
    {
        var exception = Assert.Throws<InvalidDataException>(() =>
            new DomainModelRegistry(Storage, registryDirectory));
        Assert.StartsWith($"{expectedCode}:", exception.Message, StringComparison.Ordinal);
    }

    private static string SeedVersion2LegacyRegistryCL(
        TinyTrainingFixtureCL fixture,
        string domain,
        string fileName)
    {
        var registry = new DomainModelRegistry(Storage, fixture.AdapterDirectory);
        Assert.True(registry.Register(CreateAdapterRegistrationCL(
            fixture,
            domain,
            CreateValidAdapterArtifactCL(fixture, fileName),
            NoDropoutLoraConfig())).IsSuccess);
        var registryPath = Path.Combine(fixture.AdapterDirectory, "registry.json");
        var root = JsonNode.Parse(File.ReadAllText(registryPath))!.AsObject();
        root["SchemaVersion"] = 2;
        foreach (var adapterNode in root["Adapters"]!.AsArray())
        {
            var adapter = adapterNode!.AsObject();
            var path = adapter["AdapterPath"]!.GetValue<string>();
            adapter.Remove("AdapterPath");
            adapter["StorageKind"] = 1;
            adapter["LegacyAbsolutePath"] = path;
        }
        var canonicalBytes = JsonSerializer.SerializeToUtf8Bytes(root);
        var nonCanonicalBytes = JsonSerializer.SerializeToUtf8Bytes(
            root,
            new JsonSerializerOptions { WriteIndented = true });
        Assert.False(nonCanonicalBytes.AsSpan().SequenceEqual(canonicalBytes));
        File.WriteAllBytes(registryPath, nonCanonicalBytes);
        var accepted = new DomainModelRegistry(Storage, fixture.AdapterDirectory)
            .GetAdapter(domain);
        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        Assert.NotNull(accepted.Value);
        Assert.Equal(nonCanonicalBytes, File.ReadAllBytes(registryPath));
        return registryPath;
    }

    private static void AssertVersion2LegacyRegistryDomainsCL(
        byte[] bytes,
        params string[] expectedDomains)
    {
        using var document = JsonDocument.Parse(bytes);
        Assert.Equal(2, document.RootElement.GetProperty("SchemaVersion").GetInt32());
        var adapters = document.RootElement.GetProperty("Adapters").EnumerateArray().ToArray();
        Assert.Equal(
            expectedDomains,
            adapters.Select(adapter => adapter.GetProperty("Domain").GetString()!).ToArray());
        Assert.All(adapters, adapter =>
        {
            Assert.Equal(1, adapter.GetProperty("StorageKind").GetInt32());
            Assert.True(adapter.TryGetProperty("LegacyAbsolutePath", out _));
            Assert.False(adapter.TryGetProperty("RetainedReference", out _));
            Assert.False(adapter.TryGetProperty("AdapterPath", out _));
        });
    }

    private static JsonObject CreateValidAdapterReferenceNodeCL(string sha256)
        => JsonSerializer.SerializeToNode(new AdapterArtifactReference(
            AdapterArtifactReference.CurrentSchemaVersion,
            AdapterArtifactStorageKind.RetainedAdapterRoot,
            "adapter-v2--d3-bug--p3-one.lora",
            new ServerOwnedContinuousLearningAdapterRootIdentity(1, 2, 0),
            new ServerOwnedContinuousLearningAdapterArtifactIdentity(1, 3, 0),
            1,
            sha256))!.AsObject();

    private static string CreateValidAdapterArtifactCL(
        TinyTrainingFixtureCL fixture,
        string fileName,
        string[]? targets = null)
    {
        Directory.CreateDirectory(fixture.AdapterDirectory);
        var model = new LlmModel(fixture.ModelConfig);
        _ = LoraApplicator.Apply(
            model,
            targets is null
                ? NoDropoutLoraConfig()
                : new LoraConfig
                {
                    Rank = 2,
                    Alpha = 4f,
                    Dropout = 0f,
                    TargetModules = targets,
                });
        var path = Path.Combine(fixture.AdapterDirectory, fileName);
        model.SaveAdapterCheckpoint(path);
        return Path.GetFullPath(path);
    }

    private static ModelAdapter CreateAdapterRegistrationCL(
        TinyTrainingFixtureCL fixture,
        string domain,
        string adapterPath,
        LoraConfig loraConfig,
        int trainingExamples = 1)
    {
        var source = TrainingSourceProvenance.CreateSourceId(
            $"test/{domain}/{Path.GetFileName(adapterPath)}",
            "fixture training bytes");
        var manifest = TrainingSourceProvenance.BuildManifest([source]).Value!;
        return new ModelAdapter(
            domain,
            adapterPath,
            trainingExamples,
            DateTimeOffset.UtcNow,
            new ModelAdapterProvenance(
                manifest,
                HashArtifactCL(adapterPath),
                HashArtifactCL(fixture.BaseCheckpointPath),
                HashArtifactCL(fixture.TokenizerPath),
                TrainingSourceProvenance.Identify(fixture.ModelConfig),
                TrainingSourceProvenance.Identify(loraConfig)));
    }

    private static string HashArtifactCL(string path)
    {
        try
        {
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);
            return TrainingSourceProvenance.Sha256(stream);
        }
        catch (Exception exception)
            when (exception is IOException
                or UnauthorizedAccessException
                or NotSupportedException)
        {
            return new string('0', 64);
        }
    }

    private static IReadOnlyList<TensorCore.Layers.Parameter> GetAdapterParameters(
        LlmModel model)
    {
        var baseParameters = new HashSet<TensorCore.Layers.Parameter>(
            model.BaseParameters,
            ReferenceEqualityComparer.Instance);
        return model.Parameters()
            .Where(parameter => !baseParameters.Contains(parameter))
            .ToArray();
    }

    private static IReadOnlyList<int[]> SnapshotParameterBits(
        IEnumerable<TensorCore.Layers.Parameter> parameters)
        => parameters
            .Select(parameter => parameter.Value.Data
                .Select(BitConverter.SingleToInt32Bits)
                .ToArray())
            .ToArray();

    private static void AssertParameterBitsEqual(
        IReadOnlyList<int[]> expected,
        IEnumerable<TensorCore.Layers.Parameter> actual)
    {
        var actualList = actual.ToArray();
        Assert.Equal(expected.Count, actualList.Length);
        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(
                expected[index],
                actualList[index].Value.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray());
        }
    }

    private static void AssertParametersEqual(
        IReadOnlyList<int[]> expected,
        IReadOnlyList<TensorCore.Layers.Parameter> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var parameterIndex = 0; parameterIndex < expected.Count; parameterIndex++)
        {
            var actualBits = actual[parameterIndex].Value.Data
                .Select(BitConverter.SingleToInt32Bits)
                .ToArray();
            Assert.Equal(expected[parameterIndex], actualBits);
        }
    }
}

// ── File-scoped fakes ─────────────────────────────────────────────────────────

file sealed class NullSessionStorageCL : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        => Task.FromResult(OperationResult<bool>.Ok(true));

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        => Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
               (IReadOnlyList<string>)Array.Empty<string>()));
}

// ICodeWriter no-op used only by DomainDatasetCache flush tests.
file sealed class NullCodeWriterCL : ICodeWriter
{
    public Task<OperationResult<bool>> WriteFileAsync(string path, string content)
        => Task.FromResult(OperationResult<bool>.Ok(true));
}

// ISemanticCompiler29 stub: returns a configurable domain and default ratio.
file sealed class StubSC29CompilerCL : SC29.ISemanticCompiler29
{
    private readonly SC29.Domain _domain;

    public StubSC29CompilerCL(SC29.Domain domain) => _domain = domain;

    public SC29.OperationResult<SC29.PipelineOutput> Compile(SC29.PipelineInput input)
        => SC29.OperationResult<SC29.PipelineOutput>.Ok(
               new SC29.PipelineOutput(_domain, [], "csharp", 15.0));
}

// ICodeReader stub that returns an empty file list.
file sealed class EmptyCodeReaderCL : ICodeReader
{
    public Task<OperationResult<IReadOnlyList<string>>> ListFilesAsync(string dir, string pattern)
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
               (IReadOnlyList<string>)Array.Empty<string>()));

    public Task<OperationResult<string>> ReadFileAsync(string path)
        => Task.FromResult(OperationResult<string>.Fail("Not found.", "NOT_FOUND"));
}

// ICodeReader stub that returns exactly one file and its content.
file sealed class SingleFileCodeReaderCL : ICodeReader
{
    private readonly string _file;
    private readonly string _content;

    public SingleFileCodeReaderCL(string file, string content)
        => (_file, _content) = (file, content);

    public Task<OperationResult<IReadOnlyList<string>>> ListFilesAsync(string dir, string pattern)
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
               (IReadOnlyList<string>)new[] { _file }));

    public Task<OperationResult<string>> ReadFileAsync(string path)
        => Task.FromResult(path == _file
               ? OperationResult<string>.Ok(_content)
               : OperationResult<string>.Fail("Not found.", "NOT_FOUND"));
}

file sealed class UnreadableAdapterArtifactLeaseCL : IDisposable
{
    private readonly string _path;
    private readonly UnixFileMode? _originalUnixMode;
    private FileStream? _exclusiveHandle;

    public string Strategy { get; }

    public UnreadableAdapterArtifactLeaseCL(string path)
    {
        _path = path;
        if (OperatingSystem.IsWindows())
        {
            _exclusiveHandle = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None);
            Strategy = "windows-share-none";
            return;
        }

        _originalUnixMode = File.GetUnixFileMode(path);
        try
        {
            File.SetUnixFileMode(path, UnixFileMode.None);
            try
            {
                // The capability probe has already proved that chmod 000 blocks
                // normal reads on this Linux environment. If the file opens here,
                // the environment changed and the test must fail loudly instead
                // of producing a false green result.
                using var privilegeProbe = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read);
            }
            catch (UnauthorizedAccessException)
            {
                Strategy = "unix-mode-none";
                return;
            }

            throw new InvalidOperationException(
                "Unreadable regular-file capability disappeared: chmod 000 no longer blocks reads in this Linux test process.");
        }
        catch
        {
            File.SetUnixFileMode(path, _originalUnixMode.Value);
            throw;
        }
    }

    public void Dispose()
    {
        _exclusiveHandle?.Dispose();
        if (!OperatingSystem.IsWindows()
            && _originalUnixMode is { } originalMode
            && File.Exists(_path))
            File.SetUnixFileMode(_path, originalMode);
    }
}

file sealed class ReparsePointFactCLAttribute : FactAttribute
{
    public ReparsePointFactCLAttribute()
        => Skip = ReparsePointTestSupportCL.GetCapabilitySkipReason();
}

file sealed class ReparsePointTheoryCLAttribute : TheoryAttribute
{
    public ReparsePointTheoryCLAttribute()
        => Skip = ReparsePointTestSupportCL.GetCapabilitySkipReason();
}

file sealed class LinuxFactCLAttribute : FactAttribute
{
    public LinuxFactCLAttribute()
    {
        if (!OperatingSystem.IsLinux())
            Skip = "PLATFORM_UNAVAILABLE: test requires Linux.";
    }
}

file sealed class LinuxX64StatxFactCLAttribute : FactAttribute
{
    public LinuxX64StatxFactCLAttribute()
        => Skip = NativeArtifactCapabilitySupportCL.GetLinuxStatxSkipReason();
}

file sealed class LinuxFifoFactCLAttribute : FactAttribute
{
    public LinuxFifoFactCLAttribute()
        => Skip = NativeArtifactCapabilitySupportCL.GetLinuxFifoSkipReason();
}

file sealed class HardLinkFactCLAttribute : FactAttribute
{
    public HardLinkFactCLAttribute()
        => Skip = NativeArtifactCapabilitySupportCL.GetHardLinkSkipReason();
}

file sealed class WindowsFactCLAttribute : FactAttribute
{
    public WindowsFactCLAttribute()
    {
        if (!OperatingSystem.IsWindows())
            Skip = "PLATFORM_UNAVAILABLE: test requires Windows.";
    }
}

file sealed class UnreadableRegularFileFactCLAttribute : FactAttribute
{
    public UnreadableRegularFileFactCLAttribute()
        => Skip = NativeArtifactCapabilitySupportCL.GetUnreadableRegularFileSkipReason();
}

file sealed class WindowsExtendedUncFactCLAttribute : FactAttribute
{
    public WindowsExtendedUncFactCLAttribute()
        => Skip = NativeArtifactCapabilitySupportCL.GetWindowsExtendedUncSkipReason();
}

file sealed class WindowsVolumeGuidFactCLAttribute : FactAttribute
{
    public WindowsVolumeGuidFactCLAttribute()
        => Skip = NativeArtifactCapabilitySupportCL.GetWindowsVolumeGuidSkipReason();
}

file sealed class WindowsCaseSensitiveDirectoryFactCLAttribute : FactAttribute
{
    public WindowsCaseSensitiveDirectoryFactCLAttribute()
        => Skip = NativeArtifactCapabilitySupportCL
            .GetWindowsCaseSensitiveDirectorySkipReason();
}

file static class NativeArtifactCapabilitySupportCL
{
    private const int ErrorInvalidFunction = 1;
    private const int ErrorAccessDenied = 5;
    private const int ErrorNotSupported = 50;
    private const int ErrorBadNetPath = 53;
    private const int ErrorNetworkAccessDenied = 65;
    private const int ErrorBadNetName = 67;
    private const int ErrorInvalidParameter = 87;
    private const int ErrorSessionCredentialConflict = 1219;
    private const int ErrorPrivilegeNotHeld = 1314;
    private const int ErrorLogonFailure = 1326;

    private const int ErrorPermissionDeniedLinux = 13;
    private const int ErrorReadOnlyFileSystemLinux = 30;
    private const int ErrorFunctionNotImplementedLinux = 38;
    private const int ErrorOperationNotSupportedLinux = 95;

    public static string? GetLinuxStatxSkipReason()
    {
        if (!OperatingSystem.IsLinux())
            return "PLATFORM_UNAVAILABLE: statx proof requires Linux.";
        if (RuntimeInformation.ProcessArchitecture != Architecture.X64)
            return "ARCHITECTURE_UNAVAILABLE: independent fstat proof requires Linux x64.";

        var root = CreateProbeRoot("statx");
        var path = Path.Combine(root, "probe.bin");
        try
        {
            File.WriteAllBytes(path, [1]);
            if (StatxLinuxProbeCL(
                    -100,
                    path,
                    0x100,
                    0x07ff,
                    out var status) == 0)
            {
                const uint requiredMask = 0x0307;
                return (status.Mask & requiredMask) == requiredMask
                    ? null
                    : $"FILESYSTEM_CAPABILITY_UNAVAILABLE: statx mask 0x{status.Mask:X8}.";
            }

            var error = Marshal.GetLastPInvokeError();
            if (error is ErrorPermissionDeniedLinux
                or ErrorFunctionNotImplementedLinux
                or ErrorOperationNotSupportedLinux)
            {
                return NativeSkipReason("statx", error);
            }

            throw UnexpectedNativeFailure("statx", error);
        }
        finally
        {
            DeleteProbeRoot(root);
        }
    }

    public static string? GetLinuxFifoSkipReason()
    {
        if (!OperatingSystem.IsLinux())
            return "PLATFORM_UNAVAILABLE: FIFO proof requires Linux.";

        var root = CreateProbeRoot("fifo");
        var path = Path.Combine(root, "probe.fifo");
        try
        {
            if (MakeFifoLinuxProbeCL(path, 0x180) == 0)
            {
                var descriptor = OpenLinuxProbeCL(
                    path,
                    0x0000_0002 | 0x0000_0800 | 0x0008_0000);
                if (descriptor >= 0)
                {
                    if (CloseLinuxProbeCL(descriptor) != 0)
                    {
                        throw UnexpectedNativeFailure(
                            "FIFO capability-probe close",
                            Marshal.GetLastPInvokeError());
                    }

                    return null;
                }

                var openError = Marshal.GetLastPInvokeError();
                if (openError is 1
                    or ErrorPermissionDeniedLinux
                    or ErrorReadOnlyFileSystemLinux
                    or ErrorOperationNotSupportedLinux)
                {
                    return NativeSkipReason("FIFO O_RDWR open", openError);
                }

                throw UnexpectedNativeFailure("FIFO O_RDWR open", openError);
            }

            var error = Marshal.GetLastPInvokeError();
            if (error is 1
                or ErrorPermissionDeniedLinux
                or ErrorReadOnlyFileSystemLinux
                or ErrorFunctionNotImplementedLinux
                or ErrorOperationNotSupportedLinux)
            {
                return NativeSkipReason("mkfifo", error);
            }

            throw UnexpectedNativeFailure("mkfifo", error);
        }
        finally
        {
            DeleteProbeRoot(root);
        }
    }

    public static string? GetHardLinkSkipReason()
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
            return "PLATFORM_UNAVAILABLE: hard-link proof requires Windows or Linux.";

        var root = CreateProbeRoot("hard-link");
        var source = Path.Combine(root, "source.bin");
        var link = Path.Combine(root, "link.bin");
        try
        {
            File.WriteAllBytes(source, [1]);
            var created = OperatingSystem.IsWindows()
                ? CreateHardLinkWindowsProbeCL(link, source, IntPtr.Zero)
                : CreateHardLinkLinuxProbeCL(source, link) == 0;
            if (created)
                return null;

            var error = Marshal.GetLastPInvokeError();
            if (OperatingSystem.IsWindows()
                ? error is ErrorInvalidFunction
                    or ErrorAccessDenied
                    or ErrorNotSupported
                    or ErrorPrivilegeNotHeld
                : error is 1
                    or ErrorPermissionDeniedLinux
                    or ErrorReadOnlyFileSystemLinux
                    or ErrorOperationNotSupportedLinux)
            {
                return NativeSkipReason("hard-link", error);
            }

            throw UnexpectedNativeFailure("hard-link", error);
        }
        finally
        {
            DeleteProbeRoot(root);
        }
    }

    public static string? GetUnreadableRegularFileSkipReason()
    {
        if (OperatingSystem.IsWindows())
            return null;
        if (!OperatingSystem.IsLinux())
            return "PLATFORM_UNAVAILABLE: unreadable regular-file proof requires Windows or Linux.";

        var root = CreateProbeRoot("unreadable-regular-file");
        var path = Path.Combine(root, "probe.lora");
        UnixFileMode? originalMode = null;
        try
        {
            File.WriteAllBytes(path, [1]);
            originalMode = File.GetUnixFileMode(path);
            File.SetUnixFileMode(path, UnixFileMode.None);
            try
            {
                // The test should run on Linux only when chmod 000 makes a
                // normal file unreadable for this process; root-like processes
                // that can still read it would otherwise produce false green.
                using var stream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read);
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }

            return "FILESYSTEM_CAPABILITY_UNAVAILABLE: chmod 000 did not block regular-file reads in this Linux process.";
        }
        catch (UnauthorizedAccessException exception)
        {
            return $"FILESYSTEM_CAPABILITY_UNAVAILABLE: unreadable regular-file probe could not prepare chmod proof: {exception.Message}";
        }
        catch (IOException exception)
        {
            return $"FILESYSTEM_CAPABILITY_UNAVAILABLE: unreadable regular-file probe could not prepare chmod proof: {exception.Message}";
        }
        catch (PlatformNotSupportedException exception)
        {
            return $"FILESYSTEM_CAPABILITY_UNAVAILABLE: unreadable regular-file probe could not prepare chmod proof: {exception.Message}";
        }
        finally
        {
            try
            {
                if (originalMode is { } mode && File.Exists(path))
                    File.SetUnixFileMode(path, mode);
            }
            finally
            {
                // Cleanup must run even if permission restoration itself fails,
                // otherwise a failed capability probe would leave drive garbage.
                DeleteProbeRoot(root);
            }
        }
    }

    public static string? GetWindowsExtendedUncSkipReason()
    {
        if (!OperatingSystem.IsWindows())
            return "PLATFORM_UNAVAILABLE: extended UNC proof requires Windows.";

        var root = CreateProbeRoot("extended-unc");
        var localPath = Path.Combine(root, "probe.bin");
        try
        {
            File.WriteAllBytes(localPath, [1]);
            var driveRoot = Path.GetPathRoot(localPath);
            if (driveRoot is null
                || driveRoot.Length < 2
                || driveRoot[1] != ':')
            {
                return "NAMESPACE_CAPABILITY_UNAVAILABLE: temp path has no drive-letter namespace.";
            }

            var uncPath = string.Concat(
                @"\\?\UNC\localhost\",
                char.ToUpperInvariant(driveRoot[0]),
                "$\\",
                localPath[driveRoot.Length..]);
            using var handle = OpenWindowsProbe(uncPath);
            if (!handle.IsInvalid)
                return null;

            var error = Marshal.GetLastPInvokeError();
            if (error is ErrorAccessDenied
                or ErrorNotSupported
                or ErrorBadNetPath
                or ErrorNetworkAccessDenied
                or ErrorBadNetName
                or ErrorSessionCredentialConflict
                or ErrorLogonFailure)
            {
                return NativeSkipReason("extended UNC", error);
            }

            throw UnexpectedNativeFailure("extended UNC", error);
        }
        finally
        {
            DeleteProbeRoot(root);
        }
    }

    public static string? GetWindowsVolumeGuidSkipReason()
    {
        if (!OperatingSystem.IsWindows())
            return "PLATFORM_UNAVAILABLE: volume GUID proof requires Windows.";

        var root = CreateProbeRoot("volume-guid");
        var localPath = Path.Combine(root, "probe.bin");
        try
        {
            File.WriteAllBytes(localPath, [1]);
            var mountPoint = new StringBuilder(1024);
            if (!GetVolumePathNameWindowsProbeCL(
                    localPath,
                    mountPoint,
                    (uint)mountPoint.Capacity))
            {
                return ClassifyWindowsCommonCapabilityOrThrow(
                    "GetVolumePathNameW",
                    Marshal.GetLastPInvokeError());
            }

            var volumeName = new StringBuilder(1024);
            if (!GetVolumeNameForVolumeMountPointWindowsProbeCL(
                    mountPoint.ToString(),
                    volumeName,
                    (uint)volumeName.Capacity))
            {
                return ClassifyWindowsCommonCapabilityOrThrow(
                    "GetVolumeNameForVolumeMountPointW",
                    Marshal.GetLastPInvokeError());
            }

            var volumePath = volumeName.ToString()
                + Path.GetRelativePath(mountPoint.ToString(), localPath);
            using var handle = OpenWindowsProbe(volumePath);
            if (!handle.IsInvalid)
                return null;

            return ClassifyWindowsCommonCapabilityOrThrow(
                "volume GUID open",
                Marshal.GetLastPInvokeError());
        }
        finally
        {
            DeleteProbeRoot(root);
        }
    }

    public static string? GetWindowsCaseSensitiveDirectorySkipReason()
    {
        if (!OperatingSystem.IsWindows())
            return "PLATFORM_UNAVAILABLE: case-sensitive-directory proof requires Windows.";

        var root = CreateProbeRoot("case-sensitive");
        try
        {
            const uint fileReadAttributes = 0x0000_0080;
            const uint fileWriteAttributes = 0x0000_0100;
            const uint shareRead = 0x0000_0001;
            const uint shareWrite = 0x0000_0002;
            const uint shareDelete = 0x0000_0004;
            const uint openExisting = 3;
            const uint fileFlagBackupSemantics = 0x0200_0000;
            using var handle = CreateFileWindowsProbeCL(
                root,
                fileReadAttributes | fileWriteAttributes,
                shareRead | shareWrite | shareDelete,
                IntPtr.Zero,
                openExisting,
                fileFlagBackupSemantics,
                IntPtr.Zero);
            if (handle.IsInvalid)
            {
                return ClassifyWindowsCommonCapabilityOrThrow(
                    "case-sensitive directory open",
                    Marshal.GetLastPInvokeError());
            }

            var information = new WindowsCaseSensitiveInformationProbeCL
            {
                Flags = 1,
            };
            if (SetFileInformationByHandleWindowsProbeCL(
                    handle,
                    23,
                    ref information,
                    (uint)Marshal.SizeOf<WindowsCaseSensitiveInformationProbeCL>()))
            {
                return null;
            }

            var error = Marshal.GetLastPInvokeError();
            if (error is ErrorInvalidFunction
                or ErrorAccessDenied
                or ErrorNotSupported
                or ErrorInvalidParameter
                or ErrorPrivilegeNotHeld)
            {
                return NativeSkipReason("case-sensitive directory", error);
            }

            throw UnexpectedNativeFailure("case-sensitive directory", error);
        }
        finally
        {
            DeleteProbeRoot(root);
        }
    }

    private static string ClassifyWindowsCommonCapabilityOrThrow(
        string operation,
        int error)
    {
        if (error is ErrorInvalidFunction
            or ErrorAccessDenied
            or ErrorNotSupported
            or ErrorPrivilegeNotHeld)
        {
            return NativeSkipReason(operation, error);
        }

        throw UnexpectedNativeFailure(operation, error);
    }

    private static SafeFileHandle OpenWindowsProbe(string path)
    {
        const uint genericRead = 0x8000_0000;
        const uint shareRead = 0x0000_0001;
        const uint shareWrite = 0x0000_0002;
        const uint shareDelete = 0x0000_0004;
        const uint openExisting = 3;
        return CreateFileWindowsProbeCL(
            path,
            genericRead,
            shareRead | shareWrite | shareDelete,
            IntPtr.Zero,
            openExisting,
            0,
            IntPtr.Zero);
    }

    private static string CreateProbeRoot(string capability)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-{capability}-capability-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteProbeRoot(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    private static string NativeSkipReason(string operation, int error)
        => $"NATIVE_CAPABILITY_UNAVAILABLE: {operation}, native error {error}.";

    private static InvalidOperationException UnexpectedNativeFailure(
        string operation,
        int error)
        => new($"Unexpected {operation} capability-probe failure: native error {error}.");

    [StructLayout(LayoutKind.Explicit, Size = 256)]
    private struct LinuxStatxProbeCL
    {
        [FieldOffset(0)] internal uint Mask;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowsCaseSensitiveInformationProbeCL
    {
        internal uint Flags;
    }

    [DllImport("libc", EntryPoint = "statx", SetLastError = true)]
    private static extern int StatxLinuxProbeCL(
        int directoryFileDescriptor,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags,
        uint mask,
        out LinuxStatxProbeCL status);

    [DllImport("libc", EntryPoint = "mkfifo", SetLastError = true)]
    private static extern int MakeFifoLinuxProbeCL(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        uint mode);

    [DllImport("libc", EntryPoint = "open", SetLastError = true)]
    private static extern int OpenLinuxProbeCL(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string path,
        int flags);

    [DllImport("libc", EntryPoint = "close", SetLastError = true)]
    private static extern int CloseLinuxProbeCL(int descriptor);

    [DllImport("libc", EntryPoint = "link", SetLastError = true)]
    private static extern int CreateHardLinkLinuxProbeCL(
        [MarshalAs(UnmanagedType.LPUTF8Str)] string existingPath,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string newPath);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "CreateHardLinkW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateHardLinkWindowsProbeCL(
        string fileName,
        string existingFileName,
        IntPtr securityAttributes);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "CreateFileW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    private static extern SafeFileHandle CreateFileWindowsProbeCL(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "SetFileInformationByHandle",
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetFileInformationByHandleWindowsProbeCL(
        SafeFileHandle file,
        int fileInformationClass,
        ref WindowsCaseSensitiveInformationProbeCL fileInformation,
        uint bufferSize);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetVolumePathNameW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetVolumePathNameWindowsProbeCL(
        string fileName,
        StringBuilder volumePathName,
        uint bufferLength);

    [DllImport(
        "kernel32.dll",
        EntryPoint = "GetVolumeNameForVolumeMountPointW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetVolumeNameForVolumeMountPointWindowsProbeCL(
        string volumeMountPoint,
        StringBuilder volumeName,
        uint bufferLength);
}

file static class ReparsePointTestSupportCL
{
    // HRESULT_FROM_WIN32(ERROR_PRIVILEGE_NOT_HELD).
    public const int HResultPrivilegeNotHeld = unchecked((int)0x80070522);

    // HRESULT_FROM_WIN32(ERROR_NOT_SUPPORTED).
    public const int HResultOperationNotSupported = unchecked((int)0x80070032);

    // HRESULT_FROM_WIN32(ERROR_INVALID_FUNCTION).
    public const int HResultInvalidFunction = unchecked((int)0x80070001);

    // HRESULT_FROM_WIN32(ERROR_ACCESS_DENIED).
    public const int HResultAccessDenied = unchecked((int)0x80070005);

    // HRESULT_FROM_WIN32(ENOTSUP/EOPNOTSUPP on Unix).
    public const int HResultOperationNotSupportedUnix =
        unchecked((int)0x8007005F);

    public static string? GetCapabilitySkipReason()
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
        {
            return "PLATFORM_UNAVAILABLE: symbolic-link proof requires Windows or Linux.";
        }

        var root = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-reparse-capability-{Guid.NewGuid():N}");
        var targetPath = Path.Combine(root, "target.lora");
        var linkPath = Path.Combine(root, "link.lora");
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllBytes(targetPath, [1]);
            try
            {
                File.CreateSymbolicLink(linkPath, targetPath);
                return null;
            }
            catch (Exception exception)
                when (IsRecognizedCapabilityLimitation(
                    exception,
                    OperatingSystem.IsWindows()))
            {
                return "REPARSE_TEST_PLATFORM_LIMITATION: " +
                    $"{exception.GetType().FullName}, HResult=0x{exception.HResult:X8}: " +
                    exception.Message;
            }
        }
        finally
        {
            if (File.Exists(linkPath))
                File.Delete(linkPath);
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    // This classifier is used only for an exception thrown directly by
    // File.CreateSymbolicLink in the capability probe above.
    public static bool IsRecognizedCapabilityLimitation(
        Exception exception,
        bool isWindows)
    {
        if (exception is NotSupportedException)
            return true;

        if (exception is UnauthorizedAccessException
            && exception.HResult == HResultAccessDenied)
        {
            return true;
        }

        if (exception is not IOException)
        {
            return false;
        }

        return isWindows
            ? exception.HResult is
                HResultPrivilegeNotHeld
                or HResultOperationNotSupported
                or HResultInvalidFunction
            : exception.HResult is
                HResultOperationNotSupported
                or HResultInvalidFunction
                or HResultOperationNotSupportedUnix;
    }
}

file sealed class SyntheticIOExceptionCL : IOException
{
    public SyntheticIOExceptionCL(int hResult)
        : base("Synthetic I/O failure for reparse capability classification.")
        => HResult = hResult;
}

file sealed class SyntheticUnauthorizedAccessExceptionCL
    : UnauthorizedAccessException
{
    public SyntheticUnauthorizedAccessExceptionCL(int hResult)
        : base("Synthetic access failure for reparse capability classification.")
        => HResult = hResult;
}

internal sealed class TinyTrainingFixtureCL : IDisposable
{
    public string RootDirectory { get; }
    public string BaseCheckpointPath { get; }
    public string TokenizerPath { get; }
    public string AdapterDirectory { get; }
    public ModelConfig ModelConfig { get; }
    public TrainingConfig TrainingConfig { get; }
    public DomainModelRegistry Registry { get; }

    public TinyTrainingFixtureCL(int contextLength = 32)
    {
        RootDirectory = Path.Combine(
            Path.GetTempPath(),
            $"continuous-learning-{Guid.NewGuid():N}");
        Directory.CreateDirectory(RootDirectory);
        BaseCheckpointPath = Path.Combine(RootDirectory, "base.bin");
        TokenizerPath = Path.Combine(RootDirectory, "tokenizer.json");
        AdapterDirectory = Path.Combine(RootDirectory, "adapters");

        ModelConfig = new ModelConfig
        {
            VocabSize = 256,
            ContextLen = contextLength,
            HiddenDim = 8,
            NumLayers = 1,
            NumHeads = 2,
            NumKvHeads = 1,
            FfnHiddenDim = 16,
        };
        TrainingConfig = new TrainingConfig
        {
            BatchSize = 1,
            SeqLen = Math.Max(1, contextLength),
            MaxLr = 1e-2f,
            MinLr = 1e-2f,
            WarmupSteps = 0,
            TotalSteps = 100,
            GradAccumSteps = 1,
            GradClipNorm = 1f,
            WeightDecay = 0f,
            CheckpointDir = Path.Combine(RootDirectory, "unused-checkpoints"),
        };

        new LlmModel(ModelConfig).SaveCheckpoint(BaseCheckpointPath);
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("tiny tokenizer corpus", 256);
        tokenizer.Save(TokenizerPath);

        Registry = new DomainModelRegistry(
            new NullSessionStorageCL(),
            AdapterDirectory);
    }

    public CheckpointBackedIncrementalTrainingSessionFactory CreateFactory()
        => new(
            ModelConfig,
            BaseCheckpointPath,
            TokenizerPath,
            Registry,
            TrainingConfig);

    public TrainingExample Example(
        string domain,
        string prompt,
        string response,
        double weight = 1d)
        => ProvenanceTestFactory.WithSource(
            new TrainingExample(
                prompt,
                response,
                domain,
                weight,
                DateTimeOffset.UtcNow),
            $"tiny/{domain}/{prompt}");

    public void Dispose()
    {
        if (Directory.Exists(RootDirectory))
            Directory.Delete(RootDirectory, recursive: true);
    }
}

file sealed class RecordingSessionFactoryCL : IIncrementalTrainingSessionFactory
{
    private readonly IIncrementalTrainingSessionFactory _inner;

    public List<IncrementalTrainingSession> Sessions { get; } = [];
    public List<IReadOnlyList<int[]>> BaseSnapshots { get; } = [];

    public RecordingSessionFactoryCL(IIncrementalTrainingSessionFactory inner)
        => _inner = inner;

    public OperationResult<IncrementalTrainingSession> Create(string domain)
    {
        var result = _inner.Create(domain);
        if (result.IsSuccess && result.Value is not null)
        {
            Sessions.Add(result.Value);
            BaseSnapshots.Add(
                result.Value.Model.BaseParameters
                    .Select(parameter => parameter.Value.Data
                        .Select(BitConverter.SingleToInt32Bits)
                        .ToArray())
                    .ToArray());
        }

        return result;
    }
}

file sealed class FixedSessionFactoryCL : IIncrementalTrainingSessionFactory
{
    private readonly IncrementalTrainingSession _session;

    public FixedSessionFactoryCL(IncrementalTrainingSession session)
        => _session = session;

    public OperationResult<IncrementalTrainingSession> Create(string domain)
        => OperationResult<IncrementalTrainingSession>.Ok(_session);
}

file sealed class ParentCheckpointObservationStepExecutorCL
    : ISupervisedTrainingStepExecutor
{
    private readonly IReadOnlyList<int[]> _expectedAdapterBits;

    public bool ObservedLoadedCheckpoint { get; private set; }
    public int ExecuteCount { get; private set; }

    public ParentCheckpointObservationStepExecutorCL(
        IReadOnlyList<int[]> expectedAdapterBits)
        => _expectedAdapterBits = expectedAdapterBits
            .Select(static bits => bits.ToArray())
            .ToArray();

    public float Execute(
        IncrementalTrainingSession session,
        IReadOnlyList<int> tokenIds,
        float weight,
        IReadOnlyList<TensorCore.Layers.Parameter> adapterParameters)
    {
        ExecuteCount++;
        Assert.Equal(_expectedAdapterBits.Count, adapterParameters.Count);
        for (var parameterIndex = 0;
             parameterIndex < _expectedAdapterBits.Count;
             parameterIndex++)
        {
            Assert.Equal(
                _expectedAdapterBits[parameterIndex],
                adapterParameters[parameterIndex].Value.Data
                    .Select(BitConverter.SingleToInt32Bits)
                    .ToArray());
        }

        ObservedLoadedCheckpoint = true;
        throw new InvalidOperationException("STOP_BEFORE_SUPERVISED_UPDATE");
    }
}

file sealed class ThrowingCheckpointReadStreamCL : Stream
{
    private readonly Exception _exception;

    public ThrowingCheckpointReadStreamCL(Exception exception)
        => _exception = exception;

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush() => throw new NotSupportedException();

    public override int Read(byte[] buffer, int offset, int count)
        => throw _exception;

    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException();

    public override void SetLength(long value)
        => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();
}

file sealed class CountingSessionFactoryCL : IIncrementalTrainingSessionFactory
{
    public int CreateCount { get; private set; }

    public OperationResult<IncrementalTrainingSession> Create(string domain)
    {
        CreateCount++;
        return OperationResult<IncrementalTrainingSession>.Fail(
            "Factory should not have been called.",
            "UNEXPECTED_FACTORY_CALL");
    }
}

file sealed class BlockingSessionFactoryCL : IIncrementalTrainingSessionFactory
{
    private int _createCount;

    public ManualResetEventSlim Entered { get; } = new(false);
    public ManualResetEventSlim Release { get; } = new(false);
    public int CreateCount => Volatile.Read(ref _createCount);

    public OperationResult<IncrementalTrainingSession> Create(string domain)
    {
        Interlocked.Increment(ref _createCount);
        Entered.Set();
        Release.Wait();
        return OperationResult<IncrementalTrainingSession>.Fail(
            "First session intentionally released without training.",
            "TRAINING_SESSION_CREATION_FAILED");
    }
}

file sealed class FixedIndexerCL : IProjectIndexer
{
    private readonly IReadOnlyList<TrainingExample> _examples;

    public FixedIndexerCL(IReadOnlyList<TrainingExample> examples)
        => _examples = examples;

    public OperationResult<IReadOnlyList<TrainingExample>> Index(string rootPath)
        => OperationResult<IReadOnlyList<TrainingExample>>.Ok(_examples);
}

file sealed class InMemoryCacheCL : IDomainDatasetCache
{
    private readonly List<TrainingExample> _examples = [];
    private readonly List<string>? _events;
    public int FlushCount { get; private set; }

    public InMemoryCacheCL(List<string>? events = null)
        => _events = events;

    public OperationResult<bool> Add(TrainingExample example)
    {
        _examples.Add(example);
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<IReadOnlyList<TrainingExample>> GetByDomain(string domain)
        => OperationResult<IReadOnlyList<TrainingExample>>.Ok(
            _examples.Where(example => example.Domain == domain).ToArray());

    public OperationResult<int> TotalCount()
        => OperationResult<int>.Ok(_examples.Count);

    public Task<OperationResult<bool>> FlushAsync()
    {
        FlushCount++;
        _events?.Add("Flush");
        return Task.FromResult(OperationResult<bool>.Ok(true));
    }
}

file sealed class ControlledCacheCL : IDomainDatasetCache
{
    private readonly OperationResult<bool> _addResult;
    private readonly OperationResult<bool> _flushResult;
    private readonly List<string> _events;
    private readonly CancellationTokenSource? _cancelDuringFlush;

    public int AddCount { get; private set; }
    public int FlushCount { get; private set; }

    public ControlledCacheCL(
        OperationResult<bool> addResult,
        OperationResult<bool> flushResult,
        List<string> events,
        CancellationTokenSource? cancelDuringFlush = null)
    {
        _addResult = addResult;
        _flushResult = flushResult;
        _events = events;
        _cancelDuringFlush = cancelDuringFlush;
    }

    public OperationResult<bool> Add(TrainingExample example)
    {
        AddCount++;
        return _addResult;
    }

    public OperationResult<IReadOnlyList<TrainingExample>> GetByDomain(
        string domain)
        => OperationResult<IReadOnlyList<TrainingExample>>.Ok([]);

    public OperationResult<int> TotalCount()
        => OperationResult<int>.Ok(AddCount);

    public Task<OperationResult<bool>> FlushAsync()
    {
        FlushCount++;
        _events.Add("Flush");
        _cancelDuringFlush?.Cancel();
        return Task.FromResult(_flushResult);
    }
}

file sealed class EmptyFeedbackRouterCL : IFeedbackRouter
{
    public OperationResult<bool> Route(FeedbackEvent feedback)
        => OperationResult<bool>.Ok(true);

    public OperationResult<IReadOnlyList<DpoPair>> GetDpoPairs()
        => OperationResult<IReadOnlyList<DpoPair>>.Ok([]);
}

file sealed class FailingIncrementalTrainerCL : IIncrementalTrainer
{
    private readonly string _message;
    private readonly string _code;

    public FailingIncrementalTrainerCL(string message, string code)
        => (_message, _code) = (message, code);

    public Task<OperationResult<TrainedAdapterArtifact>> TrainAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair> dpoPairs,
        CancellationToken cancellationToken = default)
        => Task.FromResult(OperationResult<TrainedAdapterArtifact>.Fail(_message, _code));
}

file sealed class NullSuccessIncrementalTrainerCL : IIncrementalTrainer
{
    public Task<OperationResult<TrainedAdapterArtifact>> TrainAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair> dpoPairs,
        CancellationToken cancellationToken = default)
        => Task.FromResult(OperationResult<TrainedAdapterArtifact>.Ok(null!));
}

file sealed class FixedTransactionalResultIncrementalTrainerCL
    : IIncrementalTrainer,
      ITransactionalIncrementalTrainer
{
    private readonly OperationResult<TrainedAdapterArtifact> _result;

    public FixedTransactionalResultIncrementalTrainerCL(
        OperationResult<TrainedAdapterArtifact> result)
        => _result = result;

    public Task<OperationResult<TrainedAdapterArtifact>> TrainAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair> dpoPairs,
        CancellationToken cancellationToken = default)
        => Task.FromResult(OperationResult<TrainedAdapterArtifact>.Fail(
            "The legacy training path was not expected.",
            "UNEXPECTED_LEGACY_TRAINING"));

    public Task<OperationResult<TrainedAdapterArtifact>> TrainTransactionAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair> dpoPairs,
        FileDpoPairStore.TransactionHandle transaction,
        CancellationToken cancellationToken = default)
        => Task.FromResult(_result);
}

file sealed class FixedPathIncrementalTrainerCL : IIncrementalTrainer
{
    private readonly string _adapterPath;

    public FixedPathIncrementalTrainerCL(string adapterPath)
        => _adapterPath = adapterPath;

    public Task<OperationResult<TrainedAdapterArtifact>> TrainAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair> dpoPairs,
        CancellationToken cancellationToken = default)
        => Task.FromResult(OperationResult<TrainedAdapterArtifact>.Ok(
            ProvenanceTestFactory.Artifact(_adapterPath, examples)));
}

file sealed class RecordingPathIncrementalTrainerCL : IIncrementalTrainer
{
    private readonly List<string> _events;
    private readonly IReadOnlyDictionary<string, string> _adapterPaths;
    private readonly CancellationTokenSource? _cancellation;
    private readonly string? _cancelDuringDomain;

    public RecordingPathIncrementalTrainerCL(
        List<string> events,
        IReadOnlyDictionary<string, string> adapterPaths,
        CancellationTokenSource? cancellation = null,
        string? cancelDuringDomain = null)
    {
        _events = events;
        _adapterPaths = adapterPaths;
        _cancellation = cancellation;
        _cancelDuringDomain = cancelDuringDomain;
    }

    public Task<OperationResult<TrainedAdapterArtifact>> TrainAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair> dpoPairs,
        CancellationToken cancellationToken = default)
    {
        _events.Add($"Train:{domain}");
        if (StringComparer.Ordinal.Equals(domain, _cancelDuringDomain))
        {
            Assert.NotNull(_cancellation);
            Assert.Equal(_cancellation.Token, cancellationToken);
            _cancellation.Cancel();
        }

        return Task.FromResult(OperationResult<TrainedAdapterArtifact>.Ok(
            ProvenanceTestFactory.Artifact(_adapterPaths[domain], examples)));
    }
}

file sealed class CancelThenSuccessIncrementalTrainerCL : IIncrementalTrainer
{
    private readonly CancellationTokenSource _cancellation;
    private readonly string _adapterPath;

    public CancelThenSuccessIncrementalTrainerCL(
        CancellationTokenSource cancellation,
        string adapterPath)
        => (_cancellation, _adapterPath) = (cancellation, adapterPath);

    public Task<OperationResult<TrainedAdapterArtifact>> TrainAsync(
        string domain,
        IReadOnlyList<TrainingExample> examples,
        IReadOnlyList<DpoPair> dpoPairs,
        CancellationToken cancellationToken = default)
    {
        Assert.Equal(_cancellation.Token, cancellationToken);
        _cancellation.Cancel();
        return Task.FromResult(OperationResult<TrainedAdapterArtifact>.Ok(
            ProvenanceTestFactory.Artifact(_adapterPath, examples)));
    }
}

file sealed class CountingRegistryCL : ContinuousLearning.IDomainModelRegistry
{
    public int RegisterCount { get; private set; }

    public OperationResult<bool> Register(ModelAdapter adapter)
    {
        RegisterCount++;
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<ModelAdapter?> GetAdapter(string domain)
        => OperationResult<ModelAdapter?>.Ok(null);

    public OperationResult<IReadOnlyList<ModelAdapter>> ListAll()
        => OperationResult<IReadOnlyList<ModelAdapter>>.Ok([]);
}

file sealed class RecordingCancellationRegistryCL
    : ContinuousLearning.IDomainModelRegistry
{
    private readonly List<string> _events;
    private readonly CancellationTokenSource _cancellation;
    private readonly bool _cancelOnFirstRegistration;
    private readonly Dictionary<string, ModelAdapter> _adapters =
        new(StringComparer.Ordinal);

    public int RegisterCount { get; private set; }
    public int ReadbackCount { get; private set; }

    public RecordingCancellationRegistryCL(
        List<string> events,
        CancellationTokenSource cancellation,
        bool cancelOnFirstRegistration)
    {
        _events = events;
        _cancellation = cancellation;
        _cancelOnFirstRegistration = cancelOnFirstRegistration;
    }

    public OperationResult<bool> Register(ModelAdapter adapter)
    {
        RegisterCount++;
        _events.Add($"Register:{adapter.Domain}");
        _adapters[adapter.Domain] = new ModelAdapter(
            adapter.Domain,
            Path.GetFullPath(adapter.AdapterPath),
            adapter.TrainingExamples,
            adapter.TrainedAt,
            adapter.Provenance);
        if (_cancelOnFirstRegistration && RegisterCount == 1)
            _cancellation.Cancel();
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<ModelAdapter?> GetAdapter(string domain)
    {
        ReadbackCount++;
        _events.Add($"Readback:{domain}");
        return OperationResult<ModelAdapter?>.Ok(
            _adapters.TryGetValue(domain, out var adapter)
                ? adapter
                : null);
    }

    public OperationResult<IReadOnlyList<ModelAdapter>> ListAll()
        => OperationResult<IReadOnlyList<ModelAdapter>>.Ok(
            _adapters.Values.ToArray());
}

file sealed class FixedResultRegistryCL : ContinuousLearning.IDomainModelRegistry
{
    private readonly OperationResult<ModelAdapter?> _getResult;

    public FixedResultRegistryCL(OperationResult<ModelAdapter?> getResult)
        => _getResult = getResult;

    public OperationResult<bool> Register(ModelAdapter adapter)
        => OperationResult<bool>.Fail(
            "Registration was not expected.",
            "UNEXPECTED_REGISTRATION");

    public OperationResult<ModelAdapter?> GetAdapter(string domain)
        => _getResult;

    public OperationResult<IReadOnlyList<ModelAdapter>> ListAll()
        => OperationResult<IReadOnlyList<ModelAdapter>>.Ok([]);
}

file sealed class ScriptedRegistryCL : ContinuousLearning.IDomainModelRegistry
{
    private readonly OperationResult<bool> _registrationResult;
    private readonly OperationResult<ModelAdapter?> _getResult;

    public ScriptedRegistryCL(
        OperationResult<bool> registrationResult,
        OperationResult<ModelAdapter?> getResult)
        => (_registrationResult, _getResult) = (registrationResult, getResult);

    public OperationResult<bool> Register(ModelAdapter adapter)
        => _registrationResult;

    public OperationResult<ModelAdapter?> GetAdapter(string domain)
        => _getResult;

    public OperationResult<IReadOnlyList<ModelAdapter>> ListAll()
        => OperationResult<IReadOnlyList<ModelAdapter>>.Ok([]);
}

file sealed class ProvenanceTamperingRegistryCL : ContinuousLearning.IDomainModelRegistry
{
    private ModelAdapter? _adapter;

    public OperationResult<bool> Register(ModelAdapter adapter)
    {
        _adapter = adapter;
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<ModelAdapter?> GetAdapter(string domain)
    {
        if (_adapter?.Provenance is not { } provenance)
            return OperationResult<ModelAdapter?>.Ok(_adapter);
        return OperationResult<ModelAdapter?>.Ok(
            _adapter with
            {
                AdapterPath = Path.GetFullPath(_adapter.AdapterPath),
                Provenance = provenance with
                {
                    AdapterSha256 = new string('f', 64),
                },
            });
    }

    public OperationResult<IReadOnlyList<ModelAdapter>> ListAll()
        => OperationResult<IReadOnlyList<ModelAdapter>>.Ok([]);
}

file sealed class ThrowingRegistryCL : ContinuousLearning.IDomainModelRegistry
{
    private readonly Exception _exception;

    public ThrowingRegistryCL(Exception exception) => _exception = exception;

    public OperationResult<bool> Register(ModelAdapter adapter)
        => throw _exception;

    public OperationResult<ModelAdapter?> GetAdapter(string domain)
        => throw _exception;

    public OperationResult<IReadOnlyList<ModelAdapter>> ListAll()
        => throw _exception;
}

file sealed class MoveNextProbeEnumerable<T> : IEnumerable<T>
{
    private readonly T[] _items;
    private readonly int? _throwOnMoveNextCall;
    private readonly Exception? _exception;

    public MoveNextProbeEnumerable(
        IEnumerable<T> items,
        int? throwOnMoveNextCall = null,
        Exception? exception = null)
    {
        _items = items.ToArray();
        _throwOnMoveNextCall = throwOnMoveNextCall;
        _exception = exception;
    }

    public int MoveNextCallCount { get; private set; }
    public int YieldedCount { get; private set; }

    public IEnumerator<T> GetEnumerator() => new Enumerator(this);

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();

    private sealed class Enumerator : IEnumerator<T>
    {
        private readonly MoveNextProbeEnumerable<T> _owner;
        private int _index = -1;

        public Enumerator(MoveNextProbeEnumerable<T> owner) => _owner = owner;

        public T Current => _owner._items[_index];
        object? System.Collections.IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _owner.MoveNextCallCount++;
            if (_owner._throwOnMoveNextCall == _owner.MoveNextCallCount)
                throw _owner._exception ?? new InvalidOperationException("Configured enumeration probe failure.");

            var next = _index + 1;
            if (next >= _owner._items.Length)
                return false;

            _index = next;
            _owner.YieldedCount++;
            return true;
        }

        public void Reset() => throw new NotSupportedException();
        public void Dispose() { }
    }
}
