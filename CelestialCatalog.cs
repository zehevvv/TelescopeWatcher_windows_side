using CosineKitty;

namespace TelescopeWatcher
{
    public enum CelestialObjectType
    {
        // Live-computed via AstronomyEngine (position changes over time)
        Planet,
        Moon,
        Sun,
        // Fixed J2000 coordinates
        Star,
        OpenCluster,
        GlobularCluster,
        Nebula,
        PlanetaryNebula,
        Galaxy,
        SupernovaRemnant,
        Other
    }

    /// <summary>
    /// A single entry in the catalog.
    /// Live objects (Planet/Moon/Sun) have <see cref="LiveBody"/> set;
    /// their <see cref="RA"/> and <see cref="Dec"/> are computed on demand via
    /// <see cref="GetCurrentCoordinates"/>.
    /// Fixed objects have <see cref="LiveBody"/> == null and constant J2000 coords.
    /// </summary>
    public class CelestialObject
    {
        public string Name { get; }
        public string? AlternateName { get; }
        public CelestialObjectType Type { get; }

        /// <summary>Non-null for solar-system bodies whose position is computed live.</summary>
        public Body? LiveBody { get; }

        /// <summary>Fixed J2000 RA in decimal hours (only valid when LiveBody is null).</summary>
        public double RA { get; private set; }

        /// <summary>Fixed J2000 Dec in decimal degrees (only valid when LiveBody is null).</summary>
        public double Dec { get; private set; }

        public bool IsLive => LiveBody.HasValue;

        public string TypeTag => Type switch
        {
            CelestialObjectType.Planet          => "Planet",
            CelestialObjectType.Moon            => "Moon",
            CelestialObjectType.Sun             => "Sun",
            CelestialObjectType.Star            => "Star",
            CelestialObjectType.OpenCluster     => "OC",
            CelestialObjectType.GlobularCluster => "GC",
            CelestialObjectType.Nebula          => "Neb",
            CelestialObjectType.PlanetaryNebula => "PN",
            CelestialObjectType.Galaxy          => "Gal",
            CelestialObjectType.SupernovaRemnant=> "SNR",
            _                                   => "Other"
        };

        public string DisplayName => AlternateName != null
            ? $"{Name}  ({AlternateName})"
            : Name;

        // Constructor for live solar-system bodies
        public CelestialObject(string name, string? alternateName,
                               CelestialObjectType type, Body liveBody)
        {
            Name = name;
            AlternateName = alternateName;
            Type = type;
            LiveBody = liveBody;
        }

        // Constructor for fixed deep-sky objects
        public CelestialObject(string name, string? alternateName,
                               CelestialObjectType type, double ra, double dec)
        {
            Name = name;
            AlternateName = alternateName;
            Type = type;
            RA = ra;
            Dec = dec;
        }

        /// <summary>
        /// Returns the current (RA, Dec) in decimal hours / degrees.
        /// For fixed objects this is instant; for live bodies it calls AstronomyEngine.
        /// <paramref name="observer"/> is only needed for aberration correction —
        /// pass null to use a geocentric position (good enough for telescope pointing).
        /// </summary>
        public (double ra, double dec) GetCurrentCoordinates(Observer? observer = null)
        {
            if (!IsLive) return (RA, Dec);

            var time = new AstroTime(DateTime.UtcNow);
            var obs = observer ?? new Observer(0, 0, 0);

            // EquatorEpoch.J2000 returns ICRS/J2000 coordinates — exactly what the
            // server's SiderealTracker.py expects: SkyCoord(ra=..., dec=..., frame='icrs').
            // Aberration.Corrected accounts for Earth's velocity through space (apparent place).
            var eq = Astronomy.Equator(LiveBody!.Value, time, obs,
                                       EquatorEpoch.J2000, Aberration.Corrected);
            return (eq.ra, eq.dec);
        }

        public bool Matches(string search)
        {
            if (string.IsNullOrWhiteSpace(search)) return true;
            var s = search.Trim();
            return Name.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                   (AlternateName?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false);
        }
    }

