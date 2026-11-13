// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ExperimentsV2.LogicalFlow;
using OrchestratorModel.Logical;
using Domain = SemanticCompiler29.Domain;
using SemanticCompilerAdapter;

public sealed class DynamicGraphLogicalOrchestratorProfileCatalogTests
{
    private const string RequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_CATALOG_REQUIRED";
    private const string RequiredMessage =
        "A dynamic graph logical orchestrator profile catalog is required.";
    private const string SelectionRequiredCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_SELECTION_ID_REQUIRED";
    private const string SelectionRequiredMessage =
        "A dynamic graph logical orchestrator selected profile id is required.";
    private const string SelectionInvalidCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_SELECTION_ID_INVALID";
    private const string SelectionInvalidMessage =
        "The dynamic graph logical orchestrator selected profile id is invalid.";
    private const string LimitCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_CATALOG_LIMIT_EXCEEDED";
    private const string LimitMessage =
        "The dynamic graph logical orchestrator profile catalog limit was exceeded.";
    private const string InvalidCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_CATALOG_INVALID";
    private const string InvalidMessage =
        "The dynamic graph logical orchestrator profile catalog is invalid.";
    private const string DuplicateCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_CATALOG_DUPLICATE_PROFILE_ID";
    private const string DuplicateMessage =
        "The dynamic graph logical orchestrator profile catalog contains a duplicate profile id.";
    private const string NotFoundCode =
        "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_PROFILE_SELECTION_NOT_FOUND";
    private const string NotFoundMessage =
        "The selected dynamic graph logical orchestrator profile was not found.";

    private const string AuthorityUnitId = "catalog-authority";
    private const string EvaluationId = "catalog-evaluation";
    private const string A = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string B = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string C = "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] LeakageSignals =
        ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"];
    private static readonly string[] IdentityProbeIds =
        ["source-id-control-01", "source-path-control-02", "correlation-id-control-03"];
    private static readonly string[] IdentityControlledSignals =
        ["source_id", "source_path", "correlation_id"];
    private static readonly string[] IdentityControlValues =
    [
        "edge-authority-source-id-control",
        "identity-control://edge-authority-source-path",
        "edge-authority-correlation-id-control"
    ];

