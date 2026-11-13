// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using OrchestratorModel.Logical;

public sealed class DynamicGraphCompositionContractsArchitectureTests
{
    private const string ContractsAssemblyName = "DynamicGraphComposition.Contracts";
    private const string RuntimeAssemblyName = "OrchestratorModel";

    [Fact]
    public void CarrierTypes_AreOwnedByContractsAssemblyWhileRuntimeTypesRemainInOrchestratorModel()
    {
        Assert.Equal(
            ContractsAssemblyName,
            typeof(DynamicGraphLogicalOrchestratorNodeDefinition).Assembly.GetName().Name);
        Assert.Equal(
            ContractsAssemblyName,
            typeof(DynamicGraphLogicalOrchestratorBootstrapProfile).Assembly.GetName().Name);
        Assert.Equal(
            ContractsAssemblyName,
            typeof(DynamicGraphLogicalOrchestratorSessionDescriptor).Assembly.GetName().Name);
        Assert.Equal(
            ContractsAssemblyName,
            typeof(DynamicGraphLogicalOrchestratorSessionResult).Assembly.GetName().Name);
        Assert.Equal(
            ContractsAssemblyName,
            typeof(IDynamicGraphLogicalOrchestratorCompositionSession).Assembly.GetName().Name);
        Assert.Equal(
            ContractsAssemblyName,
            typeof(IDynamicGraphLogicalOrchestratorTopologyMutationSession).Assembly.GetName().Name);
        Assert.Equal(
            ContractsAssemblyName,
            typeof(IDynamicGraphLogicalOrchestratorTopologyRunSession).Assembly.GetName().Name);
        Assert.Equal(
            ContractsAssemblyName,
            typeof(IDynamicGraphLogicalOrchestratorCompositionExecutor).Assembly.GetName().Name);

        Assert.Equal(
            RuntimeAssemblyName,
            typeof(DynamicGraphLogicalOrchestratorCompositionResult).Assembly.GetName().Name);
        Assert.Equal(
            RuntimeAssemblyName,
            typeof(DynamicGraphLogicalOrchestratorComposition).Assembly.GetName().Name);
        Assert.Equal(
            RuntimeAssemblyName,
            typeof(DynamicGraphLogicalOrchestratorRuntime).Assembly.GetName().Name);
    }

    [Fact]
    public void OrchestratorModel_ExportsForwardersForBothExtractedCarrierTypes()
    {
        var runtimeAssembly = typeof(DynamicGraphLogicalOrchestratorComposition).Assembly;
        var forwardedTypes = runtimeAssembly.GetForwardedTypes();

        Assert.Equal(
            [
                typeof(DynamicGraphLogicalOrchestratorBootstrapProfile),
                typeof(DynamicGraphLogicalOrchestratorNodeDefinition)
            ],
            forwardedTypes.OrderBy(type => type.FullName, StringComparer.Ordinal));
        Assert.All(forwardedTypes, type => Assert.Equal(ContractsAssemblyName, type.Assembly.GetName().Name));
    }

    [Fact]
    public void CarrierPublicPositionalShape_IsExactlyPreserved()
    {
        AssertCarrierShape(
            typeof(DynamicGraphLogicalOrchestratorNodeDefinition),
            [
                ("LogicalSubPhaseId", typeof(string)),
                ("Descriptor", typeof(DynamicGraphNodeDescriptor))
            ]);

        AssertCarrierShape(
            typeof(DynamicGraphLogicalOrchestratorBootstrapProfile),
            [
                ("ProfileId", typeof(string)),
                ("TenantId", typeof(string)),
                ("DomainTag", typeof(string)),
                ("ContextId", typeof(string)),
                ("SignalId", typeof(string)),
                ("SourceRef", typeof(string)),
                ("TraceRef", typeof(string)),
                ("NodeDefinitions", typeof(DynamicGraphLogicalOrchestratorNodeDefinition[])),
                ("EdgeBindings", typeof(DynamicGraphRegistryEdgeBinding[])),
                ("Budget", typeof(TopologyBuildBudget))
            ]);

        AssertCarrierShape(
            typeof(DynamicGraphLogicalOrchestratorSessionDescriptor),
            [
                ("ProfileId", typeof(string)),
                ("TenantId", typeof(string)),
                ("DomainTag", typeof(string)),
                ("ContextId", typeof(string))
            ]);

        AssertCarrierShape(
            typeof(DynamicGraphLogicalOrchestratorSessionResult),
            [
                ("Descriptor", typeof(DynamicGraphLogicalOrchestratorSessionDescriptor)),
                ("Revision", typeof(long)),
                ("SignalId", typeof(string)),
                ("SourceKind", typeof(DynamicGraphBuildSourceKind)),
                ("NodeCount", typeof(int)),
                ("EdgeCount", typeof(int)),
                ("InitialRevisionAdvanced", typeof(bool))
            ]);

        AssertCarrierShape(
            typeof(DynamicGraphLogicalOrchestratorTopologyRunResult),
            [
                ("TopologyRevision", typeof(long)),
                ("TopologySignalId", typeof(string)),
                ("TopologySourceKind", typeof(DynamicGraphBuildSourceKind)),
                ("Run", typeof(DynamicGraphLogicalOrchestratorRunResult))
            ]);
    }