    public static class CelestialCatalog
    {
        public static readonly IReadOnlyList<CelestialObject> Objects =
            new List<CelestialObject>
        {
            // ── Solar System (live positions computed by AstronomyEngine) ──────────
            new("Sun",      null,   CelestialObjectType.Sun,    Body.Sun),
            new("Moon",     null,   CelestialObjectType.Moon,   Body.Moon),
            new("Mercury",  null,   CelestialObjectType.Planet, Body.Mercury),
            new("Venus",    null,   CelestialObjectType.Planet, Body.Venus),
            new("Mars",     null,   CelestialObjectType.Planet, Body.Mars),
            new("Jupiter",  null,   CelestialObjectType.Planet, Body.Jupiter),
            new("Saturn",   null,   CelestialObjectType.Planet, Body.Saturn),
            new("Uranus",   null,   CelestialObjectType.Planet, Body.Uranus),
            new("Neptune",  null,   CelestialObjectType.Planet, Body.Neptune),

            // ── Bright / Named Stars (fixed J2000) ───────────────────────────────
            new("Sirius",        "α CMa", CelestialObjectType.Star,              6.7525, -16.7161),
            new("Canopus",       "α Car", CelestialObjectType.Star,              6.3992, -52.6957),
            new("Arcturus",      "α Boo", CelestialObjectType.Star,             14.2612,  19.1822),
            new("Vega",          "α Lyr", CelestialObjectType.Star,             18.6156,  38.7836),
            new("Capella",       "α Aur", CelestialObjectType.Star,              5.2781,  45.9980),
            new("Rigel",         "β Ori", CelestialObjectType.Star,              5.2423,  -8.2017),
            new("Procyon",       "α CMi", CelestialObjectType.Star,              7.6553,   5.2250),
            new("Betelgeuse",    "α Ori", CelestialObjectType.Star,              5.9194,   7.4071),
            new("Aldebaran",     "α Tau", CelestialObjectType.Star,              4.5987,  16.5093),
            new("Antares",       "α Sco", CelestialObjectType.Star,             16.4901, -26.4320),
            new("Spica",         "α Vir", CelestialObjectType.Star,             13.4199, -11.1613),
            new("Pollux",        "β Gem", CelestialObjectType.Star,              7.7553,  28.0262),
            new("Fomalhaut",     "α PsA", CelestialObjectType.Star,             22.9608, -29.6223),
            new("Deneb",         "α Cyg", CelestialObjectType.Star,             20.6905,  45.2803),
            new("Regulus",       "α Leo", CelestialObjectType.Star,             10.1395,  11.9672),
            new("Polaris",       "α UMi", CelestialObjectType.Star,              2.5303,  89.2641),
            new("Castor",        "α Gem", CelestialObjectType.Star,              7.5766,  31.8883),
            new("Mimosa",        "β Cru", CelestialObjectType.Star,             12.7953, -59.6888),
            new("Acrux",         "α Cru", CelestialObjectType.Star,             12.4433, -63.0991),

            // ── Open Clusters ────────────────────────────────────────────────────
            new("M45",  "Pleiades",          CelestialObjectType.OpenCluster,    3.7833,  24.1167),
            new("M44",  "Beehive Cluster",   CelestialObjectType.OpenCluster,    8.6667,  19.9833),
            new("M35",  null,                CelestialObjectType.OpenCluster,    6.1500,  24.3333),
            new("M36",  null,                CelestialObjectType.OpenCluster,    5.6000,  34.1333),
            new("M37",  null,                CelestialObjectType.OpenCluster,    5.8667,  32.5500),
            new("M38",  null,                CelestialObjectType.OpenCluster,    5.4667,  35.8333),
            new("M34",  null,                CelestialObjectType.OpenCluster,    2.7000,  42.7333),
            new("M6",   "Butterfly Cluster", CelestialObjectType.OpenCluster,   17.6667, -32.2167),
            new("M7",   "Ptolemy Cluster",   CelestialObjectType.OpenCluster,   17.8983, -34.8167),
            new("M11",  "Wild Duck Cluster", CelestialObjectType.OpenCluster,   18.8500,  -6.2667),
            new("M52",  null,                CelestialObjectType.OpenCluster,   23.4000,  61.5833),
            new("M103", null,                CelestialObjectType.OpenCluster,    1.5583,  60.6500),

            // ── Globular Clusters ────────────────────────────────────────────────
            new("M13",           "Hercules GC", CelestialObjectType.GlobularCluster, 16.6950,  36.4600),
            new("M5",            null,          CelestialObjectType.GlobularCluster, 15.3100,   2.0817),
            new("M3",            null,          CelestialObjectType.GlobularCluster, 13.7033,  28.3767),
            new("M22",           null,          CelestialObjectType.GlobularCluster, 18.6067, -23.9017),
            new("M15",           null,          CelestialObjectType.GlobularCluster, 21.4997,  12.1670),
            new("M92",           null,          CelestialObjectType.GlobularCluster, 17.2847,  43.1350),
            new("M2",            null,          CelestialObjectType.GlobularCluster, 21.5578,  -0.8233),
            new("M4",            null,          CelestialObjectType.GlobularCluster, 16.3933, -26.5267),
            new("M10",           null,          CelestialObjectType.GlobularCluster, 16.9517,  -4.0983),
            new("M12",           null,          CelestialObjectType.GlobularCluster, 16.7867,  -1.9483),
            new("Omega Centauri","NGC 5139",    CelestialObjectType.GlobularCluster, 13.4467, -47.4767),
            new("47 Tucanae",    "NGC 104",     CelestialObjectType.GlobularCluster,  0.4017, -72.0817),

            // ── Nebulae ──────────────────────────────────────────────────────────
            new("M42",           "Orion Nebula",      CelestialObjectType.Nebula,  5.5883,  -5.3900),
            new("M43",           "De Mairan's Neb.",  CelestialObjectType.Nebula,  5.5950,  -5.2700),
            new("M8",            "Lagoon Nebula",      CelestialObjectType.Nebula, 18.0617, -24.3833),
            new("M17",           "Omega Nebula",       CelestialObjectType.Nebula, 18.3467, -16.1767),
            new("M20",           "Trifid Nebula",      CelestialObjectType.Nebula, 18.0467, -22.9717),
            new("M78",           null,                 CelestialObjectType.Nebula,  5.7783,   0.0767),
            new("Carina Nebula", "NGC 3372",           CelestialObjectType.Nebula, 10.7467, -59.8667),
            new("Eagle Nebula",  "M16 / NGC 6611",     CelestialObjectType.Nebula, 18.3133, -13.7917),
            new("Rosette Nebula","NGC 2237",           CelestialObjectType.Nebula,  6.5333,   4.9667),
            new("Horsehead Neb.","Barnard 33",         CelestialObjectType.Nebula,  5.6833,  -2.4583),
            new("Flame Nebula",  "NGC 2024",           CelestialObjectType.Nebula,  5.6883,  -1.8517),
            new("Cone Nebula",   "NGC 2264",           CelestialObjectType.Nebula,  6.6450,   9.8933),

            // ── Planetary Nebulae ─────────────────────────────────────────────────
            new("M57",          "Ring Nebula",     CelestialObjectType.PlanetaryNebula, 18.8928,  33.0289),
            new("M27",          "Dumbbell Nebula", CelestialObjectType.PlanetaryNebula, 19.9939,  22.7211),
            new("M97",          "Owl Nebula",      CelestialObjectType.PlanetaryNebula, 11.2467,  55.0183),
            new("Helix Nebula", "NGC 7293",        CelestialObjectType.PlanetaryNebula, 22.4942, -20.8367),
            new("Blinking Plan.","NGC 6826",       CelestialObjectType.PlanetaryNebula, 19.7483,  50.5250),
            new("Cat's Eye Neb.","NGC 6543",       CelestialObjectType.PlanetaryNebula, 17.9747,  66.6333),

            // ── Supernova Remnants ────────────────────────────────────────────────
            new("M1",           "Crab Nebula",  CelestialObjectType.SupernovaRemnant,  5.5756,  22.0145),
            new("Veil Nebula",  "NGC 6992",     CelestialObjectType.SupernovaRemnant, 20.9333,  31.7167),
            new("Cassiopeia A", "3C 461",       CelestialObjectType.SupernovaRemnant, 23.3908,  58.8117),

            // ── Galaxies ──────────────────────────────────────────────────────────
            new("M31",  "Andromeda Galaxy",       CelestialObjectType.Galaxy,  0.7122,  41.2689),
            new("M32",  null,                     CelestialObjectType.Galaxy,  0.7106,  40.8650),
            new("M33",  "Triangulum Galaxy",      CelestialObjectType.Galaxy,  1.5644,  30.6600),
            new("M81",  "Bode's Galaxy",          CelestialObjectType.Galaxy,  9.9256,  69.0653),
            new("M82",  "Cigar Galaxy",           CelestialObjectType.Galaxy,  9.9256,  69.6797),
            new("M51",  "Whirlpool Galaxy",       CelestialObjectType.Galaxy, 13.4978,  47.1952),
            new("M101", "Pinwheel Galaxy",        CelestialObjectType.Galaxy, 14.0533,  54.3489),
            new("M104", "Sombrero Galaxy",        CelestialObjectType.Galaxy, 12.6667, -11.6233),
            new("M64",  "Black Eye Galaxy",       CelestialObjectType.Galaxy, 12.9456,  21.6828),
            new("M77",  "Cetus A",                CelestialObjectType.Galaxy,  2.7117,  -0.0133),
            new("M87",  "Virgo A",                CelestialObjectType.Galaxy, 12.5136,  12.3911),
            new("M49",  null,                     CelestialObjectType.Galaxy, 12.4967,   8.0000),
            new("M84",  null,                     CelestialObjectType.Galaxy, 12.4189,  12.8872),
            new("M86",  null,                     CelestialObjectType.Galaxy, 12.4361,  12.9458),
            new("M94",  null,                     CelestialObjectType.Galaxy, 12.8483,  41.1200),
            new("M106", null,                     CelestialObjectType.Galaxy, 12.3167,  47.3033),
            new("Centaurus A",           "NGC 5128", CelestialObjectType.Galaxy, 13.4258, -43.0192),
            new("Large Magellanic Cloud","LMC",      CelestialObjectType.Galaxy,  5.3917, -69.7561),
            new("Small Magellanic Cloud","SMC",      CelestialObjectType.Galaxy,  0.8750, -72.8003),
        };

        public static IEnumerable<CelestialObject> Search(string search) =>
            Objects.Where(o => o.Matches(search));
    }
}
