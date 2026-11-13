// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Globalization;
using System.Text;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

internal enum CertificationHistoryCompleteness { Unknown, Complete, AttestedEmpty }
internal enum CertificationLineageKind { Unknown, KnownRoot, Derived }

[Flags]
internal enum CertificationExposureRole
{
    None = 0,
    ReferenceAcquisitionOnly = 1,
    Training = 2,
    TeacherVisibleDiagnostic = 4,
    AuthoringOrTeaching = 8,
    Unknown = 16
}

internal sealed class CertificationHistoryCoverage(
    CertificationHistoryCompleteness training,
    CertificationHistoryCompleteness diagnostic,
    CertificationHistoryCompleteness teaching)
{
    internal CertificationHistoryCompleteness Training { get; } = training;
    internal CertificationHistoryCompleteness Diagnostic { get; } = diagnostic;
    internal CertificationHistoryCompleteness Teaching { get; } = teaching;
}

internal sealed class CertificationOwnerSnapshot(string id, string version)
{
    internal string Id { get; } = id;
    internal string Version { get; } = version;
}

internal sealed class CertificationSourceBinding(
    string entryId, string sourceEntityFingerprint, string ownerSnapshotId, string ownerSnapshotVersion)
{
    internal string EntryId { get; } = entryId;
    internal string SourceEntityFingerprint { get; } = sourceEntityFingerprint;
    internal string OwnerSnapshotId { get; } = ownerSnapshotId;
    internal string OwnerSnapshotVersion { get; } = ownerSnapshotVersion;
}

internal sealed class CodeDocumentationCertificationSource
{
    internal CodeDocumentationCertificationSource(
        string sourceEntityFingerprint, string domain, string subject,
        string ownerSnapshotId, string ownerSnapshotVersion, string sourceInput,
        string originEvidenceRef, CertificationExposureRole roles,
        CertificationLineageKind lineageKind, IEnumerable<string>? ancestors)
    {
        SourceEntityFingerprint = sourceEntityFingerprint;
        Domain = domain;
        Subject = subject;
        OwnerSnapshotId = ownerSnapshotId;
        OwnerSnapshotVersion = ownerSnapshotVersion;
        SourceInput = sourceInput;
        OriginEvidenceRef = originEvidenceRef;
        Roles = roles;
        LineageKind = lineageKind;
        Ancestors = CertificationSnapshot.Copy(ancestors, CertificationSnapshot.MaximumEdges, out var valid);
        SnapshotValid = valid;
    }

    internal string SourceEntityFingerprint { get; }
    internal string Domain { get; }
    internal string Subject { get; }
    internal string OwnerSnapshotId { get; }
    internal string OwnerSnapshotVersion { get; }
    internal string SourceInput { get; }
    internal string OriginEvidenceRef { get; }
    internal CertificationExposureRole Roles { get; }
    internal CertificationLineageKind LineageKind { get; }
    internal IReadOnlyList<string> Ancestors { get; }
    internal bool SnapshotValid { get; }
}

internal sealed class CodeDocumentationCertificationScopeDeclaration
{
    internal const string CurrentSchema = "code-documentation-certification-scope-v1";
    internal const string CertificationPurpose = "independent-certification-v1";

    internal CodeDocumentationCertificationScopeDeclaration(
        string schema, string domain, string designPattern, string subject,
        string scopeId, string scopeVersion, string purpose, DateTimeOffset coverageCutUtc,
        string coverageRevision, IEnumerable<string>? checkpointLineageManifestIdentities,
        IEnumerable<CertificationOwnerSnapshot>? ownerSnapshots,
        CertificationHistoryCoverage? history,
        IEnumerable<CertificationSourceBinding>? certificationSourceBindings)
    {
        Schema = schema;
        Domain = domain;
        DesignPattern = designPattern;
        Subject = subject;
        ScopeId = scopeId;
        ScopeVersion = scopeVersion;
        Purpose = purpose;
        CoverageCutUtc = coverageCutUtc;
        CoverageRevision = coverageRevision;
        History = history;
        CheckpointLineageManifestIdentities = CertificationSnapshot.Copy(
            checkpointLineageManifestIdentities, CertificationSnapshot.MaximumSources, out var lineageValid);
        OwnerSnapshots = CertificationSnapshot.Copy(
            ownerSnapshots, CertificationSnapshot.MaximumSources, out var ownersValid);
        CertificationSourceBindings = CertificationSnapshot.Copy(
            certificationSourceBindings, CertificationSnapshot.MaximumSources, out var bindingsValid);
        SnapshotValid = lineageValid && ownersValid && bindingsValid;
    }

