---
title: "Chapter 17: numerical kernels, relative-gap diagnostics, and the four-dimensional example"
source_pdf_pages: [693, 695]
book_pages: [657, 659]
batch: "014"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: false
continues_after_file: false
---

# Chapter 17: numerical kernels, relative-gap diagnostics, and the four-dimensional example

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: spectral degeneracy](0689-0692-ch17-dwell-time-and-spectral-degeneracy.md) · [Next: shifted power and RQI](0696-0698-ch17-shifted-power-and-rayleigh-quotient-iteration.md)

> Editorial note: Full transcription of PDF 693–695 (printed pages 657–659). The original numerical-kernel set, claim about division by a vanishing Rayleigh quotient, indexing rule for $k_C$, signed example, and repeated prose are preserved. Notes separately distinguish the valid structural-band perturbation argument from the incomplete or inconsistent classification rules.

<a id="pdf-0693"></a>
## PDF 693 · Book page 657

<a id="section-17-3-3"></a>
### 17.3.3 Type C: zero eigenvalues

<a id="definition-17-5"></a>
**Definition 17.5 (Type-C regime: kernel directions).** $A_{\mathrm{res}}$ is in the *Type-C regime* if there exist eigenvalues $\lambda_k$ with

<a id="equation-17-14"></a>
$$
|\lambda_k|<\theta_{\mathrm{rank}}:=\sqrt{\varepsilon_{\mathrm{mach}}}\cdot\lambda_1(A_{\mathrm{res}}).
\tag{17.14}
$$

The corresponding eigenvectors span the *numerical kernel* of $A_{\mathrm{res}}$: $\ker_{\mathrm{num}}(A_{\mathrm{res}})=\{v\in\mathbb R^d:|A_{\mathrm{res}}v|<\theta_{\mathrm{rank}}\|v\|\}$.

**Geometric content.** A Type-C eigenvalue corresponds to a direction $v_k\in\mathbb R^d$ in which the residual operator carries no directional information: $A_{\mathrm{res}}v_k\approx0$. The growth criterion $\lambda_{\max}(A_{\mathrm{res}})>\theta_w$ of Chapter 15 was designed to exclude such directions, since $|\lambda_k|<\theta_w$ for any $\lambda_k$ in the numerical kernel (by construction $\theta_{\mathrm{rank}}\ll\theta_w$ for any reasonable scale of $\theta_w$). The problem, however, is not detection but *numerical stability*: when $A_{\mathrm{res}}$ has a non-trivial numerical kernel, the MCA EXIN iteration becomes singular. Specifically, the Rayleigh quotient $\widetilde y/\|u\|^2\to0$ as $u$ approaches a kernel direction, causing the EXIN update step to divide by a vanishing quantity.

**When Type C occurs in practice.** Type C is the regime that appears most frequently in real INCRT runs, contrary to what its “pathological” definition might suggest. Three sources:

1. *Structural decoupling.* The motor $M_a$ may have rank lower than $d$ by construction (e.g., the dimension-four example of Appendix 15.20 in Chapter 15 has $M_a$ supported on a 3-dimensional subspace, with $e_4$ exactly in the kernel). The fourth eigenvalue of $A_{\mathrm{res}}$ in that example is $\lambda_2=0$ exactly.
2. *Saturation of the deflation basis.* As growth events accumulate and $K$ approaches the effective rank of $\mathcal B$, the projection $P^\perp=I-QQ^\top$ progressively reduces the rank of $A_{\mathrm{res}}$. After $K\approx\operatorname{rank}(\mathcal B)$ growth events, the residual is exclusively supported on the numerical kernel.
3. *Floating-point accumulation.* Even when $\mathcal B$ has full rank in exact arithmetic, the computed $A_{\mathrm{res}}$ accumulates $O(K\varepsilon_{\mathrm{mach}})$ errors in the deflation step, eventually producing eigenvalues below $\theta_{\mathrm{rank}}$.

The empirical signature of Type C in the SARS-CoV-2 experiment of the SARS-CoV-2 experiment of Chapter 18 is the *flattening* of $\Gamma_{\mathrm{res}}$ after $K=3$ growth events visible in Figure 7: the residual energy stops decreasing not because the algorithm has converged, but because the remaining directions are in the numerical kernel.

<a id="prescription-0693-rank-truncation"></a>
**Strategy: rank truncation.** The correct response to Type C is to *remove* the kernel directions from the active domain of the gate. Operationally:

1. Detect kernel directions via the threshold $|\lambda_k|<\theta_{\mathrm{rank}}$.
2. Augment the deflation basis $Q$ with the kernel eigenvectors, treating them as “zero-cost” directions that contribute neither to the head count $K$ nor to the residual energy.
3. Restrict subsequent gate iterations to the orthogonal complement of the augmented basis.

