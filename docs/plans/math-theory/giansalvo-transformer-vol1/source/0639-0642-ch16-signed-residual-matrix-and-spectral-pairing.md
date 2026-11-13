---
title: "Chapter 16: signed residual matrices, trace identities, and empirical spectral pairing"
source_pdf_pages: [639, 642]
book_pages: [603, 606]
batch: "013"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 16: signed residual matrices, trace identities, and empirical spectral pairing

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: signed task operators](0635-0638-ch16-signed-task-operators-and-bilateral-bounds.md) · [Next: level-two pruning and stabilisation](0643-0646-ch16-level-two-pruning-and-stabilisation.md)

> Editorial note: Full transcription of PDF 639–642 (printed pages 603–606), including Table 16.8 and the complete three-dimensional matrix example. Original proof-status tags, empirical claims, numerical approximations, and the distinction between the three residual operators are preserved. The prose in Corollary 16.47’s proof says “Frobenius norm” where its displayed identity correctly uses the squared norm. Conjecture 16.48 continues on PDF 643.

<a id="pdf-0639"></a>
## PDF 639 · Book page 603

during early training, the predicted ratio $K_-^*/K_+^*$ computed from $\widehat\kappa_{A,\pm}$, and compared with the ratio implied by the deployed architecture.

**Result.** the table reports the calibration across the four families.

<a id="table-16-8"></a>
| Family | Task | $\widehat\kappa_{A,+}$ | $\widehat\kappa_{A,-}$ | $\widehat{K_-^*}/\widehat{K_+^*}$ | deployed ratio |
|---|---|---:|---:|---:|---:|
| RL | DMC cheetah-run | 5.2 | 4.4 | 0.71 | 0.65 |
| Contrastive | SimCLR CIFAR-10 | 3.8 | 4.5 | 1.40 | 1.25 |
| GAN | WGAN-GP CelebA-32 | 4.1 | 4.7 | 1.31 | 1.00 (parity) |
| Adversarial | PGD-ResNet CIFAR-10 | 6.3 | 3.2 | 0.26 | 0.30 |

**Table 16.8:** Signed compressed-sensing bound applied to four indefinite-operator task families.

*The ratio $\widehat{K_-^*}/\widehat{K_+^*}$ is computed from the empirically estimated signed directional-complexity indices; the “deployed ratio” is the ratio of architectural components in typical deployed implementations of each task. The agreement is within a factor of 1.4 across all four families, with the GAN row showing the largest deviation because the deployed parity assumption (generator and discriminator of equal size) is a heuristic rather than a spectral derivation.*

The framework recovers the qualitative split between the four regimes: RL and adversarial training have $K_-^*<K_+^*$ (suppression is the smaller part); contrastive learning and GANs have $K_-^*\ge K_+^*$ (suppression dominates or matches capture). The quantitative agreement is within 40% across the table, which is the regime in which the signed CS bound is operationally informative.

**Summary.** Indefinite task operators admit a Jordan decomposition into positive and negative classes, each of which is positive semi-definite and orthogonal to the other. The compressed-sensing bound applies independently to each class, giving signed counts $K_\pm^*=\kappa_{A,\pm}^2\log(\Gamma_0^\pm/\theta_w)$ whose sum is the total architectural budget. The bilateral gate generalises to four channels, decoupling the two classes at the dynamical level. The framework specialises to reinforcement learning, contrastive learning, GANs, and adversarial training, recovering known structural properties of each. The eight extensions of the preceding sections compose with the signed framework: each acts independently within each class, with cross-class interaction trivial under the symmetry of the task operator.

<a id="section-16-13"></a>
## 16.13 Signed spectral stabilisation and INCRT as architecture initialisation

The preceding section established the signed framework at the level of the task operator $\mathcal B$: when $\mathcal B$ is indefinite, its Jordan decomposition $\mathcal B=\mathcal B_+-\mathcal B_-$ produces two positive semi-definite classes, and the compressed-sensing bound applies to each class with its own directional-complexity index $\kappa_{A,\pm}$ and its own directional energy $\Gamma_0^\pm$. The treatment was setup-level: signed structure was postulated on $\mathcal B$ and propagated through the deflated residual operators $A_{\mathrm{res}}^+,A_{\mathrm{res}}^-$.

The present section descends one level. We show that the signed structure of the residual is not a postulate at the level of $\mathcal B$ but a structural property of a *single attention head* viewed through its algebraic residual. At the head level, the signed operator emerges automatically from the antisymmetric part of the effective query–key product, and the consequences of its indefinite signature reach the controller in three operationally distinct ways. First, the

<a id="pdf-0640"></a>
## PDF 640 · Book page 604

