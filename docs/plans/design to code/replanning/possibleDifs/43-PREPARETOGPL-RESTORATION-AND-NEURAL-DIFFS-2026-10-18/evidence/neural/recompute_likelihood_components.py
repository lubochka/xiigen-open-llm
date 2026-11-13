"""Read retained scores only; never call Student, optimizer, teacher, or judge.

Run: python recompute_likelihood_components.py
Output goes to likelihood-components.json next to this script. Source JSON uses
System.Text.Json's shortest decimal for C# float. Round-trip through binary32 to
recover the exact original score before subtracting in Python binary64.
"""

from __future__ import annotations

import hashlib
import json
import math
import struct
from pathlib import Path

ROOT = Path(__file__).resolve().parent
CORE_SHA = "a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6"
URL_ROOT = f"https://github.com/lubochka/llm_mvp_core/blob/{CORE_SHA}/training-artifacts/experiments/code-documentation-simulation"
SOURCE_GIT_BLOBS = {
    "simulation-csharp-c0-score25-20261015-01": "7717871f5ab4c3f4c7f4dd43b95baafaf50a185a",
    "simulation-csharp-t-score25-20261015-01": "d59b221d6fe0dcb2b86144a06fc031dd7b187ca1",
    "simulation-enru-C0-score16-20261016-01": "3f6dcf9c5a5e0fecf72cec74a7b4f88b6f890406",
    "simulation-enru-T-score16-20261016-01": "5b4d29b0d88667d6b9f4a3cc5c5067cba7380884",
    "simulation-enru-C0-free4-20261016-01": "0d852805093ea2dba7bde7f17ffcf88d8f7e8100",
    "simulation-enru-T-free4-20261016-01": "d68c2d7b15a05a2497f1beb40d3eacadd2524edf",
}


def f32(value: float) -> float:
    return struct.unpack("<f", struct.pack("<f", value))[0]


def source(run_id: str) -> tuple[dict, dict]:
    path = ROOT / "measurements" / f"{run_id}.json"
    raw = path.read_bytes()
    source_git_blob = hashlib.sha1(f"blob {len(raw)}\0".encode() + raw).hexdigest()
    assert source_git_blob == SOURCE_GIT_BLOBS[run_id], "Input bytes differ from the pinned Git source."
    data = json.loads(raw)
    assert data["runId"] == run_id
    assert data["stateUnchanged"] is True
    assert data["error"] is None
    assert data["optimizerCalls"] == 0
    assert data["externalTeacherCalls"] == 0
    assert data["externalEvaluatorCalls"] == 0
    scores = {row["id"]: row for row in data["scores"]}
    assert len(scores) == len(data["scores"])
    for row in scores.values():
        chosen, rejected = f32(row["chosenMean"]), f32(row["rejectedMean"])
        assert math.isfinite(chosen) and math.isfinite(rejected)
        assert chosen - rejected == row["normalizedMargin"]
        assert chosen * row["chosenTargets"] == row["chosenSum"]
        assert rejected * row["rejectedTargets"] == row["rejectedSum"]
    metadata = {
        "runId": run_id,
        "sourceUrl": f"{URL_ROOT}/{run_id}/evaluation-receipt.json",
        "gitBlobSha1": source_git_blob,
        "fileSha256": hashlib.sha256(raw).hexdigest(),
        "panelSha256": data["panelSha256"],
        "candidateSha256": data["candidateSha256"],
        "domain": data.get("domain"),  # Earlier C# receipts did not serialize this field.
        "scoringIssued": data["scoringIssued"],
        "scoringCompleted": data["scoringCompleted"],
        "generationIssued": data["generationIssued"],
        "generationCompleted": data["generationCompleted"],
        "qualityVerdict": data["qualityVerdict"],
    }
    return scores, metadata


