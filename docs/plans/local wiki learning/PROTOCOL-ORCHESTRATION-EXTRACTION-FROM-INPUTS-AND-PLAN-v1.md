# Protocol: Orchestration Extraction From Input Files And Produced Plan

Status: draft v1.

Purpose:

This protocol explains how to extract a human orchestration story from a user
task, input files, produced planning files, state files, and final plan.

The goal is not to summarize documents and not to list technical components.
The goal is to explain, in human language, how the user's task becomes a
controlled path from intention to verified result.

## 1. What This Protocol Is For

Use this protocol when the source package contains:

- an original user request;
- input files or input ZIPs;
- produced planning artifacts;
- state files;
- merged/canonical documents;
- a final plan or implementation plan.

The expected output is an orchestration explanation like:

```text
What are we building?
Why does it matter?
What is the new capability?
How does work move from input to result?
What phases/sessions make this happen?
What is verified at each point?
What is the current state?
```

## 2. Core Rule

Do not start from phase tables, file lists, class names, factory numbers, task
numbers, or interface names.

Start from the human product or system capability.

Bad start:

```text
The plan contains F566-F579, T219-T226, CF-256-CF-269.
```

Good start:

```text
We are building a Giant Shop Marketplace orchestration: seller onboarding,
catalog moderation, cart-to-order saga, escrow payments, fulfillment, disputes,
seller payouts, and search ranking.
```

## 3. Source Classification

Before extracting orchestration, classify every source.

| Source type | Meaning | How to use it |
|---|---|---|
| USER_PROMPT | The human task that started the work | Defines the goal |
| INPUT_FILES | Original material before planning | Defines the raw domain and constraints |
| PRODUCED_FILES | Files created by planning work | Defines what the planning process produced |
| STATE_FILE | Current progress / completion / validation state | Defines whether work is complete, draft, blocked, or waiting |
| CANONICAL_MERGED_DOC | Current accepted system truth | Beats older input files |
| PLAN_FILE | Human or implementation plan | Shows intended execution path |
| VALIDATION_FILE | Proof or failure report | Shows whether claims are verified |
| RAW_SESSION | Conversation history | Use only to understand how the plan was born |
| GENERATED_OUTPUT | Prior model answer | Evidence only; never trust without source check |

Authority order:

1. Current user instruction.
2. The specific protocol or skill named by the user.
3. State / canonical merged documents.
4. Produced planning artifacts.
5. Input files.
6. Raw session.
7. Prior generated summaries.

## 4. Required Input Bundle

Never extract orchestration from a single plan file if input/context files exist.

Required bundle:

```text
1. User prompt or task origin
2. Input files / input ZIP
3. Produced files / produced ZIP
4. State file
5. Final plan or implementation plan
6. Validation report, if present
```

Example:

Flow 33 cannot be extracted only from `FLOW33_IMPLEMENTATION_PLAN_v2.md`.
It requires:

```text
33-system initiation.md
33-files.zip
33-input files.zip
produced plan files
state / merged docs
```

Flow 16 cannot be extracted only from a factory list.
It requires:

```text
16-input files.zip
16 - files.zip
SESSION_STATE_MERGE.md
FLOW16_P0_PLAN.md
FLOW16_VALIDATION.md
merged architecture/task/skill/BFA files
```

## 4.1. Special Case: Huge Session History Plus Produced Artifacts

Some flows are not born from one clean plan file. They are born from a long
conversation where the user repeatedly corrected, refined, and pushed the model
until a real plan emerged.

In that case, the long session file is not just transcript noise. It is the
history of how the orchestration was discovered.

Use this mode when the package contains:

```text
large session history file
input ZIP
produced files ZIP
current SESSION_STATE / merged state
renumber map or merge map
final plan / validation
```

Example:

```text
33-system initiation.md
33-files.zip
33-input files.zip
SESSION_STATE_MERGE.md
FLOW33_RENUMBER_MAP.json
implementation / reference plans
```

The job is not to summarize the whole session. The job is to recover the
decision path that produced the orchestration.

Required extraction from a huge session:

```text
1. Initial human problem
2. Major corrections from the user
3. Architectural concepts that survived corrections
4. Produced artifacts that became canonical
5. Current state after merge / renumbering
6. Final red thread of the orchestration
```

Bad:

```text
The session discusses kernel, bootstrap, GraphRAG, arbiters, and UI.
```

Good:

```text
Across the session, the plan evolved from "write code from documents" into a
self-building process: a small hand-written kernel reads the PlanBundle,
bootstraps registries and GraphRAG, then runs implement-family loops with AI
generation, arbiters, progress ledger, UI, and human gates.
```

## 4.2. Special Case: Input ZIP Plus Final Files ZIP

Some flow packages arrive as two ZIPs:

```text
[N]-input files.zip
[N] - files.zip
```

Treat them as two different time layers.

The `input files` ZIP is the older/source layer. It often contains the original
flow before merge, old numbering, older validation, or predecessor flow names.

The `files` ZIP is the produced/final layer. It usually contains the canonical
merged state, updated numbering, validation, and final artifacts.

Required analysis order:

```text
1. Open final state first: SESSION_STATE_MERGE.md or equivalent.
2. Identify current counts and completion state.
3. Open the flow master plan: for example FLOW16_P0_PLAN.md.
4. Open canonical merged architecture/task/BFA/skills files.
5. Open validation report.
6. Only then compare against the input ZIP.
```

Why:

The final ZIP tells what the system currently believes. The input ZIP explains
where it came from. If you read input first, you may report stale numbering or
old status as current truth.

Example:

```text
16-input files.zip contains the older Giant Shop Marketplace material
previously known as FLOW-08.

16 - files.zip contains the produced FLOW-16 canonical merge.

The orchestration must explain both:
- same marketplace domain;
- new canonical numbering;
- what was strengthened during merge;
- final validation state.
```

Bad:

```text
FLOW-08 and FLOW-16 are two marketplace systems.
```

Good:

```text
FLOW-16 is the merged, renumbered, strengthened version of the older Giant Shop
Marketplace material. The same domain moved from older IDs to canonical IDs
inside the global XIIGen state.
```

## 4.3. Special Case: Adapter / Port Plan ZIPs

Some plans describe a port or adapter from an existing engine into a new design
tool or host platform.

Examples:

```text
figma to code adapt to canva.zip -> Canva text adapter
w1-webflow-plan.zip -> Webflow layout + text adapter
m1-miro-plan.zip -> Miro shape heuristics adapter
```

For this class, the orchestration must explain three things before sessions:

```text
1. What the host platform gives us.
2. What the existing engine already knows how to do.
3. What adaptation problem is new for this platform.
```

Good comparison:

```text
Canva: text-only, easiest port; translate text runs into the existing text CSS branch.
Webflow: full layout + text; Webflow exposes real CSS, so the adapter can compare generated CSS to platform truth.
Miro: freeform canvas; no semantic layout, so the adapter must infer structure and layout from shapes and coordinates.
```

Bad:

```text
Create canva-adapter.ts, webflow-adapter.ts, miro-adapter.ts.
```

Why bad:

It names files but misses the reason each adapter is different.

Required adapter plan shape:

```text
What plugin/app are we building?
What can a designer select?
What data can the platform provide?
Which part of the existing pipeline is reused?
What new adapter logic is needed?
What does the UI show?
What can be written back?
What proves the adapter works?
What is current STATE.json session?
```

