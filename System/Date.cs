namespace Cangjie.TypeSharp.System;

public class Date
{
	private static readonly global::System.DateTimeOffset UnixEpoch = global::System.DateTimeOffset.UnixEpoch;
	private global::System.DateTimeOffset? _value;

	public Date()
	{
		_value = global::System.DateTimeOffset.Now;
	}

	public Date(double milliseconds)
	{
		_value = FromUnixMilliseconds(milliseconds);
	}

	public Date(string value)
	{
		_value = ParseToDateTimeOffset(value);
	}

	public Date(int year, int month, int date = 1, int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0)
	{
		_value = FromLocalComponents(year, month, date, hours, minutes, seconds, milliseconds);
	}

	public static double now()
	{
		return ToUnixMilliseconds(global::System.DateTimeOffset.Now);
	}

	public static double parse(string value)
	{
		var parsed = ParseToDateTimeOffset(value);
		return parsed.HasValue ? ToUnixMilliseconds(parsed.Value) : double.NaN;
	}

	public static double UTC(int year, int month, int date = 1, int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0)
	{
		var y = NormalizeYear(year);
		try
		{
			var dt = new global::System.DateTimeOffset(y, 1, 1, 0, 0, 0, global::System.TimeSpan.Zero)
				.AddMonths(month)
				.AddDays(date - 1)
				.AddHours(hours)
				.AddMinutes(minutes)
				.AddSeconds(seconds)
				.AddMilliseconds(milliseconds);
			return ToUnixMilliseconds(dt);
		}
		catch
		{
			return double.NaN;
		}
	}

	public double getDate() => HasValue ? Local.DateTime.Day : double.NaN;
	public double getDay() => HasValue ? (int)Local.DateTime.DayOfWeek : double.NaN;
	public double getFullYear() => HasValue ? Local.DateTime.Year : double.NaN;
	public double getHours() => HasValue ? Local.DateTime.Hour : double.NaN;
	public double getMilliseconds() => HasValue ? Local.DateTime.Millisecond : double.NaN;
	public double getMinutes() => HasValue ? Local.DateTime.Minute : double.NaN;
	public double getMonth() => HasValue ? Local.DateTime.Month - 1 : double.NaN;
	public double getSeconds() => HasValue ? Local.DateTime.Second : double.NaN;
	public double getTime() => HasValue ? ToUnixMilliseconds(_value!.Value) : double.NaN;
	public double getTimezoneOffset() => HasValue ? -Local.Offset.TotalMinutes : double.NaN;

	public double getUTCDate() => HasValue ? Utc.Day : double.NaN;
	public double getUTCDay() => HasValue ? (int)Utc.DayOfWeek : double.NaN;
	public double getUTCFullYear() => HasValue ? Utc.Year : double.NaN;
	public double getUTCHours() => HasValue ? Utc.Hour : double.NaN;
	public double getUTCMilliseconds() => HasValue ? Utc.Millisecond : double.NaN;
	public double getUTCMinutes() => HasValue ? Utc.Minute : double.NaN;
	public double getUTCMonth() => HasValue ? Utc.Month - 1 : double.NaN;
	public double getUTCSeconds() => HasValue ? Utc.Second : double.NaN;

	public double setTime(double milliseconds)
	{
		_value = FromUnixMilliseconds(milliseconds);
		return getTime();
	}

