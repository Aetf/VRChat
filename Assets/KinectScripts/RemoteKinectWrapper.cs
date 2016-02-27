// comment or uncomment the following #define directives
// depending on whether you use KinectExtras together with KinectManager

//#define USE_KINECT_INTERACTION_OR_FACETRACKING
//#define USE_SPEECH_RECOGNITION

using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;
using System.Text;

using KinectCommon;
using BodyData = LibRawFrameData.KinectData;

// Wrapper class that holds the various structs and dll imports
// needed to set up a model with the Kinect.
public class RemoteKinectWrapper
{
    public static int NuiInitialize(NuiInitializeFlags dwFlags)
    {
        return 0;
    }

    public static void NuiShutdown()
    {
    }

    public static int NuiCameraElevationSetAngle(int angle)
    {
        return 0;
    }

    public static int NuiCameraElevationGetAngle(out int plAngleDegrees)
    {
        plAngleDegrees = 0;
        return 0;
    }

    public static int NuiImageGetColorPixelCoordinatesFromDepthPixelAtResolution(NuiImageResolution eColorResolution, NuiImageResolution eDepthResolution, ref NuiImageViewArea pcViewArea, int lDepthX, int lDepthY, ushort sDepthValue, out int plColorX, out int plColorY)
    {
        plColorX = plColorY = 0;
        return -1;
    }

    /*
	 * kinect skeleton functions
	 */
    public static int NuiSkeletonTrackingEnable(IntPtr hNextFrameEvent, uint dwFlags)
	{
		return 0;
	}

    public static uint NuiSkeletonGetNextFrame(uint dwMillisecondsToWait, ref NuiSkeletonFrame pSkeletonFrame)
    {
        //TODO: NuiSkeletonGetNextFrame
        if (dwMillisecondsToWait != 0)
            Debug.LogWarning("non-zero wdMillisecondsToWait is not implemented, will be ignored.");
        RawFrameData rawFrame = KinectProxy.Instance.GetLatestFrame();
        if (rawFrame != null)
        {
            int count = 0;
            foreach (var body in rawFrame.bodies)
            {
                PopulateSkeletonData(body, ref pSkeletonFrame.SkeletonData[count++]);
            }
            for (; count < Constants.SkeletonCount; count++)
            {
                pSkeletonFrame.SkeletonData[count].eTrackingState = NuiSkeletonTrackingState.NotTracked;
            }
            return 0;
        }
        return (uint)NuiErrorCodes.SkeletalEngineBusy;
    }

    public static int NuiTransformSmooth(ref NuiSkeletonFrame pSkeletonFrame, ref NuiTransformSmoothParameters pSmoothingParams)
    {
        return 0;
    }
	
    /*
	 * kinect video functions
	 */

    public static int NuiImageStreamOpen(NuiImageType eImageType, NuiImageResolution eResolution, uint dwImageFrameFlags_NotUsed, uint dwFrameLimit, IntPtr hNextFrameEvent, ref IntPtr phStreamHandle)
    {
        return 0;
    }
	
    public static int NuiImageStreamGetNextFrame(IntPtr phStreamHandle, uint dwMillisecondsToWait, ref IntPtr ppcImageFrame)
    {
        return 0;
    }
	
    public static int NuiImageStreamReleaseFrame(IntPtr phStreamHandle, IntPtr ppcImageFrame)
    {
        return 0;
    }
	
	public static int NuiImageStreamSetImageFrameFlags (IntPtr phStreamHandle, NuiImageStreamFlags dvImageFrameFlags)
    {
        return 0;
    }
	
    public static int NuiImageResolutionToSize(NuiImageResolution eResolution,out uint frameWidth,out uint frameHeight)
    {
        switch (eResolution)
        {
            case NuiImageResolution.resolution1280x960:
                frameHeight = 960;
                frameWidth = 1280;
                break;
            case NuiImageResolution.resolution640x480:
                frameHeight = 480;
                frameWidth = 640;
                break;
            case NuiImageResolution.resolution320x240:
                frameHeight = 240;
                frameWidth = 320;
                break;
            case NuiImageResolution.resolution80x60:
                frameHeight = 60;
                frameWidth = 80;
                break;
            default:
                frameHeight = 0;
                frameWidth = 0;
                return -1;
        }
        return 0;
    }

