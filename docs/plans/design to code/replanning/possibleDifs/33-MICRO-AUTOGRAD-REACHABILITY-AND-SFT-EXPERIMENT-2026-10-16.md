# MICRO-N33 — 2 быстрых проверки графа и 1 ограниченный SFT contrast

**PLAN / NOT_RUN · 16.10.2026.** Вход: [N33 autograd source proof](33-NOVEL-AUTOGRAD-REACHABILITY-EMBEDDING-AND-BIAS-2026-10-16.md). Эксперимент нельзя выдавать за исполненную модель, CODEX/Astra judgement или повышение качества RP01. Только один новый gradient repair в изолированной dev-ветке после подтверждения source; никаких изменений frozen C#500/ENRU500, моделей и оригинальных heldout.

## Step 0: preflight ≤ несколько десятков строк, НИКАКОГО обучения

Проект: `00.TensorCore` с одной существующей тестовой точкой/временной консолью (не переписывать старые тесты). Сравнить с exact source `Embedding.cs`, `Tensor.cs`, `Linear.cs`, `TensorOps.Sum`, `Trainer.TrainSupervisedNextToken`. До запуска зафиксировать HEAD, .NET runtime/OS, model seed, входы. Тест доступных primitives:

```csharp
using TensorCore.Core;
using TensorCore.Layers;

var ids = new Tensor(new float[] { 1, 1, 2 }, new TensorShape(1, 3));
var embedding = new Embedding(vocabSize: 8, hiddenDim: 4);
var projected = embedding.Forward(ids);
var objective = TensorOps.Sum(projected);
objective.Backward();
Console.WriteLine($"embedding: paramRequires={embedding.Weight.Value.RequiresGrad}; " +
                  $"outputRequires={projected.RequiresGrad}; " +
                  $"paramGradient={(embedding.Weight.Grad is null ? "NULL" : "NONNULL")}");

var linear = new Linear(inFeatures: 4, outFeatures: 2, useBias: true);
var x = new Tensor(new float[] { 1, 2, 3, 4 }, new TensorShape(1, 4));
var score = TensorOps.Sum(linear.Forward(x));
score.Backward();
Console.WriteLine($"linear: weightGrad={(linear.Weight.Grad is null ? "NULL" : "NONNULL")}; " +
                  $"biasGrad={(linear.Bias!.Grad is null ? "NULL" : "NONNULL")}");
```

**Expected current branch from static control-flow:** `embedding: true/false/NULL`, `linear: NONNULL/NULL`. If actual differs, STOP: source/runtime differ; investigate before any repair. This is a capability test of backward reachability, not a domain skill test. Do **not** infer that no other weights can be trained.

## Step 1: isolated one-change repair; finite difference

Change ONLY `Embedding.Forward` to return an autograd-connected gather whose `BackwardFunc` performs

```
grad[ tokenId * hiddenDim + d ] += output.Grad[b,t,d]
```

for all rows, positions and dimensions, including repeated IDs; add `Weight.Value` as child if `RequiresGrad`. No gradient for IDs: they are categorical indexes stored as float. Explicitly check range of integer IDs, cancellation and finite grad. Avoid in-place parameter mutation inside backward; optimizer owns updates. Validate `id=1` occurring twice produces **sum** of both contributions, while unused `id=3` gradient is exactly zero. Add one finite-difference directional check at `id=1,d=0`, `epsilon` selected to exceed float rounding (e.g. 1e-3 after calibration) with `|analytic-numeric|/max(1,|numeric|)<1e-2`. Negative control: marked frozen `Embedding.Weight.Value.RequiresGrad=false` must receive no grad.

For `Linear(useBias:true)` run a **separate** one-change patch: attach inverse broadcast to Bias and sum gradients over every expansion position. No repair should be bundled into the same A/B causal arm as Embedding. For ordinary Transformer q/v, bias seam is OUT_OF_SCOPE. Success: `Bias.Grad` non-null and equals analytic position sums; no changed linear weight gradients outside FP tolerance.

## Step 2: shortest trainability ablation, only if Step 1 passes

**Hypothesis H33-A:** missing Embedding backward makes planned SFT/random-B0 pretraining ineffective *at its token embedding leaf*. Difference may or may not explain downstream domain quality; do not promote automatically.

