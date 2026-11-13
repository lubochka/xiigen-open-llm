# Held-Out and Corruption Pairs (013)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding gap
# Production date: 2026-05-06 | pair_kind: held-out-and-corruption-pairs

---

## B32-HO-013-001 — HELD-OUT: "scrappy architecture" → ARCHITECTURE fires; style is warning

**Request:** "Scrappy architecture for the microservices system."

**Expected (correct) output:**
```
Predicates: ["ARCHITECTURE"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: ArchitectureFlow31
TransitionType: documentation
DomainId: "software-development"
```

**Reasoning:**
- "scrappy" → gap-vibe-coding-style-001 (warning only)
- "architecture" → ARCHITECTURE fires ✓

---

## B32-HO-013-002 — CORRUPTION: "scrappy" suppresses ARCHITECTURE (WRONG)

**Corrupted output:**
```
Predicates: []
Reason: "scrappy architecture" = informal/exploratory = no formal predicate
```

**Why this is wrong:**
- "architecture" is in ARCHITECTURE lexicon → ARCHITECTURE must fire
- "scrappy" is gap vocabulary → warning only; never suppresses ARCHITECTURE
- This is v6-02 corruption pattern

**Correct predicates:** ["ARCHITECTURE"]

---

## B32-HO-013-003 — HELD-OUT: "scrappy build + test" → IMPLEMENT + TEST both fire

**Request:** "Scrappy build — implement the payment service and test it."

**Expected output:**
```
Predicates: ["IMPLEMENT", "TEST"]
GapWarnings: [gap-vibe-coding-style-001]
RouteTarget: [ImplementationFlow, TestFlow]
DomainId: "software-development"
```

---
