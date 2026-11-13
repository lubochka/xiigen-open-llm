# Layer 2 — Nine-Node 1.1 Pairs (018)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: nine_node_flow
# UX Scenario: WishlistAndSavedItems — UI alone — trusted_learned

---

### Node 01 — DomainSplitterUnit

A:
  TransitionId: "transition-B01-l2-018-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  UserRequest: "Map all screens in the wishlist page and inventory all SavedItems UI components."
  PredicateIds: ["UI"]

B:
  DomainPackets:
    - PacketId: "B01-l2-018-packet-ui"
      DomainTag: "UI"
      Focus: "Wishlist and SavedItems screen inventory — populated state, empty state, component types"
      UserSignal: "screens, wishlist page, SavedItems, UI components"
  PacketCount: 1 | Confidence: 0.92

### Node 02 — DomainTranslatorUnit

A: DomainPacket — UI
B:
  WorkUnits: ["Identify wishlist screen states", "Inventory SavedItems components", "Map button interactions"]

### Node 03 — SchemePlannerUnit

A: [TranslatedPacket]
B:
  WorkBranches:
    - WISHLIST-SCREENS: WishlistPagePopulated, WishlistPageEmpty, MoveToCartModal
    - SAVED-ITEMS-COMPONENTS: ItemCard, ShareButton, RemoveButton, AddToCartButton
    - VISUAL-TOKENS: ColorTokens, SpacingTokens, TypographyTokens

### Node 04 — DomainProcessorUnit

A: WorkBranch — WISHLIST-SCREENS
B:
  ProcessedScreens:
    - ScreenId: "WSH-001" | Name: "WishlistPagePopulated" | Type: "grid" | ItemCount: "variable"
    - ScreenId: "WSH-002" | Name: "WishlistPageEmpty" | Type: "empty-state"
    - ScreenId: "WSH-003" | Name: "MoveToCartModal" | Type: "modal" | ActionButtons: 2

### Node 05 — ApplierUnit

A: ProcessedScreens
B:
  AppliedScreens:
    - ScreenId: "WSH-001" | Layout: "3-col desktop / 1-col mobile" | Spacing: "space-4 card-gap"
    - ScreenId: "WSH-002" | Layout: "centered-empty" | Typography: "heading-md" | Color: "surface-muted"
    - ScreenId: "WSH-003" | Layout: "modal-sm" | Buttons: ["AddToCart(brand-primary)", "Cancel(neutral)"]

### Node 06 — ResultTranslatorUnit

A: [All wishlist screens and components]
B: ScreenCount: 3 | ComponentCount: 4 | VisualTokenSets: 3 | Unresolved: 0

### Node 07 — ArbiterSelectorUnit

A: [ResultTranslation]
B: ArbiterType: "StandardScreenInventoryArbiter"

### Node 08 — ArbiterUnit

A: ArbiterType: "StandardScreenInventoryArbiter"
B:
  Verdict: "APPROVED"
  GateBCheck: "3 screens; 4 components; all states documented"

### Node 09 — FormatterUnit

A: ArbiterVerdict — APPROVED
B:
  FinalHumanResult:
    Title: "WishlistAndSavedItems Screen and Component Inventory"
    SubPhaseId: "1.1"
    ScreenCount: 3
    ComponentCount: 4
    Screens:
      - ScreenId: "WSH-001" | Name: "WishlistPagePopulated" | EntryConditions: ["items saved"] | ExitTransitions: ["→WSH-003 (move to cart)", "→product page"]
      - ScreenId: "WSH-002" | Name: "WishlistPageEmpty" | EntryConditions: ["no saved items"] | ExitTransitions: ["→browse products"]
      - ScreenId: "WSH-003" | Name: "MoveToCartModal" | EntryConditions: ["from WSH-001"] | ExitTransitions: ["→cart", "→WSH-001 (cancel)"]
    GateBResult: "PASS: 3 screens; entry/exit documented; 4 components inventoried"
  Confidence: 0.92
