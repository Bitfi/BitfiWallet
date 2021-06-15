using System;

namespace NBitcoin.Logging
{
	public class Logs
	{
		static Logs()
		{
			
		}
		

		public const int ColumnLength = 16;
	}

	public class FuncLoggerFactory
	{
		

		public void Dispose()
		{

		}
	}


	/// <summary>
	/// Minimalistic logger that does nothing.
	/// </summary>
	public class NullLogger
	{
		public static NullLogger Instance { get; } = new NullLogger();

		private NullLogger()
		{
		}

		/// <inheritdoc />
		public IDisposable BeginScope<TState>(TState state)
		{
			return NullScope.Instance;
		}

		/// <inheritdoc />
		public bool IsEnabled()
		{
			return false;
		}

		/// <inheritdoc />
		public void Log<TState>()
		{
		}
	}

	/// <summary>
	/// An empty scope without any logic
	/// </summary>
	public class NullScope : IDisposable
	{
		public static NullScope Instance { get; } = new NullScope();

		private NullScope()
		{
		}

		/// <inheritdoc />
		public void Dispose()
		{
		}
	}

}
