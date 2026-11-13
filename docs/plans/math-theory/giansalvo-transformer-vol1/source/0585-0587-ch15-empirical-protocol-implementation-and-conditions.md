---
title: "Chapter 15: empirical protocol, implementation checklist, and conditional guarantees"
source_pdf_pages: [585, 587]
book_pages: [549, 551]
batch: "012"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 15: empirical protocol, implementation checklist, and conditional guarantees

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Next: signed residual geometry](0588-0590-ch15-signed-residuals-and-level-one-rule.md)

> Editorial note: Full transcription of PDF 585–587 (printed pages 549–551). Tables are retained at their physical page positions; the sentence in §15.41 and the numbered implementation checklist continue across page boundaries. Table 15.14, associated with §15.43, follows on PDF 588. Statements describing experiments are the author's statements, not experiments reproduced for this library.

<a id="pdf-0585"></a>
## PDF 585 · Book page 549

<a id="table-15-11"></a>
| KPI | Expected curve | Meaning |
|---|---|---|
| $H_\ell(t)$ | Stepwise nondecreasing before termination | Capacity is added only at events. |
| $\rho_\ell(t)$ | Drops after accepted growth | Coverage improves. |
| $C_{\mathrm{orb}}^{(\ell)}(t)$ | Does not drift upward after growth | New heads do not collapse into old ones. |
| $\Gamma_{\mathrm{res}}^{(\ell)}(t)$ | Falls or remains bounded | Residual injection is controlled. |
| $V_{\mathrm{marg}}$ | New heads acquire positive value or are rolled back | Growth is useful. |
| Anchor count | Nondecreasing or replacement-preserved | Directional tails are safe. |
| Termination flag | Activates after tolerance plateau | Growth has stopped for a measurable reason. |

**Table 15.11:** Core empirical protocol for the dynamic INCRT chapter.

<a id="section-15-39"></a>
## 15.39 Core empirical protocol

The minimal empirical illustration can be made precise as a short protocol. A small rationalised model is initialised below the estimated covering number of a synthetic or controlled task. Training is run with the controller active. The reported curves are not only accuracy and loss, but the controller variables:

<a id="display-0585-controller-curves"></a>
$$
H_\ell(t),\quad \rho_\ell(t),\quad C_{\mathrm{orb}}^{(\ell)}(t),\quad
\Gamma_{\mathrm{res}}^{(\ell)}(t),\quad
\max_h V_{\mathrm{marg},h}^{(\ell)}(t),\quad
|\mathcal H_{A,\ell}^{\mathrm{anchor}}(t)|.
$$

A growth event is shown as a vertical line in the diagnostic plots. The expected signature of a successful run is:

1. before growth, $\rho_\ell(t)$ has a stable plateau above tolerance;
2. after growth, $\rho_\ell(t)$ decreases over the adaptation window;
3. $C_{\mathrm{orb}}^{(\ell)}(t)$ does not increase persistently;
4. directional anchors are not removed;
5. after finitely many events, all improvement curves fall below tolerance.

This protocol is intentionally smaller than a full BERT-scale comparison. Its purpose is to validate the controller loop itself. Full-scale comparisons at equal compute, benchmark accuracy, perplexity, and large-model KPIs are left to the next chapter or to companion experiments.

<a id="section-15-40"></a>
## 15.40 What is deferred to INCRT II

The updated quadrittico source contains more material than is appropriate for the core dynamic chapter. The following topics are preserved for the next chapter, where the original application-driven INCRT formulation is developed in full.

<a id="section-15-41"></a>
## 15.41 Relationship with the original INCRT formulation

The original INCRT formulation remains important. It was the application-driven form of the method: a model begins small, monitors residual directional energy, adds capacity when

<a id="pdf-0586"></a>
## PDF 586 · Book page 550

<a id="table-15-12"></a>
| Deferred topic | Reason for deferral |
|---|---|
| Compressed-sensing bound | Important for extended theory, but not needed to define the core controller. |
| Three-level framework and depth self-determination | Generalises growth beyond heads inside fixed layers. |
| Persistent task drift and homeostasis | Replaces finite termination with continual equilibrium. |
| Input-conditional expert count | Extends INCRT toward routing and mixture-of-experts regimes. |
| Architectural transfer | Studies reuse across related tasks rather than core growth. |
| Adaptive optimisers and preconditioned gates | Refines training dynamics after the controller has been defined. |
| PAC-style guarantees | Gives statistical guarantees for architecture selection; not needed for the constructive algorithm. |
| Beyond orthogonal deflation | Extends the original spectral view beyond the clean case. |
| Applications and empirical history | Documents the success of the original INCRT formulation. |

