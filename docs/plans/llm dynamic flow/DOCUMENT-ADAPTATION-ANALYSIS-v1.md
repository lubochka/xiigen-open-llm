# Document Adaptation Analysis v1
## What each document needs to reflect from the new findings

This analysis covers all ten documents in the review zip. For each document
it states what is still accurate, what is now outdated or incomplete, and
what specific changes are needed to align with the findings from the E51
experiments, the projects 21-23 design, and the sessions that followed.

## Maintenance Status - 2026-05-04

The highest-priority corrections from this analysis have now been applied to
the live dynamic-flow document folder:

- `DECISIONS-LOCKED.md` now includes D-14 and D-15.
- `EXAMINATION-METHOD-FINAL.md` now includes the node-00 logical-structure
  compiler pre-stage, 60-slot feature shape, arbiter-aware KNN registry note,
  output file entry, and projects 21-23 InternalResultExaminer connection.
- `FLOW-01-TRAINING-DATASET-PLAN-v4.md` now warns that the ledger's
  `B expected` entries are hints, not final training outputs, and names the
  E51B registration proof.
- The architecture, canonical sequence, high-level amendment, arbiter-KNN
  amendment, logical-structure examination, external-AI amendment, and the
  older document-examination plan now carry short maintenance notes that connect
  them to E51/E52 and the projects 21-23 implementation slice.

The visual topology follow-up has also been completed: `dynamic-flow-v2-node-topology.csv`
now includes `N00` for the logical structure compiler and `N03B` for the
budget and confidence gate, and `dynamic-flow-high-level-topology-v2.mmd`
shows those nodes in the runtime flow.

---

## 1. DECISIONS-LOCKED.md

**What is still accurate.** D-01 through D-12 are unchanged and correct.
D-13 (tier promotion requires coverage verification) was added correctly
during the E51A sessions and is current.

**What is missing.** Two decisions established through the sessions are
not yet recorded here.

**Changes needed.**

Add D-14:

> D-14 | Internal LLM always first; external AI requires explicit
> single-use approval with a named missing-competence statement;
> all results examined by the internal model before return; all
> examined results stored as training signals; training runs at
> idle time only under IdleDetector gate; a new adapter installs
> only if after-accuracy exceeds before-accuracy.
>
> Rationale: E51B CH-01/CH-02 proved the approval and boundary design.
> Projects 21-23 design session established the five-component
> implementation pattern.

Add D-15:

> D-15 | Priority decides what to attempt. KNN uses arbiter history
> to choose better memory. Arbiters decide what is accepted. No KNN
> confidence, route priority, budget pressure, style preference, or
> external AI advice can override an arbiter rejection. The only
> exception is an explicit user correction.
>
> Rationale: E51B CH-02 demonstrated this in a closed proof.
> The canonical integration sequence locked it as a runtime invariant.

---

## 2. DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md

**What is still accurate.** The seven principles, the cast of unit roles,
the four registries, the feedback paths, and the trust hierarchy are all
still correct as foundational design. Principles 1-7 remain valid.

**What is now outdated or incomplete.**

The cast of unit roles in section 2 does not include the new roles
established by the high-level design amendment: intake gate, system
topology router, sub-subject router, evidence pack selector, result
convergence gate, and budget and confidence gate.

The external AI path in section 6 (synchronous vs asynchronous training)
does not reflect the knowledge edge detector design, the external help
planner, the context pack builder, or the external result adapter. The
plan only addresses training-time external sources, not runtime external
AI calls with approval gating.

The "Internal AI Throughout" principle (Principle 5) is now qualified:
external AI is permitted but only through an explicit approval gate with
a named missing-competence statement. The plan currently says external AI
is "not part of this architecture." That needs to be updated to describe
it as a controlled escape path rather than a prohibition.

Section 2 describes the logical predicate vocabulary implicitly but does
not name it as the shared inter-unit communication medium. The E51A and
E51B experiments established this explicitly.

The style adapter role note about per-user personalization being
architecturally different from shared domain corpora is still missing.

**Changes needed.**

