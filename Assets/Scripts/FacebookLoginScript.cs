using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System;
public class FacebookLoginScript : MonoBehaviour
{
    protected string lastResponse = "";
    public GameObject playerName, currentChips,  playerPictureIdentifier;
    //    public Texture newTexture;
    void Start()
    {
        CallFBInit();

    }
    private void CallFBInit()
    {
        FB.Init(OnInitComplete, OnHideUnity);
    }

    private void OnInitComplete()
    {
        Debug.Log("FB.Init completed: Is user logged in? " + FB.IsLoggedIn);
        // check if user is logged in if user is not logged in call method to login in facebook acount
        if (!FB.IsLoggedIn)
        {
            CallFBLogin();
        }
        else
            // if user is logged in call to get player name
            callApiToGetName();
    }
    private void OnHideUnity(bool isGameShown)
    {
        Debug.Log("Is game showing? " + isGameShown);
    }

    private void CallFBLogin()
    {
        // do not add publish_action if your app do not have publish_action permission(see status and review of your facebook app for more information)
        FB.Login("public_profile,publish_actions,user_friends",
        LoginCallback);
    }

    // callback on facebook login
    void LoginCallback(FBResult result)
    {
        if (result.Error != null)
            lastResponse = "Error Response:\n" + result.Error;
        else if (!FB.IsLoggedIn)
            lastResponse = "Login cancelled by Player";
        else
        {
            lastResponse = "Login was successful!";

            // on login success call method to get name of player
            callApiToGetName();
        }
    }

    private void callApiToGetName()
    {
        FB.API("me?fields=name", Facebook.HttpMethod.GET, displayReceivedName);
    }

    // callback on received name

    void displayReceivedName(FBResult result)
    {
        Debug.Log("name responce received " + result.Text);
        // retrieve data from received response in result
        IDictionary dict = Facebook.MiniJSON.Json.Deserialize(result.Text) as IDictionary;
        string fbname = dict["name"].ToString();
        playerName.GetComponent<TextMesh>().text = "" + fbname;

        // call method to retrieve profile picture
        StartCoroutine(getProfilePicture());
    }


    public IEnumerator getProfilePicture()
    {
        string stringToDownload;

        // string to receive picture
        stringToDownload = "https" + "://graph.facebook.com/" + FB.UserId + "/picture?type=square";

        WWW url = null;
        url = new WWW(stringToDownload);
        yield return url;
        Texture2D textFb2 = new Texture2D(url.texture.width, url.texture.height, TextureFormat.ARGB32, false);

        // create sprite from received texture
        Sprite sprite = Sprite.Create(url.texture, new Rect(0, 0, url.texture.width, url.texture.height), new Vector2(0.5f, 0.5f), 32);

        playerPictureIdentifier.GetComponent<SpriteRenderer>().sprite = sprite;

        //call method to get score
        getScore();

    }


    public void getScore()
    {
        FB.API("/me/scores", Facebook.HttpMethod.GET, displayStoredScore);
    }

    // callback on score request
    private void displayStoredScore(FBResult r)
    {

        Debug.Log("displayStoredScore: " + r.Text);
        // response received : displayStoredScore: {"data":[{"user":{"id":"401165980041224","name":"Guruz Appz"},"score":2000,"application":{"name":"test","id":"150178115343****"}}]}

        // fetch data from received data
        var dict = Facebook.MiniJSON.Json.Deserialize(r.Text) as IDictionary;

        // retrieve data from received response in result
        System.Collections.Generic.List<System.Object> fbDataResponse_List = (System.Collections.Generic.List<System.Object>)dict["data"];
        System.Collections.Generic.Dictionary<String, System.Object> userDataDictionary = (Dictionary<string, System.Object>)fbDataResponse_List[0];
        if (userDataDictionary.ContainsKey("score"))
        {
            int score = int.Parse("" + userDataDictionary["score"].ToString());
            currentChips.GetComponent<TextMesh>().text = "" + score;
        }
        else
        {
            //if score field is not there score will be posted for     zero
            PostScore(0);
        }
    }

    private void CallFBLogout()
    {
        FB.Logout();
    }

    //method to post score
    public void PostScore(float value)
    {
        Debug.Log("posting score " + value);
        var query = new Dictionary<string, string>();
        query["score"] = "" + value;
        FB.API("/me/scores", Facebook.HttpMethod.POST, PostCallBack, query);
    }

    private void PostCallBack(FBResult result)
    {
        Debug.Log("callback for postcallback " + result.Error);
        getScore();
    }
}


