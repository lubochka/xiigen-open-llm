# Held-Out and Corruption Pairs (004)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-004-001 — HO: "accountability structure" → ARCHITECTURE+DOCUMENT

**Held-out request:** "Define the accountability structure for the programme delivery workstreams."

**Expected output:**
- "Define" → DOCUMENT fires (Rule 8)
- "accountability" → gap
- "structure" → ARCHITECTURE fires (Rule 7)
- "programme" → not in any predicate lexicon
- "delivery" → gap
- "workstreams" → gap
- Predicates: ["ARCHITECTURE", "DOCUMENT"]
- DomainId: "software-development"
- Routes: 3.1 + 2.1
- Gap IDs: gap-project-managing-accountability-001, gap-project-managing-manage-delivery-001, gap-project-managing-work-stream-001

---

## B37-HO-004-002 — COR: ARCHITECTURE suppressed in governance context (wrong)

**Corrupted output:** "Define the accountability structure" → ["DOCUMENT"] only — ARCHITECTURE suppressed because "it's organizational, not software architecture"

**Why this is wrong:** Rule 7 — "structure" → ARCHITECTURE fires ALWAYS. No context override.

**Correct output:** ["ARCHITECTURE", "DOCUMENT"] — both fire. Both routes: 3.1 + 2.1.

**Corruption label:** ARCHITECTURE suppression in organizational governance context (variant of v6-02 pattern applied to ARCHITECTURE)

---

## B37-HO-004-003 — HO: governance structure without define

**Request:** "Establish the governance structure for the steering committee."

**Expected output:**
- "Establish" → not in any predicate lexicon
- "governance" → not in any predicate lexicon
- "structure" → ARCHITECTURE fires (Rule 7)
- "steering" → not in any predicate lexicon
- Predicates: ["ARCHITECTURE"]
- DomainId: "software-development"
- Gap IDs: gap-project-managing-accountability-001 (governance = accountability)

---

## B37-HO-004-004 — COR: "structure" suppressed AND "define" suppressed

**Corrupted output:** "Define the RACI structure" → [] — both ARCHITECTURE and DOCUMENT suppressed "because this is organizational design"

**Why this is wrong:** "define" → DOCUMENT fires (Rule 8). "structure" → ARCHITECTURE fires (Rule 7). Neither can be suppressed based on organizational intent.

**Correct output:** ["ARCHITECTURE", "DOCUMENT"]

---

## B37-HO-004-005 — HO: escalation structure with QA

**Request:** "Review the escalation structure and define the approval criteria."

**Expected output:**
- "Review" → QA fires (Rule 19)
- "escalation" → gap (Rule 13)
- "structure" → ARCHITECTURE fires (Rule 7)
- "define" → DOCUMENT fires (Rule 8)
- "approval" → not in any predicate lexicon
- "criteria" → DOCUMENT fires (Rule 8)
- Predicates: ["QA", "ARCHITECTURE", "DOCUMENT"]
- DomainId: "software-development"
- Routes: QA sub-phase + 3.1 + 2.1
- Gap IDs: gap-project-managing-escalation-001
