// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace LoRA;

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using TransformerModel.Config;

public readonly record struct LoraAdapterRandomSeeds(
    ulong InitializationSeed,
    ulong DropoutSeed);

public sealed record LoraPairRandomSeedAuthority(
    ulong RootInitializationSeed,
    string PolicyDropoutStreamDomain,
    string ReferenceDropoutStreamDomain);

/// <summary>
/// Produces the canonical identity frames for deterministic policy/reference
/// adapter topology. This is the sole producer of the XTOP and XLRC hashes.
/// </summary>
public static class LoraRandomStateIdentity
{
    public static string ComputeTopologySha256(
        ModelConfig modelConfig,
        LoraConfig config,
        LoraPairRandomSeedAuthority authority)
    {
        ArgumentNullException.ThrowIfNull(modelConfig);
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(authority);
        var canonical = LoraApplicator.ValidateAndCanonicalize(config);
        ValidateAuthority(modelConfig, canonical, authority);

        using var stream = new MemoryStream();
        stream.Write("XTOP"u8);
        WriteUInt32(stream, 1);
        var policy = BuildEntries(modelConfig, canonical, authority.PolicyDropoutStreamDomain);
        var reference = BuildEntries(modelConfig, canonical, authority.ReferenceDropoutStreamDomain);
        WriteUInt32(stream, checked((uint)policy.Count));
        foreach (var entry in policy)
            WriteTopologyEntry(stream, entry);
        WriteUInt32(stream, checked((uint)reference.Count));
        foreach (var entry in reference)
            WriteTopologyEntry(stream, entry);
        return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
    }

    public static string ComputeLoraConfigurationSha256(
        LoraConfig config,
        LoraPairRandomSeedAuthority authority)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(authority);
        var canonical = LoraApplicator.ValidateAndCanonicalize(config);
        using var stream = new MemoryStream();
        stream.Write("XLRC"u8);
        WriteUInt32(stream, 1);
        WriteString(stream, LoraAdapterRandomSeedDerivation.DerivationScheme);
        WriteUInt64(stream, authority.RootInitializationSeed);
        WriteString(stream, authority.PolicyDropoutStreamDomain);
        WriteString(stream, authority.ReferenceDropoutStreamDomain);
        WriteUInt32(stream, checked((uint)canonical.TargetModules.Length));
        foreach (var target in canonical.TargetModules)
            WriteString(stream, target);
        WriteInt32(stream, canonical.Rank);
        WriteInt32(stream, BitConverter.SingleToInt32Bits(canonical.Alpha));
        WriteInt32(stream, BitConverter.SingleToInt32Bits(canonical.Scaling));
        WriteInt32(stream, BitConverter.SingleToInt32Bits(canonical.Dropout));
        return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
    }

    private static IReadOnlyList<TopologyEntry> BuildEntries(
        ModelConfig modelConfig,
        LoraConfig config,
        string domain)
    {
        var entries = new List<TopologyEntry>(
            checked(modelConfig.NumLayers * config.TargetModules.Length));
        foreach (var blockIndex in Enumerable.Range(0, modelConfig.NumLayers))
        {
            foreach (var projection in config.TargetModules)
            {
                var output = projection is "k_proj" or "v_proj"
                    ? modelConfig.NumKvHeads * modelConfig.HeadDim
                    : modelConfig.HiddenDim;
                entries.Add(new TopologyEntry(
                    $"lora.block_{blockIndex:D4}.{projection}",
                    domain,
                    modelConfig.HiddenDim,
                    output,
                    config.Rank));
            }
        }
        return entries
            .OrderBy(static entry => entry.AdapterKey, StringComparer.Ordinal)
            .ThenBy(static entry => entry.StreamDomain, StringComparer.Ordinal)
            .ToArray();
    }

    private static void ValidateAuthority(
        ModelConfig modelConfig,
        LoraConfig config,
        LoraPairRandomSeedAuthority authority)
    {
        _ = LoraAdapterRandomSeedDerivation.Derive(
            0,
            0,
            "q_proj",
            authority.PolicyDropoutStreamDomain);
        _ = LoraAdapterRandomSeedDerivation.Derive(
            0,
            0,
            "q_proj",
            authority.ReferenceDropoutStreamDomain);
        if (modelConfig.NumLayers < 0 || config.TargetModules.Length > 4096)
            throw new InvalidOperationException("DPO_RANDOM_STATE_IDENTITY_INVALID");
    }

    private static void WriteTopologyEntry(Stream stream, TopologyEntry entry)
    {
        WriteString(stream, entry.AdapterKey);
        WriteString(stream, entry.StreamDomain);
        WriteInt32(stream, entry.BaseInFeatures);
        WriteInt32(stream, entry.BaseOutFeatures);
        WriteInt32(stream, entry.LoraRank);
    }

    private static void WriteString(Stream stream, string value)
    {
        var bytes = StrictUtf8.GetBytes(value);
        WriteUInt32(stream, checked((uint)bytes.Length));
        stream.Write(bytes);
    }

    private static void WriteUInt32(Stream stream, uint value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        stream.Write(bytes);
    }

    private static void WriteUInt64(Stream stream, ulong value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, value);
        stream.Write(bytes);
    }

    private static void WriteInt32(Stream stream, int value)
        => WriteUInt32(stream, unchecked((uint)value));

    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private sealed record TopologyEntry(
        string AdapterKey,
        string StreamDomain,
        int BaseInFeatures,
        int BaseOutFeatures,
        int LoraRank);
}

