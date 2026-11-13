// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel.Logical;

using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

internal delegate OperationResult<DynamicGraphLogicalOrchestratorCompositionResult>
    DynamicGraphLogicalOrchestratorProfileCatalogComposer(
        ISessionStorage? storage,
        DynamicGraphLogicalOrchestratorBootstrapProfile profile);

internal delegate OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile>
    DynamicGraphLogicalOrchestratorProfileCatalogSelector(
        ReadOnlyMemory<byte> utf8Catalog,
        string? selectedProfileId);

// Selects one exact bootstrap profile from a bounded caller-supplied UTF-8 catalog.
public static class DynamicGraphLogicalOrchestratorProfileCatalog
{
    private const int MaximumCatalogBytes = 1_048_576;
    private const int MaximumContainerDepth = 64;
    private const int MaximumRootItemCount = 64;

    private const string CatalogRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_CATALOG_REQUIRED";
    private const string SelectionIdRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_SELECTION_ID_REQUIRED";
    private const string SelectionIdInvalidCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_SELECTION_ID_INVALID";
    private const string CatalogLimitExceededCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_CATALOG_LIMIT_EXCEEDED";
    private const string CatalogInvalidCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_CATALOG_INVALID";
    private const string DuplicateProfileIdCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_CATALOG_DUPLICATE_PROFILE_ID";
    private const string SelectionNotFoundCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_SELECTION_NOT_FOUND";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        AllowTrailingCommas = false,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = MaximumContainerDepth
    };

    public static OperationResult<DynamicGraphLogicalOrchestratorCompositionResult> ComposeSelected(
        ISessionStorage? storage,
        ReadOnlyMemory<byte> utf8Catalog,
        string? selectedProfileId)
        => ComposeSelectedCore(
            storage,
            utf8Catalog,
            selectedProfileId,
            static (selectedStorage, profile) =>
                DynamicGraphLogicalOrchestratorComposition.Create(selectedStorage!, profile));

    internal static OperationResult<DynamicGraphLogicalOrchestratorCompositionResult> ComposeSelectedCore(
        ISessionStorage? storage,
        ReadOnlyMemory<byte> utf8Catalog,
        string? selectedProfileId,
        DynamicGraphLogicalOrchestratorProfileCatalogComposer composer)
        => ComposeSelectedWithSelectorCore(
            storage,
            utf8Catalog,
            selectedProfileId,
            Select,
            composer);

    internal static OperationResult<DynamicGraphLogicalOrchestratorCompositionResult>
        ComposeSelectedWithSelectorCore(
            ISessionStorage? storage,
            ReadOnlyMemory<byte> utf8Catalog,
            string? selectedProfileId,
            DynamicGraphLogicalOrchestratorProfileCatalogSelector selector,
            DynamicGraphLogicalOrchestratorProfileCatalogComposer composer)
    {
        var selection = selector(utf8Catalog, selectedProfileId);
        if (!selection.IsSuccess)
            return selection.MapError<DynamicGraphLogicalOrchestratorCompositionResult>();

        return composer(storage, selection.Value!);
    }

    public static OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile> Select(
        ReadOnlyMemory<byte> utf8Catalog,
        string? selectedProfileId)
    {
        if (utf8Catalog.IsEmpty)
            return CatalogRequired();

        if (string.IsNullOrWhiteSpace(selectedProfileId))
            return SelectionIdRequired();

        if (!IsCanonicalIdentifier(selectedProfileId))
            return SelectionIdInvalid();

        if (utf8Catalog.Length > MaximumCatalogBytes)
            return CatalogLimitExceeded();

        var catalogSnapshot = utf8Catalog.ToArray();
        if (HasUtf8Bom(catalogSnapshot))
            return CatalogInvalid();

        DynamicGraphLogicalOrchestratorBootstrapProfile? selectedProfile;
        try
        {
            var preflight = Preflight(catalogSnapshot);
            if (!preflight.IsSuccess)
                return preflight.MapError<DynamicGraphLogicalOrchestratorBootstrapProfile>();

            using var document = JsonDocument.Parse(
                catalogSnapshot,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = MaximumContainerDepth
                });
            if (document.RootElement.ValueKind != JsonValueKind.Array
                || ContainsDuplicateProperty(document.RootElement))
            {
                return CatalogInvalid();
            }

            var profiles = JsonSerializer.Deserialize<DynamicGraphLogicalOrchestratorBootstrapProfile[]>(
                catalogSnapshot,
                SerializerOptions);
            if (profiles is null || profiles.Any(profile => profile is null))
                return CatalogInvalid();

            foreach (var profile in profiles)
            {
                if (!IsCanonicalIdentifier(profile.ProfileId))
                    return CatalogInvalid();
            }

            var profileIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var profile in profiles)
            {
                if (!profileIds.Add(profile.ProfileId))
                    return DuplicateProfileId();
            }

            selectedProfile = profiles.FirstOrDefault(profile =>
                StringComparer.Ordinal.Equals(profile.ProfileId, selectedProfileId));
            if (selectedProfile is null)
                return SelectionNotFound();
        }
        catch (JsonException)
        {
            return CatalogInvalid();
        }
        catch (NotSupportedException)
        {
            return CatalogInvalid();
        }

        return OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile>.Ok(selectedProfile!);
    }

    private static OperationResult<bool> Preflight(ReadOnlySpan<byte> catalogSnapshot)
    {
        var reader = new Utf8JsonReader(
            catalogSnapshot,
            new JsonReaderOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = int.MaxValue
            });
        var openContainerDepth = 0;
        var rootItemCount = 0;
        var rootIsArray = false;
        var tokenCount = 0;

        while (reader.Read())
        {
            tokenCount++;
            var tokenType = reader.TokenType;
            var startsContainer = tokenType is JsonTokenType.StartArray or JsonTokenType.StartObject;
            var isValue = startsContainer
                || tokenType is JsonTokenType.String
                    or JsonTokenType.Number
                    or JsonTokenType.True
                    or JsonTokenType.False
                    or JsonTokenType.Null;

            if (rootIsArray && openContainerDepth == 1 && isValue)
            {
                rootItemCount++;
                if (rootItemCount > MaximumRootItemCount)
                    return CatalogLimitExceededBoolean();
            }

            if (startsContainer)
            {
                if (tokenCount == 1)
                    rootIsArray = tokenType == JsonTokenType.StartArray;

                openContainerDepth++;
                if (openContainerDepth > MaximumContainerDepth)
                    return CatalogLimitExceededBoolean();
            }
            else if (tokenType is JsonTokenType.EndArray or JsonTokenType.EndObject)
            {
                openContainerDepth--;
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool ContainsDuplicateProperty(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name) || ContainsDuplicateProperty(property.Value))
                    return true;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (ContainsDuplicateProperty(item))
                    return true;
            }
        }

        return false;
    }

    private static bool IsCanonicalIdentifier(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !StringComparer.Ordinal.Equals(value, value.Trim()))
        {
            return false;
        }

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsControl(character) || character is '\u0085' or '\u2028' or '\u2029')
                return false;

            if (char.IsHighSurrogate(character))
            {
                if (index + 1 >= value.Length || !char.IsLowSurrogate(value[index + 1]))
                    return false;

                index++;
            }
            else if (char.IsLowSurrogate(character))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasUtf8Bom(ReadOnlySpan<byte> value)
        => value.Length >= 3
            && value[0] == 0xEF
            && value[1] == 0xBB
            && value[2] == 0xBF;

    private static OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile> CatalogRequired()
        => Failure(
            "A dynamic graph logical orchestrator profile catalog is required.",
            CatalogRequiredCode);

    private static OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile> SelectionIdRequired()
        => Failure(
            "A dynamic graph logical orchestrator selected profile id is required.",
            SelectionIdRequiredCode);

    private static OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile> SelectionIdInvalid()
        => Failure(
            "The dynamic graph logical orchestrator selected profile id is invalid.",
            SelectionIdInvalidCode);

    private static OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile> CatalogLimitExceeded()
        => Failure(
            "The dynamic graph logical orchestrator profile catalog limit was exceeded.",
            CatalogLimitExceededCode);

    private static OperationResult<bool> CatalogLimitExceededBoolean()
        => OperationResult<bool>.Fail(
            "The dynamic graph logical orchestrator profile catalog limit was exceeded.",
            CatalogLimitExceededCode);

    private static OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile> CatalogInvalid()
        => Failure(
            "The dynamic graph logical orchestrator profile catalog is invalid.",
            CatalogInvalidCode);

    private static OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile> DuplicateProfileId()
        => Failure(
            "The dynamic graph logical orchestrator profile catalog contains a duplicate profile id.",
            DuplicateProfileIdCode);

    private static OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile> SelectionNotFound()
        => Failure(
            "The selected dynamic graph logical orchestrator profile was not found.",
            SelectionNotFoundCode);

    private static OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile> Failure(
        string message,
        string code)
        => OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile>.Fail(message, code);
}
