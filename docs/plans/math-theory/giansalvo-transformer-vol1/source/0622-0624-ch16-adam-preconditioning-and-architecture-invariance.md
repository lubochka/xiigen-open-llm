---
title: "Chapter 16: Adam preconditioning, convergence, and architectural invariance"
type: source-transcription
language: en
batch: "013"
source_pdf_pages: [622, 624]
book_pages: [586, 588]
---

# Chapter 16: Adam preconditioning, convergence, and architectural invariance

[Library index](../README.md) · [Source and transcription policy](../SOURCE.md)

<a id="pdf-0622"></a>
## PDF 622 · Book page 586

The preceding theorems make no statement about whether the gate still converges under such preconditioning, whether the compressed-sensing bound retains its $\kappa_A^2$ scaling under Adam-style updates, or whether the homeostatic equilibrium of INCRT I is preserved. A direct extension of the analysis is needed. The present section develops it.

The conclusion is that the gate convergence theorem (the theorem stated in this part) holds under Adam-style preconditioning, with the contraction rate replaced by an effective rate that *decouples* the gate dynamics from the directional task complexity index. The static head count of the theorem stated in this part is unchanged — the architecture grown under Adam has the same asymptotic head count as the one grown under SGD — but the time to reach it is reduced by a factor of $\kappa_A$. The framework is therefore not only compatible with adaptive optimisers but explains, structurally, why they are preferred in practice.

<a id="section-16-8-1"></a>
### 16.8.1 The Adam preconditioner

Let $g_t=\nabla_\theta\ell(\theta_t)$ denote the loss gradient at step $t$. The Adam update [citation] maintains exponential moving averages of the gradient and its squared coordinates:

<a id="equation-16-40"></a>
$$
m_t=\beta_1m_{t-1}+(1-\beta_1)\,g_t,
\tag{16.40}
$$

<a id="equation-16-41"></a>
$$
v_t=\beta_2v_{t-1}+(1-\beta_2)\,g_t\odot g_t,
\tag{16.41}
$$

with the parameter update

<a id="equation-16-42"></a>
$$
\theta_{t+1}=\theta_t-\eta_t\frac{\widehat m_t}{\sqrt{\widehat v_t}+\varepsilon_{\rm Adam}},
\tag{16.42}
$$

where $\widehat m_t=m_t/(1-\beta_1^t)$ and $\widehat v_t=v_t/(1-\beta_2^t)$ are bias-corrected. The decoupled variant AdamW adds a weight-decay term that does not affect the gate analysis below and is omitted for brevity.

The diagonal matrix $P_t=\operatorname{diag}(\sqrt{\widehat v_t}+\varepsilon_{\rm Adam})$ is the Adam preconditioner; the update can be written as $\theta_{t+1}=\theta_t-\eta_tP_t^{-1}\widehat m_t$, which is the form most useful for the analysis below. Two properties of $P_t$ are central.

- (a) *Coordinate-wise scaling:* $P_t$ acts independently on each coordinate, scaling the effective learning rate by the inverse root mean-square of recent gradients. Coordinates with consistently large gradients receive smaller steps; coordinates with small gradients receive larger ones.
- (b) *Asymptotic stability:* under bounded gradients with $\|g_t\|_\infty\le G$, the entries of $P_t$ are bounded above by $G$ and below by $\varepsilon_{\rm Adam}$, giving a uniformly bounded condition number $\sup_t\kappa(P_t)\le G/\varepsilon_{\rm Adam}$.

Property (b) is what allows the convergence analysis of Adam [citation] to proceed without uniform strong-convexity assumptions.

<a id="section-16-8-2"></a>
### 16.8.2 The preconditioned gate flow

The gate of INCRT I is a coupled system of two power-iteration-like flows: one for $u^+$ on $A_{\rm res}$ and one for $u^-$ on $A_{\rm res}$ as well, with antisymmetric coupling. Under SGD, both flows have the same effective learning rate $\eta^+$, and the contraction rate of the residual energy is $4c_0\eta^+$ (the theorem stated in this part). The two-time-scale assumption (A3) of that theorem — $\alpha\le(\Delta^+)^2\eta^+/(2\|X\|_F^2\|\nabla\ell\|_F)$ — requires the gate updates to be slow relative to the parameter updates.

