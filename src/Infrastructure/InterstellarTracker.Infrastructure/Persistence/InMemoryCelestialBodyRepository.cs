using InterstellarTracker.Application.Common.Interfaces;
using InterstellarTracker.Domain.Entities;
using InterstellarTracker.Domain.ValueObjects;

namespace InterstellarTracker.Infrastructure.Persistence;

/// <summary>
/// In-memory implementation of the celestial body repository.
/// Contains data for the solar system and known interstellar objects.
/// Data sources: NASA JPL Horizons, Minor Planet Center, Wikipedia
/// </summary>
public class InMemoryCelestialBodyRepository : ICelestialBodyRepository
{
    private const double AU_TO_METERS = 149_597_870_700.0;
    private const double SUN_GM = 1.32712440018e20;

    private readonly Dictionary<string, CelestialBody> _bodies;
    private readonly Dictionary<string, InterstellarObject> _interstellarObjects;

    public InMemoryCelestialBodyRepository()
    {
        _bodies = new Dictionary<string, CelestialBody>();
        _interstellarObjects = new Dictionary<string, InterstellarObject>();
        InitializeData();
    }

    public Task<CelestialBody?> GetByIdAsync(string bodyId, CancellationToken cancellationToken = default)
    {
        _bodies.TryGetValue(bodyId, out var body);
        return Task.FromResult(body);
    }