public static class LoraAdapterRandomSeedDerivation
{
    public const string DerivationScheme =
        "xiigen.lora.adapter-random-seed-derivation/v1";
    public const string InitializationDomain =
        "xiigen.lora.adapter.initialization-seed/v1";
    public const string DropoutDomain =
        "xiigen.lora.adapter.dropout-seed/v1";
    public static LoraAdapterRandomSeeds Derive(
        ulong rootSeed,
        int blockIndex,
        string canonicalProjection,
        string dropoutStreamDomain)
    {
        if (blockIndex < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(blockIndex),
                "LORA_BLOCK_INDEX_INVALID");
        }
        if (canonicalProjection is not
            ("q_proj" or "k_proj" or "v_proj" or "o_proj"))
        {
            throw new ArgumentException(
                "LORA_PROJECTION_INVALID",
                nameof(canonicalProjection));
        }
        if (!IsCanonicalDomain(dropoutStreamDomain))
        {
            throw new ArgumentException(
                "LORA_DROPOUT_STREAM_DOMAIN_INVALID",
                nameof(dropoutStreamDomain));
        }

        return new LoraAdapterRandomSeeds(
            DeriveSeed(
                InitializationDomain,
                rootSeed,
                blockIndex,
                canonicalProjection,
                dropoutStreamDomain: null),
            DeriveSeed(
                DropoutDomain,
                rootSeed,
                blockIndex,
                canonicalProjection,
                dropoutStreamDomain));
    }

    private static ulong DeriveSeed(
        string domain,
        ulong rootSeed,
        int blockIndex,
        string projection,
        string? dropoutStreamDomain)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendFrame(hash, domain);
        Span<byte> root = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(root, rootSeed);
        hash.AppendData(root);
        Span<byte> block = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(block, blockIndex);
        hash.AppendData(block);
        AppendFrame(hash, projection);
        if (dropoutStreamDomain is not null)
            AppendFrame(hash, dropoutStreamDomain);
        var digest = hash.GetHashAndReset();
        return BinaryPrimitives.ReadUInt64BigEndian(digest);
    }

    private static void AppendFrame(IncrementalHash hash, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        Span<byte> length = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(length, bytes.Length);
        hash.AppendData(length);
        hash.AppendData(bytes);
    }

    private static bool IsCanonicalDomain(string? value)
    {
        if (value is null || value.Length is < 1 or > 64
            || value[0] is < 'a' or > 'z')
        {
            return false;
        }
        for (var index = 1; index < value.Length; index++)
        {
            var character = value[index];
            if (!((character >= 'a' && character <= 'z')
                  || (character >= '0' && character <= '9')
                  || character is '.' or '-'))
            {
                return false;
            }
        }
        return true;
    }
}
