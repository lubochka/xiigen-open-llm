# Subject Expertise Grouping Assumption v1

## Purpose

This document proposes how to divide the generated training sets into subject
and expertise groups.

The split is an assumption, not a final truth. It is meant to give the learning
pipeline a practical first structure: each group teaches a different kind of
thinking, and each group can still feed all nine UML nodes.

## Source Used

The grouping was derived from:

- `all-files-and-zip-entries-manifest-with-nested.csv`
- `all-files-compact-status.csv`
- folder names, archive paths, file extensions, readable kind, category, and first readable text sample

Total source records classified:

- physical files: 69,614
- top-level zip entries: 242,630
- nested zip entries: 117,781
- total source records: 430,025

## Important Assumption

Do not divide only by file type.

The same subject can appear as:

- a markdown plan
- a session file
- a zipped source file
- a generated implementation file
- a test fixture
- an old version

The first useful division is therefore by expertise field, then by source role.

## Proposed Expertise Groups

| Group | Expertise field | Records | Main teaching purpose |
|---|---:|---:|---|
| G01 | Session, History, And Version Evidence | 280,727 | Teach the system to understand how documents were created, changed, resumed, corrected, and reused. |
| G07 | Design, Figma, Visual Editing, And UI Understanding | 72,712 | Teach screen/design interpretation, component meaning, layout intent, and design-to-function reasoning. |
| G02 | Flow Planning And Orchestration | 29,936 | Teach triggers, nodes, branches, parallel work, waits, recovery, review, and final merge. |
| G03 | Architecture, Fabric, Factory, And Skill Rules | 25,256 | Teach engine rules, skill boundaries, factory responsibilities, fabric selection, and allowed extension paths. |
| G04 | Learning, Teaching, Prompt, And Model Improvement | 8,860 | Teach how learning examples, prompts, judging, rephrasing, and improvement loops should work. |
| G06 | Business Domains And Product Modules | 5,250 | Teach product/business subject matter such as shops, finance, social, blog, events, forms, ERP, and marketplaces. |
| G09 | Backend, APIs, Services, And Providers | 3,228 | Teach service boundaries, providers, server responsibilities, and backend behavior. |
| G05 | Retrieval, Graph Knowledge, And Context Assembly | 2,337 | Teach source indexing, context selection, graph relations, retrieval, and evidence assembly. |
| G08 | Frontend, Mobile, Web, And Client Rendering | 843 | Teach client rendering, UI runtime, frontend frameworks, and client-side behavior. |
| G12 | Testing, Validation, Simulation, And Quality Gates | 351 | Teach how to prove behavior, simulate flows, validate coverage, and block unsafe results. |
| G14 | WordPress, Plugin, Theme, And CMS Integration | 262 | Teach WordPress/plugin/theme/CMS-specific integration and packaging. |
| G18 | Archive Containers And Zip Boundary Handling | 117 | Teach archive identity, nested zip boundaries, and correct source provenance. |
| G19 | General Project Knowledge And Uncategorized Files | 71 | Hold low-signal or uncategorized project files until they can be reclassified. |
| G10 | Security, Authorization, Identity, And Tenancy | 42 | Teach identity, tenant boundary, privacy, permission, and isolation reasoning. |
| G11 | Data, Database, Search, Cache, And Storage | 17 | Teach storage, schema, indexing, cache, and data lifecycle reasoning. |
| G13 | DevOps, CI-CD, Environment, And Deployment | 12 | Teach deployment, environment, CI/CD, provisioning, backup, and operational recovery. |
| G15 | Migration, Translation, And Multi-Stack Alternatives | 4 | Teach cross-stack translation and target-platform alternatives. |

The counts above are first-pass classifier counts. Some records should later
receive secondary labels. For example, a session file about Figma belongs first
to G01 as history evidence, but secondarily to G07 as design knowledge.

## Recommended Two-Level Split

Use two labels for every source record:

1. **Expertise group**: what kind of knowledge this source teaches.
2. **Source role**: what kind of evidence this file is.

Recommended source roles:

- final source document
- old version
- session or history evidence
- generated implementation
- test or validation fixture
- archive container
- archive entry
- binary or visual asset
- dependency or tooling copy
- generated training output

This prevents a common mistake: mixing the subject of a file with the role of a
file. A session about architecture is not the architecture document itself. It is
history evidence about architecture.

## How Each Group Should Feed The Nine Nodes

