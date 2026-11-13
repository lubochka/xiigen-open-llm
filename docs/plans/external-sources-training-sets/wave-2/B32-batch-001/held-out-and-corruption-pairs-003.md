# Held-Out and Corruption Pairs (003)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-003-001 — HELD-OUT: "throw together a service" → IMPLEMENT fires on "service"?

**Request:** "Throw together a quick service for the order processing."

**Analysis:**
- "throw together" → gap-vibe-coding-hack-001 (gap, not predicate)
- "service" → domain noun; "service" alone does not fire IMPLEMENT
  IMPLEMENT fires on action verbs: implement, build, create, write, code
  "service" is a NOUN, not a verb
- "order processing" → domain noun

**Expected output:**
```
Predicates: []
GapWarnings: [gap-vibe-coding-hack-001]
RouteTarget: knowledge_edge
DomainId: "software-development"
CapabilityAction: SCHEDULE_CAPABILITY_LEARNING
```

**Note:** "service" does NOT fire IMPLEMENT. Only action verbs fire IMPLEMENT. "service" is a domain noun indicating the target artifact, not an action.

---

## B32-HO-003-002 — CORRUPTION: IMPLEMENT suppressed by "throw together" (WRONG)

**Alternate request:** "Build a quick service for the order processing — throw it together."

**Corrupted output:**
```
Predicates: []
Reason: "throw it together" signals informal request; IMPLEMENT suppressed
```

**Why this is wrong:**
- "build" is in the IMPLEMENT lexicon; IMPLEMENT fires unconditionally
- "throw it together" is gap vocabulary; it cannot suppress IMPLEMENT
- v6-02 corruption pattern: framing suppresses predicates (WRONG)

**Correct output:**
```
Predicates: ["IMPLEMENT"]
GapWarnings: [gap-vibe-coding-hack-001]
RouteTarget: BackendServiceFlow41
TransitionType: feature_addition
DomainId: "software-development"
```

---

## B32-HO-003-003 — CORRUPTION: "service" fires IMPLEMENT (WRONG)

**Corrupted output for 003-001:**
```
Request: "Throw together a quick service for order processing."
WRONG: Predicates: ["IMPLEMENT"]
WRONG reasoning: "service" is the target of an IMPLEMENT action
```

**Why this is wrong:**
- "service" is a noun; it does not fire IMPLEMENT
- IMPLEMENT fires on action verbs (implement, build, create, write, code)
- Domain nouns (service, module, handler, API) indicate the artifact but do not trigger predicates
- Without an action verb, predicates = []

---
