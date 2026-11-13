# Development Phases, Sub-Phases, and Document Identification Guide
# How to map the development lifecycle to orchestration experiments
# Version: 1.0 — 2026-05-04

---

## How to Read This Document

This document has two parts.

Part 1 is the complete phase and sub-phase list for the development lifecycle.
Each phase has a purpose statement, a list of sub-phases, and a list of the
question each sub-phase must answer before it can be considered complete.

Part 2 is the document identification method — the rules for deciding whether
a given document belongs to a phase, and the signals that make the assignment
unambiguous.

---

## Part 1 — The Phase and Sub-Phase List

---

### Phase 1 — Design

**What this phase answers:** What does the product look like, how does the
user interact with it, and what constraints does the visual and interaction
layer place on everything that follows?

Design is upstream of everything. Its decisions create constraints that
functional specs, technical design, and implementation must honor. A design
decision that is made without awareness of technical constraints produces
rework. A technical decision that is made without awareness of design
constraints produces misaligned implementation. The phase transition gate
between Design and Functional Specification is the first critical checkpoint
in the lifecycle.

---

#### Sub-phase 1.1 — Design System and Tokens

**Question it answers:** What are the non-negotiable visual primitives that
every screen must use?

This sub-phase produces the design system contract: color tokens, typography
scale, spacing scale, elevation levels, border radii, motion duration values,
and component primitives. Every downstream design artifact inherits from this
contract. A screen that uses a color not in the token set is a violation. A
component that uses a spacing value not in the scale is a violation.

**What makes a document belong here:** The document defines or refers to
named token values, defines a named design system, or establishes which visual
decisions are non-negotiable across all screens.

**Primary signals in the document:** token names, color palette definitions,
typography scale, spacing system, component primitive names.

---

#### Sub-phase 1.2 — Screen and Layout Structure

**Question it answers:** What screens exist, what is the purpose of each,
and what is the structural relationship between them?

This sub-phase produces the screen inventory and the layout intent for each
screen. It names the navigation model (persistent sidebar, tab bar, modal
flow, wizard), the content hierarchy on each screen, and the responsive
behavior at different breakpoints. It does not define pixel-level detail —
it defines structural intent.

**What makes a document belong here:** The document enumerates screens by
name and purpose, defines the navigation structure between screens, or
specifies the layout intent (not the pixel layout) for a screen or screen
family.

**Primary signals:** screen names and purposes, navigation model names,
layout structure descriptions, breakpoint behavior.

---

#### Sub-phase 1.3 — Interaction Model and State Transitions

**Question it answers:** How does the UI behave when the user acts on it,
and what states does each component move through?

This sub-phase produces the interaction model: which elements respond to
which user actions, how transitions between states are triggered, what the
loading and error states look like, and how the user recovers from errors.
It includes state diagrams for complex interactions (multi-step forms,
expandable content, drag and drop).

**What makes a document belong here:** The document describes user action
triggers and their consequences, defines component states and transitions,
or specifies how the UI communicates loading, error, or empty states to
the user.

**Primary signals:** state names, transition triggers, loading and error
state descriptions, gesture and interaction vocabulary.

---

#### Sub-phase 1.4 — Accessibility and Compliance

**Question it answers:** What accessibility standard applies to this product,
and what does compliance require from each screen and component?

This sub-phase produces the accessibility requirement set: the target WCAG
level, the specific success criteria that apply to this product's interaction
patterns, the keyboard navigation requirements, the screen reader behavior
requirements, and the color contrast minimums. It also covers any regulatory
compliance requirements (GDPR data display, financial disclosure formatting,
age verification).

**What makes a document belong here:** The document names an accessibility
standard (WCAG, Section 508, EN 301 549), defines compliance requirements
for specific component types, or specifies regulatory display or interaction
requirements.

**Primary signals:** WCAG level references, accessibility success criteria,
keyboard navigation requirements, ARIA pattern specifications, regulatory
requirement names.

---

#### Sub-phase 1.5 — Motion and Animation

**Question it answers:** What motion behavior does the product use, and
what rules govern when and how things move?

This sub-phase produces the motion specification: which interactions use
animation, what the duration and easing values are for each category of
motion (micro-interaction, page transition, loading, feedback), and which
interactions must not use motion (for users with motion sensitivity). It
connects to the design token system from sub-phase 1.1.

