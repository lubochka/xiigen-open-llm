// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
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
    }

    [Fact]
    public void ExecutionContracts_PublicSignaturesRemainCoreOnlyAndDtoSurfaceIsBounded()
    {
        var executionContractTypes = new[]
        {
            typeof(DynamicGraphLogicalOrchestratorSessionDescriptor),
            typeof(DynamicGraphLogicalOrchestratorSessionResult),
            typeof(IDynamicGraphLogicalOrchestratorCompositionSession),
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
            || type.FullName?.Contains("ProjectWikiTrainingFlowAdapter", StringComparison.Ordinal) == true);

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

        var contractFiles = Directory.EnumerateFiles(
                Path.GetDirectoryName(contractsProject)!,
                "*",
                SearchOption.AllDirectories)
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
        var sourceFiles = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
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
