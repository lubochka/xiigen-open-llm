---
title: "Chapter 11: signature orbits, training-time compatibility, and two empirical traces"
source_pdf_pages: [365, 367]
book_pages: [329, 331]
batch: "008"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: false
continues_after_file: true
---

# Chapter 11: signature orbits, training-time compatibility, and two empirical traces

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous](0361-0364-ch10-empirical-status-and-synthesis.md#pdf-0364)

> Editorial note: Full transcription of PDF 365–367 (printed pages 329–331), including the chapter introduction, author-labelled epistemic status, all unresolved ?? references, Definition 11.1, §11.2 and the opening of §11.3. Forward references retain their author-assigned status; they are not independent verification of later results. Section 11.3 continues on PDF 368. Notes 106–107 separate these introductory claims from established results.

<a id="pdf-0365"></a>
## PDF 365 · Book 329

<a id="chapter-11"></a>
# Chapter 11. Signature Orbits and Training Dynamics

The preceding two chapters developed a formalism for the dynamics of a single attention head: the antisymmetric sector $M_a$ of the score matrix moves along a trajectory in the Lie algebra $\mathfrak{so}(d)$, driven by the gradient of the loss, until it reaches a fixed point. That analysis was, necessarily, local: it described the dynamics at a single layer, for a single head, in isolation.

Real Transformers are not single heads. They are stacks of layers, each inheriting the representation produced by the one below. The question this chapter asks is therefore different in kind from the ones that preceded it: not how does a single head learn, but how does the collection of all heads, operating in sequence, produce a representation that is useful for the task?

Two phenomena account for this. The first is geometric: the directional energy $\Gamma$ accumulated by the antisymmetric sector grows with depth, and this growth is not monotone by accident. It is the consequence of a positive-feedback mechanism — the **Residual Amplification Proposition** — that operates through the cumulative representation $X^{(l)}=X^{(0)}+\sum_{l'\le l}\Delta^{(l')}$ established in Chapter ??. The second part is diagnostic: the effective rank of the output projection $W_O^{(l)}$ decays with depth in a manner consistent with the predictions of the **Channel Destruction** mechanism of Chapter ??. These two facts together determine the layer-wise profile of a trained Transformer.

Both parts converge on a single quantity: the **residual directional energy** $\Gamma_{\mathrm{res}}$, defined here for the first time. This quantity measures, at any layer $l$ and any moment during training, the directional information that the current architecture is not yet representing. Its role in the design of the **Incremental Transformer** (Chapter ??) connects the dynamics of Part III to the architecture of Part V.

<a id="chapter-11-epistemic-status"></a>
**Epistemic status of results in this chapter.**

- **Proved (algebraically closed, verified numerically):** Canonical Symplectic Decomposition (Prop. 11.9), Rank Parity (Cor. 11.12), Monotone Decay of $\Gamma_{\mathrm{res}}$ (Prop. 11.46), Quantitative Decay Rate $2\rho^2$ (Cor. 11.47), Termination (Cor. 11.49), FLOPs Equivalence (Prop. 11.55), Optimal Minimum $d_k$ (Prop. 11.60), Post-LN Amplification Bounds (Prop. 11.19).
- **Conditional on (H2’)** (proved under the directional task condition $\partial\mathcal L/\partial\Gamma<0$, established in Lemma 11.34): Residual Amplification (Prop. 11.33), Amplification

<a id="pdf-0366"></a>
## PDF 366 · Book 330

Bifurcation (Prop. 11.38). Hypothesis (H2’) holds in gradient flow, in the NTK regime, and for any task rewarding directional attention.

- **Proof sketches (full derivations in Chapters 9–10):** Fixed-Point Classification (Prop. 11.14), Gap-Opening Transition (Prop. 11.16).
- **Conjectures (§11.11, verified in Chapter 18):** activation-depth correlation, activation-cone correlation, rank decay with depth, dominant-eigenvector alignment, antisymmetric NTK component.

<a id="chapter-11-earlier-connections"></a>
**Connections to earlier chapters.**

- The cumulative representation $X^{(l)}$ and the Rank-Neutrality Theorem (??) are used throughout without re-statement.
- The pair $(M^{(h)},N^{(h)})=(W_Q^{(h)}W_K^{(h)\top},W_V^{(h)}W_O^{(h)})$ and the diagnostic $r_{\mathrm{eff}}(W_O^{(l)})$ are as defined in Chapter ??, Remark ??.
- The decomposition $M=M_s+M_a$, the asymmetry index $\alpha_h=\lVert M_a\rVert_F/\lVert M\rVert_F$, and the Channel Destruction Proposition (??) are from Chapter ??.
- The coherence chain $I_{\mathrm{cone}}^{(l)}\nearrow\Rightarrow\Gamma^{(l)}\nearrow\Rightarrow\bar\Delta^{(l)}\nearrow\Rightarrow H^{(l)}\searrow$ is from Chapter ??, Theorem (Ch. 8).
- The motor formalism and the characterisation of the fixed points of the flow of $M_a$ in $\mathfrak{so}(d)$ are developed in full in Chapter ??; the antisymmetric score motor $G_a$ is from Chapter ??. This chapter uses these results without motor notation (see §11.5 and the boundary note there).

<a id="chapter-11-later-connections"></a>
**Connections to later chapters.**

