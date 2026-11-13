// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace ContinuousLearning;

using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using LoRA;
using TensorCore.Core;
using TransformerModel;
using TransformerModel.Config;

/// <summary>Detached, untrusted expectations; matching them grants no effect authority.</summary>
internal sealed class CodeDocumentationPreparedBaselineReference
{
    internal CodeDocumentationPreparedBaselineReference(int formatVersion,
        string repositoryRoot, string objectiveSha256, string baselineRoot,
        string baselineRootRelativePath, string baselinePath, string claimPath, string lockPath,
        string requestSha256, string sha256, long byteLength, string classification)
    {
        FormatVersion = formatVersion;
        RepositoryRoot = repositoryRoot;
        ObjectiveSha256 = objectiveSha256;
        BaselineRoot = baselineRoot;
        BaselineRootRelativePath = baselineRootRelativePath;
        BaselinePath = baselinePath;
        ClaimPath = claimPath;
        LockPath = lockPath;
        RequestSha256 = requestSha256;
        Sha256 = sha256;
        ByteLength = byteLength;
        Classification = classification;
    }

    internal int FormatVersion { get; }
    internal string RepositoryRoot { get; }
    internal string ObjectiveSha256 { get; }
    internal string BaselineRoot { get; }
    internal string BaselineRootRelativePath { get; }
    internal string BaselinePath { get; }
    internal string ClaimPath { get; }
    internal string LockPath { get; }
    internal string RequestSha256 { get; }
    internal string Sha256 { get; }
    internal long ByteLength { get; }
    internal string Classification { get; }
}

/// <summary>Two detached scalar expectations; neither field authenticates owner evidence.</summary>
internal sealed class CodeDocumentationPreparedBaselineExternalReference
{
    internal CodeDocumentationPreparedBaselineExternalReference(string receiptId, string receiptSha256)
    {
        ReceiptId = receiptId; // Preserve the supplied identifier as an untrusted scalar.
        ReceiptSha256 = receiptSha256; // Preserve the supplied original-byte digest without normalization.
    }

    internal string ReceiptId { get; } // Expose only the detached domain-bound identifier.
    internal string ReceiptSha256 { get; } // Expose only the detached original baseline digest.
}

/// <summary>Engineering readback only; it grants no training or publication effect.</summary>
internal sealed class CodeDocumentationPreparedBaselineReadback
{
    internal CodeDocumentationPreparedBaselineReadback(object issuer,
        RepositoryOwnedPreparedBaselineAddress address, string requestSha256,
        string sha256, long byteLength,
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality baseline,
        bool needsLesson)
    {
        Issuer = issuer;
        Address = address;
        RequestSha256 = requestSha256;
        Sha256 = sha256;
        ByteLength = byteLength;
        Baseline = baseline;
        Classification = needsLesson ? "NEEDS_LESSON" : "NO_TRAINING_NEEDED";
        Reference = new(CodeDocumentationRepositoryOwnedPreparedBaselineJournal.FormatVersion,
            address.RepositoryRoot, address.ObjectiveSha256, address.BaselineRoot,
            address.BaselineRootRelativePath, address.BaselinePath, address.ClaimPath, address.LockPath,
            requestSha256, sha256, byteLength, Classification);
    }

    private object Issuer { get; }
    internal bool WasIssuedBy(object issuer) => ReferenceEquals(Issuer, issuer);
    internal RepositoryOwnedPreparedBaselineAddress Address { get; }
    internal string RequestSha256 { get; }
    internal string Sha256 { get; }
    internal long ByteLength { get; }
    internal CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality Baseline { get; }
    internal string Classification { get; }
    internal CodeDocumentationPreparedBaselineReference Reference { get; }
}

/// <summary>
/// Original producer and durable transaction. An uncertain Started claim is
/// never retried: a completed transaction can be reopened without measuring.
/// </summary>
internal sealed class CodeDocumentationRepositoryOwnedPreparedBaselineJournal
{
    internal const int FormatVersion = 1;
    internal const int MaximumBaselineBytes = 16 * 1024 * 1024;
    internal const string InvalidErrorCode = "PREPARED_BASELINE_INVALID";
    internal const string RecoveryRequiredErrorCode = "PREPARED_BASELINE_RECOVERY_REQUIRED";
    internal const string BudgetErrorCode = "PREPARED_BASELINE_BUDGET_EXCEEDED";
    internal const string BusyErrorCode = "PREPARED_BASELINE_BUSY";
    private const int MaximumClaimBytes = 256 * 1024;
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Gates = new(
        OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        MaxDepth = 32,
    };
    private readonly object _issuer = new();
    // Internal observation seams never accept a baseline/model as authority.
    private readonly Action<LlmModel>? _modelLoaded;
    private readonly Action<string>? _durablePoint;

    internal CodeDocumentationRepositoryOwnedPreparedBaselineJournal(
        Action<LlmModel>? modelLoaded = null, Action<string>? durablePoint = null)
    {
        _modelLoaded = modelLoaded;
        _durablePoint = durablePoint;
    }

    internal Task<OperationResult<CodeDocumentationPreparedBaselineReadback>> ProduceOrOpenAsync(
        CodeDocumentationOneLessonGrantCatalog catalog,
        CodeDocumentationOneLessonSelectorV1 selector,
        string repositoryRoot, string domain, string pattern, string modelId, string version,
        ModelConfig config, RepositoryOwnedModelRuntimeCompatibility compatibility,
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context,
        LoraConfig? lora = null, LoraPairRandomSeedAuthority? adapterSeeds = null,
        CancellationToken cancellationToken = default,
        CodeDocumentationPreparedBaselineReadback? requiredReadback = null)
        // The original producer alone may enter first production; its defaults stay unchanged.
        => OpenCoreAsync(catalog, selector, repositoryRoot, domain, pattern, modelId, version,
            config, compatibility, context, lora, adapterSeeds, cancellationToken, requiredReadback,
            allowCreate: true);

    /// <summary>Reopens committed owner evidence without creating or measuring a baseline.</summary>
    internal Task<OperationResult<CodeDocumentationPreparedBaselineReadback>> OpenExistingAsync(
        CodeDocumentationOneLessonGrantCatalog catalog,
        CodeDocumentationOneLessonSelectorV1 selector,
        string repositoryRoot, string domain, string pattern, string modelId, string version,
        ModelConfig config, RepositoryOwnedModelRuntimeCompatibility compatibility,
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context,
        LoraConfig? lora = null, LoraPairRandomSeedAuthority? adapterSeeds = null,
        CancellationToken cancellationToken = default)
        // Fresh owners authenticate disk bytes; no prior owner-issued graph is required.
        => OpenCoreAsync(catalog, selector, repositoryRoot, domain, pattern, modelId, version,
            config, compatibility, context, lora, adapterSeeds, cancellationToken,
            requiredReadback: null, allowCreate: false);

    /// <summary>Ordinary startup entry: recover candidate request anchors only inside the original owner gate.</summary>
    internal async Task<OperationResult<OrdinaryOriginal>> ProduceOrOpenOrdinaryAsync(
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector,
        string repositoryRoot, string domain, string pattern, string modelId, string version,
        ModelConfig config, RepositoryOwnedModelRuntimeCompatibility compatibility,
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context,
        LoraConfig? lora, LoraPairRandomSeedAuthority? seeds, CancellationToken cancellationToken)
    {
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext? captured = null;
        var opened = await OpenCoreAsync(catalog, selector, repositoryRoot, domain, pattern, modelId,
            version, config, compatibility, context, lora, seeds, cancellationToken, null, true,
            ordinaryCapture: value => captured = value).ConfigureAwait(false);
        if (!opened.IsSuccess || opened.Value is null) return opened.MapError<OrdinaryOriginal>();
        if (captured is null || !IsIssuedReadback(opened.Value))
            return OperationResult<OrdinaryOriginal>.Fail("Original context capture was not owner issued.", InvalidErrorCode);
        return OperationResult<OrdinaryOriginal>.Ok(OrdinaryOriginal.Mint(this, opened.Value, captured));
    }

    internal sealed class OrdinaryOriginal
    {
        private OrdinaryOriginal(CodeDocumentationPreparedBaselineReadback readback,
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context)
        { Readback = readback; Context = context; }
        internal CodeDocumentationPreparedBaselineReadback Readback { get; }
        internal CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext Context { get; }
        internal static OrdinaryOriginal Mint(CodeDocumentationRepositoryOwnedPreparedBaselineJournal owner,
            CodeDocumentationPreparedBaselineReadback readback,
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context)
        {
            if (!owner.IsIssuedReadback(readback)) throw new InvalidOperationException("Foreign original readback.");
            return new(readback, context);
        }
    }

