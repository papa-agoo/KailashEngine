﻿

layout(location = 0) out vec4 color;

in vec3 g_viewPosition;

vec2 computeMoments(float depth)
{
	vec2 moments;

	moments.x = depth;
	moments.y = depth * depth;

	float dx = dFdx(depth);
	float dy = dFdy(depth);
	moments.y += 0.25*(dx*dx + dy*dy);

	return moments;
}

vec2 pack2(float d)
{
	vec2 bias = vec2(1.0 / 255.0, 0.0);
	vec2 color = vec2(d, fract(d * 255.0));
	return color - (color.yy * bias);
}

vec2 g_EVSMExponents = vec2(40.0, 30.0);
vec2 warpDepth(float depth)
{
    depth = 2.0f * depth - 1.0f;
    float pos =  exp( g_EVSMExponents.x * depth);
    float neg = -exp(-g_EVSMExponents.y * depth);
    return vec2(pos, neg);
}

void main()
{
	float depth = length(g_viewPosition);
/*
	vec2 depth_moments = computeMoments(depth);
	//depth_moments.x = exp(depth);
	vec4 depth_packed = vec4(pack2(depth_moments.x),pack2(depth_moments.y));

	color = depth_packed;
*/
	
	depth = calcLinearDepth(depth, 0.1, 100.0);

	vec2 warped_depth = warpDepth(depth);
	color = vec4(warped_depth, warped_depth * warped_depth);

}