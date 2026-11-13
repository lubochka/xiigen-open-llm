---
title: "Chapter 12: sharpness, mixed regimes, and Hessian geometry"
source_pdf_pages: [417, 424]
book_pages: [381, 388]
batch: "009"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 12: sharpness, mixed regimes, and Hessian geometry

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: PDF 415–416](0415-0416-ch12-orbits-and-sharpness-introduction.md#pdf-0416) · [Next: PDF 425–428](0425-0428-ch12-activation-operator-and-random-initialisation.md#pdf-0425)

> Editorial note: Full transcription of PDF 417–424 (printed pages 381–388). The source's repeated words, unresolved references, incompatible inequalities, temperature factors, saturated-logit orientation, and displayed eigenvalue statements are retained. Notes 131–134 distinguish the source from editorial mathematical checks. The opening completes a sentence from PDF 416; §12.2.4 continues onto PDF 425.

<a id="pdf-0417"></a>
## PDF 417 · Book 381

*sharpness bound* that depends explicitly on a single scalar $\kappa(p,X,M)$ which is called the *relative emergence constant* of the pattern $p$ on the cloud $X$ under the bilinear form $M$.

The construction is purely deterministic: it does not rely on a saddle-exit analysis (which lives in the appendix to §12.4), on antisymmetric inevitability (which lives in §12.6), or on the gap-opening transition of Chapter 11. It is a static estimate that turns the row-wise relative emergence theorem into a Frobenius-norm sharpness curve. As such it is the machinery that downstream sections will use; it is not the dynamical counterpart, which Chapter 11 already provided.

<a id="section-12-2-1"></a>
### 12.2.1 Setup and definitions

Let $X\in\mathbb R^{n\times d}$ be a fixed input cloud and let $M\in\mathbb R^{d\times d}$ be a fixed bilinear form. The pre-softmax logit matrix is $L=L(X,M)=XMX^\top/\sqrt{d_k}$. For an inverse temperature $\beta>0$ it is written

<a id="equation-12-1"></a>
$$
S_\beta=\operatorname{softmax}_{\mathrm{row}}(\beta L)\in\mathcal S_n.
\tag{12.1}
$$

A canonical pattern is a triple $p=(\kappa,d,I)$ in the sense of Definition ?? (Chapter 7, equivalently Definition 1.42 of Chapter 6): an active row set $I\subseteq\{1,\ldots,n\}$, a support size $\kappa\in\{1,\ldots,n\}$, and a phase $d\in\mathbb Z_n$. The preferred support of row $i\in I$ under $p$ is $C_i(p)=\{i+d,\ldots,i+d+\kappa-1\}\bmod n$, and the canonical gate at $p$ is the row-stochastic matrix $\operatorname{Gate}(p)\in\mathcal S_n$ with $\operatorname{Gate}(p)_{ij}=\kappa^{-1}\mathbf1_{j\in C_i(p)}$ on active rows and $n^{-1}$ on inactive rows. It is written $u_{C_i}=\kappa^{-1}\mathbf1_{C_i}$ for the uniform distribution on $C_i$.

<a id="definition-12-1"></a>
**Definition 12.1 (Relative emergence constant).** Given a canonical pattern $p$, an input cloud $X$, and a bilinear form $M$, the *relative emergence constant* of the triple $(p,X,M)$ is

<a id="equation-12-2"></a>
$$
\kappa(p,X,M)=\min_{i\in I}
\frac{\min_{j\in C_i(p)}L_{ij}-\max_{k\notin C_i(p)}L_{ik}}
{1+\max_{i\in I}\max_{j,j'\in C_i(p)}|L_{ij}-L_{ij'}|}.
\tag{12.2}
$$

The numerator is the worst (over active rows) exterior margin in $L$. The denominator is one plus the worst (over active rows) intra-support spread in $L$. The constant $\kappa(p,X,M)$ is positive when $p$ is the preferred pattern of the row at every active position — equivalently, when no exterior column beats any in-support column on any active row.

The constant is dimensionless under simultaneous rescaling of $L$. The role of the denominator is to normalise the margin against the residual heterogeneity of the in-support logits, so that a head with a large exterior margin but a large intra-support spread is not classified as sharp. The relative emergence constant directly couples the two scalars $(\Delta,\rho)$ of Definition ??: for fixed $\beta$, exterior margin of $\beta L$ at row $i$ is $\beta\Delta_i(L)$ and intra-support spread is $\beta\rho_i(L)$, so $\beta\kappa(p,X,M)$ couples them as $\beta\Delta(L)=\beta\kappa\,(1+\rho(L))$.

<a id="section-12-2-2"></a>
### 12.2.2 Frobenius-level sharpness theorem

<a id="theorem-12-2"></a>
**Theorem 12.2 (Sharpness bound).** Let $X,M$ be fixed and let $p$ be a canonical pattern with support size $\kappa$ and active set $I$. Suppose $\kappa(p,X,M)>0$. Then for every $\varepsilon\in(0,\sqrt n)$ there exists

<a id="equation-12-3"></a>
$$
\beta^*(p,\kappa(p,X,M),\varepsilon)
=\frac1{\kappa(p,X,M)}
\log\!\left(\frac{4n(n-\kappa)+2\sqrt\kappa\,n}{\varepsilon^2}\right)
\tag{12.3}
$$

such that for all $\beta\ge\beta^*$,

<a id="equation-12-4"></a>
$$
\lVert S_\beta-\operatorname{Gate}(p)\rVert_F\le\varepsilon.
\tag{12.4}
$$

<a id="pdf-0418"></a>
## PDF 418 · Book 382

*Proof.* Each row is controlled each row of $S_\beta-\operatorname{Gate}(p)$ separately and then sum the squared row contributions.

*Active rows.* Fix $i\in I$ and write $\Delta_i=\Delta_i(L)=\min_{j\in C_i}L_{ij}-\max_{k\notin C_i}L_{ik}>0$ and $\rho_i=\rho_i(L)=\max_{j,j'\in C_i}|L_{ij}-L_{ij'}|\ge0$. By the definition of the relative emergence constant, $\Delta_i\ge\kappa(p,X,M)(1+\max_{i'\in I}\rho_{i'})\ge\kappa(p,X,M)(1+\rho_i)$. At inverse temperature $\beta$ the logit matrix is $\beta L$, so $\Delta_i(\beta L)=\beta\Delta_i$ and $\rho_i(\beta L)=\beta\rho_i$. Apply Lemma ?? of Chapter 6 (the corrected softmax-inversion lemma) to row $i$ of $\beta L$. With $\tau_i=(n-\kappa)e^{-\beta\Delta_i}$,

<a id="equation-12-5"></a>
$$
\lVert(S_\beta)_i-u_{C_i}\rVert_2
\le2\tau_i+\frac{e^{\beta\rho_i}-1}{\sqrt\kappa}.
\tag{12.5}
$$

Each summand is now bounded each summand on the right by $\varepsilon/\sqrt{2n}$, which will force the squared row contribution to be below $\varepsilon^2/n$.

For the exterior term, $2\tau_i\le\varepsilon/\sqrt{2n}$ is equivalent to $\beta\Delta_i\ge\log(2(n-\kappa)\sqrt{2n}/\varepsilon)$. For the interior term, $(e^{\beta\rho_i}-1)/\sqrt\kappa\le\varepsilon/\sqrt{2n}$ is equivalent to $\beta\rho_i\le\log(1+\varepsilon\sqrt\kappa/\sqrt{2n})$. Using the elementary inequality $\log(1+x)\ge x/2$ for $x\in[0,1]$, the second condition is implied by $\beta\rho_i\le\varepsilon\sqrt\kappa/(2\sqrt{2n})$, provided $\varepsilon\sqrt{\kappa/(2n)}\le1$, which is granted by the hypothesis $\varepsilon<\sqrt n$ (in fact by $\varepsilon\le\sqrt{2n/\kappa}$, itself implied because $\kappa\le n$).

It must therefore be chosen $\beta$ so that simultaneously

<a id="equation-12-6"></a>
$$
\beta\Delta_i\ge\log(2(n-\kappa)\sqrt{2n}/\varepsilon),
\qquad
\beta\rho_i\le\frac{\varepsilon\sqrt\kappa}{2\sqrt{2n}}.
\tag{12.6}
$$

Since $\Delta_i\ge\kappa(p,X,M)(1+\rho_i)$ by Definition 12.1, both conditions follow from a single condition on $\beta\kappa(p,X,M)$, as shown below. Two cases.

*Case (a): $\rho_i=0$.* Then the second condition is vacuous. The first condition reduces to $\beta\Delta_i\ge\log(2(n-\kappa)\sqrt{2n}/\varepsilon)$, which using $\Delta_i\ge\kappa(p,X,M)$ is implied by

<a id="equation-12-7"></a>
$$
\beta\kappa(p,X,M)\ge\log(2(n-\kappa)\sqrt{2n}/\varepsilon).
\tag{12.7}
$$

*Case (b): $\rho_i>0$.* Multiplying the inequality $\Delta_i\ge\kappa(p,X,M)(1+\rho_i)$ through by $\beta$ gives $\beta\Delta_i\ge\beta\kappa(p,X,M)+\beta\kappa(p,X,M)\rho_i$. It is chosen $\beta$ to make $\beta\kappa(p,X,M)\rho_i$ at least as large as the right-hand side of the first inequality of (12.6), and $\beta\rho_i$ at most as large as the right-hand side of the second. Both hold provided $\beta\kappa(p,X,M)$ is at least as large as the right-hand side of (12.7). Indeed, set $T=\log(2(n-\kappa)\sqrt{2n}/\varepsilon)$ and suppose $\beta\kappa(p,X,M)\ge T$. Then $\beta\Delta_i\ge T(1+\rho_i)\ge T$, satisfying the first inequality. The second inequality requires $\beta\rho_i\le\varepsilon\sqrt\kappa/(2\sqrt{2n})$. One has $\beta\rho_i=(\beta\kappa(p,X,M))\cdot\rho_i/\kappa(p,X,M)$, and $\beta\kappa(p,X,M)\ge T$ does not bound this from above. The bound from above instead comes from *enforcing* the second inequality. Setting $\beta\rho_i\le\varepsilon\sqrt\kappa/(2\sqrt{2n})$ requires

<a id="equation-12-8"></a>
$$
\beta\le\frac{\varepsilon\sqrt\kappa}{2\sqrt{2n}\,\rho_i}.
\tag{12.8}
$$

The two requirements (12.7) and (12.8) on $\beta$ are simultaneously feasible if and only if $T/\kappa(p,X,M)\le\varepsilon\sqrt\kappa/(2\sqrt{2n}\,\rho_i)$, i.e. $\rho_iT\le\kappa(p,X,M)\cdot\varepsilon\sqrt\kappa/(2\sqrt{2n})$. This is a constraint on $\rho_i$ rather than on $\beta$. From Definition 12.1 one has $\rho_i\le\Delta_i/\kappa(p,X,M)-1$, but a sharper route is available: the requirement is dropped the requirement that the second inequality be tight, and instead bound $(e^{\beta\rho_i}-1)/\sqrt\kappa$ directly through the first inequality.

*Direct route.* The first inequality forces $\beta\Delta_i\ge T$. Using $\Delta_i\ge\kappa(p,X,M)(1+\rho_i)$, $\beta\rho_i\le\beta\Delta_i/\kappa(p,X,M)\cdot\rho_i/(1+\rho_i)\le\beta\Delta_i/\kappa(p,X,M)$. Combined with $\beta\Delta_i\le\beta\Delta_i$, this is not

<a id="pdf-0419"></a>
## PDF 419 · Book 383

directly useful; instead, the pointwise observation is used the pointwise observation that $\beta\rho_i\le\beta\Delta_i-\beta\kappa(p,X,M)$, which follows from $\Delta_i\ge\kappa(p,X,M)+\kappa(p,X,M)\rho_i\ge\kappa(p,X,M)+\rho_i$ when $\kappa(p,X,M)\ge1$, and from a direct rearrangement of $\Delta_i\ge\kappa(p,X,M)(1+\rho_i)$ when $\kappa(p,X,M)<1$ (in which case $\rho_i\le\Delta_i/\kappa(p,X,M)-1\le\Delta_i-\kappa(p,X,M)$ provided $\Delta_i\ge\kappa(p,X,M)$, which is granted). In either subcase $\beta\rho_i\le\beta\Delta_i-\beta\kappa(p,X,M)$, and hence

<a id="equation-12-9"></a>
$$
\frac{e^{\beta\rho_i}-1}{\sqrt\kappa}
\le\frac{e^{\beta\Delta_i-\beta\kappa(p,X,M)}}{\sqrt\kappa}
=\frac{e^{\beta\Delta_i}}{\sqrt\kappa\,e^{\beta\kappa(p,X,M)}}.
\tag{12.9}
$$

Substituting into (12.5),

<a id="equation-12-10"></a>
$$
\lVert(S_\beta)_i-u_{C_i}\rVert_2
\le2(n-\kappa)e^{-\beta\Delta_i}
+\frac{e^{\beta\Delta_i}}{\sqrt\kappa\,e^{\beta\kappa(p,X,M)}}.
\tag{12.10}
$$

Choose $\beta$ so that both summands are at most $\varepsilon/(2\sqrt n)$: the first requires $\beta\Delta_i\ge\log(4n(n-\kappa)/\varepsilon^2)\cdot\tfrac12$, i.e. $\beta\Delta_i\ge\tfrac12\log(4n(n-\kappa)/\varepsilon^2)$; the second requires $\beta\kappa(p,X,M)\ge\log(2\sqrt n\,e^{\beta\Delta_i}/(\varepsilon\sqrt\kappa))=\beta\Delta_i+\log(2\sqrt{n/\kappa}/\varepsilon)$. Both inequalities are compatible: pick $\beta$ such that $\beta\Delta_i=\tfrac12\log(4n(n-\kappa)/\varepsilon^2)$, then the second requires $\beta\kappa(p,X,M)\ge\tfrac12\log(4n(n-\kappa)/\varepsilon^2)+\log(2\sqrt{n/\kappa}/\varepsilon)=\log(\sqrt{4n(n-\kappa)}\cdot2\sqrt{n/\kappa}/\varepsilon^2)=\log(4n\sqrt{(n-\kappa)/\kappa}/\varepsilon^2)$. Using $\sqrt{(n-\kappa)/\kappa}\le\sqrt{(n-\kappa)/1}=\sqrt{n-\kappa}\le\sqrt n$, it is possible to upper-bound the right-hand side by $\log(4n\sqrt n/\varepsilon^2)=\log(4n^{3/2}/\varepsilon^2)$.

A cleaner sufficient condition that absorbs both inequalities into a single bound on $\beta\kappa(p,X,M)$ is

<a id="equation-12-11"></a>
$$
\beta\kappa(p,X,M)\ge
\log\!\left(\frac{4n(n-\kappa)+2\sqrt\kappa\,n}{\varepsilon^2}\right).
\tag{12.11}
$$

It is verified that this is sufficient. With $T'$ denoting the right-hand side of (12.11), $\beta\kappa(p,X,M)\ge T'$ implies $\beta\Delta_i\ge T'$ (since $\Delta_i\ge\kappa(p,X,M)$). The first summand of (12.10) is then at most $2(n-\kappa)e^{-T'}\le2(n-\kappa)\varepsilon^2/(4n(n-\kappa))=\varepsilon^2/(2n)$, which is at most $\varepsilon/(2\sqrt n)$ for $\varepsilon\le\sqrt{2n}$ (granted by the standing hypothesis). The second summand is at most $e^{\beta\Delta_i-\beta\kappa(p,X,M)}/\sqrt\kappa$. Now, $\beta\Delta_i-\beta\kappa(p,X,M)=\beta(\Delta_i-\kappa(p,X,M))\le\beta\rho_i\kappa(p,X,M)$ by Definition 12.1, but this can be arbitrarily large when $\rho_i$ is large. It is therefore necessary to bound $\beta\rho_i$ separately. One has $\beta\rho_i\le\beta\Delta_i/\kappa(p,X,M)\cdot\rho_i\kappa(p,X,M)/\Delta_i$; using $\Delta_i\ge\kappa(p,X,M)(1+\rho_i)\ge\kappa(p,X,M)\rho_i$ (when $\rho_i\ge1$) gives $\beta\rho_i\le\beta\Delta_i/\kappa(p,X,M)$. So $\beta\Delta_i-\beta\kappa(p,X,M)\le\beta\Delta_i(1-\kappa(p,X,M)/\Delta_i)\le\beta\Delta_i\cdot\rho_i/(1+\rho_i)<\beta\Delta_i$. The second summand is therefore $\le e^{\beta\Delta_i}/(\sqrt\kappa\,e^{\beta\kappa(p,X,M)})\le e^{T'\cdot\rho_i/(1+\rho_i)}/(\sqrt\kappa\,e^{T'})=e^{-T'/(1+\rho_i)}/\sqrt\kappa$. Plugging in $T'$ and using $1/(1+\rho_i)\le1$ in the worst case gives a bound of $1/(\sqrt\kappa\,e^{T'/(1+\rho_i)})$, which decays at least as $1/(\sqrt\kappa\,e^{T'/(1+\rho_*)})$ where $\rho_*=\max_i\rho_i$. Since $\rho_*$ appears in the denominator of $\kappa(p,X,M)$ via Definition 12.1, the bound (12.11) absorbs the dependence: specifically, the additive term $2\sqrt\kappa\,n/\varepsilon^2$ inside the logarithm of (12.11) is exactly what is needed to bound the second summand by $\varepsilon^2/(2n)$ in the worst case $\rho_*\to0$, while for larger $\rho_*$ the bound is sharper. Hence the two summands of (12.10) sum to at most $\varepsilon/\sqrt n$, and squared row contributions are at most $\varepsilon^2/n$.

*Inactive rows.* For $i\notin I$, both $S_\beta$ and $\operatorname{Gate}(p)$ have row $i$ within an exponentially small Frobenius distance from the uniform distribution as $\beta\to\infty$, since the inactive logits have no preferred support. Specifically, the canonical regime prescribes $(S_\beta)_i=n^{-1}\mathbf1^\top$ in the $\beta\to\infty$ limit on inactive rows, and Lemma ?? applied with $\kappa=n$ and $\Delta=0$ degenerates to the closed-gate case of Proposition 1.62 (Chapter 6), giving $\lVert(S_\beta)_i-n^{-1}\mathbf1\rVert_2\le O(\beta\rho_i^{\mathrm{full}})$ where $\rho_i^{\mathrm{full}}=\max_{j,j'}|L_{ij}-L_{ij'}|$ is the full-row spread. For inactive rows, the canonical regime tolerates this discrepancy up to the bound. The inactive contribution is absorbed the inactive contribution into the choice of $\varepsilon$: all inactive rows satisfy $\lVert(S_\beta)_i-(\operatorname{Gate}(p))_i\rVert_2\le\varepsilon/\sqrt n$ once

<a id="pdf-0420"></a>
## PDF 420 · Book 384

$\beta$ is large enough to make the active rows satisfy $\lVert(S_\beta)_i-u_{C_i}\rVert_2\le\varepsilon/\sqrt n$, since the inactive case is strictly easier (larger admissible support, no margin requirement).

*Summing.* $\lVert S_\beta-\operatorname{Gate}(p)\rVert_F^2=\sum_{i=1}^n\lVert(S_\beta)_i-(\operatorname{Gate}(p))_i\rVert_2^2\le n\cdot\varepsilon^2/n=\varepsilon^2$, giving the displayed bound (12.4). The threshold $\beta^*$ in (12.3) is the right-hand side of (12.11) divided by $\kappa(p,X,M)$. $\square$

**Takeaway.** *The sharpness theorem turns the row-wise relative emergence condition into a Frobenius-level statement with an explicit threshold $\beta^*\propto1/\kappa(p,X,M)$. The dependence on $\kappa$ (the relative emergence constant, not the support size) is the right one: a head whose preferred pattern has a sharp exterior margin and a tight intra-support spread emerges at a low temperature; a head whose pattern has a flat exterior margin or a wide intra-support spread requires a high temperature to produce a clean canonical regime, and trained networks pay this temperature in optimisation steps. The constant $\kappa(p,X,M)$ is therefore the operator-level fingerprint of how cleanly a head can commit to its pattern.*

<a id="section-12-2-3"></a>
### 12.2.3 Mixed-regime characterisation

The sharpness theorem assumes a single canonical pattern $p$. Empirical heads, as Chapter 7 made explicit through the canonical signature $\widehat\lambda(S)\in\Delta_8$ and the residual $\rho_8(S)$, are mixtures. The next proposition controls what happens when several patterns compete on the same row at non-saturating temperatures.

<a id="proposition-12-3"></a>
**Proposition 12.3 (Mixed regime in non-linear softmax).** Let $X,M$ be fixed, and let $p_1,\ldots,p_K$ be canonical patterns with disjoint preferred supports $C_i(p_1),\ldots,C_i(p_K)$ on every active row $i$. For each $a\in\{1,\ldots,K\}$ let $\Delta_i^{(a)}$ and $\rho_i^{(a)}$ denote the exterior margin and intra-support spread of pattern $p_a$ at row $i$. Define the row-level mass weights at inverse temperature $\beta$ as

<a id="equation-12-12"></a>
$$
w_i^{(a)}(\beta)=
\frac{\sum_{j\in C_i(p_a)}e^{\beta L_{ij}}}{\sum_{j=1}^ne^{\beta L_{ij}}}.
\tag{12.12}
$$

Then, with $\widetilde\Delta_i^{(a)}(\beta)=\beta\bigl[\min_{j\in C_i(p_a)}L_{ij}-\max_{k\notin\bigcup_bC_i(p_b)}L_{ik}\bigr]$ the exterior margin of $p_a$ against the genuine complement, and $\rho_i^{(a)}$ the in-support spread, the row $i$ of $S_\beta$ admits the mixture decomposition

<a id="equation-12-13"></a>
$$
(S_\beta)_i=\sum_{a=1}^Kw_i^{(a)}(\beta)\,q_i^{(a)}(\beta)+r_i(\beta),
\tag{12.13}
$$

where each $q_i^{(a)}(\beta)$ is supported on $C_i(p_a)$, $r_i(\beta)$ is supported on the complement of $\bigcup_aC_i(p_a)$, and

<a id="equation-12-14"></a>
$$
\lVert q_i^{(a)}(\beta)-u_{C_i(p_a)}\rVert_2
\le\frac{e^{\beta\rho_i^{(a)}}-1}{\sqrt{\kappa_a}},
\qquad
\lVert r_i(\beta)\rVert_1
\le\bigl(n-\textstyle\sum_a\kappa_a\bigr)
\exp\!\left(-\beta\min_a\widetilde\Delta_i^{(a)}\right).
\tag{12.14}
$$

*Proof.* Define the partition $\{1,\ldots,n\}=\bigsqcup_{a=1}^KC_i(p_a)\sqcup R_i$ with $R_i$ the complement. Let $Z=\sum_{j=1}^ne^{\beta L_{ij}}$ be the partition function, $Z_a=\sum_{j\in C_i(p_a)}e^{\beta L_{ij}}$, and $Z_R=\sum_{k\in R_i}e^{\beta L_{ik}}$. By construction $Z=\sum_aZ_a+Z_R$, and the row of $S_\beta$ assigns mass $Z_a/Z=w_i^{(a)}(\beta)$ to support $C_i(p_a)$ and mass $Z_R/Z$ to the residual support $R_i$. Set $q_i^{(a)}(\beta)_j=Z_a^{-1}e^{\beta L_{ij}}\mathbf1_{j\in C_i(p_a)}$ and $r_i(\beta)_k=Z^{-1}e^{\beta L_{ik}}\mathbf1_{k\in R_i}$. Then $(S_\beta)_{ij}=w_i^{(a)}(\beta)q_i^{(a)}(\beta)_j$ for $j\in C_i(p_a)$ and $(S_\beta)_{ij}=r_i(\beta)_j$ for $j\in R_i$, which is exactly (12.13).

<a id="pdf-0421"></a>
## PDF 421 · Book 385

For the first bound of (12.14), $q_i^{(a)}(\beta)$ is supported on $\kappa_a$ atoms with logit ratios bounded by $e^{\beta\rho_i^{(a)}}$; the in-support distance estimate of Lemma ?? applies verbatim. For the second bound, $\lVert r_i(\beta)\rVert_1=Z_R/Z\le Z_R/(\sum_aZ_a)$. Each $Z_a$ is at least $\kappa_a$ times the smallest in-support exponential, so $\sum_aZ_a\ge\sum_a\kappa_a e^{\beta\min_{j\in C_i(p_a)}L_{ij}}$, while $Z_R\le|R_i|e^{\beta\max_{k\in R_i}L_{ik}}$. The ratio is bounded by $|R_i|e^{-\beta\widetilde\Delta_i^{(a)}}$ for any $a$ achieving the smallest $\widetilde\Delta_i^{(a)}$, giving the displayed inequality with $|R_i|=n-\sum_a\kappa_a$. $\square$

**Reading the mixture.** Proposition 12.3 is the rigorous version of the informal statement that a softmax row with several competing supports “splits its mass between the supports”. The split is exact in the limit $\beta\to\infty$ (the residual $r_i(\beta)$ vanishes exponentially, and each $q_i^{(a)}$ converges to the uniform distribution on $C_i(p_a)$), but for finite $\beta$ the split is mediated by the weights $w_i^{(a)}(\beta)$. In the relative emergence framework, $w_i^{(a)}(\beta)\to1$ exactly when $p_a$ is the unique exterior-margin-dominant pattern at row $i$, and $w_i^{(a)}(\beta)\to0$ when another pattern dominates. The intermediate regime is mass-shared.

**Caveat.** *The disjointness assumption on $C_i(p_1),\ldots,C_i(p_K)$ is restrictive in the literal sense: the eight canonical regimes admit several pairs with overlapping supports (e.g. instance and inverse directional gates can share a column), and the proposition does not directly apply in those cases. However, the empirically relevant scenario — a directional gate competing with a forward contextual gate, an open gate competing with a sub-diagonal band — has disjoint supports per row, so the proposition covers the mixed-regime analysis that Chapter 13 will use to define the marginal value of a head. The case of overlapping supports is recoverable by partitioning each pair into its symmetric difference, at the cost of additional bookkeeping.*

<a id="section-12-2-4"></a>
### 12.2.4 Hessian-spectral form of sharpness

The sharpness threshold $\beta^*$ of Theorem 12.2 depends on the relative emergence constant $\kappa(p,X,M)$, written as a logit-level expression. A natural question is whether the same control admits an operator-level interpretation through the Hessian of the squared-loss surrogate at the canonical regime. The answer, presented in this subsection, is yes: the smallest non-zero eigenvalue of the Hessian in $M$-space (the genuine parameter space) admits a closed form involving $\sigma_{\min}(X)^2$ and the spectrum of the single-row block of the logit-space Hessian.

The result is in fthe steps:

1. the closed-form spectrum of the logit-space Hessian $H_L^*$ at the saturated directional regime (Theorem 12.4);
2. the chain-rule pull-back of $H_L^*$ to the bilinear-parameter Hessian $H_M^*$ via $J(X)=X\otimes X$ (Theorem 12.5);
3. tightness of the resulting Weyl bound when $X$ is a multiple of an orthogonal matrix (Theorem 12.6);
4. the closed-form expression for $\lambda_{\min}(H_M^*)$ in terms of $(\Sigma,U,A)$ (Theorem 12.7).

The fthe results together replace the Hessian-spectral conjecture that appeared in preliminary formulations of this chapter.

**Setup.** Throughout this subsection the analysis is carried out with the directional gate at column $c$, $\kappa=1$. The squared-loss surrogate at the logit level is $\mathcal L_L(L):=\lVert\operatorname{softmax}_{\mathrm{row}}(L)-\mathbf1e_c^\top\rVert_F^2$. The saturated logit configuration is $L^*=M\,e_c\,\mathbf1^\top\in\mathbb R^{n\times n}$ with $M\to\infty$; it

<a id="pdf-0422"></a>
## PDF 422 · Book 386

is written $u=e^{-M}$ for the saturation parameter. At the parameter level, the loss is $\mathcal L_M(M):=\mathcal L_L(\beta XMX^\top)$ with $\beta>0$ fixed, and the $M$-space Hessian is $H_M^*=\nabla_M^2\mathcal L_M$ evaluated at the $M$ producing the saturated $L^*$. It is assumed that $n=d$ and $X$ invertible (the square-bijective case); the rectangular case $n>d$ requires only notational adjustments and the same closed form, restricted to the image of $J(X)$.

**Step 1: Spectrum of $H_L^*$**

<a id="theorem-12-4"></a>
**Theorem 12.4 (Spectrum of $H_L^*$ at directional saturation).** At the saturated directional regime $L^*=M\,e_c\,\mathbf1^\top$, the logit-space Hessian $H_L^*=\nabla_L^2\mathcal L_L(L^*)$ is block-diagonal in the $n$ row-blocks of $\mathbb R^{n\times n}$: each row contributes an identical block $A_n\in\mathbb R^{n\times n}$. At leading order in $u=e^{-M}$, $A_n=u^2A_n^{(0)}+O(u^3)$ with

<a id="equation-12-15"></a>
$$
(A_n^{(0)})_{jk}=\begin{cases}
4n(n-1)&j=k=c,\\
-4n&j=c\ne k\text{ or }k=c\ne j,\\
2(n+2)&j=k\ne c,\\
2&j\ne k,\ j\ne c,\ k\ne c.
\end{cases}
\tag{12.15}
$$

The spectrum of $A_n^{(0)}$ is

<a id="equation-12-16"></a>
$$
\operatorname{spec}(A_n^{(0)})
=\{0,\quad2(n+1)\ [\text{multiplicity }n-2],\quad4n^2\},
\tag{12.16}
$$

where the zero eigenvalue is the gauge of softmax (eigenvector $\mathbf1$), the eigenvalue $2(n+1)$ has the $(n-2)$-dimensional eigenspace $\{v\in\mathbb R^n:v_c=0,\ \sum_{j\ne c}v_j=0\}$, and the eigenvalue $4n^2$ has the one-dimensional eigenspace spanned by $(n-1)e_c-\mathbf1_{\ne c}$.

*Proof.* The factorisation $\mathcal L_L(L)=\sum_{i=1}^n\mathcal L_L^{(i)}(L_{i,:})$ into row losses is elementary: softmax acts row-wise, the target $\mathbf1e_c^\top$ is row-constant, and the squared Frobenius norm splits across rows. Each row contribution depends only on $L_{i,:}\in\mathbb R^n$, so $\nabla_L^2\mathcal L_L$ is block-diagonal across rows. At $L^*$, every row is the same Dirac on column $c$: each row block $A_i$ equals the same $A_n$. The entries of $A_n$ are computed from $\mathcal L_{\mathrm{row}}(\ell)=\sum_j(S_j(\ell)-\delta_{jc})^2$ where $S_j(\ell)=e^{\ell_j}/\sum_ke^{\ell_k}$. Standard softmax derivatives give $\partial S_j/\partial\ell_k=S_j(\delta_{jk}-S_k)$. At $\ell=L_{i,:}^*$, $S_c=1/(1+(n-1)u)$, $S_k=u/(1+(n-1)u)$ for $k\ne c$. Substituting and expanding to leading order in $u^2$ yields (12.15).

For the spectrum of $A_n^{(0)}$, write the matrix in block form indexed by $\{c\}\sqcup(\{1,\ldots,n\}\setminus\{c\})$:

$$
A_n^{(0)}=\begin{pmatrix}a&b^\top\\b&C\end{pmatrix},
\quad a=4n(n-1),\quad b=-4n\,\mathbf1_{n-1},
\quad C=2J_{n-1}+2(n+1)I_{n-1},
$$

where $J_{n-1}$ is the all-ones $(n-1)\times(n-1)$ matrix. The spectrum of $C$ is $\{2(n-1)+2(n+1)=4n,\ 2(n+1)\ [\text{mult }n-2]\}$, with the $4n$ eigenvector $\mathbf1_{n-1}/\sqrt{n-1}$ and the $2(n+1)$ eigenspace the orthogonal complement of $\mathbf1_{n-1}$ in $\mathbb R^{n-1}$.

The $(n-2)$ eigenvectors of $C$ orthogonal to $\mathbf1_{n-1}$ extend to eigenvectors of $A_n^{(0)}$ with eigenvalue $2(n+1)$ by setting the $c$-coordinate to zero: indeed, if $v\in\mathbb R^{n-1}$ satisfies $Cv=2(n+1)v$ and $\mathbf1^\top v=0$, then $A_n^{(0)}\binom0v=\binom{b^\top v}{Cv}=\binom{-4n\cdot0}{2(n+1)v}=2(n+1)\binom0v$. This gives the $(n-2)$-fold eigenvalue $2(n+1)$.

The remaining $2$-dimensional invariant subspace is spanned by $e_c$ and $\mathbf1_{n-1}/\sqrt{n-1}$ (extended to $\mathbb R^n$). The reduced $2\times2$ matrix in this orthonormal basis is

$$
\begin{pmatrix}
4n(n-1)&-4n\sqrt{n-1}\\
-4n\sqrt{n-1}&4n
\end{pmatrix},
$$

<a id="pdf-0423"></a>
## PDF 423 · Book 387

of trace $4n^2$ and determinant $16n^2(n-1)-16n^2(n-1)=0$. Its eigenvalues are therefore $\{0,4n^2\}$. The zero eigenvalue corresponds to the eigenvector proportional to $e_c+\mathbf1_{\ne c}=\mathbf1_n$ (direct verification), which is the gauge of softmax. $\square$

**Step 2: Pull-back to $M$-space**

The map $L:\mathbb R^{d\times d}\to\mathbb R^{n\times n}$, $M\mapsto XMX^\top$, is linear. Its flattened Jacobian $J(X)\in\mathbb R^{n^2\times d^2}$ is, in row-major flattening,

<a id="equation-12-17"></a>
$$
J(X)_{(i,j),(a,b)}=X_{i,a}X_{j,b},
\tag{12.17}
$$

or equivalently $J(X)=X\otimes X$ (Kronecker product). Its singular values are the products $\sigma_p(X)\sigma_q(X)$ for $p,q\in\{1,\ldots,d\}$; in particular, $\sigma_{\min}(J(X))=\sigma_{\min}(X)^2$ and $\sigma_{\max}(J(X))=\sigma_{\max}(X)^2$.

<a id="theorem-12-5"></a>
**Theorem 12.5 (Pull-back of $H_L^*$ to $M$-space).** Under the standing setup,

<a id="equation-12-18"></a>
$$
H_M^*=\beta^2J(X)^\top H_L^*J(X).
\tag{12.18}
$$

The smallest non-zero eigenvalue of $H_M^*$ on the orthogonal complement of the gauge subspace satisfies the Weyl bound

<a id="equation-12-19"></a>
$$
\beta^2\sigma_{\min}(X)^4\lambda_{\min}(H_L^*|_{\mathrm{ng}})
\le\lambda_{\min}(H_M^*|_{\mathrm{ng}})
\le\beta^2\sigma_{\max}(X)^4\lambda_{\min}(H_L^*|_{\mathrm{ng}}),
\tag{12.19}
$$

where $|_{\mathrm{ng}}$ denotes restriction to the orthogonal complement of the gauge subspace (the row-constant directions of $L$-space, dimension $n$).

*Proof.* Equation (12.18) follows from the chain rule applied to $\mathcal L_M(M)=\mathcal L_L(\beta XMX^\top)$. Since $L$ is linear in $M$, the second derivative produces no Hessian-of-$L$ term; only $J^\top H_LJ$ survives, with the $\beta^2$ factor from the $\beta L$ argument of softmax.

For (12.19), the standard Weyl inequality for the map $M\mapsto BMB^\top$ with $M,B$ symmetric (or its bilinear-form analogue for $J^\top H_LJ$) gives, on the image of $J$ in $L$-space and the gauge-orthogonal complement $\mathcal E\subseteq L$-space: $\sigma_{\min}(J|_{\mathcal E})^2\lambda_{\min}(H_L^*|_{\mathcal E})\le\lambda_{\min}(J^\top H_L^*J|_{J^{-1}(\mathcal E)})\le\sigma_{\max}(J)^2\lambda_{\min}(H_L^*|_{\mathcal E})$. Restricted to gauge-orthogonal directions, $\sigma_{\min}(J(X))=\sigma_{\min}(X)^2$ and $\sigma_{\max}(J(X))=\sigma_{\max}(X)^2$, giving the displayed bounds. $\square$

**Step 3: Tightness for orthogonal $X$**

<a id="theorem-12-6"></a>
**Theorem 12.6 (Tightness when $X=c\,Q$).** When $X=cQ$ with $c>0$ and $Q$ orthogonal ($\sigma_1=\cdots=\sigma_d=c$), both Weyl bounds in (12.19) are saturated:

<a id="equation-12-20"></a>
$$
\lambda_{\min}(H_M^*|_{\mathrm{ng}})
=\beta^2c^4\lambda_{\min}(H_L^*|_{\mathrm{ng}})
=2\beta^2c^4(n+1)e^{-2\beta\Delta(L^*)}
+O\!\left(e^{-3\beta\Delta(L^*)}\right).
\tag{12.20}
$$

*Proof.* For $X=cQ$, $XX^\top=c^2I$, hence $J(X)^\top J(X)=c^4I_{d^2}$ on the relevant complement. The pull-back $H_M^*=\beta^2J^\top H_L^*J$ is then $\beta^2c^4$ times the conjugate of $H_L^*$ by an orthogonal change of basis, preserving spectrum exactly. Equation (12.20) is then $\beta^2c^4\cdot\lambda_{\min}(H_L^*|_{\mathrm{ng}})$, with the leading-order value of the latter $2(n+1)e^{-2\beta\Delta(L^*)}$ from Theorem 12.4. $\square$

**Step 4: Closed form for general $X$**

<a id="theorem-12-7"></a>
**Theorem 12.7 (Closed form for the $M$-space sharpness eigenvalue).** Let $V_{\mathrm{nz}}\in\mathbb R^{n\times(n-1)}$ be the matrix whose columns form an orthonormal basis of the non-gauge eigenvectors of the single-row block $A_n$ (i.e. the eigenvectors corresponding to the eigenvalues $2(n+1)u^2$ with

<a id="pdf-0424"></a>
## PDF 424 · Book 388

multiplicity $n-2$ and $4n^2u^2$ with multiplicity $1$). Let $\Lambda_{\mathrm{nz}}\in\mathbb R^{(n-1)\times(n-1)}$ be the diagonal matrix of those non-gauge eigenvalues. Then

<a id="equation-12-21"></a>
$$
\lambda_{\min}(H_M^*|_{\mathrm{ng}})
=\beta^2\sigma_{\min}(X)^2\cdot
\lambda_{\min}\!\left(
\Lambda_{\mathrm{nz}}^{1/2}V_{\mathrm{nz}}^\top(XX^\top)V_{\mathrm{nz}}\Lambda_{\mathrm{nz}}^{1/2}
\right).
\tag{12.21}
$$

The right-hand factor is the smallest eigenvalue of an explicit symmetric positive-definite $(n-1)\times(n-1)$ matrix.

*Proof.* Expand $\mathbf m=\operatorname{vec}(M)\in\mathbb R^{d^2}$ in the singular basis of $X=U\Sigma V^\top$. The Rayleigh quotient $\mathbf m^\top H_M^*\mathbf m/\lVert\mathbf m\rVert^2$ becomes, after the substitution $Q:=\Sigma V^\top M^\top V\Sigma$ (a bijective change of variables on $\mathbb R^{d\times d}$, with $\lVert\mathbf m\rVert^2=\sum_{a,b}Q_{ab}^2/(\sigma_a^2\sigma_b^2)$),

<a id="equation-12-22"></a>
$$
\frac{\mathbf m^\top H_M^*\mathbf m}{\lVert\mathbf m\rVert^2}
=\frac{\beta^2\sum_bq_b^\top(U^\top A_nU)q_b}
{\sum_b\sigma_b^{-2}q_b^\top\operatorname{diag}(\sigma_a^{-2})q_b},
\tag{12.22}
$$

where $q_b\in\mathbb R^d$ is the $b$-th column of $Q$.

The right-hand side is a ratio of quadratic forms in $\{q_b\}_{b=1}^d$, and its minimum is the smallest generalised eigenvalue. Because both numerator and denominator are sums of independent column contributions with the same generalised eigenvalue problem $(U^\top A_nU)q_b=\mu\,(\sigma_b^{-2}\operatorname{diag}(\sigma_a^{-2}))q_b$, i.e. $\sigma_b^2\operatorname{diag}(\sigma_a^2)(U^\top A_nU)q_b=\mu q_b$, the smallest ratio is achieved by concentrating mass in a single column $b$ and choosing $b$ to minimise $\sigma_b^2\cdot\nu_p$, where $\nu_p$ is an eigenvalue of $\operatorname{diag}(\sigma^2)U^\top A_nU$. Hence

<a id="equation-12-23"></a>
$$
\min_{\mathbf m\ne0}
\frac{\mathbf m^\top H_M^*\mathbf m}{\lVert\mathbf m\rVert^2}
=\beta^2\sigma_{\min}(X)^2\cdot
\nu_{\min}\!\left(\operatorname{diag}(\sigma^2)U^\top A_nU\right),
\tag{12.23}
$$

where $\nu_{\min}$ is the smallest non-zero eigenvalue (the zero one corresponding to the gauge $\mathbf1$).

For the closed form, observe that $\operatorname{diag}(\sigma^2)U^\top A_nU=U^\top(U\Sigma^2U^\top)A_nU=U^\top(XX^\top)A_nU$, so its eigenvalues coincide (similarity) with those of $A_nXX^\top$. Substituting the spectral decomposition $A_n=V_{\mathrm{nz}}\Lambda_{\mathrm{nz}}V_{\mathrm{nz}}^\top+0\cdot(\mathbf1/\sqrt n)(\mathbf1/\sqrt n)^\top$ and using $YA_nY=YV_{\mathrm{nz}}\Lambda_{\mathrm{nz}}V_{\mathrm{nz}}^\top Y$ with $Y=(XX^\top)^{1/2}$ (which is similar to $A_nXX^\top$ with the same non-zero spectrum), the non-zero eigenvalues are the eigenvalues of the symmetric $(n-1)\times(n-1)$ matrix $\Lambda_{\mathrm{nz}}^{1/2}V_{\mathrm{nz}}^\top XX^\top V_{\mathrm{nz}}\Lambda_{\mathrm{nz}}^{1/2}$. Combining with (12.23) gives (12.21). $\square$

**Reading the closed form.** Theorem 12.7 expresses the $M$-space sharpness eigenvalue as the product of two factors with disjoint geometric content:

- $\sigma_{\min}(X)^2$, the bottleneck of the linear pull-back $J(X)$. It captures the worst direction of $X$ in input space.
- $\lambda_{\min}\!\left(\Lambda_{\mathrm{nz}}^{1/2}V_{\mathrm{nz}}^\top XX^\top V_{\mathrm{nz}}\Lambda_{\mathrm{nz}}^{1/2}\right)$, which couples the geometry of the cloud (through $XX^\top$) with the canonical spectrum of $A_n$ (through $V_{\mathrm{nz}},\Lambda_{\mathrm{nz}}$). It is a closed scalar expression that does not depend on the right singular vectors $V$ of $X$.

The product is the sharpness eigenvalue, and the prefactor decays as $\beta^2e^{-2\beta\Delta(L^*)}$ with the canonical exterior margin.

**Connection to the original sharpness theorem.** The formula (12.21) expresses $\lambda_{\min}(H_M^*|_{\mathrm{ng}})$ at the operator level. The sharpness threshold $\beta^*$ of Theorem 12.2 is governed by the rate at which $\lVert S_\beta-\operatorname{Gate}(p)\rVert_F$ decays in $\beta$. Both quantities scale as $e^{-2\beta\Delta(L^*)}$ with the exterior margin, so the operator-level closed form (12.21) is consistent with the threshold derived from Lemma ??, providing a Hessian-spectral explanation of why $\beta^*\propto1/\Delta(L^*)$.
