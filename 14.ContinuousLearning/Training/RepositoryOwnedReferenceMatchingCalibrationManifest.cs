// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

/// <summary>
/// Immutable manifest for one repository-owned reference-matching calibration.
/// It stores only identities, hashes, counts, rates and row ids; answer bytes stay
/// in caller memory and never become a training/DPO artifact.
/// </summary>
public sealed record RepositoryOwnedReferenceMatchingCalibrationManifest(
    string Schema,
    int SchemaVersion,
    string Context,
    string RepositoryRelativeCalibrationDirectory,
    string RepositoryRelativeManifestPath,
    string RepositoryRelativeLinkedCorpusManifestPath,
    string LinkedCorpusManifestSha256,
    string Domain,
    string Subject,
    string CorpusVersion,
    string CorpusSha256,
    int CorpusEntryCount,
    string RuleDomain,
    string RuleId,
    string RuleVersion,
    string RuleHumanDescription,
    string RuleIdentitySha256,
    string CalibrationSetSha256,
    int KnownPositiveCount,
    IReadOnlyList<string> KnownPositivePairIds,
    int KnownNegativeCount,
    IReadOnlyList<string> KnownNegativePairIds,
    int FalseMissCount,
    double FalseMissRate,
    IReadOnlyList<string> FalseMissPairIds,
    int FalseHitCount,
    double FalseHitRate,
    IReadOnlyList<string> FalseHitPairIds,
    string ResultSha256,
    string ManifestSha256);

/// <summary>
/// Freshly reopened calibration manifest plus the sealed frozen corpus it links to.
/// </summary>
public sealed record RepositoryOwnedReferenceMatchingCalibrationOpenResult(
    RepositoryOwnedReferenceMatchingCalibrationManifest Manifest,
    RepositoryOwnedReferenceCorpusOpenResult LinkedCorpus);

/// <summary>
/// Persists and reopens sealed repository-owned calibration numbers.
/// </summary>
public static class RepositoryOwnedReferenceMatchingCalibrationManifestSealer
{
    public const string InvalidErrorCode =
        "REPOSITORY_OWNED_REFERENCE_MATCHING_CALIBRATION_MANIFEST_INVALID";
    public const string ConflictErrorCode =
        "REPOSITORY_OWNED_REFERENCE_MATCHING_CALIBRATION_MANIFEST_CONFLICT";
    public const string UnavailableErrorCode =
        "REPOSITORY_OWNED_REFERENCE_MATCHING_CALIBRATION_MANIFEST_UNAVAILABLE";

    public const string CurrentSchema =
        "repository-owned-reference-matching-calibration-manifest-v1";
    public const int CurrentSchemaVersion = 1;
    public const string CurrentContext =
        "continuous-learning-reference-matching-calibration-v1";

    private const string ManifestTempPrefix = ".calibration-manifest.";
    private const string TempSuffix = ".tmp";
    private const int FileBufferSize = 81920;
    private const int HashHexLength = 64;
    private const int ManifestJsonMaxDepth = 8;
    private const int MaximumIdentityTextLength = 256;
    private const int MaximumHumanDescriptionTextLength = 4096;
    private const int MaximumRepositoryRelativePathLength = 1024;
    private const int MaximumPairCount = 10_000;
    private const long MaximumManifestBytes = 2L * 1024L * 1024L;
    private const UnixFileMode OwnerDirectoryMode =
        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;

    private const string SchemaProperty = "schema";
    private const string SchemaVersionProperty = "schemaVersion";
    private const string ContextProperty = "context";
    private const string RepositoryRelativeCalibrationDirectoryProperty =
        "repositoryRelativeCalibrationDirectory";
    private const string RepositoryRelativeManifestPathProperty =
        "repositoryRelativeManifestPath";
    private const string RepositoryRelativeLinkedCorpusManifestPathProperty =
        "repositoryRelativeLinkedCorpusManifestPath";
    private const string LinkedCorpusManifestSha256Property =
        "linkedCorpusManifestSha256";
    private const string DomainProperty = "domain";
    private const string SubjectProperty = "subject";
    private const string CorpusVersionProperty = "corpusVersion";
    private const string CorpusSha256Property = "corpusSha256";
    private const string CorpusEntryCountProperty = "corpusEntryCount";
    private const string RuleDomainProperty = "ruleDomain";
    private const string RuleIdProperty = "ruleId";
    private const string RuleVersionProperty = "ruleVersion";
    private const string RuleHumanDescriptionProperty = "ruleHumanDescription";
    private const string RuleIdentitySha256Property = "ruleIdentitySha256";
    private const string CalibrationSetSha256Property = "calibrationSetSha256";
    private const string KnownPositiveCountProperty = "knownPositiveCount";
    private const string KnownPositivePairIdsProperty = "knownPositivePairIds";
    private const string KnownNegativeCountProperty = "knownNegativeCount";
    private const string KnownNegativePairIdsProperty = "knownNegativePairIds";
    private const string FalseMissCountProperty = "falseMissCount";
    private const string FalseMissRateProperty = "falseMissRate";
    private const string FalseMissPairIdsProperty = "falseMissPairIds";
    private const string FalseHitCountProperty = "falseHitCount";
    private const string FalseHitRateProperty = "falseHitRate";
    private const string FalseHitPairIdsProperty = "falseHitPairIds";
    private const string ResultSha256Property = "resultSha256";
    private const string ManifestSha256Property = "manifestSha256";

    /// <summary>
    /// Alias for callers that name the operation as sealing rather than persisting.
    /// </summary>
    public static OperationResult<RepositoryOwnedReferenceMatchingCalibrationManifest>
        Seal(
            RepositoryOwnedReferenceMatchingCalibrationAddress address,
            ReferenceMatchingCalibrationInput input,
            ReferenceMatchingRuleCalibrationResult result)
        => PersistAndSeal(address, input, result);

    /// <summary>
    /// Evaluates a domain rule and stores the resulting calibration manifest in
    /// the repository-owned location derived from the already sealed corpus.
    /// </summary>
    public static OperationResult<RepositoryOwnedReferenceMatchingCalibrationManifest>
        EvaluatePersistAndSeal(
            string repositoryRoot,
            string subjectSlug,
            ReferenceMatchingCalibrationInput input,
            ReferenceMatchingRulePredicate matchingRule)
    {
        if (input is null)
        {
            return Invalid<
                RepositoryOwnedReferenceMatchingCalibrationManifest>();
        }

        if (matchingRule is null)
        {
            return Invalid<
                RepositoryOwnedReferenceMatchingCalibrationManifest>();
        }

        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateReferenceMatchingCalibrationAddress(
                repositoryRoot,
                input.Corpus.DomainTag,
                subjectSlug,
                input.Corpus.CorpusVersion,
                input.RuleIdentity.RuleId,
                input.RuleIdentity.IdentitySha256,
                input.CalibrationSetSha256);
        if (!address.IsSuccess || address.Value is null)
        {
            return address
                .MapError<RepositoryOwnedReferenceMatchingCalibrationManifest>();
        }

        var inputValidation = ValidateAddressAndInputBeforeEvaluation(
            address.Value,
            input);
        if (!inputValidation.IsSuccess || inputValidation.Value is null)
        {
            return inputValidation
                .MapError<RepositoryOwnedReferenceMatchingCalibrationManifest>();
        }

        // WHY: the destination and linked sealed corpus must be proven before
        // the caller predicate runs. A wrong repo, wrong subject, missing corpus
        // or parallel path therefore cannot execute domain code or create a
        // calibration tree.
        var result = ReferenceMatchingRuleCalibrationEvaluator.Evaluate(
            input,
            matchingRule);
        if (!result.IsSuccess || result.Value is null)
        {
            return result
                .MapError<RepositoryOwnedReferenceMatchingCalibrationManifest>();
        }

        // The caller supplies only the subject slug. Everything else is bound to
        // already validated identities, so a domain worker cannot accidentally
        // write calibration numbers under a parallel path or a stale rule hash.
        return PersistAndSeal(address.Value, input, result.Value);
    }

