// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;

public sealed record DesignConversionHumanApprovalActor(
    string TenantId,
    string ActorId,
    IReadOnlyList<string> Roles,
    bool Authenticated);

public sealed record DesignConversionHumanApprovalChallengeIssueRequest(
    string JobId,
    DesignConversionHumanApprovalActor Actor,
    TimeSpan? TimeToLive = null);

public sealed record DesignConversionHumanApprovalDecisionRequest(
    string ChallengeId,
    string JobId,
    DesignConversionHumanApprovalActor Actor,
    IReadOnlyList<string> AcknowledgedViewportIds,
    string OperationKey,
    string Reason);

public sealed record DesignConversionHumanApprovalSnapshotRef(
    string RefKind,
    string ViewportId,
    string RefId,
    string UriRef,
    string Sha256,
    int Width,
    int Height,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DesignConversionHumanApprovalSnapshot(
    string SnapshotId,
    string SnapshotDigest,
    string JobId,
    string RequestId,
    string TenantId,
    DateTimeOffset? JobUpdatedAtUtc,
    IReadOnlyList<string> RequiredViewportIds,
    IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> TargetScreenshotRefs,
    IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> RenderedScreenshotRefs,
    IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> ComparisonRefs,
    IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> VisualJudgeRefs,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionHumanApprovalChallenge(
    string ChallengeId,
    string JobId,
    string RequestId,
    string TenantId,
    string IssuedToActorId,
    string SnapshotDigest,
    DateTimeOffset IssuedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    IReadOnlyList<string> RequiredViewportIds,
    IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> TargetScreenshotRefs,
    IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> RenderedScreenshotRefs,
    IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> ComparisonRefs,
    IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> VisualJudgeRefs,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionHumanApprovalRecord(
    string ApprovalId,
    string ChallengeId,
    string JobId,
    string RequestId,
    string TenantId,
    string ActorId,
    DateTimeOffset ApprovedAtUtc,
    string Reason,
    string SnapshotDigest,
    string OperationKeyHash,
    bool AcceptedForUser,
    bool ProductReady,
    bool TrainedOrLearnedProvenance,
    bool DpoEligible,
    bool ModelWeightsPublished,
    IReadOnlyList<string> RequiredViewportIds,
    IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> TargetScreenshotRefs,
    IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> RenderedScreenshotRefs,
    IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> ComparisonRefs,
    IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> VisualJudgeRefs,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public sealed record DesignConversionHumanApprovalResult(
    DesignConversionHumanApprovalRecord Record,
    bool RuntimeStateChanged,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyDictionary<string, string> BoundaryMetadata);

public interface IDesignConversionHumanApprovalStore
{
    Task<OperationResult<DesignConversionHumanApprovalChallenge>> AppendChallengeAsync(
        DesignConversionHumanApprovalChallenge challenge,
        CancellationToken cancellationToken = default);

    Task<OperationResult<DesignConversionHumanApprovalChallenge>> GetChallengeAsync(
        string challengeId,
        CancellationToken cancellationToken = default);

    Task<OperationResult<DesignConversionHumanApprovalRecord>> AppendApprovalIfOperationKeyUnusedAsync(
        DesignConversionHumanApprovalRecord record,
        CancellationToken cancellationToken = default);

    Task<OperationResult<IReadOnlyList<DesignConversionHumanApprovalRecord>>> ListApprovalRecordsAsync(
        CancellationToken cancellationToken = default);
}

public sealed class InMemoryDesignConversionHumanApprovalStore : IDesignConversionHumanApprovalStore
{
    private readonly object _gate = new();
    private readonly Dictionary<string, DesignConversionHumanApprovalChallenge> _challenges = new(StringComparer.Ordinal);
    private readonly List<DesignConversionHumanApprovalRecord> _approvals = [];

    public Task<OperationResult<DesignConversionHumanApprovalChallenge>> AppendChallengeAsync(
        DesignConversionHumanApprovalChallenge challenge,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<DesignConversionHumanApprovalChallenge>.Fail("Approval challenge write cancelled.", "CANCELLED"));
        if (challenge is null)
            return Task.FromResult(OperationResult<DesignConversionHumanApprovalChallenge>.Fail("Approval challenge is required.", "DESIGN_CONVERSION_APPROVAL_CHALLENGE_REQUIRED"));

        lock (_gate)
        {
            if (_challenges.ContainsKey(challenge.ChallengeId))
            {
                return Task.FromResult(OperationResult<DesignConversionHumanApprovalChallenge>.Fail(
                    "DesignConversion human approval challenge has already been issued.",
                    "DESIGN_CONVERSION_APPROVAL_CHALLENGE_REPLAYED"));
            }

            _challenges[challenge.ChallengeId] = challenge;
        }

        return Task.FromResult(OperationResult<DesignConversionHumanApprovalChallenge>.Ok(challenge));
    }

    public Task<OperationResult<DesignConversionHumanApprovalChallenge>> GetChallengeAsync(
        string challengeId,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<DesignConversionHumanApprovalChallenge>.Fail("Approval challenge read cancelled.", "CANCELLED"));
        if (string.IsNullOrWhiteSpace(challengeId))
            return Task.FromResult(OperationResult<DesignConversionHumanApprovalChallenge>.Fail("Approval challenge id is required.", "DESIGN_CONVERSION_APPROVAL_CHALLENGE_REQUIRED"));

        lock (_gate)
        {
            return Task.FromResult(_challenges.TryGetValue(challengeId.Trim(), out var challenge)
                ? OperationResult<DesignConversionHumanApprovalChallenge>.Ok(challenge)
                : OperationResult<DesignConversionHumanApprovalChallenge>.Fail(
                    "DesignConversion human approval challenge was not found.",
                    "DESIGN_CONVERSION_APPROVAL_CHALLENGE_NOT_FOUND"));
        }
    }

    public Task<OperationResult<DesignConversionHumanApprovalRecord>> AppendApprovalIfOperationKeyUnusedAsync(
        DesignConversionHumanApprovalRecord record,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<DesignConversionHumanApprovalRecord>.Fail("Approval write cancelled.", "CANCELLED"));
        if (record is null)
            return Task.FromResult(OperationResult<DesignConversionHumanApprovalRecord>.Fail("Approval record is required.", "DESIGN_CONVERSION_APPROVAL_RECORD_REQUIRED"));

        lock (_gate)
        {
            if (_approvals.Any(approval =>
                    string.Equals(approval.TenantId, record.TenantId, StringComparison.Ordinal) &&
                    string.Equals(approval.ChallengeId, record.ChallengeId, StringComparison.Ordinal)))
            {
                return Task.FromResult(OperationResult<DesignConversionHumanApprovalRecord>.Fail(
                    "DesignConversion human approval challenge has already been used.",
                    "DESIGN_CONVERSION_APPROVAL_CHALLENGE_REPLAYED"));
            }

            if (_approvals.Any(approval =>
                    string.Equals(approval.TenantId, record.TenantId, StringComparison.Ordinal) &&
                    string.Equals(approval.OperationKeyHash, record.OperationKeyHash, StringComparison.Ordinal)))
            {
                return Task.FromResult(OperationResult<DesignConversionHumanApprovalRecord>.Fail(
                    "DesignConversion human approval operation key has already been used.",
                    "DESIGN_CONVERSION_APPROVAL_OPERATION_REPLAYED"));
            }

            _approvals.Add(record);
        }

        return Task.FromResult(OperationResult<DesignConversionHumanApprovalRecord>.Ok(record));
    }

    public Task<OperationResult<IReadOnlyList<DesignConversionHumanApprovalRecord>>> ListApprovalRecordsAsync(
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<IReadOnlyList<DesignConversionHumanApprovalRecord>>.Fail("Approval read cancelled.", "CANCELLED"));

        lock (_gate)
            return Task.FromResult(OperationResult<IReadOnlyList<DesignConversionHumanApprovalRecord>>.Ok(_approvals.ToArray()));
    }
}

