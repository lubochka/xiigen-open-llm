---
title: "Chapter 5: refined edge bound, permutation equivariance, and stability"
source_pdf_pages: [172, 178]
book_pages: [136, 142]
batch: "004"
language: en
type: source-transcription
status: complete-for-page-range
continues_after_file: false
---

# Chapter 5: refined edge bound, permutation equivariance, and stability

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous pages: PDF 165–171](0165-0171-ch05-indices-geometry-and-experiments.md) · [Next chapter: PDF 179](0179-0186-ch06-notation-and-prior-taxonomies.md#pdf-0179)

> **Editorial scope.** Full English transcription of PDF 172–178 / printed pages 136–142, completing Chapter 5. The opening sentence continues PDF 171. All stated constants, the $\varepsilon$ qualification in the proof, bibliographic placeholders, and author corrections to earlier drafts are retained. Mathematical qualifications are separate in [note 34](../notes/34-refined-edge-and-logsumexp-majorant.md), [note 35](../notes/35-permutation-equivariance-and-order.md), and [note 36](../notes/36-attention-forward-and-loss-stability.md). In this chapter, $A$ denotes the antisymmetric score component and $P$ denotes row-softmax attention.

<a id="pdf-0172"></a>
## PDF 172 · Book 136

the present chapter is theoretically self-contained and we do not duplicate the empirical study here.

<a id="section-5-8"></a>
### 5.8 Concluding Remarks

We close with three results. First, a corrected refinement of the edge bound (resolving OP3). Second, a clean separation between two facts that the chapter takes care not to conflate: the permutation equivariance of a transformer in the absence of order-breaking information (Proposition 5.15), and the bidirectional stability of the forward computation under removal of the antisymmetric component of the score matrices (Theorem 5.20). Third, the genuinely open dynamical problem (OP1) which is the subject of a later chapter.

A remark on what is *not* claimed below is in order, since an earlier draft of this chapter overreached on this point. There is no result of the form “a transformer with $\delta_h=0$ on every head is permutation-equivariant”. The implication is in the wrong direction: $\delta_h=0$ for every head is a special case of the equivariance hypothesis, but it is not its content. Permutation equivariance follows from absence of positional encoding, absence of causal masking, and weight sharing across token positions — all of which hold regardless of whether $L$ is symmetric or not. Consequently there is no “symmetry obstruction” on the loss that is unlocked specifically by $\sum_h\delta_h>0$: the obstruction is unlocked by positional information, which is an architectural choice, not an algebraic property of $M$. The bidirectional stability theorem below is the correct algebraic statement on the role of $\sum_h\delta_h$: it bounds, in both directions, the perturbation induced on the forward computation by removing the antisymmetric component of the logits, without invoking any spurious lower bound against a permutation-invariant baseline.

<a id="op3-refined-edge"></a>
**OP3: refinement of the edge bound (resolved).** The Lipschitz constant of LSE in $\ell^\infty$ used in the proof of Proposition 5.5 is the worst-case constant. A sharper bound is obtained by exploiting the fact that the gradient of LSE at any point is a probability distribution. The refinement is best stated with an unnormalised majorant; the version with a normalised probability requires a partition-function correction, as we make explicit at the end.

<a id="proposition-5-11"></a>
**Proposition 5.11 (Refined edge persistence).** Let $L=S+A$ be as in Proposition 5.5, fix an ordered pair $(i,j)$ with $i\ne j$, and write $L_i,L_j$ for the corresponding rows. For $t\in[0,1]$, let $p^{(t)}:=\operatorname{softmax}((1-t)L_j+tL_i)\in\Delta^{n-1}$, the row-softmax of the linear interpolant. Then

<a id="equation-5-10"></a>
$$
\log\frac{P_{ij}}{P_{ji}}
=2A_{ij}-\int_0^1\langle p^{(t)},L_i-L_j\rangle\,dt. \tag{5.10}
$$

Consequently, for any $r\in\mathbb R_+^n$ (not required to be a probability distribution) satisfying

<a id="equation-5-11"></a>
$$
r_k\ge p_k^{(t)}
\qquad\text{for all }k\in\{1,\ldots,n\}\text{ and all }t\in[0,1], \tag{5.11}
$$

one has

<a id="equation-5-12"></a>
$$
\log\frac{P_{ij}}{P_{ji}}\ge 2A_{ij}-\langle r,|L_i-L_j|\rangle. \tag{5.12}
$$

In particular, choosing $r_k=\sup_{t\in[0,1]}p_k^{(t)}$ (the pointwise sup, which trivially satisfies (5.11)) yields the tightest such bound,

<a id="equation-5-13"></a>
$$
\log\frac{P_{ij}}{P_{ji}}
\ge 2A_{ij}-\sum_{k=1}^n
\left(\sup_{t\in[0,1]}p_k^{(t)}\right)|L_{ik}-L_{jk}|. \tag{5.13}
$$

<a id="pdf-0173"></a>
## PDF 173 · Book 137

*Proof.* By the fundamental theorem of calculus applied to the smooth function $\phi(t)=\operatorname{LSE}((1-t)L_j+tL_i)$,

$$
\operatorname{LSE}(L_i)-\operatorname{LSE}(L_j)
=\int_0^1\phi'(t)\,dt
=\int_0^1
\left\langle\nabla\operatorname{LSE}((1-t)L_j+tL_i),L_i-L_j\right\rangle\,dt.
$$

Since $\nabla\operatorname{LSE}(u)=\operatorname{softmax}(u)=p^{(t)}$, this gives

$$
\operatorname{LSE}(L_i)-\operatorname{LSE}(L_j)
=\int_0^1\langle p^{(t)},L_i-L_j\rangle\,dt.
$$

Combined with the identity $L_{ij}-L_{ji}=2A_{ij}$ and the expression $\log(P_{ij}/P_{ji})=(L_{ij}-L_{ji})-(\operatorname{LSE}(L_i)-\operatorname{LSE}(L_j))$ (used already in the proof of Proposition 5.5), (5.10) follows. For (5.12), observe that for every $k$ and every $t$,

$$
p_k^{(t)}(L_{ik}-L_{jk})
\le p_k^{(t)}|L_{ik}-L_{jk}|
\le r_k|L_{ik}-L_{jk}|,
$$

where the last step uses (5.11). Summing over $k$ and integrating over $t\in[0,1]$ gives $\int_0^1\langle p^{(t)},L_i-L_j\rangle\,dt\le\langle r,|L_i-L_j|\rangle$, and substituting in (5.10) yields (5.12). The specialisation (5.13) follows from the choice $r_k=\sup_t p_k^{(t)}$, which by construction satisfies $r_k\ge p_k^{(t)}$ for all $t$. $\square$

<a id="remark-5-12"></a>
**Remark 5.12 (Why $r$ is unnormalised).** A previous formulation of this proposition required $q\in\Delta^{n-1}$ (a probability distribution) satisfying $q\ge p^{(t)}$ entrywise for all $t$. That hypothesis is degenerate: if $q$ and $p^{(t)}$ are both probability distributions and $q_k\ge p_k^{(t)}$ for every $k$, then $\sum_kq_k=\sum_kp_k^{(t)}=1$ forces $q=p^{(t)}$ at every $t$, which is possible only when $p^{(t)}$ is constant in $t$. The corrected formulation drops the normalisation: $r$ is a pointwise upper envelope of the family $\{p^{(t)}\}_{t\in[0,1]}$, with $\sum_kr_k$ generally $>1$, and the bound retains the form (5.12) without extra factors. If a normalised probability $q:=r/Z$ with $Z=\sum_kr_k$ is preferred, the bound reads

$$
\log\frac{P_{ij}}{P_{ji}}\ge 2A_{ij}-Z\langle q,|L_i-L_j|\rangle,
$$

i.e. the partition factor $Z$ cannot be absorbed.

<a id="remark-5-13"></a>
**Remark 5.13 (Relation with the $\ell^\infty$ bound).** Proposition 5.11 does not literally recover Proposition 5.5 by choosing a majorant supported on a single coordinate. Setting $r_{k^*}=1$ and $r_k=0$ for $k\ne k^*$ would fail the condition $r_k\ge p_k^{(t)}$ at every $k\ne k^*$ and every $t$, since softmax probabilities are strictly positive ($p_k^{(t)}>0$ for all $k$ and $t$). The original $\ell^\infty$ bound of Proposition 5.5 follows instead from its own proof, namely the Lipschitz estimate $|\operatorname{LSE}(L_i)-\operatorname{LSE}(L_j)|\le\|L_i-L_j\|_\infty$, and is not derived from Proposition 5.11. The two results are complementary: Proposition 5.5 is a worst-case parameter-level bound, while Proposition 5.11 is a path-dependent refinement that may be sharper when the path distributions $p^{(t)}$ place little mass on coordinates with large $|L_{ik}-L_{jk}|$.

<a id="remark-5-14"></a>
**Remark 5.14 (Valid choices of majorant).** A universally valid choice is the sup-pointwise majorant

<a id="equation-5-14"></a>
$$
r_k=\sup_{t\in[0,1]}p_k^{(t)}, \tag{5.14}
$$

which trivially satisfies (5.11) and yields the sharpest pointwise bound (5.13). Computing $r_k$ exactly requires a one-dimensional optimisation per coordinate; in practice it is evaluated by sampling $t$ on a grid in $[0,1]$ and taking the maximum over the grid (with a numerical safety factor if needed).

<a id="pdf-0174"></a>
## PDF 174 · Book 138

A tempting shortcut — the endpoint expression $r_k=\max(P_{ik},P_{jk})$ — is *not* a valid majorant in general, because a softmax coordinate along the linear interpolant $(1-t)L_j+tL_i$ can attain its maximum at an interior $t\in(0,1)$ rather than at an endpoint. A simple counterexample with three coordinates: take $L_j=(0,10,-10)$ and $L_i=(0,-10,10)$. The first coordinate has $P_{j1}\approx P_{i1}\approx e^{-10}$ at both endpoints, but at $t=\frac12$ the interpolant is the zero vector and $p_1^{(1/2)}=\frac13$, four orders of magnitude above $\max(P_{i1},P_{j1})$. The endpoint vector therefore violates $r_k\ge p_k^{(t)}$ massively on this example; it cannot be substituted for the sup-pointwise choice.

**Caveat.** *The refined bound (5.12) is path-dependent and not expressible in terms of parameters alone: it requires evaluating $P$ (or at least a valid majorant $r$ of the path-distribution $p^{(t)}$, e.g. the sup-pointwise choice (5.14)). For pure parameter-level verification, Proposition 5.5 remains the relevant result; the refinement is the natural complement when one has access to the attention matrix on the input of interest, and is sharper than Proposition 5.5 whenever the path distributions $p^{(t)}$ place little mass on the coordinate $k^*=\arg\max_k|L_{ik}-L_{jk}|$.*

<a id="equivariance-without-position"></a>
**Permutation equivariance without positional information.** Before stating the stability theorem, we record the correct equivariance statement, and explain why it is independent of the asymmetry budget $\sum_h\delta_h$.

<a id="proposition-5-15"></a>
**Proposition 5.15 (Permutation equivariance without positional information).** Let $T$ be a transformer block consisting of: multi-head self-attention with weight matrices shared across token positions, no positional encoding, no causal mask; a token-wise LayerNorm; a position-wise feed-forward network with weights shared across token positions; and additive residual connections. Then $T$ is equivariant under token permutations: for every permutation matrix $\Pi\in\mathbb R^{n\times n}$,

<a id="equation-5-15"></a>
$$
T(\Pi X)=\Pi T(X). \tag{5.15}
$$

Composition of such blocks preserves the property. In particular, if the classification head is itself permutation-invariant (e.g. a token-wise projection followed by a permutation-invariant pooling), then the overall expected cross-entropy loss is bounded below by

<a id="equation-5-16"></a>
$$
\mathbb E_X[\mathcal L_{\mathrm{task}}(T(X))]
\ge \mathcal L_{\mathrm{sym}}
:=\mathbb E_X[H(Y\mid\operatorname{Sym}(X))], \tag{5.16}
$$

where $\operatorname{Sym}(X)$ is the orbit of $X$ under the symmetric group acting by token permutation.

*Proof.* Consider a single head with parameters $W_Q,W_K,W_V,W_O$ shared across positions, so that $M=W_QW_K^\top$. Under $X\mapsto\Pi X$, the score matrix transforms as

$$
L=XMX^\top\mapsto(\Pi X)M(\Pi X)^\top=\Pi L\Pi^\top,
$$

because the parameters of $M$ do not depend on position and therefore are invariant under the permutation. The row-wise softmax commutes with simultaneous row-and-column permutation: for every $L\in\mathbb R^{n\times n}$ and every permutation matrix $\Pi$,

$$
\operatorname{softmax}_{\mathrm{row}}(\Pi L\Pi^\top)
=\Pi\operatorname{softmax}_{\mathrm{row}}(L)\Pi^\top.
$$

Indeed, the $i$-th row of $\Pi L\Pi^\top$ is the $\pi^{-1}(i)$-th row of $L$ permuted in its column indices by $\Pi^\top$, and the softmax depends only on the multiset of entries within a row; hence each row of $\operatorname{softmax}_{\mathrm{row}}(\Pi L\Pi^\top)$ is the permutation-by-$\Pi^\top$ of the $\pi^{-1}(i)$-th row of $\operatorname{softmax}_{\mathrm{row}}(L)$, which is exactly the $i$-th row of $\Pi\operatorname{softmax}_{\mathrm{row}}(L)\Pi^\top$. The value matrix transforms as $V=XW_V\mapsto\Pi XW_V=\Pi V$. Therefore the attention output transforms as

$$PV\mapsto(\Pi P\Pi^\top)(\Pi V)=\Pi PV.$$

<a id="pdf-0175"></a>
## PDF 175 · Book 139

Concatenation of head outputs along the feature dimension and multiplication by the (position-shared) output projection $W_O$ preserves equivariance, as does additive residual connection, token-wise LayerNorm, and the position-wise FFN. This proves (5.15) for a single block; composition of equivariant maps is equivariant, so the property is preserved across the stack. Equation (5.16) follows from the data-processing inequality: any permutation-invariant predictor of $Y$ factors through the orbit $\operatorname{Sym}(X)$, and its conditional entropy is bounded below by $H(Y\mid\operatorname{Sym}(X))$. $\square$

<a id="remark-5-16"></a>
**Remark 5.16 (The role of $\delta_h$ in the equivariance argument).** The proof of Proposition 5.15 does not use $L=L^\top$ at any step. In particular, the equivariance identity $\operatorname{softmax}_{\mathrm{row}}(\Pi L\Pi^\top)=\Pi\operatorname{softmax}_{\mathrm{row}}(L)\Pi^\top$ holds for arbitrary $L$ and is independent of the symmetric/antisymmetric decomposition. The previous draft of this chapter contained a result that asserted, in the same form, that $\delta_h^{(\ell,h)}=0$ on every head implies permutation equivariance; the implication is true but trivial (it follows from the architectural hypotheses alone, with no appeal to symmetry of $L$), and it conflates two distinct facts. We record this explicitly to avoid the mistake reappearing downstream: permutation equivariance, and hence the lower bound (5.16), holds for the unmodified transformer with $\sum_h\delta_h>0$ as well, so this lower bound is not unlocked by the asymmetry budget. The correct algebraic statement on the role of $\sum_h\delta_h$ is the stability theorem below.

<a id="remark-5-17"></a>
**Remark 5.17 (Common mistake avoided).** A symmetric score matrix $L=L^\top$ does *not* produce a symmetric attention matrix $P=\operatorname{softmax}_{\mathrm{row}}(L)$: the row-wise softmax depends on the row partition functions, which differ across rows. A two-by-two example suffices: $L=\begin{pmatrix}0&1\\1&2\end{pmatrix}$ is symmetric, with $P_{12}=e/(1+e)$ and $P_{21}=1/(1+e)$, so $P_{12}\ne P_{21}$. The correct invariance property of $P$ is the equivariance identity stated in the proof above, which holds for all $L$ and uses only that the softmax is row-wise.

<a id="antisymmetric-removal-perturbation"></a>
**Quantitative perturbation under antisymmetric removal.** We turn to the role of $\sum_h\delta_h$ in the forward computation. The relevant statement is a bidirectional stability bound: removing the antisymmetric component of every score matrix in the network perturbs the output by a quantity controlled, up and down, by $\sum_{\ell,h}\delta_h^{(\ell,h)}\beta_h^{(\ell,h)}$. We first record the standard $\ell^2$-Lipschitz constant of the softmax, then the per-head perturbation lemma, then the main stability theorem.

<a id="lemma-5-18"></a>
**Lemma 5.18 (Softmax is $\frac12$-Lipschitz in $\ell^2$; Gao and Pavel, 2017).** For every $u,v\in\mathbb R^n$, $\|\operatorname{softmax}(u)-\operatorname{softmax}(v)\|_2\le\frac12\|u-v\|_2$. The constant is sharp.

*Proof sketch.* The Jacobian of softmax at $u$ is $J(u)=\operatorname{diag}(p)-pp^\top$ with $p=\operatorname{softmax}(u)$. The matrix $J(u)$ is symmetric positive semidefinite with $J(u)\mathbf1=0$, and a direct calculation shows its largest eigenvalue is bounded by $\frac12$, with equality at $n=2,p=(\frac12,\frac12)$. The Lipschitz bound follows from the mean value inequality. $\square$

<a id="proposition-5-19"></a>
**Proposition 5.19 (Per-head representation perturbation).** Let a single attention head have score matrix $L=S+A$ with $S$ symmetric and $A$ antisymmetric, value matrix $V$, and define $P=\operatorname{softmax}_{\mathrm{row}}(L)$ and $P^s=\operatorname{softmax}_{\mathrm{row}}(S)$. Then

<a id="equation-5-17"></a>
$$
\|(P-P^s)V\|_F
\le\frac12\|A\|_F\|V\|_{\mathrm{op}}
=\frac14\|L-L^\top\|_F\|V\|_{\mathrm{op}}. \tag{5.17}
$$

<a id="pdf-0176"></a>
## PDF 176 · Book 140

*Proof.* By Lemma 5.18 applied row-by-row,

$$
\begin{aligned}
\|P-P^s\|_F^2
&=\sum_i\|\operatorname{softmax}(L_i)-\operatorname{softmax}(S_i)\|_2^2\\
&\le\frac14\sum_i\|L_i-S_i\|_2^2
=\frac14\|L-S\|_F^2
=\frac14\|A\|_F^2,
\end{aligned}
$$

since $L-S=A$. Hence $\|P-P^s\|_F\le\frac12\|A\|_F$. The bound on $(P-P^s)V$ follows from the elementary inequality $\|MV\|_F\le\|M\|_F\|V\|_{\mathrm{op}}$. The final identity uses $\|A\|_F=\frac12\|L-L^\top\|_F$, which holds because $A=(L-L^\top)/2$. $\square$

<a id="theorem-5-20"></a>
**Theorem 5.20 (Antisymmetric-removal stability).** Let $T$ be a transformer with $L$ layers and $H$ heads per layer. Let $T^{\mathrm{sym}}$ be the model obtained by replacing, at every layer $\ell$ and every head $h$, the score matrix $L^{(\ell,h)}$ by its symmetric part $S^{(\ell,h)}=(L^{(\ell,h)}+(L^{(\ell,h)})^\top)/2$, with all other parameters held fixed. Assume each transformer block (multi-head attention with output projection, plus FFN, plus residual, plus LayerNorm) is Lipschitz in Frobenius norm with constant at most $K$ on the relevant compact region of activation space; the classification head is Lipschitz with constant $K_c$; and the loss $\mathcal L_{\mathrm{task}}$ is Lipschitz in the logits with constant $K_{\mathrm{CE}}$. Define the per-head budget

<a id="equation-5-18"></a>
$$
\beta_h^{(\ell,h)}(X)
:=
\|L^{(\ell,h)}\|_F\cdot\|V^{(\ell,h)}\|_{\mathrm{op}}, \tag{5.18}
$$

where $V^{(\ell,h)}$ is the value matrix of head $h$ at layer $\ell$ on input $X$. Then, for every input $X$,

<a id="equation-5-19"></a>
$$
\left|\mathcal L_{\mathrm{task}}(T(X))
-\mathcal L_{\mathrm{task}}(T^{\mathrm{sym}}(X))\right|
\le C_{\mathrm{stab}}\sum_{\ell,h}
\delta_h^{(\ell,h)}(X)\,\beta_h^{(\ell,h)}(X), \tag{5.19}
$$

with the explicit constant

<a id="equation-5-20"></a>
$$
C_{\mathrm{stab}}
:=
\frac{K_cK_{\mathrm{CE}}\max(1,K^{L-1})}{4}. \tag{5.20}
$$

Equivalently, both $\mathcal L_{\mathrm{task}}(T(X))\le\mathcal L_{\mathrm{task}}(T^{\mathrm{sym}}(X))+C_{\mathrm{stab}}\sum_{\ell,h}\delta_h^{(\ell,h)}\beta_h^{(\ell,h)}$ and the matching lower bound hold pointwise; expectations over $X$ are obtained by integrating both inequalities.

*Proof. Step 1: per-layer perturbation.* Within layer $\ell$, the multi-head attention block applies the heads in parallel, concatenates their outputs, and applies the output projection $W_O^{(\ell)}$. By the triangle inequality and Proposition 5.19, the multi-head attention outputs of the unperturbed and symmetrised models, evaluated on the same input $X^{(\ell)}$, differ by

$$
\left\|\mathrm{MH}^{(\ell)}(X^{(\ell)})
-\mathrm{MH}^{(\ell),\mathrm{sym}}(X^{(\ell)})\right\|_F
\le\frac12\|W_O^{(\ell)}\|_{\mathrm{op}}
\sum_h\|A^{(\ell,h)}\|_F\|V^{(\ell,h)}\|_{\mathrm{op}}.
$$

Using $\|A^{(\ell,h)}\|_F=\frac12\|L^{(\ell,h)}-(L^{(\ell,h)})^\top\|_F=\frac12\delta_h^{(\ell,h)}\|L^{(\ell,h)}\|_F$ (by Definition 5.9, modulo the $\varepsilon$ regulariser), this gives

$$
\|\mathrm{MH}^{(\ell)}-\mathrm{MH}^{(\ell),\mathrm{sym}}\|_F
\le\frac14\|W_O^{(\ell)}\|_{\mathrm{op}}
\sum_h\delta_h^{(\ell,h)}\beta_h^{(\ell,h)}.
$$

Absorbing $\|W_O^{(\ell)}\|_{\mathrm{op}}$ into the per-block Lipschitz constant $K$ (since $K$ already controls the operator norm of the output projection and the rest of the block), the bound reads

<a id="equation-5-21"></a>
$$
\|\mathrm{MH}^{(\ell)}-\mathrm{MH}^{(\ell),\mathrm{sym}}\|_F
\le\frac14\sum_h\delta_h^{(\ell,h)}\beta_h^{(\ell,h)}. \tag{5.21}
$$

<a id="pdf-0177"></a>
## PDF 177 · Book 141

*Step 2: propagation across layers.* Let $X^{(\ell)},X^{(\ell),\mathrm{sym}}$ denote the activations at the input of layer $\ell+1$ in the two models, with $X^{(0)}=X^{(0),\mathrm{sym}}=X$. Layer $\ell$ applies a Lipschitz function (with constant $K$) to its input, except for the multi-head attention sub-block, whose perturbation is bounded by (5.21). The combined recursion is

$$
\|X^{(\ell)}-X^{(\ell),\mathrm{sym}}\|_F
\le K\|X^{(\ell-1)}-X^{(\ell-1),\mathrm{sym}}\|_F
+\frac14\sum_h\delta_h^{(\ell,h)}\beta_h^{(\ell,h)}.
$$

Iterating from $\ell=1$ to $\ell=L$ with the initial condition $X^{(0)}=X^{(0),\mathrm{sym}}$ yields the telescoping bound

$$
\|X^{(L)}-X^{(L),\mathrm{sym}}\|_F
\le\frac14\sum_{\ell=1}^L K^{L-\ell}
\sum_{h=1}^H\delta_h^{(\ell,h)}\beta_h^{(\ell,h)}.
$$

The largest factor is $K^{L-1}$, attained at $\ell=1$. If $K\ge1$, the maximum across $\ell$ of $K^{L-\ell}$ is $K^{L-1}$. If $K<1$, the maximum is 1 (attained at $\ell=L$). In either case, $K^{L-\ell}\le\max(1,K^{L-1})$, so

<a id="equation-5-22"></a>
$$
\|X^{(L)}-X^{(L),\mathrm{sym}}\|_F
\le\frac{\max(1,K^{L-1})}{4}
\sum_{\ell,h}\delta_h^{(\ell,h)}\beta_h^{(\ell,h)}. \tag{5.22}
$$

*Step 3: classification head and loss.* Lipschitz of the classification head and of the loss in the logits gives

$$
\left|\mathcal L_{\mathrm{task}}(T(X))
-\mathcal L_{\mathrm{task}}(T^{\mathrm{sym}}(X))\right|
\le K_cK_{\mathrm{CE}}\|X^{(L)}-X^{(L),\mathrm{sym}}\|_F.
$$

Combining with (5.22) yields (5.19) with the constant (5.20). $\square$

<a id="remark-5-21"></a>
**Remark 5.21 (What the stability theorem says, and what it does not).** Theorem 5.20 is a pointwise, bidirectional bound: the loss of $T$ and the loss of $T^{\mathrm{sym}}$ differ by at most $C_{\mathrm{stab}}\sum_{\ell,h}\delta_h^{(\ell,h)}\beta_h^{(\ell,h)}$, in either direction. In particular:

- It does *not* say that $T$ achieves a strictly lower loss than $T^{\mathrm{sym}}$. Whether $T$ improves on $T^{\mathrm{sym}}$, or worsens it, depends on the trained parameters; the stability bound is symmetric and silent on the sign of the improvement.
- It does *not* produce a lower bound of the form “$\mathbb E[\mathcal L_{\mathrm{task}}(T(X))]\ge\mathcal L_{\mathrm{sym}}-C\sum_{\ell,h}\delta_h\beta_h$” against a permutation-invariant baseline. Such a bound would require $T^{\mathrm{sym}}$ to be permutation-equivariant, but $T^{\mathrm{sym}}$ is permutation-equivariant exactly when the architecture is permutation-equivariant (Proposition 5.15), in which case so is $T$ itself, and the bound on $T$ already follows without invoking $T^{\mathrm{sym}}$. There is therefore no algebra-to-loss lower bound unlocked specifically by the asymmetry budget; what the asymmetry budget unlocks is forward-pass perturbation, both up and down, of the magnitude given by (5.19).

The combination of Proposition 5.15 and Theorem 5.20 is the correct factoring of the two phenomena that the previous draft conflated.

<a id="remark-5-22"></a>
**Remark 5.22 (Tightness of the constants).** The constants $K,K_c,K_{\mathrm{CE}}$ are computable from the trained model: $K$ from operator norms of weight matrices and Lipschitz properties of LayerNorm and the FFN, $K_c$ from the classification head, $K_{\mathrm{CE}}$ from a uniform bound on the logits over the data distribution. The factor $\frac14$ on the softmax side is sharp (Lemma 5.18). The block Lipschitz constant $K$ of a typical trained transformer is bounded but greater than one; Kim et al. (2021) estimate $K\approx1.05$ to $1.2$ for BERT-base, giving $K^{L-1}\approx K^{11}\approx1.7$ to $7.4$ for BERT-base ($L=12$). The bound is non-vacuous in practice for moderate depths.

<a id="pdf-0178"></a>
## PDF 178 · Book 142

**Caveat.** *The bound (5.19) is bidirectional: removing the antisymmetric component cannot perturb the loss by more than $C_{\mathrm{stab}}\sum_{\ell,h}\delta_h\beta_h$ in absolute value. It does not certify that the antisymmetric component lowers the loss; nor does it certify that the symmetrised model achieves any specific performance level. It is a stability statement about the forward map, not an optimality statement about either model.*

<a id="op1-training-trajectory"></a>
**OP1 (still open): trajectory of $\rho_h$ during training.** The static analysis of this chapter takes $M$ as given and asks what happens in the post-softmax space. The dynamical question — when does $\rho_h$ grow from its random initial value to the directional regime during training? — is the subject of Chapter 11. We do not pursue it here.

**Takeaway.** *The chapter establishes four rigorous results around the post-softmax persistence of antisymmetric structure. Theorem 5.2 (Antisymmetric Sink) and Proposition 5.5 (Edge Persistence) give global and local sufficient conditions for the algebraic decomposition $M=M_s+M_a$ of Chapter 4 to survive the softmax. Proposition 5.11 (Refined Edge Persistence) sharpens the edge bound via the integral identity for LSE, with a correctly stated unnormalised majorant. Theorem 5.20 (Antisymmetric-removal Stability) quantifies the bidirectional perturbation of the forward computation under removal of the antisymmetric components of the score matrices. Permutation equivariance of a transformer without positional information (Proposition 5.15) is recorded separately, since it follows from the architectural hypotheses alone and does not depend on the asymmetry budget. The four results turn the empirical attention sink phenomenon into a structural prediction from operator-theoretic conditions on the trained parameters, with explicit quantitative consequences for the post-softmax matrix $P$ and a controlled (bidirectional) perturbation bound on the task loss.*

<a id="ch05-bibliographic-notes"></a>
### Bibliographic notes

The two main theorems of this chapter (Theorem 5.2 and Proposition 5.5) appear, with proofs essentially identical to those given here, in the extended companion to Bonino et al. (2025) (subsection 3.1.1, “Symmetric/antisymmetric decomposition and post-softmax directional persistence”). The refined edge bound of Proposition 5.11, the permutation-equivariance proposition without positional information (Proposition 5.15), and the antisymmetric-removal stability theorem (Theorem 5.20) are new contributions of the present chapter. An earlier draft of Section 5.8 contained two further results — a Symmetry Obstruction proposition asserting that $\delta_h=0$ on every head implies permutation equivariance, and a Quantitative Coupling theorem deriving a one-sided lower bound on the loss from the asymmetry budget — which were withdrawn. The withdrawal is documented in the proof discussion of Proposition 5.15 and in Remark 5.21; the corrected statements are the ones above.

The Lipschitz constant of the softmax used in Lemma 5.18 is from Gao and Pavel (2017); the constant for the transformer block in Remark 5.22 is from Kim et al. (2021).

The attention sink phenomenon was named by Xiao et al. (2024); the most systematic empirical study is Gu et al. (2025); the functional account in terms of over-mixing prevention is Barbero et al. (2025); the active-dormant mechanism is Guo et al. (2024); the geometric reading via reference frames is et al (2025d); the spectral reading is Cancedda (2024).

The decomposition $M=M_s+M_a$ was introduced concurrently and independently by Bonino et al. (2025) and Saponati et al. (2025). A complete formal comparison between the two streams is given in Chapter 4.

> **Editorial boundary.** Chapter 5 ends completely on PDF 178 / printed page 142. [The next chapter begins on PDF 179](0179-0186-ch06-notation-and-prior-taxonomies.md#pdf-0179). The future experiments and training dynamics cited above are not supplied or reconstructed here.
