using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[ExecuteInEditMode]
public class DropboxUploader : MonoBehaviour
{
    public string AppKey;
    public string AppSecret;
    public string RefreshToken;

    private string accessToken;
    private float accessTokenExpriationTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenAppconsole()
    {
        var url = $"https://www.dropbox.com/developers/apps";
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = url, UseShellExecute = true });
    }

    public void GetAuthorisiationCode()
    {
        var url = $"https://www.dropbox.com/oauth2/authorize?client_id={AppKey}&token_access_type=offline&response_type=code";
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = url, UseShellExecute = true });
    }

    private struct TokenResponse
    {
        public string access_token;
        public string token_type;
        public int expires_in;
        public string refresh_token;
        public string scope;
        public string uid;
        public string account_id;
    }

    private struct Token
    {
        public string access_token;
        public int expires_in;
        public string token_type;
    }

    private struct UploadArgs
    {
        public string path;
        public string mode;
        public bool autorename;
        public bool mute;
    }


    public void GetRefreshToken(string code)
    {
        StartCoroutine(GetRefreshTokenCoroutine(code));
    }

    private IEnumerator GetRefreshTokenCoroutine(string code)
    {
        var parms = new Dictionary<string, string>();
        parms["client_id"] = AppKey;
        parms["client_secret"] = AppSecret;
        parms["code"] = code;
        parms["grant_type"] = "authorization_code";
        var request = UnityWebRequest.Post($"https://api.dropboxapi.com/oauth2/token", parms);

        yield return request.SendWebRequest();

        switch (request.result)
        {
            case UnityWebRequest.Result.Success:
                {
                    var response = JsonUtility.FromJson<TokenResponse>(request.downloadHandler.text);
                    RefreshToken = response.refresh_token;
                }
                break;
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.ProtocolError:
            case UnityWebRequest.Result.DataProcessingError:
                {
                    Debug.LogError(request.error);
                }
                break;
        }

        Debug.Log(request.result);

        request.Dispose();
    }

    private IEnumerator UpdateAccessToken()
    {
        var parms = new Dictionary<string, string>();
        parms["client_id"] = AppKey;
        parms["client_secret"] = AppSecret;
        parms["refresh_token"] = RefreshToken;
        parms["grant_type"] = "refresh_token";

        var request = UnityWebRequest.Post($"https://api.dropboxapi.com/oauth2/token", parms);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            request.Dispose();
            yield break;
        }

        var token = JsonUtility.FromJson<Token>(request.downloadHandler.text);

        accessToken = token.access_token;
        accessTokenExpriationTime = Time.realtimeSinceStartup + token.expires_in;

        request.Dispose();
    }

    public IEnumerator UploadCoroutine(string filename, byte[] data)
    {
        // Get the access token using our permanent refresh token, if necessary

        if (string.IsNullOrEmpty(accessToken) || Time.realtimeSinceStartup > accessTokenExpriationTime)
        {
            yield return UpdateAccessToken();
        }

        var parms = new Dictionary<string, string>();
        parms["client_id"] = AppKey;
        parms["client_secret"] = AppSecret;
        parms["refresh_token"] = RefreshToken;
        parms["grant_type"] = "refresh_token";

        var request = UnityWebRequest.Post($"https://content.dropboxapi.com/2/files/upload", parms);

        var args = new UploadArgs();
        args.autorename = true;
        args.mode = "add";
        args.mute = false;
        args.path = $"/{filename}";

        request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        request.SetRequestHeader("Dropbox-API-Arg", JsonUtility.ToJson(args));
        request.SetRequestHeader("Content-Type", "application/octet-stream");

        request.uploadHandler = new UploadHandlerRaw(data);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            if(request.downloadHandler != null)
            {
                Debug.LogError(request.downloadHandler.text);
            }
            request.Dispose();
            yield break;
        }

        Debug.Log(request.downloadHandler.text);

        request.Dispose();
    }

    public void Upload()
    {
        var data = System.Text.Encoding.UTF8.GetBytes("Hello");

        StartCoroutine(UploadCoroutine(System.Guid.NewGuid().ToString()+".txt", data));
    }


}