    /// <summary>
    /// Persists a manifest-only calibration snapshot and then reopens it.
    /// </summary>
    public static OperationResult<RepositoryOwnedReferenceMatchingCalibrationManifest>
        PersistAndSeal(
            RepositoryOwnedReferenceMatchingCalibrationAddress address,
            ReferenceMatchingCalibrationInput input,
            ReferenceMatchingRuleCalibrationResult result)
    {
        var validation = ValidateAddressInputAndResult(address, input, result);
        if (!validation.IsSuccess || validation.Value is null)
            return validation
                .MapError<RepositoryOwnedReferenceMatchingCalibrationManifest>();

        try
        {
            var custodyContext = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
                .PreparePublicContextForStrictReaders(address.RepositoryRoot);
            if (!custodyContext.IsSuccess
                || custodyContext.Value is null
                || !custodyContext.Value.CoversReferenceCorpusRoot(
                    address.RepositoryRoot))
            {
                return Unavailable<
                    RepositoryOwnedReferenceMatchingCalibrationManifest>();
            }

            var rootReady = EnsureWritableCalibrationRoot(address);
            if (!rootReady.IsSuccess)
            {
                return rootReady
                    .MapError<RepositoryOwnedReferenceMatchingCalibrationManifest>();
            }

            var revalidation = ValidateAddressInputAndResult(
                address,
                input,
                result);
            if (!revalidation.IsSuccess || revalidation.Value is null)
            {
                return revalidation
                    .MapError<RepositoryOwnedReferenceMatchingCalibrationManifest>();
            }

            return PersistAndSealCore(
                address,
                input,
                result,
                revalidation.Value);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<
                RepositoryOwnedReferenceMatchingCalibrationManifest>();
        }
    }

    /// <summary>
    /// Alias for callers that name the operation as a strict open.
    /// </summary>
    public static OperationResult<RepositoryOwnedReferenceMatchingCalibrationOpenResult>
        Open(RepositoryOwnedReferenceMatchingCalibrationAddress address)
        => OpenSealed(address);

    /// <summary>
    /// Reopens the calibration manifest and the sealed frozen corpus it names.
    /// </summary>
    public static OperationResult<RepositoryOwnedReferenceMatchingCalibrationOpenResult>
        OpenSealed(RepositoryOwnedReferenceMatchingCalibrationAddress address)
    {
        var validation = ValidateAddress(address);
        if (!validation.IsSuccess)
        {
            return validation
                .MapError<RepositoryOwnedReferenceMatchingCalibrationOpenResult>();
        }

        var custodyContext = RepositoryOwnedTrainingArtifactCloneCustodyPreparer
            .PreparePublicContextForStrictReaders(address.RepositoryRoot);
        if (!custodyContext.IsSuccess
            || custodyContext.Value is null
            || !custodyContext.Value.CoversReferenceCorpusRoot(
                address.RepositoryRoot))
        {
            return Unavailable<
                RepositoryOwnedReferenceMatchingCalibrationOpenResult>();
        }

        return OpenSealedCore(address);
    }

    private static OperationResult<RepositoryOwnedReferenceMatchingCalibrationManifest>
        PersistAndSealCore(
            RepositoryOwnedReferenceMatchingCalibrationAddress address,
            ReferenceMatchingCalibrationInput input,
            ReferenceMatchingRuleCalibrationResult result,
            RepositoryOwnedReferenceCorpusOpenResult linkedCorpus)
    {
        var manifest = BuildManifest(address, input, result, linkedCorpus);
        var manifestBytes = WriteCanonicalManifestBytes(manifest);

        if (File.Exists(address.ManifestPath))
        {
            var existing = OpenSealedCore(address);
            if (!existing.IsSuccess || existing.Value is null)
            {
                return existing
                    .MapError<RepositoryOwnedReferenceMatchingCalibrationManifest>();
            }

            // WHY: the calibration address embeds the rule and set hashes.
            // Re-running the exact same calibration is safe; changing any bound
            // number at the same address would rewrite history and must fail.
            return ManifestsEqual(existing.Value.Manifest, manifest)
                   && CanonicalBytesEqual(
                       WriteCanonicalManifestBytes(existing.Value.Manifest),
                       manifestBytes)
                ? OperationResult<
                    RepositoryOwnedReferenceMatchingCalibrationManifest>.Ok(
                    existing.Value.Manifest)
                : Conflict<
                    RepositoryOwnedReferenceMatchingCalibrationManifest>();
        }

        var published = PublishBytesWithoutOverwrite(
            address,
            manifestBytes);
        if (!published.IsSuccess)
        {
            return published
                .MapError<RepositoryOwnedReferenceMatchingCalibrationManifest>();
        }

        var opened = OpenSealedCore(address);
        return opened.IsSuccess && opened.Value is not null
            ? OperationResult<
                RepositoryOwnedReferenceMatchingCalibrationManifest>.Ok(
                opened.Value.Manifest)
            : opened
                .MapError<RepositoryOwnedReferenceMatchingCalibrationManifest>();
    }

    private static OperationResult<RepositoryOwnedReferenceMatchingCalibrationOpenResult>
        OpenSealedCore(
            RepositoryOwnedReferenceMatchingCalibrationAddress address)
    {
        var validation = ValidateAddress(address);
        if (!validation.IsSuccess)
        {
            return validation
                .MapError<RepositoryOwnedReferenceMatchingCalibrationOpenResult>();
        }

        try
        {
            if (!File.Exists(address.ManifestPath))
                return Invalid<
                    RepositoryOwnedReferenceMatchingCalibrationOpenResult>();

            var linkedCorpus = OpenLinkedCorpus(address);
            if (!linkedCorpus.IsSuccess || linkedCorpus.Value is null)
            {
                return Invalid<
                    RepositoryOwnedReferenceMatchingCalibrationOpenResult>();
            }

            var manifest = ReadStrictManifest(address.ManifestPath);
            if (!manifest.IsSuccess || manifest.Value is null)
            {
                return manifest
                    .MapError<
                        RepositoryOwnedReferenceMatchingCalibrationOpenResult>();
            }

            var manifestValidation = ValidateManifestForAddressAndLinkedCorpus(
                address,
                manifest.Value,
                linkedCorpus.Value);
            if (!manifestValidation.IsSuccess)
            {
                return manifestValidation
                    .MapError<
                        RepositoryOwnedReferenceMatchingCalibrationOpenResult>();
            }

            return OperationResult<
                RepositoryOwnedReferenceMatchingCalibrationOpenResult>.Ok(
                new RepositoryOwnedReferenceMatchingCalibrationOpenResult(
                    manifest.Value,
                    linkedCorpus.Value));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<
                RepositoryOwnedReferenceMatchingCalibrationOpenResult>();
        }
    }

