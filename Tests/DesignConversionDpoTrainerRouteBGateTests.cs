// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Globalization;
using DesignConversionDomain;
using DPO;

public sealed class DesignConversionDpoTrainerRouteBGateTests
{
    [Fact]
    public void RouteBGatePublishesCanonicalTrainerRouteContract()
    {
        Assert.Equal("REAL_TRAINER_ROUTE_B_GATE", DesignConversionDpoTrainerRouteBGate.GateId);
        Assert.Equal("11.DPO:DpoTrainer", DesignConversionDpoTrainerRouteBGate.TrainerSourceSystem);
        Assert.Equal("11.DPO/DpoTrainer", DesignConversionDpoTrainerRouteBGate.DerivationSourceSystem);
        Assert.Equal("11.DPO:DpoTrainer.StepVector", DesignConversionDpoTrainerRouteBGate.StepVectorIngredient);
        Assert.Equal(
            "11.DPO:DpoTrainer.StepVector:fresh_loaded",
            DesignConversionDpoTrainerRouteBGate.FreshLoadedSourceMechanism);
        Assert.Equal(
            "11.DPO:DpoTrainer.policy_checkpoint",
            DesignConversionDpoTrainerRouteBGate.PolicyCheckpointArtifactKind);

        var ingredients = DesignConversionDpoTrainerRouteBGate.RuntimeIngredientIds();
        Assert.Contains(DesignConversionDpoTrainerRouteBGate.TransformerModelIngredient, ingredients);
        Assert.Contains(DesignConversionDpoTrainerRouteBGate.LoraApplicatorIngredient, ingredients);
        Assert.Contains(DesignConversionDpoTrainerRouteBGate.TrainerSourceSystem, ingredients);
        Assert.Contains(DesignConversionDpoTrainerRouteBGate.StepVectorIngredient, ingredients);
        Assert.True(DesignConversionDpoTrainerRouteBGate.IsStepVectorIngredient(
            DesignConversionDpoTrainerRouteBGate.StepVectorIngredient));
        Assert.False(DesignConversionDpoTrainerRouteBGate.IsStepVectorIngredient(
            DesignConversionDpoTrainerRouteBGate.TrainerSourceSystem));
    }

    [Fact]
    public void FreshLoadedDerivationAppendIsIdempotentAndCodeOwned()
    {
        var appended = DesignConversionDpoTrainerRouteBGate
            .AppendFreshLoadedPolicyCheckpointDerivation("runtime");

        Assert.Equal(
            "runtime;source=11.DPO/DpoTrainer;policy_checkpoint=fresh_loaded_converted",
            appended);
        Assert.Equal(
            appended,
            DesignConversionDpoTrainerRouteBGate.AppendFreshLoadedPolicyCheckpointDerivation(appended));
        Assert.True(DesignConversionDpoTrainerRouteBGate.HasTrainerSourceDerivation(appended));
    }

    [Fact]
    public void TrainingHandoffValidationAcceptsCanonicalCompletedOutput()
    {
        const int completedStepCount = 3;
        var result = DesignConversionDpoTrainerRouteBGate.ValidateTrainingHandoff(
            completedStepCount,
            [
                DesignConversionDpoTrainerRouteBGate.TrainerSourceSystem,
                DesignConversionDpoTrainerRouteBGate.StepVectorIngredient
            ],
            [Path.Combine("checkpoints", DpoTrainer.BuildPolicyCheckpointFileName(completedStepCount))]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value);
    }

    [Theory]
    [InlineData("zero_step")]
    [InlineData("null_ingredients")]
    [InlineData("missing_trainer")]
    [InlineData("missing_step_vector")]
    [InlineData("null_checkpoints")]
    [InlineData("wrong_checkpoint_filename")]
    public void TrainingHandoffValidationRejectsRouteMutations(string mutation)
    {
        const int completedStepCount = 3;
        var actualStepCount = completedStepCount;
        IReadOnlyList<string>? ingredients =
        [
            DesignConversionDpoTrainerRouteBGate.TrainerSourceSystem,
            DesignConversionDpoTrainerRouteBGate.StepVectorIngredient
        ];
        IReadOnlyList<string>? checkpointPaths =
        [
            Path.Combine("checkpoints", DpoTrainer.BuildPolicyCheckpointFileName(completedStepCount))
        ];

        switch (mutation)
        {
            case "zero_step":
                actualStepCount = 0;
                break;
            case "null_ingredients":
                ingredients = null;
                break;
            case "missing_trainer":
                ingredients = [DesignConversionDpoTrainerRouteBGate.StepVectorIngredient];
                break;
            case "missing_step_vector":
                ingredients = [DesignConversionDpoTrainerRouteBGate.TrainerSourceSystem];
                break;
            case "null_checkpoints":
                checkpointPaths = null;
                break;
            case "wrong_checkpoint_filename":
                checkpointPaths =
                [
                    Path.Combine("checkpoints", DpoTrainer.BuildPolicyCheckpointFileName(completedStepCount - 1))
                ];
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null);
        }

