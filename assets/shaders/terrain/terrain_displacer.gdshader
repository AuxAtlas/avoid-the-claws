shader_type spatial;

uniform float height_scale = 0.5;
uniform sampler2D terrain_texture;
uniform vec4 light_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform vec4 dark_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);

void vertex() {
    // Sample a height value from a texture using UV coordinates
    float height = texture(terrain_texture, UV).r;
    
    // Displace the vertex along the Y-axis (or use NORMAL for outward push)
    VERTEX.y += height * (height_scale * 0.15);
	NORMAL.y += height * height_scale;
}

void fragment() {
	// Called for every pixel the material is visible on.
	vec4 mixed = mix(light_color, dark_color, texture(terrain_texture, UV).r);
	ALBEDO = mixed.rgb;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
