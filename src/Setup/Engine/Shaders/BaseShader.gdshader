shader_type spatial;
render_mode depth_draw_opaque, specular_disabled, ambient_light_disabled;
uniform vec4 albedo : source_color = vec4(1.0);
uniform sampler2D albedo_texture : source_color, hint_default_white, filter_nearest_mipmap;
uniform bool vertex_color_enabled = true;
uniform bool vertex_snap_enabled = true;
uniform float snap_resolution : hint_range(16.0, 512.0) = 128.0;
uniform float alpha_scissor_threshold : hint_range(0.0, 1.0) = 0.5;

varying vec2 uv_out;
varying vec3 vertex_normal_view;
varying vec4 vertex_col;

void vertex() {
    vertex_normal_view = normalize((MODELVIEW_MATRIX * vec4(NORMAL, 0.0)).xyz);
    vertex_col = COLOR;
    uv_out = UV;
    vec4 clip_pos = PROJECTION_MATRIX * MODELVIEW_MATRIX * vec4(VERTEX, 1.0);
    if (vertex_snap_enabled) {
        clip_pos.xyz /= clip_pos.w;
        clip_pos.xy = floor(clip_pos.xy * snap_resolution) / snap_resolution;
        clip_pos.xyz *= clip_pos.w;
    }
    POSITION = clip_pos;
}

void fragment() {
    vec4 tex = texture(albedo_texture, uv_out);
    vec4 vc = vertex_color_enabled ? vertex_col : vec4(1.0);
    ALBEDO = tex.rgb * albedo.rgb * vc.rgb;
    ALPHA = tex.a * albedo.a * vc.a;
    ALPHA_SCISSOR_THRESHOLD = alpha_scissor_threshold;
    NORMAL = vertex_normal_view;
}

void light() {
    float NdotL = dot(vertex_normal_view, LIGHT);
    float half_lambert = NdotL * 0.5 + 0.5;
    half_lambert = half_lambert * half_lambert;
    DIFFUSE_LIGHT += LIGHT_COLOR * half_lambert * ATTENUATION;
}