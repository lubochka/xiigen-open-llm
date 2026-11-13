// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace LlmFromScratch.Tests;

// Guard против возврата к бедной DPO-паре как training authority: source census
// проверяет Roslyn semantic type, а не строку "new DpoPair" в тексте.
public sealed class PoorDpoPairConstructionGuardTests
{
    private static readonly object RepoCensusLock = new();
    private static IReadOnlyList<DpoPairConstructionSite>? cachedRepoCensus;

    private const string CustomsSite =
        "11.DPO/Models/DpoTrainerPairAdmission.cs|DpoTrainerPairCustoms.CreateAdmitted|var pair = new DpoPair";
    private const string TranslationHeldOutSite =
        "27.TranslationDomain/TranslationDpoTrainerBridge.cs|TranslationDpoTextFeaturizer.BuildRecord(TranslationDpoHeldOutEvaluationRecord)|Pair: new DpoPair";
    private const string ExternalExperimentSite =
        "experiments-v2/INTERNAL-TRAIN-READY-V191/runner/InternalTrainReadyDpoRunner/Program.cs|InternalTrainReadyDpoRunner.top-level|pairs.Select(pair => JsonSerializer.Serialize(new DpoPair";

    [Fact]
    public void DpoModelsPoorPairConstruction_CensusContainsOnlyKnownSites()
    {
        var sites = FindDpoModelsPairConstructionSites()
            .Select(site => site.Key)
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToArray();
        var expected = new[]
        {
            CustomsSite,
            TranslationHeldOutSite,
            ExternalExperimentSite
        }.OrderBy(key => key, StringComparer.Ordinal).ToArray();

        // Изменение census требует явного решения: semantic guard ловит
        // unqualified/qualified/global/alias и target-typed формы.
        Assert.Equal(expected, sites);
    }

    [Fact]
    public void ProductTrainingPoorPairConstruction_OnlyCustomsOwnsAdmissions()
    {
        var sites = FindDpoModelsPairConstructionSites();
        var forbiddenProductTrainingSites = sites
            .Where(site => site.Scope == DpoPairConstructionScope.ProductTraining)
            .ToArray();

        // Translation оставляет прямую бедную пару только для held-out evaluation:
        // рядом обязан быть RowKind HELDOUT и Admission null.
        var heldOut = Assert.Single(
            sites,
            site => site.Key == TranslationHeldOutSite);
        Assert.Equal(
            DpoPairConstructionScope.ProductHeldOutEvaluation,
            heldOut.Scope);
        Assert.Contains("RowKind: \"HELDOUT\"", heldOut.ContextWindow);
        Assert.Contains("Admission: null", heldOut.ContextWindow);

        // Внешний runner остается историческим prohibited/non-product site. Он
        // counted для аудита, но не входит в legal product training allowance.
        var external = Assert.Single(
            sites,
            site => site.Key == ExternalExperimentSite);
        Assert.Equal(
            DpoPairConstructionScope.HistoricalProhibitedExperiment,
            external.Scope);
        Assert.StartsWith("experiments-v2/", external.RelativePath);

        Assert.Equal(
            DpoPairConstructionScope.CustomsAdmissionAuthority,
            Assert.Single(sites, site => site.Key == CustomsSite).Scope);
        Assert.Empty(forbiddenProductTrainingSites);
    }

