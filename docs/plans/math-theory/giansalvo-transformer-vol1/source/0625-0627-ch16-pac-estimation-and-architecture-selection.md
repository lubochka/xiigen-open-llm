---
title: "Chapter 16: optimiser evidence, PAC estimation, and architecture selection"
type: source-transcription
language: en
batch: "013"
source_pdf_pages: [625, 627]
book_pages: [589, 591]
---

# Chapter 16: optimiser evidence, PAC estimation, and architecture selection

[Library index](../README.md) · [Source and transcription policy](../SOURCE.md)

<a id="pdf-0625"></a>
## PDF 625 · Book page 589

approximates the inverse Hessian, which equals $A_{\rm res}^{-1}$ at saddle points. Substituting into the gate flow gives a contraction rate $4\Delta^+\cdot\delta_0^{-1}\cdot\delta_0=4\Delta^+$, matching the Adam result up to a constant factor and recovering the same $\kappa_A$-independence.

<a id="section-16-8-6"></a>
### 16.8.6 Empirical signature of the acceleration

The prediction of the theorem stated in this part is concrete: the time to grow each head under Adam should be approximately $\kappa_A$ times shorter than under SGD, on the same task. For the SST-2 task of INCRT I with $\widehat\kappa_A\approx12.4$, the predicted speedup is between ten and thirteen times.

The training curves of standard transformers confirm a speedup of this order of magnitude: BERT-base trained with SGD requires hundreds of thousands of optimisation steps to reach a given perplexity [citation], while the same network trained with AdamW reaches the same perplexity in of order ten thousand steps [citation]. The framework attributes this empirical observation to the $\kappa_A$-cancellation of the theorem stated in this part, not to a vague notion of curvature awareness.

**Summary.** Adaptive optimisers — Adam, AdamW, Lion, and the second-order preconditioning methods — accelerate the gate convergence by exactly the factor $\kappa_A$ that the spectral asymmetry would otherwise impose as a slowdown. The asymptotic architecture, characterised by the compressed-sensing bound, is unchanged: the optimiser determines the time to reach the optimal architecture, not what it is. The four extensions of the preceding sections — continual operation, expert routing, architectural transfer, and adaptive optimisation — are mutually compatible: each modifies one axis of the framework while leaving the others unchanged, and their combination yields a unified theory of architecture growth that covers the regimes of practical interest in modern deep learning.

<a id="section-16-9"></a>
## 16.9 PAC guarantees for architecture selection

The compressed-sensing bound of the theorem stated in this part counts the minimum number of heads needed to represent the task geometry. It is a structural result: it does not depend on the number of training samples. The question of sample complexity — how many training examples are needed to reliably select the correct architecture — is the subject of this section.

The fundamental difficulty is that INCRT is not a fixed-architecture learner: it produces a *sequence* of architectures $f_1,f_2,\ldots,f_{K^*}$ with $k$ heads each, and the decision to add a head depends on the data. The result is established in four steps: (i) bound the Rademacher complexity of each fixed-$k$ class, (ii) apply a structural risk minimisation (SRM) union bound across architectures, (iii) control the task operator estimation error, and (iv) combine into a single theorem.

<a id="section-16-9-1"></a>
### 16.9.1 Rademacher complexity of fixed architectures

Let $\mathcal F_k$ denote the function class of INCRT networks with exactly $k$ heads (across all layers), input bounded $\|X\|_F\le B_X$, motors bounded $\|M_a^{(l,h)}\|_F\le\rho_{\max}$ (by the homeostatic theorem), value matrices at the NTK-optimal scale, and Lipschitz classifier $g$ with $\|g\|_{\rm Lip}\le L_g$.

<a id="pdf-0626"></a>
## PDF 626 · Book page 590

<a id="proposition-16-33"></a>
**Proposition 16.33 (Rademacher complexity).** The empirical Rademacher complexity of $\mathcal F_k$ over $m$ i.i.d. samples satisfies

<a id="equation-16-46"></a>
$$
\widehat{\mathfrak R}_m(\mathcal F_k)\le L_g\,B_X\,\sigma_V^*\,\rho_{\max}\,k\sqrt{\frac{2\log(2d)}m}.
\tag{16.46}
$$