**What makes a document belong here:** The document specifies animation
durations or easing curves by name, defines when motion is appropriate
versus inappropriate, or establishes motion as a named design decision.

**Primary signals:** animation duration values, easing function names,
motion category names (micro-interaction, transition, etc.), reduced
motion requirements.

---

### Phase 2 — Functional Specification

**What this phase answers:** What does the product do, who does it for,
under what rules, and what are the observable outcomes?

Functional specification is the contract between design intent and technical
implementation. It receives design decisions as constraints and produces a
specification that technical design can implement. It does not specify how
anything is built — only what is built and under what conditions.

---

#### Sub-phase 2.1 — User Stories and Acceptance Criteria

**Question it answers:** What does each type of user need to accomplish, and
what observable conditions confirm that need is met?

This sub-phase produces the user story set with acceptance criteria. Each
story names the user type, the goal, and the motivation. Each acceptance
criterion is a testable condition — not a description of implementation but
a statement of observable outcome.

**What makes a document belong here:** The document is organized by user
type and goal, expresses requirements as observable outcomes rather than
implementation instructions, or contains acceptance criteria in the "given
/ when / then" or equivalent pattern.

**Primary signals:** user type names, goal statements, acceptance criteria,
given/when/then patterns, scenario names.

---

#### Sub-phase 2.2 — Business Rules

**Question it answers:** What constraints, policies, and calculations govern
the product's behavior independent of any implementation choice?

This sub-phase produces the business rule catalog. Each rule is a named,
versioned statement about what the system must do or must not do under
specific conditions. Rules come from the domain — regulatory requirements,
business policies, contractual obligations — not from technical preference.

**What makes a document belong here:** The document states rules in
domain vocabulary rather than technical vocabulary, names the authority
source for each rule (regulatory body, business policy, contractual term),
or catalogs named business policies with their conditions and consequences.

**Primary signals:** rule names, policy names, domain vocabulary, authority
source references, condition-consequence structures.

---

#### Sub-phase 2.3 — Integration Point Specification

**Question it answers:** What external systems does the product interact
with, at what level of abstraction, and what does the product need from
each?

This sub-phase produces the integration requirement set — not the integration
design. It names which external systems exist, what the product needs from
them (not how to get it), and what constraints the product places on those
interactions (latency tolerance, data format requirements, fallback behavior
when the external system is unavailable).

**What makes a document belong here:** The document names external systems
by their business purpose, specifies what the product needs from each
without specifying the implementation mechanism, or defines the business-level
constraints on external interactions.

**Primary signals:** external system names by business purpose, need
statements (not implementation statements), constraint statements about
external interactions.

---

#### Sub-phase 2.4 — Data Model Boundaries

**Question it answers:** What are the core entities the product works with,
what do they mean in business terms, and what are the fundamental
relationships between them?

This sub-phase produces the business data model — not the technical schema.
It names the entities (User, Order, Product, Subscription), their business
meaning, their key attributes at the conceptual level, and the relationships
between them (an Order belongs to one User, an Order contains one or more
Products). It does not specify column types, indexes, or storage format.

**What makes a document belong here:** The document defines entities by their
business meaning rather than their technical storage shape, names relationships
in business terms (belongs to, contains, triggers, depends on), or establishes
the conceptual data model independently of implementation.

**Primary signals:** entity names with business definitions, relationship
vocabulary in business terms, conceptual attribute names without technical
type specifications.

---

### Phase 3 — Technical Design

**What this phase answers:** How will the product be built, what are the
technical decisions that follow from the functional requirements, and what
constraints does the technical layer place on implementation?

Technical design receives functional specifications as requirements and
produces the technical contract that implementation must follow. It specifies
architecture, data schema, API contracts, security model, and infrastructure
— all at a level that is independent of any specific implementation session.

---

#### Sub-phase 3.1 — Architecture and Component Topology

**Question it answers:** What are the components of the system, how do they
relate to each other, and what are the non-negotiable architectural rules?

This sub-phase produces the architecture decision set and the component
topology. It names the services, their responsibilities, their boundaries,
and the rules that govern how they interact. It specifies what is
non-negotiable (no direct database access from the presentation layer,
all external dependencies through interfaces) and what is configurable
(caching strategy, retry policy).

**What makes a document belong here:** The document names components and
their responsibilities, establishes architectural rules in terms of what is
allowed and what is forbidden, or records an architecture decision with its
rationale and the alternatives that were rejected.

