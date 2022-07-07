using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WalletLibrary.ActiveApe.ApeShift
{
	//mq wrapper


	public enum api_methods
	{
		get_envoy_token = 1, invoke_envoy = 2, subscribe = 3,
		invoke_application = 4, create_blockchain_mq = 5, create_application = 6, get_address_count = 7
	}
	public class GetEnvoyTokenRequest
	{
		public uint reusable_minutes { get; set; }
		public EnvoyTask envoy_task { get; set; }
	}

	public class GetEnvoyTokenResponse
	{
		public string envoy_token { get; set; }
		public string task_id { get; set; }
	}

	public class EnvoyTask
	{
		public bool single_instance { get; set; }
		public bool send_offline_message { get; set; }
		public string task_context { get; set; }
		public uint timeout { get; set; }
		public bool persist_timeout { get; set; }
	}

	public class InvokeEnvoyRequest
	{
		public string task_id { get; set; }
		public string message { get; set; }
		public uint new_timeout { get; set; }
	}
	public class InvokeEnvoyResponse
	{
		public string status { get; set; }
	}



	public class ProviderInfoResponse
	{
		public string connection_sring { get; set; }
		public string public_key { get; set; }
		public string error_message { get; set; }
		public List<ChannelInfoItem> infoItems { get; set; }
	}

	public class ChannelInfoItem
	{
		public string Title { get; set; }
		public string Info { get; set; }
	}

	//blockchainsmq


	public class CreateBlockchainMQRequest
	{
		public string queue_name { get; set; }
	}
	public class CreateBlockchainMQResponse
	{
		public string connection_string { get; set; }
	}

	public class SubscibeRequest
	{
		public string network { get; set; }
		public string address { get; set; }
		public string queue_name { get; set; }
		public string event_type { get; set; }
		public string include_data { get; set; }
	}

	public class SubscribeResponse
	{
		public string status { get; set; }
	}


	//async360 channels

	public class CreateApplicationRequest
	{
		public string public_key { get; set; }

		public string channel_name { get; set; }
	}
	public class CreateApplicationResponse
	{
		public string connection_string { get; set; }
	}

	public class InvokeApplicatonRequest
	{
		public string protected_message { get; set; }
		public string to_public_key { get; set; }
		public string from_public_key { get; set; }
		public string request_context { get; set; }
	}

	public class InvokeApplicatonResponse
	{
		public string status { get; set; }
	}



	//other

	public class RegisterRequest
	{
		public string email { get; set; }
	}

	public class RegisterResponse
	{
		public string api_secret { get; set; }
		public string public_id { get; set; }
	}


	public class AddressCountRequest
	{
		public string network { get; set; }
		public string queue_name { get; set; }
	}
	public class AddressCountResponse
	{
		public int address_count { get; set; }
	}

	//mq wrapper


	public class MQResponse<T>
	{
		[JsonProperty("dqcommand")]
		public string DequeueCommand { get; set; }

		[JsonProperty("result")]
		public T Result { get; set; }

	}
	public class MQResponse
	{
		public string ClientID { get; set; }
		public string MessageID { get; set; }
		public string PopReceipt { get; set; }

	}
	public class MQNetworkResult
	{
		public string TxnEvent { get; set; }
		public string Network { get; set; }
		public int Height { get; set; }
		public string TxnId { get; set; }
		public string Address { get; set; }
		public string Value { get; set; }
		public string UserData { get; set; }
	}

	public class MQDiffieResult
	{
		public string FromPublicKey { get; set; }
		public string ProtectedMessage { get; set; }
		public string MQContext { get; set; }
		public string ECStatus { get; set; }
		public string ECDisplayCode { get; set; }
		public string ECPublicKey { get; set; }
	}




	//api wrapper

	public class ApiRequest<T>
	{
		public string Method { get; set; }
		public string Id { get; set; }
		public T Params { get; set; }
	}

	public class JError
	{
		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("code")]
		public int Code { get; set; }
	}

	public class JResponseWrapper<T>
	{
		//[JsonProperty("id")]
		//public string Id { get; set; }

		//[JsonProperty("jsonrpc")]
		//public string Jsonrpc { get; set; }

		[JsonProperty("error")]
		public JError Error { get; set; }

		[JsonProperty("result")]
		public T Result { get; set; }
	}

	//client ws

	public class UserInfo
	{
		public string ClientToken { get; set; }
	}
	class WSResponse
	{
		public string Error { get; set; }
		public string Ticks { get; set; }
		public string Message { get; set; }
		public bool Completed { get; set; }
	}


	//api




}