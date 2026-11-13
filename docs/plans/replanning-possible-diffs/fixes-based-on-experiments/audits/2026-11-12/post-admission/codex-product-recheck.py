#!/usr/bin/env python3
"""Read-only independent model and saved-evidence check; never executes C#.

The enumerator has no dynamic-programming pruning. Its result is compared with
an independently written projection of the currently pinned planner. Sources
must have the expected Git blob hashes. This is analytical support, not a native
acceptance run or evidence for runtime memory/time, training, or checkpoint IO.
"""
from __future__ import annotations
import argparse
from collections import Counter, defaultdict
import hashlib
import json
from pathlib import Path
from statistics import median
import xml.etree.ElementTree as ET

PIN = "f089a7d55d359ff103d0cdd9ddfb26eee346a16b"
SOURCES = {
    "27.TranslationDomain/TranslationPhraseCompositionPlanner.cs": "4a5ce93149713cfe8da7156d623592d58f5dbf90",
    "27.TranslationDomain/TranslationTrainedUnits.cs": "1ae9111175f00f022b24e67d79e917d4e12aa2e7",
    "Tests/ExperimentFixes/TranslationPhraseAdmissionAlignmentTests.cs": "31c6e87b144cff05225cfddd7646fe29f535f755",
    "Tests/ExperimentFixes/TranslationPhraseCoverageLearningTests.cs": "195d7800cf9c332023ac2a9f4ad52521b4348edb",
    "42.ProjectWikiDomain/PromptFlowGroundingModel.cs": "b1982c77c593f0f5dcc9747a7345c0d770981344",
    "42.ProjectWikiDomain/OrchestrationKnnStore.cs": "2bf119f4caee7c807d80c398f54b695a0304e0d9",
    "42.ProjectWikiDomain/OrchestrationPlanMachine.cs": "449aa38d4cc316b53d5506d1f387a4308a93f028",
}
ARTIFACT_ROOT = "artifacts/fbe/AUDIT-A07-PHRASE-ADMISSION/attempt-0001"
ARTIFACTS = {
    "results/final-two-classes-named.trx": "c32d3c7015b69996d4632c046f6482e8743d0b88",
    "results/final-card-four-named.trx": "397515d44b0c4438ef05d1a6f6ee40f492150846",
    "results/sensitivity-algorithm-mutation-named.trx": "d09a6645eaa22318844076f3ba94f30e1e948ae3",
    "receipts/allocation-admissible-search-accepted-six-reading-panel.json": "ba9376436504264632feb3e1723a90cc383a26ba",
    "receipts/allocation-admissible-search-versus-single-answer-dense.json": "be7943b852b7f62f0dc9750d319eed31d6186154",
}

def blob(data):
    return hashlib.sha1(b"blob " + str(len(data)).encode() + b"\0" + data).hexdigest()

def checked(root, expected):
    result = []
    for name, want in expected.items():
        data = (root / name).read_bytes()
        actual = blob(data)
        if actual != want:
            raise RuntimeError(f"Source changed: {name}: {actual} != {want}")
        result.append({"path": name, "git_blob_sha": actual, "bytes": len(data)})
    return result

# A plan contains covered source positions, phrase count and ordered source keys.
EMPTY = (0, 0, ())

def rank(plan):
    return (-plan[0], plan[1], plan[2])

def admissible(plan, n):
    return plan[1] > 0 and (plan[0] * 100 >= 85 * n or
                            (plan[1] >= 2 and plan[0] * 2 >= n))

def prepend(edge, tail):
    start, end, key = edge
    return (end - start + tail[0], 1 + tail[1], (key,) + tail[2])

def exhaustive(n, edges):
    """Enumerate every skip/phrase path; no coverage/count/category pruning."""
    by_start = defaultdict(list)
    for edge in edges:
        by_start[edge[0]].append(edge)
    def visit(at):
        if at == n:
            yield EMPTY
            return
        yield from visit(at + 1)
        for edge in by_start[at]:
            for tail in visit(edge[1]):
                yield prepend(edge, tail)
    all_plans = list(visit(0))
    accepted = [plan for plan in all_plans if admissible(plan, n)]
    return min(accepted, key=rank) if accepted else EMPTY

