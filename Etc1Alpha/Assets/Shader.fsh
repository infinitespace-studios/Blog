uniform lowp sampler2D u_Texture; 
varying mediump vec2 v_TexCoordinate;


void main()
{
	float cutoff = 0.28;
    vec4 colour = texture2D(u_Texture, v_TexCoordinate);
    if ((colour.r <= cutoff) && 
        (colour.g <= cutoff) && 
        (colour.b <= cutoff)) {
        colour.a = colour.r;
    }

    gl_FragColor = colour;
}