    /// <summary>Uses the same ordinary anchor capture, but cannot create or calibrate.</summary>
    internal async Task<OperationResult<OrdinaryOriginal>> OpenExistingOrdinaryAsync( // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        string repositoryRoot, string domain, string pattern, string modelId, string version, // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        ModelConfig config, RepositoryOwnedModelRuntimeCompatibility compatibility, // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context, // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        LoraConfig? lora, LoraPairRandomSeedAuthority? seeds, CancellationToken cancellationToken) // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
    { // Begin reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext? captured=null; // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        var opened=await OpenCoreAsync(catalog,selector,repositoryRoot,domain,pattern,modelId,version, // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            config,compatibility,context,lora,seeds,cancellationToken,null,allowCreate:false, // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            ordinaryCapture:value=>captured=value).ConfigureAwait(false); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        if(!opened.IsSuccess || opened.Value is null) return opened.MapError<OrdinaryOriginal>(); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        if(captured is null || !IsIssuedReadback(opened.Value)) // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            return OperationResult<OrdinaryOriginal>.Fail("Existing original context was not owner issued.",InvalidErrorCode); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        return OperationResult<OrdinaryOriginal>.Ok(OrdinaryOriginal.Mint(this,opened.Value,captured)); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
    } // Finish reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.

    /// <summary>Fail-only saved candidate/claim check before an evaluation factory; grants no baseline.</summary>
    internal async Task<OperationResult<bool>> InspectExistingOrdinaryExternalReferenceAsync( // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        CodeDocumentationOneLessonGrantCatalog catalog,CodeDocumentationOneLessonSelectorV1 selector, // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        string repositoryRoot,CodeDocumentationPreparedBaselineExternalReference expected,CancellationToken token) // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
    { // Begin reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        var resolved=catalog.Resolve(selector); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        if(!resolved.IsSuccess || resolved.Value?.CheckpointSelection?.Parent is not {} parent) // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            return Fail(RecoveryRequiredErrorCode).MapError<bool>(); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        var addressResult=RepositoryOwnedTrainingArtifactLayout.CreatePreparedBaselineAddress(repositoryRoot, // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            parent.DomainSlug,parent.DesignPatternSlug,parent.ModelIdSlug,parent.ModelVersion, // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            Hash(Encoding.UTF8.GetBytes(resolved.Value.ObjectiveId))); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        if(!addressResult.IsSuccess || addressResult.Value is null) return addressResult.MapError<bool>(); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        var address=addressResult.Value; // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        if(!Directory.Exists(address.BaselineRoot)) return Fail(RecoveryRequiredErrorCode).MapError<bool>(); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        var gate=Gates.GetOrAdd(address.BaselineRoot,_=>new SemaphoreSlim(1,1)); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        if(!await gate.WaitAsync(0,token).ConfigureAwait(false)) return Fail(BusyErrorCode).MapError<bool>(); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        try // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        { // Begin reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            token.ThrowIfCancellationRequested(); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            EnsureJournalDirectories(address,false); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            using var ownerLock=OpenOwnerLock(address.LockPath,false); ownerLock.Lock(0,1); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            var claims=Read(address.ClaimPath,MaximumClaimBytes); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            if(claims.Contains((byte)'\r')) return Fail(RecoveryRequiredErrorCode).MapError<bool>(); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            var lines=new UTF8Encoding(false,true).GetString(claims).Split('\n'); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            if(lines.Length!=3 || lines[2].Length!=0) return Fail(RecoveryRequiredErrorCode).MapError<bool>(); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            var startedBytes=Encoding.UTF8.GetBytes(lines[0]); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            var started=Deserialize<StartedClaim>(startedBytes); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            var committed=Deserialize<CommittedClaim>(Encoding.UTF8.GetBytes(lines[1])); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            var raw=Read(address.BaselinePath,MaximumBaselineBytes); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            var record=Deserialize<BaselineRecord>(raw); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            if(started.FormatVersion!=FormatVersion || started.Kind!="Started" || committed.FormatVersion!=FormatVersion || // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
                committed.Kind!="Committed" || record.FormatVersion!=FormatVersion || !IsHash(record.RequestSha256) || // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
                started.RequestSha256!=record.RequestSha256 || committed.RequestSha256!=record.RequestSha256 || // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
                committed.StartedSha256!=Hash(startedBytes) || committed.BaselineByteLength!=raw.LongLength || // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
                committed.BaselineSha256!=Hash(raw)) return Fail(RecoveryRequiredErrorCode).MapError<bool>(); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            var candidate=CreateExternalReferenceExpectation(address.ObjectiveSha256,record.RequestSha256,Hash(raw),raw.LongLength); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            if(!candidate.IsSuccess || candidate.Value is null || candidate.Value.ReceiptId!=expected.ReceiptId || // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
                candidate.Value.ReceiptSha256!=expected.ReceiptSha256 || // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
                !Read(address.ClaimPath,MaximumClaimBytes).AsSpan().SequenceEqual(claims) || // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
                !Read(address.BaselinePath,MaximumBaselineBytes).AsSpan().SequenceEqual(raw)) // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
                return Fail(RecoveryRequiredErrorCode).MapError<bool>(); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            token.ThrowIfCancellationRequested(); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
            return OperationResult<bool>.Ok(true); // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        } // Finish reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        catch(OperationCanceledException) {throw;} // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        catch(Exception ex) when(SnapshotExceptionGraphClassifier.IsNonFatal(ex)) // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        {return Fail(RecoveryRequiredErrorCode).MapError<bool>();} // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
        finally {gate.Release();} // Reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.
    } // Finish reuse existing ordinary anchor capture and claim custody while forbidding creation or recalibration.

    /// <summary>Formats a bounded detached scalar expectation without reading or authenticating disk evidence.</summary>
    internal static OperationResult<CodeDocumentationPreparedBaselineExternalReference> CreateExternalReferenceExpectation(
        string objectiveSha256, string requestSha256, string baselineSha256, long byteLength)
    {
        if (!IsLowerAsciiHex(objectiveSha256, 64) || !IsLowerAsciiHex(requestSha256, 64) ||
            !IsLowerAsciiHex(baselineSha256, 64) || byteLength is < 1 or > MaximumBaselineBytes) // Keep the exact scalar grammar and original byte cap.
            return OperationResult<CodeDocumentationPreparedBaselineExternalReference>.Fail(
                "Prepared baseline external expectation requires exact bounded scalars.", InvalidErrorCode); // Invalid scalars never become normalized authority.
        var preimage = "xiigen-s04-original-baseline-reference-v2\n" + objectiveSha256 + "\n" +
            requestSha256 + "\n" + baselineSha256 + "\n" + byteLength.ToString(CultureInfo.InvariantCulture) + "\n"; // Include the final LF in the distinct ORIGINAL identity domain.
        var digest = Hash(Encoding.UTF8.GetBytes(preimage)); // Hash only the bounded scalar preimage, without storage or graph access.
        return OperationResult<CodeDocumentationPreparedBaselineExternalReference>.Ok(
            new(digest[..32], baselineSha256)); // Keep the first sixteen digest bytes and the full original raw-byte SHA256.
    }

    /// <summary>Projects a genuine same-owner readback without inspecting its mutable quality graph.</summary>
    internal OperationResult<CodeDocumentationPreparedBaselineExternalReference> GetExternalReference(
        CodeDocumentationPreparedBaselineReadback? readback)
    {
        if (!IsIssuedReadback(readback)) // A copied graph or foreign issuer cannot create a genuine projection.
            return OperationResult<CodeDocumentationPreparedBaselineExternalReference>.Fail(
                "Prepared baseline external projection requires this owner's readback.", InvalidErrorCode); // Preserve expected business refusal without throwing.
        return CreateExternalReferenceExpectation(readback!.Address.ObjectiveSha256,
            readback.RequestSha256, readback.Sha256, readback.ByteLength); // Use immutable issued scalars; this is not fresh disk authentication.
    }

    /// <summary>Authenticates detached expectations through exactly one current existing-only owner read.</summary>
    internal async Task<OperationResult<CodeDocumentationPreparedBaselineReadback>> OpenExistingByExternalReferenceAsync(
        CodeDocumentationPreparedBaselineExternalReference? expectedReference,
        CodeDocumentationOneLessonGrantCatalog catalog,
        CodeDocumentationOneLessonSelectorV1 selector,
        string repositoryRoot, string domain, string pattern, string modelId, string version,
        ModelConfig config, RepositoryOwnedModelRuntimeCompatibility compatibility,
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context,
        LoraConfig? lora = null, LoraPairRandomSeedAuthority? adapterSeeds = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested(); // Cancellation dominates even a malformed or absent expectation.
        var receiptId = expectedReference?.ReceiptId; // Copy the detached identifier once before opening lower evidence.
        var receiptSha256 = expectedReference?.ReceiptSha256; // Copy the detached digest once without interpreting it as a path.
        if (!IsLowerAsciiHex(receiptId, 32) || !IsLowerAsciiHex(receiptSha256, 64)) // Reject syntax before catalog, disk, model or lower-owner access.
            return Fail(InvalidErrorCode); // Malformed external expectations do not enter the original producer.
        var opened = await OpenExistingAsync(catalog, selector, repositoryRoot, domain, pattern,
            modelId, version, config, compatibility, context, lora, adapterSeeds, cancellationToken)
            .ConfigureAwait(false); // Forward every trusted binding to this owner exactly once with allowCreate:false.
        if (!opened.IsSuccess) return opened; // Preserve the lower owner's exact error and cancellation behavior.
        var fresh = opened.Value; // The lower owner supplies the full graph freshly authenticated from disk.
        if (fresh is null || !IsIssuedReadback(fresh)) return Fail(InvalidErrorCode); // Success must carry this receiving owner's nonce.
        var projected = GetExternalReference(fresh); // Recompute both scalar identities from immutable fresh readback fields.
        if (!projected.IsSuccess || projected.Value is null ||
            !StringComparer.Ordinal.Equals(receiptId, projected.Value.ReceiptId) ||
            !StringComparer.Ordinal.Equals(receiptSha256, projected.Value.ReceiptSha256)) // Both external expectations must match without normalization.
            return Fail(InvalidErrorCode); // A syntactically valid mismatch grants no effect and returns no graph.
        return opened; // Return the actual fresh lower graph, never the caller's mutable evidence.
    }

    private static bool IsLowerAsciiHex(string? value, int width)
        => value is not null && value.Length == width &&
            value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f'); // Admit only the exact lowercase ASCII wire widths.

    /// <summary>Compares untrusted scalars with a fresh, fully validated original disk result.</summary>
    internal async Task<OperationResult<CodeDocumentationPreparedBaselineReadback>> OpenExistingByReferenceAsync(
        CodeDocumentationPreparedBaselineReference? expectedReference,
        CodeDocumentationOneLessonGrantCatalog catalog,
        CodeDocumentationOneLessonSelectorV1 selector,
        string repositoryRoot, string domain, string pattern, string modelId, string version,
        ModelConfig config, RepositoryOwnedModelRuntimeCompatibility compatibility,
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context,
        LoraConfig? lora = null, LoraPairRandomSeedAuthority? adapterSeeds = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!ValidReference(expectedReference)) return Fail(InvalidErrorCode);
        var expected = expectedReference!;
        var resolved = catalog.Resolve(selector);
        if (!resolved.IsSuccess || resolved.Value is null)
            return resolved.MapError<CodeDocumentationPreparedBaselineReadback>();
        // Only current trusted inputs derive the path. Supplied coordinates are never opened or repaired.
        var address = RepositoryOwnedTrainingArtifactLayout.CreatePreparedBaselineAddress(
            repositoryRoot, domain, pattern, modelId, version,
            Hash(Encoding.UTF8.GetBytes(resolved.Value.ObjectiveId)));
        if (!address.IsSuccess || address.Value is null)
            return address.MapError<CodeDocumentationPreparedBaselineReadback>();
        if (!ReferenceMatchesAddress(expected, address.Value)) return Fail(InvalidErrorCode);
        var opened = await OpenExistingAsync(catalog, selector, repositoryRoot, domain, pattern,
            modelId, version, config, compatibility, context, lora, adapterSeeds, cancellationToken)
            .ConfigureAwait(false);
        if (!opened.IsSuccess) return opened;
        var fresh = opened.Value;
        if (fresh is null || !IsIssuedReadback(fresh) ||
            !ReferenceMatchesAddress(expected, fresh.Address) ||
            expected.FormatVersion != fresh.Reference.FormatVersion ||
            !StringComparer.Ordinal.Equals(expected.RequestSha256, fresh.RequestSha256) ||
            !StringComparer.Ordinal.Equals(expected.Sha256, fresh.Sha256) ||
            expected.ByteLength != fresh.ByteLength ||
            !StringComparer.Ordinal.Equals(expected.Classification, fresh.Classification))
            return Fail(InvalidErrorCode);
        return opened;
    }

