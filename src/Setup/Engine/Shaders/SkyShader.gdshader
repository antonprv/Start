shader_type spatial;
render_mode cull_disabled, unshaded, specular_disabled, ambient_light_disabled;

// ---------------------------------------------------------------------------
// Day / Night
// ---------------------------------------------------------------------------
uniform float time_of_day : hint_range(0.0, 1.0, 0.001) = 0.5;
uniform bool procedural_sky_enabled = true;

// ---------------------------------------------------------------------------
// Sky
// ---------------------------------------------------------------------------
uniform vec4 sky_horizon_color_day   : source_color = vec4(0.85, 0.72, 0.55, 1.0);
uniform vec4 sky_zenith_color_day    : source_color = vec4(0.15, 0.25, 0.55, 1.0);
uniform vec4 ground_horizon_color_day: source_color = vec4(0.55, 0.45, 0.35, 1.0);
uniform vec4 ground_color_day        : source_color = vec4(0.10, 0.08, 0.07, 1.0);

uniform vec4 sky_horizon_color_night  : source_color = vec4(0.05, 0.06, 0.12, 1.0);
uniform vec4 sky_zenith_color_night   : source_color = vec4(0.01, 0.01, 0.04, 1.0);
uniform vec4 ground_horizon_color_night: source_color = vec4(0.03, 0.03, 0.05, 1.0);
uniform vec4 ground_color_night       : source_color = vec4(0.01, 0.01, 0.02, 1.0);

uniform float horizon_blur  : hint_range(0.001, 1.0) = 0.08;
uniform float sky_curve     : hint_range(0.1, 8.0)  = 1.0;
uniform float ground_curve  : hint_range(0.1, 8.0)  = 1.0;
uniform vec2 day_night_range = vec2(-0.2, 0.15);

// ---------------------------------------------------------------------------
// Sun
// ---------------------------------------------------------------------------
uniform bool  sun_enabled      = true;
uniform vec4  sun_color        : source_color = vec4(1.0, 0.95, 0.85, 1.0);
uniform float sun_size         : hint_range(0.0001, 0.5) = 0.02;
uniform vec4  sun_halo_color   : source_color = vec4(1.0, 0.8, 0.5, 0.6);
uniform float sun_halo_size    : hint_range(0.0, 2.0)   = 0.3;

// ---------------------------------------------------------------------------
// Moon
// ---------------------------------------------------------------------------
uniform bool      moon_enabled   = true;
uniform sampler2D moon_texture   : source_color, hint_default_white, filter_linear;
uniform vec3      moon_direction = vec3(0.0, -1.0, 0.0);
uniform float     moon_size      : hint_range(0.001, 0.5) = 0.05;
uniform vec4      moon_tint      : source_color = vec4(1.0, 1.0, 1.0, 1.0);

// ---------------------------------------------------------------------------
// Clouds - two samples per layer, rotated + scaled to hide tiling. No full fBm.
// ---------------------------------------------------------------------------
uniform bool      clouds_enabled = true;
uniform sampler2D noise_texture  : hint_default_white, filter_linear, repeat_enable;

uniform vec4  cloud_color_day   : source_color = vec4(0.85, 0.88, 0.95, 1.0);
uniform vec4  cloud_color_night : source_color = vec4(0.18, 0.20, 0.32, 1.0);

uniform float cloud_density   : hint_range(0.0, 32.0) = 5.0;
uniform float cloud_threshold : hint_range(0.0,  1.0) = 0.3;
uniform float cloud_height    : hint_range(0.1, 100.0) = 1.0;
uniform vec2  cloud_tiling    = vec2(1.0);
uniform vec2  cloud_wind_speed = vec2(0.5, 0.0);
uniform float clouds_horizon_fade : hint_range(0.0, 1.0) = 0.15;

uniform bool  clouds_layer2_enabled = false;
uniform float cloud_height_2    : hint_range(0.1, 100.0) = 2.0;
uniform vec2  cloud_tiling_2    = vec2(0.7);
uniform vec2  cloud_wind_speed_2 = vec2(-0.3, 0.1);
uniform float cloud_density_2   : hint_range(0.0, 32.0) = 3.0;
uniform float cloud_threshold_2 : hint_range(0.0,  1.0) = 0.4;

// ---------------------------------------------------------------------------
// Stars - stable, no trig. Triplanar projection in object space.
// ---------------------------------------------------------------------------
uniform bool  stars_enabled        = true;
uniform float star_density         : hint_range(0.0, 3.0, 0.001) = 1.3;
uniform float star_size            : hint_range(0.1, 2.0, 0.01)  = 1.0;
uniform float star_brightness      : hint_range(0.0, 2.0)         = 1.0;
uniform float stars_horizon_fade   : hint_range(0.0, 1.0)        = 0.15;
uniform bool  star_twinkle_enabled = false;
uniform float star_twinkle_speed   : hint_range(0.0, 3.0)          = 0.5;

