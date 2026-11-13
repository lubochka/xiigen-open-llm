"""Source-only arithmetic witness for StructuralKnn.ArgumentSyntax ordinal loss.

This does not parse C#, does not call Roslyn or native StructuralKnnPredictor,
does not train a product model, and does not measure native learning quality.
The symbolic fields below are deliberately conditional source-derived inputs.
"""
from __future__ import annotations

import json
from dataclasses import asdict, dataclass, replace
from pathlib import Path


@dataclass(frozen=True)
class VectorWitness:
    AstNodeType: str = "IdentifierName"
    Parent1Type: str = "Argument"
    Parent2Type: str = "ArgumentList"
    Parent3Type: str = "InvocationExpression"
    PositionInParent: int = 0
    SiblingCount: int = 1
    DepthInMethod: int = 4
    ScopeDepth: int = 1
    FileId: int = 0
    SyntacticRole: int = 6
    ArgumentOrdinal: int = -1


def legacy_distance(a: VectorWitness, b: VectorWitness) -> float:
    # StructuralKnn.cs L27-35; symbolic equality is enough for this arithmetic.
    return (
        (4.0 if a.SyntacticRole != b.SyntacticRole else 0.0)
        + (3.0 if a.Parent1Type != b.Parent1Type else 0.0)
        + (2.0 if a.AstNodeType != b.AstNodeType else 0.0)
        + (2.0 if a.Parent2Type != b.Parent2Type else 0.0)
        + (1.5 if a.Parent3Type != b.Parent3Type else 0.0)
        + (0.3 if a.PositionInParent != b.PositionInParent else 0.0)
        + abs(a.ScopeDepth - b.ScopeDepth) * 0.3
        + abs(a.DepthInMethod - b.DepthInMethod) * 0.2
    )


def ordinal_distance(a: VectorWitness, b: VectorWitness) -> float:
    # Proposed single additional feature; fixed weight, not tuned on exam.
    return legacy_distance(a, b) + (0.3 if a.ArgumentOrdinal != b.ArgumentOrdinal else 0.0)


def predict_k1(query: VectorWitness, memory, distance):
    if not memory:
        return {"modeled_empty_memory_return_code": 0, "state": "UNTRAINED"}
    # Python min, like LINQ OrderBy, preserves encounter order for equal keys.
    i, (vector, next_kind) = min(enumerate(memory), key=lambda item: distance(query, item[1][0]))
    return {"selected_training_ordinal": i, "symbolic_next_kind": next_kind,
            "distance": round(distance(query, vector), 10)}


def main():
    left = VectorWitness(ArgumentOrdinal=0)
    right = VectorWitness(ArgumentOrdinal=1)
    memory = [(left, "Argument"), (right, "ReturnStatement")]
    queries = [
        ("nested_block_first_argument", replace(left, DepthInMethod=5, ScopeDepth=2)),
        ("nested_block_last_argument", replace(right, DepthInMethod=5, ScopeDepth=2)),
        ("conditional_block_first_argument", replace(left, DepthInMethod=6, ScopeDepth=2)),
        ("conditional_block_last_argument", replace(right, DepthInMethod=6, ScopeDepth=2)),
    ]
    rows = []
    for qid, q in queries:
        rows.append({"id": qid, "synthetic_vector": asdict(q),
                     "before_any_memory_A": predict_k1(q, [], legacy_distance),
                     "before_any_memory_B": predict_k1(q, [], ordinal_distance),
                     "after_memory_A": predict_k1(q, memory, legacy_distance),
                     "after_memory_B": predict_k1(q, memory, ordinal_distance)})
    payload = {
        "evidence_type": "SOURCE_ONLY_ARITHMETIC_WITNESS",
        "native_roslyn_run": False,
        "native_knn_run": False,
        "product_learning_quality_measured": False,
        "native_quality_metrics": None,
        "assumption": "Native Roslyn must independently confirm equal legacy vectors and different next-kind targets for positional argument expressions.",
        "not_an_observed_ast": "All vectors are symbolic witnesses; depth values are illustrative and must be replaced with native readback.",
        "core_commit": "eb05219c27663835ce927c7bf97cf1274c07cddb",
        "source": "12.SemanticCompiler/Prediction/StructuralKnn.cs:27-35,42-95,145-161,200",
        "k": 1,
        "added_feature_weight": 0.3,
        "training_witness_count": len(memory),
        "query_witness_count": len(queries),
        "same_parent_vector_distance_A": legacy_distance(left, right),
        "same_parent_vector_distance_B": ordinal_distance(left, right),
        "query_rows": rows,
        "reverse_memory_control": {
            "A": [predict_k1(q, list(reversed(memory)), legacy_distance) for _, q in queries],
            "B": [predict_k1(q, list(reversed(memory)), ordinal_distance) for _, q in queries],
        },
        "negative_control_no_argument": {
            "A_distance": legacy_distance(VectorWitness(), VectorWitness()),
            "B_distance": ordinal_distance(VectorWitness(), VectorWitness()),
        },
        "interpretation": "The proposed field can separate these supplied vectors; this is not a measured change from 50% to 100% native accuracy and is not evidence of an RP01 regression.",
    }
    out = Path(__file__).with_name("argument_ordinal_probe_result.json")
    out.write_text(json.dumps(payload, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({"result": str(out), "evidence_type": payload["evidence_type"],
                      "source_distance_A": payload["same_parent_vector_distance_A"],
                      "source_distance_B": payload["same_parent_vector_distance_B"],
                      "native_roslyn_run": False, "native_quality_metrics": None}, ensure_ascii=False))


if __name__ == "__main__":
    main()