    private static bool ValidReference(CodeDocumentationPreparedBaselineReference? reference)
        => reference is not null && reference.FormatVersion == FormatVersion &&
            reference.ObjectiveSha256 is not null && IsHash(reference.ObjectiveSha256) &&
            reference.RequestSha256 is not null && IsHash(reference.RequestSha256) &&
            reference.Sha256 is not null && IsHash(reference.Sha256) &&
            reference.ByteLength is >= 1 and <= MaximumBaselineBytes &&
            reference.Classification is "NEEDS_LESSON" or "NO_TRAINING_NEEDED" &&
            ValidCoordinate(reference.RepositoryRoot) && ValidCoordinate(reference.BaselineRoot) &&
            ValidCoordinate(reference.BaselineRootRelativePath) && ValidCoordinate(reference.BaselinePath) &&
            ValidCoordinate(reference.ClaimPath) && ValidCoordinate(reference.LockPath);

    private static bool ValidCoordinate(string? value)
        => !string.IsNullOrWhiteSpace(value) && !value.Any(char.IsControl);

    private static bool ReferenceMatchesAddress(CodeDocumentationPreparedBaselineReference expected,
        RepositoryOwnedPreparedBaselineAddress address)
    {
        var paths = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        return paths.Equals(expected.RepositoryRoot, address.RepositoryRoot) &&
            StringComparer.Ordinal.Equals(expected.ObjectiveSha256, address.ObjectiveSha256) &&
            paths.Equals(expected.BaselineRoot, address.BaselineRoot) &&
            StringComparer.Ordinal.Equals(expected.BaselineRootRelativePath, address.BaselineRootRelativePath) &&
            paths.Equals(expected.BaselinePath, address.BaselinePath) &&
            paths.Equals(expected.ClaimPath, address.ClaimPath) && paths.Equals(expected.LockPath, address.LockPath);
    }

