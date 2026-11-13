---
title: "Chapter 17: shifted power, Rayleigh quotient iteration, and the Krylov introduction"
source_pdf_pages: [696, 698]
book_pages: [660, 662]
batch: "014"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: false
continues_after_file: true
---

# Chapter 17: shifted power, Rayleigh quotient iteration, and the Krylov introduction

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: relative-gap test](0693-0695-ch17-kernel-detection-and-relative-gap-test.md) · [Next: Lanczos and orthogonalisation](0699-0700-ch17-lanczos-two-step-and-orthogonalisation.md)

> Editorial note: Full transcription of PDF 696–698 (printed pages 660–662), including the complete printed proof of Lemma 17.8 and Table 17.1. The omitted-cross-term argument, claims about shifts, PCG and computational cost, empirical numbers, and unresolved “?Parlett” citation are retained as in the original. Editorial analysis distinguishes the valid local RQI principle from these proof and implementation limitations.

<a id="pdf-0696"></a>
## PDF 696 · Book page 660

<a id="section-17-4"></a>
## 17.4 Classical spectral methods revisited

The bidirectional gate of Chapter 15 is, at its core, a stochastic power-method variant: Oja’s rule is the gradient-flow discretisation of the Rayleigh quotient ascent on the unit sphere, and MCA EXIN is the corresponding flow on the reciprocal end. Both are first-order methods with linear convergence rate. The numerical analysis literature, however, provides a rich catalogue of higher-order methods for the symmetric eigenvalue problem: shifted power iteration with quadratic improvement, Rayleigh quotient iteration with cubic local rate, Krylov-subspace methods with rate squared in the gap, and incremental QR factorisations with backward stability. None of these is new Wilkinson (1965); Parlett (1980); Saad (2011); Trefethen and Bau (1997); what is new is their adaptation to the online, homeostatic, self-architecting setting of INCRT.

This section revisits four classical methods and assesses their applicability to the three regimes of §17.3: power iteration with shift (§17.4.1), Rayleigh quotient iteration (§17.4.2), Lanczos with Krylov depth $m=2$ (§17.4.3), and incremental QR factorisation (§17.4.4). The closing comparison (§17.4.5) summarises rates, costs, and applicability in tabular form, providing the toolkit on which the unified gate of §17.5 is built.

<a id="section-17-4-1"></a>
### 17.4.1 Power iteration with shift

The simplest acceleration of the power method is the introduction of a *spectral shift*. Given $A\in\operatorname{Sym}(\mathbb R^d)$ and a scalar $\mu\in\mathbb R$, the shifted iteration

<a id="equation-17-16"></a>
$$
u_{t+1}=\operatorname{normalise}\bigl((A-\mu I)u_t\bigr)
\tag{17.16}
$$

converges to the eigenvector $v_k$ whose eigenvalue $\lambda_k$ maximises $|\lambda_k-\mu|$. The convergence rate is

<a id="equation-17-17"></a>
$$
\rho_{\mathrm{shift}}(\mu)=\frac{|\lambda_{\mathrm{next}}-\mu|}{|\lambda_{\mathrm{target}}-\mu|},
\tag{17.17}
$$

where $\lambda_{\mathrm{target}}$ is the targeted eigenvalue and $\lambda_{\mathrm{next}}$ the second-largest in absolute distance from $\mu$. By choosing $\mu$ close to $\lambda_{\mathrm{next}}$, the rate $\rho_{\mathrm{shift}}$ can be driven toward zero.

**Application to the bidirectional gate.** For the dominant probe $u^+$, take $\mu=\widehat\lambda_2$, the current estimate of the second-largest eigenvalue of $A_{\mathrm{res}}$. The rate becomes $|\lambda_2-\widehat\lambda_2|/|\lambda_1-\widehat\lambda_2|$, which under accurate $\widehat\lambda_2$ is small even when $\Delta_+=\lambda_1-\lambda_2$ is small. Symmetrically, for $u^-$ take $\mu=\widehat\lambda_{d-1}$.

