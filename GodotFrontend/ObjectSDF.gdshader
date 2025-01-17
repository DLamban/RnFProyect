shader_type spatial;
render_mode unshaded;

#define MAX_STEPS 990
#define MAX_DISTANCE 190.
#define MARCH_SMOKE_STEP .01
#define SURFACE_DISTANCE .005
//varying vec3 world_pos;
uniform sampler2D depth_texture : hint_depth_texture;
uniform sampler2D noise_tex;
uniform sampler3D noise_3d_tex;
struct sdfResult {
	float dist;
	float absorption;	
};
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

float sdBox( vec3 p, vec3 b )
{
  vec3 q = abs(p) - b;
  return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0);
}
float sdPlane( vec3 p, vec3 n, float h )
{
  // n must be normalized
  return dot(p,n) + h;
}
float sdTorus( vec3 p, vec2 t )
{
  vec2 q = vec2(length(p.xz)-t.x,p.y);
  return length(q)-t.y;
}
float twistedTorus(vec3 p,vec2 t){
	const float k = 10.0; // or some other amount
    float c = cos(k*p.y);
    float s = sin(k*p.y);
    float x_rot = c * p.x - s * p.z;
    float z_rot = s * p.x + c * p.z;

    // Creamos un nuevo vector con las coordenadas rotadas
    vec3 q = vec3(x_rot, z_rot,p.y);
    return sdTorus(q,t);
}

float sdSphere(vec3 p, float radius){
	return length(p)-radius;
}
float sdFireball(vec3 p){
	return sdSphere(p,.35);
}
float displacementGlob(vec3 p){
	float d = sin(p.x)*sin(p.y)*sin(p.z);
	return d;
}

