// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tokenizer;

namespace ExperimentsV2;

public static class InputAdapters
{
    public static string ForE01(DatasetContents d) => d.CombinedText;
    public static (string Combined, IReadOnlyList<string> AllSources) ForE05(DatasetContents d)
        => (d.CombinedText, d.Files.Select(f => f.Text).ToList().AsReadOnly());

    public static IReadOnlyList<string> ForE02(DatasetContents d)
        => d.Files.Select(f => f.Text).ToList().AsReadOnly();

    public static OperationResult<IReadOnlyList<int>> ForE03(DatasetContents d)
    {
        if (string.IsNullOrWhiteSpace(d.CombinedText))
            return OperationResult<IReadOnlyList<int>>.Fail("Пустой текст для E03");

        // Char-level ASCII tokenization (vocab=128).
        // BPE requires vocabSize≥256; at 512 the trigram table (512³=134M) is unreachable
        // at any realistic corpus size.  With 128 chars the trigram table is only 2M —
        // feasible at Tier 3 (410K tokens).
        var tokens = d.CombinedText
            .Select(c => c < 128 ? (int)c : 127)
            .ToList();

        return tokens.Count < 10
            ? OperationResult<IReadOnlyList<int>>.Fail("Слишком мало токенов для E03")
            : OperationResult<IReadOnlyList<int>>.Ok(tokens.AsReadOnly());
    }

    public static OperationResult<(
        IReadOnlyList<(string Id, string Source)> Documents,
        IReadOnlyList<(string Query, string ExpectedId)> Queries)>
        ForE04(DatasetContents d)
    {
        if (d.FileCount < 20)
            return OperationResult<(IReadOnlyList<(string, string)>,
                                    IReadOnlyList<(string, string)>)>
                .Fail($"E04 требует Tier 2+ (>= 20 файлов). Сейчас Tier {d.Tier} = {d.FileCount} файлов. " +
                      $"Запустите снова с tier: 2.");

        var documents = d.Files
            .Select(f => (Path.GetFileNameWithoutExtension(f.Path), f.Text))
            .ToList();

        var queries = BuildCrossFileQueries(d.Files);

        if (queries.Count == 0)
            return OperationResult<(IReadOnlyList<(string, string)>,
                                    IReadOnlyList<(string, string)>)>
                .Fail("Не найдено кросс-файловых вызовов. E04 нужны зависимости между файлами.");

        return OperationResult<(IReadOnlyList<(string, string)>,
                                IReadOnlyList<(string, string)>)>
            .Ok((documents.AsReadOnly(), queries.AsReadOnly()));
    }

    public static OperationResult<IReadOnlyList<(string Caller, string Callee)>>
        ForE06(DatasetContents d)
    {
        var pairs = new List<(string, string)>();

        foreach (var (_, source) in d.Files)
        {
            var root = CSharpSyntaxTree.ParseText(source).GetRoot();
            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                string caller = method.Identifier.Text;
                foreach (var inv in method.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    string callee = inv.Expression switch
                    {
                        MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
                        IdentifierNameSyntax id         => id.Identifier.Text,
                        _                               => string.Empty
                    };
                    if (!string.IsNullOrEmpty(callee) && callee != caller)
                        pairs.Add((caller, callee));
                }
            }
        }

        return pairs.Count == 0
            ? OperationResult<IReadOnlyList<(string, string)>>.Fail("Граф вызовов пуст")
            : OperationResult<IReadOnlyList<(string, string)>>.Ok(pairs.AsReadOnly());
    }

    private static List<(string Query, string ExpectedId)> BuildCrossFileQueries(
        IReadOnlyList<(string Path, string Text)> files)
    {
        var defs = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (path, src) in files)
        {
            string fileId = Path.GetFileNameWithoutExtension(path);
            var root = CSharpSyntaxTree.ParseText(src).GetRoot();
            foreach (var m in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
                if (m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)))
                    defs.TryAdd(m.Identifier.Text, fileId);
        }

        var queries = new List<(string, string)>();
        foreach (var (path, src) in files)
        {
            string currentId = Path.GetFileNameWithoutExtension(path);
            var root = CSharpSyntaxTree.ParseText(src).GetRoot();
            foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                string callee = inv.Expression switch
                {
                    MemberAccessExpressionSyntax ma => ma.Name.Identifier.Text,
                    IdentifierNameSyntax id         => id.Identifier.Text,
                    _                               => string.Empty
                };
                if (defs.TryGetValue(callee, out string? defFile) && defFile != currentId)
                {
                    var entry = (callee, defFile);
                    if (!queries.Contains(entry)) queries.Add(entry);
                }
            }
        }
        return queries;
    }
}
