// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

/// <summary>
/// Exclusive process lease for one accounting-reconciliation batch. The lease
/// lives beside, rather than inside, the strict post-evaluation inventory.
/// </summary>
public sealed class DesignConversionAccountingReconciliationLease : IDisposable
{
    public const string LeaseHeldErrorCode =
        "DESIGN_CONVERSION_ACCOUNTING_RECONCILIATION_LEASE_HELD";
    public const string LeaseUnavailableErrorCode =
        "DESIGN_CONVERSION_ACCOUNTING_RECONCILIATION_LEASE_UNAVAILABLE";
    public const string LeaseSafeMessage =
        "DesignConversion accounting reconciliation lease is unavailable.";
    public const string CancelledErrorCode = "CANCELLED";
    public const string LeaseDirectoryNamePrefix = "accounting-reconciliation-lifecycle-";
    public const string LeaseFileName = ".accounting-reconciliation.lease";

    private readonly DesignConversionHandleBoundDirectory _directory;
    private readonly FileStream _stream;
    private bool _disposed;

    private DesignConversionAccountingReconciliationLease(
        DesignConversionHandleBoundDirectory directory,
        FileStream stream)
    {
        _directory = directory;
        _stream = stream;
    }

    public string LeaseRootDirectory { get; private init; } = string.Empty;

    public static OperationResult<DesignConversionAccountingReconciliationLease> TryAcquire(
        string postEvaluationStateRootDirectory,
        string visualCostBudgetStoreRootDirectory,
        string hostScope)
        => TryAcquireOnce(
            postEvaluationStateRootDirectory,
            visualCostBudgetStoreRootDirectory,
            hostScope);

