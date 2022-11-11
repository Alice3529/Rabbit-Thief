using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class PlayfabFriendController : MonoBehaviour
{
    public static Action<List<FriendInfo>> OnFriendListUpdated;
    private void Awake()
    {
        UIAddFriend.OnAddFriend += AddFriendHandler;
    }

    void AddFriendHandler(string friendName)
    {

        var request = new AddFriendRequest { FriendTitleDisplayName = friendName };
        PlayFabClientAPI.AddFriend(request, OnSuccessed, OnFailed);
    }

    private void OnFailed(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }

    private void OnSuccessed(AddFriendResult result)
    {
        GetPlayfabFriends();
    }

    private void GetPlayfabFriends()
    {
        var request = new GetFriendsListRequest { IncludeSteamFriends = false, IncludeFacebookFriends = false, XboxToken = null };
    }

    private void OnFriendsListSuccess(GetFriendsListResult result)
    {
        OnFriendListUpdated?.Invoke(result.Friends);
    }
}
