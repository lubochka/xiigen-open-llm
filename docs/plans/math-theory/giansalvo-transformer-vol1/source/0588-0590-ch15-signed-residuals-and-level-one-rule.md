---
title: "Chapter 15: signed residual geometry and the level-one protection rule"
source_pdf_pages: [588, 590]
book_pages: [552, 554]
batch: "012"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 15: signed residual geometry and the level-one protection rule

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous](0585-0587-ch15-empirical-protocol-implementation-and-conditions.md) · [Next: proof and three regimes](0591-0592-ch15-regimes-and-dynamic-conclusion.md)

> Editorial note: Full transcription of PDF 588–590 (printed pages 552–554). The author's status labels, sign conventions, non-negativity claims, and proportionality formula are retained literally. In Definition 15.23 the absolute-value bars are visible in the PDF and are preserved here. Proposition 15.24's proof starts on PDF 591; editorial checks are separate in the Russian notes.

<a id="pdf-0588"></a>
## PDF 588 · Book page 552

<a id="table-15-14"></a>
| Core statement | Condition required | If condition fails |
|---|---|---|
| Diagnostic stability | Two-time-scale window and concentration | Use longer windows or slower controller updates. |
| Coverage decrease | Implementable head near $\sigma^\star$ | Use template search, split a nearby head, or reject growth. |
| Directional preservation | Anchor constraint enforced | Directional starvation becomes possible. |
| Finite termination | Finite useful cover and improvement floor | Use homeostatic tracking rather than termination. |

**Table 15.14:** Conditional points that remain inside the core INCRT theorem package.

<a id="section-15-44"></a>
## 15.44 From scalar residual diagnostics to signed residual geometry

The core controller of this chapter operates on a small list of scalar diagnostics: the coverage residual $\rho_\ell(t)$, the residual amplification $\Gamma_{\mathrm{res}}^{(\ell)}(t)$, the orbital crowding $C_{\mathrm{orb}}^{(\ell)}(t)$, the marginal value $V_{\mathrm{marg},h}^{(\ell)}(t)$, the directional ratio $\alpha_h^{(\ell)}(t)$, and the residual gate $g_h^{(\ell)}(t)$. Each of these is a non-negative aggregate—a squared distance, a ratio of norms, an ablation gap, or a bounded coefficient—and the controller reads them as magnitudes. This is enough to drive the four event templates of the core chapter, and the theorems of Sections 15.8–15.11 go through with magnitude information alone.

There is, however, a structural feature of the residual that magnitudes alone cannot see. The residual of the attention block is naturally an *indefinite* object: it carries both positive and negative responses along an internal eigendecomposition of its symmetric algebraic part, and the controller, when it consults only $\rho_\ell$ and $\Gamma_{\mathrm{res}}^{(\ell)}$, sees the sum of squared contributions rather than the signed breakdown. This section records that limitation, identifies one minimal correction that already belongs in the core chapter, and points to the matrix and spectral refinement that is the proper subject of INCRT II. No rigorous spectral claim is made here. The role of this section is exactly that of a bridge: it makes the boundary of the scalar core explicit and announces where it is crossed.

<a id="section-15-44-1"></a>
### 15.44.1 The scalar diagnostics are sign-blind

A direct inspection of the core state shows the limitation. The coverage residual (15.4) is the second moment of a distance to the dictionary; the residual amplification (15.5) is the ratio of two non-negative norms; the marginal value (15.6) is a difference of losses but enters the controller only through its magnitude. None of these quantities distinguishes an under-covered region whose residual energy is concentrated on a single directional mode of one sign from an under-covered region whose residual energy is distributed across two opposite directional modes that nearly cancel in any aggregate. The two cases produce the same $\rho_\ell$ and the same $\Gamma_{\mathrm{res}}^{(\ell)}$ up to higher-order corrections, yet they call for different architectural events. The first asks for one head added near a single signature region; the second asks either for a paired head allocation around an oriented pair of signatures or for a single head equipped with a signed gate.

The sign-asymmetry becomes sharpest in the head-response protection rule used by the operational INCRT formulation that this chapter rationalises. In that formulation a head is declared a preservation candidate, that is to say protected from pruning, through a scalar

<a id="pdf-0589"></a>
## PDF 589 · Book page 553

<a id="table-15-15"></a>
| Core scalar quantity | What it captures | What it does not see |
|---|---|---|
| $\rho_\ell(t)$ | Aggregate distance from signatures to dictionary atoms. | Whether the residual coverage gap is concentrated on a single oriented mode or on a pair of opposite modes. |
| $\Gamma_{\mathrm{res}}^{(\ell)}(t)$ | Aggregate residual energy injected into the layer. | The signed decomposition of that energy across the eigendecomposition of the symmetric residual operator. |
| $V_{\mathrm{marg},h}^{(\ell)}(t)$ | Importance of head $h$ measured by ablation or proxy. | Whether the head responds along a positive or a negative oriented mode of the residual operator. |
| $\gamma_h^{(\ell)}(t)>\phi_g$ | Preservation candidacy through a scalar threshold. | The corresponding preservation candidacy of an equally informative head with negative $\gamma_h$. |

**Table 15.15:** Scalar diagnostics of the core controller, and the oriented information that they aggregate. The right column motivates the level-one correction of Section 15.44.3 and the matrix refinement of INCRT II.

threshold on its response,

<a id="equation-15-62"></a>
$$
\operatorname{protect}_h(t)\Longleftrightarrow
\gamma_h^{(\ell)}(t)>\phi_g,
\tag{15.62}
$$

