---
title: "Chapter 6: parametric dictionary, two signatures, and canonical projection"
source_pdf_pages: [211, 214]
book_pages: [175, 178]
batch: "005"
language: en
type: source-transcription
status: complete-for-page-range
continues_after_file: true
---

# Chapter 6: parametric dictionary, two signatures, and canonical projection

[Library index](../README.md) · [Source and transcription policy](../SOURCE.md) · [Previous pages: PDF 207–210](0207-0210-ch06-relative-completeness-and-fourier-grammar.md#pdf-0210) · [Next pages: PDF 215–220](0215-0220-ch06-signature-experiments-and-empirical-links.md#pdf-0215)

> Editorial note: Full transcription of PDF 211–214 (printed pages 175–178). The source's dictionary cardinalities, definitions, mathematical claims, and distinction or conflation between eight representatives and the full dictionary are retained. See [relative completeness](../notes/45-relative-completeness-and-fourier-grammar.md), [dictionary and signatures](../notes/46-parametric-dictionary-and-two-signatures.md), and [projection and stability](../notes/47-canonical-projection-and-stability.md) for separate editorial analysis. The statement of Theorem 6.56 is complete here, but its proof continues with the final paragraph on PDF 215.

<a id="pdf-0211"></a>
## PDF 211 · Book 175

<a id="section-6-16-4"></a>
### 6.16.4 Closure of the catalogue, with the right qualifier

With Theorems 6.44 and 6.49, the catalogue is closed in two independent senses, both qualified by the shift-regular restriction. The group-theoretic theorem closes it combinatorially: there are no other shift-regular row-stochastic matrices than the eight canonical forms taken with their parametric families. The Fourier theorem closes it spectrally: there are no other shift-regular spectral supports. The two senses agree by Pontryagin duality.

For row-stochastic matrices that violate the linear-support axiom — non-cyclic permutations, irregular blocks, non-uniform supports, row-dependent support sizes — the catalogue does not provide an exact classification. The next section introduces the canonical-coordinate system that handles arbitrary $S\in\mathcal S_n$ by Frobenius projection plus residual.

<a id="section-6-17"></a>
### 6.17 Canonical signatures, projection, and stratification

The relative-completeness theorems of §6.16 classify shift-regular row-stochastic matrices: every such matrix is one of the eight canonical forms or a convex combination of them. They say nothing about non-shift-regular matrices, of which there are many. A non-cyclic permutation, a row whose support concentrates with non-uniform weights, a head whose support sizes vary across rows: none of these is shift-regular, and none can therefore be equal to a canonical regime or any of its mixtures. Empirical attention maps drawn from trained transformers regularly violate one or another axiom of Definition 6.42, and the question of how to describe such maps within the eight-regime framework requires a descriptor that gracefully degrades outside the shift-regular class.

This section constructs such a descriptor. The idea is to replace the exact match against canonical forms with a Frobenius projection onto the convex hull of the eight regime families: every $S\in\mathcal S_n$ is decomposed as the canonical projection $\Pi_8(S)$ plus a non-canonical residual $R_8(S)$. The projection is unique by Hilbert projection theorem; the residual $\rho_8(S)=\lVert R_8(S)\rVert_F$ is zero exactly when $S$ lies in the canonical hull, and grows to measure the distance to the canonical world otherwise. To each regime $\alpha$ we attach two complementary signatures: a Frobenius projection coefficient $\widehat\lambda_\alpha(S)\in[0,1]$ that sums to one over the eight regimes, and a normalised distance signature $\sigma_\alpha(S)$ that records geometric similarity. The pair $(\widehat\lambda,\rho_8)$ places every attention map in eight-dimensional canonical coordinates and extends the eight-regime catalogue to the entire row-stochastic polytope $\mathcal S_n$.

The construction is rigorous, computable in polynomial time by quadratic programming, Lipschitz under perturbations of $S$, and exact on the canonical hull. We verify each property by direct calculation, then exhibit seven didactic experiments that visualise the geometry on synthetic and synthetic-BERT-style inputs.

<a id="section-6-17-1"></a>
### 6.17.1 The parametric dictionary as a union of orbits

Each canonical regime is not a single matrix but a parametric family. The directional gate has $n$ choices of source column. The cyclic contextual gates have $n-1$ non-trivial shifts in each direction. The cluster head has all rectangular block supports. We make these parametric dependencies explicit.

<a id="definition-6-51"></a>
**Definition 6.51 (Parametric dictionary).** For each canonical regime $\alpha\in\{1,\ldots,8\}$, let $\Theta_\alpha(n)$ be its discrete parameter space and let $\mathcal D_\alpha(n)=\{S_{\alpha,\theta}:\theta\in\Theta_\alpha(n)\}$ be the corresponding orbit of canonical prototypes. The full parametric dictionary at sequence length $n$ is the

<a id="pdf-0212"></a>
## PDF 212 · Book 176

union

<a id="equation-6-34"></a>
$$
\mathcal D_8(n)=\bigcup_{\alpha=1}^8\mathcal D_\alpha(n)\subseteq\mathcal S_n.
\tag{6.34}
$$

The orbits are listed below, with the parameter cardinality at $n=8$ displayed in parentheses for concreteness:

- $\mathcal D_{\mathrm{open}}(n)=\{I_n\}$ — (1 atom)
- $\mathcal D_{\mathrm{closed}}(n)=\{n^{-1}\mathbf1\mathbf1^\top\}$ — (1 atom)
- $\mathcal D_{\mathrm{dir}}(n)=\{S_V^{(c)}:1\leq c\leq n\}$ — ($n=8$ atoms)
- $\mathcal D_{\mathrm{back}}(n)=\{S_{sD}^{(d)}:1\leq d\leq n-1\}$ cyclic shifts — (7 atoms)
- $\mathcal D_{\mathrm{forw}}(n)=\{S_{SD}^{(d)}:1\leq d\leq n-1\}$ cyclic shifts — (7 atoms)
- $\mathcal D_{\mathrm{clust}}(n)=\{S_B^{(R,C)}:R,C\subseteq[n],\ 1<|C|<n\}$ — (2304 atoms)
- $\mathcal D_{\mathrm{inst}}(n)=\{S_P^{(r,c)}:r,c\in[n]\}$ — ($n^2=64$ atoms)
- $\mathcal D_{\mathrm{inv}}(n)=\{S_H^{(r,C)}:r\in[n],\ C\subsetneq[n],\ |C|>1\}$ — (384 atoms)

After de-duplication, the cardinality of $\mathcal D_8(8)$ is $2769$. The dictionary grows polynomially in $n$ for each individual orbit, exponentially in $n$ for the cluster orbit (due to its arbitrary rectangular blocks). The polynomial growth of seven of the eight orbits is good news: most regimes are sampled by $O(n^2)$ atoms or fewer.

**The canonical hull.** The convex hull of the dictionary is

<a id="equation-6-35"></a>
$$
K_8(n)=\operatorname{conv}\bigl(\mathcal D_8(n)\bigr)\subseteq\mathcal S_n.
\tag{6.35}
$$

By construction $K_8(n)$ is a non-empty compact convex polytope contained in the row-stochastic polytope $\mathcal S_n$. Elements of $K_8(n)$ are exactly the finite convex combinations of canonical prototypes that Proposition 6.37 described. The complement $\mathcal S_n\setminus K_8(n)$ contains all maps that are not expressible as canonical mixtures: non-cyclic permutations, irregular supports, deformations.

<a id="section-6-17-2"></a>
### 6.17.2 The two signatures

Two complementary signatures will be attached to each $S$. The distance signature $\sigma_\alpha(S)$ records geometric proximity to each regime family independently, and is appropriate for empirical similarity ranking. The projection signature $\widehat\lambda_\alpha(S)$ records the canonical decomposition coefficients that arise from the Frobenius projection onto the eight families simultaneously, and is appropriate for canonical recovery and exact identification on the canonical hull. Both are useful; they answer different questions.

**Distance signature.** For each regime $\alpha$ define the regime distance

<a id="equation-6-36"></a>
$$
d_\alpha(S):=\frac1{\sqrt n}\min_{Q\in\mathcal D_\alpha(n)}\lVert S-Q\rVert_F.
\tag{6.36}
$$

The normalisation by $\sqrt n$ makes the scale comparable across sequence lengths. Given a bandwidth $\tau>0$, define the raw similarity $q_\alpha(S)=\exp\bigl(-d_\alpha(S)^2/\tau^2\bigr)$ and the normalised

<a id="pdf-0213"></a>
## PDF 213 · Book 177

distance signature

<a id="equation-6-37"></a>
$$
\sigma_\alpha(S)=\frac{q_\alpha(S)}{\sum_\beta q_\beta(S)}.
\tag{6.37}
$$

The vector $\sigma_8(S)=(\sigma_1(S),\ldots,\sigma_8(S))\in\Delta_8$ is the eight-regime distance signature.

**Projection signature.** A second, complementary signature is obtained by simultaneously projecting $S$ onto the canonical hull. Let $\{S_\alpha^*\}_{\alpha=1}^8$ be one chosen representative per family (the central member of each orbit, e.g. the centred shift, the central instance, the central cluster), and define

<a id="equation-6-38"></a>
$$
\widehat\lambda(S)=\underset{\lambda\in\Delta_8}{\arg\min}\left\lVert S-\sum_\alpha\lambda_\alpha S_\alpha^*\right\rVert_F^2.
\tag{6.38}
$$

The vector $\widehat\lambda(S)\in\Delta_8$ minimises a strictly convex quadratic on a compact polytope and is therefore unique. Its $\alpha$-th coordinate is the canonical projection weight on representative $S_\alpha^*$.

<a id="remark-6-52"></a>
**Remark 6.52 (Operational difference between the two signatures).** On a clean canonical regime $S=S_\alpha^*$: the projection signature is $\widehat\lambda=e_\alpha$ exactly, while the distance signature has $\sigma_\alpha\to1$ only in the small-bandwidth limit $\tau\to0$. On a generic input: the projection signature picks the best convex mixture of regime representatives, while the distance signature reports similarity to each family separately and may simultaneously have large $\sigma$ for two structurally close regimes (forward and backward shifts at the same $|d|$, for instance). The projection is appropriate for asking “what canonical mixture explains $S$ best?”; the distance signature is appropriate for asking “which regimes does $S$ resemble?”.

<a id="section-6-17-3"></a>
### 6.17.3 Existence, uniqueness, and Lipschitz continuity

Both signatures are well-defined and continuous in $S$.

<a id="proposition-6-53"></a>
**Proposition 6.53 (Lipschitz continuity of distance signature).** For every regime $\alpha$, the regime distance $d_\alpha:\mathcal S_n\to\mathbb R_{\geq0}$ is $n^{-1/2}$-Lipschitz in Frobenius norm: for all $S,S'\in\mathcal S_n$,

$$
|d_\alpha(S)-d_\alpha(S')|\leq n^{-1/2}\lVert S-S'\rVert_F.
$$

Consequently, the raw similarities $q_\alpha$ and the normalised signatures $\sigma_\alpha$ are continuous on $\mathcal S_n$, with $\sigma_\alpha:\mathcal S_n\to[0,1]$ Lipschitz on every compact set.

*Proof.* The distance to a non-empty closed set in a Hilbert space is $1$-Lipschitz. Since $d_\alpha=n^{-1/2}\operatorname{dist}_F(\cdot,\mathcal D_\alpha(n))$, its Lipschitz constant is $n^{-1/2}$. The maps $x\mapsto e^{-x^2/\tau^2}$ and $(q_1,\ldots,q_8)\mapsto q_\alpha/\sum_\beta q_\beta$ are smooth on $\{q:\sum_\beta q_\beta>0\}$. Since each $q_\beta(S)>0$ for every $S$ (the exponential never vanishes), the denominator is bounded below on every compact set, giving Lipschitz continuity of the normalisation. $\square$

<a id="proposition-6-54"></a>
**Proposition 6.54 (Existence and uniqueness of projection signature).** The minimisation problem (6.38) has a unique minimiser $\widehat\lambda(S)\in\Delta_8$ for every $S\in\mathcal S_n$ whenever the eight chosen representatives $\{S_\alpha^*\}$ are affinely independent in $\mathbb R^{n\times n}$. The map $S\mapsto\widehat\lambda(S)$ is Lipschitz in Frobenius norm on $\mathcal S_n$.

*Proof.* The objective in (6.38) is a convex quadratic in $\lambda$, strictly convex when the representatives are affinely independent. The simplex $\Delta_8$ is a compact convex polytope. A strictly convex continuous function on a compact convex set attains a unique minimum. Lipschitz continuity of the minimiser as a function of the linear term in the quadratic objective follows from the fact that the active face of the simplex changes piecewise-linearly in $S$ and the

<a id="pdf-0214"></a>
## PDF 214 · Book 178

projection onto each face is itself Lipschitz; the union of the resulting piecewise-linear maps is Lipschitz on $\mathcal S_n$. $\square$

<a id="remark-6-55"></a>
**Remark 6.55 (The choice of representatives).** The projection signature (6.38) depends on the chosen representative $S_\alpha^*$ within each family. Different choices produce slightly different signatures away from canonical forms, but the qualitative structure — which families dominate, the size of the residual — is robust under reasonable choices. In practice we use the central or symmetric member of each orbit (e.g. $d=1$ shift, central directional source, central cluster). Future work could replace the representative-based projection with the full hull projection $\arg\min_{Q\in K_8(n)}\lVert S-Q\rVert_F$, which is unique without reference to representatives but requires a quadratic program of size $|\mathcal D_8(n)|$ rather than $8$.

<a id="section-6-17-4"></a>
### 6.17.4 The canonical projection theorem

The two signatures together imply the central theorem of this section: a unique decomposition of every row-stochastic matrix into canonical and non-canonical components.

<a id="theorem-6-56"></a>
**Theorem 6.56 (Canonical projection and stratification).** For every $S\in\mathcal S_n$ there is a unique decomposition

$$
S=\Pi_8(S)+R_8(S),
$$

where

<a id="equation-6-39"></a>
$$
\Pi_8(S):=\underset{Q\in K_8(n)}{\arg\min}\lVert S-Q\rVert_F
\tag{6.39}
$$

is the Frobenius projection of $S$ onto the canonical hull, and

<a id="equation-6-40"></a>
$$
R_8(S):=S-\Pi_8(S)
\tag{6.40}
$$

is the non-canonical residual. Moreover:

1. $R_8(S)=0$ if and only if $S\in K_8(n)$.
2. The projection satisfies the variational inequality

<a id="equation-6-41"></a>
$$
\langle R_8(S),Q-\Pi_8(S)\rangle_F\leq0\qquad\forall Q\in K_8(n).
\tag{6.41}
$$

3. The polytope $\mathcal S_n$ is stratified by the faces of $K_8(n)$. If $F\preceq K_8(n)$ is a face and

<a id="equation-6-42"></a>
$$
C_F=\{S\in\mathcal S_n:\Pi_8(S)\in\operatorname{relint}(F)\},
\tag{6.42}
$$

then

<a id="equation-6-43"></a>
$$
\mathcal S_n=\bigsqcup_{F\preceq K_8(n)}C_F,
\tag{6.43}
$$

the union being disjoint.

*Proof.* The set $K_8(n)$ is non-empty, compact, and convex in the Euclidean space $\mathbb R^{n\times n}$. The Hilbert projection theorem yields a unique nearest point $\Pi_8(S)$, and the decomposition $S=\Pi_8(S)+R_8(S)$ is by definition.

The equality $R_8(S)=0$ is equivalent to $S=\Pi_8(S)\in K_8(n)$. Conversely, if $S\in K_8(n)$, the nearest point in $K_8(n)$ is $S$ itself.

The variational inequality (6.41) is the standard first-order optimality condition for Euclidean projection onto a closed convex set: a point $Q_0\in K_8(n)$ is the projection of $S$ if and only if $\langle S-Q_0,Q-Q_0\rangle_F\leq0$ for all $Q\in K_8(n)$. Substituting $Q_0=\Pi_8(S)$ gives the displayed form.

> Editorial boundary: The proof of Theorem 6.56 continues with its final paragraph about the unique face containing the projection in its relative interior on [PDF 215](0215-0220-ch06-signature-experiments-and-empirical-links.md#pdf-0215). No proof-ending square is supplied here because it occurs on that next page.