If there are multiple related port plans, always include the comparison table.
It prevents the false impression that every adapter is "same steps, different
file names".

## 4.4. Special Case: Multi-Feature Platform Flow

Some flows add several platform capabilities at once. They are not one feature
and not just a registry update.

Example:

```text
15-build mvps files
```

The orchestration must explain both layers:

```text
1. Definition-time orchestration:
   how task types, BFA rules, stress tests, skills, decisions, validation, and
   canonical merge are produced.

2. Runtime orchestration:
   how a real user/system event is routed through a flow template into the
   correct task type and quality gates.
```

For Flow 15, the human product is:

```text
Multi-Tenancy MVP Builder Platform capabilities:
- export generated code to GitHub;
- install plugins safely;
- discover plugins in a marketplace;
- fork a sandbox and commit/discard changes;
- manage subscriptions;
- meter usage;
- enforce paywalls.
```

The runtime red thread is:

```text
User/system event
-> Flow Template 38 dispatcher
-> matching task type
-> AF stations
-> iron rules and quality gates
-> BFA conflict/stress checks
-> canonical merge / runtime capability available
```

Bad:

```text
P4B adds task types, P5 adds BFA, P6 adds skills.
```

Good:

```text
The platform learns several monetized MVP capabilities. First it defines each
capability as a reusable task type, then protects them with BFA and stress
tests, then stores reusable skills/decisions, then merges everything into the
canonical engine. At runtime, user events like PluginInstallRequested or
SubscriptionCreateRequested are routed by the flow template into the correct
task type and gates.
```

## 4.5. Special Case: System-Wide Flow State After Many Prior Flows

Some packages are not only about one feature. They also describe the whole
system after many previous flows.

Example:

```text
18-files.zip
18 - input files.zip
```

In this case, the orchestration must explain two levels:

```text
1. System-wide orchestration pattern:
   how XIIGen evolves through FLOW-01 -> FLOW-N.

2. Specific flow orchestration:
   what this FLOW adds and how its phases implement that capability.
```

For system-wide files, first identify:

```text
global counters / current state;
flow registry;
what previous flows already added;
what this flow adds;
next available IDs;
canonical merged files;
save points and recovery commands.
```

Good:

```text
XIIGen grows as a sequence of release trains. Each FLOW adds factories, task
types, BFA rules, skills, decisions, templates, validation, then merges into
the seven canonical files and updates SESSION_STATE. FLOW-18 is the release
train that gives the system a visual flow builder, code injection engine,
sandbox, marketplace, promotion, Git/versioning, analytics, and final templates.
```

Bad:

```text
FLOW-18 adds F631-F696, T247-T268, CF-295-CF-328.
```

Why bad:

It reports counters but misses the orchestration pattern of release trains and
canonical merge.

Required FLOW-level extraction:

```text
what the flow adds in product terms;
how many phases;
what each phase makes possible;
where save points/recovery commands exist;
which AF stations do the work;
which BFA/DNA/Iron Rules protect it;
how it merges into canonical state;
what the next flow can start from.
```

## 4.6. Special Case: Marketplace / Financial / Contract Domain Flow

Some flows build a domain platform with money, contracts, evidence, reputation,
or compliance. These flows must be explained through both generation-time and
runtime orchestration.

Example:

```text
17-files.zip
17-input files.zip
Freelancer Marketplace / Upwork-like platform
```

Required split:

```text
1. Generation orchestration:
   how XIIGen adds factories, task types, BFA rules, stress tests, skills,
   decisions, validation, and canonical merge.

2. Runtime orchestration:
   how the finished platform handles real business processes through events,
   timers, DAG templates, sagas, ledgers, and compliance gates.
```

For freelancer / contract platforms, the runtime story must include:

```text
job creation;
proposal / shortlist;
contract activation;
escrow funding;
milestone delivery;
evidence capture;
review / release;
dispute and refund;
reputation update;
enterprise compliance / KYC gates.
```

Safety mechanisms are not optional details. They are part of the orchestration:

```text
idempotent money operations;
append-only ledger;
escrow hold and dispute in one atomic transaction;
fresh KYC before contract activation;
participant-only access to work diary evidence;
durable timers for auto-release, evidence capture, and KYC expiry;
compensation when saga steps fail.
```

Good:

```text
FLOW-17 teaches XIIGen to build an Upwork-like freelancer marketplace. During
generation, it adds the factories, task types, BFA rules, skills, and templates
for profiles, jobs, proposals, contracts, escrow, disputes, reputation, and
enterprise compliance. At runtime, the FlowOrchestrator turns API calls, queue
events, and timers into contract/escrow/dispute DAGs with idempotent money
operations, append-only ledgers, evidence capture, and KYC gates.
```

Bad:

```text
FLOW-17 adds F580-F630, T227-T246, CF-270-CF-294.
```

Why bad:

It hides the business lifecycle and the safety model. For financial/contract
domains, the safety model is the orchestration.

## 4.7. Special Case: Migration Certificate Plus New Flow Package

Some packages contain two related but different stories:

```text
1. A migration / certificate phase that changes the whole project baseline.
2. A new FLOW that adds a product or platform capability after that baseline.
```

Example:

```text
PHASE_19_CERTIFICATE.md
19-files.zip
19-input files.zip
```

For this class, do not merge the two stories into one vague "FLOW-19 did
everything" paragraph.

Required split:

```text
Migration / certificate layer:
What global transition was completed?
What stack or architecture baseline changed?
Which documents were updated?
What old assumptions were removed?
What does the certificate prove?

New FLOW layer:
What capability does this flow add?
What phases implement it?
Which artifacts are created?
How is it validated and merged?
What can the next flow rely on?
```

For Phase 19 + FLOW-19:

```text
Phase 19 = migration to a stack-neutral / Node.js + TypeScript + NestJS + React web baseline.
FLOW-19 = CI/CD & DevOps Control Plane.
```

The human orchestration must explain:

```text
1. The project first moves to a new stack-neutral baseline.
2. Then FLOW-19 adds production-grade DevOps capability:
   component catalog, ephemeral environments, config and secrets management,
   GitOps, tests, backups, disaster recovery drills, and multi-tenant onboarding.
3. The flow uses pre-flight numbering from the previous state.
4. Each phase has save points and recovery commands.
5. Results merge into the seven canonical documents.
6. Backward compatibility is preserved.
```

Bad:

```text
FLOW-19 adds CI/CD and updates files.
```

Good:

```text
Phase 19 first changes the foundation: XIIGen is no longer tied to the old
.NET/React Native baseline and becomes stack-neutral. On top of that new
baseline, FLOW-19 adds a DevOps control plane: the engine can define components,
create environments, manage secrets and configs, run GitOps pipelines, test and
recover deployments, and prepare the next flow from an updated canonical state.
```

Why this matters:

A migration certificate changes what future plans are allowed to assume. A flow
package adds new engine capability. The orchestration must keep those roles
separate.

## 4.8. Special Case: Regulated Platform / Graph API / Advertising Flow

Some flows add a large regulated platform capability: public Graph API,
advertising, sponsored content, consent, billing, attribution, quotas, or policy
enforcement.

Example:

```text
20-final files.zip
20-input files.zip
FLOW-20: Sponsored Content + Graph API + Advertising Platform
```

For this class, the orchestration must explain:

```text
1. What business/platform capability is being added.
2. Which regulated or safety-sensitive operations must be protected.
3. How the flow is built through phases and canonical merge.
4. How code generation is orchestrated internally.
5. Which configuration values are freedom/config, not hardcoded product logic.
6. How backward compatibility with previous flows is protected.
```

FLOW-20 required human story:

```text
The engine learns to build a Graph API and advertising platform: developers can
access graph data through controlled APIs, advertisers can run sponsored content
and auctions, consent is checked before targeting, billing is append-only, and
old flows remain safe.
```

Important runtime/business processes to look for:

```text
developer Graph API access;
consent gate before targeting;
ad auction under latency limit;
sponsored content delivery;
attribution window;
usage quotas;
append-only billing ledger;
policy / safety checks;
tenant isolation.
```

Required internal orchestration layer:

```text
INVENTORY -> SYNTHESIS -> JUDGMENT
```

Meaning:

```text
INVENTORY gathers context, patterns, prior flows, and constraints.
SYNTHESIS generates the implementation.
JUDGMENT reviews against DNA, iron rules, quality gates, BFA, stress tests, and backward compatibility.
```

Bad:

```text
FLOW-20 adds 118 factories and 20 task types.
```

Good:

```text
FLOW-20 adds a production advertising and Graph API capability. The flow first
defines the platform contracts, then uses INVENTORY/SYNTHESIS/JUDGMENT to
generate and verify code. Consent must happen before targeting, ad auction must
respect latency, billing must be append-only, quotas and attribution are
configurable, and BFA/stress tests protect previous flows.
```

Why this matters:

Advertising and Graph API flows are easy to describe as "more services", but
their orchestration is really about consent, policy, latency, billing integrity,
configuration, and backward compatibility.

### 4.8.1. Required Bridge: Functional Description To Architecture

For regulated platform flows, do not jump from product functionality directly
to factories, task types, BFA, or implementation phases.

The required bridge is:

```text
functional/product description
-> logical zones
-> factory families
-> design decisions
-> task types / factories / BFA / skills / merge
```

For FLOW-20, the bridge is:

```text
Functional description:
Graph API + Sponsored Content + Ads Platform + Multi-Tenant Governance

Logical zones:
A. Graph API Plane
B. Ads Management
C. Ads Delivery
D. Measurement
E. Revenue Integrity
F. Multi-Tenant Governance

Factory families:
Zone A -> Families 103-108
Zone B -> Families 109-112
Zone C -> Families 113-114
Zone D -> Family 115
Zone E -> Families 116-117
Zone F -> Family 118

Design decisions:
DD-164..DD-179 explain the architectural choices that make the functional
requirements implementable and enforceable.
```

Why this bridge matters:

```text
Zones explain responsibility boundaries.
Families explain which service groups belong together.
Design decisions explain why the implementation must work one way and not another.
Only after that can task types, factories, iron rules, BFA, and skills make sense.
```

Bad:

```text
The platform needs Graph API and ads, so P1 creates factories and P2 creates task types.
```

Good:

```text
The platform needs Graph API, ads, measurement, revenue integrity, and tenant
governance. These become six zones. Each zone is split into coherent factory
families: Graph API gateway/schema/permissions/webhooks; ads management;
delivery/auction; measurement; fraud/billing; tenant governance. Design
decisions then lock the critical choices: REST public API, consent before
targeting, Redis-only stateless auction, append-only spend ledger, HMAC
webhooks, and configurable attribution windows.
```

For every complex product flow, explicitly name this bridge. If the bridge is
missing, the orchestration is incomplete.

## 4.9. Special Case: No-Code Forms / Automation / Connector Platform

Some flows build a no-code business platform where non-developers create forms,
collect submissions, run automations, and connect external systems.

Example:

```text
21-files.zip
21-input files.zip
FLOW-21: Forms and Flows / no-code form automation platform
```

The orchestration must start with the user-facing lifecycle:

```text
business user designs a form;
form is published;
end user fills it;
submission is received;
submission is normalized, validated, anti-spam checked, and persisted;
notifications are sent;
data is exported or sent to CRM/webhook/payment/mailing list;
automation recipes run IF-THEN flows;
external connectors authenticate through OAuth;
tenant isolation protects all of it.
```

The functional-to-architecture bridge must show that zones follow the lifecycle,
not random technical categories.

FLOW-21 bridge:

```text
Functional need: no-code forms + submissions + notifications + integrations +
recipes + connectors + multi-tenant isolation.

Zones:
A. Form Schema & Authoring
B. Runtime / Wizard / AI Assistant
C. Submission Pipeline
D. Record Lifecycle
E. Notifications
F. Integration Feeds
G. Automation Recipes
H. Connector SDK
I. Multi-Tenant Isolation

Families:
A -> Family 119
B -> Family 120
C -> Family 121
D -> Family 122
E -> Family 123
F -> Family 124
G -> Family 125
H -> Family 126
I -> Family 127

Design decisions:
DD-180..DD-191 lock the architecture: form schema in Elasticsearch,
persist-before-emit, anti-spam as a fabric gate, recipes as DAGs, and forms
multi-tenancy inheriting proven isolation models.
```

Good:

```text
FLOW-21 teaches XIIGen to build a no-code forms and automation platform. The
architecture follows the lifecycle: author form, run form, receive submission,
persist and audit record, notify people, send data to external systems, run
automation recipes, connect through OAuth, and isolate every tenant. Only after
this lifecycle is clear do zones, families, DDs, task types, factories, BFA, and
skills make sense.
```

Bad:

```text
FLOW-21 adds 49 factories, 20 task types, and 9 zones.
```

Why bad:

It hides the form/submission/automation lifecycle and does not explain why the
zones were chosen.

## 4.10. Special Case: Visual Editor / Site Builder / CMS Platform

Some flows build a visual editing and publishing platform. They must be
explained from the point of view of what the business user sees and does.

Example:

```text
22-files.zip
22-input files.zip
FLOW-22: Visual Editor & Site Builder Platform
```

Required first layer:

```text
business user visually builds pages;
admin creates custom content types;
editor writes and reviews content;
publisher publishes through SSG/CDN/cache/search pipeline;
designer changes design tokens;
media is transformed and optimized;
workspaces are isolated by tenant/workspace.
```

For this class, use this analysis order:

```text
1. Final SESSION_STATE_MERGE.md first: current truth and final numbering.
2. Renumbering map: translate old input numbering to final canonical numbers.
3. MASTER PLAN "No-code explanation": what user sees.
4. FLOW DETAILS zones: how product experience is divided.
5. ENGINE_ARCHITECTURE: factory families and fabric resolution.
6. UNIFIED_SOURCE_INDEX: design decisions explaining why.
7. BFA / skills / validation: guardrails and reusable patterns.
8. Master execution phases: how it was implemented.
```

The zones should be described in the order of the user's experience:

```text
Visual editor canvas
-> CMS / content modeling
-> content authoring
-> publishing pipeline
-> theme / design system
-> routing / SEO
-> media library and cross-cutting workspace concerns
```

Key technical decisions must be translated into user/system effect:

```text
Hybrid SSR/SSG -> preview is always fresh, published site is fast.
Optimistic concurrency / ETag -> multiple people can work without blocking each other too much.
Append-only events / CloudEvents -> publishing and audit history are reliable.
workspaceId = tenantId -> isolation reuses proven multi-tenant model.
AI assist non-blocking -> editor never waits for AI to continue working.
```