    [Fact]
    public void ExecutionContracts_PublicSignaturesRemainCoreOnlyAndDtoSurfaceIsBounded()
    {
        var executionContractTypes = new[]
        {
            typeof(DynamicGraphLogicalOrchestratorSessionDescriptor),
            typeof(DynamicGraphLogicalOrchestratorSessionResult),
            typeof(DynamicGraphLogicalOrchestratorTopologyRunResult),
            typeof(IDynamicGraphLogicalOrchestratorCompositionSession),
            typeof(IDynamicGraphLogicalOrchestratorTopologyMutationSession),
            typeof(IDynamicGraphLogicalOrchestratorTopologyRunSession),
            typeof(IDynamicGraphLogicalOrchestratorCompositionExecutor)
        };
        Assert.All(
            executionContractTypes,
            type => Assert.Equal(ContractsAssemblyName, type.Assembly.GetName().Name));

        var publicSignatureTypes = executionContractTypes
            .SelectMany(PublicSignatureTypes)
            .Distinct()
            .ToArray();
        Assert.DoesNotContain(publicSignatureTypes, type =>
            type.FullName?.Contains("DynamicGraphLogicalOrchestratorRuntime", StringComparison.Ordinal) == true
            || type.FullName?.Contains("IDynamicGraphTopologyRuntimeOwner", StringComparison.Ordinal) == true
            || type.FullName?.Contains("ISessionStorage", StringComparison.Ordinal) == true
            || type.FullName?.Contains("LogicalOrchestratorAssistant", StringComparison.Ordinal) == true
            || type.FullName?.Contains("ProjectWikiTrainingFlowAdapter", StringComparison.Ordinal) == true
            || type.FullName?.Contains("Training", StringComparison.OrdinalIgnoreCase) == true
            || type.FullName?.Contains("Dpo", StringComparison.OrdinalIgnoreCase) == true
            || type.FullName?.Contains("Teacher", StringComparison.OrdinalIgnoreCase) == true
            || type.FullName?.Contains("Judge", StringComparison.OrdinalIgnoreCase) == true);

        var resultProperties = typeof(DynamicGraphLogicalOrchestratorSessionResult)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.DoesNotContain(resultProperties, property =>
            property.Name is "SourceRef" or "TraceRef"
            || property.PropertyType == typeof(DynamicGraphNodeDescriptor)
            || property.PropertyType.IsArray
            || typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType)
               && property.PropertyType != typeof(string));
        Assert.DoesNotContain(
            resultProperties,
            property => property.Name.Contains("Receipt", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TopologyRunContract_IsStandaloneExactOneMethodCoreOnlySurface()
    {
        var topologyRunSession = typeof(IDynamicGraphLogicalOrchestratorTopologyRunSession);
        Assert.True(topologyRunSession.IsPublic);
        Assert.True(topologyRunSession.IsInterface);
        Assert.Empty(topologyRunSession.GetInterfaces());
        Assert.Empty(topologyRunSession.GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

        var method = Assert.Single(
            topologyRunSession.GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            candidate => !candidate.IsSpecialName);
        Assert.Equal("RunOnCurrentTopologyAsync", method.Name);
        Assert.Equal(
            ["request", "cancellationToken"],
            method.GetParameters().Select(parameter => parameter.Name));
        Assert.Equal(
            [typeof(DynamicGraphLogicalOrchestratorRunRequest), typeof(CancellationToken)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.False(method.GetParameters()[0].HasDefaultValue);
        Assert.True(method.GetParameters()[1].HasDefaultValue);
        Assert.Equal(
            typeof(Task<OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>>),
            method.ReturnType);

        Assert.False(topologyRunSession.IsAssignableFrom(
            typeof(IDynamicGraphLogicalOrchestratorCompositionSession)));
        Assert.False(topologyRunSession.IsAssignableFrom(
            typeof(IDynamicGraphLogicalOrchestratorTopologyMutationSession)));
        Assert.False(topologyRunSession.IsAssignableFrom(
            typeof(IDynamicGraphLogicalOrchestratorRunSession)));
        Assert.DoesNotContain(
            PublicSignatureTypes(topologyRunSession),
            type => type == typeof(IDynamicGraphTopologyRuntimeOwner)
                    || type == typeof(ISessionStorage)
                    || type.FullName?.Contains("Training", StringComparison.OrdinalIgnoreCase) == true
                    || type.FullName?.Contains("Dpo", StringComparison.OrdinalIgnoreCase) == true
                    || type.FullName?.Contains("Teacher", StringComparison.OrdinalIgnoreCase) == true
                    || type.FullName?.Contains("Judge", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public void TopologyMutationContract_IsSeparateExactOneMethodSurfaceAndReadSessionRemainsTwoMembers()
    {
        var readSession = typeof(IDynamicGraphLogicalOrchestratorCompositionSession);
        Assert.Empty(readSession.GetInterfaces());
        Assert.Equal(
            ["Descriptor"],
            readSession
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name));
        Assert.Equal(
            ["ReadCurrent"],
            readSession
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsSpecialName)
                .Select(method => method.Name));

        var mutationSession = typeof(IDynamicGraphLogicalOrchestratorTopologyMutationSession);
        Assert.True(mutationSession.IsPublic);
        Assert.True(mutationSession.IsInterface);
        Assert.Empty(mutationSession.GetInterfaces());
        Assert.Empty(
            mutationSession.GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        var method = Assert.Single(
            mutationSession.GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            candidate => !candidate.IsSpecialName);
        Assert.Equal("ApplyRegistryProjection", method.Name);
        Assert.Equal(
            ["expectedRevision", "request", "budget"],
            method.GetParameters().Select(parameter => parameter.Name));
        Assert.Equal(
            [typeof(long), typeof(DynamicGraphRegistryProjectionRequest), typeof(TopologyBuildBudget)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.All(method.GetParameters(), parameter => Assert.False(parameter.HasDefaultValue));
        Assert.Equal(
            typeof(OperationResult<DynamicGraphTopologyApplyResult>),
            method.ReturnType);
        Assert.False(mutationSession.IsAssignableFrom(readSession));
        Assert.False(readSession.IsAssignableFrom(mutationSession));

        Assert.Equal(
            "Core",
            typeof(DynamicGraphRegistryProjectionRequest).Assembly.GetName().Name);
        Assert.Equal("Core", typeof(TopologyBuildBudget).Assembly.GetName().Name);
        Assert.Equal("Core", typeof(DynamicGraphTopologyApplyResult).Assembly.GetName().Name);
        Assert.DoesNotContain(
            PublicSignatureTypes(mutationSession),
            type => type == typeof(IDynamicGraphTopologyRuntimeOwner)
                    || type == typeof(ISessionStorage)
                    || type.FullName?.Contains("Training", StringComparison.OrdinalIgnoreCase) == true
                    || type.FullName?.Contains("Dpo", StringComparison.OrdinalIgnoreCase) == true);
    }

    [Fact]
    public void ContractsProject_HasExactSettingsSingleDirectReferenceAndExpectedRecursiveClosure()
    {
        var root = FindRepositoryRoot();
        var contractsProject = Path.Combine(
            root,
            "19.DynamicGraphComposition.Contracts",
            "19.DynamicGraphComposition.Contracts.csproj");
        var document = XDocument.Load(contractsProject);

        Assert.Equal("net9.0", ReadSingleProperty(document, "TargetFramework"));
        Assert.Equal("enable", ReadSingleProperty(document, "Nullable"));
        Assert.Equal("enable", ReadSingleProperty(document, "ImplicitUsings"));
        Assert.Equal(ContractsAssemblyName, ReadSingleProperty(document, "AssemblyName"));
        Assert.Equal("OrchestratorModel.Logical", ReadSingleProperty(document, "RootNamespace"));

        var directReferences = ReadProjectReferences(contractsProject);
        Assert.Equal(
            [NormalizeRelativePath(Path.Combine("00.Core", "Core.csproj"))],
            directReferences.Select(path => NormalizeRelativePath(Path.GetRelativePath(root, path))));

        Assert.Equal(
            [
                NormalizeRelativePath(Path.Combine("00.Core", "Core.csproj")),
                NormalizeRelativePath(Path.Combine("19.DynamicGraphComposition.Contracts", "19.DynamicGraphComposition.Contracts.csproj")),
                NormalizeRelativePath(Path.Combine("29.SemanticCompiler", "SemanticCompiler.csproj"))
            ],
            ReadProjectClosure(contractsProject)
                .Select(path => NormalizeRelativePath(Path.GetRelativePath(root, path)))
                .OrderBy(path => path, StringComparer.Ordinal));
    }

    [Fact]
    public void ContractsAssemblyAndProjectGraph_ContainNoForbiddenRuntimeOrTrainingDependencies()
    {
        var root = FindRepositoryRoot();
        var contractsProject = Path.Combine(
            root,
            "19.DynamicGraphComposition.Contracts",
            "19.DynamicGraphComposition.Contracts.csproj");
        var forbiddenProjectSegments = new[]
        {
            "15.OrchestratorModel",
            "21.AgentInfrastructure",
            "18.CapabilityTraining",
            "11.DPO",
            "25.ModelServer",
            "42.ProjectWikiDomain",
            "35.ExperimentsV2",
            "27.TranslationDomain",
            "44.DesignConversionDomain",
            "tools"
        };

        var closure = ReadProjectClosure(contractsProject)
            .Select(NormalizeRelativePath)
            .ToArray();
        Assert.DoesNotContain(
            closure,
            path => forbiddenProjectSegments.Any(segment =>
                path.Contains(segment, StringComparison.OrdinalIgnoreCase)));

        var referencedAssemblies = typeof(DynamicGraphLogicalOrchestratorBootstrapProfile)
            .Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();
        Assert.DoesNotContain(
            referencedAssemblies,
            name => name.Contains("OrchestratorModel", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("AgentInfrastructure", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("CapabilityTraining", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("DPO", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("ModelServer", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("ProjectWiki", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("ExperimentsV2", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("TranslationDomain", StringComparison.OrdinalIgnoreCase)
                    || name.Contains("DesignConversion", StringComparison.OrdinalIgnoreCase));

        var contractFiles = RepositorySourceScanner.EnumerateFiles(
                Path.GetDirectoryName(contractsProject)!,
                "*")
            .Where(path => !IsBuildArtifact(path))
            .ToArray();
        var forbiddenContent = new Regex(
            @"\b(dpo|teacher|judge|tp1|tp2|training|trainbatch|teachingsessionbuilder)\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        Assert.DoesNotContain(
            contractFiles,
            path => forbiddenContent.IsMatch(File.ReadAllText(path)));
    }

    [Fact]
    public void ExtractedCarrierFqns_HaveOneDefinitionAndNoDuplicateInOrchestratorModel()
    {
        var contractsAssembly = typeof(DynamicGraphLogicalOrchestratorBootstrapProfile).Assembly;
        var runtimeAssembly = typeof(DynamicGraphLogicalOrchestratorComposition).Assembly;
        var expectedNames = new[]
        {
            typeof(DynamicGraphLogicalOrchestratorNodeDefinition).FullName!,
            typeof(DynamicGraphLogicalOrchestratorBootstrapProfile).FullName!
        };

        foreach (var expectedName in expectedNames)
        {
            Assert.Single(
                contractsAssembly.DefinedTypes,
                type => string.Equals(type.FullName, expectedName, StringComparison.Ordinal));
            Assert.DoesNotContain(
                runtimeAssembly.DefinedTypes,
                type => string.Equals(type.FullName, expectedName, StringComparison.Ordinal));
        }

        var root = FindRepositoryRoot();
        var sourceFiles = RepositorySourceScanner.EnumerateCSharpFiles(root)
            .Where(path => !IsBuildArtifact(path))
            .ToArray();
        foreach (var typeName in expectedNames.Select(name => name[(name.LastIndexOf('.') + 1)..]))
        {
            var declaration = new Regex(
                $@"\bpublic\s+sealed\s+record\s+{Regex.Escape(typeName)}\b",
                RegexOptions.CultureInvariant);
            Assert.Single(sourceFiles, path => declaration.IsMatch(File.ReadAllText(path)));
        }
    }

    [Fact]
    public void Solution_RegistersContractsProjectInsideMatchingSolutionFolder()
    {
        var solution = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "LlmFromScratch.sln"));
        var folderMatch = Regex.Match(
            solution,
            "Project\\(\"\\{2150E333-8FDC-42A3-9474-1A3956D46DE8\\}\"\\) = \"19\\.DynamicGraphComposition\\.Contracts\", \"19\\.DynamicGraphComposition\\.Contracts\", \"(?<id>\\{[^}]+\\})\"",
            RegexOptions.CultureInvariant);
        var projectMatch = Regex.Match(
            solution,
            "Project\\(\"\\{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC\\}\"\\) = \"19\\.DynamicGraphComposition\\.Contracts\", \"19\\.DynamicGraphComposition\\.Contracts\\\\19\\.DynamicGraphComposition\\.Contracts\\.csproj\", \"(?<id>\\{[^}]+\\})\"",
            RegexOptions.CultureInvariant);

        Assert.True(folderMatch.Success);
        Assert.True(projectMatch.Success);
        Assert.Contains(
            $"{projectMatch.Groups["id"].Value} = {folderMatch.Groups["id"].Value}",
            solution,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExecutorProductionSource_UsesExplicitCoreChainsAndFailsClosedOnSuccessfulNullSource()
    {
        var root = FindRepositoryRoot();
        var adapterSource = File.ReadAllText(Path.Combine(
            root,
            "15.OrchestratorModel",
            "Logical",
            "DynamicGraphLogicalOrchestratorCompositionExecutorAdapter.cs"));
        var compositionSource = File.ReadAllText(Path.Combine(
            root,
            "15.OrchestratorModel",
            "Logical",
            "DynamicGraphLogicalOrchestratorComposition.cs"));

        Assert.Single(Regex.Matches(adapterSource, @"\.CreateCore\(").Cast<Match>());
        Assert.Equal(2, Regex.Matches(
            compositionSource,
            @"DynamicGraphLogicalOrchestratorRuntime\.CreateDefault\(").Count);
        Assert.Single(Regex.Matches(
            compositionSource,
            @"DynamicGraphLogicalOrchestratorRuntime\.CreateDefaultWithEdgeAuthorityPreload\(").Cast<Match>());
        Assert.Contains("NullSessionStorage.Instance", adapterSource, StringComparison.Ordinal);
        Assert.Contains("current.IsSuccess && current.Value is null", adapterSource, StringComparison.Ordinal);
        Assert.Contains(
            "DYNAMIC_GRAPH_LOGICAL_ORCHESTRATOR_SESSION_SOURCE_RESULT_REQUIRED",
            adapterSource,
            StringComparison.Ordinal);
        var applyRegistryProjectionMethod = Regex.Match(
            adapterSource,
            @"public\s+OperationResult<DynamicGraphTopologyApplyResult>\s+ApplyRegistryProjection\(\s*long expectedRevision,\s*DynamicGraphRegistryProjectionRequest request,\s*TopologyBuildBudget budget\)\s*\{(?<body>[\s\S]*?)\n        \}\s*\n\s*public OperationResult<DynamicGraphLogicalOrchestratorCheckpointCapture>",
            RegexOptions.CultureInvariant);
        Assert.True(applyRegistryProjectionMethod.Success);
        var applyRegistryProjectionBody = applyRegistryProjectionMethod.Groups["body"].Value;
        Assert.Single(Regex.Matches(
            applyRegistryProjectionBody,
            @"_runtime\.ApplyRegistryProjection\(\s*expectedRevision,\s*request!,\s*budget!\s*\)")
            .Cast<Match>());
        Assert.Single(Regex.Matches(
            applyRegistryProjectionBody,
            @"candidate\s*=\s*CreateProfileFromProjection\(\s*request,\s*budget\s*\)\s*;")
            .Cast<Match>());
        Assert.Single(Regex.Matches(
            applyRegistryProjectionBody,
            @"if\s*\(\s*applied is null\s*\)\s*return\s+SessionSourceFailure<DynamicGraphTopologyApplyResult>\(\s*\)\s*;")
            .Cast<Match>());
        Assert.Single(Regex.Matches(
            applyRegistryProjectionBody,
            @"if\s*\(\s*applied\.IsSuccess\s*&&\s*applied\.Value is not null\s*&&\s*applied\.Value\.RevisionAdvanced\s*&&\s*candidate is not null\s*\)\s*\{\s*lock\s*\(\s*_profileGate\s*\)\s*_currentProfile\s*=\s*candidate\s*;\s*\}")
            .Cast<Match>());
        Assert.Single(Regex.Matches(
            applyRegistryProjectionBody,
            @"_currentProfile\s*=\s*candidate\s*;")
            .Cast<Match>());
        Assert.Single(Regex.Matches(
            applyRegistryProjectionBody,
            @"return\s+applied\s*;")
            .Cast<Match>());

        var topologyRunMethod = Regex.Match(
            adapterSource,
            @"RunOnCurrentTopologyAsync\([\s\S]*?\n        private static OperationResult<DynamicGraphLogicalOrchestratorRunResult> MapOwnedRunResult",
            RegexOptions.CultureInvariant);
        Assert.True(topologyRunMethod.Success);
        Assert.Single(Regex.Matches(
            topologyRunMethod.Value,
            @"_runtime\.TopologySource\.ReadCurrent\(\)").Cast<Match>());
        Assert.Contains(
            "RunPreparedTopologyAsync",
            topologyRunMethod.Value,
            StringComparison.Ordinal);
        Assert.Contains(
            "scenarioId: AdaptiveLogicalOrchestrator.ScenarioId",
            topologyRunMethod.Value,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "RunAsync(",
            topologyRunMethod.Value,
            StringComparison.Ordinal);

        var legacyRunMethod = Regex.Match(
            adapterSource,
            @"public async Task<OperationResult<DynamicGraphLogicalOrchestratorRunResult>> RunAsync\([\s\S]*?\n        public async Task<OperationResult<DynamicGraphLogicalOrchestratorTopologyRunResult>>",
            RegexOptions.CultureInvariant);
        Assert.True(legacyRunMethod.Success);
        Assert.DoesNotContain(
            "_runtime.TopologySource.ReadCurrent()",
            legacyRunMethod.Value,
            StringComparison.Ordinal);
        Assert.Contains("MapOwnedRunResult", legacyRunMethod.Value, StringComparison.Ordinal);
    }

    private static void AssertCarrierShape(
        Type carrierType,
        IReadOnlyList<(string Name, Type Type)> expected)
    {
        Assert.Equal("OrchestratorModel.Logical", carrierType.Namespace);
        Assert.True(carrierType.IsPublic);
        Assert.True(carrierType.IsSealed);

        var properties = carrierType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .OrderBy(property => property.MetadataToken)
            .Select(property => (property.Name, property.PropertyType))
            .ToArray();
        Assert.Equal(expected, properties);

        var constructor = Assert.Single(carrierType.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Equal(
            expected.Select(item => item.Type),
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
    }

    private static string ReadSingleProperty(XDocument document, string name)
        => Assert.Single(document.Descendants(name)).Value;

    private static IEnumerable<Type> PublicSignatureTypes(Type type)
        => type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .SelectMany(constructor => constructor.GetParameters().Select(parameter => parameter.ParameterType))
            .Concat(type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType)))
            .Concat(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(property => property.PropertyType))
            .SelectMany(FlattenType);

    private static IEnumerable<Type> FlattenType(Type type)
    {
        yield return type;
        if (!type.IsGenericType)
            yield break;

        foreach (var argument in type.GetGenericArguments().SelectMany(FlattenType))
            yield return argument;
    }

    private static string[] ReadProjectReferences(string projectPath)
    {
        var projectDirectory = Path.GetDirectoryName(projectPath)!;
        return XDocument.Load(projectPath)
            .Descendants("ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => Path.GetFullPath(Path.Combine(projectDirectory, value!)))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string[] ReadProjectClosure(string rootProject)
    {
        var closure = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new Stack<string>();
        pending.Push(Path.GetFullPath(rootProject));
        while (pending.Count > 0)
        {
            var project = pending.Pop();
            if (!closure.Add(project))
                continue;

            foreach (var reference in ReadProjectReferences(project))
                pending.Push(reference);
        }

        return closure.OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
            directory = directory.Parent;

        return directory?.FullName
               ?? throw new DirectoryNotFoundException("Repository root with LlmFromScratch.sln was not found.");
    }

    private static string NormalizeRelativePath(string path)
        => path.Replace('\\', '/');

    private static bool IsBuildArtifact(string path)
        => NormalizeRelativePath(path).Contains("/bin/", StringComparison.OrdinalIgnoreCase)
           || NormalizeRelativePath(path).Contains("/obj/", StringComparison.OrdinalIgnoreCase);
}