**Primary signals:** component names and responsibility statements,
architectural rule statements (must, must not, always, never), ADR (Architecture
Decision Record) structure, technology choice rationale.

---

#### Sub-phase 3.2 — Data Schema and Migration Design

**Question it answers:** How are the business entities stored, how do existing
schemas evolve to support new requirements, and what are the safety constraints
on that evolution?

This sub-phase produces the technical data schema and the migration plan.
The schema specifies table structures, column types, indexes, constraints,
and foreign key relationships. The migration plan specifies the sequence of
schema changes, the safety constraints (zero downtime, no table locks on
large tables), and the rollback strategy for each change.

**What makes a document belong here:** The document specifies database column
types and constraints, defines a migration sequence with named phases, or
establishes safety constraints for schema evolution (maximum lock duration,
batch size requirements, rollback conditions).

**Primary signals:** column type specifications, migration phase names, lock
and downtime constraints, expand-contract pattern references, backfill
requirements.

---

#### Sub-phase 3.3 — API Contract Definition

**Question it answers:** What is the exact interface between components, what
does each endpoint receive and return, and what are the contract obligations
on both sides?

This sub-phase produces the API contract — the precise specification of each
endpoint or message interface including input shape, output shape, error
codes, versioning rules, and breaking change policy. It is more specific than
the integration point specification from Phase 2 and more stable than
implementation code.

**What makes a document belong here:** The document specifies input and output
shapes for specific endpoints, defines error codes and their meanings, or
establishes versioning and breaking change policy for an interface.

**Primary signals:** endpoint names, request and response shape specifications,
HTTP status codes or equivalent, versioning scheme, breaking change definitions.

---

#### Sub-phase 3.4 — Security and Authorization Model

**Question it answers:** Who is allowed to do what, how is that enforced,
and what are the non-negotiable security rules?

This sub-phase produces the authorization model — the mapping of user roles
to permitted actions, the enforcement points, and the non-negotiable security
rules. It specifies what requires authentication, what requires specific roles,
what is publicly accessible, and what monitoring and audit requirements apply.

**What makes a document belong here:** The document defines user roles and
their permitted actions, specifies authentication requirements for specific
operations, or establishes security rules that apply across all components
(all database writes must include tenant identifier, all external calls must
be logged).

**Primary signals:** role names, permission statements, authentication
requirement specifications, audit and logging requirements, tenant isolation
rules.

---

#### Sub-phase 3.5 — Infrastructure and Deployment Topology

**Question it answers:** Where does the system run, how is it deployed, and
what does the deployment topology look like?

This sub-phase produces the infrastructure design — the deployment units,
their scaling behavior, their networking requirements, their resource
constraints, and the deployment sequence. It connects to the security model
(network isolation, secret management) and to the CI/CD pipeline design.

**What makes a document belong here:** The document names deployment units
and their resource requirements, specifies networking and isolation rules,
or defines the deployment sequence with its gates and rollback conditions.

**Primary signals:** deployment unit names, resource specification (memory,
CPU, storage), network topology, scaling rules, deployment gate conditions.

---

### Phase 4 — Implementation

**What this phase answers:** What was built, does it match the technical
design, and what deferred items or known issues remain?

Implementation receives technical design as its contract. It does not make
design decisions — it executes them. When implementation encounters something
the technical design did not specify, it surfaces it as a gap rather than
making an undocumented decision.

---

#### Sub-phase 4.1 — Backend Services

**Question it answers:** Are the server-side components built according to
the architecture, following the API contracts, with the required security
enforcement?

**What makes a document belong here:** The document is an implementation plan,
session plan, or phase plan for server-side code. It names specific files,
classes, or methods to create. It references the technical design decisions
as constraints rather than discovering them.

**Primary signals:** class names and their parent classes, method signatures,
interface names to implement, gate commands that verify the implementation
follows the design (grep commands, test assertions).

---

#### Sub-phase 4.2 — Frontend and Client Rendering

**Question it answers:** Are the client-side components built according to
the design system, using the interaction model, and connected to the backend
through the specified API contracts?

**What makes a document belong here:** The document is an implementation plan
for client-side code. It references design tokens by name, specifies component
hierarchy, and names the API endpoints the client will call.

**Primary signals:** component names, design token references, API endpoint
references, state management descriptions, responsive breakpoint specifications.

