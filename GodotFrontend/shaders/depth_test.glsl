#[compute]
#version 450
//LINKKKKKKKKKKKKK FOR DEPTHTTTTT
// https://www.youtube.com/watch?v=Ov0LcLjmKX8
layout(local_size_x= 16, local_size_y=16,local_size_z=1) in;
layout(rgba16f, binding=0,set=0) uniform image2D input_image;
layout(binding = 0,set=1)uniform sampler2D depth_tex;
layout (push_constant, std430) uniform Params{
    vec2 screen_size;
    float inv_proj_2w;
    float inv_proj_3w;
} p;

void main(){
    ivec2 pixel = ivec2(gl_GlobalInvocationID.xy);
    vec2 size = p.screen_size;
    vec2 uv = pixel/size;
    
    float depth = texture(depth_tex,uv).r;
    float linear_depth = 1./ (depth*p.inv_proj_2w + p.inv_proj_3w);

    //linear_depth = clamp(linear_depth/50.,0.,1.);
    if (pixel.x >= size.x || pixel.y >= size.y) return;
    // Coordenadas del centro de la pantalla (normalizadas)
    vec2 centerCoords = vec2(0.5, 0.5); 

    // Leer el valor de profundidad del píxel central
    float central_depth = texture(depth_tex, centerCoords).r;
    float central_depth_linear = 1.0 / (central_depth * p.inv_proj_2w + p.inv_proj_3w);
    float distance_center_depth = clamp(abs(linear_depth - central_depth_linear),0.,1.);
    vec4 color = imageLoad(input_image,pixel);
    
    color.rgb = vec3(distance_center_depth,color.g,color.b);// * color.rgb;
    
    
    imageStore(input_image,pixel,color);
}