**Limitation: dependence on shift accuracy.** The rate (17.17) improves only if $\mu$ is genuinely close to $\lambda_{\mathrm{next}}$. In the online INCRT setting, $\mu$ is itself estimated from the data, and a poor estimate can degrade the rate to that of the unshifted iteration or worse: if $\mu$ is closer to $\lambda_{\mathrm{target}}$ than to $\lambda_{\mathrm{next}}$, the iteration may converge to the *wrong eigenvector*. The Rayleigh quotient iteration (§17.4.2) addresses this limitation by adapting the shift online, achieving cubic local convergence at the cost of solving a linear system per step.

**Applicability to the three regimes.** Shifted power iteration is applicable to Type A (with any $\mu\ne\lambda_1$, the iteration converges to a basis vector of $V_m$ on suitable initialisation) and to Type B (with $\mu$ chosen between the cluster and the rest of the spectrum, the cluster is captured collectively). It is fragile in Type C: when $\mu$ is close to a kernel eigenvalue $\lambda_k\approx0$, the iteration $(A-\mu I)$ has small leading singular values and the normalisation step amplifies floating-point noise. The cure is to combine shifted power iteration with the rank-truncation of §17.3.3, restricting the iteration to the orthogonal complement of the numerical kernel.

<a id="pdf-0697"></a>
## PDF 697 · Book page 661

<a id="section-17-4-2"></a>
### 17.4.2 Rayleigh quotient iteration

The Rayleigh quotient iteration (RQI) is the adaptive-shift refinement of shifted power iteration. Given the current iterate $u_t\in S^{d-1}$, compute the Rayleigh quotient $\mu_t=R(u_t)=u_t^\top Au_t$ and update

<a id="equation-17-18"></a>
$$
u_{t+1}=\operatorname{normalise}\bigl((A-\mu_tI)^{-1}u_t\bigr).
\tag{17.18}
$$

The shift is updated at every step from the current best estimate of the target eigenvalue, achieving the following rate.

<a id="lemma-17-8"></a>
**Lemma 17.8 (Cubic local convergence of RQI; Wilkinson 1965).** *Let $A\in\operatorname{Sym}(\mathbb R^d)$ and let $v$ be a simple eigenvector of $A$ with eigenvalue $\lambda$. There exists a neighbourhood $U$ of $v$ in $S^{d-1}$ and a constant $C>0$ such that for any $u_0\in U$, the iterates $u_t$ of (17.18) satisfy*

<a id="equation-17-19"></a>
$$
\|u_{t+1}-v\|_2\le C\,\|u_t-v\|_2^3.
\tag{17.19}
$$

*Proof.* Spherical coordinates around $v$ are used. Decompose $u=v\cos\theta+v_\perp\sin\theta$ with $v_\perp\in\operatorname{span}\{v\}^\perp$, $\|v_\perp\|_2=1$, and let $Av_\perp=\lambda_\perp v_\perp+r$ where $r\perp v_\perp$ and $\lambda_\perp=v_\perp^\top Av_\perp$ is bounded on $\operatorname{span}\{v\}^\perp$ by $\|A\|_2$. The Rayleigh quotient at $u$ is

<a id="equation-17-20"></a>
$$
R(u)=u^\top Au
=\lambda\cos^2\theta+\lambda_\perp\sin^2\theta
+2\cos\theta\sin\theta\,v^\top Av_\perp
=\lambda\cos^2\theta+\lambda_\perp\sin^2\theta,
\tag{17.20}
$$

where the cross term vanishes because $Av=\lambda v$ and $v\perp v_\perp$. Hence

<a id="equation-17-21"></a>
$$
\lambda-R(u)=(\lambda-\lambda_\perp)\sin^2\theta
=\Delta_\perp\sin^2\theta,
\qquad\Delta_\perp:=\lambda-\lambda_\perp.
\tag{17.21}
$$

