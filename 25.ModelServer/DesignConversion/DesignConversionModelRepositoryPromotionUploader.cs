// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;
using ModelServer.Contracts;

public sealed class DesignConversionModelRepositoryPromotionUploader :
    IDesignConversionBoundedPromotionUploader,
    IDesignConversionPromotionUploaderPreflight
{
    public int MaximumSerializedReceiptUtf8Bytes => 64 * 1024;

    public OperationResult<string> PreflightConfigurationFingerprintSha256()
    {
        var validated = ValidateServerBaseUrl(_serverBaseUrl);
        if (!validated.IsSuccess)
            return validated.MapError<string>();
        OperationResult<string>? repositoryIdentity;
        OperationResult<string>? locatorIdentity;
        try
        {
            repositoryIdentity =
                _repository.PreflightStableConfigurationIdentitySha256();
            locatorIdentity =
                _locator.PreflightStableConfigurationIdentitySha256();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return ConfigurationIdentityInvalid();
        }
        if (repositoryIdentity is null || locatorIdentity is null)
            return ConfigurationIdentityInvalid();
        if (!repositoryIdentity.IsSuccess)
            return repositoryIdentity;
        if (!locatorIdentity.IsSuccess)
            return locatorIdentity;
        if (!IsCanonicalSha256Ref(repositoryIdentity.Value) ||
            !IsCanonicalSha256Ref(locatorIdentity.Value))
        {
            return ConfigurationIdentityInvalid();
        }
        var material = string.Join("\n",
        [
            UploadContractVersion,
            validated.Value!,
            repositoryIdentity.Value!,
            locatorIdentity.Value!,
            MaximumSerializedReceiptUtf8Bytes.ToString(
                CultureInfo.InvariantCulture)
        ]);
        return OperationResult<string>.Ok(
            "sha256:" + Convert.ToHexString(SHA256.HashData(
                    Encoding.UTF8.GetBytes(material)))
                .ToLowerInvariant());
    }

    private const string UploadContractVersion = "design_conversion_model_repository_promotion_upload_v1";
    private const string UnitFamilyId = "design_conversion_learned_weight_json_checkpoint";
    private const string TaskType = "design_conversion_learned_weight_json_checkpoint_not_transformer_not_dpo_not_cloud_ready";
    private const string EvaluatedDpoUnitFamilyId = "design_conversion_evaluated_dpo_policy_checkpoint";
    private const string EvaluatedDpoTaskType =
        "design_conversion_evaluated_dpo_policy_checkpoint_transformer_dpo_not_cloud_ready";

    private static bool IsCanonicalSha256Ref(string? value)
        => value is { Length: 71 } &&
           value.StartsWith("sha256:", StringComparison.Ordinal) &&
           value.AsSpan(7).ToString().All(character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static OperationResult<string> ConfigurationIdentityInvalid()
        => OperationResult<string>.Fail(
            "DesignConversion repository promotion requires canonical non-secret repository and locator configuration identities.",
            "DESIGN_CONVERSION_REPOSITORY_UPLOAD_CONFIGURATION_IDENTITY_INVALID");

    private readonly IModelRepository _repository;
    private readonly IRemoteUnitLocator _locator;
    private readonly string _serverBaseUrl;
    private readonly Func<
        DesignConversionDpoEvaluatedArtifactUploadLease,
        DesignConversionMachineLoadResult,
        bool> _evaluatedDpoLearnedWeightAuthorityMatcher;
    private readonly DesignConversionPromotionGateEvaluator _gateEvaluator = new();

    public DesignConversionModelRepositoryPromotionUploader(
        IModelRepository repository,
        IRemoteUnitLocator locator,
        string serverBaseUrl)
        : this(
            repository,
            locator,
            serverBaseUrl,
            static (lease, load) => lease.MatchesLoadedCheckpoint(load))
    {
    }

    internal DesignConversionModelRepositoryPromotionUploader(
        IModelRepository repository,
        IRemoteUnitLocator locator,
        string serverBaseUrl,
        Func<
            DesignConversionDpoEvaluatedArtifactUploadLease,
            DesignConversionMachineLoadResult,
            bool> evaluatedDpoLearnedWeightAuthorityMatcher)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _locator = locator ?? throw new ArgumentNullException(nameof(locator));
        _evaluatedDpoLearnedWeightAuthorityMatcher =
            evaluatedDpoLearnedWeightAuthorityMatcher ??
            throw new ArgumentNullException(nameof(evaluatedDpoLearnedWeightAuthorityMatcher));
        var validatedServerBaseUrl = ValidateServerBaseUrl(
            serverBaseUrl ?? throw new ArgumentNullException(nameof(serverBaseUrl)));
        if (!validatedServerBaseUrl.IsSuccess)
            throw new ArgumentException(validatedServerBaseUrl.ErrorMessage, nameof(serverBaseUrl));

        _serverBaseUrl = validatedServerBaseUrl.Value!;
    }

    public async Task<OperationResult<DesignConversionPromotionUploadReceipt>> UploadAsync(
        DesignConversionTrainingManifest manifest,
        DesignConversionPromotionGateReport gateReport,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Fail("DesignConversion model repository promotion upload was cancelled.", "CANCELLED");

        var serverBaseUrl = ValidateServerBaseUrl(_serverBaseUrl);
        if (!serverBaseUrl.IsSuccess)
            return serverBaseUrl.MapError<DesignConversionPromotionUploadReceipt>();

        if (manifest is null)
        {
            return Fail(
                "DesignConversion model repository promotion manifest is required.",
                "DESIGN_CONVERSION_REPOSITORY_UPLOAD_MANIFEST_REQUIRED");
        }

        if (gateReport is null)
        {
            return Fail(
                "DesignConversion model repository promotion gate report is required.",
                "DESIGN_CONVERSION_REPOSITORY_UPLOAD_GATE_REPORT_REQUIRED");
        }

        var decision = _gateEvaluator.Decide(gateReport);
        if (!decision.AllowsUpload)
        {
            return Fail(
                "DesignConversion model repository promotion gate did not allow upload.",
                "DESIGN_CONVERSION_REPOSITORY_UPLOAD_GATE_REJECTED");
        }

        var artifacts = ValidateArtifacts(manifest.PromotionArtifacts);
        if (!artifacts.IsSuccess)
            return artifacts.MapError<DesignConversionPromotionUploadReceipt>();

        using var originalArtifactSet = artifacts.Value!;
        var artifactSet = originalArtifactSet;
        if (!artifactSet.Revalidate())
            return EvaluatedArtifactPublicationChanged();

        var loaded = await LoadAndValidateCheckpointAsync(
            manifest,
            artifactSet.CheckpointPath,
            validateManifestCheckpointMetadata: !artifactSet.IsEvaluatedDpoArtifactSet,
            ct);
        if (!artifactSet.Revalidate())
            return EvaluatedArtifactPublicationChanged();
        if (!loaded.IsSuccess)
            return loaded.MapError<DesignConversionPromotionUploadReceipt>();

        var loadResult = loaded.Value!.LoadResult;
        var checkpoint = loadResult.Checkpoint;
        var loadedExport = loaded.Value.Export;
        var checkpointSha256 = loadResult.CheckpointSha256;
        if (!artifactSet.IsEvaluatedDpoArtifactSet)
        {
            // Унаследованный DTO называет поле policy-checkpoint, но repository хранит SHA именно bytes загружаемого payload.
            // Для non-eval bytes payload - это свежезагруженный runtime checkpoint, поэтому оба унаследованных SHA совпадают.
            artifactSet = artifactSet with { PolicyCheckpointSha256 = checkpointSha256 };
        }

        var policyCheckpointSha256 = artifactSet.PolicyCheckpointSha256;
        var criticWeights = CountLearnedWeightArtifacts(checkpoint.CriticUnit);
        var improverWeights = CountLearnedWeightArtifacts(checkpoint.ImproverUnit);
        var criticObservations = CountLearnedWeightObservations(checkpoint.CriticUnit);
        var improverObservations = CountLearnedWeightObservations(checkpoint.ImproverUnit);
        var loadedWeightAuthority = loadResult.Machine.GetLoadedLearnedWeightAuthoritySummary();

        if (artifactSet.IsEvaluatedDpoArtifactSet)
        {
            if (artifactSet.EvaluatedLease is not { } evaluatedLease ||
                !_evaluatedDpoLearnedWeightAuthorityMatcher(evaluatedLease, loadResult))
            {
                if (!artifactSet.Revalidate())
                    return EvaluatedArtifactPublicationChanged();

                return Fail(
                    "DesignConversion evaluated DPO repository upload requires a validated sibling policy learned-weight authority and fresh-loaded runtime scorer.",
                    "DESIGN_CONVERSION_REPOSITORY_UPLOAD_EVALUATED_DPO_ARTIFACT_CONTRACT_INVALID");
            }
        }
        else if (!RuntimeUnitHasRealLearnedWeights(checkpoint.CriticUnit) ||
                 !RuntimeUnitHasRealLearnedWeights(checkpoint.ImproverUnit))
        {
            return Fail(
                "DesignConversion checkpoint must contain learned-weight artifacts for both runtime units.",
                "DESIGN_CONVERSION_REPOSITORY_UPLOAD_LEARNED_WEIGHTS_REQUIRED");
        }

        var sourceDatasetSha256 = BuildSourceDatasetSha256(manifest);
        var createdAtUtc = DateTimeOffset.UtcNow;
        var uploadUnits = new[]
        {
            BuildUploadUnit(
                checkpoint.CriticUnit,
                manifest,
                loadedExport,
                artifactSet,
                checkpointSha256,
                policyCheckpointSha256,
                sourceDatasetSha256,
                criticWeights,
                criticObservations,
                createdAtUtc),
            BuildUploadUnit(
                checkpoint.ImproverUnit,
                manifest,
                loadedExport,
                artifactSet,
                checkpointSha256,
                policyCheckpointSha256,
                sourceDatasetSha256,
                improverWeights,
                improverObservations,
                createdAtUtc)
        };

        var uploadedUnits = new List<UploadedUnit>(capacity: uploadUnits.Length);
        foreach (var unit in uploadUnits)
        {
            if (ct.IsCancellationRequested)
                return Fail("DesignConversion model repository promotion upload was cancelled.", "CANCELLED");
            if (!artifactSet.Revalidate())
                return EvaluatedArtifactPublicationChanged();

            var upload = await _repository.UploadAsync(unit.Manifest, serverBaseUrl.Value!, ct);
            if (!artifactSet.Revalidate())
                return EvaluatedArtifactPublicationChanged();
            if (!upload.IsSuccess)
            {
                return Fail(
                    "DesignConversion model repository upload failed before verification.",
                    PreserveSafeErrorCode(
                        upload.ErrorCode,
                        "DESIGN_CONVERSION_REPOSITORY_UPLOAD_FAILED"));
            }

            if (upload.Value is null)
            {
                return Fail(
                    "DesignConversion model repository upload returned no remote manifest.",
                    "DESIGN_CONVERSION_REPOSITORY_UPLOAD_REMOTE_MANIFEST_REQUIRED");
            }

            var uploaded = ValidateUploadedManifest(upload.Value, unit, serverBaseUrl.Value!);
            if (!uploaded.IsSuccess)
                return uploaded.MapError<DesignConversionPromotionUploadReceipt>();

            uploadedUnits.Add(new UploadedUnit(unit, upload.Value));
        }

        foreach (var uploaded in uploadedUnits)
        {
            if (ct.IsCancellationRequested)
                return Fail("DesignConversion model repository promotion verification was cancelled.", "CANCELLED");
            if (!artifactSet.Revalidate())
                return EvaluatedArtifactPublicationChanged();

            var loadedWeights = await _repository.LoadWeightsAsync(
                uploaded.Remote.UnitId,
                serverBaseUrl.Value!,
                artifactSet.PolicyCheckpointUploadPath,
                ct);
            if (!artifactSet.Revalidate())
                return EvaluatedArtifactPublicationChanged();
            if (!loadedWeights.IsSuccess)
            {
                return Fail(
                    "DesignConversion model repository fresh-load verification failed.",
                    PreserveSafeErrorCode(
                        loadedWeights.ErrorCode,
                        "DESIGN_CONVERSION_REPOSITORY_LOAD_FAILED"));
            }

            if (loadedWeights.Value is null)
            {
                return Fail(
                    "DesignConversion model repository fresh-load verification returned no stream.",
                    "DESIGN_CONVERSION_REPOSITORY_LOAD_STREAM_REQUIRED");
            }

            string loadedSha256;
            await using (loadedWeights.Value)
            {
                var sha = await ComputeSha256HexAsync(loadedWeights.Value, ct);
                if (!sha.IsSuccess)
                    return sha.MapError<DesignConversionPromotionUploadReceipt>();

                loadedSha256 = sha.Value!;
            }

            if (!artifactSet.Revalidate())
                return EvaluatedArtifactPublicationChanged();

            if (!string.Equals(
                    loadedSha256,
                    uploaded.Remote.ManifestSha256,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Fail(
                    "DesignConversion model repository fresh-load hash did not match uploaded manifest.",
                    "DESIGN_CONVERSION_REPOSITORY_LOAD_HASH_MISMATCH");
            }

            uploaded.FreshLoadSha256 = loadedSha256;
        }

        var registeredUnits = new List<RemoteUnitManifest>(capacity: uploadedUnits.Count);
        foreach (var uploaded in uploadedUnits)
        {
            if (ct.IsCancellationRequested)
                return Fail("DesignConversion model repository promotion registration was cancelled.", "CANCELLED");
            if (!artifactSet.Revalidate())
                return EvaluatedArtifactPublicationChanged();

            var register = await _locator.RegisterAsync(uploaded.Remote, ct);
            if (!artifactSet.Revalidate())
                return EvaluatedArtifactPublicationChanged();
            if (!register.IsSuccess)
            {
                return FailLocatorRegistration(
                    PreserveSafeErrorCode(
                        register.ErrorCode,
                        "DESIGN_CONVERSION_REPOSITORY_LOCATOR_REGISTER_FAILED"),
                    registeredUnits.Count);
            }

            if (register.Value != true)
            {
                return FailLocatorRegistration(
                    "DESIGN_CONVERSION_REPOSITORY_LOCATOR_REGISTER_NOT_ACCEPTED",
                    registeredUnits.Count);
            }

            registeredUnits.Add(uploaded.Remote);
        }

        foreach (var uploaded in uploadedUnits)
        {
            if (ct.IsCancellationRequested)
                return Fail("DesignConversion model repository promotion locate verification was cancelled.", "CANCELLED");
            if (!artifactSet.Revalidate())
                return EvaluatedArtifactPublicationChanged();

            var located = await _locator.LocateAsync(uploaded.Remote.UnitId, ct);
            if (!artifactSet.Revalidate())
                return EvaluatedArtifactPublicationChanged();
            if (!located.IsSuccess)
            {
                return Fail(
                    "DesignConversion remote locator verification failed.",
                    PreserveSafeErrorCode(
                        located.ErrorCode,
                        "DESIGN_CONVERSION_REPOSITORY_LOCATOR_LOOKUP_FAILED"));
            }

            if (located.Value is null)
            {
                return Fail(
                    "DesignConversion remote locator verification returned no manifest.",
                    "DESIGN_CONVERSION_REPOSITORY_LOCATOR_LOOKUP_MANIFEST_REQUIRED");
            }

            var identity = ValidateLocatedManifest(located.Value, uploaded.Remote);
            if (!identity.IsSuccess)
                return identity.MapError<DesignConversionPromotionUploadReceipt>();

            uploaded.Located = located.Value;
        }

        if (!artifactSet.Revalidate())
            return EvaluatedArtifactPublicationChanged();

        var primary = uploadedUnits[0];
        var remoteRef = BuildRemoteRef(primary.Located!, serverBaseUrl.Value!);
        if (!remoteRef.IsSuccess)
            return remoteRef.MapError<DesignConversionPromotionUploadReceipt>();

        var uploadId = BuildSafeToken(
            "repo-upload",
            UploadContractVersion,
            manifest.ManifestId,
            checkpointSha256,
            string.Join("|", uploadedUnits.Select(unit => unit.Remote.ManifestSha256)));
        var provenanceRef = BuildSafeToken(
            "repo-prov",
            serverBaseUrl.Value!,
            remoteRef.Value!,
            manifest.ManifestRef,
            sourceDatasetSha256);

        var receipt = new DesignConversionPromotionUploadReceipt(
                UploadId: uploadId,
                RemoteRef: remoteRef.Value!,
                UploadedAtUtc: DateTimeOffset.UtcNow,
                EvidenceRefs: BuildReceiptEvidence(
                    uploadedUnits,
                    artifactSet,
                    checkpointSha256,
                    policyCheckpointSha256,
                    sourceDatasetSha256,
                    criticWeights,
                    improverWeights,
                    criticObservations,
                    improverObservations,
                    loadedWeightAuthority))
            {
                PublicationKind = DesignConversionPromotionPublicationKind.VerifiedModelServerUpload,
                UploadAttempted = true,
                UploadSucceeded = true,
                VerificationSucceeded = true,
                PublicationStatus = DesignConversionPromotionPublicationStatus.UploadedVerified,
                ProvenanceRef = provenanceRef
            };
        if (!artifactSet.Revalidate())
            return EvaluatedArtifactPublicationChanged();

        return OperationResult<DesignConversionPromotionUploadReceipt>.Ok(receipt);
    }

    private static OperationResult<ValidatedPromotionArtifacts> ValidateArtifacts(
        DesignConversionTrainingManifestArtifacts? artifacts)
    {
        if (artifacts is null)
        {
            return OperationResult<ValidatedPromotionArtifacts>.Fail(
                "DesignConversion model repository promotion artifacts are required.",
                "DESIGN_CONVERSION_REPOSITORY_UPLOAD_ARTIFACTS_REQUIRED");
        }

        if (!string.IsNullOrWhiteSpace(artifacts.SourceEncoderPath))
        {
            var acquired = DesignConversionDpoPromotionArtifactContractCodec
                .AcquireValidatedEvaluatedArtifactUpload(artifacts);
            if (!acquired.IsSuccess || acquired.Value is null)
            {
                return OperationResult<ValidatedPromotionArtifacts>.Fail(
                    "DesignConversion model repository evaluated DPO artifact publication is invalid.",
                    "DESIGN_CONVERSION_REPOSITORY_UPLOAD_EVALUATED_DPO_ARTIFACT_CONTRACT_INVALID");
            }

            var lease = acquired.Value;
            return OperationResult<ValidatedPromotionArtifacts>.Ok(
                new ValidatedPromotionArtifacts(
                    lease.RuntimeCheckpointPath,
                    lease.TokenizerPath,
                    Path.GetFullPath(artifacts.PolicyPath),
                    lease.PolicyCheckpointPath,
                    lease.PolicyCheckpointSha256,
                    IsEvaluatedDpoArtifactSet: true,
                    PolicyTrainingContext: lease.TrainingContext,
                    SourceRecordCount: lease.SourceRecordCount,
                    BridgePreferenceCount: lease.BridgePreferenceCount,
                    DpoStepCount: lease.DpoStepCount,
                    EvaluatedLease: lease));
        }

        var checkpoint = ValidateExistingFile(artifacts.CheckpointPath, "CHECKPOINT");
        if (!checkpoint.IsSuccess)
            return checkpoint.MapError<ValidatedPromotionArtifacts>();

        var tokenizer = ValidateExistingFile(artifacts.TokenizerPath, "TOKENIZER");
        if (!tokenizer.IsSuccess)
            return tokenizer.MapError<ValidatedPromotionArtifacts>();

        var policy = ValidateExistingFile(artifacts.PolicyPath, "POLICY");
        if (!policy.IsSuccess)
            return policy.MapError<ValidatedPromotionArtifacts>();

        return OperationResult<ValidatedPromotionArtifacts>.Ok(new ValidatedPromotionArtifacts(
            checkpoint.Value!,
            tokenizer.Value!,
            policy.Value!,
            PolicyCheckpointUploadPath: checkpoint.Value!,
            PolicyCheckpointSha256: string.Empty,
            IsEvaluatedDpoArtifactSet: false,
            PolicyTrainingContext: null,
            SourceRecordCount: 0,
            BridgePreferenceCount: 0,
            DpoStepCount: 0,
            EvaluatedLease: null));
    }

    private static OperationResult<string> ValidateExistingFile(string? path, string artifactRole)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return OperationResult<string>.Fail(
                "DesignConversion model repository promotion artifact path is required.",
                $"DESIGN_CONVERSION_REPOSITORY_UPLOAD_{artifactRole}_ARTIFACT_PATH_REQUIRED");
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(path);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return OperationResult<string>.Fail(
                "DesignConversion model repository promotion artifact path is invalid.",
                $"DESIGN_CONVERSION_REPOSITORY_UPLOAD_{artifactRole}_ARTIFACT_PATH_INVALID");
        }

        if (!File.Exists(fullPath))
        {
            return OperationResult<string>.Fail(
                "DesignConversion model repository promotion artifact file is missing.",
                $"DESIGN_CONVERSION_REPOSITORY_UPLOAD_{artifactRole}_ARTIFACT_MISSING");
        }

        return OperationResult<string>.Ok(fullPath);
    }

    private static async Task<OperationResult<LoadedCheckpoint>> LoadAndValidateCheckpointAsync(
        DesignConversionTrainingManifest manifest,
        string checkpointPath,
        bool validateManifestCheckpointMetadata,
        CancellationToken ct)
    {
        var loaded = await DesignConversionMachine.LoadCheckpointAsync(checkpointPath, ct);
        if (!loaded.IsSuccess)
        {
            return OperationResult<LoadedCheckpoint>.Fail(
                "DesignConversion learned-weight checkpoint could not be fresh-loaded.",
                PreserveSafeErrorCode(
                    loaded.ErrorCode,
                    "DESIGN_CONVERSION_REPOSITORY_UPLOAD_CHECKPOINT_LOAD_FAILED"));
        }

        if (loaded.Value is null)
        {
            return OperationResult<LoadedCheckpoint>.Fail(
                "DesignConversion learned-weight checkpoint fresh-load returned no checkpoint.",
                "DESIGN_CONVERSION_REPOSITORY_UPLOAD_CHECKPOINT_LOAD_EMPTY");
        }

        var export = await loaded.Value.Machine.ExportAsync(ct);
        if (!export.IsSuccess)
        {
            return OperationResult<LoadedCheckpoint>.Fail(
                "DesignConversion learned-weight checkpoint export verification failed.",
                PreserveSafeErrorCode(
                    export.ErrorCode,
                    "DESIGN_CONVERSION_REPOSITORY_UPLOAD_CHECKPOINT_EXPORT_FAILED"));
        }

        if (export.Value is null)
        {
            return OperationResult<LoadedCheckpoint>.Fail(
                "DesignConversion learned-weight checkpoint export verification returned no export.",
                "DESIGN_CONVERSION_REPOSITORY_UPLOAD_CHECKPOINT_EXPORT_EMPTY");
        }

        if (validateManifestCheckpointMetadata)
        {
            var expectedCheckpointRef = NormalizeManifestCheckpointRef(manifest.CheckpointRef);
            var expectedManifestRef =
                $"manifest://{manifest.MachineId}/{manifest.Version}/sha256/{loaded.Value.CheckpointSha256}";
            if (!string.Equals(export.Value.MachineId, manifest.MachineId, StringComparison.Ordinal) ||
                !string.Equals(export.Value.DomainTag, manifest.DomainTag, StringComparison.Ordinal) ||
                !string.Equals(export.Value.Version, manifest.Version, StringComparison.Ordinal) ||
                !string.Equals(export.Value.ManifestRef, manifest.ManifestRef, StringComparison.Ordinal) ||
                !string.Equals(export.Value.ManifestRef, expectedManifestRef, StringComparison.Ordinal) ||
                expectedCheckpointRef is null ||
                !string.Equals(export.Value.CheckpointRef, expectedCheckpointRef, StringComparison.OrdinalIgnoreCase))
            {
                return OperationResult<LoadedCheckpoint>.Fail(
                    "DesignConversion learned-weight checkpoint metadata does not match the promotion manifest.",
                    "DESIGN_CONVERSION_REPOSITORY_UPLOAD_CHECKPOINT_METADATA_MISMATCH");
            }
        }

        return OperationResult<LoadedCheckpoint>.Ok(new LoadedCheckpoint(
            loaded.Value,
            export.Value));
    }

    private static string? NormalizeManifestCheckpointRef(string? checkpointRef)
    {
        if (string.IsNullOrWhiteSpace(checkpointRef))
            return null;

        try
        {
            return Path.IsPathFullyQualified(checkpointRef)
                ? Path.GetFullPath(checkpointRef)
                : null;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }
    }

    private static UploadUnit BuildUploadUnit(
        DesignConversionRuntimeUnitCheckpoint unit,
        DesignConversionTrainingManifest manifest,
        LearningMachineExport loadedExport,
        ValidatedPromotionArtifacts artifacts,
        string checkpointSha256,
        string policyCheckpointSha256,
        string sourceDatasetSha256,
        int learnedWeightArtifactCount,
        int learnedWeightObservationCount,
        DateTimeOffset createdAtUtc)
    {
        var manifestIdHash = ShortSha256Hex(manifest.ManifestId, 16);
        var manifestRefHash = ShortSha256Hex(manifest.ManifestRef, 16);
        var checkpointHashRef = ShortSha256Hex(checkpointSha256, 16);
        var policyHashRef = ShortSha256Hex(policyCheckpointSha256, 16);
        var isEvaluatedDpo = artifacts.IsEvaluatedDpoArtifactSet;
        var context = artifacts.PolicyTrainingContext;
        var lora = context?.Lora;
        var boundedTrainingText = isEvaluatedDpo
            ? string.Join(
                " ",
                "DesignConversion evaluated DPO policy checkpoint promotion.",
                $"unit_id={unit.UnitId};",
                $"runtime_checkpoint_sha256={checkpointHashRef};",
                $"policy_checkpoint_sha256={policyHashRef};",
                $"manifest_ref_sha256={manifestRefHash};",
                $"dpo_step_count={artifacts.DpoStepCount.ToString(CultureInfo.InvariantCulture)};",
                $"accepted_record_count={manifest.AcceptedRecordCount.ToString(CultureInfo.InvariantCulture)};",
                "source_encoder_contract_present;",
                "transformer_config_from_policy_contract;",
                "no_cloud_readiness_claim;",
                "no_raw_training_payloads.")
            : string.Join(
                " ",
                "DesignConversion learned-weight JSON checkpoint promotion.",
                $"unit_id={unit.UnitId};",
                $"checkpoint_sha256={checkpointHashRef};",
                $"manifest_ref_sha256={manifestRefHash};",
                $"batch_count={manifest.Batches.Count.ToString(CultureInfo.InvariantCulture)};",
                $"accepted_record_count={manifest.AcceptedRecordCount.ToString(CultureInfo.InvariantCulture)};",
                "non_transformer_json_checkpoint;",
                "no_dpo_readiness_claim;",
                "no_cloud_readiness_claim;",
                "no_raw_training_payloads.");
        var boundedOutputText = isEvaluatedDpo
            ? string.Join(
                " ",
                "Repository manifest maps a DesignConversion evaluated DPO transformer policy checkpoint.",
                $"unit_id={unit.UnitId};",
                $"source_record_count={artifacts.SourceRecordCount.ToString(CultureInfo.InvariantCulture)};",
                $"bridge_preference_count={artifacts.BridgePreferenceCount.ToString(CultureInfo.InvariantCulture)};",
                $"dpo_step_count={artifacts.DpoStepCount.ToString(CultureInfo.InvariantCulture)};",
                "transformer_config_nonzero;",
                lora is null ? "lora_config_absent;" : "lora_config_from_policy_contract;",
                "dpo_policy_checkpoint_not_cloud_ready.")
            : string.Join(
                " ",
                "Repository manifest maps a DesignConversion learned-weight JSON checkpoint runtime unit.",
                $"unit_id={unit.UnitId};",
                $"learned_weight_artifact_count={learnedWeightArtifactCount.ToString(CultureInfo.InvariantCulture)};",
                $"learned_weight_observation_count={learnedWeightObservationCount.ToString(CultureInfo.InvariantCulture)};",
                "transformer_config_zero;",
                "lora_config_empty;",
                "not_transformer_not_dpo_not_cloud_ready.");
        var originalPrompt = isEvaluatedDpo
            ? string.Join(
                " ",
                "Publish DesignConversion evaluated DPO policy checkpoint to model repository.",
                $"machine_id={loadedExport.MachineId};",
                $"domain_tag={loadedExport.DomainTag};",
                $"version={loadedExport.Version};",
                "source_encoder_contract_used_for_repository_upload;",
                "policy_checkpoint_artifact_used_for_fresh_load_verification;",
                "no_absolute_paths_or_raw_payloads_in_public_receipt.")
            : string.Join(
                " ",
                "Publish DesignConversion learned-weight JSON checkpoint to model repository.",
                $"machine_id={loadedExport.MachineId};",
                $"domain_tag={loadedExport.DomainTag};",
                $"version={loadedExport.Version};",
                "full_machine_checkpoint_used_as_policy_checkpoint;",
                "no_absolute_paths_or_raw_payloads_in_public_receipt.");

        var generated = new GeneratedCapabilityCheckpointManifest(
            UnitId: unit.UnitId,
            UnitRole: unit.UnitRole,
            DomainTag: unit.DomainTag,
            Version: manifest.Version,
            TrustState: LogicalUnitTrustState.Probationary.ToString(),
            FlowId: DesignConversionRoutingFlow.FlowId,
            ScenarioId: "design-conversion-model-repository-promotion",
            PhaseId: "design-conversion",
            SubPhaseId: unit.UnitId,
            TaskType: isEvaluatedDpo ? EvaluatedDpoTaskType : TaskType,
            TrainingText: boundedTrainingText,
            OutputText: boundedOutputText,
            ExampleId: $"learned-weight-json-checkpoint-{manifestIdHash}-{ShortSha256Hex(unit.UnitId, 8)}",
            PredicateIds: isEvaluatedDpo
                ?
                [
                    "design_conversion_evaluated_dpo_policy_checkpoint",
                    "dpo_source_encoder_contract_present",
                    "transformer_config_from_policy_contract",
                    "no_raw_training_payloads"
                ]
                :
                [
                    "design_conversion_learned_weight_json_checkpoint",
                    "non_transformer_zero_config",
                    "no_raw_training_payloads"
                ],
            DomainTags: [manifest.DomainTag],
            PolicyCheckpointPath: artifacts.PolicyCheckpointUploadPath,
            TokenizerPath: artifacts.TokenizerPath,
            VocabSize: context?.VocabSize ?? 0,
            ContextLen: context?.ContextLength ?? 0,
            HiddenDim: context?.HiddenDim ?? 0,
            NumLayers: context?.NumLayers ?? 0,
            NumHeads: context?.NumHeads ?? 0,
            NumKvHeads: context?.NumKvHeads ?? 0,
            FfnHiddenDim: context?.FfnHiddenDim ?? 0,
            LoraRank: lora?.Rank ?? 0,
            LoraAlpha: lora?.Alpha ?? 0f,
            LoraDropout: lora?.Dropout ?? 0f,
            LoraTargetModules: lora?.TargetModules.ToArray() ?? [],
            CreatedAtUtc: createdAtUtc,
            UnitFamilyId: isEvaluatedDpo ? EvaluatedDpoUnitFamilyId : UnitFamilyId,
            TrainingStepCount: isEvaluatedDpo
                ? Math.Max(1, artifacts.DpoStepCount)
                : Math.Max(1, manifest.Batches.Count),
            OriginalPrompt: originalPrompt,
            ServerBaseUrl: null,
            ManifestSha256: policyCheckpointSha256,
            TrainingBranch: isEvaluatedDpo
                ? "design-conversion-evaluated-dpo-policy-checkpoint"
                : "design-conversion-learned-weight-json-checkpoint",
            TrainingRunId: $"manifest-{manifestIdHash}",
            SourceDatasetSha256: sourceDatasetSha256,
            RopeTheta: context?.RopeTheta,
            RmsNormEps: context?.RmsNormEps,
            // Унаследованный ManifestSha256 у remote repository остается hash загруженного payload-файла.
            // RuntimeProvenanceSha256 отдельно сохраняет hash runtime checkpoint, если evaluated DPO грузит policy bytes.
            PolicyCheckpointSha256: policyCheckpointSha256,
            RuntimeProvenanceSha256: checkpointSha256);

        return new UploadUnit(
            Manifest: generated,
            RuntimeUnit: unit,
            LearnedWeightArtifactCount: learnedWeightArtifactCount,
            LearnedWeightObservationCount: learnedWeightObservationCount);
    }

    private static OperationResult<bool> ValidateUploadedManifest(
        RemoteUnitManifest remote,
        UploadUnit expected,
        string serverBaseUrl)
    {
        var expectedUnitEndpoint = $"/models/{Uri.EscapeDataString(expected.Manifest.UnitId)}";
        if (!string.Equals(remote.UnitId, expected.Manifest.UnitId, StringComparison.Ordinal) ||
            !string.Equals(remote.UnitRole, expected.Manifest.UnitRole, StringComparison.Ordinal) ||
            !string.Equals(remote.DomainTag, expected.Manifest.DomainTag, StringComparison.Ordinal) ||
            !string.Equals(remote.Version, expected.Manifest.Version, StringComparison.Ordinal) ||
            !string.Equals(remote.ServerBaseUrl, serverBaseUrl, StringComparison.Ordinal) ||
            !string.Equals(remote.UnitEndpoint, expectedUnitEndpoint, StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(remote.ManifestSha256) ||
            !string.Equals(remote.ManifestSha256, expected.Manifest.ManifestSha256, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(remote.TrainingBranch, expected.Manifest.TrainingBranch ?? string.Empty, StringComparison.Ordinal) ||
            !string.Equals(remote.TrainingRunId, expected.Manifest.TrainingRunId ?? string.Empty, StringComparison.Ordinal) ||
            !string.Equals(remote.SourceDatasetSha256, expected.Manifest.SourceDatasetSha256, StringComparison.Ordinal) ||
            !string.Equals(remote.TokenizerPath, expected.Manifest.TokenizerPath, StringComparison.Ordinal) ||
            !RemoteModelConfigMatches(remote.ModelConfig, expected.Manifest) ||
            !RemoteLoraConfigMatches(remote.LoraConfig, expected.Manifest) ||
            !BuildRemoteRef(remote, serverBaseUrl).IsSuccess)
        {
            var remoteRef = BuildRemoteRef(remote, serverBaseUrl);
            return OperationResult<bool>.Fail(
                $"DesignConversion model repository upload returned an incoherent remote manifest. DIAG remote_ref_success={remoteRef.IsSuccess} remote_ref_error={remoteRef.ErrorCode} remote_ref_message={remoteRef.ErrorMessage}",
                "DESIGN_CONVERSION_REPOSITORY_UPLOAD_REMOTE_MANIFEST_MISMATCH");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateLocatedManifest(
        RemoteUnitManifest located,
        RemoteUnitManifest expected)
    {
        if (!string.Equals(located.UnitId, expected.UnitId, StringComparison.Ordinal) ||
            !string.Equals(located.UnitRole, expected.UnitRole, StringComparison.Ordinal) ||
            !string.Equals(located.DomainTag, expected.DomainTag, StringComparison.Ordinal) ||
            !string.Equals(located.Version, expected.Version, StringComparison.Ordinal) ||
            !string.Equals(located.ServerBaseUrl, expected.ServerBaseUrl, StringComparison.Ordinal) ||
            !string.Equals(located.UnitEndpoint, expected.UnitEndpoint, StringComparison.Ordinal) ||
            !string.Equals(located.ManifestSha256, expected.ManifestSha256, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(located.TrainingBranch, expected.TrainingBranch, StringComparison.Ordinal) ||
            !string.Equals(located.TrainingRunId, expected.TrainingRunId, StringComparison.Ordinal) ||
            !string.Equals(located.SourceDatasetSha256, expected.SourceDatasetSha256, StringComparison.Ordinal) ||
            !string.Equals(located.TokenizerPath, expected.TokenizerPath, StringComparison.Ordinal) ||
            !RemoteModelConfigMatches(located.ModelConfig, expected.ModelConfig) ||
            !RemoteLoraConfigMatches(located.LoraConfig, expected.LoraConfig) ||
            located.RegisteredAtUtc != expected.RegisteredAtUtc)
        {
            return OperationResult<bool>.Fail(
                "DesignConversion remote locator returned a manifest identity mismatch.",
                "DESIGN_CONVERSION_REPOSITORY_LOCATOR_IDENTITY_MISMATCH");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool RemoteModelConfigMatches(
        RemoteModelConfig? config,
        GeneratedCapabilityCheckpointManifest expected)
        => config is not null &&
           config.VocabSize == expected.VocabSize &&
           config.ContextLen == expected.ContextLen &&
           config.HiddenDim == expected.HiddenDim &&
           config.NumLayers == expected.NumLayers &&
           config.NumHeads == expected.NumHeads &&
           config.NumKvHeads == expected.NumKvHeads &&
           config.FfnHiddenDim == expected.FfnHiddenDim &&
           NullableSingleBitsEqual(config.RopeTheta, expected.RopeTheta) &&
           NullableSingleBitsEqual(config.RmsNormEps, expected.RmsNormEps);

    private static bool RemoteModelConfigMatches(
        RemoteModelConfig? actual,
        RemoteModelConfig? expected)
        => actual is not null &&
           expected is not null &&
           actual.VocabSize == expected.VocabSize &&
           actual.ContextLen == expected.ContextLen &&
           actual.HiddenDim == expected.HiddenDim &&
           actual.NumLayers == expected.NumLayers &&
           actual.NumHeads == expected.NumHeads &&
           actual.NumKvHeads == expected.NumKvHeads &&
           actual.FfnHiddenDim == expected.FfnHiddenDim &&
           NullableSingleBitsEqual(actual.RopeTheta, expected.RopeTheta) &&
           NullableSingleBitsEqual(actual.RmsNormEps, expected.RmsNormEps);

    private static bool RemoteLoraConfigMatches(
        RemoteLoraConfig? config,
        GeneratedCapabilityCheckpointManifest expected)
        => config is not null &&
           config.Rank == expected.LoraRank &&
           BitConverter.SingleToInt32Bits(config.Alpha) == BitConverter.SingleToInt32Bits(expected.LoraAlpha) &&
           BitConverter.SingleToInt32Bits(config.Dropout) == BitConverter.SingleToInt32Bits(expected.LoraDropout) &&
           (config.TargetModules ?? []).SequenceEqual(
               expected.LoraTargetModules,
               StringComparer.Ordinal);

    private static bool RemoteLoraConfigMatches(
        RemoteLoraConfig? actual,
        RemoteLoraConfig? expected)
        => actual is not null &&
           expected is not null &&
           actual.Rank == expected.Rank &&
           BitConverter.SingleToInt32Bits(actual.Alpha) == BitConverter.SingleToInt32Bits(expected.Alpha) &&
           BitConverter.SingleToInt32Bits(actual.Dropout) == BitConverter.SingleToInt32Bits(expected.Dropout) &&
           (actual.TargetModules ?? []).SequenceEqual(
               expected.TargetModules ?? [],
               StringComparer.Ordinal);

    private static bool NullableSingleBitsEqual(float? actual, float? expected)
        => actual.HasValue == expected.HasValue &&
           (!actual.HasValue ||
            BitConverter.SingleToInt32Bits(actual.Value) ==
            BitConverter.SingleToInt32Bits(expected!.Value));

    private static OperationResult<string> BuildRemoteRef(RemoteUnitManifest manifest, string fallbackServerBaseUrl)
    {
        var baseUrl = string.IsNullOrWhiteSpace(manifest.ServerBaseUrl)
            ? fallbackServerBaseUrl
            : manifest.ServerBaseUrl;

        if (!Uri.TryCreate(baseUrl.TrimEnd('/') + "/", UriKind.Absolute, out var parsedBase) ||
            !string.Equals(parsedBase.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !HasSafeReceiptHost(parsedBase) ||
            !string.IsNullOrEmpty(parsedBase.UserInfo) ||
            !string.IsNullOrEmpty(parsedBase.Query) ||
            !string.IsNullOrEmpty(parsedBase.Fragment))
        {
            return OperationResult<string>.Fail(
                "DesignConversion remote locator endpoint base URL was not a safe absolute HTTPS URL.",
                "DESIGN_CONVERSION_REPOSITORY_REMOTE_REF_HTTPS_REQUIRED");
        }

        if (string.IsNullOrWhiteSpace(manifest.UnitEndpoint))
        {
            return OperationResult<string>.Fail(
                "DesignConversion remote locator endpoint was missing.",
                "DESIGN_CONVERSION_REPOSITORY_REMOTE_REF_ENDPOINT_REQUIRED");
        }

        var trimmedEndpoint = manifest.UnitEndpoint.Trim();
        if (!HasSafeUnitEndpoint(trimmedEndpoint))
        {
            return OperationResult<string>.Fail(
                "DesignConversion remote locator endpoint was not a safe relative path.",
                "DESIGN_CONVERSION_REPOSITORY_REMOTE_REF_ENDPOINT_UNSAFE");
        }

        var endpoint = trimmedEndpoint.StartsWith("/", StringComparison.Ordinal)
            ? trimmedEndpoint[1..]
            : trimmedEndpoint;
        var remoteRef = new Uri(parsedBase, endpoint).AbsoluteUri;
        if (!Uri.TryCreate(remoteRef, UriKind.Absolute, out var parsedRemote) ||
            !string.Equals(parsedRemote.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !HasSafeReceiptHost(parsedRemote) ||
            !string.IsNullOrEmpty(parsedRemote.UserInfo) ||
            !string.IsNullOrEmpty(parsedRemote.Query) ||
            !string.IsNullOrEmpty(parsedRemote.Fragment) ||
            !HasSafeRemoteReceiptPath(parsedRemote.AbsolutePath))
        {
            return OperationResult<string>.Fail(
                "DesignConversion remote locator endpoint was not a safe absolute HTTPS URL.",
                "DESIGN_CONVERSION_REPOSITORY_REMOTE_REF_HTTPS_REQUIRED");
        }

        return OperationResult<string>.Ok(parsedRemote.AbsoluteUri);
    }

    private static IReadOnlyList<string> BuildReceiptEvidence(
        IReadOnlyList<UploadedUnit> uploadedUnits,
        ValidatedPromotionArtifacts artifacts,
        string checkpointSha256,
        string policyCheckpointSha256,
        string sourceDatasetSha256,
        int criticWeights,
        int improverWeights,
        int criticObservations,
        int improverObservations,
        DesignConversionLoadedLearnedWeightAuthoritySummary loadedWeightAuthority)
    {
        IReadOnlyList<string> routeEvidence = artifacts.IsEvaluatedDpoArtifactSet
            ? new[]
            {
                "DESIGN_CONVERSION_EVALUATED_DPO_POLICY_CHECKPOINT_UPLOADED",
                "DESIGN_CONVERSION_REPOSITORY_VALIDATED_SIBLING_DPO_POLICY_WEIGHT_AUTHORITY",
                "MODEL_REPOSITORY_SHARED_DPO_POLICY_CHECKPOINT_PUBLISHED_FOR_BOTH_LOGICAL_UNITS",
                "MODEL_REPOSITORY_POLICY_CHECKPOINT_ARTIFACT_SHA_VERIFIED_BOTH_UNITS",
                "TRANSFORMER_AND_LORA_CONFIG_FROM_DPO_POLICY_CONTRACT",
                "NO_CLOUD_READINESS_CLAIM"
            }
            : new[]
            {
                "DESIGN_CONVERSION_LEARNED_WEIGHT_JSON_CHECKPOINT_EXPLICIT_NON_TRANSFORMER",
                "TRANSFORMER_AND_LORA_CONFIG_ZERO_EMPTY",
                "NO_DPO_OR_CLOUD_READINESS_CLAIM"
            };

        return new[]
        {
            "MODEL_REPOSITORY_UPLOAD_BOTH_UNITS_SUCCEEDED",
            "MODEL_REPOSITORY_FRESH_LOAD_SHA_VERIFIED_BOTH_UNITS",
            "REMOTE_LOCATOR_REGISTERED_BOTH_UNITS",
            "REMOTE_LOCATOR_LOCATED_BOTH_UNITS",
            $"checkpoint_sha256:{ShortSha256Hex(checkpointSha256, 16)}",
            $"policy_checkpoint_sha256:{ShortSha256Hex(policyCheckpointSha256, 16)}",
            $"source_dataset_sha256:{sourceDatasetSha256}",
            $"critic_unit_sha256:{ShortSha256Hex(uploadedUnits[0].FreshLoadSha256 ?? string.Empty, 16)}",
            $"improver_unit_sha256:{ShortSha256Hex(uploadedUnits[1].FreshLoadSha256 ?? string.Empty, 16)}",
            $"critic_learned_weight_artifacts:{criticWeights.ToString(CultureInfo.InvariantCulture)}",
            $"improver_learned_weight_artifacts:{improverWeights.ToString(CultureInfo.InvariantCulture)}",
            $"critic_learned_weight_observations:{criticObservations.ToString(CultureInfo.InvariantCulture)}",
            $"improver_learned_weight_observations:{improverObservations.ToString(CultureInfo.InvariantCulture)}",
            $"dpo_policy_learned_weight_artifacts:{loadedWeightAuthority.DpoPolicyLearnedWeightArtifactCount.ToString(CultureInfo.InvariantCulture)}",
            $"dpo_policy_learned_weight_observations:{loadedWeightAuthority.DpoPolicyLearnedWeightObservationCount.ToString(CultureInfo.InvariantCulture)}",
            $"loaded_learned_weight_total_artifacts:{loadedWeightAuthority.TotalLearnedWeightArtifactCount.ToString(CultureInfo.InvariantCulture)}",
            $"loaded_learned_weight_total_observations:{loadedWeightAuthority.TotalLearnedWeightObservationCount.ToString(CultureInfo.InvariantCulture)}",
            $"repository_logical_unit_publications:{uploadedUnits.Count.ToString(CultureInfo.InvariantCulture)}",
            $"repository_distinct_policy_checkpoint_artifacts:{uploadedUnits.Select(unit => $"{Path.GetFullPath(unit.Unit.Manifest.PolicyCheckpointPath)}|{unit.Unit.Manifest.PolicyCheckpointSha256}").Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString(CultureInfo.InvariantCulture)}",
            $"registered_critic_endpoint_sha256:{ShortSha256Hex(uploadedUnits[0].Remote.UnitEndpoint, 16)}",
            $"registered_improver_endpoint_sha256:{ShortSha256Hex(uploadedUnits[1].Remote.UnitEndpoint, 16)}"
        }
        .Concat(routeEvidence)
        .Distinct(StringComparer.Ordinal)
        .Order(StringComparer.Ordinal)
        .ToArray();
    }

    private static OperationResult<string> ValidateServerBaseUrl(string serverBaseUrl)
    {
        var trimmedServerBaseUrl = serverBaseUrl?.Trim() ?? string.Empty;
        if (trimmedServerBaseUrl.Length == 0 ||
            !HasSafeRawAbsoluteUrlPath(trimmedServerBaseUrl) ||
            !Uri.TryCreate(trimmedServerBaseUrl, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !HasSafeReceiptHost(uri) ||
            !HasSafeRemoteReceiptPath(uri.AbsolutePath) ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment))
        {
            return OperationResult<string>.Fail(
                "DesignConversion model repository server base URL must be an absolute HTTPS URL with a safe path and without credentials, query, or fragment.",
                "DESIGN_CONVERSION_REPOSITORY_UPLOAD_SERVER_BASE_URL_HTTPS_REQUIRED");
        }

        return OperationResult<string>.Ok(uri.AbsoluteUri.TrimEnd('/'));
    }

    private static bool HasSafeRawAbsoluteUrlPath(string absoluteUrl)
    {
        if (absoluteUrl.Any(ch => char.IsControl(ch) || char.IsWhiteSpace(ch) || ch == '\\'))
            return false;

        var schemeDelimiterIndex = absoluteUrl.IndexOf("://", StringComparison.Ordinal);
        if (schemeDelimiterIndex <= 0)
            return false;

        var authorityStartIndex = schemeDelimiterIndex + 3;
        var componentStartIndex = absoluteUrl.IndexOfAny(['/', '?', '#'], authorityStartIndex);
        if (componentStartIndex < 0 || absoluteUrl[componentStartIndex] != '/')
            return HasSafeRemoteReceiptPath("/");

        var pathEndIndex = absoluteUrl.IndexOfAny(['?', '#'], componentStartIndex);
        var rawPath = pathEndIndex < 0
            ? absoluteUrl[componentStartIndex..]
            : absoluteUrl[componentStartIndex..pathEndIndex];
        return HasSafeRemoteReceiptPath(rawPath);
    }

    private static bool HasSafeUnitEndpoint(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint) ||
            !string.Equals(endpoint, endpoint.Trim(), StringComparison.Ordinal) ||
            endpoint.Any(ch => char.IsControl(ch) || char.IsWhiteSpace(ch)) ||
            endpoint.Contains('\\', StringComparison.Ordinal) ||
            endpoint.IndexOfAny(['?', '#']) >= 0 ||
            endpoint.StartsWith("//", StringComparison.Ordinal) ||
            HasUriScheme(endpoint))
        {
            return false;
        }

        var rawPath = endpoint.StartsWith("/", StringComparison.Ordinal)
            ? endpoint
            : "/" + endpoint;
        return HasSafeRemoteReceiptPath(rawPath);
    }

    private static bool HasUriScheme(string value)
    {
        var colonIndex = value.IndexOf(':');
        if (colonIndex <= 0 ||
            !char.IsAsciiLetter(value[0]))
        {
            return false;
        }

        // На Linux путь "/models/..." может распознаться как absolute file URI; здесь нужен только явный URI scheme.
        return value[..colonIndex].All(ch =>
            char.IsAsciiLetterOrDigit(ch) ||
            ch is '+' or '-' or '.');
    }

    private static bool HasSafeReceiptHost(Uri uri)
    {
        var host = uri.IdnHost;
        if (string.IsNullOrWhiteSpace(host) || host.Length > 253)
            return false;

        return uri.HostNameType switch
        {
            UriHostNameType.Dns => host
                .Split('.', StringSplitOptions.None)
                .All(label => label.Length is > 0 and <= 63 &&
                              char.IsAsciiLetterOrDigit(label[0]) &&
                              char.IsAsciiLetterOrDigit(label[^1]) &&
                              label.All(ch => char.IsAsciiLetterOrDigit(ch) || ch == '-')),
            UriHostNameType.IPv4 => host.All(ch => char.IsAsciiDigit(ch) || ch == '.'),
            UriHostNameType.IPv6 => host.All(ch => char.IsAsciiHexDigit(ch) || ch is ':' or '.'),
            _ => false
        };
    }

    private static bool HasSafeRemoteReceiptPath(string rawPath)
    {
        const int maxDecodeLayers = 8;
        var currentLayer = rawPath;

        for (var decodeCount = 0; ; decodeCount++)
        {
            if (!TryValidateRemoteReceiptPathLayer(currentLayer, out var hasPercentEscapes))
                return false;
            if (!hasPercentEscapes)
                return true;
            if (decodeCount >= maxDecodeLayers)
                return false;

            string decodedLayer;
            try
            {
                decodedLayer = Uri.UnescapeDataString(currentLayer);
            }
            catch (UriFormatException)
            {
                return false;
            }

            if (string.Equals(decodedLayer, currentLayer, StringComparison.Ordinal))
                return false;

            currentLayer = decodedLayer;
        }
    }

    private static bool TryValidateRemoteReceiptPathLayer(string path, out bool hasPercentEscapes)
    {
        hasPercentEscapes = false;
        if (path.Any(ch => char.IsControl(ch) || char.IsWhiteSpace(ch) || ch == '\\') ||
            path.Split('/', StringSplitOptions.None).Any(segment => segment is "." or ".."))
        {
            return false;
        }

        for (var index = 0; index < path.Length; index++)
        {
            if (path[index] != '%')
                continue;

            hasPercentEscapes = true;
            if (index > path.Length - 3)
                return false;

            var high = HexDigitValue(path[index + 1]);
            var low = HexDigitValue(path[index + 2]);
            if (high < 0 || low < 0)
                return false;

            var decodedByte = (high << 4) | low;
            if (decodedByte <= 0x20 ||
                decodedByte == 0x7f ||
                decodedByte is 0x2f or 0x5c)
            {
                return false;
            }

            index += 2;
        }

        return true;
    }

    private static int HexDigitValue(char value)
        => value switch
        {
            >= '0' and <= '9' => value - '0',
            >= 'a' and <= 'f' => value - 'a' + 10,
            >= 'A' and <= 'F' => value - 'A' + 10,
            _ => -1
        };

    private static async Task<OperationResult<string>> ComputeSha256HexAsync(
        Stream stream,
        CancellationToken ct)
    {
        try
        {
            if (stream.CanSeek)
                stream.Position = 0;

            var hash = await SHA256.HashDataAsync(stream, ct);
            return OperationResult<string>.Ok(Convert.ToHexString(hash).ToLowerInvariant());
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return OperationResult<string>.Fail(
                "DesignConversion model repository stream hash was cancelled.",
                "CANCELLED");
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException or NotSupportedException)
        {
            return OperationResult<string>.Fail(
                "DesignConversion model repository stream hash failed.",
                "DESIGN_CONVERSION_REPOSITORY_LOAD_HASH_FAILED");
        }
    }

    private static string BuildSourceDatasetSha256(DesignConversionTrainingManifest manifest)
    {
        var refs = (manifest.Batches ?? [])
            .SelectMany(batch =>
                (batch.RecordIds ?? []).Select(recordId => $"record:{recordId}")
                    .Concat((batch.SourceIds ?? []).Select(sourceId => $"source:{sourceId}")))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var material = refs.Length == 0
            ? "design-conversion:empty-record-source-ref-set"
            : string.Join("\n", refs);
        return Sha256Hex(material);
    }

    private static bool RuntimeUnitHasRealLearnedWeights(DesignConversionRuntimeUnitCheckpoint unit)
        => unit.LearnedWeightsPresent &&
           CountLearnedWeightArtifacts(unit) > 0;

    private static int CountLearnedWeightArtifacts(DesignConversionRuntimeUnitCheckpoint unit)
        => unit.LearnedWeights?
            .Count(IsRealLearnedWeight) ?? 0;

    private static int CountLearnedWeightObservations(DesignConversionRuntimeUnitCheckpoint unit)
        => unit.LearnedWeights?
            .Where(IsRealLearnedWeight)
            .Sum(weight => weight.ObservationCount) ?? 0;

    private static bool IsRealLearnedWeight(DesignConversionLearnedWeightCheckpoint? weight)
        => weight is not null &&
           !string.IsNullOrWhiteSpace(weight.FeatureKey) &&
           IsAllowedLearnedWeightFeatureKey(weight.FeatureKey) &&
           double.IsFinite(weight.Weight) &&
           weight.ObservationCount > 0 &&
           !string.IsNullOrWhiteSpace(weight.LastBatchId) &&
           !string.IsNullOrWhiteSpace(weight.Derivation) &&
           weight.SourceIdentityHashes is not null &&
           weight.SourceIdentityHashes.Count > 0 &&
           weight.SourceIdentityHashes.All(IsSha256Hex) &&
           weight.EvidenceRefs is not null &&
           weight.EvidenceRefs.Contains(DesignConversionMachine.LearnedWeightsSanitizedFieldsEvidence, StringComparer.Ordinal);

    private static bool IsAllowedLearnedWeightFeatureKey(string featureKey)
        => !string.IsNullOrWhiteSpace(featureKey) &&
           !featureKey.Contains("usefulness", StringComparison.OrdinalIgnoreCase) &&
           !featureKey.Contains("review_priority", StringComparison.OrdinalIgnoreCase) &&
           !featureKey.Contains("accepted_delta", StringComparison.OrdinalIgnoreCase) &&
           !featureKey.Contains("score_margin", StringComparison.OrdinalIgnoreCase) &&
           !string.Equals(featureKey, "visual:chosen_score", StringComparison.Ordinal) &&
           !string.Equals(featureKey, "visual:rejected_score_inverse", StringComparison.Ordinal);

    private static bool IsSha256Hex(string? value)
        => value is { Length: 64 } &&
           value.All(char.IsAsciiHexDigit);

    private static string BuildSafeToken(string prefix, params string[] parts)
        => $"{prefix}:{ShortSha256Hex(string.Join("\n", parts), 24)}";

    private static string ShortSha256Hex(string value, int length)
        => Sha256Hex(value)[..Math.Min(length, 64)];

    private static string Sha256Hex(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static OperationResult<DesignConversionPromotionUploadReceipt> Fail(string message, string code)
        => OperationResult<DesignConversionPromotionUploadReceipt>.Fail(message, code);

    private static OperationResult<DesignConversionPromotionUploadReceipt> FailLocatorRegistration(
        string code,
        int acceptedRegistrationCount)
        => Fail(
            acceptedRegistrationCount > 0
                ? "DesignConversion remote locator registration failed after a prior unit registration may already be visible; IRemoteUnitLocator has no rollback API, so promotion fails closed and no success receipt is exposed."
                : "DesignConversion remote locator registration failed before all units were registered; promotion fails closed and no success receipt is exposed.",
            code);

    private static OperationResult<DesignConversionPromotionUploadReceipt>
        EvaluatedArtifactPublicationChanged()
        => Fail(
            "DesignConversion evaluated DPO artifact publication changed during repository promotion.",
            "DESIGN_CONVERSION_REPOSITORY_UPLOAD_EVALUATED_DPO_ARTIFACT_REVALIDATION_FAILED");

    private static string PreserveSafeErrorCode(string? errorCode, string fallback)
    {
        if (string.IsNullOrWhiteSpace(errorCode) ||
            errorCode.Length > 128 ||
            errorCode.Any(ch => !char.IsAsciiLetterOrDigit(ch) && ch is not '_' and not '-') ||
            DesignConversionDpoLeakageGuard.ContainsForbiddenLeakage(errorCode))
        {
            return fallback;
        }

        return errorCode;
    }

    private sealed record ValidatedPromotionArtifacts(
        string CheckpointPath,
        string TokenizerPath,
        string PolicyPath,
        string PolicyCheckpointUploadPath,
        string PolicyCheckpointSha256,
        bool IsEvaluatedDpoArtifactSet,
        DesignConversionDpoPolicyCheckpointTrainingContext? PolicyTrainingContext,
        int SourceRecordCount,
        int BridgePreferenceCount,
        int DpoStepCount,
        DesignConversionDpoEvaluatedArtifactUploadLease? EvaluatedLease) : IDisposable
    {
        public bool Revalidate()
            => EvaluatedLease?.Revalidate() ?? true;

        public void Dispose()
            => EvaluatedLease?.Dispose();
    }

    private sealed record LoadedCheckpoint(
        DesignConversionMachineLoadResult LoadResult,
        LearningMachineExport Export);

    private sealed record UploadUnit(
        GeneratedCapabilityCheckpointManifest Manifest,
        DesignConversionRuntimeUnitCheckpoint RuntimeUnit,
        int LearnedWeightArtifactCount,
        int LearnedWeightObservationCount);

    private sealed class UploadedUnit(UploadUnit unit, RemoteUnitManifest remote)
    {
        public UploadUnit Unit { get; } = unit;
        public RemoteUnitManifest Remote { get; } = remote;
        public string? FreshLoadSha256 { get; set; }
        public RemoteUnitManifest? Located { get; set; }
    }
}
