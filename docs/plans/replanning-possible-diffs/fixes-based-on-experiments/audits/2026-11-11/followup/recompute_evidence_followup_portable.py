#!/usr/bin/env python3
"""Recount pinned historical text/XML receipts; no subprocess, network or models.

Use --core for a checkout at the pinned commit and --output for a NEW JSON.
Inputs are checked against embedded Git blob identities. Existing output is
refused; no fetched manifests or local scratch metadata are required.
This does not claim fresh native test results.
"""
import argparse
import csv
import hashlib
import json
import re
import statistics
import xml.etree.ElementTree as ET
from collections import Counter
from pathlib import Path

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--core", type=Path, required=True, help="Repository root containing the pinned inputs")
parser.add_argument("--output", type=Path, required=True, help="New JSON file; must not already exist")
args = parser.parse_args()
if not __debug__:
    parser.error("Do not use Python -O: this recount requires its assertions enabled")
CORE = args.core.resolve()
OUTPUT = args.output.resolve()
if not CORE.is_dir():
    parser.error("--core must be an existing repository directory")
if OUTPUT.exists():
    parser.error("--output already exists; choose a new output path (no overwrite)")
COMMIT = "55cfd9d00bbebdeae04da7c505b84d0b1cb9b747"
FULL = "artifacts/fbe/FULL-SUITE-20261110/attempt-0001/"
GUARD = "artifacts/fbe/FIX-BATCH-INVENTORY-GUARD/attempt-0001/"
ROW = "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/"
expected = {
    "Tests/ExperimentFixes/ExperimentEvidenceSink.cs": "1066100b3c1d645f04de027c2480720b18663987",
    "Tests/ExperimentFixes/ExperimentEvidenceSinkTests.cs": "45518e7ed7c05d3b63dfd1fc845f99c01c47351b",
    "Tests/ExperimentFixes/P4801WeightingCurriculumCeilingTests.cs": "8d9eac7def423d344c1146449912a2c49b022af2",
    "Tests/ExperimentFixes/P4901FinitenessGovernedWarmStartTests.cs": "b9fcddbcb2a5983c7d0bb04e6d8691d5bc83868e",
    "Tests/ExperimentFixes/P5201RejectedAnnotationAddressArmsTests.cs": "db97f983f64bd8822c85213d8b5b8ba3b87abac5",
    "Tests/ExperimentFixes/P5801CorrectiveLessonCounterExampleArmsTests.cs": "a0e4f9156e4b852c37fbf44a8c30df99577b0a1a",
    "Tests/TestSuiteBatchManifest.cs": "f4f653ff45916f1bd48ae26cde84a671bf149cea",
    "Tests/TestSuiteBatchInventoryGuardTests.cs": "987bb002708e036dcfd11e129e87ce1014f3def3",
    "tools/test-batches/batches.tsv": "a4ed0ee4970df7a0abb61147a2b9535398a3b8cf",
    "tools/test-batches/run-test-batches.ps1": "95e56c103260002bacbe17190fc6d91922598eb9",
    "tools/test-batches/summarize-test-batches.ps1": "fc59159efb3fec3da02961f6c1dadfe0576ac2e7",
    FULL + "counts-two-ways.json": "c1d7732b59d9382ffd86b8faef9d57c864f65be5",
    FULL + "full-run.log": "dcdab8a9e6875776b320b94969adcdbd4409bcc6",
    FULL + "claims-boundaries.md": "4a96881908fe9599f29968edd46f0b4239a6b511",
    FULL + "run-exit.txt": "4767c7d23ecda8f197debc3171611cf33ec56907",
    GUARD + "guard-after-fix.trx": "1b60427c9f8ac910cf4f882e2cdee1380dcc8eb1",
    GUARD + "guard-after-restore.trx": "05b9874123e75e4001d984580d37b049632448cf",
    GUARD + "guard-mutation-reverse.trx": "23df33567959971e79f9417529f868cc9d4c9104",
    GUARD + "remaining-cause-unassigned-classes.txt": "96a0a07b5d306293e892f8462950da15427efbf8",
}
expected.update({
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/README-class-proof-journal.md": "b5be4341658d5604562b55f4fcf9b2d456eea157",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/batch-01-detail.tsv": "d8ccbd470c1f848ef2651fcae8d576a2aaeb3312",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/batch-02-detail.tsv": "0a702148c371f5fb38bf5551c62e86723a3cebe8",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/batch-03-detail.tsv": "d1fd443829af200914491ef85ef4d6a36c0c7900",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/batch-04-detail.tsv": "428704f7bb1b97c31d7464c3e07d0c078885eaf7",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/batch-05-detail.tsv": "ee355b2b5e79aa66ce216305c4dda238e6f51b42",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/batch-06-detail.tsv": "c0e2efec638730f9c1838c80e03eed178b5c5821",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/batch-07-detail.tsv": "e7990735b8ca33817c399528fdeb989f25ae14c7",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/batch-08-detail.tsv": "9fc485bb782b178b2af64474f3440b50bbc290cb",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/batch-09-detail.tsv": "326054656d793e2cdf096f3e79b6be1c18d9c6cc",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/batch-10-detail.tsv": "227a657ac951135b6682920a559a5be2731379a9",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/batch-11-detail.tsv": "fd18e288ff759b27590d4e9dd377499ff4f51ef0",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/batch-12-detail.tsv": "5a8c9a6b0e428501d8cca95cf74236104754f49c",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/batch-13-detail.tsv": "fe2aa61a5bba083f84eab38790a86837c2563165",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/batch-14-detail.tsv": "b28c1e5c105396646dfdb9f0636d9552bec3ebe3",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/batch-15-detail.tsv": "30d577ea2f6f837a1096f230ea25f4325be3601d",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/batch-16-detail.tsv": "2bb815d4025b617db06e900e996bc28644a1c584",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/batch-summary.tsv": "73add0a21b26f5d292355d6e454fda59fb6a0948",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/class-other-list.tsv": "097064cf550d6472e5d8dea0e842034ee6766b21",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/evidence-bucket-table.tsv": "e6b50298373cbf327188a3796d69711b256882c8",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/git-index-fingerprint-before.txt": "261e01fb29ae3a394643c0a347f9ade6eb5946a9",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/git-index-movement-observation.txt": "736d589c12113b90f10b2ca8908de8d0af1efd04",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/git-pointer-movement-report.txt": "53f646b830e72fa41de766705c749ba559fbc118",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/method-byte-level-demonstration.txt": "104dbbb016373166e6e1256a521da860f9f48ed6",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/method-independent-tool-crosscheck.txt": "838045bea8fa1ab2a90997099c2770ed4da91bb8",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/method-reference-vectors.txt": "f6a8b7259acd4037519916de665ffcd6f19e2dd8",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/method-streaming-selftest.txt": "dd834c9d7f37859f957b6de9f22567896c57afff",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/object-missing-list.tsv": "81c6ba834ef2ef38734313250782639fe271bbdd",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/pointer-pairs.tsv": "c65e86de71985918776642ee2d2fd5fb916533dc",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/scope-and-distribution.txt": "3b95ac3df828a2e5a9f421414dff9bc0b455e047",
    "artifacts/fbe/ROW124-CLASS-PROOF/attempt-0001/selectivity-honesty-check.txt": "638c3c1f1775cb36c361d9e36cb276230dec15a0"
})


