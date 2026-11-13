# Held-Out and Corruption Pairs (006)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-006-001 — HO: "review escalation log" → QA fires

**Held-out request:** "Review the escalation log and identify overdue items."

**Expected output:**
- "Review" → QA fires (Rule 19: even in managing context)
- "escalation" → gap (Rule 13)
- "log" → not in any predicate lexicon
- "identify" → not in any predicate lexicon
- "overdue" → not in any predicate lexicon
- Predicates: ["QA"]
- DomainId: "software-development"
- Route: QA sub-phase (ReviewFlow)
- Gap IDs: gap-project-managing-escalation-001

---

## B37-HO-006-002 — COR: QA suppressed in managing context (wrong)

**Corrupted output:** "Review the escalation log" → [] — QA suppressed because "reviewing an escalation log is project management, not QA"

**Why this is wrong:** Rule 19 — "review" → QA fires ALWAYS. Even in project managing context. No intent override.

**Correct output:** ["QA"], DomainId "software-development", QA sub-phase

**Corruption label:** QA suppression for "review" in managing context (variant of v6-02 applied to QA)

---

## B37-HO-006-003 — HO: "review" + RAID log

**Request:** "Review all RAID log entries before the monthly governance meeting."

**Expected output:**
- "Review" → QA fires (Rule 19)
- "RAID" → gap
- Predicates: ["QA"]
- DomainId: "software-development"
- Gap IDs: gap-project-managing-raid-log-001

---

## B37-HO-006-004 — HO: "escalation" without "review" → knowledge_edge

**Request:** "Escalate all outstanding RAID log issues to the senior steering committee."

**Expected output:**
- "Escalate" → gap (Rule 13: escalate not in any predicate lexicon)
- "RAID" → gap
- "issues" → gap (RAID-I)
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: gap-project-managing-escalation-001, gap-project-managing-raid-log-001

**Contrast with 006-001:** Adding "review" to this sentence would fire QA and change everything. Without "review" → zero predicates.

---

## B37-HO-006-005 — HO: "review" + managing vocabulary: predicate dominates

**Request:** "Review the escalation process, track blocker resolution, and follow up on action items."

**Expected output:**
- "Review" → QA fires
- "escalation" → gap
- "track" → gap (Rule 2)
- "blocker" → gap
- "follow up" → gap
- "action items" → gap
- Predicates: ["QA"]
- DomainId: "software-development"
- Route: QA sub-phase
- Gap IDs: gap-project-managing-escalation-001, gap-project-managing-track-001, gap-project-managing-blockers-001, gap-project-managing-follow-up-001, gap-project-managing-action-items-001

**Teaching: One "review" token dominates the entire sentence of managing vocabulary.**