    internal string Schema { get; }
    internal string Domain { get; }
    internal string DesignPattern { get; }
    internal string Subject { get; }
    internal string ScopeId { get; }
    internal string ScopeVersion { get; }
    internal string Purpose { get; }
    internal DateTimeOffset CoverageCutUtc { get; }
    internal string CoverageRevision { get; }
    internal IReadOnlyList<string> CheckpointLineageManifestIdentities { get; }
    internal IReadOnlyList<CertificationOwnerSnapshot> OwnerSnapshots { get; }
    internal CertificationHistoryCoverage? History { get; }
    internal IReadOnlyList<CertificationSourceBinding> CertificationSourceBindings { get; }
    internal bool SnapshotValid { get; }
}

internal sealed class CodeDocumentationCertificationExposureIndex
{
    internal const string CurrentSchema = "code-documentation-certification-exposure-index-v1";

    internal CodeDocumentationCertificationExposureIndex(
        string schema, string version, CodeDocumentationCertificationScopeDeclaration scope,
        string coveredCheckpointManifestSha256, CertificationHistoryCoverage? history,
        IEnumerable<CodeDocumentationCertificationSource>? sources)
    {
        Schema = schema;
        Version = version;
        Scope = scope;
        CoveredCheckpointManifestSha256 = coveredCheckpointManifestSha256;
        History = history;
        Sources = CertificationSnapshot.Copy(sources, CertificationSnapshot.MaximumSources, out var valid);
        SnapshotValid = valid;
    }

    internal string Schema { get; }
    internal string Version { get; }
    internal CodeDocumentationCertificationScopeDeclaration Scope { get; }
    internal string CoveredCheckpointManifestSha256 { get; }
    internal CertificationHistoryCoverage? History { get; }
    internal IReadOnlyList<CodeDocumentationCertificationSource> Sources { get; }
    internal bool SnapshotValid { get; }
}

// Value snapshots never retain caller collections or silently accept a bounded prefix.
// A failed snapshot carries no partial data and cannot issue authority.
internal static class CertificationSnapshot
{
    internal const int MaximumSources = 10_000;
    internal const int MaximumEdges = 100_000;

    internal static IReadOnlyList<T> Copy<T>(IEnumerable<T>? values, int maximum, out bool valid)
    {
        valid = false;
        if (values is null)
            return Array.AsReadOnly(Array.Empty<T>());
        var copy = new List<T>();
        try
        {
            IEnumerator<T>? iterator = null;
            Exception? primary = null;
            try
            {
                iterator = values.GetEnumerator();
                while (iterator.MoveNext())
                {
                    var current = iterator.Current;
                    if (copy.Count == maximum || current is null)
                        return Array.AsReadOnly(Array.Empty<T>());
                    copy.Add(current);
                }
            }
            catch (Exception exception)
            {
                primary = exception;
                throw;
            }
            finally
            {
                try { iterator?.Dispose(); }
                catch (Exception exception) when (primary is not null && SnapshotExceptionGraphClassifier.IsNonFatal(exception))
                {
                    // Secondary cleanup cannot hide cancellation/fatal primary control flow.
                }
            }
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Array.AsReadOnly(Array.Empty<T>());
        }
        valid = true;
        return Array.AsReadOnly(copy.ToArray());
    }
}

/// <summary>
/// Снимок доверенного server owner, а не подтверждение чистоты экзамена.
/// Production provisioning/caller отдельно обязаны получить полную историю;
/// HTTP-утверждение, hash или публичный manifest эту границу не заменяют.
/// </summary>
internal sealed class ServerOwnedCodeDocumentationCertificationScope
{
    private ServerOwnedCodeDocumentationCertificationScope(
        CodeDocumentationCertificationScopeDeclaration declaration,
        CodeDocumentationSavedModelAssessmentModelPassport candidate,
        RepositoryOwnedReferenceCorpusOpenResult corpus,
        CodeDocumentationSavedModelAssessmentGenerationSettings generation,
        ContinuousLearningTp1DpoConfiguration evaluator,
        MeasurementNoisePolicyConfig noise,
        CodeDocumentationCertificationExposureIndex index,
        int expectedCalls)
    {
        Declaration = declaration;
        Candidate = candidate with { };
        Corpus = new(corpus.Manifest with { }, corpus.Corpus);
        Generation = generation with { };
        Evaluator = evaluator;
        Noise = noise;
        Index = index;
        ExpectedCalls = expectedCalls;
        ScopeIdentity = CertificationIdentity.Scope(declaration);
        IndexIdentity = CertificationIdentity.Index(index);
        ConditionsIdentity = CertificationIdentity.Conditions(this);
    }

    internal CodeDocumentationCertificationScopeDeclaration Declaration { get; }
    internal CodeDocumentationSavedModelAssessmentModelPassport Candidate { get; }
    internal RepositoryOwnedReferenceCorpusOpenResult Corpus { get; }
    internal CodeDocumentationSavedModelAssessmentGenerationSettings Generation { get; }
    internal ContinuousLearningTp1DpoConfiguration Evaluator { get; }
    internal MeasurementNoisePolicyConfig Noise { get; }
    internal CodeDocumentationCertificationExposureIndex Index { get; }
    internal int ExpectedCalls { get; }
    internal string ScopeIdentity { get; }
    internal string IndexIdentity { get; }
    internal string ConditionsIdentity { get; }