    private async Task<OperationResult<CodeDocumentationPreparedBaselineReadback>> OpenCoreAsync(
        CodeDocumentationOneLessonGrantCatalog catalog,
        CodeDocumentationOneLessonSelectorV1 selector,
        string repositoryRoot, string domain, string pattern, string modelId, string version,
        ModelConfig config, RepositoryOwnedModelRuntimeCompatibility compatibility,
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context,
        LoraConfig? lora, LoraPairRandomSeedAuthority? adapterSeeds,
        CancellationToken cancellationToken, CodeDocumentationPreparedBaselineReadback? requiredReadback,
        bool allowCreate,
        Action<CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext>? ordinaryCapture = null)
    {
        if (requiredReadback is not null && !IsIssuedReadback(requiredReadback))
            return Fail(InvalidErrorCode);
        var resolved = catalog.Resolve(selector);
        if (!resolved.IsSuccess || resolved.Value is null)
            return resolved.MapError<CodeDocumentationPreparedBaselineReadback>();
        var grant = resolved.Value;
        if (config is null || compatibility is null ||
            !StringComparer.Ordinal.Equals(compatibility.ConfigSha256,
                TrainingSourceProvenance.Identify(config).Sha256))
            return Fail(InvalidErrorCode);
        // Reject known-invalid inputs before creating immutable owner history.
        // Calibration and this preflight share the same pure context validator.
        var contextValidation = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
            .ValidateContext(context);
        if (!contextValidation.IsSuccess)
            return contextValidation.MapError<CodeDocumentationPreparedBaselineReadback>();
        if (ordinaryCapture is not null && context.MaximumDuration !=
            CodeDocumentationSavedModelAssessmentCalibrationService.DefaultMaximumDuration)
            return Fail(InvalidErrorCode);
        var plan = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.PlanQualityGate(
            context.Corpus, context.GenerationSettings, context.EvaluatorConfiguration,
            context.NoisePolicyConfig);
        if (!plan.IsSuccess || plan.Value is null)
            return plan.MapError<CodeDocumentationPreparedBaselineReadback>();
        var n = plan.Value.BaselinePlan.ExpectedEvaluationCallCount;
        var cases = context.Corpus.Entries.Where(e => e.Partition == FrozenReferencePartition.HeldOut)
            .OrderBy(e => e.EntryId, StringComparer.Ordinal).ToArray();
        try
        {
            if (cases.Length is < 1 or > 200 || context.NoisePolicyConfig.RepeatCount < 2 ||
                n != checked(cases.Length * context.NoisePolicyConfig.RepeatCount) || n > 5000 ||
                catalog.MaxTeacherCalls is < 1 or > 10000 ||
                checked(2 * n) > catalog.MaxTeacherCalls ||
                checked(2 * n) > context.EvaluatorConfiguration.MaxTeacherCalls ||
                grant.FormatVersion != 2 || grant.DescriptorsV2 is not { } descriptors ||
                descriptors.CaseCount != cases.Length ||
                descriptors.RepeatCount != context.NoisePolicyConfig.RepeatCount ||
                !FitsCompleteRecord(cases.Select(e => e.EntryId).ToArray(),
                    context.NoisePolicyConfig.RepeatCount))
                return Fail(BudgetErrorCode);
        }
        catch (OverflowException) { return Fail(BudgetErrorCode); }

        var addressResult = RepositoryOwnedTrainingArtifactLayout.CreatePreparedBaselineAddress(
            repositoryRoot, domain, pattern, modelId, version,
            Hash(Encoding.UTF8.GetBytes(grant.ObjectiveId)));
        if (!addressResult.IsSuccess || addressResult.Value is null)
            return addressResult.MapError<CodeDocumentationPreparedBaselineReadback>();
        var address = addressResult.Value;
        // Missing owner history is an existing-only refusal before any gate or model access.
        if (!allowCreate && !Directory.Exists(address.BaselineRoot)) return Fail(InvalidErrorCode);
        var gate = Gates.GetOrAdd(address.BaselineRoot, _ => new SemaphoreSlim(1, 1));
        if (!await gate.WaitAsync(0, cancellationToken).ConfigureAwait(false))
            return Fail(BusyErrorCode);
        LlmModel? model = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureJournalDirectories(address, allowCreate); // Provision only missing journal-owned directories; existing custody is never repaired.
            using var ownerLock = OpenOwnerLock(address.LockPath, allowCreate); // Existing-only readers cannot bootstrap an absent lock.
            ownerLock.Lock(0, 1);
            var loaded = new RepositoryOwnedModelCheckpointLoader().LoadLatestCompatible(
                repositoryRoot, domain, pattern, modelId, version, compatibility);
            if (!loaded.IsSuccess || loaded.Value is null)
                return loaded.MapError<CodeDocumentationPreparedBaselineReadback>();
            using var checkpoint = loaded.Value;
            var parent = checkpoint.Manifest;
            if (!StringComparer.Ordinal.Equals(grant.ParentManifestSha256, parent.ManifestSha256) ||
                !StringComparer.Ordinal.Equals(grant.ParentWeightsSha256, checkpoint.WeightsSha256))
                return Fail(InvalidErrorCode);
            var corpusAddress = RepositoryOwnedTrainingArtifactLayout.CreateReferenceCorpusAddress(
                repositoryRoot, context.CorpusManifest.Domain, context.CorpusManifest.Subject,
                context.CorpusManifest.CorpusVersion);
            if (!corpusAddress.IsSuccess || corpusAddress.Value is null)
                return Fail(InvalidErrorCode);
            var reopenedCorpus = RepositoryOwnedReferenceCorpusManifestSealer.OpenSealed(corpusAddress.Value);
            if (!reopenedCorpus.IsSuccess || reopenedCorpus.Value is null ||
                !Equals(reopenedCorpus.Value.Manifest, context.CorpusManifest) ||
                !StringComparer.Ordinal.Equals(reopenedCorpus.Value.Corpus.CorpusSha256,
                    context.Corpus.CorpusSha256))
                return Fail(InvalidErrorCode);
            context = context with { Corpus = reopenedCorpus.Value.Corpus,
                CorpusManifest = reopenedCorpus.Value.Manifest };
            // Admission comes from the reopened owner's entries, never the
            // caller's copied hash paired with a shortened Entries collection.
            var actualCases = context.Corpus.Entries
                .Where(e => e.Partition == FrozenReferencePartition.HeldOut)
                .OrderBy(e => e.EntryId, StringComparer.Ordinal).ToArray();
            if (!actualCases.Select(e => e.EntryId).SequenceEqual(
                    cases.Select(e => e.EntryId), StringComparer.Ordinal))
                return Fail(InvalidErrorCode);
            plan = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.PlanQualityGate(
                context.Corpus, context.GenerationSettings, context.EvaluatorConfiguration,
                context.NoisePolicyConfig);
            if (!plan.IsSuccess || plan.Value is null)
                return plan.MapError<CodeDocumentationPreparedBaselineReadback>();
            n = checked(actualCases.Length * context.NoisePolicyConfig.RepeatCount);
            if (actualCases.Length is < 1 or > 200 || n > 5000 ||
                n != plan.Value.BaselinePlan.ExpectedEvaluationCallCount ||
                checked(2 * n) > catalog.MaxTeacherCalls ||
                checked(2 * n) > context.EvaluatorConfiguration.MaxTeacherCalls ||
                grant.DescriptorsV2!.CaseCount != actualCases.Length ||
                grant.DescriptorsV2.RepeatCount != context.NoisePolicyConfig.RepeatCount ||
                !FitsCompleteRecord(actualCases.Select(e => e.EntryId).ToArray(),
                    context.NoisePolicyConfig.RepeatCount)) return Fail(BudgetErrorCode);
            var passport = new CodeDocumentationSavedModelAssessmentModelPassport(
                parent.Domain, parent.DesignPattern, parent.ModelId, parent.Version, parent.Step,
                parent.RepositoryRelativeManifestPath, parent.ManifestSha256,
                parent.RepositoryRelativeWeightsPath, checkpoint.WeightsSha256, checkpoint.WeightsByteLength,
                checkpoint.WeightsByteLength, RepositoryRelativeModelIdentityPath:
                    parent.RepositoryRelativeManifestPath, ModelIdentitySha256: parent.ManifestSha256);
            if (ordinaryCapture is not null && (File.Exists(address.ClaimPath) || File.Exists(address.BaselinePath)))
            {
                // These persisted values are candidate anchors, never authority. Reopen below still
                // authenticates Started, Committed, exact raw hash/length, RequestHash and the full exam.
                var raw = Read(address.BaselinePath, MaximumBaselineBytes);
                var candidate = Deserialize<BaselineRecord>(raw);
                var utc = candidate.Baseline.Calibration.Calibration.MeasurementNoisePassport.CalibratedAtUtc;
                if (utc.Offset != TimeSpan.Zero || context.MaximumDuration !=
                    CodeDocumentationSavedModelAssessmentCalibrationService.DefaultMaximumDuration)
                    return Fail(RecoveryRequiredErrorCode);
                context = context with { ObservedAtUtc = utc,
                    EvidenceRefs = Array.AsReadOnly(candidate.Baseline.Calibration.RepresentativeAssessment.EvidenceRefs.ToArray()) };
            }
            else if (ordinaryCapture is not null)
            {
                context = context with { ObservedAtUtc = DateTimeOffset.UtcNow,
                    EvidenceRefs = Array.AsReadOnly(context.EvidenceRefs.ToArray()) };
            }
            var requestHash = RequestHash(grant, catalog.MaxTeacherCalls, compatibility,
                context, passport, lora, adapterSeeds);
            var runId = "prepared-baseline:" + address.ObjectiveSha256;
            if (requiredReadback is not null &&
                (requiredReadback.Address.BaselineRoot != address.BaselineRoot ||
                 requiredReadback.RequestSha256 != requestHash)) return Fail(InvalidErrorCode);
            if (File.Exists(address.ClaimPath) || File.Exists(address.BaselinePath))
            {
                // Validation also needs the actual model serialization identity.
                model = LoadModel(checkpoint, config, lora, adapterSeeds, cancellationToken);
                var opened = Reopen(address, requestHash, model, passport, context, plan.Value.BaselinePlan, runId);
                if (requiredReadback is not null && (!opened.IsSuccess || opened.Value is null ||
                    opened.Value.Sha256 != requiredReadback.Sha256 ||
                    opened.Value.ByteLength != requiredReadback.ByteLength)) return Fail(RecoveryRequiredErrorCode);
                if (opened.IsSuccess) ordinaryCapture?.Invoke(context);
                return opened;
            }
            if (requiredReadback is not null) return Fail(RecoveryRequiredErrorCode);
            // This refusal dominates every first-production hook, write and calibration below.
            // A valid root/lock with no history cannot instruct a reader to produce new evidence.
            if (!allowCreate) return Fail(InvalidErrorCode);
            model = LoadModel(checkpoint, config, lora, adapterSeeds, cancellationToken);
            _modelLoaded?.Invoke(model);
            var state = CaptureState(model, compatibility, lora, adapterSeeds);
            var started = new StartedClaim(FormatVersion, "Started", requestHash, checked(2 * n));
            var startedBytes = Serialize(started);
            CreateNew(address.ClaimPath, AppendNewline(startedBytes));
            _durablePoint?.Invoke("Started");
            OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality> measured;
            try
            {
                measured = await new CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService()
                    .CalibrateBaselineAsync(context, model, passport, runId, cancellationToken)
                    .ConfigureAwait(false);
            }
            finally
            {
                if (!state.AsSpan().SequenceEqual(CaptureState(model, compatibility, lora, adapterSeeds)))
                    throw new InvalidDataException("Prepared baseline inference changed model state.");
            }
            if (!measured.IsSuccess || measured.Value is null)
                return ordinaryCapture is null ? Fail(RecoveryRequiredErrorCode) :
                    OperationResult<CodeDocumentationPreparedBaselineReadback>.Fail(
                        "Prepared baseline calibration did not complete: " + measured.ErrorCode +
                        ". Owner history is preserved without remeasurement. " + measured.ErrorMessage, RecoveryRequiredErrorCode); // Preserve the assessment owner's safe diagnostic detail without granting a retry.
            if (!ValidBaseline(measured.Value, model, passport, context, plan.Value.BaselinePlan, runId))
                return ordinaryCapture is null ? Fail(RecoveryRequiredErrorCode) :
                    OperationResult<CodeDocumentationPreparedBaselineReadback>.Fail(
                        "Prepared baseline calibration did not match the exact current exam. Owner history is preserved without remeasurement.",
                        RecoveryRequiredErrorCode);
            var record = new BaselineRecord(FormatVersion, requestHash, passport, measured.Value);
            var bytes = Serialize(record);
            if (bytes.Length is < 1 or > MaximumBaselineBytes)
                return Fail(RecoveryRequiredErrorCode);
            CreateNew(address.BaselinePath, bytes);
            if (!bytes.AsSpan().SequenceEqual(Read(address.BaselinePath, MaximumBaselineBytes)))
                return Fail(RecoveryRequiredErrorCode);
            _durablePoint?.Invoke("BaselinePersisted");
            var committed = new CommittedClaim(FormatVersion, "Committed", requestHash,
                Hash(startedBytes), Hash(bytes), bytes.LongLength);
            using (var claim = Open(address.ClaimPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var existing = SecureLocalFileBoundary.ReadBoundedStableBytes(claim, MaximumClaimBytes);
                if (existing is null || !existing.AsSpan().SequenceEqual(AppendNewline(startedBytes)))
                    return Fail(RecoveryRequiredErrorCode);
                claim.Position = claim.Length;
                claim.Write(AppendNewline(Serialize(committed)));
                claim.Flush(true);
                CheckHandle(claim, address.ClaimPath);
            }
            _durablePoint?.Invoke("Committed");
            var fresh = Reopen(address, requestHash, model, passport, context, plan.Value.BaselinePlan, runId);
            if (fresh.IsSuccess) ordinaryCapture?.Invoke(context);
            return fresh;
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Fail(File.Exists(address.ClaimPath) || File.Exists(address.BaselinePath)
                ? RecoveryRequiredErrorCode : InvalidErrorCode);
        }
        finally
        {
            // LlmModel has no IDisposable contract and is not retained by this owner.
            model = null;
            gate.Release();
        }
    }

