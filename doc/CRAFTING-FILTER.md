# Crafting search memory

The crafting menu keeps the last search you confirmed. Closing it and pressing the craft key again shows that same search. This matches the construction menu's saved filter (`uistate.construction_filter` in the world's `uistate.json`), except the crafting menu actually applies the saved text when it opens. In 0.I-1 the construction menu stores the text and then clears the on-screen copy on open.

The value lives on the world UI state, next to recipe favorites. It is not a separate global file and it is not stored on the character. Characters in the same world share it, the same way they share the construction filter.

A search started by the game, such as "craft this item" (`r:<name>`) or opening an in-progress craft, is used for that opening only. It does not replace the remembered search. Reset (`r` in the crafting menu, the existing Reset filter binding) clears the remembered text. Confirming an empty search clears it too. Favorites, hidden recipes, and categories are unchanged.

The filtered view labels the query and the reset key on the subhead row (`Filter: …  [r] Reset`) so a restored search is not a blank "Searched" tab.

## Manual check

1. In game, open crafting (`&` / Shift+7).
2. Press `f`, type `candle`, confirm. The list is filtered and the subhead shows the query.
3. Close the menu with Escape. Open crafting again. Do not press `f`. The `candle` filter is still there.
4. Press `r`. The category tabs return. Close and open crafting again. The list is unfiltered.
5. Optional, only if you care about the save file: set a filter, save and quit, load the same world, open crafting. The filter is still there. It is written with the rest of `uistate.json` when the world is saved.

No new keys. Reopening a search does not ask you to type it again. Reset is the same key it already was.
