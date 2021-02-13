using System;

namespace ExifLib
{
    /// <summary>
    /// All exif tags as per the Exif standard 2.3, JEITA CP-3451C
    /// </summary>
    public enum ExifTags : ushort
    {
        // primary tags
        [IFD(IFD.IFD0)]
        ImageWidth = 0x100,
        [IFD(IFD.IFD0)]
        ImageLength = 0x101,
        [IFD(IFD.IFD0)]
        BitsPerSample = 0x102,
        [IFD(IFD.IFD0)]
        Compression = 0x103,
        [IFD(IFD.IFD0)]
        PhotometricInterpretation = 0x106,
        [IFD(IFD.IFD0)]
        ImageDescription = 0x10E,
        [IFD(IFD.IFD0)]
        Make = 0x10F,
        [IFD(IFD.IFD0)]
        Model = 0x110,
        [IFD(IFD.IFD0)]
        StripOffsets = 0x111,
        [IFD(IFD.IFD0)]
        Orientation = 0x112,
        [IFD(IFD.IFD0)]
        SamplesPerPixel = 0x115,
        [IFD(IFD.IFD0)]
        RowsPerStrip = 0x116,
        [IFD(IFD.IFD0)]
        StripByteCounts = 0x117,
        [IFD(IFD.IFD0)]
        XResolution = 0x11A,
        [IFD(IFD.IFD0)]
        YResolution = 0x11B,
        [IFD(IFD.IFD0)]
        PlanarConfiguration = 0x11C,
        [IFD(IFD.IFD0)]
        ResolutionUnit = 0x128,
        [IFD(IFD.IFD0)]
        TransferFunction = 0x12D,
        [IFD(IFD.IFD0)]
        Software = 0x131,
        [IFD(IFD.IFD0)]
        DateTime = 0x132,
        [IFD(IFD.IFD0)]
        Artist = 0x13B,
        [IFD(IFD.IFD0)]
        WhitePoint = 0x13E,
        [IFD(IFD.IFD0)]
        PrimaryChromaticities = 0x13F,
        [IFD(IFD.IFD0)]
        JPEGInterchangeFormat = 0x201,
        [IFD(IFD.IFD0)]
        JPEGInterchangeFormatLength = 0x202,
        [IFD(IFD.IFD0)]
        YCbCrCoefficients = 0x211,
        [IFD(IFD.IFD0)]
        YCbCrSubSampling = 0x212,
        [IFD(IFD.IFD0)]
        YCbCrPositioning = 0x213,
        [IFD(IFD.IFD0)]
        ReferenceBlackWhite = 0x214,
        [IFD(IFD.IFD0)]
        Copyright = 0x8298,

        // EXIF tags
        [IFD(IFD.EXIF)]
        ExposureTime = 0x829A,
        [IFD(IFD.EXIF)]
        FNumber = 0x829D,
        [IFD(IFD.EXIF)]
        ExposureProgram = 0x8822,
        [IFD(IFD.EXIF)]
        SpectralSensitivity = 0x8824,
        [IFD(IFD.EXIF)]
        [Obsolete("Renamed to PhotographicSensitivity")]
        ISOSpeedRatings = 0x8827,
        [IFD(IFD.EXIF)]
        PhotographicSensitivity = 0x8827,
        [IFD(IFD.EXIF)]
        OECF = 0x8828,
        [IFD(IFD.EXIF)]
        SensitivityType = 0x8830,
        [IFD(IFD.EXIF)]
        StandardOutputSensitivity = 0x8831,
        [IFD(IFD.EXIF)]
        RecommendedExposureIndex = 0x8832,
        [IFD(IFD.EXIF)]
        ISOSpeed = 0x8833,
        [IFD(IFD.EXIF)]
        ISOSpeedLatitudeyyy = 0x8834,
        [IFD(IFD.EXIF)]
        ISOSpeedLatitudezzz = 0x8835,

