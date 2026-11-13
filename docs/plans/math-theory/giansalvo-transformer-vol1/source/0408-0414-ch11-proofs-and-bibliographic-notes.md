---
title: "Chapter 11: remaining conclusions, proofs, and bibliographic notes"
source_pdf_pages: [408, 414]
book_pages: [372, 378]
batch: "009"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: false
---

# Chapter 11: remaining conclusions, proofs, and bibliographic notes

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: PDF 401–407](0401-0407-ch11-predictions-coherence-and-experiments.md#pdf-0407) · [Next: PDF 415–416](0415-0416-ch12-orbits-and-sharpness-introduction.md#pdf-0415)

> Editorial note: Full transcription of PDF 408–414 (printed pages 372–378), completing chapter 11. This includes the remainder of §11.14, all of §11.15 and §11.16, Lemma 11.76, Remark 11.77, and equations (11.35)–(11.45). The proof's changing assumptions, trace signs, bounds, effective-rank definition, gradient expression, and unresolved citations are preserved as printed. Russian notes 126–129 assess these claims separately.

<a id="pdf-0408"></a>
## PDF 408 · Book 372

- **Activation-Cone Index Correlation (11.40).**
- **Layer Rank Decay Proposition (11.41).**
- **Monotone Decay of $\Gamma_{\mathrm{res}}$ (11.46)** and **Quantitative Decay Rate (11.47):** reduction per eigenplane step equals $2\rho^{(l)2}$ (corrected from the preliminary $\rho^{(l)2}$; the factor 2 arises from the antisymmetric pair structure and is verified numerically in Exp. 6, Appendix).
- **Termination Corollary (11.49)** and **Heads Grow in Pairs (11.51):** terminates in $\le r_l/2$ eigenplane steps (or $\le r_l$ single-vector steps).

**Forward connections.**

- **Chapter ??** (Pattern-Emergence Theorem) requires the activation sequence $\gamma_h^{(l)}$, the Residual Amplification Proposition, and the attractor geometry of Section 11.5.2 as direct prerequisites.
- **Chapter ??** (Subspace Activation Theory) uses the fixed-point structure of Section 11.5.2 to define the silence manifold.
- **Chapter ??** (Redundancy Quantified) uses the Layer Rank Decay Proposition to quantify the gap between parametric and functional complexity.
- **Chapter ??** (INCRT) operationalises $\Gamma_{\mathrm{res}}^{(l)}$ as the growth criterion, using the spectral structure of $P_\perp XM_aX^\top P_\perp$ to determine the direction of each new head.

<a id="section-11-15"></a>
### 11.15 Proofs

The proofs below are self-contained. A reader primarily interested in the results may read each proposition statement and skip to the next on first reading; the proofs are included to make the constants $c_1,c_2$ and the convergence rates quantitatively explicit.

<a id="section-11-15-1"></a>
### 11.15.1 Proof of Proposition 11.33 (Residual Amplification)

The bound $\Gamma^{(l)}\ge c_1l\,\Gamma^{(0)}-c_2$ under the three hypotheses stated in the proposition.

**Setup and notation.** Fix a head $h$ and write $M_a\equiv M_a^{(h)}$ (suppressing layer superscripts on $M_a$ where no ambiguity arises; the layer-dependence of $X$ is always explicit). Write $f^{(l)}:=\lVert X^{(l)}M_aX^{(l)\top}\rVert_F$ so that $\Gamma^{(l)}=\frac1{H_l}\sum_hf^{(l)}$; it suffices to bound $f^{(l)}$ from below for each head.

By hypothesis, $\lVert M_a\rVert_F\ge m>0$ and $\lVert\Delta^{(l)}\rVert_F\ge\delta>0$ for all $l$. Write $X^{(l)}=X^{(l-1)}+\Delta^{(l)}$.

**Step 1: One-step recurrence.**

Expand using bilinearity of the map $(A,B)\mapsto AM_aB^\top$:

<a id="equation-11-35"></a>
$$
\begin{aligned}
X^{(l)}M_aX^{(l)\top}
&=(X^{(l-1)}+\Delta^{(l)})M_a(X^{(l-1)}+\Delta^{(l)})^\top\\
&=X^{(l-1)}M_aX^{(l-1)\top}
+X^{(l-1)}M_a\Delta^{(l)\top}
+\Delta^{(l)}M_aX^{(l-1)\top}
+\Delta^{(l)}M_a\Delta^{(l)\top}.
\end{aligned}
\tag{11.35}
$$

Taking Frobenius norms and applying the triangle inequality:

<a id="equation-11-36"></a>
$$
f^{(l)}\ge f^{(l-1)}
-2\,\lVert\Delta^{(l)}\rVert_F\lVert X^{(l-1)}\rVert_F\lVert M_a\rVert_F
-\lVert\Delta^{(l)}\rVert_F^2\lVert M_a\rVert_F.
\tag{11.36}
$$

(The cross-terms give $\lVert X^{(l-1)}M_a\Delta^{(l)\top}\rVert_F\le\lVert X^{(l-1)}\rVert_F\lVert M_a\rVert_F\lVert\Delta^{(l)}\rVert_F$ by submultiplicativity, and the antisymmetry of $M_a$ means $\lVert\Delta^{(l)}M_aX^{(l-1)\top}\rVert_F=\lVert X^{(l-1)}M_a\Delta^{(l)\top}\rVert_F$, so the two cross-terms sum to $\le2(\cdots)$.)

<a id="pdf-0409"></a>
## PDF 409 · Book 373

This recurrence shows $f^{(l)}$ can decrease. The positive contribution comes from the cross-terms when $X^{(l-1)}$ and $\Delta^{(l)}$ reinforce each other. The net effect is shown to be positive by working with the *sum* $\sum_lf^{(l)}$ rather than the step-by-step bound.

**Step 2: Growth via the cumulative representation.**

Since $X^{(l)}=X^{(0)}+\sum_{l'=1}^l\Delta^{(l')}$:

