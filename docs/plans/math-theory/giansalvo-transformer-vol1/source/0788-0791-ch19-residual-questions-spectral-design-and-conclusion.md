---
title: "Chapter 19: residual questions, spectral architecture design and conclusion"
source_pdf_pages: [788, 791]
book_pages: [752, 755]
batch: "016"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: false
---

# Chapter 19: residual questions, spectral architecture design and conclusion

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous source](0785-0787-ch19-empirical-illustration-and-rank-results.md#pdf-0787)

> Editorial note: Full transcription of PDF 788–791, completing Chapter 19. The source's architectural assertions, chapter/part references, conditional controller assumptions and final proof-of-concept qualification are retained. “Deterministic” is the author's claim for a fixed data/backbone input; this conversion does not implement or validate the proposed spectral pre-pass.

<a id="pdf-0788"></a>
## PDF 788 · Book page 752

**Head symmetry.** Unanchored MHB-G preserves $\mathrm{Sym}_H$. A-MHB-G resolves this by attaching each head to a learned anchor $a_h$. Head exchange is then no longer an invisible symmetry of the operator: it is merely a relabelling of dictionary atoms when anchors and heads are permuted together. This is precisely the type of symmetry breaking required by the dictionary-geometric and INCRT components of the volume.

**Rank collapse.** The bilinear token-to-token term generically moves representations outside the invariant low-rank varieties that trap pure attention. Residual connections are therefore no longer the only structural answer to rank collapse. The recommended architecture still retains the residual stream for optimisation robustness, but not because the bilinear operator lacks a rank-preserving mechanism. The finer question that remains is singular-value conditioning across very deep stacks, not the existence of a rank-generating primitive.

**Incremental growth.** A-MHB-G is naturally compatible with INCRT. A new head is added by introducing a new anchor, a left projection, and a right projection. The new anchor may be initialised from a residual direction, an underrepresented cluster of tokens, or a persistent high-loss region of the routing graph. The old heads need not be redefined. Growth, pruning, freezing, and monitoring are attached to explicit head-level quantities: load, routing entropy, effective rank, and validation sensitivity.

**Interpretability.** MHB-G and A-MHB-G do not produce ordinary attention maps. Their interpretability object is a sparse bilinear interaction graph: vertices are tokens, edges are selected partners, edge weights are $\alpha_{ij}$, and edge features are the bilinear channels $(x_iW_L^h)\odot(x_jW_R^h)$, modulated by anchor gates. This is richer than an attention matrix because it separates partner selection from feature synthesis and head allocation.

**Relation to MoE.** MoE routes tokens to fixed parameterised experts. A-MHB-G routes tokens to other tokens, while anchors organise heads as dictionary-indexed bilinear experts. The sequence itself supplies a dynamic expert pool. In this sense, A-MHB-G may be read as a self-generated MoE layer without a separate bank of FFN experts.

**Causal and long-context variants.** The autoregressive version is obtained by replacing $T_k(i)$ with the causal set $T_k^{-}(i)\subseteq\{1,\ldots,i\}$. The long-context version replaces dense partner search by dictionary-routed partner search: tokens first select a small set of dictionary cells and then search for partners only inside those cells. The quadratic cost is therefore a property of the dense implementation, not of the bilinear primitive itself.

**Expressivity.** Standard attention selects and averages value vectors. A-MHB-G selects partner tokens and synthesises pairwise feature interactions. The primitive is therefore not merely a cheaper attention approximation; it changes the computation from value aggregation to bilinear token-pair synthesis. This explains why the operator can act simultaneously as sequence mixer and channel mixer.

**What remains.** The remaining question is empirical scaling. The chapter establishes an architecture, its symmetry audit, its computational advantage, its INCRT-compatible growth mechanism, its interpretability object, and its causal and long-context variants. What remains is to determine, at large scale, the compute-optimal regimes in which A-MHB-G is preferable to standard MHA+MLP, SwiGLU, or expert-based alternatives. This is not a

<a id="pdf-0789"></a>
## PDF 789 · Book page 753

conceptual incompleteness of the chapter; it is the experimental programme opened by the end of Volume I.

<a id="section-19-11"></a>
### 19.11 From incremental growth to spectral architecture design

The empirical and architectural picture assembled by the preceding sections of this chapter places A-MHB-G in direct contact with the INCRT controller of the earlier chapters. This section, the last before the closing remarks, records the conceptual synthesis that the contact produces. The INCRT controller, viewed across the entire volume rather than at any single chapter, has three operational specialisations of one dynamic system. A-MHB-G is the natural carrier of all three, because its anchored bilinear structure exposes exactly the head-level objects that each specialisation requires.

**Three operational regimes of one controller.** The first regime is **expansion**: the INCRT controller is run during training on a randomly initialised A-MHB-G backbone. Growth events introduce new anchors $a_{H(t)+1}$, new left and right bilinear projections $(W_L^{H(t)+1},W_R^{H(t)+1})$, and new head gates $g_{H(t)+1}(\cdot)$, as in the controller of Section 19.6. The residual diagnostics fall, growth terminates when the four tolerances of the INCRT core chapter are simultaneously satisfied, and the architecture $(L^\star,\{K_\ell^\star\},\{a_h^\star\})$ that the controller has produced is the operational endpoint of training. The second regime is **stabilisation**: the same INCRT controller is run on a warm-started A-MHB-G backbone, in a setting where the head-level signed residual operators $A_h^{\mathrm{sig}}$ already have very few dominant signed modes. No growth event is triggered; the controller operates through protection and pruning only. When the backbone is over-initialised, the dynamics may converge from above by selecting the surviving heads under the signed-spectral protection and pruning rule of the signed-spectral chapter; otherwise the controller simply stabilises the initially active signed modes at their current count. The level-one rule of the INCRT core chapter, and the level-two rule of the signed-spectral extension chapter, are the diagnostics that operate in this regime, with A-MHB-G's anchor-permutation symmetry already broken by the dictionary so that heads have stable identity throughout. The third regime is **architecture-initialisation**: the controller is applied before the model is trained, as a spectral pre-pass on the head-level signed operators of a candidate A-MHB-G backbone. The leading positive and negative eigenvector pairs $(u_h^+,u_h^-)$ of each $A_h^{\mathrm{sig}}$ provide the spectral directions from which anchor seeds $\{a_h^\star\}$ may be initialised, and the layer-level signed compressed-sensing bound of the signed-spectral chapter fixes the per-layer head count $K_\ell^\star$. Training then proceeds with the architecture fixed.

**Unification.** These three regimes are not three controllers, but a single dynamic system observed under three different conditions. Their differences lie in two parameters that the controller does not change. The first parameter is the **initialisation** of the backbone, which varies between random, warm-started, and untrained. The second is the **time-scale** on which the controller is run, which varies between during-training (regimes I and II) and before-training (regime III). The controller itself, its diagnostic state, its event templates, its protection and pruning rules, and the four core hypotheses on which its convergence properties rest, are the same across the three regimes. The signed compressed-sensing target $K^\star$ of the signed-spectral chapter provides a common diagnostic reference across the three regimes, rather than a guaranteed dynamic attractor: it is approached from above by pruning in the over-initialised stabilisation regime, computed analytically before training in the architecture-initialisation regime, and grown towards from below by accepted growth

<a id="pdf-0790"></a>
## PDF 790 · Book page 754

events in the expansion regime, but is not in general a fixed point that the controller reaches autonomously from arbitrary initial conditions.

**Analytic neural architecture search.** In regime III, INCRT realises a deterministic neural-architecture-search procedure. Given a backbone family, an input distribution, and a residual budget $\theta_w$, the procedure outputs an architecture without hyperparameter search, without random restarts, and without an outer optimisation loop. The output is reproducible across runs that share the same data and the same backbone initialisation, since the spectrum of the head-level signed operators $\{A_h^{\mathrm{sig}}\}_h$ depends only on the input covariance $\Sigma_X$ and on the candidate query–key projections. The anchored bilinear structure of A-MHB-G is what makes this output usable at training time: the eigenvector pairs $(u_h^+,u_h^-)$ supply the spectral directions from which anchor seeds are initialised, the per-layer counts $K_\ell^\star$ become structural head allocations, and the four-channel gate of the signed-spectral chapter is initialised on the same pairs that the pre-pass has produced. The analytic and the operational sides of architecture selection meet at the anchors.

**A unifying lens.** The synthesis is not a new architecture and not a new controller. It is a unifying lens through which the architectural choices of A-MHB-G acquire their operational meaning, and through which the apparently disparate uses of INCRT across the volume become specialisations of one principle. In simple or still-forming backbones, INCRT appears as explicit growth; in warm-started backbones, as protection and pruning; in candidate untrained backbones, as the spectral pre-pass that produces the architecture deterministically. A single dynamic system, seen at three time-scales, organises the architectural endpoint of Volume I.

<a id="section-19-12"></a>
### 19.12 Concluding Remarks

The programme of Volume I unfolds in four stages. Part I–II provides a geometric characterisation of the forward pass of the standard Transformer. Part III characterises the dynamics of the backward pass via the motor formalism. Part IV unifies the geometric and dynamic perspectives through pattern emergence, subspace activation, and learned symmetry breaking. Part V shifts to architectural rationalisation: design principles (Ch. 15), the rationalised Transformer (Ch. 16), incremental growth (the INCRT development of Chs. 15–17), and experimental validation (Ch. 18).

The book closes here, with Chapter 19, by proposing the **Multi-Head Bilinear Gated layer** as the architecture that the entire monograph recommends. The recommendation is not merely a research proposal but the natural endpoint of the theoretical programme: every design choice in A-MHB-G can be traced to a specific lesson of the preceding chapters. The bilinear cross-token product reduces the gauge non-identifiability identified in Ch. 8. The content-dependent top-$k$ routing breaks the depth symmetry of Ch. 10 without a residual stream. The anchor gates break the residual head-permutation symmetry by tying each head to a learned dictionary atom. The compatibility with incremental head growth is made operational by a concrete INCRT controller: heads can be born, warmed up, frozen, split, merged, or pruned according to measurable geometric signals. The parameter and FLOPs reductions instantiate the design corollary of Ch. 16.

We do not claim that A-MHB-G is the only architecture compatible with the lessons of this monograph; we claim that it is one such architecture, made explicit, and we hand it to the reader as a working starting point. The empirical illustration of Section 19.9 should be read

<a id="pdf-0791"></a>
## PDF 791 · Book page 755

as a proof of concept, not a definitive validation: such validation belongs to a future research programme building on the foundations laid in this volume.



> Editorial navigation: [Previous source](0785-0787-ch19-empirical-illustration-and-rank-results.md#pdf-0787) · [Next source](0792-0796-appendix-a-mathematical-background.md#pdf-0792)