Since $v$ is simple, $\Delta_\perp\ge\min_{j\ne1}|\lambda-\lambda_j|=: \Delta_v>0$ provided $\theta$ is small enough that $v_\perp$ remains in the span of the non-target eigenvectors with bounded coefficients (this is the neighbourhood $U$).

The iteration (17.18) solves $(A-R(u_t)I)x=u_t$, which by (17.21) has a near-singular component along $v$ with amplification factor $1/(\lambda-R(u_t))=1/(\Delta_\perp\sin^2\theta_t)$. Writing $x=\alpha v+\beta v_\perp$ and projecting the linear system onto the eigenbasis,

<a id="equation-17-22"></a>
$$
\alpha(\lambda-R(u_t))=\cos\theta_t,
\qquad\beta(\lambda_\perp-R(u_t))+(\mathrm{cross})=\sin\theta_t,
\tag{17.22}
$$

with $\lambda_\perp-R(u_t)=-\Delta_\perp\cos^2\theta_t$ from (17.20). Therefore

<a id="equation-17-23"></a>
$$
\frac\beta\alpha
=\frac{\sin\theta_t}{\cos\theta_t}\cdot\frac{\lambda-R(u_t)}{\lambda_\perp-R(u_t)}
=\tan\theta_t\cdot\frac{\Delta_\perp\sin^2\theta_t}{-\Delta_\perp\cos^2\theta_t}
=-\frac{\sin^3\theta_t}{\cos^3\theta_t}.
\tag{17.23}
$$

After normalisation, $u_{t+1}=(\alpha v+\beta v_\perp)/\|\cdot\|_2$, so $\tan\theta_{t+1}=|\beta/\alpha|=\tan^3\theta_t$. For $\theta_t$ small, $\tan\theta_t=\sin\theta_t(1+O(\sin^2\theta_t))$, hence

<a id="equation-17-24"></a>
$$
\sin\theta_{t+1}=\sin^3\theta_t\bigl(1+O(\sin^2\theta_t)\bigr),
\tag{17.24}
$$

which gives $\|u_{t+1}-v\|_2=2|\sin(\theta_{t+1}/2)|\le C\|u_t-v\|_2^3$ for $u_t\in U$ with $C$ depending only on $\Delta_v^{-1}$. The standard treatment is (Parlett, 1980, Theorem 4.3.1); see also (Trefethen and Bau, 1997, Lecture 27). $\square$

**Empirical verification (cubicity in three steps).** A direct numerical test of the cubic rate on a synthetic $d=8$ symmetric matrix with well-separated spectrum, starting from $\sin\theta_0\approx0.30$, yields the trajectory in Table 17.1. The ratio $\sin\theta_{t+1}/\sin^3\theta_t$ remains $\approx1$ across the non-trivial regime, confirming Lemma 17.8. After only three iterations, $\sin\theta_3$ drops below machine precision; for comparison, Oja’s rule on the same problem (starting from the same $u_0$) reaches only $\sin\theta\approx1.4\cdot10^{-6}$ after 50 iterations. The practical implication for Algorithm 7 is that the RQI refinement step (Step 4, regular branch) terminates in $O(1)$ iterations once the Lanczos scoping has placed the iterate in the basin of attraction — the asymptotic $O(d^2/\sqrt{\Delta_+}\cdot\log\log(1/\varepsilon))$ cost is dominated by the constant prefactor of 2–4 RQI steps, not by $\log\log(1/\varepsilon)$.

<a id="pdf-0698"></a>
## PDF 698 · Book page 662

<a id="table-17-1"></a>
**Table 17.1:** Empirical convergence of Rayleigh quotient iteration on a synthetic $d=8$ symmetric matrix. The constant ratio $\sin\theta_{t+1}/\sin^3\theta_t\approx1$ confirms the cubic local rate of Lemma 17.8.

