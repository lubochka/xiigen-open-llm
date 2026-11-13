# Layer 0-C — Knowledge-Edge Pairs (011)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B38-0C-011-001 — Complete B38 vocabulary table

### Fires QA (Quality Assurance predicate)
review, approve, approved, approval, verify, verification, validate, validation,
accept, acceptance (in quality context), quality, audit (release context),
sign-off, sign off, compliance, inspect, inspection

### Fires IMPLEMENT (Implementation predicate)
service (ALWAYS), build (ALWAYS including artifacts/provenance),
pipeline (code artifact context), implementation, code (code-authoring context),
module (code context), component (code-authoring context)

### Fires DOCUMENT (Documentation predicate)
criteria (ALWAYS — all qualifiers), document, documentation

### Fires SECURITY (Security predicate)
security, vulnerability, audit (in security context — co-fires with QA)

### Fires BOTH QA and SECURITY
audit + security (when both present in same sentence — rule 19)

### B38 Gap Vocabulary (CapabilityGapWarning, no predicate)
go-live, checklist, rollout, cutover, runbook, launch, on-call,
monitor, execute (execution context), deployment (operational context),
release-gate (compound), rollback, threshold

---

## B38-0C-011-002 — Terms with dual classification (context-dependent)

"audit":
- Release context only → QA
- Security context (with "security") → QA + SECURITY

"compliance":
- Quality context → QA
- Security context → SECURITY support (still also QA)

"component":
- Code-authoring sentence → IMPLEMENT
- Pure process sentence (no code action) → context word only

"check":
- Quality-gate context → QA (borderline)
- Checklist context → GAP (checklist absorbs)

---

## B38-0C-011-003 — Terms that NEVER fire any predicate in B38

monitor, execute (execution), deployment, go-live, checklist, rollout,
cutover, runbook, launch, on-call, rollback, threshold, milestone,
window, schedule (as noun), procedure, team, organization, stakeholder,
coordination, handover, transition (non-code)

These terms, appearing alone, guarantee either a gap warning (if other predicates fire)
or knowledge_edge (if they are the only operative terms).
