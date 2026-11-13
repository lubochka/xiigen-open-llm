# Layer 3 — Teaching Unit Pairs (013)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU-013 — "fine-tune" vocabulary teaching unit

**Teaching goal**: Explain why "fine-tune" is always AI vocabulary and when code verbs override it.

**Rule 1**: "fine-tune the model" → gap. "fine-tune" is an AI training verb. It describes adjusting model weights — an AI operation, not a code artifact.

**Rule 2**: "build a fine-tuning pipeline" → IMPLEMENT. "build" is a code verb. "fine-tuning pipeline" is the software artifact (a service, not the training act itself). Code verb wins.

**Rule 3**: "implement the fine-tuning job scheduler" → IMPLEMENT. "implement" = code verb. "scheduler" = software artifact. The AI context (fine-tuning) is the domain, not the verb.

**Rule 4**: "adapt the model by fine-tuning" → gap. Double AI vocabulary. Neither "adapt" nor "fine-tune" fires a code predicate.

**Rule 5**: "integrate the fine-tuning service with the API" → API_CONTRACT. "integrate" always fires. "fine-tuning service" is the integration object, not a predicate modifier.

**Summary**: The verb determines the predicate, not the object. "fine-tune" (AI verb) → gap. Code verbs applied to fine-tuning artifacts → fire code predicates.
