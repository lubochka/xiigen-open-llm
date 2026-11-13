# A3 — AI inventing algorithms (AlphaEvolve / DeepSeek moment)

**Trend.** A high-profile 2025-2026 direction pairs an LLM with an evolutionary loop (generate -> test -> select -> mutate) so the system discovers genuinely new algorithms, as in AlphaEvolve.

**How this project relates — explicit scope-down.** LlmFromScratch does NOT implement an AlphaEvolve-style evolutionary generate-test-select loop. `35.ExperimentsV2/ExperimentRunner.cs` is a fixed harness of human-written experiments (E01..E52, baseline-vs-enhanced plus a tier ladder), driven by a `switch`-based dispatch — there is no mutation, population, fitness, or candidate-generation step. The closest adjacent capability is deterministic code optimization in `13.CodeOptimizer`: Roslyn-based passes such as constant folding and common-subexpression elimination — compiler optimizations, not algorithm invention.

**Where in the code.** `35.ExperimentsV2/ExperimentRunner.cs:8` — `Run` (line 20), `RunTierLadder` (line 59), `RunAll` (line 107, the fixed E01..E52 list), `Dispatch` (line 156, a switch table); a grep for `evolv|mutation|fitness|population` over `35.ExperimentsV2` returns a single match, and it is prose, not code — the word `evolve` inside a sample question string at `35.ExperimentsV2/Infrastructure/EpisodeCorpusLoader.cs:63`. There is no mutation, population, fitness, or candidate-generation construct anywhere in the harness. `13.CodeOptimizer/Pipeline/OptimizationPipeline.cs:34` — `CSharpSyntaxTree.ParseText` (Roslyn AST) driving deterministic constant-folding / common-subexpression-elimination passes.

**Orchestration.** The experiment "orchestration" is the fixed dispatch inside `ExperimentRunner` (`RunAll` -> `Dispatch` switch); the optimizer runs a fixed sequence of Roslyn passes in `OptimizationPipeline`. Neither loops back on its own output to breed new candidates.

**Honesty boundary.** This specific trend is NOT implemented here: there is no self-improving evolutionary algorithm-invention loop. What exists is a deterministic experiment harness plus deterministic Roslyn compiler optimizations. Presenting either as "AI that invents algorithms" (AlphaEvolve) would be an overclaim.