- Chapter ?? (Pattern-Emergence Theorem) requires the activation sequence $\gamma_h^{(l)}$ and the Residual Amplification Proposition as direct prerequisites.
- Chapter ?? (Redundancy Quantified) uses the Layer Rank Decay Proposition to explain why $r_{\mathrm{eff}}(W_O^{(l)})$ falls with depth.
- Chapter ?? (INCRT) uses $\Gamma_{\mathrm{res}}$ as the core growth criterion; the formal definition given here (Definition 11.43) is the one that Chapter ?? operationalises.

<a id="section-11-1"></a>
### 11.1 Signature Orbits: The Training-Time Object

The preceding chapter fixed the local motor language of a self-attention head. It distinguished the score-side coordinates

<a id="display-11-score-side-coordinates"></a>
$$
\alpha_{\mathrm{SA}}^{(\ell,h)},\qquad\Gamma_s^{(\ell,h)},\qquad\Gamma_a^{(\ell,h)}
$$

from attention-side observables such as entropy, margin and attention asymmetry, and from signature-side observables such as the canonical signature distribution $\mu_\ell$ and residual coordinates $\rho_{\ell,h}$. The present chapter studies the motion of these quantities during training.

A head is therefore not represented by a single terminal point. It is represented by an orbit:

<a id="display-11-headwise-observable-curve"></a>
$$
t\longmapsto\Bigl(\alpha_{\mathrm{SA}}^{(\ell,h)}(t),\Gamma_s^{(\ell,h)}(t),
\Gamma_a^{(\ell,h)}(t),H^{(\ell,h)}(t),\operatorname{asym}^{(\ell,h)}(t),
\Sigma(A^{(\ell,h)}(t)),\rho_{\ell,h}(t)\Bigr).
$$

A layer is the empirical distribution of such headwise orbits. A trained model is the stack of these evolving distributions.

<a id="definition-11-1"></a>
**Definition 11.1 (Headwise signature orbit).** For a head $h$ in layer $\ell$, its signature orbit is the training-time curve

<a id="display-11-signature-orbit"></a>
$$
\mathcal O_{\ell,h}
=\left\{\Bigl(\alpha_{\mathrm{SA}}^{(\ell,h)}(t),\Gamma_s^{(\ell,h)}(t),
\Gamma_a^{(\ell,h)}(t),\Sigma(A^{(\ell,h)}(t)),\rho_{\ell,h}(t)\Bigr):t\ge0\right\}.
$$

<a id="pdf-0367"></a>
## PDF 367 · Book 331

When only the score-side and attention-side coordinates are measured, the resulting curve is called the **observable S–A orbit**.

This distinction matters. The compact SARS experiments below measure observable S–A orbits: S–A balances, symmetric and antisymmetric energies, attention entropy, attention asymmetry, and local S/A interventions. They do not yet measure the full canonical signature path $\Sigma(A(t))$. The theoretical role of this chapter is to explain why the measured orbit is already informative and why the missing signature-side projection is the correct next experimental target.

<a id="section-11-2"></a>
### 11.2 Saturation, Slack Tightening, and Two-Flow Propagation

The training-time compatibility chain should not be read as a naive differentiation of static inequalities. From $A(t)\ge B(t)$ one cannot conclude $\dot A(t)\ge\dot B(t)$. The correct dynamical statement is **slack tightening**: if the loss gradient is aligned with a diagnostic quantity on a training interval, then the slack in the corresponding compatibility inequality decreases up to second-order optimisation error.

For an observable $F(\theta)$, a first-order descent step gives

<a id="display-11-observable-descent"></a>
$$
F(\theta-\eta\nabla_\theta\mathcal L)-F(\theta)
=-\eta\langle\nabla_\theta F,\nabla_\theta\mathcal L\rangle+O(\eta^2).
$$

Thus a compatibility region is forward-stable only under an alignment condition between the observable gradient and the loss gradient. This is the saturation hypothesis of the chapter: the regime is not a formal stable manifold unless stronger hypotheses are added; it is a locally forward-invariant compatibility region.

On the S–A side, Chapter 10 showed that the effective score motor decomposes into Frobenius-orthogonal symmetric and antisymmetric channels. During training these channels produce two projected flows:

<a id="display-11-projected-sa-flows"></a>
$$
\partial_tM_s=-\eta G_s+O(\eta^2),\qquad
\partial_tM_a=-\eta G_a+O(\eta^2).
$$

The coordinate $\alpha_{\mathrm{SA}}$ is therefore a genuine orbit coordinate, not a post-hoc label. Its first-order motion is controlled by the two projected inner products

<a id="display-11-projected-inner-products"></a>
$$
\langle M_s,G_s\rangle_F,\qquad\langle M_a,G_a\rangle_F.
$$

The rest of the chapter asks how these headwise projected flows interact with residual amplification, rank decay, channel destruction and signature geography across depth.

<a id="section-11-3"></a>
### 11.3 Empirical Orbit Atlas: Two SARS Training Traces

Two compact SARS training traces are used as orbit probes. They are not presented as final biological classifiers. Their validation accuracy and macro-F1 remain weak. Their purpose is to test whether the S–A coordinates, their orbits and their causal interventions are measurable and informative.

The first trace is the *directional-tail trace*: a six-layer, eight-head compact encoder trained with a stronger learning rate. It produces a mostly symmetric network but retains a small number of heads with substantial antisymmetric balance and a strong antisymmetric sign-flip effect. The second trace is the *symmetric-collapse trace*: a lower-learning-rate, clipped-class-weight run that is numerically smoother but collapses almost entirely onto the majority class.

[Continue: PDF 368](0368-0372-ch11-empirical-orbit-atlas.md#pdf-0368)