    internal bool IsIssuedReadback(CodeDocumentationPreparedBaselineReadback? readback)
        => readback is not null && readback.WasIssuedBy(_issuer);

    internal Task<OperationResult<CodeDocumentationPreparedBaselineReadback>> ReopenIssuedAsync(
        CodeDocumentationPreparedBaselineReadback readback,
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector,
        string repositoryRoot, string domain, string pattern, string modelId, string version,
        ModelConfig config, RepositoryOwnedModelRuntimeCompatibility compatibility,
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context,
        LoraConfig? lora = null, LoraPairRandomSeedAuthority? adapterSeeds = null,
        CancellationToken cancellationToken = default)
        => readback is null || !IsIssuedReadback(readback)
            ? Task.FromResult(Fail(InvalidErrorCode))
            : ProduceOrOpenAsync(catalog, selector, repositoryRoot, domain, pattern, modelId, version,
                config, compatibility, context, lora, adapterSeeds, cancellationToken, readback);

    private OperationResult<CodeDocumentationPreparedBaselineReadback> Reopen(
        RepositoryOwnedPreparedBaselineAddress address, string requestHash, LlmModel model,
        CodeDocumentationSavedModelAssessmentModelPassport passport,
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context,
        CodeDocumentationSavedModelAssessmentCalibrationPlan plan, string runId)
    {
        var claimBytes = Read(address.ClaimPath, MaximumClaimBytes);
        var lines = new UTF8Encoding(false, true).GetString(claimBytes).Split('\n');
        if (lines.Length != 3 || lines[2].Length != 0) return Fail(RecoveryRequiredErrorCode);
        var startedBytes = Encoding.UTF8.GetBytes(lines[0]);
        var started = Deserialize<StartedClaim>(startedBytes);
        var committed = Deserialize<CommittedClaim>(Encoding.UTF8.GetBytes(lines[1]));
        if (started.FormatVersion != FormatVersion || started.Kind != "Started" ||
            started.RequestSha256 != requestHash || started.ReservedEvaluationCalls != checked(2 * plan.ExpectedEvaluationCallCount) ||
            committed.FormatVersion != FormatVersion || committed.Kind != "Committed" ||
            committed.RequestSha256 != requestHash || committed.StartedSha256 != Hash(startedBytes))
            return Fail(RecoveryRequiredErrorCode);
        var bytes = Read(address.BaselinePath, MaximumBaselineBytes);
        if (bytes.LongLength != committed.BaselineByteLength || Hash(bytes) != committed.BaselineSha256)
            return Fail(RecoveryRequiredErrorCode);
        var record = Deserialize<BaselineRecord>(bytes);
        if (record.FormatVersion != FormatVersion || record.RequestSha256 != requestHash ||
            !Equals(record.Parent, passport) || !ValidBaseline(record.Baseline, model, passport, context, plan, runId))
            return Fail(RecoveryRequiredErrorCode);
        var classification = CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService
            .BaselineRequiresNewTrainingPair(record.Baseline);
        if (!classification.IsSuccess && classification.ErrorCode !=
            CodeDocumentationRepositoryOwnedDpoTrainingQualityGateService.BaselineAlreadySufficientErrorCode)
            return Fail(RecoveryRequiredErrorCode);
        return OperationResult<CodeDocumentationPreparedBaselineReadback>.Ok(new(
            _issuer, address, requestHash, committed.BaselineSha256, bytes.LongLength,
            record.Baseline, classification.IsSuccess && classification.Value));
    }