This is not a regularisation in the Tikhonov sense (which would shift all eigenvalues uniformly and bias the entire spectrum), but a geometric restriction of the dynamics to the numerically relevant subspace. The distinction has consequences for the universal complexity law: kernel

<a id="pdf-0694"></a>
## PDF 694 · Book page 658

directions are excluded from the count $N^*$, refining the bound $N^*=\Theta(\kappa_A^2\log(\Gamma_0/\theta_w))$ of Corollary 16.9 of Chapter 16 (see §17.6.1).

<a id="section-17-3-4"></a>
### 17.3.4 Detection criterion: the relative gap test

The three regimes of Definitions 17.3–17.5 are detected jointly by a single diagnostic, the *relative gap test*, which generalises the Level-2 trigger of Chapter 15 to all three regimes.

<a id="definition-17-6"></a>
**Definition 17.6 (Relative gap test).** Given an estimate $\widehat\lambda_1\ge\cdots\ge\widehat\lambda_{\tau_{\max}}$ of the top $\tau_{\max}$ eigenvalues of $A_{\mathrm{res}}$ (computed e.g. by Lanczos with Krylov depth $\tau_{\max}$, see §17.4.3), the relative gap test classifies the regime as follows:

1. For each $k=1,\ldots,\tau_{\max}$, compute $\gamma_k:=(\widehat\lambda_k-\widehat\lambda_{k+1})/\widehat\lambda_k$ (with $\widehat\lambda_{\tau_{\max}+1}:=0$).
2. Identify the largest $k_C$ such that $|\widehat\lambda_{k_C+1}|<\theta_{\mathrm{rank}}$. The eigenvectors indexed $k_C+1,\ldots,\tau_{\max}$ are *kernel directions* (Type C).
3. Identify the largest $m\le k_C$ such that $\gamma_k<\gamma_{\mathrm{near}}$ for $k=1,\ldots,m-1$ and $\gamma_m\ge\gamma_{\mathrm{near}}$. The top $m$ eigenvalues form a *near-degenerate cluster* (Type B if $m>1$, regular regime if $m=1$).

The triple $(m,k_C,\tau_{\max})$ summarises the spectral classification at this growth event.

**Properties of the gap test.** The relative gap test has three properties that recommend it over alternatives such as ratio thresholds on absolute eigenvalues:

<a id="lemma-17-7"></a>
**Lemma 17.7 (Stability of the relative gap test on the structural spectrum).** *Let $A_{\mathrm{res}}$ and $\widetilde A_{\mathrm{res}}=A_{\mathrm{res}}+\delta A$ have eigenvalues $\lambda_k$ and $\widetilde\lambda_k$ respectively. Assume that the indices under consideration belong to the non-kernel structural band, namely*

<a id="display-0694-structural-band"></a>
$$
\lambda_k\ge c\lambda_1,\qquad\lambda_{k+1}\ge c\lambda_1,
$$

*for some fixed $c\in(0,1]$, and that $\|\delta A\|_2\le\eta\lambda_1$ with $\eta<c/2$. Then the relative gaps satisfy*

<a id="display-0694-relative-gap-stability"></a>
$$
|\gamma_k-\widetilde\gamma_k|\le C(c)\eta+O_c(\eta^2),
$$

*where one may take $C(c)=2c^{-1}+1$ for sufficiently small $\eta$. In particular, for the top cluster with $\lambda_k=\Theta(\lambda_1)$, the relative gap test is stable under small operator perturbations. No uniform statement is claimed for kernel indices; those are separated first by the rank threshold $\theta_{\mathrm{rank}}$.*

*Proof.* By Weyl’s inequality,

<a id="display-0694-weyl-eigenvalue-error"></a>
$$
|\widetilde\lambda_j-\lambda_j|\le\|\delta A\|_2\le\eta\lambda_1,
\qquad j=k,k+1.
$$

On the structural band, $\lambda_j\ge c\lambda_1$, hence $|\widetilde\lambda_j-\lambda_j|/\lambda_j\le\eta/c$. Writing $\delta\lambda_j=\widetilde\lambda_j-\lambda_j$ gives

<a id="display-0694-relative-gap-expansion"></a>
$$
\begin{aligned}
\widetilde\gamma_k
&=\frac{\widetilde\lambda_k-\widetilde\lambda_{k+1}}{\widetilde\lambda_k}\\
&=\frac{(\lambda_k-\lambda_{k+1})+(\delta\lambda_k-\delta\lambda_{k+1})}
{\lambda_k(1+\delta\lambda_k/\lambda_k)}\\
&=\gamma_k\left(1-\frac{\delta\lambda_k}{\lambda_k}\right)
+\frac{\delta\lambda_k-\delta\lambda_{k+1}}{\lambda_k}+O_c(\eta^2).
\end{aligned}
$$