public sealed class JsonlDesignConversionHumanApprovalStore : IDesignConversionHumanApprovalStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    private readonly object _gate = new();
    private readonly string _journalPath;
    private readonly string _lockPath;
    private readonly Dictionary<string, DesignConversionHumanApprovalChallenge> _challenges = new(StringComparer.Ordinal);
    private readonly List<DesignConversionHumanApprovalRecord> _approvals = [];

    public JsonlDesignConversionHumanApprovalStore(string journalPath)
    {
        if (string.IsNullOrWhiteSpace(journalPath))
            throw new ArgumentException("DesignConversion human approval journal path is required.", nameof(journalPath));

        _journalPath = Path.GetFullPath(journalPath);
        _lockPath = _journalPath + ".lock";
        var directory = Path.GetDirectoryName(_journalPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
        ReloadExistingEvents();
    }

    public Task<OperationResult<DesignConversionHumanApprovalChallenge>> AppendChallengeAsync(
        DesignConversionHumanApprovalChallenge challenge,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<DesignConversionHumanApprovalChallenge>.Fail("Approval challenge write cancelled.", "CANCELLED"));
        if (challenge is null)
            return Task.FromResult(OperationResult<DesignConversionHumanApprovalChallenge>.Fail("Approval challenge is required.", "DESIGN_CONVERSION_APPROVAL_CHALLENGE_REQUIRED"));

        lock (_gate)
        {
            try
            {
                using var fileLock = AcquireFileLock(cancellationToken);
                ReloadExistingEvents();
                if (_challenges.ContainsKey(challenge.ChallengeId))
                {
                    return Task.FromResult(OperationResult<DesignConversionHumanApprovalChallenge>.Fail(
                        "DesignConversion human approval challenge has already been issued.",
                        "DESIGN_CONVERSION_APPROVAL_CHALLENGE_REPLAYED"));
                }

                _challenges[challenge.ChallengeId] = challenge;
                AppendEvent(new StoredHumanApprovalEvent("challenge", challenge, null));
            }
            catch (OperationCanceledException)
            {
                return Task.FromResult(OperationResult<DesignConversionHumanApprovalChallenge>.Fail(
                    "Approval challenge write cancelled.",
                    "CANCELLED"));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                return Task.FromResult(OperationResult<DesignConversionHumanApprovalChallenge>.Fail(
                    "DesignConversion human approval store is unavailable.",
                    "DESIGN_CONVERSION_APPROVAL_STORE_UNAVAILABLE"));
            }
        }

        return Task.FromResult(OperationResult<DesignConversionHumanApprovalChallenge>.Ok(challenge));
    }

    public Task<OperationResult<DesignConversionHumanApprovalChallenge>> GetChallengeAsync(
        string challengeId,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<DesignConversionHumanApprovalChallenge>.Fail("Approval challenge read cancelled.", "CANCELLED"));
        if (string.IsNullOrWhiteSpace(challengeId))
            return Task.FromResult(OperationResult<DesignConversionHumanApprovalChallenge>.Fail("Approval challenge id is required.", "DESIGN_CONVERSION_APPROVAL_CHALLENGE_REQUIRED"));

        lock (_gate)
        {
            try
            {
                using var fileLock = AcquireFileLock(cancellationToken);
                ReloadExistingEvents();
                return Task.FromResult(_challenges.TryGetValue(challengeId.Trim(), out var challenge)
                    ? OperationResult<DesignConversionHumanApprovalChallenge>.Ok(challenge)
                    : OperationResult<DesignConversionHumanApprovalChallenge>.Fail(
                        "DesignConversion human approval challenge was not found.",
                        "DESIGN_CONVERSION_APPROVAL_CHALLENGE_NOT_FOUND"));
            }
            catch (OperationCanceledException)
            {
                return Task.FromResult(OperationResult<DesignConversionHumanApprovalChallenge>.Fail(
                    "Approval challenge read cancelled.",
                    "CANCELLED"));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                return Task.FromResult(OperationResult<DesignConversionHumanApprovalChallenge>.Fail(
                    "DesignConversion human approval store is unavailable.",
                    "DESIGN_CONVERSION_APPROVAL_STORE_UNAVAILABLE"));
            }
        }
    }

    public Task<OperationResult<DesignConversionHumanApprovalRecord>> AppendApprovalIfOperationKeyUnusedAsync(
        DesignConversionHumanApprovalRecord record,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<DesignConversionHumanApprovalRecord>.Fail("Approval write cancelled.", "CANCELLED"));
        if (record is null)
            return Task.FromResult(OperationResult<DesignConversionHumanApprovalRecord>.Fail("Approval record is required.", "DESIGN_CONVERSION_APPROVAL_RECORD_REQUIRED"));

        lock (_gate)
        {
            try
            {
                using var fileLock = AcquireFileLock(cancellationToken);
                ReloadExistingEvents();
                if (_approvals.Any(approval =>
                        string.Equals(approval.TenantId, record.TenantId, StringComparison.Ordinal) &&
                        string.Equals(approval.ChallengeId, record.ChallengeId, StringComparison.Ordinal)))
                {
                    return Task.FromResult(OperationResult<DesignConversionHumanApprovalRecord>.Fail(
                        "DesignConversion human approval challenge has already been used.",
                        "DESIGN_CONVERSION_APPROVAL_CHALLENGE_REPLAYED"));
                }

                if (_approvals.Any(approval =>
                    string.Equals(approval.TenantId, record.TenantId, StringComparison.Ordinal) &&
                    string.Equals(approval.OperationKeyHash, record.OperationKeyHash, StringComparison.Ordinal)))
                {
                    return Task.FromResult(OperationResult<DesignConversionHumanApprovalRecord>.Fail(
                        "DesignConversion human approval operation key has already been used.",
                        "DESIGN_CONVERSION_APPROVAL_OPERATION_REPLAYED"));
                }

                _approvals.Add(record);
                AppendEvent(new StoredHumanApprovalEvent("approval", null, record));
            }
            catch (OperationCanceledException)
            {
                return Task.FromResult(OperationResult<DesignConversionHumanApprovalRecord>.Fail(
                    "Approval write cancelled.",
                    "CANCELLED"));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                return Task.FromResult(OperationResult<DesignConversionHumanApprovalRecord>.Fail(
                    "DesignConversion human approval store is unavailable.",
                    "DESIGN_CONVERSION_APPROVAL_STORE_UNAVAILABLE"));
            }
        }

        return Task.FromResult(OperationResult<DesignConversionHumanApprovalRecord>.Ok(record));
    }

    public Task<OperationResult<IReadOnlyList<DesignConversionHumanApprovalRecord>>> ListApprovalRecordsAsync(
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<IReadOnlyList<DesignConversionHumanApprovalRecord>>.Fail("Approval read cancelled.", "CANCELLED"));

        lock (_gate)
        {
            try
            {
                using var fileLock = AcquireFileLock(cancellationToken);
                ReloadExistingEvents();
                return Task.FromResult(OperationResult<IReadOnlyList<DesignConversionHumanApprovalRecord>>.Ok(_approvals.ToArray()));
            }
            catch (OperationCanceledException)
            {
                return Task.FromResult(OperationResult<IReadOnlyList<DesignConversionHumanApprovalRecord>>.Fail(
                    "Approval read cancelled.",
                    "CANCELLED"));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                return Task.FromResult(OperationResult<IReadOnlyList<DesignConversionHumanApprovalRecord>>.Fail(
                    "DesignConversion human approval store is unavailable.",
                    "DESIGN_CONVERSION_APPROVAL_STORE_UNAVAILABLE"));
            }
        }
    }

    private void ReloadExistingEvents()
    {
        _challenges.Clear();
        _approvals.Clear();
        if (!File.Exists(_journalPath))
            return;

        foreach (var line in File.ReadLines(_journalPath))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var stored = JsonSerializer.Deserialize<StoredHumanApprovalEvent>(line, JsonOptions);
            if (stored?.Challenge is not null)
                _challenges[stored.Challenge.ChallengeId] = stored.Challenge;
            if (stored?.Approval is not null)
                _approvals.Add(stored.Approval);
        }
    }

    private void AppendEvent(StoredHumanApprovalEvent stored)
    {
        var line = JsonSerializer.Serialize(stored, JsonOptions);
        var bytes = Encoding.UTF8.GetBytes(line + Environment.NewLine);
        using var stream = new FileStream(_journalPath, FileMode.Append, FileAccess.Write, FileShare.Read);
        stream.Write(bytes);
        stream.Flush(flushToDisk: true);
    }

    private FileStream AcquireFileLock(CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var directory = Path.GetDirectoryName(_lockPath);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);
                return new FileStream(_lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException) when (DateTimeOffset.UtcNow - startedAt < TimeSpan.FromSeconds(5))
            {
                Thread.Sleep(20);
            }
        }
    }

    private sealed record StoredHumanApprovalEvent(
        string Kind,
        DesignConversionHumanApprovalChallenge? Challenge,
        DesignConversionHumanApprovalRecord? Approval);
}

public sealed class DesignConversionHumanApprovalService
{
    public const string RequiredApproverRole = "design-conversion-human-approver";
    public const string ApprovalBoundary = "design_conversion_non_dpo_human_approval_refs_only";
    public static readonly TimeSpan DefaultChallengeTimeToLive = TimeSpan.FromMinutes(10);

    private const string ExternalVisualAiComparisonReviewRefKind = "external_visual_ai_comparison_receipt_refs_only";
    private const string ExternalVisualAiComparisonReviewUriPrefix =
        "artifact://design-conversion/human-approval/external-visual-ai-comparison/";

    private readonly IDesignConversionHumanApprovalStore _store;
    private readonly Func<DateTimeOffset> _utcNow;

    public DesignConversionHumanApprovalService(
        IDesignConversionHumanApprovalStore? store = null,
        Func<DateTimeOffset>? utcNow = null)
    {
        _store = store ?? new InMemoryDesignConversionHumanApprovalStore();
        _utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
    }