Under Adam, the situation differs in two respects. First, the effective learning rate of each gate component is adapted to the local gradient scale. Second, the two coupled flows of the

<a id="pdf-0623"></a>
## PDF 623 · Book page 587

gate experience different gradient scales because $A_{\rm res}$ acts on $u^+$ along its leading eigenvector (with associated gradient norm proportional to $\Gamma_0$) and on $u^-$ along its trailing eigenvector (with gradient norm proportional to $\delta_0=\Gamma_0/\kappa_A$).

The Adam preconditioner therefore scales the two flows asymmetrically: the $u^-$ flow, with its smaller gradient norm, receives a proportionally larger effective learning rate, while the $u^+$ flow receives a smaller one. The asymmetry is precisely the inverse of the spectral asymmetry that produces the $\kappa_A^2$ factor in the SGD analysis. The two effects compose.

<a id="lemma-16-30"></a>
**Lemma 16.30 (Asymptotic preconditioner scales).** Let $g_t^+,g_t^-$ denote the gradient components of the gate along the $u^+$ and $u^-$ directions respectively, with $\mathbb E[\|g_t^+\|^2]=\Gamma_0^2/d$ and $\mathbb E[\|g_t^-\|^2]=\delta_0^2/d$. Under Adam updates with $\beta_2\in(0,1)$, the asymptotic preconditioner entries satisfy

<a id="equation-16-43"></a>
$$
P_\infty^+=\Gamma_0/\sqrt d,\qquad P_\infty^-=\delta_0/\sqrt d,
\tag{16.43}
$$

with convergence rate $1-\beta_2$. The ratio $P_\infty^+/P_\infty^-=\kappa_A$.

*Proof.* The exponential moving average defining the preconditioner converges to the stationary mean of $g_t\odot g_t$ at rate $1-\beta_2$. Taking expectations and bias-correcting yields the stated values. The ratio is the definition of $\kappa_A$. $\square$

The lemma is the technical lever for the analysis to follow. The asymptotic Adam preconditioner is a diagonal matrix whose ratio between the largest and smallest entries equals $\kappa_A$. When the preconditioner is inverted in the parameter update, this ratio inverts: the effective learning rate ratio $\eta_{\rm eff}^-/\eta_{\rm eff}^+$ becomes $\kappa_A$. The slow $u^-$ branch is accelerated by exactly the factor that the spectral asymmetry would otherwise impose as a slowdown.

<a id="section-16-8-3"></a>
### 16.8.3 Decoupled gate convergence

The asymmetric scaling identified by the lemma stated above modifies the gate convergence theorem. The proof of the theorem stated in this part relies on a bound of the form $\dot\Gamma_{\rm res}\le-4c_0\eta^+\Delta^+\Gamma_{\rm res}$, where the coefficient $c_0=\delta_0^2/\Gamma_0^2=1/\kappa_A^2$ comes directly from the spectral asymmetry. Under Adam, the effective learning rates absorb this asymmetry.

<a id="theorem-16-31"></a>
**Theorem 16.31 (Adam-aware gate convergence).** Let the gate of INCRT I be updated under Adam with parameters $(\beta_1,\beta_2,\varepsilon_{\rm Adam})$. Under the assumptions of the theorem stated in this part with (A3) replaced by the Adam-compatible

<a id="equation-16-44"></a>
$$
\alpha\le\frac{(\Delta^+)^2\eta_{\rm eff}^+}{2\|X\|_F^2},
\tag{16.44}
$$

the residual energy contracts at rate

<a id="equation-16-45"></a>
$$
\dot\Gamma_{\rm res}\le-4\eta^+\Delta^+\,\Gamma_{\rm res},
\tag{16.45}
$$

that is, with the $c_0=1/\kappa_A^2$ factor of the theorem stated in this part replaced by unity.

*Proof sketch.* The Adam preconditioner sets the effective learning rate ratio $\eta_{\rm eff}^-/\eta_{\rm eff}^+=\kappa_A$ at stationarity by the lemma. Substituting into the saddle-point Lyapunov of the theorem stated in this part cancels the $c_0=1/\kappa_A^2$ factor: where the SGD analysis writes $4c_0\eta^+\Delta^+=4\eta^+\Delta^+/\kappa_A^2$, the Adam analysis writes $4\eta_{\rm eff}^+\Delta^+\cdot\kappa_A^2\cdot1/\kappa_A^2=4\eta^+\Delta^+$. The remainder of the proof carries through unchanged; the full calculation is in the technical appendix. $\square$

