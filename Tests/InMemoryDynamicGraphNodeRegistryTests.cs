// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class InMemoryDynamicGraphNodeRegistryTests
{
    [Fact]
    public void PublicSurface_IsTypedAndContainsOnlyRegisterAndResolve()
    {
        var type = typeof(InMemoryDynamicGraphNodeRegistry<string, string>);
        var methods = type.GetMethods()
            .Where(method => method.DeclaringType == type && method.IsPublic && !method.IsSpecialName)
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["Register", "Resolve"], methods.Select(method => method.Name));
        Assert.Equal(
            typeof(OperationResult<DynamicGraphNodeDescriptor>),
            methods.Single(method => method.Name == "Register").ReturnType);
        Assert.Equal(
            typeof(IDynamicGraphNode<string, string>),
            methods.Single(method => method.Name == "Register").GetParameters().Single().ParameterType);
        Assert.Equal(
            typeof(OperationResult<IDynamicGraphNode<string, string>>),
            methods.Single(method => method.Name == "Resolve").ReturnType);
        Assert.Equal(
            typeof(string),
            methods.Single(method => method.Name == "Resolve").GetParameters().Single().ParameterType);
        Assert.DoesNotContain(type.GetFields(), field => field.FieldType == typeof(object));
    }

    [Fact]
    public void RegisterAndResolve_TypedHappyPathUsesOrdinalIdentityAndSeparateDescriptorSnapshots()
    {
        var registry = new InMemoryDynamicGraphNodeRegistry<string, string>();
        var descriptor = CreateDescriptor();
        var node = new MutableNode(descriptor.NodeId, descriptor);

        var registration = registry.Register(node);
        var resolution = registry.Resolve("node-a");
        var ordinalMiss = registry.Resolve("NODE-A");

        Assert.True(registration.IsSuccess);
        Assert.Equal(descriptor, registration.Value);
        Assert.NotSame(descriptor, registration.Value);
        Assert.True(resolution.IsSuccess);
        Assert.Same(node, resolution.Value);
        AssertFailure(ordinalMiss, "DYNAMIC_GRAPH_NODE_REGISTRY_NODE_NOT_FOUND");
    }

    [Fact]
    public void Register_RejectsNullIdentityDescriptorAndGenericTypeMismatches()
    {
        AssertRegistrationFailure(
            null!,
            "DYNAMIC_GRAPH_NODE_REGISTRY_NODE_REQUIRED");
        AssertRegistrationFailure(
            new MutableNode(" ", CreateDescriptor() with { NodeId = " " }),
            "DYNAMIC_GRAPH_NODE_REGISTRY_NODE_ID_REQUIRED");
        AssertRegistrationFailure(
            new MutableNode("node-a", null!),
            "DYNAMIC_GRAPH_NODE_REGISTRY_NODE_REQUIRED");
        AssertRegistrationFailure(
            new MutableNode("node-a", CreateDescriptor() with { NodeId = "node-b" }),
            "DYNAMIC_GRAPH_NODE_REGISTRY_IDENTITY_MISMATCH");
        AssertRegistrationFailure(
            new MutableNode("node-a", CreateDescriptor() with { TInputTypeName = nameof(Int32) }),
            "DYNAMIC_GRAPH_NODE_REGISTRY_INPUT_TYPE_MISMATCH");
        AssertRegistrationFailure(
            new MutableNode("node-a", CreateDescriptor() with { TOutputTypeName = nameof(Int32) }),
            "DYNAMIC_GRAPH_NODE_REGISTRY_OUTPUT_TYPE_MISMATCH");
    }

    [Fact]
    public void Register_PropagatesExistingTopologyValidatorFailureCodeAndMessage()
    {
        var registry = new InMemoryDynamicGraphNodeRegistry<string, string>();
        var node = new MutableNode(
            "node-a",
            CreateDescriptor() with { AuthorityRole = "unsupported-role" });

        var result = registry.Register(node);

        AssertFailure(result, "DYNAMIC_GRAPH_NODE_AUTHORITY_ROLE_INVALID");
        Assert.Equal("Dynamic graph node 'node-a' authority role is invalid.", result.ErrorMessage);
    }

    [Fact]
    public void Register_DuplicateExactIdFailsAndRetainsOriginalNode()
    {
        var registry = new InMemoryDynamicGraphNodeRegistry<string, string>();
        var original = new MutableNode("node-a", CreateDescriptor());
        var duplicate = new MutableNode("node-a", CreateDescriptor());

        Assert.True(registry.Register(original).IsSuccess);
        var duplicateResult = registry.Register(duplicate);
        var resolution = registry.Resolve("node-a");

        AssertFailure(duplicateResult, "DYNAMIC_GRAPH_NODE_REGISTRY_DUPLICATE_NODE_ID");
        Assert.True(resolution.IsSuccess);
        Assert.Same(original, resolution.Value);
    }

    [Fact]
    public void Resolve_RejectsBlankMissingAndDescriptorOrIdentityDrift()
    {
        var registry = new InMemoryDynamicGraphNodeRegistry<string, string>();
        var node = new MutableNode("node-a", CreateDescriptor());
        Assert.True(registry.Register(node).IsSuccess);

        AssertFailure(
            registry.Resolve(" "),
            "DYNAMIC_GRAPH_NODE_REGISTRY_NODE_ID_REQUIRED");
        AssertFailure(
            registry.Resolve("node-missing"),
            "DYNAMIC_GRAPH_NODE_REGISTRY_NODE_NOT_FOUND");

        node.Descriptor = node.Descriptor with { DomainTag = "drifted-domain" };
        AssertFailure(
            registry.Resolve("node-a"),
            "DYNAMIC_GRAPH_NODE_REGISTRY_DESCRIPTOR_DRIFT");

        var identityRegistry = new InMemoryDynamicGraphNodeRegistry<string, string>();
        var identityNode = new MutableNode("node-a", CreateDescriptor());
        Assert.True(identityRegistry.Register(identityNode).IsSuccess);
        identityNode.NodeId = "node-b";
        AssertFailure(
            identityRegistry.Resolve("node-a"),
            "DYNAMIC_GRAPH_NODE_REGISTRY_DESCRIPTOR_DRIFT");
    }

    private static void AssertRegistrationFailure(
        IDynamicGraphNode<string, string> node,
        string expectedCode)
    {
        var registry = new InMemoryDynamicGraphNodeRegistry<string, string>();
        AssertFailure(registry.Register(node), expectedCode);
    }

    private static DynamicGraphNodeDescriptor CreateDescriptor()
        => new(
            NodeId: "node-a",
            DomainTag: "domain-a",
            TInputTypeName: nameof(String),
            TOutputTypeName: nameof(String),
            IsTrained: false,
            DeterministicFinalAuthority: false,
            RuntimeSemanticQualityClaimed: false,
            AuthorityRole: "baseline_candidate_generator",
            CheckpointRef: null,
            ManifestRef: null);

    private static void AssertFailure<T>(OperationResult<T> result, string expectedCode)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    private sealed class MutableNode : IDynamicGraphNode<string, string>
    {
        public MutableNode(string nodeId, DynamicGraphNodeDescriptor descriptor)
        {
            NodeId = nodeId;
            Descriptor = descriptor;
        }

        public string NodeId { get; set; }

        public DynamicGraphNodeDescriptor Descriptor { get; set; }

        public Task<OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>> ExecuteAsync(
            LogicalUnitRequest<string> request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(OperationResult<LogicalUnitResult<IQueryable<GraphNodeCandidate>>>.Ok(
                new LogicalUnitResult<IQueryable<GraphNodeCandidate>>(
                    request.UnitId,
                    request.UnitRole,
                    request.DomainTag,
                    Array.Empty<GraphNodeCandidate>().AsQueryable(),
                    0.0,
                    [],
                    [],
                    [],
                    [])));
    }
}
