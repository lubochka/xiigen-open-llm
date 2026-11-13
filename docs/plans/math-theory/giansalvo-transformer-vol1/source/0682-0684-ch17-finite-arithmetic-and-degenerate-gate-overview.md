---
title: "Chapter 17: finite arithmetic, spectral degeneracy, and the unified gate overview"
type: source-transcription
language: en
batch: "014"
source_pdf_pages: [682, 684]
book_pages: [646, 648]
---

# Chapter 17: finite arithmetic, spectral degeneracy, and the unified gate overview

[Library index](../README.md) · [Source and transcription policy](../SOURCE.md)

[Previous source](0679-0681-ch16-diffusion-calibration-and-concluding-conjectures.md#pdf-0681) · [Following source](0685-0688-ch17-gate-rates-rayleigh-noise-and-orthogonality.md#pdf-0685)

<a id="pdf-0682"></a>
## PDF 682 · Book page 646

<a id="chapter-17"></a>
# Chapter 17. Spectral Gate Dynamics: Degeneracy Regimes and the Unified Gate

<a id="section-17-1"></a>
## 17.1 From continuous time to finite arithmetic

Chapter 15 introduced the dynamic INCRT controller as the mechanism that turns the static rationalisation of Chapter 14 into an incremental architecture. In that chapter, the bidirectional gate is defined as the core allocation device: positive probes are driven toward dominant residual directions, negative probes detect trailing directions, and growth, tracking, pruning, and termination are coordinated by the homeostatic controller. Chapter 16 then extends this controller beyond the ideal core setting, including moving targets, block gates, centered scores, universal counts, Adam-like preconditioning, PAC-style guarantees, non-greedy deflation, rank-adaptive variants, deployment constraints, in-context learning, and diffusion-like families. The present chapter has a narrower role. It is the finite-arithmetic completion of the gate: the same controller is retained, but the spectral subroutine is made robust when the residual spectrum is exactly degenerate, nearly degenerate, or numerically indistinguishable from a kernel.

The continuous-time gate analysis of Chapter 15 is correct in its stated regime. It describes Robbins–Monro flows on the unit sphere, with smooth dynamics, exact deflation, and a strictly positive gap

<a id="display-0682-positive-leading-gap"></a>
$$
\Delta_+=\lambda_1(A_{\rm res})-\lambda_2(A_{\rm res})>0
$$

between the dominant and second eigenvalues of the residual operator. The extended results of Chapter 16 show how the same logic survives several non-idealities, provided the relevant spectral and statistical hypotheses remain controlled. In floating-point arithmetic, however, three additional effects have to be separated explicitly: the loss of a unique eigenvector under multiplicity, the blow-up of iteration counts and eigenvector sensitivity under small gaps, and the possibility that some deflated directions are numerical kernel directions rather than structural heads. The present chapter asks what these errors are, how they accumulate, and what algorithmic response keeps the INCRT framework intact when the gap is small, absent, or below the numerical detection threshold.

The architecture of an Incremental Transformer — the depth $L$, the per-layer width $K_w$, and the per-head dimension — is determined by the spectral geometry of the task operator

<a id="display-0682-signed-task-operator"></a>
$$
\mathcal B=\operatorname{sym}(X^\top XM_a).
$$

<a id="pdf-0683"></a>
## PDF 683 · Book page 647

Two algorithmic ingredients realise this determination. First, the bidirectional gate of Chapter 15 allocates one head per Level-1 growth event, sending $u^+\to v_1(A_{\rm res})$ and $u^-\to v_d(A_{\rm res})$ via Oja’s rule and MCA EXIN respectively. Second, the block mechanisms developed in Chapter 16 allocate $m$ heads simultaneously when the dominant eigenspace has multiplicity $m>1$. The present chapter reconciles these two viewpoints: a block gate is not only an architectural extension, but also the natural numerical response when a single-vector gate cannot identify a unique stable direction.

The question addressed below is what happens when the spectral-gap hypothesis of the core gate is violated or stressed — that is, when the residual operator exhibits one of three regimes that the original controller did not need to separate explicitly.

**Exact degeneracy.** The dominant eigenvalue has multiplicity $m\ge2$. The single-vector limit of Oja’s rule is not well-defined; only the $m$-eigenspace is. Block MCA EXIN handles this case at the algorithmic level, but the regime has to be detected and dispatched before single-vector convergence is requested.

**Near-degeneracy.** $\Delta_+>0$ strictly, but small relative to $\lambda_1$. The convergence rate of Oja’s rule satisfies $\lambda_2/\lambda_1\to1$, so the iteration count required to reach a prescribed precision diverges as $\Delta_+\to0$. Independently, the sensitivity of the dominant eigendirection to operator-level perturbations grows as $1/\Delta_+$ by Davis–Kahan, so the gate estimate inherits an amplification factor that is invisible when the limit $t\to\infty$ is taken before the limit $\Delta_+\to0$.

**Kernel directions.** One or more eigenvalues are zero, or below the floating-point detection threshold. The Rayleigh quotient $\widetilde y/\|u\|^2$ tends to zero and the MCA EXIN iteration becomes singular. The growth threshold $\theta_w$ in Chapter 15 implicitly excludes these directions from accepted growth events, while the deployment and quantisation analysis of Chapter 16 shows that numerical floors must be treated as part of the effective architecture. This chapter makes the distinction explicit: a direction may be deflated without corresponding to a structural head.

The three regimes correspond to three qualitatively different geometries of the residual operator, each calling for a different algorithmic response. That response is developed by classifying the regimes (§17.3), assembling a toolkit of classical spectral methods adapted to the online INCRT setting (§17.4), composing them into a single homeostatic algorithm that dispatches on the detected regime (§17.5), and illustrating the framework on small but informative spectra together with a retrospective reading of the SARS-CoV-2 trajectory of Chapter 18 (§17.6). The partial-resolution section §17.7 turns several operational open questions into usable design rules, while the closing §17.8 catalogues the remaining structural limitations and open questions.

The toolkit assembled in §17.4 is entirely classical, with references reaching back to Lanczos (1950) and Wilkinson (1965). What is new in the present context is the composition: a single algorithm (Algorithm 7) selects the appropriate spectral method at each growth event, governed by the relative gap test of Definition 17.6. The composition recovers the bidirectional gate of Chapter 15 as a limiting case in the regular regime and recovers the block logic of Chapter 16 when an eigenspace rather than a single eigendirection has to be learned. The unified gate is therefore a strict refinement: numerical robustness is improved without altering the algorithmic logic where the original framework already applies.

The convergence theorem of the unified gate (Theorem 17.11) should be read conditionally. It generalises the block-gate result of Chapter 16 to the three numerical regimes above, with

<a id="pdf-0684"></a>
## PDF 684 · Book page 648

an explicit bias bound proportional to the regularisation amplitude $\varepsilon_{\rm reg}$. At the default value $\sqrt{\varepsilon_{\rm mach}}\,\|A_{\rm res}\|$, this bias falls below the other numerical errors in the gate dynamics, but the basin-entry and online-classification assumptions are stated explicitly rather than hidden in the proof.

A note on the chain of hypotheses is useful. The core controller of Chapter 15 is built on the static regularity hypotheses prepared in Chapter 14, while the extensions of Chapter 16 specify how these hypotheses are modified under moving targets, preconditioners, rank constraints, and finite budgets. The unified gate does not replace those assumptions. It modifies only the spectral response of the gate when the residual spectrum enters a degenerate or near-degenerate regime. Theorem 17.11 therefore inherits the same input domain as the core and extended INCRT results, with additional numerical hypotheses made explicit in §17.5.3.

[Continue reading](0685-0688-ch17-gate-rates-rayleigh-noise-and-orthogonality.md#pdf-0685).