        [IFD(IFD.EXIF)]
        ExifVersion = 0x9000,
        [IFD(IFD.EXIF)]
        DateTimeOriginal = 0x9003,
        [IFD(IFD.EXIF)]
        DateTimeDigitized = 0x9004,
        [IFD(IFD.EXIF)]
        ComponentsConfiguration = 0x9101,
        [IFD(IFD.EXIF)]
        CompressedBitsPerPixel = 0x9102,
        [IFD(IFD.EXIF)]
        ShutterSpeedValue = 0x9201,
        [IFD(IFD.EXIF)]
        ApertureValue = 0x9202,
        [IFD(IFD.EXIF)]
        BrightnessValue = 0x9203,
        [IFD(IFD.EXIF)]
        ExposureBiasValue = 0x9204,
        [IFD(IFD.EXIF)]
        MaxApertureValue = 0x9205,
        [IFD(IFD.EXIF)]
        SubjectDistance = 0x9206,
        [IFD(IFD.EXIF)]
        MeteringMode = 0x9207,
        [IFD(IFD.EXIF)]
        LightSource = 0x9208,
        [IFD(IFD.EXIF)]
        Flash = 0x9209,
        [IFD(IFD.EXIF)]
        FocalLength = 0x920A,
        [IFD(IFD.EXIF)]
        SubjectArea = 0x9214,
        [IFD(IFD.EXIF)]
        MakerNote = 0x927C,
        [IFD(IFD.EXIF)]
        UserComment = 0x9286,
        [IFD(IFD.EXIF)]
        SubsecTime = 0x9290,
        [IFD(IFD.EXIF)]
        SubsecTimeOriginal = 0x9291,
        [IFD(IFD.EXIF)]
        SubsecTimeDigitized = 0x9292,
        [IFD(IFD.EXIF)]
        FlashpixVersion = 0xA000,
        [IFD(IFD.EXIF)]
        ColorSpace = 0xA001,
        [IFD(IFD.EXIF)]
        PixelXDimension = 0xA002,
        [IFD(IFD.EXIF)]
        PixelYDimension = 0xA003,
        [IFD(IFD.EXIF)]
        RelatedSoundFile = 0xA004,
        [IFD(IFD.EXIF)]
        FlashEnergy = 0xA20B,
        [IFD(IFD.EXIF)]
        SpatialFrequencyResponse = 0xA20C,
        [IFD(IFD.EXIF)]
        FocalPlaneXResolution = 0xA20E,
        [IFD(IFD.EXIF)]
        FocalPlaneYResolution = 0xA20F,
        [IFD(IFD.EXIF)]
        FocalPlaneResolutionUnit = 0xA210,
        [IFD(IFD.EXIF)]
        SubjectLocation = 0xA214,
        [IFD(IFD.EXIF)]
        ExposureIndex = 0xA215,
        [IFD(IFD.EXIF)]
        SensingMethod = 0xA217,
        [IFD(IFD.EXIF)]
        FileSource = 0xA300,
        [IFD(IFD.EXIF)]
        SceneType = 0xA301,
        [IFD(IFD.EXIF)]
        CFAPattern = 0xA302,
        [IFD(IFD.EXIF)]
        CustomRendered = 0xA401,
        [IFD(IFD.EXIF)]
        ExposureMode = 0xA402,
        [IFD(IFD.EXIF)]
        WhiteBalance = 0xA403,
        [IFD(IFD.EXIF)]
        DigitalZoomRatio = 0xA404,
        [IFD(IFD.EXIF)]
        FocalLengthIn35mmFilm = 0xA405,
        [IFD(IFD.EXIF)]
        SceneCaptureType = 0xA406,
        [IFD(IFD.EXIF)]
        GainControl = 0xA407,
        [IFD(IFD.EXIF)]
        Contrast = 0xA408,
        [IFD(IFD.EXIF)]
        Saturation = 0xA409,
        [IFD(IFD.EXIF)]
        Sharpness = 0xA40A,
        [IFD(IFD.EXIF)]
        DeviceSettingDescription = 0xA40B,
        [IFD(IFD.EXIF)]
        SubjectDistanceRange = 0xA40C,
        [IFD(IFD.EXIF)]
        ImageUniqueID = 0xA420,
        [IFD(IFD.EXIF)]
        CameraOwnerName = 0xA430,
        [IFD(IFD.EXIF)]
        BodySerialNumber = 0xA431,
        [IFD(IFD.EXIF)]
        LensSpecification = 0xA432,
        [IFD(IFD.EXIF)]
        LensMake = 0xA433,
        [IFD(IFD.EXIF)]
        LensModel = 0xA434,
        [IFD(IFD.EXIF)]
        LensSerialNumber = 0xA435,