### G01 - Session, History, And Version Evidence

Best for:

- Node 1: detect that a user is asking about creation history, changes, resume state, or source evidence.
- Node 2: translate session fragments into clear evidence statements.
- Node 3: plan a version-history reconstruction.
- Node 7 and Node 8: decide whether creation evidence is strong enough or only candidate evidence.

Do not use G01 as final product truth unless the session is the selected source
document for that round.

### G02 - Flow Planning And Orchestration

Best for:

- Node 1: detect triggers, branches, flows, waits, recovery, and merge points.
- Node 3: produce correct parallel and ordered scheme plans.
- Node 4 and Node 5: keep work pieces separate.
- Node 7 and Node 8: review missing branches, wrong order, and lost recovery paths.

This is one of the core groups for dynamic-flow behavior.

### G03 - Architecture, Fabric, Factory, And Skill Rules

Best for:

- Node 2: translate human requests into strict architecture language.
- Node 3: plan work without breaking factory/fabric boundaries.
- Node 7 and Node 8: select architecture-rule reviewers and block violations.

This group teaches what the system is allowed to do.

### G04 - Learning, Teaching, Prompt, And Model Improvement

Best for:

- Node 1: recognize learning-set, prompt, judging, and improvement requests.
- Node 2: translate vague learning goals into exact teaching responsibilities.
- Node 9: format learning material for human review.

This group should become the main teacher for making better training pairs.

### G05 - Retrieval, Graph Knowledge, And Context Assembly

Best for:

- Node 1: detect retrieval, source, graph, evidence, and context needs.
- Node 2: translate user needs into retrieval-context language.
- Node 3: plan source selection, graph expansion, and evidence assembly.
- Node 7 and Node 8: review missing evidence or unsupported claims.

This group should not answer from memory; it teaches how to gather evidence.

### G06 - Business Domains And Product Modules

Best for:

- Node 1: identify product domains from human language.
- Node 2: translate business requests into domain-specific language.
- Node 6 and Node 9: explain technical/business results to people.

This group should be subdivided by product domain:

- identity and onboarding
- shops and marketplaces
- social network and feed
- finance and ERP
- events and calendar
- forms and workflow
- blog and CMS
- sponsored content and graph access
- learning and gamification

### G07 - Design, Figma, Visual Editing, And UI Understanding

Best for:

- Node 1: detect screen, component, design, layout, or visual-editor intent.
- Node 2: translate design fragments into functional meaning.
- Node 3: plan design-to-function interpretation.
- Node 7 and Node 8: review unsupported visual assumptions.

This group should be one of the biggest specialized learning tracks.

### G08 - Frontend, Mobile, Web, And Client Rendering

Best for:

- Node 2: translate UI/client requests into client-runtime responsibilities.
- Node 4 and Node 5: process and apply client behavior.
- Node 9: format client-facing implementation plans.

Keep this separate from G07. G07 understands what a design means; G08 understands
how a client surface behaves.

### G09 - Backend, APIs, Services, And Providers

Best for:

- Node 2: translate service/API/provider requests into backend responsibilities.
- Node 3: plan service boundaries.
- Node 4 and Node 5: process backend work pieces.

Keep this separate from architecture rules. G03 says what is allowed; G09 says
how backend responsibilities behave.

### G10 - Security, Authorization, Identity, And Tenancy

Best for:

- Node 1: detect identity, permission, privacy, tenant, and isolation concerns.
- Node 7 and Node 8: select and run safety/privacy/tenant-boundary arbiters.

Even though its first-pass count is small, this group should have high priority
because it protects all other groups.

### G11 - Data, Database, Search, Cache, And Storage

Best for:

- Node 2: translate data needs into storage/search/cache responsibilities.
- Node 3: plan data movement and consistency.
- Node 7 and Node 8: review missing records, wrong index use, or unsafe data assumptions.

This group should later absorb more records through secondary labeling because
many architecture and business files contain data rules.

### G12 - Testing, Validation, Simulation, And Quality Gates

Best for:

- Node 3: plan checks and simulation paths.
- Node 7 and Node 8: decide which review units are required.
- Node 10 simulation files: prove a training package works.

This group teaches how the system knows it is correct.

### G13 - DevOps, CI-CD, Environment, And Deployment

Best for:

- Node 3: plan build, environment, deployment, backup, and recovery work.
- Node 7 and Node 8: review release safety and operational readiness.

