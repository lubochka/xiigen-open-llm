// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

/// <summary>
/// Платный reference-ответ DesignConversion после локальной проверки формы
/// содержимого. Этот тип не выдаёт полномочие сервера: он только держит HTML/CSS
/// в памяти до построения канонического хэша для общего DPO manifest identity.
/// </summary>
internal sealed class DesignConversionPaidReferenceMaterial
{
    private const int MaximumFieldUtf8Bytes = 262_144;

    private DesignConversionPaidReferenceMaterial(
        string html,
        string css)
    {
        Html = html;
        Css = css;
    }

    internal string Html { get; }
    internal string Css { get; }

    internal const string InvalidErrorCode =
        "DESIGN_CONVERSION_PAID_REFERENCE_MATERIAL_INVALID";

    internal const string InvalidSafeMessage =
        "DesignConversion paid reference material must contain canonicalizable HTML and CSS.";

    internal static OperationResult<DesignConversionPaidReferenceMaterial>
        Create(string html, string css)
    {
        if (!IsValidField(html) || !IsValidField(css))
        {
            return OperationResult<DesignConversionPaidReferenceMaterial>
                .Fail(InvalidSafeMessage, InvalidErrorCode);
        }

        return OperationResult<DesignConversionPaidReferenceMaterial>
            .Ok(new DesignConversionPaidReferenceMaterial(
                html,
                css));
    }

    internal string ComputeCanonicalSha256()
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("schema", "design-conversion-paid-reference-v1");
            writer.WriteString("html", Html);
            writer.WriteString("css", Css);
            writer.WriteEndObject();
            writer.Flush();
        }

        return ComputeSha256(buffer.ToArray());
    }

    private static bool IsValidField(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Any(static character =>
                char.IsControl(character) &&
                character is not ('\r' or '\n' or '\t')))
        {
            return false;
        }

        return Encoding.UTF8.GetByteCount(value) <= MaximumFieldUtf8Bytes;
    }

    private static string ComputeSha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

/// <summary>
/// Внутренний набор входов для построения DesignConversion DPO transaction
/// identity. Внешний caller не получает этот контракт напрямую; доверенный root
/// должен прийти из Program/ModelServer boundary, а не из клиентского JSON.
/// </summary>
internal sealed record DesignConversionRepositoryOwnedDpoTransactionContextFactoryConfiguration(
    string TrustedRepositoryRoot,
    string AssertedRepositoryRoot,
    string DomainSlug,
    string DesignPatternSlug,
    string DatasetVersion,
    RepositoryOwnedDpoOpenTeacherIdentity OpenTeacher,
    RepositoryOwnedDpoGenerationPolicyIdentity GenerationPolicy,
    RepositoryOwnedDpoCreatorIdentity Creator,
    IReadOnlyList<DesignConversionPaidReferenceMaterial>
        PaidReferenceMaterials);

/// <summary>
/// Внутренняя factory для DesignConversion repository-owned DPO identity. Она
/// канонизирует только identity/context: не вызывает teacher, не пишет пары или
/// manifest и не создаёт model directory.
/// </summary>
internal static class DesignConversionRepositoryOwnedDpoTransactionContextFactory
{
    internal const string FixedDomainSlug = "figma-html-css";
    internal const string FixedDesignPatternSlug = "design-to-code";

    internal const string InvalidErrorCode =
        "DESIGN_CONVERSION_REPOSITORY_OWNED_DPO_TRANSACTION_CONTEXT_FACTORY_INVALID";

    internal const string InvalidSafeMessage =
        "DesignConversion repository-owned DPO transaction context factory input is invalid.";

