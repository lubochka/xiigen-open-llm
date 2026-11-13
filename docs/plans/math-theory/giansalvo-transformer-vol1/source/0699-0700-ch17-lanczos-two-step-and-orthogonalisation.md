---
title: "Chapter 17: two-vector Lanczos, claimed convergence, and the start of incremental QR"
source_pdf_pages: [699, 700]
book_pages: [663, 664]
batch: "014"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 17: two-vector Lanczos, claimed convergence, and the start of incremental QR

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: shifted power and RQI](0696-0698-ch17-shifted-power-and-rayleigh-quotient-iteration.md)

> Editorial note: Full transcription of PDF 699–700 (printed pages 663–664). The original restarted-Lanczos bound and proof, rate-squaring claim, projector-recovery claims, computational estimates, and unresolved “(?, §19.3)” citation are preserved. The range ends before the incremental-QR display (17.34), which begins on PDF 701; no steps from that display are included here.

<a id="pdf-0699"></a>
## PDF 699 · Book page 663

<a id="prescription-0699-two-vector-lanczos"></a>
**Construction for $m=2$.** Given $u_0\in S^{d-1}$, define

<a id="equation-17-26"></a>
$$
\alpha_0=u_0^\top Au_0,
\tag{17.26}
$$

<a id="equation-17-27"></a>
$$
w_1=Au_0-\alpha_0u_0,
\tag{17.27}
$$

<a id="equation-17-28"></a>
$$
\beta_0=\|w_1\|_2,
\tag{17.28}
$$

<a id="equation-17-29"></a>
$$
u_1=w_1/\beta_0,
\tag{17.29}
$$

so that $\{u_0,u_1\}$ is orthonormal by construction ($u_1^\top u_0=(Au_0-\alpha_0u_0)^\top u_0/\beta_0=(\alpha_0-\alpha_0)/\beta_0=0$). The action of $A$ on this basis is captured by the $2\times2$ tridiagonal matrix

<a id="equation-17-30"></a>
$$
T_2=\begin{pmatrix}\alpha_0&\beta_0\\\beta_0&\alpha_1\end{pmatrix},
\qquad\alpha_1=u_1^\top Au_1.
\tag{17.30}
$$

The Ritz values are the eigenvalues of $T_2$:

<a id="equation-17-31"></a>
$$
\widehat\lambda_\pm=\frac{\alpha_0+\alpha_1}{2}
\pm\sqrt{\left(\frac{\alpha_0-\alpha_1}{2}\right)^2+\beta_0^2}.
\tag{17.31}
$$

The Ritz vectors are obtained by applying the eigenvectors of $T_2$ in the basis $\{u_0,u_1\}$: $\widehat v_\pm=c_\pm u_0+s_\pm u_1$, with $(c_\pm,s_\pm)$ the normalised eigenvectors of $T_2$.

**Bidirectional structure.** The two Ritz values $\widehat\lambda_+$ and $\widehat\lambda_-$ approximate the *largest* and *smallest* eigenvalues of $A$ (assuming the Krylov subspace contains components along both extremes, which holds with probability one for random $u_0$). The corresponding Ritz vectors $\widehat v_+$ and $\widehat v_-$ approximate $v_1(A)$ and $v_d(A)$ respectively. This is precisely the bidirectional structure required by the gate: a single Lanczos pass simultaneously produces approximations to both probes $u^+$ and $u^-$, with mutual orthogonality guaranteed by construction.

<a id="lemma-17-9"></a>
**Lemma 17.9 (Convergence rate of Lanczos $m=2$; cf. Saad 2011).** *Let $A\in\operatorname{Sym}(\mathbb R^d)$ with eigenvalues $\lambda_1>\lambda_2\ge\cdots\ge\lambda_d$ and corresponding eigenvectors $v_1,\ldots,v_d$. Let $u_0\in S^{d-1}$ satisfy $|u_0^\top v_1|=\cos\theta_0\ne0$. Then after $t$ iterations of restarted Lanczos with $m=2$ (i.e., $t$ independent Lanczos passes, each starting from the current best Ritz vector $\widehat v_+$), the angular error satisfies*

<a id="equation-17-32"></a>
$$
\sin^2\theta_t\le\tan^2\theta_0\cdot
\left(\frac{\lambda_2-\lambda_d}{\lambda_1-\lambda_d}\right)^{2t}.
\tag{17.32}
$$

*Proof (classical bound, attributed to Kaniel–Paige–Saad).* The Kaniel–Paige–Saad bound (Saad, 2011, Theorem 6.4) for unrestarted Lanczos states that for Krylov depth $m$,

<a id="equation-17-33"></a>
$$
\sin^2\theta_m\le\tan^2\theta_0\cdot\frac1{T_{m-1}^2(\gamma)},
\qquad\gamma=1+2\cdot\frac{\lambda_1-\lambda_2}{\lambda_2-\lambda_d},
\tag{17.33}
$$

