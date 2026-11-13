# Layer 0-C — Knowledge-Edge Pairs (024)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Complete B01 Knowledge-Edge Decision Table

---

## B01-0C-024-001 — Complete gap vocabulary list for B01

```
source_id: xiigen-b01-0c-024-gap-vocab
source_license_state: first_party
pair_kind: knowledge_edge
```

### Gap vocabulary (no predicate fires)

| Term | Predicate it resembles | Why it does NOT fire |
|---|---|---|
| navigation | UI | Not in UI lexicon — "navigation" is explicitly excluded |
| menu | UI | Not in UI lexicon |
| nav bar | UI | Not in UI lexicon |
| sidebar | UI | Not in UI lexicon |
| drawer | UI | Not in UI lexicon |
| experience | UI | Only "ux" fires UI, not "user experience" or "experience" |
| flow | UI | Not in UI lexicon |
| journey | UI | Not in UI lexicon |
| redesign | IMPLEMENT | Not in IMPLEMENT lexicon |
| improve | IMPLEMENT | Not in IMPLEMENT lexicon |
| update (non-code) | IMPLEMENT | "update" is not in IMPLEMENT; "write", "build", "create" etc. are |
| integrate | IMPLEMENT | Not in IMPLEMENT lexicon |
| run | IMPLEMENT | Not in IMPLEMENT lexicon |
| automate | IMPLEMENT | Not in IMPLEMENT lexicon |
| allow | IMPLEMENT | Not in IMPLEMENT lexicon |
| support | IMPLEMENT | Not in IMPLEMENT lexicon |
| dark mode | UI | Compound; "mode" alone not in lexicon |
| brand | any | Not in any predicate lexicon |
| card sorting | any | UX research methodology; not registered |

---

## B01-0C-024-002 — Summary: knowledge-edge result path vs. normal path

```
source_id: xiigen-b01-0c-024-summary
source_license_state: first_party
pair_kind: knowledge_edge
```

Knowledge-edge path (KNOWLEDGE_EDGE):
  Trigger: CompiledPredicateIds is empty []
  OverallVerdict: "KNOWLEDGE_EDGE"
  FlowId: "knowledge-edge-missing-flow"
  Metrics required keys: knowledge_edge_reason, matched_predicate_count,
    capability_gap_id, capability_gap_description, recommended_action, next_flow_id
  Action: SCHEDULE_CAPABILITY_LEARNING
  FinalHumanResult: null

Normal registered-flow path (FLOW_EXECUTING):
  Trigger: CompiledPredicateIds has ≥1 matched predicate
  OverallVerdict: "FLOW_EXECUTING"
  FlowId: selected by factory (trusted_learned or deterministic_generic)
  Metrics: matched_predicate_count ≥ 1; no gap fields required
  Action: EXECUTE_FLOW

Rules:
  1. Model must NOT infer predicates from topic context alone
  2. Model must NOT guess a sub-phase from gap vocabulary
  3. Capability-learning signal is ONLY emitted on knowledge-edge path
  4. KnowledgeEdge: true always paired with SubPhaseId: null

---

## B01-0C-024-003 — Final contrast: "ux" fires; "user experience" does not

```
source_id: xiigen-b01-0c-024-ux-contrast
source_license_state: first_party
pair_kind: knowledge_edge
```

A-fires: "Map the ux flows across our onboarding screens."
  → "ux" fires UI, "screens" fires UI → CompiledPredicateIds: ["UI"] → FLOW_EXECUTING

A-no-fire: "Map the user experience across our onboarding."
  → "experience" NOT in UI lexicon. "onboarding" not in any lexicon.
  → CompiledPredicateIds: [] → KNOWLEDGE_EDGE

Teaching point: "ux" (exact abbreviation) is in the UI lexicon.
"user experience" (longform) is NOT. The compiler matches exact terms, not semantic equivalents.