<a id="equation-11-37"></a>
$$
\lVert X^{(l)}\rVert_F
\ge\lVert X^{(0)}\rVert_F+\sum_{l'=1}^l\lVert\Delta^{(l')}\rVert_F
-[\text{cancellation terms}].
\tag{11.37}
$$

More precisely, by Proposition ?? (Chapter ??) and the triangle inequality:

$$
\lVert X^{(l)}\rVert_F\ge\lVert X^{(0)}\rVert_F+l\,\delta-B_l,
$$

where $B_l$ accounts for cancellation between updates (bounded: $B_l\le l\cdot\max_{l'}\lVert\Delta^{(l')}\rVert_F=:l\cdot\delta_{\max}$, so $\lVert X^{(l)}\rVert_F\ge\lVert X^{(0)}\rVert_F+l(\delta-\delta_{\max})$; under H1 this is $\ge\lVert X^{(0)}\rVert_F$ which suffices if $\delta_{\max}$ is finite, but a sharper bound is needed).

For a cleaner argument, use instead the identity:

<a id="equation-11-38"></a>
$$
f^{(l)2}=\lVert X^{(l)}M_aX^{(l)\top}\rVert_F^2
\ge\frac{\bigl(\operatorname{tr}(X^{(l)}M_aX^{(l)\top}\cdot S)\bigr)^2}{\lVert S\rVert_F^2}
\tag{11.38}
$$

for any test matrix $S\in\mathfrak{so}(n)$, by the Cauchy-Schwarz inequality for the Frobenius inner product. Choose $S=X^{(0)}M_aX^{(0)\top}/f^{(0)}$ (normalised initial operator). Then:

<a id="equation-11-39"></a>
$$
\begin{aligned}
\operatorname{tr}\bigl(X^{(l)}M_aX^{(l)\top}\cdot S\bigr)
&=\frac1{f^{(0)}}\operatorname{tr}\left[
\left(X^{(0)}+\sum_{l'}\Delta^{(l')}\right)M_a
\left(X^{(0)}+\sum_{l'}\Delta^{(l')}\right)^\top
\cdot X^{(0)}M_aX^{(0)\top}\right]\\
&\ge f^{(0)}+\frac1{f^{(0)}}\sum_{l'=1}^l
\operatorname{tr}\left[\Delta^{(l')}M_aX^{(0)\top}\cdot X^{(0)}M_aX^{(0)\top}\right]-C_1,
\end{aligned}
\tag{11.39}
$$

where $C_1$ collects higher-order terms in $\Delta^{(l')}$ and is bounded independently of $l$.

The key term is $\operatorname{tr}[\Delta^{(l')}M_aX^{(0)\top}\cdot X^{(0)}M_aX^{(0)\top}]$. By hypothesis H3 ($X^{(0)}$ has full row rank, so $X^{(0)}M_aX^{(0)\top}\ne0$) and H2 (updates are non-trivial and not orthogonal to the initial signal on average), this sum grows linearly in $l$ with a positive constant:

$$
\sum_{l'=1}^l\operatorname{tr}\left[
\Delta^{(l')}M_aX^{(0)\top}\cdot X^{(0)}M_aX^{(0)\top}
\right]\ge l\cdot\kappa\,\delta\,f^{(0)},
$$

where $\kappa>0$ depends on the alignment between $\Delta^{(l')}$ and the initial eigenvectors of $M_a$ (non-zero by the generic position hypothesis).

**Step 3: Bounding the higher-order terms $C_1$.**

The expansion (11.39) contains cross-terms of the form $\operatorname{tr}[\Delta^{(l')}M_a\Delta^{(l'')\top}\cdot X^{(0)}M_aX^{(0)\top}]$ for $l'\ne l''$, and quadratic terms $\operatorname{tr}[\Delta^{(l')}M_a\Delta^{(l')\top}\cdot X^{(0)}M_aX^{(0)\top}]$. These must be bounded uniformly in $l$.

*Quadratic terms.* By submultiplicativity and the Cauchy-Schwarz inequality:

$$
\left|\operatorname{tr}\left[
\Delta^{(l')}M_a\Delta^{(l')\top}\cdot X^{(0)}M_aX^{(0)\top}
\right]\right|
\le\lVert\Delta^{(l')}\rVert_F^2\cdot\lVert M_a\rVert_F^2\cdot\lVert X^{(0)}\rVert_F^2.
$$

<a id="pdf-0410"></a>
## PDF 410 · Book 374

Under H1, $\lVert\Delta^{(l')}\rVert_F\le\delta_{\max}<\infty$, so each quadratic term is bounded by $\delta_{\max}^2\lVert M_a\rVert_F^2\lVert X^{(0)}\rVert_F^2$. Summing over $l'=1,\ldots,l$ gives a contribution $O(l)$ — but with a coefficient of order $\delta_{\max}^2$, which is small relative to the linear term $l\kappa\delta f^{(0)}$ when $\kappa\delta\gg\delta_{\max}^2\lVert M_a\rVert_F\lVert X^{(0)}\rVert_F^2/f^{(0)}$.

*Cross terms ($l'\ne l''$, from expanding $(X^{(0)}+\sum\Delta)^2$).* These vanish in the Cauchy-Schwarz lower bound (11.38) because the bound uses $\langle A,S\rangle^2/\lVert S\rVert_F^2\le\lVert A\rVert_F^2$, and the cross-term contributions to $\langle\cdot,S\rangle$ have mixed signs that cancel in expectation (by H2: the updates are not systematically correlated).

More precisely: define $C_1:=\sum_{l'=1}^l\lVert\Delta^{(l')}\rVert_F^2\lVert M_a\rVert_F\lVert X^{(0)}\rVert_F^2/f^{(0)}$. This is bounded by $l\delta_{\max}^2\lVert M_a\rVert_F\lVert X^{(0)}\rVert_F^2/f^{(0)}$, a constant times $l$. The net result of Steps 2–3 is then:

$$
f^{(l)}\ge f^{(0)}
+l\bigl(\kappa\delta-\delta_{\max}^2\lVert M_a\rVert_F\lVert X^{(0)}\rVert_F^2/f^{(0)}\bigr)-C_2,
$$

where $C_2$ is a constant (independent of $l$) bounding the residual terms. The coefficient of $l$ is positive under H2 when $\kappa\delta>\delta_{\max}^2\lVert M_a\rVert_F\lVert X^{(0)}\rVert_F^2/f^{(0)}$, i.e., when the aligned growth is faster than the quadratic loss from unaligned fluctuations. Setting $c_1=(\kappa\delta-\delta_{\max}^2\lVert M_a\rVert_F\lVert X^{(0)}\rVert_F^2/f^{(0)})/\Gamma^{(0)}>0$ and $c_2=C_2$ gives the stated bound. The constant $\kappa$ and hence $c_1$ admit an explicit expression in terms of the Directional Energy Theorem (Chapter ??, Theorem C). Define the *initial directional alignment*:

<a id="equation-11-40"></a>
$$
\kappa:=
\frac{\operatorname{tr}\left[M_a^2\bigl(X^{(0)\top}X^{(0)}\bigr)^2\right]}
{\lVert M_a\rVert_F^2\cdot\lVert X^{(0)}\rVert_F^4}.
\tag{11.40}
$$

This is the Frobenius inner product between the squared score matrix $M_a^2$ and the squared Gram matrix $(X^{(0)\top}X^{(0)})^2$, normalised to lie in $[0,1]$. It measures how well the metric induced by $M_a$ on $\mathbb R^d$ aligns with the representational geometry of the initial token embedding.

The constants are then:

<a id="equation-11-41"></a>
$$
c_1=\frac{\kappa\,\delta}{\Gamma^{(0)}},
\qquad
c_2=\frac{C_1}{(\Gamma^{(0)})^2}+\delta_{\max}^2\lVert M_a\rVert_F,
\tag{11.41}
$$

where $C_1$ is the bound on the higher-order terms in the trace expansion (11.39).

**Interpretation.** The formula (11.40) has a direct semantic reading. For tasks with strong syntactic structure (directional dependencies between tokens), $M_a$ has large projections onto the principal directions of $X^{(0)\top}X^{(0)}$, making $\kappa$ large and the amplification rapid. For purely semantic tasks where $M_s$ dominates and $\lVert M_a\rVert_F/\lVert M\rVert_F$ is small, $\kappa$ is small and the growth rate is slow — consistent with the observation that semantic heads tend to be more uniform and develop later in training.

Substituting back into (11.38)–(11.39):

$$
f^{(l)}\ge f^{(0)}+l\,\kappa\,\delta-C_2,
$$

for a constant $C_2>0$. Setting $c_1=\kappa\,\delta/\Gamma^{(0)}$ and $c_2=C_2$ gives the stated bound with $\Gamma^{(0)}=f^{(0)}$ (up to the normalisation by $H_l$, which is bounded away from 0).

**Remarks on the hypotheses.**

- H1 ($\lVert\Delta^{(l)}\rVert_F\ge\delta>0$) ensures the sequence is non-trivially updating; it fails only if the model reaches an exact loss minimum, at which point $\Gamma^{(l)}$ is also at a fixed point.
- H2 ($\lVert M_a^{(h,l)}\rVert_F\ge m>0$) excludes sequence-silent heads (class (i) of Proposition 11.14). The bound applies to each directional or reciprocal head individually.
- H3 (full row rank of $X^{(0)}$) is generic for random initialisation with $n\le d$ and is guaranteed by the Rank-Neutrality Theorem (Theorem ??, Chapter ??) to be preserved under layer normalisation. $\square$

<a id="pdf-0411"></a>
## PDF 411 · Book 375

<a id="section-11-15-2"></a>
### 11.15.2 Proof of Proposition 11.40 (Activation-Cone Index Correlation)

The goal is to show that $\operatorname{Corr}_l(\gamma_h^{(l)},I_{\mathrm{cone}}^{(l)})>0$ and that $\mathbb E[\gamma_h^{(l)}\mid I_{\mathrm{cone}}^{(l)}]$ is non-decreasing in $I_{\mathrm{cone}}^{(l)}$.

**Shared driving variable.** Both $\gamma_h^{(l)}$ and $I_{\mathrm{cone}}^{(l)}$ are non-decreasing functions of the same underlying quantity: the spectral concentration of the operator $D^{(h,l)}=X^{(l)}M_a^{(h,l)}X^{(l)\top}$. Formally, let $\lambda_1^{(h,l)}\ge\lambda_2^{(h,l)}\ge\cdots\ge0$ be the ordered singular values of $D^{(h,l)}$, and define the *spectral concentration index*:

<a id="equation-11-42"></a>
$$
\rho^{(h,l)}:=\frac{\lambda_1^{(h,l)}}{\sum_k\lambda_k^{(h,l)}}\in[0,1].
\tag{11.42}
$$

This measures how much energy is in the leading singular value. Observe that $\sum_k\lambda_k^{(h,l)}=\lVert D^{(h,l)}\rVert_*=\lVert X^{(l)}M_a^{(h,l)}X^{(l)\top}\rVert_*$ (nuclear norm).

**Monotonicity in $\rho$.**

*(i) $\gamma_h^{(l)}$ is non-decreasing in $\rho^{(h,l)}$:* The asymmetry index $\alpha_h^{(l)}=\lVert M_a^{(h,l)}\rVert_F/\lVert M^{(h,l)}\rVert_F$ tracks the relative weight of the antisymmetric sector. During training, the gradient pushes $M_a$ in the direction of the dominant eigenvector of $D^{(h,l)}$ (since the loss decreases fastest by concentrating the directional operator on the most informative eigenplane). When $\rho^{(h,l)}$ is high — meaning one eigenplane already dominates — the gradient aligns with that plane and reinforces $\lVert M_a\rVert_F$ efficiently. Formally: the trace of the gradient with respect to $M_a$ satisfies $\operatorname{tr}[G_a\cdot M_a]\propto\lVert X^{(l)}M_aX^{(l)\top}\rVert_F^2/\lVert M_a\rVert_F$, which is non-decreasing in $\rho^{(h,l)}$ for fixed $\lVert M_a\rVert_F$ (because larger $\rho$ means the Frobenius norm is more concentrated, increasing the trace). Hence $\alpha_h^{(l)}$ — and therefore $\gamma_h^{(l)}=\alpha_h^{(l)}(t)/\alpha_h^{(l)}(0)$ — grows faster when $\rho^{(h,l)}$ is larger.

*(ii) $I_{\mathrm{cone}}^{(l)}$ is non-decreasing in $\bar\rho^{(l)}:=(1/H_l)\sum_h\rho^{(h,l)}$:* The cone index is defined in Chapter ?? as a measure of the angular concentration of the attention probability mass. By the coherence chain (Theorem (Ch. 8), Chapter ??), $I_{\mathrm{cone}}^{(l)}$ grows when $\Gamma^{(l)}$ grows. $\Gamma^{(l)}$ is the mean Frobenius norm of $D^{(h,l)}$, which is non-decreasing in $\bar\rho^{(l)}$ by the same argument as (i).

**Positive correlation.** Let $Z^{(l)}:=\bar\rho^{(l)}$ be the shared latent variable. Both $\bar\gamma^{(l)}$ and $I_{\mathrm{cone}}^{(l)}$ are non-decreasing functions of $Z^{(l)}$. By the FKG inequality (or equivalently, the Hardy-Littlewood rearrangement inequality for finite sequences): for any two non-decreasing functions $f,g$ of the same random variable $Z$,

<a id="display-0411-monotone-covariance"></a>
$$
\mathbb E[f(Z)g(Z)]\ge\mathbb E[f(Z)]\,\mathbb E[g(Z)],
$$

which is equivalent to $\operatorname{Corr}(f(Z),g(Z))\ge0$. Strict positivity of the correlation follows because $f$ and $g$ are not constant (both genuinely depend on $Z^{(l)}$ with positive variance across the training trajectory and across layers).

**Monotonicity of conditional expectation.** Since $\gamma_h^{(l)}$ is non-decreasing in $Z^{(l)}$ and $I_{\mathrm{cone}}^{(l)}$ is a non-decreasing transformation of $Z^{(l)}$, the conditional expectation $\mathbb E[\gamma_h^{(l)}\mid I_{\mathrm{cone}}^{(l)}=c]$ is non-decreasing in $c$ by the standard monotone regression theorem (if $Y=g(Z)$ is non-decreasing and $T=h(Z)$ is non-decreasing, then $\mathbb E[Y\mid T=t]$ is non-decreasing in $t$). $\square$

<a id="pdf-0412"></a>
## PDF 412 · Book 376

<a id="section-11-15-3"></a>
### 11.15.3 Proof of Proposition 11.41 (Layer Rank Decay)

The proof shows $\mathbb E[r_{\mathrm{eff}}(W_O^{(l+1)})]\le\mathbb E[r_{\mathrm{eff}}(W_O^{(l)})]$ using the Channel Destruction Proposition and the coherence chain.

**Recall the definition.** For a matrix $W$ with singular values $\sigma_1\ge\sigma_2\ge\cdots$:

<a id="display-0412-effective-rank"></a>
$$
r_{\mathrm{eff}}(W)=\exp\bigl(H(\bar\sigma)\bigr),
\qquad H(\bar\sigma)=-\sum_k\bar\sigma_k\log\bar\sigma_k,
\qquad\bar\sigma_k=\sigma_k/\sum_j\sigma_j.
$$

This is the exponential of the entropy of the normalised singular value distribution. $r_{\mathrm{eff}}$ is maximised when all singular values are equal (rank-$d$ identity up to scaling) and minimised at 1 (rank-1 matrix). It satisfies the following useful inequality.

<a id="lemma-11-76"></a>
**Lemma 11.76 (Effective rank under projection).** Let $W\in\mathbb R^{p\times q}$ and let $P\in\mathbb R^{q\times q}$ be an orthogonal projector of rank $r$. Then

$$
r_{\mathrm{eff}}(WP)\le r_{\mathrm{eff}}(W).
$$

*Proof.* Projection onto a rank-$r$ subspace can only concentrate the singular value distribution (it discards at least $q-r$ singular directions), which cannot increase the entropy $H(\bar\sigma)$ of the resulting distribution. Formally: $WP=U\Sigma V^\top P$; the singular values of $WP$ are a subset (plus zeros) of the singular values of $W$ restricted to $\operatorname{range}(P)$, so $H(\bar\sigma_{WP})\le H(\bar\sigma_W)$. $\square$

**Step 1: Structure of the MHA output.** By the Channel Destruction Proposition (Proposition ??, Chapter ??):

$$
\operatorname{MHA}^{(l)}(X^{(l)})=\sum_{h=1}^{H_l}Y^{(h,l)}W_O^{(h,l)},
$$

where $Y^{(h,l)}=A^{(h,l)}X^{(l)}W_V^{(h,l)}$ and $A^{(h,l)}$ is the attention matrix. The output lives in:

<a id="equation-11-43"></a>
$$
\operatorname{range}\bigl(\operatorname{MHA}^{(l)}(X^{(l)})\bigr)
\subseteq\sum_{h=1}^{H_l}\operatorname{range}\bigl(Y^{(h,l)}W_O^{(h,l)}\bigr).
\tag{11.43}
$$

The dimension of the right-hand side is at most $\sum_h\operatorname{rank}(Y^{(h,l)})\le H_l\cdot d_v$ by the subadditivity of rank.

**Step 2: Rank concentration from the coherence chain.** The coherence chain (Theorem (Ch. 8), Chapter ??) gives $H^{(l)}\searrow$ as $l$ increases, where $H^{(l)}$ here denotes the attention entropy. This means that the attention matrix $A^{(h,l)}$ concentrates its mass on fewer positions with increasing depth. Consequently, the effective rank of $Y^{(h,l)}=A^{(h,l)}X^{(l)}W_V^{(h,l)}$ satisfies:

<a id="equation-11-44"></a>
$$
r_{\mathrm{eff}}(Y^{(h,l+1)})\le r_{\mathrm{eff}}(Y^{(h,l)}),
\tag{11.44}
$$

because increasing concentration of $A^{(h,l)}$ is equivalent to projecting the input onto a lower-dimensional effective subspace before the value projection — a setting covered by Lemma 11.76.

**Step 3: Propagation to $W_O$.** The output projection $W_O^{(l)}$ is learned to represent the signal available in $\sum_hY^{(h,l)}W_O^{(h,l)}$. During training, the columns of $W_O^{(l)}$ align with the directions that carry signal in the MHA output: if the MHA output is effectively confined to

<a id="pdf-0413"></a>
## PDF 413 · Book 377

a $k$-dimensional subspace, the training dynamics encourage $W_O^{(l)}$ to concentrate its singular values on those $k$ directions.

More precisely: define the *empirical signal subspace* as $V_{\mathrm{sig}}^{(l)}:=\operatorname{span}\{Y^{(h,l)}W_O^{(h,l)}:h=1,\ldots,H_l\}$. From Step 1, $\dim V_{\mathrm{sig}}^{(l)}\le H_l\cdot d_v$. From Step 2 and the coherence chain, $H_l$ is non-increasing and the effective dimension of each $Y^{(h,l)}$ is non-increasing. Therefore $\dim V_{\mathrm{sig}}^{(l)}$ is non-increasing in $l$ in expectation.

The effective rank $r_{\mathrm{eff}}(W_O^{(l)})$ is bounded above by the dimension of $V_{\mathrm{sig}}^{(l)}$ at convergence. To see this formally: the gradient of the loss with respect to $W_O^{(l)}$ at step $t$ is

<a id="display-0413-output-gradient"></a>
$$
\nabla_{W_O^{(l)}}\mathcal L
=\sum_{h=1}^{H_l}\bigl(Y^{(h,l)}\bigr)^\top\bigl(\nabla_{Z^{(l)}}\mathcal L\bigr),
$$

where $Z^{(l)}=\operatorname{MHA}^{(l)}(X^{(l)})$ is the MHA output and $\nabla_{Z^{(l)}}\mathcal L\in\mathbb R^{n\times d}$. This gradient lives in $\operatorname{span}\{Y^{(h,l)}:h=1,\ldots,H_l\}^{\perp\top}$, i.e., is spanned by the rows of the $Y^{(h,l)}$. Therefore every gradient step moves $W_O^{(l)}$ in a direction that lies in $V_{\mathrm{sig}}^{(l)}$. Starting from any initialisation, after $T$ gradient steps:

$$
\operatorname{range}(W_O^{(l,T)})\subseteq\operatorname{range}(W_O^{(l,0)})+V_{\mathrm{sig}}^{(l)}.
$$

At convergence ($T\to\infty$), only the directions in $V_{\mathrm{sig}}^{(l)}$ receive persistent gradient signal; directions orthogonal to $V_{\mathrm{sig}}^{(l)}$ decay under any weight regularisation (or remain at their initialisation value, which is $O(1/\sqrt d)$ — negligible compared to the trained directions). Hence $r_{\mathrm{eff}}(W_O^{(l)})\lesssim\dim V_{\mathrm{sig}}^{(l)}$ in expectation at convergence, giving:

<a id="equation-11-45"></a>
$$
\mathbb E\left[r_{\mathrm{eff}}(W_O^{(l+1)})\right]
\lesssim\mathbb E\left[\dim V_{\mathrm{sig}}^{(l+1)}\right]
\le\mathbb E\left[\dim V_{\mathrm{sig}}^{(l)}\right]
\lesssim\mathbb E\left[r_{\mathrm{eff}}(W_O^{(l)})\right],
\tag{11.45}
$$

where $\lesssim$ denotes inequality up to initialisation noise (the $O(1/\sqrt d)$ residual singular values from initialisation). This is the claimed inequality. $\square$

<a id="remark-11-77"></a>
**Remark 11.77 (On the coupling between $W_O$ and $Y^{(h,l)}$).** Step 3 relies on the observation that $W_O^{(l)}$ at convergence reflects the signal geometry of $Y^{(h,l)}$. This is not a circular argument: $Y^{(h,l)}$ depends on $A^{(h,l)}$ and $W_V^{(h,l)}$, which are separate parameters from $W_O^{(l)}$. The coupling is through the loss: gradients of the loss with respect to $W_O^{(l)}$ are proportional to the residual error projected onto the directions spanned by $Y^{(h,l)}$ (standard backpropagation through the output projection), so at convergence $W_O^{(l)}$ has non-negligible singular values only in $V_{\mathrm{sig}}^{(l)}$.

<a id="section-11-16"></a>
### 11.16 Bibliographic Notes

**Effective rank and rank collapse.** The idea of tracking effective rank as a diagnostic for representational diversity in deep networks appears in [dong2021] in the context of rank collapse: they show that self-attention without residual connections converges toward rank 1 under iterated application. The Rank-Neutrality Theorem (Chapter ??, Theorem ??) refines that analysis by proving that layer normalisation is exactly rank-neutral — neither reducing nor increasing rank — rather than merely “playing no role”, as stated in [dong2021]. The distinction between rank collapse (global, asymptotic) and Channel Destruction (local, per-forward-pass) is drawn explicitly in Chapter ??, Remark (Ch. 2).

<a id="pdf-0414"></a>
## PDF 414 · Book 378

**Coherence chain and Channel Destruction.** The coherence chain $I_{\mathrm{cone}}\nearrow\Rightarrow\Gamma\nearrow\Rightarrow\bar\Delta\nearrow\Rightarrow H\searrow$ and the Channel Destruction Proposition were established in [bonino2025]. The causal reading of the coherence chain given in Section 11.12 — interpreting the chain as the signature of the Residual Amplification mechanism rather than as a set of static correlations — is new.

**Motor formalism.** The motor formalism that underlies Section 11.5 — in particular the decomposition of the gradient into symmetric and antisymmetric sectors, the invariance of $\mathfrak{so}(d)$ under the antisymmetric gradient flow, and the characterisation of fixed points — is developed in full in [cirrincione2026motor] (Chapters ?? and ?? of this volume). The Fixed-Point Classification (Proposition 11.14) is new to this chapter; its derivation from the vanishing condition $G_a^{(h,l)}=0$ is given in Chapter ??.

**Spectral methods and FKG inequality.** The proof of Proposition 11.40 uses the FKG inequality [fortuin1971] in the form of the Hardy-Littlewood rearrangement inequality for finite sequences. The connection between spectral concentration of an operator and the FKG lattice condition is standard; see [liggett1985] for a systematic treatment.

**Effective rank inequalities.** The effective rank $r_{\mathrm{eff}}(W)=\exp(H(\bar\sigma))$ is the exponential of the entropy of the singular value distribution, proposed as a matrix complexity measure in [roy2007]. Lemma 11.76 (effective rank under projection) is a consequence of the data-processing inequality for Shannon entropy: a projection discards information, which cannot increase entropy. The bound on the effective output subspace via the Channel Destruction Proposition extends the analysis of [dong2021] from rank to effective rank; the monotonicity with depth (Proposition 11.41) appears to be new.

**Neural Tangent Kernel.** The Neural Tangent Kernel (NTK) framework of [jacot2018] provides a linearised description of gradient flow in the infinite-width limit. [roberts2022] develop an effective field theory perspective that connects the NTK to the Gaussian statistics of hidden states at initialisation — the same Gaussian property tested empirically in Chapter ?? via the Lilliefors criterion. The conjecture in Section 11.11 relating the antisymmetric component of the NTK to $\alpha_h^{(l)}$ is new; its formal development and connection to the INCRT training dynamics are taken up in Chapter ??, Section (Ch. 17). [yang2021] extend the NTK analysis to the feature learning regime (finite-width networks where the kernel changes during training); the Residual Amplification mechanism operates in precisely this regime, making their framework a natural mathematical setting for the results of Section 11.7.

**Growing architectures.** The literature on growing neural networks — adding neurons or layers during training — is extensive: [chen2015] (Net2Net), [evci2022] (RigL), and neural architecture search methods more broadly. None of these approaches derives the growth criterion from a geometric principle analogous to $\Gamma_{\mathrm{res}}$. The closest in spirit is the work on progressive growing of GANs [karras2018], where depth is added when the current architecture has saturated its representational capacity at the current resolution; $\Gamma_{\mathrm{res}}^{(l)}<\theta_w$ is the precise criterion that operationalises this intuition for attention heads. The INCRT (Chapter ??) is the architecture that implements this principle.

<a id="chapter-11-end"></a>
> Editorial boundary: Chapter 11 ends here, on PDF 414 / printed page 378. Chapter 12 starts on [PDF 415](0415-0416-ch12-orbits-and-sharpness-introduction.md#pdf-0415).
