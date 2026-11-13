#!/usr/bin/env python3
"""Bounded projections of f95 ownership gaps. Does not execute C# or train a model."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import tempfile


PIN = "f95a96b324ca59a97abf4a2a3e5c5efdc4e65f64"
INPUTS = {
    "ExperimentEvidenceSink.cs": "69372a6a6a3a63c7d62d29dbda0fcd3215715144",
    "ExperimentAttemptOwnershipTests.cs": "963166aa5bbcae4e7c131d4bc649e8f92b1c9603",
    "P5801CorrectiveLessonCounterExampleArmsTests.cs": "a0e4f9156e4b852c37fbf44a8c30df99577b0a1a",
}


def read_sources(source_dir):
    sources = {}
    for name, expected in INPUTS.items():
        data = (source_dir / name).read_bytes()
        actual = hashlib.sha1(b"blob " + str(len(data)).encode() + b"\0" + data).hexdigest()
        if actual != expected:
            raise ValueError(f"Wrong source bytes: {name}: {actual}")
        sources[name] = data.decode("utf-8")
    return sources


def resolve_projection(root, relative):
    """ASCII/POSIX projection of lines 583-618, including OrdinalIgnoreCase."""
    if not relative.strip() or os.path.isabs(relative):
        raise ValueError("rejected path")
    candidate = Path(os.path.abspath(root / relative))
    boundary = str(root) + os.sep
    if not str(candidate).lower().startswith(boundary.lower()):
        raise ValueError("rejected path")
    return candidate


def successful_new_file_branch(root, relative, payload):
    """Only project the success branch for an absent destination; not File.Move internals."""
    path = resolve_projection(root, relative)
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("xb") as output:
        output.write(payload)
    return path


def run(sources):
    sink = sources["ExperimentEvidenceSink.cs"]
    p58 = sources["P5801CorrectiveLessonCounterExampleArmsTests.cs"]
    assert "if (!candidate.StartsWith(boundary, StringComparison.OrdinalIgnoreCase))" in sink
    assert "return WriteSingleShotBytes(AttemptCompletionFileName, Utf8WithoutByteOrderMark.GetBytes(exactPayload));" in sink
    writer = sink.split("private string WriteSingleShotBytes(", 1)[1].split("private static void TryDeleteOwnTemporaryFile", 1)[0]
    assert "AttemptCompletionFileName" not in writer and "TryReadMarkerRunIdentifier" not in writer
    resolver = sink.split("public string ResolvePath(", 1)[1].split("public string EnsureWriteRootDirectory", 1)[0]
    assert "return path;" in resolver and "File.Exists" not in resolver
    assert "private static readonly Lazy<ExperimentEvidenceSink> Evidence = new(() =>" in p58
    assert p58.count("Evidence.Value") == 1
    observations = []
    with tempfile.TemporaryDirectory(prefix="codex-f95-projection-") as tmp:
        parent = Path(tmp)
        root = parent / "attempt"
        sibling = parent / "ATTEMPT"
        root.mkdir()
        sibling.mkdir()
        if root.samefile(sibling):
            raise RuntimeError("Case-sensitive filesystem required for this bounded counterexample")
        legal = resolve_projection(root, "nested/legal.json")
        escaped = successful_new_file_branch(root, "../ATTEMPT/foreign.json", b"outside root\n")
        rejected_normal_escape = False
        try:
            resolve_projection(root, "../foreign/file.json")
        except ValueError:
            rejected_normal_escape = True
        assert escaped.parent.samefile(sibling) and rejected_normal_escape
        assert str(legal).startswith(str(root) + os.sep)
        observations.append({
            "id": "L1_CASE_SENSITIVE_SIBLING_ESCAPE",
            "evidence_kind": "path-guard translation plus real POSIX filesystem write",
            "case_sensitive_filesystem": True,
            "relative_path": "../ATTEMPT/foreign.json",
            "root_basename": "attempt",
            "case_insensitive_guard_accepts": True,
            "file_created_in_distinct_sibling": True,
            "lawful_nested_path_accepts": True,
            "ordinary_parent_escape_rejects": rejected_normal_escape,
            "case_sensitive_boundary_control_rejects": not str(escaped).startswith(str(root) + os.sep),
        })

        sealed = parent / "sealed"
        sealed.mkdir()
        marker = successful_new_file_branch(sealed, "владение-попыткой.json", b'{"run_id":"owner-A"}\n')
        completed = successful_new_file_branch(sealed, "завершение-попытки.json", b'{"status":"done"}\n')
        late = successful_new_file_branch(sealed, "late-raw.json", b'{"after_completion":true}\n')
        duplicate_completion_rejected = False
        try:
            successful_new_file_branch(sealed, "завершение-попытки.json", b"replacement")
        except FileExistsError:
            duplicate_completion_rejected = True
        assert duplicate_completion_rejected and marker.exists() and completed.exists() and late.exists()
        observations.append({
            "id": "L2_COMPLETION_DOES_NOT_SEAL_WRITER",
            "evidence_kind": "successful absent-target write branch projection; no File.Move runtime claim",
            "completion_exists_before_later_write": True,
            "new_raw_name_written_after_completion": True,
            "duplicate_completion_name_rejected_control": duplicate_completion_rejected,
        })

        bypass = parent / "path-api"
        bypass.mkdir()
        raw = successful_new_file_branch(bypass, "frozen-raw.json", b"original bytes\n")
        before = hashlib.sha256(raw.read_bytes()).hexdigest()
        returned = resolve_projection(bypass, "frozen-raw.json")
        returned.write_text("replacement through caller write\n", encoding="utf-8")
        after = hashlib.sha256(raw.read_bytes()).hexdigest()
        assert before != after
        observations.append({
            "id": "L3_RESOLVED_PATH_BYPASSES_SINGLE_SHOT_WRITE",
            "evidence_kind": "ResolvePath translation plus real caller-level replacing write",
            "existing_raw_path_returned": returned == raw,
            "raw_bytes_changed": before != after,
            "original_sha256": before,
            "replacement_sha256": after,
            "owner_marker_path_also_accepted": resolve_projection(bypass, "владение-попыткой.json").parent == bypass,
        })

    main = p58.index("public void PreparedCorpusLineAndActualWrongOutputAreMeasuredOnBothAxesSeparately()")
    train = p58.index('var setupLessons = TrainBatch(setupUnits, BuildSetupBatch("P5801-setup-probe"));', main)
    first_write = p58.index('WriteEvidence("setup-probe.json"', main)
    assert train < first_write and "Evidence.Value" not in p58[main:first_write]
    observations.append({
        "id": "L4_P58_FIRST_SINK_ACCESS_FOLLOWS_SETUP_TRAIN",
        "evidence_kind": "source-order check only; no model execution",
        "train_line": p58[:train].count("\n") + 1,
        "first_evidence_write_line": p58[:first_write].count("\n") + 1,
        "only_Evidence_Value_site_line": p58[:p58.index("Evidence.Value")].count("\n") + 1,
        "claim_before_setup_train_not_established_for_ordinary_main": True,
        "scope": "fresh class/process running this main Fact without earlier sink access",
    })
    return {
        "source_commit": PIN,
        "source_git_blobs": INPUTS,
        "native_tests_executed": 0,
        "model_training_calls": 0,
        "observation_records": len(observations),
        "observations": observations,
        "conclusion": "FOUR_SCOPED_REMAINING_GAPS_OBSERVED_NOT_PRODUCT_ACCEPTANCE",
        "limitations": [
            "No C# or PowerShell execution; source pin and bounded Python projection only.",
            "Case example uses ASCII POSIX paths on a case-sensitive filesystem.",
            "Successful new-file branch does not emulate atomic File.Move internals or crash durability.",
            "Existing native ownership receipts retain their own accepted scope.",
            "The 13-caller regression suite was not discovered or executed here.",
        ],
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--source-dir", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with args.output.open("x", encoding="utf-8") as output:
        result = run(read_sources(args.source_dir))
        json.dump(result, output, ensure_ascii=False, indent=2)
        output.write("\n")
    print(result["conclusion"])


if __name__ == "__main__":
    main()
