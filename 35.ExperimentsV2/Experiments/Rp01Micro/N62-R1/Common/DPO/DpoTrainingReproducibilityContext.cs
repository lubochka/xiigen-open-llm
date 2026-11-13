// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only

namespace DPO;

using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using LoRA;

public sealed record DpoTrainingReproducibilityContext(
    int SchemaVersion,
    ulong RootInitializationSeed,
    string TrainingConfigurationFingerprint,
    DpoTrainingSeededLoraTopology SeededLoraTopology,
    string RandomDerivationScheme,
    string InitializationDomain,
    string DropoutDomain,
    string PolicyStreamDomain,
    string ReferenceStreamDomain,
    string FreshLoadStreamDomain,
    string ReproducibilityScope)
{
    public static DpoTrainingReproducibilityContext CreateCurrent(
        ulong rootInitializationSeed,
        string trainingConfigurationFingerprint,
        DpoTrainingSeededLoraTopology seededLoraTopology,
        string policyStreamDomain,
        string referenceStreamDomain,
        string freshLoadStreamDomain)
    {
        // Этот объект переносит серверную власть над случайностью в доменный
        // runner: seed, fingerprint конфигурации и имена потоков должны прийти
        // от ModelServer, а не быть заново придуманы внутри обучения перевода.
        var context = new DpoTrainingReproducibilityContext(
            DpoTrainingReproducibilityContextContract.CurrentSchemaVersion,
            rootInitializationSeed,
            trainingConfigurationFingerprint,
            seededLoraTopology,
            LoraAdapterRandomSeedDerivation.DerivationScheme,
            LoraAdapterRandomSeedDerivation.InitializationDomain,
            LoraAdapterRandomSeedDerivation.DropoutDomain,
            policyStreamDomain,
            referenceStreamDomain,
            freshLoadStreamDomain,
            DpoTrainingReproducibilityContextContract
                .InitializationOnlyReproducibilityScope);
        if (!DpoTrainingReproducibilityContextContract.TryValidateCurrent(
                context,
                out var reason))
        {
            throw new ArgumentException(reason);
        }

        return context;
    }
}

public sealed record DpoTrainingReproducibilityEvidence(
    DpoTrainingReproducibilityContext Context,
    string ContextSha256,
    IReadOnlyList<string> EvidenceRefs)
{
    public static DpoTrainingReproducibilityEvidence CreateCurrent(
        DpoTrainingReproducibilityContext context)
    {
        if (!DpoTrainingReproducibilityContextContract.TryValidateCurrent(
                context,
                out var reason))
        {
            throw new ArgumentException(reason, nameof(context));
        }

        var hash = DpoTrainingReproducibilityContextContract
            .ComputeCanonicalSha256(context);
        // Отчёт хранит не только поля контекста, но и короткий канонический
        // отпечаток: continuation сравнивает именно его, чтобы не продолжить
        // checkpoint под другой серверной конфигурацией.
        return new DpoTrainingReproducibilityEvidence(
            context,
            hash,
            DpoTrainingReproducibilityContextContract.BuildRequiredEvidenceRefs(
                context,
                hash));
    }
}

public static class DpoTrainingReproducibilityContextContract
{
    public const int CurrentSchemaVersion = 2;
    public const string InitializationOnlyReproducibilityScope =
        "initialization-only-no-mid-step-dropout-resume";
    public const string TypedEvidence =
        "DPO_TRAINING_REPRODUCIBILITY_CONTEXT_TYPED";
    public const string InitializationReproducibilityOnlyEvidence =
        "DPO_TRAINING_INITIALIZATION_REPRODUCIBILITY_ONLY";
    public const string RootInitializationSeedEvidencePrefix =
        "dpo_training_root_initialization_seed:";
    public const string TrainingConfigurationFingerprintEvidencePrefix =
        "dpo_training_configuration_fingerprint:";
    public const string SeededLoraTopologySha256EvidencePrefix =
        "dpo_training_seeded_lora_topology_sha256:";
    public const string RandomDerivationSchemeEvidencePrefix =
        "dpo_training_random_derivation_scheme:";
    public const string ContextSha256EvidencePrefix =
        "dpo_training_reproducibility_context_sha256:";

    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public static bool IsValidCurrent(
        DpoTrainingReproducibilityContext? context)
        => TryValidateCurrent(context, out _);

