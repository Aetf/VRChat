using UnityEngine;
using System;
using System.Collections;

namespace KinectCommon
{

    public static class Constants
    {
        public const int SkeletonCount = 6;
        public const int SkeletonMaxTracked = 2;
        public const int SkeletonInvalidTrackingID = 0;

        public const float DepthHorizontalFOV = 58.5f;
        public const float DepthVerticalFOV = 45.6f;

        public const int ColorImageWidth = 640;
        public const int ColorImageHeight = 480;
        public const NuiImageResolution ColorImageResolution = NuiImageResolution.resolution640x480;

        public const int DepthImageWidth = 640;
        public const int DepthImageHeight = 480;
        public const NuiImageResolution DepthImageResolution = NuiImageResolution.resolution640x480;

        public const bool IsNearMode = false;

        public const float MinTimeBetweenSameGestures = 0.0f;
        public const float PoseCompleteDuration = 1.0f;
        public const float ClickStayDuration = 2.5f;
    }

    /// <summary>
    ///Structs and constants for interfacing C# with the Kinect.dll 
    /// </summary>

    [Flags]
    public enum NuiInitializeFlags : uint
    {
        UsesAudio = 0x10000000,
        UsesDepthAndPlayerIndex = 0x00000001,
        UsesColor = 0x00000002,
        UsesSkeleton = 0x00000008,
        UsesDepth = 0x00000020,
        UsesHighQualityColor = 0x00000040
    }

    public enum NuiErrorCodes : uint
    {
        FrameNoData = 0x83010001,
        StreamNotEnabled = 0x83010002,
        ImageStreamInUse = 0x83010003,
        FrameLimitExceeded = 0x83010004,
        FeatureNotInitialized = 0x83010005,
        DeviceNotGenuine = 0x83010006,
        InsufficientBandwidth = 0x83010007,
        DeviceNotSupported = 0x83010008,
        DeviceInUse = 0x83010009,

        DatabaseNotFound = 0x8301000D,
        DatabaseVersionMismatch = 0x8301000E,
        HardwareFeatureUnavailable = 0x8301000F,

        DeviceNotConnected = 0x83010014,
        DeviceNotReady = 0x83010015,
        SkeletalEngineBusy = 0x830100AA,
        DeviceNotPowered = 0x8301027F,
    }

    public enum NuiSkeletonPositionIndex : int
    {
        AnkleLeft = 14,
        AnkleRight = 18,
        ElbowLeft = 5,
        ElbowRight = 9,
        FootLeft = 15,
        FootRight = 19,
        HandLeft = 7,
        HandRight = 11,
        HandTipLeft = 21,
        HandTipRight = 23,
        Head = 3,
        HipLeft = 12,
        HipRight = 16,
        KneeLeft = 13,
        KneeRight = 17,
        Neck = 2,
        ShoulderLeft = 4,
        ShoulderRight = 8,
        SpineBase = 0,
        SpineMid = 1,
        SpineShoulder = 20,
        ThumbLeft = 22,
        ThumbRight = 24,
        WristLeft = 6,
        WristRight = 10,

        Count = 25
    }

    public enum NuiSkeletonPositionTrackingState
    {
        NotTracked = 0,
        Inferred,
        Tracked
    }

    public enum NuiSkeletonTrackingState
    {
        NotTracked = 0,
        PositionOnly,
        SkeletonTracked
    }

    public enum NuiImageType
    {
        DepthAndPlayerIndex = 0,    // USHORT
        Color,                      // RGB32 data
        ColorYUV,                   // YUY2 stream from camera h/w, but converted to RGB32 before user getting it.
        ColorRawYUV,                // YUY2 stream from camera h/w.
        Depth                       // USHORT
    }

    public enum NuiImageResolution
    {
        resolutionInvalid = -1,
        resolution80x60 = 0,
        resolution320x240 = 1,
        resolution640x480 = 2,
        resolution1280x960 = 3     // for hires color only
    }

    public enum NuiImageStreamFlags
    {
        None = 0x00000000,
        SupressNoFrameData = 0x0001000,
        EnableNearMode = 0x00020000,
        TooFarIsNonZero = 0x0004000
    }

    [Flags]
    public enum FrameEdges
    {
        None = 0,
        Right = 1,
        Left = 2,
        Top = 4,
        Bottom = 8
    }

    public struct NuiSkeletonData
    {
        public NuiSkeletonTrackingState eTrackingState;
        public ulong TrackingId;
        public Vector4 Position
        {
            get {
                return SkeletonPositions[(int)NuiSkeletonPositionIndex.SpineBase];
            }
            set { }
        }
        // Count = 20
        public Vector4[] SkeletonPositions;
        // Count = 20
        public NuiSkeletonPositionTrackingState[] SkeletonTrackingState;
        public FrameEdges ClippedEdges;
    }

    public struct NuiSkeletonFrame
    {
        // Count = 6
        public NuiSkeletonData[] SkeletonData;
    }

    public struct NuiTransformSmoothParameters
    {
        public float fSmoothing;
        public float fCorrection;
        public float fPrediction;
        public float fJitterRadius;
        public float fMaxDeviationRadius;
    }

    public struct NuiSkeletonBoneRotation
    {
        public Matrix4x4 rotationMatrix;
        public Quaternion rotationQuaternion;
    }

    public struct NuiSkeletonBoneOrientation
    {
        public NuiSkeletonPositionIndex endJoint;
        public NuiSkeletonPositionIndex startJoint;
        public NuiSkeletonBoneRotation hierarchicalRotation;
        public NuiSkeletonBoneRotation absoluteRotation;
    }

    public struct NuiImageViewArea
    {
        public int eDigitalZoom;
        public int lCenterX;
        public int lCenterY;
    }

    public class NuiImageBuffer
    {
        public int m_Width;
        public int m_Height;
        public int m_BytesPerPixel;
        public IntPtr m_pBuffer;
    }

    public struct NuiImageFrame
    {
        public Int64 liTimeStamp;
        public uint dwFrameNumber;
        public NuiImageType eImageType;
        public NuiImageResolution eResolution;
        public IntPtr pFrameTexture;
        public uint dwFrameFlags_NotUsed;
        public NuiImageViewArea ViewArea_NotUsed;
    }

    public struct NuiLockedRect
    {
        public int pitch;
        public int size;
        public IntPtr pBits;

    }

    public struct ColorCust
    {
        public byte b;
        public byte g;
        public byte r;
        public byte a;
    }

    public struct ColorBuffer
    {
        // Count = 640*480
        public ColorCust[] pixels;
    }

    public struct DepthBuffer
    {
        // Count = 640*480
        public ushort[] pixels;
    }

    public struct NuiSurfaceDesc
    {
        uint width;
        uint height;
    }
}
