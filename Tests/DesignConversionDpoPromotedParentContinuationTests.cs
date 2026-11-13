// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DesignConversionDomain;
using DPO;
using Tokenizer;
using TransformerModel;
using TransformerModel.Config;

public sealed class DesignConversionDpoPromotedParentContinuationTests
{
    [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateHardLink(
        string newFileName,
        string existingFileName,
        IntPtr securityAttributes);

    [Fact]
    public void EmptyJournalResolvesNone()
    {
        var resolved = JsonlDesignConversionLocalArtifactDb
            .ResolveCurrentLocalPromotionActiveState([]);

        Assert.Equal(DesignConversionLocalPromotionActiveStateKind.None, resolved.Kind);
        Assert.Null(resolved.Active);
    }

    [Fact]
    public void AppendOrderSelectsSecondReadyPromotionWithoutTimestamps()
    {
        using var fixture = new ParentFixture();
        var first = fixture.Ready("a", prior: null);
        var second = fixture.Ready("b", first);
        first = first with
        {
            BoundaryMetadata = With(
                first.BoundaryMetadata,
                "training_intake_promote_local_recorded_at_utc",
                "9999-12-31T23:59:59Z")
        };
        second = second with
        {
            BoundaryMetadata = With(
                second.BoundaryMetadata,
                "training_intake_promote_local_recorded_at_utc",
                "0001-01-01T00:00:00Z")
        };

        var resolved = JsonlDesignConversionLocalArtifactDb
            .ResolveCurrentLocalPromotionActiveState([first, second]);

        Assert.Equal(DesignConversionLocalPromotionActiveStateKind.Active, resolved.Kind);
        Assert.Equal(ActiveRef("b"), resolved.Active!.ActiveLocalModelRef);
    }

    [Fact]
    public void AppendOrderRollbackRestoresExactlyOneEarlierPromotion()
    {
        using var fixture = new ParentFixture();
        var first = fixture.Ready("a", prior: null);
        var second = fixture.Ready("b", first);
        var rollback = Rollback(second, first, "rollback-b-to-a");

        var resolved = JsonlDesignConversionLocalArtifactDb
            .ResolveCurrentLocalPromotionActiveState([first, second, rollback]);

        Assert.Equal(DesignConversionLocalPromotionActiveStateKind.Active, resolved.Kind);
        Assert.Equal(ActiveRef("a"), resolved.Active!.ActiveLocalModelRef);
        Assert.Equal(first.PromotedLocalModel!.CheckpointSha256, resolved.Active.ActiveCheckpointSha256);
    }

    [Theory]
    [InlineData("target")]
    [InlineData("restored")]
    [InlineData("duplicate")]
    public void MalformedConflictingOrDuplicateRollbackIsInvalid(string mutation)
    {
        using var fixture = new ParentFixture();
        var first = fixture.Ready("a", prior: null);
        var second = fixture.Ready("b", first);
        var rollback = Rollback(second, first, "rollback-b-to-a");
        IReadOnlyList<DesignConversionTrainingIntakeLocalPromotionResult> rows;
        if (mutation == "target")
        {
            rollback = rollback with
            {
                BoundaryMetadata = With(
                    rollback.BoundaryMetadata,
                    "local_promotion_rollback_target_local_model_ref",
                    ActiveRef("a"))
            };
            rows = [first, second, rollback];
        }
        else if (mutation == "restored")
        {
            rollback = rollback with
            {
                BoundaryMetadata = With(
                    rollback.BoundaryMetadata,
                    "local_promotion_rollback_restored_active_checkpoint_sha256",
                    Hash("forged"))
            };
            rows = [first, second, rollback];
        }
        else
        {
            rows = [first, second, rollback, rollback];
        }

        var resolved = JsonlDesignConversionLocalArtifactDb
            .ResolveCurrentLocalPromotionActiveState(rows);

        Assert.Equal(DesignConversionLocalPromotionActiveStateKind.Invalid, resolved.Kind);
    }

    [Fact]
    public void LegacyReadyPromotionWithoutTrustedAnchorsIsInvalid()
    {
        using var fixture = new ParentFixture();
        var ready = fixture.Ready("legacy", prior: null);
        var metadata = new Dictionary<string, string>(ready.BoundaryMetadata, StringComparer.Ordinal);
        metadata.Remove(DesignConversionDpoPromotionArtifactContractCodec.ArtifactSetIdAnchorMetadataKey);

        var resolved = JsonlDesignConversionLocalArtifactDb
            .ResolveCurrentLocalPromotionActiveState(
                [ready with { BoundaryMetadata = metadata }]);

        Assert.Equal(DesignConversionLocalPromotionActiveStateKind.Invalid, resolved.Kind);
    }

    [Fact]
    public void ActiveClaimThatIsNotReadyOrLacksDpoEvidenceIsInvalid()
    {
        using var fixture = new ParentFixture();
        var ready = fixture.Ready("active", prior: null);
        var notReady = ready with { LocalPromotionReady = false };
        var missingEvidence = ready with
        {
            PromotedLocalModel = ready.PromotedLocalModel! with
            {
                EvidenceRefs = ready.PromotedLocalModel.EvidenceRefs
                    .Where(value => !value.StartsWith(
                        "dpo_policy_checkpoint_file:",
                        StringComparison.Ordinal))
                    .ToArray()
            }
        };

        Assert.Equal(
            DesignConversionLocalPromotionActiveStateKind.Invalid,
            JsonlDesignConversionLocalArtifactDb
                .ResolveCurrentLocalPromotionActiveState([notReady]).Kind);
        Assert.Equal(
            DesignConversionLocalPromotionActiveStateKind.Invalid,
            JsonlDesignConversionLocalArtifactDb
                .ResolveCurrentLocalPromotionActiveState([missingEvidence]).Kind);
    }

    [Fact]
    public void NonDpoReadyPromotionDoesNotRequireDpoPolicyCheckpointFileRef()
    {
        using var fixture = new ParentFixture();
        var ready = fixture.Ready("non-dpo-active", prior: null);
        var nonDpo = ready with
        {
            PromotedLocalModel = ready.PromotedLocalModel! with
            {
                EvidenceRefs = ready.PromotedLocalModel.EvidenceRefs
                    .Where(value =>
                        !string.Equals(
                            value,
                            DesignConversionMachine
                                .DpoTrainerPolicyCheckpointConvertedEvidence,
                            StringComparison.Ordinal) &&
                        !string.Equals(
                            value,
                            DesignConversionMachine
                                .DpoTrainerPolicyCheckpointFreshLoadedEvidence,
                            StringComparison.Ordinal) &&
                        !value.StartsWith(
                            "dpo_policy_checkpoint_file:",
                            StringComparison.Ordinal) &&
                        !value.StartsWith(
                            "dpo_policy_checkpoint_sha256:",
                            StringComparison.Ordinal))
                    .ToArray()
            }
        };

        var resolved = JsonlDesignConversionLocalArtifactDb
            .ResolveCurrentLocalPromotionActiveState([nonDpo]);

        Assert.Equal(DesignConversionLocalPromotionActiveStateKind.Active, resolved.Kind);
        Assert.False(nonDpo.PromotedLocalModel!.DpoTrainerPolicyCheckpointConverted);
        Assert.False(nonDpo.PromotedLocalModel.DpoTrainerPolicyCheckpointFreshLoaded);
        Assert.Equal(string.Empty, nonDpo.PromotedLocalModel.DpoPolicyCheckpointFileRef);
    }

    [Theory]
    [InlineData("decision")]
    [InlineData("receipt-written")]
    [InlineData("receipt-kind")]
    [InlineData("upload-attempted")]
    [InlineData("promotion-claimed")]
    [InlineData("publication-kind")]
    [InlineData("publication-status")]
    public void ReadyPromotionRequiresExactTrustedLocalPublicationSemantics(
        string mutation)
    {
        using var fixture = new ParentFixture();
        var ready = fixture.Ready("publication-semantics", prior: null);
        var publication = ready.Publication!;
        publication = mutation switch
        {
            "decision" => publication with
            {
                Decision = publication.Decision with
                {
                    Kind = DesignConversionPromotionDecisionKind.ContinueTraining
                }
            },
            "receipt-written" => publication with
            {
                Manifest = publication.Manifest with
                {
                    LocalPromotionReceiptWritten = false
                }
            },
            "receipt-kind" => publication with
            {
                Manifest = publication.Manifest with
                {
                    PromotionReceiptKind = "model_server_upload_verified"
                }
            },
            "upload-attempted" => publication with
            {
                Manifest = publication.Manifest with
                {
                    ModelServerUploadAttempted = true
                }
            },
            "promotion-claimed" => publication with
            {
                Manifest = publication.Manifest with
                {
                    ModelServerPromotionClaimed = true
                }
            },
            "publication-kind" => publication with
            {
                PublicationKind = DesignConversionPromotionPublicationKind
                    .VerifiedModelServerUpload
            },
            "publication-status" => publication with
            {
                PublicationStatus = DesignConversionPromotionPublicationStatus
                    .UploadedVerified
            },
            _ => throw new ArgumentOutOfRangeException(nameof(mutation))
        };

        var resolved = JsonlDesignConversionLocalArtifactDb
            .ResolveCurrentLocalPromotionActiveState(
                [ready with { Publication = publication }]);

        Assert.Equal(DesignConversionLocalPromotionActiveStateKind.Invalid, resolved.Kind);
        Assert.Null(resolved.Active);
    }

    [Fact]
    public void UnknownRecordKindIsInvalidEvenWithoutAnActiveClaim()
    {
        using var fixture = new ParentFixture();
        var row = fixture.Ready("unknown-kind", prior: null) with
        {
            LocalPromotionReady = false,
            PromotedLocalModel = null,
            BoundaryMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["local_promotion_record_kind"] = "future-untrusted-kind"
            }
        };

        Assert.Equal(
            DesignConversionLocalPromotionActiveStateKind.Invalid,
            JsonlDesignConversionLocalArtifactDb
                .ResolveCurrentLocalPromotionActiveState([row]).Kind);
    }

    [Fact]
    public void HistoricalNonReadyRowWithoutAnActiveClaimIsIgnored()
    {
        using var fixture = new ParentFixture();
        var row = fixture.Ready("historical", prior: null) with
        {
            LocalPromotionReady = false,
            PromotedLocalModel = null,
            BoundaryMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
        };

        Assert.Equal(
            DesignConversionLocalPromotionActiveStateKind.None,
            JsonlDesignConversionLocalArtifactDb
                .ResolveCurrentLocalPromotionActiveState([row]).Kind);
    }

    [Fact]
    public void ReadyPromotionWithWrongSchemaIsInvalid()
    {
        using var fixture = new ParentFixture();
        var row = fixture.Ready("wrong-schema", prior: null);
        row = row with
        {
            BoundaryMetadata = With(row.BoundaryMetadata, "schema_version", "999")
        };

        Assert.Equal(
            DesignConversionLocalPromotionActiveStateKind.Invalid,
            JsonlDesignConversionLocalArtifactDb
                .ResolveCurrentLocalPromotionActiveState([row]).Kind);
    }

    [Fact]
    public void NonReadyRowThatClaimsAnActiveModelIsInvalid()
    {
        using var fixture = new ParentFixture();
        var row = fixture.Ready("non-ready-claim", prior: null) with
        {
            LocalPromotionReady = false,
            PromotedLocalModel = null
        };

        Assert.Equal(
            DesignConversionLocalPromotionActiveStateKind.Invalid,
            JsonlDesignConversionLocalArtifactDb
                .ResolveCurrentLocalPromotionActiveState([row]).Kind);
    }

    [Fact]
    public async Task LegacyPublishedParentWithoutCanonicalBpeResolvesRetainsFiveArtifactsAndTransfersOnce()
    {
        using var fixture = await CanonicalParentFixture.CreateAsync();
        var openedStages = new List<DesignConversionDpoPromotedParentIoStage>();

        var resolved = await DesignConversionDpoPromotedParentContinuation.ResolveAsync(
            fixture.Active,
            fixture.Root,
            CancellationToken.None,
            (stage, _) => openedStages.Add(stage));

        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        var lease = resolved.Value!;
        var movedOutput = fixture.Paths.OutputDirectory + "-moved";
        var outputMovedDuringLease = false;
        var policyCheckpointBytes = await File.ReadAllBytesAsync(
            fixture.PathFor("policy-checkpoint"));
        try
        {
            Assert.Equal(5, fixture.AllArtifactPaths.Count);
            Assert.Null(lease.Request.ParentCanonicalBpeTokenizerSnapshot);
            Assert.Contains(
                DesignConversionDpoPromotedParentIoStage.BeforeTokenizerArtifactOpen,
                openedStages);
            Assert.DoesNotContain(
                DesignConversionDpoPromotedParentIoStage
                    .BeforeCanonicalBpeTokenizerArtifactOpen,
                openedStages);
            Assert.Contains(DesignConversionDpoPromotedParentIoStage.AfterAllArtifactsOpen, openedStages);
            Assert.Equal(fixture.Publication.Anchors.ArtifactSetId, lease.Request.SourceEncoderContract.Binding.ArtifactSetId);
            AssertAnchorMatches(
                fixture.Paths.TokenizerArtifactPath,
                fixture.Publication.Anchors.TokenizerArtifactSha256,
                fixture.Publication.Anchors.TokenizerArtifactByteLength);
            AssertAnchorMatches(
                fixture.Paths.SourceEncoderContractPath,
                fixture.Publication.Anchors.SourceEncoderContractSha256,
                fixture.Publication.Anchors.SourceEncoderContractByteLength);
            AssertAnchorMatches(
                fixture.Paths.PolicyCheckpointContractPath,
                fixture.Publication.Anchors.PolicyCheckpointContractSha256,
                fixture.Publication.Anchors.PolicyCheckpointContractByteLength);
            Assert.NotNull(lease.Request.TokenizerArtifact);
            Assert.Equal(
                fixture.Publication.TokenizerArtifact.VocabularyIdentitySha256,
                lease.Request.TokenizerArtifact!.VocabularyIdentitySha256);
            Assert.NotNull(lease.Request.RuntimeEncodingBinding);
            Assert.Equal(
                fixture.Publication.TokenizerArtifact.VocabularyIdentitySha256,
                lease.Request.RuntimeEncodingBinding!
                    .TokenizerVocabularyIdentitySha256);
            Assert.Equal(
                fixture.Publication.SourceEncoderContract.Binding
                    .SourceEncoderDescriptor.PolicyTokenMapping,
                lease.Request.RuntimeEncodingBinding.PolicyTokenMapping);

            outputMovedDuringLease =
                await AssertRetainedParentLeasePlatformContractAsync(
                    fixture,
                    movedOutput,
                    policyCheckpointBytes);

            var first = await lease.Request.CheckpointSource.OpenCheckpointSnapshotAsync(
                CancellationToken.None);
            var second = await lease.Request.CheckpointSource.OpenCheckpointSnapshotAsync(
                CancellationToken.None);
            Assert.True(first.IsSuccess, first.ErrorMessage);
            Assert.False(second.IsSuccess);
            Assert.Equal(
                DesignConversionDpoPromotedParentContinuation.SourceAlreadyConsumedErrorCode,
                second.ErrorCode);
            using var transferred = first.Value!;
            Assert.Equal(
                fixture.Publication.PolicyCheckpointContract.Binding.PolicyCheckpointByteLength,
                transferred.Length);
            Assert.Equal(
                policyCheckpointBytes,
                await ReadAllBytesAsync(transferred));
        }
        finally
        {
            lease.Dispose();
        }

        if (!outputMovedDuringLease)
        {
            Directory.Move(fixture.Paths.OutputDirectory, movedOutput);
        }
        Assert.False(Directory.Exists(fixture.Paths.OutputDirectory));
        Assert.True(Directory.Exists(movedOutput));
        Directory.Move(movedOutput, fixture.Paths.OutputDirectory);
    }