def projected_current(n, edges):
    """Per-position/per-exact-count DP matching pinned PlanAdmissible's rule."""
    best = [[None] * (n + 1) for _ in range(n + 1)]
    best[n][0] = EMPTY
    for pos in range(n - 1, -1, -1):
        for spans in range(n + 1):
            here = best[pos + 1][spans]
            if spans:
                for edge in edges:
                    if edge[0] != pos:
                        continue
                    tail = best[edge[1]][spans - 1]
                    if tail is None:
                        continue
                    candidate = prepend(edge, tail)
                    if here is None or rank(candidate) < rank(here):
                        here = candidate
            best[pos][spans] = here
    candidates = [plan for plan in best[0] if plan and admissible(plan, n)]
    return min(candidates, key=rank) if candidates else EMPTY

def projected_old(n, edges):
    best = [EMPTY] * (n + 1)
    for pos in range(n - 1, -1, -1):
        candidates = [best[pos + 1]]
        candidates.extend(prepend(edge, best[edge[1]]) for edge in edges if edge[0] == pos)
        best[pos] = min(candidates, key=rank)
    return best[0] if admissible(best[0], n) else EMPTY

def all_intervals(n):
    return [(i, j, " ".join(f"a{k+1}" for k in range(i, j)))
            for i in range(n) for j in range(i + 1, n + 1)]

def planner_checks():
    total, mismatches, old_differences = 0, [], 0
    by_length, first_old = {}, None
    for n in range(1, 6):
        possible = all_intervals(n)
        count = 0
        for mask in range(1 << len(possible)):
            edges = [edge for bit, edge in enumerate(possible) if mask & (1 << bit)]
            actual = projected_current(n, edges)
            expected = exhaustive(n, edges)
            if actual != expected:
                mismatches.append({"n": n, "mask": mask, "expected": expected, "actual": actual})
            old = projected_old(n, edges)
            if old != expected:
                old_differences += 1
                if first_old is None:
                    first_old = {"n": n, "edges": edges, "expected": expected, "old": old}
            total += 1
            count += 1
        by_length[str(n)] = count
    # Both named A07 shapes use intervals with zero-based positions.
    named = []
    for n, intervals in [(8, [(0, 2), (4, 6), (1, 7)]), (6, [(0, 2), (2, 4), (1, 5)])]:
        edges = [(i, j, " ".join(f"a{k+1}" for k in range(i, j))) for i, j in intervals]
        named.append({"n": n, "old": projected_old(n, edges), "current": projected_current(n, edges),
                      "independent_exhaustive": exhaustive(n, edges)})
    assert total == 33866 and not mismatches and old_differences > 0
    return {"graphs": total, "graphs_by_length": by_length,
            "current_projection_mismatches": mismatches,
            "old_policy_differences": old_differences, "first_old_counterexample": first_old,
            "named_counterexamples": named,
            "boundary": "ASCII distinct-position interval graphs n=1..5; two named n=6/8 cases; no C# execution, no learned state or target-language evaluation, no 0/1/2+ optimization tested"}

def trx_checks(root):
    ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
    result = []
    for name in [p for p in ARTIFACTS if p.endswith(".trx")]:
        tree = ET.parse(root / ARTIFACT_ROOT / name)
        rows = tree.findall(".//t:UnitTestResult", ns)
        counts = tree.find(".//t:Counters", ns).attrib
        by_class = defaultdict(Counter)
        for row in rows:
            by_class[row.attrib["testName"].rsplit(".", 1)[0]][row.attrib["outcome"]] += 1
        assert len(rows) == int(counts["total"])
        assert sum(row.attrib["outcome"] == "Passed" for row in rows) == int(counts["passed"])
        result.append({"path": ARTIFACT_ROOT + "/" + name, "total": len(rows),
                       "by_class": {key: dict(value) for key, value in by_class.items()},
                       "failed_names": [r.attrib["testName"] for r in rows if r.attrib["outcome"] == "Failed"]})
    return result

def resource_checks(root):
    result = []
    for name in [p for p in ARTIFACTS if p.endswith(".json")]:
        item = json.loads((root / ARTIFACT_ROOT / name).read_text(encoding="utf-8-sig"))
        old = median(item["SingleAnswerBytesPerRepeat"])
        new = median(item["AdmissibleBytesPerRepeat"])
        assert old == item["SingleAnswerMedianBytes"] and new == item["AdmissibleMedianBytes"]
        result.append({"path": ARTIFACT_ROOT + "/" + name, "repeats": len(item["AdmissibleBytesPerRepeat"]),
                       "old_median": old, "new_median": new, "growth_percent": 100 * (new-old)/old,
                       "scope": "stored native component allocation observation, re-read only"})
    return {"stored_component_recounts": result,
            "counterexample_to_densest_label": {
                "source_words": 8, "fixture_matching_phrases": 14,
                "all_distinct_contiguous_phrases": len(all_intervals(8)),
                "all_intervals_without_exact_full_input_phrase": len(all_intervals(8))-1,
                "multiword_proper_contiguous_phrases": sum(1 for i, j, _ in all_intervals(8) if 2 <= j-i <= 7),
                "actual_training_and_ordinary_route_reachability": "UNRUN; must be verified before measuring allocations",
                "unrelated_memory_entries_have_no_scope_cap": True,
                "boundary": "combinatorial/source fact; no allocation, elapsed-time or training measurement"},
            "panel_lengths": [3, 2, 2, 1, 8, 9],
            "planner_scope_max": 8,
            "native_full_product_AB_executed_here": False}

