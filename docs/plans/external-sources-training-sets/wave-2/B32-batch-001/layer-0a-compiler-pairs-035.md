# Layer 0-A — Compiler Pairs (035)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-0a-compiler-pairs

---

## B32-0A-035-001 — gap + QA embedded: QA fires alongside gap

**Input:** "Quick and dirty build — QA the checkout flow."

```
Token scan:
"quick and dirty" → gap-vibe-coding-quick-dirty-001
"build" → IMPLEMENT fires ✓
"QA" → QA fires ✓ (positive directive form)
"checkout flow" → scope

Predicates: ["IMPLEMENT", "QA"]
GapWarnings: [gap-vibe-coding-quick-dirty-001]
RouteTarget: [ImplementationFlow, QAFlow]
CapabilityGapWarning: style_qualifier_noted
Note: QAFlow executes last in multi-predicate sequence
```

---

## B32-0A-035-002 — "scrappy" + QA: qualifier does not suppress QA

**Input:** "Scrappy but QA the registration form before release."

```
Token scan:
"scrappy" → gap-vibe-coding-style-001
"QA" → QA fires ✓ (positive directive form)
"registration form" → scope

Predicates: ["QA"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: QAFlow
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-035-003 — "good enough" + QA

**Input:** "Good enough quality — still QA the payment flow."

```
Token scan:
"good enough" → gap-vibe-coding-style-001
"QA" → QA fires ✓

Predicates: ["QA"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: QAFlow
CapabilityGapWarning: style_qualifier_noted
```

---

## B32-0A-035-004 — "minimum viable" + QA + IMPLEMENT

**Input:** "Minimum viable — implement the feature and QA it."

```
Token scan:
"minimum viable" → gap-vibe-coding-style-001
"implement" → IMPLEMENT fires ✓
"QA" → QA fires ✓

Predicates: ["IMPLEMENT", "QA"]
GapWarnings: [gap-vibe-coding-style-001]
CapabilityGapWarning: style_qualifier_noted
Note: QAFlow executes after ImplementationFlow in ordered sequence
```

---

## B32-0A-035-005 — "PoC" + QA + TEST: full quality chain despite gap framing

**Input:** "PoC build — implement, test, and QA the happy path."

```
Token scan:
"PoC" → gap-vibe-coding-prototype-001
"implement" → IMPLEMENT fires ✓
"test" → TEST fires ✓
"QA" → QA fires ✓

Predicates: ["IMPLEMENT", "TEST", "QA"]
GapWarnings: [gap-vibe-coding-prototype-001]
CapabilityGapWarning: style_qualifier_noted
Note: All three fire despite PoC framing; gap warning does not suppress any predicate
```

---