The theorem says that Adam, applied to the bilateral gate of INCRT I, removes the dependence on $\kappa_A$ from the contraction rate. The gate converges at rate $4\eta^+\Delta^+$ under Adam, versus $4\eta^+\Delta^+/\kappa_A^2$ under SGD. The acceleration is exactly $\kappa_A^2$.

<a id="pdf-0624"></a>
## PDF 624 · Book page 588

**Decoupling.** The result has a transparent interpretation. Adam's coordinate-wise scaling normalises the two branches of the gate to operate at the same effective scale. The slow branch ($u^-$, with small natural gradient) is accelerated; the fast branch ($u^+$) is decelerated, and the two end up balanced. The spectral asymmetry that makes the SGD gate slow on ill-conditioned tasks does not appear in the Adam analysis: the preconditioner has cancelled it.

<a id="section-16-8-4"></a>
### 16.8.4 Invariance of the architecture

The contraction rate changes under Adam, but the asymptotic head count does not.

<a id="corollary-16-32"></a>
**Corollary 16.32 (Architecture invariance under Adam).** Under the conditions of the theorem stated in this part, the INCRT-grown architecture under Adam updates has the same asymptotic head count $K^*$ as under SGD updates, satisfying the compressed-sensing bound of the theorem. The time to grow each successive head is reduced by a factor of $\kappa_A$.

*Proof.* The compressed-sensing bound is a statement about the residual energy threshold $\theta_w$ and the spectral structure of $\mathcal B$, neither of which depend on the optimiser. The optimiser affects only the rate at which $\Gamma_{\rm res}\to\theta_w$. the theorem stated in this part gives the new rate; integrating the faster contraction yields the time reduction. $\square$

The corollary closes the loop on the central question of the section. The static structural result — the optimal head count $K^*=\kappa_A^2\log(\Gamma_0/\theta_w)$ — is a property of the task, not of the optimiser used to reach it. Adam reaches it faster, but does not change what it is. The same statement, with the same proof structure, applies to the homeostatic equilibrium of the theorem stated in this part, the optimal expert count of the theorem stated in this part, and the architectural distance of the theorem. The four extensions of the framework developed in INCRT I and the present extension block are all optimiser-invariant in the same sense.

<a id="section-16-8-5"></a>
### 16.8.5 Beyond Adam: AdamW, Lion, and second-order methods

Three optimiser families are in active use beyond bare Adam.

**AdamW.** The decoupled weight decay [citation] adds a term $-\eta_t\lambda_{\rm wd}\theta_t$ to the parameter update, unrelated to the gradient. The weight decay reduces the running parameter norm but does not modify the gate dynamics, which depend only on the directions $u^+,u^-$ and the preconditioner ratio. the theorem stated in this part therefore applies to AdamW unchanged.

**Lion.** Lion [citation] replaces the magnitude-based preconditioning of Adam with a sign-based update, $\theta_{t+1}=\theta_t-\eta_t\operatorname{sign}(\beta_1m_{t-1}+(1-\beta_1)g_t)$. The sign operation is a coordinate-wise normalisation to unit absolute value, independent of the gradient magnitude. The asymptotic preconditioner ratio of the lemma stated above becomes $P_\infty^+/P_\infty^-=1$ (both branches normalised to unit scale), and the theorem stated in this part holds with the same conclusion: the contraction rate is independent of $\kappa_A$. The argument is in fact tighter for Lion: the sign operation removes *any* non-uniformity, not only the spectral one.

**Second-order methods.** Shampoo [citation] and its descendants (SOAP [citation], Muon [citation], AdamW-style Shampoo [citation]) maintain non-diagonal preconditioners that capture left- and right-multiplicative curvature. For the gate dynamics — which act on a low-dimensional subspace of the parameter space — the relevant quantity is the preconditioner restricted to the gate subspace. On this subspace, the second-order preconditioner