    [Fact]
    public void SemanticGuard_FixtureDetectsEveryDirectDpoModelsPairConstructionShape()
    {
        // WHY: это C# внутри raw string, не настоящий xUnit-класс. Namespace и
        // public class здесь не нужны, зато не сбивают focused-manifest scanner.
        const string directFormsSource = """
            using DPO.Models;
            using PairAlias = DPO.Models.DpoPair;
            using Models = DPO.Models;

            internal sealed class DirectForms
            {
                public DpoPair Pair { get; } = new() { Prompt = "p" };

                public DpoPair ExplicitUnqualified() => new DpoPair { Prompt = "p" };

                public DPO.Models.DpoPair ExplicitQualified() => new DPO.Models.DpoPair { Prompt = "p" };

                public global::DPO.Models.DpoPair ExplicitGlobal() => new global::DPO.Models.DpoPair { Prompt = "p" };

                public PairAlias ExplicitAlias() => new PairAlias { Prompt = "p" };

                public Models.DpoPair ExplicitNamespaceAlias() => new Models.DpoPair { Prompt = "p" };

                public DpoPair TargetTypedReturn() => new() { Prompt = "p" };

                public DpoPair TargetTypedLocal()
                {
                    DpoPair pair = new() { Prompt = "p" };
                    return pair;
                }
            }
            """;
        var sources = new[]
        {
            new SourceFile(
                "fixtures/PoorPairGuardFixture/DirectForms.cs",
                directFormsSource)
        };

        var sites = FindDpoModelsPairConstructionSitesFromSources(sources);
        var statements = sites
            .Select(site => site.Statement)
            .OrderBy(statement => statement, StringComparer.Ordinal)
            .ToArray();

        // Self-fixture намеренно содержит запрещенные формы. Если одна форма не
        // найдена, real census мог бы молча пропустить новый product bypass.
        Assert.Equal(8, sites.Count);
        Assert.Contains(
            "public DpoPair Pair { get; } = new() { Prompt = \"p\" };",
            statements);
        Assert.Contains(
            "public DpoPair ExplicitUnqualified() => new DpoPair { Prompt = \"p\" };",
            statements);
        Assert.Contains(
            "public DPO.Models.DpoPair ExplicitQualified() => new DPO.Models.DpoPair { Prompt = \"p\" };",
            statements);
        Assert.Contains(
            "public global::DPO.Models.DpoPair ExplicitGlobal() => new global::DPO.Models.DpoPair { Prompt = \"p\" };",
            statements);
        Assert.Contains(
            "public PairAlias ExplicitAlias() => new PairAlias { Prompt = \"p\" };",
            statements);
        Assert.Contains(
            "public Models.DpoPair ExplicitNamespaceAlias() => new Models.DpoPair { Prompt = \"p\" };",
            statements);
        Assert.Contains(
            "public DpoPair TargetTypedReturn() => new() { Prompt = \"p\" };",
            statements);
        Assert.Contains(
            "DpoPair pair = new() { Prompt = \"p\" };",
            statements);
    }

    [Fact]
    public void SemanticGuard_FixtureDetectsCrossFileGlobalAliasTargetTypedCreation()
    {
        const string globalAliasSource = """
            global using PairAlias = DPO.Models.DpoPair;
            """;
        // WHY: product file специально не содержит текст "DpoPair". Так guard
        // доказывает, что target-typed new() найден через cross-file global alias.
        const string productSourceWithoutDpoPairText = """
            internal sealed class ProductSource
            {
                public PairAlias Create()
                {
                    PairAlias pair = new() { Prompt = "p" };
                    return pair;
                }
            }
            """;
        var sources = new[]
        {
            new SourceFile(
                "fixtures/PoorPairGuardFixture/GlobalAliases.cs",
                globalAliasSource),
            new SourceFile(
                "fixtures/PoorPairGuardFixture/ProductSource.cs",
                productSourceWithoutDpoPairText)
        };

        var site = Assert.Single(
            FindDpoModelsPairConstructionSitesFromSources(sources));

        // WHY: product source здесь не содержит текст "DpoPair". Guard обязан
        // видеть global using alias из соседнего файла через общий compilation,
        // иначе target-typed new() становится bypass-ом.
        Assert.DoesNotContain(
            "DpoPair",
            productSourceWithoutDpoPairText,
            StringComparison.Ordinal);
        Assert.Equal(
            "fixtures/PoorPairGuardFixture/ProductSource.cs",
            site.RelativePath);
        Assert.Equal(
            "PairAlias pair = new() { Prompt = \"p\" };",
            site.Statement);
    }

