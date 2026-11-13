# Layer 3 — Teaching Unit Pairs (004)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-004-001 — Style qualifiers never suppress action predicates: complete rule

**Teaching unit:** This is the most important B32 rule. Style qualifiers (vibe vocabulary, quality modifiers, urgency words) are ALWAYS gap warnings. They NEVER interfere with predicate detection. An action predicate fires if and only if a registered action verb is present, regardless of surrounding context.

**Complete rule:**
```
FOR EVERY request R containing style qualifier Q and action verb V:
  PREDICATE fires on V
  GAP WARNING emitted for Q
  These two outcomes are independent
  Q cannot cancel, modify, or suppress the predicate fired by V
```

---

## B32-L3-004-002 — Style qualifier taxonomy for B32

**All B32 style qualifiers (terms that become gap warnings, never predicates):**

| Style qualifier type | Examples |
|---|---|
| Vibe style | vibe, vibe code, creative mode |
| Speed qualifiers | quick, fast, rapid |
| Quality reducers | quick and dirty, nothing fancy, rough |
| Informality markers | hack together, cobble together, throwaway |
| Minimum bar | just make it work, good enough, just get it working |
| Exploration | exploratory, spike |
| Draft quality | rough-cut, prototype (when alone) |
| Intuition | feel right, intuition-driven, feels right |

---

## B32-L3-004-003 — Teaching examples: style qualifiers with all 7 predicates

**Rule verified for all 7 predicates:**

| Request | Style qualifier | Predicate | Style suppresses? |
|---|---|---|---|
| "Quick and dirty — implement X" | quick and dirty | IMPLEMENT | NO |
| "Nothing fancy — design the architecture" | nothing fancy | ARCHITECTURE | NO |
| "Vibe code the screen" | vibe code | UI + IMPLEMENT | NO |
| "Throwaway — integrate the OAuth API" | throwaway | API_CONTRACT | NO |
| "Hack together the security layer" | hack together | SECURITY | NO |
| "Quick unit tests — nothing fancy" | nothing fancy + quick | TEST | NO |
| "Rough-cut documentation" | rough-cut | DOCUMENT | NO |
| "Vibe QA the whole thing" | vibe | QA | NO |

Style qualifiers never suppress any of the 7 predicates.

---

## B32-L3-004-004 — Why this rule exists: preventing v6-02 corruptions

**v6-02 corruption:** A system that treats style qualifiers as routing modifiers will incorrectly route requests to knowledge_edge when valid action verbs are present.

**Consequence of v6-02:**
- User says "just make it work — implement the handler"
- System routes to knowledge_edge (thinking "just make it work" cancels IMPLEMENT)
- User's valid implementation request is never fulfilled
- User is confused: they asked to implement something but got no implementation

**Prevention:** Style qualifiers → gap warnings only. Action verbs → predicates only. These are independent parsing channels.

---