    internal static OperationResult<ServerOwnedCodeDocumentationCertificationScope> CreateFromServerOwnedSnapshot(
        CodeDocumentationCertificationScopeDeclaration declaration,
        CodeDocumentationSavedModelAssessmentModelPassport acceptedCheckpoint,
        RepositoryOwnedReferenceCorpusOpenResult certificationCorpus,
        CodeDocumentationSavedModelAssessmentGenerationSettings generation,
        ContinuousLearningTp1DpoConfiguration evaluator,
        MeasurementNoisePolicyConfig noise,
        CodeDocumentationCertificationExposureIndex approvedExposureIndex)
    {
        if (!CertificationValidation.StructuralScope(declaration) ||
            !CertificationValidation.StructuralIndex(approvedExposureIndex))
            return Failure("INPUT_INVALID");
        if (!CertificationValidation.Candidate(acceptedCheckpoint))
            return Failure("CANDIDATE_MISMATCH");
        if (!CertificationValidation.Corpus(certificationCorpus))
            return Failure("CORPUS_MISMATCH");
        if (!CertificationValidation.Generation(generation) || evaluator is null || noise is null ||
            noise.IdentitySha256 != MeasurementNoisePolicyConfig.Canonical.IdentitySha256)
            return Failure("CONDITIONS_MISMATCH");

        var plan = CodeDocumentationSavedModelAssessmentCalibrationService.PlanCalibration(
            certificationCorpus.Corpus, generation, evaluator, noise);
        if (!plan.IsSuccess || plan.Value is null)
            return Failure("CONDITIONS_MISMATCH");

        // Semantic provenance/overlap intentionally belongs to TryCreate, not this issuer.
        return OperationResult<ServerOwnedCodeDocumentationCertificationScope>.Ok(new(
            declaration, acceptedCheckpoint, certificationCorpus, generation, evaluator, noise,
            approvedExposureIndex, plan.Value.ExpectedEvaluationCallCount));
    }

    private static OperationResult<ServerOwnedCodeDocumentationCertificationScope> Failure(string suffix)
        => OperationResult<ServerOwnedCodeDocumentationCertificationScope>.Fail(
            "Серверный снимок допуска экзамена не прошёл проверку.", CodeDocumentationCertificationAdmission.ErrorPrefix + suffix);
}

/// <summary>
/// Неизменяемый допуск в пределах принятой истории источников. Не содержит
/// закрытых текстов, результатов измерения, сертификата или права повторить экзамен.
/// </summary>
public sealed class CodeDocumentationCertificationAdmission
{
    internal const string ErrorPrefix = "CODE_DOCUMENTATION_CERTIFICATION_ADMISSION_";
    public const string CurrentSchema = "code-documentation-certification-admission-v1";
    public const string Metric = "code-documentation-case-pass-fraction-v1";
    public const string TargetPolicy = "code-documentation-strict-target-noise-v1";
    public const double TargetCorrectFraction = 0.95d;

    private CodeDocumentationCertificationAdmission(ServerOwnedCodeDocumentationCertificationScope authority)
    {
        Schema = CurrentSchema;
        Purpose = authority.Declaration.Purpose;
        ScopeId = authority.Declaration.ScopeId;
        ScopeVersion = authority.Declaration.ScopeVersion;
        CoverageCutUtc = authority.Declaration.CoverageCutUtc;
        ScopeIdentitySha256 = authority.ScopeIdentity;
        ExposureIndexIdentitySha256 = authority.IndexIdentity;
        ExposureIndexVersion = authority.Index.Version;
        EvaluationConditionsIdentitySha256 = authority.ConditionsIdentity;
        Candidate = authority.Candidate with { };
        CorpusManifest = authority.Corpus.Manifest with { };
        CaseIds = Array.AsReadOnly(authority.Corpus.Corpus.EntryNames.ToArray());
        GenerationSettings = authority.Generation with { };
        GenerationSettingsSha256 = CodeDocumentationSavedModelAssessmentService.ComputeGenerationSettingsSha256(
            GenerationSettings, authority.Corpus.Corpus.Entries);
        var profile = authority.Evaluator.EvaluationProfile;
        Evaluator = new(authority.Evaluator.ModelId, authority.Evaluator.ExpectedOllamaManifestDigest,
            authority.Evaluator.ExpectedOllamaModelLayerDigest, profile.ProfileId,
            profile.ConfigurationSha256, Array.AsReadOnly(Array.Empty<string>()));
        NoisePolicyIdentitySha256 = authority.Noise.IdentitySha256;
        RepeatCount = authority.Noise.RepeatCount;
        StandardDeviationMultiplier = authority.Noise.StandardDeviationMultiplier;
        ExpectedEvaluationCallCount = authority.ExpectedCalls;
        AdmissionIdentitySha256 = CertificationIdentity.Admission(authority);
    }