    public Task<IEnumerable<CelestialBody>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<CelestialBody>>(_bodies.Values);
    }

    public Task<InterstellarObject?> GetInterstellarObjectByIdAsync(string objectId, CancellationToken cancellationToken = default)
    {
        _interstellarObjects.TryGetValue(objectId, out var obj);
        return Task.FromResult(obj);
    }

    private void InitializeData()
    {
        InitializeSun();
        InitializePlanets();
        InitializeDwarfPlanets();
        InitializeComets();
        InitializeInterstellarObjects();
    }

    private static double DegToRad(double degrees) => degrees * Math.PI / 180.0;

    private static OrbitalElements CreateOrbit(double aAU, double e, double iDeg, double lonDeg, double argDeg, double mDeg, double epoch)
    {
        return new OrbitalElements(
            semiMajorAxisMeters: aAU * AU_TO_METERS,
            eccentricity: e,
            inclinationRadians: DegToRad(iDeg),
            longitudeOfAscendingNodeRadians: DegToRad(lonDeg),
            argumentOfPeriapsisRadians: DegToRad(argDeg),
            meanAnomalyAtEpochRadians: DegToRad(mDeg),
            epochJulianDate: epoch,
            gravitationalParameter: SUN_GM
        );
    }

    private void InitializeSun()
    {
        _bodies["sun"] = new CelestialBody(
            id: "sun",
            name: "Sun",
            type: CelestialBodyType.Star,
            massKg: 1.98892e30,
            radiusMeters: 696_000_000.0,
            orbitalElements: null,
            visual: VisualProperties.Star(new RgbColor(1.0, 0.9, 0.7))
        );
    }

    private void InitializePlanets()
    {
        // Mercury
        _bodies["mercury"] = new CelestialBody(
            id: "mercury",
            name: "Mercury",
            type: CelestialBodyType.Planet,
            massKg: 3.3011e23,
            radiusMeters: 2_439_700.0,
            orbitalElements: CreateOrbit(0.387, 0.2056, 7.005, 48.331, 29.124, 174.795, 2451545.0),
            visual: VisualProperties.Planet(new RgbColor(0.5, 0.5, 0.5), 0.142)
        );

        // Venus
        _bodies["venus"] = new CelestialBody(
            id: "venus",
            name: "Venus",
            type: CelestialBodyType.Planet,
            massKg: 4.8675e24,
            radiusMeters: 6_051_800.0,
            orbitalElements: CreateOrbit(0.723, 0.00677, 3.395, 76.681, 54.852, 50.115, 2451545.0),
            visual: VisualProperties.Planet(new RgbColor(0.9, 0.8, 0.6), 0.76)
        );

        // Earth
        _bodies["earth"] = new CelestialBody(
            id: "earth",
            name: "Earth",
            type: CelestialBodyType.Planet,
            massKg: 5.97237e24,
            radiusMeters: 6_371_000.0,
            orbitalElements: CreateOrbit(1.0, 0.0167, 0.00005, -11.261, 102.947, 100.464, 2451545.0),
            visual: VisualProperties.Planet(new RgbColor(0.2, 0.5, 0.9), 0.306)
        );

        // Mars
        _bodies["mars"] = new CelestialBody(
            id: "mars",
            name: "Mars",
            type: CelestialBodyType.Planet,
            massKg: 6.4171e23,
            radiusMeters: 3_389_500.0,
            orbitalElements: CreateOrbit(1.524, 0.0934, 1.851, 49.579, 286.462, 19.412, 2451545.0),
            visual: VisualProperties.Planet(new RgbColor(0.9, 0.4, 0.2), 0.170)
        );

        // Jupiter
        _bodies["jupiter"] = new CelestialBody(
            id: "jupiter",
            name: "Jupiter",
            type: CelestialBodyType.Planet,
            massKg: 1.8982e27,
            radiusMeters: 69_911_000.0,
            orbitalElements: CreateOrbit(5.203, 0.0484, 1.305, 100.556, 275.067, 34.404, 2451545.0),
            visual: VisualProperties.Planet(new RgbColor(0.8, 0.7, 0.6), 0.52)
        );

        // Saturn
        _bodies["saturn"] = new CelestialBody(
            id: "saturn",
            name: "Saturn",
            type: CelestialBodyType.Planet,
            massKg: 5.6834e26,
            radiusMeters: 58_232_000.0,
            orbitalElements: CreateOrbit(9.537, 0.0542, 2.484, 113.715, 336.041, 49.954, 2451545.0),
            visual: VisualProperties.Planet(new RgbColor(0.9, 0.8, 0.6), 0.47)
        );

        // Uranus
        _bodies["uranus"] = new CelestialBody(
            id: "uranus",
            name: "Uranus",
            type: CelestialBodyType.Planet,
            massKg: 8.6810e25,
            radiusMeters: 25_362_000.0,
            orbitalElements: CreateOrbit(19.191, 0.0472, 0.770, 74.230, 96.734, 142.955, 2451545.0),
            visual: VisualProperties.Planet(new RgbColor(0.6, 0.8, 0.9), 0.51)
        );

        // Neptune
        _bodies["neptune"] = new CelestialBody(
            id: "neptune",
            name: "Neptune",
            type: CelestialBodyType.Planet,
            massKg: 1.02413e26,
            radiusMeters: 24_622_000.0,
            orbitalElements: CreateOrbit(30.069, 0.00859, 1.769, 131.722, 273.250, 267.767, 2451545.0),
            visual: VisualProperties.Planet(new RgbColor(0.3, 0.4, 0.9), 0.41)
        );
    }

    private void InitializeDwarfPlanets()
    {
        // Pluto
        _bodies["pluto"] = new CelestialBody(
            id: "pluto",
            name: "Pluto",
            type: CelestialBodyType.DwarfPlanet,
            massKg: 1.303e22,
            radiusMeters: 1_188_300.0,
            orbitalElements: CreateOrbit(39.482, 0.2488, 17.142, 110.303, 224.067, 238.928, 2451545.0),
            visual: VisualProperties.Planet(new RgbColor(0.8, 0.7, 0.6), 0.575)
        );

        // Ceres
        _bodies["ceres"] = new CelestialBody(
            id: "ceres",
            name: "Ceres",
            type: CelestialBodyType.DwarfPlanet,
            massKg: 9.3835e20,
            radiusMeters: 469_730.0,
            orbitalElements: CreateOrbit(2.767, 0.0758, 10.593, 80.329, 73.115, 95.989, 2451545.0),
            visual: VisualProperties.Planet(new RgbColor(0.6, 0.6, 0.6), 0.09)
        );

        // Eris
        _bodies["eris"] = new CelestialBody(
            id: "eris",
            name: "Eris",
            type: CelestialBodyType.DwarfPlanet,
            massKg: 1.66e22,
            radiusMeters: 1_163_000.0,
            orbitalElements: CreateOrbit(67.668, 0.44177, 44.040, 35.951, 151.639, 205.989, 2451545.0),
            visual: VisualProperties.Planet(new RgbColor(0.9, 0.9, 0.9), 0.96)
        );

        // Makemake
        _bodies["makemake"] = new CelestialBody(
            id: "makemake",
            name: "Makemake",
            type: CelestialBodyType.DwarfPlanet,
            massKg: 3.1e21,
            radiusMeters: 715_000.0,
            orbitalElements: CreateOrbit(45.791, 0.159, 28.96, 79.382, 294.834, 165.514, 2451545.0),
            visual: VisualProperties.Planet(new RgbColor(0.8, 0.7, 0.6), 0.81)
        );

        // Haumea
        _bodies["haumea"] = new CelestialBody(
            id: "haumea",
            name: "Haumea",
            type: CelestialBodyType.DwarfPlanet,
            massKg: 4.006e21,
            radiusMeters: 816_000.0,
            orbitalElements: CreateOrbit(43.335, 0.195, 28.19, 122.167, 239.041, 218.205, 2451545.0),
            visual: VisualProperties.Planet(new RgbColor(0.9, 0.9, 0.9), 0.804)
        );
    }

    private void InitializeComets()
    {
        // 1P/Halley - famous periodic comet
        _bodies["halley"] = new CelestialBody(
            id: "halley",
            name: "1P/Halley",
            type: CelestialBodyType.Comet,
            massKg: 2.2e14,
            radiusMeters: 5_500.0,
            orbitalElements: CreateOrbit(17.834, 0.96714, 162.263, 58.420, 111.333, 38.076, 2446470.5),
            visual: VisualProperties.Comet(new RgbColor(0.7, 0.8, 0.9))
        );

        // C/1995 O1 (Hale-Bopp)
        _bodies["hale-bopp"] = new CelestialBody(
            id: "hale-bopp",
            name: "C/1995 O1 (Hale-Bopp)",
            type: CelestialBodyType.Comet,
            massKg: 1.3e16,
            radiusMeters: 30_000.0,
            orbitalElements: CreateOrbit(250.46, 0.995068, 89.430, 282.471, 130.590, 0.0, 2450540.0),
            visual: VisualProperties.Comet(new RgbColor(0.7, 0.8, 0.9))
        );

        // 2P/Encke
        _bodies["encke"] = new CelestialBody(
            id: "encke",
            name: "2P/Encke",
            type: CelestialBodyType.Comet,
            massKg: 7.2e13,
            radiusMeters: 2_400.0,
            orbitalElements: CreateOrbit(2.218, 0.8502, 11.782, 334.568, 186.543, 152.655, 2451545.0),
            visual: VisualProperties.Comet(new RgbColor(0.7, 0.8, 0.9))
        );

        // 67P/Churyumov-Gerasimenko - visited by Rosetta
        _bodies["churyumov-gerasimenko"] = new CelestialBody(
            id: "churyumov-gerasimenko",
            name: "67P/Churyumov-Gerasimenko",
            type: CelestialBodyType.Comet,
            massKg: 9.982e12,
            radiusMeters: 2_000.0,
            orbitalElements: CreateOrbit(3.463, 0.641, 7.041, 50.147, 12.780, 94.416, 2451545.0),
            visual: VisualProperties.Comet(new RgbColor(0.7, 0.8, 0.9))
        );

        // C/1996 B2 (Hyakutake)
        _bodies["hyakutake"] = new CelestialBody(
            id: "hyakutake",
            name: "C/1996 B2 (Hyakutake)",
            type: CelestialBodyType.Comet,
            massKg: 8.2e13,
            radiusMeters: 2_500.0,
            orbitalElements: CreateOrbit(1700.0, 0.999897, 124.920, 188.046, 130.173, 0.0, 2450182.5),
            visual: VisualProperties.Comet(new RgbColor(0.7, 0.8, 0.9))
        );
    }

    private void InitializeInterstellarObjects()
    {
        // 1I/'Oumuamua - first confirmed interstellar object
        // Discovery: Oct 19, 2017 by Pan-STARRS (Robert Weryk)
        // Highly elongated, estimated 230m long
        _interstellarObjects["oumuamua"] = new InterstellarObject(
            id: "oumuamua",
            designation: "1I/2017 U1",
            name: "1I/'Oumuamua",
            orbitalElements: CreateOrbit(-1.279, 1.201, 122.741, 24.597, 241.811, 0.0, 2458080.5),
            discoveryDate: new DateTimeOffset(2017, 10, 19, 0, 0, 0, TimeSpan.Zero),
            discoverer: "Pan-STARRS (Robert Weryk)",
            visual: VisualProperties.Planet(new RgbColor(0.6, 0.5, 0.5), 0.10),
            estimatedDiameterMeters: 230.0
        );

        // 2I/Borisov - second confirmed interstellar object
        // Discovery: Aug 30, 2019 by Gennadiy Borisov
        // Active comet, ~1km nucleus
        _interstellarObjects["borisov"] = new InterstellarObject(
            id: "borisov",
            designation: "2I/2019 Q4",
            name: "2I/Borisov",
            orbitalElements: CreateOrbit(-0.851, 3.357, 44.053, 308.151, 209.124, 0.0, 2458826.5),
            discoveryDate: new DateTimeOffset(2019, 8, 30, 0, 0, 0, TimeSpan.Zero),
            discoverer: "Gennadiy Borisov",
            visual: VisualProperties.Comet(new RgbColor(0.7, 0.8, 0.9)),
            estimatedDiameterMeters: 1000.0
        );

        // 3I/ATLAS - third confirmed interstellar object
        // Discovery: Jul 1, 2025 by ATLAS survey
        // Record eccentricity: 6.14, perihelion Oct 29, 2025
        _interstellarObjects["atlas"] = new InterstellarObject(
            id: "atlas",
            designation: "3I/2024 S1",
            name: "3I/ATLAS",
            orbitalElements: CreateOrbit(-0.193, 6.14, 88.5, 125.4, 342.1, 0.0, 2460613.98194),
            discoveryDate: new DateTimeOffset(2025, 7, 1, 0, 0, 0, TimeSpan.Zero),
            discoverer: "ATLAS Survey",
            visual: VisualProperties.Comet(new RgbColor(0.7, 0.8, 0.9)),
            estimatedDiameterMeters: 1600.0
        );
    }
}
