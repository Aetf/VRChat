using UnityEngine;
using System.Collections;
using System;

public class SimpleGestureListener : MonoBehaviour, KinectGestures.GestureListenerInterface
{
	// GUI Text to display the gesture messages.
	public GUIText GestureInfo;
	
	// private bool to track if progress message has been displayed
	private bool progressDisplayed;
	
	
	public void UserDetected(ulong userId, int userIndex)
	{
		// as an example - detect these user specific gestures
		KinectManager manager = KinectManager.Instance;

		manager.DetectGesture(userId, KinectGestures.Gestures.Jump);
		manager.DetectGesture(userId, KinectGestures.Gestures.Squat);

//		manager.DetectGesture(userId, KinectGestures.Gestures.Push);
//		manager.DetectGesture(userId, KinectGestures.Gestures.Pull);
		
//		manager.DetectGesture(userId, KinectCommon.Gestures.SwipeUp);
//		manager.DetectGesture(userId, KinectCommon.Gestures.SwipeDown);
		
		if(GestureInfo != null)
		{
			GestureInfo.GetComponent<GUIText>().text = "SwipeLeft, SwipeRight, Jump or Squat.";
		}
	}
	
	public void UserLost(ulong userId, int userIndex)
	{
		if(GestureInfo != null)
		{
			GestureInfo.GetComponent<GUIText>().text = string.Empty;
		}
	}

	public void GestureInProgress(ulong userId, int userIndex, KinectGestures.Gestures gesture, 
	                              float progress, KinectCommon.NuiSkeletonPositionIndex joint, Vector3 screenPos)
	{
		//GestureInfo.guiText.text = string.Format("{0} Progress: {1:F1}%", gesture, (progress * 100));
		if(gesture == KinectGestures.Gestures.Click && progress > 0.3f)
		{
			string sGestureText = string.Format ("{0} {1:F1}% complete", gesture, progress * 100);
			if(GestureInfo != null)
				GestureInfo.GetComponent<GUIText>().text = sGestureText;
			
			progressDisplayed = true;
		}		
		else if((gesture == KinectGestures.Gestures.ZoomOut || gesture == KinectGestures.Gestures.ZoomIn) && progress > 0.5f)
		{
			string sGestureText = string.Format ("{0} detected, zoom={1:F1}%", gesture, screenPos.z * 100);
			if(GestureInfo != null)
				GestureInfo.GetComponent<GUIText>().text = sGestureText;
			
			progressDisplayed = true;
		}
		else if(gesture == KinectGestures.Gestures.Wheel && progress > 0.5f)
		{
			string sGestureText = string.Format ("{0} detected, angle={1:F1} deg", gesture, screenPos.z);
			if(GestureInfo != null)
				GestureInfo.GetComponent<GUIText>().text = sGestureText;
			
			progressDisplayed = true;
		}
	}

	public bool GestureCompleted (ulong userId, int userIndex, KinectGestures.Gestures gesture, 
	                              KinectCommon.NuiSkeletonPositionIndex joint, Vector3 screenPos)
	{
		string sGestureText = gesture + " detected";
		if(gesture == KinectGestures.Gestures.Click)
			sGestureText += string.Format(" at ({0:F1}, {1:F1})", screenPos.x, screenPos.y);
		
		if(GestureInfo != null)
			GestureInfo.GetComponent<GUIText>().text = sGestureText;
		
		progressDisplayed = false;
		
		return true;
	}

	public bool GestureCancelled (ulong userId, int userIndex, KinectGestures.Gestures gesture, 
	                              KinectCommon.NuiSkeletonPositionIndex joint)
	{
		if(progressDisplayed)
		{
			// clear the progress info
			if(GestureInfo != null)
				GestureInfo.GetComponent<GUIText>().text = String.Empty;
			
			progressDisplayed = false;
		}
		
		return true;
	}
	
}
