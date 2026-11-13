#!/usr/bin/env python3
"""Isolated arithmetic/carrier probes. Does NOT execute C#, train, or restore a model.

Source pin: llm_mvp_core 44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76.
Only reproduces the specific equality and ordering operations cited in the reports.
Variant B is a research control; it does not modify repository source.
"""
from __future__ import annotations

from dataclasses import asdict, dataclass, replace
import json
from pathlib import Path

CURRENT = "44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76"
DONOR = "3036a01acec938f0b83fa4ffbda26202cc35d639"


@dataclass(frozen=True)
class Decision:
    document_id: str
    source_status: str = "ready"
    phase_id: str = "3"
    target_id: str = "3.3"
    confidence: float = 0.93
    nearest_wrong: float = 0.0
    knowledge_edge: bool = False
    failure_classification: str = "source_classified_by_learned_edge_authority"
    authority_role: str = "trained_edge_unit"
    evidence_predicates: tuple[str, ...] = ("API_CONTRACT",)


def aggregate_current(rows: list[Decision]) -> dict:
    # MultiSubPhaseRouterUnit.cs L125-L145 and L738-L754.
    by_target: dict[str, Decision] = {}
    for row in rows:
        previous = by_target.get(row.target_id)
        if previous is not None and previous != row:
            return {"success": False, "error": "MULTI_ROUTER_LEARNED_AUTHORITY_RESULT_INVALID"}
        by_target.setdefault(row.target_id, row)
    return {"success": True, "targets": list(by_target)}


def aggregate_donor(rows: list[Decision]) -> dict:
    # external_domains MultiSubPhaseRouterUnit.cs L121-L127, target-level merge.
    by_target: dict[str, Decision] = {}
    for row in rows:
        previous = by_target.get(row.target_id)
        if previous is None or previous.confidence < row.confidence:
            by_target[row.target_id] = row
    return {"success": True, "targets": list(by_target)}


def aggregate_identity_control(rows: list[Decision], allowed_source_ids: set[str]) -> dict:
    # Restricted counterfactual: different lawful provenance does not mean conflicting decision.
    # All semantic fields, exact confidence, evidence and authority still must agree.
    by_target: dict[str, Decision] = {}
    provenance: dict[str, list[str]] = {}
    for row in rows:
        if row.document_id not in allowed_source_ids:
            return {"success": False, "error": "UNKNOWN_SOURCE_ID"}
        previous = by_target.get(row.target_id)
        if previous is not None and previous != replace(row, document_id=previous.document_id):
            return {"success": False, "error": "CONFLICTING_DECISION"}
        by_target.setdefault(row.target_id, row)
        provenance.setdefault(row.target_id, []).append(row.document_id)
    return {"success": True, "targets": list(by_target), "source_ids_by_target": provenance}


def relevance_floor(scores: dict[str, float]) -> dict[str, float]:
    # DeterministicSubPhaseBaselineScorer.cs L82-L94.
    positive = {key: score for key, score in scores.items() if score > 0.0}
    if not positive:
        return {}
    best = max(positive.values())
    floor = max(0.20, best * 0.80) if best >= 0.20 else best
    return {key: score for key, score in positive.items() if score >= floor}


def current_edge_candidates(scores: dict[str, float], allowed: set[str]) -> dict[str, float]:
    # Provider L157 executes full-corpus floor; L164-L176 filters edge eligibility afterward.
    return {key: score for key, score in relevance_floor(scores).items() if key in allowed}


def eligibility_first_control(scores: dict[str, float], allowed: set[str]) -> dict[str, float]:
    # Raw scores remain those computed with the FULL corpus: no BM25/normalization change.
    return relevance_floor({key: score for key, score in scores.items() if key in allowed})


def run() -> dict:
    # The producer names segment documents {documentId}-segment-{index + 1}, L138-L156.
    first = Decision("normal-request-prompt-segment-1")
    second = replace(first, document_id="normal-request-prompt-segment-2")
    allowed_sources = {first.document_id, second.document_id}
    different_target = replace(second, phase_id="4", target_id="4.1", evidence_predicates=("IMPLEMENT",))
    mismatch = replace(second, confidence=0.99)
    segment_cases = {
        "distinct_real_source_ids_same_semantic_target": [first, second],
        "same_source_id_exact_repeat_control": [first, first],
        "different_targets_control": [first, different_target],
        "actual_decision_conflict_control": [first, mismatch],
    }
    segment_results = {}
    for name, rows in segment_cases.items():
        segment_results[name] = {
            "inputs": [asdict(row) for row in rows],
            "current_formula": aggregate_current(rows),
            "donor_baseline_formula": aggregate_donor(rows),
            "B_provenance_aware_identity_control": aggregate_identity_control(rows, allowed_sources),
        }
    assert not segment_results["distinct_real_source_ids_same_semantic_target"]["current_formula"]["success"]
    assert segment_results["distinct_real_source_ids_same_semantic_target"]["B_provenance_aware_identity_control"]["success"]
    assert segment_results["same_source_id_exact_repeat_control"]["current_formula"]["success"]
    assert segment_results["different_targets_control"]["current_formula"]["success"]
    assert not segment_results["actual_decision_conflict_control"]["B_provenance_aware_identity_control"]["success"]

    # Analytically realizable source setting, NOT measured native score:
    # empty context; prompt "bug test"; predicate COMMON; neutral topology names/purposes;
    # allowed 5.1 has COMMON,OTHER => predicateScore 1/2; disallowed 4.4 has COMMON => 1.
    # Both pass anchors. No id mention or name/purpose token overlap.
    # Source raw scores: allowed=.275+.02*hybridA; distractor=.55+.02*hybridB.
    # HybridRanker default fused value lies in [0,1]. Take the case most favorable to allowed.
    allowed_scores = {"5.1": 0.275 + 0.02 * 1.0}
    expanded_scores = {**allowed_scores, "4.4": 0.55 + 0.02 * 0.0}
    allowed = {"5.1"}
    edge_results = {
        "raw_score_kind": "analytical_bound_fixture_not_native_observation",
        "allowed_ids": sorted(allowed),
        "score_bounds": {"allowed_5.1": [0.275, 0.295], "ineligible_4.4": [0.55, 0.57]},
        "proof_inequality": "max_allowed=.295 < .8*min_ineligible=.44",
        "small_registry_current": current_edge_candidates(allowed_scores, allowed),
        "expanded_registry_current": current_edge_candidates(expanded_scores, allowed),
        "expanded_registry_B_eligibility_first": eligibility_first_control(expanded_scores, allowed),
        "same_full_raw_scores_both_arms": expanded_scores,
    }
    assert edge_results["small_registry_current"] == {"5.1": 0.29500000000000004}
    assert not edge_results["expanded_registry_current"]
    assert edge_results["expanded_registry_B_eligibility_first"] == edge_results["small_registry_current"]
    assert "4.4" not in edge_results["expanded_registry_B_eligibility_first"]

    return {
        "status": "ISOLATED_FORMULA_PROBE_COMPLETE",
        "source_sha": CURRENT,
        "donor_sha": DONOR,
        "native_csharp_executed": False,
        "model_training_executed": False,
        "checkpoint_restore_executed": False,
        "external_generator_called": False,
        "learning_quality_verdict": "NOT_MEASURED",
        "segment_identity": segment_results,
        "edge_eligibility_floor": edge_results,
    }


if __name__ == "__main__":
    result = run()
    target = Path(__file__).with_name("probe_results.json")
    target.write_text(json.dumps(result, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(json.dumps({"status": result["status"], "output": str(target), "quality": result["learning_quality_verdict"]}))
