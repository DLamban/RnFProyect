// NOTE: Shader automatically converted from Godot Engine 4.3.stable.mono's StandardMaterial3D.

shader_type spatial;
render_mode cull_disabled;
uniform vec3 tip_color:source_color;
uniform vec3 base_color:source_color;
uniform vec3 fake_ambient_oclusion:source_color;
uniform vec3 burning_tip_color:source_color;
uniform vec3 burning_tip_color2:source_color;
uniform vec3 burned_color:source_color;

uniform sampler2D noise;
uniform sampler2D height_variance_tex;
uniform vec2 mesh_size;
uniform float displacement_force : hint_range(0.0, 1.0);
varying vec3 world_pos;
varying vec2 tex_world_pos;
varying vec3 data_texture_var;
uniform sampler2D data_texture;
varying float grass_flattened;
varying float grass_burned;
void vertex() {
	//MAYBE USE INSTANCE_CUSTOM or INSTANCE_ID

	world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	tex_world_pos = vec2(
		(world_pos.x + mesh_size.x/2.) /mesh_size.x,
		(world_pos.y + mesh_size.y/2.) /mesh_size.y
	);
	data_texture_var = texture(data_texture,vec2(tex_world_pos.x,tex_world_pos.y)).rgb;
	grass_flattened = data_texture_var.r;
	grass_burned = data_texture_var.g;
	float grass_not_flattened = 1.- grass_flattened;
	vec2 tex_position = vec2(world_pos.x ,world_pos.y);
	float displacement_factor = max(0,VERTEX.y - 0.01);
	// squared for smoother change
	displacement_factor *= displacement_factor;
   	// HEIGHT VARIANCE
	float height_variance = texture(height_variance_tex, tex_world_pos).r;
	VERTEX.y = VERTEX.y * ((height_variance * displacement_factor));


	// WIND NOISE
	world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	float move = texture(noise, tex_position + vec2(0.12,0.07)*TIME).r;


	world_pos.x += move * displacement_force * displacement_factor * grass_not_flattened;
	VERTEX = (inverse(MODEL_MATRIX) * vec4(world_pos, 1.0)).xyz;
	// Flatten da grass
	//VERTEX.x *= -10.;//
	VERTEX.y *= grass_not_flattened+0.25;
	VERTEX.x += (1. - grass_not_flattened) * displacement_factor+1.;

}
vec3 grass_color_mix(vec2 uv){
	float t = smoothstep(0.1,1.,uv.y);
	// improve flattened grass
	return
		mix(
			fake_ambient_oclusion,
			mix(
				mix(tip_color,base_color,uv.y),
				fake_ambient_oclusion,t
			),1.2-grass_flattened
		);
}
vec3 grass_burned_mix(vec2 uv){
	float move = texture(noise, vec2(0.12,0.07)*TIME).r;
	float t = smoothstep(0.01,0.15,uv.y);
	float t2 = smoothstep(0.001,0.12,uv.y+ fract(abs(sin(move)))*0.08) ;
	return
		mix ( burning_tip_color2,
			mix(burning_tip_color,
				mix(burned_color,fake_ambient_oclusion,uv.y),
				t
			),t2
		);
}
vec3 fresnel_glow(float amount, float intensity, vec3 color, vec3 normal, vec3 view)
{
	return pow((1.0 - dot(normalize(normal), normalize(view))), amount) * color * intensity;
}
void fragment() {
	if (!FRONT_FACING){
		NORMAL = -NORMAL;
	}
	// Definimos los dos colores posibles
    vec3 color_normal = grass_color_mix(UV);
    vec3 color_burned = grass_burned_mix(UV);
    
    // Creamos una máscara suave basada en el valor de grass_burned
    // El primer valor (0.4) es donde empieza a quemarse, el segundo (0.6) donde está totalmente quemado
    float burn_mask = smoothstep(0.4, 0.6, grass_burned);
    
    // Mezclamos linealmente los dos colores
    ALBEDO = mix(color_normal, color_burned, burn_mask);

    // Para la emisión, también la multiplicamos por la inversa de la máscara 
    // para que el brillo desaparezca mientras se quema
    vec3 fresnel_color = vec3(0.5, 0.7, 0.3);
    vec3 fresnel = fresnel_glow(7.0, 4.5, fresnel_color, NORMAL, VIEW);
    EMISSION = fresnel * (1.0 - burn_mask);



}
