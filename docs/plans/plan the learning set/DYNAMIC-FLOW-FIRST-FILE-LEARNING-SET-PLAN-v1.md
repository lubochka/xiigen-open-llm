# Dynamic Flow First-File Learning Set Plan v1

## Purpose

This plan defines how to translate the first architecture document into learning
sets for the dynamic flow.

The first document is:

`C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md`

The source diagram is:

`C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\improve algo.drawio.xml`

The creation session is:

`C:\Users\lubao\Documents\xiigen\llm mvp\session_files\Product session 5.txt`

The goal is not to summarize the document. The goal is to preserve every source
line, every source reason, every diagram node, and every diagram edge so a set of
trained flow stations can later recreate the same architecture plan and explain
how they reached it.

## Correction From This Round

The earlier attempt cut the examination too soon. It treated the extracted
`session_files` folder as enough evidence and did not first examine the wider
`C:\Users\lubao\Documents\xiigen` tree.

That is now corrected.

The full tree was searched for:

- live copies of the architecture draft
- live copies of the diagram
- Product and Claude session files
- old, Older, history, sessions, and session_files locations
- archive entries with the architecture draft, the diagram, and Product sessions
- archive content containing unique architecture phrases, even if the file had
  been renamed

## Evidence Inventory

### Files Checked

The full-tree inventory checked:

- 28,427 live files
- 6,318 live directories
- 1,868 archive files
- 119,957 text-like archive entries during the content search

Large text-like archive entries skipped by the content search:

- 27 entries above the search size limit

Archive open failures:

- 0

### Live Target Files Found

Only one live architecture draft was found:

- `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md`
- size: 27,855 bytes
- live hash prefix: `98551861F4FAD3B9`

Only one live diagram file with the requested name was found:

- `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\improve algo.drawio.xml`
- size: 15,427 bytes
- live hash prefix: `21DEC72AAED7DEEF`

### Archive Target Files Found

The architecture draft appears in:

- `C:\Users\lubao\Documents\xiigen\llm mvp.zip`
- entry: `llm mvp/llm dynamic flow/DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md`
- size: 27,855 bytes
- archive hash prefix: `98551861F4FAD3B9`

Conclusion:

The live architecture draft and the archive architecture draft are the same
version by content.

The diagram appears in:

- `C:\Users\lubao\Documents\xiigen\llm mvp.zip`
- entry: `llm mvp/improve algo.drawio.xml`
- size: 14,478 bytes
- archive hash prefix: `EB89B1A29708AC6F`

Conclusion:

The live diagram and the archive diagram are not identical. This is a real
version edge and must be preserved.

### Archive Content Search Result

The archive content search found the architecture draft content only in:

- `C:\Users\lubao\Documents\xiigen\llm mvp.zip`

The archive content search found the creation-session architecture language in:

- `C:\Users\lubao\Documents\xiigen\llm mvp.zip`
- `C:\Users\lubao\Documents\xiigen\llm mvp\session files.zip`

No renamed older copy of the architecture draft was found in the searched
archive content.

## Version Conclusion

The first-file version chain is:

1. Product session 5 creates the architecture need and then creates the draft.
2. The live draft is the current draft file.
3. `llm mvp.zip` contains the same draft content.
4. Product sessions 6, 7, and 8 reference the draft as the current architecture
   target, but do not create a different draft version.
5. The live diagram is newer than the archived diagram. It has the same edge
   count, but three additional text nodes.

Therefore, for this first file, the architecture text has one confirmed content
version and one archive copy. The diagram has two confirmed content versions.

## Source Session Trail

### Product Session 5: Diagram Introduction

Relevant source lines:

- Product session 5 lines 401-418

These lines introduce the user's desired flow:

- a small unit as the basic working unit
- a domain-finding unit
- a unit that translates the user request into domain language
- a unit that breaks the domain request into smaller domain tasks and required
  transformations
- a unit that orchestrates each task
- a unit that applies each task part
- a unit that transforms and aggregates results
- an arbiter possibility before the final user-facing format
- a unit that adapts the result to a user-liked style

These lines feed the diagram node set and the edge set. They are not optional
context.

### Product Session 5: Arbiter Selector Correction

Relevant source lines:

- Product session 5 lines 741-777

