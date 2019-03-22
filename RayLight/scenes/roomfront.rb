#!/usr/bin/env ruby
require 'RayLightModelling.rb'




### header ###

$iterations       = 100
$width, $height   = 324, 200
$eyePosition      = [3.125, 1.77, 0.05]
$lookDirection    = [-0.6, -0.3, 1]
$viewAngle        = 100
$skyEmission      = [0.97585, 0.98825, 1.0359]#[8068, 9060, 12872]#[9758.5, 9882.5, 10359]
$groundReflection = [0.1, 0.09, 0.07]




### materials ###

teststuff = cover([0.7, 0.2, 0.2], [100, 1000, 100])

brass           = cover([0.541, 0.505, 0.394], cBlack)
wallMaterial    = cover([0.87, 0.87, 0.9], cBlack)
floorMaterial   = cover([0.5, 0.5, 0.45], cBlack)
ceilingMaterial = cover(cWhite, cBlack)
woodDark        = cover([0.386, 0.3375, 0.289], cBlack)
aluminium       = cover([0.34, 0.34, 0.34], cBlack)




### building ###

floor   = instance( quadrangle2, scale(4), floorMaterial )
ceiling = instance( quadrangle2, rotate(:x, 180), translate(0, 0, 1), scale(4), ceilingMaterial )

wallleftfloor   = instance( quadrangle2, scale(1, 1, 2.5), floorMaterial )
wallleftceiling = instance( quadrangle2, rotate(:x, 180), translate(0, 0, 1), scale(1, 1, 2.5), ceilingMaterial )
wallleftsill     = instance( quadrangle2, scale(0.14, 1, 2.5), woodDark )
wallleftsillside = instance( quadrangle2, scale(0.86, 1, 0.14), woodDark )
wallleftbaylower = instance( quadrangle2, translate(-1, 0, 0), rotate(:z, 90), scale(1, 0.75, 2.5), wallMaterial )
wallleftbayupper = instance( quadrangle2, translate(-1, 0, 0), rotate(:z, 90), scale(1, 0.25, 2.5), wallMaterial )
wallleftbaysidelower = instance( quadrangle2, rotate(:x, 90), scale(1, 0.75, 1), wallMaterial )
wallleftbaysideupper = instance( quadrangle2, rotate(:x, 90), scale(1, 0.25, 1), wallMaterial )
walllefttop = instance( quadrangle2, translate(-1, 0, 0), rotate(:z, 90), scale(1, 0.3, 2.5), wallMaterial )
wallleftcolumn = instance( quadrangle2, translate(-1, 0, 0), rotate(:z, 90), scale(1, 2.6, 0.75), wallMaterial )
windowcolumnpiece = instance( quadrangle2, rotate(:x, 90), scale(0.14, 1.4, 1), woodDark )
windowcolumn = group(
   windowcolumnpiece,
   instance( windowcolumnpiece, rotate(:y, -90), translate(0, 0, 0.14) )
)
windowframemainhorizontal = instance( quadrangle2, scale(0.625, 1, 0.0575), aluminium )
windowframeverticallower  = instance( quadrangle2, scale(0.0575, 1, 0.915), aluminium )
windowframeverticalupper  = instance( quadrangle2, scale(0.0575, 1, 0.255), aluminium )
windowmodulemain = group(
   instance( windowframemainhorizontal ),
   instance( windowframemainhorizontal, translate(0, 0, 0.9725) ),
   instance( windowframemainhorizontal, translate(0, 0, 1.03) ),
   instance( windowframemainhorizontal, translate(0, 0, 1.3425) ),
   instance( windowframeverticallower, translate(0, 0, 0.0575) ),
   instance( windowframeverticallower, translate(0.5675, 0, 0.0575) ),
   instance( windowframeverticalupper, translate(0, 0, 1.0875) ),
   instance( windowframeverticalupper, translate(0.5675, 0, 1.0875) )
)
windowmoduleside = instance( windowmodulemain, scale(0.86/0.625, 1, 1) )
windowmain = group(
   instance( windowmodulemain ),
   instance( windowmodulemain, translate(0.625, 0, 0) ),
   instance( windowmodulemain, translate(1.25, 0, 0) ),
   instance( windowmodulemain, translate(1.875, 0, 0) )
)
window = group(
   instance( wallleftsill, translate(-1.14, 0.75, 0.75) ),
   instance( wallleftsill, rotate(:z, 180), translate(-1, 2.15, 0.75) ),
   instance( wallleftsillside, translate(-1, 0.75, 3.25) ),
   instance( wallleftsillside, translate(-1, 0.75, 0.61) ),
   instance( wallleftsillside, rotate(:x, 180), translate(-1, 2.15, 3.39) ),
   instance( wallleftsillside, rotate(:x, 180), translate(-1, 2.15, 0.75) ),
   instance( windowcolumn, translate(-0.14, 0.75, 3.25) ),
   instance( windowcolumn, rotate(:y, -90), translate(-0.14, 0.75, 0.75) ),
   instance( windowcolumn, rotate(:y,  90), translate(-1, 0.75, 3.25) ),
   instance( windowcolumn, rotate(:y, 180), translate(-1, 0.75, 0.75) ),
   instance( windowmain, rotate(:x, 90), rotate(:y, 90), translate(-1.14, 0.75, 0.75) ),
   instance( windowmoduleside, rotate(:x, 90), translate(-1, 0.75, 3.39) ),
   instance( windowmoduleside, rotate(:x, 90), rotate(:y, 180), translate(-0.14, 0.75, 0.61) )
)
wallleft = group(
   instance( wallleftfloor,   translate(-1, 0, 0.75) ),
   instance( wallleftceiling, translate(-1, 2.3, 0.75) ),
   instance( wallleftbaylower, translate(-1, 0, 0.75) ),
   instance( wallleftbayupper, translate(-1, 2.15, 0.75) ),
   instance( wallleftbaysidelower, translate(-1, 0, 3.25) ),
   instance( wallleftbaysideupper, translate(-1, 2.15, 3.25) ),
   instance( wallleftbaysidelower, rotate(:y, 180), translate(0, 0, 0.75) ),
   instance( wallleftbaysideupper, rotate(:y, 180), translate(0, 2.15, 0.75) ),
   instance( walllefttop, translate(0, 2.3, 0.75) ),
   instance( wallleftcolumn, translate(0, 0, 0) ),
   instance( wallleftcolumn, translate(0, 0, 3.25) ),
   window
)

