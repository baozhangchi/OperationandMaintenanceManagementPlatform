namespace OAMMP.Common;

public interface IMonitorClientHandler
{
}

public class QueryLogArgs
{
	public QueryLogArgs()
	{
	}

	public QueryLogArgs(int count)
	{
		Count = count;
	}

	public QueryLogArgs(DateTime startTime)
	{
		StartTime = startTime;
	}

	public QueryLogArgs(DateTime startTime, DateTime endTime)
	{
		StartTime = startTime;
		EndTime = endTime;
	}

	public DateTime? StartTime { get; set; }
	public DateTime? EndTime { get; set; }
	public int? Count { get; set; }
}