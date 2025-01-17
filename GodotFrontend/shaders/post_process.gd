@tool
extends CompositorEffect
class_name TiltShiftEffect
@export var half_size : bool = true
var rd: RenderingDevice
var context = "tilt-shift"
var shader: RID
var pipeline: RID
var texture : StringName = "texture"
var pong_texture : StringName = "pong_texture"
var gaussian_blur_shader : RID
var gaussian_blur_pipeline : RID

func _init() -> void:
	effect_callback_type = EFFECT_CALLBACK_TYPE_POST_TRANSPARENT
	rd = RenderingServer.get_rendering_device()
	RenderingServer.call_on_render_thread(_initialize_compute)


# System notifications, we want to react on the notification that
# alerts us we are about to be destroyed.
func _notification(what: int) -> void:
	if what == NOTIFICATION_PREDELETE:
		if shader.is_valid():
			# Freeing our shader will also free any dependents such as the pipeline!
			rd.free_rid(shader)
		if gaussian_blur_shader.is_valid():
				rd.free_rid(gaussian_blur_shader)

#region Code in this region runs on the rendering thread.
# Compile our shader at initialization.
func _initialize_compute() -> void:
	rd = RenderingServer.get_rendering_device()
	print (rd)
	if not rd:
		return

	# Compile our shader.
	
	var shader_file := load("res://shaders/depth_test.glsl")
	var shader_spirv: RDShaderSPIRV = shader_file.get_spirv()

	shader = rd.shader_create_from_spirv(shader_spirv)
	if shader.is_valid():
		pipeline = rd.compute_pipeline_create(shader)

	#create blur shader
	shader_file = load("res://shaders/gaussian_blur.glsl")
	shader_spirv = shader_file.get_spirv()
	gaussian_blur_shader = rd.shader_create_from_spirv(shader_spirv)
	gaussian_blur_pipeline = rd.compute_pipeline_create(gaussian_blur_shader)

