# Cross-Layer Synthesis (004)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## CLS-004 — Six-predicate: Full LMS platform cycle with guidance context

**Request**: "Architect the LMS platform, integrate with Okta SSO, build the course engine, secure user data, write tests, document the API, and QA the release — and mentor the team throughout."

### Layer 0-A (Compiler)
Tokens: architect+platform → ARCHITECTURE; integrate → API_CONTRACT; build → IMPLEMENT; secure → SECURITY; write tests → TEST; document → DOCUMENT; QA → QA; mentor → gap
Predicate set = ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]
"mentor" → gap warning (seven predicates fire; GapHandler suppressed)

### Layer 0-B (Router)
Seven predicates → Seven flows selected.

### Layer 0-C (Knowledge Edge)
"mentor the team throughout" = guidance frame for the entire request → gap warning (not gap — predicates present).
"LMS platform" = software system → IMPLEMENT + ARCHITECTURE target.
All seven action verbs fire independently; guidance wrapper does not collapse predicates.

### Layer 0-D (Flow Factory)
Seven flows. TransitionType: feature_addition.

### Layer 1 (Transition Orchestrator)
Orchestration: ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow → E2ETestingFlow53 ‖ DocumentationFlow → QAGatingFlow73
QA absolute last.

### Layer 2 (Flow Execution)
All seven flows approved. Full cycle complete.

### Layer 3 (Teaching)
Key insight: "mentor the team throughout" = guidance vocabulary in the framing/context position. It does NOT suppress or reduce any of the seven predicate-firing action verbs. The guidance framing becomes a gap warning; all seven code-action verbs fire their predicates.

**TransitionType**: feature_addition | **Gap**: gap warning for "mentor" (not full SCHEDULE_CAPABILITY_LEARNING)