    private static OperationResult<RepositoryOwnedReferenceCorpusOpenResult>
        ValidateAddressInputAndResult(
            RepositoryOwnedReferenceMatchingCalibrationAddress address,
            ReferenceMatchingCalibrationInput input,
            ReferenceMatchingRuleCalibrationResult result)
    {
        var addressValidation = ValidateAddress(address);
        if (!addressValidation.IsSuccess)
        {
            return addressValidation
                .MapError<RepositoryOwnedReferenceCorpusOpenResult>();
        }

        if (input is null || result is null)
            return Invalid<RepositoryOwnedReferenceCorpusOpenResult>();

        var inputValidation = ValidateAddressAndInputBeforeEvaluation(
            address,
            input);
        if (!inputValidation.IsSuccess || inputValidation.Value is null)
        {
            return inputValidation
                .MapError<RepositoryOwnedReferenceCorpusOpenResult>();
        }

        if (!ResultMatchesInput(input, result))
        {
            return Invalid<RepositoryOwnedReferenceCorpusOpenResult>();
        }

        return inputValidation;
    }

    private static OperationResult<RepositoryOwnedReferenceCorpusOpenResult>
        ValidateAddressAndInputBeforeEvaluation(
            RepositoryOwnedReferenceMatchingCalibrationAddress address,
            ReferenceMatchingCalibrationInput input)
    {
        var addressValidation = ValidateAddress(address);
        if (!addressValidation.IsSuccess)
        {
            return addressValidation
                .MapError<RepositoryOwnedReferenceCorpusOpenResult>();
        }

        if (input is null)
            return Invalid<RepositoryOwnedReferenceCorpusOpenResult>();

        var linkedCorpus = OpenLinkedCorpus(address);
        if (!linkedCorpus.IsSuccess || linkedCorpus.Value is null)
            return Invalid<RepositoryOwnedReferenceCorpusOpenResult>();

        return CorpusMatches(linkedCorpus.Value.Corpus, input.Corpus)
               && AddressMatchesInput(address, input)
            ? linkedCorpus
            : Invalid<RepositoryOwnedReferenceCorpusOpenResult>();
    }

