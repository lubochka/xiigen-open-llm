#!/usr/bin/env python3
"""Bounded source-formula witnesses. This is not a .NET or learning run.

The fixture supplies roles and constituent spans as diagnostic annotations.
It does not claim the production tokenizer/parser can infer those annotations.
Read the pinned files with --repo /path/to/existing/llm_mvp_core, or use
--source-root /path/to/snapshots containing current/ and clean/ subdirectories.
Every required source must match the expected source-manifest.json commit,
Git blob SHA, SHA256 and byte count before any proxy calculation or output.
"""
from __future__ import annotations

import hashlib
import json
import argparse
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parent
PINNED_COMMITS = {
    "current": "358af351e22a92a3f7938d1f9221d5aeb7ddb053",
    "clean": "e346b88a3cc1fcce48aefa626bfc7b00509eefd6",
}
SOURCE_PATHS = (
    "12.SemanticCompiler/Translation/PatternClassifier.cs",
    "12.SemanticCompiler/Translation/PatternReorderer.cs",
    "12.SemanticCompiler/Prediction/MethodKnn.cs",
    "35.ExperimentsV2/Infrastructure/PatternReorderer.cs",
)
SVO, SOV, VSO, IMPERATIVE_V, IMPERATIVE_VS = 0, 1, 2, 3, 4


def read_verified_sources(repo, source_root, manifest_path):
    """Fail closed against expected blobs; branch parity alone is insufficient."""
    manifest_bytes = manifest_path.read_bytes()
    manifest = json.loads(manifest_bytes)
    if not isinstance(manifest, list):
        raise ValueError("Expected a list of source-manifest records")
    expected = {}
    for label, commit in PINNED_COMMITS.items():
        for path in SOURCE_PATHS:
            matches = [row for row in manifest if isinstance(row, dict)
                       and row.get("label") == label and row.get("path") == path]
            if len(matches) != 1:
                raise ValueError(f"Expected one manifest record for {label}:{path}")
            record = matches[0]
            if record.get("sha") != commit:
                raise ValueError(f"Pinned commit mismatch for {label}:{path}")
            for field, length in (("git_blob_sha", 40), ("sha256", 64)):
                value = record.get(field)
                if (not isinstance(value, str) or len(value) != length
                        or any(char not in "0123456789abcdef" for char in value)):
                    raise ValueError(f"Invalid expected {field} for {label}:{path}")
            if type(record.get("byte_length")) is not int or record["byte_length"] < 0:
                raise ValueError(f"Invalid expected byte_length for {label}:{path}")
            expected[(label, path)] = record

    sources = {}
    verification = []
    for (label, path), record in expected.items():
        if repo is not None:
            fetched = subprocess.run(
                ["git", "-C", str(repo), "show", f"{record['sha']}:{path}"],
                check=False, capture_output=True,
            )
            if fetched.returncode:
                raise ValueError(f"git show could not read pinned source {label}:{path}")
            raw = fetched.stdout
        else:
            raw = (source_root / label / path).read_bytes()
        actual_blob = hashlib.sha1(b"blob " + str(len(raw)).encode() + b"\0" + raw).hexdigest()
        actual_sha256 = hashlib.sha256(raw).hexdigest()
        if (actual_blob != record["git_blob_sha"] or actual_sha256 != record["sha256"]
                or len(raw) != record["byte_length"]):
            raise ValueError(f"SOURCE_HASH_MISMATCH {label}:{path}; proxy not executed")
        sources[(label, path)] = raw
        verification.append({"label": label, "commit": record["sha"], "path": path,
                             "git_blob_sha": actual_blob, "sha256": actual_sha256,
                             "byte_length": len(raw), "matched_expected_manifest": True})
    return sources, verification, hashlib.sha256(manifest_bytes).hexdigest()


def legacy_reorder(words, pattern):
    result = list(words)
    if pattern == IMPERATIVE_VS:
        return [word for word in result if word[1] != 3]
    if pattern == VSO:
        subject = next((i for i, word in enumerate(result) if i >= 1 and word[1] in (7, 3)), -1)
        if subject > 0:
            result.insert(0, result.pop(subject))
    return result