    public string Schema { get; }
    public string Purpose { get; }
    public string ScopeId { get; }
    public string ScopeVersion { get; }
    public DateTimeOffset CoverageCutUtc { get; }
    public string ScopeIdentitySha256 { get; }
    public string ExposureIndexIdentitySha256 { get; }
    public string ExposureIndexVersion { get; }
    public string EvaluationConditionsIdentitySha256 { get; }
    public string AdmissionIdentitySha256 { get; }
    public CodeDocumentationSavedModelAssessmentModelPassport Candidate { get; }
    public RepositoryOwnedReferenceCorpusManifest CorpusManifest { get; }
    public IReadOnlyList<string> CaseIds { get; }
    public CodeDocumentationSavedModelAssessmentGenerationSettings GenerationSettings { get; }
    public string GenerationSettingsSha256 { get; }
    public CodeDocumentationSavedModelAssessmentEvaluatorReceipt Evaluator { get; }
    public string NoisePolicyIdentitySha256 { get; }
    public int RepeatCount { get; }
    public double StandardDeviationMultiplier { get; }
    public string MetricProfileId => Metric;
    public string TargetPolicyId => TargetPolicy;
    public double Target => TargetCorrectFraction;
    public int ExpectedEvaluationCallCount { get; }

    internal static OperationResult<CodeDocumentationCertificationAdmission> TryCreate(
        ServerOwnedCodeDocumentationCertificationScope? authority,
        CodeDocumentationSavedModelAssessmentModelPassport? candidate,
        RepositoryOwnedReferenceCorpusOpenResult? corpus,
        CodeDocumentationCertificationExposureIndex? exposureIndex)
    {
        if (authority is null)
            return Failure("AUTHORITY_REQUIRED");
        if (candidate is null || corpus is null || exposureIndex is null)
            return Failure("INPUT_INVALID");
        if (!CertificationValidation.Candidate(candidate) || candidate != authority.Candidate)
            return Failure("CANDIDATE_MISMATCH");
        if (!CertificationValidation.Corpus(corpus) || corpus.Manifest != authority.Corpus.Manifest ||
            corpus.Corpus.CanonicalRepresentation != authority.Corpus.Corpus.CanonicalRepresentation)
            return Failure("CORPUS_MISMATCH");
        if (!CertificationValidation.StructuralIndex(exposureIndex) ||
            CertificationIdentity.Index(exposureIndex) != authority.IndexIdentity)
            return Failure("SCOPE_MISMATCH");
        var scope = authority.Declaration;
        if (scope.Purpose != CodeDocumentationCertificationScopeDeclaration.CertificationPurpose)
            return Failure("PURPOSE_INVALID");
        if (CertificationIdentity.Scope(exposureIndex.Scope) != authority.ScopeIdentity ||
            scope.Domain != candidate.DomainSlug || scope.DesignPattern != candidate.DesignPatternSlug ||
            scope.Domain != corpus.Corpus.DomainTag || scope.Subject != corpus.Corpus.SubjectId)
            return Failure("SCOPE_MISMATCH");

        var entries = corpus.Corpus.Entries;
        if (entries.Any(static entry => entry.Partition != FrozenReferencePartition.HeldOut) ||
            entries.Select(static entry => entry.SourceEntityFingerprint).Distinct(StringComparer.Ordinal).Count() != entries.Count ||
            entries.Select(static entry => FrozenReferenceEntry.ComputeNormalizedTextSha256(entry.OwnerInput))
                .Distinct(StringComparer.Ordinal).Count() != entries.Count)
            return Failure("CORPUS_MISMATCH");
        if (!CertificationValidation.CompleteProvenance(scope, candidate, entries, exposureIndex))
            return Failure("PROVENANCE_INCOMPLETE");

        var nodes = exposureIndex.Sources.ToDictionary(static node => node.SourceEntityFingerprint, StringComparer.Ordinal);
        var certification = entries.Select(static entry => entry.SourceEntityFingerprint).ToHashSet(StringComparer.Ordinal);
        var exposed = nodes.Values.Where(static node => CertificationValidation.Forbidden(node.Roles))
            .Select(static node => node.SourceEntityFingerprint).ToHashSet(StringComparer.Ordinal);
        if (certification.Overlaps(exposed))
            return Failure("SOURCE_OVERLAP");
        var exposedContent = exposed.Select(id => FrozenReferenceEntry.ComputeNormalizedTextSha256(nodes[id].SourceInput))
            .ToHashSet(StringComparer.Ordinal);
        if (entries.Any(entry => exposedContent.Contains(FrozenReferenceEntry.ComputeNormalizedTextSha256(entry.OwnerInput))))
            return Failure("CONTENT_OVERLAP");
        if (Closure(certification, nodes).Overlaps(Closure(exposed, nodes)))
            return Failure("LINEAGE_OVERLAP");

        return OperationResult<CodeDocumentationCertificationAdmission>.Ok(new(authority));
    }