These lines establish that the arbiter choice is itself made by a learning unit.
This creates the mirror pattern:

- the front of the flow chooses domains
- the back of the flow chooses arbiters
- both are small learning units
- both improve from feedback
- low confidence creates the need for a new domain or a new arbiter

These lines feed the arbiter selector node, the arbiter pool node, and the
feedback edge back into reprocessing.

### Product Session 5: User Correction and Draft Creation

Relevant source lines:

- Product session 5 lines 1082-1193

These are the creation lines for the architecture document.

Important source facts:

- the user rejects hardcoded behavior
- every unit must support continuous learning
- the first document must remain high-level
- the architecture is not yet a build plan
- the first document must define principles, roles, registries, feedback paths,
  trust hierarchy, and testing philosophy
- the first document must not define schemas, implementation order, bootstrap
  rules, or subproject breakdown
- the user approves creation of the draft
- the assistant reports that the draft was created

These lines feed the architecture boundary node, the learning principle node,
the version history node, and the future-document planning node.

### Product Sessions 6, 7, and 8: Later Consumers

Relevant source lines:

- Product session 6 line 1
- Product session 6 lines 34, 57, 366, 368, 393, 588, 676
- Product session 7 lines 34, 57, 366, 368, 413
- Product session 8 lines 34, 57, 366, 368, 413

These sessions do not create a new architecture draft. They prove that the draft
became the active reference document for later work.

These lines feed the version-history learning set, not the creation learning
set.

## Diagram Version Difference

The live diagram has:

- 27 nodes
- 23 edges

The archive diagram has:

- 24 nodes
- 23 edges

Nodes present only in the live diagram:

- `A`
- `B`
- `These are n different actions that needs to be done in order to reach from A -> B`

No edge exists only in the live diagram.

No edge exists only in the archive diagram.

Conclusion:

The live diagram adds explanatory text without changing the flow edge structure.
The learning set must keep both facts:

- the archive diagram is a prior diagram version
- the live diagram is the current diagram version
- the added text explains the meaning of the multi-action path from A to B
- the edge structure stays stable across both diagram versions

## Binding Diagram Nodes

The learning sets must preserve these diagram nodes. The plain-language names
below are for learning-set readability. The exact diagram labels must also be
kept in the source row.

| Plain-language node name | Exact diagram label |
|---|---|
| Existing unit source | What we build so FAR -> take as LLM unit |
| User request | User prompt |
| Domain splitter | LLM unit to split user prompt to domains d1...dm |
| Domain request translator | LLM Unit -> To Work With user prompt and translate it to domain language |
| Domain scheme planner | LLM Unit to configure scheme plan A -> db1 db2 db3 ... |
| Domain processor | LLM Unit to process db1...dbn |
| Domain applier | LLM unit to apply on fbi(bi)->Rq..i |
| Transformation prompt set | promts b1..bn transformational prompts: fb1...fbn |
| Result language translator | LLM unit to translate Rb1..n -> user language domain |
| User-liked formatter | LLM unit to format translated results into one prompt in user "likable" form |
| Domain orchestration layer | Domain orchestration in db1...dbn |
| Domain processing layer | Domain processing of db1..dbn |
| Domain transformation layer | Domain fb1...fbn |
| Domain result-translation layer | Domain translation Rb1...Rbn |
| Domain user-format layer | Domain translating to user likable format |
| Domain prompt extraction layer | Domain translations of prompts to d1...dm and extrapolating the proper parts of prompt to the domain -> ud1...udm |
| Domain prompt-to-plan layer | Domain of translating udj -> pdj |
| Domain prompt-to-transform layer | Domain of translating udj, di -> fbi , dbi |
| Arbiter pool | Arbitr units |
| Arbiter domain layer | Arbitr domain for b1...bn |
| Arbiter selector | Unit that decides which arbitrs are required |
| Domain list label | d1...dm |
| Arbiter visibility label | sees all R1...Rn |
| Reprocessing label | We may send some of fbx1...fbxn back to reprocessing with apx1...abxn |
| Start marker | A |
| End marker | B |
| Multi-action explanation | These are n different actions that needs to be done in order to reach from A -> B |

## Binding Diagram Edges

The learning sets must preserve these 23 edges. No edge may be removed because
each edge is a teachable transition between units.