Bad:

```text
FLOW-22 adds 44 factories, 20 task types, 28 BFA rules, and 10 skills.
```

Good:

```text
FLOW-22 gives XIIGen the ability to generate a visual site builder and headless
CMS. The user's journey is the structure: build a page visually, define content
models, write content, publish through a safe pipeline, manage themes/SEO/media,
and keep every workspace isolated. The final files renumber the older input
artifacts, so descriptions must use the canonical F901-F944 / T327-T346 state.
```

Why this matters:

Visual builder flows are easy to over-technicalize. The orchestration must keep
the user-facing authoring/publishing experience visible from start to finish.

## 4.11. Special Case: Extension Of A Previous Flow

Some flows are not standalone products. They extend a previous flow by deepening
one area.

Example:

```text
23-files.zip
23-input files.zip
FLOW-23: Visual Editor Extended
extends FLOW-22: Visual Editor & Site Builder Platform
```

For this class, start by stating the relationship:

```text
FLOW-22 built the base site builder.
FLOW-23 deepens the editor into a Webflow/Figma/Wix-like canvas engine.
```

The orchestration must explain:

```text
what already existed from the previous flow;
what becomes deeper or more powerful now;
which file is the source of truth for the new flow;
whether input files are old pattern/delta files;
what renumbering must be applied;
how backward compatibility is preserved.
```

Mandatory analysis order:

```text
1. Open the P0 plan / master plan first if it exists.
2. Check whether it explicitly contains no-code explanation, renumbering map,
   phase breakdown, validation, examples, RAG notes, and post-flow state.
3. If yes, treat it as the main source of truth for the new flow.
4. Open SESSION_STATE_MERGE.md next to confirm counters, registry, flow details,
   and final state.
5. Read input/delta archives after that only to understand historical pattern,
   document style, or old numbering.
6. Never copy old IDs from input/delta files until the renumbering map is applied.
```

FLOW-23 human capability:

```text
hierarchical canvas nodes;
symbols / reusable blocks;
layers and collaborative editing;
versioning and undo/redo;
deterministic layout engine;
AI breakpoint advisor;
data binding to CMS via JSONPath;
template mode preview;
repeaters;
multi-tenant hardening;
code export to production frameworks.
```

Required warning:

If the input archive contains old delta files from a previous flow, do not treat
them as current truth. Use the current P0 plan and final SESSION_STATE as truth,
and use old files only for structure/pattern reference.

Bad:

```text
FLOW-23 is another visual editor and adds F697-F733.
```

Good:

```text
FLOW-23 extends the FLOW-22 site builder. The base editor already exists; this
flow adds the deeper canvas mechanics: node hierarchy, symbols, layers,
collaboration, deterministic layout, CMS data binding, template mode, repeaters,
tenant hardening, and code export. The P0 plan is the source of truth, while old
input delta files only show the expected document pattern. Current canonical
numbers are F945-F981 and T347-T366.
```

Why this matters:

Extension flows require a before/after story. Without it, the answer either
repeats the previous flow or treats old input numbers as current.

## 4.12. Special Case: Personal AI Tutor / Learning Calendar Flow

Use this branch when the flow builds an adaptive learning system, tutor,
calendar-based education assistant, quiz/gamification loop, or domain-pack
learning product.

Example:

```text
24-files.zip
24-input files.zip
FLOW-24: Personal AI Tutor / Learning Calendar Extension
```

Required source order:

```text
1. Final merged files first: SESSION_STATE_MERGE.md and ENGINE_ARCHITECTURE_MERGED.md.
2. Use them for current state, final IDs, domain packs, zones, task types,
   templates, DNA compliance, and backward compatibility.
3. Use input files for the original product request and deeper descriptions,
   but translate old IDs through the current merged numbering.
4. If input files have old ranges, explicitly say they were renumbered.
```

The orchestration must start from the learning lifecycle, not from artifacts.

Human product story:

```text
student / learner is onboarded;
profile, age, consent, and baseline knowledge are diagnosed;
system builds a knowledge map and 30-day plan;
daily micro-lessons are generated and calendar-synced;
quiz results are graded server-side;
points, badges, streaks, and leaderboard update safely;
tomorrow's plan adapts to today's result;
missed lessons and calendar conflicts trigger recovery;
domain packs specialize the experience for school, business owner, or developer.
```

Technical design bridge:

```text
Learning lifecycle -> zones:
identity and consent;
knowledge diagnosis;
curriculum graph and RAG;
lesson composition and safety gate;
quiz engine and gamification ledger;
adaptive planner and calendar sync;
integration / validation / compatibility.

Hard product risks -> design decisions:
minor safety -> consent gate and SafetyGateToken;
cheating -> server-side grading and immutable attempts;
lost progress -> append-only ledger;
wrong schedule -> timezone from student profile only;
calendar reliability -> connectors behind Queue Fabric;
different audiences -> domain packs as Elasticsearch config, not code branches;
adaptive teaching -> prerequisite Graph-RAG plus vector search.
```

Implementation story:

```text
foundation zones first: identity/consent, diagnosis, curriculum graph;
lesson and quiz/gamification zones can progress after the foundation;
calendar/adaptive planning comes after daily lesson and quiz signals exist;
final validation checks BFA, stress tests, DNA, backward compatibility, and
renumbering from old input IDs to final canonical IDs.
```

Bad:

```text
FLOW-24 adds F982-F1027, T367-T374, and a learning calendar extension.
```

Good:

```text
FLOW-24 turns XIIGen into a calendar-based personal AI tutor. The user journey
starts with onboarding and diagnosis, moves into daily 5-10 minute lessons,
server-graded quizzes, gamified progress, calendar sync, adaptation, and
recovery. The architecture follows that journey through consent/identity,
knowledge diagnosis, curriculum RAG, lesson+safety, quiz+ledger,
planner+calendar, and final validation zones. Current merged files provide the
canonical IDs and final state; input files provide the original request and
detailed descriptions in older numbering.
```

Why this matters:

Learning flows are easy to flatten into "AI generates lessons". The actual
orchestration is the closed loop: diagnose -> teach -> test -> reward -> adapt
-> schedule -> recover, with consent and safety enforced throughout.

## 5. Extraction Algorithm

### Step 1. Find The Human Product

Ask:

```text
What is the thing being built in human terms?
Who uses it?
What real-world process does it support?
```

Examples:

```text
Canva C5: a Canva plugin that adapts selected text elements into the existing CSS pipeline.
Webflow W1: a Webflow Designer extension that compares generated CSS with real Webflow CSS.
Miro M1: a Miro app that infers HTML/layout from canvas shapes and positions.
Flow 16: a giant marketplace system with sellers, listings, cart, order, escrow, disputes, payouts, and search.
Flow 33: a self-building XIIGen bootstrap engine that turns planning documents into executable registries, flows, AI generation loops, checks, and delivery state.
Flow 24: a calendar-based personal AI tutor that diagnoses knowledge, teaches daily micro-lessons, quizzes, gamifies, adapts, and schedules learning safely.
```

If this sentence cannot be written clearly, stop. The orchestration is not ready.

### Step 1.1. Extract The Functional Description First

Before architecture, extract the user's functional/product expectation.

Ask:

```text
What should exist for real users when this is done?
What can users, developers, admins, advertisers, sellers, buyers, or tenants do?
What lifecycle does the product support?
What must be true before the system is safe to use?
```

This section must be free of implementation IDs.

Bad:

```text
The user wants F734-F851 and T287-T306.
```

Good:

```text
Developers can read/write graph nodes and edges through a governed API.
Advertisers can create campaigns, submit creatives, target audiences, enter an
auction, measure results, and receive billing. Tenants remain isolated through
quotas, audit, and noisy-neighbor controls.
```

If a file contains `No-code explanation`, `What We're Building`, `Big Picture`,
or similar human-facing section, read it as the primary source for this step.
Do not treat it as decoration.

### Step 1.2. Build The Functional-To-Architecture Bridge

After the functional description, find the intermediate design layers that make
implementation possible.

Typical bridge:

```text
product capabilities
-> logical zones / domains
-> factory families / service groups
-> design decisions / constraints
-> task types / factories / rules / skills
```

For each layer, explain why it exists:

```text
Zones divide the product into responsibility areas.
Families group services that need high cohesion.
Design decisions lock the non-obvious architecture choices.
Task types turn the design into executable engine contracts.
Factories provide replaceable implementation surfaces.
BFA/rules/tests protect the rest of the system.
```

Do not skip from product description to implementation artifacts. That is a
missing bridge.

For product/lifecycle flows, zones must follow the lifecycle.

Ask:

```text
What does the user do first?
What happens next?
Where does data enter the system?
Where is it validated and persisted?
Where do notifications, integrations, billing, or automation happen?
Where is tenant/security/compliance enforced?
```

If zones do not map to this lifecycle, explain why. If the explanation is
missing, the bridge from product to architecture is incomplete.

For visual/editor products, the lifecycle is usually the user's working order:

```text
create/edit
-> model/configure
-> author
-> preview/review
-> publish
-> route/SEO/cache/search
-> manage media/assets
-> govern workspace/tenant
```

Use that order unless the files prove a different product journey.

For learning / tutor / calendar products, the lifecycle is the teaching loop:

```text
onboard learner
-> collect consent and profile
-> diagnose baseline knowledge
-> build knowledge map and plan
-> generate daily lesson
-> safety-check and localize
-> sync lesson to calendar
-> quiz and grade server-side
-> update ledger, gamification, and knowledge map
-> adapt next plan
-> recover missed/conflicting sessions
```

Use this loop before explaining zones, task types, or factories.

### Step 2. Identify The New Capability

Ask:

```text
What is new here compared with previous flows/plans/projects?
```

Examples:

```text
Canva C5: first simple text-only port.
Webflow W1: full layout/text adapter with real CSS as truth source.
Miro M1: shape heuristics and spatial analysis because Miro has no semantic layout.
Flow 16: long marketplace sagas with compensation, escrow, dispute windows, and two-actor safety rules.
Flow 33: self-building platform loop from PlanBundle to generated, judged, published code.
```

This is the anchor of the explanation. Without it the answer becomes a list.

### Step 3. Extract The Red Thread

Find the central chain:

```text
input
-> transformation
-> execution process
-> verification
-> persisted state
-> human control
-> final proof
```

Flow 33 red thread:

```text
PlanBundle
-> mini kernel
-> bootstrap-flow
-> registries and GraphRAG
-> implement-family loop
-> multi-model generation
-> arbiters and retry
-> GitHub/Jira/Progress Ledger
-> UI/Human Gate
-> first family successfully generated and promoted
```

Flow 16 red thread:

```text
Marketplace domain
-> service factories
-> task types
-> cart-to-order saga
-> idempotency and transactional outbox
-> AI moderation / search / arbitration
-> BFA rules
-> validation and canonical merge
-> engine can generate marketplace implementation safely
```

For long-session flows, the red thread is usually not written in one place.
It must be synthesized from:

```text
conversation corrections
produced plan files
current state
canonical merged documents
validation reports
```

Do not trust one nice paragraph if it conflicts with current state or produced
canonical files.

### Step 3.1. Extract The Recurring FLOW Pattern

When a package contains a flow registry or many prior flows, extract the
repeating orchestration pattern before explaining the current flow.

Ask:

```text
How does one FLOW usually enter the system?
What artifacts does it add?
What gates does it pass?
Where is it merged?
How does it update global state?
What can the next FLOW rely on?
```

Typical XIIGen FLOW pattern:

```text
master plan
-> phases P0..Pn
-> factories / task types / templates
-> BFA rules and stress tests
-> skills / decisions / records
-> validation
-> merge into seven canonical files
-> SESSION_STATE update
-> next ID ranges unlocked
```

This pattern is the orchestration skeleton. The current flow's phases are the
specific payload moving through that skeleton.

### Step 4. Separate Orchestration From Inventory

Inventory says what exists.

Orchestration says how work moves.

Bad:

```text
Add DATABASE FABRIC, QUEUE FABRIC, AI ENGINE FABRIC, RAG FABRIC.
```

Good:

```text
All marketplace services must talk through fabrics, so generated code never
binds directly to PostgreSQL, Redis, Stripe, or AI providers. This keeps the
marketplace tenant-safe, replaceable, and consistent across generated services.
```

Bad:

```text
Add F566-F579.
```

Good:

```text
The marketplace is split into service capabilities: seller identity, catalog,
listing moderation, cart/order, payment escrow, inventory, fulfillment, dispute
handling, payout, and search ranking. These become factories so the engine can
generate and validate each business area separately.
```

### Step 5. Extract Sessions / Phases As Human Work Chunks

Every session must answer:

```text
Session N:
What changes for the user/system?
Why is this session needed before the next one?
What concrete result appears?
What must be verified?
Where is the stop/approval point?
```

Do not write:

```text
Session 3: create shape-classifier.ts, spatial-analyzer.ts.
```

Write:

```text
Session 3: build the Miro inference brain. The app learns to classify canvas
shapes as likely HTML elements and infer layout from positions. After this,
the adapter can understand a Miro board instead of just reading raw shapes.
```

### Step 6. Extract Business Process Before Technical Details

If the flow represents a domain, explain the main business process before
numbers and artifacts.

For Flow 16:

```text
Buyer adds product to cart.
Order saga starts.
Inventory is reserved.
Payment is held in escrow.
Fulfillment path begins.
If delivery or payment fails, compensation runs.
If dispute opens, payout freezes.
After safety window, seller payout is released.
Search ranking learns from signals.
```

Only after this explain factories, task types, BFA rules, skills, and validation.

For financial, contract, marketplace, or compliance domains, also extract the
runtime lifecycle and safety guarantees before engine artifacts.

Example:

```text
Client posts a job.
Freelancers submit proposals.
Client shortlists and creates contract.
KYC/compliance gates pass.
Escrow is funded.
Milestone is delivered.
Evidence is captured by timer.
Payment is released or dispute opens.
Reputation is updated from immutable records.
```

Then explain which flow templates, task types, BFA rules, and skills implement
that lifecycle.

If the flow contains runtime events, explain at least one real event path.

Example:

```text
PluginInstallRequested
-> dispatcher recognizes plugin install
-> plugin installation task type starts
-> sandbox/security checks run
-> marketplace/plugin rules are enforced
-> install is approved or blocked
-> status is recorded and visible to the user
```

Do not hide runtime behavior behind IDs.

Bad:

```text
Flow Template 38 routes to task types Txxx.
```

