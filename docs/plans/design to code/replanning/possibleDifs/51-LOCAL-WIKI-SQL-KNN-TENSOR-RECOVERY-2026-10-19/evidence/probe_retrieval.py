"""Small deterministic mechanism probes; NOT Roslyn/native training or quality evaluation.

Source: llm_mvp_core@4945b8e85c17b5f150eb5b840d16bbdc4b506242.
Reproduces only AstRelevanceRanker.Index/Bm25Score and KnnSkillFilter control flow.
All term bags/provider responses below are constructed analytic fixtures, not DPO.
"""
import json
import math
from pathlib import Path


class IndexProxy:
    def __init__(self, corrected):
        self.index = {}
        self.df = {}
        self.corrected = corrected

    def put(self, chunk_id, terms):
        if self.corrected and chunk_id in self.index:
            for term in self.index[chunk_id]:
                self.df[term] -= 1
                if self.df[term] == 0:
                    del self.df[term]
        self.index[chunk_id] = dict(terms)
        for term in terms:
            self.df[term] = self.df.get(term, 0) + 1

    def search(self, query_terms):
        n = len(self.index)
        avg_len = sum(sum(x.values()) for x in self.index.values()) / n
        scores = []
        for chunk_id, terms in self.index.items():
            dl = sum(terms.values())
            score = 0.0
            for term in query_terms:
                if term not in terms:
                    continue
                df = self.df.get(term, 0)
                idf = math.log((n - df + .5) / (df + .5) + 1.0)
                tf = terms[term]
                norm = 1.5 * (1.0 - .75 + .75 * dl / avg_len)
                score += idf * (tf * 2.5) / (tf + norm)
            scores.append({"id": chunk_id, "score": score})
        return sorted(scores, key=lambda row: -row["score"])


def probe_reindex():
    rows = []
    for corrected in (False, True):
        idx = IndexProxy(corrected)
        idx.put("relevant", {"NullLiteralExpression": 1})
        idx.put("unrelated", {"ForStatement": 1})
        for occurrence in (1, 2, 3, 4):
            if occurrence > 1:
                idx.put("relevant", {"NullLiteralExpression": 1})
            rows.append({
                "arm": "decrement_previous_contribution" if corrected else "current_formula",
                "relevant_index_occurrence": occurrence,
                "distinct_document_count": len(idx.index),
                "document_frequency": idx.df["NullLiteralExpression"],
                "ranking": idx.search(["NullLiteralExpression"]),
            })
    return rows


def skill_filter(candidates, hits, threshold=.4):
    if not candidates or not hits:
        return list(candidates), None
    confidence = hits[0][1] / (sum(score for _, score in hits) + 1e-9)
    if confidence < threshold:
        return list(candidates), confidence
    selected_ids = {chunk_id for chunk_id, _ in hits}
    return [c for c in candidates if c in selected_ids], confidence


def probe_skill_scope():
    candidates = ["skill-null-check", "skill-code-review"]
    corpus = [("outside-code-file", .90), ("outside-doc-file", .10),
              ("skill-null-check", .09), ("skill-code-review", .01)]
    global_hits = corpus[:2]
    scoped_hits = [x for x in corpus if x[0] in candidates][:2]
    rows = []
    for arm, hits in [("global_topk_then_intersection", global_hits),
                      ("eligible_scope_before_topk", scoped_hits)]:
        output, confidence = skill_filter(candidates, hits)
        rows.append({"arm": arm, "hits": hits, "confidence": confidence,
                     "selected_candidates": output})
    return rows


def probe_role_contract():
    # These parent/child relations are analytic C# AST fixtures. No parser is run.
    # Current GetSyntacticRole uses direct parent, not any ancestor condition.
    emitted_for_if_equals_null = ["EqualsExpression:7", "NullLiteralExpression:4"]
    advertised_query_key = "NullLiteralExpression:7"
    return {
        "fixture": "if (x == null) return;",
        "constructed_direct_parent_feature_keys": emitted_for_if_equals_null,
        "advertised_query_key": advertised_query_key,
        "structural_channel_matches": advertised_query_key in emitted_for_if_equals_null,
        "native_roslyn_verified": False,
        "limitations": "Plain NullLiteralExpression can still match; natural-query parser branch can bypass this role map entirely.",
    }


if __name__ == "__main__":
    result = {
        "status": "EXECUTED_PYTHON_MECHANISM_PROXY_NOT_NATIVE_TRAINING",
        "source_commit": "4945b8e85c17b5f150eb5b840d16bbdc4b506242",
        "no_training_quality_claim": True,
        "ast_document_frequency": probe_reindex(),
        "skill_candidate_scope": probe_skill_scope(),
        "structural_query_contract": probe_role_contract(),
    }
    path = Path(__file__).with_name("probe_results.json")
    path.write_text(json.dumps(result, indent=2, ensure_ascii=False) + "\n")
    print(json.dumps(result, indent=2, ensure_ascii=False))
