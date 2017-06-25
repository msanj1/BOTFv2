
FB.init({ appId: "413005845422605", status: true, cookie: true });
function sendRequestInvite() {
  
  
    GetFriendsNotUsingApp();
    
//     console.log('Got friends that are not using the app yet: ', nonAppFriendIDs);
//  
}


function GetFriendsNotUsingApp() {
    var nonAppFriendIDs = [];
  //First, get friends that are using the app
    FB.api({ method: 'friends.getAppUsers' }, function (appFriendResponse) {
        var appFriendIDs = appFriendResponse; //users using the app
        console.log("response:" + appFriendResponse);
        //Now fetch all of the user's friends so that we can determine who hasn't used the app yet
        FB.api('/me/friends', { fields: 'id, name, picture' }, function (friendResponse) {
            var friends = friendResponse.data; //all my friends

            //limit to a 200 friends so it's fast
            for (var k = 0; k < friends.length && k < 200; k++) {
                var friend = friends[k];
                var index = 1;



                for (var i = 0; i < appFriendIDs.length; i++) {
                    if (appFriendIDs[i] == friend.id) {
                        index = -1;
                    }
                }

                if (index == 1) {
                    nonAppFriendIDs.push(friend.id);
                }
            }
           
            FB.ui({
                method: 'apprequests',
                suggestions: nonAppFriendIDs,
                message: 'Learn how to make your mobile web app social',
              }, function(response) {
                console.log('sendRequestInvite UI response: ', response);
              });



        });
    });
}


