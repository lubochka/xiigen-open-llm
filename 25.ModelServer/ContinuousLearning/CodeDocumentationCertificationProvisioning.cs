// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using TrainableRouter.Training;

/// <summary>
/// Reads the operator-owned certification snapshot selected by startup
/// configuration. The descriptor is trust-input from the protected deployment
/// boundary; approvalRef and hashes bind bytes but do not prove their facts.
/// </summary>
internal static class CodeDocumentationCertificationProvisioning
{
    internal const string EnvironmentName =
        "CONTINUOUS_LEARNING_CODE_DOCUMENTATION_CERTIFICATION_PROVISIONING_JSON";
    internal const string DescriptorSchema =
        "code-documentation-certification-provisioning-descriptor-v1";
    internal const string SnapshotSchema =
        "code-documentation-certification-approved-snapshot-v1";
    internal const string ErrorPrefix =
        "CODE_DOCUMENTATION_CERTIFICATION_PROVISIONING_";
    internal const int MaximumDescriptorBytes = 32 * 1024;
    internal const int MaximumSnapshotBytes = 32 * 1024 * 1024;
    internal const int MaximumSnapshotCount = 3;
    internal const int MaximumDepth = 16;

    private const int MaximumIdentityLength = 128;
    private const int MaximumProfileSlugLength = 96;
    private const int MaximumApprovalReferenceLength = 128;
    private const int MaximumPathLength = 320;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly JsonDocumentOptions JsonOptions = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = MaximumDepth
    };

    internal static OperationResult<ICodeDocumentationCertificationAuthorityProvider>
        LoadProvider(
            Func<string, string?> readEnvironment,
            string trustedRepositoryRoot)
    {
        ArgumentNullException.ThrowIfNull(readEnvironment);
        if (!TryCanonicalRoot(trustedRepositoryRoot, out var root))
            return Failure<ICodeDocumentationCertificationAuthorityProvider>("ROOT_INVALID");

        try
        {
            var descriptorText = readEnvironment(EnvironmentName);
            if (string.IsNullOrWhiteSpace(descriptorText))
                return Failure<ICodeDocumentationCertificationAuthorityProvider>("DESCRIPTOR_MISSING");

            var descriptorByteCount = StrictUtf8.GetByteCount(descriptorText);
            if (descriptorByteCount is <= 0 or > MaximumDescriptorBytes)
                return Failure<ICodeDocumentationCertificationAuthorityProvider>("DESCRIPTOR_LIMIT_EXCEEDED");

            var descriptorBytes = StrictUtf8.GetBytes(descriptorText);
            using var descriptorDocument = JsonDocument.Parse(descriptorBytes, JsonOptions);
            if (!TryReadDescriptor(descriptorDocument.RootElement, out var descriptors))
                return Failure<ICodeDocumentationCertificationAuthorityProvider>("DESCRIPTOR_INVALID");

            var approved = new List<ValidatedCertificationProvisioningRecord>(descriptors.Count);
            foreach (var descriptor in descriptors)
            {
                var snapshot = ReadSnapshot(root, descriptor);
                if (!snapshot.IsSuccess || snapshot.Value is null)
                    return Failure<ICodeDocumentationCertificationAuthorityProvider>(
                        snapshot.ErrorCode?.StartsWith(ErrorPrefix, StringComparison.Ordinal) == true
                            ? snapshot.ErrorCode[ErrorPrefix.Length..]
                            : "PAYLOAD_INVALID");
                approved.Add(snapshot.Value);
            }

            return OperationResult<ICodeDocumentationCertificationAuthorityProvider>.Ok(
                new ServerOwnedCodeDocumentationCertificationAuthorityProvider(root, approved));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Failure<ICodeDocumentationCertificationAuthorityProvider>("DESCRIPTOR_INVALID");
        }
    }

    private static OperationResult<ValidatedCertificationProvisioningRecord> ReadSnapshot(
        string trustedRepositoryRoot,
        CertificationProvisioningDescriptor descriptor)
    {
        if (!TryGetSnapshotPath(trustedRepositoryRoot, descriptor, out var path))
            return Failure<ValidatedCertificationProvisioningRecord>("PATH_INVALID");

        try
        {
            // Public checkout ancestors are allowed. The fixed provisioning
            // directory and payload must retain operator-only custody.
            if (!HasCustodyForSnapshotPath(trustedRepositoryRoot, path) ||
                !File.Exists(path) ||
                !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path))
            {
                return Failure<ValidatedCertificationProvisioningRecord>("CUSTODY_INVALID");
            }

            using var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                FileOptions.SequentialScan,
                bufferSize: 4096);
            if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    path))
            {
                return Failure<ValidatedCertificationProvisioningRecord>("HANDLE_INVALID");
            }

            var payloadBytes = SecureLocalFileBoundary.ReadBoundedStableBytes(
                stream,
                MaximumSnapshotBytes);
            if (payloadBytes is null ||
                payloadBytes.LongLength != descriptor.SnapshotByteLength ||
                payloadBytes.Length is < 1 or > MaximumSnapshotBytes)
            {
                return Failure<ValidatedCertificationProvisioningRecord>("PAYLOAD_LENGTH_INVALID");
            }

            var actualHash = Convert.ToHexString(SHA256.HashData(payloadBytes))
                .ToLowerInvariant();
            if (!StringComparer.Ordinal.Equals(actualHash, descriptor.SnapshotSha256))
                return Failure<ValidatedCertificationProvisioningRecord>("PAYLOAD_HASH_INVALID");

            // Recheck custody, reparse state and the opened handle after the
            // stable read so a replacement cannot become accepted input.
            if (!HasCustodyForSnapshotPath(trustedRepositoryRoot, path) ||
                !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path) ||
                !SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(
                    stream.SafeFileHandle,
                    path))
            {
                return Failure<ValidatedCertificationProvisioningRecord>("POST_READ_CUSTODY_INVALID");
            }

            try
            {
                _ = StrictUtf8.GetString(payloadBytes);
                using var snapshotDocument = JsonDocument.Parse(payloadBytes, JsonOptions);
                if (!TryReadSnapshot(snapshotDocument.RootElement, descriptor, out var declaration, out var index) ||
                    !CertificationValidation.StructuralScope(declaration) ||
                    !CertificationValidation.StructuralIndex(index))
                    return Failure<ValidatedCertificationProvisioningRecord>("PAYLOAD_INVALID");

                return OperationResult<ValidatedCertificationProvisioningRecord>.Ok(
                    new ValidatedCertificationProvisioningRecord(
                        descriptor,
                        declaration,
                        index));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
                when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
            {
                return Failure<ValidatedCertificationProvisioningRecord>("PAYLOAD_INVALID");
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Failure<ValidatedCertificationProvisioningRecord>("STORAGE_UNAVAILABLE");
        }
    }

    private static bool TryReadDescriptor(
        JsonElement root,
        out IReadOnlyList<CertificationProvisioningDescriptor> descriptors)
    {
        descriptors = Array.Empty<CertificationProvisioningDescriptor>();
        if (!HasExactProperties(root, "schema", "snapshots") ||
            !TryString(root, "schema", out var schema) ||
            schema != DescriptorSchema ||
            !TryArray(root, "snapshots", out var snapshots) ||
            snapshots.GetArrayLength() is < 1 or > MaximumSnapshotCount)
        {
            return false;
        }

        var values = new List<CertificationProvisioningDescriptor>(snapshots.GetArrayLength());
        var selectors = new HashSet<(string, string, string, string, string, string, string)>();
        foreach (var element in snapshots.EnumerateArray())
        {
            if (!HasExactProperties(
                    element,
                    "domainSlug",
                    "designPatternSlug",
                    "subjectSlug",
                    "scopeId",
                    "scopeVersion",
                    "ownerSnapshotId",
                    "ownerSnapshotVersion",
                    "approvalRef",
                    "snapshotRelativePath",
                    "snapshotSha256",
                    "snapshotByteLength") ||
                !TryString(element, "domainSlug", out var domainSlug) ||
                !TryString(element, "designPatternSlug", out var designPatternSlug) ||
                !TryString(element, "subjectSlug", out var subjectSlug) ||
                !TryString(element, "scopeId", out var scopeId) ||
                !TryString(element, "scopeVersion", out var scopeVersion) ||
                !TryString(element, "ownerSnapshotId", out var ownerSnapshotId) ||
                !TryString(element, "ownerSnapshotVersion", out var ownerSnapshotVersion) ||
                !TryString(element, "approvalRef", out var approvalRef) ||
                !TryString(element, "snapshotRelativePath", out var snapshotRelativePath) ||
                !TryString(element, "snapshotSha256", out var snapshotSha256) ||
                !TryInt64(element, "snapshotByteLength", out var snapshotByteLength) ||
                !IsProfileSlug(domainSlug) ||
                !IsProfileSlug(designPatternSlug) ||
                !IsProfileSlug(subjectSlug) ||
                !IsIdentity(scopeId) ||
                !IsIdentity(scopeVersion) ||
                !IsIdentity(ownerSnapshotId) ||
                !IsIdentity(ownerSnapshotVersion) ||
                !IsApprovalReference(approvalRef) ||
                !IsLowerHex(snapshotSha256) ||
                snapshotByteLength is < 1 or > MaximumSnapshotBytes ||
                !IsExactSnapshotPath(
                    snapshotRelativePath,
                    domainSlug,
                    designPatternSlug,
                    snapshotSha256))
            {
                return false;
            }

            if (!selectors.Add((domainSlug, designPatternSlug, subjectSlug,
                    scopeId, scopeVersion, ownerSnapshotId, ownerSnapshotVersion)))
                return false;

            values.Add(new CertificationProvisioningDescriptor(
                domainSlug,
                designPatternSlug,
                subjectSlug,
                scopeId,
                scopeVersion,
                ownerSnapshotId,
                ownerSnapshotVersion,
                approvalRef,
                snapshotRelativePath,
                snapshotSha256,
                snapshotByteLength));
        }

        descriptors = Array.AsReadOnly(values.ToArray());
        return true;
    }

    private static bool TryReadSnapshot(
        JsonElement root,
        CertificationProvisioningDescriptor descriptor,
        out CodeDocumentationCertificationScopeDeclaration declaration,
        out CodeDocumentationCertificationExposureIndex index)
    {
        declaration = null!;
        index = null!;
        if (!HasExactProperties(root, "schema", "approvalRef", "declaration", "exposureIndex") ||
            !TryString(root, "schema", out var schema) ||
            schema != SnapshotSchema ||
            !TryString(root, "approvalRef", out var approvalRef) ||
            approvalRef != descriptor.ApprovalRef ||
            !TryObject(root, "declaration", out var declarationElement) ||
            !TryObject(root, "exposureIndex", out var indexElement) ||
            !TryReadDeclaration(declarationElement, out declaration) ||
            !TryReadExposureIndex(indexElement, declaration, out index) ||
            declaration.Domain != descriptor.DomainSlug ||
            declaration.DesignPattern != descriptor.DesignPatternSlug ||
            declaration.Subject != descriptor.SubjectSlug ||
            declaration.ScopeId != descriptor.ScopeId ||
            declaration.ScopeVersion != descriptor.ScopeVersion ||
            index.Version != descriptor.ScopeVersion ||
            !declaration.OwnerSnapshots.Any(owner =>
                owner.Id == descriptor.OwnerSnapshotId &&
                owner.Version == descriptor.OwnerSnapshotVersion))
        {
            declaration = null!;
            index = null!;
            return false;
        }

        return true;
    }

    private static bool TryReadDeclaration(
        JsonElement element,
        out CodeDocumentationCertificationScopeDeclaration declaration)
    {
        declaration = null!;
        if (!HasExactProperties(
                element,
                "domain",
                "designPattern",
                "subject",
                "scopeId",
                "scopeVersion",
                "purpose",
                "coverageCutUtc",
                "coverageRevision",
                "checkpointLineageManifestIdentities",
                "ownerSnapshots",
                "history",
                "certificationSourceBindings") ||
            !TryString(element, "domain", out var domain) ||
            !TryString(element, "designPattern", out var designPattern) ||
            !TryString(element, "subject", out var subject) ||
            !TryString(element, "scopeId", out var scopeId) ||
            !TryString(element, "scopeVersion", out var scopeVersion) ||
            !TryString(element, "purpose", out var purpose) ||
            !TryUtc(element, "coverageCutUtc", out var coverageCutUtc) ||
            !TryString(element, "coverageRevision", out var coverageRevision) ||
            !TrySha256ReferenceArray(element, "checkpointLineageManifestIdentities", out var lineage,
                expectBareDigest: true) ||
            !TryOwnerSnapshots(element, "ownerSnapshots", out var ownerSnapshots) ||
            !TryHistory(element, "history", out var history) ||
            !TrySourceBindings(element, "certificationSourceBindings", out var bindings) ||
            !IsProfileSlug(domain) ||
            !IsProfileSlug(designPattern) ||
            !IsProfileSlug(subject) ||
            !IsIdentity(scopeId) ||
            !IsIdentity(scopeVersion) ||
            purpose != CodeDocumentationCertificationScopeDeclaration.CertificationPurpose ||
            !IsIdentity(coverageRevision))
        {
            return false;
        }

        declaration = new CodeDocumentationCertificationScopeDeclaration(
            CodeDocumentationCertificationScopeDeclaration.CurrentSchema,
            domain,
            designPattern,
            subject,
            scopeId,
            scopeVersion,
            purpose,
            coverageCutUtc,
            coverageRevision,
            lineage,
            ownerSnapshots,
            history,
            bindings);
        return declaration.SnapshotValid;
    }

    private static bool TryReadExposureIndex(
        JsonElement element,
        CodeDocumentationCertificationScopeDeclaration declaration,
        out CodeDocumentationCertificationExposureIndex index)
    {
        index = null!;
        if (!HasExactProperties(
                element,
                "version",
                "coveredCheckpointManifestSha256",
                "history",
                "sources") ||
            !TryString(element, "version", out var version) ||
            !TryString(element, "coveredCheckpointManifestSha256", out var coveredCheckpoint) ||
            !TryHistory(element, "history", out var history) ||
            !TrySources(element, "sources", out var sources) ||
            !IsIdentity(version) ||
            !IsLowerHex(coveredCheckpoint))
        {
            return false;
        }

        index = new CodeDocumentationCertificationExposureIndex(
            CodeDocumentationCertificationExposureIndex.CurrentSchema,
            version,
            declaration,
            coveredCheckpoint,
            history,
            sources);
        return index.SnapshotValid;
    }

    private static bool TryOwnerSnapshots(
        JsonElement parent,
        string name,
        out IReadOnlyList<CertificationOwnerSnapshot> values)
    {
        values = Array.Empty<CertificationOwnerSnapshot>();
        if (!TryArray(parent, name, out var array) || array.GetArrayLength() is < 1 or > CertificationSnapshot.MaximumSources)
            return false;
        var result = new List<CertificationOwnerSnapshot>(array.GetArrayLength());
        foreach (var element in array.EnumerateArray())
        {
            if (!HasExactProperties(element, "id", "version") ||
                !TryString(element, "id", out var id) ||
                !TryString(element, "version", out var version) ||
                !IsIdentity(id) ||
                !IsIdentity(version))
            {
                return false;
            }
            result.Add(new CertificationOwnerSnapshot(id, version));
        }
        values = Array.AsReadOnly(result.ToArray());
        return true;
    }

    private static bool TrySourceBindings(
        JsonElement parent,
        string name,
        out IReadOnlyList<CertificationSourceBinding> values)
    {
        values = Array.Empty<CertificationSourceBinding>();
        if (!TryArray(parent, name, out var array) || array.GetArrayLength() is < 1 or > CertificationSnapshot.MaximumSources)
            return false;
        var result = new List<CertificationSourceBinding>(array.GetArrayLength());
        foreach (var element in array.EnumerateArray())
        {
            if (!HasExactProperties(
                    element,
                    "entryId",
                    "sourceEntityFingerprint",
                    "ownerSnapshotId",
                    "ownerSnapshotVersion") ||
                !TryString(element, "entryId", out var entryId) ||
                !TryString(element, "sourceEntityFingerprint", out var fingerprint) ||
                !TryString(element, "ownerSnapshotId", out var ownerId) ||
                !TryString(element, "ownerSnapshotVersion", out var ownerVersion) ||
                !IsIdentity(entryId) ||
                !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(fingerprint) ||
                !IsIdentity(ownerId) ||
                !IsIdentity(ownerVersion))
            {
                return false;
            }
            result.Add(new CertificationSourceBinding(entryId, fingerprint, ownerId, ownerVersion));
        }
        values = Array.AsReadOnly(result.ToArray());
        return true;
    }

    private static bool TrySources(
        JsonElement parent,
        string name,
        out IReadOnlyList<CodeDocumentationCertificationSource> values)
    {
        values = Array.Empty<CodeDocumentationCertificationSource>();
        if (!TryArray(parent, name, out var array) || array.GetArrayLength() is < 1 or > CertificationSnapshot.MaximumSources)
            return false;
        var result = new List<CodeDocumentationCertificationSource>(array.GetArrayLength());
        var edgeCount = 0L;
        foreach (var element in array.EnumerateArray())
        {
            if (!HasExactProperties(
                    element,
                    "sourceEntityFingerprint",
                    "domain",
                    "subject",
                    "ownerSnapshotId",
                    "ownerSnapshotVersion",
                    "sourceInput",
                    "originEvidenceRef",
                    "roles",
                    "lineageKind",
                    "ancestors") ||
                !TryString(element, "sourceEntityFingerprint", out var fingerprint) ||
                !TryString(element, "domain", out var domain) ||
                !TryString(element, "subject", out var subject) ||
                !TryString(element, "ownerSnapshotId", out var ownerId) ||
                !TryString(element, "ownerSnapshotVersion", out var ownerVersion) ||
                !TryString(element, "sourceInput", out var sourceInput) ||
                !TryString(element, "originEvidenceRef", out var originEvidenceRef) ||
                !TryRoles(element, "roles", out var roles) ||
                !TryString(element, "lineageKind", out var lineageKindText) ||
                !TryArray(element, "ancestors", out var ancestorElements) ||
                ancestorElements.GetArrayLength() > CertificationSnapshot.MaximumEdges - edgeCount ||
                !TrySha256ReferenceArray(element, "ancestors", out var ancestors, allowEmpty: true) ||
                !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(fingerprint) ||
                !IsProfileSlug(domain) ||
                !IsProfileSlug(subject) ||
                !IsIdentity(ownerId) ||
                !IsIdentity(ownerVersion) ||
                !FrozenReferenceCanonicalization.NormalizeContent(sourceInput).IsSuccess ||
                !FrozenReferenceCanonicalization.IsCanonicalSha256Ref(originEvidenceRef) ||
                !TryLineageKind(lineageKindText, out var lineageKind))
            {
                return false;
            }

            edgeCount += ancestors.Count;
            result.Add(new CodeDocumentationCertificationSource(
                fingerprint,
                domain,
                subject,
                ownerId,
                ownerVersion,
                sourceInput,
                originEvidenceRef,
                roles,
                lineageKind,
                ancestors));
        }
        values = Array.AsReadOnly(result.ToArray());
        return true;
    }

    private static bool TryRoles(
        JsonElement parent,
        string name,
        out CertificationExposureRole roles)
    {
        roles = CertificationExposureRole.None;
        if (!TryArray(parent, name, out var array) || array.GetArrayLength() is < 1 or > 4)
            return false;
        foreach (var item in array.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String || item.GetString() is not { } text)
                return false;
            var role = text switch
            {
                "reference-acquisition-only" => CertificationExposureRole.ReferenceAcquisitionOnly,
                "training" => CertificationExposureRole.Training,
                "teacher-visible-diagnostic" => CertificationExposureRole.TeacherVisibleDiagnostic,
                "authoring-or-teaching" => CertificationExposureRole.AuthoringOrTeaching,
                _ => CertificationExposureRole.Unknown
            };
            if (role == CertificationExposureRole.Unknown || (roles & role) != 0)
                return false;
            roles |= role;
        }
        return roles != CertificationExposureRole.None;
    }

    private static bool TryHistory(
        JsonElement parent,
        string name,
        out CertificationHistoryCoverage history)
    {
        history = null!;
        if (!TryObject(parent, name, out var element) ||
            !HasExactProperties(element, "training", "diagnostic", "teaching") ||
            !TryString(element, "training", out var training) ||
            !TryString(element, "diagnostic", out var diagnostic) ||
            !TryString(element, "teaching", out var teaching) ||
            !TryCompleteness(training, out var trainingValue) ||
            !TryCompleteness(diagnostic, out var diagnosticValue) ||
            !TryCompleteness(teaching, out var teachingValue))
        {
            return false;
        }
        history = new CertificationHistoryCoverage(trainingValue, diagnosticValue, teachingValue);
        return true;
    }

    private static bool TrySha256ReferenceArray(
        JsonElement parent,
        string name,
        out IReadOnlyList<string> values,
        bool allowEmpty = false,
        bool expectBareDigest = false)
    {
        values = Array.Empty<string>();
        if (!TryArray(parent, name, out var array) ||
            (!allowEmpty && array.GetArrayLength() == 0) ||
            array.GetArrayLength() > CertificationSnapshot.MaximumSources)
            return false;
        var result = new List<string>(array.GetArrayLength());
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in array.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String ||
                item.GetString() is not { } value ||
                !(expectBareDigest
                    ? IsLowerHex(value)
                    : FrozenReferenceCanonicalization.IsCanonicalSha256Ref(value)) ||
                !seen.Add(value))
                return false;
            result.Add(value);
        }
        values = Array.AsReadOnly(result.ToArray());
        return true;
    }

    private static bool TryGetSnapshotPath(
        string trustedRepositoryRoot,
        CertificationProvisioningDescriptor descriptor,
        out string path)
    {
        path = string.Empty;
        if (!IsExactSnapshotPath(
                descriptor.SnapshotRelativePath,
                descriptor.DomainSlug,
                descriptor.DesignPatternSlug,
                descriptor.SnapshotSha256) ||
            !TryCanonicalRoot(trustedRepositoryRoot, out var root))
        {
            return false;
        }

        var relative = descriptor.SnapshotRelativePath.Replace('/', Path.DirectorySeparatorChar);
        var candidate = Path.GetFullPath(Path.Combine(root, relative));
        if (!IsStrictDescendant(root, candidate) ||
            !StringComparer.Ordinal.Equals(
                Path.GetRelativePath(root, candidate).Replace('\\', '/'),
                descriptor.SnapshotRelativePath))
        {
            return false;
        }
        path = candidate;
        return true;
    }

    private static bool TryCanonicalRoot(string value, out string root)
    {
        root = string.Empty;
        if (!SecureLocalFileBoundary.IsSupportedPlatform() ||
            string.IsNullOrWhiteSpace(value) ||
            !Path.IsPathFullyQualified(value))
        {
            return false;
        }
        try
        {
            root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(value));
            return Directory.Exists(root) &&
                   !SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(root);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            root = string.Empty;
            return false;
        }
    }

    private static bool HasCustodyForSnapshotPath(string root, string file)
    {
        if (!IsStrictDescendant(root, file) ||
            SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(file))
        {
            return false;
        }

        var provisioningRoot = Path.GetDirectoryName(file);
        return provisioningRoot is not null &&
               IsStrictDescendant(root, provisioningRoot) &&
               Path.GetFileName(provisioningRoot) == "provisioning" &&
               Directory.Exists(provisioningRoot) &&
               SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(provisioningRoot);
    }

    private static bool HasExactProperties(JsonElement element, params string[] names)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return false;
        var expected = new HashSet<string>(names, StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
            if (!expected.Contains(property.Name) || !seen.Add(property.Name))
                return false;
        return seen.Count == expected.Count;
    }

    private static bool TryString(JsonElement parent, string name, out string value)
    {
        value = string.Empty;
        return parent.TryGetProperty(name, out var property) &&
               property.ValueKind == JsonValueKind.String &&
               property.GetString() is { } parsed &&
               (value = parsed) is not null;
    }

    private static bool TryInt64(JsonElement parent, string name, out long value)
    {
        value = 0;
        return parent.TryGetProperty(name, out var property) &&
               property.ValueKind == JsonValueKind.Number &&
               property.TryGetInt64(out value);
    }

    private static bool TryArray(JsonElement parent, string name, out JsonElement value)
    {
        value = default;
        return parent.TryGetProperty(name, out value) && value.ValueKind == JsonValueKind.Array;
    }

    private static bool TryObject(JsonElement parent, string name, out JsonElement value)
    {
        value = default;
        return parent.TryGetProperty(name, out value) && value.ValueKind == JsonValueKind.Object;
    }

    private static bool TryUtc(JsonElement parent, string name, out DateTimeOffset value)
    {
        value = default;
        if (!TryString(parent, name, out var text) || !text.EndsWith('Z'))
            return false;
        return DateTimeOffset.TryParse(
                   text,
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.RoundtripKind,
                   out value) &&
               FrozenReferenceCanonicalization.IsUtc(value);
    }

    private static bool TryCompleteness(string value, out CertificationHistoryCompleteness result)
    {
        result = value switch
        {
            "unknown" => CertificationHistoryCompleteness.Unknown,
            "complete" => CertificationHistoryCompleteness.Complete,
            "attested-empty" => CertificationHistoryCompleteness.AttestedEmpty,
            _ => (CertificationHistoryCompleteness)(-1)
        };
        return result is CertificationHistoryCompleteness.Unknown or
            CertificationHistoryCompleteness.Complete or
            CertificationHistoryCompleteness.AttestedEmpty;
    }

    private static bool TryLineageKind(string value, out CertificationLineageKind result)
    {
        result = value switch
        {
            "known-root" => CertificationLineageKind.KnownRoot,
            "derived" => CertificationLineageKind.Derived,
            _ => (CertificationLineageKind)(-1)
        };
        return result is CertificationLineageKind.KnownRoot or CertificationLineageKind.Derived;
    }

    private static bool IsProfileSlug(string value)
        => value.Length is > 0 and <= MaximumProfileSlugLength &&
           value.All(static character => character is >= 'a' and <= 'z' or >= '0' and <= '9' or '-');

    private static bool IsIdentity(string value)
        => value.Length is > 0 and <= MaximumIdentityLength &&
           FrozenReferenceCanonicalization.NormalizeIdentity(value).IsSuccess;

    private static bool IsApprovalReference(string value)
        => value.Length is > 0 and <= MaximumApprovalReferenceLength &&
           value[0] is >= 'a' and <= 'z' or >= '0' and <= '9' &&
           value[1..].All(static character => character is >= 'a' and <= 'z' or >= '0' and <= '9' or '-');

    private static bool IsLowerHex(string value)
        => value.Length == 64 && value.All(static character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static bool IsExactSnapshotPath(
        string value,
        string domainSlug,
        string designPatternSlug,
        string snapshotSha256)
    {
        var expected = $"training-artifacts/assessments/{domainSlug}/{designPatternSlug}/provisioning/{snapshotSha256}.json";
        return value.Length is > 0 and <= MaximumPathLength &&
               value == expected &&
               value.All(static character => character is >= (char)0x21 and <= (char)0x7e) &&
               !value.Contains('\\');
    }

    private static bool IsStrictDescendant(string root, string candidate)
    {
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        var prefix = Path.TrimEndingDirectorySeparator(root) + Path.DirectorySeparatorChar;
        return !PathsEqual(root, candidate) && candidate.StartsWith(prefix, comparison);
    }

    private static bool PathsEqual(string first, string second)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(first)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(second)),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);

    private static OperationResult<T> Failure<T>(string suffix)
        => OperationResult<T>.Fail(
            "Серверный снимок допуска экзамена недоступен или не прошёл строгую проверку.",
            ErrorPrefix + suffix);
}

