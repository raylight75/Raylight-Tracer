

#-- classes --------------------------------------------------------------------

#-- objects

class Triangle

   def initialize( vertex0, vertex1, vertex2, quality )

      @vertexs = vertex0, vertex1, vertex2
      @quality = quality

   end

   def output( *args )

      transforms = (args.length > 1 ? args[0] : [])
      target     = args.last

      # transform
      triangle = self
      transforms.reverse_each do |t|
         triangle = t.transform( triangle )
      end

      # write
      triangle.vertexs.each do |v|
         target.printf( "(%.3e %.3e %.3e) ", *v )
      end

      target.printf( " (%.3g %.3g %.3g) (%.3g %.3g %.3g)",
         *(triangle.quality.flatten) )

      target << "\n"

   end

   attr_accessor :vertexs, :quality

end


class Instance

   def initialize( object, *transforms )

      @object     = object
      @transforms = transforms.reverse

   end

   def output( *args )

      transforms = (args.length > 1 ? args[0] : [])
      target     = args.last

      #length = transforms.length
      @object.output( transforms + @transforms, target )
      #transforms = transforms[0, length]

   end

end


class Group

   def initialize( *objects )

      @objects = objects

   end

   def output( *args )

      @objects.each { |i| i.output( *args ) }

      #args.last << "\n"

   end

end




#-- transforms

class Translate

   def initialize( xyz )

      @xyz = xyz

   end

   def transform( triangle )

      vertexs = triangle.vertexs.collect do |v|
         Array.new( 3 ) { |i| v[i] + @xyz[i] }
      end

      Triangle.new( vertexs[0], vertexs[1], vertexs[2], triangle.quality )

   end

end


class Rotate

   def initialize( axis, angle )

      @axis  = axis
      @angle = angle * (Math::PI / 180.0)

   end

   def transform( triangle )

      sin, cos = Math.sin( @angle ), Math.cos( @angle )

      vertexs = triangle.vertexs.collect do |v|
         case @axis
         when :x
            [ v[0], (cos * v[1]) + (sin * v[2]), (-sin * v[1]) + (cos * v[2]) ]
         when :y
            [ (cos * v[0]) + (-sin * v[2]), v[1], (sin * v[0]) + (cos * v[2]) ]
         when :z
            [ (cos * v[0]) + (sin * v[1]), (-sin * v[0]) + (cos * v[1]), v[2] ]
         end
      end

      Triangle.new( vertexs[0], vertexs[1], vertexs[2], triangle.quality )

   end

end


class Scale

   def initialize( xyz )

      @xyz = xyz.length == 3 ? xyz : xyz * 3

   end

   def transform( triangle )

      vertexs = triangle.vertexs.collect do |v|
         Array.new( 3 ) { |i| v[i] * @xyz[i] }
      end

      Triangle.new( vertexs[0], vertexs[1], vertexs[2], triangle.quality )

   end

end


class Cover

   def initialize( quality )

      @quality = quality

   end

   def transform( triangle )

      Triangle.new( triangle.vertexs[0], triangle.vertexs[1],
         triangle.vertexs[2], @quality )

   end

end




#-- shorter constructor synonyms -----------------------------------------------

def triangle( *args ) Triangle.new( *args ) end
def instance( *args ) Instance.new( *args ) end
def group(    *args ) Group.new(    *args ) end

def translate( *args ) Translate.new( args ) end
def scale(     *args ) Scale.new(     args ) end
def rotate(    *args ) Rotate.new(   *args ) end

def cover( *args ) Cover.new( args ) end




#-- generate command -----------------------------------------------------------

#-- defaults
$iterations       = 100
$width, $height   = 200, 200
$eyePosition      = [0.500, 0.500, -1.255]
$lookDirection    = [0, 0, 1]
$viewAngle        = 60
$skyEmission      = [906.4, 942.4, 1151.2]
$groundReflection = [0.1, 0.09, 0.07]


def output( scene )

   ext = ".ml"

   filename = File.basename( $0, ".rb" ) + ext + ".txt"
   File.open( filename, "w" ) do |file|

      file.printf( "#RayLight\n\n" )

      file.printf( "%i\n\n%i %i\n\n", $iterations, $width, $height )

      file.printf( "(%.3e %.3e %.3e) ", *$eyePosition )
      file.printf( "(%.3e %.3e %.3e) ", *$lookDirection )
      file.printf( "%.3g\n\n\n",         $viewAngle )

      file.printf( "(%.3e %.3e %.3e) ",      *$skyEmission )
      file.printf( "(%.3g %.3g %.3g)\n\n\n", *$groundReflection )

      scene.output( file )

   end