    private sealed record PreparedSelectedStudentInputs(LlmModel Model, Tokenizer.BpeTokenizer Tokenizer, // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        CodeDocumentationSelectedStudentRequestV1 Request, RepositoryOwnedModelRuntimeCompatibility Compatibility, // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        LoraConfig? Lora, LoraPairRandomSeedAuthority? Seeds); // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
    internal Task<OperationResult<SyntheticSelectedTaskStudentOperation>> PrepareSyntheticSelectedTaskStudentOperationAsync( // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        CodeDocumentationPreparedBaselineExternalReference original, string repositoryRoot, string domain, string pattern, string modelId, string version, // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        ModelConfig config, RepositoryOwnedModelRuntimeCompatibility compatibility, CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context, // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        LoraConfig? lora, LoraPairRandomSeedAuthority? seeds, CodeDocumentationOneLessonSourceSelection.SelectedTeachingMaterialReadback material, // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        CodeDocumentationOneLessonBindingProjectionV2 binding, ReadOnlyMemory<byte> savedSelectionFrameLf, string syntheticLeaseSha256, CancellationToken cancellationToken) // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        => SyntheticSelectedTaskStudentOperation.PrepareOnOwnerAsync(this, catalog, selector, original, repositoryRoot, domain, pattern, modelId, version, config, compatibility, context, lora, seeds, material, binding, savedSelectionFrameLf, syntheticLeaseSha256, cancellationToken); // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
    internal async Task<OperationResult<bool>> ValidateSyntheticSelectedStudentRequestAsync( // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        CodeDocumentationPreparedBaselineExternalReference original, string repositoryRoot, string domain, string pattern, string modelId, string version, // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        ModelConfig config, RepositoryOwnedModelRuntimeCompatibility compatibility, CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context, // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        LoraConfig? lora, LoraPairRandomSeedAuthority? seeds, CodeDocumentationOneLessonSourceSelection.SelectedTeachingMaterialReadback material, // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        CodeDocumentationOneLessonBindingProjectionV2 binding, ReadOnlyMemory<byte> savedSelectionFrameLf, string syntheticLeaseSha256, CodeDocumentationSelectedStudentRequestV1 savedRequest, CancellationToken cancellationToken) // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
    { // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        var prepared = await PrepareSelectedStudentInputsAsync(catalog, selector, original, repositoryRoot, domain, pattern, modelId, version, config, compatibility, context, lora, seeds, material, binding, savedSelectionFrameLf, syntheticLeaseSha256, cancellationToken).ConfigureAwait(false); // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        if (!prepared.IsSuccess || prepared.Value is null) return prepared.MapError<bool>(); // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        var current = CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(prepared.Value.Request)); // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        var saved = CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(savedRequest)); // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        return current.AsSpan().SequenceEqual(saved) ? OperationResult<bool>.Ok(true) : SelectedStudentPreparationInvalid<bool>(); // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
    } // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
    private static OperationResult<T> SelectedStudentPreparationInvalid<T>() => OperationResult<T>.Fail( // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.
        "Selected Student inputs differ from the genuine existing owners.", "ONE_LESSON_SELECTED_STUDENT_INPUT_INVALID"); // Prepare or replay-validate only genuinely reopened inputs; the pure replay path never mints a Generate operation.

    private async Task<OperationResult<PreparedSelectedStudentInputs>> PrepareSelectedStudentInputsAsync( // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        CodeDocumentationPreparedBaselineExternalReference original, string repositoryRoot, string domain, string pattern, string modelId, string version, // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        ModelConfig config, RepositoryOwnedModelRuntimeCompatibility compatibility, CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context, // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        LoraConfig? lora, LoraPairRandomSeedAuthority? seeds, CodeDocumentationOneLessonSourceSelection.SelectedTeachingMaterialReadback material, // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        CodeDocumentationOneLessonBindingProjectionV2 binding, ReadOnlyMemory<byte> savedSelectionFrameLf, string syntheticLeaseSha256, CancellationToken cancellationToken) // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
    { // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        cancellationToken.ThrowIfCancellationRequested(); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        var opened = await OpenExistingByExternalReferenceAsync(original, catalog, selector, repositoryRoot, domain, pattern, // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            modelId, version, config, compatibility, context, lora, seeds, cancellationToken).ConfigureAwait(false); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        if (!opened.IsSuccess || opened.Value is null) return opened.MapError<PreparedSelectedStudentInputs>(); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        var resolved = catalog.Resolve(selector); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        if (!resolved.IsSuccess || resolved.Value is null) return resolved.MapError<PreparedSelectedStudentInputs>(); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        var grant = resolved.Value; // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        var choice = CodeDocumentationOneLessonV2Wire.DecodeTeachingSelectionFrame(savedSelectionFrameLf, binding); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        if (!choice.IsSuccess || choice.Value is null) return choice.MapError<PreparedSelectedStudentInputs>(); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        var p = choice.Value.Payload; var item = material.Item.DeclaredItem; // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        if (syntheticLeaseSha256 != CodeDocumentationOneLessonV2Wire.SelectedStudentLeaseSha256() || // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            material.Graph.Selection.SourceSelectionSha256 != grant.SourceSelection!.SourceSelectionSha256 || p.SourceSha256 != material.Graph.Selection.SourceSelectionSha256 || // Genuine SOURCE readback reconstructs a detached immutable owner value; compare its complete commitment rather than allocation identity.
            p.OriginalReceiptId != original.ReceiptId || p.OriginalReceiptSha256 != original.ReceiptSha256 || // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            p.ItemId != item.ItemId || p.SourceId != item.SourceId || p.ReferenceEntryId != item.ReferenceEntryId || // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            p.SourceEntityFingerprint != item.SourceEntityFingerprint || binding.ParentManifestSha256 != grant.ParentManifestSha256 || // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            binding.ParentWeightsSha256 != grant.ParentWeightsSha256 || binding.ConfigurationSha256 != grant.ConfigurationSha256 || // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            TrainingSourceProvenance.Identify(config).Sha256 != compatibility.ConfigSha256 || grant.CheckpointSelection is null || // Validate the actual model configuration through its runtime owner; the grant separately pins the protected evaluator configuration.
            FrozenReferenceEntry.ComputeNormalizedTextSha256(material.OwnerInput) != material.Item.Entry.OwnerInputSha256) // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            return SelectedStudentPreparationInvalid<PreparedSelectedStudentInputs>(); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        var loaded = new RepositoryOwnedModelCheckpointLoader().LoadLatestCompatible(repositoryRoot, domain, pattern, modelId, version, compatibility); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        if (!loaded.IsSuccess || loaded.Value is null) return loaded.MapError<PreparedSelectedStudentInputs>(); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        using var checkpoint = loaded.Value; // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        if (checkpoint.Manifest.ManifestSha256 != grant.ParentManifestSha256 || checkpoint.WeightsSha256 != grant.ParentWeightsSha256 || // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            checkpoint.WeightsByteLength != grant.CheckpointSelection.Parent.WeightsByteLength) // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            return SelectedStudentPreparationInvalid<PreparedSelectedStudentInputs>(); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        using var tokenizerStream = new MemoryStream(); context.Tokenizer.Save(tokenizerStream); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        var tokenizerBytes = tokenizerStream.ToArray(); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        using var detachedStream = new MemoryStream(tokenizerBytes, writable: false); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        var tokenizer = Tokenizer.BpeTokenizer.Load(detachedStream); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        using var checkTokenizer = new MemoryStream(); tokenizer.Save(checkTokenizer); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        if (!checkTokenizer.ToArray().AsSpan().SequenceEqual(tokenizerBytes)) return SelectedStudentPreparationInvalid<PreparedSelectedStudentInputs>(); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        var taskBytes = material.CopyOwnerInputUtf8(); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        if (taskBytes.Length is < 1 or > 65536 || !taskBytes.AsSpan().SequenceEqual(new UTF8Encoding(false, true).GetBytes(material.OwnerInput))) // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            return SelectedStudentPreparationInvalid<PreparedSelectedStudentInputs>(); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        var settings = CodeDocumentationOneLessonV2Wire.CaptureSelectedStudentSettings(context.GenerationSettings); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        var tokens = tokenizer.Encode(material.OwnerInput); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        if (tokens.Count == 0 || tokens.Count > config.ContextLen || // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            (long)tokens.Count + context.GenerationSettings.MaxNewTokens - 1 > config.ContextLen) // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            return SelectedStudentPreparationInvalid<PreparedSelectedStudentInputs>(); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        var model = LoadModel(checkpoint, config, lora, seeds, cancellationToken); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        var before = Hash(CaptureState(model, compatibility, lora, seeds)); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        var request = new CodeDocumentationSelectedStudentRequestV1("code-documentation-selected-student-input-v1", "SyntheticTest", // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            syntheticLeaseSha256, CodeDocumentationOneLessonV2Wire.SelectedStudentBindingSha256(binding), // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            Hash(savedSelectionFrameLf.Span[..^1].ToArray()), original.ReceiptId, original.ReceiptSha256, // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            p.SourceSha256, p.FrozenContextSha256, p.MaterialAdmissionEvidenceSha256, p.ItemId, p.SourceId, // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            p.ReferenceEntryId, p.SourceEntityFingerprint, material.OwnerInput, taskBytes.Length.ToString(CultureInfo.InvariantCulture), // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            Hash(taskBytes), checkpoint.Manifest.ManifestSha256, checkpoint.WeightsSha256, grant.ConfigurationSha256, // Preserve the evaluator pin in the saved binding/request; the full runtime digest below independently binds the verified model configuration.
            CodeDocumentationOneLessonV2Wire.SelectedStudentRuntimeSha256(compatibility), Hash(tokenizerBytes), before, // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
            settings, CodeDocumentationOneLessonV2Wire.SelectedStudentSettingsSha256(settings)); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        if (!CodeDocumentationOneLessonV2Wire.ValidSelectedStudentRequest(request)) return SelectedStudentPreparationInvalid<PreparedSelectedStudentInputs>(); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        cancellationToken.ThrowIfCancellationRequested(); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
        return OperationResult<PreparedSelectedStudentInputs>.Ok(new(model, tokenizer, request, compatibility, lora, seeds)); // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.
    } // Derive the full request from existing-only ORIGINAL, verified parent, actual frozen tokenizer and complete genuine task.

    internal sealed class SyntheticSelectedTaskStudentOperation : IDisposable // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
    { // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        private PreparedSelectedStudentInputs? inputs; // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        private int consumed; private string? completedResponse; private string? completedState; // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        internal CodeDocumentationSelectedStudentRequestV1 Request { get; } // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        private SyntheticSelectedTaskStudentOperation(PreparedSelectedStudentInputs prepared) // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        { inputs = prepared; Request = prepared.Request; } // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        internal static async Task<OperationResult<SyntheticSelectedTaskStudentOperation>> PrepareOnOwnerAsync( // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            CodeDocumentationRepositoryOwnedPreparedBaselineJournal owner, // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            CodeDocumentationOneLessonGrantCatalog catalog, CodeDocumentationOneLessonSelectorV1 selector, // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            CodeDocumentationPreparedBaselineExternalReference original, string repositoryRoot, string domain, string pattern, string modelId, string version, // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            ModelConfig config, RepositoryOwnedModelRuntimeCompatibility compatibility, CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context, // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            LoraConfig? lora, LoraPairRandomSeedAuthority? seeds, CodeDocumentationOneLessonSourceSelection.SelectedTeachingMaterialReadback material, // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            CodeDocumentationOneLessonBindingProjectionV2 binding, ReadOnlyMemory<byte> savedSelectionFrameLf, string syntheticLeaseSha256, CancellationToken cancellationToken) // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        { // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            var prepared = await owner.PrepareSelectedStudentInputsAsync(catalog, selector, original, repositoryRoot, domain, pattern, modelId, version, config, compatibility, context, lora, seeds, material, binding, savedSelectionFrameLf, syntheticLeaseSha256, cancellationToken).ConfigureAwait(false); // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            if (!prepared.IsSuccess || prepared.Value is null) return prepared.MapError<SyntheticSelectedTaskStudentOperation>(); // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            var operation = new SyntheticSelectedTaskStudentOperation(prepared.Value); // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            return OperationResult<SyntheticSelectedTaskStudentOperation>.Ok(operation); // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        } // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        internal bool HasUnchangedState => inputs is { } current && // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            Hash(CaptureState(current.Model, current.Compatibility, current.Lora, current.Seeds)) == Request.ModelStateBeforeSha256; // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        internal bool MatchesCompleted(string response, string state) => Volatile.Read(ref consumed) == 2 && // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            completedResponse == response && completedState == state && HasUnchangedState; // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        internal OperationResult<SyntheticSelectedTaskStudentResult> ExecuteOnce(CancellationToken token) // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        { // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            if (inputs is not { } current || Interlocked.CompareExchange(ref consumed, 1, 0) != 0) // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
                return SelectedStudentPreparationInvalid<SyntheticSelectedTaskStudentResult>(); // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            if (!HasUnchangedState) return SelectedStudentPreparationInvalid<SyntheticSelectedTaskStudentResult>(); // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            var settings = CodeDocumentationOneLessonV2Wire.ReadSelectedStudentSettings(Request.GenerationSettings); // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            string? response = null; string? after = null; Exception? primary = null; // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            try // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            { // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
                response = CodeDocumentationStudentGeneration.GeneratePrivate(current.Model, current.Tokenizer,
                    Request.Task, settings, token).Text;
            } // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            catch (Exception ex) when (SnapshotExceptionGraphClassifier.IsNonFatal(ex)) { primary = ex; } // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            finally // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            { // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
                try { after = Hash(CaptureState(current.Model, current.Compatibility, current.Lora, current.Seeds)); } // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
                catch (Exception ex) when (SnapshotExceptionGraphClassifier.IsNonFatal(ex)) // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
                { if (primary is null) primary = ex; else primary.Data["SelectedStudentFinalStateFailure"] = ex; } // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            } // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            if (primary is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(primary).Throw(); // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            if (!CodeDocumentationOneLessonV2Wire.ValidSelectedStudentResponse(Request, response, after, out _)) // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
                return SelectedStudentPreparationInvalid<SyntheticSelectedTaskStudentResult>(); // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            completedResponse = response; completedState = after; Volatile.Write(ref consumed, 2); // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            return OperationResult<SyntheticSelectedTaskStudentResult>.Ok(SyntheticSelectedTaskStudentResult.Mint(this, response!, after!)); // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        } // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        public void Dispose() { inputs = null; } // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
    } // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
    internal sealed class SyntheticSelectedTaskStudentResult // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
    { // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        internal CodeDocumentationSelectedStudentRequestV1 Request { get; } // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        internal string Response { get; } // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        internal string ModelStateAfterSha256 { get; } // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        private readonly byte[] responseBytes; // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        private SyntheticSelectedTaskStudentResult(SyntheticSelectedTaskStudentOperation owner, string response, string state) // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        { Request = owner.Request; Response = response; ModelStateAfterSha256 = state; responseBytes = new UTF8Encoding(false, true).GetBytes(response); } // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        internal byte[] CopyResponseUtf8() => responseBytes.ToArray(); // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        internal static SyntheticSelectedTaskStudentResult Mint(SyntheticSelectedTaskStudentOperation owner, string response, string state) // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        { // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            if (!owner.MatchesCompleted(response, state)) throw new InvalidOperationException("Unissued selected Student result."); // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
            return new(owner, response, state); // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
        } // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.
    } // Keep actual neural Generate one-shot, preserve primary cancellation/state failures, and mint full output only from completed operation.

