// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Collections.Immutable;
using AgentInfrastructure;

/// <summary>
/// Internal learned-weight authority for <see cref="DesignConversionMachine"/>.
/// The contract carries only deterministic, sanitized observations; it never carries
/// a raw preference row or external execution reference.
/// </summary>
public interface IDesignConversionMachineGradientBridge
{
    Task<OperationResult<DesignConversionMachineGradientBridgeResult>> ComputeUpdatesAsync(
        DesignConversionMachineGradientBridgeRequest request,
        CancellationToken ct = default);
}

/// <summary>
/// Sanitized observations for one machine batch. <see cref="BatchIdentityHash"/> is the
/// canonical lowercase SHA-256 identity of the internal batch identifier; the raw batch
/// identifier never crosses this boundary. The version binds returned updates to the
/// state against which the bridge calculated them.
/// </summary>
public sealed record DesignConversionMachineGradientBridgeRequest(
    string BatchIdentityHash,
    string DomainTag,
    string InputMachineVersion,
    ImmutableArray<DesignConversionMachineGradientBridgeSanitizedSignal> SanitizedSignals);

/// <summary>
/// One machine-derived semantic feature observation. Custody identities remain private to
/// the machine and never cross this configurable boundary.
/// </summary>
public sealed record DesignConversionMachineGradientBridgeSanitizedSignal(
    string CanonicalUnitId,
    string AllowedFeatureKey,
    double SignalValue);

/// <summary>
/// Validated learned-weight authority response for exactly one input batch identity and version.
/// <see cref="BatchIdentityHash"/> must echo the canonical lowercase SHA-256 request identity.
/// </summary>
public sealed record DesignConversionMachineGradientBridgeResult(
    string BatchIdentityHash,
    string DomainTag,
    string InputMachineVersion,
    double AverageRewardMargin,
    ImmutableArray<DesignConversionMachineGradientBridgeUpdate> Updates);

/// <summary>
/// One bridge-authoritative semantic contribution. The machine owns the persisted derivation
/// label and attaches its privately authorized custody identities after validation.
/// </summary>
public sealed record DesignConversionMachineGradientBridgeUpdate(
    string CanonicalUnitId,
    string AllowedFeatureKey,
    double Contribution);
