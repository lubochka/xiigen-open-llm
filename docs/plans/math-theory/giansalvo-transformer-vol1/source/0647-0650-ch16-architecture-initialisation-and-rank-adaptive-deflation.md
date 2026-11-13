---
title: "Chapter 16: architecture initialisation, open guarantees, and the start of rank-adaptive deflation"
source_pdf_pages: [647, 650]
book_pages: [611, 614]
batch: "013"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 16: architecture initialisation, open guarantees, and the start of rank-adaptive deflation

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: level-two pruning and stabilisation](0643-0646-ch16-level-two-pruning-and-stabilisation.md)

> Editorial note: Full transcription of PDF 647–650 (printed pages 611–614). The six-step procedure continues the introduction on PDF 646. Conjectures, future-work statements, `[citation]` placeholders, the unfinished references “Open Question 13.x” and “13.x+1”, and the original “product” wording next to $\sum_h r_h$ are retained. The range ends after the assumptions and definition (16.81) of Lemma 16.57. Its three properties and proof are outside this batch and have not been transcribed here.

<a id="pdf-0647"></a>
## PDF 647 · Book page 611

<a id="algorithm-0647-architecture-initialisation-steps"></a>
1. **Estimate the input covariance.** Compute $\Sigma_X=\frac1B\sum_bX_b^\top X_b$ on a calibration batch of size $B$ drawn from the training distribution, before any gradient step is taken.

2. **Compute the head-level signed operators.** For every layer $\ell$ and every head $h$ of the candidate backbone, form $M_{\mathrm{eff},h}=W_q^{(h)}(W_k^{(h)})^\top$, extract its antisymmetric part $K_h$ as in (16.72), and compute $A_h^{\mathrm{sig}}=\operatorname{sym}(\Sigma_XK_h)$ as in (16.73).

3. **Diagonalise.** Compute the eigendecomposition of each $A_h^{\mathrm{sig}}$, sort the eigenvalues in ascending order, and identify the leading positive–negative pair $(u_h^+,u_h^-)$ with eigenvalues $(\lambda_h^+,\lambda_h^-)$.

4. **Apply the signed CS bound layer by layer.** For each layer $\ell$, aggregate the spectral structure of the heads to form the layer-level signed energies $\Gamma_0^\pm(\ell)$ and the layer-level signed complexities $\kappa_{A,\pm}(\ell)$. Set $K_\ell^{\star,\pm}=\kappa_{A,\pm}(\ell)^2\log(\Gamma_0^\pm(\ell)/\theta_w)$ via Theorem 16.44 applied at the layer level, and $K_\ell^\star=K_\ell^{\star,+}+K_\ell^{\star,-}$.

5. **Determine the depth $L^\star$.** Run the depth-self-determination procedure of Section 16.4 on the layer-level signed energies $\{\Gamma_0^\pm(\ell)\}_\ell$, terminating when the residual layer-energy falls below the layer-level threshold. This yields $L^\star$.

6. **Initialise the four-channel gate.** For every selected head $h$, initialise the four-channel gate of Section 16.12.2 on the pair $(u_h^+,u_h^-)$ obtained in Step 3, with the class-balance ratio inferred from the layer-level signed complexities of Step 4.

The procedure is deterministic given the calibration batch and the candidate backbone initialisation. Its output is the architecture $(L^\star,\{K_\ell^\star\},\{(u_h^+,u_h^-)\}_h)$, which is then frozen for the subsequent training run. The training run uses the standard four-channel gate and the standard optimiser; the controller of INCRT I is no longer run at training time, because the architecture it would otherwise have produced has already been computed at initialisation.

**Analytic neural architecture search.** Under the architecture-initialisation interpretation, INCRT becomes a procedure that takes as input a backbone family, an input distribution, and a residual budget $\theta_w$, and outputs an architecture deterministically. Three operational properties are inherited from the matrix framework of the present section. First, the output architecture is determined entirely by the spectrum of $A_h^{\mathrm{sig}}$ on the input covariance $\Sigma_X$, and is therefore reproducible across runs that share the same data and the same initialisation. Second, the head counts $K_\ell^\star$ are obtained directly from the signed compressed-sensing bound of Theorem 16.44 applied to the head-level signed spectrum at each layer, with no search over hyperparameters. Third, the oriented pairs $(u_h^+,u_h^-)$ attached to each head at output time are the natural initialisations of the four-channel gate at training time, short-circuiting the initialisation step of the level-two rule that the dynamic controller would otherwise have to discover.