    private static HashSet<string> Closure(IEnumerable<string> roots,
        IReadOnlyDictionary<string, CodeDocumentationCertificationSource> nodes)
    {
        var reached = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>(roots);
        while (pending.TryPop(out var id))
        {
            if (!reached.Add(id))
                continue;
            foreach (var ancestor in nodes[id].Ancestors)
                pending.Push(ancestor);
        }
        return reached;
    }

    private static OperationResult<CodeDocumentationCertificationAdmission> Failure(string suffix)
        => OperationResult<CodeDocumentationCertificationAdmission>.Fail(
            "Независимый экзамен не допущен: условия или полнота истории источников не подтверждены.", ErrorPrefix + suffix);
}

internal static class CertificationValidation
{
    internal static bool StructuralScope(CodeDocumentationCertificationScopeDeclaration? scope)
        => scope is not null && scope.SnapshotValid &&
           scope.Schema == CodeDocumentationCertificationScopeDeclaration.CurrentSchema &&
           Identity(scope.Domain) && Identity(scope.DesignPattern) && Identity(scope.Subject) &&
           Identity(scope.ScopeId) && Identity(scope.ScopeVersion) && Identity(scope.Purpose) &&
           Identity(scope.CoverageRevision) && FrozenReferenceCanonicalization.IsUtc(scope.CoverageCutUtc) &&
           scope.CheckpointLineageManifestIdentities.All(Hex) &&
           scope.OwnerSnapshots.All(static owner => Identity(owner.Id) && OptionalIdentity(owner.Version)) &&
           scope.CertificationSourceBindings.All(static binding =>
               Ref(binding.EntryId) && Ref(binding.SourceEntityFingerprint) &&
               Identity(binding.OwnerSnapshotId) && OptionalIdentity(binding.OwnerSnapshotVersion));

    internal static bool StructuralIndex(CodeDocumentationCertificationExposureIndex? index)
        => index is not null && index.SnapshotValid && StructuralScope(index.Scope) &&
           index.Schema == CodeDocumentationCertificationExposureIndex.CurrentSchema && Identity(index.Version) &&
           Hex(index.CoveredCheckpointManifestSha256) &&
           index.Sources.Sum(static source => (long)source.Ancestors.Count) <= CertificationSnapshot.MaximumEdges &&
           index.Sources.All(static source => source.SnapshotValid && Ref(source.SourceEntityFingerprint) &&
               Identity(source.Domain) && Identity(source.Subject) && Identity(source.OwnerSnapshotId) &&
               OptionalIdentity(source.OwnerSnapshotVersion) && Content(source.SourceInput) &&
               Ref(source.OriginEvidenceRef) && source.Ancestors.All(Ref));

    internal static bool Candidate(CodeDocumentationSavedModelAssessmentModelPassport? candidate)
        => candidate is not null &&
           candidate.ModelSourceKind == CodeDocumentationSavedModelAssessmentModelPassport.AcceptedRepositoryCheckpointSourceKind &&
           Identity(candidate.DomainSlug) && Identity(candidate.DesignPatternSlug) && Identity(candidate.ModelIdSlug) &&
           Identity(candidate.ModelVersion) && candidate.CheckpointStep > 0 &&
           Path(candidate.RepositoryRelativeManifestPath) && Hex(candidate.ManifestSha256) &&
           Path(candidate.RepositoryRelativeWeightsPath) && Hex(candidate.WeightsSha256) && candidate.WeightsByteLength > 0 &&
           candidate.MaximumModelSerializationBytes >= candidate.WeightsByteLength &&
           ((string.IsNullOrEmpty(candidate.RepositoryRelativeModelIdentityPath) && string.IsNullOrEmpty(candidate.ModelIdentitySha256)) ||
            (candidate.RepositoryRelativeModelIdentityPath == candidate.RepositoryRelativeManifestPath &&
             candidate.ModelIdentitySha256 == candidate.ManifestSha256));

    internal static bool Corpus(RepositoryOwnedReferenceCorpusOpenResult? opened)
    {
        if (opened?.Manifest is not { } manifest || opened.Corpus is not { } corpus)
            return false;
        return manifest.Schema == RepositoryOwnedReferenceCorpusManifestSealer.CurrentSchema &&
               manifest.Context == RepositoryOwnedReferenceCorpusManifestSealer.CurrentContext &&
               manifest.Domain == corpus.DomainTag && manifest.Subject == corpus.SubjectId &&
               manifest.CorpusVersion == corpus.CorpusVersion && manifest.CorpusName == corpus.CorpusName &&
               manifest.FrozenAtUtc == FrozenReferenceCanonicalization.FormatUtc(corpus.FrozenAtUtc) &&
               manifest.EntryCount == corpus.Count && corpus.Count is > 0 and <= 10_000 &&
               manifest.CorpusSha256 == corpus.CorpusSha256 && Ref(manifest.CorpusSha256) &&
               Hex(manifest.ManifestSha256) && Hex(manifest.PayloadSha256) &&
               manifest.PayloadByteLength is > 0 and <= 32L * 1024 * 1024 &&
               Path(manifest.RepositoryRelativeCorpusDirectory) &&
               manifest.RepositoryRelativeCorpusDirectory == $"training-data/reference-corpora/{manifest.Domain}/{manifest.Subject}/{manifest.CorpusVersion}" &&
               manifest.RepositoryRelativePayloadPath == manifest.RepositoryRelativeCorpusDirectory + "/corpus.json" &&
               manifest.RepositoryRelativeManifestPath == manifest.RepositoryRelativeCorpusDirectory + "/manifest.json";
    }

