// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace ContinuousLearning;

using AgentInfrastructure;
using Store = CodeDocumentationCertificationSessionStore;

/// <summary>
/// Internal server-owned handoff for material disclosure. The snapshot is issued
/// from a validated certification scope; callers cannot supply arbitrary keys.
/// </summary>
internal sealed class CodeDocumentationMaterialBoundary
{
    private readonly Store _store;
    private readonly CodeDocumentationMaterialSnapshot _snapshot;

    private CodeDocumentationMaterialBoundary(Store store, CodeDocumentationMaterialSnapshot snapshot)
    {
        _store = store;
        _snapshot = snapshot;
    }

    internal static OperationResult<CodeDocumentationMaterialBoundary> CreateFromServerOwnedSnapshot(
        string trustedRepositoryRoot, string expectedStoreId, Store store,
        CodeDocumentationMaterialSnapshot approvedSnapshot)
    {
        if (store is null || approvedSnapshot is null)
            return Failure<CodeDocumentationMaterialBoundary>("INVALID_PERMIT");

        var binding = store.ValidateOwnerBinding(trustedRepositoryRoot, expectedStoreId);
        if (!binding.IsSuccess)
            return binding.MapError<CodeDocumentationMaterialBoundary>();
        if (!approvedSnapshot.IsOwnedBy(store) || approvedSnapshot.TrustedRepositoryRoot != trustedRepositoryRoot ||
            approvedSnapshot.ExpectedStoreId != expectedStoreId)
            return Failure<CodeDocumentationMaterialBoundary>("IDENTITY_MISMATCH");

        return OperationResult<CodeDocumentationMaterialBoundary>.Ok(new(store, approvedSnapshot));
    }

    internal static OperationResult<CodeDocumentationMaterialBoundary> CreateFromServerOwnedCertificationScope(
        string trustedRepositoryRoot, string expectedStoreId, Store store,
        ServerOwnedCodeDocumentationCertificationScope authority, CertificationExposureRole role)
    {
        if (store is null)
            return Failure<CodeDocumentationMaterialBoundary>("INVALID_PERMIT");
        var snapshot = store.IssueMaterialSnapshot(trustedRepositoryRoot, expectedStoreId, authority, role);
        return snapshot.IsSuccess
            ? CreateFromServerOwnedSnapshot(trustedRepositoryRoot, expectedStoreId, store, snapshot.Value!)
            : snapshot.MapError<CodeDocumentationMaterialBoundary>();
    }

    internal static OperationResult<CodeDocumentationMaterialBoundary>
        CreateFromServerOwnedDiagnosticCorpus(
            string trustedRepositoryRoot,
            string expectedStoreId,
            Store store,
            RepositoryOwnedReferenceCorpusOpenResult corpus)
    {
        if (store is null)
            return Failure<CodeDocumentationMaterialBoundary>("INVALID_PERMIT");
        var snapshot = store.IssueDiagnosticMaterialSnapshot(
            trustedRepositoryRoot,
            expectedStoreId,
            corpus);
        return snapshot.IsSuccess
            ? CreateFromServerOwnedSnapshot(
                trustedRepositoryRoot,
                expectedStoreId,
                store,
                snapshot.Value!)
            : snapshot.MapError<CodeDocumentationMaterialBoundary>();
    }

    internal OperationResult<CodeDocumentationMaterialExposureUse> RecordMaterialExposureAndHoldLease(
        CancellationToken cancellationToken = default)
        => _store.RecordMaterialExposureAndHoldLease(_snapshot, _snapshot.Role, cancellationToken);

    internal OperationResult<CodeDocumentationMaterialExposureUse> BeginDiagnosticDisclosure(
        CancellationToken cancellationToken = default)
        => _snapshot.Role == CertificationExposureRole.TeacherVisibleDiagnostic
            ? RecordMaterialExposureAndHoldLease(cancellationToken)
            : Failure<CodeDocumentationMaterialExposureUse>("INVALID_PERMIT");