---

#### Sub-phase 4.3 — Integration and Provider Connectors

**Question it answers:** Are the external system integrations implemented
according to the integration point specifications, with the required
abstraction boundaries?

**What makes a document belong here:** The document describes implementing
a connection to an external system, references an interface the connector
must implement, or specifies how the integration is tested in isolation.

**Primary signals:** interface names to implement, external system names,
abstraction boundary descriptions, mock or stub requirements.

---

#### Sub-phase 4.4 — Data Migration Execution

**Question it answers:** Was the migration executed according to the migration
plan, did it complete without downtime, and is the data in the expected state?

**What makes a document belong here:** The document describes a specific
migration execution — the SQL or migration framework commands, the batch
parameters, the verification steps, and the rollback instructions if the
migration fails.

**Primary signals:** migration step names, SQL or migration framework syntax,
verification command descriptions, rollback conditions, batch size and pause
specifications.

---

#### Sub-phase 4.5 — Documentation

**Question it answers:** Is the system's behavior, configuration, and
operation documented well enough that someone new to the system can understand
it?

**What makes a document belong here:** The document describes what the system
does and how to operate it, rather than how to build it. It may include API
references, configuration guides, runbooks, and architecture summaries.

**Primary signals:** audience is operator or consumer rather than implementor,
describes how to use or operate rather than how to build, references completed
implementation artifacts.

---

### Phase 5 — Testing

**What this phase answers:** Does the implementation match the functional
specification and technical design, and is that match verifiable and
repeatable?

Testing is not a final gate — it is the proof mechanism. Its output is a
coverage map and a verdict set that QA and downstream phases can rely on.

---

#### Sub-phase 5.1 — Unit and Contract Tests

**Question it answers:** Does each component in isolation behave according
to its contract?

**What makes a document belong here:** The document specifies what to test
at the component boundary, what constitutes a passing contract test, or
defines the input-output pairs that a component must handle correctly.

**Primary signals:** component names under test, input-output pair
specifications, contract assertions (this method always returns
OperationResult, this interface always produces a specific shape).

---

#### Sub-phase 5.2 — Integration Tests

**Question it answers:** Do components work correctly together at the
boundaries defined by the API contracts?

**What makes a document belong here:** The document specifies tests that
cross component boundaries, require a real or realistic database connection,
or test a sequence of interactions across more than one service.

**Primary signals:** multi-component test scenario names, real dependency
requirements (actual database, actual message queue), sequence of steps
that cross service boundaries.

---

#### Sub-phase 5.3 — End-to-End Scenario Tests

**Question it answers:** Does the system produce the outcomes specified in
the acceptance criteria?

**What makes a document belong here:** The document specifies test scenarios
that start from a user action and verify an observable outcome, trace through
the full system stack, or directly correspond to acceptance criteria from
the functional specification.

**Primary signals:** user action starting points, acceptance criteria
references, observable outcome assertions (the user sees, the system
sends, the database contains).

---

#### Sub-phase 5.4 — Performance and Load Tests

**Question it answers:** Does the system meet the performance requirements
under expected and peak load?

**What makes a document belong here:** The document specifies load profiles
(request rate, concurrency, duration), defines pass conditions in measurable
terms (p95 response time under X milliseconds, zero errors under Y concurrent
users), or establishes performance baselines.

**Primary signals:** load profile specifications, p95/p99 response time
targets, concurrency requirements, error rate thresholds.

---

### Phase 6 — QA

**What this phase answers:** Does the system meet all functional, security,
accessibility, and compliance requirements, and is there evidence for each?

QA is structured review against the functional specification and the
acceptance criteria. It does not discover new requirements — it verifies
that the stated requirements are met.

---

#### Sub-phase 6.1 — Functional Review

**Question it answers:** Does each acceptance criterion from the functional
specification have passing test evidence?

**What makes a document belong here:** The document maps acceptance criteria
to test results, identifies which criteria have no test coverage, or produces
a verdict (pass, fail, not tested) for each acceptance criterion.

**Primary signals:** acceptance criteria references, pass/fail verdicts,
test evidence references, coverage gap statements.

---

#### Sub-phase 6.2 — Regression Coverage Verification

**Question it answers:** Does the test suite protect against regressions in
all previously working behavior?

**What makes a document belong here:** The document specifies what regression
test coverage means for this system, identifies gaps in regression protection,
or verifies that a change did not break previously passing behavior.

