// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace DPO;

using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using LoRA;
using TransformerModel;

public sealed record DpoTrainingSeededLoraTopology(
    int Rank,
    float Alpha,
    float Dropout,
    IReadOnlyList<string> OrderedTargetModules)
{
    public static DpoTrainingSeededLoraTopology FromLoraConfig(
        LoraConfig config)
    {
        var canonical = LoraApplicator.ValidateAndCanonicalize(config);
        return new DpoTrainingSeededLoraTopology(
            canonical.Rank,
            canonical.Alpha,
            canonical.Dropout,
            canonical.TargetModules.ToArray());
    }

    public LoraConfig ToLoraConfig()
    {
        if (!DpoTrainingSeededLoraTopologyContract.TryValidateTopology(
                this,
                out var reason))
        {
            throw new ArgumentException(reason, nameof(OrderedTargetModules));
        }

        var canonical = LoraApplicator.ValidateAndCanonicalize(new LoraConfig
        {
            Rank = Rank,
            Alpha = Alpha,
            Dropout = Dropout,
            TargetModules = OrderedTargetModules.ToArray()
        });
        if (canonical.Rank != Rank ||
            BitConverter.SingleToInt32Bits(canonical.Alpha) !=
            BitConverter.SingleToInt32Bits(Alpha) ||
            BitConverter.SingleToInt32Bits(canonical.Dropout) !=
            BitConverter.SingleToInt32Bits(Dropout) ||
            !canonical.TargetModules.SequenceEqual(
                OrderedTargetModules,
                StringComparer.Ordinal))
        {
            throw new ArgumentException(
                "DPO seeded LoRA topology is not canonical.",
                nameof(OrderedTargetModules));
        }

        return canonical;
    }
}

public enum DpoTrainingSeededLoraModelRole
{
    Policy,
    Reference,
    FreshLoad
}

public sealed record DpoTrainingSeededLoraTopologyApplicationEvidence(
    DpoTrainingSeededLoraTopology Topology,
    string TopologySha256,
    int PolicyAdapterCount,
    int ReferenceAdapterCount,
    int FreshLoadAdapterCount,
    string PolicyStreamDomain,
    string ReferenceStreamDomain,
    string FreshLoadStreamDomain,
    IReadOnlyList<string> EvidenceRefs);

public static class DpoTrainingSeededLoraTopologyPreparer
{
    public static DpoTrainingSeededLoraTopologyApplicationEvidence
        ApplyFreshSeededTriple(
            LlmModel policyModel,
            LlmModel referenceModel,
            LlmModel freshLoadModel,
            DpoTrainingReproducibilityContext context)
    {
        ValidateContextOrThrow(context);
        // Три модели готовятся одним атомарным вызовом LoRA: если fresh-load
        // уже занят или домены перепутаны, policy/reference не остаются
        // наполовину изменёнными.
        var application = LoraApplicator.ApplyFreshSeededTriple(
            policyModel,
            referenceModel,
            freshLoadModel,
            context.SeededLoraTopology.ToLoraConfig(),
            context.RootInitializationSeed,
            context.PolicyStreamDomain,
            context.ReferenceStreamDomain,
            context.FreshLoadStreamDomain);

        return BuildEvidence(
            context,
            application.PolicyModelAdapters.Count,
            application.ReferenceModelAdapters.Count,
            application.FreshLoadModelAdapters.Count);
    }

    public static DpoTrainingSeededLoraTopologyApplicationEvidence
        ValidateSeededTriple(
            LlmModel policyModel,
            LlmModel referenceModel,
            LlmModel freshLoadModel,
            DpoTrainingReproducibilityContext context)
    {
        var policyAdapters = ValidateSeededRole(
            policyModel,
            context,
            DpoTrainingSeededLoraModelRole.Policy);
        var referenceAdapters = ValidateSeededRole(
            referenceModel,
            context,
            DpoTrainingSeededLoraModelRole.Reference);
        var freshLoadAdapters = ValidateSeededRole(
            freshLoadModel,
            context,
            DpoTrainingSeededLoraModelRole.FreshLoad);

        return BuildEvidence(
            context,
            policyAdapters.Count,
            referenceAdapters.Count,
            freshLoadAdapters.Count);
    }