*Proof.* By the contraction lemma for the Lipschitz classifier $g$, it suffices to bound the Rademacher complexity of the attention output $\sum_h\operatorname{Attn}_h(X)$. Each head contributes a rank-$d_V$ linear map modulated by the softmax attention weights. Under the near-uniform softmax assumption, the attention map of head $h$ is $O(B_X\sigma_V^*/\sqrt{d_k})$-bounded in operator norm. The $k$ heads contribute independently to the Rademacher sum because their motor directions are orthogonal (enforced by the deflation mechanism), so the total is $k$ times the single-head contribution. The $\sqrt{\log(2d)/m}$ factor is the standard matrix Rademacher bound for $d\times d$ bounded matrices. $\square$

<a id="section-16-9-2"></a>
### 16.9.2 Structural risk minimisation

INCRT produces architectures with $k=1,2,\ldots,K^*$ heads. Assigning prior weight $p_k=6/(\pi^2k^2)$ to model size $k$ (which sums to 1), the SRM generalisation bound gives: for each $k$, with probability $\ge1-\delta\,p_k$,

<a id="equation-16-47"></a>
$$
L(f_k)\le\widehat L_m(f_k)+\widehat{\mathfrak R}_m(\mathcal F_k)+\sqrt{\frac{2\log k+\log(\pi^2/(6\delta))}{2m}}.
\tag{16.47}
$$

The model-selection penalty is $2\log k+O(\log(1/\delta))$, which is logarithmic in $K^*$ — negligible compared to the Rademacher term.

<a id="section-16-9-3"></a>
### 16.9.3 Task operator estimation

The growth decisions depend on the eigenstructure of the empirical task operator $\widehat{\mathcal B}_A$. Define the *relevant gap*:

<a id="equation-16-48"></a>
$$
\Delta_{\rm rel}=\min_{j:\,|\lambda_j(\mathcal B)-\theta_w|<\theta_w}|\lambda_j(\mathcal B)-\theta_w|,
\tag{16.48}
$$

the minimum distance of any eigenvalue of the population task operator from the growth threshold. If $\Delta_{\rm rel}>0$ (no eigenvalue sits exactly at the threshold), then INCRT's growth decisions are correct whenever the estimation error satisfies $\|\widehat{\mathcal B}_A-\mathcal B\|_2<\Delta_{\rm rel}$.

<a id="lemma-16-34"></a>
**Lemma 16.34 (Task operator concentration).** Let $X_1,\ldots,X_m$ be i.i.d. samples with $\|X_i\|_F\le B_X$, and let $\widehat{\mathcal B}_A$ be the empirical task operator. Then with probability $\ge1-\delta$,

<a id="equation-16-49"></a>
$$
\|\widehat{\mathcal B}_A-\mathcal B\|_2\le B_X^2\,\rho_{\max}\sqrt{\frac{2d\log(2d/\delta)}m}.
\tag{16.49}
$$

*Proof.* The task operator $\mathcal B=\operatorname{sym}(X^\top XM_a)$ is a degree-2 polynomial in $X$ of spectral norm bounded by $\|X\|_F^2\|M_a\|_2\le B_X^2\rho_{\max}$. The empirical operator $\widehat{\mathcal B}_A=m^{-1}\sum_i\operatorname{sym}(X_i^\top X_iM_a)$ is a sum of i.i.d. bounded symmetric matrices. By the matrix Bernstein inequality (Tropp 2012, Theorem 6.1), $\|\widehat{\mathcal B}_A-\mathcal B\|_2\le\varepsilon$ with probability $\ge1-2d\exp(-m\varepsilon^2/(2B_X^4\rho_{\max}^2))$. Setting the failure probability to $\delta$ and solving for $\varepsilon$ gives the result. $\square$

For INCRT's architecture decisions to be correct, the estimation error must satisfy $B_X^2\rho_{\max}\sqrt{2d\log(2d/\delta)/m}\le\Delta_{\rm rel}$, i.e.,

<a id="equation-16-50"></a>
$$
m\ge\frac{2B_X^4\rho_{\max}^2\,d\,\log(2d/\delta)}{\Delta_{\rm rel}^2}.
\tag{16.50}
$$

<a id="pdf-0627"></a>
## PDF 627 · Book page 591

<a id="section-16-9-4"></a>
### 16.9.4 The PAC theorem

Combining Steps 1–3 via a union bound (failure probability $\delta/2$ for Rademacher + $\delta/2$ for operator estimation):

