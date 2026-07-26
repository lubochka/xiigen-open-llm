// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler;

// TemplateBeautifier stores code snippets annotated with predicate sequences.
// E23: +833% beautification_accuracy — K-NN template retrieval on predicate sets.
public static class TemplateBeautifier
{
    public const string CSHARP     = "CSharp";
    public const string TYPESCRIPT = "TypeScript";
    public const string GENERIC    = "Generic";

    public record Template(
        int[]  Predicates,
        string Stack,
        string Code,
        string Description);

    public static readonly IReadOnlyList<Template> All = new[]
    {
        // ── C# patterns ────────────────────────────────────────────────────────
        new Template(
            new[] { LogicExtractor.NULL_GUARD, LogicExtractor.FAIL },
            CSHARP,
            "if ({VAR} == null)\n    return OperationResult<{TYPE}>.Fail(\"{VAR} cannot be null\");",
            "Null guard → Fail"),

        new Template(
            new[] { LogicExtractor.NULL_GUARD, LogicExtractor.FAIL, LogicExtractor.SUCCESS },
            CSHARP,
            "if ({VAR} == null)\n    return OperationResult<{TYPE}>.Fail(\"not found\");\n// ...\nreturn OperationResult<{TYPE}>.Ok(result);",
            "Null guard → Fail | Success"),

        new Template(
            new[] { LogicExtractor.VALIDATE_OP, LogicExtractor.FAIL },
            CSHARP,
            "var valid = _validator.Validate({VAR});\nif (!valid.IsSuccess)\n    return OperationResult<{TYPE}>.Fail(valid.ErrorMessage);",
            "Validate → Fail"),

        new Template(
            new[] { LogicExtractor.VALIDATE_OP, LogicExtractor.FAIL, LogicExtractor.SUCCESS },
            CSHARP,
            "var valid = _validator.Validate({VAR});\nif (!valid.IsSuccess)\n    return OperationResult<{TYPE}>.Fail(valid.ErrorMessage);\nreturn OperationResult<{TYPE}>.Ok(result);",
            "Validate → Fail | Success"),

        new Template(
            new[] { LogicExtractor.ITERATE, LogicExtractor.GET_CALL },
            CSHARP,
            "foreach (var item in {COLLECTION})\n{\n    var result = Get{ITEM}(item.Id);\n    if (!result.IsSuccess) continue;\n}",
            "Iterate → Get"),

        new Template(
            new[] { LogicExtractor.ITERATE, LogicExtractor.ASSIGN },
            CSHARP,
            "foreach (var item in {COLLECTION})\n    {RESULT}.Add(item.{FIELD});",
            "Iterate → Assign"),

        new Template(
            new[] { LogicExtractor.THROW_OP },
            CSHARP,
            "throw new InvalidOperationException(\"{MESSAGE}\");",
            "Throw exception"),

        new Template(
            new[] { LogicExtractor.HANDLE_ERROR, LogicExtractor.LOG_OP },
            CSHARP,
            "try\n{\n    // ...\n}\ncatch (Exception ex)\n{\n    _logger.LogError(ex, \"{MESSAGE}\");\n    return OperationResult<{TYPE}>.Fail(ex.Message);\n}",
            "Try-catch → Log → Fail"),

        new Template(
            new[] { LogicExtractor.AWAIT, LogicExtractor.GET_CALL },
            CSHARP,
            "var result = await Get{ITEM}Async({VAR});\nif (!result.IsSuccess)\n    return OperationResult<{TYPE}>.Fail(result.ErrorMessage);",
            "Await → Get"),

        // ── TypeScript / NestJS patterns ────────────────────────────────────────
        new Template(
            new[] { LogicExtractor.NULL_GUARD, LogicExtractor.FAIL },
            TYPESCRIPT,
            "if (!{VAR}) {\n  throw new NotFoundException(`{VAR} not found`);\n}",
            "NestJS null guard → NotFoundException"),

        new Template(
            new[] { LogicExtractor.VALIDATE_OP, LogicExtractor.FAIL },
            TYPESCRIPT,
            "const errors = await this.validator.validate({VAR});\nif (errors.length > 0) {\n  throw new BadRequestException(errors);\n}",
            "NestJS validate → BadRequest"),

        new Template(
            new[] { LogicExtractor.ITERATE, LogicExtractor.GET_CALL },
            TYPESCRIPT,
            "const results = await Promise.all(\n  {COLLECTION}.map(item => this.get{ITEM}(item.id))\n);",
            "NestJS iterate → Promise.all"),

        // ── Generic (Python-style) patterns ───────────────────────────────────
        new Template(
            new[] { LogicExtractor.NULL_GUARD, LogicExtractor.FAIL },
            GENERIC,
            "if {VAR} is None:\n    raise ValueError(f\"{VAR} cannot be None\")",
            "Python null guard"),

        new Template(
            new[] { LogicExtractor.ITERATE, LogicExtractor.TRANSFORM },
            GENERIC,
            "{RESULT} = [{TRANSFORM}(item) for item in {COLLECTION}]",
            "Python list comprehension"),
    }.AsReadOnly();

    public static IReadOnlyList<Template> ForStack(string stack)
        => All.Where(t => t.Stack == stack || t.Stack == GENERIC).ToList().AsReadOnly();

    // K-NN retrieval: find the template whose predicate set is closest to the query
    public static Template? FindBest(IReadOnlyList<int> predicates, string stack, int k = 3)
    {
        var candidates = ForStack(stack);
        if (candidates.Count == 0) return null;

        var querySet = new HashSet<int>(predicates);
        return candidates
            .OrderBy(t =>
            {
                var tSet  = new HashSet<int>(t.Predicates);
                int inter = querySet.Count(x => tSet.Contains(x));
                int union = querySet.Count + tSet.Count - inter;
                return union == 0 ? 0.0 : 1.0 - (double)inter / union;
            })
            .First();
    }
}