**Primary signals:** regression scope definitions, before/after behavior
comparisons, coverage gap descriptions.

---

#### Sub-phase 6.3 — Security and Compliance Review

**Question it answers:** Does the system enforce the authorization model,
protect sensitive data, and meet regulatory requirements?

**What makes a document belong here:** The document reviews implementation
against the security model from technical design, tests authorization
boundaries (can a user of role X access resource Y), or verifies regulatory
compliance (data retention, right to erasure, audit logging).

**Primary signals:** role and permission test scenarios, sensitive data
handling verification, regulatory requirement references with evidence.

---

#### Sub-phase 6.4 — Accessibility Audit

**Question it answers:** Does each screen and component meet the accessibility
requirements from the design phase?

**What makes a document belong here:** The document tests accessibility
requirements against the implemented UI, names specific WCAG success criteria
and whether they pass, or documents remediation needed for failing criteria.

**Primary signals:** WCAG success criteria references with pass/fail verdicts,
screen reader test descriptions, keyboard navigation test results.

---

### Phase 7 — QA Automation

**What this phase answers:** Are the QA verification steps automated,
repeatable, and integrated into the delivery pipeline?

QA automation converts the manual verification steps from Phase 6 into
automated checks that run on every change. It does not produce new
requirements or new test scenarios — it automates the ones already validated
in Phase 6.

---

#### Sub-phase 7.1 — Test Scenario Automation

**Question it answers:** Are the end-to-end and integration scenarios from
Phase 5 and Phase 6 automated?

**What makes a document belong here:** The document specifies how to convert
a manual test scenario into an automated one, defines the automation framework
being used, or establishes which scenarios must be automated before promotion.

**Primary signals:** automation framework names, scenario-to-automation
mapping, promotion gate references.

---

#### Sub-phase 7.2 — Regression Suite Maintenance

**Question it answers:** How does the regression suite stay current as the
system changes?

**What makes a document belong here:** The document specifies the protocol
for adding new regression tests when new behavior is added, defines when a
test should be retired, or establishes the ownership model for the test suite.

**Primary signals:** addition protocol descriptions, retirement criteria,
ownership and maintenance responsibilities.

---

#### Sub-phase 7.3 — CI/CD Gate Configuration

**Question it answers:** What automated checks must pass before code can
be merged, deployed, or promoted?

**What makes a document belong here:** The document defines the gates in the
delivery pipeline, specifies what each gate checks, or establishes the
criteria for a passing build.

**Primary signals:** gate names, what each gate checks, pass/fail criteria,
pipeline stage names.

---

#### Sub-phase 7.4 — Coverage and Reporting

**Question it answers:** How is test coverage measured and communicated,
and what are the minimum acceptable coverage levels?

**What makes a document belong here:** The document defines coverage metrics,
establishes minimum thresholds for each metric, or specifies the reporting
format and audience for test results.

**Primary signals:** coverage metric names (line coverage, branch coverage,
scenario coverage), threshold values, reporting audience and format.

---

## Part 2 — Document Identification Method

---

### The Two Questions

Every document classification starts with two questions asked in sequence.

**Question 1 — What problem does this document resolve?**

Read the document's opening section, its purpose statement, or its first
substantive paragraph. Ignore the title. Titles are often misleading. The
content reveals the problem being resolved.

If the document resolves a problem about how something looks or behaves for
the user, it belongs to Phase 1 or Phase 2 depending on specificity.

If the document resolves a problem about what the system must do under what
rules, it belongs to Phase 2.

If the document resolves a problem about how the system is structured or
built, it belongs to Phase 3 or Phase 4 depending on whether it specifies
structure or executes it.

If the document resolves a problem about whether the system works correctly,
it belongs to Phase 5 or Phase 6.

If the document resolves a problem about making verification repeatable and
automatic, it belongs to Phase 7.

**Question 2 — What vocabulary does the document use?**

Look for the terms the document uses most frequently and most precisely. The
vocabulary reveals the sub-phase more reliably than any other signal.

Design system vocabulary (tokens, primitives, breakpoints) → sub-phase 1.1
Screen and navigation vocabulary (screen names, layout, hierarchy) → sub-phase 1.2
Interaction and state vocabulary (triggers, states, transitions) → sub-phase 1.3
Accessibility vocabulary (WCAG, ARIA, keyboard navigation) → sub-phase 1.4
Motion vocabulary (duration, easing, animation categories) → sub-phase 1.5

