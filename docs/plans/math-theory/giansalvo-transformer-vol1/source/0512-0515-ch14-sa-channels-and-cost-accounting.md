---
title: "Chapter 14: symmetric–antisymmetric channels and parameter/FLOP accounting"
source_pdf_pages: [512, 515]
book_pages: [476, 479]
batch: "011"
language: en
type: source-transcription
status: complete-for-range
---

# Chapter 14: symmetric–antisymmetric channels and parameter/FLOP accounting

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous pages](0505-0511-ch14-motivation-design-and-block.md#pdf-0511)

> Faithful English transcription of physical PDF pages 512–515 / printed pages 476–479. Includes §14.4, Proposition 14.9, and §14.5 up to its final budget formula. The concluding paragraph of §14.5 continues on PDF 516. The new squared-energy convention for alpha and all implementation heuristics are retained as printed; editorial analysis is separate in Russian notes 175–176.

<a id="pdf-0512"></a>
## PDF 512 · Book 476

<a id="section-14-4"></a>
### 14.4 Separate symmetric and antisymmetric update channels

The central algebraic justification for the rationalised block is the orthogonal decomposition of a square motor into symmetric and antisymmetric components. This decomposition is not an interpretation imposed after training; it is a canonical identity. For any matrix $M$,

<a id="display-0512-sa-decomposition"></a>
$$
M=M_s+M_a,
\qquad M_s=\frac{M+M^\top}{2},
\qquad M_a=\frac{M-M^\top}{2},
$$

and

<a id="display-0512-frobenius-orthogonality"></a>
$$
\langle M_s,M_a\rangle_F=0.
$$

A head may therefore contain two algebraically distinct components even when it is implemented as a single bilinear operation. The rationalised block simply makes this distinction visible.

<a id="section-14-4-1"></a>
### 14.4.1 Interpretation of the two channels

The symmetric channel represents reciprocal relations. It is naturally associated with similarity, mutual binding, local clustering, averaging, and stabilisation. The antisymmetric channel represents oriented relations. It is naturally associated with displacement, order, precedence, role asymmetry, and directional task structure. This distinction should not be read as a hard semantic labelling of all heads. It is a coordinate system: a head can lie between the two extremes, and the mixed channel exists precisely because useful operations may couple reciprocal and directional components.

The scalar

<a id="display-0512-squared-alpha"></a>
$$
\alpha_h=
\frac{\lVert M_{a,h}\rVert_F^2}
{\lVert M_{s,h}\rVert_F^2+\lVert M_{a,h}\rVert_F^2+\varepsilon}
$$

serves as a convenient coordinate. Values near zero indicate symmetric dominance; values near one indicate directional dominance. The small constant $\varepsilon$ avoids instability for nearly null motors. In implementation, $M_h$ may be replaced by the appropriate effective bilinear map induced by query–key or value–output interactions.

<a id="section-14-4-2"></a>
### 14.4.2 Why separation helps

A standard head can represent both $M_s$ and $M_a$, but this does not imply that a standard block allocates them well. When the same head budget is used for both, three failure modes appear.

First, symmetric crowding may hide directional scarcity. Many heads may learn similar reciprocal patterns, producing acceptable local smoothing while leaving orientation poorly represented. The loss may remain competitive on tasks where directional structure is weak, but the architecture has no way to certify that the directional tail is protected.

Second, directional heads may be pruned accidentally if their contribution is small in average norm but large under targeted intervention. This is exactly the case in which marginal value should be used instead of norm alone.

Third, mixed heads may become overloaded. A single head may carry both a reciprocal grouping operation and an oriented displacement operation. Such a head can be functionally valuable, but it is also a sign that the architecture may need a split or a separate channel. The rationalised block addresses these problems by making $S$ and $A$ updates separately observable. The output can still be recombined, but recombination is gated rather than blind.

<a id="pdf-0513"></a>
## PDF 513 · Book 477

<a id="proposition-14-9"></a>
**Proposition 14.9 (Orthogonal channel accounting).** *Assume that the effective motor of a head admits the decomposition $M_h=M_{s,h}+M_{a,h}$ and that diagnostic energies are measured in the Frobenius inner product. Then the symmetric and antisymmetric contributions satisfy*

<a id="display-0513-energy-accounting"></a>
$$
\lVert M_h\rVert_F^2=\lVert M_{s,h}\rVert_F^2+\lVert M_{a,h}\rVert_F^2.
$$

*Consequently, any energy budget assigned to the head can be decomposed without cross-term ambiguity.*

*Proof.* The identity follows immediately from $\langle M_{s,h},M_{a,h}\rangle_F=0$. $\square$

This proposition is elementary, but it is architecturally useful. It permits a head to be audited by how much capacity is assigned to reciprocal and directional components. It also gives a simple diagnostic for whether a layer is dominated by symmetric smoothing or by directional transport.

<a id="section-14-4-3"></a>
### 14.4.3 Channel tying and channel freedom

There are two implementation extremes. In a tied implementation, a head is constrained to be symmetric or antisymmetric by construction. In a free implementation, the head is unconstrained but its $\alpha_h$ coordinate is monitored and regularised. The rationalised block does not force one extreme. A practical architecture may use a small number of tied anchor heads together with free mixed heads. This hybrid design is often preferable, because it preserves expressivity while ensuring that the layer cannot entirely lose one channel.

Let

<a id="display-0513-anchor-subsets"></a>
$$
\mathcal H_{S,\ell}^{\mathrm{anchor}}\subseteq\mathcal H_\ell,
\qquad\mathcal H_{A,\ell}^{\mathrm{anchor}}\subseteq\mathcal H_\ell
$$

be anchor subsets. A simple static rule is

<a id="display-0513-anchor-budget"></a>
$$
\left|\mathcal H_{A,\ell}^{\mathrm{anchor}}\right|\ge\lceil\eta_\ell H_\ell\rceil,
$$

where $\eta_\ell$ is an estimate of the directional share required at layer $\ell$. A possible proxy is

<a id="display-0513-directional-share"></a>
$$
\eta_\ell=
\frac{\Gamma_{\mathrm{res},A}^{(\ell)}}
{\Gamma_{\mathrm{res},S}^{(\ell)}+\Gamma_{\mathrm{res},A}^{(\ell)}+\varepsilon}.
$$

This rule should be interpreted as a static allocation heuristic. In INCRT, the same quantity becomes a time-dependent growth signal.

<a id="section-14-4-4"></a>
### 14.4.4 Relation with two-flow interpretation

The two-flow interpretation says that representation transport has a reciprocal component and a directional component. The rationalised block is the architectural form of this interpretation. The $S$ channel controls reciprocal consolidation; the $A$ channel controls directional transport; the mixed channel allows the two flows to interact. Without this separation, the two-flow view remains diagnostic. With the separation, it becomes a design principle.

**Takeaway.** *The S–A split is not a decorative decomposition. It gives an orthogonal accounting of reciprocal and directional capacity, protects directional tails, exposes symmetric crowding, and prepares the dynamic allocation rules of the incremental architecture.*

<a id="pdf-0514"></a>
## PDF 514 · Book 478

<a id="section-14-5"></a>
### 14.5 Parameter and FLOP accounting

A rationalised architecture should not be justified only by diagrams. It must also be accountable in parameters and computation. The purpose of this section is not to claim that the rationalised block is always cheaper than a standard Transformer block. That claim would be too strong. The correct claim is more precise: the rationalised block replaces uniform allocation by diagnostic allocation. It may be cheaper, equal in cost, or slightly more expensive depending on the chosen tolerance; but the expenditure is interpretable.

<a id="section-14-5-1"></a>
### 14.5.1 Standard accounting

For a standard layer with model dimension $d$, $H$ heads, and head dimension $d_k=d/H$, the attention projections have approximately

<a id="display-0514-standard-attention-budget"></a>
$$
P_{\mathrm{att}}^{\mathrm{std}}\approx4d^2
$$

parameters: query, key, value, and output. The feed-forward sub-layer with expansion $d_{\mathrm{ff}}$ has approximately

<a id="display-0514-standard-ffn-budget"></a>
$$
P_{\mathrm{ff}}^{\mathrm{std}}\approx2d\,d_{\mathrm{ff}}
$$

parameters, ignoring biases. With the usual choice $d_{\mathrm{ff}}=4d$, this becomes $8d^2$. The idealised standard block has therefore

<a id="display-0514-standard-block-budget"></a>
$$
P_{\mathrm{block}}^{\mathrm{std}}\approx12d^2.
$$

The corresponding leading FLOP terms have the same qualitative allocation: attention spends computation on projections and attention matrices, while the feed-forward block usually dominates in dense regimes.

This accounting is simple but uninformative. It explains how many parameters are used; it does not explain why these parameters are allocated uniformly across heads and uniformly across feed-forward coordinates.

<a id="section-14-5-2"></a>
### 14.5.2 Rationalised accounting

The rationalised block decomposes the attention budget as

<a id="display-0514-rationalised-attention-budget"></a>
$$
P_{\mathrm{att}}^{\mathrm{rat}}=P_S+P_A+P_M+P_{\mathrm{gate}}+P_{\mathrm{out}},
$$

where $P_S$ is the symmetric-channel budget, $P_A$ the directional-channel budget, $P_M$ the mixed-channel budget, $P_{\mathrm{gate}}$ the diagnostic-gating budget, and $P_{\mathrm{out}}$ the output aggregation budget. In a conservative implementation, this total can be set equal to $4d^2$ by redistributing the standard projections. In an expanded implementation, extra gates or adapters add a lower-order term.

The feed-forward budget becomes

<a id="display-0514-rationalised-ffn-budget"></a>
$$
P_{\mathrm{ff}}^{\mathrm{rat}}=2d\,d_{\mathrm{ff}}^{\mathrm{rat}}+P_{\mathrm{route}},
$$

where $P_{\mathrm{route}}$ accounts for routing or gating. The width $d_{\mathrm{ff}}^{\mathrm{rat}}$ is no longer forced to be $4d$. It may be chosen from a residual-rank or coverage criterion. A schematic rule is

<a id="display-0514-rank-allocation-rule"></a>
$$
d_{\mathrm{ff}}^{\mathrm{rat}}=d+\lambda_Sr_S^{(\ell)}+\lambda_Ar_A^{(\ell)}+\lambda_Rr_{\mathrm{res}}^{(\ell)},
$$

where the $r$ terms are effective ranks or dimensions associated with symmetric, directional, and residual components. This expression should be interpreted as an allocation formula, not as a universal law.

<a id="pdf-0515"></a>
## PDF 515 · Book 479

<a id="section-14-5-3"></a>
### 14.5.3 Cost regimes

Three regimes are useful.

**Iso-parameter rationalisation.** The total parameter count is kept close to the standard block. The gain is not compression but interpretability and allocation. Heads are redistributed into $S$, $A$, and mixed channels, and the feed-forward width is reweighted without changing the total budget substantially.

**Compression rationalisation.** If symmetric crowding is high and many heads have low marginal value, the number of effective head slots or feed-forward units can be reduced. This is the regime in which rationalisation becomes a compression method.

**Expansion rationalisation.** If coverage residuals and directional residual energy remain high, the rationalised block may require more capacity than the standard baseline at the same nominal $H$. This is not a failure. It indicates that the baseline is under-allocated for the measured geometry. In the static chapter, this appears as a design warning. In INCRT, it becomes a growth trigger.

<a id="table-14-2"></a>
| Regime | Parameter effect | Interpretation |
|---|---|---|
| Iso-parameter | approximately unchanged | same budget, better allocation and monitoring |
| Compression | lower effective budget | symmetric crowding or low marginal value has been detected |
| Expansion | higher local budget | uncovered signatures or high residual energy remain |

**Table 14.2:** Parameter regimes of the rationalised block. The architecture is not defined by being smaller; it is defined by making the allocation measurable.

<a id="section-14-5-4"></a>
### 14.5.4 FLOP interpretation

The FLOP cost follows the same principle. A diagnostic gate has negligible cost compared with attention matrices and dense feed-forward layers. The main cost is incurred when additional heads, wider feed-forward channels, or more expensive orbit diagnostics are used. For this reason, a practical implementation should distinguish between training-time diagnostics and inference-time execution. During training, crowding and marginality may be estimated periodically. During inference, the resulting gates and allocations may be frozen or simplified.

This distinction is important. The rationalised block does not require computing a full orbit-crowding metric at every inference step. It requires that the architecture be designed and trained with these quantities in view. After training, many diagnostics can be compiled into gates, masks, or structured parameter groups.

<a id="section-14-5-5"></a>
### 14.5.5 Accounting conclusion

The standard block gives a clean formula, $P\approx12d^2$, but little explanation of allocation. The rationalised block gives a more structured formula:

<a id="display-0515-rationalised-block-budget"></a>
$$
P_{\mathrm{block}}^{\mathrm{rat}}=P_S+P_A+P_M+P_{\mathrm{gate}}+P_{\mathrm{out}}+2d\,d_{\mathrm{ff}}^{\mathrm{rat}}+P_{\mathrm{route}}.
$$

---

[Continuation of §14.5.5](0516-0517-ch14-subsumption-and-algebraic-instances.md#pdf-0516) · [Russian note 175](../notes/175-sa-energy-alpha-conventions-and-channel-protection.md) · [Russian note 176](../notes/176-parameter-flop-budgets-and-diagnostic-overhead.md)
