// Godot 4.3, Forward+ or Mobile
shader_type spatial;
render_mode unshaded, fog_disabled;

uniform sampler2D screen_texture : source_color, hint_screen_texture;
uniform sampler2D depth_texture : hint_depth_texture;
uniform sampler2D normal_rough_texture : hint_normal_roughness_texture;
uniform vec3 focal_point = vec3(0.,0.,0.);
uniform vec2 center_focal_point = vec2(0.5,0.85);
void vertex() {
	POSITION = vec4(VERTEX.xy, 1.0, 1.0);
}



// BLUR VARS
uniform int blur_strength: hint_range(1,8) = 3;
uniform float blur_falloff: hint_range(1.0, 20.0) = 7.0;
uniform float blur_near: hint_range(0.0, 100.0) = 20.0;
uniform float blur_far: hint_range(0.0, 250.0) = 200.0;
// Configurable Box Blur by Nighteyes -> https://godotshaders.com/shader/configurable-box-blur/
vec3 blur_size(sampler2D tex,vec2 uv, vec2 pixelSize) {
	vec2 pixel = uv / pixelSize;
	int x_min = max(int(pixel.x) - blur_strength, 0);
	int y_min = max(int(pixel.y) - blur_strength, 0);
	int x_max = min(int(pixel.x) + blur_strength, int(1.0 / pixelSize.x));
	int y_max = min(int(pixel.y) + blur_strength, int(1.0 / pixelSize.y));

	int count = 0;
	vec3 color = vec3(0.0);
	for(int x = x_min; x <= x_max; x++) {
		for(int y = y_min; y <= y_max; y++) {
			color += texture(tex, vec2(float(x), float(y)) * pixelSize).rgb;
			count++;
		}
	}
	return color / float(count);
}
void fragment() {

	float depth_raw_center = texture(depth_texture, center_focal_point).x;

	vec4 screen = texture(screen_texture, SCREEN_UV);
	float depth_raw = texture(depth_texture, SCREEN_UV).x;
	vec3 ndc_center = vec3(SCREEN_UV * 2.0 - 1.0, depth_raw_center);
	vec4 position_view_center = INV_PROJECTION_MATRIX * vec4(ndc_center, 1.0);
	position_view_center.xyz /= position_view_center.w;
	float linear_depth_center = -position_view_center.z;

	vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth_raw);
	vec4 position_view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
	position_view.xyz /= position_view.w;
	float linear_depth = -position_view.z;

	float depth_difference = abs(linear_depth - linear_depth_center);
	vec3 blur = blur_size(screen_texture, SCREEN_UV, 1.0/VIEWPORT_SIZE);
	vec4 world = INV_VIEW_MATRIX * INV_PROJECTION_MATRIX * vec4(ndc, 1.0);
	vec3 position_world = world.xyz / world.w;

	// GOT JIGGLY DEPTH because of jiggly grass, maybe do a mean or fixed point
	ALBEDO = mix(blur, screen.rgb, clamp(1. - depth_difference,0.,1.));

}