Good:

```text
When the user clicks install plugin, the dispatcher routes that event into the
plugin installation line, runs security and compatibility checks, then either
installs the plugin or blocks it with a visible reason.
```

### Step 6.1. Preserve The Story Of Realization

When a flow is about system implementation, not just product functionality,
extract the "how realization happens" story.

Ask:

```text
What is written manually once?
What becomes data after that?
What starts the first execution?
What process repeats for each unit/family/module?
What checks prevent bad output from being accepted?
Where is progress stored?
How can a human see, pause, approve, or resume?
```

For Flow 33 this gives:

```text
manual mini-kernel
-> bootstrap-flow
-> PlanBundle to registries
-> GraphRAG seeding
-> implement-family meta-flow
-> multi-model generation
-> arbiters and retry
-> Progress Ledger
-> UI/Human Gate
-> promotion ladder
```

This is different from ordinary implementation phases. It explains how the
system turns a plan into repeated self-building execution.

### Step 7. Handle Renumbering And Version Drift

If input files and produced files use different numbers, explain it.

Example:

```text
The input files describe the older FLOW-08 marketplace numbering.
The produced FLOW-16 package contains the merged canonical numbering.
The same domain was renumbered during merge:
F244 -> F566
T83 -> T219
CF-64 -> CF-256
```

Never present old and new numbers as two separate systems unless evidence says
they are separate.

### Step 8. Identify Guardrails And Safety Mechanisms

For each orchestration, identify what prevents chaos.

Examples:

Flow 16:

```text
idempotency key registry
transactional outbox
compensation chain
tenant isolation
buyer/seller conflict rules
dispute freezes payout
seller cannot withdraw confirmed order inventory
```

Flow 33:

```text
state tracker
sentinel
resumable bootstrap
GraphRAG impact checks
5 arbiters
retry loop
human gate
progress ledger
promotion ladder
```

If the flow has a generation pipeline, identify internal orchestration engines.

Example:

```text
INVENTORY: gathers source context, previous flows, patterns, and constraints.
SYNTHESIS: generates the new code/artifacts.
JUDGMENT: reviews with AF stations, BFA, DNA, iron rules, quality gates, stress tests.
```

Do not collapse this into "AI generates code". The orchestration is the
separation of gathering, generating, and judging.

If the flow uses configurable policy, quotas, thresholds, targeting rules,
attribution windows, or similar values, identify the freedom/config layer.

Example:

```text
Consent rules, auction timeout, attribution window, usage quotas, and billing
policy must be configurable. They are not hidden as hardcoded product decisions.
```

### Step 8.1. Use Design Decisions As The Architecture Bridge

When Design Decisions exist, treat them as first-class orchestration evidence.

Design Decisions answer:

```text
What business requirement forced this architecture choice?
What alternative was rejected or avoided?
Which zone/family/task does this decision constrain?
Which iron rules, quality gates, or BFA rules should enforce it later?
```

Good DD extraction:

```text
Business wants ad auction under 50 ms.
DD says auction must be stateless and Redis-only.
That constrains the Auction & Delivery family and later becomes iron rules and
latency quality gates for the auction task type.
```

Bad DD extraction:

```text
DD-166 exists.
```

For FLOW-20, required DD bridge examples:

```text
Graph API for developers -> REST graph paths, not public GraphQL.
Fine-grained security -> per-node / per-edge / per-field permissions.
Auction under 50 ms -> stateless auction + Redis-only hot state.
Privacy -> consent-before-evaluation blocking gate.
Political ads -> mandatory dual gate.
Financial audit -> append-only spend ledger.
Webhook safety -> HMAC mandatory.
Multi-tenant isolation -> edge-only tenant resolution and per-tenant quotas.
Fraud protection -> fraud gate blocking before billing.
Attribution flexibility -> attribution windows as FREEDOM config.
```

### Step 9. Compare Source Version With Final Version

When input files and produced files represent different lifecycle moments,
compare them explicitly.

Ask:

```text
What existed in the input version?
What changed in the produced version?
Was numbering updated?
Were links to previous flows added?
Was validation strengthened?
Were new engine primitives introduced?
Was backward compatibility preserved?
```

For Flow 16, this check should find:

```text
older marketplace material existed in input files;
canonical FLOW-16 numbers replaced old numbers;
links to previous FLOW-01..FLOW-15 were added;
validation increased / became final;
EP-4 idempotency and EP-5 transactional outbox were clarified as engine primitives;
backward compatibility with prior flows was preserved.
```

Do not hide this comparison. It is part of the orchestration story because it
shows how a domain becomes canonical in the larger system.

### Step 9.1. Separate Global Migration From Flow Capability

If a package includes a certificate, migration phase, stack transition, or
baseline update, separate it from the current flow capability.

Ask:

```text
Is this file proving that the whole project baseline changed?
Is this other file defining a new flow capability?
What assumptions changed for all future work?
What capability is newly added only by this flow?
```

Example:

```text
PHASE_19_CERTIFICATE.md proves the project migrated to a stack-neutral modern
baseline.

FLOW-19 files define the CI/CD & DevOps Control Plane capability.
```

Do not describe both as "FLOW-19 added DevOps". That loses the migration story.

### Step 9.2. Identify The Source Of Truth When Inputs Are Historical

Some packages include old input files, prior-flow delta files, or template files
whose numbering is no longer current. Do not assume the input archive is the
highest authority.

Ask:

```text
Is there a P0 plan or master plan that declares itself as the current flow plan?
Does that plan contain a renumbering map?
Does final SESSION_STATE_MERGE.md confirm the post-flow counters?
Do input files use older IDs or a previous flow name?
Are the input files meant as source requirements, or only as historical examples?
```

Required conclusion:

```text
Current source of truth: [file]
State verification source: [file]
Historical / pattern-only source: [file/archive]
Renumbering rule: [old range] -> [new range]
```

Example:

```text
For FLOW-23, FLOW23_P0_PLAN.md is the working source of truth. It contains the
no-code explanation, renumbering map, phase breakdown, validation, examples,
RAG notes, and post-flow state. SESSION_STATE_MERGE.md confirms the final
state. The old input archive is used only for delta-file pattern and style,
not for current numbering.
```

Never mix old and new IDs in the same explanation unless explicitly explaining
the renumbering.

### Step 10. Extract Verification And Completion State

State must be explicit.

Examples:

```text
STATE.json says current_session: 0, so the plan is ready but implementation has not started.
FLOW16_VALIDATION says 99/99 PASS, so FLOW-16 is complete and merged.
Flow 33 plan says Phase 0, so the self-building implementation is planned but not executed.
```

Never say "complete" unless a state or validation file proves it.

### Step 11. Explain The Analysis Process When Asked

If the user asks "how did you work" or "describe the process you used", answer
with the same discipline as the orchestration itself.

Required process explanation:

```text
1. What I received as input
2. Which files represented original input
3. Which files represented produced/current state
4. How I scanned for the main blocks
5. How I synchronized current numbers/state
6. How I extracted the red thread
7. What I deliberately filtered out as low-level noise
8. How I checked consistency before writing the answer
```

Good process explanation:

```text
I used 33-system initiation.md to understand how the idea evolved, 33-input
files.zip to see the original source material, 33-files.zip and
SESSION_STATE_MERGE.md to see the canonical produced state, then synchronized
the numbering through FLOW33_RENUMBER_MAP.json. From that I extracted the
surviving red thread: Kernel -> Bootstrap -> Registries/GraphRAG ->
Implement-Family -> Arbiters -> Ledger/UI.
```