// ---------------------------------------------------------------------------
varying vec3 v_obj_dir;     // object-space, stays stable regardless of camera rotation
varying vec3 v_world_dir;   // world-space, follows camera
varying vec3 v_sun_dir;     // sun direction - computed once in vertex, not per pixel
varying float v_day;

// ---------------------------------------------------------------------------
float ray_plane_y(float plane_y, vec3 ray_dir) {
    return plane_y / max(ray_dir.y, 0.0001);
}

// irrational angle rotation to break tiling. basically free.
// sin/cos of constants - compiler can fold these.
const float CLOUD_ROT_SIN = 0.6018; // sin(37 deg)
const float CLOUD_ROT_COS = 0.7986; // cos(37 deg)

vec2 rotate_uv(vec2 uv) {
    return vec2(
        uv.x * CLOUD_ROT_COS - uv.y * CLOUD_ROT_SIN,
        uv.x * CLOUD_ROT_SIN + uv.y * CLOUD_ROT_COS
    );
}

// same texture, two samples with different scale/rot/offset.
// fBm-like variation, kills visible tiling almost completely.
// one extra texture fetch, but cheapest way to avoid regular patterns without full fBm.
vec4 cloud_layer(
    float plane_y, vec2 tiling, vec2 wind,
    float density, float threshold,
    vec3 eye, vec3 sun_rgb, vec3 cloud_rgb, vec3 fog_rgb, float horizon_fade
) {
    float t = ray_plane_y(plane_y, eye);
    if (t < 0.0) return vec4(0.0);
    vec2 base = eye.xz * t * 0.2 + wind * TIME * 0.1;

    float raw_a = texture(noise_texture, base * tiling).r;
    // second layer: rotated, different scale, phase offset - breaks periodicity
    vec2 uv_b = rotate_uv(base) * tiling * 2.37 + vec2(0.37, 0.91);
    float raw_b = texture(noise_texture, uv_b).r;
    float raw = mix(raw_a, raw_b, 0.5);

    // smoothstep instead of pow() - cheaper ALU, close enough for retro look
    float h = smoothstep(threshold, 1.0, raw);
    float n_dot_v = max(eye.y, 0.0001);
    float absorb = exp(-density * (1.0 - h * n_dot_v));
    vec3 col = cloud_rgb + sun_rgb * absorb;

    // blend sky color into clouds at horizon instead of just fading alpha.
    // looks like clouds dissolving into haze rather than a hard cut.
    float horizon_t = 1.0 - smoothstep(0.0, horizon_fade, eye.y);
    col = mix(col, fog_rgb, horizon_t);
    // alpha fades closer than the color blend
    float fade = smoothstep(0.0, horizon_fade * 0.5, eye.y);

    return vec4(col, h * fade);
}

vec3 sun_dir_from_tod(float tod) {
    float angle = mix(-PI / 3.0, PI * 0.5, tod);
    return normalize(vec3(0.0, sin(angle), cos(angle)));
}

vec3 sky_gradient(vec3 dir, float day_t) {
    float h = dir.y;
    float sky_t = pow(clamp(h, 0.0, 1.0), sky_curve);
    float ground_t = pow(clamp(-h, 0.0, 1.0), ground_curve);
    vec4 shc = mix(sky_horizon_color_night, sky_horizon_color_day, day_t);
    vec4 szc = mix(sky_zenith_color_night,  sky_zenith_color_day,  day_t);
    vec4 ghc = mix(ground_horizon_color_night, ground_horizon_color_day, day_t);
    vec4 gc  = mix(ground_color_night, ground_color_day, day_t);
    vec3 col = h >= 0.0 ? mix(shc.rgb, szc.rgb, sky_t) : mix(ghc.rgb, gc.rgb, ground_t);
    float hm = 1.0 - smoothstep(0.0, horizon_blur, abs(h));
    col = mix(col, mix(shc.rgb, ghc.rgb, 0.5), hm);
    return col;
}

float hash13(vec3 p) {
    return fract(sin(dot(p, vec3(127.1, 311.7, 74.7))) * 43758.5453);
}

// ---------------------------------------------------------------------------
void vertex() {
    v_obj_dir = normalize(VERTEX);

    vec3 world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    vec3 cam_pos = (INV_VIEW_MATRIX * vec4(0.0, 0.0, 0.0, 1.0)).xyz;
    v_world_dir = normalize(world_pos - cam_pos);

    vec3 sun_d = sun_dir_from_tod(time_of_day);
    v_sun_dir = sun_d;
    v_day = smoothstep(day_night_range.x, day_night_range.y, clamp(sun_d.y, -1.0, 1.0));

    POSITION = PROJECTION_MATRIX * MODELVIEW_MATRIX * vec4(VERTEX, 1.0);
}