In section 2, add the six roles from the amendment: intake gate, system
topology router, sub-subject router, evidence pack selector, result
convergence gate, budget and confidence gate.

Add one sentence for each new role following the format of existing role
descriptions.

Add a note to the style adapter entry distinguishing it from all other
roles: it trains on per-user preference history, not on a shared domain
corpus, and its storage, update cadence, and rollback semantics are
therefore different.

In Principle 5, replace the blanket prohibition with a qualified rule:
external AI is not in the operational control path. It is accessible
only through a user-approved knowledge edge path when the local system
can name a specific missing competence. The system attempts local units
first in all cases.

Add a new section or subsection naming the logical predicate vocabulary
(the 22-item set validated by E15-E17, extended to 60 feature vector
slots in E51B) as the shared inter-unit communication medium for all
pipeline nodes between the logical structure compiler and the result
translator.

Add a note to section 7 (What Comes After This Document) naming the
canonical integration sequence document as the authoritative runtime
order, and the examination method document as the authoritative training
set preparation method.

---

## 3. DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1.md

**What is still accurate.** Changes 1 through 12 are all correct and
remain valid. The 20-node revised flow list is the right shape. The
training-set consequences section is accurate. The "what not to do"
list is accurate.

**What is now outdated or incomplete.**

The budget and confidence gate (Change 9) does not appear as a named
node in the 20-node CSV. The sessions noted this gap. It needs to appear
explicitly, either as its own node or clearly mapped into an existing
node with a note.

The 20-node flow does not include the logical structure compiler as a
named pre-processing stage. The amendment was written before the logical
structure compiler examination was completed. The flow currently starts
at the intake gate, but the canonical integration sequence places the
logical structure compiler between raw input and the intake gate.

The arbiter return path (after arbiter rejection) sends fragments back
to the local domain splitter. The sessions noted that this does not
cover the case where the wrong area was selected upstream. A return path
to the topology router or sub-subject router is missing.

**Changes needed.**

Add the logical structure compiler as a named pre-stage before the
intake gate, or add a note explaining that the intake gate receives the
output of the logical structure compiler, not raw text.

Either add the budget and confidence gate as node 2a (between sub-subject
router and evidence pack selector) or state explicitly which of the 20
named nodes absorbs its function and how.

Add a note to the arbiter return path section distinguishing between
a translation-level failure (returns to the domain translator array, which
is the current design) and an area-routing failure (returns to the topology
router or sub-subject router). Both are valid return destinations
depending on the failure type.

---

## 4. DYNAMIC-FLOW-CANONICAL-INTEGRATION-SEQUENCE-v1.md

**What is still accurate.** The 20-step canonical sequence is correct
and matches the architectural design. The dataset preparation sequence
is correct. The repeated failure rule and idle trigger rule are correct.

**What is now outdated or incomplete.**

The document does not reference the E51 experiments as evidence for the
design. The E51B trace evidence (particularly CH-01 and CH-02) directly
validates sections 10 and 15 of the sequence. Without that reference,
the document reads as design intent rather than proven behavior.

The document does not reference the projects 21-23 implementation design
as the first concrete application of the sequence in actual code. That
connection needs to be stated so future sessions know where the first
working instance of the pattern lives.

The knowledge edge detector (section 10) lists "failed attempt count" as
an input but does not specify the threshold rule. The session discussion
established a concrete rule: one immediate hard edge for absent competence,
two consecutive failures with the same blocking reason, or three total
failures at the same station. That threshold belongs in the document,
not only in the session record.

**Changes needed.**

Add a "Validated by" reference section at the top or end of the document
citing E51B (6/6 trace success, CH-01 knowledge edge, CH-02 arbiter
authority) as experimental validation of the runtime sequence design.

Add a "First implementation" note citing the projects 21-23 component
design (InternalLlmGateway, InternalResultExaminer, ExternalAiRequestPackage,
ExternalAiResultAdapter, LearningEventScheduler) as the first working
instance of the pattern.

In section 10, add the concrete threshold rule as specified in the
sessions: immediate hard edge when no trusted unit or allowed evidence
exists; two consecutive same-reason failures after one repair attempt;
three total failures at the same station regardless of reason variation.

