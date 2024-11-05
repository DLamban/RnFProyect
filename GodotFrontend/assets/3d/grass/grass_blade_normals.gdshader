// NOTE: Shader automatically converted from Godot Engine 4.3.stable.mono's StandardMaterial3D.

shader_type spatial;
render_mode cull_disabled;
uniform vec3 color1:source_color;
uniform vec3 color2:source_color;
void vertex() {
	
}

void fragment() {
	ALBEDO = mix(color1,color2,UV.y);
	if (!FRONT_FACING){
		NORMAL = -NORMAL;
	}
}