1. User request -> Domain splitter
2. Domain splitter -> Domain request translator
3. Domain splitter -> Domain prompt extraction layer
4. Domain request translator -> Domain scheme planner
5. Domain request translator -> Transformation prompt set
6. Domain scheme planner -> Domain processor
7. Domain scheme planner -> Domain orchestration layer
8. Domain processor -> Domain applier
9. Domain processor -> Domain processing layer
10. Domain applier -> Result language translator
11. Domain applier -> Domain transformation layer
12. Domain applier -> Domain applier
13. Transformation prompt set -> Domain prompt-to-plan layer
14. Transformation prompt set -> Domain prompt-to-transform layer
15. Domain prompt-to-plan layer -> Domain prompt-to-transform layer
16. Result language translator -> User-liked formatter
17. Result language translator -> Domain result-translation layer
18. User-liked formatter -> Domain user-format layer
19. Domain result-translation layer -> Arbiter selector
20. Arbiter selector -> Arbiter pool
21. Arbiter pool -> Domain result-translation layer
22. Arbiter pool -> Domain splitter
23. Arbiter pool -> Arbiter domain layer

Special note:

The self-edge on the Domain applier node must not be removed. It represents
repeated application of transformations and reprocessing. If this edge is lost,
the trained flow cannot explain how a partial result returns for another pass.

## Batch Families For The First File

The first file must be translated into these families. Each family produces
learning rows. A row is a complete teaching record, not a short label.

### Family 1: Source Document Lines

One row per line of:

`DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md`

There are 234 source lines.

Rules:

- keep the original line text
- keep blank lines as boundary rows
- keep heading lines as orientation rows
- assign every line to at least one diagram node or to document-boundary
  teaching
- record the incoming edge that makes the line necessary when possible
- record the outgoing edge that the line prepares when possible
- record what a later station would fail to know if this line were missing

### Family 2: Creation Session Lines

One row per line from the relevant creation spans of Product session 5:

- lines 401-418
- lines 741-777
- lines 1082-1193

Rules:

- preserve the original line text
- preserve the speaker context
- mark whether the line is user instruction, assistant interpretation, user
  correction, approval, or created-artifact confirmation
- connect the line to the architecture document line or diagram node it teaches
- do not turn the line into a short summary

### Family 3: Later-Session Reference Lines

One row per later-session reference line from Product sessions 6, 7, and 8.

Rules:

- preserve the original line text
- mark each as later use, not creation
- connect it to the version-history learning set
- record which document line or diagram claim it reused

### Family 4: Diagram Node Rows

One row per diagram node.

Rules:

- keep the exact diagram label
- keep the plain-language node name
- state what this node receives from earlier nodes
- state what this node must produce for later nodes
- list the document lines and session lines that teach this node
- mark nodes with no direct document support as gaps, not as inferred facts

### Family 5: Diagram Edge Rows

One row per diagram edge.

Rules:

- keep source node and target node
- explain the transition in human language
- list the source document or session lines that justify the transition
- if the edge is present in the diagram but weakly explained by the document,
  mark it as a missing teaching row
- never drop the self-edge or the feedback edge

### Family 6: Version Rows

One row per version fact.

Required rows:

- live architecture draft equals archive architecture draft by content
- live diagram differs from archive diagram
- archive diagram has 24 nodes and 23 edges
- live diagram has 27 nodes and 23 edges
- live diagram adds only the three explanatory text nodes
- Product session 5 is the creation source
- Product sessions 6, 7, and 8 are later consumers

### Family 7: Simulation Rows

One row per simulation check.

The simulation rows teach a reviewer how to test whether the learning sets can
recreate the architecture.

## Source Document Line Routing Plan

The actual batch must still contain one row per source line. The ranges below
are not a replacement for line rows. They are the routing rules used when
creating the 234 line rows.