    private static OperationResult<bool> ValidateAddress(
        RepositoryOwnedReferenceMatchingCalibrationAddress address)
    {
        if (address is null
            || !RepositoryOwnedTrainingArtifactLayout
                .IsValidReferenceMatchingCalibrationAddress(address)
            || AnyExistingPathSegmentIsReparsePoint(address.CalibrationRoot)
            || AnyExistingPathSegmentIsReparsePoint(address.ManifestPath)
            || Directory.Exists(address.ManifestPath))
        {
            return Invalid<bool>();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<RepositoryOwnedReferenceCorpusOpenResult>
        OpenLinkedCorpus(
            RepositoryOwnedReferenceMatchingCalibrationAddress address)
    {
        var linkedAddress = RepositoryOwnedTrainingArtifactLayout
            .CreateReferenceCorpusAddress(
                address.RepositoryRoot,
                address.DomainSlug,
                address.SubjectSlug,
                address.CorpusVersion);
        if (!linkedAddress.IsSuccess || linkedAddress.Value is null)
            return Invalid<RepositoryOwnedReferenceCorpusOpenResult>();

        var opened = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(
            linkedAddress.Value);
        return opened.IsSuccess && opened.Value is not null
            ? opened
            : Invalid<RepositoryOwnedReferenceCorpusOpenResult>();
    }

    private static bool AddressMatchesInput(
        RepositoryOwnedReferenceMatchingCalibrationAddress address,
        ReferenceMatchingCalibrationInput input)
        => input.Corpus is not null
           && input.RuleIdentity is not null
           && StringComparer.Ordinal.Equals(address.DomainSlug, input.Corpus.DomainTag)
           && StringComparer.Ordinal.Equals(address.SubjectSlug, input.Corpus.SubjectId)
           && StringComparer.Ordinal.Equals(
               address.CorpusVersion,
               input.Corpus.CorpusVersion)
           && StringComparer.Ordinal.Equals(
               address.RuleIdSlug,
               input.RuleIdentity.RuleId)
           && StringComparer.Ordinal.Equals(
               address.RuleIdentitySha256,
               input.RuleIdentity.IdentitySha256)
           && StringComparer.Ordinal.Equals(
               address.CalibrationSetSha256,
               input.CalibrationSetSha256);

    private static bool ResultMatchesInput(
        ReferenceMatchingCalibrationInput input,
        ReferenceMatchingRuleCalibrationResult result)
    {
        if (result.RuleIdentity is null
            || !RuleIdentityMatches(input.RuleIdentity, result.RuleIdentity)
            || !StringComparer.Ordinal.Equals(
                result.CorpusSha256,
                input.Corpus.CorpusSha256)
            || result.CorpusEntryCount != input.Corpus.Count
            || !StringComparer.Ordinal.Equals(
                result.CalibrationSetSha256,
                input.CalibrationSetSha256)
            || result.KnownPositiveCount != input.KnownPositivePairs.Count
            || result.KnownNegativeCount != input.KnownNegativePairs.Count)
        {
            return false;
        }

        var positivePairIds = PairIds(input.KnownPositivePairs);
        var negativePairIds = PairIds(input.KnownNegativePairs);
        var falseMissIds = SnapshotIds(result.FalseMissPairIds);
        var falseHitIds = SnapshotIds(result.FalseHitPairIds);
        if (falseMissIds is null
            || falseHitIds is null
            || !IdsOrderedAndUnique(positivePairIds)
            || !IdsOrderedAndUnique(negativePairIds)
            || !IdsOrderedAndUnique(falseMissIds)
            || !IdsOrderedAndUnique(falseHitIds)
            || falseMissIds.Count != result.FalseMissCount
            || falseHitIds.Count != result.FalseHitCount
            || result.FalseMissCount < 0
            || result.FalseHitCount < 0
            || result.FalseMissCount > result.KnownPositiveCount
            || result.FalseHitCount > result.KnownNegativeCount
            || !IdsBelongTo(falseMissIds, positivePairIds)
            || !IdsBelongTo(falseHitIds, negativePairIds)
            || !RateMatches(result.FalseMissRate, result.FalseMissCount, result.KnownPositiveCount)
            || !RateMatches(result.FalseHitRate, result.FalseHitCount, result.KnownNegativeCount))
        {
            return false;
        }

        return FrozenReferenceCanonicalization.IsCanonicalSha256Ref(
                   result.ResultSha256)
               && StringComparer.Ordinal.Equals(
                   ComputeResultSha256(
                       result.CorpusSha256,
                       result.CorpusEntryCount,
                       result.RuleIdentity.IdentitySha256,
                       result.CalibrationSetSha256,
                       result.KnownPositiveCount,
                       positivePairIds,
                       result.FalseMissCount,
                       result.FalseMissRate,
                       falseMissIds,
                       result.KnownNegativeCount,
                       negativePairIds,
                       result.FalseHitCount,
                       result.FalseHitRate,
                       falseHitIds),
                   result.ResultSha256);
    }

    private static OperationResult<bool> ValidateManifestForAddressAndLinkedCorpus(
        RepositoryOwnedReferenceMatchingCalibrationAddress address,
        RepositoryOwnedReferenceMatchingCalibrationManifest manifest,
        RepositoryOwnedReferenceCorpusOpenResult linkedCorpus)
    {
        if (!StringComparer.Ordinal.Equals(manifest.Schema, CurrentSchema)
            || manifest.SchemaVersion != CurrentSchemaVersion
            || !StringComparer.Ordinal.Equals(manifest.Context, CurrentContext)
            || !StringComparer.Ordinal.Equals(
                manifest.RepositoryRelativeCalibrationDirectory,
                address.CalibrationRootRelativePath)
            || !StringComparer.Ordinal.Equals(
                manifest.RepositoryRelativeManifestPath,
                address.ManifestRelativePath)
            || !StringComparer.Ordinal.Equals(
                manifest.RepositoryRelativeLinkedCorpusManifestPath,
                linkedCorpus.Manifest.RepositoryRelativeManifestPath)
            || !StringComparer.Ordinal.Equals(
                manifest.LinkedCorpusManifestSha256,
                linkedCorpus.Manifest.ManifestSha256)
            || !StringComparer.Ordinal.Equals(manifest.Domain, address.DomainSlug)
            || !StringComparer.Ordinal.Equals(manifest.Subject, address.SubjectSlug)
            || !StringComparer.Ordinal.Equals(
                manifest.CorpusVersion,
                address.CorpusVersion)
            || !StringComparer.Ordinal.Equals(
                manifest.CorpusSha256,
                linkedCorpus.Corpus.CorpusSha256)
            || manifest.CorpusEntryCount != linkedCorpus.Corpus.Count
            || !StringComparer.Ordinal.Equals(
                manifest.RuleDomain,
                manifest.Domain)
            || !StringComparer.Ordinal.Equals(
                manifest.RuleId,
                address.RuleIdSlug)
            || !StringComparer.Ordinal.Equals(
                manifest.RuleIdentitySha256,
                address.RuleIdentitySha256)
            || !StringComparer.Ordinal.Equals(
                manifest.CalibrationSetSha256,
                address.CalibrationSetSha256)
            || !FrozenReferenceCanonicalization.NormalizeIdentity(
                manifest.RuleDomain).IsSuccess
            || !FrozenReferenceCanonicalization.NormalizeIdentity(
                manifest.RuleId).IsSuccess
            || !FrozenReferenceCanonicalization.NormalizeIdentity(
                manifest.RuleVersion).IsSuccess
            || !FrozenReferenceCanonicalization.NormalizeHumanDescription(
                manifest.RuleHumanDescription).IsSuccess
            || !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(
                manifest.CorpusSha256)
            || !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(
                manifest.RuleIdentitySha256)
            || !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(
                manifest.CalibrationSetSha256)
            || !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(
                manifest.ResultSha256)
            || !IsCanonicalSha256Hex(manifest.LinkedCorpusManifestSha256)
            || !IsCanonicalSha256Hex(manifest.ManifestSha256))
        {
            return Invalid<bool>();
        }

        var normalizedRuleDescription =
            FrozenReferenceCanonicalization.NormalizeHumanDescription(
                manifest.RuleHumanDescription);
        if (!normalizedRuleDescription.IsSuccess
            || normalizedRuleDescription.Value is null
            || !StringComparer.Ordinal.Equals(
                normalizedRuleDescription.Value,
                manifest.RuleHumanDescription))
        {
            return Invalid<bool>();
        }

        var ruleIdentity = ReferenceMatchingRuleIdentity.Create(
            manifest.RuleDomain,
            manifest.RuleId,
            manifest.RuleVersion,
            manifest.RuleHumanDescription);
        if (!ruleIdentity.IsSuccess
            || ruleIdentity.Value is null
            || !StringComparer.Ordinal.Equals(
                ruleIdentity.Value.DomainTag,
                manifest.RuleDomain)
            || !StringComparer.Ordinal.Equals(
                ruleIdentity.Value.RuleId,
                manifest.RuleId)
            || !StringComparer.Ordinal.Equals(
                ruleIdentity.Value.RuleVersion,
                manifest.RuleVersion)
            || !StringComparer.Ordinal.Equals(
                ruleIdentity.Value.HumanDescription,
                manifest.RuleHumanDescription)
            || !StringComparer.Ordinal.Equals(
                ruleIdentity.Value.IdentitySha256,
                manifest.RuleIdentitySha256))
        {
            return Invalid<bool>();
        }

        if (!CountsRatesAndIdsAreConsistent(manifest))
            return Invalid<bool>();

        return OperationResult<bool>.Ok(true);
    }

    private static bool CountsRatesAndIdsAreConsistent(
        RepositoryOwnedReferenceMatchingCalibrationManifest manifest)
    {
        if (manifest.KnownPositiveCount <= 0
            || manifest.KnownNegativeCount <= 0
            || manifest.KnownPositiveCount > MaximumPairCount
            || manifest.KnownNegativeCount > MaximumPairCount
            || manifest.KnownPositivePairIds.Count != manifest.KnownPositiveCount
            || manifest.KnownNegativePairIds.Count != manifest.KnownNegativeCount
            || manifest.FalseMissPairIds.Count != manifest.FalseMissCount
            || manifest.FalseHitPairIds.Count != manifest.FalseHitCount
            || manifest.FalseMissCount < 0
            || manifest.FalseHitCount < 0
            || manifest.FalseMissCount > manifest.KnownPositiveCount
            || manifest.FalseHitCount > manifest.KnownNegativeCount
            || !IdsOrderedAndUnique(manifest.KnownPositivePairIds)
            || !IdsOrderedAndUnique(manifest.KnownNegativePairIds)
            || !IdsOrderedAndUnique(manifest.FalseMissPairIds)
            || !IdsOrderedAndUnique(manifest.FalseHitPairIds)
            || IdsIntersect(
                manifest.KnownPositivePairIds,
                manifest.KnownNegativePairIds)
            || !IdsBelongTo(
                manifest.FalseMissPairIds,
                manifest.KnownPositivePairIds)
            || !IdsBelongTo(
                manifest.FalseHitPairIds,
                manifest.KnownNegativePairIds)
            || !RateMatches(
                manifest.FalseMissRate,
                manifest.FalseMissCount,
                manifest.KnownPositiveCount)
            || !RateMatches(
                manifest.FalseHitRate,
                manifest.FalseHitCount,
                manifest.KnownNegativeCount))
        {
            return false;
        }

        return StringComparer.Ordinal.Equals(
            ComputeResultSha256(
                manifest.CorpusSha256,
                manifest.CorpusEntryCount,
                manifest.RuleIdentitySha256,
                manifest.CalibrationSetSha256,
                manifest.KnownPositiveCount,
                manifest.KnownPositivePairIds,
                manifest.FalseMissCount,
                manifest.FalseMissRate,
                manifest.FalseMissPairIds,
                manifest.KnownNegativeCount,
                manifest.KnownNegativePairIds,
                manifest.FalseHitCount,
                manifest.FalseHitRate,
                manifest.FalseHitPairIds),
            manifest.ResultSha256);
    }

    private static RepositoryOwnedReferenceMatchingCalibrationManifest BuildManifest(
        RepositoryOwnedReferenceMatchingCalibrationAddress address,
        ReferenceMatchingCalibrationInput input,
        ReferenceMatchingRuleCalibrationResult result,
        RepositoryOwnedReferenceCorpusOpenResult linkedCorpus)
    {
        var unsigned = new RepositoryOwnedReferenceMatchingCalibrationManifest(
            CurrentSchema,
            CurrentSchemaVersion,
            CurrentContext,
            address.CalibrationRootRelativePath,
            address.ManifestRelativePath,
            linkedCorpus.Manifest.RepositoryRelativeManifestPath,
            linkedCorpus.Manifest.ManifestSha256,
            address.DomainSlug,
            address.SubjectSlug,
            address.CorpusVersion,
            input.Corpus.CorpusSha256,
            input.Corpus.Count,
            input.RuleIdentity.DomainTag,
            input.RuleIdentity.RuleId,
            input.RuleIdentity.RuleVersion,
            input.RuleIdentity.HumanDescription,
            input.RuleIdentity.IdentitySha256,
            input.CalibrationSetSha256,
            input.KnownPositivePairs.Count,
            PairIds(input.KnownPositivePairs),
            input.KnownNegativePairs.Count,
            PairIds(input.KnownNegativePairs),
            result.FalseMissCount,
            result.FalseMissRate,
            Array.AsReadOnly(result.FalseMissPairIds.ToArray()),
            result.FalseHitCount,
            result.FalseHitRate,
            Array.AsReadOnly(result.FalseHitPairIds.ToArray()),
            result.ResultSha256,
            string.Empty);
        return unsigned with
        {
            ManifestSha256 = ComputeSha256(WriteCanonicalManifestBytes(unsigned)),
        };
    }

    private static OperationResult<RepositoryOwnedReferenceMatchingCalibrationManifest>
        ReadStrictManifest(string manifestPath)
    {
        var bytes = ReadBoundedStableFileBytes(manifestPath, MaximumManifestBytes);
        if (!bytes.IsSuccess || bytes.Value is null)
        {
            return bytes
                .MapError<RepositoryOwnedReferenceMatchingCalibrationManifest>();
        }

        try
        {
            using var document = JsonDocument.Parse(
                bytes.Value,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = ManifestJsonMaxDepth,
                });
            if (!TryReadManifestElement(document.RootElement, out var manifest))
            {
                return Invalid<
                    RepositoryOwnedReferenceMatchingCalibrationManifest>();
            }

            var selfHash = ComputeSha256(WriteCanonicalManifestBytes(
                manifest with
                {
                    ManifestSha256 = string.Empty,
                }));
            if (!StringComparer.Ordinal.Equals(selfHash, manifest.ManifestSha256))
            {
                return Invalid<
                    RepositoryOwnedReferenceMatchingCalibrationManifest>();
            }

            return OperationResult<
                RepositoryOwnedReferenceMatchingCalibrationManifest>.Ok(
                manifest);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid<RepositoryOwnedReferenceMatchingCalibrationManifest>();
        }
    }

    private static bool TryReadManifestElement(
        JsonElement element,
        out RepositoryOwnedReferenceMatchingCalibrationManifest manifest)
    {
        manifest = EmptyManifest();
        if (!ObjectHasExactProperties(
                element,
                SchemaProperty,
                SchemaVersionProperty,
                ContextProperty,
                RepositoryRelativeCalibrationDirectoryProperty,
                RepositoryRelativeManifestPathProperty,
                RepositoryRelativeLinkedCorpusManifestPathProperty,
                LinkedCorpusManifestSha256Property,
                DomainProperty,
                SubjectProperty,
                CorpusVersionProperty,
                CorpusSha256Property,
                CorpusEntryCountProperty,
                RuleDomainProperty,
                RuleIdProperty,
                RuleVersionProperty,
                RuleHumanDescriptionProperty,
                RuleIdentitySha256Property,
                CalibrationSetSha256Property,
                KnownPositiveCountProperty,
                KnownPositivePairIdsProperty,
                KnownNegativeCountProperty,
                KnownNegativePairIdsProperty,
                FalseMissCountProperty,
                FalseMissRateProperty,
                FalseMissPairIdsProperty,
                FalseHitCountProperty,
                FalseHitRateProperty,
                FalseHitPairIdsProperty,
                ResultSha256Property,
                ManifestSha256Property)
            || !TryReadString(
                element,
                SchemaProperty,
                MaximumIdentityTextLength,
                out var schema)
            || !TryReadInt(element, SchemaVersionProperty, out var schemaVersion)
            || !TryReadString(
                element,
                ContextProperty,
                MaximumIdentityTextLength,
                out var context)
            || !TryReadString(
                element,
                RepositoryRelativeCalibrationDirectoryProperty,
                MaximumRepositoryRelativePathLength,
                out var calibrationDirectory)
            || !TryReadString(
                element,
                RepositoryRelativeManifestPathProperty,
                MaximumRepositoryRelativePathLength,
                out var manifestPath)
            || !TryReadString(
                element,
                RepositoryRelativeLinkedCorpusManifestPathProperty,
                MaximumRepositoryRelativePathLength,
                out var linkedCorpusManifestPath)
            || !TryReadString(
                element,
                LinkedCorpusManifestSha256Property,
                HashHexLength,
                out var linkedCorpusManifestSha256)
            || !TryReadString(
                element,
                DomainProperty,
                MaximumIdentityTextLength,
                out var domain)
            || !TryReadString(
                element,
                SubjectProperty,
                MaximumIdentityTextLength,
                out var subject)
            || !TryReadString(
                element,
                CorpusVersionProperty,
                MaximumIdentityTextLength,
                out var corpusVersion)
            || !TryReadString(
                element,
                CorpusSha256Property,
                "sha256:".Length + HashHexLength,
                out var corpusSha256)
            || !TryReadInt(
                element,
                CorpusEntryCountProperty,
                out var corpusEntryCount)
            || !TryReadString(
                element,
                RuleDomainProperty,
                MaximumIdentityTextLength,
                out var ruleDomain)
            || !TryReadString(
                element,
                RuleIdProperty,
                MaximumIdentityTextLength,
                out var ruleId)
            || !TryReadString(
                element,
                RuleVersionProperty,
                MaximumIdentityTextLength,
                out var ruleVersion)
            || !TryReadString(
                element,
                RuleHumanDescriptionProperty,
                MaximumHumanDescriptionTextLength,
                out var ruleHumanDescription)
            || !TryReadString(
                element,
                RuleIdentitySha256Property,
                "sha256:".Length + HashHexLength,
                out var ruleIdentitySha256)
            || !TryReadString(
                element,
                CalibrationSetSha256Property,
                "sha256:".Length + HashHexLength,
                out var calibrationSetSha256)
            || !TryReadInt(
                element,
                KnownPositiveCountProperty,
                out var knownPositiveCount)
            || !TryReadStringArray(
                element,
                KnownPositivePairIdsProperty,
                out var knownPositivePairIds)
            || !TryReadInt(
                element,
                KnownNegativeCountProperty,
                out var knownNegativeCount)
            || !TryReadStringArray(
                element,
                KnownNegativePairIdsProperty,
                out var knownNegativePairIds)
            || !TryReadInt(element, FalseMissCountProperty, out var falseMissCount)
            || !TryReadDouble(element, FalseMissRateProperty, out var falseMissRate)
            || !TryReadStringArray(
                element,
                FalseMissPairIdsProperty,
                out var falseMissPairIds)
            || !TryReadInt(element, FalseHitCountProperty, out var falseHitCount)
            || !TryReadDouble(element, FalseHitRateProperty, out var falseHitRate)
            || !TryReadStringArray(
                element,
                FalseHitPairIdsProperty,
                out var falseHitPairIds)
            || !TryReadString(
                element,
                ResultSha256Property,
                "sha256:".Length + HashHexLength,
                out var resultSha256)
            || !TryReadString(
                element,
                ManifestSha256Property,
                HashHexLength,
                out var manifestSha256))
        {
            return false;
        }

        manifest = new RepositoryOwnedReferenceMatchingCalibrationManifest(
            schema,
            schemaVersion,
            context,
            calibrationDirectory,
            manifestPath,
            linkedCorpusManifestPath,
            linkedCorpusManifestSha256,
            domain,
            subject,
            corpusVersion,
            corpusSha256,
            corpusEntryCount,
            ruleDomain,
            ruleId,
            ruleVersion,
            ruleHumanDescription,
            ruleIdentitySha256,
            calibrationSetSha256,
            knownPositiveCount,
            knownPositivePairIds,
            knownNegativeCount,
            knownNegativePairIds,
            falseMissCount,
            falseMissRate,
            falseMissPairIds,
            falseHitCount,
            falseHitRate,
            falseHitPairIds,
            resultSha256,
            manifestSha256);
        return true;
    }

    private static byte[] WriteCanonicalManifestBytes(
        RepositoryOwnedReferenceMatchingCalibrationManifest manifest)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString(SchemaProperty, manifest.Schema);
            writer.WriteNumber(SchemaVersionProperty, manifest.SchemaVersion);
            writer.WriteString(ContextProperty, manifest.Context);
            writer.WriteString(
                RepositoryRelativeCalibrationDirectoryProperty,
                manifest.RepositoryRelativeCalibrationDirectory);
            writer.WriteString(
                RepositoryRelativeManifestPathProperty,
                manifest.RepositoryRelativeManifestPath);
            writer.WriteString(
                RepositoryRelativeLinkedCorpusManifestPathProperty,
                manifest.RepositoryRelativeLinkedCorpusManifestPath);
            writer.WriteString(
                LinkedCorpusManifestSha256Property,
                manifest.LinkedCorpusManifestSha256);
            writer.WriteString(DomainProperty, manifest.Domain);
            writer.WriteString(SubjectProperty, manifest.Subject);
            writer.WriteString(CorpusVersionProperty, manifest.CorpusVersion);
            writer.WriteString(CorpusSha256Property, manifest.CorpusSha256);
            writer.WriteNumber(
                CorpusEntryCountProperty,
                manifest.CorpusEntryCount);
            writer.WriteString(RuleDomainProperty, manifest.RuleDomain);
            writer.WriteString(RuleIdProperty, manifest.RuleId);
            writer.WriteString(RuleVersionProperty, manifest.RuleVersion);
            writer.WriteString(
                RuleHumanDescriptionProperty,
                manifest.RuleHumanDescription);
            writer.WriteString(
                RuleIdentitySha256Property,
                manifest.RuleIdentitySha256);
            writer.WriteString(
                CalibrationSetSha256Property,
                manifest.CalibrationSetSha256);
            writer.WriteNumber(
                KnownPositiveCountProperty,
                manifest.KnownPositiveCount);
            WriteStringArray(
                writer,
                KnownPositivePairIdsProperty,
                manifest.KnownPositivePairIds);
            writer.WriteNumber(
                KnownNegativeCountProperty,
                manifest.KnownNegativeCount);
            WriteStringArray(
                writer,
                KnownNegativePairIdsProperty,
                manifest.KnownNegativePairIds);
            writer.WriteNumber(FalseMissCountProperty, manifest.FalseMissCount);
            writer.WriteNumber(FalseMissRateProperty, manifest.FalseMissRate);
            WriteStringArray(
                writer,
                FalseMissPairIdsProperty,
                manifest.FalseMissPairIds);
            writer.WriteNumber(FalseHitCountProperty, manifest.FalseHitCount);
            writer.WriteNumber(FalseHitRateProperty, manifest.FalseHitRate);
            WriteStringArray(
                writer,
                FalseHitPairIdsProperty,
                manifest.FalseHitPairIds);
            writer.WriteString(ResultSha256Property, manifest.ResultSha256);
            writer.WriteString(ManifestSha256Property, manifest.ManifestSha256);
            writer.WriteEndObject();
            writer.Flush();
        }