where $T_{m-1}$ is the Chebyshev polynomial of degree $m-1$. For $m=2$, $T_1(\gamma)=\gamma$, giving the single-pass bound $\sin^2\theta_2\le\tan^2\theta_0/\gamma^2\le\tan^2\theta_0\cdot\bigl((\lambda_2-\lambda_d)/(2(\lambda_1-\lambda_2))\bigr)^2$. The restarted version replaces $\theta_0$ by $\theta_{t-1}$ at each pass, giving a geometric decay with rate $\rho_{\mathrm{Lanczos}}=(\lambda_2-\lambda_d)/(\lambda_1-\lambda_d)$. The squared rate is then $\rho_{\mathrm{Lanczos}}^2=O((\lambda_2/\lambda_1)^2)$ in the regime $\lambda_1\gg|\lambda_d|$, recovering the well-known rate-squaring property of Lanczos. $\square$

**Comparison with Oja and shifted power.** Three relevant comparisons:

- *vs. Oja’s rule* (rate $\lambda_2/\lambda_1$): Lanczos $m=2$ achieves rate $(\lambda_2/\lambda_1)^2$, doubling the convergence speed in log scale per iteration.

<a id="pdf-0700"></a>
## PDF 700 · Book page 664

- *vs. shifted power* (rate $|\lambda_2-\mu|/|\lambda_1-\mu|$): Lanczos requires no a priori knowledge of $\mu$, yet achieves comparable rates without the risk of converging to the wrong eigenvector under poor shift estimation.
- *vs. RQI* (cubic local rate): Lanczos has slower asymptotic rate but is globally convergent and provides Ritz vectors as starting points for RQI’s local refinement. The two methods are complementary, and the unified gate of §17.5 uses Lanczos for scoping and RQI for refinement.

**Reorthogonalisation at $m=2$.** The classical pathology of Lanczos — loss of orthogonality among Krylov vectors after many iterations (Golub and Van Loan, 2013, §6.1.5) — does not occur at $m=2$, where only one orthogonalisation step is performed. Full reorthogonalisation costs $O(d)$ at $m=2$ and maintains machine-precision orthogonality. For larger Krylov depths $\tau_{\max}\in\{8,16\}$ used for the scoping phase of the unified gate, full reorthogonalisation costs $O(\tau_{\max}d)$ per pass, still subdominant to the cost of the matrix–vector products.

**Applicability.** Lanczos $m=2$ is applicable to all three regimes:

- **Type A:** the Ritz vectors $\widehat v_\pm$ converge to a basis pair of the dominant and trailing eigenspaces; in the degenerate case the basis selection depends on $u_0$ but the projector is recovered exactly.
- **Type B:** the rate $(\lambda_2/\lambda_1)^2$ degrades to $1-O(\Delta_+/\lambda_1)$, slow but well-defined.
- **Type C:** when combined with the rank-truncation of §17.3.3, Lanczos automatically excludes kernel eigenvalues from the Ritz values (since $|\widehat\lambda_k|<\theta_{\mathrm{rank}}$ implies $\widehat v_k$ is added to the deflation basis and removed from subsequent passes).

<a id="section-17-4-4"></a>
### 17.4.4 Incremental QR factorisation of the deflation basis

The deflation basis $Q^{(l)}\in\mathbb R^{d\times K}$ accumulates one column per Level-1 growth event and $m$ columns per Level-2 event, plus zero-cost kernel directions from §17.3.3. The deflation step $A_{\mathrm{res}}\leftarrow P^\perp A_{\mathrm{res}}P^\perp$ with $P^\perp=I-QQ^\top$ is exact in floating point only when $Q$ is orthonormal; loss of orthogonality propagates to the residual operator and silently violates the exact-deflation Lemma 16.1 of Chapter 16.

**The orthogonality drift problem.** Classical Gram-Schmidt orthogonalisation, when applied repeatedly to append new columns to $Q$, exhibits orthogonality drift with condition number $O(\kappa(Q)^2)$, where $\kappa(Q)$ is the condition number of $Q$ as a matrix (Golub and Van Loan, 2013, §5.2). Modified Gram-Schmidt improves this to $O(\kappa(Q))$ but is still suboptimal. The remedy is the Householder QR factorisation, which has backward-stable orthogonality bounds $O(\varepsilon_{\mathrm{mach}})$ independent of $\kappa(Q)$ (?, §19.3).

<a id="prescription-0700-incremental-qr-introduction"></a>
**Incremental QR via Givens rotations.** For the online INCRT setting, full Householder QR is too expensive (cost $O(dK^2)$ per growth event, dominated by the $K\times K$ block). The appropriate technique is *incremental QR*, in which each new column $u_k$ is appended via a sequence of Givens rotations that reorthogonalise it against the existing columns of $Q$:

> Batch boundary: PDF 700 ends immediately after the colon introducing the incremental-QR steps. PDF 701 begins the display “Given $Q^{(K)}=\widetilde Q^{(K)}R^{(K)}$ in QR form, append column $u_{K+1}$:”, numbered (17.34). That display and its algorithmic steps are outside this batch. Section 17.4.4 and Chapter 17 continue.

## Continuation in batch 015

The boundary above records the end of batch 014. The incremental-QR display (17.34), stability discussion, Table 17.2 and the remaining text of §17.4.4 are now available in [PDF 701–702](0701-0702-ch17-incremental-qr-and-spectral-method-comparison.md#pdf-0701). Section 17.4.4 spans PDF 700–702 and is now fully transcribed. Editorial analysis is in [note 293](../notes/293-incremental-qr-symbols-reprojection-and-stability.md).
