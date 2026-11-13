#!/usr/bin/env python3
"""Audit arithmetic and source-equivalent models, never native/model execution.

Usage: python recompute_product_measurements.py --core /path/to/core \
       --output /new/exclusive/result.json
Inputs must match source-manifest.json beside this script. The WC01 numerical
replay follows the earlier audit replay, re-executed against verified blobs;
the exhaustive interval oracle and candidate DP are separate implementations.
Only Python's standard library is used. Existing results are never overwritten.
"""
import argparse
import hashlib
import itertools
import json
import math
import re
import struct
from collections import Counter
from pathlib import Path


def fingerprint(path):
    data = path.read_bytes()
    return {"bytes": len(data), "sha256": hashlib.sha256(data).hexdigest(),
            "git_blob_sha1": hashlib.sha1(b"blob " + str(len(data)).encode() + b"\0" + data).hexdigest()}


def phrase_checks():
    # Plan = ordered tuple of (start, end, canonical source key). Skips are not
    # stored. The comparator is source v1's coverage / span count / key order.
    def coverage(plan):
        return sum(e - s for s, e, _ in plan)

    def rank(plan):
        return (-coverage(plan), len(plan), tuple(k for _, _, k in plan))

    def admitted(plan, n):
        # Integer form avoids introducing a new floating-point threshold.
        return bool(plan) and (100 * coverage(plan) >= 85 * n or
                               len(plan) >= 2 and 2 * coverage(plan) >= n)

    def current_dp(n, edges):
        best = [()] * (n + 1)
        for p in range(n - 1, -1, -1):
            candidates = [best[p + 1]]
            candidates.extend((edge,) + best[edge[1]] for edge in edges if edge[0] == p)
            best[p] = min(candidates, key=rank)
        return best[0]

    def candidate_dp(n, edges):
        # Proposed repair, not current product code. Keep the best suffix per
        # count category 0 / 1 / >=2, retaining actual count for tie breaking.
        best = [{} for _ in range(n + 1)]
        best[n] = {0: ()}
        for p in range(n - 1, -1, -1):
            candidates = list(best[p + 1].values())
            for edge in edges:
                if edge[0] == p:
                    candidates.extend((edge,) + tail for tail in best[edge[1]].values())
            for plan in candidates:
                category = min(2, len(plan))
                incumbent = best[p].get(category)
                if incumbent is None or rank(plan) < rank(incumbent):
                    best[p][category] = plan
        allowed = [p for p in best[0].values() if admitted(p, n)]
        return min(allowed, key=rank) if allowed else ()

    def oracle(n, edges):
        # Independent combinations oracle: enumerate subsets and reject
        # overlaps. It does not use suffix DP or its pruning rule.
        for count in range(len(edges) + 1):
            for chosen in itertools.combinations(sorted(edges), count):
                if all(left[1] <= right[0] for left, right in zip(chosen, chosen[1:])):
                    yield chosen

    def show(plan, n):
        return {"covered": coverage(plan), "spans": len(plan),
                "keys": [x[2] for x in plan], "coverage_span_admitted": admitted(plan, n)}

    witnesses = []
    for n, intervals in [(8, [(0, 2), (4, 6), (1, 7)]),
                         (6, [(0, 2), (2, 4), (1, 5)])]:
        edges = [(s, e, " ".join("a" + str(i + 1) for i in range(s, e))) for s, e in intervals]
        plans = list(oracle(n, edges))
        expected = min((p for p in plans if admitted(p, n)), key=rank)
        actual, proposed = current_dp(n, edges), candidate_dp(n, edges)
        assert not admitted(actual, n) and admitted(expected, n) and proposed == expected
        witnesses.append({"tokens": n, "intervals": edges, "all_plans": len(plans),
                          "current": show(actual, n), "best_admitted": show(expected, n),
                          "candidate": show(proposed, n)})

    checked = 0
    graph_counts = {}
    for n in range(1, 6):
        edges = [(s, e, " ".join("a" + str(i + 1) for i in range(s, e)))
                 for s in range(n) for e in range(s + 1, n + 1)]
        edge_index = {edge: i for i, edge in enumerate(edges)}
        possible = [(sum(1 << edge_index[x] for x in plan), plan)
                    for plan in oracle(n, edges) if admitted(plan, n)]
        for mask in range(1 << len(edges)):
            selected = [edge for i, edge in enumerate(edges) if mask & (1 << i)]
            eligible = [plan for required, plan in possible if required & mask == required]
            expected = min(eligible, key=rank) if eligible else ()
            assert candidate_dp(n, selected) == expected, (n, mask)
            checked += 1
        graph_counts[str(n)] = 1 << len(edges)
    return {"witnesses": witnesses, "candidate_graphs": checked,
            "graphs_by_token_count": graph_counts, "candidate_mismatches": 0,
            "status": "PASS_ANALYTICAL_MODEL",
            "limits": "Normalized synthetic interval model only: no C#, tokenization, target validity, facade, timing or allocation run. Not exhaustive for n=6..8 or arbitrary key order."}