    private static LlmModel LoadModel(RepositoryOwnedModelCheckpointLoadResult checkpoint,
        ModelConfig config, LoraConfig? lora, LoraPairRandomSeedAuthority? seeds,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        checkpoint.Weights.Position = 0;
        if (lora is null)
            LlmModel.ValidateFullCheckpointArtifact(checkpoint.Weights, config,
                checkpoint.WeightsByteLength, token);
        var model = new LlmModel(config);
        if (lora is not null)
        {
            var canonical = LoraApplicator.ValidateAndCanonicalize(lora);
            if (seeds is null) throw new InvalidDataException("Seeded adapter authority is required.");
            LoraApplicator.ApplySeeded(model, canonical, seeds.RootInitializationSeed,
                seeds.PolicyDropoutStreamDomain);
        }
        checkpoint.Weights.Position = 0;
        model.LoadCheckpoint(checkpoint.Weights);
        return model;
    }

    private static bool ValidBaseline(CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality baseline,
        LlmModel model, CodeDocumentationSavedModelAssessmentModelPassport passport,
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context,
        CodeDocumentationSavedModelAssessmentCalibrationPlan plan, string runId)
    {
        var request = new CodeDocumentationSavedModelAssessmentRequest(model, context.Tokenizer,
            passport, context.Corpus, context.CorpusManifest, context.EvaluationClient,
            context.EvaluatorConfiguration, context.GenerationSettings, runId,
            context.ObservedAtUtc, context.EvidenceRefs);
        if (baseline is null || baseline.ExpectedEvaluationCallCount != plan.ExpectedEvaluationCallCount ||
            baseline.ObservedEvaluationCallCount != plan.ExpectedEvaluationCallCount ||
            !CodeDocumentationSavedModelAssessmentCalibrationService.BaselineMatchesCurrentExam(
                baseline.Calibration, request, context.NoisePolicyConfig, plan)) return false;
        var report = baseline.Calibration.RepresentativeAssessment;
        using var serialization = new MemoryStream();
        model.SaveCheckpointToStream(serialization);
        var modelHash = Hash(serialization.ToArray());
        if (!report.IsSuccess || report.AssessmentRunId != runId + "-repeat-001" ||
            report.ModelSourceKind != passport.ModelSourceKind ||
            report.ModelManifestSha256 != passport.ManifestSha256 ||
            report.SavedWeightsSha256 != passport.WeightsSha256 ||
            report.ModelIdentitySha256 != passport.ModelIdentitySha256 ||
            report.RepositoryRelativeModelIdentityPath != passport.RepositoryRelativeModelIdentityPath ||
            report.ModelSerializationBeforeSha256 != modelHash || report.ModelSerializationAfterSha256 != modelHash ||
            report.ModelSerializationBeforeByteLength != serialization.Length ||
            report.ModelSerializationAfterByteLength != serialization.Length) return false;
        var ids = new HashSet<string>(StringComparer.Ordinal);
        var observations = baseline.Calibration.Calibration.Observations;
        for (var i = 0; i < observations.Count; i++)
        {
            var observation = observations[i];
            if (observation.AssessmentRunId != runId + "-repeat-" + (i + 1).ToString("D3") ||
                observation.ModelSerializationBeforeSha256 != modelHash ||
                observation.ModelSerializationAfterSha256 != modelHash ||
                observation.StudentAnswerSha256.Count != plan.HeldOutCaseCount ||
                observation.ObservedEvaluationRequestIds.Count != plan.HeldOutCaseCount ||
                observation.StudentAnswerSha256.Any(hash => !IsHash(hash)) ||
                observation.ObservedEvaluationRequestIds.Any(id => string.IsNullOrWhiteSpace(id) ||
                    id.Length > 256 || !ids.Add(id))) return false;
        }
        return ids.Count == plan.ExpectedEvaluationCallCount &&
            baseline.Calibration.Calibration.IsSuccess &&
            MeasurementNoisePolicy.TryValidatePassport(baseline.Calibration.Calibration.MeasurementNoisePassport, out _);
    }

    private static string RequestHash(CodeDocumentationOneLessonGrantV1 grant, int allowance,
        RepositoryOwnedModelRuntimeCompatibility compatibility,
        CodeDocumentationRepositoryOwnedDpoTrainingQualityGateContext context,
        CodeDocumentationSavedModelAssessmentModelPassport passport, LoraConfig? lora,
        LoraPairRandomSeedAuthority? seeds)
    {
        using var tokenizerBytes = new MemoryStream();
        context.Tokenizer.Save(tokenizerBytes);
        // This is this producer's request binding, not the future complete S04.02A commitment.
        return Hash(JsonSerializer.SerializeToUtf8Bytes(new
        {
            Schema = "prepared-baseline-request-v1", Grant = grant,
            grant.FormatVersion, grant.DescriptorsV2, Allowance = allowance,
            Parent = passport, Runtime = compatibility, Corpus = context.CorpusManifest,
            CaseIds = context.Corpus.Entries.Where(e => e.Partition == FrozenReferencePartition.HeldOut)
                .OrderBy(e => e.EntryId, StringComparer.Ordinal).Select(e => e.EntryId).ToArray(),
            TokenizerSha256 = Hash(tokenizerBytes.ToArray()), Generation = context.GenerationSettings,
            Evaluator = context.EvaluatorConfiguration, Noise = context.NoisePolicyConfig,
            context.MaximumDuration, context.ObservedAtUtc, Evidence = context.EvidenceRefs,
            Adapter = lora, Seeds = seeds,
        }, Json));
    }

    internal static bool FitsCompleteRecord(IReadOnlyList<string> caseIds, int repeats)
        => CompleteRecordUpperBoundBytes(caseIds, repeats) is { } bound && bound <= MaximumBaselineBytes;

    internal static long? CompleteRecordUpperBoundBytes(IReadOnlyList<string> caseIds, int repeats)
    {
        // Конечная верхняя граница исходного полного формата, до любых эффектов.
        // Один элемент каждой из трёх массивных колонок: case id, ASCII SHA256,
        // request id (не более 256 UTF-16 символов). JSON даёт максимум 6 байт
        // на символ; +3 на элемент покрывает кавычки и запятую, без усечения.
        try
        {
            if (caseIds is null || caseIds.Count is < 1 or > 200 || repeats < 2 ||
                caseIds.Any(id => string.IsNullOrWhiteSpace(id) || id.Length > 256)) return null;
            var n = checked((long)caseIds.Count * repeats);
            if (n > 5000) return null;
            var maximumCaseLength = caseIds.Max(id => id.Length);
            var cellBytes = 6L * (256 + maximumCaseLength) + 64 + 3 * 3;

            // Run id строит этот owner: ASCII prefix + 64 hex + "-repeat-" +
            // D3 (четыре цифры для 1000..5000). Это не произвольная строка 256.
            // Сериализуем только пустой каркас типизированного observation:
            // реальные имена полей, три ASCII-хэша, максимальные счётчики и
            // длиннейшее конечное double. Каркас не является результатом замера.
            var runIdLength = "prepared-baseline:".Length + 64 + "-repeat-".Length +
                Math.Max(3, repeats.ToString(System.Globalization.CultureInfo.InvariantCulture).Length);
            var hash = new string('a', 64);
            var observationEnvelopeBytes = Serialize(new CodeDocumentationSavedModelAssessmentCalibrationObservation(
                new string('a', runIdLength), caseIds.Count, caseIds.Count, double.MinValue,
                hash, hash, hash, [], [], [])).LongLength;
            // Ещё один double на повтор лежит в NoisePassport.RepeatedScores.
            // По одной запятой для observations/scores, включая последний элемент
            // как консервативный запас; массивные скобки уже есть в каркасе.
            var repeatBytes = observationEnvelopeBytes + 1 + Serialize(double.MinValue).LongLength + 1;

            // Representative хранит preview (cap1024), case id, публичную причину
            // (пять переводов AssessAsync, каждый <=64 символов), SHA и счётчики;
            // его evaluator дополнительно хранит N request ids. 1024 покрывает
            // имена/числа/кавычки всех шести полей case result, не ответы/массивы.
            var representativeCaseBytes = 6L * (1024 + maximumCaseLength + 64) + 1024 + 6L * 256 + 3;
            // Не зависящие от N/R паспорт, scorer (дважды), пути, настройки и
            // сообщения остаются под прежним запасом256KiB: фиксированный граф,
            // identity cap256/path cap1024. Два evidence-массива <=128 и <=133
            // элементов (128 исходных +5 calibration), каждый <=512 символов,
            // покрыты прежними 4*128 слотами. Эти запасы не масштабируются с R.
            return checked(256L * 1024 + n * cellBytes + repeats * repeatBytes +
                caseIds.Count * representativeCaseBytes +
                4L * 128 * (6 * 512 + 32));
        }
        catch (OverflowException) { return null; }
    }