def annotated_reorder(words, pattern, subject_span):
    """Proposed local alternative using supplied, independently reviewed span."""
    result = list(words)
    if pattern == IMPERATIVE_VS:
        if subject_span is None:
            return result
        start, end = subject_span
        return result[:start] + result[end:]
    if pattern == VSO and subject_span is not None:
        start, end = subject_span
        return result[start:end] + result[:start] + result[end:]
    return result


def is_svo(words):
    first_verb = next((i for i, w in enumerate(words) if w[1] == 8), -1)
    first_noun = next((i for i, w in enumerate(words) if w[1] in (7, 3)), -1)
    return first_verb == -1 or first_noun == -1 or first_noun < first_verb


def old_canonical(kinds):
    kinds = set(kinds)
    if kinds & {"Try", "Catch"}:
        return "ErrorHandling"
    if {"If", "Ret"} <= kinds:
        return "Guard"
    if kinds & {"ForEach", "For"}:
        return "Loop"
    if "Await" in kinds:
        return "Async"
    return "Simple" if not kinds else "Mixed"


def broad_loop_canonical(kinds):
    kinds = set(kinds)
    if kinds & {"Try", "Catch"}:
        return "ErrorHandling"
    if {"If", "Ret"} <= kinds:
        return "Guard"
    if kinds & {"ForEach", "For", "While", "Do"}:
        return "Loop"
    if "Await" in kinds:
        return "Async"
    return "Simple" if not kinds else "Mixed"