    internal bool MatchesAssessmentContext(
        string trustedRepositoryRoot,
        string diagnosticCorpusManifestSha256)
        => _snapshot.Role == CertificationExposureRole.TeacherVisibleDiagnostic &&
           StringComparer.Ordinal.Equals(
               _snapshot.TrustedRepositoryRoot,
               trustedRepositoryRoot) &&
           StringComparer.Ordinal.Equals(
               _snapshot.DiagnosticCorpusManifestSha256,
               diagnosticCorpusManifestSha256);

    internal OperationResult<CodeDocumentationMaterialExposureUse> BeginAuthoringOrTeaching(
        CancellationToken cancellationToken = default)
        => _snapshot.Role == CertificationExposureRole.AuthoringOrTeaching
            ? RecordMaterialExposureAndHoldLease(cancellationToken)
            : Failure<CodeDocumentationMaterialExposureUse>("INVALID_PERMIT");

    private static OperationResult<T> Failure<T>(string suffix)
        => OperationResult<T>.Fail("Серверная граница material exposure не прошла проверку.", Store.ErrorPrefix + suffix);
}

/// <summary>
/// Server-owned issuer for teacher-visible diagnostic material. It owns the
/// session store and derives the boundary from the server-opened corpus; the
/// assessment endpoint never accepts a caller-created purpose or source key.
/// </summary>
internal interface ICodeDocumentationMaterialBoundaryProvider
{
    OperationResult<CodeDocumentationMaterialBoundary>
        IssueTeacherVisibleDiagnostic(
            string trustedRepositoryRoot,
            RepositoryOwnedReferenceCorpusOpenResult corpus);
}

internal sealed class CodeDocumentationServerOwnedMaterialBoundaryProvider
    : ICodeDocumentationMaterialBoundaryProvider
{
    private readonly string _trustedRepositoryRoot;
    private readonly string _expectedStoreId;
    private readonly Store _store;

    private CodeDocumentationServerOwnedMaterialBoundaryProvider(
        string trustedRepositoryRoot,
        string expectedStoreId,
        Store store)
    {
        _trustedRepositoryRoot = trustedRepositoryRoot;
        _expectedStoreId = expectedStoreId;
        _store = store;
    }

    internal static OperationResult<CodeDocumentationServerOwnedMaterialBoundaryProvider>
        Create(
            string trustedRepositoryRoot,
            string expectedStoreId,
            Store store)
    {
        if (string.IsNullOrWhiteSpace(trustedRepositoryRoot) ||
            string.IsNullOrWhiteSpace(expectedStoreId) ||
            store is null)
        {
            return OperationResult<CodeDocumentationServerOwnedMaterialBoundaryProvider>.Fail(
                "Серверный issuer material boundary имеет неполную конфигурацию.",
                Store.ErrorPrefix + "INVALID_PERMIT");
        }

        return OperationResult<CodeDocumentationServerOwnedMaterialBoundaryProvider>.Ok(
            new(trustedRepositoryRoot, expectedStoreId, store));
    }

    public OperationResult<CodeDocumentationMaterialBoundary>
        IssueTeacherVisibleDiagnostic(
            string trustedRepositoryRoot,
            RepositoryOwnedReferenceCorpusOpenResult corpus)
        => StringComparer.Ordinal.Equals(
                trustedRepositoryRoot,
                _trustedRepositoryRoot)
            ? CodeDocumentationMaterialBoundary.CreateFromServerOwnedDiagnosticCorpus(
                _trustedRepositoryRoot,
                _expectedStoreId,
                _store,
                corpus)
            : OperationResult<CodeDocumentationMaterialBoundary>.Fail(
                "Серверный issuer material boundary не совпал с доверенным корнем.",
            Store.ErrorPrefix + "IDENTITY_MISMATCH");
}

