WIZARD TOWER - 16x16 PIXEL ART SIDESCROLLER TILESET  v1.8
by bubbabba  -  https://bubbabba.itch.io

WHAT IS INSIDE
  tileset/wizard_tower_tiles_16.png   172 tiles on a 16x16 grid (12 columns)
  tileset/wizard_tower_tiles_32.png   the same atlas at 2x, for 32x32 projects
  tileset/tiles/                      every tile as its own PNG, named
  props/                              91 static props (furniture, shelves, windows, doors,
                                      kitchen, arms and armour, yard and garden...)
  animated/*_strip.png                42 animated props as horizontal frame strips
  animated/frames/                    the same animations, one PNG per frame
  animated/gif/                       previews
  animated/frames.txt                 frame counts, sizes and suggested speeds
  backgrounds/                        parallax layers (sky, mountains, far + near forest),
                                      day and night, 384 px wide, loop seamlessly
  glow/                               light pools (warm, teal, purple, gold; 32/64/128 px)
                                      as alpha overlays - put them over walls near lamps
  characters/                         11 NPCs (archmage, apprentice, witch, knight, merchant,
                                      noble, maid, bard, farmer, monk, sweeper), each with an
                                      8-frame idle and an 8-frame walk, 32x40 px, + GIFs
  scenes/                             the example scenes from the store page as PNGs
  tiled/                              Tiled tilesets (.tsx) + the tower and the house as
                                      maps (.tmx), animations already set up on the props

THE TILES
  tower_*     warm limestone walls, 9-slice (tl t tr / l c r / bl b br)
  keep_*      dark interior stone, 9-slice
  plaster_0-3, stonebg_0-3, slate_0-3   2x2 fill blocks for back walls and roofs
  floor_wood_*, floor_stone_*           platforms you stand on: l, m, m2, r
  wainscot_0-1, planksbg_0-1            wall panelling and plank back walls
  ground_top_0-2, ground_fill_0-2       grass and dirt, repeat every 3 tiles
  roof_l / roof_r                       45-degree roof edges
  roof_in_l / roof_in_r / roof_in_peak  the tile just inside the edge: finishes the
                                        slate band and the rafter for cut-away roofs
  roof_peak, eave_0-1, stairs_l, stairs_r

  New in v1.8: walk pass: side walks redrawn - knees bend, feet lift heel-first and
  reach forward, both feet swing from one point under the body, and the
  planted foot stays flat on the ground in every frame; boots and shoes
  have a shaped cuff, toe cap and a solid flat sole.

  New in v1.7: fix pass - flames, candles, the cat, stove, floating book, orrery
  and wall sconce no longer clip at their frame edges (a few animations are a few
  px taller or wider; they keep their bottom anchor), and the hourglass now turns
  over at the end of its loop instead of snapping back to full.

  New in v1.6: 45 new props, 21 of them animated - a smoking chain lantern, orrery,
  spinning wheel, scrying orb, incense burner, bubbling flask rack and tube, wall
  sconce, glowing rune stone, floating candles, cuckoo clock, goldfish bowl,
  steaming kettle, oil lamp, canary in a cage, waving pennant, grindstone,
  levitating crystal, sleeping hound, mouse hole and a dangling spider; plus a coat
  rack, urn fern, orange tree, landscape, wall shelf, plate rack, jugs, keg rack,
  crate stack, wine rack, marble bust, anvil, broom, wheelbarrow, log pile, specimen
  jars, hanging hams, rolled rugs, hand cart, fence, tea table, swag valance, bone
  pile and footstool. The scenes are fuller too.

  New in v1.2: smoother animations - every flame, the owl, cat, clock, stove,
  alembic and crystal ball now loop over 8-16 frames; the grandfather clock
  pendulum is centred and stays inside its case.

  New in v1.1:
  wallpaper_red_0-3, wallpaper_green_0-3   damask wallpaper, 2x2 fill blocks
  brick_0-3, cellar_0-3, clay_0-3          brick, rough cellar stone, clay roof fill
  clayroof_*, clayeave_0-1                 a red clay roof set, same layout as the slate one
  floor_beam_*, floor_carpet_*             timber storey floor with a beam; floor with a runner
  cobble_top_0-2                           cobbled street surface
  cornice_0-1, skirting_0-1                overlays for the top / bottom row of any wall
  post_t/m/b, brace_l/r, log_end           timber frame overlays (posts, knee braces)
  ladder_t/m/b, rail_l/m/r                 ladders and a balustrade
  crenel_0-1                               battlements (one merlon every 2 tiles)
  tuft_0-3                                 grass and flower tufts, go in the row above ground

SEAMLESS WALLS
  The stone textures repeat every 2 tiles, so each 9-slice piece comes in four
  versions: no suffix, _v1, _v2, _v3. Pick by where the tile sits:
      (x even, y even) -> none   (x odd, y even) -> _v1
      (x even, y odd)  -> _v2    (x odd, y odd)  -> _v3
  where x, y are the tile's grid position minus its place in the 3x3 slice
  (tl = 0,0 ... br = 2,2). Or ignore it - any version works, the matching one
  just hides the repeat. The fill blocks work the same way: plaster_(x%2 + 2*(y%2)).

PROPS
  Props sit on the floor line: put the bottom row of the sprite on the top row of
  the floor tile. Animated props loop; suggested speeds are in frames.txt.

CHARACTERS
  characters/<name>_idle_strip.png and <name>_walk_strip.png, 8 frames of 32x40.
  They face right; flip for left. The feet stand on the bottom 2 rows, so put the
  bottom row on the floor line like a prop. Idle has blinks, breathing and a little
  action each (the archmage casts, the bard strums, the sweeper sweeps...).

NIGHT
  The night scenes are the day tiles with a dark blue layer on top (RGBA 12,14,40
  at about 47% opacity) and the glow overlays over lamps and fires.

LICENSE - see LICENSE.txt