def fingerprint(path):
    b = path.read_bytes()
    return {"bytes": len(b), "sha256": hashlib.sha256(b).hexdigest(),
            "git_blob_sha1": hashlib.sha1(b"blob " + str(len(b)).encode() + b"\0" + b).hexdigest()}


input_hashes = {}
for path, blob in sorted(expected.items()):
    value = fingerprint(CORE / path)
    assert value["git_blob_sha1"] == blob, (path, value, blob)
    value["pinned_blob_verified"] = True
    input_hashes[path] = value


def read(path):
    return (CORE / path).read_text(encoding="utf-8-sig")


def records(path):
    lines = (line for line in read(path).splitlines() if line.strip() and not line.startswith("#"))
    return list(csv.DictReader(lines, delimiter="\t"))


def method(name):
    return name.split("(", 1)[0]


log = read(FULL + "full-run.log")
failures, skipped = [], []
for line in log.splitlines():
    match = re.match(r"^\s+(Failed|Skipped) (.+) \[[^\]]+\]$", line)
    if match:
        (failures if match[1] == "Failed" else skipped).append(match[2])
terminal_rows = re.findall(r"Failed!.*?Failed:\s*(\d+), Passed:\s*(\d+), Skipped:\s*(\d+), Total:\s*(\d+)", log)
assert len(terminal_rows) == 1, terminal_rows
terminal = dict(zip(("Failed", "Passed", "Skipped", "Total"), map(int, terminal_rows[0])))
author = json.loads(read(FULL + "counts-two-ways.json"))
author_names = [r["testName"] for r in author["failures"]]
assert Counter(failures) == Counter(author_names)
assert terminal["Failed"] == len(failures)
assert terminal["Skipped"] == len(skipped)
assert sum(terminal[k] for k in ("Failed", "Passed", "Skipped")) == terminal["Total"]
main_result = {
    "independent_evidence": "console lines and terminal; author JSON is a corroborating derivative, not raw main TRX",
    "console_failed_rows": len(failures), "console_skipped_rows": len(skipped),
    "console_distinct_failed_methods": len({method(n) for n in failures}),
    "console_terminal": terminal,
    "test_run_aborted_line_present": "Test Run Aborted." in log.splitlines(),
    "run_exit_receipt": read(FULL + "run-exit.txt"),
    "author_json_failure_name_multiset_matches_console": True,
    "author_json_way_a_tally": author["way_a_tally"],
    "author_json_way_b_counters": author["way_b_counters"],
    "author_json_two_methods_agree": author["two_methods_agree"],
    "failed_result_rows_by_class": dict(Counter(method(n).rsplit(".", 1)[0] for n in failures).most_common()),
    "coverage_complete": False,
    "raw_main_trx_obtained": False,
    "raw_main_trx_unavailable": {"blob": "b991fcbcc58e067bd45555a74b95604726987da9", "bytes": 22273912,
                                 "reason": "two fetch_blob calls returned Transport closed; no payload obtained"},
}