<a id="conjecture-16-55"></a>
**Conjecture 16.55 (Architecture initialisation matches dynamic INCRT in expectation).** [CONJ] *Under the assumptions of the compressed-sensing bound of Section 16.3 together with the empirical pairing of Conjecture 16.48, the architecture produced by the architecture-initialisation use of INCRT agrees, in expectation over the input distribution, with the architecture that the dynamic controller of INCRT I would produce on a randomly initialised backbone trained on the same task.*

The conjecture is the operational endpoint of the present section. Its proof requires a coupling argument between the spectrum of $A_h^{\mathrm{sig}}$ at initialisation and the spectrum of the deflated residual operator $A_{\mathrm{res}}^{(k)}$ along a trained trajectory; the construction of such a coupling is one

<a id="pdf-0648"></a>
## PDF 648 · Book page 612

of the open problems of Volume II. Empirical validation of the conjecture, on the four task families of Section 16.12.4, is the natural agenda for the next empirical pass.

<a id="section-16-13-6"></a>
### 16.13.6 Three regimes of the dynamic controller, unified

INCRT I introduced the three operational regimes of the controller in scalar terms, in the bridge subsection that prepares the present chapter. We restate the three regimes here in the matrix framework that the present section makes available.

- *Regime I — expansion.* The controller is run during training on a randomly initialised backbone. The spectrum of $A_h^{\mathrm{sig}}$ exposes oriented coverage gaps that have not yet been captured by any head, and the four-channel gate of Section 16.12 adds heads, with $n_{\mathrm{growth}}>0$ and decreasing $\Gamma_{\mathrm{res}}^{(k)}$ at every accepted event. Termination is reached when the four tolerances of finite termination hold simultaneously, in agreement with the compressed-sensing target $K^\star$.
- *Regime II — stabilisation.* The controller is run during training on a warm-started backbone, as in Section 16.13.4. The spectrum of $A_h^{\mathrm{sig}}$ already has very few dominant signed modes, $n_{\mathrm{growth}}=0$, and protection and pruning operate asymmetrically. Termination is reached at $K_{\mathrm{obs}}\approx K_{\mathrm{pred}}^\star$ from above.
- *Regime III — architecture initialisation.* The controller is run *before* training, on a candidate backbone, as a spectral pre-pass on $\{A_h^{\mathrm{sig}}\}_h$ in the sense of Definition 16.54. The output architecture is fixed for the subsequent training run. The compressed-sensing target $K^\star$ is computed once at initialisation, and the four-channel gate is initialised from $\{(u_h^+,u_h^-)\}_h$.

<a id="conjecture-16-56"></a>
**Conjecture 16.56 (Three-regime unification, matrix form).** [CONJ] *The expansion, stabilisation, and architecture-initialisation regimes are three operational specialisations of the same dynamic system, acting on the head-level signed residual operators $\{A_h^{\mathrm{sig}}\}_h$. The differences between the three regimes lie in the initialisation of the backbone and in the time-scale on which the controller is run, not in the architecture of the controller itself. The compressed-sensing target $K^\star$ of Section 16.3 is invariant across the three regimes when $A_h^{\mathrm{sig}}$ admits a uniform involution as in (16.76).*

The conjecture is the matrix version of the scalar conjecture recorded in the bridge subsection of INCRT I. Its strengthening here is the invariance claim on $K^\star$: with the involution condition (16.76), the three regimes agree on the head count. Without it, the agreement is only up to the empirical pairing error $\Delta_{\max}$ of Conjecture 16.48, and is therefore conditional on the structural condition the conjecture postulates.

<a id="section-16-13-7"></a>
### 16.13.7 Open questions

Three open questions are recorded here to delimit the boundary of the matrix framework of the present section.

**Existence of a structural involution.** The condition $JA_h^{\mathrm{sig}}J^\top=-A_h^{\mathrm{sig}}$ with $J\in O(d)$, $J^2=I$, is the structural condition that turns the empirical pairing of Conjecture 16.48 into a rigorous statement. The conditions on $(\Sigma_X,K_h)$ under which such a $J$ exists are not characterised in this volume. A sufficient condition is the existence of an orthogonal involution $J\in O(d)$, $J^2=I$, that preserves the input covariance and reverses the antisymmetric query–key part,