    [Fact]
    public async Task CanonicalBpePublishedParentResolvesRetainsSixArtifactsAndExposesPathFreeSnapshot()
    {
        using var fixture = await CanonicalParentFixture.CreateAsync(
            includeCanonicalBpe: true);
        var openedStages = new List<DesignConversionDpoPromotedParentIoStage>();
        DesignConversionDpoCanonicalBpeTokenizerSnapshotReference? snapshot = null;

        var resolved = await DesignConversionDpoPromotedParentContinuation.ResolveAsync(
            fixture.Active,
            fixture.Root,
            CancellationToken.None,
            (stage, _) => openedStages.Add(stage));

        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        var lease = resolved.Value!;
        var movedOutput = fixture.Paths.OutputDirectory + "-moved";
        var outputMovedDuringLease = false;
        var policyCheckpointBytes = await File.ReadAllBytesAsync(
            fixture.PathFor("policy-checkpoint"));
        try
        {
            Assert.Equal(6, fixture.AllArtifactPaths.Count);
            Assert.NotNull(fixture.Publication.CanonicalBpeTokenizer);
            Assert.Contains(
                DesignConversionDpoPromotedParentIoStage
                    .BeforeCanonicalBpeTokenizerArtifactOpen,
                openedStages);
            Assert.Contains(
                DesignConversionDpoPromotedParentIoStage.AfterAllArtifactsOpen,
                openedStages);
            snapshot = Assert.IsType<
                DesignConversionDpoCanonicalBpeTokenizerSnapshotReference>(
                lease.Request.ParentCanonicalBpeTokenizerSnapshot);
            Assert.Equal(
                fixture.Publication.CanonicalBpeTokenizer!.Descriptor,
                snapshot.Descriptor);
            Assert.False(snapshot.Descriptor.RawPayloadPersisted);
            Assert.False(snapshot.Descriptor.AbsolutePathPersisted);

            var opened = await snapshot.OpenSnapshotAsync(CancellationToken.None);
            Assert.True(opened.IsSuccess, opened.ErrorMessage);
            var openedStream = opened.Value!;
            await using (openedStream)
            {
                Assert.Equal(
                    fixture.CanonicalBpeTokenizerBytes,
                    await ReadAllBytesAsync(openedStream));
            }

            outputMovedDuringLease =
                await AssertRetainedParentLeasePlatformContractAsync(
                    fixture,
                    movedOutput,
                    policyCheckpointBytes);

            var firstCheckpoint = await lease.Request.CheckpointSource
                .OpenCheckpointSnapshotAsync(CancellationToken.None);
            Assert.True(firstCheckpoint.IsSuccess, firstCheckpoint.ErrorMessage);
            var firstCheckpointStream = firstCheckpoint.Value!;
            await using (firstCheckpointStream)
            {
                Assert.Equal(
                    policyCheckpointBytes,
                    await ReadAllBytesAsync(firstCheckpointStream));
            }
        }
        finally
        {
            lease.Dispose();
        }

        if (!outputMovedDuringLease)
        {
            Directory.Move(fixture.Paths.OutputDirectory, movedOutput);
        }
        Assert.False(Directory.Exists(fixture.Paths.OutputDirectory));
        Assert.True(Directory.Exists(movedOutput));
        var reopenedAfterMove = await snapshot!.OpenSnapshotAsync(CancellationToken.None);
        Assert.True(reopenedAfterMove.IsSuccess, reopenedAfterMove.ErrorMessage);
        var reopenedAfterMoveStream = reopenedAfterMove.Value!;
        await using (reopenedAfterMoveStream)
        {
            Assert.Equal(
                fixture.CanonicalBpeTokenizerBytes,
                await ReadAllBytesAsync(reopenedAfterMoveStream));
        }
        Directory.Move(movedOutput, fixture.Paths.OutputDirectory);
    }

    [Fact]
    public async Task PromotedParentContinuationUsesSeparateProofAndProviderSourcesEndToEnd()
    {
        using var fixture = await CanonicalParentFixture.CreateAsync(
            evaluatedPublication: true);
        var transferRoles = new List<string>();
        var proofObservations = new List<
            DesignConversionDpoContinuationMutationProofObservation>();
        var sinkObservations = new List<
            DesignConversionDpoOptimizerMutationSinkObservation>();
        var mutationBoundaries = new List<string>();
        var orderingEvents = new List<string>();
        var previousProofObserver = DesignConversionDpoPolicyCheckpointBridge
            .ContinuationMutationProofObserverForTesting;
        var previousSinkObserver = DesignConversionDpoPolicyCheckpointBridge
            .OptimizerMutationSinkObserverForTesting;
        var previousMutationObserver = DesignConversionDpoPolicyCheckpointBridge
            .MutationBoundaryObserverForTesting;
        TypedCompositeBridgeAuthorityFixture? typed = null;
        DesignConversionDpoPromotedParentContinuationLease? continuationLease =
            null;
        DesignConversionDpoEvaluatedBridgeLease? childBridgeLease = null;

        try
        {
            DesignConversionDpoPolicyCheckpointBridge
                .ContinuationMutationProofObserverForTesting =
                observation =>
                {
                    proofObservations.Add(observation);
                    orderingEvents.Add($"proof:{observation.Phase}");
                };
            DesignConversionDpoPolicyCheckpointBridge
                .OptimizerMutationSinkObserverForTesting =
                sinkObservations.Add;
            DesignConversionDpoPolicyCheckpointBridge
                .MutationBoundaryObserverForTesting =
                boundary =>
                {
                    mutationBoundaries.Add(boundary);
                    orderingEvents.Add($"mutation:{boundary}");
                };

            var resolved = await DesignConversionDpoPromotedParentContinuation
                .ResolveAsync(
                    fixture.Active,
                    fixture.Root,
                    CancellationToken.None,
                    (stage, role) =>
                    {
                        if (stage == DesignConversionDpoPromotedParentIoStage
                                .BeforeCheckpointTransfer)
                        {
                            transferRoles.Add(role);
                        }
                    });
            Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
            continuationLease = resolved.Value!;
            var providerSource = Assert.IsType<
                DesignConversionDpoPromotedParentContinuation
                    .OneShotCheckpointSource>(
                continuationLease.Request.CheckpointSource);
            var bridgeProofSource = Assert.IsType<
                DesignConversionDpoPromotedParentContinuation
                    .OneShotCheckpointSource>(
                continuationLease.Request.BridgeProofCheckpointSource);
            Assert.Equal(
                DesignConversionDpoPromotedParentContinuation
                    .ProviderTransferCheckpointSourceRole,
                providerSource.Role);
            Assert.Equal(
                DesignConversionDpoPromotedParentContinuation
                    .BridgeProofCheckpointSourceRole,
                bridgeProofSource.Role);
            Assert.Equal(0, providerSource.TransferCount);
            Assert.Equal(0, bridgeProofSource.TransferCount);

            var provider = DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .CreateWithOptions(
                    new NeverOpenedCheckpointSource(),
                    DesignConversionDpoPolicyBaseTestProvider.CreateConfig(),
                    "test-org/canonical-parent",
                    "revision-gap-3b",
                    fixture.Publication.PolicyCheckpointContract.TrainingContext
                        .BaseCheckpointSha256,
                    DesignConversionDpoPolicyBaseTestProvider
                        .CreateCurrentProviderOptions());
            Assert.True(provider.IsSuccess, provider.ErrorMessage);
            var partition = BuildEvaluatedPartitionForTests(
                "promoted-parent-continuation-e2e",
                heldOutCount: 1);
            var permitObserver =
                new DesignConversionLegalTeacherCapabilityTestFactory
                    .PermitCountingObserverForTesting(orderingEvents.Add);
            var authority = DesignConversionLegalTeacherCapabilityTestFactory
                .CreateAdmittedWithPermitObserver(permitObserver);
            var prepared = await DesignConversionDpoPolicyCheckpointBridge
                .PrepareEvaluatedCompositeExecutionAsync(
                    authority,
                    partition,
                    provider.Value!,
                    new DesignConversionDpoPolicyCheckpointBridgeOptions(1, 1),
                    continuationLease.Request,
                    useContinuation: true,
                    CancellationToken.None);
            Assert.True(prepared.IsSuccess, prepared.ErrorMessage);
            Assert.NotNull(prepared.Value);
            Assert.Equal(0, providerSource.TransferCount);
            Assert.Equal(1, bridgeProofSource.TransferCount);

            typed = CreateTypedCompositeBridgeAuthority(
                authority,
                prepared.Value!,
                "promoted-parent-continuation-e2e");
            var childOutput = Path.Combine(fixture.Root, "child-output");
            var childRuntimeCheckpoint = Path.Combine(
                childOutput,
                "child-runtime.json");
            var executed = await DesignConversionDpoPolicyCheckpointBridge
                .ExecutePreparedEvaluatedCompositeWithTransferredLeaseAsync(
                    typed.ParentLease,
                    typed.Binding,
                    prepared.Value!,
                    childOutput,
                    "promoted-parent-continuation-e2e",
                    childRuntimeCheckpoint,
                    ct: CancellationToken.None);

            Assert.True(executed.IsSuccess, executed.ErrorMessage);
            childBridgeLease = executed.Value!;
            Assert.Equal(1, providerSource.TransferCount);
            Assert.Equal(1, bridgeProofSource.TransferCount);
            Assert.Equal(
            [
                DesignConversionDpoPromotedParentContinuation
                    .BridgeProofCheckpointSourceRole,
                DesignConversionDpoPromotedParentContinuation
                    .ProviderTransferCheckpointSourceRole
            ], transferRoles);
            Assert.Collection(
                proofObservations,
                expected =>
                {
                    Assert.Equal("expected", expected.Phase);
                    Assert.False(string.IsNullOrWhiteSpace(
                        expected.ExpectedModelSetStateFingerprint));
                    Assert.Null(expected.ActualModelSetStateFingerprint);
                },
                actual =>
                {
                    Assert.Equal("actual", actual.Phase);
                    Assert.Equal(
                        actual.ExpectedModelSetStateFingerprint,
                        actual.ActualModelSetStateFingerprint);
                });
            Assert.DoesNotContain(
                "dpo_bridge:evaluated:policy_provider",
                mutationBoundaries);
            Assert.NotEmpty(mutationBoundaries);
            Assert.Equal(
                "dpo_bridge:evaluated:policy_provider_result",
                mutationBoundaries[0]);
            var expectedProofIndex = orderingEvents.IndexOf("proof:expected");
            var actualProofIndex = orderingEvents.IndexOf("proof:actual");
            var firstPermitGrantIndex = orderingEvents.IndexOf(
                "permit:grant");
            var firstActivePermitIndex = orderingEvents.IndexOf(
                "permit:active:1");
            var firstMutationIndex = orderingEvents.IndexOf(
                "mutation:dpo_bridge:evaluated:policy_provider_result");
            Assert.True(expectedProofIndex >= 0, string.Join("|", orderingEvents));
            Assert.True(actualProofIndex > expectedProofIndex, string.Join("|", orderingEvents));
            Assert.True(firstPermitGrantIndex > actualProofIndex, string.Join("|", orderingEvents));
            Assert.True(firstActivePermitIndex > firstPermitGrantIndex, string.Join("|", orderingEvents));
            Assert.True(firstMutationIndex > actualProofIndex, string.Join("|", orderingEvents));
            Assert.True(firstMutationIndex > firstActivePermitIndex, string.Join("|", orderingEvents));
            Assert.True(permitObserver.GrantCount > 0);
            Assert.NotEmpty(permitObserver.ActivePermitCountTransitions);
            Assert.Equal(1, permitObserver.ActivePermitCountTransitions[0]);
            Assert.Single(
                sinkObservations,
                static observation => observation.Stage == "evaluated_dpo");
            Assert.Equal(
                "immediate_parent",
                childBridgeLease.Evaluation.ReferenceMode);
            Assert.True(File.Exists(Path.Combine(
                childOutput,
                childBridgeLease.Result.PolicyCheckpointPath)));
            Assert.True(File.Exists(Path.Combine(
                childOutput,
                childBridgeLease.Result.RuntimeCheckpointPath)));
            childBridgeLease.CommitPublishedArtifacts();
        }
        finally
        {
            childBridgeLease?.Dispose();
            continuationLease?.Dispose();
            DesignConversionDpoPolicyCheckpointBridge
                .ContinuationMutationProofObserverForTesting =
                previousProofObserver;
            DesignConversionDpoPolicyCheckpointBridge
                .OptimizerMutationSinkObserverForTesting =
                previousSinkObserver;
            DesignConversionDpoPolicyCheckpointBridge
                .MutationBoundaryObserverForTesting =
                previousMutationObserver;
            typed?.ParentLease.Dispose();
        }
    }

    private static async Task<bool> AssertRetainedParentLeasePlatformContractAsync(
        CanonicalParentFixture fixture,
        string movedOutput,
        byte[] expectedPolicyCheckpointBytes)
    {
        if (OperatingSystem.IsWindows())
        {
            foreach (var path in fixture.AllArtifactPaths)
            {
                Assert.Throws<IOException>(() => File.Open(
                    path,
                    FileMode.Open,
                    FileAccess.Write,
                    FileShare.ReadWrite | FileShare.Delete));
                Assert.Throws<IOException>(() => File.Delete(path));
            }

            Assert.Throws<IOException>(() => Directory.Move(
                fixture.Paths.OutputDirectory,
                movedOutput));
            Assert.True(Directory.Exists(fixture.Paths.OutputDirectory));
            Assert.False(Directory.Exists(movedOutput));
            return false;
        }

        if (OperatingSystem.IsLinux())
        {
            // На Windows sharing mandatory: чужой write/delete/move обязан
            // упасть. На Linux flock advisory, и произвольный File.Open его
            // законно обходит; поэтому contract oracle здесь другой: legal
            // readers остаются совместимыми, а deferred checkpoint transfer
            // обязан отдать уже валидированный path-free snapshot, даже если
            // имя/живой inode затем были изменены вне cooperative writer'ов.
            foreach (var path in fixture.AllArtifactPaths)
                Assert.NotEmpty(await File.ReadAllBytesAsync(path));

            var policyCheckpointPath = fixture.PathFor("policy-checkpoint");
            await using (var writer = new FileStream(
                             policyCheckpointPath,
                             FileMode.Open,
                             FileAccess.Write,
                             FileShare.ReadWrite | FileShare.Delete))
            {
                writer.Position = 0;
                writer.WriteByte(
                    (byte)(expectedPolicyCheckpointBytes[0] ^ 0x5a));
                writer.Flush(flushToDisk: true);
            }

            Assert.NotEqual(
                expectedPolicyCheckpointBytes,
                await File.ReadAllBytesAsync(policyCheckpointPath));
            Directory.Move(fixture.Paths.OutputDirectory, movedOutput);
            Assert.False(Directory.Exists(fixture.Paths.OutputDirectory));
            Assert.True(Directory.Exists(movedOutput));
            return true;
        }

        throw new PlatformNotSupportedException(
            "Continuation retention tests support Windows and Linux only.");
    }

