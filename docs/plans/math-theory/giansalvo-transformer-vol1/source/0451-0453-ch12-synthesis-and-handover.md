---
title: "Chapter 12: synthesis and handover to redundancy theory"
source_pdf_pages: [451, 453]
book_pages: [415, 417]
batch: "010"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: false
continues_after_file: false
---

# Chapter 12: synthesis and handover to redundancy theory

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: PDF 446–450](0446-0450-ch12-orientation-gaps-and-architecture-no-go.md#pdf-0450) · [Next: PDF 454–460](0454-0460-ch13-redundancy-notions-and-silence.md#pdf-0454)

> Editorial note: Full transcription of PDF 451–453 (printed pages 415–417), completing Chapter 12. Original claims, empty reference/status slots, the word “fthe”, and unresolved §?? are retained. This synthesis repeats several claims discussed critically in the preceding Russian notes; note 147 links those qualifications without changing the author's summary.

<a id="pdf-0451"></a>
## PDF 451 · Book 415

<a id="section-12-8"></a>
### 12.8 Synthesis and Handover

This chapter has produced three coordinates: *sharpness* (§12.2), *activation* (§12.3), and *silence* (§12.4). It has also identified the active subspace (§12.5) and proven the three rigorous antisymmetric inevitability theorems (§12.6). What it has not done is construct, propose, or even prescribe an architecture.

The omission is intentional. Two interventions on the standard multi-head attention block are clearly visible from what one has done: one might want to remove heads with low activation energy ($\mathcal E^{(\ell,h)}\to0$), and one might want to grow heads where the residual amplification $\Gamma_{\mathrm{res}}^{(\ell)}$ is high. Neither prescription, however, is yet justified by the chapter's content. Removing heads requires a redundancy analysis — the head being removed must be functionally equivalent to a combination of other heads, not merely silent — and the redundancy analysis lives in Chapter 13. Growing heads requires a marginal-value calculation — the new head must add usable directions to the residual stream, not duplicate existing ones — which also lives in Chapter 13. The architectural synthesis lives in Chapter 14.

<a id="section-12-8-1"></a>
#### 12.8.1 What the chapter has established

The main results of the chapter, in the order of their development, are these:

1. *Sharpness theorem* (Theorem 12.2, ): an explicit threshold $\beta^*=\Theta(\log(n)/\kappa(p,X,M))$ at which the softmax of $\beta L$ is $\varepsilon$-close in Frobenius norm to the canonical regime $p$. The dependence on the relative emergence constant $\kappa(p,X,M)$ is the operator-level fingerprint of how cleanly a head can commit to its pattern.
2. *Mixed regime characterisation* (Proposition 12.3, ): a rigorous decomposition of the softmax row into a weighted mixture of canonical-support distributions, with explicit bounds on the weights and the residual.
3. *Hessian-spectral form of sharpness* (Theorems 12.4–12.7, ): a four-step closed-form determination of the $M$-space Hessian's smallest non-gauge eigenvalue. Step 1 gives the explicit spectrum $\{0,2(n+1)u^2\ (\mathrm{mult}\ n-2),4n^2u^2\}$ of the single-row block of $H_L^*$ at directional saturation; Step 2 identifies $H_M^*=\beta^2J(X)^\top H_L^*J(X)$ via the Kronecker pull-back $J(X)=X\otimes X$, with Weyl bounds $\sigma_{\min}(X)^4\lambda_{\min}(H_L^*)\le\lambda_{\min}(H_M^*)\le\sigma_{\max}(X)^4\lambda_{\min}(H_L^*)$; Step 3 establishes tightness of both Weyl bounds when $X$ is a multiple of an orthogonal matrix; Step 4 produces the closed form $\lambda_{\min}(H_M^*\rvert_{\mathrm{ng}})=\beta^2\sigma_{\min}(X)^2\cdot\lambda_{\min}(\Lambda_{\mathrm{nz}}^{1/2}V_{\mathrm{nz}}^\top XX^\top V_{\mathrm{nz}}\Lambda_{\mathrm{nz}}^{1/2})$. These fthe results replace the Hessian-spectral conjecture of a preliminary formulation.
4. *Activation operator* (Definitions 12.8–12.9, ): the frame-invariant operator $\mathcal A^{(h)}(X)=\pi_{U(X)}M_a^{(h)}\pi_{U(X)}$, separating the alignment factor in the canonical residual $\Gamma_{\mathrm{res}}^{(h)}(X)$ from the size factors.
5. *Spectral interpretation of activation* (Theorem 12.11, ): the activation energy decomposes as a bilinear sum over symplectic eigenplanes, with explicit cross-eigenplane terms that the original preliminary formulation of Chapter 6 had omitted.
6. *Activation threshold under random init* (Proposition 12.12, , on Xavier init): a calibration result asserting that randomly initialised heads have activation energy of order $d_k$ with high probability; explicitly conditional on Xavier initialisation.
7. *Silence criterion* (Theorem 12.14, ): the activation operator vanishes if and only if the

<a id="pdf-0452"></a>
## PDF 452 · Book 416

cloud's row span is isotropic for the alternating form induced by $M_a^{(h)}$.

8. *Generic non-silence* (Corollary 12.15, ): Haar-random subspaces of dimension $\ge2$ are non-silent with probability one.
9. *Silence manifold* (Proposition 12.16, ): the silence locus is a closed real-algebraic subvariety of the Grassmannian, of strictly lower dimension when $r\ge2$ and $r\le d-K$, empty when $r>d-K$.
10. *Active subspace and eigenplane decomposition* (Theorem 12.18, ): the active subspace equals the direct sum of the projected eigenplanes.
11. *Antisymmetric inevitability* (Theorems 12.25–12.28 with the Symmetry Conservation Lemma 12.24, ): for content-bilinear oriented tasks at score level, the orientation gap $\delta(\mathcal T)=\lVert B_a\rVert_F^2/\lVert B\rVert_F^2$ is realised exactly in the bilinear-form ratio $\lVert M_a\rVert_F^2/\lVert M\rVert_F^2$ at single-head, in the aggregate ratio $\lVert(M_{\mathrm{tot}})_a\rVert_F^2/\lVert M_{\mathrm{tot}}\rVert_F^2$ at multi-head, and in the per-head sum ratio under min-norm regularisation. The original conjecture (sum of per-head ratios with no regularisation) is shown to fail in multi-head by an explicit gauge-inflation counterexample; the three rigorous theorems above replace it.

<a id="section-12-8-2"></a>
#### 12.8.2 What the chapter has not done, by design

The chapter has not produced any of the following, all of which belong to subsequent chapters.

- *No definition of redundancy.* The activation framework prepares the ingredients (active subspace, activation energy, usage coefficient), but the redundancy analysis — parametric, functional, dynamic — is the subject of Chapter 13.
- *No design principle.* The chapter exhibits diagnostics, not prescriptions. The six design principles (P1–P6 of §13.10) are derived from the chapter's coordinates in Chapter 13.
- *No marginal-value calculation.* The marginal value of a head, defined in §13.6 as the contribution of the head to loss, signature coverage, and intervention effect, is built on the chapter's diagnostics but is itself a separate construction.
- *No architectural alternative to MHA+MLP.* The rationalised block of Chapter 14 uses the chapter's coordinates as inputs, but the block's definition (Definition 14.1), parameter count (Proposition 14.2), and FLOP accounting (§14.4) are not yet present.
- *No growth rule, moving target, or termination criterion.* These are the content of INCRT (Chapter 17), which uses the rationalised block as its static skeleton.

<a id="section-12-8-3"></a>
#### 12.8.3 Output to redundancy theory

The handover to Chapter 13 is summarised by the following fthe observables, one per head:

1. The *sharpness coordinate* $\kappa^{(\ell,h)}(p^*,X,M)$, the relative emergence constant of the head's preferred pattern $p^*$ on the cloud. It is computable in $O(n^2)$ time per head per forward pass.
2. The *activation operator* $\mathcal A^{(\ell,h)}(X)=\pi_{U^{(\ell)}(X)}M_a^{(\ell,h)}\pi_{U^{(\ell)}(X)}$, with its squared Frobenius norm $\mathcal E^{(\ell,h)}(X)$ and active subspace $V^{(\ell,h)}(X)$.
3. The *silence flag*: whether $U^{(\ell)}(X)$ lies on $\mathcal S(M_a^{(\ell,h)})$, equivalently whether $\mathcal E^{(\ell,h)}(X)=0$.

<a id="pdf-0453"></a>
## PDF 453 · Book 417

4. The *usage coefficient* $\chi_\tau^{(\ell,h)}(X)$ for each relevant task direction $\tau$, in the sense of Definition 12.19.

These fthe observables, together with the canonical signature $\widehat\lambda^{(\ell,h)}\in\Delta_8$ and the residual $\rho_8^{(\ell,h)}$ inherited from Chapter 7, form the local description on which Chapter 13's redundancy analysis operates. The aggregation across heads is layer-wise, and the layer-wise quantities feed the global trace operators (Trace A, Trace B) of §??.

<a id="section-12-8-4"></a>
#### 12.8.4 Output to the rationalised block

The link to Chapter 14 is more selective. Three results from this chapter are direct prerequisites of the rationalised block:

- The *activation operator framework* is the definitional basis for the active-subspace dimension that §14.3 uses to set the per-head value dimension $d_v$ proportional to $\dim V^{(\ell,h)}$.
- The *silence criterion* provides the operational test by which Chapter 14 prunes redundant heads in the rationalised block: a head is removable if its cloud lies on or near the silence locus and no other head's coverage of the cloud's task directions depends on it.
- *Antisymmetric inevitability* (B0, theorem-grade): by Theorems 12.25–12.28, the rationalised block's prescription that $\sum_h\alpha_h^{(\ell,h)}>0$ at every layer is non-vacuous on oriented tasks at score level. The prescription is now an unconditional consequence of the rigorous theorems for content-bilinear oriented tasks; the open directions (output-level, multi-layer, strong positional encodings) are documented at the end of §12.6.

<a id="section-12-8-5"></a>
#### 12.8.5 Closing remark

The local theory of Chapter 6 produced canonical labels. The global theory of Chapter 7 produced canonical coordinates. Chapter 10 produced the S–A decomposition. Chapter 11 produced trajectories. This chapter produces *numbers*: per-head, per-layer scalars and operators that quantify how sharp a regime is, whether the geometry is seen by the cloud, and which directions of the cloud the head is using. These are the observables that Chapters 13–14 need.

The orbits of Chapter 11 are now read through the activation operator and its scalars. The signatures of Chapter 7 are now equipped with quantitative content. The eight regimes of Chapter 6 are now the targets of an emergence theorem with a Frobenius-level threshold and the carriers of an operator-level activation that can be measured in any layer of any trained transformer.

The architecture has not been designed. It will be, in Chapter 14, on the basis of crowding and marginal value (Chapter 13), which depend on the coordinates produced here. The forward chain is closed.

> Editorial boundary: Chapter 12 ends here. [Chapter 13 begins on PDF 454](0454-0460-ch13-redundancy-notions-and-silence.md#chapter-13).
