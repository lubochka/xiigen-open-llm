---
title: "Chapter 15: stability, coverage decrease, anchor preservation, and finite termination"
source_pdf_pages: [556, 559]
book_pages: [520, 523]
batch: "012"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 15: stability, coverage decrease, anchor preservation, and finite termination

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: diagnostic regularity](0551-0555-ch15-state-events-and-diagnostic-regularity.md#assumption-15-1) · [Next: gates and growth criteria](0560-0563-ch15-gates-growth-criteria-and-budget.md#pdf-0560)

> Editorial note: Full transcription of PDF 556–559 (printed pages 520–523), preserving the stated hypotheses, bounds, proofs, and source terminology. Mathematical qualifications and counterexamples belong to the separate Russian notes and do not alter the author's theorem statements here.

<a id="pdf-0556"></a>
## PDF 556 · Book page 520

<a id="theorem-15-2"></a>
**Theorem 15.2 (Stability of diagnostic estimates over a growth window).** Under Assumption 15.1, for each layer $\ell$ and every confidence level $1-\delta$, with probability at least $1-\delta$,

<a id="equation-15-22"></a>
$$
W_2\bigl(\widehat\mu_\ell(t),\mu_\ell(t)\bigr)
\le C_\mu\eta_t m+C_\mu'\sqrt{\frac{\log(1/\delta)}m},
\tag{15.22}
$$

<a id="equation-15-23"></a>
$$
\left|\widehat\Gamma_{\mathrm{res}}^{(\ell)}(t)
-\Gamma_{\mathrm{res}}^{(\ell)}(t)\right|
\le C_\Gamma\eta_t m+C_\Gamma'\sqrt{\frac{\log(1/\delta)}m},
\tag{15.23}
$$

<a id="equation-15-24"></a>
$$
\left|\widehat V_{\mathrm{marg},h}^{(\ell)}(t)
-V_{\mathrm{marg},h}^{(\ell)}(t)\right|
\le C_V\eta_t m+C_V'\sqrt{\frac{\log(1/\delta)}m}.
\tag{15.24}
$$

Consequently, if

<a id="equation-15-25"></a>
$$
C\eta_t m+C'\sqrt{\frac{\log(1/\delta)}m}
\le\epsilon_{\mathrm{stab}},
\tag{15.25}
$$

then the controller can evaluate growth triggers with error at most $\epsilon_{\mathrm{stab}}$ on that window.

**Proof.** The proof separates drift and sampling noise. Over a window of length $m$, the parameter drift is bounded by $O(\eta_t m)$ under the local Lipschitz constants of Assumption 15.1. The empirical estimates are averages of bounded or sub-Gaussian quantities on the window, hence concentration gives the $O(\sqrt{\log(1/\delta)/m})$ term. The Wasserstein bound for $\mu_\ell$ follows from the Lipschitz dependence of signatures on parameters and the empirical measure concentration inequality on the signature space. Applying the same argument to $\Gamma_{\mathrm{res}}^{(\ell)}$ and $V_{\mathrm{marg},h}^{(\ell)}$ gives (15.23) and (15.24).

<a id="remark-15-3"></a>
**Remark 15.3 (Window calibration).** The bound has the usual bias–variance form. A very short window has high sampling noise; a very long window averages over a moving target. The useful window is the range in which both terms in (15.25) are below the hysteresis margin of the growth rule.

<a id="section-15-9"></a>
## 15.9 Coverage residual decrease under growth

The second dynamic requirement is that adding a head near an uncovered region actually reduces the coverage residual. This is the signature-geometric version of the residual deflation idea used in the original INCRT formulation.

<a id="assumption-15-4"></a>
**Assumption 15.4 (Implementable local cover).** Let $\sigma_\ell^\star(t)$ be defined by (15.11). Suppose that the new head can be initialised so that, after one adaptation window,

<a id="equation-15-26"></a>
$$
\mathbb E\left\|\sigma_{\mathrm{new}}^{(\ell)}(t+\Delta)
-\sigma_\ell^\star(t)\right\|^2
\le\xi_{\mathrm{init}}^2+\tau_{\mathrm{opt}}^2,
\tag{15.26}
$$

where $\xi_{\mathrm{init}}$ is implementation mismatch and $\tau_{\mathrm{opt}}$ is the optimisation lag over the adaptation window.

<a id="theorem-15-5"></a>
**Theorem 15.5 (Coverage residual decrease under correctly placed growth).** Assume Assumption 15.4. Let $r_\ell(t)=d(\sigma_\ell^\star(t),\mathcal D_\ell(t))$ and suppose $r_\ell(t)>2(\xi_{\mathrm{init}}+\tau_{\mathrm{opt}})$. Then adding a head near $\sigma_\ell^\star(t)$ decreases the expected coverage residual:

<a id="equation-15-27"></a>
$$
\mathbb E\bigl[\rho_\ell(t+\Delta)\mid\text{add at }\sigma_\ell^\star(t)\bigr]
\le\rho_\ell(t)-c_\ell r_\ell(t)^2
+O(\xi_{\mathrm{init}}^2+\tau_{\mathrm{opt}}^2+\epsilon_{\mathrm{stab}}),
\tag{15.27}
$$

where $c_\ell>0$ is the empirical mass of the uncovered neighbourhood selected by the growth rule. In particular, the decrease is strict whenever

<a id="equation-15-28"></a>
$$
c_\ell r_\ell(t)^2>
C(\xi_{\mathrm{init}}^2+\tau_{\mathrm{opt}}^2+\epsilon_{\mathrm{stab}}).
\tag{15.28}
$$

<a id="pdf-0557"></a>
## PDF 557 · Book page 521

**Proof.** Let $U$ be a neighbourhood of $\sigma_\ell^\star(t)$ assigned to the new atom after the growth event. Before growth, all points in $U$ are at distance at least of order $r_\ell(t)$ from the dictionary. After growth, the new atom lies within expected squared distance $\xi_{\mathrm{init}}^2+\tau_{\mathrm{opt}}^2$ of $\sigma_\ell^\star(t)$. The contribution of $U$ to the integral defining $\rho_\ell$ therefore decreases by at least a constant multiple of $r_\ell(t)^2$, up to implementation, optimisation, and estimation errors. The remaining part of the support is unaffected except for the same stability error, which gives (15.27).

**Relation to residual-energy deflation.** The original INCRT formulation often described growth as a deflation of residual directional energy. That view is retained as a special case. When signature distance is dominated by a residual directional coordinate, a decrease of $\rho_\ell(t)$ implies a decrease of the unresolved component of $\Gamma_{\mathrm{res}}^{(\ell)}(t)$. The present theorem is more general because it acts on the full signature geography rather than on a single spectral residual.

<a id="section-15-10"></a>
## 15.10 Directional-tail preservation

The third dynamic requirement prevents a common failure mode: early training may reward symmetric aggregation, causing growth to allocate new capacity away from directional tails. INCRT must not destroy directional anchors once they have stabilised.

<a id="proposition-15-6"></a>
**Proposition 15.6 (Directional-tail preservation under constrained growth).** Suppose that the preservation constraint (15.18) is enforced at every growth and pruning event. Then, for every layer $\ell$,

<a id="equation-15-29"></a>
$$
\left|\mathcal H_{A,\ell}^{\mathrm{anchor}}(t_{k+1})\right|
\ge\left|\mathcal H_{A,\ell}^{\mathrm{anchor}}(t_k)\right|
\tag{15.29}
$$

across all architecture-update times $t_k$. In particular, if the fractional constraint (15.19) holds at initialisation and every new pruning decision respects replacement, then directional starvation cannot occur.

**Proof.** There are three possible architecture events. Adding a head cannot remove an existing directional anchor. Splitting a head preserves the parent anchor or creates a child with comparable $\alpha_h$ and marginal value by the replacement condition. Pruning is forbidden for anchors unless another head already satisfies the anchor inequalities. Hence the anchor count is non-decreasing at update times. The fractional statement follows by induction over architecture updates.

<a id="remark-15-7"></a>
**Remark 15.7 (Why this is not optional).** The channel-intervention experiment of Chapter 14 showed that the symmetric and directional channels can become operationally distinct. Once this distinction has been learned, a growth controller that ignores $\alpha_h$ can accidentally erase the directional channel. The anchor rule is therefore not merely a regulariser. It is a structural safeguard.

<a id="section-15-11"></a>
## 15.11 Finite termination

The fourth dynamic requirement is termination. Without a termination theorem, incremental growth would be an uncontrolled expansion rule rather than an architecture selection mechanism.

<a id="assumption-15-8"></a>
**Assumption 15.8 (Lower-bounded improvement and finite useful cover).** For each layer $\ell$, assume that the signature distribution admits a finite $\epsilon_\rho$-cover with $K_\ell^\star<\infty$, and that every

<a id="pdf-0558"></a>
## PDF 558 · Book page 522

accepted growth event satisfying (15.28) decreases either $\rho_\ell$ or the unresolved component of $\Gamma_{\mathrm{res}}^{(\ell)}$ by at least $\delta_{\mathrm{grow}}>0$ until the termination tolerances are reached.

<a id="theorem-15-9"></a>
**Theorem 15.9 (Finite termination of the core INCRT controller).** Assume diagnostic stability, coverage decrease under correctly placed growth, directional-tail preservation, and Assumption 15.8. Then the core INCRT controller terminates after finitely many growth events. More precisely, there exists $T<\infty$ such that, for all $t>T$, no new growth event is triggered and criterion (15.20) holds for all layers.

**Proof.** The quantities $\rho_\ell(t)$ and the unresolved part of $\Gamma_{\mathrm{res}}^{(\ell)}(t)$ are non-negative. By Theorem 15.5 and Assumption 15.8, every accepted growth event decreases one of these non-negative quantities by at least $\delta_{\mathrm{grow}}$ until the tolerance region is reached. Therefore only finitely many such events can occur before all layers enter the tolerance region. Directional-tail preservation prevents this finite process from achieving termination by deleting stabilised directional anchors. Hysteresis in criterion (15.20) prevents immediate re-triggering caused by window noise. Thus, after finitely many windows, the controller stops growing.

<a id="corollary-15-10"></a>
**Corollary 15.10 (No unbounded growth under irrelevant residual energy).** If a residual component remains high but produces no coverage improvement and no marginal value above threshold, then it cannot trigger unbounded growth. It is absorbed by the termination rule as task-irrelevant residual energy.

<a id="section-15-12"></a>
## 15.12 Why the four dynamic hypotheses are sufficient

The four results above are not independent decorations. Together they form the minimal proof architecture of core INCRT.

1. H1 makes growth decisions reliable: the controller is not allowed to react faster than its diagnostics can be estimated.
2. H2 makes growth meaningful: a correctly placed head must reduce coverage residual in expectation.
3. H3 makes growth safe: previously stabilised directional tails cannot be destroyed by later pruning or symmetric over-allocation.
4. H4 makes growth finite: the process cannot continue indefinitely once the residual improvements fall below tolerance.

Thus INCRT is not simply a heuristic that adds heads. It is a controlled dynamic system. H1 controls measurement error, H2 controls descent, H3 controls invariant preservation, and H4 controls stopping. Removing any one of them creates a specific failure mode: noisy growth, useless growth, destructive growth, or unbounded growth.

<a id="section-15-13"></a>
## 15.13 Alignment with training criteria

The original INCRT text compared three growth criteria: a geometric residual criterion, an NTK-projected slowdown criterion, and an observed convergence criterion. The core chapter keeps the idea but weakens the claim. The criteria need not be exactly equivalent in a nonlinear Transformer. What is required is alignment under the near-linear regime in which the growth decision is made.

<a id="proposition-15-11"></a>
**Proposition 15.11 (Alignment of geometric and training criteria).** Assume that, on the growth window, the block is in a near-linear regime, the NTK varies by at most $\epsilon_K$, and the

<a id="pdf-0559"></a>
## PDF 559 · Book page 523

<a id="table-15-4"></a>
| Hypothesis | What it guarantees | Failure without it |
|---|---|---|
| H1 | Reliable diagnostic estimates on a growth window | Growth reacts to minibatch noise. |
| H2 | Accepted growth decreases coverage residual | Capacity is added without geometric improvement. |
| H3 | Directional anchors are preserved | Directional tails are destroyed by symmetric crowding. |
| H4 | Growth terminates in finite time | Persistent residual energy causes over-expansion. |

**Table 15.4:** Logical role of the four dynamic results in the core INCRT chapter.

diagnostic estimates satisfy Theorem 15.2. Then the geometric growth score $\mathcal G_\ell(t)$ in (15.12) and the projected training-slowdown score are aligned up to an error $O(\epsilon_K+\epsilon_{\mathrm{stab}})$. In particular, if the gap between the best growth direction and the second-best one is larger than this error, the two criteria select the same growth region.

**Proof sketch.** In a near-linear regime, the loss decrease along a candidate growth direction is controlled by the projection of the residual onto the local tangent feature space. The geometric score measures the same residual through coverage and residual amplification coordinates. Stability transfers empirical estimates to the current window, while the NTK drift contributes the $\epsilon_K$ term. A margin gap then gives agreement of the selected region.

<a id="remark-15-12"></a>
**Remark 15.12 (No uniqueness claim).** The proposition does not say that the geometric criterion is the unique correct criterion. It says that, in the regime where an incremental decision is meant to be stable, the geometric and training criteria are aligned. This is the claim needed for the dynamic controller.

<a id="section-15-14"></a>
## 15.14 Recovered core mechanisms from the original INCRT

The preceding sections deliberately translated INCRT into the dynamic variables of the Rationalised Transformer. This translation is necessary, but by itself it risks making the chapter appear thinner than the original INCRT formulation. The older formulation contained several mechanisms that are not peripheral. They are part of the operational identity of INCRT and must remain in the core chapter, although in a more disciplined form. This section therefore recovers the essential mechanisms of the original construction and rewrites them in the language of coverage, marginal value, directional anchors, and finite termination.

The guiding principle is simple. A mechanism is kept in this chapter when it is needed to implement the dynamic controller itself. A mechanism is deferred to INCRT II when it supplies an extension, an external guarantee, a large-scale application, or a different asymptotic interpretation. Thus the bidirectional gate, the growth criteria, the convergence argument, the minimality statement, initialisation, and knowledge preservation remain here. Compressed sensing, PAC-style architecture selection, persistent drift, adaptive optimiser theory, expert count, architectural transfer, and the broad application history are exported to the following chapter.

[Continue with §15.14.1 on PDF 560](0560-0563-ch15-gates-growth-criteria-and-budget.md#section-15-14-1).
