---
title: "Chapter 14: algebraic subsumption and two rationalised instances"
source_pdf_pages: [516, 517]
book_pages: [480, 481]
batch: "011"
language: en
type: source-transcription
status: complete-for-range
---

# Chapter 14: algebraic subsumption and two rationalised instances

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous pages](0512-0515-ch14-sa-channels-and-cost-accounting.md#pdf-0515)

> Faithful English transcription of physical PDF pages 516–517 / printed pages 480–481. Completes §14.5.5 and includes §§14.6–14.6.3, Propositions 14.10–14.12, and the beginning of §14.6.4. The final sentence continues on PDF 518. The source's subsumption claim is retained literally; its relation to the displayed block in Definition 14.7 is examined separately in Russian notes 177–178.

<a id="pdf-0516"></a>
## PDF 516 · Book 480

The formula is less compact but more informative. Each term corresponds to a measurable requirement.

**Takeaway.** *The Rationalised Transformer should not be advertised primarily as a smaller Transformer. Its main advantage is accountable allocation. Compression is possible when crowding is high; expansion is justified when residual energy and uncovered signatures remain high.*

<a id="section-14-6"></a>
### 14.6 Universality and subsumption claims

A rationalised architecture must be explicit about what is proved, what is an algebraic identity, and what is only a design interpretation. This section fixes that boundary. It is included before the geography-control section because the subsequent prescriptions use the rationalised block as an architectural template, not as an unrestricted universal claim.

The word *subsumption* is used here only in the following precise sense. An architecture $\mathcal B_1$ algebraically subsumes an architecture $\mathcal B_0$ if a parameter choice, a gate choice, or a limit inside $\mathcal B_1$ gives exactly the input–output map of $\mathcal B_0$ at block level. This is different from a universal approximation theorem, which concerns density of a function class in a specified topology. The two notions should not be conflated.

<a id="section-14-6-1"></a>
### 14.6.1 Degenerate recovery of the standard MHA+FFN block

Let a standard pre-normalised Transformer block be written as

<a id="display-0516-standard-prenorm-block"></a>
$$
X^+=X+\operatorname{MHA}(\operatorname{LN}(X)),
\qquad Y=X^++\operatorname{FFN}(\operatorname{LN}(X^+)).
$$

The rationalised block has the same residual skeleton but exposes separate S–A channels, dictionary assignments, gates, and diagnostic regularisers. If the diagnostic penalties are set to zero, if all residual gates are set to one, if the symmetric, antisymmetric, and mixed branches are merged into a single attention branch, and if the feed-forward completion is chosen as the ordinary FFN, the map is exactly the standard block.

<a id="proposition-14-10"></a>
**Proposition 14.10 (Algebraic recovery of MHA+FFN).** *The standard MHA+FFN Transformer block is an algebraic degeneration of the rationalised Transformer template.*

*Proof.* Choose the rationalised block with one unconstrained attention branch, identity residual gates, no crowding penalty, no dictionary assignment penalty, and a feed-forward completion equal to the standard FFN. The S–A split is then not used as a constraint: the matrices are simply recombined as $M_h=M_{s,h}+M_{a,h}$. The resulting residual update is the standard MHA update followed by the standard FFN update. Hence the two input–output maps coincide. $\square$

This proposition is the only statement needed to ensure that rationalisation does not reduce the baseline expressivity by construction. It is an algebraic statement, not a benchmark claim.

<a id="section-14-6-2"></a>
### 14.6.2 Instance I: prototype-gated orthogonal projection

A first concrete instance is obtained when the dictionary representatives are used as prototype directions and the residual update is projected onto a controlled subspace. Let $\mathcal D_\ell=\{a_1,\ldots,a_{K_\ell}\}$ be the layer dictionary and let $P_{a_k}$ denote the projection, or soft projection,

<a id="pdf-0517"></a>
## PDF 517 · Book 481

associated with atom $a_k$. A prototype-gated orthogonal-projection instance has attention residual

<a id="display-0517-prototype-projection-update"></a>
$$
R_{\mathrm{att}}(X)=\sum_{k=1}^{K_\ell}g_k(X)\,P_{a_k}\,U_k(X),
$$

where $g_k(X)$ is a bounded gate and $U_k$ is a local update assigned to atom $a_k$. When the projections are chosen from the S–A split, the same expression decomposes into symmetric, directional, and mixed components.

<a id="proposition-14-11"></a>
**Proposition 14.11 (Prototype-projection instance).** *The prototype-gated projection block is an instance of the rationalised Transformer whenever its atoms are the selected dictionary representatives, its gates satisfy the marginality constraints, and its residual injection obeys the bound used for $\Gamma_{\mathrm{res}}$.*

*Proof.* The dictionary representatives provide coverage, the projections implement controlled assignment to atoms, the gates give marginality-aware weighting, and the residual bound gives the required amplification constraint. These are precisely the components required by the rationalised static class. Therefore the block is an implementation of the rationalised template. $\square$

The point of this instance is not to introduce a separate architecture. It shows that the abstract definition can be realised by a simple projection-and-gating construction.

<a id="section-14-6-3"></a>
### 14.6.3 Instance II: multi-head block with geometric allocation

A second instance keeps the ordinary multi-head parameterisation but changes the allocation rule. Heads are no longer treated as exchangeable copies. Each head receives a layer atom, a channel role, and a residual budget:

<a id="display-0517-geometric-head-assignment"></a>
$$
h\longmapsto\bigl(a(h),c(h),b(h)\bigr),
\qquad c(h)\in\{S,A,M\}.
$$

The attention map remains multi-head, but the head family is constrained by coverage, crowding, marginality, and residual amplification. This gives a multi-head block with geometric allocation.

<a id="proposition-14-12"></a>
**Proposition 14.12 (Geometrically allocated multi-head block).** *A multi-head block whose heads are assigned by the dictionary objective, whose S–A roles are fixed by the allocation rule, whose duplicate assignments are penalised by crowding, and whose residual budgets are gated by $\Gamma_{\mathrm{res}}$, is an algebraic instance of the rationalised Transformer.*

*Proof.* The block has the same residual skeleton as the rationalised Transformer. Its heads realise the dictionary representatives, its channel labels realise the S–A split, its assignment objective supplies crowding control, and its residual budgets implement bounded residual injection. Thus the block satisfies the defining constraints of the rationalised template. $\square$

This statement deliberately stops at the algebraic level. It does not claim that every geometrically allocated multi-head block is optimal, nor that every possible Transformer variant is subsumed.

<a id="section-14-6-4"></a>
### 14.6.4 Universal approximation status

Because Proposition 14.10 embeds the standard MHA+FFN block as a degenerate case, any universal approximation theorem that applies to the standard class under a specified set of

---

**Page boundary.** The sentence continues on [PDF 518](0518-0522-ch14-static-geography-closure.md#pdf-0518), beginning “assumptions also applies”. No words have been supplied to complete it on PDF 517.

[Next pages](0518-0522-ch14-static-geography-closure.md#pdf-0518) · [Russian note 177](../notes/177-prenorm-recovery-and-the-postnorm-template.md) · [Russian note 178](../notes/178-projection-instances-and-conditional-expressivity.md)
