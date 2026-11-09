# Quality-of-life audit for 0.I-1

Source of truth: commit `7b2efa5cea38e4d4d97dd0e63b28b9148623da59` (tag `0.I-1`) plus one play of that binary. Nothing in this document is implemented. Turn-based costs, danger, and explicit confirmation of destructive acts stay visible. This is not an auto-play design.

Key counts below were produced by the key log in `tools/vm-baseline/playtest-keys.txt` on the tiles build. Counts are for the sequence that actually changed game state. Failed keypresses during the reload attempt are listed separately and are not treated as required steps.

## What already helps

These are in 0.I-1 today. A change should extend them, not replace them.

- **Play Now and Random** skip the eight character-creation tabs. Play Now also skips the world-setup screen (`main_menu.cpp` calls `pick_world` with prompts off, and `make_new_world(false)` skips `show_worldgen_basic`). Measured: `n` then `d` reached world generation with no "Are you SURE" dialog.
- **Autoselect one target** is on by default. Option `AUTOSELECT_SINGLE_VALID_TARGET` (interface, accessibility group, default true) and `choose_adjacent_highlight(..., allow_autoselect = true)` skip the direction prompt when exactly one adjacent tile is valid for examine, open, or pickup. Measured: after the ear plugs were the only item on the ground, `g` opened the pickup list immediately.
- **Advanced inventory** (`/`) already moves one item (Enter), a stack (`M`), or everything (`,`). Pane filters are stored on `uistate.transfer_save` and serialized (`uistate.h`, `advanced_inv_pane_save_state::filter`).
- **Pickup and drop are already multi-select.** The pickup screen's select-all key is `,`. Drop is the multidrop screen ("type a number before selecting").
- **Recraft** (`-`) repeats `lastrecipe` and `last_batch` (`Character::recraft`). **Craft as long as possible** exists as `long_craft` and has no default key.
- **Construction memory** restores `uistate.construction_filter` and `uistate.last_construction`. Crafting does not restore the active filter (see below).
- **Favorites** exist for items and for recipes (`uistate.favorite_recipes`). `AUTO_INV_ASSIGN` defaults to `favorites`.
- **The action menu** (Enter) already sorts nearby open, close, examine, and butcher to the top when `can_interact_at` says they are legal (`handle_action.cpp` / `action.cpp`, weight >= 200). Quicksave is added to that menu only when it already has a hotkey.

## Measured flows

### 1. New game, default scenario

Keys: `n`, `d`. Count: **2**, then unattended world generation.

No world-name screen and no character tabs. The result was world Kennard Corner, character Drema Hutson, in the evac shelter.

The custom path is the long one. `newcharacter.cpp` walks eight tabs: POINTS, SCENARIO, PROFESSION, BACKGROUND, STATS, TRAITS, SKILLS, DESCRIPTION. Leaving the last tab calls `query_yn("Are you SURE you're finished?")`, and an empty name asks a second question. That path was not played, so it has no measured key count. The tab list and the two prompts are in the source.

### 2. One step

Keys: one arrow. Count: **1** per orthogonal step.

Right moved the character from inside the shelter onto the dirt east of the door. Up onto the bench printed `Moving onto this bench is slow.` The move pool on the sidebar changed. Time cost stayed visible. Nothing to remove here.

### 3. Inventory open and close

Keys: `i`, Escape. Count: **2**.

The screen showed carried and worn items and the hint `[/] Filter`. Filter is per opening; this was not a repeated-search test, so no claim is made that the inventory filter is forgotten.

### 4. Drop one item

Keys: `d`, `c`, Return. Count: **3**.

`c` was the letter on the ear plugs row in that specific multidrop list. The list is the batch UI: a count can be typed first, and more letters can be marked before Return. There was no extra "are you sure" for an ordinary drop. The message log said the pair was dropped on the bench.

### 5. Pick that item back up

Keys: `g`, `a`, Return. Count: **3**.

`g` did **not** ask for a direction. The pickup list opened on the bench, which was the only tile with items. `a` was the letter on that row. The log then said `You pick up 1 pair of ear plugs`.

This is the single-target case the existing option already covers. It is not evidence about a floor with several piles.

### 6. Save, leave, and load again

Save and quit: Shift+S, then `Y`. Count: **2**. The dialog was `Save and quit? (Case Sensitive)` with `Y/yes` and `N/no`. It returned to the main menu and did not exit the process.

Leave the process from that menu: `q`, then `Y`. Count: **2**. Exit code 0.

Load in a new process of the same binary and the same `--userdir`:

- The menu opened on Load, showing Kennard Corner with one character.
- Return opened the character list.
- The character row's key was `1`. Pressing `1` loaded the game.

Successful load keys: Return, `1`. Count: **2**, plus load time.

The key log also has `a`, Up, and Return from an earlier attempt that did not load the character, because `a` is not the row key. Those three are operator misses, not a required prefix.

After load, the shelter, the message log (including the ear-plug lines), and the inventory contents were still there. `i` showed the ear plugs again.

## Gaps, ranked

Rank is benefit for repeated play divided by the chance of hiding a cost or doing the wrong thing. None of these are scheduled until one of them is chosen.

### 1. Show the actions that already exist and have no key

Benefit: high. Risk: low, if no new default key is forced.

Unbound in `data/raw/keybindings.json` while the code is already there:

| Action | Id | What it already does |
| --- | --- | --- |
| Pick up items from all nearby tiles | `pickup_all` | `game::pickup_all` opens the pickup list for the current tile and every adjacent tile. Still a list, still a confirm. |
| Craft as long as possible | `long_craft` | Repeats a chosen recipe until it cannot continue. Time still passes per craft. |
| Quicksave | `quicksave` | Saves without quitting. Omitted from the Enter menu unless a hotkey exists (`action.cpp`, `hotkey_for_action(ACTION_QUICKSAVE)`). |
| Quickload | `quickload` | Loads the last save. Higher risk than quicksave because it discards unsaved minutes. |
| Toggle auto pickup | `toggle_auto_pickup` | Existing option. Easy to turn into silent looting, including items the player did not look at. |
| Unload container | `unload_container` | Directional unload. |
| Quick toggle hauling | `haul_toggle` | Hauling already has `\`. |

First change, if this is chosen: list quicksave, pickup_all, and long_craft in the Enter menu even when unbound, with the words "unbound". Do not invent a default key in the same change. F5 is already `TOGGLE_NUMPAD_NAVIGATION`. A second change can bind keys after a conflict check.

Do not bind auto pickup as a recommended default. The player should still see the items.

### 2. Remember the crafting filter the way construction already does

Implemented on this branch. See `doc/CRAFTING-FILTER.md`. The remembered search is `uistate.crafting_filter` in the world's `uistate.json`. Reset clears it. A one-shot caller filter does not replace it.

Benefit: high for anyone crafting more than once. Risk: low if the filter stays visible.

The `&` action calls `player_character.craft()` with the default empty filter (`handle_action.cpp`). The item menu can pass `r:<item name>` once (`view_recipe_crafting_menu` in `game.cpp`). That string is not what the next `&` opens with. The search popup's history id is `craft_recipe_filter`, so previous searches can be recalled with up/down inside the popup, but the list itself opens unfiltered. Construction copies `uistate.construction_filter` back into the menu and writes it on the way out (`construction.cpp`).

Proposal: store the active crafting filter next to `favorite_recipes` / `recent_recipes` and show it in the header that already renders a filter. Keep `r` (`RESET_FILTER`) as the clear. Do not start a craft because the filter matched one recipe.

### 3. Use the existing all-nearby pickup for several piles

Benefit: high while looting a room. Risk: medium if it picks a tile the player did not mean.

Flow 5 shows the one-tile case is already short. The remaining cost is several adjacent tiles: `g`, a direction, mark items, Return, then again for the next tile. `pickup_all` is the code path that already gathers those tiles into one list.

Proposal: once it is discoverable (item 1), keep the list and the confirm. Show which tile each row came from (the bench row in the playtest was already labeled `BENCH`). Do not skip the confirm, and do not start the activity without the player pressing it. Time cost stays the normal pickup activity.

### 4. Remove the second "are you sure" on world creation and on the last character tab

Benefit: medium, and only for people not using Play Now. Risk: low if Esc still aborts.

`worldfactory.cpp` asks `Are you SURE you're finished?` after Finish is chosen, including when the name is already valid. `newcharacter.cpp` asks the same question when leaving the description tab with a name filled in. Play Now already skips both. Measured new-game path did not see them.

Proposal: Finish on those two screens commits. Keep the empty-name question, because that one changes the name rather than repeating the button. Keep the save-and-quit `Y`. That dialog is the difference between saving and walking away; the playtest had to answer it, and quicksave (item 1) is the way to save without leaving.

### 5. One line for the single nearby action the Enter menu already computed

Benefit: medium. Risk: medium if the line spoils information look would not show.

`handle_action_menu` already finds a legal open, close, or examine in the eight neighbors and floats it to the top of Enter. The player still has to open that menu to see it.

Proposal: when exactly one of those actions is legal, print one log or sidebar line such as the direction and the key that already exists (`o`, `c`, or `e`). Do not press it. Do not add a line for smash, fire, or butcher of an unseen corpse beyond what examine already reveals. If two doors are in reach, print nothing and keep the direction prompt (autoselect already handles the exactly-one case).

### 6. Same filter memory for the surrounding-items list

Benefit: medium. Risk: low.

`uistate.list_item_filter` is marked not serialized. The AIM filter and the construction filter are serialized. After a save, `V` (list items around the player) forgets its filter and the other two do not.

Proposal: serialize that filter with the same visible "filter active" text the list already has, and a clear key. Do not change what `V` reveals.

### 7. A shorter custom character, with the points still on screen

Benefit: medium for people who refuse Play Now. Risk: higher than the items above, because it is a real UI flow.

Eight tabs, then the sure-prompt in flow 1. Play Now and Random already exist, so this is not the first thing to build.

Proposal, only if chosen later: a path that asks scenario, profession, and name, and keeps a points total on screen, with the other five tabs one key away. Do not hide remaining points. Do not add a campaign or a new scenario.

## Out of scope for the first change

- The `csharp-mapgen` branch and any settlement or civilization work.
- A new renderer, a new UI toolkit, or a rewrite of the tiles path. 0.I-1 is SDL2. Later experimentals moved toward SDL3; that move is not part of this baseline.
- Survival numbers, loot, monster stats, or bringing back removed content.
- Any action that spends turns, moves items, or walks without a confirm and a visible time cost.

## Suggested first pick

Item 1, and only the "show unbound actions in the Enter menu" half. It is a small, testable change, it does not spend turns, and it makes quicksave, pickup_all, and long_craft findable. Item 2 is the next small change if crafting friction is the priority instead.
