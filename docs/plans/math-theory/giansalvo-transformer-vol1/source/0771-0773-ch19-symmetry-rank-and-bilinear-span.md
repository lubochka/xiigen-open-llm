---
title: "Chapter 19: implementation bounds, symmetry, rank obstruction and bilinear span"
source_pdf_pages: [771, 773]
book_pages: [735, 737]
batch: "016"
language: en
type: source-transcription
status: complete-for-page-range
---

# Chapter 19: implementation bounds, symmetry, rank obstruction and bilinear span

[Library index](../README.md) · [Source policy](../SOURCE.md)

> Editorial note: Full transcription of PDF 771–773. Printed formulas, claims, algorithm steps, numerical values and source inconsistencies are preserved. Editorial analysis is separate in the Russian reading notes and SOURCE-NOTES.md. Figure images are direct renders of the source PDF.

<a id="pdf-0771"></a>
## PDF 771 · Book page 735

The discrete top-$k$ can produce instability if the scoring matrix changes sharply between consecutive training steps, causing partners to flip and the optimisation landscape to discontinuously change. In practice, the use of a small softmax temperature inside Phase 1 and the standard learning rate warmup of BERT-style training are sufficient to stabilise the routing.

**Asymptotic complexity.** Phase 1 has complexity $O(n^2d)$ (the scoring matrix is $n\times n$ and computed via a $d$-dimensional inner product). Phase 2 is $O(nd^2)$ (independent of $k,H$). Phase 3 is $O(nkd_h)$ per head, hence $O(nkd)$ total over the $H$ heads. The dominant terms are $O(n^2d+nd^2)$, which matches the asymptotic cost of standard MHA. Crucially, MHB-G does not pay the additional $O(nd\cdot d_{\mathrm{ff}})=O(nd^2)$ cost of the MLP block: that cost is entirely absorbed by the bilinear product, which already serves as the channel-mixing component.

**Memory footprint.** The largest intermediate tensor is the scoring matrix $S\in\mathbb R^{B\times n\times n}$, where $B$ is the batch size. For $n=512$ and $B=32$, this is $32\cdot512\cdot512\cdot4=32$ MB per layer, identical to standard MHA. The cached projections $L^h,R^h\in\mathbb R^{B\times n\times H\times d_h}$ take $32\cdot512\cdot768\cdot4=48$ MB each, again comparable to MHA’s $K,V$ caches.

**Numerical considerations.** The Hadamard product $L_i^h\odot R_j^h$ can produce values of large magnitude when $L_i^h$ and $R_j^h$ have correlated signs in many channels. Without normalisation between layers, this can cause the iterated representations to grow exponentially in depth. The LayerNorm of Algorithm 8 (applied at the start of each block) controls this growth and is essential for stable training. Empirically, removing LayerNorm causes divergence within the first few hundred steps.

<a id="section-19-4"></a>
### 19.4 Symmetry audit of MHB-G and A-MHB-G

We now characterise how the proposed layer interacts with the symmetries isolated in the previous chapters: the channel/gauge symmetry of Ch. 8, the depth/rank-collapse mechanism of Ch. 10, the head-permutation symmetry $\operatorname{Sym}_H$, and the layer-normalisation symmetry $G_{\mathrm{LN}}$ of Ch. 1. The audit is deliberately split into two levels. MHB-G is the base bilinear operator; A-MHB-G is the recommended operator, obtained by adding anchor gates.

<a id="section-19-4-1"></a>
### 19.4.1 Notation

We denote by $\operatorname{diag}^{\star}(m)\subset\operatorname{GL}(\mathbb R^m)$ the abelian subgroup of invertible diagonal matrices, isomorphic to $(\mathbb R^*)^m$. The head permutation group is $\operatorname{Sym}_H$. All genericity statements are meant in the Lebesgue-a.e. sense in the appropriate finite-dimensional weight or input space. We keep the row-vector convention of Definition 19.1: token vectors are rows of $X$.

<a id="section-19-4-2"></a>
### 19.4.2 Reduced gauge symmetry

<a id="lemma-19-2"></a>
**Lemma 19.2 (Reduced gauge symmetry).** Let $D_h,E_h\in\operatorname{diag}^{\star}(d_h)$ satisfy $D_hE_h=I_{d_h}$ for $h=1,\ldots,H$. The simultaneous transformation

