using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using agora_gaming_rtc;

using agora_gaming_rtc;

public class HelloUnity3D : MonoBehaviour
{
	public InputField mChannelNameInputField;
	public Text mShownMessage;

	private static IRtcEngine mRtcEngine = null;

	private static bool mInitialized = false;

	// PLEASE KEEP THIS App ID IN SAFE PLACE
	// Get your own App ID at https://dashboard.agora.io/
	// After you entered the App ID, remove ## outside of Your App ID
	private static string appId = "8678fa1b05fb49979ec1641f4edc3ac9";

	private static string appKey = "82hegw5u8y3dx";
	private static string appSecret = "JRZTss91O9l";
	private static UserInfo userInfo;
	private static int messageId;

	void Awake ()
	{
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 30;
	}

	// Use this for initialization
	void Start ()
	{
		if (!mInitialized) {
			mInitialized = true;
			isRecord = false;

			mRtcEngine = IRtcEngine.GetEngine (appId);

			mRtcEngine.OnJoinChannelSuccess += (string channelName, uint uid, int elapsed) => {
				string joinSuccessMessage = string.Format ("joinChannel callback uid: {0}, channel: {1}, version: {2}", uid, channelName, IRtcEngine.GetSdkVersion ());
				Debug.Log (joinSuccessMessage);

				mShownMessage.GetComponent<Text> ().text = (joinSuccessMessage);
			};

			mRtcEngine.OnLeaveChannel += (RtcStats stats) => {
				string leaveChannelMessage = string.Format ("onLeaveChannel callback duration {0}, tx: {1}, rx: {2}, tx kbps: {3}, rx kbps: {4}", stats.duration, stats.txBytes, stats.rxBytes, stats.txKBitRate, stats.rxKBitRate);
				Debug.Log (leaveChannelMessage);
				mShownMessage.GetComponent<Text> ().text = (leaveChannelMessage);
			};

			mRtcEngine.OnUserJoined += (uint uid, int elapsed) => {
				string userJoinedMessage = string.Format ("onUserJoined callback uid {0} {1}", uid, elapsed);
				//int a = mRtcEngine.SetRemoteVoicePosition (uid, -1 ,50.0);
				//Debug.Log ("OnUserJoined  setRemoteVoicePosition a = "+ a);
				AudioEffectManagerImpl audio = new AudioEffectManagerImpl (mRtcEngine);
				int a = audio.SetRemoteVoicePosition(uid, -1 ,0);
				Debug.Log ("OnUserJoined  setRemoteVoicePosition = "+ a);
				Debug.Log (userJoinedMessage);
			};

			mRtcEngine.OnUserOffline += (uint uid, USER_OFFLINE_REASON reason) => {
				string userOfflineMessage = string.Format ("onUserOffline callback uid {0} {1}", uid, reason);
				Debug.Log (userOfflineMessage);
			};

			mRtcEngine.OnVolumeIndication += (AudioVolumeInfo[] speakers, int speakerNumber, int totalVolume) => {
				if (speakerNumber == 0 || speakers == null) {
					Debug.Log (string.Format ("onVolumeIndication only local {0}", totalVolume));
				}

				for (int idx = 0; idx < speakerNumber; idx++) {
					string volumeIndicationMessage = string.Format ("{0} onVolumeIndication {1} {2}", speakerNumber, speakers [idx].uid, speakers [idx].volume);
					Debug.Log (volumeIndicationMessage);
				}
			};

			mRtcEngine.OnUserMuted += (uint uid, bool muted) => {
				string userMutedMessage = string.Format ("onUserMuted callback uid {0} {1}", uid, muted);
				Debug.Log (userMutedMessage);
			};

			mRtcEngine.OnWarning += (int warn, string msg) => {
				string description = IRtcEngine.GetErrorDescription (warn);
				string warningMessage = string.Format ("onWarning callback {0} {1} {2}", warn, msg, description);
				Debug.Log (warningMessage);
			};

			mRtcEngine.OnError += (int error, string msg) => {
				string description = IRtcEngine.GetErrorDescription (error);
				string errorMessage = string.Format ("onError callback {0} {1} {2}", error, msg, description);
				Debug.Log (errorMessage);
			};

			mRtcEngine.OnRtcStats += (RtcStats stats) => {
				string rtcStatsMessage = string.Format ("onRtcStats callback duration {0}, tx: {1}, rx: {2}, tx kbps: {3}, rx kbps: {4}, tx(a) kbps: {5}, rx(a) kbps: {6} users {7}",
					                         stats.duration, stats.txBytes, stats.rxBytes, stats.txKBitRate, stats.rxKBitRate, stats.txAudioKBitRate, stats.rxAudioKBitRate, stats.users);
				Debug.Log (rtcStatsMessage);

				int lengthOfMixingFile = mRtcEngine.GetAudioMixingDuration ();
				int currentTs = mRtcEngine.GetAudioMixingCurrentPosition ();

				string mixingMessage = string.Format ("Mixing File Meta {0}, {1}", lengthOfMixingFile, currentTs);
				Debug.Log (mixingMessage);
			};

			mRtcEngine.OnAudioRouteChanged += (AUDIO_ROUTE route) => {
				string routeMessage = string.Format ("onAudioRouteChanged {0}", route);
				Debug.Log (routeMessage);
			};

			mRtcEngine.OnRequestChannelKey += () => {
				string requestKeyMessage = string.Format ("OnRequestChannelKey");
				Debug.Log (requestKeyMessage);
			};

			mRtcEngine.OnConnectionInterrupted += () => {
				string interruptedMessage = string.Format ("OnConnectionInterrupted");
				Debug.Log (interruptedMessage);
			};

			mRtcEngine.OnConnectionLost += () => {
				string lostMessage = string.Format ("OnConnectionLost");
				Debug.Log (lostMessage);
			};

			mRtcEngine.SetLogFilter (LOG_FILTER.INFO);

			// mRtcEngine.setLogFile("path_to_file_unity.log");

			mRtcEngine.SetChannelProfile (CHANNEL_PROFILE.GAME_FREE_MODE);
			mRtcEngine.EnableAudio ();

			// mRtcEngine.SetChannelProfile (CHANNEL_PROFILE.GAME_COMMAND_MODE);
			// mRtcEngine.SetClientRole (CLIENT_ROLE.BROADCASTER);
		}
	}

