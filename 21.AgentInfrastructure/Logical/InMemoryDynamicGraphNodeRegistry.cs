// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// This typed registry is the executable-node lookup boundary for the bounded graph candidate runner.
public sealed class InMemoryDynamicGraphNodeRegistry<TInput, TOutput>
{
    private const string NodeRequiredCode = "DYNAMIC_GRAPH_NODE_REGISTRY_NODE_REQUIRED";
    private const string NodeIdRequiredCode = "DYNAMIC_GRAPH_NODE_REGISTRY_NODE_ID_REQUIRED";
    private const string IdentityMismatchCode = "DYNAMIC_GRAPH_NODE_REGISTRY_IDENTITY_MISMATCH";
    private const string InputTypeMismatchCode = "DYNAMIC_GRAPH_NODE_REGISTRY_INPUT_TYPE_MISMATCH";
    private const string OutputTypeMismatchCode = "DYNAMIC_GRAPH_NODE_REGISTRY_OUTPUT_TYPE_MISMATCH";
    private const string DuplicateNodeIdCode = "DYNAMIC_GRAPH_NODE_REGISTRY_DUPLICATE_NODE_ID";
    private const string NodeNotFoundCode = "DYNAMIC_GRAPH_NODE_REGISTRY_NODE_NOT_FOUND";
    private const string DescriptorDriftCode = "DYNAMIC_GRAPH_NODE_REGISTRY_DESCRIPTOR_DRIFT";
    private const string RegistrationExceptionCode = "DYNAMIC_GRAPH_NODE_REGISTRY_REGISTRATION_EXCEPTION";
    private const string ValidationTenant = "__dynamic_graph_node_registry_validation__";
    private const string ValidationDomain = "__dynamic_graph_node_registry_domain__";
    private const string ValidationContext = "__dynamic_graph_node_registry_validation__";

    private readonly Dictionary<string, RegistryEntry> _entries = new(StringComparer.Ordinal);

    // Register validates one typed node, retains its executable identity, and snapshots its descriptor.
    public OperationResult<DynamicGraphNodeDescriptor> Register(IDynamicGraphNode<TInput, TOutput> node)
    {
        if (node is null)
            return OperationResult<DynamicGraphNodeDescriptor>.Fail(
                "Dynamic graph registry node is required.",
                NodeRequiredCode);

        try
        {
            var nodeId = node.NodeId;
            if (!IsCanonicalSingleLineValue(nodeId))
                return OperationResult<DynamicGraphNodeDescriptor>.Fail(
                    "Dynamic graph registry node id is required.",
                    NodeIdRequiredCode);

            var descriptor = node.Descriptor;
            if (descriptor is null)
                return OperationResult<DynamicGraphNodeDescriptor>.Fail(
                    "Dynamic graph registry node descriptor is required.",
                    NodeRequiredCode);

            if (!StringComparer.Ordinal.Equals(nodeId, descriptor.NodeId))
                return OperationResult<DynamicGraphNodeDescriptor>.Fail(
                    "Dynamic graph registry node identity does not match its descriptor.",
                    IdentityMismatchCode);

            if (!StringComparer.Ordinal.Equals(descriptor.TInputTypeName, typeof(TInput).Name))
                return OperationResult<DynamicGraphNodeDescriptor>.Fail(
                    "Dynamic graph registry node input type does not match the registry boundary.",
                    InputTypeMismatchCode);

            if (!StringComparer.Ordinal.Equals(descriptor.TOutputTypeName, typeof(TOutput).Name))
                return OperationResult<DynamicGraphNodeDescriptor>.Fail(
                    "Dynamic graph registry node output type does not match the registry boundary.",
                    OutputTypeMismatchCode);

            var topologyDomain = string.IsNullOrWhiteSpace(descriptor.DomainTag)
                || StringComparer.Ordinal.Equals(descriptor.DomainTag, DynamicGraphValidation.DomainAgnosticTag)
                    ? ValidationDomain
                    : descriptor.DomainTag;
            var topology = new DynamicGraphTopology(
                ValidationTenant,
                topologyDomain,
                ValidationContext,
                [node],
                [],
                [new DynamicGraphNodeOrder(
                    $"{nodeId}:registry-validation-order",
                    nodeId,
                    ValidationTenant,
                    topologyDomain,
                    ValidationContext,
                    0)]);
            var topologyValidation = DynamicGraphValidation.Validate(topology);
            if (!topologyValidation.IsSuccess)
                return OperationResult<DynamicGraphNodeDescriptor>.Fail(
                    topologyValidation.ErrorMessage,
                    topologyValidation.ErrorCode);

            if (_entries.ContainsKey(nodeId))
                return OperationResult<DynamicGraphNodeDescriptor>.Fail(
                    "Dynamic graph registry node id is already registered.",
                    DuplicateNodeIdCode);

            var storedDescriptor = descriptor with { };
            _entries.Add(nodeId, new RegistryEntry(node, storedDescriptor));

            return OperationResult<DynamicGraphNodeDescriptor>.Ok(storedDescriptor with { });
        }
        catch (Exception exception) when (IsNonFatalRegistryException(exception))
        {
            return OperationResult<DynamicGraphNodeDescriptor>.Fail(
                "Dynamic graph registry node registration failed unexpectedly.",
                RegistrationExceptionCode);
        }
    }