$$
W_L^h\mapsto W_L^hD_h,\qquad W_R^h\mapsto W_R^hE_h
$$

leaves the bilinear contribution of MHB-G, and hence of A-MHB-G, invariant for every input $X$. Conversely, a non-diagonal change of basis in the hidden head coordinates does not preserve the Hadamard bilinear term for all inputs.

> Editorial transcription note: Blank symbol positions after “channel/gauge symmetry” and “depth/rank-collapse mechanism” in the PDF are not filled with inferred names. The later prose reference to “LayerNorm of Algorithm 8” is preserved although Algorithm 8 contains no LayerNorm step.


<a id="pdf-0772"></a>
## PDF 772 · Book page 736

*Proof.* For row vectors $u,v\in\mathbb R^{d_h}$ and diagonal $D=\operatorname{diag}(d_1,\ldots,d_{d_h})$, $(uD)\odot v=(u\odot v)D$. Hence, if $D_hE_h=I$,

$$
(x_iW_L^hD_h)\odot(x_jW_R^hE_h)
=(x_iW_L^h\odot x_jW_R^h)(D_hE_h)
=x_iW_L^h\odot x_jW_R^h.
$$

For the converse, suppose that a non-diagonal matrix $A$ preserved $(uA)\odot(vA^{-1})$ for all $u,v$. Looking at one coordinate $c$ and choosing $u,v$ with independent coordinates forces all off-diagonal entries contributing to coordinate $c$ to vanish. Repeating for every coordinate forces $A$ to be diagonal. Thus the admissible gauge is precisely the diagonal subgroup. $\square$

<a id="remark-19-3"></a>
**Remark 19.3.** Standard MHA admits a full $\operatorname{GL}(d_h)^H$ value/output gauge. MHB-G reduces this to $\operatorname{diag}^{\star}(d_h)^H$, shrinking the non-identifiability from $Hd_h^2$ to $Hd_h$ degrees of freedom. For BERT-base this is a reduction from 49152 to 768 degrees of freedom per layer.

<a id="section-19-4-3"></a>
### 19.4.3 Head permutation and its resolution by anchors

<a id="lemma-19-4"></a>
**Lemma 19.4 (Residual head permutation of unanchored MHB-G).** For every $\pi\in\operatorname{Sym}_H$, the simultaneous transformation

$$
W_L^h\mapsto W_L^{\pi(h)},\qquad W_R^h\mapsto W_R^{\pi(h)}
$$

leaves unanchored MHB-G invariant.

*Proof.* The inner head aggregation is a sum, and addition is commutative. The routing weights $\alpha_{ij}$ depend on $W_s$ and the input, not on the indexing of $W_L^h,W_R^h$. $\square$

<a id="lemma-19-5"></a>
**Lemma 19.5 (Anchor gates break head permutation generically).** Assume that the anchor vectors $a_1,\ldots,a_H\in\mathbb R^d$ are pairwise distinct and that the data distribution is not supported on the finite union of hyperplanes $\{x:x(a_h-a_q)=0\}$. Then A-MHB-G is not invariant under a non-trivial permutation of the bilinear heads unless the anchors are permuted simultaneously. Therefore the apparent head symmetry becomes only a relabelling of dictionary atoms, not an operational non-identifiability.

*Proof.* If $a_h\ne a_q$, then the gate ratio satisfies

$$
\frac{g_h(x)}{g_q(x)}=\exp\bigl(x(a_h-a_q)\bigr).
$$

By the support assumption this ratio is not identically equal to one. A non-trivial permutation of the bilinear weights without the corresponding permutation of anchors therefore changes at least one gate-weighted head contribution on a set of positive measure. If anchors and heads are permuted together, the operator is merely relabelled, exactly as in any dictionary model. $\square$

<a id="section-19-4-4"></a>
### 19.4.4 Generic obstruction to rank collapse

<a id="lemma-19-6"></a>
**Lemma 19.6 (Generic one-step rank obstruction).** Fix generic weights $W_s,W_O$ and generic bilinear projections $\{W_L^h,W_R^h\}$ satisfying Lemma 19.8. On every open routing cell on which the top-$k$ partner sets are fixed and LayerNorm is non-degenerate, the map

