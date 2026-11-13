# Held-Out and Corruption Pairs (013)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-013-A — Held-out: "fine-tune" boundary cases

A (gap): "Fine-tune the model on customer support conversations." → Predicates: [] → GapHandler → gap-ai-agent-llm-001
A (IMPLEMENT): "Build the fine-tuning pipeline for customer support data." → Predicates: ["IMPLEMENT"] → ImplementationFlow41
A (API_CONTRACT): "Integrate the fine-tuning service with the model registry API." → Predicates: ["API_CONTRACT"] → APIContractFlow33
Distinction: verb determines routing. "fine-tune" → gap; "build fine-tuning" → IMPLEMENT; "integrate fine-tuning" → API_CONTRACT.

---

## HC-013-B — Held-out: double AI vocabulary does not change gap classification

A (double gap): "Adapt the model by fine-tuning it on proprietary data." → Predicates: [] → GapHandler → gap-ai-agent-llm-001
Reason: Both "adapt" and "fine-tune" are AI vocabulary. Stacking AI vocabulary does not change gap status or create new predicates.

---

## HC-013-C — Corruption v6-01: "FINE_TUNE" invented predicate

v6 corrupt: "Fine-tune the model" → Predicates: ["FINE_TUNE"]
Correct: Predicates: [] → GapHandler
Error: FINE_TUNE does not exist in the nine-predicate lexicon. Nine predicates: IMPLEMENT, ARCHITECTURE, API_CONTRACT, SECURITY, AUTHORIZATION, TEST, DOCUMENT, QA, (none for gap).

---

## HC-013-D — Held-out: fine-tune + QA compound

A: "Fine-tune the model and QA the fine-tuned outputs."
B: Predicates: ["QA"] | GapHandler suppressed (QA fired); gap-ai-agent-llm-001 silently skipped.
Reason: "QA" abbreviation fires QA predicate; "fine-tune" gap skipped. QA is last.
Phrasing-B: QA fires even when combined with AI gap vocabulary.
