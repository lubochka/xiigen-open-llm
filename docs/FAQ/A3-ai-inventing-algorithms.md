# Can LlmFromScratch invent new algorithms (AlphaEvolve-style)?

No. LlmFromScratch does not implement an AlphaEvolve-style evolutionary generate-test-select loop — there is no mutation, population, fitness, or candidate-generation step anywhere. What exists instead is a fixed harness of human-written experiments plus deterministic Roslyn compiler optimizations (constant folding, common-subexpression elimination), which are compiler passes, not algorithm invention.

LlmFromScratch does NOT implement an AlphaEvolve-style evolutionary generate-test-select loop. `35.ExperimentsV2/ExperimentRunner.cs:8` is a fixed harness of human-written experiments (E01..E52, baseline-vs-enhanced plus a tier ladder), driven by a `switch`-based dispatch — there is no mutation, population, fitness, or candidate-generation step. Inside that harness, `Run` (line 20), `RunTierLadder` (line 59), `RunAll` (line 107, the fixed E01..E52 list) and `Dispatch` (line 156, a switch table) never loop back on their own output to breed new candidates. A grep for `evolv|mutation|fitness|population` over `35.ExperimentsV2` returns a single match, and it is prose, not code — the word `evolve` inside a sample question string at `35.ExperimentsV2/Infrastructure/EpisodeCorpusLoader.cs:63`. The closest adjacent capability is deterministic code optimization in `13.CodeOptimizer`: `13.CodeOptimizer/Pipeline/OptimizationPipeline.cs:34` uses `CSharpSyntaxTree.ParseText` (Roslyn AST) to drive constant-folding and common-subexpression-elimination passes — compiler optimizations, not algorithm invention. Honesty boundary: this specific trend is not implemented here; presenting either the harness or the Roslyn optimizer as "AI that invents algorithms" would be an overclaim.

### Where is this in the code?

The experiment "orchestration" is the fixed dispatch inside `ExperimentRunner` (`RunAll` -> `Dispatch` switch); the optimizer runs a fixed sequence of Roslyn passes in `OptimizationPipeline`. Neither loops back on its own output to breed new candidates.

### What is the industry trend?

A high-profile 2025-2026 direction pairs an LLM with an evolutionary loop (generate -> test -> select -> mutate) so the system discovers genuinely new algorithms, as in AlphaEvolve.

### Honesty boundary

This specific trend is NOT implemented here: there is no self-improving evolutionary algorithm-invention loop. What exists is a deterministic experiment harness plus deterministic Roslyn compiler optimizations. Presenting either as "AI that invents algorithms" (AlphaEvolve) would be an overclaim.

Last updated: 2026-07-24