	// Update is called once per frame
	void Update ()
	{
		if (mRtcEngine != null) {
			mRtcEngine.Poll ();
		}
	}

	private static string channelName;

	public void JoinChannel ()
	{
		
		channelName = mChannelNameInputField.text.Trim ();

		Debug.Log (string.Format ("tap joinChannel with channel name {0}", channelName));

		if (string.IsNullOrEmpty (channelName)) {
			return;
		}

		mRtcEngine.JoinChannel (channelName, "extra", 0);
		// mRtcEngine.JoinChannelByKey ("YOUR_CHANNEL_KEY", channelName, "extra", 9527);
	}

	public void LeaveChannel ()
	{
		// int duration = mRtcEngine.GetAudioMixingDuration ();
		// int current_duration = mRtcEngine.GetAudioMixingCurrentPosition ();

		// IAudioEffectManager effect = mRtcEngine.GetAudioEffectManager();
		// effect.StopAllEffects ();

		mRtcEngine.LeaveChannel ();
	}
		
	//IM Start
	public void IMClick ()
	{
		if (mRtcEngine.OnMessageReceived == null) {
			mRtcEngine.OnMessageReceived = onMessageReceive;
			mRtcEngine.OnConnectSuccess = onConnectSuccess;
			mRtcEngine.OnCreateDiscussionSuccess = onCreateDiscussionSuccess;
			mRtcEngine.OnCreateDiscussionError = onCreateDiscussionError;
			mRtcEngine.OnGetDiscussionError = onGetDiscussionError;
			mRtcEngine.OnGetDiscussionSuccess = onGetDiscussionSuccess;
			mRtcEngine.OnQuitDiscussionError = onQuitDiscussionError;
			mRtcEngine.OnQuitDiscussionSuccess = onQuitDiscussionSuccess;
			mRtcEngine.OnAddMemberToDiscussionError = onAddMemberToDiscussionError;
			mRtcEngine.OnAddMemberToDiscussionSuccess = onAddMemberToDiscussionSuccess;
			mRtcEngine.OnRemoveMemberFromDiscussionError = onRemoveMemberFromDiscussionError;
			mRtcEngine.OnRemoveMemberFromDiscussionSuccess = onRemoveMemberFromDiscussionSuccess;
			mRtcEngine.OnSendMessageAttached = onSendMessageAttached;
			mRtcEngine.OnSendMessageSuccess = onSendMessageSuccess;
			mRtcEngine.OnSendMessageError = onSendMessageError;
			mRtcEngine.OnJoinChatroomSuccess = onJoinChatRoomSuccess;
			mRtcEngine.OnJoinChatroomError = onJoinChatRoomError;
			mRtcEngine.OnSendVoiceMessageAttached = onSendVoiceMessageAttached;	
			mRtcEngine.OnSendVoiceMessageSuccess = onSendVoiceMessageSuccess;
			mRtcEngine.OnSendVoiceMessageError = onSendVoiceMessageError;
			mRtcEngine.OnGetHistoryMessageSuccess = OnGetHistoryMessageSuccess;
		}
		loadDiscussion ();
	}