User and goal vocabulary (user type, goal, acceptance criterion) → sub-phase 2.1
Rule and policy vocabulary (must, must not, when X then Y, policy name) → sub-phase 2.2
Integration vocabulary (external system by business name, what is needed) → sub-phase 2.3
Entity and relationship vocabulary (entity name, belongs to, contains) → sub-phase 2.4

Architecture vocabulary (component names, architectural rules) → sub-phase 3.1
Schema vocabulary (column types, constraints, migration phases) → sub-phase 3.2
API vocabulary (endpoint names, request/response shapes, error codes) → sub-phase 3.3
Security vocabulary (roles, permissions, enforcement points, tenant) → sub-phase 3.4
Infrastructure vocabulary (deployment units, scaling, network) → sub-phase 3.5

Implementation vocabulary (class names, methods, gate commands, test counts) → Phase 4
Test scenario vocabulary (input, expected output, assertion) → Phase 5
Review and verdict vocabulary (pass, fail, not tested, coverage) → Phase 6
Automation and pipeline vocabulary (CI gate, suite, threshold) → Phase 7

---

### The Four Clarifying Rules

**Rule 1 — Specificity determines depth.**

A document that describes what users need in general belongs to Phase 2.
A document that describes what the system must produce in specific measurable
terms belongs to Phase 2 or Phase 3 depending on whether it uses technical
or business vocabulary. A document that describes exactly which code to write
belongs to Phase 4.

When the vocabulary is ambiguous, ask: could this document be written without
knowing which technology is used? If yes, it is Phase 1, 2, or 3. If no,
it is Phase 4 or later.

**Rule 2 — One document, one primary phase.**

Every document belongs primarily to one phase. It may contain content that
informs other phases as background — a technical design document may
summarize the functional requirements it addresses — but the primary phase
is the one the document was produced to advance.

Assign the primary phase based on the problem the document was written to
resolve, not based on the content it contains as supporting context.

**Rule 3 — Implementation plans belong to Phase 4, not to the phase they
reference.**

A Phase 4 session plan that begins by listing the technical design decisions
it is implementing belongs to Phase 4. It is an implementation artifact. The
technical design decisions it references belong to Phase 3. Do not assign
the session plan to Phase 3 because it cites Phase 3 documents.

**Rule 4 — The absence of a document is itself a signal.**

If a document for a specific sub-phase does not exist, that is a knowledge
edge. The orchestration for that sub-phase cannot be complete without source
authority. Naming the missing document and marking the sub-phase as boundary
is the correct behavior — the same pattern as the biometric passport
verification knowledge edge in E53.

---

### The Experiment Readiness Check

A document is ready to be the source for an experiment when it satisfies
three conditions:

**Condition 1 — It has a primary phase assignment.** You can state which
phase and sub-phase the document belongs to without ambiguity. If the
assignment is ambiguous, resolve it using the two questions and four rules
before proceeding.

**Condition 2 — It makes at least three verifiable claims.** The document
claims that something is required, enforced, or prohibited. Each claim can
be checked: either it is present and correct, or it is present but not
enforced, or it is absent. If the document makes only vague statements
without verifiable claims, it is not ready to be an experiment source. It
should first be refined into a document that makes specific claims.

**Condition 3 — At least one claim has a known failure mode.** You can
describe what it would look like for the claim to appear satisfied while
actually failing. If you cannot describe a failure mode for any of the
claims, the document is too simple to be an experiment source. The experiment
would pass trivially and reveal nothing.

If all three conditions are met, the document is ready. Apply the
six-step translation method from HOW-TO-TRANSLATE-A-DOCUMENT-INTO-AN-EXPERIMENT
to produce the experiment design.

---

### The Transition Experiment Readiness Check

A transition between two phases is ready to be an experiment when both
phases have at least one document each, and the upstream document makes
decisions that the downstream document must either acknowledge or incorporate.

Before running the transition experiment, produce the upstream context packet
by hand. Read the upstream document and extract: decisions made, constraints
inherited, knowledge edges, boundary markers, and rejected items. This
extraction is the source of truth for the transition gate experiment. If the
extraction produces fewer than three entries across all five categories,
the upstream document did not make enough decisions to produce a meaningful
transition test. Find a richer upstream document before proceeding.