internal sealed class CertificationProvisioningDescriptor(
    string domainSlug,
    string designPatternSlug,
    string subjectSlug,
    string scopeId,
    string scopeVersion,
    string ownerSnapshotId,
    string ownerSnapshotVersion,
    string approvalRef,
    string snapshotRelativePath,
    string snapshotSha256,
    long snapshotByteLength)
{
    internal string DomainSlug { get; } = domainSlug;
    internal string DesignPatternSlug { get; } = designPatternSlug;
    internal string SubjectSlug { get; } = subjectSlug;
    internal string ScopeId { get; } = scopeId;
    internal string ScopeVersion { get; } = scopeVersion;
    internal string OwnerSnapshotId { get; } = ownerSnapshotId;
    internal string OwnerSnapshotVersion { get; } = ownerSnapshotVersion;
    internal string ApprovalRef { get; } = approvalRef;
    internal string SnapshotRelativePath { get; } = snapshotRelativePath;
    internal string SnapshotSha256 { get; } = snapshotSha256;
    internal long SnapshotByteLength { get; } = snapshotByteLength;
}

internal sealed class ValidatedCertificationProvisioningRecord(
    CertificationProvisioningDescriptor descriptor,
    CodeDocumentationCertificationScopeDeclaration declaration,
    CodeDocumentationCertificationExposureIndex exposureIndex)
{
    internal CertificationProvisioningDescriptor Descriptor { get; } = descriptor;
    internal CodeDocumentationCertificationScopeDeclaration Declaration { get; } = declaration;
    internal CodeDocumentationCertificationExposureIndex ExposureIndex { get; } = exposureIndex;
}

