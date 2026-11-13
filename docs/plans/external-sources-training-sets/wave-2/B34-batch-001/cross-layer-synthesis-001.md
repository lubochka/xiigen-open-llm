# Cross-Layer Synthesis (001)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## CLS-001 — IMPLEMENT+DOCUMENT: Build onboarding portal + write the guide

**Request**: "Build the developer onboarding portal and write the onboarding guide for new engineers."

### Layer 0-A (Compiler)
Tokens: build → IMPLEMENT; portal → software component; write → DOCUMENT; guide → documentation artifact
Predicates: ["IMPLEMENT", "DOCUMENT"]

### Layer 0-B (Router)
IMPLEMENT → ImplementationFlow41
DOCUMENT → DocumentationFlow

### Layer 0-C (Knowledge Edge)
"build a portal" confirmed IMPLEMENT (software system noun). "write a guide" confirmed DOCUMENT (documentation artifact).
No gap vocabulary in direct action verbs. Guidance context of "onboarding" = object, not predicate suppressor.

### Layer 0-D (Flow Factory)
Two flows selected. TransitionType: feature_addition.
Parallel-eligible: portal build and guide writing have no data dependency.

### Layer 1 (Transition Orchestrator)
Orchestration: ImplementationFlow41 ‖ DocumentationFlow (parallel)

### Layer 2 (Flow Execution)
ImplementationFlow41: OnboardingPortal scoped — registration, environment wizard, checklist. APPROVED.
DocumentationFlow: DeveloperOnboardingGuide scoped — 4 sections, new engineer audience. APPROVED.

### Layer 3 (Teaching)
Key rule: "build" + software noun → IMPLEMENT; "write" + documentation noun → DOCUMENT. Both fire from single request. Guidance context "onboarding" does not reduce predicates.

**TransitionType**: feature_addition | **Gap logged**: None
