# Layer 0-A — Compiler Pairs (024)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-024-001 — Complete gap vocabulary reference: extended B37 terms

**Extended B37 gap vocabulary (new terms from 013-024):**

| Token / Phrase | Gap ID | Notes |
|---------------|--------|-------|
| risk log | gap-project-managing-raid-log-001 | RAID-R vocabulary |
| change request | gap-project-managing-manage-delivery-001 | change management vocabulary |
| project charter | gap-project-managing-manage-delivery-001 | programme document (gap: no DOCUMENT if no trigger) |
| decision log | gap-project-managing-manage-delivery-001 | project governance vocabulary |
| lessons learned | gap-project-managing-manage-delivery-001 | retrospective vocabulary |
| accountability | gap-project-managing-accountability-001 | ownership/RACI vocabulary |
| accountability structure | ARCHITECTURE fires on "structure" | false positive; gap-project-managing-accountability-001 also raised |
| team/cross-workstream dependencies | gap-project-managing-dependencies-team-001 | no API co-occurrence |
| API/integration dependencies | API_CONTRACT fires | API co-occurrence present |
| stakeholder (any context) | DOCUMENT fires | false positive; gap-project-managing-stakeholder-update-001 also raised |
| structure (any context) | ARCHITECTURE fires | always — governance, org, team, accountability |
| review (any context) | QA fires | always — managing review, RAID review, etc. |
| define (any context) | DOCUMENT fires | always |

---

## B37-0A-024-002 — New vocabulary predicate scan: confirm all gap

**Confirm zero-predicate status for all new extended B37 vocabulary:**

| Token | Predicate fires? | Correct result |
|-------|----------------|----------------|
| risk log | No | [] |
| change request | No | [] |
| project charter | No | [] |
| decision log | No | [] |
| lessons learned | No | [] |
| accountability | No | [] |
| RACI | No | [] |
| sign-off | No | [] |
| programme board | No | [] |
| workstream ownership | No | [] |

All ten tokens: zero predicates. DomainId "project-management". SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B37-0A-024-003 — False positive trigger tokens confirmed in B37-013-024 range

**False positive confirmations:**

1. "accountability structure" → ARCHITECTURE fires on "structure"
   Correct: ["ARCHITECTURE"], DomainId "software-development"
   Not: [], DomainId "project-management"

2. "stakeholder update/report/notification" → DOCUMENT fires on "stakeholder"
   Correct: ["DOCUMENT"], DomainId "software-development"
   Not: [], DomainId "project-management"

3. "review the risk log" → QA fires on "review"
   Correct: ["QA"], DomainId "software-development"
   Not: [], DomainId "project-management"

4. "define the change request process" → DOCUMENT fires on "define"
   Correct: ["DOCUMENT"], DomainId "software-development"
   Not: [], DomainId "project-management"

5. "API dependencies" → API_CONTRACT fires (api co-occurrence)
   Correct: ["API_CONTRACT"], DomainId "software-development"
   Not: [], DomainId "project-management"

---

## B37-0A-024-004 — Combined extended and base B37 vocabulary scan

**Input request:**
"Track the risk log, coordinate change requests, follow up on lessons-learned action items, and escalate blockers to stakeholders."

**Compiler analysis:**

Token scan:
- "Track" → gap (Rule 2)
- "risk log" → gap
- "coordinate" → not in predicate lexicon (Rule 11)
- "change requests" → gap
- "follow up" → gap
- "lessons-learned" → gap
- "action items" → gap
- "escalate" → not in predicate lexicon (Rule 13)
- "blockers" → gap
- "stakeholders" → DOCUMENT fires (Rule 6)

Result: ["DOCUMENT"]

DomainId: "software-development"

CapabilityGapWarnings:
- gap-project-managing-track-001
- gap-project-managing-raid-log-001
- gap-project-managing-coordinate-001
- gap-project-managing-manage-delivery-001
- gap-project-managing-follow-up-001
- gap-project-managing-action-items-001
- gap-project-managing-escalation-001
- gap-project-managing-blockers-001
- gap-project-managing-stakeholder-update-001

---

## B37-0A-024-005 — DomainId rule for extended vocabulary: confirmation

**Complete DomainId decision confirmation for extended B37 vocabulary:**

Zero-predicate requests (DomainId = "project-management"):
- Pure risk log management
- Pure change request coordination
- Pure project charter circulation
- Pure decision log tracking
- Pure lessons-learned sessions
- Pure accountability coordination
- Pure team dependency mapping

One-predicate requests (DomainId = "software-development"):
- Any of the above + "stakeholder" → ["DOCUMENT"]
- Any of the above + "structure" → ["ARCHITECTURE"]
- Any of the above + "review" → ["QA"]
- Any of the above + "define" → ["DOCUMENT"]
- Any of the above + "build/create/implement/service/module" → ["IMPLEMENT"]
- Any of the above + API co-occurrence + "dependencies" → ["API_CONTRACT"]

**Rule 14 is absolute:** ANY predicate → "software-development". No exceptions.
