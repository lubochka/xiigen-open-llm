---
title: "Chapter 15: INCRT I — dynamic variables and introduction to the core controller"
source_pdf_pages: [547, 550]
book_pages: [511, 514]
batch: "011"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: false
continues_after_file: true
---

# Chapter 15: INCRT I — dynamic variables and introduction to the core controller

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: end of Chapter 14](0541-0546-ch14-static-closure-and-incrt-handoff.md#pdf-0546)

> Editorial note: Full transcription of PDF 547–550 (printed pages 511–514), including the chapter opening, equations 15.1–15.7, and Tables 15.1–15.2. The prose of §15.4 ends on PDF 550. Its references to Figures 15.1–15.3 and Table 15.3 are forward references: those objects lie beyond this batch and are not reproduced here. The controller's later definitions and proofs are not part of this page range.

<a id="pdf-0547"></a>
## PDF 547 · Book page 511

<a id="chapter-15"></a>
# Chapter 15
# INCRT I: From Static Rationalisation to Incremental Growth

<a id="section-15-1"></a>
## 15.1 From static rationalisation to incremental growth

Chapter 14 ended with a static architecture. The Rationalised Transformer made the geometric variables of the previous chapters explicit, measurable, and programmable. It separated symmetric and directional channels, exposed residual amplification, introduced marginal value as a preservation criterion, and turned crowding and coverage into architectural quantities. What remained static was the choice of capacity itself. The number of heads, the allocation of symmetric and antisymmetric channels, the width of residual completion, and the size of the dictionary were still fixed before the training trajectory had revealed the geometry of the task.

This chapter removes that last static choice. The Incremental Transformer (INCRT) is the dynamic version of the Rationalised Transformer. It keeps the static block of Chapter 14, but equips it with a controller that monitors the signature geography during training and performs three kinds of architectural actions:

<a id="display-0547-three-actions"></a>
$$
\text{growth},\qquad \text{tracking},\qquad \text{termination}.
$$

The construction is therefore summarised by the identity

<a id="equation-15-1"></a>
$$
\text{INCRT}=\text{Rationalised Transformer}+\text{growth}+\text{tracking}+\text{termination}.
\tag{15.1}
$$

The original INCRT formulation was application-driven: it was designed to work on tasks where a fixed architecture either over-allocated capacity or missed residual directional structure. The present formulation does not replace that operational core. It explains it. The dynamic controller developed below shows which static quantities are tracked, which residuals trigger growth, which channels must be preserved, and why the process terminates rather than expanding without bound. More extensive variants of the original INCRT theory—including compressed-sensing interpretations, persistent drift, adaptive optimisers, expert counts, transfer, and PAC-style guarantees—are kept for the following chapter. The present chapter is the core dynamic architecture.

<a id="pdf-0548"></a>
## PDF 548 · Book page 512

**Static closure and dynamic opening.** The last section of Chapter 14 described the Rationalised Transformer as a static programming interface:

<a id="display-0548-static-interface"></a>
$$
\text{measure}\longrightarrow\text{diagnose}\longrightarrow
\text{intervene}\longrightarrow\text{validate}.
$$

INCRT adds one further arrow:

<a id="display-0548-controller-arrow"></a>
$$
\text{validate}\longrightarrow\text{grow or stop}.
$$

Thus the static interface becomes a dynamic controller. A head is not added because a hyperparameter says that a layer should have more heads. It is added because the current dictionary fails to cover a measurable region of the signature geography, because residual amplification remains above tolerance, or because a marginal response cannot be assigned to any existing channel without crowding.

<a id="section-15-2"></a>
## 15.2 The original INCRT as an operational principle

The split of the INCRT material into two chapters should not be read as a correction of the original formulation. The original INCRT was successful because it embodied a simple and robust engineering idea: a model should begin with a controlled amount of capacity, monitor the residual structure that the current architecture cannot capture, and add capacity only when that residual is stable, useful, and not already represented. This principle is kept intact. It is now rewritten in the language of the Rationalised Transformer so that each operation has a measurable geometric interpretation.

The old operational loop may be summarised as

<a id="display-0548-operational-loop"></a>
$$
\text{train}\longrightarrow\text{measure residual demand}\longrightarrow
\text{expand if needed}\longrightarrow\text{stabilise}\longrightarrow
\text{stop or prune}.
$$

The new formulation keeps this loop but replaces an undifferentiated residual by the dynamic state of the previous section. Residual energy is no longer treated as a scalar alarm. It is decomposed into coverage residual, residual amplification, crowding, marginal value, and directional allocation. This is what allows the controller to distinguish between four situations that the older formulation could sometimes mix together:

1. an actually uncovered region of signature space;
2. an already covered region with too much crowding;
3. a high residual that has marginal value and should trigger growth;
4. a high residual that has no marginal value and should not trigger unbounded expansion.

The core message is therefore not that the old INCRT is replaced. The old INCRT worked because it followed the right operational law. The present chapter makes that law explicit: growth should occur exactly where the static programming interface of Chapter 14 reports stable under-coverage, useful residual amplification, or unresolved marginal response.

<a id="section-15-3"></a>
## 15.3 Dynamic variables

The dynamic chapter begins with the static quantities of the previous chapter. For every layer $\ell$, the Rationalised Transformer supplied a signature distribution, a dictionary of active atoms, coverage and crowding diagnostics, residual amplification, marginal values, and channel allocations. INCRT makes each of these quantities time-dependent.

<a id="pdf-0549"></a>
## PDF 549 · Book page 513

<a id="table-15-1"></a>
| Original INCRT idea | Rationalised dynamic reading | Effect in this chapter |
|---|---|---|
| Residual demand | $\rho_\ell(t)$ and $\Gamma_{\mathrm{res}}^{(\ell)}(t)$ | Growth is triggered only by stable, useful residual structure. |
| Incremental expansion | Add, split, or widen a rationalised component | Growth is local in signature space rather than global in parameter space. |
| Useful unit | High $V_{\mathrm{marg},h}(t)$ | Important heads are preserved even when crowded. |
| Directional component | High $\alpha_h(t)$ with marginal value | Directional tails become protected anchors. |
| Compression | Low marginal value plus high crowding | Pruning is geometry-aware rather than norm-based. |
| Stopping | Tolerances on $\rho_\ell$, $\Gamma_{\mathrm{res}}$, and $V_{\mathrm{marg}}$ | Termination is an architectural criterion, not a manual decision. |

**Table 15.1:** How the original operational INCRT principle is re-expressed through Chapter 14. The new formulation preserves the working logic of INCRT while making the controller measurable and programmable.

The notation below follows the static chapter. Let $\sigma_h^{(\ell)}(t)$ denote the current signature of head $h$ in layer $\ell$. The moving empirical signature distribution is

<a id="equation-15-2"></a>
$$
\mu_\ell(t)=\frac1{H_\ell(t)}
\sum_{h=1}^{H_\ell(t)}\delta_{\sigma_h^{(\ell)}(t)},
\tag{15.2}
$$

where $H_\ell(t)$ is the current number of heads at layer $\ell$. The current dictionary is

<a id="equation-15-3"></a>
$$
\mathcal D_\ell(t)=
\{a_1^{(\ell)}(t),\ldots,a_{K_\ell(t)}^{(\ell)}(t)\},
\tag{15.3}
$$

with atoms in the same signature space as the heads. The coverage residual is

<a id="equation-15-4"></a>
$$
\rho_\ell(t)=\int d\bigl(\sigma,\mathcal D_\ell(t)\bigr)^2\,d\mu_\ell(t)(\sigma),
\qquad d(\sigma,\mathcal D)=\min_{a\in\mathcal D}\|\sigma-a\|.
\tag{15.4}
$$

The residual amplification score is

<a id="equation-15-5"></a>
$$
\Gamma_{\mathrm{res}}^{(\ell)}(t)
=\frac{\|R_\ell(X_t)\|}{\|X_t\|+\epsilon},
\tag{15.5}
$$

where $R_\ell$ is the residual contribution of the layer on the current growth window and $\epsilon>0$ prevents division by zero. The marginal value of a head is estimated by controlled ablation or by a differentiable proxy:

<a id="equation-15-6"></a>
$$
V_{\mathrm{marg},h}^{(\ell)}(t)
=\mathcal L_t(B_\ell\setminus h)-\mathcal L_t(B_\ell),
\tag{15.6}
$$

possibly augmented by coverage, residual, and intervention terms as in the static programming interface.

**Directional task complexity index.** The original INCRT text used a spectral expression to estimate the number of incremental capture steps. In this volume the quantity is not called a spectral condition number, because it is not a condition number in the classical numerical-analysis sense. It is a *directional task complexity index*. In one simplified residual-energy model it may be written as

<a id="equation-15-7"></a>
$$
\kappa_T=\frac{\Gamma_0}{\delta_0},
\tag{15.7}
$$

<a id="pdf-0550"></a>
## PDF 550 · Book page 514

<a id="table-15-2"></a>
| Static quantity | Dynamic INCRT quantity | Dynamic role |
|---|---|---|
| Signature distribution $\mu_\ell$ | Moving distribution $\mu_\ell(t)$ | Empirical geometry tracked on a growth window. |
| Dictionary $\mathcal D_\ell$ | Moving dictionary $\mathcal D_\ell(t)$ | Set of currently represented signature atoms. |
| Coverage residual $\rho_\ell$ | $\rho_\ell(t)$ | Primary trigger for adding or splitting heads. |
| Crowding $C_{\mathrm{orb}}^{(\ell)}$ | $C_{\mathrm{orb}}^{(\ell)}(t)$ | Prevents growth from creating redundant heads. |
| Residual amplification $\Gamma_{\mathrm{res}}^{(\ell)}$ | $\Gamma_{\mathrm{res}}^{(\ell)}(t)$ | Detects unresolved residual energy and excessive injection. |
| Marginal value $V_{\mathrm{marg},h}$ | $V_{\mathrm{marg},h}(t)$ | Protects useful heads and identifies prunable ones. |
| Directional ratio $\alpha_h$ | $\alpha_h(t)$ | Tracks symmetric–directional allocation. |
| Residual gate $g_h$ | $g_h(t)$ | Controls the strength of injected residual computation. |
| Static intervention | Growth/pruning event | Converts diagnosis into architecture update. |

**Table 15.2:** Static quantities inherited from Chapter 14 and their dynamic counterparts in INCRT.

where $\Gamma_0$ is the initial residual directional energy and $\delta_0$ is the smallest reliably capturable directional component. This index estimates how many incremental capture steps are needed before the residual falls below tolerance. It should not be interpreted as a universal numerical condition number.

<a id="section-15-4"></a>
## 15.4 Visual summary of the core controller

The dynamic controller can be represented by three diagrams that were already present in the updated INCRT material. They are retained here because they make the core mechanism easier to read before the more extended theory of the next chapter.

Figure 15.1 should be read together with Table 15.3. The controller does not react to a single large residual value. It waits until the residual is stable, marginally useful, and geometrically under-covered. This is why INCRT differs from a naive “add-capacity-when-loss-is-high” rule. Loss alone is not the trigger; the trigger is the conjunction of loss-relevant residual demand and signature under-coverage.

Figure 15.2 expresses the main conceptual point of the chapter. INCRT is not an independent mechanism attached to a Transformer after the fact. It is the time-dependent version of the same geometry that produced the Rationalised Transformer. Static rationalisation gives the coordinates; incrementalisation turns those coordinates into events.

Figure 15.3 also clarifies the division between Chapters 15 and 16. The current chapter proves the core growth controller inside fixed layers. The following chapter restores the more expansive quadrittico material: three-level dynamics, homeostasis, transfer, and statistical guarantees. This keeps the main INCRT mechanism readable while preserving the full scope of the original theory.

<a id="batch-011-boundary"></a>
> Editorial boundary: Batch 011 ends at the complete sentence “of the original theory.” The prose of §15.4 has been transcribed; its referenced figures are still pending. PDF 551 begins with Figures 15.1 and 15.2, then §15.5, “The INCRT state.” No text, equations, or figures from PDF 551 are included here. Chapter 15 remains incomplete.


> Continuation published in batch 012: [Figures 15.1–15.2 and §15.5 on PDF 551](0551-0555-ch15-state-events-and-diagnostic-regularity.md#pdf-0551). Chapter 15 is now complete through PDF 592. The boundary note above records the original scope of batch 011.