def tokens(text):
    import re
    return set(re.findall(r"[a-z0-9]+", text.lower()))

def flow_check():
    """Two simple ASCII lessons; source projection, no facade or checkpoint run."""
    lessons = [dict(id="witness-a", flow="FLOW-01", prompt="alpha"),
               dict(id="witness-b", flow="FLOW-02", prompt="beta")]
    result = []
    for name, entries, prompt in [("A_only_negative", lessons[:1], "beta FLOW-01"),
                                  ("A_B_negative", lessons, "beta FLOW-01"),
                                  ("B_A_negative", list(reversed(lessons)), "beta FLOW-01"),
                                  ("positive_A", lessons, "alpha FLOW-01"),
                                  ("positive_B", lessons, "beta FLOW-02")]:
        # Repeated scope/spans cannot alter set-token weights in these cases.
        query = tokens(prompt)
        scored = [(len(query & tokens(p["prompt"])), len(query & tokens(p["flow"])), p) for p in entries]
        scored.sort(key=lambda x: (-(x[0]+x[1]), -x[1], x[2]["flow"]))
        inp, scope, candidate = scored[0]
        def witness(candidates):
            a = []
            for p in candidates:
                other = tokens(p["prompt"])
                score = len(query & other)/len(query | other) if query | other else 1.0
                a.append((score, p))
            a.sort(key=lambda x: (-x[0], x[1]["id"]))
            return a[0] if a else (0.0, None)
        global_score, global_pair = witness(entries)
        own_score, own_pair = witness([p for p in entries if p["flow"] == candidate["flow"]])
        def confidence(w):
            return min(1.0, .70+(inp+scope)*.10+w*.20) if inp+scope > 0 and w > 0 else None
        result.append({"case": name, "prompt": prompt, "candidate": candidate["flow"],
                       "candidate_input_score": inp, "candidate_scope_score": scope,
                       "global_witness": global_pair["flow"] if global_pair else None,
                       "global_similarity": global_score, "current_confidence": confidence(global_score),
                       "same_flow_similarity": own_score, "proposed_scoped_confidence": confidence(own_score)})
    assert result[0]["current_confidence"] is None
    assert abs(result[1]["current_confidence"] - 29/30) < 1e-12
    assert result[1]["proposed_scoped_confidence"] is None
    assert result[2]["current_confidence"] == result[1]["current_confidence"]
    return {"cases": result, "native_calls": 0,
            "boundary": "independent ASCII arithmetic/source projection; checkpoint persistence and C# facade still UNRUN; same-flow filtering does not establish active-revision membership or relevance"}

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--core", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    before = checked(args.core, SOURCES)
    artifacts = checked(args.core, {ARTIFACT_ROOT+"/"+k: v for k,v in ARTIFACTS.items()})
    result = {"source_commit": PIN, "native_runtime_executed": False,
              "new_training_calls": 0, "verified_sources": before,
              "verified_saved_artifacts": artifacts,
              "saved_native_trx": trx_checks(args.core),
              "phrase_search_model": planner_checks(),
              "resource_evidence": resource_checks(args.core),
              "foreign_flow_witness": flow_check()}
    assert checked(args.core, SOURCES) == before
    assert checked(args.core, {ARTIFACT_ROOT+"/"+k: v for k,v in ARTIFACTS.items()}) == artifacts
    with args.output.open("x", encoding="utf-8") as stream:
        json.dump(result, stream, ensure_ascii=False, indent=2)
        stream.write("\n")
    print(json.dumps({"output": str(args.output), "graphs": result["phrase_search_model"]["graphs"],
                      "projection_mismatches": len(result["phrase_search_model"]["current_projection_mismatches"]),
                      "old_policy_differences": result["phrase_search_model"]["old_policy_differences"],
                      "sources_verified": len(before), "saved_artifacts_verified": len(artifacts),
                      "native_runtime_executed": False}))

if __name__ == "__main__":
    main()
