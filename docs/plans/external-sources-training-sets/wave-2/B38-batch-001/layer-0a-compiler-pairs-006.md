# Layer 0-A — Compiler Pairs (006)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: compiler

---

## B38-0A-006-001 — "pipeline" as code artifact fires IMPLEMENT; review+approve fire QA

Input:
"Review the CI/CD pipeline configuration and approve it for the production release."

Predicate analysis:
- "Review" → QA
- "CI/CD" → context qualifier (Continuous Integration / Continuous Deployment)
- "pipeline" → IMPLEMENT (pipeline as code artifact/configuration fires IMPLEMENT)
- "configuration" → supports IMPLEMENT (code-level artifact)
- "approve" → QA

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 7.1 (QA) + 4.1 (IMPLEMENT)
DomainId: software-development

Reasoning: "pipeline" in CI/CD context refers to a code artifact (the pipeline configuration). This fires IMPLEMENT because pipeline configuration is code that is authored/implemented. "Review" and "approve" fire QA. QA is ordered last in the pipeline.

---

## B38-0A-006-002 — "pipeline" fires IMPLEMENT even without CI/CD qualifier

Input:
"Inspect the deployment pipeline code and validate the stage configuration."

Predicate analysis:
- "Inspect" → QA
- "deployment" → GAP (deployment alone is gap)
- "pipeline" → IMPLEMENT (pipeline as code artifact)
- "code" → supports IMPLEMENT
- "validate" → QA
- "stage configuration" → code artifact context, supports IMPLEMENT

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: ["deployment"]
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 7.1 + 4.1
DomainId: software-development

Reasoning: "deployment" is still gap even though "pipeline" fires IMPLEMENT. The two words do not combine — each is evaluated independently. "Deployment pipeline" does not rescue "deployment" from gap status.

---

## B38-0A-006-003 — "pipeline" in process description (non-code context) — IMPLEMENT fires

Input:
"The release pipeline must be reviewed and approved by the QA lead before launch."

Predicate analysis:
- "pipeline" → IMPLEMENT (pipeline is treated as code artifact in all B38 contexts)
- "reviewed" → QA
- "approved" → QA
- "QA" → QA (explicit QA mention)
- "launch" → GAP

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: ["launch"]
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 7.1 + 4.1
DomainId: software-development

Reasoning: Even in a sentence describing a process ("the release pipeline must be reviewed"), "pipeline" fires IMPLEMENT because the B38 compiler treats pipeline as a code artifact term. This is consistent with rule 2's approach to "service" — domain terms fire their predicate regardless of sentence intent.

---

## B38-0A-006-004 — "pipeline" + "service" both fire IMPLEMENT (one predicate)

Input:
"Approve the user authentication service pipeline after review."

Predicate analysis:
- "Approve" → QA
- "service" → IMPLEMENT
- "pipeline" → IMPLEMENT (multiple IMPLEMENT triggers → single IMPLEMENT predicate)
- "review" → QA

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: []
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 7.1 + 4.1
DomainId: software-development

Reasoning: "service" and "pipeline" both fire IMPLEMENT but consolidate to one IMPLEMENT predicate (no duplication). "Approve" and "review" consolidate to one QA predicate. DecisionCount = 2 (one per unique predicate).

---

## B38-0A-006-005 — "pipeline" in audit sentence: QA+IMPLEMENT confirmed

Input:
"Audit the CI/CD pipeline stages and sign off on the production deployment configuration."

Predicate analysis:
- "Audit" → QA
- "CI/CD" → context
- "pipeline" → IMPLEMENT
- "sign off" → QA
- "deployment" → GAP
- "configuration" → supports IMPLEMENT

Predicates fired: ["QA", "IMPLEMENT"]
CapabilityGapWarnings: ["deployment"]
TransitionType: feature_addition
DecisionCount: 2
Phase routing: 7.1 + 4.1
DomainId: software-development

Reasoning: Consistent with prior pairs in this file. "deployment" remains gap even in a sentence that otherwise fires IMPLEMENT. The gap is isolated to the word "deployment" itself.
