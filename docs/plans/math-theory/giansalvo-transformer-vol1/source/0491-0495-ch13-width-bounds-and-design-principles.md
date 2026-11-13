---
title: "Chapter 13: specialisation-aware width, self-sufficiency, and six design principles"
source_pdf_pages: [491, 495]
book_pages: [455, 459]
batch: "010"
language: en
type: source-transcription
status: complete-for-range
---

# Chapter 13: specialisation-aware width, self-sufficiency, and six design principles

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous pages](0485-0490-ch13-dictionary-thresholds-and-rank-contraction.md#pdf-0490) · [Next pages](0496-0500-ch13-pruning-growth-and-synthesis.md#pdf-0496)

> Faithful English transcription of physical PDF pages 491–495 / printed pages 455–459. PDF 491 continues the sentence at the end of PDF 490. Original formulas, claims, qualifications, unresolved references, and the six design principles are preserved. Editorial checks are separate in Russian notes 165–168.

<a id="pdf-0491"></a>
## PDF 491 · Book 455

of the score matrix, and an antisymmetric component $R_h^{(\ell),a}$, contributed by $M_a^{(\ell,h)}$. The two components have different rank characteristics: by the symplectic decomposition of Chapter 11 (Proposition ?? and the canonical symplectic form), the antisymmetric component contributes *eigenplanes* of dimension at most $2$, while the symmetric component contributes a positive-semidefinite operator of rank up to $d_k$.

<a id="theorem-13-34"></a>
**Theorem 13.34 (Specialisation-aware $d_{\mathrm{ff}}$ lower bound).** Under the standing hypotheses of §13.1 and the symplectic decomposition of $M_a$ of Chapter 11, the minimum feed-forward width at layer $\ell$ satisfies

<a id="equation-13-65"></a>
$$
d_{\mathrm{ff}}^*(\ell)\ge
2d_{\mathrm{model}}-\sum_{h=1}^{H}
\left[(1-\alpha_h^{(\ell)})^2d_k+2(\alpha_h^{(\ell)})^2\right]
\tag{13.65}
$$

where $\alpha_h^{(\ell)}:=\lVert M_a^{(\ell,h)}\rVert_F/\lVert M^{(\ell,h)}\rVert_F\in[0,1]$ is the S–A balance of head $(\ell,h)$.

*Proof.* The argument refines the rank count of Theorem 13.32 by separating the symmetric and antisymmetric contributions of each head.

*Step 1: head-wise rank decomposition.* Fix a head $(\ell,h)$. The score matrix decomposes orthogonally as $M^{(\ell,h)}=M_s^{(\ell,h)}+M_a^{(\ell,h)}$ with $\lVert M^{(\ell,h)}\rVert_F^2=\lVert M_s^{(\ell,h)}\rVert_F^2+\lVert M_a^{(\ell,h)}\rVert_F^2$. The squared S–A balance $(\alpha_h^{(\ell)})^2$ measures the antisymmetric fraction; its complement $1-(\alpha_h^{(\ell)})^2$ is the symmetric fraction. The two fractions are inherited by the row space:

<a id="equation-13-66"></a>
$$
R_h^{(\ell)}=R_h^{(\ell),s}\oplus R_h^{(\ell),a},
\tag{13.66}
$$

where the direct sum is generic in the post-training regime (Hypothesis H-trained).

*Step 2: rank bounds for the two components.* The symmetric component $R_h^{(\ell),s}$ has rank at most $d_k$; in the regime of small antisymmetric fraction $(\alpha_h\approx0)$, the head behaves as a reciprocal attention head, and the symmetric component saturates this bound. Conversely, in the regime of large antisymmetric fraction $(\alpha_h\approx1)$, the head's score is purely antisymmetric and, by the canonical symplectic decomposition (Chapter 11), the antisymmetric component $R_h^{(\ell),a}$ has rank at most $2$ per dominant eigenplane. In the intermediate regime, the rank scales quadratically with the relevant fraction, giving

<a id="equation-13-67"></a>
$$
\dim R_h^{(\ell),s}\le(1-\alpha_h^{(\ell)})^2d_k,
\tag{13.67}
$$

<a id="equation-13-68"></a>
$$
\dim R_h^{(\ell),a}\le2(\alpha_h^{(\ell)})^2.
\tag{13.68}
$$

The quadratic dependence is the natural one: the contribution of a component to the rank scales as the square of its norm fraction, because both the column space and the row space depend on the component's spectrum.

*Step 3: layer-wise sum and bound on the joint rank.* Summing over heads,

<a id="equation-13-69"></a>
$$
r^{(\ell)}=\dim\!\left(\textstyle\sum_hR_h^{(\ell)}\right)
\le\textstyle\sum_h\left[(1-\alpha_h^{(\ell)})^2d_k+2(\alpha_h^{(\ell)})^2\right],
\tag{13.69}
$$

where the sum is bounded above by the sum of the dimensions of the individual head row spaces.

*Step 4: substitution into the rank-contraction bound.* Substituting the above into Corollary 13.33 gives the claimed bound (13.65). $\square$

<a id="pdf-0492"></a>
## PDF 492 · Book 456

**Reading the refined bound.** The refined bound reproduces (13.64) at the two extremes and interpolates between them as a function of the specialisation distribution.

- *Unspecialised regime* ($\alpha_h=0$ for all $h$). The bound reduces to $d_{\mathrm{ff}}^*\ge2d_{\mathrm{model}}-Hd_k=d_{\mathrm{model}}$ when $Hd_k=d_{\mathrm{model}}$, recovering the unspecialised rank-contraction bound of Corollary 13.33.
- *Maximally specialised regime* ($\alpha_h=1$ for all $h$). The bound reduces to $d_{\mathrm{ff}}^*\ge2d_{\mathrm{model}}-2H$, again recovering the rank-contraction bound under maximal directional specialisation.
- *Mixed regime* (heterogeneous $\alpha_h$). The bound is controlled by the second moment of the specialisation distribution. A layer with a balanced mix of symmetric and directional heads — corresponding to $\langle\alpha_h^2\rangle\approx1/2$ — has $d_{\mathrm{ff}}^*\ge2d_{\mathrm{model}}-\frac12Hd_k-H$, substantially larger than either extreme.

**Numerical example: BertBase.** For BertBase ($H=12,d_k=64,d_{\mathrm{model}}=768$), the bound takes the values $d_{\mathrm{ff}}^*\ge768$ (unspecialised, $\alpha=0$), $d_{\mathrm{ff}}^*\ge1512$ (maximally specialised, $\alpha=1$), $d_{\mathrm{ff}}^*\ge1140$ (balanced mix, $\langle\alpha^2\rangle\approx0.5$). The standard choice $d_{\mathrm{ff}}=3072=4d_{\mathrm{model}}$ is therefore loose at every regime: in the balanced mix it provides almost a $3\times$ overhead. The rank-contraction analysis suggests that a width of approximately $2d_{\mathrm{model}}$ would suffice for the trained configuration; the additional factor of two in the standard choice must be justified by other considerations (training-time gradient flow, optimisation conditioning) that lie outside the scope of this chapter.

<a id="section-13-8-4"></a>
### 13.8.4 Self-sufficiency

The combined picture of the dictionary-geometric framework (§13.7) and the specialisation-aware width bound (Theorem 13.34) gives a notion of *self-sufficiency* for the multi-head plus feed-forward block.

<a id="definition-13-35"></a>
**Definition 13.35 (Self-sufficient block).** A multi-head attention block at layer $\ell$ is *self-sufficient* if it carries: (i) at least $K^*=\binom H2$ inter-head dictionary atoms with the threshold property of Theorem 13.27; (ii) a feed-forward sub-layer of width $d_{\mathrm{ff}}^*(\ell)$ at least equal to the bound (13.65); and (iii) a head specialisation distribution $\{\alpha_h^{(\ell)}\}$ with pairwise gap $\delta=\min_{h\ne h'}|\alpha_h-\alpha_{h'}|\ge0.45$ (Theorem 13.31).

The three conditions are the algebraic, dimensional, and dynamical requirements respectively. Conditions (i) and (iii) ensure that the block achieves orbit separation under standard training; condition (ii) ensures that it does not lose information through rank contraction. The rationalised block of Chapter 14 satisfies all three by construction; the standard transformer block satisfies none in general, and is therefore self-sufficient only by accident or by heuristic over-provisioning.

**Preview.** *The next section extracts the six design principles that the preceding analysis prescribes.*

<a id="section-13-9"></a>
### 13.9 Six design principles

The preceding sections have produced three categories of result. The first is a vocabulary of redundancy, marginality, and crowding (§13.2–§13.6). The second is a pair of architectural constraints: the dictionary threshold (§13.7) and the specialisation-aware feed-forward lower bound (§13.8). The third, implicit in the operational readings of those sections, is a set of

<a id="pdf-0493"></a>
## PDF 493 · Book 457

qualitative prescriptions about how attention blocks should be designed if the analysis is taken seriously.

The present section extracts six such prescriptions in compact form. Each is a *design principle*: an architectural rule, derived from the preceding theorems and conjectures, with a verifiable diagnostic and an explicit consequence at the block level.

The six principles together form the input to Chapter 14. They are not theorems: they are prescriptions whose justification rests on the results cited beside them. The numbering P1–P6 is preserved across this chapter and the next.

<a id="section-13-9-1"></a>
### 13.9.1 The six principles

<a id="principle-13-36"></a>
**Design Principle 13.36 (P1: Preserve directional tails).** A head with $\alpha_h^{(\ell)}$ above the directional-tail threshold of Chapter 11 (Definition ??), and a non-negligible antisymmetric intervention marginal value $\mathcal V_I^a(h)$, must not be pruned or compressed regardless of the verdict of $C_{\mathrm{stat}}^{(\ell)}$ on the layer.

**Justification.** Directional-tail heads carry the antisymmetric mass that the task requires by the orientation conservation analysis of Chapter 12 (Theorem 12.32). Their signatures cluster in the high-$\alpha$ region of $\Sigma$ and are detected as crowded by static-crowding analysis alone. The crowded configuration is not redundancy: by the protection rule of §13.4, two directional-tail heads with distinct Trace B responses cover complementary orientations and are operationally non-redundant. The principle prevents the mistake of compressing the directional tail on geometric grounds.

**Diagnostic.** $\alpha_h^{(\ell)}$ above threshold and $\mathcal V_I^a(h)$ above the layer's median antisymmetric intervention.

**Architectural consequence.** The rationalised block reserves a portion of its head budget for directional-tail heads, with explicit per-head allocation tied to the task's orientation requirements.

<a id="principle-13-37"></a>
**Design Principle 13.37 (P2: Compress symmetric crowding).** A pair of heads $(h,h')$ with high static crowding $C_{\mathrm{stat}}^{(\ell)}$, low antisymmetric content $\alpha_h^{(\ell)},\alpha_{h'}^{(\ell)}\approx0$, and identical Trace A responses is a candidate for compression by averaging into a single symmetric head.

**Justification.** The symmetric sector $M_s^{(\ell,h)}$ admits direct averaging without the anti-commutativity obstruction of the antisymmetric sector. Two symmetric heads with coincident Trace A response are in the regime (b) of §13.4: their orbit content is the same, and the joint coverage they provide is indistinguishable from that of a single averaged head. The parametric saving is exactly $4d_{\mathrm{model}}d_k$ per compressed pair, with a coverage loss that the protection rule of §13.3 treats as the residual between the two heads' Trace A responses.

**Diagnostic.** $C_{\mathrm{stat}}^{(\ell)}$ above the layer median; $\alpha_h,\alpha_{h'}$ both below the symmetric threshold; intervention values $\mathcal V_I^s(h),\mathcal V_I^s(h')$ within a tolerance.

**Architectural consequence.** The rationalised block separates the symmetric and antisymmetric update channels (Chapter 14, §14.3); P2 prescribes how many symmetric channels are sufficient.

<a id="pdf-0494"></a>
## PDF 494 · Book 458

<a id="principle-13-38"></a>
**Design Principle 13.38 (P3: Split overloaded heads).** A head whose activation operator $\mathcal A^{(\ell,h)}(X)$ has rank greater than two, with eigenplane structure indicating multiple incompatible orientations or multiple disjoint task-directions, is a candidate for splitting into two or more specialised heads.

**Justification.** By the canonical symplectic decomposition of Chapter 11 (Proposition ?? and the two-eigenplane structure of $M_a$), a single head can carry at most one dominant antisymmetric eigenplane efficiently. A head with multiple eigenplanes of comparable magnitude is forced to mediate between conflicting orientation demands, with reduced sharpness on each. Splitting the head allocates each demand to a specialised head, recovering full sharpness per eigenplane at the cost of an additional parameter block.

**Diagnostic.** $\operatorname{rank}\mathcal A^{(\ell,h)}(X)>2$ on a representative $X\sim\mu_X$, with second eigenvalue gap below the sharpness threshold of Chapter 12.

**Architectural consequence.** The rationalised block's head-allocation rule treats overloaded heads as growth targets, allocating two heads where one would have sat in the standard architecture.

<a id="principle-13-39"></a>
**Design Principle 13.39 (P4: Grow where $\Gamma_{\mathrm{res}}$ remains high).** Layers with residual directional energy $\Gamma_{\mathrm{res}}^{(\ell)}$ above the layer-mean threshold, and at least one head with positive $\mathcal V_\Gamma(h)$ smaller than the median, are candidates for growth: the addition of one or more heads with specialisation tuned to the unresolved directional content.

**Justification.** By the monotone-decay property of $\Gamma_{\mathrm{res}}$ (Proposition ??), residual directional energy can only decrease as heads are added with eigenplanes covering the unresolved directions. A layer with high $\Gamma_{\mathrm{res}}^{(\ell)}$ has unresolved directional capacity that the existing heads cannot close. Growth is the architectural operation that closes the residual; it is the dual of pruning (P1, P2).

**Diagnostic.** $\Gamma_{\mathrm{res}}^{(\ell)}$ above the layer-mean threshold, combined with a saturation criterion on the existing heads (Chapter 11, §??).

**Architectural consequence.** The rationalised block carries an explicit growth rule that INCRT (Chapter 15) generalises to the fully dynamic case.

<a id="principle-13-40"></a>
**Design Principle 13.40 (P5: Avoid uniform heads).** Allocation of identical resources to all heads at a layer violates the specialisation gap requirement $\delta=\min_{h\ne h'}|\alpha_h^{(\ell)}-\alpha_{h'}^{(\ell)}|\ge0.45$ of Theorem 13.31 and is incompatible with the dictionary threshold of Theorem 13.27.

**Justification.** Theorem 13.31 requires a specialisation gap to ensure coefficient diversity (T2) of the dictionary-geometric framework. Uniform allocation, in which every head shares the same parametrisation, head dimension, and initialisation distribution, drives the system toward $\alpha_h$ clustering at a single value, violating the gap requirement and falling back into the symmetric-collapse failure mode of Chapter 11.

**Diagnostic.** Layer-wise variance of $\alpha_h^{(\ell)}$ below a threshold; or the explicit detection of uniform parameter sharing across heads.

<a id="pdf-0495"></a>
## PDF 495 · Book 459

<a id="table-13-5"></a>
| # | Principle | Diagnostic | Architectural consequence |
|---|---|---|---|
| P1 | Preserve directional tails | $\alpha_h$ high, $\mathcal V_I^a(h)$ high | reserved head budget |
| P2 | Compress symmetric crowding | $C_{\mathrm{stat}}$ high, $\alpha_h\approx0$, Trace A match | symmetric channel sharing |
| P3 | Split overloaded heads | $\operatorname{rank}\mathcal A^{(\ell,h)}>2$, eigenplane mixing | growth as splitting |
| P4 | Grow where $\Gamma_{\mathrm{res}}$ remains high | $\Gamma_{\mathrm{res}}^{(\ell)}$ above layer mean | explicit growth rule |
| P5 | Avoid uniform heads | $\operatorname{Var}(\alpha_h)$ below threshold | heterogeneous allocation |
| P6 | Monitor orbit diversity | $C_{\mathrm{orb}}^{(\ell)}$ tracked over training | training-time monitors |

**Table 13.5: Diagnostic and architectural consequence for each design principle.** The full theoretical sources are listed in the principle statements and their justifications.

**Architectural consequence.** The rationalised block uses heterogeneous head allocation, with specialisation enforced either by separate parametrisation (symmetric vs antisymmetric channels), by initialisation diversity, or by an explicit specialisation regulariser.

<a id="principle-13-41"></a>
**Design Principle 13.41 (P6: Monitor orbit diversity).** Endpoint diversity is insufficient as a measure of representational coverage. The architecture must monitor orbital crowding $C_{\mathrm{orb}}^{(\ell)}$ alongside, and ideally with priority over, static crowding $C_{\mathrm{stat}}^{(\ell)}$.

**Justification.** By the three-regime analysis of §13.4, two heads with distinct endpoint signatures may have followed identical orbits (regime b), giving high orbital crowding and low static crowding. The standard-architecture diagnostic, which measures endpoint diversity through the variance of trained weights, misses this regime entirely. The orbital-crowding functional is the correct measure: it captures the trajectory-level redundancy that endpoint analysis cannot detect.

**Diagnostic.** $C_{\mathrm{orb}}^{(\ell)}$ tracked along the training trajectory, with the crossover marker of §13.4 identifying the moment at which orbit-driven redundancy overtakes endpoint-driven redundancy.

**Architectural consequence.** The rationalised block instruments its training with orbital crowding monitors; the incremental block of Chapter 15 uses these monitors to time the growth and pruning operations.

<a id="section-13-9-2"></a>
### 13.9.2 Diagnostic and consequence summary

A compact summary of the six principles, in the form recommended by the editorial memo for Chapter 13, appears in Table 13.5.

<a id="section-13-9-3"></a>
### 13.9.3 What the principles do not prescribe

The six principles are prescriptive in their respective domains but leave three architectural decisions outside their scope.

**Number of layers.** None of P1–P6 prescribes the depth of the model. Layer count is governed by the layer-wise re-extraction analysis of Chapter ??, not by the redundancy analysis of this chapter. The principles apply within a layer; their extension to depth is a separate question handled in Chapter 14.

> Continued on [PDF 496](0496-0500-ch13-pruning-growth-and-synthesis.md#pdf-0496).
