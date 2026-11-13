#!/usr/bin/env python3
"""Source-equivalent replay of two WikiTraceAggregatorUnit selectors.

This is a deterministic Python translation of the cited C# selection rules,
not native .NET execution and not a model-quality/training experiment.
Reads immutable downloaded GitHub JSON evidence; writes only a result receipt.
"""
import argparse
import copy
import hashlib
import json
import math
from collections import Counter, defaultdict
from pathlib import Path

DONOR_SHA = "219d7ba89dc3ef43e03e17f1461ad96154d3dd7a"
CURRENT_SHA = "4945b8e85c17b5f150eb5b840d16bbdc4b506242"
DONOR_REGISTRY_BLOB = "3a77453b73cf6e0257a35641c8a799defe493c53"
CURRENT_REGISTRY_BLOB = "f4a2a6b4f9e164a3e91fa64671e4efcb59c3c077"
WEAK = {"rejected", "blocked", "experimental_no_authority"}


def load_json(path):
    return json.loads(Path(path).read_text(encoding="utf-8-sig"))


def number(value):
    try:
        return float(value)
    except (ValueError, TypeError):
        return -math.inf


def validate(registry, current):
    """Return first error in the matching registry validation order."""
    es = registry["entries"]
    if registry["training_continuation_blocked_until_registry_validated"]:
        return "PROJECT_WIKI_TRAINING_BLOCKED_UNTIL_REGISTRY_VALIDATED"
    if any(e["status"] not in WEAK | {"accepted"} for e in es):
        return "PROJECT_WIKI_REGISTRY_INVALID_STATUS"
    if any(e["status"] == "rejected" and e["checkpoint_allowed_as_continuation"] for e in es):
        return "REJECTED_CHECKPOINT_CONTINUATION_SOURCE"
    accepted = [e for e in es if e["status"] == "accepted"]
    if any(not (e.get("runner_report_path") or "").strip() for e in accepted):
        return "ACCEPTED_RUNNER_REPORT_MISSING"
    if any(e["checkpoint_allowed_as_continuation"] and not (e.get("checkpoint_ref") or "").strip() for e in accepted):
        return "ACCEPTED_CHECKPOINT_REFERENCE_MISSING"
    if any(e.get("locator_status") == "UNKNOWN" for e in accepted):
        return "ACCEPTED_LOCATOR_STATUS_UNKNOWN"
    if current:
        publishable = [e for e in accepted if e.get("import_export_status") != "not_publishable_baseline_only"]
        if any(not (e.get("locator_key") or "").strip() for e in publishable):
            return "ACCEPTED_LOCATOR_KEY_MISSING"
        if any(not (e.get("remote_unit_id") or "").strip() for e in publishable):
            return "ACCEPTED_REMOTE_UNIT_ID_MISSING"
    return None


def ranks(entries, donor):
    all_groups = defaultdict(list)
    for e in entries:
        all_groups[e["unit_name"]].append(e)
    # Match GroupBy encounter order after each source's Where filter. This
    # matters to the current selector's stable tie ordering.
    group_order = {}
    for e in entries:
        name = e["unit_name"]
        if donor and (not name or not name.strip() or name == "UNKNOWN"):
            continue
        if not donor and e["status"] not in WEAK:
            continue
        group_order.setdefault(name, None)
    result = []
    for name in group_order:
        es = all_groups[name]
        weak = [e for e in es if e["status"] in WEAK]
        accepted = [e for e in es if e["status"] == "accepted" and e["checkpoint_allowed_as_continuation"]]
        best = max([number(e["metrics"]["accuracy"]) for e in accepted], default=-math.inf)
        # All actual batch IDs / unit names in this probe are ASCII. Python
        # lexicographic order therefore equals C# StringComparer.Ordinal.
        recent = sorted(es, key=lambda e: e["batch_id"], reverse=True)[:8]
        recent_rejected = sum(e["status"] == "rejected" for e in recent)
        recent_weak = sum(e["status"] in WEAK for e in recent)
        blocked = sum(e["status"] == "blocked" for e in es)
        experimental = sum(e["status"] == "experimental_no_authority" for e in es)
        if donor:
            score = recent_rejected * 5 + blocked * 2 + min(experimental, 10)
            score += 20 if not accepted else 15 if best < .75 else 5 if best < .90 else 0
            if best >= .85 and recent_rejected >= 3:
                score = 0
            score = max(0, score)
        else:
            score = sum({"rejected": 3, "blocked": 2, "experimental_no_authority": 1}[e["status"]] for e in weak)
        result.append({
            "unit": name, "score": score, "weak_row_count": len(weak),
            "accepted_continuation_count": len(accepted),
            "best_accepted_accuracy": best if math.isfinite(best) else None,
            "recent_rejected": recent_rejected, "recent_weak": recent_weak,
            "recent_batch_ids": [e["batch_id"] for e in recent],
            "eligible": bool(name and name.strip() and name != "UNKNOWN" and score > 0) if donor else bool(name != "UNKNOWN" and weak),
        })
    if donor:
        result.sort(key=lambda r: (-r["score"], -r["recent_weak"], r["unit"]))
    else:
        # Stable sort preserves first group encounter order, as LINQ does.
        result.sort(key=lambda r: (-r["score"], -r["weak_row_count"]))
    return result


