// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OssEquivalents;

// Validates a Dictionary<string,object> against a declared schema dictionary.
// Schema format: { "fieldName": "string" | "number" | "boolean" | "object" | "array" }
// A "?" suffix on the type marks the field as optional (e.g. "string?").
// This is the .NET equivalent of zod from the OSS plan.
public sealed class SchemaValidator
{
    // Validate the given data against the expected schema.
    public OperationResult<bool> Validate(
        Dictionary<string, object>? data,
        Dictionary<string, object>  schema)
    {
        if (data is null)
            return OperationResult<bool>.Fail("Data is null.", "NULL_DATA");

        foreach (var (field, typeSpec) in schema)
        {
            string typeName = typeSpec?.ToString() ?? "";
            bool   optional = typeName.EndsWith('?');
            string baseType = optional ? typeName[..^1] : typeName;

            if (!data.TryGetValue(field, out var value))
            {
                if (optional) continue;
                return OperationResult<bool>.Fail(
                    $"Required field '{field}' is missing.", "MISSING_FIELD");
            }

            if (!MatchesType(value, baseType))
                return OperationResult<bool>.Fail(
                    $"Field '{field}' expected type '{baseType}' but got '{value?.GetType().Name}'.",
                    "TYPE_MISMATCH");
        }

        return OperationResult<bool>.Ok(true);
    }

    // Check whether a value matches the declared type name.
    private static bool MatchesType(object? value, string typeName) => typeName switch
    {
        "string"  => value is string
                     || (value is System.Text.Json.JsonElement js
                         && js.ValueKind == System.Text.Json.JsonValueKind.String),
        "number"  => value is int or long or float or double or decimal
                     || (value is System.Text.Json.JsonElement je
                         && je.ValueKind == System.Text.Json.JsonValueKind.Number),
        "boolean" => value is bool
                     || (value is System.Text.Json.JsonElement jb
                         && (jb.ValueKind == System.Text.Json.JsonValueKind.True
                          || jb.ValueKind == System.Text.Json.JsonValueKind.False)),
        "array"   => value is System.Collections.IList,
        "object"  => value is Dictionary<string, object>
                     || (value is System.Text.Json.JsonElement jo
                         && jo.ValueKind == System.Text.Json.JsonValueKind.Object),
        _         => true   // unknown type — pass through
    };
}