<a id="theorem-16-35"></a>
**Theorem 16.35 (PAC guarantee for INCRT).** Let $\mathcal B$ be the population task operator with directional complexity $\kappa_A=\Gamma_0/\theta_w$ and relevant gap $\Delta_{\rm rel}>0$. Let the inputs be bounded $\|X\|_F\le B_X$, the classifier be $L_g$-Lipschitz, and the motors bounded $\|M_a\|_F\le\rho_{\max}$. Then with probability $\ge1-\delta$ over $m$ i.i.d. training samples, the INCRT algorithm produces a terminal architecture with $\widehat K^*=K^*$ heads (correct architecture) and generalisation error $L(f_{\widehat K^*})\le\widehat L_m(f_{\widehat K^*})+\varepsilon$, provided

<a id="equation-16-51"></a>
$$
\boxed{m\ge\frac{C_{\rm PAC}\,B_X^4\,\rho_{\max}^2\,d}{\Delta_{\rm rel}^2}\cdot\log\!\left(\frac{d\,K^*}{\delta}\right),}
\tag{16.51}
$$

where $C_{\rm PAC}$ is a universal constant.

*Proof.* By the lemma stated above with failure probability $\delta/2$, the preceding condition guarantees that all eigenvalue-threshold comparisons are correct, so $\widehat K^*=K^*$. By Proposition the preceding result and the SRM envelope with failure probability $\delta/2$, the generalisation error at the correct architecture satisfies $L(f_{K^*})\le\widehat L_m(f_{K^*})+\varepsilon$ with $\varepsilon=O(K^*\sqrt{\log(d)/m})$. The dominant requirement is the operator-estimation condition above, giving the stated bound. $\square$

**Matching lower bound.** The sample-complexity bound is essentially tight in $d$ and $\Delta_{\rm rel}$:

<a id="theorem-16-36"></a>
**Theorem 16.36 (Lower bound on architecture identification).** For any algorithm $\mathcal A$ that takes $m$ i.i.d. samples from a distribution with task operator $\mathcal B$ and outputs an architecture with $\widehat K$ heads, there exists a pair of task operators $\mathcal B,\mathcal B'$ with $\|\mathcal B-\mathcal B'\|_2=2\Delta_{\rm rel}$ and $K^*(\mathcal B)\ne K^*(\mathcal B')$ such that

<a id="equation-16-52"></a>
$$
\Pr[\widehat K\ne K^*]\ge\frac14\qquad\text{whenever}\qquad m\le\frac{c\,d}{\Delta_{\rm rel}^2},
\tag{16.52}
$$

where $c>0$ is a universal constant.

*Proof sketch.* Construct $\mathcal B$ and $\mathcal B'$ that are identical except for one eigenvalue: $\lambda_j(\mathcal B)=\theta_w+\Delta_{\rm rel}$ and $\lambda_j(\mathcal B')=\theta_w-\Delta_{\rm rel}$. Under $\mathcal B$, the correct architecture has one more head than under $\mathcal B'$. By Le Cam's method, the total variation distance between $m$ samples from the two distributions is at most $\sqrt{m\,\|\mathcal B-\mathcal B'\|_F^2/(2B_X^4)}\le\sqrt{2m\,\Delta_{\rm rel}^2/B_X^4}$. For this to exceed $1/2$ (needed to distinguish the two hypotheses), one needs $m\ge B_X^4/(8\Delta_{\rm rel}^2)=\Omega(d/\Delta_{\rm rel}^2)$ after accounting for the $d$-dimensional structure of $\mathcal B$. $\square$

**Tightness.** The upper preceding bound scales as $O(d/\Delta_{\rm rel}^2\cdot\log(dK^*/\delta))$; the lower bound scales as $\Omega(d/\Delta_{\rm rel}^2)$. The gap is a logarithmic factor $\log(dK^*/\delta)$, which is standard in PAC learning and unlikely to be removable without additional structural assumptions.

**Interpretation.** The relevant gap $\Delta_{\rm rel}$ controls the sample complexity. When $\Delta_{\rm rel}$ is large (no eigenvalue is close to $\theta_w$, as in CoV-2 where $\Delta_{\rm rel}\approx\theta_w/2$), the bound is easily satisfied: a few hundred samples suffice. When $\Delta_{\rm rel}$ is small (eigenvalues cluster near the threshold, as in hard classification tasks), the bound requires more samples. the table verifies the bound on the experimental data.