    [Fact]
    public void PublicSurface_IsExactStaticTwoMethodsAndInternalSeamsAreNotPublic()
    {
        var type = typeof(DynamicGraphLogicalOrchestratorProfileCatalog);

        Assert.True(type.IsAbstract);
        Assert.True(type.IsSealed);
        Assert.Empty(type.GetConstructors(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        var methods = type.GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        Assert.Equal(2, methods.Length);
        var composeSelected = Assert.Single(methods, method => method.Name == "ComposeSelected");
        Assert.Equal(
            typeof(OperationResult<DynamicGraphLogicalOrchestratorCompositionResult>),
            composeSelected.ReturnType);
        Assert.Equal(
            [typeof(ISessionStorage), typeof(ReadOnlyMemory<byte>), typeof(string)],
            composeSelected.GetParameters().Select(parameter => parameter.ParameterType));
        var select = Assert.Single(methods, method => method.Name == "Select");
        Assert.Equal(
            typeof(OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile>),
            select.ReturnType);
        Assert.Equal(
            [typeof(ReadOnlyMemory<byte>), typeof(string)],
            select.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(type.GetNestedTypes(BindingFlags.Public));

        var core = Assert.Single(type.GetMethods(
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly),
            candidate => candidate.Name == "ComposeSelectedCore");
        Assert.True(core.IsAssembly);
        Assert.Equal(
            typeof(OperationResult<DynamicGraphLogicalOrchestratorCompositionResult>),
            core.ReturnType);
        var composerType = core.GetParameters()[3].ParameterType;
        Assert.Equal(
            [typeof(ISessionStorage), typeof(ReadOnlyMemory<byte>), typeof(string), composerType],
            core.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.True(typeof(Delegate).IsAssignableFrom(composerType));
        Assert.True(composerType.IsNotPublic);
        Assert.Empty(composerType.GetNestedTypes(BindingFlags.Public));
        var composerInvoke = composerType.GetMethod("Invoke");
        Assert.NotNull(composerInvoke);
        Assert.Equal(
            typeof(OperationResult<DynamicGraphLogicalOrchestratorCompositionResult>),
            composerInvoke.ReturnType);
        Assert.Equal(
            [typeof(ISessionStorage), typeof(DynamicGraphLogicalOrchestratorBootstrapProfile)],
            composerInvoke.GetParameters().Select(parameter => parameter.ParameterType));

        var selectorCore = Assert.Single(type.GetMethods(
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly),
            candidate => candidate.Name == "ComposeSelectedWithSelectorCore");
        Assert.True(selectorCore.IsAssembly);
        Assert.Equal(
            typeof(OperationResult<DynamicGraphLogicalOrchestratorCompositionResult>),
            selectorCore.ReturnType);
        var selectorType = selectorCore.GetParameters()[3].ParameterType;
        Assert.Equal(
            [
                typeof(ISessionStorage),
                typeof(ReadOnlyMemory<byte>),
                typeof(string),
                selectorType,
                composerType
            ],
            selectorCore.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.True(typeof(Delegate).IsAssignableFrom(selectorType));
        Assert.True(selectorType.IsNotPublic);
        Assert.Same(composerType, selectorCore.GetParameters()[4].ParameterType);
        var selectorInvoke = selectorType.GetMethod("Invoke");
        Assert.NotNull(selectorInvoke);
        Assert.Equal(
            typeof(OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile>),
            selectorInvoke.ReturnType);
        Assert.Equal(
            [typeof(ReadOnlyMemory<byte>), typeof(string)],
            selectorInvoke.GetParameters().Select(parameter => parameter.ParameterType));
    }

    [Fact]
    public void ComposeSelectedWithSelectorCore_InvokesEachDelegateExactlyOnceAndMapsSelectorFailure()
    {
        var storage = new NullSessionStorage();
        var catalog = Catalog(Profile("first"), Profile("middle"), Profile("last"));
        var selectedProfile = RichProfile("middle");
        var selector = new RecordingSelector(
            OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile>.Ok(selectedProfile));
        var expected = DynamicGraphLogicalOrchestratorComposition.Create(storage, Profile("expected"));
        var recorder = new RecordingComposer(expected);

        var actual = DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelectedWithSelectorCore(
            storage,
            catalog,
            "middle",
            selector.Select,
            recorder.Compose);

        Assert.Same(expected, actual);
        Assert.Equal(1, selector.CallCount);
        Assert.True(catalog.AsSpan().SequenceEqual(selector.Catalog.Span));
        Assert.Equal("middle", selector.SelectedProfileId, StringComparer.Ordinal);
        Assert.Equal(1, recorder.CallCount);
        Assert.Same(storage, recorder.Storage);
        Assert.Same(selectedProfile, recorder.Profile);

        var selectorFailure = OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile>.Fail(
            "selection failure",
            "SELECTION_FAILURE");
        var failingSelector = new RecordingSelector(selectorFailure);
        var skippedComposer = new RecordingComposer(expected);

        var mappedFailure = DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelectedWithSelectorCore(
            storage,
            catalog,
            "middle",
            failingSelector.Select,
            skippedComposer.Compose);

        AssertFailure(mappedFailure, "SELECTION_FAILURE", "selection failure");
        Assert.Equal(1, failingSelector.CallCount);
        Assert.Equal(0, skippedComposer.CallCount);
    }

    [Fact]
    public void ComposeSelectedWithSelectorCore_ReturnsSameDownstreamFailureAndDoesNotCatchDelegateException()
    {
        var expected = OperationResult<DynamicGraphLogicalOrchestratorCompositionResult>.Fail(
            "downstream failure",
            "DOWNSTREAM_FAILURE");
        var recorder = new RecordingComposer(expected);
        var selector = SuccessfulSelector("selected");

        var actual = DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelectedWithSelectorCore(
            new NullSessionStorage(),
            Catalog(Profile("selected")),
            "selected",
            selector.Select,
            recorder.Compose);

        Assert.Same(expected, actual);
        Assert.Equal(1, selector.CallCount);
        Assert.Equal(1, recorder.CallCount);
        var invalidOperationSelector = SuccessfulSelector("selected");
        var exception = Assert.Throws<InvalidOperationException>(() =>
            DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelectedWithSelectorCore(
                new NullSessionStorage(),
                Catalog(Profile("selected")),
                "selected",
                invalidOperationSelector.Select,
                static (_, _) => throw new InvalidOperationException("fatal composition graph")));
        Assert.Equal("fatal composition graph", exception.Message, StringComparer.Ordinal);
        Assert.Equal(1, invalidOperationSelector.CallCount);

        var jsonException = new JsonException("composer JSON exception");
        var jsonComposerCallCount = 0;
        var jsonExceptionSelector = SuccessfulSelector("selected");
        var escapedJsonException = Assert.Throws<JsonException>(() =>
            DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelectedWithSelectorCore(
                new NullSessionStorage(),
                Catalog(Profile("selected")),
                "selected",
                jsonExceptionSelector.Select,
                (_, _) =>
                {
                    jsonComposerCallCount++;
                    throw jsonException;
                }));
        Assert.Same(jsonException, escapedJsonException);
        Assert.Equal(1, jsonExceptionSelector.CallCount);
        Assert.Equal(1, jsonComposerCallCount);

        var notSupportedException = new NotSupportedException("composer unsupported exception");
        var notSupportedComposerCallCount = 0;
        var notSupportedSelector = SuccessfulSelector("selected");
        var escapedNotSupportedException = Assert.Throws<NotSupportedException>(() =>
            DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelectedWithSelectorCore(
                new NullSessionStorage(),
                Catalog(Profile("selected")),
                "selected",
                notSupportedSelector.Select,
                (_, _) =>
                {
                    notSupportedComposerCallCount++;
                    throw notSupportedException;
                }));
        Assert.Same(notSupportedException, escapedNotSupportedException);
        Assert.Equal(1, notSupportedSelector.CallCount);
        Assert.Equal(1, notSupportedComposerCallCount);

        var selectorException = new JsonException("selector JSON exception");
        var skippedComposer = new RecordingComposer(expected);
        var escapedSelectorException = Assert.Throws<JsonException>(() =>
            DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelectedWithSelectorCore(
                new NullSessionStorage(),
                Catalog(Profile("selected")),
                "selected",
                (_, _) => throw selectorException,
                skippedComposer.Compose));
        Assert.Same(selectorException, escapedSelectorException);
        Assert.Equal(0, skippedComposer.CallCount);

        var fourParameterJsonException = new JsonException("four-parameter composer JSON exception");
        var fourParameterJsonComposerCallCount = 0;
        var escapedFourParameterJsonException = Assert.Throws<JsonException>(() =>
            DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelectedCore(
                new NullSessionStorage(),
                Catalog(Profile("selected")),
                "selected",
                (_, _) =>
                {
                    fourParameterJsonComposerCallCount++;
                    throw fourParameterJsonException;
                }));
        Assert.Same(fourParameterJsonException, escapedFourParameterJsonException);
        Assert.Equal(1, fourParameterJsonComposerCallCount);

        var fourParameterNotSupportedException = new NotSupportedException(
            "four-parameter composer unsupported exception");
        var fourParameterNotSupportedComposerCallCount = 0;
        var escapedFourParameterNotSupportedException = Assert.Throws<NotSupportedException>(() =>
            DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelectedCore(
                new NullSessionStorage(),
                Catalog(Profile("selected")),
                "selected",
                (_, _) =>
                {
                    fourParameterNotSupportedComposerCallCount++;
                    throw fourParameterNotSupportedException;
                }));
        Assert.Same(fourParameterNotSupportedException, escapedFourParameterNotSupportedException);
        Assert.Equal(1, fourParameterNotSupportedComposerCallCount);
    }

    [Fact]
    public void ComposeSelectedCore_RequiredAndSelectorPrecedenceNeverCallsComposer()
    {
        AssertCatalogFailure(default, null, RequiredCode, RequiredMessage);
        AssertCatalogFailure(ReadOnlyMemory<byte>.Empty, "selected", RequiredCode, RequiredMessage);

        var oversized = new byte[1_048_577];
        oversized.AsSpan().Fill((byte)' ');
        AssertCatalogFailure(oversized, null, SelectionRequiredCode, SelectionRequiredMessage);
        AssertCatalogFailure(oversized, "", SelectionRequiredCode, SelectionRequiredMessage);
        AssertCatalogFailure(oversized, " \t", SelectionRequiredCode, SelectionRequiredMessage);
        AssertCatalogFailure(oversized, " selected", SelectionInvalidCode, SelectionInvalidMessage);
        AssertCatalogFailure(oversized, "selected\u0001", SelectionInvalidCode, SelectionInvalidMessage);
        AssertCatalogFailure(oversized, "selected\u0085", SelectionInvalidCode, SelectionInvalidMessage);
        AssertCatalogFailure(oversized, "selected\u2028", SelectionInvalidCode, SelectionInvalidMessage);
        AssertCatalogFailure(oversized, "selected\u2029", SelectionInvalidCode, SelectionInvalidMessage);
        AssertCatalogFailure(oversized, "selected\uD800", SelectionInvalidCode, SelectionInvalidMessage);
        AssertCatalogFailure(oversized, "selected\uDC00", SelectionInvalidCode, SelectionInvalidMessage);
        AssertCatalogFailure(oversized, "selected", LimitCode, LimitMessage);
    }

    [Fact]
    public void ComposeSelectedCore_ByteDepthAndRootItemLimitsHaveExactBoundaries()
    {
        var storage = new NullSessionStorage();
        var valid = Catalog(Profile("selected"));
        var exact = new byte[1_048_576];
        valid.CopyTo(exact, 0);
        exact.AsSpan(valid.Length).Fill((byte)' ');
        var recorder = new RecordingComposer(
            DynamicGraphLogicalOrchestratorComposition.Create(storage, Profile("result")));

        var exactResult = DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelectedCore(
            storage,
            exact,
            "selected",
            recorder.Compose);

        Assert.Same(recorder.Result, exactResult);
        Assert.Equal(1, recorder.CallCount);
        AssertCatalogFailure(new byte[1_048_577], "selected", LimitCode, LimitMessage);

        var profiles64 = Enumerable.Range(0, 64).Select(index => Profile($"profile-{index:D2}")).ToArray();
        var accepted64 = new RecordingComposer(recorder.Result);
        var selected64 = DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelectedCore(
            storage,
            Catalog(profiles64),
            "profile-63",
            accepted64.Compose);
        Assert.Same(recorder.Result, selected64);
        Assert.Equal(1, accepted64.CallCount);
        AssertCatalogFailure(
            Catalog(Enumerable.Range(0, 65).Select(index => Profile($"profile-{index:D2}")).ToArray()),
            "profile-64",
            LimitCode,
            LimitMessage);

        AssertCatalogFailure(NestedCatalog(64), "selected", InvalidCode, InvalidMessage);
        AssertCatalogFailure(NestedCatalog(65), "selected", LimitCode, LimitMessage);
        AssertCatalogFailure(
            Encoding.UTF8.GetBytes("[" + new string('[', 64) + "!"),
            "selected",
            LimitCode,
            LimitMessage);
        AssertCatalogFailure(
            Encoding.UTF8.GetBytes(
                "[" + string.Join(",", Enumerable.Repeat("0", 65)) + ",!]"),
            "selected",
            LimitCode,
            LimitMessage);
        AssertCatalogFailure(
            Encoding.UTF8.GetBytes("[!" + new string('[', 64)),
            "selected",
            InvalidCode,
            InvalidMessage);
    }

    [Fact]
    public void ComposeSelectedCore_StrictJsonSchemaAndRecursiveDuplicatesAreInvalid()
    {
        var json = Encoding.UTF8.GetString(Catalog(Profile("selected")));
        var invalidCases = new List<byte[]>
        {
            Encoding.UTF8.GetBytes(" "),
            Encoding.UTF8.GetBytes("null"),
            Encoding.UTF8.GetBytes("{}"),
            new byte[] { 0xFF },
            new byte[] { 0xEF, 0xBB, 0xBF }.Concat(Catalog(Profile("selected"))).ToArray(),
            Encoding.UTF8.GetBytes("[/*comment*/" + json[1..]),
            Encoding.UTF8.GetBytes(json.Insert(json.Length - 1, ",")),
            Encoding.UTF8.GetBytes(json.Replace("\"profileId\"", "\"ProfileId\"", StringComparison.Ordinal)),
            Encoding.UTF8.GetBytes(json.Replace("\"tenantId\"", "\"unknownRoot\":1,\"tenantId\"", StringComparison.Ordinal)),
            Encoding.UTF8.GetBytes(json.Replace("\"maxNodes\":8", "\"unknownNested\":1,\"maxNodes\":8", StringComparison.Ordinal)),
            Encoding.UTF8.GetBytes(json.Replace("\"tenantId\":\"tenant-selected\",", "", StringComparison.Ordinal)),
            Encoding.UTF8.GetBytes(json.Replace("\"profileId\":\"selected\"", "\"profileId\":\"selected\",\"profileId\":\"selected\"", StringComparison.Ordinal)),
            Encoding.UTF8.GetBytes(json.Replace("\"maxNodes\":8", "\"maxNodes\":8,\"maxNodes\":8", StringComparison.Ordinal)),
            JsonSerializer.SerializeToUtf8Bytes<DynamicGraphLogicalOrchestratorBootstrapProfile?[]>([null], JsonOptions),
            Encoding.UTF8.GetBytes(json.Replace("\"profileId\":\"selected\"", "\"profileId\":\" selected\"", StringComparison.Ordinal)),
            Encoding.UTF8.GetBytes(json.Replace("\"profileId\":\"selected\"", "\"profileId\":\"selected\\u0001\"", StringComparison.Ordinal)),
            Encoding.UTF8.GetBytes(json.Replace("\"profileId\":\"selected\"", "\"profileId\":\"\\uD800\"", StringComparison.Ordinal))
        };

        foreach (var invalid in invalidCases)
            AssertCatalogFailure(invalid, "selected", InvalidCode, InvalidMessage);
    }

    [Fact]
    public void ComposeSelectedCore_ValidatesAllIdsBeforeOrdinalDuplicateAndSelection()
    {
        AssertCatalogFailure(
            Catalog(Profile("duplicate"), Profile("duplicate")),
            "duplicate",
            DuplicateCode,
            DuplicateMessage);
        AssertCatalogFailure(
            Catalog(Profile("duplicate"), Profile("duplicate"), Profile(" invalid")),
            "duplicate",
            InvalidCode,
            InvalidMessage);
        AssertCatalogFailure(Catalog(), "selected", NotFoundCode, NotFoundMessage);
        AssertCatalogFailure(Catalog(Profile("Selected")), "selected", NotFoundCode, NotFoundMessage);

        var expected = OperationResult<DynamicGraphLogicalOrchestratorCompositionResult>.Fail("sentinel", "SENTINEL");
        var recorder = new RecordingComposer(expected);
        var result = DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelectedCore(
            new NullSessionStorage(),
            Catalog(Profile("Case"), Profile("case")),
            "case",
            recorder.Compose);
        Assert.Same(expected, result);
        Assert.Equal(1, recorder.CallCount);
        Assert.Equal("case", recorder.Profile!.ProfileId, StringComparer.Ordinal);
    }

    [Fact]
    public void ComposeSelected_PublicBoundaryDeserializesAllButComposesOnlySelectedProfile()
    {
        var invalidUnselected = Profile("invalid-unselected") with
        {
            NodeDefinitions = [],
            Budget = new TopologyBuildBudget(0, 0, 0, 0)
        };

        var validSelected = DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelected(
            new NullSessionStorage(),
            Catalog(invalidUnselected, Profile("selected")),
            "selected");
        AssertSuccess(validSelected);

        var invalidBudget = Profile("selected") with { Budget = new TopologyBuildBudget(0, 8, 8, 8) };
        Assert.Equal(
            invalidBudget.Budget,
            AssertSuccess(DynamicGraphLogicalOrchestratorProfileCatalog.Select(
                Catalog(invalidBudget),
                "selected")).Budget);
        AssertFailure(
            DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelected(
                new NullSessionStorage(),
                Catalog(invalidBudget),
                "selected"),
            "DYNAMIC_GRAPH_TOPOLOGY_BUILD_BUDGET_INVALID",
            "Dynamic graph topology build budget is invalid.");

        var nullBudget = Profile("selected") with { Budget = null! };
        Assert.Null(AssertSuccess(DynamicGraphLogicalOrchestratorProfileCatalog.Select(
            Catalog(nullBudget),
            "selected")).Budget);
        AssertFailure(
            DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelected(
                new NullSessionStorage(),
                Catalog(nullBudget),
                "selected"),
            "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_BUDGET_REQUIRED",
            "A dynamic graph topology budget is required.");

        var nullNodes = Profile("selected") with { NodeDefinitions = null! };
        Assert.Null(AssertSuccess(DynamicGraphLogicalOrchestratorProfileCatalog.Select(
            Catalog(nullNodes),
            "selected")).NodeDefinitions);
        AssertFailure(
            DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelected(
                new NullSessionStorage(),
                Catalog(nullNodes),
                "selected"),
            "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_NODE_DEFINITIONS_REQUIRED",
            "At least one dynamic graph logical orchestrator node definition is required.");

        AssertFailure(
            DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelected(
                null,
                default,
                "selected"),
            RequiredCode,
            RequiredMessage);
        AssertFailure(
            DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelected(
                null,
                Catalog(Profile("selected")),
                "missing"),
            NotFoundCode,
            NotFoundMessage);
        AssertFailure(
            DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelected(
                null,
                Catalog(Profile("selected")),
                "selected"),
            "STORAGE_REQUIRED",
            "Session storage is required.");
    }

    [Fact]
    public void ComposeSelected_CatalogOrderDoesNotChangeSelectedRuntimeEvidence()
    {
        var selected = Profile("selected");
        var first = AssertSuccess(DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelected(
            new NullSessionStorage(),
            Catalog(Profile("a"), selected, Profile("z")),
            "selected"));
        var second = AssertSuccess(DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelected(
            new NullSessionStorage(),
            Catalog(Profile("z"), selected, Profile("a")),
            "selected"));

        Assert.Equal(first.ProfileId, second.ProfileId, StringComparer.Ordinal);
        Assert.Equal(RuntimeSnapshot(first.Runtime), RuntimeSnapshot(second.Runtime), StringComparer.Ordinal);
    }

    [Fact]
    public void ComposeSelectedCore_SnapshotsCallerBytesBeforeAndAfterDelegate()
    {
        var richInput = Catalog(Profile("first"), RichProfile("selected"), Profile("last"));
        var selected = AssertSuccess(DynamicGraphLogicalOrchestratorProfileCatalog.Select(
            richInput,
            "selected"));
        richInput.AsSpan().Fill((byte)'z');

        Assert.Equal("selected", selected.ProfileId, StringComparer.Ordinal);
        Assert.Equal("tenant-selected", selected.TenantId, StringComparer.Ordinal);
        Assert.Equal("3.4", Assert.Single(selected.NodeDefinitions).Descriptor.NodeId, StringComparer.Ordinal);
        var selectedEdge = Assert.Single(selected.EdgeBindings);
        Assert.Equal("edge-selected", selectedEdge.EdgeId, StringComparer.Ordinal);
        Assert.Equal(["transition-selected"], selectedEdge.LogicalTransitionIds);
        Assert.Equal(LeakageSignals, selectedEdge.ExcludedLeakageSignals);
        Assert.Equal("source-proof:selected", selectedEdge.SourceProof, StringComparer.Ordinal);

        var storage = new NullSessionStorage();
        var input = Catalog(Profile("selected"));
        DynamicGraphLogicalOrchestratorBootstrapProfile? captured = null;

        var result = DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelectedCore(
            storage,
            input,
            "selected",
            (selectedStorage, selectedProfile) =>
            {
                captured = selectedProfile;
                var composed = DynamicGraphLogicalOrchestratorComposition.Create(
                    selectedStorage!,
                    selectedProfile);
                input.AsSpan().Fill((byte)'x');
                return composed;
            });
        input.AsSpan().Fill((byte)'y');

        var composition = AssertSuccess(result);
        Assert.NotNull(captured);
        Assert.Equal("selected", captured.ProfileId, StringComparer.Ordinal);
        Assert.Equal("tenant-selected", captured.TenantId, StringComparer.Ordinal);
        Assert.Equal("3.4", Assert.Single(captured.NodeDefinitions).Descriptor.NodeId, StringComparer.Ordinal);
        Assert.Equal("selected", composition.ProfileId, StringComparer.Ordinal);
        Assert.Contains("3.4", RuntimeSnapshot(composition.Runtime), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Select_ConcurrentCallsReturnIndependentProfileGraphs()
    {
        ReadOnlyMemory<byte> catalog = Catalog(RichProfile("a"), RichProfile("b"));

        var results = await Task.WhenAll(
            Task.Run(() => DynamicGraphLogicalOrchestratorProfileCatalog.Select(catalog, "a")),
            Task.Run(() => DynamicGraphLogicalOrchestratorProfileCatalog.Select(catalog, "a")),
            Task.Run(() => DynamicGraphLogicalOrchestratorProfileCatalog.Select(catalog, "b")));

        var firstA = AssertSuccess(results[0]);
        var secondA = AssertSuccess(results[1]);
        var selectedB = AssertSuccess(results[2]);
        Assert.Equal("a", firstA.ProfileId, StringComparer.Ordinal);
        Assert.Equal("a", secondA.ProfileId, StringComparer.Ordinal);
        Assert.Equal("b", selectedB.ProfileId, StringComparer.Ordinal);
        Assert.NotSame(firstA, secondA);
        Assert.NotSame(firstA.NodeDefinitions, secondA.NodeDefinitions);
        Assert.NotSame(firstA.NodeDefinitions[0], secondA.NodeDefinitions[0]);
        Assert.NotSame(firstA.NodeDefinitions[0].Descriptor, secondA.NodeDefinitions[0].Descriptor);
        Assert.NotSame(firstA.EdgeBindings, secondA.EdgeBindings);
        Assert.NotSame(firstA.EdgeBindings[0], secondA.EdgeBindings[0]);
        Assert.NotSame(
            firstA.EdgeBindings[0].LogicalTransitionIds,
            secondA.EdgeBindings[0].LogicalTransitionIds);
        Assert.NotSame(
            firstA.EdgeBindings[0].ExcludedLeakageSignals,
            secondA.EdgeBindings[0].ExcludedLeakageSignals);

        firstA.NodeDefinitions[0] = Profile("mutated").NodeDefinitions[0];
        firstA.EdgeBindings[0].LogicalTransitionIds[0] = "mutated-transition";
        firstA.EdgeBindings[0].ExcludedLeakageSignals[0] = "mutated-signal";

        Assert.Equal("3.4", secondA.NodeDefinitions[0].Descriptor.NodeId, StringComparer.Ordinal);
        Assert.Equal("transition-a", secondA.EdgeBindings[0].LogicalTransitionIds[0], StringComparer.Ordinal);
        Assert.Equal("candidate_index", secondA.EdgeBindings[0].ExcludedLeakageSignals[0], StringComparer.Ordinal);
    }

    [Fact]
    public async Task ComposeSelected_PublicRuntimeExecutesExistingSecurityS1WithoutRegistryMutation()
    {
        const string runId = "catalog-security-s1-run";
        var composition = AssertSuccess(DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelected(
            new NullSessionStorage(),
            Catalog(Profile("selected")),
            "selected"));
        Assert.Equal(1, composition.Runtime.InitialApply.CurrentRevision);
        var assistant = composition.Runtime.Assistant;
        var flowRegistry = ReadPrivateField<InMemoryLogicalFlowRegistry>(assistant, "_factorySelectedTargetFlowRegistry");
        var unitRegistry = ReadPrivateField<InMemoryLogicalUnitRegistry>(assistant, "_factorySelectedTargetUnitRegistry");
        var topologyRegistry = ReadPrivateField<ILogicalTopologyRegistry>(assistant, "_factorySelectedTargetTopologyRegistry");
        var beforeFlows = SnapshotFlowDescriptors(flowRegistry);
        var beforeUnits = SnapshotUnitDescriptors(unitRegistry);
        var beforeTopology = SnapshotTopology(topologyRegistry);
        var context = new LogicalOrchestratorRequest(
            runId,
            SecurityVerification34FlowAdapter.ScenarioId,
            SecurityScenarioS1(),
            new LogicalExecutionFlags(),
            new Dictionary<string, object> { ["source"] = "profile-catalog" });

        var executed = await assistant.RunFactorySelectedTargetExecutionAsync(
            context,
            ValidTarget(SecurityVerification34FlowAdapter.FlowId),
            ValidAuthorityResult(SecurityVerification34FlowAdapter.FlowId),
            ValidNoveltyRequest(SecurityVerification34FlowAdapter.FlowId));

        Assert.True(executed.IsSuccess, $"{executed.ErrorCode}: {executed.ErrorMessage}");
        Assert.Equal(1, executed.Value!.DirectTargetInvocationCount);
        Assert.Equal(SecurityVerification34FlowAdapter.FlowId, executed.Value.ExecutionResult.TargetId, StringComparer.Ordinal);
        Assert.Equal(runId, executed.Value.ExecutionResult.ExecutionId, StringComparer.Ordinal);
        Assert.Equal(["CB4-STAGE-5-S1", "CB4-3.4-001"], executed.Value.ExecutionResult.EvidenceIds);
        Assert.Equal(beforeFlows, SnapshotFlowDescriptors(flowRegistry));
        Assert.Equal(beforeUnits, SnapshotUnitDescriptors(unitRegistry));
        Assert.Equal(beforeTopology, SnapshotTopology(topologyRegistry));
    }

    private static byte[] Catalog(params DynamicGraphLogicalOrchestratorBootstrapProfile[] profiles)
        => JsonSerializer.SerializeToUtf8Bytes(profiles, JsonOptions);

    private static byte[] NestedCatalog(int containerDepth)
    {
        var nestedDepth = containerDepth - 1;
        return Encoding.UTF8.GetBytes(
            "[" + new string('[', nestedDepth) + "0" + new string(']', nestedDepth) + "]");
    }

    private static DynamicGraphLogicalOrchestratorBootstrapProfile Profile(string profileId)
        => new(
            ProfileId: profileId,
            TenantId: $"tenant-{profileId}",
            DomainTag: DevelopmentLifecycleTopologySeeder.DomainId,
            ContextId: $"context-{profileId}",
            SignalId: $"signal-{profileId}",
            SourceRef: $"catalog:{profileId}",
            TraceRef: $"trace:{profileId}",
            NodeDefinitions:
            [
                new DynamicGraphLogicalOrchestratorNodeDefinition(
                    "3.4",
                    new DynamicGraphNodeDescriptor(
                        "3.4",
                        DevelopmentLifecycleTopologySeeder.DomainId,
                        "System.String",
                        "System.String",
                        false,
                        false,
                        false,
                        "baseline_candidate_generator",
                        null,
                        null))
            ],
            EdgeBindings: [],
            Budget: new TopologyBuildBudget(8, 8, 8, 8));

    private static DynamicGraphLogicalOrchestratorBootstrapProfile RichProfile(string profileId)
        => Profile(profileId) with
        {
            EdgeBindings =
            [
                new DynamicGraphRegistryEdgeBinding(
                    EdgeId: $"edge-{profileId}",
                    LogicalTransitionIds: [$"transition-{profileId}"],
                    Kind: GraphEdgeKind.FanOut,
                    ParallelEligible: false,
                    AuthorityUnitId: $"authority-{profileId}",
                    AuthorityUnitLevel: "L2",
                    DeterministicFinalAuthority: false,
                    BaselineUnitId: $"baseline-{profileId}",
                    BaselineRole: "candidate_generator",
                    ExcludedLeakageSignals: LeakageSignals.ToArray(),
                    SourceProof: $"source-proof:{profileId}",
                    CutoverProof: $"cutover-proof:{profileId}",
                    AblationProofRef: $"ablation-proof:{profileId}")
            ]
        };

    private static RecordingSelector SuccessfulSelector(string profileId)
        => new(OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile>.Ok(Profile(profileId)));

    private static string RuntimeSnapshot(DynamicGraphLogicalOrchestratorRuntime runtime)
    {
        var current = AssertSuccess(runtime.TopologySource.ReadCurrent());
        return string.Join(
            "|",
            current.Revision,
            current.SignalId,
            current.SourceRef,
            current.TraceRef,
            string.Join(",", current.Topology.Nodes.Select(node =>
                $"{node.NodeId}:{node.Descriptor.DomainTag}:{node.Descriptor.AuthorityRole}")),
            string.Join(",", current.Topology.Order.Select(entry => entry.NodeId)));
    }

    private static void AssertCatalogFailure(
        ReadOnlyMemory<byte> catalog,
        string? selectedProfileId,
        string code,
        string message)
    {
        var selection = DynamicGraphLogicalOrchestratorProfileCatalog.Select(
            catalog,
            selectedProfileId);
        AssertFailure(selection, code, message);

        var recorder = new RecordingComposer(
            OperationResult<DynamicGraphLogicalOrchestratorCompositionResult>.Fail("unexpected", "UNEXPECTED"));
        var result = DynamicGraphLogicalOrchestratorProfileCatalog.ComposeSelectedCore(
            new NullSessionStorage(),
            catalog,
            selectedProfileId,
            recorder.Compose);

        AssertFailure(result, code, message);
        Assert.Equal(0, recorder.CallCount);
        Assert.Null(recorder.Profile);
    }

    private static T AssertSuccess<T>(OperationResult<T> result)
    {
        Assert.True(result.IsSuccess, $"{result.ErrorCode}: {result.ErrorMessage}");
        return Assert.IsAssignableFrom<T>(result.Value);
    }

    private static void AssertFailure<T>(OperationResult<T> result, string code, string message)
    {
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(code, result.ErrorCode, StringComparer.Ordinal);
        Assert.Equal(message, result.ErrorMessage, StringComparer.Ordinal);
    }

    private static T ReadPrivateField<T>(object source, string fieldName)
    {
        var field = source.GetType().GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.NotNull(field);
        return Assert.IsAssignableFrom<T>(field.GetValue(source));
    }

    private static string[] SnapshotFlowDescriptors(InMemoryLogicalFlowRegistry registry)
        => registry.ListDescriptors()
            .Select(descriptor => string.Join(
                "|",
                descriptor.FlowId,
                descriptor.ScenarioId,
                descriptor.Version,
                descriptor.InputType.AssemblyQualifiedName,
                descriptor.RegistrationOrder))
            .ToArray();

    private static string[] SnapshotUnitDescriptors(InMemoryLogicalUnitRegistry registry)
        => registry.ListDescriptors()
            .Select(descriptor => string.Join(
                "|",
                descriptor.UnitId,
                descriptor.Version,
                descriptor.InputType.AssemblyQualifiedName,
                descriptor.OutputType.AssemblyQualifiedName,
                descriptor.RegistrationOrder))
            .ToArray();

    private static string[] SnapshotTopology(ILogicalTopologyRegistry registry)
    {
        var domains = AssertSuccess(registry.ListDomains());
        var phases = AssertSuccess(registry.ListPhases());
        var subPhases = AssertSuccess(registry.ListSubPhases());
        var transitions = AssertSuccess(registry.ListTransitions());
        return domains.Select(entry => entry.ToString() ?? "<null>")
            .Concat(phases.Select(entry => entry.ToString() ?? "<null>"))
            .Concat(subPhases.Select(entry => entry.ToString() ?? "<null>"))
            .Concat(transitions.Select(entry => entry.ToString() ?? "<null>"))
            .ToArray();
    }

    private static string SecurityScenarioS1()
        => """
            Our backend authentication service has verified controls in production: password minimum length 15 characters, OTP expiry 10 minutes, TOTP window 30 seconds, session idle timeout 20 minutes with risk analysis RA-AUTH-003, re-authentication before username email password or MFA factor changes, JWT validation with RS256 only, none algorithm rejected, exp nbf and aud validated, and RBAC plus per-resource ownership checks on all API endpoints. Does this implementation satisfy OWASP ASVS 5.0.0 for sub-phase 3.4?
            """;

    private static DynamicGraphMeasurementTarget ValidTarget(string edgeId)
        => new(
            edgeId,
            DynamicGraphMeasurementTargetLevel.L3LogicalFlow,
            DynamicGraphMeasurementMechanismKind.LearningSubject,
            false);

    private static DynamicGraphEdgeAuthorityNoveltyEvaluationRequest ValidNoveltyRequest(string edgeId)
    {
        var paired = Paired(edgeId);
        return new DynamicGraphEdgeAuthorityNoveltyEvaluationRequest(
            paired,
            [new DynamicGraphEdgeAuthorityNoveltyTrainingBinding("train-a", "train-hash", [A], [A])],
            paired.Rows.Select(row => row.RowId).ToArray(),
            Enumerable.Range(0, paired.Rows.Count)
                .Select(index => new DynamicGraphEdgeAuthorityNoveltyRowInput(
                    $"row-{index:D3}",
                    index % 2 == 0 ? B : C,
                    [index % 2 == 0 ? B : C]))
                .ToArray(),
            "fingerprint-normalization-v1",
            "tokenization-v1");
    }

    private static DynamicGraphEdgeAuthorityAblationEvaluationResult Paired(string edgeId)
    {
        const int rowCount = 2;
        const int learnedCorrect = 2;
        const int baselineCorrect = 1;
        var split = new DynamicGraphMeasurementSplitProof(
            ["train-a"],
            ["eval-a"],
            ["train-hash", "eval-hash"],
            0,
            "simhash",
            0.85d);
        var rows = Enumerable.Range(0, rowCount)
            .Select(index => new DynamicGraphEdgeAuthorityAblationRowEvidence(
                $"row-{index:D3}",
                "eval-a",
                "eval-hash",
                "document-000",
                "repo://eval/a",
                $"label-{index:D3}",
                "target-a",
                index < learnedCorrect ? "target-a" : "target-b",
                index < baselineCorrect ? "target-a" : "target-b",
                index < learnedCorrect,
                index < baselineCorrect))
            .ToArray();
        var learnedAccuracy = (double)learnedCorrect / rowCount;
        var baselineAccuracy = (double)baselineCorrect / rowCount;
        var rawDrop = learnedAccuracy - baselineAccuracy;
        var normalizedDrop = Math.Round(rawDrop, 12, MidpointRounding.AwayFromZero);
        var constructor = Assert.Single(
            typeof(DynamicGraphEdgeAuthorityAblationEvaluationResult)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance),
            candidate => candidate.GetParameters().Length == 12);
        return Assert.IsType<DynamicGraphEdgeAuthorityAblationEvaluationResult>(constructor.Invoke(
        [
            EvaluationId,
            edgeId,
            AuthorityUnitId,
            split,
            rows,
            learnedCorrect,
            learnedAccuracy,
            baselineCorrect,
            baselineAccuracy,
            rawDrop,
            normalizedDrop,
            normalizedDrop >= DynamicGraphAblationMeter.MinimumAccuracyDrop
        ]));
    }

    private static EdgeAuthorityExecutionResult ValidAuthorityResult(string edgeId)
    {
        var candidates = Enumerable.Range(0, 3)
            .Select(index => new EdgeCandidate(
                $"target-{(char)('a' + index)}",
                [$"PREDICATE_{(char)('A' + index)}"],
                0.2d + (index * 0.2d),
                RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole))
            .ToArray();
        var decision = new EdgeAuthorityDecision(
            [("target-c", 0.93d)],
            1,
            false,
            RoutingAuthorityVocabulary.TrainedEdgeUnitRole,
            candidates.OrderByDescending(candidate => candidate.BaselineScore)
                .ThenBy(candidate => candidate.TargetSubPhaseId, StringComparer.Ordinal)
                .Select(candidate => candidate.TargetSubPhaseId)
                .ToArray());
        var normalizedInput = new EdgeAuthorityInput(
            edgeId,
            ["edge", "selection", "target"],
            candidates.SelectMany(candidate => candidate.EvidencePredicates).ToArray(),
            new ContextSignals(
                [Domain.CODE_Q],
                ["completed-sub-phase"],
                ["active-edge"],
                new Dictionary<Domain, double> { [Domain.CODE_Q] = 0.8d }),
            candidates,
            LeakageSignals);
        var orderReport = new DynamicGraphEdgeCandidateOrderLeakageReport(
            edgeId,
            AuthorityUnitId,
            true,
            Enumerable.Range(0, 5)
                .Select(index => new DynamicGraphEdgeCandidateOrderPermutationProbe(
                    $"candidate-order-permutation-{index + 1:D2}",
                    CreatePermutation(candidates, index),
                    CopyDecision(decision)))
                .ToArray(),
            LeakageSignals);
        var identityReport = new DynamicGraphEdgeIdentityLeakageReport(
            edgeId,
            AuthorityUnitId,
            true,
            Enumerable.Range(0, 3)
                .Select(index => new DynamicGraphEdgeIdentityControlProbe(
                    IdentityProbeIds[index],
                    IdentityControlledSignals[index],
                    IdentityControlValues[index],
                    CopyDecision(decision)))
                .ToArray(),
            LeakageSignals);
        return new EdgeAuthorityExecutionResult(
            normalizedInput,
            candidates.Select(CopyCandidate).ToArray(),
            CopyDecision(decision),
            new DynamicGraphEdgeAuthorityEvidence(
                edgeId,
                AuthorityUnitId,
                "L2",
                false,
                "selection-target-baseline-unit",
                "candidate_generator",
                LeakageSignals,
                "selection-target-source-proof",
                "selection-target-cutover-proof",
                "selection-target-ablation-proof"),
            orderReport,
            identityReport,
            0.93d,
            ["evidence"],
            []);
    }

    private static EdgeCandidate[] CreatePermutation(IReadOnlyList<EdgeCandidate> candidates, int index)
    {
        var source = candidates.Select(CopyCandidate).ToArray();
        var offset = index switch
        {
            0 => 0,
            1 => 1,
            2 => 2,
            3 => -1,
            4 => -2,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
        return offset < 0
            ? Rotate(source.Reverse().ToArray(), -offset - 1)
            : Rotate(source, offset);
    }

    private static EdgeCandidate[] Rotate(IReadOnlyList<EdgeCandidate> source, int offset)
        => Enumerable.Range(0, source.Count)
            .Select(index => CopyCandidate(source[(index + offset) % source.Count]))
            .ToArray();

    private static EdgeCandidate CopyCandidate(EdgeCandidate source)
        => source with { EvidencePredicates = source.EvidencePredicates.ToArray() };

    private static EdgeAuthorityDecision CopyDecision(EdgeAuthorityDecision source)
        => new(
            source.OrderedTargets.ToArray(),
            source.FanOutCount,
            source.KnowledgeEdge,
            source.AuthoritySource,
            source.AblationBaselineRank.ToArray());

    private sealed class RecordingComposer
    {
        internal RecordingComposer(OperationResult<DynamicGraphLogicalOrchestratorCompositionResult> result)
        {
            Result = result;
        }

        internal OperationResult<DynamicGraphLogicalOrchestratorCompositionResult> Result { get; }

        internal int CallCount { get; private set; }

        internal ISessionStorage? Storage { get; private set; }

        internal DynamicGraphLogicalOrchestratorBootstrapProfile? Profile { get; private set; }

        internal OperationResult<DynamicGraphLogicalOrchestratorCompositionResult> Compose(
            ISessionStorage? storage,
            DynamicGraphLogicalOrchestratorBootstrapProfile profile)
        {
            CallCount++;
            Storage = storage;
            Profile = profile;
            return Result;
        }
    }

    private sealed class RecordingSelector
    {
        internal RecordingSelector(OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile> result)
        {
            Result = result;
        }

        internal OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile> Result { get; }

        internal int CallCount { get; private set; }

        internal ReadOnlyMemory<byte> Catalog { get; private set; }

        internal string? SelectedProfileId { get; private set; }

        internal OperationResult<DynamicGraphLogicalOrchestratorBootstrapProfile> Select(
            ReadOnlyMemory<byte> catalog,
            string? selectedProfileId)
        {
            CallCount++;
            Catalog = catalog;
            SelectedProfileId = selectedProfileId;
            return Result;
        }
    }

    private sealed class NullSessionStorage : ISessionStorage
    {
        public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
            => Task.FromResult(OperationResult<bool>.Ok(true));

        public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
            => Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));

        public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
            => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>()));
    }
}
