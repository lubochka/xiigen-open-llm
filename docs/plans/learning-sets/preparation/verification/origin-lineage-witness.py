"""Bounded data-validator regression witness; no generation or learning runs.

Usage: python origin-lineage-witness.py /absolute/path/to/tools/sets.py
No repository data or validation artifacts are written by this script.
"""
from copy import deepcopy
import importlib.util
import json
from pathlib import Path
import sys

sys.dont_write_bytecode = True
tool_path = Path(sys.argv[1]).resolve()
spec = importlib.util.spec_from_file_location("xiigen_sets_review", tool_path)
sets = importlib.util.module_from_spec(spec)
spec.loader.exec_module(sets)
seed = sets.rows(sets.BASE / "domains/orchestration/reference-examples.jsonl")[0]


def fixture(tag, split):
    row = deepcopy(seed)
    row.update(pair_id="INTEGRATION-PROBE-" + tag,
               family_id="integration-probe-family-" + tag,
               split=split,
               prompt="Integration review fixture " + tag,
               chosen="choice-" + tag,
               rejected="other-" + tag,
               provenance={"kind": "synthetic_from_repo",
                           "generator_model": "validator_probe_not_a_generation_run"},
               metadata={"origin_family_id": "integration-probe-shared-origin",
                         "leakage_group_ids": ["integration-probe-group-" + tag]})
    return row


cases = []


def check(name, rows, expected_error=None):
    errors = sets.validate_rows(rows)
    passed = (any(expected_error in error for error in errors)
              if expected_error else errors == [])
    cases.append({"name": name, "expected_error": expected_error,
                  "errors": errors, "passed": passed})
    assert passed, (name, errors)


# Pre-fix witness: different family/group IDs and prompts, but the same declared
# origin occurs in TRAIN and EXAM. Before the fix this returned errors=[].
check("same_origin_crosses_train_exam", [fixture("A", "train"), fixture("B", "exam")],
      "RELATED_FAMILY_SPLIT_LEAKAGE")
check("same_origin_same_split_allowed", [fixture("A", "train"), fixture("B", "train")])

# The same lineage name must not evade the check by changing its field role.
a, b = fixture("A", "train"), fixture("B", "exam")
b["metadata"]["origin_family_id"] = "integration-probe-other-origin"
b["family_id"] = a["metadata"]["origin_family_id"]
check("origin_matches_other_family", [a, b], "RELATED_FAMILY_SPLIT_LEAKAGE")
a, b = fixture("A", "train"), fixture("B", "exam")
b["metadata"]["origin_family_id"] = "integration-probe-other-origin"
b["metadata"]["leakage_group_ids"] = [a["metadata"]["origin_family_id"]]
check("origin_matches_other_group", [a, b], "RELATED_FAMILY_SPLIT_LEAKAGE")

# Invalid JSON values should yield a diagnostic rather than an unhashable error.
for index, origin in enumerate(["", "  ", None, 17, [], {}]):
    row = fixture("BAD" + str(index), "exam")
    row["metadata"]["origin_family_id"] = origin
    check("malformed_origin_" + str(index), [row], "ORIGIN_FAMILY_ID_MUST_BE_NONEMPTY_STRING")

# A missing optional origin remains compatible with existing data.
a = fixture("ABSENT", "train")
del a["metadata"]["origin_family_id"]
check("absent_optional_origin_allowed", [a])

print(json.dumps({"scope": "DATA_VALIDATOR_ONLY_NOT_LEARNING",
                  "tool_path": str(tool_path), "tool_sha256": sets.digest(tool_path),
                  "model_generation": "NOT_RUN", "native_training": "NOT_RUN",
                  "cases": cases}, ensure_ascii=False, indent=2))
