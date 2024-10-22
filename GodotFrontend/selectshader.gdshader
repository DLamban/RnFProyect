shader_type spatial;
render_mode unshaded,diffuse_burley,specular_disabled;
// playing with noise for particle/organic movement
uniform vec2 scroll1 = vec2(0.1, 0.1);
uniform vec2 scroll2 = vec2(-0.2, 0.2);
uniform sampler2D texture1;
uniform sampler2D texture2;
uniform sampler2D colorTexture;

// Grosor del borde
uniform float base_border_thickness  = .02; 

// calculate the border of the  rectangle so we draw an outline
// we use the scale to get a coherente border indepent of the transform scale, maybe is not needed
float calc_is_border(vec2 uv, float scale_factor){
	float border_thickness = base_border_thickness;// /(scale_factor*5.0); // Ajustar el grosor
	float left_edge = smoothstep(0, border_thickness, uv.x);             // Borde izquierdo
    float right_edge = smoothstep(1.0,1.0 - border_thickness, uv.x);     // Borde derecho
    float top_edge =  smoothstep(0,border_thickness, uv.y);              // Borde superior
    float bottom_edge = smoothstep(1, 1.0 - border_thickness,uv.y);     // Borde inferior
	float result = left_edge * right_edge * top_edge * bottom_edge;
	return pow (result,4);

}
void fragment() {
    // Obtener las coordenadas UV del fragmento
    vec2 uv = UV;
	
	 vec3 scale = vec3(
        length(vec3(MODEL_MATRIX[0].x, MODEL_MATRIX[0].y, MODEL_MATRIX[0].z)), // Escala en x
        length(vec3(MODEL_MATRIX[1].x, MODEL_MATRIX[1].y, MODEL_MATRIX[1].z)), // Escala en y
        length(vec3(MODEL_MATRIX[2].x, MODEL_MATRIX[2].y, MODEL_MATRIX[2].z))  // Escala en z
    );

	float scale_factor = max(scale.x, max(scale.y, scale.z)); // Usar la escala más pequeña
    float is_border = calc_is_border(uv,scale_factor);

	float  texNoise1 = texture(texture1,uv + (scroll1 * TIME)).r;
	float  texNoise2 = texture(texture2, uv + scroll2*TIME).r;
	float energy = texNoise1 * texNoise2;
	vec4 color = texture( colorTexture, vec2(energy));
    ALBEDO = color.rgb;//vec3(energy,energy,energy);
	//ALPHA = energy; // Mantiene la opacidad solo en los bordes
    ALPHA = is_border * energy; // Mantiene la opacidad solo en los bordes
}