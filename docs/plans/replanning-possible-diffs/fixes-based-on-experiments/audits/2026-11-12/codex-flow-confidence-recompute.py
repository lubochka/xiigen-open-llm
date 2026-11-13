#!/usr/bin/env python3
"""ASCII, source-derived arithmetic witness; does NOT run or emulate the .NET assembly.

Only the two one-lesson flows below are modelled. The classifier regex is read from
the pinned C# source; all other classifier branches are explicitly excluded for
these fixtures. The result is a mathematical/source trace, not native acceptance.
Writes one NEW JSON output; never changes an input or existing output.
"""
import argparse
import hashlib
import json
import math
import re
from pathlib import Path

SOURCE_REF = "dcb931773147267c8864a532351d22b4c351265b"
INPUTS = {
    "42.ProjectWikiDomain/PromptFlowGroundingModel.cs": {
        "git_blob_sha1": "b1982c77c593f0f5dcc9747a7345c0d770981344"
    },
    "42.ProjectWikiDomain/OrchestrationKnnStore.cs": {
        "git_blob_sha1": "2bf119f4caee7c807d80c398f54b695a0304e0d9"
    },
    "42.ProjectWikiDomain/OrchestrationPlanMachine.cs": {
        "git_blob_sha1": "449aa38d4cc316b53d5506d1f387a4308a93f028"
    },
    "42.ProjectWikiDomain/OrchestrationPlanContracts.cs": {
        "git_blob_sha1": "916b3eb6f64e88459d8ef3a8a629d9aa445b2bd2"
    },
    "42.ProjectWikiDomain/HumanIntentBoundaryClassifier.cs": {
        "git_blob_sha1": "effb98f900fab397f4da38462b474c60d8fc3e45"
    },
    "42.ProjectWikiDomain/TrainedRouteComposerUnit.cs": {
        "git_blob_sha1": "848a5f7e6d5a315ade30c47eaf79ee9b7f2d1e12"
    }
}

def blob_sha(data):
    return hashlib.sha1(b"blob " + str(len(data)).encode() + b"\0" + data).hexdigest()

def tokenize(text):
    # These witnesses contain only ASCII. This is exactly the relevant subset
    # of the C# [\\p{L}\\p{N}]+ regex; not a general Unicode reimplementation.
    if not text.isascii():
        raise ValueError("The witness is restricted to ASCII")
    return sorted(set(re.findall(r"[A-Za-z0-9]+", text.lower())))

def jaccard(left, right):
    left, right = set(left), set(right)
    if not left and not right:
        return 1.0
    if not left or not right:
        return 0.0
    return len(left & right) / len(left | right)

def classify_fixture(prompt, classifier_source):
    # Read the actual regex literal, not a guessed flow naming convention.
    match = re.search(r'FlowSpanRegex = new\(\s*@"([^"]+)"', classifier_source)
    if match is None:
        raise ValueError("Pinned FlowSpanRegex literal was not found")
    spans = list(dict.fromkeys(m.group(0) for m in re.finditer(
        match.group(1), prompt, flags=re.IGNORECASE)))[:8]
    # Source lines35-44,49-77 have no matching branch for these ASCII prompts.
    disallowed = (
        "until i approve", "until approved", "wait for approval", "pending approval",
        "requires approval", "require approval", "do not continue until", "don't continue until",
        "load planning skills", "load skills", "planning skills", "session init",
        "explain", "explanation", "why", "how does", "how works",
        "translate flow", "migration", "migrate", "from python", "to node", "react native",
        "phase only", "proceed", "continue phase", "next phase",
        "review architecture", "architecture review", "architectural review",
        "testing plan", "test plan", "create tests", "qa plan")
    if any(x in prompt.lower() for x in disallowed) or re.search(r"\bphase\s+\d", prompt, re.I):
        raise ValueError("Fixture reaches an unmodelled classifier branch")
    forbidden_re = re.search(r'ForbiddenSpanRegex = new\(\s*@"([^"]+)"', classifier_source)
    if forbidden_re is None or re.search(forbidden_re.group(1), prompt, re.I):
        raise ValueError("Fixture reaches an unmodelled forbidden-scope branch")
    allowed = []
    for span in spans:
        number = int(re.search(r"\d{1,2}", span).group(0))
        if 1 <= number <= 99:
            flow = f"FLOW-{number:02d}"
            if flow not in allowed:
                allowed.append(flow)
    # No phase keyword or dotted number occurs in any of these fixtures.
    if not spans or "phase" in prompt.lower() or "." in prompt:
        raise ValueError("Fixture is not the known-flow, no-phase case")
    return {
        "Kind": "KnownFlowPlanning", "AllowedFlowScope": allowed,
        "ForbiddenFlowScope": [], "PhaseScope": [], "RequiresApprovalGate": False,
        "RequiresSaveState": True, "HumanConstraints": [], "RequestedFlowTextSpans": spans,
    }

