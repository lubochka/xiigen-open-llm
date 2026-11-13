# Cross-Layer Synthesis (007)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: cross-layer-synthesis

---

## B37-CLS-007-001 — ARCHITECTURE+DOCUMENT: "define accountability structure and document for stakeholders"

**Input:** "Define the accountability structure and document the governance process for all stakeholders."

**Full cross-layer trace: ARCHITECTURE + DOCUMENT both fire from managing sentence**

---

### Layer 0-A (Compiler)

Token scan:
- "Define" → DOCUMENT fires (Rule 8: define fires DOCUMENT)
- "accountability structure" → "structure" fires ARCHITECTURE (Rule 7: structure always)
- "accountability" → gap
- "document" → DOCUMENT fires (Rule 8)
- "governance process" → not in predicate lexicon
- "stakeholders" → DOCUMENT fires (Rule 6)

Three DOCUMENT triggers (define, document, stakeholders) → ONE DOCUMENT predicate
One ARCHITECTURE trigger (structure) → ONE ARCHITECTURE predicate

Predicate list: ["DOCUMENT", "ARCHITECTURE"]

---

### Layer 0-B (Router)

DOCUMENT → DocumentationFlow (2.1)
ARCHITECTURE → ArchitectureFlow31 (3.1)
DomainId: "software-development" (predicates fired)
Routes: 2.1 + 3.1 (parallel eligible)

---

### Layer 0-C (Knowledge Edge)

Gap vocabulary identified alongside predicates:
- "accountability" → gap-project-managing-accountability-001
- "governance process" → not in gap lexicon (governance = gap adjacent but no specific gap ID)

Managing intent advisory: governance accountability process (unaddressed by either flow)

---

### Layer 0-D (Flow Factory)

DOCUMENT → DocumentationFlow (2.1)
ARCHITECTURE → ArchitectureFlow31 (3.1)
Both factories selected (parallel eligible)
DomainId: "software-development"
SCHEDULE_CAPABILITY_LEARNING: NOT emitted

---

### Layer 1 (Transition Orchestrator)

TransitionType: documentation + architecture_design (two predicates, two transitions)
ContextCarryForward: false
Next states: DocumentationFlow (2.1) + ArchitectureFlow31 (3.1)
Gap IDs forwarded: [gap-project-managing-accountability-001]

---

### Layer 2 (Capability Gap Extended)

Mixed-signal trace recorded:
- Primary routing: DOCUMENT + ARCHITECTURE → 2.1 + 3.1
- Managing intent: governance accountability documentation for stakeholders (partially addressed by DocumentationFlow, not addressed by ArchitectureFlow31)
- Gap IDs: gap-project-managing-accountability-001

---

## B37-CLS-007-002 — Full output summary

```
Predicates: ["DOCUMENT", "ARCHITECTURE"]
DomainId: "software-development"
Routes: DocumentationFlow (2.1) + ArchitectureFlow31 (3.1)
TransitionType: documentation + architecture_design
ContextCarryForward: false
SCHEDULE_CAPABILITY_LEARNING: NOT emitted
CapabilityGapWarnings: [
  gap-project-managing-accountability-001
]
```

---

## B37-CLS-007-003 — Why both DOCUMENT and ARCHITECTURE fire simultaneously

Multiple trigger tokens in one sentence produce multiple predicates:
- "define" → DOCUMENT
- "document" → DOCUMENT (same predicate, one emission)
- "stakeholders" → DOCUMENT (same predicate, one emission)
- "structure" → ARCHITECTURE

Result: ONE DOCUMENT predicate (from three triggers) + ONE ARCHITECTURE predicate (from one trigger). Two predicates total.

---

## B37-CLS-007-004 — v6 failure modes

| v6 corruption | What goes wrong | Correct value |
|--------------|----------------|--------------|
| v6-02 | DOCUMENT suppressed for "stakeholders" in governance context | DOCUMENT must fire — no suppression |
| v6-03 | DomainId "project-management" despite ARCHITECTURE firing | DomainId "software-development" — predicates fired |
| v6-01 | GOVERNANCE predicate added alongside ARCHITECTURE and DOCUMENT | ["DOCUMENT", "ARCHITECTURE"] only — no GOVERNANCE |

All three v6 types are present as risks in this mixed sentence.
