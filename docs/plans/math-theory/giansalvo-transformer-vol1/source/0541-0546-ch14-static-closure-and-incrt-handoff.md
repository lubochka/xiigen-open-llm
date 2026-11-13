---
title: "Chapter 14: static closure, allocation, gate stability, and the handoff to INCRT"
source_pdf_pages: [541, 546]
book_pages: [505, 510]
batch: "011"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: false
---

# Chapter 14: static closure, allocation, gate stability, and the handoff to INCRT

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: PDF 535–540](0535-0540-ch14-illustrations-implementations-and-boundaries.md#pdf-0540)

> Editorial note: Full transcription of PDF 541–546 (printed pages 505–510), completing Chapter 14. Definitions, propositions, proofs, the gate lemma, and Table 14.11 are retained in full. PDF 546 contains the chapter's final takeaway. Statements about closure and future guarantees remain the author's statements; their conditions and limitations are discussed separately.

<a id="pdf-0541"></a>
## PDF 541 · Book page 505

<a id="section-14-14"></a>
## 14.14 Resolution of the architectural degrees of freedom

The purpose of this section is to collect the closure choices made in the chapter. The preceding sections leave no separate catalogue of open architectural problems inside the chapter. Instead, each potentially ambiguous point is resolved at the static level by a definition, a constrained optimisation problem, or a proposition under explicit hypotheses. This is the appropriate status for a chapter whose role is to define the fixed architecture that the next chapter will make incremental.

<a id="section-14-14-1"></a>
### 14.14.1 Minimality within the rationalised class

Minimality cannot mean that no other neural architecture could solve the same task. It means something more precise: within the class of residual attention blocks that must expose signature coverage, S–A separation, crowding control, marginal value, and residual amplification, no component of the rationalised block is redundant.

<a id="definition-14-21"></a>
**Definition 14.21 (Rationalised static class).** A static residual-attention block belongs to the *rationalised class* if it contains: (i) effective representatives of the layer signature dictionary; (ii) an explicit or recoverable S–A split; (iii) a crowding penalty or assignment rule; (iv) marginality-aware gates or equivalent preservation weights; and (v) a residual amplification constraint or penalty.

<a id="proposition-14-22"></a>
**Proposition 14.22 (Relative minimality).** In the rationalised static class, the rationalised Transformer block is minimal up to implementation equivalence: removing dictionary representatives breaks coverage, removing the S–A split removes directional-tail control, removing crowding control permits duplicate allocation, removing marginal gates removes head-value selection, and removing residual control leaves $\Gamma_{\mathrm{res}}$ unmanaged.

*Proof.* Each listed component is the unique carrier of one required diagnostic constraint in Definition 14.21. If dictionary representatives are removed, Proposition 14.14 no longer gives coverage. If the S–A split is removed, the architecture no longer distinguishes reciprocal and directional updates, despite the algebraic decomposition of the motor. If crowding control is removed, two heads may occupy the same atom without penalty. If marginal gates are removed, pruning and preservation cannot depend on value. If residual control is removed, the residual stream receives updates without a bound tied to $\Gamma_{\mathrm{res}}$. Thus every component is necessary relative to the stated class. Different implementations may realise the same components, but the constraints themselves cannot be deleted. $\square$

This resolves the minimality question at the level needed for the book: the block is not claimed to be globally unique among all architectures; it is minimal among architectures that agree to satisfy the geometric requirements developed in the preceding chapters.

<a id="section-14-14-2"></a>
### 14.14.2 S–A allocation

The amount of capacity assigned to the symmetric, antisymmetric, and mixed channels is determined by a layer allocation problem. Let $p_S,p_A,p_M\ge0$ with $p_S+p_A+p_M=1$ denote the fractions of attention capacity assigned to the three channels. Let

<a id="display-0541-channel-errors"></a>
$$
E_S(p_S),\quad E_A(p_A),\quad E_M(p_M)
$$

be decreasing approximation errors estimated from the symmetric, directional, and interaction residuals. The static allocation is

<a id="display-0541-allocation-objective"></a>
$$
(p_S^\star,p_A^\star,p_M^\star)
=\underset{p_S+p_A+p_M=1}{\operatorname{argmin}}
\bigl(E_S(p_S)+E_A(p_A)+E_M(p_M)+\eta\,\Phi(p_S,p_A,p_M)\bigr),
$$

<a id="pdf-0542"></a>
## PDF 542 · Book page 506

where $\Phi$ is a mild regulariser preventing degenerate channel starvation.

<a id="proposition-14-23"></a>
**Proposition 14.23 (Water-filling allocation rule).** If $E_S,E_A,E_M$ are differentiable, convex, and decreasing on $(0,1]$, then any interior optimum satisfies

<a id="display-0542-water-filling"></a>
$$
-E_S'(p_S^\star)=-E_A'(p_A^\star)=-E_M'(p_M^\star),
$$

after including the derivative of the regulariser. Capacity is therefore assigned until the marginal reduction of residual error is equalised across channels.

*Proof.* This is the Karush–Kuhn–Tucker condition for a convex optimisation problem on the simplex. The Lagrange multiplier associated with $p_S+p_A+p_M=1$ is equal to each active marginal derivative, with the regulariser included in the corresponding derivative. Boundary solutions are handled by the usual inequality conditions. $\square$

The S–A allocation is therefore not arbitrary. It is fixed by measured residual errors and their marginal decrease under capacity.

<a id="section-14-14-3"></a>
### 14.14.3 Feed-forward design

The feed-forward layer is resolved by residual completion. Attention transports and mixes token information; the feed-forward map completes the unresolved coordinate-wise and channel-wise transformations. In the rationalised block it is written as

<a id="display-0542-feedforward-split"></a>
$$
R_{\mathrm{ff}}(X)=R_{\mathrm{ff},S}(X)+R_{\mathrm{ff},A}(X)+R_{\mathrm{ff},M}(X),
$$

with optional parameter sharing when one component has low marginal value. The feed-forward width assigned to each component follows the same water-filling rule as Proposition 14.23. This resolves whether the FFN should be unique or split: it is conceptually split by diagnostics, and implementation may share parameters only when the measured marginal loss of sharing is below tolerance.

<a id="section-14-14-4"></a>
### 14.14.4 Gate stability

Marginality-aware gates must not collapse prematurely. The static block therefore uses bounded gates with floors:

<a id="display-0542-gate-floor"></a>
$$
g_h=g_{\min}+(g_{\max}-g_{\min})\,\sigma(u_h),
\qquad 0<g_{\min}<g_{\max}<\infty.
$$

The gate variable $u_h$ may depend on marginal value, crowding, residual energy, and S–A ratio, but the floor prevents head starvation before the pruning criterion has been evaluated.

<a id="lemma-14-24"></a>
**Lemma 14.24 (No premature gate deletion).** With $g_{\min}>0$, every head receives a non-zero residual path during the diagnostic phase. A head therefore cannot be deleted solely because its gate has accidentally reached zero. Deletion requires an explicit pruning decision based on marginal value and coverage preservation.

*Proof.* The formula for $g_h$ gives $g_h\ge g_{\min}>0$ for all finite $u_h$. Hence the head remains active in the residual path. A zero contribution can occur only after a separate pruning operation removes the head or sets its effective output to zero. $\square$

This closes the gate-stability issue in the static chapter. Dynamic birth and death of heads is deliberately postponed to INCRT, where it becomes a controlled growth-and-termination mechanism rather than an accidental gate collapse.

<a id="pdf-0543"></a>
## PDF 543 · Book page 507

<a id="section-14-14-5"></a>
### 14.14.5 Subsumption and approximation status

The subsumption question is resolved explicitly in Section 14.6. The standard MHA+FFN block is recovered as a degenerate case, prototype-gated projection and geometrically allocated multi-head blocks are stated as concrete instances, and universal approximation is inherited only under the assumptions of the corresponding standard theorem. No unrestricted subsumption claim is therefore used in the chapter.

<a id="section-14-14-6"></a>
### 14.14.6 Static-to-incremental handoff

The last degree of freedom concerns the transition to INCRT. The static chapter resolves this by exporting four variables:

<a id="display-0543-exported-diagnostics"></a>
$$
\rho_\ell,\qquad C_{\mathrm{occ}}^{(\ell)},\qquad
\Gamma_{\mathrm{res}}^{(\ell)},\qquad V_{\mathrm{marg},h}.
$$

When these quantities are used only for allocation, the architecture is the rationalised Transformer. When the same quantities are used to create, split, prune, or terminate heads, the architecture becomes incremental. The transition is therefore not an open conceptual jump; it is a change in how the already defined diagnostics are acted upon.

<a id="section-14-14-7"></a>
### 14.14.7 Closed chapter statement

The chapter therefore closes the static architectural problem as follows.

1. $K_\ell^\star$ is defined as the empirical covering number of the layer signature geography.
2. Crowding is minimised by an explicit assignment objective.
3. Residual amplification is bounded by gated residual injection and monitored by $\Gamma_{\mathrm{res}}$.
4. S–A capacity is assigned by marginal residual-error reduction.
5. Feed-forward capacity is treated as diagnostic residual completion.
6. Gates are stabilised by floors and pruning is separated from accidental gate collapse.
7. The standard Transformer is recovered as a degenerate case.
8. INCRT is obtained by turning the same static diagnostics into growth and termination rules.

**Takeaway.** *The chapter resolves the static problems needed before INCRT. What remains for the next chapter is not the definition of the rationalised architecture, but the dynamics of changing it during training.*

<a id="section-14-15"></a>
## 14.15 Bridge to INCRT

The Rationalised Transformer is the static architecture obtained by enforcing the diagnostics of the previous chapters at block level. It is not yet the Incremental Transformer. The distinction is essential. In the present chapter, the number of heads, the channel dimensions, and the dictionary resolution are fixed before or during a static design phase. In INCRT, these quantities become time-dependent and are updated by growth and termination rules.

<a id="section-14-15-1"></a>
### 14.15.1 What the static block fixes

A static rationalised block fixes the following objects:

<a id="display-0543-static-objects"></a>
$$
H_\ell,\quad \mathcal H_{S,\ell},\quad \mathcal H_{A,\ell},\quad
\mathcal H_{M,\ell},\quad \mathcal D_\ell,\quad
G_{\mathrm{att}}^{(\ell)},\quad G_{\mathrm{ff}}^{(\ell)},\quad
d_{\mathrm{ff}}^{\mathrm{rat}}.
$$

<a id="pdf-0544"></a>
## PDF 544 · Book page 508

These choices may depend on a preliminary estimate of the signature geography, but they do not change by adding new heads during training. The block can detect insufficiency through $\rho_\ell$, $C_{\mathrm{orb}}^{(\ell)}$, and $\Gamma_{\mathrm{res}}^{(\ell)}$; it cannot by itself create new capacity.

This is why the chapter should end before introducing growth. If growth were included here, the conceptual distinction between rationalisation and incrementality would be lost. Rationalisation is the static allocation principle. Incrementality is the dynamic mechanism that modifies allocation during training.

<a id="section-14-15-2"></a>
### 14.15.2 What INCRT adds

INCRT adds three mechanisms.

**Growth.** When coverage residual or residual directional energy remains above tolerance, a new head or channel component may be introduced. The new component is not arbitrary; it is placed near the uncovered or under-served region of signature space.

**Tracking.** The dictionary and signature geography are allowed to move during training. The target is therefore not fixed. INCRT tracks a moving geometry rather than merely fitting a static allocation.

**Termination.** Growth must stop. The stopping rule is based on saturation of coverage, reduction of residual energy, and stabilisation of marginal value. Without termination, incrementality would become uncontrolled expansion.

The relationship can be written compactly as

<a id="display-0544-incrt-identity"></a>
$$
\boxed{\text{INCRT}=\text{Rationalised Transformer}+\text{growth}+\text{tracking}+\text{termination}}.
$$

This identity is conceptual rather than algebraic, but it captures the organisation of the monograph.

<a id="section-14-15-3"></a>
### 14.15.3 Static-to-dynamic correspondence

<a id="table-14-11"></a>
| Rationalised Transformer | INCRT |
|---|---|
| fixed number of head slots $H_\ell$ | time-dependent head count $H_\ell(t)$ |
| fixed dictionary $\mathcal D_\ell$ | moving dictionary $\mathcal D_\ell(t)$ |
| static S–A allocation | adaptive S–A allocation |
| coverage residual as diagnostic | coverage residual as growth trigger |
| $\Gamma_{\mathrm{res}}^{(\ell)}$ as allocation signal | $\Gamma_{\mathrm{res}}^{(\ell)}(t)$ as tracking signal |
| marginal value as pruning/retention criterion | marginal value as growth and termination criterion |
| static sufficiency | dynamic convergence or stabilisation |

**Table 14.11:** Correspondence between the static rationalised Transformer and the dynamic INCRT construction.

<a id="section-14-15-4"></a>
### 14.15.4 Prepared hypotheses for the next chapter

The next chapter requires the following hypotheses, prepared here but not proved here.

1. The diagnostic estimates of $\mu_\ell(t)$, $\Gamma_{\mathrm{res}}^{(\ell)}(t)$, and marginal value are sufficiently stable over the update window used for growth decisions.

<a id="pdf-0545"></a>
## PDF 545 · Book page 509

2. Adding a head near an uncovered signature region reduces the corresponding coverage residual in expectation.
3. Growth does not destroy previously stabilised directional tails, provided the S–A allocation constraints are preserved.
4. Termination occurs when residual improvements fall below tolerance across all monitored channels.

These are dynamic statements. They belong to the stochastic-approximation and tracking analysis of INCRT. The present chapter supplies only the static fixed point that such an analysis attempts to reach.

<a id="section-14-15-5"></a>
### 14.15.5 Outlook items not deferred to the next chapter

Not every open question raised by the chapter is exported to Chapter 15. Two items are outlook for empirical or theoretical work outside the monograph; they are listed here so that the boundary between the static chapter, the dynamic chapter, and external work is explicit.

**Empirical dominance at equal compute.** Whether the rationalised block, in its full form, attains lower task loss than the standard MHA+FFN block at equal parameter count and equal training compute is an empirical question. Section 14.12 specifies the minimal full-model protocol that would settle it; the protocol is BERT-base scale, four architectures (standard, prototype-gated projection, rationalised, and a comprehensive instance), four geometric KPIs (coverage residual, residual directional energy, principal angles, orbit diversity), and standard task metrics. The execution of this protocol is left to companion experimental work. A null result (rationalised matches standard accuracy with better diagnostics) already supports the chapter’s central claim, which is architectural control rather than benchmark dominance.

**Truly task-uniform lower bound on $K^\star$.** Proposition 14.17 gives a lower bound that is uniform within a fixed task class $\mathcal T$. A stronger bound that holds uniformly over arbitrary task classes, parameterised by an intrinsic complexity measure of the task family, is left as an outlook item. It would require a notion of intrinsic complexity for transformer-relevant task families that is independent of the chosen diagnostic metric. The covering-number argument used here does not extend automatically to such a setting, and the question is more naturally addressed in a dedicated theoretical paper.

These two items are mentioned not to extend the chapter’s scope but to fix its boundary. The static chapter is closed; Chapter 15 addresses the dynamic items; the two outlook items above are addressed elsewhere.

<a id="section-14-15-6"></a>
### 14.15.6 Closing statement

The chapter has converted the theory of signatures, S–A decomposition, residual energy, redundancy, and marginal value into a concrete static architecture. The result is not a wholesale replacement of the Transformer template. It is a rationalised version of that template: attention is decomposed into symmetric, directional, and mixed channels; aggregation is gated by diagnostic quantities; feed-forward capacity is interpreted as non-linear completion of unresolved residual directions; and head allocation is governed by coverage, crowding, and marginal value.

This is the appropriate stopping point for the static theory. Once the block can say where it is insufficient, the natural next step is to let it grow. That step is the subject of INCRT.

<a id="pdf-0546"></a>
## PDF 546 · Book page 510

**Takeaway.** *The Rationalised Transformer is the static fixed architecture. INCRT is obtained when the same diagnostics are promoted from monitoring variables to growth, tracking, and termination rules.*

<a id="chapter-14-end"></a>
> Editorial boundary: Chapter 14 ends here. [Chapter 15 begins on PDF 547](0547-0550-ch15-dynamic-variables-and-controller-introduction.md#chapter-15).