    public static bool TryValidateCurrent(
        DpoTrainingReproducibilityContext? context,
        out string reason)
    {
        reason = string.Empty;
        if (context is null)
        {
            reason = "DPO training reproducibility context is required.";
            return false;
        }

        if (context.SchemaVersion != CurrentSchemaVersion)
        {
            reason = "DPO training reproducibility context schema is not current.";
            return false;
        }

        if (!IsCanonicalFingerprint(context.TrainingConfigurationFingerprint))
        {
            reason = "DPO training configuration fingerprint must be 64 lowercase hexadecimal characters.";
            return false;
        }

        if (!DpoTrainingSeededLoraTopologyContract.TryValidateTopology(
                context.SeededLoraTopology,
                out reason))
        {
            // Серверная власть над воспроизводимостью теперь включает форму
            // LoRA, а не только seed. Иначе одинаковый seed мог бы породить
            // разные checkpoint-файлы при другой rank/alpha/target-конфигурации.
            return false;
        }

        if (!string.Equals(
                context.RandomDerivationScheme,
                LoraAdapterRandomSeedDerivation.DerivationScheme,
                StringComparison.Ordinal) ||
            !string.Equals(
                context.InitializationDomain,
                LoraAdapterRandomSeedDerivation.InitializationDomain,
                StringComparison.Ordinal) ||
            !string.Equals(
                context.DropoutDomain,
                LoraAdapterRandomSeedDerivation.DropoutDomain,
                StringComparison.Ordinal))
        {
            // Здесь контракт привязан к реальным константам LoRA-слоя: если
            // генерация seed изменилась, старый контекст нельзя молча принять.
            reason = "DPO training reproducibility context does not match the active LoRA random derivation contract.";
            return false;
        }

        if (!string.Equals(
                context.ReproducibilityScope,
                InitializationOnlyReproducibilityScope,
                StringComparison.Ordinal))
        {
            reason = "DPO training reproducibility context scope is not the initialization-only scope.";
            return false;
        }

        if (!IsCanonicalDomain(context.PolicyStreamDomain) ||
            !IsCanonicalDomain(context.ReferenceStreamDomain) ||
            !IsCanonicalDomain(context.FreshLoadStreamDomain))
        {
            reason = "DPO training reproducibility stream domains must be nonempty canonical domains.";
            return false;
        }

        if (new[]
            {
                context.PolicyStreamDomain,
                context.ReferenceStreamDomain,
                context.FreshLoadStreamDomain
            }.Distinct(StringComparer.Ordinal).Count() != 3)
        {
            // Policy, reference и fresh-load получают разные домены, чтобы
            // два независимых потока случайности не стали одним и тем же seed.
            reason = "DPO training reproducibility stream domains must be distinct.";
            return false;
        }

        return true;
    }

    public static bool IsValidEvidence(
        DpoTrainingReproducibilityEvidence? evidence)
        => TryValidateEvidence(evidence, out _);

    public static bool TryValidateEvidence(
        DpoTrainingReproducibilityEvidence? evidence,
        out string reason)
    {
        reason = string.Empty;
        if (evidence is null)
        {
            reason = "DPO training reproducibility evidence is required.";
            return false;
        }

        if (!TryValidateCurrent(evidence.Context, out reason))
            return false;

        var expectedHash = ComputeCanonicalSha256(evidence.Context);
        if (!string.Equals(
                evidence.ContextSha256,
                expectedHash,
                StringComparison.Ordinal))
        {
            // Любая ручная правка evidence становится видна до чтения
            // checkpoint-файлов: hash обязан совпасть с вложенным контекстом.
            reason = "DPO training reproducibility evidence hash does not match its context.";
            return false;
        }

        if (evidence.EvidenceRefs is null)
        {
            reason = "DPO training reproducibility evidence refs are required.";
            return false;
        }

        var required = BuildRequiredEvidenceRefs(evidence.Context, expectedHash);
        if (!required.All(item =>
                evidence.EvidenceRefs.Contains(item, StringComparer.Ordinal)))
        {
            reason = "DPO training reproducibility evidence refs are incomplete.";
            return false;
        }

        if (!HasExactlyOneCanonicalReservedEvidenceRef(
                evidence.EvidenceRefs,
                RootInitializationSeedEvidencePrefix,
                $"{RootInitializationSeedEvidencePrefix}" +
                evidence.Context.RootInitializationSeed.ToString(
                    CultureInfo.InvariantCulture)) ||
            !HasExactlyOneCanonicalReservedEvidenceRef(
                evidence.EvidenceRefs,
                TrainingConfigurationFingerprintEvidencePrefix,
                $"{TrainingConfigurationFingerprintEvidencePrefix}" +
                evidence.Context.TrainingConfigurationFingerprint) ||
            !HasExactlyOneCanonicalReservedEvidenceRef(
                evidence.EvidenceRefs,
                SeededLoraTopologySha256EvidencePrefix,
                $"{SeededLoraTopologySha256EvidencePrefix}" +
                DpoTrainingSeededLoraTopologyContract
                    .ComputeCanonicalSha256(
                        evidence.Context.SeededLoraTopology)) ||
            !HasExactlyOneCanonicalReservedEvidenceRef(
                evidence.EvidenceRefs,
                RandomDerivationSchemeEvidencePrefix,
                $"{RandomDerivationSchemeEvidencePrefix}" +
                evidence.Context.RandomDerivationScheme) ||
            !HasExactlyOneCanonicalReservedEvidenceRef(
                evidence.EvidenceRefs,
                ContextSha256EvidencePrefix,
                $"{ContextSha256EvidencePrefix}{expectedHash}"))
        {
            reason = "DPO training reproducibility evidence refs contain a tampered reserved value.";
            return false;
        }

        return true;
    }

