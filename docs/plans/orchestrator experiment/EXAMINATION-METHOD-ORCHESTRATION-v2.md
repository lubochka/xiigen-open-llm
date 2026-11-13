# Document Examination to Training Sets — Orchestration Edition
# Version: 2.0 — 2026-05-05
# Replaces: EXAMINATION-METHOD-FINAL (2).md for orchestration-focused waves
# Focus: Node 1 (domain splitter), Node 2 (domain translator),
#        Node 3 (scheme planner), Node 7 (arbiter selector)
# Code-level behavior is deferred

---

## What this document is

This is the examination method for producing orchestration-focused training pairs.
It replaces the general method for Waves 2 through 4.

The general method remains correct and is not cancelled — it governs code-level
pair production in later waves. This document governs the orchestration-first waves
where the goal is to teach the system to correctly split a user prompt into the
domains it touches and route each domain fragment through the right pipeline.

The failures documented in the general method remain in force. Every failure mode
named there (A was a document line, B described output instead of showing it, no
rephrasing pairs, no pass cases in Node 8, simulation checked the wrong criterion)
can recur in orchestration pairs. This document adds the orchestration-specific
failure modes and their corrections.

---

## Why the previous orchestration pairs were insufficient

The CB4 pairs for Nodes 1, 2, 3, and 7 were written with security-domain prompts
only. The A side was always a prompt about authentication, authorization, or token
validation. A developer asking about something in sub-phase 1.2 (screen layout),
sub-phase 3.1 (architecture topology), or sub-phase 5.2 (integration tests) was
never in the training data.

The result: the domain splitter and domain translator have deep knowledge of one
sub-phase and no knowledge of thirty others. The scheme planner has seen security
work plans but never a feature development plan, a migration plan, or a testing
plan. The arbiter selector has learned which security arbiters fire for which
security signals but has never seen a non-security result set.

The correction is to write pairs across all 31 sub-phases before going deeper on
any single one.

---

## The five orchestration-specific failure modes

**Orchestration Failure 1 — Collapsed multi-domain prompt.**

The A describes a situation that spans two or more sub-phases. The B produces one
territory label instead of the correct array. Example: the user asks about
"adding JWT validation to the login endpoint" — this spans sub-phase 3.4 (the
JWT validation security control) and sub-phase 3.3 (the login endpoint API
contract). A B that produces only one territory has trained the node to ignore
the second domain.

Detection: count the territory labels in B and compare to the number of distinct
sub-phase concerns in A. If A contains N distinct sub-phase concerns and B
contains fewer than N labels, the pair is collapsed.

Correction: each sub-phase concern in A must appear as a separate territory label
in B, with its own sub-phase assignment and its own signal words.

**Orchestration Failure 2 — Wrong sub-phase assignment.**

The A describes something that belongs to sub-phase X. The B assigns it to
sub-phase Y. This is the most consequential failure because all downstream work
is now routed to the wrong place.

The most common wrong assignments:
- Sub-phase 3.4 assigned to 3.3 (security model described as API contract)
- Sub-phase 6.3 assigned to 3.4 (compliance review described as security design)
- Sub-phase 5.2 assigned to 5.1 (integration test described as unit test)
- Sub-phase 4.3 assigned to 4.1 (connector code described as backend service)

Detection: read the B's sub-phase assignments and verify each against the content
of A. A security control (who is allowed to call something) is 3.4. An API
contract (what the call must contain) is 3.3. A test that verifies one unit in
isolation is 5.1. A test that verifies two units talking to each other is 5.2.

Correction: revise the B to assign the correct sub-phase. If the A is genuinely
ambiguous between two sub-phases, revise the A to be more specific, or change
the B to a clarification request.

**Orchestration Failure 3 — Invented territory.**

The B contains a territory label that has no grounding in the A. The A described
something in sub-phase 3.4. The B produced territory labels for sub-phases 3.4,
3.3, and 4.1. The A did not mention API contracts or implementation. The B
invented them based on what usually accompanies security work.

Detection: for each territory label in B, find the sentence or phrase in A that
grounds it. If no grounding sentence exists, the territory is invented.

Correction: remove invented territory labels. The node must learn to split what
is present, not to predict what is usually present. Predicting what is usually
present belongs to Node 3 (the scheme planner), which produces the plan for
handling the identified territories.

**Orchestration Failure 4 — Ambiguity resolved by assumption.**

The A is genuinely ambiguous between two sub-phases. The B picks one without
stating the ambiguity. The node trained on this pair learns to assume when it
should ask.

