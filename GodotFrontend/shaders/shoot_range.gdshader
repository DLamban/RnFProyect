shader_type spatial;



uniform vec4 long_range_color:source_color ; 
uniform vec4 short_range_color:source_color ; 
uniform float range=1;
uniform float unit_width=0.25;//dm
varying vec3 world_position;
uniform float plane_size=20.;
void vertex()
{
	world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}
void fragment() {
    
	float inch = 0.254/plane_size;//m we should divide by the size of the planemesh
	float range_dm = range * inch;
	float short_range_dm = range_dm/2.;
	float unit_width_uv_space = unit_width/plane_size;
	
	vec2 center_left_circle = vec2(0.5 - (unit_width_uv_space/2.),1);
	float dist_left_circle = sqrt(dot(UV - center_left_circle, UV -center_left_circle));
	
	vec2 center_right_circle = vec2((0.5 + (unit_width_uv_space/2.)),1);
	float dist_right_circle = sqrt(dot(UV - center_right_circle, UV -center_right_circle));
    
	// Maybe morganize this!
	if (dist_left_circle < short_range_dm && UV.x <center_left_circle.x
	 		|| UV.x > center_left_circle.x &&  1.- UV.y < short_range_dm && UV.x < center_right_circle.x
	 		|| dist_right_circle < short_range_dm && UV.x >center_right_circle.x	
			){
		ALBEDO = short_range_color.rgb;
	}
	else if (dist_left_circle < range_dm && UV.x <center_left_circle.x
	 		|| UV.x > center_left_circle.x &&  1.- UV.y < range_dm && UV.x < center_right_circle.x
	 		|| dist_right_circle < range_dm && UV.x >center_right_circle.x	
			) 
	{
        ALBEDO = long_range_color.rgb;
    }	
	
	else {
        discard; // Fuera de la elipse, no dibujar
    }
	ALPHA = 0.55;
}