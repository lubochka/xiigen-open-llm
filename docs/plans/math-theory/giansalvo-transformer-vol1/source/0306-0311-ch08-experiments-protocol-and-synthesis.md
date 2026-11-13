---
title: "Chapter 8: explanatory experiments, empirical protocol, and synthesis"
source_pdf_pages: [306, 311]
book_pages: [270, 275]
batch: "007"
language: en
type: source-transcription
status: complete-for-page-range
continues_after_file: true
---

# Chapter 8: explanatory experiments, empirical protocol, and synthesis

[Library index](../README.md) · [Source and transcription policy](../SOURCE.md) · [Previous pages: PDF 301–305](0301-0305-ch08-compatibility-bounds-and-signature-transport.md#pdf-0305)

> Editorial note: Full transcription of PDF 306–311 (printed pages 270–275), including Table 8.1, both finite-head input tables, the seven-row diagnostic reinterpretation table, and all displayed formulas. The final sentence on PDF 311 is visibly truncated in the supplied PDF at “architecture rationali”; it is preserved without an inferred ending. Chapter 9 begins on PDF 312. Synthetic experiments and the author's claim that scripts/data accompany a package are source statements, not newly supplied experiment artifacts.

<a id="pdf-0306"></a>
## PDF 306 · Book 270

<a id="table-8-1"></a>
**Table 8.1:** Synthetic explanatory diagnostics for the layer-geography experiment. The values are illustrative only.

| Quantity | Symbol | Value |
|---|---|---|
| Number of layers | $L$ | 12 |
| Mean inter-layer velocity | $\frac1{L-1}\sum_\ell v_\ell$ | 0.269 |
| Transition layer with maximal velocity | $\arg\max_\ell v_\ell$ | 6 |
| Mean residual across layers | $\frac1L\sum_\ell\bar\rho_\ell$ | 0.180 |
| Residual drop from first to last layer | $\bar\rho_1-\bar\rho_L$ | 0.160 |

<a id="section-8-14-4"></a>
### 8.14.4 Experiment D: transition observables

The synthetic transport path also yields the transition observables from Figure 8.11. The experiment is useful because it exhibits three distinct types of change: barycentric transport, residual change, and spread change. A single scalar drift cannot distinguish those regimes, whereas the triplet $(\Phi_\ell^\Lambda,\Phi_\ell^\rho,\Delta\operatorname{Spread}_\ell)$ can.

<a id="section-8-14-5"></a>
### 8.14.5 Illustrative summary table

Table 8.1 collects the main quantities extracted from the synthetic layer family.

<a id="section-8-14-6"></a>
### 8.14.6 Didactic use of the experiments

These compact examples perform a precise didactic role. Experiment A explains the notion of signature re-extraction. Experiment B clarifies the status of the entropy theorem. Experiment C explains why transport plans should not be confused with head identities. Experiment D prepares the interpretation of transition layers and later discussions of redundancy. Together, they make the global chapter easier to digest without burdening it with large-scale empirical infrastructure.

<a id="section-8-15"></a>
### 8.15 Empirical protocol for a real model

The synthetic experiments are only explanatory. For a real model, the following protocol gives the data required to instantiate the chapter.

<a id="section-8-15-1"></a>
#### 8.15.1 Per-head measurements

For each layer $\ell$, head $h$, input batch $B$, and sequence length convention, compute:

<a id="display-8-per-head-measurements"></a>
$$
\Lambda_{\ell,h},\qquad
\rho_{\ell,h},\qquad
H_{\ell,h},\qquad
\bar\Delta_{\ell,h},\qquad
\Gamma_{\ell,h},\qquad
I_{\mathrm{cone},\ell}.
$$

The first two are inherited from the canonical signature map. The next three are attention-level observables. The cone index is a representation-level observable and therefore attached to the layer rather than the individual head.

<a id="section-8-15-2"></a>
#### 8.15.2 Per-layer measurements

The empirical measure $\mu_\ell$ is constructed for each batch. Then $v_\ell=W_2(\mu_\ell,\mu_{\ell+1})$ is computed for consecutive layers. Repeating this over batches gives a distribution of velocities rather than a single number. Transition layers should be identified by robust statistics, such as median velocity and interquartile range.

<a id="pdf-0307"></a>
## PDF 307 · Book 271

<a id="section-8-15-3"></a>
#### 8.15.3 Per-transition measurements

For every transition $\ell\to\ell+1$, compute an optimal coupling and the derived quantities

<a id="display-8-per-transition-measurements"></a>
$$
D_\ell^\Lambda,\qquad D_\ell^\rho,\qquad\Delta\operatorname{Spread}_\ell.
$$

The signed barycentric drift should be plotted coordinate-wise. The residual drift should be compared to entropy and margin changes. Spread change should be interpreted with care: crowding may indicate redundancy, but it may also indicate functional consensus.

<a id="section-8-15-4"></a>
#### 8.15.4 Reporting standard

Every empirical use of the framework should report:

- the signature metric $d_\Sigma$ and the residual scale $\alpha$;
- the dictionary version used for the canonical projection;
- the sequence-length and masking convention;
- the batch distribution and task;
- the solver or approximation used for the Wasserstein coupling;
- confidence intervals over inputs or seeds when available.

Without these details, a transport plot is not reproducible.

<a id="section-8-16"></a>
### 8.16 Worked finite-head transport example

A small numerical example clarifies the meaning of optimal coupling. Suppose two adjacent layers each have four heads and the signatures are simplified to two canonical coordinates plus one residual coordinate:

$$
z_h=(\lambda_{h,1},\lambda_{h,2},\rho_h).
$$

Let the first layer contain

<a id="table-8-finite-head-first-layer"></a>
| $h$ | $\lambda_1$ | $\lambda_2$ | $\rho$ |
|---|---|---|---|
| 1 | 0.80 | 0.20 | 0.12 |
| 2 | 0.70 | 0.30 | 0.15 |
| 3 | 0.25 | 0.75 | 0.20 |
| 4 | 0.20 | 0.80 | 0.18 |

and the second layer contain

<a id="table-8-finite-head-second-layer"></a>
| $h'$ | $\lambda'_1$ | $\lambda'_2$ | $\rho'$ |
|---|---|---|---|
| 1 | 0.76 | 0.24 | 0.10 |
| 2 | 0.30 | 0.70 | 0.16 |
| 3 | 0.18 | 0.82 | 0.17 |
| 4 | 0.66 | 0.34 | 0.14. |

With Euclidean cost in this simplified space, the optimal assignment pairs head 1 with 1, head 2 with 4, head 3 with 2, and head 4 with 3. This assignment is geometrically natural: heads are paired by proximity in signature space, not by index.

The example shows why the word “matching” must be handled carefully. The matching is an optimal plan for comparing two empirical measures. It is not a statement that head 2 at layer $\ell$ has become head 4 at layer $\ell+1$. The latter statement has no invariant meaning. The invariant facts are the transport cost and the population-level changes obtained by integrating against the coupling.

<a id="pdf-0308"></a>
## PDF 308 · Book 272

<a id="section-8-16-1"></a>
#### 8.16.1 Derived quantities

From such a coupling one computes:

<a id="display-8-finite-head-drifts"></a>
$$
D_\ell^\Lambda=\frac14\sum_{h=1}^4(\Lambda'_{\sigma(h)}-\Lambda_h),
\qquad
D_\ell^\rho=\frac14\sum_{h=1}^4(\rho'_{\sigma(h)}-\rho_h).
$$

In the example, $D_\ell^\rho<0$, so the transition is mildly canonicalising. The signed barycentric drift indicates whether the first or second coordinate gained mass. If the same calculation is repeated across all layers, an interpretable depth-wise flow field is obtained.

<a id="section-8-16-2"></a>
#### 8.16.2 Why hard transition matrices are insufficient

A hard transition matrix would first assign each head to the largest coordinate, then count transitions. That procedure would lose three pieces of information: mixture strength, residual size, and geometric proximity inside the simplex. Transport keeps all three. Hard transitions can still be plotted as a coarse summary, but the transport computation is the primary object.

<a id="section-8-17"></a>
### 8.17 Reinterpretation of the older diagnostic figures

The previous version of this chapter contained several diagnostic figures. Their conceptual content remains useful, but the captions and interpretations must be updated.

<a id="table-8-diagnostic-reinterpretation"></a>
| Older diagnostic | Revised interpretation |
|---|---|
| Re-extraction loop | Signature re-extraction from a mixed residual representation. Head identity is not transported; layer geography is regenerated. |
| Channel-destruction flow | Head-gauge non-identifiability. Linear recoverability may hold in a fixed parametrisation, but canonical head identity does not. |
| Cone-growth diagram | Representation-alignment diagnostic. Cone index modulates available signatures but does not determine them. |
| SVD-alignment diagram | Singular concentration of the representation cloud; useful for predicting alignment conditions. |
| Margin-row diagram | Row-wise concentration mechanism behind the entropy envelope. |
| Entropy-decay plot | Conditional entropy-proxy control under margin growth. |
| Coherence-chain diagram | Compatibility profile, not a universal causal chain. |

This reinterpretation preserves the educational value of the old figures while removing the assumptions that are no longer acceptable after the signature framework of Chapter 7. In particular, no figure should be read as proving that each head implements one of the canonical patterns exactly. Each figure should instead be read as a diagnostic of one component of the re-extraction process.

<a id="section-8-17-1"></a>
#### 8.17.1 Caption rule

Whenever an older figure is reused, its caption should obey the following rule:

<a id="pdf-0309"></a>
## PDF 309 · Book 273

> The figure must describe distributions of signatures, compatibility profiles, or transport diagnostics. It must not describe exact hard-pattern inheritance across layers.

This rule is sufficient to prevent the old terminology from re-entering the chapter through visual explanations.

<a id="section-8-18"></a>
### 8.18 Re-extraction, redundancy, and design implications

Repeated re-extraction has an immediate side effect. If many layers extract geometrically similar populations of signatures, and many heads within those layers occupy the same region of signature space, redundancy appears as *signature crowding*. The phenomenon is not yet analysed in full detail here; that task belongs to the later chapters on redundancy and design. Still, the present chapter already identifies its geometric mechanism.

1. A mixed residual representation enters a new block.
2. The block extracts a fresh family of local signatures.
3. Only a subset of the theoretically available directions is used.
4. Similar extraction biases reappear in adjacent layers.
5. The empirical measures $\mu_\ell$ cluster in nearby regions of signature space.

The re-extraction picture therefore explains why redundancy should not be expected to vanish automatically in an over-parameterised Transformer. Since no canonical head identity is preserved across layers, architectural diversity is not guaranteed by the index set $\{1,\ldots,H\}$. Diversity must instead be read directly from the geometry of the empirical measures.

The same conclusion has design consequences. Transport peaks identify layers that genuinely reorganise the computation. Small transport together with persistent crowding suggests layers or heads that may be pruned, merged, or regularised. Residual increase identifies points where the current canonical dictionary may be too coarse. The diagnostic profile developed here is thus intended to connect immediately to pruning and rationalisation principles in later chapters.

<a id="section-8-19"></a>
### 8.19 Synthesis

The chapter can now be summarised succinctly.

1. The head index is not a canonical state variable. Proposition 8.2 shows that the layer map is invariant under head permutations, and Proposition 8.3 shows that the issue is not unconditional linear destruction of information.
2. The correct depth-wise state variable is the empirical signature geography $\mu_\ell$, inherited from Chapter 7.
3. Inter-layer information flow is described by transport between geographies, not by a literal tracking of labelled heads.
4. Cone alignment, directional activation, margins, entropy, and residuals form a compatibility profile. They constrain each other, but they do not constitute a universal deterministic chain.
5. Transition layers are revealed by peaks of the Wasserstein velocity and by the corresponding transition observables.

<a id="pdf-0310"></a>
## PDF 310 · Book 274

6. Repeated extraction from a mixed representation explains the emergence of signature crowding and prepares the study of redundancy.

It is therefore appropriate to read the static geography of Chapter 7 as the equilibrium of a depth-wise re-extraction process. The present chapter fixes the depth coordinate $\ell$ and studies the transition $\mu_\ell\mapsto\mu_{\ell+1}$. The next dynamical question concerns the training coordinate $t$: how does the extraction mechanism itself move during optimisation? That question leads naturally to the training-time analysis of Chapter 11.

<a id="legacy-integration-notes"></a>
### Legacy integration notes

The present chapter preserves the main valid insights of the previous Chapter 8 while altering their logical placement. The permutation symmetry discussion has been retained and sharpened. The generic recoverability argument has been reformulated as a statement against absolute channel-destruction rhetoric. The cone index and the singular-alignment discussion have been preserved as alignment diagnostics. The margin–entropy theorem has been retained in corrected form. The former coherence chain has been replaced by the weaker and more accurate compatibility-profile language. Finally, the earlier intuition that each layer must reconstruct structure from the representation inherited from the preceding block has been retained as the central thesis of signature re-extraction.

<a id="section-8-20"></a>
### 8.20 Interfaces with Subsequent Chapters

The present chapter supplies depth-wise machinery. Later chapters use it in different directions.

<a id="section-8-20-1"></a>
#### 8.20.1 Interface with the motor formalism

The motor formalism can treat $\mu_\ell\mapsto\mu_{\ell+1}$ as a mesoscopic transition. The re-extraction loop supplies the mechanism, and transport supplies the metric. The motor variables should therefore act on signature geographies rather than on hard pattern labels.

<a id="section-8-20-2"></a>
#### 8.20.2 Interface with training dynamics

Chapter 11 changes the independent variable. Here the transition is depth-wise:

<a id="display-8-depth-transition"></a>
$$
\mu_\ell(X)\longmapsto\mu_{\ell+1}(X).
$$

In training dynamics the transition is temporal:

<a id="display-8-training-transition"></a>
$$
\mu_\ell(t;X)\longmapsto\mu_\ell(t+\Delta t;X).
$$

The same metric layer can be used in both cases. The depth-wise velocity $v_\ell$ becomes a training-time velocity, and transition layers become transition phases.

<a id="section-8-20-3"></a>
#### 8.20.3 Interface with redundancy

Redundancy analysis requires distinguishing mean behaviour from population spread. A layer with many heads near the same signature region may be redundant even if its mean signature is informative. The spread observable and the transport coupling are therefore direct inputs to later redundancy measures.

<a id="pdf-0311"></a>
## PDF 311 · Book 275

<a id="section-8-20-4"></a>
#### 8.20.4 Interface with design

Design principles should not merely impose more heads or fewer heads. They should prescribe desired geography: sufficient spread, controlled residual, low pathological crowding, and transition layers positioned where reorganisation is useful. The chapter thus supplies the diagnostic vocabulary for architecture rationali

> Editorial source defect: The supplied PDF ends Chapter 8 with the unfinished word “rationali” and no final punctuation. PDF 312 starts Chapter 9; no continuation of this sentence is supplied at that boundary.

[Continue: Chapter 9, PDF 312–317](0312-0317-ch09-motor-taxonomy-and-related-work.md#pdf-0312) · [Russian note: finite-head example](../notes/82-measurement-protocol-and-finite-head-transport.md) · [Russian note: Chapter 8 synthesis](../notes/83-signature-crowding-and-chapter-eight-boundaries.md)
