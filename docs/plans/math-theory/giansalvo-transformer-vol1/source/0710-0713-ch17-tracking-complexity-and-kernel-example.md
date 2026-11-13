---
title: "Chapter 17: tracking, structural complexity, and the dimension-four kernel example"
source_pdf_pages: [710, 713]
book_pages: [674, 677]
batch: "015"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 17: tracking, structural complexity, and the dimension-four kernel example

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: conditional convergence](0706-0709-ch17-subspace-bias-and-conditional-convergence.md#corollary-17-12) · [Next: empirical diagnostics and signed gates](0714-0717-ch17-empirical-diagnostics-and-signed-gates.md)

> Editorial note: Full transcription of PDF 710–713 (printed pages 674–677). The original changes of count units, complexity notation, numerical outputs, code comments and experimental wording are retained. Listing 17.1 is transcribed as printed Python, with typography spacing normalised; executing the listing for an editorial check does not reproduce the other reported experiments.

<a id="pdf-0710"></a>
## PDF 710 · Book page 674

is the bias of the regularised Block MCA EXIN (Lemma 17.10), inherited at each Level-3 event that triggers a Type-A/B branch.

The dwell-time condition follows from substituting the external separation $\delta_{\mathrm{out}}$ for $\Delta_+$ in the dwell-time condition of Chapter 15; under (U2), $\delta_{\mathrm{out}}$ is bounded away from zero uniformly across growth events, so the dwell time is finite even when the internal spread $\delta_{\mathrm{in}}$ is zero (Type A) or arbitrarily small (Type B). $\square$

**Practical consequence.** For typical INCRT parameters $K\sim100$, $n\sim200$, the Chapter 15 jump bound is $J_{\max}=O(1/n+K\varepsilon_{\mathrm{mach}})\approx5\cdot10^{-3}$ in double precision, dominated by the $K\varepsilon_{\mathrm{mach}}\approx10^{-14}$ term only asymptotically. The unified gate gives $J_{\max}^{\mathrm{unified}}=O(1/n+\varepsilon_{\mathrm{reg}})\approx5\cdot10^{-3}$, identical to leading order but *floating-point robust*: the bound is independent of $K$, hence the constants in the moving-target convergence rate do not deteriorate as the architecture grows.

**Interpretive remark.** The refinement above does not improve the leading-order asymptotic size of the jump bound in the data-dominated regime; it improves its numerical ’envelope’. The dependence on the accumulated orthogonality drift of the deflation basis disappears, and the cluster separation $\delta_{\mathrm{out}}$ replaces the single-vector gap $\Delta_+$ whenever the dominant structure is genuinely multi-dimensional. This is the precise sense in which the unified gate should be read as the finite-arithmetic completion of the bidirectional gate rather than as a separate controller.

<a id="section-17-6"></a>
## 17.6 Implications, worked examples, and bridge to MHB-G

The theoretical framework of §§17.3–17.5 is now made concrete on three fronts. First, §17.6.1 draws an explicit accounting consequence of the rank-truncation step: the distinction between structural units (heads counted in the universal complexity bound) and kernel directions (subspaces excluded from the count by Type-C detection). Two numerical experiments then validate the unified gate: §17.6.2 executes the dimension-four example of Appendix 15.20 of Chapter 15 with explicit kernel detection (Type-C), and §17.6.3 demonstrates on a synthetic near-degenerate spectrum (Type-B) that the regularised Block MCA EXIN recovers the dominant subspace in 200 iterations while Oja’s rule and single-vector Lanczos saturate at the angular barrier $\sin\theta\approx1$. §17.6.4 re-interprets the SARS-CoV-2 trajectory of Chapter 18 through the lens of regime classification, recording a plausible reading of the flattening of $\Gamma_{\mathrm{res}}$ at $K=3$ as consistent with the onset of a Type-C-like regime, while leaving the empirical verification to Chapter 18. §17.6.5 then specialises the chapter’s numerical framework to the signed residual operator $A_h^{\mathrm{sig}}$ and the level-two protection rule introduced in Chapter 16, recording the gap, drift, and leakage analysis of the score $S_h(t)$ in finite arithmetic. The closing §17.6.6 connects the chapter to the Multi-Head Bilinear Gating architecture (Chapter 19), articulating the role of the unified gate as a numerical prerequisite for the routing top-$k$ mechanism.

<a id="section-17-6-1"></a>
### 17.6.1 Structural units vs. kernel directions

The universal complexity law of Chapter 16 (Corollary 16.9 there) states that

<a id="equation-17-45"></a>
$$
N^*=\Theta\bigl(\kappa_A^2\log(\Gamma_0/\theta_w)\bigr),
\tag{17.45}
$$

where $N^*$ is the total number of structural units (heads) at termination and $\kappa_A=\Gamma_0/\theta_w$ is the directional task complexity. (17.45) counts every growth event regardless of the spectral

<a id="pdf-0711"></a>
## PDF 711 · Book page 675

regime in which it occurs. The unified gate of §17.5 introduces a structural distinction that refines the count.

<a id="definition-17-13"></a>
**Definition 17.13 (Structural unit and kernel direction).** At a growth event of the unified gate (Algorithm 7):

- A *structural unit* is a head allocated in Step 5 of the algorithm. Its motor $M_a^{(k)}$ is non-zero, it contributes to the attention computation, and it is counted in $N^*$.
- A *kernel direction* is an eigenvector appended to the deflation basis $Q$ in Step 2 (rank truncation). It carries no motor and does not contribute to attention; it is excluded from $N^*$.

<a id="proposition-17-14"></a>
**Proposition 17.14 (Refined complexity bound).** *Let $N_{\mathrm{struct}}^*$ denote the number of structural units allocated by the unified gate, $K_C^*$ the total number of kernel directions accumulated, and $N_{\mathrm{defl}}^*=N_{\mathrm{struct}}^*+K_C^*$ the total number of deflated directions. Then*

<a id="equation-17-46"></a>
$$
N_{\mathrm{struct}}^*=N_{\mathrm{defl}}^*-K_C^*
\le N_{\mathrm{defl}}^*
=\Theta\bigl(\kappa_A^2\log(\Gamma_0/\theta_w)\bigr).
\tag{17.46}
$$

*The first equality is the accounting identity induced by the unified gate; the inequality is strict whenever $K_C^*>0$; the second equality is the universal bound of Chapter 16, which controls the total number of deflated directions. In particular, every kernel direction reduces the number of structural units relative to the total deflation count by one, and the structural count $N_{\mathrm{struct}}^*$ is bounded above by the same universal-law expression that controls $N_{\mathrm{defl}}^*$.*

*Proof.* The architecture grown by the unified gate satisfies the orthogonal-deflation principle (§17.4.4 preserves $Q$ orthonormal up to floating-point). Each structural unit and each kernel direction consumes one dimension of the deflation basis, so $N_{\mathrm{defl}}^*=N_{\mathrm{struct}}^*+K_C^*$. The universal-law analysis of Chapter 16 controls the number of deflated directions, giving $N_{\mathrm{defl}}^*=\Theta(\kappa_A^2\log(\Gamma_0/\theta_w))$ in the asymptotic sense. Subtracting $K_C^*\ge0$ from the identity gives (17.46). The strict inequality $N_{\mathrm{struct}}^*<N_{\mathrm{defl}}^*$ when $K_C^*>0$ is immediate from the same identity. $\square$

**Remark on the asymptotic nature of the bound.** The right-hand side of (17.46) is asymptotic in the sense of $\Theta(\cdot)$, not an exact equality, so the bound can be loose by a multiplicative constant even when $K_C^*=0$. The accounting identity $N_{\mathrm{struct}}^*=N_{\mathrm{defl}}^*-K_C^*$ is, however, exact: the role of kernel detection is to make a strict bookkeeping distinction within the deflation budget, not to tighten the universal bound itself.

**Geometric interpretation.** The universal-law bound counts *dimensions* of the residual space that must be deflated to bring $\|A_{\mathrm{res}}\|_F$ below threshold. The unified gate distinguishes two ways to deflate a dimension: by allocating a head with non-zero directional energy (structural unit), or by removing a numerically empty direction (kernel direction). Both contribute to the deflation, but only the former is paid for in the parameter budget of the architecture. This is the constructive content of the rank-truncation step: it identifies the subspace on which directional capacity is genuinely needed, before allocating it.

**When does it matter?** For tasks with full-rank task operator $\mathcal B$ and embedding dimension $d\ge\operatorname{rank}(\mathcal B)$, $K_C^*=0$ asymptotically and the refined bound is equivalent to the universal bound. Three settings produce $K_C^*>0$:

1. Embedding dimension $d>\operatorname{rank}(\mathcal B)$: surplus dimensions are kernel directions from the start.

<a id="pdf-0712"></a>
## PDF 712 · Book page 676

2. Saturation regime: as growth events accumulate, the residual rank decreases below $d$, and late growth events progressively encounter kernel directions.
3. Floating-point accumulation: even when $\mathcal B$ has full rank in exact arithmetic, the computed $A_{\mathrm{res}}^{(k)}$ accumulates $O(K\varepsilon_{\mathrm{mach}})$ errors that produce numerical kernel directions for $K$ sufficiently large.

The dimension-four example of §17.6.2 exemplifies case 1 with $d=4$ and $\operatorname{rank}(\mathcal B)=3$.

<a id="section-17-6-2"></a>
### 17.6.2 Numerical experiment 1: dimension four with kernel direction

Recall the setup of Appendix 15.20 of Chapter 15: $X\in\mathbb R^{6\times4}$ and $M_a\in\mathfrak{so}(4)$ are given by Equations 41–42 there, and the resulting task operator $\mathcal B=\operatorname{sym}(X^\top XM_a)$ has spectrum

<a id="equation-17-47"></a>
$$
\lambda_1\approx2.5094,\qquad
\lambda_2=0,\qquad
\lambda_3\approx-0.3731,\qquad
\lambda_4\approx-2.1363.
\tag{17.47}
$$

Listing 17.1 executes the unified gate on this operator, following Algorithm 7 step by step.

<a id="listing-17-1"></a>
~~~python
import numpy as np
from numpy.linalg import eigh, norm, qr

# Setup from Appendix B of Ch. 17
X = np.array([[1,0,0,0],[0,1,0,0],[1,1,0,0],
              [0,0,1,0],[0,0,0,1],[1,0,1,0]], float)
Ma = np.array([[0,2,0,0],[-2,0,1,0],[0,-1,0,0],[0,0,0,0]], float)
B = 0.5*(X.T@X@Ma + Ma.T@X.T@X)

# Step 1: Lanczos scoping (full eig for d=4)
ritz_values, ritz_vectors = eigh(B)
ritz_values = ritz_values[::-1]; ritz_vectors = ritz_vectors[:, ::-1]

# Step 2: Rank truncation (Type C detection)
eps_mach = np.finfo(float).eps
theta_rank = np.sqrt(eps_mach) * abs(ritz_values[0])
kernel_idx = np.where(np.abs(ritz_values) < theta_rank)[0]
# Result: kernel_idx = [1], v_kernel = e_4

# Step 3: Relative gap test on non-kernel spectrum
non_kernel = [k for k in range(4) if k not in kernel_idx]
rel_gap_top = (ritz_values[non_kernel[0]] - ritz_values[non_kernel[1]]) \
              / abs(ritz_values[non_kernel[0]])
# Result: rel_gap_top = 1.149 >> gamma_near = 0.05 -> regular regime

# Step 4: Bidirectional growth event (m=1, regular: RQI from Lanczos)
v1 = ritz_vectors[:, non_kernel[0]]   # u^+
vr = ritz_vectors[:, non_kernel[-1]]  # u^-

# Step 5-6: Allocate one bidirectional head, update Q
Q = np.column_stack([ritz_vectors[:, kernel_idx], v1, vr])
Q, _ = qr(Q)  # incremental QR
~~~

**Listing 17.1:** Unified gate on dimension-four example. Output is reported below the listing.

**Numerical results.** The execution of Listing 17.1 produces:

- $\theta_{\mathrm{rank}}=\sqrt{\varepsilon_{\mathrm{mach}}}\cdot\lambda_1\approx3.74\times10^{-8}$.

<a id="pdf-0713"></a>
## PDF 713 · Book page 677

- Kernel detected: index $k=1$, with $|\lambda_2|=0<\theta_{\mathrm{rank}}$, eigenvector $v_2=e_4$.
- Non-kernel spectrum: $\{\lambda_1,\lambda_3,\lambda_4\}=\{2.51,-0.37,-2.14\}$.
- Relative gap top: $\gamma_1^{\mathrm{nk}}=(2.51-(-0.37))/2.51\approx1.149$, well above $\gamma_{\mathrm{near}}=0.05$. Regular regime at the dominant end.
- Relative gap bottom: $\gamma_{d-1}^{\mathrm{nk}}=(-0.37-(-2.14))/2.14\approx0.825$. Regular regime at the trailing end.
- Bidirectional growth event: $u^+=v_1$, $u^-=v_4$ (in the notation of the original Appendix 15.20).
- Final deflation basis $Q\in\mathbb R^{4\times3}$ with orthogonality error $\|Q^\top Q-I\|_F\approx4.4\times10^{-16}$ (machine precision).

**Comparison with the Chapter 15 algorithm.** The Chapter 15 bidirectional gate produces the same final architecture ($K_{\mathrm{struct}}^*=1$ structural unit) on this example, but for a different reason: it does not detect the kernel direction $e_4$ explicitly, relying instead on the fact that $\lambda_2=0$ does not trigger the growth criterion $\lambda_{\max}(A_{\mathrm{res}})>\theta_w$. The unified gate certifies the same outcome by classification, providing a kernel-aware accounting:

<a id="equation-17-48"></a>
$$
N_{\mathrm{struct}}^*=1,\qquad
K_C^*=1,\qquad
N_{\mathrm{struct}}^*+K_C^*=2<N_{\mathrm{bound}}^*\approx4,
\tag{17.48}
$$

where $N_{\mathrm{bound}}^*$ is the universal-law upper bound from Equation 16.27 of Chapter 16. The architecture is sparser than the universal bound predicts, by Proposition 17.14, because of the explicit Type-C exclusion.

<a id="section-17-6-3"></a>
### 17.6.3 Numerical experiment 2: near-degenerate spectrum

A more striking comparison emerges in the Type-B regime, where the behaviour of the three methods (Oja, Lanczos $m=2$ single-vector, regularised Block MCA EXIN) differs qualitatively, not just quantitatively.

**Setup.** Consider $A_{\mathrm{res}}\in\operatorname{Sym}(\mathbb R^8)$ with prescribed spectrum

<a id="equation-17-49"></a>
$$
\lambda=(2.0,1.99,1.0,0.5,0.2,0.1,0.05,0.0).
\tag{17.49}
$$

The dominant gap is $\Delta_+=0.01$, with relative gap $0.005$, well below $\gamma_{\mathrm{near}}=0.05$: the spectrum is in the Type-B regime with $m=2$ at the dominant end. The regime detection of Definition 17.6 returns $m=2$, dispatching to the regularised Block MCA EXIN branch of Algorithm 7.

**Three methods compared.** Three convergence experiments are run on the same $A_{\mathrm{res}}$, starting from the same initial vector $u_0\in S^7$:

1. *Oja’s rule (Chapter 15):* single-vector iteration on $u^+$, 500 iterations with learning rate $\eta=0.01$.
2. *Restarted Lanczos $m=2$:* single-vector at each restart, 20 restarts ($\approx40$ matvec products).
3. *Regularised Block MCA EXIN ($m=2$):* block iteration on $W\in\operatorname{St}(2,8)$, 200 iterations with learning rate $0.05$ and regularisation $\varepsilon_{\mathrm{reg}}=10^{-8}$.

For Oja and Lanczos, the convergence metric is the angular error to $v_1$: $\sin\theta_t=\sqrt{1-(u_t^\top v_1)^2}$. For the block method, it is the principal subspace angle $\sin\angle(W_t,V_2)$, computed via SVD of $W_t^\top V_2$. Figure 17.7 shows the convergence curves on a logarithmic scale.
