---
title: "Chapter 7: head-gauge symmetry, local signatures, and layer geography"
source_pdf_pages: [245, 250]
book_pages: [209, 214]
batch: "005"
language: en
type: source-transcription
status: complete-for-page-range
continues_after_file: true
---

# Chapter 7: head-gauge symmetry, local signatures, and layer geography

[Library index](../README.md) · [Source and transcription policy](../SOURCE.md) · [Previous pages: PDF 239–244](0239-0244-ch07-information-flow-and-residual-stream.md#pdf-0244)

> Editorial note: Full transcription of PDF 245–250 (printed pages 209–214), including Table 7.2, all numbered statements and equations, unnumbered displays, the complete historical footnote, and the final paragraph of §7.7. Statements about channel destruction, the product gauge group, optimisation, and earlier minimum-energy barycentric conventions are retained as printed. See [note 56](../notes/56-residual-stream-and-representation-clustering.md), [note 57](../notes/57-head-gauge-and-channel-identifiability.md), and [note 58](../notes/58-global-geographies-and-layer-observables.md) for separate editorial qualifications. No body text after PDF 250 is included or used.

<a id="pdf-0245"></a>
## PDF 245 · Book 209

<a id="table-7-2"></a>
| Extraction point | # clusters | ARI |
| --- | --- | --- |
| Input embeddings (no CLS), $X^{(0)}$ | 8 | 0.291 |
| Output embeddings (no CLS), $X^{(L)}$ | 14 | 0.885 |
| CLS output, $x_{\mathrm{CLS}}^{(L)}$ | 8 | 0.978 |
| Pre-softmax classifier head | 8 | 0.979 |
| Post-softmax classifier head | 8 | 0.981 |

**Table 7.2:** Build-up of the cluster structure across the extraction points of trained BERT-base on the SARS-CoV-2 eight-variant classification task. ARI computed against ground-truth class labels; ARI = 0 corresponds to chance, ARI = 1 to a perfect match. Data from Ghione (2022).

0.978 at the CLS output of the last encoder layer, with the classifier head producing the final small refinement. Second, the bulk of the cluster build-up happens within the encoder, not at the classifier head: the CLS output already achieves ARI = 0.978, and the subsequent linear projection plus softmax adds only 0.003. The encoder is doing the classification work; the head is reading it off. Third, the “contraction effect of softmax” is empirically visible in the Pareto charts of the principal components: the post-softmax classifier head produces six approximately equally-weighted principal directions for an eight-class problem, indicating that the eight clusters have been arranged onto an approximately six-dimensional submanifold with two clusters absorbed into outliers along that submanifold.

The cluster-side view is consistent with the operator-side chain (7.5): the directional alignment that the cone index measures, when summed across heads of a layer, produces the directional separation that the cluster analysis registers. The two views are dual measurements of the same dynamics.

<a id="section-7-4"></a>
### 7.4 Head-Gauge Symmetry and Channel Non-Identifiability

The propagation map of §7.2 suggests an intuition that needs to be put to the test. Looking at the grid column by column, one is tempted to say that head $h$ at layer $\ell+1$ inherits, refines, or revises the work done by head $h$ at layer $\ell$. The columns look like channels: persistent sequences of computation across the depth of the network, each carrying its own line of reasoning. The intuition is wrong, and proving that it is wrong is the purpose of this section. Two structural results — the Channel Destruction Theorem of Cap. 8 and the Residual Amplification Proposition of Cap. 11 — combine to show that the columns of the grid carry no persistent identity. What the propagation map suggests as a channel is a projection artefact: a way of arranging the heads on the page that respects the concatenation order chosen by the implementation, but that has no algebraic counterpart in the architecture. The reading is reinforced by a third observation, which the section closes with: the absence of channel structure is exactly what one would predict from a gauge-theoretic analysis of the architecture, where the freedom to permute and re-coordinatise the heads is a symmetry rather than a defect.

**The MLP as an information mixer.** The first result is structural and concerns the multi-layer perceptron that follows each multi-head attention block. Write $Y^{(h)}\in\mathbb R^{n\times d_v}$ for the output of head $h$ before the output projection and $Y_{\mathrm{MH}}=\operatorname{Conc}_{h=1}^{H}(Y^{(h)})W_O$ for the concatenated multi-head output projected by $W_O$. The MLP receives $Y_{\mathrm{MH}}$ (after the residual addition and layer normalisation) and produces $Y_{\mathrm{MLP}}$ as $f(Y_{\mathrm{MH}}W_1)W_2+\mathrm{bias}$, with

<a id="pdf-0246"></a>
## PDF 246 · Book 210

$f$ the GeLU nonlinearity. The block decomposition of $W_1$ along the concatenation gives the form that drives the rest of the section.

<a id="theorem-7-5"></a>
**Theorem 7.5 (Channel Destruction by the MLP; ?).** Let $W_1$ be partitioned into $H$ horizontal blocks $W_1^{(h)}\in\mathbb R^{d_v\times d_{\mathrm{ff}}}$ aligned with the heads. Then:

**(i) Order destruction.** For every permutation $\pi\in\mathfrak S_H$,

<a id="equation-7-7"></a>
$$
\operatorname{MLP}\!\left(
\operatorname{Conc}_{h=1}^{H}\!\left(Y^{(\pi(h))}\right)W_O^\pi
\right)
=\operatorname{MLP}\!\left(
\operatorname{Conc}_{h=1}^{H}\!\left(Y^{(h)}\right)W_O
\right),
\tag{7.7}
$$

with $W_O^\pi$ the corresponding row-permutation of $W_O$.

**(ii) Mean contraction.** The MLP output depends on the head outputs only through their weighted sum:

<a id="equation-7-8"></a>
$$
Y_{\mathrm{MLP}}
=f\!\left(\sum_{h=1}^{H}Y^{(h)}\widetilde W_1^{(h)}\right)W_2+\mathrm{bias},
\tag{7.8}
$$

with $\widetilde W_1^{(h)}$ the composition of $W_O$ on the $h$-th block with the corresponding rows of $W_1$.

**(iii) Rank contraction.** The output rank is bounded by

<a id="equation-7-9"></a>
$$
\operatorname{rank}Y_{\mathrm{MLP}}
\le\min(n,Hd_k)=\min(n,d_{\mathrm{model}}),
\tag{7.9}
$$

which equals $d_{\mathrm{model}}$ for $n\ge d_{\mathrm{model}}$ and is the regime of BERT-base on the SARS-CoV-2 sequences of Cap. 3.

**Preview.** *The proof, given in full in Cap. 8, decomposes the matrix product $\operatorname{Conc}(Y^{(h)})W_1$ along the block structure of $W_1$ to obtain (7.8); (7.7) follows from the symmetry of the sum, and (7.9) from rank subadditivity together with the rank-non-increasing action of GeLU on the row space. The name Channel Non-Identifiability Theorem is also in use when $Hd_k=d_{\mathrm{model}}$, which is the canonical BERT regime, and is the form referenced in this chapter.*

The three parts of Theorem 7.5 have a single combined consequence: from the point of view of any layer downstream of the MLP, the output at position $i$ is a function of the *set* of head contributions $\{Y^{(h)}\}_{h=1}^{H}$, not of the *tuple*. The next attention layer cannot recover which head produced which contribution, because the information needed to do so has been linearly mixed and then non-linearly compressed into a representation of rank at most $d_{\mathrm{model}}$. Whatever directional structure the next layer needs, it must re-extract from this mixed representation; nothing is preserved by position. The columns of the propagation map of Figure ?? record, for each layer, which head produced which pattern in that layer. They do not record an inheritance: there is none.

**The interference of the residual stream.** The Channel Destruction Theorem shows that the inputs to layer $\ell+1$ contain no head-identity information. A second result, proved in Cap. 11 as part of the analysis of the training dynamics, controls the cumulative effect of the layer increments on the directional energy.

<a id="proposition-7-6"></a>
**Proposition 7.6 (Residual Amplification of Directional Energy; ?).** Let $\Gamma_0^{(\ell)}=\|X^{(0)}M_a^{(\ell)}(X^{(0)})^\top\|_F$ be the directional energy that the layer-$\ell$ antisymmetric operator $M_a^{(\ell)}$ would extract from the initial embedding alone, and let $\Delta^{(\le\ell)}=\sum_{\ell'=1}^{\ell}\Delta^{(\ell')}$ be the cumulative residual increment up to layer $\ell$. Then

