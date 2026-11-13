# Layer 2 — Capability-Gap-Extended Pairs (010)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: capability-gap-extended-pairs

---

## B37-L2-010-001 — "manage delivery with build": IMPLEMENT fires; manage/delivery → gap warnings

**Input:** "Manage the delivery of the authentication service and build the access control module."

**Full mixed-signal trace:**

Step 1 — Compiler scan:
- "Manage" → not in any predicate lexicon (Rule 10) → gap
- "delivery" → gap
- "authentication" → not directly a predicate trigger (context: service type)
- "service" → IMPLEMENT fires (Rule 9: service is IMPLEMENT trigger)
- "build" → IMPLEMENT fires (Rule 9)
- "access control" → not in any predicate lexicon
- "module" → IMPLEMENT fires (Rule 9)

Step 2 — Predicate list: ["IMPLEMENT"] (three triggers, one predicate)

Step 3 — DomainId: "software-development"

Step 4 — Route: BackendServiceFlow41 (4.1)

Step 5 — SCHEDULE_CAPABILITY_LEARNING: NOT emitted

Step 6 — CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001 (managing intent unmet)

**Key: "Manage the delivery of" is pure managing vocabulary. IMPLEMENT fires on "service", "build", "module". gap-project-managing-manage-delivery-001 is a mixed-signal warning.**

---

## B37-L2-010-002 — gap-project-managing-manage-delivery-001: mixed-signal warning purpose

**This gap ID is unique in B37 because it is a mixed-signal warning, not a pure gap.**

**Why "mixed-signal":**
- "manage" alone → zero predicates (Rule 10)
- "delivery" alone → zero predicates
- "manage delivery of the service" → IMPLEMENT fires on "service"
- The managing framing creates a confusing signal: intent is managing, but IMPLEMENT fires

**The gap ID records:** The system routed to BackendServiceFlow41, but the original intent included delivery management. The delivery management portion is unaddressed.

---

## B37-L2-010-003 — "manage delivery": scenarios where IMPLEMENT does NOT fire

**Input:** "Manage the delivery programme and coordinate all workstream leads."

**Trace:**
- "Manage" → gap (Rule 10)
- "delivery" → gap
- "programme" → not in any predicate lexicon
- "coordinate" → gap (Rule 11)
- "workstream" → gap

Result: []

DomainId: "project-management"

Gap IDs: gap-project-managing-manage-delivery-001, gap-project-managing-coordinate-001, gap-project-managing-work-stream-001

SCL emitted

**Contrast with 010-001:** No IMPLEMENT-triggering tokens (no service/build/module) → pure gap.

---

## B37-L2-010-004 — "delivery service" token ambiguity

**Input:** "Manage the delivery service for the enterprise platform."

**Trace:**
- "Manage" → gap (Rule 10)
- "delivery" → gap
- "service" → IMPLEMENT fires (Rule 9: service is an IMPLEMENT trigger)
- "enterprise" → not in any predicate lexicon
- "platform" → not in any predicate lexicon

Result: ["IMPLEMENT"]

DomainId: "software-development"

CapabilityGapWarnings: gap-project-managing-manage-delivery-001

**Note:** "delivery service" fires IMPLEMENT because "service" is in the IMPLEMENT lexicon. Even though "delivery service" in this context means a delivery/logistics service (not a software service), the lexical rule fires IMPLEMENT on the token "service".

---

## B37-L2-010-005 — Five "manage delivery" scenario outcomes

1. "Manage delivery." → [] (pure gap — no IMPLEMENT triggers)
2. "Manage delivery of the service." → ["IMPLEMENT"] (service fires)
3. "Manage delivery and build the module." → ["IMPLEMENT"] (build + module fire)
4. "Manage delivery and update stakeholders." → ["DOCUMENT"] (stakeholders fires)
5. "Manage delivery and review progress." → ["QA"] (review fires)

Pattern: "Manage delivery" is always gap vocabulary. What fires depends entirely on other tokens in the sentence.
