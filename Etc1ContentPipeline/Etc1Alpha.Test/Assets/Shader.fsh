uniform lowp sampler2D u_Texture; 
uniform lowp sampler2D u_Alpha; 
varying mediump vec2 v_TexCoordinate;


void main()
{
	vec4 colour = texture2D(u_Texture, v_TexCoordinate);
	colour.a = texture2D(u_Alpha, v_TexCoordinate).r;
    gl_FragColor = colour;
}