<a id="equation-7-10"></a>
$$
\Gamma^{(\ell)}
\le \Gamma_0^{(\ell)}
+2\|X^{(0)}\|_F\|\Delta^{(\le\ell)}\|_F
  \sigma_1\!\left(M_a^{(\ell)}\right)
+\left\|\Delta^{(\le\ell)}M_a^{(\ell)}
  (\Delta^{(\le\ell)})^\top\right\|_F.
\tag{7.10}
$$

<a id="pdf-0247"></a>
## PDF 247 · Book 211

The middle term is the *cross-interference* between the initial embedding and the cumulative increments: when those increments align with the leading eigenplane of $M_a^{(\ell)}$, the interference is constructive and amplifies $\Gamma^{(\ell)}$ beyond the sum of the autonomous contributions of $X^{(0)}$ and of the increments alone.

**Preview.** *The proof, given in Cap. 11, applies the triangle inequality to the bilinear form $(X^{(0)}+\Delta^{(\le\ell)})M_a^{(\ell)}(X^{(0)}+\Delta^{(\le\ell)})^\top$ and bounds the off-diagonal terms by the operator-Frobenius pairing $\|AB\|_F\le\|A\|_F\|B\|_{\mathrm{op}}$.*

The proposition refines the picture of the residual stream of §7.3 in a structural direction. The cumulative increment $\Delta^{(\le\ell)}$ is the sum of all the layer contributions up to layer $\ell$, each of which is itself a sum over heads after the MLP mixing of Theorem 7.5. Three regimes follow. When the increments are uncorrelated with the initial embedding — the regime of the lower layers, where the directional search has not yet converged — the cross term in (7.10) is small, and $\Gamma^{(\ell)}$ is approximately the sum of the two autonomous contributions. When the increments align with the leading eigenplane of $M_a^{(\ell)}$, the cross term is positive and large, and $\Gamma^{(\ell)}$ grows faster than either contribution alone. This is the regime of the middle layers and the structural cause of the steepening cone-index curve of Figure 7.6. When the increments are nearly orthogonal to the eigenplane — the regime of the upper layers in the C3 condition, where most heads are already silenced — the cross term collapses and $\Gamma^{(\ell)}$ plateaus. The three regimes are exactly the three regimes of the residual stream in §7.3, observed at the level of the directional energy rather than the cone index, and unified by the interference geometry of (7.10).

