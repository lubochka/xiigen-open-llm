---
title: "Chapter 12: activation operator, spectral energy, and random initialisation"
source_pdf_pages: [425, 428]
book_pages: [389, 392]
batch: "009"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 12: activation operator, spectral energy, and random initialisation

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: PDF 417–424](0417-0424-ch12-sharpness-mixed-regimes-and-hessian.md#pdf-0424) · [Next: PDF 429–433](0429-0433-ch12-silence-isotropy-and-saddle-exit.md#pdf-0429)

> Editorial note: Full transcription of PDF 425–428 (printed pages 389–392), including objects 12.8–12.12 and equations (12.24)–(12.30). The source's Gram-trace identity, dimensionless-language claim, Gaussian normalisation, repeated $d_k$ in (12.30), and concentration argument are retained as printed. Notes 134–136 explain the relevant qualifications. The final caveat continues onto PDF 429.

<a id="pdf-0425"></a>
## PDF 425 · Book 389

**Caveat.** *The closed form is stated and proven for the directional gate ($\kappa=1$, target $\mathbf1e_c^\top$). Analogous results hold for the other deterministic regimes (open, instance, contextual) by the same argument, with $A_n$ replaced by the appropriate single-row block of $H_L^*$ at the corresponding saturated configuration. For distributed regimes ($\kappa>1$), the saturated configuration $L^*$ requires $\rho_i=0$ exactly inside the support, and the spectrum of $A_n$ changes accordingly. Each of these cases is a routine computation with the same structural form, and they are not reproduced them here.*

<a id="section-12-2-5"></a>
### 12.2.5 From sharpness to activation

The sharpness theorem says when a head’s score matrix is close to a canonical regime; it does not say whether the regime is actually used to transport information. A directional gate may be sharp without ever activating its antisymmetric component on the cloud — if the row span $U^{(\ell)}$ is isotropic for the alternating form $\omega_a^{(\ell,h)}(x,y)=x^\top M_a^{(\ell,h)}y$, the directional structure is silent. The next section makes activation precise, and the section after that characterises silence.

<a id="section-12-3"></a>
### 12.3 The activation operator

The previous section asked when a head’s score matrix is close to its preferred canonical regime. The present section asks a complementary question: given that a head has a non-trivial antisymmetric sector $M_a^{(h)}\in\mathbb R^{d\times d}$, when does the input cloud $X\in\mathbb R^{n\times d}$ see that sector?

The question is not rhetorical. Chapter 7 already exhibited the phenomenon: the canonical residual $\Gamma_{\mathrm{res}}^{(h)}(X)=\lVert XM_a^{(h)}X^\top\rVert_F$ can be zero on a particular cloud while $M_a^{(h)}$ is non-zero as a matrix. The quantity $\Gamma_{\mathrm{res}}$ collapses three structural facts into one scalar — the size of $X$, the size of $M_a^{(h)}$, and the alignment between the row span of $X$ and the eigenplanes of $M_a^{(h)}$ — and a low value of $\Gamma_{\mathrm{res}}$ is therefore difficult to read. The activation operator defined below separates these three contributions: it isolates the alignment factor as an intrinsic, dimensionless object, and it makes the size and geometry of the activation explicit.

The construction is a lift to operators of the row-span observable $\widehat\gamma_{\ell,h}(X)$ of §7.7.13. Where $\widehat\gamma$ measures the normalised length of the antisymmetric activation vector in the cloud frame, the activation operator measures the entire restricted alternating form. The two are coupled, but the operator level is strictly richer: it carries the eigenplanes of $M_a^{(h)}$ that the cloud selects, the projected areas, and the rank of the head’s actual contribution on the cloud.

<a id="section-12-3-1"></a>
### 12.3.1 Definition and basic properties

Let $X\in\mathbb R^{n\times d}$ have row span $U=U(X)\subseteq\mathbb R^d$, with $r:=\dim U=\operatorname{rank}(X)$. Let $\pi_U\in\mathbb R^{d\times d}$ be the orthogonal projector onto $U$. For a head $h$ with antisymmetric sector $M_a^{(h)}$:

<a id="definition-12-8"></a>
**Definition 12.8 (Activation operator).** The *activation operator* of head $h$ on cloud $X$ is

<a id="equation-12-24"></a>
$$
\mathcal A^{(h)}(X):=\pi_{U(X)}M_a^{(h)}\pi_{U(X)}\in\mathbb R^{d\times d}.
\tag{12.24}
$$

The operator $\mathcal A^{(h)}(X)$ is antisymmetric (since $M_a^{(h)}$ is and projectors commute with transposition only when symmetric: explicitly $(\pi_UM_a\pi_U)^\top=\pi_U^\top M_a^\top\pi_U^\top=\pi_U(-M_a)\pi_U=-\pi_UM_a\pi_U$). It is supported on $U$, in the sense that $\mathcal A^{(h)}(X)v=0$ for $v\in U^\perp$ and $\mathcal A^{(h)}(X)v\in U$ for $v\in U$. Its rank does not exceed $\min(r,\operatorname{rank}M_a^{(h)})$, and it is invariant under any choice of

<a id="pdf-0426"></a>
## PDF 426 · Book 390

basis for $U$ that preserves orientation. The activation operator is not a function of the matrix $X$ itself, only of its row span; it is therefore the right object for an analysis that does not depend on a choice of basis for the input cloud.

<a id="definition-12-9"></a>
**Definition 12.9 (Activation energy).** The *activation energy* of head $h$ on cloud $X$ is

<a id="equation-12-25"></a>
$$
\mathcal E^{(h)}(X):=\lVert\mathcal A^{(h)}(X)\rVert_F^2.
\tag{12.25}
$$

The activation energy is a non-negative scalar that is zero exactly when the restricted alternating form $\omega_U^{(h)}(x,y):=x^\top M_a^{(h)}y$ is identically zero on $U\times U$. It is monotone in $U$: enlarging the row span never decreases the activation energy, since the projector $\pi_U$ becomes larger. It is also monotone in $M_a^{(h)}$: scaling the antisymmetric sector by $c$ scales the activation energy by $c^2$.

<a id="remark-12-10"></a>
**Remark 12.10 (Comparison with $\Gamma_{\mathrm{res}}$ and $\widehat\gamma$).** The three observables — the canonical residual $\Gamma_{\mathrm{res}}^{(h)}(X)=\lVert XM_a^{(h)}X^\top\rVert_F$ of Chapter 7, the normalised activation $\widehat\gamma_{\ell,h}(X)$ of §7.7.13, and the activation energy $\mathcal E^{(h)}(X)$ of Definition 12.9 — are related but distinct. They sit in the following hierarchy:

- $\Gamma_{\mathrm{res}}^{(h)}(X)$ is the raw, unnormalised quantity, depending on the size of $X$, the size of $M_a^{(h)}$, and the alignment.
- $\widehat\gamma_{\ell,h}(X)=\Gamma_{\mathrm{res}}^{(h)}(X)/(\lVert X\rVert_{\mathrm{op}}^2\lVert M_a^{(h)}\rVert_F)$ is normalised by the size of $X$ and $M_a^{(h)}$, isolating the alignment.
- $\mathcal E^{(h)}(X)=\lVert\pi_{U(X)}M_a^{(h)}\pi_{U(X)}\rVert_F^2$ is a frame-invariant operator-level quantity that depends on $U(X)$ and $M_a^{(h)}$ only, with no scaling by $\lVert X\rVert$.

The relationship between them is given by $\Gamma_{\mathrm{res}}^{(h)}(X)^2=\operatorname{tr}\bigl((XX^\top)XM_a^{(h)\top}M_a^{(h)}X^\top\bigr)$, which equals $\mathcal E^{(h)}(X)$ when the rows of $X$ are orthonormal in $U$ and $\lVert XX^\top\rVert_F^2=r$. For general $X$ the two differ by a Gram-matrix factor.

<a id="section-12-3-2"></a>
### 12.3.2 Spectral interpretation

The activation energy admits a clean reading in terms of the symplectic eigenplanes of $M_a^{(h)}$ on $U$, which is the rigorous version of the informal statement of §6.16 (Cap. 6). The reading uses the canonical form of antisymmetric matrices on a real Euclidean space.

**Canonical form of $M_a^{(h)}$.** Every real antisymmetric $M_a\in\mathbb R^{d\times d}$ admits an orthonormal basis $(e_1,f_1,e_2,f_2,\ldots,e_K,f_K,g_1,\ldots,g_{d-2K})$ in which

<a id="equation-12-26"></a>
$$
M_a=\sigma_1(e_1f_1^\top-f_1e_1^\top)+\cdots+
\sigma_K(e_Kf_K^\top-f_Ke_K^\top),
\tag{12.26}
$$

where $\sigma_1\ge\cdots\ge\sigma_K>0$ are the positive singular values and the $g_a$ span $\ker M_a$. The two-dimensional subspaces $E_k=\operatorname{span}(e_k,f_k)$ are the *symplectic eigenplanes* of $M_a$. The bilinear form $\omega(x,y)=x^\top M_ay$ acts as a signed area form $\sigma_k\det(\cdot)$ on $E_k$ and vanishes between distinct eigenplanes. The following uses this canonical form throughout the section.

**Plücker coordinates of $U$.** Given a subspace $U\subseteq\mathbb R^d$ of dimension $r$, the second exterior power $\Lambda^2U$ is a subspace of $\Lambda^2\mathbb R^d$ of dimension $\binom r2$. The orthogonal projection $\pi_{\Lambda^2U}:\Lambda^2\mathbb R^d\to\Lambda^2U$ is induced by $\pi_U$ via the standard functorial property $\pi_{\Lambda^2U}(x\wedge y)=(\pi_Ux)\wedge(\pi_Uy)$. The bivector

<a id="equation-12-27"></a>
$$
\omega_a:=\sum_{k=1}^K\sigma_k(e_k\wedge f_k)\in\Lambda^2\mathbb R^d
\tag{12.27}
$$

<a id="pdf-0427"></a>
## PDF 427 · Book 391

is the bivector dual to $M_a$, with $\lVert\omega_a\rVert^2=\sum_k\sigma_k^2=\tfrac12\lVert M_a\rVert_F^2$.

<a id="theorem-12-11"></a>
**Theorem 12.11 (Spectral interpretation of activation energy).** Let $M_a^{(h)}\in\mathbb R^{d\times d}$ be antisymmetric with canonical form (12.26) and symplectic eigenplanes $E_1,\ldots,E_K$. Let $U\subseteq\mathbb R^d$ be a subspace of dimension $r\ge2$. Then

<a id="equation-12-28"></a>
$$
\mathcal E^{(h)}(X)=2\lVert\pi_{\Lambda^2U}(\omega_a)\rVert^2
=2\sum_{k,\ell=1}^K\sigma_k\sigma_\ell
\langle\pi_Ue_k\wedge\pi_Uf_k,\pi_Ue_\ell\wedge\pi_Uf_\ell\rangle,
\tag{12.28}
$$

where the inner product is the canonical extension of the Euclidean product to $\Lambda^2\mathbb R^d$. In particular, when $U$ is aligned with a single eigenplane $E_k$ (i.e. when $\pi_Ue_\ell\wedge\pi_Uf_\ell=0$ for all $\ell\ne k$), the formula collapses to

<a id="equation-12-29"></a>
$$
\mathcal E^{(h)}(X)=2\sigma_k^2\bigl(\operatorname{Area}_{E_k}(\pi_U)\bigr)^2,
\tag{12.29}
$$

where $\operatorname{Area}_{E_k}(\pi_U):=\lVert\pi_Ue_k\wedge\pi_Uf_k\rVert$ is the projected area of the eigenplane $E_k$ onto $U$.

*Proof.* The passage between the matrix description $M_a^{(h)}$ and the bivector description $\omega_a$ via the standard isomorphism between the space of antisymmetric $d\times d$ matrices and $\Lambda^2\mathbb R^d$, which sends $e_kf_k^\top-f_ke_k^\top\mapsto e_k\wedge f_k$. Under this isomorphism the Frobenius norm of an antisymmetric matrix equals $\sqrt2$ times the norm of the corresponding bivector: $\lVert e_kf_k^\top-f_ke_k^\top\rVert_F^2=2$ and $\lVert e_k\wedge f_k\rVert^2=1$. Hence $\lVert M_a^{(h)}\rVert_F^2=2\lVert\omega_a\rVert^2=2\sum_k\sigma_k^2$.

The restriction $\pi_UM_a^{(h)}\pi_U$ corresponds, under the same isomorphism, to the bivector $\pi_{\Lambda^2U}(\omega_a)$: $\pi_{\Lambda^2U}(e_k\wedge f_k)=(\pi_Ue_k)\wedge(\pi_Uf_k)$, which is the bivector representative of $\pi_U(e_kf_k^\top-f_ke_k^\top)\pi_U$. Therefore $\mathcal E^{(h)}(X)=\lVert\pi_UM_a^{(h)}\pi_U\rVert_F^2=2\lVert\pi_{\Lambda^2U}(\omega_a)\rVert^2$, which is the first equality of (12.28).

The expansion in the second equality follows from the bilinearity of the inner product on $\Lambda^2\mathbb R^d$:

$$
\begin{aligned}
\lVert\pi_{\Lambda^2U}(\omega_a)\rVert^2
&=\left\lVert\sum_{k=1}^K\sigma_k(\pi_Ue_k\wedge\pi_Uf_k)\right\rVert^2\\
&=\sum_{k,\ell=1}^K\sigma_k\sigma_\ell
\langle\pi_Ue_k\wedge\pi_Uf_k,\pi_Ue_\ell\wedge\pi_Uf_\ell\rangle.
\end{aligned}
$$

For the single-plane collapse, suppose $U$ is aligned with $E_k$ in the sense that $\pi_Ue_\ell\wedge\pi_Uf_\ell=0$ for all $\ell\ne k$. (This holds, in particular, when $U$ is contained in $E_k\oplus L$ for some Lagrangian subspace $L$ of the symplectic complement, but more generally whenever the projected eigenplanes for $\ell\ne k$ are degenerate or coincide with linear combinations of $\pi_Ue_k,\pi_Uf_k$.) Then all cross terms vanish, the diagonal $\ell=k$ term gives $\sigma_k^2\lVert\pi_Ue_k\wedge\pi_Uf_k\rVert^2$, and one obtains (12.29). $\square$

**Consequence: cross-eigenplane contributions are not negligible.** The expansion (12.28) makes explicit the inter-eigenplane cross terms $\langle\pi_Ue_k\wedge\pi_Uf_k,\pi_Ue_\ell\wedge\pi_Uf_\ell\rangle$ for $k\ne\ell$. These vanish only when the projected eigenplanes are orthogonal in $\Lambda^2\mathbb R^d$, which is a more restrictive condition than the orthogonality of the eigenplanes $E_k\perp E_\ell$ in $\mathbb R^d$. The eigenplanes themselves are pairwise orthogonal in $\mathbb R^d$ (by the canonical form), but their projections onto $U$ generally are not. The diagonal-only form $\mathcal E^{(h)}(X)=\sum_k2\sigma_k^2\operatorname{Area}_{E_k}(\pi_U)^2$, which the preliminary formulation of Chapter 6 (§6.19) claimed too broadly, holds only under the alignment hypothesis used in (12.29). For generic $U$ the cross-eigenplane terms contribute, and the spectral identity is genuinely bilinear.

<a id="pdf-0428"></a>
## PDF 428 · Book 392

<a id="section-12-3-3"></a>
### 12.3.3 Activation threshold under random initialisation

Theorem 12.11 computes activation energy as a function of $U$ and $M_a^{(h)}$, but says nothing about its size at the start of training. The natural quantitative question is: under standard initialisation, how large is the activation energy of a typical head?

<a id="proposition-12-12"></a>
**Proposition 12.12 (Activation threshold under Xavier initialisation).** Suppose the input cloud $X\in\mathbb R^{n\times d}$ has rows $x_1,\ldots,x_n$ drawn i.i.d. from $\mathcal N(0,I_d)$, and that the head’s weight matrices $W_Q,W_K\in\mathbb R^{d\times d_k}$ are drawn from Xavier initialisation: $(W_Q)_{ij},(W_K)_{ij}\sim\mathcal N(0,1/d)$ i.i.d. Set $M^{(h)}=W_QW_K^\top\in\mathbb R^{d\times d}$ and let $M_a^{(h)}=\tfrac12(M^{(h)}-M^{(h)\top})$. Then there exist absolute constants $c_1,c_2>0$ such that, with probability at least $1-c_2e^{-c_1n}$,

<a id="equation-12-30"></a>
$$
\mathcal E^{(h)}(X)\ge c_1\cdot
\frac{\min(n,d)\,\bigl(\min(n,d)-1\bigr)}{d(d-1)}
\frac{d_k}{d}\,d_k.
\tag{12.30}
$$

*Proof.* Let $r=\operatorname{rank}(X)=\min(n,d)$ a.s. Conditional on $X$, the row span $U(X)$ is a uniformly distributed (Haar) $r$-dimensional subspace of $\mathbb R^d$, by the rotational invariance of the standard Gaussian.

The Frobenius norm of $M_a^{(h)}$ admits a direct calculation. Each entry of $M^{(h)}=W_QW_K^\top$ is a sum of $d_k$ products of independent $\mathcal N(0,1/d)$ Gaussians, with mean zero and variance $d_k/d^2$. The expected squared Frobenius norm of $M_a^{(h)}$ is $\mathbb E\lVert M_a^{(h)}\rVert_F^2=\tfrac12\sum_{i\ne j}\operatorname{Var}(M_{ij}-M_{ji})=\tfrac12\cdot2\sum_{i\ne j}\operatorname{Var}(M_{ij})=d(d-1)\cdot d_k/d^2=(d-1)d_k/d$. Concentration of squared Frobenius norm of products of two Gaussian matrices is a standard sub-exponential estimate (e.g. ?, Theorem 6.3.2): with probability at least $1-2e^{-cn}$, $\tfrac12(d-1)d_k/d\le\lVert M_a^{(h)}\rVert_F^2\le\tfrac32(d-1)d_k/d$.

Conditional on $\lVert M_a^{(h)}\rVert_F^2$ and the Haar-uniform $U(X)\sim\operatorname{Gr}(r,d)$, Theorem 6.69 (the corrected generic-activation expectation, Cap. 6) gives $\mathbb E[\mathcal E^{(h)}(X)\mid M_a^{(h)}]=\frac{r(r-1)}{d(d-1)}\lVert M_a^{(h)}\rVert_F^2$. Concentration of $\mathcal E^{(h)}$ around its mean conditional on $M_a^{(h)}$ is a Lipschitz-on-Grassmannian concentration result (the function $U\mapsto\lVert\pi_UM_a^{(h)}\pi_U\rVert_F^2$ is $2\lVert M_a^{(h)}\rVert_{\mathrm{op}}$-Lipschitz with respect to the geodesic distance on $\operatorname{Gr}(r,d)$, and Haar measure on $\operatorname{Gr}(r,d)$ has a log-Sobolev constant of order $1/d$). Combining the two concentration bounds gives, with probability at least $1-c_2e^{-c_1n}$, $\mathcal E^{(h)}(X)\ge\tfrac12\frac{r(r-1)}{d(d-1)}\tfrac12(d-1)d_k/d=\tfrac14\frac{r(r-1)}{d^2}d_k$. Setting $c_1=\tfrac14$ (and adjusting the exponent constants accordingly) gives the claim, since $\frac{r(r-1)}{d^2}d_k=\frac{r(r-1)}{d(d-1)}\cdot\frac{(d-1)d_k}{d}\ge\frac{r(r-1)}{d(d-1)}\cdot d_k\cdot\frac{d-1}{d}$, which is the bound (12.30) up to absolute constants. $\square$

**Reading the bound.** At the start of training under Xavier initialisation, a typical head’s activation energy is $\Theta(d_k)$ when $n\ge d$, dropping by a factor of $(n/d)^2$ when $n<d$. This is a calibration result, not a universal activation law: it asserts a specific scaling under specific initialisation hypotheses. The bound (12.30) does not say that trained heads have this activation energy — training drives the geometry, and after training the energy depends on the trajectory in signature space (Chapter 11). The bound says only that, at initialisation, the activation energy is detectable: a randomly initialised head is not silent.

**Caveat.** *The proposition is conditioned on Xavier initialisation. Other initialisations give different scalings. He initialisation $(W)_{ij}\sim\mathcal N(0,2/d)$ multiplies the right-hand side by $4$; LayerNorm-aware initialisations and orthogonal initialisations (??) change the structural form of $M_a^{(h)}$, in particular by removing the diagonal-anisotropy that Xavier introduces. The proposition should be read as a calibration theorem under one specific protocol; the structural*
