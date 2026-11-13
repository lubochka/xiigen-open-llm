#!/usr/bin/env python3
"""Read-only recount of pinned historical XIIGEN evidence. No dotnet invocation.

The 22 MB main full-suite.trx is deliberately NOT an input. Main-suite results
are independently recounted from console rows and checked against the author's
JSON. The three guard TRX and two smaller-project TRX are parsed directly.
"""
from __future__ import annotations

import argparse
import collections
import hashlib
import json
from pathlib import Path
import re
import xml.etree.ElementTree as ET

CORE_SHA = "009bf226174441fa0fbf0336e74ad2f2b7174d92"
NAMESPACE = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
DEFAULT_WORK = Path(__file__).resolve().parents[1]


def fingerprint(path: Path) -> dict:
    data = path.read_bytes()
    return {
        "bytes": len(data),
        "sha256": hashlib.sha256(data).hexdigest(),
        "git_blob_sha1": hashlib.sha1(b"blob " + str(len(data)).encode() + b"\0" + data).hexdigest(),
    }


def parse_trx(path: Path) -> tuple[dict, dict]:
    root = ET.parse(path).getroot()
    result_nodes = root.findall(".//t:UnitTestResult", NAMESPACE)
    outcomes = collections.Counter(node.attrib["outcome"] for node in result_nodes)
    records = {}
    for node in result_nodes:
        name = node.attrib["testName"]
        if name in records:
            raise ValueError(f"Ambiguous duplicate test name in {path.name}: {name}")
        records[name] = {
            "outcome": node.attrib["outcome"],
            "message": node.findtext(".//t:Message", default="", namespaces=NAMESPACE),
        }
    counter_node = root.find(".//t:ResultSummary/t:Counters", NAMESPACE)
    counters = {k: int(v) for k, v in counter_node.attrib.items()} if counter_node is not None else {}
    summary_node = root.find(".//t:ResultSummary", NAMESPACE)
    failures = {name: record for name, record in records.items() if record["outcome"] == "Failed"}
    return {
        **fingerprint(path),
        "result_summary_outcome": summary_node.attrib.get("outcome") if summary_node is not None else None,
        "result_elements": len(result_nodes),
        "outcomes": dict(outcomes),
        "counters": counters,
        "counter_total_matches_rows": counters.get("total") == len(result_nodes),
        "failed_test_names": sorted(failures),
        "failure_message_classification": {
            term: sum(term in record["message"] for record in failures.values())
            for term in ["TargetFramework", ".kiro", "new_test_class_without_strong_batch_signal"]
        },
    }, records


def method_name(display_name: str) -> str:
    return display_name.split("(", 1)[0]


def positive_filter_terms(filter_text: str) -> list[str]:
    # Source: TestSuiteBatchManifest.GetPositiveFilterTerms (1850+) and
    # ClassOrMethodFilterTermMatches (1834+). This is a bounded text comparison,
    # not execution of the C# discovery or first-match assignment algorithm.
    return [
        part.strip().strip("()")[len("FullyQualifiedName~"):]
        for part in re.split(r"[|&]", filter_text)
        if part.strip().strip("()").startswith("FullyQualifiedName~")
    ]


