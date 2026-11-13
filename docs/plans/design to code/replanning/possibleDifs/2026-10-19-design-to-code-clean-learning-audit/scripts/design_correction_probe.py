#!/usr/bin/env python3
"""Read-only source-equivalent diagnostics, not native C# or a training run.

The fixture dictionaries are mechanical counterexamples. They are not
teacher-authored DPO and must never be admitted as a training corpus.
"""
import argparse
import hashlib
import json
import math
import re
from pathlib import Path


FIELDS = (
    "correction_category", "selected_correction_category", "correction_rule",
    "selected_correction_rule", "defect_category",
)
META_FIELDS = ("correction_category", "selected_correction_category")
ARRAY_FIELDS = ("correction_categories", "defect_categories")
ALLOWED_RULES = {"contrast", "spacing", "overflow", "typography", "generic-visual-defect"}
SOURCE_BLOBS = {
    "44.DesignConversionDomain/DesignConversionTrainedUnits.cs":
        "3ef9f245d70df2c6dee673f7b9af5a15b3584701",
    "44.DesignConversionDomain/DesignConversionMachine.cs":
        "947383043f9b40c352bdf28ab492febc2ed7ca5e",
}
BASELINE_MACHINE_BLOB = "bad9b74f153afdd00ca6c7d42e185e312b9634a3"


def normalize(signal, preserve_canonical=False):
    signal = signal or ""
    if preserve_canonical and signal in ALLOWED_RULES:
        return signal
    for rule, tokens in (
        ("contrast", ("contrast", "color", "foreground", "background")),
        ("spacing", ("spacing", "gap", "padding", "margin", "crowded")),
        ("overflow", ("overflow", "clip", "clipping", "wrap", "truncat")),
        ("typography", ("font", "text", "typography", "readability")),
    ):
        if signal.strip() and any(token in signal.lower() for token in tokens):
            return rule
    sanitized = re.sub("[^a-z0-9]", "_", signal.strip().lower()).strip("_")
    return sanitized or "generic-visual-defect"


def read_rules(obj, skip_absent=False, preserve_canonical=False):
    rules = set()

    def add(value):
        # All fixtures are ASCII rule strings; non-ASCII sanitization is not tested.
        value = value if isinstance(value, str) else None
        if skip_absent and not (value or "").strip():
            return
        rules.add(normalize(value, preserve_canonical))

    def arrays(part):
        for name in ARRAY_FIELDS:
            if isinstance(part.get(name), list):
                for value in part[name]:
                    if isinstance(value, str):
                        add(value)

    for name in FIELDS:
        add(obj.get(name))
    arrays(obj)
    if isinstance(obj.get("metadata"), dict):
        for name in META_FIELDS:
            add(obj["metadata"].get(name))
        arrays(obj["metadata"])
    return sorted(rules)


def allowed_feature(feature, admission_mode="current"):
    if admission_mode == "clean":
        return (bool(feature.strip())
                and not any(x in feature.lower() for x in
                            ("usefulness", "review_priority", "accepted_delta", "score_margin"))
                and feature not in ("visual:chosen_score", "visual:rejected_score_inverse"))
    return (feature in {"internal_vector:mean", "internal_vector:dimension",
                        "source_kind:figma", "source_kind:canva", "source_kind:other"}
            or feature in {"correction_category:" + rule for rule in ALLOWED_RULES})


def deltas(chosen, rejected, skip_absent=False, preserve_canonical=False,
           admission_mode="current"):
    positive = set(read_rules(chosen, skip_absent, preserve_canonical))
    negative = set(read_rules(rejected, skip_absent, preserve_canonical)) - positive
    # Mirrors AddDelta -> IsAllowedLearnedWeightFeatureKey.
    positive = {x for x in positive if allowed_feature("correction_category:" + x, admission_mode)}
    negative = {x for x in negative if allowed_feature("correction_category:" + x, admission_mode)}
    return {**{key: 1 for key in sorted(positive)},
            **{key: -1 for key in sorted(negative)}}


def choose(available, weights, treatment="current"):
    available = sorted(available)
    ranked = sorted(
        [key for key in available if weights.get(key, 0) > 0],
        key=lambda key: (-weights[key], key),
    )
    # Equal observation counts are held fixed in every fixture.
    if ranked:
        return ranked if treatment == "co_required" else ranked[:1]
    if treatment == "respect_negative":
        return [key for key in available if weights.get(key, 0) >= 0]
    return available