def select(registry, donor, failed_units=()):
    error = validate(registry, current=not donor)
    ranking = ranks(registry["entries"], donor)
    live_failed = next((x for x in failed_units if x and x.strip()), None)
    target = None if error else live_failed or next((x["unit"] for x in ranking if x["eligible"]), "ProjectWikiRequestRouterUnit")
    return {"validation_error": error, "target": target, "live_failed_override": live_failed, "ranking": ranking}


def compact(selection, focus):
    return {
        "validation_error": selection["validation_error"],
        "target": selection["target"],
        "live_failed_override": selection["live_failed_override"],
        "focus": next((r for r in selection["ranking"] if r["unit"] == focus), None),
        "ranking": [{k: r[k] for k in ("unit", "score", "weak_row_count", "accepted_continuation_count", "best_accepted_accuracy", "recent_rejected", "recent_weak", "eligible")} for r in selection["ranking"]],
    }


def append_events(registry, focus, statuses):
    out = copy.deepcopy(registry)
    seed = next(e for e in out["entries"] if e["unit_name"] == focus and e["status"] == "accepted")
    for i, status in enumerate(statuses):
        e = copy.deepcopy(seed)
        e["batch_id"] = f"runner-metrics/9999-12-31-source-proxy-{i:03d}"
        e["status"] = status
        e["status_reason"] = "SOURCE_PROXY_EVENT_ONLY_NOT_A_RUN"
        e["metrics"]["accuracy"] = 1.0 if status == "accepted" else .50
        e["checkpoint_allowed_as_continuation"] = status == "accepted"
        out["entries"].append(e)
    out["status_counts"] = dict(Counter(e["status"] for e in out["entries"]))
    return out


def main():
    p = argparse.ArgumentParser()
    p.add_argument("--donor", default=str(Path(__file__).with_name("donor-registry-input.json")))
    p.add_argument("--current", default=str(Path(__file__).with_name("current-registry-input.json")))
    p.add_argument("--output", default=str(Path(__file__).with_name("selector-receipt.json")))
    a = p.parse_args()
    donor, current = load_json(a.donor), load_json(a.current)
    csel = select(current, donor=False)
    assert csel["validation_error"] is None
    focus = csel["target"]
    cases = []
    variants = [
        ("matched_current_registry", current, []),
        ("one_new_accepted_1_00", append_events(current, focus, ["accepted"]), ["accepted"]),
        ("eight_recent_accepted_1_00", append_events(current, focus, ["accepted"] * 8), ["accepted"] * 8),
        ("seven_rejected_then_one_accepted_1_00", append_events(current, focus, ["rejected"] * 7 + ["accepted"]), ["rejected"] * 7 + ["accepted"]),
        ("one_new_rejected", append_events(current, focus, ["rejected"]), ["rejected"]),
    ]
    for name, corpus, events in variants:
        cases.append({"case": name, "entry_count": len(corpus["entries"]), "events": events,
                      "donor_selector": compact(select(corpus, donor=True), focus),
                      "current_selector": compact(select(corpus, donor=False), focus)})
    control_target = "DocumentationGeneratorUnit"
    controls = [{"case": "fresh_failed_unit_override", "failed_units": [control_target],
                 "donor_selector": compact(select(current, donor=True, failed_units=[control_target]), focus),
                 "current_selector": compact(select(current, donor=False, failed_units=[control_target]), focus)}]
    blocked = copy.deepcopy(current)
    blocked["training_continuation_blocked_until_registry_validated"] = True
    controls.append({"case": "registry_gate_blocks_both", "donor_selector": compact(select(blocked, donor=True), focus),
                     "current_selector": compact(select(blocked, donor=False), focus)})
    receipt = {
        "evidence_level": "EXECUTED_SOURCE_EQUIVALENT_PYTHON_NOT_NATIVE_DOTNET",
        "training_run_performed": False, "learning_quality_gain_measured": False,
        "sources": {"repository": "lubochka/llm_mvp_core", "donor_sha": DONOR_SHA, "current_sha": CURRENT_SHA,
                    "selector_path": "42.ProjectWikiDomain/WikiTraceAggregatorUnit.cs",
                    "donor_selector_lines": "60-170", "current_selector_lines": "63-115",
                    "registry_path": "43.LearningLocalWikiData/registry/PROJECT-WIKI-TRAINING-ASSET-REGISTRY.json",
                    "donor_registry_blob": DONOR_REGISTRY_BLOB, "current_registry_blob": CURRENT_REGISTRY_BLOB},
        "input_file_sha256": {"donor": hashlib.sha256(Path(a.donor).read_bytes()).hexdigest(),
                              "current": hashlib.sha256(Path(a.current).read_bytes()).hexdigest()},
        "input_projection_provenance": {"donor": donor.get("_evidence"), "current": current.get("_evidence")},
        "note": "Current checked corpus passes both source-equivalent registry validators. Full donor corpus lacks locator fields required by current validator; its cross-validator refusal is preserved below.",
        "donor_corpus": {"entry_count": len(donor["entries"]), "donor_selector": compact(select(donor, True), focus), "current_selector": compact(select(donor, False), focus)},
        "focus_unit": focus, "matched_cases": cases, "controls": controls,
    }
    Path(a.output).write_text(json.dumps(receipt, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"focus": focus, "matched_cases": [{"case": c["case"], "donor_target": c["donor_selector"]["target"], "current_target": c["current_selector"]["target"], "donor_focus_score": c["donor_selector"]["focus"]["score"], "current_focus_score": c["current_selector"]["focus"]["score"]} for c in cases], "controls": [{"case": c["case"], "donor_target": c["donor_selector"]["target"], "current_target": c["current_selector"]["target"]} for c in controls]}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
