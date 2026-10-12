using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
[InitializeOnLoad]
public static class MailInspect
{
 static MailInspect(){ EditorApplication.delayCall+=Dump; EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode) EditorApplication.delayCall+=Dump;}; }
 static void Dump(){
 var sb=new StringBuilder();
 foreach(var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
 foreach(var t in root.GetComponentsInChildren<Transform>(true)) {
 if(t.name.Contains("Mail") || t.name=="InboxRoot" || t.name=="Left_Grid" || t.name=="Btn_Email"){
 sb.AppendLine(t.name+" active="+t.gameObject.activeSelf+" hierarchy="+t.gameObject.activeInHierarchy+" parent="+t.parent?.name);
 foreach(var c in t.GetComponents<Component>()) if(c) sb.AppendLine(EditorJsonUtility.ToJson(c));
 }
 }
 File.WriteAllText("Temp/mail-inspection.txt",sb.ToString());
 }
}