| Iteration $t$ | $\sin\theta_t$ | $\sin^3\theta_t$ | $\sin\theta_{t+1}/\sin^3\theta_t$ |
|---:|---:|---:|---:|
| 0 | $2.96\cdot10^{-1}$ | $2.58\cdot10^{-2}$ | 1.25 |
| 1 | $3.23\cdot10^{-2}$ | $3.38\cdot10^{-5}$ | 1.09 |
| 2 | $3.69\cdot10^{-5}$ | $5.04\cdot10^{-14}$ | (machine zero) |
| 3 | 0 | — | — |

**Cost per iteration.** The RQI step (17.18) requires solving the linear system $(A-\mu_tI)x=u_t$. Direct factorisation costs $O(d^3)$, prohibitive for the inner loop of an online algorithm. However, two observations make RQI practical in the INCRT setting:

1. The residual operator $A_{\mathrm{res}}=P^\perp\mathcal BP^\perp$ lives on the orthogonal complement of the deflation basis $Q$, hence has effective rank $d-K$. After the rank-truncation of §17.3.3, the effective rank is further reduced to $d-K-|\ker_{\mathrm{num}}|$.
2. The system $(A_{\mathrm{res}}-\mu I)x=u$ can be solved iteratively by preconditioned conjugate gradient (PCG) with $O(d^2)$ cost per CG iteration and $O(\sqrt\kappa)$ iterations to convergence, where $\kappa=(\lambda_{\max}-\mu)/(\lambda_{\min}^{>0}-\mu)$ is the condition number on the active subspace. For $\mu$ close to a target eigenvalue, $\kappa=O(1/\Delta_+)$, and the total cost per RQI step is $O(d^2/\sqrt{\Delta_+})$.

Combined with the cubic local rate, the total cost to reach precision $\varepsilon$ from a starting point in the basin of attraction is $O(d^2/\sqrt{\Delta_+}\cdot\log\log(1/\varepsilon))$, dominated by the linear-system solve.

**Caveat: locality.** Lemma 17.8 guarantees cubic convergence *locally*, in a neighbourhood of $v$. Outside this neighbourhood, RQI can oscillate unpredictably or converge to a different eigenvector than intended. The standard remedy is to precede RQI with a *scoping* phase that places the iterate in the basin of attraction; in the unified gate of §17.5, this scoping is performed by Lanczos with Krylov depth $\tau_{\max}$ (§17.4.3), which returns Ritz vectors that are guaranteed to be within the basin of the corresponding true eigenvectors.

**Applicability.** RQI is applicable to Type A (with $\mu_t$ converging to the cluster eigenvalue $\lambda_1$, the iteration converges to one specific basis vector of $V_m$, the choice depending on initialisation) and excellent for Type B (where the small gap is exactly the regime in which RQI’s cubic rate dominates). It is fragile in Type C: when $\mu_t$ approaches a kernel eigenvalue $\lambda_k\approx0$, the linear system $(A-\mu_tI)x=u_t$ becomes singular and the inverse amplifies floating-point noise without bound. This is the same pathology as in shifted power iteration (§17.4.1), and the same cure applies: rank-truncate before invoking RQI.

<a id="section-17-4-3"></a>
### 17.4.3 Lanczos with $m=2$ Krylov vectors

The Lanczos algorithm ?Parlett (1980) constructs an orthonormal basis of the Krylov subspace

<a id="equation-17-25"></a>
$$
\mathcal K_m(A,u_0)=\operatorname{span}\{u_0,Au_0,A^2u_0,\ldots,A^{m-1}u_0\},
\tag{17.25}
$$

and tridiagonalises $A$ on this subspace. The eigenvalues of the resulting $m\times m$ tridiagonal matrix, called *Ritz values*, approximate the extremal eigenvalues of $A$ at a rate that is the square of the rate of the power method. For $m=2$, the construction is particularly transparent and provides the building block of the unified gate.