    [Theory]
    [InlineData("tokenizer-artifact")]
    [InlineData("source-contract")]
    [InlineData("policy-contract")]
    [InlineData("runtime-checkpoint")]
    [InlineData("policy-checkpoint")]
    public async Task CanonicalParentRejectsEveryArtifactTamperWithoutLeakingPaths(
        string artifact)
    {
        using var fixture = await CanonicalParentFixture.CreateAsync();
        var path = fixture.PathFor(artifact);
        var bytes = await File.ReadAllBytesAsync(path);
        bytes[bytes.Length / 2] ^= 0x5a;
        await File.WriteAllBytesAsync(path, bytes);

        var resolved = await DesignConversionDpoPromotedParentContinuation.ResolveAsync(
            fixture.Active,
            fixture.Root,
            CancellationToken.None);

        Assert.False(resolved.IsSuccess);
        Assert.Contains(
            resolved.ErrorCode,
            new[]
            {
                DesignConversionDpoPromotedParentContinuation.ContractInvalidErrorCode,
                DesignConversionDpoPromotedParentContinuation.ArtifactMismatchErrorCode,
                DesignConversionDpoPromotedParentContinuation.TrustFailedErrorCode
            });
        Assert.DoesNotContain(fixture.Root, resolved.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.GetFileName(path), resolved.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("tampered")]
    [InlineData("anchor-binding-mismatch")]
    public async Task CanonicalBpeParentSidecarMutationsFailClosedWithoutLeakingPaths(
        string mutation)
    {
        using var fixture = await CanonicalParentFixture.CreateAsync(
            includeCanonicalBpe: true);
        var active = fixture.Active;
        var path = fixture.Paths.CanonicalBpeTokenizerPath!;
        if (mutation == "missing")
        {
            File.Delete(path);
        }
        else if (mutation == "tampered")
        {
            var bytes = await File.ReadAllBytesAsync(path);
            bytes[bytes.Length / 2] ^= 0x5a;
            await File.WriteAllBytesAsync(path, bytes);
        }
        else
        {
            active = active with
            {
                Anchors = active.Anchors with
                {
                    CanonicalBpeTokenizer =
                        active.Anchors.CanonicalBpeTokenizer! with
                        {
                            FileName = "mismatched-parent" +
                                DesignConversionDpoPromotionArtifactContractCodec
                                    .CanonicalBpeTokenizerArtifactSuffix
                        }
                }
            };
        }

        var resolved = await DesignConversionDpoPromotedParentContinuation.ResolveAsync(
            active,
            fixture.Root,
            CancellationToken.None);

        Assert.False(resolved.IsSuccess);
        Assert.Contains(
            resolved.ErrorCode,
            new[]
            {
                DesignConversionDpoPromotedParentContinuation.NotFoundErrorCode,
                DesignConversionDpoPromotedParentContinuation.ContractInvalidErrorCode,
                DesignConversionDpoPromotedParentContinuation.ArtifactMismatchErrorCode,
                DesignConversionDpoPromotedParentContinuation.TrustFailedErrorCode
            });
        Assert.DoesNotContain(fixture.Root, resolved.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Path.GetFileName(path), resolved.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Null(resolved.Value);
    }

    [Theory]
    [InlineData("source-contract")]
    [InlineData("policy-contract")]
    public async Task NonCanonicalOrOversizedContractIsRejected(string artifact)
    {
        using var fixture = await CanonicalParentFixture.CreateAsync();
        var path = fixture.PathFor(artifact);
        var original = await File.ReadAllBytesAsync(path);
        await File.WriteAllBytesAsync(path, [0x20, .. original]);

        var nonCanonical = await DesignConversionDpoPromotedParentContinuation.ResolveAsync(
            fixture.Active,
            fixture.Root,
            CancellationToken.None);
        Assert.False(nonCanonical.IsSuccess);

        await File.WriteAllBytesAsync(path, original);
        using (var oversized = File.Open(path, FileMode.Open, FileAccess.Write, FileShare.None))
            oversized.SetLength((1024 * 1024) + 1);
        var tooLarge = await DesignConversionDpoPromotedParentContinuation.ResolveAsync(
            fixture.Active,
            fixture.Root,
            CancellationToken.None);
        Assert.False(tooLarge.IsSuccess);
    }

    [Fact]
    public async Task ReplacementAttemptAfterAllFiveOpensFailsClosed()
    {
        using var fixture = await CanonicalParentFixture.CreateAsync();
        var hookObserved = false;
        var replacementSucceeded = false;
        var replacement = Path.Combine(fixture.Root, "replacement.bin");
        await File.WriteAllBytesAsync(replacement, [1, 2, 3]);

        var resolved = await DesignConversionDpoPromotedParentContinuation.ResolveAsync(
            fixture.Active,
            fixture.Root,
            CancellationToken.None,
            (stage, _) =>
            {
                if (stage != DesignConversionDpoPromotedParentIoStage.AfterAllArtifactsOpen)
                    return;
                hookObserved = true;
                try
                {
                    File.Move(
                        replacement,
                        fixture.Paths.PolicyCheckpointContractPath,
                        overwrite: true);
                    replacementSucceeded = true;
                }
                catch (IOException)
                {
                }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                }
            });

        Assert.True(hookObserved);
        if (replacementSucceeded)
        {
            Assert.False(resolved.IsSuccess);
            Assert.Equal(
                DesignConversionDpoPromotedParentContinuation.TrustFailedErrorCode,
                resolved.ErrorCode);
        }
        else
        {
            Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
            resolved.Value!.Dispose();
        }
    }

    [Fact]
    public async Task CompletedRunnerTrainPartitionUsesBatchOrderAgainstFinalSnapshot()
    {
        var fixture = await BuildCompletedRunnerPartitionFixtureAsync();
        var finalSnapshot = fixture.FinalResolvedRecords
            .Select(record => record with
            {
                UsefulnessScore = record.UsefulnessScore + 0.0001
            })
            .Reverse()
            .ToArray();

        var resolved = JsonlDesignConversionLocalArtifactDb
            .ResolveCompletedRunnerDpoTrainPartition(
                fixture.CompletedRunnerResult,
                finalSnapshot,
                CancellationToken.None);
        var baseline = JsonlDesignConversionLocalArtifactDb
            .ResolveCompletedRunnerDpoTrainPartition(
                fixture.CompletedRunnerResult,
                fixture.FinalResolvedRecords,
                CancellationToken.None);

        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        Assert.True(baseline.IsSuccess, baseline.ErrorMessage);
        var expectedBatchOrder = fixture.CompletedRunnerResult.PipelineResult.Batches
            .SelectMany(batch => batch.Records)
            .Select(RecordIdentityHash)
            .ToArray();
        Assert.Equal(
            expectedBatchOrder,
            resolved.Value!.TrainRecords.Select(RecordIdentityHash).ToArray());
        Assert.All(
            resolved.Value.TrainRecords,
            record => Assert.Contains(
                record,
                finalSnapshot));
        Assert.Equal(
            fixture.CompletedRunnerResult.DatasetManifest.HeldOutSources
                .SelectMany(group => group.RecordIdentityHashes)
                .Order(StringComparer.Ordinal),
            resolved.Value.HeldOutRecords.Select(RecordIdentityHash));
        Assert.All(
            new[]
            {
                resolved.Value.TrainRecordSetSha256,
                resolved.Value.HeldOutRecordSetSha256,
                resolved.Value.TrainSourceSetSha256,
                resolved.Value.HeldOutSourceSetSha256,
                resolved.Value.PartitionSnapshotSha256
            },
            hash => Assert.Matches("^[0-9a-f]{64}$", hash));
        Assert.Equal(
            baseline.Value!.TrainRecordSetSha256,
            resolved.Value.TrainRecordSetSha256);
        Assert.Equal(
            baseline.Value.HeldOutRecordSetSha256,
            resolved.Value.HeldOutRecordSetSha256);
        Assert.NotEqual(
            baseline.Value.PartitionSnapshotSha256,
            resolved.Value.PartitionSnapshotSha256);
    }

    [Fact]
    public async Task CancelledTrainPartitionSelectorWinsOverInvalidRunnerResult()
    {
        var fixture = await BuildCompletedRunnerPartitionFixtureAsync();
        var invalid = MutateCompletedRunnerPartition(
            fixture,
            "gate-disagreement");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var resolved = JsonlDesignConversionLocalArtifactDb
            .ResolveCompletedRunnerDpoTrainPartition(
                invalid.CompletedRunnerResult,
                invalid.FinalResolvedRecords,
                cancellation.Token);

        Assert.False(resolved.IsSuccess);
        Assert.Null(resolved.Value);
        Assert.Equal("CANCELLED", resolved.ErrorCode);
        Assert.Equal(
            "DesignConversion local artifact DB operation cancelled.",
            resolved.ErrorMessage);
    }

    [Theory]
    [InlineData("null-result")]
    [InlineData("null-final-snapshot")]
    [InlineData("missing-final-record")]
    [InlineData("new-final-record")]
    [InlineData("duplicate-final-record")]
    [InlineData("agreed-count-mismatch")]
    [InlineData("gate-required-exceeds-actual")]
    [InlineData("gate-disagreement")]
    [InlineData("gate-not-passed")]
    [InlineData("gate-not-source-disjoint")]
    [InlineData("manifest-dataset-mismatch")]
    [InlineData("manifest-domain-mismatch")]
    [InlineData("manifest-accepted-count-mismatch")]
    [InlineData("manifest-noncanonical-hash")]
    [InlineData("manifest-noncanonical-source-hash")]
    [InlineData("manifest-duplicate-hash")]
    [InlineData("manifest-train-heldout-overlap")]
    [InlineData("manifest-train-heldout-source-overlap")]
    [InlineData("empty-batches")]
    [InlineData("duplicate-batch-id-and-record")]
    [InlineData("empty-batch-records")]
    [InlineData("batch-unit-mismatch")]
    [InlineData("batch-record-not-in-train")]
    [InlineData("batch-training-item-count-mismatch")]
    [InlineData("all-training-items-empty")]
    [InlineData("pipeline-training-item-order-mismatch")]
    public async Task CompletedRunnerTrainPartitionRejectsEveryAuthorityMutation(
        string mutation)
    {
        var fixture = await BuildCompletedRunnerPartitionFixtureAsync();
        var mutated = MutateCompletedRunnerPartition(fixture, mutation);

        var resolved = JsonlDesignConversionLocalArtifactDb
            .ResolveCompletedRunnerDpoTrainPartition(
                mutated.CompletedRunnerResult,
                mutated.FinalResolvedRecords,
                CancellationToken.None);

        Assert.False(resolved.IsSuccess);
        Assert.Null(resolved.Value);
        Assert.Equal(
            JsonlDesignConversionLocalArtifactDb
                .DpoPolicyBridgeTrainPartitionInvalidErrorCode,
            resolved.ErrorCode);
        Assert.Equal(
            "The DesignConversion DPO train partition is invalid.",
            resolved.ErrorMessage);
    }

    [Fact]
    public async Task LocalArtifactDbRoutesColdThenContinuationAndReplaysBeforeParentOpen()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-gap-3b-local-db",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
            var basePath = Path.Combine(root, "cold-base.bin");
            new LlmModel(config).SaveCheckpoint(basePath);
            var baseBytes = await File.ReadAllBytesAsync(basePath);
            var coldSource = new CountingByteArrayCheckpointSource(baseBytes);
            var created = DesignConversionCheckpointBackedDpoPolicyBaseProvider.CreateWithOptions(
                coldSource,
                config,
                "test-org/local-db-continuation",
                "revision-gap-3b",
                Hash(baseBytes),
                DesignConversionDpoPolicyBaseTestProvider.CreateCurrentProviderOptions());
            Assert.True(created.IsSuccess, created.ErrorMessage);
            var provider = new RecordingContinuationProvider(created.Value!);
            var db = new JsonlDesignConversionLocalArtifactDb(
                root,
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                dpoPolicyBaseProvider: provider);
            const string semanticPreferenceScope = "gap-3b-semantic-preference";
            await DesignConversionPostEvaluationEligibilityTestFixture
                .SeedApprovedHandoffAsync(
                    db,
                    "gap-3b-train",
                    semanticContentScope: semanticPreferenceScope);
            await DesignConversionPostEvaluationEligibilityTestFixture
                .SeedApprovedHandoffAsync(
                    db,
                    "gap-3b-heldout",
                    semanticContentScope: semanticPreferenceScope);
            var api = new DesignConversionDpoUsefulnessApi(db);
            var handoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(ApprovedTrainingOnly: true),
                DateTimeOffset.UnixEpoch.AddMinutes(300));
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            Assert.Equal(2, handoff.Value!.Rows.Count);
            var intake = await api.CreateTrainingIntakeAsync(
                handoff.Value,
                "requester:gap-3b",
                "exercise cold then retained promoted-parent continuation",
                DateTimeOffset.UnixEpoch.AddMinutes(301));
            Assert.True(intake.IsSuccess, intake.ErrorMessage);

            var coldAdmission = BuildAdmissionMetadata(
                "tenant:gap-3b-routing",
                "idempotency:gap-3b-routing-cold");
            var continuedAdmission = BuildAdmissionMetadata(
                "tenant:gap-3b-routing",
                "idempotency:gap-3b-routing-continued");
            var coldOutput = Path.Combine(root, "cold-promotion");
            var coldOptions = BuildLocalPromotionOptions("gap-3b-cold-run");
            var cold = await RunLocalPromotionWithAdmissionAsync(
                api,
                intake.Value!.IntakeId,
                coldOutput,
                coldOptions,
                coldAdmission);
            Assert.True(cold.IsSuccess, cold.ErrorMessage);
            Assert.True(cold.Value!.LocalPromotionReady);
            Assert.Equal(
                JsonlDesignConversionLocalArtifactDb.TrainingIntakeLocalPromotionSchemaVersion,
                cold.Value.BoundaryMetadata["schema_version"]);
            Assert.Equal(
                JsonlDesignConversionLocalArtifactDb.TrainingIntakeRunSchemaVersion,
                cold.Value.TrainingRun.BoundaryMetadata["schema_version"]);
            Assert.Equal(
                "1",
                cold.Value.TrainingRun.BoundaryMetadata["evaluated_dpo_accuracy"]);
            Assert.Equal(
                "proven",
                cold.Value.TrainingRun.BoundaryMetadata["source_disjoint_status"]);
            AssertColdCycleLineageMetadata(cold.Value.TrainingRun.BoundaryMetadata);
            AssertColdCycleLineageMetadata(cold.Value.BoundaryMetadata);
            await AssertSingleTrainPartitionPolicyEvidenceAsync(
                cold.Value,
                "provider_fresh_step_zero",
                Hash(baseBytes));
            Assert.Equal(1, provider.ColdInvocationCount);
            Assert.Equal(0, provider.ContinuationInvocationCount);
            Assert.Equal(1, coldSource.OpenCount);
            Assert.Equal(1, coldSource.LastStream!.DisposeCount);
            Assert.Equal(
                bool.FalseString,
                cold.Value.BoundaryMetadata[
                    DesignConversionDpoPromotedParentContinuation.ContinuationUsedMetadataKey]);

            var continuedTrainHandoff = await DesignConversionPostEvaluationEligibilityTestFixture
                .SeedApprovedHandoffAsync(
                    db,
                    "gap-3b-continued-train",
                    semanticContentScope: semanticPreferenceScope);
            var continuedHeldOutHandoff = await DesignConversionPostEvaluationEligibilityTestFixture
                .SeedApprovedHandoffAsync(
                    db,
                    "gap-3b-continued-heldout",
                    semanticContentScope: semanticPreferenceScope);
            var continuedHandoffMetadata = new Dictionary<string, string>(
                continuedTrainHandoff.BoundaryMetadata,
                StringComparer.Ordinal)
            {
                ["approved_candidate_count"] = "2"
            };
            var continuedHandoff = continuedTrainHandoff with
            {
                ArtifactId = "approved-dpo-handoff-gap-3b-continued",
                ArtifactRef = "memory://design-conversion/dpo-handoff/gap-3b-continued",
                ApprovedCandidateCount = 2,
                Rows = continuedTrainHandoff.Rows
                    .Concat(continuedHeldOutHandoff.Rows)
                    .ToArray(),
                BoundaryMetadata = continuedHandoffMetadata
            };
            var continuedIntake = await api.CreateTrainingIntakeAsync(
                continuedHandoff,
                "requester:gap-3b-continued",
                "exercise retained promoted-parent continuation with fresh teacher request ids",
                DateTimeOffset.UnixEpoch.AddMinutes(302));
            Assert.True(continuedIntake.IsSuccess, continuedIntake.ErrorMessage);

            var childOutput = Path.Combine(root, "continued-promotion");
            var continuedOptions = BuildLocalPromotionOptions("gap-3b-continued-run") with
            {
                DatasetOptions = coldOptions.DatasetOptions
            };
            var continued = await RunLocalPromotionWithAdmissionAsync(
                api,
                continuedIntake.Value!.IntakeId,
                childOutput,
                continuedOptions,
                continuedAdmission);
            Assert.True(continued.IsSuccess, continued.ErrorMessage);
            Assert.True(continued.Value!.LocalPromotionReady);
            var coldPolicySha256 = cold.Value.PromotedLocalModel!.EvidenceRefs
                .Single(value => value.StartsWith(
                    "dpo_policy_checkpoint_sha256:",
                    StringComparison.Ordinal))
                ["dpo_policy_checkpoint_sha256:".Length..];
            await AssertSingleTrainPartitionPolicyEvidenceAsync(
                continued.Value,
                "immediate_parent",
                coldPolicySha256);
            Assert.Equal(1, provider.ColdInvocationCount);
            Assert.Equal(1, provider.ContinuationInvocationCount);
            Assert.Equal(1, coldSource.OpenCount);
            Assert.Equal(
                bool.TrueString,
                continued.Value.BoundaryMetadata[
                    DesignConversionDpoPromotedParentContinuation.ContinuationUsedMetadataKey]);
            Assert.Equal(
                cold.Value.PromotedLocalModel!.EvidenceRefs
                    .Single(value => value.StartsWith(
                        "dpo_policy_checkpoint_sha256:",
                        StringComparison.Ordinal))
                    ["dpo_policy_checkpoint_sha256:".Length..],
                continued.Value.BoundaryMetadata[
                    DesignConversionDpoPromotedParentContinuation.ParentPolicySha256MetadataKey]);
            var coldActiveRef = cold.Value.BoundaryMetadata["local_promotion_active_local_model_ref"];
            Assert.Equal(
                coldActiveRef,
                continued.Value.BoundaryMetadata["local_promotion_prior_active_local_model_ref"]);
            Assert.Equal(
                DesignConversionDpoPromotedParentContinuation.HashActiveRef(coldActiveRef),
                continued.Value.BoundaryMetadata[
                    DesignConversionDpoPromotedParentContinuation.ParentActiveRefSha256MetadataKey]);
            AssertChangedHeldOutContinuationCycleLineageMetadata(
                continued.Value.TrainingRun.BoundaryMetadata,
                coldPolicySha256,
                "gap-3b-cold-run");
            AssertChangedHeldOutContinuationCycleLineageMetadata(
                continued.Value.BoundaryMetadata,
                coldPolicySha256,
                "gap-3b-cold-run");

            Directory.Delete(childOutput, recursive: true);
            var replay = await RunLocalPromotionWithAdmissionAsync(
                api,
                continuedIntake.Value.IntakeId,
                childOutput,
                continuedOptions,
                continuedAdmission);
            Assert.True(replay.IsSuccess, replay.ErrorMessage);
            Assert.Equal(
                bool.TrueString,
                replay.Value!.BoundaryMetadata["training_intake_idempotency_replay_hit"]);
            Assert.Equal(1, provider.ColdInvocationCount);
            Assert.Equal(1, provider.ContinuationInvocationCount);
            Assert.False(Directory.Exists(childOutput));
            AssertChangedHeldOutContinuationCycleLineageMetadata(
                replay.Value.TrainingRun.BoundaryMetadata,
                coldPolicySha256,
                "gap-3b-cold-run");
            Assert.False(continued.Value.TrainingRun.BoundaryMetadata.ContainsKey(
                "evaluated_dpo_cycle_accuracy_delta"));
            Assert.False(replay.Value.TrainingRun.BoundaryMetadata.ContainsKey(
                "evaluated_dpo_cycle_accuracy_delta"));

