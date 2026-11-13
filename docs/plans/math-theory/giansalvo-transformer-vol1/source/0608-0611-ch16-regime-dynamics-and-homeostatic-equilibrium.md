---
title: "Chapter 16: regime dynamics, persistent drift, and homeostatic equilibrium"
source_pdf_pages: [608, 611]
book_pages: [572, 575]
batch: "013"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 16: regime dynamics, persistent drift, and homeostatic equilibrium

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: depth growth and accounting](0604-0607-ch16-depth-growth-and-multilayer-accounting.md) · [Next: continual operation](0612-0614-ch16-continual-operation-and-input-conditional-complexity.md)

> Editorial note: Full transcription of PDF 608–611 (printed pages 572–575). The original future-directed discount sum, proof sketches, generic theorem references, and claimed equalities are retained. In particular, this transcription does not replace the source's future sum with a running average or insert missing assumptions into its rate and forgetting claims.

<a id="pdf-0608"></a>
## PDF 608 · Book page 572

**Rate of architectural convergence.** Each architectural event (growth, prune, or layer addition) reduces $V$ by at least $\varepsilon_V=\min(\delta_0^2/L_{\max},\phi_g^2/L_{\max})$. The total number of events is therefore bounded by

<a id="equation-16-19"></a>
$$
N_{\mathrm{events}}\le\frac{V(0)}{\varepsilon_V}
=\frac{L_{\max}\Gamma_0^2}{\min(\delta_0^2,\phi_g^2)}
=O\bigl(\kappa_A^4\log^2(\Gamma_0/\theta_w)\bigr),
\tag{16.19}
$$

using $L_{\max}=O(\kappa_A\log)$ and $\Gamma_0^2/\delta_0^2=\kappa_A^2$. This is a generous upper bound; in practice, the number of events is much smaller because most growth events capture several $\delta_0^2$ worth of residual energy.

<a id="section-16-4-6"></a>
### 16.4.6 Regime dynamics

A final extension connects the three-level growth dynamics to the NTK-motor bridge of INCRT I.

<a id="theorem-16-14"></a>
**Theorem 16.14 (Regime dynamics).** The ratio $L/N$ of active layers to total heads at convergence satisfies

<a id="equation-16-20"></a>
$$
\frac LN=\frac1{K_w},
\tag{16.20}
$$

where $K_w$ is the number of heads per layer at convergence, determined by the spectral structure of $\mathcal B$. The dNTK-motor bridge (INCRT I, Theorem B3) implies that this ratio is a continuous function of the task complexity $\kappa_A$, with limiting behaviour $L/N\to1/\kappa_A^2$ as $\kappa_A\to\infty$.

<a id="corollary-16-15"></a>
**Corollary 16.15 (Depth–width split).** The total complexity bound $N^*=\Theta(\kappa_A^2\log(\Gamma_0/\theta_w))$ splits as

<a id="equation-16-21"></a>
$$
L^*=\Theta\bigl(\kappa_A\log(\Gamma_0/\theta_w)\bigr),
\qquad K_w=\Theta(\kappa_A).
\tag{16.21}
$$

High-complexity tasks ($\kappa_A$ large) produce deep, narrow architectures; low-complexity tasks produce shallow, wide architectures. The SST-2 experimental result $\kappa_{\mathrm{SST2}}\approx13.2$ predicts approximately one layer per dozen heads, closely matching the BERT-base architecture used as a comparison baseline.

<a id="section-16-5"></a>
## 16.5 Persistent task drift and homeostatic equilibrium

The moving-target convergence theorem of this part of the construction guarantees tracking under *summable drift*: $\sum_t\beta_t<\infty$. Most realistic learning regimes satisfy this condition. Pretraining, fine-tuning to convergence, and stationary online learning all produce drifts that decay quickly enough. In each of these settings, the architecture grown by INCRT eventually stabilises and the residual error sits within a finite ball $R_\infty$ around the instantaneous optimum.

There is, however, a regime in which summability fails by design: continual learning, in which a sequence of tasks is presented indefinitely, each producing a non-vanishing perturbation of the running task operator. Here $\beta_t$ does not decay; it has a positive long-run average $\bar\beta>0$. The bound of the theorem stated in this part formally diverges ($R_\infty=\infty$), and the architecture cannot stabilise in the fixed sense of the theorem. What can replace stabilisation in this setting? The answer is a *homeostatic equilibrium* between growth and pruning.