end








#-- basic solid constants --####################################################

def cBlack()     [0, 0, 0]          end
def cWhite()     [0.9, 0.9, 0.9]    end
def cWhite95()   [0.95, 0.95, 0.95] end
def cGreyLight() [0.75, 0.75, 0.75] end

def qDefault() [cGreyLight, cBlack] end


# square, z 0 plane, width 1, centered on origin
#
def quadrangle

   vertexs = [ [-0.5, -0.5, 0], [ 0.5, -0.5, 0], [-0.5,  0.5, 0],
      [ 0.5,  0.5, 0] ]

   group(
      triangle( vertexs[2], vertexs[1], vertexs[0], qDefault ),
      triangle( vertexs[1], vertexs[2], vertexs[3], qDefault ) )

end


# cube, width 1, centered on origin
#
def block

   q = qDefault
   v = Array.new( 8 ) do |i|
      [(i & 1) - 0.5, ((i >> 1) & 1) - 0.5, ((i >> 2) & 1) - 0.5]
   end

   group(
      triangle( v[0], v[2], v[1], q ), triangle( v[3], v[1], v[2], q ),
      triangle( v[5], v[7], v[4], q ), triangle( v[6], v[4], v[7], q ),
      triangle( v[4], v[6], v[0], q ), triangle( v[2], v[0], v[6], q ),
      triangle( v[1], v[3], v[5], q ), triangle( v[7], v[5], v[3], q ),
      triangle( v[0], v[1], v[4], q ), triangle( v[5], v[4], v[1], q ),
      triangle( v[2], v[6], v[3], q ), triangle( v[7], v[3], v[6], q ) )

end


# cube with one missing face, width 1, centered on origin
#
def blockOpen

   q = qDefault
   v = Array.new( 8 ) do |i|
      [(i & 1) - 0.5, ((i >> 1) & 1) - 0.5, ((i >> 2) & 1) - 0.5]
   end

   group(
      triangle( v[0], v[2], v[1], q ), triangle( v[3], v[1], v[2], q ),
      triangle( v[5], v[7], v[4], q ), triangle( v[6], v[4], v[7], q ),
      triangle( v[4], v[6], v[0], q ), triangle( v[2], v[0], v[6], q ),
      triangle( v[1], v[3], v[5], q ), triangle( v[7], v[5], v[3], q ),
      triangle( v[2], v[6], v[3], q ), triangle( v[7], v[3], v[6], q ) )

end


def quadrangle2
   instance( quadrangle, rotate(:x, -90), translate(0.5, 0, 0.5) )
end

def block2
   instance( block, translate(0.5, 0.5, 0.5) )
end

def blockOpen2
   instance( blockOpen, translate(0.5, 0.5, 0.5) )
end



# cylinder
# sphere
# ...







#-- example --------------------------------------------------------------------

##!/usr/bin/env ruby
#require 'MiniLightModelling.rb'
#
#
#$iterations = 10
#
#
#grey  = [0.7, 0.7, 0.7]
#red   = [0.7, 0.2, 0.2]
#green = [0.2, 0.7, 0.2]
#
#floorBackCeiling = group(
#   instance( quadrangle, rotate(:x, -90), translate(0.5, 0, 0.5) ),
#   instance( quadrangle, rotate(:x,  90), translate(0.5, 1, 0.5) ),
#   instance( quadrangle, translate(0.5, 0.5, 1) )
#)
#
#mainBox = group(
#   instance( floorBackCeiling, cover( grey, cBlack ) ),
#   instance( quadrangle, rotate(:y,  90), translate(0, 0.5, 0.5), cover( red,   cBlack ) ),
#   instance( quadrangle, rotate(:y, -90), translate(1, 0.5, 0.5), cover( green, cBlack ) )
#)
#
#lamp = instance( quadrangle,
#   scale(0.2), rotate(:x, 90),
#   cover( cWhite, [1000, 1000, 1000] )
#)
#
#
#output( group(
#   mainBox,
#   instance( lamp, translate(0.5, 0.95, 0.5) )
#) )