	public void onButtonClicked ()
	{
		// which GameObject?
		if (name.CompareTo ("IM") == 0) {
			IMClick ();
		} else if (name.CompareTo ("Send") == 0) {
			onSendClicked ();
		} else if (name.CompareTo ("SendChannel") == 0) {
			onSendChannelClicked ();
		} else if (name.CompareTo ("SendVoice") == 0) {
			onSendVoice ();
		} else if (name.CompareTo ("StartRecord") == 0) {
			startRecord ();
		} else if (name.CompareTo ("StopRecord") == 0) {
			stopRecord ();
		} else if (name.CompareTo ("Discussion") == 0) {
			//loadDiscussion ();
			Debug.Log ("Discussion-----onClick----wrapper--->loadDiscussion");
		} else if (name.CompareTo ("CreateDiscussion") == 0) {
			createDiscussion ();
		} else if (name.CompareTo ("AddMember") == 0) {
			addMember ();
		} else if (name.CompareTo ("RemoveMember") == 0) {
			removeMember ();
		} else if (name.CompareTo ("QuitDiscussion") == 0) {
			quitDiscussion ();
		} else if (name.CompareTo ("GetDiscussion") == 0) {
			getDiscussion ();
		} else if (name.CompareTo ("SetDiscussionName") == 0) {
			//setDiscussionName ();
		} else if (name.CompareTo ("SetDiscussionInvite") == 0) {
			//setDiscussionInvite ();
		} else if (name.CompareTo ("GetHistoryMessage") == 0) {
			mRtcEngine.AgoraIMGetHistoryMessages (1, "yang", messageId, 4);
		} else if (name.CompareTo ("SendDiscussion") == 0) {
			onSendDiscussionClicked ();
		} else if (name.CompareTo ("Return") == 0) {
			onReturn ();
		} else if (name.CompareTo ("PlayAudio") == 0) {
			playAudio ();		
		} else if (name.CompareTo ("GetHistory") == 0) {
//			getHistoryMessage (agora_gaming_rtc.ConverSationType.CHARTROOM, chatRoomId, 5, 5);
		}
	}


	private void onSendDiscussionClicked ()
	{
		GameObject gameObject = GameObject.Find ("MessageText");
		InputField input = gameObject.GetComponent<InputField> ();
		string s1 = input.text;
		logD ("AgoraRongConnect--->C#----wrapper---> " + "----message----->" + s1);
		mRtcEngine.AgoraIMSendMessage (discussionId, s1, ConversationType.DISCUSSION, userInfo);
	}

	private void onSendClicked ()
	{
		
		GameObject gameObject = GameObject.Find ("MessageText");
		InputField input = gameObject.GetComponent<InputField> ();
		string s1 = input.text;
		GameObject gameObject1 = GameObject.Find ("SenderId");
		InputField input2 = gameObject1.GetComponent<InputField> ();
		string s2 = input2.text;
		//logD ("AgoraRongConnect--->C#----targetID---> " + s2 + "----message----->" + s1);
		mRtcEngine.AgoraIMSendMessage (s2, s1, agora_gaming_rtc.ConversationType.PRIVATE, userInfo);

	}

	private void onSendChannelClicked ()
	{
		GameObject gameObject = GameObject.Find ("MessageText");
		InputField input = gameObject.GetComponent<InputField> ();
		string channelMessage = input.text;
		if (channelMessage != null) {
			mRtcEngine.AgoraIMSendMessage (channelName, channelMessage, ConversationType.CHATROOM, userInfo);
		}
	}

	private static string filePath;

	private void onSendVoice ()
	{
		if (isRecord) {
			filePath = mRtcEngine.AgoraIMGetRecordAudioFilePath ();
			int length = mRtcEngine.AgoraIMGetRecordAudioLength ();
			logD ("onSendVoice--->wrapper ---- path ---> " + filePath + "----length ----->" + length);
			GameObject gameObject1 = GameObject.Find ("SenderId");
			InputField input2 = gameObject1.GetComponent<InputField> ();
			mRtcEngine.AgoraIMSendVoiceMessage (input2.text, agora_gaming_rtc.ConversationType.PRIVATE, filePath, length, userInfo);
			isRecord = false;
		}
	}

