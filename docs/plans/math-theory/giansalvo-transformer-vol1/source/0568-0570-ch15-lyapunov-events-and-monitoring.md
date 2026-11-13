---
title: "Chapter 15: Lyapunov accounting, local event templates, and monitoring windows"
source_pdf_pages: [568, 570]
book_pages: [532, 534]
batch: "012"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: false
continues_after_file: false
---

# Chapter 15: Lyapunov accounting, local event templates, and monitoring windows

[Library index](../README.md) · [Source policy](../SOURCE.md)

> Editorial note: Full transcription of PDF 568–570 (printed pages 532–534). Original claims, formulae, captions, contradictions, and missing-image placeholders are retained. Editorial observations are identified explicitly. Running page headers are represented by the page markers below.

<a id="pdf-0568"></a>
## PDF 568 · Book page 532

<a id="section-15-22"></a>
## 15.22 Lyapunov proof sketch for the core controller

The complete proof in the original quadrittico was longer than the present core chapter requires. Still, the main Lyapunov idea should remain here, because it explains why the growth process does not continue indefinitely.

Let

<a id="display-0568-penalised-lyapunov"></a>
$$
\mathcal V(t)=a_\rho\rho(t)+a_\Gamma\Gamma_{\mathrm{res}}(t)+a_C C_{\mathrm{orb}}(t)+a_N|\mathcal H(t)|-a_V\sum_h V_{\mathrm{marg},h}(t).
$$

The positive terms penalise residual demand, amplification, crowding, and size. The negative term rewards useful marginal contribution. An event is accepted only if its validated change satisfies

<a id="equation-15-44"></a>
$$
\Delta\mathcal V_e(t)=\mathcal V_e(t+W)-\mathcal V(t)\le-\varepsilon_{\mathcal V}.
\tag{15.44}
$$

Events that do not satisfy this inequality are rejected, neutralised, or deferred. Since $\mathcal V$ is bounded below on the admissible architecture family, there cannot be infinitely many accepted events with a uniform decrease. This is the same argument as in Theorem 15.18, but written in the accounting language of the original INCRT proof.

The role of H1 is to ensure that $\Delta\mathcal V_e$ is measured with enough stability to justify the decision. The role of H2 is to provide a source of negative change when a genuine uncovered region exists. The role of H3 is to prevent a decrease in size or crowding from being obtained by deleting an essential directional tail. The role of H4 is to convert the absence of further validated decrease into a stopping rule.

This is the core proof pattern. The following chapter can generalise it to persistent drift, structural risk minimisation, transfer, and optimiser-specific preconditioning. The present chapter needs only the stationary and slow-drift controller argument.

<a id="section-15-23"></a>
## 15.23 Implementation-level event templates

The event family used by the controller should be explicit. Without explicit event templates, INCRT would be a principle rather than an architecture. The core chapter therefore records the four local events that are sufficient for the static-to-dynamic transition developed here. Each event has a trigger, a safe initialisation rule, an acceptance test, and a rollback rule.

<a id="section-15-23-1"></a>
### 15.23.1 Template I: add a new head

The first event adds a head near an uncovered region of the signature geography. The trigger is

<a id="display-0568-add-head-trigger"></a>
$$
\widehat\rho_\ell(t)>\theta_\rho,\qquad
\widehat\Gamma_{\mathrm{res}}^{(\ell)}(t)>\theta_\Gamma,\qquad
\widehat C_{\mathrm{orb}}^{(\ell)}(t)<\theta_C^{\max}
$$

with an uncovered maximiser $\sigma_\ell^\star(t)$. The head is initialised by a small perturbation of a template motor matching $\sigma_\ell^\star(t)$, while its output projection and gate start near zero. The event is accepted only if the validation window reports a decrease in $\rho_\ell$ or $\Gamma_{\mathrm{res}}^{(\ell)}$ and a non-negative marginal value after size and crowding penalties.

This template is the direct implementation of H2. It should be the most common growth event when the current layer simply lacks a representative atom.

<a id="section-15-23-2"></a>
### 15.23.2 Template II: split an overloaded head

The second event is used when the layer has enough heads in number but one head owns a dispersed region. Let $\mathcal R_h(t)$ be the responsibility region of head $h$. Define its internal

<a id="pdf-0569"></a>
## PDF 569 · Book page 533

dispersion by

<a id="display-0569-responsibility-dispersion"></a>
$$
D_h(t)=\int_{\mathcal R_h(t)}d\bigl(\sigma,\bar\sigma_h(t)\bigr)^2\,d\mu_\ell(t)(\sigma),
$$

where $\bar\sigma_h(t)$ is the local centroid. A split is triggered when $D_h(t)$ and $V_{\mathrm{marg},h}(t)$ are both high. Unlike adding a head, splitting preserves a useful computation and resolves its internal heterogeneity. It is therefore safer than adding a completely new head when the uncovered residual is near an already useful component.

The split event is accepted when the two children reduce internal dispersion or improve marginal value without increasing residual amplification. If the split fails validation, the children are merged back into the parent by averaging their output projections and restoring the parent gate.

<a id="section-15-23-3"></a>
### 15.23.3 Template III: widen the residual completion path

Not every residual should be represented by a new attention head. Some residual structure is not directional enough to justify a head and is better handled by the residual completion path. This is the role of feed-forward or low-rank completion growth. The trigger is

