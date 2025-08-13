#version 330
out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D texture0;
uniform sampler2D texture1;

uniform vec4 objectColor;
uniform float textureMix;

void main()
{
    vec4 samp0 = texture(texture0, texCoord),
    samp1 = texture(texture1, texCoord);

    vec4 texMix = mix(samp0, samp1, textureMix);
    outputColor = texMix * objectColor;
}