Bad process explanation:

```text
I read the files and summarized the plan.
```

Why bad:

It does not show source roles, synchronization, filtering, or consistency check.

### Step 12. Use Human Synthesis, Not File Copying

Do not copy long document fragments 1-to-1.

The job is to synthesize facts from multiple files into a coherent human
picture.

For domain flows, the final explanation should include:

```text
plain product analogy if helpful;
domain story;
key orchestration layers;
main process table if useful;
first-time capabilities;
why the system is safe/reliable;
final implementation process;
completion state.
```

For Flow 16, a good synthesis highlights:

```text
Giant Shop Marketplace;
service factories;
fabrics;
task types;
cart-to-order saga;
EP-4 / EP-5;
AF stations;
BFA rules;
skills;
DNA invariants;
P0-P6 production process;
99/99 validation.
```

But it must not read like a copied index. It must explain why those pieces work
together.

### Step 13. Final Simulation Check

Before finalizing, mentally simulate the extracted orchestration.

Ask:

```text
If someone followed this explanation, would they understand what is being built?
Would they understand why the order matters?
Would they understand what gets produced after each phase?
Would they understand how the result is checked?
Would they understand what the current state is?
Did I replace the product story with technical inventory?
```

If the answer is no, rewrite.

## 6. Output Format

Use this format for the final orchestration extraction:

```markdown
# Orchestration: [Name]

## What We Are Building

[Plain human explanation.]

## What It Will Be Able To Do

1. ...
2. ...
3. ...

## Main New Capability

[What is new/different/hard here.]

## Functional To Architecture Bridge

[Product capabilities -> zones/domains -> families/service groups -> design
decisions -> implementation artifacts.]

## How This Differs From Previous Flows

| Flow / project | What it did | Difference here |
|---|---|---|

## How The Work Is Orchestrated

### Layer / Stage 1: [Human name]
What happens:
Why it matters:
What appears after:
How it is verified:

### Layer / Stage 2: ...

## Main Business Or System Process

[The real process in human steps.]

## Safety And Control

[Guardrails, gates, state, retries, human approval.]

## Current State

[From STATE / validation / merged docs.]

## Source And Process Notes

[Only include when useful or requested: input files used, produced files used,
state/renumbering source, and how the red thread was extracted.]

## What Can Happen Next

[Next allowed step, not invented beyond evidence.]
```

## 7. Good And Bad Examples

### Good Example: Flow 33

Good:

```text
We are building the XIIGen self-building bootstrap engine. It starts from a
small hand-written kernel, reads the PlanBundle, turns planning documents into
registries and GraphRAG, then uses an implement-family flow to generate,
judge, publish, and track code. Human control is preserved through state,
progress ledger, UI, and approval gates.
```

Bad:

```text
Build Database Fabric, Queue Fabric, AI Fabric, Flow Engine, RAG, GitHub,
Jira, UI, Chat.
```

Why bad:

It lists parts but does not explain how the system builds itself.

### Good Example: Flow 16

Good:

```text
We are building marketplace orchestration. The engine can generate seller
onboarding, listing moderation, cart-to-order saga, escrow payment, inventory,
fulfillment, disputes, seller payout, and ranking. The key new capability is a
multi-day saga with idempotency, transactional outbox, and compensation if
payment, inventory, or delivery fails.
```

Bad:

```text
Add 14 factories, 8 task types, 14 BFA rules, 8 skills.
```

Why bad:

It hides the marketplace process and does not explain buyer/seller/order/payment
flow.

### Good Example: Flow 18

Good:

```text
FLOW-18 is the release train that gives XIIGen a visual cockpit: users can build
flows on a canvas, generate code for nodes, test safely in a sandbox,
collaborate, publish templates, promote flows, version them in Git, and learn
from analytics. It follows the normal XIIGen release pattern: phases with save
points, AF station generation/review, BFA/DNA protection, stress testing,
template completion, canonical merge, and SESSION_STATE update.
```

Bad:

```text
FLOW-18 adds 66 factories, 22 task types, 34 BFA rules, and 20 stress tests.
```

Why bad:

It reports size but does not explain that FLOW-18 creates the visual flow
creation and code injection engine, nor how it moves through the release train.

### Good Example: Flow 17

Good:

```text
FLOW-17 builds a freelancer marketplace. The generation plan adds the platform
contracts for profiles, jobs, proposals, contracts, escrow, evidence, disputes,
reputation, and enterprise compliance. The runtime orchestration starts from
real events: API calls, queue events, and durable timers trigger marketplace,
contract-escrow, dispute, and compliance DAGs. Money is protected by
idempotency, append-only ledger, escrow/dispute atomicity, KYC gates, and
participant-only evidence access.
```

Bad:

```text
FLOW-17 creates 51 factories, 20 task types, 25 BFA rules, and 12 skills.
```

Why bad:

It hides the Upwork-like lifecycle and does not explain how contracts, escrow,
evidence, disputes, reputation, and compliance are orchestrated at runtime.

### Good Example: Phase 19 + Flow 19

Good:

```text
Phase 19 is the baseline migration: XIIGen becomes stack-neutral and moves to a
modern Node.js/TypeScript/NestJS/React web foundation. FLOW-19 is the capability
built on top of that baseline: a CI/CD and DevOps control plane that manages
component catalogs, environments, configs, secrets, GitOps, tests, backups,
disaster recovery drills, and multi-tenant onboarding. The implementation uses
pre-flight numbering from FLOW-18, builds phases with save points and recovery
commands, validates with BFA/DNA/Iron Rules, then merges into the seven
canonical files and updates SESSION_STATE for FLOW-20.
```

Bad:

```text
FLOW-19 updates the stack and adds DevOps factories.
```

Why bad:

It merges the certificate/migration layer with the flow capability layer and
does not explain why stack-neutrality changes future orchestration.

### Good Example: Flow 20

Good:

```text
FLOW-20 gives XIIGen a Graph API and advertising platform. It is not just a
large service bundle: it must protect consent before targeting, run ad auctions
within latency constraints, keep billing append-only, enforce quotas and policy,
and remain compatible with all prior flows. The implementation is orchestrated
through phases and through three internal engines: INVENTORY gathers source
context and patterns, SYNTHESIS generates artifacts, and JUDGMENT checks DNA,
iron rules, BFA, stress tests, and backward compatibility before canonical merge.
```

Bad:

```text
FLOW-20 adds Graph API, advertising services, factories, task types, and BFA.
```

Why bad:

It misses the regulated runtime lifecycle: consent, targeting, auction,
attribution, quota, billing, policy, and backward compatibility.

Bridge example:

```text
Functional need: Graph API + Ads + Measurement + Revenue Integrity + Tenant Governance.
Architecture bridge: six zones divide those needs by responsibility.
Family bridge: each zone becomes coherent factory families, for example Ads
Delivery becomes Targeting Engine plus Auction & Delivery.
Decision bridge: DD-166/167/169/177/179 lock the non-obvious rules: Redis-only
auction, consent before targeting, append-only ledger, fraud before billing,
and configurable attribution windows.
Only after this bridge do task types, factories, BFA, skills, and merge make sense.
```

### Good Example: Flow 21

