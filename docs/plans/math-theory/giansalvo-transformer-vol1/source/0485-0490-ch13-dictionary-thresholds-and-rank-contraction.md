---
title: "Chapter 13: dictionary thresholds, training conditions, and rank contraction"
source_pdf_pages: [485, 490]
book_pages: [449, 454]
batch: "010"
language: en
type: source-transcription
status: complete-for-range
---

# Chapter 13: dictionary thresholds, training conditions, and rank contraction

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous pages](0479-0484-ch13-trajectory-values-and-redundancy-hierarchy.md#pdf-0484) · [Next pages](0491-0495-ch13-width-bounds-and-design-principles.md#pdf-0491)

> Faithful English transcription of physical PDF pages 485–490 / printed pages 449–454. Original claims, unresolved references, and mathematical inconsistencies are retained; editorial analysis is separate in Russian notes 163–166. PDF 485 continues the proof discussion of Theorem 13.24 from PDF 484. PDF 490 ends mid-sentence; the sentence continues on PDF 491.

<a id="pdf-0485"></a>
## PDF 485 · Book 449

parametrically equivalent) but only one of them is on the post-training input support, so functional equivalence is empty in the strict sense. $\square$

**Why this is conditional.** The result is conditional on (NO-1) and (NO-2). Both hypotheses are plausible for trained transformers under standard initialisation, but neither is generic: pathological constructions (e.g. two heads with identical activation operators yet distinct training histories preserved by accidental symmetries of the optimiser) violate (NO-2) in measure zero, and the pathologies of §13.6 above can violate (NO-1) at small but positive measure.

<a id="section-13-6-4"></a>
### 13.6.4 Operational reading

The hierarchy organises the pruning and compression decisions of §13.10.

- Detecting *parametric* redundancy is the cheapest: it requires only the architecture and the trained weights. Pruning on parametric grounds alone is the most conservative criterion, because parametric equivalence is the coarsest equivalence, and removing one of two parametrically equivalent heads loses no information about the input–output map.
- Detecting *functional* redundancy requires the input distribution. Pruning on functional grounds is more aggressive: it removes heads whose action on the cloud is already covered, even when their parameters differ.
- Detecting *dynamic* redundancy requires the training history. Pruning on dynamic grounds is the most aggressive: it removes heads whose orbits coincide, even when their endpoint actions and parameters differ.

The cost of detection grows with the strength of the criterion. Architectures that aim at minimal redundancy must therefore decide which level of detection to invest in. The rationalised block of Chapter 14 works at the dynamic level by construction: its heads are differentiated on signature orbits before being differentiated on parameters or actions, so the hierarchy is automatically saturated.

**Preview.** *The next section imports the dictionary-geometric framework of the dictionary-geometric analysis, which provides the algebraic counterpart of the hierarchy: a precise minimum number of attention atoms required to break the channel symmetry, with the corresponding minimal head count.*

<a id="section-13-7"></a>
### 13.7 Dictionary-geometric framework

The previous sections have measured redundancy. The question that remains is the inverse: given a task, what is the minimum number of heads, and the minimum dictionary of attention atoms, that can realise it without redundancy? This section answers the question for the *representational* redundancy associated with head-channel non-identifiability.

The framework used here is the dictionary-geometric framework. The exposition records its three core statements with explicit dependence assumptions. The first characterises which dictionaries break the channel symmetry; the second gives the minimal dictionary size; the third gives the training criterion under which gradient descent reaches a separating configuration.

<a id="pdf-0486"></a>
## PDF 486 · Book 450

<a id="section-13-7-1"></a>
### 13.7.1 The channel symmetry

The starting point is the identification of a gauge symmetry of the multi-head attention block whose orbits coincide with indistinguishable head configurations.

<a id="definition-13-25"></a>
**Definition 13.25 (Channel symmetry group).** Let $H$ be the number of heads and $d_k$ be the head dimension. The *channel symmetry group* is

