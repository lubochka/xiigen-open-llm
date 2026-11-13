# Layer 0-A — Compiler Pairs (008)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: compiler-pairs

---

## B37-0A-008-001 — "test" fires TEST even in estimation context

**Input request:**
"Test the delivery estimates with the stakeholders and validate the timeline assumptions."

**Compiler analysis:**

Token scan:
- "Test" → TEST fires on "test" ALWAYS (Rule 21: even if context is "test the estimates")
- "delivery" → gap vocabulary
- "estimates" → not in any predicate lexicon
- "stakeholders" → DOCUMENT fires (Rule 6)
- "validate" → QA fires on "validate" (validate is in QA lexicon)
- "timeline" → not in any predicate lexicon
- "assumptions" → not in any predicate lexicon

Result: ["TEST", "DOCUMENT", "QA"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001 ("delivery estimates" managing intent not captured)
- gap-project-managing-track-status-001 (timeline tracking intent not captured)

Note: "test" fires TEST regardless of whether the sentence is about software testing or testing/validating estimates in a project managing context. The lexical token governs, not the semantic intent.

---

## B37-0A-008-002 — "test" in planning sentence: TEST fires unconditionally

**Input request:**
"Test the assumptions in the project plan before presenting to the steering committee."

**Compiler analysis:**

Token scan:
- "Test" → TEST fires (Rule 21)
- "assumptions" → not in any predicate lexicon
- "plan" → not in any predicate lexicon
- "steering" → not in any predicate lexicon

Result: ["TEST"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-raid-log-001 (assumptions = RAID-A)

---

## B37-0A-008-003 — "validate" fires QA: confirmation

**Input request:**
"Validate the delivery timeline against the original project baseline and report the variance."

**Compiler analysis:**

Token scan:
- "Validate" → QA fires on "validate"
- "delivery" → gap vocabulary
- "timeline" → not in any predicate lexicon
- "report" → DOCUMENT does NOT fire on "report" (Rule 3)
- "variance" → not in any predicate lexicon

Result: ["QA"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001 ("delivery timeline" managing intent)
- gap-project-managing-status-report-001 ("report the variance" managing intent)

---

## B37-0A-008-004 — "test" + "stakeholder" + "validate": all three predicates

**Input request:**
"Test the project estimates, validate the assumptions with stakeholders, and report back."

**Compiler analysis:**

Token scan:
- "Test" → TEST fires (Rule 21)
- "validate" → QA fires
- "stakeholders" → DOCUMENT fires (Rule 6)
- "report" → DOCUMENT does NOT fire on "report" alone
- "estimates" → gap vocabulary
- "assumptions" → gap vocabulary

Result: ["TEST", "QA", "DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-raid-log-001 (estimates + assumptions both RAID-related)

---

## B37-0A-008-005 — Confirming: "test" fires TEST even with no test artifact mentioned

**Input request:**
"Test whether the current delivery pace is sustainable given the team's capacity."

**Compiler analysis:**

Token scan:
- "Test" → TEST fires (Rule 21)
- "delivery" → gap vocabulary
- "capacity" → not in any predicate lexicon
- "sustainable" → not in any predicate lexicon

Result: ["TEST"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-manage-delivery-001 ("delivery pace" managing intent)
- gap-project-managing-track-status-001 (pace tracking intent)

Note: Even "test whether" in a project planning question fires TEST. The lexical rule is absolute.
