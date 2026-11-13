# 01 - UML Frame

## Active Nodes

1. Domain splitter: user prompt -> array of domain labels.
2. Domain translator array: one domain fragment -> one formal domain statement.
3. Scheme planner: all formal statements -> ordered and parallel work pieces.
4. Domain processor array: one work piece -> complete processed result.
5. Applier array: one processed piece -> concrete outcome.
6. Result translator array: one formal result -> readable result.
7. Arbiter selector: formal result collection -> required reviewers.
8. Arbiter units: review responsibility -> pass, concern, or reprocess.
9. Formatter: approved translated results -> one finished answer.

## Edges

User prompt moves to the splitter as ordinary human language. The splitter emits labels. Each label and fragment moves to one translator. All translated statements move to the planner. The planner emits separate work pieces. Work pieces move in parallel through processors and appliers. Formal results move both to result translators and to the arbiter selector. Arbiters review before formatting and can send failed fragments back to the front of the flow. Only approved translated results reach the formatter.

## Edge Integrity Rule

No package may train a node as if it receives source-document lines at runtime. The source document is evidence for creating runtime-shaped A-to-B pairs; it is not the runtime input itself.

## Source Topic

FLOW-06: USER GROUPS & COMMUNITIES — REFERENCE PLAN v3