	private static bool isRecord = false;

	private void startRecord ()
	{	
		isRecord = true;
		mRtcEngine.AgoraIMStartRecordAudio ();
	}

	private void stopRecord ()
	{
		if (isRecord) {
			mRtcEngine.AgoraIMStopRecordAudio ();
		}
	}

	public void playAudio ()
	{
		logD ("unity ----->wrapper---playAudio----filePath -> " + filePath);
		if (filePath != null) {
			mRtcEngine.AgoraIMStartPlayAudio (filePath);
		}
	}

	public void createDiscussion ()
	{
		mRtcEngine.AgoraIMCreateDiscussion ("hehe", "12345,zhang");
	}

	public void addMember ()
	{
		mRtcEngine.AgoraIMAddMemberToDiscussion (discussionId, "12345,zhang,4243543");
	}

	public void removeMember ()
	{
		mRtcEngine.AgoraIMRemoveMemberFromDiscussion (discussionId, "123");
	}

	public void quitDiscussion ()
	{
		mRtcEngine.AgoraIMQuitDiscussion (discussionId);
	}

	public void getDiscussion ()
	{
		mRtcEngine.AgoraIMGetDiscussion (discussionId);
	}

	public void loadDiscussion ()
	{
		GameObject gameObject = GameObject.Find ("mChannelNameInputField");
		InputField input = gameObject.GetComponent<InputField> ();
		channelName = input.text;
		if (string.IsNullOrEmpty (channelName)) {
			logD ("channelName is empty");
			input.placeholder.GetComponent<Text> ().text = "Please input your chatRoomID";

		} else {
			logD ("channelName is not null");

//			zhang
			mRtcEngine.AgoraIMInit (appKey);
			mRtcEngine.AgoraIMConnect ("qyKknH03CtaTmAVpp5FhCMz7psBGhT4XrcX5ZITBY4pOJ/9FDpbJ6a9gymzDN1i3deDs7BFxUHYhKacTGi+Qhw==");	
			mRtcEngine.LeaveChannel ();


			//12345
			//mRtcEngine.AgoraIMInit("82hegw5u8y3dx");
			//mRtcEngine.AgoraIMConnect("YXf2Dp3I3+iM7KFYIcp6i8z7psBGhT4XrcX5ZITBY4pOJ/9FDpbJ6WTAec4LrsATB1mr7uno1Kk1ssc7DJOQGQ==");	
			//mRtcEngine.AgoraIMConnect("XKSg2GY5Jth5eJuHBEGbZNRl9bQ/jDob3/tLwFxUFoeSyZwLtsrMPoDSGo531Exqh8vB3WPzu1pHTCKB3wnLMW80nPpqk+OW");
			//mRtcEngine.LeaveChannel();
//			// enable log
			mRtcEngine.AgoraIMInitMessageReceiveListener ();
			userInfo = new UserInfo ();
			userInfo.userId = "zhang";
			userInfo.name = "zhang0000000";
			userInfo.portraitUri = "https://www.alibaba.com/";

			SceneManager.LoadScene ("IMScene", LoadSceneMode.Single);

		}
	}

	private void onReturn ()
	{
		SceneManager.LoadScene ("HelloUnity3D", LoadSceneMode.Single);
	}

	private void onMessageReceive (Message message)
	{

		logD ("onMessageReceived  ");
		string objecName = message.objectName;
		if (objecName.CompareTo ("RC:TxtMsg") == 0) {
			TextMessage mes = (TextMessage)message;
			messageId	= mes.messageId;
			Debug.Log ("Unity  messageReceive    content  =  " + mes.content + " senderUserId = " + mes.senderUserId + "");
			showMessageText (mes.content);
		} else if (objecName.CompareTo ("RC:VcMsg") == 0) {
			showMessageText (((VoiceMessage)message).objectName + " " + ((VoiceMessage)message).uri + " " + ((VoiceMessage)message).duration);
			filePath = ((VoiceMessage)message).uri;
			//messageId	= mes.messageId;
			//logD ("voiceMesssageReceive    uri  =  " + ((VoiceMessage)message).uri);
		}
	}

	private void showMessageText (string message)
	{
		logD ("showMessageText  =  " + message);
		GameObject messageReceived = GameObject.Find ("MessageText");
		logD ("showMessageText   messageReceived  ");
		InputField input = messageReceived.GetComponent<InputField> ();
		logD ("showMessageText   input  ");
		input.text = message;
		logD ("showMessageText   input.text  ");
	}