The construction below shows that the same intrinsic quantity that controls the static head budget — the directional task complexity index $\kappa_A$ — also fixes the equilibrium between plasticity and stability under persistent drift. The architecture neither stabilises nor diverges: it reaches a stationary distribution in which growth events and pruning events balance, with

<a id="pdf-0609"></a>
## PDF 609 · Book page 573

a head budget that fluctuates around an effective $K_{\mathrm{eff}}^*$ determined by the drift level and the same $\kappa_A$ that appears in the compressed-sensing bound.

<a id="section-16-5-1"></a>
### 16.5.1 The discounted Lyapunov

The fixed Lyapunov $W_t=\sum_\ell\Gamma_{\mathrm{res}}^{(\ell)}(t)^2$ used in Theorems the theorem stated in this part and the theorem stated in this part weights the residual energy of the present moment equally with that of the distant past and future. Under persistent drift, this functional grows without bound: the contributions from the (infinitely many) future drifts make $\sum_{s\ge t}W_s=\infty$.

A natural replacement is the *discounted Lyapunov*

<a id="equation-16-22"></a>
$$
V_t^{(\gamma)}=\sum_{s\ge t}\gamma^{s-t}W_s,
\qquad\gamma\in(0,1).
\tag{16.22}
$$

For any $\gamma$ strictly less than one, the sum converges as long as $W_s$ is bounded: future contributions decay geometrically. The interpretation is that the architecture should care about future residual energy, but progressively less for events further away. The parameter $\gamma$ encodes a horizon: $1/(1-\gamma)$ is the effective number of timesteps over which residual energy is integrated.

<a id="theorem-16-16"></a>
**Theorem 16.16 (Discounted moving-target convergence).** Let $\mathcal B(t)$ be a sequence of task operators with bounded drift $\|\mathcal B(t+1)-\mathcal B(t)\|_F\le\beta_{\max}$ for all $t$, and let $\gamma\in(0,1)$. Under the assumptions of the theorem stated in this part, the discounted Lyapunov satisfies

<a id="equation-16-23"></a>
$$
V_t^{(\gamma)}
\le V_0^{(\gamma)}
\bigl(1-4c_0\eta^+(1-\gamma)\bigr)^t+R_\infty^{(\gamma)},
\qquad
R_\infty^{(\gamma)}
=\frac{\beta_{\max}^2}{(1-\gamma)\,4c_0\eta^+},
\tag{16.23}
$$

where $c_0=\delta_0^2/\Gamma_0^2$ as in the theorem.

*Proof sketch.* The argument adapts the Lyapunov decrease of the theorem. Within an inter-event interval, the gate flow contracts $\Gamma_{\mathrm{res}}(t)$ exponentially at rate $4c_0\eta^+$, and the perturbation due to the drift adds at most $\beta_{\max}^2$ per step. Multiplying the standard Lyapunov decrease by $\gamma^{s-t}$ and summing telescopes to the bound above. The constant $1-\gamma$ in the denominator of $R_\infty^{(\gamma)}$ arises from the geometric series of discount factors. The full computation is a direct extension of the proof in the technical appendix.

the theorem stated in this part reduces to the theorem stated in this part in the summable limit: when $\beta_t\to0$ along the trajectory and $\gamma\to1$ along a matching schedule, the discounted bound recovers the original. For $\gamma<1$ and persistent drift, the bound is finite, and the architecture tracks the moving optimum within a ball whose radius trades off against the discount. A more aggressive forgetting — smaller $\gamma$ — yields tighter tracking ($R_\infty^{(\gamma)}$ smaller) at the price of lower retention of past structure.

<a id="section-16-5-2"></a>
### 16.5.2 Growth and pruning rates from drift

The discounted Lyapunov controls residual energy in expectation, but the architectural consequences — how fast does the head count change? — require an independent estimate of the rates at which growth and pruning events fire under drift.

Consider the residual operator $A_{\mathrm{res}}^{(\ell)}(t)$ at layer $\ell$. Persistent drift in $\mathcal B$ propagates to $A_{\mathrm{res}}$, raising $\lambda_1(A_{\mathrm{res}}^{(\ell)})$ above $\theta_w$ on directions previously captured but no longer aligned with the current task. This triggers growth events. Symmetrically, the directional energy $\gamma_h$ of heads

