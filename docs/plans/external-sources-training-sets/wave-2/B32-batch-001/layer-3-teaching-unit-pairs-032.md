# Layer 3 — Teaching Unit Pairs (032)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-032-001 — Teaching: SECURITY fires from "secure" imperative, not "security" noun

**Teaching unit:** SECURITY predicate trigger boundary

"Secure the X" → SECURITY fires.
"Think about security" / "we need security" / "security concerns" → SECURITY does NOT fire.

Gap qualifiers never suppress "secure" imperative when present.

---

## B32-L3-032-002 — Teaching: gap ID must match the actual vocabulary

**Teaching unit:** Gap ID specificity

Different gap vocabulary produces different gap IDs:
- "lean", "pragmatic", "expedient", etc. → gap-vibe-coding-style-001
- "hack", "hack together", "cobble together" → gap-vibe-coding-hack-001
- "spike", "tracer bullet", "walking skeleton" → gap-vibe-coding-spike-001
- "prototype", "PoC (Proof-of-Concept)" → gap-vibe-coding-prototype-001

The gap ID in GapWarnings must match the actual term used. Using gap-vibe-coding-style-001 for "hack together" is incorrect.

---

## B32-L3-032-003 — Teaching: ARCHITECTURE + SECURITY → documentation TransitionType

When ARCHITECTURE and SECURITY are both present without IMPLEMENT:
- TransitionType = documentation (ARCHITECTURE drives)
- SECURITY contributes SecurityFlow but yields to ARCHITECTURE's type contribution

The hierarchy: feature_addition (IMPLEMENT/UI) > documentation (ARCHITECTURE/API_CONTRACT/DOCUMENT) > no_contribution (TEST/QA/SECURITY).

---

## B32-L3-032-004 — Teaching: "add security" vs "secure the endpoint"

| Expression | SECURITY fires? | Reason |
|---|---|---|
| "secure the endpoint" | YES | Direct imperative with object |
| "audit the security" | YES | Audit as security verb |
| "add security" | CONTEXT-DEPENDENT | Ambiguous; not a reliable trigger |
| "add HTTPS" | YES | Specific security action |
| "improve security" | NO | Vague improvement; not imperative |

For B32 training, use the clear "secure X" and "audit" forms.

---