    public static string GetNuiErrorString(int hr)
    {
        string message = string.Empty;
        uint uhr = (uint)hr;

        switch (uhr)
        {
            case (uint)NuiErrorCodes.FrameNoData:
                message = "Frame contains no data.";
                break;
            case (uint)NuiErrorCodes.StreamNotEnabled:
                message = "Stream is not enabled.";
                break;
            case (uint)NuiErrorCodes.ImageStreamInUse:
                message = "Image stream is already in use.";
                break;
            case (uint)NuiErrorCodes.FrameLimitExceeded:
                message = "Frame limit is exceeded.";
                break;
            case (uint)NuiErrorCodes.FeatureNotInitialized:
                message = "Feature is not initialized.";
                break;
            case (uint)NuiErrorCodes.DeviceNotGenuine:
                message = "Device is not genuine.";
                break;
            case (uint)NuiErrorCodes.InsufficientBandwidth:
                message = "Bandwidth is not sufficient.";
                break;
            case (uint)NuiErrorCodes.DeviceNotSupported:
                message = "Device is not supported (e.g. Kinect for XBox 360).";
                break;
            case (uint)NuiErrorCodes.DeviceInUse:
                message = "Device is already in use.";
                break;
            case (uint)NuiErrorCodes.DatabaseNotFound:
                message = "Database not found.";
                break;
            case (uint)NuiErrorCodes.DatabaseVersionMismatch:
                message = "Database version mismatch.";
                break;
            case (uint)NuiErrorCodes.HardwareFeatureUnavailable:
                message = "Hardware feature is not available.";
                break;
            case (uint)NuiErrorCodes.DeviceNotConnected:
                message = "Device is not connected.";
                break;
            case (uint)NuiErrorCodes.DeviceNotReady:
                message = "Device is not ready.";
                break;
            case (uint)NuiErrorCodes.SkeletalEngineBusy:
                message = "Skeletal engine is busy.";
                break;
            case (uint)NuiErrorCodes.DeviceNotPowered:
                message = "Device is not powered.";
                break;

            default:
                message = "hr=0x" + uhr.ToString("X");
                break;
        }

        return message;
    }

    public static int GetDepthWidth()
    {
        return Constants.DepthImageWidth;
    }

    public static int GetDepthHeight()
    {
        return Constants.DepthImageHeight;
    }

    public static int GetColorWidth()
    {
        return Constants.ColorImageWidth;
    }

    public static int GetColorHeight()
    {
        return Constants.ColorImageHeight;
    }

    public static Vector3 MapSkeletonPointToDepthPoint(Vector3 skeletonPoint)
    {
        float fDepthX;
        float fDepthY;
        float fDepthZ;

        NuiTransformSkeletonToDepthImage(skeletonPoint, out fDepthX, out fDepthY, out fDepthZ);

        Vector3 point = new Vector3();
        point.x = (int)((fDepthX * Constants.DepthImageWidth) + 0.5f);
        point.y = (int)((fDepthY * Constants.DepthImageHeight) + 0.5f);
        point.z = (int)(fDepthZ + 0.5f);

        return point;
    }

    private static void NuiTransformSkeletonToDepthImage(Vector3 vPoint, out float pfDepthX, out float pfDepthY, out float pfDepthZ)
    {
        if (vPoint.z > float.Epsilon)
        {
            pfDepthX = 0.5f + ((vPoint.x * 285.63f) / (vPoint.z * 320f));
            pfDepthY = 0.5f - ((vPoint.y * 285.63f) / (vPoint.z * 240f));
            pfDepthZ = vPoint.z * 1000f;
        }
        else
        {
            pfDepthX = 0f;
            pfDepthY = 0f;
            pfDepthZ = 0f;
        }
    }