def wiki_replay(read):
    model = read("42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs")
    fixture = json.loads(read("experiments-v2/RP01-MICRO/WC01/fixture.json"))
    snapshot = json.loads(read("experiments-v2/RP01-MICRO/WC01/results/attempt-001/A/after-snapshot.json"))
    labels = re.findall(r"ProjectWikiRequestRouteKind\.(\w+)",
                        model.split("private static readonly ProjectWikiRequestRouteKind[] Labels =", 1)[1].split("];", 1)[0])
    assert labels == snapshot["labels"]
    assert "private const int EpochCount = 18;" in model
    assert "private const double LearningRate = 0.14;" in model

    def features(text):
        assert text.isascii(), "This replay only establishes equivalence on the pinned ASCII fixture"
        tokens = re.findall(r"[A-Za-z0-9_-]+", text.lower())
        counts = {}
        for token in tokens:
            counts[token] = counts.get(token, 0.) + 1.
        for left, right in zip(tokens, tokens[1:]):
            key = left + "_" + right
            counts[key] = counts.get(key, 0.) + 1.
        norm = max(1., math.sqrt(sum(v * v for v in counts.values())))
        return {key: value / norm for key, value in counts.items()}

    prepared = [(row, features(row["prompt"])) for row in fixture["train"]]
    weights, bias = {}, [0.] * len(labels)
    ledger = []
    for epoch in range(18):
        updates = []
        for row, feat in prepared:
            scores = list(bias)
            for key, value in feat.items():
                if key in weights:
                    for i in range(len(scores)):
                        scores[i] += weights[key][i] * value
            predicted = max(range(len(scores)), key=scores.__getitem__)
            expected = labels.index(row["expectedRouteKind"])
            if expected == predicted:
                continue
            for key, value in feat.items():
                w = weights.setdefault(key, [0.] * len(labels))
                w[expected] += .14 * value
                w[predicted] -= .14 * value
            bias[expected] += .14
            bias[predicted] -= .14
            updates.append(row["id"])
        ledger.append({"epoch": epoch + 1, "presentations": len(prepared), "update_rows": updates})
    assert set(weights) == set(snapshot["tokenWeights"])
    pairs = list(zip(bias, snapshot["biases"]))
    for key, value in weights.items():
        pairs.extend(zip(value, snapshot["tokenWeights"][key]))
    assert all(struct.pack(">d", a) == struct.pack(">d", b) for a, b in pairs)
    counts = [len(x["update_rows"]) for x in ledger]
    return {"kind": "Source-equivalent Python replay; earlier replay method reused, input blobs independently checked",
            "updates": sum(counts), "active_epochs": sum(n > 0 for n in counts),
            "empty_epochs": sum(n == 0 for n in counts), "updates_by_epoch": counts,
            "snapshot_double_parameters": len(pairs), "bit_exact": True,
            "first_complete_no_update_epoch": counts.index(0) + 1, "ledger": ledger,
            "limits": "Not a native training or speed measurement; does not justify a fixed global epoch count."}


def p49_overlap(read):
    source = read("experiments-v2/RP01-MICRO/P4901/src/FinitenessGovernedVerdict.cs")
    def pairs(method):
        block = source.split("public static FrozenPair[] " + method + "()", 1)[1].split("};", 1)[0]
        return [(int(a), int(b)) for a, b in re.findall(r'new FrozenPair\("[^"]+", "[^"]+", \d+, (\d+), (\d+)\)', block)]
    panel = [(name, int(a), int(b)) for name, a, b in re.findall(
        r'new PanelRow\("([^"]+)",\s*"EXAM",\s*"EXAM",\s*"[^"]+",\s*"[^"]+",\s*"[^"]+",\s*(\d+),\s*(\d+)\)', source)]
    assert len(pairs("WarmPairs")) == len(pairs("NewPairs")) == 6 and len(panel) == 4
    result = {}
    for name, train in [("warm_plus_new", pairs("WarmPairs") + pairs("NewPairs")), ("new_only", pairs("NewPairs"))]:
        inputs, exact = {a for a, _ in train}, set(train)
        rows = [{"row": name, "input": a, "gold": b,
                 "category": "exact_pair_seen" if (a, b) in exact else "input_seen_target_new" if a in inputs else "input_unseen"}
                for name, a, b in panel]
        counts = Counter(r["category"] for r in rows)
        result[name] = {"rows": rows, "total": len(panel),
                        "input_overlap": sum(a in inputs for _, a, _ in panel),
                        "pair_overlap": sum((a, b) in exact for _, a, b in panel),
                        "disjoint_counts": dict(counts)}
    result["exam_constant_ceiling"] = max(Counter(b for _, _, b in panel).values())
    result["exam_input_function_ceiling"] = sum(max(Counter(b for _, x, b in panel if x == a).values()) for a in {a for _, a, _ in panel})
    return result


