#!/usr/bin/env python3
"""Small source/formula diagnostic; DOES NOT execute C#, DPO, LoRA or a model.

Usage:
  python autonomous_source_probe.py --output autonomous_source_probe_result.json
  python autonomous_source_probe.py --source-root source --output result.json

source-root contains current/ and external/ pinned snapshots. A supplied source
root is fully verified against source_manifest.json before the formulas run.
The replay probe proves cohort membership, not .NET Random's exact draw.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import struct
from pathlib import Path


def f32(value: float) -> float:
    return struct.unpack("f", struct.pack("f", value))[0]


def git_blob_sha1(data: bytes) -> str:
    return hashlib.sha1(b"blob " + str(len(data)).encode() + b"\0" + data).hexdigest()


def verify_sources(root: Path, manifest_path: Path) -> dict:
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    verified = []
    for entry in manifest["sources"]:
        relative = entry["local_path"].removeprefix("source/")
        data = (root / relative).read_bytes()
        assert git_blob_sha1(data) == entry["git_blob_sha1"], relative
        assert hashlib.sha256(data).hexdigest() == entry["sha256"], relative
        verified.append({"path": relative, "git_blob_sha1": entry["git_blob_sha1"]})
    def read(branch: str, file: str) -> str:
        return (root / branch / file).read_text(encoding="utf-8-sig")
    owner = read("current", "16.AutonomousLearning/Pipeline/AutonomousLearningPipeline.cs")
    trainer = read("current", "16.AutonomousLearning/Training/AutonomousTrainer.cs")
    weighter = read("current", "16.AutonomousLearning/Preprocessing/DatasetWeighter.cs")
    replay = read("current", "16.AutonomousLearning/Training/ReplayBuffer.cs")
    assert owner.index("StringComparer.Ordinal.Equals(example.Domain, domain)") < owner.index("rawExamples.GroupBy(e => e.Domain).Count()")
    assert "rawExamples.GroupBy(e => e.Domain).Min(g => g.Count())" in owner
    assert "Math.Min(e.Weight * maxWeight, maxWeight)" in weighter
    assert trainer.index("await _trainer.TrainAsync(") < trainer.index("_buffer.Add(ex)") < trainer.index("return trainResult;", trainer.index("_buffer.Add(ex)"))
    assert owner.index("await _trainer.TrainPreparedAsync(") < owner.index("AccuracyEvaluationPhase.Candidate") < owner.index("if (enhanced <= baseline)") < owner.index("_registry.Register(adapter)")
    assert "requestedDomains.Contains(_buffer[index].Domain)" in replay
    assert "newExamples.Concat(replayed)" in replay
    current_weight = (root / "current/16.AutonomousLearning/Preprocessing/DatasetWeighter.cs").read_bytes()
    donor_weight = (root / "external/16.AutonomousLearning/Preprocessing/DatasetWeighter.cs").read_bytes()
    same_without_header = b"".join(current_weight.splitlines(keepends=True)[5:]) == donor_weight
    assert same_without_header
    return {"performed": True, "verified_source_count": len(verified), "verified_sources": verified,
            "literal_call_order_checks_passed": True,
            "weighter_same_bytes_after_exactly_five_license_lines": same_without_header}


def weight_case(name: str, weights: list[float], domains: list[str]) -> dict:
    requested_domain = domains[0]
    admitted = bool(weights) and all(d == requested_domain for d in domains)
    if not admitted:
        return {"name": name, "admitted": False, "error": "INVALID_TRAINING_EXAMPLE", "weighter_called": False}
    counts = {domain: domains.count(domain) for domain in set(domains)}
    num_classes, rare_count = len(counts), min(counts.values())
    cap = len(weights) / (num_classes * rare_count)
    weighted = [min(weight * cap, cap) for weight in weights]
    return {"name": name, "admitted": True, "num_classes": num_classes,
            "rare_count": rare_count, "max_weight": cap, "weights_before": weights,
            "weights_after": weighted, "last_to_first_ratio_before": weights[-1] / weights[0],
            "last_to_first_ratio_after": weighted[-1] / weighted[0]}


def replay_case(name: str, train_succeeded: bool, candidate_accepted: bool,
                same_domain: bool = True) -> dict:
    # Five previously unseen lessons belong to one candidate, and buffer starts empty.
    # Provenance/checkpoint validation succeeds whenever native training succeeded.
    buffer_after_train = 5 if train_succeeded else 0
    registry_promoted = train_succeeded and candidate_accepted
    eligible = buffer_after_train if same_domain else 0
    replay_count_before_cap = round(f32(f32(5 * f32(0.3)) / f32(f32(1.0) - f32(0.3))))
    replay_count = min(replay_count_before_cap, eligible)
    return {"name": name, "train_succeeded": train_succeeded,
            "candidate_quality_accepted": candidate_accepted,
            "buffer_count_after_training": buffer_after_train,
            "registry_promoted": registry_promoted,
            "buffer_count_after_quality_decision": buffer_after_train,
            "cycle_2_new_count": 5, "cycle_2_eligible_old_count": eligible,
            "cycle_2_replay_count": replay_count,
            "cycle_2_total_count": 5 + replay_count,
            "all_cycle_2_replayed_records_from_cycle_1_candidate": replay_count > 0,
            "specific_dotnet_rng_draw": "NOT_SIMULATED",
            "lesson_semantic_quality": "NOT_EVALUATED"}


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--source-root", type=Path)
    parser.add_argument("--manifest", type=Path, default=Path(__file__).with_name("source_manifest.json"))
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    verification = verify_sources(args.source_root, args.manifest) if args.source_root else {"performed": False}
    weight_results = [
        weight_case("uniform_provider_control", [1, 1, 1, 1, 1], ["CODE_Q"] * 5),
        weight_case("valid_intradomain_importance_weight", [1, 1, 1, 1, 4], ["CODE_Q"] * 5),
        weight_case("normalized_weight_control", [.25, .25, .25, .25, 1], ["CODE_Q"] * 5),
        weight_case("category_encoded_as_other_domain_control", [1, 1, 1, 1, 4], ["CODE_Q"] * 4 + ["BUG"]),
    ]
    replay_results = [
        replay_case("trained_candidate_quality_rejected", True, False),
        replay_case("training_failed_control", False, False),
        replay_case("candidate_quality_accepted_control", True, True),
        replay_case("different_domain_control", True, False, False),
    ]
    assert weight_results[1]["weights_after"] == [1] * 5
    assert weight_results[2]["last_to_first_ratio_after"] == 4
    assert replay_results[0]["cycle_2_replay_count"] == 2
    assert replay_results[1]["cycle_2_replay_count"] == 0
    assert replay_results[3]["cycle_2_replay_count"] == 0
    result = {"schema": "source-formula-diagnostic/v1", "execution_kind": "PYTHON_SOURCE_EQUIVALENT_FORMULAS_AND_ORDER_ASSERTIONS",
              "native_csharp_executed": False, "model_training_executed": False,
              "free_answers_measured": False, "teacher_or_judge_called": False,
              "source_verification": verification, "weight_cases": weight_results,
              "replay_cases": replay_results,
              "interpretation_limit": "Field transformations and cohort membership only; no measured quality gain or RP01 cause."}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(json.dumps({"output": str(args.output), "source_verification_performed": verification["performed"],
                      "weight_cases": len(weight_results), "replay_cases": len(replay_results),
                      "native_csharp_executed": False, "quality_measured": False}, ensure_ascii=False))


if __name__ == "__main__":
    main()
