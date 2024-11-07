// NOTE: Shader automatically converted from Godot Engine 4.3.stable.mono's StandardMaterial3D.

shader_type spatial;
render_mode cull_disabled;
uniform vec3 color1:source_color;
uniform vec3 color2:source_color;
uniform vec3 fake_ambient_oclusion:source_color;
uniform sampler2D noise;
uniform sampler2D height_variance_tex;
uniform vec2 mesh_size;
uniform float displacement_force : hint_range(0.0, 1.0);
varying vec3 world_pos;
varying vec2 tex_world_pos;
varying vec3 data_texture_var;
uniform sampler2D data_texture;
varying float grass_flattened;
void vertex() {
	world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	tex_world_pos = vec2( 
		(world_pos.x + mesh_size.x/2.) /mesh_size.x, 
		(world_pos.y + mesh_size.y/2.) /mesh_size.y
	);
	data_texture_var = texture(data_texture,vec2(tex_world_pos.x,tex_world_pos.y)).rgb;
	float grass_not_flattened = data_texture_var.x;
	grass_flattened = 1.- grass_not_flattened;
	vec2 tex_position = vec2(world_pos.x ,world_pos.y);
	float displacement_factor = max(0,VERTEX.y - 0.01);
	// squared for smoother change
	displacement_factor *= displacement_factor;
   	// HEIGHT VARIANCE 
	float height_variance = texture(height_variance_tex, tex_world_pos).r;
	VERTEX.y = VERTEX.y * ((height_variance * displacement_factor));

	
	// WIND NOISE
	world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	float move = texture(noise, tex_position + vec2(0.07,0.07)*TIME).r;
	
	
	world_pos.x += move * displacement_force * displacement_factor * grass_not_flattened;
	VERTEX = (inverse(MODEL_MATRIX) * vec4(world_pos, 1.0)).xyz;
	// Flatten da grass
	//VERTEX.x *= -10.;//
	VERTEX.y *= grass_not_flattened+0.1;
	VERTEX.x += (1. - grass_not_flattened) * displacement_factor+1.;
	
}
vec3 grass_color_mix(vec2 uv){
	float t = smoothstep(0.1,1.,uv.y);
	// improve flattened grass
	return mix(mix(color1,color2,uv.y),fake_ambient_oclusion,t)*1.-grass_flattened;
}
void fragment() {

	//ALBEDO = vec3(data_texture_var.x,0.,0.);
	ALBEDO = grass_color_mix(UV);
	if (!FRONT_FACING){
		NORMAL = -NORMAL;
	}
}