def feature_fit(raw_adjustment):
    return round(0.5 + math.tanh(raw_adjustment / 2.0) * 0.5, 6)


def blob_hash(data):
    return hashlib.sha1(b"blob " + str(len(data)).encode() + b"\0" + data).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source-root", type=Path, required=True)
    parser.add_argument("--baseline-source-root", type=Path)
    parser.add_argument("--out", type=Path, required=True)
    args = parser.parse_args()
    checks = {}
    for relative, expected in SOURCE_BLOBS.items():
        actual = blob_hash((args.source_root / relative).read_bytes())
        if actual != expected:
            raise SystemExit(f"Source version mismatch: {relative}: {actual}")
        checks[relative] = actual
    baseline_verified = None
    if args.baseline_source_root is not None:
        relative = "44.DesignConversionDomain/DesignConversionMachine.cs"
        baseline_verified = blob_hash((args.baseline_source_root / relative).read_bytes())
        if baseline_verified != BASELINE_MACHINE_BLOB:
            raise SystemExit(f"Baseline source version mismatch: {baseline_verified}")

    chosen = {"correction_category": "spacing"}
    rejected = {"correction_category": "overflow"}
    result = {
        "scope": "SOURCE_EQUIVALENT_PYTHON_NOT_NATIVE_CSHARP",
        "training_executed": False,
        "semantic_quality_gain_measured": False,
        "source_commit": "358af351e22a92a3f7938d1f9221d5aeb7ddb053",
        "source_blobs_verified": checks,
        "baseline_machine_blob_verified": baseline_verified,
        "baseline_commit": "e346b88a3cc1fcce48aefa626bfc7b00509eefd6",
        "cross_branch_admission": {
            "scope": "feature admission formula; full checkpoint import NOT_RUN",
            "explicit_all_fields_clean":
                deltas(dict.fromkeys(FIELDS, "generic-visual-defect"),
                       dict.fromkeys(FIELDS, "overflow"), admission_mode="clean"),
            "explicit_all_fields_current":
                deltas(dict.fromkeys(FIELDS, "generic-visual-defect"),
                       dict.fromkeys(FIELDS, "overflow")),
            "partial_clean":
                deltas({"correction_category": "generic-visual-defect"}, rejected,
                       admission_mode="clean"),
            "underscore_feature_allowed_clean":
                allowed_feature("correction_category:generic_visual_defect", "clean"),
            "underscore_feature_allowed_current":
                allowed_feature("correction_category:generic_visual_defect"),
            "known_feature_control_both_allow": [
                allowed_feature("correction_category:spacing", "clean"),
                allowed_feature("correction_category:spacing"),
            ],
            "forbidden_signal_control_both_reject": [
                allowed_feature("review_priority", "clean"),
                allowed_feature("review_priority"),
            ],
            "migration_warning": "partial clean can contain hyphen and underscore; naive alias duplicates keys",
            "quality_effect": "NOT_ESTABLISHED; old ordinary generic runtime also used hyphen",
        },
        "phantom_generic": {
            "chosen_rules_current": read_rules(chosen),
            "rejected_rules_current": read_rules(rejected),
            "delta_current": deltas(chosen, rejected),
            "delta_skip_absent": deltas(chosen, rejected, True),
            "all_scalar_fields_present_control":
                deltas(dict.fromkeys(FIELDS, "spacing"),
                       dict.fromkeys(FIELDS, "overflow")),
            "explicit_canonical_normalized_current": normalize("generic-visual-defect"),
            "explicit_canonical_all_fields_current":
                deltas(dict.fromkeys(FIELDS, "generic-visual-defect"),
                       dict.fromkeys(FIELDS, "overflow")),
            "explicit_canonical_partial_current":
                deltas({"correction_category": "generic-visual-defect"}, rejected),
            "explicit_canonical_skip_absent_only":
                deltas({"correction_category": "generic-visual-defect"}, rejected, True),
            "explicit_canonical_skip_and_preserve":
                deltas({"correction_category": "generic-visual-defect"}, rejected, True, True),
            "remediation_scope": "SKIP_EMPTY_ALONE_INCOMPLETE; canonical identity control required",
            "heldout_scope": "local feature-fit only, not visual G3",
            "heldout_input": {
                "unit_id": "design_conversion_runtime_scaffold_improver",
                "source_kind": "figma",
            },
            "state_assumption": "source_kind weight=1 and phantom generic weight=1",
            "fit_with_phantom": feature_fit(2),
            "fit_without_phantom": feature_fit(1),
            "feature_fit_threshold": 0.8,
        },
        "negative_fallback": {
            "same_context_known_bad": ["spacing"],
            "weights": {"spacing": -1},
            "current": choose(["spacing"], {"spacing": -1}),
            "respect_negative": choose(["spacing"], {"spacing": -1}, "respect_negative"),
            "unobserved_control_current": choose(["spacing"], {}),
            "unobserved_control_treatment": choose(["spacing"], {}, "respect_negative"),
            "positive_control_current": choose(["spacing"], {"spacing": 1}),
            "positive_control_treatment": choose(["spacing"], {"spacing": 1}, "respect_negative"),
        },
        "co_required_cardinality": {
            "case_assumption": "two independent corrections are both required",
            "available": ["contrast", "spacing"],
            "weights": {"contrast": 0.6, "spacing": 0.5},
            "no_state": choose(["contrast", "spacing"], {}),
            "trained_current": choose(["contrast", "spacing"], {"contrast": 0.6, "spacing": 0.5}),
            "co_required_treatment": choose(["contrast", "spacing"], {"contrast": 0.6, "spacing": 0.5}, "co_required"),
            "single_defect_control_current": choose(["contrast"], {"contrast": 0.6}),
            "single_defect_control_treatment": choose(["contrast"], {"contrast": 0.6}, "co_required"),
            "mutually_exclusive_actions": "NOT_TESTED; never infer blanket top-k policy",
        },
    }
    p = result["phantom_generic"]
    a = result["cross_branch_admission"]
    assert a["explicit_all_fields_clean"]["generic_visual_defect"] == 1
    assert "generic_visual_defect" not in a["explicit_all_fields_current"]
    assert a["underscore_feature_allowed_clean"] is True
    assert a["underscore_feature_allowed_current"] is False
    assert a["known_feature_control_both_allow"] == [True, True]
    assert a["forbidden_signal_control_both_reject"] == [False, False]
    assert a["partial_clean"]["generic_visual_defect"] == 1
    assert a["partial_clean"]["generic-visual-defect"] == 1
    assert p["delta_current"]["generic-visual-defect"] == 1
    assert "generic-visual-defect" not in p["delta_skip_absent"]
    assert "generic-visual-defect" not in p["all_scalar_fields_present_control"]
    assert p["explicit_canonical_normalized_current"] == "generic_visual_defect"
    assert "generic-visual-defect" not in p["explicit_canonical_all_fields_current"]
    assert p["explicit_canonical_partial_current"]["generic-visual-defect"] == 1
    assert "generic-visual-defect" not in p["explicit_canonical_skip_absent_only"]
    assert p["explicit_canonical_skip_and_preserve"]["generic-visual-defect"] == 1
    assert p["fit_with_phantom"] >= 0.8 > p["fit_without_phantom"]
    n = result["negative_fallback"]
    assert n["current"] == ["spacing"] and n["respect_negative"] == []
    assert n["unobserved_control_current"] == n["unobserved_control_treatment"]
    assert n["positive_control_current"] == n["positive_control_treatment"]
    c = result["co_required_cardinality"]
    assert len(c["no_state"]) == 2 and len(c["trained_current"]) == 1
    assert c["co_required_treatment"] == c["no_state"]
    assert c["single_defect_control_current"] == c["single_defect_control_treatment"]
    result["mechanism_assertions"] = "PASS"
    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n")
    print(json.dumps({
        "mechanism_assertions": "PASS",
        "feature_fit": [p["fit_without_phantom"], p["fit_with_phantom"]],
        "negative_current": n["current"],
        "co_required_current": c["trained_current"],
        "native_training": "NOT_RUN",
    }, ensure_ascii=False))


if __name__ == "__main__":
    main()