    private static IReadOnlyList<DpoPairConstructionSite>
        FindDpoModelsPairConstructionSites(
            [CallerFilePath] string callerFilePath = "")
    {
        if (cachedRepoCensus is not null)
            return cachedRepoCensus;

        lock (RepoCensusLock)
        {
            if (cachedRepoCensus is not null)
                return cachedRepoCensus;

            cachedRepoCensus = BuildDpoModelsPairConstructionSites(
                callerFilePath);
            return cachedRepoCensus;
        }
    }

    private static IReadOnlyList<DpoPairConstructionSite>
        BuildDpoModelsPairConstructionSites(string callerFilePath)
    {
        var repoRoot = ResolveRepoRoot(callerFilePath);
        var sources = EnumerateScannedSourceFiles(repoRoot)
            .OrderBy(file => file, StringComparer.Ordinal)
            .Select(file => SourceFile.FromFile(repoRoot, file))
            .ToArray();

        return FindDpoModelsPairConstructionSitesFromSources(sources);
    }

    private static string ResolveRepoRoot(string callerFilePath)
    {
        var callerDirectory = Path.GetDirectoryName(callerFilePath);
        var candidates = new[]
        {
            string.IsNullOrWhiteSpace(callerDirectory)
                ? ""
                : Path.GetFullPath(Path.Combine(callerDirectory, "..")),
            Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                ".."))
        };

        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
                continue;
            if (Directory.Exists(Path.Combine(candidate, "11.DPO")) &&
                Directory.Exists(Path.Combine(candidate, "Tests")))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException(
            "Roslyn guard cannot resolve repository root for source census.");
    }

    private static IReadOnlyList<DpoPairConstructionSite>
        FindDpoModelsPairConstructionSitesFromSources(
            IReadOnlyList<SourceFile> sources)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(
            LanguageVersion.Preview);
        var syntaxTreesBySource = sources
            .OrderBy(source => source.RelativePath, StringComparer.Ordinal)
            .Select(source => new
            {
                Source = source,
                Tree = CSharpSyntaxTree.ParseText(
                    SourceText.From(source.Text, Encoding.UTF8),
                    parseOptions,
                    path: source.RelativePath)
            })
            .ToArray();
        var sourceByTreePath = syntaxTreesBySource.ToDictionary(
            item => item.Tree.FilePath,
            item => item.Source,
            StringComparer.Ordinal);
        var metadataReferences = CreateMetadataReferences(sources).ToArray();
        var compilationOptions =
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithNullableContextOptions(NullableContextOptions.Enable);
        var compilation = CSharpCompilation.Create(
            "PoorDpoPairConstructionGuard",
            syntaxTreesBySource.Select(item => item.Tree),
            metadataReferences,
            compilationOptions);
        var targetSymbol = compilation.GetTypeByMetadataName("DPO.Models.DpoPair")
                           ?? throw new InvalidOperationException(
                               "Roslyn guard cannot resolve DPO.Models.DpoPair.");
        var targetAliases = ResolveDpoPairAliases(
            syntaxTreesBySource.Select(item => item.Tree.GetRoot()));
        var sites = new List<DpoPairConstructionSite>();

        foreach (var syntaxTree in syntaxTreesBySource
                     .Select(item => item.Tree)
                     .OrderBy(tree => tree.FilePath, StringComparer.Ordinal))
        {
            var source = sourceByTreePath[syntaxTree.FilePath];
            var semanticModel = compilation.GetSemanticModel(
                syntaxTree,
                ignoreAccessibility: true);
            var root = syntaxTree.GetRoot();
            foreach (var creation in root
                         .DescendantNodes()
                         .OfType<BaseObjectCreationExpressionSyntax>()
                         .OrderBy(node => node.SpanStart))
            {
                // WHY: внутри candidate file не остается syntax allowlist.
                // Любая object creation форма проходит через semantic model,
                // чтобы namespace alias/property initializer не выпадали из census.
                if (!IsDpoModelsPairConstruction(
                        source,
                        semanticModel,
                        targetSymbol,
                        targetAliases,
                        creation))
                {
                    continue;
                }

                var lineIndex = creation.GetLocation()
                    .GetLineSpan()
                    .StartLinePosition
                    .Line;
                var context = ResolveSymbolContext(
                    source.RelativePath,
                    semanticModel,
                    creation);
                var statement = NormalizeStatement(source.Lines[lineIndex]);
                sites.Add(new DpoPairConstructionSite(
                    source.RelativePath,
                    context,
                    statement,
                    BuildContextWindow(source.Lines, lineIndex),
                    ClassifyScope(source.RelativePath, context, statement)));
            }
        }

        return sites
            .OrderBy(site => site.Key, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool IsDpoModelsPairConstruction(
        SourceFile source,
        SemanticModel semanticModel,
        INamedTypeSymbol targetSymbol,
        DpoPairAliases targetAliases,
        BaseObjectCreationExpressionSyntax creation)
    {
        var typeInfo = semanticModel.GetTypeInfo(creation);
        if (IsTargetType(typeInfo.Type, targetSymbol) ||
            IsTargetType(typeInfo.ConvertedType, targetSymbol))
        {
            return true;
        }

        if (LooksLikeDpoPairCreationSyntax(creation, targetAliases) &&
            HasMissingOrErrorType(typeInfo))
        {
            var line = creation.GetLocation()
                .GetLineSpan()
                .StartLinePosition
                .Line + 1;
            throw new InvalidOperationException(
                "Roslyn guard saw DPO-like object creation but semantic " +
                "resolution returned null/error type at " +
                $"{source.RelativePath}:{line}. This must not become a false negative.");
        }

        return false;
    }

    private static bool IsTargetType(
        ITypeSymbol? actualType,
        INamedTypeSymbol targetSymbol)
        => actualType is not null &&
           (SymbolEqualityComparer.Default.Equals(actualType, targetSymbol) ||
            string.Equals(
                actualType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                "global::DPO.Models.DpoPair",
                StringComparison.Ordinal));

    private static bool HasMissingOrErrorType(TypeInfo typeInfo)
        => IsMissingOrErrorType(typeInfo.Type) &&
           IsMissingOrErrorType(typeInfo.ConvertedType);

    private static bool IsMissingOrErrorType(ITypeSymbol? symbol)
        => symbol is null ||
           symbol is IErrorTypeSymbol ||
           symbol.Kind == SymbolKind.ErrorType;

    private static DpoPairAliases ResolveDpoPairAliases(
        IEnumerable<SyntaxNode> roots)
    {
        var typeAliases = new HashSet<string>(StringComparer.Ordinal);
        var namespaceAliases = new HashSet<string>(StringComparer.Ordinal);

        foreach (var usingDirective in roots
                     .SelectMany(root => root.DescendantNodes())
                     .OfType<UsingDirectiveSyntax>())
        {
            if (usingDirective.Alias is null || usingDirective.Name is null)
                continue;

            var alias = usingDirective.Alias.Name.Identifier.ValueText;
            var target = usingDirective.Name.ToString().Trim();
            if (IsDpoPairTypeName(target))
            {
                typeAliases.Add(alias);
            }
            else if (IsDpoModelsNamespaceName(target))
            {
                namespaceAliases.Add(alias);
            }
        }

        return new DpoPairAliases(typeAliases, namespaceAliases);
    }

    private static bool LooksLikeDpoPairCreationSyntax(
        BaseObjectCreationExpressionSyntax creation,
        DpoPairAliases targetAliases)
    {
        if (creation is ObjectCreationExpressionSyntax objectCreation)
            return IsDpoPairLikeTypeName(
                objectCreation.Type.ToString(),
                targetAliases);

        return TryGetImplicitCreationTargetTypeName(creation, out var typeName) &&
               IsDpoPairLikeTypeName(typeName, targetAliases);
    }

    private static bool TryGetImplicitCreationTargetTypeName(
        SyntaxNode creation,
        out string typeName)
    {
        typeName = "";
        if (creation.Parent is EqualsValueClauseSyntax
            {
                Parent: VariableDeclaratorSyntax
                {
                    Parent: VariableDeclarationSyntax variableDeclaration
                }
            })
        {
            typeName = variableDeclaration.Type.ToString();
            return true;
        }

        if (creation.Parent is EqualsValueClauseSyntax
            {
                Parent: PropertyDeclarationSyntax property
            })
        {
            typeName = property.Type.ToString();
            return true;
        }

        if (creation.Parent is ArrowExpressionClauseSyntax
            {
                Parent: MethodDeclarationSyntax method
            })
        {
            typeName = method.ReturnType.ToString();
            return true;
        }

        if (creation.Parent is ReturnStatementSyntax)
        {
            var containingMethod = creation.Ancestors()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault();
            if (containingMethod is null)
                return false;

            typeName = containingMethod.ReturnType.ToString();
            return true;
        }

        return false;
    }

    private static bool IsDpoPairLikeTypeName(
        string typeName,
        DpoPairAliases targetAliases)
    {
        var normalized = typeName.Trim();
        return IsDpoPairTypeName(normalized) ||
               targetAliases.TypeAliases.Contains(normalized) ||
               targetAliases.NamespaceAliases.Any(alias =>
                   string.Equals(
                       normalized,
                       alias + ".DpoPair",
                       StringComparison.Ordinal));
    }

    private static bool IsDpoPairTypeName(string typeName)
        => string.Equals(typeName, "DpoPair", StringComparison.Ordinal) ||
           string.Equals(typeName, "DPO.Models.DpoPair", StringComparison.Ordinal) ||
           string.Equals(typeName, "global::DPO.Models.DpoPair", StringComparison.Ordinal);

    private static bool IsDpoModelsNamespaceName(string typeName)
        => string.Equals(typeName, "DPO.Models", StringComparison.Ordinal) ||
           string.Equals(typeName, "global::DPO.Models", StringComparison.Ordinal);

    private static IEnumerable<string> EnumerateScannedSourceFiles(string repoRoot)
    {
        var pendingDirectories = new Stack<string>();
        pendingDirectories.Push(repoRoot);

        while (pendingDirectories.Count > 0)
        {
            var directory = pendingDirectories.Pop();
            foreach (var file in Directory.EnumerateFiles(directory, "*.cs"))
            {
                if (IsScannedPath(repoRoot, file))
                    yield return file;
            }

            foreach (var childDirectory in Directory
                         .EnumerateDirectories(directory)
                         .OrderByDescending(path => path, StringComparer.Ordinal))
            {
                if (File.GetAttributes(childDirectory).HasFlag(FileAttributes.ReparsePoint))
                    continue;
                if (IsScannedPath(repoRoot, childDirectory))
                    pendingDirectories.Push(childDirectory);
            }
        }
    }

    private static IEnumerable<MetadataReference> CreateMetadataReferences(
        IReadOnlyList<SourceFile> sources)
    {
        var trustedPlatformAssemblies =
            (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        if (string.IsNullOrWhiteSpace(trustedPlatformAssemblies))
            return [];

        var includeDpoAssembly = !sources.Any(source =>
            string.Equals(
                source.RelativePath,
                "11.DPO/Models/DpoPair.cs",
                StringComparison.Ordinal));
        var productReferences = includeDpoAssembly
            ? [typeof(DPO.Models.DpoPair).Assembly.Location]
            : Array.Empty<string>();

        var referencePaths = trustedPlatformAssemblies
            .Split(Path.PathSeparator)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Concat(productReferences)
            .Where(File.Exists)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return referencePaths
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();
    }

    private static bool IsScannedPath(string repoRoot, string path)
    {
        var relativePath = NormalizeRelativePath(repoRoot, path);
        var segments = relativePath.Split('/');
        return !relativePath.StartsWith("Tests/", StringComparison.Ordinal)
               && !relativePath.StartsWith("Tests.", StringComparison.Ordinal)
               && !segments.Contains("Tests", StringComparer.Ordinal)
               && !segments.Contains("bin", StringComparer.OrdinalIgnoreCase)
               && !segments.Contains("obj", StringComparer.OrdinalIgnoreCase)
               && !segments.Contains("TestResults", StringComparer.OrdinalIgnoreCase)
               && !segments.Contains(".git", StringComparer.OrdinalIgnoreCase)
               && !segments.Contains("artifacts", StringComparer.OrdinalIgnoreCase)
               && !segments.Contains("docs", StringComparer.OrdinalIgnoreCase)
               && !segments.Contains("evidence", StringComparer.OrdinalIgnoreCase);
    }

    private static string ResolveSymbolContext(
        string relativePath,
        SemanticModel semanticModel,
        SyntaxNode creation)
    {
        if (relativePath == "11.DPO/Models/DpoTrainerPairAdmission.cs")
            return "DpoTrainerPairCustoms.CreateAdmitted";

        if (relativePath == "27.TranslationDomain/TranslationDpoTrainerBridge.cs")
            return "TranslationDpoTextFeaturizer.BuildRecord(TranslationDpoHeldOutEvaluationRecord)";

        if (relativePath ==
            "experiments-v2/INTERNAL-TRAIN-READY-V191/runner/InternalTrainReadyDpoRunner/Program.cs")
        {
            return "InternalTrainReadyDpoRunner.top-level";
        }

        if (semanticModel.GetEnclosingSymbol(creation.SpanStart) is IMethodSymbol method)
        {
            var typePrefix = method.ContainingType is null
                ? ""
                : method.ContainingType.Name + ".";
            var parameters = method.Parameters.Length == 0
                ? ""
                : "(" + string.Join(
                    ", ",
                    method.Parameters.Select(parameter => parameter.Type.Name)) + ")";
            return typePrefix + method.Name + parameters;
        }

        return "unknown-context";
    }

    private static DpoPairConstructionScope ClassifyScope(
        string relativePath,
        string context,
        string statement)
    {
        var key = string.Join("|", relativePath, context, statement);
        if (key == CustomsSite)
            return DpoPairConstructionScope.CustomsAdmissionAuthority;
        if (key == TranslationHeldOutSite)
            return DpoPairConstructionScope.ProductHeldOutEvaluation;
        if (relativePath.StartsWith("experiments-v2/", StringComparison.Ordinal))
            return DpoPairConstructionScope.HistoricalProhibitedExperiment;

        return DpoPairConstructionScope.ProductTraining;
    }

    private static string BuildContextWindow(string[] lines, int lineIndex)
    {
        var start = Math.Max(0, lineIndex - 12);
        var end = Math.Min(lines.Length - 1, lineIndex + 12);
        return string.Join("\n", lines[start..(end + 1)]);
    }

    private static string NormalizeRelativePath(string repoRoot, string file)
        => Path.GetRelativePath(repoRoot, file).Replace('\\', '/');

    private static string NormalizeStatement(string line)
        => string.Join(
            " ",
            line.Trim().Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries));

    private enum DpoPairConstructionScope
    {
        CustomsAdmissionAuthority,
        ProductHeldOutEvaluation,
        HistoricalProhibitedExperiment,
        ProductTraining
    }

    private sealed record SourceFile(string RelativePath, string Text)
    {
        public string[] Lines => Text.Replace("\r\n", "\n").Split('\n');

        public static SourceFile FromFile(string repoRoot, string file)
            => new(NormalizeRelativePath(repoRoot, file), File.ReadAllText(file));
    }

    private sealed record DpoPairAliases(
        IReadOnlySet<string> TypeAliases,
        IReadOnlySet<string> NamespaceAliases);

    private sealed record DpoPairConstructionSite(
        string RelativePath,
        string SymbolContext,
        string Statement,
        string ContextWindow,
        DpoPairConstructionScope Scope)
    {
        public string Key => string.Join(
            "|",
            RelativePath,
            SymbolContext,
            Statement);
    }
}
