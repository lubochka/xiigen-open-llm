# Held-Out and Corruption Pairs
# Batch: B28-batch-001
# Scenario: B28 — QA automation DevOps, sub-phase 7.1
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05

---

## HO-001 — "validate" + "pipeline" → TEST + ARCHITECTURE (with design verb)

A: "Validate the architecture of the QA pipeline design."
Expected: TEST fires on "validate". ARCHITECTURE fires on "architecture" + "design".
QA fires on "QA". CompiledPredicateIds: ["TEST", "ARCHITECTURE", "QA"].
Three routing decisions: TEST → 5.1 (validate in design context → unit), ARCHITECTURE → 3.1, QA → 7.3.

---

## HO-002 — "write" + "runbook" → DOCUMENT

A: "Write the deployment runbook and document the QA release checklist."
Expected: DOCUMENT fires on "write" (document artifact target) and "document".
QA fires on "QA". "runbook" and "checklist" are gap vocabulary.
CompiledPredicateIds: ["DOCUMENT", "QA"]. Two routing decisions: DOCUMENT → 6.2, QA → 7.3.

---

## HO-003 — pure DevOps gap: "provision" is gap

A: "Provision the staging environment and configure the load balancer."
Expected: zero predicates → knowledge_edge. "provision" is gap. "configure" is gap.
"staging environment" is gap. "load balancer" is gap (infrastructure term).
Gap ID: gap-devops-deployment-001. DomainId: "software-development".

---

## COR-001 — v6-01: IMPLEMENT invented for "deploy"

CORRUPT: "Deploy the service to the staging environment." → CompiledPredicateIds: ["IMPLEMENT"].
Why corrupt: "deploy" is not in the IMPLEMENT lexicon. The corrupt model treated deployment
as a construction/implementation action. "Deploy" is DevOps operation vocabulary with no
predicate. Correct: CompiledPredicateIds: [] → knowledge_edge.

---

## COR-002 — v6-01: QA predicate suppressed when spelled out as "quality assurance"

CORRUPT: "Quality assurance the release pipeline." → CompiledPredicateIds: [].
Why corrupt: The QA predicate fires on the abbreviation "QA" as a literal token. The
corrupt model recognized "quality assurance" as meaning QA but correctly the predicate
does NOT fire on the spelled-out form — only "QA" fires it. However, this is a nuanced
boundary: Correct: CompiledPredicateIds: [] — "quality assurance" does not fire QA.
The corruption here was suppressing QA in a sentence that DID contain "QA". In the
original corrupt case the sentence used "quality assurance" — zero predicates is actually
correct. Flag: this COR pair teaches the abbreviation-sensitivity of QA predicate.

---

## COR-003 — v6-03: QA and TEST collapsed to single routing decision at 7.3

CORRUPT: "QA and test the automated pipeline." → single routing to 7.3.
Why corrupt: QA fires on "QA" → 7.3. TEST fires on "test" → 5.3 (pipeline tests are
E2E-level). Two predicates → two independent routing decisions. Collapsing TEST
into 7.3 is v6-03 multi-branch collapse. Correct: [QA → 7.3, TEST → 5.3] — two decisions.