def text(words):
    return " ".join(w[0] for w in words)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    inputs = parser.add_mutually_exclusive_group()
    inputs.add_argument("--repo", type=Path, help="Existing checkout; reads pinned objects with git show only")
    inputs.add_argument("--source-root", type=Path, default=ROOT / "source",
                        help="Snapshot root with current/ and clean/ directories")
    parser.add_argument("--manifest", type=Path, default=ROOT / "source-manifest.json",
                        help="Expected immutable source hashes and commit identities")
    parser.add_argument("--output", type=Path, default=ROOT / "knn_structural_proxy_results.json")
    args = parser.parse_args()
    try:
        sources, verification, manifest_sha256 = read_verified_sources(
            args.repo, args.source_root, args.manifest)
    except (OSError, ValueError) as error:
        parser.error(str(error))
    reorder_source = sources[("current", "12.SemanticCompiler/Translation/PatternReorderer.cs")].decode("utf-8")
    method_source = sources[("current", "12.SemanticCompiler/Prediction/MethodKnn.cs")].decode("utf-8")
    assert "words.Where(w => w.PosTag != 3).ToList()" in reorder_source
    assert "result.FindIndex(1, w => w.PosTag == 7 || w.PosTag == 3)" in reorder_source
    assert 'if (kinds.Contains("ForEach") || kinds.Contains("For"))' in method_source
    source_equality = []
    for path in ("Translation/PatternClassifier.cs", "Translation/PatternReorderer.cs", "Prediction/MethodKnn.cs"):
        a = sources[("current", "12.SemanticCompiler/" + path)]
        b = sources[("clean", "12.SemanticCompiler/" + path)]
        assert a == b
        source_equality.append({"path": "12.SemanticCompiler/" + path, "current_clean_identical": True,
                                "sha256": hashlib.sha256(a).hexdigest()})
    helper_path = "35.ExperimentsV2/Infrastructure/PatternReorderer.cs"
    helper_current = sources[("current", helper_path)]
    helper_clean = sources[("clean", helper_path)]
    assert helper_current == helper_clean
    assert b"words.Where(w => w.PosTag != 3).ToList()" in helper_current
    source_equality.append({"path": helper_path, "current_clean_identical": True,
                            "sha256": hashlib.sha256(helper_current).hexdigest()})

    # The first case is exactly the donor E27 case, not a newly generated lesson.
    pronoun_cases = [
        ("donor_single_subject", [("Add",8),("you",3),("check",7),("to",1),("method",7)], IMPERATIVE_VS, (1,2), "Add check to method"),
        ("subject_object_recipient", [("Send",8),("you",3),("it",3),("to",1),("her",3)], IMPERATIVE_VS, (1,2), "Send it to her"),
        ("subject_indirect_object", [("Tell",8),("you",3),("him",3),("about",1),("it",3)], IMPERATIVE_VS, (1,2), "Tell him about it"),
        ("implicit_subject_control", [("Send",8),("it",3),("to",1),("her",3)], IMPERATIVE_V, None, "Send it to her"),
    ]
    constituent_cases = [
        ("donor_pronoun_subject", [("add",8),("I",3),("check",7)], VSO, (1,2), "I add check"),
        ("adjective_subject", [("reads",8),("old",9),("server",7),("file",7)], VSO, (1,3), "old server reads file"),
        ("determiner_adjective_subject", [("reads",8),("the",0),("old",9),("server",7),("file",7)], VSO, (1,4), "the old server reads file"),
        ("single_noun_control", [("reads",8),("server",7),("file",7)], VSO, (1,2), "server reads file"),
    ]

    def examine(cases):
        result = []
        for name, words, pattern, span, expected in cases:
            old = legacy_reorder(words, pattern)
            new = annotated_reorder(words, pattern, span)
            assert text(new) == expected
            result.append({"case": name, "words": words, "source_pattern": pattern, "diagnostic_subject_span": span,
                           "expected_annotation_contract": expected, "legacy": text(old), "proposed_with_supplied_span": text(new),
                           "legacy_matches_annotation": text(old) == expected,
                           "legacy_is_svo": is_svo(old), "proposed_matches_annotation": text(new) == expected})
        return result

    loop_cases = [
        {"kind_bag": kinds, "legacy": old_canonical(kinds), "proposed_broad_loop": broad_loop_canonical(kinds)}
        for kinds in (["While","Call"], ["Do","Call"], ["For","Call"], ["ForEach","Call"], ["Call"])
    ]
    # This witness needs no inference over string syntax: the actual public API consumes a KindBag.
    assert [r["legacy"] for r in loop_cases] == ["Mixed", "Mixed", "Loop", "Loop", "Mixed"]

    result = {
        "evidence_kind": "SOURCE_FORMULA_PROXY_ONLY",
        "native_dotnet_execution": False,
        "new_production_training": False,
        "measured_learning_quality": False,
        "diagnostic_examples_are_training_data": False,
        "annotation_origin": "Assistant-authored diagnostic contracts; no teacher or judge runtime executed",
        "source_refs": {"current": PINNED_COMMITS["current"], "DesignToCode-clean": PINNED_COMMITS["clean"]},
        "source_verification": {"status": "ALL_REQUIRED_BLOBS_MATCH_EXPECTED_MANIFEST",
                                "manifest_sha256": manifest_sha256, "files": verification},
        "source_equality": source_equality,
        "pronoun_roles": examine(pronoun_cases),
        "subject_constituents": examine(constituent_cases),
        "loop_label_contract": loop_cases,
    }
    output = args.output
    output.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n")
    print(json.dumps({"output": str(output), "evidence_kind": result["evidence_kind"],
                      "pronoun_annotation_failures_legacy": sum(not r["legacy_matches_annotation"] for r in result["pronoun_roles"]),
                      "constituent_annotation_failures_legacy": sum(not r["legacy_matches_annotation"] for r in result["subject_constituents"]),
                      "harmful_constituent_cases_passing_is_svo": sum(not r["legacy_matches_annotation"] and r["legacy_is_svo"] for r in result["subject_constituents"]),
                      "loop_label_differences": sum(r["legacy"] != r["proposed_broad_loop"] for r in loop_cases)}, ensure_ascii=False))


if __name__ == "__main__":
    main()
