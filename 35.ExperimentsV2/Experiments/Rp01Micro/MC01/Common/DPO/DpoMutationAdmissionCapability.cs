// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DPO;

/// <summary>
/// Internal proof that a DPO mutation capability was issued by a trusted
/// admission boundary. Public callers cannot implement this marker.
/// </summary>
internal interface IDpoMutationAdmissionIssuer
{
    IDisposable AcquireDpoTrainerMutationPermit();
}

/// <summary>
/// Opaque, revocable authority required by the low-level DPO trainer.
/// </summary>
public sealed class DpoMutationAdmissionCapability
{
    public const string RequiredErrorCode =
        "DPO_TRAINER_MUTATION_ADMISSION_CAPABILITY_REQUIRED";

    private readonly IDpoMutationAdmissionIssuer _issuer;

    private DpoMutationAdmissionCapability(
        IDpoMutationAdmissionIssuer issuer)
    {
        _issuer = issuer;
    }

    internal static DpoMutationAdmissionCapability Issue(
        IDpoMutationAdmissionIssuer issuer)
    {
        ArgumentNullException.ThrowIfNull(issuer);
        return new DpoMutationAdmissionCapability(issuer);
    }

    internal IDisposable AcquireMutationPermit()
        => _issuer.AcquireDpoTrainerMutationPermit()
           ?? throw new DpoMutationAdmissionException(
               "A live DPO trainer mutation permit is required.",
               RequiredErrorCode);
}

/// <summary>
/// Stable fail-closed exception emitted when low-level DPO mutation authority
/// is absent, revoked, or otherwise unavailable.
/// </summary>
public sealed class DpoMutationAdmissionException : InvalidOperationException
{
    internal DpoMutationAdmissionException(
        string message,
        string errorCode)
        : base($"{errorCode}: {message}")
    {
        ErrorCode = string.IsNullOrWhiteSpace(errorCode)
            ? DpoMutationAdmissionCapability.RequiredErrorCode
            : errorCode;
    }

    public string ErrorCode { get; }
}