    internal static OperationResult<DesignConversionRepositoryOwnedDpoTransactionContext>
        Create(
            DesignConversionRepositoryOwnedDpoTransactionContextFactoryConfiguration
                configuration)
    {
        if (configuration is null ||
            !UsesTrustedRepositoryRoot(configuration) ||
            !UsesFixedDesignConversionAddress(configuration) ||
            !IsAllowedLocalQwenOllamaTeacher(configuration.OpenTeacher))
        {
            return Invalid();
        }

        try
        {
            var address = RepositoryOwnedTrainingArtifactLayout
                .CreateDpoDatasetAddress(
                    configuration.TrustedRepositoryRoot,
                    FixedDomainSlug,
                    FixedDesignPatternSlug,
                    configuration.DatasetVersion);
            if (!address.IsSuccess || address.Value is null)
                return Invalid();

            var paidReferenceHashes = ComputePaidReferenceHashes(
                configuration.PaidReferenceMaterials);
            if (paidReferenceHashes is null)
                return Invalid();

            var identity = RepositoryOwnedDpoDatasetManifestSealer
                .CreateRequestIdentity(
                    paidReferenceHashes,
                    configuration.OpenTeacher,
                    configuration.GenerationPolicy,
                    configuration.Creator);
            if (!identity.IsSuccess || identity.Value is null)
                return Invalid();

            // Серверный Program boundary уже выбрал trusted root. Factory
            // дополнительно требует, чтобы любой asserted root совпал с ним,
            // и затем строит address только из trusted root. Так downstream не
            // сможет перенести DPO или будущую модель в caller-supplied путь.
            var context =
                new DesignConversionRepositoryOwnedDpoTransactionContext(
                    Enabled: true,
                    address.Value,
                    identity.Value);
            var validation = context.Validate();
            return validation.IsSuccess
                ? OperationResult<DesignConversionRepositoryOwnedDpoTransactionContext>
                    .Ok(context)
                : Invalid();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid();
        }
    }

    private static bool UsesTrustedRepositoryRoot(
        DesignConversionRepositoryOwnedDpoTransactionContextFactoryConfiguration
            configuration)
    {
        var trusted = NormalizeRepositoryRoot(configuration.TrustedRepositoryRoot);
        var asserted = NormalizeRepositoryRoot(configuration.AssertedRepositoryRoot);
        return trusted is not null &&
               asserted is not null &&
               StringComparer.OrdinalIgnoreCase.Equals(trusted, asserted);
    }

    private static string? NormalizeRepositoryRoot(string? repositoryRoot)
    {
        if (string.IsNullOrWhiteSpace(repositoryRoot))
            return null;

        try
        {
            if (!Path.IsPathFullyQualified(repositoryRoot))
                return null;

            return Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(repositoryRoot));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return null;
        }
    }

    private static bool UsesFixedDesignConversionAddress(
        DesignConversionRepositoryOwnedDpoTransactionContextFactoryConfiguration
            configuration)
        => StringComparer.Ordinal.Equals(
               configuration.DomainSlug,
               FixedDomainSlug) &&
           StringComparer.Ordinal.Equals(
               configuration.DesignPatternSlug,
               FixedDesignPatternSlug);

    private static bool IsAllowedLocalQwenOllamaTeacher(
        RepositoryOwnedDpoOpenTeacherIdentity? teacher)
        => teacher is not null &&
           StringComparer.Ordinal.Equals(
               teacher.Family,
               OpenWeightTeacherFamily.Qwen.ToString()) &&
           ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaModelId(
               teacher.ModelId) &&
           ContinuousLearningTp1DpoConfiguration.IsCanonicalOllamaManifestDigest(
               teacher.ModelVersion) &&
           LegalTrainingTeacherAdmissionAuthority
               .IsCanonicalLocalOllamaArtifactIdentity(
                   teacher.ModelVersion,
                   teacher.ModelArtifactDigestKind,
                   teacher.ModelArtifactSha256) &&
           !ForbiddenModelIdentityRegistry.ContainsForbiddenIdentity(
               teacher.ModelId) &&
           !ForbiddenModelIdentityRegistry.ContainsForbiddenIdentity(
               teacher.ModelVersion);

    private static IReadOnlyList<string>? ComputePaidReferenceHashes(
        IReadOnlyList<DesignConversionPaidReferenceMaterial>?
            paidReferenceMaterials)
    {
        if (paidReferenceMaterials is not { Count: > 0 })
            return null;

        var hashes = new List<string>(paidReferenceMaterials.Count);
        foreach (var material in paidReferenceMaterials)
        {
            if (material is null)
                return null;

            hashes.Add(material.ComputeCanonicalSha256());
        }

        return Array.AsReadOnly(hashes.ToArray());
    }

    private static OperationResult<DesignConversionRepositoryOwnedDpoTransactionContext>
        Invalid()
        => OperationResult<DesignConversionRepositoryOwnedDpoTransactionContext>
            .Fail(InvalidSafeMessage, InvalidErrorCode);
}
