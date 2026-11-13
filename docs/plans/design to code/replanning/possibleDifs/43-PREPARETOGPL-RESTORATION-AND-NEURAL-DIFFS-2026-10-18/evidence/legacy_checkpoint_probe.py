"""Source/data comparison only. This does not execute TranslationMachine or migrate it.

Run on the two pinned core worktrees:
  python legacy_checkpoint_probe.py --baseline-root BASE --current-root CURRENT --output RESULT.json
The three original Git blobs are checked before extracting runtime direction sets.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import re
from pathlib import Path

BASE = "31346d8928be4469b01aaf313780e8f950ff91e2"
NOW = "a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6"
MACHINE = "27.TranslationDomain/TranslationMachine.cs"
CHECKPOINT = "models/translation/translation-v1/translation-v1-checkpoint.json"
EXPECTED = {
    "baseline_machine": "4b94eb7967410ca2cd2d41ec3cd6a5f4dd21ead7",
    "current_machine": "fd0e736de3739236b4dde2d6b9d000f5ebae8416",
    "checkpoint": "a4cf054ba3f8a6ee6aa9818edc6c5705e3456c25",
}


def checked_bytes(path: Path, expected: str) -> bytes:
    raw = path.read_bytes()
    actual = hashlib.sha1(f"blob {len(raw)}\0".encode() + raw).hexdigest()
    if actual != expected:
        raise ValueError(f"Original Git bytes required: {path.name}, expected {expected}, got {actual}")
    return raw


def runtime_directions(source: str) -> list[str]:
    body = source.split("private static TranslationRuntime CreateRuntime()", 1)[1]
    found = re.findall(r"new ExecutorUnit\(TranslationDirection\.(\w+), knnStore\)", body)
    if not found or len(found) != len(set(found)):
        raise ValueError("Unexpected runtime source structure")
    return sorted(found)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--baseline-root", type=Path, required=True)
    parser.add_argument("--current-root", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    old_source = checked_bytes(args.baseline_root / MACHINE, EXPECTED["baseline_machine"]).decode("utf-8-sig")
    new_source = checked_bytes(args.current_root / MACHINE, EXPECTED["current_machine"]).decode("utf-8-sig")
    raw = checked_bytes(args.baseline_root / CHECKPOINT, EXPECTED["checkpoint"])
    current_raw = checked_bytes(args.current_root / CHECKPOINT, EXPECTED["checkpoint"])
    assert current_raw == raw
    data = json.loads(raw.decode("utf-8-sig"))
    recorded = sorted(e["direction"] for e in data["executors"])
    old, new = runtime_directions(old_source), runtime_directions(new_source)
    assert len(recorded) == len(set(recorded))
    assert data["schema_version"] == 1
    assert "checkpoint.SchemaVersion != 1" in old_source
    assert "checkpoint.SchemaVersion != 1" in new_source
    assert "EXECUTOR_DIRECTIONS_INCOMPLETE" in old_source
    assert "EXECUTOR_DIRECTIONS_INCOMPLETE" in new_source
    assert "var structureValidation = ValidateCheckpointStructure(checkpoint);" in new_source
    assert "var machine = new TranslationMachine();" in new_source
    assert "var importResult = machine.ImportCheckpoint(checkpoint);" in new_source
    assert set(old) - set(recorded) == set()
    assert set(new) - set(recorded) == {"ES_EN", "EN_ES"}
    result = {
        "analysisKind": "SOURCE_AND_RETAINED_DATA_COMPARISON",
        "nativeLoaderExecuted": False,
        "migrationExecuted": False,
        "newTrainingCalls": 0,
        "newInferenceCalls": 0,
        "baselineCommit": BASE,
        "currentCommit": NOW,
        "originalGitBlobs": EXPECTED,
        "checkpointBytes": len(raw),
        "checkpointSha256": hashlib.sha256(raw).hexdigest(),
        "checkpointIdenticalInBothBranches": True,
        "schemaVersionInBothLoadersAndCheckpoint": 1,
        "checkpointDirections": recorded,
        "baselineRuntimeDirections": old,
        "currentRuntimeDirections": new,
        "missingForBaseline": sorted(set(old) - set(recorded)),
        "missingForCurrent": sorted(set(new) - set(recorded)),
        "predictedCurrentError": "EXECUTOR_DIRECTIONS_INCOMPLETE",
        "trainedState": {
            "hasStructuredTraining": data["has_structured_training"],
            "version": data["version"],
            "knnTriples": len(data["knn_store"]["entries"]),
            "exactSources": sum(len(e["exact_source_map"]) for e in data["executors"]),
            "phraseMappings": sum(len(e["phrase_map"]) for e in data["executors"]),
            "historicalBatches": len(data["training_history"]),
            "structuredIdentities": len(data["structured_training_identities"]),
        },
        "limits": "Set comparison and exact source-path inspection, not C# execution. This identifies legacy-state incompatibility; it does not establish a cause of RP01 neural quality or a successful migration.",
        "sources": [
            f"https://github.com/lubochka/llm_mvp_core/blob/{BASE}/{MACHINE}#L306-L362",
            f"https://github.com/lubochka/llm_mvp_core/blob/{NOW}/{MACHINE}#L308-L430",
            f"https://github.com/lubochka/llm_mvp_core/blob/{NOW}/{MACHINE}#L611-L650",
            f"https://github.com/lubochka/llm_mvp_core/blob/{NOW}/{MACHINE}#L714-L780",
            f"https://github.com/lubochka/llm_mvp_core/blob/{BASE}/{CHECKPOINT}",
            f"https://github.com/lubochka/llm_mvp_core/blob/{NOW}/{CHECKPOINT}",
        ],
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(result, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
