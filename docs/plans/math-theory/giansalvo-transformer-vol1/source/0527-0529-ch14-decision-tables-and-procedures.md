---
title: "Chapter 14: diagnostic decision tables and static procedures"
source_pdf_pages: [527, 529]
book_pages: [491, 493]
batch: "011"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 14: diagnostic decision tables and static procedures

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: PDF 523–526](0523-0526-ch14-design-recipe-and-static-interface.md#pdf-0526) · [Next: PDF 530–534](0530-0534-ch14-channel-interventions-and-toy-illustrations.md#pdf-0530)

> Editorial note: Full transcription of PDF 527–529 (printed pages 491–493), including every row of Tables 14.5–14.6 and all steps of Procedures 14.1–14.3. Procedure 14.2 crosses the page boundary. The source's terminology for attention/motor diagnostics, gate floor, pruning, and static capacity changes is retained; note 184 discusses the implementation qualifications.

<a id="pdf-0527"></a>
## PDF 527 · Book 491

<a id="section-14-10-1"></a>
#### 14.10.1 From coordinates to diagnostics

The quantities used by the interface have a direct implementation meaning. For a validation batch $\mathcal D$, each head $h$ in layer $\ell$ gives an empirical attention or motor matrix $M_{\ell h}(\mathcal D)$. Its symmetric and antisymmetric components are

<a id="display-0527-symmetric-antisymmetric-components"></a>
$$
M_{s,\ell h}=\frac{M_{\ell h}+M_{\ell h}^{\top}}2,\qquad M_{a,\ell h}=\frac{M_{\ell h}-M_{\ell h}^{\top}}2.
$$

The directional ratio $\alpha_{\ell h}$ measures whether the head is mainly reciprocal, mainly directional, or mixed. The residual amplification $\Gamma_{\mathrm{res}}^{(\ell)}$ measures how strongly the layer injects a new residual direction. Crowding measures whether several heads occupy almost the same signature region. Marginal value measures whether removing or weakening a head changes validation behaviour. Coverage measures whether the layer represents the required dictionary of signatures.

These quantities should not be read independently. A head with low marginal value and high crowding is a pruning candidate. A head with high crowding but high marginal value should not be removed without checking whether it implements a repeated but useful operation. A head with high $\alpha_{\ell h}$ and high marginal value is a directional-tail head and should be protected in oriented tasks. A layer with high $\Gamma_{\mathrm{res}}^{(\ell)}$ may not need more heads; it may need safer residual gates.

<a id="section-14-10-2"></a>
#### 14.10.2 Symptoms, diagnostics, and static interventions

Table 14.5 summarises the operational reading. The table is intended as a programmer-facing object. It does not replace the definitions; it tells how the definitions should be used during model surgery.

<a id="table-14-5"></a>

| Observed symptom | Diagnostic quantity | Interpretation | Static intervention |
|---|---|---|---|
| Many heads have nearly identical signatures | high local crowding $C_{\mathrm{orb}}$ | capacity is concentrated in the same region of signature space | merge, prune, or penalise redundant heads |
| Task requires order but heads remain reciprocal | low $\alpha_{\ell h}$ in useful heads | directional channel is under-allocated | increase or protect A-channel capacity |
| Residual stream becomes unstable | high $\Gamma_{\mathrm{res}}^{(\ell)}$ | residual injection is too aggressive | reduce gates, add clipping, or slow gate updates |
| Ablation does not affect validation loss | low $V_{\mathrm{marg}}(h)$ | head is removable or compressible | prune, merge, or reallocate the slot |
| Ablation sharply hurts performance | high $V_{\mathrm{marg}}(h)$ | head is functionally critical | preserve the head even under moderate crowding |
| Coverage remains poor | low dictionary coverage | the layer misses a required signature region | split an overloaded head or add a representative slot |
| All heads remain diffuse | high entropy and low diversity | specialisation is weak | sharpen attention or regularise diversity |
| FFN absorbs the correction | low attention marginality and high residual completion | attention is under-allocated or poorly typed | rebalance attention and feed-forward width |
| Directional heads disappear during pruning | high $\alpha_{\ell h}$ but low naive norm | norm-based pruning is unsafe | protect directional-tail heads by marginal value |

**Table 14.5:** Programming interface for static Transformer rationalisation. The intervention is static: it modifies, gates, prunes, or reallocates a fixed block and then validates the result.

A more compact decision table is useful when implementing automated scripts. Table 14.6 gives threshold-style rules. The thresholds are not universal constants. They are selected from validation statistics, for example by comparing each quantity with its layer median or with its moving estimate over checkpoints.

<a id="pdf-0528"></a>
## PDF 528 · Book 492

<a id="table-14-6"></a>

| Condition | Static decision | Validation check |
|---|---|---|
| $V_{\mathrm{marg}}(h)<\tau_V$ and $C_h>\tau_C$ | prune or merge $h$ | loss, coverage, and $\Gamma_{\mathrm{res}}$ do not degrade |
| $V_{\mathrm{marg}}(h)>\tau_V$ and $\alpha_h>\tau_A$ | preserve as directional head | oriented validation slice remains stable |
| $\Gamma_{\mathrm{res}}^{(\ell)}>\tau_\Gamma$ | lower residual gate or add gate floor | residual norm returns to target band |
| coverage $<\tau_{\mathrm{cov}}$ and crowding is low | add or split a representative slot | missing signature atom becomes covered |
| coverage $<\tau_{\mathrm{cov}}$ and crowding is high | redistribute crowded heads | coverage improves without increasing $H_\ell$ |
| $V_{\mathrm{marg}}(h)$ is noisy across batches | postpone pruning | moving estimate becomes stable |
| FFN marginality dominates attention marginality | increase typed attention or reduce generic FFN | attention contribution becomes measurable |

**Table 14.6:** Threshold-style static decisions. The symbols $\tau_V$, $\tau_C$, $\tau_A$, $\tau_\Gamma$ and $\tau_{\mathrm{cov}}$ denote validation-dependent thresholds, not universal constants.

<a id="section-14-10-3"></a>
#### 14.10.3 Procedural algorithms

The interface can be written as three short procedures. They are intentionally expressed as implementation procedures rather than as mathematical optimisation algorithms. This avoids suggesting a false global optimality claim and makes the recipe directly translatable into code.

<a id="procedure-14-1"></a>
**Procedure 14.1: Static diagnosis of a Transformer block.** Given a trained block $B_\ell$ and a validation batch $\mathcal D$, compute a diagnostic profile as follows.

1. For each head $h$, compute the empirical attention or motor matrix $M_{\ell h}(\mathcal D)$.
2. Decompose $M_{\ell h}$ into $M_{s,\ell h}$ and $M_{a,\ell h}$.
3. Compute $\alpha_{\ell h}$, the head signature, the head entropy, and the residual response.
4. Estimate $V_{\mathrm{marg}}(h)$ by ablation, gate weakening, or a first-order intervention score.
5. Compute layer-level coverage, crowding, orbit diversity, and $\Gamma_{\mathrm{res}}^{(\ell)}$.
6. Return the diagnostic profile

<a id="display-0528-diagnostic-profile"></a>
$$
\mathcal P_\ell=\{\alpha_{\ell h},V_{\mathrm{marg}}(h),C_h,\Gamma_{\mathrm{res}}^{(\ell)},\mathrm{Cov}_\ell,D_{\mathrm{orb}}^{(\ell)}\}_{h=1}^{H_\ell}.
$$

The output is a static map of the layer. It can be computed for a standard block as well as for a rationalised block.

<a id="procedure-14-2"></a>
**Procedure 14.2: Static rationalisation of a trained block.** Given the diagnostic profile $\mathcal P_\ell$, construct a rationalised static block as follows.

1. Mark heads with low marginal value and high crowding as pruning or merging candidates.
2. Mark heads with high $\alpha_{\ell h}$ and high marginal value as protected directional heads.
3. If $\Gamma_{\mathrm{res}}^{(\ell)}$ is above its target band, reduce the corresponding residual gates before changing head count.
4. If coverage is low in an uncrowded region, split an overloaded head or allocate a new representative slot.

<a id="pdf-0529"></a>
## PDF 529 · Book 493

5. If coverage is low in a crowded region, redistribute or decorrelate the crowded heads before adding capacity.
6. Reassign surviving heads to $S$, $A$, and mixed channels.
7. Fine-tune the modified block and repeat the diagnosis on the same validation protocol.

This procedure performs static model surgery. It changes the organisation of a fixed block, but it does not yet introduce the incremental growth rule of INCRT.

<a id="procedure-14-3"></a>
**Procedure 14.3: Geometry-aware pruning and preservation.** Given a pruning budget $q$, select heads for removal or preservation as follows.

1. Exclude protected heads with high directional ratio and high marginal value.
2. Rank the remaining heads by increasing marginal value and decreasing local crowding.
3. Remove at most $q$ heads from the top of this list.
4. Recompute loss, coverage, crowding, orbit diversity, and $\Gamma_{\mathrm{res}}$.
5. Accept the pruning step only if the validation loss and the geometry diagnostics remain inside their allowed bands.
6. If the check fails, restore the most directional or most marginally valuable removed head and retry.

This rule explains why norm-based pruning is insufficient. A small-norm directional head may be rare but necessary, whereas a large reciprocal head may be redundant.

<a id="section-14-10-4"></a>
#### 14.10.4 What the programmer gains

The static interface gives three practical gains. First, it gives diagnostics. A head is no longer described only by an attention map or a norm; it is described by directionality, marginal value, crowding, coverage, and residual effect. Second, it gives interventions. The possible actions are not limited to deleting heads. One may lower gates, protect directional tails, split overloaded heads, compress symmetric crowds, or rebalance attention and feed-forward width. Third, it gives validation criteria. A modification is accepted only if task performance and geometric diagnostics remain stable.

The interface is therefore useful even if the incremental architecture is never used. A programmer can apply it to a standard trained Transformer, obtain a static diagnosis, perform geometry-aware pruning or reallocation, and audit the result. INCRT will later replace the fixed intervention loop by a growth process, but the programming interface already has independent value.

**Takeaway.** *The static theory ends when its coordinates become implementation diagnostics. The Rationalised Transformer is therefore both an architecture and a programming interface for diagnosing, pruning, reallocating, and stabilising Transformer blocks.*

<a id="section-14-11"></a>
### 14.11 Fixed-position channel intervention experiment

The previous section interprets the rationalised block as a static programming interface. This section adds a controlled experiment whose purpose is to test that interpretation directly. The question is not whether the rationalised block can outperform a standard