        return buffer.ToArray();
    }

    private static void WriteStringArray(
        Utf8JsonWriter writer,
        string propertyName,
        IReadOnlyList<string> values)
    {
        writer.WriteStartArray(propertyName);
        foreach (var value in values)
            writer.WriteStringValue(value);
        writer.WriteEndArray();
    }

    private static bool ObjectHasExactProperties(
        JsonElement element,
        params string[] expectedNames)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return false;

        var expected = new HashSet<string>(expectedNames, StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!expected.Contains(property.Name) || !seen.Add(property.Name))
                return false;
        }

        return seen.Count == expected.Count;
    }

    private static bool TryReadString(
        JsonElement element,
        string propertyName,
        int maximumChars,
        out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString() ?? string.Empty;
        return value.Length <= maximumChars;
    }

    private static bool TryReadInt(
        JsonElement element,
        string propertyName,
        out int value)
    {
        value = 0;
        return element.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.Number
               && property.TryGetInt32(out value);
    }

    private static bool TryReadDouble(
        JsonElement element,
        string propertyName,
        out double value)
    {
        value = 0D;
        return element.TryGetProperty(propertyName, out var property)
               && property.ValueKind == JsonValueKind.Number
               && property.TryGetDouble(out value)
               && double.IsFinite(value);
    }

    private static bool TryReadStringArray(
        JsonElement element,
        string propertyName,
        out IReadOnlyList<string> values)
    {
        values = Array.Empty<string>();
        if (!element.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        var result = new List<string>();
        foreach (var item in property.EnumerateArray())
        {
            if (result.Count >= MaximumPairCount
                || item.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            var value = item.GetString() ?? string.Empty;
            if (value.Length > MaximumIdentityTextLength)
                return false;
            result.Add(value);
        }

        values = Array.AsReadOnly(result.ToArray());
        return true;
    }

    private static OperationResult<bool> PublishBytesWithoutOverwrite(
        RepositoryOwnedReferenceMatchingCalibrationAddress address,
        byte[] bytes)
    {
        var temporaryPath = Path.Combine(
            address.CalibrationRoot,
            ManifestTempPrefix + Guid.NewGuid().ToString("N") + TempSuffix);
        var published = false;
        try
        {
            if (!IsStrictDescendant(
                    address.ManifestPath,
                    address.CalibrationRoot)
                || AnyExistingPathSegmentIsReparsePoint(address.ManifestPath)
                || Directory.Exists(address.ManifestPath))
            {
                return Unavailable<bool>();
            }

            using (var stream = SecureLocalFileBoundary.OpenOwnerOnlyFile(
                       temporaryPath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       FileOptions.WriteThrough,
                       FileBufferSize))
            {
                stream.Write(bytes);
                stream.Flush(flushToDisk: true);
                if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                        stream.SafeFileHandle,
                        temporaryPath))
                {
                    return Unavailable<bool>();
                }
            }

            File.Move(temporaryPath, address.ManifestPath);
            published = true;
            var readback = ReadBoundedStableFileBytes(
                address.ManifestPath,
                MaximumManifestBytes);
            if (!readback.IsSuccess || readback.Value is null)
                return readback.MapError<bool>();
            return CanonicalBytesEqual(readback.Value, bytes)
                ? OperationResult<bool>.Ok(true)
                : Unavailable<bool>();
        }
        catch (IOException) when (File.Exists(address.ManifestPath))
        {
            var existing = ReadBoundedStableFileBytes(
                address.ManifestPath,
                MaximumManifestBytes);
            if (!existing.IsSuccess || existing.Value is null)
                return existing.MapError<bool>();
            return CanonicalBytesEqual(existing.Value, bytes)
                ? OperationResult<bool>.Ok(false)
                : Conflict<bool>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<bool>();
        }
        finally
        {
            if (!published)
                DeleteOwnStaging(address, temporaryPath);
        }
    }

    private static OperationResult<byte[]> ReadBoundedStableFileBytes(
        string path,
        long maximumBytes)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            if (!SecureLocalFileBoundary.IsSupportedPlatform()
                || AnyExistingPathSegmentIsReparsePoint(fullPath)
                || !File.Exists(fullPath))
            {
                return Invalid<byte[]>();
            }

            using var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                FileBufferSize,
                FileOptions.SequentialScan);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    fullPath))
            {
                return Invalid<byte[]>();
            }

            var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(
                stream,
                maximumBytes);
            if (bytes is null
                || bytes.Length == 0
                || !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    fullPath)
                || AnyExistingPathSegmentIsReparsePoint(fullPath))
            {
                return Invalid<byte[]>();
            }

            return OperationResult<byte[]>.Ok(bytes);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<byte[]>();
        }
    }

    private static OperationResult<bool> EnsureWritableCalibrationRoot(
        RepositoryOwnedReferenceMatchingCalibrationAddress address)
    {
        try
        {
            if (!SecureLocalFileBoundary.IsSupportedPlatform()
                || AnyExistingPathSegmentIsReparsePoint(address.CalibrationRoot))
            {
                return Unavailable<bool>();
            }

            if (Directory.Exists(address.CalibrationRoot))
            {
                if (!HasSafeUnixDirectoryMode(address.CalibrationRoot))
                    return Unavailable<bool>();
            }
            else if (File.Exists(address.CalibrationRoot))
            {
                return Unavailable<bool>();
            }
            else if (OperatingSystem.IsWindows())
            {
                Directory.CreateDirectory(address.CalibrationRoot);
            }
            else
            {
                Directory.CreateDirectory(
                    address.CalibrationRoot,
                    OwnerDirectoryMode);
            }

            if (AnyExistingPathSegmentIsReparsePoint(address.CalibrationRoot)
                || !HasSafeUnixDirectoryMode(address.CalibrationRoot))
            {
                return Unavailable<bool>();
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Unavailable<bool>();
        }
    }

    private static bool CorpusMatches(
        FrozenReferenceCorpus left,
        FrozenReferenceCorpus right)
        => left is not null
           && right is not null
           && StringComparer.Ordinal.Equals(left.DomainTag, right.DomainTag)
           && StringComparer.Ordinal.Equals(left.SubjectId, right.SubjectId)
           && StringComparer.Ordinal.Equals(left.CorpusVersion, right.CorpusVersion)
           && left.Count == right.Count
           && StringComparer.Ordinal.Equals(left.CorpusSha256, right.CorpusSha256)
           && StringComparer.Ordinal.Equals(
               left.CanonicalRepresentation,
               right.CanonicalRepresentation)
           && left.EntryNames.SequenceEqual(right.EntryNames, StringComparer.Ordinal);

    private static bool RuleIdentityMatches(
        ReferenceMatchingRuleIdentity left,
        ReferenceMatchingRuleIdentity right)
        => left is not null
           && right is not null
           && StringComparer.Ordinal.Equals(left.DomainTag, right.DomainTag)
           && StringComparer.Ordinal.Equals(left.RuleId, right.RuleId)
           && StringComparer.Ordinal.Equals(left.RuleVersion, right.RuleVersion)
           && StringComparer.Ordinal.Equals(
               left.IdentitySha256,
               right.IdentitySha256)
           && StringComparer.Ordinal.Equals(
               left.CanonicalRepresentation,
               right.CanonicalRepresentation);

    private static IReadOnlyList<string> PairIds(
        IReadOnlyList<ReferenceMatchingCalibrationPair> pairs)
        => Array.AsReadOnly(
            pairs.Select(static pair => pair.PairId)
                .OrderBy(static value => value, StringComparer.Ordinal)
                .ToArray());

    private static IReadOnlyList<string>? SnapshotIds(
        IReadOnlyList<string> values)
        => values is null
            ? null
            : Array.AsReadOnly(values.ToArray());

    private static bool IdsOrderedAndUnique(IReadOnlyList<string> values)
    {
        if (values.Count > MaximumPairCount)
            return false;

        string? previous = null;
        foreach (var value in values)
        {
            if (!FrozenReferenceCanonicalization.NormalizeIdentity(value)
                    .IsSuccess)
            {
                return false;
            }

            if (previous is not null
                && StringComparer.Ordinal.Compare(previous, value) >= 0)
            {
                return false;
            }

            previous = value;
        }

        return true;
    }

    private static bool IdsBelongTo(
        IReadOnlyList<string> subset,
        IReadOnlyList<string> superset)
    {
        var accepted = new HashSet<string>(superset, StringComparer.Ordinal);
        return subset.All(accepted.Contains);
    }

    private static bool IdsIntersect(
        IReadOnlyList<string> left,
        IReadOnlyList<string> right)
    {
        var accepted = new HashSet<string>(left, StringComparer.Ordinal);
        return right.Any(accepted.Contains);
    }

    private static bool RateMatches(double rate, int count, int denominator)
        => denominator > 0
           && count >= 0
           && count <= denominator
           && double.IsFinite(rate)
           && rate.Equals((double)count / denominator);

    private static string ComputeResultSha256(
        string corpusSha256,
        int corpusEntryCount,
        string ruleIdentitySha256,
        string calibrationSetSha256,
        int knownPositiveCount,
        IReadOnlyList<string> knownPositivePairIds,
        int falseMissCount,
        double falseMissRate,
        IReadOnlyList<string> falseMissPairIds,
        int knownNegativeCount,
        IReadOnlyList<string> knownNegativePairIds,
        int falseHitCount,
        double falseHitRate,
        IReadOnlyList<string> falseHitPairIds)
    {
        var canonical = new StringBuilder();
        FrozenReferenceCanonicalization.Append(
            canonical,
            "schema",
            "xiigen.reference-matching-calibration-result.v2");
        FrozenReferenceCanonicalization.Append(
            canonical,
            "corpus_sha256",
            corpusSha256);
        FrozenReferenceCanonicalization.Append(
            canonical,
            "corpus_entry_count",
            corpusEntryCount.ToString(CultureInfo.InvariantCulture));
        FrozenReferenceCanonicalization.Append(
            canonical,
            "rule_identity_sha256",
            ruleIdentitySha256);
        FrozenReferenceCanonicalization.Append(
            canonical,
            "calibration_set_sha256",
            calibrationSetSha256);
        FrozenReferenceCanonicalization.Append(
            canonical,
            "known_positive_count",
            knownPositiveCount.ToString(CultureInfo.InvariantCulture));
        AppendIds(canonical, "known_positive", knownPositivePairIds);
        FrozenReferenceCanonicalization.Append(
            canonical,
            "false_miss_count",
            falseMissCount.ToString(CultureInfo.InvariantCulture));
        FrozenReferenceCanonicalization.Append(
            canonical,
            "false_miss_rate",
            falseMissRate.ToString("R", CultureInfo.InvariantCulture));
        AppendIds(canonical, "false_miss", falseMissPairIds);
        FrozenReferenceCanonicalization.Append(
            canonical,
            "known_negative_count",
            knownNegativeCount.ToString(CultureInfo.InvariantCulture));
        AppendIds(canonical, "known_negative", knownNegativePairIds);
        FrozenReferenceCanonicalization.Append(
            canonical,
            "false_hit_count",
            falseHitCount.ToString(CultureInfo.InvariantCulture));
        FrozenReferenceCanonicalization.Append(
            canonical,
            "false_hit_rate",
            falseHitRate.ToString("R", CultureInfo.InvariantCulture));
        AppendIds(canonical, "false_hit", falseHitPairIds);
        return FrozenReferenceCanonicalization.Sha256Ref(canonical.ToString());
    }

    private static void AppendIds(
        StringBuilder canonical,
        string prefix,
        IReadOnlyList<string> ids)
    {
        FrozenReferenceCanonicalization.Append(
            canonical,
            prefix + ".count",
            ids.Count.ToString(CultureInfo.InvariantCulture));
        for (var index = 0; index < ids.Count; index++)
        {
            FrozenReferenceCanonicalization.Append(
                canonical,
                prefix + "." + index.ToString(CultureInfo.InvariantCulture),
                ids[index]);
        }
    }

    private static string ComputeSha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static bool IsCanonicalSha256Hex(string? value)
        => value is { Length: HashHexLength }
           && value.All(static character => character is >= '0' and <= '9'
               or >= 'a' and <= 'f');

    private static bool CanonicalBytesEqual(byte[] left, byte[] right)
        => left.Length == right.Length
           && CryptographicOperations.FixedTimeEquals(left, right);

    private static bool ManifestsEqual(
        RepositoryOwnedReferenceMatchingCalibrationManifest left,
        RepositoryOwnedReferenceMatchingCalibrationManifest right)
        => StringComparer.Ordinal.Equals(left.Schema, right.Schema)
           && left.SchemaVersion == right.SchemaVersion
           && StringComparer.Ordinal.Equals(left.Context, right.Context)
           && StringComparer.Ordinal.Equals(
               left.RepositoryRelativeCalibrationDirectory,
               right.RepositoryRelativeCalibrationDirectory)
           && StringComparer.Ordinal.Equals(
               left.RepositoryRelativeManifestPath,
               right.RepositoryRelativeManifestPath)
           && StringComparer.Ordinal.Equals(
               left.RepositoryRelativeLinkedCorpusManifestPath,
               right.RepositoryRelativeLinkedCorpusManifestPath)
           && StringComparer.Ordinal.Equals(
               left.LinkedCorpusManifestSha256,
               right.LinkedCorpusManifestSha256)
           && StringComparer.Ordinal.Equals(left.Domain, right.Domain)
           && StringComparer.Ordinal.Equals(left.Subject, right.Subject)
           && StringComparer.Ordinal.Equals(
               left.CorpusVersion,
               right.CorpusVersion)
           && StringComparer.Ordinal.Equals(left.CorpusSha256, right.CorpusSha256)
           && left.CorpusEntryCount == right.CorpusEntryCount
           && StringComparer.Ordinal.Equals(left.RuleDomain, right.RuleDomain)
           && StringComparer.Ordinal.Equals(left.RuleId, right.RuleId)
           && StringComparer.Ordinal.Equals(left.RuleVersion, right.RuleVersion)
           && StringComparer.Ordinal.Equals(
               left.RuleHumanDescription,
               right.RuleHumanDescription)
           && StringComparer.Ordinal.Equals(
               left.RuleIdentitySha256,
               right.RuleIdentitySha256)
           && StringComparer.Ordinal.Equals(
               left.CalibrationSetSha256,
               right.CalibrationSetSha256)
           && left.KnownPositiveCount == right.KnownPositiveCount
           && left.KnownPositivePairIds.SequenceEqual(
               right.KnownPositivePairIds,
               StringComparer.Ordinal)
           && left.KnownNegativeCount == right.KnownNegativeCount
           && left.KnownNegativePairIds.SequenceEqual(
               right.KnownNegativePairIds,
               StringComparer.Ordinal)
           && left.FalseMissCount == right.FalseMissCount
           && left.FalseMissRate.Equals(right.FalseMissRate)
           && left.FalseMissPairIds.SequenceEqual(
               right.FalseMissPairIds,
               StringComparer.Ordinal)
           && left.FalseHitCount == right.FalseHitCount
           && left.FalseHitRate.Equals(right.FalseHitRate)
           && left.FalseHitPairIds.SequenceEqual(
               right.FalseHitPairIds,
               StringComparer.Ordinal)
           && StringComparer.Ordinal.Equals(left.ResultSha256, right.ResultSha256)
           && StringComparer.Ordinal.Equals(
               left.ManifestSha256,
               right.ManifestSha256);

    private static bool IsStrictDescendant(string candidate, string root)
    {
        var normalizedCandidate = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(candidate));
        var normalizedRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(root));
        if (PathsEqual(normalizedCandidate, normalizedRoot))
            return false;
        var prefix = normalizedRoot + Path.DirectorySeparatorChar;
        return normalizedCandidate.StartsWith(
            prefix,
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);
    }

    private static bool PathsEqual(string first, string second)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(first)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(second)),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    private static bool HasSafeUnixDirectoryMode(string path)
        => SecureLocalFileBoundary.HasOwnerOnlyDirectoryMode(path);

    private static bool AnyExistingPathSegmentIsReparsePoint(string fullPath)
        => SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(fullPath);

    private static void DeleteOwnStaging(
        RepositoryOwnedReferenceMatchingCalibrationAddress address,
        string temporaryPath)
    {
        try
        {
            var fullPath = Path.GetFullPath(temporaryPath);
            var fileName = Path.GetFileName(fullPath);
            if (PathsEqual(
                    Path.GetDirectoryName(fullPath)!,
                    address.CalibrationRoot)
                && fileName.StartsWith(
                    ManifestTempPrefix,
                    StringComparison.Ordinal)
                && fileName.EndsWith(TempSuffix, StringComparison.Ordinal)
                && !AnyExistingPathSegmentIsReparsePoint(fullPath)
                && File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            // Cleanup is limited to this calibration directory and own temp prefix.
        }
    }

    private static RepositoryOwnedReferenceMatchingCalibrationManifest
        EmptyManifest()
        => new(
            string.Empty,
            0,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            0,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            0,
            Array.Empty<string>(),
            0,
            Array.Empty<string>(),
            0,
            0D,
            Array.Empty<string>(),
            0,
            0D,
            Array.Empty<string>(),
            string.Empty,
            string.Empty);

    private static OperationResult<T> Invalid<T>()
        => OperationResult<T>.Fail(
            "Repository-owned reference matching calibration manifest is invalid.",
            InvalidErrorCode);

    private static OperationResult<T> Conflict<T>()
        => OperationResult<T>.Fail(
            "Repository-owned reference matching calibration already seals different content.",
            ConflictErrorCode);

    private static OperationResult<T> Unavailable<T>()
        => OperationResult<T>.Fail(
            "Repository-owned reference matching calibration storage is unavailable.",
            UnavailableErrorCode);
}