---

## 5. ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md

**What is still accurate.** The two-level design (arbiter signals guide
KNN memory, arbiter verdicts are hard gates) is correct. The positive,
negative, and warning neighbor classification is correct. The authority
rules are correct.

**What is now outdated or incomplete.**

The document was written before the E51 experiments. The CH-02 trace in
E51B is direct evidence for the design — the duplicate-account instruction
reached the arbiter with a nearest-neighbor similarity of 0.857 and was
still rejected. This needs to be cited as validation.

The document does not address the domain-scoped KNN design from E51A
Phase 2 — the finding that KNN retrieval should be scoped to the
predicted domain pool before neighbor selection. This is a structural
addition to the arbiter-aware KNN memory design.

The document does not describe what happens when the domain classifier
misroutes and KNN searches the wrong pool. The E51A wrong-domain guard
(all neighbors below 0.50 confidence threshold) should be referenced as
the safeguard against this failure mode.

**Changes needed.**

Add a reference to the E51B CH-02 evidence as experimental validation of
the arbiter authority rule: high KNN similarity (0.857) did not override
the identity-consistency arbiter rejection.

Add a section or paragraph on domain-scoped KNN retrieval: the neighbor
pool should be scoped to the predicted domain before similarity ranking,
so that the arbiter history consulted is relevant to the current domain.

Add a note on the wrong-domain retrieval guard: if the top neighbor score
within the scoped pool is below 0.50, the knowledge edge detector should
treat this as a low-confidence signal rather than a confident retrieval.

---

## 6. PROMPT-AND-DATASET-LOGICAL-STRUCTURE-EXAMINATION-v1.md

**What is still accurate.** The main finding (a shared logical structure
compiler must serve both the prompt path and the dataset path) is correct
and has been validated by the E51 experiments. The ten compiler steps are
correct. The meaning graph assembler is correctly identified as the
critical missing step. The recommended first source family (user
registration) was correctly chosen and confirmed by E51B.

**What is now outdated or incomplete.**

The document proposes four new arbiter families (semantic fidelity,
coverage, edge preservation, reverse fidelity). The sessions decided
these should not be four separate families added to the global arbiter
pool — they should be quality checks inside the fidelity and coverage
checker (step 9 of the compiler chain). This needs to be reflected.

The document does not reference the E51B evidence, which ran exactly the
recommended first source family (user registration, Flow-01) and
demonstrated the design working in practice.

The document does not name the 60-slot feature vector as the concrete
output format of the logical structure compiler, which was established
in E51B: 22 predicate count slots, 5 domain one-hot slots, up to 32
normalized root slots, and 1 decomposition count slot.

**Changes needed.**

In the section proposing four new arbiter families, add a note clarifying
that these are internal quality checks within the fidelity and coverage
checker (step 9), not new members of the global arbiter pool. The global
arbiter pool handles domain-level quality. The compiler's own checker
handles structure-level quality. Keeping them separate prevents the
arbiter selector's training problem from growing.

Add a reference to E51B as the first closed-loop validation of the
design using the recommended user registration source family. Note that
the 12 teaching records and 6 test/challenge records used the same
feature-building path for both teaching and runtime, which was the
core claim of the examination.

Add the 60-slot feature vector specification to the section on the
proposed logical structure shape, as the concrete format validated by E51B.

---

## 7. EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT-v1.md

**What is still accurate.** The 8-station path (knowledge edge detector
through idle learning queue), the authority rules, the supplier registry
concept, the knowledge edge signals, the relation to user clarification,
and the topology position rules are all still correct.

**What is now outdated or incomplete.**

The document does not reference the E51B CH-01 evidence, which demonstrated
the knowledge edge detection behavior in a closed proof: the biometric
passport request found neighbors (TR-09 at 0.3162, TR-04 at 0.2158,
TR-03 at 0.1984) but none exceeded the confidence threshold, and the
system correctly created a context pack and learning gap packet rather
than producing an answer.

