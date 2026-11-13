# Layer 0-A — Compiler Pairs (032)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-032-001 — "paper prototype" alone → zero predicates

**Input:** "Paper prototype for the new onboarding flow."

```
Token scan:
"paper prototype" → gap-vibe-coding-prototype-001 (low-fidelity artifact-type framing; no action verb)
"onboarding flow" → domain noun; no action verb

Action verb scan: none present

Predicates: []
GapWarnings: [gap-vibe-coding-prototype-001]
RouteTarget: knowledge_edge
SCHEDULE_CAPABILITY_LEARNING: emitted (DomainId: software-development)
```

---

## B32-0A-032-002 — "paper prototype" + implement → IMPLEMENT fires

**Input:** "Create a paper prototype then implement the onboarding screens."

```
Token scan:
"create" → IMPLEMENT fires ✓ (create fires IMPLEMENT)
"paper prototype" → gap-vibe-coding-prototype-001
"implement" → IMPLEMENT fires ✓

Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-prototype-001]
RouteTarget: ImplementationFlow
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-032-003 — "paper prototype" + UI

**Input:** "Sketch out a paper prototype for the onboarding screen."

```
Token scan:
"sketch out" → not in IMPLEMENT lexicon (sketch = drawing verb; not code action)
"paper prototype" → gap-vibe-coding-prototype-001
"screen" → UI fires ✓

Note: "sketch" does not fire IMPLEMENT; "screen" fires UI

Predicates: ["UI"]
GapWarnings: [gap-vibe-coding-prototype-001]
RouteTarget: UIFlow
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-032-004 — "wireframe" → same family as paper prototype

**Input:** "Wireframe for the checkout screen."

```
Token scan:
"wireframe" → gap-vibe-coding-prototype-001 (low-fidelity artifact; same gap family)
"screen" → UI fires ✓

Predicates: ["UI"]
GapWarnings: [gap-vibe-coding-prototype-001]
RouteTarget: UIFlow
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-032-005 — "paper prototype" + "good enough": multiple qualifiers

**Input:** "Good enough paper prototype for the signup screen."

```
Token scan:
"good enough" → gap-vibe-coding-style-001
"paper prototype" → gap-vibe-coding-prototype-001
"screen" → UI fires ✓ (screen fires UI)
No additional action verb

Predicates: ["UI"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-prototype-001]
RouteTarget: UIFlow
CapabilityGapWarning: style_qualifier_noted
SCHEDULE_CAPABILITY_LEARNING: NOT emitted (predicates non-empty)
```

---