            var promotions = await db.ListTrainingIntakeLocalPromotionRecordsAsync();
            Assert.True(promotions.IsSuccess, promotions.ErrorMessage);
            Assert.NotNull(promotions.Value);
            var promotionRows = promotions.Value!;
            var state = JsonlDesignConversionLocalArtifactDb
                .ResolveCurrentLocalPromotionActiveState(promotionRows);
            Assert.Equal(DesignConversionLocalPromotionActiveStateKind.Active, state.Kind);
            Assert.Equal(2, promotionRows.Count(row => row.LocalPromotionReady));
            var coldJournal = promotionRows.Single(row =>
                string.Equals(
                    row.TrainingRun.RunId,
                    "gap-3b-cold-run",
                    StringComparison.Ordinal));
            AssertColdCycleLineageMetadata(coldJournal.TrainingRun.BoundaryMetadata);
            AssertColdCycleLineageMetadata(coldJournal.BoundaryMetadata);
            Assert.True(
                DesignConversionDpoPromotionArtifactContractCodec
                    .TryReadTrustedAnchorMetadata(
                        coldJournal.BoundaryMetadata,
                        out var journaledAnchors));
            Assert.NotNull(journaledAnchors);
            AssertAnchorMatches(
                coldJournal.Publication!.Manifest.PromotionArtifacts!.TokenizerPath,
                journaledAnchors!.TokenizerArtifactSha256,
                journaledAnchors.TokenizerArtifactByteLength);
            AssertAnchorMatches(
                SourceContractPathForPolicyContractPath(
                    coldJournal.Publication.Manifest.PromotionArtifacts.PolicyPath),
                journaledAnchors.SourceEncoderContractSha256,
                journaledAnchors.SourceEncoderContractByteLength);
            AssertAnchorMatches(
                coldJournal.Publication.Manifest.PromotionArtifacts.PolicyPath,
                journaledAnchors.PolicyCheckpointContractSha256,
                journaledAnchors.PolicyCheckpointContractByteLength);
            var childJournal = promotionRows.Single(row =>
                string.Equals(
                    row.TrainingRun.RunId,
                    "gap-3b-continued-run",
                    StringComparison.Ordinal));
            AssertChangedHeldOutContinuationCycleLineageMetadata(
                childJournal.TrainingRun.BoundaryMetadata,
                coldPolicySha256,
                "gap-3b-cold-run");
            AssertChangedHeldOutContinuationCycleLineageMetadata(
                childJournal.BoundaryMetadata,
                coldPolicySha256,
                "gap-3b-cold-run");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData("corrupt")]
    [InlineData("null")]
    public async Task LocalArtifactDbRejectsTransformedCompletedRunnerBeforeBridgeOrPublication(
        string mutation)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-gap-6a-transformed-runner",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
            var basePath = Path.Combine(root, "cold-base.bin");
            new LlmModel(config).SaveCheckpoint(basePath);
            var baseBytes = await File.ReadAllBytesAsync(basePath);
            var coldSource = new CountingByteArrayCheckpointSource(baseBytes);
            var created = DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
                coldSource,
                config,
                "test-org/gap-6a-transformed-runner",
                "revision-gap-6a",
                Hash(baseBytes));
            Assert.True(created.IsSuccess, created.ErrorMessage);
            var provider = new RecordingContinuationProvider(created.Value!);
            var transformCount = 0;
            var db = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                dpoPolicyBaseProvider: provider,
                runTrainingIntakeLocalPromotionCompletedRunnerResultTransformerForTesting:
                    completed =>
                    {
                        transformCount++;
                        return mutation == "null"
                            ? null
                            : completed with
                            {
                                DatasetCountGate = completed.DatasetCountGate with
                                {
                                    ActualTrainRecords =
                                        completed.DatasetCountGate.ActualTrainRecords + 1
                                }
                            };
                    },
                legalTeacherAdmissionAuthority:
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted());
            await DesignConversionPostEvaluationEligibilityTestFixture
                .SeedApprovedHandoffAsync(db, $"gap-6a-{mutation}-train");
            await DesignConversionPostEvaluationEligibilityTestFixture
                .SeedApprovedHandoffAsync(db, $"gap-6a-{mutation}-heldout");
            var api = new DesignConversionDpoUsefulnessApi(db);
            var handoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(ApprovedTrainingOnly: true),
                DateTimeOffset.UnixEpoch.AddMinutes(320));
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            Assert.Equal(2, handoff.Value!.Rows.Count);
            var intake = await api.CreateTrainingIntakeAsync(
                handoff.Value,
                $"requester:gap-6a-{mutation}",
                "reject a transformed completed runner result before the policy bridge",
                DateTimeOffset.UnixEpoch.AddMinutes(321));
            Assert.True(intake.IsSuccess, intake.ErrorMessage);

            var runId = $"gap-6a-transformed-{mutation}";
            var output = Path.Combine(root, $"must-not-exist-{mutation}");
            var failed = await RunLocalPromotionWithAdmissionAsync(
                api,
                intake.Value!.IntakeId,
                output,
                BuildLocalPromotionOptions(runId),
                BuildAdmissionMetadata(
                    $"tenant:gap-6a-{mutation}",
                    $"idempotency:gap-6a-{mutation}"));

            Assert.False(failed.IsSuccess);
            Assert.Null(failed.Value);
            Assert.Equal(1, transformCount);
            Assert.Equal(
                "DESIGN_CONVERSION_TRAINING_INTAKE_PROMOTE_LOCAL_INDETERMINATE_AFTER_INTENT",
                failed.ErrorCode);
            Assert.Equal(
                "DesignConversion local promotion stopped after durable mutation intent; retry is indeterminate and will not repeat training.",
                failed.ErrorMessage);
            Assert.Equal(0, provider.ColdInvocationCount);
            Assert.Equal(0, provider.ContinuationInvocationCount);
            Assert.Equal(0, coldSource.OpenCount);
            Assert.False(Directory.Exists(output));
            Assert.Empty(Directory.EnumerateFiles(
                root,
                $"*{runId}*",
                SearchOption.AllDirectories));
            var promotions = await db.ListTrainingIntakeLocalPromotionRecordsAsync();
            Assert.True(promotions.IsSuccess, promotions.ErrorMessage);
            Assert.Empty(promotions.Value!);
            Assert.DoesNotContain(
                promotions.Value!,
                row => row.LocalPromotionReady || row.Publication is not null);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task CancellationAfterPathObserverWinsOverInvalidTransformedPartition()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-gap-6a-selector-cancellation",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
            var basePath = Path.Combine(root, "cold-base.bin");
            new LlmModel(config).SaveCheckpoint(basePath);
            var baseBytes = await File.ReadAllBytesAsync(basePath);
            var coldSource = new CountingByteArrayCheckpointSource(baseBytes);
            var created = DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
                coldSource,
                config,
                "test-org/gap-6a-selector-cancellation",
                "revision-gap-6a-cancellation",
                Hash(baseBytes));
            Assert.True(created.IsSuccess, created.ErrorMessage);
            var provider = new RecordingContinuationProvider(created.Value!);
            using var cancellation = new CancellationTokenSource();
            var transformCount = 0;
            var observerCount = 0;
            var db = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                dpoPolicyBaseProvider: provider,
                runTrainingIntakeLocalPromotionBeforePolicyBridgePathResolveObserverForTesting:
                    () =>
                    {
                        observerCount++;
                        cancellation.Cancel();
                    },
                runTrainingIntakeLocalPromotionCompletedRunnerResultTransformerForTesting:
                    completed =>
                    {
                        transformCount++;
                        return completed with
                        {
                            DatasetCountGate = completed.DatasetCountGate with
                            {
                                ActualTrainRecords =
                                    completed.DatasetCountGate.ActualTrainRecords + 1
                            }
                        };
                    },
                legalTeacherAdmissionAuthority:
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted());
            await DesignConversionPostEvaluationEligibilityTestFixture
                .SeedApprovedHandoffAsync(db, "gap-6a-cancel-train");
            await DesignConversionPostEvaluationEligibilityTestFixture
                .SeedApprovedHandoffAsync(db, "gap-6a-cancel-heldout");
            var api = new DesignConversionDpoUsefulnessApi(db);
            var handoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(ApprovedTrainingOnly: true),
                DateTimeOffset.UnixEpoch.AddMinutes(330));
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            Assert.Equal(2, handoff.Value!.Rows.Count);
            var intake = await api.CreateTrainingIntakeAsync(
                handoff.Value,
                "requester:gap-6a-cancel",
                "prove cancellation wins immediately before partition selection",
                DateTimeOffset.UnixEpoch.AddMinutes(331));
            Assert.True(intake.IsSuccess, intake.ErrorMessage);

            const string runId = "gap-6a-selector-cancelled";
            var output = Path.Combine(root, "must-not-exist-cancelled");
            var failed = await RunLocalPromotionWithAdmissionAsync(
                api,
                intake.Value!.IntakeId,
                output,
                BuildLocalPromotionOptions(runId),
                BuildAdmissionMetadata(
                    "tenant:gap-6a-cancel",
                    "idempotency:gap-6a-cancel"),
                cancellation.Token);

            Assert.False(failed.IsSuccess);
            Assert.Null(failed.Value);
            Assert.Equal(
                "DESIGN_CONVERSION_TRAINING_INTAKE_PROMOTE_LOCAL_INDETERMINATE_AFTER_INTENT",
                failed.ErrorCode);
            Assert.Equal(
                "DesignConversion local promotion stopped after durable mutation intent; retry is indeterminate and will not repeat training.",
                failed.ErrorMessage);
            Assert.Equal(1, transformCount);
            Assert.Equal(1, observerCount);
            Assert.Equal(0, provider.ColdInvocationCount);
            Assert.Equal(0, provider.ContinuationInvocationCount);
            Assert.Equal(0, coldSource.OpenCount);
            Assert.False(Directory.Exists(output));
            Assert.Empty(Directory.EnumerateFiles(
                root,
                $"*{runId}*",
                SearchOption.AllDirectories));
            var promotions = await db.ListTrainingIntakeLocalPromotionRecordsAsync();
            Assert.True(promotions.IsSuccess, promotions.ErrorMessage);
            Assert.Empty(promotions.Value!);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task EligibilityMutationInPathObserverIsRevalidatedBeforePartitionSelection()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-gap-6a-post-observer-revalidation",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var setup = new JsonlDesignConversionLocalArtifactDb(root);
            await DesignConversionPostEvaluationEligibilityTestFixture
                .SeedApprovedHandoffAsync(setup, "gap-6a-revalidate-train");
            await DesignConversionPostEvaluationEligibilityTestFixture
                .SeedApprovedHandoffAsync(setup, "gap-6a-revalidate-heldout");
            var records = await setup.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery(
                    ApprovedTrainingOnly: true));
            Assert.True(records.IsSuccess, records.ErrorMessage);
            Assert.Equal(2, records.Value!.Count);
            var stateRef = records.Value[0].Metadata[
                JsonDesignConversionPostEvaluationStateStore.StateRefMetadataKey];
            var statePath = Path.Combine(
                root,
                JsonDesignConversionPostEvaluationStateStore.DirectoryName,
                $"{stateRef}.json");
            Assert.True(File.Exists(statePath));

            var setupApi = new DesignConversionDpoUsefulnessApi(setup);
            var handoff = await setupApi.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(ApprovedTrainingOnly: true),
                DateTimeOffset.UnixEpoch.AddMinutes(340));
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            Assert.Equal(2, handoff.Value!.Rows.Count);
            var intake = await setupApi.CreateTrainingIntakeAsync(
                handoff.Value,
                "requester:gap-6a-post-observer-revalidation",
                "revalidate eligibility after the final mutable path observer",
                DateTimeOffset.UnixEpoch.AddMinutes(341));
            Assert.True(intake.IsSuccess, intake.ErrorMessage);

            var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
            var basePath = Path.Combine(root, "cold-base.bin");
            new LlmModel(config).SaveCheckpoint(basePath);
            var baseBytes = await File.ReadAllBytesAsync(basePath);
            var coldSource = new CountingByteArrayCheckpointSource(baseBytes);
            var created = DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
                coldSource,
                config,
                "test-org/gap-6a-post-observer-revalidation",
                "revision-gap-6a-post-observer-revalidation",
                Hash(baseBytes));
            Assert.True(created.IsSuccess, created.ErrorMessage);
            var provider = new RecordingContinuationProvider(created.Value!);
            var observerCount = 0;
            var db = new JsonlDesignConversionLocalArtifactDb(
                root,
                createTrainingIntakeJournalWaitTaskObserverForTesting: null,
                createTrainingIntakeJournalGateAcquiredCallbackForTesting: null,
                dpoPolicyBaseProvider: provider,
                runTrainingIntakeLocalPromotionBeforePolicyBridgePathResolveObserverForTesting:
                    () =>
                    {
                        observerCount++;
                        File.Delete(statePath);
                    },
                legalTeacherAdmissionAuthority:
                    DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted());
            var output = Path.Combine(root, "must-not-exist-revalidation-output");
            var expectedOutput =
                BuildExpectedTrainingIntakeLocalPromotionOutputDirectory(
                    root,
                    output,
                    "gap-6a-post-observer-revalidation");
            var promotion = await RunLocalPromotionWithAdmissionAsync(
                new DesignConversionDpoUsefulnessApi(db),
                intake.Value!.IntakeId,
                output,
                BuildLocalPromotionOptions("gap-6a-post-observer-revalidation"),
                BuildAdmissionMetadata(
                    "tenant:gap-6a-post-observer-revalidation",
                    "idempotency:gap-6a-post-observer-revalidation"));

            Assert.True(promotion.IsSuccess, promotion.ErrorMessage);
            Assert.Equal(1, observerCount);
            var blocked = promotion.Value!;
            Assert.False(blocked.LocalPromotionReady);
            Assert.Null(blocked.Publication);
            Assert.Null(blocked.PromotedLocalModel);
            Assert.Equal(expectedOutput, blocked.PromotionOutputDirectory);
            Assert.Equal(string.Empty, blocked.PromotedCheckpointPath);
            Assert.Contains(
                "local_promotion_blocked_immediately_before_policy_bridge",
                blocked.BlockReasons);
            Assert.Equal(0, provider.ColdInvocationCount);
            Assert.Equal(0, provider.ContinuationInvocationCount);
            Assert.Equal(0, coldSource.OpenCount);
            Assert.False(Directory.Exists(output));
            Assert.False(Directory.Exists(expectedOutput));

            var promotions = await db.ListTrainingIntakeLocalPromotionRecordsAsync();
            Assert.True(promotions.IsSuccess, promotions.ErrorMessage);
            var blockedJournal = Assert.Single(promotions.Value!);
            Assert.False(blockedJournal.LocalPromotionReady);
            Assert.Null(blockedJournal.Publication);
            Assert.Equal(
                expectedOutput,
                blockedJournal.PromotionOutputDirectory);
            Assert.Equal(string.Empty, blockedJournal.PromotedCheckpointPath);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task LocalArtifactDbRejectsJournalWithMissingAnchorBeforeAnyFallbackOrChildOutput()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "design-conversion-gap-3b-invalid-parent",
            Guid.NewGuid().ToString("N"));
        const string privateMarker = "PRIVATE_C:/Users/example/promoted-parent-secret";
        Directory.CreateDirectory(root);
        try
        {
            var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
            var basePath = Path.Combine(root, "cold-base.bin");
            new LlmModel(config).SaveCheckpoint(basePath);
            var baseBytes = await File.ReadAllBytesAsync(basePath);
            var coldSource = new CountingByteArrayCheckpointSource(baseBytes);
            var created = DesignConversionCheckpointBackedDpoPolicyBaseProvider.CreateWithOptions(
                coldSource,
                config,
                "test-org/local-db-invalid-parent",
                "revision-gap-3b",
                Hash(baseBytes),
                DesignConversionDpoPolicyBaseTestProvider.CreateCurrentProviderOptions());
            Assert.True(created.IsSuccess, created.ErrorMessage);
            var provider = new RecordingContinuationProvider(created.Value!);
            var db = new JsonlDesignConversionLocalArtifactDb(
                root,
                DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                dpoPolicyBaseProvider: provider);
            const string semanticPreferenceScope =
                "gap-3b-invalid-parent-semantic-preference";
            await DesignConversionPostEvaluationEligibilityTestFixture
                .SeedApprovedHandoffAsync(
                    db,
                    "gap-3b-invalid-train",
                    semanticContentScope: semanticPreferenceScope);
            await DesignConversionPostEvaluationEligibilityTestFixture
                .SeedApprovedHandoffAsync(
                    db,
                    "gap-3b-invalid-heldout",
                    semanticContentScope: semanticPreferenceScope);
            var api = new DesignConversionDpoUsefulnessApi(db);
            var handoff = await api.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(ApprovedTrainingOnly: true),
                DateTimeOffset.UnixEpoch.AddMinutes(310));
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var intake = await api.CreateTrainingIntakeAsync(
                handoff.Value!,
                "requester:gap-3b-invalid-parent",
                "persist a real active parent before corrupting its trusted anchor",
                DateTimeOffset.UnixEpoch.AddMinutes(311));
            Assert.True(intake.IsSuccess, intake.ErrorMessage);

            var cold = await RunLocalPromotionWithAdmissionAsync(
                api,
                intake.Value!.IntakeId,
                Path.Combine(root, "valid-parent-output"),
                BuildLocalPromotionOptions("gap-3b-invalid-parent-seed"),
                BuildAdmissionMetadata(
                    "tenant:gap-3b-invalid-parent",
                    "idempotency:gap-3b-invalid-parent-seed"));
            Assert.True(cold.IsSuccess, cold.ErrorMessage);
            Assert.True(cold.Value!.LocalPromotionReady);
            Assert.Equal(1, provider.ColdInvocationCount);
            Assert.Equal(0, provider.ContinuationInvocationCount);

            var journalPath = Path.Combine(
                root,
                JsonlDesignConversionLocalArtifactDb
                    .TrainingIntakeLocalPromotionFileName);
            var journalLines = (await File.ReadAllLinesAsync(journalPath))
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();
            var journalLine = Assert.Single(journalLines);
            var journalRecord = JsonSerializer.Deserialize<
                DesignConversionTrainingIntakeLocalPromotionResult>(
                journalLine,
                DesignConversionCheckpointJsonOptions.Standard);
            Assert.NotNull(journalRecord);
            var corruptedMetadata = new Dictionary<string, string>(
                journalRecord!.BoundaryMetadata,
                StringComparer.Ordinal);
            Assert.True(corruptedMetadata.Remove(
                DesignConversionDpoPromotionArtifactContractCodec
                    .SourceContractSha256AnchorMetadataKey));
            var corruptedRecord = journalRecord with
            {
                BoundaryMetadata = corruptedMetadata
            };
            var compactJournalJsonOptions = new JsonSerializerOptions(
                DesignConversionCheckpointJsonOptions.Standard)
            {
                WriteIndented = false
            };
            await File.WriteAllTextAsync(
                journalPath,
                JsonSerializer.Serialize(
                    corruptedRecord,
                    compactJournalJsonOptions) + Environment.NewLine,
                new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier: false,
                    throwOnInvalidBytes: true));

            var childOutput = Path.Combine(root, "must-not-exist-child-output");
            var invalidAdmission = new Dictionary<string, string>(
                BuildAdmissionMetadata(
                    "tenant:gap-3b-invalid-parent",
                    "idempotency:gap-3b-invalid-parent-child"),
                StringComparer.Ordinal)
            {
                ["training_intake_private_path_marker_ref"] = privateMarker
            };
            var failed = await RunLocalPromotionWithAdmissionAsync(
                new DesignConversionDpoUsefulnessApi(
                    new JsonlDesignConversionLocalArtifactDb(
                        root,
                        DesignConversionLegalTeacherCapabilityTestFactory.CreateAdmitted(),
                        dpoPolicyBaseProvider: provider)),
                intake.Value.IntakeId,
                childOutput,
                BuildLocalPromotionOptions("gap-3b-invalid-parent-child"),
                invalidAdmission);

            Assert.False(failed.IsSuccess);
            Assert.Equal(
                DesignConversionDpoPromotedParentContinuation.StateInvalidErrorCode,
                failed.ErrorCode);
            Assert.Equal(
                "The DesignConversion DPO continuation parent state is invalid.",
                failed.ErrorMessage);
            Assert.Equal(1, provider.ColdInvocationCount);
            Assert.Equal(0, provider.ContinuationInvocationCount);
            Assert.Equal(1, coldSource.OpenCount);
            Assert.False(Directory.Exists(childOutput));
            var safeEnvelope = JsonSerializer.Serialize(
                new
                {
                    failed.ErrorCode,
                    failed.ErrorMessage,
                    Metadata = failed.Value?.BoundaryMetadata
                },
                DesignConversionCheckpointJsonOptions.Standard);
            Assert.DoesNotContain(root, safeEnvelope, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(privateMarker, safeEnvelope, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("cold-base.bin", safeEnvelope, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task InvokeAndDisposePassesTokenSanitizesOrdinaryFailureAndReleasesHandles()
    {
        using var fixture = await CanonicalParentFixture.CreateAsync();
        var resolved = await DesignConversionDpoPromotedParentContinuation.ResolveAsync(
            fixture.Active,
            fixture.Root,
            CancellationToken.None);
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        using var cancellation = new CancellationTokenSource();
        var observedToken = default(CancellationToken);

        var invoked = await resolved.Value!.InvokeAndDisposeAsync<string>(
            (_, passedToken) =>
            {
                observedToken = passedToken;
                return Task.FromException<OperationResult<string>>(
                    new IOException("private-operation-detail"));
            },
            cancellation.Token);

        Assert.Equal(cancellation.Token, observedToken);
        Assert.False(invoked.IsSuccess);
        Assert.Equal(
            DesignConversionDpoPromotedParentContinuation.TrustFailedErrorCode,
            invoked.ErrorCode);
        Assert.DoesNotContain("private-operation-detail", invoked.ErrorMessage, StringComparison.Ordinal);
        Directory.Delete(fixture.Paths.OutputDirectory, recursive: true);
    }

    [Fact]
    public async Task InvokeAndDisposeMapsRequestedCancellationAndReleasesHandles()
    {
        using var fixture = await CanonicalParentFixture.CreateAsync();
        var resolved = await DesignConversionDpoPromotedParentContinuation.ResolveAsync(
            fixture.Active,
            fixture.Root,
            CancellationToken.None);
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var invoked = await resolved.Value!.InvokeAndDisposeAsync<string>(
            (_, _) => Task.FromException<OperationResult<string>>(
                new OperationCanceledException(cancellation.Token)),
            cancellation.Token);

        Assert.False(invoked.IsSuccess);
        Assert.Equal("CANCELLED", invoked.ErrorCode);
        Directory.Delete(fixture.Paths.OutputDirectory, recursive: true);
    }

    [Fact]
    public async Task HardLinkedParentArtifactIsRejected()
    {
        if (!OperatingSystem.IsWindows())
            return;
        using var fixture = await CanonicalParentFixture.CreateAsync();
        File.Delete(fixture.Paths.SourceEncoderContractPath);
        Assert.True(CreateHardLink(
            fixture.Paths.SourceEncoderContractPath,
            fixture.Paths.PolicyCheckpointContractPath,
            IntPtr.Zero));

        var resolved = await DesignConversionDpoPromotedParentContinuation.ResolveAsync(
            fixture.Active,
            fixture.Root,
            CancellationToken.None);

        Assert.False(resolved.IsSuccess);
        Assert.Equal(
            DesignConversionDpoPromotedParentContinuation.TrustFailedErrorCode,
            resolved.ErrorCode);
    }

    [Fact]
    public async Task ReparsePointPromotionDirectoryIsRejected()
    {
        using var fixture = await CanonicalParentFixture.CreateAsync();
        var link = Path.Combine(fixture.Root, "linked-parent-output");
        try
        {
            Directory.CreateSymbolicLink(link, fixture.Paths.OutputDirectory);
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }
        catch (IOException)
        {
            return;
        }
        var active = fixture.Active with
        {
            PromotionOutputDirectory = link,
            PromotedCheckpointPath = Rebase(fixture.Active.PromotedCheckpointPath),
            ModelCheckpointPath = Rebase(fixture.Active.ModelCheckpointPath),
            ManifestCheckpointPath = Rebase(fixture.Active.ManifestCheckpointPath),
            ManifestTokenizerArtifactPath = Rebase(
                fixture.Active.ManifestTokenizerArtifactPath),
            ManifestSourceEncoderContractPath = Rebase(
                fixture.Active.ManifestSourceEncoderContractPath),
            ManifestPolicyCheckpointContractPath = Rebase(
                fixture.Active.ManifestPolicyCheckpointContractPath)
        };

        var resolved = await DesignConversionDpoPromotedParentContinuation.ResolveAsync(
            active,
            fixture.Root,
            CancellationToken.None);

        Assert.False(resolved.IsSuccess);
        Assert.Contains(
            resolved.ErrorCode,
            new[]
            {
                DesignConversionDpoPromotedParentContinuation.PathInvalidErrorCode,
                DesignConversionDpoPromotedParentContinuation.TrustFailedErrorCode
            });

        string Rebase(string path)
            => Path.Combine(link, Path.GetFileName(path));
    }

    [Theory]
    [InlineData("outside")]
    [InlineData("traversal")]
    [InlineData("uri")]
    [InlineData("stored-path")]
    public async Task ForgedParentPathsFailBeforeAnyDirectoryOpen(string mutation)
    {
        using var fixture = new ParentFixture();
        var active = Active(fixture.Ready("path", prior: null));
        active = mutation switch
        {
            "outside" => active with
            {
                PromotionOutputDirectory = Path.Combine(
                    Path.GetDirectoryName(fixture.Root)!,
                    "outside")
            },
            "traversal" => active with
            {
                PromotionOutputDirectory = Path.Combine(fixture.Root, "..", "outside")
            },
            "uri" => active with { PromotionOutputDirectory = "file:///forged-parent" },
            _ => active with
            {
                PromotedCheckpointPath = active.PromotedCheckpointPath + ".forged"
            }
        };
        var ioCalls = 0;

        var resolved = await DesignConversionDpoPromotedParentContinuation.ResolveAsync(
            active,
            fixture.Root,
            CancellationToken.None,
            (_, _) => ioCalls++);

        Assert.False(resolved.IsSuccess);
        Assert.Equal(
            DesignConversionDpoPromotedParentContinuation.PathInvalidErrorCode,
            resolved.ErrorCode);
        Assert.Equal(0, ioCalls);
    }

    [Fact]
    public async Task MissingParentDirectoryIsNotCreated()
    {
        using var fixture = new ParentFixture();
        var active = Active(fixture.Ready("missing", prior: null));
        Assert.False(Directory.Exists(active.PromotionOutputDirectory));

        var resolved = await DesignConversionDpoPromotedParentContinuation.ResolveAsync(
            active,
            fixture.Root,
            CancellationToken.None);

        Assert.False(resolved.IsSuccess);
        Assert.Equal(
            DesignConversionDpoPromotedParentContinuation.NotFoundErrorCode,
            resolved.ErrorCode);
        Assert.False(Directory.Exists(active.PromotionOutputDirectory));
    }

    [Fact]
    public async Task RequestedCancellationPrecedesPathAndIoResolution()
    {
        using var fixture = new ParentFixture();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var ioCalls = 0;

        var resolved = await DesignConversionDpoPromotedParentContinuation.ResolveAsync(
            Active(fixture.Ready("cancel", prior: null)),
            fixture.Root,
            cancellation.Token,
            (_, _) => ioCalls++);

        Assert.False(resolved.IsSuccess);
        Assert.Equal(
            DesignConversionDpoPromotedParentContinuation.CancelledErrorCode,
            resolved.ErrorCode);
        Assert.Equal(0, ioCalls);
    }

    [Fact]
    public async Task OneShotSourceCancelsBeforeTransferThenTransfersExactlyOnce()
    {
        byte[] bytes = [1, 2, 3];
        using var source = new DesignConversionDpoPromotedParentContinuation
            .OneShotCheckpointSource(
                bytes,
                DesignConversionDpoPromotedParentContinuation
                    .ProviderTransferCheckpointSourceRole,
                beforeIoForTesting: null);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var cancelled = await source.OpenCheckpointSnapshotAsync(cancellation.Token);
        Assert.False(cancelled.IsSuccess);
        Assert.Equal(
            DesignConversionDpoPromotedParentContinuation.CancelledErrorCode,
            cancelled.ErrorCode);
        Assert.Equal(0, source.TransferCount);

        var first = await source.OpenCheckpointSnapshotAsync(CancellationToken.None);
        var second = await source.OpenCheckpointSnapshotAsync(CancellationToken.None);
        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.False(second.IsSuccess);
        Assert.Equal(
            DesignConversionDpoPromotedParentContinuation.SourceAlreadyConsumedErrorCode,
            second.ErrorCode);
        Assert.Equal(1, source.TransferCount);

        source.Dispose();
        using var transferred = first.Value!;
        Assert.Equal(bytes, await ReadAllBytesAsync(transferred));
    }

    [Fact]
    public async Task NeverTransferredOneShotSourceDisposesCustodyWithoutTransfer()
    {
        var source = new DesignConversionDpoPromotedParentContinuation
            .OneShotCheckpointSource(
                [1],
                DesignConversionDpoPromotedParentContinuation
                    .ProviderTransferCheckpointSourceRole,
                beforeIoForTesting: null);

        source.Dispose();
        source.Dispose();

        var opened = await source.OpenCheckpointSnapshotAsync(CancellationToken.None);

        Assert.False(opened.IsSuccess);
        Assert.Equal(
            DesignConversionDpoPromotedParentContinuation.SourceAlreadyConsumedErrorCode,
            opened.ErrorCode);
        Assert.Equal(0, source.TransferCount);
    }

    [Fact]
    public async Task FatalBeforeTransferEscapesByIdentityAndLeavesCustodyForRetry()
    {
        var fatal = new OutOfMemoryException("fatal-before-transfer");
        var throwOnce = true;
        using var source = new DesignConversionDpoPromotedParentContinuation
            .OneShotCheckpointSource(
                [1],
                DesignConversionDpoPromotedParentContinuation
                    .ProviderTransferCheckpointSourceRole,
                (stage, _) =>
                {
                    if (stage == DesignConversionDpoPromotedParentIoStage
                            .BeforeCheckpointTransfer &&
                        throwOnce)
                    {
                        throwOnce = false;
                        throw fatal;
                    }
                });

        var caught = await Assert.ThrowsAsync<OutOfMemoryException>(
            () => source.OpenCheckpointSnapshotAsync(CancellationToken.None));

        Assert.Same(fatal, caught);
        Assert.Equal(0, source.TransferCount);

        var retry = await source.OpenCheckpointSnapshotAsync(CancellationToken.None);

        Assert.True(retry.IsSuccess, retry.ErrorMessage);
        Assert.Equal(1, source.TransferCount);
        using var transferred = retry.Value!;
        Assert.Equal(new byte[] { 1 }, await ReadAllBytesAsync(transferred));
    }

    [Fact]
    public void OrdinaryCleanupFailureIsSanitizedAndLaterResourcesStillDispose()
    {
        var ordinary = new TrackingDisposable(
            new IOException("ordinary-retained-cleanup"));
        var later = new TrackingDisposable(exception: null);

        DesignConversionDpoPromotedParentContinuation.DisposeResources(
            beforeIoForTesting: null,
            ordinary,
            later);

        Assert.Equal(1, ordinary.DisposeCount);
        Assert.Equal(1, later.DisposeCount);
    }

    [Fact]
    public void FatalCleanupGraphEscapesByIdentityAfterAllResourcesDispose()
    {
        var fatal = new AggregateException(
            "nested-unrequested-cancellation",
            new OperationCanceledException("unrequested"));
        var first = new TrackingDisposable(fatal);
        var later = new TrackingDisposable(exception: null);

        var caught = Assert.Throws<AggregateException>(() =>
            DesignConversionDpoPromotedParentContinuation.DisposeResources(
                beforeIoForTesting: null,
                first,
                later));

        Assert.Same(fatal, caught);
        Assert.Equal(1, first.DisposeCount);
        Assert.Equal(1, later.DisposeCount);
    }

    [Fact]
    public async Task ProviderContractRejectBeforeConsumeLeavesOneShotOwnershipWithHelper()
    {
        var source = new DesignConversionDpoPromotedParentContinuation
            .OneShotCheckpointSource(
                [1],
                DesignConversionDpoPromotedParentContinuation
                    .ProviderTransferCheckpointSourceRole,
                beforeIoForTesting: null);
        var config = new ModelConfig
        {
            VocabSize = 256,
            ContextLen = 8,
            HiddenDim = 8,
            NumLayers = 1,
            NumHeads = 2,
            NumKvHeads = 1,
            FfnHiddenDim = 16,
            RopeTheta = 10_000,
            RmsNormEps = 1e-5f
        };
        var provider = DesignConversionCheckpointBackedDpoPolicyBaseProvider.Create(
            new NeverOpenedCheckpointSource(),
            config,
            "parent-model",
            "parent-revision",
            Hash("cold-parent"));
        Assert.True(provider.IsSuccess, provider.ErrorMessage);

        var rejected = await provider.Value!.CreateContinuedModelSetAsync(
            new DesignConversionDpoPolicyContinuationRequest(source, null!, null!),
            CancellationToken.None);

        Assert.False(rejected.IsSuccess);
        Assert.Equal(
            DesignConversionCheckpointBackedDpoPolicyBaseProvider
                .ContinuationContractInvalidErrorCode,
            rejected.ErrorCode);
        Assert.Equal(0, source.TransferCount);

        var transferred = await source.OpenCheckpointSnapshotAsync(
            CancellationToken.None);
        Assert.True(transferred.IsSuccess, transferred.ErrorMessage);
        Assert.Equal(1, source.TransferCount);
        using var stream = transferred.Value!;
        Assert.Equal(new byte[] { 1 }, await ReadAllBytesAsync(stream));
    }

    private static DesignConversionDpoResolvedPartition
        BuildEvaluatedPartitionForTests(
            string id,
            int heldOutCount)
    {
        var train = WithSourceIdentity(
            BuildTrainingRecord(
                $"{id}-train",
                DesignConversionDpoUnits.ImproverUnit,
                usefulness: 0.99),
            $"{id}-source-train");
        var heldOut = Enumerable.Range(0, heldOutCount)
            .Select(index => WithSourceIdentity(
                BuildTrainingRecord(
                    $"{id}-heldout-{index}",
                    index % 2 == 0
                        ? DesignConversionDpoUnits.CriticUnit
                        : DesignConversionDpoUnits.ImproverUnit,
                    usefulness: 0.98 - index * 0.01),
                $"{id}-source-heldout-{index}"))
            .ToArray();
        var provisional = new DesignConversionDpoResolvedPartition(
            id,
            [train],
            heldOut,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);
        Assert.True(DesignConversionDpoConsumedPartitionProjectionContract
            .TryCreate(
                provisional,
                provisional.TrainRecords.Count,
                out var full));
        Assert.NotNull(full);
        return provisional with
        {
            TrainRecordSetSha256 = full!.TrainRecordSetSha256,
            HeldOutRecordSetSha256 = full.HeldOutRecordSetSha256,
            TrainSourceSetSha256 = full.TrainSourceSetSha256,
            HeldOutSourceSetSha256 = full.HeldOutSourceSetSha256,
            PartitionSnapshotSha256 = full.PartitionSnapshotSha256
        };
    }

    private static DesignConversionDpoTrainingRecord BuildTrainingRecord(
        string id,
        string unitId,
        double usefulness)
    {
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords(
            [DesignConversionDpoGeneratorTests.CreateExample(id, unitId, usefulness)]);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.QuarantinedRecords);
        return Assert.Single(build.Value.AcceptedRecords);
    }

    private static DesignConversionDpoTrainingRecord WithSourceIdentity(
        DesignConversionDpoTrainingRecord record,
        string sourceId)
    {
        var metadata = new Dictionary<string, string>(
            record.Provenance.Metadata,
            StringComparer.Ordinal)
        {
            ["source_design_id"] = sourceId
        };

        return record with
        {
            Provenance = record.Provenance with
            {
                SourceRequestId = $"request://design-conversion/{sourceId}",
                SourceManifestRef = $"manifest://design-conversion/{sourceId}",
                Metadata = metadata
            }
        };
    }

    private static TypedCompositeBridgeAuthorityFixture
        CreateTypedCompositeBridgeAuthority(
            ServerOwnedLegalTrainingTeacherAdmissionAuthority authority,
            DesignConversionDpoPolicyCheckpointBridge
                .PreparedEvaluatedCompositeExecution prepared,
            string id)
    {
        var bridgeInput = prepared.FinalOptimizer.Inputs[0];
        var pipelineInput = new LearningTrainingItem(
            $"{id}:pipeline:0",
            bridgeInput.Prompt,
            bridgeInput.Chosen,
            bridgeInput.Rejected);
        var pipeline = DesignConversionDpoTrainingContentAuthority
            .ProjectFinalOptimizerInputs(
                [pipelineInput],
                [TrainingTeacherProvenance.Deterministic(
                    bridgeInput.Provenance!.RequestId)],
                static input => input.Prompt,
                static input => input.ChosenResponse,
                static input => input.RejectedResponse,
                static _ => Array.Empty<float>(),
                static _ => null,
                static input =>
                    $"typed-composite-bridge-test|item={input.ItemId}");
        Assert.True(pipeline.IsSuccess, pipeline.ErrorMessage);
        var pair = DpoPairTrainingContentAuthority.Bind(
            new DpoPair(
                pipelineInput.Prompt,
                pipelineInput.ChosenResponse,
                pipelineInput.RejectedResponse,
                DesignConversionMachine.DomainTag,
                DateTimeOffset.UnixEpoch)
            {
                TeacherProvenance = pipeline.Value!.Provenance[0],
                GovernanceDecision = new TrainingDataGovernanceDecision(
                    licenseAccepted: true,
                    isQuarantined: false,
                    ["sha256:" + new string('d', 64)])
            });
        Assert.True(pair.IsSuccess, pair.ErrorMessage);
        var commitment = DesignConversionDpoTrainingContentAuthority
            .ProjectCompositeOptimizerInputs(
                pipeline.Value,
                [pair.Value!],
                prepared.FinalOptimizer);
        Assert.True(commitment.IsSuccess, commitment.ErrorMessage);
        var lease = authority.AcquireTrainingLease(
            commitment.Value!.FinalOptimizer.Content,
            commitment.Value.FinalOptimizer.Provenance);
        Assert.True(lease.IsSuccess, lease.ErrorMessage);
        var binding = DesignConversionPipelineTrainingBinding.MintComposite(
            commitment.Value,
            pipeline.Value,
            prepared.FinalOptimizer,
            lease.Value);
        Assert.True(binding.IsSuccess, binding.ErrorMessage);
        return new TypedCompositeBridgeAuthorityFixture(
            commitment.Value,
            binding.Value!,
            lease.Value!);
    }

    private sealed record TypedCompositeBridgeAuthorityFixture(
        DesignConversionDpoTrainingContentAuthority
            .CompositeOptimizerCommitment Commitment,
        DesignConversionPipelineTrainingBinding Binding,
        ServerOwnedLegalTrainingTeacherAdmissionLease ParentLease);

    private static DesignConversionLocalPromotionActive Active(
        DesignConversionTrainingIntakeLocalPromotionResult record)
    {
        var resolved = JsonlDesignConversionLocalArtifactDb
            .ResolveCurrentLocalPromotionActiveState([record]);
        Assert.Equal(DesignConversionLocalPromotionActiveStateKind.Active, resolved.Kind);
        return resolved.Active!;
    }

    private static DesignConversionTrainingIntakeLocalPromotionResult Rollback(
        DesignConversionTrainingIntakeLocalPromotionResult target,
        DesignConversionTrainingIntakeLocalPromotionResult restored,
        string rollbackId)
    {
        var targetRef = target.BoundaryMetadata["local_promotion_active_local_model_ref"];
        var restoredRef = restored.BoundaryMetadata["local_promotion_active_local_model_ref"];
        var restoredModel = restored.PromotedLocalModel!;
        var metadata = new Dictionary<string, string>(target.BoundaryMetadata, StringComparer.Ordinal)
        {
            ["schema_version"] = JsonlDesignConversionLocalArtifactDb
                .TrainingIntakeLocalPromotionRollbackSchemaVersion,
            ["local_promotion_record_kind"] = "rollback",
            ["local_promotion_rollback_id"] = rollbackId,
            ["local_promotion_rollback_applied"] = bool.TrueString,
            ["local_promotion_prior_state_restored"] = bool.TrueString,
            ["local_promotion_rollback_target_local_model_ref"] = targetRef,
            ["local_promotion_rollback_previous_active_local_model_ref"] = targetRef,
            ["local_promotion_rollback_restored_active_local_model_ref"] = restoredRef,
            ["local_promotion_rollback_restored_active_model_id"] = restoredModel.ModelId,
            ["local_promotion_rollback_restored_active_checkpoint_sha256"] =
                restoredModel.CheckpointSha256,
            ["local_promotion_active_local_model_ref"] = restoredRef,
            ["local_promotion_active_model_id"] = restoredModel.ModelId,
            ["local_promotion_active_checkpoint_sha256"] = restoredModel.CheckpointSha256
        };
        return target with { BoundaryMetadata = metadata };
    }

    private static IReadOnlyDictionary<string, string> With(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        string value)
    {
        var result = new Dictionary<string, string>(metadata, StringComparer.Ordinal)
        {
            [key] = value
        };
        return result;
    }

    private static string ActiveRef(string scope)
        => $"local://design-conversion/promoted-local-model/{scope}";

    private static string Hash(string value)
        => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)))
            .ToLowerInvariant();

    private static string Hash(byte[] value)
        => Convert.ToHexString(SHA256.HashData(value)).ToLowerInvariant();

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream)
    {
        stream.Position = 0;
        using var copy = new MemoryStream();
        await stream.CopyToAsync(copy);
        return copy.ToArray();
    }

    private static void AssertAnchorMatches(string path, string sha256, long byteLength)
    {
        var bytes = File.ReadAllBytes(path);
        Assert.Equal(byteLength, bytes.LongLength);
        Assert.Equal(sha256, Hash(bytes));
    }

    private static string SourceContractPathForPolicyContractPath(string policyContractPath)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(policyContractPath))!;
        var policyLeaf = Path.GetFileName(policyContractPath);
        Assert.EndsWith(
            DesignConversionDpoPromotionArtifactContractCodec
                .PolicyCheckpointContractSuffix,
            policyLeaf,
            StringComparison.Ordinal);
        var sourceLeaf =
            policyLeaf[..^DesignConversionDpoPromotionArtifactContractCodec
                .PolicyCheckpointContractSuffix.Length] +
            DesignConversionDpoPromotionArtifactContractCodec
                .SourceEncoderContractSuffix;
        return Path.Combine(directory, sourceLeaf);
    }

    private static async Task AssertSingleTrainPartitionPolicyEvidenceAsync(
        DesignConversionTrainingIntakeLocalPromotionResult promotion,
        string expectedReferenceMode,
        string expectedReferenceSha256)
    {
        var runnerEvidence = Assert.IsType<DesignConversionTrainingIntakeRunnerEvidence>(
            promotion.TrainingRun.RunnerEvidence);
        Assert.Equal(1, runnerEvidence.ActualTrainRecords);
        Assert.Equal(1, runnerEvidence.ActualHeldOutRecords);
        Assert.True(runnerEvidence.SourceDisjoint);

        var artifacts = Assert.IsType<DesignConversionTrainingManifestArtifacts>(
            promotion.Publication?.Manifest.PromotionArtifacts);
        var sourceContract = JsonSerializer.Deserialize<
            DesignConversionDpoSourceEncoderContract>(
                await File.ReadAllBytesAsync(SourceContractPathForPolicyContractPath(
                    artifacts.PolicyPath)),
                DesignConversionCheckpointJsonOptions.Standard);
        var policyContract = JsonSerializer.Deserialize<
            DesignConversionDpoPolicyCheckpointContract>(
                await File.ReadAllBytesAsync(artifacts.PolicyPath),
                DesignConversionCheckpointJsonOptions.Standard);
        Assert.NotNull(sourceContract);
        Assert.NotNull(policyContract);
        Assert.True(DesignConversionDpoPromotionArtifactContractCodec.ValidateContractPair(
            sourceContract,
            policyContract));
        var evaluation = Assert.IsType<DesignConversionDpoHeldOutEvaluation>(
            policyContract!.Evaluation);
        Assert.Equal(expectedReferenceMode, evaluation.ReferenceMode);
        Assert.Equal(expectedReferenceSha256, evaluation.ReferencePolicyCheckpointSha256);
        Assert.Equal(
            policyContract.Binding.PolicyCheckpointSha256,
            evaluation.ChildPolicyCheckpointSha256);
        Assert.Equal(2, policyContract!.SourceRecordCount);
        Assert.Equal(1, policyContract.BridgePreferenceCount);

        var runtime = await DesignConversionMachine.LoadCheckpointAsync(
            promotion.PromotedCheckpointPath);
        Assert.True(runtime.IsSuccess, runtime.ErrorMessage);
        var runtimePolicyRef = Assert.IsType<DesignConversionDpoPolicyCheckpointRef>(
            runtime.Value!.Checkpoint.DpoPolicyCheckpointRef);
        Assert.True(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Validate(
            runtimePolicyRef.TrainingContext.LegalTeacherAdmissionCommitment));
        Assert.Equal(
            policyContract!.TrainingContext.LegalTeacherAdmissionCommitment,
            runtimePolicyRef.TrainingContext.LegalTeacherAdmissionCommitment);
        Assert.True(DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.MatchesCheckpointEvidence(
            runtimePolicyRef.TrainingContext.LegalTeacherAdmissionCommitment,
            runtimePolicyRef.EvidenceRefs));
        Assert.Equal(2, runtimePolicyRef.SourceRecordCount);
        Assert.Equal(1, runtimePolicyRef.BridgePreferenceCount);
    }

    private static async Task<CompletedRunnerPartitionFixture>
        BuildCompletedRunnerPartitionFixtureAsync()
    {
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords(
        [
            DesignConversionDpoGeneratorTests.CreateExample(
                "gap-6a-selector-a",
                DesignConversionDpoUnits.CriticUnit,
                0.99),
            DesignConversionDpoGeneratorTests.CreateExample(
                "gap-6a-selector-b",
                DesignConversionDpoUnits.ImproverUnit,
                0.98),
            DesignConversionDpoGeneratorTests.CreateExample(
                "gap-6a-selector-c",
                DesignConversionDpoUnits.CriticUnit,
                0.97),
            DesignConversionDpoGeneratorTests.CreateExample(
                "gap-6a-selector-d",
                DesignConversionDpoUnits.ImproverUnit,
                0.96)
        ]);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        Assert.Empty(build.Value!.QuarantinedRecords);
        var records = build.Value.AcceptedRecords
            .Select((record, index) => record with
            {
                Provenance = record.Provenance with
                {
                    SourceRequestId = $"gap-6a-request-{index}",
                    SourceManifestRef = $"manifest://gap-6a/source-{index}"
                }
            })
            .ToArray();
        var runId = $"gap-6a-selector-{Guid.NewGuid():N}";
        var completed = await DesignConversionLegalTeacherCapabilityTestFactory
            .RunStrictRunnerAsync(
                new DesignConversionTrainingRunner(),
                new DesignConversionMachine(),
                records,
                BuildLocalPromotionOptions(runId),
                CancellationToken.None);
        Assert.True(completed.IsSuccess, completed.ErrorMessage);
        Assert.True(completed.Value!.DatasetCountGate.Passed);
        Assert.True(completed.Value.DatasetCountGate.SourceDisjoint);
        Assert.True(completed.Value.DatasetCountGate.ActualTrainRecords > 1);
        Assert.True(completed.Value.DatasetCountGate.ActualHeldOutRecords > 0);

        return new CompletedRunnerPartitionFixture(completed.Value, records);
    }

    private static CompletedRunnerPartitionMutation MutateCompletedRunnerPartition(
        CompletedRunnerPartitionFixture fixture,
        string mutation)
    {
        DesignConversionTrainingRunResult? completed = fixture.CompletedRunnerResult;
        IReadOnlyList<DesignConversionDpoTrainingRecord>? final =
            fixture.FinalResolvedRecords;
        var pipeline = completed.PipelineResult;
        var firstBatch = pipeline.Batches[0];

        switch (mutation)
        {
            case "null-result":
                completed = null;
                break;
            case "null-final-snapshot":
                final = null;
                break;
            case "missing-final-record":
                final = final.Skip(1).ToArray();
                break;
            case "new-final-record":
                final = final.Append(final[0] with
                {
                    RecordId = final[0].RecordId + "-new"
                }).ToArray();
                break;
            case "duplicate-final-record":
                final = final.Append(final[0]).ToArray();
                break;
            case "agreed-count-mismatch":
                completed = WithAllDatasetCountGates(
                    completed,
                    gate => gate with
                    {
                        ActualTrainRecords = gate.ActualTrainRecords + 1
                    });
                break;
            case "gate-required-exceeds-actual":
                completed = WithAllDatasetCountGates(
                    completed,
                    gate => gate with
                    {
                        RequiredTrainRecords = gate.ActualTrainRecords + 1
                    });
                break;
            case "gate-disagreement":
                completed = completed with
                {
                    PipelineResult = pipeline with
                    {
                        DatasetCountGate = pipeline.DatasetCountGate with
                        {
                            RequiredTrainRecords =
                                pipeline.DatasetCountGate.RequiredTrainRecords + 1
                        }
                    }
                };
                break;
            case "gate-not-passed":
                completed = WithAllDatasetCountGates(
                    completed,
                    gate => gate with { Passed = false });
                break;
            case "gate-not-source-disjoint":
                completed = WithAllDatasetCountGates(
                    completed,
                    gate => gate with { SourceDisjoint = false });
                break;
            case "manifest-dataset-mismatch":
                completed = completed with
                {
                    PipelineResult = pipeline with
                    {
                        DatasetManifest = pipeline.DatasetManifest with
                        {
                            DatasetId = pipeline.DatasetManifest.DatasetId + "-other"
                        }
                    }
                };
                break;
            case "manifest-domain-mismatch":
                completed = WithBothDatasetManifests(
                    completed,
                    manifest => manifest with { DomainTag = "other-domain" });
                break;
            case "manifest-accepted-count-mismatch":
                completed = completed with
                {
                    DatasetManifest = completed.DatasetManifest with
                    {
                        AcceptedRecordCount =
                            completed.DatasetManifest.AcceptedRecordCount + 1
                    },
                    PipelineResult = pipeline with
                    {
                        AcceptedRecordCount = pipeline.AcceptedRecordCount + 1,
                        DatasetManifest = pipeline.DatasetManifest with
                        {
                            AcceptedRecordCount =
                                pipeline.DatasetManifest.AcceptedRecordCount + 1
                        }
                    }
                };
                break;
            case "manifest-noncanonical-hash":
                completed = WithBothDatasetManifests(
                    completed,
                    manifest => ReplaceFirstManifestIdentityHash(
                        manifest,
                        train: true,
                        value => "g" + value[1..]));
                break;
            case "manifest-noncanonical-source-hash":
                completed = WithBothDatasetManifests(
                    completed,
                    manifest => ReplaceFirstManifestSourceIdentityHash(
                        manifest,
                        train: true,
                        value => "g" + value[1..]));
                break;
            case "manifest-duplicate-hash":
                completed = WithBothDatasetManifests(
                    completed,
                    manifest => DuplicateFirstManifestIdentityHash(
                        manifest,
                        train: true));
                break;
            case "manifest-train-heldout-overlap":
                var trainIdentityHash = completed.DatasetManifest
                    .TrainSources[0]
                    .RecordIdentityHashes[0];
                completed = WithBothDatasetManifests(
                    completed,
                    manifest => ReplaceFirstManifestIdentityHash(
                        manifest,
                        train: false,
                        _ => trainIdentityHash));
                break;
            case "manifest-train-heldout-source-overlap":
                var trainSourceIdentityHash = completed.DatasetManifest
                    .TrainSources[0]
                    .SourceIdentityHash;
                completed = WithBothDatasetManifests(
                    completed,
                    manifest => ReplaceFirstManifestSourceIdentityHash(
                        manifest,
                        train: false,
                        _ => trainSourceIdentityHash));
                break;
            case "empty-batches":
                completed = completed with
                {
                    PipelineResult = pipeline with { Batches = [] }
                };
                break;
            case "duplicate-batch-id-and-record":
                completed = completed with
                {
                    PipelineResult = pipeline with
                    {
                        Batches = pipeline.Batches.Append(firstBatch).ToArray()
                    }
                };
                break;
            case "empty-batch-records":
                completed = ReplaceFirstBatch(
                    completed,
                    firstBatch with { Records = [] });
                break;
            case "batch-unit-mismatch":
                completed = ReplaceFirstBatch(
                    completed,
                    firstBatch with { UnitId = firstBatch.UnitId + "-other" });
                break;
            case "batch-record-not-in-train":
                var replacementRecords = firstBatch.Records.ToArray();
                replacementRecords[0] = replacementRecords[0] with
                {
                    RecordId = replacementRecords[0].RecordId + "-other"
                };
                completed = ReplaceFirstBatch(
                    completed,
                    firstBatch with { Records = replacementRecords });
                break;
            case "batch-training-item-count-mismatch":
                completed = ReplaceFirstBatch(
                    completed,
                    firstBatch with { TrainingItems = [] });
                break;
            case "all-training-items-empty":
                completed = completed with
                {
                    PipelineResult = pipeline with
                    {
                        Batches = pipeline.Batches
                            .Select(batch => batch with { TrainingItems = [] })
                            .ToArray(),
                        TrainingItems = []
                    }
                };
                break;
            case "pipeline-training-item-order-mismatch":
                completed = completed with
                {
                    PipelineResult = pipeline with
                    {
                        TrainingItems = pipeline.TrainingItems.Reverse().ToArray()
                    }
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null);
        }

        return new CompletedRunnerPartitionMutation(completed, final);
    }

    private static DesignConversionTrainingRunResult WithAllDatasetCountGates(
        DesignConversionTrainingRunResult completed,
        Func<DesignConversionDpoDatasetCountGate, DesignConversionDpoDatasetCountGate>
            transform)
    {
        var pipeline = completed.PipelineResult;
        return completed with
        {
            DatasetCountGate = transform(completed.DatasetCountGate),
            DatasetManifest = completed.DatasetManifest with
            {
                CountGate = transform(completed.DatasetManifest.CountGate)
            },
            PipelineResult = pipeline with
            {
                DatasetCountGate = transform(pipeline.DatasetCountGate),
                DatasetManifest = pipeline.DatasetManifest with
                {
                    CountGate = transform(pipeline.DatasetManifest.CountGate)
                }
            }
        };
    }

    private static DesignConversionTrainingRunResult WithBothDatasetManifests(
        DesignConversionTrainingRunResult completed,
        Func<DesignConversionDpoDatasetManifest, DesignConversionDpoDatasetManifest>
            transform)
        => completed with
        {
            DatasetManifest = transform(completed.DatasetManifest),
            PipelineResult = completed.PipelineResult with
            {
                DatasetManifest = transform(completed.PipelineResult.DatasetManifest)
            }
        };

    private static DesignConversionDpoDatasetManifest ReplaceFirstManifestIdentityHash(
        DesignConversionDpoDatasetManifest manifest,
        bool train,
        Func<string, string> transform)
    {
        var groups = (train ? manifest.TrainSources : manifest.HeldOutSources)
            .ToArray();
        var hashes = groups[0].RecordIdentityHashes.ToArray();
        hashes[0] = transform(hashes[0]);
        groups[0] = groups[0] with { RecordIdentityHashes = hashes };
        return train
            ? manifest with { TrainSources = groups }
            : manifest with { HeldOutSources = groups };
    }

    private static DesignConversionDpoDatasetManifest DuplicateFirstManifestIdentityHash(
        DesignConversionDpoDatasetManifest manifest,
        bool train)
    {
        var groups = (train ? manifest.TrainSources : manifest.HeldOutSources)
            .ToArray();
        var hashes = groups[0].RecordIdentityHashes
            .Append(groups[0].RecordIdentityHashes[0])
            .ToArray();
        groups[0] = groups[0] with
        {
            RecordCount = hashes.Length,
            RecordIdentityHashes = hashes
        };
        return train
            ? manifest with { TrainSources = groups }
            : manifest with { HeldOutSources = groups };
    }

    private static DesignConversionDpoDatasetManifest ReplaceFirstManifestSourceIdentityHash(
        DesignConversionDpoDatasetManifest manifest,
        bool train,
        Func<string, string> transform)
    {
        var groups = (train ? manifest.TrainSources : manifest.HeldOutSources)
            .ToArray();
        groups[0] = groups[0] with
        {
            SourceIdentityHash = transform(groups[0].SourceIdentityHash)
        };
        return train
            ? manifest with { TrainSources = groups }
            : manifest with { HeldOutSources = groups };
    }

    private static DesignConversionTrainingRunResult ReplaceFirstBatch(
        DesignConversionTrainingRunResult completed,
        DesignConversionTrainingBatch replacement)
    {
        var batches = completed.PipelineResult.Batches.ToArray();
        batches[0] = replacement;
        return completed with
        {
            PipelineResult = completed.PipelineResult with { Batches = batches }
        };
    }

    private static string RecordIdentityHash(DesignConversionDpoTrainingRecord record)
        => Hash($"record|{record.RecordId}|{record.UnitId}");

    private static IReadOnlyDictionary<string, string> BuildAdmissionMetadata(
        string tenant,
        string idempotencyKey)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["training_intake_tenant_id_ref"] = tenant,
            ["training_intake_idempotency_key_ref"] = idempotencyKey
        };

    private static void AssertColdCycleLineageMetadata(
        IReadOnlyDictionary<string, string> metadata)
    {
        Assert.Equal("1", metadata["evaluated_dpo_cycle_ordinal"]);
        Assert.Equal("1", metadata["evaluated_dpo_cycle_current_measured_accuracy"]);
        Assert.Equal(bool.FalseString, metadata["evaluated_dpo_cycle_has_parent"]);
        Assert.Equal("cold_baseline", metadata["evaluated_dpo_cycle_comparison_status"]);
        Assert.False(metadata.ContainsKey("evaluated_dpo_cycle_parent_ordinal"));
        Assert.False(metadata.ContainsKey("evaluated_dpo_cycle_parent_measured_accuracy"));
        Assert.False(metadata.ContainsKey("evaluated_dpo_cycle_parent_policy_checkpoint_sha256"));
        Assert.False(metadata.ContainsKey("evaluated_dpo_cycle_parent_dataset_id"));
        Assert.False(metadata.ContainsKey("evaluated_dpo_cycle_parent_heldout_record_set_sha256"));
        Assert.False(metadata.ContainsKey("evaluated_dpo_cycle_parent_heldout_source_set_sha256"));
        Assert.False(metadata.ContainsKey("evaluated_dpo_cycle_parent_heldout_evaluation_input_sha256"));
        Assert.False(metadata.ContainsKey("evaluated_dpo_cycle_accuracy_delta"));
    }

    private static void AssertComparableContinuationCycleLineageMetadata(
        IReadOnlyDictionary<string, string> metadata,
        string expectedParentPolicySha256,
        string expectedParentDatasetId)
    {
        Assert.Equal("2", metadata["evaluated_dpo_cycle_ordinal"]);
        Assert.Equal("1", metadata["evaluated_dpo_cycle_current_measured_accuracy"]);
        Assert.Equal(bool.TrueString, metadata["evaluated_dpo_cycle_has_parent"]);
        Assert.Equal("comparable_continuation", metadata["evaluated_dpo_cycle_comparison_status"]);
        Assert.Equal("1", metadata["evaluated_dpo_cycle_parent_ordinal"]);
        Assert.Equal("1", metadata["evaluated_dpo_cycle_parent_measured_accuracy"]);
        Assert.Equal(
            expectedParentPolicySha256,
            metadata["evaluated_dpo_cycle_parent_policy_checkpoint_sha256"]);
        Assert.Equal(
            expectedParentDatasetId,
            metadata["evaluated_dpo_cycle_parent_dataset_id"]);
        Assert.False(string.IsNullOrWhiteSpace(
            metadata["evaluated_dpo_cycle_parent_heldout_record_set_sha256"]));
        Assert.False(string.IsNullOrWhiteSpace(
            metadata["evaluated_dpo_cycle_parent_heldout_source_set_sha256"]));
        Assert.False(string.IsNullOrWhiteSpace(
            metadata["evaluated_dpo_cycle_parent_heldout_evaluation_input_sha256"]));
        Assert.Equal("0", metadata["evaluated_dpo_cycle_accuracy_delta"]);
    }

    private static void AssertChangedHeldOutContinuationCycleLineageMetadata(
        IReadOnlyDictionary<string, string> metadata,
        string expectedParentPolicySha256,
        string expectedParentDatasetId)
    {
        Assert.Equal("2", metadata["evaluated_dpo_cycle_ordinal"]);
        Assert.Equal("1", metadata["evaluated_dpo_cycle_current_measured_accuracy"]);
        Assert.Equal(bool.TrueString, metadata["evaluated_dpo_cycle_has_parent"]);
        Assert.Equal(
            "changed_heldout_not_comparable",
            metadata["evaluated_dpo_cycle_comparison_status"]);
        Assert.Equal("1", metadata["evaluated_dpo_cycle_parent_ordinal"]);
        Assert.Equal("1", metadata["evaluated_dpo_cycle_parent_measured_accuracy"]);
        Assert.Equal(
            expectedParentPolicySha256,
            metadata["evaluated_dpo_cycle_parent_policy_checkpoint_sha256"]);
        Assert.Equal(
            expectedParentDatasetId,
            metadata["evaluated_dpo_cycle_parent_dataset_id"]);
        Assert.False(string.IsNullOrWhiteSpace(
            metadata["evaluated_dpo_cycle_parent_heldout_record_set_sha256"]));
        Assert.False(string.IsNullOrWhiteSpace(
            metadata["evaluated_dpo_cycle_parent_heldout_source_set_sha256"]));
        Assert.False(string.IsNullOrWhiteSpace(
            metadata["evaluated_dpo_cycle_parent_heldout_evaluation_input_sha256"]));
        Assert.False(metadata.ContainsKey("evaluated_dpo_cycle_accuracy_delta"));
    }

    private static Task<OperationResult<
        DesignConversionTrainingIntakeLocalPromotionResult>>
        RunLocalPromotionWithAdmissionAsync(
            DesignConversionDpoUsefulnessApi api,
            string intakeId,
            string outputDirectory,
            DesignConversionTrainingPipelineOptions options,
            IReadOnlyDictionary<string, string> admission,
            CancellationToken ct = default)
        => DesignConversionTrainingIntakeRouteAdmissionContext.RunAsync(
            admission,
            () => api.RunTrainingIntakeAndPromoteLocalAsync(
                intakeId,
                outputDirectory,
                options,
                ct));

    private static DesignConversionTrainingPipelineOptions BuildLocalPromotionOptions(
        string runId)
        => new(
            RunId: runId,
            BatchSize: 1,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: runId,
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1,
                Preflight: DesignConversionDpoDatasetPreflight
                    .LocalTrainingIntakeSourceManifestDisjointProof(
                        [$"{runId}:source-disjoint-proof"])),
            MeasurementGateRequest: new DesignConversionMeasurementGateRequest(
                GateId: $"{runId}:gate",
                UnitId: DesignConversionDpoUnits.CriticUnit,
                HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                    HeldOutRecordCount: 1,
                    HeldOutQualityScore: 0.95,
                    RuntimeMs: 100,
                    CostUsd: 0.01m,
                    PeakRamMb: 64,
                    EvidenceRefs: [$"ref:{runId}:heldout"]),
                IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                    SourceDisjoint: true,
                    LeakageDetected: false,
                    HiddenDeterminismDetected: false,
                    AblationPassed: true,
                    EvidenceRefs: [$"ref:{runId}:integrity"]),
                Thresholds: new DesignConversionMeasurementGateThresholds(
                    RequiredHeldOutQualityScore: 0.90,
                    MaxRuntimeMs: 500,
                    MaxCostUsd: 1.00m,
                    MaxPeakRamMb: 512)),
            DpoPolicyBridgeOptions: new DesignConversionDpoPolicyCheckpointBridgeOptions(
                MaxBridgePreferenceCount: 2,
                DpoStepCount: 1));

    private static string BuildExpectedTrainingIntakeLocalPromotionOutputDirectory(
        string rootDirectory,
        string promotionOutputDirectory,
        string runId)
    {
        var requestedPaths =
            DesignConversionDpoPromotionArtifactContractCodec.ResolvePaths(
                rootDirectory,
                promotionOutputDirectory,
                runId);
        Assert.True(requestedPaths.IsSuccess, requestedPaths.ErrorMessage);
        var artifactOutputDirectory = Path.Combine(
            requestedPaths.Value!.OutputDirectory,
            $"dpo-{Hash(runId)[..12]}-dpo-policy-checkpoints");
        var canonicalPaths =
            DesignConversionDpoPromotionArtifactContractCodec.ResolvePaths(
                rootDirectory,
                artifactOutputDirectory,
                runId);
        Assert.True(canonicalPaths.IsSuccess, canonicalPaths.ErrorMessage);
        return canonicalPaths.Value!.OutputDirectory;
    }

    private sealed record CompletedRunnerPartitionFixture(
        DesignConversionTrainingRunResult CompletedRunnerResult,
        IReadOnlyList<DesignConversionDpoTrainingRecord> FinalResolvedRecords);

    private sealed record CanonicalBpeFixture(
        byte[] Bytes,
        DesignConversionDpoCanonicalBpeTokenizerArtifactDescriptor Descriptor);

    private sealed record CompletedRunnerPartitionMutation(
        DesignConversionTrainingRunResult? CompletedRunnerResult,
        IReadOnlyList<DesignConversionDpoTrainingRecord>? FinalResolvedRecords);

    private sealed class CanonicalParentFixture : IDisposable
    {
        private CanonicalParentFixture(
            string root,
            DesignConversionDpoPromotionArtifactPaths paths,
            DesignConversionDpoPromotionArtifactPublication publication,
            DesignConversionTrainingIntakeLocalPromotionResult journalRecord,
            byte[]? canonicalBpeTokenizerBytes)
        {
            Root = root;
            Paths = paths;
            Publication = publication;
            JournalRecord = journalRecord;
            Active = DesignConversionDpoPromotedParentContinuationTests.Active(journalRecord);
            CanonicalBpeTokenizerBytes = canonicalBpeTokenizerBytes ?? [];
        }

        public string Root { get; }
        public DesignConversionDpoPromotionArtifactPaths Paths { get; }
        public DesignConversionDpoPromotionArtifactPublication Publication { get; }
        public DesignConversionTrainingIntakeLocalPromotionResult JournalRecord { get; }
        public DesignConversionLocalPromotionActive Active { get; }
        public byte[] CanonicalBpeTokenizerBytes { get; }

        public IReadOnlyList<string> AllArtifactPaths =>
            Publication.CanonicalBpeTokenizer is null
                ?
                [
                    Paths.TokenizerArtifactPath,
                    Paths.SourceEncoderContractPath,
                    Paths.PolicyCheckpointContractPath,
                    Paths.RuntimeCheckpointPath,
                    Path.Combine(
                        Paths.OutputDirectory,
                        Publication.Anchors.PolicyCheckpointFileName)
                ]
                :
                [
                    Paths.TokenizerArtifactPath,
                    Paths.CanonicalBpeTokenizerPath!,
                    Paths.SourceEncoderContractPath,
                    Paths.PolicyCheckpointContractPath,
                    Paths.RuntimeCheckpointPath,
                    Path.Combine(
                        Paths.OutputDirectory,
                        Publication.Anchors.PolicyCheckpointFileName)
                ];

        public string PathFor(string artifact)
            => artifact switch
            {
                "tokenizer-artifact" => Paths.TokenizerArtifactPath,
                "canonical-bpe-tokenizer" => Paths.CanonicalBpeTokenizerPath!,
                "source-contract" => Paths.SourceEncoderContractPath,
                "policy-contract" => Paths.PolicyCheckpointContractPath,
                "runtime-checkpoint" => Paths.RuntimeCheckpointPath,
                "policy-checkpoint" => Path.Combine(
                    Paths.OutputDirectory,
                    Publication.Anchors.PolicyCheckpointFileName),
                _ => throw new ArgumentOutOfRangeException(nameof(artifact))
            };

        public static async Task<CanonicalParentFixture> CreateAsync(
            bool includeCanonicalBpe = false,
            bool evaluatedPublication = false)
        {
            var root = Path.Combine(
                Path.GetTempPath(),
                "design-conversion-gap-3b-canonical",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                const string runId = "canonical-parent-run";
                var output = Path.Combine(root, "parent-output");
                var pathResult = DesignConversionDpoPromotionArtifactContractCodec.ResolvePaths(
                    root,
                    output,
                    runId);
                Assert.True(pathResult.IsSuccess, pathResult.ErrorMessage);
                var paths = pathResult.Value!;

                var records = BuildCanonicalRecords();
                var config = DesignConversionDpoPolicyBaseTestProvider.CreateConfig();
                var canonicalBpeTokenizer = includeCanonicalBpe
                    ? CreateCanonicalBpeFixture(config)
                    : null;
                var baseCheckpointPath = Path.Combine(root, "base-policy.bin");
                new LlmModel(config).SaveCheckpoint(baseCheckpointPath);
                var baseBytes = await File.ReadAllBytesAsync(baseCheckpointPath);
                var source = new ByteArrayCheckpointSource(baseBytes);
                var canonicalBpeDescriptor = canonicalBpeTokenizer?.Descriptor;
                var providerResult = DesignConversionCheckpointBackedDpoPolicyBaseProvider
                    .CreateWithOptions(
                        source,
                        config,
                        "test-org/canonical-parent",
                        "revision-gap-3b",
                        Hash(baseBytes),
                        DesignConversionDpoPolicyBaseTestProvider
                            .CreateCurrentProviderOptions(),
                        canonicalBpeDescriptor,
                        canonicalBpeTokenizer is null
                            ? null
                            : new ByteArrayCanonicalBpeTokenizerSnapshotSource(
                                canonicalBpeTokenizer.Bytes));
                Assert.True(providerResult.IsSuccess, providerResult.ErrorMessage);

                var bridgeOptions = new DesignConversionDpoPolicyCheckpointBridgeOptions(
                    MaxBridgePreferenceCount: 2,
                    DpoStepCount: 1);
                DesignConversionDpoPolicyRuntimeCheckpointBridgeResult bridgeValue;
                DesignConversionDpoPromotionArtifactPublication publication;
                if (evaluatedPublication)
                {
                    var evaluated = await DesignConversionDpoPolicyCheckpointBridge
                        .ExecuteEvaluatedAndConvertAsync(
                            DesignConversionLegalTeacherCapabilityTestFactory
                                .CreateAdmitted(),
                            BuildEvaluatedPartitionForTests(
                                "canonical-parent-evaluated",
                                heldOutCount: 1),
                            output,
                            runId,
                            paths.RuntimeCheckpointPath,
                            providerResult.Value,
                            bridgeOptions,
                            ct: CancellationToken.None);
                    Assert.True(evaluated.IsSuccess, evaluated.ErrorMessage);
                    using var evaluatedLease = evaluated.Value!;
                    bridgeValue = WithCanonicalBpeSnapshot(
                        evaluatedLease.Result,
                        canonicalBpeTokenizer,
                        config);
                    var published = await DesignConversionDpoPromotionArtifactContractCodec
                        .PublishEvaluatedAsync(
                            paths,
                            bridgeValue,
                            evaluatedLease.Evaluation,
                            CancellationToken.None);
                    Assert.True(published.IsSuccess, published.ErrorMessage);
                    evaluatedLease.CommitPublishedArtifacts();
                    publication = published.Value!;
                }
                else
                {
                    var bridge = await DesignConversionDpoPolicyCheckpointBridge
                        .ExecuteAndConvertAsync(
                            DesignConversionLegalTeacherCapabilityTestFactory
                                .CreateAdmitted(),
                            records,
                            output,
                            runId,
                            paths.RuntimeCheckpointPath,
                            providerResult.Value,
                            bridgeOptions,
                            CancellationToken.None);
                    Assert.True(bridge.IsSuccess, bridge.ErrorMessage);
                    bridgeValue = WithCanonicalBpeSnapshot(
                        bridge.Value!,
                        canonicalBpeTokenizer,
                        config);

                    var published = await DesignConversionDpoPromotionArtifactContractCodec
                        .PublishAsync(
                            paths,
                            bridgeValue,
                            CancellationToken.None);
                    Assert.True(published.IsSuccess, published.ErrorMessage);
                    publication = published.Value!;
                }
                var journal = BuildJournalRecord(paths, bridgeValue, publication);
                return new CanonicalParentFixture(
                    root,
                    paths,
                    publication,
                    journal,
                    canonicalBpeTokenizer?.Bytes);
            }
            catch
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, recursive: true);
                throw;
            }
        }

        private static DesignConversionDpoPolicyRuntimeCheckpointBridgeResult
            WithCanonicalBpeSnapshot(
                DesignConversionDpoPolicyRuntimeCheckpointBridgeResult bridge,
                CanonicalBpeFixture? canonicalBpeTokenizer,
                ModelConfig config)
        {
            if (canonicalBpeTokenizer is null)
                return bridge;

            return bridge with
            {
                CanonicalBpeTokenizerSnapshot =
                    new DesignConversionDpoCanonicalBpeTokenizerSnapshotReference(
                        canonicalBpeTokenizer.Descriptor,
                        config,
                        new ByteArrayCanonicalBpeTokenizerSnapshotSource(
                            canonicalBpeTokenizer.Bytes))
            };
        }

        private static IReadOnlyList<DesignConversionDpoTrainingRecord> BuildCanonicalRecords()
        {
            var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords(
            [
                DesignConversionDpoGeneratorTests.CreateExample(
                    "canonical-parent-a",
                    DesignConversionDpoUnits.CriticUnit,
                    0.98),
                DesignConversionDpoGeneratorTests.CreateExample(
                    "canonical-parent-b",
                    DesignConversionDpoUnits.ImproverUnit,
                    0.97)
            ]);
            Assert.True(build.IsSuccess, build.ErrorMessage);
            Assert.Empty(build.Value!.QuarantinedRecords);
            return build.Value.AcceptedRecords;
        }

        private static CanonicalBpeFixture CreateCanonicalBpeFixture(
            ModelConfig config)
        {
            var tokenizer = new BpeTokenizer();
            tokenizer.Train(
                string.Concat(Enumerable.Repeat(
                    "parent canonical bpe alpha beta gamma delta ",
                    32)),
                vocabSize: 258);
            Assert.Equal(258, tokenizer.VocabSize);

            using var stream = new MemoryStream();
            tokenizer.Save(stream);
            var bytes = stream.ToArray();
            var descriptor = DesignConversionDpoPromotionArtifactContractCodec
                .BuildCanonicalBpeTokenizerArtifactDescriptor(bytes, config);
            Assert.True(descriptor.IsSuccess, descriptor.ErrorMessage);
            return new CanonicalBpeFixture(bytes, descriptor.Value!);
        }

        private static DesignConversionTrainingIntakeLocalPromotionResult BuildJournalRecord(
            DesignConversionDpoPromotionArtifactPaths paths,
            DesignConversionDpoPolicyRuntimeCheckpointBridgeResult bridge,
            DesignConversionDpoPromotionArtifactPublication artifacts)
        {
            var checkpointSha = artifacts.PolicyCheckpointContract.Binding.RuntimeCheckpointSha256;
            var modelId = "canonical-parent-model";
            var activeRef = ActiveRef("canonical-parent");
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["schema_version"] = JsonlDesignConversionLocalArtifactDb
                    .TrainingIntakeLocalPromotionSchemaVersion,
                ["local_promotion_record_kind"] = "promotion",
                ["local_promotion_ready"] = bool.TrueString,
                ["local_promotion_active_local_model_ref"] = activeRef,
                ["local_promotion_active_model_id"] = modelId,
                ["local_promotion_active_checkpoint_sha256"] = checkpointSha,
                ["local_promotion_prior_state_recorded"] = bool.TrueString,
                ["local_promotion_prior_state_kind"] = "none",
                ["local_promotion_prior_active_local_model_ref"] = string.Empty,
                ["local_promotion_prior_active_model_id"] = string.Empty,
                ["local_promotion_prior_active_checkpoint_sha256"] = string.Empty
            };
            metadata = new Dictionary<string, string>(
                DesignConversionDpoPromotionArtifactContractCodec.AddTrustedAnchorMetadata(
                    metadata,
                    artifacts.Anchors),
                StringComparer.Ordinal);
            var manifest = new DesignConversionTrainingManifest(
                "canonical-parent-manifest",
                DesignConversionMachine.MachineId,
                DesignConversionMachine.DomainTag,
                "1",
                paths.RuntimeCheckpointFileName,
                "manifest/canonical-parent",
                DateTimeOffset.UnixEpoch,
                bridge.SourceRecordCount,
                0,
                ["critic", "improver"],
                [],
                [],
                false,
                false,
                false)
            {
                PromotionUploadRef = activeRef,
                LocalPromotionReceiptWritten = true,
                PromotionReceiptKind = "local_receipt_only",
                PromotionArtifacts = artifacts.ManifestArtifacts
            };
            var publication = new DesignConversionPromotionPublicationResult(
                new DesignConversionPromotionDecision(
                    DesignConversionPromotionDecisionKind.Promote,
                    [],
                    []),
                manifest,
                null)
            {
                PublicationKind = DesignConversionPromotionPublicationKind.LocalOnlyNoRemoteAttempt,
                PublicationStatus = DesignConversionPromotionPublicationStatus.NotUploaded
            };
            var model = new DesignConversionPromotedLocalModel(
                modelId,
                manifest.ManifestId,
                manifest.ManifestRef,
                paths.RuntimeCheckpointPath,
                checkpointSha,
                DesignConversionMachine.MachineId,
                DesignConversionMachine.DomainTag,
                "1",
                bridge.SourceRecordCount,
                bridge.BridgePreferenceCount,
                true,
                true,
                true,
                true,
                bridge.EvidenceRefs);
            var run = new DesignConversionTrainingIntakeRunResult(
                paths.SafeRunId,
                "canonical-parent-intake",
                "canonical-parent-preflight",
                bridge.SourceRecordCount,
                bridge.BridgePreferenceCount,
                true,
                "proven",
                "proven",
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                "test",
                "completed",
                string.Empty,
                "created",
                "mutated",
                "real_weights_proven",
                DesignConversionMeasurementGateStatus.Passed,
                "allowed",
                "ready",
                [],
                null,
                [],
                [],
                metadata);
            return new DesignConversionTrainingIntakeLocalPromotionResult(
                run,
                true,
                false,
                paths.OutputDirectory,
                paths.RuntimeCheckpointPath,
                publication,
                model,
                [],
                [],
                [],
                metadata);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }

    private sealed class ByteArrayCheckpointSource(byte[] bytes)
        : IDesignConversionDpoPolicyCheckpointSource
    {
        public Task<OperationResult<Stream>> OpenCheckpointSnapshotAsync(
            CancellationToken cancellationToken)
            => cancellationToken.IsCancellationRequested
                ? Task.FromResult(OperationResult<Stream>.Fail("cancelled", "CANCELLED"))
                : Task.FromResult(OperationResult<Stream>.Ok(
                    new MemoryStream(bytes, writable: false)));
    }

    private sealed class ByteArrayCanonicalBpeTokenizerSnapshotSource(byte[] bytes)
        : IDesignConversionDpoCanonicalBpeTokenizerSnapshotSource
    {
        public Task<OperationResult<Stream>> OpenCanonicalBpeTokenizerSnapshotAsync(
            CancellationToken cancellationToken)
            => cancellationToken.IsCancellationRequested
                ? Task.FromResult(OperationResult<Stream>.Fail("cancelled", "CANCELLED"))
                : Task.FromResult(OperationResult<Stream>.Ok(
                    new MemoryStream(bytes, writable: false)));
    }

    private sealed class CountingByteArrayCheckpointSource(byte[] bytes)
        : IDesignConversionDpoPolicyCheckpointSource
    {
        public int OpenCount { get; private set; }
        public TrackingMemoryStream? LastStream { get; private set; }

        public Task<OperationResult<Stream>> OpenCheckpointSnapshotAsync(
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromResult(OperationResult<Stream>.Fail("cancelled", "CANCELLED"));
            OpenCount++;
            LastStream = new TrackingMemoryStream(bytes);
            return Task.FromResult(OperationResult<Stream>.Ok(LastStream));
        }
    }

    private sealed class RecordingContinuationProvider(
        DesignConversionCheckpointBackedDpoPolicyBaseProvider inner)
        : IDesignConversionDpoPolicyContinuationProvider
    {
        public int ColdInvocationCount { get; private set; }
        public int ContinuationInvocationCount { get; private set; }

        public OperationResult<DesignConversionDpoPolicyBasePreparationDescriptor>
            DescribePreparation()
            => inner.DescribePreparation();

        public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
            CreateModelSetAsync(CancellationToken cancellationToken)
        {
            ColdInvocationCount++;
            return inner.CreateModelSetAsync(cancellationToken);
        }

        public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
            CreateModelSetAsync(
                DesignConversionDpoPolicyBasePreparationDescriptor preparation,
                CancellationToken cancellationToken)
        {
            ColdInvocationCount++;
            return inner.CreateModelSetAsync(preparation, cancellationToken);
        }

        public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
            CreateContinuedModelSetAsync(
                DesignConversionDpoPolicyContinuationRequest continuationRequest,
                CancellationToken cancellationToken)
        {
            ContinuationInvocationCount++;
            return inner.CreateContinuedModelSetAsync(
                continuationRequest,
                cancellationToken);
        }

        public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
            CreateContinuedModelSetAsync(
                DesignConversionDpoPolicyBasePreparationDescriptor preparation,
                DesignConversionDpoPolicyContinuationRequest continuationRequest,
                CancellationToken cancellationToken)
        {
            ContinuationInvocationCount++;
            return inner.CreateContinuedModelSetAsync(
                preparation,
                continuationRequest,
                cancellationToken);
        }

        // Отдельный счётчик сверочных обращений: прежние счётчики не трогаем.
        public int VerificationInvocationCount { get; private set; }

        // Сверочный набор не строим сами: передаём внутреннему доверенному
        // поставщику тем же способом, каким передаём продолжение.
        public Task<OperationResult<DesignConversionDpoPolicyBaseModelSet>>
            CreateContinuationVerificationModelSetAsync(
                ModelConfig config,
                DesignConversionDpoPolicyRandomContext randomContext,
                CancellationToken cancellationToken)
        {
            VerificationInvocationCount++;
            return inner.CreateContinuationVerificationModelSetAsync(
                config,
                randomContext,
                cancellationToken);
        }
    }

    private sealed class ParentFixture : IDisposable
    {
        public ParentFixture()
        {
            Root = Path.Combine(
                Path.GetTempPath(),
                "design-conversion-gap-3b-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public DesignConversionTrainingIntakeLocalPromotionResult Ready(
            string scope,
            DesignConversionTrainingIntakeLocalPromotionResult? prior)
        {
            var runId = $"{scope}-run";
            var output = Path.Combine(Root, scope);
            var runtimePath = Path.Combine(output, $"{runId}-promoted-checkpoint.json");
            var tokenizerPath = Path.Combine(
                output,
                runId + DesignConversionDpoPromotionArtifactContractCodec
                    .TokenizerArtifactSuffix);
            var sourcePath = Path.Combine(
                output,
                runId + DesignConversionDpoPromotionArtifactContractCodec
                    .SourceEncoderContractSuffix);
            var policyContractPath = Path.Combine(
                output,
                runId + DesignConversionDpoPromotionArtifactContractCodec
                    .PolicyCheckpointContractSuffix);
            var policyLeaf = DpoTrainer.BuildPolicyCheckpointFileName(1);
            var checkpointSha = Hash($"{scope}:runtime");
            var modelId = $"{scope}-model";
            var activeRef = ActiveRef(scope);
            var priorRef = prior?.BoundaryMetadata[
                "local_promotion_active_local_model_ref"] ?? string.Empty;
            var priorModelId = prior?.PromotedLocalModel?.ModelId ?? string.Empty;
            var priorCheckpoint = prior?.PromotedLocalModel?.CheckpointSha256 ?? string.Empty;
            var anchors = new DesignConversionDpoPromotionArtifactAnchors(
                "dpo-promotion-set-" + Hash($"{scope}:set"),
                Path.GetFileName(tokenizerPath),
                Hash($"{scope}:tokenizer-artifact"),
                80,
                Hash($"{scope}:source-contract"),
                100,
                Hash($"{scope}:policy-contract"),
                120,
                policyLeaf);
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["schema_version"] = JsonlDesignConversionLocalArtifactDb
                    .TrainingIntakeLocalPromotionSchemaVersion,
                ["local_promotion_record_kind"] = "promotion",
                ["local_promotion_ready"] = bool.TrueString,
                ["local_promotion_active_local_model_ref"] = activeRef,
                ["local_promotion_active_model_id"] = modelId,
                ["local_promotion_active_checkpoint_sha256"] = checkpointSha,
                ["local_promotion_prior_state_recorded"] = bool.TrueString,
                ["local_promotion_prior_state_kind"] =
                    prior is null ? "none" : "local_promotion_ref",
                ["local_promotion_prior_active_local_model_ref"] = priorRef,
                ["local_promotion_prior_active_model_id"] = priorModelId,
                ["local_promotion_prior_active_checkpoint_sha256"] = priorCheckpoint
            };
            metadata = new Dictionary<string, string>(
                DesignConversionDpoPromotionArtifactContractCodec
                    .AddTrustedAnchorMetadata(metadata, anchors),
                StringComparer.Ordinal);
            var manifest = new DesignConversionTrainingManifest(
                $"{scope}-manifest",
                DesignConversionMachine.MachineId,
                DesignConversionMachine.DomainTag,
                "1",
                Path.GetFileName(runtimePath),
                $"manifest/{scope}",
                DateTimeOffset.UnixEpoch,
                1,
                0,
                ["critic"],
                [],
                [],
                false,
                false,
                false)
            {
                PromotionUploadRef = activeRef,
                LocalPromotionReceiptWritten = true,
                PromotionReceiptKind = "local_receipt_only",
                PromotionArtifacts = new DesignConversionTrainingManifestArtifacts(
                    runtimePath,
                    tokenizerPath,
                    policyContractPath)
            };
            var publication = new DesignConversionPromotionPublicationResult(
                new DesignConversionPromotionDecision(
                    DesignConversionPromotionDecisionKind.Promote,
                    [],
                    []),
                manifest,
                null)
            {
                PublicationKind = DesignConversionPromotionPublicationKind
                    .LocalOnlyNoRemoteAttempt,
                PublicationStatus = DesignConversionPromotionPublicationStatus
                    .NotUploaded
            };
            var evidence = new[]
            {
                DesignConversionMachine.DpoTrainerPolicyCheckpointConvertedEvidence,
                DesignConversionMachine.DpoTrainerPolicyCheckpointFreshLoadedEvidence,
                $"dpo_policy_checkpoint_file:{policyLeaf}",
                "dpo_policy_checkpoint_sha256:" + Hash($"{scope}:policy")
            };
            var model = new DesignConversionPromotedLocalModel(
                modelId,
                manifest.ManifestId,
                manifest.ManifestRef,
                runtimePath,
                checkpointSha,
                DesignConversionMachine.MachineId,
                DesignConversionMachine.DomainTag,
                "1",
                1,
                1,
                true,
                true,
                true,
                true,
                evidence);
            var run = new DesignConversionTrainingIntakeRunResult(
                runId,
                $"{scope}-intake",
                $"{scope}-preflight",
                1,
                1,
                true,
                "proven",
                "proven",
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                "test",
                "completed",
                string.Empty,
                "created",
                "mutated",
                "real_weights_proven",
                DesignConversionMeasurementGateStatus.Passed,
                "allowed",
                "ready",
                [],
                null,
                [],
                [],
                metadata);
            return new DesignConversionTrainingIntakeLocalPromotionResult(
                run,
                true,
                false,
                output,
                runtimePath,
                publication,
                model,
                [],
                [],
                [],
                metadata);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }

    private sealed class TrackingMemoryStream(byte[] bytes) : MemoryStream(bytes)
    {
        private int _disposeCount;

        public int DisposeCount => Volatile.Read(ref _disposeCount);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Interlocked.Increment(ref _disposeCount);
            base.Dispose(disposing);
        }
    }

    private sealed class NeverOpenedCheckpointSource
        : IDesignConversionDpoPolicyCheckpointSource
    {
        public Task<OperationResult<Stream>> OpenCheckpointSnapshotAsync(
            CancellationToken cancellationToken)
            => throw new InvalidOperationException("The cold source must not be opened.");
    }

    private sealed class TrackingDisposable(Exception? exception) : IDisposable
    {
        private int _disposeCount;

        public int DisposeCount => Volatile.Read(ref _disposeCount);

        public void Dispose()
        {
            Interlocked.Increment(ref _disposeCount);
            if (exception is not null)
                throw exception;
        }
    }
}
