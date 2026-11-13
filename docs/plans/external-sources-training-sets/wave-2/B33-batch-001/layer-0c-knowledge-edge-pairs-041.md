# Layer 0-C — Knowledge Edge Pairs (041)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-041-001 — Benchmark gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| benchmark (as AI evaluation goal, standalone) | No code verb | gap-ai-ml-training-001 |
| benchmark harness (with "build") | Code verb present | IMPLEMENT fires |
| benchmark results (with "document") | "document" present | DOCUMENT fires |

Note: "benchmark" as a standalone AI evaluation goal → gap. "benchmark harness" as software artifact → IMPLEMENT when "build" is present.

---

## KE-041-002 — Evaluation metric gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| evaluation metric (standalone) | No code verb | gap-ai-ml-training-001 |
| evaluation metric calculator (with "develop") | Code verb present | IMPLEMENT fires |
| evaluation metric selection (with "document") | "document" present | DOCUMENT fires |

---

## KE-041-003 — F1 / BLEU / ROUGE are evaluation metric synonyms

Rule: F1, BLEU (bilingual evaluation understudy), ROUGE (recall-oriented understudy for gisting evaluation) are specific evaluation metrics.
All three map to gap-ai-ml-training-001 when used as standalone terms.
"Compute F1 score" (as goal, no code verb) → gap-ai-ml-training-001.
"Build a service that computes F1 score" → IMPLEMENT.

---

## KE-041-004 — "Develop" fires IMPLEMENT identically to "build" and "implement"

Rule: "develop" is in the same code-verb class as "build", "implement", "create".
"develop an evaluation metric calculator" → IMPLEMENT.
No distinction between develop / build / implement / create for predicate selection.

---

## KE-041-005 — DOCUMENT fires on "document the X" regardless of AI vocabulary in X

Rule: "document" fires DOCUMENT when "document" is the primary verb.
"document the evaluation metric selection" → DOCUMENT.
"document the benchmark results" → DOCUMENT.
AI vocabulary in the object position does not suppress DOCUMENT.
