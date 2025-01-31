
shader_type spatial;
// color property
uniform vec3 color_low :source_color = vec3(0.0, 0.3, 0.8);  
uniform vec3 color_mid :source_color = vec3(0.0, 0.8, 0.0);  
uniform vec3 color_high :source_color  = vec3(0.8, 0.6, 0.0);
uniform float step_size : hint_range(0.1, 2.0) = 0.5; // Tamaño de los escalones
varying float v_height;
varying vec3 v_color;
void vertex() {
	v_height = VERTEX.y;
	    // Cuantizamos la altura
    float quantized_height = floor(v_height / step_size) * step_size;

    // Selección del color según la altura
    if (quantized_height > -.8) {
        v_color = color_high;
    } else if (quantized_height > -1.8) {
        v_color = color_mid;
    } else {
        v_color = color_low;
    }
}
void fragment() {
	// same normal vector for every face
	NORMAL =  normalize(cross(dFdy(VERTEX), dFdx(VERTEX)));

  	ALBEDO = v_color;  // El color es constante para todo el triángulo
}