extends Node3D


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	var space := get_viewport().world_3d.space
	PhysicsServer3D.space_set_active(space, false)


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
