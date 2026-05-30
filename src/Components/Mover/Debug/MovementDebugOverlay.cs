// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Components.Mover.Core;
using FastMath;
using Godot;

namespace Components.Mover.Debug
{
    /// <summary>
    /// Runtime debug overlay. Draws:
    ///   • Cyan arrow  — current velocity (3D, world-space)
    ///   • Yellow arrow — wish direction scaled to MaxSpeed (3D, world-space)
    ///   • Corner HUD  — numeric readout of speed, velocity components, floor state
    ///
    /// Usage: MoverComponent creates this automatically when ShowDebug = true.
    /// You can also add it as a child node manually in the editor.
    /// </summary>
    public partial class MovementDebugOverlay : Node3D
    {
        // ── 3D arrows ────────────────────────────────────────────────
        private MeshInstance3D _meshInstance;
        private ImmediateMesh _mesh;
        private StandardMaterial3D _lineMat;

        // ── HUD label ────────────────────────────────────────────────
        private CanvasLayer _canvas;
        private Label _label;

        // ── Config ───────────────────────────────────────────────────
        private static readonly Color VelocityColor = new(0f, 1f, 1f);    // cyan
        private static readonly Color WishDirColor = new(1f, 0.9f, 0f);  // yellow
        private const float WishDirDisplayScale = 3f;   // wish dir arrow length in world units
        private const float ArrowHeadRatio = 0.22f;
        private const float ArrowHeadSpread = 0.4f;

        // ─────────────────────────────────────────────────────────────
        // Setup
        // ─────────────────────────────────────────────────────────────

        public override void _Ready()
        {
            SetupArrows();
            SetupHud();
        }

        private void SetupArrows()
        {
            _mesh = new ImmediateMesh();

            _lineMat = new StandardMaterial3D
            {
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                VertexColorUseAsAlbedo = true,
                NoDepthTest = true,  // always draw on top
                RenderPriority = 100,
                CullMode = BaseMaterial3D.CullModeEnum.Disabled
            };

            _meshInstance = new MeshInstance3D
            {
                Mesh = _mesh,
                MaterialOverride = _lineMat,
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off
            };

            AddChild(_meshInstance);
        }

        private void SetupHud()
        {
            _canvas = new CanvasLayer { Layer = 127 };
            _label = new Label { Position = new Vector2(12f, 12f) };

            _label.AddThemeFontSizeOverride("font_size", 14);
            _label.AddThemeColorOverride("font_color", Colors.White);
            _label.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.8f));
            _label.AddThemeConstantOverride("shadow_offset_x", 1);
            _label.AddThemeConstantOverride("shadow_offset_y", 1);

            _canvas.AddChild(_label);
            AddChild(_canvas);
        }

        // ─────────────────────────────────────────────────────────────
        // Per-frame update
        // ─────────────────────────────────────────────────────────────

        /// <param name="worldOrigin">Base point for arrows (e.g. character hip height).</param>
        /// <param name="velocity">   Current velocity vector.</param>
        /// <param name="wishDir">    Normalized wish direction.</param>
        /// <param name="isOnFloor">  Ground state for HUD display.</param>
        /// <param name="mode">       Active MovementMode for HUD display.</param>
        public void UpdateOverlay(
            Vector3 worldOrigin,
            Vector3 velocity,
            Vector3 wishDir,
            bool isOnFloor,
            MovementMode mode
        )
        {
            // Сам overlay ставим в позицию персонажа
            GlobalPosition = worldOrigin;

            // ── 3D arrows ──
            _mesh.ClearSurfaces();

            const float velocityArrowLength = 2.5f;

            if (velocity.LengthSquared() > 0.01f)
            {
                DrawArrow(
                    Vector3.Zero,
                    velocity.Normalized() * velocityArrowLength,
                    VelocityColor);
            }

            if (!wishDir.IsZeroApprox())
            {
                DrawArrow(
                    Vector3.Zero,
                    wishDir.Normalized() * WishDirDisplayScale,
                    WishDirColor);
            }

            // ── HUD ──
            float hSpeed = new Vector3(velocity.X, 0f, velocity.Z).Length();

            _label.Text =
                $"[Mover]  {mode}\n" +
                $"Vel    {velocity.X,6:F2}  {velocity.Y,6:F2}  {velocity.Z,6:F2}\n" +
                $"HSpeed {hSpeed,6:F2} m/s\n" +
                $"Wish   {wishDir.X,6:F2}  {wishDir.Y,6:F2}  {wishDir.Z,6:F2}\n" +
                $"Floor  {(isOnFloor ? "YES" : "no")}";
        }

        // ─────────────────────────────────────────────────────────────
        // Drawing helpers
        // ─────────────────────────────────────────────────────────────

        private void DrawArrow(Vector3 origin, Vector3 direction, Color color)
        {
            if (direction.LengthSq() < 0.001f)
                return;

            float len = direction.FastLength();
            Vector3 dir = direction / len;
            Vector3 tip = origin + direction;

            // Choose a perpendicular that is not collinear with dir
            Vector3 perp = dir.Abs().FastDot(Vector3.Up) < 0.95f
                ? dir.FastCross(Vector3.Up).FastNormalized()
                : dir.FastCross(Vector3.Forward).FastNormalized();

            float headLen = len * ArrowHeadRatio;
            float headSide = headLen * ArrowHeadSpread;

            _mesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

            // Shaft
            SurfaceVertex(origin, color);
            SurfaceVertex(tip, color);

            // Arrowhead left wing
            SurfaceVertex(tip, color);
            SurfaceVertex(tip - dir * headLen + perp * headSide, color);

            // Arrowhead right wing
            SurfaceVertex(tip, color);
            SurfaceVertex(tip - dir * headLen - perp * headSide, color);

            _mesh.SurfaceEnd();
        }

        private void SurfaceVertex(Vector3 pos, Color color)
        {
            _mesh.SurfaceSetColor(color);
            _mesh.SurfaceAddVertex(pos);
        }
    }
}
