#!/usr/bin/env python3
"""Read-only recheck of the saved AUDIT-F03 evidence at one pinned core snapshot.
No dotnet, shell launcher, training, mutation, or product probe is executed.
Usage: python3 codex-late-ownership-evidence.py --core /path/to/core --output new.json
"""
from __future__ import annotations
import argparse
import collections
import hashlib
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET

COMMIT = "f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64"
ART = "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/"
PINS = {
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/logs/build-01-sink-only.log": "1d99c067af3c4ea005317c4bee3008e65cba5a6d",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/logs/build-02-with-tests.log": "ac7fa87e71af53b75cf6399c6c04cbaa218377ef",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/logs/build-03-final-source.log": "48d60f0e01afdaa0529f39e92996a662fbba618f",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/logs/build-04-solution-release.log": "47791f60a592e62b6bedbb4661f4e8d159005a84",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/logs/build-05-final-test-project.log": "161974f0278b1f83d3f56a22b0509ade00959a64",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/logs/build-06-final-after-revert.log": "077d62111e1427fdd0b9c58bfa391b365f777c29",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/logs/discovery-01.log": "719d3a873ad3c8ef511b1513bb70397eab3be32a",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/logs/dll-identity.txt": "558385225f0999a9168b63a535a144091148337e",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/logs/mutation-a-createnew-removed.log": "9e52dc1365ddd2cb30d5db33f11b94121312bc8a",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/logs/mutation-b-untracked-check-removed.log": "cd1134e1d42ce3eae91a14754edac56a24afdd13",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/logs/run-01-green.log": "40069319bebb37442b6b12bae6941fbc28458557",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/logs/run-02-green-after-revert.log": "30a5c69f52f64d14e904675cb6105b6ce1a04c72",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/results/f03-green-01.trx": "2b54391f1dc6c888242b72d95a19082ad6ad8bb4",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/results/f03-green-02-after-revert.trx": "9682d2799e71835cc3bafef8335a2f2f8724ad20",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/results/f03-mutation-a-red.trx": "0c385d818120d0f7ef2183a8fd1e2009bbf25dd2",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/results/f03-mutation-b-red.trx": "3778cf5b8f8905cc1772b06bfbe8493294d35280",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/review/review-build-release.log": "5ea260a588957a9ba1f99d08ac70ebae38e2daee",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/review/review-f03-independent.trx": "040866059daf6d42cd9520dd408c2c4fc981a872",
  "artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0001/review/review-test-run.log": "0f6e6862626fb3cd3bdc3a1562fcd87ccbf3d07d",
  "Tests/ExperimentFixes/ExperimentEvidenceSink.cs": "69372a6a6a3a63c7d62d29dbda0fcd3215715144",
  "Tests/ExperimentFixes/ExperimentAttemptOwnershipTests.cs": "963166aa5bbcae4e7c131d4bc649e8f92b1c9603",
  "Tests/ExperimentFixes/ExperimentEvidenceSinkTests.cs": "45518e7ed7c05d3b63dfd1fc845f99c01c47351b",
  "Tests/DpoStepVectorTimingTests.cs": "3345868e5f8ac670866c63a42bc4fd0897fbec4c"
}
NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
OWN_CLASS = "LlmFromScratch.Tests.ExperimentFixes.ExperimentAttemptOwnershipTests"
SINK_CLASS = "LlmFromScratch.Tests.ExperimentFixes.ExperimentEvidenceSinkTests"
CALLER_CLASS = "LlmFromScratch.Tests.DpoStepVectorTimingTests"
CALLER = CALLER_CLASS + ".ObservationWrite_JudgesPublicationByActualRunTargetSoEvidenceStillReachesStorage"
FACT = re.compile(r"\[Fact\]\s*(?:(?://[^\r\n]*|/\*[\s\S]*?\*/)\s*)*public\s+void\s+(\w+)\s*\(")