<a id="display-0569-completion-trigger"></a>
$$
\widehat\Gamma_{\mathrm{res}}^{(\ell)}(t)>\theta_\Gamma,\qquad
\widehat\rho_\ell(t)\le\theta_\rho,\qquad
\widehat V_{\mathrm{marg},h}(t)\text{ is diffuse across heads.}
$$

The interpretation is that the dictionary covers the signature geography, but there remains amplitude or nonlinear completion demand. Adding more heads would crowd the dictionary; widening the completion path is the correct event.

This distinction is important for the conceptual balance of the chapter. INCRT is not a blind head-growth mechanism. It is a capacity-placement mechanism. A head is added only when the missing structure is head-like.

<a id="section-15-23-4"></a>
### 15.23.4 Template IV: neutralise, prune, or preserve

The fourth event is the inverse of growth. A component is eligible for neutralisation when its marginal value is low and its responsibility region is covered by other components. It is eligible for deletion only after a hysteresis period. A directional anchor is not eligible for deletion unless a replacement anchor exists. The corresponding rule is

<a id="display-0569-anchor-exclusion"></a>
$$
h\in\mathcal H_A^{\mathrm{anchor}}(t)\quad\Longrightarrow\quad
h\notin\mathcal E_{\mathrm{prune}}(t)
$$

unless a child or neighbour satisfies the same anchor condition.

This event is what turns INCRT from a growth-only system into a self-regulating architecture. It also explains why the H3 experiment in this chapter is included even though it is small: preserving a directional tail may not minimise $\rho$ at the next step, but it prevents starvation of the antisymmetric channel.

<a id="section-15-24"></a>
## 15.24 Monitoring windows and two-time-scale operation

INCRT acts on a slower time scale than gradient descent. This separation is not an implementation convenience; it is a theoretical requirement. If the controller acts at every optimiser step, diagnostic noise is mistaken for architecture demand. If it acts too slowly, a missing component may delay training for too long. The correct regime is

<a id="display-0569-two-timescales"></a>
$$
1\ll W\ll T_{\mathrm{drift}},
$$

<a id="pdf-0570"></a>
## PDF 570 · Book page 534

where $W$ is the diagnostic window and $T_{\mathrm{drift}}$ is the time scale on which the signature distribution changes substantially.

Let $\widehat z(t)$ denote any diagnostic among $\rho_\ell$, $\Gamma_{\mathrm{res}}^{(\ell)}$, $C_{\mathrm{orb}}^{(\ell)}$, $V_{\mathrm{marg},h}$, or $\alpha_h$. The controller uses the windowed estimate

<a id="display-0570-windowed-estimate"></a>
$$
\bar z_W(t)=\frac1W\sum_{\tau=t-W+1}^{t}\widehat z(\tau)
$$

and the stability indicator

<a id="display-0570-stability-indicator"></a>
$$
S_W[z](t)=\frac1W\sum_{\tau=t-W+1}^{t}\left|\widehat z(\tau)-\bar z_W(t)\right|.
$$

A diagnostic is actionable only if

<a id="display-0570-actionability"></a>
$$
S_W[z](t)<\theta_z^{\mathrm{stab}}.
$$

This simple rule is the implementation form of H1. It prevents single-batch spikes from opening gates and it gives the validation window a well-defined statistical meaning.

<a id="remark-15-21"></a>
**Remark 15.21 (Why stability is checked before usefulness).** A tempting implementation would first check whether a growth event appears useful and then ask whether the diagnostic was stable. INCRT reverses the order. Unstable usefulness is not usefulness; it is a measurement artefact. Stability is therefore a precondition for event selection.

<a id="section-15-25"></a>
## 15.25 Core relationship with training dynamics

The preceding chapters analysed training dynamics through signatures, orbits, redundancy, and residual channels. INCRT uses that analysis operationally. The controller does not replace gradient descent; it changes the space in which gradient descent acts. Between two controller events the parameters are updated normally. At an event time, the architecture is modified so that subsequent gradient descent has access to a missing direction or no longer wastes effort on crowded components.

A useful schematic decomposition is

<a id="display-0570-tangent-update"></a>
$$
\Delta\theta(t)=\Delta\theta_\parallel(t)+\Delta\theta_\perp(t),
$$

where $\Delta\theta_\parallel$ lies in the tangent space already represented by the current architecture and $\Delta\theta_\perp$ points toward unresolved signature directions. A fixed architecture can only express $\Delta\theta_\perp$ indirectly, by bending existing components. INCRT instead adds or splits components so that a future update can represent this direction more directly.

This gives a training-dynamical interpretation of the coverage residual:

<a id="display-0570-tangent-interpretation"></a>
$$
\rho_\ell(t)\text{ high}\quad\Longrightarrow\quad
\text{the current tangent family is geometrically incomplete.}
$$

The marginal-value test then asks whether completing that tangent family matters for the loss. The crowding penalty asks whether the same effect can already be obtained by existing components. The anchor rule asks whether the proposed change would destroy a directional mode whose importance is not visible from short-term loss decrease alone.

This interpretation also clarifies why the experiments in this chapter are synthetic. They do not attempt to reproduce all of Transformer training. They isolate the controller logic: estimate a geometry, act on a stable residual, validate the action, and stop when no validated action remains.

---

[Continue with PDF 571](0571-0575-ch15-failure-cases-block-and-empirical-interface.md#pdf-0571)
