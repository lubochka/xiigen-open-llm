# LlmFromScratch

> An LLM and full ML stack built from scratch in pure C# (.NET 9) — no PyTorch,
> no TensorFlow, no ML.NET training. Every algorithm is implemented, tested, and
> benchmarked on its own.

![License: AGPL v3](https://img.shields.io/badge/License-AGPL_v3-blue.svg)
![Language: C#](https://img.shields.io/badge/language-C%23-512BD4.svg)
![.NET 9](https://img.shields.io/badge/.NET-9-512BD4.svg)
![Built from scratch](https://img.shields.io/badge/ML-from--scratch-success.svg)
![No PyTorch](https://img.shields.io/badge/no-PyTorch-critical.svg)

<!-- demo.gif: a short screen capture of a from-scratch run (tokenizer -> model
     -> inference) goes here as docs/assets/demo.gif -->

## What is LlmFromScratch?

LlmFromScratch is an LLM and full machine-learning stack built from scratch in
pure C# (.NET 9) — a study and reference stack, not a finished product. It
implements its own tensor engine, tokenizer, transformer, training loop, LoRA,
and DPO trainer, with no PyTorch, TensorFlow, or ML.NET training dependency.

LlmFromScratch organizes its code as 47 numbered subproject directories
(`00.*`–`44.*`) in the repository root, each one a single layer you can read
from top to bottom. Its foundation is a hand-written tensor engine with SIMD and
autograd (`00.TensorCore`); the `00.TensorCore` and `03.TransformerModel` project
files declare zero `PackageReference` entries, so the tensor math and the
transformer depend on no PyTorch, TensorFlow, or ML.NET training package. Several
components are trained for real — the DPO trainer (`11.DPO/DpoTrainer.cs`), the
LoRA applicator (`10.LoRA/LoraApplicator.cs`), the training loop
(`04.TrainingLoop/Trainer.cs`), and the RU↔EN translation model
(`27.TranslationDomain/TranslationMachine.cs`) — with a real checkpoint lifecycle
recorded under `models/` (`models/index.json`). One command, `dotnet test`,
builds every subproject and runs the entire xUnit test suite with no network
access or model downloads. The project is released under `AGPL-3.0-only` (see
`LICENSE`), and is also referred to as xiigen-open-llm, the trainable core of the
XIIGen ecosystem.

## Quick start

    dotnet test        # build the whole solution and run the test suite

That single command compiles every subproject and runs the xUnit suite. No
network access, no model downloads, and no external ML runtime are required — the
tensor math, the tokenizer, the model, and the training code are all in this
repository.

## Project status

This is a study and reference stack under active development, not a finished
product, and the test suite does not currently pass in full. Run `dotnet test`
and judge the results yourself rather than assuming a green suite: the codebase
and its training data are large, and defects can be carried across during
changes that touch many files at once. Some of the current failures are
environmental rather than defects in the code — a test that compares a recorded
SHA-256 against a file read as raw bytes will disagree on a checkout whose line
endings were rewritten, while the data itself is intact; the per-file detail is
in `training-data/DATA-STATUS.md`. Treat a failing test as something to
investigate rather than as a verdict on the whole project.

## Why does this project exist?

This is a learning-first, dependency-light codebase. It contains a BPE
tokenizer, a transformer, a training loop, a LoRA adapter, and a from-scratch DPO
trainer, alongside search, graph, and a small orchestration and agent layer — all
written as readable C# that you can step through end to end.

It is a study and reference stack, not a finished product. Some parts are trained
and exercised for real; others are honest skeletons that establish an interface
and a shape without a working model behind them. The project labels each
component for what it is rather than for what it aspires to be: parts that only
sketch an interface are called skeletons, and quality gates that run a fixed set
of deterministic checks are described as gates, not as adaptive intelligence. If
you want to know exactly which is which before you read the code, the FAQ is the
fastest way to calibrate your expectations.

## Documentation

- Architecture and subprojects: `docs/`
- Trends FAQ (how this maps to current AI trends): [`docs/FAQ/`](docs/FAQ/README.md) (15 topics)

One page per topic — pick the question you came with:

**Model and training capabilities**

- [Do the agents in LlmFromScratch check their own output?](docs/FAQ/A1-self-checking-agents.md)
- [How does LlmFromScratch handle continual learning and catastrophic forgetting?](docs/FAQ/A2-hybrid-architectures-continual-learning.md)
- [Can LlmFromScratch invent new algorithms (AlphaEvolve-style)?](docs/FAQ/A3-ai-inventing-algorithms.md)
- [Which new training methods does LlmFromScratch implement?](docs/FAQ/A4-new-training-methods.md)
- [How does LlmFromScratch compare to narrow-domain systems like AlphaGeometry?](docs/FAQ/A5-progress-in-narrow-domains.md)

**Developer workflow and agentic tooling**

- [Is LlmFromScratch a multi-agent system?](docs/FAQ/B1-agentic-ai-multi-agent.md)
- [How does LlmFromScratch reflect the shift from coding to orchestration?](docs/FAQ/B2-developer-role-shift.md)
- [How was LlmFromScratch built with agentic coding tools?](docs/FAQ/B3-tooling-race.md)
- [What does large-scale AI adoption mean for LlmFromScratch?](docs/FAQ/B4-adoption-scale.md)
- [Does LlmFromScratch support vibe coding and natural-language intent?](docs/FAQ/B5-vibe-coding.md)

**Local-first and ecosystem**

- [Does LlmFromScratch run fully on-device, without the cloud?](docs/FAQ/C1-on-device-no-cloud.md)
- [Is LlmFromScratch a local LLM like llama.cpp or Ollama?](docs/FAQ/C2-local-llms.md)
- [How does LlmFromScratch use CLAUDE.md-style behavioral rules?](docs/FAQ/C3-claude-md-behavioral-rules.md)
- [Is LlmFromScratch an agentic framework like CrewAI or AutoGen?](docs/FAQ/C4-agentic-frameworks.md)
- [How does LlmFromScratch relate to MCP servers and AI gateways?](docs/FAQ/C5-better-ai-apps-mcp-gateways.md)

Alongside the 15 topics, and not counted among them, there is a licensing note:
[Why is LlmFromScratch licensed under AGPL-3.0?](docs/FAQ/why-agpl.md).

## Where should I start reading?

New to the codebase? Read these five, in order:

1. `docs/ARCHITECTURE.md` — what this project is and how the layers fit together.
2. `docs/SUBPROJECT-MAP.md` — which of the numbered subprojects are real and which are skeletons, with status.
3. `docs/COMPONENT-INTERFACES.md` — the interface contracts each layer implements.
4. `01.Tokenizer/` — a small, fully implemented subproject (a BPE tokenizer) with tests in `Tests/`, to read the code style end to end.
5. `docs/FAQ/` — how the stack maps to current AI trends, and what is deliberately out of scope.

## What is in the repository?

Where each capability stands today (statuses are literal — skeletons are labelled
skeletons, gates are labelled gates):

| Component | Status | Evidence |
|---|---|---|
| BPE tokenizer | Implemented and tested | `01.Tokenizer/BpeTokenizer.cs` |
| Transformer + tensor engine (SIMD, autograd) | Implemented and tested | `00.TensorCore`, `03.TransformerModel` |
| Training loop | Trained for real | `04.TrainingLoop/Trainer.cs` |
| LoRA fine-tuning | Trained for real | `10.LoRA/LoraApplicator.cs` |
| DPO training | Trained for real | `11.DPO/DpoTrainer.cs` |
| RU↔EN translation model | Trained for real, checkpoint lifecycle | `27.TranslationDomain/TranslationMachine.cs`, `models/` |
| Quality gate | Deterministic gate (2 of 16 named checks, no retry) | `15.OrchestratorModel/QualityOrchestrator.cs` |
| PromptToTaskMachine, OrchestrationReasoningMachine | Skeletons (not trained) | `43.SmartSkillsDomain/` |

Subprojects are numbered by layer (`00.*`–`44.*`). Grouped by theme, with a
representative example from each group rather than the full list:

- Core & model — e.g. `00.TensorCore`, `01.Tokenizer`, `03.TransformerModel`, `05.Embeddings`, `09.Inference`.
- Training — e.g. `04.TrainingLoop`, `10.LoRA`, `11.DPO`, `14.ContinuousLearning`, `26.TrainableRouter`.
- Search & graph — e.g. `06.VectorSearch`, `07.GraphEngine`, `08.GraphRag`, `12.SearchEngine`.
- Semantic & compiler — e.g. `13.CodeOptimizer`, `28.SemanticNlp`, `29.SemanticCompiler`.
- Orchestration & agents — e.g. `15.OrchestratorModel`, `21.AgentInfrastructure`, `22.AgentAlgorithms`, `25.ModelServer`.
- Domains — e.g. `17.DynamicDomains`, `27.TranslationDomain`, `42.ProjectWikiDomain`, `44.DesignConversionDomain`.

Plus `docs/` (architecture, maps, and the trends FAQ), `Tests/` (the xUnit test
suite), and `tools/` (developer utilities). `AGENTS.md` and `CLAUDE.md` describe
the project's internal development methodology (a session-driven, agent-assisted
way of working) and are not the product manual — start at `README.md`.

## Contributing

See `CONTRIBUTING.md` and the issues labelled `good first issue`. Because the code
is meant to be read, contributions that improve clarity, tests, and documentation
are as welcome as new algorithms.

## Related projects

**xiigen-open-llm** (this repository) is the trainable core of the XIIGen
ecosystem: from-scratch models, DPO and LoRA training, and a small orchestration
layer, in pure C#.

**XIIGen (xiigen-mvp)** is the sibling application layer of the same ecosystem. It
collects and cleans the training signal and consumes the finished manifests
produced here; it does not train model weights itself.

Both projects are released under the same license (`AGPL-3.0-only`). A link to the
public xiigen-mvp repository will be added here once it is published; until then
this is a described relationship, not a link.

## License

The source code in this repository is licensed under the GNU Affero General
Public License v3.0 only (AGPL-3.0-only).

The AGPL license applies to the software code in this repository. It does not
grant any rights to third-party model outputs, third-party datasets,
proprietary prompts, private customer data, or materials governed by separate
terms.

The full text is in `LICENSE`, which also carries an additional-terms note
under section 7(b): keep the attribution to the original project intact in
copies and derivatives. Copyright and third-party attributions live in
`NOTICE`, and the inventory of third-party components ships in
`THIRD-PARTY-NOTICES.md`.

## Training Data Notice

This repository may include experimental DPO JSON files and training examples.

Some DPO examples may have been generated, drafted, reviewed, or assisted by
AI systems, including Codex and other third-party models. These files are
provided for transparency, research, review, reproducibility, and local
experimentation only.

Before using any DPO JSON file for training, fine-tuning, distillation,
benchmarking, redistribution, or commercial use, you are responsible for
verifying that you have all required rights and that your use complies with:

- the license of the source material;
- the terms of any AI service or model used to generate or assist the data;
- applicable law;
- your organization's policies.

Do not use any DPO file for model training if the applicable third-party
model/service terms prohibit that use.

## External Model Output Warning

Some AI providers restrict the use of their outputs for training models,
especially competing models or distillation workflows. For example, OpenAI
restricts using Output to develop competing AI models except for permitted
exceptions, and Anthropic states that Claude outputs may not be used as
training targets for competing models.[^1]

[^1]: See the providers' own terms for the current wording: OpenAI Terms of
    Use (<https://openai.com/policies/terms-of-use/>) and Anthropic Commercial
    Terms of Service (<https://www.anthropic.com/legal/commercial-terms>).

Accordingly, DPO files marked as `ai-assisted-review-required` or
`evidence-only` are not approved for training unless you verify for yourself
that the applicable terms and provenance allow it.

## Dataset Status Labels

Each training file should be treated according to its status:

- `ai-assisted-review-required`: do not train until you verify provider terms
  and provenance.
- `evidence-only`: provided only for inspection, comparison, debugging, or
  reproducibility.

The per-file status registry is `training-data/DATA-STATUS.md`.

## No Warranty for Data

All code, DPO files, examples, datasets, model artifacts, prompts, and
documentation are provided "as is," without warranty of correctness, legality,
fitness for purpose, non-infringement, or suitability for training.

You are solely responsible for deciding whether a file may be used for your
intended purpose.

Last updated: 2026-07-26.