Since $|\gamma_k|\le1$ and $|\delta\lambda_k-\delta\lambda_{k+1}|/\lambda_k\le2\eta/c$, the stated bound follows. The structural-band assumption is essential: as $\lambda_k$ approaches the numerical floor, relative perturbations of $\gamma_k$ need not remain small, and this is precisely why Type-C directions are detected before the near-degenerate cluster is classified. $\square$

<a id="pdf-0695"></a>
## PDF 695 · Book page 659

The lemma justifies the use of $\gamma_k$ as a robust indicator: small perturbations of $A_{\mathrm{res}}$ (e.g., from Lanczos approximation errors or floating-point noise) produce small perturbations of $\gamma_k$, preserving the regime classification.

**Comparison with the Chapter 15 trigger.** The Level-2 trigger of Chapter 15, $\lambda_1/\lambda_2<1+\gamma$ with $\gamma=0.05$, is equivalent to $\gamma_1<0.05/(1+0.05)\approx0.048$, i.e., a special case of Definition 17.6 for $k=1$. The generalisation here unifies three trigger conditions (Level-1 head growth, Level-2 block growth, kernel detection) under a single test: read $(m,k_C)$ from the spectrum and dispatch to the appropriate algorithmic branch.

<a id="section-17-3-5"></a>
### 17.3.5 Worked example: dimension four classified

The dimension-four example of Appendix 15.20 of Chapter 15 provides a concrete illustration of all three regimes coexisting in a single residual operator. Recall the spectrum (Equation 46 of that appendix):

<a id="equation-17-15"></a>
$$
\lambda_1\approx2.51,\qquad\lambda_2=0,\qquad
\lambda_3\approx-0.37,\qquad\lambda_4\approx-2.14.
\tag{17.15}
$$

Apply the relative gap test (Definition 17.6) with default thresholds $\theta_{\mathrm{rank}}=\sqrt{\varepsilon_{\mathrm{mach}}}\cdot\lambda_1\approx3.7\cdot10^{-8}$ and $\gamma_{\mathrm{near}}=0.05$:

- $|\lambda_2|=0<\theta_{\mathrm{rank}}$ exactly: index $k=2$ is a **Type-C kernel direction**. The eigenvector $v_2=e_4$ is appended to the deflation basis as a zero-cost direction.
- $\gamma_1=(2.51-0)/2.51\approx1.0\gg\gamma_{\mathrm{near}}$: the dominant eigenvalue $\lambda_1$ is well-separated from the next non-kernel eigenvalue (which after kernel exclusion is $\lambda_3\approx-0.37$). **Regular** regime at the dominant end.
- Symmetrically, the trailing eigenvalue $\lambda_4\approx-2.14$ is well-separated from $\lambda_3\approx-0.37$ (relative gap $\approx0.83$): **regular** regime at the trailing end as well.

The classification is therefore: *regular dominant, regular trailing, one Type-C kernel direction.* The unified gate of §17.5 responds by:

<a id="prescription-0695-four-dimensional-classification"></a>
1. Adding $e_4$ to $Q$ as a kernel direction (zero-cost).
2. Allocating one bidirectional head with $u^+=v_1$ and $u^-=v_4$ via Rayleigh quotient iteration on the truncated operator $P_{e_4}^\perp A_{\mathrm{res}}P_{e_4}^\perp$.
3. Terminating with $K^*=1$ structural unit, well below the universal-law bound $K_{\mathrm{bound}}^*\approx4$ (Equation 16.27 of Chapter 16).

**Comparison with the Chapter 15 algorithm.** The bidirectional gate of Chapter 15 produces the same final architecture ($K^*=1$ head) on this example, but for a different reason: it does not detect the Type-C direction explicitly, and instead relies on the fact that $\lambda_2=0$ does not trigger the Level-1 growth criterion $\lambda_{\max}(A_{\mathrm{res}})>\theta_w$ at any iteration. The unified gate *certifies* the same outcome by classification, providing a kernel-aware accounting of the structural budget. The difference becomes substantial when the spectrum has near-zero (rather than exactly zero) eigenvalues: in that case, the Chapter 15 algorithm risks oscillation between growth and pruning of marginal heads, while the unified gate removes the offending directions structurally.

A complete numerical execution of this example, including the trajectory of Lanczos Ritz values and the deflation steps, is given in §17.6.2.
