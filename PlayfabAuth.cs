using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class PlayfabAuth : MonoBehaviour
{
    LoginWithPlayFabRequest logInRequest;
    LogInUI logInUI;
    public bool IsAuthenticated = false;
    string name;

    RegisterPlayFabUserRequest registerRequest;
    public static Action<string> errorToLogIn;
    public static Action<string> errorToRegister;



    private void Start()
    {
        logInUI = FindObjectOfType<LogInUI>();
    }

    public void LogIn()
    {
        logInRequest = new LoginWithPlayFabRequest();
        logInRequest.InfoRequestParameters = new GetPlayerCombinedInfoRequestParams()
        {
            GetPlayerProfile = true,
        };
        logInRequest.Username = logInUI.LogInUserName.text;
        logInRequest.Password = logInUI.LogInPassword.text;
        PlayFabClientAPI.LoginWithPlayFab(logInRequest, OnSuccess, OnFailure);
    }

    public void Register()
    {
        RegisterPlayFabUserRequest registerRequest = new RegisterPlayFabUserRequest();
        registerRequest.Email = logInUI.RegisterEmail.text;
        registerRequest.Username = logInUI.RegisterName.text;
        registerRequest.Password = logInUI.RegisterPassword.text;
        name = logInUI.RegisterName.text;
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnSuccessRegister, OnFailureRegister);

    }

    private void OnFailureRegister(PlayFabError error)
    {
        string report=error.GenerateErrorReport();
        Debug.Log(report);
        string message= error.ErrorMessage;
        if (report.Contains("Password"))
        {
            message = "Password must be between 6 and 100 characters.";

        }
        else if (report.Contains("Email"))
        {
            message = "Email address is not valid.";
        }
        else if (report.Contains("available"))
        {
            message = "Username not available.";
        }
        else if (report.Contains("Username"))
        {
            message = "Username must be between 3 and 20 characters.";
        }
        errorToRegister.Invoke(message);
    }

    private void OnSuccessRegister(RegisterPlayFabUserResult result)
    {
        errorToRegister.Invoke("Your account has been created. Log In.");
        UpdateDisplayName();
    }

    private void OnFailure(PlayFabError error)
    {
        IsAuthenticated = false;
        errorToLogIn.Invoke(error.GenerateErrorReport());

    }

    private void OnSuccess(LoginResult result)
    {
        PhotonNetwork.NickName = result.InfoResultPayload.PlayerProfile.DisplayName;
        logInUI.DisactiveAllForms();
    }

    public void UpdateDisplayName()
    {
        var request = new UpdateUserTitleDisplayNameRequest();
        request.DisplayName = name;
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnSuccessUpdate, OnFailureUpdate);
    }

    private void OnFailureUpdate(PlayFabError obj)
    {
        Debug.Log("failure");
    }

    private void OnSuccessUpdate(UpdateUserTitleDisplayNameResult obj)
    {
        Debug.Log("Success");
    }

}
