using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WalletLibrary.NoxShared
{
	public enum ManagerRequestType
	{
		Authorization = 0,
		Validation = 1,
		Registration = 2,
		Info = 3,
		Subscription = 4,
		MessageStatus,
	}
	public class ManagerResponse
	{
		public bool success { get; set; }
		public string error_message { get; set; }
		public ValidationResponse validationResponse { get; set; }
		public RegistrationResponse registrationResponse { get; set; }
		public SubsriptionResponse subsriptionResponse { get; set; }
		public SubscriptionInfo subscriptionInfo { get; set; }
		public MessageStatusResponse messageStatusResponse { get; set; }
		public InfoResponse infoResponse { get; set; }
		public bool subscriptionError { get; set; }
	}

	public class SubscriptionMethods
	{
		public string[] methods { get; set; }
	}

	public class SubscriptionInfo
	{
		public SubscriptionMethods subscriptionMethods { get; set; }
		public string message_prompt { get; set; }
		public string title_prompt { get; set; }
		public string message_notice { get; set; }
		public string title_notice { get; set; }
		public string btn_a_notice { get; set; }
		public string btn_b_notice { get; set; }
		public string mthds_title { get; set; }
	}
	public class InfoResponse
	{
		public string Title { get; set; }
		public string Article { get; set; }
	}
	public class ManagerRequest
	{
		public ManagerAuthorization authorization { get; set; }
		public ValidationRequest validationRequest { get; set; }
		public RegistrationRequest registrationRequest { get; set; }
		public SubsriptionRequest subsriptionRequest { get; set; }
		public ManagerRequestType requestType { get; set; }
		public MessageStatusRequest messageStatusRequest { get; set; }

	}


	public class ManagerAuthorization
	{
		public string DevicePubHash { get; set; }
		public string Signature { get; set; }
		public string Message { get; set; }
		public string DeviceID { get; set; }
		public string SGAMSGTOKEN { get; set; }
		public string HdrType { get; set; }

	}


	public class SubsriptionRequest
	{
		public string blkNet { get; set; }
		public int option { get; set; }
	}

	public class SubsriptionResponse
	{
		public string smsToken { get; set; }
		public string subscription_message { get; set; }
	}
	public class ValidationRequest
	{
		public string pubKey { get; set; }
	}
	public class MessageStatusRequest
	{
		public string user_pubKey { get; set; }
	}
	public class MessageStatusResponse
	{
		public string[] peer_name { get; set; }
	}
	public class RegistrationRequest
	{
		public string pubKey { get; set; }
		public string registerName { get; set; }
	}

	public class ValidationResponse
	{
		public bool IsRegistered { get; set; }
		public string validation_message { get; set; }
		public List<string> ape_tokens { get; set; }

	}

	public class RegistrationResponse
	{
		public bool IsRegistered { get; set; }
		public string registration_message { get; set; }
	}


	//socket

	public class SharedRequest
	{
		public SharedRequestType sharedRequestType { get; set; }
		public string encryptedMessage { get; set; }
		public SharedAuthRequest authRequest { get; set; }
		public string MessageID { get; set; }
	}
	public class SharedAuthRequest
	{
		public string UserSession { get; set; }
		public string PeerSession { get; set; }
		public ManagerAuthorization authorization { get; set; }
	}
	public class SharedResponse
	{
		public SharedResponseType responseType { get; set; }
		public string message { get; set; }
		public bool offLine { get; set; }
		public DateTime dateTime { get; set; }
		public string MessageID { get; set; }

		public bool BottomStack { get; set; }
	}

	public enum SharedResponseType
	{
		Service = 0,
		Peer = 1,
		User = 2,
		Receipt
	}

	public enum SharedRequestType
	{
		Auth = 0,
		Message = 1,
		Receipt = 2,
		More = 3
	}

	public class MessageResponse
	{
		public SharedResponseType responseType { get; set; }
		public string serviceMessage { get; set; }
		public NoxParagraph peerMessage { get; set; }
		public bool offLine { get; set; }
		public DateTime dateTime { get; set; }
		public Guid MessageID { get; set; }
		public bool isHistory { get; set; }

		public bool BottomStack { get; set; }
	}



}