def term_matches(class_name: str, term: str) -> bool:
    class_name, term = class_name.casefold(), term.casefold()
    if term.endswith("."):
        return class_name == term[:-1] or class_name.startswith(term)
    return term in class_name


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--core", type=Path, default=DEFAULT_WORK / "repos/core")
    parser.add_argument("--trees", type=Path, default=DEFAULT_WORK / "trees")
    parser.add_argument("--output", type=Path, default=Path(__file__).with_name("suite_recomputation.json"))
    args = parser.parse_args()
    core = args.core.resolve()
    full = core / "artifacts/fbe/FULL-SUITE-20261110/attempt-0001"
    guard = core / "artifacts/fbe/FIX-BATCH-INVENTORY-GUARD/attempt-0001"
    report = {
        "audit_source_commit": CORE_SHA,
        "native_run_performed_by_this_script": False,
        "main_full_suite_raw_trx_parsed": False,
        "main_raw_trx_unavailable": {
            "git_blob": "b991fcbcc58e067bd45555a74b95604726987da9",
            "bytes": 22273912,
            "reason": "GitHub contents returned empty content; blob transport closed. No local raw TRX obtained.",
        },
        "input_hashes": {},
    }
    source_paths = [
        "Tests/ExperimentFixes/ExperimentEvidenceSink.cs",
        "Tests/ExperimentFixes/ExperimentEvidenceSinkTests.cs",
        "Tests/ExperimentFixes/P4801WeightingCurriculumCeilingTests.cs",
        "Tests/ExperimentFixes/P4901FinitenessGovernedWarmStartTests.cs",
        "Tests/ExperimentFixes/P5201RejectedAnnotationAddressArmsTests.cs",
        "Tests/ExperimentFixes/P5801CorrectiveLessonCounterExampleArmsTests.cs",
        "Tests/TestSuiteBatchManifest.cs",
        "Tests/TestSuiteBatchInventoryGuardTests.cs",
        "tools/test-batches/batches.tsv",
    ]
    artifact_paths = [
        *[p.relative_to(core).as_posix() for p in full.iterdir() if p.is_file()],
        *[p.relative_to(core).as_posix() for p in guard.iterdir() if p.is_file()],
    ]
    expected_blobs = {}
    for tree_name, prefix in [("core-Tests.json", "Tests/"), ("core-tools.json", "tools/"), ("core-artifacts_fbe.json", "artifacts/fbe/")]:
        tree = json.loads((args.trees / tree_name).read_text())
        for item in tree["tree"]:
            if item.get("type") == "blob":
                expected_blobs[prefix + item["path"]] = item["sha"]
    for relative in sorted(set(source_paths + artifact_paths)):
        path = core / relative
        if not path.exists():
            continue
        fp = fingerprint(path)
        expected = expected_blobs.get(relative)
        fp["expected_git_blob_sha1"] = expected
        fp["pinned_tree_match"] = fp["git_blob_sha1"] == expected if expected else None
        if expected and not fp["pinned_tree_match"]:
            raise ValueError(f"Pinned blob mismatch: {relative}")
        report["input_hashes"][relative] = fp

    author_counts = json.loads((full / "counts-two-ways.json").read_text())
    log_lines = (full / "full-run.log").read_text().splitlines()
    observed_failures = [re.sub(r" \[[^\]]+\]$", "", line.strip()[7:]) for line in log_lines if re.match(r"\s+Failed ", line)]
    observed_skips = [re.sub(r" \[[^\]]+\]$", "", line.strip()[8:]) for line in log_lines if re.match(r"\s+Skipped ", line)]
    terminal = next(line for line in reversed(log_lines) if re.match(r"Failed!.*Failed:", line))
    match = re.search(r"Failed:\s*(\d+), Passed:\s*(\d+), Skipped:\s*(\d+), Total:\s*(\d+)", terminal)
    terminal_counts = dict(zip(["Failed", "Passed", "Skipped", "Total"], map(int, match.groups())))
    failure_names = [row["testName"] for row in author_counts["failures"]]
    failed_methods = {method_name(name) for name in failure_names}
    failure_by_class = collections.Counter(method_name(name).rsplit(".", 1)[0] for name in failure_names)
    report["main_suite_historical_evidence"] = {
        "evidence_level": "Independent console recount plus author-generated JSON; NOT independent raw main TRX parse",
        "console_failed_rows": len(observed_failures),
        "console_skipped_rows": len(observed_skips),
        "console_failed_distinct_methods": len({method_name(name) for name in observed_failures}),
        "console_terminal_line": terminal,
        "console_terminal_counts": terminal_counts,
        "console_contains_test_run_aborted": any(line.strip() == "Test Run Aborted." for line in log_lines),
        "author_json_way_a_tally": author_counts["way_a_tally"],
        "author_json_way_b_counters": author_counts["way_b_counters"],
        "author_json_two_methods_agree": author_counts["two_methods_agree"],
        "author_json_failure_records": len(failure_names),
        "author_json_failure_distinct_methods": len(failed_methods),
        "failure_names_console_minus_json": sorted(set(observed_failures) - set(failure_names)),
        "failure_names_json_minus_console": sorted(set(failure_names) - set(observed_failures)),
        "observed_arithmetic_consistent": terminal_counts["Passed"] + terminal_counts["Failed"] + terminal_counts["Skipped"] == terminal_counts["Total"],
        "failure_by_class": dict(failure_by_class.most_common()),
        "coverage_complete_proven": False,
        "scope_reason": "Testhost abortion is present. Total counts recorded rows, not independent expected discovery. The author's final correction restores 501 as a lower bound.",
    }
    guard_runs, guard_records = {}, {}
    for name in ["guard-after-fix.trx", "guard-mutation-reverse.trx", "guard-after-restore.trx"]:
        guard_runs[name], guard_records[name] = parse_trx(guard / name)
    fixed = guard_records["guard-after-fix.trx"]
    mutated = guard_records["guard-mutation-reverse.trx"]
    restored = guard_records["guard-after-restore.trx"]
    full_guard_failed = {name for name in failure_names if ".TestSuiteBatchInventoryGuardTests." in name}
    mutation_failed = {name for name, row in mutated.items() if row["outcome"] == "Failed"}
    missing_classes = sorted({
        line.split("\t")[1]
        for row in fixed.values()
        for line in row["message"].splitlines()
        if line.startswith("new_test_class_without_strong_batch_signal:")
    })
    saved_missing = (guard / "remaining-cause-unassigned-classes.txt").read_text().splitlines()
    all_same_names = set(fixed) == set(mutated) == set(restored)
    report["guard_historical_raw_trx"] = {
        "evidence_level": "Independent direct XML parsing of three historical TRX files; not a fresh C# run",
        "runs": guard_runs,
        "same_test_name_sets_all_three_runs": all_same_names,
        "fix_and_restore_same_outcomes": all_same_names and all(fixed[n]["outcome"] == restored[n]["outcome"] for n in fixed),
        "historical_full_suite_guard_failures": len(full_guard_failed),
        "mutation_failed_names_equal_author_full_suite_guard_failed_names": mutation_failed == full_guard_failed,
        "failed_to_passed": sorted(n for n in fixed if mutated[n]["outcome"] == "Failed" and fixed[n]["outcome"] == "Passed"),
        "still_failed": sorted(n for n in fixed if mutated[n]["outcome"] == "Failed" and fixed[n]["outcome"] == "Failed"),
        "passed_to_failed": sorted(n for n in fixed if mutated[n]["outcome"] == "Passed" and fixed[n]["outcome"] == "Failed"),
        "missing_strong_signal_classes_from_trx": missing_classes,
        "missing_classes_match_separate_saved_list": set(missing_classes) == set(saved_missing),
    }
    batch_rows = []
    for line in (core / "tools/test-batches/batches.tsv").read_text().splitlines():
        if line.strip() and not line.startswith("#"):
            fields = line.split("\t")
            batch_rows.append({"batch": fields[1], "dll": fields[3], "filter": fields[4]})
    current_class_matches = {}
    for name in missing_classes:
        matches = []
        for row in batch_rows:
            if row["dll"] != "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll":
                continue
            for term in positive_filter_terms(row["filter"]):
                if term_matches(name, term):
                    matches.append({"batch": row["batch"], "positive_term": term})
        current_class_matches[name] = matches
    report["current_batch_prefix_check"] = {
        "evidence_level": "Source-derived positive-term comparison only; no C# project discovery executed",
        "physical_rows": len(batch_rows),
        "historical_missing_classes_checked": len(missing_classes),
        "classes_with_no_matching_positive_term_now": sum(not v for v in current_class_matches.values()),
        "matches": current_class_matches,
    }
    report["smaller_projects_historical_raw_trx"] = {}
    for name in ["designconversion-modelactivation.trx", "apidashboardwpf.trx"]:
        report["smaller_projects_historical_raw_trx"][name] = parse_trx(full / name)[0]
    report["smaller_projects_aggregation_warning"] = "Kept separate: linked source tests can duplicate the main project's coverage; do not add to a unique-test total."
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n")
    print(json.dumps({
        "output": str(args.output),
        "pinned_inputs_checked": len(report["input_hashes"]),
        "main_console": terminal_counts,
        "main_coverage_complete_proven": False,
        "guard_failed_to_passed": len(report["guard_historical_raw_trx"]["failed_to_passed"]),
        "guard_still_failed": len(report["guard_historical_raw_trx"]["still_failed"]),
        "missing_classes_still_without_positive_term": report["current_batch_prefix_check"]["classes_with_no_matching_positive_term_now"],
    }, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
