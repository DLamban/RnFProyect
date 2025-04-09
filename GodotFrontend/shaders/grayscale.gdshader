shader_type spatial;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform float desaturation_amount : hint_range(0.0, 1.0) = 0.65; // Controla el nivel de desaturación (0 = sin cambio, 1 = completamente desaturado)
uniform float darkness_amount : hint_range(0.0, 1.0) = 0.2; // Controla cuán apagado estará el color (0 = sin cambio, 1 = completamente apagado)

void fragment() {
  vec4 currentColor = textureLod(screen_texture, SCREEN_UV, 0.0);


    float grayscale = dot(currentColor.rgb, vec3(0.299, 0.587, 0.114));

    // Aplicar la desaturación
    vec3 desaturated_color = mix(currentColor.rgb, vec3(grayscale), desaturation_amount);

    // Reducir el brillo para hacerlo más apagado
    vec3 final_color = mix(desaturated_color, vec3(0.0), darkness_amount);

    // El resultado final (mezcla con la textura original)

  	ALBEDO = final_color;
  	ALPHA = 1.;
}