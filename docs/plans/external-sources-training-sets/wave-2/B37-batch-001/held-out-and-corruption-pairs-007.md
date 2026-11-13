# Held-Out and Corruption Pairs (007)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-007-001 — HO: "build the service, track progress" → IMPLEMENT fires; track is gap warning

**Held-out request:** "Build the delivery tracking service and track progress weekly."

**Expected output:**
- "Build" → IMPLEMENT fires (Rule 9)
- "delivery" → gap
- "tracking" → gap (track family)
- "service" → IMPLEMENT fires (Rule 9)
- "track" → gap (Rule 2: track ≠ TEST)
- "progress" → gap (Rule 12)
- Predicates: ["IMPLEMENT"]
- DomainId: "software-development"
- Route: BackendServiceFlow41 (4.1)
- Gap IDs: gap-project-managing-track-progress-001, gap-project-managing-track-001, gap-project-managing-manage-delivery-001

---

## B37-HO-007-002 — HO: IMPLEMENT fires on service; track does not modify result

**Key teaching:** "track" in the same sentence as IMPLEMENT triggers does not affect the predicate result. "track" is gap regardless of what other tokens are present.

Variation 1: "Build the service." → ["IMPLEMENT"]
Variation 2: "Build the service and track progress." → ["IMPLEMENT"] (track adds gap warning only)
Variation 3: "Track progress and build the service." → ["IMPLEMENT"] (same result regardless of order)

The IMPLEMENT predicate is determined solely by IMPLEMENT tokens (build, service, module). "track" never modifies this.

---

## B37-HO-007-003 — COR: "track" fires TEST alongside IMPLEMENT (wrong)

**Corrupted output:** "Build the service and track progress." → ["IMPLEMENT", "TEST"] — "track" incorrectly fires TEST

**Why this is wrong:** Rule 2 — "track" is NOT in the TEST lexicon. TEST fires on: test, verify, assert, validate (QA), coverage, spec, case, suite, scenario, acceptance, unit, regression, e2e.

**Correct output:** ["IMPLEMENT"] — only IMPLEMENT fires. "track" → gap.

---

## B37-HO-007-004 — HO: build + track + review → IMPLEMENT + QA

**Request:** "Build the tracking module, review the implementation, and track delivery progress."

**Expected output:**
- "Build" → IMPLEMENT fires
- "tracking" → gap
- "module" → IMPLEMENT fires
- "review" → QA fires (Rule 19)
- "implementation" → IMPLEMENT fires (implement family)
- "track" → gap (Rule 2)
- "delivery" → gap
- "progress" → gap
- Predicates: ["IMPLEMENT", "QA"]
- DomainId: "software-development"
- Routes: BackendServiceFlow41 + QA sub-phase
- Gap IDs: gap-project-managing-track-status-001, gap-project-managing-track-001, gap-project-managing-manage-delivery-001, gap-project-managing-progress-001

---

## B37-HO-007-005 — HO: pure IMPLEMENT without any managing vocabulary

**Request:** "Build the notification service module."

**Expected output:**
- "Build" → IMPLEMENT fires
- "notification" → not in predicate lexicon directly
- "service" → IMPLEMENT fires
- "module" → IMPLEMENT fires
- Predicates: ["IMPLEMENT"]
- DomainId: "software-development"
- Gap IDs: none (no managing vocabulary present)

**Contrast with earlier examples:** No gap warnings when no managing vocabulary present. Gap warnings only appear when B37 managing vocabulary is in the sentence alongside predicate tokens.
