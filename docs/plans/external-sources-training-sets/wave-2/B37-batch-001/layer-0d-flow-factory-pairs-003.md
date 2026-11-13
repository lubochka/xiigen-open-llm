# Layer 0-D — Flow Factory Pairs (003)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: flow-factory-pairs

---

## B37-0D-003-001 — DOCUMENT factory: DocumentationFlow (stakeholder firing)

**Input:** "Send a progress update to stakeholders and report the on-track status for each workstream."

**Compiler output:** ["DOCUMENT"]

**Flow factory decision:**
- DOCUMENT predicate → DocumentationFlow selected (sub-phase 2.1)
- Factory: DocumentationFlow
- Reason for firing: "stakeholders" token
- Intent was: managing communication (false positive)

**Factory configuration:**
- Flow: DocumentationFlow
- Sub-phase: 2.1
- DomainId: "software-development"
- Context carries: gap warnings for managing vocabulary

**Gap warnings forwarded to factory context:**
- gap-project-managing-progress-001
- gap-project-managing-track-status-001
- gap-project-managing-work-stream-001
- gap-project-managing-stakeholder-update-001

---

## B37-0D-003-002 — DOCUMENT factory: "define" triggers in managing sentence

**Input:** "Define the escalation process and share the criteria with the project team."

**Compiler output:** ["DOCUMENT"]

**Flow factory decision:**
- DOCUMENT (define, criteria) → DocumentationFlow
- Factory: DocumentationFlow / sub-phase 2.1
- DomainId: "software-development"

**Gap warnings:**
- gap-project-managing-escalation-001

---

## B37-0D-003-003 — DOCUMENT factory with multiple triggers

**Input:** "Describe the stakeholder communication plan and document the reporting requirements."

**Compiler output:** ["DOCUMENT"]

**Flow factory decision:**
- DOCUMENT fires on: describe, stakeholder, document, requirements (four tokens, one predicate)
- Factory: DocumentationFlow / sub-phase 2.1
- Multiple triggers do not create multiple factory instances — one DocumentationFlow

**Gap warnings:**
- gap-project-managing-stakeholder-update-001 (intent: managing communication, not documentation)

---

## B37-0D-003-004 — DOCUMENT factory context: false positive advisory

**Factory advisory: DocumentationFlow invoked in B37 managing context**

When DocumentationFlow is invoked in a B37 request:
- The managing intent (status reporting, stakeholder notification) is NOT addressed by the flow
- The flow will process documentation work (requirements, specs, criteria)
- The gap warnings indicate the unaddressed managing intent
- The factory is correct per lexicon rules; the mismatch is a capability gap signal

**This advisory is embedded in the factory output** when gap-project-managing-stakeholder-update-001 is present:
"Note: Stakeholder notification intent detected. DocumentationFlow invoked due to lexical trigger. Managing capability gap recorded."

---

## B37-0D-003-005 — DOCUMENT factory selection rules for B37

**When DocumentationFlow is selected in B37 context:**

Trigger tokens in B37 sentences that select DocumentationFlow:
- "stakeholder(s)" — most common B37 false positive trigger
- "define" — in governance/criteria sentences
- "document" — explicitly in the request
- "criteria" — in escalation criteria sentences
- "requirements" — in process documentation sentences
- "describe" — in framework description sentences
- "spec" — in technical specification sentences within managing framing

**Factory selection is unconditional:** Once DOCUMENT fires, DocumentationFlow is selected. The managing context does not modify the factory selection.