    public static int GetSkeletonJointParent(int jointIndex)
    {
        switch (jointIndex)
        {
            case (int)NuiSkeletonPositionIndex.SpineBase:
                return (int)NuiSkeletonPositionIndex.SpineBase;
            case (int)NuiSkeletonPositionIndex.SpineMid:
                return (int)NuiSkeletonPositionIndex.SpineBase;
            case (int)NuiSkeletonPositionIndex.Neck:
                return (int)NuiSkeletonPositionIndex.SpineShoulder;
            case (int)NuiSkeletonPositionIndex.Head:
                return (int)NuiSkeletonPositionIndex.Neck;
            case (int)NuiSkeletonPositionIndex.ShoulderLeft:
                return (int)NuiSkeletonPositionIndex.SpineShoulder;
            case (int)NuiSkeletonPositionIndex.ElbowLeft:
                return (int)NuiSkeletonPositionIndex.ShoulderLeft;
            case (int)NuiSkeletonPositionIndex.WristLeft:
                return (int)NuiSkeletonPositionIndex.ElbowLeft;
            case (int)NuiSkeletonPositionIndex.HandLeft:
                return (int)NuiSkeletonPositionIndex.WristLeft;
            case (int)NuiSkeletonPositionIndex.ShoulderRight:
                return (int)NuiSkeletonPositionIndex.SpineShoulder;
            case (int)NuiSkeletonPositionIndex.ElbowRight:
                return (int)NuiSkeletonPositionIndex.ShoulderRight;
            case (int)NuiSkeletonPositionIndex.WristRight:
                return (int)NuiSkeletonPositionIndex.ElbowRight;
            case (int)NuiSkeletonPositionIndex.HandRight:
                return (int)NuiSkeletonPositionIndex.WristRight;
            case (int)NuiSkeletonPositionIndex.HipLeft:
                return (int)NuiSkeletonPositionIndex.SpineBase;
            case (int)NuiSkeletonPositionIndex.KneeLeft:
                return (int)NuiSkeletonPositionIndex.HipLeft;
            case (int)NuiSkeletonPositionIndex.AnkleLeft:
                return (int)NuiSkeletonPositionIndex.KneeLeft;
            case (int)NuiSkeletonPositionIndex.FootLeft:
                return (int)NuiSkeletonPositionIndex.AnkleLeft;
            case (int)NuiSkeletonPositionIndex.HipRight:
                return (int)NuiSkeletonPositionIndex.SpineBase;
            case (int)NuiSkeletonPositionIndex.KneeRight:
                return (int)NuiSkeletonPositionIndex.HipRight;
            case (int)NuiSkeletonPositionIndex.AnkleRight:
                return (int)NuiSkeletonPositionIndex.KneeRight;
            case (int)NuiSkeletonPositionIndex.FootRight:
                return (int)NuiSkeletonPositionIndex.AnkleRight;
            case (int)NuiSkeletonPositionIndex.SpineShoulder:
                return (int)NuiSkeletonPositionIndex.SpineMid;
            case (int)NuiSkeletonPositionIndex.HandTipLeft:
                return (int)NuiSkeletonPositionIndex.HandLeft;
            case (int)NuiSkeletonPositionIndex.ThumbLeft:
                return (int)NuiSkeletonPositionIndex.HandLeft;
            case (int)NuiSkeletonPositionIndex.HandTipRight:
                return (int)NuiSkeletonPositionIndex.HandRight;
            case (int)NuiSkeletonPositionIndex.ThumbRight:
                return (int)NuiSkeletonPositionIndex.HandRight;
        }

        return (int)NuiSkeletonPositionIndex.SpineBase;
    }

    public static int GetSkeletonMirroredJoint(int jointIndex)
    {
        switch (jointIndex)
        {
            case (int)NuiSkeletonPositionIndex.ShoulderLeft:
                return (int)NuiSkeletonPositionIndex.ShoulderRight;
            case (int)NuiSkeletonPositionIndex.ElbowLeft:
                return (int)NuiSkeletonPositionIndex.ElbowRight;
            case (int)NuiSkeletonPositionIndex.WristLeft:
                return (int)NuiSkeletonPositionIndex.WristRight;
            case (int)NuiSkeletonPositionIndex.HandLeft:
                return (int)NuiSkeletonPositionIndex.HandRight;
            case (int)NuiSkeletonPositionIndex.ShoulderRight:
                return (int)NuiSkeletonPositionIndex.ShoulderLeft;
            case (int)NuiSkeletonPositionIndex.ElbowRight:
                return (int)NuiSkeletonPositionIndex.ElbowLeft;
            case (int)NuiSkeletonPositionIndex.WristRight:
                return (int)NuiSkeletonPositionIndex.WristLeft;
            case (int)NuiSkeletonPositionIndex.HandRight:
                return (int)NuiSkeletonPositionIndex.HandLeft;
            case (int)NuiSkeletonPositionIndex.HipLeft:
                return (int)NuiSkeletonPositionIndex.HipRight;
            case (int)NuiSkeletonPositionIndex.KneeLeft:
                return (int)NuiSkeletonPositionIndex.KneeRight;
            case (int)NuiSkeletonPositionIndex.AnkleLeft:
                return (int)NuiSkeletonPositionIndex.AnkleRight;
            case (int)NuiSkeletonPositionIndex.FootLeft:
                return (int)NuiSkeletonPositionIndex.FootRight;
            case (int)NuiSkeletonPositionIndex.HipRight:
                return (int)NuiSkeletonPositionIndex.HipLeft;
            case (int)NuiSkeletonPositionIndex.KneeRight:
                return (int)NuiSkeletonPositionIndex.KneeLeft;
            case (int)NuiSkeletonPositionIndex.AnkleRight:
                return (int)NuiSkeletonPositionIndex.AnkleLeft;
            case (int)NuiSkeletonPositionIndex.FootRight:
                return (int)NuiSkeletonPositionIndex.FootLeft;
            case (int)NuiSkeletonPositionIndex.HandTipLeft:
                return (int)NuiSkeletonPositionIndex.HandTipRight;
            case (int)NuiSkeletonPositionIndex.ThumbLeft:
                return (int)NuiSkeletonPositionIndex.ThumbRight;
            case (int)NuiSkeletonPositionIndex.HandTipRight:
                return (int)NuiSkeletonPositionIndex.HandTipLeft;
            case (int)NuiSkeletonPositionIndex.ThumbRight:
                return (int)NuiSkeletonPositionIndex.ThumbLeft;
        }

        return jointIndex;
    }

