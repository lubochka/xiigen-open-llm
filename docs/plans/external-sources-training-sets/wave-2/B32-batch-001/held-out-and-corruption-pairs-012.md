# Held-Out and Corruption Pairs (012)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-012-001 — Mixed held-out case 1: vibe + SECURITY

**Request:** "Vibe out the security layer — just make the tokens safe."

**Analysis:**
- "vibe out" → gap-vibe-coding-style-001 ("out" not in lexicon; vibe is gap)
- "security" → SECURITY fires ✓
- "just make" → not "just make it work" (different phrasing); "make" alone is not in IMPLEMENT lexicon
- "tokens safe" → domain nouns in security context

**Expected output:**
```
Predicates: ["SECURITY"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: SecurityFlow
TransitionType: feature_addition
DomainId: "software-development"
```

---

## B32-HO-012-002 — Mixed held-out case 2: hack + DOCUMENT

**Request:** "Hack together some documentation for the API — nothing comprehensive."

**Analysis:**
- "hack together" → gap-vibe-coding-hack-001
- "documentation" → DOCUMENT fires ✓ (root "document")
- "API" → API_CONTRACT fires ✓
- "nothing comprehensive" → gap qualifier (variant of nothing-fancy-001)

**Expected output:**
```
Predicates: ["DOCUMENT", "API_CONTRACT"]
GapWarnings: [gap-vibe-coding-hack-001, gap-vibe-coding-nothing-fancy-001]
RouteTarget: [DocumentFlow, APIContractFlow]
TransitionType: documentation (no IMPLEMENT)
DomainId: "software-development"
```

---

## B32-HO-012-003 — Mixed held-out case 3: "prototype" + UI

**Request:** "Vibe out a prototype screen for the user profile."

**Analysis:**
- "vibe out" → gap-vibe-coding-style-001
- "prototype" → gap-vibe-coding-prototype-001 (no action verb; but wait — does "vibe out" provide an action?)
  - "out" is not in IMPLEMENT lexicon
  - No action verb present
- "screen" → UI fires ✓

**Expected output:**
```
Predicates: ["UI"]
GapWarnings: [gap-vibe-coding-style-001, gap-vibe-coding-prototype-001]
RouteTarget: UIFlow
TransitionType: feature_addition
DomainId: "software-development"
```

Note: UI fires on "screen"; no IMPLEMENT (no action verb); "prototype" is gap warning.

---

## B32-HO-012-004 — Mixed held-out case 4: rough-cut + TEST + IMPLEMENT

**Request:** "Quick and dirty — write rough-cut tests and implement the cache invalidation."

**Analysis:**
- "quick and dirty" → gap-vibe-coding-quick-dirty-001
- "write" → IMPLEMENT fires ✓
- "rough-cut" → gap-vibe-coding-rough-cut-001
- "tests" → TEST fires ✓ ("tests" root matches "test")
- "implement" → IMPLEMENT fires ✓ (deduplicated)
- "cache invalidation" → domain noun

**Expected output:**
```
Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-quick-dirty-001, gap-vibe-coding-rough-cut-001]
RouteTarget: [BackendServiceFlow41, TestFlow]
TransitionType: feature_addition
DomainId: "software-development"
OrderConstraint: TestFlow after BackendServiceFlow41
```

---