        // GPS tags
        [IFD(IFD.GPS)]
        GPSVersionID = 0x0,
        [IFD(IFD.GPS)]
        GPSLatitudeRef = 0x1,
        [IFD(IFD.GPS)]
        GPSLatitude = 0x2,
        [IFD(IFD.GPS)]
        GPSLongitudeRef = 0x3,
        [IFD(IFD.GPS)]
        GPSLongitude = 0x4,
        [IFD(IFD.GPS)]
        GPSAltitudeRef = 0x5,
        [IFD(IFD.GPS)]
        GPSAltitude = 0x6,
        [IFD(IFD.GPS)]
        GPSTimestamp = 0x7,
        [IFD(IFD.GPS)]
        GPSSatellites = 0x8,
        [IFD(IFD.GPS)]
        GPSStatus = 0x9,
        [IFD(IFD.GPS)]
        GPSMeasureMode = 0xA,
        [IFD(IFD.GPS)]
        GPSDOP = 0xB,
        [IFD(IFD.GPS)]
        GPSSpeedRef = 0xC,
        [IFD(IFD.GPS)]
        GPSSpeed = 0xD,
        [IFD(IFD.GPS)]
        GPSTrackRef = 0xE,
        [IFD(IFD.GPS)]
        GPSTrack = 0xF,
        [IFD(IFD.GPS)]
        GPSImgDirectionRef = 0x10,
        [IFD(IFD.GPS)]
        GPSImgDirection = 0x11,
        [IFD(IFD.GPS)]
        GPSMapDatum = 0x12,
        [IFD(IFD.GPS)]
        GPSDestLatitudeRef = 0x13,
        [IFD(IFD.GPS)]
        GPSDestLatitude = 0x14,
        [IFD(IFD.GPS)]
        GPSDestLongitudeRef = 0x15,
        [IFD(IFD.GPS)]
        GPSDestLongitude = 0x16,
        [IFD(IFD.GPS)]
        GPSDestBearingRef = 0x17,
        [IFD(IFD.GPS)]
        GPSDestBearing = 0x18,
        [IFD(IFD.GPS)]
        GPSDestDistanceRef = 0x19,
        [IFD(IFD.GPS)]
        GPSDestDistance = 0x1A,
        [IFD(IFD.GPS)]
        GPSProcessingMethod = 0x1B,
        [IFD(IFD.GPS)]
        GPSAreaInformation = 0x1C,
        [IFD(IFD.GPS)]
        GPSDateStamp = 0x1D,
        [IFD(IFD.GPS)]
        GPSDifferential = 0x1E,
        [IFD(IFD.GPS)]
        GPSHPositioningError = 0x1F,

        // Microsoft Windows metadata. Non-standard, but ubiquitous
        [IFD(IFD.IFD0)]
        XPTitle = 0x9c9b,
        [IFD(IFD.IFD0)]
        XPComment = 0x9c9c,
        [IFD(IFD.IFD0)]
        XPAuthor = 0x9c9d,
        [IFD(IFD.IFD0)]
        XPKeywords = 0x9c9e,
        [IFD(IFD.IFD0)]
        XPSubject = 0x9c9f
    }

    [AttributeUsage(AttributeTargets.Field)]
    internal class IFDAttribute : Attribute
    {
        public readonly IFD IFD;

        public IFDAttribute(IFD ifd)
        {
            IFD = ifd;
        }
    }

    public enum IFD
    {
        IFD0,
        EXIF,
        GPS
    }
}