**Gauge structure of the channel.** The two preceding paragraphs read together yield a sharper statement than either alone. The MLP destroys the channel identity; the residual stream propagates a sum of contributions in which the head index is no longer recoverable; the directional energy of the next layer depends only on the total increment, not on its decomposition into per-head pieces. The architecture treats the head index as a redundant coordinate, and its action on the observable quantities is invariant under any re-labelling of that coordinate. This is precisely the structural property that, in physics, defines a gauge symmetry.

The relevant gauge group is the product

<a id="equation-7-11"></a>
$$
\mathcal G_{\mathrm{channel}}
=GL(d_k)\times\mathfrak S_H,
\tag{7.11}
$$

where $GL(d_k)$ acts on each head by re-coordinatising the key/query subspace ($W_Q^{(h)}\mapsto W_Q^{(h)}G$ and $W_K^{(h)}\mapsto W_K^{(h)}G^{-\top}$ leave $M^{(h)}=W_Q^{(h)}(W_K^{(h)})^\top$ invariant), and $\mathfrak S_H$ permutes the heads.[1](#footnote-7-1) The action of $GL(d_k)$ on the canonical pair $(M,N)$ introduced in Cap. 2 is by conjugation on the inner factors, which leaves the score matrix and the value-output product invariant; the action of $\mathfrak S_H$ permutes the $\widetilde W_1^{(h)}$ blocks of (7.8), which leaves the MLP output invariant by part (i) of Theorem 7.5.

<a id="footnote-7-1"></a>
**Footnote 1.** The terminology *gauge*, in its modern form, goes back to Weyl's 1918 attempt to extend Riemannian geometry by allowing the local scale (*Maßstab*) of vectors to vary position by position; the German word means precisely *measure* or *rod*. Yang and Mills (1954) reused the term for non-Abelian local symmetries of quantum-field gauge potentials, and the modern usage in physics descends from there. The structural content is invariant across the half-century: a gauge group is a group of transformations of the parametrisation that leaves the physical observables unchanged. In the Transformer, the parameters $(W_Q^{(h)},W_K^{(h)},W_O)$ admit such transformations, and the group $\mathcal G_{\mathrm{channel}}$ is the corresponding gauge.

<a id="pdf-0248"></a>
## PDF 248 · Book 212

<a id="proposition-7-7"></a>
**Proposition 7.7 (Channel as a gauge orbit).** The map from the parameter vector $\theta=(W_Q^{(h)},W_K^{(h)},W_O,W_1,W_2)_{h=1}^{H}$ to the output of the encoder block is constant on the orbits of the $\mathcal G_{\mathrm{channel}}$-action defined above. Two parameter vectors $\theta,\theta'$ that lie in the same orbit implement the same encoder block, and no measurement performed at the layer-$\ell+1$ inputs can distinguish them.

*Proof.* $GL(d_k)$-invariance of $M^{(h)}$ is the standard parametrisation freedom of the canonical pair (Cap. 2, Symmetry-Breaking Decomposition). $\mathfrak S_H$-invariance of the MLP output is part (i) of Theorem 7.5. The two actions commute, since $GL(d_k)$ acts within each head and $\mathfrak S_H$ acts on the head index, so the combined action is that of the product group. Constancy on orbits is the conjunction of the two invariances. $\square$

The gauge interpretation closes the argument that opened the section. The propagation map of Figure ?? displays the heads in a particular order, fixed by the implementation, but the architecture is invariant under arbitrary re-orderings: any column-by-column reading of the map is a gauge-fixing, not an algebraic structure of the network. The channel does not exist as an architectural object. What does exist, and is the topic of §??, is the distribution of patterns within each layer and the way that distribution evolves with depth. A channel-free description must be adopted to make the right invariants visible.

**Caveat.** *The gauge group $\mathcal G_{\mathrm{channel}}$ is the symmetry of the trained encoder block, not of the training procedure. Standard backpropagation breaks $\mathfrak S_H$-symmetry through random initialisation: at $t=0$, the head outputs $Y^{(h)}$ are statistically distinguishable because the initial weights are independent draws, and the trajectories from different initialisations populate different gauge orbits. The architecture is invariant; the optimiser is not. This is the same distinction as in physics, where the action of a field theory is gauge-invariant but a particular solution of the equations of motion is gauge-fixed by initial or boundary conditions.*

**Takeaway.** *The MLP destroys the channel identity by mixing the head outputs through a sum that is invariant under permutation of the heads; the residual stream propagates a cumulative increment in which the head index is no longer recoverable; the directional energy of any downstream layer depends on the total increment, not on its per-head decomposition. The combined symmetry $\mathcal G_{\mathrm{channel}}=GL(d_k)\times\mathfrak S_H$ is a gauge group of the architecture in the classical Weyl–Yang–Mills sense: a group of parameter transformations that leaves the physical observables unchanged. The columns of the propagation map are a gauge-fixing of this symmetry, not an algebraic structure of the network.*

**Transition to the signature formalism.** The preceding four sections retain the empirical and structural core of the global analysis: the layer–head grid, the BERT geography, the flow of observables along depth, and the non-identifiability of head channels. The remaining sections turn these observations into a formal state space. Hard pattern labels are replaced by canonical signatures, layer tables are replaced by empirical distributions of signatures, and head-to-head transitions are replaced by transport of signature geographies.

<a id="section-7-5"></a>
### 7.5 From local signatures to global geographies

The local construction used in this chapter is the one established in Chapter 10. For a sequence length $n$, the eight canonical attention regimes determine a finite dictionary $\mathcal D_8(n)$

<a id="pdf-0249"></a>
## PDF 249 · Book 213

and its canonical hull

<a id="display-7-canonical-hull"></a>
$$
K_8(n)=\operatorname{conv}\mathcal D_8(n)\subset\mathcal S_n.
$$

For every attention map $S\in\mathcal S_n$, Chapter 10 defines its canonical projection, residual, and regime-level signature. Only the notation is recalled here, since the proofs and the local classification belong to the previous chapter.

<a id="definition-7-8"></a>
**Definition 7.8 (Local canonical projection and residual).** The canonical projection of $S$ is

<a id="display-7-canonical-projection"></a>
$$
\Pi_8(S)=\operatorname*{arg\,min}_{Q\in K_8(n)}\|S-Q\|_F,
$$

and the residual is

<a id="display-7-canonical-residual"></a>
$$
R_8(S)=S-\Pi_8(S),\qquad
\rho_8(S)=\|R_8(S)\|_F.
$$

The projection exists and is unique, and the residual is stable:

<a id="display-7-residual-lipschitz"></a>
$$
|\rho_8(S)-\rho_8(T)|\le\|S-T\|_F.
$$

These are local facts about one attention map. They are not reproved in this chapter.

<a id="proposition-7-9"></a>
**Proposition 7.9 (Local projection facts, recalled).** For every $S\in\mathcal S_n$, the point $\Pi_8(S)$ is uniquely defined, $S=\Pi_8(S)+R_8(S)$, and the residual map $S\mapsto\rho_8(S)$ is 1-Lipschitz in Frobenius norm.

The projection may admit more than one convex representation by atoms of $\mathcal D_8(n)$. Chapter 10 fixes this by choosing the minimum-energy barycentric representation and then aggregating atom weights over the eight regime families. The resulting vector is denoted

<a id="display-7-regime-weights"></a>
$$
\Lambda(S)\in\Delta_8.
$$

<a id="definition-7-10"></a>
**Definition 7.10 (Local canonical signature).** The local canonical signature of an attention map $S$ is

<a id="display-7-local-signature"></a>
$$
\Sigma_8(S)=\bigl(\Lambda(S),\rho_8(S)\bigr).
$$

The vector $\Lambda(S)$ records the canonical regime composition of the projection, while $\rho_8(S)$ records the non-canonical residual.

The row-stochastic polytope is also stratified by the faces of the canonical hull: the active face of $S$ is the unique face whose relative interior contains $\Pi_8(S)$. This is the correct replacement for a hard pattern label. A head is not said to be exactly one of the eight regimes; it has a canonical projection, a regime-level signature, an active face, and a residual.

<a id="proposition-7-11"></a>
**Proposition 7.11 (Canonical strata, recalled).** Each attention map belongs to a unique canonical stratum determined by the active face of $K_8(n)$ containing its projection. Inside a fixed simplicial face the barycentric coordinates vary locally Lipschitz-continuously with the attention map; at a face boundary the active canonical explanation may change.

The purpose of the present chapter begins at the next level. Given a model, an input, a layer, and a head, one obtains

<a id="display-7-head-observable"></a>
$$
S_{\ell,h}(X)\quad\longmapsto\quad
\Sigma_{\ell,h}(X)=\Sigma_8\!\left(S_{\ell,h}(X)\right).
$$

The chapter studies how these signatures are distributed over heads, layers, inputs, checkpoints, and tasks. Thus the local map $S\mapsto\Sigma_8(S)$ is taken as an observable, and the global object is no longer a single attention map but a population of signatures.

<a id="pdf-0250"></a>
## PDF 250 · Book 214

<a id="section-7-6"></a>
### 7.6 Dictionary coverage recalled

One population-level scalar from Chapter 10 will be used repeatedly. If $\nu$ is a distribution of attention maps, the dictionary coverage compares the average squared residual with the total variance of the population. It is an $R^2$-like statistic: high coverage means that the canonical hull explains the population well; low or negative coverage means that the dictionary is no better than, or worse than, a centroid baseline for that population.

Coverage is geometric, not causal. A high-coverage dictionary explains attention maps as matrices; it does not by itself prove that the canonical component is behaviourally sufficient. The canonical replacement test of Section 7.40 supplies that causal probe.

<a id="section-7-7"></a>
### 7.7 Layer geography

For input $X$, layer $\ell$, and head $h$, write

<a id="display-7-head-attention"></a>
$$
S_{\ell,h}(X)\in\mathcal S_n
$$

for the attention map. Its local signature is

<a id="display-7-head-signature"></a>
$$
\Sigma_{\ell,h}(X)=\bigl(\Lambda_{\ell,h}(X),\rho_{\ell,h}(X)\bigr).
$$

The layer is represented by the empirical distribution

<a id="display-7-layer-geography"></a>
$$
\mu_\ell(X)=\frac1H\sum_{h=1}^{H}\delta_{\Sigma_{\ell,h}(X)}.
$$

This distribution is the layer geography.

<a id="definition-7-12"></a>
**Definition 7.12 (Mean layer signature).** The mean layer signature is

<a id="display-7-mean-layer-signature"></a>
$$
\bar\Lambda_\ell(X)=\frac1H\sum_{h=1}^{H}\Lambda_{\ell,h}(X),
\qquad
\bar\rho_\ell(X)=\frac1H\sum_{h=1}^{H}\rho_{\ell,h}(X).
$$

The empirical distribution $\mu_\ell$ contains more information than the mean. Two layers can have the same mean signature but different diversity across heads. The mean is useful for visualisation; the full distribution is the invariant object.

<a id="proposition-7-13"></a>
**Proposition 7.13 (Head-permutation invariance).** The empirical distribution $\mu_\ell(X)$, its moments, its average signature, and any Wasserstein distance between two such empirical distributions are invariant under permutations of the heads inside a layer.

*Proof.* A permutation of the heads merely reorders the atoms in the empirical measure. The measure itself does not change. Any quantity defined only from the measure is therefore invariant. $\square$

This is why the global theory is distributional. The index of a head inside a layer is not a gauge-invariant coordinate. Output projection, residual addition, normalisation, and MLP mixing further weaken the idea that one can follow a fixed channel through depth. The layer geography avoids this problem by treating the heads in a layer as a finite empirical population.

<a id="batch-005-boundary"></a>
> Editorial boundary note: PDF 250 / printed page 214 ends after the complete preceding paragraph. Section 7.7 and Proposition 7.13 are complete. The final source words are “the heads in a layer as a finite empirical population.” The next section is §7.8 on PDF 251 / printed page 215 according to the already processed contents; its body has not been read in this batch. No continuation is supplied here.


> Continuation added in batch 006: the historical boundary above is now followed by [§7.8, PDF 251](0251-0257-ch07-signature-transport-and-closed-locus.md#pdf-0251). Chapter 7 is complete through PDF 284; current coverage and boundary are recorded in [progress.json](../progress.json).
