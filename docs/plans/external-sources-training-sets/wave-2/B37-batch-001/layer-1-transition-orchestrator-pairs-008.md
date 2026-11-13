# Layer 1 — Transition Orchestrator Pairs (008)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: transition-orchestrator-pairs

---

## B37-L1-008-001 — IMPLEMENT+DOCUMENT: feature_addition; parallel eligible

**Input:** "Build the delivery tracking module and document the API requirements."

**Compiler output:** ["IMPLEMENT", "DOCUMENT"]

**Transition orchestrator:**
- TransitionType: feature_addition + documentation
- ContextCarryForward: false
- Next states: BackendServiceFlow41 (4.1) + DocumentationFlow (2.1)
- DomainId: "software-development"
- Parallel eligible: Yes (BackendServiceFlow41 and DocumentationFlow are independent)

**Gap IDs forwarded:**
- gap-project-managing-track-status-001 (delivery tracking intent)
- gap-project-managing-manage-delivery-001

---

## B37-L1-008-002 — IMPLEMENT+DOCUMENT parallel execution

**Why IMPLEMENT + DOCUMENT are parallel eligible:**
- BackendServiceFlow41 builds the service/module (software artifact)
- DocumentationFlow creates documentation (spec/requirements artifact)
- Neither depends on the other's completion
- Both can proceed simultaneously
- DomainId: "software-development" for both

**Contrast with ordered:** IMPLEMENT then TEST is ordered (cannot test before building). IMPLEMENT + DOCUMENT has no ordering dependency.

---

## B37-L1-008-003 — IMPLEMENT+ARCHITECTURE: feature_addition + architecture_design

**Input:** "Design the service architecture and build the module."

**Compiler output:** ["IMPLEMENT", "ARCHITECTURE"]

**Transition orchestrator:**
- TransitionType: feature_addition + architecture_design
- Next states: BackendServiceFlow41 (4.1) + ArchitectureFlow31 (3.1)
- DomainId: "software-development"
- Parallel eligible: Yes (but architecture typically informs build; soft ordering)

---

## B37-L1-008-004 — IMPLEMENT+ARCHITECTURE+DOCUMENT: three parallel

**Input:** "Define the module structure, build the service, and document the requirements."

**Compiler output:** ["IMPLEMENT", "ARCHITECTURE", "DOCUMENT"]

**Transition orchestrator:**
- TransitionType: feature_addition + architecture_design + documentation
- Next states: BackendServiceFlow41 + ArchitectureFlow31 + DocumentationFlow
- DomainId: "software-development"
- Parallel eligible: Yes for all three (soft preference: architecture before build, but documentation is independent)

---

## B37-L1-008-005 — Parallel eligibility rules for B37 transitions

**Parallel eligible combinations:**
- IMPLEMENT + DOCUMENT → Yes (independent outputs)
- IMPLEMENT + ARCHITECTURE → Yes (soft ordering: arch before build)
- ARCHITECTURE + DOCUMENT → Yes (independent)
- API_CONTRACT + DOCUMENT → Yes (independent)
- QA + DOCUMENT → Yes (independent)
- TEST + DOCUMENT → Yes (independent)

**Ordered combinations (not parallel):**
- IMPLEMENT then TEST → build before test
- TEST then QA (validate) → test before gating

**B37 gap handling is always separate from flow transitions:**
Gap warnings are recorded asynchronously; they do not participate in the parallel/ordered flow execution.
