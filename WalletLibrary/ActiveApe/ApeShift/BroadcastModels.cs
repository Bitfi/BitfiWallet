using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WalletLibrary.ActiveApe.ApeShift
{
	public enum wallet_methods
	{
		transfer = 1,
		sign_message = 2,
		get_addresses = 3,
		get_pub_keys = 4,
		get_device_info = 5,
		get_device_envoy = 6,
		get_legacy_profile = 7,
		vibe = 8,
		connect_wallet = 9,
		status = 10
	}


	public class DeviceEvent<T>
	{
		public envoy_event event_type { get; set; }
		public T event_info { get; set; }
	}

	public enum envoy_event
	{
		battery_changed = 1,
		user_availability = 2,
		session_status = 3
	}

	public class SessionInfo
	{
		public bool IsDisposed { get; set; }
	}

	public class BatteryInfo
	{
		public bool IsCharging { get; set; }
		public Int32 Level { get; set; }
	}

	public class AvailabilityInfo
	{
		public bool IsUserBusy { get; set; }
		public bool IsUserBlocking { get; set; }
	}


	public delegate void DataMSGCompletedEventHandler(DataMSGCompletedEventArgs e);
	public partial class DataMSGCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
	{
		public DataMSGCompletedEventArgs(object result) : base(null, false, null) { Message = (string)result; }
		public string Message { get; }
	}

	public delegate void BroadcastRequestCompletedEventHandler(BroadcastRequestCompletedEventArgs e);
	public partial class BroadcastRequestCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
	{
		public BroadcastRequestCompletedEventArgs(WalletRequest<object> result, MQDiffieResult sender, wallet_methods method)
			: base(null, false, null)
		{ Request = result; Sender = sender; Method = method; }

		public WalletRequest<object> Request { get; }
		public MQDiffieResult Sender { get; }
		public wallet_methods Method { get; }

	}

	public delegate void BroadcastResponseCompletedEventHandler(BroadcastResponseCompletedEventArgs e);
	public partial class BroadcastResponseCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
	{
		public BroadcastResponseCompletedEventArgs(object result, MQDiffieResult sender, bool error) : base(null, false, null)
		{ Response = result; Sender = sender; IsError = error; }
		public object Response { get; }
		public bool IsError { get; }
		public MQDiffieResult Sender { get; }
	}

	public class WalletRequest<T>
	{
		public string Method { get; set; }
		public string Id { get; set; }
		public T Params { get; set; }
	}


}
