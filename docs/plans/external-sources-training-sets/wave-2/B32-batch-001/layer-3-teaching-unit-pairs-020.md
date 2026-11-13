# Layer 3 — Teaching Unit Pairs (020)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: layer-3-teaching-unit-pairs

---

## B32-L3-020-001 — Gap + action verb: the core B32 mixed pattern teaching

**Teaching unit:** The most important B32 pattern is the co-occurrence of gap vocabulary with action verbs. The rule is simple and absolute: style qualifiers are warnings; action verbs fire predicates.

**Three-step mental model:**

1. Scan for action verbs first → fire predicates if found
2. Scan for gap vocabulary → emit gap warnings (never suppress predicates found in step 1)
3. Determine route based on predicates:
   - predicates = [] → knowledge_edge + SCHEDULE_CAPABILITY_LEARNING
   - predicates ≠ [] → route to active flow(s) + CapabilityGapWarning: style_qualifier_noted

---

## B32-L3-020-002 — Examples: gap + action verb; predicates always fire

**Master table:**

| Request | Gap vocabulary | Action verb | Predicates | Route |
|---|---|---|---|---|
| "Just make it work — build checkout" | just-works-001 | build | [IMPLEMENT] | ImplementationFlow |
| "Quick and dirty — implement billing" | quick-dirty-001 | implement | [IMPLEMENT] | ImplementationFlow |
| "Vibe code it — create an export" | style-001 | create (+ code) | [IMPLEMENT] | ImplementationFlow |
| "Hack together + write adapter" | hack-001 | write | [IMPLEMENT] | ImplementationFlow |
| "Scrappy build + test" | style-001 | build + test | [IMPLEMENT, TEST] | Impl + Test |

---

## B32-L3-020-003 — v6-02 corruption: what it looks like and why it's wrong

**v6-02 corruption definition:** Action verb present but predicates = [].

**Corrupted output example:**
```
Request: "Just make it work — build the checkout flow."
CORRUPT: Predicates: [] 
CORRUPT reason: "just make it work" = informal = no formal action needed
```

**Why this is wrong:**
- "build" is in IMPLEMENT lexicon
- IMPLEMENT MUST fire on "build"
- "just make it work" is gap vocabulary only — it cannot suppress IMPLEMENT
- Any output with "build/implement/create/write/code" present but predicates empty is v6-02 corrupt

---