Detection: read the A and ask whether a developer who typed this might have meant
sub-phase X or sub-phase Y without being able to tell from the text. If yes, the
A is ambiguous and the B should be a clarification request, not a territory array.

Correction: change the B to a clarification request that names the ambiguous
dimension explicitly. "Are you asking about the contract the endpoint exposes
(what it accepts and returns) or about who is allowed to call it?"

**Orchestration Failure 5 — Plan without structure.**

The B of a Node 3 pair lists steps without dependency markers, parallel markers,
wait states, or recovery paths. A list of steps is a summary, not a plan. A node
trained on step lists learns to produce step lists. It does not learn to produce
the structured execution plan the downstream nodes need.

Detection: check whether the B contains at least one of each required element:
an explicit dependency (step B requires step A to complete first), an explicit
parallel marker (step C and step D can run simultaneously), at least one wait
state (the plan pauses here for external input), and at least one named recovery
path (if step E fails because of X, do Y).

Correction: add the structural elements. If the scenario does not naturally
require all four elements, choose a different scenario that does, or expand the
prompt to include the dimension that creates the missing element.

---

## Before writing any pairs — orchestration edition

### Step 1 — Name the source document and its development area

State which sub-phases this source document can provide prompt material for.
A KEP about production-readiness gates provides material for sub-phases 3.5
(infrastructure and deployment topology), 4.1 (backend services), 5.4
(performance and load tests), and 7.3 (CI/CD gate configuration). Name them
all before writing any pairs, because the pair set must cover all of them.

### Step 2 — Confirm the license

Same as the general method. External sources require the per-source license
confirmation before any pairs are written.

### Step 3 — Read the source document for user-facing language

The goal is not to extract technical facts. The goal is to identify the phrases
and situations the source document describes that a developer might use when
asking for help with that kind of work.

For a KEP: read the Motivation section and the User Stories section. These contain
the developer-facing language.