    public async Task<OperationResult<DesignConversionHumanApprovalChallenge>> IssueChallengeAsync(
        DesignConversionHumanApprovalChallengeIssueRequest request,
        DesignConversionThinClientStatusResult status,
        DesignConversionInternalEvaluationResult? evaluation,
        CancellationToken cancellationToken = default)
    {
        var actor = ValidateActor(request?.Actor);
        if (!actor.IsSuccess)
            return actor.MapError<DesignConversionHumanApprovalChallenge>();

        var snapshot = BuildSnapshot(request!.Actor.TenantId, request.JobId, status, evaluation);
        if (!snapshot.IsSuccess)
            return snapshot.MapError<DesignConversionHumanApprovalChallenge>();

        var now = _utcNow();
        var ttl = request.TimeToLive is { } requestedTtl && requestedTtl > TimeSpan.Zero
            ? requestedTtl
            : DefaultChallengeTimeToLive;
        var value = snapshot.Value!;
        var challenge = new DesignConversionHumanApprovalChallenge(
            ChallengeId: $"human-approval-challenge:{ShortDigest(value.SnapshotDigest)}:{now.UtcTicks.ToString(CultureInfo.InvariantCulture)}:{Guid.NewGuid():N}",
            JobId: value.JobId,
            RequestId: value.RequestId,
            TenantId: value.TenantId,
            IssuedToActorId: SanitizeScalar(request.Actor.ActorId),
            SnapshotDigest: value.SnapshotDigest,
            IssuedAtUtc: now,
            ExpiresAtUtc: now.Add(ttl),
            RequiredViewportIds: value.RequiredViewportIds,
            TargetScreenshotRefs: value.TargetScreenshotRefs,
            RenderedScreenshotRefs: value.RenderedScreenshotRefs,
            ComparisonRefs: value.ComparisonRefs,
            VisualJudgeRefs: value.VisualJudgeRefs,
            BoundaryMetadata: MergeMetadata(value.BoundaryMetadata, new Dictionary<string, string>
            {
                ["human_approval_challenge_issued"] = bool.TrueString,
                ["challenge_expires_at_utc"] = now.Add(ttl).ToString("O", CultureInfo.InvariantCulture),
                ["product_ready"] = bool.FalseString
            }));

        return await _store.AppendChallengeAsync(challenge, cancellationToken);
    }

    public async Task<OperationResult<DesignConversionHumanApprovalResult>> ApproveAsync(
        DesignConversionHumanApprovalDecisionRequest request,
        DesignConversionThinClientStatusResult status,
        DesignConversionInternalEvaluationResult? evaluation,
        CancellationToken cancellationToken = default)
    {
        var actor = ValidateActor(request?.Actor);
        if (!actor.IsSuccess)
            return actor.MapError<DesignConversionHumanApprovalResult>();

        if (string.IsNullOrWhiteSpace(request!.OperationKey))
        {
            return OperationResult<DesignConversionHumanApprovalResult>.Fail(
                "DesignConversion human approval requires a single-use operation key.",
                "DESIGN_CONVERSION_APPROVAL_OPERATION_KEY_REQUIRED");
        }

        var challengeResult = await _store.GetChallengeAsync(request.ChallengeId, cancellationToken);
        if (!challengeResult.IsSuccess)
            return challengeResult.MapError<DesignConversionHumanApprovalResult>();

        var challenge = challengeResult.Value!;
        if (!string.Equals(challenge.TenantId, request.Actor.TenantId, StringComparison.Ordinal) ||
            !string.Equals(challenge.IssuedToActorId, SanitizeScalar(request.Actor.ActorId), StringComparison.Ordinal))
        {
            return OperationResult<DesignConversionHumanApprovalResult>.Fail(
                "DesignConversion human approval actor is not authorized for this tenant or challenge.",
                "DESIGN_CONVERSION_APPROVAL_AUTHORIZATION_DENIED");
        }

        var now = _utcNow();
        if (now > challenge.ExpiresAtUtc)
        {
            return OperationResult<DesignConversionHumanApprovalResult>.Fail(
                "DesignConversion human approval challenge has expired.",
                "DESIGN_CONVERSION_APPROVAL_CHALLENGE_EXPIRED");
        }

        if (!string.Equals(challenge.JobId, request.JobId, StringComparison.Ordinal))
        {
            return OperationResult<DesignConversionHumanApprovalResult>.Fail(
                "DesignConversion human approval challenge does not match the current job snapshot.",
                "DESIGN_CONVERSION_APPROVAL_SNAPSHOT_STALE");
        }

        var snapshot = BuildSnapshot(request.Actor.TenantId, request.JobId, status, evaluation);
        if (!snapshot.IsSuccess)
            return snapshot.MapError<DesignConversionHumanApprovalResult>();

        var value = snapshot.Value!;
        if (!string.Equals(challenge.SnapshotDigest, value.SnapshotDigest, StringComparison.Ordinal))
        {
            return OperationResult<DesignConversionHumanApprovalResult>.Fail(
                "DesignConversion human approval snapshot is stale.",
                "DESIGN_CONVERSION_APPROVAL_SNAPSHOT_STALE");
        }

        if (!SameSet(request.AcknowledgedViewportIds, value.RequiredViewportIds))
        {
            return OperationResult<DesignConversionHumanApprovalResult>.Fail(
                "DesignConversion human approval must acknowledge exactly the required viewport set.",
                "DESIGN_CONVERSION_APPROVAL_VIEWPORT_ACK_MISMATCH");
        }

        var operationKeyHash = HashRef($"human-approval-operation-key:{request.Actor.TenantId}", request.OperationKey);
        var record = new DesignConversionHumanApprovalRecord(
            ApprovalId: $"human-approval:{ShortDigest(value.SnapshotDigest)}:{ShortDigest(operationKeyHash)}",
            ChallengeId: challenge.ChallengeId,
            JobId: value.JobId,
            RequestId: value.RequestId,
            TenantId: value.TenantId,
            ActorId: SanitizeScalar(request.Actor.ActorId),
            ApprovedAtUtc: now,
            Reason: SanitizeReason(request.Reason),
            SnapshotDigest: value.SnapshotDigest,
            OperationKeyHash: operationKeyHash,
            AcceptedForUser: true,
            ProductReady: false,
            TrainedOrLearnedProvenance: false,
            DpoEligible: false,
            ModelWeightsPublished: false,
            RequiredViewportIds: value.RequiredViewportIds,
            TargetScreenshotRefs: value.TargetScreenshotRefs,
            RenderedScreenshotRefs: value.RenderedScreenshotRefs,
            ComparisonRefs: value.ComparisonRefs,
            VisualJudgeRefs: value.VisualJudgeRefs,
            BoundaryMetadata: MergeMetadata(value.BoundaryMetadata, new Dictionary<string, string>
            {
                ["human_approval_status"] = "accepted-for-user",
                ["accepted_for_user"] = bool.TrueString,
                ["runtime_state_changed"] = bool.FalseString,
                ["product_ready"] = bool.FalseString,
                ["trained_or_learned_provenance"] = bool.FalseString,
                ["dpo_eligible"] = bool.FalseString,
                ["model_weights_published"] = bool.FalseString,
                ["raw_html_included"] = bool.FalseString,
                ["raw_css_included"] = bool.FalseString,
                ["screenshot_bytes_included"] = bool.FalseString,
                ["token_or_secret_included"] = bool.FalseString
            }));

        var append = await _store.AppendApprovalIfOperationKeyUnusedAsync(record, cancellationToken);
        if (!append.IsSuccess)
            return append.MapError<DesignConversionHumanApprovalResult>();

        var evidenceRefs = new[]
        {
            ApprovalBoundary,
            "DESIGN_CONVERSION_NON_DPO_HUMAN_APPROVAL_ACCEPTED_FOR_USER",
            "DESIGN_CONVERSION_HUMAN_APPROVAL_PRODUCT_READY_FALSE",
            "DESIGN_CONVERSION_HUMAN_APPROVAL_DPO_ELIGIBLE_FALSE",
            "DESIGN_CONVERSION_HUMAN_APPROVAL_MODEL_WEIGHTS_PUBLISHED_FALSE"
        };

        return OperationResult<DesignConversionHumanApprovalResult>.Ok(new DesignConversionHumanApprovalResult(
            append.Value!,
            RuntimeStateChanged: false,
            EvidenceRefs: evidenceRefs,
            BoundaryMetadata: record.BoundaryMetadata));
    }

    public async Task<OperationResult<IReadOnlyList<DesignConversionHumanApprovalRecord>>> ListApprovalRecordsAsync(
        CancellationToken cancellationToken = default)
        => await _store.ListApprovalRecordsAsync(cancellationToken);

    private static OperationResult<DesignConversionHumanApprovalSnapshot> BuildSnapshot(
        string tenantId,
        string jobId,
        DesignConversionThinClientStatusResult status,
        DesignConversionInternalEvaluationResult? evaluation)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            return OperationResult<DesignConversionHumanApprovalSnapshot>.Fail("DesignConversion job id is required.", "JOB_ID_REQUIRED");
        if (status is null)
            return OperationResult<DesignConversionHumanApprovalSnapshot>.Fail("DesignConversion runtime status is required.", "DESIGN_CONVERSION_APPROVAL_STATUS_REQUIRED");
        if (!HasCompletedRuntimeEvaluation(status.ContractStatus))
        {
            return OperationResult<DesignConversionHumanApprovalSnapshot>.Fail(
                "DesignConversion human approval requires a completed runtime evaluation snapshot.",
                "DESIGN_CONVERSION_APPROVAL_SNAPSHOT_NOT_COMPLETED");
        }

        if (evaluation is null)
        {
            return OperationResult<DesignConversionHumanApprovalSnapshot>.Fail(
                "DesignConversion human approval requires a server-owned captured evaluation snapshot.",
                "DESIGN_CONVERSION_APPROVAL_SNAPSHOT_EVIDENCE_INCOMPLETE");
        }