wallrightmain = instance( quadrangle2, scale(4, 1, 2.6), wallMaterial )
wallright = group(
   instance( wallrightmain, rotate(:x, 90), rotate(:y, -90), translate(0, 0, 4) )
   #instance( footboard, rotate(:x, 90), rotate(:y, -90), translate(0.01, 0, 4) )
)

wallbackcolumn = instance( quadrangle2, scale(1.28, 1, 2.6),  wallMaterial )
chimneyside    = instance( quadrangle2, scale(0.37, 1, 2.6),  wallMaterial )
chimneyfront   = instance( quadrangle2, scale(1.44, 1, 1.44), wallMaterial )
chimneyplace   = instance( quadrangle2, scale(1.44, 1, 1.16), woodDark )   # todo
wallback = group(
   instance( wallbackcolumn, rotate(:x, 90) ),
   instance( wallbackcolumn, rotate(:x, 90), translate(2.72, 0, 0) ),
   instance( chimneyfront,   rotate(:x, 90), translate(1.28, 1.16, -0.37) ),
   instance( chimneyplace,   rotate(:x, 90), translate(1.28, 0, -0.37) ),
   instance( chimneyside,  rotate(:x, 90), rotate(:y, -90), translate(1.28, 0, 0) ),
   instance( chimneyside,  rotate(:x, 90), rotate(:y,  90), translate(2.72, 0, -0.37) )
)

wallfrontmain = instance( quadrangle2, scale(4, 1, 2.6), wallMaterial )
wallfront = group(
   instance( wallfrontmain, rotate(:x, 90), rotate(:y, 180), translate(4, 0, 0) )
   #cupboard
   #sphere0
)

building = group(
   floor,
   instance( ceiling, translate(0, 2.6, 0) ),
   instance( wallright, translate(4, 0, 0) ),
   instance( wallback,  translate(0, 0, 4) ),
   instance( wallleft,  translate(0, 0, 0) ),
   instance( wallfront, translate(0, 0, 0) )
)




### contents ###

ballluminaire = instance( blockOpen,
   rotate(:x, 180), scale(0.25),
   cover(cWhite, [2000, 2000, 2000])
)
balllamptop = instance( quadrangle2,
   translate(-0.5, 0.5, -0.5), scale(0.25),
   cover(cWhite, cBlack)
)
balllampbase  = instance( blockOpen,
   rotate(:x, 180), translate(0, -0.5, 0), scale(0.125, 0.01, 0.125),
   brass
)
balllampstalk = instance( blockOpen,
   rotate(:x, 180), translate(0, -0.5, 0), scale(0.02, 0.1, 0.02),
   brass
)
balllamp = group(
   instance( ballluminaire, translate(0, -0.225, 0) ),
   instance( balllamptop,   translate(0, -0.225, 0) ),
   balllampbase,
   balllampstalk
)

