# Deck UI

`DeckScreen.prefab` is placed under `UIG_BottomNavMenu/UI_Deck/DeckLayout` in MainMenu.
It follows the GDD Units and Relics mockups using standard uGUI and TMP components.
The existing bottom navigation opens/closes the existing Deck CanvasGroup.

- `EquippedDeck/UnitSlots`: five unit slots (three example units, two empty).
- `EquippedDeck/RelicSlots`: one empty slot and two example locked slots (Stage 1 / 100 gems).
- `Tabs`: Units and Relics buttons use Inspector SetActive events to switch pages.
- `UnitsPage/Viewport/Content`: Your Units and Unowned Units rows.
- `RelicsPage/Viewport/Content`: Your Relics row.

All ownership, equipped labels, names, and unlock requirements are display placeholders.
Card buttons deliberately have no purchase/equip/unlock handlers. The backend developer can
populate the named cards and attach listeners without replacing the layout. No runtime
scripts, player-data changes, or backend calls are added by this UI.

The original Deck placeholder is kept inactive. The shared profile, resources and footer
remain in their existing objects. To edit visible Deck content outside Play Mode, temporarily
set UI_Deck's CanvasGroup alpha to 1, then restore it to 0 before saving.