	public double setMilliseconds(int ms) => SetLocal(dt => dt.AddMilliseconds(ms - dt.Millisecond));
	public double setSeconds(int sec, int? ms = null) => SetLocal(dt =>
	{
		var v = new global::System.DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, 0, dt.Kind)
			.AddSeconds(sec)
			.AddMilliseconds(ms ?? dt.Millisecond);
		return v;
	});

	public double setMinutes(int min, int? sec = null, int? ms = null) => SetLocal(dt =>
	{
		var v = new global::System.DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, 0, dt.Kind)
			.AddMinutes(min)
			.AddSeconds(sec ?? dt.Second)
			.AddMilliseconds(ms ?? dt.Millisecond);
		return v;
	});

	public double setHours(int hour, int? min = null, int? sec = null, int? ms = null) => SetLocal(dt =>
	{
		var v = new global::System.DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, dt.Kind)
			.AddHours(hour)
			.AddMinutes(min ?? dt.Minute)
			.AddSeconds(sec ?? dt.Second)
			.AddMilliseconds(ms ?? dt.Millisecond);
		return v;
	});

	public double setDate(int day) => SetLocal(dt => new global::System.DateTime(dt.Year, dt.Month, 1, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, dt.Kind).AddDays(day - 1));

	public double setMonth(int month, int? day = null) => SetLocal(dt =>
	{
		var v = new global::System.DateTime(dt.Year, 1, 1, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, dt.Kind)
			.AddMonths(month)
			.AddDays((day ?? dt.Day) - 1);
		return v;
	});

	public double setFullYear(int year, int? month = null, int? day = null) => SetLocal(dt =>
	{
		var y = NormalizeYear(year);
		var v = new global::System.DateTime(y, 1, 1, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, dt.Kind)
			.AddMonths(month ?? (dt.Month - 1))
			.AddDays((day ?? dt.Day) - 1);
		return v;
	});

	public double setUTCMilliseconds(int ms) => SetUtc(dt => dt.AddMilliseconds(ms - dt.Millisecond));
	public double setUTCSeconds(int sec, int? ms = null) => SetUtc(dt =>
	{
		var v = new global::System.DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, 0, dt.Kind)
			.AddSeconds(sec)
			.AddMilliseconds(ms ?? dt.Millisecond);
		return v;
	});

	public double setUTCMinutes(int min, int? sec = null, int? ms = null) => SetUtc(dt =>
	{
		var v = new global::System.DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, 0, dt.Kind)
			.AddMinutes(min)
			.AddSeconds(sec ?? dt.Second)
			.AddMilliseconds(ms ?? dt.Millisecond);
		return v;
	});

	public double setUTCHours(int hour, int? min = null, int? sec = null, int? ms = null) => SetUtc(dt =>
	{
		var v = new global::System.DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, 0, dt.Kind)
			.AddHours(hour)
			.AddMinutes(min ?? dt.Minute)
			.AddSeconds(sec ?? dt.Second)
			.AddMilliseconds(ms ?? dt.Millisecond);
		return v;
	});

	public double setUTCDate(int day) => SetUtc(dt => new global::System.DateTime(dt.Year, dt.Month, 1, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, dt.Kind).AddDays(day - 1));

	public double setUTCMonth(int month, int? day = null) => SetUtc(dt =>
	{
		var v = new global::System.DateTime(dt.Year, 1, 1, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, dt.Kind)
			.AddMonths(month)
			.AddDays((day ?? dt.Day) - 1);
		return v;
	});

	public double setUTCFullYear(int year, int? month = null, int? day = null) => SetUtc(dt =>
	{
		var y = NormalizeYear(year);
		var v = new global::System.DateTime(y, 1, 1, dt.Hour, dt.Minute, dt.Second, dt.Millisecond, dt.Kind)
			.AddMonths(month ?? (dt.Month - 1))
			.AddDays((day ?? dt.Day) - 1);
		return v;
	});

	public string toISOString() => HasValue ? Utc.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", global::System.Globalization.CultureInfo.InvariantCulture) : "Invalid Date";
	public string toUTCString() => HasValue ? Utc.ToString("r", global::System.Globalization.CultureInfo.InvariantCulture) : "Invalid Date";
	public string toDateString() => HasValue ? Local.DateTime.ToString("ddd MMM dd yyyy", global::System.Globalization.CultureInfo.InvariantCulture) : "Invalid Date";
	public string toTimeString() => HasValue ? Local.DateTime.ToString("HH:mm:ss 'GMT'zzz", global::System.Globalization.CultureInfo.InvariantCulture) : "Invalid Date";
	public string toJSON() => HasValue ? toISOString() : "null";
	public double valueOf() => getTime();
	public override string ToString() => HasValue ? Local.DateTime.ToString("ddd MMM dd yyyy HH:mm:ss 'GMT'zzz", global::System.Globalization.CultureInfo.InvariantCulture) : "Invalid Date";

	private bool HasValue => _value.HasValue;
	private global::System.DateTimeOffset Local => _value!.Value.ToLocalTime();
	private global::System.DateTime Utc => _value!.Value.UtcDateTime;

	private static int NormalizeYear(int year) => year is >= 0 and <= 99 ? year + 1900 : year;

	private static double ToUnixMilliseconds(global::System.DateTimeOffset value) => (value - UnixEpoch).TotalMilliseconds;

	private static global::System.DateTimeOffset? FromUnixMilliseconds(double milliseconds)
	{
		if (double.IsNaN(milliseconds) || double.IsInfinity(milliseconds))
		{
			return null;
		}

		try
		{
			return UnixEpoch.AddMilliseconds(milliseconds);
		}
		catch
		{
			return null;
		}
	}

	private static global::System.DateTimeOffset? ParseToDateTimeOffset(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}

		if (global::System.DateTimeOffset.TryParse(value, global::System.Globalization.CultureInfo.InvariantCulture, global::System.Globalization.DateTimeStyles.RoundtripKind, out var dto) ||
			global::System.DateTimeOffset.TryParse(value, out dto))
		{
			return dto;
		}

		return null;
	}

	private static global::System.DateTimeOffset? FromLocalComponents(int year, int month, int day, int hours, int minutes, int seconds, int milliseconds)
	{
		var y = NormalizeYear(year);
		try
		{
			var dt = new global::System.DateTime(y, 1, 1, 0, 0, 0, 0, global::System.DateTimeKind.Local)
				.AddMonths(month)
				.AddDays(day - 1)
				.AddHours(hours)
				.AddMinutes(minutes)
				.AddSeconds(seconds)
				.AddMilliseconds(milliseconds);
			return new global::System.DateTimeOffset(dt);
		}
		catch
		{
			return null;
		}
	}

	private double SetLocal(global::System.Func<global::System.DateTime, global::System.DateTime> transform)
	{
		if (!HasValue) return double.NaN;
		try
		{
			var local = Local.DateTime;
			var next = transform(local);
			_value = new global::System.DateTimeOffset(next);
			return getTime();
		}
		catch
		{
			_value = null;
			return double.NaN;
		}
	}

	private double SetUtc(global::System.Func<global::System.DateTime, global::System.DateTime> transform)
	{
		if (!HasValue) return double.NaN;
		try
		{
			var utc = Utc;
			var next = transform(global::System.DateTime.SpecifyKind(utc, global::System.DateTimeKind.Utc));
			_value = new global::System.DateTimeOffset(global::System.DateTime.SpecifyKind(next, global::System.DateTimeKind.Utc));
			return getTime();
		}
		catch
		{
			_value = null;
			return double.NaN;
		}
	}
}