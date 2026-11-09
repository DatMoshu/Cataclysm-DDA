#pragma once
#ifndef CATA_SRC_CRAFTING_GUI_H
#define CATA_SRC_CRAFTING_GUI_H

#include <string>
#include <string_view>
#include <utility>
#include <vector>

#include "type_id.h"

class Character;
class JsonObject;
class inventory;
class recipe;
class recipe_subset;

/**
 * Open crafting menu where user selects who will craft what (the crafter & the recipe).
 *
 * @param crafter: who opened the menu - the initial crafter
 * @param filterstring: initial filter to execute
 *
 * Return: if recipe * is not nullptr, then Character * is not nullptr either.
 */
std::pair<Character *, const recipe *> select_crafter_and_crafting_recipe( int &batch_size_out,
        const recipe_id &goto_recipe, Character *crafter, std::string filterstring = "",
        bool camp_crafting = false, inventory *inventory_override = nullptr );

/**
 * Filter the crafting menu should show when it opens.
 *
 * An empty caller_filter restores the remembered search. A non-empty
 * caller_filter (the "craft this item" shortcut) or a jump to a specific
 * recipe is used for this opening only and does not replace the memory.
 */
std::string crafting_filter_for_menu_open( std::string_view caller_filter, bool have_goto_recipe );

/** Remember a search the player confirmed. An empty string clears the memory. */
void remember_crafting_filter( std::string_view filter );

/** Forget the remembered crafting search. The reset-filter key uses this. */
void clear_remembered_crafting_filter();
std::pair<std::vector<const recipe *>, bool> recipes_from_cat( const recipe_subset
        &available_recipes, const crafting_category_id &cat, const std::string &subcat );

void load_recipe_category( const JsonObject &jsobj, const std::string &src );
void reset_recipe_categories();

// Returns nullptr if the category does not exist, or a pointer to its vector
// of subcategories it the category does exist
const std::vector<std::string> *subcategories_for_category( const std::string &category );

struct crafting_category {
    crafting_category_id id;
    bool was_loaded = false;

    bool is_hidden;
    bool is_practice;
    bool is_building;
    bool is_wildcard;
    std::vector<std::string> subcategories;

    void load( const JsonObject &jo, std::string_view src );
};

#endif // CATA_SRC_CRAFTING_GUI_H
