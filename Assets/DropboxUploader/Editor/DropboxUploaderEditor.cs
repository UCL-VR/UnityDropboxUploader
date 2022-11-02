using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Net.Http;

[CustomEditor(typeof(DropboxUploader))]
public class DropboxUploaderEditor : Editor
{
    private enum State
    {
        NeedCredentials,
        NeedCode,
        RequestCode,
        Authorised
    }

    private State state;
    private string code;

    public override void OnInspectorGUI()
    {
       // base.OnInspectorGUI();

        var component = target as DropboxUploader;

        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("AppKey"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("AppSecret"));

        serializedObject.ApplyModifiedProperties();

        if(string.IsNullOrEmpty(component.AppKey) || string.IsNullOrEmpty(component.AppSecret))
        {
            state = State.NeedCredentials;
        }
        else if(state == State.RequestCode)
        {
            if(!string.IsNullOrEmpty(component.RefreshToken))
            {
                state = State.Authorised;
            }
        }
        else if(string.IsNullOrEmpty(component.RefreshToken))
        {
            state = State.NeedCode;
        }
        else
        {
            state = State.Authorised;
        }


        switch (state)
        {
            case State.NeedCredentials:
                EditorGUILayout.HelpBox("Enter your App Key and Secret to set up DropboxUploader", MessageType.Warning);
                if(GUILayout.Button("Open App Console"))
                {
                    component.OpenAppconsole();
                }
                break;
            case State.NeedCode:
                EditorGUILayout.HelpBox("Click Authorise then enter the Access Code", MessageType.Warning);
                goto case State.Authorised;
            case State.Authorised:
                EditorGUILayout.HelpBox("DropboxUploader is Ready", MessageType.Info);
                if (GUILayout.Button("Authorise"))
                {
                    component.GetAuthorisiationCode();
                    state = State.RequestCode;
                }
                break;
            case State.RequestCode:
                code = EditorGUILayout.TextField("Code", code);
                if(GUILayout.Button("Accept"))
                {
                    component.GetRefreshToken(code);
                }
                break;
        }

        if (GUILayout.Button("Upload"))
        {
            component.Upload();
        }
    }

}