The count is small in first-pass classification, but many files in tooling/build
may be secondarily assigned here.

### G14 - WordPress, Plugin, Theme, And CMS Integration

Best for:

- Node 1: detect WordPress/CMS/plugin/theme requests.
- Node 2: translate to CMS-specific responsibilities.
- Node 5: apply packaging and integration outcomes.

Keep this as its own applied-platform group because WordPress has special
packaging and runtime conventions.

### G15 - Migration, Translation, And Multi-Stack Alternatives

Best for:

- Node 1: detect target-stack translation requests.
- Node 2: translate canonical behavior into target-platform responsibilities.
- Node 3: plan parallel target branches.

This group should grow through secondary labels from React, Vue, Angular,
WordPress, Node, Java, Go, Rust, PHP, Python, and .NET files.

### G16 - External Tooling, Build Output, And Dependencies

This group is mostly curation material, not primary domain truth.

Use it to teach:

- whether a file is source or dependency
- whether it should be trusted as project knowledge
- whether it should be excluded from domain truth

### G17 - Binary Assets, Images, Documents, And Theme Media

This group should teach source handling and provenance, not factual claims.

For binary files, the training pair should say what can be known from path,
extension, size, archive context, and metadata unless a separate extraction step
actually reads the content.

### G18 - Archive Containers And Zip Boundary Handling

Best for:

- preserving archive path and entry path
- distinguishing a zip container from the files inside it
- preventing nested zip contents from losing provenance

This is not a product subject. It is a source-boundary expertise group.

### G19 - General Project Knowledge And Uncategorized Files

This is a temporary holding group. It should shrink over time as better
secondary labels are added.

## Recommended Batch Structure

For each expertise group, create:

- `group-index.md`
- `source-records.csv`
- `node-01-domain-splitter-set.jsonl`
- `node-02-domain-translator-set.jsonl`
- `node-03-scheme-planner-set.jsonl`
- `node-04-domain-processor-set.jsonl`
- `node-05-applier-set.jsonl`
- `node-06-result-translator-set.jsonl`
- `node-07-arbiter-selector-set.jsonl`
- `node-08-arbiter-units-set.jsonl`
- `node-09-formatter-set.jsonl`
- `simulation-checks.jsonl`

Each group should still cover all nine nodes. The split is by expertise, not by
node.

## Recommended Processing Order

1. G03 Architecture, Fabric, Factory, And Skill Rules
2. G02 Flow Planning And Orchestration
3. G04 Learning, Teaching, Prompt, And Model Improvement
4. G12 Testing, Validation, Simulation, And Quality Gates
5. G05 Retrieval, Graph Knowledge, And Context Assembly
6. G10 Security, Authorization, Identity, And Tenancy
7. G06 Business Domains And Product Modules
8. G07 Design, Figma, Visual Editing, And UI Understanding
9. G09 Backend, APIs, Services, And Providers
10. G08 Frontend, Mobile, Web, And Client Rendering
11. G11 Data, Database, Search, Cache, And Storage
12. G13 DevOps, CI-CD, Environment, And Deployment
13. G14 WordPress, Plugin, Theme, And CMS Integration
14. G15 Migration, Translation, And Multi-Stack Alternatives
15. G01 Session, History, And Version Evidence
16. G18 Archive Containers And Zip Boundary Handling
17. G16 External Tooling, Build Output, And Dependencies
18. G17 Binary Assets, Images, Documents, And Theme Media
19. G19 General Project Knowledge And Uncategorized Files

The reason G01 is not first, even though it is largest, is that sessions need the
architecture and flow vocabulary to be interpreted correctly.

## What Not To Mix

- Do not mix final source documents with session evidence unless the session is
  the selected source for that round.
- Do not mix binary assets with extracted text as if they have the same evidence.
- Do not mix design understanding with frontend implementation.
- Do not mix architecture permission rules with backend behavior.
- Do not mix archive containers with archive entries.
- Do not use dependency folders as product truth unless the task is explicitly
  about dependencies.

## Next Improvement

The next useful step is multi-label classification.

The first pass assigns one primary group. The better version should assign:

- primary expertise group
- secondary expertise group
- source role
- confidence
- reason

That would let a session file about Figma be both:

- primary: Session, History, And Version Evidence
- secondary: Design, Figma, Visual Editing, And UI Understanding

and a zipped backend service file be both:

- primary: Backend, APIs, Services, And Providers
- source role: archive entry