<a id="equation-16-80"></a>
$$
J\Sigma_XJ^\top=\Sigma_X,\qquad JK_hJ^\top=-K_h.
\tag{16.80}
$$

<a id="pdf-0649"></a>
## PDF 649 · Book page 613

Under (16.80), using $\Sigma_X^\top=\Sigma_X$, $K_h^\top=-K_h$, and $J^\top J=I$, one verifies directly that

<a id="display-0649-involution-sufficiency-calculation"></a>
$$
JA_h^{\mathrm{sig}}J^\top
=\tfrac12J\bigl(\Sigma_XK_h-K_h\Sigma_X\bigr)J^\top
=\tfrac12\bigl(\Sigma_X(-K_h)-(-K_h)\Sigma_X\bigr)
=-A_h^{\mathrm{sig}},
$$

which is the involution condition (16.76). The conditions under which the data covariance $\Sigma_X$ admits such a non-trivial $\mathbb Z_2$-symmetry, and under which the trained antisymmetric part $K_h$ anti-commutes with it in the sense of (16.80), are not known for typical training trajectories.

**Aggregation of $A_h^{\mathrm{sig}}$ to the global deflated $A_{\mathrm{res}}^{(k)}$.** Section 16.13.1 announces that $A_h^{\mathrm{sig}}$ refines $A_{\mathrm{res}}^{(k)}$, but does not derive the explicit aggregation map. The construction requires four ingredients: (i) a projection of $A_h^{\mathrm{sig}}$ onto the captured subspace at the current deflation step; (ii) a class decomposition into positive and negative parts in the sense of Section 16.12; (iii) a head-aggregation rule that combines the contributions of all active heads; (iv) a compatibility check with the deflation lemma of Section 16.3. The four ingredients are recorded as Open Question 13.x in Section 16.19, and are part of the agenda of Volume II.

**Convergence proof for the level-two rule.** Definition 16.49 introduces a protection rule based on a score $S_h(t)$ that couples four quantities of distinct natures. The convergence of the level-two controller, in the sense of the four core properties of INCRT I (diagnostic stability, coverage-residual decrease, anchor preservation, finite termination), is not established in this chapter. The expected statement is that under the involution condition (16.76) and a separation of time scales between the controller and the optimiser, the level-two rule satisfies a Lyapunov-type contraction on the score $S_h(t)$ that strictly refines the level-one contraction. The complete statement and proof are recorded as Open Question 13.x+1 of Section 16.19.

<a id="section-16-13-8"></a>
### 16.13.8 Summary of the section

The present section descended from the setup-level framework of Section 16.12 to the head level. Four items have been committed.

The head-level signed residual operator $A_h^{\mathrm{sig}}=\operatorname{sym}(\Sigma_XK_h)$ was defined in (16.73). Its symmetry and tracelessness were proved in Proposition 16.46, and its indefinite spectrum was derived in Corollary 16.47.

The empirical pairing of the signed spectrum was recorded in Conjecture 16.48, together with the sufficient structural condition (16.76) that would elevate the pairing from empirical observation to a rigorous statement.

The level-two diagnostic rule of Definition 16.49 was given in matrix form, as the operational endpoint of the four-channel signed gate of Section 16.12 and as the strict refinement of the level-one rule of INCRT I certified by Proposition 16.50.

The stabilisation regime of the controller (Section 16.13.4) was documented on the sweep Table 16.9, with three accompanying conjectures (no-growth, upper-selection, asymmetric attractor); and the architecture-initialisation regime of Definition 16.54 was announced as the third operational specialisation of the controller, with Conjecture 16.55 stating the expected match with the dynamic controller in expectation over the input distribution.

The three regimes were unified in Conjecture 16.56, the matrix form of the scalar conjecture recorded in the bridge subsection of INCRT I. Open questions on the existence of a structural involution, on the aggregation of $A_h^{\mathrm{sig}}$ to the global deflated $A_{\mathrm{res}}^{(k)}$, and on the convergence of the level-two rule, were recorded for Volume II.

<a id="pdf-0650"></a>
## PDF 650 · Book page 614

<a id="section-16-14"></a>
## 16.14 Rank-adaptive heads and the count–rank invariant

