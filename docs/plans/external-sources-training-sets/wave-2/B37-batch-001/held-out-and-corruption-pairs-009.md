# Held-Out and Corruption Pairs (009)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-009-001 — HO: "stakeholder status update" → DOCUMENT fires

**Held-out request:** "Send the weekly stakeholder status update with the current RAG-status."

**Expected output:**
- "Send" → not in any predicate lexicon
- "weekly" → not in any predicate lexicon
- "stakeholder" → DOCUMENT fires (Rule 6: ALWAYS)
- "status" → gap (Rule 4)
- "update" → not in DOCUMENT trigger list
- "RAG-status" → gap
- Predicates: ["DOCUMENT"]
- DomainId: "software-development"
- Route: DocumentationFlow (2.1)
- Gap IDs: gap-project-managing-stakeholder-update-001, gap-project-managing-rag-status-001, gap-project-managing-status-report-001

---

## B37-HO-009-002 — COR: DOCUMENT suppressed for stakeholder status update (wrong)

**Corrupted output:** "Send weekly stakeholder status update." → [] — DOCUMENT suppressed because "this is a communication, not documentation"

**Why this is wrong:** Rule 6 — "stakeholder" → DOCUMENT fires ALWAYS. "Status update communication" context does not suppress DOCUMENT.

**Correct output:** ["DOCUMENT"], DomainId "software-development"

**Corruption type:** v6-02

---

## B37-HO-009-003 — HO: "stakeholder" in meeting invite sentence

**Request:** "Schedule a stakeholder review meeting to discuss escalation status."

**Expected output:**
- "Schedule" → not in any predicate lexicon
- "stakeholder" → DOCUMENT fires (Rule 6)
- "review" → QA fires (Rule 19)
- "meeting" → not in any predicate lexicon
- "escalation" → gap (Rule 13)
- "status" → gap (Rule 4)
- Predicates: ["DOCUMENT", "QA"]
- DomainId: "software-development"
- Routes: DocumentationFlow + QA sub-phase
- Gap IDs: gap-project-managing-escalation-001, gap-project-managing-stakeholder-update-001

---

## B37-HO-009-004 — HO: "stakeholder" in requirements context (correct DOCUMENT)

**Request:** "Collect stakeholder requirements for the delivery reporting module."

**Expected output:**
- "Collect" → not in DOCUMENT trigger list directly, but "stakeholder" + "requirements" fire DOCUMENT
- "stakeholder" → DOCUMENT fires (Rule 6)
- "requirements" → DOCUMENT fires (Rule 8)
- "delivery" → gap
- "reporting" → not in predicate lexicon directly
- "module" → IMPLEMENT fires (Rule 9)
- Predicates: ["DOCUMENT", "IMPLEMENT"]
- DomainId: "software-development"
- Gap IDs: gap-project-managing-manage-delivery-001

**Note:** In this case, DOCUMENT is a TRUE positive (genuine requirements documentation intent). No stakeholder-update gap warning needed because intent is genuinely documentation.

---

## B37-HO-009-005 — HO: distinguishing true positive vs false positive "stakeholder" DOCUMENT

**True positive (documentation intent):**
- "Gather stakeholder requirements" → DOCUMENT (correct — documentation work)
- "Review stakeholder specifications" → DOCUMENT + QA (correct)
- "Define acceptance criteria for stakeholders" → DOCUMENT + TEST (correct)

**False positive (managing intent):**
- "Update stakeholders on status" → DOCUMENT (false positive — emit stakeholder-update gap warning)
- "Notify stakeholders of escalation" → DOCUMENT (false positive)
- "Report to stakeholders on blockers" → DOCUMENT (false positive)

**The predicate fires in both cases. The gap warning distinguishes them.**