The document does not describe the five-component implementation design
for projects 21-23, which is the first concrete translation of this
amendment into running code. The ExternalAiRequestPackage structure is
the direct implementation of the context pack described in station 3.

The document uses "external result adapter" and "external help planner"
but does not distinguish the single-use approval gate as a distinct
mechanism between the knowledge edge detector and the help planner. The
projects 21-23 design added this as a PermissionGate extension with
single-use semantics.

**Changes needed.**

Add a reference to E51B CH-01 as experimental validation of the knowledge
edge detection behavior, including the specific neighbor scores that
demonstrated present-but-low-confidence retrieval as the correct trigger
signal (rather than zero-neighbor retrieval).

Add a note between station 1 (knowledge edge detector) and station 2
(external help planner) describing the user approval gate: when the
detector emits a hard edge signal, the system must present the external
help context pack to the user and receive explicit approval before
proceeding. Approval is single-use. It does not grant persistent permission.

Add a "First implementation" reference to the projects 21-23 five-component
design and specifically name ExternalAiRequestPackage as the concrete form
of the context pack described in station 3.

---

## 8. DOCUMENT-EXAMINATION-TO-TRAINING-SETS-PLAN-v1.md

**What is still accurate.** The one-document rule, the phase structure
(seven phases), the non-negotiable rules, the per-node teaching guidance,
and the validation checklist are all still correct.

**What is now outdated or incomplete.**

This document describes a 9-node flow (the original dynamic flow). The
architecture now includes the logical structure compiler as a pre-stage,
the intake gate, system topology router, sub-subject routers, evidence
pack selector, result convergence gate, and budget and confidence gate.
None of these appear in the training file structure.

