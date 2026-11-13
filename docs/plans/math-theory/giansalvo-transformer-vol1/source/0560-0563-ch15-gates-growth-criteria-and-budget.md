---
title: "Chapter 15: bidirectional gates, growth criteria, complexity budget, and diagnostic minimality"
source_pdf_pages: [560, 563]
book_pages: [524, 527]
batch: "012"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 15: bidirectional gates, growth criteria, complexity budget, and diagnostic minimality

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: core mechanisms](0556-0559-ch15-stability-cover-preservation-and-termination.md#section-15-14) · [Next: convergence and safe growth](0564-0567-ch15-convergence-safe-growth-and-core-algorithm.md#pdf-0564)

> Editorial note: Full transcription of PDF 560–563 (printed pages 524–527). Definitions and formulas retain the source's signs and conventions, including the different use of marginal value in (15.37). The opening paragraph of §15.17 continues on PDF 564.

<a id="pdf-0560"></a>
## PDF 560 · Book page 524

<a id="section-15-14-1"></a>
### 15.14.1 The bidirectional gate as the local actuator

The controller does not act directly on a global architecture variable. Its local actuator is a gate attached to a candidate component. In the original INCRT formulation this gate was bidirectional: it could open when a residual mode became stable and useful, and it could close when the corresponding component ceased to contribute. In the rationalised formulation the same idea is kept, but the gate is driven by measurable Chapter 14 quantities.

For a candidate component $h$ in layer $\ell$, define the gate score

<a id="equation-15-30"></a>
$$
S_{\ell h}(t)=b_0+b_\rho\widetilde\rho_{\ell h}(t)
+b_\Gamma\widetilde\Gamma_{\mathrm{res},\ell h}(t)
+b_V\widetilde V_{\mathrm{marg},h}(t)
-b_C\widetilde C_{\mathrm{orb},h}(t)
+b_A\widetilde\alpha_h(t),
\tag{15.30}
$$

where tildes denote normalised diagnostics on the current decision window. The gate is

<a id="equation-15-31"></a>
$$
g_{\ell h}(t)=\sigma\bigl(S_{\ell h}(t)\bigr),
\tag{15.31}
$$

with $\sigma$ the logistic function. A high coverage residual and a high marginal value open the gate. High crowding closes it unless the component is a protected directional anchor. The directional term is not a universal bonus. It is active only when the task geometry reports a directional tail or when the anchor-preservation rule has labelled the head as part of $\mathcal H_A^{\mathrm{anchor}}$.

The word *bidirectional* is important. The same gate is not only a growth switch. It is also a stabilisation and pruning switch. When $g_{\ell h}(t)$ increases across a decision window, the controller may accept the candidate component. When it decreases and the marginal value is small, the component becomes eligible for pruning. When the head is a directional anchor, low value is not sufficient for removal; the controller first requires a replacement or an explicit violation of the anchor condition. Thus the gate is not merely a differentiable mask. It is the local variable through which the architectural controller touches the network.

<a id="definition-15-13"></a>
**Definition 15.13 (Bidirectional INCRT gate).** A candidate component $h$ in layer $\ell$ is equipped with a bidirectional gate $g_{\ell h}(t)\in[0,1]$. The gate is *opening* on a window $W$ if

<a id="display-0560-opening-gate"></a>
$$
\frac1W\sum_{\tau=t-W+1}^{t}\Delta g_{\ell h}(\tau)>\epsilon_g,
$$

and *closing* if the same quantity is below $-\epsilon_g$. An opening gate may trigger growth when the coverage and residual conditions are satisfied. A closing gate may trigger pruning only when the component is not a protected anchor and its marginal value remains below threshold.

This definition gives the bridge between the old operational INCRT and the rationalised controller. The old gate tested whether the current architecture was insufficient. The new gate says which insufficiency is being measured, how it is stabilised over time, and which failure mode is being avoided.

<a id="section-15-14-2"></a>
### 15.14.2 Growth, pruning, and neutralisation

The gate can have three outcomes. First, it can open and add a component. Second, it can close and remove a component. Third, it can neutralise a component without deleting it. This third case is often overlooked, but it is important for knowledge preservation.

Let $F_{\ell h}(X;t)$ be the contribution of a head or channel to the layer update. The gated update has the schematic form

<a id="equation-15-32"></a>
$$
X_{\ell+1}=X_\ell+
\sum_{h\in\mathcal H_\ell(t)}g_{\ell h}(t)F_{\ell h}(X_\ell;t)
+R_\ell(X_\ell;t),
\tag{15.32}
$$

<a id="pdf-0561"></a>
## PDF 561 · Book page 525

where $R_\ell$ is the residual completion path. Growth adds a new term with small initial gate and controlled norm. Pruning removes a term after a hysteresis period. Neutralisation keeps the term but drives its gate toward a floor value, preserving the option to reopen it if the task geometry later returns to the corresponding region.

<a id="proposition-15-14"></a>
**Proposition 15.14 (Neutralisation before deletion).** Assume that layer updates are Lipschitz in the gated component and that the gate of a component $h$ is reduced from $g$ to $g'$ over a window rather than set to zero abruptly. Then the perturbation of the layer output satisfies

<a id="display-0561-neutralisation-bound"></a>
$$
\|\Delta X_{\ell+1}\|_F
\le|g-g'|\,L_h\|F_{\ell h}(X_\ell)\|_F+O(\eta W),
$$

where $L_h$ is the local Lipschitz constant and $O(\eta W)$ accounts for the training drift inside the window. Consequently, pruning after neutralisation is less disruptive than direct deletion.

The proposition is elementary, but it justifies a practical rule. INCRT should not delete useful structure immediately after the diagnostic falls. It should first neutralise, observe, and only then prune. This is the dynamic counterpart of the preservation rule already used for directional anchors.

<a id="section-15-15"></a>
## 15.15 Growth criteria

The original INCRT formulation used three ways of recognising architectural insufficiency. The first was geometric: the current architecture failed to capture a residual direction. The second was dynamical: training slowed down because the remaining gradient projected onto an unresolved subspace. The third was marginal: a new component produced a measurable improvement. In the old text these criteria were sometimes presented as essentially equivalent. In the present chapter a more careful statement is used. They are not identical in all regimes. They are aligned under the slow-drift, near-linear, and stable-window assumptions that make the INCRT controller meaningful.

<a id="section-15-15-1"></a>
### 15.15.1 The geometric criterion

The geometric criterion is the most direct one. It is expressed by the coverage residual

<a id="equation-15-33"></a>
$$
\rho_\ell(t)=\int d\bigl(\sigma,\mathcal D_\ell(t)\bigr)^2\,d\mu_\ell(t)(\sigma),
\tag{15.33}
$$

where $\mu_\ell(t)$ is the moving signature distribution and $\mathcal D_\ell(t)$ is the dictionary of represented atoms. A growth event is geometrically justified when there exists a region of positive mass for which $d(\sigma,\mathcal D_\ell(t))$ remains large across the decision window. In practice the controller uses the empirical maximiser

<a id="equation-15-34"></a>
$$
\sigma_\ell^\star(t)\in
\mathop{\arg\max}_{\sigma\in\operatorname{supp}(\widehat{\mu_\ell(t)})}
d\bigl(\sigma,\mathcal D_\ell(t)\bigr).
\tag{15.34}
$$

This is the criterion used in H2. It has the advantage of being local in signature space: the controller knows not only that the architecture is insufficient, but where it is insufficient.

<a id="section-15-15-2"></a>
### 15.15.2 The training-slowdown criterion

A residual region is useful only if it matters for the loss. Let $G_\ell(t)$ denote the layerwise gradient signal and let $P_{\mathcal D_\ell(t)}$ be the projection onto the span of currently represented signature directions. The unresolved gradient component is

<a id="equation-15-35"></a>
$$
G_\ell^\perp(t)=\bigl(I-P_{\mathcal D_\ell(t)}\bigr)G_\ell(t).
\tag{15.35}
$$

<a id="pdf-0562"></a>
## PDF 562 · Book page 526

A purely geometric hole with negligible $G_\ell^\perp$ should not trigger aggressive growth. Conversely, a moderate geometric hole may be important when the unresolved gradient component is large. This leads to the training-aware score

<a id="equation-15-36"></a>
$$
T_\ell(t)=\rho_\ell(t)^{1/2}\|G_\ell^\perp(t)\|_F.
\tag{15.36}
$$

The score does not replace $\rho_\ell(t)$; it filters it. The controller grows only when the uncovered region is also active in the training dynamics.

<a id="section-15-15-3"></a>
### 15.15.3 The marginal-value criterion

The third criterion is empirical and architectural. A proposed component is accepted only if its estimated marginal value is positive after a short validation window. If $\mathcal A$ is the current architecture and $\mathcal A+h$ the architecture with the candidate component, define

<a id="equation-15-37"></a>
$$
V_{\mathrm{marg},h}(t)
=\widehat{\mathcal L}_W(\mathcal A;t)
-\widehat{\mathcal L}_W(\mathcal A+h;t)
-\lambda_{\mathrm{size}}\Delta\operatorname{size}(h)
-\lambda_{\mathrm{crowd}}\Delta C_{\mathrm{orb}}(h).
\tag{15.37}
$$

This is the practical acceptance test. It prevents the geometric controller from adding heads that cover a region but do not improve the task or that improve it only by creating excessive crowding.

<a id="proposition-15-15"></a>
**Proposition 15.15 (Alignment of the three growth criteria).** Assume a stable diagnostic window, small parameter drift, a locally linearised training dynamics, and a candidate component whose signature lies within $O(\xi)$ of the uncovered maximiser $\sigma_\ell^\star(t)$. Then the geometric score $\rho_\ell(t)$, the unresolved-gradient score $T_\ell(t)$, and the marginal value $V_{\mathrm{marg},h}(t)$ are aligned in the following sense: a strict increase in the uncovered residual component that carries non-negligible gradient mass produces a positive first-order contribution to the expected marginal value, up to errors of order $O(\eta W+\xi+m^{-1/2})$.

**Proof sketch.** Over a stable window, the empirical signature distribution and the gradient projection vary by at most the H1 error. Adding a component near $\sigma_\ell^\star$ reduces the local geometric residual by H2. In the local linearisation, the loss decrease is proportional to the squared projection of the gradient onto the newly represented direction. This projection is exactly what $T_\ell(t)$ measures. The marginal-value estimate differs from the linearised decrease by sampling error, optimisation drift, and the crowding and size penalties. These terms give the stated error. The result is an alignment, not an identity: any of the assumptions can fail in a strongly nonlinear or rapidly drifting regime.

This proposition is the corrected version of the stronger equivalence sometimes suggested by the original text. It keeps the useful insight while avoiding an unnecessary claim. INCRT does not need the three criteria to be mathematically identical. It needs them to point in the same direction when the controller is allowed to act.

<a id="section-15-16"></a>
## 15.16 Directional task complexity and minimal head count

The original INCRT theory used a spectral quantity to estimate how many heads a layer should eventually contain. In the present chapter this quantity is not called a condition number in the classical numerical-analysis sense. It is a *directional task complexity index*. The distinction matters. A numerical condition number describes sensitivity of a problem to perturbations. The INCRT quantity measures how many stable directional or signature regions must be represented before residual demand saturates.

<a id="pdf-0563"></a>
## PDF 563 · Book page 527

Let $\Gamma_{\ell,0}$ denote the initial residual directional demand and let $\delta_{\ell,0}$ denote the smallest useful residual decrement that the controller accepts as a genuine architectural improvement. Define

<a id="equation-15-38"></a>
$$
\kappa_\ell^{\mathrm{task}}=\frac{\Gamma_{\ell,0}}{\delta_{\ell,0}}.
\tag{15.38}
$$

This number is not an exact head count. It is an upper scale for the number of useful accepted events when each event is required to reduce residual demand by at least $\delta_{\ell,0}$.

<a id="proposition-15-16"></a>
**Proposition 15.16 (Complexity-controlled growth budget).** Assume that every accepted growth event in layer $\ell$ reduces the monitored residual demand by at least $\delta_{\ell,0}>0$ until the termination tolerance is reached. If the initial residual demand is $\Gamma_{\ell,0}$, then the number $N_\ell$ of accepted growth events before termination satisfies

<a id="display-0563-growth-event-budget"></a>
$$
N_\ell\le
\left\lceil\frac{\Gamma_{\ell,0}-\epsilon_\Gamma}{\delta_{\ell,0}}\right\rceil
\le\left\lceil\kappa_\ell^{\mathrm{task}}\right\rceil.
$$

This result is deliberately modest. It does not say that the optimal number of heads is always $\lceil\kappa_\ell^{\mathrm{task}}\rceil$. It says that the dynamic controller cannot accept more useful growth events than the residual budget can pay for. In regimes where each stable signature component corresponds to one useful head, the bound becomes a good predictor of the terminal head count. In regimes with mixed heads, shared channels, or residual completion, it is an upper scale rather than an equality.

<a id="section-15-16-1"></a>
### 15.16.1 Minimality relative to a diagnostic family

Minimality in INCRT should also be understood relatively. The controller is not claimed to find a globally minimal Transformer among all possible architectures. It finds a minimal architecture relative to the diagnostic family it is allowed to observe and the event templates it is allowed to perform. This is the right notion for a constructive theory.

Let $\mathcal E_\ell$ be the event family available to the controller in layer $\ell$: adding a head, splitting an overloaded head, adding a channel, widening the residual completion path, or pruning a low-value crowded component. Let $\mathcal A(t)$ be the architecture produced by INCRT. The terminal architecture $\mathcal A(T)$ is diagnostically minimal if no event in $\mathcal E_\ell$ yields a stable improvement above threshold in any layer.

<a id="definition-15-17"></a>
**Definition 15.17 (Diagnostic minimality).** An INCRT terminal architecture is diagnostically minimal with respect to the event family $\mathcal E$ and the tolerance vector $(\epsilon_\rho,\epsilon_\Gamma,\epsilon_V)$ if, for every admissible event $e\in\mathcal E$,

<a id="display-0563-diagnostic-minimality"></a>
$$
\Delta_e\rho_\ell<\epsilon_\rho,\qquad
\Delta_e\Gamma_{\mathrm{res}}^{(\ell)}<\epsilon_\Gamma,\qquad
\Delta_e V_{\mathrm{marg}}<\epsilon_V
$$

on the validation window, unless the event is required by a preservation constraint.

This definition is stronger than merely saying that growth has stopped. Growth may stop because a maximum budget has been reached. Diagnostic minimality says something more: within the event family, no available local action can justify itself.

<a id="section-15-17"></a>
## 15.17 Convergence and homeostatic stability

The theoretical closure of the core chapter is not a large-scale convergence theorem for full Transformer training. Such a theorem would be unrealistic and would exceed the role of this chapter. The appropriate statement concerns the architecture controller. Under stable

[The sentence continues on PDF 564](0564-0567-ch15-convergence-safe-growth-and-core-algorithm.md#pdf-0564).