def parse_trx(path):
    root = ET.fromstring((CORE / path).read_bytes())
    rows = root.findall(".//{*}UnitTestResult")
    data = {}
    for r in rows:
        name = r.attrib["testName"]
        assert name not in data, (path, name)
        data[name] = {"outcome": r.attrib["outcome"],
                      "message": r.findtext(".//{*}Message", default="")}
    summary = root.find(".//{*}ResultSummary")
    counters = {k: int(v) for k, v in summary.find("{*}Counters").attrib.items()}
    assert counters["total"] == len(rows)
    failed = [x for x in data.values() if x["outcome"] == "Failed"]
    info = {"rows": len(rows), "outcomes": dict(Counter(r["outcome"] for r in data.values())),
            "result_summary_outcome": summary.attrib.get("outcome"), "counters": counters,
            "failed_message_markers": {m: sum(m in x["message"] for x in failed)
                for m in ("TargetFramework", ".kiro", "new_test_class_without_strong_batch_signal")}}
    return info, data


guard_info, guard_rows = {}, {}
for name in ("guard-after-fix.trx", "guard-mutation-reverse.trx", "guard-after-restore.trx"):
    guard_info[name], guard_rows[name] = parse_trx(GUARD + name)
fixed, reversed_rows, restored = (guard_rows[n] for n in ("guard-after-fix.trx", "guard-mutation-reverse.trx", "guard-after-restore.trx"))
assert set(fixed) == set(reversed_rows) == set(restored)
assert all(fixed[n]["outcome"] == restored[n]["outcome"] for n in fixed)
transitions = Counter((reversed_rows[n]["outcome"], fixed[n]["outcome"]) for n in fixed)
missing_classes = sorted({line.split("\t")[1] for row in fixed.values() for line in row["message"].splitlines()
                          if line.startswith("new_test_class_without_strong_batch_signal:")})
saved_classes = read(GUARD + "remaining-cause-unassigned-classes.txt").splitlines()
assert set(saved_classes) == set(missing_classes)
batch_rows = [line.split("\t") for line in read("tools/test-batches/batches.tsv").splitlines()
              if line.strip() and not line.startswith("#")]


def positive_terms(filter_text):
    for piece in re.split(r"[|&]", filter_text):
        term = piece.strip()
        while term.startswith("("):
            term = term[1:].strip()
        while term.endswith(")"):
            term = term[:-1].strip()
        if term.startswith("FullyQualifiedName~"):
            yield term[len("FullyQualifiedName~"):]


def term_matches(name, term):
    a, b = name.casefold(), term.casefold()
    return (a == b[:-1] or a.startswith(b)) if b.endswith(".") else b in a


class_matches = {}
for name in missing_classes:
    class_matches[name] = [{"batch": row[1], "term": term} for row in batch_rows
                           if row[3] == "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll"
                           for term in positive_terms(row[4]) if term_matches(name, term)]
guard_result = {
    "evidence_level": "independent XML parsing of saved TRX; no fresh C# run",
    "runs": guard_info, "same_60_names": len(fixed) == 60,
    "fix_restore_outcomes_equal": True,
    "mutation_to_fix_transitions": {a + "->" + b: n for (a, b), n in sorted(transitions.items())},
    "mutation_failed_names_equal_main_author_json_guard_failures":
        {n for n, r in reversed_rows.items() if r["outcome"] == "Failed"} ==
        {n for n in author_names if ".TestSuiteBatchInventoryGuardTests." in n},
    "missing_classes_from_trx": missing_classes, "saved_missing_list_matches": True,
    "physical_batch_manifest_rows": len(batch_rows),
    "main_dll_manifest_rows": sum(row[3] == "Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll" for row in batch_rows),
    "source_positive_term_matches": class_matches,
    "classes_without_positive_term": sum(not matches for matches in class_matches.values()),
    "source_check_limit": "positive signal only; no current discovery, C# first-match scheduler or full suite executed",
}