	private static string discussionId;

	private void onCreateDiscussionSuccess (string s)
	{
		logD ("unity-->onCreateDiscussionSuccess--->wrapper-->" + s);
		discussionId = s;
		showMessageText ("onCreateDiscussionSuccess  " + s);

	}

	private void showDiscussionMessage (string message)
	{
		GameObject game = GameObject.Find ("MessageText");
		InputField texts = game.GetComponent<InputField> ();
		texts.text = message;
	}

	private void onCreateDiscussionError (string s)
	{
		showMessageText ("onCreateDiscussionError  " + s);
	}

	private void onAddMemberToDiscussionSuccess ()
	{
		showDiscussionMessage ("onAddMemberToDiscussionSuccess  ");
	}

	private void onAddMemberToDiscussionError (string s)
	{
		showDiscussionMessage ("onAddMemberToDiscussionError  " + s);
	}

	private void onRemoveMemberFromDiscussionSuccess ()
	{
		showDiscussionMessage ("onRemoveMemberFromDiscussionSuccess");
	}

	private void onRemoveMemberFromDiscussionError (string s)
	{
		showDiscussionMessage ("onRemoveMemberFromDiscussionError  " + s);
	}

	private void onQuitDiscussionSuccess ()
	{
		showDiscussionMessage ("onQuitDiscussionSuccess");
	}

	private void onQuitDiscussionError (string s)
	{
		showDiscussionMessage ("onQuitDiscussionError  " + s);
	}

	private void onGetDiscussionError (string s)
	{
		showDiscussionMessage ("onGetDiscussionError  " + s);
	}

	private void onGetDiscussionSuccess (Discussion s)
	{
		showDiscussionMessage ("onGetDiscussionSuccess  " + s.creatorId + " " + s.discussionName + " " + (s.memberList) [0]);
	}

	private void onConnectSuccess (string s)
	{
		//logD ("onConnectSuccess");
		mRtcEngine.AgoraIMJoinChatroom (channelName, 0);

	}

	private void logD (String message)
	{
		Debug.Log ("HelloWorld" + message);
	}

	private void onSendVoiceMessageError (string errCode)
	{
		logD ("onSendVoiceMessageError  ---> " + errCode);
		showMessageText ("onSendVoiceMessageError  ---> " + errCode);
	}

	private void onSendVoiceMessageSuccess (Message message)
	{
		showMessageText ("onSendVoiceMessageSuccess " + ((VoiceMessage)message).duration + "    " + ((VoiceMessage)message).uri);
	}

	private void onSendVoiceMessageAttached (Message message)
	{
		showMessageText ("onSendVoiceMessageAttached " + ((VoiceMessage)message).duration + "    " + ((VoiceMessage)message).uri);
		filePath = ((VoiceMessage)message).uri;
	}

	private void onJoinChatRoomError (string errCode)
	{
		showMessageText ("onJoinChatRoomError " + errCode);
	}

	private void onSendMessageAttached (Message message)
	{
		string objecName = message.objectName;
		if (objecName.CompareTo ("RC:TxtMsg") == 0) {
			showMessageText ("onSendMessageAttached " + ((TextMessage)message).objectName + "  " + ((TextMessage)message).content);
		} else if (objecName.CompareTo ("RC:VcMsg") == 0) {
			showMessageText ("onSendMessageAttached" + ((VoiceMessage)message).objectName + "  " + ((VoiceMessage)message).uri);
		}
	}

	private void onJoinChatRoomSuccess ()
	{
		showMessageText ("OnJoinChatRoomSuccess");
	}

	private void onSendMessageSuccess (Message message)
	{
		string objecName = message.objectName;

		if (objecName.CompareTo ("RC:TxtMsg") == 0) {
			showMessageText ("onSendMessageSuccess  " + ((TextMessage)message).objectName + "  " + ((TextMessage)message).content);
		} else if (objecName.CompareTo ("RC:VcMsg") == 0) {
			showMessageText ("onSendMessageSuccess " + ((VoiceMessage)message).objectName + "  " + ((VoiceMessage)message).uri);
		}
	}

	private void OnGetHistoryMessageSuccess (Message[] message)
	{
		showMessageText ("OnGetHistoryMessageSuccess" + ((TextMessage)message [1]).conversationType + "  content  " + ((TextMessage)message [1]).content + "  " + ((TextMessage)message [2]).conversationType + "  content  " + ((TextMessage)message [2]).content);
	}

	private void onSendMessageError (string errCode)
	{
		showMessageText ("onSendMessageError " + errCode);
	}
}