    public static IReadOnlyList<LoraLinear> ValidateSeededRole(
        LlmModel model,
        DpoTrainingReproducibilityContext context,
        DpoTrainingSeededLoraModelRole role)
    {
        ValidateContextOrThrow(context);
        var domain = role switch
        {
            DpoTrainingSeededLoraModelRole.Policy =>
                context.PolicyStreamDomain,
            DpoTrainingSeededLoraModelRole.Reference =>
                context.ReferenceStreamDomain,
            DpoTrainingSeededLoraModelRole.FreshLoad =>
                context.FreshLoadStreamDomain,
            _ => throw new ArgumentOutOfRangeException(
                nameof(role),
                "DPO seeded LoRA role is unknown.")
        };

        // Проверка использует тот же root seed и роль-домен, что и серверный
        // контекст. Поэтому reference нельзя выдать за policy простым
        // совпадением формы адаптеров.
        return LoraApplicator.ValidateSeededApplicationProvenance(
            model,
            context.SeededLoraTopology.ToLoraConfig(),
            (blockIndex, projection) =>
                LoraAdapterRandomSeedDerivation.Derive(
                    context.RootInitializationSeed,
                    blockIndex,
                    projection,
                    domain));
    }

    private static void ValidateContextOrThrow(
        DpoTrainingReproducibilityContext context)
    {
        if (!DpoTrainingReproducibilityContextContract.TryValidateCurrent(
                context,
                out var reason))
        {
            throw new ArgumentException(reason, nameof(context));
        }
    }

    private static DpoTrainingSeededLoraTopologyApplicationEvidence
        BuildEvidence(
            DpoTrainingReproducibilityContext context,
            int policyAdapterCount,
            int referenceAdapterCount,
            int freshLoadAdapterCount)
    {
        var topologySha256 =
            DpoTrainingSeededLoraTopologyContract.ComputeCanonicalSha256(
                context.SeededLoraTopology);
        return new DpoTrainingSeededLoraTopologyApplicationEvidence(
            context.SeededLoraTopology,
            topologySha256,
            policyAdapterCount,
            referenceAdapterCount,
            freshLoadAdapterCount,
            context.PolicyStreamDomain,
            context.ReferenceStreamDomain,
            context.FreshLoadStreamDomain,
            DpoTrainingSeededLoraTopologyContract
                .BuildApplicationEvidenceRefs(
                    context,
                    topologySha256,
                    policyAdapterCount,
                    referenceAdapterCount,
                    freshLoadAdapterCount));
    }
}

public static class DpoTrainingSeededLoraTopologyContract
{
    public const string TopologySha256EvidencePrefix =
        "dpo_training_seeded_lora_topology_sha256:";
    public const string PolicyAdapterCountEvidencePrefix =
        "dpo_training_seeded_lora_policy_adapter_count:";
    public const string ReferenceAdapterCountEvidencePrefix =
        "dpo_training_seeded_lora_reference_adapter_count:";
    public const string FreshLoadAdapterCountEvidencePrefix =
        "dpo_training_seeded_lora_fresh_load_adapter_count:";

    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public static bool TryValidateTopology(
        DpoTrainingSeededLoraTopology? topology,
        out string reason)
    {
        reason = string.Empty;
        if (topology is null)
        {
            reason = "DPO seeded LoRA topology is required.";
            return false;
        }

        try
        {
            var canonical = LoraApplicator.ValidateAndCanonicalize(
                new LoraConfig
                {
                    Rank = topology.Rank,
                    Alpha = topology.Alpha,
                    Dropout = topology.Dropout,
                    TargetModules =
                        topology.OrderedTargetModules?.ToArray() ??
                        throw new ArgumentException(
                            "DPO seeded LoRA target modules are required.")
                });
            if (canonical.TargetModules.Length == 0)
            {
                reason = "DPO seeded LoRA topology must target at least one projection.";
                return false;
            }

            if (canonical.Rank != topology.Rank ||
                BitConverter.SingleToInt32Bits(canonical.Alpha) !=
                BitConverter.SingleToInt32Bits(topology.Alpha) ||
                BitConverter.SingleToInt32Bits(canonical.Dropout) !=
                BitConverter.SingleToInt32Bits(topology.Dropout) ||
                !canonical.TargetModules.SequenceEqual(
                    topology.OrderedTargetModules,
                    StringComparer.Ordinal))
            {
                reason = "DPO seeded LoRA topology must be canonical and ordered.";
                return false;
            }
        }
        catch (Exception exception) when (
            exception is ArgumentException or InvalidOperationException or OverflowException)
        {
            reason = exception.Message;
            return false;
        }

        return true;
    }