    public static string ComputeCanonicalSha256(
        DpoTrainingReproducibilityContext context)
    {
        if (!TryValidateCurrent(context, out var reason))
            throw new ArgumentException(reason, nameof(context));

        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        // Фрейминг длинами защищает hash от двусмысленных склеек строк:
        // разные наборы полей не могут дать один и тот же байтовый поток.
        AppendString(hash, "dpo-training-reproducibility-context-v2");
        AppendInt32(hash, context.SchemaVersion);
        AppendUInt64(hash, context.RootInitializationSeed);
        AppendString(hash, context.TrainingConfigurationFingerprint);
        AppendString(
            hash,
            DpoTrainingSeededLoraTopologyContract.ComputeCanonicalSha256(
                context.SeededLoraTopology));
        AppendString(hash, context.RandomDerivationScheme);
        AppendString(hash, context.InitializationDomain);
        AppendString(hash, context.DropoutDomain);
        AppendString(hash, context.PolicyStreamDomain);
        AppendString(hash, context.ReferenceStreamDomain);
        AppendString(hash, context.FreshLoadStreamDomain);
        AppendString(hash, context.ReproducibilityScope);
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    public static IReadOnlyList<string> BuildRequiredEvidenceRefs(
        DpoTrainingReproducibilityContext context,
        string contextSha256)
    {
        if (!TryValidateCurrent(context, out var reason))
            throw new ArgumentException(reason, nameof(context));
        if (!IsCanonicalFingerprint(contextSha256))
        {
            throw new ArgumentException(
                "DPO training reproducibility context hash must be canonical SHA-256.",
                nameof(contextSha256));
        }

        return
        [
            TypedEvidence,
            InitializationReproducibilityOnlyEvidence,
            $"{RootInitializationSeedEvidencePrefix}" +
            context.RootInitializationSeed.ToString(CultureInfo.InvariantCulture),
            $"{TrainingConfigurationFingerprintEvidencePrefix}" +
            context.TrainingConfigurationFingerprint,
            $"{SeededLoraTopologySha256EvidencePrefix}" +
            DpoTrainingSeededLoraTopologyContract.ComputeCanonicalSha256(
                context.SeededLoraTopology),
            $"{RandomDerivationSchemeEvidencePrefix}" +
            context.RandomDerivationScheme,
            $"{ContextSha256EvidencePrefix}{contextSha256}"
        ];
    }

    private static bool HasExactlyOneCanonicalReservedEvidenceRef(
        IReadOnlyList<string> evidenceRefs,
        string prefix,
        string expected)
    {
        var matchCount = 0;
        foreach (var evidenceRef in evidenceRefs)
        {
            if (evidenceRef is null ||
                !evidenceRef.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            matchCount++;
            if (matchCount > 1 ||
                !string.Equals(evidenceRef, expected, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return matchCount == 1;
    }

    public static bool IsCanonicalFingerprint(string? value)
    {
        if (value is not { Length: 64 })
            return false;
        foreach (var character in value)
        {
            if (!((character >= '0' && character <= '9') ||
                  (character >= 'a' && character <= 'f')))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsCanonicalDomain(string? value)
    {
        if (value is null ||
            value.Length is < 1 or > 64 ||
            value[0] is < 'a' or > 'z')
        {
            return false;
        }

        for (var index = 1; index < value.Length; index++)
        {
            var character = value[index];
            if (!((character >= 'a' && character <= 'z') ||
                  (character >= '0' && character <= '9') ||
                  character is '.' or '-'))
            {
                return false;
            }
        }

        return true;
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

    private static void AppendUInt64(IncrementalHash hash, ulong value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(bytes, value);
        hash.AppendData(bytes);
    }
}