        if (!string.Equals(status.Job.JobId, jobId.Trim(), StringComparison.Ordinal) ||
            !string.Equals(evaluation.JobId, status.Job.JobId, StringComparison.Ordinal))
        {
            return OperationResult<DesignConversionHumanApprovalSnapshot>.Fail(
                "DesignConversion human approval status and evaluation job revision do not match.",
                "DESIGN_CONVERSION_APPROVAL_SNAPSHOT_STALE");
        }

        var requiredViewportIds = evaluation.VisualQaBoundaryTrace.MultiResolutionRender.RequiredViewportIds
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
        if (requiredViewportIds.Length == 0 || !evaluation.VisualQaBoundaryTrace.MultiResolutionRender.IsComplete)
        {
            return OperationResult<DesignConversionHumanApprovalSnapshot>.Fail(
                "DesignConversion human approval requires every required viewport to be evaluated.",
                "DESIGN_CONVERSION_APPROVAL_VIEWPORT_SET_INCOMPLETE");
        }

        var targetRefs = BuildTargetRefs(evaluation, requiredViewportIds);
        if (!targetRefs.IsSuccess)
            return targetRefs.MapError<DesignConversionHumanApprovalSnapshot>();
        var renderedRefs = BuildRenderedRefs(evaluation, requiredViewportIds);
        if (!renderedRefs.IsSuccess)
            return renderedRefs.MapError<DesignConversionHumanApprovalSnapshot>();
        var comparisonRefs = BuildComparisonRefs(evaluation, requiredViewportIds);
        if (!comparisonRefs.IsSuccess)
            return comparisonRefs.MapError<DesignConversionHumanApprovalSnapshot>();
        var visualJudgeRefs = BuildVisualJudgeRefs(evaluation, requiredViewportIds);
        if (!visualJudgeRefs.IsSuccess)
            return visualJudgeRefs.MapError<DesignConversionHumanApprovalSnapshot>();