protection rule of the controller acquires a sign-corrected form already documented as the level-one diagnostic of the previous chapter; the level-two rule of the four-channel signed gate is recorded here in matrix form. Second, a regime of the controller emerges in which the backbone is warm-started and the residual operator has very few dominant signed modes: in this *stabilisation regime* the controller does not grow but does preserve and prune. Third, the controller may be applied *before* training, on a brief spectral pre-pass over the residual operator of a candidate backbone, in order to output an architecture $(L^\star,\{K_\ell^\star\},\{(u_h^+,u_h^-)\})$ deterministically; this is the *architecture-initialisation regime* of INCRT, and is announced here for development in Volume II.

Throughout the section, the head-level signed operator is denoted $A_h^{\mathrm{sig}}$, and is to be distinguished from the global deflated residual operator $A_{\mathrm{res}}^{(k)}$ of Section 16.3. The relation between the two is discussed in Section 16.13.1.

<a id="section-16-13-1"></a>
### 16.13.1 The head-level signed residual operator

Fix a layer index, suppressed in the notation, and consider a single attention head $h$ with query and key projection matrices $W_q^{(h)},W_k^{(h)}\in\mathbb R^{d\times d_k}$ acting on a batch of inputs $X\in\mathbb R^{n\times d}$. Write

<a id="equation-16-70"></a>
$$
M_{\mathrm{eff},h}:=W_q^{(h)}\bigl(W_k^{(h)}\bigr)^\top\in\mathbb R^{d\times d}
\tag{16.70}
$$

for the effective query–key product, and

<a id="equation-16-71"></a>
$$
\Sigma_X:=\frac1B\sum_{b=1}^B X_b^\top X_b\in\mathbb R^{d\times d}
\tag{16.71}
$$

for the empirical input covariance averaged over a batch of size $B$. Decompose $M_{\mathrm{eff},h}$ into its symmetric and antisymmetric parts,

<a id="equation-16-72"></a>
$$
M_{\mathrm{eff},h}=M_h^{\mathrm{sym}}+K_h,\qquad
M_h^{\mathrm{sym}}=\operatorname{sym}(M_{\mathrm{eff},h}),\qquad
K_h=\operatorname{skew}(M_{\mathrm{eff},h}),
\tag{16.72}
$$

where $\operatorname{sym}(\cdot)$ and $\operatorname{skew}(\cdot)$ denote the symmetric and antisymmetric components, with $\operatorname{sym}(M)=\frac12(M+M^\top)$ and $\operatorname{skew}(M)=\frac12(M-M^\top)$. The *head-level signed residual operator* is

<a id="equation-16-73"></a>
$$
A_h^{\mathrm{sig}}:=\operatorname{sym}(\Sigma_XK_h)
=\frac12\bigl(\Sigma_XK_h+K_h^\top\Sigma_X^\top\bigr)\in\mathbb R^{d\times d}.
\tag{16.73}
$$

Using $\Sigma_X^\top=\Sigma_X$ and $K_h^\top=-K_h$, the definition admits the commutator form

<a id="equation-16-74"></a>
$$
A_h^{\mathrm{sig}}=\frac12[\Sigma_X,K_h]
=\frac12\bigl(\Sigma_XK_h-K_h\Sigma_X\bigr),
\tag{16.74}
$$

which is the basic structural identity of the head-level signed operator.

**Worked example in dimension three.** A small dimensional example illustrates the algebraic structure of (16.74). Fix $d=3$ and take

<a id="display-0640-example-covariance-and-skew"></a>
$$
\Sigma_X=\begin{pmatrix}3&1&0\\1&2&0\\0&0&1\end{pmatrix},
\qquad
K_h=\begin{pmatrix}0&1&0\\-1&0&2\\0&-2&0\end{pmatrix},
$$

with $\Sigma_X\succ0$ symmetric and $K_h$ antisymmetric. Direct computation of the commutator gives

<a id="display-0640-example-signed-matrix"></a>
$$
A_h^{\mathrm{sig}}=\tfrac12[\Sigma_X,K_h]
=\begin{pmatrix}-1&\frac12&1\\\frac12&1&1\\1&1&0\end{pmatrix}.
$$

<a id="pdf-0641"></a>
## PDF 641 · Book page 605

