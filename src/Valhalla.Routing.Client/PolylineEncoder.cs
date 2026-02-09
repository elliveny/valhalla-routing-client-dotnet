using System.Text;

namespace Valhalla.Routing;

/// <summary>
/// Provides utilities for encoding and decoding polylines using Google's Polyline Algorithm.
/// Valhalla uses precision 6 (6 decimal places for coordinates).
/// </summary>
/// <remarks>
/// Reference: https://developers.google.com/maps/documentation/utilities/polylinealgorithm.
/// </remarks>
public static class PolylineEncoder
{
    /// <summary>
    /// Encodes a list of coordinates into a polyline string.
    /// </summary>
    /// <param name="coordinates">List of (latitude, longitude) tuples.</param>
    /// <param name="precision">Decimal precision (default 6 for Valhalla). Must be between 0 and 10.</param>
    /// <returns>Encoded polyline string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when coordinates is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when precision is less than 0 or greater than 10.</exception>
    public static string Encode(IEnumerable<(double latitude, double longitude)> coordinates, int precision = 6)
    {
        ArgumentNullException.ThrowIfNull(coordinates);

        if (precision < 0 || precision > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(precision), precision, "Precision must be between 0 and 10.");
        }

        var coordList = coordinates.ToList();
        if (coordList.Count == 0)
        {
            return string.Empty;
        }

        var factor = Math.Pow(10, precision);
        var output = new StringBuilder();
        var prevLat = 0L;
        var prevLon = 0L;

        foreach (var (latitude, longitude) in coordList)
        {
            var latScaled = (long)Math.Round(latitude * factor, MidpointRounding.AwayFromZero);
            var lonScaled = (long)Math.Round(longitude * factor, MidpointRounding.AwayFromZero);

            EncodeValue(latScaled - prevLat, output);
            EncodeValue(lonScaled - prevLon, output);

            prevLat = latScaled;
            prevLon = lonScaled;
        }

        return output.ToString();
    }

    /// <summary>
    /// Decodes a polyline string into a list of coordinates.
    /// </summary>
    /// <param name="encodedPolyline">The encoded polyline string.</param>
    /// <param name="precision">Decimal precision (default 6 for Valhalla). Must be between 0 and 10.</param>
    /// <returns>List of (latitude, longitude) tuples.</returns>
    /// <exception cref="ArgumentNullException">Thrown when encodedPolyline is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when precision is less than 0 or greater than 10.</exception>
    /// <exception cref="FormatException">Thrown when the encoded polyline is malformed or truncated.</exception>
    public static IReadOnlyList<(double latitude, double longitude)> Decode(string encodedPolyline, int precision = 6)
    {
        ArgumentNullException.ThrowIfNull(encodedPolyline);

        if (precision < 0 || precision > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(precision), precision, "Precision must be between 0 and 10.");
        }

        if (string.IsNullOrEmpty(encodedPolyline))
        {
            return Array.Empty<(double, double)>();
        }

        var factor = Math.Pow(10, precision);
        var coordinates = new List<(double, double)>();
        var index = 0;
        var lat = 0L;
        var lon = 0L;

        while (index < encodedPolyline.Length)
        {
            lat += DecodeValue(encodedPolyline, ref index);
            lon += DecodeValue(encodedPolyline, ref index);

            coordinates.Add((lat / factor, lon / factor));
        }

        return coordinates.AsReadOnly();
    }

    private static void EncodeValue(long value, StringBuilder output)
    {
        var encoded = value < 0 ? ~(value << 1) : value << 1;

        while (encoded >= 0x20)
        {
            output.Append((char)((0x20 | (encoded & 0x1f)) + 63));
            encoded >>= 5;
        }

        output.Append((char)(encoded + 63));
    }

    private static long DecodeValue(string encoded, ref int index)
    {
        var result = 0L;
        var shift = 0;
        long b;

        do
        {
            if (index >= encoded.Length)
            {
                throw new FormatException("Malformed or truncated polyline string: unexpected end of input.");
            }

            b = encoded[index++] - 63;
            result |= (b & 0x1f) << shift;
            shift += 5;
        }
        while (b >= 0x20);

        return (result & 1) != 0 ? ~(result >> 1) : result >> 1;
    }
}
