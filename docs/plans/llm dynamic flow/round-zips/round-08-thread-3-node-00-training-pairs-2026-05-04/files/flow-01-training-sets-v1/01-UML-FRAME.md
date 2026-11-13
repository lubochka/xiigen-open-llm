# Flow-01 Training Sets v1 - UML Frame

## Frame Used In This Round

This round covers the new pre-stage only: node 00, the logical structure compiler.

The compiler sits before the topology router and before the nine-node dynamic-flow pipeline. It receives raw user text and emits a checked logical structure that later nodes can route, split, plan, process, review, and format.

## Edge Inventory

### Edge 00 - Raw input to logical structure compiler

What moves from the source node:

Raw text from a human or document source. In this round, the source is a user registration prompt.

What the target node receives:

The exact raw text, not a shortened source-line label and not a description of the document.

What the target node must produce:

A compiled logical structure with:

- source role;
- detected subject;
- local subject;
- twenty-two predicate counts;
- five local domain slots;
- normalized roots;
- meaning graph;
- decomposition count;
- confidence;
- missing-coverage signal.

What breaks if this edge is missing:

The topology router receives raw language instead of checked logical structure. Training data and runtime prompts then use different representations, which breaks the teaching/runtime symmetry proved by E51B.

### Edge 01 - Logical structure compiler to topology and local routing

What moves from the source node:

The compiled logical structure from node 00.

What the target node receives:

The source role, predicate counts, domain slots, roots, meaning graph, confidence, and missing-coverage signal.

What the target node must produce:

The next routing decision: whether the prompt belongs to Flow-01 registration, which local subject is strongest, and whether the request is covered, boundary, blank, or a knowledge edge.

What breaks if this edge is missing:

The local dynamic flow cannot know whether to use provider entry, email verification, duplicate prevention, token safety, or knowledge-edge handling. Later nodes may produce an answer without knowing whether the prompt was covered.

## Boundary Of This Round

No downstream edge is trained here. The simulation check describes what downstream nodes would probably do if trained only from this node-00 file, and names what remains missing.