// ---------------------------------------------------------------------------
void fragment() {
    vec3 obj_dir = normalize(v_obj_dir);
    vec3 world_dir = normalize(v_world_dir);
    float df = v_day;
    // renormalize after interpolation - cheap, avoids jitter on low-poly seams
    vec3 sun_d = normalize(v_sun_dir);

    // --- sky ---
    vec3 result = procedural_sky_enabled ? sky_gradient(world_dir, df) : vec3(0.0);

    // --- sun (disc + halo in one smoothstep block) ---
    // fade out at night using the day factor computed in vertex
    if (sun_enabled && procedural_sky_enabled) {
        float ca = dot(world_dir, sun_d);
        float disc = smoothstep(1.0 - sun_size, 1.0 - sun_size * 0.5, ca);
        float halo = smoothstep(1.0 - sun_size - sun_halo_size, 1.0 - sun_size, ca);
        result += sun_color.rgb * disc * df;
        result += sun_halo_color.rgb * halo * sun_halo_color.a * (1.0 - disc) * df;
    }

    // --- stars: triplanar object-space projection, no atan/acos ---
    if (stars_enabled) {
        float night_vis = (1.0 - df) * smoothstep(0.0, stars_horizon_fade, world_dir.y);
        if (night_vis > 0.001) {
            // pick dominant axis for triplanar. cheap, no trig.
            vec3 a = abs(obj_dir);
            vec2 grid;
            if (a.x > a.y && a.x > a.z) {
                grid = obj_dir.yz / a.x;
            } else if (a.y > a.z) {
                grid = obj_dir.xz / a.y;
            } else {
                grid = obj_dir.xy / a.z;
            }

            // floor() gives stable hash per cell, unaffected by vertex interpolation
            const float STAR_GRID_SCALE = 20.0; // fewer cells = sparser stars, but they read as actual points
            vec2 scaled = grid * STAR_GRID_SCALE;
            vec2 cell = floor(scaled);
            vec2 local = fract(scaled); // position inside the cell

            float h1 = hash13(vec3(cell, 0.0));
            float h2 = fract(h1 * 91.7);       // random brightness / tint
            float hx = fract(h1 * 13.13);
            float hy = fract(h1 * 47.31);

            // density drives threshold: higher density = lower threshold = more stars
            float threshold = 1.0 - clamp(star_density / 3.0, 0.0, 1.0) * 0.5;
            float occupied = step(threshold, h1);

            // jitter star center to hide the grid
            vec2 star_center = vec2(hx, hy) * 0.7 + 0.15; // keep inside cell with margin from edges
            float dist = length(local - star_center);

            // soft circle, not a square. random radius + global star_size multiplier
            float radius = mix(0.05, 0.16, h2) * star_size;
            float star = occupied * (1.0 - smoothstep(radius * 0.5, radius, dist));

            if (star > 0.001) {
                vec3 col = h2 > 0.5 ? vec3(1.0, 0.95, 0.9) : vec3(0.85, 0.9, 1.0);
                if (star_twinkle_enabled) {
                    star *= 0.75 + 0.25 * sin(TIME * star_twinkle_speed * 0.5 + h1 * 6.283);
                }
                result += col * star * star_brightness * night_vis;
            }
        }
    }

    // --- clouds ---
    if (clouds_enabled) {
        vec3 sun_rgb = mix(sun_color.rgb * 2.0, sun_color.rgb * 8.0, df);
        vec3 cloud_rgb = mix(cloud_color_night.rgb, cloud_color_day.rgb, df);
        // horizon fog color = sky horizon color, where clouds fade out
        vec3 fog_rgb = mix(sky_horizon_color_night.rgb, sky_horizon_color_day.rgb, df);

        vec4 l1 = cloud_layer(
            cloud_height, cloud_tiling, cloud_wind_speed,
            cloud_density, cloud_threshold,
            world_dir, sun_rgb, cloud_rgb, fog_rgb, clouds_horizon_fade
        );
        result = mix(result, l1.rgb, l1.a);

        if (clouds_layer2_enabled) {
            vec4 l2 = cloud_layer(
                cloud_height_2, cloud_tiling_2, cloud_wind_speed_2,
                cloud_density_2, cloud_threshold_2,
                world_dir, sun_rgb, cloud_rgb, fog_rgb, clouds_horizon_fade
            );
            result = mix(result, l2.rgb, l2.a);
        }
    }

    // --- moon ---
    if (moon_enabled) {
        vec3 md = normalize(moon_direction);
        float ctm = dot(world_dir, md);
        float mcr = cos(moon_size);
        if (ctm > mcr) {
            vec3 up  = abs(md.y) < 0.999 ? vec3(0.0, 1.0, 0.0) : vec3(1.0, 0.0, 0.0);
            vec3 tan = normalize(cross(up, md));
            vec3 bit = cross(md, tan);
            vec3 prj = world_dir - md * ctm;
            float u  = dot(prj, tan) / sin(moon_size);
            float v  = dot(prj, bit) / sin(moon_size);
            vec4 ms  = texture(moon_texture, vec2(u, v) * 0.5 + 0.5);
            result   = mix(result, ms.rgb * moon_tint.rgb, ms.a * step(length(vec2(u, v)), 1.0));
        }
    }

    ALBEDO = result;
    ALPHA  = 1.0;
}