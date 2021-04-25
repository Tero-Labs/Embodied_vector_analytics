using UnityEngine;
using System.Collections;

public class RefreshAndroidGallery : MonoBehaviour {

		void RefreshGallery(string path)
		{
				AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");

				AndroidJavaClass javaClass = new AndroidJavaClass("com.example.refresgallery.RefreshFromUnity");
				javaClass.CallStatic("RefreshGallery",jo,path);
		}
}