The framework so far determines the head count $K_\ell$ and the depth $L^*$ from the spectral structure of the task operator, under the assumption that the head dimension $d_k$ is fixed by the practitioner. The transformer convention $d_k=d/H$ is preserved through the analysis but never derived from the task: it is a design choice inherited from [citation] and not justified by any property of $\mathcal B$.

The convention has been challenged from several directions. [citation] show that the standard scaling $d_k=d/H$ produces a low-rank bottleneck: when $H$ exceeds $d/N$ (with $N$ the sequence length), the per-head representational rank saturates and additional heads add no expressive power. The proposal of that work is to set $d_k=N$ independent of $H$, a prescription that improves performance empirically but has no derivation from the task itself. [citation] prove a complementary statement: low-rank attention layers cannot approximate certain target functions unless $H$ is exponential in the embedding dimension. [citation] show that the empirical attention rank is bounded above by approximately $0.63N$ regardless of $d_k$, saturating at $d_k=\Omega(\log N)$.

These results, taken together, indicate that the head dimension is not a free architectural choice but a structural one, with an optimum determined by the spectrum of the task. The compressed-sensing argument that fixes $K^*$ in INCRT I extends naturally to fix the rank distribution: each head should capture as many spectral directions as a single deflation step can isolate without degrading orthogonality, and the head count should adapt inversely. The development that follows makes the extension precise.

The conclusion is a count–rank invariant: the product $\sum_h r_h$, summed over the heads of a layer, is a property of the task and does not depend on how the spectral budget is split between count and rank. A wide-and-shallow split (many heads, low rank each) and a narrow-and-deep split (few heads, high rank each) yield the same total spectral coverage if the deflation is performed properly. The standard convention $d_k=d/H$ is recovered as one point on this trade-off; the proposal of [citation] ($d_k=N$) is recovered as another; the framework provides the continuous interpolation between them.

<a id="section-16-14-1"></a>
### 16.14.1 Rank-adaptive deflation

The exact-deflation lemma of INCRT I (the lemma stated above of INCRT I) projects the residual onto the orthogonal complement of a single direction $u^+$ at each growth event. The residual decreases by exactly $\lambda_1(A_{\mathrm{res}})$, the leading eigenvalue. A natural extension projects onto the span of the leading $r$ eigenvectors at once: the residual then decreases by $\sum_{i=1}^r\lambda_i(A_{\mathrm{res}})$.

<a id="lemma-16-57"></a>
**Lemma 16.57 (Rank-$r$ exact deflation).** *Let $A_{\mathrm{res}}\in\mathbb R^{d\times d}$ be symmetric positive semi-definite with spectral decomposition*

<a id="display-0650-ordered-residual-spectral-decomposition"></a>
$$
A_{\mathrm{res}}=\sum_{i=1}^d\lambda_i u_i u_i^\top,
\qquad\lambda_1\ge\lambda_2\ge\cdots\ge\lambda_d\ge0,
$$

*and orthonormal eigenvectors $\{u_i\}_{i=1}^d$. For an integer $r\in\{1,\ldots,d\}$, define the rank-$r$ exact deflation of $A_{\mathrm{res}}$ on its leading $r$-dimensional eigenspace by*

<a id="equation-16-81"></a>
$$
A_{\mathrm{res}}^{(\mathrm{new})}:=A_{\mathrm{res}}-\sum_{i=1}^r\lambda_i u_i u_i^\top
=\sum_{i=r+1}^d\lambda_i u_i u_i^\top.
\tag{16.81}
$$

*Then the following three properties hold.*

> Batch boundary: PDF 650 ends with “Then the following three properties hold.” Lemma 16.57 is incomplete here: only its assumptions and definition (16.81) have been transcribed. PDF 651 starts “(i) Energy reduction.” The three properties, their formulas, and the proof belong to the next batch; §16.14.1 and Chapter 16 continue.

> Continuation published in batch 014: [PDF 651, the three conclusions and proof of Lemma 16.57](0651-0654-ch16-rank-deflation-proof-and-count-rank-invariant.md#continuation-lemma-16-57). The boundary note above records the original scope of batch 013. The lemma is now available in full across PDF 650–653; editorial qualifications are separate from the source. Chapter 16 ends on PDF 681; the current batch boundary is PDF 700 in §17.4.4.