def analyze(domain: str, run_pairs: list[tuple[str, str]], count: int) -> dict:
    all_ids: set[str] = set()
    details, sources = [], []
    for before_run, after_run in run_pairs:
        before, before_meta = source(before_run)
        after, after_meta = source(after_run)
        assert before.keys() == after.keys()
        assert not all_ids.intersection(before)
        assert before_meta["panelSha256"] == after_meta["panelSha256"]
        assert before_meta["domain"] == after_meta["domain"]
        all_ids.update(before)
        sources.append({"before": before_meta, "after": after_meta})
        for row_id, old in before.items():
            new = after[row_id]
            assert old["chosenTargets"] == new["chosenTargets"]
            assert old["rejectedTargets"] == new["rejectedTargets"]
            old_c, new_c = f32(old["chosenMean"]), f32(new["chosenMean"])
            old_r, new_r = f32(old["rejectedMean"]), f32(new["rejectedMean"])
            dc, dr = new_c - old_c, new_r - old_r
            dm = dc - dr
            assert dm == new["normalizedMargin"] - old["normalizedMargin"]
            details.append({
                "id": row_id,
                "chosenTargets": old["chosenTargets"],
                "rejectedTargets": old["rejectedTargets"],
                "chosenMeanBefore": old_c,
                "chosenMeanAfter": new_c,
                "rejectedMeanBefore": old_r,
                "rejectedMeanAfter": new_r,
                "deltaChosenMean": dc,
                "deltaRejectedMean": dr,
                "deltaNormalizedMargin": dm,
                "chosenDecreasedDespiteImprovedMargin": dc < 0 and dm > 0,
                "freeGenerationWasAlsoRequested": before_meta["generationIssued"] > 0,
            })
    assert len(details) == count
    details.sort(key=lambda item: item["id"])
    mean_dc = math.fsum(item["deltaChosenMean"] for item in details) / count
    mean_dr = math.fsum(item["deltaRejectedMean"] for item in details) / count
    mean_dm = math.fsum(item["deltaNormalizedMargin"] for item in details) / count
    mean_c0 = math.fsum(item["chosenMeanBefore"] for item in details) / count
    mean_c1 = math.fsum(item["chosenMeanAfter"] for item in details) / count
    mean_r0 = math.fsum(item["rejectedMeanBefore"] for item in details) / count
    mean_r1 = math.fsum(item["rejectedMeanAfter"] for item in details) / count
    return {
        "domain": domain,
        "count": count,
        "meanChosenBefore": mean_c0,
        "meanChosenAfter": mean_c1,
        "meanRejectedBefore": mean_r0,
        "meanRejectedAfter": mean_r1,
        "meanDeltaChosen": mean_dc,
        "meanDeltaRejected": mean_dr,
        "meanDeltaNormalizedMargin": mean_dm,
        "countChosenDecreased": sum(item["deltaChosenMean"] < 0 for item in details),
        "countRejectedDecreased": sum(item["deltaRejectedMean"] < 0 for item in details),
        "countBothDecreased": sum(item["deltaChosenMean"] < 0 and item["deltaRejectedMean"] < 0 for item in details),
        "countMarginImproved": sum(item["deltaNormalizedMargin"] > 0 for item in details),
        "countChosenDecreasedDespiteImprovedMargin": sum(item["chosenDecreasedDespiteImprovedMargin"] for item in details),
        "countFreePromptsChosenDecreasedDespiteImprovedMargin": sum(item["freeGenerationWasAlsoRequested"] and item["chosenDecreasedDespiteImprovedMargin"] for item in details),
        "meanChosenGeometricProbabilityRelativeChange": math.expm1(mean_dc),
        "chosenTargetWeightedDelta": math.fsum(item["deltaChosenMean"] * item["chosenTargets"] for item in details) / sum(item["chosenTargets"] for item in details),
        "sources": sources,
        "details": details,
    }


result = {
    "analysisKind": "RECALCULATION_OF_RETAINED_NATIVE_EVALUATION_SCORES",
    "newStudentCalls": 0,
    "newOptimizerCalls": 0,
    "newExternalTeacherCalls": 0,
    "newExternalEvaluatorCalls": 0,
    "qualityExperimentExecuted": False,
    "coreFrozenSha": CORE_SHA,
    "method": "Binary32 round-trip of original C# floats; paired subtraction in binary64; equal weight per retained prompt.",
    "limits": "Retained panels only. No new semantic grading, no statistical population inference, no causal proof of optimizer interference; scores condition on the entire provided answer prefix.",
    "domains": [
        analyze("C# code documentation B0 -> C50", [
            ("simulation-csharp-c0-score25-20261015-01", "simulation-csharp-t-score25-20261015-01"),
        ], 25),
        analyze("ENRU C50 -> C100", [
            ("simulation-enru-C0-score16-20261016-01", "simulation-enru-T-score16-20261016-01"),
            ("simulation-enru-C0-free4-20261016-01", "simulation-enru-T-free4-20261016-01"),
        ], 20),
    ],
}
destination = ROOT / "likelihood-components.json"
destination.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n")
print(json.dumps({"analysisKind": result["analysisKind"], "domains": [{key: value for key, value in domain.items() if key not in {"details", "sources"}} for domain in result["domains"]]}, ensure_ascii=False, indent=2))
