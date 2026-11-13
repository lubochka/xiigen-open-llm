# Layer 0-C — Knowledge Edge Pairs (013)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-013-001 — Edge: "fine-tune" is always AI vocabulary

"fine-tune the model" → gap (AI training verb)
"fine-tune the function" → gap (same verb; "function" does not change verb classification)
"implement the fine-tuning job" → IMPLEMENT (code verb overrides AI object)
"build the fine-tune data pipeline" → IMPLEMENT (code verb overrides AI object)

Teaching: "fine-tune" as a standalone verb always → gap. Only code verbs (implement, build) applied to artifacts fire predicates.

---

## KE-013-002 — Edge: "fine-tune" vs "refactor" disambiguation

"fine-tune the code" → IMPLEMENT (refactoring/tuning code = code artifact; code verb implied by code context)
"fine-tune the model weights" → gap (AI training vocabulary; model weights = AI artifact, not code)
Teaching: "fine-tune the code" is ambiguous but resolved by object: code artifact → IMPLEMENT; AI artifact → gap. Object determines gap/predicate classification when verb is "fine-tune".

---

## KE-013-003 — Edge: past tense and conditional do not create predicates

"we fine-tuned the model last month" → gap (past tense reporting; no present code action)
"if we fine-tuned on customer data, accuracy would improve" → gap (conditional; no code action)
Teaching: Past tense and conditional "fine-tune" are observations → gap. No predicate fires from historical or hypothetical AI actions.

---

## KE-013-004 — Edge: "fine-tune" + "integrate" in same request

"Fine-tune the model and integrate it with the production API." → ["API_CONTRACT"] only
Teaching: "fine-tune" = gap; "integrate" = API_CONTRACT fires. Mixed gap + code request: gap vocabulary silently skipped; only predicates fire. Result: ["API_CONTRACT"].

---

## KE-013-005 — Edge: "adapt the model by fine-tuning" — double AI vocabulary

"adapt the model by fine-tuning it" → gap (both "adapt" and "fine-tune" are AI vocabulary; no code verb fires)
"build a model adapter using fine-tuning checkpoints" → IMPLEMENT (code verb fires despite AI object)
Teaching: Stacking AI vocabulary does not change gap classification. A single code verb is sufficient to fire IMPLEMENT regardless of how many AI terms are in the object.
