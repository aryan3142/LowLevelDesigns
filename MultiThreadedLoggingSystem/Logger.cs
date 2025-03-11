using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class Logger : IDisposable
{
	private readonly ConcurrentQueue<string> _logQueue = new();
	private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
	private readonly string _logFilePath;
    private bool _isRunning = true;

    public Logger(string logFilePath = "app.log")
	{
		_logFilePath = logFilePath;
	}

	public void Log(string message)
	{
		_logQueue.Enqueue(message);
		_ = Task.Run(ProcessLog);
	}

	public async void ProcessLog()
	{
		if (!_semaphoreSlim.Wait(0)) return;

		try
		{
			using StreamWriter writer = new StreamWriter(_logFilePath, true);
			while(_logQueue.TryDequeue(out string message) && _isRunning)
			{
				await writer.WriteLine(message);
				await writer.FlushAsync();
			}
		}
		finally
		{
			_semaphoreSlim.Release();
		}
	}

    public void Dispose()
    {
		_isRunning = false;
        _semaphoreSlim.Wait();
        _semaphoreSlim.Dispose();
    }
}

public enum LogLevel
{
	Info,
	Warning,
	Error
}

class ProgramLogger
{
	static void Main()
	{
		using var logger = new Logger();

		// Parallel.For gaurantees that each iteration runs in a separate thread, every thread may not be a new thread created because it reuses threads that are available in the threadpool.
		Parallel.For(0,10, i =>{
			string message = $"Message {i} from thead {Thread.CurrentThread.ManagedThreadId} - Logging done!";
			logger.Log(message);
		});
	}
}
