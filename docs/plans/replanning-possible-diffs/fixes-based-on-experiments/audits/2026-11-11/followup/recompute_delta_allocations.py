#!/usr/bin/env python3
"""Recount saved B01 allocation receipts at 05caa77; no model/native calls."""
import argparse
import hashlib
import json
import statistics
from pathlib import Path

PIN = "05caa77dc629d4801783f839f2011db06b1a92ac"
PREFIX = "artifacts/fbe/AUDIT-A07-PHRASE-ADMISSION/attempt-0001/receipts/"
INPUTS = {
    PREFIX + "allocation-admissible-search-accepted-six-reading-panel.json":
        "ba9376436504264632feb3e1723a90cc383a26ba",
    PREFIX + "allocation-admissible-search-versus-single-answer-dense.json":
        "be7943b852b7f62f0dc9750d319eed31d6186154",
}


def fp(path):
    data = path.read_bytes()
    return {"bytes": len(data), "sha256": hashlib.sha256(data).hexdigest(),
            "git_blob": hashlib.sha1(b"blob " + str(len(data)).encode() + b"\0" + data).hexdigest()}


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("--core", type=Path, required=True)
    p.add_argument("--output", type=Path, required=True)
    a = p.parse_args()
    if a.output.exists():
        p.error("Output already exists; choose a new path")
    fingerprints, results = {}, {}
    for (path, expected), (label, cap, witness) in zip(
            INPUTS.items(), [("six_readings", 10400, 10368), ("dense_eight_tokens", 14300, 14240)]):
        before = fp(a.core / path)
        if before["git_blob"] != expected:
            raise ValueError("Input blob mismatch: " + path)
        fingerprints[path] = before
        raw = json.loads((a.core / path).read_text(encoding="utf-8-sig"))
        old, new = raw["SingleAnswerBytesPerRepeat"], raw["AdmissibleBytesPerRepeat"]
        if len(old) != 7 or len(new) != 7 or any(not isinstance(v, int) or v < 0 for v in old + new):
            raise ValueError("Unexpected saved repeat shape: " + path)
        x, y = statistics.median(old), statistics.median(new)
        if x != raw["SingleAnswerMedianBytes"] or y != raw["AdmissibleMedianBytes"]:
            raise ValueError("Saved median does not match rows: " + path)
        results[label] = {"old_median": x, "new_median": y, "delta_bytes": y - x,
                          "component_growth_percent": 100 * (y - x) / x,
                          "cap_bytes": cap, "cap_growth_percent_from_new": 100 * (cap - y) / y,
                          "synthetic_guard_witness_bytes": witness,
                          "witness_growth_percent": 100 * (witness - y) / y,
                          "actual_absolute_comparator": "candidate < cap",
                          "witness_within_absolute_cap": witness < cap,
                          "witness_kind": "arithmetic boundary example, not observed native mutation"}
    for path, old in fingerprints.items():
        if fp(a.core / path) != old:
            raise ValueError("Input changed during recount: " + path)
    report = {"source_ref": PIN, "input_fingerprints": fingerprints, "inputs_unchanged": True,
              "native_runs": 0, "production_model_calls": 0, "results": results,
              "borrowed_denominator_arithmetic_only": {
                  "historical_process_bytes": 239488,
                  "historical_mixed_delta_percent": (9248 - 1344) / 239488 * 100,
                  "new_component_delta_over_old_process_percent": (9864 - 7232) / 239488 * 100,
                  "sum_of_mixed_percentages": ((9248 - 1344) + (9864 - 7232)) / 239488 * 100,
                  "proves_current_product_overhead_below_five_percent": False},
              "limits": "Saved planner-only measurements; no matched whole-product A/B, global dense upper-bound proof, timing or actual guard mutation performed."}
    a.output.parent.mkdir(parents=True, exist_ok=True)
    with a.output.open("x", encoding="utf-8") as out:
        json.dump(report, out, ensure_ascii=False, indent=2)
        out.write("\n")
    print(json.dumps({"output": str(a.output), "results": results}, ensure_ascii=False))


if __name__ == "__main__":
    main()
