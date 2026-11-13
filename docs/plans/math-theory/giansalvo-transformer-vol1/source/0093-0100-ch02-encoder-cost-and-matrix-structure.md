---
title: "Chapter 2: encoder block, computational cost, and the beginning of matrix structure"
source_pdf_pages: [93, 100]
book_pages: [57, 64]
batch: "002"
language: en
type: source-transcription
status: complete-for-page-range
continues_after_batch: true
---

# Chapter 2: encoder block, computational cost, and the beginning of matrix structure

[Library index](../README.md) · [Source and transcription policy](../SOURCE.md) · [Previous pages: PDF 82–92](0082-0092-ch02-output-projection-and-variants.md)

> Editorial note: This file transcribes all source content on PDF pages 93–100 (printed pages 57–64). It begins with the continuation of Remark 2.20 and ends after the opening fragment of Remark 2.29. The next physical page is [PDF 101 / printed page 65](0101-0108-ch02-kernel-rank-gauge-and-asymmetry.md#pdf-0101), now available in batch 003; it contains Figure 2.11. The sentence resumes on [PDF 102](0101-0108-ch02-kernel-rank-gauge-and-asymmetry.md#pdf-0102). Unresolved citations, inconsistent claims and numerical conventions are retained; see the Russian notes on [projection variants](../notes/12-projection-variants.md), [FFN and residual streams](../notes/13-ffn-and-residual-stream.md) and [computational cost](../notes/14-computational-cost.md).

<a id="pdf-0093"></a>
## PDF 93 · Book 57

<a id="remark-2-20-continuation"></a>
> Editorial boundary: continuation of [Remark 2.20 on PDF 92](0082-0092-ch02-output-projection-and-variants.md#remark-2-20).

The key observation is that $\operatorname{rank}(M_a^{(l)})\le d_k^{(l)}$, and $d_k^{(l)}$ decreases with the level in HKT. This is the *opposite* of what the Rank-Scale Proportionality principle (Chapter ??) prescribes: coarser scales should have more rank in $M_a$, not less, because global directed structure is richer and harder to compress than local structure.

The resolution of this tension is itself a design choice: HKT uses more heads at fine scales (where local structure is dense) and fewer, higher-rank heads at coarse scales. The budget is balanced, but distributed according to scale. HKT also proves that the hierarchical score matrix defines a valid symmetric positive definite kernel (Proposition 3.1), and that HKT strictly subsumes standard MHA and causal convolution in representational capacity (Proposition 3.4). The total computational overhead is bounded above by $\frac43$ regardless of the number of levels (Proposition 4.5), reaching $1.31\times$ for $L=3$. On a synthetic ListOps benchmark, HKT achieves $+19$ percentage points over standard MHA at this modest overhead.

The HKT architecture is one instantiation of a general principle: the heads of a multi-head attention mechanism should be understood as a *multi-resolution atlas* of the token space, not a collection of equivalent projections. The formal development of this principle — and its connection to the directional energy theory of Chapter 4 — is one of the themes of Chapter ?? and of Volume II.

> Editorial note: The bound $\operatorname{rank}(M_a)\le d_k$ does not follow from $\operatorname{rank}(M)\le d_k$. Without additional conditions, the general bound is $\operatorname{rank}(M_a)\le\min(d,2d_k)$. The cited HKT propositions and experiments are reported by the author here; their proofs and measurements have not been independently supplied or checked in this page range.

<a id="section-2-4"></a>
### 2.4 The Feed-Forward Block and the Residual Stream

Every encoder block in BERT contains two sub-layers: the multi-head attention of Section 2.3, and a feed-forward network (FFN, also called an MLP). The two sub-layers are connected by residual connections and separated by layer normalisations. Together they define the *encoder block*, the building brick from which all of BERT’s layers are assembled.

<a id="section-2-4-1"></a>
#### 2.4.1 The two-layer feed-forward network

<a id="definition-2-21"></a>
**Definition 2.21 (Feed-forward block).** The *feed-forward network* (FFN) of a single encoder block is a two-layer MLP:

<a id="equation-2-18"></a>
$$
\mathrm{FFN}(Z):=\varphi(ZW_1+\mathbf1b_1^\top)W_2+\mathbf1b_2^\top,\tag{2.18}
$$

where $W_1\in\mathbb R^{d\times d_{\mathrm{ff}}}$, $b_1\in\mathbb R^{d_{\mathrm{ff}}}$, $W_2\in\mathbb R^{d_{\mathrm{ff}}\times d}$, $b_2\in\mathbb R^d$, and $\varphi:\mathbb R\to\mathbb R$ is a coordinate-wise nonlinearity. In $\mathrm{BERT}_{\mathrm{base}}$, $\varphi=\mathrm{GeLU}$ (Gaussian Error Linear Unit, $\mathrm{GeLU}(t)=t\Phi(t)$, with $\Phi$ the standard normal CDF) and $d_{\mathrm{ff}}=4d=3{,}072$.

The FFN first projects each token representation from $\mathbb R^d$ to the wider space $\mathbb R^{d_{\mathrm{ff}}}$ (expansion by a factor of 4), applies $\varphi$ coordinate-wise, then projects back to $\mathbb R^d$. Each token is processed independently: the FFN applies the same function to each row of $Z$ without mixing information between rows. In this sense, the FFN is a *pointwise* operation — the opposite of the attention mechanism, which mixes information across the sequence dimension.

<a id="remark-2-22"></a>
**Remark 2.22 (Activation functions across Transformer variants).** The choice of $\varphi$ in (2.18) has evolved considerably since the original Transformer, and the evolution reflects a deepening algebraic understanding of what the FFN computes. We distinguish two families.

**Non-gated activations.** The original Transformer (Vaswani et al., 2017) used $\mathrm{ReLU}(t)=\max(0,t)$, which introduces exact zeros: on a Gaussian input approximately 50% of hidden neurons are inactive at any forward pass. BERT (Devlin et al., 2019a) replaced it with $\mathrm{GeLU}(t)=t\Phi(t)$ (?), which is smooth and differentiable everywhere and produces soft suppression (neurons with negative pre-activation are attenuated, not silenced). SiLU (?),

<a id="pdf-0094"></a>
## PDF 94 · Book 58

defined as $\mathrm{SiLU}(t)=t\sigma(t)$ with $\sigma$ the logistic sigmoid, is algebraically similar to GeLU and used in PaLM (?) and Gemini.

**Gated activations (GLU family).** ? introduced a family of gated variants in which the single wide projection $ZW_1$ is replaced by two parallel projections:

<a id="equation-2-19"></a>
$$
\mathrm{FFN}_{\mathrm{gate}}(Z):=[\varphi(ZW_1)\odot(ZW_2)]W_3,\tag{2.19}
$$

where $\odot$ is coordinate-wise multiplication and $W_1,W_2\in\mathbb R^{d\times d'_{\mathrm{ff}}}$. The principal instances are **SwiGLU** ($\varphi=\mathrm{SiLU}$, used in LLaMA (?), Mistral, and Gemma), **GeGLU** ($\varphi=\mathrm{GeLU}$, PaLM), and **ReGLU** ($\varphi=\mathrm{ReLU}$, maximal structured sparsity). To keep parameter count comparable, gated variants use $d'_{\mathrm{ff}}=\frac23d_{\mathrm{ff}}$, so that the total count $3dd'_{\mathrm{ff}}$ matches $2dd_{\mathrm{ff}}$.

**Algebraic distinction: static versus dynamic sparsity.** The two families produce qualitatively different sparsity structures. Non-gated activations produce *static sparsity*: the pattern of active versus suppressed neurons is determined by the marginal distribution of pre-activation values and is statistically similar across tokens. The gate $ZW_2$ in gated variants introduces *dynamic sparsity*: the active subspace depends on the specific token representation $Z$ and varies from token to token within the same forward pass. In the key-value memory interpretation of Geva et al. (2021) — where $W_1$ encodes “keys” and $W_3$ encodes “values” — the gate $ZW_2$ selects which values to retrieve in a token-conditional manner, making the computation genuinely input-dependent in a way that static activations cannot achieve.

The formal consequences for information flow through the residual stream — in particular how the sparsity induced by $\varphi$ interacts with the Channel Destruction effect of $W_O$ analysed in Chapter ?? — depend critically on this distinction.

<a id="table-2-7"></a>
**Table 2.7:** Feed-forward sub-layer parameters, $\mathrm{BERT}_{\mathrm{base}}$ ($d=768$, $d_{\mathrm{ff}}=3072$).

| Matrix | Shape | Parameters |
|---|---|---:|
| $W_1$ | $d\times d_{\mathrm{ff}}$ | $768\times3{,}072=2{,}359{,}296$ |
| $b_1$ | $d_{\mathrm{ff}}$ | $3{,}072$ |
| $W_2$ | $d_{\mathrm{ff}}\times d$ | $3{,}072\times768=2{,}359{,}296$ |
| $b_2$ | $d$ | $768$ |
| **Total (FFN)** | | **4,722,432** |

The FFN contains roughly twice as many parameters as the attention sub-layer (Table 2.7); its inference cost is derived in Section 2.5 and placed in direct comparison with the attention cost in Table 2.8. Its role in the network is the subject of active debate in the interpretability literature (Geva et al., 2021; Meng et al., 2022): some analyses suggest it acts as a key-value memory store; others emphasise its role as a non-linear feature combiner. The analysis of this book focuses on what the FFN does to the channel structure of the attention output — a question whose answer depends, as Remark 2.22 shows, on which activation family is used.

<a id="section-2-4-2"></a>
#### 2.4.2 The encoder block and the residual stream

<a id="definition-2-23"></a>
**Definition 2.23 (Encoder block).** A single encoder block transforms the hidden state matrix $X^{(l-1)}\in\mathbb R^{n\times d}$ into $X^{(l)}\in\mathbb R^{n\times d}$ via two residual sub-layers, in the pre-norm convention used

<a id="pdf-0095"></a>
## PDF 95 · Book 59

by BERT (Devlin et al., 2019a):

<a id="equation-2-20"></a>
$$
Z^{(l)}:=X^{(l-1)}+\mathrm{MHA}(\mathrm{LN}(X^{(l-1)})),\tag{2.20}
$$

<a id="equation-2-21"></a>
$$
X^{(l)}:=Z^{(l)}+\mathrm{FFN}(\mathrm{LN}(Z^{(l)})),\tag{2.21}
$$

where LN is the layer normalisation of Definition 1.23 (Chapter 1).

The residual connections ensure that the cumulative representation of Chapter 1 (Definition 1.28) is preserved: unrolling Equations (2.20)–(2.21) across all $L$ blocks gives

$$
X^{(L)}=X^{(0)}+\sum_{l=1}^L\Delta^{(l)},
$$

where $\Delta^{(l)}=\mathrm{MHA}(\mathrm{LN}(X^{(l-1)}))+\mathrm{FFN}(\mathrm{LN}(Z^{(l)}))$ is the total increment at block $l$. The Rank-Neutrality Theorem (Theorem 1.25 of Chapter 1) guarantees that LN preserves the rank of the hidden state at each step; any change in rank must therefore come from the learned transformations $W_Q^{(h)},W_K^{(h)},W_V^{(h)},W_O,W_1,W_2$.

<a id="remark-2-24"></a>
**Remark 2.24 (Pre-norm vs post-norm: geometric and gradient-flow consequences).** Definition 2.23 uses the *pre-norm* convention, in which layer normalisation is applied to the input of each sub-layer before the transformation:

$$
X^{(l)}=X^{(l-1)}+f(\mathrm{LN}(X^{(l-1)})).
$$

The alternative *post-norm* convention (Vaswani et al., 2017), used in the original BERT and the original Transformer, applies normalisation *after* the residual addition:

$$
X^{(l)}=\mathrm{LN}(X^{(l-1)}+f(X^{(l-1)})).
$$

**Geometric consequence.** In post-norm, LN acts on $X^{(l-1)}+f(X^{(l-1)})$: it projects the combined vector onto $S^{d-1}\cap\mathbf1^\perp$, the sphere within the zero-mean hyperplane, erasing the scale of both the residual and the sub-layer output together. If $f(X^{(l-1)})$ is small relative to $X^{(l-1)}$ — as at initialisation, when weights are near zero — normalisation effectively absorbs the sub-layer contribution into the rescaling, leaving the direction of $X^{(l-1)}$ nearly unchanged. In pre-norm, the residual $X^{(l-1)}$ passes through unchanged: only $\mathrm{LN}(X^{(l-1)})$, the normalised version, enters $f$. The Rank-Neutrality Theorem applies to $\mathrm{LN}(X^{(l-1)})$ alone, and the residual stream $X^{(l)}=X^{(0)}+\sum_{l'}\Delta^{(l')}$ is a plain sum of unnormalised increments — precisely the structure exploited throughout Part II.

**Gradient-flow consequence.** ? prove with mean-field theory that in post-norm, at initialisation, the expected gradient norm near the output layers is $O(L)$ in the number of layers, causing instability and requiring a carefully tuned learning-rate warm-up. In pre-norm, gradient norms are $O(1)$ at every depth: the residual path $X^{(l-1)}$ provides a direct gradient highway, and the LN Jacobian does not accumulate multiplicatively. ? make the post-norm pathology precise: the product of LN Jacobians across $L$ layers has spectral radius $<1$ per factor, producing exponential gradient decay. Pre-norm eliminates this product by keeping LN inside the residual branch.

**Limits of pre-norm: massive activations.** Pre-norm has its own drawback: because the residual identity path is never normalised, large activation values that arise in attention or MLP outputs persist across layers without attenuation. ? document this “massive activation” phenomenon empirically in large pre-trained models and show it can degrade representation quality. ? prove that for a trained pre-norm Transformer with weight decay, gradient norms

<a id="pdf-0096"></a>
## PDF 96 · Book 60

with respect to the final-layer weights can be unbounded in the presence of large activations, whereas Peri-LN — which applies LN both before and after each sub-layer — guarantees bounded gradients.

**NormFormer.** ? identify a “gradient magnitude mismatch” in pre-norm: early layers receive much larger gradients than late layers, the reverse of the post-norm pathology. Their NormFormer adds extra normalisation layers (post-attention, post-FFN, and per-head scaling) to pre-norm, recovering uniform gradient magnitude at the cost of a negligible parameter increase ($<0.1\%$).

**Algebraic consequence for this book.** The Rank-Neutrality Theorem (Chapter 1) holds in both conventions. The difference is in what the theorem applies to: in post-norm it applies to the sum $X^{(l-1)}+f(X^{(l-1)})$, making the cumulative representation $X^{(L)}=X^{(0)}+\sum_l\Delta^{(l)}$ an approximation (exact only when the LN scaling factors are constant). In pre-norm the theorem applies to $X^{(l-1)}$ alone, the cumulative representation is exact, and the analysis of Part II goes through without approximation. This is the primary reason we adopt pre-norm throughout.

> Editorial note: Definition 2.23 attributes pre-norm to BERT, while Remark 2.24 on the same page explicitly attributes post-norm to original BERT. Both source statements are retained. The literal “?” citations in Remark 2.24 are unresolved in the supplied PDF and have not been replaced by guessed references.

<a id="section-2-4-3"></a>
#### 2.4.3 Channel destruction: a preview

Equation (2.8) shows that the output of the multi-head attention sub-layer is a sum:

$$
\mathrm{MHA}(X_{\mathrm{pos}})=\sum_{h=1}^H Y^{(h)}W_O^{(h)}.
$$

This sum is invariant under any permutation of the heads. The FFN block receives this sum as its input; it therefore cannot, in principle, distinguish which head contributed which part of the signal. After the FFN and the second residual connection, the representation that enters the next block retains no record of the head indices. Each subsequent attention layer must reconstruct whatever head-specific structure it needs from a mixture that has lost the channel labels.

<a id="proposition-2-25"></a>
**Proposition 2.25 (Permutation equivariance of the multi-head sum).** *Let $\mathrm{MHA}(X)=\sum_{h=1}^H Y^{(h)}W_O^{(h)}$ be the multi-head output. For any permutation $\pi$ of $\{1,\ldots,H\}$,*

$$
\sum_{h=1}^H Y^{(\pi(h))}W_O^{(\pi(h))}
=\sum_{h=1}^H Y^{(h)}W_O^{(h)}.
$$

*Consequently, the labelling of the summands is a parametrisation convention: simultaneous relabelling of heads and output blocks leaves $\mathrm{MHA}(X)$ unchanged.*

*Proof.* Addition is commutative. $\square$

<a id="remark-2-26"></a>
**Remark 2.26 (Linear recoverability of the head outputs).** A naive reading of Proposition 2.25 is that the individual head contributions $Y^{(h)}W_O^{(h)}$ are irrecoverably destroyed by the sum. *This reading is too strong.* In the canonical regime $Hd_v=d$ used by every standard Transformer (BERT-base: $H=12$, $d_v=64$, $d=768$), the matrix $W_O\in\mathbb R^{Hd_v\times d}=\mathbb R^{d\times d}$ is square; if it is invertible (the generic case for a randomly initialised $W_O$ and the empirical case for trained checkpoints), then

$$
\operatorname{Concat}(Y^{(1)},\ldots,Y^{(H)})=\mathrm{MHA}(X)W_O^{-1},
$$

so the concatenated head outputs $Y^{(h)}$ are linearly recoverable from $\mathrm{MHA}(X)$, and consequently the contributions $Y^{(h)}W_O^{(h)}$ are also recoverable. What is ambiguous is the head

<a id="pdf-0097"></a>
## PDF 97 · Book 61

label: an external observer who sees only $\mathrm{MHA}(X)$ and does not know which block of $W_O$ corresponds to which head has no way to assign the $H$ recovered blocks to indices $1,\ldots,H$ uniquely. The permutation symmetry of Proposition 2.25 is therefore a *label ambiguity*, not an information loss.

What is lost is *interpretability across layers*. The downstream FFN block and the next attention layer receive only $\mathrm{MHA}(X)$, not the labelled tuple $(Y^{(h)}W_O^{(h)})_h$. They cannot exploit the head labels even though the head-specific contributions are linearly available, because the next layer’s attention/FFN parameters are functions of the summed input alone. This is the structurally meaningful version of "channel destruction", and it is what motivates the analysis of the global information flow in Chapter ??: the summation does not destroy the information, but it makes the head-specific information unavailable to any computation that respects the architectural composition.

<a id="remark-2-27"></a>
**Remark 2.27 (Rank collapse versus channel destruction).** The Channel Destruction phenomenon of Proposition 2.25 is distinct from, and should not be confused with, the *rank collapse* identified by Dong et al. (2021b). The two phenomena differ in mechanism, scale, and remedy.

*Rank collapse* (Dong et al.) is a *global, asymptotic* phenomenon: if self-attention is applied iteratively without residual connections, the product $A^{(L)}\cdots A^{(1)}X^{(0)}$ converges toward rank 1 as $L\to\infty$, because each attention matrix $A^{(l)}$ is row-stochastic and repeated products of row-stochastic matrices converge. The remedy is the residual connection plus the MLP, which introduces new directions outside the current row space (Proposition 1.30).

*Channel Destruction* is a *local, per-forward-pass* phenomenon: within a single layer, the sum $\sum_hY^{(h)}W_O^{(h)}$ irreversibly erases the identity of individual heads. This occurs before the MLP, and the MLP does not repair it — it operates on the already-mixed output and therefore cannot separate the head contributions. Dong et al. show that the MLP increases the rank of $X^{(l)}$; this addresses rank collapse but leaves channel destruction untouched.

Three mechanisms, three distinct questions:

<a id="table-mechanisms-unnumbered"></a>
| Mechanism | Cause | Remedy |
|---|---|---|
| Rank collapse | Iterated row-stochastic maps | MLP + residual |
| Channel Destruction | Commutative sum in MHA | Not in standard arch. |
| Position-invariance of $W_O$ | Fixed map per head | PG-OP (§2.3.3) |
| Layer normalisation | — | Rank-neutral (Thm. 1.25) |

The last row corrects a secondary claim of Dong et al., who write that layer normalisation “plays no role” in preventing rank collapse. The Rank-Neutrality Theorem (Theorem 1.25) establishes something stronger: layer normalisation is *exactly rank-preserving* under non-degeneracy conditions. It does not prevent rank collapse — but not because it is passive or irrelevant. It is transparent: the rank of $X^{(l)}$ and the rank of $\mathrm{LN}(X^{(l)})$ are identical, making them interchangeable for any purpose that depends on representational rank. Neutrality is a precise theorem, not a failure.

> Editorial note: Proposition 2.25 establishes invariance under simultaneous relabelling. It does not establish irrecoverable information loss or an inability of a later linear map to decode an invertible $W_O$. Likewise, row-stochasticity alone is insufficient for convergence to rank one. The stronger prose claims above require conditions beyond the displayed proposition; see [the residual-stream note](../notes/13-ffn-and-residual-stream.md#channel-destruction).

<a id="section-2-5"></a>
### 2.5 Computational Cost of the Attention Mechanism

The algebraic structure of the attention mechanism determines not only what it can compute, but at what cost. This section derives exact floating-point operation (FLOP) counts for every component of the encoder block, yielding a precise formula for the total inference cost of $\mathrm{BERT}$. The analysis provides two things: a concrete accounting of where parameters and

<a id="pdf-0098"></a>
## PDF 98 · Book 62

operations live, and the correct vocabulary for discussing the efficiency gains promised by the theory of Part II.

A FLOP is a single floating-point multiply-add. Given matrices $A\in\mathbb R^{m\times n}$ and $B\in\mathbb R^{n\times p}$, the matrix product $AB$ costs $2mnp$ FLOPs ($n$ multiplications and $n-1$ additions per output entry, giving $\approx2n$ per entry and $mp$ entries). We use this throughout.

Table 2.8 provides the complete picture for a single encoder layer of $\mathrm{BERT}_{\mathrm{base}}$ at sequence length $L=128$: for each component it shows the parameter matrices (from Tables 19.1 and 2.7), the symbolic FLOP formula derived below, and the numerical cost. This joint view is the natural reference for the efficiency comparisons in the rest of the chapter.

<a id="table-2-8"></a>
**Table 2.8:** Parameters and inference FLOPs per encoder layer, $\mathrm{BERT}_{\mathrm{base}}$ ($d=768$, $H=12$, $d_k=64$, $d_{\mathrm{ff}}=3072$, $L=128$). FLOPs computed under the standard convention $\operatorname{cost}(AB)=2mnp$ for $A\in\mathbb R^{m\times n}$, $B\in\mathbb R^{n\times p}$, applied to equations (2.22) and (2.23). Percentages refer to the total layer cost.

| Component | Parameters | FLOP formula | MFLOPs | % |
|---|---:|---|---:|---:|
| *Attention sub-layer (Tab. 19.1)* | | | | |
| QKV proj. ($3H$ matrices) | 1,769,472 | $6Ld^2$ | 453.0 | 24.3 |
| Score $QK^\top$ | — | $2L^2d$ | 25.2 | 1.4 |
| Scaling + softmax | — | $5L^2H$ | 1.0 | 0.0 |
| Weighted sum $AV$ | — | $2L^2d$ | 25.2 | 1.4 |
| Output proj. $W_O$ | 589,824 | $2Ld^2$ | 151.0 | 8.1 |
| **MHA total** | **2,362,368** | **(2.22)** | **655.4** | **35.1** |
| *Feed-forward sub-layer (Tab. 2.7)* | | | | |
| Expansion $W_1$ | 2,359,296 | $8Ld^2$ | 604.0 | 32.4 |
| Activation $\varphi$ | — | $\sim4Ld$ | 0.4 | 0.0 |
| Projection $W_2$ | 2,359,296 | $8Ld^2$ | 604.0 | 32.4 |
| **FFN total** | **4,722,432** | **(2.23)** | **1,208.4** | **64.8** |
| Add + Norm ($\times2$) | 3,072 | $18Ld$ | 1.8 | 0.1 |
| **Layer total** | **7,087,872** | **(2.23)** | **1,865.5** | **100** |

The FFN uses $2\times$ more parameters and $1.85\times$ more FLOPs than MHA. QKV projections account for 24.3% of the total layer cost on their own.

<a id="section-2-5-1"></a>
#### 2.5.1 Multi-head attention

Fix $L$ (sequence length), $h$ (number of heads), $d=d_{\mathrm{model}}$ (model dimension), and $d_k=d/h$ (per-head key dimension). The MHA computation has six stages.

**1. QKV projections.** Each head computes $Q_i=XW_Q^{(i)}$, $K_i=XW_K^{(i)}$, $V_i=XW_V^{(i)}$ with $W_*^{(i)}\in\mathbb R^{d\times d_k}$. Each projection costs $2Ldd_k$ FLOPs. With three projections and $h$ heads, and using $hd_k=d$:

$$
\mathrm{FLOPs}_{\mathrm{proj}}=3h\cdot2Ldd_k=6Ld^2.
$$

**2. Attention scores $Q_iK_i^\top$.** Each head produces $Q_iK_i^\top\in\mathbb R^{L\times L}$ with cost $2L^2d_k$. Summed over $h$ heads:

$$
\mathrm{FLOPs}_{QK^\top}=2hL^2d_k=2L^2d.
$$

**3. Scaling by $1/\sqrt{d_k}$.** One scalar multiplication per entry: $hL^2=L^2h$ FLOPs.

<a id="pdf-0099"></a>
## PDF 99 · Book 63

**4. Row-wise softmax.** For each of the $L$ rows of each head’s score matrix, computing softmax requires: max-finding ($L-1$), subtraction ($L$), exponentials ($L$), sum ($L-1$), and division ($L$), giving $5L-2$ FLOPs per row. With $L$ rows and $h$ heads:

$$
\mathrm{FLOPs}_{\mathrm{softmax}}=hL(5L-2)=5L^2h-2Lh.
$$

**5. Attention output $A_iV_i$.** Each $A_i\in\mathbb R^{L\times L}$ multiplied by $V_i\in\mathbb R^{L\times d_k}$ costs $2L^2d_k$ per head:

$$
\mathrm{FLOPs}_{AV}=2hL^2d_k=2L^2d.
$$

**6. Output projection $W_O$.** The concatenation $[Y^{(1)}|\cdots|Y^{(h)}]\in\mathbb R^{L\times d}$ projected by $W_O\in\mathbb R^{d\times d}$ costs:

$$
\mathrm{FLOPs}_{\mathrm{out}}=2Ld^2.
$$

**Total MHA.** Summing the six terms:

<a id="equation-2-22"></a>
$$
\mathrm{FLOPs}_{\mathrm{MHA}}=8Ld^2+4L^2d+6L^2h-2Lh.\tag{2.22}
$$

The dominant regime depends on sequence length: for short sequences ($L\ll d$) the cost is $O(Ld^2)$; for long sequences ($L\gg d$) it is $O(L^2d)$, reflecting the quadratic cost of the score matrix computation.

<a id="section-2-5-2"></a>
#### 2.5.2 Feed-forward network and layer normalisation

**FFN.** With $d_{\mathrm{ff}}=4d$ and using ReLU (one comparison per activation): step (a) $XW_1$ costs $2Ldd_{\mathrm{ff}}=8Ld^2$; step (b) ReLU costs $Ld_{\mathrm{ff}}=4Ld$; step (c) $\mathrm{ReLU}(\cdot)W_2$ costs $2Ld_{\mathrm{ff}}d=8Ld^2$:

$$
\mathrm{FLOPs}_{\mathrm{FFN}}=16Ld^2+4Ld.
$$

**Layer normalisation.** For each token vector $x\in\mathbb R^d$: mean ($d$ ops), variance ($3d$ ops), normalisation ($2d+2$ ops), affine transformation ($2d$ ops) — total $8d+2$ FLOPs per token. For $L$ tokens and two LN applications per block, plus two residual additions:

$$
\mathrm{FLOPs}_{\mathrm{Add+Norm}}\approx18Ld.
$$

<a id="section-2-5-3"></a>
#### 2.5.3 Total cost per layer and for $\mathrm{BERT}$

Combining the three components, one Transformer layer costs:

<a id="equation-2-23"></a>
$$
\mathrm{FLOPs}_{\mathrm{layer}}
=\underbrace{8Ld^2+4L^2d+6L^2h-2Lh}_{\mathrm{MHA}}
+\underbrace{16Ld^2+4Ld}_{\mathrm{FFN}}
+\underbrace{18Ld}_{\mathrm{Add+Norm}}.\tag{2.23}
$$

Grouping by dominant terms:

$$
\mathrm{FLOPs}_{\mathrm{layer}}
=24Ld^2+4L^2d+(6L^2h-2Lh)+22Ld+4L.
$$

For an $N$-layer encoder, total inference FLOPs are:

<a id="equation-2-24"></a>
$$
\mathrm{FLOPs}_{\mathrm{BERT}}
\approx N\cdot\bigl(24Ld^2+4L^2d+6L^2h-2Lh+22Ld+4L\bigr),\tag{2.24}
$$

where embedding ($Ld$) and classification head ($2Cd_{\mathrm{model}}+5C$) contribute less than $0.01\%$ of the total. For $\mathrm{BERT}_{\mathrm{base}}$ at $L=128$:

$$
\mathrm{FLOPs}_{\mathrm{BERT\text{-}base}}\approx2.239\times10^{10}=\mathbf{22.39}\ \mathrm{GFLOPs}.
$$

<a id="pdf-0100"></a>
## PDF 100 · Book 64

<a id="table-2-9"></a>
**Table 2.9:** Inference FLOPs for $\mathrm{BERT}_{\mathrm{base}}$ and $\mathrm{BERT}_{\mathrm{large}}$ at sequence length $L=128$, computed from the exact formula (2.23) including Add + Norm. The encoder stack accounts for $>99.99\%$ of the total; embedding and classification head are negligible.

| Model | Layers $N$ | $d$ | Heads $h$ | GFLOPs (forward) |
|---|---:|---:|---:|---:|
| $\mathrm{BERT}_{\mathrm{base}}$ | 12 | 768 | 12 | $\approx22.39$ |
| $\mathrm{BERT}_{\mathrm{large}}$ | 24 | 1024 | 16 | $\approx79.03$ |

The breakdown per layer is: MHA 35.1%, FFN 64.8%, Add+Norm 0.1%. Note that figures of $\approx21.8$ GFLOPs appearing in some analyses reflect calculations that omit the Add+Norm contribution, which is small ($\approx0.6\%$ of the total) but nonzero.

Training costs approximately three times the forward pass: $\mathrm{FLOPs}_{\mathrm{train}}\approx3\,\mathrm{FLOPs}_{\mathrm{forward}}$, where the factor accounts for the backward pass ($\approx1\times$) and the optimizer step (Adam: $\approx0.5\times$ forward).

<a id="remark-2-28"></a>
**Remark 2.28 (Computational implications of head redundancy).** Equation (2.22) makes the cost of redundancy concrete. Pruning a single attention head removes not just its softmax computation but all of its per-head operations: the three QKV projections, the score matrix $Q_iK_i^\top$, the weighted sum $A_iV_i$, and the corresponding block of the output projection $W_O$. For a head of $\mathrm{BERT}_{\mathrm{base}}$ ($d=768$, $d_k=64$, $L=128$), the per-head cost (under the standard $2mnp$ convention) is:

$$
3\cdot2Ldd_k+2L^2d_k+L^2+L(5L-2)+2L^2d_k+2Ld_kd
\approx5.46\times10^7\ \mathrm{FLOPs}.
$$

If 50% of the $H=12$ heads can be pruned without loss of performance — as Voita et al. (2019b) demonstrate — the saving over the full 12-layer encoder is approximately $\mathbf{3.93}$ GFLOPs, or $\mathbf{17.6\%}$ of the total encoder cost. The theory of Part II will characterise precisely *which* heads are redundant and *why*: the Channel Destruction Theorem (Chapter ??) and the Pattern-Emergence Theorem (Chapter 10) together imply that redundant heads are not an accident of training, but a structural consequence of the architecture’s functional capacity.

> Editorial note: The arithmetic above uses the book’s operation-count model, not a hardware timing measurement. The cost discussion contains inconsistent descriptions of a FLOP, scaling-plus-softmax, the $4L$ normalisation term, Add+Norm percentages and the training multiplier. Original expressions and values are retained; [the computational-cost note](../notes/14-computational-cost.md#source-issues) gives a separate reconciliation.

<a id="section-2-6"></a>
### 2.6 The Structure of $M$

We have defined the score matrix $L=X_{\mathrm{pos}}MX_{\mathrm{pos}}^\top/\sqrt{d_k}$ and identified it as the central object of analysis. The properties of $L$ — and hence everything an attention head can compute — are determined by the properties of $M=W_QW_K^\top$. This section examines those properties directly.

<a id="section-2-6-1"></a>
#### 2.6.1 $M$ as the product of two distinct learnable matrices

The matrix $M=W_QW_K^\top$ is not the square of a single matrix; it is the product of two independently parameterised rectangles. This matters because it removes every structural constraint that would hold if the two factors were related. If $W_Q=W_K$, then $M=W_QW_Q^\top$ would be positive semidefinite and symmetric: the score $L_{ij}=x_iMx_j^\top/\sqrt{d_k}$ would be a genuine inner product on $\mathbb R^d$, and the attention mechanism would measure similarity in the metric sense. But $W_Q$ and $W_K$ are learned independently; the trained model has no reason to align them, and in practice it does not.

> Editorial note: A positive semidefinite form can be degenerate. A genuine inner product on all of $\mathbb R^d$ additionally requires positive definiteness; for $d_k<d$, $W_QW_Q^\top$ necessarily has a nontrivial kernel. Also, $M$ alone specifies score formation, whereas the head’s full output also depends on the writing map $N$ defined earlier.

<a id="remark-2-29"></a>
**Remark 2.29 (The limit case $W_Q=W_K$: kernel attention).** When $W_Q=W_K=:W$, the

> Editorial boundary: The supplied page ends here, mid-sentence and inside Remark 2.29. Batch 003 preserves the intervening [Figure 2.11 on PDF 101](0101-0108-ch02-kernel-rank-gauge-and-asymmetry.md#pdf-0101) and the [continuation of the sentence on PDF 102](0101-0108-ch02-kernel-rank-gauge-and-asymmetry.md#pdf-0102). Remark 2.29 is now complete across these files; this source file retains the original PDF 100 boundary.
