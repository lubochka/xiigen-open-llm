// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace TransformerModel;

/// <summary>
/// Neutral stochastic state owned by a transformer adapter. This carrier is
/// intentionally independent from LoRA and DPO so the model layer can expose
/// a portable continuation boundary without a dependency cycle.
/// </summary>
public sealed record TransformerAdapterRandomState(
    string StreamDomain,
    ulong InitializationSeed,
    ulong DropoutSeed,
    ulong ConsumedCursor);

public sealed record LlmModelRandomStateEntry(
    string AdapterKey,
    string StreamDomain,
    ulong InitializationSeed,
    ulong DropoutSeed,
    ulong ConsumedCursor);

public sealed record LlmModelRandomStateIdentity(
    string TopologySha256,
    string LoraConfigurationSha256);

public sealed record LlmModelRandomStateSnapshot(
    uint FormatVersion,
    string Algorithm,
    IReadOnlyList<LlmModelRandomStateEntry> Entries,
    string TopologySha256,
    string LoraConfigurationSha256,
    int RandomStateStep)
{
    public const uint CurrentFormatVersion = 1;
    public const string CurrentAlgorithm = "splitmix64/v1";
}
