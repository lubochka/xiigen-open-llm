// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text.Json;
using AgentInfrastructure.Logical;
using OrchestratorModel.Logical;

namespace LlmFromScratch.DynamicGraphCompositionLiveLocalServer;

internal static class DynamicGraphCompositionProfilePolicy
{
    internal static DynamicGraphCompositionProfileAdmission Admit(
        DynamicGraphLogicalOrchestratorBootstrapProfile? profile,
        string configuredTenantId)
    {
        if (profile is null)
            return Reject(400, "HOST_PROFILE_REQUIRED", "A composition profile is required.");
        if (profile.NodeDefinitions is null || profile.EdgeBindings is null || profile.Budget is null)
            return Reject(400, "HOST_PROFILE_SHAPE_INVALID", "The composition profile is incomplete.");

        DynamicGraphLogicalOrchestratorBootstrapProfile ownedProfile;
        try
        {
            ownedProfile = DynamicGraphCompositionSessionPersistence.SnapshotProfile(profile);
        }
        catch (Exception exception) when (
            (exception is JsonException
                or NotSupportedException
                or DynamicGraphCompositionPersistenceException)
            && DynamicGraphCompositionSessionPersistence.IsNonFatalException(exception))
        {
            return Reject(400, "HOST_PROFILE_SHAPE_INVALID", "The composition profile is incomplete.");
        }

        return ValidateOwnedSnapshot(ownedProfile, configuredTenantId);
    }

    internal static DynamicGraphCompositionProfileAdmission ValidateOwnedSnapshot(
        DynamicGraphLogicalOrchestratorBootstrapProfile profile,
        string configuredTenantId)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (!DynamicGraphCompositionServerOptions.IsSafeIdentifier(
                profile.TenantId, 1, DynamicGraphCompositionServerOptions.MaxTenantIdLength))
        {
            return Reject(400, "HOST_PROFILE_TENANT_INVALID", "The profile tenant id is invalid.");
        }
        if (!string.Equals(profile.TenantId, configuredTenantId, StringComparison.Ordinal))
        {
            return Reject(
                403,
                "HOST_PROFILE_TENANT_MISMATCH",
                "The profile does not match the configured tenant binding.");
        }
        if (profile.NodeDefinitions is null || profile.EdgeBindings is null || profile.Budget is null)
            return Reject(400, "HOST_PROFILE_SHAPE_INVALID", "The composition profile is incomplete.");
        if (GraphLimitsExceeded(profile))
        {
            return Reject(
                422,
                "HOST_GRAPH_LIMIT_EXCEEDED",
                "The profile exceeds the bounded local graph limits.");
        }
        if (ContainsForbiddenAuthority(profile))
        {
            return Reject(
                422,
                "HOST_TRAINED_NODE_INPUT_FORBIDDEN",
                "Trained node input is forbidden on this deterministic composition boundary.");
        }

        byte[] canonical;
        try
        {
            canonical = Canonicalize(profile);
        }
        catch (Exception exception) when (
            (exception is JsonException or NotSupportedException)
            && DynamicGraphCompositionSessionPersistence.IsNonFatalException(exception))
        {
            return Reject(400, "HOST_PROFILE_SHAPE_INVALID", "The composition profile is incomplete.");
        }

        if (canonical.Length > DynamicGraphCompositionServerOptions.MaxInputLength)
        {
            CryptographicOperations.ZeroMemory(canonical);
            return Reject(
                413,
                "HOST_REQUEST_BODY_LIMIT_EXCEEDED",
                "The request body exceeds the local limit.");
        }

        var digestBytes = SHA256.HashData(canonical);
        try
        {
            return new DynamicGraphCompositionProfileAdmission(
                true,
                200,
                string.Empty,
                string.Empty,
                profile,
                Convert.ToHexString(digestBytes));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(digestBytes);
            CryptographicOperations.ZeroMemory(canonical);
        }
    }

    internal static byte[] Canonicalize<T>(T value)
    {
        var element = JsonSerializer.SerializeToElement(value, DynamicGraphCompositionJson.CheckpointOptions);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
            WriteCanonical(writer, element);
        return stream.ToArray();
    }

    private static bool GraphLimitsExceeded(DynamicGraphLogicalOrchestratorBootstrapProfile profile)
        => profile.NodeDefinitions.Length > DynamicGraphCompositionServerOptions.MaxGraphNodes
           || profile.EdgeBindings.Length > DynamicGraphCompositionServerOptions.MaxGraphEdges
           || profile.Budget.MaxNodes > DynamicGraphCompositionServerOptions.MaxGraphNodes
           || profile.Budget.MaxEdges > DynamicGraphCompositionServerOptions.MaxGraphEdges
           || profile.Budget.MaxDepth > DynamicGraphCompositionServerOptions.MaxGraphDepth
           || profile.Budget.MaxParallelBranches > DynamicGraphCompositionServerOptions.MaxParallelBranches;

    private static bool ContainsForbiddenAuthority(
        DynamicGraphLogicalOrchestratorBootstrapProfile profile)
        => profile.NodeDefinitions.Any(definition => definition?.Descriptor is null
            || definition.Descriptor.IsTrained
            || definition.Descriptor.DeterministicFinalAuthority
            || definition.Descriptor.RuntimeSemanticQualityClaimed
            || definition.Descriptor.CheckpointRef is not null
            || definition.Descriptor.ManifestRef is not null
            || definition.Descriptor.AuthorityRole is null
            || definition.Descriptor.AuthorityRole.Contains("trained", StringComparison.OrdinalIgnoreCase))
           || profile.EdgeBindings.Any(binding => binding is null
               || binding.DeterministicFinalAuthority);

    private static void WriteCanonical(Utf8JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject().OrderBy(
                             property => property.Name,
                             StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteCanonical(writer, property.Value);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                    WriteCanonical(writer, item);
                writer.WriteEndArray();
                break;
            default:
                element.WriteTo(writer);
                break;
        }
    }

    private static DynamicGraphCompositionProfileAdmission Reject(int statusCode, string code, string message)
        => new(false, statusCode, code, message, null, null);
}

internal sealed record DynamicGraphCompositionProfileAdmission(
    bool IsAccepted,
    int StatusCode,
    string Code,
    string Message,
    DynamicGraphLogicalOrchestratorBootstrapProfile? Profile,
    string? CanonicalDigest);