- **Training data**: 8–16 tiny train-only synthetic token sequences, e.g. repeatable (prefix, next-token) mappings with 3–5 classes, plus 8 heldout *distinct templates*; freeze tokenizer once. Original English/Russian/C# corpora not changed; labels/chosen/rejected are not needed for this gradient probe.
- **A — CURRENT**: existing Embedding.Forward, frozen base/head/block/norm except token_embedding.weight trainable, exact same Student B0 seed, optimizer initial state and 10–30 supervised steps.
- **B — ONLY_GATHER_GRAD_REPAIRED**: same tensors/settings, **single** differential: autograd gather + scatter-add. Same valid `Trainer.TrainSupervisedNextToken` or pretraining path; same inputs/steps and wall/compute budget. Learning rate, optimizer moments, clip, weight decay and memory documented.
- **Optional C — POSITIVE_CONTROL**: unfreeze head as well, current embedding; only if A/B insufficient to demonstrate difference and run budget allows. This tests whether ordinary head learns something, **not** an A/B embedding proof.
- **No misleading comparison**: do not compare DPO-only C#500 with SFT+LoRA as if same compute or same datasets. Separate DPO and normal generation stage after SFT validation.

**Primary success condition**: Arm A `grad_embedding=NULL`, `norm(Δembedding)=0`, B `grad_embedding≠NULL` and `norm(Δembedding)>0` on train signal; directional gradient matches finite differences; frozen params remain unchanged. **Secondary**: train next-token NLL drop and chosen-token accuracy (teacher forcing). **Tertiary**: exact heldout NLL and actual autoregressive generations, evaluated on same frozen prompts, raw outputs and independent domain-law grade. A better gradient with no heldout gain is a fixed training capability, **not demonstrated useful learning**.

**Abort rules**: (1) no native mismatch; (2) B gradient nonfinite; (3) unallowed base parameters change; (4) rapid 8–16-case training shows no difference despite verified gradients; (5) checkpoint/load mismatch; (6) time cap set by executor upfront. Do not trigger 500 new pairs or Windows coverage matrix from this experiment. Test only changed deterministic edge.

## Step 3: quality bridge, only after gradient probe

For real RP01 recovery, independently compare **pretrained/warmed base** vs **random B0**, and SFT→DPO vs DPO-only on frozen source-disjoint panels. First prove proper byte-BPE IDs, EOS target/stop policy, q/v LoRA parameters, optimizer continuity and normal cold consumer. Domain-law exam sets:
- C#: exact syntax/compilation semantics and AST order; separate Roslyn oracle from CODEX/Astra qualitative judge.
- EN↔RU: negation, referents, gender, clause order; POS/sequence context kNN must be separately ablated, not applied to all domains.
- SQL: scopes, NULL truth and aggregation; tree/order retrieval only where relevant.
- Pure mathematical or short deterministic task: structural kNN may not help; include no-kNN control.
On every domain, compare train-token accuracy, pair margin, free response law accuracy, retention and cold-readback separately. Preserve role ownership: CODEX Astra 6 UltraCode selects; real approved open Chinese model authors **both** DPO sides; independent CODEX Astra 6 UltraCode grades pair; separate CODEX Astra grades actual Student output. No fake actor receipts. Independent assessor uses blind immutable case IDs.

## Artifacts per single tiny run

`N33-run-id` + exact code SHA + branch + tokenizer SHA + base/reference/LoRA SHA; JSON row `{arm,seed,task_id,input_token_ids,gradient_nonnull,grad_norm,delta_weight_norm,train_nll,heldout_nll,generation,law_verdict,stop_reason,elapsed_ms,peak_ram_mb}`; one paired Markdown verdict with PASS/FAIL/NOT_RUN per criterion. Review by code reviewer then plan/experiment reviewer; no manual reinterpretation of frozen prior RP01 outcomes. **Current state: NOT_RUN — proposal only.**

### Source pointers

- [Embedding no graph](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/00.TensorCore/Layers/Embedding.cs#L36-L56); [Parameter requiresGrad](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/00.TensorCore/Layers/Parameter.cs#L14-L22).
- [Linear detached ExpandBias](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/00.TensorCore/Layers/Linear.cs#L49-L87); [TensorOps.Sum backward](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/00.TensorCore/Core/TensorOps.cs#L507-L528).
- [Trainer supervised](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/04.TrainingLoop/Trainer.cs#L100-L147); [existing RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md).