    internal static bool Generation(CodeDocumentationSavedModelAssessmentGenerationSettings? settings)
        => settings is not null && settings.MaxNewTokens is > 0 and <= 2_048 &&
           float.IsFinite(settings.Temperature) && settings.Temperature is >= 0f and <= 2f &&
           settings.TopK is >= 0 and <= 10_000 && float.IsFinite(settings.TopP) && settings.TopP is > 0f and <= 1f &&
           settings.MaxHeldOutCases is >= 1 and <= 200 && settings.MaxAnswerChars is >= 1 and <= 65_536;

    internal static bool CompleteProvenance(CodeDocumentationCertificationScopeDeclaration scope,
        CodeDocumentationSavedModelAssessmentModelPassport candidate,
        IReadOnlyList<FrozenReferenceEntry> entries, CodeDocumentationCertificationExposureIndex index)
    {
        if (!History(scope.History, index.Sources) || !History(index.History, index.Sources) ||
            !SameHistory(scope.History!, index.History!) ||
            index.CoveredCheckpointManifestSha256 != candidate.ManifestSha256 ||
            !scope.CheckpointLineageManifestIdentities.Contains(candidate.ManifestSha256, StringComparer.Ordinal) ||
            scope.CheckpointLineageManifestIdentities.Distinct(StringComparer.Ordinal).Count() != scope.CheckpointLineageManifestIdentities.Count)
            return false;
        var owners = new HashSet<(string Id, string Version)>();
        foreach (var owner in scope.OwnerSnapshots)
            if (!Identity(owner.Version) || !owners.Add((owner.Id, owner.Version)))
                return false;
        var nodes = new Dictionary<string, CodeDocumentationCertificationSource>(StringComparer.Ordinal);
        foreach (var source in index.Sources)
        {
            if (source.Domain != scope.Domain || source.Subject != scope.Subject ||
                !owners.Contains((source.OwnerSnapshotId, source.OwnerSnapshotVersion)) ||
                (source.Roles & ~(CertificationExposureRole.ReferenceAcquisitionOnly | CertificationExposureRole.Training |
                    CertificationExposureRole.TeacherVisibleDiagnostic | CertificationExposureRole.AuthoringOrTeaching)) != 0 ||
                !nodes.TryAdd(source.SourceEntityFingerprint, source))
                return false;
        }
        if (!AcyclicCompleteGraph(nodes))
            return false;
        var bindings = new Dictionary<string, CertificationSourceBinding>(StringComparer.Ordinal);
        foreach (var binding in scope.CertificationSourceBindings)
            if (!bindings.TryAdd(binding.EntryId, binding))
                return false;
        if (bindings.Count != entries.Count)
            return false;
        foreach (var entry in entries)
        {
            if (!bindings.TryGetValue(entry.EntryId, out var binding) ||
                binding.SourceEntityFingerprint != entry.SourceEntityFingerprint ||
                !nodes.TryGetValue(entry.SourceEntityFingerprint, out var node) ||
                node.OwnerSnapshotId != binding.OwnerSnapshotId || node.OwnerSnapshotVersion != binding.OwnerSnapshotVersion ||
                FrozenReferenceCanonicalization.NormalizeContent(node.SourceInput).Value != entry.OwnerInput)
                return false;
        }
        return true;
    }

    private static bool AcyclicCompleteGraph(IReadOnlyDictionary<string, CodeDocumentationCertificationSource> nodes)
    {
        var remaining = new Dictionary<string, int>(StringComparer.Ordinal);
        var descendants = nodes.Keys.ToDictionary(static id => id, static _ => new List<string>(), StringComparer.Ordinal);
        foreach (var (id, node) in nodes)
        {
            if ((node.LineageKind == CertificationLineageKind.KnownRoot && node.Ancestors.Count != 0) ||
                (node.LineageKind == CertificationLineageKind.Derived && node.Ancestors.Count == 0) ||
                node.LineageKind is not (CertificationLineageKind.KnownRoot or CertificationLineageKind.Derived) ||
                node.Ancestors.Distinct(StringComparer.Ordinal).Count() != node.Ancestors.Count)
                return false;
            remaining[id] = node.Ancestors.Count;
            foreach (var ancestor in node.Ancestors)
            {
                if (!descendants.TryGetValue(ancestor, out var children))
                    return false;
                children.Add(id);
            }
        }
        var roots = new Queue<string>(remaining.Where(static pair => pair.Value == 0).Select(static pair => pair.Key));
        var visited = 0;
        while (roots.TryDequeue(out var id))
        {
            visited++;
            foreach (var child in descendants[id])
                if (--remaining[child] == 0)
                    roots.Enqueue(child);
        }
        return visited == nodes.Count;
    }