        var snapshotTenant = SanitizeScalar(tenantId);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["approval_boundary"] = ApprovalBoundary,
            ["snapshot_source"] = "captured_server_owned_completed_evaluation",
            ["snapshot_digest_computed_server_side"] = bool.TrueString,
            ["client_snapshot_digest_accepted"] = bool.FalseString,
            ["required_viewport_count"] = requiredViewportIds.Length.ToString(CultureInfo.InvariantCulture),
            ["target_screenshot_ref_count"] = targetRefs.Value!.Count.ToString(CultureInfo.InvariantCulture),
            ["rendered_screenshot_ref_count"] = renderedRefs.Value!.Count.ToString(CultureInfo.InvariantCulture),
            ["comparison_ref_count"] = comparisonRefs.Value!.Count.ToString(CultureInfo.InvariantCulture),
            ["visual_judge_ref_count"] = visualJudgeRefs.Value!.Count.ToString(CultureInfo.InvariantCulture),
            ["product_ready"] = bool.FalseString,
            ["trained_or_learned_provenance"] = bool.FalseString,
            ["dpo_eligible"] = bool.FalseString,
            ["model_weights_published"] = bool.FalseString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["screenshot_bytes_included"] = bool.FalseString,
            ["token_or_secret_included"] = bool.FalseString
        };
        var digest = ComputeSnapshotDigest(
            status,
            evaluation,
            snapshotTenant,
            requiredViewportIds,
            targetRefs.Value!,
            renderedRefs.Value!,
            comparisonRefs.Value!,
            visualJudgeRefs.Value!);

        return OperationResult<DesignConversionHumanApprovalSnapshot>.Ok(new DesignConversionHumanApprovalSnapshot(
            SnapshotId: $"human-approval-snapshot:{ShortDigest(digest)}",
            SnapshotDigest: digest,
            JobId: status.Job.JobId,
            RequestId: status.Job.RequestId,
            TenantId: snapshotTenant,
            JobUpdatedAtUtc: status.Job.UpdatedAtUtc,
            RequiredViewportIds: requiredViewportIds,
            TargetScreenshotRefs: targetRefs.Value!,
            RenderedScreenshotRefs: renderedRefs.Value!,
            ComparisonRefs: comparisonRefs.Value!,
            VisualJudgeRefs: visualJudgeRefs.Value!,
            BoundaryMetadata: metadata));
    }

    private static bool HasCompletedRuntimeEvaluation(string? contractStatus)
        => string.Equals(contractStatus, DesignConversionThinClientStatus.Completed, StringComparison.Ordinal) ||
           string.Equals(contractStatus, DesignConversionThinClientStatus.Quarantined, StringComparison.Ordinal) ||
           string.Equals(contractStatus, DesignConversionThinClientStatus.NotReady, StringComparison.Ordinal);

    private static OperationResult<IReadOnlyList<DesignConversionHumanApprovalSnapshotRef>> BuildTargetRefs(
        DesignConversionInternalEvaluationResult evaluation,
        IReadOnlyList<string> requiredViewportIds)
    {
        var refs = evaluation.SubmittedRequest.Screenshots
            .Concat(evaluation.SubmittedRequest.DesignRenders)
            .Select(artifact => new
            {
                Artifact = artifact,
                ViewportId = ResolveTargetViewportId(
                    artifact.Width,
                    artifact.Height,
                    artifact.Metadata,
                    requiredViewportIds)
            })
            .Where(resolved => !string.IsNullOrWhiteSpace(resolved.ViewportId))
            .Select(resolved => ToSnapshotRef(
                "target-screenshot",
                resolved.ViewportId,
                resolved.Artifact.ArtifactId,
                resolved.Artifact.Uri,
                resolved.Artifact.Sha256,
                resolved.Artifact.Width,
                resolved.Artifact.Height,
                resolved.Artifact.Metadata))
            .Where(reference => requiredViewportIds.Contains(reference.ViewportId, StringComparer.Ordinal))
            .GroupBy(reference => reference.ViewportId, StringComparer.Ordinal)
            .Select(group => group.OrderBy(reference => reference.RefId, StringComparer.Ordinal).First())
            .OrderBy(reference => reference.ViewportId, StringComparer.Ordinal)
            .ToArray();

        return ValidateViewportRefs("target screenshot", refs, requiredViewportIds);
    }

    private static OperationResult<IReadOnlyList<DesignConversionHumanApprovalSnapshotRef>> BuildRenderedRefs(
        DesignConversionInternalEvaluationResult evaluation,
        IReadOnlyList<string> requiredViewportIds)
    {
        var refs = evaluation.RenderResults
            .Select(result => ToSnapshotRef(
                "rendered-screenshot",
                result.Image.Resolution.ResolutionId,
                result.Image.ImageId,
                result.Image.Uri,
                result.Image.Sha256,
                result.Image.Resolution.Width,
                result.Image.Resolution.Height,
                result.Image.Metadata))
            .Where(reference => requiredViewportIds.Contains(reference.ViewportId, StringComparer.Ordinal))
            .OrderBy(reference => reference.ViewportId, StringComparer.Ordinal)
            .ToArray();

        return ValidateViewportRefs(
            "rendered screenshot",
            refs,
            requiredViewportIds,
            rejectSyntheticOrScaffoldProof: true);
    }

    private static OperationResult<IReadOnlyList<DesignConversionHumanApprovalSnapshotRef>> BuildComparisonRefs(
        DesignConversionInternalEvaluationResult evaluation,
        IReadOnlyList<string> requiredViewportIds)
    {
        var refs = evaluation.ComparisonArtifacts
            .Select(ToComparisonSnapshotRef)
            .Where(reference => requiredViewportIds.Contains(reference.ViewportId, StringComparer.Ordinal))
            .OrderBy(reference => reference.ViewportId, StringComparer.Ordinal)
            .ToArray();

        return ValidateViewportRefs(
            "comparison",
            refs,
            requiredViewportIds,
            rejectSyntheticOrScaffoldProof: true,
            allowExternalVisualAiRefsOnlyProof: true);
    }

    private static DesignConversionHumanApprovalSnapshotRef ToComparisonSnapshotRef(
        DesignConversionQaComparisonArtifact artifact)
    {
        var externalVisualAiReviewMetadata = BuildExternalVisualAiComparisonReviewMetadata(artifact);
        if (IsFullyCanonicalTrustedExternalVisualAiComparisonArtifact(artifact, externalVisualAiReviewMetadata))
        {
            var reviewSha = BuildExternalVisualAiComparisonReviewSha(artifact, externalVisualAiReviewMetadata);
            return ToSnapshotRef(
                "comparison",
                artifact.Actual.Resolution.ResolutionId,
                artifact.ArtifactId,
                ExternalVisualAiComparisonReviewUriPrefix + ShortDigest(reviewSha),
                reviewSha,
                artifact.Actual.Resolution.Width,
                artifact.Actual.Resolution.Height,
                externalVisualAiReviewMetadata);
        }

        return ToSnapshotRef(
            "comparison",
            artifact.Actual.Resolution.ResolutionId,
            artifact.ArtifactId,
            artifact.DiffImageUri,
            artifact.DiffImageSha256,
            artifact.Actual.Resolution.Width,
            artifact.Actual.Resolution.Height,
            MergeMetadata(new Dictionary<string, string>
            {
                ["comparison_request_id"] = artifact.ComparisonRequestId,
                ["visual_score"] = artifact.Verdict.Score.ToString("R", CultureInfo.InvariantCulture),
                ["passed"] = artifact.Verdict.Passed.ToString(),
                ["media_type"] = "image/png",
                ["png_width"] = artifact.Actual.Resolution.Width.ToString(CultureInfo.InvariantCulture),
                ["png_height"] = artifact.Actual.Resolution.Height.ToString(CultureInfo.InvariantCulture)
            }, artifact.Metadata));
    }

    private static bool IsFullyCanonicalTrustedExternalVisualAiComparisonArtifact(
        DesignConversionQaComparisonArtifact artifact,
        IReadOnlyDictionary<string, string> reviewMetadata)
    {
        var artifactMetadata = artifact.Metadata;
        return MetadataEquals(artifactMetadata, "provider_kind", DesignConversionExternalVisualAiComparisonProvider.ProviderKind) &&
               MetadataEquals(artifactMetadata, "comparison_provider_kind", DesignConversionExternalVisualAiComparisonProvider.ProviderKind) &&
               MetadataTruthy(artifactMetadata, DesignConversionComparisonEvidenceTrustBoundary.ServerCanonicalizedKey) &&
               MetadataTruthy(artifactMetadata, DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey) &&
               MetadataTruthy(artifactMetadata, DesignConversionRenderEvidenceTrustBoundary.ApprovalEvidenceAllowedKey) &&
               MetadataEquals(
                   artifactMetadata,
                   DesignConversionComparisonEvidenceTrustBoundary.TrustClassificationKey,
                   DesignConversionComparisonEvidenceTrustBoundary.ServerOwnedExternalVisualAiReceiptValidatedClassification) &&
               MetadataTruthy(artifactMetadata, "server_owned_external_visual_ai_receipt_validated") &&
               MetadataEquals(artifactMetadata, "server_owned_external_visual_ai_receipt_validation_reason", "valid") &&
               MetadataTruthy(artifactMetadata, "visual_comparison_proven") &&
               MetadataTruthy(artifactMetadata, "network_call_executed") &&
               MetadataTruthy(artifactMetadata, "real_external_provider_executed") &&
               HasCanonicalExternalVisualAiReceiptBindings(reviewMetadata, artifact.ArtifactId) &&
               MetadataEquals(artifactMetadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey, artifact.ArtifactId) &&
               MetadataEquals(artifactMetadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey, artifact.ComparisonRequestId) &&
               MetadataEquals(artifactMetadata, "server_bound_comparison_request_id", artifact.ComparisonRequestId) &&
               MetadataEquals(artifactMetadata, "server_bound_expected_sha256", artifact.Expected.Sha256) &&
               MetadataEquals(artifactMetadata, "server_bound_rendered_sha256", artifact.Actual.Sha256);
    }

    private static bool HasCanonicalExternalVisualAiReceiptBindings(
        IReadOnlyDictionary<string, string> metadata,
        string comparisonArtifactId)
    {
        var serverBoundJobId = FirstMetadata(metadata, "server_bound_job_id");
        var serverBoundRequestId = FirstMetadata(metadata, "server_bound_request_id");
        var serverBoundComparisonRequestId = FirstMetadata(metadata, "server_bound_comparison_request_id");
        var providerName = FirstMetadata(metadata, "provider_name");
        var providerModel = FirstMetadata(metadata, "provider_model");
        var providerEndpointHost = FirstMetadata(metadata, "provider_endpoint_host");
        var receiptId = FirstMetadata(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey);
        var receiptVisualResultId = FirstMetadata(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey);
        var receiptComparisonArtifactId = FirstMetadata(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey);

        return !string.IsNullOrWhiteSpace(comparisonArtifactId) &&
               !string.IsNullOrWhiteSpace(serverBoundJobId) &&
               !string.IsNullOrWhiteSpace(serverBoundRequestId) &&
               !string.IsNullOrWhiteSpace(serverBoundComparisonRequestId) &&
               !string.IsNullOrWhiteSpace(providerName) &&
               !string.IsNullOrWhiteSpace(providerModel) &&
               !string.IsNullOrWhiteSpace(providerEndpointHost) &&
               !string.IsNullOrWhiteSpace(receiptId) &&
               !string.IsNullOrWhiteSpace(receiptVisualResultId) &&
               !string.IsNullOrWhiteSpace(receiptComparisonArtifactId) &&
               MetadataTruthy(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptKey) &&
               MetadataEquals(
                   metadata,
                   ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersionKey,
                   ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVersion) &&
               MetadataTruthy(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptExternalCallExecutedKey) &&
               MetadataTruthy(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseIdentityVerifiedKey) &&
               MetadataTruthy(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptResponseSchemaVerifiedKey) &&
               MetadataEquals(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJobIdKey, serverBoundJobId) &&
               MetadataEquals(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRequestIdKey, serverBoundRequestId) &&
               MetadataEquals(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptRootRequestIdKey, serverBoundRequestId) &&
               MetadataEquals(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptJudgeRequestIdKey, serverBoundComparisonRequestId) &&
               MetadataEquals(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey, serverBoundComparisonRequestId) &&
               MetadataEquals(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey, serverBoundComparisonRequestId) &&
               MetadataEquals(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderNameKey, providerName) &&
               MetadataEquals(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderModelKey, providerModel) &&
               MetadataEquals(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptEndpointHostKey, providerEndpointHost) &&
               MetadataEquals(metadata, "external_visual_ai_receipt_id", receiptId) &&
               MetadataEquals(metadata, "external_visual_ai_receipt_visual_result_id", receiptVisualResultId) &&
               MetadataEquals(metadata, "external_visual_ai_receipt_comparison_artifact_id", receiptComparisonArtifactId) &&
               MetadataEquals(metadata, "comparison_artifact_receipt_id", receiptComparisonArtifactId) &&
               string.Equals(receiptComparisonArtifactId, comparisonArtifactId, StringComparison.Ordinal);
    }

    private static IReadOnlyDictionary<string, string> BuildExternalVisualAiComparisonReviewMetadata(
        DesignConversionQaComparisonArtifact artifact)
    {
        var receiptId = FirstMetadata(artifact.Metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey);
        var receiptVisualResultId = FirstMetadata(artifact.Metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey);
        var receiptComparisonArtifactId = FirstMetadata(artifact.Metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey);
        var metadata = new Dictionary<string, string>(artifact.Metadata ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var pair in new Dictionary<string, string>
        {
            ["comparison_request_id"] = artifact.ComparisonRequestId,
            ["visual_score"] = artifact.Verdict.Score.ToString("R", CultureInfo.InvariantCulture),
            ["passed"] = artifact.Verdict.Passed.ToString(),
            ["media_type"] = "application/ref",
            ["proof_media_type"] = "application/ref",
            ["human_review_reference_kind"] = ExternalVisualAiComparisonReviewRefKind,
            ["server_owned_refs_only_human_review_reference"] = bool.TrueString,
            ["refs_only_comparison_artifact_reference"] = bool.TrueString,
            ["diff_png_fabricated"] = bool.FalseString,
            ["raw_html_included"] = bool.FalseString,
            ["raw_css_included"] = bool.FalseString,
            ["raw_design_tree_included"] = bool.FalseString,
            ["server_bound_comparison_request_id"] = artifact.ComparisonRequestId,
            ["server_bound_expected_sha256"] = artifact.Expected.Sha256,
            ["server_bound_rendered_sha256"] = artifact.Actual.Sha256,
            ["expected_image_id"] = artifact.Expected.ImageId,
            ["expected_image_sha256"] = artifact.Expected.Sha256,
            ["rendered_image_id"] = artifact.Actual.ImageId,
            ["rendered_image_sha256"] = artifact.Actual.Sha256,
            ["external_visual_ai_receipt_id"] = receiptId,
            ["external_visual_ai_receipt_visual_result_id"] = receiptVisualResultId,
            ["external_visual_ai_receipt_comparison_artifact_id"] = receiptComparisonArtifactId,
            ["comparison_artifact_receipt_id"] = receiptComparisonArtifactId
        })
        {
            metadata[pair.Key] = pair.Value;
        }

        return metadata;
    }

    private static string BuildExternalVisualAiComparisonReviewSha(
        DesignConversionQaComparisonArtifact artifact,
        IReadOnlyDictionary<string, string> metadata)
    {
        var stable = new
        {
            artifact.ArtifactId,
            artifact.ComparisonRequestId,
            expected = new
            {
                artifact.Expected.ImageId,
                artifact.Expected.Uri,
                artifact.Expected.Sha256,
                artifact.Expected.Resolution.ResolutionId,
                artifact.Expected.Resolution.Width,
                artifact.Expected.Resolution.Height,
                artifact.Expected.Resolution.DeviceScaleFactor
            },
            actual = new
            {
                artifact.Actual.ImageId,
                artifact.Actual.Uri,
                artifact.Actual.Sha256,
                artifact.Actual.Resolution.ResolutionId,
                artifact.Actual.Resolution.Width,
                artifact.Actual.Resolution.Height,
                artifact.Actual.Resolution.DeviceScaleFactor
            },
            verdict = new
            {
                artifact.Verdict.VerdictId,
                artifact.Verdict.Score,
                artifact.Verdict.Passed
            },
            receipt = new
            {
                id = FirstMetadata(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptIdKey),
                visual_result_id = FirstMetadata(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptVisualResultIdKey),
                comparison_artifact_id = FirstMetadata(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey),
                provider_request_id = FirstMetadata(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptProviderRequestIdKey),
                comparison_request_id = FirstMetadata(metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonRequestIdKey)
            }
        };
        var json = JsonSerializer.Serialize(stable, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return HashRef("design-conversion-human-approval-external-visual-ai-comparison-ref", json);
    }

    private static OperationResult<IReadOnlyList<DesignConversionHumanApprovalSnapshotRef>> BuildVisualJudgeRefs(
        DesignConversionInternalEvaluationResult evaluation,
        IReadOnlyList<string> requiredViewportIds)
    {
        var refs = evaluation.VisualJudgeResults
            .Select((result, index) => ToSnapshotRef(
                "visual-judge",
                index < requiredViewportIds.Count ? requiredViewportIds[index] : ViewportIdFromJudgeId(result.JudgeRequestId, requiredViewportIds),
                result.JudgeResultId,
                string.Join("|", result.EvidenceRefs.Where(static value => !string.IsNullOrWhiteSpace(value)).OrderBy(static value => value, StringComparer.Ordinal)),
                HashRef("visual-judge", string.Join("|", result.EvidenceRefs.Concat([result.Verdict.VerdictId, result.Status, result.Verdict.Score.ToString("R", CultureInfo.InvariantCulture)]))),
                width: 1,
                height: 1,
                new Dictionary<string, string>
                {
                    ["judge_request_id"] = result.JudgeRequestId,
                    ["verdict_id"] = result.Verdict.VerdictId,
                    ["visual_score"] = result.Verdict.Score.ToString("R", CultureInfo.InvariantCulture),
                    ["status"] = result.Status,
                    ["evidence_ref_count"] = result.EvidenceRefs.Count.ToString(CultureInfo.InvariantCulture),
                    ["media_type"] = "application/ref"
                }))
            .Where(reference => requiredViewportIds.Contains(reference.ViewportId, StringComparer.Ordinal))
            .OrderBy(reference => reference.ViewportId, StringComparer.Ordinal)
            .ToArray();

        return ValidateViewportRefs("visual judge", refs, requiredViewportIds, requirePngProof: false);
    }

    private static OperationResult<IReadOnlyList<DesignConversionHumanApprovalSnapshotRef>> ValidateViewportRefs(
        string subject,
        IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> refs,
        IReadOnlyList<string> requiredViewportIds,
        bool requirePngProof = true,
        bool rejectSyntheticOrScaffoldProof = false,
        bool allowExternalVisualAiRefsOnlyProof = false)
    {
        if (refs.Count == 0)
        {
            return OperationResult<IReadOnlyList<DesignConversionHumanApprovalSnapshotRef>>.Fail(
                $"DesignConversion human approval snapshot is missing {subject} refs.",
                "DESIGN_CONVERSION_APPROVAL_SNAPSHOT_EVIDENCE_INCOMPLETE");
        }

        foreach (var viewportId in requiredViewportIds)
        {
            if (!refs.Any(reference => string.Equals(reference.ViewportId, viewportId, StringComparison.Ordinal)))
            {
                return OperationResult<IReadOnlyList<DesignConversionHumanApprovalSnapshotRef>>.Fail(
                    $"DesignConversion human approval snapshot is missing {subject} ref for viewport '{viewportId}'.",
                    "DESIGN_CONVERSION_APPROVAL_VIEWPORT_SET_INCOMPLETE");
            }
        }

        foreach (var reference in refs)
        {
            var hasValidPngProof = HasValidPngProof(reference);
            var hasValidExternalVisualAiRef = allowExternalVisualAiRefsOnlyProof &&
                                              HasValidExternalVisualAiHumanReviewRef(reference);
            if (requirePngProof && !hasValidPngProof && !hasValidExternalVisualAiRef)
            {
                return OperationResult<IReadOnlyList<DesignConversionHumanApprovalSnapshotRef>>.Fail(
                    $"DesignConversion human approval snapshot has invalid PNG proof metadata for {subject} ref '{reference.RefId}'.",
                    "DESIGN_CONVERSION_APPROVAL_PNG_PROOF_INVALID");
            }

            if (rejectSyntheticOrScaffoldProof && IsSyntheticOrScaffoldApprovalProof(reference))
            {
                return OperationResult<IReadOnlyList<DesignConversionHumanApprovalSnapshotRef>>.Fail(
                    $"DesignConversion human approval snapshot cannot use scaffold, synthetic, or metadata-only proof for {subject} ref '{reference.RefId}'.",
                    "DESIGN_CONVERSION_APPROVAL_SYNTHETIC_RENDER_PROOF_REJECTED");
            }

            if (!IsValidSha256(reference.Sha256) || string.IsNullOrWhiteSpace(reference.RefId) || string.IsNullOrWhiteSpace(reference.UriRef))
            {
                return OperationResult<IReadOnlyList<DesignConversionHumanApprovalSnapshotRef>>.Fail(
                    $"DesignConversion human approval snapshot has invalid immutable ref metadata for {subject}.",
                    "DESIGN_CONVERSION_APPROVAL_SNAPSHOT_EVIDENCE_INCOMPLETE");
            }
        }

        return OperationResult<IReadOnlyList<DesignConversionHumanApprovalSnapshotRef>>.Ok(refs);
    }

    private static bool IsSyntheticOrScaffoldApprovalProof(DesignConversionHumanApprovalSnapshotRef reference)
        => MetadataTruthy(reference.Metadata, DesignConversionRenderEvidenceTrustBoundary.SyntheticOrScaffoldKey) ||
           MetadataTruthy(reference.Metadata, DesignConversionRenderEvidenceTrustBoundary.ScaffoldOrSyntheticRenderKey) ||
           (string.Equals(reference.RefKind, "comparison", StringComparison.Ordinal) &&
            !IsApprovedComparisonProvider(reference.Metadata)) ||
           !MetadataTruthy(reference.Metadata, DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey) ||
           !MetadataTruthy(reference.Metadata, DesignConversionRenderEvidenceTrustBoundary.ApprovalEvidenceAllowedKey) ||
           MetadataTruthy(reference.Metadata, DeterministicDesignConversionComparisonProvider.VisualComparisonNotProvenMarker) ||
           MetadataTruthy(reference.Metadata, "metadata_only_comparison_rejected") ||
           MetadataEquals(reference.Metadata, "status", DeterministicDesignConversionComparisonProvider.BoundedNonReadyStatus) ||
           MetadataEquals(reference.Metadata, "hidden_determinism_boundary", "metadata_only_render_evidence") ||
           MetadataEquals(reference.Metadata, "hidden_determinism_boundary", "metadata_only_comparison") ||
           MetadataEquals(reference.Metadata, "hidden_determinism_boundary", "deterministic_local_png_artifact_evidence") ||
           MetadataEquals(reference.Metadata, "hidden_determinism_boundary", "untrusted_injected_provider_render_evidence") ||
           MetadataEquals(reference.Metadata, "provider_kind", DeterministicDesignConversionRenderProvider.ProviderKind) ||
           MetadataEquals(reference.Metadata, "provider_kind", DeterministicLocalDesignConversionPngRenderProvider.ProviderKind) ||
           MetadataEquals(reference.Metadata, "provider_kind", DeterministicDesignConversionComparisonProvider.ProviderKind);

    private static bool IsApprovedComparisonProvider(IReadOnlyDictionary<string, string> metadata)
        => (MetadataEquals(metadata, "provider_kind", ServerOwnedLocalPngComparisonProvider.ProviderKind) &&
            MetadataEquals(metadata, "comparison_provider_kind", ServerOwnedLocalPngComparisonProvider.ProviderKind)) ||
           (MetadataEquals(metadata, "provider_kind", DesignConversionExternalVisualAiComparisonProvider.ProviderKind) &&
            MetadataEquals(metadata, "comparison_provider_kind", DesignConversionExternalVisualAiComparisonProvider.ProviderKind));

    private static bool HasValidExternalVisualAiHumanReviewRef(DesignConversionHumanApprovalSnapshotRef reference)
        => reference.Width > 0 &&
           reference.Height > 0 &&
           IsValidSha256(reference.Sha256) &&
           string.Equals(
               reference.UriRef,
               ExternalVisualAiComparisonReviewUriPrefix + ShortDigest(reference.Sha256),
               StringComparison.Ordinal) &&
           MetadataEquals(reference.Metadata, "media_type", "application/ref") &&
           MetadataEquals(reference.Metadata, "proof_media_type", "application/ref") &&
           MetadataEquals(reference.Metadata, "human_review_reference_kind", ExternalVisualAiComparisonReviewRefKind) &&
           MetadataTruthy(reference.Metadata, "server_owned_refs_only_human_review_reference") &&
           MetadataTruthy(reference.Metadata, "refs_only_comparison_artifact_reference") &&
           MetadataFalse(reference.Metadata, "diff_png_fabricated") &&
           MetadataFalse(reference.Metadata, "raw_html_included") &&
           MetadataFalse(reference.Metadata, "raw_css_included") &&
           MetadataFalse(reference.Metadata, "raw_design_tree_included") &&
           MetadataEquals(reference.Metadata, "provider_kind", DesignConversionExternalVisualAiComparisonProvider.ProviderKind) &&
           MetadataEquals(reference.Metadata, "comparison_provider_kind", DesignConversionExternalVisualAiComparisonProvider.ProviderKind) &&
           MetadataTruthy(reference.Metadata, DesignConversionComparisonEvidenceTrustBoundary.ServerCanonicalizedKey) &&
           MetadataTruthy(reference.Metadata, DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey) &&
           MetadataTruthy(reference.Metadata, DesignConversionRenderEvidenceTrustBoundary.ApprovalEvidenceAllowedKey) &&
           MetadataEquals(
               reference.Metadata,
               DesignConversionComparisonEvidenceTrustBoundary.TrustClassificationKey,
               DesignConversionComparisonEvidenceTrustBoundary.ServerOwnedExternalVisualAiReceiptValidatedClassification) &&
           MetadataTruthy(reference.Metadata, "server_owned_external_visual_ai_receipt_validated") &&
           MetadataEquals(reference.Metadata, "server_owned_external_visual_ai_receipt_validation_reason", "valid") &&
           MetadataTruthy(reference.Metadata, "visual_comparison_proven") &&
           MetadataTruthy(reference.Metadata, "network_call_executed") &&
           MetadataTruthy(reference.Metadata, "real_external_provider_executed") &&
           HasCanonicalExternalVisualAiReceiptBindings(reference.Metadata, reference.RefId) &&
           IsValidSha256(FirstMetadata(reference.Metadata, "expected_image_sha256")) &&
           IsValidSha256(FirstMetadata(reference.Metadata, "rendered_image_sha256")) &&
           !string.IsNullOrWhiteSpace(FirstMetadata(reference.Metadata, "expected_image_id")) &&
           !string.IsNullOrWhiteSpace(FirstMetadata(reference.Metadata, "rendered_image_id")) &&
           !string.IsNullOrWhiteSpace(FirstMetadata(reference.Metadata, "external_visual_ai_receipt_id")) &&
           !string.IsNullOrWhiteSpace(FirstMetadata(reference.Metadata, "external_visual_ai_receipt_visual_result_id")) &&
           MetadataEquals(reference.Metadata, ExternalHttpDesignConversionVisualJudge.ServerCallReceiptComparisonArtifactIdKey, reference.RefId) &&
           MetadataEquals(reference.Metadata, "external_visual_ai_receipt_comparison_artifact_id", reference.RefId) &&
           MetadataEquals(reference.Metadata, "comparison_artifact_receipt_id", reference.RefId) &&
           MetadataEquals(
               reference.Metadata,
               "server_bound_comparison_request_id",
               FirstMetadata(reference.Metadata, "comparison_request_id")) &&
           MetadataEquals(
               reference.Metadata,
               "server_bound_expected_sha256",
               FirstMetadata(reference.Metadata, "expected_image_sha256")) &&
           MetadataEquals(
               reference.Metadata,
               "server_bound_rendered_sha256",
               FirstMetadata(reference.Metadata, "rendered_image_sha256"));

    private static OperationResult<bool> ValidateActor(DesignConversionHumanApprovalActor? actor)
    {
        if (actor is null ||
            !actor.Authenticated ||
            string.IsNullOrWhiteSpace(actor.TenantId) ||
            string.IsNullOrWhiteSpace(actor.ActorId) ||
            actor.Roles is null ||
            !actor.Roles.Contains(RequiredApproverRole, StringComparer.Ordinal))
        {
            return OperationResult<bool>.Fail(
                "DesignConversion human approval requires an authenticated tenant-scoped human approver.",
                "DESIGN_CONVERSION_APPROVAL_AUTHORIZATION_DENIED");
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool HasValidPngProof(DesignConversionHumanApprovalSnapshotRef reference)
    {
        if (reference.Width <= 0 || reference.Height <= 0)
            return false;
        if (!IsValidSha256(reference.Sha256))
            return false;
        if (!reference.UriRef.StartsWith("artifact://", StringComparison.OrdinalIgnoreCase) &&
            !reference.UriRef.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) &&
            !reference.UriRef.StartsWith("local-png-ref:sha256:", StringComparison.OrdinalIgnoreCase) &&
            !reference.UriRef.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var mediaType = FirstMetadata(reference.Metadata, "media_type", "content_type", "proof_media_type");
        var deterministicPngBytesWritten = MetadataTruthy(reference.Metadata, "actual_png_bytes_written");
        if (!string.Equals(mediaType, "image/png", StringComparison.OrdinalIgnoreCase) &&
            !deterministicPngBytesWritten)
            return false;

        return MetadataDimensionsMatchPngProof(reference.Metadata, reference.Width, reference.Height);
    }

    private static string ComputeSnapshotDigest(
        DesignConversionThinClientStatusResult status,
        DesignConversionInternalEvaluationResult evaluation,
        string tenantId,
        IReadOnlyList<string> requiredViewportIds,
        IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> targetRefs,
        IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> renderedRefs,
        IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> comparisonRefs,
        IReadOnlyList<DesignConversionHumanApprovalSnapshotRef> visualJudgeRefs)
    {
        var stable = new
        {
            tenant_id = tenantId,
            job_id = status.Job.JobId,
            request_id = status.Job.RequestId,
            job_updated_at_utc = status.Job.UpdatedAtUtc?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty,
            contract_status = status.ContractStatus,
            verdict_score = evaluation.Verdict.Score.ToString("R", CultureInfo.InvariantCulture),
            verdict_passed = evaluation.Verdict.Passed,
            required_viewports = requiredViewportIds,
            target_refs = targetRefs.Select(ToDigestRef).ToArray(),
            rendered_refs = renderedRefs.Select(ToDigestRef).ToArray(),
            comparison_refs = comparisonRefs.Select(ToDigestRef).ToArray(),
            visual_judge_refs = visualJudgeRefs.Select(ToDigestRef).ToArray()
        };

        var json = JsonSerializer.Serialize(stable, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return HashRef("design-conversion-human-approval-snapshot", json);
    }

    private static object ToDigestRef(DesignConversionHumanApprovalSnapshotRef reference)
        => new
        {
            reference.RefKind,
            reference.ViewportId,
            reference.RefId,
            reference.UriRef,
            reference.Sha256,
            reference.Width,
            reference.Height,
            metadata = reference.Metadata
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)
        };

    private static DesignConversionHumanApprovalSnapshotRef ToSnapshotRef(
        string kind,
        string viewportId,
        string refId,
        string uriRef,
        string sha256,
        int width,
        int height,
        IReadOnlyDictionary<string, string>? metadata)
        => new(
            RefKind: kind,
            ViewportId: SanitizeScalar(viewportId),
            RefId: SanitizeScalar(refId),
            UriRef: SanitizeArtifactUriRef(uriRef),
            Sha256: SanitizeScalar(sha256),
            Width: width,
            Height: height,
            Metadata: SanitizeMetadata(metadata));

    internal static string ResolveTargetViewportId(
        int width,
        int height,
        IReadOnlyDictionary<string, string>? metadata,
        IReadOnlyList<string> requiredViewportIds)
    {
        var compatibleRequiredViewportIds = requiredViewportIds
            .Select(ParseRequiredViewportId)
            .Where(candidate => candidate.Width == width && candidate.Height == height)
            .GroupBy(candidate => candidate.ViewportId, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();
        if (compatibleRequiredViewportIds.Length == 0)
            return string.Empty;

        var explicitScale = ResolveTargetViewportDeviceScaleFactor(metadata);
        if (!explicitScale.IsSuccess)
            return string.Empty;

        var hasViewportIdentityMetadata =
            DesignConversionRenderResolutionFactory.HasViewportIdentityMetadata(metadata);
        IReadOnlyCollection<double> explicitDeviceScaleFactors = explicitScale.Value.HasValue
            ? [explicitScale.Value.Value]
            : [];
        var callerIdentity = DesignConversionRenderResolutionFactory.ResolveCallerViewportIdentity(
            width,
            height,
            metadata,
            explicitDeviceScaleFactors);
        if (!callerIdentity.IsSuccess)
            return string.Empty;

        if (hasViewportIdentityMetadata)
        {
            var requiredScale = callerIdentity.Value.EmbeddedDeviceScaleFactor
                                ?? explicitScale.Value
                                ?? 1d;
            return SingleMatchingRequiredViewportId(
                compatibleRequiredViewportIds,
                requiredScale,
                callerIdentity.Value.IdentityToken);
        }

        if (explicitScale.Value.HasValue)
        {
            return SingleMatchingRequiredViewportId(
                compatibleRequiredViewportIds,
                explicitScale.Value.Value,
                identityToken: null);
        }

        var legacyDimensionOnlyMatches = compatibleRequiredViewportIds
            .Where(candidate => string.IsNullOrWhiteSpace(candidate.IdentityToken))
            .Take(2)
            .ToArray();
        return legacyDimensionOnlyMatches.Length == 1
            ? legacyDimensionOnlyMatches[0].ViewportId
            : string.Empty;
    }

    private static RequiredViewportCandidate ParseRequiredViewportId(string viewportId)
    {
        if (!DesignConversionRenderResolutionFactory.TryParseCanonicalViewportId(viewportId, out var parsed))
            return new RequiredViewportCandidate(viewportId, -1, -1, 1d, null);

        return new RequiredViewportCandidate(
            viewportId,
            parsed.Width,
            parsed.Height,
            parsed.DeviceScaleFactor ?? 1d,
            parsed.IdentityToken);
    }

    private static OperationResult<double?> ResolveTargetViewportDeviceScaleFactor(
        IReadOnlyDictionary<string, string>? metadata)
    {
        var scaleMetadata = DesignConversionRenderResolutionFactory.FirstDeviceScaleFactorMetadata(metadata);
        if (string.IsNullOrWhiteSpace(scaleMetadata))
            return OperationResult<double?>.Ok(null);

        if (!double.TryParse(scaleMetadata, NumberStyles.Float, CultureInfo.InvariantCulture, out var deviceScaleFactor) ||
            !double.IsFinite(deviceScaleFactor) ||
            deviceScaleFactor <= 0)
        {
            return OperationResult<double?>.Fail(
                "DesignConversion human approval target viewport device scale factor is invalid.",
                "DESIGN_CONVERSION_APPROVAL_VIEWPORT_SCALE_INVALID");
        }

        return OperationResult<double?>.Ok(deviceScaleFactor);
    }

    private static string SingleMatchingRequiredViewportId(
        IReadOnlyList<RequiredViewportCandidate> candidates,
        double deviceScaleFactor,
        string? identityToken)
    {
        var matches = candidates
            .Where(candidate => candidate.DeviceScaleFactor.Equals(deviceScaleFactor) &&
                                IdentityTokenMatches(candidate.IdentityToken, identityToken))
            .Take(2)
            .ToArray();
        return matches.Length == 1 ? matches[0].ViewportId : string.Empty;
    }

    private static bool IdentityTokenMatches(string? requiredIdentityToken, string? metadataIdentityToken)
    {
        if (string.IsNullOrWhiteSpace(requiredIdentityToken) && string.IsNullOrWhiteSpace(metadataIdentityToken))
            return true;
        if (string.IsNullOrWhiteSpace(requiredIdentityToken) || string.IsNullOrWhiteSpace(metadataIdentityToken))
            return false;

        return string.Equals(requiredIdentityToken, metadataIdentityToken, StringComparison.Ordinal);
    }

    private readonly record struct RequiredViewportCandidate(
        string ViewportId,
        int Width,
        int Height,
        double DeviceScaleFactor,
        string? IdentityToken);

    private static string ViewportIdFromJudgeId(string judgeRequestId, IReadOnlyList<string> requiredViewportIds)
    {
        foreach (var viewportId in requiredViewportIds)
        {
            var suffix = viewportId.Replace("source-", string.Empty, StringComparison.Ordinal);
            if (judgeRequestId.Contains(suffix, StringComparison.OrdinalIgnoreCase) ||
                judgeRequestId.Contains(viewportId, StringComparison.OrdinalIgnoreCase))
            {
                return viewportId;
            }
        }

        return requiredViewportIds.Count == 1 ? requiredViewportIds[0] : string.Empty;
    }

    private static bool SameSet(IReadOnlyList<string>? left, IReadOnlyList<string> right)
    {
        var leftSet = (left ?? [])
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value.Trim())
            .ToHashSet(StringComparer.Ordinal);
        var rightSet = right.ToHashSet(StringComparer.Ordinal);
        return leftSet.SetEquals(rightSet);
    }

    private static string HashRef(string scope, string value)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(scope + "\n" + value))).ToLowerInvariant();

    private static string ShortDigest(string digest)
    {
        var clean = digest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)
            ? digest["sha256:".Length..]
            : digest;
        return clean.Length <= 16 ? clean : clean[..16];
    }

    private static bool IsValidSha256(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
        var text = value.Trim();
        if (!text.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
            return false;
        var hex = text["sha256:".Length..];
        return hex.Length == 64 && hex.All(static c => char.IsAsciiHexDigit(c));
    }

    private static bool MetadataIntMatches(IReadOnlyDictionary<string, string> metadata, IReadOnlyList<string> keys, int expected)
    {
        foreach (var key in keys)
        {
            if (!metadata.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
                continue;

            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed == expected;
        }

        return true;
    }

    internal static bool MetadataDimensionsMatchPngProof(
        IReadOnlyDictionary<string, string> metadata,
        int expectedLogicalWidth,
        int expectedLogicalHeight)
    {
        if (expectedLogicalWidth <= 0 || expectedLogicalHeight <= 0)
            return false;

        var hasViewportWidth = HasMetadataValue(metadata, "viewport_width");
        var hasViewportHeight = HasMetadataValue(metadata, "viewport_height");
        if (hasViewportWidth != hasViewportHeight)
            return false;
        if (hasViewportWidth &&
            (!TryMetadataPositiveInt(metadata, "viewport_width", out var viewportWidth) ||
             !TryMetadataPositiveInt(metadata, "viewport_height", out var viewportHeight) ||
             viewportWidth != expectedLogicalWidth ||
             viewportHeight != expectedLogicalHeight))
        {
            return false;
        }

        var hasPngWidth = HasMetadataValue(metadata, "png_width");
        var hasPngHeight = HasMetadataValue(metadata, "png_height");
        if (hasPngWidth != hasPngHeight)
            return false;
        if (!hasPngWidth)
            return false;
        if (!TryMetadataPositiveInt(metadata, "png_width", out var pngWidth) ||
            !TryMetadataPositiveInt(metadata, "png_height", out var pngHeight))
        {
            return false;
        }

        if (HasMetadataValue(metadata, DesignConversionRenderResolutionFactory.DeviceScaleFactorMetadataKey))
        {
            if (!double.TryParse(
                    FirstMetadata(metadata, DesignConversionRenderResolutionFactory.DeviceScaleFactorMetadataKey),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var scale) ||
                !double.IsFinite(scale) ||
                scale <= 0)
            {
                return false;
            }

            var physical = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
                new RenderQaResolution("human-approval-png-proof", expectedLogicalWidth, expectedLogicalHeight, scale),
                "DESIGN_CONVERSION_APPROVAL_PNG_PROOF_INVALID",
                "DesignConversion human approval PNG proof");
            return physical.IsSuccess &&
                   pngWidth == physical.Value!.PhysicalWidth &&
                   pngHeight == physical.Value.PhysicalHeight;
        }

        return pngWidth == expectedLogicalWidth && pngHeight == expectedLogicalHeight;
    }

    private static bool HasMetadataValue(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value);

    private static bool TryMetadataPositiveInt(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        out int parsed)
    {
        parsed = 0;
        return metadata.TryGetValue(key, out var value) &&
               !string.IsNullOrWhiteSpace(value) &&
               int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) &&
               parsed > 0;
    }

    private static bool MetadataTruthy(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           (string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "1", StringComparison.Ordinal) ||
            string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));

    private static bool MetadataFalse(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) &&
           (string.Equals(value, bool.FalseString, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "0", StringComparison.Ordinal) ||
            string.Equals(value, "no", StringComparison.OrdinalIgnoreCase));

    private static bool MetadataEquals(IReadOnlyDictionary<string, string> metadata, string key, string expected)
        => metadata.TryGetValue(key, out var value) &&
           string.Equals(value, expected, StringComparison.Ordinal);

    private static string SanitizeArtifactUriRef(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var trimmed = value.Trim();
        if (trimmed.StartsWith("file:", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith(@"\\", StringComparison.Ordinal) ||
            trimmed.StartsWith("/", StringComparison.Ordinal) ||
            (trimmed.Length >= 3 && char.IsAsciiLetter(trimmed[0]) && trimmed[1] == ':' && (trimmed[2] == '\\' || trimmed[2] == '/')))
        {
            return "local-png-ref:" + HashRef("design-conversion-human-approval-local-uri", trimmed);
        }

        return SanitizeScalar(trimmed);
    }

    private static string FirstMetadata(IReadOnlyDictionary<string, string> metadata, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return string.Empty;
    }

    private static IReadOnlyDictionary<string, string> SanitizeMetadata(IReadOnlyDictionary<string, string>? metadata)
    {
        var sanitized = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in metadata ?? new Dictionary<string, string>())
        {
            var key = SanitizeScalar(pair.Key);
            if (string.IsNullOrWhiteSpace(key))
                continue;
            sanitized[key] = SanitizeScalar(pair.Value);
        }

        return sanitized;
    }

    private static string SanitizeReason(string value)
    {
        var sanitized = SanitizeScalar(value);
        return sanitized.Length <= 256 ? sanitized : HashRef("approval-reason", sanitized);
    }

    private static string SanitizeScalar(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var trimmed = value.Trim();
        if (trimmed.Length > 512 ||
            trimmed.Contains("<html", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("<style", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("PRIVATE", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("SECRET", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("TOKEN", StringComparison.OrdinalIgnoreCase))
        {
            return HashRef("redacted-human-approval-scalar", trimmed);
        }

        return trimmed;
    }

    private static IReadOnlyDictionary<string, string> MergeMetadata(
        IReadOnlyDictionary<string, string>? first,
        IReadOnlyDictionary<string, string>? second)
    {
        var merged = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in first ?? new Dictionary<string, string>())
            merged[pair.Key] = pair.Value;
        foreach (var pair in second ?? new Dictionary<string, string>())
            merged[pair.Key] = pair.Value;
        return merged;
    }
}