$$
F(X)_i=\operatorname{MHBG}(x_i,\operatorname{LN}(X))
$$

is real analytic. For Lebesgue-a.e. input $X$ with $\operatorname{rank}(X)=r<\min(n,d)$, $F(X)$ does not remain trapped in the rank-$\le r$ variety. Equivalently, rank collapse is generically obstructed at one layer. The same conclusion holds for A-MHB-G whenever the anchor gates are non-zero.


<a id="pdf-0773"></a>
## PDF 773 · Book page 737

*Proof.* On a fixed routing cell, $F$ is a composition of LayerNorm on its non-degenerate domain, polynomial bilinear maps, softmax weights restricted to the selected partners, a linear output projection, and an analytic activation such as GeLU. Hence all $(r+1)\times(r+1)$ minors of $F(X)$ are real-analytic functions of $X$. The bad set on which all such minors vanish is either the whole routing cell or a proper analytic subset. Lemma 19.8 gives a witness input and generic weights for which the bilinear span escapes any prescribed $r$-dimensional row subspace after projection by $W_O$; hence the bad set is proper. Proper real-analytic zero sets have Lebesgue measure zero. The anchor gates of A-MHB-G multiply head contributions by non-zero analytic factors and therefore do not restore the rank-collapse variety generically. $\square$

<a id="remark-19-7"></a>
**Remark 19.7.** The lemma is intentionally formulated as a one-step generic obstruction. Deep stacks require control of singular-value conditioning under composition. The empirical results of Section 19.9 show exactly this distinction: MHB-G without residual delays rank collapse substantially, while the recommended residual A-MHB-G combines bilinear rank obstruction with the optimisation stability of the residual stream.

<a id="section-19-4-5"></a>
### 19.4.5 Generic full bilinear span

<a id="lemma-19-8"></a>
**Lemma 19.8 (Generic full bilinear span).** Let $W_L,W_R\in\mathbb R^{d\times d_h}$ and define

$$
\mathcal V_{L,R}=\operatorname{span}\{(uW_L)\odot(vW_R):u,v\in\mathbb R^d\}\subseteq\mathbb R^{d_h}.
$$

If $d\ge d_h$, then the set of pairs $(W_L,W_R)$ for which $\mathcal V_{L,R}=\mathbb R^{d_h}$ is open and dense, and its complement has Lebesgue measure zero.

*Proof.* The condition $\mathcal V_{L,R}\ne\mathbb R^{d_h}$ is equivalent to the existence of a non-zero vector $\xi\in\mathbb R^{d_h}$ such that

$$
\sum_{c=1}^{d_h}\xi_c(uW_L)_c(vW_R)_c=0
\qquad\text{for all }u,v\in\mathbb R^d.
$$

Since this is a bilinear identity in $u$ and $v$, all coefficients of $u_iv_j$ vanish:

$$
\sum_{c=1}^{d_h}\xi_c\ell_{ic}r_{jc}=0,\qquad i,j=1,\ldots,d.
$$

These are polynomial equations in $(\xi,W_L,W_R)$. Their projection onto weight space is semi-algebraic. It is proper because, for $d\ge d_h$, the witness $W_L=W_R=\begin{pmatrix}I_{d_h}\\0\end{pmatrix}$ yields $(e_iW_L)\odot(e_iW_R)=e_i$ for every $i\le d_h$. Therefore the bad set is a proper semi-algebraic subset of weight space and has Lebesgue measure zero. $\square$

<a id="section-19-4-6"></a>
### 19.4.6 Audit table

The corrected audit is summarised in Figure 19.6. In words: MHB-G reduces the full gauge symmetry, A-MHB-G resolves head permutation through anchors, bilinear token-to-token synthesis generically obstructs the rank-collapse mechanism, and LayerNorm preserves the desired $G_{\mathrm{LN}}$ invariance. Thus the main apparent open problems of the operator are not left open; they are absorbed into the final architecture.


> Editorial navigation: [Previous source](0767-0770-ch19-bilinear-operator-anchored-gates-and-routing.md#pdf-0770) · [Next source](0774-0776-ch19-cost-tables-numerical-audit-and-ddcl.md#pdf-0774)