    private static bool History(CertificationHistoryCoverage? history, IReadOnlyList<CodeDocumentationCertificationSource> nodes)
        => history is not null &&
           Category(history.Training, CertificationExposureRole.Training, nodes) &&
           Category(history.Diagnostic, CertificationExposureRole.TeacherVisibleDiagnostic, nodes) &&
           Category(history.Teaching, CertificationExposureRole.AuthoringOrTeaching, nodes);

    private static bool Category(CertificationHistoryCompleteness completeness, CertificationExposureRole role,
        IReadOnlyList<CodeDocumentationCertificationSource> nodes)
        => completeness == CertificationHistoryCompleteness.Complete ||
           (completeness == CertificationHistoryCompleteness.AttestedEmpty && nodes.All(node => (node.Roles & role) == 0));

    private static bool SameHistory(CertificationHistoryCoverage left, CertificationHistoryCoverage right)
        => left.Training == right.Training && left.Diagnostic == right.Diagnostic && left.Teaching == right.Teaching;

    internal static bool Forbidden(CertificationExposureRole roles)
        => (roles & (CertificationExposureRole.Training | CertificationExposureRole.TeacherVisibleDiagnostic |
            CertificationExposureRole.AuthoringOrTeaching)) != 0;

    private static bool Identity(string? value) => FrozenReferenceCanonicalization.NormalizeIdentity(value).IsSuccess;
    private static bool OptionalIdentity(string? value) => value == string.Empty || Identity(value);
    private static bool Ref(string? value) => FrozenReferenceCanonicalization.IsCanonicalSha256Ref(value);
    private static bool Hex(string? value) => ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaManifestDigest(value);
    private static bool Content(string? value) => !string.IsNullOrWhiteSpace(value) && FrozenReferenceCanonicalization.NormalizeContent(value).IsSuccess;
    private static bool Path(string? value)
        => !string.IsNullOrWhiteSpace(value) && value.Length <= 1024 && !value.Contains('\\') && !value.Contains(':') &&
           value.Split('/').All(static part => part.Length > 0 && part is not ("." or "..") &&
               part.All(static c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.'));
}

internal static class CertificationIdentity
{
    internal static string Scope(CodeDocumentationCertificationScopeDeclaration scope)
    {
        var text = new StringBuilder();
        Add(text, "schema", scope.Schema, scope.Domain, scope.DesignPattern, scope.Subject, scope.ScopeId,
            scope.ScopeVersion, scope.Purpose, FrozenReferenceCanonicalization.FormatUtc(scope.CoverageCutUtc), scope.CoverageRevision);
        AddHistory(text, scope.History);
        foreach (var manifest in scope.CheckpointLineageManifestIdentities.Order(StringComparer.Ordinal))
            Add(text, "checkpoint-lineage", manifest);
        foreach (var owner in scope.OwnerSnapshots.OrderBy(static x => x.Id, StringComparer.Ordinal).ThenBy(static x => x.Version, StringComparer.Ordinal))
            Add(text, "owner", owner.Id, owner.Version);
        foreach (var binding in scope.CertificationSourceBindings.OrderBy(static x => x.EntryId, StringComparer.Ordinal)
                     .ThenBy(static x => x.SourceEntityFingerprint, StringComparer.Ordinal)
                     .ThenBy(static x => x.OwnerSnapshotId, StringComparer.Ordinal).ThenBy(static x => x.OwnerSnapshotVersion, StringComparer.Ordinal))
            Add(text, "cert-binding", binding.EntryId, binding.SourceEntityFingerprint, binding.OwnerSnapshotId, binding.OwnerSnapshotVersion);
        return Hash(text);
    }

    internal static string Index(CodeDocumentationCertificationExposureIndex index)
    {
        var text = new StringBuilder();
        Add(text, "schema", index.Schema, index.Version, Scope(index.Scope), index.CoveredCheckpointManifestSha256);
        AddHistory(text, index.History);
        // Sorting full node representations also makes conflicting duplicates order-independent;
        // semantic validation rejects them instead of selecting a winner.
        foreach (var node in index.Sources.Select(Node).Order(StringComparer.Ordinal))
            Add(text, "source", node);
        return Hash(text);
    }

    internal static string Conditions(ServerOwnedCodeDocumentationCertificationScope authority)
    {
        var text = new StringBuilder();
        var profile = authority.Evaluator.EvaluationProfile;
        Add(text, "schema", "code-documentation-certification-conditions-v1",
            authority.Corpus.Manifest.ManifestSha256, authority.Corpus.Manifest.PayloadSha256,
            authority.Corpus.Corpus.CorpusSha256, authority.Corpus.Corpus.CorpusVersion,
            CodeDocumentationSavedModelAssessmentService.ComputeGenerationSettingsSha256(authority.Generation, authority.Corpus.Corpus.Entries),
            authority.Evaluator.ModelId, authority.Evaluator.ExpectedOllamaManifestDigest, authority.Evaluator.ExpectedOllamaModelLayerDigest,
            profile.ProfileId, profile.ConfigurationSha256, authority.Noise.IdentitySha256,
            authority.Noise.RepeatCount.ToString(CultureInfo.InvariantCulture),
            authority.Noise.StandardDeviationMultiplier.ToString("R", CultureInfo.InvariantCulture),
            CodeDocumentationCertificationAdmission.Metric, CodeDocumentationCertificationAdmission.TargetPolicy,
            CodeDocumentationCertificationAdmission.TargetCorrectFraction.ToString("R", CultureInfo.InvariantCulture),
            authority.ExpectedCalls.ToString(CultureInfo.InvariantCulture), authority.Evaluator.MaxTeacherCalls.ToString(CultureInfo.InvariantCulture));
        foreach (var id in authority.Corpus.Corpus.EntryNames)
            Add(text, "case", id);
        return Hash(text);
    }

    internal static string Admission(ServerOwnedCodeDocumentationCertificationScope authority)
    {
        var text = new StringBuilder();
        var candidate = authority.Candidate;
        var manifest = authority.Corpus.Manifest;
        Add(text, "schema", CodeDocumentationCertificationAdmission.CurrentSchema,
            authority.ScopeIdentity, authority.IndexIdentity, authority.ConditionsIdentity);
        Add(text, "candidate", candidate.DomainSlug, candidate.DesignPatternSlug, candidate.ModelIdSlug,
            candidate.ModelVersion, candidate.CheckpointStep.ToString(CultureInfo.InvariantCulture), candidate.ModelSourceKind,
            candidate.RepositoryRelativeManifestPath, candidate.ManifestSha256, candidate.RepositoryRelativeWeightsPath,
            candidate.WeightsSha256, candidate.WeightsByteLength.ToString(CultureInfo.InvariantCulture),
            candidate.MaximumModelSerializationBytes.ToString(CultureInfo.InvariantCulture), candidate.RepositoryRelativeModelIdentityPath,
            candidate.ModelIdentitySha256);
        Add(text, "corpus", manifest.Schema, manifest.Context, manifest.Domain, manifest.Subject, manifest.CorpusName,
            manifest.CorpusVersion, manifest.FrozenAtUtc, manifest.EntryCount.ToString(CultureInfo.InvariantCulture),
            manifest.CorpusSha256, manifest.RepositoryRelativeCorpusDirectory, manifest.RepositoryRelativePayloadPath,
            manifest.PayloadByteLength.ToString(CultureInfo.InvariantCulture), manifest.PayloadSha256,
            manifest.RepositoryRelativeManifestPath, manifest.ManifestSha256, authority.Corpus.Corpus.CanonicalRepresentation);
        return Hash(text);
    }

    private static string Node(CodeDocumentationCertificationSource node)
    {
        var text = new StringBuilder();
        Add(text, "node", node.SourceEntityFingerprint, node.Domain, node.Subject, node.OwnerSnapshotId,
            node.OwnerSnapshotVersion, FrozenReferenceCanonicalization.NormalizeContent(node.SourceInput).Value,
            node.OriginEvidenceRef, ((int)node.Roles).ToString(CultureInfo.InvariantCulture), ((int)node.LineageKind).ToString(CultureInfo.InvariantCulture));
        foreach (var ancestor in node.Ancestors.Order(StringComparer.Ordinal))
            Add(text, "ancestor", ancestor);
        return text.ToString();
    }

    private static void AddHistory(StringBuilder text, CertificationHistoryCoverage? history)
        => Add(text, "history", history is null ? null : ((int)history.Training).ToString(CultureInfo.InvariantCulture),
            history is null ? null : ((int)history.Diagnostic).ToString(CultureInfo.InvariantCulture),
            history is null ? null : ((int)history.Teaching).ToString(CultureInfo.InvariantCulture));

    private static void Add(StringBuilder text, string name, params string?[] values)
    {
        FrozenReferenceCanonicalization.Append(text, "field", name);
        FrozenReferenceCanonicalization.Append(text, "arity", values.Length.ToString(CultureInfo.InvariantCulture));
        for (var i = 0; i < values.Length; i++)
            FrozenReferenceCanonicalization.Append(text, i.ToString(CultureInfo.InvariantCulture), values[i]);
    }

    private static string Hash(StringBuilder text) => FrozenReferenceCanonicalization.Sha256Ref(text.ToString());
}
