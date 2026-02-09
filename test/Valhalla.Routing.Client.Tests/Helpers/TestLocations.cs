namespace Valhalla.Routing.Client.Tests.Helpers;

/// <summary>
/// Provides standard test location coordinates for use in integration tests.
/// All coordinates are in Luxembourg, which has good OpenStreetMap coverage in the default Valhalla container.
/// </summary>
public static class TestLocations
{
    /// <summary>
    /// Gets the latitude coordinate for Luxembourg City center (Place d'Armes).
    /// </summary>
    public const double LuxembourgCityCenterLatitude = 49.6116;

    /// <summary>
    /// Gets the longitude coordinate for Luxembourg City center (Place d'Armes).
    /// </summary>
    public const double LuxembourgCityCenterLongitude = 6.1319;

    /// <summary>
    /// Gets the latitude coordinate for Luxembourg Airport (Findel).
    /// </summary>
    public const double LuxembourgAirportLatitude = 49.6233;

    /// <summary>
    /// Gets the longitude coordinate for Luxembourg Airport (Findel).
    /// </summary>
    public const double LuxembourgAirportLongitude = 6.2044;

    /// <summary>
    /// Gets the latitude coordinate for Kirchberg district (European institutions).
    /// </summary>
    public const double KirchbergLatitude = 49.6217;

    /// <summary>
    /// Gets the longitude coordinate for Kirchberg district (European institutions).
    /// </summary>
    public const double KirchbergLongitude = 6.1686;

    /// <summary>
    /// Gets the latitude coordinate for Esch-sur-Alzette (second largest city in Luxembourg).
    /// </summary>
    public const double EschSurAlzetteLatitude = 49.4958;

    /// <summary>
    /// Gets the longitude coordinate for Esch-sur-Alzette (second largest city in Luxembourg).
    /// </summary>
    public const double EschSurAlzetteLongitude = 5.9806;

    /// <summary>
    /// Gets a set of closely-spaced GPS trace points for map matching tests.
    /// These points represent a realistic GPS trace along a route from Luxembourg City Center towards Kirchberg.
    /// Points are spaced approximately 300-500 meters apart, suitable for map matching.
    /// </summary>
    /// <returns>Array of trace points suitable for map matching.</returns>
    public static TracePoint[] GetShortGpsTrace()
    {
        // This trace represents a path from Luxembourg City Center heading northeast
        // Points are roughly 300-500m apart to simulate a realistic GPS trace
        return new[]
        {
            new TracePoint { Lat = 49.6116, Lon = 6.1319 }, // Luxembourg City Center (Place d'Armes)
            new TracePoint { Lat = 49.6140, Lon = 6.1360 }, // ~400m northeast
            new TracePoint { Lat = 49.6165, Lon = 6.1405 }, // ~450m northeast
            new TracePoint { Lat = 49.6190, Lon = 6.1450 }, // ~450m northeast
            new TracePoint { Lat = 49.6210, Lon = 6.1500 }, // ~450m northeast
        };
    }

    /// <summary>
    /// Gets an encoded polyline representing a valid route in Luxembourg for map matching tests.
    /// This polyline was generated from an actual route calculation between City Center and Kirchberg.
    /// </summary>
    /// <returns>Encoded polyline string (precision 6).</returns>
    public static string GetValidEncodedPolyline()
    {
        // This is a real encoded polyline from Luxembourg City Center towards Kirchberg
        // It represents actual road segments in the Luxembourg OSM data
        // Generated from coordinates along Avenue de la Liberté and Rue Alcide de Gasperi
        return PolylineEncoder.Encode(
            new[]
            {
                (49.6116, 6.1319),
                (49.6140, 6.1360),
                (49.6165, 6.1405),
                (49.6190, 6.1450),
                (49.6210, 6.1500),
            },
            precision: 6);
    }
}
