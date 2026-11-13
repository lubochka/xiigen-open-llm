# Layer 0-A — Compiler Pairs (015)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-015-001 — "project-charter" vocabulary: zero predicates fire

**Input request:**
"Create the project charter for the new platform migration programme."

**Compiler analysis:**

Token scan:
- "Create" → IMPLEMENT fires (Rule 9: create fires IMPLEMENT)
- "project charter" → not in any predicate lexicon (gap vocabulary)
- "platform" → not directly in predicate lexicon
- "migration" → not in any predicate lexicon
- "programme" → not in any predicate lexicon

Result: ["IMPLEMENT"]

DomainId: "software-development" (IMPLEMENT predicate fired — note: "create" lexically fires IMPLEMENT even when referring to a document)

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001

Note: "create the project charter" feels like document creation, but the compiler fires IMPLEMENT on the lexical token "create" regardless of intent. This is a known false positive for IMPLEMENT.

---

## B37-0A-015-002 — "project-charter" with "define": DOCUMENT fires

**Input request:**
"Define the scope and objectives in the project charter before kickoff."

**Compiler analysis:**

Token scan:
- "Define" → DOCUMENT fires (Rule 8)
- "scope" → not in DOCUMENT trigger list (scope ≠ spec, description, etc.)
- "objectives" → not in predicate lexicon
- "project charter" → gap
- "kickoff" → not in predicate lexicon

Result: ["DOCUMENT"]

DomainId: "software-development" (DOCUMENT predicate fired)

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001

---

## B37-0A-015-003 — "project-charter" pure management: zero predicates

**Input request:**
"Circulate the project charter and collect sign-off from all workstream leads."

**Compiler analysis:**

Token scan:
- "Circulate" → not in any predicate lexicon
- "project charter" → gap
- "collect" → not in any predicate lexicon
- "sign-off" → not in any predicate lexicon
- "workstream" → gap

Result: []

DomainId: "project-management"

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001
- gap-project-managing-work-stream-001
- gap-project-managing-accountability-001 (sign-off = accountability vocabulary)

SCHEDULE_CAPABILITY_LEARNING: emitted

---

## B37-0A-015-004 — "project-charter" with "stakeholder": DOCUMENT fires

**Input request:**
"Share the project charter with all stakeholders and confirm the delivery timeline."

**Compiler analysis:**

Token scan:
- "Share" → not in predicate lexicon
- "project charter" → gap
- "stakeholders" → DOCUMENT fires (Rule 6)
- "confirm" → not in predicate lexicon
- "delivery timeline" → gap (delivery vocabulary)

Result: ["DOCUMENT"]

DomainId: "software-development" (DOCUMENT predicate fired on "stakeholders")

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001
- gap-project-managing-stakeholder-update-001

---

## B37-0A-015-005 — "project-charter" with "review": QA fires

**Input request:**
"Review the project charter against the original scope assumptions and track any deviations."

**Compiler analysis:**

Token scan:
- "Review" → QA fires (Rule 19)
- "project charter" → gap
- "scope" → not in predicate lexicon
- "assumptions" → gap (RAID-A)
- "track" → gap (Rule 2: track ≠ TEST)
- "deviations" → not in predicate lexicon

Result: ["QA"]

DomainId: "software-development" (QA predicate fired)

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001
- gap-project-managing-raid-log-001 (assumptions = RAID-A)
- gap-project-managing-track-001
