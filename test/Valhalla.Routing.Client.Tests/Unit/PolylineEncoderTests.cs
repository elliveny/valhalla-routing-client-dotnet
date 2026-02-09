using Valhalla.Routing;

namespace Valhalla.Routing.Client.Tests.Unit;

/// <summary>
/// Unit tests for PolylineEncoder encoding and decoding utilities.
/// </summary>
public class PolylineEncoderTests
{
    [Fact]
    public void PolylineEncoder_Decode_KnownTestVector_ReturnsCorrectCoordinates()
    {
        // Arrange
        // Test vector from Google's polyline algorithm documentation (precision 5)
        // "_p~iF~ps|U_ulLnnqC_mqNvxq`@" represents coordinates with precision 5
        // For precision 6, we use a different test vector
        // "_izlhA~rlgdF_{geC~ywl@_kwzCn`{nI" represents:
        // (38.5, -120.2), (40.7, -120.95), (43.252, -126.453)
        var encoded = "_izlhA~rlgdF_{geC~ywl@_kwzCn`{nI";

        // Act
        var coordinates = PolylineEncoder.Decode(encoded, precision: 6);

        // Assert
        coordinates.Should().NotBeNull();
        coordinates.Should().HaveCount(3);

        // Verify coordinates with tolerance for floating-point precision
        coordinates[0].latitude.Should().BeApproximately(38.5, 0.000001);
        coordinates[0].longitude.Should().BeApproximately(-120.2, 0.000001);
        coordinates[1].latitude.Should().BeApproximately(40.7, 0.000001);
        coordinates[1].longitude.Should().BeApproximately(-120.95, 0.000001);
        coordinates[2].latitude.Should().BeApproximately(43.252, 0.000001);
        coordinates[2].longitude.Should().BeApproximately(-126.453, 0.000001);
    }

    [Fact]
    public void PolylineEncoder_Encode_KnownTestVector_ReturnsCorrectString()
    {
        // Arrange
        var coordinates = new List<(double lat, double lon)>
        {
            (38.5, -120.2),
            (40.7, -120.95),
            (43.252, -126.453),
        };

        // Act
        var encoded = PolylineEncoder.Encode(coordinates, precision: 6);

        // Assert
        encoded.Should().Be("_izlhA~rlgdF_{geC~ywl@_kwzCn`{nI");
    }

    [Fact]
    public void PolylineEncoder_RoundTrip_PreservesCoordinates()
    {
        // Arrange
        var originalCoordinates = new List<(double lat, double lon)>
        {
            (49.6116, 6.1319),  // Luxembourg City Center
            (49.6233, 6.2044),  // Luxembourg Airport
            (49.6217, 6.1686),  // Kirchberg
            (49.4958, 5.9806),  // Esch-sur-Alzette
        };

        // Act
        var encoded = PolylineEncoder.Encode(originalCoordinates);
        var decoded = PolylineEncoder.Decode(encoded);

        // Assert
        decoded.Should().HaveCount(originalCoordinates.Count);

        for (var i = 0; i < originalCoordinates.Count; i++)
        {
            decoded[i].latitude.Should().BeApproximately(originalCoordinates[i].lat, 0.000001);
            decoded[i].longitude.Should().BeApproximately(originalCoordinates[i].lon, 0.000001);
        }
    }

    [Fact]
    public void PolylineEncoder_Decode_EmptyString_ReturnsEmptyList()
    {
        // Arrange
        var encoded = string.Empty;

        // Act
        var coordinates = PolylineEncoder.Decode(encoded);

        // Assert
        coordinates.Should().NotBeNull();
        coordinates.Should().BeEmpty();
    }

    [Fact]
    public void PolylineEncoder_Encode_EmptyList_ReturnsEmptyString()
    {
        // Arrange
        var coordinates = new List<(double lat, double lon)>();

        // Act
        var encoded = PolylineEncoder.Encode(coordinates);

        // Assert
        encoded.Should().BeEmpty();
    }

    [Fact]
    public void PolylineEncoder_Precision6_MatchesValhallaOutput()
    {
        // Arrange
        // Test that default precision 6 is used (Valhalla's default)
        var coordinates = new List<(double lat, double lon)>
        {
            (52.5200, 13.4050),  // Berlin
            (52.5205, 13.4060),
            (52.5210, 13.4070),
        };

        // Act - Using default precision (should be 6)
        var encodedDefault = PolylineEncoder.Encode(coordinates);
        var encodedExplicit6 = PolylineEncoder.Encode(coordinates, precision: 6);

        // Assert - Both should be identical
        encodedDefault.Should().Be(encodedExplicit6);

        // Verify decoding works with default precision
        var decodedDefault = PolylineEncoder.Decode(encodedDefault);
        var decodedExplicit6 = PolylineEncoder.Decode(encodedExplicit6, precision: 6);

        decodedDefault.Should().HaveCount(coordinates.Count);
        decodedExplicit6.Should().HaveCount(coordinates.Count);

        for (var i = 0; i < coordinates.Count; i++)
        {
            decodedDefault[i].latitude.Should().BeApproximately(coordinates[i].lat, 0.000001);
            decodedDefault[i].longitude.Should().BeApproximately(coordinates[i].lon, 0.000001);
        }
    }

    [Fact]
    public void PolylineEncoder_Encode_NullCoordinates_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => PolylineEncoder.Encode(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PolylineEncoder_Decode_NullString_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => PolylineEncoder.Decode(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PolylineEncoder_Encode_NegativePrecision_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var coordinates = new List<(double lat, double lon)> { (49.6116, 6.1319) };

        // Act & Assert
        var act = () => PolylineEncoder.Encode(coordinates, precision: -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void PolylineEncoder_Encode_PrecisionTooHigh_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var coordinates = new List<(double lat, double lon)> { (49.6116, 6.1319) };

        // Act & Assert
        var act = () => PolylineEncoder.Encode(coordinates, precision: 11);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void PolylineEncoder_Decode_NegativePrecision_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var encoded = "_izlhA~rlgdF";

        // Act & Assert
        var act = () => PolylineEncoder.Decode(encoded, precision: -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void PolylineEncoder_Decode_PrecisionTooHigh_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var encoded = "_izlhA~rlgdF";

        // Act & Assert
        var act = () => PolylineEncoder.Decode(encoded, precision: 11);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void PolylineEncoder_Decode_MalformedPolyline_ThrowsFormatException()
    {
        // Arrange - A truncated polyline string (incomplete encoding)
        var malformed = "?";

        // Act & Assert
        var act = () => PolylineEncoder.Decode(malformed);
        act.Should().Throw<FormatException>()
            .WithMessage("*Malformed or truncated polyline*");
    }

    [Fact]
    public void PolylineEncoder_Decode_ReturnsReadOnlyCollection()
    {
        // Arrange
        var encoded = "_izlhA~rlgdF";

        // Act
        var result = PolylineEncoder.Decode(encoded);

        // Assert
        result.Should().BeAssignableTo<System.Collections.ObjectModel.ReadOnlyCollection<(double latitude, double longitude)>>();
    }
}
