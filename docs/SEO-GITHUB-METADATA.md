# GitHub repository metadata (About + Topics)

Set these in the GitHub repository UI only AFTER the license is actually changed
to AGPL-3.0-only. Do not open the storefront (About/Topics/social preview) before
the license flip.

## About (repository description, <= 350 characters; measured length: 284)

An LLM and full ML stack built from scratch in pure C# (.NET 9): BPE tokenizer,
transformer, training loop, LoRA, and a from-scratch DPO trainer, plus an
orchestration/agent layer. A study/reference stack, not a finished product.
AGPL-3.0-only. Trainable core of the XIIGen ecosystem.

## Topics (candidate set; the maintainer finalizes before publishing)

csharp, dotnet, llm, machine-learning, from-scratch, local-llm, agents,
agentic-ai, dpo, lora, transformer, bpe-tokenizer, multi-agent, mcp,
no-pytorch, agpl, ai-agents, semantic-search, orchestration

## Deliberately excluded topics (honesty)

- open-source-llm — no trained general-purpose LLM weights are shipped; the
  transformer / tokenizer / training loop are a from-scratch implementation, and
  the only trained model is the domain TranslationMachine (RU<->EN), not a
  universal LLM. Use "llm" + "from-scratch" for the implementation instead.
- production-ready — no releases, load benchmarks, or SLA; this is a
  study/reference stack, not a finished product.
- battle-tested — no production operation or incident history.
- enterprise — no enterprise deployments, support, or guarantees.

## Social preview

A 1280x640 image with the project name and one honest pitch line (the same pitch
as the README/About). Upload it in Settings -> Social preview.
