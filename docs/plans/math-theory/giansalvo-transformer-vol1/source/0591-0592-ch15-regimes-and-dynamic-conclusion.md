---
title: "Chapter 15: protection proof, three controller regimes, and conclusion"
source_pdf_pages: [591, 592]
book_pages: [555, 556]
batch: "012"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: false
---

# Chapter 15: protection proof, three controller regimes, and conclusion

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: signed geometry](0588-0590-ch15-signed-residuals-and-level-one-rule.md) · [Next: Chapter 16](0593-0597-ch16-extension-status-and-exact-deflation.md)

> Editorial note: Full transcription of PDF 591–592 (printed pages 555–556), including the complete proof sketch of Proposition 15.24 and Conjecture 15.25. This file completes Chapter 15. Narrative announcements of later material do not establish that later results have been processed or verified.

<a id="pdf-0591"></a>
## PDF 591 · Book page 555

*Sketch.* None of the four core proofs refers to $\gamma_h^{(\ell)}(t)$. The diagnostic stability argument of Section 15.8 refers to $\widehat\rho_\ell$, $\widehat\Gamma_{\mathrm{res}}^{(\ell)}$, and $\widehat V_{\mathrm{marg},h}^{(\ell)}$; the coverage-decrease argument of Section 15.9 refers to $\rho_\ell$ and to the implementability of new heads; the directional-tail preservation argument of Section 15.10 refers to $\alpha_h^{(\ell)}$ and $V_{\mathrm{marg},h}^{(\ell)}$; the finite-termination argument of Section 15.11 refers to the termination tolerances of (15.20). The substitution $\gamma_h\mapsto|\gamma_h|$ acts only on the imported operational protection rule and, by symmetrising the inequality, can only enlarge the protected set. The four core proofs are therefore unaffected. $\square$

We refer to the original sign-asymmetric protection rule of (15.62) as the *level-zero* rule of the controller, and to the symmetric rule of Definition 15.23 as the *level-one* rule. A finer diagnostic, the *level-two* rule, is the natural endpoint of this progression and is recorded here for completeness. At level two, the scalar response $\gamma_h^{(\ell)}(t)$ is replaced by an oriented pair $(u_h^+,u_h^-)$ associated with a head, together with a running ratio $\bar\rho_h(t)$ and a relative score $s_h(t)$ that compares the two oriented contributions before any protection or pruning decision is taken. The level-two rule is the operational endpoint of the four-channel signed gate developed in INCRT II and is not used in the core chapter; it appears in this volume only as the matrix and spectral refinement of the level-one rule introduced above.

<a id="section-15-44-4"></a>
### 15.44.4 Three regimes of the dynamic controller

The level-one rule of the previous subsection is the only change that the present chapter commits to. It is useful, however, to indicate what the matrix refinement of INCRT II will buy. The dynamic controller of this chapter has been described, throughout, in a single regime: a randomly initialised model whose residual exposes a directional coverage gap, with the controller adding heads until the gap closes and growth terminates. This is the regime in which the static-to-dynamic argument of the chapter was constructed, and it is the regime in which the core empirical protocol of Section 15.39 was run. We refer to it as *Regime I*.

Two further regimes appear once the residual operator is allowed to be signed, and we anticipate them in narrative form only.

In *Regime II*, the stabilisation regime, the controller is run on a warm-started backbone whose residual operator already has very few dominant signed modes. The controller does not grow—no event of `AddHead` type is selected—but it does preserve and prune. The level-one rule of Definition 15.23 is the minimum that the controller needs in order to behave correctly in this regime. The matrix and spectral diagnostics of INCRT II are needed in order to understand why no growth is triggered and which heads are preserved.

In *Regime III*, the architecture-selection regime, the controller is applied before the model is trained, on a brief spectral pre-pass over the residual operator of a candidate backbone, in order to produce an architecture $(L^\star,\{K_\ell^\star\},\{(u_h^+,u_h^-)\})$ for that backbone. Growth and pruning are then absorbed into a single architectural draft. This is the analytic neural-architecture-search use of INCRT, and it lies outside both this chapter and INCRT II in its full form; it is the topic of the final synthesis of Volume I.

<a id="conjecture-15-25"></a>
**Conjecture 15.25 (Three-regime unification of the dynamic controller).** [CONJ] The expansion, stabilisation, and architecture-selection regimes are three operational specialisations of one dynamic controller acting on the signed residual geometry of an attention block. The differences between them lie in the initialisation of the backbone and in the time-scale on which the controller is run, not in the architecture of the controller itself.

This is a conjecture because the unification is at present documented by the empirical and operational alignment of the three regimes, and not by a single Lyapunov-style argument

<a id="pdf-0592"></a>
## PDF 592 · Book page 556

that covers all three. A unified treatment requires the matrix language of INCRT II together with the spectral gate analysis of the subsequent chapter on numerical treatment of the bidirectional gate, and is therefore not attempted here.

<a id="section-15-44-5"></a>
### 15.44.5 Bridge to INCRT II

The contents of the present section are deliberately limited. Two items have been committed to the core chapter: the recognition that the scalar diagnostics $\rho_\ell$, $\Gamma_{\mathrm{res}}^{(\ell)}$, $V_{\mathrm{marg},h}^{(\ell)}$, and $\alpha_h^{(\ell)}$ aggregate magnitudes and do not see the oriented structure of the residual operator (Sections 15.44.1–15.44.2); and the level-one correction of the imported protection rule (Definition 15.23 and Proposition 15.24), which is the smallest change that removes the sign-asymmetry of the original rule without altering the four core theorems of the chapter.

The remaining items have been announced and deferred. The matrix construction of the symmetric residual operator from the input covariance and the antisymmetric query–key part is given in INCRT II, including the commutator form and the zero-trace property used by Proposition 15.22; the approximate pairing $(+\lambda,-\lambda)$ of signed eigenvalues is documented there with the status [P-COND]; the level-two rule based on the oriented pair $(u_h^+,u_h^-)$ and on the running ratio $\bar\rho_h(t)$ is the operational form of the four-channel signed gate of INCRT II; Regime II of the controller is documented in INCRT II as a separate operating mode of the same dynamic system; Regime III is announced in INCRT II and developed in the final synthesis of Volume I.

The core controller of this chapter is complete on its own terms. It grows, tracks, preserves, and terminates on the basis of the magnitude information that the scalar diagnostics supply, and the level-one rule of Definition 15.23 is the smallest correction that the chapter requires. The orientation of the residual is left to INCRT II, and the architecture-selection use of the controller is left to the final synthesis of the volume. The Rationalised Transformer was made incremental in this chapter; it will be made signed in the next.

<a id="section-15-45"></a>
## 15.45 Concluding Remarks

The static part of the book ended when every theoretical coordinate had become an implementation diagnostic. The dynamic part begins when those diagnostics become architectural actions. INCRT is therefore not a separate invention attached after the Rationalised Transformer. It is the same construction with time restored:

<a id="display-0592-static-to-dynamic"></a>
$$
\begin{aligned}
\text{static signature geography}&\rightsquigarrow\text{moving signature geography},\\
\text{static coverage}&\rightsquigarrow\text{growth by coverage residual},\\
\text{static marginality}&\rightsquigarrow\text{pruning and preservation},\\
\text{static residual control}&\rightsquigarrow\text{finite termination}.
\end{aligned}
$$

The result is the core INCRT principle: the architecture should not merely be chosen before training. It should be derived during training from the geometry that the task itself reveals.