# Called by the rendering thread every frame.
func _render_callback(p_effect_callback_type: EffectCallbackType, p_render_data: RenderData) -> void:
	if rd and p_effect_callback_type == EFFECT_CALLBACK_TYPE_POST_TRANSPARENT and pipeline.is_valid():
		# Get our render scene buffers object, this gives us access to our render buffers.
		# Note that implementation differs per renderer hence the need for the cast.
		var render_scene_buffers := p_render_data.get_render_scene_buffers()
		var render_scene_data : RenderSceneDataRD = p_render_data.get_render_scene_data()
		if render_scene_buffers and render_scene_data:
			# Get our render size, this is the 3D render resolution!
			var size: Vector2i = render_scene_buffers.get_internal_size()
			if size.x == 0 and size.y == 0:
				return
			var effect_size : Vector2 = size
			if effect_size.x == 0.0 and effect_size.y == 0.0:
				return
			# We can use a compute shader here.
			@warning_ignore("integer_division")
			var x_groups := (size.x - 1) / 8 + 1
			@warning_ignore("integer_division")
			var y_groups := (size.y - 1) / 8 + 1
			var z_groups := 1

			# Create push constant.
			# Must be aligned to 16 bytes and be in the same order as defined in the shader.
			var inv_proj_mat:Projection = render_scene_data.get_cam_projection().inverse()
			var push_constant := PackedFloat32Array([
				size.x,
				size.y,
				inv_proj_mat[2].w,
				inv_proj_mat[3].w,
			])
			# CREATE TEXTURE USED FOR EFFECTS
			if render_scene_buffers.has_texture(context, texture):
				var tf : RDTextureFormat = render_scene_buffers.get_texture_format(context, texture)
				if tf.width != effect_size.x or tf.height != effect_size.y:
					# This will clear all textures for this viewport under this context
					render_scene_buffers.clear_context(context)

			if !render_scene_buffers.has_texture(context, texture):
				var usage_bits : int = RenderingDevice.TEXTURE_USAGE_SAMPLING_BIT | RenderingDevice.TEXTURE_USAGE_STORAGE_BIT
				render_scene_buffers.create_texture(context, texture, RenderingDevice.DATA_FORMAT_R16_UNORM, usage_bits, RenderingDevice.TEXTURE_SAMPLES_1, effect_size, 1, 1, true)
				render_scene_buffers.create_texture(context, pong_texture, RenderingDevice.DATA_FORMAT_R16G16B16A16_SFLOAT, usage_bits, RenderingDevice.TEXTURE_SAMPLES_1, effect_size, 1, 1, true)

			# Loop through views just in case we're doing stereo rendering. No extra cost if this is mono.
			var view_count: int = render_scene_buffers.get_view_count()
			for view in view_count:
				var pong_texture_image = render_scene_buffers.get_texture_slice(context, pong_texture, view, 0, 1, 1)
				# Get the RID for our color image, we will be reading from and writing to it.
				var input_image: RID = render_scene_buffers.get_color_layer(view)

				# Create a uniform set, this will be cached, the cache will be cleared if our viewports configuration is changed.
				var uniform := RDUniform.new()
				uniform.uniform_type = RenderingDevice.UNIFORM_TYPE_IMAGE
				uniform.binding = 0
				uniform.add_id(input_image)
				
				var sampler_state:RDSamplerState = RDSamplerState.new()
				sampler_state.min_filter = RenderingDevice.SAMPLER_FILTER_LINEAR
				sampler_state.mag_filter = RenderingDevice.SAMPLER_FILTER_LINEAR
				var linear_sampler:RID = rd.sampler_create(sampler_state)
				
				# DEPTH texture uniform
				var depth_image = render_scene_buffers.get_depth_layer(view)
				var depth_uniform := RDUniform.new()
				depth_uniform.uniform_type = RenderingDevice.UNIFORM_TYPE_SAMPLER_WITH_TEXTURE
				depth_uniform.binding = 0
				depth_uniform.add_id(linear_sampler)
				depth_uniform.add_id(depth_image)

				var uniform_set := UniformSetCacheRD.get_cache(shader, 0, [uniform])
				var uniform_set_depth := UniformSetCacheRD.get_cache(shader, 1, [depth_uniform])
				# Run our compute shader.
				var compute_list := rd.compute_list_begin()
				# rd.compute_list_bind_compute_pipeline(compute_list, pipeline)
				# rd.compute_list_bind_uniform_set(compute_list, uniform_set, 0)
				# rd.compute_list_bind_uniform_set(compute_list, uniform_set_depth, 1)
				# rd.compute_list_set_push_constant(compute_list, push_constant.to_byte_array(), push_constant.size() * 4)
				# rd.compute_list_dispatch(compute_list, x_groups, y_groups, z_groups)
				# rd.compute_list_end()


				#BLUR SHADER
				
				uniform = RDUniform.new()
				uniform.uniform_type = RenderingDevice.UNIFORM_TYPE_IMAGE
				uniform.binding = 0
				uniform.add_id(input_image)
				var texture_uniform_set := UniformSetCacheRD.get_cache(gaussian_blur_shader, 0, [uniform])
				
				uniform = RDUniform.new()
				uniform.uniform_type = RenderingDevice.UNIFORM_TYPE_IMAGE
				uniform.binding = 0
				uniform.add_id(pong_texture_image)
				var pong_texture_uniform_set := UniformSetCacheRD.get_cache(gaussian_blur_shader, 1, [uniform])

				var gaussian_blur_size:float = 16.0
				# ADD DEPTH
				var uniform_set_depth_gauss := UniformSetCacheRD.get_cache(gaussian_blur_shader, 2, [depth_uniform])

				# Horizontal first

				# Update push constant
				push_constant = PackedFloat32Array([
					effect_size.x,
					effect_size.y,
					gaussian_blur_size,
					0.0,
					inv_proj_mat[2].w,
					inv_proj_mat[3].w,
					0.0,
					0.0,
				])
				rd.draw_command_begin_label("Apply horizontal gaussian blur " + str(view), Color(1.0, 1.0, 1.0, 1.0))

				#var compute_list = rd.compute_list_begin()
				rd.compute_list_bind_compute_pipeline(compute_list, gaussian_blur_pipeline)
				rd.compute_list_bind_uniform_set(compute_list, texture_uniform_set, 0)
				rd.compute_list_bind_uniform_set(compute_list, pong_texture_uniform_set, 1)
				rd.compute_list_bind_uniform_set(compute_list, uniform_set_depth_gauss, 2)
				rd.compute_list_set_push_constant(compute_list, push_constant.to_byte_array(), push_constant.size() * 4)
				rd.compute_list_dispatch(compute_list, x_groups, y_groups, 1)
				rd.compute_list_end()

				rd.draw_command_end_label()
				# And vertical
				push_constant = PackedFloat32Array([
					effect_size.x,
					effect_size.y,				
					0.0,
					gaussian_blur_size,
					inv_proj_mat[2].w,
					inv_proj_mat[3].w,
					0.0,
					0.0,
				])

				rd.draw_command_begin_label("Apply vertical gaussian blur " + str(view), Color(1.0, 1.0, 1.0, 1.0))

				compute_list = rd.compute_list_begin()
				rd.compute_list_bind_compute_pipeline(compute_list, gaussian_blur_pipeline)
				rd.compute_list_bind_uniform_set(compute_list, pong_texture_uniform_set, 0)
				rd.compute_list_bind_uniform_set(compute_list, texture_uniform_set, 1)
				rd.compute_list_bind_uniform_set(compute_list, uniform_set_depth_gauss, 2)
				rd.compute_list_set_push_constant(compute_list, push_constant.to_byte_array(), push_constant.size() * 4)
				rd.compute_list_dispatch(compute_list, x_groups, y_groups, 1)
				rd.compute_list_end()

				rd.draw_command_end_label()

#endregion