    // Resolve uses ordinal identity and rejects any executable-node or descriptor drift since registration.
    public OperationResult<IDynamicGraphNode<TInput, TOutput>> Resolve(string nodeId)
    {
        if (!IsCanonicalSingleLineValue(nodeId))
            return OperationResult<IDynamicGraphNode<TInput, TOutput>>.Fail(
                "Dynamic graph registry node id is required.",
                NodeIdRequiredCode);

        if (!_entries.TryGetValue(nodeId, out var entry))
            return OperationResult<IDynamicGraphNode<TInput, TOutput>>.Fail(
                "Dynamic graph registry node was not found.",
                NodeNotFoundCode);

        try
        {
            var currentNodeId = entry.Node.NodeId;
            var currentDescriptor = entry.Node.Descriptor;
            if (!StringComparer.Ordinal.Equals(currentNodeId, nodeId)
                || currentDescriptor is null
                || !Equals(currentDescriptor, entry.DescriptorSnapshot))
            {
                return OperationResult<IDynamicGraphNode<TInput, TOutput>>.Fail(
                    "Dynamic graph registry node descriptor drifted after registration.",
                    DescriptorDriftCode);
            }
        }
        catch (Exception exception) when (IsNonFatalRegistryException(exception))
        {
            return OperationResult<IDynamicGraphNode<TInput, TOutput>>.Fail(
                "Dynamic graph registry node descriptor drifted after registration.",
                DescriptorDriftCode);
        }

        return OperationResult<IDynamicGraphNode<TInput, TOutput>>.Ok(entry.Node);
    }

    private static bool IsCanonicalSingleLineValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && string.Equals(value, value.Trim(), StringComparison.Ordinal)
           && value.All(character =>
               !char.IsControl(character)
               && character is not '\r'
               && character is not '\n'
               && character is not '\u0085'
               && character is not '\u2028'
               && character is not '\u2029');

    private static bool IsNonFatalRegistryException(Exception exception)
        => exception is not OutOfMemoryException
           && exception is not StackOverflowException
           && exception is not AccessViolationException
           && exception is not AppDomainUnloadedException
           && exception is not BadImageFormatException
           && !StringComparer.Ordinal.Equals(
               exception.GetType().FullName,
               "System.Threading.ThreadAbortException");

    private sealed record RegistryEntry(
        IDynamicGraphNode<TInput, TOutput> Node,
        DynamicGraphNodeDescriptor DescriptorSnapshot);
}