| Source lines | Primary teaching target | Secondary teaching targets |
|---|---|---|
| 1-12 | Document boundary and architecture intent | Version history, future-document boundary |
| 13-21 | Universal continuous learning | Dataset curator, arbiter pool, all unit nodes |
| 23-29 | Dynamic flow topology | Domain scheme planner, domain processor, version rows |
| 31-37 | Self-similar units | Domain splitter, arbiter selector, swappability judge |
| 39-45 | Versioning and rollback | Version rows, registry rows, feedback rows |
| 47-53 | Internal intelligence throughout | Every decision node, arbiter selector, gap detector |
| 55-61 | Provenance | Version rows, dataset rows, feedback rows |
| 63-75 | Testing philosophy | Simulation rows, replay rows, gap rows |
| 79-111 | Cast of unit roles | All diagram node rows and missing-node rows |
| 115-127 | Four registries | Version rows, learning memory rows, feedback rows |
| 131-141 | Feedback paths | Arbiter pool, arbiter selector, reprocessing edge |
| 145-159 | External source trust | Dataset curator, gap detector, missing-unit rows |
| 163-173 | Synchronous and asynchronous training | Feedback rows, user-facing timing rows |
| 177-191 | What comes next | Future-document boundary, version sequence |
| 195-209 | Open items | Human review rows, unresolved decision rows |
| 213-230 | What the document does not decide | Boundary rows, anti-overreach rows |
| 234 | End marker | Document completion row |

Important:

Lines that are blank or separators still get rows. They teach separation of
sections and prevent the model from joining two unrelated ideas into one
station.

## Session Line Routing Plan

The actual session batch must still contain one row per selected session line.
The ranges below are not a replacement for line rows.

| Session lines | Primary teaching target | Why it belongs |
|---|---|---|
| Product session 5, 401-418 | Diagram node origin | User describes the desired flow in human language before the architecture plan exists |
| Product session 5, 741-777 | Arbiter selector and mirror pattern | User resolves the arbiter choice as a learning unit, not a fixed option |
| Product session 5, 1082-1084 | User correction | Establishes no hardcoded behavior and rejects premature implementation detail |
| Product session 5, 1086-1098 | Architecture restatement | Assistant restates the coherent system before writing the document |
| Product session 5, 1102-1123 | First-document scope | Defines what the architecture document must contain |
| Product session 5, 1125-1134 | Future-document order | Separates architecture from later specifications |
| Product session 5, 1137-1139 | Draft-or-iterate choice | Shows the approval gate before artifact creation |
| Product session 5, 1141-1173 | Parallel-session integration | Adds mirror pattern, provenance, testing philosophy, and no-bootstrap correction |
| Product session 5, 1177-1187 | Draft approval and delivered artifact | Confirms the draft was requested and created |
| Product session 5, 1192-1193 | Artifact identity | Names the created document |
| Product session 6 references | Later use | Proves the draft became the active architecture target |
| Product session 7 references | Later use | Proves continued reuse of the same draft |
| Product session 8 references | Later use | Proves continued reuse of the same draft |

## Row Shape For Every Learning Row

Every row must contain these fields in clear human language:

1. Source family
2. Source file
3. Source line number
4. Original source text
5. Speaker, when the source is a session
6. Primary diagram node
7. Secondary diagram nodes
8. Incoming edge, when the line explains one
9. Outgoing edge, when the line prepares one
10. Teaching purpose
11. What this row prevents the flow from forgetting
12. What would go wrong if this row were removed
13. Related architecture document line, when the source is a session line
14. Related session line, when the source is a document line
15. Version role: creation, current version, archive copy, later consumer, or
    diagram revision
16. Simulation check that should fail if this row is missing

The row explanation must not be a rephrase only. It must teach why the source
line belongs to a node or edge.

## Human-Language Rules For The Sets

The learning rows must be readable by a person.

Allowed:

- original source text, even if the source text contains technical words
- plain-language explanation of why a line belongs to a node
- expanded teaching notes that make the logic clearer
- exact file names and line numbers for evidence

Not allowed:

- replacing a source line with a shorter summary
- using a row only to say the same sentence in other words
- assuming a later station already understands hidden architecture logic
- hiding an edge because it is awkward
- merging two different source lines just because they sound similar
- treating later sessions as if they created the draft
- treating the archive diagram as identical to the live diagram

## Simulation Plan

The simulation is how we test whether the learning sets are enough.

### Simulation 1: Document-Only Reconstruction

Input:

- Family 1 rows only

Expected result:

- the architecture plan can be recreated in the right section order
- seven principles appear
- unit roles appear
- registries appear
- feedback paths appear
- trust hierarchy appears
- synchronous and asynchronous training appears
- future documents and open items appear

Expected missing parts:

- why the user rejected hardcoded behavior
- why the document must stay high-level
- why the arbiter selector mirrors the domain splitter
- why the diagram is shaped as a parallel flow

### Simulation 2: Document Plus Creation Session

Input:

- Family 1 rows
- Family 2 rows

Expected result:

- the architecture plan can be recreated
- the user's correction is preserved
- the no-hardcoded-learning rule is preserved
- the no-premature-detail boundary is preserved
- the arbiter selector can be justified from session evidence

Expected missing parts:

- later-session proof that the draft became the active reference
- diagram version difference

### Simulation 3: Add Later Sessions

Input:

- Family 1 rows
- Family 2 rows
- Family 3 rows

Expected result:

- the flow can distinguish creation evidence from later-consumer evidence
- Product session 5 remains the creation source
- Product sessions 6, 7, and 8 are treated as reuse, not new versions

Expected missing parts:

- edge-level reconstruction from the diagram

### Simulation 4: Add Diagram Nodes And Edges

Input:

- Families 1 through 5

Expected result:

- a UML-like flow can be recreated with 27 live nodes and 23 live edges
- the 23 edges are present
- the self-edge is present
- the arbiter feedback edge is present
- the domain splitter and arbiter selector mirror each other
- every architecture section has at least one route into the diagram

Expected missing parts:

- version difference between live and archived diagram

### Simulation 5: Add Version Rows

Input:

- Families 1 through 6

Expected result:

- the architecture text has one confirmed current content version
- the archive architecture text is recognized as the same content
- the diagram has two versions
- the live diagram is recognized as current
- the archive diagram is recognized as prior
- the three added live text nodes are preserved

### Simulation 6: Edge Removal Test

Input:

- all families, but remove one edge row at a time

Expected result:

- removing the self-edge must break the explanation of repeated application
- removing the arbiter feedback edge must break the explanation of reprocessing
- removing the domain splitter edge must break domain-specific routing
- removing the formatter edge must break the final user-liked result

If removing an edge does not visibly damage the simulated output, the training
row for that edge is too weak and must be expanded.

## Acceptance Checklist

The first-file batch is acceptable only when all items below are true.

- All 234 architecture document lines have rows.
- Selected Product session 5 creation lines have rows.
- Product sessions 6, 7, and 8 reference lines have rows.
- Every row keeps original source text.
- Blank lines and separators are accounted for.
- Every live diagram node has a row.
- Every live diagram edge has a row.
- The archive diagram version is represented.
- The live diagram version is represented.
- The three live-only explanatory nodes are represented.
- Product session 5 is marked as the creation session.
- Product sessions 6, 7, and 8 are marked as later consumers.
- No row is only a rephrase.
- No source line is shortened.
- No edge is removed.
- Every missing or weak source support point is marked as a gap.
- The simulation can recreate the architecture plan without hidden assumptions.
- The simulation can recreate the live diagram edge count.
- The simulation can explain what changed between archive diagram and live
  diagram.

## Known Gaps To Preserve

These are not failures of the plan. They are facts the learning set must expose.

1. The architecture draft itself does not describe every diagram edge in equal
   detail. Some edge teaching must come from Product session 5 and the diagram.
2. The diagram includes domain-layer nodes that are stronger visually than they
   are textually explained in the architecture draft.
3. The live diagram has explanatory text nodes that the archive diagram does not
   have.
4. Product sessions 6, 7, and 8 prove later use of the draft, but they do not
   create a second architecture text version.
5. The architecture document's test baseline says 877 in the source context, but
   the current verified test baseline from the loaded session is 938. A learning
   row must preserve the source value and a separate version row must preserve
   the current verified value. Do not silently overwrite one with the other.

## Next Work Batch

The next batch should create these files:

1. `architecture-draft-line-set-v1.md`
   - 234 rows, one per source line
2. `product-session-5-creation-line-set-v1.md`
   - one row per selected Product session 5 line
3. `later-session-reference-line-set-v1.md`
   - Product session 6, 7, and 8 reference rows
4. `diagram-node-set-v1.md`
   - 27 live diagram node rows plus archive-version notes
5. `diagram-edge-set-v1.md`
   - 23 edge rows
6. `first-file-simulation-v1.md`
   - six simulation checks and missing-part report

Those files should be created only after this plan is accepted as the correct
first-file translation method.