Good:

```text
FLOW-21 builds a no-code forms and automation platform. The lifecycle starts
with a business user designing and publishing a form, then an end user submits
data, the system normalizes/validates/anti-spam checks/persists the submission,
notifications and integrations fire, automation recipes run, connectors
authenticate through OAuth, and tenant isolation protects everything. The zones
follow that lifecycle: authoring, runtime, submission pipeline, record lifecycle,
notifications, integrations, recipes, connectors, and multi-tenant isolation.
Design decisions then lock the hard rules: schemas in Elasticsearch,
persist-before-emit, anti-spam as fabric gate, recipes as DAGs, and inherited MT
isolation.
```

Bad:

```text
FLOW-21 adds forms, flows, 49 factories, 20 task types, and 12 skills.
```

Why bad:

It does not explain the no-code lifecycle or why the zones/families are the
right decomposition.

### Good Example: Flow 22

Good:

```text
FLOW-22 builds a visual editor and site builder. The orchestration follows the
business user's journey: create pages on a canvas, define content types, author
content, publish through SSG/CDN/cache/search, manage design tokens and SEO,
optimize media, and keep every workspace isolated. The final state renumbers old
input artifacts into F901-F944 / T327-T346, so the explanation uses canonical
numbers while preserving the original product request. Design decisions explain
why: hybrid SSR/SSG keeps previews fresh and published pages fast, optimistic
locking enables teamwork, append-only events protect audit, workspaceId=tenantId
reuses proven isolation, and AI assist stays non-blocking.
```

Bad:

```text
FLOW-22 adds a visual editor, CMS, publishing pipeline, 44 factories, and 20 task types.
```

Why bad:

It names features but does not show the authoring/publishing journey, the
renumbering from input to final state, or the DD bridge.

### Good Example: Flow 23

Good:

```text
FLOW-23 extends FLOW-22. FLOW-22 already gave XIIGen a visual site builder and
CMS foundation; FLOW-23 deepens that into a Webflow/Figma/Wix-like canvas
engine. The user-facing delta is node hierarchy, reusable symbols, layers,
collaboration, undo/redo, deterministic layout, CMS data binding, template
mode, repeaters, stronger tenant isolation, and production code export.
Technically, the P0 plan is the source of truth; old input files are historical
delta patterns and must be renumbered. The explanation therefore starts with
the FLOW-22 -> FLOW-23 change, then maps each capability to storage,
collaboration, layout, binding, tenant isolation, code export, AF review, BFA,
and phased canonical-file updates.
```

Bad:

```text
FLOW-23 adds Visual Editor Extended, 37 factories, 20 task types, and 28 BFA rules.
```

Why bad:

It treats the flow as standalone, hides the extension from FLOW-22, ignores the
P0 plan as source of truth, and skips the old-input to canonical-number bridge.

### Good Example: Flow 24

Good:

```text
FLOW-24 builds a personal AI tutor that lives in the learner's calendar. The
orchestration follows the learning loop: onboarding and consent, baseline
diagnosis, knowledge map, daily micro-lesson, safety gate, calendar sync,
server-side quiz grading, gamification ledger, adaptation of tomorrow's plan,
and recovery for missed or conflicting lessons. The technical design follows
that same loop: identity/consent, diagnosis, curriculum graph/RAG, lesson
composer/safety, quiz/ledger, adaptive planner/calendar connectors, and final
validation. Final merged files are current truth; input files contain the
original request and old numbering that must be translated.
```

Bad:

```text
FLOW-24 adds a learning calendar extension, seven zones, F982-F1027, and T367-T374.
```

Why bad:

It hides the closed learning loop and does not show how consent, safety,
calendar reliability, grading, gamification, and adaptation shape the technical
design.

### Good Example: Miro M1

Good:

```text
We are building a Miro plugin that turns freeform board shapes into inferred
HTML/CSS. Because Miro does not know what a card, button, section, or flex row
is, the main work is a heuristics engine that classifies shapes and infers
layout from position, size, grouping, and text.
```

Bad:

```text
Create shape-classifier.ts and spatial-analyzer.ts.
```

Why bad:

It names files but does not explain the inference problem.

## 8. Rejection Checklist

Reject the orchestration extraction if any are true:

- It starts with numbers, files, classes, or interfaces instead of the human product.
- It can be copied to a different project with almost no change.
- It lists components without explaining the process between them.
- It does not name the main new capability.
- It does not explain what input becomes what output.
- It does not show how work is verified.
- It does not mention current state.
- It ignores input files or produced state.
- It treats old and new numbering as the same without explaining renumbering.
- It says "complete" without validation evidence.
- It ignores the session history when the final orchestration was created by many user corrections.
- It cannot explain which files were input, which files were produced output, and which file is current state.
- It says "I read everything" but cannot name the source role of each major file.
- It fails to state what was deliberately filtered out as low-level technical noise.
- For adapter plans, it does not explain what is reused from the existing engine and what is new for the target platform.
- For related adapter plans, it does not compare why Canva/Webflow/Miro/etc. differ.
- For multi-feature platform flows, it does not separate definition-time orchestration from runtime orchestration.
- For event-driven flows, it does not show at least one concrete event path from user/system action to result.
- For system-wide flow packages, it does not explain the recurring FLOW release pattern.
- It omits save points, recovery commands, canonical merge, or SESSION_STATE update when those are part of the source.
- It reports global counters without explaining what capability the current flow adds.
- For marketplace/financial/contract flows, it does not explain the runtime lifecycle before factories/task IDs.
- It treats idempotency, ledger, escrow, KYC, evidence capture, timers, or dispute compensation as side details instead of orchestration guardrails.
- For migration/certificate packages, it merges baseline migration and new flow capability into one vague story.
- It ignores certificate evidence about stack transition, document updates, or old assumptions removed.
- For DevOps/control-plane flows, it does not explain environments, config/secrets, GitOps, backups, disaster recovery, and multi-tenant operations as one operational lifecycle.
- For regulated Graph/API/advertising flows, it does not explain consent, targeting, auction latency, attribution, quotas, billing ledger, policy, and tenant isolation.
- It collapses INVENTORY/SYNTHESIS/JUDGMENT into "AI generates code".
- It hides freedom/config values as if they were hardcoded implementation details.
- It jumps from functional description directly to factories/task types without explaining zones, families, and design decisions.
- It lists Design Decisions without explaining which business requirement each one answers.
- For no-code/forms/automation flows, it does not explain the user/data lifecycle before zones and families.
- It names zones without explaining why those zones match the lifecycle.
- For visual editor/site builder flows, it does not describe the user's authoring/publishing journey.
- It ignores `No-code explanation` / `What We're Building` sections as the source of the human product story.
- It misses renumbering from old input artifacts to canonical final state.
- It treats historical input/delta files as current truth when a P0/master plan and final state override them.
- It describes an extension flow as if it were standalone instead of explaining the previous-flow baseline and the new delta.
- It mixes old and new IDs without a clear renumbering explanation.
- For learning/tutor/calendar flows, it does not describe the closed teaching loop from diagnosis to adaptation and recovery.
- It says "AI tutor generates lessons" without explaining consent, safety, calendar sync, grading, gamification ledger, and adaptive planning.

## 9. One-Line Rule

```text
Orchestration extraction is not "what parts are in the plan".
It is "how the user's goal becomes a controlled, verified path to a working result".
```