The document uses the phrase "B expected" in its ledger format but the
examination method (EXAMINATION-METHOD-FINAL.md) has superseded this with
a stronger requirement: B must be the actual output written out in full,
not described or labeled. The ledger format in this plan still allows
descriptive B entries ("planned work pieces for entry points, services,
event chains, storage, errors, and duplicate handling") which the final
method has identified as a failure mode.

The document does not reference the examination method document, which
supersedes and extends it.

**Changes needed.**

Add a note at the top of the document stating that it is superseded by
EXAMINATION-METHOD-FINAL.md for all pair writing guidance, and that the
final method document's good/bad pair examples and node-by-node writing
guidance take precedence.

Update the output file structure to include node files for the logical
structure compiler steps. At minimum, add a `node-00-source-role-gate-set.md`
and a `node-00-logical-structure-compiler-set.md` prefix to the existing
node file list, to reflect that the compiler is a pre-stage that training
sets must also cover.

In the "B expected" column description, add a note that B must be the
actual output written in full — not a label, not a description, not a
category name. Reference the failure modes documented in the examination
method for the reason.

---

## 9. EXAMINATION-METHOD-FINAL.md

**What is still accurate.** This document is the most complete and
accurate of the ten. The failure modes, the A-to-B definitions, the
rephrasing rule, the one-document rule, the node-by-node guidance, the
good and bad pair examples, the source registry, and the validation
checklist are all correct and current.

**What is now outdated or incomplete.**

The document describes a 9-node pipeline throughout. The logical structure
compiler is not named as an additional pre-stage. The node files and the
training pair guidance cover Nodes 1 through 9 but not the compiler chain.

The source registry in Group 1 (proposal and debate archives) focuses on
Nodes 1, 3, 7, and 8. The arbiter-aware KNN design now means that the
proposal archives also have value for KNN memory construction — the
community objections and rejection histories in proposals are exactly the
kind of negative-neighbor and warning-neighbor material that the
arbiter-aware KNN needs. This use case is not named in the registry.

The examination order for external sources (Phase 1, 2, 3) is still
correct but does not mention the projects 21-23 implementation context.
The pairs produced by Phase 1 (proposal archives) will directly feed the
InternalResultExaminer's training data once that component is built,
because the examiner checks against arbiter constraint patterns that
proposal rejection histories contain. This connection is not stated.

**Changes needed.**

Add a section or note describing the logical structure compiler pre-stage
and its training file (tentatively `node-00-compiler-set.md`). State what
A and B look like for the compiler: A is raw text (user prompt, document
line, or session fragment), B is the compiled logical structure (predicates,
normalized roots, meaning graph, confidence). Reference the 60-slot feature
vector as the concrete output format.

In the source registry, add a note to Group 1 (proposal archives) stating
that rejected proposals and community objection threads also supply
negative-neighbor and warning-neighbor material for the arbiter-aware KNN
memory, in addition to their arbiter-unit pair content.

Add a sentence in the examination order section connecting Phase 1 output
to the InternalResultExaminer component in projects 21-23: the arbiter
constraint patterns extracted from proposal rejection histories will become
the examiner's training signal for structured result checking.

---

## 10. FLOW-01-TRAINING-DATASET-PLAN-v4.md

**What is still accurate.** The source contract, version rows, changed
lines record, corrected UML rules, and the full source line ledger (D001
through D357) are all still accurate and should not be changed. The
provenance work is correct. The UML node and edge ledgers are correct.

**What is now outdated or incomplete.**

The plan was written against the original 9-node flow shape. The logical
structure compiler, intake gate, topology router, sub-subject routers,
evidence pack selector, and convergence gate are not represented in the
node file structure.

The B entries in the source line ledger use the old descriptive format
that the examination method has identified as a failure mode. For example,
D001 states "B expected: the subject: user registration and sign-up
onboarding" — this is a label, not the actual subject text that Node 1
should produce.

The validation checklist at the end of the plan does not include the
checks added by the examination method: that no B describes the output
instead of showing it, that the rephrasing rule is satisfied, that Node 8
has at least one pass case.

The plan does not reference E51B, which used this exact document's subject
domain (Flow-01 user registration) as its teaching set and produced the
first closed-loop validation. The E51B evidence (TE-01 through TE-04 and
CH-01, CH-02) is directly derived from the Flow-01 registration material.

**Changes needed.**

Add a note to the plan header stating that the source line ledger
(D001-D357) establishes the source ground truth correctly and must not
be changed, but the per-node training pair files must be written or
rewritten following EXAMINATION-METHOD-FINAL.md rather than the ledger's
"B expected" column entries.

Add the logical structure compiler as a pre-stage in the node file list.
The `node-00-compiler-set.md` file should contain pairs where A is a line
from the registration document (or a user prompt about registration) and B
is its compiled logical structure: predicates, normalized roots, and the
key meaning units for the registration domain.

Update the validation checklist to add the examination method checks:
every node file B must be the actual output, not a description; every pair
appears with at least one rephrasing; Node 8 has at least one pass case;
the simulation names its missing parts.

Add a section at the end of the plan connecting it to the E51B evidence:
note that the 12 teaching records and 6 test/challenge records in E51B
were derived from this document's subject domain, and the 6/6 trace
success rate is the first closed-loop validation that the Flow-01
registration material can teach a dynamic-flow system effectively.

---

## Summary — Priority Order for Applying Changes

These changes are not all equal in urgency. The order below reflects
which documents block downstream work if left outdated.

**Must change before next implementation session:**
DECISIONS-LOCKED.md (D-14 and D-15 are missing locked rules that the
implementation sessions will need to reference).

**Must change before next training set session:**
EXAMINATION-METHOD-FINAL.md (the logical structure compiler node is
missing, and the source registry KNN note is missing).
FLOW-01-TRAINING-DATASET-PLAN-v4.md (the B format problem will
contaminate any pairs written from it before the note is added).

**Should change before next architecture review:**
DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md (Principle 5 prohibition
is now wrong; new roles are missing).
DYNAMIC-FLOW-CANONICAL-INTEGRATION-SEQUENCE-v1.md (threshold rule for
repeated failure is missing; evidence references are missing).

**Can be updated in a dedicated maintenance session:**
DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1.md.
ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md.
PROMPT-AND-DATASET-LOGICAL-STRUCTURE-EXAMINATION-v1.md.
EXTERNAL-AI-KNOWLEDGE-EDGE-AMENDMENT-v1.md.
DOCUMENT-EXAMINATION-TO-TRAINING-SETS-PLAN-v1.md.
