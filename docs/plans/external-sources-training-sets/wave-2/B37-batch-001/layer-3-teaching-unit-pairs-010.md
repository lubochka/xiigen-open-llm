# Layer 3 — Teaching-Unit Pairs (010)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: teaching-unit-pairs

---

## B37-L3-010-001 — Teaching: v6 corruption patterns for B37

**v6 corruption overview:**

Three distinct v6 corruption types exist in B37. All stem from the same root cause: the model overrides lexical firing rules based on semantic intent analysis.

**Root cause:** The model "understands" the request is about project management and tries to be "helpful" by:
- v6-01: Inventing a predicate that "should" exist for managing vocabulary
- v6-02: Suppressing a predicate that "doesn't apply" to the managing context
- v6-03: Setting DomainId based on intent rather than predicate scan result

The correct model behaviour is to NEVER apply semantic intent analysis to override lexical rules.

---

## B37-L3-010-002 — Teaching: v6-01 detailed pattern

**v6-01: Inventing MANAGEMENT or COORDINATION predicate**

Trigger: Request contains "coordinate", "manage", "track", or similar managing vocabulary with clear managing intent.

v6 corrupt thought process: "This is a project management request. The system should route it to project management handling. I'll emit a MANAGEMENT predicate."

Why it's wrong: MANAGEMENT is not in the predicate registry. The factory registry has no ManagementFlow. Any factory invocation based on a MANAGEMENT predicate fails at factory lookup.

Correct thought process: "These tokens are not in any predicate lexicon. Zero predicates. knowledge_edge. SCL emitted. Gap IDs forwarded."

---

## B37-L3-010-003 — Teaching: v6-02 detailed pattern

**v6-02: "stakeholder" DOCUMENT suppressed**

Trigger: Request contains "stakeholders" with clear managing intent (notification, update, communication).

v6 corrupt thought process: "The user wants to notify stakeholders. This is not documentation. I should suppress DOCUMENT to avoid routing to DocumentationFlow."

Why it's wrong: "stakeholder" → DOCUMENT fires ALWAYS (Rule 6). The lexical rule has no managing exception. Suppression breaks consistency with all non-managing sentences where "stakeholder" correctly fires DOCUMENT.

Correct thought process: "DOCUMENT fires on stakeholders. Route to 2.1. Emit gap-project-managing-stakeholder-update-001 to capture notification intent."

---

## B37-L3-010-004 — Teaching: v6-03 detailed pattern

**v6-03: DomainId "project-management" set when predicate fired**

Trigger: Request contains a predicate-firing token (e.g., "build", "service", "module") alongside heavy managing vocabulary.

v6 corrupt thought process: "The request is mostly about project management. I should set DomainId 'project-management' to reflect the dominant topic."

Why it's wrong: DomainId is computed from predicate scan result, not topic weighting. The rule is binary: zero predicates → "project-management"; any predicates → "software-development". No majority voting.

Correct thought process: "IMPLEMENT fired (build/service/module). DomainId = 'software-development'. Gap warnings for managing vocabulary."

---

## B37-L3-010-005 — Teaching: three-question v6 diagnostic test

**Question 1 (v6-01 test):**
"Coordinate blockers and manage workstreams." — what is the predicate list?
Correct: []
v6-01 fail: ["MANAGEMENT"] or ["COORDINATION"]

**Question 2 (v6-02 test):**
"Notify stakeholders of current status." — what is the predicate list?
Correct: ["DOCUMENT"]
v6-02 fail: []

**Question 3 (v6-03 test):**
"Build the tracking module for the project management team." — what is DomainId?
Correct: "software-development" (IMPLEMENT fired: build + module)
v6-03 fail: "project-management"

All three must be answered correctly for v6 certification.
