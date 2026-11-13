---
title: "Chapter 12: from signature orbits to sharpness, activation, and silence"
source_pdf_pages: [415, 416]
book_pages: [379, 380]
batch: "009"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: false
continues_after_file: true
---

# Chapter 12: from signature orbits to sharpness, activation, and silence

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: PDF 408–414](0408-0414-ch11-proofs-and-bibliographic-notes.md#pdf-0414)

> Editorial note: Full transcription of PDF 415–416 (printed pages 379–380), including the chapter title, §12.1 and the beginning of §12.2. The grammatical repetitions, identification of residual energy with “rank decay”, and the original wording of the closed-gate interpretation are preserved. The last sentence continues on PDF 417. Russian note 130 distinguishes the three proposed observables from the canonical signature and from claims that have not been established by this introduction.

<a id="pdf-0415"></a>
## PDF 415 · Book 379

<a id="chapter-12"></a>
### Chapter 12

## Sharpness, Activation, and Silence

<a id="section-12-1"></a>
### 12.1 From orbits to emergence

Chapter 11 produced an object that the local theory of Chapters 6–7 did not have: a *trajectory in signature space*. A head $(\ell,h)$ no longer occupies a single canonical regime; it traces, across training time, a curve $t\mapsto\widehat\lambda^{(\ell,h)}(t)\in\Delta_8$ in the canonical signature simplex, with an attached residual $t\mapsto\rho_8^{(\ell,h)}(t)$ measuring how far the head sits from the canonical hull at each moment of training. The orbit $\mathcal O_h$ of the head is the image of this curve. Chapter 11 catalogued the orbits, identified saturation and slack-tightening, and recorded the layer-wise rank decay $\Gamma_{\mathrm{res}}^{(\ell)}$ that the orbit-level dynamics induce on the residual stream.

The natural reading of these orbits is operator-theoretic, not behavioural. An orbit that converges to the directional vertex $e_{\mathrm{dir}}\in\Delta_8$ has not just acquired the visual appearance of a vertical line: it has acquired a specific operator condition, namely a uniform exterior margin in the antisymmetric sector against a single column. An orbit that converges toward the closed gate has not just become uniform: it has lost both exterior margin and intra-support flatness with respect to every preferred support. The empirical content of an orbit is exhausted by the operator-level conditions on $M_s$ and $M_a$ that produce the canonical regime at its endpoint, together with the rate at which those conditions tighten.

This chapter makes the operator-level reading precise. Three coordinates emerge, each measurable per-head and per-layer, each computable on a forward pass, and each — crucially — separable from the others.

**Sharpness.** The sharpness coordinate quantifies *how strongly a head has committed to its preferred regime*. A head whose pre-softmax logits carry a marginal exterior gap of order one against the rest of the row produces a soft attention map; the same head with logits scaled by a temperature $\beta\gg1$ produces a sharp map indistinguishable from the canonical regime. Sharpness, then, is the rate at which the residual $\rho_8^{(\ell,h)}$ collapses to zero as a function of the underlying logit margin. It is the dynamical content of the relative emergence theorem of §6.18.

**Activation.** The activation coordinate quantifies *whether the head's geometry is seen by the input cloud at all*. A head with a non-zero antisymmetric sector $M_a^{(\ell,h)}$ may produce, on a particular input cloud $X^{(\ell)}\in\mathbb R^{n\times d}$, an antisymmetric logit matrix $X^{(\ell)}M_a^{(\ell,h)}(X^{(\ell)})^\top$ that is identically zero. The geometry of the head exists, but the geometry is unseen. Activation is the intrinsic energy with which the head's antisymmetric sector acts on the row span of

<a id="pdf-0416"></a>
## PDF 416 · Book 380

the cloud, made precise in §12.3.

**Silence.** The silence coordinate is the boundary case of activation: the configurations in which activation vanishes identically. The local Theorem 6.66 of Chapter 6 characterised silence as isotropy of the row span for the alternating bilinear form induced by $M_a$. The characterisation is retained that characterisation here, lift it from a single layer to a depth-indexed observable, and add the algebraic-geometric reading of the silence locus as a Schubert cycle in the Grassmannian.

The three coordinates do not classify heads — the canonical signature $\widehat\lambda^{(\ell,h)}$ already does that. They *measure* signatures. A head whose signature is concentrated on the directional vertex with high sharpness, full activation, and zero silence is qualitatively different from a head whose signature is concentrated on the same vertex with low sharpness, partial activation, and a non-zero residual silence component, even though both have the same canonical projection. Chapter 13 will use the quantitative content of the three coordinates to define redundancy, to extract the six design principles, and to measure the marginal value of a head; Chapter 14 will use the resulting prescriptions to construct the rationalised block. None of that is possible without coordinates: the local theory of Chapter 6 produces canonical labels, the global theory of Chapter 11 produces canonical orbits, and this chapter produces canonical numbers.

**Reading conventions.** The chapter uses the standard mathematical hierarchy of the volume. Definitions introduce objects that are used later; theorems and propositions carry complete proofs under the assumptions explicitly stated in their hypotheses; conjectures are reserved for statements not proved in the present volume; remarks and examples provide interpretation rather than new formal claims. No additional status tags are used in the body of the book. When a result is conditional on an auxiliary hypothesis, the dependency is stated in the result itself or in the paragraph immediately preceding it. When a direction is speculative, it is placed in the closing section on possible extensions rather than marked by an internal editorial label.

**Where the chapter ends.** The chapter does not propose an architecture. It produces operating coordinates. Crowding and design pass to Chapter 13; the rationalised block to Chapter 14; incrementality, growth, and termination to Chapter 17. The end of this chapter is the moment when sharpness, activation, and silence are sufficiently articulated that crowding can be measured.

<a id="section-12-2"></a>
### 12.2 Sharpness of pattern emergence

The relative emergence theorem of Chapter 6 (Theorem 6.61) gives a sufficient condition for the softmax of a logit matrix to be close to a canonical regime: an exterior margin $\Delta\ge\log(n-\kappa)+t$ on every active row and an intra-support spread $\rho\le\bar\rho$. The conclusion is the row-wise bound

<a id="display-0416-row-bound"></a>
$$
\lVert S_i-u_{C_i}\rVert_2\le2e^{-t}+\frac{e^{\bar\rho}-1}{\sqrt\kappa}.
$$

The condition is uniform: every row must independently satisfy both inequalities. What this section provides is the dynamical version of the same statement, in which the logit matrix is parametrised by an inverse temperature $\beta>0$ and the question is at which $\beta$ the resulting attention map is $\varepsilon$-close to the canonical regime. The answer is a Frobenius-level

> Editorial continuation: The final sentence continues on PDF 417; the source has not been completed or paraphrased across that boundary here.
