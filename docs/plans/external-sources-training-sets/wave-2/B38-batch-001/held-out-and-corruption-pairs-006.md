# Held-Out and Corruption Pairs (006)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## B38-HO-006-001 — HELD-OUT: "CI/CD pipeline review" → QA+IMPLEMENT

Input: "Review the CI/CD pipeline stages and approve the configuration for production."

Correct result:
- "Review" → QA
- "CI/CD" → context qualifier (Continuous Integration / Continuous Deployment)
- "pipeline" → IMPLEMENT (pipeline as code artifact)
- "stages" → context word (stage definitions part of pipeline; pipeline fires IMPLEMENT already)
- "approve" → QA
- "configuration" → context word (supports IMPLEMENT context; pipeline already fires IMPLEMENT)
- "production" → context word
- PredicateSet: ["QA", "IMPLEMENT"]
- CapabilityGapWarnings: []
- TransitionType: feature_addition
- Phase routing: 4.1 + 7.1

Gate B (held-out assertion): PredicateSet == ["QA","IMPLEMENT"] AND TransitionType == "feature_addition"

---

## B38-HO-006-002 — CORRUPTION: "pipeline" → API_CONTRACT (wrong predicate)

Corrupt input processing:
- "Review" → QA ✓
- "CI/CD" → context ✓
- "pipeline" → API_CONTRACT (WRONG — pipeline is mistakenly classified as API contract vocabulary)
- "approve" → QA ✓

Corrupted result:
- PredicateSet: ["QA", "API_CONTRACT"] (WRONG — should be ["QA","IMPLEMENT"])
- Phase routing: 7.1 + API_CONTRACT phase (WRONG)

Why this is wrong:
"pipeline" in B38 is a code artifact (CI/CD pipeline configuration). It fires IMPLEMENT.
API_CONTRACT is a different predicate for API design/specification vocabulary.
"pipeline" is not an API contract term. It is an IMPLEMENT term.

The confusion may arise from the fact that pipelines sometimes use API-like definitions, but the word "pipeline" itself is classified as an IMPLEMENT artifact in B38.

---

## B38-HO-006-003 — HELD-OUT variant: pipeline + service (double IMPLEMENT triggers)

Input: "Review the auth service CI/CD pipeline and approve the production deployment."
Correct:
- "Review" → QA; "approve" → QA
- "auth service" → IMPLEMENT (service); "pipeline" → IMPLEMENT (pipeline)
- "deployment" → GAP
- PredicateSet: ["QA","IMPLEMENT"] (service + pipeline both fire IMPLEMENT; one predicate)
- CapabilityGapWarnings: ["deployment"]
- TransitionType: feature_addition