The matrix is manifestly symmetric and has trace $(-1)+1+0=0$, in agreement with Proposition 16.46. Its eigenvalues $\{\lambda_1,\lambda_2,\lambda_3\}$ are roots of the characteristic polynomial $\lambda^3-\frac{13}{4}\lambda-1=0$, and are real with indefinite sign: approximately $\lambda_1\approx-1.62$, $\lambda_2\approx-0.32$, and $\lambda_3\approx1.94$, summing to zero. The example shows two structural features that are typical of the empirical spectra of $A_h^{\mathrm{sig}}$ reported in Section 16.13.2: the spectrum is indefinite by tracelessness, and the eigenvalues are not in exact $(+\lambda,-\lambda)$ pairs because no orthogonal involution $J$ satisfying (16.76) exists for this particular $(\Sigma_X,K_h)$ pair.

**Structural origin.** The operator $A_h^{\mathrm{sig}}$ arises naturally as the linear residual of the attention contribution of head $h$ after the symmetric directions $M_h^{\mathrm{sym}}$ have been absorbed into a positive task component. In matrix form, the head computes a bilinear interaction $XM_{\mathrm{eff},h}X^\top$ between query and key projections; the symmetric part $M_h^{\mathrm{sym}}$ acts on the input covariance through the symmetrised product $\operatorname{sym}(\Sigma_XM_h^{\mathrm{sym}})=\frac12(\Sigma_XM_h^{\mathrm{sym}}+M_h^{\mathrm{sym}}\Sigma_X)$, which is symmetric in general, and positive semi-definite when $\Sigma_X$ and $M_h^{\mathrm{sym}}$ commute, or under the positive-class compatibility assumptions of the preceding section; the symmetrised product of two positive semi-definite operators is not positive semi-definite in general when the factors do not commute. The antisymmetric part $K_h$, in contrast, acts through the commutator $[\Sigma_X,K_h]$, which is the symmetric part of a non-symmetric product and is the head-level analogue of the signed quantity $\operatorname{sym}(X^\top XM_a)$ flagged in Section 16.3 as “not positive in general and therefore treated later in the indefinite-operator extension”. The present section is that treatment, at head level.

<a id="proposition-16-46"></a>
**Proposition 16.46 (Symmetry and tracelessness of $A_h^{\mathrm{sig}}$).** [T-PROVED] *For every head $h$ and every batch, the head-level signed residual operator $A_h^{\mathrm{sig}}$ defined in (16.73) is symmetric and traceless:*

<a id="display-0641-symmetry-and-tracelessness"></a>
$$
\bigl(A_h^{\mathrm{sig}}\bigr)^\top=A_h^{\mathrm{sig}},
\qquad\operatorname{tr}(A_h^{\mathrm{sig}})=0.
$$

*Proof.* Symmetry follows from $\operatorname{sym}(M)^\top=\operatorname{sym}(M)$ for any square $M$. For tracelessness, expand the commutator form (16.74): $\operatorname{tr}(\Sigma_XK_h-K_h\Sigma_X)=\operatorname{tr}(\Sigma_XK_h)-\operatorname{tr}(K_h\Sigma_X)=0$ by the cyclic property of the trace. Hence $\operatorname{tr}(A_h^{\mathrm{sig}})=0$. $\square$

<a id="corollary-16-47"></a>
**Corollary 16.47 (Indefinite spectrum).** [T-PROVED] *Whenever $A_h^{\mathrm{sig}}\ne0$, its spectrum is indefinite. Specifically, denoting its real eigenvalues by $\{\lambda_i^{(h)}\}_{i=1}^d$,*

<a id="display-0641-trace-and-frobenius-eigenvalues"></a>
$$
\sum_{i=1}^d\lambda_i^{(h)}=0,
\qquad
\bigl\|A_h^{\mathrm{sig}}\bigr\|_F^2
=\sum_{i=1}^d\bigl(\lambda_i^{(h)}\bigr)^2,
$$

*and at least one eigenvalue is strictly positive and at least one is strictly negative.*

*Proof.* Since $A_h^{\mathrm{sig}}$ is symmetric by Proposition 16.46, all its eigenvalues are real. The trace equals the sum of eigenvalues, which is zero by the same proposition; the Frobenius norm equals the sum of squared eigenvalues by the spectral theorem on real symmetric matrices. If all non-zero eigenvalues had the same sign, their sum could not vanish unless they were all zero, contradicting $A_h^{\mathrm{sig}}\ne0$. Hence at least one eigenvalue is strictly positive and at least one is strictly negative. $\square$

**Two distinct signed operators and a global positive counterpart.** The operator $A_h^{\mathrm{sig}}$ is a per-head, signed, batch-level object: it is defined head by head, and its spectrum is the local spectrum of head $h$. For diagnostic purposes the controller does not always

<a id="pdf-0642"></a>
## PDF 642 · Book page 606

have access to the per-head spectra, and an aggregated quantity is used instead. Define the *aggregated signed residual operator* at the layer level by

