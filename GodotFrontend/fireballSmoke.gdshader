shader_type spatial;
render_mode unshaded;

#define MAX_STEPS 990
#define MAX_DISTANCE 190.
#define MARCH_SMOKE_STEP .01
#define SURFACE_DISTANCE .005
//varying vec3 world_pos;
uniform sampler2D depth_texture : hint_depth_texture;
uniform sampler3D noise_3d_tex;

// Rotación en torno a X
vec3 rotateAroundX(vec3 p, float angle) {
    float c = cos(angle);
    float s = sin(angle);
    return vec3(
        p.x,
        p.y * c - p.z * s, // y'
        p.y * s + p.z * c  // z'
    );
}

// Rotación en torno a Y
vec3 rotateAroundY(vec3 p, float angle) {
    float c = cos(angle);
    float s = sin(angle);
    return vec3(
        p.x * c + p.z * s, // x'
        p.y,
        -p.x * s + p.z * c // z'
    );
}
vec3 rotateAroundZ(vec3 point, float angle) {
    float c = cos(angle);
    float s = sin(angle);
    return vec3(
        point.x * c - point.y * s, // x'
        point.x * s + point.y * c, // y'
        point.z                   // z' (no cambia)
    );
}


float sdBall(vec3 p){
	return length(p)-0.45;
}
float sdSmokeBall(vec3 p){
	p = rotateAroundX(p,TAU*(fract(TIME/13.)));
	float ball = sdBall(p)+0.15;
	//ball = opSmoothUnion(ball, sdGyroid(p),0.1);


	return ball;
}

float SDF(vec3 p){
 return sdSmokeBall(p);
}

struct Absorption_result{
	float absortion;
	float d_surface;
};
Absorption_result calculate_absorption(vec3 ro, vec3 rd, float depth){

	float ABSORPTIONRATE = 5.;
	float distance_crossed =0.0;
	float dist_surface = 0.0;
	// calculate the distance crossed in the sdf
	float dfo =0.;
	vec3 p = ro +rd *dfo;
	bool inside_smoke = false;
	vec3 cube_param = vec3(.4);
	for (int i =0;i<MAX_STEPS;i++){
		p = ro +rd *dfo;

		if (!inside_smoke){
			float d = SDF(p);
			dfo +=d;
			if (d<SURFACE_DISTANCE){
				dist_surface  = dfo;
				inside_smoke = true;
			}
		}
		else{
			float d = SDF(p);
			dfo+=MARCH_SMOKE_STEP;
			float noise = texture(noise_3d_tex,vec3(p.x+ fract(TIME/7.) ,p.y+fract(TIME/13.),p.z +fract(TIME/13.) )).r ;
			distance_crossed += MARCH_SMOKE_STEP*(noise-0.47);
			if (d>SURFACE_DISTANCE ) {

				break;
			}
		}
	}
	float absorption = 0.0;
	if (distance_crossed>0.0){
		// Exponential
		//absorption =  1.- exp(-distance_crossed*ABSORPTIONRATE);
		// linear
		absorption = 1.- distance_crossed*ABSORPTIONRATE;
	}
	return Absorption_result(absorption, dist_surface);
}
struct RayMarchResult {
	vec4 color;
	float d_surface;
};
RayMarchResult ray_march_absorption_color(vec3 ro, vec3 rd, float depth){
	Absorption_result absorption_res = calculate_absorption(ro,rd,depth);

	float density =absorption_res.absortion*1.;
	vec4 color = vec4(mix(vec3(.05,.01,.01),vec3(.01,.0,0.),density), density );
	return RayMarchResult(color,absorption_res.d_surface);
}

varying vec3 vertex_ls;
void vertex() {
	vertex_ls = VERTEX;
	//world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
	float depth_raw = texture(depth_texture, SCREEN_UV).x;
    vec4 upos = INV_PROJECTION_MATRIX* vec4(SCREEN_UV * 2.0 - 1.0, depth_raw,1.0);
    vec3 pixel_position = upos.xyz/upos.w;
    float linear_depth = length(pixel_position);
	vec3 ray_origin = (inverse(MODEL_MATRIX) * vec4(CAMERA_POSITION_WORLD,1)).xyz;
	vec3 ray_dir = normalize(vertex_ls - ray_origin);
	float angle = radians(0.0); // Rotación de 45 grados

	RayMarchResult res = ray_march_absorption_color(ray_origin,ray_dir,linear_depth);
	vec4 colorfinal = res.color;

	ALBEDO = colorfinal.rgb;
	ALPHA = colorfinal.a;


}