    public static bool PollSkeleton(ref NuiTransformSmoothParameters smoothParameters, ref NuiSkeletonFrame skeletonFrame)
    {
        bool newSkeleton = false;

        uint hr = RemoteKinectWrapper.NuiSkeletonGetNextFrame(0, ref skeletonFrame);
        if (hr == 0)
        {
            newSkeleton = true;
        }

        if (newSkeleton)
        {
            int h = RemoteKinectWrapper.NuiTransformSmooth(ref skeletonFrame, ref smoothParameters);
            if (h != 0)
            {
                Debug.Log("Skeleton Data Smoothing failed");
            }
        }

        return newSkeleton;
    }

    public static bool PollColor(IntPtr colorStreamHandle, ref byte[] videoBuffer, ref Color32[] colorImage)
    {
        return false;
    }

    public static bool PollDepth(IntPtr depthStreamHandle, bool isNearMode, ref ushort[] depthPlayerData)
    {
        return false;
    }

    private static void PopulateSkeletonData(BodyData data, ref NuiSkeletonData skeleton)
    {
        int jointsCount = (int)NuiSkeletonPositionIndex.Count;

        skeleton.eTrackingState = data.isTracked ? NuiSkeletonTrackingState.SkeletonTracked : NuiSkeletonTrackingState.NotTracked;
        skeleton.TrackingId = data.trackingId;
        skeleton.ClippedEdges = (FrameEdges)data.clippedEdges;

        if (skeleton.SkeletonPositions == null)
            skeleton.SkeletonPositions = new Vector4[jointsCount];
        if (skeleton.SkeletonTrackingState == null)
            skeleton.SkeletonTrackingState = new NuiSkeletonPositionTrackingState[jointsCount];
        for (int i = 0; i!= jointsCount; i++)
        {
            data.SetJointPosition(i, ref skeleton.SkeletonPositions[i]);
            data.SetJointTrackingState(i, ref skeleton.SkeletonTrackingState[i]);
        }

        /*
        var i = (int)NuiSkeletonPositionIndex.HandLeft;
        data.JointPosition(i, ref skeleton.SkeletonPositions[i]);
        skeleton.eSkeletonPositionTrackingState[i] = NuiSkeletonPositionTrackingState.Tracked;

        i = (int)NuiSkeletonPositionIndex.HandRight;
        data.JointPosition(i, ref skeleton.SkeletonPositions[i]);
        skeleton.eSkeletonPositionTrackingState[i] = NuiSkeletonPositionTrackingState.Tracked;

        i = (int)NuiSkeletonPositionIndex.Head;
        data.JointPosition(i, ref skeleton.SkeletonPositions[i]);
        skeleton.eSkeletonPositionTrackingState[i] = NuiSkeletonPositionTrackingState.Tracked;

        i = (int)NuiSkeletonPositionIndex.Neck;
        data.JointPosition(i, ref skeleton.SkeletonPositions[i]);
        skeleton.eSkeletonPositionTrackingState[i] = NuiSkeletonPositionTrackingState.Tracked;

        i = (int)NuiSkeletonPositionIndex.ShoulderLeft;
        data.JointPosition(i, ref skeleton.SkeletonPositions[i]);
        skeleton.eSkeletonPositionTrackingState[i] = NuiSkeletonPositionTrackingState.Tracked;

        i = (int)NuiSkeletonPositionIndex.ShoulderRight;
        data.JointPosition(i, ref skeleton.SkeletonPositions[i]);
        skeleton.eSkeletonPositionTrackingState[i] = NuiSkeletonPositionTrackingState.Tracked;

        i = (int)NuiSkeletonPositionIndex.SpineBase;
        data.JointPosition(i, ref skeleton.SkeletonPositions[i]);
        skeleton.eSkeletonPositionTrackingState[i] = NuiSkeletonPositionTrackingState.Tracked;
        */
    }