def p52_ceiling(read):
    source = read("Tests/ExperimentFixes/P5201RejectedAnnotationAddressArmsTests.cs")
    block = source.split("private static ExamRow[] ExamRows()", 1)[1].split("];", 1)[0]
    rows = re.findall(r'new ExamRow\("([^"]+)", "([^"]+)", TenseReading\.(\w+)', block)
    assert len(rows) == 4 and all(gold.isascii() for _, gold, _ in rows)
    counts = Counter(gold.lower() for _, gold, _ in rows)
    saved = json.loads(read("experiments-v2/RP01-MICRO/P5201/results/attempt-0002/metrics.json"))
    observed = saved["предел_постоянного_ответа"]["верных_ответов"]
    return {"panel_rows": len(rows), "gold_histogram": dict(counts),
            "computed_answer_constant_ceiling": max(counts.values()),
            "computed_tense_constant_ceiling": max(Counter(t for _, _, t in rows).values()),
            "saved_attempt2_ceiling_field": observed,
            "saved_field_is_wrong": observed != max(counts.values()),
            "comparison_scope": "ASCII gold in the pinned panel; case-insensitive whole-string equality. No new normalization."}


def loss_counterexample():
    def losses(target):
        total = 1. + math.exp(target)
        probability = math.exp(target) / total
        return -math.log(max(probability, 1e-7)), math.log(total) - target, probability
    target, h = -20., 1e-5
    clipped, true_ce, probability = losses(target)
    lower, upper = losses(target - h), losses(target + h)
    return {"logits": [0., target], "target_index": 1, "target_probability": probability,
            "clipped_loss": clipped, "stable_ce": true_ce,
            "reported_loss_finite_difference": (upper[0] - lower[0]) / (2 * h),
            "true_ce_finite_difference": (upper[1] - lower[1]) / (2 * h),
            "implemented_target_gradient": probability - 1., "h": h,
            "kind": "Independent scalar arithmetic; no C# neural network invoked"}


def main():
    if not __debug__:
        raise SystemExit("Audit assertions are required; run without Python -O/-OO or PYTHONOPTIMIZE")
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--core", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    manifest = json.loads(Path(__file__).with_name("source-manifest.json").read_text())
    before = {}
    for path, expected in manifest["inputs"].items():
        actual = fingerprint(args.core / path)
        if actual["git_blob_sha1"] != expected:
            raise ValueError(f"Input blob mismatch: {path}: {actual['git_blob_sha1']} != {expected}")
        before[path] = actual
    def read(path):
        if path not in before:
            raise ValueError("Unpinned input: " + path)
        return (args.core / path).read_text(encoding="utf-8-sig")
    result = {"source_ref": manifest["source_ref"], "input_fingerprints": before,
              "native_runs": 0, "production_model_calls": 0, "new_scientific_experiments": 0,
              "phrase": phrase_checks(), "wiki": wiki_replay(read),
              "p49": p49_overlap(read), "p52": p52_ceiling(read), "loss": loss_counterexample()}
    result["inputs_unchanged"] = all(fingerprint(args.core / p) == value for p, value in before.items())
    assert result["inputs_unchanged"]
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with args.output.open("x", encoding="utf-8") as stream:
        json.dump(result, stream, ensure_ascii=False, indent=2)
        stream.write("\n")
    print(json.dumps({"output": str(args.output), "inputs_verified": len(before),
                      "phrase_graphs": result["phrase"]["candidate_graphs"],
                      "wiki_updates": result["wiki"]["updates_by_epoch"],
                      "p52_ceiling": result["p52"]["computed_answer_constant_ceiling"],
                      "native_runs": 0, "production_model_calls": 0}, ensure_ascii=False))


if __name__ == "__main__":
    main()