/// <summary>
/// DI-created lifetime owner for the automatically composed server-owned
/// material boundary. Explicit providers are deliberately not wrapped here:
/// their caller retains ownership.
/// </summary>
internal sealed class CodeDocumentationServerOwnedMaterialBoundaryLifetime
    : ICodeDocumentationMaterialBoundaryProvider, IDisposable
{
    private readonly ICodeDocumentationMaterialBoundaryProvider _provider;
    private readonly CodeDocumentationCertificationSessionStore _store;
    private int _disposed;

    internal CodeDocumentationServerOwnedMaterialBoundaryLifetime(
        ICodeDocumentationMaterialBoundaryProvider provider,
        CodeDocumentationCertificationSessionStore store)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    internal CodeDocumentationCertificationSessionStore StoreForTesting => _store;

    public OperationResult<CodeDocumentationMaterialBoundary>
        IssueTeacherVisibleDiagnostic(
            string trustedRepositoryRoot,
            RepositoryOwnedReferenceCorpusOpenResult corpus)
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            return OperationResult<CodeDocumentationMaterialBoundary>.Fail(
                "Серверный issuer material boundary уже освобождён.",
                CodeDocumentationCertificationSessionStore.ErrorPrefix +
                "STORAGE_UNAVAILABLE");
        }

        return _provider.IssueTeacherVisibleDiagnostic(
            trustedRepositoryRoot,
            corpus);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            _store.Dispose();
    }
}

/// <summary>Opaque, immutable material binding issued by the current store.</summary>
internal sealed class CodeDocumentationMaterialSnapshot
{
    private readonly Store _owner;

    private CodeDocumentationMaterialSnapshot(Store owner, string trustedRepositoryRoot, string expectedStoreId,
        CertificationExposureRole role, string sourceBindingIdentitySha256,
        string? diagnosticCorpusManifestSha256, IReadOnlyList<Store.ExposureKey> materialExposureKeys)
    {
        _owner = owner;
        TrustedRepositoryRoot = trustedRepositoryRoot;
        ExpectedStoreId = expectedStoreId;
        Role = role;
        SourceBindingIdentitySha256 = sourceBindingIdentitySha256;
        DiagnosticCorpusManifestSha256 = diagnosticCorpusManifestSha256;
        MaterialExposureKeys = new System.Collections.ObjectModel.ReadOnlyCollection<Store.ExposureKey>(
            materialExposureKeys.ToArray());
    }

    internal string TrustedRepositoryRoot { get; }
    internal string ExpectedStoreId { get; }
    internal CertificationExposureRole Role { get; }
    internal string SourceBindingIdentitySha256 { get; }
    internal string? DiagnosticCorpusManifestSha256 { get; }
    internal IReadOnlyList<Store.ExposureKey> MaterialExposureKeys { get; }

    internal bool IsOwnedBy(Store owner) => ReferenceEquals(_owner, owner);

    // Only the store's validated issuer calls this factory; no caller-provided key factory exists.
    internal static CodeDocumentationMaterialSnapshot Issue(Store owner, string trustedRepositoryRoot,
        string expectedStoreId, CertificationExposureRole role, string sourceBindingIdentitySha256,
        string? diagnosticCorpusManifestSha256, IReadOnlyList<Store.ExposureKey> materialExposureKeys)
        => new(owner, trustedRepositoryRoot, expectedStoreId, role, sourceBindingIdentitySha256,
            diagnosticCorpusManifestSha256, materialExposureKeys);
}

/// <summary>Opaque lease-backed capability for one durable material transition.</summary>
internal sealed class CodeDocumentationMaterialExposureUse : IDisposable
{
    private readonly Store _owner;
    private int _disposed;

    private CodeDocumentationMaterialExposureUse(Store owner, string exposureIdentitySha256, string role)
    {
        _owner = owner;
        ExposureIdentitySha256 = exposureIdentitySha256;
        Role = role;
    }

    internal string ExposureIdentitySha256 { get; }
    internal string Role { get; }
    internal bool IsOwnedBy(Store owner) => ReferenceEquals(_owner, owner) && Volatile.Read(ref _disposed) == 0;

    internal static CodeDocumentationMaterialExposureUse Mint(Store owner, string exposureIdentitySha256, string role)
        => new(owner, exposureIdentitySha256, role);

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            _owner.ReleaseMaterialUse(this);
    }
}
