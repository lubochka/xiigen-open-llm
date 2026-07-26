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

## Off-site GEO TODO (post-publication only)

None of these can be executed while the repository is private: AI crawlers cannot
see a private repo at all, so effective GEO is 0 until publication. Every item
below is a dated TODO owned by the maintainer, to be done after the AGPL license
flip and repository publication.

- [ ] Set About, Topics, and social preview (already specified above in this
      file). Do this only after the AGPL license flip and repo publication.
      (added 2026-07-24)
- [ ] Decide on a public web home (GitHub Pages or a domain). Honest note:
      GitHub README pages are server-rendered and already crawlable, so a
      separate site is optional, not required for GEO. (added 2026-07-24)
- [ ] llms.txt: optional, low priority. Google Search ignores llms.txt (official
      Google guidance, 2026); it may help some non-Google AI crawlers. Never
      present it as a Google ranking lever. (added 2026-07-24)
- [ ] Post one honest announcement per relevant channel after publication (e.g. a
      single Reddit / Hacker News post, a LinkedIn note). No mention farming, no
      coordinated posting, no star-begging: brand mentions must be earned, and
      faking them is both dishonest and (per Google) not a ranking lever anyway.
      (added 2026-07-24)
- [ ] Record docs/assets/demo.gif (tokenizer -> model -> inference run) and
      replace the placeholder comment in README.md. Multi-modal content has
      ~156% higher AI-citation selection rates, and this is the repo's one
      missing visual. (added 2026-07-24)
- [ ] Re-run tools/verify-docs-claims.ps1 and refresh "Last updated" dates no
      less than every 90 days. Content under ~3 months old is roughly 3x more
      likely to be cited; content 6+ months stale loses eligibility.
      (added 2026-07-24)
