# Layer 3 — Teaching-Unit Pairs (013)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: teaching-unit-pairs

---

## B37-L3-013-001 — Teaching: risk log is gap vocabulary

**Concept:** "Risk log" is B37 gap vocabulary with no registered predicate.

Q: "Maintain the risk log." → predicates?
A: []. DomainId "project-management". knowledge_edge. SCL emitted.

Q: "Track risks in the risk log." → predicates?
A: []. Both "track" (Rule 2) and "risk log" are gap. DomainId "project-management".

Q: "Review the risk log." → predicates?
A: [QA]. "review" fires QA. DomainId "software-development". Gap warning: raid-log-001.

---

## B37-L3-013-002 — Teaching: risk log predicate scenarios

**Learning pairs for risk log:**

Q: "Document all risks in the risk log." → predicates?
A: [DOCUMENT]. DomainId "software-development".

Q: "Build a risk log dashboard." → predicates?
A: [IMPLEMENT]. DomainId "software-development".

Q: "Review the risk log entries." → predicates?
A: [QA]. DomainId "software-development".

Q: "Escalate risks from the risk log." → predicates?
A: []. "escalate" not in any predicate lexicon (Rule 13). DomainId "project-management".

---

## B37-L3-013-003 — Teaching: risk log gap ID

**Gap ID:** gap-project-managing-raid-log-001

Risk log = RAID-R (Risks component of RAID). The same gap ID covers: risk log, risk register, RAID-R, risk tracking, risk management.

**Exam question:** What is the gap ID for "monitor the risk log"?
A: gap-project-managing-raid-log-001

---

## B37-L3-013-004 — Teaching: false positive scenarios for risk log

**Risk log false positive table:**

| Trigger token in sentence | Predicate fires | Gap warning raised |
|--------------------------|----------------|-------------------|
| review | QA | raid-log-001 |
| document | DOCUMENT | raid-log-001 |
| define | DOCUMENT | raid-log-001 |
| build | IMPLEMENT | raid-log-001 |
| create | IMPLEMENT | raid-log-001 |
| test | TEST | raid-log-001 |

In each case: the predicate fires on the trigger token; the risk log intent is captured in gap warning gap-project-managing-raid-log-001.

---

## B37-L3-013-005 — Teaching: risk log exam

**10-question risk log exam:**

1. "Maintain risk log." → []
2. "Track risks in log." → []
3. "Monitor risk log." → []
4. "Escalate risks." → []
5. "Review risk log." → [QA]
6. "Document risks." → [DOCUMENT]
7. "Define risk criteria." → [DOCUMENT]
8. "Build risk log." → [IMPLEMENT]
9. "Test risk mitigations." → [TEST]
10. DomainId for question 5? → "software-development"

Questions 1-4: knowledge_edge, SCL emitted, gap-project-managing-raid-log-001.
Questions 5-9: predicate-appropriate flows, no SCL.
