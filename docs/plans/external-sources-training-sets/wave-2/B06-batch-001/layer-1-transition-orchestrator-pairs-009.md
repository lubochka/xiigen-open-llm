# Layer 1 — Transition Orchestrator Pairs (009)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## Method note

File 009 covers TransitionType summary for B06: complete table with all combinations.

---

## B06-T1-009-001 — TransitionType summary table for B06

```
source_id: xiigen-b06-t1-009-001
source_title: XIIGen B06 T1 — TransitionType summary table
source_license_state: first_party
pair_kind: transition_orchestrator
```

B06 TransitionType Summary Table:

  Predicates                           | GlobalTransitionType | Sub-phase decisions
  -------------------------------------|----------------------|--------------------
  ["DOCUMENT"]                         | documentation        | 2.1
  ["DOCUMENT"] + stakeholder sig       | documentation        | 2.1 + 2.4
  ["DOCUMENT", "IMPLEMENT"]            | feature_addition     | 2.1 + 4.1
  ["DOCUMENT", "UI"]                   | feature_addition     | 2.1 + 1.1
  ["DOCUMENT", "IMPLEMENT", "UI"]      | feature_addition     | 2.1 + 4.1 + 1.1
  ["ARCHITECTURE", "DOCUMENT"]         | documentation        | 3.1 + 2.1
  ["ARCHITECTURE", "DOCUMENT", "IMP"]  | feature_addition     | 3.1 + 2.1 + 4.1
  ["ARCH", "DOC", "IMP", "UI"]         | feature_addition     | 3.1 + 2.1 + 4.1 + 1.1
  []                                   | null (knowledge-edge)| none

  Rules:
    1. IMPLEMENT present → feature_addition
    2. UI present → feature_addition
    3. Only DOCUMENT and/or ARCHITECTURE → documentation
    4. knowledge-edge → null TransitionType

---

## B06-T1-009-002 — TransitionType metadata fields

```
source_id: xiigen-b06-t1-009-002
source_title: XIIGen B06 T1 — TransitionType metadata fields
source_license_state: first_party
pair_kind: transition_orchestrator
```

Teaching note — TransitionType metadata:

  Each transition carries metadata explaining the classification:
    transition_classification_basis: the lexicon terms that drove the classification
    primary_intent: "create" / "document" / "design" / "implement" / "design_ux"
    domain_signal: the domain work type being signaled

  primary_intent mapping:
    feature_addition + IMPLEMENT → primary_intent: "implement" (if 4.1 transition)
    feature_addition + DOCUMENT  → primary_intent: "create" (new requirements artifact)
    documentation + DOCUMENT     → primary_intent: "document" (existing artifact)
    documentation + ARCHITECTURE → primary_intent: "design" (architecture documentation)
    feature_addition + UI        → primary_intent: "design_ux" (new UX design)

  domain_signal mapping:
    2.1 → "requirements_authoring" or "business_requirements"
    2.4 → "stakeholder_requirements"
    3.1 → "architecture_design" or "architecture_authoring"
    4.1 → "backend_implementation"
    1.1 → "ui_requirements" or "ui_requirements_authoring"

---

## B06-T1-009-003 — ContextCarryForward field

```
source_id: xiigen-b06-t1-009-003
source_title: XIIGen B06 T1 — ContextCarryForward field
source_license_state: first_party
pair_kind: transition_orchestrator
```

Teaching note — ContextCarryForward field:

  ContextCarryForward: true means the user context from the original request
  (UserRequest, evidence IDs, session state) is passed forward to the flow
  execution. The flow's N01 DomainSplitterUnit receives the full UserRequest.

  ContextCarryForward: false would mean the context is discarded (not used
  in B06 training pairs — always true for 2.1 requirements flows).

  EvidenceIds in the transition link back to the compiler output document IDs.
  This maintains the chain: compiler → router → factory → transition → flow.
  Each step in the chain references the previous step's output by document ID.
