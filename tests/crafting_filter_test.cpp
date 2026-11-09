#include <sstream>
#include <string>

#include "cata_catch.h"
#include "cata_scope_helpers.h"
#include "crafting_gui.h"
#include "json.h"
#include "json_loader.h"
#include "uistate.h"

static void round_trip_uistate()
{
    std::ostringstream os;
    JsonOut jsout( os );
    uistate.serialize( jsout );

    const std::string saved = uistate.crafting_filter;
    uistate.crafting_filter = "clobbered-before-load";
    const JsonValue jsin = json_loader::from_string( os.str() );
    uistate.deserialize( jsin.get_object() );
    CHECK( uistate.crafting_filter == saved );
}

TEST_CASE( "crafting_menu_remembers_search", "[crafting][ui]" )
{
    restore_on_out_of_scope<std::string> restore_filter( uistate.crafting_filter );
    restore_on_out_of_scope<decltype( uistate.favorite_recipes )> restore_favs(
        uistate.favorite_recipes );
    const auto favorites_before = uistate.favorite_recipes;

    SECTION( "reopen without a new search restores the remembered filter" ) {
        remember_crafting_filter( "candle" );
        // Opening craft again passes an empty caller filter. No extra keys.
        CHECK( crafting_filter_for_menu_open( "", false ) == "candle" );
    }

    SECTION( "a one-shot caller filter does not replace the remembered search" ) {
        remember_crafting_filter( "candle" );
        CHECK( crafting_filter_for_menu_open( "r:knife", false ) == "r:knife" );
        CHECK( uistate.crafting_filter == "candle" );
        CHECK( crafting_filter_for_menu_open( "", false ) == "candle" );
    }

    SECTION( "jumping to a recipe does not apply or replace the remembered search" ) {
        remember_crafting_filter( "candle" );
        CHECK( crafting_filter_for_menu_open( "", true ).empty() );
        CHECK( uistate.crafting_filter == "candle" );
    }

    SECTION( "confirming a search remembers it, including an empty confirm" ) {
        remember_crafting_filter( "rag" );
        CHECK( uistate.crafting_filter == "rag" );
        remember_crafting_filter( "" );
        CHECK( uistate.crafting_filter.empty() );
        CHECK( crafting_filter_for_menu_open( "", false ).empty() );
    }

    SECTION( "reset clears the remembered search" ) {
        remember_crafting_filter( "splint" );
        clear_remembered_crafting_filter();
        CHECK( uistate.crafting_filter.empty() );
        CHECK( crafting_filter_for_menu_open( "", false ).empty() );
    }

    SECTION( "unusual and no-match searches round-trip" ) {
        const std::string queries[] = {
            "zzz_no_such_recipe",
            "c:plastic,-rotten",
            "-clean",
            "  spaced  ",
            "木"
        };
        for( const std::string &query : queries ) {
            remember_crafting_filter( query );
            CHECK( crafting_filter_for_menu_open( "", false ) == query );
            round_trip_uistate();
            CHECK( uistate.crafting_filter == query );
        }
    }

    SECTION( "favorites are left alone" ) {
        remember_crafting_filter( "band" );
        clear_remembered_crafting_filter();
        CHECK( uistate.favorite_recipes == favorites_before );
    }
}
