shader_type canvas_item;
uniform vec2 scroll1 = vec2(0.0, 0.1);
uniform vec2 scroll2 = vec2(0.0, -0.2);
uniform sampler2D texture2;
uniform sampler2D colorTexture;
uniform float radius : hint_range(0.0, 1.0) = 0.5; 
uniform float smoothness : hint_range(0.0, 1.0) = 0.1;
void fragment(){
  	// add the effect
	float  texNoise1 = texture(TEXTURE,UV + (scroll1 * TIME)).r;
	float  texNoise2 = texture(texture2, UV + scroll2*TIME).r;
	float energy = texNoise1 * texNoise2;
	
	// set at center circle	    
    vec2 center = vec2(0.5, 0.5); // Centro del círculo
    float dist = distance(UV, center); // Distancia del fragmento al centro
	float gradient = smoothstep(radius, radius - smoothness, dist);
    // Crear el gradiente: valor entre 0 (centro) y 1 (borde)
	vec4 color = texture( colorTexture, vec2(energy));
	color.a = color.a * gradient;
	COLOR = color;
}