namespace ExperimentsV2;

// BeautifierTemplateLibrary stores code snippets annotated with the predicate
// sequences they implement. Used by E23 for K-NN template retrieval.
public static class BeautifierTemplateLibrary
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
        // ── C# patterns ──────────────────────────────────────────────────────
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

        // ── TypeScript / NestJS patterns ──────────────────────────────────────
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

        // ── Generic (Python-style) patterns ────────────────────────────────────
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
}