def checked(core: Path):
    result = {}
    for path, expected in PINS.items():
        data = (core / path).read_bytes()
        actual = hashlib.sha1(b"blob " + str(len(data)).encode() + b"\0" + data).hexdigest()
        if actual != expected:
            raise ValueError(f"Git blob mismatch: {path}; got {actual}, expected {expected}")
        result[path] = {"git_blob_sha": actual, "sha256": hashlib.sha256(data).hexdigest(), "bytes": len(data)}
    return result

def content(core: Path, path: str):
    return (core / path).read_text(encoding="utf-8-sig")

def parse_trx(core: Path, relative: str):
    root = ET.parse(core / (ART + relative)).getroot()
    definitions = {}
    for d in root.findall("t:TestDefinitions/t:UnitTest", NS):
        method = d.find("t:TestMethod", NS)
        assert method is not None
        entry = {
            "qualified_method": method.attrib["className"] + "." + method.attrib["name"],
            "execution_id": d.find("t:Execution", NS).attrib["id"],
            "storage": d.attrib.get("storage"),
        }
        assert d.attrib["id"] not in definitions
        definitions[d.attrib["id"]] = entry
    rows = []
    for r in root.findall("t:Results/t:UnitTestResult", NS):
        a = r.attrib
        d = definitions[a["testId"]]
        assert a["testName"] == d["qualified_method"]
        assert a["executionId"] == d["execution_id"]
        row = {
            "name": a["testName"], "test_id": a["testId"],
            "execution_id": a["executionId"], "outcome": a["outcome"],
            "start": a["startTime"], "end": a["endTime"],
            "error": r.findtext("t:Output/t:ErrorInfo/t:Message", "", NS),
            "stack": r.findtext("t:Output/t:ErrorInfo/t:StackTrace", "", NS),
        }
        rows.append(row)
    assert len({r["test_id"] for r in rows}) == len(rows)
    assert len({r["execution_id"] for r in rows}) == len(rows)
    assert set(definitions) == {r["test_id"] for r in rows}
    entries = {(e.attrib["testId"], e.attrib["executionId"])
               for e in root.findall("t:TestEntries/t:TestEntry", NS)}
    assert entries == {(r["test_id"], r["execution_id"]) for r in rows}
    counts = collections.Counter(r["outcome"] for r in rows)
    assert set(counts).issubset({"Passed", "Failed"})
    raw_counters = root.find("t:ResultSummary/t:Counters", NS).attrib
    assert int(raw_counters["total"]) == len(rows)
    assert int(raw_counters["executed"]) == len(rows)
    assert int(raw_counters["passed"]) == counts["Passed"]
    assert int(raw_counters["failed"]) == counts["Failed"]
    for key in ("error", "timeout", "aborted", "inconclusive", "notExecuted"):
        assert int(raw_counters[key]) == 0
    return {
        "path": ART + relative, "run_id": root.attrib["id"],
        "times": root.find("t:Times", NS).attrib,
        "row_counts": dict(sorted(counts.items())), "rows": sorted(rows, key=lambda x: x["name"]),
        "declared_counters": raw_counters, "definitions_entries_results_agree": True,
    }