def query_text(prompt, contract):
    return " ".join(x for x in (
        prompt, " ".join(contract["AllowedFlowScope"]),
        " ".join(contract["RequestedFlowTextSpans"]),
        " ".join(contract["HumanConstraints"])) if x.strip())

def evaluate(pairs, prompt, classifier_source, restrict_witness=False):
    if len({p["flow_ref"] for p in pairs}) != len(pairs):
        raise ValueError("This arithmetic witness models only one lesson per flow")
    contract = classify_fixture(prompt, classifier_source)
    query = query_text(prompt, contract)
    tokens = tokenize(query)
    scores = []
    for pair in pairs:
        # Tokenize() deduplicates: every fresh BuildTokenWeights value is 1.
        input_score = len(set(tokens) & set(tokenize(pair["prompt"])))
        scope_score = len(set(tokens) & set(tokenize(pair["flow_ref"])))
        scores.append({"flow_ref": pair["flow_ref"], "input_score": input_score,
                       "scope_score": scope_score, "total": input_score + scope_score})
    scores.sort(key=lambda x: (-x["total"], -x["scope_score"], x["flow_ref"]))
    best = scores[0]
    neighbors = [
        {"pair_id": p["pair_id"], "flow_ref": p["flow_ref"],
         "jaccard": jaccard(tokens, tokenize(p["prompt"]))}
        for p in pairs if not restrict_witness or p["flow_ref"] == best["flow_ref"]
    ]
    neighbors.sort(key=lambda x: (-x["jaccard"], x["pair_id"]))
    nearest = neighbors[0] if neighbors else None
    candidate = next(p for p in pairs if p["flow_ref"] == best["flow_ref"])
    own_j = jaccard(tokens, tokenize(candidate["prompt"]))
    result = {
        "prompt": prompt, "contract": contract, "query_text": query,
        "query_tokens": tokens, "candidate_scores": scores, "selected_flow": best["flow_ref"],
        "witness_scope": "selected_flow" if restrict_witness else "global",
        "nearest": nearest, "selected_flow_jaccard_before_evidence": own_j,
        "confidence": None, "predicted_route_mode": "Mode2Generalization",
        "predicted_body_marker": None, "body_lookup": None,
        "basis": "source-derived arithmetic; native GeneratePlanAsync UNRUN",
    }
    if best["total"] <= 0 or nearest is None or nearest["jaccard"] <= 0:
        result["try_ground_exit"] = "reject: nonpositive candidate or absent/nonpositive witness"
        return result
    confidence = min(1.0, 0.70 + best["total"] * 0.10 + nearest["jaccard"] * 0.20)
    result["confidence"] = confidence
    result["try_ground_exit"] = "return grounding" if confidence >= 0.50 else "reject: confidence"
    if confidence < 0.75 or best["flow_ref"] not in contract["AllowedFlowScope"]:
        return result
    evidence = sorted(set(tokenize(candidate["prompt"]) + tokenize(candidate["flow_ref"])))[:8]
    body_query = query + " " + " ".join(evidence)
    # Current FIX-IM correctly limits this lookup to the selected flow.
    body_score = jaccard(tokenize(body_query), tokenize(candidate["prompt"]))
    result["body_lookup"] = {
        "evidence_features": evidence, "query_tokens": tokenize(body_query),
        "pair_id": candidate["pair_id"], "flow_ref": candidate["flow_ref"],
        "jaccard": body_score,
    }
    result["predicted_route_mode"] = "LearnedFlowGrounding"
    result["predicted_body_marker"] = candidate["body_marker"]
    return result

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--core", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    if args.output.exists():
        raise FileExistsError(f"Refusing to overwrite {args.output}")
    verified = {}
    for path, meta in INPUTS.items():
        actual = blob_sha((args.core / path).read_bytes())
        if actual != meta["git_blob_sha1"]:
            raise ValueError(f"Pinned input mismatch: {path}: {actual}")
        verified[path] = actual
    classifier = (args.core / "42.ProjectWikiDomain/HumanIntentBoundaryClassifier.cs").read_text()
    a = {"pair_id": "witness-a", "flow_ref": "FLOW-01", "prompt": "alpha",
         "body_marker": "ALPHA_BODY_ONLY"}
    b = {"pair_id": "witness-b", "flow_ref": "FLOW-02", "prompt": "beta",
         "body_marker": "BETA_BODY_ONLY"}
    cases = {
        "A_only_negative": evaluate([a], "beta FLOW-01", classifier),
        "A_plus_B_negative_current": evaluate([a, b], "beta FLOW-01", classifier),
        "A_plus_B_negative_proposed": evaluate([a, b], "beta FLOW-01", classifier, True),
        "B_then_A_negative_current": evaluate([b, a], "beta FLOW-01", classifier),
        "A_only_positive": evaluate([a], "alpha FLOW-01", classifier),
        "A_plus_B_positive_A_current": evaluate([a, b], "alpha FLOW-01", classifier),
        "A_plus_B_positive_A_proposed": evaluate([a, b], "alpha FLOW-01", classifier, True),
        "A_plus_B_positive_B_current": evaluate([a, b], "beta FLOW-02", classifier),
        "A_plus_B_positive_B_proposed": evaluate([a, b], "beta FLOW-02", classifier, True),
    }
    negative = cases["A_plus_B_negative_current"]
    assert cases["A_only_negative"]["predicted_route_mode"] == "Mode2Generalization"
    assert negative["selected_flow"] == "FLOW-01"
    assert negative["nearest"]["flow_ref"] == "FLOW-02"
    assert math.isclose(negative["nearest"]["jaccard"], 1/3, rel_tol=0, abs_tol=1e-15)
    assert negative["selected_flow_jaccard_before_evidence"] == 0
    assert math.isclose(negative["confidence"], 29/30, rel_tol=0, abs_tol=1e-15)
    assert negative["body_lookup"]["jaccard"] == 1/4
    assert negative["predicted_body_marker"] == "ALPHA_BODY_ONLY"
    assert cases["A_plus_B_negative_proposed"]["predicted_route_mode"] == "Mode2Generalization"
    assert cases["B_then_A_negative_current"] == negative
    for name in ("A_only_positive", "A_plus_B_positive_A_current",
                 "A_plus_B_positive_A_proposed", "A_plus_B_positive_B_current",
                 "A_plus_B_positive_B_proposed"):
        assert cases[name]["predicted_route_mode"] == "LearnedFlowGrounding"
        assert cases[name]["confidence"] == 1.0
    result = {
        "kind": "analytic-source-witness", "source_ref": SOURCE_REF,
        "native_runtime_executed": False, "native_train_calls": 0,
        "native_generate_calls": 0, "input_blob_sha1": verified,
        "fixture": {"pairs": [a, b], "query": "beta FLOW-01"},
        "cases": cases,
        "limitations": [
            "ASCII one-lesson-per-flow arithmetic only; not a general C# interpreter.",
            "Classifier regex evaluated with Python only for these ASCII fixtures.",
            "No .NET compilation, TrainOrchestrationBatchAsync or GeneratePlanAsync was executed.",
            "No claim about accuracy, calibration, task execution, latency or allocation.",
            "Scoped-witness cases describe a proposed repair, not modified product behavior.",
        ],
    }
    # Guard against concurrent input changes and refuse all output overwrites.
    for path, expected in verified.items():
        if blob_sha((args.core / path).read_bytes()) != expected:
            raise RuntimeError(f"Input changed while computing: {path}")
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with args.output.open("x", encoding="utf-8") as handle:
        json.dump(result, handle, ensure_ascii=False, indent=2)
        handle.write("\n")
    print(json.dumps({"output": str(args.output), "verified_inputs": len(verified),
                      "analytic_cases": len(cases), "native_runtime_executed": False}))

if __name__ == "__main__":
    main()

