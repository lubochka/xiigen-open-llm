# Layer 0-C — Knowledge-Edge Pairs (010)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: knowledge-edge-pairs

---

## B37-0C-010-001 — Complete B37 gap vocabulary list with gap IDs

**Primary B37 gap IDs (registered):**

| Gap ID | Vocabulary covered |
|--------|-------------------|
| gap-project-managing-track-001 | track (standalone) |
| gap-project-managing-coordinate-001 | coordinate |
| gap-project-managing-blockers-001 | blockers, blockers holding up |
| gap-project-managing-dependencies-team-001 | team dependencies, project dependencies (no API co-occurrence) |
| gap-project-managing-stakeholder-update-001 | stakeholder update intent (not documentation) |
| gap-project-managing-escalation-001 | escalation, escalate |
| gap-project-managing-accountability-001 | accountability, ownership, RACI |
| gap-project-managing-follow-up-001 | follow-up, follow up |
| gap-project-managing-action-items-001 | action items, action item |
| gap-project-managing-track-status-001 | on-track, off-track, track status |
| gap-project-managing-rag-status-001 | RAG-status, RAG report |
| gap-project-managing-raid-log-001 | RAID log, RAID, risks+assumptions+issues+dependencies |
| gap-project-managing-progress-001 | progress, progress update |
| gap-project-managing-status-report-001 | status report, weekly report, delivery report |
| gap-project-managing-manage-delivery-001 | manage delivery, delivery status, delivery pace |
| gap-project-managing-work-stream-001 | workstream, work stream, workstreams |

---

## B37-0C-010-002 — Compound gap IDs (registered)

| Gap ID | Vocabulary covered |
|--------|-------------------|
| gap-project-managing-coordinate-blockers-001 | coordinate + blockers together |
| gap-project-managing-escalation-accountability-001 | escalation + accountability together |
| gap-project-managing-track-progress-001 | track progress (compound phrase) |

**When to use compound vs individual gap IDs:**
- If only one term present: use individual ID
- If compound phrase appears together: use compound ID AND individual IDs for each component
- Example: "coordinate blockers" → gap-project-managing-coordinate-blockers-001 + gap-project-managing-coordinate-001 + gap-project-managing-blockers-001

---

## B37-0C-010-003 — Gap ID selection rules

**Selection rule 1: Use most specific gap ID**
"track progress" → gap-project-managing-track-progress-001 (not just gap-project-managing-track-001)

**Selection rule 2: Include both compound and component IDs**
"coordinate blockers" → gap-project-managing-coordinate-blockers-001, gap-project-managing-coordinate-001, gap-project-managing-blockers-001

**Selection rule 3: gap-project-managing-manage-delivery-001 is a mixed-signal warning**
This ID signals that "manage" + "delivery" creates a false positive risk (IMPLEMENT may fire on other tokens). Flag it as a warning, not just a gap.

**Selection rule 4: Sequence numbers are fixed at 001**
All registered gap IDs use sequence 001. Do not invent new sequence numbers without explicit registration.

---

## B37-0C-010-004 — Gap vocabulary that never fires any predicate

**These tokens appear in B37 requests and NEVER fire any predicate:**

manage, coordinate, track, progress, blockers, escalate, escalation, accountability, follow-up, action items, on-track, off-track, RAG-status, RAID, workstream, status, report (standalone), delivery (standalone), programme, steering committee, PMO

None of these tokens appear in: TEST lexicon, QA lexicon, DOCUMENT lexicon, ARCHITECTURE lexicon, IMPLEMENT lexicon, or API_CONTRACT lexicon.

---

## B37-0C-010-005 — Gap vocabulary that COINCIDENTALLY fires predicates (false positives)

**These B37-adjacent tokens DO fire predicates (false positives in managing context):**

| Token | Predicate | Rule |
|-------|----------|------|
| stakeholder | DOCUMENT | Rule 6 — fires always |
| structure | ARCHITECTURE | Rule 7 — fires always |
| define | DOCUMENT | Rule 8 |
| document | DOCUMENT | Rule 8 |
| criteria | DOCUMENT | Rule 8 |
| requirements | DOCUMENT | Rule 8 |
| describe | DOCUMENT | Rule 8 |
| review | QA | Rule 19 — fires always |
| test | TEST | Rule 21 — fires always |
| validate | QA | |
| build | IMPLEMENT | Rule 9 |
| service | IMPLEMENT | Rule 9 |
| module | IMPLEMENT | Rule 9 |
| create | IMPLEMENT | Rule 9 |
| dependencies + API co-occurrence | API_CONTRACT | Rule 5 |
