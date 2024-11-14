shader_type spatial;
render_mode unshaded,diffuse_burley,specular_disabled;
// playing with noise for particle/organic movement
uniform vec2 scroll1 = vec2(0.1, 0.1);
uniform vec2 scroll2 = vec2(-0.2, 0.2);
uniform sampler2D texture1;
uniform sampler2D texture2;
uniform sampler2D colorTexture;
//uniform float radius : hint_range(0.0, 1.0) = 0.5;
uniform float smoothness : hint_range(0.0, 1.0) = 0.1;
// Grosor del borde
uniform float base_border_thickness: hint_range(0.0, 1.0)  = .02;
uniform float thickness: hint_range(0.0, 1.0) = 0.01;
uniform float speed = 0.5;

void fragment() {
    // Obtener las coordenadas UV del fragmento
    vec2 uv = UV;


	float  texNoise1 = texture(texture1,uv + (scroll1 * TIME)).r;
	float  texNoise2 = texture(texture2, uv + scroll2*TIME).r;
	float energy = texNoise1 * texNoise2;
	vec4 color = texture( colorTexture, vec2(energy));
	float radius = (1.0 -fract(TIME))/2.5;
	vec2 center = vec2(0.5, 0.5); // Centro del círculo
    float dist = distance(UV, center); // Distancia del fragmento al centro
	//float gradient = smoothstep(radius, radius - smoothness, dist);
	float edge = smoothstep(radius, radius - thickness, dist) - smoothstep(radius + thickness, radius, dist);
    ALBEDO = color.rgb;//vec3(energy,energy,energy);
	//ALPHA = energy; // Mantiene la opacidad solo en los bordes
    ALPHA = energy * edge; // Mantiene la opacidad solo en los bordes

}