<a id="equation-16-75"></a>
$$
A_{\mathrm{res}}^{\mathrm{sig}}:=\operatorname{sym}\bigl(\Sigma_X\operatorname{skew}(\overline M_{\mathrm{eff}})\bigr),
\qquad
\overline M_{\mathrm{eff}}:=\frac1H\sum_{h=1}^H M_{\mathrm{eff},h},
\tag{16.75}
$$

where the average is over the heads of the layer. The operator $A_{\mathrm{res}}^{\mathrm{sig}}$ is the layer-level counterpart of the per-head $A_h^{\mathrm{sig}}$, and shares with it the algebraic structure of Proposition 16.46: it is symmetric and traceless, by the same commutator argument. These two objects are not the same as $A_{\mathrm{res}}^{(k)}=P_\perp^{(k)}\mathcal BP_\perp^{(k)}$ of Section 16.3, which is a global, positive semi-definite, deflation-level object whose construction absorbs the antisymmetric component into a class-balanced positive task energy as in Section 16.12. The three operators sit at three different levels of the architecture: $A_h^{\mathrm{sig}}$ per head, $A_{\mathrm{res}}^{\mathrm{sig}}$ per layer, and $A_{\mathrm{res}}^{(k)}$ per deflation step on the global task operator. For the rest of the section, the algebraic statements (Proposition 16.46 and Corollary 16.47) hold equally for $A_h^{\mathrm{sig}}$ and for $A_{\mathrm{res}}^{\mathrm{sig}}$, while the empirical diagnostics of Section 16.13.2 below were computed on $A_{\mathrm{res}}^{\mathrm{sig}}$, the layer-level aggregated form, and are extrapolated to the per-head form $A_h^{\mathrm{sig}}$ via the head-aggregation map sketched here and detailed in Volume II.

<a id="section-16-13-2"></a>
### 16.13.2 Empirical pairing of the signed spectrum

The structural property $\operatorname{tr}(A_h^{\mathrm{sig}})=0$ implies that positive and negative eigenvalues coexist, but it does not imply that they appear in matched pairs $(+\lambda,-\lambda)$. Empirically, the pairing is observed across several training setups; we record the observation and its status.

**Setup.** The empirical diagnostics below were computed on the aggregated signed residual operator $A_{\mathrm{res}}^{\mathrm{sig}}$ of (16.75), which is the layer-level counterpart of the per-head object $A_h^{\mathrm{sig}}$. The aggregated operator was computed on four configurations of a two-layer attention block trained on a sentence classification objective: two pairs of datasets (SST-2 and MNLI) crossed with two input embedding choices (a BD bidirectional embedding and a PCA projection), and in one case crossed with the presence or absence of an MLM warm-start of the backbone. The four configurations are denoted SST2-BD-MLM, MNLI-BD-MLM, MNLI-PCA-MLM, and MNLI-BD-NO-MLM. For each configuration, the eigenvalues of $A_{\mathrm{res}}^{\mathrm{sig}}$ were sorted in ascending order $\lambda_1\le\cdots\le\lambda_d$ and the candidate pairing was defined as $\lambda_i\leftrightarrow-\lambda_{d-i+1}$.

**Observation.** Across the four configurations, the relative discrepancy of the empirical pairing with respect to the top-ten mean magnitude of the spectrum ranged from 0.037 to 0.175. Symbolically, defining

<a id="display-0642-pairing-relative-discrepancy"></a>
$$
\Delta_i^{(h)}=
\frac{|\lambda_i^{(h)}+\lambda_{d-i+1}^{(h)}|}
{\frac1{10}\sum_{j\in\mathrm{Top10}}|\lambda_j^{(h)}|},
\qquad i=1,\ldots,5,
$$

the maximum relative discrepancy $\max_i\Delta_i^{(h)}$ took the values approximately 0.072, 0.175, 0.175, and 0.037 across the four configurations, while the mean discrepancy remained substantially smaller in each case. The pairing is visually strong and numerically small on average, although the worst-case normalised discrepancy reaches approximately 17.5% in the MLM configurations on MNLI, and is smallest on the no-MLM configuration.

<a id="conjecture-16-48"></a>
**Conjecture 16.48 (Empirical pairing of the signed spectrum).** [P-COND] *For the aggregated signed residual operator $A_{\mathrm{res}}^{\mathrm{sig}}$ on representative natural-language objectives, and for the per-head $A_h^{\mathrm{sig}}$ inferred from it under the aggregation map of Section 16.13.1, the spectrum*

> Boundary: Conjecture 16.48 continues on [PDF 643](0643-0646-ch16-level-two-pruning-and-stabilisation.md#pdf-0643).
