using System;
using System.IO;
using UnityEditor;
using UnityEngine;

class ProjectSnapshot
{
	[MenuItem("Tools/Take Snapshot %1")]
	public static void TakeSnapshot()
	{
		string snapshot_path = "snapshot_" + DateTime.Now.ToString("dd-MM-yy-hh-mm-ss") + ".png";
		ScreenCapture.CaptureScreenshot(Path.Combine("Assets", snapshot_path));

		Debug.Log("<b>[ProjectSnapshot] :</b> Snapshot has been taken and stored in " + Path.Combine(Application.dataPath, snapshot_path));
	}
}