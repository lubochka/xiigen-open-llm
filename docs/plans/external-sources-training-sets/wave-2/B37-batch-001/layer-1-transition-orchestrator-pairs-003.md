# Layer 1 — Transition Orchestrator Pairs (003)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-003-001 — DOCUMENT in managing context: documentation TransitionType

**Input:** "Send a progress update to stakeholders and report the on-track status."

**Compiler output:** ["DOCUMENT"]

**Transition orchestrator:**
- TransitionType: documentation (DOCUMENT predicate → documentation transition)
- ContextCarryForward: false (new request, no prior flow)
- Next state: DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap IDs forwarded: gap-project-managing-progress-001, gap-project-managing-track-status-001, gap-project-managing-stakeholder-update-001

**Orchestrator advisory:** TransitionType "documentation" is correct per predicate, but the managing intent is a documentation false positive. The orchestrator does not override this — it routes correctly per lexicon.

---

## B37-L1-003-002 — DOCUMENT: "define" triggers documentation transition

**Input:** "Define the escalation criteria and document the approval process."

**Compiler output:** ["DOCUMENT"]

**Transition orchestrator:**
- TransitionType: documentation
- ContextCarryForward: false
- Next state: DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap IDs: gap-project-managing-escalation-001

---

## B37-L1-003-003 — DOCUMENT from prior gap context: transition from knowledge_edge

**Scenario:** User was in knowledge_edge state (prior managing request), now issues DOCUMENT-triggering request.

**Prior state:** knowledge_edge
**Input:** "Now document the escalation criteria and share with stakeholders."

**Compiler output:** ["DOCUMENT"]

**Transition orchestrator:**
- TransitionType: documentation (from knowledge_edge to DocumentationFlow)
- ContextCarryForward: false (knowledge_edge has no carry-forward context)
- Next state: DocumentationFlow (2.1)
- DomainId: "software-development"
- Gap IDs: gap-project-managing-escalation-001, gap-project-managing-stakeholder-update-001

---

## B37-L1-003-004 — DOCUMENT + ARCHITECTURE: documentation + architecture transition

**Input:** "Define the governance structure and document the escalation criteria."

**Compiler output:** ["ARCHITECTURE", "DOCUMENT"]

**Transition orchestrator:**
- TransitionType: documentation + architecture_design (compound)
- ContextCarryForward: false
- Next states: DocumentationFlow (2.1) + ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Parallel transition eligible: Yes

---

## B37-L1-003-005 — documentation TransitionType: complete rules

**When TransitionType = "documentation":**
- DOCUMENT predicate fired
- Preceding context: any (new request, prior gap, prior other flow)
- Next state: DocumentationFlow (2.1)
- DomainId: "software-development"

**When TransitionType = "documentation" in B37 context:**
- Almost always a false positive (stakeholder, define, criteria in managing sentences)
- Gap warnings forwarded
- Managing intent unaddressed by DocumentationFlow
- This is the correct behaviour per current lexicon rules
