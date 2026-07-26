@tool
class_name FuncGodotMapPreservePaint
extends FuncGodotMap

const _MAX_TRANSFER_DISTANCE := 0.05

var _paint_snapshots: Array[VertexColorPaintSnapshot] = []

func build() -> void:
	_capture_paint_snapshots()
	super.build()
	_restore_paint_snapshots()

func _capture_paint_snapshots() -> void:
	_paint_snapshots.clear()
	for data_node in find_children("*", "VertexColorData", true, false):
		var mesh_instance := data_node.get_parent() as MeshInstance3D
		if not mesh_instance or not mesh_instance.mesh:
			continue
		var snap := VertexColorPaintSnapshot.capture_from_mesh_instance(mesh_instance, data_node)
		if not snap.colors.is_empty():
			_paint_snapshots.append(snap)

func _restore_paint_snapshots() -> void:
	if _paint_snapshots.is_empty():
		return

	var options := {
		"max_distance": _MAX_TRANSFER_DISTANCE,
		"use_normal_filter": true,
		"unmatched_fill": VertexColorTransfer.UNMATCHED_BLACK,
	}
	var mesh_instances := find_children("*", "MeshInstance3D", true, false)
	var scene_root = get_tree().edited_scene_root if Engine.is_editor_hint() else null
	var restored_nodes: Dictionary = {}

	for snap in _paint_snapshots:
		var snap_aabb := _snapshot_world_aabb(snap)
		if snap_aabb.size == Vector3.ZERO:
			continue
		var grown_aabb := snap_aabb.grow(_MAX_TRANSFER_DISTANCE)
		var snap_volume: float = grown_aabb.get_volume()
		if snap_volume <= 0.0:
			continue

		var target: MeshInstance3D = null
		var best_ratio := 0.0
		for mesh_instance in mesh_instances:
			var mesh_aabb: AABB = mesh_instance.global_transform * mesh_instance.get_aabb()
			if not mesh_aabb.intersects(grown_aabb):
				continue
			var overlap_volume: float = mesh_aabb.intersection(grown_aabb).get_volume()
			# Fraction of the SNAPSHOT volume covered by this mesh,
			# not the absolute overlap volume.
			# This prevents a large neighboring mesh that only clips the snapshot
			# from beating a smaller mesh that covers almost the entire snapshot.
			var ratio: float = overlap_volume / snap_volume
			if ratio > best_ratio:
				best_ratio = ratio
				target = mesh_instance

		if target == null or best_ratio < 0.5:
			VertexPainterLog.warn(
				"Failed to find a reliable match for one of the paint snapshots (best match: %.0f%%)."
				% (best_ratio * 100.0))
			continue

		var surface_colors := VertexColorTransfer.transfer_to_mesh(snap, target, options)
		if surface_colors.is_empty():
			continue

		var target_id := target.get_instance_id()
		var data_node: VertexColorData

		if restored_nodes.has(target_id):
			data_node = restored_nodes[target_id]
		else:
			data_node = VertexColorData.new()
			data_node.name = "VertexColorData"
			target.add_child(data_node, true)
			if scene_root:
				data_node.owner = scene_root
			data_node.ensure_paintable_color_mesh()
			data_node.ensure_paintable_runtime_mesh()
			restored_nodes[target_id] = data_node

		for surf_idx in surface_colors.keys():
			data_node.update_surface_colors(int(surf_idx), surface_colors[surf_idx])
		data_node.flush_gpu_updates()

	_paint_snapshots.clear()

func _snapshot_world_aabb(snap: VertexColorPaintSnapshot) -> AABB:
	var positions: PackedVector3Array = snap.world_positions
	if positions.is_empty():
		return AABB()
	var aabb := AABB(positions[0], Vector3.ZERO)
	for i in range(1, positions.size()):
		aabb = aabb.expand(positions[i])
	return aabb