where $\gamma_h^{(\ell)}(t)$ is a scalar head response (a signed gate activity, a contraction coefficient, or an inner product with a target direction, depending on the implementation) and $\phi_g$ is a preservation threshold. Pruning candidacy, in this formulation, is the complementary event $|\gamma_h^{(\ell)}(t)|\le\phi_g$ sustained over the required control window, subject to the marginality and crowding conditions of Section 15.7.3. The core pruning rule of the present chapter does not refer to $\gamma_h$: it is built on the sign-neutral diagnostics $V_{\mathrm{marg},h}$ and $C_h$ of Section 15.7.3. An implementation that wires the operational protection rule (15.62) into the core controller, however, inherits its sign-asymmetry. A head whose response is large and negative—which, on the underlying geometry, would be just as informative as a head whose response is large and positive—fails the threshold and is left unprotected. This is a sign-asymmetry of the imported rule, not a property of the underlying geometry. We register it here and correct it minimally in Section 15.44.3 below.

The general statement is that the scalar layer of the controller sees the *magnitude* of the residual geometry, never its orientation. For most of the dynamics of the core chapter this is sufficient, because growth, anchor preservation, and termination are concerned with whether useful response exists at all. When the question becomes which orientation of the response deserves a separate head, or which pair of orientations should share a single head with a signed gate, the scalar layer is no longer the right tool.

<a id="section-15-44-2"></a>
### 15.44.2 Indefinite signature of the residual operator

The structural reason behind the sign-blindness of the scalar diagnostics is that the residual operator, written in its core algebraic form, has zero trace. We state this fact at the level appropriate to the present chapter, deferring the matrix derivation to INCRT II.

<a id="proposition-15-22"></a>
**Proposition 15.22 (Conditional signed refinement of the scalar residual).** [T-COND] Assume that the scalar residual amplification of layer $\ell$ admits the matrix refinement developed in INCRT II, with a symmetric residual component constructed from the input covariance and the antisymmetric part of the effective query–key operator. Then this component is

<a id="pdf-0590"></a>
## PDF 590 · Book page 554

traceless. Consequently, whenever it is non-zero, its spectrum contains both positive and negative directions, while the scalar diagnostic $\Gamma_{\mathrm{res}}^{(\ell)}$ records only their aggregate magnitude:

<a id="display-0590-signed-spectrum"></a>
$$
\Gamma_{\mathrm{res}}^{(\ell)}(t)\propto
\sum_i\lambda_i^{(\ell)}(t)^2,
\qquad \sum_i\lambda_i^{(\ell)}(t)=0,
$$

where the $\lambda_i^{(\ell)}(t)$ are the signed eigenvalues of the symmetric component.

The matrix construction underlying the proposition is given in INCRT II, together with its proof. Within the present chapter we use only its scalar consequence: a non-zero $\Gamma_{\mathrm{res}}^{(\ell)}$ is compatible with an internal signed structure that the scalar controller does not see. No statement about a specific pairing of positive and negative eigenvalues is made here.

**Empirical pairing is conditional, not theorematic.** In numerical experiments documented in INCRT II, the signed eigenvalues of the symmetric component show approximate pairing of the form $(+\lambda,-\lambda)$ across several training setups, with small relative discrepancy with respect to the dominant magnitudes. This is an empirical observation, and its status in INCRT II is [P-COND]. Zero trace alone does not force a $(+\lambda,-\lambda)$ pairing of the eigenvalues: a low-dimensional counterexample shows that traceless symmetric matrices need not have paired spectra. A genuine pairing requires a structural involution—an orthogonal map $J$ with $J^2=I$ and $JA_{\mathrm{res}}J^\top=-A_{\mathrm{res}}$ in the matrix language of INCRT II—and the conditions under which such an involution exists are part of the analysis of the next chapter. Within the present chapter we record the empirical pattern but build no theorem on it.

<a id="section-15-44-3"></a>
### 15.44.3 A minimal sign correction at the scalar level: the level-one diagnostic

The single architectural consequence of the discussion above that is small enough to remain inside the core chapter is the symmetrisation of the protection rule (15.62). We adopt it explicitly.

<a id="definition-15-23"></a>
**Definition 15.23 (Level-one diagnostic rule).** [NEW][DEF] The *level-one diagnostic rule* of INCRT replaces the protection inequality $\gamma_h^{(\ell)}(t)>\phi_g$ of (15.62), in any implementation that imports the operational INCRT head-response protection rule, by the symmetric inequality

<a id="display-0590-level-one-protection"></a>
$$
\operatorname{protect}_h(t)\Longleftrightarrow
\bigl|\gamma_h^{(\ell)}(t)\bigr|>\phi_g,
$$

possibly applied to a Polyak–Ruppert average $\bar\gamma_h^{(\ell)}(t)$ over the current growth window. Equivalently, a head is flagged for pruning only when $|\gamma_h^{(\ell)}(t)|\le\phi_g$ over the required control window, jointly with the marginality and crowding conditions of Section 15.7.3. The remaining four event templates—add, split, expand, terminate—and the core pruning rule of Section 15.7.3 are unchanged.

This is the smallest change that removes the sign-asymmetry of the imported protection rule without altering the rest of the controller. It does not require any matrix or spectral object: $\gamma_h^{(\ell)}(t)$ remains a scalar head response, and the threshold $\phi_g$ remains a scalar bias tuned by validation. The only change is that heads with informative negative response are now preserved together with heads with informative positive response.

<a id="proposition-15-24"></a>
**Proposition 15.24 (Preservation of the core theorem package).** [T-PROVED] Under Definition 15.23, the four core results of this chapter—diagnostic stability (Section 15.8), coverage-residual decrease under growth (Section 15.9), directional-tail preservation (Section 15.10), and finite termination (Section 15.11)—continue to hold without modification of their hypotheses or of their conclusions.

