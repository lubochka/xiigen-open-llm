---
title: "Chapter 17: incremental QR and comparison of spectral methods"
source_pdf_pages: [701, 702]
book_pages: [665, 666]
batch: "015"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: false
---

# Chapter 17: incremental QR and comparison of spectral methods

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: incremental QR introduction](0699-0700-ch17-lanczos-two-step-and-orthogonalisation.md#prescription-0700-incremental-qr-introduction) · [Next: unified gate](0703-0705-ch17-unified-gate-algorithm-and-regularised-block-flow.md)

> Editorial note: Full transcription of PDF 701–702 (printed pages 665–666). Section 17.4.4 continues from PDF 700 and ends on PDF 702 before §17.4.5. The Q/tilde-Q notation, “Givens” wording, unresolved citation, table values, rate claims and cost conventions are preserved literally. Editorial checks appear separately in notes 293–294.

<a id="pdf-0701"></a>
## PDF 701 · Book page 665

<a id="continuation-section-17-4-4"></a>
### Continuation of 17.4.4 Incremental QR factorisation of the deflation basis

<a id="equation-17-34"></a>
$$
\begin{aligned}
&\text{Given }Q^{(K)}=\widetilde Q^{(K)}R^{(K)}\text{ in QR form,}\\
&\text{append column }u_{K+1}:\\
&\quad\widetilde u\leftarrow u_{K+1}-Q^{(K)}(Q^{(K)\top}u_{K+1})
&&\text{(projection)}\\
&\quad\widetilde u\leftarrow\widetilde u-Q^{(K)}(Q^{(K)\top}\widetilde u)
&&\text{(reprojection, MGS)}\\
&\quad r_{K+1}\leftarrow\|\widetilde u\|_2\\
&\quad q_{K+1}\leftarrow\widetilde u/r_{K+1}\\
&\quad Q^{(K+1)}\leftarrow[Q^{(K)}\mid q_{K+1}],\quad
R^{(K+1)}\leftarrow
\begin{pmatrix}
R^{(K)}&Q^{(K)\top}u_{K+1}\\
0&r_{K+1}
\end{pmatrix}.
\end{aligned}
\tag{17.34}
$$

The reprojection step (sometimes called “twice is enough” **?**) restores backward stability at the cost of one additional projection. The total cost per growth event is $O(dK)$, matching the cost of the deflation step itself.

**Stability bound.** With the incremental Givens-based QR, the orthogonality error of $Q^{(K)}$ after $K$ growth events satisfies

<a id="equation-17-35"></a>
$$
\|Q^{(K)\top}Q^{(K)}-I_K\|_2
\le c_1K\,\varepsilon_{\mathrm{mach}}+O(\varepsilon_{\mathrm{mach}}^2),
\tag{17.35}
$$

with $c_1$ a small geometric constant. Compared with the classical Gram-Schmidt bound $O(K^2\varepsilon_{\mathrm{mach}})$, the improvement is substantial: for $K\sim100$ growth events (typical in real INCRT runs) and $\varepsilon_{\mathrm{mach}}\sim10^{-16}$ (double precision), classical GS yields $\sim10^{-12}$ orthogonality error, while incremental Givens QR yields $\sim10^{-14}$.

**Empirical verification (stress test on ill-conditioned columns).** A direct numerical comparison on a $d=200$ ambient space, appending up to $K=150$ columns each strongly correlated with the previous one ($\cos\angle=0.999$), is reported in Table 17.2. The classical Gram-Schmidt error grows as $K^{1.98}$ in log-log fit (predicted: $K^2$), confirming the quadratic drift, and reaches $6.4\cdot10^{-11}$ at $K=150$. The twice-is-enough variant exhibits a near-flat error $\approx1.3\cdot10^{-15}$, two to three orders of magnitude better than the predicted $O(K\varepsilon_{\mathrm{mach}})$ floor: the second projection effectively quenches the $K$-dependence in regimes where the columns of $Q$ are ill-conditioned. The ratio of errors at $K=150$ is $\approx47\,000$, providing a quantitative justification for the choice of incremental QR in Step 6 of Algorithm 7.

<a id="table-17-2"></a>
**Table 17.2:** Empirical orthogonality drift of classical Gram-Schmidt versus twice-is-enough incremental QR, on a stress test with strongly correlated columns ($\cos\angle=0.999$ between consecutive columns) in $d=200$.

| $K$ | Classical GS error | Twice-is-enough error | Ratio |
|---:|---:|---:|---:|
| 10 | $1.6\cdot10^{-13}$ | $1.1\cdot10^{-15}$ | $145\times$ |
| 30 | $2.8\cdot10^{-12}$ | $1.1\cdot10^{-15}$ | $2\,500\times$ |
| 50 | $3.3\cdot10^{-12}$ | $1.2\cdot10^{-15}$ | $2\,800\times$ |
| 100 | $2.9\cdot10^{-11}$ | $1.3\cdot10^{-15}$ | $22\,000\times$ |
| 150 | $6.4\cdot10^{-11}$ | $1.3\cdot10^{-15}$ | $47\,000\times$ |
| slope $K^p$ | $p=1.98$ (theory: 2) | $p=0.08$ (theory: 1) | — |

**Consequence for the moving-target theorem.** The orthogonality error (17.35) enters the jump bound of Theorem 16.3 of Chapter 16 as a multiplicative factor: $J_{\max}$ scales linearly

<a id="pdf-0702"></a>
## PDF 702 · Book page 666

with $\|Q^\top Q-I\|_2$. Replacing classical Gram-Schmidt by incremental Givens QR reduces $J_{\max}$ by a factor $K$, equivalently reducing the required dwell time $T_{\mathrm{conv}}$ in Equation the dwell-time condition of Chapter 15 by the same factor. This is the most impactful single numerical improvement of the chapter.

**Applicability.** Incremental QR is orthogonal to the regime classification: it improves the deflation step independently of whether the spectrum is in Type A, B, or C. It is therefore active throughout the unified gate, regardless of the regime detected by the relative gap test.

<a id="section-17-4-5"></a>
### 17.4.5 Comparison and applicability summary

The five methods discussed in this section — Oja’s rule (recapped from Chapter 15), MCA EXIN (idem), shifted power, Rayleigh quotient iteration, Lanczos $m=2$, plus the orthogonal QR refinement — have complementary strengths and applicability. Table 17.3 summarises rates, costs, and regime applicability.

<a id="table-17-3"></a>
**Table 17.3:** Comparison of spectral methods for the bidirectional gate. Rate columns give the convergence rate (smaller is faster). Cost is per iteration. Applicability: ✓ = native, $\sim$ = applicable with care, × = fails or fragile.

| Method | Rate | Cost/iter | Type A | Type B | Type C |
|---|---|---|:---:|:---:|:---:|
| Oja (Cap. 15) | $\lambda_2/\lambda_1$ | $O(d)$ | ✓ | $\sim$ | × |
| MCA EXIN (Cap. 15) | $\lambda_{d-1}/\lambda_d$ | $O(d)$ | ✓ | $\sim$ | × |
| Shifted power | $\lvert\lambda_2-\mu\rvert/\lvert\lambda_1-\mu\rvert$ | $O(d)$ | ✓ | ✓ | × |
| Rayleigh quotient | cubic local | $O(d^2/\sqrt{\Delta_+})$ | ✓ | ✓ | × |
| Lanczos $m=2$ | $(\lambda_2/\lambda_1)^2$ | $O(d)$ | ✓ | ✓ | ✓$^*$ |
| Block MCA EXIN (Cap. 15) | $\Delta_m$ | $O(Tmd^2)$ | ✓$^\dagger$ | $\sim$ | $\sim$ |

$^*$ with rank truncation (§17.3.3). $^\dagger$ native via the block formulation (Theorem 16.5 of Chapter 16).

**Reading the table.** Three observations guide the design of the unified gate:

1. No single method covers all three regimes natively. Lanczos $m=2$ comes closest, with full coverage if combined with rank truncation, but its asymptotic rate is slower than RQI’s cubic.
2. The methods are *composable*: Lanczos provides a globally convergent scoping phase, RQI provides locally cubic refinement, Block MCA EXIN handles degenerate clusters, and incremental QR maintains deflation accuracy throughout.
3. The choice between methods is *data-driven*, not a priori. The relative gap test (Definition 17.6) provides the dispatcher: read the regime $(m,k_C)$ from the spectrum, choose the appropriate algorithmic branch.

The unified gate of §17.5 implements precisely this composition.

**What is gained over Chapter 15.** The bidirectional gate of Chapter 15 corresponds to the first two rows of Table 17.3: Oja for $u^+$, MCA EXIN for $u^-$. Its applicability is restricted to the regular regime, with documented fragility in Type B and outright failure in Type C. The unified gate extends coverage to all three regimes by adding Lanczos scoping (rows 5), RQI refinement (row 4), and block handling (row 6) as needed. The extensions are drop-in: when the regime is regular, the unified gate reduces to the Chapter 15 gate by construction, with only the QR improvement of §17.4.4 as an unconditional upgrade.