<a id="pdf-0610"></a>
## PDF 610 · Book page 574

that are no longer well aligned falls below $\phi_g$, triggering pruning events. The two rates $\dot K_+(t)$ and $\dot K_-(t)$ depend on $\beta_t$ through the spectral perturbation of $A_{\mathrm{res}}$.

<a id="lemma-16-17"></a>
**Lemma 16.17 (Growth and pruning rates).** Under bounded drift $\beta_t\le\beta_{\max}$ and stationary distribution of the residual subspace, the expected growth rate at layer $\ell$ admits the bound

<a id="equation-16-24"></a>
$$
\dot K_+(\ell,t)\le\frac{\beta_t^2}{\theta_w\cdot\Delta_\ell^+},
\tag{16.24}
$$

where $\Delta_\ell^+=\lambda_1-\lambda_2$ is the spectral gap of $A_{\mathrm{res}}^{(\ell)}$. The expected pruning rate satisfies the symmetric bound

<a id="equation-16-25"></a>
$$
\dot K_-(\ell,t)\le\frac{\beta_t^2}{\phi_g\cdot\Delta_\ell^-},
\tag{16.25}
$$

with $\Delta_\ell^-=\lambda_{r-1}-\lambda_r$.

*Proof sketch.* The bound follows from spectral perturbation theory (Davis–Kahan). A drift $\beta_t$ rotates the eigenvectors of $A_{\mathrm{res}}$ by an angle bounded by $\beta_t/\Delta_\ell^+$; an eigenvalue formerly below $\theta_w$ rises above it when its squared rotation reaches $\theta_w$, which occurs in $\Theta(\theta_w\Delta_\ell^+/\beta_t^2)$ steps. The reciprocal is the rate. The analogue for pruning is identical with $\theta_w$ replaced by $\phi_g$ and $\Delta^+$ by $\Delta^-$.

This lemma already shows that the growth rate is quadratic in the drift and inversely proportional to the spectral gap. A small spectral gap (near-degenerate spectrum) makes the architecture jittery: small drifts cause many events. A wide spectral gap stabilises the architecture against drift. The remainder of the section explores when the two rates balance.

<a id="section-16-5-3"></a>
### 16.5.3 The homeostatic equilibrium

When $\dot K_+(\ell,t)>\dot K_-(\ell,t)$, the architecture grows on average; the head count drifts upward. When $\dot K_-(\ell,t)>\dot K_+(\ell,t)$, the architecture shrinks. A stationary regime — in which the head count fluctuates but does not drift — requires the two rates to balance.

<a id="theorem-16-18"></a>
**Theorem 16.18 (Homeostatic equilibrium).** Under bounded drift $\beta_t\le\beta_{\max}$ and the symmetric threshold relation $\phi_g=\theta_w/\kappa_A^2$, the expected growth and pruning rates at layer $\ell$ balance, $\mathbb E[\dot K_+]=\mathbb E[\dot K_-]$, when the discount factor of the running Lyapunov satisfies

<a id="equation-16-26"></a>
$$
\gamma_{\mathrm{hom}}=1-\frac1{\kappa_A^2}.
\tag{16.26}
$$

For $\gamma>\gamma_{\mathrm{hom}}$, growth dominates and the architecture expands; for $\gamma<\gamma_{\mathrm{hom}}$, pruning dominates and the architecture contracts.

*Proof sketch.* Setting $\dot K_+=\dot K_-$ in the lemma stated above gives $\theta_w\Delta^+=\phi_g\Delta^-$, which under the symmetric spectral assumption $\Delta^+\approx\Delta^-\approx\Delta$ reduces to $\phi_g/\theta_w=1$, contradicting the admissibility constraint $\phi_g<\theta_w$. The balance is therefore not attained at fixed thresholds but at the horizon of the running Lyapunov. Substituting $\phi_g=\theta_w/\kappa_A^2$ into the rate equality and matching to the discount factor of the theorem stated in this part (which controls how the running Lyapunov accumulates past residual energy) yields $\gamma_{\mathrm{hom}}=1-1/\kappa_A^2$. The full derivation is in the technical appendix.

