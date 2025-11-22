# VISUAL BUG: Orbit Lines Not Aligned with Ecliptic

**Status:** Open  
**Priority:** Medium  
**Reported:** 2025-11-22  
**Labels:** bug, visualization, orbital-mechanics

## Description
The orbital path lines rendered in the 3D visualization appear to be perpendicular to the ecliptic plane instead of lying on the XY plane (ecliptic).

## Expected Behavior
- Orbital paths should be rendered on or near the ecliptic plane (XY plane in our coordinate system)
- Inclination should only tilt orbits slightly from this plane
- Camera looking down Z-axis should see orbits as nearly flat ellipses

## Observed Behavior
- Orbit lines appear nearly perpendicular to ecliptic from default camera view
- Suggests coordinate transformation issue or incorrect plane calculation

## Potential Root Causes

### 1. MeshGenerator.GenerateOrbitPath()
```csharp
// Current implementation generates vertices in 2D then converts to 3D
// May not be applying inclination/ascending node rotations correctly
for (int i = 0; i <= segments; i++) { ... }
```

### 2. OrbitalElements.CalculatePosition()
- Coordinate transformation from orbital frame to ecliptic frame
- Rotation matrix order: R_z(Ω) * R_x(i) * R_z(ω)
- May have wrong axis or order

### 3. Camera Default Orientation
- Camera.cs default: distance=50 AU, azimuth=45°, elevation=30°
- May not be looking at ecliptic plane from expected angle

## Investigation Steps
- [ ] Add debug visualization for ecliptic plane (XY grid at Z=0)
- [ ] Print orbital elements for Earth: i≈0°, Ω≈0°, should be nearly flat
- [ ] Verify rotation matrix multiplication order in CalculatePosition()
- [ ] Check GenerateOrbitPath coordinate system assumptions
- [ ] Compare calculated Earth position with JPL Horizons ephemeris
- [ ] Test with zero inclination orbit - should be perfect XY circle

## Proposed Solutions

### Option A: Fix Coordinate Transformation
Review `OrbitalElements.CalculatePosition()` rotation matrices:
```csharp
// Correct order for ecliptic coordinates:
// 1. Rotate by argument of periapsis (ω) in orbital plane
// 2. Rotate by inclination (i) to tilt plane
// 3. Rotate by longitude of ascending node (Ω) to orient line of nodes
```

### Option B: Fix Orbit Path Generation
Update `MeshGenerator.GenerateOrbitPath()` to apply same transformations as CalculatePosition().

### Option C: Both
Ensure both methods use identical coordinate system definitions.

## Future Testing Strategy

### Automated Visual Testing
1. **Screenshot Capture:**
   ```csharp
   // Use Silk.NET framebuffer read
   GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, buffer);
   SaveToPNG("test-output/solar-system-view.png");
   ```

2. **Reference Images:**
   - Create golden images of correct orbital configurations
   - Store in `tests/Integration.Tests/ReferenceImages/`

3. **Image Comparison:**
   - Use perceptual diff tools (ImageSharp, SixLabors.ImageSharp.Drawing)
   - Threshold-based comparison with tolerance for anti-aliasing

4. **CI Integration:**
   ```yaml
   # .github/workflows/visual-regression.yml
   - name: Visual Regression Tests
     run: dotnet test --filter "Category=VisualRegression"
   - name: Upload Diff Images
     if: failure()
     uses: actions/upload-artifact@v3
   ```

### Unit Tests for Orbital Mechanics
```csharp
[Fact]
public void Earth_Orbit_Should_Be_Nearly_Flat()
{
    // Earth: i ≈ 0°, should have minimal Z component
    var position = earthOrbit.CalculatePosition(J2000);
    Assert.True(Math.Abs(position.Z) < 0.01 * position.Length);
}

[Fact]
public void GenerateOrbitPath_ZeroInclination_Should_Be_XY_Plane()
{
    var vertices = MeshGenerator.GenerateOrbitPath(a: 1.0, e: 0.0, i: 0.0, ...);
    foreach (var v in vertices) {
        Assert.Equal(0.0, v.Z, precision: 6);
    }
}
```

## Files to Review
- `src/Web/InterstellarTracker.Web/Rendering/MeshGenerator.cs` (line 150-183)
- `src/Domain/InterstellarTracker.Domain/ValueObjects/OrbitalElements.cs` (CalculatePosition method)
- `src/Web/InterstellarTracker.Web/Window.cs` (LoadCelestialBodies, line 90-130)
- `src/Web/InterstellarTracker.Web/Rendering/Camera.cs` (default orientation)

## References
- [Orbital Elements - NASA](https://ssd.jpl.nasa.gov/planets/approx_pos.html)
- [Coordinate Systems - JPL Horizons](https://ssd.jpl.nasa.gov/horizons/manual.html#frames)
- Vallado, "Fundamentals of Astrodynamics and Applications", Chapter 3

## Notes
User observation: "las líneas que describen las órbitas no están bien alineadas con la eclíptica, de hecho me ha parecido que eran casi perpendiculares"

This is a critical visual bug affecting usability - users cannot properly understand orbital relationships if planes are incorrect.