    public static string ComputeCanonicalSha256(
        DpoTrainingSeededLoraTopology topology)
    {
        if (!TryValidateTopology(topology, out var reason))
            throw new ArgumentException(reason, nameof(topology));

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        // Топология входит в отдельный hash, чтобы отчёты могли показать
        // человеку: checkpoint зависит не только от seed, но и от состава LoRA.
        AppendString(hash, "dpo-training-seeded-lora-topology-v1");
        AppendInt32(hash, topology.Rank);
        AppendInt32(hash, BitConverter.SingleToInt32Bits(topology.Alpha));
        AppendInt32(hash, BitConverter.SingleToInt32Bits(topology.Dropout));
        AppendInt32(hash, topology.OrderedTargetModules.Count);
        foreach (var target in topology.OrderedTargetModules)
            AppendString(hash, target);
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    public static bool TryValidateApplicationEvidence(
        DpoTrainingSeededLoraTopologyApplicationEvidence? evidence,
        DpoTrainingReproducibilityContext context,
        out string reason)
    {
        reason = string.Empty;
        if (evidence is null)
        {
            reason = "DPO seeded LoRA topology application evidence is required.";
            return false;
        }

        if (!DpoTrainingReproducibilityContextContract.TryValidateCurrent(
                context,
                out reason))
        {
            return false;
        }

        if (!TryValidateTopology(evidence.Topology, out reason))
            return false;

        var expectedTopologyHash = ComputeCanonicalSha256(
            context.SeededLoraTopology);
        if (!string.Equals(
                evidence.TopologySha256,
                expectedTopologyHash,
                StringComparison.Ordinal) ||
            !string.Equals(
                ComputeCanonicalSha256(evidence.Topology),
                expectedTopologyHash,
                StringComparison.Ordinal))
        {
            reason = "DPO seeded LoRA topology evidence does not match the active context.";
            return false;
        }

        if (evidence.PolicyAdapterCount <= 0 ||
            evidence.ReferenceAdapterCount <= 0 ||
            evidence.FreshLoadAdapterCount <= 0)
        {
            reason = "DPO seeded LoRA topology evidence must contain positive adapter counts.";
            return false;
        }

        if (!string.Equals(
                evidence.PolicyStreamDomain,
                context.PolicyStreamDomain,
                StringComparison.Ordinal) ||
            !string.Equals(
                evidence.ReferenceStreamDomain,
                context.ReferenceStreamDomain,
                StringComparison.Ordinal) ||
            !string.Equals(
                evidence.FreshLoadStreamDomain,
                context.FreshLoadStreamDomain,
                StringComparison.Ordinal))
        {
            reason = "DPO seeded LoRA topology evidence stream domains do not match the active context.";
            return false;
        }

        if (evidence.EvidenceRefs is null)
        {
            reason = "DPO seeded LoRA topology evidence refs are required.";
            return false;
        }

        var required = BuildApplicationEvidenceRefs(
            context,
            expectedTopologyHash,
            evidence.PolicyAdapterCount,
            evidence.ReferenceAdapterCount,
            evidence.FreshLoadAdapterCount);
        if (!required.All(item =>
                evidence.EvidenceRefs.Contains(item, StringComparer.Ordinal)))
        {
            reason = "DPO seeded LoRA topology evidence refs are incomplete.";
            return false;
        }

        return true;
    }

    public static IReadOnlyList<string> BuildApplicationEvidenceRefs(
        DpoTrainingReproducibilityContext context,
        string topologySha256,
        int policyAdapterCount,
        int referenceAdapterCount,
        int freshLoadAdapterCount)
    {
        if (!DpoTrainingReproducibilityContextContract.TryValidateCurrent(
                context,
                out var reason))
        {
            throw new ArgumentException(reason, nameof(context));
        }

        if (!DpoTrainingReproducibilityContextContract.IsCanonicalFingerprint(
                topologySha256))
        {
            throw new ArgumentException(
                "DPO seeded LoRA topology hash must be canonical SHA-256.",
                nameof(topologySha256));
        }

        return
        [
            $"{TopologySha256EvidencePrefix}{topologySha256}",
            $"{PolicyAdapterCountEvidencePrefix}" +
            policyAdapterCount.ToString(CultureInfo.InvariantCulture),
            $"{ReferenceAdapterCountEvidencePrefix}" +
            referenceAdapterCount.ToString(CultureInfo.InvariantCulture),
            $"{FreshLoadAdapterCountEvidencePrefix}" +
            freshLoadAdapterCount.ToString(CultureInfo.InvariantCulture)
        ];
    }

    private static void AppendString(IncrementalHash hash, string value)
    {
        var bytes = StrictUtf8.GetBytes(value);
        AppendInt32(hash, bytes.Length);
        hash.AppendData(bytes);
    }

    private static void AppendInt32(IncrementalHash hash, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        hash.AppendData(bytes);
    }
}
