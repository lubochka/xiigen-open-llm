#!/usr/bin/env python3
"""DesignToCode-clean N54 source-equivalent counterexamples; NOT native .NET,
NOT model training, NOT a visual quality result. Python 3 standard library.
C# references: DesignConversionMachine.cs:2937-3005;
DesignConversionTrainedUnits.cs:298-353;
DesignConversionDpoDatasetBuilder.cs:553-586,1231-1260,1308-1380.
"""
import json
import re
from pathlib import Path

BLOCKED = {
    "id", "identifier", "identity", "ref", "reference", "hash", "digest",
    "checksum", "sha", "sha256", "path", "filepath", "filename", "file",
    "uri", "url", "manifest", "checkpoint", "artifact", "output", "result",
    "reviewer", "score", "verdict", "accepted", "rejected", "passed",
    "failed", "approval",
}

def key_tokens(key):
    # Equivalent for the provided ASCII/CamelCase test keys; not a general C# port.
    return [w.lower() for w in re.findall(r'[A-Z]+(?=[A-Z][a-z]|[^a-z]|$)|[A-Z]?[a-z]+|\d+', key)]

def sanitize(node):
    if isinstance(node, dict):
        return {k: sanitize(v) for k, v in node.items()
                if not any(t in BLOCKED for t in key_tokens(k))}
    if isinstance(node, list):
        return [sanitize(x) for x in node]
    return node

def choose_rules(available, preference_weights):
    ranked = sorted(
        [(rule, weight) for rule, weight in preference_weights.items()
         if rule in available and weight > 0],
        key=lambda z: (-z[1], z[0]),
    )
    return [ranked[0][0]] if ranked else sorted(set(available))

def machine_margin(chosen, rejected):
    if "visual_score" in chosen and "visual_score" in rejected:
        return round(chosen["visual_score"] - rejected["visual_score"], 6)
    return None if chosen == rejected else 1.0

available = ["contrast", "overflow"]
rules = {
    "no_learned_weights": choose_rules(available, {}),
    "learned_contrast_only": choose_rules(available, {"contrast": 0.8}),
    "learned_contrast_and_overflow": choose_rules(
        available, {"contrast": 0.8, "overflow": 0.9}),
    "nonpositive_weights_control": choose_rules(
        available, {"contrast": 0.0, "overflow": -0.3}),
    "single_defect_control": choose_rules(["contrast"], {"contrast": 0.8}),
}

source_a = {"view": {"layout": {"parentRef": "panel-A",
                               "direction": "vertical"}, "ariaLabel": "Save"}}
source_b = {"view": {"layout": {"parentRef": "panel-B",
                               "direction": "vertical"}, "ariaLabel": "Save"}}
san_a, san_b = sanitize(source_a), sanitize(source_b)
vec_a = [0.1] * 16 + [0.0, 1.0]
vec_b = [0.1] * 16 + [1.0, 1.0]
body_a = {"html": "<a/>", "css": "a{}"}
body_b = {"html": "<b/>", "css": "b{}"}


def local_source_feature_deltas(prompt, chosen, rejected):
    # Restricted to canonical {html,css} when no correction_category fields exist.
    if chosen == rejected:
        return {}
    vector = prompt["internal_vector"]
    return {
        "internal_vector:mean": round(sum(vector) / len(vector), 6),
        "internal_vector:dimension": len(vector),
        "source_kind:" + prompt["source_kind"]: 1.0,
    }

probe_prompt = {
    "unit_id": "design_conversion_runtime_scaffold_improver",
    "source_kind": "figma",
    "internal_vector": [0.1] * 16,
}
feature_deltas_forward = local_source_feature_deltas(probe_prompt, body_a, body_b)
feature_deltas_reversed = local_source_feature_deltas(probe_prompt, body_b, body_a)

result = {
    "scope": "source_equivalent_toy_proxy",
    "native_dotnet_executed": False,
    "trained_model_executed": False,
    "real_quality_measured": False,
    "N54_00_pair_margin_contract": {
        "gold_strong": machine_margin(
            {**body_a, "visual_score": 0.9},
            {**body_b, "visual_score": 0.1}),
        "gold_inverted": machine_margin(
            {**body_a, "visual_score": 0.1},
            {**body_b, "visual_score": 0.9}),
        "current_strong": machine_margin(body_a, body_b),
        "current_inverted": machine_margin(body_a, body_b),
        "current_identical_control": machine_margin(body_a, body_a),
    },
    "N54_00B_swapped_preference_feature_deltas": {
        "forward": feature_deltas_forward,
        "reversed": feature_deltas_reversed,
        "identical_under_swap": feature_deltas_forward == feature_deltas_reversed,
    },
    "N54_01_multidefect_rules": {
        k: {"selected": v,
            "available_count": 1 if k == "single_defect_control" else 2,
            "selected_count": len(v)}
        for k, v in rules.items()
    },
    "N54_02_parent_ref": {
        "input_a": source_a, "input_b": source_b,
        "sanitized_a": san_a, "sanitized_b": san_b,
        "different_inputs": source_a != source_b,
        "same_sanitized_source_structure": san_a == san_b,
    },
    "N54_03_vector_prefix": {
        "vector_dim": len(vec_a), "prefix_cap": 16,
        "different_full_vectors": vec_a != vec_b,
        "same_visible_prefix": vec_a[:16] == vec_b[:16],
    },
}
assert result["N54_00_pair_margin_contract"]["gold_strong"] == 0.8
assert result["N54_00_pair_margin_contract"]["gold_inverted"] == -0.8
assert result["N54_00_pair_margin_contract"]["current_strong"] == 1.0
assert feature_deltas_forward == feature_deltas_reversed
assert len(rules["no_learned_weights"]) == 2
assert len(rules["learned_contrast_only"]) == 1
assert result["N54_02_parent_ref"]["same_sanitized_source_structure"]
assert result["N54_03_vector_prefix"]["same_visible_prefix"]
print(json.dumps(result, ensure_ascii=False, indent=2))
