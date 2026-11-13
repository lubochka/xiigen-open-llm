# Layer 3 — Teaching Unit Pairs (037)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-037-001 — Teaching: negation patterns with gap vocabulary

**Teaching unit:** Gap + negation interaction rules

1. Gap vocabulary present + all predicates negated → predicates = [] → knowledge_edge → SCL
2. Gap vocabulary present + some predicates negated, some positive → only positive predicates fire + gap warning
3. No gap vocabulary + negation only → no SCL, no gap warning

---

## B32-L3-037-002 — Teaching: no gap vocabulary = no CapabilityGapWarning

**Teaching unit:** CapabilityGapWarning only appears when gap vocabulary is present

"Build the service, no tests needed." → no gap vocabulary → no CapabilityGapWarning in output.

CapabilityGapWarning = "style_qualifier_noted" appears only when:
- Predicates are non-empty AND gap vocabulary was present in the request.

---

## B32-L3-037-003 — Teaching: SCL fires only from gap vocabulary

**Teaching unit:** SCL rules (definitive)

SCL (SCHEDULE_CAPABILITY_LEARNING) emits when:
- Gap vocabulary is present AND predicates = []

SCL does NOT emit when:
- No gap vocabulary (even if predicates = [])
- Gap vocabulary present but predicates are non-empty

---

## B32-L3-037-004 — Teaching: negations do not affect DomainId

**Teaching unit:** DomainId is invariant across all patterns

DomainId = "software-development" in all outputs including:
- Gap + negations only
- No gap + negations + positive predicates
- Knowledge_edge paths
- Active flow paths

No pattern changes DomainId.

---