float opSmoothUnion( float d1, float d2, float k )
{
    float h = clamp( 0.5 + 0.5*(d2-d1)/k, 0.0, 1.0 );
    return mix( d2, d1, h ) - k*h*(1.0-h);
}
float opSmoothSubtraction( float d1, float d2, float k )
{
    float h = clamp( 0.5 - 0.5*(d2+d1)/k, 0.0, 1.0 );
    return mix( d2, -d1, h ) + k*h*(1.0-h);
}
float SDFVortexBlob(vec3 p){
	float torusRadius = 0.31;
	float torusWidth = 0.05;

	vec3 rotated_p = rotateAroundX(p,TAU*(fract(TIME/13.)));
	float torus1d = twistedTorus(rotated_p,vec2(torusRadius,torusWidth));
	rotated_p = rotateAroundZ(p,TAU*(fract(TIME/7.)));
	float torus2d = twistedTorus(rotated_p,vec2(torusRadius,torusWidth));
	//gimball locked at Y
	rotated_p = rotateAroundX(p,TAU/8.);
	rotated_p = rotateAroundZ(p,TAU/9.);
	rotated_p = rotateAroundX(rotated_p,TAU*(fract(TIME/17.)));
	float torus3d = twistedTorus(rotated_p,vec2(torusRadius,torusWidth));
	float d = sdSphere(p,.35);

	vec3 boxVector = vec3(.7,.7,.5);
	vec3 translated_p = p - vec3(0.,0.,-0.9);
	float dplane = sdBox(translated_p,boxVector);
	//return torus3d;
	return opSmoothUnion(
			dplane,
			opSmoothSubtraction(
				opSmoothUnion(
					opSmoothUnion(
						torus3d,torus1d,.1
					)
					,torus2d,0.1)
				,d,0.15)
			,.5);

}
float noise(vec3 x ) {
  vec3 p = floor(x);
  vec3 f = fract(x);
  f = f * f * (3. - 2. * f);
  vec2 uv = (p.xy + vec2(37.0, 239.0) * p.z) + f.xy;
  vec2 tex = textureLod(noise_tex,(uv + 0.5) / 256.0, 0.0).yx;
  return mix( tex.x, tex.y, f.z ) * 2.0 - 1.0;
}
float fbm(vec3 p) {
  vec3 q = p + TIME* 0.5 * vec3(1.0, -0.2, -1.0);
  float g = noise(q);
  float f = 0.0;
  float scale = 0.5;
  float factor = 2.02;
  for (int i = 0; i < 6; i++) {
      f += scale * noise(q);
      q *= factor;
      factor += 0.21;
      scale *= 0.5;
  }
  return f;

}
float sdGyroid(vec3 p) {
    float scale = 15.;
    vec3 p2 = p*scale;    
    return (abs(dot(sin(p2), cos(p2.zxy)))-.4)/scale;
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
float sdBallShell(vec3 p){
	return abs(sdBall(p))-.015;
}
float SDFOrb(vec3 p){
	//p = rotateAroundX(p,TAU*(fract(TIME/13.)));
	p = rotateAroundX(p,TAU*0.1);
	float ballShell = sdBallShell(p);	
	float gyroid =  sdGyroid(p);
	float d = max(ballShell,gyroid);
	return d;
}
float SDFCloud(vec3 p){
	return 0.;
}
float SDF(vec3 p){
 return sdSmokeBall(p);
}
float calculateAbsorption(){
	return 0.0;
}
sdfResult RayMarcherLightAbsortion(vec3 ro, vec3 rd, float depth){
	
	float dfo = 0.;
	float sdfDist = 0.;
	bool insideSmoke = false;
	bool crossedSmoke = false;
	float absorption = 0.;

	// REMEMBER TO REDUCE STEP SO WE REDUCE BANDING
	for (int i =0;i<MAX_STEPS;i++){
		if (insideSmoke== false){ // raymarch until we get in the smoke
			vec3 p =ro+rd *dfo;
			float ds = sdSmokeBall(p);
			float ds2 = SDF(p);
			//ds = min(ds,ds2);
			dfo +=ds2/10.;
			sdfDist = dfo;		

			if (ds<0. && crossedSmoke ==false){
				insideSmoke = true;				
			}
			if (ds2<SURFACE_DISTANCE ) {
				absorption =1.;
				break;
			}
			//if (dfo>MAX_DISTANCE){break;}
		}else{//fixed raymarch step
			vec3 p =ro+rd *dfo;
			float ds = MARCH_SMOKE_STEP;
		 	//ds = min(ds,MARCH_SMOKE_STEP);
			float dssmoke = sdSmokeBall(p);
			dfo +=ds;
			if (dssmoke > SURFACE_DISTANCE) {insideSmoke=false;crossedSmoke=true;}
			absorption += 0.009;
			//absorption =0.15;
			absorption = clamp(absorption,0,0.98);
			if ( dfo>MAX_DISTANCE ) break;	
		}		
	}
	
	return sdfResult(sdfDist, absorption);
}
float RayMarcher(vec3 ro, vec3 rd, float depth){
	float dfo = 0.;
	for (int i =0;i<MAX_STEPS;i++){
		vec3 p =ro+rd *dfo;
		float ds = SDF(p);
		dfo +=ds/10.0;
		if (ds<SURFACE_DISTANCE || dfo >MAX_DISTANCE ) break;
	}
	
	return dfo;
}

vec4 raymarch_density(vec3 ro, vec3 rd, float depth){
	float dfo = 0.;
	float density = 0.;
	vec4 res = vec4(0.0);
	for (int i =0;i<MAX_STEPS;i++){
		vec3 p =ro+rd *dfo;	
		float f = fbm(p);
		float density = (-sdBall(p))/50.;
		if (density > 0.0) {
			vec4 color = vec4(mix(vec3(1.0,1.0,1.0), vec3(0.0, 0.0, 0.0), density), density );
			color.rgb *= color.a;	
			res += color * (1.0 - res.a);
		}
		dfo += MARCH_SMOKE_STEP;
	
	}
	//if (dfo > MAX_DISTANCE) return res;
	return res;
	
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
			float noise = texture(noise_3d_tex,vec3(p.x+ fract(TIME/7.) ,p.y+fract(TIME/13.),p.z )).r;
			distance_crossed += MARCH_SMOKE_STEP*(noise-0.49);
			if (d>SURFACE_DISTANCE ) {
	//			exit_point = p;
	
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
	vec4 color = vec4(mix(vec3(1.,1.,.4),vec3(.1,.0,0.),density), density );
	return RayMarchResult(color,absorption_res.d_surface);
}

vec4 RayMarchOnlySmoke(vec3 ro, vec3 rd, float depth){
	float absorption = 0.;
	float dfo = 0.;
	float sdf_dist = 0.;
	bool inside_smoke = false;
	for (int i =0;i<MAX_STEPS;i++){
		if (!inside_smoke){
			vec3 p =ro+rd *dfo;
			float ds = sdSmokeBall(p);
			dfo +=ds/10.;
			sdf_dist = dfo;
			if (ds<SURFACE_DISTANCE ) {inside_smoke = true;}	
		}else{
			vec3 p =ro+rd *dfo;
			float ds = MARCH_SMOKE_STEP;
			float dist_smoke = sdSmokeBall(p);
			dfo+=ds;
			float f  = fbm(p);
			absorption +=  0.003;
			
			if (dist_smoke>0.1 || dfo > MAX_DISTANCE || dfo >depth) {
				inside_smoke = false;
				break;	
			}
		}
		if ( dfo >MAX_DISTANCE ) break;
		
	}	
	if (sdf_dist >depth || dfo > MAX_DISTANCE) return vec4(0.0);
	vec3 p =ro+rd *dfo;

	absorption = clamp(absorption,0.,0.99);
	vec3 color = mix(vec3(1.,1.,1.),vec3(0.,0.,0.),absorption);
	
	return vec4(color.rgb,absorption);
}
vec4 RayMarcherLightAbsortionColorRes(vec3 ro, vec3 rd, float depth){
	
	float dfo = 0.;
	float sdfDist = 0.;
	bool insideSmoke = false;
	bool crossedSmoke = false;
	float absorption = 0.;
	vec3 color = vec3(0.4,0.5,0.1);
	// REMEMBER TO REDUCE STEP SO WE REDUCE BANDING
	for (int i =0;i<MAX_STEPS;i++){
		if (insideSmoke== false){ // raymarch until we get in the smoke
			vec3 p =ro+rd *dfo;
			float ds = sdSmokeBall(p);
			float ds2 = SDF(p);
			//ds = min(ds,ds2);
			dfo +=ds2/10.;
	

			if (ds<0. && crossedSmoke ==false){
				insideSmoke = true;				
			}
			//if (dfo >depth) {break;}
			if (ds2<SURFACE_DISTANCE) {
				if (crossedSmoke == false){
					absorption =1.;
					break;	
				}else{
					absorption =1.;
					break;
				}
				sdfDist = dfo;	
			}
			//if (dfo>MAX_DISTANCE){break;}
		}else{//fixed raymarch step
			vec3 p =ro+rd *dfo;
			float ds = MARCH_SMOKE_STEP;
		 	//ds = min(ds,MARCH_SMOKE_STEP);
			float dssmoke = sdSmokeBall(p);
			dfo +=ds;
			if (dssmoke > SURFACE_DISTANCE) {insideSmoke=false;crossedSmoke=true;}
			absorption += 0.009;
			//absorption =0.15;
			absorption = clamp(absorption,0,0.98);
			if ( dfo> depth){insideSmoke=false;crossedSmoke= true;absorption = 0.; return vec4(0.0);}
		}		
	}
	if (dfo >depth || dfo > MAX_DISTANCE) return vec4(0.0);
	
	if (absorption<0.99){
		
		color = vec3(0.9,0.1,0.1);
	}
	return vec4 (color,absorption);
}


varying vec3 vertex_ls;
void vertex() {
	vertex_ls = VERTEX;
	//world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

vec3 calculateNormal(vec3 p) {
    const vec2 e = vec2(0.01, 0.0);
    return normalize(vec3(
        SDF(p + e.xyy) - SDF(p - e.xyy),
        SDF(p + e.yxy) - SDF(p - e.yxy),
        SDF(p + e.yyx) - SDF(p - e.yyx)
    ));
}

float calculateShadow(vec3 p, vec3 light_dir) {
    float shadow = 1.0;
    float distance_traveled = 0.0;

    for (int i = 0; i < MAX_STEPS; i++) {
        vec3 test_point = p + light_dir * distance_traveled;
        float d = SDF(test_point);
        if (d < SURFACE_DISTANCE) {
            return 0.0;
        }
        shadow = min(shadow, 10.0 * d / distance_traveled);
        distance_traveled += d;

        if (distance_traveled > MAX_DISTANCE) {
            break;
        }
    }
    return shadow;
}

void fragment() {
	float depth_raw = texture(depth_texture, SCREEN_UV).x;
    vec4 upos = INV_PROJECTION_MATRIX* vec4(SCREEN_UV * 2.0 - 1.0, depth_raw,1.0);
    vec3 pixel_position = upos.xyz/upos.w;
    float linear_depth = length(pixel_position);
	vec3 ray_origin = (inverse(MODEL_MATRIX) * vec4(CAMERA_POSITION_WORLD,1)).xyz;
	vec3 ray_dir = normalize(vertex_ls - ray_origin);
	float angle = radians(0.0); // Rotación de 45 grados

    // Aplica la rotación al origen del rayo
    //ray_origin = rotateAroundZ(ray_origin, angle);
    //ray_dir = rotateAroundZ(ray_dir, angle);
	//float d = RayMarcher(ray_origin,ray_dir,linear_depth);
	
	//if (d >MAX_DISTANCE|| d>linear_depth ) {
		//discard;
		////ALBEDO = vec3(0.,0.,0.);
		////ALPHA = 0.0;
	//}
	//else{
		//vec3 hit_point = ray_origin + ray_dir * d;
    	//vec3 normal = calculateNormal(hit_point);
    	////vec3 light_dir = normalize(vec3(1.0, 1.0, 1.0));
		//vec3 light_dir = -normalize(hit_point);
	    //float shadow = calculateShadow(hit_point + normal * SURFACE_DISTANCE, light_dir);
	    //vec3 light_color = vec3(1.0, .17, 0.1);
	    //vec3 diffuse = max(dot(normal, light_dir), 0.0) * light_color;
		//vec3 color = vec3(1.0, 1.0, .0) * diffuse;
		////color = pow(color, vec3(.4545));	// gamma correction
	    //ALBEDO = color;// * shadow;
		//ALPHA = 1.;
	//}
	 //CLOUD RAYMARCH
	
	RayMarchResult res = ray_march_absorption_color(ray_origin,ray_dir,linear_depth);
	vec4 colorfinal = res.color;
	vec3 hit_point = ray_origin + ray_dir * res.d_surface;
	vec3 normal = calculateNormal(hit_point);
   	//vec3 light_dir = normalize(vec3(1.0, 1.0, 1.0));
	//vec3 light_dir = normalize(hit_point);
    //float shadow = calculateShadow(hit_point + normal * SURFACE_DISTANCE, light_dir);
    //vec3 light_color = vec3(1.0, .17, 0.1);
    //vec3 diffuse = max(dot(normal, light_dir), 0.0) * light_color;
	//vec3 color = vec3(1.0, 1.0, .0) * diffuse;
	//color = pow(color, vec3(.4545));	// gamma correction


	
	ALBEDO = colorfinal.rgb;
	
	ALPHA = colorfinal.a;
	
	// oldie
	//vec4 colorfinal = RayMarcherLightAbsortionColorRes(ray_origin,ray_dir,linear_depth)	;
	//ALBEDO = colorfinal.rgb;
	//ALPHA = colorfinal.a;
	//
	//sdfResult result = RayMarcherLightAbsortion(ray_origin,ray_dir,linear_depth);
	//float d = result.dist;
	//float absorption = result.absorption;
	//if (d>MAX_DISTANCE || d>linear_depth ) {
		//ALBEDO = vec3(0.,0.,0.);
		//ALPHA = 0.0;
	//}
	//else{
		//
		//vec3 hit_point = ray_origin + ray_dir * d;
		//vec3 color = vec3(1.0, 1.0, 1.0);
		//if (absorption >0.99){
			//vec3 normal = calculateNormal(hit_point);
	    	////vec3 light_dir = normalize(vec3(1.0, 1.0, 1.0));
			//vec3 light_dir = normalize(hit_point);
		    //float shadow = calculateShadow(hit_point + normal * SURFACE_DISTANCE, light_dir);
		    //vec3 light_color = vec3(1.0, .17, 0.1);
		    //vec3 diffuse = max(dot(normal, light_dir), 0.0) * light_color;
			//color = vec3(1.0, 1.0, .0) * diffuse;
			//ALPHA = absorption;
		//}else{
			//ALPHA =0.;
		//}
		////color = pow(color, vec3(.4545));	// gamma correction
	    //ALBEDO = color;// * shadow;
		////ALBEDO *= color;
		//
	//}

 	
	
	
	
}