    internal static byte[] CaptureState(LlmModel model,
        RepositoryOwnedModelRuntimeCompatibility compatibility, LoraConfig? lora,
        LoraPairRandomSeedAuthority? seeds)
    {
        var loraHash = lora is null ? Hash("adapter-free"u8.ToArray()) :
            seeds is null ? throw new InvalidDataException("Missing deterministic adapter authority.") :
                LoraRandomStateIdentity.ComputeLoraConfigurationSha256(lora, seeds);
        var random = model.CaptureRandomState(compatibility.TopologySha256, loraHash, 0);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        foreach (var parameter in model.Parameters())
        {
            var value = parameter.Value;
            writer.Write(value.RequiresGrad);
            WriteTensor(writer, value);
            writer.Write(value.Grad is not null);
            if (value.Grad is { } grad) { writer.Write(grad.RequiresGrad); WriteTensor(writer, grad); }
        }
        writer.Write(JsonSerializer.SerializeToUtf8Bytes(random, Json));
        writer.Flush();
        return stream.ToArray();
    }

    private static void WriteTensor(BinaryWriter writer, Tensor tensor)
    {
        writer.Write(tensor.Shape.Rank);
        foreach (var dimension in tensor.Shape.Dimensions) writer.Write(dimension);
        writer.Write(tensor.Data.Length);
        foreach (var value in tensor.Data) writer.Write(BitConverter.SingleToInt32Bits(value));
    }

    private static void EnsureJournalDirectories(RepositoryOwnedPreparedBaselineAddress address, bool allowCreate) // Keep directory custody inside the original baseline owner.
    { // Validate and provision only the directory levels owned by this ORIGINAL journal.
        var journalParent = Path.GetDirectoryName(address.BaselineRoot)!; // The validated layout places the objective under prepared-baselines.
        var modelVersionRoot = Path.GetDirectoryName(journalParent)!; // This existing artifact ancestor belongs to the checkpoint owner.
        if (!Directory.Exists(modelVersionRoot) || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(address.BaselineRoot)) // Do not create legacy artifact ancestors or traverse a substituted path.
            throw new InvalidDataException("Prepared baseline artifact ancestor is missing or unsafe."); // Missing canonical prerequisites fail before any journal write.
        foreach (var path in new[] { journalParent, address.BaselineRoot }) // Native Windows creation is nonrecursive, so provision these two owned levels in order.
        { // Establish custody for the journal parent before admitting its objective directory.
            if (allowCreate && !Directory.Exists(path)) // Receiving owners never create or repair missing history directories.
                SecureLocalFileBoundary.CreateCustodyOwnerOnlyDirectory(path); // New directories receive the protected native custody boundary at creation.
            if (!Directory.Exists(path) || !SecureLocalFileBoundary.HasCustodyOwnerOnlyDirectoryMode(path) || SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path)) // Existing directories must already satisfy custody and path identity.
                throw new InvalidDataException("Prepared baseline directory custody rejected."); // Preserve unsafe existing permissions without altering them.
        } // Finish custody and reparse validation for the current owned directory.
    } // Leave checkpoint ancestors untouched after admitting both journal directory levels.

    private static FileStream OpenOwnerLock(string path, bool allowCreate) // Reuse the session owner's missing-only bootstrap and concurrent-creator pattern.
    { // Acquire this canonical journal lock through authenticated missing-only ownership.
        try // Prefer reopening existing custody without creating or changing lock history.
        { // Authenticate the existing lock with exclusive sharing before any creation path.
            return Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None); // Reopen and authenticate an existing canonical lock first.
        } // Preserve missing-file versus other refusals for the producer-only bootstrap filter.
        catch (IOException exception) when (allowCreate && (exception is FileNotFoundException || (exception.HResult & 0xffff) == 2)) // Only a producer's actual missing-file result permits creation.
        { // Allow only a producer with a confirmed missing lock to attempt exclusive creation.
            try // Create the absent canonical lock without replacing another owner file.
            { // Give the missing-lock producer one exclusive CreateNew attempt.
                return Open(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None); // Exclusive creation cannot overwrite a pre-existing lock.
            } // Return a successfully authenticated new handle or retain the creation refusal.
            catch (IOException) when (File.Exists(path)) // A concurrent creator may have won after the missing-file observation.
            { // Reopen only the canonical file observed after a competing creator won.
                return Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None); // Custody and exclusive sharing still reject foreign files or a live owner.
            } // Retain custody and sharing refusals when reopening the competing creator lock.
        } // Complete missing-only bootstrap without repairing any existing lock.
    } // Yield an authenticated owner handle or preserve the original refusal.

    private static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
    {
        if (SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path))
            throw new InvalidDataException("Reparse path rejected.");
        var stream = SecureLocalFileBoundary.OpenCustodyOwnerOnlyFile(path, mode, access, share, FileOptions.WriteThrough); // Create and reopen every journal file through the same custody boundary.
        try { CheckHandle(stream, path); return stream; }
        catch { stream.Dispose(); throw; }
    }

    private static void CheckHandle(FileStream stream, string path)
    {
        if (!SecureLocalFileBoundary.OpenedHandleMatchesRequestedPath(stream.SafeFileHandle, path) ||
            !SecureLocalFileBoundary.HasCustodyOwnerOnlyFileMode(path) || // Authenticate protected custody instead of the generic Windows permission shortcut.
            SecureLocalFileBoundary.AnyExistingPathSegmentIsReparsePoint(path))
            throw new InvalidDataException("Unstable baseline file identity.");
    }

    private static byte[] Read(string path, int maximum)
    {
        using var stream = Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var bytes = SecureLocalFileBoundary.ReadBoundedStableBytes(stream, maximum)
            ?? throw new InvalidDataException("Incomplete baseline bytes.");
        CheckHandle(stream, path);
        return bytes;
    }

    private static void CreateNew(string path, byte[] bytes)
    {
        using var stream = Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        stream.Write(bytes);
        stream.Flush(true);
        CheckHandle(stream, path);
    }

    private static byte[] Serialize<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value, Json);
    private static T Deserialize<T>(byte[] bytes)
    {
        using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 32 });
        ValidateJson(document.RootElement);
        var value = JsonSerializer.Deserialize<T>(bytes, Json) ?? throw new JsonException("Null baseline.");
        // Reject omitted fields, noncanonical spelling/order/encoding and trailing bytes.
        if (!bytes.AsSpan().SequenceEqual(Serialize(value))) throw new JsonException("Noncanonical baseline.");
        return value;
    }

    private static void ValidateJson(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Null: throw new JsonException("Null prepared field.");
            case JsonValueKind.Object:
                var names = new HashSet<string>(StringComparer.Ordinal);
                foreach (var property in element.EnumerateObject())
                { if (!names.Add(property.Name)) throw new JsonException("Duplicate prepared field."); ValidateJson(property.Value); }
                break;
            case JsonValueKind.Array:
                if (element.GetArrayLength() > 10000) throw new JsonException("Prepared collection exceeds bound.");
                foreach (var item in element.EnumerateArray()) ValidateJson(item);
                break;
            case JsonValueKind.String:
                if (element.GetString()!.Length > 16384) throw new JsonException("Prepared string exceeds bound.");
                break;
        }
    }

    private static bool IsHash(string value) => value.Length == 64 &&
        value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    private static byte[] AppendNewline(byte[] bytes) => [.. bytes, (byte)'\n'];
    private static OperationResult<CodeDocumentationPreparedBaselineReadback> Fail(string code)
        => OperationResult<CodeDocumentationPreparedBaselineReadback>.Fail(
            "Prepared baseline requires exact owner evidence; uncertain history is preserved without remeasurement.", code);
    private sealed record StartedClaim(int FormatVersion, string Kind, string RequestSha256, int ReservedEvaluationCalls);
    private sealed record CommittedClaim(int FormatVersion, string Kind, string RequestSha256,
        string StartedSha256, string BaselineSha256, long BaselineByteLength);
    private sealed record BaselineRecord(int FormatVersion, string RequestSha256,
        CodeDocumentationSavedModelAssessmentModelPassport Parent,
        CodeDocumentationRepositoryOwnedDpoTrainingBaselineQuality Baseline);
}
