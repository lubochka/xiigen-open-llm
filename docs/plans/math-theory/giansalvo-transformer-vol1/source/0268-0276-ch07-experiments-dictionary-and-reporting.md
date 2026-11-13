---
title: "Chapter 7: synthesis, empirical protocol, dictionary expansion, and reporting"
source_pdf_pages: [268, 276]
book_pages: [232, 240]
batch: "006"
language: en
type: source-transcription
status: complete-for-page-range
continues_after_file: true
---

# Chapter 7: synthesis, empirical protocol, dictionary expansion, and reporting

[Library index](../README.md) · [Source and transcription policy](../SOURCE.md) · [Previous pages: PDF 265–267](0265-0267-ch07-replacement-map-and-hypotheses.md#pdf-0267)

> Editorial note: Full transcription of PDF 268–276 (printed pages 232–240), including §§7.27–7.37 and the opening of §7.38. Conjectures, empirical recommendations, and architectural interpretations retain their original status. All mathematical displays in this range are unnumbered in the source. The phrase “The argument now make” is retained as printed. See [note 66](../notes/66-factorial-geography-and-research-hypotheses.md), [note 67](../notes/67-reproducible-signature-measurements-and-model-comparison.md), and [note 68](../notes/68-causal-pruning-distribution-shift-and-dictionary-growth.md) for separate explanations and qualifications.

<a id="pdf-0268"></a>
## PDF 268 · Book 232

<a id="section-7-27"></a>
### 7.27 Synthesis

The corrected global theory has a different object from the old one. The old object was a grid of hard pattern labels. The new object is a grid of canonical signatures, aggregated into layer distributions and transported across depth.

The local map is

$$
S_{\ell,h}(X)\longmapsto
\Sigma_{\ell,h}(X)=\bigl(\Lambda_{\ell,h}(X),\rho_{\ell,h}(X)\bigr).
$$

The layer map is

$$
\{S_{\ell,h}\}_{h=1}^{H}\longmapsto
\mu_\ell=\frac1H\sum_h\delta_{\Sigma_{\ell,h}}.
$$

The global map is

$$
\{\mu_\ell\}_{\ell=1}^{L},
$$

the geography of attention.

This geography is rich enough to keep the useful ideas of the preliminary global analysis: layer grids, C1–C4 experimental contrasts, channel mixing, co-occurrence statistics, activation trajectories, all-closed saddle escape, finite-noise selection, Łojasiewicz convergence, and BERT-small empirical validation. It is also rigorous enough to avoid the old errors: no absolute eight-pattern completeness, no hard canonicalisation theorem, no false cone-to-activation implication, no margin-only uniformity claim, no unnormalised activation fraction, and no head-indexed gauge dependence.

The central thesis of the chapter is therefore:

> The global state of attention is the distribution of canonical signatures across heads, layers, inputs, and training time.

This thesis is the bridge to the following chapters. Any later argument about pruning, robustness, training, information flow, or architecture can now be stated in terms of signatures. If a later chapter needs only residual stability, it can cite Proposition 7.9. If it needs convergence, it can cite Theorem 7.28. If it needs global geography, it can use the empirical distribution $\mu_\ell$. If it needs the dynamics of emergence, it can use local saddle escape and projected escape signatures.

The price is that this chapter is long. The benefit is that the foundations are no longer brittle. Later chapters may move pieces of this machinery to specialised locations. For now, keeping the entire signature theory in Chapter 7 gives the rest of the monograph a stable base.

<a id="section-7-28"></a>
### 7.28 Interpreting the C1–C4 geography

The C1–C4 experiment should be read as a factorial design with two binary factors:

$$
\text{weights}\in\{\text{base},\text{fine-tuned}\},
\qquad
\text{input}\in\{\text{white noise},\text{identity-like}\}.
$$

This design is valuable because attention geography depends on both the score operator and the input cloud. A head is not simply a fixed matrix $M$; it produces

$$
S(X)=\operatorname{softmax}_{\mathrm{row}}
\bigl(XMX^\top/\sqrt{d_k}\bigr).
$$

Changing $X$ can change the signature even when $M$ is fixed.

<a id="pdf-0269"></a>
## PDF 269 · Book 233

<a id="section-7-28-1"></a>
#### 7.28.1 Weight-driven signatures

A signature component is weight-driven if it persists across input conditions but changes under fine-tuning. For example, if a directional coordinate appears in both white-noise and identity input after fine-tuning but not before, the directional behaviour is likely encoded in the trained weights.

<a id="section-7-28-2"></a>
#### 7.28.2 Input-driven signatures

A signature component is input-driven if it appears for identity-like inputs but not for white-noise inputs under the same weights. Open and contextual regimes often have this property because they depend on self-similarity and local positional geometry in the input cloud.

<a id="section-7-28-3"></a>
#### 7.28.3 Interaction signatures

A component is interaction-driven if it appears only for a particular combination of weights and input. Such signatures are especially important: they suggest that the trained operator has created a geometry that is activated only by structured inputs.

<a id="section-7-28-4"></a>
#### 7.28.4 Residual interpretation

The residual should also be compared across C1–C4. High residual in white-noise conditions can indicate that the dictionary is not suitable for random inputs. High residual only after fine-tuning can indicate that the task has produced a new non-canonical mechanism. High residual only under identity-like inputs can indicate structured attention that is not among the eight canonical regimes.

Thus the C1–C4 design is not merely a visual comparison. It is a decomposition of geography into weight effects, input effects, and their interaction.

<a id="section-7-29"></a>
### 7.29 Conjectures and empirical hypotheses

The chapter contains rigorous theorems where possible. Several important ideas remain conjectural. They are stated explicitly so that later chapters can use them as hypotheses rather than hidden assumptions.

<a id="conjecture-7-30"></a>
**Conjecture 7.30 (Signature specialisation).** *During successful training on a structured task, the empirical layer distributions $\mu_\ell(t)$ become more separated across depth. In particular, early, middle, and late layers tend to occupy different regions of signature space.*

<a id="conjecture-7-31"></a>
**Conjecture 7.31 (Residual maturation).** *For tasks well described by the eight-regime dictionary, the average canonical residual*

$$
\bar\rho(t)=\frac1{LH}\sum_{\ell,h}\rho_{\ell,h}(t)
$$

*decreases after an initial exploration phase. For tasks requiring non-canonical mechanisms, $\bar\rho(t)$ may remain high or increase.*

<a id="conjecture-7-32"></a>
**Conjecture 7.32 (Projected unstable signatures).** *Near a symmetric high-entropy critical region, the unstable Hessian component with smallest exit time predicts the dominant early movement of the canonical signature distribution.*

<a id="pdf-0270"></a>
## PDF 270 · Book 234

<a id="conjecture-7-33"></a>
**Conjecture 7.33 (Task fingerprint).** *Each task distribution induces a characteristic average geography*

$$
\bar\mu_{\mathrm{task}}
=\mathbb E_{X\sim\mathcal D_{\mathrm{task}}}
\frac1L\sum_{\ell=1}^{L}\mu_\ell(X),
$$

*and tasks with similar mechanistic solutions have nearby fingerprints.*

<a id="conjecture-7-34"></a>
**Conjecture 7.34 (Residual discovery).** *Stable high-residual structures that recur across models, seeds, and tasks correspond to missing canonical regimes and should be promoted to new dictionary atoms.*

These statements are not used as theorems in this chapter. They organise the empirical programme.

<a id="section-7-30"></a>
### 7.30 Extended empirical protocol for signature geography

A signature theory becomes useful only if its measurements can be made reproducibly. This section records a protocol that later empirical chapters can follow without changing the mathematical definitions.

<a id="section-7-30-1"></a>
#### 7.30.1 Inputs and checkpoints

For a trained or partially trained Transformer, choose a set of checkpoints $t_1,\ldots,t_T$, a set of input distributions $\mathcal D_1,\ldots,\mathcal D_m$, and a sequence-length convention. For each checkpoint and each input batch, compute the attention maps $S_{\ell,h}(X;t)$. If the model uses variable sequence lengths, the dictionary must either be rebuilt at each length or the maps must be restricted to a common effective length. Padding tokens should be handled before signature computation; otherwise the closed coordinate can be artificially inflated by masked rows.

<a id="section-7-30-2"></a>
#### 7.30.2 Dictionary selection

The full dictionary $\mathcal D_8(n)$ can be large. For diagnostics, a restricted dictionary is often preferable. A restricted dictionary should be declared before measuring signatures. Typical restrictions include:

- all open and closed atoms;
- all directional columns;
- shifts with $|d|\le d_{\max}$;
- contiguous cluster blocks with bounded size;
- instance atoms above a salience threshold;
- inverse-directional supports selected by row entropy and support mass.

Changing the dictionary changes the residual. Therefore residuals should only be compared between experiments that use the same dictionary construction.

<a id="section-7-30-3"></a>
#### 7.30.3 Normalisation

Attention maps already lie in $\mathcal S_n$, so the Frobenius metric is meaningful. For comparisons across lengths, use the normalised distance

$$
n^{-1/2}\|S-T\|_F.
$$

<a id="pdf-0271"></a>
## PDF 271 · Book 235

The same normalisation should be used for residuals. Entropy should be normalised by $\log n$ if sequence lengths vary. Directional activation should use the operator-norm normalisation of Definition 7.21.

<a id="section-7-30-4"></a>
#### 7.30.4 Reported quantities

Every empirical figure should report at least:

$$
\bar\Lambda_\ell,\qquad
\bar\rho_\ell,\qquad
H_\ell,\qquad
\widehat\gamma_\ell,\qquad
\Delta_\ell.
$$

When possible, also report the variance across heads. A layer with mean signature $(0.5,0.5)$ may be a layer of mixed heads or a layer with half the heads pure in one regime and half pure in another. The distribution matters.

<a id="section-7-30-5"></a>
#### 7.30.5 Uncertainty

For trained models, report variability across seeds and inputs. For fine-tuning, report variability across checkpoints. A geography that is stable across seeds is a stronger structural feature than a geography visible only in one run. A geography that appears before loss improvement may be a precursor; a geography that appears only after convergence may be a by-product.

<a id="section-7-30-6"></a>
#### 7.30.6 Causal validation

A signature is descriptive until it is tested causally. The basic intervention is

$$
S_{\ell,h}\mapsto\Pi_8(S_{\ell,h}).
$$

If the output changes little, the canonical component explains the head. If the output changes substantially, the residual is functionally important. The second intervention is atom replacement:

$$
S_{\ell,h}\mapsto D_a.
$$

This tests which canonical atom best preserves the computation. The third intervention is residual deletion:

$$
S_{\ell,h}\mapsto S_{\ell,h}-R_8(S_{\ell,h}).
$$

This is the same as projection but should be interpreted as removing non-canonical structure.

<a id="section-7-31"></a>
### 7.31 Model comparison

Signature geography gives a model-level comparison tool. For a model $M$, input distribution $\mathcal D$, and layer $\ell$, define

$$
\mu_\ell^M(\mathcal D)
=\mathbb E_{X\sim\mathcal D}
\frac1H\sum_{h=1}^{H}\delta_{\Sigma_{\ell,h}^M(X)}.
$$

Two models $M$ and $M'$ can be compared by

$$
\sum_{\ell=1}^{L}
W_1\bigl(\mu_\ell^M(\mathcal D),\mu_\ell^{M'}(\mathcal D)\bigr).
$$

This distance ignores arbitrary head numbering and focuses on the distribution of mechanisms.

<a id="pdf-0272"></a>
## PDF 272 · Book 236

Such a comparison can answer several questions. Does fine-tuning move only late layers? Does distillation preserve the geography of the teacher? Do larger models reduce residuals? Do models trained on different domains occupy different regions of signature space? Does a protein model use cluster signatures more than a language model? These questions are meaningful because signatures are defined at the level of attention operators, not raw weights.

<a id="section-7-32"></a>
### 7.32 Signature-aware pruning

Pruning should not be based on entropy alone. A closed head can be useless, but it can also compute a useful global average. A high residual head can be noise, but it can also encode a missing regime. Signature-aware pruning combines canonical and causal information.

A simple pruning score can use:

$$
\Lambda_{\mathrm{closed}},\qquad
H,\qquad
\widehat\gamma,\qquad
\rho_8,\qquad
\Delta_{\mathrm{task}},
$$

where $\Delta_{\mathrm{task}}$ is a causal sensitivity score, such as the increase in validation loss after ablating the head. Heads with high closed mass, high entropy, low activation, low residual, and low task sensitivity are strong pruning candidates. Heads with high residual and high task sensitivity should be kept and studied.

<a id="definition-7-35"></a>
**Definition 7.35 (Signature pruning score).** A generic signature pruning score has the form

$$
P_{\ell,h}
=a\Lambda_{\mathrm{closed}}+bH-c\widehat\gamma-d\rho_8-e\Delta_{\mathrm{task}},
$$

with non-negative coefficients selected by validation. Large $P_{\ell,h}$ indicates likely redundancy.

This formula is not a theorem; it is an operational template. Its purpose is to show how geography can be exploited. The residual enters with a negative sign because non-canonical structure may be important. If empirical evidence shows that high residual is mostly noise in a particular setting, the coefficient can be changed. The theory provides the features; the pruning policy is empirical.

<a id="section-7-33"></a>
### 7.33 Distribution shift and robustness

Let $\mu_\ell^{\mathrm{train}}$ be the expected training geography. For a test input $X$, define the geography shift

$$
G(X)=\sum_\ell
W_1\bigl(\mu_\ell(X),\mu_\ell^{\mathrm{train}}\bigr).
$$

A large value of $G(X)$ indicates that the model is processing the input using an unusual internal geography. This can happen even when the output confidence is high. Therefore geography shift is a candidate internal distribution-shift detector.

For adversarial robustness, compare $X$ and a perturbed input $\widetilde X$:

$$
\Delta_{\mathrm{geo}}(X,\widetilde X)=
\sum_\ell W_1\bigl(\mu_\ell(X),\mu_\ell(\widetilde X)\bigr).
$$

A robust model should have small geography drift under semantically preserving perturbations. A perturbation that leaves the output unchanged but radically changes the geography has altered the internal computation. A perturbation that changes the output while preserving geography may exploit a downstream readout rather than attention structure.

These diagnostics are not replacements for accuracy. They are mechanistic observables that can detect failures before or alongside output-level metrics.

<a id="pdf-0273"></a>
## PDF 273 · Book 237

<a id="section-7-34"></a>
### 7.34 Detailed experimental design for future chapters

This chapter is theoretical, but it is written to support empirical chapters. This section specifies the measurements that should be reported whenever attention geography is used.

<a id="section-7-34-1"></a>
#### 7.34.1 Per-head record

For each input $X$, layer $\ell$, head $h$, and checkpoint $t$, record:

$$
\Lambda_{\ell,h}(X,t),\qquad
\rho_{\ell,h}(X,t),\qquad
H_{\ell,h}(X,t),\qquad
\Delta_{\ell,h}(X,t),\qquad
\widehat\gamma_{\ell,h}(X,t).
$$

If value matrices are available, record the norm of the head output and the change in loss under head ablation. If gradients are available, record gradient norm with respect to logits and with respect to value outputs. This separates a geometrically distinctive head from a functionally important head.

<a id="section-7-34-2"></a>
#### 7.34.2 Per-layer record

For each layer, record:

$$
\bar\Lambda_\ell,\qquad
\operatorname{Var}(\Lambda_\ell),\qquad
\bar\rho_\ell,\qquad
\operatorname{Var}(\rho_\ell),
$$

and the empirical distribution $\mu_\ell$. Report also the fraction of heads with residual above thresholds such as $0.1,0.25,0.5$ in normalised Frobenius units. This identifies layers where the canonical dictionary is insufficient.

<a id="section-7-34-3"></a>
#### 7.34.3 Across-layer record

For consecutive layers, record the signature drift

$$
D_{\ell\to\ell+1}=W_1(\mu_\ell,\mu_{\ell+1}).
$$

If a hard dominant-label transition matrix is shown, report it as a coarse discretisation and include the average residual of the heads in each transition cell. A transition from directional to cluster with high residual is not the same as the same transition with low residual.

<a id="section-7-34-4"></a>
#### 7.34.4 Across-training record

For checkpoints, record:

$$
t\mapsto\bar\rho(t),\qquad
t\mapsto\bar\Lambda(t),\qquad
t\mapsto D_{\ell\to\ell+1}(t).
$$

Plot these against training loss. If observables move before loss improves, the geometry may be a precursor. If loss improves before geography stabilises, the model may first solve the task with non-canonical mechanisms and only later simplify.

<a id="section-7-34-5"></a>
#### 7.34.5 Across-input record

For a fixed model, compare clean inputs, corrupted inputs, adversarial inputs, and out-of-distribution inputs. The key quantity is geography shift:

$$
G(X)=\sum_\ell W_1\bigl(\mu_\ell(X),\mu_\ell^{\mathrm{reference}}\bigr).
$$

A high value of $G(X)$ indicates an internal processing shift.

<a id="pdf-0274"></a>
## PDF 274 · Book 238

<a id="section-7-34-6"></a>
#### 7.34.6 Across-model record

When comparing models, use the same dictionary and metric. If sequence lengths differ, either restrict to a common length or use a length-normalised dictionary. Report layer-aligned and depth-normalised distances. For models with different numbers of heads, empirical distributions remain comparable; this is one advantage of the measure-valued formulation.

<a id="section-7-34-7"></a>
#### 7.34.7 Reporting standard

Every experiment should include:

1. dictionary definition;
2. metric definition;
3. residual normalisation;
4. projection solver or approximation method;
5. seed and input variability;
6. causal intervention results when available.

Without these details, signature plots are descriptive but not reproducible.

<a id="section-7-35"></a>
### 7.35 Dictionary expansion

The eight-regime dictionary is a starting point, not a prison. The residual gives a principled method for discovering new regimes.

<a id="section-7-35-1"></a>
#### 7.35.1 Residual clustering

Collect residuals

$$
R_8\bigl(S_{\ell,h}(X)\bigr)
$$

over a large corpus of models, layers, heads, and inputs. Normalise them by their Frobenius norm when the norm is nonzero. Cluster the normalised residual directions. A cluster that appears across many seeds or tasks indicates stable non-canonical structure.

<a id="section-7-35-2"></a>
#### 7.35.2 Candidate atom extraction

For each residual cluster, compute a representative residual direction $\bar R$. Add it to the canonical projection at the corresponding face to obtain a candidate attention map

$$
D_{\mathrm{new}}=\Pi_8(S)+\epsilon\bar R,
$$

then project back to the row-stochastic polytope if needed. The candidate becomes a proposed new dictionary atom if it is interpretable, stable, and causally useful.

<a id="section-7-35-3"></a>
#### 7.35.3 Causal test

A residual cluster is promoted only if replacing the residual by its cluster representative preserves or improves model behaviour on a relevant task, or if deleting it reliably damages behaviour. This prevents the dictionary from growing around noise.

<a id="pdf-0275"></a>
## PDF 275 · Book 239

<a id="section-7-35-4"></a>
#### 7.35.4 Mathematical test

A new atom should come with a defining property: support geometry, symmetry group, rank/entropy profile, or logit-level emergence condition. The goal is not to add arbitrary examples but to expand the canonical grammar.

<a id="section-7-35-5"></a>
#### 7.35.5 Iterated dictionaries

After adding a new regime, define

$$
K_9(n)=\operatorname{conv}\mathcal D_9(n)
$$

and recompute residuals. A successful new atom reduces residuals systematically. If residuals remain structured, repeat. This yields an iterative programme:

$$
\mathcal D_8\to\mathcal D_9\to\mathcal D_{10}\to\cdots.
$$

The eight regimes remain the first-order dictionary, but the theory is open to refinement.

<a id="section-7-36"></a>
### 7.36 Examples of possible missing regimes

Several families are natural candidates for future expansion.

**Permutation regimes.** The open and contextual gates are special permutation matrices. General permutations are valid deterministic attention maps but are not in the eight-regime dictionary unless added explicitly. Tasks involving sorting, reordering, or structured copying may produce such maps.

**Hierarchical blocks.** A head may attend to nested blocks: coarse groups at one scale and fine groups inside them. This cannot be represented exactly by a single cluster atom.

**Sparse irregular supports.** Some heads may attend to non-contiguous sparse sets. These are not instance gates if multiple rows have irregular supports, and they are not inverse-directional gates if the active supports vary row by row.

**Low-rank non-uniform maps.** A matrix can be low-rank but have non-uniform rows that do not match the closed, directional, or cluster prototypes. Such maps may arise when a head computes a smooth global weighting rather than a uniform average.

**Task-specific anchors.** A head may attend to anchors defined by content rather than position. Across inputs, the anchor position changes, so the attention map family is not captured by a fixed positional atom. Such regimes may need content-conditioned dictionary atoms.

The residual is the mechanism that reveals these cases.

<a id="section-7-37"></a>
### 7.37 Dependency map for later chapters

The reason this chapter is long is that later chapters need a safe foundation. This section records which results should be cited for which future purpose.

<a id="pdf-0276"></a>
## PDF 276 · Book 240

<a id="section-7-37-1"></a>
#### 7.37.1 For pruning

Use:

$$
\Lambda_{\mathrm{closed}},\qquad
\rho_8,\qquad
\widehat\gamma,\qquad
\text{task sensitivity}.
$$

Cite residual stability, layer geography, and signature-aware pruning. Do not cite hard canonicalisation.

<a id="section-7-37-2"></a>
#### 7.37.2 For robustness

Use geography drift

$$
\sum_\ell W_1\bigl(\mu_\ell(X),\mu_\ell(\widetilde X)\bigr).
$$

Cite head-permutation invariance and signature transport. If the robustness chapter uses logits, cite softmax-to-residual stability.

<a id="section-7-37-3"></a>
#### 7.37.3 For training dynamics

Use saddle escape, finite-noise exit, and signature convergence. The training chapter should not claim that gradient flow selects one of eight patterns. It may claim that gradient flow selects unstable directions whose projected signatures bias the early geography.

<a id="section-7-37-4"></a>
#### 7.37.4 For information flow

Use layer geography and auxiliary observables. Cone index, entropy, margin, and activation should be treated as components of a coherence profile. Do not use the old implication chain as a theorem.

<a id="section-7-37-5"></a>
#### 7.37.5 For architecture design

Use model comparison and task fingerprints. If an architecture lacks the regimes needed by a task, its signature geography will show low mass in the relevant coordinates or high residual in the layers where those mechanisms are expected.

<a id="section-7-37-6"></a>
#### 7.37.6 For discovery of new mechanisms

Use residual clustering and dictionary expansion. A later chapter can propose a ninth regime only if it reduces residuals, has a structural definition, and passes a causal test.

<a id="section-7-37-7"></a>
#### 7.37.7 For empirical chapters

Use the reporting standard of Section 7.34. Every signature plot should report dictionary, metric, residual normalisation, and solver.

<a id="section-7-38"></a>
### 7.38 Metric and transport geometry of signatures

The previous sections defined the signature state space and the layer geography. The argument now make the metric layer explicit. A local signature is a point of

$$
\Sigma_R=\Delta_8\times[0,R],
$$

where $\Delta_8$ carries the regime weights and the residual coordinate records non-canonical mass. The simplest metric is $\|\Lambda-\Lambda'\|_1+|\rho-\rho'|$. It is useful for first visualisation, but it is not the most natural metric on a simplex.

---

[Continue: PDF 277–284, §7.38.1 Hellinger geometry](0277-0284-ch07-metric-geometry-and-canonical-replacement.md#pdf-0277)