pair_rows = records(ROW + "pointer-pairs.tsv")
pair_map = {r["path"]: r for r in pair_rows}
assert len(pair_map) == len(pair_rows)
details = [r for n in range(1, 17) for r in records(ROW + f"batch-{n:02d}-detail.tsv")]
detail_map = {r["path"]: r for r in details}
assert len(detail_map) == len(details)
assert set(detail_map) == set(pair_map)
bad = []
for path, p in pair_map.items():
    d = detail_map[path]
    pre, now = int(p["pre_size"]), int(p["now_size"])
    if not (int(p["delta"]) == now-pre and int(d["pre_declared_bytes"]) == pre
            and int(d["bytes_removed"]) == pre-now and d["note"] == "result_bytes=" + str(now)):
        bad.append(path)
assert not bad, bad
batch_summary = records(ROW + "batch-summary.tsv")
remaining = len(pair_rows)
for row in batch_summary:
    sub = records(ROW + f"batch-{int(row['batch']):02d}-detail.tsv")
    remaining -= len(sub)
    assert int(row["processed"]) == len(sub)
    assert int(row["class_proven"]) == sum(r["verdict"] == "PROVEN" for r in sub)
    assert int(row["class_other"]) == int(row["object_missing"]) == 0
    assert int(row["remaining"]) == remaining
removed = [int(r["bytes_removed"]) for r in details]
bucket_rows = records(ROW + "evidence-bucket-table.tsv")
bucket_columns = list(bucket_rows[0])
expected_buckets = Counter(r["pre_oid"][:2] for r in pair_rows)
actual_buckets = {r[bucket_columns[0]]: int(r[bucket_columns[1]]) for r in bucket_rows}
assert actual_buckets == expected_buckets
assert all(int(r[bucket_columns[1]]) == int(r[bucket_columns[2]]) for r in bucket_rows)
pair_count = len({(r["pre_oid"], r["now_oid"]) for r in pair_rows})
row_result = {
    "evidence_level": "independent recount/join of saved historical text receipts; binary transformations NOT rerun",
    "detail_rows": len(details), "unique_paths": len(pair_map),
    "verdicts": dict(Counter(r["verdict"] for r in details)), "distinct_object_pairs": pair_count,
    "distinct_pre_oids": len({r["pre_oid"] for r in pair_rows}),
    "distinct_now_oids": len({r["now_oid"] for r in pair_rows}),
    "removed_bytes_from_detail": sum(removed),
    "size_delta_total_from_pointers": sum(int(r["pre_size"])-int(r["now_size"]) for r in pair_rows),
    "removed_bytes_min_median_max": [min(removed), statistics.median(removed), max(removed)],
    "per_path_sizes_and_detail_join_agree": True, "batch_rows": len(batch_summary),
    "bucket_rows": len(bucket_rows), "buckets_match_pointer_pre_oid_prefixes": True,
    "affected_paths_under_training_artifacts": sum(r["path"].startswith("training-artifacts/") for r in pair_rows),
    "other_list_rows": len(records(ROW + "class-other-list.tsv")),
    "object_missing_list_rows": len(records(ROW + "object-missing-list.tsv")),
    "historical_source_commits_from_receipt": ["68f0ce4949958b6d560b1630626fdfcc2af8f0dc", "187b54b8844a3cdce7d6c931afe0aa7dca67d9a0"],
    "historical_transform_claim": "remove 0x0D only immediately before 0x0A and compare resulting SHA256 with current pointer oid",
    "historical_claim_limit": "39 added paths excluded; 2027 pair-level hashes were reported, not independently recomputed here",
    "restoration_performed_or_current_restoration_status_proven": False,
    "receipt_reproducibility_limit": "31-file proof package contains no original executable transform script and detail columns omit observed output SHA256; PROVEN is the author's recorded predicate",
}

for path, before in input_hashes.items():
    assert fingerprint(CORE / path)["sha256"] == before["sha256"], path
report = {"audit_source_commit": COMMIT, "subagent_id_or_NONE": "/root/skills_scope",
          "native_runs": 0, "Train": 0, "Predict": 0, "Flow": 0, "Load": 0,
          "input_files_unchanged_after_recount": True, "input_hashes": input_hashes,
          "script_sha256": hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),
          "main_suite": main_result, "guard": guard_result, "row124": row_result}
output = OUTPUT
output.parent.mkdir(parents=True, exist_ok=True)
with output.open("x", encoding="utf-8", newline="\n") as handle:
    handle.write(json.dumps(report, ensure_ascii=False, indent=2) + "\n")
print(json.dumps({"output": str(output), "pinned_inputs": len(input_hashes),
                  "main_terminal": terminal, "guard_transitions": guard_result["mutation_to_fix_transitions"],
                  "unmatched_classes": guard_result["classes_without_positive_term"],
                  "row124_paths": len(pair_map), "row124_pairs": pair_count,
                  "row124_removed_bytes": sum(removed)}, indent=2))
