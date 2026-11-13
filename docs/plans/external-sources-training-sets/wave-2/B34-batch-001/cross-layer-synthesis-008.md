# Cross-Layer Synthesis (008)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## CLS-008 — Multiple embedded predicates: IMPLEMENT+API_CONTRACT+TEST all fire alongside guidance vocabulary

**Request**: "Coach through building the auth service, walk through integrating it with the identity provider, and guide through writing the contract tests — mentoring the team throughout."

### Layer 0-A (Compiler)
Tokens:
- "coach through building the auth service" → coach = guidance; building auth service → IMPLEMENT
- "walk through integrating it with the identity provider" → walk-through = guidance; integrating → API_CONTRACT
- "guide through writing the contract tests" → guide = guidance; writing contract tests → TEST
- "mentoring the team throughout" → mentor = guidance (context/framing)
Predicate set = ["IMPLEMENT", "API_CONTRACT", "TEST"]
CapabilityGapWarning: coach, walk-through, guide, mentor = all gap-skills-guidance-mentor-001

### Layer 0-B (Router)
Predicates = ["IMPLEMENT", "API_CONTRACT", "TEST"] → Three flows selected.
APIContractFlow33, ImplementationFlow41, UnitTestingFlow51.
GapHandler NOT invoked.

### Layer 0-C (Knowledge Edge)
Embedded verb rule applied three times:
1. "building the auth service" → IMPLEMENT (build + service noun)
2. "integrating with identity provider" → API_CONTRACT (integrate always fires)
3. "writing the contract tests" → TEST (write + test noun)
Four guidance frames become four gap warnings. Predicate set = [IMPLEMENT, API_CONTRACT, TEST].
"Mentoring throughout" = guidance context wrapper → gap warning only.

### Layer 0-D (Flow Factory)
Three predicates → Three flows.
IMPLEMENT present → TransitionType: feature_addition.
Factory ordering: APIContractFlow33 before ImplementationFlow41 (API before IMPLEMENT rule).

### Layer 1 (Transition Orchestrator)
Orchestration: APIContractFlow33 → ImplementationFlow41 → UnitTestingFlow51
TransitionType: feature_addition
Gap warnings consolidated: coach, walk-through, guide, mentor → single CapabilityGapWarning: gap-skills-guidance-mentor-001
SCHEDULE_CAPABILITY_LEARNING NOT emitted.

### Layer 2 (Flow Execution)
APIContractFlow33: identity provider integration scoped — OAuth2/OIDC (OpenID Connect) endpoints, token validation, session binding. APPROVED.
ImplementationFlow41: auth service built — login, token issuance, refresh, logout flows. APPROVED.
UnitTestingFlow51: contract tests written — token issuance contract, refresh contract, invalid token handling. TEST_PASS.

### Layer 3 (Teaching)
Key insight: "mentoring throughout" as a framing wrapper does NOT suppress any of the three embedded code predicates. Four guidance verbs appear in this request (coach, walk-through, guide, mentor) — all four become gap warnings. The three embedded code actions (build → IMPLEMENT, integrate → API_CONTRACT, write tests → TEST) all fire independently. GapHandler is suppressed because predicate set is non-empty. SCHEDULE_CAPABILITY_LEARNING is not emitted. The mentoring/guidance vocabulary is captured as supplemental warnings only.

**TransitionType**: feature_addition | **Gap warning**: gap-skills-guidance-mentor-001 | **SCHEDULE_CAPABILITY_LEARNING**: NOT emitted
