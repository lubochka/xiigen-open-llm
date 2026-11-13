# Held-Out and Corruption Pairs (011)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-011-001 — Three v6 corruptions: v6-01 (MANAGEMENT invented)

**Held-out request:** "Coordinate escalations and manage delivery blockers."

**v6-01 corrupted output:**
- Predicates: ["MANAGEMENT"]
- DomainId: "project-management" (wrong even if MANAGEMENT existed)
- Factory: ManagementFlow (doesn't exist)

**Correct output:**
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-escalation-001, gap-project-managing-manage-delivery-001, gap-project-managing-blockers-001

**v6-01 markers:**
1. "MANAGEMENT" not in predicate registry → invented predicate
2. ManagementFlow not in factory registry → invented factory
3. Even if DomainId "project-management" was set, with a non-empty predicate list it should be "software-development"

---

## B37-HO-011-002 — Three v6 corruptions: v6-02 (DOCUMENT suppressed)

**Held-out request:** "Notify stakeholders of the critical escalation and provide status."

**v6-02 corrupted output:**
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- Justification: "Notifying stakeholders of escalation status is project management, not documentation."

**Correct output:**
- "stakeholders" → DOCUMENT fires (Rule 6: ALWAYS)
- Predicates: ["DOCUMENT"]
- DomainId: "software-development"
- Route: DocumentationFlow (2.1)
- Gap IDs: gap-project-managing-escalation-001, gap-project-managing-stakeholder-update-001, gap-project-managing-status-report-001

**v6-02 markers:**
1. "stakeholders" present but DOCUMENT absent → suppression occurred
2. DomainId "project-management" despite stakeholder being present → symptom of suppression

---

## B37-HO-011-003 — Three v6 corruptions: v6-03 (DomainId bleeding)

**Held-out request:** "Manage the delivery of the authentication service and build the access control module."

**v6-03 corrupted output:**
- Predicates: ["IMPLEMENT"]
- DomainId: "project-management" (wrong — IMPLEMENT fired)
- Factory: BackendServiceFlow41

**Correct output:**
- Predicates: ["IMPLEMENT"] (correct)
- DomainId: "software-development" (IMPLEMENT fired → "software-development")
- Factory: BackendServiceFlow41 (correct)
- Gap IDs: gap-project-managing-manage-delivery-001

**v6-03 markers:**
1. Predicate list non-empty ("IMPLEMENT")
2. DomainId set to "project-management" → impossible when predicates fired
3. Factory correctly selected but DomainId contradicts factory activation

---

## B37-HO-011-004 — Compound corruption: v6-01 + v6-03

**Held-out request:** "Coordinate the tracking service delivery and manage workstream escalations."

**Compound corrupted output:**
- Predicates: ["MANAGEMENT", "IMPLEMENT"] (v6-01: MANAGEMENT invented)
- DomainId: "project-management" (v6-03: wrong DomainId despite predicates)
- Factory: ManagementFlow + BackendServiceFlow41

**Correct output:**
- "Coordinate" → gap
- "tracking" → gap (track family)
- "service" → IMPLEMENT fires
- "delivery" → gap
- "manage" → gap (Rule 10)
- "workstream" → gap
- "escalations" → gap (Rule 13)
- Predicates: ["IMPLEMENT"]
- DomainId: "software-development"
- Factory: BackendServiceFlow41
- Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-track-status-001, gap-project-managing-manage-delivery-001, gap-project-managing-work-stream-001, gap-project-managing-escalation-001

---

## B37-HO-011-005 — All three v6 corruptions in one sentence

**Held-out request:** "Coordinate escalations, notify stakeholders of blockers, and build the tracking module."

**All-three corrupted output (v6 maximum failure):**
- Predicates: ["MANAGEMENT", "IMPLEMENT"] — v6-01 (MANAGEMENT), v6-02 (DOCUMENT suppressed for stakeholder)
- DomainId: "project-management" — v6-03 (wrong despite IMPLEMENT)
- Factory: ManagementFlow + BackendServiceFlow41

**Correct output:**
- "Coordinate" → gap
- "escalations" → gap
- "stakeholders" → DOCUMENT fires (cannot suppress)
- "blockers" → gap
- "build" → IMPLEMENT fires
- "tracking" → gap
- "module" → IMPLEMENT fires
- Predicates: ["DOCUMENT", "IMPLEMENT"]
- DomainId: "software-development"
- Factory: DocumentationFlow + BackendServiceFlow41
- Gap IDs: gap-project-managing-coordinate-001, gap-project-managing-escalation-001, gap-project-managing-blockers-001, gap-project-managing-track-status-001, gap-project-managing-stakeholder-update-001
