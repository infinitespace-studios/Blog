attribute vec4 position;
attribute vec2 a_TexCoordinate;
varying vec2 v_TexCoordinate;
uniform float translate;

void main()
{
    gl_Position = position;
    gl_Position.y += sin(translate) / 2.0;

    // Pass through the texture coordinate.
    v_TexCoordinate = a_TexCoordinate;
}