<a id="equation-13-56"></a>
$$
\mathcal G_{\mathrm{channel}}:=\prod_{h=1}^{H}O(d_k),
\tag{13.56}
$$

acting on the per-head outputs $(Y^{(1)},\ldots,Y^{(H)})$ by $Y^{(h)}\mapsto Y^{(h)}R_h^\top$ for $R_h\in O(d_k)$.

The action of $\mathcal G_{\mathrm{channel}}$ leaves the multi-head attention output $\operatorname{MHA}(X)=\sum_hY^{(h)}W_O^{(h)}$ unchanged under the simultaneous transformation $W_O^{(h)}\mapsto R_hW_O^{(h)}$. The orbits of the action are therefore the equivalence classes of head configurations that produce the same block output. The channel-symmetry calculation shows that the orbit of a single head configuration has dimension $\binom{d_k}{2}$ per head, and the joint orbit on $H$ heads has dimension $H\binom{d_k}{2}$ as a sub-manifold of the parameter space.

<a id="definition-13-26"></a>
**Definition 13.26 (Orbit-separating dictionary).** A finite set of antisymmetric matrices $\mathcal D=\{\Gamma_1,\ldots,\Gamma_K\}\subset\mathfrak{so}(d_{\mathrm{model}})$ together with a family of input–position-dependent coefficients $\{\beta_k(X,i)\}_{k=1}^{K}$ *separates the orbits* of $\mathcal G_{\mathrm{channel}}$ if for almost every pair $(Y,Y')$ in the same $\mathcal G_{\mathrm{channel}}$-orbit there exists $X$ such that

<a id="equation-13-57"></a>
$$
\exp\!\left(\sum_{k=1}^{K}\beta_k(X,i)\Gamma_k\right)\cdot Y_i
\ne
\exp\!\left(\sum_{k=1}^{K}\beta_k(X,i)\Gamma_k\right)\cdot Y'_i
\tag{13.57}
$$

for some position $i$.

<a id="section-13-7-2"></a>
### 13.7.2 The dictionary threshold theorem

The first main theorem characterises the dictionaries that achieve orbit separation. Let $\mathfrak m\subset\mathfrak{so}(d_{\mathrm{model}})$ denote the orthogonal complement of the Lie algebra of $\mathcal G_{\mathrm{channel}}$, with block decomposition $\mathfrak m=\bigoplus_{h<h'}\mathfrak m_{hh'}$ where $\mathfrak m_{hh'}\cong\mathbb R^{d_k\times d_k}$ is the inter-head block coupling head $h$ to head $h'$. Let $\pi_{hh'}$ denote the orthogonal projection onto $\mathfrak m_{hh'}$.

The theorem requires two non-degeneracy hypotheses on the cloud and the coefficients.

<a id="hypothesis-n"></a>
- *Hypothesis N (norm non-degeneracy):* the action of $\mathcal G_{\mathrm{channel}}$ preserves cloud-row norms, and these are bounded away from zero. Implied by layer normalisation acting before the dictionary application.

<a id="hypothesis-g"></a>
- *Hypothesis G (toral avoidance):* for every position $i$, the family $\{\Gamma_k\beta_k(X,i)\}$ is generic in the sense that, with probability one over the input distribution $\mu_X$, no proper sub-torus of the maximal torus $T^{d_k}\subset SO(2d_k)$ contains the action at $i$. Implied by training condition T2 (Definition 13.29 below) plus head specialisation, by Theorem 13.31.

<a id="theorem-13-27"></a>
**Theorem 13.27 (Dictionary Threshold Theorem).** Under Hypothesis N and Hypothesis G, the dictionary $\mathcal D$ with position-dependent coefficients $\beta_k(\cdot,i)$ separates the orbits of $\mathcal G_{\mathrm{channel}}$ if and only if for every position $i$ and every pair $h\ne h'$

<a id="equation-13-58"></a>
$$
\dim\!\left(\pi_{hh'}\!\left(\operatorname{span}\{\Gamma_k\beta_k(X,i):k=1,\ldots,K\}\right)\right)\ge d_k
\tag{13.58}
$$

for generic $X\sim\mu_X$.

<a id="pdf-0487"></a>
## PDF 487 · Book 451

*Proof.* The argument proceeds through four local steps. First, separation is local in position: if two channel-equivalent configurations cannot be distinguished at any position, the global map obtained by applying the dictionary to all positions cannot separate the pair. The position coordinate is therefore the only place where the dictionary can break the channel orbit.

Second, for a fixed pair of heads $(h,h')$ and a fixed position $i$, only the projection of the dictionary atoms onto the inter-head block $\mathfrak m_{hh'}$ can detect the corresponding channel ambiguity. If the projected span has dimension smaller than $d_k$, a non-zero ambiguity remains orthogonal to all dictionary probes at that position. Moving the configuration along this undetected ambiguity produces a pair that is not separated. This proves necessity of (13.58).

Third, when the projected span has dimension at least $d_k$ in every inter-head block, the dictionary contains enough independent probes to test each $d_k$-dimensional pairwise ambiguity. Hypothesis N prevents these probes from being annihilated by zero-norm cloud components. Hypothesis G excludes the exceptional case in which the position-dependent coefficients remain trapped in a lower-dimensional toral subsystem and therefore fail to explore the available inter-head directions. The role of the torus condition is thus generic separation, not transitivity of a maximal torus on a sphere.

Finally, under these two non-degeneracy assumptions, every non-trivial channel displacement is detected at some position on a positive-measure set of clouds. The local separation criterion then lifts the position-wise separation to orbit separation. This proves sufficiency. $\square$

**Operational reading.** The condition (13.58) says: at every position and for every pair of heads, the dictionary atoms must collectively project a space of dimension at least $d_k$ onto the inter-head block $\mathfrak m_{hh'}$. The number $d_k$ is precisely the dimension of the per-head ambiguity, so the requirement is that each pairwise ambiguity be fully covered.

<a id="section-13-7-3"></a>
### 13.7.3 Minimal dictionary

The threshold theorem is sharp: the dictionary that just satisfies it has a precise minimal size.

<a id="corollary-13-28"></a>
**Corollary 13.28 (Minimal dictionary).** Under the block-local rank convention used in Theorem 13.27, the minimal dictionary satisfying the threshold condition has

<a id="equation-13-59"></a>
$$
K^*=\binom H2
\tag{13.59}
$$

atoms, one per pair of heads, each of rank $d_k$. The parameter cost of such a minimal dictionary is $O(H^2d_{\mathrm{model}})$ for the $\beta_k$ coefficient networks. For a BERT-base scale block $(H=12,d_k=64,d_{\mathrm{model}}=768)$, this gives $K^*=66$ atoms with parameter cost approximately $152\text{ K}$ per layer, against the $360\text{ K}$ degrees of freedom destroyed per layer by the head-channel non-identifiability theorem (the channel-symmetry calculation).

*Proof.* Each pair $(h,h')$ requires its own contribution of dimension at least $d_k$ in $\mathfrak m_{hh'}$. Since each block $\mathfrak m_{hh'}$ must receive a $d_k$-dimensional projected span, one full-rank block atom assigned to the pair suffices. This gives $\binom H2$ atoms in total. Below this count, by pigeonhole at least one pair receives $\dim\le d_k-1$, violating (13.58). $\square$

<a id="pdf-0488"></a>
## PDF 488 · Book 452

**Comparison with a diagonal position-gated projection.** A diagonal position-gated projection uses $K=H$ atoms with $\pi_{hh'}(\Gamma_k)=0$ for all $h\ne h'$. The dimension condition (13.58) is therefore not met for any inter-head pair: a diagonal position-gated projection does not satisfy Theorem 13.27 and does not achieve full orbit separation. It addresses head-channel non-identifiability only partially, by acting through the diagonal of the block decomposition. The dictionary-geometric framework gives the precise algebraic reason for this partiality: a diagonal position-gated projection lives in the centraliser of $\mathcal G_{\mathrm{channel}}$, not in $\mathfrak m$.

<a id="section-13-7-4"></a>
### 13.7.4 Training conditions and the convergence theorem

The threshold theorem characterises orbit-separating dictionaries *statically*: it says when a dictionary, if it exists, achieves orbit separation. The remaining question is when gradient descent reaches such a dictionary. Three training conditions are sufficient.

<a id="definition-13-29"></a>
**Definition 13.29 (Training conditions T1, T2, T3).**

- **T1 (non-central initialisation):** $\pi_{hh'}(\Gamma_k)\ne0$ at initialisation for all $k=1,\ldots,K$ and all $h<h'$.
- **T2 (coefficient diversity):** the position-wise correlation between coefficient pairs satisfies $|\rho_{kk'}^{(i)}|:=|\operatorname{Corr}_{X\sim\mu_X}(\beta_k(X,i),\beta_{k'}(X,i))|\le1-\varepsilon$ for all $k\ne k'$, all positions $i$, and some $\varepsilon>0$.
- **T3 (Riemannian gradient on $\mathfrak{so}$):** the atom update is the antisymmetric projection of the Euclidean gradient, $\Gamma_k\leftarrow\Gamma_k-\eta\,\operatorname{antisym}(\partial L/\partial\Gamma_k)$, keeping atoms in $\mathfrak{so}(d_{\mathrm{model}})$.

<a id="theorem-13-30"></a>
**Theorem 13.30 (Training Criterion).** Under conditions T1, T2, T3, with layer normalisation acting before the dictionary (which implies Hypothesis N), and provided the task loss $L_{\mathrm{task}}$ is strictly convex on the orbits of $\mathcal G_{\mathrm{channel}}$, gradient descent on $L_{\mathrm{task}}$ converges, with probability one over random initialisation, to a dictionary satisfying the threshold condition (13.58).

**Why this is conditional.** The conclusion is conditional on strict convexity of $L_{\mathrm{task}}$ on the $\mathcal G_{\mathrm{channel}}$-orbits. It is a sufficient condition under which the orbit decomposition of the loss landscape is non-degenerate. It holds in special cases (linear classifiers on sufficiently rich feature spaces; tasks with unique decision boundary modulo gauge), but a general proof for masked language modelling and related pre-training objectives is open.

**Operational reading.** T1 ensures non-zero gradient with respect to the inter-head atoms from initialisation. T3 keeps the atoms inside the antisymmetric algebra. T2 prevents atom collapse and implies Hypothesis G, by Theorem 13.31 below. Layer normalisation provides Hypothesis N. The strict-convexity hypothesis on the loss provides the gradient signal that distinguishes orbit-inequivalent configurations and drives the dictionary toward the threshold.

<a id="section-13-7-5"></a>
### 13.7.5 Specialisation guarantees coefficient diversity

The training condition T2 (coefficient diversity) is not directly verifiable from initialisation alone. The following theorem shows that head specialisation, measured by the S–A balance $\alpha_h$ of Chapter 9, is sufficient.

<a id="theorem-13-31"></a>
**Theorem 13.31 (Specialisation implies decorrelation).** Let $\alpha_{h_k}$ and $\alpha_{h_{k'}}$ be the S–A balances of the two heads associated to dictionary atoms $\Gamma_k,\Gamma_{k'}$ under the minimal-dictionary correspondence of Corollary 13.28. If

<a id="equation-13-60"></a>
$$
|\alpha_{h_k}-\alpha_{h_{k'}}|\ge\delta>0
\tag{13.60}
$$

<a id="pdf-0489"></a>
## PDF 489 · Book 453

for every dictionary pair $(k,k')$, then

<a id="equation-13-61"></a>
$$
|\rho_{kk'}^{(i)}|\le1-\frac12\delta^2+O(\delta^3)
\tag{13.61}
$$

uniformly in position $i$. Condition T2 holds with $\varepsilon=\frac12\delta^2$ without explicit regularisation. For a BERT-base scale block a specialisation threshold of $\delta\ge0.45$ suffices for $\varepsilon=0.1$.

The result gives a sufficient route from head specialisation to coefficient diversity. It does not claim that a gap in $\alpha_h$ alone forces decorrelation without assumptions on the coefficient maps. Under the stated regularity, the burden of monitoring coefficient correlations can be reduced to monitoring head specialisation, an observable already in use throughout Chapter 11. Specialisation, in turn, is driven by the training dynamics analysed in Chapter 11 via the gap-opening transition (Theorem ??). The dictionary-geometric framework therefore plugs directly into the orbit-level training analysis: post-training models with broken specialisation symmetry can satisfy Hypothesis G via T2 when the coefficient-map regularity is met.

<a id="section-13-7-6"></a>
### 13.7.6 What the framework gives the rationalised block

The dictionary-geometric framework supplies three quantitative inputs to the rationalised block of Chapter 14:

1. A *minimal head count* for a given task: by Corollary 13.28, the rationalised block carries at least $K^*=\binom H2$ inter-head atoms, with parameter cost $O(H^2d_{\mathrm{model}})$. This is the foundation of design principle P2 (§13.9).
2. A *specialisation requirement*: by Theorem 13.31, the rationalised block must produce or maintain a specialisation gap $\delta\ge0.45$ across pairs. This is the foundation of P5.
3. A *training prescription*: under Theorem 13.30, the rationalised block trains correctly under standard gradient descent with the antisymmetric update of T3. This is the foundation of the training criterion of §13.10.

**Preview.** *Section 13.9 extracts the six design principles and recovers the framework's prescriptions in plain operational form.*

<a id="section-13-8"></a>
### 13.8 Self-sufficiency and feed-forward width

The dictionary-geometric framework of §13.7 gives a minimal head count under the assumption that the multi-head output is post-processed by a feed-forward network of sufficient width. The present section makes the second assumption quantitative. The standard choice $d_{\mathrm{ff}}=4d_{\mathrm{model}}$, used in BertBase and most descendants, is an engineering heuristic: there is no general derivation of the factor four. The analysis below replaces the heuristic by a lower bound that depends on the rank contraction caused by head specialisation.

The result has two parts. The first imports the Rank Contraction Theorem of the self-sufficiency theorem and its width corollary; both are recorded as with reference to the original proofs. The second is a new specialisation-aware refinement that combines the rank contraction bound with the dictionary-geometric constraints of §13.7, obtaining a sharper lower bound on $d_{\mathrm{ff}}$ in terms of the S–A balance distribution.

<a id="section-13-8-1"></a>
### 13.8.1 Specialisation–contraction–expansion

The starting observation is that head specialisation, which is beneficial for orbit separation (Theorem 13.31), is detrimental for the rank of the multi-head output. The two effects must

<a id="pdf-0490"></a>
## PDF 490 · Book 454

be reconciled by the feed-forward sub-layer.

Let $R_h^{(\ell)}:=\operatorname{rowsp}(Y^{(\ell,h)}W_O^{(\ell,h)})\subseteq\mathbb R^{d_{\mathrm{model}}}$ denote the row space written by head $h$ at layer $\ell$ into the multi-head output. By construction $\dim R_h^{(\ell)}\le d_k$, with equality when $Y^{(\ell,h)}$ has full column rank and $W_O^{(\ell,h)}$ is injective on its codomain.

<a id="theorem-13-32"></a>
**Theorem 13.32 (Rank Contraction Theorem).** The rank of the multi-head output satisfies

<a id="equation-13-62"></a>
$$
\operatorname{rank}\!\left(\operatorname{MHA}(X^{(\ell)})\right)
=\dim\!\left(\textstyle\sum_{h=1}^{H}R_h^{(\ell)}\right).
\tag{13.62}
$$

When the heads at layer $\ell$ write into a common subspace $V^{(\ell)}\subsetneq\mathbb R^{d_{\mathrm{model}}}$, the rank is bounded above by $\dim V^{(\ell)}\ll d_{\mathrm{model}}$. In the specialised regime in which each head's antisymmetric sector $M_a^{(\ell,h)}$ acts on a single dominant eigenplane, the contribution of each head to $R_h^{(\ell)}$ is at most two-dimensional, so

<a id="equation-13-63"></a>
$$
\operatorname{rank}\!\left(\operatorname{MHA}(X^{(\ell)})\right)\le2H.
\tag{13.63}
$$

The contraction has a precise architectural reading: the more specialised the heads, the smaller the rank of their joint output, and the more the feed-forward sub-layer is forced to compensate.

<a id="section-13-8-2"></a>
### 13.8.2 The $d_{\mathrm{ff}}$ lower bound from rank contraction

The feed-forward sub-layer must regenerate the directions that the multi-head output failed to write. The minimal width follows from this observation by a direct dimension count.

<a id="corollary-13-33"></a>
**Corollary 13.33 (Width lower bound from rank contraction).** Let $r^{(\ell)}:=\operatorname{rank}(\operatorname{MHA}(X^{(\ell)}))$ denote the rank of the multi-head output at layer $\ell$, viewed as an $\mathbb R^{n\times d_{\mathrm{model}}}$ matrix. To restore a post-block representation of full rank $d_{\mathrm{model}}$, the feed-forward sub-layer must generate at least $d_{\mathrm{model}}-r^{(\ell)}$ new directions outside $\sum_hR_h^{(\ell)}$. Under the standing hypothesis that the non-linearity is GeLU and the feed-forward weights are generic, the minimum feed-forward width satisfies

<a id="equation-13-64"></a>
$$
d_{\mathrm{ff}}^*(\ell)\ge2d_{\mathrm{model}}-r^{(\ell)}.
\tag{13.64}
$$

**Reading the bound.** At one extreme, if heads are unspecialised and write isotropically $(r^{(\ell)}=d_{\mathrm{model}})$, the bound reduces to $d_{\mathrm{ff}}^*\ge d_{\mathrm{model}}$ — the feed-forward sub-layer needs only to refine the existing representation. At the other extreme, with maximal specialisation $(r^{(\ell)}=2H\ll d_{\mathrm{model}})$, the bound becomes $d_{\mathrm{ff}}^*\ge2d_{\mathrm{model}}-2H$, which is larger than the model dimension and approaches the standard $4d_{\mathrm{model}}$ heuristic only in the regime $2d_{\mathrm{model}}-2H\approx4d_{\mathrm{model}}$, i.e. $H\approx-d_{\mathrm{model}}$, which is never the case. The canonical choice $d_{\mathrm{ff}}=4d_{\mathrm{model}}$ is therefore never tight under the rank-contraction analysis: it is too large in the unspecialised regime and asymptotically appropriate (but for the wrong reason) in the maximally specialised regime.

<a id="section-13-8-3"></a>
### 13.8.3 A specialisation-aware refinement

The bound (13.64) treats $r^{(\ell)}$ as a single number measuring the joint rank. The refinement of this section uses the S–A balance distribution $\{\alpha_h^{(\ell)}\}_{h=1}^{H}$ of Chapter 9 to obtain a bound that distinguishes the directional contribution of each head from its symmetric one.

The starting observation is structural. The row space $R_h^{(\ell)}$ written by head $h$ decomposes naturally into a symmetric component $R_h^{(\ell),s}$, contributed by the symmetric sector $M_s^{(\ell,h)}$

> Continued on [PDF 491](0491-0495-ch13-width-bounds-and-design-principles.md#pdf-0491).