tabletop = instance( block2, scale(1, 0.04, 0.5), woodDark )
tableleg = instance( block2, scale(0.04, 0.76, 0.04), woodDark )
table = group(
   instance( tabletop, translate(0, 0.76, 0) ),
   instance( tableleg, translate(0.01, 0, 0.01) ),
   instance( tableleg, translate(0.95, 0, 0.01) ),
   instance( tableleg, translate(0.01, 0, 0.45) ),
   instance( tableleg, translate(0.95, 0, 0.45) )
)

sofabase = instance( block2, scale(0.475, 0.25, 0.49) )
sofaseat = instance( block2, scale(0.475, 0.15, 0.6) )
sofaside = instance( block2, scale(0.55, 0.15, 0.6) )
sofaback = instance( block2, scale(0.475, 0.15, 0.9) )
sofa1 = instance( group(
   instance( sofabase, translate(0.15, 0.05, 0.01) ),
   instance( sofaback, rotate(:x, 80), translate(0.15, 0.05, 0.65) ),
   instance( sofaside, rotate(:z, -90), translate(0.15, 0.05, 0.05) ),
   instance( sofaside, rotate(:z, -90), translate(0.775, 0.05, 0.05) ),
   instance( sofaseat, translate(0.15, 0.03, 0) )
), cover([0.245, 0.293, 0.261], cBlack) )
sofa2 = instance( group(
   instance( sofabase, translate(0.15, 0.05, 0.01) ),
   instance( sofabase, translate(0.625, 0.05, 0.01) ),
   instance( sofaback, rotate(:x, 80), translate(0.15, 0.05, 0.65) ),
   instance( sofaback, rotate(:x, 80), translate(0.625, 0.05, 0.65) ),
   instance( sofaside, rotate(:z, -90), translate(0.15, 0.05, 0.05) ),
   instance( sofaside, rotate(:z, -90), translate(1.25, 0.05, 0.05) ),
   instance( sofaseat, translate(0.15, 0.03, 0) ),
   instance( sofaseat, translate(0.625, 0.03, 0) )
), cover([0.245, 0.293, 0.261], cBlack) )

plantpot = instance( blockOpen, scale(0.3), translate(0, 0.15, 0), brass )
cupboard = instance( block2, scale(1.2, 0.73, 0.45), woodDark )
rug      = instance( blockOpen2, scale(1.44, 0.005, 1), cover([0.52, 0.35, 0.25], cBlack) )

contents = group(
   instance( balllamp, translate(2, 2.6, 2) ),
   instance( table, rotate(:y, 90), translate(0, 0.002, 1.5) ),
   instance( plantpot, translate(-0.3, 0.002, 1.15) ),
   instance( plantpot, translate(-0.3, 0.002, 2.85) ),
   instance( cupboard, rotate(:y, 180), translate(2.1, 0.2, 0.49) ),
   instance( rug, translate(1.28, 0.002, 2.12) ),
   instance( sofa2, rotate(:y, 180), translate(2.615, 0.002, 1.963) ),
   instance( sofa1, rotate(:y, 90), translate(1.13, 0.002, 2.345) ),
   instance( sofa1, rotate(:y, -90), translate(2.87, 0.002, 3.12) )
)


### sun ###

def sun( luminance, seasonalAngle, diurnalAngle )

   resolution = 12

   vertexs = Array.new( resolution ) do |i|
      angle = i * (2 * Math::PI / resolution)
      radius = 1.392e+9 / 2
      [ Math::cos(angle) * radius, 0, Math::sin(angle) * radius ]
   end

   triangles = Array.new( resolution ) do |i|
      triangle( [0,0,0], vertexs[i], vertexs[(i + 1) % resolution], qDefault )
   end

   instance( group( *triangles ),
      translate(0, 149.6e+9, 0),
      rotate(:x, seasonalAngle),
      rotate(:z, diurnalAngle),
      cover(cBlack, luminance)
   )

end


output( group(
   #instance( sun( [1177902548, 993796380, 828301072], 30, 30 ), rotate(:y, -90) ),
   building,
   contents
) )