the theorem stated in this part contains the central observation of this section. The homeostatic discount factor coincides exactly with the geometric decay rate $\rho=1-1/\kappa_A^2$ of the static compressed-sensing bound (the theorem stated in this part). The same intrinsic

<a id="pdf-0611"></a>
## PDF 611 · Book page 575

complexity that determines the head count of a static architecture — through $K^*=\kappa_A^2\log(\Gamma_0/\theta_w)$ — also determines the equilibrium horizon of a dynamic one: the architecture should integrate its diagnostics over $1/(1-\gamma_{\mathrm{hom}})=\kappa_A^2$ timesteps.

The duality has a clear reading. A static architecture sized for an ill-conditioned task (large $\kappa_A$) needs many heads. A dynamic architecture facing the same task under drift needs to remember its diagnostics over many timesteps. The two constraints are quantitatively the same.

**The effective head count.** At the homeostatic equilibrium, the architecture fluctuates around an effective count $K_{\mathrm{eff}}^*$ determined by the drift level. Combining the discounted bound of the theorem stated in this part with the static bound of the theorem stated in this part gives

<a id="equation-16-27"></a>
$$
K_{\mathrm{eff}}^*=\kappa_A^2
\log\left(\frac{\Gamma_0}{\theta_w+\beta_{\max}^2\kappa_A^2}\right).
\tag{16.27}
$$

For $\beta_{\max}\to0$ the equation reduces to the static $K^*$; for finite drift, the residual energy floor shifts upward by $\beta_{\max}^2\kappa_A^2$, shrinking the effective head count logarithmically.

<a id="section-16-5-4"></a>
### 16.5.4 A finite-time forgetting bound

The homeostatic equilibrium is a statement about head counts on average. A separate question concerns *retention*: when a task $\mathcal T_k$ has not been re-encountered for $t-t_k$ steps, how much of its representational structure has been lost?

Define forgetting on $\mathcal T_k$ at time $t\ge t_k$ as

<a id="equation-16-28"></a>
$$
F_k(t)=\|\mathcal B_k-\Pi_{\mathcal H(t)}\mathcal B_k\|_F^2,
\tag{16.28}
$$

where $\mathcal B_k$ is the task operator of $\mathcal T_k$ and $\Pi_{\mathcal H(t)}$ is the orthogonal projection onto the subspace spanned by the heads active at time $t$.

<a id="theorem-16-19"></a>
**Theorem 16.19 (Forgetting under homeostasis).** Under the homeostatic equilibrium of the moving-target theorem, with bounded drift $\beta_{\max}$, forgetting on a task $\mathcal T_k$ last encountered at time $t_k$ satisfies

<a id="equation-16-29"></a>
$$
\mathbb E[F_k(t)]
\le F_k(t_k)+(t-t_k)^2\beta_{\max}^2\kappa_A^4(1-\gamma)^2.
\tag{16.29}
$$

*Proof sketch.* The drift accumulates over the gap $t-t_k$ at rate $\beta_{\max}$ per step, projecting onto the subspace of $\mathcal B_k$ at rate $\kappa_A^2$ (the inverse of the spectral gap). Squaring and integrating gives the quadratic dependence on $(t-t_k)$ and the factor $\kappa_A^4$. The factor $(1-\gamma)^2$ measures how aggressively the running Lyapunov forgets past structure: at $\gamma=1$ (no discount, no plasticity) the bound vanishes; at $\gamma=0$ (full discount, no retention) it is maximal.

The bound interpolates between the two extremes. At $\gamma=1$ there is no forgetting — but the architecture cannot adapt either, because it never discounts past residual energy. At $\gamma=0$ there is no retention — but the architecture adapts maximally, because it weights only the current step. The homeostatic value $\gamma_{\mathrm{hom}}=1-1/\kappa_A^2$ sits between these extremes, and at this value the trade-off is balanced: the forgetting penalty $(1-\gamma_{\mathrm{hom}})^2=1/\kappa_A^4$ exactly cancels the $\kappa_A^4$ factor from the spectral perturbation.

<a id="corollary-16-20"></a>
**Corollary 16.20 (Plasticity-stability frontier).** At the homeostatic equilibrium, the forgetting bound becomes

<a id="equation-16-30"></a>
$$
\mathbb E[F_k(t)]\le F_k(t_k)+(t-t_k)^2\beta_{\max}^2,
\tag{16.30}
$$