def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument("--core", required=True, type=Path)
    p.add_argument("--output", required=True, type=Path)
    args = p.parse_args()
    if args.output.exists():
        p.error("--output must be new; existing evidence is never overwritten")
    inputs = checked(args.core)
    present = {ART + str(f.relative_to(args.core / ART)).replace("\\", "/")
               for f in (args.core / ART).rglob("*") if f.is_file()}
    assert present == {x for x in PINS if x.startswith(ART)}, "Pinned attempt inventory differs"
    own_text = content(args.core, "Tests/ExperimentFixes/ExperimentAttemptOwnershipTests.cs")
    own_methods = set(FACT.findall(own_text))
    sink_methods = set(FACT.findall(content(args.core, "Tests/ExperimentFixes/ExperimentEvidenceSinkTests.cs")))
    assert len(own_methods) == 9 and len(sink_methods) == 4
    own = {OWN_CLASS + "." + x for x in own_methods}
    sink = {SINK_CLASS + "." + x for x in sink_methods}
    assert CALLER.rsplit(".", 1)[-1] in FACT.findall(content(args.core, "Tests/DpoStepVectorTimingTests.cs"))
    discovery = content(args.core, ART + "logs/discovery-01.log")
    discovered = set(re.findall(r"^\s*(LlmFromScratch\.Tests\.ExperimentFixes\.ExperimentAttemptOwnershipTests\.\w+)\s*$", discovery, re.M))
    assert discovered == own
    relative_trx = [
        "results/f03-green-01.trx", "results/f03-green-02-after-revert.trx",
        "results/f03-mutation-a-red.trx", "results/f03-mutation-b-red.trx",
        "review/review-f03-independent.trx",
    ]
    runs = [parse_trx(args.core, x) for x in relative_trx]
    expected_names = [own | sink | {CALLER}, own | sink | {CALLER}, own, own, own | sink]
    for run, expected in zip(runs, expected_names):
        assert {r["name"] for r in run["rows"]} == expected
    # Stable test IDs cross-check the actual selections, not only summary counters.
    name_to_id = {}
    for run in runs:
        for row in run["rows"]:
            assert name_to_id.setdefault(row["name"], row["test_id"]) == row["test_id"]
    assert runs[0]["row_counts"] == runs[1]["row_counts"] == {"Passed": 14}
    assert runs[4]["row_counts"] == {"Passed": 13}
    failures = []
    expected_failures = [
        ("TwoConcurrentRunsRequestOneAddressAndOnlyOneOwnsItWhileLosersNeverTrain", 188, "Assert.Equal(1, winners);"),
        ("ExistingUntrackedNonEmptyDirectoryWithoutOwnerMarkerIsNeverOverwritten", 305, "var refusal = Assert.Throws<ExperimentAttemptOwnershipRefusedException>(() =>"),
    ]
    source_lines = own_text.splitlines()
    for run, (method, line, current_statement) in zip(runs[2:4], expected_failures):
        assert run["row_counts"] == {"Failed": 1, "Passed": 8}
        failed = [r for r in run["rows"] if r["outcome"] == "Failed"]
        assert len(failed) == 1 and failed[0]["name"] == OWN_CLASS + "." + method
        assert "line " + str(line) in failed[0]["stack"]
        assert source_lines[line - 1].strip() == current_statement
        failures.append({
            "run": run["path"], "method": failed[0]["name"], "message": failed[0]["error"],
            "stack_line": line, "current_pinned_statement": current_statement,
        })
    assert re.search(r"Expected:\s+1\s+Actual:\s+8", failures[0]["message"])
    assert "No exception was thrown" in failures[1]["message"]
    run1 = content(args.core, ART + "logs/run-01-green.log")
    expected_filter = "|".join([
        "FullyQualifiedName~ExperimentAttemptOwnershipTests",
        "FullyQualifiedName~ExperimentEvidenceSinkTests",
        "FullyQualifiedName~DpoStepVectorTimingTests.ObservationWrite_JudgesPublicationByActualRunTargetSoEvidenceStillReachesStorage",
    ])
    assert expected_filter in run1
    run2 = content(args.core, ART + "logs/run-02-green-after-revert.log")
    assert "--filter <F03+accepted+caller>" in run2
    build_logs = {}
    for path in PINS:
        if path.endswith(".log") and ("build-" in path or "/logs/mutation-" in path):
            text = content(args.core, path)
            build_logs[path] = {
                "first_line": text.splitlines()[0],
                "start_utc": re.findall(r"^start_utc=(.+)$", text, re.M),
                "end_utc": re.findall(r"^end_utc=(.+)$", text, re.M),
                "exit_receipts": re.findall(r"^(?:exit|build_exit|test_exit|BUILD_EXIT)=(-?\d+)\s*$", text, re.M),
                "explicit_build_success": "Build succeeded." in text,
                "has_zero_build_or_standalone_exit": bool(re.search(r"^(?:exit|build_exit|BUILD_EXIT)=0\s*$", text, re.M)),
            }
    complete_standalone = [x for x, v in build_logs.items()
        if "/logs/build-" in x and v["has_zero_build_or_standalone_exit"]]
    assert len(complete_standalone) == 5
    assert not build_logs[ART + "logs/build-05-final-test-project.log"]["has_zero_build_or_standalone_exit"]
    assert build_logs[ART + "review/review-build-release.log"]["has_zero_build_or_standalone_exit"]
    dll_receipt = content(args.core, ART + "logs/dll-identity.txt").splitlines()
    assert len(dll_receipt) == 2
    dll_hash, dll_path = dll_receipt[0].split(" *", 1)
    assert re.fullmatch(r"[0-9a-f]{64}", dll_hash)
    assert re.fullmatch(r"[0-9a-f]{40}", dll_receipt[1])
    result = {
        "audit_date_utc": "2026-11-12", "core_snapshot": COMMIT,
        "mode": "read-only parsing of saved raw; zero native or product executions",
        "inputs": inputs,
        "inventory": {
            "artifact_files": len(present), "source_files": len(PINS) - len(present),
            "artifact_paths": sorted(present),
            "commit_description_artifact_count": 21,
            "count_bound": "This exact attempt subtree contains 19 tracked files at the audited commit.",
        },
        "runs": runs,
        "selection_reconciliation": {
            "ownership_source_facts": sorted(own), "sink_source_facts": sorted(sink),
            "discovery_matches_final_ownership_method_names": True,
            "all_runs_use_same_test_id_per_name": True,
            "green1_and_green2_exact_same_selected_ids": True,
            "review_omits_only_this_green_caller": CALLER,
            "green1_literal_filter": expected_filter,
            "green2_filter_receipt": "<F03+accepted+caller>",
            "review_exact_filter_command_in_log": False,
            "bound": "Saved named rows prove the executed selections; names are compatible with final source. This does not authenticate the compiled source bytes.",
        },
        "mutation_failures": failures,
        "build_logs": build_logs,
        "complete_standalone_producer_build_logs": complete_standalone,
        "dll_receipt": {
            "sha256_text": dll_hash, "path_text": dll_path, "base_commit_text": dll_receipt[1],
            "captured_utc_present": False, "source_blob_hashes_present": False,
            "per_run_hash_or_mutation_patch_binding_present": False,
            "bound": "One saved DLL hash and base commit do not prove every no-build run used final f95 source or the stated mutation. No wrong-DLL conclusion is drawn.",
        },
        "limits": [
            "All five TRX files were parsed, not executed; no C# build or test run was made by this audit.",
            "The reviewer omitted the one caller test; none of the nine ownership tests is missing.",
            "Nine ownership methods test structural helpers; simulated lesson counters are not evidence of real training execution.",
            "Mutation descriptions come from log headers; red rows and final matching assert lines are observed, mutated source snapshots are absent here.",
            "Build05 has no terminal result in its saved bytes; missing terminal evidence is not classified as a failed build.",
            "The old A07 records and all original F03 raw remain unchanged.",
        ],
    }
    assert checked(args.core) == inputs, "An input changed during the audit"
    with args.output.open("x", encoding="utf-8") as out:
        json.dump(result, out, ensure_ascii=False, indent=2, sort_keys=True)
        out.write("\n")
    print(json.dumps({
        "inputs_verified": len(inputs), "artifact_files": len(present),
        "runs": [{"path": x["path"], "counts": x["row_counts"]} for x in runs],
        "output": str(args.output),
    }, ensure_ascii=False))

if __name__ == "__main__":
    main()

