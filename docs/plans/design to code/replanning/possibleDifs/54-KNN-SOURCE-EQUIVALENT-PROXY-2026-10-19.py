#!/usr/bin/env python3
"""Independent, read-only source-equivalent sanity proxy for XIIGEN N54.

NOT a native .NET test, DPO training run, or product-quality evaluation.
Mirrors the integer-hash arithmetic and the dictionary/index alias mechanism
visible in 21.AgentInfrastructure/Logical/KnnLogicalUnit.cs.
"""
import json


def unchecked_csharp_hash(s: str) -> int:
    h = 23
    for char in s:
        h = (h * 31 + ord(char)) & 0xFFFFFFFF
    return h - 0x100000000 if h & 0x80000000 else h


def alias_demo():
    # Index retains every insertion, while dictionary overwrites on duplicate ID.
    rows = [
        {"id": "OP-01", "input": "x++", "output": "returns-old-value"},
        {"id": "OP-01", "input": "++x", "output": "returns-new-value"},
    ]
    index = []
    examples = {}
    for row in rows:
        trimmed_id = row["id"].strip()
        examples[trimmed_id] = row
        index.append({"id": trimmed_id, "input": row["input"]})
    # This is an explicit hypothetical hit for the first stored vector,
    # not a measurement of native HNSW neighbor ordering.
    first_hit = index[0]
    materialized = examples[first_hit["id"]]["output"]
    return {
        "index_insertions": len(index),
        "unique_dictionary_entries": len(examples),
        "hypothetical_first_hit": first_hit,
        "expected_first_row_output": rows[0]["output"],
        "materialized_output_after_alias": materialized,
        "output_mismatch_conditional_on_first_hit": materialized != rows[0]["output"],
    }


def main():
    witness = "ZSJBINM"
    signed = unchecked_csharp_hash(witness)
    assert signed == -2147483648, (witness, signed)
    assert unchecked_csharp_hash("ZSJBINN") != -2147483648
    alias = alias_demo()
    assert alias["index_insertions"] == 2
    assert alias["unique_dictionary_entries"] == 1
    assert alias["output_mismatch_conditional_on_first_hit"]
    result = {
        "id": "XIIGEN-N54-PROXY",
        "as_of": "2026-10-19",
        "status": "SOURCE_EQUIVALENT_PROXY_ONLY",
        "native_dotnet_executed": False,
        "learning_model_trained": False,
        "product_semantic_gain_measured": False,
        "hash_witness": {
            "predicate_id": witness,
            "csharp_unchecked_signed_int32_hash": signed,
            "unsigned_hex": "0x80000000",
            "dotnet_math_abs_int32_min_would_throw": True,
            "control_predicate_id": "ZSJBINN",
            "control_signed_hash": unchecked_csharp_hash("ZSJBINN"),
        },
        "id_alias": alias,
        "metadata_type_hypothesis": "SOURCE_ONLY: inference accepts exactly string[], whereas direct caller could pass List<string>; not executed",
    }
    print(json.dumps(result, ensure_ascii=False, indent=2, sort_keys=True))


if __name__ == "__main__":
    main()
