shader_type spatial;
render_mode unshaded;

const int MAX_STEPS = 300;
const float MAX_DISTANCE = 1000.0;
const float MIN_DISTANCE = 0.001;
uniform vec2 center_focal_point = vec2(0.5,0.85);
uniform sampler2D DEPTH_TEXTURE : source_color, hint_depth_texture;
uniform sampler2D screen_texture : source_color, hint_screen_texture;
void vertex() {
    POSITION = vec4(VERTEX.xy, 1.0, 1.0);
}

float sdSphere (vec3 p, vec3 centrum, float radius) {
    return length(centrum-p) - radius;
}

// infinte repetitions
// adapted from https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
vec3 opRep(vec3 p, vec3 c) {
    vec3 q = mod(p+0.5*c,c)-0.5*c;
    return q;
}

float get_distance(vec3 p) {
    //p = opRep(p, vec3(3)); // uncomment for repeating spheres
    return sdSphere(p, vec3(0.,1.9,0.2), 1.0);
}

vec3 get_normal(vec3 p) {
    vec2 e = vec2(1e-3, 0);

    vec3 n = get_distance(p) - vec3(
        get_distance(p - e.xyy),
        get_distance(p - e.yxy),
        get_distance(p - e.yyx)
    );

    return normalize(n);
}

vec4 raymarch(vec3 ray_origin, vec3 ray_dir, float depth) {
    float t = 0.0;
    vec3 p = ray_origin;
    for (int i = 0; i < MAX_STEPS; i++)
    {
        float d = get_distance(p);
        t += d;
        if (t > MAX_DISTANCE || t >= depth)
            break;

        p += d * ray_dir;
        if (abs(d)  < MIN_DISTANCE)
            return vec4(get_normal(p), 1);
     }
    //return vec4(float(i) * 0.01); // uncomment for simple glow effect
    return vec4(0.0);
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
    float depth_raw = texture(DEPTH_TEXTURE, SCREEN_UV,0.0).r;
    vec4 upos = INV_PROJECTION_MATRIX* vec4(SCREEN_UV * 2.0 - 1.0, depth_raw,1.0);
    vec3 pixel_position = upos.xyz/upos.w;
    float linear_depth = length(pixel_position);

    vec3 weird_uv = vec3(SCREEN_UV * 2.0 - 1.0, 0.0);
    vec4 camera = INV_VIEW_MATRIX * INV_PROJECTION_MATRIX * vec4(weird_uv, 1);

    vec3 ray_origin = INV_VIEW_MATRIX[3].xyz;
    vec3 ray_dir = normalize(camera.xyz);

    vec4 col = raymarch(ray_origin, ray_dir, linear_depth);
    if (col.a <=0.) discard;
    //ALPHA = col.a;
    ALBEDO = col.rgb;

	float depth_raw_center = texture(DEPTH_TEXTURE, center_focal_point).x;

	vec4 screen = texture(screen_texture, SCREEN_UV);
	vec3 ndc_center = vec3(SCREEN_UV * 2.0 - 1.0, depth_raw_center);
	vec4 position_view_center = INV_PROJECTION_MATRIX * vec4(ndc_center, 1.0);
	position_view_center.xyz /= position_view_center.w;
	float linear_depth_center = -position_view_center.z;


	float depth_difference = abs(linear_depth - linear_depth_center);
	vec3 blur = blur_size(screen_texture, SCREEN_UV, 1.0/VIEWPORT_SIZE);

	// GOT JIGGLY DEPTH because of jiggly grass, maybe do a mean or fixed point
	//ALBEDO += mix(blur, screen.rgb, clamp(1. - depth_difference,0.,1.));



}