        var result = DesignConversionDpoTrainerRouteBGate.ValidateTrainingHandoff(
            actualStepCount,
            ingredients,
            checkpointPaths);

        AssertRouteBGateFailure(result);
    }

    [Fact]
    public void TypedRuntimeCheckpointValidationAcceptsCanonicalLoadedRoute()
    {
        const int completedStepCount = 3;
        var checkpointRef = BuildValidCheckpointRef(completedStepCount);
        var result = DesignConversionDpoTrainerRouteBGate.ValidateTypedRuntimeCheckpoint(
            checkpointRef,
            completedStepCount,
            DesignConversionDpoTrainerRouteBGate.RuntimeIngredientIds(),
            callerFreshLoadVerified: true);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value);
        var expectedRandomContext = DesignConversionDpoPolicyBaseTestProvider
            .CreateCurrentRandomContext();
        Assert.Equal(4, checkpointRef.TrainingContext.SchemaVersion);
        Assert.Equal(expectedRandomContext, checkpointRef.TrainingContext.RandomContext);
        Assert.True(DesignConversionDpoPolicyRandomContextContract.HasRequiredEvidenceRefs(
            expectedRandomContext,
            checkpointRef.EvidenceRefs));
        Assert.Contains(
            $"{DesignConversionDpoPolicyRandomContextContract.RootInitializationSeedEvidencePrefix}" +
            expectedRandomContext.RootInitializationSeed.ToString(CultureInfo.InvariantCulture),
            checkpointRef.EvidenceRefs);
    }

    [Theory]
    [InlineData("null_ref")]
    [InlineData("wrong_artifact")]
    [InlineData("wrong_source")]
    [InlineData("wrong_mechanism")]
    [InlineData("fresh_load_false")]
    [InlineData("converted_false")]
    [InlineData("caller_fresh_load_false")]
    [InlineData("step_mismatch")]
    [InlineData("wrong_checkpoint_filename")]
    [InlineData("invalid_sha")]
    [InlineData("wrong_runtime_bridge")]
    [InlineData("raw_payload_false")]
    [InlineData("absolute_path_true")]
    [InlineData("zero_payload_length")]
    [InlineData("legacy_schema")]
    [InlineData("stale_schema_3")]
    [InlineData("missing_random_context")]
    [InlineData("null_random_context")]
    [InlineData("invalid_random_context")]
    [InlineData("wrong_random_root")]
    [InlineData("wrong_random_fingerprint")]
    [InlineData("missing_random_evidence")]
    [InlineData("tampered_random_evidence")]
    [InlineData("conflicting_root_evidence")]
    [InlineData("conflicting_fingerprint_evidence")]
    [InlineData("conflicting_derivation_evidence")]
    [InlineData("missing_commitment")]
    [InlineData("evidence_added_after_commitment")]
    [InlineData("missing_trainer")]
    [InlineData("missing_step_vector")]
    public void TypedRuntimeCheckpointValidationRejectsRouteMutations(string mutation)
    {
        const int completedStepCount = 3;
        DesignConversionDpoPolicyCheckpointRef? checkpointRef = BuildValidCheckpointRef(completedStepCount);
        IReadOnlyList<string> ingredients = DesignConversionDpoTrainerRouteBGate.RuntimeIngredientIds();
        var callerFreshLoadVerified = true;

        checkpointRef = mutation switch
        {
            "null_ref" => null,
            "wrong_artifact" => checkpointRef with { ArtifactKind = "wrong-artifact" },
            "wrong_source" => checkpointRef with { SourceSystem = "wrong-source" },
            "wrong_mechanism" => checkpointRef with { SourceMechanism = "wrong-mechanism" },
            "fresh_load_false" => checkpointRef with { FreshLoadVerified = false },
            "converted_false" => checkpointRef with { ConvertedToRuntimeWeights = false },
            "step_mismatch" => checkpointRef with { DpoStepCount = completedStepCount - 1 },
            "wrong_checkpoint_filename" => checkpointRef with
            {
                PolicyCheckpointFileName = DpoTrainer.BuildPolicyCheckpointFileName(completedStepCount - 1)
            },
            "invalid_sha" => checkpointRef with { Sha256 = new string('A', 64) },
            "wrong_runtime_bridge" => checkpointRef with { RuntimeBridgeKind = "wrong" },
            "raw_payload_false" => checkpointRef with
            {
                TrainingContext = checkpointRef.TrainingContext with { RawPayloadPersisted = false }
            },
            "absolute_path_true" => checkpointRef with
            {
                TrainingContext = checkpointRef.TrainingContext with { AbsolutePathPersisted = true }
            },
            "zero_payload_length" => checkpointRef with { PolicyCheckpointByteLength = 0 },
            "legacy_schema" => checkpointRef with
            {
                TrainingContext = checkpointRef.TrainingContext with { SchemaVersion = 2 }
            },
            "stale_schema_3" => checkpointRef with
            {
                TrainingContext = checkpointRef.TrainingContext with { SchemaVersion = 3 }
            },
            "missing_random_context" or "null_random_context" => checkpointRef with
            {
                TrainingContext = checkpointRef.TrainingContext with { RandomContext = null }
            },
            "invalid_random_context" => checkpointRef with
            {
                TrainingContext = checkpointRef.TrainingContext with
                {
                    RandomContext = checkpointRef.TrainingContext.RandomContext! with
                    {
                        DerivationScheme = "invalid"
                    }
                }
            },
            "wrong_random_root" => checkpointRef with
            {
                TrainingContext = checkpointRef.TrainingContext with
                {
                    RandomContext = checkpointRef.TrainingContext.RandomContext! with
                    {
                        RootInitializationSeed = 1UL
                    }
                }
            },
            "wrong_random_fingerprint" => checkpointRef with
            {
                TrainingContext = checkpointRef.TrainingContext with
                {
                    RandomContext = checkpointRef.TrainingContext.RandomContext! with
                    {
                        TrainingConfigurationFingerprint = new string('f', 64)
                    }
                }
            },
            "missing_random_evidence" => WithCommittedEvidence(
                checkpointRef,
                checkpointRef.EvidenceRefs.Where(evidence => !string.Equals(
                    evidence,
                    DesignConversionDpoPolicyRandomContextContract.TypedEvidence,
                    StringComparison.Ordinal))),
            "tampered_random_evidence" => WithCommittedEvidence(
                checkpointRef,
                checkpointRef.EvidenceRefs.Select(evidence => string.Equals(
                        evidence,
                        DesignConversionDpoPolicyRandomContextContract
                            .InitializationReproducibilityOnlyEvidence,
                        StringComparison.Ordinal)
                    ? $"{evidence}:tampered"
                    : evidence)),
            "conflicting_root_evidence" => WithCommittedEvidence(
                checkpointRef,
                checkpointRef.EvidenceRefs.Append(
                    $"{DesignConversionDpoPolicyRandomContextContract.RootInitializationSeedEvidencePrefix}" +
                    checked(checkpointRef.TrainingContext.RandomContext!
                        .RootInitializationSeed + 1UL)
                        .ToString(CultureInfo.InvariantCulture))),
            "conflicting_fingerprint_evidence" => WithCommittedEvidence(
                checkpointRef,
                checkpointRef.EvidenceRefs.Append(
                    $"{DesignConversionDpoPolicyRandomContextContract.TrainingConfigurationFingerprintEvidencePrefix}" +
                    new string('f', 64))),
            "conflicting_derivation_evidence" => WithCommittedEvidence(
                checkpointRef,
                checkpointRef.EvidenceRefs.Append(
                    $"{DesignConversionDpoPolicyRandomContextContract.RandomDerivationSchemeEvidencePrefix}" +
                    "conflicting-derivation/v1")),
            "missing_commitment" => checkpointRef with
            {
                TrainingContext = checkpointRef.TrainingContext with
                {
                    LegalTeacherAdmissionCommitment = null
                }
            },
            "evidence_added_after_commitment" => checkpointRef with
            {
                EvidenceRefs = checkpointRef.EvidenceRefs
                    .Concat(["mutation:after-commitment"])
                    .ToArray()
            },
            _ => checkpointRef
        };
        if (mutation == "caller_fresh_load_false")
            callerFreshLoadVerified = false;
        if (mutation == "missing_trainer")
            ingredients = [DesignConversionDpoTrainerRouteBGate.StepVectorIngredient];
        if (mutation == "missing_step_vector")
            ingredients = [DesignConversionDpoTrainerRouteBGate.TrainerSourceSystem];
        if (mutation.StartsWith("conflicting_", StringComparison.Ordinal))
        {
            Assert.NotNull(checkpointRef);
            Assert.True(
                DesignConversionDpoLegalTeacherAdmissionCommitmentCodec.Validate(
                    checkpointRef!.TrainingContext
                        .LegalTeacherAdmissionCommitment));
            Assert.True(
                DesignConversionDpoLegalTeacherAdmissionCommitmentCodec
                    .MatchesCheckpointEvidence(
                        checkpointRef.TrainingContext
                            .LegalTeacherAdmissionCommitment,
                        checkpointRef.EvidenceRefs));
            Assert.False(
                DesignConversionDpoPolicyRandomContextContract
                    .HasRequiredEvidenceRefs(
                        checkpointRef.TrainingContext.RandomContext,
                        checkpointRef.EvidenceRefs));
        }

        var result = DesignConversionDpoTrainerRouteBGate.ValidateTypedRuntimeCheckpoint(
            checkpointRef,
            completedStepCount,
            ingredients,
            callerFreshLoadVerified);

        AssertRouteBGateFailure(result);
    }

    private static DesignConversionDpoPolicyCheckpointRef BuildValidCheckpointRef(int completedStepCount)
    {
        var randomContext = DesignConversionDpoPolicyBaseTestProvider
            .CreateCurrentRandomContext();
        var evidenceRefs = DesignConversionDpoPolicyRandomContextContract
            .BuildRequiredEvidenceRefs(randomContext);
        return new(
            ArtifactKind: DesignConversionDpoTrainerRouteBGate.PolicyCheckpointArtifactKind,
            PolicyCheckpointFileName: DpoTrainer.BuildPolicyCheckpointFileName(completedStepCount),
            Sha256: new string('a', 64),
            SourceSystem: DesignConversionDpoTrainerRouteBGate.TrainerSourceSystem,
            SourceMechanism: DesignConversionDpoTrainerRouteBGate.FreshLoadedSourceMechanism,
            RuntimeBridgeKind: DesignConversionMachine.DpoPolicyDirectInferenceRuntimeBridgeKind,
            FreshLoadVerified: true,
            ConvertedToRuntimeWeights: true,
            SourceRecordCount: 1,
            BridgePreferenceCount: 1,
            DpoStepCount: completedStepCount,
            TrainingContext: (new DesignConversionDpoPolicyCheckpointTrainingContext(
                ConfigKind: "test",
                VocabSize: 1,
                ContextLength: 1,
                HiddenDim: 1,
                NumLayers: 1,
                NumHeads: 1,
                NumKvHeads: 1,
                FfnHiddenDim: 1,
                RawPayloadPersisted: true,
                AbsolutePathPersisted: false,
                SchemaVersion: 4)) with
            {
                RandomContext = randomContext,
                LegalTeacherAdmissionCommitment =
                    DesignConversionDpoLegalTeacherAdmissionCommitmentTestFactory.Create(
                        evidenceRefs)
            },
            EvidenceRefs: evidenceRefs,
            PolicyCheckpointByteLength: 1);
    }

    private static DesignConversionDpoPolicyCheckpointRef WithCommittedEvidence(
        DesignConversionDpoPolicyCheckpointRef checkpointRef,
        IEnumerable<string> evidenceRefs)
    {
        var snapshot = evidenceRefs.ToArray();
        return checkpointRef with
        {
            TrainingContext = checkpointRef.TrainingContext with
            {
                LegalTeacherAdmissionCommitment =
                    DesignConversionDpoLegalTeacherAdmissionCommitmentTestFactory.Create(snapshot)
            },
            EvidenceRefs = snapshot
        };
    }

    private static void AssertRouteBGateFailure(AgentInfrastructure.OperationResult<bool> result)
    {
        Assert.False(result.IsSuccess);
        Assert.False(result.Value);
        Assert.Equal(DesignConversionDpoTrainerRouteBGate.FailureCode, result.ErrorCode);
        Assert.Equal(DesignConversionDpoTrainerRouteBGate.FailureMessage, result.ErrorMessage);
    }
}