**Table 15.12:** Material preserved for INCRT II: Extensions, Guarantees, and Applications.

the present architecture is insufficient, and removes or freezes capacity when it is no longer useful. The present chapter gives this idea a static-to-dynamic foundation. Its main change is not operational but conceptual. Residual directional energy is no longer the only state variable. It is embedded in a larger diagnostic state:

<a id="display-0586-diagnostic-state"></a>
$$
\bigl(\rho_\ell,C_{\mathrm{orb}}^{(\ell)},\Gamma_{\mathrm{res}}^{(\ell)},
V_{\mathrm{marg},h},\alpha_h,\mathcal D_\ell,\mu_\ell\bigr).
$$

This is why the old INCRT should not be discarded. It is the historical and empirical core. The rationalised INCRT developed here explains why the core works and supplies safeguards against over-growth, crowding, and directional starvation.

The following chapter develops the broader theory: compressed-sensing views of growth, homeostatic drift, expert counts, architectural transfer, adaptive optimisers, PAC-style selection guarantees, and applications. Those topics are not removed; they are separated so that the core dynamic architecture can be read without losing its main line.

<a id="section-15-42"></a>
## 15.42 Implementation checklist for a programmer

The core controller can be implemented without using the full extended theory of the next chapter. A practical implementation needs the following objects.

1. A routine that extracts head signatures from validation batches.
2. A dictionary tracker for $\mathcal D_\ell(t)$.
3. A coverage residual estimator for $\rho_\ell(t)$.
4. A residual amplification estimator for $\Gamma_{\mathrm{res}}^{(\ell)}(t)$.
5. A marginal value estimator, preferably exact ablation for small models and a differentiable proxy for large models.
6. A growth selector implementing Table 15.3.
7. A safe event executor with warm-up, validation, and rollback.
8. A preservation layer that prevents directional anchors from being removed.

<a id="pdf-0587"></a>
## PDF 587 · Book page 551

<a id="table-15-13"></a>
| Implementation object | Minimal version | Scalable version |
|---|---|---|
| Signature extractor | Store attention maps on a validation batch | Streaming sketches of signature coordinates. |
| Coverage estimator | Exact nearest atom distance | Approximate nearest-neighbour dictionary. |
| Marginal value | Head ablation | Learned proxy calibrated by occasional ablation. |
| Growth event | Add or split one head | Batched candidate events with rollback. |
| Anchor preservation | Hard no-prune list | Replacement certificates for anchors. |
| Termination | Fixed thresholds for $q$ windows | Adaptive thresholds with validation hysteresis. |

**Table 15.13:** Minimal and scalable implementations of the core INCRT controller.

9. A termination monitor with hysteresis.

The implementation does not require every coefficient to be fixed theoretically. As in Chapter 14, coefficients are biases tuned by validation. What must not be changed is the logical order of the controller: first estimate, then test stability, then act, then validate, and only then commit the architectural change.

This checklist is the practical counterpart of the mathematical core. It is also the reason why the chapter is separated from the broader INCRT II material. Before studying homeostasis, transfer, experts, or PAC guarantees, the reader should see exactly how the basic controller is implemented.

<a id="section-15-43"></a>
## 15.43 Remaining conditional points inside the core chapter

The core chapter closes the essential dynamic loop, but several statements remain conditional in the precise mathematical sense. These are not deferred because they are unimportant; they are stated with their assumptions because a completely unconditional theorem would be false for arbitrary non-stationary training trajectories.

First, diagnostic stability depends on a separation between the optimiser scale and the controller scale. If the controller updates as fast as the model parameters, the moving target can outrun the estimates. Second, coverage decrease depends on implementability: a new head must be initialised close enough to the selected signature region. Third, directional-tail preservation is a theorem only for anchor-preserving controllers. A controller that is allowed to prune anchors freely can obviously destroy them. Fourth, finite termination requires finite useful cover and a lower bound on accepted improvements. In continual or drifting tasks, termination is replaced by homeostatic equilibrium, which belongs to INCRT II.

This is the appropriate level of honesty for the core chapter. The controller is not claimed to solve every possible dynamic training problem. It is shown to be a well-defined incremental architecture under the assumptions that make a growth controller meaningful.