/// <summary>
/// Adapts only immutable records loaded by the strict reader into the existing
/// server-owned authority interface. It does not store mutable current-clean
/// state and never infers historical completeness from the live corpus.
/// </summary>
internal sealed class ServerOwnedCodeDocumentationCertificationAuthorityProvider
    : ICodeDocumentationCertificationAuthorityProvider
{
    private readonly string _trustedRepositoryRoot;
    private readonly IReadOnlyList<ValidatedCertificationProvisioningRecord> _records;

    internal ServerOwnedCodeDocumentationCertificationAuthorityProvider(
        string trustedRepositoryRoot,
        IReadOnlyList<ValidatedCertificationProvisioningRecord> records)
    {
        _trustedRepositoryRoot = trustedRepositoryRoot;
        _records = Array.AsReadOnly(records.ToArray());
    }

    public OperationResult<ServerOwnedCodeDocumentationCertificationScope> Resolve(
        string trustedRepositoryRoot,
        CodeDocumentationRepositoryOwnedModelCheckpointStartRequest startRequest,
        RepositoryOwnedModelCheckpointManifest parent,
        RepositoryOwnedReferenceCorpusOpenResult corpus,
        CodeDocumentationCertificationProfile profile,
        ContinuousLearningTp1DpoConfiguration evaluator)
    {
        if (startRequest is null || parent is null || corpus is null || profile is null || evaluator is null)
            return Failure("INPUT_INVALID");

        if (!PathsEqual(_trustedRepositoryRoot, trustedRepositoryRoot) ||
            !PathsEqual(_trustedRepositoryRoot, startRequest.RepositoryRoot) ||
            !String.Equals(startRequest.DomainSlug, profile.DomainSlug, StringComparison.Ordinal) ||
            !String.Equals(startRequest.DesignPatternSlug, profile.DesignPatternSlug, StringComparison.Ordinal))
            return Failure("ROOT_MISMATCH");

        var record = _records.SingleOrDefault(item =>
            item.Descriptor.DomainSlug == profile.DomainSlug &&
            item.Descriptor.DesignPatternSlug == profile.DesignPatternSlug &&
            item.Descriptor.SubjectSlug == profile.SubjectSlug &&
            item.Descriptor.ScopeId == profile.ScopeId &&
            item.Descriptor.ScopeVersion == profile.ScopeVersion &&
            item.Descriptor.OwnerSnapshotId == profile.OwnerSnapshotId &&
            item.Descriptor.OwnerSnapshotVersion == profile.OwnerSnapshotVersion);
        if (record is null ||
            parent.Domain != profile.DomainSlug ||
            parent.DesignPattern != profile.DesignPatternSlug ||
            record.ExposureIndex.CoveredCheckpointManifestSha256 != parent.ManifestSha256 ||
            corpus.Manifest.Domain != profile.DomainSlug ||
            corpus.Manifest.Subject != profile.SubjectSlug ||
            corpus.Manifest.CorpusVersion != profile.CorpusVersion ||
            corpus.Manifest.ManifestSha256 != profile.ExpectedCorpusManifestSha256 ||
            record.Declaration.Domain != profile.DomainSlug ||
            record.Declaration.DesignPattern != profile.DesignPatternSlug ||
            record.Declaration.Subject != profile.SubjectSlug ||
            record.Declaration.ScopeId != profile.ScopeId ||
            record.Declaration.ScopeVersion != profile.ScopeVersion ||
            record.Declaration.CoverageRevision != profile.CoverageRevision ||
            record.Declaration.CoverageCutUtc != profile.CoverageCutUtc ||
            record.ExposureIndex.Version != profile.ScopeVersion ||
            !record.Declaration.OwnerSnapshots.Any(owner =>
                owner.Id == profile.OwnerSnapshotId && owner.Version == profile.OwnerSnapshotVersion) ||
            record.Declaration.CertificationSourceBindings.Any(binding =>
                binding.OwnerSnapshotId != profile.OwnerSnapshotId ||
                binding.OwnerSnapshotVersion != profile.OwnerSnapshotVersion))
        {
            return Failure("SELECTED_CONTEXT_MISMATCH");
        }

        var candidate = new CodeDocumentationSavedModelAssessmentModelPassport(
            parent.Domain,
            parent.DesignPattern,
            parent.ModelId,
            parent.Version,
            parent.Step,
            parent.RepositoryRelativeManifestPath,
            parent.ManifestSha256,
            parent.RepositoryRelativeWeightsPath,
            parent.WeightsSha256,
            parent.WeightsByteLength,
            startRequest.MaximumCheckpointBytes,
            CodeDocumentationSavedModelAssessmentModelPassport.AcceptedRepositoryCheckpointSourceKind,
            parent.RepositoryRelativeManifestPath,
            parent.ManifestSha256);
        var created = ServerOwnedCodeDocumentationCertificationScope.CreateFromServerOwnedSnapshot(
            record.Declaration,
            candidate,
            corpus,
            profile.GenerationSettings,
            evaluator,
            MeasurementNoisePolicyConfig.Canonical,
            record.ExposureIndex);
        if (!created.IsSuccess || created.Value is null)
            return Failure(created.ErrorCode?.Replace(
                CodeDocumentationCertificationAdmission.ErrorPrefix,
                string.Empty,
                StringComparison.Ordinal) ?? "SCOPE_INVALID");

        var admitted = CodeDocumentationCertificationAdmission.TryCreate(
            created.Value,
            candidate,
            corpus,
            record.ExposureIndex);
        return admitted.IsSuccess
            ? created
            : Failure(admitted.ErrorCode?.Replace(
                CodeDocumentationCertificationAdmission.ErrorPrefix,
                string.Empty,
                StringComparison.Ordinal) ?? "ADMISSION_REJECTED");
    }

    private static OperationResult<ServerOwnedCodeDocumentationCertificationScope> Failure(
        string suffix)
        => OperationResult<ServerOwnedCodeDocumentationCertificationScope>.Fail(
            "Серверный снимок допуска экзамена не соответствует выбранному контексту.",
            CodeDocumentationCertificationProvisioning.ErrorPrefix + suffix);

    private static bool PathsEqual(string first, string second)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(first)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(second)),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);
}
