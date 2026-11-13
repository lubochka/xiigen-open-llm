# Held-Out and Corruption Pairs (002)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-002-001 — HO: "RAG-status report" → knowledge_edge

**Held-out request:** "Generate the RAG-status report for Q3 and circulate to the programme board."

**Expected output:**
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: gap-project-managing-rag-status-001, gap-project-managing-status-report-001

**Reasoning:** "RAG-status" and "report" are both gap vocabulary. No predicate fires. DomainId "project-management".

---

## B37-HO-002-002 — COR: "report" fires DOCUMENT (wrong)

**Corrupted output:** "Generate the RAG-status report" → ["DOCUMENT"] — "report" fires DOCUMENT

**Why this is wrong:** Rule 3 — "report" does NOT fire DOCUMENT. DOCUMENT fires on: document, spec, define, describe, requirement, story, criteria, stakeholder, functional, behavior, use-case. "report" is NOT on this list.

**Correct output:** [] (zero predicates)

**Corruption label:** false-positive DOCUMENT from "report"

---

## B37-HO-002-003 — HO variant: "RAG-status" with stakeholder

**Request:** "Share the RAG-status report with all stakeholders."

**Expected output:**
- "stakeholders" → DOCUMENT fires (Rule 6)
- "RAG-status" → gap
- "report" → DOCUMENT does NOT fire (Rule 3)
- Predicates: ["DOCUMENT"]
- DomainId: "software-development"
- Gap IDs: gap-project-managing-rag-status-001, gap-project-managing-status-report-001, gap-project-managing-stakeholder-update-001

**Key:** With "stakeholders", DOCUMENT fires. Without "stakeholders", zero predicates.

---

## B37-HO-002-004 — COR: "RAG-status" fires QA (wrong)

**Corrupted output:** "Review RAG-status for all workstreams." → predicates correctly ["QA"] from "review"
Wait — this is CORRECT. "review" → QA fires.

**Revised corruption test:** "Check RAG-status for all workstreams." → ["QA"] (wrong — "check" does not fire QA)

**Correct output for "check":** []

**Corruption label:** "check" incorrectly equated with "review" for QA firing. "Check" does not fire any predicate.

---

## B37-HO-002-005 — HO: pure RAG-status without predicates

**Request:** "Produce an amber RAG-status for workstream 4 due to escalating blockers."

**Expected output:**
- "Produce" → not in IMPLEMENT trigger list
- "RAG-status" → gap
- "workstream" → gap
- "escalating" → gap (escalation family)
- "blockers" → gap
- Predicates: []
- DomainId: "project-management"
- Gap IDs: gap-project-managing-rag-status-001, gap-project-managing-work-stream-001, gap-project-managing-escalation-001, gap-project-managing-blockers-001
- SCL: emitted
