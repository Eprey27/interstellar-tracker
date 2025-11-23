# Orbital Mechanics

## Table of Contents

- [Hyperbolic Orbits](#hyperbolic-orbits)
- [Keplerian Elements](#keplerian-elements)
- [Position Calculation](#position-calculation)
- [Velocity Calculation](#velocity-calculation)
- [Coordinate Systems](#coordinate-systems)
- [2I/Borisov Parameters](#2iborisov-parameters)

## Hyperbolic Orbits

Órbita hiperbólica: trayectoria de un objeto con energía suficiente para escapar del sistema solar.

### Características

- **Eccentricity (e)**: e > 1 (hiperbólica)
- **Semi-major axis (a)**: a < 0 (por convención)
- **Energy**: Positiva (escape)
- **Period**: Infinito (no periódica)

### Ecuación de Órbita (polar)

```
r = a(1 - e²) / (1 + e·cos(ν))
```

Donde:

- `r`: distancia al foco (Sol)
- `a`: semi-major axis (negativo)
- `e`: eccentricity (> 1)
- `ν`: true anomaly (ángulo desde periapsis)

## Keplerian Elements

Los 6 elementos orbitales clásicos definen completamente una órbita:

1. **a** (semi-major axis): Escala de la órbita
2. **e** (eccentricity): Forma de la órbita
3. **i** (inclination): Inclinación respecto al plano de referencia
4. **Ω** (longitude of ascending node): Orientación del nodo ascendente
5. **ω** (argument of periapsis): Orientación del periapsis
6. **ν** o **M** (true anomaly o mean anomaly): Posición en la órbita

### 2I/Borisov Orbital Elements (Epoch: J2000)

```
a = -2.5 AU (aproximado, órbita hiperbólica)
e = 3.357 (altamente hiperbólica)
i = 44.053° (inclinación moderada)
Ω = 308.15° (nodo ascendente)
ω = 209.12° (argumento periapsis)
T_peri = 2019-12-08 (fecha de perihelio)
```

## Position Calculation

### Algoritmo (simplificado)

1. **Calcular Mean Anomaly (M)**:

   ```
   M = n(t - T_peri)
   ```

   Donde `n = √(μ/|a|³)` (mean motion)

2. **Resolver Eccentric Anomaly (E)** (Kepler's equation para hiperbólica):

   ```
   M = e·sinh(E) - E
   ```

   (Resolver numéricamente con Newton-Raphson)

3. **Calcular True Anomaly (ν)**:

   ```
   tan(ν/2) = √((e+1)/(e-1)) · tanh(E/2)
   ```

4. **Posición en plano orbital (2D)**:

   ```
   r = a(1 - e²) / (1 + e·cos(ν))
   x_orb = r·cos(ν)
   y_orb = r·sin(ν)
   ```

5. **Transformar a sistema J2000 (3D)** con rotaciones (Ω, i, ω)

### Implementación

Ver: `src/Application/InterstellarTracker.Application/CelestialBodies/Services/OrbitCalculationService.cs`

## Velocity Calculation

### Vis-viva Equation (hyperbolic)

```
v² = μ(2/r - 1/a)
```

Para órbitas hiperbólicas (a < 0): velocidad siempre > escape velocity

### Componentes

- **Radial velocity**: `v_r = √(μ/p) · e·sin(ν)`
- **Tangential velocity**: `v_t = √(μ/p) · (1 + e·cos(ν))`

Donde `p = a(1 - e²)` (semi-latus rectum)

## Coordinate Systems

### J2000 (ICRF)

- **Origin**: Solar System Barycenter
- **X-axis**: Vernal Equinox (J2000 epoch)
- **Z-axis**: Celestial North Pole
- **Epoch**: 1 January 2000, 12:00 TT

### Ecliptic Coordinates

- Plano de la órbita terrestre
- Útil para visualización sistema solar

### Orbital Plane

- Sistema local a la órbita
- Simplifica cálculos

### Transformaciones

Ver: `src/Application/InterstellarTracker.Application/CelestialBodies/Services/CoordinateTransformService.cs`

## 2I/Borisov Parameters

### Physical Properties

- **Discovery**: 30 August 2019 (Gennady Borisov)
- **Type**: Interstellar comet
- **Origin**: Extrasolar (velocidad escape > heliocentric)
- **Perihelion distance**: ~2 AU
- **Perihelion date**: 8 December 2019
- **Exit velocity**: ~30 km/s (respecto al Sol)

### Why Interesting?

- Segundo objeto interestelar confirmado (después de Oumuamua)
- Composición: similar a cometas del sistema solar (volátiles detectados)
- Oportunidad única para estudiar material extrasolar

## References

- **Curtis, H.D.**: "Orbital Mechanics for Engineering Students" (textbook)
- **NASA JPL**: [Horizons System](https://ssd.jpl.nasa.gov/horizons/)
- **2I/Borisov Data**: [JPL Small-Body Database](https://ssd.jpl.nasa.gov/tools/sbdb_lookup.html#/?sstr=2I)
- **IAU Minor Planet Center**: Official orbital elements

## Validation

Test suite: `tests/Domain.Tests/InterstellarTracker.Domain.Tests/Entities/HyperbolicOrbitTests.cs`

Casos críticos:

- Perihelion position (máxima precisión requerida)
- Asymptotic velocity (velocidad de escape)
- Coordinate transformations (J2000 ↔ Ecliptic)

**Target accuracy**: < 1% error en posición, < 0.1% error en velocidad
