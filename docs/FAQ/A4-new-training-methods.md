# Which new training methods does LlmFromScratch implement?

LlmFromScratch implements Direct Preference Optimization (DPO), Low-Rank Adaptation (LoRA), and a classic supervised training loop from scratch — the established methods, not the specific 2026 techniques (activation sparsity, prospective credit assignment) named in the trend. Its routing-profile builder is a deterministic scaffold with no gradient step, not a trained model.

LlmFromScratch implements three real from-scratch training mechanisms. `11.DPO/DpoTrainer.cs:16` runs a genuine DPO step (`Step` at line 38) computing `margin = chosenReward - rejectedReward` (line 51), `loss = -log sigmoid(margin)` (line 54), a manual backward pass (line 277), an AdamW optimizer step (line 61), and checkpoint save (lines 76/344). `10.LoRA/LoraApplicator.cs:17` builds real from-scratch LoRA adapters over a frozen base, and `04.TrainingLoop/Trainer.cs:24` is the classic supervised loop (forward -> cross-entropy -> backward -> clip -> optimizer step -> zero-grad). Honesty boundary: DPO, LoRA and the supervised Trainer are the verified-trained list. The `26.TrainableRouter` routing profile is a scaffold only — `26.TrainableRouter/Training/RoutingProfileTrainer.cs:10` aggregates keywords and `26.TrainableRouter/Training/RoutingDpoPairBuilder.cs:10` builds preference pairs, both with no gradient/loss step. A grep for `margin|loss|gradient|backward|AdamW` over `26.TrainableRouter` returns 0, confirming the router runs no gradient update; none of this implements the specific 2026 methods named in the trend — those remain the trend, while what actually ships here is classic preference optimization and low-rank adaptation.

### Where is this in the code?

Training is driven per batch by `DpoTrainer.Step` (the DPO update) and by the supervised `Trainer` loop, with gradient accumulation and checkpointing; the router's keyword profiles and DPO pairs are assembled by `RoutingProfileTrainer` / `RoutingDpoPairBuilder` as scaffolding, not applied through a gradient update.

### What is the industry trend?

2026 training research explores newer methods such as selective activation sparsity and prospective credit assignment to make training cheaper and more sample-efficient.

### Honesty boundary

The real, implemented from-scratch training mechanisms here are DPO (`DpoTrainer`), LoRA (`LoraApplicator`) and the classic supervised `Trainer` loop — the verified-trained list. The `26.TrainableRouter` routing profile is a training scaffold (keyword-profile building plus DPO-pair construction); it runs no gradient/loss step (a grep for `margin|loss|gradient|backward|AdamW` over `26.TrainableRouter` returns 0), so it is not a gradient-trained model and is not in the verified-trained list. And none of this implements the specific 2026 methods named in the trend (activation sparsity, prospective credit assignment) — those are the trend; what ships here is classic preference optimization and low-rank adaptation.

Last updated: 2026-07-24