    private static Vector3 GetPositionBetweenIndices(ref Vector3[] jointsPos, NuiSkeletonPositionIndex p1, NuiSkeletonPositionIndex p2)
    {
        Vector3 pVec1 = jointsPos[(int)p1];
        Vector3 pVec2 = jointsPos[(int)p2];

        return pVec2 - pVec1;
    }

    //populate matrix using the columns
    private static void PopulateMatrix(ref Matrix4x4 jointOrientation, Vector3 xCol, Vector3 yCol, Vector3 zCol)
    {
        jointOrientation.SetColumn(0, xCol);
        jointOrientation.SetColumn(1, yCol);
        jointOrientation.SetColumn(2, zCol);
    }

    //constructs an orientation from a vector that specifies the x axis
    private static void MakeMatrixFromX(Vector3 v1, ref Matrix4x4 jointOrientation, bool flip)
    {
        //matrix columns
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        //set first column to the vector between the previous joint and the current one, this sets the two degrees of freedom
        xCol = v1.normalized;

        //set second column to an arbitrary vector perpendicular to the first column
        yCol.x = 0.0f;
        yCol.y = !flip ? xCol.z : -xCol.z;
        yCol.z = !flip ? -xCol.y : xCol.y;
        yCol.Normalize();

        //third column is fully determined by the first two, and it must be their cross product
        zCol = Vector3.Cross(xCol, yCol);

        //copy values into matrix
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }

    //constructs an orientation from a vector that specifies the y axis
    private static void MakeMatrixFromY(Vector3 v1, ref Matrix4x4 jointOrientation)
    {
        //matrix columns
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        //set first column to the vector between the previous joint and the current one, this sets the two degrees of freedom
        yCol = v1.normalized;

        //set second column to an arbitrary vector perpendicular to the first column
        xCol.x = yCol.y;
        xCol.y = -yCol.x;
        xCol.z = 0.0f;
        xCol.Normalize();

        //third column is fully determined by the first two, and it must be their cross product
        zCol = Vector3.Cross(xCol, yCol);

        //copy values into matrix
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }

    //constructs an orientation from a vector that specifies the x axis
    private static void MakeMatrixFromZ(Vector3 v1, ref Matrix4x4 jointOrientation)
    {
        //matrix columns
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        //set first column to the vector between the previous joint and the current one, this sets the two degrees of freedom
        zCol = v1.normalized;

        //set second column to an arbitrary vector perpendicular to the first column
        xCol.x = zCol.y;
        xCol.y = -zCol.x;
        xCol.z = 0.0f;
        xCol.Normalize();

        //third column is fully determined by the first two, and it must be their cross product
        yCol = Vector3.Cross(zCol, xCol);

        //copy values into matrix
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }

    //constructs an orientation from 2 vectors: the first specifies the x axis, and the next specifies the y axis
    //uses the first vector as x axis, then constructs the other axes using cross products
    private static void MakeMatrixFromXY(Vector3 xUnnormalized, Vector3 yUnnormalized, ref Matrix4x4 jointOrientation)
    {
        //matrix columns
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        //set up the three different columns to be rearranged and flipped
        xCol = xUnnormalized.normalized;
        zCol = Vector3.Cross(xCol, yUnnormalized.normalized).normalized;
        yCol = Vector3.Cross(zCol, xCol).normalized;
        //yCol = yUnnormalized.normalized;
        //zCol = Vector3.Cross(xCol, yCol).normalized;

        //copy values into matrix
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }

    //constructs an orientation from 2 vectors: the first specifies the x axis, and the next specifies the y axis
    //uses the second vector as y axis, then constructs the other axes using cross products
    private static void MakeMatrixFromYX(Vector3 xUnnormalized, Vector3 yUnnormalized, ref Matrix4x4 jointOrientation)
    {
        //matrix columns
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        //set up the three different columns to be rearranged and flipped
        yCol = yUnnormalized.normalized;
        zCol = Vector3.Cross(xUnnormalized.normalized, yCol).normalized;
        xCol = Vector3.Cross(yCol, zCol).normalized;
        //xCol = xUnnormalized.normalized;
        //zCol = Vector3.Cross(xCol, yCol).normalized;

        //copy values into matrix
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }

    //constructs an orientation from 2 vectors: the first specifies the x axis, and the next specifies the y axis
    //uses the second vector as y axis, then constructs the other axes using cross products
    private static void MakeMatrixFromYZ(Vector3 yUnnormalized, Vector3 zUnnormalized, ref Matrix4x4 jointOrientation)
    {
        //matrix columns
        Vector3 xCol;
        Vector3 yCol;
        Vector3 zCol;

        //set up the three different columns to be rearranged and flipped
        yCol = yUnnormalized.normalized;
        xCol = Vector3.Cross(yCol, zUnnormalized.normalized).normalized;
        zCol = Vector3.Cross(xCol, yCol).normalized;
        //zCol = zUnnormalized.normalized;
        //xCol = Vector3.Cross(yCol, zCol).normalized;

        //copy values into matrix
        PopulateMatrix(ref jointOrientation, xCol, yCol, zCol);
    }

    // calculate the joint orientations, based on joint positions and their tracked state
    public static void GetSkeletonJointOrientation(ref Vector3[] jointsPos, ref bool[] jointsTracked, ref Matrix4x4[] jointOrients)
    {
        Vector3 vx;
        Vector3 vy;
        Vector3 vz;

        // NUI_SKELETON_POSITION_HIP_CENTER
        if (jointsTracked[(int)NuiSkeletonPositionIndex.SpineBase] && jointsTracked[(int)NuiSkeletonPositionIndex.SpineMid] &&
            jointsTracked[(int)NuiSkeletonPositionIndex.HipLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.HipRight])
        {
            vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.SpineBase, NuiSkeletonPositionIndex.SpineMid);
            vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
            MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.SpineBase]);

            // make a correction of about 40 degrees back to the front
            Matrix4x4 mat = jointOrients[(int)NuiSkeletonPositionIndex.SpineBase];
            Quaternion quat = Quaternion.LookRotation(mat.GetColumn(2), mat.GetColumn(1));
            quat *= Quaternion.Euler(-40, 0, 0);
            jointOrients[(int)NuiSkeletonPositionIndex.SpineBase].SetTRS(Vector3.zero, quat, Vector3.one);
        }

        if (jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderRight])
        {
            // NUI_SKELETON_POSITION_SPINE
            if (jointsTracked[(int)NuiSkeletonPositionIndex.SpineMid] && jointsTracked[(int)NuiSkeletonPositionIndex.SpineShoulder])
            {
                vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.SpineMid, NuiSkeletonPositionIndex.SpineShoulder);
                vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderLeft, NuiSkeletonPositionIndex.ShoulderRight);
                MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.SpineMid]);
            }

            if (jointsTracked[(int)NuiSkeletonPositionIndex.SpineShoulder] && jointsTracked[(int)NuiSkeletonPositionIndex.Head])
            {
                // NUI_SKELETON_POSITION_SHOULDER_CENTER
                //if(jointsTracked[(int)NuiSkeletonPositionIndex.SpineShoulder] && jointsTracked[(int)NuiSkeletonPositionIndex.Head])
                //{
                vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.SpineShoulder, NuiSkeletonPositionIndex.Head);
                vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderLeft, NuiSkeletonPositionIndex.ShoulderRight);
                MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.SpineShoulder]);
                //}

                // NUI_SKELETON_POSITION_HEAD
                //if(jointsTracked[(int)NuiSkeletonPositionIndex.SpineShoulder] && jointsTracked[(int)NuiSkeletonPositionIndex.Head])
                //{
                //			        vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.SpineShoulder, NuiSkeletonPositionIndex.Head);
                //			        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderLeft, NuiSkeletonPositionIndex.ShoulderRight);
                MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.Head]);
                //MakeMatrixFromY(vy, ref jointOrients[(int)NuiSkeletonPositionIndex.Head]);
                //}
            }
        }

        if (jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.ElbowLeft] &&
            //jointsTracked[(int)NuiSkeletonPositionIndex.WristLeft])
            jointsTracked[(int)NuiSkeletonPositionIndex.SpineMid] && jointsTracked[(int)NuiSkeletonPositionIndex.SpineShoulder])
        {
            // NUI_SKELETON_POSITION_SHOULDER_LEFT
            //if(jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.ElbowLeft])
            {
                vx = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderLeft, NuiSkeletonPositionIndex.ElbowLeft);
                //vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ElbowLeft, NuiSkeletonPositionIndex.WristLeft);
                vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.SpineMid, NuiSkeletonPositionIndex.SpineShoulder);
                MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ShoulderLeft]);
            }

            // NUI_SKELETON_POSITION_ELBOW_LEFT
            //if(jointsTracked[(int)NuiSkeletonPositionIndex.ElbowLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.WristLeft])
            {
                vx = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ElbowLeft, NuiSkeletonPositionIndex.WristLeft);
                //vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderLeft, NuiSkeletonPositionIndex.ElbowLeft);
                vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.SpineMid, NuiSkeletonPositionIndex.SpineShoulder);
                MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ElbowLeft]);
            }
        }

        if (jointsTracked[(int)NuiSkeletonPositionIndex.WristLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.HandLeft] &&
         jointsTracked[(int)NuiSkeletonPositionIndex.SpineMid] && jointsTracked[(int)NuiSkeletonPositionIndex.SpineShoulder])
        {
            // NUI_SKELETON_POSITION_WRIST_LEFT
            //if(jointsTracked[(int)NuiSkeletonPositionIndex.WristLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.HandLeft])
            //{
            vx = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.WristLeft, NuiSkeletonPositionIndex.HandLeft);
            //MakeMatrixFromX(vx, ref jointOrients[(int)NuiSkeletonPositionIndex.WristLeft], false);
            vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.SpineMid, NuiSkeletonPositionIndex.SpineShoulder);
            MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.WristLeft]);
            //}

            // NUI_SKELETON_POSITION_HAND_LEFT:
            //if(jointsTracked[(int)NuiSkeletonPositionIndex.WristLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.HandLeft])
            //{
            //		        vx = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.WristLeft, NuiSkeletonPositionIndex.HandLeft);
            //		        //MakeMatrixFromX(vx, ref jointOrients[(int)NuiSkeletonPositionIndex.HandLeft], false);
            //				vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.Spine, NuiSkeletonPositionIndex.SpineShoulder);
            MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.HandLeft]);
            //}
        }

        if (jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderRight] && jointsTracked[(int)NuiSkeletonPositionIndex.ElbowRight] &&
            //jointsTracked[(int)NuiSkeletonPositionIndex.WristRight])
            jointsTracked[(int)NuiSkeletonPositionIndex.SpineMid] && jointsTracked[(int)NuiSkeletonPositionIndex.SpineShoulder])
        {
            // NUI_SKELETON_POSITION_SHOULDER_RIGHT
            //if(jointsTracked[(int)NuiSkeletonPositionIndex.ShoulderRight] && jointsTracked[(int)NuiSkeletonPositionIndex.ElbowRight])
            {
                vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderRight, NuiSkeletonPositionIndex.ElbowRight);
                //vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ElbowRight, NuiSkeletonPositionIndex.WristRight);
                vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.SpineMid, NuiSkeletonPositionIndex.SpineShoulder);
                MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ShoulderRight]);
            }

            // NUI_SKELETON_POSITION_ELBOW_RIGHT
            //if(jointsTracked[(int)NuiSkeletonPositionIndex.ElbowRight] && jointsTracked[(int)NuiSkeletonPositionIndex.WristRight])
            {
                vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ElbowRight, NuiSkeletonPositionIndex.WristRight);
                //vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.ShoulderRight, NuiSkeletonPositionIndex.ElbowRight);
                vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.SpineMid, NuiSkeletonPositionIndex.SpineShoulder);
                MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.ElbowRight]);
            }
        }

        if (jointsTracked[(int)NuiSkeletonPositionIndex.WristRight] && jointsTracked[(int)NuiSkeletonPositionIndex.HandRight] &&
            jointsTracked[(int)NuiSkeletonPositionIndex.SpineMid] && jointsTracked[(int)NuiSkeletonPositionIndex.SpineShoulder])
        {
            // NUI_SKELETON_POSITION_WRIST_RIGHT
            //if(jointsTracked[(int)NuiSkeletonPositionIndex.WristRight] && jointsTracked[(int)NuiSkeletonPositionIndex.HandRight])
            //{
            vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.WristRight, NuiSkeletonPositionIndex.HandRight);
            //MakeMatrixFromX(vx, ref jointOrients[(int)NuiSkeletonPositionIndex.WristRight], true);
            vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.SpineMid, NuiSkeletonPositionIndex.SpineShoulder);
            MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.WristRight]);
            //}

            // NUI_SKELETON_POSITION_HAND_RIGHT
            //if(jointsTracked[(int)NuiSkeletonPositionIndex.WristRight] && jointsTracked[(int)NuiSkeletonPositionIndex.HandRight])
            //{
            //		        vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.WristRight, NuiSkeletonPositionIndex.HandRight);
            //		        //MakeMatrixFromX(vx, ref jointOrients[(int)NuiSkeletonPositionIndex.HandRight], true);
            //				vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.Spine, NuiSkeletonPositionIndex.SpineShoulder);
            MakeMatrixFromXY(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.HandRight]);
            //}
        }

        // NUI_SKELETON_POSITION_HIP_LEFT
        if (jointsTracked[(int)NuiSkeletonPositionIndex.HipLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.KneeLeft] &&
            jointsTracked[(int)NuiSkeletonPositionIndex.HipRight])
        {
            vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeLeft, NuiSkeletonPositionIndex.HipLeft);
            vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
            MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.HipLeft]);

            // NUI_SKELETON_POSITION_KNEE_LEFT
            if (jointsTracked[(int)NuiSkeletonPositionIndex.AnkleLeft])
            {
                vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeLeft, NuiSkeletonPositionIndex.AnkleLeft);
                //vz = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.AnkleLeft, NuiSkeletonPositionIndex.FootLeft);
                //MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.KneeLeft]);
                vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
                MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.KneeLeft]);
            }
        }

        if (jointsTracked[(int)NuiSkeletonPositionIndex.KneeLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.AnkleLeft] &&
            jointsTracked[(int)NuiSkeletonPositionIndex.FootLeft])
        {
            // NUI_SKELETON_POSITION_ANKLE_LEFT
            //if(jointsTracked[(int)NuiSkeletonPositionIndex.AnkleLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.FootLeft])
            //{
            vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeLeft, NuiSkeletonPositionIndex.AnkleLeft);
            vz = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.FootLeft, NuiSkeletonPositionIndex.AnkleLeft);
            MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.AnkleLeft]);
            //MakeMatrixFromZ(vz, ref jointOrients[(int)NuiSkeletonPositionIndex.AnkleLeft]);
            //}

            // NUI_SKELETON_POSITION_FOOT_LEFT
            //if(jointsTracked[(int)NuiSkeletonPositionIndex.AnkleLeft] && jointsTracked[(int)NuiSkeletonPositionIndex.FootLeft])
            //{
            //		        vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeLeft, NuiSkeletonPositionIndex.AnkleLeft);
            //		        vz = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.FootLeft, NuiSkeletonPositionIndex.AnkleLeft);
            MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.FootLeft]);
            //MakeMatrixFromZ(vz, ref jointOrients[(int)NuiSkeletonPositionIndex.FootLeft]);
            //}
        }

        // NUI_SKELETON_POSITION_HIP_RIGHT
        if (jointsTracked[(int)NuiSkeletonPositionIndex.HipRight] && jointsTracked[(int)NuiSkeletonPositionIndex.KneeRight] &&
            jointsTracked[(int)NuiSkeletonPositionIndex.HipLeft])
        {
            vy = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeRight, NuiSkeletonPositionIndex.HipRight);
            vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
            MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.HipRight]);

            // NUI_SKELETON_POSITION_KNEE_RIGHT
            if (jointsTracked[(int)NuiSkeletonPositionIndex.AnkleRight])
            {
                vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeRight, NuiSkeletonPositionIndex.AnkleRight);
                //vz = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.AnkleRight, NuiSkeletonPositionIndex.FootRight);
                //MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.KneeRight]);
                vx = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.HipLeft, NuiSkeletonPositionIndex.HipRight);
                MakeMatrixFromYX(vx, vy, ref jointOrients[(int)NuiSkeletonPositionIndex.KneeRight]);
            }
        }

        if (jointsTracked[(int)NuiSkeletonPositionIndex.KneeRight] && jointsTracked[(int)NuiSkeletonPositionIndex.AnkleRight] &&
            jointsTracked[(int)NuiSkeletonPositionIndex.FootRight])
        {
            // NUI_SKELETON_POSITION_ANKLE_RIGHT
            //if(jointsTracked[(int)NuiSkeletonPositionIndex.AnkleRight] && jointsTracked[(int)NuiSkeletonPositionIndex.FootRight])
            //{
            vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeRight, NuiSkeletonPositionIndex.AnkleRight);
            vz = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.FootRight, NuiSkeletonPositionIndex.AnkleRight);
            MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.AnkleRight]);
            //MakeMatrixFromZ(vz, ref jointOrients[(int)NuiSkeletonPositionIndex.AnkleRight]);
            //}

            // NUI_SKELETON_POSITION_FOOT_RIGHT
            //if(jointsTracked[(int)NuiSkeletonPositionIndex.AnkleRight] && jointsTracked[(int)NuiSkeletonPositionIndex.FootRight])
            //{
            //		        vy = -GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.KneeRight, NuiSkeletonPositionIndex.AnkleRight);
            //		        vz = GetPositionBetweenIndices(ref jointsPos, NuiSkeletonPositionIndex.FootRight, NuiSkeletonPositionIndex.AnkleRight);
            MakeMatrixFromYZ(vy, vz, ref jointOrients[(int)NuiSkeletonPositionIndex.FootRight]);
            //MakeMatrixFromZ(vz, ref jointOrients[(int)NuiSkeletonPositionIndex.FootRight]);
            //}
        }
    }
}