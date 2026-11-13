# Node-01 - Domain Splitter Set

Runtime A is a human prompt. Runtime B is an array of domain labels.

## Pair 1.1 - Runtime Split

A: "Use this document to understand <purpose and keep its main responsibilities visible."

B: ["training input","expected output","example coverage","rephrasing coverage"]

Pair 1.1 - Rephrased

A: "Apply this document to understand <purpose and keep its main responsibilities visible."

B: ["training input","expected output","example coverage","rephrasing coverage"]

## Pair 1.2 - Runtime Split

A: "Explain how <purpose should guide a human request without losing decisions, risks, or expected results."

B: ["expected output","example coverage","node responsibility","gap marker"]

Pair 1.2 - Rephrased

A: "Show how <purpose should guide a human request without losing decisions, risks, or expected results."

B: ["expected output","example coverage","node responsibility","gap marker"]

## Pair 1.3 - Runtime Split

A: "Plan the work described by <purpose as separated responsibilities that can move through the dynamic flow."

B: ["example coverage","rephrasing coverage","quality signal","curation decision"]

Pair 1.3 - Rephrased

A: "Prepare the work described by <purpose as separated responsibilities that can move through the dynamic flow."

B: ["example coverage","rephrasing coverage","quality signal","curation decision"]

## Pair 1.4 - Runtime Split

A: "Find the review points, exceptions, and recovery needs in <purpose."

B: ["node responsibility","quality signal","context7 mcp first - current docs, no hallucination","official docs - when context7 lacks coverage"]

Pair 1.4 - Rephrased

A: "Identify the review points, exceptions, and recovery needs in <purpose."

B: ["node responsibility","quality signal","context7 mcp first - current docs, no hallucination","official docs - when context7 lacks coverage"]

## Pair 1.5 - Runtime Split

A: "Turn <purpose into learning examples for the node pipeline."

B: ["training input","gap marker","websearch last - for comparisons and trends only","if verified: return to plan-phase.md with confirmation. no discovery.md needed."]

Pair 1.5 - Rephrased

A: "Translate <purpose into learning examples for the node pipeline."

B: ["training input","gap marker","websearch last - for comparisons and trends only","if verified: return to plan-phase.md with confirmation. no discovery.md needed."]

## Pair 1.6 - Runtime Split

A: "Compare the source meaning of <purpose with related versions or session evidence."

B: ["training input","context7 mcp first - current docs, no hallucination","websearch last - for comparisons and trends only","if concerns found: escalate to level 2."]

Pair 1.6 - Rephrased

A: "Contrast the source meaning of <purpose with related versions or session evidence."

B: ["training input","context7 mcp first - current docs, no hallucination","websearch last - for comparisons and trends only","if concerns found: escalate to level 2."]

## Pair 1.7 - Runtime Split

A: "Prepare a user-facing answer from approved results about <purpose."

B: ["gap marker","curation decision","if verified: return to plan-phase.md with confirmation. no discovery.md needed.","identify what to discover:"]

Pair 1.7 - Rephrased

A: "Prepare a user-facing answer from approved results about <purpose."

B: ["gap marker","curation decision","if verified: return to plan-phase.md with confirmation. no discovery.md needed.","identify what to discover:"]

## Pair 1.8 - Runtime Split

A: "Check whether <purpose has enough evidence to be trusted by a reviewer."

B: ["node responsibility","quality signal","official docs - when context7 lacks coverage","websearch last - for comparisons and trends only"]

Pair 1.8 - Rephrased

A: "Check whether <purpose has enough evidence to be trusted by a reviewer."

B: ["node responsibility","quality signal","official docs - when context7 lacks coverage","websearch last - for comparisons and trends only"]

## Pair 1.9 - Runtime Split

A: "Show what would break if one responsibility in <purpose were skipped."

B: ["example coverage","rephrasing coverage","quality signal","context7 mcp first - current docs, no hallucination"]

Pair 1.9 - Rephrased

A: "Show what would break if one responsibility in <purpose were skipped."

B: ["example coverage","rephrasing coverage","quality signal","context7 mcp first - current docs, no hallucination"]

## Pair 1.10 - Runtime Split

A: "Build a simulation that proves the learning and teaching material content can pass through all nine nodes."

B: ["training input","rephrasing coverage","gap marker","if verified: return to plan-phase.md with confirmation. no discovery.md needed."]

Pair 1.10 - Rephrased

A: "Build a simulation that proves the learning and teaching material content can pass through all nine nodes."

B: ["training input","rephrasing coverage","gap marker","if verified: return to plan-phase.md with confirmation. no discovery.md needed."]