For a Google AIP: read the Guidance section. The imperative sentences ("API
methods should...") describe what developers are trying to achieve.

For a Kubernetes documentation page: read the introductory explanation. The
first paragraphs describe what the feature does from the user's perspective.

Do not extract technical specification language and use it as A. Specification
language is what the system must produce, not what the user types.

### Step 4 — Identify the three scenario types for this source

For each sub-phase the source covers:

S1 scenario — a clean prompt that belongs clearly to this sub-phase. The
developer is asking about exactly the thing this sub-phase governs, and
nothing else.

S2 scenario — a prompt that combines this sub-phase with an adjacent one.
The most natural combination: 3.4 and 3.3 (security model and API contract),
3.1 and 3.2 (architecture and data schema), 5.1 and 5.2 (unit and integration
tests).

S3 scenario — a prompt that could belong to this sub-phase or to a different
one, with no enough signal to decide. The classic ambiguous prompts are those
that describe a symptom ("the 403 error is wrong") without indicating whether
the issue is in the security model (3.4), the API contract (3.3), or the
implementation (4.1).

### Step 5 — Write the A sides first, then the B sides

Write all A sides before any B sides. Reading through a set of A sides reveals
whether they are actually varied or whether they all describe the same situation
in slightly different words.

The A side test: read the A and ask "could this prompt have been typed by an
actual developer?" If the answer is no — if it sounds like a test scenario or
a specification — revise it.

---

## Node 1 pair structure

Every Node 1 pair follows this exact structure.

**A:** A developer prompt written in natural language. Minimum one sentence,
maximum one paragraph. No formal structure. No bullet points. No labeled fields.
The kind of text that appears in a chat interface.

**B (clean routing — S1):** A territory array with one entry per domain. Each
entry contains: the territory label (a descriptive name for the domain), the
sub-phase identifier, the signal words that triggered the assignment, and a
confidence indicator (HIGH, MEDIUM, LOW).

Example S1 B:
```
territories: [
  {
    label: "session-management-security",
    sub_phase: "3.4",
    signal_words: ["session", "timeout", "re-authentication", "inactivity"],
    confidence: HIGH
  }
]
split_type: single_domain
clarification_needed: false
```

**B (multi-domain routing — S2):** A territory array with two or more entries.
Each entry is structured the same way. The ordering of entries reflects the
sequence in which the domains would typically be addressed — not necessarily
the order in which the user mentioned them.

Example S2 B:
```
territories: [
  {
    label: "api-contract-definition",
    sub_phase: "3.3",
    signal_words: ["endpoint", "POST /login", "request body"],
    confidence: HIGH
  },
  {
    label: "security-and-authorization-model",
    sub_phase: "3.4",
    signal_words: ["JWT", "validation", "unauthorized"],
    confidence: HIGH
  }
]
split_type: multi_domain
dependency: "3.4 security controls constrain 3.3 contract definition — design 3.4 first"
clarification_needed: false
```

**B (ambiguous routing — S3):** A clarification request. No territory array.

Example S3 B:
```
clarification_needed: true
ambiguous_dimension: "contract vs authorization"
questions: [
  "When you say the 403 response is wrong — is the endpoint itself incorrectly
   defined (wrong status code in the contract), or is the authorization rule
   that produces the 403 too strict or too permissive?",
  "Are you trying to change what the API returns, or who is allowed to call it?"
]
territories: []
```

---

## Node 2 pair structure

The A side is one territory entry from a Node 1 output, plus the relevant
fragment of the original user prompt.

The B side is the formal domain vocabulary translation of that fragment. The
formal vocabulary depends on the sub-phase.

**Sub-phase 3.4 (security and authorization model) formal vocabulary:**

The B must include: the authentication control type (bearer token, session cookie,
API key, certificate), the authorization model (RBAC, ABAC, policy-based, ACL),
the specific verification claim (which ASVS requirement, which RFC section, which
CWE the claim relates to), and the verification evidence type (design claim,
test result, code review, penetration test finding).

Example B for a 3.4 territory:
```
domain: "security-and-authorization-model"
sub_phase: "3.4"
formal_translation:
  authentication_control: "JWT bearer token — RS256 asymmetric signature"
  authorization_model: "RBAC — role claims in token, checked at endpoint level"
  verification_claim:
    type: "design claim"
    standard: "OWASP ASVS 5.0.0"
    requirement: "V7.3.1 — inactivity timeout enforced per documented risk analysis"
    evidence: "20-minute idle timeout with risk analysis document on file"
  open_questions: []
```

**Sub-phase 3.3 (API contract definition) formal vocabulary:**

The B must include: the endpoint identifier (method and path), the request shape
(required fields, optional fields, validation rules), the response shape (status
codes and their meanings, response body schema for each status), and any
breaking-change indicators (changes that existing callers would need to update
for).

**Sub-phase 3.1 (architecture and component topology) formal vocabulary:**

The B must include: the component name, the component role (what it produces and
what it consumes), the integration pattern it uses (synchronous call, event,
queue, direct database access), the boundary constraints (what it must not do),
and the decision status (proposed, locked, or under review).

**Sub-phase 5.2 (integration tests) formal vocabulary:**

The B must include: the components under test (at least two), the integration
point being tested (which interface between them), the test scenario description
(what state triggers the test, what action is taken, what outcome is verified),
and the isolation method (what is real, what is mocked, what is seeded).

---

## Node 3 pair structure

The A side is the complete set of Node 2 outputs — all the formal territory
translations from one routing decision.

The B side is the ordered execution plan. The plan must contain every required
structural element.

**Required structural elements:**

Steps numbered in execution order. Numbers are not labels — they define order.
Step 2 cannot begin until step 1 is complete, unless an explicit parallel marker
is present.

Parallel markers. When two or more steps can run simultaneously, they share the
same step number with a letter suffix: 2a, 2b, 2c. All steps sharing a number
can run at the same time. The first step after them (step 3) cannot begin until
all of them complete.

Dependency markers. When step N depends on the output of step M but they do not
share a sequential number, the dependency must be stated explicitly: "step 4
requires the authorization model from step 2."

Wait states. A wait state is a point in the plan where execution pauses for
something external: a stakeholder approval, a response from an external system,
a human review, or a calendar event. Wait states must name what is being waited
for and what happens if it does not arrive.

Recovery paths. Each step that can fail must have a named recovery path. The
recovery path names: the failure condition that triggers it, the recovery action,
and where in the plan execution resumes after recovery.

**Example Node 3 B for a two-territory plan (3.4 + 3.3):**

```
plan_type: sequential_with_dependency
total_steps: 5

step_1:
  action: "Design the authorization model for the login endpoint"
  domain: "3.4 security-and-authorization-model"
  produces: "Authorization model decision — RBAC with role claims in JWT"
  recovery: "If authorization model cannot be decided without architecture decision
             → escalate to architecture review (sub-phase 3.1), wait state W1"

step_2:
  action: "Define the login endpoint API contract"
  domain: "3.3 api-contract-definition"
  requires: "Authorization model from step 1 — the 401/403 response codes
             and the Authorization header requirement depend on the auth model"
  produces: "OpenAPI contract for POST /auth/login"
  recovery: "If contract conflicts with existing API versioning constraints
             → flag as breaking change, pause for versioning decision (wait state W2)"

wait_state_W1:
  waiting_for: "Architecture review sign-off on the authorization model"
  timeout: "2 business days"
  on_timeout: "Escalate to tech lead"

wait_state_W2:
  waiting_for: "Versioning decision from API governance"
  timeout: "1 business day"
  on_timeout: "Proceed with non-breaking alternative and document the limitation"

step_3:
  action: "Write acceptance criteria for both the security control and the contract"
  domain: "both"
  requires: "Steps 1 and 2 complete"
  produces: "Acceptance criteria document"
  recovery: "If acceptance criteria conflict between domains → resolve in review step"

step_4a:
  action: "Implement the JWT validation logic"
  domain: "4.1 backend-services"
  produces: "Implementation of authentication middleware"

step_4b:
  action: "Implement the contract validation layer"
  domain: "4.1 backend-services"
  produces: "Request schema validation middleware"

step_5:
  action: "Write integration tests for the authenticated endpoint"
  domain: "5.2 integration-tests"
  requires: "Steps 4a and 4b both complete"
  produces: "Integration test suite for POST /auth/login"
```

---

## Node 7 pair structure

The A side is the set of formal results from Nodes 4 and 5 — what the domain
processors and appliers produced.

The B side is the arbiter selection decision: which review units fire, why each
was selected, and in what order they fire.

The B must be explicit about the selection trigger. "Security arbiter fires" is
not explicit. "ASVS-V7-Session-Arbiter fires because the formal result from the
3.4 territory contains a session timeout claim and the territory flag confirms
ASVS Level 2 is required" is explicit.

The B must also state which arbiters were considered and not selected, and why:
"API-Contract-Consistency-Arbiter was considered but not selected because no
existing API endpoints are being modified — this is a new endpoint."

---

## The rephrasing rule — orchestration edition

Every pair must appear at least twice: once from the developer's perspective and
once from the system-owner's perspective.

Developer perspective: "I need to add JWT validation to the login endpoint."

System-owner perspective: "We need to ensure the login endpoint enforces token-
based authentication so we can control access per user role."

These describe the same work. They arrive at Node 1 in different words. The node
must recognize both as belonging to the same territory array. Both phrasings must
appear in the training set.

Do not rephrase by shortening. A rephrasing that strips words without changing
perspective is not a rephrasing — it is a summary. The detail level must be
preserved.

---

## Acceptance criteria checklist for orchestration pairs

Before any pair is promoted to the training set, check each item:

**Node 1 pair checklist:**
- [ ] A is written in natural developer language, not specification language
- [ ] B contains the actual territory array or the actual clarification request
- [ ] Each territory in B has a sub-phase assignment that is correct
- [ ] Each territory in B has signal words drawn from the A text
- [ ] No territory in B is invented — every label has grounding in A
- [ ] If A is ambiguous, B is a clarification request, not a guess
- [ ] A rephrasing pair exists at the same detail level

**Node 2 pair checklist:**
- [ ] A is one territory plus the relevant fragment from the original prompt
- [ ] B is the formal vocabulary translation, not a description of it
- [ ] B uses the correct formal vocabulary for the sub-phase
- [ ] Open questions in B are genuine — they reflect something the A did not specify
- [ ] A rephrasing pair exists

**Node 3 pair checklist:**
- [ ] A is the complete set of Node 2 formal translations
- [ ] B contains numbered steps in execution order
- [ ] B contains at least one explicit dependency marker
- [ ] B contains parallel markers where steps can run simultaneously
- [ ] B contains at least one wait state with its timeout and timeout action
- [ ] B contains a recovery path for each step that can fail
- [ ] A rephrasing pair exists

**Node 7 pair checklist:**
- [ ] A is the formal result set from Nodes 4 and 5
- [ ] B names each selected arbiter unit with its specific trigger
- [ ] B names considered-but-not-selected arbiters with the reason each was excluded
- [ ] A rephrasing pair exists

---

## Source-specific notes for orchestration pairs

### XIIGen session files (first-party material)

These are the highest-priority source for Node 1 A sides because they are real
prompts from the actual development work. The session files contain exactly the
language Luba uses when asking for development help.

Extraction method: read a session file and identify every message from Luba that
describes a development need or poses a development question. Each such message
is a candidate A side. Copy the message verbatim. Do not paraphrase or clean up.

Then assign it to its sub-phase using the two-question method. Then write the
B side — the territory array the node should have produced for that prompt.

The rephrasing pair is the same prompt written from the system specification
perspective: what would a requirements document say about the same need?

### Kubernetes Enhancement Proposals

The Motivation and User Stories sections of a KEP provide developer-facing
language. The rest of the KEP provides technical specification language.

Use the Motivation and User Stories sections for A sides. Use the technical
sections to verify the sub-phase assignment and to write the formal vocabulary
translation (B side of Node 2 pairs).

One KEP commonly spans multiple sub-phases. A KEP about pod disruption budgets
covers sub-phases 3.5 (infrastructure topology), 5.4 (performance testing), and
7.3 (CI/CD gate configuration). Write pairs for all three sub-phases from the
same KEP, not just the most obvious one.

### Google AIP documents

Each AIP guidance section contains imperative sentences about what APIs must do.
Convert imperative sentences into developer questions:

"APIs must return resource representations" → "What should my API return when
someone requests a resource?"

"Long-running operations should use the LRO pattern" → "I have an operation that
takes several minutes — how should I design the API for it?"

These conversions are the A sides. The B side assigns the correct sub-phase
(sub-phase 3.3 for API contract questions, sub-phase 3.1 for architecture pattern
questions).

### Stack Overflow (CC-BY-SA 4.0, attribution required)

Stack Overflow questions are the most realistic source of developer prompt
language — they are questions developers actually typed when stuck. They carry
CC-BY-SA 4.0 license. Every pair derived from a Stack Overflow question must
carry the question URL and license in its source field.

The question title is the A side. The question body provides context that helps
verify the sub-phase assignment. The accepted answer is not used in pairs — it
belongs to the B side of Node 8 or Node 9 pairs in later waves, not Node 1 pairs.

Search for questions tagged with the relevant technology and filtered to questions
that describe a development situation rather than asking for a code fix. The tag
combination [oauth2]+[jwt]+[authorization] produces 3.4 territory questions.
The tag combination [rest-api]+[openapi]+[schema] produces 3.3 territory questions.

---

## Anti-patterns specific to orchestration pairs

**Anti-pattern O1 — The universal domain splitter.**

The A describes something specific. The B produces territory labels for all
related sub-phases, not just the ones the A actually mentions. The node learns
to always produce a maximal territory array rather than the minimal correct array.

Example of O1: A says "I need to validate that JWT tokens expire correctly." B
produces territories for 3.4 (correct), 3.3 (not mentioned), 4.1 (not mentioned),
5.1 (not mentioned). The A mentioned one thing. The B assigned four territories.

**Anti-pattern O2 — The specification-language A.**

The A reads like a requirement rather than a developer prompt. "The system must
implement JWT token expiry validation per ASVS V7.3.1" is a requirement. "The
tokens aren't expiring when they should — I think the exp claim validation is
wrong" is a developer prompt. Both describe the same problem. Only the second is
a correct Node 1 A side.

**Anti-pattern O3 — The dependency-free plan.**

The Node 3 B lists steps without stating which steps depend on which. The list
looks complete because all the steps are present. But the node trained on this
list learns to produce lists, not plans. A plan has structure. A list does not.

**Anti-pattern O4 — The single-phase Node 3 pair.**

Every Node 3 B covers only one sub-phase. The node never learns to plan across
sub-phase boundaries. When a real multi-sub-phase prompt arrives, the plan stops
at the boundary of the trained sub-phase and the downstream work is left undone.

The correction: at least one third of Node 3 pairs must span two or more sub-phases.
The multi-sub-phase pairs are the most important ones for orchestration correctness.

---

## Coverage target summary

| Node | Pairs per sub-phase (minimum) | Total minimum across 31 sub-phases |
|---|---|---|
| Node 1 | 20 (5 clean + 5 split + 5 cross-phase + 5 ambiguous) | 620 |
| Node 2 | 15 (5 per territory type, 3 territory types minimum per sub-phase) | 465 |
| Node 3 | 10 (5 single-sub-phase + 5 multi-sub-phase) | 310 |
| Node 7 | 6 (2 per arbiter selection pattern) | 186 |
| Total minimum | | 1,581 |

These are minimums. The hard-to-distinguish adjacent sub-phase pairs (3.3 vs 3.4,
5.1 vs 5.2, 4.1 vs 4.3) require twice the minimum pair count to achieve 17/20
routing accuracy.

---

## Version note

This document governs Waves 2, 3, and 4. Wave 5 (cross-phase context carry-
forward) and Wave 6 (code-level pair production) will have their own method
documents produced when those waves begin. Code-level pairs are not produced
under this document.