    public static async Task<OperationResult<DesignConversionAccountingReconciliationLease>>
        TryAcquireAsync(
            string postEvaluationStateRootDirectory,
            string visualCostBudgetStoreRootDirectory,
            string hostScope,
            TimeSpan acquisitionTimeout,
            TimeSpan retryDelay,
            CancellationToken cancellationToken = default)
    {
        if (acquisitionTimeout < TimeSpan.Zero ||
            retryDelay <= TimeSpan.Zero)
        {
            return OperationResult<DesignConversionAccountingReconciliationLease>.Fail(
                LeaseSafeMessage,
                LeaseUnavailableErrorCode);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<DesignConversionAccountingReconciliationLease>.Fail(
                LeaseSafeMessage,
                CancelledErrorCode);
        }

        var started = Stopwatch.GetTimestamp();
        while (true)
        {
            var attempt = TryAcquireOnce(
                postEvaluationStateRootDirectory,
                visualCostBudgetStoreRootDirectory,
                hostScope);
            if (attempt.IsSuccess ||
                !string.Equals(
                    attempt.ErrorCode,
                    LeaseHeldErrorCode,
                    StringComparison.Ordinal))
            {
                return attempt;
            }

            var elapsed = Stopwatch.GetElapsedTime(started);
            if (elapsed >= acquisitionTimeout)
                return attempt;

            var remaining = acquisitionTimeout - elapsed;
            var delay = remaining < retryDelay ? remaining : retryDelay;
            try
            {
                await Task.Delay(delay, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return OperationResult<DesignConversionAccountingReconciliationLease>.Fail(
                    LeaseSafeMessage,
                    CancelledErrorCode);
            }
        }
    }

    private static OperationResult<DesignConversionAccountingReconciliationLease>
        TryAcquireOnce(
            string postEvaluationStateRootDirectory,
            string visualCostBudgetStoreRootDirectory,
            string hostScope)
    {
        if (string.IsNullOrWhiteSpace(postEvaluationStateRootDirectory) ||
            !Path.IsPathFullyQualified(postEvaluationStateRootDirectory) ||
            string.IsNullOrWhiteSpace(visualCostBudgetStoreRootDirectory) ||
            !Path.IsPathFullyQualified(visualCostBudgetStoreRootDirectory) ||
            !TryCanonicalizeHostScope(hostScope, out var canonicalHostScope))
        {
            return OperationResult<DesignConversionAccountingReconciliationLease>.Fail(
                LeaseSafeMessage,
                LeaseUnavailableErrorCode);
        }

        DesignConversionHandleBoundDirectory? directory = null;
        try
        {
            var exactStateRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(postEvaluationStateRootDirectory));
            var exactBudgetRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(visualCostBudgetStoreRootDirectory));
            var leaseAnchor = Path.GetDirectoryName(exactBudgetRoot);
            var strictArtifactRoot = Path.GetDirectoryName(exactStateRoot);
            if (string.IsNullOrWhiteSpace(leaseAnchor) ||
                string.IsNullOrWhiteSpace(strictArtifactRoot))
            {
                return OperationResult<DesignConversionAccountingReconciliationLease>.Fail(
                    LeaseSafeMessage,
                    LeaseUnavailableErrorCode);
            }

            var leaseRoot = Path.Combine(
                leaseAnchor,
                BuildLeaseDirectoryName(
                    exactStateRoot,
                    exactBudgetRoot,
                    canonicalHostScope));
            if (IsSameOrDescendant(leaseRoot, strictArtifactRoot) ||
                IsSameOrDescendant(leaseRoot, exactStateRoot) ||
                IsSameOrDescendant(leaseRoot, exactBudgetRoot))
            {
                return OperationResult<DesignConversionAccountingReconciliationLease>.Fail(
                    LeaseSafeMessage,
                    LeaseUnavailableErrorCode);
            }

            directory = DesignConversionHandleBoundDirectory.AcquireOrCreate(
                leaseRoot,
                beforeIo: null);
            var stream = directory.OpenOrCreate(
                LeaseFileName,
                FileShare.None,
                writeThrough: true);
            if (stream.Length != 0L)
            {
                stream.Dispose();
                directory.Dispose();
                return OperationResult<DesignConversionAccountingReconciliationLease>.Fail(
                    LeaseSafeMessage,
                    LeaseUnavailableErrorCode);
            }

            var lease = new DesignConversionAccountingReconciliationLease(
                directory,
                stream)
            {
                LeaseRootDirectory = leaseRoot
            };
            directory = null;
            return OperationResult<DesignConversionAccountingReconciliationLease>.Ok(lease);
        }
        catch (DesignConversionHandleBoundSharingViolationException)
        {
            directory?.Dispose();
            return OperationResult<DesignConversionAccountingReconciliationLease>.Fail(
                LeaseSafeMessage,
                LeaseHeldErrorCode);
        }
        catch (Exception ex) when (
            ex is IOException or UnauthorizedAccessException or
                InvalidDataException or PlatformNotSupportedException or
                ArgumentException or NotSupportedException)
        {
            directory?.Dispose();
            return OperationResult<DesignConversionAccountingReconciliationLease>.Fail(
                LeaseSafeMessage,
                LeaseUnavailableErrorCode);
        }
    }

    private static string BuildLeaseDirectoryName(
        string exactStateRoot,
        string exactBudgetRoot,
        string hostScope)
    {
        var identity = string.Join(
            "\n",
            OperatingSystem.IsWindows()
                ? exactStateRoot.ToUpperInvariant()
                : exactStateRoot,
            OperatingSystem.IsWindows()
                ? exactBudgetRoot.ToUpperInvariant()
                : exactBudgetRoot,
            hostScope.Trim());
        var hash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(identity)))
            .ToLowerInvariant();
        return LeaseDirectoryNamePrefix + hash;
    }

    private static bool TryCanonicalizeHostScope(
        string? hostScope,
        out string canonicalHostScope)
    {
        canonicalHostScope = string.Empty;
        if (string.IsNullOrWhiteSpace(hostScope))
            return false;

        var candidate = hostScope
            .Normalize(NormalizationForm.FormKC)
            .Trim()
            .ToLowerInvariant();
        if (candidate.Length is < 1 or > 64 ||
            candidate.Any(ch =>
                ch is not (>= 'a' and <= 'z') &&
                ch is not (>= '0' and <= '9') &&
                ch is not '_' and not '-'))
        {
            return false;
        }

        canonicalHostScope = candidate;
        return true;
    }

    private static bool IsSameOrDescendant(string candidate, string root)
    {
        var relative = Path.GetRelativePath(root, candidate);
        return !Path.IsPathRooted(relative) &&
               !string.Equals(relative, "..", StringComparison.Ordinal) &&
               !relative.StartsWith(
                   ".." + Path.DirectorySeparatorChar,
                   StringComparison.Ordinal) &&
               !relative.StartsWith(
                   ".." + Path.AltDirectorySeparatorChar,
                   StringComparison.Ordinal);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _stream.Dispose();
        _directory.Dispose();
    }
}
